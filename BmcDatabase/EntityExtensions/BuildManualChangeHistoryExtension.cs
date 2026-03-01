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
	public partial class BuildManualChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)buildManualId; }
			set { buildManualId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BuildManualChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 buildManualId { get; set; }
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
		public class BuildManualChangeHistoryOutputDTO : BuildManualChangeHistoryDTO
		{
			public BuildManual.BuildManualDTO buildManual { get; set; }
		}


		/// <summary>
		///
		/// Converts a BuildManualChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BuildManualChangeHistoryDTO ToDTO()
		{
			return new BuildManualChangeHistoryDTO
			{
				id = this.id,
				buildManualId = this.buildManualId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a BuildManualChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BuildManualChangeHistoryDTO> ToDTOList(List<BuildManualChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildManualChangeHistoryDTO> output = new List<BuildManualChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (BuildManualChangeHistory buildManualChangeHistory in data)
			{
				output.Add(buildManualChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BuildManualChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BuildManualChangeHistoryEntity type directly.
		///
		/// </summary>
		public BuildManualChangeHistoryOutputDTO ToOutputDTO()
		{
			return new BuildManualChangeHistoryOutputDTO
			{
				id = this.id,
				buildManualId = this.buildManualId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				buildManual = this.buildManual?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BuildManualChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of BuildManualChangeHistory objects to avoid using the BuildManualChangeHistory entity type directly.
		///
		/// </summary>
		public static List<BuildManualChangeHistoryOutputDTO> ToOutputDTOList(List<BuildManualChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildManualChangeHistoryOutputDTO> output = new List<BuildManualChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (BuildManualChangeHistory buildManualChangeHistory in data)
			{
				output.Add(buildManualChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BuildManualChangeHistory Object.
		///
		/// </summary>
		public static Database.BuildManualChangeHistory FromDTO(BuildManualChangeHistoryDTO dto)
		{
			return new Database.BuildManualChangeHistory
			{
				id = dto.id,
				buildManualId = dto.buildManualId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BuildManualChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(BuildManualChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.buildManualId = dto.buildManualId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a BuildManualChangeHistory Object.
		///
		/// </summary>
		public BuildManualChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BuildManualChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				buildManualId = this.buildManualId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildManualChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildManualChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BuildManualChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BuildManualChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BuildManualChangeHistory buildManualChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (buildManualChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildManualChangeHistory.id,
				buildManualId = buildManualChangeHistory.buildManualId,
				versionNumber = buildManualChangeHistory.versionNumber,
				timeStamp = buildManualChangeHistory.timeStamp,
				userId = buildManualChangeHistory.userId,
				data = buildManualChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BuildManualChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BuildManualChangeHistory buildManualChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (buildManualChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildManualChangeHistory.id,
				buildManualId = buildManualChangeHistory.buildManualId,
				versionNumber = buildManualChangeHistory.versionNumber,
				timeStamp = buildManualChangeHistory.timeStamp,
				userId = buildManualChangeHistory.userId,
				data = buildManualChangeHistory.data,
				buildManual = BuildManual.CreateMinimalAnonymous(buildManualChangeHistory.buildManual),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BuildManualChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BuildManualChangeHistory buildManualChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (buildManualChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildManualChangeHistory.id,
				name = buildManualChangeHistory.id,
				description = buildManualChangeHistory.id
			 };
		}
	}
}
