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
	public partial class OfficeContactChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)officeContactId; }
			set { officeContactId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class OfficeContactChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 officeContactId { get; set; }
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
		public class OfficeContactChangeHistoryOutputDTO : OfficeContactChangeHistoryDTO
		{
			public OfficeContact.OfficeContactDTO officeContact { get; set; }
		}


		/// <summary>
		///
		/// Converts a OfficeContactChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public OfficeContactChangeHistoryDTO ToDTO()
		{
			return new OfficeContactChangeHistoryDTO
			{
				id = this.id,
				officeContactId = this.officeContactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a OfficeContactChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<OfficeContactChangeHistoryDTO> ToDTOList(List<OfficeContactChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<OfficeContactChangeHistoryDTO> output = new List<OfficeContactChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (OfficeContactChangeHistory officeContactChangeHistory in data)
			{
				output.Add(officeContactChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a OfficeContactChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the OfficeContactChangeHistoryEntity type directly.
		///
		/// </summary>
		public OfficeContactChangeHistoryOutputDTO ToOutputDTO()
		{
			return new OfficeContactChangeHistoryOutputDTO
			{
				id = this.id,
				officeContactId = this.officeContactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				officeContact = this.officeContact?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a OfficeContactChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of OfficeContactChangeHistory objects to avoid using the OfficeContactChangeHistory entity type directly.
		///
		/// </summary>
		public static List<OfficeContactChangeHistoryOutputDTO> ToOutputDTOList(List<OfficeContactChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<OfficeContactChangeHistoryOutputDTO> output = new List<OfficeContactChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (OfficeContactChangeHistory officeContactChangeHistory in data)
			{
				output.Add(officeContactChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a OfficeContactChangeHistory Object.
		///
		/// </summary>
		public static Database.OfficeContactChangeHistory FromDTO(OfficeContactChangeHistoryDTO dto)
		{
			return new Database.OfficeContactChangeHistory
			{
				id = dto.id,
				officeContactId = dto.officeContactId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a OfficeContactChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(OfficeContactChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.officeContactId = dto.officeContactId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a OfficeContactChangeHistory Object.
		///
		/// </summary>
		public OfficeContactChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new OfficeContactChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				officeContactId = this.officeContactId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a OfficeContactChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a OfficeContactChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a OfficeContactChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a OfficeContactChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.OfficeContactChangeHistory officeContactChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (officeContactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = officeContactChangeHistory.id,
				officeContactId = officeContactChangeHistory.officeContactId,
				versionNumber = officeContactChangeHistory.versionNumber,
				timeStamp = officeContactChangeHistory.timeStamp,
				userId = officeContactChangeHistory.userId,
				data = officeContactChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a OfficeContactChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(OfficeContactChangeHistory officeContactChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (officeContactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = officeContactChangeHistory.id,
				officeContactId = officeContactChangeHistory.officeContactId,
				versionNumber = officeContactChangeHistory.versionNumber,
				timeStamp = officeContactChangeHistory.timeStamp,
				userId = officeContactChangeHistory.userId,
				data = officeContactChangeHistory.data,
				officeContact = OfficeContact.CreateMinimalAnonymous(officeContactChangeHistory.officeContact),
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a OfficeContactChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(OfficeContactChangeHistory officeContactChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (officeContactChangeHistory == null)
			{
				return null;
			}

			return new {
				id = officeContactChangeHistory.id,
				name = officeContactChangeHistory.id,
				description = officeContactChangeHistory.id
			 };
		}
	}
}
