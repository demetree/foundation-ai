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
	public partial class BuildChallengeEntry : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BuildChallengeEntryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 buildChallengeId { get; set; }
			[Required]
			public Int32 publishedMocId { get; set; }
			[Required]
			public DateTime submittedDate { get; set; }
			public String entryNotes { get; set; }
			[Required]
			public Int32 voteCount { get; set; }
			[Required]
			public Boolean isWinner { get; set; }
			[Required]
			public Boolean isDisqualified { get; set; }
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
		public class BuildChallengeEntryOutputDTO : BuildChallengeEntryDTO
		{
			public BuildChallenge.BuildChallengeDTO buildChallenge { get; set; }
			public PublishedMoc.PublishedMocDTO publishedMoc { get; set; }
		}


		/// <summary>
		///
		/// Converts a BuildChallengeEntry to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BuildChallengeEntryDTO ToDTO()
		{
			return new BuildChallengeEntryDTO
			{
				id = this.id,
				buildChallengeId = this.buildChallengeId,
				publishedMocId = this.publishedMocId,
				submittedDate = this.submittedDate,
				entryNotes = this.entryNotes,
				voteCount = this.voteCount,
				isWinner = this.isWinner,
				isDisqualified = this.isDisqualified,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a BuildChallengeEntry list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BuildChallengeEntryDTO> ToDTOList(List<BuildChallengeEntry> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildChallengeEntryDTO> output = new List<BuildChallengeEntryDTO>();

			output.Capacity = data.Count;

			foreach (BuildChallengeEntry buildChallengeEntry in data)
			{
				output.Add(buildChallengeEntry.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BuildChallengeEntry to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BuildChallengeEntryEntity type directly.
		///
		/// </summary>
		public BuildChallengeEntryOutputDTO ToOutputDTO()
		{
			return new BuildChallengeEntryOutputDTO
			{
				id = this.id,
				buildChallengeId = this.buildChallengeId,
				publishedMocId = this.publishedMocId,
				submittedDate = this.submittedDate,
				entryNotes = this.entryNotes,
				voteCount = this.voteCount,
				isWinner = this.isWinner,
				isDisqualified = this.isDisqualified,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				buildChallenge = this.buildChallenge?.ToDTO(),
				publishedMoc = this.publishedMoc?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BuildChallengeEntry list to list of Output Data Transfer Object intended to be used for serializing a list of BuildChallengeEntry objects to avoid using the BuildChallengeEntry entity type directly.
		///
		/// </summary>
		public static List<BuildChallengeEntryOutputDTO> ToOutputDTOList(List<BuildChallengeEntry> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildChallengeEntryOutputDTO> output = new List<BuildChallengeEntryOutputDTO>();

			output.Capacity = data.Count;

			foreach (BuildChallengeEntry buildChallengeEntry in data)
			{
				output.Add(buildChallengeEntry.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BuildChallengeEntry Object.
		///
		/// </summary>
		public static Database.BuildChallengeEntry FromDTO(BuildChallengeEntryDTO dto)
		{
			return new Database.BuildChallengeEntry
			{
				id = dto.id,
				buildChallengeId = dto.buildChallengeId,
				publishedMocId = dto.publishedMocId,
				submittedDate = dto.submittedDate,
				entryNotes = dto.entryNotes,
				voteCount = dto.voteCount,
				isWinner = dto.isWinner,
				isDisqualified = dto.isDisqualified,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BuildChallengeEntry Object.
		///
		/// </summary>
		public void ApplyDTO(BuildChallengeEntryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.buildChallengeId = dto.buildChallengeId;
			this.publishedMocId = dto.publishedMocId;
			this.submittedDate = dto.submittedDate;
			this.entryNotes = dto.entryNotes;
			this.voteCount = dto.voteCount;
			this.isWinner = dto.isWinner;
			this.isDisqualified = dto.isDisqualified;
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
		/// Creates a deep copy clone of a BuildChallengeEntry Object.
		///
		/// </summary>
		public BuildChallengeEntry Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BuildChallengeEntry{
				id = this.id,
				tenantGuid = this.tenantGuid,
				buildChallengeId = this.buildChallengeId,
				publishedMocId = this.publishedMocId,
				submittedDate = this.submittedDate,
				entryNotes = this.entryNotes,
				voteCount = this.voteCount,
				isWinner = this.isWinner,
				isDisqualified = this.isDisqualified,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildChallengeEntry Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildChallengeEntry Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BuildChallengeEntry Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BuildChallengeEntry Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BuildChallengeEntry buildChallengeEntry)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (buildChallengeEntry == null)
			{
				return null;
			}

			return new {
				id = buildChallengeEntry.id,
				buildChallengeId = buildChallengeEntry.buildChallengeId,
				publishedMocId = buildChallengeEntry.publishedMocId,
				submittedDate = buildChallengeEntry.submittedDate,
				entryNotes = buildChallengeEntry.entryNotes,
				voteCount = buildChallengeEntry.voteCount,
				isWinner = buildChallengeEntry.isWinner,
				isDisqualified = buildChallengeEntry.isDisqualified,
				objectGuid = buildChallengeEntry.objectGuid,
				active = buildChallengeEntry.active,
				deleted = buildChallengeEntry.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BuildChallengeEntry Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BuildChallengeEntry buildChallengeEntry)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (buildChallengeEntry == null)
			{
				return null;
			}

			return new {
				id = buildChallengeEntry.id,
				buildChallengeId = buildChallengeEntry.buildChallengeId,
				publishedMocId = buildChallengeEntry.publishedMocId,
				submittedDate = buildChallengeEntry.submittedDate,
				entryNotes = buildChallengeEntry.entryNotes,
				voteCount = buildChallengeEntry.voteCount,
				isWinner = buildChallengeEntry.isWinner,
				isDisqualified = buildChallengeEntry.isDisqualified,
				objectGuid = buildChallengeEntry.objectGuid,
				active = buildChallengeEntry.active,
				deleted = buildChallengeEntry.deleted,
				buildChallenge = BuildChallenge.CreateMinimalAnonymous(buildChallengeEntry.buildChallenge),
				publishedMoc = PublishedMoc.CreateMinimalAnonymous(buildChallengeEntry.publishedMoc)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BuildChallengeEntry Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BuildChallengeEntry buildChallengeEntry)
		{
			//
			// Return a very minimal object.
			//
			if (buildChallengeEntry == null)
			{
				return null;
			}

			return new {
				id = buildChallengeEntry.id,
				name = buildChallengeEntry.id,
				description = buildChallengeEntry.id
			 };
		}
	}
}
