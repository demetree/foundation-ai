using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Security;
using Foundation.Security.Database;
using Foundation.BMC.Database;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Custom controller for "My Part Lists" — managing named part lists
    /// with denormalized part and colour information.
    ///
    /// Endpoints:
    ///   GET    /api/my-part-lists              — All part lists with summary stats
    ///   GET    /api/my-part-lists/{id}         — Single list with denormalized items
    ///   POST   /api/my-part-lists              — Create a new part list
    ///   PUT    /api/my-part-lists/{id}         — Update list properties
    ///   DELETE /api/my-part-lists/{id}         — Soft-delete list
    ///   POST   /api/my-part-lists/{id}/items   — Add a part to the list
    ///   PUT    /api/my-part-lists/{id}/items/{itemId}  — Update item quantity
    ///   DELETE /api/my-part-lists/{id}/items/{itemId}  — Remove item from list
    ///
    /// All endpoints are tenant-scoped. Write operations check "BMC Collection Writer" role.
    /// </summary>
    public class MyPartListsController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;

        private readonly BMCContext _context;
        private readonly ILogger<MyPartListsController> _logger;


        public MyPartListsController(BMCContext context, ILogger<MyPartListsController> logger)
            : base("BMC", "UserPartList")
        {
            _context = context;
            _logger = logger;

            _context.Database.SetCommandTimeout(60);
        }


        #region DTOs

        /// <summary>
        /// Summary DTO for the part lists listing page.
        /// </summary>
        public class PartListSummaryDto
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool isBuildable { get; set; }
            public int? rebrickableListId { get; set; }
            public int itemCount { get; set; }
            public int totalParts { get; set; }
            public int uniqueColours { get; set; }
            public int versionNumber { get; set; }
        }

        /// <summary>
        /// Denormalized part item within a part list.
        /// </summary>
        public class PartListItemDto
        {
            public int id { get; set; }
            public int brickPartId { get; set; }
            public string partName { get; set; }
            public string partNum { get; set; }
            public string partImageUrl { get; set; }
            public string partCategory { get; set; }
            public string rebrickablePartUrl { get; set; }
            public int brickColourId { get; set; }
            public string colourName { get; set; }
            public string colourHex { get; set; }
            public bool isTransparent { get; set; }
            public int quantity { get; set; }
        }

        /// <summary>
        /// Detailed DTO for a single part list with all items.
        /// </summary>
        public class PartListDetailDto
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool isBuildable { get; set; }
            public int? rebrickableListId { get; set; }
            public int versionNumber { get; set; }
            public List<PartListItemDto> items { get; set; }
        }

        /// <summary>
        /// Request body for creating a new part list.
        /// </summary>
        public class CreatePartListRequest
        {
            public string name { get; set; }
            public bool isBuildable { get; set; } = false;
        }

        /// <summary>
        /// Request body for updating a part list.
        /// </summary>
        public class UpdatePartListRequest
        {
            public string name { get; set; }
            public bool? isBuildable { get; set; }
        }

        /// <summary>
        /// Request body for adding a part to a list.
        /// </summary>
        public class AddPartToListRequest
        {
            public int brickPartId { get; set; }
            public int brickColourId { get; set; }
            public int quantity { get; set; } = 1;
        }

        /// <summary>
        /// Request body for updating a part list item.
        /// </summary>
        public class UpdatePartListItemRequest
        {
            public int? quantity { get; set; }
            public int? brickColourId { get; set; }
        }

        #endregion


        // ─────────────────────────────────────────────────────────────────
        // GET /api/my-part-lists — all part lists with summary stats
        // ─────────────────────────────────────────────────────────────────

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-part-lists")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var lists = await _context.UserPartLists
                .Where(l => l.tenantGuid == tenantGuid && l.active == true && l.deleted == false)
                .OrderBy(l => l.name)
                .Select(l => new PartListSummaryDto
                {
                    id = l.id,
                    name = l.name,
                    isBuildable = l.isBuildable,
                    rebrickableListId = l.rebrickableListId,
                    versionNumber = l.versionNumber,
                    itemCount = l.UserPartListItems
                        .Where(i => i.active == true && i.deleted == false)
                        .Count(),
                    totalParts = l.UserPartListItems
                        .Where(i => i.active == true && i.deleted == false)
                        .Sum(i => i.quantity),
                    uniqueColours = l.UserPartListItems
                        .Where(i => i.active == true && i.deleted == false)
                        .Select(i => i.brickColourId)
                        .Distinct()
                        .Count()
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"Get my part lists — {lists.Count} lists");

            return Ok(lists);
        }


        // ─────────────────────────────────────────────────────────────────
        // GET /api/my-part-lists/{id} — single list with items
        // ─────────────────────────────────────────────────────────────────

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-part-lists/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var list = await _context.UserPartLists
                .AsNoTracking()
                .Where(l => l.id == id && l.tenantGuid == tenantGuid && l.active == true && l.deleted == false)
                .Select(l => new PartListDetailDto
                {
                    id = l.id,
                    name = l.name,
                    isBuildable = l.isBuildable,
                    rebrickableListId = l.rebrickableListId,
                    versionNumber = l.versionNumber,
                    items = l.UserPartListItems
                        .Where(i => i.active == true && i.deleted == false)
                        .Select(i => new PartListItemDto
                        {
                            id = i.id,
                            brickPartId = i.brickPartId,
                            partName = i.brickPart.name,
                            partNum = i.brickPart.rebrickablePartNum,
                            partImageUrl = i.brickPart.rebrickableImgUrl,
                            partCategory = i.brickPart.brickCategory != null ? i.brickPart.brickCategory.name : null,
                            rebrickablePartUrl = i.brickPart.rebrickablePartUrl,
                            brickColourId = i.brickColourId,
                            colourName = i.brickColour.name,
                            colourHex = i.brickColour.hexRgb,
                            isTransparent = i.brickColour.isTransparent,
                            quantity = i.quantity
                        })
                        .OrderBy(i => i.partName)
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (list == null) return NotFound("Part list not found.");

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity,
                $"Get part list detail — {list.name}, {list.items.Count} items", id.ToString());

            return Ok(list);
        }


        // ─────────────────────────────────────────────────────────────────
        // POST /api/my-part-lists — create a new part list
        // ─────────────────────────────────────────────────────────────────

        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-part-lists")]
        public async Task<IActionResult> Create([FromBody] CreatePartListRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (request == null || string.IsNullOrWhiteSpace(request.name))
                return BadRequest("Name is required.");

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var newList = new UserPartList
            {
                name = request.name.Trim(),
                isBuildable = request.isBuildable,
                versionNumber = 1,
                tenantGuid = tenantGuid,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.UserPartLists.Add(newList);
            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                $"Create part list — {newList.name}", newList.id.ToString());

            return Ok(new { id = newList.id, name = newList.name });
        }


        // ─────────────────────────────────────────────────────────────────
        // PUT /api/my-part-lists/{id} — update list properties
        // ─────────────────────────────────────────────────────────────────

        [HttpPut]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-part-lists/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePartListRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (request == null) return BadRequest();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var list = await _context.UserPartLists
                .FirstOrDefaultAsync(l => l.id == id && l.tenantGuid == tenantGuid && l.active == true && l.deleted == false, cancellationToken);

            if (list == null) return NotFound("Part list not found.");

            if (!string.IsNullOrWhiteSpace(request.name)) list.name = request.name.Trim();
            if (request.isBuildable.HasValue) list.isBuildable = request.isBuildable.Value;
            list.versionNumber++;

            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                $"Update part list — {list.name} (v{list.versionNumber})", id.ToString());

            return Ok(new { id = list.id, name = list.name, versionNumber = list.versionNumber });
        }


        // ─────────────────────────────────────────────────────────────────
        // DELETE /api/my-part-lists/{id} — soft-delete list
        // ─────────────────────────────────────────────────────────────────

        [HttpDelete]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-part-lists/{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var list = await _context.UserPartLists
                .FirstOrDefaultAsync(l => l.id == id && l.tenantGuid == tenantGuid && l.active == true && l.deleted == false, cancellationToken);

            if (list == null) return NotFound("Part list not found.");

            // Soft-delete all items too
            var items = await _context.UserPartListItems
                .Where(i => i.userPartListId == id && i.active == true && i.deleted == false)
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.deleted = true;
                item.active = false;
            }

            list.deleted = true;
            list.active = false;
            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
                $"Delete part list — {list.name} ({items.Count} items)", id.ToString());

            return Ok(new { action = "removed" });
        }


        // ─────────────────────────────────────────────────────────────────
        // POST /api/my-part-lists/{id}/items — add a part to the list
        // ─────────────────────────────────────────────────────────────────

        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-part-lists/{id}/items")]
        public async Task<IActionResult> AddItem(int id, [FromBody] AddPartToListRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (request == null) return BadRequest();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            // Verify list exists
            var list = await _context.UserPartLists
                .FirstOrDefaultAsync(l => l.id == id && l.tenantGuid == tenantGuid && l.active == true && l.deleted == false, cancellationToken);
            if (list == null) return NotFound("Part list not found.");

            // Verify part exists
            var partExists = await _context.BrickParts.AnyAsync(p => p.id == request.brickPartId && p.active == true && p.deleted == false, cancellationToken);
            if (!partExists) return NotFound("Brick part not found.");

            // Verify colour exists
            var colourExists = await _context.BrickColours.AnyAsync(c => c.id == request.brickColourId && c.active == true && c.deleted == false, cancellationToken);
            if (!colourExists) return NotFound("Brick colour not found.");

            // Check for existing item (same part + same colour = upsert)
            var existing = await _context.UserPartListItems
                .FirstOrDefaultAsync(i =>
                    i.userPartListId == id
                    && i.brickPartId == request.brickPartId
                    && i.brickColourId == request.brickColourId
                    && i.active == true
                    && i.deleted == false, cancellationToken);

            string action;
            int itemId;

            if (existing != null)
            {
                existing.quantity += Math.Max(request.quantity, 1);
                action = "updated";
                itemId = existing.id;
            }
            else
            {
                var newItem = new UserPartListItem
                {
                    userPartListId = id,
                    brickPartId = request.brickPartId,
                    brickColourId = request.brickColourId,
                    quantity = Math.Max(request.quantity, 1),
                    tenantGuid = tenantGuid,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.UserPartListItems.Add(newItem);
                await _context.SaveChangesAsync(cancellationToken);
                action = "created";
                itemId = newItem.id;
            }

            list.versionNumber++;
            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                $"Add part to list — partId={request.brickPartId}, colourId={request.brickColourId}, qty={request.quantity}", itemId.ToString());

            return Ok(new { action, id = itemId });
        }


        // ─────────────────────────────────────────────────────────────────
        // PUT /api/my-part-lists/{id}/items/{itemId} — update item
        // ─────────────────────────────────────────────────────────────────

        [HttpPut]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-part-lists/{id}/items/{itemId}")]
        public async Task<IActionResult> UpdateItem(int id, int itemId, [FromBody] UpdatePartListItemRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (request == null) return BadRequest();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            // Verify list ownership
            var listExists = await _context.UserPartLists
                .AnyAsync(l => l.id == id && l.tenantGuid == tenantGuid && l.active == true && l.deleted == false, cancellationToken);
            if (!listExists) return NotFound("Part list not found.");

            var item = await _context.UserPartListItems
                .FirstOrDefaultAsync(i =>
                    i.id == itemId
                    && i.userPartListId == id
                    && i.active == true
                    && i.deleted == false, cancellationToken);

            if (item == null) return NotFound("Part list item not found.");

            if (request.quantity.HasValue) item.quantity = Math.Max(request.quantity.Value, 1);
            if (request.brickColourId.HasValue)
            {
                var colourExists = await _context.BrickColours
                    .AnyAsync(c => c.id == request.brickColourId.Value && c.active == true && c.deleted == false, cancellationToken);
                if (!colourExists) return NotFound("Brick colour not found.");
                item.brickColourId = request.brickColourId.Value;
            }

            // Bump list version
            var list = await _context.UserPartLists.FindAsync(new object[] { id }, cancellationToken);
            if (list != null) list.versionNumber++;

            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                $"Update part list item — itemId={itemId}, qty={item.quantity}", itemId.ToString());

            return Ok(new { id = item.id, quantity = item.quantity });
        }


        // ─────────────────────────────────────────────────────────────────
        // DELETE /api/my-part-lists/{id}/items/{itemId} — remove item
        // ─────────────────────────────────────────────────────────────────

        [HttpDelete]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-part-lists/{id}/items/{itemId}")]
        public async Task<IActionResult> RemoveItem(int id, int itemId, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            // Verify list ownership
            var listExists = await _context.UserPartLists
                .AnyAsync(l => l.id == id && l.tenantGuid == tenantGuid && l.active == true && l.deleted == false, cancellationToken);
            if (!listExists) return NotFound("Part list not found.");

            var item = await _context.UserPartListItems
                .FirstOrDefaultAsync(i =>
                    i.id == itemId
                    && i.userPartListId == id
                    && i.active == true
                    && i.deleted == false, cancellationToken);

            if (item == null) return NotFound("Part list item not found.");

            item.deleted = true;
            item.active = false;

            // Bump list version
            var list = await _context.UserPartLists.FindAsync(new object[] { id }, cancellationToken);
            if (list != null) list.versionNumber++;

            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
                $"Remove part from list — itemId={itemId}", itemId.ToString());

            return Ok(new { action = "removed" });
        }


        // ─────────────────────────────────────────────────────────────────
        // Helper: get tenant GUID or fail
        // ─────────────────────────────────────────────────────────────────

        private async Task<Guid> GetTenantGuidOrFail(CancellationToken cancellationToken)
        {
            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            try
            {
                return await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error,
                    $"Non-tenant user attempted to access my-part-lists — {securityUser?.accountName}",
                    securityUser?.accountName, ex);
                return Guid.Empty;
            }
        }
    }
}
