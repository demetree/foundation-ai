using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class SalesforceTenantLink : IVersionTrackedEntity<SalesforceTenantLink>, IAnonymousConvertible
	{
        /// <summary>
        /// This is for setting the context for change history inquiries.
        /// </summary>
        private SchedulerContext _contextForVersionInquiry = null;
        private Guid _tenantGuidForVersionInquiry = Guid.Empty;



        /// <summary>
        /// 
        /// Gets the a Change History toolset for the user that support write and read operations.
        /// 
        /// </summary>
        /// <param name="context">A context object that contains the entities</param>
        /// <param name="securityUser">The security user that the changes will be made on behalf of.</param>
        /// <param name="insideTransaction">Whether or not there is a transaction in process by the using function</param>
        /// <returns>A change history toolset instance to interact with the change history of the entity</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static ChangeHistoryToolset<SalesforceTenantLink, SalesforceTenantLinkChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (securityUser == null)
            {
                throw new ArgumentNullException(nameof(securityUser));
            }

            //
            // This table does not have data visibility enabled, therefore the user ID is to be taken directly from the security user object.
            // 
            return new ChangeHistoryToolset<SalesforceTenantLink, SalesforceTenantLinkChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
        }


        /// <summary>
        /// 
        /// Gets the a Change History toolset for the user that support write and read operations.  (Async variant)
        /// 
        /// </summary>
        /// <param name="context">A context object that contains the entities</param>
        /// <param name="securityUser">The security user that the changes will be made on behalf of.</param>
        /// <param name="insideTransaction">Whether or not there is a transaction in process by the using function</param>
        /// <returns>A change history toolset instance to interact with the change history of the entity</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static Task<ChangeHistoryToolset<SalesforceTenantLink, SalesforceTenantLinkChangeHistory>> GetChangeHistoryToolsetForWritingAsync(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (securityUser == null)
            {
                throw new ArgumentNullException(nameof(securityUser));
            }

            //
            // This table does not have data visibility enabled, therefore the user ID is to be taken directly from the security user object.
            // No async work is needed — return completed task.
            // 
            return Task.FromResult(new ChangeHistoryToolset<SalesforceTenantLink, SalesforceTenantLinkChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken));
        }

        /// <summary>
        /// 
        /// Gets the a Change History toolset for read only purposes.
        /// 
        /// </summary>
        /// <param name="context">A context object that contains the entities</param>
        /// <returns>A change history toolset instance to interact with the change history of the entity</returns>       
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static ChangeHistoryToolset<SalesforceTenantLink, SalesforceTenantLinkChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<SalesforceTenantLink, SalesforceTenantLinkChangeHistory>(context, cancellationToken);
        }


        /// <summary>
        /// 
        /// This needs to be called before running any version inquiry method from the IVersionTrackedEntity interface.
        ///
        /// It sets up the context and the tenant guid to use.  Provide the context used for the work, and the tenant guid of the user executing the logic.
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tenantGuid"></param>
        public void SetupVersionInquiry(SchedulerContext context, Guid tenantGuid)
        {
            _contextForVersionInquiry = context;
            _tenantGuidForVersionInquiry = tenantGuid;
        }


        /// <summary>
        /// 
        /// Gets meta data and optionally the entity data about the entity's version history using the version of the entity as the basis for the query.
        /// 
        /// Use this to get the update user/time metadata for this version.  IncludingData here is optional and default to false, as it is probably redundant in most cases 
        /// unless the entity you're working with might have unsaved changes.
        /// 
        /// </summary>
        /// <param name="includeData">Whether or not to return the entity data with the results.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<VersionInformation<SalesforceTenantLink>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
        {
            return await GetVersionAsync(this.versionNumber, includeData, cancellationToken).ConfigureAwait(false);
        }


        /// <summary>
        /// 
        /// Gets meta data and optionally the entity data about the first version of the entity.  Equivalent to GetVersionAsync(1, includeData), but name is a bit more concise.
        /// 
        /// </summary>
        /// <param name="includeData">Whether or not to return the entity data with the results.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<VersionInformation<SalesforceTenantLink>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
        {
            return await GetVersionAsync(1, includeData, cancellationToken).ConfigureAwait(false);
        }


        /// <summary>
        /// 
        /// Gets meta data and optionally the entity data about the version of the entity at the provided point in time.
        /// 
        /// </summary>
        /// <param name="includeData">Whether or not to return the entity data with the results.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<VersionInformation<SalesforceTenantLink>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
        {
            if (_contextForVersionInquiry == null || _tenantGuidForVersionInquiry == Guid.Empty)
            {
                throw new Exception("Context for version inquiry is not set.  Please call SetupVersionInquiry() before calling this function.");
            }


            var chts = GetChangeHistoryToolsetForReading(_contextForVersionInquiry, cancellationToken);

            // Get the version for the point in time provided
            AuditEntry versionAudit = await chts.GetAuditForTime(this, pointInTime).ConfigureAwait(false);

            if (versionAudit == null)
            {
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this SalesforceTenantLink entity.");
            }

            VersionInformation<SalesforceTenantLink> version = new VersionInformation<SalesforceTenantLink>();

            version.versionNumber = versionAudit.versionNumber;

            version.timeStamp = versionAudit.timeStamp;

            if (versionAudit.userId.HasValue == true)
            {
                // Note that this system has multi tenancy enabled but not data visibility, so it gets its change history users from the security module by linking to tenant users.
                version.user = await Foundation.Security.ChangeHistoryMultiTenant.GetChangeHistoryUserAsync(versionAudit.userId.Value, _tenantGuidForVersionInquiry, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Continency to return a change history user configured to indicate that we don't know the user.
                version.user = new ChangeHistoryUser() { firstName = "Unknown", id = 0, middleName = null, lastName = "User" };
            }

            if (includeData == true)
            {
                version.data = await chts.GetVersionAsync(this, versionAudit.versionNumber).ConfigureAwait(false);
            }

            return version;
        }


        /// <summary>
        /// 
        /// Gets meta data and optionally the entity data about a specific version of the entity.
        /// 
        /// </summary>
        /// <param name="includeData">Whether or not to return the entity data with the results.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<VersionInformation<SalesforceTenantLink>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
        {
            if (_contextForVersionInquiry == null || _tenantGuidForVersionInquiry == Guid.Empty)
            {
                throw new Exception("Context for version inquiry is not set.  Please call SetupVersionInquiry() before accessing the GetVersion function.");
            }

            var chts = GetChangeHistoryToolsetForReading(_contextForVersionInquiry, cancellationToken);

            // Get the requested version
            AuditEntry versionAudit = await chts.GetAuditForVersion(this, versionNumber).ConfigureAwait(false);

            if (versionAudit == null)
            {
                throw new Exception($"No change history found for version {versionNumber} of this SalesforceTenantLink entity.");
            }

            VersionInformation<SalesforceTenantLink> version = new VersionInformation<SalesforceTenantLink>();

            version.versionNumber = versionAudit.versionNumber;
            version.timeStamp = versionAudit.timeStamp;

            if (versionAudit.userId.HasValue == true)
            {
                // Note that this system has multi tenancy enabled but not data visibility, so it gets its change history users from the security module by linking to tenant users.
                version.user = await Foundation.Security.ChangeHistoryMultiTenant.GetChangeHistoryUserAsync(versionAudit.userId.Value, _tenantGuidForVersionInquiry, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Continency to return a change history user configured to indicate that we don't know the user.
                version.user = new ChangeHistoryUser() { firstName = "Unknown", id = 0, middleName = null, lastName = "User" };
            }

            if (includeData == true)
            {
                version.data = await chts.GetVersionAsync(this, versionNumber).ConfigureAwait(false);
            }

            return version;
        }


        /// <summary>
        /// 
        /// This gets all the available meta data version information for this entity, and optionally the entity states too
        /// 
        /// </summary>
        /// <param name="includeData">Whether or not to return the entity data with the results.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<VersionInformation<SalesforceTenantLink>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
        {
            if (_contextForVersionInquiry == null || _tenantGuidForVersionInquiry == Guid.Empty)
            {
                throw new Exception("Context for version inquiry is not set.Please call SetupVersionInquiry() before accessing the GetAllVersions function.");
            }

            var chts = GetChangeHistoryToolsetForReading(_contextForVersionInquiry, cancellationToken);

            List<AuditEntry> versionAudits = await chts.GetAuditTrailAsync(this).ConfigureAwait(false);

            if (versionAudits == null)
            {
                throw new Exception($"No change history audits found for this entity.");
            }

            List <VersionInformation<SalesforceTenantLink>> versions = new List<VersionInformation<SalesforceTenantLink>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<SalesforceTenantLink> version = new VersionInformation<SalesforceTenantLink>();

                version.versionNumber = versionAudit.versionNumber;
                version.timeStamp = versionAudit.timeStamp;

                if (versionAudit.userId.HasValue == true)
                {
                // Note that this system has multi tenancy enabled but not data visibility, so it gets its change history users from the security module by linking to tenant users.
                version.user = await Foundation.Security.ChangeHistoryMultiTenant.GetChangeHistoryUserAsync(versionAudit.userId.Value, _tenantGuidForVersionInquiry, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    // Continency to return a change history user configured to indicate that we don't know the user.
                    version.user = new ChangeHistoryUser() { firstName = "Unknown", id = 0, middleName = null, lastName = "User" };
                }

                if (includeData == true)
                {
                    version.data = await chts.GetVersionAsync(this, versionAudit.versionNumber).ConfigureAwait(false);
                }

                versions.Add(version);
            }

            return versions;
        }


		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SalesforceTenantLinkDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Boolean syncEnabled { get; set; }
			public String syncDirectionFlags { get; set; }
			public Int32? pullIntervalMinutes { get; set; }
			public DateTime? lastPullDate { get; set; }
			public String loginUrl { get; set; }
			public String sfClientId { get; set; }
			public String sfClientSecret { get; set; }
			public String sfUsername { get; set; }
			public String sfPassword { get; set; }
			public String sfSecurityToken { get; set; }
			public String instanceUrl { get; set; }
			public String apiVersion { get; set; }
			public Int32 versionNumber { get; set; }
			[Required]
			public Guid objectGuid { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class SalesforceTenantLinkOutputDTO : SalesforceTenantLinkDTO
		{
		}


		/// <summary>
		///
		/// Converts a SalesforceTenantLink to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SalesforceTenantLinkDTO ToDTO()
		{
			return new SalesforceTenantLinkDTO
			{
				id = this.id,
				syncEnabled = this.syncEnabled,
				syncDirectionFlags = this.syncDirectionFlags,
				pullIntervalMinutes = this.pullIntervalMinutes,
				lastPullDate = this.lastPullDate,
				loginUrl = this.loginUrl,
				sfClientId = this.sfClientId,
				sfClientSecret = this.sfClientSecret,
				sfUsername = this.sfUsername,
				sfPassword = this.sfPassword,
				sfSecurityToken = this.sfSecurityToken,
				instanceUrl = this.instanceUrl,
				apiVersion = this.apiVersion,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SalesforceTenantLink list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SalesforceTenantLinkDTO> ToDTOList(List<SalesforceTenantLink> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SalesforceTenantLinkDTO> output = new List<SalesforceTenantLinkDTO>();

			output.Capacity = data.Count;

			foreach (SalesforceTenantLink salesforceTenantLink in data)
			{
				output.Add(salesforceTenantLink.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SalesforceTenantLink to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SalesforceTenantLink Entity type directly.
		///
		/// </summary>
		public SalesforceTenantLinkOutputDTO ToOutputDTO()
		{
			return new SalesforceTenantLinkOutputDTO
			{
				id = this.id,
				syncEnabled = this.syncEnabled,
				syncDirectionFlags = this.syncDirectionFlags,
				pullIntervalMinutes = this.pullIntervalMinutes,
				lastPullDate = this.lastPullDate,
				loginUrl = this.loginUrl,
				sfClientId = this.sfClientId,
				sfClientSecret = this.sfClientSecret,
				sfUsername = this.sfUsername,
				sfPassword = this.sfPassword,
				sfSecurityToken = this.sfSecurityToken,
				instanceUrl = this.instanceUrl,
				apiVersion = this.apiVersion,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SalesforceTenantLink list to list of Output Data Transfer Object intended to be used for serializing a list of SalesforceTenantLink objects to avoid using the SalesforceTenantLink entity type directly.
		///
		/// </summary>
		public static List<SalesforceTenantLinkOutputDTO> ToOutputDTOList(List<SalesforceTenantLink> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SalesforceTenantLinkOutputDTO> output = new List<SalesforceTenantLinkOutputDTO>();

			output.Capacity = data.Count;

			foreach (SalesforceTenantLink salesforceTenantLink in data)
			{
				output.Add(salesforceTenantLink.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SalesforceTenantLink Object.
		///
		/// </summary>
		public static Database.SalesforceTenantLink FromDTO(SalesforceTenantLinkDTO dto)
		{
			return new Database.SalesforceTenantLink
			{
				id = dto.id,
				syncEnabled = dto.syncEnabled,
				syncDirectionFlags = dto.syncDirectionFlags,
				pullIntervalMinutes = dto.pullIntervalMinutes,
				lastPullDate = dto.lastPullDate,
				loginUrl = dto.loginUrl,
				sfClientId = dto.sfClientId,
				sfClientSecret = dto.sfClientSecret,
				sfUsername = dto.sfUsername,
				sfPassword = dto.sfPassword,
				sfSecurityToken = dto.sfSecurityToken,
				instanceUrl = dto.instanceUrl,
				apiVersion = dto.apiVersion,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SalesforceTenantLink Object.
		///
		/// </summary>
		public void ApplyDTO(SalesforceTenantLinkDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.syncEnabled = dto.syncEnabled;
			this.syncDirectionFlags = dto.syncDirectionFlags;
			this.pullIntervalMinutes = dto.pullIntervalMinutes;
			this.lastPullDate = dto.lastPullDate;
			this.loginUrl = dto.loginUrl;
			this.sfClientId = dto.sfClientId;
			this.sfClientSecret = dto.sfClientSecret;
			this.sfUsername = dto.sfUsername;
			this.sfPassword = dto.sfPassword;
			this.sfSecurityToken = dto.sfSecurityToken;
			this.instanceUrl = dto.instanceUrl;
			this.apiVersion = dto.apiVersion;
			this.versionNumber = dto.versionNumber;
			this.objectGuid = dto.objectGuid;
			if (dto.active.HasValue == true)
			{
				this.active = dto.active.Value;
			}
			if (dto.deleted.HasValue == true)
			{
				this.deleted = dto.deleted.Value;
			}
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a SalesforceTenantLink Object.
		///
		/// </summary>
		public SalesforceTenantLink Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SalesforceTenantLink{
				id = this.id,
				tenantGuid = this.tenantGuid,
				syncEnabled = this.syncEnabled,
				syncDirectionFlags = this.syncDirectionFlags,
				pullIntervalMinutes = this.pullIntervalMinutes,
				lastPullDate = this.lastPullDate,
				loginUrl = this.loginUrl,
				sfClientId = this.sfClientId,
				sfClientSecret = this.sfClientSecret,
				sfUsername = this.sfUsername,
				sfPassword = this.sfPassword,
				sfSecurityToken = this.sfSecurityToken,
				instanceUrl = this.instanceUrl,
				apiVersion = this.apiVersion,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SalesforceTenantLink Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SalesforceTenantLink Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SalesforceTenantLink Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SalesforceTenantLink Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SalesforceTenantLink salesforceTenantLink)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (salesforceTenantLink == null)
			{
				return null;
			}

			return new {
				id = salesforceTenantLink.id,
				syncEnabled = salesforceTenantLink.syncEnabled,
				syncDirectionFlags = salesforceTenantLink.syncDirectionFlags,
				pullIntervalMinutes = salesforceTenantLink.pullIntervalMinutes,
				lastPullDate = salesforceTenantLink.lastPullDate,
				loginUrl = salesforceTenantLink.loginUrl,
				sfClientId = salesforceTenantLink.sfClientId,
				sfClientSecret = salesforceTenantLink.sfClientSecret,
				sfUsername = salesforceTenantLink.sfUsername,
				sfPassword = salesforceTenantLink.sfPassword,
				sfSecurityToken = salesforceTenantLink.sfSecurityToken,
				instanceUrl = salesforceTenantLink.instanceUrl,
				apiVersion = salesforceTenantLink.apiVersion,
				versionNumber = salesforceTenantLink.versionNumber,
				objectGuid = salesforceTenantLink.objectGuid,
				active = salesforceTenantLink.active,
				deleted = salesforceTenantLink.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SalesforceTenantLink Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SalesforceTenantLink salesforceTenantLink)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (salesforceTenantLink == null)
			{
				return null;
			}

			return new {
				id = salesforceTenantLink.id,
				syncEnabled = salesforceTenantLink.syncEnabled,
				syncDirectionFlags = salesforceTenantLink.syncDirectionFlags,
				pullIntervalMinutes = salesforceTenantLink.pullIntervalMinutes,
				lastPullDate = salesforceTenantLink.lastPullDate,
				loginUrl = salesforceTenantLink.loginUrl,
				sfClientId = salesforceTenantLink.sfClientId,
				sfClientSecret = salesforceTenantLink.sfClientSecret,
				sfUsername = salesforceTenantLink.sfUsername,
				sfPassword = salesforceTenantLink.sfPassword,
				sfSecurityToken = salesforceTenantLink.sfSecurityToken,
				instanceUrl = salesforceTenantLink.instanceUrl,
				apiVersion = salesforceTenantLink.apiVersion,
				versionNumber = salesforceTenantLink.versionNumber,
				objectGuid = salesforceTenantLink.objectGuid,
				active = salesforceTenantLink.active,
				deleted = salesforceTenantLink.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SalesforceTenantLink Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SalesforceTenantLink salesforceTenantLink)
		{
			//
			// Return a very minimal object.
			//
			if (salesforceTenantLink == null)
			{
				return null;
			}

			return new {
				id = salesforceTenantLink.id,
				name = salesforceTenantLink.syncDirectionFlags,
				description = string.Join(", ", new[] { salesforceTenantLink.syncDirectionFlags, salesforceTenantLink.loginUrl, salesforceTenantLink.sfClientId}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
