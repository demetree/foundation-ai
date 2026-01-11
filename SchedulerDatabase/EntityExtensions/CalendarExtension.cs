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
	public partial class Calendar : IVersionTrackedEntity<Calendar>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<Calendar, CalendarChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<Calendar, CalendarChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static ChangeHistoryToolset<Calendar, CalendarChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<Calendar, CalendarChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<Calendar>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<Calendar>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<Calendar>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this Calendar entity.");
            }

            VersionInformation<Calendar> version = new VersionInformation<Calendar>();

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
        public async Task<VersionInformation<Calendar>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this Calendar entity.");
            }

            VersionInformation<Calendar> version = new VersionInformation<Calendar>();

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
        public async Task<List<VersionInformation<Calendar>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<Calendar>> versions = new List<VersionInformation<Calendar>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<Calendar> version = new VersionInformation<Calendar>();

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
		public class CalendarDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
			public Int32? officeId { get; set; }
			public Boolean? isDefault { get; set; }
			public Int32? iconId { get; set; }
			public String color { get; set; }
			[Required]
			public Guid objectGuid { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
			public Int32 versionNumber { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class CalendarOutputDTO : CalendarDTO
		{
			public Icon.IconDTO icon { get; set; }
			public Office.OfficeDTO office { get; set; }
		}


		/// <summary>
		///
		/// Converts a Calendar to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public CalendarDTO ToDTO()
		{
			return new CalendarDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				officeId = this.officeId,
				isDefault = this.isDefault,
				iconId = this.iconId,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				versionNumber = this.versionNumber
			};
		}


		/// <summary>
		///
		/// Converts a Calendar list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<CalendarDTO> ToDTOList(List<Calendar> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CalendarDTO> output = new List<CalendarDTO>();

			output.Capacity = data.Count;

			foreach (Calendar calendar in data)
			{
				output.Add(calendar.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a Calendar to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the CalendarEntity type directly.
		///
		/// </summary>
		public CalendarOutputDTO ToOutputDTO()
		{
			return new CalendarOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				officeId = this.officeId,
				isDefault = this.isDefault,
				iconId = this.iconId,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				versionNumber = this.versionNumber,
				icon = this.icon?.ToDTO(),
				office = this.office?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a Calendar list to list of Output Data Transfer Object intended to be used for serializing a list of Calendar objects to avoid using the Calendar entity type directly.
		///
		/// </summary>
		public static List<CalendarOutputDTO> ToOutputDTOList(List<Calendar> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CalendarOutputDTO> output = new List<CalendarOutputDTO>();

			output.Capacity = data.Count;

			foreach (Calendar calendar in data)
			{
				output.Add(calendar.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a Calendar Object.
		///
		/// </summary>
		public static Database.Calendar FromDTO(CalendarDTO dto)
		{
			return new Database.Calendar
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				officeId = dto.officeId,
				isDefault = dto.isDefault,
				iconId = dto.iconId,
				color = dto.color,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false,
				versionNumber = dto.versionNumber
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a Calendar Object.
		///
		/// </summary>
		public void ApplyDTO(CalendarDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.officeId = dto.officeId;
			this.isDefault = dto.isDefault;
			this.iconId = dto.iconId;
			this.color = dto.color;
			this.objectGuid = dto.objectGuid;
			if (dto.active.HasValue == true)
			{
				this.active = dto.active.Value;
			}
			if (dto.deleted.HasValue == true)
			{
				this.deleted = dto.deleted.Value;
			}
			this.versionNumber = dto.versionNumber;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a Calendar Object.
		///
		/// </summary>
		public Calendar Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new Calendar{
				id = this.id,
				tenantGuid = this.tenantGuid,
				name = this.name,
				description = this.description,
				officeId = this.officeId,
				isDefault = this.isDefault,
				iconId = this.iconId,
				color = this.color,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				versionNumber = this.versionNumber,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Calendar Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Calendar Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a Calendar Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a Calendar Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.Calendar calendar)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (calendar == null)
			{
				return null;
			}

			return new {
				id = calendar.id,
				name = calendar.name,
				description = calendar.description,
				officeId = calendar.officeId,
				isDefault = calendar.isDefault,
				iconId = calendar.iconId,
				color = calendar.color,
				objectGuid = calendar.objectGuid,
				active = calendar.active,
				deleted = calendar.deleted,
				versionNumber = calendar.versionNumber,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a Calendar Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(Calendar calendar)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (calendar == null)
			{
				return null;
			}

			return new {
				id = calendar.id,
				name = calendar.name,
				description = calendar.description,
				officeId = calendar.officeId,
				isDefault = calendar.isDefault,
				iconId = calendar.iconId,
				color = calendar.color,
				objectGuid = calendar.objectGuid,
				active = calendar.active,
				deleted = calendar.deleted,
				versionNumber = calendar.versionNumber,
				icon = Icon.CreateMinimalAnonymous(calendar.icon),
				office = Office.CreateMinimalAnonymous(calendar.office)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a Calendar Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(Calendar calendar)
		{
			//
			// Return a very minimal object.
			//
			if (calendar == null)
			{
				return null;
			}

			return new {
				id = calendar.id,
				name = calendar.name,
				description = calendar.description,
			 };
		}
	}
}
