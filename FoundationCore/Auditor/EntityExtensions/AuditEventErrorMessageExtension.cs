using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Auditor.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class AuditEventErrorMessage : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AuditEventErrorMessageDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 auditEventId { get; set; }
			[Required]
			public String errorMessage { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class AuditEventErrorMessageOutputDTO : AuditEventErrorMessageDTO
		{
			public AuditEvent.AuditEventDTO auditEvent { get; set; }
		}


		/// <summary>
		///
		/// Converts a AuditEventErrorMessage to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AuditEventErrorMessageDTO ToDTO()
		{
			return new AuditEventErrorMessageDTO
			{
				id = this.id,
				auditEventId = this.auditEventId,
				errorMessage = this.errorMessage
			};
		}


		/// <summary>
		///
		/// Converts a AuditEventErrorMessage list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AuditEventErrorMessageDTO> ToDTOList(List<AuditEventErrorMessage> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditEventErrorMessageDTO> output = new List<AuditEventErrorMessageDTO>();

			output.Capacity = data.Count;

			foreach (AuditEventErrorMessage auditEventErrorMessage in data)
			{
				output.Add(auditEventErrorMessage.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AuditEventErrorMessage to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AuditEventErrorMessageEntity type directly.
		///
		/// </summary>
		public AuditEventErrorMessageOutputDTO ToOutputDTO()
		{
			return new AuditEventErrorMessageOutputDTO
			{
				id = this.id,
				auditEventId = this.auditEventId,
				errorMessage = this.errorMessage,
				auditEvent = this.auditEvent?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a AuditEventErrorMessage list to list of Output Data Transfer Object intended to be used for serializing a list of AuditEventErrorMessage objects to avoid using the AuditEventErrorMessage entity type directly.
		///
		/// </summary>
		public static List<AuditEventErrorMessageOutputDTO> ToOutputDTOList(List<AuditEventErrorMessage> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditEventErrorMessageOutputDTO> output = new List<AuditEventErrorMessageOutputDTO>();

			output.Capacity = data.Count;

			foreach (AuditEventErrorMessage auditEventErrorMessage in data)
			{
				output.Add(auditEventErrorMessage.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AuditEventErrorMessage Object.
		///
		/// </summary>
		public static Database.AuditEventErrorMessage FromDTO(AuditEventErrorMessageDTO dto)
		{
			return new Database.AuditEventErrorMessage
			{
				id = dto.id,
				auditEventId = dto.auditEventId,
				errorMessage = dto.errorMessage
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AuditEventErrorMessage Object.
		///
		/// </summary>
		public void ApplyDTO(AuditEventErrorMessageDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.auditEventId = dto.auditEventId;
			this.errorMessage = dto.errorMessage;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a AuditEventErrorMessage Object.
		///
		/// </summary>
		public AuditEventErrorMessage Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AuditEventErrorMessage{
				id = this.id,
				auditEventId = this.auditEventId,
				errorMessage = this.errorMessage,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditEventErrorMessage Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditEventErrorMessage Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AuditEventErrorMessage Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AuditEventErrorMessage Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AuditEventErrorMessage auditEventErrorMessage)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (auditEventErrorMessage == null)
			{
				return null;
			}

			return new {
				id = auditEventErrorMessage.id,
				auditEventId = auditEventErrorMessage.auditEventId,
				errorMessage = auditEventErrorMessage.errorMessage,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AuditEventErrorMessage Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AuditEventErrorMessage auditEventErrorMessage)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (auditEventErrorMessage == null)
			{
				return null;
			}

			return new {
				id = auditEventErrorMessage.id,
				auditEventId = auditEventErrorMessage.auditEventId,
				errorMessage = auditEventErrorMessage.errorMessage,
				auditEvent = AuditEvent.CreateMinimalAnonymous(auditEventErrorMessage.auditEvent)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AuditEventErrorMessage Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AuditEventErrorMessage auditEventErrorMessage)
		{
			//
			// Return a very minimal object.
			//
			if (auditEventErrorMessage == null)
			{
				return null;
			}

			return new {
				id = auditEventErrorMessage.id,
				name = auditEventErrorMessage.id,
				description = auditEventErrorMessage.id
			 };
		}
	}
}
