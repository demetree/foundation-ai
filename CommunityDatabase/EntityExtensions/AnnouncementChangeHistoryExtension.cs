using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Foundation.Entity;
using Foundation.ChangeHistory;

namespace Foundation.Community.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class AnnouncementChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)announcementId; }
			set { announcementId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AnnouncementChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 announcementId { get; set; }
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
		public class AnnouncementChangeHistoryOutputDTO : AnnouncementChangeHistoryDTO
		{
			public Announcement.AnnouncementDTO announcement { get; set; }
		}


		/// <summary>
		///
		/// Converts a AnnouncementChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AnnouncementChangeHistoryDTO ToDTO()
		{
			return new AnnouncementChangeHistoryDTO
			{
				id = this.id,
				announcementId = this.announcementId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a AnnouncementChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AnnouncementChangeHistoryDTO> ToDTOList(List<AnnouncementChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AnnouncementChangeHistoryDTO> output = new List<AnnouncementChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (AnnouncementChangeHistory announcementChangeHistory in data)
			{
				output.Add(announcementChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AnnouncementChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AnnouncementChangeHistoryEntity type directly.
		///
		/// </summary>
		public AnnouncementChangeHistoryOutputDTO ToOutputDTO()
		{
			return new AnnouncementChangeHistoryOutputDTO
			{
				id = this.id,
				announcementId = this.announcementId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				announcement = this.announcement?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a AnnouncementChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of AnnouncementChangeHistory objects to avoid using the AnnouncementChangeHistory entity type directly.
		///
		/// </summary>
		public static List<AnnouncementChangeHistoryOutputDTO> ToOutputDTOList(List<AnnouncementChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AnnouncementChangeHistoryOutputDTO> output = new List<AnnouncementChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (AnnouncementChangeHistory announcementChangeHistory in data)
			{
				output.Add(announcementChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AnnouncementChangeHistory Object.
		///
		/// </summary>
		public static Database.AnnouncementChangeHistory FromDTO(AnnouncementChangeHistoryDTO dto)
		{
			return new Database.AnnouncementChangeHistory
			{
				id = dto.id,
				announcementId = dto.announcementId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AnnouncementChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(AnnouncementChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.announcementId = dto.announcementId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a AnnouncementChangeHistory Object.
		///
		/// </summary>
		public AnnouncementChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AnnouncementChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				announcementId = this.announcementId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AnnouncementChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AnnouncementChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AnnouncementChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AnnouncementChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AnnouncementChangeHistory announcementChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (announcementChangeHistory == null)
			{
				return null;
			}

			return new {
				id = announcementChangeHistory.id,
				announcementId = announcementChangeHistory.announcementId,
				versionNumber = announcementChangeHistory.versionNumber,
				timeStamp = announcementChangeHistory.timeStamp,
				userId = announcementChangeHistory.userId,
				data = announcementChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AnnouncementChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AnnouncementChangeHistory announcementChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (announcementChangeHistory == null)
			{
				return null;
			}

			return new {
				id = announcementChangeHistory.id,
				announcementId = announcementChangeHistory.announcementId,
				versionNumber = announcementChangeHistory.versionNumber,
				timeStamp = announcementChangeHistory.timeStamp,
				userId = announcementChangeHistory.userId,
				data = announcementChangeHistory.data,
				announcement = Announcement.CreateMinimalAnonymous(announcementChangeHistory.announcement)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AnnouncementChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AnnouncementChangeHistory announcementChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (announcementChangeHistory == null)
			{
				return null;
			}

			return new {
				id = announcementChangeHistory.id,
				name = announcementChangeHistory.id,
				description = announcementChangeHistory.id
			 };
		}
	}
}
