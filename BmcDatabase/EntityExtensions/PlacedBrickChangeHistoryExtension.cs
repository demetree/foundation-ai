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
	public partial class PlacedBrickChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)placedBrickId; }
			set { placedBrickId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PlacedBrickChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 placedBrickId { get; set; }
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
		public class PlacedBrickChangeHistoryOutputDTO : PlacedBrickChangeHistoryDTO
		{
			public PlacedBrick.PlacedBrickDTO placedBrick { get; set; }
		}


		/// <summary>
		///
		/// Converts a PlacedBrickChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PlacedBrickChangeHistoryDTO ToDTO()
		{
			return new PlacedBrickChangeHistoryDTO
			{
				id = this.id,
				placedBrickId = this.placedBrickId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a PlacedBrickChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PlacedBrickChangeHistoryDTO> ToDTOList(List<PlacedBrickChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PlacedBrickChangeHistoryDTO> output = new List<PlacedBrickChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (PlacedBrickChangeHistory placedBrickChangeHistory in data)
			{
				output.Add(placedBrickChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PlacedBrickChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PlacedBrickChangeHistoryEntity type directly.
		///
		/// </summary>
		public PlacedBrickChangeHistoryOutputDTO ToOutputDTO()
		{
			return new PlacedBrickChangeHistoryOutputDTO
			{
				id = this.id,
				placedBrickId = this.placedBrickId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				placedBrick = this.placedBrick?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a PlacedBrickChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of PlacedBrickChangeHistory objects to avoid using the PlacedBrickChangeHistory entity type directly.
		///
		/// </summary>
		public static List<PlacedBrickChangeHistoryOutputDTO> ToOutputDTOList(List<PlacedBrickChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PlacedBrickChangeHistoryOutputDTO> output = new List<PlacedBrickChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (PlacedBrickChangeHistory placedBrickChangeHistory in data)
			{
				output.Add(placedBrickChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PlacedBrickChangeHistory Object.
		///
		/// </summary>
		public static Database.PlacedBrickChangeHistory FromDTO(PlacedBrickChangeHistoryDTO dto)
		{
			return new Database.PlacedBrickChangeHistory
			{
				id = dto.id,
				placedBrickId = dto.placedBrickId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PlacedBrickChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(PlacedBrickChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.placedBrickId = dto.placedBrickId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a PlacedBrickChangeHistory Object.
		///
		/// </summary>
		public PlacedBrickChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PlacedBrickChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				placedBrickId = this.placedBrickId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PlacedBrickChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PlacedBrickChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PlacedBrickChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PlacedBrickChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PlacedBrickChangeHistory placedBrickChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (placedBrickChangeHistory == null)
			{
				return null;
			}

			return new {
				id = placedBrickChangeHistory.id,
				placedBrickId = placedBrickChangeHistory.placedBrickId,
				versionNumber = placedBrickChangeHistory.versionNumber,
				timeStamp = placedBrickChangeHistory.timeStamp,
				userId = placedBrickChangeHistory.userId,
				data = placedBrickChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PlacedBrickChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PlacedBrickChangeHistory placedBrickChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (placedBrickChangeHistory == null)
			{
				return null;
			}

			return new {
				id = placedBrickChangeHistory.id,
				placedBrickId = placedBrickChangeHistory.placedBrickId,
				versionNumber = placedBrickChangeHistory.versionNumber,
				timeStamp = placedBrickChangeHistory.timeStamp,
				userId = placedBrickChangeHistory.userId,
				data = placedBrickChangeHistory.data,
				placedBrick = PlacedBrick.CreateMinimalAnonymous(placedBrickChangeHistory.placedBrick)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PlacedBrickChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PlacedBrickChangeHistory placedBrickChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (placedBrickChangeHistory == null)
			{
				return null;
			}

			return new {
				id = placedBrickChangeHistory.id,
				name = placedBrickChangeHistory.id,
				description = placedBrickChangeHistory.id
			 };
		}
	}
}
