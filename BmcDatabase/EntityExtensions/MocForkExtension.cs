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
	public partial class MocFork : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class MocForkDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 forkedMocId { get; set; }
			[Required]
			public Int32 sourceMocId { get; set; }
			public Int32? mocVersionId { get; set; }
			[Required]
			public Guid forkerTenantGuid { get; set; }
			[Required]
			public DateTime forkedDate { get; set; }
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
		public class MocForkOutputDTO : MocForkDTO
		{
			public PublishedMoc.PublishedMocDTO forkedMoc { get; set; }
			public MocVersion.MocVersionDTO mocVersion { get; set; }
			public PublishedMoc.PublishedMocDTO sourceMoc { get; set; }
		}


		/// <summary>
		///
		/// Converts a MocFork to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public MocForkDTO ToDTO()
		{
			return new MocForkDTO
			{
				id = this.id,
				forkedMocId = this.forkedMocId,
				sourceMocId = this.sourceMocId,
				mocVersionId = this.mocVersionId,
				forkerTenantGuid = this.forkerTenantGuid,
				forkedDate = this.forkedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a MocFork list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<MocForkDTO> ToDTOList(List<MocFork> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MocForkDTO> output = new List<MocForkDTO>();

			output.Capacity = data.Count;

			foreach (MocFork mocFork in data)
			{
				output.Add(mocFork.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a MocFork to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the MocForkEntity type directly.
		///
		/// </summary>
		public MocForkOutputDTO ToOutputDTO()
		{
			return new MocForkOutputDTO
			{
				id = this.id,
				forkedMocId = this.forkedMocId,
				sourceMocId = this.sourceMocId,
				mocVersionId = this.mocVersionId,
				forkerTenantGuid = this.forkerTenantGuid,
				forkedDate = this.forkedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				forkedMoc = this.forkedMoc?.ToDTO(),
				mocVersion = this.mocVersion?.ToDTO(),
				sourceMoc = this.sourceMoc?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a MocFork list to list of Output Data Transfer Object intended to be used for serializing a list of MocFork objects to avoid using the MocFork entity type directly.
		///
		/// </summary>
		public static List<MocForkOutputDTO> ToOutputDTOList(List<MocFork> data)
		{
			if (data == null)
			{
				return null;
			}

			List<MocForkOutputDTO> output = new List<MocForkOutputDTO>();

			output.Capacity = data.Count;

			foreach (MocFork mocFork in data)
			{
				output.Add(mocFork.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a MocFork Object.
		///
		/// </summary>
		public static Database.MocFork FromDTO(MocForkDTO dto)
		{
			return new Database.MocFork
			{
				id = dto.id,
				forkedMocId = dto.forkedMocId,
				sourceMocId = dto.sourceMocId,
				mocVersionId = dto.mocVersionId,
				forkerTenantGuid = dto.forkerTenantGuid,
				forkedDate = dto.forkedDate,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a MocFork Object.
		///
		/// </summary>
		public void ApplyDTO(MocForkDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.forkedMocId = dto.forkedMocId;
			this.sourceMocId = dto.sourceMocId;
			this.mocVersionId = dto.mocVersionId;
			this.forkerTenantGuid = dto.forkerTenantGuid;
			this.forkedDate = dto.forkedDate;
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
		/// Creates a deep copy clone of a MocFork Object.
		///
		/// </summary>
		public MocFork Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new MocFork{
				id = this.id,
				forkedMocId = this.forkedMocId,
				sourceMocId = this.sourceMocId,
				mocVersionId = this.mocVersionId,
				forkerTenantGuid = this.forkerTenantGuid,
				forkedDate = this.forkedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MocFork Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a MocFork Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a MocFork Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a MocFork Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.MocFork mocFork)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (mocFork == null)
			{
				return null;
			}

			return new {
				id = mocFork.id,
				forkedMocId = mocFork.forkedMocId,
				sourceMocId = mocFork.sourceMocId,
				mocVersionId = mocFork.mocVersionId,
				forkerTenantGuid = mocFork.forkerTenantGuid,
				forkedDate = mocFork.forkedDate,
				objectGuid = mocFork.objectGuid,
				active = mocFork.active,
				deleted = mocFork.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a MocFork Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(MocFork mocFork)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (mocFork == null)
			{
				return null;
			}

			return new {
				id = mocFork.id,
				forkedMocId = mocFork.forkedMocId,
				sourceMocId = mocFork.sourceMocId,
				mocVersionId = mocFork.mocVersionId,
				forkerTenantGuid = mocFork.forkerTenantGuid,
				forkedDate = mocFork.forkedDate,
				objectGuid = mocFork.objectGuid,
				active = mocFork.active,
				deleted = mocFork.deleted,
				forkedMoc = PublishedMoc.CreateMinimalAnonymous(mocFork.forkedMoc),
				mocVersion = MocVersion.CreateMinimalAnonymous(mocFork.mocVersion),
				sourceMoc = PublishedMoc.CreateMinimalAnonymous(mocFork.sourceMoc)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a MocFork Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(MocFork mocFork)
		{
			//
			// Return a very minimal object.
			//
			if (mocFork == null)
			{
				return null;
			}

			return new {
				id = mocFork.id,
				name = mocFork.id,
				description = mocFork.id
			 };
		}
	}
}
