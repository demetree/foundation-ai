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
	public partial class BrickSetSetReview : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BrickSetSetReviewDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 legoSetId { get; set; }
			[Required]
			public String reviewAuthor { get; set; }
			public DateTime? reviewDate { get; set; }
			public String reviewTitle { get; set; }
			public String reviewBody { get; set; }
			public Int32? overallRating { get; set; }
			public Int32? buildingExperienceRating { get; set; }
			public Int32? valueForMoneyRating { get; set; }
			public Int32? partsRating { get; set; }
			public Int32? playabilityRating { get; set; }
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
		public class BrickSetSetReviewOutputDTO : BrickSetSetReviewDTO
		{
			public LegoSet.LegoSetDTO legoSet { get; set; }
		}


		/// <summary>
		///
		/// Converts a BrickSetSetReview to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BrickSetSetReviewDTO ToDTO()
		{
			return new BrickSetSetReviewDTO
			{
				id = this.id,
				legoSetId = this.legoSetId,
				reviewAuthor = this.reviewAuthor,
				reviewDate = this.reviewDate,
				reviewTitle = this.reviewTitle,
				reviewBody = this.reviewBody,
				overallRating = this.overallRating,
				buildingExperienceRating = this.buildingExperienceRating,
				valueForMoneyRating = this.valueForMoneyRating,
				partsRating = this.partsRating,
				playabilityRating = this.playabilityRating,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BrickSetSetReview list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BrickSetSetReviewDTO> ToDTOList(List<BrickSetSetReview> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickSetSetReviewDTO> output = new List<BrickSetSetReviewDTO>();

			output.Capacity = data.Count;

			foreach (BrickSetSetReview brickSetSetReview in data)
			{
				output.Add(brickSetSetReview.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BrickSetSetReview to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BrickSetSetReviewEntity type directly.
		///
		/// </summary>
		public BrickSetSetReviewOutputDTO ToOutputDTO()
		{
			return new BrickSetSetReviewOutputDTO
			{
				id = this.id,
				legoSetId = this.legoSetId,
				reviewAuthor = this.reviewAuthor,
				reviewDate = this.reviewDate,
				reviewTitle = this.reviewTitle,
				reviewBody = this.reviewBody,
				overallRating = this.overallRating,
				buildingExperienceRating = this.buildingExperienceRating,
				valueForMoneyRating = this.valueForMoneyRating,
				partsRating = this.partsRating,
				playabilityRating = this.playabilityRating,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				legoSet = this.legoSet?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BrickSetSetReview list to list of Output Data Transfer Object intended to be used for serializing a list of BrickSetSetReview objects to avoid using the BrickSetSetReview entity type directly.
		///
		/// </summary>
		public static List<BrickSetSetReviewOutputDTO> ToOutputDTOList(List<BrickSetSetReview> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BrickSetSetReviewOutputDTO> output = new List<BrickSetSetReviewOutputDTO>();

			output.Capacity = data.Count;

			foreach (BrickSetSetReview brickSetSetReview in data)
			{
				output.Add(brickSetSetReview.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BrickSetSetReview Object.
		///
		/// </summary>
		public static Database.BrickSetSetReview FromDTO(BrickSetSetReviewDTO dto)
		{
			return new Database.BrickSetSetReview
			{
				id = dto.id,
				legoSetId = dto.legoSetId,
				reviewAuthor = dto.reviewAuthor,
				reviewDate = dto.reviewDate,
				reviewTitle = dto.reviewTitle,
				reviewBody = dto.reviewBody,
				overallRating = dto.overallRating,
				buildingExperienceRating = dto.buildingExperienceRating,
				valueForMoneyRating = dto.valueForMoneyRating,
				partsRating = dto.partsRating,
				playabilityRating = dto.playabilityRating,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BrickSetSetReview Object.
		///
		/// </summary>
		public void ApplyDTO(BrickSetSetReviewDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.legoSetId = dto.legoSetId;
			this.reviewAuthor = dto.reviewAuthor;
			this.reviewDate = dto.reviewDate;
			this.reviewTitle = dto.reviewTitle;
			this.reviewBody = dto.reviewBody;
			this.overallRating = dto.overallRating;
			this.buildingExperienceRating = dto.buildingExperienceRating;
			this.valueForMoneyRating = dto.valueForMoneyRating;
			this.partsRating = dto.partsRating;
			this.playabilityRating = dto.playabilityRating;
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
		/// Creates a deep copy clone of a BrickSetSetReview Object.
		///
		/// </summary>
		public BrickSetSetReview Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BrickSetSetReview{
				id = this.id,
				legoSetId = this.legoSetId,
				reviewAuthor = this.reviewAuthor,
				reviewDate = this.reviewDate,
				reviewTitle = this.reviewTitle,
				reviewBody = this.reviewBody,
				overallRating = this.overallRating,
				buildingExperienceRating = this.buildingExperienceRating,
				valueForMoneyRating = this.valueForMoneyRating,
				partsRating = this.partsRating,
				playabilityRating = this.playabilityRating,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickSetSetReview Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BrickSetSetReview Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BrickSetSetReview Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BrickSetSetReview Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BrickSetSetReview brickSetSetReview)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (brickSetSetReview == null)
			{
				return null;
			}

			return new {
				id = brickSetSetReview.id,
				legoSetId = brickSetSetReview.legoSetId,
				reviewAuthor = brickSetSetReview.reviewAuthor,
				reviewDate = brickSetSetReview.reviewDate,
				reviewTitle = brickSetSetReview.reviewTitle,
				reviewBody = brickSetSetReview.reviewBody,
				overallRating = brickSetSetReview.overallRating,
				buildingExperienceRating = brickSetSetReview.buildingExperienceRating,
				valueForMoneyRating = brickSetSetReview.valueForMoneyRating,
				partsRating = brickSetSetReview.partsRating,
				playabilityRating = brickSetSetReview.playabilityRating,
				objectGuid = brickSetSetReview.objectGuid,
				active = brickSetSetReview.active,
				deleted = brickSetSetReview.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BrickSetSetReview Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BrickSetSetReview brickSetSetReview)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (brickSetSetReview == null)
			{
				return null;
			}

			return new {
				id = brickSetSetReview.id,
				legoSetId = brickSetSetReview.legoSetId,
				reviewAuthor = brickSetSetReview.reviewAuthor,
				reviewDate = brickSetSetReview.reviewDate,
				reviewTitle = brickSetSetReview.reviewTitle,
				reviewBody = brickSetSetReview.reviewBody,
				overallRating = brickSetSetReview.overallRating,
				buildingExperienceRating = brickSetSetReview.buildingExperienceRating,
				valueForMoneyRating = brickSetSetReview.valueForMoneyRating,
				partsRating = brickSetSetReview.partsRating,
				playabilityRating = brickSetSetReview.playabilityRating,
				objectGuid = brickSetSetReview.objectGuid,
				active = brickSetSetReview.active,
				deleted = brickSetSetReview.deleted,
				legoSet = LegoSet.CreateMinimalAnonymous(brickSetSetReview.legoSet)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BrickSetSetReview Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BrickSetSetReview brickSetSetReview)
		{
			//
			// Return a very minimal object.
			//
			if (brickSetSetReview == null)
			{
				return null;
			}

			return new {
				id = brickSetSetReview.id,
				name = brickSetSetReview.reviewAuthor,
				description = string.Join(", ", new[] { brickSetSetReview.reviewAuthor}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
