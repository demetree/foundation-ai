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
    /// This auto generated class provides the basic CRUD operations for the ExternalCommunicationRecipient entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ExternalCommunicationRecipient entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ExternalCommunicationRecipientsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private AuditorContext _context;

		private ILogger<ExternalCommunicationRecipientsController> _logger;

		public ExternalCommunicationRecipientsController(AuditorContext context, ILogger<ExternalCommunicationRecipientsController> logger) : base("Auditor", "ExternalCommunicationRecipient")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ExternalCommunicationRecipients filtered by the parameters provided.
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
		[Route("api/ExternalCommunicationRecipients")]
		public async Task<IActionResult> GetExternalCommunicationRecipients(
			int? externalCommunicationId = null,
			string recipient = null,
			string type = null,
			int? pageSize = null,
			int? pageNumber = null,
			string anyStringContains = null,
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

			IQueryable<Database.ExternalCommunicationRecipient> query = (from ecr in _context.ExternalCommunicationRecipients select ecr);
			if (externalCommunicationId.HasValue == true)
			{
				query = query.Where(ecr => ecr.externalCommunicationId == externalCommunicationId.Value);
			}
			if (string.IsNullOrEmpty(recipient) == false)
			{
				query = query.Where(ecr => ecr.recipient == recipient);
			}
			if (string.IsNullOrEmpty(type) == false)
			{
				query = query.Where(ecr => ecr.type == type);
			}

			query = query.OrderBy(ecr => ecr.recipient).ThenBy(ecr => ecr.type);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.externalCommunication);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the External Communication Recipient, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.recipient.Contains(anyStringContains)
			       || x.type.Contains(anyStringContains)
			       || (includeRelations == true && x.externalCommunication.communicationType.Contains(anyStringContains))
			       || (includeRelations == true && x.externalCommunication.subject.Contains(anyStringContains))
			       || (includeRelations == true && x.externalCommunication.message.Contains(anyStringContains))
			       || (includeRelations == true && x.externalCommunication.responseMessage.Contains(anyStringContains))
			       || (includeRelations == true && x.externalCommunication.exceptionText.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ExternalCommunicationRecipient> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ExternalCommunicationRecipient externalCommunicationRecipient in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(externalCommunicationRecipient, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Auditor.ExternalCommunicationRecipient Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Auditor.ExternalCommunicationRecipient Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ExternalCommunicationRecipients filtered by the parameters provided.  Its query is similar to the GetExternalCommunicationRecipients method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ExternalCommunicationRecipients/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? externalCommunicationId = null,
			string recipient = null,
			string type = null,
			string anyStringContains = null,
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

			IQueryable<Database.ExternalCommunicationRecipient> query = (from ecr in _context.ExternalCommunicationRecipients select ecr);
			if (externalCommunicationId.HasValue == true)
			{
				query = query.Where(ecr => ecr.externalCommunicationId == externalCommunicationId.Value);
			}
			if (recipient != null)
			{
				query = query.Where(ecr => ecr.recipient == recipient);
			}
			if (type != null)
			{
				query = query.Where(ecr => ecr.type == type);
			}

			//
			// Add the any string contains parameter to span all the string fields on the External Communication Recipient, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.recipient.Contains(anyStringContains)
			       || x.type.Contains(anyStringContains)
			       || x.externalCommunication.communicationType.Contains(anyStringContains)
			       || x.externalCommunication.subject.Contains(anyStringContains)
			       || x.externalCommunication.message.Contains(anyStringContains)
			       || x.externalCommunication.responseMessage.Contains(anyStringContains)
			       || x.externalCommunication.exceptionText.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ExternalCommunicationRecipient by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ExternalCommunicationRecipient/{id}")]
		public async Task<IActionResult> GetExternalCommunicationRecipient(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.ExternalCommunicationRecipient> query = (from ecr in _context.ExternalCommunicationRecipients where
				(ecr.id == id)
					select ecr);

				if (includeRelations == true)
				{
					query = query.Include(x => x.externalCommunication);
					query = query.AsSplitQuery();
				}

				Database.ExternalCommunicationRecipient materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Auditor.ExternalCommunicationRecipient Entity was read with Admin privilege." : "Auditor.ExternalCommunicationRecipient Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ExternalCommunicationRecipient", materialized.id, materialized.recipient));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Auditor.ExternalCommunicationRecipient entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Auditor.ExternalCommunicationRecipient.   Entity was read with Admin privilege." : "Exception caught during entity read of Auditor.ExternalCommunicationRecipient.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ExternalCommunicationRecipient record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ExternalCommunicationRecipient/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutExternalCommunicationRecipient(int id, [FromBody]Database.ExternalCommunicationRecipient.ExternalCommunicationRecipientDTO externalCommunicationRecipientDTO, CancellationToken cancellationToken = default)
		{
			if (externalCommunicationRecipientDTO == null)
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



			if (id != externalCommunicationRecipientDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.ExternalCommunicationRecipient> query = (from x in _context.ExternalCommunicationRecipients
				where
				(x.id == id)
				select x);


			Database.ExternalCommunicationRecipient existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.ExternalCommunicationRecipient PUT", id.ToString(), new Exception("No Auditor.ExternalCommunicationRecipient entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ExternalCommunicationRecipient cloneOfExisting = (Database.ExternalCommunicationRecipient)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ExternalCommunicationRecipient object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ExternalCommunicationRecipient externalCommunicationRecipient = (Database.ExternalCommunicationRecipient)_context.Entry(existing).GetDatabaseValues().ToObject();
			externalCommunicationRecipient.ApplyDTO(externalCommunicationRecipientDTO);


			if (externalCommunicationRecipient.recipient != null && externalCommunicationRecipient.recipient.Length > 100)
			{
				externalCommunicationRecipient.recipient = externalCommunicationRecipient.recipient.Substring(0, 100);
			}

			if (externalCommunicationRecipient.type != null && externalCommunicationRecipient.type.Length > 50)
			{
				externalCommunicationRecipient.type = externalCommunicationRecipient.type.Substring(0, 50);
			}

			EntityEntry<Database.ExternalCommunicationRecipient> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(externalCommunicationRecipient);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Auditor.ExternalCommunicationRecipient entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ExternalCommunicationRecipient.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ExternalCommunicationRecipient.CreateAnonymousWithFirstLevelSubObjects(externalCommunicationRecipient)),
					null);


				return Ok(Database.ExternalCommunicationRecipient.CreateAnonymous(externalCommunicationRecipient));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Auditor.ExternalCommunicationRecipient entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ExternalCommunicationRecipient.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ExternalCommunicationRecipient.CreateAnonymousWithFirstLevelSubObjects(externalCommunicationRecipient)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ExternalCommunicationRecipient record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ExternalCommunicationRecipient", Name = "ExternalCommunicationRecipient")]
		public async Task<IActionResult> PostExternalCommunicationRecipient([FromBody]Database.ExternalCommunicationRecipient.ExternalCommunicationRecipientDTO externalCommunicationRecipientDTO, CancellationToken cancellationToken = default)
		{
			if (externalCommunicationRecipientDTO == null)
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
			// Create a new ExternalCommunicationRecipient object using the data from the DTO
			//
			Database.ExternalCommunicationRecipient externalCommunicationRecipient = Database.ExternalCommunicationRecipient.FromDTO(externalCommunicationRecipientDTO);

			try
			{
				if (externalCommunicationRecipient.recipient != null && externalCommunicationRecipient.recipient.Length > 100)
				{
					externalCommunicationRecipient.recipient = externalCommunicationRecipient.recipient.Substring(0, 100);
				}

				if (externalCommunicationRecipient.type != null && externalCommunicationRecipient.type.Length > 50)
				{
					externalCommunicationRecipient.type = externalCommunicationRecipient.type.Substring(0, 50);
				}

				_context.ExternalCommunicationRecipients.Add(externalCommunicationRecipient);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Auditor.ExternalCommunicationRecipient entity successfully created.",
					true,
					externalCommunicationRecipient.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ExternalCommunicationRecipient.CreateAnonymousWithFirstLevelSubObjects(externalCommunicationRecipient)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Auditor.ExternalCommunicationRecipient entity creation failed.", false, externalCommunicationRecipient.id.ToString(), "", JsonSerializer.Serialize(externalCommunicationRecipient), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ExternalCommunicationRecipient", externalCommunicationRecipient.id, externalCommunicationRecipient.recipient));

			return CreatedAtRoute("ExternalCommunicationRecipient", new { id = externalCommunicationRecipient.id }, Database.ExternalCommunicationRecipient.CreateAnonymousWithFirstLevelSubObjects(externalCommunicationRecipient));
		}



        /// <summary>
        /// 
        /// This deletes a ExternalCommunicationRecipient record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ExternalCommunicationRecipient/{id}")]
		[Route("api/ExternalCommunicationRecipient")]
		public async Task<IActionResult> DeleteExternalCommunicationRecipient(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.ExternalCommunicationRecipient> query = (from x in _context.ExternalCommunicationRecipients
				where
				(x.id == id)
				select x);


			Database.ExternalCommunicationRecipient externalCommunicationRecipient = await query.FirstOrDefaultAsync(cancellationToken);

			if (externalCommunicationRecipient == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.ExternalCommunicationRecipient DELETE", id.ToString(), new Exception("No Auditor.ExternalCommunicationRecipient entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ExternalCommunicationRecipient cloneOfExisting = (Database.ExternalCommunicationRecipient)_context.Entry(externalCommunicationRecipient).GetDatabaseValues().ToObject();


			try
			{
				_context.ExternalCommunicationRecipients.Remove(externalCommunicationRecipient);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.ExternalCommunicationRecipient entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ExternalCommunicationRecipient.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ExternalCommunicationRecipient.CreateAnonymousWithFirstLevelSubObjects(externalCommunicationRecipient)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.ExternalCommunicationRecipient entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ExternalCommunicationRecipient.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ExternalCommunicationRecipient.CreateAnonymousWithFirstLevelSubObjects(externalCommunicationRecipient)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ExternalCommunicationRecipient records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ExternalCommunicationRecipients/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? externalCommunicationId = null,
			string recipient = null,
			string type = null,
			string anyStringContains = null,
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

			IQueryable<Database.ExternalCommunicationRecipient> query = (from ecr in _context.ExternalCommunicationRecipients select ecr);
			if (externalCommunicationId.HasValue == true)
			{
				query = query.Where(ecr => ecr.externalCommunicationId == externalCommunicationId.Value);
			}
			if (string.IsNullOrEmpty(recipient) == false)
			{
				query = query.Where(ecr => ecr.recipient == recipient);
			}
			if (string.IsNullOrEmpty(type) == false)
			{
				query = query.Where(ecr => ecr.type == type);
			}


			//
			// Add the any string contains parameter to span all the string fields on the External Communication Recipient, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.recipient.Contains(anyStringContains)
			       || x.type.Contains(anyStringContains)
			       || x.externalCommunication.communicationType.Contains(anyStringContains)
			       || x.externalCommunication.subject.Contains(anyStringContains)
			       || x.externalCommunication.message.Contains(anyStringContains)
			       || x.externalCommunication.responseMessage.Contains(anyStringContains)
			       || x.externalCommunication.exceptionText.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.recipient).ThenBy(x => x.type);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ExternalCommunicationRecipient.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ExternalCommunicationRecipient/CreateAuditEvent")]
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
