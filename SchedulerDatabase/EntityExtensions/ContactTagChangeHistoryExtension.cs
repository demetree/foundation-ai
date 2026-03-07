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
	public partial class ContactTagChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)contactTagId; }
			set { contactTagId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ContactTagChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 contactTagId { get; set; }
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
		public class ContactTagChangeHistoryOutputDTO : ContactTagChangeHistoryDTO
		{
			public ContactTag.ContactTagDTO contactTag { get; set; }
		}


		/// <summary>
		///
		/// Converts a ContactTagChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ContactTagChangeHistoryDTO ToDTO()
		{
			return new ContactTagChangeHistoryDTO
			{
				id = this.id,
				contactTagId = this.contactTagId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ContactTagChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ContactTagChangeHistoryDTO> ToDTOList(List<ContactTagChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContactTagChangeHistoryDTO> output = new List<ContactTagChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ContactTagChangeHistory contactTagChangeHistory in data)
			{
				output.Add(contactTagChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ContactTagChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ContactTagChangeHistoryEntity type directly.
		///
		/// </summary>
		public ContactTagChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ContactTagChangeHistoryOutputDTO
			{
				id = this.id,
				contactTagId = this.contactTagId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				contactTag = this.contactTag?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ContactTagChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ContactTagChangeHistory objects to avoid using the ContactTagChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ContactTagChangeHistoryOutputDTO> ToOutputDTOList(List<ContactTagChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContactTagChangeHistoryOutputDTO> output = new List<ContactTagChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ContactTagChangeHistory contactTagChangeHistory in data)
			{
				output.Add(contactTagChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ContactTagChangeHistory Object.
		///
		/// </summary>
		public static Database.ContactTagChangeHistory FromDTO(ContactTagChangeHistoryDTO dto)
		{
			return new Database.ContactTagChangeHistory
			{
				id = dto.id,
				contactTagId = dto.contactTagId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ContactTagChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ContactTagChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.contactTagId = dto.contactTagId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ContactTagChangeHistory Object.
		///
		/// </summary>
		public ContactTagChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ContactTagChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				contactTagId = this.contactTagId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContactTagChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContactTagChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ContactTagChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ContactTagChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ContactTagChangeHistory contactTagChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (contactTagChangeHistory == null)
			{
				return null;
			}

			return new {
				id = contactTagChangeHistory.id,
				contactTagId = contactTagChangeHistory.contactTagId,
				versionNumber = contactTagChangeHistory.versionNumber,
				timeStamp = contactTagChangeHistory.timeStamp,
				userId = contactTagChangeHistory.userId,
				data = contactTagChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ContactTagChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ContactTagChangeHistory contactTagChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (contactTagChangeHistory == null)
			{
				return null;
			}

			return new {
				id = contactTagChangeHistory.id,
				contactTagId = contactTagChangeHistory.contactTagId,
				versionNumber = contactTagChangeHistory.versionNumber,
				timeStamp = contactTagChangeHistory.timeStamp,
				userId = contactTagChangeHistory.userId,
				data = contactTagChangeHistory.data,
				contactTag = ContactTag.CreateMinimalAnonymous(contactTagChangeHistory.contactTag)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ContactTagChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ContactTagChangeHistory contactTagChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (contactTagChangeHistory == null)
			{
				return null;
			}

			return new {
				id = contactTagChangeHistory.id,
				name = contactTagChangeHistory.id,
				description = contactTagChangeHistory.id
			 };
		}
	}
}
