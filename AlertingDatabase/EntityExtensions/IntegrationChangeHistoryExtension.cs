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
	public partial class IntegrationChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)integrationId; }
			set { integrationId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class IntegrationChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 integrationId { get; set; }
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
		public class IntegrationChangeHistoryOutputDTO : IntegrationChangeHistoryDTO
		{
			public Integration.IntegrationDTO integration { get; set; }
		}


		/// <summary>
		///
		/// Converts a IntegrationChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public IntegrationChangeHistoryDTO ToDTO()
		{
			return new IntegrationChangeHistoryDTO
			{
				id = this.id,
				integrationId = this.integrationId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a IntegrationChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<IntegrationChangeHistoryDTO> ToDTOList(List<IntegrationChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IntegrationChangeHistoryDTO> output = new List<IntegrationChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (IntegrationChangeHistory integrationChangeHistory in data)
			{
				output.Add(integrationChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a IntegrationChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the IntegrationChangeHistoryEntity type directly.
		///
		/// </summary>
		public IntegrationChangeHistoryOutputDTO ToOutputDTO()
		{
			return new IntegrationChangeHistoryOutputDTO
			{
				id = this.id,
				integrationId = this.integrationId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				integration = this.integration?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a IntegrationChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of IntegrationChangeHistory objects to avoid using the IntegrationChangeHistory entity type directly.
		///
		/// </summary>
		public static List<IntegrationChangeHistoryOutputDTO> ToOutputDTOList(List<IntegrationChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IntegrationChangeHistoryOutputDTO> output = new List<IntegrationChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (IntegrationChangeHistory integrationChangeHistory in data)
			{
				output.Add(integrationChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a IntegrationChangeHistory Object.
		///
		/// </summary>
		public static Database.IntegrationChangeHistory FromDTO(IntegrationChangeHistoryDTO dto)
		{
			return new Database.IntegrationChangeHistory
			{
				id = dto.id,
				integrationId = dto.integrationId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a IntegrationChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(IntegrationChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.integrationId = dto.integrationId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a IntegrationChangeHistory Object.
		///
		/// </summary>
		public IntegrationChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new IntegrationChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				integrationId = this.integrationId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IntegrationChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IntegrationChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a IntegrationChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a IntegrationChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.IntegrationChangeHistory integrationChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (integrationChangeHistory == null)
			{
				return null;
			}

			return new {
				id = integrationChangeHistory.id,
				integrationId = integrationChangeHistory.integrationId,
				versionNumber = integrationChangeHistory.versionNumber,
				timeStamp = integrationChangeHistory.timeStamp,
				userId = integrationChangeHistory.userId,
				data = integrationChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a IntegrationChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(IntegrationChangeHistory integrationChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (integrationChangeHistory == null)
			{
				return null;
			}

			return new {
				id = integrationChangeHistory.id,
				integrationId = integrationChangeHistory.integrationId,
				versionNumber = integrationChangeHistory.versionNumber,
				timeStamp = integrationChangeHistory.timeStamp,
				userId = integrationChangeHistory.userId,
				data = integrationChangeHistory.data,
				integration = Integration.CreateMinimalAnonymous(integrationChangeHistory.integration)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a IntegrationChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(IntegrationChangeHistory integrationChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (integrationChangeHistory == null)
			{
				return null;
			}

			return new {
				id = integrationChangeHistory.id,
				name = integrationChangeHistory.id,
				description = integrationChangeHistory.id
			 };
		}
	}
}
