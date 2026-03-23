using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.DeepSpace.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class StorageObjectChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)storageObjectId; }
			set { storageObjectId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class StorageObjectChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 storageObjectId { get; set; }
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
		public class StorageObjectChangeHistoryOutputDTO : StorageObjectChangeHistoryDTO
		{
			public StorageObject.StorageObjectDTO storageObject { get; set; }
		}


		/// <summary>
		///
		/// Converts a StorageObjectChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public StorageObjectChangeHistoryDTO ToDTO()
		{
			return new StorageObjectChangeHistoryDTO
			{
				id = this.id,
				storageObjectId = this.storageObjectId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a StorageObjectChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<StorageObjectChangeHistoryDTO> ToDTOList(List<StorageObjectChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<StorageObjectChangeHistoryDTO> output = new List<StorageObjectChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (StorageObjectChangeHistory storageObjectChangeHistory in data)
			{
				output.Add(storageObjectChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a StorageObjectChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the StorageObjectChangeHistory Entity type directly.
		///
		/// </summary>
		public StorageObjectChangeHistoryOutputDTO ToOutputDTO()
		{
			return new StorageObjectChangeHistoryOutputDTO
			{
				id = this.id,
				storageObjectId = this.storageObjectId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				storageObject = this.storageObject?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a StorageObjectChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of StorageObjectChangeHistory objects to avoid using the StorageObjectChangeHistory entity type directly.
		///
		/// </summary>
		public static List<StorageObjectChangeHistoryOutputDTO> ToOutputDTOList(List<StorageObjectChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<StorageObjectChangeHistoryOutputDTO> output = new List<StorageObjectChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (StorageObjectChangeHistory storageObjectChangeHistory in data)
			{
				output.Add(storageObjectChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a StorageObjectChangeHistory Object.
		///
		/// </summary>
		public static Database.StorageObjectChangeHistory FromDTO(StorageObjectChangeHistoryDTO dto)
		{
			return new Database.StorageObjectChangeHistory
			{
				id = dto.id,
				storageObjectId = dto.storageObjectId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a StorageObjectChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(StorageObjectChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.storageObjectId = dto.storageObjectId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a StorageObjectChangeHistory Object.
		///
		/// </summary>
		public StorageObjectChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new StorageObjectChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				storageObjectId = this.storageObjectId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a StorageObjectChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a StorageObjectChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a StorageObjectChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a StorageObjectChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.StorageObjectChangeHistory storageObjectChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (storageObjectChangeHistory == null)
			{
				return null;
			}

			return new {
				id = storageObjectChangeHistory.id,
				storageObjectId = storageObjectChangeHistory.storageObjectId,
				versionNumber = storageObjectChangeHistory.versionNumber,
				timeStamp = storageObjectChangeHistory.timeStamp,
				userId = storageObjectChangeHistory.userId,
				data = storageObjectChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a StorageObjectChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(StorageObjectChangeHistory storageObjectChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (storageObjectChangeHistory == null)
			{
				return null;
			}

			return new {
				id = storageObjectChangeHistory.id,
				storageObjectId = storageObjectChangeHistory.storageObjectId,
				versionNumber = storageObjectChangeHistory.versionNumber,
				timeStamp = storageObjectChangeHistory.timeStamp,
				userId = storageObjectChangeHistory.userId,
				data = storageObjectChangeHistory.data,
				storageObject = StorageObject.CreateMinimalAnonymous(storageObjectChangeHistory.storageObject)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a StorageObjectChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(StorageObjectChangeHistory storageObjectChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (storageObjectChangeHistory == null)
			{
				return null;
			}

			return new {
				id = storageObjectChangeHistory.id,
				name = storageObjectChangeHistory.id,
				description = storageObjectChangeHistory.id
			 };
		}
	}
}
