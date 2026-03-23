using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.DeepSpace.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class ReplicationTargetChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)replicationTargetId; }
			set { replicationTargetId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class ReplicationTargetChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 replicationTargetId { get; set; }
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
		public class ReplicationTargetChangeHistoryOutputDTO : ReplicationTargetChangeHistoryDTO
		{
			public ReplicationTarget.ReplicationTargetDTO replicationTarget { get; set; }
		}


		/// <summary>
		///
		/// Converts a ReplicationTargetChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public ReplicationTargetChangeHistoryDTO ToDTO()
		{
			return new ReplicationTargetChangeHistoryDTO
			{
				id = this.id,
				replicationTargetId = this.replicationTargetId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a ReplicationTargetChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<ReplicationTargetChangeHistoryDTO> ToDTOList(List<ReplicationTargetChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ReplicationTargetChangeHistoryDTO> output = new List<ReplicationTargetChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (ReplicationTargetChangeHistory replicationTargetChangeHistory in data)
			{
				output.Add(replicationTargetChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a ReplicationTargetChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the ReplicationTargetChangeHistory Entity type directly.
		///
		/// </summary>
		public ReplicationTargetChangeHistoryOutputDTO ToOutputDTO()
		{
			return new ReplicationTargetChangeHistoryOutputDTO
			{
				id = this.id,
				replicationTargetId = this.replicationTargetId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				replicationTarget = this.replicationTarget?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a ReplicationTargetChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of ReplicationTargetChangeHistory objects to avoid using the ReplicationTargetChangeHistory entity type directly.
		///
		/// </summary>
		public static List<ReplicationTargetChangeHistoryOutputDTO> ToOutputDTOList(List<ReplicationTargetChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<ReplicationTargetChangeHistoryOutputDTO> output = new List<ReplicationTargetChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (ReplicationTargetChangeHistory replicationTargetChangeHistory in data)
			{
				output.Add(replicationTargetChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a ReplicationTargetChangeHistory Object.
		///
		/// </summary>
		public static Database.ReplicationTargetChangeHistory FromDTO(ReplicationTargetChangeHistoryDTO dto)
		{
			return new Database.ReplicationTargetChangeHistory
			{
				id = dto.id,
				replicationTargetId = dto.replicationTargetId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a ReplicationTargetChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(ReplicationTargetChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.replicationTargetId = dto.replicationTargetId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a ReplicationTargetChangeHistory Object.
		///
		/// </summary>
		public ReplicationTargetChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new ReplicationTargetChangeHistory{
				id = this.id,
				replicationTargetId = this.replicationTargetId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ReplicationTargetChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a ReplicationTargetChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a ReplicationTargetChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a ReplicationTargetChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.ReplicationTargetChangeHistory replicationTargetChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (replicationTargetChangeHistory == null)
			{
				return null;
			}

			return new {
				id = replicationTargetChangeHistory.id,
				replicationTargetId = replicationTargetChangeHistory.replicationTargetId,
				versionNumber = replicationTargetChangeHistory.versionNumber,
				timeStamp = replicationTargetChangeHistory.timeStamp,
				userId = replicationTargetChangeHistory.userId,
				data = replicationTargetChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a ReplicationTargetChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(ReplicationTargetChangeHistory replicationTargetChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (replicationTargetChangeHistory == null)
			{
				return null;
			}

			return new {
				id = replicationTargetChangeHistory.id,
				replicationTargetId = replicationTargetChangeHistory.replicationTargetId,
				versionNumber = replicationTargetChangeHistory.versionNumber,
				timeStamp = replicationTargetChangeHistory.timeStamp,
				userId = replicationTargetChangeHistory.userId,
				data = replicationTargetChangeHistory.data,
				replicationTarget = ReplicationTarget.CreateMinimalAnonymous(replicationTargetChangeHistory.replicationTarget)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a ReplicationTargetChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(ReplicationTargetChangeHistory replicationTargetChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (replicationTargetChangeHistory == null)
			{
				return null;
			}

			return new {
				id = replicationTargetChangeHistory.id,
				name = replicationTargetChangeHistory.id,
				description = replicationTargetChangeHistory.id
			 };
		}
	}
}
