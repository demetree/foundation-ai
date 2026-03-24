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
	public partial class OfficeChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)officeId; }
			set { officeId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class OfficeChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 officeId { get; set; }
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
		public class OfficeChangeHistoryOutputDTO : OfficeChangeHistoryDTO
		{
			public Office.OfficeDTO office { get; set; }
		}


		/// <summary>
		///
		/// Converts a OfficeChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public OfficeChangeHistoryDTO ToDTO()
		{
			return new OfficeChangeHistoryDTO
			{
				id = this.id,
				officeId = this.officeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a OfficeChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<OfficeChangeHistoryDTO> ToDTOList(List<OfficeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<OfficeChangeHistoryDTO> output = new List<OfficeChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (OfficeChangeHistory officeChangeHistory in data)
			{
				output.Add(officeChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a OfficeChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the OfficeChangeHistory Entity type directly.
		///
		/// </summary>
		public OfficeChangeHistoryOutputDTO ToOutputDTO()
		{
			return new OfficeChangeHistoryOutputDTO
			{
				id = this.id,
				officeId = this.officeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				office = this.office?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a OfficeChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of OfficeChangeHistory objects to avoid using the OfficeChangeHistory entity type directly.
		///
		/// </summary>
		public static List<OfficeChangeHistoryOutputDTO> ToOutputDTOList(List<OfficeChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<OfficeChangeHistoryOutputDTO> output = new List<OfficeChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (OfficeChangeHistory officeChangeHistory in data)
			{
				output.Add(officeChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a OfficeChangeHistory Object.
		///
		/// </summary>
		public static Database.OfficeChangeHistory FromDTO(OfficeChangeHistoryDTO dto)
		{
			return new Database.OfficeChangeHistory
			{
				id = dto.id,
				officeId = dto.officeId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a OfficeChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(OfficeChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.officeId = dto.officeId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a OfficeChangeHistory Object.
		///
		/// </summary>
		public OfficeChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new OfficeChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				officeId = this.officeId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a OfficeChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a OfficeChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a OfficeChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a OfficeChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.OfficeChangeHistory officeChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (officeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = officeChangeHistory.id,
				officeId = officeChangeHistory.officeId,
				versionNumber = officeChangeHistory.versionNumber,
				timeStamp = officeChangeHistory.timeStamp,
				userId = officeChangeHistory.userId,
				data = officeChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a OfficeChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(OfficeChangeHistory officeChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (officeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = officeChangeHistory.id,
				officeId = officeChangeHistory.officeId,
				versionNumber = officeChangeHistory.versionNumber,
				timeStamp = officeChangeHistory.timeStamp,
				userId = officeChangeHistory.userId,
				data = officeChangeHistory.data,
				office = Office.CreateMinimalAnonymous(officeChangeHistory.office)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a OfficeChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(OfficeChangeHistory officeChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (officeChangeHistory == null)
			{
				return null;
			}

			return new {
				id = officeChangeHistory.id,
				name = officeChangeHistory.id,
				description = officeChangeHistory.id
			 };
		}
	}
}
