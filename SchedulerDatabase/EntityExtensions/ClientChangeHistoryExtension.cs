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
	public partial class ClientChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)clientId; }
			set { clientId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ClientChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 clientId { get; set; }
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
		public class ClientChangeHistoryOutputDTO : ClientChangeHistoryDTO
		{
			public Client.ClientDTO client { get; set; }
		}


		/// <summary>
		///
		/// Converts a ClientChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ClientChangeHistoryDTO ToDTO()
		{
			return new ClientChangeHistoryDTO
			{
				id = this.id,
				clientId = this.clientId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ClientChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ClientChangeHistoryDTO> ToDTOList(List<ClientChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ClientChangeHistoryDTO> output = new List<ClientChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ClientChangeHistory clientChangeHistory in data)
			{
				output.Add(clientChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ClientChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ClientChangeHistory Entity type directly.
		///
		/// </summary>
		public ClientChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ClientChangeHistoryOutputDTO
			{
				id = this.id,
				clientId = this.clientId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				client = this.client?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ClientChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ClientChangeHistory objects to avoid using the ClientChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ClientChangeHistoryOutputDTO> ToOutputDTOList(List<ClientChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ClientChangeHistoryOutputDTO> output = new List<ClientChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ClientChangeHistory clientChangeHistory in data)
			{
				output.Add(clientChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ClientChangeHistory Object.
		///
		/// </summary>
		public static Database.ClientChangeHistory FromDTO(ClientChangeHistoryDTO dto)
		{
			return new Database.ClientChangeHistory
			{
				id = dto.id,
				clientId = dto.clientId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ClientChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ClientChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.clientId = dto.clientId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ClientChangeHistory Object.
		///
		/// </summary>
		public ClientChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ClientChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				clientId = this.clientId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ClientChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ClientChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ClientChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ClientChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ClientChangeHistory clientChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (clientChangeHistory == null)
			{
				return null;
			}

			return new {
				id = clientChangeHistory.id,
				clientId = clientChangeHistory.clientId,
				versionNumber = clientChangeHistory.versionNumber,
				timeStamp = clientChangeHistory.timeStamp,
				userId = clientChangeHistory.userId,
				data = clientChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ClientChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ClientChangeHistory clientChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (clientChangeHistory == null)
			{
				return null;
			}

			return new {
				id = clientChangeHistory.id,
				clientId = clientChangeHistory.clientId,
				versionNumber = clientChangeHistory.versionNumber,
				timeStamp = clientChangeHistory.timeStamp,
				userId = clientChangeHistory.userId,
				data = clientChangeHistory.data,
				client = Client.CreateMinimalAnonymous(clientChangeHistory.client)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ClientChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ClientChangeHistory clientChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (clientChangeHistory == null)
			{
				return null;
			}

			return new {
				id = clientChangeHistory.id,
				name = clientChangeHistory.id,
				description = clientChangeHistory.id
			 };
		}
	}
}
