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
import { IntelligenceService } from './services/intelligence.service';
import { RagProviderResolver } from './services/resolvers/rag-provider.resolver';
import { GeminiGroundingProvider } from './services/providers/gemini-grounding.provider';
import { IntelligenceModalComponent } from './components/shared/intelligence-modal/intelligence-modal.component';
import { ChangeHistoryViewerComponent } from './components/shared/change-history-viewer/change-history-viewer.component';
import { LocationMapComponent } from './components/shared/location-map/location-map.component';

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
import { OverviewManagerTabComponent } from './components/overview/overview-manager-tab/overview-manager-tab.component';
import { OverviewDispatcherTabComponent } from './components/overview/overview-dispatcher-tab/overview-dispatcher-tab.component';
import { OverviewSchedulerTabComponent } from './components/overview/overview-scheduler-tab/overview-scheduler-tab.component';
import { OverviewCoordinatorTabComponent } from './components/overview/overview-coordinator-tab/overview-Coordinator-tab.component';

import { HeaderComponent } from './components/header/header.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { ResetPasswordComponent } from './components/reset-password/reset-password.component';
import { NewUserComponent } from './components/new-user/new-user.component';

import { SystemHealthComponent } from './components/system-health/system-health.component';
import { SystemHealthService } from './services/system-health.service';

//
// Custom Components
//
import { SchedulerCalendarComponent } from './components/scheduler/scheduler-calendar/scheduler-calendar.component';
import { EventAddEditModalComponent } from './components/scheduler/event-add-edit-modal/event-add-edit-modal.component';
import { AdministrationComponent } from './components/administration/administration.component';
import { AddTenantProfileComponent } from './components/add-tenant-profile/add-tenant-profile.component';
import { RecurrenceBuilderComponent } from './components/scheduler/recurrence-builder/recurrence-builder.component';
import { TemplateManagerComponent } from './components/scheduler/template-manager/template-manager.component';
import { TemplateAddEditModalComponent } from './components/scheduler/template-add-edit-modal/template-add-edit-modal.component';


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
import { ResourceShiftTabComponent } from './components/resource-custom/resource-shift-tab/resource-shift-tab.component';
import { ResourceShiftAddEditModalComponent } from './components/resource-custom/resource-shift-add-edit-modal/resource-shift-add-edit-modal.component';
import { ResourceRatesTabComponent } from './components/resource-custom/resource-rates-tab/resource-rates-tab.component';
import { ResourceRateOverrideAddModalComponent } from './components/resource-custom/resource-rate-sheet-override-add-modal/resource-rate-sheet-override-add-modal.component';
import { ResourceAssignmentsTabComponent } from './components/resource-custom/resource-assignments-tab/resource-assignments-tab.component';
import { ResourceContactsTabComponent } from './components/resource-custom/resource-contacts-tab/resource-contacts-tab.component';
import { ResourceContactCustomAddEditModalComponent } from './components/resource-custom/resource-contact-custom-add-edit-modal/resource-contact-custom-add-edit-modal.component';
import { ResourceNotificationsTabComponent } from './components/resource-custom/resource-notifications-tab/resource-notifications-tab.component';
import { NotificationSubscriptionCustomAddEditModalComponent } from './components/resource-custom/notification-subscription-custom-add-edit-modal/notification-subscription-custom-add-edit-modal.component';

import { ShiftCustomListingComponent } from './components/shift-custom/shift-custom-listing/shift-custom-listing.component';
import { ShiftCustomDetailComponent } from './components/shift-custom/shift-custom-detail/shift-custom-detail.component';
import { ShiftCustomTableComponent } from './components/shift-custom/shift-custom-table/shift-custom-table.component';
import { ShiftCustomAddEditComponent } from './components/shift-custom/shift-custom-add-edit/shift-custom-add-edit.component';

import { ShiftPatternCustomListingComponent } from './components/shift-pattern-custom/shift-pattern-custom-listing/shift-pattern-custom-listing.component';
import { ShiftPatternCustomDetailComponent } from './components/shift-pattern-custom/shift-pattern-custom-detail/shift-pattern-custom-detail.component';
import { ShiftPatternCustomTableComponent } from './components/shift-pattern-custom/shift-pattern-custom-table/shift-pattern-custom-table.component';
import { ShiftPatternCustomAddEditComponent } from './components/shift-pattern-custom/shift-pattern-custom-add-edit/shift-pattern-custom-add-edit.component';
import { ShiftPatternDayAddEditModalComponent } from './components/shift-pattern-custom/shift-pattern-day-add-edit-modal/shift-pattern-day-add-edit-modal.component';

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
// Volunteer custom optimizations
//
import { VolunteerCustomListingComponent } from './components/volunteer-custom/volunteer-custom-listing/volunteer-custom-listing.component';
import { VolunteerCustomDetailComponent } from './components/volunteer-custom/volunteer-custom-detail/volunteer-custom-detail.component';
import { VolunteerCustomAddEditComponent } from './components/volunteer-custom/volunteer-custom-add-edit/volunteer-custom-add-edit.component';
import { VolunteerCustomTableComponent } from './components/volunteer-custom/volunteer-custom-table/volunteer-custom-table.component';
import { VolunteerOverviewTabComponent } from './components/volunteer-custom/volunteer-overview-tab/volunteer-overview-tab.component';
import { VolunteerGroupsTabComponent } from './components/volunteer-custom/volunteer-groups-tab/volunteer-groups-tab.component';
import { VolunteerAssignmentsTabComponent } from './components/volunteer-custom/volunteer-assignments-tab/volunteer-assignments-tab.component';
import { VolunteerHoursTabComponent } from './components/volunteer-custom/volunteer-hours-tab/volunteer-hours-tab.component';
import { VolunteerDashboardComponent } from './components/volunteer-custom/volunteer-dashboard/volunteer-dashboard.component';
import { VolunteerCalendarComponent } from './components/volunteer-custom/volunteer-calendar/volunteer-calendar.component';

//
// Volunteer Group custom optimizations
//
import { VolunteerGroupCustomListingComponent } from './components/volunteer-group-custom/volunteer-group-custom-listing/volunteer-group-custom-listing.component';
import { VolunteerGroupCustomDetailComponent } from './components/volunteer-group-custom/volunteer-group-custom-detail/volunteer-group-custom-detail.component';
import { VolunteerGroupCustomAddEditComponent } from './components/volunteer-group-custom/volunteer-group-custom-add-edit/volunteer-group-custom-add-edit.component';
import { VolunteerGroupCustomTableComponent } from './components/volunteer-group-custom/volunteer-group-custom-table/volunteer-group-custom-table.component';
import { VolunteerGroupOverviewTabComponent } from './components/volunteer-group-custom/volunteer-group-overview-tab/volunteer-group-overview-tab.component';
import { VolunteerGroupMembersTabComponent } from './components/volunteer-group-custom/volunteer-group-members-tab/volunteer-group-members-tab.component';
import { VolunteerGroupAddMemberModalComponent } from './components/volunteer-group-custom/volunteer-group-add-member-modal/volunteer-group-add-member-modal.component';

//
// Financial custom components
//
import { FinancialCustomDashboardComponent } from './components/financial-custom/financial-custom-dashboard/financial-custom-dashboard.component';
import { FinancialTransactionCustomListingComponent } from './components/financial-custom/financial-transaction-custom-listing/financial-transaction-custom-listing.component';
import { FinancialCategoryCustomListingComponent } from './components/financial-custom/financial-category-custom-listing/financial-category-custom-listing.component';
import { FinancialTransactionCustomAddEditComponent } from './components/financial-custom/financial-transaction-custom-add-edit/financial-transaction-custom-add-edit.component';
import { FinancialBudgetManagerComponent } from './components/financial-custom/financial-budget-manager/financial-budget-manager.component';
import { FinancialCategoryCustomAddEditComponent } from './components/financial-custom/financial-category-custom-add-edit/financial-category-custom-add-edit.component';

//
// Invoice custom components
//
import { InvoiceCustomListingComponent } from './components/invoice-custom/invoice-custom-listing/invoice-custom-listing.component';
import { InvoiceCustomDetailComponent } from './components/invoice-custom/invoice-custom-detail/invoice-custom-detail.component';
import { InvoiceCustomAddEditComponent } from './components/invoice-custom/invoice-custom-add-edit/invoice-custom-add-edit.component';

//
// Receipt custom components
//
import { ReceiptCustomListingComponent } from './components/receipt-custom/receipt-custom-listing/receipt-custom-listing.component';
import { ReceiptCustomDetailComponent } from './components/receipt-custom/receipt-custom-detail/receipt-custom-detail.component';
import { ReceiptCustomAddEditComponent } from './components/receipt-custom/receipt-custom-add-edit/receipt-custom-add-edit.component';

//
// Payment custom components
//
import { PaymentCustomListingComponent } from './components/payment-custom/payment-custom-listing/payment-custom-listing.component';
import { PaymentCustomDetailComponent } from './components/payment-custom/payment-custom-detail/payment-custom-detail.component';
import { PaymentCustomAddEditComponent } from './components/payment-custom/payment-custom-add-edit/payment-custom-add-edit.component';

//
// Budget report & Deposit manager
//
import { BudgetReportComponent } from './components/financial-custom/budget-report/budget-report.component';
import { DepositManagerComponent } from './components/financial-custom/deposit-manager/deposit-manager.component';
import { PnlReportComponent } from './components/financial-custom/pnl-report/pnl-report.component';
import { AccountantReportsComponent } from './components/financial-custom/accountant-reports/accountant-reports.component';
import { RentalAgreementTrackerComponent } from './components/scheduler-custom/rental-agreement-tracker/rental-agreement-tracker.component';
import { FiscalPeriodCloseComponent } from './components/financial-custom/fiscal-period-close/fiscal-period-close.component';


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
import { ContactRelationshipEditModalComponent } from './components/contact-custom/contact-relationship-edit-modal/contact-relationship-edit-modal.component';
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
// Scheduling Target custom optimizations
//
import { SchedulingTargetCustomListingComponent } from './components/scheduling-target-custom/scheduling-target-custom-listing/scheduling-target-custom-listing.component';
import { SchedulingTargetCustomTableComponent } from './components/scheduling-target-custom/scheduling-target-custom-table/scheduling-target-custom-table.component';
import { SchedulingTargetCustomDetailComponent } from './components/scheduling-target-custom/scheduling-target-custom-detail/scheduling-target-custom-detail.component';


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
import { ConfirmationDialogComponent } from './services/confirmation-dialog/confirmation-dialog.component';

import { InputDialogService } from './services/input-dialog.service';
import { InputDialogComponent } from './services/input-dialog/input-dialog.component';


//
// Data support services
//
import { CurrentUserService } from './services/current-user.service';
import { CacheManagerService } from './services/cache-manager.service';
import { TenantHelperService } from './services/tenant-helper.service';



//
// Beginning of imports for Scheduler Data Services
//
import { SchedulerDataServiceManagerService } from './scheduler-data-services/scheduler-data-service-manager.service';
import { AccountTypeService } from './scheduler-data-services/account-type.service';
import { AppealService } from './scheduler-data-services/appeal.service';
import { AppealChangeHistoryService } from './scheduler-data-services/appeal-change-history.service';
import { AssignmentRoleService } from './scheduler-data-services/assignment-role.service';
import { AssignmentRoleQualificationRequirementService } from './scheduler-data-services/assignment-role-qualification-requirement.service';
import { AssignmentRoleQualificationRequirementChangeHistoryService } from './scheduler-data-services/assignment-role-qualification-requirement-change-history.service';
import { AssignmentStatusService } from './scheduler-data-services/assignment-status.service';
import { AttributeDefinitionService } from './scheduler-data-services/attribute-definition.service';
import { AttributeDefinitionChangeHistoryService } from './scheduler-data-services/attribute-definition-change-history.service';
import { AttributeDefinitionEntityService } from './scheduler-data-services/attribute-definition-entity.service';
import { AttributeDefinitionTypeService } from './scheduler-data-services/attribute-definition-type.service';
import { BatchService } from './scheduler-data-services/batch.service';
import { BatchChangeHistoryService } from './scheduler-data-services/batch-change-history.service';
import { BatchStatusService } from './scheduler-data-services/batch-status.service';
import { BookingSourceTypeService } from './scheduler-data-services/booking-source-type.service';
import { BudgetService } from './scheduler-data-services/budget.service';
import { BudgetChangeHistoryService } from './scheduler-data-services/budget-change-history.service';
import { CalendarService } from './scheduler-data-services/calendar.service';
import { CalendarChangeHistoryService } from './scheduler-data-services/calendar-change-history.service';
import { CampaignService } from './scheduler-data-services/campaign.service';
import { CampaignChangeHistoryService } from './scheduler-data-services/campaign-change-history.service';
import { ChargeStatusService } from './scheduler-data-services/charge-status.service';
import { ChargeStatusChangeHistoryService } from './scheduler-data-services/charge-status-change-history.service';
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
import { DocumentService } from './scheduler-data-services/document.service';
import { DocumentChangeHistoryService } from './scheduler-data-services/document-change-history.service';
import { DocumentTypeService } from './scheduler-data-services/document-type.service';
import { EventCalendarService } from './scheduler-data-services/event-calendar.service';
import { EventChargeService } from './scheduler-data-services/event-charge.service';
import { EventChargeChangeHistoryService } from './scheduler-data-services/event-charge-change-history.service';
import { EventResourceAssignmentService } from './scheduler-data-services/event-resource-assignment.service';
import { EventResourceAssignmentChangeHistoryService } from './scheduler-data-services/event-resource-assignment-change-history.service';
import { EventStatusService } from './scheduler-data-services/event-status.service';
import { FinancialCategoryService } from './scheduler-data-services/financial-category.service';
import { FinancialCategoryChangeHistoryService } from './scheduler-data-services/financial-category-change-history.service';
import { FinancialOfficeService } from './scheduler-data-services/financial-office.service';
import { FinancialOfficeChangeHistoryService } from './scheduler-data-services/financial-office-change-history.service';
import { FinancialTransactionService } from './scheduler-data-services/financial-transaction.service';
import { FinancialTransactionChangeHistoryService } from './scheduler-data-services/financial-transaction-change-history.service';
import { FiscalPeriodService } from './scheduler-data-services/fiscal-period.service';
import { FiscalPeriodChangeHistoryService } from './scheduler-data-services/fiscal-period-change-history.service';
import { FundService } from './scheduler-data-services/fund.service';
import { FundChangeHistoryService } from './scheduler-data-services/fund-change-history.service';
import { GiftService } from './scheduler-data-services/gift.service';
import { GiftChangeHistoryService } from './scheduler-data-services/gift-change-history.service';
import { HouseholdService } from './scheduler-data-services/household.service';
import { HouseholdChangeHistoryService } from './scheduler-data-services/household-change-history.service';
import { IconService } from './scheduler-data-services/icon.service';
import { InteractionTypeService } from './scheduler-data-services/interaction-type.service';
import { InvoiceService } from './scheduler-data-services/invoice.service';
import { InvoiceChangeHistoryService } from './scheduler-data-services/invoice-change-history.service';
import { InvoiceLineItemService } from './scheduler-data-services/invoice-line-item.service';
import { InvoiceStatusService } from './scheduler-data-services/invoice-status.service';
import { NotificationSubscriptionService } from './scheduler-data-services/notification-subscription.service';
import { NotificationSubscriptionChangeHistoryService } from './scheduler-data-services/notification-subscription-change-history.service';
import { NotificationTypeService } from './scheduler-data-services/notification-type.service';
import { OfficeService } from './scheduler-data-services/office.service';
import { OfficeChangeHistoryService } from './scheduler-data-services/office-change-history.service';
import { OfficeContactService } from './scheduler-data-services/office-contact.service';
import { OfficeContactChangeHistoryService } from './scheduler-data-services/office-contact-change-history.service';
import { OfficeTypeService } from './scheduler-data-services/office-type.service';
import { PaymentMethodService } from './scheduler-data-services/payment-method.service';
import { PaymentProviderService } from './scheduler-data-services/payment-provider.service';
import { PaymentProviderChangeHistoryService } from './scheduler-data-services/payment-provider-change-history.service';
import { PaymentTransactionService } from './scheduler-data-services/payment-transaction.service';
import { PaymentTransactionChangeHistoryService } from './scheduler-data-services/payment-transaction-change-history.service';
import { PaymentTypeService } from './scheduler-data-services/payment-type.service';
import { PaymentTypeChangeHistoryService } from './scheduler-data-services/payment-type-change-history.service';
import { PeriodStatusService } from './scheduler-data-services/period-status.service';
import { PledgeService } from './scheduler-data-services/pledge.service';
import { PledgeChangeHistoryService } from './scheduler-data-services/pledge-change-history.service';
import { PriorityService } from './scheduler-data-services/priority.service';
import { QualificationService } from './scheduler-data-services/qualification.service';
import { RateSheetService } from './scheduler-data-services/rate-sheet.service';
import { RateSheetChangeHistoryService } from './scheduler-data-services/rate-sheet-change-history.service';
import { RateTypeService } from './scheduler-data-services/rate-type.service';
import { ReceiptService } from './scheduler-data-services/receipt.service';
import { ReceiptChangeHistoryService } from './scheduler-data-services/receipt-change-history.service';
import { ReceiptTypeService } from './scheduler-data-services/receipt-type.service';
import { ReceiptTypeChangeHistoryService } from './scheduler-data-services/receipt-type-change-history.service';
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
import { TaxCodeService } from './scheduler-data-services/tax-code.service';
import { TenantProfileService } from './scheduler-data-services/tenant-profile.service';
import { TenantProfileChangeHistoryService } from './scheduler-data-services/tenant-profile-change-history.service';
import { TimeZoneService } from './scheduler-data-services/time-zone.service';
import { TributeService } from './scheduler-data-services/tribute.service';
import { TributeChangeHistoryService } from './scheduler-data-services/tribute-change-history.service';
import { TributeTypeService } from './scheduler-data-services/tribute-type.service';
import { VolunteerGroupService } from './scheduler-data-services/volunteer-group.service';
import { VolunteerGroupChangeHistoryService } from './scheduler-data-services/volunteer-group-change-history.service';
import { VolunteerGroupMemberService } from './scheduler-data-services/volunteer-group-member.service';
import { VolunteerGroupMemberChangeHistoryService } from './scheduler-data-services/volunteer-group-member-change-history.service';
import { VolunteerProfileService } from './scheduler-data-services/volunteer-profile.service';
import { VolunteerProfileChangeHistoryService } from './scheduler-data-services/volunteer-profile-change-history.service';
import { VolunteerStatusService } from './scheduler-data-services/volunteer-status.service';
//
// End of imports for Scheduler Data Services
//



//
// Beginning of imports for Scheduler Data Components
//
import { AccountTypeListingComponent } from './scheduler-data-components/account-type/account-type-listing/account-type-listing.component';
import { AccountTypeAddEditComponent } from './scheduler-data-components/account-type/account-type-add-edit/account-type-add-edit.component';
import { AccountTypeDetailComponent } from './scheduler-data-components/account-type/account-type-detail/account-type-detail.component';
import { AccountTypeTableComponent } from './scheduler-data-components/account-type/account-type-table/account-type-table.component';
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
import { AttributeDefinitionChangeHistoryListingComponent } from './scheduler-data-components/attribute-definition-change-history/attribute-definition-change-history-listing/attribute-definition-change-history-listing.component';
import { AttributeDefinitionChangeHistoryAddEditComponent } from './scheduler-data-components/attribute-definition-change-history/attribute-definition-change-history-add-edit/attribute-definition-change-history-add-edit.component';
import { AttributeDefinitionChangeHistoryDetailComponent } from './scheduler-data-components/attribute-definition-change-history/attribute-definition-change-history-detail/attribute-definition-change-history-detail.component';
import { AttributeDefinitionChangeHistoryTableComponent } from './scheduler-data-components/attribute-definition-change-history/attribute-definition-change-history-table/attribute-definition-change-history-table.component';
import { AttributeDefinitionEntityListingComponent } from './scheduler-data-components/attribute-definition-entity/attribute-definition-entity-listing/attribute-definition-entity-listing.component';
import { AttributeDefinitionEntityAddEditComponent } from './scheduler-data-components/attribute-definition-entity/attribute-definition-entity-add-edit/attribute-definition-entity-add-edit.component';
import { AttributeDefinitionEntityDetailComponent } from './scheduler-data-components/attribute-definition-entity/attribute-definition-entity-detail/attribute-definition-entity-detail.component';
import { AttributeDefinitionEntityTableComponent } from './scheduler-data-components/attribute-definition-entity/attribute-definition-entity-table/attribute-definition-entity-table.component';
import { AttributeDefinitionTypeListingComponent } from './scheduler-data-components/attribute-definition-type/attribute-definition-type-listing/attribute-definition-type-listing.component';
import { AttributeDefinitionTypeAddEditComponent } from './scheduler-data-components/attribute-definition-type/attribute-definition-type-add-edit/attribute-definition-type-add-edit.component';
import { AttributeDefinitionTypeDetailComponent } from './scheduler-data-components/attribute-definition-type/attribute-definition-type-detail/attribute-definition-type-detail.component';
import { AttributeDefinitionTypeTableComponent } from './scheduler-data-components/attribute-definition-type/attribute-definition-type-table/attribute-definition-type-table.component';
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
import { BudgetListingComponent } from './scheduler-data-components/budget/budget-listing/budget-listing.component';
import { BudgetAddEditComponent } from './scheduler-data-components/budget/budget-add-edit/budget-add-edit.component';
import { BudgetDetailComponent } from './scheduler-data-components/budget/budget-detail/budget-detail.component';
import { BudgetTableComponent } from './scheduler-data-components/budget/budget-table/budget-table.component';
import { BudgetChangeHistoryListingComponent } from './scheduler-data-components/budget-change-history/budget-change-history-listing/budget-change-history-listing.component';
import { BudgetChangeHistoryAddEditComponent } from './scheduler-data-components/budget-change-history/budget-change-history-add-edit/budget-change-history-add-edit.component';
import { BudgetChangeHistoryDetailComponent } from './scheduler-data-components/budget-change-history/budget-change-history-detail/budget-change-history-detail.component';
import { BudgetChangeHistoryTableComponent } from './scheduler-data-components/budget-change-history/budget-change-history-table/budget-change-history-table.component';
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
import { ChargeStatusChangeHistoryListingComponent } from './scheduler-data-components/charge-status-change-history/charge-status-change-history-listing/charge-status-change-history-listing.component';
import { ChargeStatusChangeHistoryAddEditComponent } from './scheduler-data-components/charge-status-change-history/charge-status-change-history-add-edit/charge-status-change-history-add-edit.component';
import { ChargeStatusChangeHistoryDetailComponent } from './scheduler-data-components/charge-status-change-history/charge-status-change-history-detail/charge-status-change-history-detail.component';
import { ChargeStatusChangeHistoryTableComponent } from './scheduler-data-components/charge-status-change-history/charge-status-change-history-table/charge-status-change-history-table.component';
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
import { DocumentListingComponent } from './scheduler-data-components/document/document-listing/document-listing.component';
import { DocumentAddEditComponent } from './scheduler-data-components/document/document-add-edit/document-add-edit.component';
import { DocumentDetailComponent } from './scheduler-data-components/document/document-detail/document-detail.component';
import { DocumentTableComponent } from './scheduler-data-components/document/document-table/document-table.component';
import { DocumentChangeHistoryListingComponent } from './scheduler-data-components/document-change-history/document-change-history-listing/document-change-history-listing.component';
import { DocumentChangeHistoryAddEditComponent } from './scheduler-data-components/document-change-history/document-change-history-add-edit/document-change-history-add-edit.component';
import { DocumentChangeHistoryDetailComponent } from './scheduler-data-components/document-change-history/document-change-history-detail/document-change-history-detail.component';
import { DocumentChangeHistoryTableComponent } from './scheduler-data-components/document-change-history/document-change-history-table/document-change-history-table.component';
import { DocumentTypeListingComponent } from './scheduler-data-components/document-type/document-type-listing/document-type-listing.component';
import { DocumentTypeAddEditComponent } from './scheduler-data-components/document-type/document-type-add-edit/document-type-add-edit.component';
import { DocumentTypeDetailComponent } from './scheduler-data-components/document-type/document-type-detail/document-type-detail.component';
import { DocumentTypeTableComponent } from './scheduler-data-components/document-type/document-type-table/document-type-table.component';
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
import { FinancialCategoryListingComponent } from './scheduler-data-components/financial-category/financial-category-listing/financial-category-listing.component';
import { FinancialCategoryAddEditComponent } from './scheduler-data-components/financial-category/financial-category-add-edit/financial-category-add-edit.component';
import { FinancialCategoryDetailComponent } from './scheduler-data-components/financial-category/financial-category-detail/financial-category-detail.component';
import { FinancialCategoryTableComponent } from './scheduler-data-components/financial-category/financial-category-table/financial-category-table.component';
import { FinancialCategoryChangeHistoryListingComponent } from './scheduler-data-components/financial-category-change-history/financial-category-change-history-listing/financial-category-change-history-listing.component';
import { FinancialCategoryChangeHistoryAddEditComponent } from './scheduler-data-components/financial-category-change-history/financial-category-change-history-add-edit/financial-category-change-history-add-edit.component';
import { FinancialCategoryChangeHistoryDetailComponent } from './scheduler-data-components/financial-category-change-history/financial-category-change-history-detail/financial-category-change-history-detail.component';
import { FinancialCategoryChangeHistoryTableComponent } from './scheduler-data-components/financial-category-change-history/financial-category-change-history-table/financial-category-change-history-table.component';
import { FinancialOfficeListingComponent } from './scheduler-data-components/financial-office/financial-office-listing/financial-office-listing.component';
import { FinancialOfficeAddEditComponent } from './scheduler-data-components/financial-office/financial-office-add-edit/financial-office-add-edit.component';
import { FinancialOfficeDetailComponent } from './scheduler-data-components/financial-office/financial-office-detail/financial-office-detail.component';
import { FinancialOfficeTableComponent } from './scheduler-data-components/financial-office/financial-office-table/financial-office-table.component';
import { FinancialOfficeChangeHistoryListingComponent } from './scheduler-data-components/financial-office-change-history/financial-office-change-history-listing/financial-office-change-history-listing.component';
import { FinancialOfficeChangeHistoryAddEditComponent } from './scheduler-data-components/financial-office-change-history/financial-office-change-history-add-edit/financial-office-change-history-add-edit.component';
import { FinancialOfficeChangeHistoryDetailComponent } from './scheduler-data-components/financial-office-change-history/financial-office-change-history-detail/financial-office-change-history-detail.component';
import { FinancialOfficeChangeHistoryTableComponent } from './scheduler-data-components/financial-office-change-history/financial-office-change-history-table/financial-office-change-history-table.component';
import { FinancialTransactionListingComponent } from './scheduler-data-components/financial-transaction/financial-transaction-listing/financial-transaction-listing.component';
import { FinancialTransactionAddEditComponent } from './scheduler-data-components/financial-transaction/financial-transaction-add-edit/financial-transaction-add-edit.component';
import { FinancialTransactionDetailComponent } from './scheduler-data-components/financial-transaction/financial-transaction-detail/financial-transaction-detail.component';
import { FinancialTransactionTableComponent } from './scheduler-data-components/financial-transaction/financial-transaction-table/financial-transaction-table.component';
import { FinancialTransactionChangeHistoryListingComponent } from './scheduler-data-components/financial-transaction-change-history/financial-transaction-change-history-listing/financial-transaction-change-history-listing.component';
import { FinancialTransactionChangeHistoryAddEditComponent } from './scheduler-data-components/financial-transaction-change-history/financial-transaction-change-history-add-edit/financial-transaction-change-history-add-edit.component';
import { FinancialTransactionChangeHistoryDetailComponent } from './scheduler-data-components/financial-transaction-change-history/financial-transaction-change-history-detail/financial-transaction-change-history-detail.component';
import { FinancialTransactionChangeHistoryTableComponent } from './scheduler-data-components/financial-transaction-change-history/financial-transaction-change-history-table/financial-transaction-change-history-table.component';
import { FiscalPeriodListingComponent } from './scheduler-data-components/fiscal-period/fiscal-period-listing/fiscal-period-listing.component';
import { FiscalPeriodAddEditComponent } from './scheduler-data-components/fiscal-period/fiscal-period-add-edit/fiscal-period-add-edit.component';
import { FiscalPeriodDetailComponent } from './scheduler-data-components/fiscal-period/fiscal-period-detail/fiscal-period-detail.component';
import { FiscalPeriodTableComponent } from './scheduler-data-components/fiscal-period/fiscal-period-table/fiscal-period-table.component';
import { FiscalPeriodChangeHistoryListingComponent } from './scheduler-data-components/fiscal-period-change-history/fiscal-period-change-history-listing/fiscal-period-change-history-listing.component';
import { FiscalPeriodChangeHistoryAddEditComponent } from './scheduler-data-components/fiscal-period-change-history/fiscal-period-change-history-add-edit/fiscal-period-change-history-add-edit.component';
import { FiscalPeriodChangeHistoryDetailComponent } from './scheduler-data-components/fiscal-period-change-history/fiscal-period-change-history-detail/fiscal-period-change-history-detail.component';
import { FiscalPeriodChangeHistoryTableComponent } from './scheduler-data-components/fiscal-period-change-history/fiscal-period-change-history-table/fiscal-period-change-history-table.component';
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
import { InvoiceListingComponent } from './scheduler-data-components/invoice/invoice-listing/invoice-listing.component';
import { InvoiceAddEditComponent } from './scheduler-data-components/invoice/invoice-add-edit/invoice-add-edit.component';
import { InvoiceDetailComponent } from './scheduler-data-components/invoice/invoice-detail/invoice-detail.component';
import { InvoiceTableComponent } from './scheduler-data-components/invoice/invoice-table/invoice-table.component';
import { InvoiceChangeHistoryListingComponent } from './scheduler-data-components/invoice-change-history/invoice-change-history-listing/invoice-change-history-listing.component';
import { InvoiceChangeHistoryAddEditComponent } from './scheduler-data-components/invoice-change-history/invoice-change-history-add-edit/invoice-change-history-add-edit.component';
import { InvoiceChangeHistoryDetailComponent } from './scheduler-data-components/invoice-change-history/invoice-change-history-detail/invoice-change-history-detail.component';
import { InvoiceChangeHistoryTableComponent } from './scheduler-data-components/invoice-change-history/invoice-change-history-table/invoice-change-history-table.component';
import { InvoiceLineItemListingComponent } from './scheduler-data-components/invoice-line-item/invoice-line-item-listing/invoice-line-item-listing.component';
import { InvoiceLineItemAddEditComponent } from './scheduler-data-components/invoice-line-item/invoice-line-item-add-edit/invoice-line-item-add-edit.component';
import { InvoiceLineItemDetailComponent } from './scheduler-data-components/invoice-line-item/invoice-line-item-detail/invoice-line-item-detail.component';
import { InvoiceLineItemTableComponent } from './scheduler-data-components/invoice-line-item/invoice-line-item-table/invoice-line-item-table.component';
import { InvoiceStatusListingComponent } from './scheduler-data-components/invoice-status/invoice-status-listing/invoice-status-listing.component';
import { InvoiceStatusAddEditComponent } from './scheduler-data-components/invoice-status/invoice-status-add-edit/invoice-status-add-edit.component';
import { InvoiceStatusDetailComponent } from './scheduler-data-components/invoice-status/invoice-status-detail/invoice-status-detail.component';
import { InvoiceStatusTableComponent } from './scheduler-data-components/invoice-status/invoice-status-table/invoice-status-table.component';
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
import { PaymentMethodListingComponent } from './scheduler-data-components/payment-method/payment-method-listing/payment-method-listing.component';
import { PaymentMethodAddEditComponent } from './scheduler-data-components/payment-method/payment-method-add-edit/payment-method-add-edit.component';
import { PaymentMethodDetailComponent } from './scheduler-data-components/payment-method/payment-method-detail/payment-method-detail.component';
import { PaymentMethodTableComponent } from './scheduler-data-components/payment-method/payment-method-table/payment-method-table.component';
import { PaymentProviderListingComponent } from './scheduler-data-components/payment-provider/payment-provider-listing/payment-provider-listing.component';
import { PaymentProviderAddEditComponent } from './scheduler-data-components/payment-provider/payment-provider-add-edit/payment-provider-add-edit.component';
import { PaymentProviderDetailComponent } from './scheduler-data-components/payment-provider/payment-provider-detail/payment-provider-detail.component';
import { PaymentProviderTableComponent } from './scheduler-data-components/payment-provider/payment-provider-table/payment-provider-table.component';
import { PaymentProviderChangeHistoryListingComponent } from './scheduler-data-components/payment-provider-change-history/payment-provider-change-history-listing/payment-provider-change-history-listing.component';
import { PaymentProviderChangeHistoryAddEditComponent } from './scheduler-data-components/payment-provider-change-history/payment-provider-change-history-add-edit/payment-provider-change-history-add-edit.component';
import { PaymentProviderChangeHistoryDetailComponent } from './scheduler-data-components/payment-provider-change-history/payment-provider-change-history-detail/payment-provider-change-history-detail.component';
import { PaymentProviderChangeHistoryTableComponent } from './scheduler-data-components/payment-provider-change-history/payment-provider-change-history-table/payment-provider-change-history-table.component';
import { PaymentTransactionListingComponent } from './scheduler-data-components/payment-transaction/payment-transaction-listing/payment-transaction-listing.component';
import { PaymentTransactionAddEditComponent } from './scheduler-data-components/payment-transaction/payment-transaction-add-edit/payment-transaction-add-edit.component';
import { PaymentTransactionDetailComponent } from './scheduler-data-components/payment-transaction/payment-transaction-detail/payment-transaction-detail.component';
import { PaymentTransactionTableComponent } from './scheduler-data-components/payment-transaction/payment-transaction-table/payment-transaction-table.component';
import { PaymentTransactionChangeHistoryListingComponent } from './scheduler-data-components/payment-transaction-change-history/payment-transaction-change-history-listing/payment-transaction-change-history-listing.component';
import { PaymentTransactionChangeHistoryAddEditComponent } from './scheduler-data-components/payment-transaction-change-history/payment-transaction-change-history-add-edit/payment-transaction-change-history-add-edit.component';
import { PaymentTransactionChangeHistoryDetailComponent } from './scheduler-data-components/payment-transaction-change-history/payment-transaction-change-history-detail/payment-transaction-change-history-detail.component';
import { PaymentTransactionChangeHistoryTableComponent } from './scheduler-data-components/payment-transaction-change-history/payment-transaction-change-history-table/payment-transaction-change-history-table.component';
import { PaymentTypeListingComponent } from './scheduler-data-components/payment-type/payment-type-listing/payment-type-listing.component';
import { PaymentTypeAddEditComponent } from './scheduler-data-components/payment-type/payment-type-add-edit/payment-type-add-edit.component';
import { PaymentTypeDetailComponent } from './scheduler-data-components/payment-type/payment-type-detail/payment-type-detail.component';
import { PaymentTypeTableComponent } from './scheduler-data-components/payment-type/payment-type-table/payment-type-table.component';
import { PaymentTypeChangeHistoryListingComponent } from './scheduler-data-components/payment-type-change-history/payment-type-change-history-listing/payment-type-change-history-listing.component';
import { PaymentTypeChangeHistoryAddEditComponent } from './scheduler-data-components/payment-type-change-history/payment-type-change-history-add-edit/payment-type-change-history-add-edit.component';
import { PaymentTypeChangeHistoryDetailComponent } from './scheduler-data-components/payment-type-change-history/payment-type-change-history-detail/payment-type-change-history-detail.component';
import { PaymentTypeChangeHistoryTableComponent } from './scheduler-data-components/payment-type-change-history/payment-type-change-history-table/payment-type-change-history-table.component';
import { PeriodStatusListingComponent } from './scheduler-data-components/period-status/period-status-listing/period-status-listing.component';
import { PeriodStatusAddEditComponent } from './scheduler-data-components/period-status/period-status-add-edit/period-status-add-edit.component';
import { PeriodStatusDetailComponent } from './scheduler-data-components/period-status/period-status-detail/period-status-detail.component';
import { PeriodStatusTableComponent } from './scheduler-data-components/period-status/period-status-table/period-status-table.component';
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
import { ReceiptListingComponent } from './scheduler-data-components/receipt/receipt-listing/receipt-listing.component';
import { ReceiptAddEditComponent } from './scheduler-data-components/receipt/receipt-add-edit/receipt-add-edit.component';
import { ReceiptDetailComponent } from './scheduler-data-components/receipt/receipt-detail/receipt-detail.component';
import { ReceiptTableComponent } from './scheduler-data-components/receipt/receipt-table/receipt-table.component';
import { ReceiptChangeHistoryListingComponent } from './scheduler-data-components/receipt-change-history/receipt-change-history-listing/receipt-change-history-listing.component';
import { ReceiptChangeHistoryAddEditComponent } from './scheduler-data-components/receipt-change-history/receipt-change-history-add-edit/receipt-change-history-add-edit.component';
import { ReceiptChangeHistoryDetailComponent } from './scheduler-data-components/receipt-change-history/receipt-change-history-detail/receipt-change-history-detail.component';
import { ReceiptChangeHistoryTableComponent } from './scheduler-data-components/receipt-change-history/receipt-change-history-table/receipt-change-history-table.component';
import { ReceiptTypeListingComponent } from './scheduler-data-components/receipt-type/receipt-type-listing/receipt-type-listing.component';
import { ReceiptTypeAddEditComponent } from './scheduler-data-components/receipt-type/receipt-type-add-edit/receipt-type-add-edit.component';
import { ReceiptTypeDetailComponent } from './scheduler-data-components/receipt-type/receipt-type-detail/receipt-type-detail.component';
import { ReceiptTypeTableComponent } from './scheduler-data-components/receipt-type/receipt-type-table/receipt-type-table.component';
import { ReceiptTypeChangeHistoryListingComponent } from './scheduler-data-components/receipt-type-change-history/receipt-type-change-history-listing/receipt-type-change-history-listing.component';
import { ReceiptTypeChangeHistoryAddEditComponent } from './scheduler-data-components/receipt-type-change-history/receipt-type-change-history-add-edit/receipt-type-change-history-add-edit.component';
import { ReceiptTypeChangeHistoryDetailComponent } from './scheduler-data-components/receipt-type-change-history/receipt-type-change-history-detail/receipt-type-change-history-detail.component';
import { ReceiptTypeChangeHistoryTableComponent } from './scheduler-data-components/receipt-type-change-history/receipt-type-change-history-table/receipt-type-change-history-table.component';
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
import { TaxCodeListingComponent } from './scheduler-data-components/tax-code/tax-code-listing/tax-code-listing.component';
import { TaxCodeAddEditComponent } from './scheduler-data-components/tax-code/tax-code-add-edit/tax-code-add-edit.component';
import { TaxCodeDetailComponent } from './scheduler-data-components/tax-code/tax-code-detail/tax-code-detail.component';
import { TaxCodeTableComponent } from './scheduler-data-components/tax-code/tax-code-table/tax-code-table.component';
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
import { VolunteerGroupListingComponent } from './scheduler-data-components/volunteer-group/volunteer-group-listing/volunteer-group-listing.component';
import { VolunteerGroupAddEditComponent } from './scheduler-data-components/volunteer-group/volunteer-group-add-edit/volunteer-group-add-edit.component';
import { VolunteerGroupDetailComponent } from './scheduler-data-components/volunteer-group/volunteer-group-detail/volunteer-group-detail.component';
import { VolunteerGroupTableComponent } from './scheduler-data-components/volunteer-group/volunteer-group-table/volunteer-group-table.component';
import { VolunteerGroupChangeHistoryListingComponent } from './scheduler-data-components/volunteer-group-change-history/volunteer-group-change-history-listing/volunteer-group-change-history-listing.component';
import { VolunteerGroupChangeHistoryAddEditComponent } from './scheduler-data-components/volunteer-group-change-history/volunteer-group-change-history-add-edit/volunteer-group-change-history-add-edit.component';
import { VolunteerGroupChangeHistoryDetailComponent } from './scheduler-data-components/volunteer-group-change-history/volunteer-group-change-history-detail/volunteer-group-change-history-detail.component';
import { VolunteerGroupChangeHistoryTableComponent } from './scheduler-data-components/volunteer-group-change-history/volunteer-group-change-history-table/volunteer-group-change-history-table.component';
import { VolunteerGroupMemberListingComponent } from './scheduler-data-components/volunteer-group-member/volunteer-group-member-listing/volunteer-group-member-listing.component';
import { VolunteerGroupMemberAddEditComponent } from './scheduler-data-components/volunteer-group-member/volunteer-group-member-add-edit/volunteer-group-member-add-edit.component';
import { VolunteerGroupMemberDetailComponent } from './scheduler-data-components/volunteer-group-member/volunteer-group-member-detail/volunteer-group-member-detail.component';
import { VolunteerGroupMemberTableComponent } from './scheduler-data-components/volunteer-group-member/volunteer-group-member-table/volunteer-group-member-table.component';
import { VolunteerGroupMemberChangeHistoryListingComponent } from './scheduler-data-components/volunteer-group-member-change-history/volunteer-group-member-change-history-listing/volunteer-group-member-change-history-listing.component';
import { VolunteerGroupMemberChangeHistoryAddEditComponent } from './scheduler-data-components/volunteer-group-member-change-history/volunteer-group-member-change-history-add-edit/volunteer-group-member-change-history-add-edit.component';
import { VolunteerGroupMemberChangeHistoryDetailComponent } from './scheduler-data-components/volunteer-group-member-change-history/volunteer-group-member-change-history-detail/volunteer-group-member-change-history-detail.component';
import { VolunteerGroupMemberChangeHistoryTableComponent } from './scheduler-data-components/volunteer-group-member-change-history/volunteer-group-member-change-history-table/volunteer-group-member-change-history-table.component';
import { VolunteerProfileListingComponent } from './scheduler-data-components/volunteer-profile/volunteer-profile-listing/volunteer-profile-listing.component';
import { VolunteerProfileAddEditComponent } from './scheduler-data-components/volunteer-profile/volunteer-profile-add-edit/volunteer-profile-add-edit.component';
import { VolunteerProfileDetailComponent } from './scheduler-data-components/volunteer-profile/volunteer-profile-detail/volunteer-profile-detail.component';
import { VolunteerProfileTableComponent } from './scheduler-data-components/volunteer-profile/volunteer-profile-table/volunteer-profile-table.component';
import { VolunteerProfileChangeHistoryListingComponent } from './scheduler-data-components/volunteer-profile-change-history/volunteer-profile-change-history-listing/volunteer-profile-change-history-listing.component';
import { VolunteerProfileChangeHistoryAddEditComponent } from './scheduler-data-components/volunteer-profile-change-history/volunteer-profile-change-history-add-edit/volunteer-profile-change-history-add-edit.component';
import { VolunteerProfileChangeHistoryDetailComponent } from './scheduler-data-components/volunteer-profile-change-history/volunteer-profile-change-history-detail/volunteer-profile-change-history-detail.component';
import { VolunteerProfileChangeHistoryTableComponent } from './scheduler-data-components/volunteer-profile-change-history/volunteer-profile-change-history-table/volunteer-profile-change-history-table.component';
import { VolunteerStatusListingComponent } from './scheduler-data-components/volunteer-status/volunteer-status-listing/volunteer-status-listing.component';
import { VolunteerStatusAddEditComponent } from './scheduler-data-components/volunteer-status/volunteer-status-add-edit/volunteer-status-add-edit.component';
import { VolunteerStatusDetailComponent } from './scheduler-data-components/volunteer-status/volunteer-status-detail/volunteer-status-detail.component';
import { VolunteerStatusTableComponent } from './scheduler-data-components/volunteer-status/volunteer-status-table/volunteer-status-table.component';
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
    IntelligenceModalComponent,
    ChangeHistoryViewerComponent,
    LocationMapComponent,

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

    HeaderComponent,
    SidebarComponent,

    SystemHealthComponent,


    //
    // Custom components
    //
    SchedulerCalendarComponent,
    EventAddEditModalComponent,
    RecurrenceBuilderComponent,
    TemplateManagerComponent,
    TemplateAddEditModalComponent,
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
    ResourceShiftTabComponent,
    ResourceShiftAddEditModalComponent,
    ResourceRatesTabComponent,
    ResourceRateOverrideAddModalComponent,
    ResourceAssignmentsTabComponent,
    ResourceContactsTabComponent,
    ResourceContactCustomAddEditModalComponent,
    ResourceNotificationsTabComponent,
    NotificationSubscriptionCustomAddEditModalComponent,

    //
    // Shift customization
    //
    ShiftCustomListingComponent,
    ShiftCustomDetailComponent,
    ShiftCustomTableComponent,
    ShiftCustomAddEditComponent,

    //
    // Shift Pattern customization
    //
    ShiftPatternCustomListingComponent,
    ShiftPatternCustomDetailComponent,
    ShiftPatternCustomTableComponent,
    ShiftPatternCustomAddEditComponent,
    ShiftPatternDayAddEditModalComponent,


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
    // Volunteer customization
    //
    VolunteerCustomListingComponent,
    VolunteerCustomDetailComponent,
    VolunteerCustomAddEditComponent,
    VolunteerCustomTableComponent,
    VolunteerOverviewTabComponent,
    VolunteerGroupsTabComponent,
    VolunteerAssignmentsTabComponent,
    VolunteerHoursTabComponent,
    VolunteerDashboardComponent,
    VolunteerCalendarComponent,

    //
    // Volunteer Group customization
    //
    VolunteerGroupCustomListingComponent,
    VolunteerGroupCustomDetailComponent,
    VolunteerGroupCustomAddEditComponent,
    VolunteerGroupCustomTableComponent,
    VolunteerGroupOverviewTabComponent,
    VolunteerGroupMembersTabComponent,
    VolunteerGroupAddMemberModalComponent,

    //
    // Financial customization
    //
    FinancialCustomDashboardComponent,
    FinancialTransactionCustomListingComponent,
    FinancialCategoryCustomListingComponent,
    FinancialTransactionCustomAddEditComponent,
    FinancialBudgetManagerComponent,
    FinancialCategoryCustomAddEditComponent,

    //
    // Invoice custom components
    //
    InvoiceCustomListingComponent,
    InvoiceCustomDetailComponent,
    InvoiceCustomAddEditComponent,

    //
    // Receipt custom components
    //
    ReceiptCustomListingComponent,
    ReceiptCustomDetailComponent,
    ReceiptCustomAddEditComponent,

    //
    // Payment custom components
    //
    PaymentCustomListingComponent,
    PaymentCustomDetailComponent,
    PaymentCustomAddEditComponent,

    //
    // Budget report & Deposit manager
    //
    BudgetReportComponent,
    DepositManagerComponent,

    //
    // P2 Financial Reports
    //
    PnlReportComponent,
    AccountantReportsComponent,

    //
    // P3 Nice-to-Haves
    //
    RentalAgreementTrackerComponent,
    FiscalPeriodCloseComponent,


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
    // Scheduling Target customization
    //
    SchedulingTargetCustomListingComponent,
    SchedulingTargetCustomTableComponent,
    SchedulingTargetCustomDetailComponent,


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
    ContactRelationshipEditModalComponent,
    ContactRelationshipsTabComponent,
    ContactFinancialsTabComponent,
    ContactScheduleTabComponent,
    ConstituentJourneyUpdateModalComponent,




    //
    // Beginning of declarations for Scheduler Data Components
//
AccountTypeListingComponent,
AccountTypeAddEditComponent,
AccountTypeDetailComponent,
AccountTypeTableComponent,
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
AttributeDefinitionChangeHistoryListingComponent,
AttributeDefinitionChangeHistoryAddEditComponent,
AttributeDefinitionChangeHistoryDetailComponent,
AttributeDefinitionChangeHistoryTableComponent,
AttributeDefinitionEntityListingComponent,
AttributeDefinitionEntityAddEditComponent,
AttributeDefinitionEntityDetailComponent,
AttributeDefinitionEntityTableComponent,
AttributeDefinitionTypeListingComponent,
AttributeDefinitionTypeAddEditComponent,
AttributeDefinitionTypeDetailComponent,
AttributeDefinitionTypeTableComponent,
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
BudgetListingComponent,
BudgetAddEditComponent,
BudgetDetailComponent,
BudgetTableComponent,
BudgetChangeHistoryListingComponent,
BudgetChangeHistoryAddEditComponent,
BudgetChangeHistoryDetailComponent,
BudgetChangeHistoryTableComponent,
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
ChargeStatusChangeHistoryListingComponent,
ChargeStatusChangeHistoryAddEditComponent,
ChargeStatusChangeHistoryDetailComponent,
ChargeStatusChangeHistoryTableComponent,
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
DocumentListingComponent,
DocumentAddEditComponent,
DocumentDetailComponent,
DocumentTableComponent,
DocumentChangeHistoryListingComponent,
DocumentChangeHistoryAddEditComponent,
DocumentChangeHistoryDetailComponent,
DocumentChangeHistoryTableComponent,
DocumentTypeListingComponent,
DocumentTypeAddEditComponent,
DocumentTypeDetailComponent,
DocumentTypeTableComponent,
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
FinancialCategoryListingComponent,
FinancialCategoryAddEditComponent,
FinancialCategoryDetailComponent,
FinancialCategoryTableComponent,
FinancialCategoryChangeHistoryListingComponent,
FinancialCategoryChangeHistoryAddEditComponent,
FinancialCategoryChangeHistoryDetailComponent,
FinancialCategoryChangeHistoryTableComponent,
FinancialOfficeListingComponent,
FinancialOfficeAddEditComponent,
FinancialOfficeDetailComponent,
FinancialOfficeTableComponent,
FinancialOfficeChangeHistoryListingComponent,
FinancialOfficeChangeHistoryAddEditComponent,
FinancialOfficeChangeHistoryDetailComponent,
FinancialOfficeChangeHistoryTableComponent,
FinancialTransactionListingComponent,
FinancialTransactionAddEditComponent,
FinancialTransactionDetailComponent,
FinancialTransactionTableComponent,
FinancialTransactionChangeHistoryListingComponent,
FinancialTransactionChangeHistoryAddEditComponent,
FinancialTransactionChangeHistoryDetailComponent,
FinancialTransactionChangeHistoryTableComponent,
FiscalPeriodListingComponent,
FiscalPeriodAddEditComponent,
FiscalPeriodDetailComponent,
FiscalPeriodTableComponent,
FiscalPeriodChangeHistoryListingComponent,
FiscalPeriodChangeHistoryAddEditComponent,
FiscalPeriodChangeHistoryDetailComponent,
FiscalPeriodChangeHistoryTableComponent,
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
InvoiceListingComponent,
InvoiceAddEditComponent,
InvoiceDetailComponent,
InvoiceTableComponent,
InvoiceChangeHistoryListingComponent,
InvoiceChangeHistoryAddEditComponent,
InvoiceChangeHistoryDetailComponent,
InvoiceChangeHistoryTableComponent,
InvoiceLineItemListingComponent,
InvoiceLineItemAddEditComponent,
InvoiceLineItemDetailComponent,
InvoiceLineItemTableComponent,
InvoiceStatusListingComponent,
InvoiceStatusAddEditComponent,
InvoiceStatusDetailComponent,
InvoiceStatusTableComponent,
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
PaymentMethodListingComponent,
PaymentMethodAddEditComponent,
PaymentMethodDetailComponent,
PaymentMethodTableComponent,
PaymentProviderListingComponent,
PaymentProviderAddEditComponent,
PaymentProviderDetailComponent,
PaymentProviderTableComponent,
PaymentProviderChangeHistoryListingComponent,
PaymentProviderChangeHistoryAddEditComponent,
PaymentProviderChangeHistoryDetailComponent,
PaymentProviderChangeHistoryTableComponent,
PaymentTransactionListingComponent,
PaymentTransactionAddEditComponent,
PaymentTransactionDetailComponent,
PaymentTransactionTableComponent,
PaymentTransactionChangeHistoryListingComponent,
PaymentTransactionChangeHistoryAddEditComponent,
PaymentTransactionChangeHistoryDetailComponent,
PaymentTransactionChangeHistoryTableComponent,
PaymentTypeListingComponent,
PaymentTypeAddEditComponent,
PaymentTypeDetailComponent,
PaymentTypeTableComponent,
PaymentTypeChangeHistoryListingComponent,
PaymentTypeChangeHistoryAddEditComponent,
PaymentTypeChangeHistoryDetailComponent,
PaymentTypeChangeHistoryTableComponent,
PeriodStatusListingComponent,
PeriodStatusAddEditComponent,
PeriodStatusDetailComponent,
PeriodStatusTableComponent,
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
ReceiptListingComponent,
ReceiptAddEditComponent,
ReceiptDetailComponent,
ReceiptTableComponent,
ReceiptChangeHistoryListingComponent,
ReceiptChangeHistoryAddEditComponent,
ReceiptChangeHistoryDetailComponent,
ReceiptChangeHistoryTableComponent,
ReceiptTypeListingComponent,
ReceiptTypeAddEditComponent,
ReceiptTypeDetailComponent,
ReceiptTypeTableComponent,
ReceiptTypeChangeHistoryListingComponent,
ReceiptTypeChangeHistoryAddEditComponent,
ReceiptTypeChangeHistoryDetailComponent,
ReceiptTypeChangeHistoryTableComponent,
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
TaxCodeListingComponent,
TaxCodeAddEditComponent,
TaxCodeDetailComponent,
TaxCodeTableComponent,
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
VolunteerGroupListingComponent,
VolunteerGroupAddEditComponent,
VolunteerGroupDetailComponent,
VolunteerGroupTableComponent,
VolunteerGroupChangeHistoryListingComponent,
VolunteerGroupChangeHistoryAddEditComponent,
VolunteerGroupChangeHistoryDetailComponent,
VolunteerGroupChangeHistoryTableComponent,
VolunteerGroupMemberListingComponent,
VolunteerGroupMemberAddEditComponent,
VolunteerGroupMemberDetailComponent,
VolunteerGroupMemberTableComponent,
VolunteerGroupMemberChangeHistoryListingComponent,
VolunteerGroupMemberChangeHistoryAddEditComponent,
VolunteerGroupMemberChangeHistoryDetailComponent,
VolunteerGroupMemberChangeHistoryTableComponent,
VolunteerProfileListingComponent,
VolunteerProfileAddEditComponent,
VolunteerProfileDetailComponent,
VolunteerProfileTableComponent,
VolunteerProfileChangeHistoryListingComponent,
VolunteerProfileChangeHistoryAddEditComponent,
VolunteerProfileChangeHistoryDetailComponent,
VolunteerProfileChangeHistoryTableComponent,
VolunteerStatusListingComponent,
VolunteerStatusAddEditComponent,
VolunteerStatusDetailComponent,
VolunteerStatusTableComponent,
//
    // End of declarations for Scheduler Data Components
    //


    //
    // Custom components
    //

    ResetPasswordComponent,
    NewUserComponent,
    OverviewManagerTabComponent,
    OverviewDispatcherTabComponent,
    OverviewSchedulerTabComponent,
    OverviewCoordinatorTabComponent,
    IntelligenceModalComponent,
    ChangeHistoryViewerComponent,
    LocationMapComponent,
    ConfirmationDialogComponent,
    InputDialogComponent,

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
    InputDialogService,
    CrewWithMembersService,
    AssignmentService,
    SchedulerHelperService,
    SystemHealthService,
    IntelligenceService,
    RagProviderResolver,
    GeminiGroundingProvider,

    //
    // Pipes
    //
    ContactFullNamePipe,
    ContrastColorPipe,



    //
    // Beginning of provider declarations for Scheduler Data Services
//
SchedulerDataServiceManagerService,
AccountTypeService,
AppealService,
AppealChangeHistoryService,
AssignmentRoleService,
AssignmentRoleQualificationRequirementService,
AssignmentRoleQualificationRequirementChangeHistoryService,
AssignmentStatusService,
AttributeDefinitionService,
AttributeDefinitionChangeHistoryService,
AttributeDefinitionEntityService,
AttributeDefinitionTypeService,
BatchService,
BatchChangeHistoryService,
BatchStatusService,
BookingSourceTypeService,
BudgetService,
BudgetChangeHistoryService,
CalendarService,
CalendarChangeHistoryService,
CampaignService,
CampaignChangeHistoryService,
ChargeStatusService,
ChargeStatusChangeHistoryService,
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
DocumentService,
DocumentChangeHistoryService,
DocumentTypeService,
EventCalendarService,
EventChargeService,
EventChargeChangeHistoryService,
EventResourceAssignmentService,
EventResourceAssignmentChangeHistoryService,
EventStatusService,
FinancialCategoryService,
FinancialCategoryChangeHistoryService,
FinancialOfficeService,
FinancialOfficeChangeHistoryService,
FinancialTransactionService,
FinancialTransactionChangeHistoryService,
FiscalPeriodService,
FiscalPeriodChangeHistoryService,
FundService,
FundChangeHistoryService,
GiftService,
GiftChangeHistoryService,
HouseholdService,
HouseholdChangeHistoryService,
IconService,
InteractionTypeService,
InvoiceService,
InvoiceChangeHistoryService,
InvoiceLineItemService,
InvoiceStatusService,
NotificationSubscriptionService,
NotificationSubscriptionChangeHistoryService,
NotificationTypeService,
OfficeService,
OfficeChangeHistoryService,
OfficeContactService,
OfficeContactChangeHistoryService,
OfficeTypeService,
PaymentMethodService,
PaymentProviderService,
PaymentProviderChangeHistoryService,
PaymentTransactionService,
PaymentTransactionChangeHistoryService,
PaymentTypeService,
PaymentTypeChangeHistoryService,
PeriodStatusService,
PledgeService,
PledgeChangeHistoryService,
PriorityService,
QualificationService,
RateSheetService,
RateSheetChangeHistoryService,
RateTypeService,
ReceiptService,
ReceiptChangeHistoryService,
ReceiptTypeService,
ReceiptTypeChangeHistoryService,
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
TaxCodeService,
TenantProfileService,
TenantProfileChangeHistoryService,
TimeZoneService,
TributeService,
TributeChangeHistoryService,
TributeTypeService,
VolunteerGroupService,
VolunteerGroupChangeHistoryService,
VolunteerGroupMemberService,
VolunteerGroupMemberChangeHistoryService,
VolunteerProfileService,
VolunteerProfileChangeHistoryService,
VolunteerStatusService,
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
