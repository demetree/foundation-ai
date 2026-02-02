import { NgModule, ErrorHandler } from '@angular/core';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { FormsModule } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';
import { RouteReuseStrategy, RouterModule } from '@angular/router';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { MatExpansionModule } from '@angular/material/expansion';
import { FullCalendarModule } from '@fullcalendar/angular';

import { NgbModalModule, NgbNavModule, NgbTooltipModule, NgbPopoverModule, NgbAccordionModule, NgbDropdownModule, NgbCarouselModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
import { OAuthModule, OAuthStorage } from 'angular-oauth2-oidc';
import { ToastaModule } from 'ngx-toasta';
import { NgSelectModule } from '@ng-select/ng-select';
import { NgChartsModule } from 'ng2-charts';


import { AppRoutingModule } from './app-routing.module';
import { AppErrorHandler } from './app-error.handler';
import { AppTranslationService, TranslateLanguageLoader } from './services/app-translation.service';
import { ConfigurationService } from './services/configuration.service';
import { AlertService } from './services/alert.service';
import { LocalStoreManager } from './services/local-store-manager.service';
import { OidcHelperService, OidcTempStorage } from './services/oidc-helper.service';

import { EqualValidator } from './directives/equal-validator.directive';
import { AutofocusDirective } from './directives/autofocus.directive';
import { BootstrapTabDirective } from './directives/bootstrap-tab.directive';
import { SpinnerDirective } from './directives/spinner.directive';


//
// Custom pipes
//
import { GroupByPipe } from './pipes/group-by.pipe';
import { FilterAndJoinPipe } from './pipes/filter-and-join.pipe';
import { BigNumberFormatPipe } from './pipes/big-number-format.pipe';
import { ContrastColorPipe } from './pipes/contrast-color.pipe';


//
// Custom controls
//
import { BooleanIconComponent } from './components/controls/boolean-icon.component';


//
// Admin components
//
import { AppComponent } from './app.component';
import { LoginComponent } from './components/login/login.component';
import { AuthCallbackComponent } from './components/auth-callback/auth-callback.component';
import { NotFoundComponent } from './components/not-found/not-found.component';


import { SearchBoxComponent } from './components/controls/search-box.component';

import { HeaderComponent } from './components/header/header.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { ResetPasswordComponent } from './components/reset-password/reset-password.component';
import { NewUserComponent } from './components/new-user/new-user.component';

import { SystemHealthComponent } from './components/system-health/system-health.component';
import { SystemHealthService } from './services/system-health.service';
import { TestHarnessComponent } from './components/test-harness/test-harness.component';
import { AlertTestHarnessService } from './services/alert-test-harness.service';
import { IntegrationManagementComponent } from './components/integration-management/integration-management.component';
import { ServiceManagementComponent } from './components/service-management/service-management.component';
import { EscalationPolicyManagementComponent } from './components/escalation-policy-management/escalation-policy-management.component';
import { EscalationPolicyEditorComponent } from './components/escalation-policy-editor/escalation-policy-editor.component';
import { ScheduleManagementComponent } from './components/schedule-management/schedule-management.component';
import { ScheduleEditorComponent } from './components/schedule-editor/schedule-editor.component';

//
// Custom Components
//



//
// Custom services
//

//
// For optimizing what happens when you go back with the back button, to retain state.
//
import { RouteReuseService } from './utility-services/route-reuse.service';

//
// Standard way to get current and previous routes
//
import { NavigationService } from './utility-services/navigation.service';

//
// Custom confirmation dialog
//
import { ConfirmationService } from './services/confirmation-service';

//
// Data support services
//
import { CurrentUserService } from './services/current-user.service';
import { CacheManagerService } from './services/cache-manager.service';
import { TenantHelperService } from './services/tenant-helper.service';



//
// Beginning of imports for Alerting Data Services
//
import { AlertingDataServiceManagerService } from './alerting-data-services/alerting-data-service-manager.service';
import { EscalationPolicyService } from './alerting-data-services/escalation-policy.service';
import { EscalationPolicyChangeHistoryService } from './alerting-data-services/escalation-policy-change-history.service';
import { EscalationRuleService } from './alerting-data-services/escalation-rule.service';
import { EscalationRuleChangeHistoryService } from './alerting-data-services/escalation-rule-change-history.service';
import { IncidentService } from './alerting-data-services/incident.service';
import { IncidentChangeHistoryService } from './alerting-data-services/incident-change-history.service';
import { IncidentEventTypeService } from './alerting-data-services/incident-event-type.service';
import { IncidentNoteService } from './alerting-data-services/incident-note.service';
import { IncidentNoteChangeHistoryService } from './alerting-data-services/incident-note-change-history.service';
import { IncidentNotificationService } from './alerting-data-services/incident-notification.service';
import { IncidentStatusTypeService } from './alerting-data-services/incident-status-type.service';
import { IncidentTimelineEventService } from './alerting-data-services/incident-timeline-event.service';
import { IntegrationService } from './alerting-data-services/integration.service';
import { IntegrationCallbackIncidentEventTypeService } from './alerting-data-services/integration-callback-incident-event-type.service';
import { IntegrationCallbackIncidentEventTypeChangeHistoryService } from './alerting-data-services/integration-callback-incident-event-type-change-history.service';
import { IntegrationChangeHistoryService } from './alerting-data-services/integration-change-history.service';
import { NotificationChannelTypeService } from './alerting-data-services/notification-channel-type.service';
import { NotificationDeliveryAttemptService } from './alerting-data-services/notification-delivery-attempt.service';
import { OnCallScheduleService } from './alerting-data-services/on-call-schedule.service';
import { OnCallScheduleChangeHistoryService } from './alerting-data-services/on-call-schedule-change-history.service';
import { ScheduleLayerService } from './alerting-data-services/schedule-layer.service';
import { ScheduleLayerChangeHistoryService } from './alerting-data-services/schedule-layer-change-history.service';
import { ScheduleLayerMemberService } from './alerting-data-services/schedule-layer-member.service';
import { ScheduleLayerMemberChangeHistoryService } from './alerting-data-services/schedule-layer-member-change-history.service';
import { ScheduleOverrideService } from './alerting-data-services/schedule-override.service';
import { ScheduleOverrideChangeHistoryService } from './alerting-data-services/schedule-override-change-history.service';
import { ScheduleOverrideTypeService } from './alerting-data-services/schedule-override-type.service';
import { ServiceService } from './alerting-data-services/service.service';
import { ServiceChangeHistoryService } from './alerting-data-services/service-change-history.service';
import { SeverityTypeService } from './alerting-data-services/severity-type.service';
import { UserNotificationChannelPreferenceService } from './alerting-data-services/user-notification-channel-preference.service';
import { UserNotificationChannelPreferenceChangeHistoryService } from './alerting-data-services/user-notification-channel-preference-change-history.service';
import { UserNotificationPreferenceService } from './alerting-data-services/user-notification-preference.service';
import { UserNotificationPreferenceChangeHistoryService } from './alerting-data-services/user-notification-preference-change-history.service';
import { WebhookDeliveryAttemptService } from './alerting-data-services/webhook-delivery-attempt.service';
//
// End of imports for Alerting Data Services
//



//
// Beginning of imports for Alerting Data Components
//
import { EscalationPolicyListingComponent } from './alerting-data-components/escalation-policy/escalation-policy-listing/escalation-policy-listing.component';
import { EscalationPolicyAddEditComponent } from './alerting-data-components/escalation-policy/escalation-policy-add-edit/escalation-policy-add-edit.component';
import { EscalationPolicyDetailComponent } from './alerting-data-components/escalation-policy/escalation-policy-detail/escalation-policy-detail.component';
import { EscalationPolicyTableComponent } from './alerting-data-components/escalation-policy/escalation-policy-table/escalation-policy-table.component';
import { EscalationPolicyChangeHistoryListingComponent } from './alerting-data-components/escalation-policy-change-history/escalation-policy-change-history-listing/escalation-policy-change-history-listing.component';
import { EscalationPolicyChangeHistoryAddEditComponent } from './alerting-data-components/escalation-policy-change-history/escalation-policy-change-history-add-edit/escalation-policy-change-history-add-edit.component';
import { EscalationPolicyChangeHistoryDetailComponent } from './alerting-data-components/escalation-policy-change-history/escalation-policy-change-history-detail/escalation-policy-change-history-detail.component';
import { EscalationPolicyChangeHistoryTableComponent } from './alerting-data-components/escalation-policy-change-history/escalation-policy-change-history-table/escalation-policy-change-history-table.component';
import { EscalationRuleListingComponent } from './alerting-data-components/escalation-rule/escalation-rule-listing/escalation-rule-listing.component';
import { EscalationRuleAddEditComponent } from './alerting-data-components/escalation-rule/escalation-rule-add-edit/escalation-rule-add-edit.component';
import { EscalationRuleDetailComponent } from './alerting-data-components/escalation-rule/escalation-rule-detail/escalation-rule-detail.component';
import { EscalationRuleTableComponent } from './alerting-data-components/escalation-rule/escalation-rule-table/escalation-rule-table.component';
import { EscalationRuleChangeHistoryListingComponent } from './alerting-data-components/escalation-rule-change-history/escalation-rule-change-history-listing/escalation-rule-change-history-listing.component';
import { EscalationRuleChangeHistoryAddEditComponent } from './alerting-data-components/escalation-rule-change-history/escalation-rule-change-history-add-edit/escalation-rule-change-history-add-edit.component';
import { EscalationRuleChangeHistoryDetailComponent } from './alerting-data-components/escalation-rule-change-history/escalation-rule-change-history-detail/escalation-rule-change-history-detail.component';
import { EscalationRuleChangeHistoryTableComponent } from './alerting-data-components/escalation-rule-change-history/escalation-rule-change-history-table/escalation-rule-change-history-table.component';
import { IncidentListingComponent } from './alerting-data-components/incident/incident-listing/incident-listing.component';
import { IncidentAddEditComponent } from './alerting-data-components/incident/incident-add-edit/incident-add-edit.component';
import { IncidentDetailComponent } from './alerting-data-components/incident/incident-detail/incident-detail.component';
import { IncidentTableComponent } from './alerting-data-components/incident/incident-table/incident-table.component';
import { IncidentChangeHistoryListingComponent } from './alerting-data-components/incident-change-history/incident-change-history-listing/incident-change-history-listing.component';
import { IncidentChangeHistoryAddEditComponent } from './alerting-data-components/incident-change-history/incident-change-history-add-edit/incident-change-history-add-edit.component';
import { IncidentChangeHistoryDetailComponent } from './alerting-data-components/incident-change-history/incident-change-history-detail/incident-change-history-detail.component';
import { IncidentChangeHistoryTableComponent } from './alerting-data-components/incident-change-history/incident-change-history-table/incident-change-history-table.component';
import { IncidentEventTypeListingComponent } from './alerting-data-components/incident-event-type/incident-event-type-listing/incident-event-type-listing.component';
import { IncidentEventTypeAddEditComponent } from './alerting-data-components/incident-event-type/incident-event-type-add-edit/incident-event-type-add-edit.component';
import { IncidentEventTypeDetailComponent } from './alerting-data-components/incident-event-type/incident-event-type-detail/incident-event-type-detail.component';
import { IncidentEventTypeTableComponent } from './alerting-data-components/incident-event-type/incident-event-type-table/incident-event-type-table.component';
import { IncidentNoteListingComponent } from './alerting-data-components/incident-note/incident-note-listing/incident-note-listing.component';
import { IncidentNoteAddEditComponent } from './alerting-data-components/incident-note/incident-note-add-edit/incident-note-add-edit.component';
import { IncidentNoteDetailComponent } from './alerting-data-components/incident-note/incident-note-detail/incident-note-detail.component';
import { IncidentNoteTableComponent } from './alerting-data-components/incident-note/incident-note-table/incident-note-table.component';
import { IncidentNoteChangeHistoryListingComponent } from './alerting-data-components/incident-note-change-history/incident-note-change-history-listing/incident-note-change-history-listing.component';
import { IncidentNoteChangeHistoryAddEditComponent } from './alerting-data-components/incident-note-change-history/incident-note-change-history-add-edit/incident-note-change-history-add-edit.component';
import { IncidentNoteChangeHistoryDetailComponent } from './alerting-data-components/incident-note-change-history/incident-note-change-history-detail/incident-note-change-history-detail.component';
import { IncidentNoteChangeHistoryTableComponent } from './alerting-data-components/incident-note-change-history/incident-note-change-history-table/incident-note-change-history-table.component';
import { IncidentNotificationListingComponent } from './alerting-data-components/incident-notification/incident-notification-listing/incident-notification-listing.component';
import { IncidentNotificationAddEditComponent } from './alerting-data-components/incident-notification/incident-notification-add-edit/incident-notification-add-edit.component';
import { IncidentNotificationDetailComponent } from './alerting-data-components/incident-notification/incident-notification-detail/incident-notification-detail.component';
import { IncidentNotificationTableComponent } from './alerting-data-components/incident-notification/incident-notification-table/incident-notification-table.component';
import { IncidentStatusTypeListingComponent } from './alerting-data-components/incident-status-type/incident-status-type-listing/incident-status-type-listing.component';
import { IncidentStatusTypeAddEditComponent } from './alerting-data-components/incident-status-type/incident-status-type-add-edit/incident-status-type-add-edit.component';
import { IncidentStatusTypeDetailComponent } from './alerting-data-components/incident-status-type/incident-status-type-detail/incident-status-type-detail.component';
import { IncidentStatusTypeTableComponent } from './alerting-data-components/incident-status-type/incident-status-type-table/incident-status-type-table.component';
import { IncidentTimelineEventListingComponent } from './alerting-data-components/incident-timeline-event/incident-timeline-event-listing/incident-timeline-event-listing.component';
import { IncidentTimelineEventAddEditComponent } from './alerting-data-components/incident-timeline-event/incident-timeline-event-add-edit/incident-timeline-event-add-edit.component';
import { IncidentTimelineEventDetailComponent } from './alerting-data-components/incident-timeline-event/incident-timeline-event-detail/incident-timeline-event-detail.component';
import { IncidentTimelineEventTableComponent } from './alerting-data-components/incident-timeline-event/incident-timeline-event-table/incident-timeline-event-table.component';
import { IntegrationListingComponent } from './alerting-data-components/integration/integration-listing/integration-listing.component';
import { IntegrationAddEditComponent } from './alerting-data-components/integration/integration-add-edit/integration-add-edit.component';
import { IntegrationDetailComponent } from './alerting-data-components/integration/integration-detail/integration-detail.component';
import { IntegrationTableComponent } from './alerting-data-components/integration/integration-table/integration-table.component';
import { IntegrationCallbackIncidentEventTypeListingComponent } from './alerting-data-components/integration-callback-incident-event-type/integration-callback-incident-event-type-listing/integration-callback-incident-event-type-listing.component';
import { IntegrationCallbackIncidentEventTypeAddEditComponent } from './alerting-data-components/integration-callback-incident-event-type/integration-callback-incident-event-type-add-edit/integration-callback-incident-event-type-add-edit.component';
import { IntegrationCallbackIncidentEventTypeDetailComponent } from './alerting-data-components/integration-callback-incident-event-type/integration-callback-incident-event-type-detail/integration-callback-incident-event-type-detail.component';
import { IntegrationCallbackIncidentEventTypeTableComponent } from './alerting-data-components/integration-callback-incident-event-type/integration-callback-incident-event-type-table/integration-callback-incident-event-type-table.component';
import { IntegrationCallbackIncidentEventTypeChangeHistoryListingComponent } from './alerting-data-components/integration-callback-incident-event-type-change-history/integration-callback-incident-event-type-change-history-listing/integration-callback-incident-event-type-change-history-listing.component';
import { IntegrationCallbackIncidentEventTypeChangeHistoryAddEditComponent } from './alerting-data-components/integration-callback-incident-event-type-change-history/integration-callback-incident-event-type-change-history-add-edit/integration-callback-incident-event-type-change-history-add-edit.component';
import { IntegrationCallbackIncidentEventTypeChangeHistoryDetailComponent } from './alerting-data-components/integration-callback-incident-event-type-change-history/integration-callback-incident-event-type-change-history-detail/integration-callback-incident-event-type-change-history-detail.component';
import { IntegrationCallbackIncidentEventTypeChangeHistoryTableComponent } from './alerting-data-components/integration-callback-incident-event-type-change-history/integration-callback-incident-event-type-change-history-table/integration-callback-incident-event-type-change-history-table.component';
import { IntegrationChangeHistoryListingComponent } from './alerting-data-components/integration-change-history/integration-change-history-listing/integration-change-history-listing.component';
import { IntegrationChangeHistoryAddEditComponent } from './alerting-data-components/integration-change-history/integration-change-history-add-edit/integration-change-history-add-edit.component';
import { IntegrationChangeHistoryDetailComponent } from './alerting-data-components/integration-change-history/integration-change-history-detail/integration-change-history-detail.component';
import { IntegrationChangeHistoryTableComponent } from './alerting-data-components/integration-change-history/integration-change-history-table/integration-change-history-table.component';
import { NotificationChannelTypeListingComponent } from './alerting-data-components/notification-channel-type/notification-channel-type-listing/notification-channel-type-listing.component';
import { NotificationChannelTypeAddEditComponent } from './alerting-data-components/notification-channel-type/notification-channel-type-add-edit/notification-channel-type-add-edit.component';
import { NotificationChannelTypeDetailComponent } from './alerting-data-components/notification-channel-type/notification-channel-type-detail/notification-channel-type-detail.component';
import { NotificationChannelTypeTableComponent } from './alerting-data-components/notification-channel-type/notification-channel-type-table/notification-channel-type-table.component';
import { NotificationDeliveryAttemptListingComponent } from './alerting-data-components/notification-delivery-attempt/notification-delivery-attempt-listing/notification-delivery-attempt-listing.component';
import { NotificationDeliveryAttemptAddEditComponent } from './alerting-data-components/notification-delivery-attempt/notification-delivery-attempt-add-edit/notification-delivery-attempt-add-edit.component';
import { NotificationDeliveryAttemptDetailComponent } from './alerting-data-components/notification-delivery-attempt/notification-delivery-attempt-detail/notification-delivery-attempt-detail.component';
import { NotificationDeliveryAttemptTableComponent } from './alerting-data-components/notification-delivery-attempt/notification-delivery-attempt-table/notification-delivery-attempt-table.component';
import { OnCallScheduleListingComponent } from './alerting-data-components/on-call-schedule/on-call-schedule-listing/on-call-schedule-listing.component';
import { OnCallScheduleAddEditComponent } from './alerting-data-components/on-call-schedule/on-call-schedule-add-edit/on-call-schedule-add-edit.component';
import { OnCallScheduleDetailComponent } from './alerting-data-components/on-call-schedule/on-call-schedule-detail/on-call-schedule-detail.component';
import { OnCallScheduleTableComponent } from './alerting-data-components/on-call-schedule/on-call-schedule-table/on-call-schedule-table.component';
import { OnCallScheduleChangeHistoryListingComponent } from './alerting-data-components/on-call-schedule-change-history/on-call-schedule-change-history-listing/on-call-schedule-change-history-listing.component';
import { OnCallScheduleChangeHistoryAddEditComponent } from './alerting-data-components/on-call-schedule-change-history/on-call-schedule-change-history-add-edit/on-call-schedule-change-history-add-edit.component';
import { OnCallScheduleChangeHistoryDetailComponent } from './alerting-data-components/on-call-schedule-change-history/on-call-schedule-change-history-detail/on-call-schedule-change-history-detail.component';
import { OnCallScheduleChangeHistoryTableComponent } from './alerting-data-components/on-call-schedule-change-history/on-call-schedule-change-history-table/on-call-schedule-change-history-table.component';
import { ScheduleLayerListingComponent } from './alerting-data-components/schedule-layer/schedule-layer-listing/schedule-layer-listing.component';
import { ScheduleLayerAddEditComponent } from './alerting-data-components/schedule-layer/schedule-layer-add-edit/schedule-layer-add-edit.component';
import { ScheduleLayerDetailComponent } from './alerting-data-components/schedule-layer/schedule-layer-detail/schedule-layer-detail.component';
import { ScheduleLayerTableComponent } from './alerting-data-components/schedule-layer/schedule-layer-table/schedule-layer-table.component';
import { ScheduleLayerChangeHistoryListingComponent } from './alerting-data-components/schedule-layer-change-history/schedule-layer-change-history-listing/schedule-layer-change-history-listing.component';
import { ScheduleLayerChangeHistoryAddEditComponent } from './alerting-data-components/schedule-layer-change-history/schedule-layer-change-history-add-edit/schedule-layer-change-history-add-edit.component';
import { ScheduleLayerChangeHistoryDetailComponent } from './alerting-data-components/schedule-layer-change-history/schedule-layer-change-history-detail/schedule-layer-change-history-detail.component';
import { ScheduleLayerChangeHistoryTableComponent } from './alerting-data-components/schedule-layer-change-history/schedule-layer-change-history-table/schedule-layer-change-history-table.component';
import { ScheduleLayerMemberListingComponent } from './alerting-data-components/schedule-layer-member/schedule-layer-member-listing/schedule-layer-member-listing.component';
import { ScheduleLayerMemberAddEditComponent } from './alerting-data-components/schedule-layer-member/schedule-layer-member-add-edit/schedule-layer-member-add-edit.component';
import { ScheduleLayerMemberDetailComponent } from './alerting-data-components/schedule-layer-member/schedule-layer-member-detail/schedule-layer-member-detail.component';
import { ScheduleLayerMemberTableComponent } from './alerting-data-components/schedule-layer-member/schedule-layer-member-table/schedule-layer-member-table.component';
import { ScheduleLayerMemberChangeHistoryListingComponent } from './alerting-data-components/schedule-layer-member-change-history/schedule-layer-member-change-history-listing/schedule-layer-member-change-history-listing.component';
import { ScheduleLayerMemberChangeHistoryAddEditComponent } from './alerting-data-components/schedule-layer-member-change-history/schedule-layer-member-change-history-add-edit/schedule-layer-member-change-history-add-edit.component';
import { ScheduleLayerMemberChangeHistoryDetailComponent } from './alerting-data-components/schedule-layer-member-change-history/schedule-layer-member-change-history-detail/schedule-layer-member-change-history-detail.component';
import { ScheduleLayerMemberChangeHistoryTableComponent } from './alerting-data-components/schedule-layer-member-change-history/schedule-layer-member-change-history-table/schedule-layer-member-change-history-table.component';
import { ScheduleOverrideListingComponent } from './alerting-data-components/schedule-override/schedule-override-listing/schedule-override-listing.component';
import { ScheduleOverrideAddEditComponent } from './alerting-data-components/schedule-override/schedule-override-add-edit/schedule-override-add-edit.component';
import { ScheduleOverrideDetailComponent } from './alerting-data-components/schedule-override/schedule-override-detail/schedule-override-detail.component';
import { ScheduleOverrideTableComponent } from './alerting-data-components/schedule-override/schedule-override-table/schedule-override-table.component';
import { ScheduleOverrideChangeHistoryListingComponent } from './alerting-data-components/schedule-override-change-history/schedule-override-change-history-listing/schedule-override-change-history-listing.component';
import { ScheduleOverrideChangeHistoryAddEditComponent } from './alerting-data-components/schedule-override-change-history/schedule-override-change-history-add-edit/schedule-override-change-history-add-edit.component';
import { ScheduleOverrideChangeHistoryDetailComponent } from './alerting-data-components/schedule-override-change-history/schedule-override-change-history-detail/schedule-override-change-history-detail.component';
import { ScheduleOverrideChangeHistoryTableComponent } from './alerting-data-components/schedule-override-change-history/schedule-override-change-history-table/schedule-override-change-history-table.component';
import { ScheduleOverrideTypeListingComponent } from './alerting-data-components/schedule-override-type/schedule-override-type-listing/schedule-override-type-listing.component';
import { ScheduleOverrideTypeAddEditComponent } from './alerting-data-components/schedule-override-type/schedule-override-type-add-edit/schedule-override-type-add-edit.component';
import { ScheduleOverrideTypeDetailComponent } from './alerting-data-components/schedule-override-type/schedule-override-type-detail/schedule-override-type-detail.component';
import { ScheduleOverrideTypeTableComponent } from './alerting-data-components/schedule-override-type/schedule-override-type-table/schedule-override-type-table.component';
import { ServiceListingComponent } from './alerting-data-components/service/service-listing/service-listing.component';
import { ServiceAddEditComponent } from './alerting-data-components/service/service-add-edit/service-add-edit.component';
import { ServiceDetailComponent } from './alerting-data-components/service/service-detail/service-detail.component';
import { ServiceTableComponent } from './alerting-data-components/service/service-table/service-table.component';
import { ServiceChangeHistoryListingComponent } from './alerting-data-components/service-change-history/service-change-history-listing/service-change-history-listing.component';
import { ServiceChangeHistoryAddEditComponent } from './alerting-data-components/service-change-history/service-change-history-add-edit/service-change-history-add-edit.component';
import { ServiceChangeHistoryDetailComponent } from './alerting-data-components/service-change-history/service-change-history-detail/service-change-history-detail.component';
import { ServiceChangeHistoryTableComponent } from './alerting-data-components/service-change-history/service-change-history-table/service-change-history-table.component';
import { SeverityTypeListingComponent } from './alerting-data-components/severity-type/severity-type-listing/severity-type-listing.component';
import { SeverityTypeAddEditComponent } from './alerting-data-components/severity-type/severity-type-add-edit/severity-type-add-edit.component';
import { SeverityTypeDetailComponent } from './alerting-data-components/severity-type/severity-type-detail/severity-type-detail.component';
import { SeverityTypeTableComponent } from './alerting-data-components/severity-type/severity-type-table/severity-type-table.component';
import { UserNotificationChannelPreferenceListingComponent } from './alerting-data-components/user-notification-channel-preference/user-notification-channel-preference-listing/user-notification-channel-preference-listing.component';
import { UserNotificationChannelPreferenceAddEditComponent } from './alerting-data-components/user-notification-channel-preference/user-notification-channel-preference-add-edit/user-notification-channel-preference-add-edit.component';
import { UserNotificationChannelPreferenceDetailComponent } from './alerting-data-components/user-notification-channel-preference/user-notification-channel-preference-detail/user-notification-channel-preference-detail.component';
import { UserNotificationChannelPreferenceTableComponent } from './alerting-data-components/user-notification-channel-preference/user-notification-channel-preference-table/user-notification-channel-preference-table.component';
import { UserNotificationChannelPreferenceChangeHistoryListingComponent } from './alerting-data-components/user-notification-channel-preference-change-history/user-notification-channel-preference-change-history-listing/user-notification-channel-preference-change-history-listing.component';
import { UserNotificationChannelPreferenceChangeHistoryAddEditComponent } from './alerting-data-components/user-notification-channel-preference-change-history/user-notification-channel-preference-change-history-add-edit/user-notification-channel-preference-change-history-add-edit.component';
import { UserNotificationChannelPreferenceChangeHistoryDetailComponent } from './alerting-data-components/user-notification-channel-preference-change-history/user-notification-channel-preference-change-history-detail/user-notification-channel-preference-change-history-detail.component';
import { UserNotificationChannelPreferenceChangeHistoryTableComponent } from './alerting-data-components/user-notification-channel-preference-change-history/user-notification-channel-preference-change-history-table/user-notification-channel-preference-change-history-table.component';
import { UserNotificationPreferenceListingComponent } from './alerting-data-components/user-notification-preference/user-notification-preference-listing/user-notification-preference-listing.component';
import { UserNotificationPreferenceAddEditComponent } from './alerting-data-components/user-notification-preference/user-notification-preference-add-edit/user-notification-preference-add-edit.component';
import { UserNotificationPreferenceDetailComponent } from './alerting-data-components/user-notification-preference/user-notification-preference-detail/user-notification-preference-detail.component';
import { UserNotificationPreferenceTableComponent } from './alerting-data-components/user-notification-preference/user-notification-preference-table/user-notification-preference-table.component';
import { UserNotificationPreferenceChangeHistoryListingComponent } from './alerting-data-components/user-notification-preference-change-history/user-notification-preference-change-history-listing/user-notification-preference-change-history-listing.component';
import { UserNotificationPreferenceChangeHistoryAddEditComponent } from './alerting-data-components/user-notification-preference-change-history/user-notification-preference-change-history-add-edit/user-notification-preference-change-history-add-edit.component';
import { UserNotificationPreferenceChangeHistoryDetailComponent } from './alerting-data-components/user-notification-preference-change-history/user-notification-preference-change-history-detail/user-notification-preference-change-history-detail.component';
import { UserNotificationPreferenceChangeHistoryTableComponent } from './alerting-data-components/user-notification-preference-change-history/user-notification-preference-change-history-table/user-notification-preference-change-history-table.component';
import { WebhookDeliveryAttemptListingComponent } from './alerting-data-components/webhook-delivery-attempt/webhook-delivery-attempt-listing/webhook-delivery-attempt-listing.component';
import { WebhookDeliveryAttemptAddEditComponent } from './alerting-data-components/webhook-delivery-attempt/webhook-delivery-attempt-add-edit/webhook-delivery-attempt-add-edit.component';
import { WebhookDeliveryAttemptDetailComponent } from './alerting-data-components/webhook-delivery-attempt/webhook-delivery-attempt-detail/webhook-delivery-attempt-detail.component';
import { WebhookDeliveryAttemptTableComponent } from './alerting-data-components/webhook-delivery-attempt/webhook-delivery-attempt-table/webhook-delivery-attempt-table.component';
//
// End of imports for Alerting Data Components
//

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    AuthCallbackComponent,
    NotFoundComponent,
    SearchBoxComponent,
    BooleanIconComponent,

    EqualValidator,
    AutofocusDirective,
    BootstrapTabDirective,
    SpinnerDirective,

    GroupByPipe,
    FilterAndJoinPipe,
    BigNumberFormatPipe,
    ContrastColorPipe,


    HeaderComponent,
    SidebarComponent,

    SystemHealthComponent,
    TestHarnessComponent,
    IntegrationManagementComponent,
    ServiceManagementComponent,
    EscalationPolicyManagementComponent,
    EscalationPolicyEditorComponent,
    ScheduleManagementComponent,
    ScheduleEditorComponent,


    //
    // Custom components
    //





    //
    // Beginning of declarations for Alerting Data Components
//
EscalationPolicyListingComponent,
EscalationPolicyAddEditComponent,
EscalationPolicyDetailComponent,
EscalationPolicyTableComponent,
EscalationPolicyChangeHistoryListingComponent,
EscalationPolicyChangeHistoryAddEditComponent,
EscalationPolicyChangeHistoryDetailComponent,
EscalationPolicyChangeHistoryTableComponent,
EscalationRuleListingComponent,
EscalationRuleAddEditComponent,
EscalationRuleDetailComponent,
EscalationRuleTableComponent,
EscalationRuleChangeHistoryListingComponent,
EscalationRuleChangeHistoryAddEditComponent,
EscalationRuleChangeHistoryDetailComponent,
EscalationRuleChangeHistoryTableComponent,
IncidentListingComponent,
IncidentAddEditComponent,
IncidentDetailComponent,
IncidentTableComponent,
IncidentChangeHistoryListingComponent,
IncidentChangeHistoryAddEditComponent,
IncidentChangeHistoryDetailComponent,
IncidentChangeHistoryTableComponent,
IncidentEventTypeListingComponent,
IncidentEventTypeAddEditComponent,
IncidentEventTypeDetailComponent,
IncidentEventTypeTableComponent,
IncidentNoteListingComponent,
IncidentNoteAddEditComponent,
IncidentNoteDetailComponent,
IncidentNoteTableComponent,
IncidentNoteChangeHistoryListingComponent,
IncidentNoteChangeHistoryAddEditComponent,
IncidentNoteChangeHistoryDetailComponent,
IncidentNoteChangeHistoryTableComponent,
IncidentNotificationListingComponent,
IncidentNotificationAddEditComponent,
IncidentNotificationDetailComponent,
IncidentNotificationTableComponent,
IncidentStatusTypeListingComponent,
IncidentStatusTypeAddEditComponent,
IncidentStatusTypeDetailComponent,
IncidentStatusTypeTableComponent,
IncidentTimelineEventListingComponent,
IncidentTimelineEventAddEditComponent,
IncidentTimelineEventDetailComponent,
IncidentTimelineEventTableComponent,
IntegrationListingComponent,
IntegrationAddEditComponent,
IntegrationDetailComponent,
IntegrationTableComponent,
IntegrationCallbackIncidentEventTypeListingComponent,
IntegrationCallbackIncidentEventTypeAddEditComponent,
IntegrationCallbackIncidentEventTypeDetailComponent,
IntegrationCallbackIncidentEventTypeTableComponent,
IntegrationCallbackIncidentEventTypeChangeHistoryListingComponent,
IntegrationCallbackIncidentEventTypeChangeHistoryAddEditComponent,
IntegrationCallbackIncidentEventTypeChangeHistoryDetailComponent,
IntegrationCallbackIncidentEventTypeChangeHistoryTableComponent,
IntegrationChangeHistoryListingComponent,
IntegrationChangeHistoryAddEditComponent,
IntegrationChangeHistoryDetailComponent,
IntegrationChangeHistoryTableComponent,
NotificationChannelTypeListingComponent,
NotificationChannelTypeAddEditComponent,
NotificationChannelTypeDetailComponent,
NotificationChannelTypeTableComponent,
NotificationDeliveryAttemptListingComponent,
NotificationDeliveryAttemptAddEditComponent,
NotificationDeliveryAttemptDetailComponent,
NotificationDeliveryAttemptTableComponent,
OnCallScheduleListingComponent,
OnCallScheduleAddEditComponent,
OnCallScheduleDetailComponent,
OnCallScheduleTableComponent,
OnCallScheduleChangeHistoryListingComponent,
OnCallScheduleChangeHistoryAddEditComponent,
OnCallScheduleChangeHistoryDetailComponent,
OnCallScheduleChangeHistoryTableComponent,
ScheduleLayerListingComponent,
ScheduleLayerAddEditComponent,
ScheduleLayerDetailComponent,
ScheduleLayerTableComponent,
ScheduleLayerChangeHistoryListingComponent,
ScheduleLayerChangeHistoryAddEditComponent,
ScheduleLayerChangeHistoryDetailComponent,
ScheduleLayerChangeHistoryTableComponent,
ScheduleLayerMemberListingComponent,
ScheduleLayerMemberAddEditComponent,
ScheduleLayerMemberDetailComponent,
ScheduleLayerMemberTableComponent,
ScheduleLayerMemberChangeHistoryListingComponent,
ScheduleLayerMemberChangeHistoryAddEditComponent,
ScheduleLayerMemberChangeHistoryDetailComponent,
ScheduleLayerMemberChangeHistoryTableComponent,
ScheduleOverrideListingComponent,
ScheduleOverrideAddEditComponent,
ScheduleOverrideDetailComponent,
ScheduleOverrideTableComponent,
ScheduleOverrideChangeHistoryListingComponent,
ScheduleOverrideChangeHistoryAddEditComponent,
ScheduleOverrideChangeHistoryDetailComponent,
ScheduleOverrideChangeHistoryTableComponent,
ScheduleOverrideTypeListingComponent,
ScheduleOverrideTypeAddEditComponent,
ScheduleOverrideTypeDetailComponent,
ScheduleOverrideTypeTableComponent,
ServiceListingComponent,
ServiceAddEditComponent,
ServiceDetailComponent,
ServiceTableComponent,
ServiceChangeHistoryListingComponent,
ServiceChangeHistoryAddEditComponent,
ServiceChangeHistoryDetailComponent,
ServiceChangeHistoryTableComponent,
SeverityTypeListingComponent,
SeverityTypeAddEditComponent,
SeverityTypeDetailComponent,
SeverityTypeTableComponent,
UserNotificationChannelPreferenceListingComponent,
UserNotificationChannelPreferenceAddEditComponent,
UserNotificationChannelPreferenceDetailComponent,
UserNotificationChannelPreferenceTableComponent,
UserNotificationChannelPreferenceChangeHistoryListingComponent,
UserNotificationChannelPreferenceChangeHistoryAddEditComponent,
UserNotificationChannelPreferenceChangeHistoryDetailComponent,
UserNotificationChannelPreferenceChangeHistoryTableComponent,
UserNotificationPreferenceListingComponent,
UserNotificationPreferenceAddEditComponent,
UserNotificationPreferenceDetailComponent,
UserNotificationPreferenceTableComponent,
UserNotificationPreferenceChangeHistoryListingComponent,
UserNotificationPreferenceChangeHistoryAddEditComponent,
UserNotificationPreferenceChangeHistoryDetailComponent,
UserNotificationPreferenceChangeHistoryTableComponent,
WebhookDeliveryAttemptListingComponent,
WebhookDeliveryAttemptAddEditComponent,
WebhookDeliveryAttemptDetailComponent,
WebhookDeliveryAttemptTableComponent,
//
    // End of declarations for Alerting Data Components
    //


    //
    // Custom components
    //

    ResetPasswordComponent,
    NewUserComponent,

  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    ScrollingModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    MatExpansionModule,
    DragDropModule,
    AppRoutingModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useClass: TranslateLanguageLoader
      }
    }),
    NgbTooltipModule,
    NgbPopoverModule,
    NgbDropdownModule,
    NgbCarouselModule,
    NgbModalModule,
    NgbNavModule,
    NgxDatatableModule,
    OAuthModule.forRoot(),
    ToastaModule.forRoot(),
    NgSelectModule,
    NgChartsModule,
    NgbAccordionModule,
    FullCalendarModule,
  ],
  exports: [SpinnerDirective],
  providers: [
    //{ provide: ErrorHandler, useClass: AppErrorHandler }, this is a wrapper around Javascript errors that just displays a popup, but makes it hard to debug
    { provide: OAuthStorage, useClass: OidcTempStorage },
    { provide: RouteReuseStrategy, useClass: RouteReuseService },
    AlertService,
    ConfigurationService,
    AppTranslationService,
    LocalStoreManager,
    OidcHelperService,

    //
    // Custom services
    //
    NavigationService,
    CurrentUserService,
    CacheManagerService,
    TenantHelperService,
    ConfirmationService,
    SystemHealthService,
    AlertTestHarnessService,

    //
    // Pipes
    //
    ContrastColorPipe,



    //
    // Beginning of provider declarations for Alerting Data Services
//
AlertingDataServiceManagerService,
EscalationPolicyService,
EscalationPolicyChangeHistoryService,
EscalationRuleService,
EscalationRuleChangeHistoryService,
IncidentService,
IncidentChangeHistoryService,
IncidentEventTypeService,
IncidentNoteService,
IncidentNoteChangeHistoryService,
IncidentNotificationService,
IncidentStatusTypeService,
IncidentTimelineEventService,
IntegrationService,
IntegrationCallbackIncidentEventTypeService,
IntegrationCallbackIncidentEventTypeChangeHistoryService,
IntegrationChangeHistoryService,
NotificationChannelTypeService,
NotificationDeliveryAttemptService,
OnCallScheduleService,
OnCallScheduleChangeHistoryService,
ScheduleLayerService,
ScheduleLayerChangeHistoryService,
ScheduleLayerMemberService,
ScheduleLayerMemberChangeHistoryService,
ScheduleOverrideService,
ScheduleOverrideChangeHistoryService,
ScheduleOverrideTypeService,
ServiceService,
ServiceChangeHistoryService,
SeverityTypeService,
UserNotificationChannelPreferenceService,
UserNotificationChannelPreferenceChangeHistoryService,
UserNotificationPreferenceService,
UserNotificationPreferenceChangeHistoryService,
WebhookDeliveryAttemptService,
//
    // End of provider declarations for Alerting Data Services
    //


    //
    // For animations.
    //
    provideAnimationsAsync()
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
