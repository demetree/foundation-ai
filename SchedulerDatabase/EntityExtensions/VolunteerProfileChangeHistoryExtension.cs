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
	public partial class VolunteerProfileChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)volunteerProfileId; }
			set { volunteerProfileId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class VolunteerProfileChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 volunteerProfileId { get; set; }
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
		public class VolunteerProfileChangeHistoryOutputDTO : VolunteerProfileChangeHistoryDTO
		{
			public VolunteerProfile.VolunteerProfileDTO volunteerProfile { get; set; }
		}


		/// <summary>
		///
		/// Converts a VolunteerProfileChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public VolunteerProfileChangeHistoryDTO ToDTO()
		{
			return new VolunteerProfileChangeHistoryDTO
			{
				id = this.id,
				volunteerProfileId = this.volunteerProfileId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a VolunteerProfileChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<VolunteerProfileChangeHistoryDTO> ToDTOList(List<VolunteerProfileChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<VolunteerProfileChangeHistoryDTO> output = new List<VolunteerProfileChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (VolunteerProfileChangeHistory volunteerProfileChangeHistory in data)
			{
				output.Add(volunteerProfileChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a VolunteerProfileChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the VolunteerProfileChangeHistory Entity type directly.
		///
		/// </summary>
		public VolunteerProfileChangeHistoryOutputDTO ToOutputDTO()
		{
			return new VolunteerProfileChangeHistoryOutputDTO
			{
				id = this.id,
				volunteerProfileId = this.volunteerProfileId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				volunteerProfile = this.volunteerProfile?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a VolunteerProfileChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of VolunteerProfileChangeHistory objects to avoid using the VolunteerProfileChangeHistory entity type directly.
		///
		/// </summary>
		public static List<VolunteerProfileChangeHistoryOutputDTO> ToOutputDTOList(List<VolunteerProfileChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<VolunteerProfileChangeHistoryOutputDTO> output = new List<VolunteerProfileChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (VolunteerProfileChangeHistory volunteerProfileChangeHistory in data)
			{
				output.Add(volunteerProfileChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a VolunteerProfileChangeHistory Object.
		///
		/// </summary>
		public static Database.VolunteerProfileChangeHistory FromDTO(VolunteerProfileChangeHistoryDTO dto)
		{
			return new Database.VolunteerProfileChangeHistory
			{
				id = dto.id,
				volunteerProfileId = dto.volunteerProfileId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a VolunteerProfileChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(VolunteerProfileChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.volunteerProfileId = dto.volunteerProfileId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a VolunteerProfileChangeHistory Object.
		///
		/// </summary>
		public VolunteerProfileChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new VolunteerProfileChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				volunteerProfileId = this.volunteerProfileId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a VolunteerProfileChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a VolunteerProfileChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a VolunteerProfileChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a VolunteerProfileChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.VolunteerProfileChangeHistory volunteerProfileChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (volunteerProfileChangeHistory == null)
			{
				return null;
			}

			return new {
				id = volunteerProfileChangeHistory.id,
				volunteerProfileId = volunteerProfileChangeHistory.volunteerProfileId,
				versionNumber = volunteerProfileChangeHistory.versionNumber,
				timeStamp = volunteerProfileChangeHistory.timeStamp,
				userId = volunteerProfileChangeHistory.userId,
				data = volunteerProfileChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a VolunteerProfileChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(VolunteerProfileChangeHistory volunteerProfileChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (volunteerProfileChangeHistory == null)
			{
				return null;
			}

			return new {
				id = volunteerProfileChangeHistory.id,
				volunteerProfileId = volunteerProfileChangeHistory.volunteerProfileId,
				versionNumber = volunteerProfileChangeHistory.versionNumber,
				timeStamp = volunteerProfileChangeHistory.timeStamp,
				userId = volunteerProfileChangeHistory.userId,
				data = volunteerProfileChangeHistory.data,
				volunteerProfile = VolunteerProfile.CreateMinimalAnonymous(volunteerProfileChangeHistory.volunteerProfile)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a VolunteerProfileChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(VolunteerProfileChangeHistory volunteerProfileChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (volunteerProfileChangeHistory == null)
			{
				return null;
			}

			return new {
				id = volunteerProfileChangeHistory.id,
				name = volunteerProfileChangeHistory.id,
				description = volunteerProfileChangeHistory.id
			 };
		}
	}
}
