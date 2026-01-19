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
    /// This auto generated class provides the basic CRUD operations for the ExternalCommunication entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ExternalCommunication entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ExternalCommunicationsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private AuditorContext _context;

		private ILogger<ExternalCommunicationsController> _logger;

		public ExternalCommunicationsController(AuditorContext context, ILogger<ExternalCommunicationsController> logger) : base("Auditor", "ExternalCommunication")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ExternalCommunications filtered by the parameters provided.
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
		[Route("api/ExternalCommunications")]
		public async Task<IActionResult> GetExternalCommunications(
			DateTime? timeStamp = null,
			int? auditUserId = null,
			string communicationType = null,
			string subject = null,
			string message = null,
			bool? completedSuccessfully = null,
			string responseMessage = null,
			string exceptionText = null,
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.ExternalCommunication> query = (from ec in _context.ExternalCommunications select ec);
			if (timeStamp.HasValue == true)
			{
				query = query.Where(ec => ec.timeStamp == timeStamp.Value);
			}
			if (auditUserId.HasValue == true)
			{
				query = query.Where(ec => ec.auditUserId == auditUserId.Value);
			}
			if (string.IsNullOrEmpty(communicationType) == false)
			{
				query = query.Where(ec => ec.communicationType == communicationType);
			}
			if (string.IsNullOrEmpty(subject) == false)
			{
				query = query.Where(ec => ec.subject == subject);
			}
			if (string.IsNullOrEmpty(message) == false)
			{
				query = query.Where(ec => ec.message == message);
			}
			if (completedSuccessfully.HasValue == true)
			{
				query = query.Where(ec => ec.completedSuccessfully == completedSuccessfully.Value);
			}
			if (string.IsNullOrEmpty(responseMessage) == false)
			{
				query = query.Where(ec => ec.responseMessage == responseMessage);
			}
			if (string.IsNullOrEmpty(exceptionText) == false)
			{
				query = query.Where(ec => ec.exceptionText == exceptionText);
			}

			query = query.OrderByDescending(ec => ec.timeStamp);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.auditUser);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the External Communication, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.communicationType.Contains(anyStringContains)
			       || x.subject.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.responseMessage.Contains(anyStringContains)
			       || x.exceptionText.Contains(anyStringContains)
			       || (includeRelations == true && x.auditUser.name.Contains(anyStringContains))
			       || (includeRelations == true && x.auditUser.comments.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ExternalCommunication> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ExternalCommunication externalCommunication in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(externalCommunication, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Auditor.ExternalCommunication Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Auditor.ExternalCommunication Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ExternalCommunications filtered by the parameters provided.  Its query is similar to the GetExternalCommunications method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ExternalCommunications/RowCount")]
		public async Task<IActionResult> GetRowCount(
			DateTime? timeStamp = null,
			int? auditUserId = null,
			string communicationType = null,
			string subject = null,
			string message = null,
			bool? completedSuccessfully = null,
			string responseMessage = null,
			string exceptionText = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.ExternalCommunication> query = (from ec in _context.ExternalCommunications select ec);
			if (timeStamp.HasValue == true)
			{
				query = query.Where(ec => ec.timeStamp == timeStamp.Value);
			}
			if (auditUserId.HasValue == true)
			{
				query = query.Where(ec => ec.auditUserId == auditUserId.Value);
			}
			if (communicationType != null)
			{
				query = query.Where(ec => ec.communicationType == communicationType);
			}
			if (subject != null)
			{
				query = query.Where(ec => ec.subject == subject);
			}
			if (message != null)
			{
				query = query.Where(ec => ec.message == message);
			}
			if (completedSuccessfully.HasValue == true)
			{
				query = query.Where(ec => ec.completedSuccessfully == completedSuccessfully.Value);
			}
			if (responseMessage != null)
			{
				query = query.Where(ec => ec.responseMessage == responseMessage);
			}
			if (exceptionText != null)
			{
				query = query.Where(ec => ec.exceptionText == exceptionText);
			}

			//
			// Add the any string contains parameter to span all the string fields on the External Communication, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.communicationType.Contains(anyStringContains)
			       || x.subject.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.responseMessage.Contains(anyStringContains)
			       || x.exceptionText.Contains(anyStringContains)
			       || x.auditUser.name.Contains(anyStringContains)
			       || x.auditUser.comments.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ExternalCommunication by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ExternalCommunication/{id}")]
		public async Task<IActionResult> GetExternalCommunication(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			try
			{
				IQueryable<Database.ExternalCommunication> query = (from ec in _context.ExternalCommunications where
				(ec.id == id)
					select ec);

				if (includeRelations == true)
				{
					query = query.Include(x => x.auditUser);
					query = query.AsSplitQuery();
				}

				Database.ExternalCommunication materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Auditor.ExternalCommunication Entity was read with Admin privilege." : "Auditor.ExternalCommunication Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ExternalCommunication", materialized.id, materialized.communicationType));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Auditor.ExternalCommunication entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Auditor.ExternalCommunication.   Entity was read with Admin privilege." : "Exception caught during entity read of Auditor.ExternalCommunication.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ExternalCommunication record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ExternalCommunication/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutExternalCommunication(int id, [FromBody]Database.ExternalCommunication.ExternalCommunicationDTO externalCommunicationDTO, CancellationToken cancellationToken = default)
		{
			if (externalCommunicationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != externalCommunicationDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.ExternalCommunication> query = (from x in _context.ExternalCommunications
				where
				(x.id == id)
				select x);


			Database.ExternalCommunication existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.ExternalCommunication PUT", id.ToString(), new Exception("No Auditor.ExternalCommunication entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ExternalCommunication cloneOfExisting = (Database.ExternalCommunication)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ExternalCommunication object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ExternalCommunication externalCommunication = (Database.ExternalCommunication)_context.Entry(existing).GetDatabaseValues().ToObject();
			externalCommunication.ApplyDTO(externalCommunicationDTO);


			if (externalCommunication.timeStamp.HasValue == true && externalCommunication.timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				externalCommunication.timeStamp = externalCommunication.timeStamp.Value.ToUniversalTime();
			}

			if (externalCommunication.communicationType != null && externalCommunication.communicationType.Length > 100)
			{
				externalCommunication.communicationType = externalCommunication.communicationType.Substring(0, 100);
			}

			if (externalCommunication.subject != null && externalCommunication.subject.Length > 2000)
			{
				externalCommunication.subject = externalCommunication.subject.Substring(0, 2000);
			}

			EntityEntry<Database.ExternalCommunication> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(externalCommunication);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Auditor.ExternalCommunication entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ExternalCommunication.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ExternalCommunication.CreateAnonymousWithFirstLevelSubObjects(externalCommunication)),
					null);


				return Ok(Database.ExternalCommunication.CreateAnonymous(externalCommunication));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Auditor.ExternalCommunication entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ExternalCommunication.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ExternalCommunication.CreateAnonymousWithFirstLevelSubObjects(externalCommunication)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ExternalCommunication record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ExternalCommunication", Name = "ExternalCommunication")]
		public async Task<IActionResult> PostExternalCommunication([FromBody]Database.ExternalCommunication.ExternalCommunicationDTO externalCommunicationDTO, CancellationToken cancellationToken = default)
		{
			if (externalCommunicationDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new ExternalCommunication object using the data from the DTO
			//
			Database.ExternalCommunication externalCommunication = Database.ExternalCommunication.FromDTO(externalCommunicationDTO);

			try
			{
				if (externalCommunication.timeStamp.HasValue == true && externalCommunication.timeStamp.Value.Kind != DateTimeKind.Utc)
				{
					externalCommunication.timeStamp = externalCommunication.timeStamp.Value.ToUniversalTime();
				}

				if (externalCommunication.communicationType != null && externalCommunication.communicationType.Length > 100)
				{
					externalCommunication.communicationType = externalCommunication.communicationType.Substring(0, 100);
				}

				if (externalCommunication.subject != null && externalCommunication.subject.Length > 2000)
				{
					externalCommunication.subject = externalCommunication.subject.Substring(0, 2000);
				}

				_context.ExternalCommunications.Add(externalCommunication);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Auditor.ExternalCommunication entity successfully created.",
					true,
					externalCommunication.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ExternalCommunication.CreateAnonymousWithFirstLevelSubObjects(externalCommunication)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Auditor.ExternalCommunication entity creation failed.", false, externalCommunication.id.ToString(), "", JsonSerializer.Serialize(externalCommunication), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ExternalCommunication", externalCommunication.id, externalCommunication.communicationType));

			return CreatedAtRoute("ExternalCommunication", new { id = externalCommunication.id }, Database.ExternalCommunication.CreateAnonymousWithFirstLevelSubObjects(externalCommunication));
		}



        /// <summary>
        /// 
        /// This deletes a ExternalCommunication record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ExternalCommunication/{id}")]
		[Route("api/ExternalCommunication")]
		public async Task<IActionResult> DeleteExternalCommunication(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.ExternalCommunication> query = (from x in _context.ExternalCommunications
				where
				(x.id == id)
				select x);


			Database.ExternalCommunication externalCommunication = await query.FirstOrDefaultAsync(cancellationToken);

			if (externalCommunication == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.ExternalCommunication DELETE", id.ToString(), new Exception("No Auditor.ExternalCommunication entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ExternalCommunication cloneOfExisting = (Database.ExternalCommunication)_context.Entry(externalCommunication).GetDatabaseValues().ToObject();


			try
			{
				_context.ExternalCommunications.Remove(externalCommunication);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.ExternalCommunication entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ExternalCommunication.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ExternalCommunication.CreateAnonymousWithFirstLevelSubObjects(externalCommunication)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.ExternalCommunication entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ExternalCommunication.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ExternalCommunication.CreateAnonymousWithFirstLevelSubObjects(externalCommunication)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ExternalCommunication records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ExternalCommunications/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			DateTime? timeStamp = null,
			int? auditUserId = null,
			string communicationType = null,
			string subject = null,
			string message = null,
			bool? completedSuccessfully = null,
			string responseMessage = null,
			string exceptionText = null,
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
			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);

			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

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
			if (timeStamp.HasValue == true && timeStamp.Value.Kind != DateTimeKind.Utc)
			{
				timeStamp = timeStamp.Value.ToUniversalTime();
			}

			IQueryable<Database.ExternalCommunication> query = (from ec in _context.ExternalCommunications select ec);
			if (timeStamp.HasValue == true)
			{
				query = query.Where(ec => ec.timeStamp == timeStamp.Value);
			}
			if (auditUserId.HasValue == true)
			{
				query = query.Where(ec => ec.auditUserId == auditUserId.Value);
			}
			if (string.IsNullOrEmpty(communicationType) == false)
			{
				query = query.Where(ec => ec.communicationType == communicationType);
			}
			if (string.IsNullOrEmpty(subject) == false)
			{
				query = query.Where(ec => ec.subject == subject);
			}
			if (string.IsNullOrEmpty(message) == false)
			{
				query = query.Where(ec => ec.message == message);
			}
			if (completedSuccessfully.HasValue == true)
			{
				query = query.Where(ec => ec.completedSuccessfully == completedSuccessfully.Value);
			}
			if (string.IsNullOrEmpty(responseMessage) == false)
			{
				query = query.Where(ec => ec.responseMessage == responseMessage);
			}
			if (string.IsNullOrEmpty(exceptionText) == false)
			{
				query = query.Where(ec => ec.exceptionText == exceptionText);
			}


			//
			// Add the any string contains parameter to span all the string fields on the External Communication, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.communicationType.Contains(anyStringContains)
			       || x.subject.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.responseMessage.Contains(anyStringContains)
			       || x.exceptionText.Contains(anyStringContains)
			       || x.auditUser.name.Contains(anyStringContains)
			       || x.auditUser.comments.Contains(anyStringContains)
			   );
			}


			query = query.OrderByDescending (x => x.timeStamp);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ExternalCommunication.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ExternalCommunication/CreateAuditEvent")]
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
