using Foundation;
using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Scheduler.Database;
using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    public partial class GiftsController
    {
        /// <summary>
        /// 
        /// This updates an existing Gift record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        [Route("api/Gift/{id}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpPost]
        [HttpPut]
        public async Task<IActionResult> PutGift(int id, [FromBody] Database.Gift.GiftDTO giftDTO, [FromServices] Foundation.Scheduler.Services.DonorJourneyCalculator journeyCalculator, CancellationToken cancellationToken = default)
        {
            if (giftDTO == null)
            {
                return BadRequest();
            }

            StartAuditEventClock();

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }



            if (id != giftDTO.id)
            {
                return BadRequest();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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


            IQueryable<Database.Gift> query = (from x in _context.Gifts
                                               where
                                               (x.id == id)
                                               select x);

            query = query.Where(x => x.tenantGuid == userTenantGuid);

            Database.Gift existing = await query.FirstOrDefaultAsync(cancellationToken);

            if (existing == null)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Gift PUT", id.ToString(), new Exception("No Scheduler.Gift entity could be found with the primary key provided."));
                return NotFound();
            }


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (giftDTO.objectGuid == Guid.Empty)
            {
                giftDTO.objectGuid = existing.objectGuid;
            }
            else if (giftDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a Gift record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


            // Copy the existing object so it can be serialized as-is in the audit and history logs.
            Database.Gift cloneOfExisting = (Database.Gift)_context.Entry(existing).GetDatabaseValues().ToObject();

            //
            // Create a new Gift object using the data from the existing record, updated with what is in the DTO.
            //
            Database.Gift gift = (Database.Gift)_context.Entry(existing).GetDatabaseValues().ToObject();
            gift.ApplyDTO(giftDTO);
            //
            // The tenant guid for any Gift being saved must match the tenant guid of the user.  
            //
            if (existing.tenantGuid != userTenantGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
                return Problem("Data integrity violation detected while attempting to save.");
            }
            else
            {
                // Assign the tenantGuid to the Gift because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
                gift.tenantGuid = existing.tenantGuid;
            }

            lock (giftPutSyncRoot)
            {
                //
                // Validate the version number for the gift being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
                //
                if (existing.versionNumber != gift.versionNumber)
                {
                    // Record has changed
                    CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "Gift save attempt was made but save request was with version " + gift.versionNumber + " and the current version number is " + existing.versionNumber, false);
                    return Problem("The Gift you are trying to update has already changed.  Please try your save again after reloading the Gift.");
                }
                else
                {
                    // Same record.  Increase version.
                    gift.versionNumber++;
                }


                // Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
                if (userIsAdmin == false && (gift.deleted == true || existing.deleted == true))
                {
                    // we're not recording state here because it is not being changed.
                    CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.Gift record.", id.ToString());
                    DestroySessionAndAuthentication();
                    return Forbid();
                }

                if (gift.receivedDate.Kind != DateTimeKind.Utc)
                {
                    gift.receivedDate = gift.receivedDate.ToUniversalTime();
                }

                if (gift.postedDate.HasValue == true && gift.postedDate.Value.Kind != DateTimeKind.Utc)
                {
                    gift.postedDate = gift.postedDate.Value.ToUniversalTime();
                }

                if (gift.referenceNumber != null && gift.referenceNumber.Length > 100)
                {
                    gift.referenceNumber = gift.referenceNumber.Substring(0, 100);
                }

                if (gift.receiptDate.HasValue == true && gift.receiptDate.Value.Kind != DateTimeKind.Utc)
                {
                    gift.receiptDate = gift.receiptDate.Value.ToUniversalTime();
                }

                try
                {
                    EntityEntry<Database.Gift> attached = _context.Entry(existing);
                    attached.CurrentValues.SetValues(gift);

                    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                    {
                        _context.SaveChanges();

                        //
                        // Now add the change history
                        //
                        GiftChangeHistory giftChangeHistory = new GiftChangeHistory();
                        giftChangeHistory.giftId = gift.id;
                        giftChangeHistory.versionNumber = gift.versionNumber;
                        giftChangeHistory.timeStamp = DateTime.UtcNow;
                        giftChangeHistory.userId = securityUser.id;
                        giftChangeHistory.tenantGuid = userTenantGuid;
                        giftChangeHistory.data = JsonSerializer.Serialize(Database.Gift.CreateAnonymousWithFirstLevelSubObjects(gift));
                        _context.GiftChangeHistories.Add(giftChangeHistory);

                        _context.SaveChanges();

                        transaction.Commit();
                    }

                    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
                        "Scheduler.Gift entity successfully updated.",
                        true,
                        id.ToString(),
                        JsonSerializer.Serialize(Database.Gift.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                        JsonSerializer.Serialize(Database.Gift.CreateAnonymousWithFirstLevelSubObjects(gift)),
                        null);

                    return Ok(Database.Gift.CreateAnonymous(gift));
                }
                catch (Exception ex)
                {
                    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
                        "Scheduler.Gift entity update failed",
                        false,
                        id.ToString(),
                        JsonSerializer.Serialize(Database.Gift.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
                        JsonSerializer.Serialize(Database.Gift.CreateAnonymousWithFirstLevelSubObjects(gift)),
                        ex);

                    return Problem(ex.Message);
                }
                finally
                {
                    // Trigger Donor Journey Calculation
                    try
                    {
                        var constituent = _context.Constituents.Find(gift.constituentId);
                        if (constituent != null)
                        {
                            var stage = journeyCalculator.CalculateStage(constituent);
                            if (stage != null && constituent.constituentJourneyStageId != stage.id)
                            {
                                constituent.constituentJourneyStageId = stage.id;
                                constituent.dateEnteredCurrentStage = DateTime.UtcNow;
                                _context.SaveChanges();
                            }
                        }
                    }
                    catch (Exception calcEx)
                    {
                        // Log error but don't fail the request
                        _logger.LogCritical(calcEx, "Error calculating donor journey stage for constituent {ConstituentId}", gift.constituentId);
                    }
                }

            }
        }

        /// <summary>
        /// 
        /// This creates a new Gift record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Gift", Name = "Gift")]
        public async Task<IActionResult> PostGift([FromBody] Database.Gift.GiftDTO giftDTO, [FromServices] Foundation.Scheduler.Services.DonorJourneyCalculator journeyCalculator, CancellationToken cancellationToken = default)
        {
            if (giftDTO == null)
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
            // Create a new Gift object using the data from the DTO
            //
            Database.Gift gift = Database.Gift.FromDTO(giftDTO);

            try
            {
                //
                // Ensure that the tenant data is correct.
                //
                gift.tenantGuid = userTenantGuid;

                if (gift.receivedDate.Kind != DateTimeKind.Utc)
                {
                    gift.receivedDate = gift.receivedDate.ToUniversalTime();
                }

                if (gift.postedDate.HasValue == true && gift.postedDate.Value.Kind != DateTimeKind.Utc)
                {
                    gift.postedDate = gift.postedDate.Value.ToUniversalTime();
                }

                if (gift.referenceNumber != null && gift.referenceNumber.Length > 100)
                {
                    gift.referenceNumber = gift.referenceNumber.Substring(0, 100);
                }

                if (gift.receiptDate.HasValue == true && gift.receiptDate.Value.Kind != DateTimeKind.Utc)
                {
                    gift.receiptDate = gift.receiptDate.Value.ToUniversalTime();
                }

                gift.objectGuid = Guid.NewGuid();
                gift.versionNumber = 1;

                _context.Gifts.Add(gift);

                await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
                {
                    await _context.SaveChangesAsync(cancellationToken);

                    //
                    // Now add the change history
                    //

                    //
                    // Detach the gift object so that no further changes will be written to the database
                    //
                    _context.Entry(gift).State = EntityState.Detached;

                    //
                    // Nullify all object properties before serializing.
                    //
                    gift.GiftChangeHistories = null;
                    gift.SoftCredits = null;
                    gift.appeal = null;
                    gift.batch = null;
                    gift.campaign = null;
                    gift.constituent = null;
                    gift.fund = null;
                    gift.office = null;
                    gift.paymentType = null;
                    gift.pledge = null;
                    gift.receiptType = null;
                    gift.tribute = null;


                    GiftChangeHistory giftChangeHistory = new GiftChangeHistory();
                    giftChangeHistory.giftId = gift.id;
                    giftChangeHistory.versionNumber = gift.versionNumber;
                    giftChangeHistory.timeStamp = DateTime.UtcNow;
                    giftChangeHistory.userId = securityUser.id;
                    giftChangeHistory.tenantGuid = userTenantGuid;
                    giftChangeHistory.data = JsonSerializer.Serialize(Database.Gift.CreateAnonymousWithFirstLevelSubObjects(gift));
                    _context.GiftChangeHistories.Add(giftChangeHistory);
                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                        "Scheduler.Gift entity successfully created.",
                        true,
                        gift.id.ToString(),
                        "",
                        JsonSerializer.Serialize(Database.Gift.CreateAnonymousWithFirstLevelSubObjects(gift)),
                        null);


                }

                // Trigger Donor Journey Calculation
                try
                {
                    var constituent = await _context.Constituents.FindAsync(gift.constituentId);
                    if (constituent != null)
                    {
                        var stage = await journeyCalculator.CalculateStageAsync(constituent);
                        if (stage != null && constituent.constituentJourneyStageId != stage.id)
                        {
                            constituent.constituentJourneyStageId = stage.id;
                            constituent.dateEnteredCurrentStage = DateTime.UtcNow;
                            await _context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                catch (Exception calcEx)
                {
                    // Log error but don't fail the request
                    _logger.LogError(calcEx, "Error calculating donor journey stage for constituent {ConstituentId}", gift.constituentId);
                }

            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.Gift entity creation failed.", false, gift.id.ToString(), "", JsonSerializer.Serialize(Database.Gift.CreateAnonymousWithFirstLevelSubObjects(gift)), ex);

                return Problem(ex.Message);
            }



            BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Gift", gift.id, gift.referenceNumber));

            return CreatedAtRoute("Gift", new { id = gift.id }, Database.Gift.CreateAnonymousWithFirstLevelSubObjects(gift));
        }


        /// <summary>
        ///
        /// Records a gift through the FinancialManagementService, which handles:
        ///   - Gift creation inside a DB transaction
        ///   - FinancialTransaction ledger entry
        ///   - Pledge reconciliation (if pledgeId provided)
        ///   - Fiscal period validation
        ///   - Change history
        ///
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/Gifts/RecordGift")]
        public async Task<IActionResult> RecordGiftAsync(
            [FromBody] RecordGiftRequest request,
            CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid tenantGuid;

            try
            {
                tenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception ex)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error,
                    "Attempt to record gift by user with no tenant. User: " + securityUser?.accountName,
                    securityUser?.accountName, ex);
                return Problem("Your user account is not configured with a tenant.");
            }

            var financialService = HttpContext.RequestServices
                .GetService(typeof(Foundation.Scheduler.Services.FinancialManagementService))
                as Foundation.Scheduler.Services.FinancialManagementService;

            if (financialService == null)
            {
                return Problem("Financial Management Service is not available.");
            }

            var result = await financialService.RecordGiftAsync(
                tenantGuid,
                request.ConstituentId,
                request.Amount,
                request.FundId,
                request.PaymentTypeId,
                request.ReceivedDate,
                request.PledgeId,
                request.CampaignId,
                request.AppealId,
                request.OfficeId,
                request.BatchId,
                request.ReferenceNumber,
                request.Notes,
                cancellationToken);

            if (!result.Success)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Miscellaneous,
                    $"Gift recording failed: {result.ErrorMessage}");
                return BadRequest(result.ErrorMessage);
            }

            await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
                $"Gift of {request.Amount:C} recorded for constituent {request.ConstituentId} via FinancialManagementService.");

            return Ok(new { success = true, message = "Gift recorded successfully.", data = result.Data });
        }


        public class RecordGiftRequest
        {
            public int ConstituentId { get; set; }
            public decimal Amount { get; set; }
            public int FundId { get; set; }
            public int PaymentTypeId { get; set; }
            public DateTime ReceivedDate { get; set; }
            public int? PledgeId { get; set; }
            public int? CampaignId { get; set; }
            public int? AppealId { get; set; }
            public int? OfficeId { get; set; }
            public int? BatchId { get; set; }
            public string ReferenceNumber { get; set; }
            public string Notes { get; set; }
        }
    }
}
