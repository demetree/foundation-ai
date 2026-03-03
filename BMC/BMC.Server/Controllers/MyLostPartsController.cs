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
    /// Custom controller for "My Lost Parts" — tracking parts lost from sets.
    ///
    /// Endpoints:
    ///   GET    /api/my-lost-parts          — All lost parts with denormalized info
    ///   GET    /api/my-lost-parts/stats    — Summary statistics
    ///   POST   /api/my-lost-parts          — Report a lost part
    ///   PUT    /api/my-lost-parts/{id}     — Update lost quantity
    ///   DELETE /api/my-lost-parts/{id}     — Remove (found it!)
    ///
    /// All endpoints are tenant-scoped. Write operations check "BMC Collection Writer" role.
    /// </summary>
    public class MyLostPartsController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;

        private readonly BMCContext _context;
        private readonly ILogger<MyLostPartsController> _logger;


        public MyLostPartsController(BMCContext context, ILogger<MyLostPartsController> logger)
            : base("BMC", "UserLostPart")
        {
            _context = context;
            _logger = logger;

            _context.Database.SetCommandTimeout(60);
        }


        #region DTOs

        /// <summary>
        /// DTO for a lost part with denormalized part, colour, and set info.
        /// </summary>
        public class LostPartDto
        {
            public int id { get; set; }
            public int brickPartId { get; set; }
            public string partName { get; set; }
            public string partNum { get; set; }
            public string partImageUrl { get; set; }
            public string partCategory { get; set; }
            public int brickColourId { get; set; }
            public string colourName { get; set; }
            public string colourHex { get; set; }
            public bool isTransparent { get; set; }
            public int? legoSetId { get; set; }
            public string setNum { get; set; }
            public string setName { get; set; }
            public string setImageUrl { get; set; }
            public int lostQuantity { get; set; }
            public int? rebrickableInvPartId { get; set; }
        }

        /// <summary>
        /// Summary statistics for lost parts.
        /// </summary>
        public class LostPartsStatsDto
        {
            public int totalRecords { get; set; }
            public int totalLostParts { get; set; }
            public int uniqueParts { get; set; }
            public int setsAffected { get; set; }
            public int uniqueColours { get; set; }
        }

        /// <summary>
        /// Request body for reporting a lost part.
        /// </summary>
        public class ReportLostPartRequest
        {
            public int brickPartId { get; set; }
            public int brickColourId { get; set; }
            public int? legoSetId { get; set; }
            public int lostQuantity { get; set; } = 1;
        }

        /// <summary>
        /// Request body for updating a lost part record.
        /// </summary>
        public class UpdateLostPartRequest
        {
            public int? lostQuantity { get; set; }
        }

        #endregion


        // ─────────────────────────────────────────────────────────────────
        // GET /api/my-lost-parts — all lost parts with denormalized info
        // ─────────────────────────────────────────────────────────────────

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-lost-parts")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string search = null,
            [FromQuery] int? setId = null,
            [FromQuery] string sort = "partName",
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var query = _context.UserLostParts
                .Where(lp => lp.tenantGuid == tenantGuid && lp.active == true && lp.deleted == false)
                .AsNoTracking();

            // Filter by set
            if (setId.HasValue)
            {
                query = query.Where(lp => lp.legoSetId == setId.Value);
            }

            // Project with denormalized data
            var projected = query.Select(lp => new LostPartDto
            {
                id = lp.id,
                brickPartId = lp.brickPartId,
                partName = lp.brickPart.name,
                partNum = lp.brickPart.rebrickablePartNum,
                partImageUrl = lp.brickPart.rebrickableImgUrl,
                partCategory = lp.brickPart.brickCategory != null ? lp.brickPart.brickCategory.name : null,
                brickColourId = lp.brickColourId,
                colourName = lp.brickColour.name,
                colourHex = lp.brickColour.hexRgb,
                isTransparent = lp.brickColour.isTransparent,
                legoSetId = lp.legoSetId,
                setNum = lp.legoSet != null ? lp.legoSet.setNumber : null,
                setName = lp.legoSet != null ? lp.legoSet.name : null,
                setImageUrl = lp.legoSet != null ? lp.legoSet.imageUrl : null,
                lostQuantity = lp.lostQuantity,
                rebrickableInvPartId = lp.rebrickableInvPartId
            });

            // Search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                projected = projected.Where(lp =>
                    (lp.partName != null && lp.partName.ToLower().Contains(term))
                    || (lp.partNum != null && lp.partNum.ToLower().Contains(term))
                    || (lp.colourName != null && lp.colourName.ToLower().Contains(term))
                    || (lp.setName != null && lp.setName.ToLower().Contains(term))
                    || (lp.setNum != null && lp.setNum.ToLower().Contains(term))
                );
            }

            // Sort
            projected = sort switch
            {
                "partName" => projected.OrderBy(lp => lp.partName),
                "colour" => projected.OrderBy(lp => lp.colourName),
                "set" => projected.OrderBy(lp => lp.setName),
                "quantity-desc" => projected.OrderByDescending(lp => lp.lostQuantity),
                "quantity-asc" => projected.OrderBy(lp => lp.lostQuantity),
                _ => projected.OrderBy(lp => lp.partName)
            };

            var results = await projected.ToListAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"Get my lost parts — {results.Count} records");

            return Ok(results);
        }


        // ─────────────────────────────────────────────────────────────────
        // GET /api/my-lost-parts/stats — summary statistics
        // ─────────────────────────────────────────────────────────────────

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-lost-parts/stats")]
        public async Task<IActionResult> GetStats(CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var baseQuery = _context.UserLostParts
                .Where(lp => lp.tenantGuid == tenantGuid && lp.active == true && lp.deleted == false);

            var stats = new LostPartsStatsDto
            {
                totalRecords = await baseQuery.CountAsync(cancellationToken),
                totalLostParts = await baseQuery.SumAsync(lp => lp.lostQuantity, cancellationToken),
                uniqueParts = await baseQuery.Select(lp => lp.brickPartId).Distinct().CountAsync(cancellationToken),
                setsAffected = await baseQuery
                    .Where(lp => lp.legoSetId != null)
                    .Select(lp => lp.legoSetId)
                    .Distinct()
                    .CountAsync(cancellationToken),
                uniqueColours = await baseQuery.Select(lp => lp.brickColourId).Distinct().CountAsync(cancellationToken)
            };

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity,
                $"Get lost parts stats — {stats.totalRecords} records, {stats.totalLostParts} total lost");

            return Ok(stats);
        }


        // ─────────────────────────────────────────────────────────────────
        // POST /api/my-lost-parts — report a lost part
        // ─────────────────────────────────────────────────────────────────

        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-lost-parts")]
        public async Task<IActionResult> Report([FromBody] ReportLostPartRequest request, CancellationToken cancellationToken = default)
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

            // Verify part exists
            var partExists = await _context.BrickParts.AnyAsync(
                p => p.id == request.brickPartId && p.active == true && p.deleted == false, cancellationToken);
            if (!partExists) return NotFound("Brick part not found.");

            // Verify colour exists
            var colourExists = await _context.BrickColours.AnyAsync(
                c => c.id == request.brickColourId && c.active == true && c.deleted == false, cancellationToken);
            if (!colourExists) return NotFound("Brick colour not found.");

            // Verify set exists (if provided)
            if (request.legoSetId.HasValue)
            {
                var setExists = await _context.LegoSets.AnyAsync(
                    s => s.id == request.legoSetId.Value && s.active == true && s.deleted == false, cancellationToken);
                if (!setExists) return NotFound("LEGO set not found.");
            }

            // Check for existing record (same part + colour + set = upsert)
            var existing = await _context.UserLostParts
                .FirstOrDefaultAsync(lp =>
                    lp.tenantGuid == tenantGuid
                    && lp.brickPartId == request.brickPartId
                    && lp.brickColourId == request.brickColourId
                    && lp.legoSetId == request.legoSetId
                    && lp.active == true
                    && lp.deleted == false, cancellationToken);

            string action;
            int resultId;

            if (existing != null)
            {
                existing.lostQuantity += Math.Max(request.lostQuantity, 1);
                action = "updated";
                resultId = existing.id;
            }
            else
            {
                var newRecord = new UserLostPart
                {
                    tenantGuid = tenantGuid,
                    brickPartId = request.brickPartId,
                    brickColourId = request.brickColourId,
                    legoSetId = request.legoSetId,
                    lostQuantity = Math.Max(request.lostQuantity, 1),
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.UserLostParts.Add(newRecord);
                await _context.SaveChangesAsync(cancellationToken);
                action = "created";
                resultId = newRecord.id;
            }

            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                $"Report lost part — partId={request.brickPartId}, colourId={request.brickColourId}, qty={request.lostQuantity}",
                resultId.ToString());

            return Ok(new { action, id = resultId });
        }


        // ─────────────────────────────────────────────────────────────────
        // PUT /api/my-lost-parts/{id} — update lost quantity
        // ─────────────────────────────────────────────────────────────────

        [HttpPut]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-lost-parts/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLostPartRequest request, CancellationToken cancellationToken = default)
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

            var record = await _context.UserLostParts
                .FirstOrDefaultAsync(lp =>
                    lp.id == id
                    && lp.tenantGuid == tenantGuid
                    && lp.active == true
                    && lp.deleted == false, cancellationToken);

            if (record == null) return NotFound("Lost part record not found.");

            if (request.lostQuantity.HasValue)
            {
                record.lostQuantity = Math.Max(request.lostQuantity.Value, 1);
            }

            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
                $"Update lost part — id={id}, qty={record.lostQuantity}", id.ToString());

            return Ok(new { id = record.id, lostQuantity = record.lostQuantity });
        }


        // ─────────────────────────────────────────────────────────────────
        // DELETE /api/my-lost-parts/{id} — found the part, remove record
        // ─────────────────────────────────────────────────────────────────

        [HttpDelete]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-lost-parts/{id}")]
        public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Collection Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var record = await _context.UserLostParts
                .FirstOrDefaultAsync(lp =>
                    lp.id == id
                    && lp.tenantGuid == tenantGuid
                    && lp.active == true
                    && lp.deleted == false, cancellationToken);

            if (record == null) return NotFound("Lost part record not found.");

            record.deleted = true;
            record.active = false;
            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
                $"Remove lost part (found!) — id={id}", id.ToString());

            return Ok(new { action = "removed" });
        }


        // ─────────────────────────────────────────────────────────────────
        // GET /api/my-lost-parts/affected-sets — distinct sets for filter
        // ─────────────────────────────────────────────────────────────────

        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/my-lost-parts/affected-sets")]
        public async Task<IActionResult> GetAffectedSets(CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            Guid tenantGuid = await GetTenantGuidOrFail(cancellationToken);
            if (tenantGuid == Guid.Empty) return Problem("Your user account is not configured with a tenant.");

            var sets = await _context.UserLostParts
                .Where(lp => lp.tenantGuid == tenantGuid
                          && lp.active == true
                          && lp.deleted == false
                          && lp.legoSetId != null)
                .Select(lp => new
                {
                    id = lp.legoSetId,
                    setNum = lp.legoSet.setNumber,
                    name = lp.legoSet.name
                })
                .Distinct()
                .OrderBy(s => s.name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            await CreateAuditEventAsync(AuditEngine.AuditType.ReadList,
                $"Get affected sets — {sets.Count} sets");

            return Ok(sets);
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
                    $"Non-tenant user attempted to access my-lost-parts — {securityUser?.accountName}",
                    securityUser?.accountName, ex);
                return Guid.Empty;
            }
        }
    }
}
