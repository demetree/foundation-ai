using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
using BMC.Rebrickable.Sync;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Custom controller for "My Set Lists" — the 1:1 Rebrickable replacement
    /// for managing named set lists.
    ///
    /// Endpoints:
    ///   GET    /api/user-set-lists              — All set lists with summary stats
    ///   GET    /api/user-set-lists/{id}         — Single set list with items (denormalized set info)
    ///   POST   /api/user-set-lists              — Create a new set list + enqueue sync
    ///   PUT    /api/user-set-lists/{id}         — Update set list name/properties + enqueue sync
    ///   DELETE /api/user-set-lists/{id}         — Soft-delete set list + enqueue sync
    ///   POST   /api/user-set-lists/{id}/items   — Add a set to the list + enqueue sync
    ///   PUT    /api/user-set-lists/{id}/items/{itemId} — Update item quantity/spares + enqueue sync
    ///   DELETE /api/user-set-lists/{id}/items/{itemId} — Remove item + enqueue sync
    ///   GET    /api/user-set-lists/{id}/history  — Version history (Foundation version control)
    ///
    /// All endpoints are tenant-scoped. Write operations check "BMC Collection Writer" role.
    /// Sync enqueueing respects the user's integration mode (RealTime or PushOnly).
    /// </summary>
    public class UserSetListController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
        private const int DEFAULT_MAX_SYNC_ATTEMPTS = 5;

        private readonly BMCContext _context;
        private readonly ILogger<UserSetListController> _logger;


        public UserSetListController(BMCContext context, ILogger<UserSetListController> logger)
            : base("BMC", "UserSetList")
        {
            _context = context;
            _logger = logger;

            _context.Database.SetCommandTimeout(60);
        }


        #region DTOs

        /// <summary>
        /// Summary DTO for the set lists listing page.
        /// </summary>
        public class SetListSummaryDto
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool isBuildable { get; set; }
            public int? rebrickableListId { get; set; }
            public int itemCount { get; set; }
            public int totalSets { get; set; }
            public int totalParts { get; set; }
            public int versionNumber { get; set; }
            public int pendingSyncCount { get; set; }
        }

        /// <summary>
        /// Denormalized set item within a set list.
        /// </summary>
        public class SetListItemDto
        {
            public int id { get; set; }
            public int legoSetId { get; set; }
            public string setName { get; set; }
            public string setNumber { get; set; }
            public string imageUrl { get; set; }
            public int year { get; set; }
            public int partCount { get; set; }
            public string themeName { get; set; }
            public string rebrickableUrl { get; set; }
            public int quantity { get; set; }
            public bool includeSpares { get; set; }
        }

        /// <summary>
        /// Detailed DTO for a single set list with all items.
        /// </summary>
        public class SetListDetailDto
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool isBuildable { get; set; }
            public int? rebrickableListId { get; set; }
            public int versionNumber { get; set; }
            public int pendingSyncCount { get; set; }
            public List<SetListItemDto> items { get; set; } = new();
        }

        /// <summary>
        /// Request body for creating a new set list.
        /// </summary>
        public class CreateSetListRequest
        {
            public string name { get; set; }
            public bool isBuildable { get; set; }
        }

        /// <summary>
        /// Request body for updating a set list.
        /// </summary>
        public class UpdateSetListRequest
        {
            public string name { get; set; }
            public bool? isBuildable { get; set; }
        }

        /// <summary>
        /// Request body for adding a set to a set list.
        /// </summary>
        public class AddSetToListRequest
        {
            public int legoSetId { get; set; }
            public int quantity { get; set; } = 1;
            public bool includeSpares { get; set; } = true;
        }

        /// <summary>
        /// Request body for updating a set list item.
        /// </summary>
        public class UpdateSetListItemRequest
        {
            public int? quantity { get; set; }
            public bool? includeSpares { get; set; }
        }

        /// <summary>
        /// Version history entry DTO.
        /// </summary>
        public class VersionHistoryDto
        {
            public int versionNumber { get; set; }
            public string changeDescription { get; set; }
            public DateTime changeDate { get; set; }
        }

        #endregion


        // ─────────────────────────────────────────────────────────────────
        // GET /api/user-set-lists — all set lists for the current tenant
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// GET /api/user-set-lists
        ///
        /// Returns all set lists for the current tenant with summary statistics.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/user-set-lists")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var setLists = await _context.UserSetLists
                .Where(sl => sl.tenantGuid == tenantGuid && sl.active == true && sl.deleted == false)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var setListIds = setLists.Select(sl => sl.id).ToList();

            // Item counts per set list
            var itemCounts = await _context.UserSetListItems
                .Where(i => setListIds.Contains(i.userSetListId) && i.active == true && i.deleted == false)
                .GroupBy(i => i.userSetListId)
                .Select(g => new
                {
                    setListId = g.Key,
                    itemCount = g.Count(),
                    totalSets = g.Sum(i => i.quantity)
                })
                .ToListAsync(cancellationToken);

            // Pending sync count per set list
            var pendingSyncCounts = await _context.RebrickableSyncQueues
                .Where(q =>
                    q.tenantGuid == tenantGuid
                    && q.active == true
                    && q.deleted == false
                    && (q.status == "Pending" || q.status == "InProgress" || q.status == "Failed")
                    && (q.entityType == "SetList" || q.entityType == "SetListItem"))
                .GroupBy(q => q.entityId)
                .Select(g => new { entityId = g.Key, count = g.Count() })
                .ToListAsync(cancellationToken);

            var summaries = setLists.Select(sl =>
            {
                var ic = itemCounts.FirstOrDefault(x => x.setListId == sl.id);
                var pc = pendingSyncCounts.FirstOrDefault(x => x.entityId == sl.id);

                return new SetListSummaryDto
                {
                    id = sl.id,
                    name = sl.name,
                    isBuildable = sl.isBuildable,
                    rebrickableListId = sl.rebrickableListId,
                    itemCount = ic?.itemCount ?? 0,
                    totalSets = ic?.totalSets ?? 0,
                    totalParts = 0,  // Will be computed by join in a future enhancement
                    versionNumber = sl.versionNumber,
                    pendingSyncCount = pc?.count ?? 0
                };
            }).ToList();

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, $"Get set lists — {summaries.Count} lists");

            return Ok(summaries);
        }


        // ─────────────────────────────────────────────────────────────────
        // GET /api/user-set-lists/{id} — single set list with items
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// GET /api/user-set-lists/{id}
        ///
        /// Returns a single set list with all items, including denormalized set info.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/user-set-lists/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var setList = await _context.UserSetLists
                .AsNoTracking()
                .FirstOrDefaultAsync(sl =>
                    sl.id == id
                    && sl.tenantGuid == tenantGuid
                    && sl.active == true
                    && sl.deleted == false, cancellationToken);

            if (setList == null)
            {
                return NotFound("Set list not found.");
            }

            var items = await _context.UserSetListItems
                .Where(i => i.userSetListId == id && i.active == true && i.deleted == false)
                .Select(i => new SetListItemDto
                {
                    id = i.id,
                    legoSetId = i.legoSetId,
                    setName = i.legoSet.name,
                    setNumber = i.legoSet.setNumber,
                    imageUrl = i.legoSet.imageUrl,
                    year = i.legoSet.year,
                    partCount = i.legoSet.partCount,
                    themeName = i.legoSet.legoTheme != null ? i.legoSet.legoTheme.name : null,
                    rebrickableUrl = i.legoSet.rebrickableUrl,
                    quantity = i.quantity,
                    includeSpares = i.includeSpares
                })
                .OrderBy(i => i.setNumber)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Pending sync count for this set list
            int pendingSync = await _context.RebrickableSyncQueues
                .CountAsync(q =>
                    q.tenantGuid == tenantGuid
                    && q.active == true
                    && q.deleted == false
                    && (q.status == "Pending" || q.status == "InProgress" || q.status == "Failed")
                    && q.entityId == id
                    && (q.entityType == "SetList" || q.entityType == "SetListItem"), cancellationToken);

            var detail = new SetListDetailDto
            {
                id = setList.id,
                name = setList.name,
                isBuildable = setList.isBuildable,
                rebrickableListId = setList.rebrickableListId,
                versionNumber = setList.versionNumber,
                pendingSyncCount = pendingSync,
                items = items
            };

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, $"Get set list — id={id}, items={items.Count}", id.ToString());

            return Ok(detail);
        }


        // ─────────────────────────────────────────────────────────────────
        // POST /api/user-set-lists — create a new set list
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// POST /api/user-set-lists
        ///
        /// Creates a new set list. Enqueues a sync message if the user's
        /// integration mode supports pushing.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/user-set-lists")]
        public async Task<IActionResult> Create([FromBody] CreateSetListRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (request == null || string.IsNullOrWhiteSpace(request.name))
            {
                return BadRequest("Name is required.");
            }

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var newList = new UserSetList
            {
                name = request.name.Trim(),
                isBuildable = request.isBuildable,
                rebrickableListId = null,
                tenantGuid = tenantGuid,
                objectGuid = Guid.NewGuid(),
                versionNumber = 1,
                active = true,
                deleted = false
            };

            _context.UserSetLists.Add(newList);
            await _context.SaveChangesAsync(cancellationToken);

            // Enqueue sync
            await EnqueueSyncAsync(tenantGuid, "Create", "SetList", newList.id,
                new { name = newList.name, isBuildable = newList.isBuildable }, cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                $"Create set list — '{newList.name}'", newList.id.ToString());

            return Ok(new { id = newList.id, name = newList.name });
        }


        // ─────────────────────────────────────────────────────────────────
        // PUT /api/user-set-lists/{id} — update a set list
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// PUT /api/user-set-lists/{id}
        ///
        /// Updates the set list name and/or isBuildable flag.
        /// Increments version number. Enqueues sync if applicable.
        /// </summary>
        [HttpPut]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/user-set-lists/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSetListRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (request == null)
            {
                return BadRequest();
            }

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var setList = await _context.UserSetLists
                .FirstOrDefaultAsync(sl =>
                    sl.id == id
                    && sl.tenantGuid == tenantGuid
                    && sl.active == true
                    && sl.deleted == false, cancellationToken);

            if (setList == null)
            {
                return NotFound("Set list not found.");
            }

            if (!string.IsNullOrWhiteSpace(request.name))
            {
                setList.name = request.name.Trim();
            }

            if (request.isBuildable.HasValue)
            {
                setList.isBuildable = request.isBuildable.Value;
            }

            // Increment version
            setList.versionNumber = setList.versionNumber + 1;

            await _context.SaveChangesAsync(cancellationToken);

            // Enqueue sync only if this list is linked to Rebrickable
            if (setList.rebrickableListId.HasValue)
            {
                await EnqueueSyncAsync(tenantGuid, "Update", "SetList", setList.id,
                    new { rebrickableListId = setList.rebrickableListId.Value, name = setList.name, isBuildable = setList.isBuildable },
                    cancellationToken);
            }

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                $"Update set list — id={id}, v{setList.versionNumber}", id.ToString());

            return Ok(new { id = setList.id, name = setList.name, versionNumber = setList.versionNumber });
        }


        // ─────────────────────────────────────────────────────────────────
        // DELETE /api/user-set-lists/{id} — soft-delete a set list
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// DELETE /api/user-set-lists/{id}
        ///
        /// Soft-deletes a set list and all its items. Enqueues sync if linked to Rebrickable.
        /// </summary>
        [HttpDelete]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/user-set-lists/{id}")]
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

            var setList = await _context.UserSetLists
                .FirstOrDefaultAsync(sl =>
                    sl.id == id
                    && sl.tenantGuid == tenantGuid
                    && sl.active == true
                    && sl.deleted == false, cancellationToken);

            if (setList == null)
            {
                return NotFound("Set list not found.");
            }

            // Soft-delete the list
            setList.deleted = true;
            setList.active = false;

            // Soft-delete all items in the list
            var items = await _context.UserSetListItems
                .Where(i => i.userSetListId == id && i.deleted == false)
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.deleted = true;
                item.active = false;
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Enqueue sync if linked
            if (setList.rebrickableListId.HasValue)
            {
                await EnqueueSyncAsync(tenantGuid, "Delete", "SetList", setList.id,
                    new { rebrickableListId = setList.rebrickableListId.Value },
                    cancellationToken);
            }

            await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
                $"Delete set list — id={id}, '{setList.name}'", id.ToString());

            return Ok(new { action = "deleted" });
        }


        // ─────────────────────────────────────────────────────────────────
        // POST /api/user-set-lists/{id}/items — add a set to the list
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// POST /api/user-set-lists/{id}/items
        ///
        /// Adds a set to the list. If the set already exists, increments the quantity.
        /// Enqueues sync if the list is linked to Rebrickable.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/user-set-lists/{id}/items")]
        public async Task<IActionResult> AddItem(int id, [FromBody] AddSetToListRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (request == null)
            {
                return BadRequest();
            }

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            // Verify list ownership
            var setList = await _context.UserSetLists
                .AsNoTracking()
                .FirstOrDefaultAsync(sl =>
                    sl.id == id
                    && sl.tenantGuid == tenantGuid
                    && sl.active == true
                    && sl.deleted == false, cancellationToken);

            if (setList == null)
            {
                return NotFound("Set list not found.");
            }

            // Verify the LEGO set exists
            var legoSet = await _context.LegoSets
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.id == request.legoSetId && s.active == true && s.deleted == false, cancellationToken);

            if (legoSet == null)
            {
                return NotFound("LEGO set not found.");
            }

            // Check for existing item (upsert)
            var existing = await _context.UserSetListItems
                .FirstOrDefaultAsync(i =>
                    i.userSetListId == id
                    && i.legoSetId == request.legoSetId
                    && i.active == true
                    && i.deleted == false, cancellationToken);

            string action;
            int itemId;

            if (existing != null)
            {
                existing.quantity = existing.quantity + Math.Max(request.quantity, 1);
                existing.includeSpares = request.includeSpares;
                action = "updated";
                itemId = existing.id;
            }
            else
            {
                var newItem = new UserSetListItem
                {
                    userSetListId = id,
                    legoSetId = request.legoSetId,
                    quantity = Math.Max(request.quantity, 1),
                    includeSpares = request.includeSpares,
                    tenantGuid = tenantGuid,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.UserSetListItems.Add(newItem);
                await _context.SaveChangesAsync(cancellationToken);
                action = "created";
                itemId = newItem.id;
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Enqueue sync if linked
            if (setList.rebrickableListId.HasValue)
            {
                await EnqueueSyncAsync(tenantGuid, "Create", "SetListItem", itemId,
                    new { rebrickableListId = setList.rebrickableListId.Value, setNum = legoSet.setNumber, quantity = request.quantity },
                    cancellationToken);
            }

            await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                $"Add set to list — listId={id}, set={legoSet.setNumber}, qty={request.quantity}", itemId.ToString());

            return Ok(new { action, id = itemId });
        }


        // ─────────────────────────────────────────────────────────────────
        // PUT /api/user-set-lists/{id}/items/{itemId} — update item
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// PUT /api/user-set-lists/{id}/items/{itemId}
        ///
        /// Updates the quantity or includeSpares flag of a set list item.
        /// </summary>
        [HttpPut]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/user-set-lists/{id}/items/{itemId}")]
        public async Task<IActionResult> UpdateItem(int id, int itemId, [FromBody] UpdateSetListItemRequest request, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (request == null)
            {
                return BadRequest();
            }

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            // Verify list ownership
            bool listExists = await _context.UserSetLists
                .AnyAsync(sl =>
                    sl.id == id
                    && sl.tenantGuid == tenantGuid
                    && sl.active == true
                    && sl.deleted == false, cancellationToken);

            if (!listExists)
            {
                return NotFound("Set list not found.");
            }

            var item = await _context.UserSetListItems
                .FirstOrDefaultAsync(i =>
                    i.id == itemId
                    && i.userSetListId == id
                    && i.active == true
                    && i.deleted == false, cancellationToken);

            if (item == null)
            {
                return NotFound("Set list item not found.");
            }

            if (request.quantity.HasValue)
            {
                item.quantity = Math.Max(request.quantity.Value, 1);
            }

            if (request.includeSpares.HasValue)
            {
                item.includeSpares = request.includeSpares.Value;
            }

            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                $"Update set list item — listId={id}, itemId={itemId}", itemId.ToString());

            return Ok(new { id = item.id, quantity = item.quantity, includeSpares = item.includeSpares });
        }


        // ─────────────────────────────────────────────────────────────────
        // DELETE /api/user-set-lists/{id}/items/{itemId} — remove item
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// DELETE /api/user-set-lists/{id}/items/{itemId}
        ///
        /// Soft-deletes a set list item. Enqueues sync if list is linked.
        /// </summary>
        [HttpDelete]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/user-set-lists/{id}/items/{itemId}")]
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

            // Verify list ownership and get rebrickableListId
            var setList = await _context.UserSetLists
                .AsNoTracking()
                .FirstOrDefaultAsync(sl =>
                    sl.id == id
                    && sl.tenantGuid == tenantGuid
                    && sl.active == true
                    && sl.deleted == false, cancellationToken);

            if (setList == null)
            {
                return NotFound("Set list not found.");
            }

            var item = await _context.UserSetListItems
                .Include(i => i.legoSet)
                .FirstOrDefaultAsync(i =>
                    i.id == itemId
                    && i.userSetListId == id
                    && i.active == true
                    && i.deleted == false, cancellationToken);

            if (item == null)
            {
                return NotFound("Set list item not found.");
            }

            item.deleted = true;
            item.active = false;
            await _context.SaveChangesAsync(cancellationToken);

            // Enqueue sync if linked
            if (setList.rebrickableListId.HasValue && item.legoSet != null)
            {
                await EnqueueSyncAsync(tenantGuid, "Delete", "SetListItem", itemId,
                    new { rebrickableListId = setList.rebrickableListId.Value, setNum = item.legoSet.setNumber },
                    cancellationToken);
            }

            await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
                $"Remove set from list — listId={id}, itemId={itemId}", itemId.ToString());

            return Ok(new { action = "removed" });
        }


        // ─────────────────────────────────────────────────────────────────
        // GET /api/user-set-lists/{id}/history — version history
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// GET /api/user-set-lists/{id}/history
        ///
        /// Returns version change history for a set list from Foundation's
        /// version control system.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/user-set-lists/{id}/history")]
        public async Task<IActionResult> GetHistory(int id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            // Verify list ownership
            bool listExists = await _context.UserSetLists
                .AnyAsync(sl =>
                    sl.id == id
                    && sl.tenantGuid == tenantGuid
                    && sl.active == true
                    && sl.deleted == false, cancellationToken);

            if (!listExists)
            {
                return NotFound("Set list not found.");
            }

            // Query the change history table generated by Foundation's version control
            var history = await _context.UserSetListChangeHistories
                .Where(h => h.userSetListId == id)
                .OrderByDescending(h => h.versionNumber)
                .Select(h => new VersionHistoryDto
                {
                    versionNumber = h.versionNumber,
                    changeDescription = h.data,
                    changeDate = h.timeStamp
                })
                .Take(50)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"Get set list history — id={id}, entries={history.Count}", id.ToString());

            return Ok(history);
        }


        // ─────────────────────────────────────────────────────────────────
        // Helper: Get current tenant GUID
        // ─────────────────────────────────────────────────────────────────

        private async Task<Guid> GetTenantGuidOrFail(CancellationToken ct)
        {
            try
            {
                SecurityUser securityUser = await GetSecurityUserAsync(ct);
                return await UserTenantGuidAsync(securityUser, ct);
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }


        // ─────────────────────────────────────────────────────────────────
        // Helper: Enqueue a sync message
        // ─────────────────────────────────────────────────────────────────

        private async Task EnqueueSyncAsync(
            Guid tenantGuid,
            string operationType,
            string entityType,
            long entityId,
            object payloadData,
            CancellationToken ct)
        {
            //
            // Check the user's integration mode — only enqueue if push-capable
            //
            var userLink = await _context.RebrickableUserLinks
                .AsNoTracking()
                .FirstOrDefaultAsync(ul =>
                    ul.tenantGuid == tenantGuid
                    && ul.active == true
                    && ul.deleted == false, ct);

            if (userLink == null) return;

            if (userLink.syncDirectionFlags != RebrickableSyncService.MODE_REALTIME &&
                userLink.syncDirectionFlags != RebrickableSyncService.MODE_PUSH_ONLY)
            {
                return;
            }

            var queueEntry = new RebrickableSyncQueue
            {
                tenantGuid = tenantGuid,
                operationType = operationType,
                entityType = entityType,
                entityId = entityId,
                payload = JsonSerializer.Serialize(payloadData),
                status = "Pending",
                createdDate = DateTime.UtcNow,
                attemptCount = 0,
                maxAttempts = DEFAULT_MAX_SYNC_ATTEMPTS,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.RebrickableSyncQueues.Add(queueEntry);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Sync queue: enqueued {Operation}.{EntityType} for entity {EntityId}",
                operationType, entityType, entityId);
        }
    }
}
