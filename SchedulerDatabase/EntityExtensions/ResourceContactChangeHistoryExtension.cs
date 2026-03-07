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
	public partial class ResourceContactChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)resourceContactId; }
			set { resourceContactId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ResourceContactChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 resourceContactId { get; set; }
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
		public class ResourceContactChangeHistoryOutputDTO : ResourceContactChangeHistoryDTO
		{
			public ResourceContact.ResourceContactDTO resourceContact { get; set; }
		}


		/// <summary>
		///
		/// Converts a ResourceContactChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ResourceContactChangeHistoryDTO ToDTO()
		{
			return new ResourceContactChangeHistoryDTO
			{
				id = this.id,
				resourceContactId = this.resourceContactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ResourceContactChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ResourceContactChangeHistoryDTO> ToDTOList(List<ResourceContactChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ResourceContactChangeHistoryDTO> output = new List<ResourceContactChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ResourceContactChangeHistory resourceContactChangeHistory in data)
			{
				output.Add(resourceContactChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ResourceContactChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ResourceContactChangeHistoryEntity type directly.
		///
		/// </summary>
		public ResourceContactChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ResourceContactChangeHistoryOutputDTO
			{
				id = this.id,
				resourceContactId = this.resourceContactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				resourceContact = this.resourceContact?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ResourceContactChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ResourceContactChangeHistory objects to avoid using the ResourceContactChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ResourceContactChangeHistoryOutputDTO> ToOutputDTOList(List<ResourceContactChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ResourceContactChangeHistoryOutputDTO> output = new List<ResourceContactChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ResourceContactChangeHistory resourceContactChangeHistory in data)
			{
				output.Add(resourceContactChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ResourceContactChangeHistory Object.
		///
		/// </summary>
		public static Database.ResourceContactChangeHistory FromDTO(ResourceContactChangeHistoryDTO dto)
		{
			return new Database.ResourceContactChangeHistory
			{
				id = dto.id,
				resourceContactId = dto.resourceContactId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ResourceContactChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ResourceContactChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.resourceContactId = dto.resourceContactId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ResourceContactChangeHistory Object.
		///
		/// </summary>
		public ResourceContactChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ResourceContactChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				resourceContactId = this.resourceContactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ResourceContactChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ResourceContactChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ResourceContactChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ResourceContactChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ResourceContactChangeHistory resourceContactChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (resourceContactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = resourceContactChangeHistory.id,
				resourceContactId = resourceContactChangeHistory.resourceContactId,
				versionNumber = resourceContactChangeHistory.versionNumber,
				timeStamp = resourceContactChangeHistory.timeStamp,
				userId = resourceContactChangeHistory.userId,
				data = resourceContactChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ResourceContactChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ResourceContactChangeHistory resourceContactChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (resourceContactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = resourceContactChangeHistory.id,
				resourceContactId = resourceContactChangeHistory.resourceContactId,
				versionNumber = resourceContactChangeHistory.versionNumber,
				timeStamp = resourceContactChangeHistory.timeStamp,
				userId = resourceContactChangeHistory.userId,
				data = resourceContactChangeHistory.data,
				resourceContact = ResourceContact.CreateMinimalAnonymous(resourceContactChangeHistory.resourceContact)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ResourceContactChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ResourceContactChangeHistory resourceContactChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (resourceContactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = resourceContactChangeHistory.id,
				name = resourceContactChangeHistory.id,
				description = resourceContactChangeHistory.id
			 };
		}
	}
}
