using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class CallParticipant : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class CallParticipantDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 callId { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public String role { get; set; }
			[Required]
			public String status { get; set; }
			public DateTime? joinedDateTime { get; set; }
			public DateTime? leftDateTime { get; set; }
			[Required]
			public Guid objectGuid { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class CallParticipantOutputDTO : CallParticipantDTO
		{
			public Call.CallDTO call { get; set; }
		}


		/// <summary>
		///
		/// Converts a CallParticipant to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public CallParticipantDTO ToDTO()
		{
			return new CallParticipantDTO
			{
				id = this.id,
				callId = this.callId,
				userId = this.userId,
				role = this.role,
				status = this.status,
				joinedDateTime = this.joinedDateTime,
				leftDateTime = this.leftDateTime,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a CallParticipant list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<CallParticipantDTO> ToDTOList(List<CallParticipant> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CallParticipantDTO> output = new List<CallParticipantDTO>();

			output.Capacity = data.Count;

			foreach (CallParticipant callParticipant in data)
			{
				output.Add(callParticipant.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a CallParticipant to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the CallParticipant Entity type directly.
		///
		/// </summary>
		public CallParticipantOutputDTO ToOutputDTO()
		{
			return new CallParticipantOutputDTO
			{
				id = this.id,
				callId = this.callId,
				userId = this.userId,
				role = this.role,
				status = this.status,
				joinedDateTime = this.joinedDateTime,
				leftDateTime = this.leftDateTime,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				call = this.call?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a CallParticipant list to list of Output Data Transfer Object intended to be used for serializing a list of CallParticipant objects to avoid using the CallParticipant entity type directly.
		///
		/// </summary>
		public static List<CallParticipantOutputDTO> ToOutputDTOList(List<CallParticipant> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CallParticipantOutputDTO> output = new List<CallParticipantOutputDTO>();

			output.Capacity = data.Count;

			foreach (CallParticipant callParticipant in data)
			{
				output.Add(callParticipant.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a CallParticipant Object.
		///
		/// </summary>
		public static Database.CallParticipant FromDTO(CallParticipantDTO dto)
		{
			return new Database.CallParticipant
			{
				id = dto.id,
				callId = dto.callId,
				userId = dto.userId,
				role = dto.role,
				status = dto.status,
				joinedDateTime = dto.joinedDateTime,
				leftDateTime = dto.leftDateTime,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a CallParticipant Object.
		///
		/// </summary>
		public void ApplyDTO(CallParticipantDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.callId = dto.callId;
			this.userId = dto.userId;
			this.role = dto.role;
			this.status = dto.status;
			this.joinedDateTime = dto.joinedDateTime;
			this.leftDateTime = dto.leftDateTime;
			this.objectGuid = dto.objectGuid;
			if (dto.active.HasValue == true)
			{
				this.active = dto.active.Value;
			}
			if (dto.deleted.HasValue == true)
			{
				this.deleted = dto.deleted.Value;
			}
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a CallParticipant Object.
		///
		/// </summary>
		public CallParticipant Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new CallParticipant{
				id = this.id,
				tenantGuid = this.tenantGuid,
				callId = this.callId,
				userId = this.userId,
				role = this.role,
				status = this.status,
				joinedDateTime = this.joinedDateTime,
				leftDateTime = this.leftDateTime,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a CallParticipant Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a CallParticipant Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a CallParticipant Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a CallParticipant Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.CallParticipant callParticipant)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (callParticipant == null)
			{
				return null;
			}

			return new {
				id = callParticipant.id,
				callId = callParticipant.callId,
				userId = callParticipant.userId,
				role = callParticipant.role,
				status = callParticipant.status,
				joinedDateTime = callParticipant.joinedDateTime,
				leftDateTime = callParticipant.leftDateTime,
				objectGuid = callParticipant.objectGuid,
				active = callParticipant.active,
				deleted = callParticipant.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a CallParticipant Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(CallParticipant callParticipant)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (callParticipant == null)
			{
				return null;
			}

			return new {
				id = callParticipant.id,
				callId = callParticipant.callId,
				userId = callParticipant.userId,
				role = callParticipant.role,
				status = callParticipant.status,
				joinedDateTime = callParticipant.joinedDateTime,
				leftDateTime = callParticipant.leftDateTime,
				objectGuid = callParticipant.objectGuid,
				active = callParticipant.active,
				deleted = callParticipant.deleted,
				call = Call.CreateMinimalAnonymous(callParticipant.call)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a CallParticipant Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(CallParticipant callParticipant)
		{
			//
			// Return a very minimal object.
			//
			if (callParticipant == null)
			{
				return null;
			}

			return new {
				id = callParticipant.id,
				name = callParticipant.role,
				description = string.Join(", ", new[] { callParticipant.role, callParticipant.status}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
