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
	public partial class LegoMinifig : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class LegoMinifigDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String figNumber { get; set; }
			[Required]
			public Int32 partCount { get; set; }
			public String imageUrl { get; set; }
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
		public class LegoMinifigOutputDTO : LegoMinifigDTO
		{
		}


		/// <summary>
		///
		/// Converts a LegoMinifig to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public LegoMinifigDTO ToDTO()
		{
			return new LegoMinifigDTO
			{
				id = this.id,
				name = this.name,
				figNumber = this.figNumber,
				partCount = this.partCount,
				imageUrl = this.imageUrl,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a LegoMinifig list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<LegoMinifigDTO> ToDTOList(List<LegoMinifig> data)
		{
			if (data == null)
			{
				return null;
			}

			List<LegoMinifigDTO> output = new List<LegoMinifigDTO>();

			output.Capacity = data.Count;

			foreach (LegoMinifig legoMinifig in data)
			{
				output.Add(legoMinifig.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a LegoMinifig to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the LegoMinifigEntity type directly.
		///
		/// </summary>
		public LegoMinifigOutputDTO ToOutputDTO()
		{
			return new LegoMinifigOutputDTO
			{
				id = this.id,
				name = this.name,
				figNumber = this.figNumber,
				partCount = this.partCount,
				imageUrl = this.imageUrl,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a LegoMinifig list to list of Output Data Transfer Object intended to be used for serializing a list of LegoMinifig objects to avoid using the LegoMinifig entity type directly.
		///
		/// </summary>
		public static List<LegoMinifigOutputDTO> ToOutputDTOList(List<LegoMinifig> data)
		{
			if (data == null)
			{
				return null;
			}

			List<LegoMinifigOutputDTO> output = new List<LegoMinifigOutputDTO>();

			output.Capacity = data.Count;

			foreach (LegoMinifig legoMinifig in data)
			{
				output.Add(legoMinifig.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a LegoMinifig Object.
		///
		/// </summary>
		public static Database.LegoMinifig FromDTO(LegoMinifigDTO dto)
		{
			return new Database.LegoMinifig
			{
				id = dto.id,
				name = dto.name,
				figNumber = dto.figNumber,
				partCount = dto.partCount,
				imageUrl = dto.imageUrl,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a LegoMinifig Object.
		///
		/// </summary>
		public void ApplyDTO(LegoMinifigDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.figNumber = dto.figNumber;
			this.partCount = dto.partCount;
			this.imageUrl = dto.imageUrl;
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
		/// Creates a deep copy clone of a LegoMinifig Object.
		///
		/// </summary>
		public LegoMinifig Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new LegoMinifig{
				id = this.id,
				name = this.name,
				figNumber = this.figNumber,
				partCount = this.partCount,
				imageUrl = this.imageUrl,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a LegoMinifig Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a LegoMinifig Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a LegoMinifig Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a LegoMinifig Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.LegoMinifig legoMinifig)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (legoMinifig == null)
			{
				return null;
			}

			return new {
				id = legoMinifig.id,
				name = legoMinifig.name,
				figNumber = legoMinifig.figNumber,
				partCount = legoMinifig.partCount,
				imageUrl = legoMinifig.imageUrl,
				objectGuid = legoMinifig.objectGuid,
				active = legoMinifig.active,
				deleted = legoMinifig.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a LegoMinifig Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(LegoMinifig legoMinifig)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (legoMinifig == null)
			{
				return null;
			}

			return new {
				id = legoMinifig.id,
				name = legoMinifig.name,
				figNumber = legoMinifig.figNumber,
				partCount = legoMinifig.partCount,
				imageUrl = legoMinifig.imageUrl,
				objectGuid = legoMinifig.objectGuid,
				active = legoMinifig.active,
				deleted = legoMinifig.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a LegoMinifig Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(LegoMinifig legoMinifig)
		{
			//
			// Return a very minimal object.
			//
			if (legoMinifig == null)
			{
				return null;
			}

			return new {
				id = legoMinifig.id,
				name = legoMinifig.name,
				description = string.Join(", ", new[] { legoMinifig.name, legoMinifig.figNumber, legoMinifig.imageUrl}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
