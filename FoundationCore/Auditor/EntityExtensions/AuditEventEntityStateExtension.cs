using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Auditor.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class AuditEventEntityState : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AuditEventEntityStateDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 auditEventId { get; set; }
			public String beforeState { get; set; }
			public String afterState { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class AuditEventEntityStateOutputDTO : AuditEventEntityStateDTO
		{
			public AuditEvent.AuditEventDTO auditEvent { get; set; }
		}


		/// <summary>
		///
		/// Converts a AuditEventEntityState to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AuditEventEntityStateDTO ToDTO()
		{
			return new AuditEventEntityStateDTO
			{
				id = this.id,
				auditEventId = this.auditEventId,
				beforeState = this.beforeState,
				afterState = this.afterState
			};
		}


		/// <summary>
		///
		/// Converts a AuditEventEntityState list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AuditEventEntityStateDTO> ToDTOList(List<AuditEventEntityState> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditEventEntityStateDTO> output = new List<AuditEventEntityStateDTO>();

			output.Capacity = data.Count;

			foreach (AuditEventEntityState auditEventEntityState in data)
			{
				output.Add(auditEventEntityState.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AuditEventEntityState to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AuditEventEntityStateEntity type directly.
		///
		/// </summary>
		public AuditEventEntityStateOutputDTO ToOutputDTO()
		{
			return new AuditEventEntityStateOutputDTO
			{
				id = this.id,
				auditEventId = this.auditEventId,
				beforeState = this.beforeState,
				afterState = this.afterState,
				auditEvent = this.auditEvent?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a AuditEventEntityState list to list of Output Data Transfer Object intended to be used for serializing a list of AuditEventEntityState objects to avoid using the AuditEventEntityState entity type directly.
		///
		/// </summary>
		public static List<AuditEventEntityStateOutputDTO> ToOutputDTOList(List<AuditEventEntityState> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditEventEntityStateOutputDTO> output = new List<AuditEventEntityStateOutputDTO>();

			output.Capacity = data.Count;

			foreach (AuditEventEntityState auditEventEntityState in data)
			{
				output.Add(auditEventEntityState.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AuditEventEntityState Object.
		///
		/// </summary>
		public static Database.AuditEventEntityState FromDTO(AuditEventEntityStateDTO dto)
		{
			return new Database.AuditEventEntityState
			{
				id = dto.id,
				auditEventId = dto.auditEventId,
				beforeState = dto.beforeState,
				afterState = dto.afterState
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AuditEventEntityState Object.
		///
		/// </summary>
		public void ApplyDTO(AuditEventEntityStateDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.auditEventId = dto.auditEventId;
			this.beforeState = dto.beforeState;
			this.afterState = dto.afterState;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a AuditEventEntityState Object.
		///
		/// </summary>
		public AuditEventEntityState Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AuditEventEntityState{
				id = this.id,
				auditEventId = this.auditEventId,
				beforeState = this.beforeState,
				afterState = this.afterState,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditEventEntityState Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditEventEntityState Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AuditEventEntityState Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AuditEventEntityState Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AuditEventEntityState auditEventEntityState)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (auditEventEntityState == null)
			{
				return null;
			}

			return new {
				id = auditEventEntityState.id,
				auditEventId = auditEventEntityState.auditEventId,
				beforeState = auditEventEntityState.beforeState,
				afterState = auditEventEntityState.afterState,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AuditEventEntityState Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AuditEventEntityState auditEventEntityState)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (auditEventEntityState == null)
			{
				return null;
			}

			return new {
				id = auditEventEntityState.id,
				auditEventId = auditEventEntityState.auditEventId,
				beforeState = auditEventEntityState.beforeState,
				afterState = auditEventEntityState.afterState,
				auditEvent = AuditEvent.CreateMinimalAnonymous(auditEventEntityState.auditEvent)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AuditEventEntityState Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AuditEventEntityState auditEventEntityState)
		{
			//
			// Return a very minimal object.
			//
			if (auditEventEntityState == null)
			{
				return null;
			}

			return new {
				id = auditEventEntityState.id,
				name = auditEventEntityState.id,
				description = auditEventEntityState.id
			 };
		}
	}
}
