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
	public partial class RateSheet : IVersionTrackedEntity<RateSheet>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<RateSheet, RateSheetChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<RateSheet, RateSheetChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static ChangeHistoryToolset<RateSheet, RateSheetChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<RateSheet, RateSheetChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<RateSheet>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<RateSheet>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<RateSheet>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this RateSheet entity.");
            }

            VersionInformation<RateSheet> version = new VersionInformation<RateSheet>();

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
        public async Task<VersionInformation<RateSheet>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this RateSheet entity.");
            }

            VersionInformation<RateSheet> version = new VersionInformation<RateSheet>();

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
        public async Task<List<VersionInformation<RateSheet>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<RateSheet>> versions = new List<VersionInformation<RateSheet>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<RateSheet> version = new VersionInformation<RateSheet>();

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
		public class RateSheetDTO
		{
			public Int32 id { get; set; }
			public Int32? officeId { get; set; }
			public Int32? assignmentRoleId { get; set; }
			public Int32? resourceId { get; set; }
			public Int32? schedulingTargetId { get; set; }
			[Required]
			public Int32 rateTypeId { get; set; }
			[Required]
			public DateTime effectiveDate { get; set; }
			[Required]
			public Int32 currencyId { get; set; }
			[Required]
			public Decimal costRate { get; set; }
			[Required]
			public Decimal billingRate { get; set; }
			public String notes { get; set; }
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
		public class RateSheetOutputDTO : RateSheetDTO
		{
			public AssignmentRole.AssignmentRoleDTO assignmentRole { get; set; }
			public Currency.CurrencyDTO currency { get; set; }
			public Office.OfficeDTO office { get; set; }
			public RateType.RateTypeDTO rateType { get; set; }
			public Resource.ResourceDTO resource { get; set; }
			public SchedulingTarget.SchedulingTargetDTO schedulingTarget { get; set; }
		}


		/// <summary>
		///
		/// Converts a RateSheet to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public RateSheetDTO ToDTO()
		{
			return new RateSheetDTO
			{
				id = this.id,
				officeId = this.officeId,
				assignmentRoleId = this.assignmentRoleId,
				resourceId = this.resourceId,
				schedulingTargetId = this.schedulingTargetId,
				rateTypeId = this.rateTypeId,
				effectiveDate = this.effectiveDate,
				currencyId = this.currencyId,
				costRate = this.costRate,
				billingRate = this.billingRate,
				notes = this.notes,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a RateSheet list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<RateSheetDTO> ToDTOList(List<RateSheet> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RateSheetDTO> output = new List<RateSheetDTO>();

			output.Capacity = data.Count;

			foreach (RateSheet rateSheet in data)
			{
				output.Add(rateSheet.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a RateSheet to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the RateSheetEntity type directly.
		///
		/// </summary>
		public RateSheetOutputDTO ToOutputDTO()
		{
			return new RateSheetOutputDTO
			{
				id = this.id,
				officeId = this.officeId,
				assignmentRoleId = this.assignmentRoleId,
				resourceId = this.resourceId,
				schedulingTargetId = this.schedulingTargetId,
				rateTypeId = this.rateTypeId,
				effectiveDate = this.effectiveDate,
				currencyId = this.currencyId,
				costRate = this.costRate,
				billingRate = this.billingRate,
				notes = this.notes,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				assignmentRole = this.assignmentRole?.ToDTO(),
				currency = this.currency?.ToDTO(),
				office = this.office?.ToDTO(),
				rateType = this.rateType?.ToDTO(),
				resource = this.resource?.ToDTO(),
				schedulingTarget = this.schedulingTarget?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a RateSheet list to list of Output Data Transfer Object intended to be used for serializing a list of RateSheet objects to avoid using the RateSheet entity type directly.
		///
		/// </summary>
		public static List<RateSheetOutputDTO> ToOutputDTOList(List<RateSheet> data)
		{
			if (data == null)
			{
				return null;
			}

			List<RateSheetOutputDTO> output = new List<RateSheetOutputDTO>();

			output.Capacity = data.Count;

			foreach (RateSheet rateSheet in data)
			{
				output.Add(rateSheet.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a RateSheet Object.
		///
		/// </summary>
		public static Database.RateSheet FromDTO(RateSheetDTO dto)
		{
			return new Database.RateSheet
			{
				id = dto.id,
				officeId = dto.officeId,
				assignmentRoleId = dto.assignmentRoleId,
				resourceId = dto.resourceId,
				schedulingTargetId = dto.schedulingTargetId,
				rateTypeId = dto.rateTypeId,
				effectiveDate = dto.effectiveDate,
				currencyId = dto.currencyId,
				costRate = dto.costRate,
				billingRate = dto.billingRate,
				notes = dto.notes,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a RateSheet Object.
		///
		/// </summary>
		public void ApplyDTO(RateSheetDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.officeId = dto.officeId;
			this.assignmentRoleId = dto.assignmentRoleId;
			this.resourceId = dto.resourceId;
			this.schedulingTargetId = dto.schedulingTargetId;
			this.rateTypeId = dto.rateTypeId;
			this.effectiveDate = dto.effectiveDate;
			this.currencyId = dto.currencyId;
			this.costRate = dto.costRate;
			this.billingRate = dto.billingRate;
			this.notes = dto.notes;
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
		/// Creates a deep copy clone of a RateSheet Object.
		///
		/// </summary>
		public RateSheet Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new RateSheet{
				id = this.id,
				tenantGuid = this.tenantGuid,
				officeId = this.officeId,
				assignmentRoleId = this.assignmentRoleId,
				resourceId = this.resourceId,
				schedulingTargetId = this.schedulingTargetId,
				rateTypeId = this.rateTypeId,
				effectiveDate = this.effectiveDate,
				currencyId = this.currencyId,
				costRate = this.costRate,
				billingRate = this.billingRate,
				notes = this.notes,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RateSheet Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a RateSheet Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a RateSheet Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a RateSheet Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.RateSheet rateSheet)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (rateSheet == null)
			{
				return null;
			}

			return new {
				id = rateSheet.id,
				officeId = rateSheet.officeId,
				assignmentRoleId = rateSheet.assignmentRoleId,
				resourceId = rateSheet.resourceId,
				schedulingTargetId = rateSheet.schedulingTargetId,
				rateTypeId = rateSheet.rateTypeId,
				effectiveDate = rateSheet.effectiveDate,
				currencyId = rateSheet.currencyId,
				costRate = rateSheet.costRate,
				billingRate = rateSheet.billingRate,
				notes = rateSheet.notes,
				versionNumber = rateSheet.versionNumber,
				objectGuid = rateSheet.objectGuid,
				active = rateSheet.active,
				deleted = rateSheet.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a RateSheet Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(RateSheet rateSheet)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (rateSheet == null)
			{
				return null;
			}

			return new {
				id = rateSheet.id,
				officeId = rateSheet.officeId,
				assignmentRoleId = rateSheet.assignmentRoleId,
				resourceId = rateSheet.resourceId,
				schedulingTargetId = rateSheet.schedulingTargetId,
				rateTypeId = rateSheet.rateTypeId,
				effectiveDate = rateSheet.effectiveDate,
				currencyId = rateSheet.currencyId,
				costRate = rateSheet.costRate,
				billingRate = rateSheet.billingRate,
				notes = rateSheet.notes,
				versionNumber = rateSheet.versionNumber,
				objectGuid = rateSheet.objectGuid,
				active = rateSheet.active,
				deleted = rateSheet.deleted,
				assignmentRole = AssignmentRole.CreateMinimalAnonymous(rateSheet.assignmentRole),
				currency = Currency.CreateMinimalAnonymous(rateSheet.currency),
				office = Office.CreateMinimalAnonymous(rateSheet.office),
				rateType = RateType.CreateMinimalAnonymous(rateSheet.rateType),
				resource = Resource.CreateMinimalAnonymous(rateSheet.resource),
				schedulingTarget = SchedulingTarget.CreateMinimalAnonymous(rateSheet.schedulingTarget)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a RateSheet Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(RateSheet rateSheet)
		{
			//
			// Return a very minimal object.
			//
			if (rateSheet == null)
			{
				return null;
			}

			return new {
				id = rateSheet.id,
				name = rateSheet.id,
				description = rateSheet.id
			 };
		}
	}
}
