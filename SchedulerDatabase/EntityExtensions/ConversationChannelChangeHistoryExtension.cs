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
	public partial class ConversationChannelChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)conversationChannelId; }
			set { conversationChannelId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ConversationChannelChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 conversationChannelId { get; set; }
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
		public class ConversationChannelChangeHistoryOutputDTO : ConversationChannelChangeHistoryDTO
		{
			public ConversationChannel.ConversationChannelDTO conversationChannel { get; set; }
		}


		/// <summary>
		///
		/// Converts a ConversationChannelChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ConversationChannelChangeHistoryDTO ToDTO()
		{
			return new ConversationChannelChangeHistoryDTO
			{
				id = this.id,
				conversationChannelId = this.conversationChannelId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ConversationChannelChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ConversationChannelChangeHistoryDTO> ToDTOList(List<ConversationChannelChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationChannelChangeHistoryDTO> output = new List<ConversationChannelChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ConversationChannelChangeHistory conversationChannelChangeHistory in data)
			{
				output.Add(conversationChannelChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ConversationChannelChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ConversationChannelChangeHistory Entity type directly.
		///
		/// </summary>
		public ConversationChannelChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ConversationChannelChangeHistoryOutputDTO
			{
				id = this.id,
				conversationChannelId = this.conversationChannelId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				conversationChannel = this.conversationChannel?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ConversationChannelChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ConversationChannelChangeHistory objects to avoid using the ConversationChannelChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ConversationChannelChangeHistoryOutputDTO> ToOutputDTOList(List<ConversationChannelChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConversationChannelChangeHistoryOutputDTO> output = new List<ConversationChannelChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ConversationChannelChangeHistory conversationChannelChangeHistory in data)
			{
				output.Add(conversationChannelChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ConversationChannelChangeHistory Object.
		///
		/// </summary>
		public static Database.ConversationChannelChangeHistory FromDTO(ConversationChannelChangeHistoryDTO dto)
		{
			return new Database.ConversationChannelChangeHistory
			{
				id = dto.id,
				conversationChannelId = dto.conversationChannelId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ConversationChannelChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ConversationChannelChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.conversationChannelId = dto.conversationChannelId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ConversationChannelChangeHistory Object.
		///
		/// </summary>
		public ConversationChannelChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ConversationChannelChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				conversationChannelId = this.conversationChannelId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationChannelChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConversationChannelChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ConversationChannelChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationChannelChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ConversationChannelChangeHistory conversationChannelChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (conversationChannelChangeHistory == null)
			{
				return null;
			}

			return new {
				id = conversationChannelChangeHistory.id,
				conversationChannelId = conversationChannelChangeHistory.conversationChannelId,
				versionNumber = conversationChannelChangeHistory.versionNumber,
				timeStamp = conversationChannelChangeHistory.timeStamp,
				userId = conversationChannelChangeHistory.userId,
				data = conversationChannelChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ConversationChannelChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ConversationChannelChangeHistory conversationChannelChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (conversationChannelChangeHistory == null)
			{
				return null;
			}

			return new {
				id = conversationChannelChangeHistory.id,
				conversationChannelId = conversationChannelChangeHistory.conversationChannelId,
				versionNumber = conversationChannelChangeHistory.versionNumber,
				timeStamp = conversationChannelChangeHistory.timeStamp,
				userId = conversationChannelChangeHistory.userId,
				data = conversationChannelChangeHistory.data,
				conversationChannel = ConversationChannel.CreateMinimalAnonymous(conversationChannelChangeHistory.conversationChannel)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ConversationChannelChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ConversationChannelChangeHistory conversationChannelChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (conversationChannelChangeHistory == null)
			{
				return null;
			}

			return new {
				id = conversationChannelChangeHistory.id,
				name = conversationChannelChangeHistory.id,
				description = conversationChannelChangeHistory.id
			 };
		}
	}
}
