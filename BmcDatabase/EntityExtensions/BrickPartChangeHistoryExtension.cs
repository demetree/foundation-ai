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
	public partial class BrickPartChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)brickPartId; }
			set { brickPartId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickPartChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 brickPartId { get; set; }
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
		public class BrickPartChangeHistoryOutputDTO : BrickPartChangeHistoryDTO
		{
			public BrickPart.BrickPartDTO brickPart { get; set; }
		}


		/// <summary>
		///
		/// Converts a BrickPartChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickPartChangeHistoryDTO ToDTO()
		{
			return new BrickPartChangeHistoryDTO
			{
				id = this.id,
				brickPartId = this.brickPartId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a BrickPartChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickPartChangeHistoryDTO> ToDTOList(List<BrickPartChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickPartChangeHistoryDTO> output = new List<BrickPartChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (BrickPartChangeHistory brickPartChangeHistory in data)
			{
				output.Add(brickPartChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickPartChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickPartChangeHistoryEntity type directly.
		///
		/// </summary>
		public BrickPartChangeHistoryOutputDTO ToOutputDTO()
		{
			return new BrickPartChangeHistoryOutputDTO
			{
				id = this.id,
				brickPartId = this.brickPartId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				brickPart = this.brickPart?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BrickPartChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of BrickPartChangeHistory objects to avoid using the BrickPartChangeHistory entity type directly.
		///
		/// </summary>
		public static List<BrickPartChangeHistoryOutputDTO> ToOutputDTOList(List<BrickPartChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickPartChangeHistoryOutputDTO> output = new List<BrickPartChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickPartChangeHistory brickPartChangeHistory in data)
			{
				output.Add(brickPartChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickPartChangeHistory Object.
		///
		/// </summary>
		public static Database.BrickPartChangeHistory FromDTO(BrickPartChangeHistoryDTO dto)
		{
			return new Database.BrickPartChangeHistory
			{
				id = dto.id,
				brickPartId = dto.brickPartId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BrickPartChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(BrickPartChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.brickPartId = dto.brickPartId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a BrickPartChangeHistory Object.
		///
		/// </summary>
		public BrickPartChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickPartChangeHistory{
				id = this.id,
				brickPartId = this.brickPartId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickPartChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickPartChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickPartChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickPartChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickPartChangeHistory brickPartChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickPartChangeHistory == null)
			{
				return null;
			}

			return new {
				id = brickPartChangeHistory.id,
				brickPartId = brickPartChangeHistory.brickPartId,
				versionNumber = brickPartChangeHistory.versionNumber,
				timeStamp = brickPartChangeHistory.timeStamp,
				userId = brickPartChangeHistory.userId,
				data = brickPartChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickPartChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickPartChangeHistory brickPartChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickPartChangeHistory == null)
			{
				return null;
			}

			return new {
				id = brickPartChangeHistory.id,
				brickPartId = brickPartChangeHistory.brickPartId,
				versionNumber = brickPartChangeHistory.versionNumber,
				timeStamp = brickPartChangeHistory.timeStamp,
				userId = brickPartChangeHistory.userId,
				data = brickPartChangeHistory.data,
				brickPart = BrickPart.CreateMinimalAnonymous(brickPartChangeHistory.brickPart),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickPartChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickPartChangeHistory brickPartChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (brickPartChangeHistory == null)
			{
				return null;
			}

			return new {
				id = brickPartChangeHistory.id,
				name = brickPartChangeHistory.id,
				description = brickPartChangeHistory.id
			 };
		}
	}
}
