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
	public partial class BuildManualPageChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)buildManualPageId; }
			set { buildManualPageId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BuildManualPageChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 buildManualPageId { get; set; }
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
		public class BuildManualPageChangeHistoryOutputDTO : BuildManualPageChangeHistoryDTO
		{
			public BuildManualPage.BuildManualPageDTO buildManualPage { get; set; }
		}


		/// <summary>
		///
		/// Converts a BuildManualPageChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BuildManualPageChangeHistoryDTO ToDTO()
		{
			return new BuildManualPageChangeHistoryDTO
			{
				id = this.id,
				buildManualPageId = this.buildManualPageId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a BuildManualPageChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BuildManualPageChangeHistoryDTO> ToDTOList(List<BuildManualPageChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildManualPageChangeHistoryDTO> output = new List<BuildManualPageChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (BuildManualPageChangeHistory buildManualPageChangeHistory in data)
			{
				output.Add(buildManualPageChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BuildManualPageChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BuildManualPageChangeHistoryEntity type directly.
		///
		/// </summary>
		public BuildManualPageChangeHistoryOutputDTO ToOutputDTO()
		{
			return new BuildManualPageChangeHistoryOutputDTO
			{
				id = this.id,
				buildManualPageId = this.buildManualPageId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				buildManualPage = this.buildManualPage?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BuildManualPageChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of BuildManualPageChangeHistory objects to avoid using the BuildManualPageChangeHistory entity type directly.
		///
		/// </summary>
		public static List<BuildManualPageChangeHistoryOutputDTO> ToOutputDTOList(List<BuildManualPageChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildManualPageChangeHistoryOutputDTO> output = new List<BuildManualPageChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (BuildManualPageChangeHistory buildManualPageChangeHistory in data)
			{
				output.Add(buildManualPageChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BuildManualPageChangeHistory Object.
		///
		/// </summary>
		public static Database.BuildManualPageChangeHistory FromDTO(BuildManualPageChangeHistoryDTO dto)
		{
			return new Database.BuildManualPageChangeHistory
			{
				id = dto.id,
				buildManualPageId = dto.buildManualPageId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BuildManualPageChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(BuildManualPageChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.buildManualPageId = dto.buildManualPageId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a BuildManualPageChangeHistory Object.
		///
		/// </summary>
		public BuildManualPageChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BuildManualPageChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				buildManualPageId = this.buildManualPageId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildManualPageChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildManualPageChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BuildManualPageChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BuildManualPageChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BuildManualPageChangeHistory buildManualPageChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (buildManualPageChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildManualPageChangeHistory.id,
				buildManualPageId = buildManualPageChangeHistory.buildManualPageId,
				versionNumber = buildManualPageChangeHistory.versionNumber,
				timeStamp = buildManualPageChangeHistory.timeStamp,
				userId = buildManualPageChangeHistory.userId,
				data = buildManualPageChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BuildManualPageChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BuildManualPageChangeHistory buildManualPageChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (buildManualPageChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildManualPageChangeHistory.id,
				buildManualPageId = buildManualPageChangeHistory.buildManualPageId,
				versionNumber = buildManualPageChangeHistory.versionNumber,
				timeStamp = buildManualPageChangeHistory.timeStamp,
				userId = buildManualPageChangeHistory.userId,
				data = buildManualPageChangeHistory.data,
				buildManualPage = BuildManualPage.CreateMinimalAnonymous(buildManualPageChangeHistory.buildManualPage),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BuildManualPageChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BuildManualPageChangeHistory buildManualPageChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (buildManualPageChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildManualPageChangeHistory.id,
				name = buildManualPageChangeHistory.id,
				description = buildManualPageChangeHistory.id
			 };
		}
	}
}
