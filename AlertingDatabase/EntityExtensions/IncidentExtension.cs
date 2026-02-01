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
	public partial class Incident : IVersionTrackedEntity<Incident>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<Incident, IncidentChangeHistory> GetChangeHistoryToolsetForWriting(AlertingContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<Incident, IncidentChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static ChangeHistoryToolset<Incident, IncidentChangeHistory> GetChangeHistoryToolsetForReading(AlertingContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<Incident, IncidentChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<Incident>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<Incident>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<Incident>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this Incident entity.");
            }

            VersionInformation<Incident> version = new VersionInformation<Incident>();

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
        public async Task<VersionInformation<Incident>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this Incident entity.");
            }

            VersionInformation<Incident> version = new VersionInformation<Incident>();

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
        public async Task<List<VersionInformation<Incident>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<Incident>> versions = new List<VersionInformation<Incident>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<Incident> version = new VersionInformation<Incident>();

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
                    version.data = await chts.GetVersionAsync(this, 1).ConfigureAwait(false);
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
		public class IncidentDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String incidentKey { get; set; }
			[Required]
			public Int32 serviceId { get; set; }
			[Required]
			public String title { get; set; }
			public String description { get; set; }
			[Required]
			public Int32 severityTypeId { get; set; }
			[Required]
			public Int32 incidentStatusTypeId { get; set; }
			[Required]
			public DateTime createdAt { get; set; }
			public Int32? escalationRuleId { get; set; }
			public Int32? currentRepeatCount { get; set; }
			public DateTime? nextEscalationAt { get; set; }
			public DateTime? acknowledgedAt { get; set; }
			public DateTime? resolvedAt { get; set; }
			public Guid? currentAssigneeObjectGuid { get; set; }
			public String sourcePayloadJson { get; set; }
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
		public class IncidentOutputDTO : IncidentDTO
		{
			public EscalationRule.EscalationRuleDTO escalationRule { get; set; }
			public IncidentStatusType.IncidentStatusTypeDTO incidentStatusType { get; set; }
			public Service.ServiceDTO service { get; set; }
			public SeverityType.SeverityTypeDTO severityType { get; set; }
		}


		/// <summary>
		///
		/// Converts a Incident to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public IncidentDTO ToDTO()
		{
			return new IncidentDTO
			{
				id = this.id,
				incidentKey = this.incidentKey,
				serviceId = this.serviceId,
				title = this.title,
				description = this.description,
				severityTypeId = this.severityTypeId,
				incidentStatusTypeId = this.incidentStatusTypeId,
				createdAt = this.createdAt,
				escalationRuleId = this.escalationRuleId,
				currentRepeatCount = this.currentRepeatCount,
				nextEscalationAt = this.nextEscalationAt,
				acknowledgedAt = this.acknowledgedAt,
				resolvedAt = this.resolvedAt,
				currentAssigneeObjectGuid = this.currentAssigneeObjectGuid,
				sourcePayloadJson = this.sourcePayloadJson,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a Incident list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<IncidentDTO> ToDTOList(List<Incident> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IncidentDTO> output = new List<IncidentDTO>();

			output.Capacity = data.Count;

			foreach (Incident incident in data)
			{
				output.Add(incident.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a Incident to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the IncidentEntity type directly.
		///
		/// </summary>
		public IncidentOutputDTO ToOutputDTO()
		{
			return new IncidentOutputDTO
			{
				id = this.id,
				incidentKey = this.incidentKey,
				serviceId = this.serviceId,
				title = this.title,
				description = this.description,
				severityTypeId = this.severityTypeId,
				incidentStatusTypeId = this.incidentStatusTypeId,
				createdAt = this.createdAt,
				escalationRuleId = this.escalationRuleId,
				currentRepeatCount = this.currentRepeatCount,
				nextEscalationAt = this.nextEscalationAt,
				acknowledgedAt = this.acknowledgedAt,
				resolvedAt = this.resolvedAt,
				currentAssigneeObjectGuid = this.currentAssigneeObjectGuid,
				sourcePayloadJson = this.sourcePayloadJson,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				escalationRule = this.escalationRule?.ToDTO(),
				incidentStatusType = this.incidentStatusType?.ToDTO(),
				service = this.service?.ToDTO(),
				severityType = this.severityType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a Incident list to list of Output Data Transfer Object intended to be used for serializing a list of Incident objects to avoid using the Incident entity type directly.
		///
		/// </summary>
		public static List<IncidentOutputDTO> ToOutputDTOList(List<Incident> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IncidentOutputDTO> output = new List<IncidentOutputDTO>();

			output.Capacity = data.Count;

			foreach (Incident incident in data)
			{
				output.Add(incident.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a Incident Object.
		///
		/// </summary>
		public static Database.Incident FromDTO(IncidentDTO dto)
		{
			return new Database.Incident
			{
				id = dto.id,
				incidentKey = dto.incidentKey,
				serviceId = dto.serviceId,
				title = dto.title,
				description = dto.description,
				severityTypeId = dto.severityTypeId,
				incidentStatusTypeId = dto.incidentStatusTypeId,
				createdAt = dto.createdAt,
				escalationRuleId = dto.escalationRuleId,
				currentRepeatCount = dto.currentRepeatCount,
				nextEscalationAt = dto.nextEscalationAt,
				acknowledgedAt = dto.acknowledgedAt,
				resolvedAt = dto.resolvedAt,
				currentAssigneeObjectGuid = dto.currentAssigneeObjectGuid,
				sourcePayloadJson = dto.sourcePayloadJson,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a Incident Object.
		///
		/// </summary>
		public void ApplyDTO(IncidentDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.incidentKey = dto.incidentKey;
			this.serviceId = dto.serviceId;
			this.title = dto.title;
			this.description = dto.description;
			this.severityTypeId = dto.severityTypeId;
			this.incidentStatusTypeId = dto.incidentStatusTypeId;
			this.createdAt = dto.createdAt;
			this.escalationRuleId = dto.escalationRuleId;
			this.currentRepeatCount = dto.currentRepeatCount;
			this.nextEscalationAt = dto.nextEscalationAt;
			this.acknowledgedAt = dto.acknowledgedAt;
			this.resolvedAt = dto.resolvedAt;
			this.currentAssigneeObjectGuid = dto.currentAssigneeObjectGuid;
			this.sourcePayloadJson = dto.sourcePayloadJson;
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
		/// Creates a deep copy clone of a Incident Object.
		///
		/// </summary>
		public Incident Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new Incident{
				id = this.id,
				tenantGuid = this.tenantGuid,
				incidentKey = this.incidentKey,
				serviceId = this.serviceId,
				title = this.title,
				description = this.description,
				severityTypeId = this.severityTypeId,
				incidentStatusTypeId = this.incidentStatusTypeId,
				createdAt = this.createdAt,
				escalationRuleId = this.escalationRuleId,
				currentRepeatCount = this.currentRepeatCount,
				nextEscalationAt = this.nextEscalationAt,
				acknowledgedAt = this.acknowledgedAt,
				resolvedAt = this.resolvedAt,
				currentAssigneeObjectGuid = this.currentAssigneeObjectGuid,
				sourcePayloadJson = this.sourcePayloadJson,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Incident Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Incident Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a Incident Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a Incident Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.Incident incident)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (incident == null)
			{
				return null;
			}

			return new {
				id = incident.id,
				incidentKey = incident.incidentKey,
				serviceId = incident.serviceId,
				title = incident.title,
				description = incident.description,
				severityTypeId = incident.severityTypeId,
				incidentStatusTypeId = incident.incidentStatusTypeId,
				createdAt = incident.createdAt,
				escalationRuleId = incident.escalationRuleId,
				currentRepeatCount = incident.currentRepeatCount,
				nextEscalationAt = incident.nextEscalationAt,
				acknowledgedAt = incident.acknowledgedAt,
				resolvedAt = incident.resolvedAt,
				currentAssigneeObjectGuid = incident.currentAssigneeObjectGuid,
				sourcePayloadJson = incident.sourcePayloadJson,
				versionNumber = incident.versionNumber,
				objectGuid = incident.objectGuid,
				active = incident.active,
				deleted = incident.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a Incident Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(Incident incident)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (incident == null)
			{
				return null;
			}

			return new {
				id = incident.id,
				incidentKey = incident.incidentKey,
				serviceId = incident.serviceId,
				title = incident.title,
				description = incident.description,
				severityTypeId = incident.severityTypeId,
				incidentStatusTypeId = incident.incidentStatusTypeId,
				createdAt = incident.createdAt,
				escalationRuleId = incident.escalationRuleId,
				currentRepeatCount = incident.currentRepeatCount,
				nextEscalationAt = incident.nextEscalationAt,
				acknowledgedAt = incident.acknowledgedAt,
				resolvedAt = incident.resolvedAt,
				currentAssigneeObjectGuid = incident.currentAssigneeObjectGuid,
				sourcePayloadJson = incident.sourcePayloadJson,
				versionNumber = incident.versionNumber,
				objectGuid = incident.objectGuid,
				active = incident.active,
				deleted = incident.deleted,
				escalationRule = EscalationRule.CreateMinimalAnonymous(incident.escalationRule),
				incidentStatusType = IncidentStatusType.CreateMinimalAnonymous(incident.incidentStatusType),
				service = Service.CreateMinimalAnonymous(incident.service),
				severityType = SeverityType.CreateMinimalAnonymous(incident.severityType)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a Incident Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(Incident incident)
		{
			//
			// Return a very minimal object.
			//
			if (incident == null)
			{
				return null;
			}

			return new {
				id = incident.id,
				description = incident.description,
				name = incident.incidentKey
			 };
		}
	}
}
