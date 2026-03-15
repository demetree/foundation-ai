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
			get { return (long)AnnouncementId; }
			set { AnnouncementId = (int)value; } 
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
			public Int32 Id { get; set; }
			public Int32 AnnouncementId { get; set; }
			public Int32 VersionNumber { get; set; }
			public DateTime TimeStamp { get; set; }
			public Int32 UserId { get; set; }
			public String Data { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class AnnouncementChangeHistoryOutputDTO : AnnouncementChangeHistoryDTO
		{
			public Announcement.AnnouncementDTO Announcement { get; set; }
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
				Id = this.Id,
				AnnouncementId = this.AnnouncementId,
				VersionNumber = this.VersionNumber,
				TimeStamp = this.TimeStamp,
				UserId = this.UserId,
				Data = this.Data
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
				Id = this.Id,
				AnnouncementId = this.AnnouncementId,
				VersionNumber = this.VersionNumber,
				TimeStamp = this.TimeStamp,
				UserId = this.UserId,
				Data = this.Data,
				Announcement = this.Announcement?.ToDTO()
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
				Id = dto.Id,
				AnnouncementId = dto.AnnouncementId,
				VersionNumber = dto.VersionNumber,
				TimeStamp = dto.TimeStamp,
				UserId = dto.UserId,
				Data = dto.Data
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

			this.AnnouncementId = dto.AnnouncementId;
			this.VersionNumber = dto.VersionNumber;
			this.TimeStamp = dto.TimeStamp;
			this.UserId = dto.UserId;
			this.Data = dto.Data;
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
				Id = this.Id,
				AnnouncementId = this.AnnouncementId,
				VersionNumber = this.VersionNumber,
				TimeStamp = this.TimeStamp,
				UserId = this.UserId,
				Data = this.Data,
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
				Id = announcementChangeHistory.Id,
				AnnouncementId = announcementChangeHistory.AnnouncementId,
				VersionNumber = announcementChangeHistory.VersionNumber,
				TimeStamp = announcementChangeHistory.TimeStamp,
				UserId = announcementChangeHistory.UserId,
				Data = announcementChangeHistory.Data,
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
				Id = announcementChangeHistory.Id,
				AnnouncementId = announcementChangeHistory.AnnouncementId,
				VersionNumber = announcementChangeHistory.VersionNumber,
				TimeStamp = announcementChangeHistory.TimeStamp,
				UserId = announcementChangeHistory.UserId,
				Data = announcementChangeHistory.Data,
				Announcement = Announcement.CreateMinimalAnonymous(announcementChangeHistory.Announcement)
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
				name = announcementChangeHistory.id,
				description = announcementChangeHistory.id
			 };
		}
	}
}
