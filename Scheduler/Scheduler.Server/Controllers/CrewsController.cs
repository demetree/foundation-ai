using DocumentFormat.OpenXml.Office2010.Excel;
using Foundation.Auditor;
using Foundation.Controllers;
using Foundation.Scheduler.Database;
using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Foundation.Auditor.AuditEngine;
using static Foundation.Scheduler.Database.Crew;
using static Foundation.Scheduler.Database.CrewMember;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// 
    /// This extends the Crew Data Controller with some new custom methods to help provide more targetted data for clients to avoid needing to make repeated web service hits.
	/// 
    /// </summary>
	public partial class CrewsController : SecureWebAPIController
	{

		//
		// Adds crew memmbers to the crew dto
		//
        public class CrewWithMembersDTO : CrewDTO
        {
			public List<CrewMemberDTO> crewMembers { get; set; }
        }




        /// <summary>
        /// 
        /// This gets a list of crews, with a sub list for the crews members.  
		/// 
		/// This is basically the regular get crews method that has been extended to return the members with the crews
        /// 
        /// </summary>
        [HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CrewsWithMembers")]
		public async Task<IActionResult> GetCrewsWithMembers(string name = null,
                                                             string description = null,
                                                             int? versionNumber = null,
                                                             Guid? objectGuid = null,
                                                             bool? active = null,
                                                             bool? deleted = null,
                                                             int? pageSize = null,
                                                             int? pageNumber = null,
                                                             string anyStringContains = null,
                                                             CancellationToken cancellationToken = default)
		{
			try
			{
				StartAuditEventClock();



				if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
				{
					return Forbid();
				}


				SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

				bool userIsWriter = await UserCanWriteAsync(securityUser, 2, cancellationToken);
				bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);

				bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);

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

				IQueryable<Crew> query = (from c in _context.Crews select c);

				query = query.Where(x => x.tenantGuid == userTenantGuid);

				if (string.IsNullOrEmpty(name) == false)
				{
					query = query.Where(c => c.name == name);
				}
				if (string.IsNullOrEmpty(description) == false)
				{
					query = query.Where(c => c.description == description);
				}
				if (versionNumber.HasValue == true)
				{
					query = query.Where(c => c.versionNumber == versionNumber.Value);
				}
				if (objectGuid.HasValue == true)
				{
					query = query.Where(c => c.objectGuid == objectGuid);
				}
				if (userIsWriter == true)
				{
					if (active.HasValue == true)
					{
						query = query.Where(c => c.active == active.Value);
					}

					if (userIsAdmin == true)
					{
						if (deleted.HasValue == true)
						{
							query = query.Where(c => c.deleted == deleted.Value);
						}
					}
					else
					{
						query = query.Where(c => c.deleted == false);
					}
				}
				else
				{
					query = query.Where(c => c.active == true);
					query = query.Where(c => c.deleted == false);
				}

				query = query.OrderBy(c => c.name);

				if (pageNumber.HasValue == true &&
					pageSize.HasValue == true)
				{
					query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
				}


				//
				// Add in the crew members
				//
				query = query.Include(x => x.CrewMembers);


				query = query.AsSplitQuery();


				//
				// Add the any string contains parameter to span all the string fields on the Crew, or on an any of the string fields on its immediate relations
				//
				// Note that this will be a time intensive parameter to apply, so use it with that understanding.
				//
				if (!string.IsNullOrEmpty(anyStringContains))
				{
					query = query.Where(x =>
						x.name.Contains(anyStringContains)
						|| x.description.Contains(anyStringContains)
					);
				}

				query = query.AsNoTracking();

				List<Database.Crew> materialized = await query.ToListAsync(cancellationToken);

				// Convert all the date properties to be of kind UTC.
				bool databaseStoresDateWithTimeZone = _context.DoesDatabaseStoreDateWithTimeZone();
				foreach (Database.Crew crew in materialized)
				{
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(crew, databaseStoresDateWithTimeZone);
				}


				await CreateAuditEventAsync(AuditEngine.AuditType.ReadList, userIsAdmin == true ? "Scheduler.Crew Entity list was read with Admin privilege.  Returning " + materialized.Count + " rows of data." : "Scheduler.Crew Entity list was read.  Returning " + materialized.Count + " rows of data.");


				//
				// Return a list of the crews DTO customization with members on it
				//
				return Ok((from c in materialized
						   select new CrewWithMembersDTO()
                        	{
                        		id = c.id,
                        		name = c.name,
                        		description = c.description,
                        		versionNumber = c.versionNumber,
                        		active = c.active,
                        		deleted = c.deleted,
                        		crewMembers = c.CrewMembers.Select(x => x.ToDTO()).ToList()
                        	}
                        	).ToList());
                        
			}
			catch (Exception ex)
			{
                await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Exception caught during entity read of Scheduler.Crew with members.", null, ex);

                return Problem($"Could not load Crews With Members.  {ex.Message}");
			}
		}
		
		


        /// <summary>
        /// 
        /// This gets a single Crew by primary key.
        ///
        /// The rate limit is 2 per second per user.
        /// 
        /// </summary>
		[HttpGet]
		[RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
		[Route("api/CrewWithMembers/{id}")]
		public async Task<IActionResult> GetCrewWithMembers(int id, CancellationToken cancellationToken = default)
		{
			StartAuditEventClock();

			if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
			{
			   return Forbid();
			}


			SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

			bool userIsWriter = await UserCanWriteAsync(securityUser, 2, cancellationToken);
			bool userIsAdmin = await UserCanAdministerAsync(securityUser, cancellationToken);
			bool userIsSecurityAdmin = await UserCanAdministerSecurityModuleAsync(securityUser, cancellationToken);
			
			
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
				IQueryable<Crew> query = (from c in _context.Crews where
							(c.id == id) &&
							(userIsAdmin == true || c.deleted == false) &&
							(userIsWriter == true || c.active == true)
					select c);


				query = query.Where(x => x.tenantGuid == userTenantGuid);

				//
				// Include the crew members
				//
				query = query.Include(x => x.CrewMembers);

				query = query.AsSplitQuery();

				Database.Crew materialized = await query.FirstOrDefaultAsync(cancellationToken);

				if (materialized != null)
				{
					
					// Convert all the date properties to be of kind UTC.
					Foundation.DateTimeUtility.ConvertAllDateTimePropertiesToUTC(materialized, _context.DoesDatabaseStoreDateWithTimeZone());

					await CreateAuditEventAsync(AuditEngine.AuditType.ReadEntity, userIsAdmin == true ? "Scheduler.Crew Entity was read with Admin privilege." : "Scheduler.Crew Entity was read.");

					BackgroundJob.Enqueue(() => SecurityLogic.AddToUserMostRecents(securityUser.id, "Crew", materialized.id, materialized.name));


					//
					// Returnthe crews DTO customization with members on it
					//
					return Ok(new CrewWithMembersDTO()
					{
						id = materialized.id,
						name = materialized.name,
						description = materialized.description,
						versionNumber = materialized.versionNumber,
						active = materialized.active,
						deleted = materialized.deleted,
						crewMembers = materialized.CrewMembers.Select(x => x.ToDTO()).ToList()
					});
                }
                else
				{
					await CreateAuditEventAsync(AuditEngine.AuditType.Error, "Attempt to read a Scheduler.Crew entity that does not exist.", id.ToString());
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				await CreateAuditEventAsync(AuditEngine.AuditType.Error, userIsAdmin == true ? "Exception caught during entity read of Scheduler.Crew.   Entity was read with Admin privilege." : "Exception caught during entity read of Scheduler.Crew.", id.ToString(), ex);
				return Problem(ex.ToString());
			}
		}

	}
}
