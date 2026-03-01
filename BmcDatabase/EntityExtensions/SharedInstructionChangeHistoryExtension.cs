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
	public partial class SharedInstructionChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)sharedInstructionId; }
			set { sharedInstructionId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class SharedInstructionChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 sharedInstructionId { get; set; }
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
		public class SharedInstructionChangeHistoryOutputDTO : SharedInstructionChangeHistoryDTO
		{
			public SharedInstruction.SharedInstructionDTO sharedInstruction { get; set; }
		}


		/// <summary>
		///
		/// Converts a SharedInstructionChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public SharedInstructionChangeHistoryDTO ToDTO()
		{
			return new SharedInstructionChangeHistoryDTO
			{
				id = this.id,
				sharedInstructionId = this.sharedInstructionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a SharedInstructionChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<SharedInstructionChangeHistoryDTO> ToDTOList(List<SharedInstructionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SharedInstructionChangeHistoryDTO> output = new List<SharedInstructionChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (SharedInstructionChangeHistory sharedInstructionChangeHistory in data)
			{
				output.Add(sharedInstructionChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a SharedInstructionChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the SharedInstructionChangeHistoryEntity type directly.
		///
		/// </summary>
		public SharedInstructionChangeHistoryOutputDTO ToOutputDTO()
		{
			return new SharedInstructionChangeHistoryOutputDTO
			{
				id = this.id,
				sharedInstructionId = this.sharedInstructionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				sharedInstruction = this.sharedInstruction?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a SharedInstructionChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of SharedInstructionChangeHistory objects to avoid using the SharedInstructionChangeHistory entity type directly.
		///
		/// </summary>
		public static List<SharedInstructionChangeHistoryOutputDTO> ToOutputDTOList(List<SharedInstructionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<SharedInstructionChangeHistoryOutputDTO> output = new List<SharedInstructionChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (SharedInstructionChangeHistory sharedInstructionChangeHistory in data)
			{
				output.Add(sharedInstructionChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a SharedInstructionChangeHistory Object.
		///
		/// </summary>
		public static Database.SharedInstructionChangeHistory FromDTO(SharedInstructionChangeHistoryDTO dto)
		{
			return new Database.SharedInstructionChangeHistory
			{
				id = dto.id,
				sharedInstructionId = dto.sharedInstructionId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a SharedInstructionChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(SharedInstructionChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.sharedInstructionId = dto.sharedInstructionId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a SharedInstructionChangeHistory Object.
		///
		/// </summary>
		public SharedInstructionChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new SharedInstructionChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				sharedInstructionId = this.sharedInstructionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SharedInstructionChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a SharedInstructionChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a SharedInstructionChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a SharedInstructionChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.SharedInstructionChangeHistory sharedInstructionChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (sharedInstructionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = sharedInstructionChangeHistory.id,
				sharedInstructionId = sharedInstructionChangeHistory.sharedInstructionId,
				versionNumber = sharedInstructionChangeHistory.versionNumber,
				timeStamp = sharedInstructionChangeHistory.timeStamp,
				userId = sharedInstructionChangeHistory.userId,
				data = sharedInstructionChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a SharedInstructionChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(SharedInstructionChangeHistory sharedInstructionChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (sharedInstructionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = sharedInstructionChangeHistory.id,
				sharedInstructionId = sharedInstructionChangeHistory.sharedInstructionId,
				versionNumber = sharedInstructionChangeHistory.versionNumber,
				timeStamp = sharedInstructionChangeHistory.timeStamp,
				userId = sharedInstructionChangeHistory.userId,
				data = sharedInstructionChangeHistory.data,
				sharedInstruction = SharedInstruction.CreateMinimalAnonymous(sharedInstructionChangeHistory.sharedInstruction),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a SharedInstructionChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(SharedInstructionChangeHistory sharedInstructionChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (sharedInstructionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = sharedInstructionChangeHistory.id,
				name = sharedInstructionChangeHistory.id,
				description = sharedInstructionChangeHistory.id
			 };
		}
	}
}
