using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Alerting.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class IncidentTimelineEvent : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class IncidentTimelineEventDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 incidentId { get; set; }
			[Required]
			public Int32 incidentEventTypeId { get; set; }
			[Required]
			public DateTime timestamp { get; set; }
			public Guid? actorObjectGuid { get; set; }
			public String detailsJson { get; set; }
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
		public class IncidentTimelineEventOutputDTO : IncidentTimelineEventDTO
		{
			public Incident.IncidentDTO incident { get; set; }
			public IncidentEventType.IncidentEventTypeDTO incidentEventType { get; set; }
		}


		/// <summary>
		///
		/// Converts a IncidentTimelineEvent to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public IncidentTimelineEventDTO ToDTO()
		{
			return new IncidentTimelineEventDTO
			{
				id = this.id,
				incidentId = this.incidentId,
				incidentEventTypeId = this.incidentEventTypeId,
				timestamp = this.timestamp,
				actorObjectGuid = this.actorObjectGuid,
				detailsJson = this.detailsJson,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a IncidentTimelineEvent list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<IncidentTimelineEventDTO> ToDTOList(List<IncidentTimelineEvent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IncidentTimelineEventDTO> output = new List<IncidentTimelineEventDTO>();

			output.Capacity = data.Count;

			foreach (IncidentTimelineEvent incidentTimelineEvent in data)
			{
				output.Add(incidentTimelineEvent.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a IncidentTimelineEvent to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the IncidentTimelineEventEntity type directly.
		///
		/// </summary>
		public IncidentTimelineEventOutputDTO ToOutputDTO()
		{
			return new IncidentTimelineEventOutputDTO
			{
				id = this.id,
				incidentId = this.incidentId,
				incidentEventTypeId = this.incidentEventTypeId,
				timestamp = this.timestamp,
				actorObjectGuid = this.actorObjectGuid,
				detailsJson = this.detailsJson,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				incident = this.incident?.ToDTO(),
				incidentEventType = this.incidentEventType?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a IncidentTimelineEvent list to list of Output Data Transfer Object intended to be used for serializing a list of IncidentTimelineEvent objects to avoid using the IncidentTimelineEvent entity type directly.
		///
		/// </summary>
		public static List<IncidentTimelineEventOutputDTO> ToOutputDTOList(List<IncidentTimelineEvent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IncidentTimelineEventOutputDTO> output = new List<IncidentTimelineEventOutputDTO>();

			output.Capacity = data.Count;

			foreach (IncidentTimelineEvent incidentTimelineEvent in data)
			{
				output.Add(incidentTimelineEvent.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a IncidentTimelineEvent Object.
		///
		/// </summary>
		public static Database.IncidentTimelineEvent FromDTO(IncidentTimelineEventDTO dto)
		{
			return new Database.IncidentTimelineEvent
			{
				id = dto.id,
				incidentId = dto.incidentId,
				incidentEventTypeId = dto.incidentEventTypeId,
				timestamp = dto.timestamp,
				actorObjectGuid = dto.actorObjectGuid,
				detailsJson = dto.detailsJson,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a IncidentTimelineEvent Object.
		///
		/// </summary>
		public void ApplyDTO(IncidentTimelineEventDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.incidentId = dto.incidentId;
			this.incidentEventTypeId = dto.incidentEventTypeId;
			this.timestamp = dto.timestamp;
			this.actorObjectGuid = dto.actorObjectGuid;
			this.detailsJson = dto.detailsJson;
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
		/// Creates a deep copy clone of a IncidentTimelineEvent Object.
		///
		/// </summary>
		public IncidentTimelineEvent Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new IncidentTimelineEvent{
				id = this.id,
				tenantGuid = this.tenantGuid,
				incidentId = this.incidentId,
				incidentEventTypeId = this.incidentEventTypeId,
				timestamp = this.timestamp,
				actorObjectGuid = this.actorObjectGuid,
				detailsJson = this.detailsJson,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IncidentTimelineEvent Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IncidentTimelineEvent Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a IncidentTimelineEvent Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a IncidentTimelineEvent Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.IncidentTimelineEvent incidentTimelineEvent)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (incidentTimelineEvent == null)
			{
				return null;
			}

			return new {
				id = incidentTimelineEvent.id,
				incidentId = incidentTimelineEvent.incidentId,
				incidentEventTypeId = incidentTimelineEvent.incidentEventTypeId,
				timestamp = incidentTimelineEvent.timestamp,
				actorObjectGuid = incidentTimelineEvent.actorObjectGuid,
				detailsJson = incidentTimelineEvent.detailsJson,
				objectGuid = incidentTimelineEvent.objectGuid,
				active = incidentTimelineEvent.active,
				deleted = incidentTimelineEvent.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a IncidentTimelineEvent Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(IncidentTimelineEvent incidentTimelineEvent)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (incidentTimelineEvent == null)
			{
				return null;
			}

			return new {
				id = incidentTimelineEvent.id,
				incidentId = incidentTimelineEvent.incidentId,
				incidentEventTypeId = incidentTimelineEvent.incidentEventTypeId,
				timestamp = incidentTimelineEvent.timestamp,
				actorObjectGuid = incidentTimelineEvent.actorObjectGuid,
				detailsJson = incidentTimelineEvent.detailsJson,
				objectGuid = incidentTimelineEvent.objectGuid,
				active = incidentTimelineEvent.active,
				deleted = incidentTimelineEvent.deleted,
				incident = Incident.CreateMinimalAnonymous(incidentTimelineEvent.incident),
				incidentEventType = IncidentEventType.CreateMinimalAnonymous(incidentTimelineEvent.incidentEventType)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a IncidentTimelineEvent Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(IncidentTimelineEvent incidentTimelineEvent)
		{
			//
			// Return a very minimal object.
			//
			if (incidentTimelineEvent == null)
			{
				return null;
			}

			return new {
				id = incidentTimelineEvent.id,
				name = incidentTimelineEvent.id,
				description = incidentTimelineEvent.id
			 };
		}
	}
}
