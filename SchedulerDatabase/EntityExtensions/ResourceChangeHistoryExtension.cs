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
	public partial class ResourceChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)resourceId; }
			set { resourceId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ResourceChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 resourceId { get; set; }
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
		public class ResourceChangeHistoryOutputDTO : ResourceChangeHistoryDTO
		{
			public Resource.ResourceDTO resource { get; set; }
		}


		/// <summary>
		///
		/// Converts a ResourceChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ResourceChangeHistoryDTO ToDTO()
		{
			return new ResourceChangeHistoryDTO
			{
				id = this.id,
				resourceId = this.resourceId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ResourceChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ResourceChangeHistoryDTO> ToDTOList(List<ResourceChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ResourceChangeHistoryDTO> output = new List<ResourceChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ResourceChangeHistory resourceChangeHistory in data)
			{
				output.Add(resourceChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ResourceChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ResourceChangeHistoryEntity type directly.
		///
		/// </summary>
		public ResourceChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ResourceChangeHistoryOutputDTO
			{
				id = this.id,
				resourceId = this.resourceId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				resource = this.resource?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ResourceChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ResourceChangeHistory objects to avoid using the ResourceChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ResourceChangeHistoryOutputDTO> ToOutputDTOList(List<ResourceChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ResourceChangeHistoryOutputDTO> output = new List<ResourceChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ResourceChangeHistory resourceChangeHistory in data)
			{
				output.Add(resourceChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ResourceChangeHistory Object.
		///
		/// </summary>
		public static Database.ResourceChangeHistory FromDTO(ResourceChangeHistoryDTO dto)
		{
			return new Database.ResourceChangeHistory
			{
				id = dto.id,
				resourceId = dto.resourceId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ResourceChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ResourceChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.resourceId = dto.resourceId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ResourceChangeHistory Object.
		///
		/// </summary>
		public ResourceChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ResourceChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				resourceId = this.resourceId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ResourceChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ResourceChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ResourceChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ResourceChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ResourceChangeHistory resourceChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (resourceChangeHistory == null)
			{
				return null;
			}

			return new {
				id = resourceChangeHistory.id,
				resourceId = resourceChangeHistory.resourceId,
				versionNumber = resourceChangeHistory.versionNumber,
				timeStamp = resourceChangeHistory.timeStamp,
				userId = resourceChangeHistory.userId,
				data = resourceChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ResourceChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ResourceChangeHistory resourceChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (resourceChangeHistory == null)
			{
				return null;
			}

			return new {
				id = resourceChangeHistory.id,
				resourceId = resourceChangeHistory.resourceId,
				versionNumber = resourceChangeHistory.versionNumber,
				timeStamp = resourceChangeHistory.timeStamp,
				userId = resourceChangeHistory.userId,
				data = resourceChangeHistory.data,
				resource = Resource.CreateMinimalAnonymous(resourceChangeHistory.resource),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ResourceChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ResourceChangeHistory resourceChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (resourceChangeHistory == null)
			{
				return null;
			}

			return new {
				id = resourceChangeHistory.id,
				name = resourceChangeHistory.id,
				description = resourceChangeHistory.id
			 };
		}
	}
}
