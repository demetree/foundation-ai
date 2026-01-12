using System;
using System.Threading;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Foundation.Security;
using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Security.Database;
using static Foundation.Auditor.AuditEngine;
using Foundation.Scheduler.Database;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the Gift entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the Gift entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class GiftsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object giftPutSyncRoot = new object();
		static object giftDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<GiftsController> _logger;

		public GiftsController(SchedulerContext context, ILogger<GiftsController> logger) : base("Scheduler", "Gift")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of Gifts filtered by the parameters provided.
		/// 
		/// There is a filter parameter for every field, and an 'anyStringContains' parameter for cross field string partial searches.
		/// 
		/// Note also the pagination control in the pageSize and pageNumber parameters.
		/// 
		/// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Gifts")]
		public async Task<IActionResult> GetGifts(
			int? officeId = null,
			int? constituentId = null,
			int? pledgeId = null,
			decimal? amount = null,
			DateTime? receivedDate = null,
			DateTime? postedDate = null,
			int? fundId = null,
			int? campaignId = null,
			int? appealId = null,
			int? paymentTypeId = null,
			string referenceNumber = null,
			int? batchId = null,
			int? receiptTypeId = null,
			DateTime? receiptDate = null,
			int? tributeId = null,
			string notes = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			int? pageSize = null,
			int? pageNumber = null,
			string anyStringContains = null,
			bool includeRelations = true,
			CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
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

			if (pageNumber.HasValue == true &&
			    pageNumber < 1)
			{
			    pageNumber = null;
			}

			if (pageSize.HasValue == true &&
			    pageSize <= 0)
			{
			    pageSize = null;
			}

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (receivedDate.HasValue == true && receivedDate.Value.Kind != DateTimeKind.Utc)
			{
				receivedDate = receivedDate.Value.ToUniversalTime();
			}

			if (postedDate.HasValue == true && postedDate.Value.Kind != DateTimeKind.Utc)
			{
				postedDate = postedDate.Value.ToUniversalTime();
			}

			if (receiptDate.HasValue == true && receiptDate.Value.Kind != DateTimeKind.Utc)
			{
				receiptDate = receiptDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Gift> query = (from g in _context.Gifts select g);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (officeId.HasValue == true)
			{
				query = query.Where(g => g.officeId == officeId.Value);
			}
			if (constituentId.HasValue == true)
			{
				query = query.Where(g => g.constituentId == constituentId.Value);
			}
			if (pledgeId.HasValue == true)
			{
				query = query.Where(g => g.pledgeId == pledgeId.Value);
			}
			if (amount.HasValue == true)
			{
				query = query.Where(g => g.amount == amount.Value);
			}
			if (receivedDate.HasValue == true)
			{
				query = query.Where(g => g.receivedDate == receivedDate.Value);
			}
			if (postedDate.HasValue == true)
			{
				query = query.Where(g => g.postedDate == postedDate.Value);
			}
			if (fundId.HasValue == true)
			{
				query = query.Where(g => g.fundId == fundId.Value);
			}
			if (campaignId.HasValue == true)
			{
				query = query.Where(g => g.campaignId == campaignId.Value);
			}
			if (appealId.HasValue == true)
			{
				query = query.Where(g => g.appealId == appealId.Value);
			}
			if (paymentTypeId.HasValue == true)
			{
				query = query.Where(g => g.paymentTypeId == paymentTypeId.Value);
			}
			if (string.IsNullOrEmpty(referenceNumber) == false)
			{
				query = query.Where(g => g.referenceNumber == referenceNumber);
			}
			if (batchId.HasValue == true)
			{
				query = query.Where(g => g.batchId == batchId.Value);
			}
			if (receiptTypeId.HasValue == true)
			{
				query = query.Where(g => g.receiptTypeId == receiptTypeId.Value);
			}
			if (receiptDate.HasValue == true)
			{
				query = query.Where(g => g.receiptDate == receiptDate.Value);
			}
			if (tributeId.HasValue == true)
			{
				query = query.Where(g => g.tributeId == tributeId.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(g => g.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(g => g.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(g => g.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(g => g.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(g => g.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(g => g.deleted == false);
				}
			}
			else
			{
				query = query.Where(g => g.active == true);
				query = query.Where(g => g.deleted == false);
			}

			query = query.OrderBy(g => g.referenceNumber);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.appeal);
				query = query.Include(x => x.batch);
				query = query.Include(x => x.campaign);
				query = query.Include(x => x.constituent);
				query = query.Include(x => x.fund);
				query = query.Include(x => x.office);
				query = query.Include(x => x.paymentType);
				query = query.Include(x => x.pledge);
				query = query.Include(x => x.receiptType);
				query = query.Include(x => x.tribute);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Gift, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.referenceNumber.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || (includeRelations == true && x.appeal.name.Contains(anyStringContains))
			       || (includeRelations == true && x.appeal.description.Contains(anyStringContains))
			       || (includeRelations == true && x.appeal.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.appeal.color.Contains(anyStringContains))
			       || (includeRelations == true && x.batch.batchNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.batch.description.Contains(anyStringContains))
			       || (includeRelations == true && x.campaign.name.Contains(anyStringContains))
			       || (includeRelations == true && x.campaign.description.Contains(anyStringContains))
			       || (includeRelations == true && x.campaign.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.campaign.color.Contains(anyStringContains))
			       || (includeRelations == true && x.constituent.constituentNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.constituent.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.constituent.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.constituent.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.constituent.color.Contains(anyStringContains))
			       || (includeRelations == true && x.constituent.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.constituent.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.fund.name.Contains(anyStringContains))
			       || (includeRelations == true && x.fund.description.Contains(anyStringContains))
			       || (includeRelations == true && x.fund.glCode.Contains(anyStringContains))
			       || (includeRelations == true && x.fund.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.fund.color.Contains(anyStringContains))
			       || (includeRelations == true && x.office.name.Contains(anyStringContains))
			       || (includeRelations == true && x.office.description.Contains(anyStringContains))
			       || (includeRelations == true && x.office.addressLine1.Contains(anyStringContains))
			       || (includeRelations == true && x.office.addressLine2.Contains(anyStringContains))
			       || (includeRelations == true && x.office.city.Contains(anyStringContains))
			       || (includeRelations == true && x.office.postalCode.Contains(anyStringContains))
			       || (includeRelations == true && x.office.phone.Contains(anyStringContains))
			       || (includeRelations == true && x.office.email.Contains(anyStringContains))
			       || (includeRelations == true && x.office.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.office.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.office.color.Contains(anyStringContains))
			       || (includeRelations == true && x.office.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.office.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.office.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.paymentType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.pledge.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.receiptType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.receiptType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.tribute.name.Contains(anyStringContains))
			       || (includeRelations == true && x.tribute.description.Contains(anyStringContains))
			       || (includeRelations == true && x.tribute.color.Contains(anyStringContains))
			       || (includeRelations == true && x.tribute.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.tribute.avatarMimeType.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.Gift> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.Gift gift in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(gift, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Gift Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Gift Entity list was read.  Returning " + materialized.Count + " rows of data.");

			// Create a new output object that only includes the relations if necessary, and doesn't include the empty list objects, so that we can reduce the amount of data being transferred.
			if (includeRelations == true)
			{
				// Return a DTO with nav properties.
				return Ok((from materializedData in materialized select materializedData.ToOutputDTO()).ToList());
			}
			else
			{
				// Return a DTO without nav properties.
				return Ok((from materializedData in materialized select materializedData.ToDTO()).ToList());
			}
		}
		
		
        /// <summary>
        /// 
        /// This returns a row count of Gifts filtered by the parameters provided.  Its query is similar to the GetGifts method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Gifts/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? officeId = null,
			int? constituentId = null,
			int? pledgeId = null,
			decimal? amount = null,
			DateTime? receivedDate = null,
			DateTime? postedDate = null,
			int? fundId = null,
			int? campaignId = null,
			int? appealId = null,
			int? paymentTypeId = null,
			string referenceNumber = null,
			int? batchId = null,
			int? receiptTypeId = null,
			DateTime? receiptDate = null,
			int? tributeId = null,
			string notes = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
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


			//
			// Fix any non-UTC date parameters that come in.
			//
			if (receivedDate.HasValue == true && receivedDate.Value.Kind != DateTimeKind.Utc)
			{
				receivedDate = receivedDate.Value.ToUniversalTime();
			}

			if (postedDate.HasValue == true && postedDate.Value.Kind != DateTimeKind.Utc)
			{
				postedDate = postedDate.Value.ToUniversalTime();
			}

			if (receiptDate.HasValue == true && receiptDate.Value.Kind != DateTimeKind.Utc)
			{
				receiptDate = receiptDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Gift> query = (from g in _context.Gifts select g);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (officeId.HasValue == true)
			{
				query = query.Where(g => g.officeId == officeId.Value);
			}
			if (constituentId.HasValue == true)
			{
				query = query.Where(g => g.constituentId == constituentId.Value);
			}
			if (pledgeId.HasValue == true)
			{
				query = query.Where(g => g.pledgeId == pledgeId.Value);
			}
			if (amount.HasValue == true)
			{
				query = query.Where(g => g.amount == amount.Value);
			}
			if (receivedDate.HasValue == true)
			{
				query = query.Where(g => g.receivedDate == receivedDate.Value);
			}
			if (postedDate.HasValue == true)
			{
				query = query.Where(g => g.postedDate == postedDate.Value);
			}
			if (fundId.HasValue == true)
			{
				query = query.Where(g => g.fundId == fundId.Value);
			}
			if (campaignId.HasValue == true)
			{
				query = query.Where(g => g.campaignId == campaignId.Value);
			}
			if (appealId.HasValue == true)
			{
				query = query.Where(g => g.appealId == appealId.Value);
			}
			if (paymentTypeId.HasValue == true)
			{
				query = query.Where(g => g.paymentTypeId == paymentTypeId.Value);
			}
			if (referenceNumber != null)
			{
				query = query.Where(g => g.referenceNumber == referenceNumber);
			}
			if (batchId.HasValue == true)
			{
				query = query.Where(g => g.batchId == batchId.Value);
			}
			if (receiptTypeId.HasValue == true)
			{
				query = query.Where(g => g.receiptTypeId == receiptTypeId.Value);
			}
			if (receiptDate.HasValue == true)
			{
				query = query.Where(g => g.receiptDate == receiptDate.Value);
			}
			if (tributeId.HasValue == true)
			{
				query = query.Where(g => g.tributeId == tributeId.Value);
			}
			if (notes != null)
			{
				query = query.Where(g => g.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(g => g.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(g => g.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(g => g.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(g => g.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(g => g.deleted == false);
				}
			}
			else
			{
				query = query.Where(g => g.active == true);
				query = query.Where(g => g.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Gift, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.referenceNumber.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.appeal.name.Contains(anyStringContains)
			       || x.appeal.description.Contains(anyStringContains)
			       || x.appeal.notes.Contains(anyStringContains)
			       || x.appeal.color.Contains(anyStringContains)
			       || x.batch.batchNumber.Contains(anyStringContains)
			       || x.batch.description.Contains(anyStringContains)
			       || x.campaign.name.Contains(anyStringContains)
			       || x.campaign.description.Contains(anyStringContains)
			       || x.campaign.notes.Contains(anyStringContains)
			       || x.campaign.color.Contains(anyStringContains)
			       || x.constituent.constituentNumber.Contains(anyStringContains)
			       || x.constituent.externalId.Contains(anyStringContains)
			       || x.constituent.notes.Contains(anyStringContains)
			       || x.constituent.attributes.Contains(anyStringContains)
			       || x.constituent.color.Contains(anyStringContains)
			       || x.constituent.avatarFileName.Contains(anyStringContains)
			       || x.constituent.avatarMimeType.Contains(anyStringContains)
			       || x.fund.name.Contains(anyStringContains)
			       || x.fund.description.Contains(anyStringContains)
			       || x.fund.glCode.Contains(anyStringContains)
			       || x.fund.notes.Contains(anyStringContains)
			       || x.fund.color.Contains(anyStringContains)
			       || x.office.name.Contains(anyStringContains)
			       || x.office.description.Contains(anyStringContains)
			       || x.office.addressLine1.Contains(anyStringContains)
			       || x.office.addressLine2.Contains(anyStringContains)
			       || x.office.city.Contains(anyStringContains)
			       || x.office.postalCode.Contains(anyStringContains)
			       || x.office.phone.Contains(anyStringContains)
			       || x.office.email.Contains(anyStringContains)
			       || x.office.notes.Contains(anyStringContains)
			       || x.office.externalId.Contains(anyStringContains)
			       || x.office.color.Contains(anyStringContains)
			       || x.office.attributes.Contains(anyStringContains)
			       || x.office.avatarFileName.Contains(anyStringContains)
			       || x.office.avatarMimeType.Contains(anyStringContains)
			       || x.paymentType.name.Contains(anyStringContains)
			       || x.paymentType.description.Contains(anyStringContains)
			       || x.pledge.notes.Contains(anyStringContains)
			       || x.receiptType.name.Contains(anyStringContains)
			       || x.receiptType.description.Contains(anyStringContains)
			       || x.tribute.name.Contains(anyStringContains)
			       || x.tribute.description.Contains(anyStringContains)
			       || x.tribute.color.Contains(anyStringContains)
			       || x.tribute.avatarFileName.Contains(anyStringContains)
			       || x.tribute.avatarMimeType.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single Gift by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Gift/{id}")]
		public async Task<IActionResult> GetGift(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
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


			try
			{
				IQueryable<Database.Gift> query = (from g in _context.Gifts where
							(g.id == id) &&
							(userIsAdmin == true || g.deleted == false) &&
							(userIsWriter == true || g.active == true)
					select g);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.appeal);
					query = query.Include(x => x.batch);
					query = query.Include(x => x.campaign);
					query = query.Include(x => x.constituent);
					query = query.Include(x => x.fund);
					query = query.Include(x => x.office);
					query = query.Include(x => x.paymentType);
					query = query.Include(x => x.pledge);
					query = query.Include(x => x.receiptType);
					query = query.Include(x => x.tribute);
					query = query.AsSplitQuery();
				}

				Database.Gift materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Gift Entity was read with Admin privilege." : "Scheduler.Gift Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Gift", materialized.id, materialized.referenceNumber));


					// Create a new output object that only includes the relations if necessary, and doesn't include the empty list objects, so that we can reduce the amount of data being transferred.
					if (includeRelations == true)
					{
						return Ok(materialized.ToOutputDTO());             // DTO with nav properties
					}
					else
					{
						return Ok(materialized.ToDTO());                   // DTO without nav properties
					}
				}
				else
				{
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Gift entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Gift.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Gift.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
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
		public async Task<IActionResult> PutGift(int id, [FromBody]Database.Gift.GiftDTO giftDTO, CancellationToken cancellationToken = default)
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

			}
		}
*/

/* This function is expected to be overridden in a custom file
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
		public async Task<IActionResult> PostGift([FromBody]Database.Gift.GiftDTO giftDTO, CancellationToken cancellationToken = default)
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
						gift. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.Gift.CreateAnonymousWithFirstLevelSubObjects(gift)),
						null);


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

*/

/* This function is expected to be overridden in a custom file

        /// <summary>
        /// 
        /// This rolls a Gift entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Gift/Rollback/{id}")]
		[Route("api/Gift/Rollback")]
		public async Task<IActionResult> RollbackToGiftVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
		{
			//
			// Data rollback is an admin only function, like Deletes.
			//
			StartAuditEventClock();
			
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			

			
			IQueryable <Database.Gift> query = (from x in _context.Gifts
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this Gift concurrently
			//
			lock (giftPutSyncRoot)
			{
				
				Database.Gift gift = query.FirstOrDefault();
				
				if (gift == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Gift rollback", id.ToString(), new Exception("No Scheduler.Gift entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the Gift current state so we can log it.
				//
				Database.Gift cloneOfExisting = (Database.Gift)_context.Entry(gift).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.GiftChangeHistories = null;
				cloneOfExisting.SoftCredits = null;
				cloneOfExisting.appeal = null;
				cloneOfExisting.batch = null;
				cloneOfExisting.campaign = null;
				cloneOfExisting.constituent = null;
				cloneOfExisting.fund = null;
				cloneOfExisting.office = null;
				cloneOfExisting.paymentType = null;
				cloneOfExisting.pledge = null;
				cloneOfExisting.receiptType = null;
				cloneOfExisting.tribute = null;

				if (versionNumber >= gift.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.Gift rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.Gift rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				GiftChangeHistory giftChangeHistory = (from x in _context.GiftChangeHistories
				                                               where
				                                               x.giftId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (giftChangeHistory != null)
				{
				    Database.Gift oldGift = JsonSerializer.Deserialize<Database.Gift>(giftChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    gift.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    gift.officeId = oldGift.officeId;
				    gift.constituentId = oldGift.constituentId;
				    gift.pledgeId = oldGift.pledgeId;
				    gift.amount = oldGift.amount;
				    gift.receivedDate = oldGift.receivedDate;
				    gift.postedDate = oldGift.postedDate;
				    gift.fundId = oldGift.fundId;
				    gift.campaignId = oldGift.campaignId;
				    gift.appealId = oldGift.appealId;
				    gift.paymentTypeId = oldGift.paymentTypeId;
				    gift.referenceNumber = oldGift.referenceNumber;
				    gift.batchId = oldGift.batchId;
				    gift.receiptTypeId = oldGift.receiptTypeId;
				    gift.receiptDate = oldGift.receiptDate;
				    gift.tributeId = oldGift.tributeId;
				    gift.notes = oldGift.notes;
				    gift.objectGuid = oldGift.objectGuid;
				    gift.active = oldGift.active;
				    gift.deleted = oldGift.deleted;

				    string serializedGift = JsonSerializer.Serialize(gift);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        GiftChangeHistory newGiftChangeHistory = new GiftChangeHistory();
				        newGiftChangeHistory.giftId = gift.id;
				        newGiftChangeHistory.versionNumber = gift.versionNumber;
				        newGiftChangeHistory.timeStamp = DateTime.UtcNow;
				        newGiftChangeHistory.userId = securityUser.id;
				        newGiftChangeHistory.tenantGuid = userTenantGuid;
				        newGiftChangeHistory.data = JsonSerializer.Serialize(Database.Gift.CreateAnonymousWithFirstLevelSubObjects(gift));
				        _context.GiftChangeHistories.Add(newGiftChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.Gift rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Gift.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Gift.CreateAnonymousWithFirstLevelSubObjects(gift)),
						null);


				    return Ok(Database.Gift.CreateAnonymous(gift));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.Gift rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.Gift rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a Gift record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Gift/{id}")]
		[Route("api/Gift")]
		public async Task<IActionResult> DeleteGift(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(cancellationToken);
			
			
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

			Database.Gift gift = await query.FirstOrDefaultAsync(cancellationToken);

			if (gift == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.Gift DELETE", id.ToString(), new Exception("No Scheduler.Gift entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.Gift cloneOfExisting = (Database.Gift)_context.Entry(gift).GetDatabaseValues().ToObject();


			lock (giftDeleteSyncRoot)
			{
			    try
			    {
			        gift.deleted = true;
			        gift.versionNumber++;

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

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Gift entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.Gift.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Gift.CreateAnonymousWithFirstLevelSubObjects(gift)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.Gift entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.Gift.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.Gift.CreateAnonymousWithFirstLevelSubObjects(gift)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


*/
        /// <summary>
        /// 
        /// This gets a list of Gift records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/Gifts/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? officeId = null,
			int? constituentId = null,
			int? pledgeId = null,
			decimal? amount = null,
			DateTime? receivedDate = null,
			DateTime? postedDate = null,
			int? fundId = null,
			int? campaignId = null,
			int? appealId = null,
			int? paymentTypeId = null,
			string referenceNumber = null,
			int? batchId = null,
			int? receiptTypeId = null,
			DateTime? receiptDate = null,
			int? tributeId = null,
			string notes = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);

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


			if (pageNumber.HasValue == true &&
			    pageNumber < 1)
			{
			    pageNumber = null;
			}

			if (pageSize.HasValue == true &&
			    pageSize <= 0)
			{
			    pageSize = null;
			}

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (receivedDate.HasValue == true && receivedDate.Value.Kind != DateTimeKind.Utc)
			{
				receivedDate = receivedDate.Value.ToUniversalTime();
			}

			if (postedDate.HasValue == true && postedDate.Value.Kind != DateTimeKind.Utc)
			{
				postedDate = postedDate.Value.ToUniversalTime();
			}

			if (receiptDate.HasValue == true && receiptDate.Value.Kind != DateTimeKind.Utc)
			{
				receiptDate = receiptDate.Value.ToUniversalTime();
			}

			IQueryable<Database.Gift> query = (from g in _context.Gifts select g);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (officeId.HasValue == true)
			{
				query = query.Where(g => g.officeId == officeId.Value);
			}
			if (constituentId.HasValue == true)
			{
				query = query.Where(g => g.constituentId == constituentId.Value);
			}
			if (pledgeId.HasValue == true)
			{
				query = query.Where(g => g.pledgeId == pledgeId.Value);
			}
			if (amount.HasValue == true)
			{
				query = query.Where(g => g.amount == amount.Value);
			}
			if (receivedDate.HasValue == true)
			{
				query = query.Where(g => g.receivedDate == receivedDate.Value);
			}
			if (postedDate.HasValue == true)
			{
				query = query.Where(g => g.postedDate == postedDate.Value);
			}
			if (fundId.HasValue == true)
			{
				query = query.Where(g => g.fundId == fundId.Value);
			}
			if (campaignId.HasValue == true)
			{
				query = query.Where(g => g.campaignId == campaignId.Value);
			}
			if (appealId.HasValue == true)
			{
				query = query.Where(g => g.appealId == appealId.Value);
			}
			if (paymentTypeId.HasValue == true)
			{
				query = query.Where(g => g.paymentTypeId == paymentTypeId.Value);
			}
			if (string.IsNullOrEmpty(referenceNumber) == false)
			{
				query = query.Where(g => g.referenceNumber == referenceNumber);
			}
			if (batchId.HasValue == true)
			{
				query = query.Where(g => g.batchId == batchId.Value);
			}
			if (receiptTypeId.HasValue == true)
			{
				query = query.Where(g => g.receiptTypeId == receiptTypeId.Value);
			}
			if (receiptDate.HasValue == true)
			{
				query = query.Where(g => g.receiptDate == receiptDate.Value);
			}
			if (tributeId.HasValue == true)
			{
				query = query.Where(g => g.tributeId == tributeId.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(g => g.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(g => g.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(g => g.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(g => g.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(g => g.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(g => g.deleted == false);
				}
			}
			else
			{
				query = query.Where(g => g.active == true);
				query = query.Where(g => g.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Gift, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.referenceNumber.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			       || x.appeal.name.Contains(anyStringContains)
			       || x.appeal.description.Contains(anyStringContains)
			       || x.appeal.notes.Contains(anyStringContains)
			       || x.appeal.color.Contains(anyStringContains)
			       || x.batch.batchNumber.Contains(anyStringContains)
			       || x.batch.description.Contains(anyStringContains)
			       || x.campaign.name.Contains(anyStringContains)
			       || x.campaign.description.Contains(anyStringContains)
			       || x.campaign.notes.Contains(anyStringContains)
			       || x.campaign.color.Contains(anyStringContains)
			       || x.constituent.constituentNumber.Contains(anyStringContains)
			       || x.constituent.externalId.Contains(anyStringContains)
			       || x.constituent.notes.Contains(anyStringContains)
			       || x.constituent.attributes.Contains(anyStringContains)
			       || x.constituent.color.Contains(anyStringContains)
			       || x.constituent.avatarFileName.Contains(anyStringContains)
			       || x.constituent.avatarMimeType.Contains(anyStringContains)
			       || x.fund.name.Contains(anyStringContains)
			       || x.fund.description.Contains(anyStringContains)
			       || x.fund.glCode.Contains(anyStringContains)
			       || x.fund.notes.Contains(anyStringContains)
			       || x.fund.color.Contains(anyStringContains)
			       || x.office.name.Contains(anyStringContains)
			       || x.office.description.Contains(anyStringContains)
			       || x.office.addressLine1.Contains(anyStringContains)
			       || x.office.addressLine2.Contains(anyStringContains)
			       || x.office.city.Contains(anyStringContains)
			       || x.office.postalCode.Contains(anyStringContains)
			       || x.office.phone.Contains(anyStringContains)
			       || x.office.email.Contains(anyStringContains)
			       || x.office.notes.Contains(anyStringContains)
			       || x.office.externalId.Contains(anyStringContains)
			       || x.office.color.Contains(anyStringContains)
			       || x.office.attributes.Contains(anyStringContains)
			       || x.office.avatarFileName.Contains(anyStringContains)
			       || x.office.avatarMimeType.Contains(anyStringContains)
			       || x.paymentType.name.Contains(anyStringContains)
			       || x.paymentType.description.Contains(anyStringContains)
			       || x.pledge.notes.Contains(anyStringContains)
			       || x.receiptType.name.Contains(anyStringContains)
			       || x.receiptType.description.Contains(anyStringContains)
			       || x.tribute.name.Contains(anyStringContains)
			       || x.tribute.description.Contains(anyStringContains)
			       || x.tribute.color.Contains(anyStringContains)
			       || x.tribute.avatarFileName.Contains(anyStringContains)
			       || x.tribute.avatarMimeType.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.referenceNumber);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.Gift.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
		}


        /// <summary>
        /// 
        /// This method creates an audit event from within the controller.  It is intended for use by custom logic in client applications that needs to create audit events.
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/Gift/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
			{
			   return Forbid();
			}

		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
