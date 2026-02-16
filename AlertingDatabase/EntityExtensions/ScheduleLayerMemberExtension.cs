using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Alerting.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class ScheduleLayerMember : IVersionTrackedEntity<ScheduleLayerMember>, IAnonymousConvertible
	{
        /// <summary>
        /// This is for setting the context for change history inquiries.
        /// </summary>
        private AlertingContext _contextForVersionInquiry = null;
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
        public static ChangeHistoryToolset<ScheduleLayerMember, ScheduleLayerMemberChangeHistory> GetChangeHistoryToolsetForWriting(AlertingContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<ScheduleLayerMember, ScheduleLayerMemberChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static ChangeHistoryToolset<ScheduleLayerMember, ScheduleLayerMemberChangeHistory> GetChangeHistoryToolsetForReading(AlertingContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<ScheduleLayerMember, ScheduleLayerMemberChangeHistory>(context, cancellationToken);
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
        public void SetupVersionInquiry(AlertingContext context, Guid tenantGuid)
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
        public async Task<VersionInformation<ScheduleLayerMember>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<ScheduleLayerMember>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<ScheduleLayerMember>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this ScheduleLayerMember entity.");
            }

            VersionInformation<ScheduleLayerMember> version = new VersionInformation<ScheduleLayerMember>();

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
        public async Task<VersionInformation<ScheduleLayerMember>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this ScheduleLayerMember entity.");
            }

            VersionInformation<ScheduleLayerMember> version = new VersionInformation<ScheduleLayerMember>();

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
        public async Task<List<VersionInformation<ScheduleLayerMember>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<ScheduleLayerMember>> versions = new List<VersionInformation<ScheduleLayerMember>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<ScheduleLayerMember> version = new VersionInformation<ScheduleLayerMember>();

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
		public class ScheduleLayerMemberDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 scheduleLayerId { get; set; }
			[Required]
			public Int32 position { get; set; }
			[Required]
			public Guid securityUserObjectGuid { get; set; }
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
		public class ScheduleLayerMemberOutputDTO : ScheduleLayerMemberDTO
		{
			public ScheduleLayer.ScheduleLayerDTO scheduleLayer { get; set; }
		}


		/// <summary>
		///
		/// Converts a ScheduleLayerMember to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ScheduleLayerMemberDTO ToDTO()
		{
			return new ScheduleLayerMemberDTO
			{
				id = this.id,
				scheduleLayerId = this.scheduleLayerId,
				position = this.position,
				securityUserObjectGuid = this.securityUserObjectGuid,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ScheduleLayerMember list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ScheduleLayerMemberDTO> ToDTOList(List<ScheduleLayerMember> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduleLayerMemberDTO> output = new List<ScheduleLayerMemberDTO>();

			output.Capacity = data.Count;

			foreach (ScheduleLayerMember scheduleLayerMember in data)
			{
				output.Add(scheduleLayerMember.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ScheduleLayerMember to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ScheduleLayerMemberEntity type directly.
		///
		/// </summary>
		public ScheduleLayerMemberOutputDTO ToOutputDTO()
		{
			return new ScheduleLayerMemberOutputDTO
			{
				id = this.id,
				scheduleLayerId = this.scheduleLayerId,
				position = this.position,
				securityUserObjectGuid = this.securityUserObjectGuid,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				scheduleLayer = this.scheduleLayer?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ScheduleLayerMember list to list of Output Data Transfer Object intended to be used for serializing a list of ScheduleLayerMember objects to avoid using the ScheduleLayerMember entity type directly.
		///
		/// </summary>
		public static List<ScheduleLayerMemberOutputDTO> ToOutputDTOList(List<ScheduleLayerMember> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduleLayerMemberOutputDTO> output = new List<ScheduleLayerMemberOutputDTO>();

			output.Capacity = data.Count;

			foreach (ScheduleLayerMember scheduleLayerMember in data)
			{
				output.Add(scheduleLayerMember.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ScheduleLayerMember Object.
		///
		/// </summary>
		public static Database.ScheduleLayerMember FromDTO(ScheduleLayerMemberDTO dto)
		{
			return new Database.ScheduleLayerMember
			{
				id = dto.id,
				scheduleLayerId = dto.scheduleLayerId,
				position = dto.position,
				securityUserObjectGuid = dto.securityUserObjectGuid,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ScheduleLayerMember Object.
		///
		/// </summary>
		public void ApplyDTO(ScheduleLayerMemberDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.scheduleLayerId = dto.scheduleLayerId;
			this.position = dto.position;
			this.securityUserObjectGuid = dto.securityUserObjectGuid;
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
		/// Creates a deep copy clone of a ScheduleLayerMember Object.
		///
		/// </summary>
		public ScheduleLayerMember Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ScheduleLayerMember{
				id = this.id,
				tenantGuid = this.tenantGuid,
				scheduleLayerId = this.scheduleLayerId,
				position = this.position,
				securityUserObjectGuid = this.securityUserObjectGuid,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduleLayerMember Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduleLayerMember Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ScheduleLayerMember Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduleLayerMember Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ScheduleLayerMember scheduleLayerMember)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (scheduleLayerMember == null)
			{
				return null;
			}

			return new {
				id = scheduleLayerMember.id,
				scheduleLayerId = scheduleLayerMember.scheduleLayerId,
				position = scheduleLayerMember.position,
				securityUserObjectGuid = scheduleLayerMember.securityUserObjectGuid,
				versionNumber = scheduleLayerMember.versionNumber,
				objectGuid = scheduleLayerMember.objectGuid,
				active = scheduleLayerMember.active,
				deleted = scheduleLayerMember.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduleLayerMember Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ScheduleLayerMember scheduleLayerMember)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (scheduleLayerMember == null)
			{
				return null;
			}

			return new {
				id = scheduleLayerMember.id,
				scheduleLayerId = scheduleLayerMember.scheduleLayerId,
				position = scheduleLayerMember.position,
				securityUserObjectGuid = scheduleLayerMember.securityUserObjectGuid,
				versionNumber = scheduleLayerMember.versionNumber,
				objectGuid = scheduleLayerMember.objectGuid,
				active = scheduleLayerMember.active,
				deleted = scheduleLayerMember.deleted,
				scheduleLayer = ScheduleLayer.CreateMinimalAnonymous(scheduleLayerMember.scheduleLayer)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ScheduleLayerMember Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ScheduleLayerMember scheduleLayerMember)
		{
			//
			// Return a very minimal object.
			//
			if (scheduleLayerMember == null)
			{
				return null;
			}

			return new {
				id = scheduleLayerMember.id,
				name = scheduleLayerMember.id,
				description = scheduleLayerMember.id
			 };
		}
	}
}
