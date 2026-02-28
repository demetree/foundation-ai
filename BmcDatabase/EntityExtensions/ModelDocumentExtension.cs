using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.BMC.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class ModelDocument : IVersionTrackedEntity<ModelDocument>, IAnonymousConvertible
	{
        /// <summary>
        /// This is for setting the context for change history inquiries.
        /// </summary>
        private BMCContext _contextForVersionInquiry = null;
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
        public static ChangeHistoryToolset<ModelDocument, ModelDocumentChangeHistory> GetChangeHistoryToolsetForWriting(BMCContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<ModelDocument, ModelDocumentChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static ChangeHistoryToolset<ModelDocument, ModelDocumentChangeHistory> GetChangeHistoryToolsetForReading(BMCContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<ModelDocument, ModelDocumentChangeHistory>(context, cancellationToken);
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
        public void SetupVersionInquiry(BMCContext context, Guid tenantGuid)
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
        public async Task<VersionInformation<ModelDocument>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<ModelDocument>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<ModelDocument>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this ModelDocument entity.");
            }

            VersionInformation<ModelDocument> version = new VersionInformation<ModelDocument>();

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
        public async Task<VersionInformation<ModelDocument>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this ModelDocument entity.");
            }

            VersionInformation<ModelDocument> version = new VersionInformation<ModelDocument>();

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
        public async Task<List<VersionInformation<ModelDocument>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<ModelDocument>> versions = new List<VersionInformation<ModelDocument>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<ModelDocument> version = new VersionInformation<ModelDocument>();

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
		public class ModelDocumentDTO
		{
			public Int32 id { get; set; }
			public Int32? projectId { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
			[Required]
			public String sourceFormat { get; set; }
			public String sourceFileName { get; set; }
			public String sourceFileFileName { get; set; }
			public Int64? sourceFileSize { get; set; }
			public Byte[] sourceFileData { get; set; }
			public String sourceFileMimeType { get; set; }
			public String author { get; set; }
			public Int32? totalPartCount { get; set; }
			public Int32? totalStepCount { get; set; }
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
		public class ModelDocumentOutputDTO : ModelDocumentDTO
		{
			public Project.ProjectDTO project { get; set; }
		}


		/// <summary>
		///
		/// Converts a ModelDocument to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ModelDocumentDTO ToDTO()
		{
			return new ModelDocumentDTO
			{
				id = this.id,
				projectId = this.projectId,
				name = this.name,
				description = this.description,
				sourceFormat = this.sourceFormat,
				sourceFileName = this.sourceFileName,
				sourceFileFileName = this.sourceFileFileName,
				sourceFileSize = this.sourceFileSize,
				sourceFileData = this.sourceFileData,
				sourceFileMimeType = this.sourceFileMimeType,
				author = this.author,
				totalPartCount = this.totalPartCount,
				totalStepCount = this.totalStepCount,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ModelDocument list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ModelDocumentDTO> ToDTOList(List<ModelDocument> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ModelDocumentDTO> output = new List<ModelDocumentDTO>();

			output.Capacity = data.Count;

			foreach (ModelDocument modelDocument in data)
			{
				output.Add(modelDocument.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ModelDocument to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ModelDocumentEntity type directly.
		///
		/// </summary>
		public ModelDocumentOutputDTO ToOutputDTO()
		{
			return new ModelDocumentOutputDTO
			{
				id = this.id,
				projectId = this.projectId,
				name = this.name,
				description = this.description,
				sourceFormat = this.sourceFormat,
				sourceFileName = this.sourceFileName,
				sourceFileFileName = this.sourceFileFileName,
				sourceFileSize = this.sourceFileSize,
				sourceFileData = this.sourceFileData,
				sourceFileMimeType = this.sourceFileMimeType,
				author = this.author,
				totalPartCount = this.totalPartCount,
				totalStepCount = this.totalStepCount,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				project = this.project?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ModelDocument list to list of Output Data Transfer Object intended to be used for serializing a list of ModelDocument objects to avoid using the ModelDocument entity type directly.
		///
		/// </summary>
		public static List<ModelDocumentOutputDTO> ToOutputDTOList(List<ModelDocument> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ModelDocumentOutputDTO> output = new List<ModelDocumentOutputDTO>();

			output.Capacity = data.Count;

			foreach (ModelDocument modelDocument in data)
			{
				output.Add(modelDocument.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ModelDocument Object.
		///
		/// </summary>
		public static Database.ModelDocument FromDTO(ModelDocumentDTO dto)
		{
			return new Database.ModelDocument
			{
				id = dto.id,
				projectId = dto.projectId,
				name = dto.name,
				description = dto.description,
				sourceFormat = dto.sourceFormat,
				sourceFileName = dto.sourceFileName,
				sourceFileFileName = dto.sourceFileFileName,
				sourceFileSize = dto.sourceFileSize,
				sourceFileData = dto.sourceFileData,
				sourceFileMimeType = dto.sourceFileMimeType,
				author = dto.author,
				totalPartCount = dto.totalPartCount,
				totalStepCount = dto.totalStepCount,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ModelDocument Object.
		///
		/// </summary>
		public void ApplyDTO(ModelDocumentDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.projectId = dto.projectId;
			this.name = dto.name;
			this.description = dto.description;
			this.sourceFormat = dto.sourceFormat;
			this.sourceFileName = dto.sourceFileName;
			this.sourceFileFileName = dto.sourceFileFileName;
			this.sourceFileSize = dto.sourceFileSize;
			this.sourceFileData = dto.sourceFileData;
			this.sourceFileMimeType = dto.sourceFileMimeType;
			this.author = dto.author;
			this.totalPartCount = dto.totalPartCount;
			this.totalStepCount = dto.totalStepCount;
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
		/// Creates a deep copy clone of a ModelDocument Object.
		///
		/// </summary>
		public ModelDocument Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ModelDocument{
				id = this.id,
				tenantGuid = this.tenantGuid,
				projectId = this.projectId,
				name = this.name,
				description = this.description,
				sourceFormat = this.sourceFormat,
				sourceFileName = this.sourceFileName,
				sourceFileFileName = this.sourceFileFileName,
				sourceFileSize = this.sourceFileSize,
				sourceFileData = this.sourceFileData,
				sourceFileMimeType = this.sourceFileMimeType,
				author = this.author,
				totalPartCount = this.totalPartCount,
				totalStepCount = this.totalStepCount,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ModelDocument Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ModelDocument Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ModelDocument Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ModelDocument Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ModelDocument modelDocument)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (modelDocument == null)
			{
				return null;
			}

			return new {
				id = modelDocument.id,
				projectId = modelDocument.projectId,
				name = modelDocument.name,
				description = modelDocument.description,
				sourceFormat = modelDocument.sourceFormat,
				sourceFileName = modelDocument.sourceFileName,
				sourceFileFileName = modelDocument.sourceFileFileName,
				sourceFileSize = modelDocument.sourceFileSize,
				sourceFileData = modelDocument.sourceFileData,
				sourceFileMimeType = modelDocument.sourceFileMimeType,
				author = modelDocument.author,
				totalPartCount = modelDocument.totalPartCount,
				totalStepCount = modelDocument.totalStepCount,
				versionNumber = modelDocument.versionNumber,
				objectGuid = modelDocument.objectGuid,
				active = modelDocument.active,
				deleted = modelDocument.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ModelDocument Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ModelDocument modelDocument)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (modelDocument == null)
			{
				return null;
			}

			return new {
				id = modelDocument.id,
				projectId = modelDocument.projectId,
				name = modelDocument.name,
				description = modelDocument.description,
				sourceFormat = modelDocument.sourceFormat,
				sourceFileName = modelDocument.sourceFileName,
				sourceFileFileName = modelDocument.sourceFileFileName,
				sourceFileSize = modelDocument.sourceFileSize,
				sourceFileData = modelDocument.sourceFileData,
				sourceFileMimeType = modelDocument.sourceFileMimeType,
				author = modelDocument.author,
				totalPartCount = modelDocument.totalPartCount,
				totalStepCount = modelDocument.totalStepCount,
				versionNumber = modelDocument.versionNumber,
				objectGuid = modelDocument.objectGuid,
				active = modelDocument.active,
				deleted = modelDocument.deleted,
				project = Project.CreateMinimalAnonymous(modelDocument.project)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ModelDocument Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ModelDocument modelDocument)
		{
			//
			// Return a very minimal object.
			//
			if (modelDocument == null)
			{
				return null;
			}

			return new {
				id = modelDocument.id,
				name = modelDocument.name,
				description = modelDocument.description,
			 };
		}
	}
}
