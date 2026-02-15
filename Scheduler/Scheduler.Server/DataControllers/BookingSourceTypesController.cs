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
    /// This auto generated class provides the basic CRUD operations for the BookingSourceType entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BookingSourceType entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BookingSourceTypesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private SchedulerContext _context;

		private ILogger<BookingSourceTypesController> _logger;

		public BookingSourceTypesController(SchedulerContext context, ILogger<BookingSourceTypesController> logger) : base("Scheduler", "BookingSourceType")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BookingSourceTypes filtered by the parameters provided.
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
		[Route("api/BookingSourceTypes")]
		public async Task<IActionResult> GetBookingSourceTypes(
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

			IQueryable<Database.BookingSourceType> query = (from bst in _context.BookingSourceTypes select bst);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(bst => bst.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(bst => bst.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(bst => bst.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(bst => bst.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bst => bst.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bst => bst.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bst => bst.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bst => bst.deleted == false);
				}
			}
			else
			{
				query = query.Where(bst => bst.active == true);
				query = query.Where(bst => bst.deleted == false);
			}

			query = query.OrderBy(bst => bst.sequence).ThenBy(bst => bst.name).ThenBy(bst => bst.description).ThenBy(bst => bst.color);

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
			// Add the any string contains parameter to span all the string fields on the Booking Source Type, or on an any of the string fields on its immediate relations
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

			query = query.AsNoTracking();
			
			List<Database.BookingSourceType> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BookingSourceType bookingSourceType in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(bookingSourceType, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.BookingSourceType Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.BookingSourceType Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BookingSourceTypes filtered by the parameters provided.  Its query is similar to the GetBookingSourceTypes method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BookingSourceTypes/RowCount")]
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

			IQueryable<Database.BookingSourceType> query = (from bst in _context.BookingSourceTypes select bst);
			if (name != null)
			{
				query = query.Where(bst => bst.name == name);
			}
			if (description != null)
			{
				query = query.Where(bst => bst.description == description);
			}
			if (color != null)
			{
				query = query.Where(bst => bst.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(bst => bst.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bst => bst.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bst => bst.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bst => bst.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bst => bst.deleted == false);
				}
			}
			else
			{
				query = query.Where(bst => bst.active == true);
				query = query.Where(bst => bst.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Booking Source Type, or on an any of the string fields on its immediate relations
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
        /// This gets a single BookingSourceType by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BookingSourceType/{id}")]
		public async Task<IActionResult> GetBookingSourceType(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.BookingSourceType> query = (from bst in _context.BookingSourceTypes where
							(bst.id == id) &&
							(userIsAdmin == true || bst.deleted == false) &&
							(userIsWriter == true || bst.active == true)
					select bst);

				if (includeRelations == true)
				{
					query = query.AsSplitQuery();
				}

				Database.BookingSourceType materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.BookingSourceType Entity was read with Admin privilege." : "Scheduler.BookingSourceType Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BookingSourceType", materialized.id, materialized.name));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.BookingSourceType entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.BookingSourceType.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.BookingSourceType.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing BookingSourceType record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BookingSourceType/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBookingSourceType(int id, [FromBody]Database.BookingSourceType.BookingSourceTypeDTO bookingSourceTypeDTO, CancellationToken cancellationToken = default)
		{
			if (bookingSourceTypeDTO == null)
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



			if (id != bookingSourceTypeDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.BookingSourceType> query = (from x in _context.BookingSourceTypes
				where
				(x.id == id)
				select x);


			Database.BookingSourceType existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.BookingSourceType PUT", id.ToString(), new Exception("No Scheduler.BookingSourceType entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (bookingSourceTypeDTO.objectGuid == Guid.Empty)
            {
                bookingSourceTypeDTO.objectGuid = existing.objectGuid;
            }
            else if (bookingSourceTypeDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BookingSourceType record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BookingSourceType cloneOfExisting = (Database.BookingSourceType)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BookingSourceType object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BookingSourceType bookingSourceType = (Database.BookingSourceType)_context.Entry(existing).GetDatabaseValues().ToObject();
			bookingSourceType.ApplyDTO(bookingSourceTypeDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (bookingSourceType.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.BookingSourceType record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (bookingSourceType.name != null && bookingSourceType.name.Length > 100)
			{
				bookingSourceType.name = bookingSourceType.name.Substring(0, 100);
			}

			if (bookingSourceType.description != null && bookingSourceType.description.Length > 500)
			{
				bookingSourceType.description = bookingSourceType.description.Substring(0, 500);
			}

			if (bookingSourceType.color != null && bookingSourceType.color.Length > 10)
			{
				bookingSourceType.color = bookingSourceType.color.Substring(0, 10);
			}

			EntityEntry<Database.BookingSourceType> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(bookingSourceType);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.BookingSourceType entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BookingSourceType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BookingSourceType.CreateAnonymousWithFirstLevelSubObjects(bookingSourceType)),
					null);


				return Ok(Database.BookingSourceType.CreateAnonymous(bookingSourceType));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.BookingSourceType entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BookingSourceType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BookingSourceType.CreateAnonymousWithFirstLevelSubObjects(bookingSourceType)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new BookingSourceType record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BookingSourceType", Name = "BookingSourceType")]
		public async Task<IActionResult> PostBookingSourceType([FromBody]Database.BookingSourceType.BookingSourceTypeDTO bookingSourceTypeDTO, CancellationToken cancellationToken = default)
		{
			if (bookingSourceTypeDTO == null)
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
			// Create a new BookingSourceType object using the data from the DTO
			//
			Database.BookingSourceType bookingSourceType = Database.BookingSourceType.FromDTO(bookingSourceTypeDTO);

			try
			{
				if (bookingSourceType.name != null && bookingSourceType.name.Length > 100)
				{
					bookingSourceType.name = bookingSourceType.name.Substring(0, 100);
				}

				if (bookingSourceType.description != null && bookingSourceType.description.Length > 500)
				{
					bookingSourceType.description = bookingSourceType.description.Substring(0, 500);
				}

				if (bookingSourceType.color != null && bookingSourceType.color.Length > 10)
				{
					bookingSourceType.color = bookingSourceType.color.Substring(0, 10);
				}

				bookingSourceType.objectGuid = Guid.NewGuid();
				_context.BookingSourceTypes.Add(bookingSourceType);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.BookingSourceType entity successfully created.",
					true,
					bookingSourceType.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BookingSourceType.CreateAnonymousWithFirstLevelSubObjects(bookingSourceType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.BookingSourceType entity creation failed.", false, bookingSourceType.id.ToString(), "", JsonSerializer.Serialize(bookingSourceType), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BookingSourceType", bookingSourceType.id, bookingSourceType.name));

			return CreatedAtRoute("BookingSourceType", new { id = bookingSourceType.id }, Database.BookingSourceType.CreateAnonymousWithFirstLevelSubObjects(bookingSourceType));
		}



        /// <summary>
        /// 
        /// This deletes a BookingSourceType record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BookingSourceType/{id}")]
		[Route("api/BookingSourceType")]
		public async Task<IActionResult> DeleteBookingSourceType(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.BookingSourceType> query = (from x in _context.BookingSourceTypes
				where
				(x.id == id)
				select x);


			Database.BookingSourceType bookingSourceType = await query.FirstOrDefaultAsync(cancellationToken);

			if (bookingSourceType == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.BookingSourceType DELETE", id.ToString(), new Exception("No Scheduler.BookingSourceType entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BookingSourceType cloneOfExisting = (Database.BookingSourceType)_context.Entry(bookingSourceType).GetDatabaseValues().ToObject();


			try
			{
				bookingSourceType.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.BookingSourceType entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BookingSourceType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BookingSourceType.CreateAnonymousWithFirstLevelSubObjects(bookingSourceType)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.BookingSourceType entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BookingSourceType.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BookingSourceType.CreateAnonymousWithFirstLevelSubObjects(bookingSourceType)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of BookingSourceType records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BookingSourceTypes/ListData")]
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

			IQueryable<Database.BookingSourceType> query = (from bst in _context.BookingSourceTypes select bst);
			if (string.IsNullOrEmpty(name) == false)
			{
				query = query.Where(bst => bst.name == name);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(bst => bst.description == description);
			}
			if (string.IsNullOrEmpty(color) == false)
			{
				query = query.Where(bst => bst.color == color);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(bst => bst.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bst => bst.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bst => bst.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bst => bst.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bst => bst.deleted == false);
				}
			}
			else
			{
				query = query.Where(bst => bst.active == true);
				query = query.Where(bst => bst.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Booking Source Type, or on an any of the string fields on its immediate relations
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
			return Ok(await (from queryData in query select Database.BookingSourceType.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BookingSourceType/CreateAuditEvent")]
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
