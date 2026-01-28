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
    /// This auto generated class provides the basic CRUD operations for the AuditPlanB entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the AuditPlanB entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class AuditPlanBsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 0;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 0;

		private AuditorContext _context;

		private ILogger<AuditPlanBsController> _logger;

		public AuditPlanBsController(AuditorContext context, ILogger<AuditPlanBsController> logger) : base("Auditor", "AuditPlanB")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This gets a list of AuditPlanBs filtered by the parameters provided.
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
		[Route("api/AuditPlanBs")]
		public async Task<IActionResult> GetAuditPlanBs(
			DateTime? startTime = null,
			DateTime? stopTime = null,
			bool? completedSuccessfully = null,
			string user = null,
			string session = null,
			string type = null,
			string accessType = null,
			string source = null,
			string userAgent = null,
			string module = null,
			string moduleEntity = null,
			string resource = null,
			string hostSystem = null,
			string primaryKey = null,
			int? threadId = null,
			string message = null,
			string beforeState = null,
			string afterState = null,
			string errorMessage = null,
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
			if (startTime.HasValue == true && startTime.Value.Kind != DateTimeKind.Utc)
			{
				startTime = startTime.Value.ToUniversalTime();
			}

			if (stopTime.HasValue == true && stopTime.Value.Kind != DateTimeKind.Utc)
			{
				stopTime = stopTime.Value.ToUniversalTime();
			}

			IQueryable<Database.AuditPlanB> query = (from apb in _context.AuditPlanBs select apb);
			if (startTime.HasValue == true)
			{
				query = query.Where(apb => apb.startTime == startTime.Value);
			}
			if (stopTime.HasValue == true)
			{
				query = query.Where(apb => apb.stopTime == stopTime.Value);
			}
			if (completedSuccessfully.HasValue == true)
			{
				query = query.Where(apb => apb.completedSuccessfully == completedSuccessfully.Value);
			}
			if (string.IsNullOrEmpty(user) == false)
			{
				query = query.Where(apb => apb.user == user);
			}
			if (string.IsNullOrEmpty(session) == false)
			{
				query = query.Where(apb => apb.session == session);
			}
			if (string.IsNullOrEmpty(type) == false)
			{
				query = query.Where(apb => apb.type == type);
			}
			if (string.IsNullOrEmpty(accessType) == false)
			{
				query = query.Where(apb => apb.accessType == accessType);
			}
			if (string.IsNullOrEmpty(source) == false)
			{
				query = query.Where(apb => apb.source == source);
			}
			if (string.IsNullOrEmpty(userAgent) == false)
			{
				query = query.Where(apb => apb.userAgent == userAgent);
			}
			if (string.IsNullOrEmpty(module) == false)
			{
				query = query.Where(apb => apb.module == module);
			}
			if (string.IsNullOrEmpty(moduleEntity) == false)
			{
				query = query.Where(apb => apb.moduleEntity == moduleEntity);
			}
			if (string.IsNullOrEmpty(resource) == false)
			{
				query = query.Where(apb => apb.resource == resource);
			}
			if (string.IsNullOrEmpty(hostSystem) == false)
			{
				query = query.Where(apb => apb.hostSystem == hostSystem);
			}
			if (string.IsNullOrEmpty(primaryKey) == false)
			{
				query = query.Where(apb => apb.primaryKey == primaryKey);
			}
			if (threadId.HasValue == true)
			{
				query = query.Where(apb => apb.threadId == threadId.Value);
			}
			if (string.IsNullOrEmpty(message) == false)
			{
				query = query.Where(apb => apb.message == message);
			}
			if (string.IsNullOrEmpty(beforeState) == false)
			{
				query = query.Where(apb => apb.beforeState == beforeState);
			}
			if (string.IsNullOrEmpty(afterState) == false)
			{
				query = query.Where(apb => apb.afterState == afterState);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(apb => apb.errorMessage == errorMessage);
			}
			if (string.IsNullOrEmpty(exceptionText) == false)
			{
				query = query.Where(apb => apb.exceptionText == exceptionText);
			}

			query = query.OrderByDescending(apb => apb.id);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Audit Plan B, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.user.Contains(anyStringContains)
			       || x.session.Contains(anyStringContains)
			       || x.type.Contains(anyStringContains)
			       || x.accessType.Contains(anyStringContains)
			       || x.source.Contains(anyStringContains)
			       || x.userAgent.Contains(anyStringContains)
			       || x.module.Contains(anyStringContains)
			       || x.moduleEntity.Contains(anyStringContains)
			       || x.resource.Contains(anyStringContains)
			       || x.hostSystem.Contains(anyStringContains)
			       || x.primaryKey.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.beforeState.Contains(anyStringContains)
			       || x.afterState.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.exceptionText.Contains(anyStringContains)
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.AuditPlanB> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.AuditPlanB auditPlanB in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(auditPlanB, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Auditor.AuditPlanB Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Auditor.AuditPlanB Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of AuditPlanBs filtered by the parameters provided.  Its query is similar to the GetAuditPlanBs method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditPlanBs/RowCount")]
		public async Task<IActionResult> GetRowCount(
			DateTime? startTime = null,
			DateTime? stopTime = null,
			bool? completedSuccessfully = null,
			string user = null,
			string session = null,
			string type = null,
			string accessType = null,
			string source = null,
			string userAgent = null,
			string module = null,
			string moduleEntity = null,
			string resource = null,
			string hostSystem = null,
			string primaryKey = null,
			int? threadId = null,
			string message = null,
			string beforeState = null,
			string afterState = null,
			string errorMessage = null,
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
			if (startTime.HasValue == true && startTime.Value.Kind != DateTimeKind.Utc)
			{
				startTime = startTime.Value.ToUniversalTime();
			}

			if (stopTime.HasValue == true && stopTime.Value.Kind != DateTimeKind.Utc)
			{
				stopTime = stopTime.Value.ToUniversalTime();
			}

			IQueryable<Database.AuditPlanB> query = (from apb in _context.AuditPlanBs select apb);
			if (startTime.HasValue == true)
			{
				query = query.Where(apb => apb.startTime == startTime.Value);
			}
			if (stopTime.HasValue == true)
			{
				query = query.Where(apb => apb.stopTime == stopTime.Value);
			}
			if (completedSuccessfully.HasValue == true)
			{
				query = query.Where(apb => apb.completedSuccessfully == completedSuccessfully.Value);
			}
			if (user != null)
			{
				query = query.Where(apb => apb.user == user);
			}
			if (session != null)
			{
				query = query.Where(apb => apb.session == session);
			}
			if (type != null)
			{
				query = query.Where(apb => apb.type == type);
			}
			if (accessType != null)
			{
				query = query.Where(apb => apb.accessType == accessType);
			}
			if (source != null)
			{
				query = query.Where(apb => apb.source == source);
			}
			if (userAgent != null)
			{
				query = query.Where(apb => apb.userAgent == userAgent);
			}
			if (module != null)
			{
				query = query.Where(apb => apb.module == module);
			}
			if (moduleEntity != null)
			{
				query = query.Where(apb => apb.moduleEntity == moduleEntity);
			}
			if (resource != null)
			{
				query = query.Where(apb => apb.resource == resource);
			}
			if (hostSystem != null)
			{
				query = query.Where(apb => apb.hostSystem == hostSystem);
			}
			if (primaryKey != null)
			{
				query = query.Where(apb => apb.primaryKey == primaryKey);
			}
			if (threadId.HasValue == true)
			{
				query = query.Where(apb => apb.threadId == threadId.Value);
			}
			if (message != null)
			{
				query = query.Where(apb => apb.message == message);
			}
			if (beforeState != null)
			{
				query = query.Where(apb => apb.beforeState == beforeState);
			}
			if (afterState != null)
			{
				query = query.Where(apb => apb.afterState == afterState);
			}
			if (errorMessage != null)
			{
				query = query.Where(apb => apb.errorMessage == errorMessage);
			}
			if (exceptionText != null)
			{
				query = query.Where(apb => apb.exceptionText == exceptionText);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Audit Plan B, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.user.Contains(anyStringContains)
			       || x.session.Contains(anyStringContains)
			       || x.type.Contains(anyStringContains)
			       || x.accessType.Contains(anyStringContains)
			       || x.source.Contains(anyStringContains)
			       || x.userAgent.Contains(anyStringContains)
			       || x.module.Contains(anyStringContains)
			       || x.moduleEntity.Contains(anyStringContains)
			       || x.resource.Contains(anyStringContains)
			       || x.hostSystem.Contains(anyStringContains)
			       || x.primaryKey.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.beforeState.Contains(anyStringContains)
			       || x.afterState.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.exceptionText.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single AuditPlanB by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditPlanB/{id}")]
		public async Task<IActionResult> GetAuditPlanB(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.AuditPlanB> query = (from apb in _context.AuditPlanBs where
				(apb.id == id)
					select apb);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.AuditPlanB materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Auditor.AuditPlanB Entity was read with Admin privilege." : "Auditor.AuditPlanB Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditPlanB", materialized.id, materialized.message));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Auditor.AuditPlanB entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Auditor.AuditPlanB.   Entity was read with Admin privilege." : "Exception caught during entity read of Auditor.AuditPlanB.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing AuditPlanB record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/AuditPlanB/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutAuditPlanB(int id, [FromBody]Database.AuditPlanB.AuditPlanBDTO auditPlanBDTO, CancellationToken cancellationToken = default)
		{
			if (auditPlanBDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			// Admin privilege needed to write to this table.
			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != auditPlanBDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 0, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.AuditPlanB> query = (from x in _context.AuditPlanBs
				where
				(x.id == id)
				select x);


			Database.AuditPlanB existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditPlanB PUT", id.ToString(), new Exception("No Auditor.AuditPlanB entity could be found with the primary key provided."));
				return NotFound();
			}

			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.AuditPlanB cloneOfExisting = (Database.AuditPlanB)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new AuditPlanB object using the data from the existing record, updated with what is in the DTO.
			//
			Database.AuditPlanB auditPlanB = (Database.AuditPlanB)_context.Entry(existing).GetDatabaseValues().ToObject();
			auditPlanB.ApplyDTO(auditPlanBDTO);


			if (auditPlanB.startTime.Kind != DateTimeKind.Utc)
			{
				auditPlanB.startTime = auditPlanB.startTime.ToUniversalTime();
			}

			if (auditPlanB.stopTime.Kind != DateTimeKind.Utc)
			{
				auditPlanB.stopTime = auditPlanB.stopTime.ToUniversalTime();
			}

			if (auditPlanB.user != null && auditPlanB.user.Length > 100)
			{
				auditPlanB.user = auditPlanB.user.Substring(0, 100);
			}

			if (auditPlanB.session != null && auditPlanB.session.Length > 100)
			{
				auditPlanB.session = auditPlanB.session.Substring(0, 100);
			}

			if (auditPlanB.type != null && auditPlanB.type.Length > 100)
			{
				auditPlanB.type = auditPlanB.type.Substring(0, 100);
			}

			if (auditPlanB.accessType != null && auditPlanB.accessType.Length > 100)
			{
				auditPlanB.accessType = auditPlanB.accessType.Substring(0, 100);
			}

			if (auditPlanB.source != null && auditPlanB.source.Length > 50)
			{
				auditPlanB.source = auditPlanB.source.Substring(0, 50);
			}

			if (auditPlanB.userAgent != null && auditPlanB.userAgent.Length > 100)
			{
				auditPlanB.userAgent = auditPlanB.userAgent.Substring(0, 100);
			}

			if (auditPlanB.module != null && auditPlanB.module.Length > 100)
			{
				auditPlanB.module = auditPlanB.module.Substring(0, 100);
			}

			if (auditPlanB.moduleEntity != null && auditPlanB.moduleEntity.Length > 100)
			{
				auditPlanB.moduleEntity = auditPlanB.moduleEntity.Substring(0, 100);
			}

			if (auditPlanB.resource != null && auditPlanB.resource.Length > 500)
			{
				auditPlanB.resource = auditPlanB.resource.Substring(0, 500);
			}

			if (auditPlanB.hostSystem != null && auditPlanB.hostSystem.Length > 50)
			{
				auditPlanB.hostSystem = auditPlanB.hostSystem.Substring(0, 50);
			}

			if (auditPlanB.primaryKey != null && auditPlanB.primaryKey.Length > 250)
			{
				auditPlanB.primaryKey = auditPlanB.primaryKey.Substring(0, 250);
			}

			EntityEntry<Database.AuditPlanB> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(auditPlanB);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditPlanB entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditPlanB.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditPlanB.CreateAnonymousWithFirstLevelSubObjects(auditPlanB)),
					null);


				return Ok(Database.AuditPlanB.CreateAnonymous(auditPlanB));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Auditor.AuditPlanB entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditPlanB.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditPlanB.CreateAnonymousWithFirstLevelSubObjects(auditPlanB)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new AuditPlanB record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditPlanB", Name = "AuditPlanB")]
		public async Task<IActionResult> PostAuditPlanB([FromBody]Database.AuditPlanB.AuditPlanBDTO auditPlanBDTO, CancellationToken cancellationToken = default)
		{
			if (auditPlanBDTO == null)
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
			// Create a new AuditPlanB object using the data from the DTO
			//
			Database.AuditPlanB auditPlanB = Database.AuditPlanB.FromDTO(auditPlanBDTO);

			try
			{
				if (auditPlanB.startTime.Kind != DateTimeKind.Utc)
				{
					auditPlanB.startTime = auditPlanB.startTime.ToUniversalTime();
				}

				if (auditPlanB.stopTime.Kind != DateTimeKind.Utc)
				{
					auditPlanB.stopTime = auditPlanB.stopTime.ToUniversalTime();
				}

				if (auditPlanB.user != null && auditPlanB.user.Length > 100)
				{
					auditPlanB.user = auditPlanB.user.Substring(0, 100);
				}

				if (auditPlanB.session != null && auditPlanB.session.Length > 100)
				{
					auditPlanB.session = auditPlanB.session.Substring(0, 100);
				}

				if (auditPlanB.type != null && auditPlanB.type.Length > 100)
				{
					auditPlanB.type = auditPlanB.type.Substring(0, 100);
				}

				if (auditPlanB.accessType != null && auditPlanB.accessType.Length > 100)
				{
					auditPlanB.accessType = auditPlanB.accessType.Substring(0, 100);
				}

				if (auditPlanB.source != null && auditPlanB.source.Length > 50)
				{
					auditPlanB.source = auditPlanB.source.Substring(0, 50);
				}

				if (auditPlanB.userAgent != null && auditPlanB.userAgent.Length > 100)
				{
					auditPlanB.userAgent = auditPlanB.userAgent.Substring(0, 100);
				}

				if (auditPlanB.module != null && auditPlanB.module.Length > 100)
				{
					auditPlanB.module = auditPlanB.module.Substring(0, 100);
				}

				if (auditPlanB.moduleEntity != null && auditPlanB.moduleEntity.Length > 100)
				{
					auditPlanB.moduleEntity = auditPlanB.moduleEntity.Substring(0, 100);
				}

				if (auditPlanB.resource != null && auditPlanB.resource.Length > 500)
				{
					auditPlanB.resource = auditPlanB.resource.Substring(0, 500);
				}

				if (auditPlanB.hostSystem != null && auditPlanB.hostSystem.Length > 50)
				{
					auditPlanB.hostSystem = auditPlanB.hostSystem.Substring(0, 50);
				}

				if (auditPlanB.primaryKey != null && auditPlanB.primaryKey.Length > 250)
				{
					auditPlanB.primaryKey = auditPlanB.primaryKey.Substring(0, 250);
				}

				_context.AuditPlanBs.Add(auditPlanB);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Auditor.AuditPlanB entity successfully created.",
					true,
					auditPlanB.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.AuditPlanB.CreateAnonymousWithFirstLevelSubObjects(auditPlanB)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Auditor.AuditPlanB entity creation failed.", false, auditPlanB.id.ToString(), "", JsonSerializer.Serialize(auditPlanB), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "AuditPlanB", auditPlanB.id, auditPlanB.message));

			return CreatedAtRoute("AuditPlanB", new { id = auditPlanB.id }, Database.AuditPlanB.CreateAnonymousWithFirstLevelSubObjects(auditPlanB));
		}



        /// <summary>
        /// 
        /// This deletes a AuditPlanB record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/AuditPlanB/{id}")]
		[Route("api/AuditPlanB")]
		public async Task<IActionResult> DeleteAuditPlanB(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.AuditPlanB> query = (from x in _context.AuditPlanBs
				where
				(x.id == id)
				select x);


			Database.AuditPlanB auditPlanB = await query.FirstOrDefaultAsync(cancellationToken);

			if (auditPlanB == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Auditor.AuditPlanB DELETE", id.ToString(), new Exception("No Auditor.AuditPlanB entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.AuditPlanB cloneOfExisting = (Database.AuditPlanB)_context.Entry(auditPlanB).GetDatabaseValues().ToObject();


			try
			{
				_context.AuditPlanBs.Remove(auditPlanB);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditPlanB entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditPlanB.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditPlanB.CreateAnonymousWithFirstLevelSubObjects(auditPlanB)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Auditor.AuditPlanB entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.AuditPlanB.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.AuditPlanB.CreateAnonymousWithFirstLevelSubObjects(auditPlanB)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of AuditPlanB records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/AuditPlanBs/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			DateTime? startTime = null,
			DateTime? stopTime = null,
			bool? completedSuccessfully = null,
			string user = null,
			string session = null,
			string type = null,
			string accessType = null,
			string source = null,
			string userAgent = null,
			string module = null,
			string moduleEntity = null,
			string resource = null,
			string hostSystem = null,
			string primaryKey = null,
			int? threadId = null,
			string message = null,
			string beforeState = null,
			string afterState = null,
			string errorMessage = null,
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
			if (startTime.HasValue == true && startTime.Value.Kind != DateTimeKind.Utc)
			{
				startTime = startTime.Value.ToUniversalTime();
			}

			if (stopTime.HasValue == true && stopTime.Value.Kind != DateTimeKind.Utc)
			{
				stopTime = stopTime.Value.ToUniversalTime();
			}

			IQueryable<Database.AuditPlanB> query = (from apb in _context.AuditPlanBs select apb);
			if (startTime.HasValue == true)
			{
				query = query.Where(apb => apb.startTime == startTime.Value);
			}
			if (stopTime.HasValue == true)
			{
				query = query.Where(apb => apb.stopTime == stopTime.Value);
			}
			if (completedSuccessfully.HasValue == true)
			{
				query = query.Where(apb => apb.completedSuccessfully == completedSuccessfully.Value);
			}
			if (string.IsNullOrEmpty(user) == false)
			{
				query = query.Where(apb => apb.user == user);
			}
			if (string.IsNullOrEmpty(session) == false)
			{
				query = query.Where(apb => apb.session == session);
			}
			if (string.IsNullOrEmpty(type) == false)
			{
				query = query.Where(apb => apb.type == type);
			}
			if (string.IsNullOrEmpty(accessType) == false)
			{
				query = query.Where(apb => apb.accessType == accessType);
			}
			if (string.IsNullOrEmpty(source) == false)
			{
				query = query.Where(apb => apb.source == source);
			}
			if (string.IsNullOrEmpty(userAgent) == false)
			{
				query = query.Where(apb => apb.userAgent == userAgent);
			}
			if (string.IsNullOrEmpty(module) == false)
			{
				query = query.Where(apb => apb.module == module);
			}
			if (string.IsNullOrEmpty(moduleEntity) == false)
			{
				query = query.Where(apb => apb.moduleEntity == moduleEntity);
			}
			if (string.IsNullOrEmpty(resource) == false)
			{
				query = query.Where(apb => apb.resource == resource);
			}
			if (string.IsNullOrEmpty(hostSystem) == false)
			{
				query = query.Where(apb => apb.hostSystem == hostSystem);
			}
			if (string.IsNullOrEmpty(primaryKey) == false)
			{
				query = query.Where(apb => apb.primaryKey == primaryKey);
			}
			if (threadId.HasValue == true)
			{
				query = query.Where(apb => apb.threadId == threadId.Value);
			}
			if (string.IsNullOrEmpty(message) == false)
			{
				query = query.Where(apb => apb.message == message);
			}
			if (string.IsNullOrEmpty(beforeState) == false)
			{
				query = query.Where(apb => apb.beforeState == beforeState);
			}
			if (string.IsNullOrEmpty(afterState) == false)
			{
				query = query.Where(apb => apb.afterState == afterState);
			}
			if (string.IsNullOrEmpty(errorMessage) == false)
			{
				query = query.Where(apb => apb.errorMessage == errorMessage);
			}
			if (string.IsNullOrEmpty(exceptionText) == false)
			{
				query = query.Where(apb => apb.exceptionText == exceptionText);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Audit Plan B, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.user.Contains(anyStringContains)
			       || x.session.Contains(anyStringContains)
			       || x.type.Contains(anyStringContains)
			       || x.accessType.Contains(anyStringContains)
			       || x.source.Contains(anyStringContains)
			       || x.userAgent.Contains(anyStringContains)
			       || x.module.Contains(anyStringContains)
			       || x.moduleEntity.Contains(anyStringContains)
			       || x.resource.Contains(anyStringContains)
			       || x.hostSystem.Contains(anyStringContains)
			       || x.primaryKey.Contains(anyStringContains)
			       || x.message.Contains(anyStringContains)
			       || x.beforeState.Contains(anyStringContains)
			       || x.afterState.Contains(anyStringContains)
			       || x.errorMessage.Contains(anyStringContains)
			       || x.exceptionText.Contains(anyStringContains)
			   );
			}


			query = query.OrderByDescending (x => x.id);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.AuditPlanB.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/AuditPlanB/CreateAuditEvent")]
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
