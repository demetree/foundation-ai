using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class PushProviderConfiguration : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PushProviderConfigurationDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String providerId { get; set; }
			[Required]
			public Boolean enabled { get; set; }
			public String configurationJson { get; set; }
			[Required]
			public DateTime dateTimeModified { get; set; }
			[Required]
			public Int32 modifiedByUserId { get; set; }
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
		public class PushProviderConfigurationOutputDTO : PushProviderConfigurationDTO
		{
		}


		/// <summary>
		///
		/// Converts a PushProviderConfiguration to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PushProviderConfigurationDTO ToDTO()
		{
			return new PushProviderConfigurationDTO
			{
				id = this.id,
				providerId = this.providerId,
				enabled = this.enabled,
				configurationJson = this.configurationJson,
				dateTimeModified = this.dateTimeModified,
				modifiedByUserId = this.modifiedByUserId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PushProviderConfiguration list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PushProviderConfigurationDTO> ToDTOList(List<PushProviderConfiguration> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PushProviderConfigurationDTO> output = new List<PushProviderConfigurationDTO>();

			output.Capacity = data.Count;

			foreach (PushProviderConfiguration pushProviderConfiguration in data)
			{
				output.Add(pushProviderConfiguration.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PushProviderConfiguration to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PushProviderConfiguration Entity type directly.
		///
		/// </summary>
		public PushProviderConfigurationOutputDTO ToOutputDTO()
		{
			return new PushProviderConfigurationOutputDTO
			{
				id = this.id,
				providerId = this.providerId,
				enabled = this.enabled,
				configurationJson = this.configurationJson,
				dateTimeModified = this.dateTimeModified,
				modifiedByUserId = this.modifiedByUserId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PushProviderConfiguration list to list of Output Data Transfer Object intended to be used for serializing a list of PushProviderConfiguration objects to avoid using the PushProviderConfiguration entity type directly.
		///
		/// </summary>
		public static List<PushProviderConfigurationOutputDTO> ToOutputDTOList(List<PushProviderConfiguration> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PushProviderConfigurationOutputDTO> output = new List<PushProviderConfigurationOutputDTO>();

			output.Capacity = data.Count;

			foreach (PushProviderConfiguration pushProviderConfiguration in data)
			{
				output.Add(pushProviderConfiguration.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PushProviderConfiguration Object.
		///
		/// </summary>
		public static Database.PushProviderConfiguration FromDTO(PushProviderConfigurationDTO dto)
		{
			return new Database.PushProviderConfiguration
			{
				id = dto.id,
				providerId = dto.providerId,
				enabled = dto.enabled,
				configurationJson = dto.configurationJson,
				dateTimeModified = dto.dateTimeModified,
				modifiedByUserId = dto.modifiedByUserId,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PushProviderConfiguration Object.
		///
		/// </summary>
		public void ApplyDTO(PushProviderConfigurationDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.providerId = dto.providerId;
			this.enabled = dto.enabled;
			this.configurationJson = dto.configurationJson;
			this.dateTimeModified = dto.dateTimeModified;
			this.modifiedByUserId = dto.modifiedByUserId;
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
		/// Creates a deep copy clone of a PushProviderConfiguration Object.
		///
		/// </summary>
		public PushProviderConfiguration Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PushProviderConfiguration{
				id = this.id,
				tenantGuid = this.tenantGuid,
				providerId = this.providerId,
				enabled = this.enabled,
				configurationJson = this.configurationJson,
				dateTimeModified = this.dateTimeModified,
				modifiedByUserId = this.modifiedByUserId,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PushProviderConfiguration Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PushProviderConfiguration Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PushProviderConfiguration Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PushProviderConfiguration Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PushProviderConfiguration pushProviderConfiguration)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (pushProviderConfiguration == null)
			{
				return null;
			}

			return new {
				id = pushProviderConfiguration.id,
				providerId = pushProviderConfiguration.providerId,
				enabled = pushProviderConfiguration.enabled,
				configurationJson = pushProviderConfiguration.configurationJson,
				dateTimeModified = pushProviderConfiguration.dateTimeModified,
				modifiedByUserId = pushProviderConfiguration.modifiedByUserId,
				objectGuid = pushProviderConfiguration.objectGuid,
				active = pushProviderConfiguration.active,
				deleted = pushProviderConfiguration.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PushProviderConfiguration Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PushProviderConfiguration pushProviderConfiguration)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (pushProviderConfiguration == null)
			{
				return null;
			}

			return new {
				id = pushProviderConfiguration.id,
				providerId = pushProviderConfiguration.providerId,
				enabled = pushProviderConfiguration.enabled,
				configurationJson = pushProviderConfiguration.configurationJson,
				dateTimeModified = pushProviderConfiguration.dateTimeModified,
				modifiedByUserId = pushProviderConfiguration.modifiedByUserId,
				objectGuid = pushProviderConfiguration.objectGuid,
				active = pushProviderConfiguration.active,
				deleted = pushProviderConfiguration.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PushProviderConfiguration Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PushProviderConfiguration pushProviderConfiguration)
		{
			//
			// Return a very minimal object.
			//
			if (pushProviderConfiguration == null)
			{
				return null;
			}

			return new {
				id = pushProviderConfiguration.id,
				name = pushProviderConfiguration.providerId,
				description = string.Join(", ", new[] { pushProviderConfiguration.providerId}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
