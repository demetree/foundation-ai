using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Community.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class ContactSubmission : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ContactSubmissionDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String email { get; set; }
			public String subject { get; set; }
			[Required]
			public String message { get; set; }
			[Required]
			public DateTime submittedDate { get; set; }
			[Required]
			public Boolean isRead { get; set; }
			[Required]
			public Boolean isArchived { get; set; }
			public String adminNotes { get; set; }
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
		public class ContactSubmissionOutputDTO : ContactSubmissionDTO
		{
		}


		/// <summary>
		///
		/// Converts a ContactSubmission to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ContactSubmissionDTO ToDTO()
		{
			return new ContactSubmissionDTO
			{
				id = this.id,
				name = this.name,
				email = this.email,
				subject = this.subject,
				message = this.message,
				submittedDate = this.submittedDate,
				isRead = this.isRead,
				isArchived = this.isArchived,
				adminNotes = this.adminNotes,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ContactSubmission list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ContactSubmissionDTO> ToDTOList(List<ContactSubmission> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContactSubmissionDTO> output = new List<ContactSubmissionDTO>();

			output.Capacity = data.Count;

			foreach (ContactSubmission contactSubmission in data)
			{
				output.Add(contactSubmission.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ContactSubmission to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ContactSubmissionEntity type directly.
		///
		/// </summary>
		public ContactSubmissionOutputDTO ToOutputDTO()
		{
			return new ContactSubmissionOutputDTO
			{
				id = this.id,
				name = this.name,
				email = this.email,
				subject = this.subject,
				message = this.message,
				submittedDate = this.submittedDate,
				isRead = this.isRead,
				isArchived = this.isArchived,
				adminNotes = this.adminNotes,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ContactSubmission list to list of Output Data Transfer Object intended to be used for serializing a list of ContactSubmission objects to avoid using the ContactSubmission entity type directly.
		///
		/// </summary>
		public static List<ContactSubmissionOutputDTO> ToOutputDTOList(List<ContactSubmission> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ContactSubmissionOutputDTO> output = new List<ContactSubmissionOutputDTO>();

			output.Capacity = data.Count;

			foreach (ContactSubmission contactSubmission in data)
			{
				output.Add(contactSubmission.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ContactSubmission Object.
		///
		/// </summary>
		public static Database.ContactSubmission FromDTO(ContactSubmissionDTO dto)
		{
			return new Database.ContactSubmission
			{
				id = dto.id,
				name = dto.name,
				email = dto.email,
				subject = dto.subject,
				message = dto.message,
				submittedDate = dto.submittedDate,
				isRead = dto.isRead,
				isArchived = dto.isArchived,
				adminNotes = dto.adminNotes,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ContactSubmission Object.
		///
		/// </summary>
		public void ApplyDTO(ContactSubmissionDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.email = dto.email;
			this.subject = dto.subject;
			this.message = dto.message;
			this.submittedDate = dto.submittedDate;
			this.isRead = dto.isRead;
			this.isArchived = dto.isArchived;
			this.adminNotes = dto.adminNotes;
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
		/// Creates a deep copy clone of a ContactSubmission Object.
		///
		/// </summary>
		public ContactSubmission Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ContactSubmission{
				id = this.id,
				tenantGuid = this.tenantGuid,
				name = this.name,
				email = this.email,
				subject = this.subject,
				message = this.message,
				submittedDate = this.submittedDate,
				isRead = this.isRead,
				isArchived = this.isArchived,
				adminNotes = this.adminNotes,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContactSubmission Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ContactSubmission Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ContactSubmission Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ContactSubmission Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ContactSubmission contactSubmission)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (contactSubmission == null)
			{
				return null;
			}

			return new {
				id = contactSubmission.id,
				name = contactSubmission.name,
				email = contactSubmission.email,
				subject = contactSubmission.subject,
				message = contactSubmission.message,
				submittedDate = contactSubmission.submittedDate,
				isRead = contactSubmission.isRead,
				isArchived = contactSubmission.isArchived,
				adminNotes = contactSubmission.adminNotes,
				objectGuid = contactSubmission.objectGuid,
				active = contactSubmission.active,
				deleted = contactSubmission.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ContactSubmission Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ContactSubmission contactSubmission)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (contactSubmission == null)
			{
				return null;
			}

			return new {
				id = contactSubmission.id,
				name = contactSubmission.name,
				email = contactSubmission.email,
				subject = contactSubmission.subject,
				message = contactSubmission.message,
				submittedDate = contactSubmission.submittedDate,
				isRead = contactSubmission.isRead,
				isArchived = contactSubmission.isArchived,
				adminNotes = contactSubmission.adminNotes,
				objectGuid = contactSubmission.objectGuid,
				active = contactSubmission.active,
				deleted = contactSubmission.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ContactSubmission Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ContactSubmission contactSubmission)
		{
			//
			// Return a very minimal object.
			//
			if (contactSubmission == null)
			{
				return null;
			}

			return new {
				id = contactSubmission.id,
				name = contactSubmission.name,
				description = string.Join(", ", new[] { contactSubmission.name, contactSubmission.email, contactSubmission.subject}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
