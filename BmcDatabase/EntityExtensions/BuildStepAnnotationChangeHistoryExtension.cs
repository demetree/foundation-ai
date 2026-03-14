using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.BMC.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class BuildStepAnnotationChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)buildStepAnnotationId; }
			set { buildStepAnnotationId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BuildStepAnnotationChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 buildStepAnnotationId { get; set; }
			public Int32 versionNumber { get; set; }
			[Required]
			public DateTime timeStamp { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public String data { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class BuildStepAnnotationChangeHistoryOutputDTO : BuildStepAnnotationChangeHistoryDTO
		{
			public BuildStepAnnotation.BuildStepAnnotationDTO buildStepAnnotation { get; set; }
		}


		/// <summary>
		///
		/// Converts a BuildStepAnnotationChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BuildStepAnnotationChangeHistoryDTO ToDTO()
		{
			return new BuildStepAnnotationChangeHistoryDTO
			{
				id = this.id,
				buildStepAnnotationId = this.buildStepAnnotationId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a BuildStepAnnotationChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BuildStepAnnotationChangeHistoryDTO> ToDTOList(List<BuildStepAnnotationChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildStepAnnotationChangeHistoryDTO> output = new List<BuildStepAnnotationChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (BuildStepAnnotationChangeHistory buildStepAnnotationChangeHistory in data)
			{
				output.Add(buildStepAnnotationChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BuildStepAnnotationChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BuildStepAnnotationChangeHistoryEntity type directly.
		///
		/// </summary>
		public BuildStepAnnotationChangeHistoryOutputDTO ToOutputDTO()
		{
			return new BuildStepAnnotationChangeHistoryOutputDTO
			{
				id = this.id,
				buildStepAnnotationId = this.buildStepAnnotationId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				buildStepAnnotation = this.buildStepAnnotation?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BuildStepAnnotationChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of BuildStepAnnotationChangeHistory objects to avoid using the BuildStepAnnotationChangeHistory entity type directly.
		///
		/// </summary>
		public static List<BuildStepAnnotationChangeHistoryOutputDTO> ToOutputDTOList(List<BuildStepAnnotationChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildStepAnnotationChangeHistoryOutputDTO> output = new List<BuildStepAnnotationChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (BuildStepAnnotationChangeHistory buildStepAnnotationChangeHistory in data)
			{
				output.Add(buildStepAnnotationChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BuildStepAnnotationChangeHistory Object.
		///
		/// </summary>
		public static Database.BuildStepAnnotationChangeHistory FromDTO(BuildStepAnnotationChangeHistoryDTO dto)
		{
			return new Database.BuildStepAnnotationChangeHistory
			{
				id = dto.id,
				buildStepAnnotationId = dto.buildStepAnnotationId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BuildStepAnnotationChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(BuildStepAnnotationChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.buildStepAnnotationId = dto.buildStepAnnotationId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a BuildStepAnnotationChangeHistory Object.
		///
		/// </summary>
		public BuildStepAnnotationChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BuildStepAnnotationChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				buildStepAnnotationId = this.buildStepAnnotationId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildStepAnnotationChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildStepAnnotationChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BuildStepAnnotationChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BuildStepAnnotationChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BuildStepAnnotationChangeHistory buildStepAnnotationChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (buildStepAnnotationChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildStepAnnotationChangeHistory.id,
				buildStepAnnotationId = buildStepAnnotationChangeHistory.buildStepAnnotationId,
				versionNumber = buildStepAnnotationChangeHistory.versionNumber,
				timeStamp = buildStepAnnotationChangeHistory.timeStamp,
				userId = buildStepAnnotationChangeHistory.userId,
				data = buildStepAnnotationChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BuildStepAnnotationChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BuildStepAnnotationChangeHistory buildStepAnnotationChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (buildStepAnnotationChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildStepAnnotationChangeHistory.id,
				buildStepAnnotationId = buildStepAnnotationChangeHistory.buildStepAnnotationId,
				versionNumber = buildStepAnnotationChangeHistory.versionNumber,
				timeStamp = buildStepAnnotationChangeHistory.timeStamp,
				userId = buildStepAnnotationChangeHistory.userId,
				data = buildStepAnnotationChangeHistory.data,
				buildStepAnnotation = BuildStepAnnotation.CreateMinimalAnonymous(buildStepAnnotationChangeHistory.buildStepAnnotation),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BuildStepAnnotationChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BuildStepAnnotationChangeHistory buildStepAnnotationChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (buildStepAnnotationChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildStepAnnotationChangeHistory.id,
				name = buildStepAnnotationChangeHistory.id,
				description = buildStepAnnotationChangeHistory.id
			 };
		}
	}
}
