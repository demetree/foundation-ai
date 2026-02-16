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
	public partial class ScheduleOverride : IVersionTrackedEntity<ScheduleOverride>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<ScheduleOverride, ScheduleOverrideChangeHistory> GetChangeHistoryToolsetForWriting(AlertingContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<ScheduleOverride, ScheduleOverrideChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static ChangeHistoryToolset<ScheduleOverride, ScheduleOverrideChangeHistory> GetChangeHistoryToolsetForReading(AlertingContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<ScheduleOverride, ScheduleOverrideChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<ScheduleOverride>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<ScheduleOverride>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<ScheduleOverride>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this ScheduleOverride entity.");
            }

            VersionInformation<ScheduleOverride> version = new VersionInformation<ScheduleOverride>();

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
        public async Task<VersionInformation<ScheduleOverride>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this ScheduleOverride entity.");
            }

            VersionInformation<ScheduleOverride> version = new VersionInformation<ScheduleOverride>();

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
        public async Task<List<VersionInformation<ScheduleOverride>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<ScheduleOverride>> versions = new List<VersionInformation<ScheduleOverride>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<ScheduleOverride> version = new VersionInformation<ScheduleOverride>();

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
		public class ScheduleOverrideDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 onCallScheduleId { get; set; }
			public Int32? scheduleLayerId { get; set; }
			[Required]
			public DateTime startDateTime { get; set; }
			[Required]
			public DateTime endDateTime { get; set; }
			[Required]
			public Int32 scheduleOverrideTypeId { get; set; }
			public Guid? originalUserObjectGuid { get; set; }
			public Guid? replacementUserObjectGuid { get; set; }
			public String reason { get; set; }
			[Required]
			public Guid createdByUserObjectGuid { get; set; }
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
		public class ScheduleOverrideOutputDTO : ScheduleOverrideDTO
		{
			public OnCallSchedule.OnCallScheduleDTO onCallSchedule { get; set; }
			public ScheduleLayer.ScheduleLayerDTO scheduleLayer { get; set; }
			public ScheduleOverrideType.ScheduleOverrideTypeDTO scheduleOverrideType { get; set; }
		}


		/// <summary>
		///
		/// Converts a ScheduleOverride to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ScheduleOverrideDTO ToDTO()
		{
			return new ScheduleOverrideDTO
			{
				id = this.id,
				onCallScheduleId = this.onCallScheduleId,
				scheduleLayerId = this.scheduleLayerId,
				startDateTime = this.startDateTime,
				endDateTime = this.endDateTime,
				scheduleOverrideTypeId = this.scheduleOverrideTypeId,
				originalUserObjectGuid = this.originalUserObjectGuid,
				replacementUserObjectGuid = this.replacementUserObjectGuid,
				reason = this.reason,
				createdByUserObjectGuid = this.createdByUserObjectGuid,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ScheduleOverride list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ScheduleOverrideDTO> ToDTOList(List<ScheduleOverride> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduleOverrideDTO> output = new List<ScheduleOverrideDTO>();

			output.Capacity = data.Count;

			foreach (ScheduleOverride scheduleOverride in data)
			{
				output.Add(scheduleOverride.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ScheduleOverride to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ScheduleOverrideEntity type directly.
		///
		/// </summary>
		public ScheduleOverrideOutputDTO ToOutputDTO()
		{
			return new ScheduleOverrideOutputDTO
			{
				id = this.id,
				onCallScheduleId = this.onCallScheduleId,
				scheduleLayerId = this.scheduleLayerId,
				startDateTime = this.startDateTime,
				endDateTime = this.endDateTime,
				scheduleOverrideTypeId = this.scheduleOverrideTypeId,
				originalUserObjectGuid = this.originalUserObjectGuid,
				replacementUserObjectGuid = this.replacementUserObjectGuid,
				reason = this.reason,
				createdByUserObjectGuid = this.createdByUserObjectGuid,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				onCallSchedule = this.onCallSchedule?.ToDTO(),
				scheduleLayer = this.scheduleLayer?.ToDTO(),
				scheduleOverrideType = this.scheduleOverrideType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ScheduleOverride list to list of Output Data Transfer Object intended to be used for serializing a list of ScheduleOverride objects to avoid using the ScheduleOverride entity type directly.
		///
		/// </summary>
		public static List<ScheduleOverrideOutputDTO> ToOutputDTOList(List<ScheduleOverride> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ScheduleOverrideOutputDTO> output = new List<ScheduleOverrideOutputDTO>();

			output.Capacity = data.Count;

			foreach (ScheduleOverride scheduleOverride in data)
			{
				output.Add(scheduleOverride.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ScheduleOverride Object.
		///
		/// </summary>
		public static Database.ScheduleOverride FromDTO(ScheduleOverrideDTO dto)
		{
			return new Database.ScheduleOverride
			{
				id = dto.id,
				onCallScheduleId = dto.onCallScheduleId,
				scheduleLayerId = dto.scheduleLayerId,
				startDateTime = dto.startDateTime,
				endDateTime = dto.endDateTime,
				scheduleOverrideTypeId = dto.scheduleOverrideTypeId,
				originalUserObjectGuid = dto.originalUserObjectGuid,
				replacementUserObjectGuid = dto.replacementUserObjectGuid,
				reason = dto.reason,
				createdByUserObjectGuid = dto.createdByUserObjectGuid,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ScheduleOverride Object.
		///
		/// </summary>
		public void ApplyDTO(ScheduleOverrideDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.onCallScheduleId = dto.onCallScheduleId;
			this.scheduleLayerId = dto.scheduleLayerId;
			this.startDateTime = dto.startDateTime;
			this.endDateTime = dto.endDateTime;
			this.scheduleOverrideTypeId = dto.scheduleOverrideTypeId;
			this.originalUserObjectGuid = dto.originalUserObjectGuid;
			this.replacementUserObjectGuid = dto.replacementUserObjectGuid;
			this.reason = dto.reason;
			this.createdByUserObjectGuid = dto.createdByUserObjectGuid;
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
		/// Creates a deep copy clone of a ScheduleOverride Object.
		///
		/// </summary>
		public ScheduleOverride Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ScheduleOverride{
				id = this.id,
				tenantGuid = this.tenantGuid,
				onCallScheduleId = this.onCallScheduleId,
				scheduleLayerId = this.scheduleLayerId,
				startDateTime = this.startDateTime,
				endDateTime = this.endDateTime,
				scheduleOverrideTypeId = this.scheduleOverrideTypeId,
				originalUserObjectGuid = this.originalUserObjectGuid,
				replacementUserObjectGuid = this.replacementUserObjectGuid,
				reason = this.reason,
				createdByUserObjectGuid = this.createdByUserObjectGuid,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduleOverride Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ScheduleOverride Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ScheduleOverride Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduleOverride Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ScheduleOverride scheduleOverride)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (scheduleOverride == null)
			{
				return null;
			}

			return new {
				id = scheduleOverride.id,
				onCallScheduleId = scheduleOverride.onCallScheduleId,
				scheduleLayerId = scheduleOverride.scheduleLayerId,
				startDateTime = scheduleOverride.startDateTime,
				endDateTime = scheduleOverride.endDateTime,
				scheduleOverrideTypeId = scheduleOverride.scheduleOverrideTypeId,
				originalUserObjectGuid = scheduleOverride.originalUserObjectGuid,
				replacementUserObjectGuid = scheduleOverride.replacementUserObjectGuid,
				reason = scheduleOverride.reason,
				createdByUserObjectGuid = scheduleOverride.createdByUserObjectGuid,
				versionNumber = scheduleOverride.versionNumber,
				objectGuid = scheduleOverride.objectGuid,
				active = scheduleOverride.active,
				deleted = scheduleOverride.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ScheduleOverride Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ScheduleOverride scheduleOverride)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (scheduleOverride == null)
			{
				return null;
			}

			return new {
				id = scheduleOverride.id,
				onCallScheduleId = scheduleOverride.onCallScheduleId,
				scheduleLayerId = scheduleOverride.scheduleLayerId,
				startDateTime = scheduleOverride.startDateTime,
				endDateTime = scheduleOverride.endDateTime,
				scheduleOverrideTypeId = scheduleOverride.scheduleOverrideTypeId,
				originalUserObjectGuid = scheduleOverride.originalUserObjectGuid,
				replacementUserObjectGuid = scheduleOverride.replacementUserObjectGuid,
				reason = scheduleOverride.reason,
				createdByUserObjectGuid = scheduleOverride.createdByUserObjectGuid,
				versionNumber = scheduleOverride.versionNumber,
				objectGuid = scheduleOverride.objectGuid,
				active = scheduleOverride.active,
				deleted = scheduleOverride.deleted,
				onCallSchedule = OnCallSchedule.CreateMinimalAnonymous(scheduleOverride.onCallSchedule),
				scheduleLayer = ScheduleLayer.CreateMinimalAnonymous(scheduleOverride.scheduleLayer),
				scheduleOverrideType = ScheduleOverrideType.CreateMinimalAnonymous(scheduleOverride.scheduleOverrideType)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ScheduleOverride Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ScheduleOverride scheduleOverride)
		{
			//
			// Return a very minimal object.
			//
			if (scheduleOverride == null)
			{
				return null;
			}

			return new {
				id = scheduleOverride.id,
				name = scheduleOverride.reason,
				description = string.Join(", ", new[] { scheduleOverride.reason}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
