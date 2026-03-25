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
	public partial class ContactChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)contactId; }
			set { contactId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ContactChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 contactId { get; set; }
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
		public class ContactChangeHistoryOutputDTO : ContactChangeHistoryDTO
		{
			public Contact.ContactDTO contact { get; set; }
		}


		/// <summary>
		///
		/// Converts a ContactChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ContactChangeHistoryDTO ToDTO()
		{
			return new ContactChangeHistoryDTO
			{
				id = this.id,
				contactId = this.contactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ContactChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ContactChangeHistoryDTO> ToDTOList(List<ContactChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContactChangeHistoryDTO> output = new List<ContactChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ContactChangeHistory contactChangeHistory in data)
			{
				output.Add(contactChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ContactChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ContactChangeHistory Entity type directly.
		///
		/// </summary>
		public ContactChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ContactChangeHistoryOutputDTO
			{
				id = this.id,
				contactId = this.contactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				contact = this.contact?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ContactChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ContactChangeHistory objects to avoid using the ContactChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ContactChangeHistoryOutputDTO> ToOutputDTOList(List<ContactChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContactChangeHistoryOutputDTO> output = new List<ContactChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ContactChangeHistory contactChangeHistory in data)
			{
				output.Add(contactChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ContactChangeHistory Object.
		///
		/// </summary>
		public static Database.ContactChangeHistory FromDTO(ContactChangeHistoryDTO dto)
		{
			return new Database.ContactChangeHistory
			{
				id = dto.id,
				contactId = dto.contactId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ContactChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ContactChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.contactId = dto.contactId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ContactChangeHistory Object.
		///
		/// </summary>
		public ContactChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ContactChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				contactId = this.contactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContactChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContactChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ContactChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ContactChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ContactChangeHistory contactChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (contactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = contactChangeHistory.id,
				contactId = contactChangeHistory.contactId,
				versionNumber = contactChangeHistory.versionNumber,
				timeStamp = contactChangeHistory.timeStamp,
				userId = contactChangeHistory.userId,
				data = contactChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ContactChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ContactChangeHistory contactChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (contactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = contactChangeHistory.id,
				contactId = contactChangeHistory.contactId,
				versionNumber = contactChangeHistory.versionNumber,
				timeStamp = contactChangeHistory.timeStamp,
				userId = contactChangeHistory.userId,
				data = contactChangeHistory.data,
				contact = Contact.CreateMinimalAnonymous(contactChangeHistory.contact),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ContactChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ContactChangeHistory contactChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (contactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = contactChangeHistory.id,
				name = contactChangeHistory.id,
				description = contactChangeHistory.id
			 };
		}
	}
}
