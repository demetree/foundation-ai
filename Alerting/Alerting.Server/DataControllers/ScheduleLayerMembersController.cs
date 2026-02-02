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
using Foundation.Alerting.Database;
using Foundation.ChangeHistory;

namespace Foundation.Alerting.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the ScheduleLayerMember entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ScheduleLayerMember entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ScheduleLayerMembersController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 100;

		static object scheduleLayerMemberPutSyncRoot = new object();
		static object scheduleLayerMemberDeleteSyncRoot = new object();

		private AlertingContext _context;

		private ILogger<ScheduleLayerMembersController> _logger;

		public ScheduleLayerMembersController(AlertingContext context, ILogger<ScheduleLayerMembersController> logger) : base("Alerting", "ScheduleLayerMember")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ScheduleLayerMembers filtered by the parameters provided.
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
		[Route("api/ScheduleLayerMembers")]
		public async Task<IActionResult> GetScheduleLayerMembers(
			int? scheduleLayerId = null,
			int? position = null,
			Guid? securityUserObjectGuid = null,
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
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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

			IQueryable<Database.ScheduleLayerMember> query = (from slm in _context.ScheduleLayerMembers select slm);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (scheduleLayerId.HasValue == true)
			{
				query = query.Where(slm => slm.scheduleLayerId == scheduleLayerId.Value);
			}
			if (position.HasValue == true)
			{
				query = query.Where(slm => slm.position == position.Value);
			}
			if (securityUserObjectGuid.HasValue == true)
			{
				query = query.Where(slm => slm.securityUserObjectGuid == securityUserObjectGuid);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(slm => slm.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(slm => slm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(slm => slm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(slm => slm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(slm => slm.deleted == false);
				}
			}
			else
			{
				query = query.Where(slm => slm.active == true);
				query = query.Where(slm => slm.deleted == false);
			}

			query = query.OrderBy(slm => slm.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.scheduleLayer);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Schedule Layer Member, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       (includeRelations == true && x.scheduleLayer.name.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduleLayer.description.Contains(anyStringContains))
			       || (includeRelations == true && x.scheduleLayer.handoffTime.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ScheduleLayerMember> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ScheduleLayerMember scheduleLayerMember in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(scheduleLayerMember, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.ScheduleLayerMember Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.ScheduleLayerMember Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ScheduleLayerMembers filtered by the parameters provided.  Its query is similar to the GetScheduleLayerMembers method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayerMembers/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? scheduleLayerId = null,
			int? position = null,
			Guid? securityUserObjectGuid = null,
			int? versionNumber = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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


			IQueryable<Database.ScheduleLayerMember> query = (from slm in _context.ScheduleLayerMembers select slm);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (scheduleLayerId.HasValue == true)
			{
				query = query.Where(slm => slm.scheduleLayerId == scheduleLayerId.Value);
			}
			if (position.HasValue == true)
			{
				query = query.Where(slm => slm.position == position.Value);
			}
			if (securityUserObjectGuid.HasValue == true)
			{
				query = query.Where(slm => slm.securityUserObjectGuid == securityUserObjectGuid);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(slm => slm.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(slm => slm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(slm => slm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(slm => slm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(slm => slm.deleted == false);
				}
			}
			else
			{
				query = query.Where(slm => slm.active == true);
				query = query.Where(slm => slm.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Schedule Layer Member, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.scheduleLayer.name.Contains(anyStringContains)
			       || x.scheduleLayer.description.Contains(anyStringContains)
			       || x.scheduleLayer.handoffTime.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ScheduleLayerMember by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayerMember/{id}")]
		public async Task<IActionResult> GetScheduleLayerMember(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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
				IQueryable<Database.ScheduleLayerMember> query = (from slm in _context.ScheduleLayerMembers where
							(slm.id == id) &&
							(userIsAdmin == true || slm.deleted == false) &&
							(userIsWriter == true || slm.active == true)
					select slm);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.scheduleLayer);
					query = query.AsSplitQuery();
				}

				Database.ScheduleLayerMember materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.ScheduleLayerMember Entity was read with Admin privilege." : "Alerting.ScheduleLayerMember Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ScheduleLayerMember", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.ScheduleLayerMember entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.ScheduleLayerMember.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.ScheduleLayerMember.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ScheduleLayerMember record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ScheduleLayerMember/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutScheduleLayerMember(int id, [FromBody]Database.ScheduleLayerMember.ScheduleLayerMemberDTO scheduleLayerMemberDTO, CancellationToken cancellationToken = default)
		{
			if (scheduleLayerMemberDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Alerting Schedule Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Schedule Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != scheduleLayerMemberDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);
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


			IQueryable<Database.ScheduleLayerMember> query = (from x in _context.ScheduleLayerMembers
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduleLayerMember existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.ScheduleLayerMember PUT", id.ToString(), new Exception("No Alerting.ScheduleLayerMember entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (scheduleLayerMemberDTO.objectGuid == Guid.Empty)
            {
                scheduleLayerMemberDTO.objectGuid = existing.objectGuid;
            }
            else if (scheduleLayerMemberDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ScheduleLayerMember record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ScheduleLayerMember cloneOfExisting = (Database.ScheduleLayerMember)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ScheduleLayerMember object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ScheduleLayerMember scheduleLayerMember = (Database.ScheduleLayerMember)_context.Entry(existing).GetDatabaseValues().ToObject();
			scheduleLayerMember.ApplyDTO(scheduleLayerMemberDTO);
			//
			// The tenant guid for any ScheduleLayerMember being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the ScheduleLayerMember because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				scheduleLayerMember.tenantGuid = existing.tenantGuid;
			}

			lock (scheduleLayerMemberPutSyncRoot)
			{
				//
				// Validate the version number for the scheduleLayerMember being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != scheduleLayerMember.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ScheduleLayerMember save attempt was made but save request was with version " + scheduleLayerMember.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ScheduleLayerMember you are trying to update has already changed.  Please try your save again after reloading the ScheduleLayerMember.");
				}
				else
				{
					// Same record.  Increase version.
					scheduleLayerMember.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (scheduleLayerMember.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Alerting.ScheduleLayerMember record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				try
				{
				    EntityEntry<Database.ScheduleLayerMember> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(scheduleLayerMember);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ScheduleLayerMemberChangeHistory scheduleLayerMemberChangeHistory = new ScheduleLayerMemberChangeHistory();
				        scheduleLayerMemberChangeHistory.scheduleLayerMemberId = scheduleLayerMember.id;
				        scheduleLayerMemberChangeHistory.versionNumber = scheduleLayerMember.versionNumber;
				        scheduleLayerMemberChangeHistory.timeStamp = DateTime.UtcNow;
				        scheduleLayerMemberChangeHistory.userId = securityUser.id;
				        scheduleLayerMemberChangeHistory.tenantGuid = userTenantGuid;
				        scheduleLayerMemberChangeHistory.data = JsonSerializer.Serialize(Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(scheduleLayerMember));
				        _context.ScheduleLayerMemberChangeHistories.Add(scheduleLayerMemberChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.ScheduleLayerMember entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(scheduleLayerMember)),
						null);

				return Ok(Database.ScheduleLayerMember.CreateAnonymous(scheduleLayerMember));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.ScheduleLayerMember entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(scheduleLayerMember)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ScheduleLayerMember record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayerMember", Name = "ScheduleLayerMember")]
		public async Task<IActionResult> PostScheduleLayerMember([FromBody]Database.ScheduleLayerMember.ScheduleLayerMemberDTO scheduleLayerMemberDTO, CancellationToken cancellationToken = default)
		{
			if (scheduleLayerMemberDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Alerting Schedule Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Schedule Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new ScheduleLayerMember object using the data from the DTO
			//
			Database.ScheduleLayerMember scheduleLayerMember = Database.ScheduleLayerMember.FromDTO(scheduleLayerMemberDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				scheduleLayerMember.tenantGuid = userTenantGuid;

				scheduleLayerMember.objectGuid = Guid.NewGuid();
				scheduleLayerMember.versionNumber = 1;

				_context.ScheduleLayerMembers.Add(scheduleLayerMember);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the scheduleLayerMember object so that no further changes will be written to the database
				    //
				    _context.Entry(scheduleLayerMember).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					scheduleLayerMember.ScheduleLayerMemberChangeHistories = null;
					scheduleLayerMember.scheduleLayer = null;


				    ScheduleLayerMemberChangeHistory scheduleLayerMemberChangeHistory = new ScheduleLayerMemberChangeHistory();
				    scheduleLayerMemberChangeHistory.scheduleLayerMemberId = scheduleLayerMember.id;
				    scheduleLayerMemberChangeHistory.versionNumber = scheduleLayerMember.versionNumber;
				    scheduleLayerMemberChangeHistory.timeStamp = DateTime.UtcNow;
				    scheduleLayerMemberChangeHistory.userId = securityUser.id;
				    scheduleLayerMemberChangeHistory.tenantGuid = userTenantGuid;
				    scheduleLayerMemberChangeHistory.data = JsonSerializer.Serialize(Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(scheduleLayerMember));
				    _context.ScheduleLayerMemberChangeHistories.Add(scheduleLayerMemberChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Alerting.ScheduleLayerMember entity successfully created.",
						true,
						scheduleLayerMember. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(scheduleLayerMember)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.ScheduleLayerMember entity creation failed.", false, scheduleLayerMember.id.ToString(), "", JsonSerializer.Serialize(Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(scheduleLayerMember)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ScheduleLayerMember", scheduleLayerMember.id, scheduleLayerMember.id.ToString()));

			return CreatedAtRoute("ScheduleLayerMember", new { id = scheduleLayerMember.id }, Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(scheduleLayerMember));
		}



        /// <summary>
        /// 
        /// This rolls a ScheduleLayerMember entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayerMember/Rollback/{id}")]
		[Route("api/ScheduleLayerMember/Rollback")]
		public async Task<IActionResult> RollbackToScheduleLayerMemberVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ScheduleLayerMember> query = (from x in _context.ScheduleLayerMembers
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this ScheduleLayerMember concurrently
			//
			lock (scheduleLayerMemberPutSyncRoot)
			{
				
				Database.ScheduleLayerMember scheduleLayerMember = query.FirstOrDefault();
				
				if (scheduleLayerMember == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.ScheduleLayerMember rollback", id.ToString(), new Exception("No Alerting.ScheduleLayerMember entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ScheduleLayerMember current state so we can log it.
				//
				Database.ScheduleLayerMember cloneOfExisting = (Database.ScheduleLayerMember)_context.Entry(scheduleLayerMember).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ScheduleLayerMemberChangeHistories = null;
				cloneOfExisting.scheduleLayer = null;

				if (versionNumber >= scheduleLayerMember.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Alerting.ScheduleLayerMember rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Alerting.ScheduleLayerMember rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ScheduleLayerMemberChangeHistory scheduleLayerMemberChangeHistory = (from x in _context.ScheduleLayerMemberChangeHistories
				                                               where
				                                               x.scheduleLayerMemberId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (scheduleLayerMemberChangeHistory != null)
				{
				    Database.ScheduleLayerMember oldScheduleLayerMember = JsonSerializer.Deserialize<Database.ScheduleLayerMember>(scheduleLayerMemberChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    scheduleLayerMember.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    scheduleLayerMember.scheduleLayerId = oldScheduleLayerMember.scheduleLayerId;
				    scheduleLayerMember.position = oldScheduleLayerMember.position;
				    scheduleLayerMember.securityUserObjectGuid = oldScheduleLayerMember.securityUserObjectGuid;
				    scheduleLayerMember.objectGuid = oldScheduleLayerMember.objectGuid;
				    scheduleLayerMember.active = oldScheduleLayerMember.active;
				    scheduleLayerMember.deleted = oldScheduleLayerMember.deleted;

				    string serializedScheduleLayerMember = JsonSerializer.Serialize(scheduleLayerMember);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ScheduleLayerMemberChangeHistory newScheduleLayerMemberChangeHistory = new ScheduleLayerMemberChangeHistory();
				        newScheduleLayerMemberChangeHistory.scheduleLayerMemberId = scheduleLayerMember.id;
				        newScheduleLayerMemberChangeHistory.versionNumber = scheduleLayerMember.versionNumber;
				        newScheduleLayerMemberChangeHistory.timeStamp = DateTime.UtcNow;
				        newScheduleLayerMemberChangeHistory.userId = securityUser.id;
				        newScheduleLayerMemberChangeHistory.tenantGuid = userTenantGuid;
				        newScheduleLayerMemberChangeHistory.data = JsonSerializer.Serialize(Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(scheduleLayerMember));
				        _context.ScheduleLayerMemberChangeHistories.Add(newScheduleLayerMemberChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.ScheduleLayerMember rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(scheduleLayerMember)),
						null);


				    return Ok(Database.ScheduleLayerMember.CreateAnonymous(scheduleLayerMember));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Alerting.ScheduleLayerMember rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Alerting.ScheduleLayerMember rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ScheduleLayerMember.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduleLayerMember</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayerMember/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetScheduleLayerMemberChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.ScheduleLayerMember scheduleLayerMember = await _context.ScheduleLayerMembers.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduleLayerMember == null)
			{
				return NotFound();
			}

			try
			{
				scheduleLayerMember.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduleLayerMember> versionInfo = await scheduleLayerMember.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ScheduleLayerMember.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduleLayerMember</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayerMember/{id}/AuditHistory")]
		public async Task<IActionResult> GetScheduleLayerMemberAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.ScheduleLayerMember scheduleLayerMember = await _context.ScheduleLayerMembers.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduleLayerMember == null)
			{
				return NotFound();
			}

			try
			{
				scheduleLayerMember.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.ScheduleLayerMember>> versions = await scheduleLayerMember.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ScheduleLayerMember.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduleLayerMember</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ScheduleLayerMember object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayerMember/{id}/Version/{version}")]
		public async Task<IActionResult> GetScheduleLayerMemberVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.ScheduleLayerMember scheduleLayerMember = await _context.ScheduleLayerMembers.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduleLayerMember == null)
			{
				return NotFound();
			}

			try
			{
				scheduleLayerMember.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduleLayerMember> versionInfo = await scheduleLayerMember.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ScheduleLayerMember at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ScheduleLayerMember</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ScheduleLayerMember object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayerMember/{id}/StateAtTime")]
		public async Task<IActionResult> GetScheduleLayerMemberStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
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


			Database.ScheduleLayerMember scheduleLayerMember = await _context.ScheduleLayerMembers.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (scheduleLayerMember == null)
			{
				return NotFound();
			}

			try
			{
				scheduleLayerMember.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.ScheduleLayerMember> versionInfo = await scheduleLayerMember.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ScheduleLayerMember record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ScheduleLayerMember/{id}")]
		[Route("api/ScheduleLayerMember")]
		public async Task<IActionResult> DeleteScheduleLayerMember(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Alerting Schedule Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Schedule Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.ScheduleLayerMember> query = (from x in _context.ScheduleLayerMembers
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.ScheduleLayerMember scheduleLayerMember = await query.FirstOrDefaultAsync(cancellationToken);

			if (scheduleLayerMember == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.ScheduleLayerMember DELETE", id.ToString(), new Exception("No Alerting.ScheduleLayerMember entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ScheduleLayerMember cloneOfExisting = (Database.ScheduleLayerMember)_context.Entry(scheduleLayerMember).GetDatabaseValues().ToObject();


			lock (scheduleLayerMemberDeleteSyncRoot)
			{
			    try
			    {
			        scheduleLayerMember.deleted = true;
			        scheduleLayerMember.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ScheduleLayerMemberChangeHistory scheduleLayerMemberChangeHistory = new ScheduleLayerMemberChangeHistory();
			        scheduleLayerMemberChangeHistory.scheduleLayerMemberId = scheduleLayerMember.id;
			        scheduleLayerMemberChangeHistory.versionNumber = scheduleLayerMember.versionNumber;
			        scheduleLayerMemberChangeHistory.timeStamp = DateTime.UtcNow;
			        scheduleLayerMemberChangeHistory.userId = securityUser.id;
			        scheduleLayerMemberChangeHistory.tenantGuid = userTenantGuid;
			        scheduleLayerMemberChangeHistory.data = JsonSerializer.Serialize(Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(scheduleLayerMember));
			        _context.ScheduleLayerMemberChangeHistories.Add(scheduleLayerMemberChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.ScheduleLayerMember entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(scheduleLayerMember)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.ScheduleLayerMember entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ScheduleLayerMember.CreateAnonymousWithFirstLevelSubObjects(scheduleLayerMember)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ScheduleLayerMember records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ScheduleLayerMembers/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? scheduleLayerId = null,
			int? position = null,
			Guid? securityUserObjectGuid = null,
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
			// Alerting Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 100, cancellationToken);


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

			IQueryable<Database.ScheduleLayerMember> query = (from slm in _context.ScheduleLayerMembers select slm);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (scheduleLayerId.HasValue == true)
			{
				query = query.Where(slm => slm.scheduleLayerId == scheduleLayerId.Value);
			}
			if (position.HasValue == true)
			{
				query = query.Where(slm => slm.position == position.Value);
			}
			if (securityUserObjectGuid.HasValue == true)
			{
				query = query.Where(slm => slm.securityUserObjectGuid == securityUserObjectGuid);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(slm => slm.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(slm => slm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(slm => slm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(slm => slm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(slm => slm.deleted == false);
				}
			}
			else
			{
				query = query.Where(slm => slm.active == true);
				query = query.Where(slm => slm.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Schedule Layer Member, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.scheduleLayer.name.Contains(anyStringContains)
			       || x.scheduleLayer.description.Contains(anyStringContains)
			       || x.scheduleLayer.handoffTime.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ScheduleLayerMember.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ScheduleLayerMember/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Schedule Writer role needed to write to this table, or Alerting Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("Alerting Schedule Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
