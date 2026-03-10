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
    /// This auto generated class provides the basic CRUD operations for the InvoiceStatus entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the InvoiceStatus entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class InvoiceStatusesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<InvoiceStatusesController> _logger;

		public InvoiceStatusesController(SchedulerContext context, ILogger<InvoiceStatusesController> logger) : base("Scheduler", "InvoiceStatus")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of InvoiceStatuses filtered by the parameters provided.
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
		[Route("api/InvoiceStatuses")]
		public async Task<IActionResult> GetInvoiceStatuses(
			string name = null,
			string description = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
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

			IQueryable<Database.InvoiceStatus> query = (from _is in _context.InvoiceStatuses select _is);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(_is => _is.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(_is => _is.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(_is => _is.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(_is => _is.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(_is => _is.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(_is => _is.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(_is => _is.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(_is => _is.deleted == false);
				}
			}
			else
			{
				query = query.Where(_is => _is.active == true);
				query = query.Where(_is => _is.deleted == false);
			}

			query = query.OrderBy(_is => _is.sequence).ThenBy(_is => _is.name).ThenBy(_is => _is.description).ThenBy(_is => _is.color);


			//
			// Add the any string contains parameter to span all the string fields on the Invoice Status, or on an any of the string fields on its immediate relations
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
			
			List<Database.InvoiceStatus> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.InvoiceStatus invoiceStatus in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(invoiceStatus, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.InvoiceStatus Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.InvoiceStatus Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of InvoiceStatuses filtered by the parameters provided.  Its query is similar to the GetInvoiceStatuses method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/InvoiceStatuses/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.InvoiceStatus> query = (from _is in _context.InvoiceStatuses select _is);
			if (name != null)
			{
				query = query.Where(_is => _is.name == name);
			}
			if (description != null)
			{
				query = query.Where(_is => _is.description == description);
			}
			if (color != null)
			{
				query = query.Where(_is => _is.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(_is => _is.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(_is => _is.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(_is => _is.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(_is => _is.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(_is => _is.deleted == false);
				}
			}
			else
			{
				query = query.Where(_is => _is.active == true);
				query = query.Where(_is => _is.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Invoice Status, or on an any of the string fields on its immediate relations
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
        /// This gets a single InvoiceStatus by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/InvoiceStatus/{id}")]
		public async Task<IActionResult> GetInvoiceStatus(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.InvoiceStatus> query = (from _is in _context.InvoiceStatuses where
							(_is.id == id) &&
							(userIsAdmin == true || _is.deleted == false) &&
							(userIsWriter == true || _is.active == true)
					select _is);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.InvoiceStatus materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.InvoiceStatus Entity was read with Admin privilege." : "Scheduler.InvoiceStatus Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "InvoiceStatus", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.InvoiceStatus entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.InvoiceStatus.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.InvoiceStatus.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing InvoiceStatus record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/InvoiceStatus/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutInvoiceStatus(int id, [FromBody]Database.InvoiceStatus.InvoiceStatusDTO invoiceStatusDTO, CancellationToken cancellationToken = default)
		{
			if (invoiceStatusDTO == null)
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



			if (id != invoiceStatusDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.InvoiceStatus> query = (from x in _context.InvoiceStatuses
				where
				(x.id == id)
				select x);


			Database.InvoiceStatus existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.InvoiceStatus PUT", id.ToString(), new Exception("No Scheduler.InvoiceStatus entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (invoiceStatusDTO.objectGuid == Guid.Empty)
            {
                invoiceStatusDTO.objectGuid = existing.objectGuid;
            }
            else if (invoiceStatusDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a InvoiceStatus record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.InvoiceStatus cloneOfExisting = (Database.InvoiceStatus)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new InvoiceStatus object using the data from the existing record, updated with what is in the DTO.
			//
			Database.InvoiceStatus invoiceStatus = (Database.InvoiceStatus)_context.Entry(existing).GetDatabaseValues().ToObject();
			invoiceStatus.ApplyDTO(invoiceStatusDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (invoiceStatus.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.InvoiceStatus record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (invoiceStatus.name != null && invoiceStatus.name.Length > 100)
			{
				invoiceStatus.name = invoiceStatus.name.Substring(0, 100);
			}

			if (invoiceStatus.description != null && invoiceStatus.description.Length > 500)
			{
				invoiceStatus.description = invoiceStatus.description.Substring(0, 500);
			}

			if (invoiceStatus.color != null && invoiceStatus.color.Length > 10)
			{
				invoiceStatus.color = invoiceStatus.color.Substring(0, 10);
			}

			EntityEntry<Database.InvoiceStatus> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(invoiceStatus);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.InvoiceStatus entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.InvoiceStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.InvoiceStatus.CreateAnonymousWithFirstLevelSubObjects(invoiceStatus)),
					null);


				return Ok(Database.InvoiceStatus.CreateAnonymous(invoiceStatus));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.InvoiceStatus entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.InvoiceStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.InvoiceStatus.CreateAnonymousWithFirstLevelSubObjects(invoiceStatus)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new InvoiceStatus record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/InvoiceStatus", Name = "InvoiceStatus")]
		public async Task<IActionResult> PostInvoiceStatus([FromBody]Database.InvoiceStatus.InvoiceStatusDTO invoiceStatusDTO, CancellationToken cancellationToken = default)
		{
			if (invoiceStatusDTO == null)
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
			// Create a new InvoiceStatus object using the data from the DTO
			//
			Database.InvoiceStatus invoiceStatus = Database.InvoiceStatus.FromDTO(invoiceStatusDTO);

			try
			{
				if (invoiceStatus.name != null && invoiceStatus.name.Length > 100)
				{
					invoiceStatus.name = invoiceStatus.name.Substring(0, 100);
				}

				if (invoiceStatus.description != null && invoiceStatus.description.Length > 500)
				{
					invoiceStatus.description = invoiceStatus.description.Substring(0, 500);
				}

				if (invoiceStatus.color != null && invoiceStatus.color.Length > 10)
				{
					invoiceStatus.color = invoiceStatus.color.Substring(0, 10);
				}

				invoiceStatus.objectGuid = Guid.NewGuid();
				_context.InvoiceStatuses.Add(invoiceStatus);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.InvoiceStatus entity successfully created.",
					true,
					invoiceStatus.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.InvoiceStatus.CreateAnonymousWithFirstLevelSubObjects(invoiceStatus)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.InvoiceStatus entity creation failed.", false, invoiceStatus.id.ToString(), "", JsonSerializer.Serialize(invoiceStatus), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "InvoiceStatus", invoiceStatus.id, invoiceStatus.name));

			return CreatedAtRoute("InvoiceStatus", new { id = invoiceStatus.id }, Database.InvoiceStatus.CreateAnonymousWithFirstLevelSubObjects(invoiceStatus));
		}



        /// <summary>
        /// 
        /// This deletes a InvoiceStatus record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/InvoiceStatus/{id}")]
		[Route("api/InvoiceStatus")]
		public async Task<IActionResult> DeleteInvoiceStatus(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.InvoiceStatus> query = (from x in _context.InvoiceStatuses
				where
				(x.id == id)
				select x);


			Database.InvoiceStatus invoiceStatus = await query.FirstOrDefaultAsync(cancellationToken);

			if (invoiceStatus == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.InvoiceStatus DELETE", id.ToString(), new Exception("No Scheduler.InvoiceStatus entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.InvoiceStatus cloneOfExisting = (Database.InvoiceStatus)_context.Entry(invoiceStatus).GetDatabaseValues().ToObject();


			try
			{
				invoiceStatus.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.InvoiceStatus entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.InvoiceStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.InvoiceStatus.CreateAnonymousWithFirstLevelSubObjects(invoiceStatus)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.InvoiceStatus entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.InvoiceStatus.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.InvoiceStatus.CreateAnonymousWithFirstLevelSubObjects(invoiceStatus)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of InvoiceStatus records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/InvoiceStatuses/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);


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

			IQueryable<Database.InvoiceStatus> query = (from _is in _context.InvoiceStatuses select _is);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(_is => _is.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(_is => _is.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(_is => _is.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(_is => _is.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(_is => _is.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(_is => _is.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(_is => _is.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(_is => _is.deleted == false);
				}
			}
			else
			{
				query = query.Where(_is => _is.active == true);
				query = query.Where(_is => _is.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Invoice Status, or on an any of the string fields on its immediate relations
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
			return Ok(await (from queryData in query select Database.InvoiceStatus.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/InvoiceStatus/CreateAuditEvent")]
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
