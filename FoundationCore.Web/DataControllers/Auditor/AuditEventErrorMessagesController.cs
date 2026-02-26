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
using Foundation.Auditor.Database;

namespace Foundation.Auditor.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the AuditEventErrorMessage entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AuditEventErrorMessage entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AuditEventErrorMessagesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private AuditorContext _context;

		private ILogger<AuditEventErrorMessagesController> _logger;

		public AuditEventErrorMessagesController(AuditorContext context, ILogger<AuditEventErrorMessagesController> logger) : base("Auditor", "AuditEventErrorMessage")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This gets a list of AuditEventErrorMessages filtered by the parameters provided.
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
		[Route("api/AuditEventErrorMessages")]
		public async Task<IActionResult> GetAuditEventErrorMessages(
			int? auditEventId = null,
			string errorMessage = null,
			int? pageSize = null,
			int? pageNumber = null,
			bool includeRelations = true,
			CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Auditor Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
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

			IQueryable<Database.AuditEventErrorMessage> query = (from aeem in _context.AuditEventErrorMessages select aeem);
			if (auditEventId.HasValue == true)
			{
				query = query.Where(aeem => aeem.auditEventId == auditEventId.Value);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(aeem => aeem.errorMessage == errorMessage);
			}

			query = query.OrderBy(aeem => aeem.id);

			if (includeRelations == true)
			{
				query = query.Include(x => x.auditEvent);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.AuditEventErrorMessage> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AuditEventErrorMessage auditEventErrorMessage in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(auditEventErrorMessage, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Auditor.AuditEventErrorMessage Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Auditor.AuditEventErrorMessage Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
		
*/
		
        /// <summary>
        /// 
        /// This returns a row count of AuditEventErrorMessages filtered by the parameters provided.  Its query is similar to the GetAuditEventErrorMessages method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditEventErrorMessages/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? auditEventId = null,
			string errorMessage = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Auditor Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			IQueryable<Database.AuditEventErrorMessage> query = (from aeem in _context.AuditEventErrorMessages select aeem);
			if (auditEventId.HasValue == true)
			{
				query = query.Where(aeem => aeem.auditEventId == auditEventId.Value);
			}
			if (errorMessage != null)
			{
				query = query.Where(aeem => aeem.errorMessage == errorMessage);
			}

			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single AuditEventErrorMessage by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditEventErrorMessage/{id}")]
		public async Task<IActionResult> GetAuditEventErrorMessage(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Auditor Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.AuditEventErrorMessage> query = (from aeem in _context.AuditEventErrorMessages where
				(aeem.id == id)
					select aeem);

				if (includeRelations == true)
				{
					query = query.Include(x => x.auditEvent);
					query = query.AsSplitQuery();
				}

				Database.AuditEventErrorMessage materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Auditor.AuditEventErrorMessage Entity was read with Admin privilege." : "Auditor.AuditEventErrorMessage Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditEventErrorMessage", materialized.id, materialized.id.ToString()));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Auditor.AuditEventErrorMessage entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Auditor.AuditEventErrorMessage.   Entity was read with Admin privilege." : "Exception caught during entity read of Auditor.AuditEventErrorMessage.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing AuditEventErrorMessage record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AuditEventErrorMessage/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAuditEventErrorMessage(int id, [FromBody]Database.AuditEventErrorMessage.AuditEventErrorMessageDTO auditEventErrorMessageDTO, CancellationToken cancellationToken = default)
		{
			if (auditEventErrorMessageDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Auditor Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != auditEventErrorMessageDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.AuditEventErrorMessage> query = (from x in _context.AuditEventErrorMessages
				where
				(x.id == id)
				select x);


			Database.AuditEventErrorMessage existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditEventErrorMessage PUT", id.ToString(), new Exception("No Auditor.AuditEventErrorMessage entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AuditEventErrorMessage cloneOfExisting = (Database.AuditEventErrorMessage)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AuditEventErrorMessage object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AuditEventErrorMessage auditEventErrorMessage = (Database.AuditEventErrorMessage)_context.Entry(existing).GetDatabaseValues().ToObject();
			auditEventErrorMessage.ApplyDTO(auditEventErrorMessageDTO);


			EntityEntry<Database.AuditEventErrorMessage> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(auditEventErrorMessage);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditEventErrorMessage entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditEventErrorMessage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditEventErrorMessage.CreateAnonymousWithFirstLevelSubObjects(auditEventErrorMessage)),
					null);


				return Ok(Database.AuditEventErrorMessage.CreateAnonymous(auditEventErrorMessage));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditEventErrorMessage entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditEventErrorMessage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditEventErrorMessage.CreateAnonymousWithFirstLevelSubObjects(auditEventErrorMessage)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new AuditEventErrorMessage record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditEventErrorMessage", Name = "AuditEventErrorMessage")]
		public async Task<IActionResult> PostAuditEventErrorMessage([FromBody]Database.AuditEventErrorMessage.AuditEventErrorMessageDTO auditEventErrorMessageDTO, CancellationToken cancellationToken = default)
		{
			if (auditEventErrorMessageDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// Auditor Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new AuditEventErrorMessage object using the data from the DTO
			//
			Database.AuditEventErrorMessage auditEventErrorMessage = Database.AuditEventErrorMessage.FromDTO(auditEventErrorMessageDTO);

			try
			{
				_context.AuditEventErrorMessages.Add(auditEventErrorMessage);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Auditor.AuditEventErrorMessage entity successfully created.",
					true,
					auditEventErrorMessage.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.AuditEventErrorMessage.CreateAnonymousWithFirstLevelSubObjects(auditEventErrorMessage)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Auditor.AuditEventErrorMessage entity creation failed.", false, auditEventErrorMessage.id.ToString(), "", JsonSerializer.Serialize(auditEventErrorMessage), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditEventErrorMessage", auditEventErrorMessage.id, auditEventErrorMessage.id.ToString()));

			return CreatedAtRoute("AuditEventErrorMessage", new { id = auditEventErrorMessage.id }, Database.AuditEventErrorMessage.CreateAnonymousWithFirstLevelSubObjects(auditEventErrorMessage));
		}



        /// <summary>
        /// 
        /// This deletes a AuditEventErrorMessage record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditEventErrorMessage/{id}")]
		[Route("api/AuditEventErrorMessage")]
		public async Task<IActionResult> DeleteAuditEventErrorMessage(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// Auditor Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.AuditEventErrorMessage> query = (from x in _context.AuditEventErrorMessages
				where
				(x.id == id)
				select x);


			Database.AuditEventErrorMessage auditEventErrorMessage = await query.FirstOrDefaultAsync(cancellationToken);

			if (auditEventErrorMessage == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditEventErrorMessage DELETE", id.ToString(), new Exception("No Auditor.AuditEventErrorMessage entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AuditEventErrorMessage cloneOfExisting = (Database.AuditEventErrorMessage)_context.Entry(auditEventErrorMessage).GetDatabaseValues().ToObject();


			try
			{
				_context.AuditEventErrorMessages.Remove(auditEventErrorMessage);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditEventErrorMessage entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditEventErrorMessage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditEventErrorMessage.CreateAnonymousWithFirstLevelSubObjects(auditEventErrorMessage)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditEventErrorMessage entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditEventErrorMessage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditEventErrorMessage.CreateAnonymousWithFirstLevelSubObjects(auditEventErrorMessage)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of AuditEventErrorMessage records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AuditEventErrorMessages/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? auditEventId = null,
			string errorMessage = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// Auditor Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);


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

			IQueryable<Database.AuditEventErrorMessage> query = (from aeem in _context.AuditEventErrorMessages select aeem);
			if (auditEventId.HasValue == true)
			{
				query = query.Where(aeem => aeem.auditEventId == auditEventId.Value);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(aeem => aeem.errorMessage == errorMessage);
			}


			query = query.OrderBy(x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.AuditEventErrorMessage.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AuditEventErrorMessage/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// Auditor Administrator role needed to write to this table.
			//
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
