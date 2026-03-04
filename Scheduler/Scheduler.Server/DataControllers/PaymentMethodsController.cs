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
    /// This auto generated class provides the basic CRUD operations for the PaymentMethod entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the PaymentMethod entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PaymentMethodsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<PaymentMethodsController> _logger;

		public PaymentMethodsController(SchedulerContext context, ILogger<PaymentMethodsController> logger) : base("Scheduler", "PaymentMethod")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of PaymentMethods filtered by the parameters provided.
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
		[Route("api/PaymentMethods")]
		public async Task<IActionResult> GetPaymentMethods(
			string name = null,
			string description = null,
			bool? isElectronic = null,
			int? sequence = null,
			string color = null,
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

			IQueryable<Database.PaymentMethod> query = (from pm in _context.PaymentMethods select pm);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pm => pm.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(pm => pm.description == description);
			}
			if (isElectronic.HasValue == true)
			{
				query = query.Where(pm => pm.isElectronic == isElectronic.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(pm => pm.sequence == sequence.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(pm => pm.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pm => pm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pm => pm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pm => pm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pm => pm.deleted == false);
				}
			}
			else
			{
				query = query.Where(pm => pm.active == true);
				query = query.Where(pm => pm.deleted == false);
			}

			query = query.OrderBy(pm => pm.sequence).ThenBy(pm => pm.name).ThenBy(pm => pm.description).ThenBy(pm => pm.color);


			//
			// Add the any string contains parameter to span all the string fields on the Payment Method, or on an any of the string fields on its immediate relations
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
			
			List<Database.PaymentMethod> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.PaymentMethod paymentMethod in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(paymentMethod, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.PaymentMethod Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.PaymentMethod Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of PaymentMethods filtered by the parameters provided.  Its query is similar to the GetPaymentMethods method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentMethods/RowCount")]
		public async Task<IActionResult> GetRowCount(
			string name = null,
			string description = null,
			bool? isElectronic = null,
			int? sequence = null,
			string color = null,
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

			IQueryable<Database.PaymentMethod> query = (from pm in _context.PaymentMethods select pm);
			if (name != null)
			{
				query = query.Where(pm => pm.name == name);
			}
			if (description != null)
			{
				query = query.Where(pm => pm.description == description);
			}
			if (isElectronic.HasValue == true)
			{
				query = query.Where(pm => pm.isElectronic == isElectronic.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(pm => pm.sequence == sequence.Value);
			}
			if (color != null)
			{
				query = query.Where(pm => pm.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pm => pm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pm => pm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pm => pm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pm => pm.deleted == false);
				}
			}
			else
			{
				query = query.Where(pm => pm.active == true);
				query = query.Where(pm => pm.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Payment Method, or on an any of the string fields on its immediate relations
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
        /// This gets a single PaymentMethod by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentMethod/{id}")]
		public async Task<IActionResult> GetPaymentMethod(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.PaymentMethod> query = (from pm in _context.PaymentMethods where
							(pm.id == id) &&
							(userIsAdmin == true || pm.deleted == false) &&
							(userIsWriter == true || pm.active == true)
					select pm);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.PaymentMethod materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.PaymentMethod Entity was read with Admin privilege." : "Scheduler.PaymentMethod Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PaymentMethod", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.PaymentMethod entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.PaymentMethod.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.PaymentMethod.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing PaymentMethod record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/PaymentMethod/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPaymentMethod(int id, [FromBody]Database.PaymentMethod.PaymentMethodDTO paymentMethodDTO, CancellationToken cancellationToken = default)
		{
			if (paymentMethodDTO == null)
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



			if (id != paymentMethodDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.PaymentMethod> query = (from x in _context.PaymentMethods
				where
				(x.id == id)
				select x);


			Database.PaymentMethod existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PaymentMethod PUT", id.ToString(), new Exception("No Scheduler.PaymentMethod entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (paymentMethodDTO.objectGuid == Guid.Empty)
            {
                paymentMethodDTO.objectGuid = existing.objectGuid;
            }
            else if (paymentMethodDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a PaymentMethod record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.PaymentMethod cloneOfExisting = (Database.PaymentMethod)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new PaymentMethod object using the data from the existing record, updated with what is in the DTO.
			//
			Database.PaymentMethod paymentMethod = (Database.PaymentMethod)_context.Entry(existing).GetDatabaseValues().ToObject();
			paymentMethod.ApplyDTO(paymentMethodDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (paymentMethod.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.PaymentMethod record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (paymentMethod.name != null && paymentMethod.name.Length > 100)
			{
				paymentMethod.name = paymentMethod.name.Substring(0, 100);
			}

			if (paymentMethod.description != null && paymentMethod.description.Length > 500)
			{
				paymentMethod.description = paymentMethod.description.Substring(0, 500);
			}

			if (paymentMethod.color != null && paymentMethod.color.Length > 10)
			{
				paymentMethod.color = paymentMethod.color.Substring(0, 10);
			}

			EntityEntry<Database.PaymentMethod> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(paymentMethod);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.PaymentMethod entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PaymentMethod.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PaymentMethod.CreateAnonymousWithFirstLevelSubObjects(paymentMethod)),
					null);


				return Ok(Database.PaymentMethod.CreateAnonymous(paymentMethod));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.PaymentMethod entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PaymentMethod.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PaymentMethod.CreateAnonymousWithFirstLevelSubObjects(paymentMethod)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new PaymentMethod record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentMethod", Name = "PaymentMethod")]
		public async Task<IActionResult> PostPaymentMethod([FromBody]Database.PaymentMethod.PaymentMethodDTO paymentMethodDTO, CancellationToken cancellationToken = default)
		{
			if (paymentMethodDTO == null)
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
			// Create a new PaymentMethod object using the data from the DTO
			//
			Database.PaymentMethod paymentMethod = Database.PaymentMethod.FromDTO(paymentMethodDTO);

			try
			{
				if (paymentMethod.name != null && paymentMethod.name.Length > 100)
				{
					paymentMethod.name = paymentMethod.name.Substring(0, 100);
				}

				if (paymentMethod.description != null && paymentMethod.description.Length > 500)
				{
					paymentMethod.description = paymentMethod.description.Substring(0, 500);
				}

				if (paymentMethod.color != null && paymentMethod.color.Length > 10)
				{
					paymentMethod.color = paymentMethod.color.Substring(0, 10);
				}

				paymentMethod.objectGuid = Guid.NewGuid();
				_context.PaymentMethods.Add(paymentMethod);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.PaymentMethod entity successfully created.",
					true,
					paymentMethod.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.PaymentMethod.CreateAnonymousWithFirstLevelSubObjects(paymentMethod)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.PaymentMethod entity creation failed.", false, paymentMethod.id.ToString(), "", JsonSerializer.Serialize(paymentMethod), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PaymentMethod", paymentMethod.id, paymentMethod.name));

			return CreatedAtRoute("PaymentMethod", new { id = paymentMethod.id }, Database.PaymentMethod.CreateAnonymousWithFirstLevelSubObjects(paymentMethod));
		}



        /// <summary>
        /// 
        /// This deletes a PaymentMethod record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PaymentMethod/{id}")]
		[Route("api/PaymentMethod")]
		public async Task<IActionResult> DeletePaymentMethod(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.PaymentMethod> query = (from x in _context.PaymentMethods
				where
				(x.id == id)
				select x);


			Database.PaymentMethod paymentMethod = await query.FirstOrDefaultAsync(cancellationToken);

			if (paymentMethod == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.PaymentMethod DELETE", id.ToString(), new Exception("No Scheduler.PaymentMethod entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.PaymentMethod cloneOfExisting = (Database.PaymentMethod)_context.Entry(paymentMethod).GetDatabaseValues().ToObject();


			try
			{
				paymentMethod.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.PaymentMethod entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PaymentMethod.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PaymentMethod.CreateAnonymousWithFirstLevelSubObjects(paymentMethod)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.PaymentMethod entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PaymentMethod.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PaymentMethod.CreateAnonymousWithFirstLevelSubObjects(paymentMethod)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of PaymentMethod records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/PaymentMethods/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			string name = null,
			string description = null,
			bool? isElectronic = null,
			int? sequence = null,
			string color = null,
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

			IQueryable<Database.PaymentMethod> query = (from pm in _context.PaymentMethods select pm);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(pm => pm.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(pm => pm.description == description);
			}
			if (isElectronic.HasValue == true)
			{
				query = query.Where(pm => pm.isElectronic == isElectronic.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(pm => pm.sequence == sequence.Value);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(pm => pm.color == color);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pm => pm.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pm => pm.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pm => pm.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pm => pm.deleted == false);
				}
			}
			else
			{
				query = query.Where(pm => pm.active == true);
				query = query.Where(pm => pm.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Payment Method, or on an any of the string fields on its immediate relations
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
			return Ok(await (from queryData in query select Database.PaymentMethod.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/PaymentMethod/CreateAuditEvent")]
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
