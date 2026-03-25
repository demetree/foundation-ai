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
	public partial class ConversationMessageChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)conversationMessageId; }
			set { conversationMessageId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ConversationMessageChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 conversationMessageId { get; set; }
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
		public class ConversationMessageChangeHistoryOutputDTO : ConversationMessageChangeHistoryDTO
		{
			public ConversationMessage.ConversationMessageDTO conversationMessage { get; set; }
		}


		/// <summary>
		///
		/// Converts a ConversationMessageChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ConversationMessageChangeHistoryDTO ToDTO()
		{
			return new ConversationMessageChangeHistoryDTO
			{
				id = this.id,
				conversationMessageId = this.conversationMessageId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ConversationMessageChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ConversationMessageChangeHistoryDTO> ToDTOList(List<ConversationMessageChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationMessageChangeHistoryDTO> output = new List<ConversationMessageChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ConversationMessageChangeHistory conversationMessageChangeHistory in data)
			{
				output.Add(conversationMessageChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ConversationMessageChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ConversationMessageChangeHistory Entity type directly.
		///
		/// </summary>
		public ConversationMessageChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ConversationMessageChangeHistoryOutputDTO
			{
				id = this.id,
				conversationMessageId = this.conversationMessageId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				conversationMessage = this.conversationMessage?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ConversationMessageChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ConversationMessageChangeHistory objects to avoid using the ConversationMessageChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ConversationMessageChangeHistoryOutputDTO> ToOutputDTOList(List<ConversationMessageChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationMessageChangeHistoryOutputDTO> output = new List<ConversationMessageChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ConversationMessageChangeHistory conversationMessageChangeHistory in data)
			{
				output.Add(conversationMessageChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ConversationMessageChangeHistory Object.
		///
		/// </summary>
		public static Database.ConversationMessageChangeHistory FromDTO(ConversationMessageChangeHistoryDTO dto)
		{
			return new Database.ConversationMessageChangeHistory
			{
				id = dto.id,
				conversationMessageId = dto.conversationMessageId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ConversationMessageChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ConversationMessageChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.conversationMessageId = dto.conversationMessageId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ConversationMessageChangeHistory Object.
		///
		/// </summary>
		public ConversationMessageChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ConversationMessageChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				conversationMessageId = this.conversationMessageId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationMessageChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationMessageChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ConversationMessageChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationMessageChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ConversationMessageChangeHistory conversationMessageChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (conversationMessageChangeHistory == null)
			{
				return null;
			}

			return new {
				id = conversationMessageChangeHistory.id,
				conversationMessageId = conversationMessageChangeHistory.conversationMessageId,
				versionNumber = conversationMessageChangeHistory.versionNumber,
				timeStamp = conversationMessageChangeHistory.timeStamp,
				userId = conversationMessageChangeHistory.userId,
				data = conversationMessageChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationMessageChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ConversationMessageChangeHistory conversationMessageChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (conversationMessageChangeHistory == null)
			{
				return null;
			}

			return new {
				id = conversationMessageChangeHistory.id,
				conversationMessageId = conversationMessageChangeHistory.conversationMessageId,
				versionNumber = conversationMessageChangeHistory.versionNumber,
				timeStamp = conversationMessageChangeHistory.timeStamp,
				userId = conversationMessageChangeHistory.userId,
				data = conversationMessageChangeHistory.data,
				conversationMessage = ConversationMessage.CreateMinimalAnonymous(conversationMessageChangeHistory.conversationMessage),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ConversationMessageChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ConversationMessageChangeHistory conversationMessageChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (conversationMessageChangeHistory == null)
			{
				return null;
			}

			return new {
				id = conversationMessageChangeHistory.id,
				name = conversationMessageChangeHistory.id,
				description = conversationMessageChangeHistory.id
			 };
		}
	}
}
