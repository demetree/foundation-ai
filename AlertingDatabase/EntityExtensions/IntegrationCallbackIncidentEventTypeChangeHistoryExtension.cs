using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Alerting.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class IntegrationCallbackIncidentEventTypeChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)integrationCallbackIncidentEventTypeId; }
			set { integrationCallbackIncidentEventTypeId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class IntegrationCallbackIncidentEventTypeChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 integrationCallbackIncidentEventTypeId { get; set; }
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
		public class IntegrationCallbackIncidentEventTypeChangeHistoryOutputDTO : IntegrationCallbackIncidentEventTypeChangeHistoryDTO
		{
			public IntegrationCallbackIncidentEventType.IntegrationCallbackIncidentEventTypeDTO integrationCallbackIncidentEventType { get; set; }
		}


		/// <summary>
		///
		/// Converts a IntegrationCallbackIncidentEventTypeChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public IntegrationCallbackIncidentEventTypeChangeHistoryDTO ToDTO()
		{
			return new IntegrationCallbackIncidentEventTypeChangeHistoryDTO
			{
				id = this.id,
				integrationCallbackIncidentEventTypeId = this.integrationCallbackIncidentEventTypeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a IntegrationCallbackIncidentEventTypeChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<IntegrationCallbackIncidentEventTypeChangeHistoryDTO> ToDTOList(List<IntegrationCallbackIncidentEventTypeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IntegrationCallbackIncidentEventTypeChangeHistoryDTO> output = new List<IntegrationCallbackIncidentEventTypeChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (IntegrationCallbackIncidentEventTypeChangeHistory integrationCallbackIncidentEventTypeChangeHistory in data)
			{
				output.Add(integrationCallbackIncidentEventTypeChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a IntegrationCallbackIncidentEventTypeChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the IntegrationCallbackIncidentEventTypeChangeHistoryEntity type directly.
		///
		/// </summary>
		public IntegrationCallbackIncidentEventTypeChangeHistoryOutputDTO ToOutputDTO()
		{
			return new IntegrationCallbackIncidentEventTypeChangeHistoryOutputDTO
			{
				id = this.id,
				integrationCallbackIncidentEventTypeId = this.integrationCallbackIncidentEventTypeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				integrationCallbackIncidentEventType = this.integrationCallbackIncidentEventType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a IntegrationCallbackIncidentEventTypeChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of IntegrationCallbackIncidentEventTypeChangeHistory objects to avoid using the IntegrationCallbackIncidentEventTypeChangeHistory entity type directly.
		///
		/// </summary>
		public static List<IntegrationCallbackIncidentEventTypeChangeHistoryOutputDTO> ToOutputDTOList(List<IntegrationCallbackIncidentEventTypeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IntegrationCallbackIncidentEventTypeChangeHistoryOutputDTO> output = new List<IntegrationCallbackIncidentEventTypeChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (IntegrationCallbackIncidentEventTypeChangeHistory integrationCallbackIncidentEventTypeChangeHistory in data)
			{
				output.Add(integrationCallbackIncidentEventTypeChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a IntegrationCallbackIncidentEventTypeChangeHistory Object.
		///
		/// </summary>
		public static Database.IntegrationCallbackIncidentEventTypeChangeHistory FromDTO(IntegrationCallbackIncidentEventTypeChangeHistoryDTO dto)
		{
			return new Database.IntegrationCallbackIncidentEventTypeChangeHistory
			{
				id = dto.id,
				integrationCallbackIncidentEventTypeId = dto.integrationCallbackIncidentEventTypeId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a IntegrationCallbackIncidentEventTypeChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(IntegrationCallbackIncidentEventTypeChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.integrationCallbackIncidentEventTypeId = dto.integrationCallbackIncidentEventTypeId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a IntegrationCallbackIncidentEventTypeChangeHistory Object.
		///
		/// </summary>
		public IntegrationCallbackIncidentEventTypeChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new IntegrationCallbackIncidentEventTypeChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				integrationCallbackIncidentEventTypeId = this.integrationCallbackIncidentEventTypeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IntegrationCallbackIncidentEventTypeChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IntegrationCallbackIncidentEventTypeChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a IntegrationCallbackIncidentEventTypeChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a IntegrationCallbackIncidentEventTypeChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.IntegrationCallbackIncidentEventTypeChangeHistory integrationCallbackIncidentEventTypeChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (integrationCallbackIncidentEventTypeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = integrationCallbackIncidentEventTypeChangeHistory.id,
				integrationCallbackIncidentEventTypeId = integrationCallbackIncidentEventTypeChangeHistory.integrationCallbackIncidentEventTypeId,
				versionNumber = integrationCallbackIncidentEventTypeChangeHistory.versionNumber,
				timeStamp = integrationCallbackIncidentEventTypeChangeHistory.timeStamp,
				userId = integrationCallbackIncidentEventTypeChangeHistory.userId,
				data = integrationCallbackIncidentEventTypeChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a IntegrationCallbackIncidentEventTypeChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(IntegrationCallbackIncidentEventTypeChangeHistory integrationCallbackIncidentEventTypeChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (integrationCallbackIncidentEventTypeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = integrationCallbackIncidentEventTypeChangeHistory.id,
				integrationCallbackIncidentEventTypeId = integrationCallbackIncidentEventTypeChangeHistory.integrationCallbackIncidentEventTypeId,
				versionNumber = integrationCallbackIncidentEventTypeChangeHistory.versionNumber,
				timeStamp = integrationCallbackIncidentEventTypeChangeHistory.timeStamp,
				userId = integrationCallbackIncidentEventTypeChangeHistory.userId,
				data = integrationCallbackIncidentEventTypeChangeHistory.data,
				integrationCallbackIncidentEventType = IntegrationCallbackIncidentEventType.CreateMinimalAnonymous(integrationCallbackIncidentEventTypeChangeHistory.integrationCallbackIncidentEventType)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a IntegrationCallbackIncidentEventTypeChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(IntegrationCallbackIncidentEventTypeChangeHistory integrationCallbackIncidentEventTypeChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (integrationCallbackIncidentEventTypeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = integrationCallbackIncidentEventTypeChangeHistory.id,
				name = integrationCallbackIncidentEventTypeChangeHistory.id,
				description = integrationCallbackIncidentEventTypeChangeHistory.id
			 };
		}
	}
}
