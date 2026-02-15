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
    /// This auto generated class provides the basic CRUD operations for the PublishedMocImage entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the PublishedMocImage entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class PublishedMocImagesController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 1;

		private BMCContext _context;

		private ILogger<PublishedMocImagesController> _logger;

		public PublishedMocImagesController(BMCContext context, ILogger<PublishedMocImagesController> logger) : base("BMC", "PublishedMocImage")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(60);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of PublishedMocImages filtered by the parameters provided.
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
		[Route("api/PublishedMocImages")]
		public async Task<IActionResult> GetPublishedMocImages(
			int? publishedMocId = null,
			string imagePath = null,
			string caption = null,
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
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.PublishedMocImage> query = (from pmi in _context.PublishedMocImages select pmi);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (publishedMocId.HasValue == true)
			{
				query = query.Where(pmi => pmi.publishedMocId == publishedMocId.Value);
			}
			if (string.IsNullOrEmpty(imagePath) == false)
			{
				query = query.Where(pmi => pmi.imagePath == imagePath);
			}
			if (string.IsNullOrEmpty(caption) == false)
			{
				query = query.Where(pmi => pmi.caption == caption);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(pmi => pmi.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pmi => pmi.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pmi => pmi.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pmi => pmi.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pmi => pmi.deleted == false);
				}
			}
			else
			{
				query = query.Where(pmi => pmi.active == true);
				query = query.Where(pmi => pmi.deleted == false);
			}

			query = query.OrderBy(pmi => pmi.sequence).ThenBy(pmi => pmi.imagePath).ThenBy(pmi => pmi.caption);

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			if (includeRelations == true)
			{
				query = query.Include(x => x.publishedMoc);
				query = query.AsSplitQuery();
			}


			//
			// Add the any string contains parameter to span all the string fields on the Published Moc Image, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.imagePath.Contains(anyStringContains)
			       || x.caption.Contains(anyStringContains)
			       || (includeRelations == true && x.publishedMoc.name.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.description.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.thumbnailImagePath.Contains(anyStringContains))
			       || (includeRelations == true && x.publishedMoc.tags.Contains(anyStringContains))
			   );
			}

			query = query.AsNoTracking();
			
			List<Database.PublishedMocImage> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.PublishedMocImage publishedMocImage in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(publishedMocImage, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.PublishedMocImage Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.PublishedMocImage Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of PublishedMocImages filtered by the parameters provided.  Its query is similar to the GetPublishedMocImages method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PublishedMocImages/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? publishedMocId = null,
			string imagePath = null,
			string caption = null,
			int? sequence = null,
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


			IQueryable<Database.PublishedMocImage> query = (from pmi in _context.PublishedMocImages select pmi);
			query = query.Where(x => x.tenantGuid == userTenantGuid);
			if (publishedMocId.HasValue == true)
			{
				query = query.Where(pmi => pmi.publishedMocId == publishedMocId.Value);
			}
			if (imagePath != null)
			{
				query = query.Where(pmi => pmi.imagePath == imagePath);
			}
			if (caption != null)
			{
				query = query.Where(pmi => pmi.caption == caption);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(pmi => pmi.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pmi => pmi.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pmi => pmi.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pmi => pmi.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pmi => pmi.deleted == false);
				}
			}
			else
			{
				query = query.Where(pmi => pmi.active == true);
				query = query.Where(pmi => pmi.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Published Moc Image, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.imagePath.Contains(anyStringContains)
			       || x.caption.Contains(anyStringContains)
			       || x.publishedMoc.name.Contains(anyStringContains)
			       || x.publishedMoc.description.Contains(anyStringContains)
			       || x.publishedMoc.thumbnailImagePath.Contains(anyStringContains)
			       || x.publishedMoc.tags.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single PublishedMocImage by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PublishedMocImage/{id}")]
		public async Task<IActionResult> GetPublishedMocImage(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.PublishedMocImage> query = (from pmi in _context.PublishedMocImages where
							(pmi.id == id) &&
							(userIsAdmin == true || pmi.deleted == false) &&
							(userIsWriter == true || pmi.active == true)
					select pmi);


				query = query.Where(x => x.tenantGuid == userTenantGuid);
				if (includeRelations == true)
				{
					query = query.Include(x => x.publishedMoc);
					query = query.AsSplitQuery();
				}

				Database.PublishedMocImage materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.PublishedMocImage Entity was read with Admin privilege." : "BMC.PublishedMocImage Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PublishedMocImage", materialized.id, materialized.imagePath));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.PublishedMocImage entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.PublishedMocImage.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.PublishedMocImage.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


		/// <summary>
		/// 
		/// This updates an existing PublishedMocImage record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/PublishedMocImage/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutPublishedMocImage(int id, [FromBody]Database.PublishedMocImage.PublishedMocImageDTO publishedMocImageDTO, CancellationToken cancellationToken = default)
		{
			if (publishedMocImageDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}



			if (id != publishedMocImageDTO.id)
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


			IQueryable<Database.PublishedMocImage> query = (from x in _context.PublishedMocImages
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.PublishedMocImage existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.PublishedMocImage PUT", id.ToString(), new Exception("No BMC.PublishedMocImage entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (publishedMocImageDTO.objectGuid == Guid.Empty)
            {
                publishedMocImageDTO.objectGuid = existing.objectGuid;
            }
            else if (publishedMocImageDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a PublishedMocImage record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.PublishedMocImage cloneOfExisting = (Database.PublishedMocImage)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new PublishedMocImage object using the data from the existing record, updated with what is in the DTO.
			//
			Database.PublishedMocImage publishedMocImage = (Database.PublishedMocImage)_context.Entry(existing).GetDatabaseValues().ToObject();
			publishedMocImage.ApplyDTO(publishedMocImageDTO);
			//
			// The tenant guid for any PublishedMocImage being saved must match the tenant guid of the user.  
			//
			if (existing.tenantGuid != userTenantGuid)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt was made to save a record with a tenant guid that is not the user's tenant guid.", false);
				return Problem("Data integrity violation detected while attempting to save.");
			}
			else
			{
				// Assign the tenantGuid to the PublishedMocImage because it shouldn't be on the input object, and we want to ensure that it always is what the correct value in case it is.
				publishedMocImage.tenantGuid = existing.tenantGuid;
			}


			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (publishedMocImage.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.PublishedMocImage record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (publishedMocImage.imagePath != null && publishedMocImage.imagePath.Length > 250)
			{
				publishedMocImage.imagePath = publishedMocImage.imagePath.Substring(0, 250);
			}

			if (publishedMocImage.caption != null && publishedMocImage.caption.Length > 250)
			{
				publishedMocImage.caption = publishedMocImage.caption.Substring(0, 250);
			}

			EntityEntry<Database.PublishedMocImage> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(publishedMocImage);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.PublishedMocImage entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PublishedMocImage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PublishedMocImage.CreateAnonymousWithFirstLevelSubObjects(publishedMocImage)),
					null);


				return Ok(Database.PublishedMocImage.CreateAnonymous(publishedMocImage));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.PublishedMocImage entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PublishedMocImage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PublishedMocImage.CreateAnonymousWithFirstLevelSubObjects(publishedMocImage)),
					ex);

				return Problem(ex.Message);
			}

		}

        /// <summary>
        /// 
        /// This creates a new PublishedMocImage record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PublishedMocImage", Name = "PublishedMocImage")]
		public async Task<IActionResult> PostPublishedMocImage([FromBody]Database.PublishedMocImage.PublishedMocImageDTO publishedMocImageDTO, CancellationToken cancellationToken = default)
		{
			if (publishedMocImageDTO == null)
			{
			   return BadRequest();
			}

			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
			// Create a new PublishedMocImage object using the data from the DTO
			//
			Database.PublishedMocImage publishedMocImage = Database.PublishedMocImage.FromDTO(publishedMocImageDTO);

			try
			{
				//
				// Ensure that the tenant data is correct.
				//
				publishedMocImage.tenantGuid = userTenantGuid;

				if (publishedMocImage.imagePath != null && publishedMocImage.imagePath.Length > 250)
				{
					publishedMocImage.imagePath = publishedMocImage.imagePath.Substring(0, 250);
				}

				if (publishedMocImage.caption != null && publishedMocImage.caption.Length > 250)
				{
					publishedMocImage.caption = publishedMocImage.caption.Substring(0, 250);
				}

				publishedMocImage.objectGuid = Guid.NewGuid();
				_context.PublishedMocImages.Add(publishedMocImage);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.PublishedMocImage entity successfully created.",
					true,
					publishedMocImage.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.PublishedMocImage.CreateAnonymousWithFirstLevelSubObjects(publishedMocImage)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.PublishedMocImage entity creation failed.", false, publishedMocImage.id.ToString(), "", JsonSerializer.Serialize(publishedMocImage), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "PublishedMocImage", publishedMocImage.id, publishedMocImage.imagePath));

			return CreatedAtRoute("PublishedMocImage", new { id = publishedMocImage.id }, Database.PublishedMocImage.CreateAnonymousWithFirstLevelSubObjects(publishedMocImage));
		}



        /// <summary>
        /// 
        /// This deletes a PublishedMocImage record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/PublishedMocImage/{id}")]
		[Route("api/PublishedMocImage")]
		public async Task<IActionResult> DeletePublishedMocImage(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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

			IQueryable<Database.PublishedMocImage> query = (from x in _context.PublishedMocImages
				where
				(x.id == id)
				select x);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			Database.PublishedMocImage publishedMocImage = await query.FirstOrDefaultAsync(cancellationToken);

			if (publishedMocImage == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.PublishedMocImage DELETE", id.ToString(), new Exception("No BMC.PublishedMocImage entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.PublishedMocImage cloneOfExisting = (Database.PublishedMocImage)_context.Entry(publishedMocImage).GetDatabaseValues().ToObject();


			try
			{
				publishedMocImage.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.PublishedMocImage entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.PublishedMocImage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PublishedMocImage.CreateAnonymousWithFirstLevelSubObjects(publishedMocImage)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.PublishedMocImage entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.PublishedMocImage.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.PublishedMocImage.CreateAnonymousWithFirstLevelSubObjects(publishedMocImage)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


        /// <summary>
        /// 
        /// This gets a list of PublishedMocImage records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/PublishedMocImages/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? publishedMocId = null,
			string imagePath = null,
			string caption = null,
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
			// BMC Reader role or better needed to read from this table, as well as the minimum read permission level.
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

			IQueryable<Database.PublishedMocImage> query = (from pmi in _context.PublishedMocImages select pmi);

			query = query.Where(x => x.tenantGuid == userTenantGuid);

			if (publishedMocId.HasValue == true)
			{
				query = query.Where(pmi => pmi.publishedMocId == publishedMocId.Value);
			}
			if (string.IsNullOrEmpty(imagePath) == false)
			{
				query = query.Where(pmi => pmi.imagePath == imagePath);
			}
			if (string.IsNullOrEmpty(caption) == false)
			{
				query = query.Where(pmi => pmi.caption == caption);
			}
			if (sequence.HasValue == true)
			{
				query = query.Where(pmi => pmi.sequence == sequence.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(pmi => pmi.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(pmi => pmi.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(pmi => pmi.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(pmi => pmi.deleted == false);
				}
			}
			else
			{
				query = query.Where(pmi => pmi.active == true);
				query = query.Where(pmi => pmi.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Published Moc Image, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.imagePath.Contains(anyStringContains)
			       || x.caption.Contains(anyStringContains)
			       || x.publishedMoc.name.Contains(anyStringContains)
			       || x.publishedMoc.description.Contains(anyStringContains)
			       || x.publishedMoc.thumbnailImagePath.Contains(anyStringContains)
			       || x.publishedMoc.tags.Contains(anyStringContains)
			   );
			}


			query = query.Where(x => x.tenantGuid == userTenantGuid);


			query = query.OrderBy(x => x.sequence).ThenBy(x => x.imagePath).ThenBy(x => x.caption);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.PublishedMocImage.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/PublishedMocImage/CreateAuditEvent")]
		public async Task<IActionResult> CreateControllerAuditEvent(AuditEngine.AuditType type, string message, string primaryKey = null, CancellationToken cancellationToken = default)
		{

			//
			// BMC Community Writer role needed to write to this table, or BMC Administrator role.  Note we do not check the user's write permission level here.  Role membership is the key to write access.
			//
			if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
			{
			   return Forbid();
			}


		    await CreateAuditEventAsync(type, message, primaryKey);

		    return Ok();
		}


	}
}
