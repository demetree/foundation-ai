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
	public partial class ExternalCommunication : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ExternalCommunicationDTO
		{
			public Int32 id { get; set; }
			public DateTime? timeStamp { get; set; }
			public Int32? auditUserId { get; set; }
			public String communicationType { get; set; }
			public String subject { get; set; }
			public String message { get; set; }
			[Required]
			public Boolean completedSuccessfully { get; set; }
			public String responseMessage { get; set; }
			public String exceptionText { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class ExternalCommunicationOutputDTO : ExternalCommunicationDTO
		{
			public AuditUser.AuditUserDTO auditUser { get; set; }
		}


		/// <summary>
		///
		/// Converts a ExternalCommunication to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ExternalCommunicationDTO ToDTO()
		{
			return new ExternalCommunicationDTO
			{
				id = this.id,
				timeStamp = this.timeStamp,
				auditUserId = this.auditUserId,
				communicationType = this.communicationType,
				subject = this.subject,
				message = this.message,
				completedSuccessfully = this.completedSuccessfully,
				responseMessage = this.responseMessage,
				exceptionText = this.exceptionText
			};
		}


		/// <summary>
		///
		/// Converts a ExternalCommunication list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ExternalCommunicationDTO> ToDTOList(List<ExternalCommunication> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ExternalCommunicationDTO> output = new List<ExternalCommunicationDTO>();

			output.Capacity = data.Count;

			foreach (ExternalCommunication externalCommunication in data)
			{
				output.Add(externalCommunication.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ExternalCommunication to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ExternalCommunicationEntity type directly.
		///
		/// </summary>
		public ExternalCommunicationOutputDTO ToOutputDTO()
		{
			return new ExternalCommunicationOutputDTO
			{
				id = this.id,
				timeStamp = this.timeStamp,
				auditUserId = this.auditUserId,
				communicationType = this.communicationType,
				subject = this.subject,
				message = this.message,
				completedSuccessfully = this.completedSuccessfully,
				responseMessage = this.responseMessage,
				exceptionText = this.exceptionText,
				auditUser = this.auditUser?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ExternalCommunication list to list of Output Data Transfer Object intended to be used for serializing a list of ExternalCommunication objects to avoid using the ExternalCommunication entity type directly.
		///
		/// </summary>
		public static List<ExternalCommunicationOutputDTO> ToOutputDTOList(List<ExternalCommunication> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ExternalCommunicationOutputDTO> output = new List<ExternalCommunicationOutputDTO>();

			output.Capacity = data.Count;

			foreach (ExternalCommunication externalCommunication in data)
			{
				output.Add(externalCommunication.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ExternalCommunication Object.
		///
		/// </summary>
		public static Database.ExternalCommunication FromDTO(ExternalCommunicationDTO dto)
		{
			return new Database.ExternalCommunication
			{
				id = dto.id,
				timeStamp = dto.timeStamp,
				auditUserId = dto.auditUserId,
				communicationType = dto.communicationType,
				subject = dto.subject,
				message = dto.message,
				completedSuccessfully = dto.completedSuccessfully,
				responseMessage = dto.responseMessage,
				exceptionText = dto.exceptionText
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ExternalCommunication Object.
		///
		/// </summary>
		public void ApplyDTO(ExternalCommunicationDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.timeStamp = dto.timeStamp;
			this.auditUserId = dto.auditUserId;
			this.communicationType = dto.communicationType;
			this.subject = dto.subject;
			this.message = dto.message;
			this.completedSuccessfully = dto.completedSuccessfully;
			this.responseMessage = dto.responseMessage;
			this.exceptionText = dto.exceptionText;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ExternalCommunication Object.
		///
		/// </summary>
		public ExternalCommunication Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ExternalCommunication{
				id = this.id,
				timeStamp = this.timeStamp,
				auditUserId = this.auditUserId,
				communicationType = this.communicationType,
				subject = this.subject,
				message = this.message,
				completedSuccessfully = this.completedSuccessfully,
				responseMessage = this.responseMessage,
				exceptionText = this.exceptionText,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ExternalCommunication Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ExternalCommunication Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ExternalCommunication Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ExternalCommunication Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ExternalCommunication externalCommunication)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (externalCommunication == null)
			{
				return null;
			}

			return new {
				id = externalCommunication.id,
				timeStamp = externalCommunication.timeStamp,
				auditUserId = externalCommunication.auditUserId,
				communicationType = externalCommunication.communicationType,
				subject = externalCommunication.subject,
				message = externalCommunication.message,
				completedSuccessfully = externalCommunication.completedSuccessfully,
				responseMessage = externalCommunication.responseMessage,
				exceptionText = externalCommunication.exceptionText,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ExternalCommunication Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ExternalCommunication externalCommunication)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (externalCommunication == null)
			{
				return null;
			}

			return new {
				id = externalCommunication.id,
				timeStamp = externalCommunication.timeStamp,
				auditUserId = externalCommunication.auditUserId,
				communicationType = externalCommunication.communicationType,
				subject = externalCommunication.subject,
				message = externalCommunication.message,
				completedSuccessfully = externalCommunication.completedSuccessfully,
				responseMessage = externalCommunication.responseMessage,
				exceptionText = externalCommunication.exceptionText,
				auditUser = AuditUser.CreateMinimalAnonymous(externalCommunication.auditUser)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ExternalCommunication Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ExternalCommunication externalCommunication)
		{
			//
			// Return a very minimal object.
			//
			if (externalCommunication == null)
			{
				return null;
			}

			return new {
				id = externalCommunication.id,
				name = externalCommunication.communicationType,
				description = string.Join(", ", new[] { externalCommunication.communicationType, externalCommunication.subject}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
