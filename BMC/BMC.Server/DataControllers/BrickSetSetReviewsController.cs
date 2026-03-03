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
    /// This auto generated class provides the basic CRUD operations for the BrickSetSetReview entity via a Web API.
	/// 
	/// It can be used as-is, or as a starting point for customizations in a new partial class, or a new class entirely.
    ///
    /// It demonstrates the features available for the BrickSetSetReview entity, possibly including: multi tenancy, data visibility, version control with rollback, and favouriting.
	/// 
    /// </summary>
	public partial class BrickSetSetReviewsController : SecureWebAPIController
	{
		public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
		public const int WRITE_PERMISSION_LEVEL_REQUIRED = 255;

		private BMCContext _context;

		private ILogger<BrickSetSetReviewsController> _logger;

		public BrickSetSetReviewsController(BMCContext context, ILogger<BrickSetSetReviewsController> logger) : base("BMC", "BrickSetSetReview")
		{
			this._context = context;
			this._logger = logger;

			this._context.Database.SetCommandTimeout(30);

			return;
		}

		/// <summary>
		/// 
		/// This gets a list of BrickSetSetReviews filtered by the parameters provided.
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
		[Route("api/BrickSetSetReviews")]
		public async Task<IActionResult> GetBrickSetSetReviews(
			int? legoSetId = null,
			string reviewAuthor = null,
			DateTime? reviewDate = null,
			string reviewTitle = null,
			string reviewBody = null,
			int? overallRating = null,
			int? buildingExperienceRating = null,
			int? valueForMoneyRating = null,
			int? partsRating = null,
			int? playabilityRating = null,
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
			if (reviewDate.HasValue == true && reviewDate.Value.Kind != DateTimeKind.Utc)
			{
				reviewDate = reviewDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BrickSetSetReview> query = (from bssr in _context.BrickSetSetReviews select bssr);
			if (legoSetId.HasValue == true)
			{
				query = query.Where(bssr => bssr.legoSetId == legoSetId.Value);
			}
			if (string.IsNullOrEmpty(reviewAuthor) == false)
			{
				query = query.Where(bssr => bssr.reviewAuthor == reviewAuthor);
			}
			if (reviewDate.HasValue == true)
			{
				query = query.Where(bssr => bssr.reviewDate == reviewDate.Value);
			}
			if (string.IsNullOrEmpty(reviewTitle) == false)
			{
				query = query.Where(bssr => bssr.reviewTitle == reviewTitle);
			}
			if (string.IsNullOrEmpty(reviewBody) == false)
			{
				query = query.Where(bssr => bssr.reviewBody == reviewBody);
			}
			if (overallRating.HasValue == true)
			{
				query = query.Where(bssr => bssr.overallRating == overallRating.Value);
			}
			if (buildingExperienceRating.HasValue == true)
			{
				query = query.Where(bssr => bssr.buildingExperienceRating == buildingExperienceRating.Value);
			}
			if (valueForMoneyRating.HasValue == true)
			{
				query = query.Where(bssr => bssr.valueForMoneyRating == valueForMoneyRating.Value);
			}
			if (partsRating.HasValue == true)
			{
				query = query.Where(bssr => bssr.partsRating == partsRating.Value);
			}
			if (playabilityRating.HasValue == true)
			{
				query = query.Where(bssr => bssr.playabilityRating == playabilityRating.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bssr => bssr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bssr => bssr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bssr => bssr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bssr => bssr.deleted == false);
				}
			}
			else
			{
				query = query.Where(bssr => bssr.active == true);
				query = query.Where(bssr => bssr.deleted == false);
			}

			query = query.OrderBy(bssr => bssr.reviewAuthor);


			//
			// Add the any string contains parameter to span all the string fields on the Brick Set Set Review, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reviewAuthor.Contains(anyStringContains)
			       || x.reviewTitle.Contains(anyStringContains)
			       || x.reviewBody.Contains(anyStringContains)
			       || (includeRelations == true && x.legoSet.name.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.setNumber.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.imageUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.brickLinkUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.rebrickableUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.rebrickableSetNum.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.brickSetUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.instructionsUrl.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.subtheme.Contains(anyStringContains))
			       || (includeRelations == true && x.legoSet.availability.Contains(anyStringContains))
			   );
			}

			if (includeRelations == true)
			{
				query = query.Include(x => x.legoSet);
				query = query.AsSplitQuery();
			}

			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			
			query = query.AsNoTracking();
			
			List<Database.BrickSetSetReview> materialized = await query.ToListAsync(cancellationToken);

			// Convert all the date properties to be of kind UTC.
			bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
			foreach (Database.BrickSetSetReview brickSetSetReview in materialized)
			{
			    Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(brickSetSetReview, databaseStoresDateWithTimeZone);
			}


			await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "BMC.BrickSetSetReview Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "BMC.BrickSetSetReview Entity list was read.  Returning " + materialized.Count + " rows of data.");

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
        /// This returns a row count of BrickSetSetReviews filtered by the parameters provided.  Its query is similar to the GetBrickSetSetReviews method, but it only returns the count of rows that would be returned.
        ///
        /// The rate limit is 10 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickSetSetReviews/RowCount")]
		public async Task<IActionResult> GetRowCount(
			int? legoSetId = null,
			string reviewAuthor = null,
			DateTime? reviewDate = null,
			string reviewTitle = null,
			string reviewBody = null,
			int? overallRating = null,
			int? buildingExperienceRating = null,
			int? valueForMoneyRating = null,
			int? partsRating = null,
			int? playabilityRating = null,
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
			if (reviewDate.HasValue == true && reviewDate.Value.Kind != DateTimeKind.Utc)
			{
				reviewDate = reviewDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BrickSetSetReview> query = (from bssr in _context.BrickSetSetReviews select bssr);
			if (legoSetId.HasValue == true)
			{
				query = query.Where(bssr => bssr.legoSetId == legoSetId.Value);
			}
			if (reviewAuthor != null)
			{
				query = query.Where(bssr => bssr.reviewAuthor == reviewAuthor);
			}
			if (reviewDate.HasValue == true)
			{
				query = query.Where(bssr => bssr.reviewDate == reviewDate.Value);
			}
			if (reviewTitle != null)
			{
				query = query.Where(bssr => bssr.reviewTitle == reviewTitle);
			}
			if (reviewBody != null)
			{
				query = query.Where(bssr => bssr.reviewBody == reviewBody);
			}
			if (overallRating.HasValue == true)
			{
				query = query.Where(bssr => bssr.overallRating == overallRating.Value);
			}
			if (buildingExperienceRating.HasValue == true)
			{
				query = query.Where(bssr => bssr.buildingExperienceRating == buildingExperienceRating.Value);
			}
			if (valueForMoneyRating.HasValue == true)
			{
				query = query.Where(bssr => bssr.valueForMoneyRating == valueForMoneyRating.Value);
			}
			if (partsRating.HasValue == true)
			{
				query = query.Where(bssr => bssr.partsRating == partsRating.Value);
			}
			if (playabilityRating.HasValue == true)
			{
				query = query.Where(bssr => bssr.playabilityRating == playabilityRating.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bssr => bssr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bssr => bssr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bssr => bssr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bssr => bssr.deleted == false);
				}
			}
			else
			{
				query = query.Where(bssr => bssr.active == true);
				query = query.Where(bssr => bssr.deleted == false);
			}

			//
			// Add the any string contains parameter to span all the string fields on the Brick Set Set Review, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reviewAuthor.Contains(anyStringContains)
			       || x.reviewTitle.Contains(anyStringContains)
			       || x.reviewBody.Contains(anyStringContains)
			       || x.legoSet.name.Contains(anyStringContains)
			       || x.legoSet.setNumber.Contains(anyStringContains)
			       || x.legoSet.imageUrl.Contains(anyStringContains)
			       || x.legoSet.brickLinkUrl.Contains(anyStringContains)
			       || x.legoSet.rebrickableUrl.Contains(anyStringContains)
			       || x.legoSet.rebrickableSetNum.Contains(anyStringContains)
			       || x.legoSet.brickSetUrl.Contains(anyStringContains)
			       || x.legoSet.instructionsUrl.Contains(anyStringContains)
			       || x.legoSet.subtheme.Contains(anyStringContains)
			       || x.legoSet.availability.Contains(anyStringContains)
			   );
			}


			int output = await query.CountAsync(cancellationToken);

			return Ok(output);
		}


        /// <summary>
        /// 
        /// This gets a single BrickSetSetReview by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickSetSetReview/{id}")]
		public async Task<IActionResult> GetBrickSetSetReview(int id, bool includeRelations = true, CancellationToken cancellationToken = default)
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
				IQueryable<Database.BrickSetSetReview> query = (from bssr in _context.BrickSetSetReviews where
							(bssr.id == id) &&
							(userIsAdmin == true || bssr.deleted == false) &&
							(userIsWriter == true || bssr.active == true)
					select bssr);

				if (includeRelations == true)
				{
					query = query.Include(x => x.legoSet);
					query = query.AsSplitQuery();
				}

				Database.BrickSetSetReview materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "BMC.BrickSetSetReview Entity was read with Admin privilege." : "BMC.BrickSetSetReview Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickSetSetReview", materialized.id, materialized.reviewAuthor));


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
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a BMC.BrickSetSetReview entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of BMC.BrickSetSetReview.   Entity was read with Admin privilege." : "Exception caught during entity read of BMC.BrickSetSetReview.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}


/* This function is expected to be overridden in a custom file
		/// <summary>
		/// 
		/// This updates an existing BrickSetSetReview record
        ///
        /// The rate limit is 2 per second per user.
		/// 
		/// </summary>
		[Route("api/BrickSetSetReview/{id}")]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[HttpPost]
		[HttpPut]
		public async Task<IActionResult> PutBrickSetSetReview(int id, [FromBody]Database.BrickSetSetReview.BrickSetSetReviewDTO brickSetSetReviewDTO, CancellationToken cancellationToken = default)
		{
			if (brickSetSetReviewDTO == null)
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



			if (id != brickSetSetReviewDTO.id)
			{
				return BadRequest();
			}

			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 255, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			IQueryable<Database.BrickSetSetReview> query = (from x in _context.BrickSetSetReviews
				where
				(x.id == id)
				select x);


			Database.BrickSetSetReview existing = await query.FirstOrDefaultAsync(cancellationToken);

			if (existing == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickSetSetReview PUT", id.ToString(), new Exception("No BMC.BrickSetSetReview entity could be found with the primary key provided."));
				return NotFound();
			}


            //
            // Validate the object guid.  If it comes in as empty Guid in the DTO, then set it to the actual value from the existing record.  If the DTO has a value then it must match the existing value.
            // 
            if (brickSetSetReviewDTO.objectGuid == Guid.Empty)
            {
                brickSetSetReviewDTO.objectGuid = existing.objectGuid;
            }
            else if (brickSetSetReviewDTO.objectGuid != existing.objectGuid)
            {
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, $"Attempt was made to change object guid on a BrickSetSetReview record.  This is not allowed.  The User is " + securityUser.accountName, existing.id.ToString());
                return Problem("Invalid Operation.");
            }


			// Copy the existing object so it can be serialized as-is in the audit and history logs.
			Database.BrickSetSetReview cloneOfExisting = (Database.BrickSetSetReview)_context.Entry(existing).GetDatabaseValues().ToObject();

			//
			// Create a new BrickSetSetReview object using the data from the existing record, updated with what is in the DTO.
			//
			Database.BrickSetSetReview brickSetSetReview = (Database.BrickSetSetReview)_context.Entry(existing).GetDatabaseValues().ToObject();
			brickSetSetReview.ApplyDTO(brickSetSetReviewDTO);

			// Is user who is not an admin trying to delete, or to work on a deleted record, or to delete a record by flipping it's deleted flag to true?
			if (userIsAdmin == false && (brickSetSetReview.deleted == true || existing.deleted == true))
			{
				// we're not recording state here because it is not being changed.
				CreateAuditEvent(AuditEngine.AuditType.UnauthorizedAccessAttempt, "Attempt to delete a record or work on a deleted BMC.BrickSetSetReview record.", id.ToString());
				DestroySessionAndAuthentication();
				return Forbid();
			}

			if (brickSetSetReview.reviewAuthor != null && brickSetSetReview.reviewAuthor.Length > 100)
			{
				brickSetSetReview.reviewAuthor = brickSetSetReview.reviewAuthor.Substring(0, 100);
			}

			if (brickSetSetReview.reviewDate.HasValue == true && brickSetSetReview.reviewDate.Value.Kind != DateTimeKind.Utc)
			{
				brickSetSetReview.reviewDate = brickSetSetReview.reviewDate.Value.ToUniversalTime();
			}

			EntityEntry<Database.BrickSetSetReview> attached = _context.Entry(existing);
			attached.CurrentValues.SetValues(brickSetSetReview);

			try
			{
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickSetSetReview entity successfully updated.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickSetSetReview.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickSetSetReview.CreateAnonymousWithFirstLevelSubObjects(brickSetSetReview)),
					null);


				return Ok(Database.BrickSetSetReview.CreateAnonymous(brickSetSetReview));
			}
			catch (Exception ex)
			{
				CreateAuditEvent(AuditEngine.AuditType.UpdateEntity,
					"BMC.BrickSetSetReview entity update failed",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickSetSetReview.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickSetSetReview.CreateAnonymousWithFirstLevelSubObjects(brickSetSetReview)),
					ex);

				return Problem(ex.Message);
			}

		}
*/

/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This creates a new BrickSetSetReview record
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpPost]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickSetSetReview", Name = "BrickSetSetReview")]
		public async Task<IActionResult> PostBrickSetSetReview([FromBody]Database.BrickSetSetReview.BrickSetSetReviewDTO brickSetSetReviewDTO, CancellationToken cancellationToken = default)
		{
			if (brickSetSetReviewDTO == null)
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
			// Create a new BrickSetSetReview object using the data from the DTO
			//
			Database.BrickSetSetReview brickSetSetReview = Database.BrickSetSetReview.FromDTO(brickSetSetReviewDTO);

			try
			{
				if (brickSetSetReview.reviewAuthor != null && brickSetSetReview.reviewAuthor.Length > 100)
				{
					brickSetSetReview.reviewAuthor = brickSetSetReview.reviewAuthor.Substring(0, 100);
				}

				if (brickSetSetReview.reviewDate.HasValue == true && brickSetSetReview.reviewDate.Value.Kind != DateTimeKind.Utc)
				{
					brickSetSetReview.reviewDate = brickSetSetReview.reviewDate.Value.ToUniversalTime();
				}

				brickSetSetReview.objectGuid = Guid.NewGuid();
				_context.BrickSetSetReviews.Add(brickSetSetReview);
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity,
					"BMC.BrickSetSetReview entity successfully created.",
					true,
					brickSetSetReview.id.ToString(),
					"",
					JsonSerializer.Serialize(Database.BrickSetSetReview.CreateAnonymousWithFirstLevelSubObjects(brickSetSetReview)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.CreateEntity, "BMC.BrickSetSetReview entity creation failed.", false, brickSetSetReview.id.ToString(), "", JsonSerializer.Serialize(brickSetSetReview), ex);

				return Problem(ex.Message);
			}


			BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "BrickSetSetReview", brickSetSetReview.id, brickSetSetReview.reviewAuthor));

			return CreatedAtRoute("BrickSetSetReview", new { id = brickSetSetReview.id }, Database.BrickSetSetReview.CreateAnonymousWithFirstLevelSubObjects(brickSetSetReview));
		}

*/


/* This function is expected to be overridden in a custom file
        /// <summary>
        /// 
        /// This deletes a BrickSetSetReview record
		/// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpDelete]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/BrickSetSetReview/{id}")]
		[Route("api/BrickSetSetReview")]
		public async Task<IActionResult> DeleteBrickSetSetReview(int id, CancellationToken cancellationToken = default)
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

			IQueryable<Database.BrickSetSetReview> query = (from x in _context.BrickSetSetReviews
				where
				(x.id == id)
				select x);


			Database.BrickSetSetReview brickSetSetReview = await query.FirstOrDefaultAsync(cancellationToken);

			if (brickSetSetReview == null)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.UpdateEntity, "Invalid primary key provided for BMC.BrickSetSetReview DELETE", id.ToString(), new Exception("No BMC.BrickSetSetReview entity could be find with the primary key provided."));
				return NotFound();
			}
			Database.BrickSetSetReview cloneOfExisting = (Database.BrickSetSetReview)_context.Entry(brickSetSetReview).GetDatabaseValues().ToObject();


			try
			{
				brickSetSetReview.deleted = true;
				await _context.SaveChangesAsync(cancellationToken);

				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickSetSetReview entity successfully deleted.",
					true,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickSetSetReview.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickSetSetReview.CreateAnonymousWithFirstLevelSubObjects(brickSetSetReview)),
					null);

			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.DeleteEntity,
					"BMC.BrickSetSetReview entity delete failed.",
					false,
					id.ToString(),
					JsonSerializer.Serialize(Database.BrickSetSetReview.CreateAnonymousWithFirstLevelSubObjects(cloneOfExisting)),
					JsonSerializer.Serialize(Database.BrickSetSetReview.CreateAnonymousWithFirstLevelSubObjects(brickSetSetReview)),
					ex);

				return Problem(ex.Message);
			}
			return Ok();
		}


*/
        /// <summary>
        /// 
        /// This gets a list of BrickSetSetReview records, filtered by the parameters provided in a simple minimal format that is useful for drop down boxes and similar.
		/// 
		/// It has the same filtering paramfeters as the full ListData method, but only returns the id and name fields.
        /// 
		/// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[Route("api/BrickSetSetReviews/ListData")]
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		public async Task<IActionResult> GetListData(
			int? legoSetId = null,
			string reviewAuthor = null,
			DateTime? reviewDate = null,
			string reviewTitle = null,
			string reviewBody = null,
			int? overallRating = null,
			int? buildingExperienceRating = null,
			int? valueForMoneyRating = null,
			int? partsRating = null,
			int? playabilityRating = null,
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
			if (reviewDate.HasValue == true && reviewDate.Value.Kind != DateTimeKind.Utc)
			{
				reviewDate = reviewDate.Value.ToUniversalTime();
			}

			IQueryable<Database.BrickSetSetReview> query = (from bssr in _context.BrickSetSetReviews select bssr);
			if (legoSetId.HasValue == true)
			{
				query = query.Where(bssr => bssr.legoSetId == legoSetId.Value);
			}
			if (string.IsNullOrEmpty(reviewAuthor) == false)
			{
				query = query.Where(bssr => bssr.reviewAuthor == reviewAuthor);
			}
			if (reviewDate.HasValue == true)
			{
				query = query.Where(bssr => bssr.reviewDate == reviewDate.Value);
			}
			if (string.IsNullOrEmpty(reviewTitle) == false)
			{
				query = query.Where(bssr => bssr.reviewTitle == reviewTitle);
			}
			if (string.IsNullOrEmpty(reviewBody) == false)
			{
				query = query.Where(bssr => bssr.reviewBody == reviewBody);
			}
			if (overallRating.HasValue == true)
			{
				query = query.Where(bssr => bssr.overallRating == overallRating.Value);
			}
			if (buildingExperienceRating.HasValue == true)
			{
				query = query.Where(bssr => bssr.buildingExperienceRating == buildingExperienceRating.Value);
			}
			if (valueForMoneyRating.HasValue == true)
			{
				query = query.Where(bssr => bssr.valueForMoneyRating == valueForMoneyRating.Value);
			}
			if (partsRating.HasValue == true)
			{
				query = query.Where(bssr => bssr.partsRating == partsRating.Value);
			}
			if (playabilityRating.HasValue == true)
			{
				query = query.Where(bssr => bssr.playabilityRating == playabilityRating.Value);
			}
			if (objectGuid.HasValue == true)
			{
				query = query.Where(bssr => bssr.objectGuid == objectGuid);
			}
			if (userIsWriter == true)
			{
				if (active.HasValue == true)
				{
					query = query.Where(bssr => bssr.active == active.Value);
				}
			
				if (userIsAdmin == true)
				{
					if (deleted.HasValue == true)
					{
						query = query.Where(bssr => bssr.deleted == deleted.Value);
					}
				}
				else
				{
					query = query.Where(bssr => bssr.deleted == false);
				}
			}
			else
			{
				query = query.Where(bssr => bssr.active == true);
				query = query.Where(bssr => bssr.deleted == false);
			}


			//
			// Add the any string contains parameter to span all the string fields on the Brick Set Set Review, or on an any of the string fields on its immediate relations
			//
			// Note that this will be a time intensive parameter to apply, so use it with that understanding.
			//
			if (!string.IsNullOrEmpty(anyStringContains))
			{
			   query = query.Where(x =>
			       x.reviewAuthor.Contains(anyStringContains)
			       || x.reviewTitle.Contains(anyStringContains)
			       || x.reviewBody.Contains(anyStringContains)
			       || x.legoSet.name.Contains(anyStringContains)
			       || x.legoSet.setNumber.Contains(anyStringContains)
			       || x.legoSet.imageUrl.Contains(anyStringContains)
			       || x.legoSet.brickLinkUrl.Contains(anyStringContains)
			       || x.legoSet.rebrickableUrl.Contains(anyStringContains)
			       || x.legoSet.rebrickableSetNum.Contains(anyStringContains)
			       || x.legoSet.brickSetUrl.Contains(anyStringContains)
			       || x.legoSet.instructionsUrl.Contains(anyStringContains)
			       || x.legoSet.subtheme.Contains(anyStringContains)
			       || x.legoSet.availability.Contains(anyStringContains)
			   );
			}


			query = query.OrderBy(x => x.reviewAuthor);
			if (pageNumber.HasValue == true &&
			    pageSize.HasValue == true)
			{
			   query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
			}
			return Ok(await (from queryData in query select Database.BrickSetSetReview.CreateMinimalAnonymous(queryData)).ToListAsync(cancellationToken));
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
		[Route("api/BrickSetSetReview/CreateAuditEvent")]
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
