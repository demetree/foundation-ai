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
	public partial class WebhookDeliveryAttempt : IAnonymousConvertible
	{
		/// <summary>
		///
		/// INPUT Data Transfer Object intended to be used for posting data into the system from the outside.  It only contains user editable value type properties.
		///
		/// Required fields are given the Required decorator
		///
		/// </summary>
		public class WebhookDeliveryAttemptDTO
		{
			public Int32 id { get; set; }
			[Required]
			public Int32 incidentId { get; set; }
			[Required]
			public Int32 integrationId { get; set; }
			public Int32? incidentTimelineEventId { get; set; }
			[Required]
			public Int32 attemptNumber { get; set; }
			[Required]
			public DateTime attemptedAt { get; set; }
			public Int32? httpStatusCode { get; set; }
			[Required]
			public Boolean success { get; set; }
			public String payloadJson { get; set; }
			public String responseBody { get; set; }
			public String errorMessage { get; set; }
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
		public class WebhookDeliveryAttemptOutputDTO : WebhookDeliveryAttemptDTO
		{
			public Incident.IncidentDTO incident { get; set; }
			public IncidentTimelineEvent.IncidentTimelineEventDTO incidentTimelineEvent { get; set; }
			public Integration.IntegrationDTO integration { get; set; }
		}


		/// <summary>
		///
		/// Converts a WebhookDeliveryAttempt to an INPUT (or No Nav property OUTPUT) Data Transfer Object intended to be used for posting data into the system from the outside, or or outputting data without nav properties.
		///
		/// Note that sub objects use the base DTO, not the output DTO, so they will not have any nav properties on them, and this is by design.
		///
		/// </summary>
		public WebhookDeliveryAttemptDTO ToDTO()
		{
			return new WebhookDeliveryAttemptDTO
			{
				id = this.id,
				incidentId = this.incidentId,
				integrationId = this.integrationId,
				incidentTimelineEventId = this.incidentTimelineEventId,
				attemptNumber = this.attemptNumber,
				attemptedAt = this.attemptedAt,
				httpStatusCode = this.httpStatusCode,
				success = this.success,
				payloadJson = this.payloadJson,
				responseBody = this.responseBody,
				errorMessage = this.errorMessage,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted
			};
		}


		/// <summary>
		///
		/// Converts a WebhookDeliveryAttempt list to list of INPUT Data Transfer Object intended to be used for posting data into the system, or outputting data without nav properties.
		///
		/// </summary>
		public static List<WebhookDeliveryAttemptDTO> ToDTOList(List<WebhookDeliveryAttempt> data)
		{
			if (data == null)
			{
				return null;
			}

			List<WebhookDeliveryAttemptDTO> output = new List<WebhookDeliveryAttemptDTO>();

			output.Capacity = data.Count;

			foreach (WebhookDeliveryAttempt webhookDeliveryAttempt in data)
			{
				output.Add(webhookDeliveryAttempt.ToDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts a WebhookDeliveryAttempt to an OUTPUT Data Transfer Object.  This is the format to be used when serializing data to send back to client requests with nav properties to avoid using the WebhookDeliveryAttemptEntity type directly.
		///
		/// </summary>
		public WebhookDeliveryAttemptOutputDTO ToOutputDTO()
		{
			return new WebhookDeliveryAttemptOutputDTO
			{
				id = this.id,
				incidentId = this.incidentId,
				integrationId = this.integrationId,
				incidentTimelineEventId = this.incidentTimelineEventId,
				attemptNumber = this.attemptNumber,
				attemptedAt = this.attemptedAt,
				httpStatusCode = this.httpStatusCode,
				success = this.success,
				payloadJson = this.payloadJson,
				responseBody = this.responseBody,
				errorMessage = this.errorMessage,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
				incident = this.incident?.ToDTO(),
				incidentTimelineEvent = this.incidentTimelineEvent?.ToDTO(),
				integration = this.integration?.ToDTO()
			};
		}


		/// <summary>
		///
		/// Converts a WebhookDeliveryAttempt list to list of Output Data Transfer Object intended to be used for serializing a list of WebhookDeliveryAttempt objects to avoid using the WebhookDeliveryAttempt entity type directly.
		///
		/// </summary>
		public static List<WebhookDeliveryAttemptOutputDTO> ToOutputDTOList(List<WebhookDeliveryAttempt> data)
		{
			if (data == null)
			{
				return null;
			}

			List<WebhookDeliveryAttemptOutputDTO> output = new List<WebhookDeliveryAttemptOutputDTO>();

			output.Capacity = data.Count;

			foreach (WebhookDeliveryAttempt webhookDeliveryAttempt in data)
			{
				output.Add(webhookDeliveryAttempt.ToOutputDTO());
			}

			return output;
		}


		/// <summary>
		///
		/// Converts an INPUT DTO to a WebhookDeliveryAttempt Object.
		///
		/// </summary>
		public static Database.WebhookDeliveryAttempt FromDTO(WebhookDeliveryAttemptDTO dto)
		{
			return new Database.WebhookDeliveryAttempt
			{
				id = dto.id,
				incidentId = dto.incidentId,
				integrationId = dto.integrationId,
				incidentTimelineEventId = dto.incidentTimelineEventId,
				attemptNumber = dto.attemptNumber,
				attemptedAt = dto.attemptedAt,
				httpStatusCode = dto.httpStatusCode,
				success = dto.success,
				payloadJson = dto.payloadJson,
				responseBody = dto.responseBody,
				errorMessage = dto.errorMessage,
				objectGuid = dto.objectGuid,
				active = dto.active ?? true,
				deleted = dto.deleted ?? false
			};
		}


		/// <summary>
		///
		/// Applies the values from an INPUT DTO to a WebhookDeliveryAttempt Object.
		///
		/// </summary>
		public void ApplyDTO(WebhookDeliveryAttemptDTO dto)
		{
			if (dto == null || this.id != dto.id)
			{
			    throw new Exception("DTO is null or has an id mismatch.");
			}

			this.incidentId = dto.incidentId;
			this.integrationId = dto.integrationId;
			this.incidentTimelineEventId = dto.incidentTimelineEventId;
			this.attemptNumber = dto.attemptNumber;
			this.attemptedAt = dto.attemptedAt;
			this.httpStatusCode = dto.httpStatusCode;
			this.success = dto.success;
			this.payloadJson = dto.payloadJson;
			this.responseBody = dto.responseBody;
			this.errorMessage = dto.errorMessage;
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
		/// Creates a deep copy clone of a WebhookDeliveryAttempt Object.
		///
		/// </summary>
		public WebhookDeliveryAttempt Clone()
		{
			//
			// Return a cloned object without any object or list properties.
			//
			return new WebhookDeliveryAttempt{
				id = this.id,
				tenantGuid = this.tenantGuid,
				incidentId = this.incidentId,
				integrationId = this.integrationId,
				incidentTimelineEventId = this.incidentTimelineEventId,
				attemptNumber = this.attemptNumber,
				attemptedAt = this.attemptedAt,
				httpStatusCode = this.httpStatusCode,
				success = this.success,
				payloadJson = this.payloadJson,
				responseBody = this.responseBody,
				errorMessage = this.errorMessage,
				objectGuid = this.objectGuid,
				active = this.active,
				deleted = this.deleted,
			 };
		}


        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a WebhookDeliveryAttempt Object.
        ///
        /// </summary>
        public object ToAnonymous()
        {
            return CreateAnonymous(this);
        }

        /// <summary>
        ///
        /// Creates an anonymous object containing properties from a WebhookDeliveryAttempt Object, with minimal versions of first level sub objects
        ///
        /// </summary>
        public object ToAnonymousWithFirstLevelSubObjects()
        {
            return CreateAnonymousWithFirstLevelSubObjects(this);
        }

        /// <summary>
        ///
        /// Creates an minimal anonymous object containing name and description properties from a WebhookDeliveryAttempt Object, as best it can.
        ///
        /// </summary>
        public object ToMinimalAnonymous()
        {
            return CreateMinimalAnonymous(this);
        }



		/// <summary>
		///
		/// Creates an anonymous object version of a WebhookDeliveryAttempt Object.
		///
		/// </summary>
		public static object CreateAnonymous(Database.WebhookDeliveryAttempt webhookDeliveryAttempt)
		{
			//
			// Return a simplified object without any object or list properties.
			//
			if (webhookDeliveryAttempt == null)
			{
				return null;
			}

			return new {
				id = webhookDeliveryAttempt.id,
				incidentId = webhookDeliveryAttempt.incidentId,
				integrationId = webhookDeliveryAttempt.integrationId,
				incidentTimelineEventId = webhookDeliveryAttempt.incidentTimelineEventId,
				attemptNumber = webhookDeliveryAttempt.attemptNumber,
				attemptedAt = webhookDeliveryAttempt.attemptedAt,
				httpStatusCode = webhookDeliveryAttempt.httpStatusCode,
				success = webhookDeliveryAttempt.success,
				payloadJson = webhookDeliveryAttempt.payloadJson,
				responseBody = webhookDeliveryAttempt.responseBody,
				errorMessage = webhookDeliveryAttempt.errorMessage,
				objectGuid = webhookDeliveryAttempt.objectGuid,
				active = webhookDeliveryAttempt.active,
				deleted = webhookDeliveryAttempt.deleted,
			 };
		}

		/// <summary>
		///
		/// Creates an anonymous object version of a WebhookDeliveryAttempt Object with first level sub ojbects.
		///
		/// </summary>
		public static object CreateAnonymousWithFirstLevelSubObjects(WebhookDeliveryAttempt webhookDeliveryAttempt)
		{
			//
			// Return a simplified object with simple first level sub objects.
			//
			if (webhookDeliveryAttempt == null)
			{
				return null;
			}

			return new {
				id = webhookDeliveryAttempt.id,
				incidentId = webhookDeliveryAttempt.incidentId,
				integrationId = webhookDeliveryAttempt.integrationId,
				incidentTimelineEventId = webhookDeliveryAttempt.incidentTimelineEventId,
				attemptNumber = webhookDeliveryAttempt.attemptNumber,
				attemptedAt = webhookDeliveryAttempt.attemptedAt,
				httpStatusCode = webhookDeliveryAttempt.httpStatusCode,
				success = webhookDeliveryAttempt.success,
				payloadJson = webhookDeliveryAttempt.payloadJson,
				responseBody = webhookDeliveryAttempt.responseBody,
				errorMessage = webhookDeliveryAttempt.errorMessage,
				objectGuid = webhookDeliveryAttempt.objectGuid,
				active = webhookDeliveryAttempt.active,
				deleted = webhookDeliveryAttempt.deleted,
				incident = Incident.CreateMinimalAnonymous(webhookDeliveryAttempt.incident),
				incidentTimelineEvent = IncidentTimelineEvent.CreateMinimalAnonymous(webhookDeliveryAttempt.incidentTimelineEvent),
				integration = Integration.CreateMinimalAnonymous(webhookDeliveryAttempt.integration)
			 };
		}

		/// <summary>
		///
		/// Creates an minimal anonymous object version of a WebhookDeliveryAttempt Object.  This has just id, name, and description properties.
		///
		/// </summary>
		public static object CreateMinimalAnonymous(WebhookDeliveryAttempt webhookDeliveryAttempt)
		{
			//
			// Return a very minimal object.
			//
			if (webhookDeliveryAttempt == null)
			{
				return null;
			}

			return new {
				id = webhookDeliveryAttempt.id,
				name = webhookDeliveryAttempt.id,
				description = webhookDeliveryAttempt.id
			 };
		}
	}
}
