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
	public partial class EscalationRuleChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)escalationRuleId; }
			set { escalationRuleId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class EscalationRuleChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 escalationRuleId { get; set; }
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
		public class EscalationRuleChangeHistoryOutputDTO : EscalationRuleChangeHistoryDTO
		{
			public EscalationRule.EscalationRuleDTO escalationRule { get; set; }
		}


		/// <summary>
		///
		/// Converts a EscalationRuleChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public EscalationRuleChangeHistoryDTO ToDTO()
		{
			return new EscalationRuleChangeHistoryDTO
			{
				id = this.id,
				escalationRuleId = this.escalationRuleId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a EscalationRuleChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<EscalationRuleChangeHistoryDTO> ToDTOList(List<EscalationRuleChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EscalationRuleChangeHistoryDTO> output = new List<EscalationRuleChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (EscalationRuleChangeHistory escalationRuleChangeHistory in data)
			{
				output.Add(escalationRuleChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a EscalationRuleChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the EscalationRuleChangeHistoryEntity type directly.
		///
		/// </summary>
		public EscalationRuleChangeHistoryOutputDTO ToOutputDTO()
		{
			return new EscalationRuleChangeHistoryOutputDTO
			{
				id = this.id,
				escalationRuleId = this.escalationRuleId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				escalationRule = this.escalationRule?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a EscalationRuleChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of EscalationRuleChangeHistory objects to avoid using the EscalationRuleChangeHistory entity type directly.
		///
		/// </summary>
		public static List<EscalationRuleChangeHistoryOutputDTO> ToOutputDTOList(List<EscalationRuleChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EscalationRuleChangeHistoryOutputDTO> output = new List<EscalationRuleChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (EscalationRuleChangeHistory escalationRuleChangeHistory in data)
			{
				output.Add(escalationRuleChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a EscalationRuleChangeHistory Object.
		///
		/// </summary>
		public static Database.EscalationRuleChangeHistory FromDTO(EscalationRuleChangeHistoryDTO dto)
		{
			return new Database.EscalationRuleChangeHistory
			{
				id = dto.id,
				escalationRuleId = dto.escalationRuleId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a EscalationRuleChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(EscalationRuleChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.escalationRuleId = dto.escalationRuleId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a EscalationRuleChangeHistory Object.
		///
		/// </summary>
		public EscalationRuleChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new EscalationRuleChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				escalationRuleId = this.escalationRuleId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EscalationRuleChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EscalationRuleChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a EscalationRuleChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a EscalationRuleChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.EscalationRuleChangeHistory escalationRuleChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (escalationRuleChangeHistory == null)
			{
				return null;
			}

			return new {
				id = escalationRuleChangeHistory.id,
				escalationRuleId = escalationRuleChangeHistory.escalationRuleId,
				versionNumber = escalationRuleChangeHistory.versionNumber,
				timeStamp = escalationRuleChangeHistory.timeStamp,
				userId = escalationRuleChangeHistory.userId,
				data = escalationRuleChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a EscalationRuleChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(EscalationRuleChangeHistory escalationRuleChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (escalationRuleChangeHistory == null)
			{
				return null;
			}

			return new {
				id = escalationRuleChangeHistory.id,
				escalationRuleId = escalationRuleChangeHistory.escalationRuleId,
				versionNumber = escalationRuleChangeHistory.versionNumber,
				timeStamp = escalationRuleChangeHistory.timeStamp,
				userId = escalationRuleChangeHistory.userId,
				data = escalationRuleChangeHistory.data,
				escalationRule = EscalationRule.CreateMinimalAnonymous(escalationRuleChangeHistory.escalationRule),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a EscalationRuleChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(EscalationRuleChangeHistory escalationRuleChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (escalationRuleChangeHistory == null)
			{
				return null;
			}

			return new {
				id = escalationRuleChangeHistory.id,
				name = escalationRuleChangeHistory.id,
				description = escalationRuleChangeHistory.id
			 };
		}
	}
}
