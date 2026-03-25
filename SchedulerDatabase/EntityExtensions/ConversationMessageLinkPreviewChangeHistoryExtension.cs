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
	public partial class ConversationMessageLinkPreviewChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)conversationMessageLinkPreviewId; }
			set { conversationMessageLinkPreviewId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ConversationMessageLinkPreviewChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 conversationMessageLinkPreviewId { get; set; }
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
		public class ConversationMessageLinkPreviewChangeHistoryOutputDTO : ConversationMessageLinkPreviewChangeHistoryDTO
		{
			public ConversationMessageLinkPreview.ConversationMessageLinkPreviewDTO conversationMessageLinkPreview { get; set; }
		}


		/// <summary>
		///
		/// Converts a ConversationMessageLinkPreviewChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ConversationMessageLinkPreviewChangeHistoryDTO ToDTO()
		{
			return new ConversationMessageLinkPreviewChangeHistoryDTO
			{
				id = this.id,
				conversationMessageLinkPreviewId = this.conversationMessageLinkPreviewId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ConversationMessageLinkPreviewChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ConversationMessageLinkPreviewChangeHistoryDTO> ToDTOList(List<ConversationMessageLinkPreviewChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationMessageLinkPreviewChangeHistoryDTO> output = new List<ConversationMessageLinkPreviewChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ConversationMessageLinkPreviewChangeHistory conversationMessageLinkPreviewChangeHistory in data)
			{
				output.Add(conversationMessageLinkPreviewChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ConversationMessageLinkPreviewChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ConversationMessageLinkPreviewChangeHistory Entity type directly.
		///
		/// </summary>
		public ConversationMessageLinkPreviewChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ConversationMessageLinkPreviewChangeHistoryOutputDTO
			{
				id = this.id,
				conversationMessageLinkPreviewId = this.conversationMessageLinkPreviewId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				conversationMessageLinkPreview = this.conversationMessageLinkPreview?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ConversationMessageLinkPreviewChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ConversationMessageLinkPreviewChangeHistory objects to avoid using the ConversationMessageLinkPreviewChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ConversationMessageLinkPreviewChangeHistoryOutputDTO> ToOutputDTOList(List<ConversationMessageLinkPreviewChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationMessageLinkPreviewChangeHistoryOutputDTO> output = new List<ConversationMessageLinkPreviewChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ConversationMessageLinkPreviewChangeHistory conversationMessageLinkPreviewChangeHistory in data)
			{
				output.Add(conversationMessageLinkPreviewChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ConversationMessageLinkPreviewChangeHistory Object.
		///
		/// </summary>
		public static Database.ConversationMessageLinkPreviewChangeHistory FromDTO(ConversationMessageLinkPreviewChangeHistoryDTO dto)
		{
			return new Database.ConversationMessageLinkPreviewChangeHistory
			{
				id = dto.id,
				conversationMessageLinkPreviewId = dto.conversationMessageLinkPreviewId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ConversationMessageLinkPreviewChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ConversationMessageLinkPreviewChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.conversationMessageLinkPreviewId = dto.conversationMessageLinkPreviewId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ConversationMessageLinkPreviewChangeHistory Object.
		///
		/// </summary>
		public ConversationMessageLinkPreviewChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ConversationMessageLinkPreviewChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				conversationMessageLinkPreviewId = this.conversationMessageLinkPreviewId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationMessageLinkPreviewChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationMessageLinkPreviewChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ConversationMessageLinkPreviewChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationMessageLinkPreviewChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ConversationMessageLinkPreviewChangeHistory conversationMessageLinkPreviewChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (conversationMessageLinkPreviewChangeHistory == null)
			{
				return null;
			}

			return new {
				id = conversationMessageLinkPreviewChangeHistory.id,
				conversationMessageLinkPreviewId = conversationMessageLinkPreviewChangeHistory.conversationMessageLinkPreviewId,
				versionNumber = conversationMessageLinkPreviewChangeHistory.versionNumber,
				timeStamp = conversationMessageLinkPreviewChangeHistory.timeStamp,
				userId = conversationMessageLinkPreviewChangeHistory.userId,
				data = conversationMessageLinkPreviewChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationMessageLinkPreviewChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ConversationMessageLinkPreviewChangeHistory conversationMessageLinkPreviewChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (conversationMessageLinkPreviewChangeHistory == null)
			{
				return null;
			}

			return new {
				id = conversationMessageLinkPreviewChangeHistory.id,
				conversationMessageLinkPreviewId = conversationMessageLinkPreviewChangeHistory.conversationMessageLinkPreviewId,
				versionNumber = conversationMessageLinkPreviewChangeHistory.versionNumber,
				timeStamp = conversationMessageLinkPreviewChangeHistory.timeStamp,
				userId = conversationMessageLinkPreviewChangeHistory.userId,
				data = conversationMessageLinkPreviewChangeHistory.data,
				conversationMessageLinkPreview = ConversationMessageLinkPreview.CreateMinimalAnonymous(conversationMessageLinkPreviewChangeHistory.conversationMessageLinkPreview),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ConversationMessageLinkPreviewChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ConversationMessageLinkPreviewChangeHistory conversationMessageLinkPreviewChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (conversationMessageLinkPreviewChangeHistory == null)
			{
				return null;
			}

			return new {
				id = conversationMessageLinkPreviewChangeHistory.id,
				name = conversationMessageLinkPreviewChangeHistory.id,
				description = conversationMessageLinkPreviewChangeHistory.id
			 };
		}
	}
}
