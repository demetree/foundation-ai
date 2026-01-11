using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Scheduler.Database;
using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Foundation.Auditor.AuditEngine;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
	///
	/// Adds custom setters for hierarchical specificity rule enforcement
	///
    /// </summary>
	public partial class RateSheetsController : SecureWebAPIController
    {

        /// <summary>
        /// 
        /// This updates an existing RateSheet record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        [Route("api/RateSheet/{id}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> PutRateSheet(int id, [FromBody] Database.RateSheet.RateSheetDTO rateSheetDTO, CancellationToken cancellationToken = default)
        {
            if (rateSheetDTO == null)
            {
                return BadRequest();
            }

            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }



            if (id != rateSheetDTO.id)
            {
                return BadRequest();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
            bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
            bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }


            IQueryable<Database.RateSheet> query = (from x in _context.RateSheets
                                                    where
                                                    (x.id == id)
                                                    select x);

            query = query.Where(x => x.tenantGuid == userTenantGuid);

            Database.RateSheet existing = await query.FirstOrDefaultAsync(cancellationToken);

            if (existing == null)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.RateSheet PUT", id.ToString(), new Exception("No Scheduler.RateSheet entity could be found with the primary key provided."));
                return NotFound();
            }

            // Copy the existing object so it can be serialized as-is in the audit and history logs.
            Database.RateSheet cloneOfExisting = (Database.RateSheet)_context.Entry(existing).GetDatabaseValues().ToObject();

            //
            // Create a new RateSheet object using the data from the existing record, updated with what is in the DTO.
            //
            Database.RateSheet rateSheet = (Database.RateSheet)_context.Entry(existing).GetDatabaseValues().ToObject();
            rateSheet.ApplyDTO(rateSheetDTO);
            //
            // The tenant guid for any RateSheet being saved must match the tenant guid of the user.  
            //
            if (existing.tenantGuid != userTenantGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
                return Problem("Data integrity violation detected while attempting to save.");
            }
            else
            {
                // Assign the tenantGuid to the RateSheet because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
                rateSheet.tenantGuid = existing.tenantGuid;
            }

            //
            // Validate the hierarchy of the rate sheet before allowing save to proceed
            //
            (bool isValid, IActionResult value) = await ValidateHierarchyRules(userTenantGuid, rateSheet, cancellationToken);

            if (!isValid)
            {
                return value;
            }

            lock (rateSheetPutSyncRoot)
            {
                //
                // Validate the version number for the rateSheet being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
                //
                if (existing.versionNumber != rateSheet.versionNumber)
                {
                    // Record has changed
                    CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "RateSheet save attempt was made but save request was with version " + rateSheet.versionNumber + " and the current version number is " + existing.versionNumber, false);
                    return Problem("The rateSheet you are trying to update has already changed.  Please try your save again after reloading the RateSheet.");
                }
                else
                {
                    // Same record.  Increase version.
                    rateSheet.versionNumber++;
                }


                // Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
                if (userIsAdmin == false && (rateSheet.deleted == true || existing.deleted == true))
                {
                    // we're not recording state here because it is not being changed.
                    CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.RateSheet record.", id.ToString());
                    DestroySessionAndAuthentication();
                    return Forbid();
                }

                if (rateSheet.effectiveDate.Kind != DateTimeKind.Utc)
                {
                    rateSheet.effectiveDate = rateSheet.effectiveDate.ToUniversalTime();
                }


                try
                {
                    EntityEntry<Database.RateSheet> attached = _context.Entry(existing);
                    attached.CurrentValues.SetValues(rateSheet);

                    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                    {
                        _context.SaveChanges();

                        //
                        // Now add the change history
                        //
                        RateSheetChangeHistory rateSheetChangeHistory = new RateSheetChangeHistory();
                        rateSheetChangeHistory.rateSheetId = rateSheet.id;
                        rateSheetChangeHistory.versionNumber = rateSheet.versionNumber;
                        rateSheetChangeHistory.timeStamp = DateTime.UtcNow;
                        rateSheetChangeHistory.userId = securityUser.id;
                        rateSheetChangeHistory.tenantGuid = userTenantGuid;
                        rateSheetChangeHistory.data = JsonSerializer.Serialize(Database.RateSheet.CreateAnonymousWithFirstLevelSubObjects(rateSheet));
                        _context.RateSheetChangeHistories.Add(rateSheetChangeHistory);

                        _context.SaveChanges();

                        transaction.Commit();
                    }

                    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
                        "Scheduler.RateSheet entity successfully updated.",
                        true,
                        id.ToString(),
                        JsonSerializer.Serialize(Database.RateSheet.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                        JsonSerializer.Serialize(Database.RateSheet.CreateAnonymousWithFirstLevelSubObjects(rateSheet)),
                        null);

                    return Ok(Database.RateSheet.CreateAnonymous(rateSheet));
                }
                catch (Exception ex)
                {
                    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
                        "Scheduler.RateSheet entity update failed",
                        false,
                        id.ToString(),
                        JsonSerializer.Serialize(Database.RateSheet.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                        JsonSerializer.Serialize(Database.RateSheet.CreateAnonymousWithFirstLevelSubObjects(rateSheet)),
                        ex);

                    return Problem(ex.Message);
                }
            }
        }


        /// <summary>
        /// 
        /// This creates a new RateSheet record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/RateSheet", Name = "RateSheet")]
        public async Task<IActionResult> PostRateSheet([FromBody] Database.RateSheet.RateSheetDTO rateSheetDTO, CancellationToken cancellationToken = default)
        {
            if (rateSheetDTO == null)
            {
                return BadRequest();
            }

            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }



            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
            bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to interact with a multi-tenancy enabled table by a user that is not configured with a tenant.  The User is " + securityUser?.accountName, securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            //
            // Create a new RateSheet object using the data from the DTO
            //
            Database.RateSheet rateSheet = Database.RateSheet.FromDTO(rateSheetDTO);

            try
            {
                //
                // Ensure that the tenant data is correct.
                //
                rateSheet.tenantGuid = userTenantGuid;

                if (rateSheet.effectiveDate.Kind != DateTimeKind.Utc)
                {
                    rateSheet.effectiveDate = rateSheet.effectiveDate.ToUniversalTime();
                }

                rateSheet.objectGuid = Guid.NewGuid();
                rateSheet.versionNumber = 1;


                //
                // Validate the hierarchy of the rate sheet before allowing save to proceed
                //
                (bool isValid, IActionResult value) = await ValidateHierarchyRules(userTenantGuid, rateSheet, cancellationToken);

                if (!isValid)
                {
                    return value;
                }


                _context.RateSheets.Add(rateSheet);

                await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
                {
                    await _context.SaveChangesAsync(cancellationToken);

                    //
                    // Now add the change history
                    //

                    //
                    // Detach the rateSheet object so that no further changes will be written to the database
                    //
                    _context.Entry(rateSheet).State = EntityState.Detached;

                    //
                    // Nullify all object properties before serializing.
                    //
                    rateSheet.RateSheetChangeHistories = null;
                    rateSheet.assignmentRole = null;
                    rateSheet.currency = null;
                    rateSheet.rateType = null;
                    rateSheet.resource = null;
                    rateSheet.schedulingTarget = null;


                    RateSheetChangeHistory rateSheetChangeHistory = new RateSheetChangeHistory();
                    rateSheetChangeHistory.rateSheetId = rateSheet.id;
                    rateSheetChangeHistory.versionNumber = rateSheet.versionNumber;
                    rateSheetChangeHistory.timeStamp = DateTime.UtcNow;
                    rateSheetChangeHistory.userId = securityUser.id;
                    rateSheetChangeHistory.tenantGuid = userTenantGuid;
                    rateSheetChangeHistory.data = JsonSerializer.Serialize(Database.RateSheet.CreateAnonymousWithFirstLevelSubObjects(rateSheet));
                    _context.RateSheetChangeHistories.Add(rateSheetChangeHistory);
                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                        "Scheduler.RateSheet entity successfully created.",
                        true,
                        rateSheet.id.ToString(),
                        "",
                        JsonSerializer.Serialize(Database.RateSheet.CreateAnonymousWithFirstLevelSubObjects(rateSheet)),
                        null);


                }
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.RateSheet entity creation failed.", false, rateSheet.id.ToString(), "", JsonSerializer.Serialize(Database.RateSheet.CreateAnonymousWithFirstLevelSubObjects(rateSheet)), ex);

                return Problem(ex.Message);
            }


            BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "RateSheet", rateSheet.id, rateSheet.id.ToString()));

            return CreatedAtRoute("RateSheet", new { id = rateSheet.id }, Database.RateSheet.CreateAnonymousWithFirstLevelSubObjects(rateSheet));
        }



        /// <summary>
        /// Validates the business rules for RateSheet hierarchy and uniqueness.
        /// Returns (true, null) if valid; otherwise (false, appropriate IActionResult).
        /// 
        /// Rules:
        /// 
        /// 1. Resource + Project + Role
        /// 2. Resource + Project
        /// 3. Resource + Role
        /// 4. Resource + Office
        /// 5. Role + Office
        /// 6. Project
        /// 7. Office
        /// 8. Role
        /// 9. Global
        /// 
        /// </summary>
        private async Task<(bool IsValid, IActionResult Result)> ValidateHierarchyRules(Guid userTenantGuid,
                                                                                        Database.RateSheet rateSheet,
                                                                                        CancellationToken cancellationToken)
        {
            //
            // Rule 1: Must have at least one specificity dimension (Role or Resource)
            //
            if (rateSheet.assignmentRoleId == null && rateSheet.resourceId == null)
            {
                string message = "RateSheet validation failed: At least one of AssignmentRole or Resource must be specified.";

                await CreateAuditEventAsync(AuditType.Error, message, false);

                //return (false, BadRequest("A RateSheet must apply to either a specific Role, a specific Resource, or both. Pure global rates are not supported."));
                return (false, Problem("A RateSheet must apply to either a specific Role, a specific Resource, or both. Pure global rates are not supported."));
            }

            //
            // Rule 2: Prevent ambiguous global rates (both Role and Resource with no Target)
            // Resource-specific rates override Role-specific, so both at global level is confusing.
            //
            if (rateSheet.schedulingTargetId == null &&
                rateSheet.assignmentRoleId.HasValue &&
                rateSheet.resourceId.HasValue)
            {
                string message = "RateSheet validation failed: Cannot specify both Role and Resource for a global rate (no SchedulingTarget). Resource rates take precedence.";

                await CreateAuditEventAsync(AuditType.Error, message, false);

                //return (false, BadRequest("For global rates, specify either Role or Resource — not both."));
                return (false, Problem("For global rates, specify either Role or Resource — not both."));
            }

            //
            // Rule 3: Prevent duplicates for same scope + rate type + effective date
            //
            bool hasDuplicate = await _context.RateSheets
                .AnyAsync(rs => rs.tenantGuid == userTenantGuid &&
                                rs.officeId == rateSheet.officeId &&
                                rs.assignmentRoleId == rateSheet.assignmentRoleId &&
                                rs.resourceId == rateSheet.resourceId &&
                                rs.schedulingTargetId == rateSheet.schedulingTargetId &&
                                rs.rateTypeId == rateSheet.rateTypeId &&
                                rs.effectiveDate.Date == rateSheet.effectiveDate.Date && // Compare date only
                                rs.id != rateSheet.id, // Exclude self during edit
                          cancellationToken);

            if (hasDuplicate)
            {
                string message = "RateSheet validation failed: Duplicate rate exists for this scope, rate type, and effective date.";

                await CreateAuditEventAsync(AuditType.Error, message, false);

                //return (false, Conflict("A RateSheet already exists with this exact combination of scope, Rate Type, and Effective Date."));
                return (false, Problem("A RateSheet already exists with this exact combination of scope, Rate Type, and Effective Date."));
            }


            //
            // Rule 4 - Rates Must not be negative
            //
            if (rateSheet.billingRate < 0)
            {
                return (false, Problem("Billing rate must not be less than 0."));
            }

            if (rateSheet.costRate < 0)
            {
                return (false, Problem("Cost rate must not be less than 0."));
            }


            // 
            // Rule 5: Warn if Billing Rate < Cost Rate (negative markup)
            // 
            if (rateSheet.billingRate < rateSheet.costRate)
            {
                string message = $"RateSheet saved with negative markup: Billing ({rateSheet.billingRate:C}) < Cost ({rateSheet.costRate:C})";

                _logger.LogWarning(message);

                await CreateAuditEventAsync(AuditType.Miscellaneous, message, false);
                // Not blocking — just a warning
            }

            // All rules passed
            return (true, null);
        }


        // ------------------------------------------------------------
        // Rate Preview / Resolve Endpoint (Fixed & Optimized)
        // ------------------------------------------------------------

        /// <summary>
        /// Resolves the applicable RateSheet for a hypothetical assignment based on the hierarchy rules.
        /// 
        /// This endpoint is used by the RateSheet preview tool to show exactly what rate would apply
        /// for the SPECIFIC combination of criteria provided by the user.
        /// 
        /// Important: When a parameter is not provided (null), it means "any" — we do NOT prefer
        /// more specific rates in that dimension. For example:
        /// - If no SchedulingTargetId is provided, only global rates are considered
        /// - If no ResourceId is provided, resource-specific rates are ignored
        /// 
        /// This ensures the preview matches real-world expectations: "show me the rate for an Operator doing Overtime"
        /// returns the global Operator rate, not a project override.
        /// 
        /// Query Parameters:
        /// - resourceId (optional)
        /// - assignmentRoleId (optional)
        /// - schedulingTargetId (optional)
        /// - rateTypeId (required)
        /// - date (required, UTC)
        /// 
        /// Rate limit: 5 per second per user
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.FivePerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/RateSheets/Resolve")]
        public async Task<IActionResult> ResolveRate(
            int? officeId,
            int? resourceId,
            int? assignmentRoleId,
            int? schedulingTargetId,
            [FromQuery] int rateTypeId,
            [FromQuery] DateTime date,
            CancellationToken cancellationToken = default)
        {
            // Basic read permission check
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);

            // Normalize date to UTC
            if (date.Kind != DateTimeKind.Utc)
            {
                date = date.ToUniversalTime();
            }

            // Validate required parameters
            if (rateTypeId <= 0)
            {
                return BadRequest("rateTypeId is required.");
            }

            // Base query: active, non-deleted, correct tenant, correct rate type, effective on or before the date
            var query = _context.RateSheets.Where(rs => rs.tenantGuid == userTenantGuid &&
                                                        rs.active == true &&
                                                        rs.deleted == false &&
                                                        rs.rateTypeId == rateTypeId == true &&
                                                        rs.effectiveDate <= date);

            // Apply exact filters for provided parameters
            if (officeId.HasValue)
            {
                query = query.Where(rs => rs.officeId == officeId.Value);
            }
            else
            {
                // When no office specified, exclude office-specific rates
                query = query.Where(rs => rs.officeId == null);
            }

            if (schedulingTargetId.HasValue)
            {
                query = query.Where(rs => rs.schedulingTargetId == schedulingTargetId.Value);
            }
            else
            {
                // Critical: When no target specified, exclude project-specific rates
                query = query.Where(rs => rs.schedulingTargetId == null);
            }

            if (resourceId.HasValue)
            {
                query = query.Where(rs => rs.resourceId == resourceId.Value);
            }
            else
            {
                // When no resource specified, exclude resource-specific rates
                query = query.Where(rs => rs.resourceId == null);
            }

            if (assignmentRoleId.HasValue)
            {
                query = query.Where(rs => rs.assignmentRoleId == assignmentRoleId.Value);
            }
            else
            {
                // When no role specified, exclude role-specific rates
                query = query.Where(rs => rs.assignmentRoleId == null);
            }

            // Order by most recent effective date (at the same specificity level)
            // Then by ID for stable tie-breaking
            query = query.OrderByDescending(rs => rs.effectiveDate)
                          .ThenByDescending(rs => rs.id);

            // Include navigation properties for friendly response
            query = query.Include(rs => rs.assignmentRole)
                         .Include(rs => rs.resource)
                         .Include(rs => rs.schedulingTarget)
                         .Include(rs => rs.currency)
                         .AsNoTracking();

            RateSheet matchedRate = await query.FirstOrDefaultAsync(cancellationToken);

            if (matchedRate == null)
            {
                await CreateAuditEventAsync(AuditType.Miscellaneous,
                    $"Rate preview: No matching rate found for Resource={resourceId}, Role={assignmentRoleId}, Target={schedulingTargetId}, RateType={rateTypeId}, Date={date:u}",
                    false);
                return Problem("No applicable rate found for the given criteria.");
            }

            // Build user-friendly scope description
            string scope = "Global";

            if (matchedRate.officeId.HasValue)
            {
                string officeName = matchedRate.office?.name ?? "Unknown Office";
                if (matchedRate.schedulingTargetId.HasValue)
                    scope = $"Project + Office: {matchedRate.schedulingTarget?.name} ({officeName})";
                else if (matchedRate.resourceId.HasValue)
                    scope = $"Resource + Office: {matchedRate.resource?.name} ({officeName})";
                else if (matchedRate.assignmentRoleId.HasValue)
                    scope = $"Role + Office: {matchedRate.assignmentRole?.name} ({officeName})";
                else
                    scope = $"Office Only: {officeName}";
            } else if (matchedRate.schedulingTargetId.HasValue)
            {
                if (matchedRate.resourceId.HasValue)
                    scope = $"Project + Resource: {matchedRate.schedulingTarget?.name}";
                else if (matchedRate.assignmentRoleId.HasValue)
                    scope = $"Project + Role: {matchedRate.schedulingTarget?.name} ({matchedRate.assignmentRole?.name})";
                else
                    scope = $"Project Only: {matchedRate.schedulingTarget?.name}";
            }
            else if (matchedRate.resourceId.HasValue)
            {
                scope = $"Resource: {matchedRate.resource?.name}";
            }
            else if (matchedRate.assignmentRoleId.HasValue)
            {
                scope = $"Role: {matchedRate.assignmentRole?.name}";
            }

            var result = new
            {
                matchedRate.id,
                matchedRate.costRate,
                matchedRate.billingRate,
                Currency = matchedRate.currency?.name ?? "Unknown",
                EffectiveDate = matchedRate.effectiveDate,
                Scope = scope
            };

            await CreateAuditEventAsync(AuditType.ReadEntity,
                                        $"Rate preview resolved: {scope} rate applied.",
                                        true);

            return Ok(result);
        }
    }
}