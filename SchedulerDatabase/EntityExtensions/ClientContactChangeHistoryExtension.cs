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
	public partial class ClientContactChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)clientContactId; }
			set { clientContactId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ClientContactChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 clientContactId { get; set; }
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
		public class ClientContactChangeHistoryOutputDTO : ClientContactChangeHistoryDTO
		{
			public ClientContact.ClientContactDTO clientContact { get; set; }
		}


		/// <summary>
		///
		/// Converts a ClientContactChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ClientContactChangeHistoryDTO ToDTO()
		{
			return new ClientContactChangeHistoryDTO
			{
				id = this.id,
				clientContactId = this.clientContactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ClientContactChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ClientContactChangeHistoryDTO> ToDTOList(List<ClientContactChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ClientContactChangeHistoryDTO> output = new List<ClientContactChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ClientContactChangeHistory clientContactChangeHistory in data)
			{
				output.Add(clientContactChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ClientContactChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ClientContactChangeHistory Entity type directly.
		///
		/// </summary>
		public ClientContactChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ClientContactChangeHistoryOutputDTO
			{
				id = this.id,
				clientContactId = this.clientContactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				clientContact = this.clientContact?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ClientContactChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ClientContactChangeHistory objects to avoid using the ClientContactChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ClientContactChangeHistoryOutputDTO> ToOutputDTOList(List<ClientContactChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ClientContactChangeHistoryOutputDTO> output = new List<ClientContactChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ClientContactChangeHistory clientContactChangeHistory in data)
			{
				output.Add(clientContactChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ClientContactChangeHistory Object.
		///
		/// </summary>
		public static Database.ClientContactChangeHistory FromDTO(ClientContactChangeHistoryDTO dto)
		{
			return new Database.ClientContactChangeHistory
			{
				id = dto.id,
				clientContactId = dto.clientContactId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ClientContactChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ClientContactChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.clientContactId = dto.clientContactId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ClientContactChangeHistory Object.
		///
		/// </summary>
		public ClientContactChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ClientContactChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				clientContactId = this.clientContactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ClientContactChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ClientContactChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ClientContactChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ClientContactChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ClientContactChangeHistory clientContactChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (clientContactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = clientContactChangeHistory.id,
				clientContactId = clientContactChangeHistory.clientContactId,
				versionNumber = clientContactChangeHistory.versionNumber,
				timeStamp = clientContactChangeHistory.timeStamp,
				userId = clientContactChangeHistory.userId,
				data = clientContactChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ClientContactChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ClientContactChangeHistory clientContactChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (clientContactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = clientContactChangeHistory.id,
				clientContactId = clientContactChangeHistory.clientContactId,
				versionNumber = clientContactChangeHistory.versionNumber,
				timeStamp = clientContactChangeHistory.timeStamp,
				userId = clientContactChangeHistory.userId,
				data = clientContactChangeHistory.data,
				clientContact = ClientContact.CreateMinimalAnonymous(clientContactChangeHistory.clientContact)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ClientContactChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ClientContactChangeHistory clientContactChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (clientContactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = clientContactChangeHistory.id,
				name = clientContactChangeHistory.id,
				description = clientContactChangeHistory.id
			 };
		}
	}
}
