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
	public partial class ContactInteractionChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)contactInteractionId; }
			set { contactInteractionId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ContactInteractionChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 contactInteractionId { get; set; }
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
		public class ContactInteractionChangeHistoryOutputDTO : ContactInteractionChangeHistoryDTO
		{
			public ContactInteraction.ContactInteractionDTO contactInteraction { get; set; }
		}


		/// <summary>
		///
		/// Converts a ContactInteractionChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ContactInteractionChangeHistoryDTO ToDTO()
		{
			return new ContactInteractionChangeHistoryDTO
			{
				id = this.id,
				contactInteractionId = this.contactInteractionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ContactInteractionChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ContactInteractionChangeHistoryDTO> ToDTOList(List<ContactInteractionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContactInteractionChangeHistoryDTO> output = new List<ContactInteractionChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ContactInteractionChangeHistory contactInteractionChangeHistory in data)
			{
				output.Add(contactInteractionChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ContactInteractionChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ContactInteractionChangeHistory Entity type directly.
		///
		/// </summary>
		public ContactInteractionChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ContactInteractionChangeHistoryOutputDTO
			{
				id = this.id,
				contactInteractionId = this.contactInteractionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				contactInteraction = this.contactInteraction?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ContactInteractionChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ContactInteractionChangeHistory objects to avoid using the ContactInteractionChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ContactInteractionChangeHistoryOutputDTO> ToOutputDTOList(List<ContactInteractionChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContactInteractionChangeHistoryOutputDTO> output = new List<ContactInteractionChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ContactInteractionChangeHistory contactInteractionChangeHistory in data)
			{
				output.Add(contactInteractionChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ContactInteractionChangeHistory Object.
		///
		/// </summary>
		public static Database.ContactInteractionChangeHistory FromDTO(ContactInteractionChangeHistoryDTO dto)
		{
			return new Database.ContactInteractionChangeHistory
			{
				id = dto.id,
				contactInteractionId = dto.contactInteractionId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ContactInteractionChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ContactInteractionChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.contactInteractionId = dto.contactInteractionId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ContactInteractionChangeHistory Object.
		///
		/// </summary>
		public ContactInteractionChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ContactInteractionChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				contactInteractionId = this.contactInteractionId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContactInteractionChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContactInteractionChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ContactInteractionChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ContactInteractionChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ContactInteractionChangeHistory contactInteractionChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (contactInteractionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = contactInteractionChangeHistory.id,
				contactInteractionId = contactInteractionChangeHistory.contactInteractionId,
				versionNumber = contactInteractionChangeHistory.versionNumber,
				timeStamp = contactInteractionChangeHistory.timeStamp,
				userId = contactInteractionChangeHistory.userId,
				data = contactInteractionChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ContactInteractionChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ContactInteractionChangeHistory contactInteractionChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (contactInteractionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = contactInteractionChangeHistory.id,
				contactInteractionId = contactInteractionChangeHistory.contactInteractionId,
				versionNumber = contactInteractionChangeHistory.versionNumber,
				timeStamp = contactInteractionChangeHistory.timeStamp,
				userId = contactInteractionChangeHistory.userId,
				data = contactInteractionChangeHistory.data,
				contactInteraction = ContactInteraction.CreateMinimalAnonymous(contactInteractionChangeHistory.contactInteraction)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ContactInteractionChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ContactInteractionChangeHistory contactInteractionChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (contactInteractionChangeHistory == null)
			{
				return null;
			}

			return new {
				id = contactInteractionChangeHistory.id,
				name = contactInteractionChangeHistory.id,
				description = contactInteractionChangeHistory.id
			 };
		}
	}
}
