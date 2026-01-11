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
	public partial class Batch : IVersionTrackedEntity<Batch>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<Batch, BatchChangeHistory> GetChangeHistoryToolsetForWriting(SchedulerContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<Batch, BatchChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static ChangeHistoryToolset<Batch, BatchChangeHistory> GetChangeHistoryToolsetForReading(SchedulerContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<Batch, BatchChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<Batch>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<Batch>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<Batch>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this Batch entity.");
            }

            VersionInformation<Batch> version = new VersionInformation<Batch>();

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
        public async Task<VersionInformation<Batch>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this Batch entity.");
            }

            VersionInformation<Batch> version = new VersionInformation<Batch>();

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
        public async Task<List<VersionInformation<Batch>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<Batch>> versions = new List<VersionInformation<Batch>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<Batch> version = new VersionInformation<Batch>();

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
		public class BatchDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String batchNumber { get; set; }
			public String description { get; set; }
			[Required]
			public DateTime dateOpened { get; set; }
			public DateTime? datePosted { get; set; }
			[Required]
			public Int32 batchStatusId { get; set; }
			[Required]
			public Decimal controlAmount { get; set; }
			[Required]
			public Int32 controlCount { get; set; }
			public Int32? defaultFundId { get; set; }
			public Int32? defaultCampaignId { get; set; }
			public Int32? defaultAppealId { get; set; }
			public DateOnly? defaultDate { get; set; }
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
		public class BatchOutputDTO : BatchDTO
		{
			public BatchStatus.BatchStatusDTO batchStatus { get; set; }
			public Appeal.AppealDTO defaultAppeal { get; set; }
			public Campaign.CampaignDTO defaultCampaign { get; set; }
			public Fund.FundDTO defaultFund { get; set; }
		}


		/// <summary>
		///
		/// Converts a Batch to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BatchDTO ToDTO()
		{
			return new BatchDTO
			{
				id = this.id,
				batchNumber = this.batchNumber,
				description = this.description,
				dateOpened = this.dateOpened,
				datePosted = this.datePosted,
				batchStatusId = this.batchStatusId,
				controlAmount = this.controlAmount,
				controlCount = this.controlCount,
				defaultFundId = this.defaultFundId,
				defaultCampaignId = this.defaultCampaignId,
				defaultAppealId = this.defaultAppealId,
				defaultDate = this.defaultDate,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a Batch list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BatchDTO> ToDTOList(List<Batch> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BatchDTO> output = new List<BatchDTO>();

			output.Capacity = data.Count;

			foreach (Batch batch in data)
			{
				output.Add(batch.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a Batch to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BatchEntity type directly.
		///
		/// </summary>
		public BatchOutputDTO ToOutputDTO()
		{
			return new BatchOutputDTO
			{
				id = this.id,
				batchNumber = this.batchNumber,
				description = this.description,
				dateOpened = this.dateOpened,
				datePosted = this.datePosted,
				batchStatusId = this.batchStatusId,
				controlAmount = this.controlAmount,
				controlCount = this.controlCount,
				defaultFundId = this.defaultFundId,
				defaultCampaignId = this.defaultCampaignId,
				defaultAppealId = this.defaultAppealId,
				defaultDate = this.defaultDate,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				batchStatus = this.batchStatus?.ToDTO(),
				defaultAppeal = this.defaultAppeal?.ToDTO(),
				defaultCampaign = this.defaultCampaign?.ToDTO(),
				defaultFund = this.defaultFund?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a Batch list to list of Output Data Transfer Object intended to be used for serializing a list of Batch objects to avoid using the Batch entity type directly.
		///
		/// </summary>
		public static List<BatchOutputDTO> ToOutputDTOList(List<Batch> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BatchOutputDTO> output = new List<BatchOutputDTO>();

			output.Capacity = data.Count;

			foreach (Batch batch in data)
			{
				output.Add(batch.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a Batch Object.
		///
		/// </summary>
		public static Database.Batch FromDTO(BatchDTO dto)
		{
			return new Database.Batch
			{
				id = dto.id,
				batchNumber = dto.batchNumber,
				description = dto.description,
				dateOpened = dto.dateOpened,
				datePosted = dto.datePosted,
				batchStatusId = dto.batchStatusId,
				controlAmount = dto.controlAmount,
				controlCount = dto.controlCount,
				defaultFundId = dto.defaultFundId,
				defaultCampaignId = dto.defaultCampaignId,
				defaultAppealId = dto.defaultAppealId,
				defaultDate = dto.defaultDate,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a Batch Object.
		///
		/// </summary>
		public void ApplyDTO(BatchDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.batchNumber = dto.batchNumber;
			this.description = dto.description;
			this.dateOpened = dto.dateOpened;
			this.datePosted = dto.datePosted;
			this.batchStatusId = dto.batchStatusId;
			this.controlAmount = dto.controlAmount;
			this.controlCount = dto.controlCount;
			this.defaultFundId = dto.defaultFundId;
			this.defaultCampaignId = dto.defaultCampaignId;
			this.defaultAppealId = dto.defaultAppealId;
			this.defaultDate = dto.defaultDate;
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
		/// Creates a deep copy clone of a Batch Object.
		///
		/// </summary>
		public Batch Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new Batch{
				id = this.id,
				tenantGuid = this.tenantGuid,
				batchNumber = this.batchNumber,
				description = this.description,
				dateOpened = this.dateOpened,
				datePosted = this.datePosted,
				batchStatusId = this.batchStatusId,
				controlAmount = this.controlAmount,
				controlCount = this.controlCount,
				defaultFundId = this.defaultFundId,
				defaultCampaignId = this.defaultCampaignId,
				defaultAppealId = this.defaultAppealId,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Batch Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Batch Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a Batch Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a Batch Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.Batch batch)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (batch == null)
			{
				return null;
			}

			return new {
				id = batch.id,
				batchNumber = batch.batchNumber,
				description = batch.description,
				dateOpened = batch.dateOpened,
				datePosted = batch.datePosted,
				batchStatusId = batch.batchStatusId,
				controlAmount = batch.controlAmount,
				controlCount = batch.controlCount,
				defaultFundId = batch.defaultFundId,
				defaultCampaignId = batch.defaultCampaignId,
				defaultAppealId = batch.defaultAppealId,
				versionNumber = batch.versionNumber,
				objectGuid = batch.objectGuid,
				active = batch.active,
				deleted = batch.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a Batch Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(Batch batch)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (batch == null)
			{
				return null;
			}

			return new {
				id = batch.id,
				batchNumber = batch.batchNumber,
				description = batch.description,
				dateOpened = batch.dateOpened,
				datePosted = batch.datePosted,
				batchStatusId = batch.batchStatusId,
				controlAmount = batch.controlAmount,
				controlCount = batch.controlCount,
				defaultFundId = batch.defaultFundId,
				defaultCampaignId = batch.defaultCampaignId,
				defaultAppealId = batch.defaultAppealId,
				defaultDate = batch.defaultDate,
				versionNumber = batch.versionNumber,
				objectGuid = batch.objectGuid,
				active = batch.active,
				deleted = batch.deleted,
				batchStatus = BatchStatus.CreateMinimalAnonymous(batch.batchStatus),
				defaultAppeal = Appeal.CreateMinimalAnonymous(batch.defaultAppeal),
				defaultCampaign = Campaign.CreateMinimalAnonymous(batch.defaultCampaign),
				defaultFund = Fund.CreateMinimalAnonymous(batch.defaultFund)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a Batch Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(Batch batch)
		{
			//
			// Return a very minimal object.
			//
			if (batch == null)
			{
				return null;
			}

			return new {
				id = batch.id,
				description = batch.description,
				name = batch.batchNumber
			 };
		}
	}
}
