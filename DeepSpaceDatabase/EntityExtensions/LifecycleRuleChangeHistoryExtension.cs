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
	public partial class LifecycleRuleChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)lifecycleRuleId; }
			set { lifecycleRuleId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class LifecycleRuleChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 lifecycleRuleId { get; set; }
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
		public class LifecycleRuleChangeHistoryOutputDTO : LifecycleRuleChangeHistoryDTO
		{
			public LifecycleRule.LifecycleRuleDTO lifecycleRule { get; set; }
		}


		/// <summary>
		///
		/// Converts a LifecycleRuleChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public LifecycleRuleChangeHistoryDTO ToDTO()
		{
			return new LifecycleRuleChangeHistoryDTO
			{
				id = this.id,
				lifecycleRuleId = this.lifecycleRuleId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a LifecycleRuleChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<LifecycleRuleChangeHistoryDTO> ToDTOList(List<LifecycleRuleChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<LifecycleRuleChangeHistoryDTO> output = new List<LifecycleRuleChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (LifecycleRuleChangeHistory lifecycleRuleChangeHistory in data)
			{
				output.Add(lifecycleRuleChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a LifecycleRuleChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the LifecycleRuleChangeHistory Entity type directly.
		///
		/// </summary>
		public LifecycleRuleChangeHistoryOutputDTO ToOutputDTO()
		{
			return new LifecycleRuleChangeHistoryOutputDTO
			{
				id = this.id,
				lifecycleRuleId = this.lifecycleRuleId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				lifecycleRule = this.lifecycleRule?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a LifecycleRuleChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of LifecycleRuleChangeHistory objects to avoid using the LifecycleRuleChangeHistory entity type directly.
		///
		/// </summary>
		public static List<LifecycleRuleChangeHistoryOutputDTO> ToOutputDTOList(List<LifecycleRuleChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<LifecycleRuleChangeHistoryOutputDTO> output = new List<LifecycleRuleChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (LifecycleRuleChangeHistory lifecycleRuleChangeHistory in data)
			{
				output.Add(lifecycleRuleChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a LifecycleRuleChangeHistory Object.
		///
		/// </summary>
		public static Database.LifecycleRuleChangeHistory FromDTO(LifecycleRuleChangeHistoryDTO dto)
		{
			return new Database.LifecycleRuleChangeHistory
			{
				id = dto.id,
				lifecycleRuleId = dto.lifecycleRuleId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a LifecycleRuleChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(LifecycleRuleChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.lifecycleRuleId = dto.lifecycleRuleId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a LifecycleRuleChangeHistory Object.
		///
		/// </summary>
		public LifecycleRuleChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new LifecycleRuleChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				lifecycleRuleId = this.lifecycleRuleId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a LifecycleRuleChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a LifecycleRuleChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a LifecycleRuleChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a LifecycleRuleChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.LifecycleRuleChangeHistory lifecycleRuleChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (lifecycleRuleChangeHistory == null)
			{
				return null;
			}

			return new {
				id = lifecycleRuleChangeHistory.id,
				lifecycleRuleId = lifecycleRuleChangeHistory.lifecycleRuleId,
				versionNumber = lifecycleRuleChangeHistory.versionNumber,
				timeStamp = lifecycleRuleChangeHistory.timeStamp,
				userId = lifecycleRuleChangeHistory.userId,
				data = lifecycleRuleChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a LifecycleRuleChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(LifecycleRuleChangeHistory lifecycleRuleChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (lifecycleRuleChangeHistory == null)
			{
				return null;
			}

			return new {
				id = lifecycleRuleChangeHistory.id,
				lifecycleRuleId = lifecycleRuleChangeHistory.lifecycleRuleId,
				versionNumber = lifecycleRuleChangeHistory.versionNumber,
				timeStamp = lifecycleRuleChangeHistory.timeStamp,
				userId = lifecycleRuleChangeHistory.userId,
				data = lifecycleRuleChangeHistory.data,
				lifecycleRule = LifecycleRule.CreateMinimalAnonymous(lifecycleRuleChangeHistory.lifecycleRule)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a LifecycleRuleChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(LifecycleRuleChangeHistory lifecycleRuleChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (lifecycleRuleChangeHistory == null)
			{
				return null;
			}

			return new {
				id = lifecycleRuleChangeHistory.id,
				name = lifecycleRuleChangeHistory.id,
				description = lifecycleRuleChangeHistory.id
			 };
		}
	}
}
