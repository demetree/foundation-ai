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
	public partial class LegoSetMinifig : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class LegoSetMinifigDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 legoSetId { get; set; }
			[Required]
			public Int32 legoMinifigId { get; set; }
			public Int32? quantity { get; set; }
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
		public class LegoSetMinifigOutputDTO : LegoSetMinifigDTO
		{
			public LegoMinifig.LegoMinifigDTO legoMinifig { get; set; }
			public LegoSet.LegoSetDTO legoSet { get; set; }
		}


		/// <summary>
		///
		/// Converts a LegoSetMinifig to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public LegoSetMinifigDTO ToDTO()
		{
			return new LegoSetMinifigDTO
			{
				id = this.id,
				legoSetId = this.legoSetId,
				legoMinifigId = this.legoMinifigId,
				quantity = this.quantity,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a LegoSetMinifig list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<LegoSetMinifigDTO> ToDTOList(List<LegoSetMinifig> data)
		{
			if (data == null)
			{
				return null;
			}

			List<LegoSetMinifigDTO> output = new List<LegoSetMinifigDTO>();

			output.Capacity = data.Count;

			foreach (LegoSetMinifig legoSetMinifig in data)
			{
				output.Add(legoSetMinifig.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a LegoSetMinifig to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the LegoSetMinifigEntity type directly.
		///
		/// </summary>
		public LegoSetMinifigOutputDTO ToOutputDTO()
		{
			return new LegoSetMinifigOutputDTO
			{
				id = this.id,
				legoSetId = this.legoSetId,
				legoMinifigId = this.legoMinifigId,
				quantity = this.quantity,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				legoMinifig = this.legoMinifig?.ToDTO(),
				legoSet = this.legoSet?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a LegoSetMinifig list to list of Output Data Transfer Object intended to be used for serializing a list of LegoSetMinifig objects to avoid using the LegoSetMinifig entity type directly.
		///
		/// </summary>
		public static List<LegoSetMinifigOutputDTO> ToOutputDTOList(List<LegoSetMinifig> data)
		{
			if (data == null)
			{
				return null;
			}

			List<LegoSetMinifigOutputDTO> output = new List<LegoSetMinifigOutputDTO>();

			output.Capacity = data.Count;

			foreach (LegoSetMinifig legoSetMinifig in data)
			{
				output.Add(legoSetMinifig.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a LegoSetMinifig Object.
		///
		/// </summary>
		public static Database.LegoSetMinifig FromDTO(LegoSetMinifigDTO dto)
		{
			return new Database.LegoSetMinifig
			{
				id = dto.id,
				legoSetId = dto.legoSetId,
				legoMinifigId = dto.legoMinifigId,
				quantity = dto.quantity,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a LegoSetMinifig Object.
		///
		/// </summary>
		public void ApplyDTO(LegoSetMinifigDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.legoSetId = dto.legoSetId;
			this.legoMinifigId = dto.legoMinifigId;
			this.quantity = dto.quantity;
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
		/// Creates a deep copy clone of a LegoSetMinifig Object.
		///
		/// </summary>
		public LegoSetMinifig Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new LegoSetMinifig{
				id = this.id,
				legoSetId = this.legoSetId,
				legoMinifigId = this.legoMinifigId,
				quantity = this.quantity,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a LegoSetMinifig Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a LegoSetMinifig Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a LegoSetMinifig Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a LegoSetMinifig Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.LegoSetMinifig legoSetMinifig)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (legoSetMinifig == null)
			{
				return null;
			}

			return new {
				id = legoSetMinifig.id,
				legoSetId = legoSetMinifig.legoSetId,
				legoMinifigId = legoSetMinifig.legoMinifigId,
				quantity = legoSetMinifig.quantity,
				objectGuid = legoSetMinifig.objectGuid,
				active = legoSetMinifig.active,
				deleted = legoSetMinifig.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a LegoSetMinifig Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(LegoSetMinifig legoSetMinifig)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (legoSetMinifig == null)
			{
				return null;
			}

			return new {
				id = legoSetMinifig.id,
				legoSetId = legoSetMinifig.legoSetId,
				legoMinifigId = legoSetMinifig.legoMinifigId,
				quantity = legoSetMinifig.quantity,
				objectGuid = legoSetMinifig.objectGuid,
				active = legoSetMinifig.active,
				deleted = legoSetMinifig.deleted,
				legoMinifig = LegoMinifig.CreateMinimalAnonymous(legoSetMinifig.legoMinifig),
				legoSet = LegoSet.CreateMinimalAnonymous(legoSetMinifig.legoSet)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a LegoSetMinifig Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(LegoSetMinifig legoSetMinifig)
		{
			//
			// Return a very minimal object.
			//
			if (legoSetMinifig == null)
			{
				return null;
			}

			return new {
				id = legoSetMinifig.id,
				name = legoSetMinifig.id,
				description = legoSetMinifig.id
			 };
		}
	}
}
