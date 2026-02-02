/*

   GENERATED SERVICE FOR THE ALERTING TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Alerting table.

   It should suffice for many workflows and data access needs, but if anything more is needed, then extend this in a 
   custom version or add an additional targeted helper service.

*/
import {Injectable} from '@angular/core';
import {EscalationPolicyService} from  './escalation-policy.service';
import {EscalationPolicyChangeHistoryService} from  './escalation-policy-change-history.service';
import {EscalationRuleService} from  './escalation-rule.service';
import {EscalationRuleChangeHistoryService} from  './escalation-rule-change-history.service';
import {IncidentService} from  './incident.service';
import {IncidentChangeHistoryService} from  './incident-change-history.service';
import {IncidentEventTypeService} from  './incident-event-type.service';
import {IncidentNoteService} from  './incident-note.service';
import {IncidentNoteChangeHistoryService} from  './incident-note-change-history.service';
import {IncidentNotificationService} from  './incident-notification.service';
import {IncidentStatusTypeService} from  './incident-status-type.service';
import {IncidentTimelineEventService} from  './incident-timeline-event.service';
import {IntegrationService} from  './integration.service';
import {IntegrationCallbackIncidentEventTypeService} from  './integration-callback-incident-event-type.service';
import {IntegrationCallbackIncidentEventTypeChangeHistoryService} from  './integration-callback-incident-event-type-change-history.service';
import {IntegrationChangeHistoryService} from  './integration-change-history.service';
import {NotificationChannelTypeService} from  './notification-channel-type.service';
import {NotificationDeliveryAttemptService} from  './notification-delivery-attempt.service';
import {OnCallScheduleService} from  './on-call-schedule.service';
import {OnCallScheduleChangeHistoryService} from  './on-call-schedule-change-history.service';
import {ScheduleLayerService} from  './schedule-layer.service';
import {ScheduleLayerChangeHistoryService} from  './schedule-layer-change-history.service';
import {ScheduleLayerMemberService} from  './schedule-layer-member.service';
import {ScheduleLayerMemberChangeHistoryService} from  './schedule-layer-member-change-history.service';
import {ServiceService} from  './service.service';
import {ServiceChangeHistoryService} from  './service-change-history.service';
import {SeverityTypeService} from  './severity-type.service';
import {UserNotificationChannelPreferenceService} from  './user-notification-channel-preference.service';
import {UserNotificationChannelPreferenceChangeHistoryService} from  './user-notification-channel-preference-change-history.service';
import {UserNotificationPreferenceService} from  './user-notification-preference.service';
import {UserNotificationPreferenceChangeHistoryService} from  './user-notification-preference-change-history.service';
import {WebhookDeliveryAttemptService} from  './webhook-delivery-attempt.service';


@Injectable({
  providedIn: 'root'
})
export class AlertingDataServiceManagerService  {

    constructor(public escalationPolicyService: EscalationPolicyService
              , public escalationPolicyChangeHistoryService: EscalationPolicyChangeHistoryService
              , public escalationRuleService: EscalationRuleService
              , public escalationRuleChangeHistoryService: EscalationRuleChangeHistoryService
              , public incidentService: IncidentService
              , public incidentChangeHistoryService: IncidentChangeHistoryService
              , public incidentEventTypeService: IncidentEventTypeService
              , public incidentNoteService: IncidentNoteService
              , public incidentNoteChangeHistoryService: IncidentNoteChangeHistoryService
              , public incidentNotificationService: IncidentNotificationService
              , public incidentStatusTypeService: IncidentStatusTypeService
              , public incidentTimelineEventService: IncidentTimelineEventService
              , public integrationService: IntegrationService
              , public integrationCallbackIncidentEventTypeService: IntegrationCallbackIncidentEventTypeService
              , public integrationCallbackIncidentEventTypeChangeHistoryService: IntegrationCallbackIncidentEventTypeChangeHistoryService
              , public integrationChangeHistoryService: IntegrationChangeHistoryService
              , public notificationChannelTypeService: NotificationChannelTypeService
              , public notificationDeliveryAttemptService: NotificationDeliveryAttemptService
              , public onCallScheduleService: OnCallScheduleService
              , public onCallScheduleChangeHistoryService: OnCallScheduleChangeHistoryService
              , public scheduleLayerService: ScheduleLayerService
              , public scheduleLayerChangeHistoryService: ScheduleLayerChangeHistoryService
              , public scheduleLayerMemberService: ScheduleLayerMemberService
              , public scheduleLayerMemberChangeHistoryService: ScheduleLayerMemberChangeHistoryService
              , public serviceService: ServiceService
              , public serviceChangeHistoryService: ServiceChangeHistoryService
              , public severityTypeService: SeverityTypeService
              , public userNotificationChannelPreferenceService: UserNotificationChannelPreferenceService
              , public userNotificationChannelPreferenceChangeHistoryService: UserNotificationChannelPreferenceChangeHistoryService
              , public userNotificationPreferenceService: UserNotificationPreferenceService
              , public userNotificationPreferenceChangeHistoryService: UserNotificationPreferenceChangeHistoryService
              , public webhookDeliveryAttemptService: WebhookDeliveryAttemptService
) { }  


    public ClearAllCaches() {

        this.escalationPolicyService.ClearAllCaches();
        this.escalationPolicyChangeHistoryService.ClearAllCaches();
        this.escalationRuleService.ClearAllCaches();
        this.escalationRuleChangeHistoryService.ClearAllCaches();
        this.incidentService.ClearAllCaches();
        this.incidentChangeHistoryService.ClearAllCaches();
        this.incidentEventTypeService.ClearAllCaches();
        this.incidentNoteService.ClearAllCaches();
        this.incidentNoteChangeHistoryService.ClearAllCaches();
        this.incidentNotificationService.ClearAllCaches();
        this.incidentStatusTypeService.ClearAllCaches();
        this.incidentTimelineEventService.ClearAllCaches();
        this.integrationService.ClearAllCaches();
        this.integrationCallbackIncidentEventTypeService.ClearAllCaches();
        this.integrationCallbackIncidentEventTypeChangeHistoryService.ClearAllCaches();
        this.integrationChangeHistoryService.ClearAllCaches();
        this.notificationChannelTypeService.ClearAllCaches();
        this.notificationDeliveryAttemptService.ClearAllCaches();
        this.onCallScheduleService.ClearAllCaches();
        this.onCallScheduleChangeHistoryService.ClearAllCaches();
        this.scheduleLayerService.ClearAllCaches();
        this.scheduleLayerChangeHistoryService.ClearAllCaches();
        this.scheduleLayerMemberService.ClearAllCaches();
        this.scheduleLayerMemberChangeHistoryService.ClearAllCaches();
        this.serviceService.ClearAllCaches();
        this.serviceChangeHistoryService.ClearAllCaches();
        this.severityTypeService.ClearAllCaches();
        this.userNotificationChannelPreferenceService.ClearAllCaches();
        this.userNotificationChannelPreferenceChangeHistoryService.ClearAllCaches();
        this.userNotificationPreferenceService.ClearAllCaches();
        this.userNotificationPreferenceChangeHistoryService.ClearAllCaches();
        this.webhookDeliveryAttemptService.ClearAllCaches();
    }
}