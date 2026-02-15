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
	public partial class UserBadgeAssignment : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class UserBadgeAssignmentDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 userBadgeId { get; set; }
			[Required]
			public DateTime awardedDate { get; set; }
			public Guid? awardedByTenantGuid { get; set; }
			public String reason { get; set; }
			[Required]
			public Boolean isDisplayed { get; set; }
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
		public class UserBadgeAssignmentOutputDTO : UserBadgeAssignmentDTO
		{
			public UserBadge.UserBadgeDTO userBadge { get; set; }
		}


		/// <summary>
		///
		/// Converts a UserBadgeAssignment to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public UserBadgeAssignmentDTO ToDTO()
		{
			return new UserBadgeAssignmentDTO
			{
				id = this.id,
				userBadgeId = this.userBadgeId,
				awardedDate = this.awardedDate,
				awardedByTenantGuid = this.awardedByTenantGuid,
				reason = this.reason,
				isDisplayed = this.isDisplayed,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a UserBadgeAssignment list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<UserBadgeAssignmentDTO> ToDTOList(List<UserBadgeAssignment> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserBadgeAssignmentDTO> output = new List<UserBadgeAssignmentDTO>();

			output.Capacity = data.Count;

			foreach (UserBadgeAssignment userBadgeAssignment in data)
			{
				output.Add(userBadgeAssignment.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a UserBadgeAssignment to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the UserBadgeAssignmentEntity type directly.
		///
		/// </summary>
		public UserBadgeAssignmentOutputDTO ToOutputDTO()
		{
			return new UserBadgeAssignmentOutputDTO
			{
				id = this.id,
				userBadgeId = this.userBadgeId,
				awardedDate = this.awardedDate,
				awardedByTenantGuid = this.awardedByTenantGuid,
				reason = this.reason,
				isDisplayed = this.isDisplayed,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				userBadge = this.userBadge?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a UserBadgeAssignment list to list of Output Data Transfer Object intended to be used for serializing a list of UserBadgeAssignment objects to avoid using the UserBadgeAssignment entity type directly.
		///
		/// </summary>
		public static List<UserBadgeAssignmentOutputDTO> ToOutputDTOList(List<UserBadgeAssignment> data)
		{
			if (data == null)
			{
				return null;
			}

			List<UserBadgeAssignmentOutputDTO> output = new List<UserBadgeAssignmentOutputDTO>();

			output.Capacity = data.Count;

			foreach (UserBadgeAssignment userBadgeAssignment in data)
			{
				output.Add(userBadgeAssignment.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a UserBadgeAssignment Object.
		///
		/// </summary>
		public static Database.UserBadgeAssignment FromDTO(UserBadgeAssignmentDTO dto)
		{
			return new Database.UserBadgeAssignment
			{
				id = dto.id,
				userBadgeId = dto.userBadgeId,
				awardedDate = dto.awardedDate,
				awardedByTenantGuid = dto.awardedByTenantGuid,
				reason = dto.reason,
				isDisplayed = dto.isDisplayed,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a UserBadgeAssignment Object.
		///
		/// </summary>
		public void ApplyDTO(UserBadgeAssignmentDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.userBadgeId = dto.userBadgeId;
			this.awardedDate = dto.awardedDate;
			this.awardedByTenantGuid = dto.awardedByTenantGuid;
			this.reason = dto.reason;
			this.isDisplayed = dto.isDisplayed;
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
		/// Creates a deep copy clone of a UserBadgeAssignment Object.
		///
		/// </summary>
		public UserBadgeAssignment Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new UserBadgeAssignment{
				id = this.id,
				tenantGuid = this.tenantGuid,
				userBadgeId = this.userBadgeId,
				awardedDate = this.awardedDate,
				awardedByTenantGuid = this.awardedByTenantGuid,
				reason = this.reason,
				isDisplayed = this.isDisplayed,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserBadgeAssignment Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a UserBadgeAssignment Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a UserBadgeAssignment Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a UserBadgeAssignment Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.UserBadgeAssignment userBadgeAssignment)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (userBadgeAssignment == null)
			{
				return null;
			}

			return new {
				id = userBadgeAssignment.id,
				userBadgeId = userBadgeAssignment.userBadgeId,
				awardedDate = userBadgeAssignment.awardedDate,
				awardedByTenantGuid = userBadgeAssignment.awardedByTenantGuid,
				reason = userBadgeAssignment.reason,
				isDisplayed = userBadgeAssignment.isDisplayed,
				objectGuid = userBadgeAssignment.objectGuid,
				active = userBadgeAssignment.active,
				deleted = userBadgeAssignment.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a UserBadgeAssignment Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(UserBadgeAssignment userBadgeAssignment)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (userBadgeAssignment == null)
			{
				return null;
			}

			return new {
				id = userBadgeAssignment.id,
				userBadgeId = userBadgeAssignment.userBadgeId,
				awardedDate = userBadgeAssignment.awardedDate,
				awardedByTenantGuid = userBadgeAssignment.awardedByTenantGuid,
				reason = userBadgeAssignment.reason,
				isDisplayed = userBadgeAssignment.isDisplayed,
				objectGuid = userBadgeAssignment.objectGuid,
				active = userBadgeAssignment.active,
				deleted = userBadgeAssignment.deleted,
				userBadge = UserBadge.CreateMinimalAnonymous(userBadgeAssignment.userBadge)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a UserBadgeAssignment Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(UserBadgeAssignment userBadgeAssignment)
		{
			//
			// Return a very minimal object.
			//
			if (userBadgeAssignment == null)
			{
				return null;
			}

			return new {
				id = userBadgeAssignment.id,
				name = userBadgeAssignment.id,
				description = userBadgeAssignment.id
			 };
		}
	}
}
