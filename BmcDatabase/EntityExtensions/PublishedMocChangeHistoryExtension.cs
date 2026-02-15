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
	public partial class PublishedMocChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)publishedMocId; }
			set { publishedMocId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PublishedMocChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 publishedMocId { get; set; }
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
		public class PublishedMocChangeHistoryOutputDTO : PublishedMocChangeHistoryDTO
		{
			public PublishedMoc.PublishedMocDTO publishedMoc { get; set; }
		}


		/// <summary>
		///
		/// Converts a PublishedMocChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PublishedMocChangeHistoryDTO ToDTO()
		{
			return new PublishedMocChangeHistoryDTO
			{
				id = this.id,
				publishedMocId = this.publishedMocId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a PublishedMocChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PublishedMocChangeHistoryDTO> ToDTOList(List<PublishedMocChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PublishedMocChangeHistoryDTO> output = new List<PublishedMocChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (PublishedMocChangeHistory publishedMocChangeHistory in data)
			{
				output.Add(publishedMocChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PublishedMocChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PublishedMocChangeHistoryEntity type directly.
		///
		/// </summary>
		public PublishedMocChangeHistoryOutputDTO ToOutputDTO()
		{
			return new PublishedMocChangeHistoryOutputDTO
			{
				id = this.id,
				publishedMocId = this.publishedMocId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				publishedMoc = this.publishedMoc?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a PublishedMocChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of PublishedMocChangeHistory objects to avoid using the PublishedMocChangeHistory entity type directly.
		///
		/// </summary>
		public static List<PublishedMocChangeHistoryOutputDTO> ToOutputDTOList(List<PublishedMocChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PublishedMocChangeHistoryOutputDTO> output = new List<PublishedMocChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (PublishedMocChangeHistory publishedMocChangeHistory in data)
			{
				output.Add(publishedMocChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PublishedMocChangeHistory Object.
		///
		/// </summary>
		public static Database.PublishedMocChangeHistory FromDTO(PublishedMocChangeHistoryDTO dto)
		{
			return new Database.PublishedMocChangeHistory
			{
				id = dto.id,
				publishedMocId = dto.publishedMocId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PublishedMocChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(PublishedMocChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.publishedMocId = dto.publishedMocId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a PublishedMocChangeHistory Object.
		///
		/// </summary>
		public PublishedMocChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PublishedMocChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				publishedMocId = this.publishedMocId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PublishedMocChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PublishedMocChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PublishedMocChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PublishedMocChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PublishedMocChangeHistory publishedMocChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (publishedMocChangeHistory == null)
			{
				return null;
			}

			return new {
				id = publishedMocChangeHistory.id,
				publishedMocId = publishedMocChangeHistory.publishedMocId,
				versionNumber = publishedMocChangeHistory.versionNumber,
				timeStamp = publishedMocChangeHistory.timeStamp,
				userId = publishedMocChangeHistory.userId,
				data = publishedMocChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PublishedMocChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PublishedMocChangeHistory publishedMocChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (publishedMocChangeHistory == null)
			{
				return null;
			}

			return new {
				id = publishedMocChangeHistory.id,
				publishedMocId = publishedMocChangeHistory.publishedMocId,
				versionNumber = publishedMocChangeHistory.versionNumber,
				timeStamp = publishedMocChangeHistory.timeStamp,
				userId = publishedMocChangeHistory.userId,
				data = publishedMocChangeHistory.data,
				publishedMoc = PublishedMoc.CreateMinimalAnonymous(publishedMocChangeHistory.publishedMoc)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PublishedMocChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PublishedMocChangeHistory publishedMocChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (publishedMocChangeHistory == null)
			{
				return null;
			}

			return new {
				id = publishedMocChangeHistory.id,
				name = publishedMocChangeHistory.id,
				description = publishedMocChangeHistory.id
			 };
		}
	}
}
