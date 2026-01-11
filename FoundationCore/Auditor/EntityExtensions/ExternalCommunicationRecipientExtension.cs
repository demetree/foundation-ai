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
	public partial class ExternalCommunicationRecipient : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ExternalCommunicationRecipientDTO
		{
			public Int32 id { get; set; }
			public Int32? externalCommunicationId { get; set; }
			public String recipient { get; set; }
			public String type { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class ExternalCommunicationRecipientOutputDTO : ExternalCommunicationRecipientDTO
		{
			public ExternalCommunication.ExternalCommunicationDTO externalCommunication { get; set; }
		}


		/// <summary>
		///
		/// Converts a ExternalCommunicationRecipient to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ExternalCommunicationRecipientDTO ToDTO()
		{
			return new ExternalCommunicationRecipientDTO
			{
				id = this.id,
				externalCommunicationId = this.externalCommunicationId,
				recipient = this.recipient,
				type = this.type
			};
		}


		/// <summary>
		///
		/// Converts a ExternalCommunicationRecipient list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ExternalCommunicationRecipientDTO> ToDTOList(List<ExternalCommunicationRecipient> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ExternalCommunicationRecipientDTO> output = new List<ExternalCommunicationRecipientDTO>();

			output.Capacity = data.Count;

			foreach (ExternalCommunicationRecipient externalCommunicationRecipient in data)
			{
				output.Add(externalCommunicationRecipient.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ExternalCommunicationRecipient to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ExternalCommunicationRecipientEntity type directly.
		///
		/// </summary>
		public ExternalCommunicationRecipientOutputDTO ToOutputDTO()
		{
			return new ExternalCommunicationRecipientOutputDTO
			{
				id = this.id,
				externalCommunicationId = this.externalCommunicationId,
				recipient = this.recipient,
				type = this.type,
				externalCommunication = this.externalCommunication?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ExternalCommunicationRecipient list to list of Output Data Transfer Object intended to be used for serializing a list of ExternalCommunicationRecipient objects to avoid using the ExternalCommunicationRecipient entity type directly.
		///
		/// </summary>
		public static List<ExternalCommunicationRecipientOutputDTO> ToOutputDTOList(List<ExternalCommunicationRecipient> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ExternalCommunicationRecipientOutputDTO> output = new List<ExternalCommunicationRecipientOutputDTO>();

			output.Capacity = data.Count;

			foreach (ExternalCommunicationRecipient externalCommunicationRecipient in data)
			{
				output.Add(externalCommunicationRecipient.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ExternalCommunicationRecipient Object.
		///
		/// </summary>
		public static Database.ExternalCommunicationRecipient FromDTO(ExternalCommunicationRecipientDTO dto)
		{
			return new Database.ExternalCommunicationRecipient
			{
				id = dto.id,
				externalCommunicationId = dto.externalCommunicationId,
				recipient = dto.recipient,
				type = dto.type
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ExternalCommunicationRecipient Object.
		///
		/// </summary>
		public void ApplyDTO(ExternalCommunicationRecipientDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.externalCommunicationId = dto.externalCommunicationId;
			this.recipient = dto.recipient;
			this.type = dto.type;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ExternalCommunicationRecipient Object.
		///
		/// </summary>
		public ExternalCommunicationRecipient Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ExternalCommunicationRecipient{
				id = this.id,
				externalCommunicationId = this.externalCommunicationId,
				recipient = this.recipient,
				type = this.type,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ExternalCommunicationRecipient Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ExternalCommunicationRecipient Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ExternalCommunicationRecipient Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ExternalCommunicationRecipient Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ExternalCommunicationRecipient externalCommunicationRecipient)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (externalCommunicationRecipient == null)
			{
				return null;
			}

			return new {
				id = externalCommunicationRecipient.id,
				externalCommunicationId = externalCommunicationRecipient.externalCommunicationId,
				recipient = externalCommunicationRecipient.recipient,
				type = externalCommunicationRecipient.type,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ExternalCommunicationRecipient Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ExternalCommunicationRecipient externalCommunicationRecipient)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (externalCommunicationRecipient == null)
			{
				return null;
			}

			return new {
				id = externalCommunicationRecipient.id,
				externalCommunicationId = externalCommunicationRecipient.externalCommunicationId,
				recipient = externalCommunicationRecipient.recipient,
				type = externalCommunicationRecipient.type,
				externalCommunication = ExternalCommunication.CreateMinimalAnonymous(externalCommunicationRecipient.externalCommunication)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ExternalCommunicationRecipient Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ExternalCommunicationRecipient externalCommunicationRecipient)
		{
			//
			// Return a very minimal object.
			//
			if (externalCommunicationRecipient == null)
			{
				return null;
			}

			return new {
				id = externalCommunicationRecipient.id,
				name = externalCommunicationRecipient.recipient,
				description = string.Join(", ", new[] { externalCommunicationRecipient.recipient, externalCommunicationRecipient.type}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
