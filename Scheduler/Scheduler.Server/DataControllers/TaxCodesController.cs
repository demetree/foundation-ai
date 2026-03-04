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
    /// This auto generated class provides the basic CRUD operations for the TaxCode entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the TaxCode entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class TaxCodesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<TaxCodesController> _logger;

		public TaxCodesController(SchedulerContext context, ILogger<TaxCodesController> logger) : base("Scheduler", "TaxCode")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of TaxCodes filtered by the parameters provided.
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
		[Route("api/TaxCodes")]
		public async Task<IActionResult> GetTaxCodes(
			string name = null,
			string description = null,
			string code = null,
			decimal? rate = null,
			bool? isDefault = null,
			bool? isExempt = null,
			string externalTaxCodeId = null,
			int? sequence = null,
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

			IQueryable<Database.TaxCode> query = (from tc in _context.TaxCodes select tc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(tc => tc.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(tc => tc.description == description);
			}
			if (string.IsNullOrEmpty(code) == false)
			{
				query = query.Where(tc => tc.code == code);
			}
			if (rate.HasValue == true)
			{
				query = query.Where(tc => tc.rate == rate.Value);
			}
			if (isDefault.HasValue == true)
			{
				query = query.Where(tc => tc.isDefault == isDefault.Value);
			}
			if (isExempt.HasValue == true)
			{
				query = query.Where(tc => tc.isExempt == isExempt.Value);
			}
			if (string.IsNullOrEmpty(externalTaxCodeId) == false)
			{
				query = query.Where(tc => tc.externalTaxCodeId == externalTaxCodeId);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(tc => tc.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(tc => tc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(tc => tc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(tc => tc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(tc => tc.deleted == false);
				}
			}
			else
			{
				query = query.Where(tc => tc.active == true);
				query = query.Where(tc => tc.deleted == false);
			}

			query = query.OrderBy(tc => tc.sequence).ThenBy(tc => tc.name).ThenBy(tc => tc.description).ThenBy(tc => tc.code);


			//
			// Add the any string contains parameter to span all the string fields on the Tax Code, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.code.Contains(anyStringContains)
			       || x.externalTaxCodeId.Contains(anyStringContains)
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
			
			List<Database.TaxCode> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.TaxCode taxCode in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(taxCode, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.TaxCode Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.TaxCode Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of TaxCodes filtered by the parameters provided.  Its query is similar to the GetTaxCodes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TaxCodes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string code = null,
			decimal? rate = null,
			bool? isDefault = null,
			bool? isExempt = null,
			string externalTaxCodeId = null,
			int? sequence = null,
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


			IQueryable<Database.TaxCode> query = (from tc in _context.TaxCodes select tc);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (name != null)
			{
				query = query.Where(tc => tc.name == name);
			}
			if (description != null)
			{
				query = query.Where(tc => tc.description == description);
			}
			if (code != null)
			{
				query = query.Where(tc => tc.code == code);
			}
			if (rate.HasValue == true)
			{
				query = query.Where(tc => tc.rate == rate.Value);
			}
			if (isDefault.HasValue == true)
			{
				query = query.Where(tc => tc.isDefault == isDefault.Value);
			}
			if (isExempt.HasValue == true)
			{
				query = query.Where(tc => tc.isExempt == isExempt.Value);
			}
			if (externalTaxCodeId != null)
			{
				query = query.Where(tc => tc.externalTaxCodeId == externalTaxCodeId);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(tc => tc.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(tc => tc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(tc => tc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(tc => tc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(tc => tc.deleted == false);
				}
			}
			else
			{
				query = query.Where(tc => tc.active == true);
				query = query.Where(tc => tc.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Tax Code, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.code.Contains(anyStringContains)
			       || x.externalTaxCodeId.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single TaxCode by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TaxCode/{id}")]
		public async Task<IActionResult> GetTaxCode(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.TaxCode> query = (from tc in _context.TaxCodes where
							(tc.id == id) &&
							(userIsAdmin == true || tc.deleted == false) &&
							(userIsWriter == true || tc.active == true)
					select tc);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.TaxCode materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.TaxCode Entity was read with Admin privilege." : "Scheduler.TaxCode Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TaxCode", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.TaxCode entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.TaxCode.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.TaxCode.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing TaxCode record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/TaxCode/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutTaxCode(int id, [FromBody]Database.TaxCode.TaxCodeDTO taxCodeDTO, CancellationToken cancellationToken = default)
		{
			if (taxCodeDTO == null)
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



			if (id != taxCodeDTO.id)
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


			IQueryable<Database.TaxCode> query = (from x in _context.TaxCodes
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.TaxCode existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.TaxCode PUT", id.ToString(), new Exception("No Scheduler.TaxCode entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (taxCodeDTO.objectGuid == Guid.Empty)
            {
                taxCodeDTO.objectGuid = existing.objectGuid;
            }
            else if (taxCodeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a TaxCode record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.TaxCode cloneOfExisting = (Database.TaxCode)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new TaxCode object using the data from the existing record, updated with what is in the DTO.
			//
			Database.TaxCode taxCode = (Database.TaxCode)_context.Entry(existing).GetDatabaseValues().ToObject();
			taxCode.ApplyDTO(taxCodeDTO);
			//
			// The tenant guid for any TaxCode being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the TaxCode because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				taxCode.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (taxCode.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.TaxCode record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (taxCode.name != null && taxCode.name.Length > 100)
			{
				taxCode.name = taxCode.name.Substring(0, 100);
			}

			if (taxCode.description != null && taxCode.description.Length > 500)
			{
				taxCode.description = taxCode.description.Substring(0, 500);
			}

			if (taxCode.code != null && taxCode.code.Length > 50)
			{
				taxCode.code = taxCode.code.Substring(0, 50);
			}

			if (taxCode.externalTaxCodeId != null && taxCode.externalTaxCodeId.Length > 250)
			{
				taxCode.externalTaxCodeId = taxCode.externalTaxCodeId.Substring(0, 250);
			}

			EntityEntry<Database.TaxCode> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(taxCode);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.TaxCode entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TaxCode.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TaxCode.CreateAnonymousWithFirstLevelSubObjects(taxCode)),
					null);


				return Ok(Database.TaxCode.CreateAnonymous(taxCode));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.TaxCode entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TaxCode.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TaxCode.CreateAnonymousWithFirstLevelSubObjects(taxCode)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new TaxCode record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TaxCode", Name = "TaxCode")]
		public async Task<IActionResult> PostTaxCode([FromBody]Database.TaxCode.TaxCodeDTO taxCodeDTO, CancellationToken cancellationToken = default)
		{
			if (taxCodeDTO == null)
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
			// Create a new TaxCode object using the data from the DTO
			//
			Database.TaxCode taxCode = Database.TaxCode.FromDTO(taxCodeDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				taxCode.tenantGuid = userTenantGuid;

				if (taxCode.name != null && taxCode.name.Length > 100)
				{
					taxCode.name = taxCode.name.Substring(0, 100);
				}

				if (taxCode.description != null && taxCode.description.Length > 500)
				{
					taxCode.description = taxCode.description.Substring(0, 500);
				}

				if (taxCode.code != null && taxCode.code.Length > 50)
				{
					taxCode.code = taxCode.code.Substring(0, 50);
				}

				if (taxCode.externalTaxCodeId != null && taxCode.externalTaxCodeId.Length > 250)
				{
					taxCode.externalTaxCodeId = taxCode.externalTaxCodeId.Substring(0, 250);
				}

				taxCode.objectGuid = Guid.NewGuid();
				_context.TaxCodes.Add(taxCode);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.TaxCode entity successfully created.",
					true,
					taxCode.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.TaxCode.CreateAnonymousWithFirstLevelSubObjects(taxCode)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.TaxCode entity creation failed.", false, taxCode.id.ToString(), "", JsonSerializer.Serialize(taxCode), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "TaxCode", taxCode.id, taxCode.name));

			return CreatedAtRoute("TaxCode", new { id = taxCode.id }, Database.TaxCode.CreateAnonymousWithFirstLevelSubObjects(taxCode));
		}



        /// <summary>
        /// 
        /// This deletes a TaxCode record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/TaxCode/{id}")]
		[Route("api/TaxCode")]
		public async Task<IActionResult> DeleteTaxCode(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.TaxCode> query = (from x in _context.TaxCodes
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.TaxCode taxCode = await query.FirstOrDefaultAsync(cancellationToken);

			if (taxCode == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.TaxCode DELETE", id.ToString(), new Exception("No Scheduler.TaxCode entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.TaxCode cloneOfExisting = (Database.TaxCode)_context.Entry(taxCode).GetDatabaseValues().ToObject();


			try
			{
				taxCode.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.TaxCode entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.TaxCode.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TaxCode.CreateAnonymousWithFirstLevelSubObjects(taxCode)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.TaxCode entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.TaxCode.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.TaxCode.CreateAnonymousWithFirstLevelSubObjects(taxCode)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of TaxCode records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/TaxCodes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string code = null,
			decimal? rate = null,
			bool? isDefault = null,
			bool? isExempt = null,
			string externalTaxCodeId = null,
			int? sequence = null,
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

			IQueryable<Database.TaxCode> query = (from tc in _context.TaxCodes select tc);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(tc => tc.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(tc => tc.description == description);
			}
			if (string.IsNullOrEmpty(code) == false)
			{
				query = query.Where(tc => tc.code == code);
			}
			if (rate.HasValue == true)
			{
				query = query.Where(tc => tc.rate == rate.Value);
			}
			if (isDefault.HasValue == true)
			{
				query = query.Where(tc => tc.isDefault == isDefault.Value);
			}
			if (isExempt.HasValue == true)
			{
				query = query.Where(tc => tc.isExempt == isExempt.Value);
			}
			if (string.IsNullOrEmpty(externalTaxCodeId) == false)
			{
				query = query.Where(tc => tc.externalTaxCodeId == externalTaxCodeId);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(tc => tc.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(tc => tc.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(tc => tc.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(tc => tc.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(tc => tc.deleted == false);
				}
			}
			else
			{
				query = query.Where(tc => tc.active == true);
				query = query.Where(tc => tc.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Tax Code, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.code.Contains(anyStringContains)
			       || x.externalTaxCodeId.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.code);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.TaxCode.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/TaxCode/CreateAuditEvent")]
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
