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
import { ContactFullNamePipe } from './pipes/contact-full-name.pipe';


//
// Custom controls
//
import { BooleanIconComponent } from './components/controls/boolean-icon.component';
import { DynamicFieldRendererComponent } from './components/controls/dynamic-field-renderer/dynamic-field-renderer.component';


//
// Admin components
//
import { AppComponent } from './app.component';
import { LoginComponent } from './components/login/login.component';
import { AuthCallbackComponent } from './components/auth-callback/auth-callback.component';
import { NotFoundComponent } from './components/not-found/not-found.component';


import { SearchBoxComponent } from './components/controls/search-box.component';
import { OverviewComponent } from './components/overview/overview.component';
import { ModalComponent } from './components/modal/modal.component';
import { HeaderComponent } from './components/header/header.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { ResetPasswordComponent } from './components/reset-password/reset-password.component';
import { NewUserComponent } from './components/new-user/new-user.component';


//
// Custom Components
//
import { SchedulerCalendarComponent } from './components/scheduler/scheduler-calendar/scheduler-calendar.component';
import { EventAddEditModalComponent } from './components/scheduler/event-add-edit-modal/event-add-edit-modal.component';
import { AdministrationComponent } from './components/administration/administration.component';
import { AddTenantProfileComponent } from './components/add-tenant-profile/add-tenant-profile.component';


// Rate Sheet Customizations
import { RateSheetCustomListingComponent } from './components/rate-sheet-custom/rate-sheet-custom-listing/rate-sheet-custom-listing.component';
import { RateSheetCustomTableComponent } from './components/rate-sheet-custom/rate-sheet-custom-table/rate-sheet-custom-table.component';
import { RateSheetCustomAddEditComponent } from './components/rate-sheet-custom/rate-sheet-custom-add-edit/rate-sheet-custom-add-edit.component';


//
// Resource custom optimizations
//
import { ResourceCustomListingComponent } from './components/resource-custom/resource-custom-listing/resource-custom-listing.component';
import { ResourceCustomDetailComponent } from './components/resource-custom/resource-custom-detail/resource-custom-detail.component';
import { ResourceCustomAddEditComponent } from './components/resource-custom/resource-custom-add-edit/resource-custom-add-edit.component';
import { ResourceCustomTableComponent } from './components/resource-custom/resource-custom-table/resource-custom-table.component';
import { ResourceOverviewTabComponent } from './components/resource-custom/resource-overview-tab/resource-overview-tab.component';
import { ResourceQualificationsTabComponent } from './components/resource-custom/resource-qualifications-tab/resource-qualifications-tab.component';
import { ResourceQualificationCustomAddModalComponent } from './components/resource-custom/resource-qualification-custom-add-modal/resource-qualification-custom-add-modal.component';
import { ResourceCrewsTabComponent } from './components/resource-custom/resource-crews-tab/resource-crews-tab.component';
import { ResourceAddToCrewModalComponent } from './components/resource-custom/resource-add-to-crew-modal/resource-add-to-crew-modal.component';
import { ResourceAvailabilityTabComponent } from './components/resource-custom/resource-availability-tab/resource-availability-tab.component';
import { ResourceAvailabilityAddModalComponent } from './components/resource-custom/resource-availability-add-modal/resource-availability-add-modal.component';
import { ResourceRatesTabComponent } from './components/resource-custom/resource-rates-tab/resource-rates-tab.component';
import { ResourceRateOverrideAddModalComponent } from './components/resource-custom/resource-rate-sheet-override-add-modal/resource-rate-sheet-override-add-modal.component';
import { ResourceAssignmentsTabComponent } from './components/resource-custom/resource-assignments-tab/resource-assignments-tab.component';
import { ResourceContactsTabComponent } from './components/resource-custom/resource-contacts-tab/resource-contacts-tab.component';
import { ResourceContactCustomAddEditModalComponent } from './components/resource-custom/resource-contact-custom-add-edit-modal/resource-contact-custom-add-edit-modal.component';
import { ResourceNotificationsTabComponent } from './components/resource-custom/resource-notifications-tab/resource-notifications-tab.component';
import { NotificationSubscriptionCustomAddEditModalComponent } from './components/resource-custom/notification-subscription-custom-add-edit-modal/notification-subscription-custom-add-edit-modal.component';

//
// Crew custom optimizations
//
import { CrewCustomListingComponent } from './components/crew-custom/crew-custom-listing/crew-custom-listing.component';
import { CrewCustomDetailComponent } from './components/crew-custom/crew-custom-detail/crew-custom-detail.component';
import { CrewCustomAddEditComponent } from './components/crew-custom/crew-custom-add-edit/crew-custom-add-edit.component';
import { CrewCustomTableComponent } from './components/crew-custom/crew-custom-table/crew-custom-table.component';
import { CrewOverviewTabComponent } from './components/crew-custom/crew-overview-tab/crew-overview-tab.component';
import { CrewAssignmentsTabComponent } from './components/crew-custom/crew-assignments-tab/crew-assignments-tab.component';
import { CrewMembersTabComponent } from './components/crew-custom/crew-members-tab/crew-members-tab.component';
import { CrewAddToCrewModalComponent } from './components/crew-custom/crew-add-to-crew-modal/crew-add-to-crew-modal.component';


//
// Office custom optimizations
//
import { OfficeCustomListingComponent } from './components/office-custom/office-custom-listing/office-custom-listing.component';
import { OfficeCustomDetailComponent } from './components/office-custom/office-custom-detail/office-custom-detail.component';
import { OfficeCustomAddEditComponent } from './components/office-custom/office-custom-add-edit/office-custom-add-edit.component';
import { OfficeCustomTableComponent } from './components/office-custom/office-custom-table/office-custom-table.component';
import { OfficeOverviewTabComponent } from './components/office-custom/office-overview-tab/office-overview-tab.component';
import { OfficeCrewsTabComponent } from './components/office-custom/office-crews-tab/office-crews-tab.component';
import { OfficeContactsTabComponent } from './components/office-custom/office-contacts-tab/office-contacts-tab.component';
import { OfficeContactCustomAddEditModalComponent } from './components/office-custom/office-contact-custom-add-edit-modal/office-contact-custom-add-edit-modal.component';
import { OfficeRatesTabComponent } from './components/office-custom/office-rates-tab/office-rates-tab.component';
import { OfficeRateOverrideAddModalComponent } from './components/office-custom/office-rate-sheet-override-add-modal/office-rate-sheet-override-add-modal.component';
import { OfficeAssignmentsTabComponent } from './components/office-custom/office-assignments-tab/office-assignments-tab.component';
import { OfficeResourcesTabComponent } from './components/office-custom/office-resources-tab/office-resources-tab.component';
import { OfficeCalendarsTabComponent } from './components/office-custom/office-calendars-tab/office-calendars-tab.component';

//
// Calendar custom optimizations
//
import { CalendarCustomAddEditComponent } from './components/calendar-custom/calendar-custom-add-edit/calendar-custom-add-edit.component';
import { CalendarCustomDetailComponent } from './components/calendar-custom/calendar-custom-detail/calendar-custom-detail.component';
import { CalendarCustomListingComponent } from './components/calendar-custom/calendar-custom-listing/calendar-custom-listing.component';
import { CalendarCustomTableComponent } from './components/calendar-custom/calendar-custom-table/calendar-custom-table.component';
import { CalendarOverviewTabComponent } from './components/calendar-custom/calendar-overview-tab/calendar-overview-tab.component';
import { CalendarAssignmentsTabComponent } from './components/calendar-custom/calendar-assignments-tab/calendar-assignments-tab.component';

//
// Contact custom optimizations
//
import { ContactCustomListingComponent } from './components/contact-custom/contact-custom-listing/contact-custom-listing.component';
import { ContactCustomDetailComponent } from './components/contact-custom/contact-custom-detail/contact-custom-detail.component';
import { ContactCustomAddEditComponent } from './components/contact-custom/contact-custom-add-edit/contact-custom-add-edit.component';
import { ConstituentJourneyUpdateModalComponent } from './components/contact-custom/constituent-journey-update-modal/constituent-journey-update-modal.component';
import { ContactCustomTableComponent } from './components/contact-custom/contact-custom-table/contact-custom-table.component';
import { ContactOverviewTabComponent } from './components/contact-custom/contact-overview-tab/contact-overview-tab.component';
import { ContactInteractionsTabComponent } from './components/contact-custom/contact-interactions-tab/contact-interactions-tab.component';
import { ContactInteractionEditModalComponent } from './components/contact-custom/contact-interaction-edit-modal/contact-interaction-edit-modal.component';
import { ContactRelationshipsTabComponent } from './components/contact-custom/contact-relationships-tab/contact-relationships-tab.component';
import { ContactFinancialsTabComponent } from './components/contact-custom/contact-financials-tab/contact-financials-tab.component';
import { ContactScheduleTabComponent } from './components/contact-custom/contact-schedule-tab/contact-schedule-tab.component';


//
// Client custom optimizations
//
import { ClientCustomAddEditComponent } from './components/client-custom/client-custom-add-edit/client-custom-add-edit.component';
import { ClientCustomDetailComponent } from './components/client-custom/client-custom-detail/client-custom-detail.component';
import { ClientCustomListingComponent } from './components/client-custom/client-custom-listing/client-custom-listing.component';
import { ClientCustomTableComponent } from './components/client-custom/client-custom-table/client-custom-table.component';
import { ClientOverviewTabComponent } from './components/client-custom/client-overview-tab/client-overview-tab.component';
import { ClientAssignmentsTabComponent } from './components/client-custom/client-assignments-tab/client-assignments-tab.component';
import { ClientContactsTabComponent } from './components/client-custom/client-contacts-tab/client-contacts-tab.component';
import { ClientContactCustomAddEditModalComponent } from './components/client-custom/client-contact-custom-add-edit-modal/client-contact-custom-add-edit-modal.component';
import { ClientTargetsTabComponent } from './components/client-custom/client-targets-tab/client-targets-tab.component';

//
// Custom services
//
import { CrewWithMembersService } from './services/crew-with-members.service';
import { AssignmentService } from './services/assignment.service';
import { SchedulerHelperService } from './services/scheduler-helper.service';

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
// Security Data services - Auto Generated
//
import { SecurityDataServiceManagerService } from './security-data-services/security-data-service-manager.service';
import { EntityDataTokenService } from './security-data-services/entity-data-token.service';
import { EntityDataTokenEventService } from './security-data-services/entity-data-token-event.service';
import { EntityDataTokenEventTypeService } from './security-data-services/entity-data-token-event-type.service';
import { LoginAttemptService } from './security-data-services/login-attempt.service';
import { ModuleService } from './security-data-services/module.service';
import { ModuleSecurityRoleService } from './security-data-services/module-security-role.service';
import { OAUTHTokenService } from './security-data-services/o-a-u-t-h-token.service';
import { PrivilegeService } from './security-data-services/privilege.service';
import { SecurityDepartmentService } from './security-data-services/security-department.service';
import { SecurityDepartmentUserService } from './security-data-services/security-department-user.service';
import { SecurityGroupService } from './security-data-services/security-group.service';
import { SecurityGroupSecurityRoleService } from './security-data-services/security-group-security-role.service';
import { SecurityOrganizationService } from './security-data-services/security-organization.service';
import { SecurityOrganizationUserService } from './security-data-services/security-organization-user.service';
import { SecurityRoleService } from './security-data-services/security-role.service';
import { SecurityTeamService } from './security-data-services/security-team.service';
import { SecurityTeamUserService } from './security-data-services/security-team-user.service';
import { SecurityTenantService } from './security-data-services/security-tenant.service';
import { SecurityTenantUserService } from './security-data-services/security-tenant-user.service';
import { SecurityUserService } from './security-data-services/security-user.service';
import { SecurityUserEventService } from './security-data-services/security-user-event.service';
import { SecurityUserEventTypeService } from './security-data-services/security-user-event-type.service';
import { SecurityUserPasswordResetTokenService } from './security-data-services/security-user-password-reset-token.service';
import { SecurityUserSecurityGroupService } from './security-data-services/security-user-security-group.service';
import { SecurityUserSecurityRoleService } from './security-data-services/security-user-security-role.service';
import { SecurityUserTitleService } from './security-data-services/security-user-title.service';
import { SystemSettingService } from './security-data-services/system-setting.service';
//
// End Security Data Services
//

//
// Auditor Data Services - Auto Generated
//
import { AuditorDataServiceManagerService } from './auditor-data-services/auditor-data-service-manager.service';
import { AuditAccessTypeService } from './auditor-data-services/audit-access-type.service';
import { AuditEventService } from './auditor-data-services/audit-event.service';
import { AuditEventEntityStateService } from './auditor-data-services/audit-event-entity-state.service';
import { AuditEventErrorMessageService } from './auditor-data-services/audit-event-error-message.service';
import { AuditHostSystemService } from './auditor-data-services/audit-host-system.service';
import { AuditModuleService } from './auditor-data-services/audit-module.service';
import { AuditModuleEntityService } from './auditor-data-services/audit-module-entity.service';
import { AuditPlanBService } from './auditor-data-services/audit-plan-b.service';
import { AuditResourceService } from './auditor-data-services/audit-resource.service';
import { AuditSessionService } from './auditor-data-services/audit-session.service';
import { AuditSourceService } from './auditor-data-services/audit-source.service';
import { AuditTypeService } from './auditor-data-services/audit-type.service';
import { AuditUserService } from './auditor-data-services/audit-user.service';
import { AuditUserAgentService } from './auditor-data-services/audit-user-agent.service';
import { ExternalCommunicationService } from './auditor-data-services/external-communication.service';
import { ExternalCommunicationRecipientService } from './auditor-data-services/external-communication-recipient.service'
//
// End Auditor Data Services
//

//
// Beginning of imports for Scheduler Data Services 
//
import { SchedulerDataServiceManagerService } from './scheduler-data-services/scheduler-data-service-manager.service';
import { AppealService } from './scheduler-data-services/appeal.service';
import { AppealChangeHistoryService } from './scheduler-data-services/appeal-change-history.service';
import { AssignmentRoleService } from './scheduler-data-services/assignment-role.service';
import { AssignmentRoleQualificationRequirementService } from './scheduler-data-services/assignment-role-qualification-requirement.service';
import { AssignmentRoleQualificationRequirementChangeHistoryService } from './scheduler-data-services/assignment-role-qualification-requirement-change-history.service';
import { AssignmentStatusService } from './scheduler-data-services/assignment-status.service';
import { AttributeDefinitionService } from './scheduler-data-services/attribute-definition.service';
import { BatchService } from './scheduler-data-services/batch.service';
import { BatchChangeHistoryService } from './scheduler-data-services/batch-change-history.service';
import { BatchStatusService } from './scheduler-data-services/batch-status.service';
import { BookingSourceTypeService } from './scheduler-data-services/booking-source-type.service';
import { CalendarService } from './scheduler-data-services/calendar.service';
import { CalendarChangeHistoryService } from './scheduler-data-services/calendar-change-history.service';
import { CampaignService } from './scheduler-data-services/campaign.service';
import { CampaignChangeHistoryService } from './scheduler-data-services/campaign-change-history.service';
import { ChargeStatusService } from './scheduler-data-services/charge-status.service';
import { ChargeTypeService } from './scheduler-data-services/charge-type.service';
import { ChargeTypeChangeHistoryService } from './scheduler-data-services/charge-type-change-history.service';
import { ClientService } from './scheduler-data-services/client.service';
import { ClientChangeHistoryService } from './scheduler-data-services/client-change-history.service';
import { ClientContactService } from './scheduler-data-services/client-contact.service';
import { ClientContactChangeHistoryService } from './scheduler-data-services/client-contact-change-history.service';
import { ClientTypeService } from './scheduler-data-services/client-type.service';
import { ConstituentService } from './scheduler-data-services/constituent.service';
import { ConstituentChangeHistoryService } from './scheduler-data-services/constituent-change-history.service';
import { ConstituentJourneyStageService } from './scheduler-data-services/constituent-journey-stage.service';
import { ConstituentJourneyStageChangeHistoryService } from './scheduler-data-services/constituent-journey-stage-change-history.service';
import { ContactService } from './scheduler-data-services/contact.service';
import { ContactChangeHistoryService } from './scheduler-data-services/contact-change-history.service';
import { ContactContactService } from './scheduler-data-services/contact-contact.service';
import { ContactContactChangeHistoryService } from './scheduler-data-services/contact-contact-change-history.service';
import { ContactInteractionService } from './scheduler-data-services/contact-interaction.service';
import { ContactInteractionChangeHistoryService } from './scheduler-data-services/contact-interaction-change-history.service';
import { ContactMethodService } from './scheduler-data-services/contact-method.service';
import { ContactTagService } from './scheduler-data-services/contact-tag.service';
import { ContactTagChangeHistoryService } from './scheduler-data-services/contact-tag-change-history.service';
import { ContactTypeService } from './scheduler-data-services/contact-type.service';
import { CountryService } from './scheduler-data-services/country.service';
import { CrewService } from './scheduler-data-services/crew.service';
import { CrewChangeHistoryService } from './scheduler-data-services/crew-change-history.service';
import { CrewMemberService } from './scheduler-data-services/crew-member.service';
import { CrewMemberChangeHistoryService } from './scheduler-data-services/crew-member-change-history.service';
import { CurrencyService } from './scheduler-data-services/currency.service';
import { DependencyTypeService } from './scheduler-data-services/dependency-type.service';
import { EventCalendarService } from './scheduler-data-services/event-calendar.service';
import { EventChargeService } from './scheduler-data-services/event-charge.service';
import { EventChargeChangeHistoryService } from './scheduler-data-services/event-charge-change-history.service';
import { EventResourceAssignmentService } from './scheduler-data-services/event-resource-assignment.service';
import { EventResourceAssignmentChangeHistoryService } from './scheduler-data-services/event-resource-assignment-change-history.service';
import { EventStatusService } from './scheduler-data-services/event-status.service';
import { FundService } from './scheduler-data-services/fund.service';
import { FundChangeHistoryService } from './scheduler-data-services/fund-change-history.service';
import { GiftService } from './scheduler-data-services/gift.service';
import { GiftChangeHistoryService } from './scheduler-data-services/gift-change-history.service';
import { HouseholdService } from './scheduler-data-services/household.service';
import { HouseholdChangeHistoryService } from './scheduler-data-services/household-change-history.service';
import { IconService } from './scheduler-data-services/icon.service';
import { InteractionTypeService } from './scheduler-data-services/interaction-type.service';
import { NotificationSubscriptionService } from './scheduler-data-services/notification-subscription.service';
import { NotificationSubscriptionChangeHistoryService } from './scheduler-data-services/notification-subscription-change-history.service';
import { NotificationTypeService } from './scheduler-data-services/notification-type.service';
import { OfficeService } from './scheduler-data-services/office.service';
import { OfficeChangeHistoryService } from './scheduler-data-services/office-change-history.service';
import { OfficeContactService } from './scheduler-data-services/office-contact.service';
import { OfficeContactChangeHistoryService } from './scheduler-data-services/office-contact-change-history.service';
import { OfficeTypeService } from './scheduler-data-services/office-type.service';
import { PaymentTypeService } from './scheduler-data-services/payment-type.service';
import { PledgeService } from './scheduler-data-services/pledge.service';
import { PledgeChangeHistoryService } from './scheduler-data-services/pledge-change-history.service';
import { PriorityService } from './scheduler-data-services/priority.service';
import { QualificationService } from './scheduler-data-services/qualification.service';
import { RateSheetService } from './scheduler-data-services/rate-sheet.service';
import { RateSheetChangeHistoryService } from './scheduler-data-services/rate-sheet-change-history.service';
import { RateTypeService } from './scheduler-data-services/rate-type.service';
import { ReceiptTypeService } from './scheduler-data-services/receipt-type.service';
import { RecurrenceExceptionService } from './scheduler-data-services/recurrence-exception.service';
import { RecurrenceExceptionChangeHistoryService } from './scheduler-data-services/recurrence-exception-change-history.service';
import { RecurrenceFrequencyService } from './scheduler-data-services/recurrence-frequency.service';
import { RecurrenceRuleService } from './scheduler-data-services/recurrence-rule.service';
import { RecurrenceRuleChangeHistoryService } from './scheduler-data-services/recurrence-rule-change-history.service';
import { RelationshipTypeService } from './scheduler-data-services/relationship-type.service';
import { ResourceService } from './scheduler-data-services/resource.service';
import { ResourceAvailabilityService } from './scheduler-data-services/resource-availability.service';
import { ResourceAvailabilityChangeHistoryService } from './scheduler-data-services/resource-availability-change-history.service';
import { ResourceChangeHistoryService } from './scheduler-data-services/resource-change-history.service';
import { ResourceContactService } from './scheduler-data-services/resource-contact.service';
import { ResourceContactChangeHistoryService } from './scheduler-data-services/resource-contact-change-history.service';
import { ResourceQualificationService } from './scheduler-data-services/resource-qualification.service';
import { ResourceQualificationChangeHistoryService } from './scheduler-data-services/resource-qualification-change-history.service';
import { ResourceShiftService } from './scheduler-data-services/resource-shift.service';
import { ResourceShiftChangeHistoryService } from './scheduler-data-services/resource-shift-change-history.service';
import { ResourceTypeService } from './scheduler-data-services/resource-type.service';
import { SalutationService } from './scheduler-data-services/salutation.service';
import { ScheduledEventService } from './scheduler-data-services/scheduled-event.service';
import { ScheduledEventChangeHistoryService } from './scheduler-data-services/scheduled-event-change-history.service';
import { ScheduledEventDependencyService } from './scheduler-data-services/scheduled-event-dependency.service';
import { ScheduledEventDependencyChangeHistoryService } from './scheduler-data-services/scheduled-event-dependency-change-history.service';
import { ScheduledEventQualificationRequirementService } from './scheduler-data-services/scheduled-event-qualification-requirement.service';
import { ScheduledEventQualificationRequirementChangeHistoryService } from './scheduler-data-services/scheduled-event-qualification-requirement-change-history.service';
import { ScheduledEventTemplateService } from './scheduler-data-services/scheduled-event-template.service';
import { ScheduledEventTemplateChangeHistoryService } from './scheduler-data-services/scheduled-event-template-change-history.service';
import { ScheduledEventTemplateChargeService } from './scheduler-data-services/scheduled-event-template-charge.service';
import { ScheduledEventTemplateChargeChangeHistoryService } from './scheduler-data-services/scheduled-event-template-charge-change-history.service';
import { ScheduledEventTemplateQualificationRequirementService } from './scheduler-data-services/scheduled-event-template-qualification-requirement.service';
import { ScheduledEventTemplateQualificationRequirementChangeHistoryService } from './scheduler-data-services/scheduled-event-template-qualification-requirement-change-history.service';
import { SchedulingTargetService } from './scheduler-data-services/scheduling-target.service';
import { SchedulingTargetAddressService } from './scheduler-data-services/scheduling-target-address.service';
import { SchedulingTargetAddressChangeHistoryService } from './scheduler-data-services/scheduling-target-address-change-history.service';
import { SchedulingTargetChangeHistoryService } from './scheduler-data-services/scheduling-target-change-history.service';
import { SchedulingTargetContactService } from './scheduler-data-services/scheduling-target-contact.service';
import { SchedulingTargetContactChangeHistoryService } from './scheduler-data-services/scheduling-target-contact-change-history.service';
import { SchedulingTargetQualificationRequirementService } from './scheduler-data-services/scheduling-target-qualification-requirement.service';
import { SchedulingTargetQualificationRequirementChangeHistoryService } from './scheduler-data-services/scheduling-target-qualification-requirement-change-history.service';
import { SchedulingTargetTypeService } from './scheduler-data-services/scheduling-target-type.service';
import { ShiftPatternService } from './scheduler-data-services/shift-pattern.service';
import { ShiftPatternChangeHistoryService } from './scheduler-data-services/shift-pattern-change-history.service';
import { ShiftPatternDayService } from './scheduler-data-services/shift-pattern-day.service';
import { ShiftPatternDayChangeHistoryService } from './scheduler-data-services/shift-pattern-day-change-history.service';
import { SoftCreditService } from './scheduler-data-services/soft-credit.service';
import { SoftCreditChangeHistoryService } from './scheduler-data-services/soft-credit-change-history.service';
import { StateProvinceService } from './scheduler-data-services/state-province.service';
import { TagService } from './scheduler-data-services/tag.service';
import { TenantProfileService } from './scheduler-data-services/tenant-profile.service';
import { TenantProfileChangeHistoryService } from './scheduler-data-services/tenant-profile-change-history.service';
import { TimeZoneService } from './scheduler-data-services/time-zone.service';
import { TributeService } from './scheduler-data-services/tribute.service';
import { TributeChangeHistoryService } from './scheduler-data-services/tribute-change-history.service';
import { TributeTypeService } from './scheduler-data-services/tribute-type.service';
//
// End of imports for Scheduler Data Services
//

//
// Security Data Components - Auto generated
//
import { EntityDataTokenListingComponent } from './security-data-components/entity-data-token/entity-data-token-listing/entity-data-token-listing.component';
import { EntityDataTokenAddEditComponent } from './security-data-components/entity-data-token/entity-data-token-add-edit/entity-data-token-add-edit.component';
import { EntityDataTokenDetailComponent } from './security-data-components/entity-data-token/entity-data-token-detail/entity-data-token-detail.component';
import { EntityDataTokenTableComponent } from './security-data-components/entity-data-token/entity-data-token-table/entity-data-token-table.component';
import { EntityDataTokenEventListingComponent } from './security-data-components/entity-data-token-event/entity-data-token-event-listing/entity-data-token-event-listing.component';
import { EntityDataTokenEventAddEditComponent } from './security-data-components/entity-data-token-event/entity-data-token-event-add-edit/entity-data-token-event-add-edit.component';
import { EntityDataTokenEventDetailComponent } from './security-data-components/entity-data-token-event/entity-data-token-event-detail/entity-data-token-event-detail.component';
import { EntityDataTokenEventTableComponent } from './security-data-components/entity-data-token-event/entity-data-token-event-table/entity-data-token-event-table.component';
import { EntityDataTokenEventTypeListingComponent } from './security-data-components/entity-data-token-event-type/entity-data-token-event-type-listing/entity-data-token-event-type-listing.component';
import { EntityDataTokenEventTypeAddEditComponent } from './security-data-components/entity-data-token-event-type/entity-data-token-event-type-add-edit/entity-data-token-event-type-add-edit.component';
import { EntityDataTokenEventTypeDetailComponent } from './security-data-components/entity-data-token-event-type/entity-data-token-event-type-detail/entity-data-token-event-type-detail.component';
import { EntityDataTokenEventTypeTableComponent } from './security-data-components/entity-data-token-event-type/entity-data-token-event-type-table/entity-data-token-event-type-table.component';
import { LoginAttemptListingComponent } from './security-data-components/login-attempt/login-attempt-listing/login-attempt-listing.component';
import { LoginAttemptAddEditComponent } from './security-data-components/login-attempt/login-attempt-add-edit/login-attempt-add-edit.component';
import { LoginAttemptDetailComponent } from './security-data-components/login-attempt/login-attempt-detail/login-attempt-detail.component';
import { LoginAttemptTableComponent } from './security-data-components/login-attempt/login-attempt-table/login-attempt-table.component';
import { ModuleListingComponent } from './security-data-components/module/module-listing/module-listing.component';
import { ModuleAddEditComponent } from './security-data-components/module/module-add-edit/module-add-edit.component';
import { ModuleDetailComponent } from './security-data-components/module/module-detail/module-detail.component';
import { ModuleTableComponent } from './security-data-components/module/module-table/module-table.component';
import { ModuleSecurityRoleListingComponent } from './security-data-components/module-security-role/module-security-role-listing/module-security-role-listing.component';
import { ModuleSecurityRoleAddEditComponent } from './security-data-components/module-security-role/module-security-role-add-edit/module-security-role-add-edit.component';
import { ModuleSecurityRoleDetailComponent } from './security-data-components/module-security-role/module-security-role-detail/module-security-role-detail.component';
import { ModuleSecurityRoleTableComponent } from './security-data-components/module-security-role/module-security-role-table/module-security-role-table.component';
import { OAUTHTokenListingComponent } from './security-data-components/o-a-u-t-h-token/o-a-u-t-h-token-listing/o-a-u-t-h-token-listing.component';
import { OAUTHTokenAddEditComponent } from './security-data-components/o-a-u-t-h-token/o-a-u-t-h-token-add-edit/o-a-u-t-h-token-add-edit.component';
import { OAUTHTokenDetailComponent } from './security-data-components/o-a-u-t-h-token/o-a-u-t-h-token-detail/o-a-u-t-h-token-detail.component';
import { OAUTHTokenTableComponent } from './security-data-components/o-a-u-t-h-token/o-a-u-t-h-token-table/o-a-u-t-h-token-table.component';
import { PrivilegeListingComponent } from './security-data-components/privilege/privilege-listing/privilege-listing.component';
import { PrivilegeAddEditComponent } from './security-data-components/privilege/privilege-add-edit/privilege-add-edit.component';
import { PrivilegeDetailComponent } from './security-data-components/privilege/privilege-detail/privilege-detail.component';
import { PrivilegeTableComponent } from './security-data-components/privilege/privilege-table/privilege-table.component';
import { SecurityDepartmentListingComponent } from './security-data-components/security-department/security-department-listing/security-department-listing.component';
import { SecurityDepartmentAddEditComponent } from './security-data-components/security-department/security-department-add-edit/security-department-add-edit.component';
import { SecurityDepartmentDetailComponent } from './security-data-components/security-department/security-department-detail/security-department-detail.component';
import { SecurityDepartmentTableComponent } from './security-data-components/security-department/security-department-table/security-department-table.component';
import { SecurityDepartmentUserListingComponent } from './security-data-components/security-department-user/security-department-user-listing/security-department-user-listing.component';
import { SecurityDepartmentUserAddEditComponent } from './security-data-components/security-department-user/security-department-user-add-edit/security-department-user-add-edit.component';
import { SecurityDepartmentUserDetailComponent } from './security-data-components/security-department-user/security-department-user-detail/security-department-user-detail.component';
import { SecurityDepartmentUserTableComponent } from './security-data-components/security-department-user/security-department-user-table/security-department-user-table.component';
import { SecurityGroupListingComponent } from './security-data-components/security-group/security-group-listing/security-group-listing.component';
import { SecurityGroupAddEditComponent } from './security-data-components/security-group/security-group-add-edit/security-group-add-edit.component';
import { SecurityGroupDetailComponent } from './security-data-components/security-group/security-group-detail/security-group-detail.component';
import { SecurityGroupTableComponent } from './security-data-components/security-group/security-group-table/security-group-table.component';
import { SecurityGroupSecurityRoleListingComponent } from './security-data-components/security-group-security-role/security-group-security-role-listing/security-group-security-role-listing.component';
import { SecurityGroupSecurityRoleAddEditComponent } from './security-data-components/security-group-security-role/security-group-security-role-add-edit/security-group-security-role-add-edit.component';
import { SecurityGroupSecurityRoleDetailComponent } from './security-data-components/security-group-security-role/security-group-security-role-detail/security-group-security-role-detail.component';
import { SecurityGroupSecurityRoleTableComponent } from './security-data-components/security-group-security-role/security-group-security-role-table/security-group-security-role-table.component';
import { SecurityOrganizationListingComponent } from './security-data-components/security-organization/security-organization-listing/security-organization-listing.component';
import { SecurityOrganizationAddEditComponent } from './security-data-components/security-organization/security-organization-add-edit/security-organization-add-edit.component';
import { SecurityOrganizationDetailComponent } from './security-data-components/security-organization/security-organization-detail/security-organization-detail.component';
import { SecurityOrganizationTableComponent } from './security-data-components/security-organization/security-organization-table/security-organization-table.component';
import { SecurityOrganizationUserListingComponent } from './security-data-components/security-organization-user/security-organization-user-listing/security-organization-user-listing.component';
import { SecurityOrganizationUserAddEditComponent } from './security-data-components/security-organization-user/security-organization-user-add-edit/security-organization-user-add-edit.component';
import { SecurityOrganizationUserDetailComponent } from './security-data-components/security-organization-user/security-organization-user-detail/security-organization-user-detail.component';
import { SecurityOrganizationUserTableComponent } from './security-data-components/security-organization-user/security-organization-user-table/security-organization-user-table.component';
import { SecurityRoleListingComponent } from './security-data-components/security-role/security-role-listing/security-role-listing.component';
import { SecurityRoleAddEditComponent } from './security-data-components/security-role/security-role-add-edit/security-role-add-edit.component';
import { SecurityRoleDetailComponent } from './security-data-components/security-role/security-role-detail/security-role-detail.component';
import { SecurityRoleTableComponent } from './security-data-components/security-role/security-role-table/security-role-table.component';
import { SecurityTeamListingComponent } from './security-data-components/security-team/security-team-listing/security-team-listing.component';
import { SecurityTeamAddEditComponent } from './security-data-components/security-team/security-team-add-edit/security-team-add-edit.component';
import { SecurityTeamDetailComponent } from './security-data-components/security-team/security-team-detail/security-team-detail.component';
import { SecurityTeamTableComponent } from './security-data-components/security-team/security-team-table/security-team-table.component';
import { SecurityTeamUserListingComponent } from './security-data-components/security-team-user/security-team-user-listing/security-team-user-listing.component';
import { SecurityTeamUserAddEditComponent } from './security-data-components/security-team-user/security-team-user-add-edit/security-team-user-add-edit.component';
import { SecurityTeamUserDetailComponent } from './security-data-components/security-team-user/security-team-user-detail/security-team-user-detail.component';
import { SecurityTeamUserTableComponent } from './security-data-components/security-team-user/security-team-user-table/security-team-user-table.component';
import { SecurityTenantListingComponent } from './security-data-components/security-tenant/security-tenant-listing/security-tenant-listing.component';
import { SecurityTenantAddEditComponent } from './security-data-components/security-tenant/security-tenant-add-edit/security-tenant-add-edit.component';
import { SecurityTenantDetailComponent } from './security-data-components/security-tenant/security-tenant-detail/security-tenant-detail.component';
import { SecurityTenantTableComponent } from './security-data-components/security-tenant/security-tenant-table/security-tenant-table.component';
import { SecurityTenantUserListingComponent } from './security-data-components/security-tenant-user/security-tenant-user-listing/security-tenant-user-listing.component';
import { SecurityTenantUserAddEditComponent } from './security-data-components/security-tenant-user/security-tenant-user-add-edit/security-tenant-user-add-edit.component';
import { SecurityTenantUserDetailComponent } from './security-data-components/security-tenant-user/security-tenant-user-detail/security-tenant-user-detail.component';
import { SecurityTenantUserTableComponent } from './security-data-components/security-tenant-user/security-tenant-user-table/security-tenant-user-table.component';
import { SecurityUserListingComponent } from './security-data-components/security-user/security-user-listing/security-user-listing.component';
import { SecurityUserAddEditComponent } from './security-data-components/security-user/security-user-add-edit/security-user-add-edit.component';
import { SecurityUserDetailComponent } from './security-data-components/security-user/security-user-detail/security-user-detail.component';
import { SecurityUserTableComponent } from './security-data-components/security-user/security-user-table/security-user-table.component';
import { SecurityUserEventListingComponent } from './security-data-components/security-user-event/security-user-event-listing/security-user-event-listing.component';
import { SecurityUserEventAddEditComponent } from './security-data-components/security-user-event/security-user-event-add-edit/security-user-event-add-edit.component';
import { SecurityUserEventDetailComponent } from './security-data-components/security-user-event/security-user-event-detail/security-user-event-detail.component';
import { SecurityUserEventTableComponent } from './security-data-components/security-user-event/security-user-event-table/security-user-event-table.component';
import { SecurityUserEventTypeListingComponent } from './security-data-components/security-user-event-type/security-user-event-type-listing/security-user-event-type-listing.component';
import { SecurityUserEventTypeAddEditComponent } from './security-data-components/security-user-event-type/security-user-event-type-add-edit/security-user-event-type-add-edit.component';
import { SecurityUserEventTypeDetailComponent } from './security-data-components/security-user-event-type/security-user-event-type-detail/security-user-event-type-detail.component';
import { SecurityUserEventTypeTableComponent } from './security-data-components/security-user-event-type/security-user-event-type-table/security-user-event-type-table.component';
import { SecurityUserPasswordResetTokenListingComponent } from './security-data-components/security-user-password-reset-token/security-user-password-reset-token-listing/security-user-password-reset-token-listing.component';
import { SecurityUserPasswordResetTokenAddEditComponent } from './security-data-components/security-user-password-reset-token/security-user-password-reset-token-add-edit/security-user-password-reset-token-add-edit.component';
import { SecurityUserPasswordResetTokenDetailComponent } from './security-data-components/security-user-password-reset-token/security-user-password-reset-token-detail/security-user-password-reset-token-detail.component';
import { SecurityUserPasswordResetTokenTableComponent } from './security-data-components/security-user-password-reset-token/security-user-password-reset-token-table/security-user-password-reset-token-table.component';
import { SecurityUserSecurityGroupListingComponent } from './security-data-components/security-user-security-group/security-user-security-group-listing/security-user-security-group-listing.component';
import { SecurityUserSecurityGroupAddEditComponent } from './security-data-components/security-user-security-group/security-user-security-group-add-edit/security-user-security-group-add-edit.component';
import { SecurityUserSecurityGroupDetailComponent } from './security-data-components/security-user-security-group/security-user-security-group-detail/security-user-security-group-detail.component';
import { SecurityUserSecurityGroupTableComponent } from './security-data-components/security-user-security-group/security-user-security-group-table/security-user-security-group-table.component';
import { SecurityUserSecurityRoleListingComponent } from './security-data-components/security-user-security-role/security-user-security-role-listing/security-user-security-role-listing.component';
import { SecurityUserSecurityRoleAddEditComponent } from './security-data-components/security-user-security-role/security-user-security-role-add-edit/security-user-security-role-add-edit.component';
import { SecurityUserSecurityRoleDetailComponent } from './security-data-components/security-user-security-role/security-user-security-role-detail/security-user-security-role-detail.component';
import { SecurityUserSecurityRoleTableComponent } from './security-data-components/security-user-security-role/security-user-security-role-table/security-user-security-role-table.component';
import { SecurityUserTitleListingComponent } from './security-data-components/security-user-title/security-user-title-listing/security-user-title-listing.component';
import { SecurityUserTitleAddEditComponent } from './security-data-components/security-user-title/security-user-title-add-edit/security-user-title-add-edit.component';
import { SecurityUserTitleDetailComponent } from './security-data-components/security-user-title/security-user-title-detail/security-user-title-detail.component';
import { SecurityUserTitleTableComponent } from './security-data-components/security-user-title/security-user-title-table/security-user-title-table.component';
import { SystemSettingListingComponent } from './security-data-components/system-setting/system-setting-listing/system-setting-listing.component';
import { SystemSettingAddEditComponent } from './security-data-components/system-setting/system-setting-add-edit/system-setting-add-edit.component';
import { SystemSettingDetailComponent } from './security-data-components/system-setting/system-setting-detail/system-setting-detail.component';
import { SystemSettingTableComponent } from './security-data-components/system-setting/system-setting-table/system-setting-table.component';
//
// End Security Data Components
//




//
// Auditor Data Components - Auto generated
//
import { AuditAccessTypeListingComponent } from './auditor-data-components/audit-access-type/audit-access-type-listing/audit-access-type-listing.component';
import { AuditAccessTypeAddEditComponent } from './auditor-data-components/audit-access-type/audit-access-type-add-edit/audit-access-type-add-edit.component';
import { AuditAccessTypeDetailComponent } from './auditor-data-components/audit-access-type/audit-access-type-detail/audit-access-type-detail.component';
import { AuditAccessTypeTableComponent } from './auditor-data-components/audit-access-type/audit-access-type-table/audit-access-type-table.component';
import { AuditEventListingComponent } from './auditor-data-components/audit-event/audit-event-listing/audit-event-listing.component';
import { AuditEventAddEditComponent } from './auditor-data-components/audit-event/audit-event-add-edit/audit-event-add-edit.component';
import { AuditEventDetailComponent } from './auditor-data-components/audit-event/audit-event-detail/audit-event-detail.component';
import { AuditEventTableComponent } from './auditor-data-components/audit-event/audit-event-table/audit-event-table.component';
import { AuditEventEntityStateListingComponent } from './auditor-data-components/audit-event-entity-state/audit-event-entity-state-listing/audit-event-entity-state-listing.component';
import { AuditEventEntityStateAddEditComponent } from './auditor-data-components/audit-event-entity-state/audit-event-entity-state-add-edit/audit-event-entity-state-add-edit.component';
import { AuditEventEntityStateDetailComponent } from './auditor-data-components/audit-event-entity-state/audit-event-entity-state-detail/audit-event-entity-state-detail.component';
import { AuditEventEntityStateTableComponent } from './auditor-data-components/audit-event-entity-state/audit-event-entity-state-table/audit-event-entity-state-table.component';
import { AuditEventErrorMessageListingComponent } from './auditor-data-components/audit-event-error-message/audit-event-error-message-listing/audit-event-error-message-listing.component';
import { AuditEventErrorMessageAddEditComponent } from './auditor-data-components/audit-event-error-message/audit-event-error-message-add-edit/audit-event-error-message-add-edit.component';
import { AuditEventErrorMessageDetailComponent } from './auditor-data-components/audit-event-error-message/audit-event-error-message-detail/audit-event-error-message-detail.component';
import { AuditEventErrorMessageTableComponent } from './auditor-data-components/audit-event-error-message/audit-event-error-message-table/audit-event-error-message-table.component';
import { AuditHostSystemListingComponent } from './auditor-data-components/audit-host-system/audit-host-system-listing/audit-host-system-listing.component';
import { AuditHostSystemAddEditComponent } from './auditor-data-components/audit-host-system/audit-host-system-add-edit/audit-host-system-add-edit.component';
import { AuditHostSystemDetailComponent } from './auditor-data-components/audit-host-system/audit-host-system-detail/audit-host-system-detail.component';
import { AuditHostSystemTableComponent } from './auditor-data-components/audit-host-system/audit-host-system-table/audit-host-system-table.component';
import { AuditModuleListingComponent } from './auditor-data-components/audit-module/audit-module-listing/audit-module-listing.component';
import { AuditModuleAddEditComponent } from './auditor-data-components/audit-module/audit-module-add-edit/audit-module-add-edit.component';
import { AuditModuleDetailComponent } from './auditor-data-components/audit-module/audit-module-detail/audit-module-detail.component';
import { AuditModuleTableComponent } from './auditor-data-components/audit-module/audit-module-table/audit-module-table.component';
import { AuditModuleEntityListingComponent } from './auditor-data-components/audit-module-entity/audit-module-entity-listing/audit-module-entity-listing.component';
import { AuditModuleEntityAddEditComponent } from './auditor-data-components/audit-module-entity/audit-module-entity-add-edit/audit-module-entity-add-edit.component';
import { AuditModuleEntityDetailComponent } from './auditor-data-components/audit-module-entity/audit-module-entity-detail/audit-module-entity-detail.component';
import { AuditModuleEntityTableComponent } from './auditor-data-components/audit-module-entity/audit-module-entity-table/audit-module-entity-table.component';
import { AuditPlanBListingComponent } from './auditor-data-components/audit-plan-b/audit-plan-b-listing/audit-plan-b-listing.component';
import { AuditPlanBAddEditComponent } from './auditor-data-components/audit-plan-b/audit-plan-b-add-edit/audit-plan-b-add-edit.component';
import { AuditPlanBDetailComponent } from './auditor-data-components/audit-plan-b/audit-plan-b-detail/audit-plan-b-detail.component';
import { AuditPlanBTableComponent } from './auditor-data-components/audit-plan-b/audit-plan-b-table/audit-plan-b-table.component';
import { AuditResourceListingComponent } from './auditor-data-components/audit-resource/audit-resource-listing/audit-resource-listing.component';
import { AuditResourceAddEditComponent } from './auditor-data-components/audit-resource/audit-resource-add-edit/audit-resource-add-edit.component';
import { AuditResourceDetailComponent } from './auditor-data-components/audit-resource/audit-resource-detail/audit-resource-detail.component';
import { AuditResourceTableComponent } from './auditor-data-components/audit-resource/audit-resource-table/audit-resource-table.component';
import { AuditSessionListingComponent } from './auditor-data-components/audit-session/audit-session-listing/audit-session-listing.component';
import { AuditSessionAddEditComponent } from './auditor-data-components/audit-session/audit-session-add-edit/audit-session-add-edit.component';
import { AuditSessionDetailComponent } from './auditor-data-components/audit-session/audit-session-detail/audit-session-detail.component';
import { AuditSessionTableComponent } from './auditor-data-components/audit-session/audit-session-table/audit-session-table.component';
import { AuditSourceListingComponent } from './auditor-data-components/audit-source/audit-source-listing/audit-source-listing.component';
import { AuditSourceAddEditComponent } from './auditor-data-components/audit-source/audit-source-add-edit/audit-source-add-edit.component';
import { AuditSourceDetailComponent } from './auditor-data-components/audit-source/audit-source-detail/audit-source-detail.component';
import { AuditSourceTableComponent } from './auditor-data-components/audit-source/audit-source-table/audit-source-table.component';
import { AuditTypeListingComponent } from './auditor-data-components/audit-type/audit-type-listing/audit-type-listing.component';
import { AuditTypeAddEditComponent } from './auditor-data-components/audit-type/audit-type-add-edit/audit-type-add-edit.component';
import { AuditTypeDetailComponent } from './auditor-data-components/audit-type/audit-type-detail/audit-type-detail.component';
import { AuditTypeTableComponent } from './auditor-data-components/audit-type/audit-type-table/audit-type-table.component';
import { AuditUserListingComponent } from './auditor-data-components/audit-user/audit-user-listing/audit-user-listing.component';
import { AuditUserAddEditComponent } from './auditor-data-components/audit-user/audit-user-add-edit/audit-user-add-edit.component';
import { AuditUserDetailComponent } from './auditor-data-components/audit-user/audit-user-detail/audit-user-detail.component';
import { AuditUserTableComponent } from './auditor-data-components/audit-user/audit-user-table/audit-user-table.component';
import { AuditUserAgentListingComponent } from './auditor-data-components/audit-user-agent/audit-user-agent-listing/audit-user-agent-listing.component';
import { AuditUserAgentAddEditComponent } from './auditor-data-components/audit-user-agent/audit-user-agent-add-edit/audit-user-agent-add-edit.component';
import { AuditUserAgentDetailComponent } from './auditor-data-components/audit-user-agent/audit-user-agent-detail/audit-user-agent-detail.component';
import { AuditUserAgentTableComponent } from './auditor-data-components/audit-user-agent/audit-user-agent-table/audit-user-agent-table.component';
import { ExternalCommunicationListingComponent } from './auditor-data-components/external-communication/external-communication-listing/external-communication-listing.component';
import { ExternalCommunicationAddEditComponent } from './auditor-data-components/external-communication/external-communication-add-edit/external-communication-add-edit.component';
import { ExternalCommunicationDetailComponent } from './auditor-data-components/external-communication/external-communication-detail/external-communication-detail.component';
import { ExternalCommunicationTableComponent } from './auditor-data-components/external-communication/external-communication-table/external-communication-table.component';
import { ExternalCommunicationRecipientListingComponent } from './auditor-data-components/external-communication-recipient/external-communication-recipient-listing/external-communication-recipient-listing.component';
import { ExternalCommunicationRecipientAddEditComponent } from './auditor-data-components/external-communication-recipient/external-communication-recipient-add-edit/external-communication-recipient-add-edit.component';
import { ExternalCommunicationRecipientDetailComponent } from './auditor-data-components/external-communication-recipient/external-communication-recipient-detail/external-communication-recipient-detail.component';
import { ExternalCommunicationRecipientTableComponent } from './auditor-data-components/external-communication-recipient/external-communication-recipient-table/external-communication-recipient-table.component';
//
// End Auditor Data Components
//


//
// Beginning of imports for Scheduler Data Components 
//
import { AppealListingComponent } from './scheduler-data-components/appeal/appeal-listing/appeal-listing.component';
import { AppealAddEditComponent } from './scheduler-data-components/appeal/appeal-add-edit/appeal-add-edit.component';
import { AppealDetailComponent } from './scheduler-data-components/appeal/appeal-detail/appeal-detail.component';
import { AppealTableComponent } from './scheduler-data-components/appeal/appeal-table/appeal-table.component';
import { AppealChangeHistoryListingComponent } from './scheduler-data-components/appeal-change-history/appeal-change-history-listing/appeal-change-history-listing.component';
import { AppealChangeHistoryAddEditComponent } from './scheduler-data-components/appeal-change-history/appeal-change-history-add-edit/appeal-change-history-add-edit.component';
import { AppealChangeHistoryDetailComponent } from './scheduler-data-components/appeal-change-history/appeal-change-history-detail/appeal-change-history-detail.component';
import { AppealChangeHistoryTableComponent } from './scheduler-data-components/appeal-change-history/appeal-change-history-table/appeal-change-history-table.component';
import { AssignmentRoleListingComponent } from './scheduler-data-components/assignment-role/assignment-role-listing/assignment-role-listing.component';
import { AssignmentRoleAddEditComponent } from './scheduler-data-components/assignment-role/assignment-role-add-edit/assignment-role-add-edit.component';
import { AssignmentRoleDetailComponent } from './scheduler-data-components/assignment-role/assignment-role-detail/assignment-role-detail.component';
import { AssignmentRoleTableComponent } from './scheduler-data-components/assignment-role/assignment-role-table/assignment-role-table.component';
import { AssignmentRoleQualificationRequirementListingComponent } from './scheduler-data-components/assignment-role-qualification-requirement/assignment-role-qualification-requirement-listing/assignment-role-qualification-requirement-listing.component';
import { AssignmentRoleQualificationRequirementAddEditComponent } from './scheduler-data-components/assignment-role-qualification-requirement/assignment-role-qualification-requirement-add-edit/assignment-role-qualification-requirement-add-edit.component';
import { AssignmentRoleQualificationRequirementDetailComponent } from './scheduler-data-components/assignment-role-qualification-requirement/assignment-role-qualification-requirement-detail/assignment-role-qualification-requirement-detail.component';
import { AssignmentRoleQualificationRequirementTableComponent } from './scheduler-data-components/assignment-role-qualification-requirement/assignment-role-qualification-requirement-table/assignment-role-qualification-requirement-table.component';
import { AssignmentRoleQualificationRequirementChangeHistoryListingComponent } from './scheduler-data-components/assignment-role-qualification-requirement-change-history/assignment-role-qualification-requirement-change-history-listing/assignment-role-qualification-requirement-change-history-listing.component';
import { AssignmentRoleQualificationRequirementChangeHistoryAddEditComponent } from './scheduler-data-components/assignment-role-qualification-requirement-change-history/assignment-role-qualification-requirement-change-history-add-edit/assignment-role-qualification-requirement-change-history-add-edit.component';
import { AssignmentRoleQualificationRequirementChangeHistoryDetailComponent } from './scheduler-data-components/assignment-role-qualification-requirement-change-history/assignment-role-qualification-requirement-change-history-detail/assignment-role-qualification-requirement-change-history-detail.component';
import { AssignmentRoleQualificationRequirementChangeHistoryTableComponent } from './scheduler-data-components/assignment-role-qualification-requirement-change-history/assignment-role-qualification-requirement-change-history-table/assignment-role-qualification-requirement-change-history-table.component';
import { AssignmentStatusListingComponent } from './scheduler-data-components/assignment-status/assignment-status-listing/assignment-status-listing.component';
import { AssignmentStatusAddEditComponent } from './scheduler-data-components/assignment-status/assignment-status-add-edit/assignment-status-add-edit.component';
import { AssignmentStatusDetailComponent } from './scheduler-data-components/assignment-status/assignment-status-detail/assignment-status-detail.component';
import { AssignmentStatusTableComponent } from './scheduler-data-components/assignment-status/assignment-status-table/assignment-status-table.component';
import { AttributeDefinitionListingComponent } from './scheduler-data-components/attribute-definition/attribute-definition-listing/attribute-definition-listing.component';
import { AttributeDefinitionAddEditComponent } from './scheduler-data-components/attribute-definition/attribute-definition-add-edit/attribute-definition-add-edit.component';
import { AttributeDefinitionDetailComponent } from './scheduler-data-components/attribute-definition/attribute-definition-detail/attribute-definition-detail.component';
import { AttributeDefinitionTableComponent } from './scheduler-data-components/attribute-definition/attribute-definition-table/attribute-definition-table.component';
import { BatchListingComponent } from './scheduler-data-components/batch/batch-listing/batch-listing.component';
import { BatchAddEditComponent } from './scheduler-data-components/batch/batch-add-edit/batch-add-edit.component';
import { BatchDetailComponent } from './scheduler-data-components/batch/batch-detail/batch-detail.component';
import { BatchTableComponent } from './scheduler-data-components/batch/batch-table/batch-table.component';
import { BatchChangeHistoryListingComponent } from './scheduler-data-components/batch-change-history/batch-change-history-listing/batch-change-history-listing.component';
import { BatchChangeHistoryAddEditComponent } from './scheduler-data-components/batch-change-history/batch-change-history-add-edit/batch-change-history-add-edit.component';
import { BatchChangeHistoryDetailComponent } from './scheduler-data-components/batch-change-history/batch-change-history-detail/batch-change-history-detail.component';
import { BatchChangeHistoryTableComponent } from './scheduler-data-components/batch-change-history/batch-change-history-table/batch-change-history-table.component';
import { BatchStatusListingComponent } from './scheduler-data-components/batch-status/batch-status-listing/batch-status-listing.component';
import { BatchStatusAddEditComponent } from './scheduler-data-components/batch-status/batch-status-add-edit/batch-status-add-edit.component';
import { BatchStatusDetailComponent } from './scheduler-data-components/batch-status/batch-status-detail/batch-status-detail.component';
import { BatchStatusTableComponent } from './scheduler-data-components/batch-status/batch-status-table/batch-status-table.component';
import { BookingSourceTypeListingComponent } from './scheduler-data-components/booking-source-type/booking-source-type-listing/booking-source-type-listing.component';
import { BookingSourceTypeAddEditComponent } from './scheduler-data-components/booking-source-type/booking-source-type-add-edit/booking-source-type-add-edit.component';
import { BookingSourceTypeDetailComponent } from './scheduler-data-components/booking-source-type/booking-source-type-detail/booking-source-type-detail.component';
import { BookingSourceTypeTableComponent } from './scheduler-data-components/booking-source-type/booking-source-type-table/booking-source-type-table.component';
import { CalendarListingComponent } from './scheduler-data-components/calendar/calendar-listing/calendar-listing.component';
import { CalendarAddEditComponent } from './scheduler-data-components/calendar/calendar-add-edit/calendar-add-edit.component';
import { CalendarDetailComponent } from './scheduler-data-components/calendar/calendar-detail/calendar-detail.component';
import { CalendarTableComponent } from './scheduler-data-components/calendar/calendar-table/calendar-table.component';
import { CalendarChangeHistoryListingComponent } from './scheduler-data-components/calendar-change-history/calendar-change-history-listing/calendar-change-history-listing.component';
import { CalendarChangeHistoryAddEditComponent } from './scheduler-data-components/calendar-change-history/calendar-change-history-add-edit/calendar-change-history-add-edit.component';
import { CalendarChangeHistoryDetailComponent } from './scheduler-data-components/calendar-change-history/calendar-change-history-detail/calendar-change-history-detail.component';
import { CalendarChangeHistoryTableComponent } from './scheduler-data-components/calendar-change-history/calendar-change-history-table/calendar-change-history-table.component';
import { CampaignListingComponent } from './scheduler-data-components/campaign/campaign-listing/campaign-listing.component';
import { CampaignAddEditComponent } from './scheduler-data-components/campaign/campaign-add-edit/campaign-add-edit.component';
import { CampaignDetailComponent } from './scheduler-data-components/campaign/campaign-detail/campaign-detail.component';
import { CampaignTableComponent } from './scheduler-data-components/campaign/campaign-table/campaign-table.component';
import { CampaignChangeHistoryListingComponent } from './scheduler-data-components/campaign-change-history/campaign-change-history-listing/campaign-change-history-listing.component';
import { CampaignChangeHistoryAddEditComponent } from './scheduler-data-components/campaign-change-history/campaign-change-history-add-edit/campaign-change-history-add-edit.component';
import { CampaignChangeHistoryDetailComponent } from './scheduler-data-components/campaign-change-history/campaign-change-history-detail/campaign-change-history-detail.component';
import { CampaignChangeHistoryTableComponent } from './scheduler-data-components/campaign-change-history/campaign-change-history-table/campaign-change-history-table.component';
import { ChargeStatusListingComponent } from './scheduler-data-components/charge-status/charge-status-listing/charge-status-listing.component';
import { ChargeStatusAddEditComponent } from './scheduler-data-components/charge-status/charge-status-add-edit/charge-status-add-edit.component';
import { ChargeStatusDetailComponent } from './scheduler-data-components/charge-status/charge-status-detail/charge-status-detail.component';
import { ChargeStatusTableComponent } from './scheduler-data-components/charge-status/charge-status-table/charge-status-table.component';
import { ChargeTypeListingComponent } from './scheduler-data-components/charge-type/charge-type-listing/charge-type-listing.component';
import { ChargeTypeAddEditComponent } from './scheduler-data-components/charge-type/charge-type-add-edit/charge-type-add-edit.component';
import { ChargeTypeDetailComponent } from './scheduler-data-components/charge-type/charge-type-detail/charge-type-detail.component';
import { ChargeTypeTableComponent } from './scheduler-data-components/charge-type/charge-type-table/charge-type-table.component';
import { ChargeTypeChangeHistoryListingComponent } from './scheduler-data-components/charge-type-change-history/charge-type-change-history-listing/charge-type-change-history-listing.component';
import { ChargeTypeChangeHistoryAddEditComponent } from './scheduler-data-components/charge-type-change-history/charge-type-change-history-add-edit/charge-type-change-history-add-edit.component';
import { ChargeTypeChangeHistoryDetailComponent } from './scheduler-data-components/charge-type-change-history/charge-type-change-history-detail/charge-type-change-history-detail.component';
import { ChargeTypeChangeHistoryTableComponent } from './scheduler-data-components/charge-type-change-history/charge-type-change-history-table/charge-type-change-history-table.component';
import { ClientListingComponent } from './scheduler-data-components/client/client-listing/client-listing.component';
import { ClientAddEditComponent } from './scheduler-data-components/client/client-add-edit/client-add-edit.component';
import { ClientDetailComponent } from './scheduler-data-components/client/client-detail/client-detail.component';
import { ClientTableComponent } from './scheduler-data-components/client/client-table/client-table.component';
import { ClientChangeHistoryListingComponent } from './scheduler-data-components/client-change-history/client-change-history-listing/client-change-history-listing.component';
import { ClientChangeHistoryAddEditComponent } from './scheduler-data-components/client-change-history/client-change-history-add-edit/client-change-history-add-edit.component';
import { ClientChangeHistoryDetailComponent } from './scheduler-data-components/client-change-history/client-change-history-detail/client-change-history-detail.component';
import { ClientChangeHistoryTableComponent } from './scheduler-data-components/client-change-history/client-change-history-table/client-change-history-table.component';
import { ClientContactListingComponent } from './scheduler-data-components/client-contact/client-contact-listing/client-contact-listing.component';
import { ClientContactAddEditComponent } from './scheduler-data-components/client-contact/client-contact-add-edit/client-contact-add-edit.component';
import { ClientContactDetailComponent } from './scheduler-data-components/client-contact/client-contact-detail/client-contact-detail.component';
import { ClientContactTableComponent } from './scheduler-data-components/client-contact/client-contact-table/client-contact-table.component';
import { ClientContactChangeHistoryListingComponent } from './scheduler-data-components/client-contact-change-history/client-contact-change-history-listing/client-contact-change-history-listing.component';
import { ClientContactChangeHistoryAddEditComponent } from './scheduler-data-components/client-contact-change-history/client-contact-change-history-add-edit/client-contact-change-history-add-edit.component';
import { ClientContactChangeHistoryDetailComponent } from './scheduler-data-components/client-contact-change-history/client-contact-change-history-detail/client-contact-change-history-detail.component';
import { ClientContactChangeHistoryTableComponent } from './scheduler-data-components/client-contact-change-history/client-contact-change-history-table/client-contact-change-history-table.component';
import { ClientTypeListingComponent } from './scheduler-data-components/client-type/client-type-listing/client-type-listing.component';
import { ClientTypeAddEditComponent } from './scheduler-data-components/client-type/client-type-add-edit/client-type-add-edit.component';
import { ClientTypeDetailComponent } from './scheduler-data-components/client-type/client-type-detail/client-type-detail.component';
import { ClientTypeTableComponent } from './scheduler-data-components/client-type/client-type-table/client-type-table.component';
import { ConstituentListingComponent } from './scheduler-data-components/constituent/constituent-listing/constituent-listing.component';
import { ConstituentAddEditComponent } from './scheduler-data-components/constituent/constituent-add-edit/constituent-add-edit.component';
import { ConstituentDetailComponent } from './scheduler-data-components/constituent/constituent-detail/constituent-detail.component';
import { ConstituentTableComponent } from './scheduler-data-components/constituent/constituent-table/constituent-table.component';
import { ConstituentChangeHistoryListingComponent } from './scheduler-data-components/constituent-change-history/constituent-change-history-listing/constituent-change-history-listing.component';
import { ConstituentChangeHistoryAddEditComponent } from './scheduler-data-components/constituent-change-history/constituent-change-history-add-edit/constituent-change-history-add-edit.component';
import { ConstituentChangeHistoryDetailComponent } from './scheduler-data-components/constituent-change-history/constituent-change-history-detail/constituent-change-history-detail.component';
import { ConstituentChangeHistoryTableComponent } from './scheduler-data-components/constituent-change-history/constituent-change-history-table/constituent-change-history-table.component';
import { ConstituentJourneyStageListingComponent } from './scheduler-data-components/constituent-journey-stage/constituent-journey-stage-listing/constituent-journey-stage-listing.component';
import { ConstituentJourneyStageAddEditComponent } from './scheduler-data-components/constituent-journey-stage/constituent-journey-stage-add-edit/constituent-journey-stage-add-edit.component';
import { ConstituentJourneyStageDetailComponent } from './scheduler-data-components/constituent-journey-stage/constituent-journey-stage-detail/constituent-journey-stage-detail.component';
import { ConstituentJourneyStageTableComponent } from './scheduler-data-components/constituent-journey-stage/constituent-journey-stage-table/constituent-journey-stage-table.component';
import { ConstituentJourneyStageChangeHistoryListingComponent } from './scheduler-data-components/constituent-journey-stage-change-history/constituent-journey-stage-change-history-listing/constituent-journey-stage-change-history-listing.component';
import { ConstituentJourneyStageChangeHistoryAddEditComponent } from './scheduler-data-components/constituent-journey-stage-change-history/constituent-journey-stage-change-history-add-edit/constituent-journey-stage-change-history-add-edit.component';
import { ConstituentJourneyStageChangeHistoryDetailComponent } from './scheduler-data-components/constituent-journey-stage-change-history/constituent-journey-stage-change-history-detail/constituent-journey-stage-change-history-detail.component';
import { ConstituentJourneyStageChangeHistoryTableComponent } from './scheduler-data-components/constituent-journey-stage-change-history/constituent-journey-stage-change-history-table/constituent-journey-stage-change-history-table.component';
import { ContactListingComponent } from './scheduler-data-components/contact/contact-listing/contact-listing.component';
import { ContactAddEditComponent } from './scheduler-data-components/contact/contact-add-edit/contact-add-edit.component';
import { ContactDetailComponent } from './scheduler-data-components/contact/contact-detail/contact-detail.component';
import { ContactTableComponent } from './scheduler-data-components/contact/contact-table/contact-table.component';
import { ContactChangeHistoryListingComponent } from './scheduler-data-components/contact-change-history/contact-change-history-listing/contact-change-history-listing.component';
import { ContactChangeHistoryAddEditComponent } from './scheduler-data-components/contact-change-history/contact-change-history-add-edit/contact-change-history-add-edit.component';
import { ContactChangeHistoryDetailComponent } from './scheduler-data-components/contact-change-history/contact-change-history-detail/contact-change-history-detail.component';
import { ContactChangeHistoryTableComponent } from './scheduler-data-components/contact-change-history/contact-change-history-table/contact-change-history-table.component';
import { ContactContactListingComponent } from './scheduler-data-components/contact-contact/contact-contact-listing/contact-contact-listing.component';
import { ContactContactAddEditComponent } from './scheduler-data-components/contact-contact/contact-contact-add-edit/contact-contact-add-edit.component';
import { ContactContactDetailComponent } from './scheduler-data-components/contact-contact/contact-contact-detail/contact-contact-detail.component';
import { ContactContactTableComponent } from './scheduler-data-components/contact-contact/contact-contact-table/contact-contact-table.component';
import { ContactContactChangeHistoryListingComponent } from './scheduler-data-components/contact-contact-change-history/contact-contact-change-history-listing/contact-contact-change-history-listing.component';
import { ContactContactChangeHistoryAddEditComponent } from './scheduler-data-components/contact-contact-change-history/contact-contact-change-history-add-edit/contact-contact-change-history-add-edit.component';
import { ContactContactChangeHistoryDetailComponent } from './scheduler-data-components/contact-contact-change-history/contact-contact-change-history-detail/contact-contact-change-history-detail.component';
import { ContactContactChangeHistoryTableComponent } from './scheduler-data-components/contact-contact-change-history/contact-contact-change-history-table/contact-contact-change-history-table.component';
import { ContactInteractionListingComponent } from './scheduler-data-components/contact-interaction/contact-interaction-listing/contact-interaction-listing.component';
import { ContactInteractionAddEditComponent } from './scheduler-data-components/contact-interaction/contact-interaction-add-edit/contact-interaction-add-edit.component';
import { ContactInteractionDetailComponent } from './scheduler-data-components/contact-interaction/contact-interaction-detail/contact-interaction-detail.component';
import { ContactInteractionTableComponent } from './scheduler-data-components/contact-interaction/contact-interaction-table/contact-interaction-table.component';
import { ContactInteractionChangeHistoryListingComponent } from './scheduler-data-components/contact-interaction-change-history/contact-interaction-change-history-listing/contact-interaction-change-history-listing.component';
import { ContactInteractionChangeHistoryAddEditComponent } from './scheduler-data-components/contact-interaction-change-history/contact-interaction-change-history-add-edit/contact-interaction-change-history-add-edit.component';
import { ContactInteractionChangeHistoryDetailComponent } from './scheduler-data-components/contact-interaction-change-history/contact-interaction-change-history-detail/contact-interaction-change-history-detail.component';
import { ContactInteractionChangeHistoryTableComponent } from './scheduler-data-components/contact-interaction-change-history/contact-interaction-change-history-table/contact-interaction-change-history-table.component';
import { ContactMethodListingComponent } from './scheduler-data-components/contact-method/contact-method-listing/contact-method-listing.component';
import { ContactMethodAddEditComponent } from './scheduler-data-components/contact-method/contact-method-add-edit/contact-method-add-edit.component';
import { ContactMethodDetailComponent } from './scheduler-data-components/contact-method/contact-method-detail/contact-method-detail.component';
import { ContactMethodTableComponent } from './scheduler-data-components/contact-method/contact-method-table/contact-method-table.component';
import { ContactTagListingComponent } from './scheduler-data-components/contact-tag/contact-tag-listing/contact-tag-listing.component';
import { ContactTagAddEditComponent } from './scheduler-data-components/contact-tag/contact-tag-add-edit/contact-tag-add-edit.component';
import { ContactTagDetailComponent } from './scheduler-data-components/contact-tag/contact-tag-detail/contact-tag-detail.component';
import { ContactTagTableComponent } from './scheduler-data-components/contact-tag/contact-tag-table/contact-tag-table.component';
import { ContactTagChangeHistoryListingComponent } from './scheduler-data-components/contact-tag-change-history/contact-tag-change-history-listing/contact-tag-change-history-listing.component';
import { ContactTagChangeHistoryAddEditComponent } from './scheduler-data-components/contact-tag-change-history/contact-tag-change-history-add-edit/contact-tag-change-history-add-edit.component';
import { ContactTagChangeHistoryDetailComponent } from './scheduler-data-components/contact-tag-change-history/contact-tag-change-history-detail/contact-tag-change-history-detail.component';
import { ContactTagChangeHistoryTableComponent } from './scheduler-data-components/contact-tag-change-history/contact-tag-change-history-table/contact-tag-change-history-table.component';
import { ContactTypeListingComponent } from './scheduler-data-components/contact-type/contact-type-listing/contact-type-listing.component';
import { ContactTypeAddEditComponent } from './scheduler-data-components/contact-type/contact-type-add-edit/contact-type-add-edit.component';
import { ContactTypeDetailComponent } from './scheduler-data-components/contact-type/contact-type-detail/contact-type-detail.component';
import { ContactTypeTableComponent } from './scheduler-data-components/contact-type/contact-type-table/contact-type-table.component';
import { CountryListingComponent } from './scheduler-data-components/country/country-listing/country-listing.component';
import { CountryAddEditComponent } from './scheduler-data-components/country/country-add-edit/country-add-edit.component';
import { CountryDetailComponent } from './scheduler-data-components/country/country-detail/country-detail.component';
import { CountryTableComponent } from './scheduler-data-components/country/country-table/country-table.component';
import { CrewListingComponent } from './scheduler-data-components/crew/crew-listing/crew-listing.component';
import { CrewAddEditComponent } from './scheduler-data-components/crew/crew-add-edit/crew-add-edit.component';
import { CrewDetailComponent } from './scheduler-data-components/crew/crew-detail/crew-detail.component';
import { CrewTableComponent } from './scheduler-data-components/crew/crew-table/crew-table.component';
import { CrewChangeHistoryListingComponent } from './scheduler-data-components/crew-change-history/crew-change-history-listing/crew-change-history-listing.component';
import { CrewChangeHistoryAddEditComponent } from './scheduler-data-components/crew-change-history/crew-change-history-add-edit/crew-change-history-add-edit.component';
import { CrewChangeHistoryDetailComponent } from './scheduler-data-components/crew-change-history/crew-change-history-detail/crew-change-history-detail.component';
import { CrewChangeHistoryTableComponent } from './scheduler-data-components/crew-change-history/crew-change-history-table/crew-change-history-table.component';
import { CrewMemberListingComponent } from './scheduler-data-components/crew-member/crew-member-listing/crew-member-listing.component';
import { CrewMemberAddEditComponent } from './scheduler-data-components/crew-member/crew-member-add-edit/crew-member-add-edit.component';
import { CrewMemberDetailComponent } from './scheduler-data-components/crew-member/crew-member-detail/crew-member-detail.component';
import { CrewMemberTableComponent } from './scheduler-data-components/crew-member/crew-member-table/crew-member-table.component';
import { CrewMemberChangeHistoryListingComponent } from './scheduler-data-components/crew-member-change-history/crew-member-change-history-listing/crew-member-change-history-listing.component';
import { CrewMemberChangeHistoryAddEditComponent } from './scheduler-data-components/crew-member-change-history/crew-member-change-history-add-edit/crew-member-change-history-add-edit.component';
import { CrewMemberChangeHistoryDetailComponent } from './scheduler-data-components/crew-member-change-history/crew-member-change-history-detail/crew-member-change-history-detail.component';
import { CrewMemberChangeHistoryTableComponent } from './scheduler-data-components/crew-member-change-history/crew-member-change-history-table/crew-member-change-history-table.component';
import { CurrencyListingComponent } from './scheduler-data-components/currency/currency-listing/currency-listing.component';
import { CurrencyAddEditComponent } from './scheduler-data-components/currency/currency-add-edit/currency-add-edit.component';
import { CurrencyDetailComponent } from './scheduler-data-components/currency/currency-detail/currency-detail.component';
import { CurrencyTableComponent } from './scheduler-data-components/currency/currency-table/currency-table.component';
import { DependencyTypeListingComponent } from './scheduler-data-components/dependency-type/dependency-type-listing/dependency-type-listing.component';
import { DependencyTypeAddEditComponent } from './scheduler-data-components/dependency-type/dependency-type-add-edit/dependency-type-add-edit.component';
import { DependencyTypeDetailComponent } from './scheduler-data-components/dependency-type/dependency-type-detail/dependency-type-detail.component';
import { DependencyTypeTableComponent } from './scheduler-data-components/dependency-type/dependency-type-table/dependency-type-table.component';
import { EventCalendarListingComponent } from './scheduler-data-components/event-calendar/event-calendar-listing/event-calendar-listing.component';
import { EventCalendarAddEditComponent } from './scheduler-data-components/event-calendar/event-calendar-add-edit/event-calendar-add-edit.component';
import { EventCalendarDetailComponent } from './scheduler-data-components/event-calendar/event-calendar-detail/event-calendar-detail.component';
import { EventCalendarTableComponent } from './scheduler-data-components/event-calendar/event-calendar-table/event-calendar-table.component';
import { EventChargeListingComponent } from './scheduler-data-components/event-charge/event-charge-listing/event-charge-listing.component';
import { EventChargeAddEditComponent } from './scheduler-data-components/event-charge/event-charge-add-edit/event-charge-add-edit.component';
import { EventChargeDetailComponent } from './scheduler-data-components/event-charge/event-charge-detail/event-charge-detail.component';
import { EventChargeTableComponent } from './scheduler-data-components/event-charge/event-charge-table/event-charge-table.component';
import { EventChargeChangeHistoryListingComponent } from './scheduler-data-components/event-charge-change-history/event-charge-change-history-listing/event-charge-change-history-listing.component';
import { EventChargeChangeHistoryAddEditComponent } from './scheduler-data-components/event-charge-change-history/event-charge-change-history-add-edit/event-charge-change-history-add-edit.component';
import { EventChargeChangeHistoryDetailComponent } from './scheduler-data-components/event-charge-change-history/event-charge-change-history-detail/event-charge-change-history-detail.component';
import { EventChargeChangeHistoryTableComponent } from './scheduler-data-components/event-charge-change-history/event-charge-change-history-table/event-charge-change-history-table.component';
import { EventResourceAssignmentListingComponent } from './scheduler-data-components/event-resource-assignment/event-resource-assignment-listing/event-resource-assignment-listing.component';
import { EventResourceAssignmentAddEditComponent } from './scheduler-data-components/event-resource-assignment/event-resource-assignment-add-edit/event-resource-assignment-add-edit.component';
import { EventResourceAssignmentDetailComponent } from './scheduler-data-components/event-resource-assignment/event-resource-assignment-detail/event-resource-assignment-detail.component';
import { EventResourceAssignmentTableComponent } from './scheduler-data-components/event-resource-assignment/event-resource-assignment-table/event-resource-assignment-table.component';
import { EventResourceAssignmentChangeHistoryListingComponent } from './scheduler-data-components/event-resource-assignment-change-history/event-resource-assignment-change-history-listing/event-resource-assignment-change-history-listing.component';
import { EventResourceAssignmentChangeHistoryAddEditComponent } from './scheduler-data-components/event-resource-assignment-change-history/event-resource-assignment-change-history-add-edit/event-resource-assignment-change-history-add-edit.component';
import { EventResourceAssignmentChangeHistoryDetailComponent } from './scheduler-data-components/event-resource-assignment-change-history/event-resource-assignment-change-history-detail/event-resource-assignment-change-history-detail.component';
import { EventResourceAssignmentChangeHistoryTableComponent } from './scheduler-data-components/event-resource-assignment-change-history/event-resource-assignment-change-history-table/event-resource-assignment-change-history-table.component';
import { EventStatusListingComponent } from './scheduler-data-components/event-status/event-status-listing/event-status-listing.component';
import { EventStatusAddEditComponent } from './scheduler-data-components/event-status/event-status-add-edit/event-status-add-edit.component';
import { EventStatusDetailComponent } from './scheduler-data-components/event-status/event-status-detail/event-status-detail.component';
import { EventStatusTableComponent } from './scheduler-data-components/event-status/event-status-table/event-status-table.component';
import { FundListingComponent } from './scheduler-data-components/fund/fund-listing/fund-listing.component';
import { FundAddEditComponent } from './scheduler-data-components/fund/fund-add-edit/fund-add-edit.component';
import { FundDetailComponent } from './scheduler-data-components/fund/fund-detail/fund-detail.component';
import { FundTableComponent } from './scheduler-data-components/fund/fund-table/fund-table.component';
import { FundChangeHistoryListingComponent } from './scheduler-data-components/fund-change-history/fund-change-history-listing/fund-change-history-listing.component';
import { FundChangeHistoryAddEditComponent } from './scheduler-data-components/fund-change-history/fund-change-history-add-edit/fund-change-history-add-edit.component';
import { FundChangeHistoryDetailComponent } from './scheduler-data-components/fund-change-history/fund-change-history-detail/fund-change-history-detail.component';
import { FundChangeHistoryTableComponent } from './scheduler-data-components/fund-change-history/fund-change-history-table/fund-change-history-table.component';
import { GiftListingComponent } from './scheduler-data-components/gift/gift-listing/gift-listing.component';
import { GiftAddEditComponent } from './scheduler-data-components/gift/gift-add-edit/gift-add-edit.component';
import { GiftDetailComponent } from './scheduler-data-components/gift/gift-detail/gift-detail.component';
import { GiftTableComponent } from './scheduler-data-components/gift/gift-table/gift-table.component';
import { GiftChangeHistoryListingComponent } from './scheduler-data-components/gift-change-history/gift-change-history-listing/gift-change-history-listing.component';
import { GiftChangeHistoryAddEditComponent } from './scheduler-data-components/gift-change-history/gift-change-history-add-edit/gift-change-history-add-edit.component';
import { GiftChangeHistoryDetailComponent } from './scheduler-data-components/gift-change-history/gift-change-history-detail/gift-change-history-detail.component';
import { GiftChangeHistoryTableComponent } from './scheduler-data-components/gift-change-history/gift-change-history-table/gift-change-history-table.component';
import { HouseholdListingComponent } from './scheduler-data-components/household/household-listing/household-listing.component';
import { HouseholdAddEditComponent } from './scheduler-data-components/household/household-add-edit/household-add-edit.component';
import { HouseholdDetailComponent } from './scheduler-data-components/household/household-detail/household-detail.component';
import { HouseholdTableComponent } from './scheduler-data-components/household/household-table/household-table.component';
import { HouseholdChangeHistoryListingComponent } from './scheduler-data-components/household-change-history/household-change-history-listing/household-change-history-listing.component';
import { HouseholdChangeHistoryAddEditComponent } from './scheduler-data-components/household-change-history/household-change-history-add-edit/household-change-history-add-edit.component';
import { HouseholdChangeHistoryDetailComponent } from './scheduler-data-components/household-change-history/household-change-history-detail/household-change-history-detail.component';
import { HouseholdChangeHistoryTableComponent } from './scheduler-data-components/household-change-history/household-change-history-table/household-change-history-table.component';
import { IconListingComponent } from './scheduler-data-components/icon/icon-listing/icon-listing.component';
import { IconAddEditComponent } from './scheduler-data-components/icon/icon-add-edit/icon-add-edit.component';
import { IconDetailComponent } from './scheduler-data-components/icon/icon-detail/icon-detail.component';
import { IconTableComponent } from './scheduler-data-components/icon/icon-table/icon-table.component';
import { InteractionTypeListingComponent } from './scheduler-data-components/interaction-type/interaction-type-listing/interaction-type-listing.component';
import { InteractionTypeAddEditComponent } from './scheduler-data-components/interaction-type/interaction-type-add-edit/interaction-type-add-edit.component';
import { InteractionTypeDetailComponent } from './scheduler-data-components/interaction-type/interaction-type-detail/interaction-type-detail.component';
import { InteractionTypeTableComponent } from './scheduler-data-components/interaction-type/interaction-type-table/interaction-type-table.component';
import { NotificationSubscriptionListingComponent } from './scheduler-data-components/notification-subscription/notification-subscription-listing/notification-subscription-listing.component';
import { NotificationSubscriptionAddEditComponent } from './scheduler-data-components/notification-subscription/notification-subscription-add-edit/notification-subscription-add-edit.component';
import { NotificationSubscriptionDetailComponent } from './scheduler-data-components/notification-subscription/notification-subscription-detail/notification-subscription-detail.component';
import { NotificationSubscriptionTableComponent } from './scheduler-data-components/notification-subscription/notification-subscription-table/notification-subscription-table.component';
import { NotificationSubscriptionChangeHistoryListingComponent } from './scheduler-data-components/notification-subscription-change-history/notification-subscription-change-history-listing/notification-subscription-change-history-listing.component';
import { NotificationSubscriptionChangeHistoryAddEditComponent } from './scheduler-data-components/notification-subscription-change-history/notification-subscription-change-history-add-edit/notification-subscription-change-history-add-edit.component';
import { NotificationSubscriptionChangeHistoryDetailComponent } from './scheduler-data-components/notification-subscription-change-history/notification-subscription-change-history-detail/notification-subscription-change-history-detail.component';
import { NotificationSubscriptionChangeHistoryTableComponent } from './scheduler-data-components/notification-subscription-change-history/notification-subscription-change-history-table/notification-subscription-change-history-table.component';
import { NotificationTypeListingComponent } from './scheduler-data-components/notification-type/notification-type-listing/notification-type-listing.component';
import { NotificationTypeAddEditComponent } from './scheduler-data-components/notification-type/notification-type-add-edit/notification-type-add-edit.component';
import { NotificationTypeDetailComponent } from './scheduler-data-components/notification-type/notification-type-detail/notification-type-detail.component';
import { NotificationTypeTableComponent } from './scheduler-data-components/notification-type/notification-type-table/notification-type-table.component';
import { OfficeListingComponent } from './scheduler-data-components/office/office-listing/office-listing.component';
import { OfficeAddEditComponent } from './scheduler-data-components/office/office-add-edit/office-add-edit.component';
import { OfficeDetailComponent } from './scheduler-data-components/office/office-detail/office-detail.component';
import { OfficeTableComponent } from './scheduler-data-components/office/office-table/office-table.component';
import { OfficeChangeHistoryListingComponent } from './scheduler-data-components/office-change-history/office-change-history-listing/office-change-history-listing.component';
import { OfficeChangeHistoryAddEditComponent } from './scheduler-data-components/office-change-history/office-change-history-add-edit/office-change-history-add-edit.component';
import { OfficeChangeHistoryDetailComponent } from './scheduler-data-components/office-change-history/office-change-history-detail/office-change-history-detail.component';
import { OfficeChangeHistoryTableComponent } from './scheduler-data-components/office-change-history/office-change-history-table/office-change-history-table.component';
import { OfficeContactListingComponent } from './scheduler-data-components/office-contact/office-contact-listing/office-contact-listing.component';
import { OfficeContactAddEditComponent } from './scheduler-data-components/office-contact/office-contact-add-edit/office-contact-add-edit.component';
import { OfficeContactDetailComponent } from './scheduler-data-components/office-contact/office-contact-detail/office-contact-detail.component';
import { OfficeContactTableComponent } from './scheduler-data-components/office-contact/office-contact-table/office-contact-table.component';
import { OfficeContactChangeHistoryListingComponent } from './scheduler-data-components/office-contact-change-history/office-contact-change-history-listing/office-contact-change-history-listing.component';
import { OfficeContactChangeHistoryAddEditComponent } from './scheduler-data-components/office-contact-change-history/office-contact-change-history-add-edit/office-contact-change-history-add-edit.component';
import { OfficeContactChangeHistoryDetailComponent } from './scheduler-data-components/office-contact-change-history/office-contact-change-history-detail/office-contact-change-history-detail.component';
import { OfficeContactChangeHistoryTableComponent } from './scheduler-data-components/office-contact-change-history/office-contact-change-history-table/office-contact-change-history-table.component';
import { OfficeTypeListingComponent } from './scheduler-data-components/office-type/office-type-listing/office-type-listing.component';
import { OfficeTypeAddEditComponent } from './scheduler-data-components/office-type/office-type-add-edit/office-type-add-edit.component';
import { OfficeTypeDetailComponent } from './scheduler-data-components/office-type/office-type-detail/office-type-detail.component';
import { OfficeTypeTableComponent } from './scheduler-data-components/office-type/office-type-table/office-type-table.component';
import { PaymentTypeListingComponent } from './scheduler-data-components/payment-type/payment-type-listing/payment-type-listing.component';
import { PaymentTypeAddEditComponent } from './scheduler-data-components/payment-type/payment-type-add-edit/payment-type-add-edit.component';
import { PaymentTypeDetailComponent } from './scheduler-data-components/payment-type/payment-type-detail/payment-type-detail.component';
import { PaymentTypeTableComponent } from './scheduler-data-components/payment-type/payment-type-table/payment-type-table.component';
import { PledgeListingComponent } from './scheduler-data-components/pledge/pledge-listing/pledge-listing.component';
import { PledgeAddEditComponent } from './scheduler-data-components/pledge/pledge-add-edit/pledge-add-edit.component';
import { PledgeDetailComponent } from './scheduler-data-components/pledge/pledge-detail/pledge-detail.component';
import { PledgeTableComponent } from './scheduler-data-components/pledge/pledge-table/pledge-table.component';
import { PledgeChangeHistoryListingComponent } from './scheduler-data-components/pledge-change-history/pledge-change-history-listing/pledge-change-history-listing.component';
import { PledgeChangeHistoryAddEditComponent } from './scheduler-data-components/pledge-change-history/pledge-change-history-add-edit/pledge-change-history-add-edit.component';
import { PledgeChangeHistoryDetailComponent } from './scheduler-data-components/pledge-change-history/pledge-change-history-detail/pledge-change-history-detail.component';
import { PledgeChangeHistoryTableComponent } from './scheduler-data-components/pledge-change-history/pledge-change-history-table/pledge-change-history-table.component';
import { PriorityListingComponent } from './scheduler-data-components/priority/priority-listing/priority-listing.component';
import { PriorityAddEditComponent } from './scheduler-data-components/priority/priority-add-edit/priority-add-edit.component';
import { PriorityDetailComponent } from './scheduler-data-components/priority/priority-detail/priority-detail.component';
import { PriorityTableComponent } from './scheduler-data-components/priority/priority-table/priority-table.component';
import { QualificationListingComponent } from './scheduler-data-components/qualification/qualification-listing/qualification-listing.component';
import { QualificationAddEditComponent } from './scheduler-data-components/qualification/qualification-add-edit/qualification-add-edit.component';
import { QualificationDetailComponent } from './scheduler-data-components/qualification/qualification-detail/qualification-detail.component';
import { QualificationTableComponent } from './scheduler-data-components/qualification/qualification-table/qualification-table.component';
import { RateSheetListingComponent } from './scheduler-data-components/rate-sheet/rate-sheet-listing/rate-sheet-listing.component';
import { RateSheetAddEditComponent } from './scheduler-data-components/rate-sheet/rate-sheet-add-edit/rate-sheet-add-edit.component';
import { RateSheetDetailComponent } from './scheduler-data-components/rate-sheet/rate-sheet-detail/rate-sheet-detail.component';
import { RateSheetTableComponent } from './scheduler-data-components/rate-sheet/rate-sheet-table/rate-sheet-table.component';
import { RateSheetChangeHistoryListingComponent } from './scheduler-data-components/rate-sheet-change-history/rate-sheet-change-history-listing/rate-sheet-change-history-listing.component';
import { RateSheetChangeHistoryAddEditComponent } from './scheduler-data-components/rate-sheet-change-history/rate-sheet-change-history-add-edit/rate-sheet-change-history-add-edit.component';
import { RateSheetChangeHistoryDetailComponent } from './scheduler-data-components/rate-sheet-change-history/rate-sheet-change-history-detail/rate-sheet-change-history-detail.component';
import { RateSheetChangeHistoryTableComponent } from './scheduler-data-components/rate-sheet-change-history/rate-sheet-change-history-table/rate-sheet-change-history-table.component';
import { RateTypeListingComponent } from './scheduler-data-components/rate-type/rate-type-listing/rate-type-listing.component';
import { RateTypeAddEditComponent } from './scheduler-data-components/rate-type/rate-type-add-edit/rate-type-add-edit.component';
import { RateTypeDetailComponent } from './scheduler-data-components/rate-type/rate-type-detail/rate-type-detail.component';
import { RateTypeTableComponent } from './scheduler-data-components/rate-type/rate-type-table/rate-type-table.component';
import { ReceiptTypeListingComponent } from './scheduler-data-components/receipt-type/receipt-type-listing/receipt-type-listing.component';
import { ReceiptTypeAddEditComponent } from './scheduler-data-components/receipt-type/receipt-type-add-edit/receipt-type-add-edit.component';
import { ReceiptTypeDetailComponent } from './scheduler-data-components/receipt-type/receipt-type-detail/receipt-type-detail.component';
import { ReceiptTypeTableComponent } from './scheduler-data-components/receipt-type/receipt-type-table/receipt-type-table.component';
import { RecurrenceExceptionListingComponent } from './scheduler-data-components/recurrence-exception/recurrence-exception-listing/recurrence-exception-listing.component';
import { RecurrenceExceptionAddEditComponent } from './scheduler-data-components/recurrence-exception/recurrence-exception-add-edit/recurrence-exception-add-edit.component';
import { RecurrenceExceptionDetailComponent } from './scheduler-data-components/recurrence-exception/recurrence-exception-detail/recurrence-exception-detail.component';
import { RecurrenceExceptionTableComponent } from './scheduler-data-components/recurrence-exception/recurrence-exception-table/recurrence-exception-table.component';
import { RecurrenceExceptionChangeHistoryListingComponent } from './scheduler-data-components/recurrence-exception-change-history/recurrence-exception-change-history-listing/recurrence-exception-change-history-listing.component';
import { RecurrenceExceptionChangeHistoryAddEditComponent } from './scheduler-data-components/recurrence-exception-change-history/recurrence-exception-change-history-add-edit/recurrence-exception-change-history-add-edit.component';
import { RecurrenceExceptionChangeHistoryDetailComponent } from './scheduler-data-components/recurrence-exception-change-history/recurrence-exception-change-history-detail/recurrence-exception-change-history-detail.component';
import { RecurrenceExceptionChangeHistoryTableComponent } from './scheduler-data-components/recurrence-exception-change-history/recurrence-exception-change-history-table/recurrence-exception-change-history-table.component';
import { RecurrenceFrequencyListingComponent } from './scheduler-data-components/recurrence-frequency/recurrence-frequency-listing/recurrence-frequency-listing.component';
import { RecurrenceFrequencyAddEditComponent } from './scheduler-data-components/recurrence-frequency/recurrence-frequency-add-edit/recurrence-frequency-add-edit.component';
import { RecurrenceFrequencyDetailComponent } from './scheduler-data-components/recurrence-frequency/recurrence-frequency-detail/recurrence-frequency-detail.component';
import { RecurrenceFrequencyTableComponent } from './scheduler-data-components/recurrence-frequency/recurrence-frequency-table/recurrence-frequency-table.component';
import { RecurrenceRuleListingComponent } from './scheduler-data-components/recurrence-rule/recurrence-rule-listing/recurrence-rule-listing.component';
import { RecurrenceRuleAddEditComponent } from './scheduler-data-components/recurrence-rule/recurrence-rule-add-edit/recurrence-rule-add-edit.component';
import { RecurrenceRuleDetailComponent } from './scheduler-data-components/recurrence-rule/recurrence-rule-detail/recurrence-rule-detail.component';
import { RecurrenceRuleTableComponent } from './scheduler-data-components/recurrence-rule/recurrence-rule-table/recurrence-rule-table.component';
import { RecurrenceRuleChangeHistoryListingComponent } from './scheduler-data-components/recurrence-rule-change-history/recurrence-rule-change-history-listing/recurrence-rule-change-history-listing.component';
import { RecurrenceRuleChangeHistoryAddEditComponent } from './scheduler-data-components/recurrence-rule-change-history/recurrence-rule-change-history-add-edit/recurrence-rule-change-history-add-edit.component';
import { RecurrenceRuleChangeHistoryDetailComponent } from './scheduler-data-components/recurrence-rule-change-history/recurrence-rule-change-history-detail/recurrence-rule-change-history-detail.component';
import { RecurrenceRuleChangeHistoryTableComponent } from './scheduler-data-components/recurrence-rule-change-history/recurrence-rule-change-history-table/recurrence-rule-change-history-table.component';
import { RelationshipTypeListingComponent } from './scheduler-data-components/relationship-type/relationship-type-listing/relationship-type-listing.component';
import { RelationshipTypeAddEditComponent } from './scheduler-data-components/relationship-type/relationship-type-add-edit/relationship-type-add-edit.component';
import { RelationshipTypeDetailComponent } from './scheduler-data-components/relationship-type/relationship-type-detail/relationship-type-detail.component';
import { RelationshipTypeTableComponent } from './scheduler-data-components/relationship-type/relationship-type-table/relationship-type-table.component';
import { ResourceListingComponent } from './scheduler-data-components/resource/resource-listing/resource-listing.component';
import { ResourceAddEditComponent } from './scheduler-data-components/resource/resource-add-edit/resource-add-edit.component';
import { ResourceDetailComponent } from './scheduler-data-components/resource/resource-detail/resource-detail.component';
import { ResourceTableComponent } from './scheduler-data-components/resource/resource-table/resource-table.component';
import { ResourceAvailabilityListingComponent } from './scheduler-data-components/resource-availability/resource-availability-listing/resource-availability-listing.component';
import { ResourceAvailabilityAddEditComponent } from './scheduler-data-components/resource-availability/resource-availability-add-edit/resource-availability-add-edit.component';
import { ResourceAvailabilityDetailComponent } from './scheduler-data-components/resource-availability/resource-availability-detail/resource-availability-detail.component';
import { ResourceAvailabilityTableComponent } from './scheduler-data-components/resource-availability/resource-availability-table/resource-availability-table.component';
import { ResourceAvailabilityChangeHistoryListingComponent } from './scheduler-data-components/resource-availability-change-history/resource-availability-change-history-listing/resource-availability-change-history-listing.component';
import { ResourceAvailabilityChangeHistoryAddEditComponent } from './scheduler-data-components/resource-availability-change-history/resource-availability-change-history-add-edit/resource-availability-change-history-add-edit.component';
import { ResourceAvailabilityChangeHistoryDetailComponent } from './scheduler-data-components/resource-availability-change-history/resource-availability-change-history-detail/resource-availability-change-history-detail.component';
import { ResourceAvailabilityChangeHistoryTableComponent } from './scheduler-data-components/resource-availability-change-history/resource-availability-change-history-table/resource-availability-change-history-table.component';
import { ResourceChangeHistoryListingComponent } from './scheduler-data-components/resource-change-history/resource-change-history-listing/resource-change-history-listing.component';
import { ResourceChangeHistoryAddEditComponent } from './scheduler-data-components/resource-change-history/resource-change-history-add-edit/resource-change-history-add-edit.component';
import { ResourceChangeHistoryDetailComponent } from './scheduler-data-components/resource-change-history/resource-change-history-detail/resource-change-history-detail.component';
import { ResourceChangeHistoryTableComponent } from './scheduler-data-components/resource-change-history/resource-change-history-table/resource-change-history-table.component';
import { ResourceContactListingComponent } from './scheduler-data-components/resource-contact/resource-contact-listing/resource-contact-listing.component';
import { ResourceContactAddEditComponent } from './scheduler-data-components/resource-contact/resource-contact-add-edit/resource-contact-add-edit.component';
import { ResourceContactDetailComponent } from './scheduler-data-components/resource-contact/resource-contact-detail/resource-contact-detail.component';
import { ResourceContactTableComponent } from './scheduler-data-components/resource-contact/resource-contact-table/resource-contact-table.component';
import { ResourceContactChangeHistoryListingComponent } from './scheduler-data-components/resource-contact-change-history/resource-contact-change-history-listing/resource-contact-change-history-listing.component';
import { ResourceContactChangeHistoryAddEditComponent } from './scheduler-data-components/resource-contact-change-history/resource-contact-change-history-add-edit/resource-contact-change-history-add-edit.component';
import { ResourceContactChangeHistoryDetailComponent } from './scheduler-data-components/resource-contact-change-history/resource-contact-change-history-detail/resource-contact-change-history-detail.component';
import { ResourceContactChangeHistoryTableComponent } from './scheduler-data-components/resource-contact-change-history/resource-contact-change-history-table/resource-contact-change-history-table.component';
import { ResourceQualificationListingComponent } from './scheduler-data-components/resource-qualification/resource-qualification-listing/resource-qualification-listing.component';
import { ResourceQualificationAddEditComponent } from './scheduler-data-components/resource-qualification/resource-qualification-add-edit/resource-qualification-add-edit.component';
import { ResourceQualificationDetailComponent } from './scheduler-data-components/resource-qualification/resource-qualification-detail/resource-qualification-detail.component';
import { ResourceQualificationTableComponent } from './scheduler-data-components/resource-qualification/resource-qualification-table/resource-qualification-table.component';
import { ResourceQualificationChangeHistoryListingComponent } from './scheduler-data-components/resource-qualification-change-history/resource-qualification-change-history-listing/resource-qualification-change-history-listing.component';
import { ResourceQualificationChangeHistoryAddEditComponent } from './scheduler-data-components/resource-qualification-change-history/resource-qualification-change-history-add-edit/resource-qualification-change-history-add-edit.component';
import { ResourceQualificationChangeHistoryDetailComponent } from './scheduler-data-components/resource-qualification-change-history/resource-qualification-change-history-detail/resource-qualification-change-history-detail.component';
import { ResourceQualificationChangeHistoryTableComponent } from './scheduler-data-components/resource-qualification-change-history/resource-qualification-change-history-table/resource-qualification-change-history-table.component';
import { ResourceShiftListingComponent } from './scheduler-data-components/resource-shift/resource-shift-listing/resource-shift-listing.component';
import { ResourceShiftAddEditComponent } from './scheduler-data-components/resource-shift/resource-shift-add-edit/resource-shift-add-edit.component';
import { ResourceShiftDetailComponent } from './scheduler-data-components/resource-shift/resource-shift-detail/resource-shift-detail.component';
import { ResourceShiftTableComponent } from './scheduler-data-components/resource-shift/resource-shift-table/resource-shift-table.component';
import { ResourceShiftChangeHistoryListingComponent } from './scheduler-data-components/resource-shift-change-history/resource-shift-change-history-listing/resource-shift-change-history-listing.component';
import { ResourceShiftChangeHistoryAddEditComponent } from './scheduler-data-components/resource-shift-change-history/resource-shift-change-history-add-edit/resource-shift-change-history-add-edit.component';
import { ResourceShiftChangeHistoryDetailComponent } from './scheduler-data-components/resource-shift-change-history/resource-shift-change-history-detail/resource-shift-change-history-detail.component';
import { ResourceShiftChangeHistoryTableComponent } from './scheduler-data-components/resource-shift-change-history/resource-shift-change-history-table/resource-shift-change-history-table.component';
import { ResourceTypeListingComponent } from './scheduler-data-components/resource-type/resource-type-listing/resource-type-listing.component';
import { ResourceTypeAddEditComponent } from './scheduler-data-components/resource-type/resource-type-add-edit/resource-type-add-edit.component';
import { ResourceTypeDetailComponent } from './scheduler-data-components/resource-type/resource-type-detail/resource-type-detail.component';
import { ResourceTypeTableComponent } from './scheduler-data-components/resource-type/resource-type-table/resource-type-table.component';
import { SalutationListingComponent } from './scheduler-data-components/salutation/salutation-listing/salutation-listing.component';
import { SalutationAddEditComponent } from './scheduler-data-components/salutation/salutation-add-edit/salutation-add-edit.component';
import { SalutationDetailComponent } from './scheduler-data-components/salutation/salutation-detail/salutation-detail.component';
import { SalutationTableComponent } from './scheduler-data-components/salutation/salutation-table/salutation-table.component';
import { ScheduledEventListingComponent } from './scheduler-data-components/scheduled-event/scheduled-event-listing/scheduled-event-listing.component';
import { ScheduledEventAddEditComponent } from './scheduler-data-components/scheduled-event/scheduled-event-add-edit/scheduled-event-add-edit.component';
import { ScheduledEventDetailComponent } from './scheduler-data-components/scheduled-event/scheduled-event-detail/scheduled-event-detail.component';
import { ScheduledEventTableComponent } from './scheduler-data-components/scheduled-event/scheduled-event-table/scheduled-event-table.component';
import { ScheduledEventChangeHistoryListingComponent } from './scheduler-data-components/scheduled-event-change-history/scheduled-event-change-history-listing/scheduled-event-change-history-listing.component';
import { ScheduledEventChangeHistoryAddEditComponent } from './scheduler-data-components/scheduled-event-change-history/scheduled-event-change-history-add-edit/scheduled-event-change-history-add-edit.component';
import { ScheduledEventChangeHistoryDetailComponent } from './scheduler-data-components/scheduled-event-change-history/scheduled-event-change-history-detail/scheduled-event-change-history-detail.component';
import { ScheduledEventChangeHistoryTableComponent } from './scheduler-data-components/scheduled-event-change-history/scheduled-event-change-history-table/scheduled-event-change-history-table.component';
import { ScheduledEventDependencyListingComponent } from './scheduler-data-components/scheduled-event-dependency/scheduled-event-dependency-listing/scheduled-event-dependency-listing.component';
import { ScheduledEventDependencyAddEditComponent } from './scheduler-data-components/scheduled-event-dependency/scheduled-event-dependency-add-edit/scheduled-event-dependency-add-edit.component';
import { ScheduledEventDependencyDetailComponent } from './scheduler-data-components/scheduled-event-dependency/scheduled-event-dependency-detail/scheduled-event-dependency-detail.component';
import { ScheduledEventDependencyTableComponent } from './scheduler-data-components/scheduled-event-dependency/scheduled-event-dependency-table/scheduled-event-dependency-table.component';
import { ScheduledEventDependencyChangeHistoryListingComponent } from './scheduler-data-components/scheduled-event-dependency-change-history/scheduled-event-dependency-change-history-listing/scheduled-event-dependency-change-history-listing.component';
import { ScheduledEventDependencyChangeHistoryAddEditComponent } from './scheduler-data-components/scheduled-event-dependency-change-history/scheduled-event-dependency-change-history-add-edit/scheduled-event-dependency-change-history-add-edit.component';
import { ScheduledEventDependencyChangeHistoryDetailComponent } from './scheduler-data-components/scheduled-event-dependency-change-history/scheduled-event-dependency-change-history-detail/scheduled-event-dependency-change-history-detail.component';
import { ScheduledEventDependencyChangeHistoryTableComponent } from './scheduler-data-components/scheduled-event-dependency-change-history/scheduled-event-dependency-change-history-table/scheduled-event-dependency-change-history-table.component';
import { ScheduledEventQualificationRequirementListingComponent } from './scheduler-data-components/scheduled-event-qualification-requirement/scheduled-event-qualification-requirement-listing/scheduled-event-qualification-requirement-listing.component';
import { ScheduledEventQualificationRequirementAddEditComponent } from './scheduler-data-components/scheduled-event-qualification-requirement/scheduled-event-qualification-requirement-add-edit/scheduled-event-qualification-requirement-add-edit.component';
import { ScheduledEventQualificationRequirementDetailComponent } from './scheduler-data-components/scheduled-event-qualification-requirement/scheduled-event-qualification-requirement-detail/scheduled-event-qualification-requirement-detail.component';
import { ScheduledEventQualificationRequirementTableComponent } from './scheduler-data-components/scheduled-event-qualification-requirement/scheduled-event-qualification-requirement-table/scheduled-event-qualification-requirement-table.component';
import { ScheduledEventQualificationRequirementChangeHistoryListingComponent } from './scheduler-data-components/scheduled-event-qualification-requirement-change-history/scheduled-event-qualification-requirement-change-history-listing/scheduled-event-qualification-requirement-change-history-listing.component';
import { ScheduledEventQualificationRequirementChangeHistoryAddEditComponent } from './scheduler-data-components/scheduled-event-qualification-requirement-change-history/scheduled-event-qualification-requirement-change-history-add-edit/scheduled-event-qualification-requirement-change-history-add-edit.component';
import { ScheduledEventQualificationRequirementChangeHistoryDetailComponent } from './scheduler-data-components/scheduled-event-qualification-requirement-change-history/scheduled-event-qualification-requirement-change-history-detail/scheduled-event-qualification-requirement-change-history-detail.component';
import { ScheduledEventQualificationRequirementChangeHistoryTableComponent } from './scheduler-data-components/scheduled-event-qualification-requirement-change-history/scheduled-event-qualification-requirement-change-history-table/scheduled-event-qualification-requirement-change-history-table.component';
import { ScheduledEventTemplateListingComponent } from './scheduler-data-components/scheduled-event-template/scheduled-event-template-listing/scheduled-event-template-listing.component';
import { ScheduledEventTemplateAddEditComponent } from './scheduler-data-components/scheduled-event-template/scheduled-event-template-add-edit/scheduled-event-template-add-edit.component';
import { ScheduledEventTemplateDetailComponent } from './scheduler-data-components/scheduled-event-template/scheduled-event-template-detail/scheduled-event-template-detail.component';
import { ScheduledEventTemplateTableComponent } from './scheduler-data-components/scheduled-event-template/scheduled-event-template-table/scheduled-event-template-table.component';
import { ScheduledEventTemplateChangeHistoryListingComponent } from './scheduler-data-components/scheduled-event-template-change-history/scheduled-event-template-change-history-listing/scheduled-event-template-change-history-listing.component';
import { ScheduledEventTemplateChangeHistoryAddEditComponent } from './scheduler-data-components/scheduled-event-template-change-history/scheduled-event-template-change-history-add-edit/scheduled-event-template-change-history-add-edit.component';
import { ScheduledEventTemplateChangeHistoryDetailComponent } from './scheduler-data-components/scheduled-event-template-change-history/scheduled-event-template-change-history-detail/scheduled-event-template-change-history-detail.component';
import { ScheduledEventTemplateChangeHistoryTableComponent } from './scheduler-data-components/scheduled-event-template-change-history/scheduled-event-template-change-history-table/scheduled-event-template-change-history-table.component';
import { ScheduledEventTemplateChargeListingComponent } from './scheduler-data-components/scheduled-event-template-charge/scheduled-event-template-charge-listing/scheduled-event-template-charge-listing.component';
import { ScheduledEventTemplateChargeAddEditComponent } from './scheduler-data-components/scheduled-event-template-charge/scheduled-event-template-charge-add-edit/scheduled-event-template-charge-add-edit.component';
import { ScheduledEventTemplateChargeDetailComponent } from './scheduler-data-components/scheduled-event-template-charge/scheduled-event-template-charge-detail/scheduled-event-template-charge-detail.component';
import { ScheduledEventTemplateChargeTableComponent } from './scheduler-data-components/scheduled-event-template-charge/scheduled-event-template-charge-table/scheduled-event-template-charge-table.component';
import { ScheduledEventTemplateChargeChangeHistoryListingComponent } from './scheduler-data-components/scheduled-event-template-charge-change-history/scheduled-event-template-charge-change-history-listing/scheduled-event-template-charge-change-history-listing.component';
import { ScheduledEventTemplateChargeChangeHistoryAddEditComponent } from './scheduler-data-components/scheduled-event-template-charge-change-history/scheduled-event-template-charge-change-history-add-edit/scheduled-event-template-charge-change-history-add-edit.component';
import { ScheduledEventTemplateChargeChangeHistoryDetailComponent } from './scheduler-data-components/scheduled-event-template-charge-change-history/scheduled-event-template-charge-change-history-detail/scheduled-event-template-charge-change-history-detail.component';
import { ScheduledEventTemplateChargeChangeHistoryTableComponent } from './scheduler-data-components/scheduled-event-template-charge-change-history/scheduled-event-template-charge-change-history-table/scheduled-event-template-charge-change-history-table.component';
import { ScheduledEventTemplateQualificationRequirementListingComponent } from './scheduler-data-components/scheduled-event-template-qualification-requirement/scheduled-event-template-qualification-requirement-listing/scheduled-event-template-qualification-requirement-listing.component';
import { ScheduledEventTemplateQualificationRequirementAddEditComponent } from './scheduler-data-components/scheduled-event-template-qualification-requirement/scheduled-event-template-qualification-requirement-add-edit/scheduled-event-template-qualification-requirement-add-edit.component';
import { ScheduledEventTemplateQualificationRequirementDetailComponent } from './scheduler-data-components/scheduled-event-template-qualification-requirement/scheduled-event-template-qualification-requirement-detail/scheduled-event-template-qualification-requirement-detail.component';
import { ScheduledEventTemplateQualificationRequirementTableComponent } from './scheduler-data-components/scheduled-event-template-qualification-requirement/scheduled-event-template-qualification-requirement-table/scheduled-event-template-qualification-requirement-table.component';
import { ScheduledEventTemplateQualificationRequirementChangeHistoryListingComponent } from './scheduler-data-components/scheduled-event-template-qualification-requirement-change-history/scheduled-event-template-qualification-requirement-change-history-listing/scheduled-event-template-qualification-requirement-change-history-listing.component';
import { ScheduledEventTemplateQualificationRequirementChangeHistoryAddEditComponent } from './scheduler-data-components/scheduled-event-template-qualification-requirement-change-history/scheduled-event-template-qualification-requirement-change-history-add-edit/scheduled-event-template-qualification-requirement-change-history-add-edit.component';
import { ScheduledEventTemplateQualificationRequirementChangeHistoryDetailComponent } from './scheduler-data-components/scheduled-event-template-qualification-requirement-change-history/scheduled-event-template-qualification-requirement-change-history-detail/scheduled-event-template-qualification-requirement-change-history-detail.component';
import { ScheduledEventTemplateQualificationRequirementChangeHistoryTableComponent } from './scheduler-data-components/scheduled-event-template-qualification-requirement-change-history/scheduled-event-template-qualification-requirement-change-history-table/scheduled-event-template-qualification-requirement-change-history-table.component';
import { SchedulingTargetListingComponent } from './scheduler-data-components/scheduling-target/scheduling-target-listing/scheduling-target-listing.component';
import { SchedulingTargetAddEditComponent } from './scheduler-data-components/scheduling-target/scheduling-target-add-edit/scheduling-target-add-edit.component';
import { SchedulingTargetDetailComponent } from './scheduler-data-components/scheduling-target/scheduling-target-detail/scheduling-target-detail.component';
import { SchedulingTargetTableComponent } from './scheduler-data-components/scheduling-target/scheduling-target-table/scheduling-target-table.component';
import { SchedulingTargetAddressListingComponent } from './scheduler-data-components/scheduling-target-address/scheduling-target-address-listing/scheduling-target-address-listing.component';
import { SchedulingTargetAddressAddEditComponent } from './scheduler-data-components/scheduling-target-address/scheduling-target-address-add-edit/scheduling-target-address-add-edit.component';
import { SchedulingTargetAddressDetailComponent } from './scheduler-data-components/scheduling-target-address/scheduling-target-address-detail/scheduling-target-address-detail.component';
import { SchedulingTargetAddressTableComponent } from './scheduler-data-components/scheduling-target-address/scheduling-target-address-table/scheduling-target-address-table.component';
import { SchedulingTargetAddressChangeHistoryListingComponent } from './scheduler-data-components/scheduling-target-address-change-history/scheduling-target-address-change-history-listing/scheduling-target-address-change-history-listing.component';
import { SchedulingTargetAddressChangeHistoryAddEditComponent } from './scheduler-data-components/scheduling-target-address-change-history/scheduling-target-address-change-history-add-edit/scheduling-target-address-change-history-add-edit.component';
import { SchedulingTargetAddressChangeHistoryDetailComponent } from './scheduler-data-components/scheduling-target-address-change-history/scheduling-target-address-change-history-detail/scheduling-target-address-change-history-detail.component';
import { SchedulingTargetAddressChangeHistoryTableComponent } from './scheduler-data-components/scheduling-target-address-change-history/scheduling-target-address-change-history-table/scheduling-target-address-change-history-table.component';
import { SchedulingTargetChangeHistoryListingComponent } from './scheduler-data-components/scheduling-target-change-history/scheduling-target-change-history-listing/scheduling-target-change-history-listing.component';
import { SchedulingTargetChangeHistoryAddEditComponent } from './scheduler-data-components/scheduling-target-change-history/scheduling-target-change-history-add-edit/scheduling-target-change-history-add-edit.component';
import { SchedulingTargetChangeHistoryDetailComponent } from './scheduler-data-components/scheduling-target-change-history/scheduling-target-change-history-detail/scheduling-target-change-history-detail.component';
import { SchedulingTargetChangeHistoryTableComponent } from './scheduler-data-components/scheduling-target-change-history/scheduling-target-change-history-table/scheduling-target-change-history-table.component';
import { SchedulingTargetContactListingComponent } from './scheduler-data-components/scheduling-target-contact/scheduling-target-contact-listing/scheduling-target-contact-listing.component';
import { SchedulingTargetContactAddEditComponent } from './scheduler-data-components/scheduling-target-contact/scheduling-target-contact-add-edit/scheduling-target-contact-add-edit.component';
import { SchedulingTargetContactDetailComponent } from './scheduler-data-components/scheduling-target-contact/scheduling-target-contact-detail/scheduling-target-contact-detail.component';
import { SchedulingTargetContactTableComponent } from './scheduler-data-components/scheduling-target-contact/scheduling-target-contact-table/scheduling-target-contact-table.component';
import { SchedulingTargetContactChangeHistoryListingComponent } from './scheduler-data-components/scheduling-target-contact-change-history/scheduling-target-contact-change-history-listing/scheduling-target-contact-change-history-listing.component';
import { SchedulingTargetContactChangeHistoryAddEditComponent } from './scheduler-data-components/scheduling-target-contact-change-history/scheduling-target-contact-change-history-add-edit/scheduling-target-contact-change-history-add-edit.component';
import { SchedulingTargetContactChangeHistoryDetailComponent } from './scheduler-data-components/scheduling-target-contact-change-history/scheduling-target-contact-change-history-detail/scheduling-target-contact-change-history-detail.component';
import { SchedulingTargetContactChangeHistoryTableComponent } from './scheduler-data-components/scheduling-target-contact-change-history/scheduling-target-contact-change-history-table/scheduling-target-contact-change-history-table.component';
import { SchedulingTargetQualificationRequirementListingComponent } from './scheduler-data-components/scheduling-target-qualification-requirement/scheduling-target-qualification-requirement-listing/scheduling-target-qualification-requirement-listing.component';
import { SchedulingTargetQualificationRequirementAddEditComponent } from './scheduler-data-components/scheduling-target-qualification-requirement/scheduling-target-qualification-requirement-add-edit/scheduling-target-qualification-requirement-add-edit.component';
import { SchedulingTargetQualificationRequirementDetailComponent } from './scheduler-data-components/scheduling-target-qualification-requirement/scheduling-target-qualification-requirement-detail/scheduling-target-qualification-requirement-detail.component';
import { SchedulingTargetQualificationRequirementTableComponent } from './scheduler-data-components/scheduling-target-qualification-requirement/scheduling-target-qualification-requirement-table/scheduling-target-qualification-requirement-table.component';
import { SchedulingTargetQualificationRequirementChangeHistoryListingComponent } from './scheduler-data-components/scheduling-target-qualification-requirement-change-history/scheduling-target-qualification-requirement-change-history-listing/scheduling-target-qualification-requirement-change-history-listing.component';
import { SchedulingTargetQualificationRequirementChangeHistoryAddEditComponent } from './scheduler-data-components/scheduling-target-qualification-requirement-change-history/scheduling-target-qualification-requirement-change-history-add-edit/scheduling-target-qualification-requirement-change-history-add-edit.component';
import { SchedulingTargetQualificationRequirementChangeHistoryDetailComponent } from './scheduler-data-components/scheduling-target-qualification-requirement-change-history/scheduling-target-qualification-requirement-change-history-detail/scheduling-target-qualification-requirement-change-history-detail.component';
import { SchedulingTargetQualificationRequirementChangeHistoryTableComponent } from './scheduler-data-components/scheduling-target-qualification-requirement-change-history/scheduling-target-qualification-requirement-change-history-table/scheduling-target-qualification-requirement-change-history-table.component';
import { SchedulingTargetTypeListingComponent } from './scheduler-data-components/scheduling-target-type/scheduling-target-type-listing/scheduling-target-type-listing.component';
import { SchedulingTargetTypeAddEditComponent } from './scheduler-data-components/scheduling-target-type/scheduling-target-type-add-edit/scheduling-target-type-add-edit.component';
import { SchedulingTargetTypeDetailComponent } from './scheduler-data-components/scheduling-target-type/scheduling-target-type-detail/scheduling-target-type-detail.component';
import { SchedulingTargetTypeTableComponent } from './scheduler-data-components/scheduling-target-type/scheduling-target-type-table/scheduling-target-type-table.component';
import { ShiftPatternListingComponent } from './scheduler-data-components/shift-pattern/shift-pattern-listing/shift-pattern-listing.component';
import { ShiftPatternAddEditComponent } from './scheduler-data-components/shift-pattern/shift-pattern-add-edit/shift-pattern-add-edit.component';
import { ShiftPatternDetailComponent } from './scheduler-data-components/shift-pattern/shift-pattern-detail/shift-pattern-detail.component';
import { ShiftPatternTableComponent } from './scheduler-data-components/shift-pattern/shift-pattern-table/shift-pattern-table.component';
import { ShiftPatternChangeHistoryListingComponent } from './scheduler-data-components/shift-pattern-change-history/shift-pattern-change-history-listing/shift-pattern-change-history-listing.component';
import { ShiftPatternChangeHistoryAddEditComponent } from './scheduler-data-components/shift-pattern-change-history/shift-pattern-change-history-add-edit/shift-pattern-change-history-add-edit.component';
import { ShiftPatternChangeHistoryDetailComponent } from './scheduler-data-components/shift-pattern-change-history/shift-pattern-change-history-detail/shift-pattern-change-history-detail.component';
import { ShiftPatternChangeHistoryTableComponent } from './scheduler-data-components/shift-pattern-change-history/shift-pattern-change-history-table/shift-pattern-change-history-table.component';
import { ShiftPatternDayListingComponent } from './scheduler-data-components/shift-pattern-day/shift-pattern-day-listing/shift-pattern-day-listing.component';
import { ShiftPatternDayAddEditComponent } from './scheduler-data-components/shift-pattern-day/shift-pattern-day-add-edit/shift-pattern-day-add-edit.component';
import { ShiftPatternDayDetailComponent } from './scheduler-data-components/shift-pattern-day/shift-pattern-day-detail/shift-pattern-day-detail.component';
import { ShiftPatternDayTableComponent } from './scheduler-data-components/shift-pattern-day/shift-pattern-day-table/shift-pattern-day-table.component';
import { ShiftPatternDayChangeHistoryListingComponent } from './scheduler-data-components/shift-pattern-day-change-history/shift-pattern-day-change-history-listing/shift-pattern-day-change-history-listing.component';
import { ShiftPatternDayChangeHistoryAddEditComponent } from './scheduler-data-components/shift-pattern-day-change-history/shift-pattern-day-change-history-add-edit/shift-pattern-day-change-history-add-edit.component';
import { ShiftPatternDayChangeHistoryDetailComponent } from './scheduler-data-components/shift-pattern-day-change-history/shift-pattern-day-change-history-detail/shift-pattern-day-change-history-detail.component';
import { ShiftPatternDayChangeHistoryTableComponent } from './scheduler-data-components/shift-pattern-day-change-history/shift-pattern-day-change-history-table/shift-pattern-day-change-history-table.component';
import { SoftCreditListingComponent } from './scheduler-data-components/soft-credit/soft-credit-listing/soft-credit-listing.component';
import { SoftCreditAddEditComponent } from './scheduler-data-components/soft-credit/soft-credit-add-edit/soft-credit-add-edit.component';
import { SoftCreditDetailComponent } from './scheduler-data-components/soft-credit/soft-credit-detail/soft-credit-detail.component';
import { SoftCreditTableComponent } from './scheduler-data-components/soft-credit/soft-credit-table/soft-credit-table.component';
import { SoftCreditChangeHistoryListingComponent } from './scheduler-data-components/soft-credit-change-history/soft-credit-change-history-listing/soft-credit-change-history-listing.component';
import { SoftCreditChangeHistoryAddEditComponent } from './scheduler-data-components/soft-credit-change-history/soft-credit-change-history-add-edit/soft-credit-change-history-add-edit.component';
import { SoftCreditChangeHistoryDetailComponent } from './scheduler-data-components/soft-credit-change-history/soft-credit-change-history-detail/soft-credit-change-history-detail.component';
import { SoftCreditChangeHistoryTableComponent } from './scheduler-data-components/soft-credit-change-history/soft-credit-change-history-table/soft-credit-change-history-table.component';
import { StateProvinceListingComponent } from './scheduler-data-components/state-province/state-province-listing/state-province-listing.component';
import { StateProvinceAddEditComponent } from './scheduler-data-components/state-province/state-province-add-edit/state-province-add-edit.component';
import { StateProvinceDetailComponent } from './scheduler-data-components/state-province/state-province-detail/state-province-detail.component';
import { StateProvinceTableComponent } from './scheduler-data-components/state-province/state-province-table/state-province-table.component';
import { TagListingComponent } from './scheduler-data-components/tag/tag-listing/tag-listing.component';
import { TagAddEditComponent } from './scheduler-data-components/tag/tag-add-edit/tag-add-edit.component';
import { TagDetailComponent } from './scheduler-data-components/tag/tag-detail/tag-detail.component';
import { TagTableComponent } from './scheduler-data-components/tag/tag-table/tag-table.component';
import { TenantProfileListingComponent } from './scheduler-data-components/tenant-profile/tenant-profile-listing/tenant-profile-listing.component';
import { TenantProfileAddEditComponent } from './scheduler-data-components/tenant-profile/tenant-profile-add-edit/tenant-profile-add-edit.component';
import { TenantProfileDetailComponent } from './scheduler-data-components/tenant-profile/tenant-profile-detail/tenant-profile-detail.component';
import { TenantProfileTableComponent } from './scheduler-data-components/tenant-profile/tenant-profile-table/tenant-profile-table.component';
import { TenantProfileChangeHistoryListingComponent } from './scheduler-data-components/tenant-profile-change-history/tenant-profile-change-history-listing/tenant-profile-change-history-listing.component';
import { TenantProfileChangeHistoryAddEditComponent } from './scheduler-data-components/tenant-profile-change-history/tenant-profile-change-history-add-edit/tenant-profile-change-history-add-edit.component';
import { TenantProfileChangeHistoryDetailComponent } from './scheduler-data-components/tenant-profile-change-history/tenant-profile-change-history-detail/tenant-profile-change-history-detail.component';
import { TenantProfileChangeHistoryTableComponent } from './scheduler-data-components/tenant-profile-change-history/tenant-profile-change-history-table/tenant-profile-change-history-table.component';
import { TimeZoneListingComponent } from './scheduler-data-components/time-zone/time-zone-listing/time-zone-listing.component';
import { TimeZoneAddEditComponent } from './scheduler-data-components/time-zone/time-zone-add-edit/time-zone-add-edit.component';
import { TimeZoneDetailComponent } from './scheduler-data-components/time-zone/time-zone-detail/time-zone-detail.component';
import { TimeZoneTableComponent } from './scheduler-data-components/time-zone/time-zone-table/time-zone-table.component';
import { TributeListingComponent } from './scheduler-data-components/tribute/tribute-listing/tribute-listing.component';
import { TributeAddEditComponent } from './scheduler-data-components/tribute/tribute-add-edit/tribute-add-edit.component';
import { TributeDetailComponent } from './scheduler-data-components/tribute/tribute-detail/tribute-detail.component';
import { TributeTableComponent } from './scheduler-data-components/tribute/tribute-table/tribute-table.component';
import { TributeChangeHistoryListingComponent } from './scheduler-data-components/tribute-change-history/tribute-change-history-listing/tribute-change-history-listing.component';
import { TributeChangeHistoryAddEditComponent } from './scheduler-data-components/tribute-change-history/tribute-change-history-add-edit/tribute-change-history-add-edit.component';
import { TributeChangeHistoryDetailComponent } from './scheduler-data-components/tribute-change-history/tribute-change-history-detail/tribute-change-history-detail.component';
import { TributeChangeHistoryTableComponent } from './scheduler-data-components/tribute-change-history/tribute-change-history-table/tribute-change-history-table.component';
import { TributeTypeListingComponent } from './scheduler-data-components/tribute-type/tribute-type-listing/tribute-type-listing.component';
import { TributeTypeAddEditComponent } from './scheduler-data-components/tribute-type/tribute-type-add-edit/tribute-type-add-edit.component';
import { TributeTypeDetailComponent } from './scheduler-data-components/tribute-type/tribute-type-detail/tribute-type-detail.component';
import { TributeTypeTableComponent } from './scheduler-data-components/tribute-type/tribute-type-table/tribute-type-table.component';
//
// End of imports for Scheduler Data Components
//


@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    AuthCallbackComponent,
    NotFoundComponent,
    SearchBoxComponent,
    BooleanIconComponent,
    DynamicFieldRendererComponent,

    EqualValidator,
    AutofocusDirective,
    BootstrapTabDirective,
    SpinnerDirective,

    GroupByPipe,
    FilterAndJoinPipe,
    BigNumberFormatPipe,
    ContrastColorPipe,
    ContactFullNamePipe,

    OverviewComponent,
    ModalComponent,

    HeaderComponent,
    SidebarComponent,


    //
    // Custom components
    //
    SchedulerCalendarComponent,
    EventAddEditModalComponent,
    AdministrationComponent,
    AddTenantProfileComponent,
    RateSheetCustomListingComponent,
    RateSheetCustomTableComponent,
    RateSheetCustomAddEditComponent,

    //
    // Resource customization - Replaced all with new
    //
    ResourceCustomListingComponent,
    ResourceCustomDetailComponent,
    ResourceCustomAddEditComponent,
    ResourceCustomTableComponent,
    ResourceOverviewTabComponent,
    ResourceQualificationsTabComponent,
    ResourceQualificationCustomAddModalComponent,
    ResourceCrewsTabComponent,
    ResourceAddToCrewModalComponent,
    ResourceAvailabilityTabComponent,
    ResourceAvailabilityAddModalComponent,
    ResourceRatesTabComponent,
    ResourceRateOverrideAddModalComponent,
    ResourceAssignmentsTabComponent,
    ResourceContactsTabComponent,
    ResourceContactCustomAddEditModalComponent,
    ResourceNotificationsTabComponent,
    NotificationSubscriptionCustomAddEditModalComponent,


    //
    // Crew customization
    //
    CrewCustomListingComponent,
    CrewCustomTableComponent,
    CrewCustomDetailComponent,
    CrewCustomAddEditComponent,
    CrewOverviewTabComponent,
    CrewAssignmentsTabComponent,
    CrewMembersTabComponent,
    CrewAddToCrewModalComponent,


    //
    // Office customization - Replaced all with new
    //
    OfficeCustomListingComponent,
    OfficeCustomDetailComponent,
    OfficeCustomAddEditComponent,
    OfficeCustomTableComponent,
    OfficeOverviewTabComponent,
    OfficeCrewsTabComponent,
    OfficeContactsTabComponent,
    OfficeContactCustomAddEditModalComponent,
    OfficeRatesTabComponent,
    OfficeRateOverrideAddModalComponent,
    OfficeAssignmentsTabComponent,
    OfficeResourcesTabComponent,
    OfficeCalendarsTabComponent,

    //
    // Calendar customization
    //
    CalendarCustomAddEditComponent,
    CalendarCustomDetailComponent,
    CalendarCustomListingComponent,
    CalendarCustomTableComponent,
    CalendarOverviewTabComponent,
    CalendarAssignmentsTabComponent,


    //
    // Client customizations
    //
    ClientCustomAddEditComponent,
    ClientCustomDetailComponent,
    ClientCustomListingComponent,
    ClientCustomTableComponent,
    ClientOverviewTabComponent,
    ClientAssignmentsTabComponent,
    ClientContactsTabComponent,
    ClientContactCustomAddEditModalComponent,
    ClientTargetsTabComponent,

    //
    // Contact customization - Replaced all with new
    //
    ContactCustomListingComponent,
    ContactCustomDetailComponent,
    ContactCustomAddEditComponent,
    ContactCustomTableComponent,
    ContactOverviewTabComponent,
    ContactInteractionsTabComponent,
    ContactInteractionEditModalComponent,
    ContactRelationshipsTabComponent,
    ContactFinancialsTabComponent,
    ContactScheduleTabComponent,
    ConstituentJourneyUpdateModalComponent,


    //
    // Security Data Components - Auto generated
    //
    EntityDataTokenListingComponent,
    EntityDataTokenAddEditComponent,
    EntityDataTokenDetailComponent,
    EntityDataTokenTableComponent,
    EntityDataTokenEventListingComponent,
    EntityDataTokenEventAddEditComponent,
    EntityDataTokenEventDetailComponent,
    EntityDataTokenEventTableComponent,
    EntityDataTokenEventTypeListingComponent,
    EntityDataTokenEventTypeAddEditComponent,
    EntityDataTokenEventTypeDetailComponent,
    EntityDataTokenEventTypeTableComponent,
    LoginAttemptListingComponent,
    LoginAttemptAddEditComponent,
    LoginAttemptDetailComponent,
    LoginAttemptTableComponent,
    ModuleListingComponent,
    ModuleAddEditComponent,
    ModuleDetailComponent,
    ModuleTableComponent,
    ModuleSecurityRoleListingComponent,
    ModuleSecurityRoleAddEditComponent,
    ModuleSecurityRoleDetailComponent,
    ModuleSecurityRoleTableComponent,
    OAUTHTokenListingComponent,
    OAUTHTokenAddEditComponent,
    OAUTHTokenDetailComponent,
    OAUTHTokenTableComponent,
    PrivilegeListingComponent,
    PrivilegeAddEditComponent,
    PrivilegeDetailComponent,
    PrivilegeTableComponent,
    SecurityDepartmentListingComponent,
    SecurityDepartmentAddEditComponent,
    SecurityDepartmentDetailComponent,
    SecurityDepartmentTableComponent,
    SecurityDepartmentUserListingComponent,
    SecurityDepartmentUserAddEditComponent,
    SecurityDepartmentUserDetailComponent,
    SecurityDepartmentUserTableComponent,
    SecurityGroupListingComponent,
    SecurityGroupAddEditComponent,
    SecurityGroupDetailComponent,
    SecurityGroupTableComponent,
    SecurityGroupSecurityRoleListingComponent,
    SecurityGroupSecurityRoleAddEditComponent,
    SecurityGroupSecurityRoleDetailComponent,
    SecurityGroupSecurityRoleTableComponent,
    SecurityOrganizationListingComponent,
    SecurityOrganizationAddEditComponent,
    SecurityOrganizationDetailComponent,
    SecurityOrganizationTableComponent,
    SecurityOrganizationUserListingComponent,
    SecurityOrganizationUserAddEditComponent,
    SecurityOrganizationUserDetailComponent,
    SecurityOrganizationUserTableComponent,
    SecurityRoleListingComponent,
    SecurityRoleAddEditComponent,
    SecurityRoleDetailComponent,
    SecurityRoleTableComponent,
    SecurityTeamListingComponent,
    SecurityTeamAddEditComponent,
    SecurityTeamDetailComponent,
    SecurityTeamTableComponent,
    SecurityTeamUserListingComponent,
    SecurityTeamUserAddEditComponent,
    SecurityTeamUserDetailComponent,
    SecurityTeamUserTableComponent,
    SecurityTenantListingComponent,
    SecurityTenantAddEditComponent,
    SecurityTenantDetailComponent,
    SecurityTenantTableComponent,
    SecurityTenantUserListingComponent,
    SecurityTenantUserAddEditComponent,
    SecurityTenantUserDetailComponent,
    SecurityTenantUserTableComponent,
    SecurityUserListingComponent,
    SecurityUserAddEditComponent,
    SecurityUserDetailComponent,
    SecurityUserTableComponent,
    SecurityUserEventListingComponent,
    SecurityUserEventAddEditComponent,
    SecurityUserEventDetailComponent,
    SecurityUserEventTableComponent,
    SecurityUserEventTypeListingComponent,
    SecurityUserEventTypeAddEditComponent,
    SecurityUserEventTypeDetailComponent,
    SecurityUserEventTypeTableComponent,
    SecurityUserPasswordResetTokenListingComponent,
    SecurityUserPasswordResetTokenAddEditComponent,
    SecurityUserPasswordResetTokenDetailComponent,
    SecurityUserPasswordResetTokenTableComponent,
    SecurityUserSecurityGroupListingComponent,
    SecurityUserSecurityGroupAddEditComponent,
    SecurityUserSecurityGroupDetailComponent,
    SecurityUserSecurityGroupTableComponent,
    SecurityUserSecurityRoleListingComponent,
    SecurityUserSecurityRoleAddEditComponent,
    SecurityUserSecurityRoleDetailComponent,
    SecurityUserSecurityRoleTableComponent,
    SecurityUserTitleListingComponent,
    SecurityUserTitleAddEditComponent,
    SecurityUserTitleDetailComponent,
    SecurityUserTitleTableComponent,
    SystemSettingListingComponent,
    SystemSettingAddEditComponent,
    SystemSettingDetailComponent,
    SystemSettingTableComponent,
    //
    // End Security Data Components
    //


    //
    // Auditor Data Components - Auto generated
    //
    AuditAccessTypeListingComponent,
    AuditAccessTypeAddEditComponent,
    AuditAccessTypeDetailComponent,
    AuditAccessTypeTableComponent,
    AuditEventListingComponent,
    AuditEventAddEditComponent,
    AuditEventDetailComponent,
    AuditEventTableComponent,
    AuditEventEntityStateListingComponent,
    AuditEventEntityStateAddEditComponent,
    AuditEventEntityStateDetailComponent,
    AuditEventEntityStateTableComponent,
    AuditEventErrorMessageListingComponent,
    AuditEventErrorMessageAddEditComponent,
    AuditEventErrorMessageDetailComponent,
    AuditEventErrorMessageTableComponent,
    AuditHostSystemListingComponent,
    AuditHostSystemAddEditComponent,
    AuditHostSystemDetailComponent,
    AuditHostSystemTableComponent,
    AuditModuleListingComponent,
    AuditModuleAddEditComponent,
    AuditModuleDetailComponent,
    AuditModuleTableComponent,
    AuditModuleEntityListingComponent,
    AuditModuleEntityAddEditComponent,
    AuditModuleEntityDetailComponent,
    AuditModuleEntityTableComponent,
    AuditPlanBListingComponent,
    AuditPlanBAddEditComponent,
    AuditPlanBDetailComponent,
    AuditPlanBTableComponent,
    AuditResourceListingComponent,
    AuditResourceAddEditComponent,
    AuditResourceDetailComponent,
    AuditResourceTableComponent,
    AuditSessionListingComponent,
    AuditSessionAddEditComponent,
    AuditSessionDetailComponent,
    AuditSessionTableComponent,
    AuditSourceListingComponent,
    AuditSourceAddEditComponent,
    AuditSourceDetailComponent,
    AuditSourceTableComponent,
    AuditTypeListingComponent,
    AuditTypeAddEditComponent,
    AuditTypeDetailComponent,
    AuditTypeTableComponent,
    AuditUserListingComponent,
    AuditUserAddEditComponent,
    AuditUserDetailComponent,
    AuditUserTableComponent,
    AuditUserAgentListingComponent,
    AuditUserAgentAddEditComponent,
    AuditUserAgentDetailComponent,
    AuditUserAgentTableComponent,
    ExternalCommunicationListingComponent,
    ExternalCommunicationAddEditComponent,
    ExternalCommunicationDetailComponent,
    ExternalCommunicationTableComponent,
    ExternalCommunicationRecipientListingComponent,
    ExternalCommunicationRecipientAddEditComponent,
    ExternalCommunicationRecipientDetailComponent,
    ExternalCommunicationRecipientTableComponent,
    //
    // End Auditor Data Components
    //


    //
    // Beginning of declarations for Scheduler Data Components 
    //
    AppealListingComponent,
    AppealAddEditComponent,
    AppealDetailComponent,
    AppealTableComponent,
    AppealChangeHistoryListingComponent,
    AppealChangeHistoryAddEditComponent,
    AppealChangeHistoryDetailComponent,
    AppealChangeHistoryTableComponent,
    AssignmentRoleListingComponent,
    AssignmentRoleAddEditComponent,
    AssignmentRoleDetailComponent,
    AssignmentRoleTableComponent,
    AssignmentRoleQualificationRequirementListingComponent,
    AssignmentRoleQualificationRequirementAddEditComponent,
    AssignmentRoleQualificationRequirementDetailComponent,
    AssignmentRoleQualificationRequirementTableComponent,
    AssignmentRoleQualificationRequirementChangeHistoryListingComponent,
    AssignmentRoleQualificationRequirementChangeHistoryAddEditComponent,
    AssignmentRoleQualificationRequirementChangeHistoryDetailComponent,
    AssignmentRoleQualificationRequirementChangeHistoryTableComponent,
    AssignmentStatusListingComponent,
    AssignmentStatusAddEditComponent,
    AssignmentStatusDetailComponent,
    AssignmentStatusTableComponent,
    AttributeDefinitionListingComponent,
    AttributeDefinitionAddEditComponent,
    AttributeDefinitionDetailComponent,
    AttributeDefinitionTableComponent,
    BatchListingComponent,
    BatchAddEditComponent,
    BatchDetailComponent,
    BatchTableComponent,
    BatchChangeHistoryListingComponent,
    BatchChangeHistoryAddEditComponent,
    BatchChangeHistoryDetailComponent,
    BatchChangeHistoryTableComponent,
    BatchStatusListingComponent,
    BatchStatusAddEditComponent,
    BatchStatusDetailComponent,
    BatchStatusTableComponent,
    BookingSourceTypeListingComponent,
    BookingSourceTypeAddEditComponent,
    BookingSourceTypeDetailComponent,
    BookingSourceTypeTableComponent,
    CalendarListingComponent,
    CalendarAddEditComponent,
    CalendarDetailComponent,
    CalendarTableComponent,
    CalendarChangeHistoryListingComponent,
    CalendarChangeHistoryAddEditComponent,
    CalendarChangeHistoryDetailComponent,
    CalendarChangeHistoryTableComponent,
    CampaignListingComponent,
    CampaignAddEditComponent,
    CampaignDetailComponent,
    CampaignTableComponent,
    CampaignChangeHistoryListingComponent,
    CampaignChangeHistoryAddEditComponent,
    CampaignChangeHistoryDetailComponent,
    CampaignChangeHistoryTableComponent,
    ChargeStatusListingComponent,
    ChargeStatusAddEditComponent,
    ChargeStatusDetailComponent,
    ChargeStatusTableComponent,
    ChargeTypeListingComponent,
    ChargeTypeAddEditComponent,
    ChargeTypeDetailComponent,
    ChargeTypeTableComponent,
    ChargeTypeChangeHistoryListingComponent,
    ChargeTypeChangeHistoryAddEditComponent,
    ChargeTypeChangeHistoryDetailComponent,
    ChargeTypeChangeHistoryTableComponent,
    ClientListingComponent,
    ClientAddEditComponent,
    ClientDetailComponent,
    ClientTableComponent,
    ClientChangeHistoryListingComponent,
    ClientChangeHistoryAddEditComponent,
    ClientChangeHistoryDetailComponent,
    ClientChangeHistoryTableComponent,
    ClientContactListingComponent,
    ClientContactAddEditComponent,
    ClientContactDetailComponent,
    ClientContactTableComponent,
    ClientContactChangeHistoryListingComponent,
    ClientContactChangeHistoryAddEditComponent,
    ClientContactChangeHistoryDetailComponent,
    ClientContactChangeHistoryTableComponent,
    ClientTypeListingComponent,
    ClientTypeAddEditComponent,
    ClientTypeDetailComponent,
    ClientTypeTableComponent,
    ConstituentListingComponent,
    ConstituentAddEditComponent,
    ConstituentDetailComponent,
    ConstituentTableComponent,
    ConstituentChangeHistoryListingComponent,
    ConstituentChangeHistoryAddEditComponent,
    ConstituentChangeHistoryDetailComponent,
    ConstituentChangeHistoryTableComponent,
    ConstituentJourneyStageListingComponent,
    ConstituentJourneyStageAddEditComponent,
    ConstituentJourneyStageDetailComponent,
    ConstituentJourneyStageTableComponent,
    ConstituentJourneyStageChangeHistoryListingComponent,
    ConstituentJourneyStageChangeHistoryAddEditComponent,
    ConstituentJourneyStageChangeHistoryDetailComponent,
    ConstituentJourneyStageChangeHistoryTableComponent,
    ContactListingComponent,
    ContactAddEditComponent,
    ContactDetailComponent,
    ContactTableComponent,
    ContactChangeHistoryListingComponent,
    ContactChangeHistoryAddEditComponent,
    ContactChangeHistoryDetailComponent,
    ContactChangeHistoryTableComponent,
    ContactContactListingComponent,
    ContactContactAddEditComponent,
    ContactContactDetailComponent,
    ContactContactTableComponent,
    ContactContactChangeHistoryListingComponent,
    ContactContactChangeHistoryAddEditComponent,
    ContactContactChangeHistoryDetailComponent,
    ContactContactChangeHistoryTableComponent,
    ContactInteractionListingComponent,
    ContactInteractionAddEditComponent,
    ContactInteractionDetailComponent,
    ContactInteractionTableComponent,
    ContactInteractionChangeHistoryListingComponent,
    ContactInteractionChangeHistoryAddEditComponent,
    ContactInteractionChangeHistoryDetailComponent,
    ContactInteractionChangeHistoryTableComponent,
    ContactMethodListingComponent,
    ContactMethodAddEditComponent,
    ContactMethodDetailComponent,
    ContactMethodTableComponent,
    ContactTagListingComponent,
    ContactTagAddEditComponent,
    ContactTagDetailComponent,
    ContactTagTableComponent,
    ContactTagChangeHistoryListingComponent,
    ContactTagChangeHistoryAddEditComponent,
    ContactTagChangeHistoryDetailComponent,
    ContactTagChangeHistoryTableComponent,
    ContactTypeListingComponent,
    ContactTypeAddEditComponent,
    ContactTypeDetailComponent,
    ContactTypeTableComponent,
    CountryListingComponent,
    CountryAddEditComponent,
    CountryDetailComponent,
    CountryTableComponent,
    CrewListingComponent,
    CrewAddEditComponent,
    CrewDetailComponent,
    CrewTableComponent,
    CrewChangeHistoryListingComponent,
    CrewChangeHistoryAddEditComponent,
    CrewChangeHistoryDetailComponent,
    CrewChangeHistoryTableComponent,
    CrewMemberListingComponent,
    CrewMemberAddEditComponent,
    CrewMemberDetailComponent,
    CrewMemberTableComponent,
    CrewMemberChangeHistoryListingComponent,
    CrewMemberChangeHistoryAddEditComponent,
    CrewMemberChangeHistoryDetailComponent,
    CrewMemberChangeHistoryTableComponent,
    CurrencyListingComponent,
    CurrencyAddEditComponent,
    CurrencyDetailComponent,
    CurrencyTableComponent,
    DependencyTypeListingComponent,
    DependencyTypeAddEditComponent,
    DependencyTypeDetailComponent,
    DependencyTypeTableComponent,
    EventCalendarListingComponent,
    EventCalendarAddEditComponent,
    EventCalendarDetailComponent,
    EventCalendarTableComponent,
    EventChargeListingComponent,
    EventChargeAddEditComponent,
    EventChargeDetailComponent,
    EventChargeTableComponent,
    EventChargeChangeHistoryListingComponent,
    EventChargeChangeHistoryAddEditComponent,
    EventChargeChangeHistoryDetailComponent,
    EventChargeChangeHistoryTableComponent,
    EventResourceAssignmentListingComponent,
    EventResourceAssignmentAddEditComponent,
    EventResourceAssignmentDetailComponent,
    EventResourceAssignmentTableComponent,
    EventResourceAssignmentChangeHistoryListingComponent,
    EventResourceAssignmentChangeHistoryAddEditComponent,
    EventResourceAssignmentChangeHistoryDetailComponent,
    EventResourceAssignmentChangeHistoryTableComponent,
    EventStatusListingComponent,
    EventStatusAddEditComponent,
    EventStatusDetailComponent,
    EventStatusTableComponent,
    FundListingComponent,
    FundAddEditComponent,
    FundDetailComponent,
    FundTableComponent,
    FundChangeHistoryListingComponent,
    FundChangeHistoryAddEditComponent,
    FundChangeHistoryDetailComponent,
    FundChangeHistoryTableComponent,
    GiftListingComponent,
    GiftAddEditComponent,
    GiftDetailComponent,
    GiftTableComponent,
    GiftChangeHistoryListingComponent,
    GiftChangeHistoryAddEditComponent,
    GiftChangeHistoryDetailComponent,
    GiftChangeHistoryTableComponent,
    HouseholdListingComponent,
    HouseholdAddEditComponent,
    HouseholdDetailComponent,
    HouseholdTableComponent,
    HouseholdChangeHistoryListingComponent,
    HouseholdChangeHistoryAddEditComponent,
    HouseholdChangeHistoryDetailComponent,
    HouseholdChangeHistoryTableComponent,
    IconListingComponent,
    IconAddEditComponent,
    IconDetailComponent,
    IconTableComponent,
    InteractionTypeListingComponent,
    InteractionTypeAddEditComponent,
    InteractionTypeDetailComponent,
    InteractionTypeTableComponent,
    NotificationSubscriptionListingComponent,
    NotificationSubscriptionAddEditComponent,
    NotificationSubscriptionDetailComponent,
    NotificationSubscriptionTableComponent,
    NotificationSubscriptionChangeHistoryListingComponent,
    NotificationSubscriptionChangeHistoryAddEditComponent,
    NotificationSubscriptionChangeHistoryDetailComponent,
    NotificationSubscriptionChangeHistoryTableComponent,
    NotificationTypeListingComponent,
    NotificationTypeAddEditComponent,
    NotificationTypeDetailComponent,
    NotificationTypeTableComponent,
    OfficeListingComponent,
    OfficeAddEditComponent,
    OfficeDetailComponent,
    OfficeTableComponent,
    OfficeChangeHistoryListingComponent,
    OfficeChangeHistoryAddEditComponent,
    OfficeChangeHistoryDetailComponent,
    OfficeChangeHistoryTableComponent,
    OfficeContactListingComponent,
    OfficeContactAddEditComponent,
    OfficeContactDetailComponent,
    OfficeContactTableComponent,
    OfficeContactChangeHistoryListingComponent,
    OfficeContactChangeHistoryAddEditComponent,
    OfficeContactChangeHistoryDetailComponent,
    OfficeContactChangeHistoryTableComponent,
    OfficeTypeListingComponent,
    OfficeTypeAddEditComponent,
    OfficeTypeDetailComponent,
    OfficeTypeTableComponent,
    PaymentTypeListingComponent,
    PaymentTypeAddEditComponent,
    PaymentTypeDetailComponent,
    PaymentTypeTableComponent,
    PledgeListingComponent,
    PledgeAddEditComponent,
    PledgeDetailComponent,
    PledgeTableComponent,
    PledgeChangeHistoryListingComponent,
    PledgeChangeHistoryAddEditComponent,
    PledgeChangeHistoryDetailComponent,
    PledgeChangeHistoryTableComponent,
    PriorityListingComponent,
    PriorityAddEditComponent,
    PriorityDetailComponent,
    PriorityTableComponent,
    QualificationListingComponent,
    QualificationAddEditComponent,
    QualificationDetailComponent,
    QualificationTableComponent,
    RateSheetListingComponent,
    RateSheetAddEditComponent,
    RateSheetDetailComponent,
    RateSheetTableComponent,
    RateSheetChangeHistoryListingComponent,
    RateSheetChangeHistoryAddEditComponent,
    RateSheetChangeHistoryDetailComponent,
    RateSheetChangeHistoryTableComponent,
    RateTypeListingComponent,
    RateTypeAddEditComponent,
    RateTypeDetailComponent,
    RateTypeTableComponent,
    ReceiptTypeListingComponent,
    ReceiptTypeAddEditComponent,
    ReceiptTypeDetailComponent,
    ReceiptTypeTableComponent,
    RecurrenceExceptionListingComponent,
    RecurrenceExceptionAddEditComponent,
    RecurrenceExceptionDetailComponent,
    RecurrenceExceptionTableComponent,
    RecurrenceExceptionChangeHistoryListingComponent,
    RecurrenceExceptionChangeHistoryAddEditComponent,
    RecurrenceExceptionChangeHistoryDetailComponent,
    RecurrenceExceptionChangeHistoryTableComponent,
    RecurrenceFrequencyListingComponent,
    RecurrenceFrequencyAddEditComponent,
    RecurrenceFrequencyDetailComponent,
    RecurrenceFrequencyTableComponent,
    RecurrenceRuleListingComponent,
    RecurrenceRuleAddEditComponent,
    RecurrenceRuleDetailComponent,
    RecurrenceRuleTableComponent,
    RecurrenceRuleChangeHistoryListingComponent,
    RecurrenceRuleChangeHistoryAddEditComponent,
    RecurrenceRuleChangeHistoryDetailComponent,
    RecurrenceRuleChangeHistoryTableComponent,
    RelationshipTypeListingComponent,
    RelationshipTypeAddEditComponent,
    RelationshipTypeDetailComponent,
    RelationshipTypeTableComponent,
    ResourceListingComponent,
    ResourceAddEditComponent,
    ResourceDetailComponent,
    ResourceTableComponent,
    ResourceAvailabilityListingComponent,
    ResourceAvailabilityAddEditComponent,
    ResourceAvailabilityDetailComponent,
    ResourceAvailabilityTableComponent,
    ResourceAvailabilityChangeHistoryListingComponent,
    ResourceAvailabilityChangeHistoryAddEditComponent,
    ResourceAvailabilityChangeHistoryDetailComponent,
    ResourceAvailabilityChangeHistoryTableComponent,
    ResourceChangeHistoryListingComponent,
    ResourceChangeHistoryAddEditComponent,
    ResourceChangeHistoryDetailComponent,
    ResourceChangeHistoryTableComponent,
    ResourceContactListingComponent,
    ResourceContactAddEditComponent,
    ResourceContactDetailComponent,
    ResourceContactTableComponent,
    ResourceContactChangeHistoryListingComponent,
    ResourceContactChangeHistoryAddEditComponent,
    ResourceContactChangeHistoryDetailComponent,
    ResourceContactChangeHistoryTableComponent,
    ResourceQualificationListingComponent,
    ResourceQualificationAddEditComponent,
    ResourceQualificationDetailComponent,
    ResourceQualificationTableComponent,
    ResourceQualificationChangeHistoryListingComponent,
    ResourceQualificationChangeHistoryAddEditComponent,
    ResourceQualificationChangeHistoryDetailComponent,
    ResourceQualificationChangeHistoryTableComponent,
    ResourceShiftListingComponent,
    ResourceShiftAddEditComponent,
    ResourceShiftDetailComponent,
    ResourceShiftTableComponent,
    ResourceShiftChangeHistoryListingComponent,
    ResourceShiftChangeHistoryAddEditComponent,
    ResourceShiftChangeHistoryDetailComponent,
    ResourceShiftChangeHistoryTableComponent,
    ResourceTypeListingComponent,
    ResourceTypeAddEditComponent,
    ResourceTypeDetailComponent,
    ResourceTypeTableComponent,
    SalutationListingComponent,
    SalutationAddEditComponent,
    SalutationDetailComponent,
    SalutationTableComponent,
    ScheduledEventListingComponent,
    ScheduledEventAddEditComponent,
    ScheduledEventDetailComponent,
    ScheduledEventTableComponent,
    ScheduledEventChangeHistoryListingComponent,
    ScheduledEventChangeHistoryAddEditComponent,
    ScheduledEventChangeHistoryDetailComponent,
    ScheduledEventChangeHistoryTableComponent,
    ScheduledEventDependencyListingComponent,
    ScheduledEventDependencyAddEditComponent,
    ScheduledEventDependencyDetailComponent,
    ScheduledEventDependencyTableComponent,
    ScheduledEventDependencyChangeHistoryListingComponent,
    ScheduledEventDependencyChangeHistoryAddEditComponent,
    ScheduledEventDependencyChangeHistoryDetailComponent,
    ScheduledEventDependencyChangeHistoryTableComponent,
    ScheduledEventQualificationRequirementListingComponent,
    ScheduledEventQualificationRequirementAddEditComponent,
    ScheduledEventQualificationRequirementDetailComponent,
    ScheduledEventQualificationRequirementTableComponent,
    ScheduledEventQualificationRequirementChangeHistoryListingComponent,
    ScheduledEventQualificationRequirementChangeHistoryAddEditComponent,
    ScheduledEventQualificationRequirementChangeHistoryDetailComponent,
    ScheduledEventQualificationRequirementChangeHistoryTableComponent,
    ScheduledEventTemplateListingComponent,
    ScheduledEventTemplateAddEditComponent,
    ScheduledEventTemplateDetailComponent,
    ScheduledEventTemplateTableComponent,
    ScheduledEventTemplateChangeHistoryListingComponent,
    ScheduledEventTemplateChangeHistoryAddEditComponent,
    ScheduledEventTemplateChangeHistoryDetailComponent,
    ScheduledEventTemplateChangeHistoryTableComponent,
    ScheduledEventTemplateChargeListingComponent,
    ScheduledEventTemplateChargeAddEditComponent,
    ScheduledEventTemplateChargeDetailComponent,
    ScheduledEventTemplateChargeTableComponent,
    ScheduledEventTemplateChargeChangeHistoryListingComponent,
    ScheduledEventTemplateChargeChangeHistoryAddEditComponent,
    ScheduledEventTemplateChargeChangeHistoryDetailComponent,
    ScheduledEventTemplateChargeChangeHistoryTableComponent,
    ScheduledEventTemplateQualificationRequirementListingComponent,
    ScheduledEventTemplateQualificationRequirementAddEditComponent,
    ScheduledEventTemplateQualificationRequirementDetailComponent,
    ScheduledEventTemplateQualificationRequirementTableComponent,
    ScheduledEventTemplateQualificationRequirementChangeHistoryListingComponent,
    ScheduledEventTemplateQualificationRequirementChangeHistoryAddEditComponent,
    ScheduledEventTemplateQualificationRequirementChangeHistoryDetailComponent,
    ScheduledEventTemplateQualificationRequirementChangeHistoryTableComponent,
    SchedulingTargetListingComponent,
    SchedulingTargetAddEditComponent,
    SchedulingTargetDetailComponent,
    SchedulingTargetTableComponent,
    SchedulingTargetAddressListingComponent,
    SchedulingTargetAddressAddEditComponent,
    SchedulingTargetAddressDetailComponent,
    SchedulingTargetAddressTableComponent,
    SchedulingTargetAddressChangeHistoryListingComponent,
    SchedulingTargetAddressChangeHistoryAddEditComponent,
    SchedulingTargetAddressChangeHistoryDetailComponent,
    SchedulingTargetAddressChangeHistoryTableComponent,
    SchedulingTargetChangeHistoryListingComponent,
    SchedulingTargetChangeHistoryAddEditComponent,
    SchedulingTargetChangeHistoryDetailComponent,
    SchedulingTargetChangeHistoryTableComponent,
    SchedulingTargetContactListingComponent,
    SchedulingTargetContactAddEditComponent,
    SchedulingTargetContactDetailComponent,
    SchedulingTargetContactTableComponent,
    SchedulingTargetContactChangeHistoryListingComponent,
    SchedulingTargetContactChangeHistoryAddEditComponent,
    SchedulingTargetContactChangeHistoryDetailComponent,
    SchedulingTargetContactChangeHistoryTableComponent,
    SchedulingTargetQualificationRequirementListingComponent,
    SchedulingTargetQualificationRequirementAddEditComponent,
    SchedulingTargetQualificationRequirementDetailComponent,
    SchedulingTargetQualificationRequirementTableComponent,
    SchedulingTargetQualificationRequirementChangeHistoryListingComponent,
    SchedulingTargetQualificationRequirementChangeHistoryAddEditComponent,
    SchedulingTargetQualificationRequirementChangeHistoryDetailComponent,
    SchedulingTargetQualificationRequirementChangeHistoryTableComponent,
    SchedulingTargetTypeListingComponent,
    SchedulingTargetTypeAddEditComponent,
    SchedulingTargetTypeDetailComponent,
    SchedulingTargetTypeTableComponent,
    ShiftPatternListingComponent,
    ShiftPatternAddEditComponent,
    ShiftPatternDetailComponent,
    ShiftPatternTableComponent,
    ShiftPatternChangeHistoryListingComponent,
    ShiftPatternChangeHistoryAddEditComponent,
    ShiftPatternChangeHistoryDetailComponent,
    ShiftPatternChangeHistoryTableComponent,
    ShiftPatternDayListingComponent,
    ShiftPatternDayAddEditComponent,
    ShiftPatternDayDetailComponent,
    ShiftPatternDayTableComponent,
    ShiftPatternDayChangeHistoryListingComponent,
    ShiftPatternDayChangeHistoryAddEditComponent,
    ShiftPatternDayChangeHistoryDetailComponent,
    ShiftPatternDayChangeHistoryTableComponent,
    SoftCreditListingComponent,
    SoftCreditAddEditComponent,
    SoftCreditDetailComponent,
    SoftCreditTableComponent,
    SoftCreditChangeHistoryListingComponent,
    SoftCreditChangeHistoryAddEditComponent,
    SoftCreditChangeHistoryDetailComponent,
    SoftCreditChangeHistoryTableComponent,
    StateProvinceListingComponent,
    StateProvinceAddEditComponent,
    StateProvinceDetailComponent,
    StateProvinceTableComponent,
    TagListingComponent,
    TagAddEditComponent,
    TagDetailComponent,
    TagTableComponent,
    TenantProfileListingComponent,
    TenantProfileAddEditComponent,
    TenantProfileDetailComponent,
    TenantProfileTableComponent,
    TenantProfileChangeHistoryListingComponent,
    TenantProfileChangeHistoryAddEditComponent,
    TenantProfileChangeHistoryDetailComponent,
    TenantProfileChangeHistoryTableComponent,
    TimeZoneListingComponent,
    TimeZoneAddEditComponent,
    TimeZoneDetailComponent,
    TimeZoneTableComponent,
    TributeListingComponent,
    TributeAddEditComponent,
    TributeDetailComponent,
    TributeTableComponent,
    TributeChangeHistoryListingComponent,
    TributeChangeHistoryAddEditComponent,
    TributeChangeHistoryDetailComponent,
    TributeChangeHistoryTableComponent,
    TributeTypeListingComponent,
    TributeTypeAddEditComponent,
    TributeTypeDetailComponent,
    TributeTypeTableComponent,
    //
    // End of declarations for Scheduler Data Components
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
    CrewWithMembersService,
    AssignmentService,
    SchedulerHelperService,

    //
    // Pipes
    //
    ContactFullNamePipe,
    ContrastColorPipe,

    //
    // Security Data Services - Auto generated
    //
    SecurityDataServiceManagerService,
    EntityDataTokenService,
    EntityDataTokenEventService,
    EntityDataTokenEventTypeService,
    LoginAttemptService,
    ModuleService,
    ModuleSecurityRoleService,
    OAUTHTokenService,
    PrivilegeService,
    SecurityDepartmentService,
    SecurityDepartmentUserService,
    SecurityGroupService,
    SecurityGroupSecurityRoleService,
    SecurityOrganizationService,
    SecurityOrganizationUserService,
    SecurityRoleService,
    SecurityTeamService,
    SecurityTeamUserService,
    SecurityTenantService,
    SecurityTenantUserService,
    SecurityUserService,
    SecurityUserEventService,
    SecurityUserEventTypeService,
    SecurityUserPasswordResetTokenService,
    SecurityUserSecurityGroupService,
    SecurityUserSecurityRoleService,
    SecurityUserTitleService,
    SystemSettingService,
    //
    // End Security Data Services
    //


    //
    // Auditor Data Services - Auto Generated
    // 
    AuditorDataServiceManagerService,
    AuditAccessTypeService,
    AuditEventService,
    AuditEventEntityStateService,
    AuditEventErrorMessageService,
    AuditHostSystemService,
    AuditModuleService,
    AuditModuleEntityService,
    AuditPlanBService,
    AuditResourceService,
    AuditSessionService,
    AuditSourceService,
    AuditTypeService,
    AuditUserService,
    AuditUserAgentService,
    ExternalCommunicationService,
    ExternalCommunicationRecipientService,
    //
    // End Auditor Data Services
    //


    //
    // Beginning of provider declarations for Scheduler Data Services 
    //
    SchedulerDataServiceManagerService,
    AppealService,
    AppealChangeHistoryService,
    AssignmentRoleService,
    AssignmentRoleQualificationRequirementService,
    AssignmentRoleQualificationRequirementChangeHistoryService,
    AssignmentStatusService,
    AttributeDefinitionService,
    BatchService,
    BatchChangeHistoryService,
    BatchStatusService,
    BookingSourceTypeService,
    CalendarService,
    CalendarChangeHistoryService,
    CampaignService,
    CampaignChangeHistoryService,
    ChargeStatusService,
    ChargeTypeService,
    ChargeTypeChangeHistoryService,
    ClientService,
    ClientChangeHistoryService,
    ClientContactService,
    ClientContactChangeHistoryService,
    ClientTypeService,
    ConstituentService,
    ConstituentChangeHistoryService,
    ConstituentJourneyStageService,
    ConstituentJourneyStageChangeHistoryService,
    ContactService,
    ContactChangeHistoryService,
    ContactContactService,
    ContactContactChangeHistoryService,
    ContactInteractionService,
    ContactInteractionChangeHistoryService,
    ContactMethodService,
    ContactTagService,
    ContactTagChangeHistoryService,
    ContactTypeService,
    CountryService,
    CrewService,
    CrewChangeHistoryService,
    CrewMemberService,
    CrewMemberChangeHistoryService,
    CurrencyService,
    DependencyTypeService,
    EventCalendarService,
    EventChargeService,
    EventChargeChangeHistoryService,
    EventResourceAssignmentService,
    EventResourceAssignmentChangeHistoryService,
    EventStatusService,
    FundService,
    FundChangeHistoryService,
    GiftService,
    GiftChangeHistoryService,
    HouseholdService,
    HouseholdChangeHistoryService,
    IconService,
    InteractionTypeService,
    NotificationSubscriptionService,
    NotificationSubscriptionChangeHistoryService,
    NotificationTypeService,
    OfficeService,
    OfficeChangeHistoryService,
    OfficeContactService,
    OfficeContactChangeHistoryService,
    OfficeTypeService,
    PaymentTypeService,
    PledgeService,
    PledgeChangeHistoryService,
    PriorityService,
    QualificationService,
    RateSheetService,
    RateSheetChangeHistoryService,
    RateTypeService,
    ReceiptTypeService,
    RecurrenceExceptionService,
    RecurrenceExceptionChangeHistoryService,
    RecurrenceFrequencyService,
    RecurrenceRuleService,
    RecurrenceRuleChangeHistoryService,
    RelationshipTypeService,
    ResourceService,
    ResourceAvailabilityService,
    ResourceAvailabilityChangeHistoryService,
    ResourceChangeHistoryService,
    ResourceContactService,
    ResourceContactChangeHistoryService,
    ResourceQualificationService,
    ResourceQualificationChangeHistoryService,
    ResourceShiftService,
    ResourceShiftChangeHistoryService,
    ResourceTypeService,
    SalutationService,
    ScheduledEventService,
    ScheduledEventChangeHistoryService,
    ScheduledEventDependencyService,
    ScheduledEventDependencyChangeHistoryService,
    ScheduledEventQualificationRequirementService,
    ScheduledEventQualificationRequirementChangeHistoryService,
    ScheduledEventTemplateService,
    ScheduledEventTemplateChangeHistoryService,
    ScheduledEventTemplateChargeService,
    ScheduledEventTemplateChargeChangeHistoryService,
    ScheduledEventTemplateQualificationRequirementService,
    ScheduledEventTemplateQualificationRequirementChangeHistoryService,
    SchedulingTargetService,
    SchedulingTargetAddressService,
    SchedulingTargetAddressChangeHistoryService,
    SchedulingTargetChangeHistoryService,
    SchedulingTargetContactService,
    SchedulingTargetContactChangeHistoryService,
    SchedulingTargetQualificationRequirementService,
    SchedulingTargetQualificationRequirementChangeHistoryService,
    SchedulingTargetTypeService,
    ShiftPatternService,
    ShiftPatternChangeHistoryService,
    ShiftPatternDayService,
    ShiftPatternDayChangeHistoryService,
    SoftCreditService,
    SoftCreditChangeHistoryService,
    StateProvinceService,
    TagService,
    TenantProfileService,
    TenantProfileChangeHistoryService,
    TimeZoneService,
    TributeService,
    TributeChangeHistoryService,
    TributeTypeService,
    //
    // End of provider declarations for Scheduler Data Services
    //


    //
    // For animations.
    //
    provideAnimationsAsync()
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
