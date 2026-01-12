using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class ResourceShiftChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)resourceShiftId; }
			set { resourceShiftId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ResourceShiftChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 resourceShiftId { get; set; }
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
		public class ResourceShiftChangeHistoryOutputDTO : ResourceShiftChangeHistoryDTO
		{
			public ResourceShift.ResourceShiftDTO resourceShift { get; set; }
		}


		/// <summary>
		///
		/// Converts a ResourceShiftChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ResourceShiftChangeHistoryDTO ToDTO()
		{
			return new ResourceShiftChangeHistoryDTO
			{
				id = this.id,
				resourceShiftId = this.resourceShiftId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ResourceShiftChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ResourceShiftChangeHistoryDTO> ToDTOList(List<ResourceShiftChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ResourceShiftChangeHistoryDTO> output = new List<ResourceShiftChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ResourceShiftChangeHistory resourceShiftChangeHistory in data)
			{
				output.Add(resourceShiftChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ResourceShiftChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ResourceShiftChangeHistoryEntity type directly.
		///
		/// </summary>
		public ResourceShiftChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ResourceShiftChangeHistoryOutputDTO
			{
				id = this.id,
				resourceShiftId = this.resourceShiftId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				resourceShift = this.resourceShift?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ResourceShiftChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ResourceShiftChangeHistory objects to avoid using the ResourceShiftChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ResourceShiftChangeHistoryOutputDTO> ToOutputDTOList(List<ResourceShiftChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ResourceShiftChangeHistoryOutputDTO> output = new List<ResourceShiftChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ResourceShiftChangeHistory resourceShiftChangeHistory in data)
			{
				output.Add(resourceShiftChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ResourceShiftChangeHistory Object.
		///
		/// </summary>
		public static Database.ResourceShiftChangeHistory FromDTO(ResourceShiftChangeHistoryDTO dto)
		{
			return new Database.ResourceShiftChangeHistory
			{
				id = dto.id,
				resourceShiftId = dto.resourceShiftId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ResourceShiftChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ResourceShiftChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.resourceShiftId = dto.resourceShiftId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ResourceShiftChangeHistory Object.
		///
		/// </summary>
		public ResourceShiftChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ResourceShiftChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				resourceShiftId = this.resourceShiftId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ResourceShiftChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ResourceShiftChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ResourceShiftChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ResourceShiftChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ResourceShiftChangeHistory resourceShiftChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (resourceShiftChangeHistory == null)
			{
				return null;
			}

			return new {
				id = resourceShiftChangeHistory.id,
				resourceShiftId = resourceShiftChangeHistory.resourceShiftId,
				versionNumber = resourceShiftChangeHistory.versionNumber,
				timeStamp = resourceShiftChangeHistory.timeStamp,
				userId = resourceShiftChangeHistory.userId,
				data = resourceShiftChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ResourceShiftChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ResourceShiftChangeHistory resourceShiftChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (resourceShiftChangeHistory == null)
			{
				return null;
			}

			return new {
				id = resourceShiftChangeHistory.id,
				resourceShiftId = resourceShiftChangeHistory.resourceShiftId,
				versionNumber = resourceShiftChangeHistory.versionNumber,
				timeStamp = resourceShiftChangeHistory.timeStamp,
				userId = resourceShiftChangeHistory.userId,
				data = resourceShiftChangeHistory.data,
				resourceShift = ResourceShift.CreateMinimalAnonymous(resourceShiftChangeHistory.resourceShift)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ResourceShiftChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ResourceShiftChangeHistory resourceShiftChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (resourceShiftChangeHistory == null)
			{
				return null;
			}

			return new {
				id = resourceShiftChangeHistory.id,
				name = resourceShiftChangeHistory.id,
				description = resourceShiftChangeHistory.id
			 };
		}
	}
}
