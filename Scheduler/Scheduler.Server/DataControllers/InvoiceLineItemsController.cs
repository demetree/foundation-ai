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
    /// This auto generated class provides the basic CRUD operations for the InvoiceLineItem entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the InvoiceLineItem entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class InvoiceLineItemsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private SchedulerContext _context;

		private ILogger<InvoiceLineItemsController> _logger;

		public InvoiceLineItemsController(SchedulerContext context, ILogger<InvoiceLineItemsController> logger) : base("Scheduler", "InvoiceLineItem")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of InvoiceLineItems filtered by the parameters provided.
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
		[Route("api/InvoiceLineItems")]
		public async Task<IActionResult> GetInvoiceLineItems(
			int? invoiceId = null,
			int? eventChargeId = null,
			int? financialCategoryId = null,
			string description = null,
			decimal? quantity = null,
			decimal? unitPrice = null,
			decimal? amount = null,
			decimal? taxAmount = null,
			decimal? totalAmount = null,
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

			IQueryable<Database.InvoiceLineItem> query = (from ili in _context.InvoiceLineItems select ili);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (invoiceId.HasValue == true)
			{
				query = query.Where(ili => ili.invoiceId == invoiceId.Value);
			}
			if (eventChargeId.HasValue == true)
			{
				query = query.Where(ili => ili.eventChargeId == eventChargeId.Value);
			}
			if (financialCategoryId.HasValue == true)
			{
				query = query.Where(ili => ili.financialCategoryId == financialCategoryId.Value);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ili => ili.description == description);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(ili => ili.quantity == quantity.Value);
			}
			if (unitPrice.HasValue == true)
			{
				query = query.Where(ili => ili.unitPrice == unitPrice.Value);
			}
			if (amount.HasValue == true)
			{
				query = query.Where(ili => ili.amount == amount.Value);
			}
			if (taxAmount.HasValue == true)
			{
				query = query.Where(ili => ili.taxAmount == taxAmount.Value);
			}
			if (totalAmount.HasValue == true)
			{
				query = query.Where(ili => ili.totalAmount == totalAmount.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ili => ili.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ili => ili.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ili => ili.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ili => ili.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ili => ili.deleted == false);
				}
			}
			else
			{
				query = query.Where(ili => ili.active == true);
				query = query.Where(ili => ili.deleted == false);
			}

			query = query.OrderBy(ili => ili.sequence).ThenBy(ili => ili.description);


			//
			// Add the any string contains parameter to span all the string fields on the Invoice Line Item, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.description.Contains(anyStringContains)
			       || (includeRelations == true && x.eventCharge.description.Contains(anyStringContains))
			       || (includeRelations == true && x.eventCharge.notes.Contains(anyStringContains))
			       || (includeRelations == true && x.eventCharge.externalId.Contains(anyStringContains))
			       || (includeRelations == true && x.financialCategory.name.Contains(anyStringContains))
			       || (includeRelations == true && x.financialCategory.description.Contains(anyStringContains))
			       || (includeRelations == true && x.financialCategory.code.Contains(anyStringContains))
			       || (includeRelations == true && x.financialCategory.externalAccountId.Contains(anyStringContains))
			       || (includeRelations == true && x.financialCategory.color.Contains(anyStringContains))
			       || (includeRelations == true && x.invoice.invoiceNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.invoice.notes.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.eventCharge);
				query = query.Include(x => x.financialCategory);
				query = query.Include(x => x.invoice);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.InvoiceLineItem> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.InvoiceLineItem invoiceLineItem in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(invoiceLineItem, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.InvoiceLineItem Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.InvoiceLineItem Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of InvoiceLineItems filtered by the parameters provided.  Its query is similar to the GetInvoiceLineItems method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/InvoiceLineItems/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? invoiceId = null,
			int? eventChargeId = null,
			int? financialCategoryId = null,
			string description = null,
			decimal? quantity = null,
			decimal? unitPrice = null,
			decimal? amount = null,
			decimal? taxAmount = null,
			decimal? totalAmount = null,
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


			IQueryable<Database.InvoiceLineItem> query = (from ili in _context.InvoiceLineItems select ili);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (invoiceId.HasValue == true)
			{
				query = query.Where(ili => ili.invoiceId == invoiceId.Value);
			}
			if (eventChargeId.HasValue == true)
			{
				query = query.Where(ili => ili.eventChargeId == eventChargeId.Value);
			}
			if (financialCategoryId.HasValue == true)
			{
				query = query.Where(ili => ili.financialCategoryId == financialCategoryId.Value);
			}
			if (description != null)
			{
				query = query.Where(ili => ili.description == description);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(ili => ili.quantity == quantity.Value);
			}
			if (unitPrice.HasValue == true)
			{
				query = query.Where(ili => ili.unitPrice == unitPrice.Value);
			}
			if (amount.HasValue == true)
			{
				query = query.Where(ili => ili.amount == amount.Value);
			}
			if (taxAmount.HasValue == true)
			{
				query = query.Where(ili => ili.taxAmount == taxAmount.Value);
			}
			if (totalAmount.HasValue == true)
			{
				query = query.Where(ili => ili.totalAmount == totalAmount.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ili => ili.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ili => ili.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ili => ili.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ili => ili.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ili => ili.deleted == false);
				}
			}
			else
			{
				query = query.Where(ili => ili.active == true);
				query = query.Where(ili => ili.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Invoice Line Item, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.description.Contains(anyStringContains)
			       || x.eventCharge.description.Contains(anyStringContains)
			       || x.eventCharge.notes.Contains(anyStringContains)
			       || x.eventCharge.externalId.Contains(anyStringContains)
			       || x.financialCategory.name.Contains(anyStringContains)
			       || x.financialCategory.description.Contains(anyStringContains)
			       || x.financialCategory.code.Contains(anyStringContains)
			       || x.financialCategory.externalAccountId.Contains(anyStringContains)
			       || x.financialCategory.color.Contains(anyStringContains)
			       || x.invoice.invoiceNumber.Contains(anyStringContains)
			       || x.invoice.notes.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single InvoiceLineItem by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/InvoiceLineItem/{id}")]
		public async Task<IActionResult> GetInvoiceLineItem(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.InvoiceLineItem> query = (from ili in _context.InvoiceLineItems where
							(ili.id == id) &&
							(userIsAdmin == true || ili.deleted == false) &&
							(userIsWriter == true || ili.active == true)
					select ili);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.eventCharge);
					query = query.Include(x => x.financialCategory);
					query = query.Include(x => x.invoice);
					query = query.AsSplitQuery();
				}

				Database.InvoiceLineItem materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.InvoiceLineItem Entity was read with Admin privilege." : "Scheduler.InvoiceLineItem Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "InvoiceLineItem", materialized.id, materialized.description));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.InvoiceLineItem entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.InvoiceLineItem.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.InvoiceLineItem.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing InvoiceLineItem record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/InvoiceLineItem/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutInvoiceLineItem(int id, [FromBody]Database.InvoiceLineItem.InvoiceLineItemDTO invoiceLineItemDTO, CancellationToken cancellationToken = default)
		{
			if (invoiceLineItemDTO == null)
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



			if (id != invoiceLineItemDTO.id)
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


			IQueryable<Database.InvoiceLineItem> query = (from x in _context.InvoiceLineItems
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.InvoiceLineItem existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.InvoiceLineItem PUT", id.ToString(), new Exception("No Scheduler.InvoiceLineItem entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (invoiceLineItemDTO.objectGuid == Guid.Empty)
            {
                invoiceLineItemDTO.objectGuid = existing.objectGuid;
            }
            else if (invoiceLineItemDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a InvoiceLineItem record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.InvoiceLineItem cloneOfExisting = (Database.InvoiceLineItem)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new InvoiceLineItem object using the data from the existing record, updated with what is in the DTO.
			//
			Database.InvoiceLineItem invoiceLineItem = (Database.InvoiceLineItem)_context.Entry(existing).GetDatabaseValues().ToObject();
			invoiceLineItem.ApplyDTO(invoiceLineItemDTO);
			//
			// The tenant guid for any InvoiceLineItem being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the InvoiceLineItem because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				invoiceLineItem.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (invoiceLineItem.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted Scheduler.InvoiceLineItem record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (invoiceLineItem.description != null && invoiceLineItem.description.Length > 500)
			{
				invoiceLineItem.description = invoiceLineItem.description.Substring(0, 500);
			}

			EntityEntry<Database.InvoiceLineItem> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(invoiceLineItem);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.InvoiceLineItem entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.InvoiceLineItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.InvoiceLineItem.CreateAnonymousWithFirstLevelSubObjects(invoiceLineItem)),
					null);


				return Ok(Database.InvoiceLineItem.CreateAnonymous(invoiceLineItem));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"Scheduler.InvoiceLineItem entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.InvoiceLineItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.InvoiceLineItem.CreateAnonymousWithFirstLevelSubObjects(invoiceLineItem)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new InvoiceLineItem record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/InvoiceLineItem", Name = "InvoiceLineItem")]
		public async Task<IActionResult> PostInvoiceLineItem([FromBody]Database.InvoiceLineItem.InvoiceLineItemDTO invoiceLineItemDTO, CancellationToken cancellationToken = default)
		{
			if (invoiceLineItemDTO == null)
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
			// Create a new InvoiceLineItem object using the data from the DTO
			//
			Database.InvoiceLineItem invoiceLineItem = Database.InvoiceLineItem.FromDTO(invoiceLineItemDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				invoiceLineItem.tenantGuid = userTenantGuid;

				if (invoiceLineItem.description != null && invoiceLineItem.description.Length > 500)
				{
					invoiceLineItem.description = invoiceLineItem.description.Substring(0, 500);
				}

				invoiceLineItem.objectGuid = Guid.NewGuid();
				_context.InvoiceLineItems.Add(invoiceLineItem);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"Scheduler.InvoiceLineItem entity successfully created.",
					true,
					invoiceLineItem.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.InvoiceLineItem.CreateAnonymousWithFirstLevelSubObjects(invoiceLineItem)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "Scheduler.InvoiceLineItem entity creation failed.", false, invoiceLineItem.id.ToString(), "", JsonSerializer.Serialize(invoiceLineItem), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "InvoiceLineItem", invoiceLineItem.id, invoiceLineItem.description));

			return CreatedAtRoute("InvoiceLineItem", new { id = invoiceLineItem.id }, Database.InvoiceLineItem.CreateAnonymousWithFirstLevelSubObjects(invoiceLineItem));
		}



        /// <summary>
        /// 
        /// This deletes a InvoiceLineItem record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/InvoiceLineItem/{id}")]
		[Route("api/InvoiceLineItem")]
		public async Task<IActionResult> DeleteInvoiceLineItem(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.InvoiceLineItem> query = (from x in _context.InvoiceLineItems
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.InvoiceLineItem invoiceLineItem = await query.FirstOrDefaultAsync(cancellationToken);

			if (invoiceLineItem == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for Scheduler.InvoiceLineItem DELETE", id.ToString(), new Exception("No Scheduler.InvoiceLineItem entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.InvoiceLineItem cloneOfExisting = (Database.InvoiceLineItem)_context.Entry(invoiceLineItem).GetDatabaseValues().ToObject();


			try
			{
				invoiceLineItem.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.InvoiceLineItem entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.InvoiceLineItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.InvoiceLineItem.CreateAnonymousWithFirstLevelSubObjects(invoiceLineItem)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"Scheduler.InvoiceLineItem entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.InvoiceLineItem.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.InvoiceLineItem.CreateAnonymousWithFirstLevelSubObjects(invoiceLineItem)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of InvoiceLineItem records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/InvoiceLineItems/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? invoiceId = null,
			int? eventChargeId = null,
			int? financialCategoryId = null,
			string description = null,
			decimal? quantity = null,
			decimal? unitPrice = null,
			decimal? amount = null,
			decimal? taxAmount = null,
			decimal? totalAmount = null,
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

			IQueryable<Database.InvoiceLineItem> query = (from ili in _context.InvoiceLineItems select ili);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (invoiceId.HasValue == true)
			{
				query = query.Where(ili => ili.invoiceId == invoiceId.Value);
			}
			if (eventChargeId.HasValue == true)
			{
				query = query.Where(ili => ili.eventChargeId == eventChargeId.Value);
			}
			if (financialCategoryId.HasValue == true)
			{
				query = query.Where(ili => ili.financialCategoryId == financialCategoryId.Value);
			}
			if (string.IsNullOrEmpty(description) == false)
			{
				query = query.Where(ili => ili.description == description);
			}
			if (quantity.HasValue == true)
			{
				query = query.Where(ili => ili.quantity == quantity.Value);
			}
			if (unitPrice.HasValue == true)
			{
				query = query.Where(ili => ili.unitPrice == unitPrice.Value);
			}
			if (amount.HasValue == true)
			{
				query = query.Where(ili => ili.amount == amount.Value);
			}
			if (taxAmount.HasValue == true)
			{
				query = query.Where(ili => ili.taxAmount == taxAmount.Value);
			}
			if (totalAmount.HasValue == true)
			{
				query = query.Where(ili => ili.totalAmount == totalAmount.Value);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(ili => ili.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(ili => ili.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(ili => ili.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(ili => ili.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(ili => ili.deleted == false);
				}
			}
			else
			{
				query = query.Where(ili => ili.active == true);
				query = query.Where(ili => ili.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Invoice Line Item, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.description.Contains(anyStringContains)
			       || x.eventCharge.description.Contains(anyStringContains)
			       || x.eventCharge.notes.Contains(anyStringContains)
			       || x.eventCharge.externalId.Contains(anyStringContains)
			       || x.financialCategory.name.Contains(anyStringContains)
			       || x.financialCategory.description.Contains(anyStringContains)
			       || x.financialCategory.code.Contains(anyStringContains)
			       || x.financialCategory.externalAccountId.Contains(anyStringContains)
			       || x.financialCategory.color.Contains(anyStringContains)
			       || x.invoice.invoiceNumber.Contains(anyStringContains)
			       || x.invoice.notes.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.description);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.InvoiceLineItem.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/InvoiceLineItem/CreateAuditEvent")]
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
