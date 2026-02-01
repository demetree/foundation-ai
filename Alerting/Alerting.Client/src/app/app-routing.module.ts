import { NgModule, Injectable } from '@angular/core';
import { RouterModule, Routes, DefaultUrlSerializer, UrlSerializer, UrlTree, TitleStrategy } from '@angular/router';


import { UnsavedChangesGuard } from './guards/unsaved-changes.guard';

import { LoginComponent } from './components/login/login.component';
import { AuthCallbackComponent } from './components/auth-callback/auth-callback.component';

//
// Custom components
//
import { NotFoundComponent } from './components/not-found/not-found.component';
import { AppTitleService } from './services/app-title.service';
import { AuthService } from './services/auth.service';
import { AuthGuard } from './services/auth-guard';
import { Utilities } from './services/utilities';
import { NewUserComponent } from './components/new-user/new-user.component';
import { ResetPasswordComponent } from './components/reset-password/reset-password.component';

import { SystemHealthComponent } from './components/system-health/system-health.component';

//
// Custom screens
//


//
// Beginning of imports for Alerting Data Components
//
import { EscalationPolicyListingComponent } from './alerting-data-components/escalation-policy/escalation-policy-listing/escalation-policy-listing.component';
import { EscalationPolicyDetailComponent } from './alerting-data-components/escalation-policy/escalation-policy-detail/escalation-policy-detail.component';
import { EscalationPolicyChangeHistoryListingComponent } from './alerting-data-components/escalation-policy-change-history/escalation-policy-change-history-listing/escalation-policy-change-history-listing.component';
import { EscalationPolicyChangeHistoryDetailComponent } from './alerting-data-components/escalation-policy-change-history/escalation-policy-change-history-detail/escalation-policy-change-history-detail.component';
import { EscalationRuleListingComponent } from './alerting-data-components/escalation-rule/escalation-rule-listing/escalation-rule-listing.component';
import { EscalationRuleDetailComponent } from './alerting-data-components/escalation-rule/escalation-rule-detail/escalation-rule-detail.component';
import { EscalationRuleChangeHistoryListingComponent } from './alerting-data-components/escalation-rule-change-history/escalation-rule-change-history-listing/escalation-rule-change-history-listing.component';
import { EscalationRuleChangeHistoryDetailComponent } from './alerting-data-components/escalation-rule-change-history/escalation-rule-change-history-detail/escalation-rule-change-history-detail.component';
import { IncidentListingComponent } from './alerting-data-components/incident/incident-listing/incident-listing.component';
import { IncidentDetailComponent } from './alerting-data-components/incident/incident-detail/incident-detail.component';
import { IncidentChangeHistoryListingComponent } from './alerting-data-components/incident-change-history/incident-change-history-listing/incident-change-history-listing.component';
import { IncidentChangeHistoryDetailComponent } from './alerting-data-components/incident-change-history/incident-change-history-detail/incident-change-history-detail.component';
import { IncidentEventTypeListingComponent } from './alerting-data-components/incident-event-type/incident-event-type-listing/incident-event-type-listing.component';
import { IncidentEventTypeDetailComponent } from './alerting-data-components/incident-event-type/incident-event-type-detail/incident-event-type-detail.component';
import { IncidentNoteListingComponent } from './alerting-data-components/incident-note/incident-note-listing/incident-note-listing.component';
import { IncidentNoteDetailComponent } from './alerting-data-components/incident-note/incident-note-detail/incident-note-detail.component';
import { IncidentNoteChangeHistoryListingComponent } from './alerting-data-components/incident-note-change-history/incident-note-change-history-listing/incident-note-change-history-listing.component';
import { IncidentNoteChangeHistoryDetailComponent } from './alerting-data-components/incident-note-change-history/incident-note-change-history-detail/incident-note-change-history-detail.component';
import { IncidentNotificationListingComponent } from './alerting-data-components/incident-notification/incident-notification-listing/incident-notification-listing.component';
import { IncidentNotificationDetailComponent } from './alerting-data-components/incident-notification/incident-notification-detail/incident-notification-detail.component';
import { IncidentStatusTypeListingComponent } from './alerting-data-components/incident-status-type/incident-status-type-listing/incident-status-type-listing.component';
import { IncidentStatusTypeDetailComponent } from './alerting-data-components/incident-status-type/incident-status-type-detail/incident-status-type-detail.component';
import { IncidentTimelineEventListingComponent } from './alerting-data-components/incident-timeline-event/incident-timeline-event-listing/incident-timeline-event-listing.component';
import { IncidentTimelineEventDetailComponent } from './alerting-data-components/incident-timeline-event/incident-timeline-event-detail/incident-timeline-event-detail.component';
import { IntegrationListingComponent } from './alerting-data-components/integration/integration-listing/integration-listing.component';
import { IntegrationDetailComponent } from './alerting-data-components/integration/integration-detail/integration-detail.component';
import { IntegrationChangeHistoryListingComponent } from './alerting-data-components/integration-change-history/integration-change-history-listing/integration-change-history-listing.component';
import { IntegrationChangeHistoryDetailComponent } from './alerting-data-components/integration-change-history/integration-change-history-detail/integration-change-history-detail.component';
import { NotificationChannelTypeListingComponent } from './alerting-data-components/notification-channel-type/notification-channel-type-listing/notification-channel-type-listing.component';
import { NotificationChannelTypeDetailComponent } from './alerting-data-components/notification-channel-type/notification-channel-type-detail/notification-channel-type-detail.component';
import { NotificationDeliveryAttemptListingComponent } from './alerting-data-components/notification-delivery-attempt/notification-delivery-attempt-listing/notification-delivery-attempt-listing.component';
import { NotificationDeliveryAttemptDetailComponent } from './alerting-data-components/notification-delivery-attempt/notification-delivery-attempt-detail/notification-delivery-attempt-detail.component';
import { OnCallScheduleListingComponent } from './alerting-data-components/on-call-schedule/on-call-schedule-listing/on-call-schedule-listing.component';
import { OnCallScheduleDetailComponent } from './alerting-data-components/on-call-schedule/on-call-schedule-detail/on-call-schedule-detail.component';
import { OnCallScheduleChangeHistoryListingComponent } from './alerting-data-components/on-call-schedule-change-history/on-call-schedule-change-history-listing/on-call-schedule-change-history-listing.component';
import { OnCallScheduleChangeHistoryDetailComponent } from './alerting-data-components/on-call-schedule-change-history/on-call-schedule-change-history-detail/on-call-schedule-change-history-detail.component';
import { ScheduleLayerListingComponent } from './alerting-data-components/schedule-layer/schedule-layer-listing/schedule-layer-listing.component';
import { ScheduleLayerDetailComponent } from './alerting-data-components/schedule-layer/schedule-layer-detail/schedule-layer-detail.component';
import { ScheduleLayerChangeHistoryListingComponent } from './alerting-data-components/schedule-layer-change-history/schedule-layer-change-history-listing/schedule-layer-change-history-listing.component';
import { ScheduleLayerChangeHistoryDetailComponent } from './alerting-data-components/schedule-layer-change-history/schedule-layer-change-history-detail/schedule-layer-change-history-detail.component';
import { ScheduleLayerMemberListingComponent } from './alerting-data-components/schedule-layer-member/schedule-layer-member-listing/schedule-layer-member-listing.component';
import { ScheduleLayerMemberDetailComponent } from './alerting-data-components/schedule-layer-member/schedule-layer-member-detail/schedule-layer-member-detail.component';
import { ScheduleLayerMemberChangeHistoryListingComponent } from './alerting-data-components/schedule-layer-member-change-history/schedule-layer-member-change-history-listing/schedule-layer-member-change-history-listing.component';
import { ScheduleLayerMemberChangeHistoryDetailComponent } from './alerting-data-components/schedule-layer-member-change-history/schedule-layer-member-change-history-detail/schedule-layer-member-change-history-detail.component';
import { ServiceListingComponent } from './alerting-data-components/service/service-listing/service-listing.component';
import { ServiceDetailComponent } from './alerting-data-components/service/service-detail/service-detail.component';
import { ServiceChangeHistoryListingComponent } from './alerting-data-components/service-change-history/service-change-history-listing/service-change-history-listing.component';
import { ServiceChangeHistoryDetailComponent } from './alerting-data-components/service-change-history/service-change-history-detail/service-change-history-detail.component';
import { SeverityTypeListingComponent } from './alerting-data-components/severity-type/severity-type-listing/severity-type-listing.component';
import { SeverityTypeDetailComponent } from './alerting-data-components/severity-type/severity-type-detail/severity-type-detail.component';
import { UserNotificationChannelPreferenceListingComponent } from './alerting-data-components/user-notification-channel-preference/user-notification-channel-preference-listing/user-notification-channel-preference-listing.component';
import { UserNotificationChannelPreferenceDetailComponent } from './alerting-data-components/user-notification-channel-preference/user-notification-channel-preference-detail/user-notification-channel-preference-detail.component';
import { UserNotificationChannelPreferenceChangeHistoryListingComponent } from './alerting-data-components/user-notification-channel-preference-change-history/user-notification-channel-preference-change-history-listing/user-notification-channel-preference-change-history-listing.component';
import { UserNotificationChannelPreferenceChangeHistoryDetailComponent } from './alerting-data-components/user-notification-channel-preference-change-history/user-notification-channel-preference-change-history-detail/user-notification-channel-preference-change-history-detail.component';
import { UserNotificationPreferenceListingComponent } from './alerting-data-components/user-notification-preference/user-notification-preference-listing/user-notification-preference-listing.component';
import { UserNotificationPreferenceDetailComponent } from './alerting-data-components/user-notification-preference/user-notification-preference-detail/user-notification-preference-detail.component';
import { UserNotificationPreferenceChangeHistoryListingComponent } from './alerting-data-components/user-notification-preference-change-history/user-notification-preference-change-history-listing/user-notification-preference-change-history-listing.component';
import { UserNotificationPreferenceChangeHistoryDetailComponent } from './alerting-data-components/user-notification-preference-change-history/user-notification-preference-change-history-detail/user-notification-preference-change-history-detail.component';
import { WebhookDeliveryAttemptListingComponent } from './alerting-data-components/webhook-delivery-attempt/webhook-delivery-attempt-listing/webhook-delivery-attempt-listing.component';
import { WebhookDeliveryAttemptDetailComponent } from './alerting-data-components/webhook-delivery-attempt/webhook-delivery-attempt-detail/webhook-delivery-attempt-detail.component';
//
// End of imports for Alerting Data Components
//


@Injectable()
export class LowerCaseUrlSerializer extends DefaultUrlSerializer {
  override parse(url: string): UrlTree {
    const possibleSeparators = /[?;#]/;
    const indexOfSeparator = url.search(possibleSeparators);
    let processedUrl: string;

    if (indexOfSeparator > -1) {
      const separator = url.charAt(indexOfSeparator);
      const urlParts = Utilities.splitInTwo(url, separator);
      urlParts.firstPart = urlParts.firstPart.toLowerCase();

      processedUrl = urlParts.firstPart + separator + urlParts.secondPart;
    } else {
      processedUrl = url.toLowerCase();
    }

    return super.parse(processedUrl);
  }
}


const routes: Routes = [

  //
  // Admin routes
  //
  { path: '', component: IncidentListingComponent, canActivate: [AuthGuard], title: 'Incidents' },
  { path: 'login', component: LoginComponent, title: 'Login' },
  { path: 'google-login', component: AuthCallbackComponent, title: 'Google Login' },
  { path: 'facebook-login', component: AuthCallbackComponent, title: 'Facebook Login' },
  { path: 'twitter-login', component: AuthCallbackComponent, title: 'Twitter Login' },
  { path: 'microsoft-login', component: AuthCallbackComponent, title: 'Microsoft Login' },

  //
  // Custom component routes - admin
  //
  { path: 'new-user/:newUserToken', component: NewUserComponent, title: 'new-user' },
  { path: 'reset-password/:token', component: ResetPasswordComponent, title: 'Reset Password' },

  //
  // Custom component routes - for better business functions - these take precedence over the code gen routes, so they are first
  //

  //
  // Override the resource paths with custom implementations
  //


  { path: 'system-health', component: SystemHealthComponent, canActivate: [AuthGuard], title: 'System Health' },




  //
  // Beginning of routes for Alerting Data Components
//
  {path: 'escalationpolicies', component: EscalationPolicyListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Escalation Policies' },
  {path: 'escalationpolicies/new', component: EscalationPolicyDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Escalation Policy' },
  {path: 'escalationpolicies/:escalationPolicyId', component: EscalationPolicyDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Escalation Policy' },
  {path: 'escalationpolicy/:escalationPolicyId', component: EscalationPolicyDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Escalation Policy' },
  {path: 'escalationpolicy',  redirectTo: 'escalationpolicies'},
  {path: 'escalationpolicychangehistories', component: EscalationPolicyChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Escalation Policy Change Histories' },
  {path: 'escalationpolicychangehistories/new', component: EscalationPolicyChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Escalation Policy Change History' },
  {path: 'escalationpolicychangehistories/:escalationPolicyChangeHistoryId', component: EscalationPolicyChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Escalation Policy Change History' },
  {path: 'escalationpolicychangehistory/:escalationPolicyChangeHistoryId', component: EscalationPolicyChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Escalation Policy Change History' },
  {path: 'escalationpolicychangehistory',  redirectTo: 'escalationpolicychangehistories'},
  {path: 'escalationrules', component: EscalationRuleListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Escalation Rules' },
  {path: 'escalationrules/new', component: EscalationRuleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Escalation Rule' },
  {path: 'escalationrules/:escalationRuleId', component: EscalationRuleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Escalation Rule' },
  {path: 'escalationrule/:escalationRuleId', component: EscalationRuleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Escalation Rule' },
  {path: 'escalationrule',  redirectTo: 'escalationrules'},
  {path: 'escalationrulechangehistories', component: EscalationRuleChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Escalation Rule Change Histories' },
  {path: 'escalationrulechangehistories/new', component: EscalationRuleChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Escalation Rule Change History' },
  {path: 'escalationrulechangehistories/:escalationRuleChangeHistoryId', component: EscalationRuleChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Escalation Rule Change History' },
  {path: 'escalationrulechangehistory/:escalationRuleChangeHistoryId', component: EscalationRuleChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Escalation Rule Change History' },
  {path: 'escalationrulechangehistory',  redirectTo: 'escalationrulechangehistories'},
  {path: 'incidents', component: IncidentListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Incidents' },
  {path: 'incidents/new', component: IncidentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Incident' },
  {path: 'incidents/:incidentId', component: IncidentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Incident' },
  {path: 'incident/:incidentId', component: IncidentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Incident' },
  {path: 'incident',  redirectTo: 'incidents'},
  {path: 'incidentchangehistories', component: IncidentChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Incident Change Histories' },
  {path: 'incidentchangehistories/new', component: IncidentChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Incident Change History' },
  {path: 'incidentchangehistories/:incidentChangeHistoryId', component: IncidentChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Incident Change History' },
  {path: 'incidentchangehistory/:incidentChangeHistoryId', component: IncidentChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Incident Change History' },
  {path: 'incidentchangehistory',  redirectTo: 'incidentchangehistories'},
  {path: 'incidenteventtypes', component: IncidentEventTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Incident Event Types' },
  {path: 'incidenteventtypes/new', component: IncidentEventTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Incident Event Type' },
  {path: 'incidenteventtypes/:incidentEventTypeId', component: IncidentEventTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Incident Event Type' },
  {path: 'incidenteventtype/:incidentEventTypeId', component: IncidentEventTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Incident Event Type' },
  {path: 'incidenteventtype',  redirectTo: 'incidenteventtypes'},
  {path: 'incidentnotes', component: IncidentNoteListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Incident Notes' },
  {path: 'incidentnotes/new', component: IncidentNoteDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Incident Note' },
  {path: 'incidentnotes/:incidentNoteId', component: IncidentNoteDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Incident Note' },
  {path: 'incidentnote/:incidentNoteId', component: IncidentNoteDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Incident Note' },
  {path: 'incidentnote',  redirectTo: 'incidentnotes'},
  {path: 'incidentnotechangehistories', component: IncidentNoteChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Incident Note Change Histories' },
  {path: 'incidentnotechangehistories/new', component: IncidentNoteChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Incident Note Change History' },
  {path: 'incidentnotechangehistories/:incidentNoteChangeHistoryId', component: IncidentNoteChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Incident Note Change History' },
  {path: 'incidentnotechangehistory/:incidentNoteChangeHistoryId', component: IncidentNoteChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Incident Note Change History' },
  {path: 'incidentnotechangehistory',  redirectTo: 'incidentnotechangehistories'},
  {path: 'incidentnotifications', component: IncidentNotificationListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Incident Notifications' },
  {path: 'incidentnotifications/new', component: IncidentNotificationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Incident Notification' },
  {path: 'incidentnotifications/:incidentNotificationId', component: IncidentNotificationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Incident Notification' },
  {path: 'incidentnotification/:incidentNotificationId', component: IncidentNotificationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Incident Notification' },
  {path: 'incidentnotification',  redirectTo: 'incidentnotifications'},
  {path: 'incidentstatustypes', component: IncidentStatusTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Incident Status Types' },
  {path: 'incidentstatustypes/new', component: IncidentStatusTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Incident Status Type' },
  {path: 'incidentstatustypes/:incidentStatusTypeId', component: IncidentStatusTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Incident Status Type' },
  {path: 'incidentstatustype/:incidentStatusTypeId', component: IncidentStatusTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Incident Status Type' },
  {path: 'incidentstatustype',  redirectTo: 'incidentstatustypes'},
  {path: 'incidenttimelineevents', component: IncidentTimelineEventListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Incident Timeline Events' },
  {path: 'incidenttimelineevents/new', component: IncidentTimelineEventDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Incident Timeline Event' },
  {path: 'incidenttimelineevents/:incidentTimelineEventId', component: IncidentTimelineEventDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Incident Timeline Event' },
  {path: 'incidenttimelineevent/:incidentTimelineEventId', component: IncidentTimelineEventDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Incident Timeline Event' },
  {path: 'incidenttimelineevent',  redirectTo: 'incidenttimelineevents'},
  {path: 'integrations', component: IntegrationListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Integrations' },
  {path: 'integrations/new', component: IntegrationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Integration' },
  {path: 'integrations/:integrationId', component: IntegrationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Integration' },
  {path: 'integration/:integrationId', component: IntegrationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Integration' },
  {path: 'integration',  redirectTo: 'integrations'},
  {path: 'integrationchangehistories', component: IntegrationChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Integration Change Histories' },
  {path: 'integrationchangehistories/new', component: IntegrationChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Integration Change History' },
  {path: 'integrationchangehistories/:integrationChangeHistoryId', component: IntegrationChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Integration Change History' },
  {path: 'integrationchangehistory/:integrationChangeHistoryId', component: IntegrationChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Integration Change History' },
  {path: 'integrationchangehistory',  redirectTo: 'integrationchangehistories'},
  {path: 'notificationchanneltypes', component: NotificationChannelTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Notification Channel Types' },
  {path: 'notificationchanneltypes/new', component: NotificationChannelTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Notification Channel Type' },
  {path: 'notificationchanneltypes/:notificationChannelTypeId', component: NotificationChannelTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Notification Channel Type' },
  {path: 'notificationchanneltype/:notificationChannelTypeId', component: NotificationChannelTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Notification Channel Type' },
  {path: 'notificationchanneltype',  redirectTo: 'notificationchanneltypes'},
  {path: 'notificationdeliveryattempts', component: NotificationDeliveryAttemptListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Notification Delivery Attempts' },
  {path: 'notificationdeliveryattempts/new', component: NotificationDeliveryAttemptDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Notification Delivery Attempt' },
  {path: 'notificationdeliveryattempts/:notificationDeliveryAttemptId', component: NotificationDeliveryAttemptDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Notification Delivery Attempt' },
  {path: 'notificationdeliveryattempt/:notificationDeliveryAttemptId', component: NotificationDeliveryAttemptDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Notification Delivery Attempt' },
  {path: 'notificationdeliveryattempt',  redirectTo: 'notificationdeliveryattempts'},
  {path: 'oncallschedules', component: OnCallScheduleListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'On Call Schedules' },
  {path: 'oncallschedules/new', component: OnCallScheduleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create On Call Schedule' },
  {path: 'oncallschedules/:onCallScheduleId', component: OnCallScheduleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit On Call Schedule' },
  {path: 'oncallschedule/:onCallScheduleId', component: OnCallScheduleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit On Call Schedule' },
  {path: 'oncallschedule',  redirectTo: 'oncallschedules'},
  {path: 'oncallschedulechangehistories', component: OnCallScheduleChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'On Call Schedule Change Histories' },
  {path: 'oncallschedulechangehistories/new', component: OnCallScheduleChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create On Call Schedule Change History' },
  {path: 'oncallschedulechangehistories/:onCallScheduleChangeHistoryId', component: OnCallScheduleChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit On Call Schedule Change History' },
  {path: 'oncallschedulechangehistory/:onCallScheduleChangeHistoryId', component: OnCallScheduleChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit On Call Schedule Change History' },
  {path: 'oncallschedulechangehistory',  redirectTo: 'oncallschedulechangehistories'},
  {path: 'schedulelayers', component: ScheduleLayerListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Schedule Layers' },
  {path: 'schedulelayers/new', component: ScheduleLayerDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Schedule Layer' },
  {path: 'schedulelayers/:scheduleLayerId', component: ScheduleLayerDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Schedule Layer' },
  {path: 'schedulelayer/:scheduleLayerId', component: ScheduleLayerDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Schedule Layer' },
  {path: 'schedulelayer',  redirectTo: 'schedulelayers'},
  {path: 'schedulelayerchangehistories', component: ScheduleLayerChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Schedule Layer Change Histories' },
  {path: 'schedulelayerchangehistories/new', component: ScheduleLayerChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Schedule Layer Change History' },
  {path: 'schedulelayerchangehistories/:scheduleLayerChangeHistoryId', component: ScheduleLayerChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Schedule Layer Change History' },
  {path: 'schedulelayerchangehistory/:scheduleLayerChangeHistoryId', component: ScheduleLayerChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Schedule Layer Change History' },
  {path: 'schedulelayerchangehistory',  redirectTo: 'schedulelayerchangehistories'},
  {path: 'schedulelayermembers', component: ScheduleLayerMemberListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Schedule Layer Members' },
  {path: 'schedulelayermembers/new', component: ScheduleLayerMemberDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Schedule Layer Member' },
  {path: 'schedulelayermembers/:scheduleLayerMemberId', component: ScheduleLayerMemberDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Schedule Layer Member' },
  {path: 'schedulelayermember/:scheduleLayerMemberId', component: ScheduleLayerMemberDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Schedule Layer Member' },
  {path: 'schedulelayermember',  redirectTo: 'schedulelayermembers'},
  {path: 'schedulelayermemberchangehistories', component: ScheduleLayerMemberChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Schedule Layer Member Change Histories' },
  {path: 'schedulelayermemberchangehistories/new', component: ScheduleLayerMemberChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Schedule Layer Member Change History' },
  {path: 'schedulelayermemberchangehistories/:scheduleLayerMemberChangeHistoryId', component: ScheduleLayerMemberChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Schedule Layer Member Change History' },
  {path: 'schedulelayermemberchangehistory/:scheduleLayerMemberChangeHistoryId', component: ScheduleLayerMemberChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Schedule Layer Member Change History' },
  {path: 'schedulelayermemberchangehistory',  redirectTo: 'schedulelayermemberchangehistories'},
  {path: 'services', component: ServiceListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Services' },
  {path: 'services/new', component: ServiceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Service' },
  {path: 'services/:serviceId', component: ServiceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Service' },
  {path: 'service/:serviceId', component: ServiceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Service' },
  {path: 'service',  redirectTo: 'services'},
  {path: 'servicechangehistories', component: ServiceChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Service Change Histories' },
  {path: 'servicechangehistories/new', component: ServiceChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Service Change History' },
  {path: 'servicechangehistories/:serviceChangeHistoryId', component: ServiceChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Service Change History' },
  {path: 'servicechangehistory/:serviceChangeHistoryId', component: ServiceChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Service Change History' },
  {path: 'servicechangehistory',  redirectTo: 'servicechangehistories'},
  {path: 'severitytypes', component: SeverityTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Severity Types' },
  {path: 'severitytypes/new', component: SeverityTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Severity Type' },
  {path: 'severitytypes/:severityTypeId', component: SeverityTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Severity Type' },
  {path: 'severitytype/:severityTypeId', component: SeverityTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Severity Type' },
  {path: 'severitytype',  redirectTo: 'severitytypes'},
  {path: 'usernotificationchannelpreferences', component: UserNotificationChannelPreferenceListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'User Notification Channel Preferences' },
  {path: 'usernotificationchannelpreferences/new', component: UserNotificationChannelPreferenceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create User Notification Channel Preference' },
  {path: 'usernotificationchannelpreferences/:userNotificationChannelPreferenceId', component: UserNotificationChannelPreferenceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Notification Channel Preference' },
  {path: 'usernotificationchannelpreference/:userNotificationChannelPreferenceId', component: UserNotificationChannelPreferenceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Notification Channel Preference' },
  {path: 'usernotificationchannelpreference',  redirectTo: 'usernotificationchannelpreferences'},
  {path: 'usernotificationchannelpreferencechangehistories', component: UserNotificationChannelPreferenceChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'User Notification Channel Preference Change Histories' },
  {path: 'usernotificationchannelpreferencechangehistories/new', component: UserNotificationChannelPreferenceChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create User Notification Channel Preference Change History' },
  {path: 'usernotificationchannelpreferencechangehistories/:userNotificationChannelPreferenceChangeHistoryId', component: UserNotificationChannelPreferenceChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Notification Channel Preference Change History' },
  {path: 'usernotificationchannelpreferencechangehistory/:userNotificationChannelPreferenceChangeHistoryId', component: UserNotificationChannelPreferenceChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Notification Channel Preference Change History' },
  {path: 'usernotificationchannelpreferencechangehistory',  redirectTo: 'usernotificationchannelpreferencechangehistories'},
  {path: 'usernotificationpreferences', component: UserNotificationPreferenceListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'User Notification Preferences' },
  {path: 'usernotificationpreferences/new', component: UserNotificationPreferenceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create User Notification Preference' },
  {path: 'usernotificationpreferences/:userNotificationPreferenceId', component: UserNotificationPreferenceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Notification Preference' },
  {path: 'usernotificationpreference/:userNotificationPreferenceId', component: UserNotificationPreferenceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Notification Preference' },
  {path: 'usernotificationpreference',  redirectTo: 'usernotificationpreferences'},
  {path: 'usernotificationpreferencechangehistories', component: UserNotificationPreferenceChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'User Notification Preference Change Histories' },
  {path: 'usernotificationpreferencechangehistories/new', component: UserNotificationPreferenceChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create User Notification Preference Change History' },
  {path: 'usernotificationpreferencechangehistories/:userNotificationPreferenceChangeHistoryId', component: UserNotificationPreferenceChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Notification Preference Change History' },
  {path: 'usernotificationpreferencechangehistory/:userNotificationPreferenceChangeHistoryId', component: UserNotificationPreferenceChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit User Notification Preference Change History' },
  {path: 'usernotificationpreferencechangehistory',  redirectTo: 'usernotificationpreferencechangehistories'},
  {path: 'webhookdeliveryattempts', component: WebhookDeliveryAttemptListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Webhook Delivery Attempts' },
  {path: 'webhookdeliveryattempts/new', component: WebhookDeliveryAttemptDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Webhook Delivery Attempt' },
  {path: 'webhookdeliveryattempts/:webhookDeliveryAttemptId', component: WebhookDeliveryAttemptDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Webhook Delivery Attempt' },
  {path: 'webhookdeliveryattempt/:webhookDeliveryAttemptId', component: WebhookDeliveryAttemptDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Webhook Delivery Attempt' },
  {path: 'webhookdeliveryattempt',  redirectTo: 'webhookdeliveryattempts'},
//
  // End of routes for Alerting Data Components
  //

  //
  // Default routes
  //
  { path: 'home', redirectTo: '/', pathMatch: 'full' },
  { path: '**', component: NotFoundComponent, title: 'Page Not Found' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {
    scrollPositionRestoration: 'enabled',   // Scroll to top or previous position when opening components
    anchorScrolling: 'enabled'              // Enables anchor scrolling so browser will scroll to to fragments within component templates when links with fragment identifiers are clicked (eg. #section1 )
  })],
  exports: [RouterModule],
  providers: [
    AuthService,
    { provide: TitleStrategy, useClass: AppTitleService },
    { provide: UrlSerializer, useClass: LowerCaseUrlSerializer }]
})
export class AppRoutingModule { }
