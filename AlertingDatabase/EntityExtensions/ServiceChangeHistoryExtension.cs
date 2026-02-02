using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Alerting.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class ServiceChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)serviceId; }
			set { serviceId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ServiceChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 serviceId { get; set; }
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
		public class ServiceChangeHistoryOutputDTO : ServiceChangeHistoryDTO
		{
			public Service.ServiceDTO service { get; set; }
		}


		/// <summary>
		///
		/// Converts a ServiceChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ServiceChangeHistoryDTO ToDTO()
		{
			return new ServiceChangeHistoryDTO
			{
				id = this.id,
				serviceId = this.serviceId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ServiceChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ServiceChangeHistoryDTO> ToDTOList(List<ServiceChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ServiceChangeHistoryDTO> output = new List<ServiceChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ServiceChangeHistory serviceChangeHistory in data)
			{
				output.Add(serviceChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ServiceChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ServiceChangeHistoryEntity type directly.
		///
		/// </summary>
		public ServiceChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ServiceChangeHistoryOutputDTO
			{
				id = this.id,
				serviceId = this.serviceId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				service = this.service?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ServiceChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ServiceChangeHistory objects to avoid using the ServiceChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ServiceChangeHistoryOutputDTO> ToOutputDTOList(List<ServiceChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ServiceChangeHistoryOutputDTO> output = new List<ServiceChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ServiceChangeHistory serviceChangeHistory in data)
			{
				output.Add(serviceChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ServiceChangeHistory Object.
		///
		/// </summary>
		public static Database.ServiceChangeHistory FromDTO(ServiceChangeHistoryDTO dto)
		{
			return new Database.ServiceChangeHistory
			{
				id = dto.id,
				serviceId = dto.serviceId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ServiceChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ServiceChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.serviceId = dto.serviceId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ServiceChangeHistory Object.
		///
		/// </summary>
		public ServiceChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ServiceChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				serviceId = this.serviceId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ServiceChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ServiceChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ServiceChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ServiceChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ServiceChangeHistory serviceChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (serviceChangeHistory == null)
			{
				return null;
			}

			return new {
				id = serviceChangeHistory.id,
				serviceId = serviceChangeHistory.serviceId,
				versionNumber = serviceChangeHistory.versionNumber,
				timeStamp = serviceChangeHistory.timeStamp,
				userId = serviceChangeHistory.userId,
				data = serviceChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ServiceChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ServiceChangeHistory serviceChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (serviceChangeHistory == null)
			{
				return null;
			}

			return new {
				id = serviceChangeHistory.id,
				serviceId = serviceChangeHistory.serviceId,
				versionNumber = serviceChangeHistory.versionNumber,
				timeStamp = serviceChangeHistory.timeStamp,
				userId = serviceChangeHistory.userId,
				data = serviceChangeHistory.data,
				service = Service.CreateMinimalAnonymous(serviceChangeHistory.service)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ServiceChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ServiceChangeHistory serviceChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (serviceChangeHistory == null)
			{
				return null;
			}

			return new {
				id = serviceChangeHistory.id,
				name = serviceChangeHistory.id,
				description = serviceChangeHistory.id
			 };
		}
	}
}
