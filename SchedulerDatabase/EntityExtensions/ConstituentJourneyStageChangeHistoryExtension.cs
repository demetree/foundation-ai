using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Scheduler.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class ConstituentJourneyStageChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)constituentJourneyStageId; }
			set { constituentJourneyStageId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ConstituentJourneyStageChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 constituentJourneyStageId { get; set; }
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
		public class ConstituentJourneyStageChangeHistoryOutputDTO : ConstituentJourneyStageChangeHistoryDTO
		{
			public ConstituentJourneyStage.ConstituentJourneyStageDTO constituentJourneyStage { get; set; }
		}


		/// <summary>
		///
		/// Converts a ConstituentJourneyStageChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ConstituentJourneyStageChangeHistoryDTO ToDTO()
		{
			return new ConstituentJourneyStageChangeHistoryDTO
			{
				id = this.id,
				constituentJourneyStageId = this.constituentJourneyStageId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ConstituentJourneyStageChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ConstituentJourneyStageChangeHistoryDTO> ToDTOList(List<ConstituentJourneyStageChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConstituentJourneyStageChangeHistoryDTO> output = new List<ConstituentJourneyStageChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ConstituentJourneyStageChangeHistory constituentJourneyStageChangeHistory in data)
			{
				output.Add(constituentJourneyStageChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ConstituentJourneyStageChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ConstituentJourneyStageChangeHistoryEntity type directly.
		///
		/// </summary>
		public ConstituentJourneyStageChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ConstituentJourneyStageChangeHistoryOutputDTO
			{
				id = this.id,
				constituentJourneyStageId = this.constituentJourneyStageId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				constituentJourneyStage = this.constituentJourneyStage?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ConstituentJourneyStageChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ConstituentJourneyStageChangeHistory objects to avoid using the ConstituentJourneyStageChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ConstituentJourneyStageChangeHistoryOutputDTO> ToOutputDTOList(List<ConstituentJourneyStageChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConstituentJourneyStageChangeHistoryOutputDTO> output = new List<ConstituentJourneyStageChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ConstituentJourneyStageChangeHistory constituentJourneyStageChangeHistory in data)
			{
				output.Add(constituentJourneyStageChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ConstituentJourneyStageChangeHistory Object.
		///
		/// </summary>
		public static Database.ConstituentJourneyStageChangeHistory FromDTO(ConstituentJourneyStageChangeHistoryDTO dto)
		{
			return new Database.ConstituentJourneyStageChangeHistory
			{
				id = dto.id,
				constituentJourneyStageId = dto.constituentJourneyStageId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ConstituentJourneyStageChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ConstituentJourneyStageChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.constituentJourneyStageId = dto.constituentJourneyStageId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ConstituentJourneyStageChangeHistory Object.
		///
		/// </summary>
		public ConstituentJourneyStageChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ConstituentJourneyStageChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				constituentJourneyStageId = this.constituentJourneyStageId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConstituentJourneyStageChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConstituentJourneyStageChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ConstituentJourneyStageChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ConstituentJourneyStageChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ConstituentJourneyStageChangeHistory constituentJourneyStageChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (constituentJourneyStageChangeHistory == null)
			{
				return null;
			}

			return new {
				id = constituentJourneyStageChangeHistory.id,
				constituentJourneyStageId = constituentJourneyStageChangeHistory.constituentJourneyStageId,
				versionNumber = constituentJourneyStageChangeHistory.versionNumber,
				timeStamp = constituentJourneyStageChangeHistory.timeStamp,
				userId = constituentJourneyStageChangeHistory.userId,
				data = constituentJourneyStageChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ConstituentJourneyStageChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ConstituentJourneyStageChangeHistory constituentJourneyStageChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (constituentJourneyStageChangeHistory == null)
			{
				return null;
			}

			return new {
				id = constituentJourneyStageChangeHistory.id,
				constituentJourneyStageId = constituentJourneyStageChangeHistory.constituentJourneyStageId,
				versionNumber = constituentJourneyStageChangeHistory.versionNumber,
				timeStamp = constituentJourneyStageChangeHistory.timeStamp,
				userId = constituentJourneyStageChangeHistory.userId,
				data = constituentJourneyStageChangeHistory.data,
				constituentJourneyStage = ConstituentJourneyStage.CreateMinimalAnonymous(constituentJourneyStageChangeHistory.constituentJourneyStage),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ConstituentJourneyStageChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ConstituentJourneyStageChangeHistory constituentJourneyStageChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (constituentJourneyStageChangeHistory == null)
			{
				return null;
			}

			return new {
				id = constituentJourneyStageChangeHistory.id,
				name = constituentJourneyStageChangeHistory.id,
				description = constituentJourneyStageChangeHistory.id
			 };
		}
	}
}
