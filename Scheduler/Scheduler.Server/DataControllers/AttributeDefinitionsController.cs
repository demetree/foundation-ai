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
    /// This auto generated class provides the basic CRUD operations for the AttributeDefinition entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AttributeDefinition entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AttributeDefinitionsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object attributeDefinitionPutSyncRoot = new object();
		static object attributeDefinitionDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<AttributeDefinitionsController> _logger;

		public AttributeDefinitionsController(SchedulerContext context, ILogger<AttributeDefinitionsController> logger) : base("Scheduler", "AttributeDefinition")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of AttributeDefinitions filtered by the parameters provided.
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
		[Route("api/AttributeDefinitions")]
		public async Task<IActionResult> GetAttributeDefinitions(
			int? attributeDefinitionEntityId = null,
			string key = null,
			string label = null,
			int? attributeDefinitionTypeId = null,
			string options = null,
			bool? isRequired = null,
			int? sequence = null,
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

			IQueryable<Database.AttributeDefinition> query = (from ad in _context.AttributeDefinitions select ad);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (attributeDefinitionEntityId.HasValue == true)
			{
				query = query.Where(ad => ad.attributeDefinitionEntityId == attributeDefinitionEntityId.Value);
			}
			if (string.IsNullOrEmpty(key) == false)
			{
				query = query.Where(ad => ad.key == key);
			}
			if (string.IsNullOrEmpty(label) == false)
			{
				query = query.Where(ad => ad.label == label);
			}
			if (attributeDefinitionTypeId.HasValue == true)
			{
				query = query.Where(ad => ad.attributeDefinitionTypeId == attributeDefinitionTypeId.Value);
			}
			if (string.IsNullOrEmpty(options) == false)
			{
				query = query.Where(ad => ad.options == options);
			}
			if (isRequired.HasValue == true)
			{
				query = query.Where(ad => ad.isRequired == isRequired.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ad => ad.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ad => ad.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ad => ad.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ad => ad.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ad => ad.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ad => ad.deleted == false);
				}
			}
			else
			{
				query = query.Where(ad => ad.active == true);
				query = query.Where(ad => ad.deleted == false);
			}

			query = query.OrderBy(ad => ad.sequence).ThenBy(ad => ad.key).ThenBy(ad => ad.label);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.attributeDefinitionEntity);
				query = query.Include(x => x.attributeDefinitionType);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Attribute Definition, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.key.Contains(anyStringContains)
			       || x.label.Contains(anyStringContains)
			       || x.options.Contains(anyStringContains)
			       || (includeRelations == true && x.attributeDefinitionEntity.name.Contains(anyStringContains))
			       || (includeRelations == true && x.attributeDefinitionEntity.description.Contains(anyStringContains))
			       || (includeRelations == true && x.attributeDefinitionType.name.Contains(anyStringContains))
			       || (includeRelations == true && x.attributeDefinitionType.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.AttributeDefinition> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AttributeDefinition attributeDefinition in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(attributeDefinition, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.AttributeDefinition Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.AttributeDefinition Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of AttributeDefinitions filtered by the parameters provided.  Its query is similar to the GetAttributeDefinitions method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinitions/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? attributeDefinitionEntityId = null,
			string key = null,
			string label = null,
			int? attributeDefinitionTypeId = null,
			string options = null,
			bool? isRequired = null,
			int? sequence = null,
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


			IQueryable<Database.AttributeDefinition> query = (from ad in _context.AttributeDefinitions select ad);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (attributeDefinitionEntityId.HasValue == true)
			{
				query = query.Where(ad => ad.attributeDefinitionEntityId == attributeDefinitionEntityId.Value);
			}
			if (key != null)
			{
				query = query.Where(ad => ad.key == key);
			}
			if (label != null)
			{
				query = query.Where(ad => ad.label == label);
			}
			if (attributeDefinitionTypeId.HasValue == true)
			{
				query = query.Where(ad => ad.attributeDefinitionTypeId == attributeDefinitionTypeId.Value);
			}
			if (options != null)
			{
				query = query.Where(ad => ad.options == options);
			}
			if (isRequired.HasValue == true)
			{
				query = query.Where(ad => ad.isRequired == isRequired.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ad => ad.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ad => ad.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ad => ad.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ad => ad.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ad => ad.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ad => ad.deleted == false);
				}
			}
			else
			{
				query = query.Where(ad => ad.active == true);
				query = query.Where(ad => ad.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Attribute Definition, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.key.Contains(anyStringContains)
			       || x.label.Contains(anyStringContains)
			       || x.options.Contains(anyStringContains)
			       || x.attributeDefinitionEntity.name.Contains(anyStringContains)
			       || x.attributeDefinitionEntity.description.Contains(anyStringContains)
			       || x.attributeDefinitionType.name.Contains(anyStringContains)
			       || x.attributeDefinitionType.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single AttributeDefinition by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinition/{id}")]
		public async Task<IActionResult> GetAttributeDefinition(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
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


			try
			{
				IQueryable<Database.AttributeDefinition> query = (from ad in _context.AttributeDefinitions where
							(ad.id == id) &&
							(userIsAdmin == true || ad.deleted == false) &&
							(userIsWriter == true || ad.active == true)
					select ad);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.attributeDefinitionEntity);
					query = query.Include(x => x.attributeDefinitionType);
					query = query.AsSplitQuery();
				}

				Database.AttributeDefinition materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.AttributeDefinition Entity was read with Admin privilege." : "Scheduler.AttributeDefinition Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AttributeDefinition", materialized.id, materialized.key));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.AttributeDefinition entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.AttributeDefinition.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.AttributeDefinition.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing AttributeDefinition record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AttributeDefinition/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAttributeDefinition(int id, [FromBody]Database.AttributeDefinition.AttributeDefinitionDTO attributeDefinitionDTO, CancellationToken cancellationToken = default)
		{
			if (attributeDefinitionDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != attributeDefinitionDTO.id)
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


			IQueryable<Database.AttributeDefinition> query = (from x in _context.AttributeDefinitions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.AttributeDefinition existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.AttributeDefinition PUT", id.ToString(), new Exception("No Scheduler.AttributeDefinition entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (attributeDefinitionDTO.objectGuid == Guid.Empty)
            {
                attributeDefinitionDTO.objectGuid = existing.objectGuid;
            }
            else if (attributeDefinitionDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a AttributeDefinition record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AttributeDefinition cloneOfExisting = (Database.AttributeDefinition)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AttributeDefinition object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AttributeDefinition attributeDefinition = (Database.AttributeDefinition)_context.Entry(existing).GetDatabaseValues().ToObject();
			attributeDefinition.ApplyDTO(attributeDefinitionDTO);
			//
			// The tenant guid for any AttributeDefinition being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the AttributeDefinition because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				attributeDefinition.tenantGuid = existing.tenantGuid;
			}

			lock (attributeDefinitionPutSyncRoot)
			{
				//
				// Validate the version number for the attributeDefinition being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != attributeDefinition.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "AttributeDefinition save attempt was made but save request was with version " + attributeDefinition.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The AttributeDefinition you are trying to update has already changed.  Please try your save again after reloading the AttributeDefinition.");
				}
				else
				{
					// Same record.  Increase version.
					attributeDefinition.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (attributeDefinition.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.AttributeDefinition record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (attributeDefinition.key != null && attributeDefinition.key.Length > 100)
				{
					attributeDefinition.key = attributeDefinition.key.Substring(0, 100);
				}

				if (attributeDefinition.label != null && attributeDefinition.label.Length > 250)
				{
					attributeDefinition.label = attributeDefinition.label.Substring(0, 250);
				}

				try
				{
				    EntityEntry<Database.AttributeDefinition> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(attributeDefinition);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        AttributeDefinitionChangeHistory attributeDefinitionChangeHistory = new AttributeDefinitionChangeHistory();
				        attributeDefinitionChangeHistory.attributeDefinitionId = attributeDefinition.id;
				        attributeDefinitionChangeHistory.versionNumber = attributeDefinition.versionNumber;
				        attributeDefinitionChangeHistory.timeStamp = DateTime.UtcNow;
				        attributeDefinitionChangeHistory.userId = securityUser.id;
				        attributeDefinitionChangeHistory.tenantGuid = userTenantGuid;
				        attributeDefinitionChangeHistory.data = JsonSerializer.Serialize(Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(attributeDefinition));
				        _context.AttributeDefinitionChangeHistories.Add(attributeDefinitionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.AttributeDefinition entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(attributeDefinition)),
						null);

				return Ok(Database.AttributeDefinition.CreateAnonymous(attributeDefinition));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.AttributeDefinition entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(attributeDefinition)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new AttributeDefinition record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinition", Name = "AttributeDefinition")]
		public async Task<IActionResult> PostAttributeDefinition([FromBody]Database.AttributeDefinition.AttributeDefinitionDTO attributeDefinitionDTO, CancellationToken cancellationToken = default)
		{
			if (attributeDefinitionDTO == null)
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
			// Create a new AttributeDefinition object using the data from the DTO
			//
			Database.AttributeDefinition attributeDefinition = Database.AttributeDefinition.FromDTO(attributeDefinitionDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				attributeDefinition.tenantGuid = userTenantGuid;

				if (attributeDefinition.key != null && attributeDefinition.key.Length > 100)
				{
					attributeDefinition.key = attributeDefinition.key.Substring(0, 100);
				}

				if (attributeDefinition.label != null && attributeDefinition.label.Length > 250)
				{
					attributeDefinition.label = attributeDefinition.label.Substring(0, 250);
				}

				attributeDefinition.objectGuid = Guid.NewGuid();
				attributeDefinition.versionNumber = 1;

				_context.AttributeDefinitions.Add(attributeDefinition);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the attributeDefinition object so that no further changes will be written to the database
				    //
				    _context.Entry(attributeDefinition).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					attributeDefinition.AttributeDefinitionChangeHistories = null;
					attributeDefinition.attributeDefinitionEntity = null;
					attributeDefinition.attributeDefinitionType = null;


				    AttributeDefinitionChangeHistory attributeDefinitionChangeHistory = new AttributeDefinitionChangeHistory();
				    attributeDefinitionChangeHistory.attributeDefinitionId = attributeDefinition.id;
				    attributeDefinitionChangeHistory.versionNumber = attributeDefinition.versionNumber;
				    attributeDefinitionChangeHistory.timeStamp = DateTime.UtcNow;
				    attributeDefinitionChangeHistory.userId = securityUser.id;
				    attributeDefinitionChangeHistory.tenantGuid = userTenantGuid;
				    attributeDefinitionChangeHistory.data = JsonSerializer.Serialize(Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(attributeDefinition));
				    _context.AttributeDefinitionChangeHistories.Add(attributeDefinitionChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.AttributeDefinition entity successfully created.",
						true,
						attributeDefinition. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(attributeDefinition)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.AttributeDefinition entity creation failed.", false, attributeDefinition.id.ToString(), "", JsonSerializer.Serialize(Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(attributeDefinition)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AttributeDefinition", attributeDefinition.id, attributeDefinition.key));

			return CreatedAtRoute("AttributeDefinition", new { id = attributeDefinition.id }, Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(attributeDefinition));
		}



        /// <summary>
        /// 
        /// This rolls a AttributeDefinition entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinition/Rollback/{id}")]
		[Route("api/AttributeDefinition/Rollback")]
		public async Task<IActionResult> RollbackToAttributeDefinitionVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.AttributeDefinition> query = (from x in _context.AttributeDefinitions
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this AttributeDefinition concurrently
			//
			lock (attributeDefinitionPutSyncRoot)
			{
				
				Database.AttributeDefinition attributeDefinition = query.FirstOrDefault();
				
				if (attributeDefinition == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.AttributeDefinition rollback", id.ToString(), new Exception("No Scheduler.AttributeDefinition entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the AttributeDefinition current state so we can log it.
				//
				Database.AttributeDefinition cloneOfExisting = (Database.AttributeDefinition)_context.Entry(attributeDefinition).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.AttributeDefinitionChangeHistories = null;
				cloneOfExisting.attributeDefinitionEntity = null;
				cloneOfExisting.attributeDefinitionType = null;

				if (versionNumber >= attributeDefinition.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.AttributeDefinition rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.AttributeDefinition rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				AttributeDefinitionChangeHistory attributeDefinitionChangeHistory = (from x in _context.AttributeDefinitionChangeHistories
				                                               where
				                                               x.attributeDefinitionId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (attributeDefinitionChangeHistory != null)
				{
				    Database.AttributeDefinition oldAttributeDefinition = JsonSerializer.Deserialize<Database.AttributeDefinition>(attributeDefinitionChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    attributeDefinition.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    attributeDefinition.attributeDefinitionEntityId = oldAttributeDefinition.attributeDefinitionEntityId;
				    attributeDefinition.key = oldAttributeDefinition.key;
				    attributeDefinition.label = oldAttributeDefinition.label;
				    attributeDefinition.attributeDefinitionTypeId = oldAttributeDefinition.attributeDefinitionTypeId;
				    attributeDefinition.options = oldAttributeDefinition.options;
				    attributeDefinition.isRequired = oldAttributeDefinition.isRequired;
				    attributeDefinition.sequence = oldAttributeDefinition.sequence;
				    attributeDefinition.objectGuid = oldAttributeDefinition.objectGuid;
				    attributeDefinition.active = oldAttributeDefinition.active;
				    attributeDefinition.deleted = oldAttributeDefinition.deleted;

				    string serializedAttributeDefinition = JsonSerializer.Serialize(attributeDefinition);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        AttributeDefinitionChangeHistory newAttributeDefinitionChangeHistory = new AttributeDefinitionChangeHistory();
				        newAttributeDefinitionChangeHistory.attributeDefinitionId = attributeDefinition.id;
				        newAttributeDefinitionChangeHistory.versionNumber = attributeDefinition.versionNumber;
				        newAttributeDefinitionChangeHistory.timeStamp = DateTime.UtcNow;
				        newAttributeDefinitionChangeHistory.userId = securityUser.id;
				        newAttributeDefinitionChangeHistory.tenantGuid = userTenantGuid;
				        newAttributeDefinitionChangeHistory.data = JsonSerializer.Serialize(Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(attributeDefinition));
				        _context.AttributeDefinitionChangeHistories.Add(newAttributeDefinitionChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.AttributeDefinition rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(attributeDefinition)),
						null);


				    return Ok(Database.AttributeDefinition.CreateAnonymous(attributeDefinition));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.AttributeDefinition rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.AttributeDefinition rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a AttributeDefinition.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the AttributeDefinition</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinition/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetAttributeDefinitionChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{
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


			Database.AttributeDefinition attributeDefinition = await _context.AttributeDefinitions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (attributeDefinition == null)
			{
				return NotFound();
			}

			try
			{
				attributeDefinition.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.AttributeDefinition> versionInfo = await attributeDefinition.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a AttributeDefinition.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the AttributeDefinition</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinition/{id}/AuditHistory")]
		public async Task<IActionResult> GetAttributeDefinitionAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{
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


			Database.AttributeDefinition attributeDefinition = await _context.AttributeDefinitions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (attributeDefinition == null)
			{
				return NotFound();
			}

			try
			{
				attributeDefinition.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.AttributeDefinition>> versions = await attributeDefinition.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a AttributeDefinition.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the AttributeDefinition</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The AttributeDefinition object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinition/{id}/Version/{version}")]
		public async Task<IActionResult> GetAttributeDefinitionVersion(int id, int version, CancellationToken cancellationToken = default)
		{
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


			Database.AttributeDefinition attributeDefinition = await _context.AttributeDefinitions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (attributeDefinition == null)
			{
				return NotFound();
			}

			try
			{
				attributeDefinition.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.AttributeDefinition> versionInfo = await attributeDefinition.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a AttributeDefinition at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the AttributeDefinition</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The AttributeDefinition object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinition/{id}/StateAtTime")]
		public async Task<IActionResult> GetAttributeDefinitionStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{
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


			Database.AttributeDefinition attributeDefinition = await _context.AttributeDefinitions.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (attributeDefinition == null)
			{
				return NotFound();
			}

			try
			{
				attributeDefinition.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.AttributeDefinition> versionInfo = await attributeDefinition.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a AttributeDefinition record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AttributeDefinition/{id}")]
		[Route("api/AttributeDefinition")]
		public async Task<IActionResult> DeleteAttributeDefinition(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.AttributeDefinition> query = (from x in _context.AttributeDefinitions
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.AttributeDefinition attributeDefinition = await query.FirstOrDefaultAsync(cancellationToken);

			if (attributeDefinition == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.AttributeDefinition DELETE", id.ToString(), new Exception("No Scheduler.AttributeDefinition entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AttributeDefinition cloneOfExisting = (Database.AttributeDefinition)_context.Entry(attributeDefinition).GetDatabaseValues().ToObject();


			lock (attributeDefinitionDeleteSyncRoot)
			{
			    try
			    {
			        attributeDefinition.deleted = true;
			        attributeDefinition.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        AttributeDefinitionChangeHistory attributeDefinitionChangeHistory = new AttributeDefinitionChangeHistory();
			        attributeDefinitionChangeHistory.attributeDefinitionId = attributeDefinition.id;
			        attributeDefinitionChangeHistory.versionNumber = attributeDefinition.versionNumber;
			        attributeDefinitionChangeHistory.timeStamp = DateTime.UtcNow;
			        attributeDefinitionChangeHistory.userId = securityUser.id;
			        attributeDefinitionChangeHistory.tenantGuid = userTenantGuid;
			        attributeDefinitionChangeHistory.data = JsonSerializer.Serialize(Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(attributeDefinition));
			        _context.AttributeDefinitionChangeHistories.Add(attributeDefinitionChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.AttributeDefinition entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(attributeDefinition)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.AttributeDefinition entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.AttributeDefinition.CreateAnonymousWithFirstLevelSubObjects(attributeDefinition)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of AttributeDefinition records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AttributeDefinitions/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? attributeDefinitionEntityId = null,
			string key = null,
			string label = null,
			int? attributeDefinitionTypeId = null,
			string options = null,
			bool? isRequired = null,
			int? sequence = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);

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

			IQueryable<Database.AttributeDefinition> query = (from ad in _context.AttributeDefinitions select ad);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (attributeDefinitionEntityId.HasValue == true)
			{
				query = query.Where(ad => ad.attributeDefinitionEntityId == attributeDefinitionEntityId.Value);
			}
			if (string.IsNullOrEmpty(key) == false)
			{
				query = query.Where(ad => ad.key == key);
			}
			if (string.IsNullOrEmpty(label) == false)
			{
				query = query.Where(ad => ad.label == label);
			}
			if (attributeDefinitionTypeId.HasValue == true)
			{
				query = query.Where(ad => ad.attributeDefinitionTypeId == attributeDefinitionTypeId.Value);
			}
			if (string.IsNullOrEmpty(options) == false)
			{
				query = query.Where(ad => ad.options == options);
			}
			if (isRequired.HasValue == true)
			{
				query = query.Where(ad => ad.isRequired == isRequired.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ad => ad.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(ad => ad.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ad => ad.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ad => ad.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ad => ad.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ad => ad.deleted == false);
				}
			}
			else
			{
				query = query.Where(ad => ad.active == true);
				query = query.Where(ad => ad.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Attribute Definition, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.key.Contains(anyStringContains)
			       || x.label.Contains(anyStringContains)
			       || x.options.Contains(anyStringContains)
			       || x.attributeDefinitionEntity.name.Contains(anyStringContains)
			       || x.attributeDefinitionEntity.description.Contains(anyStringContains)
			       || x.attributeDefinitionType.name.Contains(anyStringContains)
			       || x.attributeDefinitionType.description.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.key).ThenBy(x => x.label);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.AttributeDefinition.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AttributeDefinition/CreateAuditEvent")]
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
