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
    /// This auto generated class provides the basic CRUD operations for the PaymentProvider entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the PaymentProvider entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PaymentProvidersController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object paymentProviderPutSyncRoot = new object();
		static object paymentProviderDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<PaymentProvidersController> _logger;

		public PaymentProvidersController(SchedulerContext context, ILogger<PaymentProvidersController> logger) : base("Scheduler", "PaymentProvider")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of PaymentProviders filtered by the parameters provided.
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
		[Route("api/PaymentProviders")]
		public async Task<IActionResult> GetPaymentProviders(
			string name = null,
			string description = null,
			string providerType = null,
			bool? isActive = null,
			string apiKeyEncrypted = null,
			string merchantId = null,
			string webhookSecret = null,
			decimal? processingFeePercent = null,
			decimal? processingFeeFixed = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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

			IQueryable<Database.PaymentProvider> query = (from pp in _context.PaymentProviders select pp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pp => pp.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(pp => pp.description == description);
			}
			if (string.IsNullOrEmpty(providerType) == false)
			{
				query = query.Where(pp => pp.providerType == providerType);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(pp => pp.isActive == isActive.Value);
			}
			if (string.IsNullOrEmpty(apiKeyEncrypted) == false)
			{
				query = query.Where(pp => pp.apiKeyEncrypted == apiKeyEncrypted);
			}
			if (string.IsNullOrEmpty(merchantId) == false)
			{
				query = query.Where(pp => pp.merchantId == merchantId);
			}
			if (string.IsNullOrEmpty(webhookSecret) == false)
			{
				query = query.Where(pp => pp.webhookSecret == webhookSecret);
			}
			if (processingFeePercent.HasValue == true)
			{
				query = query.Where(pp => pp.processingFeePercent == processingFeePercent.Value);
			}
			if (processingFeeFixed.HasValue == true)
			{
				query = query.Where(pp => pp.processingFeeFixed == processingFeeFixed.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(pp => pp.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(pp => pp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pp => pp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pp => pp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pp => pp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pp => pp.deleted == false);
				}
			}
			else
			{
				query = query.Where(pp => pp.active == true);
				query = query.Where(pp => pp.deleted == false);
			}

			query = query.OrderBy(pp => pp.name).ThenBy(pp => pp.description).ThenBy(pp => pp.providerType);


			//
			// Add the any string contains parameter to span all the string fields on the Payment Provider, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.providerType.Contains(anyStringContains)
			       || x.apiKeyEncrypted.Contains(anyStringContains)
			       || x.merchantId.Contains(anyStringContains)
			       || x.webhookSecret.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			   );
			}

			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.PaymentProvider> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.PaymentProvider paymentProvider in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(paymentProvider, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.PaymentProvider Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.PaymentProvider Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of PaymentProviders filtered by the parameters provided.  Its query is similar to the GetPaymentProviders method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentProviders/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string providerType = null,
			bool? isActive = null,
			string apiKeyEncrypted = null,
			string merchantId = null,
			string webhookSecret = null,
			decimal? processingFeePercent = null,
			decimal? processingFeeFixed = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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


			IQueryable<Database.PaymentProvider> query = (from pp in _context.PaymentProviders select pp);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(pp => pp.name == name);
			}
			if (description != null)
			{
				query = query.Where(pp => pp.description == description);
			}
			if (providerType != null)
			{
				query = query.Where(pp => pp.providerType == providerType);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(pp => pp.isActive == isActive.Value);
			}
			if (apiKeyEncrypted != null)
			{
				query = query.Where(pp => pp.apiKeyEncrypted == apiKeyEncrypted);
			}
			if (merchantId != null)
			{
				query = query.Where(pp => pp.merchantId == merchantId);
			}
			if (webhookSecret != null)
			{
				query = query.Where(pp => pp.webhookSecret == webhookSecret);
			}
			if (processingFeePercent.HasValue == true)
			{
				query = query.Where(pp => pp.processingFeePercent == processingFeePercent.Value);
			}
			if (processingFeeFixed.HasValue == true)
			{
				query = query.Where(pp => pp.processingFeeFixed == processingFeeFixed.Value);
			}
			if (notes != null)
			{
				query = query.Where(pp => pp.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(pp => pp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pp => pp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pp => pp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pp => pp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pp => pp.deleted == false);
				}
			}
			else
			{
				query = query.Where(pp => pp.active == true);
				query = query.Where(pp => pp.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Payment Provider, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.providerType.Contains(anyStringContains)
			       || x.apiKeyEncrypted.Contains(anyStringContains)
			       || x.merchantId.Contains(anyStringContains)
			       || x.webhookSecret.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single PaymentProvider by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentProvider/{id}")]
		public async Task<IActionResult> GetPaymentProvider(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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
				IQueryable<Database.PaymentProvider> query = (from pp in _context.PaymentProviders where
							(pp.id == id) &&
							(userIsAdmin == true || pp.deleted == false) &&
							(userIsWriter == true || pp.active == true)
					select pp);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.PaymentProvider materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.PaymentProvider Entity was read with Admin privilege." : "Scheduler.PaymentProvider Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PaymentProvider", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.PaymentProvider entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.PaymentProvider.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.PaymentProvider.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing PaymentProvider record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/PaymentProvider/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPaymentProvider(int id, [FromBody]Database.PaymentProvider.PaymentProviderDTO paymentProviderDTO, CancellationToken cancellationToken = default)
		{
			if (paymentProviderDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != paymentProviderDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
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


			IQueryable<Database.PaymentProvider> query = (from x in _context.PaymentProviders
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.PaymentProvider existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PaymentProvider PUT", id.ToString(), new Exception("No Scheduler.PaymentProvider entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (paymentProviderDTO.objectGuid == Guid.Empty)
            {
                paymentProviderDTO.objectGuid = existing.objectGuid;
            }
            else if (paymentProviderDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a PaymentProvider record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.PaymentProvider cloneOfExisting = (Database.PaymentProvider)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new PaymentProvider object using the data from the existing record, updated with what is in the DTO.
			//
			Database.PaymentProvider paymentProvider = (Database.PaymentProvider)_context.Entry(existing).GetDatabaseValues().ToObject();
			paymentProvider.ApplyDTO(paymentProviderDTO);
			//
			// The tenant guid for any PaymentProvider being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the PaymentProvider because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				paymentProvider.tenantGuid = existing.tenantGuid;
			}

			lock (paymentProviderPutSyncRoot)
			{
				//
				// Validate the version number for the paymentProvider being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != paymentProvider.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "PaymentProvider save attempt was made but save request was with version " + paymentProvider.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The PaymentProvider you are trying to update has already changed.  Please try your save again after reloading the PaymentProvider.");
				}
				else
				{
					// Same record.  Increase version.
					paymentProvider.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (paymentProvider.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.PaymentProvider record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (paymentProvider.name != null && paymentProvider.name.Length > 100)
				{
					paymentProvider.name = paymentProvider.name.Substring(0, 100);
				}

				if (paymentProvider.description != null && paymentProvider.description.Length > 500)
				{
					paymentProvider.description = paymentProvider.description.Substring(0, 500);
				}

				if (paymentProvider.providerType != null && paymentProvider.providerType.Length > 50)
				{
					paymentProvider.providerType = paymentProvider.providerType.Substring(0, 50);
				}

				if (paymentProvider.merchantId != null && paymentProvider.merchantId.Length > 100)
				{
					paymentProvider.merchantId = paymentProvider.merchantId.Substring(0, 100);
				}

				try
				{
				    EntityEntry<Database.PaymentProvider> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(paymentProvider);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        PaymentProviderChangeHistory paymentProviderChangeHistory = new PaymentProviderChangeHistory();
				        paymentProviderChangeHistory.paymentProviderId = paymentProvider.id;
				        paymentProviderChangeHistory.versionNumber = paymentProvider.versionNumber;
				        paymentProviderChangeHistory.timeStamp = DateTime.UtcNow;
				        paymentProviderChangeHistory.userId = securityUser.id;
				        paymentProviderChangeHistory.tenantGuid = userTenantGuid;
				        paymentProviderChangeHistory.data = JsonSerializer.Serialize(Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(paymentProvider));
				        _context.PaymentProviderChangeHistories.Add(paymentProviderChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.PaymentProvider entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(paymentProvider)),
						null);

				return Ok(Database.PaymentProvider.CreateAnonymous(paymentProvider));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.PaymentProvider entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(paymentProvider)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new PaymentProvider record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentProvider", Name = "PaymentProvider")]
		public async Task<IActionResult> PostPaymentProvider([FromBody]Database.PaymentProvider.PaymentProviderDTO paymentProviderDTO, CancellationToken cancellationToken = default)
		{
			if (paymentProviderDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new PaymentProvider object using the data from the DTO
			//
			Database.PaymentProvider paymentProvider = Database.PaymentProvider.FromDTO(paymentProviderDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				paymentProvider.tenantGuid = userTenantGuid;

				if (paymentProvider.name != null && paymentProvider.name.Length > 100)
				{
					paymentProvider.name = paymentProvider.name.Substring(0, 100);
				}

				if (paymentProvider.description != null && paymentProvider.description.Length > 500)
				{
					paymentProvider.description = paymentProvider.description.Substring(0, 500);
				}

				if (paymentProvider.providerType != null && paymentProvider.providerType.Length > 50)
				{
					paymentProvider.providerType = paymentProvider.providerType.Substring(0, 50);
				}

				if (paymentProvider.merchantId != null && paymentProvider.merchantId.Length > 100)
				{
					paymentProvider.merchantId = paymentProvider.merchantId.Substring(0, 100);
				}

				paymentProvider.objectGuid = Guid.NewGuid();
				paymentProvider.versionNumber = 1;

				_context.PaymentProviders.Add(paymentProvider);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the paymentProvider object so that no further changes will be written to the database
				    //
				    _context.Entry(paymentProvider).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					paymentProvider.PaymentProviderChangeHistories = null;
					paymentProvider.PaymentTransactions = null;


				    PaymentProviderChangeHistory paymentProviderChangeHistory = new PaymentProviderChangeHistory();
				    paymentProviderChangeHistory.paymentProviderId = paymentProvider.id;
				    paymentProviderChangeHistory.versionNumber = paymentProvider.versionNumber;
				    paymentProviderChangeHistory.timeStamp = DateTime.UtcNow;
				    paymentProviderChangeHistory.userId = securityUser.id;
				    paymentProviderChangeHistory.tenantGuid = userTenantGuid;
				    paymentProviderChangeHistory.data = JsonSerializer.Serialize(Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(paymentProvider));
				    _context.PaymentProviderChangeHistories.Add(paymentProviderChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.PaymentProvider entity successfully created.",
						true,
						paymentProvider. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(paymentProvider)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.PaymentProvider entity creation failed.", false, paymentProvider.id.ToString(), "", JsonSerializer.Serialize(Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(paymentProvider)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PaymentProvider", paymentProvider.id, paymentProvider.name));

			return CreatedAtRoute("PaymentProvider", new { id = paymentProvider.id }, Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(paymentProvider));
		}



        /// <summary>
        /// 
        /// This rolls a PaymentProvider entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentProvider/Rollback/{id}")]
		[Route("api/PaymentProvider/Rollback")]
		public async Task<IActionResult> RollbackToPaymentProviderVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.PaymentProvider> query = (from x in _context.PaymentProviders
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this PaymentProvider concurrently
			//
			lock (paymentProviderPutSyncRoot)
			{
				
				Database.PaymentProvider paymentProvider = query.FirstOrDefault();
				
				if (paymentProvider == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PaymentProvider rollback", id.ToString(), new Exception("No Scheduler.PaymentProvider entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the PaymentProvider current state so we can log it.
				//
				Database.PaymentProvider cloneOfExisting = (Database.PaymentProvider)_context.Entry(paymentProvider).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.PaymentProviderChangeHistories = null;
				cloneOfExisting.PaymentTransactions = null;

				if (versionNumber >= paymentProvider.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.PaymentProvider rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.PaymentProvider rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				PaymentProviderChangeHistory paymentProviderChangeHistory = (from x in _context.PaymentProviderChangeHistories
				                                               where
				                                               x.paymentProviderId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (paymentProviderChangeHistory != null)
				{
				    Database.PaymentProvider oldPaymentProvider = JsonSerializer.Deserialize<Database.PaymentProvider>(paymentProviderChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    paymentProvider.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    paymentProvider.name = oldPaymentProvider.name;
				    paymentProvider.description = oldPaymentProvider.description;
				    paymentProvider.providerType = oldPaymentProvider.providerType;
				    paymentProvider.isActive = oldPaymentProvider.isActive;
				    paymentProvider.apiKeyEncrypted = oldPaymentProvider.apiKeyEncrypted;
				    paymentProvider.merchantId = oldPaymentProvider.merchantId;
				    paymentProvider.webhookSecret = oldPaymentProvider.webhookSecret;
				    paymentProvider.processingFeePercent = oldPaymentProvider.processingFeePercent;
				    paymentProvider.processingFeeFixed = oldPaymentProvider.processingFeeFixed;
				    paymentProvider.notes = oldPaymentProvider.notes;
				    paymentProvider.objectGuid = oldPaymentProvider.objectGuid;
				    paymentProvider.active = oldPaymentProvider.active;
				    paymentProvider.deleted = oldPaymentProvider.deleted;

				    string serializedPaymentProvider = JsonSerializer.Serialize(paymentProvider);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        PaymentProviderChangeHistory newPaymentProviderChangeHistory = new PaymentProviderChangeHistory();
				        newPaymentProviderChangeHistory.paymentProviderId = paymentProvider.id;
				        newPaymentProviderChangeHistory.versionNumber = paymentProvider.versionNumber;
				        newPaymentProviderChangeHistory.timeStamp = DateTime.UtcNow;
				        newPaymentProviderChangeHistory.userId = securityUser.id;
				        newPaymentProviderChangeHistory.tenantGuid = userTenantGuid;
				        newPaymentProviderChangeHistory.data = JsonSerializer.Serialize(Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(paymentProvider));
				        _context.PaymentProviderChangeHistories.Add(newPaymentProviderChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.PaymentProvider rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(paymentProvider)),
						null);


				    return Ok(Database.PaymentProvider.CreateAnonymous(paymentProvider));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.PaymentProvider rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.PaymentProvider rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a PaymentProvider.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PaymentProvider</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentProvider/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetPaymentProviderChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.PaymentProvider paymentProvider = await _context.PaymentProviders.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (paymentProvider == null)
			{
				return NotFound();
			}

			try
			{
				paymentProvider.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.PaymentProvider> versionInfo = await paymentProvider.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a PaymentProvider.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PaymentProvider</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentProvider/{id}/AuditHistory")]
		public async Task<IActionResult> GetPaymentProviderAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.PaymentProvider paymentProvider = await _context.PaymentProviders.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (paymentProvider == null)
			{
				return NotFound();
			}

			try
			{
				paymentProvider.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.PaymentProvider>> versions = await paymentProvider.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a PaymentProvider.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PaymentProvider</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The PaymentProvider object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentProvider/{id}/Version/{version}")]
		public async Task<IActionResult> GetPaymentProviderVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.PaymentProvider paymentProvider = await _context.PaymentProviders.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (paymentProvider == null)
			{
				return NotFound();
			}

			try
			{
				paymentProvider.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.PaymentProvider> versionInfo = await paymentProvider.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a PaymentProvider at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the PaymentProvider</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The PaymentProvider object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentProvider/{id}/StateAtTime")]
		public async Task<IActionResult> GetPaymentProviderStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.PaymentProvider paymentProvider = await _context.PaymentProviders.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (paymentProvider == null)
			{
				return NotFound();
			}

			try
			{
				paymentProvider.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.PaymentProvider> versionInfo = await paymentProvider.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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

        /// <summary>
        /// 
        /// This deletes a PaymentProvider record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentProvider/{id}")]
		[Route("api/PaymentProvider")]
		public async Task<IActionResult> DeletePaymentProvider(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.PaymentProvider> query = (from x in _context.PaymentProviders
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.PaymentProvider paymentProvider = await query.FirstOrDefaultAsync(cancellationToken);

			if (paymentProvider == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PaymentProvider DELETE", id.ToString(), new Exception("No Scheduler.PaymentProvider entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.PaymentProvider cloneOfExisting = (Database.PaymentProvider)_context.Entry(paymentProvider).GetDatabaseValues().ToObject();


			lock (paymentProviderDeleteSyncRoot)
			{
			    try
			    {
			        paymentProvider.deleted = true;
			        paymentProvider.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        PaymentProviderChangeHistory paymentProviderChangeHistory = new PaymentProviderChangeHistory();
			        paymentProviderChangeHistory.paymentProviderId = paymentProvider.id;
			        paymentProviderChangeHistory.versionNumber = paymentProvider.versionNumber;
			        paymentProviderChangeHistory.timeStamp = DateTime.UtcNow;
			        paymentProviderChangeHistory.userId = securityUser.id;
			        paymentProviderChangeHistory.tenantGuid = userTenantGuid;
			        paymentProviderChangeHistory.data = JsonSerializer.Serialize(Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(paymentProvider));
			        _context.PaymentProviderChangeHistories.Add(paymentProviderChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.PaymentProvider entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(paymentProvider)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.PaymentProvider entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.PaymentProvider.CreateAnonymousWithFirstLevelSubObjects(paymentProvider)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of PaymentProvider records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/PaymentProviders/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string providerType = null,
			bool? isActive = null,
			string apiKeyEncrypted = null,
			string merchantId = null,
			string webhookSecret = null,
			decimal? processingFeePercent = null,
			decimal? processingFeeFixed = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);


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

			IQueryable<Database.PaymentProvider> query = (from pp in _context.PaymentProviders select pp);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pp => pp.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(pp => pp.description == description);
			}
			if (string.IsNullOrEmpty(providerType) == false)
			{
				query = query.Where(pp => pp.providerType == providerType);
			}
			if (isActive.HasValue == true)
			{
				query = query.Where(pp => pp.isActive == isActive.Value);
			}
			if (string.IsNullOrEmpty(apiKeyEncrypted) == false)
			{
				query = query.Where(pp => pp.apiKeyEncrypted == apiKeyEncrypted);
			}
			if (string.IsNullOrEmpty(merchantId) == false)
			{
				query = query.Where(pp => pp.merchantId == merchantId);
			}
			if (string.IsNullOrEmpty(webhookSecret) == false)
			{
				query = query.Where(pp => pp.webhookSecret == webhookSecret);
			}
			if (processingFeePercent.HasValue == true)
			{
				query = query.Where(pp => pp.processingFeePercent == processingFeePercent.Value);
			}
			if (processingFeeFixed.HasValue == true)
			{
				query = query.Where(pp => pp.processingFeeFixed == processingFeeFixed.Value);
			}
			if (string.IsNullOrEmpty(notes) == false)
			{
				query = query.Where(pp => pp.notes == notes);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(pp => pp.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pp => pp.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pp => pp.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pp => pp.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pp => pp.deleted == false);
				}
			}
			else
			{
				query = query.Where(pp => pp.active == true);
				query = query.Where(pp => pp.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Payment Provider, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.providerType.Contains(anyStringContains)
			       || x.apiKeyEncrypted.Contains(anyStringContains)
			       || x.merchantId.Contains(anyStringContains)
			       || x.webhookSecret.Contains(anyStringContains)
			       || x.notes.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.providerType);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.PaymentProvider.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/PaymentProvider/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Config Writer role needed to write to this table, or Scheduler Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Scheduler Config Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
