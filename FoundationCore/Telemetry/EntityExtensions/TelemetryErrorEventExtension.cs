using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Foundation.Entity;

namespace Foundation.Telemetry.Telemetry.Database
{
	//
	// The purpose of this partial class is to provide helper methods to convert an object into a simpler anonymous object better suited for JSON serialization to web client for the web api controllers to use.
	//
	public partial class TelemetryErrorEvent : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class TelemetryErrorEventDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 telemetryApplicationId { get; set; }
			public Int32? telemetrySnapshotId { get; set; }
			public Int64? originalAuditEventId { get; set; }
			[Required]
			public DateTime occurredAt { get; set; }
			public String auditTypeName { get; set; }
			public String moduleName { get; set; }
			public String entityName { get; set; }
			public String userName { get; set; }
			public String message { get; set; }
			public String errorMessage { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class TelemetryErrorEventOutputDTO : TelemetryErrorEventDTO
		{
			public TelemetryApplication.TelemetryApplicationDTO telemetryApplication { get; set; }
			public TelemetrySnapshot.TelemetrySnapshotDTO telemetrySnapshot { get; set; }
		}


		/// <summary>
		///
		/// Converts a TelemetryErrorEvent to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public TelemetryErrorEventDTO ToDTO()
		{
			return new TelemetryErrorEventDTO
			{
				id = this.id,
				telemetryApplicationId = this.telemetryApplicationId,
				telemetrySnapshotId = this.telemetrySnapshotId,
				originalAuditEventId = this.originalAuditEventId,
				occurredAt = this.occurredAt,
				auditTypeName = this.auditTypeName,
				moduleName = this.moduleName,
				entityName = this.entityName,
				userName = this.userName,
				message = this.message,
				errorMessage = this.errorMessage
			};
		}


		/// <summary>
		///
		/// Converts a TelemetryErrorEvent list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<TelemetryErrorEventDTO> ToDTOList(List<TelemetryErrorEvent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetryErrorEventDTO> output = new List<TelemetryErrorEventDTO>();

			output.Capacity = data.Count;

			foreach (TelemetryErrorEvent telemetryErrorEvent in data)
			{
				output.Add(telemetryErrorEvent.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a TelemetryErrorEvent to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the TelemetryErrorEventEntity type directly.
		///
		/// </summary>
		public TelemetryErrorEventOutputDTO ToOutputDTO()
		{
			return new TelemetryErrorEventOutputDTO
			{
				id = this.id,
				telemetryApplicationId = this.telemetryApplicationId,
				telemetrySnapshotId = this.telemetrySnapshotId,
				originalAuditEventId = this.originalAuditEventId,
				occurredAt = this.occurredAt,
				auditTypeName = this.auditTypeName,
				moduleName = this.moduleName,
				entityName = this.entityName,
				userName = this.userName,
				message = this.message,
				errorMessage = this.errorMessage,
				telemetryApplication = this.telemetryApplication?.ToDTO(),
				telemetrySnapshot = this.telemetrySnapshot?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a TelemetryErrorEvent list to list of Output Data Transfer Object intended to be used for serializing a list of TelemetryErrorEvent objects to avoid using the TelemetryErrorEvent entity type directly.
		///
		/// </summary>
		public static List<TelemetryErrorEventOutputDTO> ToOutputDTOList(List<TelemetryErrorEvent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<TelemetryErrorEventOutputDTO> output = new List<TelemetryErrorEventOutputDTO>();

			output.Capacity = data.Count;

			foreach (TelemetryErrorEvent telemetryErrorEvent in data)
			{
				output.Add(telemetryErrorEvent.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a TelemetryErrorEvent Object.
		///
		/// </summary>
		public static Telemetry.Database.TelemetryErrorEvent FromDTO(TelemetryErrorEventDTO dto)
		{
			return new Telemetry.Database.TelemetryErrorEvent
			{
				id = dto.id,
				telemetryApplicationId = dto.telemetryApplicationId,
				telemetrySnapshotId = dto.telemetrySnapshotId,
				originalAuditEventId = dto.originalAuditEventId,
				occurredAt = dto.occurredAt,
				auditTypeName = dto.auditTypeName,
				moduleName = dto.moduleName,
				entityName = dto.entityName,
				userName = dto.userName,
				message = dto.message,
				errorMessage = dto.errorMessage
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a TelemetryErrorEvent Object.
		///
		/// </summary>
		public void ApplyDTO(TelemetryErrorEventDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.telemetryApplicationId = dto.telemetryApplicationId;
			this.telemetrySnapshotId = dto.telemetrySnapshotId;
			this.originalAuditEventId = dto.originalAuditEventId;
			this.occurredAt = dto.occurredAt;
			this.auditTypeName = dto.auditTypeName;
			this.moduleName = dto.moduleName;
			this.entityName = dto.entityName;
			this.userName = dto.userName;
			this.message = dto.message;
			this.errorMessage = dto.errorMessage;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a TelemetryErrorEvent Object.
		///
		/// </summary>
		public TelemetryErrorEvent Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new TelemetryErrorEvent{
				id = this.id,
				telemetryApplicationId = this.telemetryApplicationId,
				telemetrySnapshotId = this.telemetrySnapshotId,
				originalAuditEventId = this.originalAuditEventId,
				occurredAt = this.occurredAt,
				auditTypeName = this.auditTypeName,
				moduleName = this.moduleName,
				entityName = this.entityName,
				userName = this.userName,
				message = this.message,
				errorMessage = this.errorMessage,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetryErrorEvent Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a TelemetryErrorEvent Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a TelemetryErrorEvent Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetryErrorEvent Object.
		///
		/// </summary>
		public static object CreateAnonymous(Telemetry.Database.TelemetryErrorEvent telemetryErrorEvent)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (telemetryErrorEvent == null)
			{
				return null;
			}

			return new {
				id = telemetryErrorEvent.id,
				telemetryApplicationId = telemetryErrorEvent.telemetryApplicationId,
				telemetrySnapshotId = telemetryErrorEvent.telemetrySnapshotId,
				originalAuditEventId = telemetryErrorEvent.originalAuditEventId,
				occurredAt = telemetryErrorEvent.occurredAt,
				auditTypeName = telemetryErrorEvent.auditTypeName,
				moduleName = telemetryErrorEvent.moduleName,
				entityName = telemetryErrorEvent.entityName,
				userName = telemetryErrorEvent.userName,
				message = telemetryErrorEvent.message,
				errorMessage = telemetryErrorEvent.errorMessage,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a TelemetryErrorEvent Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(TelemetryErrorEvent telemetryErrorEvent)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (telemetryErrorEvent == null)
			{
				return null;
			}

			return new {
				id = telemetryErrorEvent.id,
				telemetryApplicationId = telemetryErrorEvent.telemetryApplicationId,
				telemetrySnapshotId = telemetryErrorEvent.telemetrySnapshotId,
				originalAuditEventId = telemetryErrorEvent.originalAuditEventId,
				occurredAt = telemetryErrorEvent.occurredAt,
				auditTypeName = telemetryErrorEvent.auditTypeName,
				moduleName = telemetryErrorEvent.moduleName,
				entityName = telemetryErrorEvent.entityName,
				userName = telemetryErrorEvent.userName,
				message = telemetryErrorEvent.message,
				errorMessage = telemetryErrorEvent.errorMessage,
				telemetryApplication = TelemetryApplication.CreateMinimalAnonymous(telemetryErrorEvent.telemetryApplication),
				telemetrySnapshot = TelemetrySnapshot.CreateMinimalAnonymous(telemetryErrorEvent.telemetrySnapshot)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a TelemetryErrorEvent Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(TelemetryErrorEvent telemetryErrorEvent)
		{
			//
			// Return a very minimal object.
			//
			if (telemetryErrorEvent == null)
			{
				return null;
			}

			return new {
				id = telemetryErrorEvent.id,
				name = telemetryErrorEvent.auditTypeName,
				description = string.Join(", ", new[] { telemetryErrorEvent.auditTypeName, telemetryErrorEvent.moduleName, telemetryErrorEvent.entityName}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
