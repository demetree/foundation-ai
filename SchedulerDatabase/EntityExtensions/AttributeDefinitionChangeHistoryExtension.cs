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
	public partial class AttributeDefinitionChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)attributeDefinitionId; }
			set { attributeDefinitionId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AttributeDefinitionChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 attributeDefinitionId { get; set; }
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
		public class AttributeDefinitionChangeHistoryOutputDTO : AttributeDefinitionChangeHistoryDTO
		{
			public AttributeDefinition.AttributeDefinitionDTO attributeDefinition { get; set; }
		}


		/// <summary>
		///
		/// Converts a AttributeDefinitionChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AttributeDefinitionChangeHistoryDTO ToDTO()
		{
			return new AttributeDefinitionChangeHistoryDTO
			{
				id = this.id,
				attributeDefinitionId = this.attributeDefinitionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a AttributeDefinitionChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AttributeDefinitionChangeHistoryDTO> ToDTOList(List<AttributeDefinitionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AttributeDefinitionChangeHistoryDTO> output = new List<AttributeDefinitionChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (AttributeDefinitionChangeHistory attributeDefinitionChangeHistory in data)
			{
				output.Add(attributeDefinitionChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AttributeDefinitionChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AttributeDefinitionChangeHistory Entity type directly.
		///
		/// </summary>
		public AttributeDefinitionChangeHistoryOutputDTO ToOutputDTO()
		{
			return new AttributeDefinitionChangeHistoryOutputDTO
			{
				id = this.id,
				attributeDefinitionId = this.attributeDefinitionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				attributeDefinition = this.attributeDefinition?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a AttributeDefinitionChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of AttributeDefinitionChangeHistory objects to avoid using the AttributeDefinitionChangeHistory entity type directly.
		///
		/// </summary>
		public static List<AttributeDefinitionChangeHistoryOutputDTO> ToOutputDTOList(List<AttributeDefinitionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AttributeDefinitionChangeHistoryOutputDTO> output = new List<AttributeDefinitionChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (AttributeDefinitionChangeHistory attributeDefinitionChangeHistory in data)
			{
				output.Add(attributeDefinitionChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AttributeDefinitionChangeHistory Object.
		///
		/// </summary>
		public static Database.AttributeDefinitionChangeHistory FromDTO(AttributeDefinitionChangeHistoryDTO dto)
		{
			return new Database.AttributeDefinitionChangeHistory
			{
				id = dto.id,
				attributeDefinitionId = dto.attributeDefinitionId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AttributeDefinitionChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(AttributeDefinitionChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.attributeDefinitionId = dto.attributeDefinitionId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a AttributeDefinitionChangeHistory Object.
		///
		/// </summary>
		public AttributeDefinitionChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AttributeDefinitionChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				attributeDefinitionId = this.attributeDefinitionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AttributeDefinitionChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AttributeDefinitionChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AttributeDefinitionChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AttributeDefinitionChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AttributeDefinitionChangeHistory attributeDefinitionChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (attributeDefinitionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = attributeDefinitionChangeHistory.id,
				attributeDefinitionId = attributeDefinitionChangeHistory.attributeDefinitionId,
				versionNumber = attributeDefinitionChangeHistory.versionNumber,
				timeStamp = attributeDefinitionChangeHistory.timeStamp,
				userId = attributeDefinitionChangeHistory.userId,
				data = attributeDefinitionChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AttributeDefinitionChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AttributeDefinitionChangeHistory attributeDefinitionChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (attributeDefinitionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = attributeDefinitionChangeHistory.id,
				attributeDefinitionId = attributeDefinitionChangeHistory.attributeDefinitionId,
				versionNumber = attributeDefinitionChangeHistory.versionNumber,
				timeStamp = attributeDefinitionChangeHistory.timeStamp,
				userId = attributeDefinitionChangeHistory.userId,
				data = attributeDefinitionChangeHistory.data,
				attributeDefinition = AttributeDefinition.CreateMinimalAnonymous(attributeDefinitionChangeHistory.attributeDefinition)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AttributeDefinitionChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AttributeDefinitionChangeHistory attributeDefinitionChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (attributeDefinitionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = attributeDefinitionChangeHistory.id,
				name = attributeDefinitionChangeHistory.id,
				description = attributeDefinitionChangeHistory.id
			 };
		}
	}
}
