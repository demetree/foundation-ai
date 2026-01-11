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
	public partial class AuditEvent : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AuditEventDTO
		{
			public Int32 id { get; set; }
			[Required]
			public DateTime startTime { get; set; }
			[Required]
			public DateTime stopTime { get; set; }
			[Required]
			public Boolean completedSuccessfully { get; set; }
			[Required]
			public Int32 auditUserId { get; set; }
			[Required]
			public Int32 auditSessionId { get; set; }
			[Required]
			public Int32 auditTypeId { get; set; }
			[Required]
			public Int32 auditAccessTypeId { get; set; }
			[Required]
			public Int32 auditSourceId { get; set; }
			[Required]
			public Int32 auditUserAgentId { get; set; }
			[Required]
			public Int32 auditModuleId { get; set; }
			[Required]
			public Int32 auditModuleEntityId { get; set; }
			[Required]
			public Int32 auditResourceId { get; set; }
			[Required]
			public Int32 auditHostSystemId { get; set; }
			public String primaryKey { get; set; }
			public Int32? threadId { get; set; }
			[Required]
			public String message { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class AuditEventOutputDTO : AuditEventDTO
		{
			public AuditAccessType.AuditAccessTypeDTO auditAccessType { get; set; }
			public AuditHostSystem.AuditHostSystemDTO auditHostSystem { get; set; }
			public AuditModule.AuditModuleDTO auditModule { get; set; }
			public AuditModuleEntity.AuditModuleEntityDTO auditModuleEntity { get; set; }
			public AuditResource.AuditResourceDTO auditResource { get; set; }
			public AuditSession.AuditSessionDTO auditSession { get; set; }
			public AuditSource.AuditSourceDTO auditSource { get; set; }
			public AuditType.AuditTypeDTO auditType { get; set; }
			public AuditUser.AuditUserDTO auditUser { get; set; }
			public AuditUserAgent.AuditUserAgentDTO auditUserAgent { get; set; }
		}


		/// <summary>
		///
		/// Converts a AuditEvent to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AuditEventDTO ToDTO()
		{
			return new AuditEventDTO
			{
				id = this.id,
				startTime = this.startTime,
				stopTime = this.stopTime,
				completedSuccessfully = this.completedSuccessfully,
				auditUserId = this.auditUserId,
				auditSessionId = this.auditSessionId,
				auditTypeId = this.auditTypeId,
				auditAccessTypeId = this.auditAccessTypeId,
				auditSourceId = this.auditSourceId,
				auditUserAgentId = this.auditUserAgentId,
				auditModuleId = this.auditModuleId,
				auditModuleEntityId = this.auditModuleEntityId,
				auditResourceId = this.auditResourceId,
				auditHostSystemId = this.auditHostSystemId,
				primaryKey = this.primaryKey,
				threadId = this.threadId,
				message = this.message
			};
		}


		/// <summary>
		///
		/// Converts a AuditEvent list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AuditEventDTO> ToDTOList(List<AuditEvent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditEventDTO> output = new List<AuditEventDTO>();

			output.Capacity = data.Count;

			foreach (AuditEvent auditEvent in data)
			{
				output.Add(auditEvent.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AuditEvent to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AuditEventEntity type directly.
		///
		/// </summary>
		public AuditEventOutputDTO ToOutputDTO()
		{
			return new AuditEventOutputDTO
			{
				id = this.id,
				startTime = this.startTime,
				stopTime = this.stopTime,
				completedSuccessfully = this.completedSuccessfully,
				auditUserId = this.auditUserId,
				auditSessionId = this.auditSessionId,
				auditTypeId = this.auditTypeId,
				auditAccessTypeId = this.auditAccessTypeId,
				auditSourceId = this.auditSourceId,
				auditUserAgentId = this.auditUserAgentId,
				auditModuleId = this.auditModuleId,
				auditModuleEntityId = this.auditModuleEntityId,
				auditResourceId = this.auditResourceId,
				auditHostSystemId = this.auditHostSystemId,
				primaryKey = this.primaryKey,
				threadId = this.threadId,
				message = this.message,
				auditAccessType = this.auditAccessType?.ToDTO(),
				auditHostSystem = this.auditHostSystem?.ToDTO(),
				auditModule = this.auditModule?.ToDTO(),
				auditModuleEntity = this.auditModuleEntity?.ToDTO(),
				auditResource = this.auditResource?.ToDTO(),
				auditSession = this.auditSession?.ToDTO(),
				auditSource = this.auditSource?.ToDTO(),
				auditType = this.auditType?.ToDTO(),
				auditUser = this.auditUser?.ToDTO(),
				auditUserAgent = this.auditUserAgent?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a AuditEvent list to list of Output Data Transfer Object intended to be used for serializing a list of AuditEvent objects to avoid using the AuditEvent entity type directly.
		///
		/// </summary>
		public static List<AuditEventOutputDTO> ToOutputDTOList(List<AuditEvent> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditEventOutputDTO> output = new List<AuditEventOutputDTO>();

			output.Capacity = data.Count;

			foreach (AuditEvent auditEvent in data)
			{
				output.Add(auditEvent.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AuditEvent Object.
		///
		/// </summary>
		public static Database.AuditEvent FromDTO(AuditEventDTO dto)
		{
			return new Database.AuditEvent
			{
				id = dto.id,
				startTime = dto.startTime,
				stopTime = dto.stopTime,
				completedSuccessfully = dto.completedSuccessfully,
				auditUserId = dto.auditUserId,
				auditSessionId = dto.auditSessionId,
				auditTypeId = dto.auditTypeId,
				auditAccessTypeId = dto.auditAccessTypeId,
				auditSourceId = dto.auditSourceId,
				auditUserAgentId = dto.auditUserAgentId,
				auditModuleId = dto.auditModuleId,
				auditModuleEntityId = dto.auditModuleEntityId,
				auditResourceId = dto.auditResourceId,
				auditHostSystemId = dto.auditHostSystemId,
				primaryKey = dto.primaryKey,
				threadId = dto.threadId,
				message = dto.message
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AuditEvent Object.
		///
		/// </summary>
		public void ApplyDTO(AuditEventDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.startTime = dto.startTime;
			this.stopTime = dto.stopTime;
			this.completedSuccessfully = dto.completedSuccessfully;
			this.auditUserId = dto.auditUserId;
			this.auditSessionId = dto.auditSessionId;
			this.auditTypeId = dto.auditTypeId;
			this.auditAccessTypeId = dto.auditAccessTypeId;
			this.auditSourceId = dto.auditSourceId;
			this.auditUserAgentId = dto.auditUserAgentId;
			this.auditModuleId = dto.auditModuleId;
			this.auditModuleEntityId = dto.auditModuleEntityId;
			this.auditResourceId = dto.auditResourceId;
			this.auditHostSystemId = dto.auditHostSystemId;
			this.primaryKey = dto.primaryKey;
			this.threadId = dto.threadId;
			this.message = dto.message;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a AuditEvent Object.
		///
		/// </summary>
		public AuditEvent Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AuditEvent{
				id = this.id,
				startTime = this.startTime,
				stopTime = this.stopTime,
				completedSuccessfully = this.completedSuccessfully,
				auditUserId = this.auditUserId,
				auditSessionId = this.auditSessionId,
				auditTypeId = this.auditTypeId,
				auditAccessTypeId = this.auditAccessTypeId,
				auditSourceId = this.auditSourceId,
				auditUserAgentId = this.auditUserAgentId,
				auditModuleId = this.auditModuleId,
				auditModuleEntityId = this.auditModuleEntityId,
				auditResourceId = this.auditResourceId,
				auditHostSystemId = this.auditHostSystemId,
				primaryKey = this.primaryKey,
				threadId = this.threadId,
				message = this.message,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditEvent Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditEvent Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AuditEvent Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AuditEvent Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AuditEvent auditEvent)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (auditEvent == null)
			{
				return null;
			}

			return new {
				id = auditEvent.id,
				startTime = auditEvent.startTime,
				stopTime = auditEvent.stopTime,
				completedSuccessfully = auditEvent.completedSuccessfully,
				auditUserId = auditEvent.auditUserId,
				auditSessionId = auditEvent.auditSessionId,
				auditTypeId = auditEvent.auditTypeId,
				auditAccessTypeId = auditEvent.auditAccessTypeId,
				auditSourceId = auditEvent.auditSourceId,
				auditUserAgentId = auditEvent.auditUserAgentId,
				auditModuleId = auditEvent.auditModuleId,
				auditModuleEntityId = auditEvent.auditModuleEntityId,
				auditResourceId = auditEvent.auditResourceId,
				auditHostSystemId = auditEvent.auditHostSystemId,
				primaryKey = auditEvent.primaryKey,
				threadId = auditEvent.threadId,
				message = auditEvent.message,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AuditEvent Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AuditEvent auditEvent)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (auditEvent == null)
			{
				return null;
			}

			return new {
				id = auditEvent.id,
				startTime = auditEvent.startTime,
				stopTime = auditEvent.stopTime,
				completedSuccessfully = auditEvent.completedSuccessfully,
				auditUserId = auditEvent.auditUserId,
				auditSessionId = auditEvent.auditSessionId,
				auditTypeId = auditEvent.auditTypeId,
				auditAccessTypeId = auditEvent.auditAccessTypeId,
				auditSourceId = auditEvent.auditSourceId,
				auditUserAgentId = auditEvent.auditUserAgentId,
				auditModuleId = auditEvent.auditModuleId,
				auditModuleEntityId = auditEvent.auditModuleEntityId,
				auditResourceId = auditEvent.auditResourceId,
				auditHostSystemId = auditEvent.auditHostSystemId,
				primaryKey = auditEvent.primaryKey,
				threadId = auditEvent.threadId,
				message = auditEvent.message,
				auditAccessType = AuditAccessType.CreateMinimalAnonymous(auditEvent.auditAccessType),
				auditHostSystem = AuditHostSystem.CreateMinimalAnonymous(auditEvent.auditHostSystem),
				auditModule = AuditModule.CreateMinimalAnonymous(auditEvent.auditModule),
				auditModuleEntity = AuditModuleEntity.CreateMinimalAnonymous(auditEvent.auditModuleEntity),
				auditResource = AuditResource.CreateMinimalAnonymous(auditEvent.auditResource),
				auditSession = AuditSession.CreateMinimalAnonymous(auditEvent.auditSession),
				auditSource = AuditSource.CreateMinimalAnonymous(auditEvent.auditSource),
				auditType = AuditType.CreateMinimalAnonymous(auditEvent.auditType),
				auditUser = AuditUser.CreateMinimalAnonymous(auditEvent.auditUser),
				auditUserAgent = AuditUserAgent.CreateMinimalAnonymous(auditEvent.auditUserAgent)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AuditEvent Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AuditEvent auditEvent)
		{
			//
			// Return a very minimal object.
			//
			if (auditEvent == null)
			{
				return null;
			}

			return new {
				id = auditEvent.id,
				name = string.Join(", ", new[] { auditEvent.message}.Where(s => !string.IsNullOrWhiteSpace(s))),
				description = string.Join(", ", new[] { auditEvent.primaryKey}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
