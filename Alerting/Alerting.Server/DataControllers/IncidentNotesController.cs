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
    /// This auto generated class provides the basic CRUD operations for the IncidentNote entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the IncidentNote entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class IncidentNotesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		static object incidentNotePutSyncRoot = new object();
		static object incidentNoteDeleteSyncRoot = new object();

		private AlertingContext _context;

		private ILogger<IncidentNotesController> _logger;

		public IncidentNotesController(AlertingContext context, ILogger<IncidentNotesController> logger) : base("Alerting", "IncidentNote")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of IncidentNotes filtered by the parameters provided.
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
		[Route("api/IncidentNotes")]
		public async Task<IActionResult> GetIncidentNotes(
			int? incidentId = null,
			Guid? authorObjectGuid = null,
			DateTime? createdAt = null,
			string content = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
			if (createdAt.HasValue == true && createdAt.Value.Kind != DateTimeKind.Utc)
			{
				createdAt = createdAt.Value.ToUniversalTime();
			}

			IQueryable<Database.IncidentNote> query = (from in_ in _context.IncidentNotes select in_);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (incidentId.HasValue == true)
			{
				query = query.Where(in_ => in_.incidentId == incidentId.Value);
			}
			if (authorObjectGuid.HasValue == true)
			{
				query = query.Where(in_ => in_.authorObjectGuid == authorObjectGuid);
			}
			if (createdAt.HasValue == true)
			{
				query = query.Where(in_ => in_.createdAt == createdAt.Value);
			}
			if (string.IsNullOrEmpty(content) == false)
			{
				query = query.Where(in_ => in_.content == content);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(in_ => in_.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(in_ => in_.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(in_ => in_.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(in_ => in_.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(in_ => in_.deleted == false);
				}
			}
			else
			{
				query = query.Where(in_ => in_.active == true);
				query = query.Where(in_ => in_.deleted == false);
			}

			query = query.OrderBy(in_ => in_.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.incident);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Incident Note, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.content.Contains(anyStringContains)
			       || (includeRelations == true && x.incident.incidentKey.Contains(anyStringContains))
			       || (includeRelations == true && x.incident.title.Contains(anyStringContains))
			       || (includeRelations == true && x.incident.description.Contains(anyStringContains))
			       || (includeRelations == true && x.incident.sourcePayloadJson.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.IncidentNote> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.IncidentNote incidentNote in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(incidentNote, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Alerting.IncidentNote Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Alerting.IncidentNote Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of IncidentNotes filtered by the parameters provided.  Its query is similar to the GetIncidentNotes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IncidentNotes/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? incidentId = null,
			Guid? authorObjectGuid = null,
			DateTime? createdAt = null,
			string content = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
			if (createdAt.HasValue == true && createdAt.Value.Kind != DateTimeKind.Utc)
			{
				createdAt = createdAt.Value.ToUniversalTime();
			}

			IQueryable<Database.IncidentNote> query = (from in_ in _context.IncidentNotes select in_);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (incidentId.HasValue == true)
			{
				query = query.Where(in_ => in_.incidentId == incidentId.Value);
			}
			if (authorObjectGuid.HasValue == true)
			{
				query = query.Where(in_ => in_.authorObjectGuid == authorObjectGuid);
			}
			if (createdAt.HasValue == true)
			{
				query = query.Where(in_ => in_.createdAt == createdAt.Value);
			}
			if (content != null)
			{
				query = query.Where(in_ => in_.content == content);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(in_ => in_.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(in_ => in_.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(in_ => in_.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(in_ => in_.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(in_ => in_.deleted == false);
				}
			}
			else
			{
				query = query.Where(in_ => in_.active == true);
				query = query.Where(in_ => in_.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Incident Note, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.content.Contains(anyStringContains)
			       || x.incident.incidentKey.Contains(anyStringContains)
			       || x.incident.title.Contains(anyStringContains)
			       || x.incident.description.Contains(anyStringContains)
			       || x.incident.sourcePayloadJson.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single IncidentNote by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IncidentNote/{id}")]
		public async Task<IActionResult> GetIncidentNote(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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
				IQueryable<Database.IncidentNote> query = (from in_ in _context.IncidentNotes where
							(in_.id == id) &&
							(userIsAdmin == true || in_.deleted == false) &&
							(userIsWriter == true || in_.active == true)
					select in_);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.incident);
					query = query.AsSplitQuery();
				}

				Database.IncidentNote materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Alerting.IncidentNote Entity was read with Admin privilege." : "Alerting.IncidentNote Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "IncidentNote", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Alerting.IncidentNote entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Alerting.IncidentNote.   Entity was read with Admin privilege." : "Exception caught during entity read of Alerting.IncidentNote.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing IncidentNote record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/IncidentNote/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutIncidentNote(int id, [FromBody]Database.IncidentNote.IncidentNoteDTO incidentNoteDTO, CancellationToken cancellationToken = default)
		{
			if (incidentNoteDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Alerting Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != incidentNoteDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);
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


			IQueryable<Database.IncidentNote> query = (from x in _context.IncidentNotes
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.IncidentNote existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.IncidentNote PUT", id.ToString(), new Exception("No Alerting.IncidentNote entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (incidentNoteDTO.objectGuid == Guid.Empty)
            {
                incidentNoteDTO.objectGuid = existing.objectGuid;
            }
            else if (incidentNoteDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a IncidentNote record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.IncidentNote cloneOfExisting = (Database.IncidentNote)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new IncidentNote object using the data from the existing record, updated with what is in the DTO.
			//
			Database.IncidentNote incidentNote = (Database.IncidentNote)_context.Entry(existing).GetDatabaseValues().ToObject();
			incidentNote.ApplyDTO(incidentNoteDTO);
			//
			// The tenant guid for any IncidentNote being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the IncidentNote because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				incidentNote.tenantGuid = existing.tenantGuid;
			}

			lock (incidentNotePutSyncRoot)
			{
				//
				// Validate the version number for the incidentNote being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != incidentNote.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "IncidentNote save attempt was made but save request was with version " + incidentNote.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The IncidentNote you are trying to update has already changed.  Please try your save again after reloading the IncidentNote.");
				}
				else
				{
					// Same record.  Increase version.
					incidentNote.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (incidentNote.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Alerting.IncidentNote record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (incidentNote.createdAt.Kind != DateTimeKind.Utc)
				{
					incidentNote.createdAt = incidentNote.createdAt.ToUniversalTime();
				}

				try
				{
				    EntityEntry<Database.IncidentNote> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(incidentNote);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        IncidentNoteChangeHistory incidentNoteChangeHistory = new IncidentNoteChangeHistory();
				        incidentNoteChangeHistory.incidentNoteId = incidentNote.id;
				        incidentNoteChangeHistory.versionNumber = incidentNote.versionNumber;
				        incidentNoteChangeHistory.timeStamp = DateTime.UtcNow;
				        incidentNoteChangeHistory.userId = securityUser.id;
				        incidentNoteChangeHistory.tenantGuid = userTenantGuid;
				        incidentNoteChangeHistory.data = JsonSerializer.Serialize(Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(incidentNote));
				        _context.IncidentNoteChangeHistories.Add(incidentNoteChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.IncidentNote entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(incidentNote)),
						null);

				return Ok(Database.IncidentNote.CreateAnonymous(incidentNote));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.IncidentNote entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(incidentNote)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new IncidentNote record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IncidentNote", Name = "IncidentNote")]
		public async Task<IActionResult> PostIncidentNote([FromBody]Database.IncidentNote.IncidentNoteDTO incidentNoteDTO, CancellationToken cancellationToken = default)
		{
			if (incidentNoteDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Alerting Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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
			// Create a new IncidentNote object using the data from the DTO
			//
			Database.IncidentNote incidentNote = Database.IncidentNote.FromDTO(incidentNoteDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				incidentNote.tenantGuid = userTenantGuid;

				if (incidentNote.createdAt.Kind != DateTimeKind.Utc)
				{
					incidentNote.createdAt = incidentNote.createdAt.ToUniversalTime();
				}

				incidentNote.objectGuid = Guid.NewGuid();
				incidentNote.versionNumber = 1;

				_context.IncidentNotes.Add(incidentNote);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the incidentNote object so that no further changes will be written to the database
				    //
				    _context.Entry(incidentNote).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					incidentNote.IncidentNoteChangeHistories = null;
					incidentNote.incident = null;


				    IncidentNoteChangeHistory incidentNoteChangeHistory = new IncidentNoteChangeHistory();
				    incidentNoteChangeHistory.incidentNoteId = incidentNote.id;
				    incidentNoteChangeHistory.versionNumber = incidentNote.versionNumber;
				    incidentNoteChangeHistory.timeStamp = DateTime.UtcNow;
				    incidentNoteChangeHistory.userId = securityUser.id;
				    incidentNoteChangeHistory.tenantGuid = userTenantGuid;
				    incidentNoteChangeHistory.data = JsonSerializer.Serialize(Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(incidentNote));
				    _context.IncidentNoteChangeHistories.Add(incidentNoteChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Alerting.IncidentNote entity successfully created.",
						true,
						incidentNote. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(incidentNote)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Alerting.IncidentNote entity creation failed.", false, incidentNote.id.ToString(), "", JsonSerializer.Serialize(Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(incidentNote)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "IncidentNote", incidentNote.id, incidentNote.id.ToString()));

			return CreatedAtRoute("IncidentNote", new { id = incidentNote.id }, Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(incidentNote));
		}



        /// <summary>
        /// 
        /// This rolls a IncidentNote entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IncidentNote/Rollback/{id}")]
		[Route("api/IncidentNote/Rollback")]
		public async Task<IActionResult> RollbackToIncidentNoteVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.IncidentNote> query = (from x in _context.IncidentNotes
			        where
			        (x.id == id)
			        select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);


			//
			// Make sure nobody else is editing this IncidentNote concurrently
			//
			lock (incidentNotePutSyncRoot)
			{
				
				Database.IncidentNote incidentNote = query.FirstOrDefault();
				
				if (incidentNote == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.IncidentNote rollback", id.ToString(), new Exception("No Alerting.IncidentNote entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the IncidentNote current state so we can log it.
				//
				Database.IncidentNote cloneOfExisting = (Database.IncidentNote)_context.Entry(incidentNote).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.IncidentNoteChangeHistories = null;
				cloneOfExisting.incident = null;

				if (versionNumber >= incidentNote.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Alerting.IncidentNote rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Alerting.IncidentNote rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				IncidentNoteChangeHistory incidentNoteChangeHistory = (from x in _context.IncidentNoteChangeHistories
				                                               where
				                                               x.incidentNoteId == id &&
				                                               x.versionNumber == versionNumber &&
				                                               x.tenantGuid == userTenantGuid
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (incidentNoteChangeHistory != null)
				{
				    Database.IncidentNote oldIncidentNote = JsonSerializer.Deserialize<Database.IncidentNote>(incidentNoteChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    incidentNote.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    incidentNote.incidentId = oldIncidentNote.incidentId;
				    incidentNote.authorObjectGuid = oldIncidentNote.authorObjectGuid;
				    incidentNote.createdAt = oldIncidentNote.createdAt;
				    incidentNote.content = oldIncidentNote.content;
				    incidentNote.objectGuid = oldIncidentNote.objectGuid;
				    incidentNote.active = oldIncidentNote.active;
				    incidentNote.deleted = oldIncidentNote.deleted;

				    string serializedIncidentNote = JsonSerializer.Serialize(incidentNote);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        IncidentNoteChangeHistory newIncidentNoteChangeHistory = new IncidentNoteChangeHistory();
				        newIncidentNoteChangeHistory.incidentNoteId = incidentNote.id;
				        newIncidentNoteChangeHistory.versionNumber = incidentNote.versionNumber;
				        newIncidentNoteChangeHistory.timeStamp = DateTime.UtcNow;
				        newIncidentNoteChangeHistory.userId = securityUser.id;
				        newIncidentNoteChangeHistory.tenantGuid = userTenantGuid;
				        newIncidentNoteChangeHistory.data = JsonSerializer.Serialize(Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(incidentNote));
				        _context.IncidentNoteChangeHistories.Add(newIncidentNoteChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Alerting.IncidentNote rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(incidentNote)),
						null);


				    return Ok(Database.IncidentNote.CreateAnonymous(incidentNote));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Alerting.IncidentNote rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Alerting.IncidentNote rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a IncidentNote.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the IncidentNote</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IncidentNote/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetIncidentNoteChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
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


			Database.IncidentNote incidentNote = await _context.IncidentNotes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (incidentNote == null)
			{
				return NotFound();
			}

			try
			{
				incidentNote.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.IncidentNote> versionInfo = await incidentNote.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a IncidentNote.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the IncidentNote</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IncidentNote/{id}/AuditHistory")]
		public async Task<IActionResult> GetIncidentNoteAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
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


			Database.IncidentNote incidentNote = await _context.IncidentNotes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (incidentNote == null)
			{
				return NotFound();
			}

			try
			{
				incidentNote.SetupVersionInquiry(_context, userTenantGuid);

				List<VersionInformation<Database.IncidentNote>> versions = await incidentNote.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a IncidentNote.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the IncidentNote</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The IncidentNote object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IncidentNote/{id}/Version/{version}")]
		public async Task<IActionResult> GetIncidentNoteVersion(int id, int version, CancellationToken cancellationToken = default)
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


			Database.IncidentNote incidentNote = await _context.IncidentNotes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (incidentNote == null)
			{
				return NotFound();
			}

			try
			{
				incidentNote.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.IncidentNote> versionInfo = await incidentNote.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a IncidentNote at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the IncidentNote</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The IncidentNote object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IncidentNote/{id}/StateAtTime")]
		public async Task<IActionResult> GetIncidentNoteStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
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


			Database.IncidentNote incidentNote = await _context.IncidentNotes.Where(x => x.id == id
				&& x.tenantGuid == userTenantGuid
			).FirstOrDefaultAsync(cancellationToken);

			if (incidentNote == null)
			{
				return NotFound();
			}

			try
			{
				incidentNote.SetupVersionInquiry(_context, userTenantGuid);

				VersionInformation<Database.IncidentNote> versionInfo = await incidentNote.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a IncidentNote record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/IncidentNote/{id}")]
		[Route("api/IncidentNote")]
		public async Task<IActionResult> DeleteIncidentNote(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Alerting Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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

			IQueryable<Database.IncidentNote> query = (from x in _context.IncidentNotes
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.IncidentNote incidentNote = await query.FirstOrDefaultAsync(cancellationToken);

			if (incidentNote == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Alerting.IncidentNote DELETE", id.ToString(), new Exception("No Alerting.IncidentNote entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.IncidentNote cloneOfExisting = (Database.IncidentNote)_context.Entry(incidentNote).GetDatabaseValues().ToObject();


			lock (incidentNoteDeleteSyncRoot)
			{
			    try
			    {
			        incidentNote.deleted = true;
			        incidentNote.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        IncidentNoteChangeHistory incidentNoteChangeHistory = new IncidentNoteChangeHistory();
			        incidentNoteChangeHistory.incidentNoteId = incidentNote.id;
			        incidentNoteChangeHistory.versionNumber = incidentNote.versionNumber;
			        incidentNoteChangeHistory.timeStamp = DateTime.UtcNow;
			        incidentNoteChangeHistory.userId = securityUser.id;
			        incidentNoteChangeHistory.tenantGuid = userTenantGuid;
			        incidentNoteChangeHistory.data = JsonSerializer.Serialize(Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(incidentNote));
			        _context.IncidentNoteChangeHistories.Add(incidentNoteChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.IncidentNote entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(incidentNote)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Alerting.IncidentNote entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.IncidentNote.CreateAnonymousWithFirstLevelSubObjects(incidentNote)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of IncidentNote records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/IncidentNotes/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? incidentId = null,
			Guid? authorObjectGuid = null,
			DateTime? createdAt = null,
			string content = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 1, cancellationToken);


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
			if (createdAt.HasValue == true && createdAt.Value.Kind != DateTimeKind.Utc)
			{
				createdAt = createdAt.Value.ToUniversalTime();
			}

			IQueryable<Database.IncidentNote> query = (from in_ in _context.IncidentNotes select in_);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (incidentId.HasValue == true)
			{
				query = query.Where(in_ => in_.incidentId == incidentId.Value);
			}
			if (authorObjectGuid.HasValue == true)
			{
				query = query.Where(in_ => in_.authorObjectGuid == authorObjectGuid);
			}
			if (createdAt.HasValue == true)
			{
				query = query.Where(in_ => in_.createdAt == createdAt.Value);
			}
			if (string.IsNullOrEmpty(content) == false)
			{
				query = query.Where(in_ => in_.content == content);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(in_ => in_.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(in_ => in_.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(in_ => in_.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(in_ => in_.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(in_ => in_.deleted == false);
				}
			}
			else
			{
				query = query.Where(in_ => in_.active == true);
				query = query.Where(in_ => in_.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Incident Note, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.content.Contains(anyStringContains)
			       || x.incident.incidentKey.Contains(anyStringContains)
			       || x.incident.title.Contains(anyStringContains)
			       || x.incident.description.Contains(anyStringContains)
			       || x.incident.sourcePayloadJson.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.IncidentNote.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/IncidentNote/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Alerting Writer role needed to write to this table, as well as the minimum write permission level.
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
