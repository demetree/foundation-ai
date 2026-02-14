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
	public partial class TenantProfile : IVersionTrackedEntity<TenantProfile>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<TenantProfile, TenantProfileChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<TenantProfile, TenantProfileChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static ChangeHistoryToolset<TenantProfile, TenantProfileChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<TenantProfile, TenantProfileChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<TenantProfile>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<TenantProfile>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<TenantProfile>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this TenantProfile entity.");
            }

            VersionInformation<TenantProfile> version = new VersionInformation<TenantProfile>();

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
        public async Task<VersionInformation<TenantProfile>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this TenantProfile entity.");
            }

            VersionInformation<TenantProfile> version = new VersionInformation<TenantProfile>();

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
        public async Task<List<VersionInformation<TenantProfile>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<TenantProfile>> versions = new List<VersionInformation<TenantProfile>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<TenantProfile> version = new VersionInformation<TenantProfile>();

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
		public class TenantProfileDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
			public String companyLogoFileName { get; set; }
			public Int64? companyLogoSize { get; set; }
			public Byte[] companyLogoData { get; set; }
			public String companyLogoMimeType { get; set; }
			public String addressLine1 { get; set; }
			public String addressLine2 { get; set; }
			public String addressLine3 { get; set; }
			public String city { get; set; }
			public String postalCode { get; set; }
			public Int32? stateProvinceId { get; set; }
			public Int32? countryId { get; set; }
			public Int32? timeZoneId { get; set; }
			public String phoneNumber { get; set; }
			public String email { get; set; }
			public String website { get; set; }
			public Double? latitude { get; set; }
			public Double? longitude { get; set; }
			public String primaryColor { get; set; }
			public String secondaryColor { get; set; }
			[Required]
			public Boolean displaysMetric { get; set; }
			[Required]
			public Boolean displaysUSTerms { get; set; }
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
		public class TenantProfileOutputDTO : TenantProfileDTO
		{
			public Country.CountryDTO country { get; set; }
			public StateProvince.StateProvinceDTO stateProvince { get; set; }
			public TimeZone.TimeZoneDTO timeZone { get; set; }
		}


		/// <summary>
		///
		/// Converts a TenantProfile to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public TenantProfileDTO ToDTO()
		{
			return new TenantProfileDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				companyLogoFileName = this.companyLogoFileName,
				companyLogoSize = this.companyLogoSize,
				companyLogoData = this.companyLogoData,
				companyLogoMimeType = this.companyLogoMimeType,
				addressLine1 = this.addressLine1,
				addressLine2 = this.addressLine2,
				addressLine3 = this.addressLine3,
				city = this.city,
				postalCode = this.postalCode,
				stateProvinceId = this.stateProvinceId,
				countryId = this.countryId,
				timeZoneId = this.timeZoneId,
				phoneNumber = this.phoneNumber,
				email = this.email,
				website = this.website,
				latitude = this.latitude,
				longitude = this.longitude,
				primaryColor = this.primaryColor,
				secondaryColor = this.secondaryColor,
				displaysMetric = this.displaysMetric,
				displaysUSTerms = this.displaysUSTerms,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a TenantProfile list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<TenantProfileDTO> ToDTOList(List<TenantProfile> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TenantProfileDTO> output = new List<TenantProfileDTO>();

			output.Capacity = data.Count;

			foreach (TenantProfile tenantProfile in data)
			{
				output.Add(tenantProfile.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a TenantProfile to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the TenantProfileEntity type directly.
		///
		/// </summary>
		public TenantProfileOutputDTO ToOutputDTO()
		{
			return new TenantProfileOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				companyLogoFileName = this.companyLogoFileName,
				companyLogoSize = this.companyLogoSize,
				companyLogoData = this.companyLogoData,
				companyLogoMimeType = this.companyLogoMimeType,
				addressLine1 = this.addressLine1,
				addressLine2 = this.addressLine2,
				addressLine3 = this.addressLine3,
				city = this.city,
				postalCode = this.postalCode,
				stateProvinceId = this.stateProvinceId,
				countryId = this.countryId,
				timeZoneId = this.timeZoneId,
				phoneNumber = this.phoneNumber,
				email = this.email,
				website = this.website,
				latitude = this.latitude,
				longitude = this.longitude,
				primaryColor = this.primaryColor,
				secondaryColor = this.secondaryColor,
				displaysMetric = this.displaysMetric,
				displaysUSTerms = this.displaysUSTerms,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				country = this.country?.ToDTO(),
				stateProvince = this.stateProvince?.ToDTO(),
				timeZone = this.timeZone?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a TenantProfile list to list of Output Data Transfer Object intended to be used for serializing a list of TenantProfile objects to avoid using the TenantProfile entity type directly.
		///
		/// </summary>
		public static List<TenantProfileOutputDTO> ToOutputDTOList(List<TenantProfile> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TenantProfileOutputDTO> output = new List<TenantProfileOutputDTO>();

			output.Capacity = data.Count;

			foreach (TenantProfile tenantProfile in data)
			{
				output.Add(tenantProfile.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a TenantProfile Object.
		///
		/// </summary>
		public static Database.TenantProfile FromDTO(TenantProfileDTO dto)
		{
			return new Database.TenantProfile
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				companyLogoFileName = dto.companyLogoFileName,
				companyLogoSize = dto.companyLogoSize,
				companyLogoData = dto.companyLogoData,
				companyLogoMimeType = dto.companyLogoMimeType,
				addressLine1 = dto.addressLine1,
				addressLine2 = dto.addressLine2,
				addressLine3 = dto.addressLine3,
				city = dto.city,
				postalCode = dto.postalCode,
				stateProvinceId = dto.stateProvinceId,
				countryId = dto.countryId,
				timeZoneId = dto.timeZoneId,
				phoneNumber = dto.phoneNumber,
				email = dto.email,
				website = dto.website,
				latitude = dto.latitude,
				longitude = dto.longitude,
				primaryColor = dto.primaryColor,
				secondaryColor = dto.secondaryColor,
				displaysMetric = dto.displaysMetric,
				displaysUSTerms = dto.displaysUSTerms,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a TenantProfile Object.
		///
		/// </summary>
		public void ApplyDTO(TenantProfileDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.companyLogoFileName = dto.companyLogoFileName;
			this.companyLogoSize = dto.companyLogoSize;
			this.companyLogoData = dto.companyLogoData;
			this.companyLogoMimeType = dto.companyLogoMimeType;
			this.addressLine1 = dto.addressLine1;
			this.addressLine2 = dto.addressLine2;
			this.addressLine3 = dto.addressLine3;
			this.city = dto.city;
			this.postalCode = dto.postalCode;
			this.stateProvinceId = dto.stateProvinceId;
			this.countryId = dto.countryId;
			this.timeZoneId = dto.timeZoneId;
			this.phoneNumber = dto.phoneNumber;
			this.email = dto.email;
			this.website = dto.website;
			this.latitude = dto.latitude;
			this.longitude = dto.longitude;
			this.primaryColor = dto.primaryColor;
			this.secondaryColor = dto.secondaryColor;
			this.displaysMetric = dto.displaysMetric;
			this.displaysUSTerms = dto.displaysUSTerms;
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
		/// Creates a deep copy clone of a TenantProfile Object.
		///
		/// </summary>
		public TenantProfile Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new TenantProfile{
				id = this.id,
				tenantGuid = this.tenantGuid,
				name = this.name,
				description = this.description,
				companyLogoFileName = this.companyLogoFileName,
				companyLogoSize = this.companyLogoSize,
				companyLogoData = this.companyLogoData,
				companyLogoMimeType = this.companyLogoMimeType,
				addressLine1 = this.addressLine1,
				addressLine2 = this.addressLine2,
				addressLine3 = this.addressLine3,
				city = this.city,
				postalCode = this.postalCode,
				stateProvinceId = this.stateProvinceId,
				countryId = this.countryId,
				timeZoneId = this.timeZoneId,
				phoneNumber = this.phoneNumber,
				email = this.email,
				website = this.website,
				latitude = this.latitude,
				longitude = this.longitude,
				primaryColor = this.primaryColor,
				secondaryColor = this.secondaryColor,
				displaysMetric = this.displaysMetric,
				displaysUSTerms = this.displaysUSTerms,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TenantProfile Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TenantProfile Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a TenantProfile Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a TenantProfile Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.TenantProfile tenantProfile)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (tenantProfile == null)
			{
				return null;
			}

			return new {
				id = tenantProfile.id,
				name = tenantProfile.name,
				description = tenantProfile.description,
				companyLogoFileName = tenantProfile.companyLogoFileName,
				companyLogoSize = tenantProfile.companyLogoSize,
				companyLogoData = tenantProfile.companyLogoData,
				companyLogoMimeType = tenantProfile.companyLogoMimeType,
				addressLine1 = tenantProfile.addressLine1,
				addressLine2 = tenantProfile.addressLine2,
				addressLine3 = tenantProfile.addressLine3,
				city = tenantProfile.city,
				postalCode = tenantProfile.postalCode,
				stateProvinceId = tenantProfile.stateProvinceId,
				countryId = tenantProfile.countryId,
				timeZoneId = tenantProfile.timeZoneId,
				phoneNumber = tenantProfile.phoneNumber,
				email = tenantProfile.email,
				website = tenantProfile.website,
				latitude = tenantProfile.latitude,
				longitude = tenantProfile.longitude,
				primaryColor = tenantProfile.primaryColor,
				secondaryColor = tenantProfile.secondaryColor,
				displaysMetric = tenantProfile.displaysMetric,
				displaysUSTerms = tenantProfile.displaysUSTerms,
				versionNumber = tenantProfile.versionNumber,
				objectGuid = tenantProfile.objectGuid,
				active = tenantProfile.active,
				deleted = tenantProfile.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a TenantProfile Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(TenantProfile tenantProfile)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (tenantProfile == null)
			{
				return null;
			}

			return new {
				id = tenantProfile.id,
				name = tenantProfile.name,
				description = tenantProfile.description,
				companyLogoFileName = tenantProfile.companyLogoFileName,
				companyLogoSize = tenantProfile.companyLogoSize,
				companyLogoData = tenantProfile.companyLogoData,
				companyLogoMimeType = tenantProfile.companyLogoMimeType,
				addressLine1 = tenantProfile.addressLine1,
				addressLine2 = tenantProfile.addressLine2,
				addressLine3 = tenantProfile.addressLine3,
				city = tenantProfile.city,
				postalCode = tenantProfile.postalCode,
				stateProvinceId = tenantProfile.stateProvinceId,
				countryId = tenantProfile.countryId,
				timeZoneId = tenantProfile.timeZoneId,
				phoneNumber = tenantProfile.phoneNumber,
				email = tenantProfile.email,
				website = tenantProfile.website,
				latitude = tenantProfile.latitude,
				longitude = tenantProfile.longitude,
				primaryColor = tenantProfile.primaryColor,
				secondaryColor = tenantProfile.secondaryColor,
				displaysMetric = tenantProfile.displaysMetric,
				displaysUSTerms = tenantProfile.displaysUSTerms,
				versionNumber = tenantProfile.versionNumber,
				objectGuid = tenantProfile.objectGuid,
				active = tenantProfile.active,
				deleted = tenantProfile.deleted,
				country = Country.CreateMinimalAnonymous(tenantProfile.country),
				stateProvince = StateProvince.CreateMinimalAnonymous(tenantProfile.stateProvince),
				timeZone = TimeZone.CreateMinimalAnonymous(tenantProfile.timeZone)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a TenantProfile Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(TenantProfile tenantProfile)
		{
			//
			// Return a very minimal object.
			//
			if (tenantProfile == null)
			{
				return null;
			}

			return new {
				id = tenantProfile.id,
				name = tenantProfile.name,
				description = tenantProfile.description,
			 };
		}
	}
}
