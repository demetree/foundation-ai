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
	public partial class ResourceAvailabilityChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)resourceAvailabilityId; }
			set { resourceAvailabilityId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ResourceAvailabilityChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 resourceAvailabilityId { get; set; }
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
		public class ResourceAvailabilityChangeHistoryOutputDTO : ResourceAvailabilityChangeHistoryDTO
		{
			public ResourceAvailability.ResourceAvailabilityDTO resourceAvailability { get; set; }
		}


		/// <summary>
		///
		/// Converts a ResourceAvailabilityChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ResourceAvailabilityChangeHistoryDTO ToDTO()
		{
			return new ResourceAvailabilityChangeHistoryDTO
			{
				id = this.id,
				resourceAvailabilityId = this.resourceAvailabilityId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ResourceAvailabilityChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ResourceAvailabilityChangeHistoryDTO> ToDTOList(List<ResourceAvailabilityChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ResourceAvailabilityChangeHistoryDTO> output = new List<ResourceAvailabilityChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ResourceAvailabilityChangeHistory resourceAvailabilityChangeHistory in data)
			{
				output.Add(resourceAvailabilityChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ResourceAvailabilityChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ResourceAvailabilityChangeHistory Entity type directly.
		///
		/// </summary>
		public ResourceAvailabilityChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ResourceAvailabilityChangeHistoryOutputDTO
			{
				id = this.id,
				resourceAvailabilityId = this.resourceAvailabilityId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				resourceAvailability = this.resourceAvailability?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ResourceAvailabilityChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ResourceAvailabilityChangeHistory objects to avoid using the ResourceAvailabilityChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ResourceAvailabilityChangeHistoryOutputDTO> ToOutputDTOList(List<ResourceAvailabilityChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ResourceAvailabilityChangeHistoryOutputDTO> output = new List<ResourceAvailabilityChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ResourceAvailabilityChangeHistory resourceAvailabilityChangeHistory in data)
			{
				output.Add(resourceAvailabilityChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ResourceAvailabilityChangeHistory Object.
		///
		/// </summary>
		public static Database.ResourceAvailabilityChangeHistory FromDTO(ResourceAvailabilityChangeHistoryDTO dto)
		{
			return new Database.ResourceAvailabilityChangeHistory
			{
				id = dto.id,
				resourceAvailabilityId = dto.resourceAvailabilityId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ResourceAvailabilityChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ResourceAvailabilityChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.resourceAvailabilityId = dto.resourceAvailabilityId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ResourceAvailabilityChangeHistory Object.
		///
		/// </summary>
		public ResourceAvailabilityChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ResourceAvailabilityChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				resourceAvailabilityId = this.resourceAvailabilityId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ResourceAvailabilityChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ResourceAvailabilityChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ResourceAvailabilityChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ResourceAvailabilityChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ResourceAvailabilityChangeHistory resourceAvailabilityChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (resourceAvailabilityChangeHistory == null)
			{
				return null;
			}

			return new {
				id = resourceAvailabilityChangeHistory.id,
				resourceAvailabilityId = resourceAvailabilityChangeHistory.resourceAvailabilityId,
				versionNumber = resourceAvailabilityChangeHistory.versionNumber,
				timeStamp = resourceAvailabilityChangeHistory.timeStamp,
				userId = resourceAvailabilityChangeHistory.userId,
				data = resourceAvailabilityChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ResourceAvailabilityChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ResourceAvailabilityChangeHistory resourceAvailabilityChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (resourceAvailabilityChangeHistory == null)
			{
				return null;
			}

			return new {
				id = resourceAvailabilityChangeHistory.id,
				resourceAvailabilityId = resourceAvailabilityChangeHistory.resourceAvailabilityId,
				versionNumber = resourceAvailabilityChangeHistory.versionNumber,
				timeStamp = resourceAvailabilityChangeHistory.timeStamp,
				userId = resourceAvailabilityChangeHistory.userId,
				data = resourceAvailabilityChangeHistory.data,
				resourceAvailability = ResourceAvailability.CreateMinimalAnonymous(resourceAvailabilityChangeHistory.resourceAvailability),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ResourceAvailabilityChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ResourceAvailabilityChangeHistory resourceAvailabilityChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (resourceAvailabilityChangeHistory == null)
			{
				return null;
			}

			return new {
				id = resourceAvailabilityChangeHistory.id,
				name = resourceAvailabilityChangeHistory.id,
				description = resourceAvailabilityChangeHistory.id
			 };
		}
	}
}
