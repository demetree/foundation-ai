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
	public partial class LegoSet : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class LegoSetDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			[Required]
			public String setNumber { get; set; }
			[Required]
			public Int32 year { get; set; }
			[Required]
			public Int32 partCount { get; set; }
			public Int32? legoThemeId { get; set; }
			public String imageUrl { get; set; }
			public String brickLinkUrl { get; set; }
			public String rebrickableUrl { get; set; }
			public String rebrickableSetNum { get; set; }
			public DateTime? lastModifiedDate { get; set; }
			public Int32? brickSetId { get; set; }
			public String brickSetUrl { get; set; }
			public Decimal? retailPriceUS { get; set; }
			public Decimal? retailPriceUK { get; set; }
			public Decimal? retailPriceCA { get; set; }
			public Decimal? retailPriceEU { get; set; }
			public String instructionsUrl { get; set; }
			public String subtheme { get; set; }
			public String availability { get; set; }
			public Int32? minifigCount { get; set; }
			public Single? brickSetRating { get; set; }
			public Int32? brickSetReviewCount { get; set; }
			public DateTime? brickSetLastEnrichedDate { get; set; }
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
		public class LegoSetOutputDTO : LegoSetDTO
		{
			public LegoTheme.LegoThemeDTO legoTheme { get; set; }
		}


		/// <summary>
		///
		/// Converts a LegoSet to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public LegoSetDTO ToDTO()
		{
			return new LegoSetDTO
			{
				id = this.id,
				name = this.name,
				setNumber = this.setNumber,
				year = this.year,
				partCount = this.partCount,
				legoThemeId = this.legoThemeId,
				imageUrl = this.imageUrl,
				brickLinkUrl = this.brickLinkUrl,
				rebrickableUrl = this.rebrickableUrl,
				rebrickableSetNum = this.rebrickableSetNum,
				lastModifiedDate = this.lastModifiedDate,
				brickSetId = this.brickSetId,
				brickSetUrl = this.brickSetUrl,
				retailPriceUS = this.retailPriceUS,
				retailPriceUK = this.retailPriceUK,
				retailPriceCA = this.retailPriceCA,
				retailPriceEU = this.retailPriceEU,
				instructionsUrl = this.instructionsUrl,
				subtheme = this.subtheme,
				availability = this.availability,
				minifigCount = this.minifigCount,
				brickSetRating = this.brickSetRating,
				brickSetReviewCount = this.brickSetReviewCount,
				brickSetLastEnrichedDate = this.brickSetLastEnrichedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a LegoSet list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<LegoSetDTO> ToDTOList(List<LegoSet> data)
		{
			if (data == null)
			{
				return null;
			}

			List<LegoSetDTO> output = new List<LegoSetDTO>();

			output.Capacity = data.Count;

			foreach (LegoSet legoSet in data)
			{
				output.Add(legoSet.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a LegoSet to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the LegoSetEntity type directly.
		///
		/// </summary>
		public LegoSetOutputDTO ToOutputDTO()
		{
			return new LegoSetOutputDTO
			{
				id = this.id,
				name = this.name,
				setNumber = this.setNumber,
				year = this.year,
				partCount = this.partCount,
				legoThemeId = this.legoThemeId,
				imageUrl = this.imageUrl,
				brickLinkUrl = this.brickLinkUrl,
				rebrickableUrl = this.rebrickableUrl,
				rebrickableSetNum = this.rebrickableSetNum,
				lastModifiedDate = this.lastModifiedDate,
				brickSetId = this.brickSetId,
				brickSetUrl = this.brickSetUrl,
				retailPriceUS = this.retailPriceUS,
				retailPriceUK = this.retailPriceUK,
				retailPriceCA = this.retailPriceCA,
				retailPriceEU = this.retailPriceEU,
				instructionsUrl = this.instructionsUrl,
				subtheme = this.subtheme,
				availability = this.availability,
				minifigCount = this.minifigCount,
				brickSetRating = this.brickSetRating,
				brickSetReviewCount = this.brickSetReviewCount,
				brickSetLastEnrichedDate = this.brickSetLastEnrichedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				legoTheme = this.legoTheme?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a LegoSet list to list of Output Data Transfer Object intended to be used for serializing a list of LegoSet objects to avoid using the LegoSet entity type directly.
		///
		/// </summary>
		public static List<LegoSetOutputDTO> ToOutputDTOList(List<LegoSet> data)
		{
			if (data == null)
			{
				return null;
			}

			List<LegoSetOutputDTO> output = new List<LegoSetOutputDTO>();

			output.Capacity = data.Count;

			foreach (LegoSet legoSet in data)
			{
				output.Add(legoSet.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a LegoSet Object.
		///
		/// </summary>
		public static Database.LegoSet FromDTO(LegoSetDTO dto)
		{
			return new Database.LegoSet
			{
				id = dto.id,
				name = dto.name,
				setNumber = dto.setNumber,
				year = dto.year,
				partCount = dto.partCount,
				legoThemeId = dto.legoThemeId,
				imageUrl = dto.imageUrl,
				brickLinkUrl = dto.brickLinkUrl,
				rebrickableUrl = dto.rebrickableUrl,
				rebrickableSetNum = dto.rebrickableSetNum,
				lastModifiedDate = dto.lastModifiedDate,
				brickSetId = dto.brickSetId,
				brickSetUrl = dto.brickSetUrl,
				retailPriceUS = dto.retailPriceUS,
				retailPriceUK = dto.retailPriceUK,
				retailPriceCA = dto.retailPriceCA,
				retailPriceEU = dto.retailPriceEU,
				instructionsUrl = dto.instructionsUrl,
				subtheme = dto.subtheme,
				availability = dto.availability,
				minifigCount = dto.minifigCount,
				brickSetRating = dto.brickSetRating,
				brickSetReviewCount = dto.brickSetReviewCount,
				brickSetLastEnrichedDate = dto.brickSetLastEnrichedDate,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a LegoSet Object.
		///
		/// </summary>
		public void ApplyDTO(LegoSetDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.setNumber = dto.setNumber;
			this.year = dto.year;
			this.partCount = dto.partCount;
			this.legoThemeId = dto.legoThemeId;
			this.imageUrl = dto.imageUrl;
			this.brickLinkUrl = dto.brickLinkUrl;
			this.rebrickableUrl = dto.rebrickableUrl;
			this.rebrickableSetNum = dto.rebrickableSetNum;
			this.lastModifiedDate = dto.lastModifiedDate;
			this.brickSetId = dto.brickSetId;
			this.brickSetUrl = dto.brickSetUrl;
			this.retailPriceUS = dto.retailPriceUS;
			this.retailPriceUK = dto.retailPriceUK;
			this.retailPriceCA = dto.retailPriceCA;
			this.retailPriceEU = dto.retailPriceEU;
			this.instructionsUrl = dto.instructionsUrl;
			this.subtheme = dto.subtheme;
			this.availability = dto.availability;
			this.minifigCount = dto.minifigCount;
			this.brickSetRating = dto.brickSetRating;
			this.brickSetReviewCount = dto.brickSetReviewCount;
			this.brickSetLastEnrichedDate = dto.brickSetLastEnrichedDate;
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
		/// Creates a deep copy clone of a LegoSet Object.
		///
		/// </summary>
		public LegoSet Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new LegoSet{
				id = this.id,
				name = this.name,
				setNumber = this.setNumber,
				year = this.year,
				partCount = this.partCount,
				legoThemeId = this.legoThemeId,
				imageUrl = this.imageUrl,
				brickLinkUrl = this.brickLinkUrl,
				rebrickableUrl = this.rebrickableUrl,
				rebrickableSetNum = this.rebrickableSetNum,
				lastModifiedDate = this.lastModifiedDate,
				brickSetId = this.brickSetId,
				brickSetUrl = this.brickSetUrl,
				retailPriceUS = this.retailPriceUS,
				retailPriceUK = this.retailPriceUK,
				retailPriceCA = this.retailPriceCA,
				retailPriceEU = this.retailPriceEU,
				instructionsUrl = this.instructionsUrl,
				subtheme = this.subtheme,
				availability = this.availability,
				minifigCount = this.minifigCount,
				brickSetRating = this.brickSetRating,
				brickSetReviewCount = this.brickSetReviewCount,
				brickSetLastEnrichedDate = this.brickSetLastEnrichedDate,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a LegoSet Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a LegoSet Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a LegoSet Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a LegoSet Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.LegoSet legoSet)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (legoSet == null)
			{
				return null;
			}

			return new {
				id = legoSet.id,
				name = legoSet.name,
				setNumber = legoSet.setNumber,
				year = legoSet.year,
				partCount = legoSet.partCount,
				legoThemeId = legoSet.legoThemeId,
				imageUrl = legoSet.imageUrl,
				brickLinkUrl = legoSet.brickLinkUrl,
				rebrickableUrl = legoSet.rebrickableUrl,
				rebrickableSetNum = legoSet.rebrickableSetNum,
				lastModifiedDate = legoSet.lastModifiedDate,
				brickSetId = legoSet.brickSetId,
				brickSetUrl = legoSet.brickSetUrl,
				retailPriceUS = legoSet.retailPriceUS,
				retailPriceUK = legoSet.retailPriceUK,
				retailPriceCA = legoSet.retailPriceCA,
				retailPriceEU = legoSet.retailPriceEU,
				instructionsUrl = legoSet.instructionsUrl,
				subtheme = legoSet.subtheme,
				availability = legoSet.availability,
				minifigCount = legoSet.minifigCount,
				brickSetRating = legoSet.brickSetRating,
				brickSetReviewCount = legoSet.brickSetReviewCount,
				brickSetLastEnrichedDate = legoSet.brickSetLastEnrichedDate,
				objectGuid = legoSet.objectGuid,
				active = legoSet.active,
				deleted = legoSet.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a LegoSet Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(LegoSet legoSet)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (legoSet == null)
			{
				return null;
			}

			return new {
				id = legoSet.id,
				name = legoSet.name,
				setNumber = legoSet.setNumber,
				year = legoSet.year,
				partCount = legoSet.partCount,
				legoThemeId = legoSet.legoThemeId,
				imageUrl = legoSet.imageUrl,
				brickLinkUrl = legoSet.brickLinkUrl,
				rebrickableUrl = legoSet.rebrickableUrl,
				rebrickableSetNum = legoSet.rebrickableSetNum,
				lastModifiedDate = legoSet.lastModifiedDate,
				brickSetId = legoSet.brickSetId,
				brickSetUrl = legoSet.brickSetUrl,
				retailPriceUS = legoSet.retailPriceUS,
				retailPriceUK = legoSet.retailPriceUK,
				retailPriceCA = legoSet.retailPriceCA,
				retailPriceEU = legoSet.retailPriceEU,
				instructionsUrl = legoSet.instructionsUrl,
				subtheme = legoSet.subtheme,
				availability = legoSet.availability,
				minifigCount = legoSet.minifigCount,
				brickSetRating = legoSet.brickSetRating,
				brickSetReviewCount = legoSet.brickSetReviewCount,
				brickSetLastEnrichedDate = legoSet.brickSetLastEnrichedDate,
				objectGuid = legoSet.objectGuid,
				active = legoSet.active,
				deleted = legoSet.deleted,
				legoTheme = LegoTheme.CreateMinimalAnonymous(legoSet.legoTheme)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a LegoSet Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(LegoSet legoSet)
		{
			//
			// Return a very minimal object.
			//
			if (legoSet == null)
			{
				return null;
			}

			return new {
				id = legoSet.id,
				name = legoSet.name,
				description = string.Join(", ", new[] { legoSet.name, legoSet.setNumber, legoSet.imageUrl}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
