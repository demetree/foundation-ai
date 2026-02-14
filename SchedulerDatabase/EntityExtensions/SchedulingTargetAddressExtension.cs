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
	public partial class SchedulingTargetAddress : IVersionTrackedEntity<SchedulingTargetAddress>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<SchedulingTargetAddress, SchedulingTargetAddressChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<SchedulingTargetAddress, SchedulingTargetAddressChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static ChangeHistoryToolset<SchedulingTargetAddress, SchedulingTargetAddressChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<SchedulingTargetAddress, SchedulingTargetAddressChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<SchedulingTargetAddress>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<SchedulingTargetAddress>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<SchedulingTargetAddress>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this SchedulingTargetAddress entity.");
            }

            VersionInformation<SchedulingTargetAddress> version = new VersionInformation<SchedulingTargetAddress>();

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
        public async Task<VersionInformation<SchedulingTargetAddress>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this SchedulingTargetAddress entity.");
            }

            VersionInformation<SchedulingTargetAddress> version = new VersionInformation<SchedulingTargetAddress>();

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
        public async Task<List<VersionInformation<SchedulingTargetAddress>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<SchedulingTargetAddress>> versions = new List<VersionInformation<SchedulingTargetAddress>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<SchedulingTargetAddress> version = new VersionInformation<SchedulingTargetAddress>();

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
		public class SchedulingTargetAddressDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 schedulingTargetId { get; set; }
			public Int32? clientId { get; set; }
			[Required]
			public String addressLine1 { get; set; }
			public String addressLine2 { get; set; }
			[Required]
			public String city { get; set; }
			public String postalCode { get; set; }
			[Required]
			public Int32 stateProvinceId { get; set; }
			[Required]
			public Int32 countryId { get; set; }
			public Double? latitude { get; set; }
			public Double? longitude { get; set; }
			public String label { get; set; }
			[Required]
			public Boolean isPrimary { get; set; }
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
		public class SchedulingTargetAddressOutputDTO : SchedulingTargetAddressDTO
		{
			public Client.ClientDTO client { get; set; }
			public Country.CountryDTO country { get; set; }
			public SchedulingTarget.SchedulingTargetDTO schedulingTarget { get; set; }
			public StateProvince.StateProvinceDTO stateProvince { get; set; }
		}


		/// <summary>
		///
		/// Converts a SchedulingTargetAddress to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SchedulingTargetAddressDTO ToDTO()
		{
			return new SchedulingTargetAddressDTO
			{
				id = this.id,
				schedulingTargetId = this.schedulingTargetId,
				clientId = this.clientId,
				addressLine1 = this.addressLine1,
				addressLine2 = this.addressLine2,
				city = this.city,
				postalCode = this.postalCode,
				stateProvinceId = this.stateProvinceId,
				countryId = this.countryId,
				latitude = this.latitude,
				longitude = this.longitude,
				label = this.label,
				isPrimary = this.isPrimary,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a SchedulingTargetAddress list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SchedulingTargetAddressDTO> ToDTOList(List<SchedulingTargetAddress> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SchedulingTargetAddressDTO> output = new List<SchedulingTargetAddressDTO>();

			output.Capacity = data.Count;

			foreach (SchedulingTargetAddress schedulingTargetAddress in data)
			{
				output.Add(schedulingTargetAddress.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SchedulingTargetAddress to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SchedulingTargetAddressEntity type directly.
		///
		/// </summary>
		public SchedulingTargetAddressOutputDTO ToOutputDTO()
		{
			return new SchedulingTargetAddressOutputDTO
			{
				id = this.id,
				schedulingTargetId = this.schedulingTargetId,
				clientId = this.clientId,
				addressLine1 = this.addressLine1,
				addressLine2 = this.addressLine2,
				city = this.city,
				postalCode = this.postalCode,
				stateProvinceId = this.stateProvinceId,
				countryId = this.countryId,
				latitude = this.latitude,
				longitude = this.longitude,
				label = this.label,
				isPrimary = this.isPrimary,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				client = this.client?.ToDTO(),
				country = this.country?.ToDTO(),
				schedulingTarget = this.schedulingTarget?.ToDTO(),
				stateProvince = this.stateProvince?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SchedulingTargetAddress list to list of Output Data Transfer Object intended to be used for serializing a list of SchedulingTargetAddress objects to avoid using the SchedulingTargetAddress entity type directly.
		///
		/// </summary>
		public static List<SchedulingTargetAddressOutputDTO> ToOutputDTOList(List<SchedulingTargetAddress> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SchedulingTargetAddressOutputDTO> output = new List<SchedulingTargetAddressOutputDTO>();

			output.Capacity = data.Count;

			foreach (SchedulingTargetAddress schedulingTargetAddress in data)
			{
				output.Add(schedulingTargetAddress.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SchedulingTargetAddress Object.
		///
		/// </summary>
		public static Database.SchedulingTargetAddress FromDTO(SchedulingTargetAddressDTO dto)
		{
			return new Database.SchedulingTargetAddress
			{
				id = dto.id,
				schedulingTargetId = dto.schedulingTargetId,
				clientId = dto.clientId,
				addressLine1 = dto.addressLine1,
				addressLine2 = dto.addressLine2,
				city = dto.city,
				postalCode = dto.postalCode,
				stateProvinceId = dto.stateProvinceId,
				countryId = dto.countryId,
				latitude = dto.latitude,
				longitude = dto.longitude,
				label = dto.label,
				isPrimary = dto.isPrimary,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SchedulingTargetAddress Object.
		///
		/// </summary>
		public void ApplyDTO(SchedulingTargetAddressDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.schedulingTargetId = dto.schedulingTargetId;
			this.clientId = dto.clientId;
			this.addressLine1 = dto.addressLine1;
			this.addressLine2 = dto.addressLine2;
			this.city = dto.city;
			this.postalCode = dto.postalCode;
			this.stateProvinceId = dto.stateProvinceId;
			this.countryId = dto.countryId;
			this.latitude = dto.latitude;
			this.longitude = dto.longitude;
			this.label = dto.label;
			this.isPrimary = dto.isPrimary;
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
		/// Creates a deep copy clone of a SchedulingTargetAddress Object.
		///
		/// </summary>
		public SchedulingTargetAddress Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SchedulingTargetAddress{
				id = this.id,
				tenantGuid = this.tenantGuid,
				schedulingTargetId = this.schedulingTargetId,
				clientId = this.clientId,
				addressLine1 = this.addressLine1,
				addressLine2 = this.addressLine2,
				city = this.city,
				postalCode = this.postalCode,
				stateProvinceId = this.stateProvinceId,
				countryId = this.countryId,
				latitude = this.latitude,
				longitude = this.longitude,
				label = this.label,
				isPrimary = this.isPrimary,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SchedulingTargetAddress Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SchedulingTargetAddress Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SchedulingTargetAddress Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SchedulingTargetAddress Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SchedulingTargetAddress schedulingTargetAddress)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (schedulingTargetAddress == null)
			{
				return null;
			}

			return new {
				id = schedulingTargetAddress.id,
				schedulingTargetId = schedulingTargetAddress.schedulingTargetId,
				clientId = schedulingTargetAddress.clientId,
				addressLine1 = schedulingTargetAddress.addressLine1,
				addressLine2 = schedulingTargetAddress.addressLine2,
				city = schedulingTargetAddress.city,
				postalCode = schedulingTargetAddress.postalCode,
				stateProvinceId = schedulingTargetAddress.stateProvinceId,
				countryId = schedulingTargetAddress.countryId,
				latitude = schedulingTargetAddress.latitude,
				longitude = schedulingTargetAddress.longitude,
				label = schedulingTargetAddress.label,
				isPrimary = schedulingTargetAddress.isPrimary,
				versionNumber = schedulingTargetAddress.versionNumber,
				objectGuid = schedulingTargetAddress.objectGuid,
				active = schedulingTargetAddress.active,
				deleted = schedulingTargetAddress.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SchedulingTargetAddress Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SchedulingTargetAddress schedulingTargetAddress)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (schedulingTargetAddress == null)
			{
				return null;
			}

			return new {
				id = schedulingTargetAddress.id,
				schedulingTargetId = schedulingTargetAddress.schedulingTargetId,
				clientId = schedulingTargetAddress.clientId,
				addressLine1 = schedulingTargetAddress.addressLine1,
				addressLine2 = schedulingTargetAddress.addressLine2,
				city = schedulingTargetAddress.city,
				postalCode = schedulingTargetAddress.postalCode,
				stateProvinceId = schedulingTargetAddress.stateProvinceId,
				countryId = schedulingTargetAddress.countryId,
				latitude = schedulingTargetAddress.latitude,
				longitude = schedulingTargetAddress.longitude,
				label = schedulingTargetAddress.label,
				isPrimary = schedulingTargetAddress.isPrimary,
				versionNumber = schedulingTargetAddress.versionNumber,
				objectGuid = schedulingTargetAddress.objectGuid,
				active = schedulingTargetAddress.active,
				deleted = schedulingTargetAddress.deleted,
				client = Client.CreateMinimalAnonymous(schedulingTargetAddress.client),
				country = Country.CreateMinimalAnonymous(schedulingTargetAddress.country),
				schedulingTarget = SchedulingTarget.CreateMinimalAnonymous(schedulingTargetAddress.schedulingTarget),
				stateProvince = StateProvince.CreateMinimalAnonymous(schedulingTargetAddress.stateProvince)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SchedulingTargetAddress Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SchedulingTargetAddress schedulingTargetAddress)
		{
			//
			// Return a very minimal object.
			//
			if (schedulingTargetAddress == null)
			{
				return null;
			}

			return new {
				id = schedulingTargetAddress.id,
				name = schedulingTargetAddress.addressLine1,
				description = string.Join(", ", new[] { schedulingTargetAddress.addressLine1, schedulingTargetAddress.addressLine2, schedulingTargetAddress.city}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
