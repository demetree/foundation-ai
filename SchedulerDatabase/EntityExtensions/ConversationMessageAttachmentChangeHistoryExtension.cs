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
	public partial class ConversationMessageAttachmentChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)conversationMessageAttachmentId; }
			set { conversationMessageAttachmentId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ConversationMessageAttachmentChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 conversationMessageAttachmentId { get; set; }
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
		public class ConversationMessageAttachmentChangeHistoryOutputDTO : ConversationMessageAttachmentChangeHistoryDTO
		{
			public ConversationMessageAttachment.ConversationMessageAttachmentDTO conversationMessageAttachment { get; set; }
		}


		/// <summary>
		///
		/// Converts a ConversationMessageAttachmentChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ConversationMessageAttachmentChangeHistoryDTO ToDTO()
		{
			return new ConversationMessageAttachmentChangeHistoryDTO
			{
				id = this.id,
				conversationMessageAttachmentId = this.conversationMessageAttachmentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ConversationMessageAttachmentChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ConversationMessageAttachmentChangeHistoryDTO> ToDTOList(List<ConversationMessageAttachmentChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationMessageAttachmentChangeHistoryDTO> output = new List<ConversationMessageAttachmentChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ConversationMessageAttachmentChangeHistory conversationMessageAttachmentChangeHistory in data)
			{
				output.Add(conversationMessageAttachmentChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ConversationMessageAttachmentChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ConversationMessageAttachmentChangeHistory Entity type directly.
		///
		/// </summary>
		public ConversationMessageAttachmentChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ConversationMessageAttachmentChangeHistoryOutputDTO
			{
				id = this.id,
				conversationMessageAttachmentId = this.conversationMessageAttachmentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				conversationMessageAttachment = this.conversationMessageAttachment?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ConversationMessageAttachmentChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ConversationMessageAttachmentChangeHistory objects to avoid using the ConversationMessageAttachmentChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ConversationMessageAttachmentChangeHistoryOutputDTO> ToOutputDTOList(List<ConversationMessageAttachmentChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationMessageAttachmentChangeHistoryOutputDTO> output = new List<ConversationMessageAttachmentChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ConversationMessageAttachmentChangeHistory conversationMessageAttachmentChangeHistory in data)
			{
				output.Add(conversationMessageAttachmentChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ConversationMessageAttachmentChangeHistory Object.
		///
		/// </summary>
		public static Database.ConversationMessageAttachmentChangeHistory FromDTO(ConversationMessageAttachmentChangeHistoryDTO dto)
		{
			return new Database.ConversationMessageAttachmentChangeHistory
			{
				id = dto.id,
				conversationMessageAttachmentId = dto.conversationMessageAttachmentId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ConversationMessageAttachmentChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ConversationMessageAttachmentChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.conversationMessageAttachmentId = dto.conversationMessageAttachmentId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ConversationMessageAttachmentChangeHistory Object.
		///
		/// </summary>
		public ConversationMessageAttachmentChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ConversationMessageAttachmentChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				conversationMessageAttachmentId = this.conversationMessageAttachmentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationMessageAttachmentChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationMessageAttachmentChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ConversationMessageAttachmentChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationMessageAttachmentChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ConversationMessageAttachmentChangeHistory conversationMessageAttachmentChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (conversationMessageAttachmentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = conversationMessageAttachmentChangeHistory.id,
				conversationMessageAttachmentId = conversationMessageAttachmentChangeHistory.conversationMessageAttachmentId,
				versionNumber = conversationMessageAttachmentChangeHistory.versionNumber,
				timeStamp = conversationMessageAttachmentChangeHistory.timeStamp,
				userId = conversationMessageAttachmentChangeHistory.userId,
				data = conversationMessageAttachmentChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationMessageAttachmentChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ConversationMessageAttachmentChangeHistory conversationMessageAttachmentChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (conversationMessageAttachmentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = conversationMessageAttachmentChangeHistory.id,
				conversationMessageAttachmentId = conversationMessageAttachmentChangeHistory.conversationMessageAttachmentId,
				versionNumber = conversationMessageAttachmentChangeHistory.versionNumber,
				timeStamp = conversationMessageAttachmentChangeHistory.timeStamp,
				userId = conversationMessageAttachmentChangeHistory.userId,
				data = conversationMessageAttachmentChangeHistory.data,
				conversationMessageAttachment = ConversationMessageAttachment.CreateMinimalAnonymous(conversationMessageAttachmentChangeHistory.conversationMessageAttachment)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ConversationMessageAttachmentChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ConversationMessageAttachmentChangeHistory conversationMessageAttachmentChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (conversationMessageAttachmentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = conversationMessageAttachmentChangeHistory.id,
				name = conversationMessageAttachmentChangeHistory.id,
				description = conversationMessageAttachmentChangeHistory.id
			 };
		}
	}
}
