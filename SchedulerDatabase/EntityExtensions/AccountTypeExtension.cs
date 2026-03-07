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
	public partial class AccountType : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AccountTypeDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String description { get; set; }
			[Required]
			public Boolean isRevenue { get; set; }
			public String externalMapping { get; set; }
			public String color { get; set; }
			public Int32? sequence { get; set; }
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
		public class AccountTypeOutputDTO : AccountTypeDTO
		{
		}


		/// <summary>
		///
		/// Converts a AccountType to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AccountTypeDTO ToDTO()
		{
			return new AccountTypeDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				isRevenue = this.isRevenue,
				externalMapping = this.externalMapping,
				color = this.color,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a AccountType list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AccountTypeDTO> ToDTOList(List<AccountType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AccountTypeDTO> output = new List<AccountTypeDTO>();

			output.Capacity = data.Count;

			foreach (AccountType accountType in data)
			{
				output.Add(accountType.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AccountType to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AccountTypeEntity type directly.
		///
		/// </summary>
		public AccountTypeOutputDTO ToOutputDTO()
		{
			return new AccountTypeOutputDTO
			{
				id = this.id,
				name = this.name,
				description = this.description,
				isRevenue = this.isRevenue,
				externalMapping = this.externalMapping,
				color = this.color,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a AccountType list to list of Output Data Transfer Object intended to be used for serializing a list of AccountType objects to avoid using the AccountType entity type directly.
		///
		/// </summary>
		public static List<AccountTypeOutputDTO> ToOutputDTOList(List<AccountType> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AccountTypeOutputDTO> output = new List<AccountTypeOutputDTO>();

			output.Capacity = data.Count;

			foreach (AccountType accountType in data)
			{
				output.Add(accountType.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AccountType Object.
		///
		/// </summary>
		public static Database.AccountType FromDTO(AccountTypeDTO dto)
		{
			return new Database.AccountType
			{
				id = dto.id,
				name = dto.name,
				description = dto.description,
				isRevenue = dto.isRevenue,
				externalMapping = dto.externalMapping,
				color = dto.color,
				sequence = dto.sequence,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AccountType Object.
		///
		/// </summary>
		public void ApplyDTO(AccountTypeDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.description = dto.description;
			this.isRevenue = dto.isRevenue;
			this.externalMapping = dto.externalMapping;
			this.color = dto.color;
			this.sequence = dto.sequence;
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
		/// Creates a deep copy clone of a AccountType Object.
		///
		/// </summary>
		public AccountType Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AccountType{
				id = this.id,
				name = this.name,
				description = this.description,
				isRevenue = this.isRevenue,
				externalMapping = this.externalMapping,
				color = this.color,
				sequence = this.sequence,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AccountType Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AccountType Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AccountType Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AccountType Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AccountType accountType)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (accountType == null)
			{
				return null;
			}

			return new {
				id = accountType.id,
				name = accountType.name,
				description = accountType.description,
				isRevenue = accountType.isRevenue,
				externalMapping = accountType.externalMapping,
				color = accountType.color,
				sequence = accountType.sequence,
				objectGuid = accountType.objectGuid,
				active = accountType.active,
				deleted = accountType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AccountType Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AccountType accountType)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (accountType == null)
			{
				return null;
			}

			return new {
				id = accountType.id,
				name = accountType.name,
				description = accountType.description,
				isRevenue = accountType.isRevenue,
				externalMapping = accountType.externalMapping,
				color = accountType.color,
				sequence = accountType.sequence,
				objectGuid = accountType.objectGuid,
				active = accountType.active,
				deleted = accountType.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AccountType Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AccountType accountType)
		{
			//
			// Return a very minimal object.
			//
			if (accountType == null)
			{
				return null;
			}

			return new {
				id = accountType.id,
				name = accountType.name,
				description = accountType.description,
			 };
		}
	}
}
