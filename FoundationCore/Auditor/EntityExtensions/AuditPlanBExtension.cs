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
	public partial class AuditPlanB : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class AuditPlanBDTO
		{
			public Int32 id { get; set; }
			[Required]
			public DateTime startTime { get; set; }
			[Required]
			public DateTime stopTime { get; set; }
			[Required]
			public Boolean completedSuccessfully { get; set; }
			public String user { get; set; }
			public String session { get; set; }
			public String type { get; set; }
			public String accessType { get; set; }
			public String source { get; set; }
			public String userAgent { get; set; }
			public String module { get; set; }
			public String moduleEntity { get; set; }
			public String resource { get; set; }
			public String hostSystem { get; set; }
			public String primaryKey { get; set; }
			public Int32? threadId { get; set; }
			public String message { get; set; }
			public String beforeState { get; set; }
			public String afterState { get; set; }
			public String errorMessage { get; set; }
			public String exceptionText { get; set; }
		}


		/// <summary>
		///
		/// OUTPUT Data Transfer Object intended to be used sending data out of the system.  Contains all value type properties and first level nav property objects, but no child object lists.
		///
		/// </summary>
		public class AuditPlanBOutputDTO : AuditPlanBDTO
		{
		}


		/// <summary>
		///
		/// Converts a AuditPlanB to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public AuditPlanBDTO ToDTO()
		{
			return new AuditPlanBDTO
			{
				id = this.id,
				startTime = this.startTime,
				stopTime = this.stopTime,
				completedSuccessfully = this.completedSuccessfully,
				user = this.user,
				session = this.session,
				type = this.type,
				accessType = this.accessType,
				source = this.source,
				userAgent = this.userAgent,
				module = this.module,
				moduleEntity = this.moduleEntity,
				resource = this.resource,
				hostSystem = this.hostSystem,
				primaryKey = this.primaryKey,
				threadId = this.threadId,
				message = this.message,
				beforeState = this.beforeState,
				afterState = this.afterState,
				errorMessage = this.errorMessage,
				exceptionText = this.exceptionText
			};
		}


		/// <summary>
		///
		/// Converts a AuditPlanB list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<AuditPlanBDTO> ToDTOList(List<AuditPlanB> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditPlanBDTO> output = new List<AuditPlanBDTO>();

			output.Capacity = data.Count;

			foreach (AuditPlanB auditPlanB in data)
			{
				output.Add(auditPlanB.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a AuditPlanB to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the AuditPlanBEntity type directly.
		///
		/// </summary>
		public AuditPlanBOutputDTO ToOutputDTO()
		{
			return new AuditPlanBOutputDTO
			{
				id = this.id,
				startTime = this.startTime,
				stopTime = this.stopTime,
				completedSuccessfully = this.completedSuccessfully,
				user = this.user,
				session = this.session,
				type = this.type,
				accessType = this.accessType,
				source = this.source,
				userAgent = this.userAgent,
				module = this.module,
				moduleEntity = this.moduleEntity,
				resource = this.resource,
				hostSystem = this.hostSystem,
				primaryKey = this.primaryKey,
				threadId = this.threadId,
				message = this.message,
				beforeState = this.beforeState,
				afterState = this.afterState,
				errorMessage = this.errorMessage,
				exceptionText = this.exceptionText
			};
		}


		/// <summary>
		///
		/// Converts a AuditPlanB list to list of Output Data Transfer Object intended to be used for serializing a list of AuditPlanB objects to avoid using the AuditPlanB entity type directly.
		///
		/// </summary>
		public static List<AuditPlanBOutputDTO> ToOutputDTOList(List<AuditPlanB> data)
		{
			if (data == null)
			{
				return null;
			}

			List<AuditPlanBOutputDTO> output = new List<AuditPlanBOutputDTO>();

			output.Capacity = data.Count;

			foreach (AuditPlanB auditPlanB in data)
			{
				output.Add(auditPlanB.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a AuditPlanB Object.
		///
		/// </summary>
		public static Database.AuditPlanB FromDTO(AuditPlanBDTO dto)
		{
			return new Database.AuditPlanB
			{
				id = dto.id,
				startTime = dto.startTime,
				stopTime = dto.stopTime,
				completedSuccessfully = dto.completedSuccessfully,
				user = dto.user,
				session = dto.session,
				type = dto.type,
				accessType = dto.accessType,
				source = dto.source,
				userAgent = dto.userAgent,
				module = dto.module,
				moduleEntity = dto.moduleEntity,
				resource = dto.resource,
				hostSystem = dto.hostSystem,
				primaryKey = dto.primaryKey,
				threadId = dto.threadId,
				message = dto.message,
				beforeState = dto.beforeState,
				afterState = dto.afterState,
				errorMessage = dto.errorMessage,
				exceptionText = dto.exceptionText
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a AuditPlanB Object.
		///
		/// </summary>
		public void ApplyDTO(AuditPlanBDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.startTime = dto.startTime;
			this.stopTime = dto.stopTime;
			this.completedSuccessfully = dto.completedSuccessfully;
			this.user = dto.user;
			this.session = dto.session;
			this.type = dto.type;
			this.accessType = dto.accessType;
			this.source = dto.source;
			this.userAgent = dto.userAgent;
			this.module = dto.module;
			this.moduleEntity = dto.moduleEntity;
			this.resource = dto.resource;
			this.hostSystem = dto.hostSystem;
			this.primaryKey = dto.primaryKey;
			this.threadId = dto.threadId;
			this.message = dto.message;
			this.beforeState = dto.beforeState;
			this.afterState = dto.afterState;
			this.errorMessage = dto.errorMessage;
			this.exceptionText = dto.exceptionText;
		}


		/// <summary>
		///
		/// Creates a deep copy clone of a AuditPlanB Object.
		///
		/// </summary>
		public AuditPlanB Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new AuditPlanB{
				id = this.id,
				startTime = this.startTime,
				stopTime = this.stopTime,
				completedSuccessfully = this.completedSuccessfully,
				user = this.user,
				session = this.session,
				type = this.type,
				accessType = this.accessType,
				source = this.source,
				userAgent = this.userAgent,
				module = this.module,
				moduleEntity = this.moduleEntity,
				resource = this.resource,
				hostSystem = this.hostSystem,
				primaryKey = this.primaryKey,
				threadId = this.threadId,
				message = this.message,
				beforeState = this.beforeState,
				afterState = this.afterState,
				errorMessage = this.errorMessage,
				exceptionText = this.exceptionText
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditPlanB Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a AuditPlanB Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a AuditPlanB Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a AuditPlanB Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.AuditPlanB auditPlanB)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (auditPlanB == null)
			{
				return null;
			}

			return new {
				id = auditPlanB.id,
				startTime = auditPlanB.startTime,
				stopTime = auditPlanB.stopTime,
				completedSuccessfully = auditPlanB.completedSuccessfully,
				user = auditPlanB.user,
				session = auditPlanB.session,
				type = auditPlanB.type,
				accessType = auditPlanB.accessType,
				source = auditPlanB.source,
				userAgent = auditPlanB.userAgent,
				module = auditPlanB.module,
				moduleEntity = auditPlanB.moduleEntity,
				resource = auditPlanB.resource,
				hostSystem = auditPlanB.hostSystem,
				primaryKey = auditPlanB.primaryKey,
				threadId = auditPlanB.threadId,
				message = auditPlanB.message,
				beforeState = auditPlanB.beforeState,
				afterState = auditPlanB.afterState,
				errorMessage = auditPlanB.errorMessage,
				exceptionText = auditPlanB.exceptionText
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a AuditPlanB Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(AuditPlanB auditPlanB)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (auditPlanB == null)
			{
				return null;
			}

			return new {
				id = auditPlanB.id,
				startTime = auditPlanB.startTime,
				stopTime = auditPlanB.stopTime,
				completedSuccessfully = auditPlanB.completedSuccessfully,
				user = auditPlanB.user,
				session = auditPlanB.session,
				type = auditPlanB.type,
				accessType = auditPlanB.accessType,
				source = auditPlanB.source,
				userAgent = auditPlanB.userAgent,
				module = auditPlanB.module,
				moduleEntity = auditPlanB.moduleEntity,
				resource = auditPlanB.resource,
				hostSystem = auditPlanB.hostSystem,
				primaryKey = auditPlanB.primaryKey,
				threadId = auditPlanB.threadId,
				message = auditPlanB.message,
				beforeState = auditPlanB.beforeState,
				afterState = auditPlanB.afterState,
				errorMessage = auditPlanB.errorMessage,
				exceptionText = auditPlanB.exceptionText
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a AuditPlanB Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(AuditPlanB auditPlanB)
		{
			//
			// Return a very minimal object.
			//
			if (auditPlanB == null)
			{
				return null;
			}

			return new {
				id = auditPlanB.id,
				name = string.Join(", ", new[] { auditPlanB.message}.Where(s => !string.IsNullOrWhiteSpace(s))),
				description = string.Join(", ", new[] { auditPlanB.user, auditPlanB.session, auditPlanB.type}.Where(s => !string.IsNullOrWhiteSpace(s)))
			 };
		}
	}
}
