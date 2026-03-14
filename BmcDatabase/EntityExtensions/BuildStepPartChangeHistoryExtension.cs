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
	public partial class BuildStepPartChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)buildStepPartId; }
			set { buildStepPartId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BuildStepPartChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 buildStepPartId { get; set; }
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
		public class BuildStepPartChangeHistoryOutputDTO : BuildStepPartChangeHistoryDTO
		{
			public BuildStepPart.BuildStepPartDTO buildStepPart { get; set; }
		}


		/// <summary>
		///
		/// Converts a BuildStepPartChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BuildStepPartChangeHistoryDTO ToDTO()
		{
			return new BuildStepPartChangeHistoryDTO
			{
				id = this.id,
				buildStepPartId = this.buildStepPartId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a BuildStepPartChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BuildStepPartChangeHistoryDTO> ToDTOList(List<BuildStepPartChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildStepPartChangeHistoryDTO> output = new List<BuildStepPartChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (BuildStepPartChangeHistory buildStepPartChangeHistory in data)
			{
				output.Add(buildStepPartChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BuildStepPartChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BuildStepPartChangeHistoryEntity type directly.
		///
		/// </summary>
		public BuildStepPartChangeHistoryOutputDTO ToOutputDTO()
		{
			return new BuildStepPartChangeHistoryOutputDTO
			{
				id = this.id,
				buildStepPartId = this.buildStepPartId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				buildStepPart = this.buildStepPart?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BuildStepPartChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of BuildStepPartChangeHistory objects to avoid using the BuildStepPartChangeHistory entity type directly.
		///
		/// </summary>
		public static List<BuildStepPartChangeHistoryOutputDTO> ToOutputDTOList(List<BuildStepPartChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildStepPartChangeHistoryOutputDTO> output = new List<BuildStepPartChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (BuildStepPartChangeHistory buildStepPartChangeHistory in data)
			{
				output.Add(buildStepPartChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BuildStepPartChangeHistory Object.
		///
		/// </summary>
		public static Database.BuildStepPartChangeHistory FromDTO(BuildStepPartChangeHistoryDTO dto)
		{
			return new Database.BuildStepPartChangeHistory
			{
				id = dto.id,
				buildStepPartId = dto.buildStepPartId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BuildStepPartChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(BuildStepPartChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.buildStepPartId = dto.buildStepPartId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a BuildStepPartChangeHistory Object.
		///
		/// </summary>
		public BuildStepPartChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BuildStepPartChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				buildStepPartId = this.buildStepPartId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildStepPartChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildStepPartChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BuildStepPartChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BuildStepPartChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BuildStepPartChangeHistory buildStepPartChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (buildStepPartChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildStepPartChangeHistory.id,
				buildStepPartId = buildStepPartChangeHistory.buildStepPartId,
				versionNumber = buildStepPartChangeHistory.versionNumber,
				timeStamp = buildStepPartChangeHistory.timeStamp,
				userId = buildStepPartChangeHistory.userId,
				data = buildStepPartChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BuildStepPartChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BuildStepPartChangeHistory buildStepPartChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (buildStepPartChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildStepPartChangeHistory.id,
				buildStepPartId = buildStepPartChangeHistory.buildStepPartId,
				versionNumber = buildStepPartChangeHistory.versionNumber,
				timeStamp = buildStepPartChangeHistory.timeStamp,
				userId = buildStepPartChangeHistory.userId,
				data = buildStepPartChangeHistory.data,
				buildStepPart = BuildStepPart.CreateMinimalAnonymous(buildStepPartChangeHistory.buildStepPart),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BuildStepPartChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BuildStepPartChangeHistory buildStepPartChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (buildStepPartChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildStepPartChangeHistory.id,
				name = buildStepPartChangeHistory.id,
				description = buildStepPartChangeHistory.id
			 };
		}
	}
}
