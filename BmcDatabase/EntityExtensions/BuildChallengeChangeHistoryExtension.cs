using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.BMC.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class BuildChallengeChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)buildChallengeId; }
			set { buildChallengeId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BuildChallengeChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 buildChallengeId { get; set; }
			public Int32 versionNumber { get; set; }
			[Required]
			public DateTime timeStamp { get; set; }
			[Required]
			public Int32 userId { get; set; }
			[Required]
			public String data { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class BuildChallengeChangeHistoryOutputDTO : BuildChallengeChangeHistoryDTO
		{
			public BuildChallenge.BuildChallengeDTO buildChallenge { get; set; }
		}


		/// <summary>
		///
		/// Converts a BuildChallengeChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BuildChallengeChangeHistoryDTO ToDTO()
		{
			return new BuildChallengeChangeHistoryDTO
			{
				id = this.id,
				buildChallengeId = this.buildChallengeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a BuildChallengeChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BuildChallengeChangeHistoryDTO> ToDTOList(List<BuildChallengeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildChallengeChangeHistoryDTO> output = new List<BuildChallengeChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (BuildChallengeChangeHistory buildChallengeChangeHistory in data)
			{
				output.Add(buildChallengeChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BuildChallengeChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BuildChallengeChangeHistoryEntity type directly.
		///
		/// </summary>
		public BuildChallengeChangeHistoryOutputDTO ToOutputDTO()
		{
			return new BuildChallengeChangeHistoryOutputDTO
			{
				id = this.id,
				buildChallengeId = this.buildChallengeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				buildChallenge = this.buildChallenge?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BuildChallengeChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of BuildChallengeChangeHistory objects to avoid using the BuildChallengeChangeHistory entity type directly.
		///
		/// </summary>
		public static List<BuildChallengeChangeHistoryOutputDTO> ToOutputDTOList(List<BuildChallengeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BuildChallengeChangeHistoryOutputDTO> output = new List<BuildChallengeChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (BuildChallengeChangeHistory buildChallengeChangeHistory in data)
			{
				output.Add(buildChallengeChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BuildChallengeChangeHistory Object.
		///
		/// </summary>
		public static Database.BuildChallengeChangeHistory FromDTO(BuildChallengeChangeHistoryDTO dto)
		{
			return new Database.BuildChallengeChangeHistory
			{
				id = dto.id,
				buildChallengeId = dto.buildChallengeId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BuildChallengeChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(BuildChallengeChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.buildChallengeId = dto.buildChallengeId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a BuildChallengeChangeHistory Object.
		///
		/// </summary>
		public BuildChallengeChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BuildChallengeChangeHistory{
				id = this.id,
				buildChallengeId = this.buildChallengeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildChallengeChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BuildChallengeChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BuildChallengeChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BuildChallengeChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BuildChallengeChangeHistory buildChallengeChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (buildChallengeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildChallengeChangeHistory.id,
				buildChallengeId = buildChallengeChangeHistory.buildChallengeId,
				versionNumber = buildChallengeChangeHistory.versionNumber,
				timeStamp = buildChallengeChangeHistory.timeStamp,
				userId = buildChallengeChangeHistory.userId,
				data = buildChallengeChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BuildChallengeChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BuildChallengeChangeHistory buildChallengeChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (buildChallengeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildChallengeChangeHistory.id,
				buildChallengeId = buildChallengeChangeHistory.buildChallengeId,
				versionNumber = buildChallengeChangeHistory.versionNumber,
				timeStamp = buildChallengeChangeHistory.timeStamp,
				userId = buildChallengeChangeHistory.userId,
				data = buildChallengeChangeHistory.data,
				buildChallenge = BuildChallenge.CreateMinimalAnonymous(buildChallengeChangeHistory.buildChallenge),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BuildChallengeChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BuildChallengeChangeHistory buildChallengeChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (buildChallengeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = buildChallengeChangeHistory.id,
				name = buildChallengeChangeHistory.id,
				description = buildChallengeChangeHistory.id
			 };
		}
	}
}
