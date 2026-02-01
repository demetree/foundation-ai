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
	public partial class EscalationPolicyChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)escalationPolicyId; }
			set { escalationPolicyId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class EscalationPolicyChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 escalationPolicyId { get; set; }
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
		public class EscalationPolicyChangeHistoryOutputDTO : EscalationPolicyChangeHistoryDTO
		{
			public EscalationPolicy.EscalationPolicyDTO escalationPolicy { get; set; }
		}


		/// <summary>
		///
		/// Converts a EscalationPolicyChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public EscalationPolicyChangeHistoryDTO ToDTO()
		{
			return new EscalationPolicyChangeHistoryDTO
			{
				id = this.id,
				escalationPolicyId = this.escalationPolicyId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a EscalationPolicyChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<EscalationPolicyChangeHistoryDTO> ToDTOList(List<EscalationPolicyChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EscalationPolicyChangeHistoryDTO> output = new List<EscalationPolicyChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (EscalationPolicyChangeHistory escalationPolicyChangeHistory in data)
			{
				output.Add(escalationPolicyChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a EscalationPolicyChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the EscalationPolicyChangeHistoryEntity type directly.
		///
		/// </summary>
		public EscalationPolicyChangeHistoryOutputDTO ToOutputDTO()
		{
			return new EscalationPolicyChangeHistoryOutputDTO
			{
				id = this.id,
				escalationPolicyId = this.escalationPolicyId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				escalationPolicy = this.escalationPolicy?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a EscalationPolicyChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of EscalationPolicyChangeHistory objects to avoid using the EscalationPolicyChangeHistory entity type directly.
		///
		/// </summary>
		public static List<EscalationPolicyChangeHistoryOutputDTO> ToOutputDTOList(List<EscalationPolicyChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<EscalationPolicyChangeHistoryOutputDTO> output = new List<EscalationPolicyChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (EscalationPolicyChangeHistory escalationPolicyChangeHistory in data)
			{
				output.Add(escalationPolicyChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a EscalationPolicyChangeHistory Object.
		///
		/// </summary>
		public static Database.EscalationPolicyChangeHistory FromDTO(EscalationPolicyChangeHistoryDTO dto)
		{
			return new Database.EscalationPolicyChangeHistory
			{
				id = dto.id,
				escalationPolicyId = dto.escalationPolicyId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a EscalationPolicyChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(EscalationPolicyChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.escalationPolicyId = dto.escalationPolicyId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a EscalationPolicyChangeHistory Object.
		///
		/// </summary>
		public EscalationPolicyChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new EscalationPolicyChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				escalationPolicyId = this.escalationPolicyId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EscalationPolicyChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a EscalationPolicyChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a EscalationPolicyChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a EscalationPolicyChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.EscalationPolicyChangeHistory escalationPolicyChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (escalationPolicyChangeHistory == null)
			{
				return null;
			}

			return new {
				id = escalationPolicyChangeHistory.id,
				escalationPolicyId = escalationPolicyChangeHistory.escalationPolicyId,
				versionNumber = escalationPolicyChangeHistory.versionNumber,
				timeStamp = escalationPolicyChangeHistory.timeStamp,
				userId = escalationPolicyChangeHistory.userId,
				data = escalationPolicyChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a EscalationPolicyChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(EscalationPolicyChangeHistory escalationPolicyChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (escalationPolicyChangeHistory == null)
			{
				return null;
			}

			return new {
				id = escalationPolicyChangeHistory.id,
				escalationPolicyId = escalationPolicyChangeHistory.escalationPolicyId,
				versionNumber = escalationPolicyChangeHistory.versionNumber,
				timeStamp = escalationPolicyChangeHistory.timeStamp,
				userId = escalationPolicyChangeHistory.userId,
				data = escalationPolicyChangeHistory.data,
				escalationPolicy = EscalationPolicy.CreateMinimalAnonymous(escalationPolicyChangeHistory.escalationPolicy),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a EscalationPolicyChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(EscalationPolicyChangeHistory escalationPolicyChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (escalationPolicyChangeHistory == null)
			{
				return null;
			}

			return new {
				id = escalationPolicyChangeHistory.id,
				name = escalationPolicyChangeHistory.id,
				description = escalationPolicyChangeHistory.id
			 };
		}
	}
}
