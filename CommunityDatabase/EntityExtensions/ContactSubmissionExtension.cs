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
			public Int32 Id { get; set; }
			public String Name { get; set; }
			public String Email { get; set; }
			public String Subject { get; set; }
			public String Message { get; set; }
			public DateTime SubmittedDate { get; set; }
			public Boolean IsRead { get; set; }
			public Boolean IsArchived { get; set; }
			public String AdminNotes { get; set; }
			public Guid ObjectGuid { get; set; }
			public Boolean? Active { get; set; }
			public Boolean? Deleted { get; set; }
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
				Id = this.Id,
				Name = this.Name,
				Email = this.Email,
				Subject = this.Subject,
				Message = this.Message,
				SubmittedDate = this.SubmittedDate,
				IsRead = this.IsRead,
				IsArchived = this.IsArchived,
				AdminNotes = this.AdminNotes,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
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
				Id = this.Id,
				Name = this.Name,
				Email = this.Email,
				Subject = this.Subject,
				Message = this.Message,
				SubmittedDate = this.SubmittedDate,
				IsRead = this.IsRead,
				IsArchived = this.IsArchived,
				AdminNotes = this.AdminNotes,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
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
				Id = dto.Id,
				Name = dto.Name,
				Email = dto.Email,
				Subject = dto.Subject,
				Message = dto.Message,
				SubmittedDate = dto.SubmittedDate,
				IsRead = dto.IsRead,
				IsArchived = dto.IsArchived,
				AdminNotes = dto.AdminNotes,
				ObjectGuid = dto.ObjectGuid,
				Active = dto.Active ?? true,
				Deleted = dto.Deleted ?? false
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

			this.Name = dto.Name;
			this.Email = dto.Email;
			this.Subject = dto.Subject;
			this.Message = dto.Message;
			this.SubmittedDate = dto.SubmittedDate;
			this.IsRead = dto.IsRead;
			this.IsArchived = dto.IsArchived;
			this.AdminNotes = dto.AdminNotes;
			this.ObjectGuid = dto.ObjectGuid;
			if (dto.Active.HasValue == true)
			{
				this.Active = dto.Active.Value;
			}
			if (dto.Deleted.HasValue == true)
			{
				this.Deleted = dto.Deleted.Value;
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
				Id = this.Id,
				Name = this.Name,
				Email = this.Email,
				Subject = this.Subject,
				Message = this.Message,
				SubmittedDate = this.SubmittedDate,
				IsRead = this.IsRead,
				IsArchived = this.IsArchived,
				AdminNotes = this.AdminNotes,
				ObjectGuid = this.ObjectGuid,
				Active = this.Active,
				Deleted = this.Deleted
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
				Id = contactSubmission.Id,
				Name = contactSubmission.Name,
				Email = contactSubmission.Email,
				Subject = contactSubmission.Subject,
				Message = contactSubmission.Message,
				SubmittedDate = contactSubmission.SubmittedDate,
				IsRead = contactSubmission.IsRead,
				IsArchived = contactSubmission.IsArchived,
				AdminNotes = contactSubmission.AdminNotes,
				ObjectGuid = contactSubmission.ObjectGuid,
				Active = contactSubmission.Active,
				Deleted = contactSubmission.Deleted
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
				Id = contactSubmission.Id,
				Name = contactSubmission.Name,
				Email = contactSubmission.Email,
				Subject = contactSubmission.Subject,
				Message = contactSubmission.Message,
				SubmittedDate = contactSubmission.SubmittedDate,
				IsRead = contactSubmission.IsRead,
				IsArchived = contactSubmission.IsArchived,
				AdminNotes = contactSubmission.AdminNotes,
				ObjectGuid = contactSubmission.ObjectGuid,
				Active = contactSubmission.Active,
				Deleted = contactSubmission.Deleted
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
				name = contactSubmission.name,
				description = string.Join(", ", new[] { contactSubmission.name, contactSubmission.email, contactSubmission.subject}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
