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
import { OverviewComponent } from './components/overview/overview.component'
import { SchedulerCalendarComponent } from './components/scheduler/scheduler-calendar/scheduler-calendar.component';
import { AdministrationComponent } from './components/administration/administration.component';
import { TemplateManagerComponent } from './components/scheduler/template-manager/template-manager.component';

import { RateSheetCustomListingComponent } from './components/rate-sheet-custom/rate-sheet-custom-listing/rate-sheet-custom-listing.component';

//
// Resource custom optimizations
//
import { ResourceCustomListingComponent } from './components/resource-custom/resource-custom-listing/resource-custom-listing.component';
import { ResourceCustomDetailComponent } from './components/resource-custom/resource-custom-detail/resource-custom-detail.component';


//
// Crew custom optimizations
//
import { CrewCustomListingComponent } from './components/crew-custom/crew-custom-listing/crew-custom-listing.component';
import { CrewCustomDetailComponent } from './components/crew-custom/crew-custom-detail/crew-custom-detail.component';

//
// Office custom optimizations
//
import { OfficeCustomListingComponent } from './components/office-custom/office-custom-listing/office-custom-listing.component';
import { OfficeCustomDetailComponent } from './components/office-custom/office-custom-detail/office-custom-detail.component';


//
// Contact custom optimizations
//
import { ContactCustomListingComponent } from './components/contact-custom/contact-custom-listing/contact-custom-listing.component';
import { ContactCustomDetailComponent } from './components/contact-custom/contact-custom-detail/contact-custom-detail.component';


//
// Calendar custom optimizations
//
import { CalendarCustomListingComponent } from './components/calendar-custom/calendar-custom-listing/calendar-custom-listing.component';
import { CalendarCustomDetailComponent } from './components/calendar-custom/calendar-custom-detail/calendar-custom-detail.component';


//
// Client optimizations
//
import { ClientCustomDetailComponent } from './components/client-custom/client-custom-detail/client-custom-detail.component';
import { ClientCustomListingComponent } from './components/client-custom/client-custom-listing/client-custom-listing.component';

//
// Scheduling Target custom optimizations
//
import { SchedulingTargetCustomListingComponent } from './components/scheduling-target-custom/scheduling-target-custom-listing/scheduling-target-custom-listing.component';
import { SchedulingTargetCustomDetailComponent } from './components/scheduling-target-custom/scheduling-target-custom-detail/scheduling-target-custom-detail.component';




//
// Beginning of imports for Scheduler Data Components
//
import { AppealListingComponent } from './scheduler-data-components/appeal/appeal-listing/appeal-listing.component';
import { AppealDetailComponent } from './scheduler-data-components/appeal/appeal-detail/appeal-detail.component';
import { AppealChangeHistoryListingComponent } from './scheduler-data-components/appeal-change-history/appeal-change-history-listing/appeal-change-history-listing.component';
import { AppealChangeHistoryDetailComponent } from './scheduler-data-components/appeal-change-history/appeal-change-history-detail/appeal-change-history-detail.component';
import { AssignmentRoleListingComponent } from './scheduler-data-components/assignment-role/assignment-role-listing/assignment-role-listing.component';
import { AssignmentRoleDetailComponent } from './scheduler-data-components/assignment-role/assignment-role-detail/assignment-role-detail.component';
import { AssignmentRoleQualificationRequirementListingComponent } from './scheduler-data-components/assignment-role-qualification-requirement/assignment-role-qualification-requirement-listing/assignment-role-qualification-requirement-listing.component';
import { AssignmentRoleQualificationRequirementDetailComponent } from './scheduler-data-components/assignment-role-qualification-requirement/assignment-role-qualification-requirement-detail/assignment-role-qualification-requirement-detail.component';
import { AssignmentRoleQualificationRequirementChangeHistoryListingComponent } from './scheduler-data-components/assignment-role-qualification-requirement-change-history/assignment-role-qualification-requirement-change-history-listing/assignment-role-qualification-requirement-change-history-listing.component';
import { AssignmentRoleQualificationRequirementChangeHistoryDetailComponent } from './scheduler-data-components/assignment-role-qualification-requirement-change-history/assignment-role-qualification-requirement-change-history-detail/assignment-role-qualification-requirement-change-history-detail.component';
import { AssignmentStatusListingComponent } from './scheduler-data-components/assignment-status/assignment-status-listing/assignment-status-listing.component';
import { AssignmentStatusDetailComponent } from './scheduler-data-components/assignment-status/assignment-status-detail/assignment-status-detail.component';
import { AttributeDefinitionListingComponent } from './scheduler-data-components/attribute-definition/attribute-definition-listing/attribute-definition-listing.component';
import { AttributeDefinitionDetailComponent } from './scheduler-data-components/attribute-definition/attribute-definition-detail/attribute-definition-detail.component';
import { AttributeDefinitionChangeHistoryListingComponent } from './scheduler-data-components/attribute-definition-change-history/attribute-definition-change-history-listing/attribute-definition-change-history-listing.component';
import { AttributeDefinitionChangeHistoryDetailComponent } from './scheduler-data-components/attribute-definition-change-history/attribute-definition-change-history-detail/attribute-definition-change-history-detail.component';
import { AttributeDefinitionEntityListingComponent } from './scheduler-data-components/attribute-definition-entity/attribute-definition-entity-listing/attribute-definition-entity-listing.component';
import { AttributeDefinitionEntityDetailComponent } from './scheduler-data-components/attribute-definition-entity/attribute-definition-entity-detail/attribute-definition-entity-detail.component';
import { AttributeDefinitionTypeListingComponent } from './scheduler-data-components/attribute-definition-type/attribute-definition-type-listing/attribute-definition-type-listing.component';
import { AttributeDefinitionTypeDetailComponent } from './scheduler-data-components/attribute-definition-type/attribute-definition-type-detail/attribute-definition-type-detail.component';
import { BatchListingComponent } from './scheduler-data-components/batch/batch-listing/batch-listing.component';
import { BatchDetailComponent } from './scheduler-data-components/batch/batch-detail/batch-detail.component';
import { BatchChangeHistoryListingComponent } from './scheduler-data-components/batch-change-history/batch-change-history-listing/batch-change-history-listing.component';
import { BatchChangeHistoryDetailComponent } from './scheduler-data-components/batch-change-history/batch-change-history-detail/batch-change-history-detail.component';
import { BatchStatusListingComponent } from './scheduler-data-components/batch-status/batch-status-listing/batch-status-listing.component';
import { BatchStatusDetailComponent } from './scheduler-data-components/batch-status/batch-status-detail/batch-status-detail.component';
import { BookingSourceTypeListingComponent } from './scheduler-data-components/booking-source-type/booking-source-type-listing/booking-source-type-listing.component';
import { BookingSourceTypeDetailComponent } from './scheduler-data-components/booking-source-type/booking-source-type-detail/booking-source-type-detail.component';
import { CalendarListingComponent } from './scheduler-data-components/calendar/calendar-listing/calendar-listing.component';
import { CalendarDetailComponent } from './scheduler-data-components/calendar/calendar-detail/calendar-detail.component';
import { CalendarChangeHistoryListingComponent } from './scheduler-data-components/calendar-change-history/calendar-change-history-listing/calendar-change-history-listing.component';
import { CalendarChangeHistoryDetailComponent } from './scheduler-data-components/calendar-change-history/calendar-change-history-detail/calendar-change-history-detail.component';
import { CampaignListingComponent } from './scheduler-data-components/campaign/campaign-listing/campaign-listing.component';
import { CampaignDetailComponent } from './scheduler-data-components/campaign/campaign-detail/campaign-detail.component';
import { CampaignChangeHistoryListingComponent } from './scheduler-data-components/campaign-change-history/campaign-change-history-listing/campaign-change-history-listing.component';
import { CampaignChangeHistoryDetailComponent } from './scheduler-data-components/campaign-change-history/campaign-change-history-detail/campaign-change-history-detail.component';
import { ChargeStatusListingComponent } from './scheduler-data-components/charge-status/charge-status-listing/charge-status-listing.component';
import { ChargeStatusDetailComponent } from './scheduler-data-components/charge-status/charge-status-detail/charge-status-detail.component';
import { ChargeTypeListingComponent } from './scheduler-data-components/charge-type/charge-type-listing/charge-type-listing.component';
import { ChargeTypeDetailComponent } from './scheduler-data-components/charge-type/charge-type-detail/charge-type-detail.component';
import { ChargeTypeChangeHistoryListingComponent } from './scheduler-data-components/charge-type-change-history/charge-type-change-history-listing/charge-type-change-history-listing.component';
import { ChargeTypeChangeHistoryDetailComponent } from './scheduler-data-components/charge-type-change-history/charge-type-change-history-detail/charge-type-change-history-detail.component';
import { ClientListingComponent } from './scheduler-data-components/client/client-listing/client-listing.component';
import { ClientDetailComponent } from './scheduler-data-components/client/client-detail/client-detail.component';
import { ClientChangeHistoryListingComponent } from './scheduler-data-components/client-change-history/client-change-history-listing/client-change-history-listing.component';
import { ClientChangeHistoryDetailComponent } from './scheduler-data-components/client-change-history/client-change-history-detail/client-change-history-detail.component';
import { ClientContactListingComponent } from './scheduler-data-components/client-contact/client-contact-listing/client-contact-listing.component';
import { ClientContactDetailComponent } from './scheduler-data-components/client-contact/client-contact-detail/client-contact-detail.component';
import { ClientContactChangeHistoryListingComponent } from './scheduler-data-components/client-contact-change-history/client-contact-change-history-listing/client-contact-change-history-listing.component';
import { ClientContactChangeHistoryDetailComponent } from './scheduler-data-components/client-contact-change-history/client-contact-change-history-detail/client-contact-change-history-detail.component';
import { ClientTypeListingComponent } from './scheduler-data-components/client-type/client-type-listing/client-type-listing.component';
import { ClientTypeDetailComponent } from './scheduler-data-components/client-type/client-type-detail/client-type-detail.component';
import { ConstituentListingComponent } from './scheduler-data-components/constituent/constituent-listing/constituent-listing.component';
import { ConstituentDetailComponent } from './scheduler-data-components/constituent/constituent-detail/constituent-detail.component';
import { ConstituentChangeHistoryListingComponent } from './scheduler-data-components/constituent-change-history/constituent-change-history-listing/constituent-change-history-listing.component';
import { ConstituentChangeHistoryDetailComponent } from './scheduler-data-components/constituent-change-history/constituent-change-history-detail/constituent-change-history-detail.component';
import { ConstituentJourneyStageListingComponent } from './scheduler-data-components/constituent-journey-stage/constituent-journey-stage-listing/constituent-journey-stage-listing.component';
import { ConstituentJourneyStageDetailComponent } from './scheduler-data-components/constituent-journey-stage/constituent-journey-stage-detail/constituent-journey-stage-detail.component';
import { ConstituentJourneyStageChangeHistoryListingComponent } from './scheduler-data-components/constituent-journey-stage-change-history/constituent-journey-stage-change-history-listing/constituent-journey-stage-change-history-listing.component';
import { ConstituentJourneyStageChangeHistoryDetailComponent } from './scheduler-data-components/constituent-journey-stage-change-history/constituent-journey-stage-change-history-detail/constituent-journey-stage-change-history-detail.component';
import { ContactListingComponent } from './scheduler-data-components/contact/contact-listing/contact-listing.component';
import { ContactDetailComponent } from './scheduler-data-components/contact/contact-detail/contact-detail.component';
import { ContactChangeHistoryListingComponent } from './scheduler-data-components/contact-change-history/contact-change-history-listing/contact-change-history-listing.component';
import { ContactChangeHistoryDetailComponent } from './scheduler-data-components/contact-change-history/contact-change-history-detail/contact-change-history-detail.component';
import { ContactContactListingComponent } from './scheduler-data-components/contact-contact/contact-contact-listing/contact-contact-listing.component';
import { ContactContactDetailComponent } from './scheduler-data-components/contact-contact/contact-contact-detail/contact-contact-detail.component';
import { ContactContactChangeHistoryListingComponent } from './scheduler-data-components/contact-contact-change-history/contact-contact-change-history-listing/contact-contact-change-history-listing.component';
import { ContactContactChangeHistoryDetailComponent } from './scheduler-data-components/contact-contact-change-history/contact-contact-change-history-detail/contact-contact-change-history-detail.component';
import { ContactInteractionListingComponent } from './scheduler-data-components/contact-interaction/contact-interaction-listing/contact-interaction-listing.component';
import { ContactInteractionDetailComponent } from './scheduler-data-components/contact-interaction/contact-interaction-detail/contact-interaction-detail.component';
import { ContactInteractionChangeHistoryListingComponent } from './scheduler-data-components/contact-interaction-change-history/contact-interaction-change-history-listing/contact-interaction-change-history-listing.component';
import { ContactInteractionChangeHistoryDetailComponent } from './scheduler-data-components/contact-interaction-change-history/contact-interaction-change-history-detail/contact-interaction-change-history-detail.component';
import { ContactMethodListingComponent } from './scheduler-data-components/contact-method/contact-method-listing/contact-method-listing.component';
import { ContactMethodDetailComponent } from './scheduler-data-components/contact-method/contact-method-detail/contact-method-detail.component';
import { ContactTagListingComponent } from './scheduler-data-components/contact-tag/contact-tag-listing/contact-tag-listing.component';
import { ContactTagDetailComponent } from './scheduler-data-components/contact-tag/contact-tag-detail/contact-tag-detail.component';
import { ContactTagChangeHistoryListingComponent } from './scheduler-data-components/contact-tag-change-history/contact-tag-change-history-listing/contact-tag-change-history-listing.component';
import { ContactTagChangeHistoryDetailComponent } from './scheduler-data-components/contact-tag-change-history/contact-tag-change-history-detail/contact-tag-change-history-detail.component';
import { ContactTypeListingComponent } from './scheduler-data-components/contact-type/contact-type-listing/contact-type-listing.component';
import { ContactTypeDetailComponent } from './scheduler-data-components/contact-type/contact-type-detail/contact-type-detail.component';
import { CountryListingComponent } from './scheduler-data-components/country/country-listing/country-listing.component';
import { CountryDetailComponent } from './scheduler-data-components/country/country-detail/country-detail.component';
import { CrewListingComponent } from './scheduler-data-components/crew/crew-listing/crew-listing.component';
import { CrewDetailComponent } from './scheduler-data-components/crew/crew-detail/crew-detail.component';
import { CrewChangeHistoryListingComponent } from './scheduler-data-components/crew-change-history/crew-change-history-listing/crew-change-history-listing.component';
import { CrewChangeHistoryDetailComponent } from './scheduler-data-components/crew-change-history/crew-change-history-detail/crew-change-history-detail.component';
import { CrewMemberListingComponent } from './scheduler-data-components/crew-member/crew-member-listing/crew-member-listing.component';
import { CrewMemberDetailComponent } from './scheduler-data-components/crew-member/crew-member-detail/crew-member-detail.component';
import { CrewMemberChangeHistoryListingComponent } from './scheduler-data-components/crew-member-change-history/crew-member-change-history-listing/crew-member-change-history-listing.component';
import { CrewMemberChangeHistoryDetailComponent } from './scheduler-data-components/crew-member-change-history/crew-member-change-history-detail/crew-member-change-history-detail.component';
import { CurrencyListingComponent } from './scheduler-data-components/currency/currency-listing/currency-listing.component';
import { CurrencyDetailComponent } from './scheduler-data-components/currency/currency-detail/currency-detail.component';
import { DependencyTypeListingComponent } from './scheduler-data-components/dependency-type/dependency-type-listing/dependency-type-listing.component';
import { DependencyTypeDetailComponent } from './scheduler-data-components/dependency-type/dependency-type-detail/dependency-type-detail.component';
import { EventCalendarListingComponent } from './scheduler-data-components/event-calendar/event-calendar-listing/event-calendar-listing.component';
import { EventCalendarDetailComponent } from './scheduler-data-components/event-calendar/event-calendar-detail/event-calendar-detail.component';
import { EventChargeListingComponent } from './scheduler-data-components/event-charge/event-charge-listing/event-charge-listing.component';
import { EventChargeDetailComponent } from './scheduler-data-components/event-charge/event-charge-detail/event-charge-detail.component';
import { EventChargeChangeHistoryListingComponent } from './scheduler-data-components/event-charge-change-history/event-charge-change-history-listing/event-charge-change-history-listing.component';
import { EventChargeChangeHistoryDetailComponent } from './scheduler-data-components/event-charge-change-history/event-charge-change-history-detail/event-charge-change-history-detail.component';
import { EventResourceAssignmentListingComponent } from './scheduler-data-components/event-resource-assignment/event-resource-assignment-listing/event-resource-assignment-listing.component';
import { EventResourceAssignmentDetailComponent } from './scheduler-data-components/event-resource-assignment/event-resource-assignment-detail/event-resource-assignment-detail.component';
import { EventResourceAssignmentChangeHistoryListingComponent } from './scheduler-data-components/event-resource-assignment-change-history/event-resource-assignment-change-history-listing/event-resource-assignment-change-history-listing.component';
import { EventResourceAssignmentChangeHistoryDetailComponent } from './scheduler-data-components/event-resource-assignment-change-history/event-resource-assignment-change-history-detail/event-resource-assignment-change-history-detail.component';
import { EventStatusListingComponent } from './scheduler-data-components/event-status/event-status-listing/event-status-listing.component';
import { EventStatusDetailComponent } from './scheduler-data-components/event-status/event-status-detail/event-status-detail.component';
import { FundListingComponent } from './scheduler-data-components/fund/fund-listing/fund-listing.component';
import { FundDetailComponent } from './scheduler-data-components/fund/fund-detail/fund-detail.component';
import { FundChangeHistoryListingComponent } from './scheduler-data-components/fund-change-history/fund-change-history-listing/fund-change-history-listing.component';
import { FundChangeHistoryDetailComponent } from './scheduler-data-components/fund-change-history/fund-change-history-detail/fund-change-history-detail.component';
import { GiftListingComponent } from './scheduler-data-components/gift/gift-listing/gift-listing.component';
import { GiftDetailComponent } from './scheduler-data-components/gift/gift-detail/gift-detail.component';
import { GiftChangeHistoryListingComponent } from './scheduler-data-components/gift-change-history/gift-change-history-listing/gift-change-history-listing.component';
import { GiftChangeHistoryDetailComponent } from './scheduler-data-components/gift-change-history/gift-change-history-detail/gift-change-history-detail.component';
import { HouseholdListingComponent } from './scheduler-data-components/household/household-listing/household-listing.component';
import { HouseholdDetailComponent } from './scheduler-data-components/household/household-detail/household-detail.component';
import { HouseholdChangeHistoryListingComponent } from './scheduler-data-components/household-change-history/household-change-history-listing/household-change-history-listing.component';
import { HouseholdChangeHistoryDetailComponent } from './scheduler-data-components/household-change-history/household-change-history-detail/household-change-history-detail.component';
import { IconListingComponent } from './scheduler-data-components/icon/icon-listing/icon-listing.component';
import { IconDetailComponent } from './scheduler-data-components/icon/icon-detail/icon-detail.component';
import { InteractionTypeListingComponent } from './scheduler-data-components/interaction-type/interaction-type-listing/interaction-type-listing.component';
import { InteractionTypeDetailComponent } from './scheduler-data-components/interaction-type/interaction-type-detail/interaction-type-detail.component';
import { NotificationSubscriptionListingComponent } from './scheduler-data-components/notification-subscription/notification-subscription-listing/notification-subscription-listing.component';
import { NotificationSubscriptionDetailComponent } from './scheduler-data-components/notification-subscription/notification-subscription-detail/notification-subscription-detail.component';
import { NotificationSubscriptionChangeHistoryListingComponent } from './scheduler-data-components/notification-subscription-change-history/notification-subscription-change-history-listing/notification-subscription-change-history-listing.component';
import { NotificationSubscriptionChangeHistoryDetailComponent } from './scheduler-data-components/notification-subscription-change-history/notification-subscription-change-history-detail/notification-subscription-change-history-detail.component';
import { NotificationTypeListingComponent } from './scheduler-data-components/notification-type/notification-type-listing/notification-type-listing.component';
import { NotificationTypeDetailComponent } from './scheduler-data-components/notification-type/notification-type-detail/notification-type-detail.component';
import { OfficeListingComponent } from './scheduler-data-components/office/office-listing/office-listing.component';
import { OfficeDetailComponent } from './scheduler-data-components/office/office-detail/office-detail.component';
import { OfficeChangeHistoryListingComponent } from './scheduler-data-components/office-change-history/office-change-history-listing/office-change-history-listing.component';
import { OfficeChangeHistoryDetailComponent } from './scheduler-data-components/office-change-history/office-change-history-detail/office-change-history-detail.component';
import { OfficeContactListingComponent } from './scheduler-data-components/office-contact/office-contact-listing/office-contact-listing.component';
import { OfficeContactDetailComponent } from './scheduler-data-components/office-contact/office-contact-detail/office-contact-detail.component';
import { OfficeContactChangeHistoryListingComponent } from './scheduler-data-components/office-contact-change-history/office-contact-change-history-listing/office-contact-change-history-listing.component';
import { OfficeContactChangeHistoryDetailComponent } from './scheduler-data-components/office-contact-change-history/office-contact-change-history-detail/office-contact-change-history-detail.component';
import { OfficeTypeListingComponent } from './scheduler-data-components/office-type/office-type-listing/office-type-listing.component';
import { OfficeTypeDetailComponent } from './scheduler-data-components/office-type/office-type-detail/office-type-detail.component';
import { PaymentTypeListingComponent } from './scheduler-data-components/payment-type/payment-type-listing/payment-type-listing.component';
import { PaymentTypeDetailComponent } from './scheduler-data-components/payment-type/payment-type-detail/payment-type-detail.component';
import { PledgeListingComponent } from './scheduler-data-components/pledge/pledge-listing/pledge-listing.component';
import { PledgeDetailComponent } from './scheduler-data-components/pledge/pledge-detail/pledge-detail.component';
import { PledgeChangeHistoryListingComponent } from './scheduler-data-components/pledge-change-history/pledge-change-history-listing/pledge-change-history-listing.component';
import { PledgeChangeHistoryDetailComponent } from './scheduler-data-components/pledge-change-history/pledge-change-history-detail/pledge-change-history-detail.component';
import { PriorityListingComponent } from './scheduler-data-components/priority/priority-listing/priority-listing.component';
import { PriorityDetailComponent } from './scheduler-data-components/priority/priority-detail/priority-detail.component';
import { QualificationListingComponent } from './scheduler-data-components/qualification/qualification-listing/qualification-listing.component';
import { QualificationDetailComponent } from './scheduler-data-components/qualification/qualification-detail/qualification-detail.component';
import { RateSheetListingComponent } from './scheduler-data-components/rate-sheet/rate-sheet-listing/rate-sheet-listing.component';
import { RateSheetDetailComponent } from './scheduler-data-components/rate-sheet/rate-sheet-detail/rate-sheet-detail.component';
import { RateSheetChangeHistoryListingComponent } from './scheduler-data-components/rate-sheet-change-history/rate-sheet-change-history-listing/rate-sheet-change-history-listing.component';
import { RateSheetChangeHistoryDetailComponent } from './scheduler-data-components/rate-sheet-change-history/rate-sheet-change-history-detail/rate-sheet-change-history-detail.component';
import { RateTypeListingComponent } from './scheduler-data-components/rate-type/rate-type-listing/rate-type-listing.component';
import { RateTypeDetailComponent } from './scheduler-data-components/rate-type/rate-type-detail/rate-type-detail.component';
import { ReceiptTypeListingComponent } from './scheduler-data-components/receipt-type/receipt-type-listing/receipt-type-listing.component';
import { ReceiptTypeDetailComponent } from './scheduler-data-components/receipt-type/receipt-type-detail/receipt-type-detail.component';
import { RecurrenceExceptionListingComponent } from './scheduler-data-components/recurrence-exception/recurrence-exception-listing/recurrence-exception-listing.component';
import { RecurrenceExceptionDetailComponent } from './scheduler-data-components/recurrence-exception/recurrence-exception-detail/recurrence-exception-detail.component';
import { RecurrenceExceptionChangeHistoryListingComponent } from './scheduler-data-components/recurrence-exception-change-history/recurrence-exception-change-history-listing/recurrence-exception-change-history-listing.component';
import { RecurrenceExceptionChangeHistoryDetailComponent } from './scheduler-data-components/recurrence-exception-change-history/recurrence-exception-change-history-detail/recurrence-exception-change-history-detail.component';
import { RecurrenceFrequencyListingComponent } from './scheduler-data-components/recurrence-frequency/recurrence-frequency-listing/recurrence-frequency-listing.component';
import { RecurrenceFrequencyDetailComponent } from './scheduler-data-components/recurrence-frequency/recurrence-frequency-detail/recurrence-frequency-detail.component';
import { RecurrenceRuleListingComponent } from './scheduler-data-components/recurrence-rule/recurrence-rule-listing/recurrence-rule-listing.component';
import { RecurrenceRuleDetailComponent } from './scheduler-data-components/recurrence-rule/recurrence-rule-detail/recurrence-rule-detail.component';
import { RecurrenceRuleChangeHistoryListingComponent } from './scheduler-data-components/recurrence-rule-change-history/recurrence-rule-change-history-listing/recurrence-rule-change-history-listing.component';
import { RecurrenceRuleChangeHistoryDetailComponent } from './scheduler-data-components/recurrence-rule-change-history/recurrence-rule-change-history-detail/recurrence-rule-change-history-detail.component';
import { RelationshipTypeListingComponent } from './scheduler-data-components/relationship-type/relationship-type-listing/relationship-type-listing.component';
import { RelationshipTypeDetailComponent } from './scheduler-data-components/relationship-type/relationship-type-detail/relationship-type-detail.component';
import { ResourceListingComponent } from './scheduler-data-components/resource/resource-listing/resource-listing.component';
import { ResourceDetailComponent } from './scheduler-data-components/resource/resource-detail/resource-detail.component';
import { ResourceAvailabilityListingComponent } from './scheduler-data-components/resource-availability/resource-availability-listing/resource-availability-listing.component';
import { ResourceAvailabilityDetailComponent } from './scheduler-data-components/resource-availability/resource-availability-detail/resource-availability-detail.component';
import { ResourceAvailabilityChangeHistoryListingComponent } from './scheduler-data-components/resource-availability-change-history/resource-availability-change-history-listing/resource-availability-change-history-listing.component';
import { ResourceAvailabilityChangeHistoryDetailComponent } from './scheduler-data-components/resource-availability-change-history/resource-availability-change-history-detail/resource-availability-change-history-detail.component';
import { ResourceChangeHistoryListingComponent } from './scheduler-data-components/resource-change-history/resource-change-history-listing/resource-change-history-listing.component';
import { ResourceChangeHistoryDetailComponent } from './scheduler-data-components/resource-change-history/resource-change-history-detail/resource-change-history-detail.component';
import { ResourceContactListingComponent } from './scheduler-data-components/resource-contact/resource-contact-listing/resource-contact-listing.component';
import { ResourceContactDetailComponent } from './scheduler-data-components/resource-contact/resource-contact-detail/resource-contact-detail.component';
import { ResourceContactChangeHistoryListingComponent } from './scheduler-data-components/resource-contact-change-history/resource-contact-change-history-listing/resource-contact-change-history-listing.component';
import { ResourceContactChangeHistoryDetailComponent } from './scheduler-data-components/resource-contact-change-history/resource-contact-change-history-detail/resource-contact-change-history-detail.component';
import { ResourceQualificationListingComponent } from './scheduler-data-components/resource-qualification/resource-qualification-listing/resource-qualification-listing.component';
import { ResourceQualificationDetailComponent } from './scheduler-data-components/resource-qualification/resource-qualification-detail/resource-qualification-detail.component';
import { ResourceQualificationChangeHistoryListingComponent } from './scheduler-data-components/resource-qualification-change-history/resource-qualification-change-history-listing/resource-qualification-change-history-listing.component';
import { ResourceQualificationChangeHistoryDetailComponent } from './scheduler-data-components/resource-qualification-change-history/resource-qualification-change-history-detail/resource-qualification-change-history-detail.component';
import { ResourceShiftListingComponent } from './scheduler-data-components/resource-shift/resource-shift-listing/resource-shift-listing.component';
import { ResourceShiftDetailComponent } from './scheduler-data-components/resource-shift/resource-shift-detail/resource-shift-detail.component';
import { ResourceShiftChangeHistoryListingComponent } from './scheduler-data-components/resource-shift-change-history/resource-shift-change-history-listing/resource-shift-change-history-listing.component';
import { ResourceShiftChangeHistoryDetailComponent } from './scheduler-data-components/resource-shift-change-history/resource-shift-change-history-detail/resource-shift-change-history-detail.component';
import { ResourceTypeListingComponent } from './scheduler-data-components/resource-type/resource-type-listing/resource-type-listing.component';
import { ResourceTypeDetailComponent } from './scheduler-data-components/resource-type/resource-type-detail/resource-type-detail.component';
import { SalutationListingComponent } from './scheduler-data-components/salutation/salutation-listing/salutation-listing.component';
import { SalutationDetailComponent } from './scheduler-data-components/salutation/salutation-detail/salutation-detail.component';
import { ScheduledEventListingComponent } from './scheduler-data-components/scheduled-event/scheduled-event-listing/scheduled-event-listing.component';
import { ScheduledEventDetailComponent } from './scheduler-data-components/scheduled-event/scheduled-event-detail/scheduled-event-detail.component';
import { ScheduledEventChangeHistoryListingComponent } from './scheduler-data-components/scheduled-event-change-history/scheduled-event-change-history-listing/scheduled-event-change-history-listing.component';
import { ScheduledEventChangeHistoryDetailComponent } from './scheduler-data-components/scheduled-event-change-history/scheduled-event-change-history-detail/scheduled-event-change-history-detail.component';
import { ScheduledEventDependencyListingComponent } from './scheduler-data-components/scheduled-event-dependency/scheduled-event-dependency-listing/scheduled-event-dependency-listing.component';
import { ScheduledEventDependencyDetailComponent } from './scheduler-data-components/scheduled-event-dependency/scheduled-event-dependency-detail/scheduled-event-dependency-detail.component';
import { ScheduledEventDependencyChangeHistoryListingComponent } from './scheduler-data-components/scheduled-event-dependency-change-history/scheduled-event-dependency-change-history-listing/scheduled-event-dependency-change-history-listing.component';
import { ScheduledEventDependencyChangeHistoryDetailComponent } from './scheduler-data-components/scheduled-event-dependency-change-history/scheduled-event-dependency-change-history-detail/scheduled-event-dependency-change-history-detail.component';
import { ScheduledEventQualificationRequirementListingComponent } from './scheduler-data-components/scheduled-event-qualification-requirement/scheduled-event-qualification-requirement-listing/scheduled-event-qualification-requirement-listing.component';
import { ScheduledEventQualificationRequirementDetailComponent } from './scheduler-data-components/scheduled-event-qualification-requirement/scheduled-event-qualification-requirement-detail/scheduled-event-qualification-requirement-detail.component';
import { ScheduledEventQualificationRequirementChangeHistoryListingComponent } from './scheduler-data-components/scheduled-event-qualification-requirement-change-history/scheduled-event-qualification-requirement-change-history-listing/scheduled-event-qualification-requirement-change-history-listing.component';
import { ScheduledEventQualificationRequirementChangeHistoryDetailComponent } from './scheduler-data-components/scheduled-event-qualification-requirement-change-history/scheduled-event-qualification-requirement-change-history-detail/scheduled-event-qualification-requirement-change-history-detail.component';
import { ScheduledEventTemplateListingComponent } from './scheduler-data-components/scheduled-event-template/scheduled-event-template-listing/scheduled-event-template-listing.component';
import { ScheduledEventTemplateDetailComponent } from './scheduler-data-components/scheduled-event-template/scheduled-event-template-detail/scheduled-event-template-detail.component';
import { ScheduledEventTemplateChangeHistoryListingComponent } from './scheduler-data-components/scheduled-event-template-change-history/scheduled-event-template-change-history-listing/scheduled-event-template-change-history-listing.component';
import { ScheduledEventTemplateChangeHistoryDetailComponent } from './scheduler-data-components/scheduled-event-template-change-history/scheduled-event-template-change-history-detail/scheduled-event-template-change-history-detail.component';
import { ScheduledEventTemplateChargeListingComponent } from './scheduler-data-components/scheduled-event-template-charge/scheduled-event-template-charge-listing/scheduled-event-template-charge-listing.component';
import { ScheduledEventTemplateChargeDetailComponent } from './scheduler-data-components/scheduled-event-template-charge/scheduled-event-template-charge-detail/scheduled-event-template-charge-detail.component';
import { ScheduledEventTemplateChargeChangeHistoryListingComponent } from './scheduler-data-components/scheduled-event-template-charge-change-history/scheduled-event-template-charge-change-history-listing/scheduled-event-template-charge-change-history-listing.component';
import { ScheduledEventTemplateChargeChangeHistoryDetailComponent } from './scheduler-data-components/scheduled-event-template-charge-change-history/scheduled-event-template-charge-change-history-detail/scheduled-event-template-charge-change-history-detail.component';
import { ScheduledEventTemplateQualificationRequirementListingComponent } from './scheduler-data-components/scheduled-event-template-qualification-requirement/scheduled-event-template-qualification-requirement-listing/scheduled-event-template-qualification-requirement-listing.component';
import { ScheduledEventTemplateQualificationRequirementDetailComponent } from './scheduler-data-components/scheduled-event-template-qualification-requirement/scheduled-event-template-qualification-requirement-detail/scheduled-event-template-qualification-requirement-detail.component';
import { ScheduledEventTemplateQualificationRequirementChangeHistoryListingComponent } from './scheduler-data-components/scheduled-event-template-qualification-requirement-change-history/scheduled-event-template-qualification-requirement-change-history-listing/scheduled-event-template-qualification-requirement-change-history-listing.component';
import { ScheduledEventTemplateQualificationRequirementChangeHistoryDetailComponent } from './scheduler-data-components/scheduled-event-template-qualification-requirement-change-history/scheduled-event-template-qualification-requirement-change-history-detail/scheduled-event-template-qualification-requirement-change-history-detail.component';
import { SchedulingTargetListingComponent } from './scheduler-data-components/scheduling-target/scheduling-target-listing/scheduling-target-listing.component';
import { SchedulingTargetDetailComponent } from './scheduler-data-components/scheduling-target/scheduling-target-detail/scheduling-target-detail.component';
import { SchedulingTargetAddressListingComponent } from './scheduler-data-components/scheduling-target-address/scheduling-target-address-listing/scheduling-target-address-listing.component';
import { SchedulingTargetAddressDetailComponent } from './scheduler-data-components/scheduling-target-address/scheduling-target-address-detail/scheduling-target-address-detail.component';
import { SchedulingTargetAddressChangeHistoryListingComponent } from './scheduler-data-components/scheduling-target-address-change-history/scheduling-target-address-change-history-listing/scheduling-target-address-change-history-listing.component';
import { SchedulingTargetAddressChangeHistoryDetailComponent } from './scheduler-data-components/scheduling-target-address-change-history/scheduling-target-address-change-history-detail/scheduling-target-address-change-history-detail.component';
import { SchedulingTargetChangeHistoryListingComponent } from './scheduler-data-components/scheduling-target-change-history/scheduling-target-change-history-listing/scheduling-target-change-history-listing.component';
import { SchedulingTargetChangeHistoryDetailComponent } from './scheduler-data-components/scheduling-target-change-history/scheduling-target-change-history-detail/scheduling-target-change-history-detail.component';
import { SchedulingTargetContactListingComponent } from './scheduler-data-components/scheduling-target-contact/scheduling-target-contact-listing/scheduling-target-contact-listing.component';
import { SchedulingTargetContactDetailComponent } from './scheduler-data-components/scheduling-target-contact/scheduling-target-contact-detail/scheduling-target-contact-detail.component';
import { SchedulingTargetContactChangeHistoryListingComponent } from './scheduler-data-components/scheduling-target-contact-change-history/scheduling-target-contact-change-history-listing/scheduling-target-contact-change-history-listing.component';
import { SchedulingTargetContactChangeHistoryDetailComponent } from './scheduler-data-components/scheduling-target-contact-change-history/scheduling-target-contact-change-history-detail/scheduling-target-contact-change-history-detail.component';
import { SchedulingTargetQualificationRequirementListingComponent } from './scheduler-data-components/scheduling-target-qualification-requirement/scheduling-target-qualification-requirement-listing/scheduling-target-qualification-requirement-listing.component';
import { SchedulingTargetQualificationRequirementDetailComponent } from './scheduler-data-components/scheduling-target-qualification-requirement/scheduling-target-qualification-requirement-detail/scheduling-target-qualification-requirement-detail.component';
import { SchedulingTargetQualificationRequirementChangeHistoryListingComponent } from './scheduler-data-components/scheduling-target-qualification-requirement-change-history/scheduling-target-qualification-requirement-change-history-listing/scheduling-target-qualification-requirement-change-history-listing.component';
import { SchedulingTargetQualificationRequirementChangeHistoryDetailComponent } from './scheduler-data-components/scheduling-target-qualification-requirement-change-history/scheduling-target-qualification-requirement-change-history-detail/scheduling-target-qualification-requirement-change-history-detail.component';
import { SchedulingTargetTypeListingComponent } from './scheduler-data-components/scheduling-target-type/scheduling-target-type-listing/scheduling-target-type-listing.component';
import { SchedulingTargetTypeDetailComponent } from './scheduler-data-components/scheduling-target-type/scheduling-target-type-detail/scheduling-target-type-detail.component';
import { ShiftPatternListingComponent } from './scheduler-data-components/shift-pattern/shift-pattern-listing/shift-pattern-listing.component';
import { ShiftPatternDetailComponent } from './scheduler-data-components/shift-pattern/shift-pattern-detail/shift-pattern-detail.component';
import { ShiftPatternChangeHistoryListingComponent } from './scheduler-data-components/shift-pattern-change-history/shift-pattern-change-history-listing/shift-pattern-change-history-listing.component';
import { ShiftPatternChangeHistoryDetailComponent } from './scheduler-data-components/shift-pattern-change-history/shift-pattern-change-history-detail/shift-pattern-change-history-detail.component';
import { ShiftPatternDayListingComponent } from './scheduler-data-components/shift-pattern-day/shift-pattern-day-listing/shift-pattern-day-listing.component';
import { ShiftPatternDayDetailComponent } from './scheduler-data-components/shift-pattern-day/shift-pattern-day-detail/shift-pattern-day-detail.component';
import { ShiftPatternDayChangeHistoryListingComponent } from './scheduler-data-components/shift-pattern-day-change-history/shift-pattern-day-change-history-listing/shift-pattern-day-change-history-listing.component';
import { ShiftPatternDayChangeHistoryDetailComponent } from './scheduler-data-components/shift-pattern-day-change-history/shift-pattern-day-change-history-detail/shift-pattern-day-change-history-detail.component';
import { SoftCreditListingComponent } from './scheduler-data-components/soft-credit/soft-credit-listing/soft-credit-listing.component';
import { SoftCreditDetailComponent } from './scheduler-data-components/soft-credit/soft-credit-detail/soft-credit-detail.component';
import { SoftCreditChangeHistoryListingComponent } from './scheduler-data-components/soft-credit-change-history/soft-credit-change-history-listing/soft-credit-change-history-listing.component';
import { SoftCreditChangeHistoryDetailComponent } from './scheduler-data-components/soft-credit-change-history/soft-credit-change-history-detail/soft-credit-change-history-detail.component';
import { StateProvinceListingComponent } from './scheduler-data-components/state-province/state-province-listing/state-province-listing.component';
import { StateProvinceDetailComponent } from './scheduler-data-components/state-province/state-province-detail/state-province-detail.component';
import { TagListingComponent } from './scheduler-data-components/tag/tag-listing/tag-listing.component';
import { TagDetailComponent } from './scheduler-data-components/tag/tag-detail/tag-detail.component';
import { TenantProfileListingComponent } from './scheduler-data-components/tenant-profile/tenant-profile-listing/tenant-profile-listing.component';
import { TenantProfileDetailComponent } from './scheduler-data-components/tenant-profile/tenant-profile-detail/tenant-profile-detail.component';
import { TenantProfileChangeHistoryListingComponent } from './scheduler-data-components/tenant-profile-change-history/tenant-profile-change-history-listing/tenant-profile-change-history-listing.component';
import { TenantProfileChangeHistoryDetailComponent } from './scheduler-data-components/tenant-profile-change-history/tenant-profile-change-history-detail/tenant-profile-change-history-detail.component';
import { TimeZoneListingComponent } from './scheduler-data-components/time-zone/time-zone-listing/time-zone-listing.component';
import { TimeZoneDetailComponent } from './scheduler-data-components/time-zone/time-zone-detail/time-zone-detail.component';
import { TributeListingComponent } from './scheduler-data-components/tribute/tribute-listing/tribute-listing.component';
import { TributeDetailComponent } from './scheduler-data-components/tribute/tribute-detail/tribute-detail.component';
import { TributeChangeHistoryListingComponent } from './scheduler-data-components/tribute-change-history/tribute-change-history-listing/tribute-change-history-listing.component';
import { TributeChangeHistoryDetailComponent } from './scheduler-data-components/tribute-change-history/tribute-change-history-detail/tribute-change-history-detail.component';
import { TributeTypeListingComponent } from './scheduler-data-components/tribute-type/tribute-type-listing/tribute-type-listing.component';
import { TributeTypeDetailComponent } from './scheduler-data-components/tribute-type/tribute-type-detail/tribute-type-detail.component';
import { VolunteerGroupListingComponent } from './scheduler-data-components/volunteer-group/volunteer-group-listing/volunteer-group-listing.component';
import { VolunteerGroupDetailComponent } from './scheduler-data-components/volunteer-group/volunteer-group-detail/volunteer-group-detail.component';
import { VolunteerGroupChangeHistoryListingComponent } from './scheduler-data-components/volunteer-group-change-history/volunteer-group-change-history-listing/volunteer-group-change-history-listing.component';
import { VolunteerGroupChangeHistoryDetailComponent } from './scheduler-data-components/volunteer-group-change-history/volunteer-group-change-history-detail/volunteer-group-change-history-detail.component';
import { VolunteerGroupMemberListingComponent } from './scheduler-data-components/volunteer-group-member/volunteer-group-member-listing/volunteer-group-member-listing.component';
import { VolunteerGroupMemberDetailComponent } from './scheduler-data-components/volunteer-group-member/volunteer-group-member-detail/volunteer-group-member-detail.component';
import { VolunteerGroupMemberChangeHistoryListingComponent } from './scheduler-data-components/volunteer-group-member-change-history/volunteer-group-member-change-history-listing/volunteer-group-member-change-history-listing.component';
import { VolunteerGroupMemberChangeHistoryDetailComponent } from './scheduler-data-components/volunteer-group-member-change-history/volunteer-group-member-change-history-detail/volunteer-group-member-change-history-detail.component';
import { VolunteerProfileListingComponent } from './scheduler-data-components/volunteer-profile/volunteer-profile-listing/volunteer-profile-listing.component';
import { VolunteerProfileDetailComponent } from './scheduler-data-components/volunteer-profile/volunteer-profile-detail/volunteer-profile-detail.component';
import { VolunteerProfileChangeHistoryListingComponent } from './scheduler-data-components/volunteer-profile-change-history/volunteer-profile-change-history-listing/volunteer-profile-change-history-listing.component';
import { VolunteerProfileChangeHistoryDetailComponent } from './scheduler-data-components/volunteer-profile-change-history/volunteer-profile-change-history-detail/volunteer-profile-change-history-detail.component';
import { VolunteerStatusListingComponent } from './scheduler-data-components/volunteer-status/volunteer-status-listing/volunteer-status-listing.component';
import { VolunteerStatusDetailComponent } from './scheduler-data-components/volunteer-status/volunteer-status-detail/volunteer-status-detail.component';
//
// End of imports for Scheduler Data Components
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
  { path: '', component: OverviewComponent, canActivate: [AuthGuard], title: 'Overview' },
  { path: 'login', component: LoginComponent, title: 'Login' },
  { path: 'google-login', component: AuthCallbackComponent, title: 'Google Login' },
  { path: 'facebook-login', component: AuthCallbackComponent, title: 'Facebook Login' },
  { path: 'twitter-login', component: AuthCallbackComponent, title: 'Twitter Login' },
  { path: 'microsoft-login', component: AuthCallbackComponent, title: 'Microsoft Login' },

  //
  // Custom component routes - admin
  //
  { path: 'overview', component: OverviewComponent, canActivate: [AuthGuard], title: 'Overview' },
  { path: 'new-user/:newUserToken', component: NewUserComponent, title: 'new-user' },
  { path: 'reset-password/:token', component: ResetPasswordComponent, title: 'Reset Password' },

  //
  // Custom component routes - for better business functions - these take precedence over the code gen routes, so they are first
  //
  { path: 'schedule', component: SchedulerCalendarComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Schedule' },
  { path: 'administration', component: AdministrationComponent, canActivate: [AuthGuard], title: 'Administration' },
  { path: 'ratesheets', component: RateSheetCustomListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Rate Sheets' },
  { path: 'templates', component: TemplateManagerComponent, canActivate: [AuthGuard], title: 'Event Templates' },

  //
  // Override the resource paths with custom implementations
  //
  { path: 'resources', component: ResourceCustomListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Resources' },
  { path: 'resources/new', component: ResourceCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Resource' },
  { path: 'resources/:resourceId', component: ResourceCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource' },
  { path: 'resource/:resourceId', component: ResourceCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource' },

  { path: 'crews', component: CrewCustomListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Crews' },
  { path: 'crews/new', component: CrewCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Crew' },
  { path: 'crews/:crewId', component: CrewCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Crew' },
  { path: 'crew/:crewId', component: CrewCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Crew' },

  { path: 'offices', component: OfficeCustomListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Offices' },
  { path: 'offices/new', component: OfficeCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Office' },
  { path: 'offices/:officeId', component: OfficeCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Office' },
  { path: 'office/:officeId', component: OfficeCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Office' },

  { path: 'contacts', component: ContactCustomListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Contacts' },
  { path: 'contacts/new', component: ContactCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Contact' },
  { path: 'contacts/:contactId', component: ContactCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact' },
  { path: 'contact/:contactId', component: ContactCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact' },


  { path: 'calendars', component: CalendarCustomListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Calendars' },
  { path: 'calendars/new', component: CalendarCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Calendar' },
  { path: 'calendars/:calendarId', component: CalendarCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Calendar' },
  { path: 'calendar/:calendarId', component: CalendarCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Calendar' },

  { path: 'clients', component: ClientCustomListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Clients' },
  { path: 'clients/new', component: ClientCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Client' },
  { path: 'clients/:clientId', component: ClientCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Client' },
  { path: 'client/:clientId', component: ClientCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Client' },


  { path: 'schedulingtargets', component: SchedulingTargetCustomListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduling Targets' },
  { path: 'schedulingtargets/new', component: SchedulingTargetCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduling Target' },
  { path: 'schedulingtargets/:schedulingTargetId', component: SchedulingTargetCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target' },
  { path: 'schedulingtarget/:schedulingTargetId', component: SchedulingTargetCustomDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target' },



  { path: 'system-health', component: SystemHealthComponent, canActivate: [AuthGuard], title: 'System Health' },




  //
  // Beginning of routes for Scheduler Data Components
  //
  { path: 'appeals', component: AppealListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Appeals' },
  { path: 'appeals/new', component: AppealDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Appeal' },
  { path: 'appeals/:appealId', component: AppealDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Appeal' },
  { path: 'appeal/:appealId', component: AppealDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Appeal' },
  { path: 'appeal', redirectTo: 'appeals' },
  { path: 'appealchangehistories', component: AppealChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Appeal Change Histories' },
  { path: 'appealchangehistories/new', component: AppealChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Appeal Change History' },
  { path: 'appealchangehistories/:appealChangeHistoryId', component: AppealChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Appeal Change History' },
  { path: 'appealchangehistory/:appealChangeHistoryId', component: AppealChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Appeal Change History' },
  { path: 'appealchangehistory', redirectTo: 'appealchangehistories' },
  { path: 'assignmentroles', component: AssignmentRoleListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Assignment Roles' },
  { path: 'assignmentroles/new', component: AssignmentRoleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Assignment Role' },
  { path: 'assignmentroles/:assignmentRoleId', component: AssignmentRoleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Assignment Role' },
  { path: 'assignmentrole/:assignmentRoleId', component: AssignmentRoleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Assignment Role' },
  { path: 'assignmentrole', redirectTo: 'assignmentroles' },
  { path: 'assignmentrolequalificationrequirements', component: AssignmentRoleQualificationRequirementListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Assignment Role Qualification Requirements' },
  { path: 'assignmentrolequalificationrequirements/new', component: AssignmentRoleQualificationRequirementDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Assignment Role Qualification Requirement' },
  { path: 'assignmentrolequalificationrequirements/:assignmentRoleQualificationRequirementId', component: AssignmentRoleQualificationRequirementDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Assignment Role Qualification Requirement' },
  { path: 'assignmentrolequalificationrequirement/:assignmentRoleQualificationRequirementId', component: AssignmentRoleQualificationRequirementDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Assignment Role Qualification Requirement' },
  { path: 'assignmentrolequalificationrequirement', redirectTo: 'assignmentrolequalificationrequirements' },
  { path: 'assignmentrolequalificationrequirementchangehistories', component: AssignmentRoleQualificationRequirementChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Assignment Role Qualification Requirement Change Histories' },
  { path: 'assignmentrolequalificationrequirementchangehistories/new', component: AssignmentRoleQualificationRequirementChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Assignment Role Qualification Requirement Change History' },
  { path: 'assignmentrolequalificationrequirementchangehistories/:assignmentRoleQualificationRequirementChangeHistoryId', component: AssignmentRoleQualificationRequirementChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Assignment Role Qualification Requirement Change History' },
  { path: 'assignmentrolequalificationrequirementchangehistory/:assignmentRoleQualificationRequirementChangeHistoryId', component: AssignmentRoleQualificationRequirementChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Assignment Role Qualification Requirement Change History' },
  { path: 'assignmentrolequalificationrequirementchangehistory', redirectTo: 'assignmentrolequalificationrequirementchangehistories' },
  { path: 'assignmentstatuses', component: AssignmentStatusListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Assignment Statuses' },
  { path: 'assignmentstatuses/new', component: AssignmentStatusDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Assignment Status' },
  { path: 'assignmentstatuses/:assignmentStatusId', component: AssignmentStatusDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Assignment Status' },
  { path: 'assignmentstatus/:assignmentStatusId', component: AssignmentStatusDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Assignment Status' },
  { path: 'assignmentstatus', redirectTo: 'assignmentstatuses' },
  { path: 'attributedefinitions', component: AttributeDefinitionListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Attribute Definitions' },
  { path: 'attributedefinitions/new', component: AttributeDefinitionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Attribute Definition' },
  { path: 'attributedefinitions/:attributeDefinitionId', component: AttributeDefinitionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Attribute Definition' },
  { path: 'attributedefinition/:attributeDefinitionId', component: AttributeDefinitionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Attribute Definition' },
  { path: 'attributedefinition', redirectTo: 'attributedefinitions' },
  { path: 'attributedefinitionchangehistories', component: AttributeDefinitionChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Attribute Definition Change Histories' },
  { path: 'attributedefinitionchangehistories/new', component: AttributeDefinitionChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Attribute Definition Change History' },
  { path: 'attributedefinitionchangehistories/:attributeDefinitionChangeHistoryId', component: AttributeDefinitionChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Attribute Definition Change History' },
  { path: 'attributedefinitionchangehistory/:attributeDefinitionChangeHistoryId', component: AttributeDefinitionChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Attribute Definition Change History' },
  { path: 'attributedefinitionchangehistory', redirectTo: 'attributedefinitionchangehistories' },
  { path: 'attributedefinitionentities', component: AttributeDefinitionEntityListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Attribute Definition Entities' },
  { path: 'attributedefinitionentities/new', component: AttributeDefinitionEntityDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Attribute Definition Entity' },
  { path: 'attributedefinitionentities/:attributeDefinitionEntityId', component: AttributeDefinitionEntityDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Attribute Definition Entity' },
  { path: 'attributedefinitionentity/:attributeDefinitionEntityId', component: AttributeDefinitionEntityDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Attribute Definition Entity' },
  { path: 'attributedefinitionentity', redirectTo: 'attributedefinitionentities' },
  { path: 'attributedefinitiontypes', component: AttributeDefinitionTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Attribute Definition Types' },
  { path: 'attributedefinitiontypes/new', component: AttributeDefinitionTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Attribute Definition Type' },
  { path: 'attributedefinitiontypes/:attributeDefinitionTypeId', component: AttributeDefinitionTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Attribute Definition Type' },
  { path: 'attributedefinitiontype/:attributeDefinitionTypeId', component: AttributeDefinitionTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Attribute Definition Type' },
  { path: 'attributedefinitiontype', redirectTo: 'attributedefinitiontypes' },
  { path: 'batches', component: BatchListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Batches' },
  { path: 'batches/new', component: BatchDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Batch' },
  { path: 'batches/:batchId', component: BatchDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Batch' },
  { path: 'batch/:batchId', component: BatchDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Batch' },
  { path: 'batch', redirectTo: 'batches' },
  { path: 'batchchangehistories', component: BatchChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Batch Change Histories' },
  { path: 'batchchangehistories/new', component: BatchChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Batch Change History' },
  { path: 'batchchangehistories/:batchChangeHistoryId', component: BatchChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Batch Change History' },
  { path: 'batchchangehistory/:batchChangeHistoryId', component: BatchChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Batch Change History' },
  { path: 'batchchangehistory', redirectTo: 'batchchangehistories' },
  { path: 'batchstatuses', component: BatchStatusListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Batch Statuses' },
  { path: 'batchstatuses/new', component: BatchStatusDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Batch Status' },
  { path: 'batchstatuses/:batchStatusId', component: BatchStatusDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Batch Status' },
  { path: 'batchstatus/:batchStatusId', component: BatchStatusDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Batch Status' },
  { path: 'batchstatus', redirectTo: 'batchstatuses' },
  { path: 'bookingsourcetypes', component: BookingSourceTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Booking Source Types' },
  { path: 'bookingsourcetypes/new', component: BookingSourceTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Booking Source Type' },
  { path: 'bookingsourcetypes/:bookingSourceTypeId', component: BookingSourceTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Booking Source Type' },
  { path: 'bookingsourcetype/:bookingSourceTypeId', component: BookingSourceTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Booking Source Type' },
  { path: 'bookingsourcetype', redirectTo: 'bookingsourcetypes' },
  { path: 'calendars', component: CalendarListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Calendars' },
  { path: 'calendars/new', component: CalendarDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Calendar' },
  { path: 'calendars/:calendarId', component: CalendarDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Calendar' },
  { path: 'calendar/:calendarId', component: CalendarDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Calendar' },
  { path: 'calendar', redirectTo: 'calendars' },
  { path: 'calendarchangehistories', component: CalendarChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Calendar Change Histories' },
  { path: 'calendarchangehistories/new', component: CalendarChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Calendar Change History' },
  { path: 'calendarchangehistories/:calendarChangeHistoryId', component: CalendarChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Calendar Change History' },
  { path: 'calendarchangehistory/:calendarChangeHistoryId', component: CalendarChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Calendar Change History' },
  { path: 'calendarchangehistory', redirectTo: 'calendarchangehistories' },
  { path: 'campaigns', component: CampaignListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Campaigns' },
  { path: 'campaigns/new', component: CampaignDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Campaign' },
  { path: 'campaigns/:campaignId', component: CampaignDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Campaign' },
  { path: 'campaign/:campaignId', component: CampaignDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Campaign' },
  { path: 'campaign', redirectTo: 'campaigns' },
  { path: 'campaignchangehistories', component: CampaignChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Campaign Change Histories' },
  { path: 'campaignchangehistories/new', component: CampaignChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Campaign Change History' },
  { path: 'campaignchangehistories/:campaignChangeHistoryId', component: CampaignChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Campaign Change History' },
  { path: 'campaignchangehistory/:campaignChangeHistoryId', component: CampaignChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Campaign Change History' },
  { path: 'campaignchangehistory', redirectTo: 'campaignchangehistories' },
  { path: 'chargestatuses', component: ChargeStatusListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Charge Statuses' },
  { path: 'chargestatuses/new', component: ChargeStatusDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Charge Status' },
  { path: 'chargestatuses/:chargeStatusId', component: ChargeStatusDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Charge Status' },
  { path: 'chargestatus/:chargeStatusId', component: ChargeStatusDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Charge Status' },
  { path: 'chargestatus', redirectTo: 'chargestatuses' },
  { path: 'chargetypes', component: ChargeTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Charge Types' },
  { path: 'chargetypes/new', component: ChargeTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Charge Type' },
  { path: 'chargetypes/:chargeTypeId', component: ChargeTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Charge Type' },
  { path: 'chargetype/:chargeTypeId', component: ChargeTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Charge Type' },
  { path: 'chargetype', redirectTo: 'chargetypes' },
  { path: 'chargetypechangehistories', component: ChargeTypeChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Charge Type Change Histories' },
  { path: 'chargetypechangehistories/new', component: ChargeTypeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Charge Type Change History' },
  { path: 'chargetypechangehistories/:chargeTypeChangeHistoryId', component: ChargeTypeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Charge Type Change History' },
  { path: 'chargetypechangehistory/:chargeTypeChangeHistoryId', component: ChargeTypeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Charge Type Change History' },
  { path: 'chargetypechangehistory', redirectTo: 'chargetypechangehistories' },
  { path: 'clients', component: ClientListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Clients' },
  { path: 'clients/new', component: ClientDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Client' },
  { path: 'clients/:clientId', component: ClientDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Client' },
  { path: 'client/:clientId', component: ClientDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Client' },
  { path: 'client', redirectTo: 'clients' },
  { path: 'clientchangehistories', component: ClientChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Client Change Histories' },
  { path: 'clientchangehistories/new', component: ClientChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Client Change History' },
  { path: 'clientchangehistories/:clientChangeHistoryId', component: ClientChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Client Change History' },
  { path: 'clientchangehistory/:clientChangeHistoryId', component: ClientChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Client Change History' },
  { path: 'clientchangehistory', redirectTo: 'clientchangehistories' },
  { path: 'clientcontacts', component: ClientContactListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Client Contacts' },
  { path: 'clientcontacts/new', component: ClientContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Client Contact' },
  { path: 'clientcontacts/:clientContactId', component: ClientContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Client Contact' },
  { path: 'clientcontact/:clientContactId', component: ClientContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Client Contact' },
  { path: 'clientcontact', redirectTo: 'clientcontacts' },
  { path: 'clientcontactchangehistories', component: ClientContactChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Client Contact Change Histories' },
  { path: 'clientcontactchangehistories/new', component: ClientContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Client Contact Change History' },
  { path: 'clientcontactchangehistories/:clientContactChangeHistoryId', component: ClientContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Client Contact Change History' },
  { path: 'clientcontactchangehistory/:clientContactChangeHistoryId', component: ClientContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Client Contact Change History' },
  { path: 'clientcontactchangehistory', redirectTo: 'clientcontactchangehistories' },
  { path: 'clienttypes', component: ClientTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Client Types' },
  { path: 'clienttypes/new', component: ClientTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Client Type' },
  { path: 'clienttypes/:clientTypeId', component: ClientTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Client Type' },
  { path: 'clienttype/:clientTypeId', component: ClientTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Client Type' },
  { path: 'clienttype', redirectTo: 'clienttypes' },
  { path: 'constituents', component: ConstituentListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Constituents' },
  { path: 'constituents/new', component: ConstituentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Constituent' },
  { path: 'constituents/:constituentId', component: ConstituentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Constituent' },
  { path: 'constituent/:constituentId', component: ConstituentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Constituent' },
  { path: 'constituent', redirectTo: 'constituents' },
  { path: 'constituentchangehistories', component: ConstituentChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Constituent Change Histories' },
  { path: 'constituentchangehistories/new', component: ConstituentChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Constituent Change History' },
  { path: 'constituentchangehistories/:constituentChangeHistoryId', component: ConstituentChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Constituent Change History' },
  { path: 'constituentchangehistory/:constituentChangeHistoryId', component: ConstituentChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Constituent Change History' },
  { path: 'constituentchangehistory', redirectTo: 'constituentchangehistories' },
  { path: 'constituentjourneystages', component: ConstituentJourneyStageListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Constituent Journey Stages' },
  { path: 'constituentjourneystages/new', component: ConstituentJourneyStageDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Constituent Journey Stage' },
  { path: 'constituentjourneystages/:constituentJourneyStageId', component: ConstituentJourneyStageDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Constituent Journey Stage' },
  { path: 'constituentjourneystage/:constituentJourneyStageId', component: ConstituentJourneyStageDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Constituent Journey Stage' },
  { path: 'constituentjourneystage', redirectTo: 'constituentjourneystages' },
  { path: 'constituentjourneystagechangehistories', component: ConstituentJourneyStageChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Constituent Journey Stage Change Histories' },
  { path: 'constituentjourneystagechangehistories/new', component: ConstituentJourneyStageChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Constituent Journey Stage Change History' },
  { path: 'constituentjourneystagechangehistories/:constituentJourneyStageChangeHistoryId', component: ConstituentJourneyStageChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Constituent Journey Stage Change History' },
  { path: 'constituentjourneystagechangehistory/:constituentJourneyStageChangeHistoryId', component: ConstituentJourneyStageChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Constituent Journey Stage Change History' },
  { path: 'constituentjourneystagechangehistory', redirectTo: 'constituentjourneystagechangehistories' },
  { path: 'contacts', component: ContactListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Contacts' },
  { path: 'contacts/new', component: ContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Contact' },
  { path: 'contacts/:contactId', component: ContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact' },
  { path: 'contact/:contactId', component: ContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact' },
  { path: 'contact', redirectTo: 'contacts' },
  { path: 'contactchangehistories', component: ContactChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Contact Change Histories' },
  { path: 'contactchangehistories/new', component: ContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Contact Change History' },
  { path: 'contactchangehistories/:contactChangeHistoryId', component: ContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Change History' },
  { path: 'contactchangehistory/:contactChangeHistoryId', component: ContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Change History' },
  { path: 'contactchangehistory', redirectTo: 'contactchangehistories' },
  { path: 'contactcontacts', component: ContactContactListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Contact Contacts' },
  { path: 'contactcontacts/new', component: ContactContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Contact Contact' },
  { path: 'contactcontacts/:contactContactId', component: ContactContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Contact' },
  { path: 'contactcontact/:contactContactId', component: ContactContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Contact' },
  { path: 'contactcontact', redirectTo: 'contactcontacts' },
  { path: 'contactcontactchangehistories', component: ContactContactChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Contact Contact Change Histories' },
  { path: 'contactcontactchangehistories/new', component: ContactContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Contact Contact Change History' },
  { path: 'contactcontactchangehistories/:contactContactChangeHistoryId', component: ContactContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Contact Change History' },
  { path: 'contactcontactchangehistory/:contactContactChangeHistoryId', component: ContactContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Contact Change History' },
  { path: 'contactcontactchangehistory', redirectTo: 'contactcontactchangehistories' },
  { path: 'contactinteractions', component: ContactInteractionListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Contact Interactions' },
  { path: 'contactinteractions/new', component: ContactInteractionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Contact Interaction' },
  { path: 'contactinteractions/:contactInteractionId', component: ContactInteractionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Interaction' },
  { path: 'contactinteraction/:contactInteractionId', component: ContactInteractionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Interaction' },
  { path: 'contactinteraction', redirectTo: 'contactinteractions' },
  { path: 'contactinteractionchangehistories', component: ContactInteractionChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Contact Interaction Change Histories' },
  { path: 'contactinteractionchangehistories/new', component: ContactInteractionChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Contact Interaction Change History' },
  { path: 'contactinteractionchangehistories/:contactInteractionChangeHistoryId', component: ContactInteractionChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Interaction Change History' },
  { path: 'contactinteractionchangehistory/:contactInteractionChangeHistoryId', component: ContactInteractionChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Interaction Change History' },
  { path: 'contactinteractionchangehistory', redirectTo: 'contactinteractionchangehistories' },
  { path: 'contactmethods', component: ContactMethodListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Contact Methods' },
  { path: 'contactmethods/new', component: ContactMethodDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Contact Method' },
  { path: 'contactmethods/:contactMethodId', component: ContactMethodDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Method' },
  { path: 'contactmethod/:contactMethodId', component: ContactMethodDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Method' },
  { path: 'contactmethod', redirectTo: 'contactmethods' },
  { path: 'contacttags', component: ContactTagListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Contact Tags' },
  { path: 'contacttags/new', component: ContactTagDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Contact Tag' },
  { path: 'contacttags/:contactTagId', component: ContactTagDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Tag' },
  { path: 'contacttag/:contactTagId', component: ContactTagDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Tag' },
  { path: 'contacttag', redirectTo: 'contacttags' },
  { path: 'contacttagchangehistories', component: ContactTagChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Contact Tag Change Histories' },
  { path: 'contacttagchangehistories/new', component: ContactTagChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Contact Tag Change History' },
  { path: 'contacttagchangehistories/:contactTagChangeHistoryId', component: ContactTagChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Tag Change History' },
  { path: 'contacttagchangehistory/:contactTagChangeHistoryId', component: ContactTagChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Tag Change History' },
  { path: 'contacttagchangehistory', redirectTo: 'contacttagchangehistories' },
  { path: 'contacttypes', component: ContactTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Contact Types' },
  { path: 'contacttypes/new', component: ContactTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Contact Type' },
  { path: 'contacttypes/:contactTypeId', component: ContactTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Type' },
  { path: 'contacttype/:contactTypeId', component: ContactTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Contact Type' },
  { path: 'contacttype', redirectTo: 'contacttypes' },
  { path: 'countries', component: CountryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Countries' },
  { path: 'countries/new', component: CountryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Country' },
  { path: 'countries/:countryId', component: CountryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Country' },
  { path: 'country/:countryId', component: CountryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Country' },
  { path: 'country', redirectTo: 'countries' },
  { path: 'crews', component: CrewListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Crews' },
  { path: 'crews/new', component: CrewDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Crew' },
  { path: 'crews/:crewId', component: CrewDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Crew' },
  { path: 'crew/:crewId', component: CrewDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Crew' },
  { path: 'crew', redirectTo: 'crews' },
  { path: 'crewchangehistories', component: CrewChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Crew Change Histories' },
  { path: 'crewchangehistories/new', component: CrewChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Crew Change History' },
  { path: 'crewchangehistories/:crewChangeHistoryId', component: CrewChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Crew Change History' },
  { path: 'crewchangehistory/:crewChangeHistoryId', component: CrewChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Crew Change History' },
  { path: 'crewchangehistory', redirectTo: 'crewchangehistories' },
  { path: 'crewmembers', component: CrewMemberListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Crew Members' },
  { path: 'crewmembers/new', component: CrewMemberDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Crew Member' },
  { path: 'crewmembers/:crewMemberId', component: CrewMemberDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Crew Member' },
  { path: 'crewmember/:crewMemberId', component: CrewMemberDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Crew Member' },
  { path: 'crewmember', redirectTo: 'crewmembers' },
  { path: 'crewmemberchangehistories', component: CrewMemberChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Crew Member Change Histories' },
  { path: 'crewmemberchangehistories/new', component: CrewMemberChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Crew Member Change History' },
  { path: 'crewmemberchangehistories/:crewMemberChangeHistoryId', component: CrewMemberChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Crew Member Change History' },
  { path: 'crewmemberchangehistory/:crewMemberChangeHistoryId', component: CrewMemberChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Crew Member Change History' },
  { path: 'crewmemberchangehistory', redirectTo: 'crewmemberchangehistories' },
  { path: 'currencies', component: CurrencyListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Currencies' },
  { path: 'currencies/new', component: CurrencyDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Currency' },
  { path: 'currencies/:currencyId', component: CurrencyDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Currency' },
  { path: 'currency/:currencyId', component: CurrencyDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Currency' },
  { path: 'currency', redirectTo: 'currencies' },
  { path: 'dependencytypes', component: DependencyTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Dependency Types' },
  { path: 'dependencytypes/new', component: DependencyTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Dependency Type' },
  { path: 'dependencytypes/:dependencyTypeId', component: DependencyTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Dependency Type' },
  { path: 'dependencytype/:dependencyTypeId', component: DependencyTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Dependency Type' },
  { path: 'dependencytype', redirectTo: 'dependencytypes' },
  { path: 'eventcalendars', component: EventCalendarListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Event Calendars' },
  { path: 'eventcalendars/new', component: EventCalendarDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Event Calendar' },
  { path: 'eventcalendars/:eventCalendarId', component: EventCalendarDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Event Calendar' },
  { path: 'eventcalendar/:eventCalendarId', component: EventCalendarDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Event Calendar' },
  { path: 'eventcalendar', redirectTo: 'eventcalendars' },
  { path: 'eventcharges', component: EventChargeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Event Charges' },
  { path: 'eventcharges/new', component: EventChargeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Event Charge' },
  { path: 'eventcharges/:eventChargeId', component: EventChargeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Event Charge' },
  { path: 'eventcharge/:eventChargeId', component: EventChargeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Event Charge' },
  { path: 'eventcharge', redirectTo: 'eventcharges' },
  { path: 'eventchargechangehistories', component: EventChargeChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Event Charge Change Histories' },
  { path: 'eventchargechangehistories/new', component: EventChargeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Event Charge Change History' },
  { path: 'eventchargechangehistories/:eventChargeChangeHistoryId', component: EventChargeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Event Charge Change History' },
  { path: 'eventchargechangehistory/:eventChargeChangeHistoryId', component: EventChargeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Event Charge Change History' },
  { path: 'eventchargechangehistory', redirectTo: 'eventchargechangehistories' },
  { path: 'eventresourceassignments', component: EventResourceAssignmentListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Event Resource Assignments' },
  { path: 'eventresourceassignments/new', component: EventResourceAssignmentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Event Resource Assignment' },
  { path: 'eventresourceassignments/:eventResourceAssignmentId', component: EventResourceAssignmentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Event Resource Assignment' },
  { path: 'eventresourceassignment/:eventResourceAssignmentId', component: EventResourceAssignmentDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Event Resource Assignment' },
  { path: 'eventresourceassignment', redirectTo: 'eventresourceassignments' },
  { path: 'eventresourceassignmentchangehistories', component: EventResourceAssignmentChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Event Resource Assignment Change Histories' },
  { path: 'eventresourceassignmentchangehistories/new', component: EventResourceAssignmentChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Event Resource Assignment Change History' },
  { path: 'eventresourceassignmentchangehistories/:eventResourceAssignmentChangeHistoryId', component: EventResourceAssignmentChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Event Resource Assignment Change History' },
  { path: 'eventresourceassignmentchangehistory/:eventResourceAssignmentChangeHistoryId', component: EventResourceAssignmentChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Event Resource Assignment Change History' },
  { path: 'eventresourceassignmentchangehistory', redirectTo: 'eventresourceassignmentchangehistories' },
  { path: 'eventstatuses', component: EventStatusListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Event Statuses' },
  { path: 'eventstatuses/new', component: EventStatusDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Event Status' },
  { path: 'eventstatuses/:eventStatusId', component: EventStatusDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Event Status' },
  { path: 'eventstatus/:eventStatusId', component: EventStatusDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Event Status' },
  { path: 'eventstatus', redirectTo: 'eventstatuses' },
  { path: 'funds', component: FundListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Funds' },
  { path: 'funds/new', component: FundDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Fund' },
  { path: 'funds/:fundId', component: FundDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Fund' },
  { path: 'fund/:fundId', component: FundDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Fund' },
  { path: 'fund', redirectTo: 'funds' },
  { path: 'fundchangehistories', component: FundChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Fund Change Histories' },
  { path: 'fundchangehistories/new', component: FundChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Fund Change History' },
  { path: 'fundchangehistories/:fundChangeHistoryId', component: FundChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Fund Change History' },
  { path: 'fundchangehistory/:fundChangeHistoryId', component: FundChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Fund Change History' },
  { path: 'fundchangehistory', redirectTo: 'fundchangehistories' },
  { path: 'gifts', component: GiftListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Gifts' },
  { path: 'gifts/new', component: GiftDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Gift' },
  { path: 'gifts/:giftId', component: GiftDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Gift' },
  { path: 'gift/:giftId', component: GiftDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Gift' },
  { path: 'gift', redirectTo: 'gifts' },
  { path: 'giftchangehistories', component: GiftChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Gift Change Histories' },
  { path: 'giftchangehistories/new', component: GiftChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Gift Change History' },
  { path: 'giftchangehistories/:giftChangeHistoryId', component: GiftChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Gift Change History' },
  { path: 'giftchangehistory/:giftChangeHistoryId', component: GiftChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Gift Change History' },
  { path: 'giftchangehistory', redirectTo: 'giftchangehistories' },
  { path: 'households', component: HouseholdListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Households' },
  { path: 'households/new', component: HouseholdDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Household' },
  { path: 'households/:householdId', component: HouseholdDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Household' },
  { path: 'household/:householdId', component: HouseholdDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Household' },
  { path: 'household', redirectTo: 'households' },
  { path: 'householdchangehistories', component: HouseholdChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Household Change Histories' },
  { path: 'householdchangehistories/new', component: HouseholdChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Household Change History' },
  { path: 'householdchangehistories/:householdChangeHistoryId', component: HouseholdChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Household Change History' },
  { path: 'householdchangehistory/:householdChangeHistoryId', component: HouseholdChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Household Change History' },
  { path: 'householdchangehistory', redirectTo: 'householdchangehistories' },
  { path: 'icons', component: IconListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Icons' },
  { path: 'icons/new', component: IconDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Icon' },
  { path: 'icons/:iconId', component: IconDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Icon' },
  { path: 'icon/:iconId', component: IconDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Icon' },
  { path: 'icon', redirectTo: 'icons' },
  { path: 'interactiontypes', component: InteractionTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Interaction Types' },
  { path: 'interactiontypes/new', component: InteractionTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Interaction Type' },
  { path: 'interactiontypes/:interactionTypeId', component: InteractionTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Interaction Type' },
  { path: 'interactiontype/:interactionTypeId', component: InteractionTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Interaction Type' },
  { path: 'interactiontype', redirectTo: 'interactiontypes' },
  { path: 'notificationsubscriptions', component: NotificationSubscriptionListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Notification Subscriptions' },
  { path: 'notificationsubscriptions/new', component: NotificationSubscriptionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Notification Subscription' },
  { path: 'notificationsubscriptions/:notificationSubscriptionId', component: NotificationSubscriptionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Notification Subscription' },
  { path: 'notificationsubscription/:notificationSubscriptionId', component: NotificationSubscriptionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Notification Subscription' },
  { path: 'notificationsubscription', redirectTo: 'notificationsubscriptions' },
  { path: 'notificationsubscriptionchangehistories', component: NotificationSubscriptionChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Notification Subscription Change Histories' },
  { path: 'notificationsubscriptionchangehistories/new', component: NotificationSubscriptionChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Notification Subscription Change History' },
  { path: 'notificationsubscriptionchangehistories/:notificationSubscriptionChangeHistoryId', component: NotificationSubscriptionChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Notification Subscription Change History' },
  { path: 'notificationsubscriptionchangehistory/:notificationSubscriptionChangeHistoryId', component: NotificationSubscriptionChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Notification Subscription Change History' },
  { path: 'notificationsubscriptionchangehistory', redirectTo: 'notificationsubscriptionchangehistories' },
  { path: 'notificationtypes', component: NotificationTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Notification Types' },
  { path: 'notificationtypes/new', component: NotificationTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Notification Type' },
  { path: 'notificationtypes/:notificationTypeId', component: NotificationTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Notification Type' },
  { path: 'notificationtype/:notificationTypeId', component: NotificationTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Notification Type' },
  { path: 'notificationtype', redirectTo: 'notificationtypes' },
  { path: 'offices', component: OfficeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Offices' },
  { path: 'offices/new', component: OfficeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Office' },
  { path: 'offices/:officeId', component: OfficeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Office' },
  { path: 'office/:officeId', component: OfficeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Office' },
  { path: 'office', redirectTo: 'offices' },
  { path: 'officechangehistories', component: OfficeChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Office Change Histories' },
  { path: 'officechangehistories/new', component: OfficeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Office Change History' },
  { path: 'officechangehistories/:officeChangeHistoryId', component: OfficeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Office Change History' },
  { path: 'officechangehistory/:officeChangeHistoryId', component: OfficeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Office Change History' },
  { path: 'officechangehistory', redirectTo: 'officechangehistories' },
  { path: 'officecontacts', component: OfficeContactListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Office Contacts' },
  { path: 'officecontacts/new', component: OfficeContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Office Contact' },
  { path: 'officecontacts/:officeContactId', component: OfficeContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Office Contact' },
  { path: 'officecontact/:officeContactId', component: OfficeContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Office Contact' },
  { path: 'officecontact', redirectTo: 'officecontacts' },
  { path: 'officecontactchangehistories', component: OfficeContactChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Office Contact Change Histories' },
  { path: 'officecontactchangehistories/new', component: OfficeContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Office Contact Change History' },
  { path: 'officecontactchangehistories/:officeContactChangeHistoryId', component: OfficeContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Office Contact Change History' },
  { path: 'officecontactchangehistory/:officeContactChangeHistoryId', component: OfficeContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Office Contact Change History' },
  { path: 'officecontactchangehistory', redirectTo: 'officecontactchangehistories' },
  { path: 'officetypes', component: OfficeTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Office Types' },
  { path: 'officetypes/new', component: OfficeTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Office Type' },
  { path: 'officetypes/:officeTypeId', component: OfficeTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Office Type' },
  { path: 'officetype/:officeTypeId', component: OfficeTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Office Type' },
  { path: 'officetype', redirectTo: 'officetypes' },
  { path: 'paymenttypes', component: PaymentTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Payment Types' },
  { path: 'paymenttypes/new', component: PaymentTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Payment Type' },
  { path: 'paymenttypes/:paymentTypeId', component: PaymentTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Payment Type' },
  { path: 'paymenttype/:paymentTypeId', component: PaymentTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Payment Type' },
  { path: 'paymenttype', redirectTo: 'paymenttypes' },
  { path: 'pledges', component: PledgeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Pledges' },
  { path: 'pledges/new', component: PledgeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Pledge' },
  { path: 'pledges/:pledgeId', component: PledgeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Pledge' },
  { path: 'pledge/:pledgeId', component: PledgeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Pledge' },
  { path: 'pledge', redirectTo: 'pledges' },
  { path: 'pledgechangehistories', component: PledgeChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Pledge Change Histories' },
  { path: 'pledgechangehistories/new', component: PledgeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Pledge Change History' },
  { path: 'pledgechangehistories/:pledgeChangeHistoryId', component: PledgeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Pledge Change History' },
  { path: 'pledgechangehistory/:pledgeChangeHistoryId', component: PledgeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Pledge Change History' },
  { path: 'pledgechangehistory', redirectTo: 'pledgechangehistories' },
  { path: 'priorities', component: PriorityListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Priorities' },
  { path: 'priorities/new', component: PriorityDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Priority' },
  { path: 'priorities/:priorityId', component: PriorityDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Priority' },
  { path: 'priority/:priorityId', component: PriorityDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Priority' },
  { path: 'priority', redirectTo: 'priorities' },
  { path: 'qualifications', component: QualificationListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Qualifications' },
  { path: 'qualifications/new', component: QualificationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Qualification' },
  { path: 'qualifications/:qualificationId', component: QualificationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Qualification' },
  { path: 'qualification/:qualificationId', component: QualificationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Qualification' },
  { path: 'qualification', redirectTo: 'qualifications' },
  { path: 'ratesheets', component: RateSheetListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Rate Sheets' },
  { path: 'ratesheets/new', component: RateSheetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Rate Sheet' },
  { path: 'ratesheets/:rateSheetId', component: RateSheetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Rate Sheet' },
  { path: 'ratesheet/:rateSheetId', component: RateSheetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Rate Sheet' },
  { path: 'ratesheet', redirectTo: 'ratesheets' },
  { path: 'ratesheetchangehistories', component: RateSheetChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Rate Sheet Change Histories' },
  { path: 'ratesheetchangehistories/new', component: RateSheetChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Rate Sheet Change History' },
  { path: 'ratesheetchangehistories/:rateSheetChangeHistoryId', component: RateSheetChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Rate Sheet Change History' },
  { path: 'ratesheetchangehistory/:rateSheetChangeHistoryId', component: RateSheetChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Rate Sheet Change History' },
  { path: 'ratesheetchangehistory', redirectTo: 'ratesheetchangehistories' },
  { path: 'ratetypes', component: RateTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Rate Types' },
  { path: 'ratetypes/new', component: RateTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Rate Type' },
  { path: 'ratetypes/:rateTypeId', component: RateTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Rate Type' },
  { path: 'ratetype/:rateTypeId', component: RateTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Rate Type' },
  { path: 'ratetype', redirectTo: 'ratetypes' },
  { path: 'receipttypes', component: ReceiptTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Receipt Types' },
  { path: 'receipttypes/new', component: ReceiptTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Receipt Type' },
  { path: 'receipttypes/:receiptTypeId', component: ReceiptTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Receipt Type' },
  { path: 'receipttype/:receiptTypeId', component: ReceiptTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Receipt Type' },
  { path: 'receipttype', redirectTo: 'receipttypes' },
  { path: 'recurrenceexceptions', component: RecurrenceExceptionListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Recurrence Exceptions' },
  { path: 'recurrenceexceptions/new', component: RecurrenceExceptionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Recurrence Exception' },
  { path: 'recurrenceexceptions/:recurrenceExceptionId', component: RecurrenceExceptionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Recurrence Exception' },
  { path: 'recurrenceexception/:recurrenceExceptionId', component: RecurrenceExceptionDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Recurrence Exception' },
  { path: 'recurrenceexception', redirectTo: 'recurrenceexceptions' },
  { path: 'recurrenceexceptionchangehistories', component: RecurrenceExceptionChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Recurrence Exception Change Histories' },
  { path: 'recurrenceexceptionchangehistories/new', component: RecurrenceExceptionChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Recurrence Exception Change History' },
  { path: 'recurrenceexceptionchangehistories/:recurrenceExceptionChangeHistoryId', component: RecurrenceExceptionChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Recurrence Exception Change History' },
  { path: 'recurrenceexceptionchangehistory/:recurrenceExceptionChangeHistoryId', component: RecurrenceExceptionChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Recurrence Exception Change History' },
  { path: 'recurrenceexceptionchangehistory', redirectTo: 'recurrenceexceptionchangehistories' },
  { path: 'recurrencefrequencies', component: RecurrenceFrequencyListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Recurrence Frequencies' },
  { path: 'recurrencefrequencies/new', component: RecurrenceFrequencyDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Recurrence Frequency' },
  { path: 'recurrencefrequencies/:recurrenceFrequencyId', component: RecurrenceFrequencyDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Recurrence Frequency' },
  { path: 'recurrencefrequency/:recurrenceFrequencyId', component: RecurrenceFrequencyDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Recurrence Frequency' },
  { path: 'recurrencefrequency', redirectTo: 'recurrencefrequencies' },
  { path: 'recurrencerules', component: RecurrenceRuleListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Recurrence Rules' },
  { path: 'recurrencerules/new', component: RecurrenceRuleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Recurrence Rule' },
  { path: 'recurrencerules/:recurrenceRuleId', component: RecurrenceRuleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Recurrence Rule' },
  { path: 'recurrencerule/:recurrenceRuleId', component: RecurrenceRuleDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Recurrence Rule' },
  { path: 'recurrencerule', redirectTo: 'recurrencerules' },
  { path: 'recurrencerulechangehistories', component: RecurrenceRuleChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Recurrence Rule Change Histories' },
  { path: 'recurrencerulechangehistories/new', component: RecurrenceRuleChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Recurrence Rule Change History' },
  { path: 'recurrencerulechangehistories/:recurrenceRuleChangeHistoryId', component: RecurrenceRuleChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Recurrence Rule Change History' },
  { path: 'recurrencerulechangehistory/:recurrenceRuleChangeHistoryId', component: RecurrenceRuleChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Recurrence Rule Change History' },
  { path: 'recurrencerulechangehistory', redirectTo: 'recurrencerulechangehistories' },
  { path: 'relationshiptypes', component: RelationshipTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Relationship Types' },
  { path: 'relationshiptypes/new', component: RelationshipTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Relationship Type' },
  { path: 'relationshiptypes/:relationshipTypeId', component: RelationshipTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Relationship Type' },
  { path: 'relationshiptype/:relationshipTypeId', component: RelationshipTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Relationship Type' },
  { path: 'relationshiptype', redirectTo: 'relationshiptypes' },
  { path: 'resources', component: ResourceListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Resources' },
  { path: 'resources/new', component: ResourceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Resource' },
  { path: 'resources/:resourceId', component: ResourceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource' },
  { path: 'resource/:resourceId', component: ResourceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource' },
  { path: 'resource', redirectTo: 'resources' },
  { path: 'resourceavailabilities', component: ResourceAvailabilityListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Resource Availabilities' },
  { path: 'resourceavailabilities/new', component: ResourceAvailabilityDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Resource Availability' },
  { path: 'resourceavailabilities/:resourceAvailabilityId', component: ResourceAvailabilityDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Availability' },
  { path: 'resourceavailability/:resourceAvailabilityId', component: ResourceAvailabilityDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Availability' },
  { path: 'resourceavailability', redirectTo: 'resourceavailabilities' },
  { path: 'resourceavailabilitychangehistories', component: ResourceAvailabilityChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Resource Availability Change Histories' },
  { path: 'resourceavailabilitychangehistories/new', component: ResourceAvailabilityChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Resource Availability Change History' },
  { path: 'resourceavailabilitychangehistories/:resourceAvailabilityChangeHistoryId', component: ResourceAvailabilityChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Availability Change History' },
  { path: 'resourceavailabilitychangehistory/:resourceAvailabilityChangeHistoryId', component: ResourceAvailabilityChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Availability Change History' },
  { path: 'resourceavailabilitychangehistory', redirectTo: 'resourceavailabilitychangehistories' },
  { path: 'resourcechangehistories', component: ResourceChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Resource Change Histories' },
  { path: 'resourcechangehistories/new', component: ResourceChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Resource Change History' },
  { path: 'resourcechangehistories/:resourceChangeHistoryId', component: ResourceChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Change History' },
  { path: 'resourcechangehistory/:resourceChangeHistoryId', component: ResourceChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Change History' },
  { path: 'resourcechangehistory', redirectTo: 'resourcechangehistories' },
  { path: 'resourcecontacts', component: ResourceContactListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Resource Contacts' },
  { path: 'resourcecontacts/new', component: ResourceContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Resource Contact' },
  { path: 'resourcecontacts/:resourceContactId', component: ResourceContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Contact' },
  { path: 'resourcecontact/:resourceContactId', component: ResourceContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Contact' },
  { path: 'resourcecontact', redirectTo: 'resourcecontacts' },
  { path: 'resourcecontactchangehistories', component: ResourceContactChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Resource Contact Change Histories' },
  { path: 'resourcecontactchangehistories/new', component: ResourceContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Resource Contact Change History' },
  { path: 'resourcecontactchangehistories/:resourceContactChangeHistoryId', component: ResourceContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Contact Change History' },
  { path: 'resourcecontactchangehistory/:resourceContactChangeHistoryId', component: ResourceContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Contact Change History' },
  { path: 'resourcecontactchangehistory', redirectTo: 'resourcecontactchangehistories' },
  { path: 'resourcequalifications', component: ResourceQualificationListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Resource Qualifications' },
  { path: 'resourcequalifications/new', component: ResourceQualificationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Resource Qualification' },
  { path: 'resourcequalifications/:resourceQualificationId', component: ResourceQualificationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Qualification' },
  { path: 'resourcequalification/:resourceQualificationId', component: ResourceQualificationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Qualification' },
  { path: 'resourcequalification', redirectTo: 'resourcequalifications' },
  { path: 'resourcequalificationchangehistories', component: ResourceQualificationChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Resource Qualification Change Histories' },
  { path: 'resourcequalificationchangehistories/new', component: ResourceQualificationChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Resource Qualification Change History' },
  { path: 'resourcequalificationchangehistories/:resourceQualificationChangeHistoryId', component: ResourceQualificationChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Qualification Change History' },
  { path: 'resourcequalificationchangehistory/:resourceQualificationChangeHistoryId', component: ResourceQualificationChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Qualification Change History' },
  { path: 'resourcequalificationchangehistory', redirectTo: 'resourcequalificationchangehistories' },
  { path: 'resourceshifts', component: ResourceShiftListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Resource Shifts' },
  { path: 'resourceshifts/new', component: ResourceShiftDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Resource Shift' },
  { path: 'resourceshifts/:resourceShiftId', component: ResourceShiftDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Shift' },
  { path: 'resourceshift/:resourceShiftId', component: ResourceShiftDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Shift' },
  { path: 'resourceshift', redirectTo: 'resourceshifts' },
  { path: 'resourceshiftchangehistories', component: ResourceShiftChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Resource Shift Change Histories' },
  { path: 'resourceshiftchangehistories/new', component: ResourceShiftChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Resource Shift Change History' },
  { path: 'resourceshiftchangehistories/:resourceShiftChangeHistoryId', component: ResourceShiftChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Shift Change History' },
  { path: 'resourceshiftchangehistory/:resourceShiftChangeHistoryId', component: ResourceShiftChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Shift Change History' },
  { path: 'resourceshiftchangehistory', redirectTo: 'resourceshiftchangehistories' },
  { path: 'resourcetypes', component: ResourceTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Resource Types' },
  { path: 'resourcetypes/new', component: ResourceTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Resource Type' },
  { path: 'resourcetypes/:resourceTypeId', component: ResourceTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Type' },
  { path: 'resourcetype/:resourceTypeId', component: ResourceTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Resource Type' },
  { path: 'resourcetype', redirectTo: 'resourcetypes' },
  { path: 'salutations', component: SalutationListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Salutations' },
  { path: 'salutations/new', component: SalutationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Salutation' },
  { path: 'salutations/:salutationId', component: SalutationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Salutation' },
  { path: 'salutation/:salutationId', component: SalutationDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Salutation' },
  { path: 'salutation', redirectTo: 'salutations' },
  { path: 'scheduledevents', component: ScheduledEventListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduled Events' },
  { path: 'scheduledevents/new', component: ScheduledEventDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduled Event' },
  { path: 'scheduledevents/:scheduledEventId', component: ScheduledEventDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event' },
  { path: 'scheduledevent/:scheduledEventId', component: ScheduledEventDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event' },
  { path: 'scheduledevent', redirectTo: 'scheduledevents' },
  { path: 'scheduledeventchangehistories', component: ScheduledEventChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduled Event Change Histories' },
  { path: 'scheduledeventchangehistories/new', component: ScheduledEventChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduled Event Change History' },
  { path: 'scheduledeventchangehistories/:scheduledEventChangeHistoryId', component: ScheduledEventChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Change History' },
  { path: 'scheduledeventchangehistory/:scheduledEventChangeHistoryId', component: ScheduledEventChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Change History' },
  { path: 'scheduledeventchangehistory', redirectTo: 'scheduledeventchangehistories' },
  { path: 'scheduledeventdependencies', component: ScheduledEventDependencyListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduled Event Dependencies' },
  { path: 'scheduledeventdependencies/new', component: ScheduledEventDependencyDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduled Event Dependency' },
  { path: 'scheduledeventdependencies/:scheduledEventDependencyId', component: ScheduledEventDependencyDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Dependency' },
  { path: 'scheduledeventdependency/:scheduledEventDependencyId', component: ScheduledEventDependencyDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Dependency' },
  { path: 'scheduledeventdependency', redirectTo: 'scheduledeventdependencies' },
  { path: 'scheduledeventdependencychangehistories', component: ScheduledEventDependencyChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduled Event Dependency Change Histories' },
  { path: 'scheduledeventdependencychangehistories/new', component: ScheduledEventDependencyChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduled Event Dependency Change History' },
  { path: 'scheduledeventdependencychangehistories/:scheduledEventDependencyChangeHistoryId', component: ScheduledEventDependencyChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Dependency Change History' },
  { path: 'scheduledeventdependencychangehistory/:scheduledEventDependencyChangeHistoryId', component: ScheduledEventDependencyChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Dependency Change History' },
  { path: 'scheduledeventdependencychangehistory', redirectTo: 'scheduledeventdependencychangehistories' },
  { path: 'scheduledeventqualificationrequirements', component: ScheduledEventQualificationRequirementListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduled Event Qualification Requirements' },
  { path: 'scheduledeventqualificationrequirements/new', component: ScheduledEventQualificationRequirementDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduled Event Qualification Requirement' },
  { path: 'scheduledeventqualificationrequirements/:scheduledEventQualificationRequirementId', component: ScheduledEventQualificationRequirementDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Qualification Requirement' },
  { path: 'scheduledeventqualificationrequirement/:scheduledEventQualificationRequirementId', component: ScheduledEventQualificationRequirementDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Qualification Requirement' },
  { path: 'scheduledeventqualificationrequirement', redirectTo: 'scheduledeventqualificationrequirements' },
  { path: 'scheduledeventqualificationrequirementchangehistories', component: ScheduledEventQualificationRequirementChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduled Event Qualification Requirement Change Histories' },
  { path: 'scheduledeventqualificationrequirementchangehistories/new', component: ScheduledEventQualificationRequirementChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduled Event Qualification Requirement Change History' },
  { path: 'scheduledeventqualificationrequirementchangehistories/:scheduledEventQualificationRequirementChangeHistoryId', component: ScheduledEventQualificationRequirementChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Qualification Requirement Change History' },
  { path: 'scheduledeventqualificationrequirementchangehistory/:scheduledEventQualificationRequirementChangeHistoryId', component: ScheduledEventQualificationRequirementChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Qualification Requirement Change History' },
  { path: 'scheduledeventqualificationrequirementchangehistory', redirectTo: 'scheduledeventqualificationrequirementchangehistories' },
  { path: 'scheduledeventtemplates', component: ScheduledEventTemplateListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduled Event Templates' },
  { path: 'scheduledeventtemplates/new', component: ScheduledEventTemplateDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduled Event Template' },
  { path: 'scheduledeventtemplates/:scheduledEventTemplateId', component: ScheduledEventTemplateDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Template' },
  { path: 'scheduledeventtemplate/:scheduledEventTemplateId', component: ScheduledEventTemplateDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Template' },
  { path: 'scheduledeventtemplate', redirectTo: 'scheduledeventtemplates' },
  { path: 'scheduledeventtemplatechangehistories', component: ScheduledEventTemplateChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduled Event Template Change Histories' },
  { path: 'scheduledeventtemplatechangehistories/new', component: ScheduledEventTemplateChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduled Event Template Change History' },
  { path: 'scheduledeventtemplatechangehistories/:scheduledEventTemplateChangeHistoryId', component: ScheduledEventTemplateChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Template Change History' },
  { path: 'scheduledeventtemplatechangehistory/:scheduledEventTemplateChangeHistoryId', component: ScheduledEventTemplateChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Template Change History' },
  { path: 'scheduledeventtemplatechangehistory', redirectTo: 'scheduledeventtemplatechangehistories' },
  { path: 'scheduledeventtemplatecharges', component: ScheduledEventTemplateChargeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduled Event Template Charges' },
  { path: 'scheduledeventtemplatecharges/new', component: ScheduledEventTemplateChargeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduled Event Template Charge' },
  { path: 'scheduledeventtemplatecharges/:scheduledEventTemplateChargeId', component: ScheduledEventTemplateChargeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Template Charge' },
  { path: 'scheduledeventtemplatecharge/:scheduledEventTemplateChargeId', component: ScheduledEventTemplateChargeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Template Charge' },
  { path: 'scheduledeventtemplatecharge', redirectTo: 'scheduledeventtemplatecharges' },
  { path: 'scheduledeventtemplatechargechangehistories', component: ScheduledEventTemplateChargeChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduled Event Template Charge Change Histories' },
  { path: 'scheduledeventtemplatechargechangehistories/new', component: ScheduledEventTemplateChargeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduled Event Template Charge Change History' },
  { path: 'scheduledeventtemplatechargechangehistories/:scheduledEventTemplateChargeChangeHistoryId', component: ScheduledEventTemplateChargeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Template Charge Change History' },
  { path: 'scheduledeventtemplatechargechangehistory/:scheduledEventTemplateChargeChangeHistoryId', component: ScheduledEventTemplateChargeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Template Charge Change History' },
  { path: 'scheduledeventtemplatechargechangehistory', redirectTo: 'scheduledeventtemplatechargechangehistories' },
  { path: 'scheduledeventtemplatequalificationrequirements', component: ScheduledEventTemplateQualificationRequirementListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduled Event Template Qualification Requirements' },
  { path: 'scheduledeventtemplatequalificationrequirements/new', component: ScheduledEventTemplateQualificationRequirementDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduled Event Template Qualification Requirement' },
  { path: 'scheduledeventtemplatequalificationrequirements/:scheduledEventTemplateQualificationRequirementId', component: ScheduledEventTemplateQualificationRequirementDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Template Qualification Requirement' },
  { path: 'scheduledeventtemplatequalificationrequirement/:scheduledEventTemplateQualificationRequirementId', component: ScheduledEventTemplateQualificationRequirementDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Template Qualification Requirement' },
  { path: 'scheduledeventtemplatequalificationrequirement', redirectTo: 'scheduledeventtemplatequalificationrequirements' },
  { path: 'scheduledeventtemplatequalificationrequirementchangehistories', component: ScheduledEventTemplateQualificationRequirementChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduled Event Template Qualification Requirement Change Histories' },
  { path: 'scheduledeventtemplatequalificationrequirementchangehistories/new', component: ScheduledEventTemplateQualificationRequirementChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduled Event Template Qualification Requirement Change History' },
  { path: 'scheduledeventtemplatequalificationrequirementchangehistories/:scheduledEventTemplateQualificationRequirementChangeHistoryId', component: ScheduledEventTemplateQualificationRequirementChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Template Qualification Requirement Change History' },
  { path: 'scheduledeventtemplatequalificationrequirementchangehistory/:scheduledEventTemplateQualificationRequirementChangeHistoryId', component: ScheduledEventTemplateQualificationRequirementChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduled Event Template Qualification Requirement Change History' },
  { path: 'scheduledeventtemplatequalificationrequirementchangehistory', redirectTo: 'scheduledeventtemplatequalificationrequirementchangehistories' },
  { path: 'schedulingtargets', component: SchedulingTargetListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduling Targets' },
  { path: 'schedulingtargets/new', component: SchedulingTargetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduling Target' },
  { path: 'schedulingtargets/:schedulingTargetId', component: SchedulingTargetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target' },
  { path: 'schedulingtarget/:schedulingTargetId', component: SchedulingTargetDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target' },
  { path: 'schedulingtarget', redirectTo: 'schedulingtargets' },
  { path: 'schedulingtargetaddresses', component: SchedulingTargetAddressListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduling Target Addresses' },
  { path: 'schedulingtargetaddresses/new', component: SchedulingTargetAddressDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduling Target Address' },
  { path: 'schedulingtargetaddresses/:schedulingTargetAddressId', component: SchedulingTargetAddressDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target Address' },
  { path: 'schedulingtargetaddress/:schedulingTargetAddressId', component: SchedulingTargetAddressDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target Address' },
  { path: 'schedulingtargetaddress', redirectTo: 'schedulingtargetaddresses' },
  { path: 'schedulingtargetaddresschangehistories', component: SchedulingTargetAddressChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduling Target Address Change Histories' },
  { path: 'schedulingtargetaddresschangehistories/new', component: SchedulingTargetAddressChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduling Target Address Change History' },
  { path: 'schedulingtargetaddresschangehistories/:schedulingTargetAddressChangeHistoryId', component: SchedulingTargetAddressChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target Address Change History' },
  { path: 'schedulingtargetaddresschangehistory/:schedulingTargetAddressChangeHistoryId', component: SchedulingTargetAddressChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target Address Change History' },
  { path: 'schedulingtargetaddresschangehistory', redirectTo: 'schedulingtargetaddresschangehistories' },
  { path: 'schedulingtargetchangehistories', component: SchedulingTargetChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduling Target Change Histories' },
  { path: 'schedulingtargetchangehistories/new', component: SchedulingTargetChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduling Target Change History' },
  { path: 'schedulingtargetchangehistories/:schedulingTargetChangeHistoryId', component: SchedulingTargetChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target Change History' },
  { path: 'schedulingtargetchangehistory/:schedulingTargetChangeHistoryId', component: SchedulingTargetChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target Change History' },
  { path: 'schedulingtargetchangehistory', redirectTo: 'schedulingtargetchangehistories' },
  { path: 'schedulingtargetcontacts', component: SchedulingTargetContactListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduling Target Contacts' },
  { path: 'schedulingtargetcontacts/new', component: SchedulingTargetContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduling Target Contact' },
  { path: 'schedulingtargetcontacts/:schedulingTargetContactId', component: SchedulingTargetContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target Contact' },
  { path: 'schedulingtargetcontact/:schedulingTargetContactId', component: SchedulingTargetContactDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target Contact' },
  { path: 'schedulingtargetcontact', redirectTo: 'schedulingtargetcontacts' },
  { path: 'schedulingtargetcontactchangehistories', component: SchedulingTargetContactChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduling Target Contact Change Histories' },
  { path: 'schedulingtargetcontactchangehistories/new', component: SchedulingTargetContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduling Target Contact Change History' },
  { path: 'schedulingtargetcontactchangehistories/:schedulingTargetContactChangeHistoryId', component: SchedulingTargetContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target Contact Change History' },
  { path: 'schedulingtargetcontactchangehistory/:schedulingTargetContactChangeHistoryId', component: SchedulingTargetContactChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target Contact Change History' },
  { path: 'schedulingtargetcontactchangehistory', redirectTo: 'schedulingtargetcontactchangehistories' },
  { path: 'schedulingtargetqualificationrequirements', component: SchedulingTargetQualificationRequirementListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduling Target Qualification Requirements' },
  { path: 'schedulingtargetqualificationrequirements/new', component: SchedulingTargetQualificationRequirementDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduling Target Qualification Requirement' },
  { path: 'schedulingtargetqualificationrequirements/:schedulingTargetQualificationRequirementId', component: SchedulingTargetQualificationRequirementDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target Qualification Requirement' },
  { path: 'schedulingtargetqualificationrequirement/:schedulingTargetQualificationRequirementId', component: SchedulingTargetQualificationRequirementDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target Qualification Requirement' },
  { path: 'schedulingtargetqualificationrequirement', redirectTo: 'schedulingtargetqualificationrequirements' },
  { path: 'schedulingtargetqualificationrequirementchangehistories', component: SchedulingTargetQualificationRequirementChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduling Target Qualification Requirement Change Histories' },
  { path: 'schedulingtargetqualificationrequirementchangehistories/new', component: SchedulingTargetQualificationRequirementChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduling Target Qualification Requirement Change History' },
  { path: 'schedulingtargetqualificationrequirementchangehistories/:schedulingTargetQualificationRequirementChangeHistoryId', component: SchedulingTargetQualificationRequirementChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target Qualification Requirement Change History' },
  { path: 'schedulingtargetqualificationrequirementchangehistory/:schedulingTargetQualificationRequirementChangeHistoryId', component: SchedulingTargetQualificationRequirementChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target Qualification Requirement Change History' },
  { path: 'schedulingtargetqualificationrequirementchangehistory', redirectTo: 'schedulingtargetqualificationrequirementchangehistories' },
  { path: 'schedulingtargettypes', component: SchedulingTargetTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Scheduling Target Types' },
  { path: 'schedulingtargettypes/new', component: SchedulingTargetTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Scheduling Target Type' },
  { path: 'schedulingtargettypes/:schedulingTargetTypeId', component: SchedulingTargetTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target Type' },
  { path: 'schedulingtargettype/:schedulingTargetTypeId', component: SchedulingTargetTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Scheduling Target Type' },
  { path: 'schedulingtargettype', redirectTo: 'schedulingtargettypes' },
  { path: 'shiftpatterns', component: ShiftPatternListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Shift Patterns' },
  { path: 'shiftpatterns/new', component: ShiftPatternDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Shift Pattern' },
  { path: 'shiftpatterns/:shiftPatternId', component: ShiftPatternDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Shift Pattern' },
  { path: 'shiftpattern/:shiftPatternId', component: ShiftPatternDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Shift Pattern' },
  { path: 'shiftpattern', redirectTo: 'shiftpatterns' },
  { path: 'shiftpatternchangehistories', component: ShiftPatternChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Shift Pattern Change Histories' },
  { path: 'shiftpatternchangehistories/new', component: ShiftPatternChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Shift Pattern Change History' },
  { path: 'shiftpatternchangehistories/:shiftPatternChangeHistoryId', component: ShiftPatternChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Shift Pattern Change History' },
  { path: 'shiftpatternchangehistory/:shiftPatternChangeHistoryId', component: ShiftPatternChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Shift Pattern Change History' },
  { path: 'shiftpatternchangehistory', redirectTo: 'shiftpatternchangehistories' },
  { path: 'shiftpatterndays', component: ShiftPatternDayListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Shift Pattern Days' },
  { path: 'shiftpatterndays/new', component: ShiftPatternDayDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Shift Pattern Day' },
  { path: 'shiftpatterndays/:shiftPatternDayId', component: ShiftPatternDayDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Shift Pattern Day' },
  { path: 'shiftpatternday/:shiftPatternDayId', component: ShiftPatternDayDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Shift Pattern Day' },
  { path: 'shiftpatternday', redirectTo: 'shiftpatterndays' },
  { path: 'shiftpatterndaychangehistories', component: ShiftPatternDayChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Shift Pattern Day Change Histories' },
  { path: 'shiftpatterndaychangehistories/new', component: ShiftPatternDayChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Shift Pattern Day Change History' },
  { path: 'shiftpatterndaychangehistories/:shiftPatternDayChangeHistoryId', component: ShiftPatternDayChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Shift Pattern Day Change History' },
  { path: 'shiftpatterndaychangehistory/:shiftPatternDayChangeHistoryId', component: ShiftPatternDayChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Shift Pattern Day Change History' },
  { path: 'shiftpatterndaychangehistory', redirectTo: 'shiftpatterndaychangehistories' },
  { path: 'softcredits', component: SoftCreditListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Soft Credits' },
  { path: 'softcredits/new', component: SoftCreditDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Soft Credit' },
  { path: 'softcredits/:softCreditId', component: SoftCreditDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Soft Credit' },
  { path: 'softcredit/:softCreditId', component: SoftCreditDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Soft Credit' },
  { path: 'softcredit', redirectTo: 'softcredits' },
  { path: 'softcreditchangehistories', component: SoftCreditChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Soft Credit Change Histories' },
  { path: 'softcreditchangehistories/new', component: SoftCreditChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Soft Credit Change History' },
  { path: 'softcreditchangehistories/:softCreditChangeHistoryId', component: SoftCreditChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Soft Credit Change History' },
  { path: 'softcreditchangehistory/:softCreditChangeHistoryId', component: SoftCreditChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Soft Credit Change History' },
  { path: 'softcreditchangehistory', redirectTo: 'softcreditchangehistories' },
  { path: 'stateprovinces', component: StateProvinceListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'State Provinces' },
  { path: 'stateprovinces/new', component: StateProvinceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create State Province' },
  { path: 'stateprovinces/:stateProvinceId', component: StateProvinceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit State Province' },
  { path: 'stateprovince/:stateProvinceId', component: StateProvinceDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit State Province' },
  { path: 'stateprovince', redirectTo: 'stateprovinces' },
  { path: 'tags', component: TagListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Tags' },
  { path: 'tags/new', component: TagDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Tag' },
  { path: 'tags/:tagId', component: TagDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Tag' },
  { path: 'tag/:tagId', component: TagDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Tag' },
  { path: 'tag', redirectTo: 'tags' },
  { path: 'tenantprofiles', component: TenantProfileListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Tenant Profiles' },
  { path: 'tenantprofiles/new', component: TenantProfileDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Tenant Profile' },
  { path: 'tenantprofiles/:tenantProfileId', component: TenantProfileDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Tenant Profile' },
  { path: 'tenantprofile/:tenantProfileId', component: TenantProfileDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Tenant Profile' },
  { path: 'tenantprofile', redirectTo: 'tenantprofiles' },
  { path: 'tenantprofilechangehistories', component: TenantProfileChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Tenant Profile Change Histories' },
  { path: 'tenantprofilechangehistories/new', component: TenantProfileChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Tenant Profile Change History' },
  { path: 'tenantprofilechangehistories/:tenantProfileChangeHistoryId', component: TenantProfileChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Tenant Profile Change History' },
  { path: 'tenantprofilechangehistory/:tenantProfileChangeHistoryId', component: TenantProfileChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Tenant Profile Change History' },
  { path: 'tenantprofilechangehistory', redirectTo: 'tenantprofilechangehistories' },
  { path: 'timezones', component: TimeZoneListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Time Zones' },
  { path: 'timezones/new', component: TimeZoneDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Time Zone' },
  { path: 'timezones/:timeZoneId', component: TimeZoneDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Time Zone' },
  { path: 'timezone/:timeZoneId', component: TimeZoneDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Time Zone' },
  { path: 'timezone', redirectTo: 'timezones' },
  { path: 'tributes', component: TributeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Tributes' },
  { path: 'tributes/new', component: TributeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Tribute' },
  { path: 'tributes/:tributeId', component: TributeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Tribute' },
  { path: 'tribute/:tributeId', component: TributeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Tribute' },
  { path: 'tribute', redirectTo: 'tributes' },
  { path: 'tributechangehistories', component: TributeChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Tribute Change Histories' },
  { path: 'tributechangehistories/new', component: TributeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Tribute Change History' },
  { path: 'tributechangehistories/:tributeChangeHistoryId', component: TributeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Tribute Change History' },
  { path: 'tributechangehistory/:tributeChangeHistoryId', component: TributeChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Tribute Change History' },
  { path: 'tributechangehistory', redirectTo: 'tributechangehistories' },
  { path: 'tributetypes', component: TributeTypeListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Tribute Types' },
  { path: 'tributetypes/new', component: TributeTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Tribute Type' },
  { path: 'tributetypes/:tributeTypeId', component: TributeTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Tribute Type' },
  { path: 'tributetype/:tributeTypeId', component: TributeTypeDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Tribute Type' },
  { path: 'tributetype', redirectTo: 'tributetypes' },
  { path: 'volunteergroups', component: VolunteerGroupListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Volunteer Groups' },
  { path: 'volunteergroups/new', component: VolunteerGroupDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Volunteer Group' },
  { path: 'volunteergroups/:volunteerGroupId', component: VolunteerGroupDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Volunteer Group' },
  { path: 'volunteergroup/:volunteerGroupId', component: VolunteerGroupDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Volunteer Group' },
  { path: 'volunteergroup', redirectTo: 'volunteergroups' },
  { path: 'volunteergroupchangehistories', component: VolunteerGroupChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Volunteer Group Change Histories' },
  { path: 'volunteergroupchangehistories/new', component: VolunteerGroupChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Volunteer Group Change History' },
  { path: 'volunteergroupchangehistories/:volunteerGroupChangeHistoryId', component: VolunteerGroupChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Volunteer Group Change History' },
  { path: 'volunteergroupchangehistory/:volunteerGroupChangeHistoryId', component: VolunteerGroupChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Volunteer Group Change History' },
  { path: 'volunteergroupchangehistory', redirectTo: 'volunteergroupchangehistories' },
  { path: 'volunteergroupmembers', component: VolunteerGroupMemberListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Volunteer Group Members' },
  { path: 'volunteergroupmembers/new', component: VolunteerGroupMemberDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Volunteer Group Member' },
  { path: 'volunteergroupmembers/:volunteerGroupMemberId', component: VolunteerGroupMemberDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Volunteer Group Member' },
  { path: 'volunteergroupmember/:volunteerGroupMemberId', component: VolunteerGroupMemberDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Volunteer Group Member' },
  { path: 'volunteergroupmember', redirectTo: 'volunteergroupmembers' },
  { path: 'volunteergroupmemberchangehistories', component: VolunteerGroupMemberChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Volunteer Group Member Change Histories' },
  { path: 'volunteergroupmemberchangehistories/new', component: VolunteerGroupMemberChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Volunteer Group Member Change History' },
  { path: 'volunteergroupmemberchangehistories/:volunteerGroupMemberChangeHistoryId', component: VolunteerGroupMemberChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Volunteer Group Member Change History' },
  { path: 'volunteergroupmemberchangehistory/:volunteerGroupMemberChangeHistoryId', component: VolunteerGroupMemberChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Volunteer Group Member Change History' },
  { path: 'volunteergroupmemberchangehistory', redirectTo: 'volunteergroupmemberchangehistories' },
  { path: 'volunteerprofiles', component: VolunteerProfileListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Volunteer Profiles' },
  { path: 'volunteerprofiles/new', component: VolunteerProfileDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Volunteer Profile' },
  { path: 'volunteerprofiles/:volunteerProfileId', component: VolunteerProfileDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Volunteer Profile' },
  { path: 'volunteerprofile/:volunteerProfileId', component: VolunteerProfileDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Volunteer Profile' },
  { path: 'volunteerprofile', redirectTo: 'volunteerprofiles' },
  { path: 'volunteerprofilechangehistories', component: VolunteerProfileChangeHistoryListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Volunteer Profile Change Histories' },
  { path: 'volunteerprofilechangehistories/new', component: VolunteerProfileChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Volunteer Profile Change History' },
  { path: 'volunteerprofilechangehistories/:volunteerProfileChangeHistoryId', component: VolunteerProfileChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Volunteer Profile Change History' },
  { path: 'volunteerprofilechangehistory/:volunteerProfileChangeHistoryId', component: VolunteerProfileChangeHistoryDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Volunteer Profile Change History' },
  { path: 'volunteerprofilechangehistory', redirectTo: 'volunteerprofilechangehistories' },
  { path: 'volunteerstatuses', component: VolunteerStatusListingComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Volunteer Statuses' },
  { path: 'volunteerstatuses/new', component: VolunteerStatusDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Create Volunteer Status' },
  { path: 'volunteerstatuses/:volunteerStatusId', component: VolunteerStatusDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Volunteer Status' },
  { path: 'volunteerstatus/:volunteerStatusId', component: VolunteerStatusDetailComponent, canActivate: [AuthGuard], canDeactivate: [UnsavedChangesGuard], title: 'Edit Volunteer Status' },
  { path: 'volunteerstatus', redirectTo: 'volunteerstatuses' },
  //
  // End of routes for Scheduler Data Components
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
