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
	public partial class IncidentNotification : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class IncidentNotificationDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 incidentId { get; set; }
			public Int32? escalationRuleId { get; set; }
			[Required]
			public Guid userObjectGuid { get; set; }
			[Required]
			public DateTime firstNotifiedAt { get; set; }
			public DateTime? lastNotifiedAt { get; set; }
			public DateTime? acknowledgedAt { get; set; }
			public Guid? acknowledgedByObjectGuid { get; set; }
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
		public class IncidentNotificationOutputDTO : IncidentNotificationDTO
		{
			public EscalationRule.EscalationRuleDTO escalationRule { get; set; }
			public Incident.IncidentDTO incident { get; set; }
		}


		/// <summary>
		///
		/// Converts a IncidentNotification to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public IncidentNotificationDTO ToDTO()
		{
			return new IncidentNotificationDTO
			{
				id = this.id,
				incidentId = this.incidentId,
				escalationRuleId = this.escalationRuleId,
				userObjectGuid = this.userObjectGuid,
				firstNotifiedAt = this.firstNotifiedAt,
				lastNotifiedAt = this.lastNotifiedAt,
				acknowledgedAt = this.acknowledgedAt,
				acknowledgedByObjectGuid = this.acknowledgedByObjectGuid,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a IncidentNotification list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<IncidentNotificationDTO> ToDTOList(List<IncidentNotification> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IncidentNotificationDTO> output = new List<IncidentNotificationDTO>();

			output.Capacity = data.Count;

			foreach (IncidentNotification incidentNotification in data)
			{
				output.Add(incidentNotification.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a IncidentNotification to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the IncidentNotificationEntity type directly.
		///
		/// </summary>
		public IncidentNotificationOutputDTO ToOutputDTO()
		{
			return new IncidentNotificationOutputDTO
			{
				id = this.id,
				incidentId = this.incidentId,
				escalationRuleId = this.escalationRuleId,
				userObjectGuid = this.userObjectGuid,
				firstNotifiedAt = this.firstNotifiedAt,
				lastNotifiedAt = this.lastNotifiedAt,
				acknowledgedAt = this.acknowledgedAt,
				acknowledgedByObjectGuid = this.acknowledgedByObjectGuid,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				escalationRule = this.escalationRule?.ToDTO(),
				incident = this.incident?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a IncidentNotification list to list of Output Data Transfer Object intended to be used for serializing a list of IncidentNotification objects to avoid using the IncidentNotification entity type directly.
		///
		/// </summary>
		public static List<IncidentNotificationOutputDTO> ToOutputDTOList(List<IncidentNotification> data)
		{
			if (data == null)
			{
				return null;
			}

			List<IncidentNotificationOutputDTO> output = new List<IncidentNotificationOutputDTO>();

			output.Capacity = data.Count;

			foreach (IncidentNotification incidentNotification in data)
			{
				output.Add(incidentNotification.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a IncidentNotification Object.
		///
		/// </summary>
		public static Database.IncidentNotification FromDTO(IncidentNotificationDTO dto)
		{
			return new Database.IncidentNotification
			{
				id = dto.id,
				incidentId = dto.incidentId,
				escalationRuleId = dto.escalationRuleId,
				userObjectGuid = dto.userObjectGuid,
				firstNotifiedAt = dto.firstNotifiedAt,
				lastNotifiedAt = dto.lastNotifiedAt,
				acknowledgedAt = dto.acknowledgedAt,
				acknowledgedByObjectGuid = dto.acknowledgedByObjectGuid,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a IncidentNotification Object.
		///
		/// </summary>
		public void ApplyDTO(IncidentNotificationDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.incidentId = dto.incidentId;
			this.escalationRuleId = dto.escalationRuleId;
			this.userObjectGuid = dto.userObjectGuid;
			this.firstNotifiedAt = dto.firstNotifiedAt;
			this.lastNotifiedAt = dto.lastNotifiedAt;
			this.acknowledgedAt = dto.acknowledgedAt;
			this.acknowledgedByObjectGuid = dto.acknowledgedByObjectGuid;
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
		/// Creates a deep copy clone of a IncidentNotification Object.
		///
		/// </summary>
		public IncidentNotification Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new IncidentNotification{
				id = this.id,
				tenantGuid = this.tenantGuid,
				incidentId = this.incidentId,
				escalationRuleId = this.escalationRuleId,
				userObjectGuid = this.userObjectGuid,
				firstNotifiedAt = this.firstNotifiedAt,
				lastNotifiedAt = this.lastNotifiedAt,
				acknowledgedAt = this.acknowledgedAt,
				acknowledgedByObjectGuid = this.acknowledgedByObjectGuid,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IncidentNotification Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a IncidentNotification Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a IncidentNotification Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a IncidentNotification Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.IncidentNotification incidentNotification)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (incidentNotification == null)
			{
				return null;
			}

			return new {
				id = incidentNotification.id,
				incidentId = incidentNotification.incidentId,
				escalationRuleId = incidentNotification.escalationRuleId,
				userObjectGuid = incidentNotification.userObjectGuid,
				firstNotifiedAt = incidentNotification.firstNotifiedAt,
				lastNotifiedAt = incidentNotification.lastNotifiedAt,
				acknowledgedAt = incidentNotification.acknowledgedAt,
				acknowledgedByObjectGuid = incidentNotification.acknowledgedByObjectGuid,
				objectGuid = incidentNotification.objectGuid,
				active = incidentNotification.active,
				deleted = incidentNotification.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a IncidentNotification Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(IncidentNotification incidentNotification)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (incidentNotification == null)
			{
				return null;
			}

			return new {
				id = incidentNotification.id,
				incidentId = incidentNotification.incidentId,
				escalationRuleId = incidentNotification.escalationRuleId,
				userObjectGuid = incidentNotification.userObjectGuid,
				firstNotifiedAt = incidentNotification.firstNotifiedAt,
				lastNotifiedAt = incidentNotification.lastNotifiedAt,
				acknowledgedAt = incidentNotification.acknowledgedAt,
				acknowledgedByObjectGuid = incidentNotification.acknowledgedByObjectGuid,
				objectGuid = incidentNotification.objectGuid,
				active = incidentNotification.active,
				deleted = incidentNotification.deleted,
				escalationRule = EscalationRule.CreateMinimalAnonymous(incidentNotification.escalationRule),
				incident = Incident.CreateMinimalAnonymous(incidentNotification.incident)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a IncidentNotification Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(IncidentNotification incidentNotification)
		{
			//
			// Return a very minimal object.
			//
			if (incidentNotification == null)
			{
				return null;
			}

			return new {
				id = incidentNotification.id,
				name = incidentNotification.id,
				description = incidentNotification.id
			 };
		}
	}
}
