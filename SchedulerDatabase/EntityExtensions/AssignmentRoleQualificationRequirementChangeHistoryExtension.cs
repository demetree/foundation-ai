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
	public partial class AssignmentRoleQualificationRequirementChangeHistory : IChangeHistoryEntity, IAnonymousConvertible
	{
		[NotMapped]
		public long primaryId 
		{
			get { return (long)assignmentRoleQualificationRequirementId; }
			set { assignmentRoleQualificationRequirementId = (int)value; } 
		}

		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AssignmentRoleQualificationRequirementChangeHistoryDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 assignmentRoleQualificationRequirementId { get; set; }
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
		public class AssignmentRoleQualificationRequirementChangeHistoryOutputDTO : AssignmentRoleQualificationRequirementChangeHistoryDTO
		{
			public AssignmentRoleQualificationRequirement.AssignmentRoleQualificationRequirementDTO assignmentRoleQualificationRequirement { get; set; }
		}


		/// <summary>
		///
		/// Converts a AssignmentRoleQualificationRequirementChangeHistory to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AssignmentRoleQualificationRequirementChangeHistoryDTO ToDTO()
		{
			return new AssignmentRoleQualificationRequirementChangeHistoryDTO
			{
				id = this.id,
				assignmentRoleQualificationRequirementId = this.assignmentRoleQualificationRequirementId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data
			};
		}


		/// <summary>
		///
		/// Converts a AssignmentRoleQualificationRequirementChangeHistory list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AssignmentRoleQualificationRequirementChangeHistoryDTO> ToDTOList(List<AssignmentRoleQualificationRequirementChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AssignmentRoleQualificationRequirementChangeHistoryDTO> output = new List<AssignmentRoleQualificationRequirementChangeHistoryDTO>();

			output.Capacity = data.Count;

			foreach (AssignmentRoleQualificationRequirementChangeHistory assignmentRoleQualificationRequirementChangeHistory in data)
			{
				output.Add(assignmentRoleQualificationRequirementChangeHistory.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AssignmentRoleQualificationRequirementChangeHistory to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AssignmentRoleQualificationRequirementChangeHistoryEntity type directly.
		///
		/// </summary>
		public AssignmentRoleQualificationRequirementChangeHistoryOutputDTO ToOutputDTO()
		{
			return new AssignmentRoleQualificationRequirementChangeHistoryOutputDTO
			{
				id = this.id,
				assignmentRoleQualificationRequirementId = this.assignmentRoleQualificationRequirementId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
				assignmentRoleQualificationRequirement = this.assignmentRoleQualificationRequirement?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a AssignmentRoleQualificationRequirementChangeHistory list to list of Output Data Transfer Object intended to be used for serializing a list of AssignmentRoleQualificationRequirementChangeHistory objects to avoid using the AssignmentRoleQualificationRequirementChangeHistory entity type directly.
		///
		/// </summary>
		public static List<AssignmentRoleQualificationRequirementChangeHistoryOutputDTO> ToOutputDTOList(List<AssignmentRoleQualificationRequirementChangeHistory> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AssignmentRoleQualificationRequirementChangeHistoryOutputDTO> output = new List<AssignmentRoleQualificationRequirementChangeHistoryOutputDTO>();

			output.Capacity = data.Count;

			foreach (AssignmentRoleQualificationRequirementChangeHistory assignmentRoleQualificationRequirementChangeHistory in data)
			{
				output.Add(assignmentRoleQualificationRequirementChangeHistory.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AssignmentRoleQualificationRequirementChangeHistory Object.
		///
		/// </summary>
		public static Database.AssignmentRoleQualificationRequirementChangeHistory FromDTO(AssignmentRoleQualificationRequirementChangeHistoryDTO dto)
		{
			return new Database.AssignmentRoleQualificationRequirementChangeHistory
			{
				id = dto.id,
				assignmentRoleQualificationRequirementId = dto.assignmentRoleQualificationRequirementId,
				versionNumber = dto.versionNumber,
				timeStamp = dto.timeStamp,
				userId = dto.userId,
				data = dto.data
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AssignmentRoleQualificationRequirementChangeHistory Object.
		///
		/// </summary>
		public void ApplyDTO(AssignmentRoleQualificationRequirementChangeHistoryDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.assignmentRoleQualificationRequirementId = dto.assignmentRoleQualificationRequirementId;
			this.versionNumber = dto.versionNumber;
			this.timeStamp = dto.timeStamp;
			this.userId = dto.userId;
			this.data = dto.data;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a AssignmentRoleQualificationRequirementChangeHistory Object.
		///
		/// </summary>
		public AssignmentRoleQualificationRequirementChangeHistory Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AssignmentRoleQualificationRequirementChangeHistory{
				id = this.id,
				tenantGuid = this.tenantGuid,
				assignmentRoleQualificationRequirementId = this.assignmentRoleQualificationRequirementId,
				versionNumber = this.versionNumber,
				timeStamp = this.timeStamp,
				userId = this.userId,
				data = this.data,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AssignmentRoleQualificationRequirementChangeHistory Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AssignmentRoleQualificationRequirementChangeHistory Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AssignmentRoleQualificationRequirementChangeHistory Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AssignmentRoleQualificationRequirementChangeHistory Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AssignmentRoleQualificationRequirementChangeHistory assignmentRoleQualificationRequirementChangeHistory)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (assignmentRoleQualificationRequirementChangeHistory == null)
			{
				return null;
			}

			return new {
				id = assignmentRoleQualificationRequirementChangeHistory.id,
				assignmentRoleQualificationRequirementId = assignmentRoleQualificationRequirementChangeHistory.assignmentRoleQualificationRequirementId,
				versionNumber = assignmentRoleQualificationRequirementChangeHistory.versionNumber,
				timeStamp = assignmentRoleQualificationRequirementChangeHistory.timeStamp,
				userId = assignmentRoleQualificationRequirementChangeHistory.userId,
				data = assignmentRoleQualificationRequirementChangeHistory.data,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AssignmentRoleQualificationRequirementChangeHistory Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AssignmentRoleQualificationRequirementChangeHistory assignmentRoleQualificationRequirementChangeHistory)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (assignmentRoleQualificationRequirementChangeHistory == null)
			{
				return null;
			}

			return new {
				id = assignmentRoleQualificationRequirementChangeHistory.id,
				assignmentRoleQualificationRequirementId = assignmentRoleQualificationRequirementChangeHistory.assignmentRoleQualificationRequirementId,
				versionNumber = assignmentRoleQualificationRequirementChangeHistory.versionNumber,
				timeStamp = assignmentRoleQualificationRequirementChangeHistory.timeStamp,
				userId = assignmentRoleQualificationRequirementChangeHistory.userId,
				data = assignmentRoleQualificationRequirementChangeHistory.data,
				assignmentRoleQualificationRequirement = AssignmentRoleQualificationRequirement.CreateMinimalAnonymous(assignmentRoleQualificationRequirementChangeHistory.assignmentRoleQualificationRequirement)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AssignmentRoleQualificationRequirementChangeHistory Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AssignmentRoleQualificationRequirementChangeHistory assignmentRoleQualificationRequirementChangeHistory)
		{
			//
			// Return a very minimal object.
			//
			if (assignmentRoleQualificationRequirementChangeHistory == null)
			{
				return null;
			}

			return new {
				id = assignmentRoleQualificationRequirementChangeHistory.id,
				name = assignmentRoleQualificationRequirementChangeHistory.id,
				description = assignmentRoleQualificationRequirementChangeHistory.id
			 };
		}
	}
}
