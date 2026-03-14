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
	public partial class BuildManualStep : IVersionTrackedEntity<BuildManualStep>, IAnonymousConvertible
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
        public static ChangeHistoryToolset<BuildManualStep, BuildManualStepChangeHistory> GetChangeHistoryToolsetForWriting(BMCContext context, Foundation.Security.Database.SecurityUser securityUser, bool insideTransaction = false, CancellationToken cancellationToken = default)
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
            return new ChangeHistoryToolset<BuildManualStep, BuildManualStepChangeHistory>(context, securityUser.id, insideTransaction, cancellationToken);
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
        public static ChangeHistoryToolset<BuildManualStep, BuildManualStepChangeHistory> GetChangeHistoryToolsetForReading(BMCContext context, CancellationToken cancellationToken = default)
        {
            return new ChangeHistoryToolset<BuildManualStep, BuildManualStepChangeHistory>(context, cancellationToken);
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
        public async Task<VersionInformation<BuildManualStep>> GetThisVersionAsync(bool includeData = false, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<BuildManualStep>> GetFirstVersionAsync(bool includeData = true, CancellationToken cancellationToken = default)
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
        public async Task<VersionInformation<BuildManualStep>> GetVersionAtTimeAsync(DateTime pointInTime, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for point in time {pointInTime.ToString("s")} of this BuildManualStep entity.");
            }

            VersionInformation<BuildManualStep> version = new VersionInformation<BuildManualStep>();

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
        public async Task<VersionInformation<BuildManualStep>> GetVersionAsync(int versionNumber, bool includeData = true, CancellationToken cancellationToken = default)
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
                throw new Exception($"No change history found for version {versionNumber} of this BuildManualStep entity.");
            }

            VersionInformation<BuildManualStep> version = new VersionInformation<BuildManualStep>();

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
        public async Task<List<VersionInformation<BuildManualStep>>> GetAllVersionsAsync(bool includeData = true, CancellationToken cancellationToken = default)
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

            List <VersionInformation<BuildManualStep>> versions = new List<VersionInformation<BuildManualStep>>();

            foreach (AuditEntry versionAudit in versionAudits)
            {
                VersionInformation<BuildManualStep> version = new VersionInformation<BuildManualStep>();

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
		public class BuildManualStepDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 buildManualPageId { get; set; }
			public Int32? stepNumber { get; set; }
			public Single? cameraPositionX { get; set; }
			public Single? cameraPositionY { get; set; }
			public Single? cameraPositionZ { get; set; }
			public Single? cameraTargetX { get; set; }
			public Single? cameraTargetY { get; set; }
			public Single? cameraTargetZ { get; set; }
			public Single? cameraZoom { get; set; }
			[Required]
			public Boolean showExplodedView { get; set; }
			public Single? explodedDistance { get; set; }
			public String renderImagePath { get; set; }
			public String pliImagePath { get; set; }
			[Required]
			public Boolean fadeStepEnabled { get; set; }
			[Required]
			public Boolean isCallout { get; set; }
			public String calloutModelName { get; set; }
			[Required]
			public Boolean showPartsListImage { get; set; }
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
		public class BuildManualStepOutputDTO : BuildManualStepDTO
		{
			public BuildManualPage.BuildManualPageDTO buildManualPage { get; set; }
		}


		/// <summary>
		///
		/// Converts a BuildManualStep to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BuildManualStepDTO ToDTO()
		{
			return new BuildManualStepDTO
			{
				id = this.id,
				buildManualPageId = this.buildManualPageId,
				stepNumber = this.stepNumber,
				cameraPositionX = this.cameraPositionX,
				cameraPositionY = this.cameraPositionY,
				cameraPositionZ = this.cameraPositionZ,
				cameraTargetX = this.cameraTargetX,
				cameraTargetY = this.cameraTargetY,
				cameraTargetZ = this.cameraTargetZ,
				cameraZoom = this.cameraZoom,
				showExplodedView = this.showExplodedView,
				explodedDistance = this.explodedDistance,
				renderImagePath = this.renderImagePath,
				pliImagePath = this.pliImagePath,
				fadeStepEnabled = this.fadeStepEnabled,
				isCallout = this.isCallout,
				calloutModelName = this.calloutModelName,
				showPartsListImage = this.showPartsListImage,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BuildManualStep list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BuildManualStepDTO> ToDTOList(List<BuildManualStep> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildManualStepDTO> output = new List<BuildManualStepDTO>();

			output.Capacity = data.Count;

			foreach (BuildManualStep buildManualStep in data)
			{
				output.Add(buildManualStep.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BuildManualStep to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BuildManualStepEntity type directly.
		///
		/// </summary>
		public BuildManualStepOutputDTO ToOutputDTO()
		{
			return new BuildManualStepOutputDTO
			{
				id = this.id,
				buildManualPageId = this.buildManualPageId,
				stepNumber = this.stepNumber,
				cameraPositionX = this.cameraPositionX,
				cameraPositionY = this.cameraPositionY,
				cameraPositionZ = this.cameraPositionZ,
				cameraTargetX = this.cameraTargetX,
				cameraTargetY = this.cameraTargetY,
				cameraTargetZ = this.cameraTargetZ,
				cameraZoom = this.cameraZoom,
				showExplodedView = this.showExplodedView,
				explodedDistance = this.explodedDistance,
				renderImagePath = this.renderImagePath,
				pliImagePath = this.pliImagePath,
				fadeStepEnabled = this.fadeStepEnabled,
				isCallout = this.isCallout,
				calloutModelName = this.calloutModelName,
				showPartsListImage = this.showPartsListImage,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				buildManualPage = this.buildManualPage?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BuildManualStep list to list of Output Data Transfer Object intended to be used for serializing a list of BuildManualStep objects to avoid using the BuildManualStep entity type directly.
		///
		/// </summary>
		public static List<BuildManualStepOutputDTO> ToOutputDTOList(List<BuildManualStep> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildManualStepOutputDTO> output = new List<BuildManualStepOutputDTO>();

			output.Capacity = data.Count;

			foreach (BuildManualStep buildManualStep in data)
			{
				output.Add(buildManualStep.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BuildManualStep Object.
		///
		/// </summary>
		public static Database.BuildManualStep FromDTO(BuildManualStepDTO dto)
		{
			return new Database.BuildManualStep
			{
				id = dto.id,
				buildManualPageId = dto.buildManualPageId,
				stepNumber = dto.stepNumber,
				cameraPositionX = dto.cameraPositionX,
				cameraPositionY = dto.cameraPositionY,
				cameraPositionZ = dto.cameraPositionZ,
				cameraTargetX = dto.cameraTargetX,
				cameraTargetY = dto.cameraTargetY,
				cameraTargetZ = dto.cameraTargetZ,
				cameraZoom = dto.cameraZoom,
				showExplodedView = dto.showExplodedView,
				explodedDistance = dto.explodedDistance,
				renderImagePath = dto.renderImagePath,
				pliImagePath = dto.pliImagePath,
				fadeStepEnabled = dto.fadeStepEnabled,
				isCallout = dto.isCallout,
				calloutModelName = dto.calloutModelName,
				showPartsListImage = dto.showPartsListImage,
				versionNumber = dto.versionNumber,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BuildManualStep Object.
		///
		/// </summary>
		public void ApplyDTO(BuildManualStepDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.buildManualPageId = dto.buildManualPageId;
			this.stepNumber = dto.stepNumber;
			this.cameraPositionX = dto.cameraPositionX;
			this.cameraPositionY = dto.cameraPositionY;
			this.cameraPositionZ = dto.cameraPositionZ;
			this.cameraTargetX = dto.cameraTargetX;
			this.cameraTargetY = dto.cameraTargetY;
			this.cameraTargetZ = dto.cameraTargetZ;
			this.cameraZoom = dto.cameraZoom;
			this.showExplodedView = dto.showExplodedView;
			this.explodedDistance = dto.explodedDistance;
			this.renderImagePath = dto.renderImagePath;
			this.pliImagePath = dto.pliImagePath;
			this.fadeStepEnabled = dto.fadeStepEnabled;
			this.isCallout = dto.isCallout;
			this.calloutModelName = dto.calloutModelName;
			this.showPartsListImage = dto.showPartsListImage;
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
		/// Creates a deep copy clone of a BuildManualStep Object.
		///
		/// </summary>
		public BuildManualStep Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BuildManualStep{
				id = this.id,
				tenantGuid = this.tenantGuid,
				buildManualPageId = this.buildManualPageId,
				stepNumber = this.stepNumber,
				cameraPositionX = this.cameraPositionX,
				cameraPositionY = this.cameraPositionY,
				cameraPositionZ = this.cameraPositionZ,
				cameraTargetX = this.cameraTargetX,
				cameraTargetY = this.cameraTargetY,
				cameraTargetZ = this.cameraTargetZ,
				cameraZoom = this.cameraZoom,
				showExplodedView = this.showExplodedView,
				explodedDistance = this.explodedDistance,
				renderImagePath = this.renderImagePath,
				pliImagePath = this.pliImagePath,
				fadeStepEnabled = this.fadeStepEnabled,
				isCallout = this.isCallout,
				calloutModelName = this.calloutModelName,
				showPartsListImage = this.showPartsListImage,
				versionNumber = this.versionNumber,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildManualStep Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildManualStep Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BuildManualStep Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BuildManualStep Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BuildManualStep buildManualStep)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (buildManualStep == null)
			{
				return null;
			}

			return new {
				id = buildManualStep.id,
				buildManualPageId = buildManualStep.buildManualPageId,
				stepNumber = buildManualStep.stepNumber,
				cameraPositionX = buildManualStep.cameraPositionX,
				cameraPositionY = buildManualStep.cameraPositionY,
				cameraPositionZ = buildManualStep.cameraPositionZ,
				cameraTargetX = buildManualStep.cameraTargetX,
				cameraTargetY = buildManualStep.cameraTargetY,
				cameraTargetZ = buildManualStep.cameraTargetZ,
				cameraZoom = buildManualStep.cameraZoom,
				showExplodedView = buildManualStep.showExplodedView,
				explodedDistance = buildManualStep.explodedDistance,
				renderImagePath = buildManualStep.renderImagePath,
				pliImagePath = buildManualStep.pliImagePath,
				fadeStepEnabled = buildManualStep.fadeStepEnabled,
				isCallout = buildManualStep.isCallout,
				calloutModelName = buildManualStep.calloutModelName,
				showPartsListImage = buildManualStep.showPartsListImage,
				versionNumber = buildManualStep.versionNumber,
				objectGuid = buildManualStep.objectGuid,
				active = buildManualStep.active,
				deleted = buildManualStep.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BuildManualStep Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BuildManualStep buildManualStep)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (buildManualStep == null)
			{
				return null;
			}

			return new {
				id = buildManualStep.id,
				buildManualPageId = buildManualStep.buildManualPageId,
				stepNumber = buildManualStep.stepNumber,
				cameraPositionX = buildManualStep.cameraPositionX,
				cameraPositionY = buildManualStep.cameraPositionY,
				cameraPositionZ = buildManualStep.cameraPositionZ,
				cameraTargetX = buildManualStep.cameraTargetX,
				cameraTargetY = buildManualStep.cameraTargetY,
				cameraTargetZ = buildManualStep.cameraTargetZ,
				cameraZoom = buildManualStep.cameraZoom,
				showExplodedView = buildManualStep.showExplodedView,
				explodedDistance = buildManualStep.explodedDistance,
				renderImagePath = buildManualStep.renderImagePath,
				pliImagePath = buildManualStep.pliImagePath,
				fadeStepEnabled = buildManualStep.fadeStepEnabled,
				isCallout = buildManualStep.isCallout,
				calloutModelName = buildManualStep.calloutModelName,
				showPartsListImage = buildManualStep.showPartsListImage,
				versionNumber = buildManualStep.versionNumber,
				objectGuid = buildManualStep.objectGuid,
				active = buildManualStep.active,
				deleted = buildManualStep.deleted,
				buildManualPage = BuildManualPage.CreateMinimalAnonymous(buildManualStep.buildManualPage)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BuildManualStep Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BuildManualStep buildManualStep)
		{
			//
			// Return a very minimal object.
			//
			if (buildManualStep == null)
			{
				return null;
			}

			return new {
				id = buildManualStep.id,
				name = buildManualStep.calloutModelName,
				description = string.Join(", ", new[] { buildManualStep.calloutModelName}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
