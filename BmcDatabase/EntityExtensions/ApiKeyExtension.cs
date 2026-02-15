using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.BMC.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class ApiKey : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ApiKeyDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String keyHash { get; set; }
			[Required]
			public String keyPrefix { get; set; }
			[Required]
			public String name { get; set; }
			public String description { get; set; }
			[Required]
			public Boolean isActive { get; set; }
			[Required]
			public DateTime createdDate { get; set; }
			public DateTime? lastUsedDate { get; set; }
			public DateTime? expiresDate { get; set; }
			[Required]
			public Int32 rateLimitPerHour { get; set; }
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
		public class ApiKeyOutputDTO : ApiKeyDTO
		{
		}


		/// <summary>
		///
		/// Converts a ApiKey to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ApiKeyDTO ToDTO()
		{
			return new ApiKeyDTO
			{
				id = this.id,
				keyHash = this.keyHash,
				keyPrefix = this.keyPrefix,
				name = this.name,
				description = this.description,
				isActive = this.isActive,
				createdDate = this.createdDate,
				lastUsedDate = this.lastUsedDate,
				expiresDate = this.expiresDate,
				rateLimitPerHour = this.rateLimitPerHour,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ApiKey list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ApiKeyDTO> ToDTOList(List<ApiKey> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ApiKeyDTO> output = new List<ApiKeyDTO>();

			output.Capacity = data.Count;

			foreach (ApiKey apiKey in data)
			{
				output.Add(apiKey.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ApiKey to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ApiKeyEntity type directly.
		///
		/// </summary>
		public ApiKeyOutputDTO ToOutputDTO()
		{
			return new ApiKeyOutputDTO
			{
				id = this.id,
				keyHash = this.keyHash,
				keyPrefix = this.keyPrefix,
				name = this.name,
				description = this.description,
				isActive = this.isActive,
				createdDate = this.createdDate,
				lastUsedDate = this.lastUsedDate,
				expiresDate = this.expiresDate,
				rateLimitPerHour = this.rateLimitPerHour,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a ApiKey list to list of Output Data Transfer Object intended to be used for serializing a list of ApiKey objects to avoid using the ApiKey entity type directly.
		///
		/// </summary>
		public static List<ApiKeyOutputDTO> ToOutputDTOList(List<ApiKey> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ApiKeyOutputDTO> output = new List<ApiKeyOutputDTO>();

			output.Capacity = data.Count;

			foreach (ApiKey apiKey in data)
			{
				output.Add(apiKey.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ApiKey Object.
		///
		/// </summary>
		public static Database.ApiKey FromDTO(ApiKeyDTO dto)
		{
			return new Database.ApiKey
			{
				id = dto.id,
				keyHash = dto.keyHash,
				keyPrefix = dto.keyPrefix,
				name = dto.name,
				description = dto.description,
				isActive = dto.isActive,
				createdDate = dto.createdDate,
				lastUsedDate = dto.lastUsedDate,
				expiresDate = dto.expiresDate,
				rateLimitPerHour = dto.rateLimitPerHour,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ApiKey Object.
		///
		/// </summary>
		public void ApplyDTO(ApiKeyDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.keyHash = dto.keyHash;
			this.keyPrefix = dto.keyPrefix;
			this.name = dto.name;
			this.description = dto.description;
			this.isActive = dto.isActive;
			this.createdDate = dto.createdDate;
			this.lastUsedDate = dto.lastUsedDate;
			this.expiresDate = dto.expiresDate;
			this.rateLimitPerHour = dto.rateLimitPerHour;
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
		/// Creates a deep copy clone of a ApiKey Object.
		///
		/// </summary>
		public ApiKey Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ApiKey{
				id = this.id,
				tenantGuid = this.tenantGuid,
				keyHash = this.keyHash,
				keyPrefix = this.keyPrefix,
				name = this.name,
				description = this.description,
				isActive = this.isActive,
				createdDate = this.createdDate,
				lastUsedDate = this.lastUsedDate,
				expiresDate = this.expiresDate,
				rateLimitPerHour = this.rateLimitPerHour,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ApiKey Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ApiKey Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ApiKey Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ApiKey Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ApiKey apiKey)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (apiKey == null)
			{
				return null;
			}

			return new {
				id = apiKey.id,
				keyHash = apiKey.keyHash,
				keyPrefix = apiKey.keyPrefix,
				name = apiKey.name,
				description = apiKey.description,
				isActive = apiKey.isActive,
				createdDate = apiKey.createdDate,
				lastUsedDate = apiKey.lastUsedDate,
				expiresDate = apiKey.expiresDate,
				rateLimitPerHour = apiKey.rateLimitPerHour,
				objectGuid = apiKey.objectGuid,
				active = apiKey.active,
				deleted = apiKey.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ApiKey Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ApiKey apiKey)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (apiKey == null)
			{
				return null;
			}

			return new {
				id = apiKey.id,
				keyHash = apiKey.keyHash,
				keyPrefix = apiKey.keyPrefix,
				name = apiKey.name,
				description = apiKey.description,
				isActive = apiKey.isActive,
				createdDate = apiKey.createdDate,
				lastUsedDate = apiKey.lastUsedDate,
				expiresDate = apiKey.expiresDate,
				rateLimitPerHour = apiKey.rateLimitPerHour,
				objectGuid = apiKey.objectGuid,
				active = apiKey.active,
				deleted = apiKey.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ApiKey Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ApiKey apiKey)
		{
			//
			// Return a very minimal object.
			//
			if (apiKey == null)
			{
				return null;
			}

			return new {
				id = apiKey.id,
				name = apiKey.name,
				description = apiKey.description,
			 };
		}
	}
}
