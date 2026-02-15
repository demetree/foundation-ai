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
using Foundation.BMC.Database;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This auto generated class provides the basic CRUD operations for the ApiRequestLog entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the ApiRequestLog entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class ApiRequestLogsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 100;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<ApiRequestLogsController> _logger;

		public ApiRequestLogsController(BMCContext context, ILogger<ApiRequestLogsController> logger) : base("BMC", "ApiRequestLog")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of ApiRequestLogs filtered by the parameters provided.
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
		[Route("api/ApiRequestLogs")]
		public async Task<IActionResult> GetApiRequestLogs(
			int? apiKeyId = null,
			string endpoint = null,
			string httpMethod = null,
			int? responseStatus = null,
			DateTime? requestDate = null,
			int? durationMs = null,
			string clientIpAddress = null,
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
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (requestDate.HasValue == true && requestDate.Value.Kind != DateTimeKind.Utc)
			{
				requestDate = requestDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ApiRequestLog> query = (from arl in _context.ApiRequestLogs select arl);
			if (apiKeyId.HasValue == true)
			{
				query = query.Where(arl => arl.apiKeyId == apiKeyId.Value);
			}
			if (string.IsNullOrEmpty(endpoint) == false)
			{
				query = query.Where(arl => arl.endpoint == endpoint);
			}
			if (string.IsNullOrEmpty(httpMethod) == false)
			{
				query = query.Where(arl => arl.httpMethod == httpMethod);
			}
			if (responseStatus.HasValue == true)
			{
				query = query.Where(arl => arl.responseStatus == responseStatus.Value);
			}
			if (requestDate.HasValue == true)
			{
				query = query.Where(arl => arl.requestDate == requestDate.Value);
			}
			if (durationMs.HasValue == true)
			{
				query = query.Where(arl => arl.durationMs == durationMs.Value);
			}
			if (string.IsNullOrEmpty(clientIpAddress) == false)
			{
				query = query.Where(arl => arl.clientIpAddress == clientIpAddress);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(arl => arl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(arl => arl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(arl => arl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(arl => arl.deleted == false);
				}
			}
			else
			{
				query = query.Where(arl => arl.active == true);
				query = query.Where(arl => arl.deleted == false);
			}

			query = query.OrderBy(arl => arl.endpoint).ThenBy(arl => arl.httpMethod).ThenBy(arl => arl.clientIpAddress);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.apiKey);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Api Request Log, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.endpoint.Contains(anyStringContains)
			       || x.httpMethod.Contains(anyStringContains)
			       || x.clientIpAddress.Contains(anyStringContains)
			       || (includeRelations == true && x.apiKey.keyHash.Contains(anyStringContains))
			       || (includeRelations == true && x.apiKey.keyPrefix.Contains(anyStringContains))
			       || (includeRelations == true && x.apiKey.name.Contains(anyStringContains))
			       || (includeRelations == true && x.apiKey.description.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.ApiRequestLog> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.ApiRequestLog apiRequestLog in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(apiRequestLog, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.ApiRequestLog Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.ApiRequestLog Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of ApiRequestLogs filtered by the parameters provided.  Its query is similar to the GetApiRequestLogs method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ApiRequestLogs/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? apiKeyId = null,
			string endpoint = null,
			string httpMethod = null,
			int? responseStatus = null,
			DateTime? requestDate = null,
			int? durationMs = null,
			string clientIpAddress = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			CancellationToken cancellationToken = default)
		{
			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
			//
			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

			//
			// Fix any non-UTC date parameters that come in.
			//
			if (requestDate.HasValue == true && requestDate.Value.Kind != DateTimeKind.Utc)
			{
				requestDate = requestDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ApiRequestLog> query = (from arl in _context.ApiRequestLogs select arl);
			if (apiKeyId.HasValue == true)
			{
				query = query.Where(arl => arl.apiKeyId == apiKeyId.Value);
			}
			if (endpoint != null)
			{
				query = query.Where(arl => arl.endpoint == endpoint);
			}
			if (httpMethod != null)
			{
				query = query.Where(arl => arl.httpMethod == httpMethod);
			}
			if (responseStatus.HasValue == true)
			{
				query = query.Where(arl => arl.responseStatus == responseStatus.Value);
			}
			if (requestDate.HasValue == true)
			{
				query = query.Where(arl => arl.requestDate == requestDate.Value);
			}
			if (durationMs.HasValue == true)
			{
				query = query.Where(arl => arl.durationMs == durationMs.Value);
			}
			if (clientIpAddress != null)
			{
				query = query.Where(arl => arl.clientIpAddress == clientIpAddress);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(arl => arl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(arl => arl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(arl => arl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(arl => arl.deleted == false);
				}
			}
			else
			{
				query = query.Where(arl => arl.active == true);
				query = query.Where(arl => arl.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Api Request Log, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.endpoint.Contains(anyStringContains)
			       || x.httpMethod.Contains(anyStringContains)
			       || x.clientIpAddress.Contains(anyStringContains)
			       || x.apiKey.keyHash.Contains(anyStringContains)
			       || x.apiKey.keyPrefix.Contains(anyStringContains)
			       || x.apiKey.name.Contains(anyStringContains)
			       || x.apiKey.description.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single ApiRequestLog by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ApiRequestLog/{id}")]
		public async Task<IActionResult> GetApiRequestLog(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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
				IQueryable<Database.ApiRequestLog> query = (from arl in _context.ApiRequestLogs where
							(arl.id == id) &&
							(userIsAdmin == true || arl.deleted == false) &&
							(userIsWriter == true || arl.active == true)
					select arl);

				if (includeRelations == true)
				{
					query = query.Include(x => x.apiKey);
					query = query.AsSplitQuery();
				}

				Database.ApiRequestLog materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.ApiRequestLog Entity was read with Admin privilege." : "BMC.ApiRequestLog Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ApiRequestLog", materialized.id, materialized.endpoint));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.ApiRequestLog entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.ApiRequestLog.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.ApiRequestLog.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing ApiRequestLog record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/ApiRequestLog/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutApiRequestLog(int id, [FromBody]Database.ApiRequestLog.ApiRequestLogDTO apiRequestLogDTO, CancellationToken cancellationToken = default)
		{
			if (apiRequestLogDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != apiRequestLogDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.ApiRequestLog> query = (from x in _context.ApiRequestLogs
				where
				(x.id == id)
				select x);


			Database.ApiRequestLog existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ApiRequestLog PUT", id.ToString(), new Exception("No BMC.ApiRequestLog entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (apiRequestLogDTO.objectGuid == Guid.Empty)
            {
                apiRequestLogDTO.objectGuid = existing.objectGuid;
            }
            else if (apiRequestLogDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a ApiRequestLog record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.ApiRequestLog cloneOfExisting = (Database.ApiRequestLog)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new ApiRequestLog object using the data from the existing record, updated with what is in the DTO.
			//
			Database.ApiRequestLog apiRequestLog = (Database.ApiRequestLog)_context.Entry(existing).GetDatabaseValues().ToObject();
			apiRequestLog.ApplyDTO(apiRequestLogDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (apiRequestLog.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.ApiRequestLog record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (apiRequestLog.endpoint != null && apiRequestLog.endpoint.Length > 250)
			{
				apiRequestLog.endpoint = apiRequestLog.endpoint.Substring(0, 250);
			}

			if (apiRequestLog.httpMethod != null && apiRequestLog.httpMethod.Length > 10)
			{
				apiRequestLog.httpMethod = apiRequestLog.httpMethod.Substring(0, 10);
			}

			if (apiRequestLog.requestDate.Kind != DateTimeKind.Utc)
			{
				apiRequestLog.requestDate = apiRequestLog.requestDate.ToUniversalTime();
			}

			if (apiRequestLog.clientIpAddress != null && apiRequestLog.clientIpAddress.Length > 100)
			{
				apiRequestLog.clientIpAddress = apiRequestLog.clientIpAddress.Substring(0, 100);
			}

			EntityEntry<Database.ApiRequestLog> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(apiRequestLog);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.ApiRequestLog entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ApiRequestLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ApiRequestLog.CreateAnonymousWithFirstLevelSubObjects(apiRequestLog)),
					null);


				return Ok(Database.ApiRequestLog.CreateAnonymous(apiRequestLog));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.ApiRequestLog entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ApiRequestLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ApiRequestLog.CreateAnonymousWithFirstLevelSubObjects(apiRequestLog)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new ApiRequestLog record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ApiRequestLog", Name = "ApiRequestLog")]
		public async Task<IActionResult> PostApiRequestLog([FromBody]Database.ApiRequestLog.ApiRequestLogDTO apiRequestLogDTO, CancellationToken cancellationToken = default)
		{
			if (apiRequestLogDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}



			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			//
			// Create a new ApiRequestLog object using the data from the DTO
			//
			Database.ApiRequestLog apiRequestLog = Database.ApiRequestLog.FromDTO(apiRequestLogDTO);

			try
			{
				if (apiRequestLog.endpoint != null && apiRequestLog.endpoint.Length > 250)
				{
					apiRequestLog.endpoint = apiRequestLog.endpoint.Substring(0, 250);
				}

				if (apiRequestLog.httpMethod != null && apiRequestLog.httpMethod.Length > 10)
				{
					apiRequestLog.httpMethod = apiRequestLog.httpMethod.Substring(0, 10);
				}

				if (apiRequestLog.requestDate.Kind != DateTimeKind.Utc)
				{
					apiRequestLog.requestDate = apiRequestLog.requestDate.ToUniversalTime();
				}

				if (apiRequestLog.clientIpAddress != null && apiRequestLog.clientIpAddress.Length > 100)
				{
					apiRequestLog.clientIpAddress = apiRequestLog.clientIpAddress.Substring(0, 100);
				}

				apiRequestLog.objectGuid = Guid.NewGuid();
				_context.ApiRequestLogs.Add(apiRequestLog);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.ApiRequestLog entity successfully created.",
					true,
					apiRequestLog.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.ApiRequestLog.CreateAnonymousWithFirstLevelSubObjects(apiRequestLog)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.ApiRequestLog entity creation failed.", false, apiRequestLog.id.ToString(), "", JsonSerializer.Serialize(apiRequestLog), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "ApiRequestLog", apiRequestLog.id, apiRequestLog.endpoint));

			return CreatedAtRoute("ApiRequestLog", new { id = apiRequestLog.id }, Database.ApiRequestLog.CreateAnonymousWithFirstLevelSubObjects(apiRequestLog));
		}



        /// <summary>
        /// 
        /// This deletes a ApiRequestLog record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/ApiRequestLog/{id}")]
		[Route("api/ApiRequestLog")]
		public async Task<IActionResult> DeleteApiRequestLog(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
			//
			if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			IQueryable<Database.ApiRequestLog> query = (from x in _context.ApiRequestLogs
				where
				(x.id == id)
				select x);


			Database.ApiRequestLog apiRequestLog = await query.FirstOrDefaultAsync(cancellationToken);

			if (apiRequestLog == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.ApiRequestLog DELETE", id.ToString(), new Exception("No BMC.ApiRequestLog entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.ApiRequestLog cloneOfExisting = (Database.ApiRequestLog)_context.Entry(apiRequestLog).GetDatabaseValues().ToObject();


			try
			{
				apiRequestLog.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ApiRequestLog entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.ApiRequestLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ApiRequestLog.CreateAnonymousWithFirstLevelSubObjects(apiRequestLog)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.ApiRequestLog entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.ApiRequestLog.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.ApiRequestLog.CreateAnonymousWithFirstLevelSubObjects(apiRequestLog)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of ApiRequestLog records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/ApiRequestLogs/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? apiKeyId = null,
			string endpoint = null,
			string httpMethod = null,
			int? responseStatus = null,
			DateTime? requestDate = null,
			int? durationMs = null,
			string clientIpAddress = null,
			Guid? objectGuid = null,
			bool? active = null,
			bool? deleted = null,
			string anyStringContains = null,
			int? pageSize = null,
			int? pageNumber = null,
			CancellationToken cancellationToken = default)
		{
			//
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			//
			// Turn any local time kinded parameters to UTC.
			//
			if (requestDate.HasValue == true && requestDate.Value.Kind != DateTimeKind.Utc)
			{
				requestDate = requestDate.Value.ToUniversalTime();
			}

			IQueryable<Database.ApiRequestLog> query = (from arl in _context.ApiRequestLogs select arl);
			if (apiKeyId.HasValue == true)
			{
				query = query.Where(arl => arl.apiKeyId == apiKeyId.Value);
			}
			if (string.IsNullOrEmpty(endpoint) == false)
			{
				query = query.Where(arl => arl.endpoint == endpoint);
			}
			if (string.IsNullOrEmpty(httpMethod) == false)
			{
				query = query.Where(arl => arl.httpMethod == httpMethod);
			}
			if (responseStatus.HasValue == true)
			{
				query = query.Where(arl => arl.responseStatus == responseStatus.Value);
			}
			if (requestDate.HasValue == true)
			{
				query = query.Where(arl => arl.requestDate == requestDate.Value);
			}
			if (durationMs.HasValue == true)
			{
				query = query.Where(arl => arl.durationMs == durationMs.Value);
			}
			if (string.IsNullOrEmpty(clientIpAddress) == false)
			{
				query = query.Where(arl => arl.clientIpAddress == clientIpAddress);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(arl => arl.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(arl => arl.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(arl => arl.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(arl => arl.deleted == false);
				}
			}
			else
			{
				query = query.Where(arl => arl.active == true);
				query = query.Where(arl => arl.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Api Request Log, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.endpoint.Contains(anyStringContains)
			       || x.httpMethod.Contains(anyStringContains)
			       || x.clientIpAddress.Contains(anyStringContains)
			       || x.apiKey.keyHash.Contains(anyStringContains)
			       || x.apiKey.keyPrefix.Contains(anyStringContains)
			       || x.apiKey.name.Contains(anyStringContains)
			       || x.apiKey.description.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.endpoint).ThenBy(x => x.httpMethod).ThenBy(x => x.clientIpAddress);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.ApiRequestLog.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/ApiRequestLog/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Writer role needed to write to this table, as well as the minimum write permission level.
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
