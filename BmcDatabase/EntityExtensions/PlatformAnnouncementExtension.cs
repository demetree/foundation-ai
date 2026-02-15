using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.BMC.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class PlatformAnnouncement : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class PlatformAnnouncementDTO
		{
			public Int32 id { get; set; }
			[Required]
			public String name { get; set; }
			public String body { get; set; }
			public String announcementType { get; set; }
			[Required]
			public DateTime startDate { get; set; }
			public DateTime? endDate { get; set; }
			[Required]
			public Boolean isActive { get; set; }
			[Required]
			public Int32 priority { get; set; }
			[Required]
			public Boolean showOnLandingPage { get; set; }
			[Required]
			public Boolean showOnDashboard { get; set; }
			[Required]
			public Guid objectGuid { get; set; }
			public Boolean? active { get; set; }
			public Boolean? deleted { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class PlatformAnnouncementOutputDTO : PlatformAnnouncementDTO
		{
		}


		/// <summary>
		///
		/// Converts a PlatformAnnouncement to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public PlatformAnnouncementDTO ToDTO()
		{
			return new PlatformAnnouncementDTO
			{
				id = this.id,
				name = this.name,
				body = this.body,
				announcementType = this.announcementType,
				startDate = this.startDate,
				endDate = this.endDate,
				isActive = this.isActive,
				priority = this.priority,
				showOnLandingPage = this.showOnLandingPage,
				showOnDashboard = this.showOnDashboard,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PlatformAnnouncement list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<PlatformAnnouncementDTO> ToDTOList(List<PlatformAnnouncement> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PlatformAnnouncementDTO> output = new List<PlatformAnnouncementDTO>();

			output.Capacity = data.Count;

			foreach (PlatformAnnouncement platformAnnouncement in data)
			{
				output.Add(platformAnnouncement.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a PlatformAnnouncement to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the PlatformAnnouncementEntity type directly.
		///
		/// </summary>
		public PlatformAnnouncementOutputDTO ToOutputDTO()
		{
			return new PlatformAnnouncementOutputDTO
			{
				id = this.id,
				name = this.name,
				body = this.body,
				announcementType = this.announcementType,
				startDate = this.startDate,
				endDate = this.endDate,
				isActive = this.isActive,
				priority = this.priority,
				showOnLandingPage = this.showOnLandingPage,
				showOnDashboard = this.showOnDashboard,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a PlatformAnnouncement list to list of Output Data Transfer Object intended to be used for serializing a list of PlatformAnnouncement objects to avoid using the PlatformAnnouncement entity type directly.
		///
		/// </summary>
		public static List<PlatformAnnouncementOutputDTO> ToOutputDTOList(List<PlatformAnnouncement> data)
		{
			if (data == null)
			{
				return null;
			}

			List<PlatformAnnouncementOutputDTO> output = new List<PlatformAnnouncementOutputDTO>();

			output.Capacity = data.Count;

			foreach (PlatformAnnouncement platformAnnouncement in data)
			{
				output.Add(platformAnnouncement.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a PlatformAnnouncement Object.
		///
		/// </summary>
		public static Database.PlatformAnnouncement FromDTO(PlatformAnnouncementDTO dto)
		{
			return new Database.PlatformAnnouncement
			{
				id = dto.id,
				name = dto.name,
				body = dto.body,
				announcementType = dto.announcementType,
				startDate = dto.startDate,
				endDate = dto.endDate,
				isActive = dto.isActive,
				priority = dto.priority,
				showOnLandingPage = dto.showOnLandingPage,
				showOnDashboard = dto.showOnDashboard,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a PlatformAnnouncement Object.
		///
		/// </summary>
		public void ApplyDTO(PlatformAnnouncementDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.name = dto.name;
			this.body = dto.body;
			this.announcementType = dto.announcementType;
			this.startDate = dto.startDate;
			this.endDate = dto.endDate;
			this.isActive = dto.isActive;
			this.priority = dto.priority;
			this.showOnLandingPage = dto.showOnLandingPage;
			this.showOnDashboard = dto.showOnDashboard;
			this.objectGuid = dto.objectGuid;
			if (dto.active.HasValue == true)
			{
				this.active = dto.active.Value;
			}
			if (dto.deleted.HasValue == true)
			{
				this.deleted = dto.deleted.Value;
			}
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a PlatformAnnouncement Object.
		///
		/// </summary>
		public PlatformAnnouncement Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new PlatformAnnouncement{
				id = this.id,
				name = this.name,
				body = this.body,
				announcementType = this.announcementType,
				startDate = this.startDate,
				endDate = this.endDate,
				isActive = this.isActive,
				priority = this.priority,
				showOnLandingPage = this.showOnLandingPage,
				showOnDashboard = this.showOnDashboard,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PlatformAnnouncement Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a PlatformAnnouncement Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a PlatformAnnouncement Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a PlatformAnnouncement Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.PlatformAnnouncement platformAnnouncement)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (platformAnnouncement == null)
			{
				return null;
			}

			return new {
				id = platformAnnouncement.id,
				name = platformAnnouncement.name,
				body = platformAnnouncement.body,
				announcementType = platformAnnouncement.announcementType,
				startDate = platformAnnouncement.startDate,
				endDate = platformAnnouncement.endDate,
				isActive = platformAnnouncement.isActive,
				priority = platformAnnouncement.priority,
				showOnLandingPage = platformAnnouncement.showOnLandingPage,
				showOnDashboard = platformAnnouncement.showOnDashboard,
				objectGuid = platformAnnouncement.objectGuid,
				active = platformAnnouncement.active,
				deleted = platformAnnouncement.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a PlatformAnnouncement Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(PlatformAnnouncement platformAnnouncement)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (platformAnnouncement == null)
			{
				return null;
			}

			return new {
				id = platformAnnouncement.id,
				name = platformAnnouncement.name,
				body = platformAnnouncement.body,
				announcementType = platformAnnouncement.announcementType,
				startDate = platformAnnouncement.startDate,
				endDate = platformAnnouncement.endDate,
				isActive = platformAnnouncement.isActive,
				priority = platformAnnouncement.priority,
				showOnLandingPage = platformAnnouncement.showOnLandingPage,
				showOnDashboard = platformAnnouncement.showOnDashboard,
				objectGuid = platformAnnouncement.objectGuid,
				active = platformAnnouncement.active,
				deleted = platformAnnouncement.deleted
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a PlatformAnnouncement Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(PlatformAnnouncement platformAnnouncement)
		{
			//
			// Return a very minimal object.
			//
			if (platformAnnouncement == null)
			{
				return null;
			}

			return new {
				id = platformAnnouncement.id,
				name = platformAnnouncement.name,
				description = string.Join(", ", new[] { platformAnnouncement.name, platformAnnouncement.announcementType}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
