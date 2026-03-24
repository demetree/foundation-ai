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
	public partial class Call : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class CallDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 callTypeId { get; set; }
			[Required]
			public Int32 callStatusId { get; set; }
			[Required]
			public String providerId { get; set; }
			public String providerCallId { get; set; }
			[Required]
			public Int32 conversationId { get; set; }
			[Required]
			public Int32 initiatorUserId { get; set; }
			[Required]
			public DateTime startDateTime { get; set; }
			public DateTime? answerDateTime { get; set; }
			public DateTime? endDateTime { get; set; }
			public Int32? durationSeconds { get; set; }
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
		public class CallOutputDTO : CallDTO
		{
			public CallStatus.CallStatusDTO callStatus { get; set; }
			public CallType.CallTypeDTO callType { get; set; }
			public Conversation.ConversationDTO conversation { get; set; }
		}


		/// <summary>
		///
		/// Converts a Call to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public CallDTO ToDTO()
		{
			return new CallDTO
			{
				id = this.id,
				callTypeId = this.callTypeId,
				callStatusId = this.callStatusId,
				providerId = this.providerId,
				providerCallId = this.providerCallId,
				conversationId = this.conversationId,
				initiatorUserId = this.initiatorUserId,
				startDateTime = this.startDateTime,
				answerDateTime = this.answerDateTime,
				endDateTime = this.endDateTime,
				durationSeconds = this.durationSeconds,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a Call list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<CallDTO> ToDTOList(List<Call> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CallDTO> output = new List<CallDTO>();

			output.Capacity = data.Count;

			foreach (Call call in data)
			{
				output.Add(call.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a Call to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the Call Entity type directly.
		///
		/// </summary>
		public CallOutputDTO ToOutputDTO()
		{
			return new CallOutputDTO
			{
				id = this.id,
				callTypeId = this.callTypeId,
				callStatusId = this.callStatusId,
				providerId = this.providerId,
				providerCallId = this.providerCallId,
				conversationId = this.conversationId,
				initiatorUserId = this.initiatorUserId,
				startDateTime = this.startDateTime,
				answerDateTime = this.answerDateTime,
				endDateTime = this.endDateTime,
				durationSeconds = this.durationSeconds,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				callStatus = this.callStatus?.ToDTO(),
				callType = this.callType?.ToDTO(),
				conversation = this.conversation?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a Call list to list of Output Data Transfer Object intended to be used for serializing a list of Call objects to avoid using the Call entity type directly.
		///
		/// </summary>
		public static List<CallOutputDTO> ToOutputDTOList(List<Call> data)
		{
			if (data == null)
			{
				return null;
			}

			List<CallOutputDTO> output = new List<CallOutputDTO>();

			output.Capacity = data.Count;

			foreach (Call call in data)
			{
				output.Add(call.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a Call Object.
		///
		/// </summary>
		public static Database.Call FromDTO(CallDTO dto)
		{
			return new Database.Call
			{
				id = dto.id,
				callTypeId = dto.callTypeId,
				callStatusId = dto.callStatusId,
				providerId = dto.providerId,
				providerCallId = dto.providerCallId,
				conversationId = dto.conversationId,
				initiatorUserId = dto.initiatorUserId,
				startDateTime = dto.startDateTime,
				answerDateTime = dto.answerDateTime,
				endDateTime = dto.endDateTime,
				durationSeconds = dto.durationSeconds,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a Call Object.
		///
		/// </summary>
		public void ApplyDTO(CallDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.callTypeId = dto.callTypeId;
			this.callStatusId = dto.callStatusId;
			this.providerId = dto.providerId;
			this.providerCallId = dto.providerCallId;
			this.conversationId = dto.conversationId;
			this.initiatorUserId = dto.initiatorUserId;
			this.startDateTime = dto.startDateTime;
			this.answerDateTime = dto.answerDateTime;
			this.endDateTime = dto.endDateTime;
			this.durationSeconds = dto.durationSeconds;
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
		/// Creates a deep copy clone of a Call Object.
		///
		/// </summary>
		public Call Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new Call{
				id = this.id,
				tenantGuid = this.tenantGuid,
				callTypeId = this.callTypeId,
				callStatusId = this.callStatusId,
				providerId = this.providerId,
				providerCallId = this.providerCallId,
				conversationId = this.conversationId,
				initiatorUserId = this.initiatorUserId,
				startDateTime = this.startDateTime,
				answerDateTime = this.answerDateTime,
				endDateTime = this.endDateTime,
				durationSeconds = this.durationSeconds,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Call Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a Call Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a Call Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a Call Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.Call call)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (call == null)
			{
				return null;
			}

			return new {
				id = call.id,
				callTypeId = call.callTypeId,
				callStatusId = call.callStatusId,
				providerId = call.providerId,
				providerCallId = call.providerCallId,
				conversationId = call.conversationId,
				initiatorUserId = call.initiatorUserId,
				startDateTime = call.startDateTime,
				answerDateTime = call.answerDateTime,
				endDateTime = call.endDateTime,
				durationSeconds = call.durationSeconds,
				objectGuid = call.objectGuid,
				active = call.active,
				deleted = call.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a Call Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(Call call)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (call == null)
			{
				return null;
			}

			return new {
				id = call.id,
				callTypeId = call.callTypeId,
				callStatusId = call.callStatusId,
				providerId = call.providerId,
				providerCallId = call.providerCallId,
				conversationId = call.conversationId,
				initiatorUserId = call.initiatorUserId,
				startDateTime = call.startDateTime,
				answerDateTime = call.answerDateTime,
				endDateTime = call.endDateTime,
				durationSeconds = call.durationSeconds,
				objectGuid = call.objectGuid,
				active = call.active,
				deleted = call.deleted,
				callStatus = CallStatus.CreateMinimalAnonymous(call.callStatus),
				callType = CallType.CreateMinimalAnonymous(call.callType),
				conversation = Conversation.CreateMinimalAnonymous(call.conversation)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a Call Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(Call call)
		{
			//
			// Return a very minimal object.
			//
			if (call == null)
			{
				return null;
			}

			return new {
				id = call.id,
				name = call.providerId,
				description = string.Join(", ", new[] { call.providerId, call.providerCallId}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
