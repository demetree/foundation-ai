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
	public partial class ResourceQualificationChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)resourceQualificationId; }
			set { resourceQualificationId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ResourceQualificationChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 resourceQualificationId { get; set; }
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
		public class ResourceQualificationChangeHistoryOutputDTO : ResourceQualificationChangeHistoryDTO
		{
			public ResourceQualification.ResourceQualificationDTO resourceQualification { get; set; }
		}


		/// <summary>
		///
		/// Converts a ResourceQualificationChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ResourceQualificationChangeHistoryDTO ToDTO()
		{
			return new ResourceQualificationChangeHistoryDTO
			{
				id = this.id,
				resourceQualificationId = this.resourceQualificationId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ResourceQualificationChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ResourceQualificationChangeHistoryDTO> ToDTOList(List<ResourceQualificationChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ResourceQualificationChangeHistoryDTO> output = new List<ResourceQualificationChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ResourceQualificationChangeHistory resourceQualificationChangeHistory in data)
			{
				output.Add(resourceQualificationChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ResourceQualificationChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ResourceQualificationChangeHistory Entity type directly.
		///
		/// </summary>
		public ResourceQualificationChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ResourceQualificationChangeHistoryOutputDTO
			{
				id = this.id,
				resourceQualificationId = this.resourceQualificationId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				resourceQualification = this.resourceQualification?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ResourceQualificationChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ResourceQualificationChangeHistory objects to avoid using the ResourceQualificationChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ResourceQualificationChangeHistoryOutputDTO> ToOutputDTOList(List<ResourceQualificationChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ResourceQualificationChangeHistoryOutputDTO> output = new List<ResourceQualificationChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ResourceQualificationChangeHistory resourceQualificationChangeHistory in data)
			{
				output.Add(resourceQualificationChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ResourceQualificationChangeHistory Object.
		///
		/// </summary>
		public static Database.ResourceQualificationChangeHistory FromDTO(ResourceQualificationChangeHistoryDTO dto)
		{
			return new Database.ResourceQualificationChangeHistory
			{
				id = dto.id,
				resourceQualificationId = dto.resourceQualificationId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ResourceQualificationChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ResourceQualificationChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.resourceQualificationId = dto.resourceQualificationId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ResourceQualificationChangeHistory Object.
		///
		/// </summary>
		public ResourceQualificationChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ResourceQualificationChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				resourceQualificationId = this.resourceQualificationId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ResourceQualificationChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ResourceQualificationChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ResourceQualificationChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ResourceQualificationChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ResourceQualificationChangeHistory resourceQualificationChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (resourceQualificationChangeHistory == null)
			{
				return null;
			}

			return new {
				id = resourceQualificationChangeHistory.id,
				resourceQualificationId = resourceQualificationChangeHistory.resourceQualificationId,
				versionNumber = resourceQualificationChangeHistory.versionNumber,
				timeStamp = resourceQualificationChangeHistory.timeStamp,
				userId = resourceQualificationChangeHistory.userId,
				data = resourceQualificationChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ResourceQualificationChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ResourceQualificationChangeHistory resourceQualificationChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (resourceQualificationChangeHistory == null)
			{
				return null;
			}

			return new {
				id = resourceQualificationChangeHistory.id,
				resourceQualificationId = resourceQualificationChangeHistory.resourceQualificationId,
				versionNumber = resourceQualificationChangeHistory.versionNumber,
				timeStamp = resourceQualificationChangeHistory.timeStamp,
				userId = resourceQualificationChangeHistory.userId,
				data = resourceQualificationChangeHistory.data,
				resourceQualification = ResourceQualification.CreateMinimalAnonymous(resourceQualificationChangeHistory.resourceQualification)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ResourceQualificationChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ResourceQualificationChangeHistory resourceQualificationChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (resourceQualificationChangeHistory == null)
			{
				return null;
			}

			return new {
				id = resourceQualificationChangeHistory.id,
				name = resourceQualificationChangeHistory.id,
				description = resourceQualificationChangeHistory.id
			 };
		}
	}
}
