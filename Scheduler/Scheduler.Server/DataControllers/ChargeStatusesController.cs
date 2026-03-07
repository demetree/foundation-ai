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
    /// This auto generated class provides the basic CRUD operations for the ChargeStatus entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ChargeStatus entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ChargeStatusesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 50;

		static object chargeStatusPutSyncRoot = new object();
		static object chargeStatusDeleteSyncRoot = new object();

		private SchedulerContext _context;

		private ILogger<ChargeStatusesController> _logger;

		public ChargeStatusesController(SchedulerContext context, ILogger<ChargeStatusesController> logger) : base("Scheduler", "ChargeStatus")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ChargeStatuses filtered by the parameters provided.
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
		[Route("api/ChargeStatuses")]
		public async Task<IActionResult> GetChargeStatuses(
			string name = null,
			string description = null,
			string color = null,
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

			IQueryable<Database.ChargeStatus> query = (from cs in _context.ChargeStatuses select cs);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(cs => cs.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(cs => cs.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(cs => cs.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(cs => cs.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cs => cs.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cs => cs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cs => cs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cs => cs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cs => cs.deleted == false);
				}
			}
			else
			{
				query = query.Where(cs => cs.active == true);
				query = query.Where(cs => cs.deleted == false);
			}

			query = query.OrderBy(cs => cs.sequence).ThenBy(cs => cs.name).ThenBy(cs => cs.description).ThenBy(cs => cs.color);


			//
			// Add the any string contains parameter to span all the string fields on the Charge Status, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
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
			
			List<Database.ChargeStatus> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ChargeStatus chargeStatus in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(chargeStatus, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.ChargeStatus Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.ChargeStatus Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ChargeStatuses filtered by the parameters provided.  Its query is similar to the GetChargeStatuses method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeStatuses/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			string color = null,
			int? sequence = null,
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

			IQueryable<Database.ChargeStatus> query = (from cs in _context.ChargeStatuses select cs);
			if (name != null)
			{
				query = query.Where(cs => cs.name == name);
			}
			if (description != null)
			{
				query = query.Where(cs => cs.description == description);
			}
			if (color != null)
			{
				query = query.Where(cs => cs.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(cs => cs.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cs => cs.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cs => cs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cs => cs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cs => cs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cs => cs.deleted == false);
				}
			}
			else
			{
				query = query.Where(cs => cs.active == true);
				query = query.Where(cs => cs.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Charge Status, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ChargeStatus by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeStatus/{id}")]
		public async Task<IActionResult> GetChargeStatus(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ChargeStatus> query = (from cs in _context.ChargeStatuses where
							(cs.id == id) &&
							(userIsAdmin == true || cs.deleted == false) &&
							(userIsWriter == true || cs.active == true)
					select cs);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.ChargeStatus materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.ChargeStatus Entity was read with Admin privilege." : "Scheduler.ChargeStatus Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ChargeStatus", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.ChargeStatus entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.ChargeStatus.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.ChargeStatus.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ChargeStatus record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ChargeStatus/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutChargeStatus(int id, [FromBody]Database.ChargeStatus.ChargeStatusDTO chargeStatusDTO, CancellationToken cancellationToken = default)
		{
			if (chargeStatusDTO == null)
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



			if (id != chargeStatusDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 50, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.ChargeStatus> query = (from x in _context.ChargeStatuses
				where
				(x.id == id)
				select x);


			Database.ChargeStatus existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ChargeStatus PUT", id.ToString(), new Exception("No Scheduler.ChargeStatus entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (chargeStatusDTO.objectGuid == Guid.Empty)
            {
                chargeStatusDTO.objectGuid = existing.objectGuid;
            }
            else if (chargeStatusDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ChargeStatus record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ChargeStatus cloneOfExisting = (Database.ChargeStatus)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ChargeStatus object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ChargeStatus chargeStatus = (Database.ChargeStatus)_context.Entry(existing).GetDatabaseValues().ToObject();
			chargeStatus.ApplyDTO(chargeStatusDTO);
			lock (chargeStatusPutSyncRoot)
			{
				//
				// Validate the version number for the chargeStatus being saved.  Error out if the database version is different than what is being saved.  If they are the same, then increment the version for this save.
				//
				if (existing.versionNumber != chargeStatus.versionNumber)
				{
					// Record has changed
					CreateAuditEvent(AuditEngine.AuditType.Miscellaneous, "ChargeStatus save attempt was made but save request was with version " + chargeStatus.versionNumber + " and the current version number is " + existing.versionNumber, false);
					return Problem("The ChargeStatus you are trying to update has already changed.  Please try your save again after reloading the ChargeStatus.");
				}
				else
				{
					// Same record.  Increase version.
					chargeStatus.versionNumber++;
				}


				// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
				if (userIsAdmin == false && (chargeStatus.deleted == true || existing.deleted == true))
				{
					// we're not recording state here because it is not being changed.
					CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.ChargeStatus record.", id.ToString());
					DestroySessionAndAuthentication();
					return Forbid();
				}

				if (chargeStatus.name != null && chargeStatus.name.Length > 100)
				{
					chargeStatus.name = chargeStatus.name.Substring(0, 100);
				}

				if (chargeStatus.description != null && chargeStatus.description.Length > 500)
				{
					chargeStatus.description = chargeStatus.description.Substring(0, 500);
				}

				if (chargeStatus.color != null && chargeStatus.color.Length > 10)
				{
					chargeStatus.color = chargeStatus.color.Substring(0, 10);
				}

				try
				{
				    EntityEntry<Database.ChargeStatus> attached = _context.Entry(existing);
				    attached.CurrentValues.SetValues(chargeStatus);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {
				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ChargeStatusChangeHistory chargeStatusChangeHistory = new ChargeStatusChangeHistory();
				        chargeStatusChangeHistory.chargeStatusId = chargeStatus.id;
				        chargeStatusChangeHistory.versionNumber = chargeStatus.versionNumber;
				        chargeStatusChangeHistory.timeStamp = DateTime.UtcNow;
				        chargeStatusChangeHistory.userId = securityUser.id;
				        chargeStatusChangeHistory.data = JsonSerializer.Serialize(Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(chargeStatus));
				        _context.ChargeStatusChangeHistories.Add(chargeStatusChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ChargeStatus entity successfully updated.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(chargeStatus)),
						null);

				return Ok(Database.ChargeStatus.CreateAnonymous(chargeStatus));
				}
				catch (Exception ex)
				{
					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ChargeStatus entity update failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(chargeStatus)),
						ex);

					return Problem(ex.Message);
				}

			}
		}

        /// <summary>
        /// 
        /// This creates a new ChargeStatus record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeStatus", Name = "ChargeStatus")]
		public async Task<IActionResult> PostChargeStatus([FromBody]Database.ChargeStatus.ChargeStatusDTO chargeStatusDTO, CancellationToken cancellationToken = default)
		{
			if (chargeStatusDTO == null)
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

			//
			// Create a new ChargeStatus object using the data from the DTO
			//
			Database.ChargeStatus chargeStatus = Database.ChargeStatus.FromDTO(chargeStatusDTO);

			try
			{
				if (chargeStatus.name != null && chargeStatus.name.Length > 100)
				{
					chargeStatus.name = chargeStatus.name.Substring(0, 100);
				}

				if (chargeStatus.description != null && chargeStatus.description.Length > 500)
				{
					chargeStatus.description = chargeStatus.description.Substring(0, 500);
				}

				if (chargeStatus.color != null && chargeStatus.color.Length > 10)
				{
					chargeStatus.color = chargeStatus.color.Substring(0, 10);
				}

				chargeStatus.objectGuid = Guid.NewGuid();
				chargeStatus.versionNumber = 1;

				_context.ChargeStatuses.Add(chargeStatus);

				await using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
				{
				    await _context.SaveChangesAsync(cancellationToken);

				    //
				    // Now add the change history
				    //

				    //
				    // Detach the chargeStatus object so that no further changes will be written to the database
				    //
				    _context.Entry(chargeStatus).State = EntityState.Detached;

				    //
				    // Nullify all object properties before serializing.
				    //
					chargeStatus.ChargeStatusChangeHistories = null;
					chargeStatus.EventCharges = null;


				    ChargeStatusChangeHistory chargeStatusChangeHistory = new ChargeStatusChangeHistory();
				    chargeStatusChangeHistory.chargeStatusId = chargeStatus.id;
				    chargeStatusChangeHistory.versionNumber = chargeStatus.versionNumber;
				    chargeStatusChangeHistory.timeStamp = DateTime.UtcNow;
				    chargeStatusChangeHistory.userId = securityUser.id;
				    chargeStatusChangeHistory.data = JsonSerializer.Serialize(Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(chargeStatus));
				    _context.ChargeStatusChangeHistories.Add(chargeStatusChangeHistory);
				    await _context.SaveChangesAsync(cancellationToken);

				    await transaction.CommitAsync(cancellationToken);

					await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
						"Scheduler.ChargeStatus entity successfully created.",
						true,
						chargeStatus. id.ToString(),
						"",
						JsonSerializer.Serialize(Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(chargeStatus)),
						null);


				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.ChargeStatus entity creation failed.", false, chargeStatus.id.ToString(), "", JsonSerializer.Serialize(Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(chargeStatus)), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ChargeStatus", chargeStatus.id, chargeStatus.name));

			return CreatedAtRoute("ChargeStatus", new { id = chargeStatus.id }, Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(chargeStatus));
		}



        /// <summary>
        /// 
        /// This rolls a ChargeStatus entity back to the state it was in at a prior version number.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPut]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeStatus/Rollback/{id}")]
		[Route("api/ChargeStatus/Rollback")]
		public async Task<IActionResult> RollbackToChargeStatusVersion(int id, int versionNumber, CancellationToken cancellationToken = default)
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

			

			
			IQueryable <Database.ChargeStatus> query = (from x in _context.ChargeStatuses
			        where
			        (x.id == id)
			        select x);


			//
			// Make sure nobody else is editing this ChargeStatus concurrently
			//
			lock (chargeStatusPutSyncRoot)
			{
				
				Database.ChargeStatus chargeStatus = query.FirstOrDefault();
				
				if (chargeStatus == null)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ChargeStatus rollback", id.ToString(), new Exception("No Scheduler.ChargeStatus entity could be find with the primary key provided for the rollback operation."));
				    return NotFound();
				}
				
				//
				// Make a copy of the ChargeStatus current state so we can log it.
				//
				Database.ChargeStatus cloneOfExisting = (Database.ChargeStatus)_context.Entry(chargeStatus).GetDatabaseValues().ToObject();
				
				//
				// Remove any object fields from the clone object so that it can serialize effectively
				//
				cloneOfExisting.ChargeStatusChangeHistories = null;
				cloneOfExisting.EventCharges = null;

				if (versionNumber >= chargeStatus.versionNumber)
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Invalid version number provided for Scheduler.ChargeStatus rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Invalid version number provided for Scheduler.ChargeStatus rollback operation.Version number provided is " + versionNumber));
				    return NotFound();
				}
				
				ChargeStatusChangeHistory chargeStatusChangeHistory = (from x in _context.ChargeStatusChangeHistories
				                                               where
				                                               x.chargeStatusId == id &&
				                                               x.versionNumber == versionNumber
				                                               select x)
				                                               .AsNoTracking()
				                                               .FirstOrDefault();

				if (chargeStatusChangeHistory != null)
				{
				    Database.ChargeStatus oldChargeStatus = JsonSerializer.Deserialize<Database.ChargeStatus>(chargeStatusChangeHistory.data);
				
				    //
				    // Increase the version number
				    //
				    chargeStatus.versionNumber++;
				
				    //
				    // Put all other fields back the way that they were 
				    //
				    chargeStatus.name = oldChargeStatus.name;
				    chargeStatus.description = oldChargeStatus.description;
				    chargeStatus.color = oldChargeStatus.color;
				    chargeStatus.sequence = oldChargeStatus.sequence;
				    chargeStatus.objectGuid = oldChargeStatus.objectGuid;
				    chargeStatus.active = oldChargeStatus.active;
				    chargeStatus.deleted = oldChargeStatus.deleted;

				    string serializedChargeStatus = JsonSerializer.Serialize(chargeStatus);

				    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				    {

				        _context.SaveChanges();

				        //
				        // Now add the change history
				        //
				        ChargeStatusChangeHistory newChargeStatusChangeHistory = new ChargeStatusChangeHistory();
				        newChargeStatusChangeHistory.chargeStatusId = chargeStatus.id;
				        newChargeStatusChangeHistory.versionNumber = chargeStatus.versionNumber;
				        newChargeStatusChangeHistory.timeStamp = DateTime.UtcNow;
				        newChargeStatusChangeHistory.userId = securityUser.id;
				        newChargeStatusChangeHistory.data = JsonSerializer.Serialize(Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(chargeStatus));
				        _context.ChargeStatusChangeHistories.Add(newChargeStatusChangeHistory);

				        _context.SaveChanges();

				        transaction.Commit();
				    }

					CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
						"Scheduler.ChargeStatus rollback process successfully rolled back to version number " + versionNumber,
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(chargeStatus)),
						null);


				    return Ok(Database.ChargeStatus.CreateAnonymous(chargeStatus));
				}
				else
				{
				    CreateAuditEvent(AuditEngine.AuditType.UpdateEntity, "Could not find version number provided for Scheduler.ChargeStatus rollback.  Version number provided is " + versionNumber, id.ToString(), new Exception("Could not find version number provided for Scheduler.ChargeStatus rollback.  Version number provided is " + versionNumber));

				    return BadRequest();
				}
			}
		}



        /// <summary>
        /// 
        /// Gets the change metadata (version info, timestamp, user) for a specific version of a ChargeStatus.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ChargeStatus</param>
        /// <param name="versionNumber">The version number to retrieve metadata for</param>
        /// <returns>VersionInformation containing timestamp and user details</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeStatus/{id}/ChangeMetadata")]
		public async Task<IActionResult> GetChargeStatusChangeMetadata(int id, int versionNumber, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.ChargeStatus chargeStatus = await _context.ChargeStatuses.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (chargeStatus == null)
			{
				return NotFound();
			}

			try
			{
				chargeStatus.SetupVersionInquiry(_context, Guid.Empty);

				VersionInformation<Database.ChargeStatus> versionInfo = await chargeStatus.GetVersionAsync(versionNumber, includeData: false, cancellationToken).ConfigureAwait(false);

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
        /// Gets the full audit history for a ChargeStatus.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ChargeStatus</param>
        /// <param name="includeData">Whether to include the full entity data for each version (can be large)</param>
        /// <returns>List of VersionInformation items</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeStatus/{id}/AuditHistory")]
		public async Task<IActionResult> GetChargeStatusAuditHistory(int id, bool includeData = false, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.ChargeStatus chargeStatus = await _context.ChargeStatuses.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (chargeStatus == null)
			{
				return NotFound();
			}

			try
			{
				chargeStatus.SetupVersionInquiry(_context, Guid.Empty);

				List<VersionInformation<Database.ChargeStatus>> versions = await chargeStatus.GetAllVersionsAsync(includeData: includeData, cancellationToken).ConfigureAwait(false);

				return Ok(versions);
			}
			catch (Exception ex)
			{
				return Problem(ex.Message);
			}
		}



        /// <summary>
        /// 
        /// Gets a specific version of a ChargeStatus.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ChargeStatus</param>
        /// <param name="version">The version number to retrieve</param>
        /// <returns>The ChargeStatus object at that version</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeStatus/{id}/Version/{version}")]
		public async Task<IActionResult> GetChargeStatusVersion(int id, int version, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.ChargeStatus chargeStatus = await _context.ChargeStatuses.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (chargeStatus == null)
			{
				return NotFound();
			}

			try
			{
				chargeStatus.SetupVersionInquiry(_context, Guid.Empty);

				VersionInformation<Database.ChargeStatus> versionInfo = await chargeStatus.GetVersionAsync(version, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// Gets the state of a ChargeStatus at a specific point in time.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
        /// <param name="id">The primary key of the ChargeStatus</param>
        /// <param name="time">The point in time (ISO format, UTC)</param>
        /// <returns>The ChargeStatus object at that time</returns>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeStatus/{id}/StateAtTime")]
		public async Task<IActionResult> GetChargeStatusStateAtTime(int id, DateTime time, CancellationToken cancellationToken = default)
		{

			//
			// Scheduler Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			Database.ChargeStatus chargeStatus = await _context.ChargeStatuses.Where(x => x.id == id
			).FirstOrDefaultAsync(cancellationToken);

			if (chargeStatus == null)
			{
				return NotFound();
			}

			try
			{
				chargeStatus.SetupVersionInquiry(_context, Guid.Empty);

				VersionInformation<Database.ChargeStatus> versionInfo = await chargeStatus.GetVersionAtTimeAsync(time, includeData: true, cancellationToken).ConfigureAwait(false);

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
        /// This deletes a ChargeStatus record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ChargeStatus/{id}")]
		[Route("api/ChargeStatus")]
		public async Task<IActionResult> DeleteChargeStatus(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ChargeStatus> query = (from x in _context.ChargeStatuses
				where
				(x.id == id)
				select x);


			Database.ChargeStatus chargeStatus = await query.FirstOrDefaultAsync(cancellationToken);

			if (chargeStatus == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.ChargeStatus DELETE", id.ToString(), new Exception("No Scheduler.ChargeStatus entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ChargeStatus cloneOfExisting = (Database.ChargeStatus)_context.Entry(chargeStatus).GetDatabaseValues().ToObject();


			lock (chargeStatusDeleteSyncRoot)
			{
			    try
			    {
			        chargeStatus.deleted = true;
			        chargeStatus.versionNumber++;

			        _context.SaveChanges();

			        //
			        // Now add the change history
			        //
			        ChargeStatusChangeHistory chargeStatusChangeHistory = new ChargeStatusChangeHistory();
			        chargeStatusChangeHistory.chargeStatusId = chargeStatus.id;
			        chargeStatusChangeHistory.versionNumber = chargeStatus.versionNumber;
			        chargeStatusChangeHistory.timeStamp = DateTime.UtcNow;
			        chargeStatusChangeHistory.userId = securityUser.id;
			        chargeStatusChangeHistory.data = JsonSerializer.Serialize(Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(chargeStatus));
			        _context.ChargeStatusChangeHistories.Add(chargeStatusChangeHistory);

			        _context.SaveChanges();

					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ChargeStatus entity successfully deleted.",
						true,
						id.ToString(),
						JsonSerializer.Serialize(Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(chargeStatus)),
						null);

			    }
			    catch (Exception ex)
			    {
					CreateAuditEvent(AuditEngine.AuditType.DeleteEntity,
						"Scheduler.ChargeStatus entity delete failed",
						false,
						id.ToString(),
						JsonSerializer.Serialize(Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
						JsonSerializer.Serialize(Database.ChargeStatus.CreateAnonymousWithFirstLevelSubObjects(chargeStatus)),
						ex);

			        return Problem(ex.Message);
			    }
			    return Ok();
			}
		}


        /// <summary>
        /// 
        /// This gets a list of ChargeStatus records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ChargeStatuses/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			string color = null,
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

			IQueryable<Database.ChargeStatus> query = (from cs in _context.ChargeStatuses select cs);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(cs => cs.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(cs => cs.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(cs => cs.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(cs => cs.sequence == sequence.Value);
			}
			if (versionNumber.HasValue == true)
			{
				query = query.Where(cs => cs.versionNumber == versionNumber.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(cs => cs.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(cs => cs.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(cs => cs.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(cs => cs.deleted == false);
				}
			}
			else
			{
				query = query.Where(cs => cs.active == true);
				query = query.Where(cs => cs.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Charge Status, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.name.Contains(anyStringContains)
			       || x.description.Contains(anyStringContains)
			       || x.color.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.name).ThenBy(x => x.description).ThenBy(x => x.color);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ChargeStatus.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ChargeStatus/CreateAuditEvent")]
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
