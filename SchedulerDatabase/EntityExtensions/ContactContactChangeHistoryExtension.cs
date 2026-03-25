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
	public partial class ContactContactChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)contactContactId; }
			set { contactContactId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ContactContactChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 contactContactId { get; set; }
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
		public class ContactContactChangeHistoryOutputDTO : ContactContactChangeHistoryDTO
		{
			public ContactContact.ContactContactDTO contactContact { get; set; }
		}


		/// <summary>
		///
		/// Converts a ContactContactChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ContactContactChangeHistoryDTO ToDTO()
		{
			return new ContactContactChangeHistoryDTO
			{
				id = this.id,
				contactContactId = this.contactContactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ContactContactChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ContactContactChangeHistoryDTO> ToDTOList(List<ContactContactChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContactContactChangeHistoryDTO> output = new List<ContactContactChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ContactContactChangeHistory contactContactChangeHistory in data)
			{
				output.Add(contactContactChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ContactContactChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ContactContactChangeHistory Entity type directly.
		///
		/// </summary>
		public ContactContactChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ContactContactChangeHistoryOutputDTO
			{
				id = this.id,
				contactContactId = this.contactContactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				contactContact = this.contactContact?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ContactContactChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ContactContactChangeHistory objects to avoid using the ContactContactChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ContactContactChangeHistoryOutputDTO> ToOutputDTOList(List<ContactContactChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContactContactChangeHistoryOutputDTO> output = new List<ContactContactChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ContactContactChangeHistory contactContactChangeHistory in data)
			{
				output.Add(contactContactChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ContactContactChangeHistory Object.
		///
		/// </summary>
		public static Database.ContactContactChangeHistory FromDTO(ContactContactChangeHistoryDTO dto)
		{
			return new Database.ContactContactChangeHistory
			{
				id = dto.id,
				contactContactId = dto.contactContactId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ContactContactChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ContactContactChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.contactContactId = dto.contactContactId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ContactContactChangeHistory Object.
		///
		/// </summary>
		public ContactContactChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ContactContactChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				contactContactId = this.contactContactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContactContactChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContactContactChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ContactContactChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ContactContactChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ContactContactChangeHistory contactContactChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (contactContactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = contactContactChangeHistory.id,
				contactContactId = contactContactChangeHistory.contactContactId,
				versionNumber = contactContactChangeHistory.versionNumber,
				timeStamp = contactContactChangeHistory.timeStamp,
				userId = contactContactChangeHistory.userId,
				data = contactContactChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ContactContactChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ContactContactChangeHistory contactContactChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (contactContactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = contactContactChangeHistory.id,
				contactContactId = contactContactChangeHistory.contactContactId,
				versionNumber = contactContactChangeHistory.versionNumber,
				timeStamp = contactContactChangeHistory.timeStamp,
				userId = contactContactChangeHistory.userId,
				data = contactContactChangeHistory.data,
				contactContact = ContactContact.CreateMinimalAnonymous(contactContactChangeHistory.contactContact),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ContactContactChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ContactContactChangeHistory contactContactChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (contactContactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = contactContactChangeHistory.id,
				name = contactContactChangeHistory.id,
				description = contactContactChangeHistory.id
			 };
		}
	}
}
