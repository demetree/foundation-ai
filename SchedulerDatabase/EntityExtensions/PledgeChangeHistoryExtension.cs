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
	public partial class PledgeChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)pledgeId; }
			set { pledgeId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PledgeChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 pledgeId { get; set; }
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
		public class PledgeChangeHistoryOutputDTO : PledgeChangeHistoryDTO
		{
			public Pledge.PledgeDTO pledge { get; set; }
		}


		/// <summary>
		///
		/// Converts a PledgeChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PledgeChangeHistoryDTO ToDTO()
		{
			return new PledgeChangeHistoryDTO
			{
				id = this.id,
				pledgeId = this.pledgeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a PledgeChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PledgeChangeHistoryDTO> ToDTOList(List<PledgeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PledgeChangeHistoryDTO> output = new List<PledgeChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (PledgeChangeHistory pledgeChangeHistory in data)
			{
				output.Add(pledgeChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PledgeChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PledgeChangeHistory Entity type directly.
		///
		/// </summary>
		public PledgeChangeHistoryOutputDTO ToOutputDTO()
		{
			return new PledgeChangeHistoryOutputDTO
			{
				id = this.id,
				pledgeId = this.pledgeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				pledge = this.pledge?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a PledgeChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of PledgeChangeHistory objects to avoid using the PledgeChangeHistory entity type directly.
		///
		/// </summary>
		public static List<PledgeChangeHistoryOutputDTO> ToOutputDTOList(List<PledgeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PledgeChangeHistoryOutputDTO> output = new List<PledgeChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (PledgeChangeHistory pledgeChangeHistory in data)
			{
				output.Add(pledgeChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PledgeChangeHistory Object.
		///
		/// </summary>
		public static Database.PledgeChangeHistory FromDTO(PledgeChangeHistoryDTO dto)
		{
			return new Database.PledgeChangeHistory
			{
				id = dto.id,
				pledgeId = dto.pledgeId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PledgeChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(PledgeChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.pledgeId = dto.pledgeId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a PledgeChangeHistory Object.
		///
		/// </summary>
		public PledgeChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PledgeChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				pledgeId = this.pledgeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PledgeChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PledgeChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PledgeChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PledgeChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PledgeChangeHistory pledgeChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (pledgeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = pledgeChangeHistory.id,
				pledgeId = pledgeChangeHistory.pledgeId,
				versionNumber = pledgeChangeHistory.versionNumber,
				timeStamp = pledgeChangeHistory.timeStamp,
				userId = pledgeChangeHistory.userId,
				data = pledgeChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PledgeChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PledgeChangeHistory pledgeChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (pledgeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = pledgeChangeHistory.id,
				pledgeId = pledgeChangeHistory.pledgeId,
				versionNumber = pledgeChangeHistory.versionNumber,
				timeStamp = pledgeChangeHistory.timeStamp,
				userId = pledgeChangeHistory.userId,
				data = pledgeChangeHistory.data,
				pledge = Pledge.CreateMinimalAnonymous(pledgeChangeHistory.pledge)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PledgeChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PledgeChangeHistory pledgeChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (pledgeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = pledgeChangeHistory.id,
				name = pledgeChangeHistory.id,
				description = pledgeChangeHistory.id
			 };
		}
	}
}
