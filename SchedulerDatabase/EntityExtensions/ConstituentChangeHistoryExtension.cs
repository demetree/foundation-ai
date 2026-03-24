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
	public partial class ConstituentChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)constituentId; }
			set { constituentId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ConstituentChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 constituentId { get; set; }
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
		public class ConstituentChangeHistoryOutputDTO : ConstituentChangeHistoryDTO
		{
			public Constituent.ConstituentDTO constituent { get; set; }
		}


		/// <summary>
		///
		/// Converts a ConstituentChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ConstituentChangeHistoryDTO ToDTO()
		{
			return new ConstituentChangeHistoryDTO
			{
				id = this.id,
				constituentId = this.constituentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ConstituentChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ConstituentChangeHistoryDTO> ToDTOList(List<ConstituentChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConstituentChangeHistoryDTO> output = new List<ConstituentChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ConstituentChangeHistory constituentChangeHistory in data)
			{
				output.Add(constituentChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ConstituentChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ConstituentChangeHistory Entity type directly.
		///
		/// </summary>
		public ConstituentChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ConstituentChangeHistoryOutputDTO
			{
				id = this.id,
				constituentId = this.constituentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				constituent = this.constituent?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ConstituentChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ConstituentChangeHistory objects to avoid using the ConstituentChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ConstituentChangeHistoryOutputDTO> ToOutputDTOList(List<ConstituentChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ConstituentChangeHistoryOutputDTO> output = new List<ConstituentChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ConstituentChangeHistory constituentChangeHistory in data)
			{
				output.Add(constituentChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ConstituentChangeHistory Object.
		///
		/// </summary>
		public static Database.ConstituentChangeHistory FromDTO(ConstituentChangeHistoryDTO dto)
		{
			return new Database.ConstituentChangeHistory
			{
				id = dto.id,
				constituentId = dto.constituentId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ConstituentChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ConstituentChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.constituentId = dto.constituentId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ConstituentChangeHistory Object.
		///
		/// </summary>
		public ConstituentChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ConstituentChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				constituentId = this.constituentId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConstituentChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ConstituentChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ConstituentChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ConstituentChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ConstituentChangeHistory constituentChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (constituentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = constituentChangeHistory.id,
				constituentId = constituentChangeHistory.constituentId,
				versionNumber = constituentChangeHistory.versionNumber,
				timeStamp = constituentChangeHistory.timeStamp,
				userId = constituentChangeHistory.userId,
				data = constituentChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ConstituentChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ConstituentChangeHistory constituentChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (constituentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = constituentChangeHistory.id,
				constituentId = constituentChangeHistory.constituentId,
				versionNumber = constituentChangeHistory.versionNumber,
				timeStamp = constituentChangeHistory.timeStamp,
				userId = constituentChangeHistory.userId,
				data = constituentChangeHistory.data,
				constituent = Constituent.CreateMinimalAnonymous(constituentChangeHistory.constituent)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ConstituentChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ConstituentChangeHistory constituentChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (constituentChangeHistory == null)
			{
				return null;
			}

			return new {
				id = constituentChangeHistory.id,
				name = constituentChangeHistory.id,
				description = constituentChangeHistory.id
			 };
		}
	}
}
