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
    /// This auto generated class provides the basic CRUD operations for the AccountType entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AccountType entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AccountTypesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		private SchedulerContext _context;

		private ILogger<AccountTypesController> _logger;

		public AccountTypesController(SchedulerContext context, ILogger<AccountTypesController> logger) : base("Scheduler", "AccountType")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of AccountTypes filtered by the parameters provided.
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
		[Route("api/AccountTypes")]
		public async Task<IActionResult> GetAccountTypes(
			string name = null,
			string description = null,
			bool? isRevenue = null,
			string externalMapping = null,
			string color = null,
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

			IQueryable<Database.AccountType> query = (from at in _context.AccountTypes select at);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(at => at.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(at => at.description == description);
			}
			if (isRevenue.HasValue == true)
			{
				query = query.Where(at => at.isRevenue == isRevenue.Value);
			}
			if (string.IsNullOrEmpty(externalMapping) == false)
			{
				query = query.Where(at => at.externalMapping == externalMapping);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(at => at.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(at => at.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(at => at.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(at => at.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(at => at.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(at => at.deleted == false);
				}
			}
			else
			{
				query = query.Where(at => at.active == true);
				query = query.Where(at => at.deleted == false);
			}

			query = query.OrderBy(at => at.sequence).ThenBy(at => at.name).ThenBy(at => at.description).ThenBy(at => at.externalMapping);


			//
			// Add the any string contains parameter to span all the string fields on the Account Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.externalMapping.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
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
			
			List<Database.AccountType> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AccountType accountType in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(accountType, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.AccountType Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.AccountType Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of AccountTypes filtered by the parameters provided.  Its query is similar to the GetAccountTypes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AccountTypes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			bool? isRevenue = null,
			string externalMapping = null,
			string color = null,
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

			IQueryable<Database.AccountType> query = (from at in _context.AccountTypes select at);
			if (name != null)
			{
				query = query.Where(at => at.name == name);
			}
			if (description != null)
			{
				query = query.Where(at => at.description == description);
			}
			if (isRevenue.HasValue == true)
			{
				query = query.Where(at => at.isRevenue == isRevenue.Value);
			}
			if (externalMapping != null)
			{
				query = query.Where(at => at.externalMapping == externalMapping);
			}
			if (color != null)
			{
				query = query.Where(at => at.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(at => at.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(at => at.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(at => at.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(at => at.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(at => at.deleted == false);
				}
			}
			else
			{
				query = query.Where(at => at.active == true);
				query = query.Where(at => at.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Account Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.externalMapping.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single AccountType by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AccountType/{id}")]
		public async Task<IActionResult> GetAccountType(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			try
			{
				IQueryable<Database.AccountType> query = (from at in _context.AccountTypes where
							(at.id == id) &&
							(userIsAdmin == true || at.deleted == false) &&
							(userIsWriter == true || at.active == true)
					select at);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.AccountType materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.AccountType Entity was read with Admin privilege." : "Scheduler.AccountType Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AccountType", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.AccountType entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.AccountType.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.AccountType.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing AccountType record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AccountType/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAccountType(int id, [FromBody]Database.AccountType.AccountTypeDTO accountTypeDTO, CancellationToken cancellationToken = default)
		{
			if (accountTypeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != accountTypeDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.AccountType> query = (from x in _context.AccountTypes
				where
				(x.id == id)
				select x);


			Database.AccountType existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.AccountType PUT", id.ToString(), new Exception("No Scheduler.AccountType entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (accountTypeDTO.objectGuid == Guid.Empty)
            {
                accountTypeDTO.objectGuid = existing.objectGuid;
            }
            else if (accountTypeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a AccountType record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AccountType cloneOfExisting = (Database.AccountType)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AccountType object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AccountType accountType = (Database.AccountType)_context.Entry(existing).GetDatabaseValues().ToObject();
			accountType.ApplyDTO(accountTypeDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (accountType.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.AccountType record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (accountType.name != null && accountType.name.Length > 100)
			{
				accountType.name = accountType.name.Substring(0, 100);
			}

			if (accountType.description != null && accountType.description.Length > 500)
			{
				accountType.description = accountType.description.Substring(0, 500);
			}

			if (accountType.externalMapping != null && accountType.externalMapping.Length > 100)
			{
				accountType.externalMapping = accountType.externalMapping.Substring(0, 100);
			}

			if (accountType.color != null && accountType.color.Length > 10)
			{
				accountType.color = accountType.color.Substring(0, 10);
			}

			EntityEntry<Database.AccountType> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(accountType);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.AccountType entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AccountType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AccountType.CreateAnonymousWithFirstLevelSubObjects(accountType)),
					null);


				return Ok(Database.AccountType.CreateAnonymous(accountType));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.AccountType entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AccountType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AccountType.CreateAnonymousWithFirstLevelSubObjects(accountType)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new AccountType record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AccountType", Name = "AccountType")]
		public async Task<IActionResult> PostAccountType([FromBody]Database.AccountType.AccountTypeDTO accountTypeDTO, CancellationToken cancellationToken = default)
		{
			if (accountTypeDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new AccountType object using the data from the DTO
			//
			Database.AccountType accountType = Database.AccountType.FromDTO(accountTypeDTO);

			try
			{
				if (accountType.name != null && accountType.name.Length > 100)
				{
					accountType.name = accountType.name.Substring(0, 100);
				}

				if (accountType.description != null && accountType.description.Length > 500)
				{
					accountType.description = accountType.description.Substring(0, 500);
				}

				if (accountType.externalMapping != null && accountType.externalMapping.Length > 100)
				{
					accountType.externalMapping = accountType.externalMapping.Substring(0, 100);
				}

				if (accountType.color != null && accountType.color.Length > 10)
				{
					accountType.color = accountType.color.Substring(0, 10);
				}

				accountType.objectGuid = Guid.NewGuid();
				_context.AccountTypes.Add(accountType);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.AccountType entity successfully created.",
					true,
					accountType.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.AccountType.CreateAnonymousWithFirstLevelSubObjects(accountType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.AccountType entity creation failed.", false, accountType.id.ToString(), "", JsonSerializer.Serialize(accountType), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AccountType", accountType.id, accountType.name));

			return CreatedAtRoute("AccountType", new { id = accountType.id }, Database.AccountType.CreateAnonymousWithFirstLevelSubObjects(accountType));
		}



        /// <summary>
        /// 
        /// This deletes a AccountType record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AccountType/{id}")]
		[Route("api/AccountType")]
		public async Task<IActionResult> DeleteAccountType(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.AccountType> query = (from x in _context.AccountTypes
				where
				(x.id == id)
				select x);


			Database.AccountType accountType = await query.FirstOrDefaultAsync(cancellationToken);

			if (accountType == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.AccountType DELETE", id.ToString(), new Exception("No Scheduler.AccountType entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AccountType cloneOfExisting = (Database.AccountType)_context.Entry(accountType).GetDatabaseValues().ToObject();


			try
			{
				accountType.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.AccountType entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AccountType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AccountType.CreateAnonymousWithFirstLevelSubObjects(accountType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.AccountType entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AccountType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AccountType.CreateAnonymousWithFirstLevelSubObjects(accountType)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of AccountType records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AccountTypes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			bool? isRevenue = null,
			string externalMapping = null,
			string color = null,
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

			IQueryable<Database.AccountType> query = (from at in _context.AccountTypes select at);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(at => at.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(at => at.description == description);
			}
			if (isRevenue.HasValue == true)
			{
				query = query.Where(at => at.isRevenue == isRevenue.Value);
			}
			if (string.IsNullOrEmpty(externalMapping) == false)
			{
				query = query.Where(at => at.externalMapping == externalMapping);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(at => at.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(at => at.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(at => at.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(at => at.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(at => at.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(at => at.deleted == false);
				}
			}
			else
			{
				query = query.Where(at => at.active == true);
				query = query.Where(at => at.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Account Type, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.externalMapping.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.externalMapping);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.AccountType.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AccountType/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
