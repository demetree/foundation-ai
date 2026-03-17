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
	public partial class BatchChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)batchId; }
			set { batchId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class BatchChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 batchId { get; set; }
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
		public class BatchChangeHistoryOutputDTO : BatchChangeHistoryDTO
		{
			public Batch.BatchDTO batch { get; set; }
		}


		/// <summary>
		///
		/// Converts a BatchChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public BatchChangeHistoryDTO ToDTO()
		{
			return new BatchChangeHistoryDTO
			{
				id = this.id,
				batchId = this.batchId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a BatchChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<BatchChangeHistoryDTO> ToDTOList(List<BatchChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BatchChangeHistoryDTO> output = new List<BatchChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (BatchChangeHistory batchChangeHistory in data)
			{
				output.Add(batchChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a BatchChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the BatchChangeHistory Entity type directly.
		///
		/// </summary>
		public BatchChangeHistoryOutputDTO ToOutputDTO()
		{
			return new BatchChangeHistoryOutputDTO
			{
				id = this.id,
				batchId = this.batchId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				batch = this.batch?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a BatchChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of BatchChangeHistory objects to avoid using the BatchChangeHistory entity type directly.
		///
		/// </summary>
		public static List<BatchChangeHistoryOutputDTO> ToOutputDTOList(List<BatchChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<BatchChangeHistoryOutputDTO> output = new List<BatchChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (BatchChangeHistory batchChangeHistory in data)
			{
				output.Add(batchChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a BatchChangeHistory Object.
		///
		/// </summary>
		public static Database.BatchChangeHistory FromDTO(BatchChangeHistoryDTO dto)
		{
			return new Database.BatchChangeHistory
			{
				id = dto.id,
				batchId = dto.batchId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a BatchChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(BatchChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.batchId = dto.batchId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a BatchChangeHistory Object.
		///
		/// </summary>
		public BatchChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new BatchChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				batchId = this.batchId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BatchChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a BatchChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a BatchChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a BatchChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.BatchChangeHistory batchChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (batchChangeHistory == null)
			{
				return null;
			}

			return new {
				id = batchChangeHistory.id,
				batchId = batchChangeHistory.batchId,
				versionNumber = batchChangeHistory.versionNumber,
				timeStamp = batchChangeHistory.timeStamp,
				userId = batchChangeHistory.userId,
				data = batchChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a BatchChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(BatchChangeHistory batchChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (batchChangeHistory == null)
			{
				return null;
			}

			return new {
				id = batchChangeHistory.id,
				batchId = batchChangeHistory.batchId,
				versionNumber = batchChangeHistory.versionNumber,
				timeStamp = batchChangeHistory.timeStamp,
				userId = batchChangeHistory.userId,
				data = batchChangeHistory.data,
				batch = Batch.CreateMinimalAnonymous(batchChangeHistory.batch),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a BatchChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(BatchChangeHistory batchChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (batchChangeHistory == null)
			{
				return null;
			}

			return new {
				id = batchChangeHistory.id,
				name = batchChangeHistory.id,
				description = batchChangeHistory.id
			 };
		}
	}
}
