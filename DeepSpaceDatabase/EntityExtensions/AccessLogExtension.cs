using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.DeepSpace.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class AccessLog : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AccessLogDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 storageObjectId { get; set; }
			[Required]
			public Int32 accessTypeId { get; set; }
			public Guid? accessedByUserGuid { get; set; }
			[Required]
			public DateTime accessedUtc { get; set; }
			public String ipAddress { get; set; }
			public Int32? bytesTransferred { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class AccessLogOutputDTO : AccessLogDTO
		{
			public AccessType.AccessTypeDTO accessType { get; set; }
			public StorageObject.StorageObjectDTO storageObject { get; set; }
		}


		/// <summary>
		///
		/// Converts a AccessLog to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AccessLogDTO ToDTO()
		{
			return new AccessLogDTO
			{
				id = this.id,
				storageObjectId = this.storageObjectId,
				accessTypeId = this.accessTypeId,
				accessedByUserGuid = this.accessedByUserGuid,
				accessedUtc = this.accessedUtc,
				ipAddress = this.ipAddress,
				bytesTransferred = this.bytesTransferred,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a AccessLog list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AccessLogDTO> ToDTOList(List<AccessLog> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AccessLogDTO> output = new List<AccessLogDTO>();

			output.Capacity = data.Count;

			foreach (AccessLog accessLog in data)
			{
				output.Add(accessLog.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AccessLog to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AccessLog Entity type directly.
		///
		/// </summary>
		public AccessLogOutputDTO ToOutputDTO()
		{
			return new AccessLogOutputDTO
			{
				id = this.id,
				storageObjectId = this.storageObjectId,
				accessTypeId = this.accessTypeId,
				accessedByUserGuid = this.accessedByUserGuid,
				accessedUtc = this.accessedUtc,
				ipAddress = this.ipAddress,
				bytesTransferred = this.bytesTransferred,
				active = this.active,
				deleted = this.deleted,
				accessType = this.accessType?.ToDTO(),
				storageObject = this.storageObject?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a AccessLog list to list of Output Data Transfer Object intended to be used for serializing a list of AccessLog objects to avoid using the AccessLog entity type directly.
		///
		/// </summary>
		public static List<AccessLogOutputDTO> ToOutputDTOList(List<AccessLog> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AccessLogOutputDTO> output = new List<AccessLogOutputDTO>();

			output.Capacity = data.Count;

			foreach (AccessLog accessLog in data)
			{
				output.Add(accessLog.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AccessLog Object.
		///
		/// </summary>
		public static Database.AccessLog FromDTO(AccessLogDTO dto)
		{
			return new Database.AccessLog
			{
				id = dto.id,
				storageObjectId = dto.storageObjectId,
				accessTypeId = dto.accessTypeId,
				accessedByUserGuid = dto.accessedByUserGuid,
				accessedUtc = dto.accessedUtc,
				ipAddress = dto.ipAddress,
				bytesTransferred = dto.bytesTransferred,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AccessLog Object.
		///
		/// </summary>
		public void ApplyDTO(AccessLogDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.storageObjectId = dto.storageObjectId;
			this.accessTypeId = dto.accessTypeId;
			this.accessedByUserGuid = dto.accessedByUserGuid;
			this.accessedUtc = dto.accessedUtc;
			this.ipAddress = dto.ipAddress;
			this.bytesTransferred = dto.bytesTransferred;
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
		/// Creates a deep copy clone of a AccessLog Object.
		///
		/// </summary>
		public AccessLog Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AccessLog{
				id = this.id,
				storageObjectId = this.storageObjectId,
				accessTypeId = this.accessTypeId,
				accessedByUserGuid = this.accessedByUserGuid,
				accessedUtc = this.accessedUtc,
				ipAddress = this.ipAddress,
				bytesTransferred = this.bytesTransferred,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AccessLog Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AccessLog Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AccessLog Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AccessLog Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AccessLog accessLog)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (accessLog == null)
			{
				return null;
			}

			return new {
				id = accessLog.id,
				storageObjectId = accessLog.storageObjectId,
				accessTypeId = accessLog.accessTypeId,
				accessedByUserGuid = accessLog.accessedByUserGuid,
				accessedUtc = accessLog.accessedUtc,
				ipAddress = accessLog.ipAddress,
				bytesTransferred = accessLog.bytesTransferred,
				active = accessLog.active,
				deleted = accessLog.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AccessLog Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AccessLog accessLog)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (accessLog == null)
			{
				return null;
			}

			return new {
				id = accessLog.id,
				storageObjectId = accessLog.storageObjectId,
				accessTypeId = accessLog.accessTypeId,
				accessedByUserGuid = accessLog.accessedByUserGuid,
				accessedUtc = accessLog.accessedUtc,
				ipAddress = accessLog.ipAddress,
				bytesTransferred = accessLog.bytesTransferred,
				active = accessLog.active,
				deleted = accessLog.deleted,
				accessType = AccessType.CreateMinimalAnonymous(accessLog.accessType),
				storageObject = StorageObject.CreateMinimalAnonymous(accessLog.storageObject)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AccessLog Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AccessLog accessLog)
		{
			//
			// Return a very minimal object.
			//
			if (accessLog == null)
			{
				return null;
			}

			return new {
				id = accessLog.id,
				name = accessLog.ipAddress,
				description = string.Join(", ", new[] { accessLog.ipAddress}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
