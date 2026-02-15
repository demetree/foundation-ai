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
using Foundation.ChangeHistory;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the RateSheet entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the RateSheet entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class RateSheetsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 30;

		static object rateSheetPutSyncRoot = new object();
		static object rateSheetDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<RateSheetsController> _logger;

		public RateSheetsController(SchedulerContext context, ILogger<RateSheetsController> logger) : base("Scheduler", "RateSheet")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of RateSheets filtered by the parameters provided.
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
		[Route("api/RateSheets")]
		public async Task<IActionResult> GetRateSheets(
			int? officeId = null,
			int? assignmentRoleId = null,
			int? resourceId = null,
			int? schedulingTargetId = null,
			int? rateTypeId = null,
			DateTime? effectiveDate = null,
			int? currencyId = null,
			decimal? costRate = null,
			decimal? billingRate = null,
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

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 30, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

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
			if (effectiveDate.HasValue == true && effectiveDate.Value.Kind != DateTimeKind.Utc)
			{
				effectiveDate = effectiveDate.Value.ToUniversalTime();
			}

			IQueryable<Database.RateSheet> query = (from rs in _context.RateSheets select rs);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (officeId.HasValue == true)
			{
				query = query.Where(rs => rs.officeId == officeId.Value);
			}
			if (assignmentRoleId.HasValue == true)
			{
				query = query.Where(rs => rs.assignmentRoleId == assignmentRoleId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(rs => rs.resourceId == resourceId.Value);
			}
			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(rs => rs.schedulingTargetId == schedulingTargetId.Value);
			}
			if (rateTypeId.HasValue == true)
			{
				query = query.Where(rs => rs.rateTypeId == rateTypeId.Value);
			}
			if (effectiveDate.HasValue == true)
			{
				query = query.Where(rs => rs.effectiveDate == effectiveDate.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(rs => rs.currencyId == currencyId.Value);
			}
			if (costRate.HasValue == true)
			{
				query = query.Where(rs => rs.costRate == costRate.Value);
			}
			if (billingRate.HasValue == true)
			{
				query = query.Where(rs => rs.billingRate == billingRate.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(rs => rs.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rs => rs.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rs => rs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rs => rs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rs => rs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rs => rs.deleted == false);
				}
			}
			else
			{
				query = query.Where(rs => rs.active == true);
				query = query.Where(rs => rs.deleted == false);
			}

			query = query.OrderBy(rs => rs.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.assignmentRole);
				query = query.Include(x => x.currency);
				query = query.Include(x => x.office);
				query = query.Include(x => x.rateType);
				query = query.Include(x => x.resource);
				query = query.Include(x => x.schedulingTarget);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Rate Sheet, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || (includeRelations == true && x.assignmentRole.name.Contains(anyStringContains))
			       || (includeRelations == true && x.assignmentRole.description.Contains(anyStringContains))
			       || (includeRelations == true && x.assignmentRole.color.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.name.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.description.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.code.Contains(anyStringContains))
			       || (includeRelations == true && x.currency.color.Contains(anyStringContains))
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
			       || (includeRelations == true && x.rateType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.rateType.description.Contains(anyStringContains))
			       || (includeRelations == true && x.rateType.color.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.name.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.description.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.color.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.resource.avatarMimeType.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.name.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.description.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.color.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.attributes.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.avatarFileName.Contains(anyStringContains))
			       || (includeRelations == true && x.schedulingTarget.avatarMimeType.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.RateSheet> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.RateSheet rateSheet in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(rateSheet, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.RateSheet Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.RateSheet Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of RateSheets filtered by the parameters provided.  Its query is similar to the GetRateSheets method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RateSheets/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? officeId = null,
			int? assignmentRoleId = null,
			int? resourceId = null,
			int? schedulingTargetId = null,
			int? rateTypeId = null,
			DateTime? effectiveDate = null,
			int? currencyId = null,
			decimal? costRate = null,
			decimal? billingRate = null,
			string notes = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 30, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
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
			if (effectiveDate.HasValue == true && effectiveDate.Value.Kind != DateTimeKind.Utc)
			{
				effectiveDate = effectiveDate.Value.ToUniversalTime();
			}

			IQueryable<Database.RateSheet> query = (from rs in _context.RateSheets select rs);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (officeId.HasValue == true)
			{
				query = query.Where(rs => rs.officeId == officeId.Value);
			}
			if (assignmentRoleId.HasValue == true)
			{
				query = query.Where(rs => rs.assignmentRoleId == assignmentRoleId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(rs => rs.resourceId == resourceId.Value);
			}
			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(rs => rs.schedulingTargetId == schedulingTargetId.Value);
			}
			if (rateTypeId.HasValue == true)
			{
				query = query.Where(rs => rs.rateTypeId == rateTypeId.Value);
			}
			if (effectiveDate.HasValue == true)
			{
				query = query.Where(rs => rs.effectiveDate == effectiveDate.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(rs => rs.currencyId == currencyId.Value);
			}
			if (costRate.HasValue == true)
			{
				query = query.Where(rs => rs.costRate == costRate.Value);
			}
			if (billingRate.HasValue == true)
			{
				query = query.Where(rs => rs.billingRate == billingRate.Value);
			}
			if (notes != null)
			{
				query = query.Where(rs => rs.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rs => rs.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rs => rs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rs => rs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rs => rs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rs => rs.deleted == false);
				}
			}
			else
			{
				query = query.Where(rs => rs.active == true);
				query = query.Where(rs => rs.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Rate Sheet, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || x.assignmentRole.name.Contains(anyStringContains)
			       || x.assignmentRole.description.Contains(anyStringContains)
			       || x.assignmentRole.color.Contains(anyStringContains)
			       || x.currency.name.Contains(anyStringContains)
			       || x.currency.description.Contains(anyStringContains)
			       || x.currency.code.Contains(anyStringContains)
			       || x.currency.color.Contains(anyStringContains)
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
			       || x.rateType.name.Contains(anyStringContains)
			       || x.rateType.description.Contains(anyStringContains)
			       || x.rateType.color.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
			       || x.schedulingTarget.name.Contains(anyStringContains)
			       || x.schedulingTarget.description.Contains(anyStringContains)
			       || x.schedulingTarget.notes.Contains(anyStringContains)
			       || x.schedulingTarget.externalId.Contains(anyStringContains)
			       || x.schedulingTarget.color.Contains(anyStringContains)
			       || x.schedulingTarget.attributes.Contains(anyStringContains)
			       || x.schedulingTarget.avatarFileName.Contains(anyStringContains)
			       || x.schedulingTarget.avatarMimeType.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single RateSheet by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RateSheet/{id}")]
		public async Task<IActionResult> GetRateSheet(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 30, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			
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
				IQueryable<Database.RateSheet> query = (from rs in _context.RateSheets where
							(rs.id == id) &&
							(userIsAdmin == true || rs.deleted == false) &&
							(userIsWriter == true || rs.active == true)
					select rs);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.assignmentRole);
					query = query.Include(x => x.currency);
					query = query.Include(x => x.office);
					query = query.Include(x => x.rateType);
					query = query.Include(x => x.resource);
					query = query.Include(x => x.schedulingTarget);
					query = query.AsSplitQuery();
				}

				Database.RateSheet materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.RateSheet Entity was read with Admin privilege." : "Scheduler.RateSheet Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "RateSheet", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.RateSheet entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.RateSheet.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.RateSheet.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
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
		public async Task<IActionResult> PutRateSheet(int id, [FromBody]Database.RateSheet.RateSheetDTO rateSheetDTO, CancellationToken cancellationToken = default)
		{
			if (rateSheetDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Resource Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Resource Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != rateSheetDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 30, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
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


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (rateSheetDTO.objectGuid == Guid.Empty)
            {
                rateSheetDTO.objectGuid = existing.objectGuid;
            }
            else if (rateSheetDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a RateSheet record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
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

			lock (rateSheetPutSyncRoot)
			{
				//
				// Validate the version number for the rateSheet being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != rateSheet.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "RateSheet save attempt was made but save request was with version " + rateSheet.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The RateSheet you are trying to update has already changed.  Please try your save again after reloading the RateSheet.");
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
*/

/* This function is expected to be overridden in a custom file
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
		public async Task<IActionResult> PostRateSheet([FromBody]Database.RateSheet.RateSheetDTO rateSheetDTO, CancellationToken cancellationToken = default)
		{
			if (rateSheetDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Resource Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Resource Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
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
					rateSheet.office = null;
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
						rateSheet. id.ToString(),
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

*/

/* This function is expected to be overridden in a custom file

        /// <summary>
        /// 
        /// This rolls a RateSheet entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RateSheet/Rollback/{id}")]
		[Route("api/RateSheet/Rollback")]
		public async Task<IActionResult> RollbackToRateSheetVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.RateSheet> query = (from x in _context.RateSheets
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this RateSheet concurrently
			//
			lock (rateSheetPutSyncRoot)
			{
				
				Database.RateSheet rateSheet = query.FirstOrDefault();
				
				if (rateSheet == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.RateSheet rollback", id.ToString(), new Exception("No Scheduler.RateSheet entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the RateSheet current state so we can log it.
				//
				Database.RateSheet cloneOfExisting = (Database.RateSheet)_context.Entry(rateSheet).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.RateSheetChangeHistories = null;
				cloneOfExisting.assignmentRole = null;
				cloneOfExisting.currency = null;
				cloneOfExisting.office = null;
				cloneOfExisting.rateType = null;
				cloneOfExisting.resource = null;
				cloneOfExisting.schedulingTarget = null;

				if (versionNumber >= rateSheet.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.RateSheet rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.RateSheet rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				RateSheetChangeHistory rateSheetChangeHistory = (from x in _context.RateSheetChangeHistories
				                                               where
				                                               x.rateSheetId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (rateSheetChangeHistory != null)
				{
				    Database.RateSheet oldRateSheet = JsonSerializer.Deserialize<Database.RateSheet>(rateSheetChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    rateSheet.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    rateSheet.officeId = oldRateSheet.officeId;
				    rateSheet.assignmentRoleId = oldRateSheet.assignmentRoleId;
				    rateSheet.resourceId = oldRateSheet.resourceId;
				    rateSheet.schedulingTargetId = oldRateSheet.schedulingTargetId;
				    rateSheet.rateTypeId = oldRateSheet.rateTypeId;
				    rateSheet.effectiveDate = oldRateSheet.effectiveDate;
				    rateSheet.currencyId = oldRateSheet.currencyId;
				    rateSheet.costRate = oldRateSheet.costRate;
				    rateSheet.billingRate = oldRateSheet.billingRate;
				    rateSheet.notes = oldRateSheet.notes;
				    rateSheet.objectGuid = oldRateSheet.objectGuid;
				    rateSheet.active = oldRateSheet.active;
				    rateSheet.deleted = oldRateSheet.deleted;

				    string serializedRateSheet = JsonSerializer.Serialize(rateSheet);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        RateSheetChangeHistory newRateSheetChangeHistory = new RateSheetChangeHistory();
				        newRateSheetChangeHistory.rateSheetId = rateSheet.id;
				        newRateSheetChangeHistory.versionNumber = rateSheet.versionNumber;
				        newRateSheetChangeHistory.timeStamp = DateTime.UtcNow;
				        newRateSheetChangeHistory.userId = securityUser.id;
				        newRateSheetChangeHistory.tenantGuid = userTenantGuid;
				        newRateSheetChangeHistory.data = JsonSerializer.Serialize(Database.RateSheet.CreateAnonymousWithFirstLevelSubObjects(rateSheet));
				        _context.RateSheetChangeHistories.Add(newRateSheetChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.RateSheet rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.RateSheet.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.RateSheet.CreateAnonymousWithFirstLevelSubObjects(rateSheet)),
						null);


				    return Ok(Database.RateSheet.CreateAnonymous(rateSheet));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.RateSheet rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.RateSheet rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}

*/



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a RateSheet.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the RateSheet</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RateSheet/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetRateSheetChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

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


			Database.RateSheet rateSheet = await _context.RateSheets.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (rateSheet == null)
			{
				return NotFound();
			}

			try
			{
				rateSheet.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.RateSheet> versionInfo = await rateSheet.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null)
				{
					return NotFound($"Version {versionNumber} not found.");
				}

				return Ok(versionInfo);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets the full audit history for a RateSheet.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the RateSheet</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RateSheet/{id}/AuditHistory")]
		public async Task<IActionResult> GetRateSheetAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

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


			Database.RateSheet rateSheet = await _context.RateSheets.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (rateSheet == null)
			{
				return NotFound();
			}

			try
			{
				rateSheet.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.RateSheet>> versions = await rateSheet.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a RateSheet.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the RateSheet</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The RateSheet object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RateSheet/{id}/Version/{version}")]
		public async Task<IActionResult> GetRateSheetVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

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


			Database.RateSheet rateSheet = await _context.RateSheets.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (rateSheet == null)
			{
				return NotFound();
			}

			try
			{
				rateSheet.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.RateSheet> versionInfo = await rateSheet.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null || versionInfo.data == null)
				{
					return NotFound();
				}

				return Ok(versionInfo.data.ToOutputDTO());
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets the state of a RateSheet at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the RateSheet</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The RateSheet object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RateSheet/{id}/StateAtTime")]
		public async Task<IActionResult> GetRateSheetStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

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


			Database.RateSheet rateSheet = await _context.RateSheets.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (rateSheet == null)
			{
				return NotFound();
			}

			try
			{
				rateSheet.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.RateSheet> versionInfo = await rateSheet.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

				if (versionInfo == null || versionInfo.data == null)
				{
					return NotFound("No state found at specified time.");
				}

				return Ok(versionInfo.data.ToOutputDTO());
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a RateSheet record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/RateSheet/{id}")]
		[Route("api/RateSheet")]
		public async Task<IActionResult> DeleteRateSheet(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Resource Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Resource Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			
			
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

			Database.RateSheet rateSheet = await query.FirstOrDefaultAsync(cancellationToken);

			if (rateSheet == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.RateSheet DELETE", id.ToString(), new Exception("No Scheduler.RateSheet entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.RateSheet cloneOfExisting = (Database.RateSheet)_context.Entry(rateSheet).GetDatabaseValues().ToObject();


			lock (rateSheetDeleteSyncRoot)
			{
			    try
			    {
			        rateSheet.deleted = true;
			        rateSheet.versionNumber++;

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

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.RateSheet entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.RateSheet.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.RateSheet.CreateAnonymousWithFirstLevelSubObjects(rateSheet)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.RateSheet entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.RateSheet.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.RateSheet.CreateAnonymousWithFirstLevelSubObjects(rateSheet)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


*/
        /// <summary>
        /// 
        /// This gets a list of RateSheet records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/RateSheets/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? officeId = null,
			int? assignmentRoleId = null,
			int? resourceId = null,
			int? schedulingTargetId = null,
			int? rateTypeId = null,
			DateTime? effectiveDate = null,
			int? currencyId = null,
			decimal? costRate = null,
			decimal? billingRate = null,
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
			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 30, cancellationToken);


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
			if (effectiveDate.HasValue == true && effectiveDate.Value.Kind != DateTimeKind.Utc)
			{
				effectiveDate = effectiveDate.Value.ToUniversalTime();
			}

			IQueryable<Database.RateSheet> query = (from rs in _context.RateSheets select rs);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (officeId.HasValue == true)
			{
				query = query.Where(rs => rs.officeId == officeId.Value);
			}
			if (assignmentRoleId.HasValue == true)
			{
				query = query.Where(rs => rs.assignmentRoleId == assignmentRoleId.Value);
			}
			if (resourceId.HasValue == true)
			{
				query = query.Where(rs => rs.resourceId == resourceId.Value);
			}
			if (schedulingTargetId.HasValue == true)
			{
				query = query.Where(rs => rs.schedulingTargetId == schedulingTargetId.Value);
			}
			if (rateTypeId.HasValue == true)
			{
				query = query.Where(rs => rs.rateTypeId == rateTypeId.Value);
			}
			if (effectiveDate.HasValue == true)
			{
				query = query.Where(rs => rs.effectiveDate == effectiveDate.Value);
			}
			if (currencyId.HasValue == true)
			{
				query = query.Where(rs => rs.currencyId == currencyId.Value);
			}
			if (costRate.HasValue == true)
			{
				query = query.Where(rs => rs.costRate == costRate.Value);
			}
			if (billingRate.HasValue == true)
			{
				query = query.Where(rs => rs.billingRate == billingRate.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(rs => rs.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(rs => rs.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(rs => rs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(rs => rs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(rs => rs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(rs => rs.deleted == false);
				}
			}
			else
			{
				query = query.Where(rs => rs.active == true);
				query = query.Where(rs => rs.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Rate Sheet, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.notes.Contains(anyStringContains)
			       || x.assignmentRole.name.Contains(anyStringContains)
			       || x.assignmentRole.description.Contains(anyStringContains)
			       || x.assignmentRole.color.Contains(anyStringContains)
			       || x.currency.name.Contains(anyStringContains)
			       || x.currency.description.Contains(anyStringContains)
			       || x.currency.code.Contains(anyStringContains)
			       || x.currency.color.Contains(anyStringContains)
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
			       || x.rateType.name.Contains(anyStringContains)
			       || x.rateType.description.Contains(anyStringContains)
			       || x.rateType.color.Contains(anyStringContains)
			       || x.resource.name.Contains(anyStringContains)
			       || x.resource.description.Contains(anyStringContains)
			       || x.resource.notes.Contains(anyStringContains)
			       || x.resource.externalId.Contains(anyStringContains)
			       || x.resource.color.Contains(anyStringContains)
			       || x.resource.attributes.Contains(anyStringContains)
			       || x.resource.avatarFileName.Contains(anyStringContains)
			       || x.resource.avatarMimeType.Contains(anyStringContains)
			       || x.schedulingTarget.name.Contains(anyStringContains)
			       || x.schedulingTarget.description.Contains(anyStringContains)
			       || x.schedulingTarget.notes.Contains(anyStringContains)
			       || x.schedulingTarget.externalId.Contains(anyStringContains)
			       || x.schedulingTarget.color.Contains(anyStringContains)
			       || x.schedulingTarget.attributes.Contains(anyStringContains)
			       || x.schedulingTarget.avatarFileName.Contains(anyStringContains)
			       || x.schedulingTarget.avatarMimeType.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.RateSheet.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/RateSheet/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Resource Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Resource Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
