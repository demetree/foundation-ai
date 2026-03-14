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
	public partial class BuildManualStepChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)buildManualStepId; }
			set { buildManualStepId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BuildManualStepChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 buildManualStepId { get; set; }
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
		public class BuildManualStepChangeHistoryOutputDTO : BuildManualStepChangeHistoryDTO
		{
			public BuildManualStep.BuildManualStepDTO buildManualStep { get; set; }
		}


		/// <summary>
		///
		/// Converts a BuildManualStepChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BuildManualStepChangeHistoryDTO ToDTO()
		{
			return new BuildManualStepChangeHistoryDTO
			{
				id = this.id,
				buildManualStepId = this.buildManualStepId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a BuildManualStepChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BuildManualStepChangeHistoryDTO> ToDTOList(List<BuildManualStepChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildManualStepChangeHistoryDTO> output = new List<BuildManualStepChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (BuildManualStepChangeHistory buildManualStepChangeHistory in data)
			{
				output.Add(buildManualStepChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BuildManualStepChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BuildManualStepChangeHistoryEntity type directly.
		///
		/// </summary>
		public BuildManualStepChangeHistoryOutputDTO ToOutputDTO()
		{
			return new BuildManualStepChangeHistoryOutputDTO
			{
				id = this.id,
				buildManualStepId = this.buildManualStepId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				buildManualStep = this.buildManualStep?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BuildManualStepChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of BuildManualStepChangeHistory objects to avoid using the BuildManualStepChangeHistory entity type directly.
		///
		/// </summary>
		public static List<BuildManualStepChangeHistoryOutputDTO> ToOutputDTOList(List<BuildManualStepChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildManualStepChangeHistoryOutputDTO> output = new List<BuildManualStepChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (BuildManualStepChangeHistory buildManualStepChangeHistory in data)
			{
				output.Add(buildManualStepChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BuildManualStepChangeHistory Object.
		///
		/// </summary>
		public static Database.BuildManualStepChangeHistory FromDTO(BuildManualStepChangeHistoryDTO dto)
		{
			return new Database.BuildManualStepChangeHistory
			{
				id = dto.id,
				buildManualStepId = dto.buildManualStepId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BuildManualStepChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(BuildManualStepChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.buildManualStepId = dto.buildManualStepId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a BuildManualStepChangeHistory Object.
		///
		/// </summary>
		public BuildManualStepChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BuildManualStepChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				buildManualStepId = this.buildManualStepId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildManualStepChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildManualStepChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BuildManualStepChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BuildManualStepChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BuildManualStepChangeHistory buildManualStepChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (buildManualStepChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildManualStepChangeHistory.id,
				buildManualStepId = buildManualStepChangeHistory.buildManualStepId,
				versionNumber = buildManualStepChangeHistory.versionNumber,
				timeStamp = buildManualStepChangeHistory.timeStamp,
				userId = buildManualStepChangeHistory.userId,
				data = buildManualStepChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BuildManualStepChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BuildManualStepChangeHistory buildManualStepChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (buildManualStepChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildManualStepChangeHistory.id,
				buildManualStepId = buildManualStepChangeHistory.buildManualStepId,
				versionNumber = buildManualStepChangeHistory.versionNumber,
				timeStamp = buildManualStepChangeHistory.timeStamp,
				userId = buildManualStepChangeHistory.userId,
				data = buildManualStepChangeHistory.data,
				buildManualStep = BuildManualStep.CreateMinimalAnonymous(buildManualStepChangeHistory.buildManualStep),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BuildManualStepChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BuildManualStepChangeHistory buildManualStepChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (buildManualStepChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildManualStepChangeHistory.id,
				name = buildManualStepChangeHistory.id,
				description = buildManualStepChangeHistory.id
			 };
		}
	}
}
