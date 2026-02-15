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
	public partial class BrickPart : IVersionTrackedEntity<BrickPart>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<BrickPart, BrickPartChangeHistory> GetChangeHistoryToolsetForWriting(BMCContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<BrickPart, BrickPartChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static ChangeHistoryToolset<BrickPart, BrickPartChangeHistory> GetChangeHistoryToolsetForReading(BMCContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<BrickPart, BrickPartChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<BrickPart>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<BrickPart>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<BrickPart>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this BrickPart entity.");
            }

            VersionInformation<BrickPart> version = new VersionInformation<BrickPart>();

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
        public async Task<VersionInformation<BrickPart>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this BrickPart entity.");
            }

            VersionInformation<BrickPart> version = new VersionInformation<BrickPart>();

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
        public async Task<List<VersionInformation<BrickPart>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<BrickPart>> versions = new List<VersionInformation<BrickPart>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<BrickPart> version = new VersionInformation<BrickPart>();

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
		public class BrickPartDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String ldrawPartId { get; set; }
			public String ldrawTitle { get; set; }
			public String ldrawCategory { get; set; }
			[Required]
			public Int32 partTypeId { get; set; }
			public String keywords { get; set; }
			public String author { get; set; }
			[Required]
			public Int32 brickCategoryId { get; set; }
			public Single? widthLdu { get; set; }
			public Single? heightLdu { get; set; }
			public Single? depthLdu { get; set; }
			public Single? massGrams { get; set; }
			public String geometryFilePath { get; set; }
			public Int32? toothCount { get; set; }
			public Single? gearRatio { get; set; }
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
		public class BrickPartOutputDTO : BrickPartDTO
		{
			public BrickCategory.BrickCategoryDTO brickCategory { get; set; }
			public PartType.PartTypeDTO partType { get; set; }
		}


		/// <summary>
		///
		/// Converts a BrickPart to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickPartDTO ToDTO()
		{
			return new BrickPartDTO
			{
				id = this.id,
				name = this.name,
				ldrawPartId = this.ldrawPartId,
				ldrawTitle = this.ldrawTitle,
				ldrawCategory = this.ldrawCategory,
				partTypeId = this.partTypeId,
				keywords = this.keywords,
				author = this.author,
				brickCategoryId = this.brickCategoryId,
				widthLdu = this.widthLdu,
				heightLdu = this.heightLdu,
				depthLdu = this.depthLdu,
				massGrams = this.massGrams,
				geometryFilePath = this.geometryFilePath,
				toothCount = this.toothCount,
				gearRatio = this.gearRatio,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickPart list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickPartDTO> ToDTOList(List<BrickPart> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickPartDTO> output = new List<BrickPartDTO>();

			output.Capacity = data.Count;

			foreach (BrickPart brickPart in data)
			{
				output.Add(brickPart.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickPart to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickPartEntity type directly.
		///
		/// </summary>
		public BrickPartOutputDTO ToOutputDTO()
		{
			return new BrickPartOutputDTO
			{
				id = this.id,
				name = this.name,
				ldrawPartId = this.ldrawPartId,
				ldrawTitle = this.ldrawTitle,
				ldrawCategory = this.ldrawCategory,
				partTypeId = this.partTypeId,
				keywords = this.keywords,
				author = this.author,
				brickCategoryId = this.brickCategoryId,
				widthLdu = this.widthLdu,
				heightLdu = this.heightLdu,
				depthLdu = this.depthLdu,
				massGrams = this.massGrams,
				geometryFilePath = this.geometryFilePath,
				toothCount = this.toothCount,
				gearRatio = this.gearRatio,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				brickCategory = this.brickCategory?.ToDTO(),
				partType = this.partType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BrickPart list to list of Output Data Transfer Object intended to be used for serializing a list of BrickPart objects to avoid using the BrickPart entity type directly.
		///
		/// </summary>
		public static List<BrickPartOutputDTO> ToOutputDTOList(List<BrickPart> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickPartOutputDTO> output = new List<BrickPartOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickPart brickPart in data)
			{
				output.Add(brickPart.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickPart Object.
		///
		/// </summary>
		public static Database.BrickPart FromDTO(BrickPartDTO dto)
		{
			return new Database.BrickPart
			{
				id = dto.id,
				name = dto.name,
				ldrawPartId = dto.ldrawPartId,
				ldrawTitle = dto.ldrawTitle,
				ldrawCategory = dto.ldrawCategory,
				partTypeId = dto.partTypeId,
				keywords = dto.keywords,
				author = dto.author,
				brickCategoryId = dto.brickCategoryId,
				widthLdu = dto.widthLdu,
				heightLdu = dto.heightLdu,
				depthLdu = dto.depthLdu,
				massGrams = dto.massGrams,
				geometryFilePath = dto.geometryFilePath,
				toothCount = dto.toothCount,
				gearRatio = dto.gearRatio,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BrickPart Object.
		///
		/// </summary>
		public void ApplyDTO(BrickPartDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.ldrawPartId = dto.ldrawPartId;
			this.ldrawTitle = dto.ldrawTitle;
			this.ldrawCategory = dto.ldrawCategory;
			this.partTypeId = dto.partTypeId;
			this.keywords = dto.keywords;
			this.author = dto.author;
			this.brickCategoryId = dto.brickCategoryId;
			this.widthLdu = dto.widthLdu;
			this.heightLdu = dto.heightLdu;
			this.depthLdu = dto.depthLdu;
			this.massGrams = dto.massGrams;
			this.geometryFilePath = dto.geometryFilePath;
			this.toothCount = dto.toothCount;
			this.gearRatio = dto.gearRatio;
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
		/// Creates a deep copy clone of a BrickPart Object.
		///
		/// </summary>
		public BrickPart Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickPart{
				id = this.id,
				name = this.name,
				ldrawPartId = this.ldrawPartId,
				ldrawTitle = this.ldrawTitle,
				ldrawCategory = this.ldrawCategory,
				partTypeId = this.partTypeId,
				keywords = this.keywords,
				author = this.author,
				brickCategoryId = this.brickCategoryId,
				widthLdu = this.widthLdu,
				heightLdu = this.heightLdu,
				depthLdu = this.depthLdu,
				massGrams = this.massGrams,
				geometryFilePath = this.geometryFilePath,
				toothCount = this.toothCount,
				gearRatio = this.gearRatio,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickPart Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickPart Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickPart Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickPart Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickPart brickPart)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickPart == null)
			{
				return null;
			}

			return new {
				id = brickPart.id,
				name = brickPart.name,
				ldrawPartId = brickPart.ldrawPartId,
				ldrawTitle = brickPart.ldrawTitle,
				ldrawCategory = brickPart.ldrawCategory,
				partTypeId = brickPart.partTypeId,
				keywords = brickPart.keywords,
				author = brickPart.author,
				brickCategoryId = brickPart.brickCategoryId,
				widthLdu = brickPart.widthLdu,
				heightLdu = brickPart.heightLdu,
				depthLdu = brickPart.depthLdu,
				massGrams = brickPart.massGrams,
				geometryFilePath = brickPart.geometryFilePath,
				toothCount = brickPart.toothCount,
				gearRatio = brickPart.gearRatio,
				versionNumber = brickPart.versionNumber,
				objectGuid = brickPart.objectGuid,
				active = brickPart.active,
				deleted = brickPart.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickPart Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickPart brickPart)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickPart == null)
			{
				return null;
			}

			return new {
				id = brickPart.id,
				name = brickPart.name,
				ldrawPartId = brickPart.ldrawPartId,
				ldrawTitle = brickPart.ldrawTitle,
				ldrawCategory = brickPart.ldrawCategory,
				partTypeId = brickPart.partTypeId,
				keywords = brickPart.keywords,
				author = brickPart.author,
				brickCategoryId = brickPart.brickCategoryId,
				widthLdu = brickPart.widthLdu,
				heightLdu = brickPart.heightLdu,
				depthLdu = brickPart.depthLdu,
				massGrams = brickPart.massGrams,
				geometryFilePath = brickPart.geometryFilePath,
				toothCount = brickPart.toothCount,
				gearRatio = brickPart.gearRatio,
				versionNumber = brickPart.versionNumber,
				objectGuid = brickPart.objectGuid,
				active = brickPart.active,
				deleted = brickPart.deleted,
				brickCategory = BrickCategory.CreateMinimalAnonymous(brickPart.brickCategory),
				partType = PartType.CreateMinimalAnonymous(brickPart.partType)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickPart Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickPart brickPart)
		{
			//
			// Return a very minimal object.
			//
			if (brickPart == null)
			{
				return null;
			}

			return new {
				id = brickPart.id,
				name = brickPart.name,
				description = string.Join(", ", new[] { brickPart.name, brickPart.ldrawPartId, brickPart.ldrawTitle}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
