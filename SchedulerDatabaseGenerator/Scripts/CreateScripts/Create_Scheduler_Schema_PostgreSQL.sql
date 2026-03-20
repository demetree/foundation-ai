/*
Scheduler scheduling system database schema.
This is a multi-tenant resource scheduling system designed primarily for construction resource planning
but flexible enough for other use cases. It supports events, individual and crew-based resource assignments,
partial time assignments, role designation, availability blackouts, and calendar grouping.
All operational tables include multi-tenant support, versioning where appropriate, auditing, and security controls.
*/
CREATE DATABASE "Scheduler"
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    TEMPLATE=template0;

\connect "Scheduler"   -- Run the create database first, then connect to it, and run the rest if running in a query tool rather than through psql scripting.



CREATE SCHEMA "Scheduler"

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE "Scheduler"."EventResourceAssignmentChangeHistory"
-- DROP TABLE "Scheduler"."EventResourceAssignment"
-- DROP TABLE "Scheduler"."DocumentShareLinkChangeHistory"
-- DROP TABLE "Scheduler"."DocumentShareLink"
-- DROP TABLE "Scheduler"."DocumentDocumentTagChangeHistory"
-- DROP TABLE "Scheduler"."DocumentDocumentTag"
-- DROP TABLE "Scheduler"."DocumentChangeHistory"
-- DROP TABLE "Scheduler"."Document"
-- DROP TABLE "Scheduler"."DocumentTagChangeHistory"
-- DROP TABLE "Scheduler"."DocumentTag"
-- DROP TABLE "Scheduler"."DocumentFolderChangeHistory"
-- DROP TABLE "Scheduler"."DocumentFolder"
-- DROP TABLE "Scheduler"."DocumentType"
-- DROP TABLE "Scheduler"."VolunteerGroupMemberChangeHistory"
-- DROP TABLE "Scheduler"."VolunteerGroupMember"
-- DROP TABLE "Scheduler"."VolunteerGroupChangeHistory"
-- DROP TABLE "Scheduler"."VolunteerGroup"
-- DROP TABLE "Scheduler"."VolunteerProfileChangeHistory"
-- DROP TABLE "Scheduler"."VolunteerProfile"
-- DROP TABLE "Scheduler"."SoftCreditChangeHistory"
-- DROP TABLE "Scheduler"."SoftCredit"
-- DROP TABLE "Scheduler"."GiftChangeHistory"
-- DROP TABLE "Scheduler"."Gift"
-- DROP TABLE "Scheduler"."TributeChangeHistory"
-- DROP TABLE "Scheduler"."Tribute"
-- DROP TABLE "Scheduler"."BatchChangeHistory"
-- DROP TABLE "Scheduler"."Batch"
-- DROP TABLE "Scheduler"."BatchStatus"
-- DROP TABLE "Scheduler"."TributeType"
-- DROP TABLE "Scheduler"."PledgeChangeHistory"
-- DROP TABLE "Scheduler"."Pledge"
-- DROP TABLE "Scheduler"."ConstituentChangeHistory"
-- DROP TABLE "Scheduler"."Constituent"
-- DROP TABLE "Scheduler"."ConstituentJourneyStageChangeHistory"
-- DROP TABLE "Scheduler"."ConstituentJourneyStage"
-- DROP TABLE "Scheduler"."HouseholdChangeHistory"
-- DROP TABLE "Scheduler"."Household"
-- DROP TABLE "Scheduler"."AppealChangeHistory"
-- DROP TABLE "Scheduler"."Appeal"
-- DROP TABLE "Scheduler"."CampaignChangeHistory"
-- DROP TABLE "Scheduler"."Campaign"
-- DROP TABLE "Scheduler"."FundChangeHistory"
-- DROP TABLE "Scheduler"."Fund"
-- DROP TABLE "Scheduler"."NotificationSubscriptionChangeHistory"
-- DROP TABLE "Scheduler"."NotificationSubscription"
-- DROP TABLE "Scheduler"."NotificationType"
-- DROP TABLE "Scheduler"."RecurrenceExceptionChangeHistory"
-- DROP TABLE "Scheduler"."RecurrenceException"
-- DROP TABLE "Scheduler"."ScheduledEventQualificationRequirementChangeHistory"
-- DROP TABLE "Scheduler"."ScheduledEventQualificationRequirement"
-- DROP TABLE "Scheduler"."ScheduledEventDependencyChangeHistory"
-- DROP TABLE "Scheduler"."ScheduledEventDependency"
-- DROP TABLE "Scheduler"."DependencyType"
-- DROP TABLE "Scheduler"."EventCalendar"
-- DROP TABLE "Scheduler"."ContactInteractionChangeHistory"
-- DROP TABLE "Scheduler"."ContactInteraction"
-- DROP TABLE "Scheduler"."ReceiptChangeHistory"
-- DROP TABLE "Scheduler"."Receipt"
-- DROP TABLE "Scheduler"."InvoiceLineItem"
-- DROP TABLE "Scheduler"."InvoiceChangeHistory"
-- DROP TABLE "Scheduler"."Invoice"
-- DROP TABLE "Scheduler"."InvoiceStatus"
-- DROP TABLE "Scheduler"."PaymentTransactionChangeHistory"
-- DROP TABLE "Scheduler"."PaymentTransaction"
-- DROP TABLE "Scheduler"."PaymentProviderChangeHistory"
-- DROP TABLE "Scheduler"."PaymentProvider"
-- DROP TABLE "Scheduler"."PaymentMethod"
-- DROP TABLE "Scheduler"."GeneralLedgerLine"
-- DROP TABLE "Scheduler"."GeneralLedgerEntry"
-- DROP TABLE "Scheduler"."BudgetChangeHistory"
-- DROP TABLE "Scheduler"."Budget"
-- DROP TABLE "Scheduler"."FinancialTransactionChangeHistory"
-- DROP TABLE "Scheduler"."FinancialTransaction"
-- DROP TABLE "Scheduler"."FiscalPeriodChangeHistory"
-- DROP TABLE "Scheduler"."FiscalPeriod"
-- DROP TABLE "Scheduler"."PeriodStatus"
-- DROP TABLE "Scheduler"."EventChargeChangeHistory"
-- DROP TABLE "Scheduler"."EventCharge"
-- DROP TABLE "Scheduler"."ChargeStatusChangeHistory"
-- DROP TABLE "Scheduler"."ChargeStatus"
-- DROP TABLE "Scheduler"."ScheduledEventChangeHistory"
-- DROP TABLE "Scheduler"."ScheduledEvent"
-- DROP TABLE "Scheduler"."EventTypeChangeHistory"
-- DROP TABLE "Scheduler"."EventType"
-- DROP TABLE "Scheduler"."ScheduledEventTemplateQualificationRequirementChangeHistory"
-- DROP TABLE "Scheduler"."ScheduledEventTemplateQualificationRequirement"
-- DROP TABLE "Scheduler"."ScheduledEventTemplateChargeChangeHistory"
-- DROP TABLE "Scheduler"."ScheduledEventTemplateCharge"
-- DROP TABLE "Scheduler"."ScheduledEventTemplateChangeHistory"
-- DROP TABLE "Scheduler"."ScheduledEventTemplate"
-- DROP TABLE "Scheduler"."CrewMemberChangeHistory"
-- DROP TABLE "Scheduler"."CrewMember"
-- DROP TABLE "Scheduler"."CrewChangeHistory"
-- DROP TABLE "Scheduler"."Crew"
-- DROP TABLE "Scheduler"."ResourceShiftChangeHistory"
-- DROP TABLE "Scheduler"."ResourceShift"
-- DROP TABLE "Scheduler"."ResourceAvailabilityChangeHistory"
-- DROP TABLE "Scheduler"."ResourceAvailability"
-- DROP TABLE "Scheduler"."ResourceQualificationChangeHistory"
-- DROP TABLE "Scheduler"."ResourceQualification"
-- DROP TABLE "Scheduler"."RateSheetChangeHistory"
-- DROP TABLE "Scheduler"."RateSheet"
-- DROP TABLE "Scheduler"."ResourceContactChangeHistory"
-- DROP TABLE "Scheduler"."ResourceContact"
-- DROP TABLE "Scheduler"."ResourceChangeHistory"
-- DROP TABLE "Scheduler"."Resource"
-- DROP TABLE "Scheduler"."ShiftPatternDayChangeHistory"
-- DROP TABLE "Scheduler"."ShiftPatternDay"
-- DROP TABLE "Scheduler"."ShiftPatternChangeHistory"
-- DROP TABLE "Scheduler"."ShiftPattern"
-- DROP TABLE "Scheduler"."RecurrenceRuleChangeHistory"
-- DROP TABLE "Scheduler"."RecurrenceRule"
-- DROP TABLE "Scheduler"."RecurrenceFrequency"
-- DROP TABLE "Scheduler"."SchedulingTargetQualificationRequirementChangeHistory"
-- DROP TABLE "Scheduler"."SchedulingTargetQualificationRequirement"
-- DROP TABLE "Scheduler"."SchedulingTargetAddressChangeHistory"
-- DROP TABLE "Scheduler"."SchedulingTargetAddress"
-- DROP TABLE "Scheduler"."SchedulingTargetContactChangeHistory"
-- DROP TABLE "Scheduler"."SchedulingTargetContact"
-- DROP TABLE "Scheduler"."SchedulingTargetChangeHistory"
-- DROP TABLE "Scheduler"."SchedulingTarget"
-- DROP TABLE "Scheduler"."SchedulingTargetType"
-- DROP TABLE "Scheduler"."AssignmentStatus"
-- DROP TABLE "Scheduler"."BookingSourceType"
-- DROP TABLE "Scheduler"."ReceiptTypeChangeHistory"
-- DROP TABLE "Scheduler"."ReceiptType"
-- DROP TABLE "Scheduler"."PaymentTypeChangeHistory"
-- DROP TABLE "Scheduler"."PaymentType"
-- DROP TABLE "Scheduler"."EventStatus"
-- DROP TABLE "Scheduler"."AssignmentRoleQualificationRequirementChangeHistory"
-- DROP TABLE "Scheduler"."AssignmentRoleQualificationRequirement"
-- DROP TABLE "Scheduler"."AssignmentRole"
-- DROP TABLE "Scheduler"."Qualification"
-- DROP TABLE "Scheduler"."TenantProfileChangeHistory"
-- DROP TABLE "Scheduler"."TenantProfile"
-- DROP TABLE "Scheduler"."ClientContactChangeHistory"
-- DROP TABLE "Scheduler"."ClientContact"
-- DROP TABLE "Scheduler"."ClientChangeHistory"
-- DROP TABLE "Scheduler"."Client"
-- DROP TABLE "Scheduler"."ClientType"
-- DROP TABLE "Scheduler"."CalendarChangeHistory"
-- DROP TABLE "Scheduler"."Calendar"
-- DROP TABLE "Scheduler"."OfficeContactChangeHistory"
-- DROP TABLE "Scheduler"."OfficeContact"
-- DROP TABLE "Scheduler"."OfficeChangeHistory"
-- DROP TABLE "Scheduler"."Office"
-- DROP TABLE "Scheduler"."OfficeType"
-- DROP TABLE "Scheduler"."ContactContactChangeHistory"
-- DROP TABLE "Scheduler"."ContactContact"
-- DROP TABLE "Scheduler"."RelationshipType"
-- DROP TABLE "Scheduler"."ContactTagChangeHistory"
-- DROP TABLE "Scheduler"."ContactTag"
-- DROP TABLE "Scheduler"."ContactChangeHistory"
-- DROP TABLE "Scheduler"."Contact"
-- DROP TABLE "Scheduler"."ContactType"
-- DROP TABLE "Scheduler"."VolunteerStatus"
-- DROP TABLE "Scheduler"."StateProvince"
-- DROP TABLE "Scheduler"."Country"
-- DROP TABLE "Scheduler"."TimeZone"
-- DROP TABLE "Scheduler"."Tag"
-- DROP TABLE "Scheduler"."ChargeTypeChangeHistory"
-- DROP TABLE "Scheduler"."ChargeType"
-- DROP TABLE "Scheduler"."TaxCode"
-- DROP TABLE "Scheduler"."FinancialCategoryChangeHistory"
-- DROP TABLE "Scheduler"."FinancialCategory"
-- DROP TABLE "Scheduler"."FinancialOfficeChangeHistory"
-- DROP TABLE "Scheduler"."FinancialOffice"
-- DROP TABLE "Scheduler"."AccountType"
-- DROP TABLE "Scheduler"."Currency"
-- DROP TABLE "Scheduler"."InteractionType"
-- DROP TABLE "Scheduler"."RateType"
-- DROP TABLE "Scheduler"."ContactMethod"
-- DROP TABLE "Scheduler"."Priority"
-- DROP TABLE "Scheduler"."ResourceType"
-- DROP TABLE "Scheduler"."Salutation"
-- DROP TABLE "Scheduler"."Icon"
-- DROP TABLE "Scheduler"."AttributeDefinitionChangeHistory"
-- DROP TABLE "Scheduler"."AttributeDefinition"
-- DROP TABLE "Scheduler"."AttributeDefinitionEntity"
-- DROP TABLE "Scheduler"."AttributeDefinitionType"

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON "EventResourceAssignmentChangeHistory" DISABLE
-- ALTER INDEX ALL ON "EventResourceAssignment" DISABLE
-- ALTER INDEX ALL ON "DocumentShareLinkChangeHistory" DISABLE
-- ALTER INDEX ALL ON "DocumentShareLink" DISABLE
-- ALTER INDEX ALL ON "DocumentDocumentTagChangeHistory" DISABLE
-- ALTER INDEX ALL ON "DocumentDocumentTag" DISABLE
-- ALTER INDEX ALL ON "DocumentChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Document" DISABLE
-- ALTER INDEX ALL ON "DocumentTagChangeHistory" DISABLE
-- ALTER INDEX ALL ON "DocumentTag" DISABLE
-- ALTER INDEX ALL ON "DocumentFolderChangeHistory" DISABLE
-- ALTER INDEX ALL ON "DocumentFolder" DISABLE
-- ALTER INDEX ALL ON "DocumentType" DISABLE
-- ALTER INDEX ALL ON "VolunteerGroupMemberChangeHistory" DISABLE
-- ALTER INDEX ALL ON "VolunteerGroupMember" DISABLE
-- ALTER INDEX ALL ON "VolunteerGroupChangeHistory" DISABLE
-- ALTER INDEX ALL ON "VolunteerGroup" DISABLE
-- ALTER INDEX ALL ON "VolunteerProfileChangeHistory" DISABLE
-- ALTER INDEX ALL ON "VolunteerProfile" DISABLE
-- ALTER INDEX ALL ON "SoftCreditChangeHistory" DISABLE
-- ALTER INDEX ALL ON "SoftCredit" DISABLE
-- ALTER INDEX ALL ON "GiftChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Gift" DISABLE
-- ALTER INDEX ALL ON "TributeChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Tribute" DISABLE
-- ALTER INDEX ALL ON "BatchChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Batch" DISABLE
-- ALTER INDEX ALL ON "BatchStatus" DISABLE
-- ALTER INDEX ALL ON "TributeType" DISABLE
-- ALTER INDEX ALL ON "PledgeChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Pledge" DISABLE
-- ALTER INDEX ALL ON "ConstituentChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Constituent" DISABLE
-- ALTER INDEX ALL ON "ConstituentJourneyStageChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ConstituentJourneyStage" DISABLE
-- ALTER INDEX ALL ON "HouseholdChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Household" DISABLE
-- ALTER INDEX ALL ON "AppealChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Appeal" DISABLE
-- ALTER INDEX ALL ON "CampaignChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Campaign" DISABLE
-- ALTER INDEX ALL ON "FundChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Fund" DISABLE
-- ALTER INDEX ALL ON "NotificationSubscriptionChangeHistory" DISABLE
-- ALTER INDEX ALL ON "NotificationSubscription" DISABLE
-- ALTER INDEX ALL ON "NotificationType" DISABLE
-- ALTER INDEX ALL ON "RecurrenceExceptionChangeHistory" DISABLE
-- ALTER INDEX ALL ON "RecurrenceException" DISABLE
-- ALTER INDEX ALL ON "ScheduledEventQualificationRequirementChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ScheduledEventQualificationRequirement" DISABLE
-- ALTER INDEX ALL ON "ScheduledEventDependencyChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ScheduledEventDependency" DISABLE
-- ALTER INDEX ALL ON "DependencyType" DISABLE
-- ALTER INDEX ALL ON "EventCalendar" DISABLE
-- ALTER INDEX ALL ON "ContactInteractionChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ContactInteraction" DISABLE
-- ALTER INDEX ALL ON "ReceiptChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Receipt" DISABLE
-- ALTER INDEX ALL ON "InvoiceLineItem" DISABLE
-- ALTER INDEX ALL ON "InvoiceChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Invoice" DISABLE
-- ALTER INDEX ALL ON "InvoiceStatus" DISABLE
-- ALTER INDEX ALL ON "PaymentTransactionChangeHistory" DISABLE
-- ALTER INDEX ALL ON "PaymentTransaction" DISABLE
-- ALTER INDEX ALL ON "PaymentProviderChangeHistory" DISABLE
-- ALTER INDEX ALL ON "PaymentProvider" DISABLE
-- ALTER INDEX ALL ON "PaymentMethod" DISABLE
-- ALTER INDEX ALL ON "GeneralLedgerLine" DISABLE
-- ALTER INDEX ALL ON "GeneralLedgerEntry" DISABLE
-- ALTER INDEX ALL ON "BudgetChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Budget" DISABLE
-- ALTER INDEX ALL ON "FinancialTransactionChangeHistory" DISABLE
-- ALTER INDEX ALL ON "FinancialTransaction" DISABLE
-- ALTER INDEX ALL ON "FiscalPeriodChangeHistory" DISABLE
-- ALTER INDEX ALL ON "FiscalPeriod" DISABLE
-- ALTER INDEX ALL ON "PeriodStatus" DISABLE
-- ALTER INDEX ALL ON "EventChargeChangeHistory" DISABLE
-- ALTER INDEX ALL ON "EventCharge" DISABLE
-- ALTER INDEX ALL ON "ChargeStatusChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ChargeStatus" DISABLE
-- ALTER INDEX ALL ON "ScheduledEventChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ScheduledEvent" DISABLE
-- ALTER INDEX ALL ON "EventTypeChangeHistory" DISABLE
-- ALTER INDEX ALL ON "EventType" DISABLE
-- ALTER INDEX ALL ON "ScheduledEventTemplateQualificationRequirementChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ScheduledEventTemplateQualificationRequirement" DISABLE
-- ALTER INDEX ALL ON "ScheduledEventTemplateChargeChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ScheduledEventTemplateCharge" DISABLE
-- ALTER INDEX ALL ON "ScheduledEventTemplateChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ScheduledEventTemplate" DISABLE
-- ALTER INDEX ALL ON "CrewMemberChangeHistory" DISABLE
-- ALTER INDEX ALL ON "CrewMember" DISABLE
-- ALTER INDEX ALL ON "CrewChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Crew" DISABLE
-- ALTER INDEX ALL ON "ResourceShiftChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ResourceShift" DISABLE
-- ALTER INDEX ALL ON "ResourceAvailabilityChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ResourceAvailability" DISABLE
-- ALTER INDEX ALL ON "ResourceQualificationChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ResourceQualification" DISABLE
-- ALTER INDEX ALL ON "RateSheetChangeHistory" DISABLE
-- ALTER INDEX ALL ON "RateSheet" DISABLE
-- ALTER INDEX ALL ON "ResourceContactChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ResourceContact" DISABLE
-- ALTER INDEX ALL ON "ResourceChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Resource" DISABLE
-- ALTER INDEX ALL ON "ShiftPatternDayChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ShiftPatternDay" DISABLE
-- ALTER INDEX ALL ON "ShiftPatternChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ShiftPattern" DISABLE
-- ALTER INDEX ALL ON "RecurrenceRuleChangeHistory" DISABLE
-- ALTER INDEX ALL ON "RecurrenceRule" DISABLE
-- ALTER INDEX ALL ON "RecurrenceFrequency" DISABLE
-- ALTER INDEX ALL ON "SchedulingTargetQualificationRequirementChangeHistory" DISABLE
-- ALTER INDEX ALL ON "SchedulingTargetQualificationRequirement" DISABLE
-- ALTER INDEX ALL ON "SchedulingTargetAddressChangeHistory" DISABLE
-- ALTER INDEX ALL ON "SchedulingTargetAddress" DISABLE
-- ALTER INDEX ALL ON "SchedulingTargetContactChangeHistory" DISABLE
-- ALTER INDEX ALL ON "SchedulingTargetContact" DISABLE
-- ALTER INDEX ALL ON "SchedulingTargetChangeHistory" DISABLE
-- ALTER INDEX ALL ON "SchedulingTarget" DISABLE
-- ALTER INDEX ALL ON "SchedulingTargetType" DISABLE
-- ALTER INDEX ALL ON "AssignmentStatus" DISABLE
-- ALTER INDEX ALL ON "BookingSourceType" DISABLE
-- ALTER INDEX ALL ON "ReceiptTypeChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ReceiptType" DISABLE
-- ALTER INDEX ALL ON "PaymentTypeChangeHistory" DISABLE
-- ALTER INDEX ALL ON "PaymentType" DISABLE
-- ALTER INDEX ALL ON "EventStatus" DISABLE
-- ALTER INDEX ALL ON "AssignmentRoleQualificationRequirementChangeHistory" DISABLE
-- ALTER INDEX ALL ON "AssignmentRoleQualificationRequirement" DISABLE
-- ALTER INDEX ALL ON "AssignmentRole" DISABLE
-- ALTER INDEX ALL ON "Qualification" DISABLE
-- ALTER INDEX ALL ON "TenantProfileChangeHistory" DISABLE
-- ALTER INDEX ALL ON "TenantProfile" DISABLE
-- ALTER INDEX ALL ON "ClientContactChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ClientContact" DISABLE
-- ALTER INDEX ALL ON "ClientChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Client" DISABLE
-- ALTER INDEX ALL ON "ClientType" DISABLE
-- ALTER INDEX ALL ON "CalendarChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Calendar" DISABLE
-- ALTER INDEX ALL ON "OfficeContactChangeHistory" DISABLE
-- ALTER INDEX ALL ON "OfficeContact" DISABLE
-- ALTER INDEX ALL ON "OfficeChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Office" DISABLE
-- ALTER INDEX ALL ON "OfficeType" DISABLE
-- ALTER INDEX ALL ON "ContactContactChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ContactContact" DISABLE
-- ALTER INDEX ALL ON "RelationshipType" DISABLE
-- ALTER INDEX ALL ON "ContactTagChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ContactTag" DISABLE
-- ALTER INDEX ALL ON "ContactChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Contact" DISABLE
-- ALTER INDEX ALL ON "ContactType" DISABLE
-- ALTER INDEX ALL ON "VolunteerStatus" DISABLE
-- ALTER INDEX ALL ON "StateProvince" DISABLE
-- ALTER INDEX ALL ON "Country" DISABLE
-- ALTER INDEX ALL ON "TimeZone" DISABLE
-- ALTER INDEX ALL ON "Tag" DISABLE
-- ALTER INDEX ALL ON "ChargeTypeChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ChargeType" DISABLE
-- ALTER INDEX ALL ON "TaxCode" DISABLE
-- ALTER INDEX ALL ON "FinancialCategoryChangeHistory" DISABLE
-- ALTER INDEX ALL ON "FinancialCategory" DISABLE
-- ALTER INDEX ALL ON "FinancialOfficeChangeHistory" DISABLE
-- ALTER INDEX ALL ON "FinancialOffice" DISABLE
-- ALTER INDEX ALL ON "AccountType" DISABLE
-- ALTER INDEX ALL ON "Currency" DISABLE
-- ALTER INDEX ALL ON "InteractionType" DISABLE
-- ALTER INDEX ALL ON "RateType" DISABLE
-- ALTER INDEX ALL ON "ContactMethod" DISABLE
-- ALTER INDEX ALL ON "Priority" DISABLE
-- ALTER INDEX ALL ON "ResourceType" DISABLE
-- ALTER INDEX ALL ON "Salutation" DISABLE
-- ALTER INDEX ALL ON "Icon" DISABLE
-- ALTER INDEX ALL ON "AttributeDefinitionChangeHistory" DISABLE
-- ALTER INDEX ALL ON "AttributeDefinition" DISABLE
-- ALTER INDEX ALL ON "AttributeDefinitionEntity" DISABLE
-- ALTER INDEX ALL ON "AttributeDefinitionType" DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON "EventResourceAssignmentChangeHistory" REBUILD
-- ALTER INDEX ALL ON "EventResourceAssignment" REBUILD
-- ALTER INDEX ALL ON "DocumentShareLinkChangeHistory" REBUILD
-- ALTER INDEX ALL ON "DocumentShareLink" REBUILD
-- ALTER INDEX ALL ON "DocumentDocumentTagChangeHistory" REBUILD
-- ALTER INDEX ALL ON "DocumentDocumentTag" REBUILD
-- ALTER INDEX ALL ON "DocumentChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Document" REBUILD
-- ALTER INDEX ALL ON "DocumentTagChangeHistory" REBUILD
-- ALTER INDEX ALL ON "DocumentTag" REBUILD
-- ALTER INDEX ALL ON "DocumentFolderChangeHistory" REBUILD
-- ALTER INDEX ALL ON "DocumentFolder" REBUILD
-- ALTER INDEX ALL ON "DocumentType" REBUILD
-- ALTER INDEX ALL ON "VolunteerGroupMemberChangeHistory" REBUILD
-- ALTER INDEX ALL ON "VolunteerGroupMember" REBUILD
-- ALTER INDEX ALL ON "VolunteerGroupChangeHistory" REBUILD
-- ALTER INDEX ALL ON "VolunteerGroup" REBUILD
-- ALTER INDEX ALL ON "VolunteerProfileChangeHistory" REBUILD
-- ALTER INDEX ALL ON "VolunteerProfile" REBUILD
-- ALTER INDEX ALL ON "SoftCreditChangeHistory" REBUILD
-- ALTER INDEX ALL ON "SoftCredit" REBUILD
-- ALTER INDEX ALL ON "GiftChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Gift" REBUILD
-- ALTER INDEX ALL ON "TributeChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Tribute" REBUILD
-- ALTER INDEX ALL ON "BatchChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Batch" REBUILD
-- ALTER INDEX ALL ON "BatchStatus" REBUILD
-- ALTER INDEX ALL ON "TributeType" REBUILD
-- ALTER INDEX ALL ON "PledgeChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Pledge" REBUILD
-- ALTER INDEX ALL ON "ConstituentChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Constituent" REBUILD
-- ALTER INDEX ALL ON "ConstituentJourneyStageChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ConstituentJourneyStage" REBUILD
-- ALTER INDEX ALL ON "HouseholdChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Household" REBUILD
-- ALTER INDEX ALL ON "AppealChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Appeal" REBUILD
-- ALTER INDEX ALL ON "CampaignChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Campaign" REBUILD
-- ALTER INDEX ALL ON "FundChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Fund" REBUILD
-- ALTER INDEX ALL ON "NotificationSubscriptionChangeHistory" REBUILD
-- ALTER INDEX ALL ON "NotificationSubscription" REBUILD
-- ALTER INDEX ALL ON "NotificationType" REBUILD
-- ALTER INDEX ALL ON "RecurrenceExceptionChangeHistory" REBUILD
-- ALTER INDEX ALL ON "RecurrenceException" REBUILD
-- ALTER INDEX ALL ON "ScheduledEventQualificationRequirementChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ScheduledEventQualificationRequirement" REBUILD
-- ALTER INDEX ALL ON "ScheduledEventDependencyChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ScheduledEventDependency" REBUILD
-- ALTER INDEX ALL ON "DependencyType" REBUILD
-- ALTER INDEX ALL ON "EventCalendar" REBUILD
-- ALTER INDEX ALL ON "ContactInteractionChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ContactInteraction" REBUILD
-- ALTER INDEX ALL ON "ReceiptChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Receipt" REBUILD
-- ALTER INDEX ALL ON "InvoiceLineItem" REBUILD
-- ALTER INDEX ALL ON "InvoiceChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Invoice" REBUILD
-- ALTER INDEX ALL ON "InvoiceStatus" REBUILD
-- ALTER INDEX ALL ON "PaymentTransactionChangeHistory" REBUILD
-- ALTER INDEX ALL ON "PaymentTransaction" REBUILD
-- ALTER INDEX ALL ON "PaymentProviderChangeHistory" REBUILD
-- ALTER INDEX ALL ON "PaymentProvider" REBUILD
-- ALTER INDEX ALL ON "PaymentMethod" REBUILD
-- ALTER INDEX ALL ON "GeneralLedgerLine" REBUILD
-- ALTER INDEX ALL ON "GeneralLedgerEntry" REBUILD
-- ALTER INDEX ALL ON "BudgetChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Budget" REBUILD
-- ALTER INDEX ALL ON "FinancialTransactionChangeHistory" REBUILD
-- ALTER INDEX ALL ON "FinancialTransaction" REBUILD
-- ALTER INDEX ALL ON "FiscalPeriodChangeHistory" REBUILD
-- ALTER INDEX ALL ON "FiscalPeriod" REBUILD
-- ALTER INDEX ALL ON "PeriodStatus" REBUILD
-- ALTER INDEX ALL ON "EventChargeChangeHistory" REBUILD
-- ALTER INDEX ALL ON "EventCharge" REBUILD
-- ALTER INDEX ALL ON "ChargeStatusChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ChargeStatus" REBUILD
-- ALTER INDEX ALL ON "ScheduledEventChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ScheduledEvent" REBUILD
-- ALTER INDEX ALL ON "EventTypeChangeHistory" REBUILD
-- ALTER INDEX ALL ON "EventType" REBUILD
-- ALTER INDEX ALL ON "ScheduledEventTemplateQualificationRequirementChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ScheduledEventTemplateQualificationRequirement" REBUILD
-- ALTER INDEX ALL ON "ScheduledEventTemplateChargeChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ScheduledEventTemplateCharge" REBUILD
-- ALTER INDEX ALL ON "ScheduledEventTemplateChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ScheduledEventTemplate" REBUILD
-- ALTER INDEX ALL ON "CrewMemberChangeHistory" REBUILD
-- ALTER INDEX ALL ON "CrewMember" REBUILD
-- ALTER INDEX ALL ON "CrewChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Crew" REBUILD
-- ALTER INDEX ALL ON "ResourceShiftChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ResourceShift" REBUILD
-- ALTER INDEX ALL ON "ResourceAvailabilityChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ResourceAvailability" REBUILD
-- ALTER INDEX ALL ON "ResourceQualificationChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ResourceQualification" REBUILD
-- ALTER INDEX ALL ON "RateSheetChangeHistory" REBUILD
-- ALTER INDEX ALL ON "RateSheet" REBUILD
-- ALTER INDEX ALL ON "ResourceContactChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ResourceContact" REBUILD
-- ALTER INDEX ALL ON "ResourceChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Resource" REBUILD
-- ALTER INDEX ALL ON "ShiftPatternDayChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ShiftPatternDay" REBUILD
-- ALTER INDEX ALL ON "ShiftPatternChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ShiftPattern" REBUILD
-- ALTER INDEX ALL ON "RecurrenceRuleChangeHistory" REBUILD
-- ALTER INDEX ALL ON "RecurrenceRule" REBUILD
-- ALTER INDEX ALL ON "RecurrenceFrequency" REBUILD
-- ALTER INDEX ALL ON "SchedulingTargetQualificationRequirementChangeHistory" REBUILD
-- ALTER INDEX ALL ON "SchedulingTargetQualificationRequirement" REBUILD
-- ALTER INDEX ALL ON "SchedulingTargetAddressChangeHistory" REBUILD
-- ALTER INDEX ALL ON "SchedulingTargetAddress" REBUILD
-- ALTER INDEX ALL ON "SchedulingTargetContactChangeHistory" REBUILD
-- ALTER INDEX ALL ON "SchedulingTargetContact" REBUILD
-- ALTER INDEX ALL ON "SchedulingTargetChangeHistory" REBUILD
-- ALTER INDEX ALL ON "SchedulingTarget" REBUILD
-- ALTER INDEX ALL ON "SchedulingTargetType" REBUILD
-- ALTER INDEX ALL ON "AssignmentStatus" REBUILD
-- ALTER INDEX ALL ON "BookingSourceType" REBUILD
-- ALTER INDEX ALL ON "ReceiptTypeChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ReceiptType" REBUILD
-- ALTER INDEX ALL ON "PaymentTypeChangeHistory" REBUILD
-- ALTER INDEX ALL ON "PaymentType" REBUILD
-- ALTER INDEX ALL ON "EventStatus" REBUILD
-- ALTER INDEX ALL ON "AssignmentRoleQualificationRequirementChangeHistory" REBUILD
-- ALTER INDEX ALL ON "AssignmentRoleQualificationRequirement" REBUILD
-- ALTER INDEX ALL ON "AssignmentRole" REBUILD
-- ALTER INDEX ALL ON "Qualification" REBUILD
-- ALTER INDEX ALL ON "TenantProfileChangeHistory" REBUILD
-- ALTER INDEX ALL ON "TenantProfile" REBUILD
-- ALTER INDEX ALL ON "ClientContactChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ClientContact" REBUILD
-- ALTER INDEX ALL ON "ClientChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Client" REBUILD
-- ALTER INDEX ALL ON "ClientType" REBUILD
-- ALTER INDEX ALL ON "CalendarChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Calendar" REBUILD
-- ALTER INDEX ALL ON "OfficeContactChangeHistory" REBUILD
-- ALTER INDEX ALL ON "OfficeContact" REBUILD
-- ALTER INDEX ALL ON "OfficeChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Office" REBUILD
-- ALTER INDEX ALL ON "OfficeType" REBUILD
-- ALTER INDEX ALL ON "ContactContactChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ContactContact" REBUILD
-- ALTER INDEX ALL ON "RelationshipType" REBUILD
-- ALTER INDEX ALL ON "ContactTagChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ContactTag" REBUILD
-- ALTER INDEX ALL ON "ContactChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Contact" REBUILD
-- ALTER INDEX ALL ON "ContactType" REBUILD
-- ALTER INDEX ALL ON "VolunteerStatus" REBUILD
-- ALTER INDEX ALL ON "StateProvince" REBUILD
-- ALTER INDEX ALL ON "Country" REBUILD
-- ALTER INDEX ALL ON "TimeZone" REBUILD
-- ALTER INDEX ALL ON "Tag" REBUILD
-- ALTER INDEX ALL ON "ChargeTypeChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ChargeType" REBUILD
-- ALTER INDEX ALL ON "TaxCode" REBUILD
-- ALTER INDEX ALL ON "FinancialCategoryChangeHistory" REBUILD
-- ALTER INDEX ALL ON "FinancialCategory" REBUILD
-- ALTER INDEX ALL ON "FinancialOfficeChangeHistory" REBUILD
-- ALTER INDEX ALL ON "FinancialOffice" REBUILD
-- ALTER INDEX ALL ON "AccountType" REBUILD
-- ALTER INDEX ALL ON "Currency" REBUILD
-- ALTER INDEX ALL ON "InteractionType" REBUILD
-- ALTER INDEX ALL ON "RateType" REBUILD
-- ALTER INDEX ALL ON "ContactMethod" REBUILD
-- ALTER INDEX ALL ON "Priority" REBUILD
-- ALTER INDEX ALL ON "ResourceType" REBUILD
-- ALTER INDEX ALL ON "Salutation" REBUILD
-- ALTER INDEX ALL ON "Icon" REBUILD
-- ALTER INDEX ALL ON "AttributeDefinitionChangeHistory" REBUILD
-- ALTER INDEX ALL ON "AttributeDefinition" REBUILD
-- ALTER INDEX ALL ON "AttributeDefinitionEntity" REBUILD
-- ALTER INDEX ALL ON "AttributeDefinitionType" REBUILD

-- Master list of available attribute data types.
CREATE TABLE "Scheduler"."AttributeDefinitionType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the AttributeDefinitionType table's name field.
CREATE INDEX "I_AttributeDefinitionType_name" ON "Scheduler"."AttributeDefinitionType" ("name")
;

-- Index on the AttributeDefinitionType table's active field.
CREATE INDEX "I_AttributeDefinitionType_active" ON "Scheduler"."AttributeDefinitionType" ("active")
;

-- Index on the AttributeDefinitionType table's deleted field.
CREATE INDEX "I_AttributeDefinitionType_deleted" ON "Scheduler"."AttributeDefinitionType" ("deleted")
;

INSERT INTO "Scheduler"."AttributeDefinitionType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Text', 'Single line text', 1, 'd1a1b2c3-1111-2222-3333-444455556661' );

INSERT INTO "Scheduler"."AttributeDefinitionType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Number', 'Numeric value', 2, 'd1a1b2c3-1111-2222-3333-444455556662' );

INSERT INTO "Scheduler"."AttributeDefinitionType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Date', 'Date value (no time)', 3, 'd1a1b2c3-1111-2222-3333-444455556663' );

INSERT INTO "Scheduler"."AttributeDefinitionType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Boolean', 'True/False checkbox', 4, 'd1a1b2c3-1111-2222-3333-444455556664' );

INSERT INTO "Scheduler"."AttributeDefinitionType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Select', 'Dropdown selection', 5, 'd1a1b2c3-1111-2222-3333-444455556665' );


-- Master list of entities that support custom attributes.
CREATE TABLE "Scheduler"."AttributeDefinitionEntity"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the AttributeDefinitionEntity table's name field.
CREATE INDEX "I_AttributeDefinitionEntity_name" ON "Scheduler"."AttributeDefinitionEntity" ("name")
;

-- Index on the AttributeDefinitionEntity table's active field.
CREATE INDEX "I_AttributeDefinitionEntity_active" ON "Scheduler"."AttributeDefinitionEntity" ("active")
;

-- Index on the AttributeDefinitionEntity table's deleted field.
CREATE INDEX "I_AttributeDefinitionEntity_deleted" ON "Scheduler"."AttributeDefinitionEntity" ("deleted")
;

INSERT INTO "Scheduler"."AttributeDefinitionEntity" ( "name", "description", "objectGuid" ) VALUES  ( 'Contact', 'Contact Records', 'e2a1b2c3-1111-2222-3333-444455556661' );

INSERT INTO "Scheduler"."AttributeDefinitionEntity" ( "name", "description", "objectGuid" ) VALUES  ( 'Constituent', 'Constituent Records', 'e2a1b2c3-1111-2222-3333-444455556662' );


-- Definitions for custom attributes on various entities (Contact, Constituent, etc.)
CREATE TABLE "Scheduler"."AttributeDefinition"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"attributeDefinitionEntityId" INT NULL,		-- The entity this attribute applies to (e.g., Contact)
	"key" VARCHAR(100) NULL,		-- The JSON key for the attribute
	"label" VARCHAR(250) NULL,		-- The human-readable label for the attribute
	"attributeDefinitionTypeId" INT NULL,		-- Data type: Text, Number, Date, etc.
	"options" TEXT NULL,		-- JSON options for Select/MultiSelect types
	"isRequired" BOOLEAN NOT NULL DEFAULT false,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "attributeDefinitionEntityId" FOREIGN KEY ("attributeDefinitionEntityId") REFERENCES "Scheduler"."AttributeDefinitionEntity"("id"),		-- Foreign key to the AttributeDefinitionEntity table.
	CONSTRAINT "attributeDefinitionTypeId" FOREIGN KEY ("attributeDefinitionTypeId") REFERENCES "Scheduler"."AttributeDefinitionType"("id"),		-- Foreign key to the AttributeDefinitionType table.
	CONSTRAINT "UC_AttributeDefinition_tenantGuid_attributeDefinitionEntityId_key" UNIQUE ( "tenantGuid", "attributeDefinitionEntityId", "key") 		-- Uniqueness enforced on the AttributeDefinition table's tenantGuid and attributeDefinitionEntityId and key fields.
);
-- Index on the AttributeDefinition table's tenantGuid field.
CREATE INDEX "I_AttributeDefinition_tenantGuid" ON "Scheduler"."AttributeDefinition" ("tenantGuid")
;

-- Index on the AttributeDefinition table's tenantGuid,attributeDefinitionEntityId fields.
CREATE INDEX "I_AttributeDefinition_tenantGuid_attributeDefinitionEntityId" ON "Scheduler"."AttributeDefinition" ("tenantGuid", "attributeDefinitionEntityId")
;

-- Index on the AttributeDefinition table's tenantGuid,attributeDefinitionTypeId fields.
CREATE INDEX "I_AttributeDefinition_tenantGuid_attributeDefinitionTypeId" ON "Scheduler"."AttributeDefinition" ("tenantGuid", "attributeDefinitionTypeId")
;

-- Index on the AttributeDefinition table's tenantGuid,active fields.
CREATE INDEX "I_AttributeDefinition_tenantGuid_active" ON "Scheduler"."AttributeDefinition" ("tenantGuid", "active")
;

-- Index on the AttributeDefinition table's tenantGuid,deleted fields.
CREATE INDEX "I_AttributeDefinition_tenantGuid_deleted" ON "Scheduler"."AttributeDefinition" ("tenantGuid", "deleted")
;


-- The change history for records from the AttributeDefinition table.
CREATE TABLE "Scheduler"."AttributeDefinitionChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"attributeDefinitionId" INT NOT NULL,		-- Link to the AttributeDefinition table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "attributeDefinitionId" FOREIGN KEY ("attributeDefinitionId") REFERENCES "Scheduler"."AttributeDefinition"("id")		-- Foreign key to the AttributeDefinition table.
);
-- Index on the AttributeDefinitionChangeHistory table's tenantGuid field.
CREATE INDEX "I_AttributeDefinitionChangeHistory_tenantGuid" ON "Scheduler"."AttributeDefinitionChangeHistory" ("tenantGuid")
;

-- Index on the AttributeDefinitionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_AttributeDefinitionChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."AttributeDefinitionChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the AttributeDefinitionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_AttributeDefinitionChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."AttributeDefinitionChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the AttributeDefinitionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_AttributeDefinitionChangeHistory_tenantGuid_userId" ON "Scheduler"."AttributeDefinitionChangeHistory" ("tenantGuid", "userId")
;

-- Index on the AttributeDefinitionChangeHistory table's tenantGuid,attributeDefinitionId fields.
CREATE INDEX "I_AttributeDefinitionChangeHistory_tenantGuid_attributeDefiniti" ON "Scheduler"."AttributeDefinitionChangeHistory" ("tenantGuid", "attributeDefinitionId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- List of icons to use on user interfaces.  Not tenant editable.
CREATE TABLE "Scheduler"."Icon"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"fontAwesomeCode" VARCHAR(50) NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the Icon table's name field.
CREATE INDEX "I_Icon_name" ON "Scheduler"."Icon" ("name")
;

-- Index on the Icon table's active field.
CREATE INDEX "I_Icon_active" ON "Scheduler"."Icon" ("active")
;

-- Index on the Icon table's deleted field.
CREATE INDEX "I_Icon_deleted" ON "Scheduler"."Icon" ("deleted")
;

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Person', 'fa-solid fa-user', 1, 'a1b2c3d4-5678-9012-3456-789abcde0001' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'People', 'fa-solid fa-users', 2, 'a1b2c3d4-5678-9012-3456-789abcde0002' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Supervisor', 'fa-solid fa-user-tie', 3, 'a1b2c3d4-5678-9012-3456-789abcde0003' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Operator', 'fa-solid fa-hard-hat', 4, 'a1b2c3d4-5678-9012-3456-789abcde0004' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Equipment', 'fa-solid fa-truck', 10, 'a1b2c3d4-5678-9012-3456-789abcde0010' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Roller', 'fa-solid fa-road', 11, 'a1b2c3d4-5678-9012-3456-789abcde0011' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Crane', 'fa-solid fa-tower-broadcast', 12, 'a1b2c3d4-5678-9012-3456-789abcde0012' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Vehicle', 'fa-solid fa-truck-pickup', 13, 'a1b2c3d4-5678-9012-3456-789abcde0013' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Tool', 'fa-solid fa-toolbox', 14, 'a1b2c3d4-5678-9012-3456-789abcde0014' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Room', 'fa-solid fa-door-open', 15, 'a1b2c3d4-5678-9012-3456-789abcde0015' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Project', 'fa-solid fa-briefcase', 20, 'a1b2c3d4-5678-9012-3456-789abcde0020' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Construction Site', 'fa-solid fa-helmet-safety', 21, 'a1b2c3d4-5678-9012-3456-789abcde0021' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Patient', 'fa-solid fa-bed-pulse', 22, 'a1b2c3d4-5678-9012-3456-789abcde0022' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Home', 'fa-solid fa-house-medical', 23, 'a1b2c3d4-5678-9012-3456-789abcde0023' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Calendar', 'fa-solid fa-calendar-days', 30, 'a1b2c3d4-5678-9012-3456-789abcde0030' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Maintenance', 'fa-solid fa-wrench', 31, 'a1b2c3d4-5678-9012-3456-789abcde0031' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Event', 'fa-solid fa-calendar-check', 32, 'a1b2c3d4-5678-9012-3456-789abcde0032' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'High Priority', 'fa-solid fa-triangle-exclamation', 40, 'a1b2c3d4-5678-9012-3456-789abcde0040' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Medium Priority', 'fa-solid fa-circle-exclamation', 41, 'a1b2c3d4-5678-9012-3456-789abcde0041' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Low Priority', 'fa-solid fa-circle-info', 42, 'a1b2c3d4-5678-9012-3456-789abcde0042' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Assignment', 'fa-solid fa-user-check', 50, 'a1b2c3d4-5678-9012-3456-789abcde0050' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Crew', 'fa-solid fa-users-gear', 51, 'a1b2c3d4-5678-9012-3456-789abcde0051' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Qualification', 'fa-solid fa-certificate', 52, 'a1b2c3d4-5678-9012-3456-789abcde0052' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Travel', 'fa-solid fa-car', 53, 'a1b2c3d4-5678-9012-3456-789abcde0053' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Location', 'fa-solid fa-location-dot', 54, 'a1b2c3d4-5678-9012-3456-789abcde0054' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Notification', 'fa-solid fa-bell', 55, 'a1b2c3d4-5678-9012-3456-789abcde0055' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Hammer', 'fa-solid fa-hammer', 100, 'a1b2c3d4-5678-9012-3456-789abcde0100' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Wrench', 'fa-solid fa-wrench', 101, 'a1b2c3d4-5678-9012-3456-789abcde0101' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Screwdriver', 'fa-solid fa-screwdriver-wrench', 102, 'a1b2c3d4-5678-9012-3456-789abcde0102' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Paint Roller', 'fa-solid fa-paint-roller', 103, 'a1b2c3d4-5678-9012-3456-789abcde0103' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Brush', 'fa-solid fa-brush', 104, 'a1b2c3d4-5678-9012-3456-789abcde0104' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Ruler / Measurements', 'fa-solid fa-ruler-combined', 105, 'a1b2c3d4-5678-9012-3456-789abcde0105' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Drafting / Architecture', 'fa-solid fa-compass-drafting', 106, 'a1b2c3d4-5678-9012-3456-789abcde0106' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Electricity / Power', 'fa-solid fa-bolt', 107, 'a1b2c3d4-5678-9012-3456-789abcde0107' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Water / Plumbing', 'fa-solid fa-faucet-drip', 108, 'a1b2c3d4-5678-9012-3456-789abcde0108' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Trowel / Masonry', 'fa-solid fa-trowel', 109, 'a1b2c3d4-5678-9012-3456-789abcde0109' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Bucket', 'fa-solid fa-bucket', 110, 'a1b2c3d4-5678-9012-3456-789abcde0110' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Doctor', 'fa-solid fa-user-doctor', 200, 'a1b2c3d4-5678-9012-3456-789abcde0200' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Nurse', 'fa-solid fa-user-nurse', 201, 'a1b2c3d4-5678-9012-3456-789abcde0201' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Stethoscope', 'fa-solid fa-stethoscope', 202, 'a1b2c3d4-5678-9012-3456-789abcde0202' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Syringe / Vaccine', 'fa-solid fa-syringe', 203, 'a1b2c3d4-5678-9012-3456-789abcde0203' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'First Aid', 'fa-solid fa-kit-medical', 204, 'a1b2c3d4-5678-9012-3456-789abcde0204' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Pills / Medication', 'fa-solid fa-pills', 205, 'a1b2c3d4-5678-9012-3456-789abcde0205' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Hospital', 'fa-solid fa-hospital', 206, 'a1b2c3d4-5678-9012-3456-789abcde0206' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Wheelchair / Accessibility', 'fa-solid fa-wheelchair', 207, 'a1b2c3d4-5678-9012-3456-789abcde0207' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Heart / Vitals', 'fa-solid fa-heart-pulse', 208, 'a1b2c3d4-5678-9012-3456-789abcde0208' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Box / Package', 'fa-solid fa-box', 300, 'a1b2c3d4-5678-9012-3456-789abcde0300' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Pallet', 'fa-solid fa-pallet', 301, 'a1b2c3d4-5678-9012-3456-789abcde0301' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Warehouse', 'fa-solid fa-warehouse', 302, 'a1b2c3d4-5678-9012-3456-789abcde0302' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Map Pin', 'fa-solid fa-map-pin', 303, 'a1b2c3d4-5678-9012-3456-789abcde0303' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Route', 'fa-solid fa-route', 304, 'a1b2c3d4-5678-9012-3456-789abcde0304' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Ship / Marine', 'fa-solid fa-ship', 305, 'a1b2c3d4-5678-9012-3456-789abcde0305' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Plane / Air', 'fa-solid fa-plane', 306, 'a1b2c3d4-5678-9012-3456-789abcde0306' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Building / Office', 'fa-solid fa-building', 400, 'a1b2c3d4-5678-9012-3456-789abcde0400' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Money / Finance', 'fa-solid fa-money-bill-wave', 401, 'a1b2c3d4-5678-9012-3456-789abcde0401' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Credit Card', 'fa-solid fa-credit-card', 402, 'a1b2c3d4-5678-9012-3456-789abcde0402' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Contract / Document', 'fa-solid fa-file-contract', 403, 'a1b2c3d4-5678-9012-3456-789abcde0403' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Signature', 'fa-solid fa-file-signature', 404, 'a1b2c3d4-5678-9012-3456-789abcde0404' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Clipboard / Checklist', 'fa-solid fa-clipboard-list', 405, 'a1b2c3d4-5678-9012-3456-789abcde0405' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Chart / Analytics', 'fa-solid fa-chart-line', 406, 'a1b2c3d4-5678-9012-3456-789abcde0406' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Phone', 'fa-solid fa-phone', 500, 'a1b2c3d4-5678-9012-3456-789abcde0500' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Laptop', 'fa-solid fa-laptop', 501, 'a1b2c3d4-5678-9012-3456-789abcde0501' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Server / Database', 'fa-solid fa-server', 502, 'a1b2c3d4-5678-9012-3456-789abcde0502' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Wifi', 'fa-solid fa-wifi', 503, 'a1b2c3d4-5678-9012-3456-789abcde0503' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Check / Success', 'fa-solid fa-check', 600, 'a1b2c3d4-5678-9012-3456-789abcde0600' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'X / Cancel', 'fa-solid fa-xmark', 601, 'a1b2c3d4-5678-9012-3456-789abcde0601' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Ban / Blocked', 'fa-solid fa-ban', 602, 'a1b2c3d4-5678-9012-3456-789abcde0602' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Clock / Time', 'fa-solid fa-clock', 603, 'a1b2c3d4-5678-9012-3456-789abcde0603' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Hourglass / Waiting', 'fa-solid fa-hourglass-half', 604, 'a1b2c3d4-5678-9012-3456-789abcde0604' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Lock / Security', 'fa-solid fa-lock', 605, 'a1b2c3d4-5678-9012-3456-789abcde0605' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Trash / Delete', 'fa-solid fa-trash', 606, 'a1b2c3d4-5678-9012-3456-789abcde0606' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Sun / Day', 'fa-solid fa-sun', 700, 'a1b2c3d4-5678-9012-3456-789abcde0700' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Cloud', 'fa-solid fa-cloud', 701, 'a1b2c3d4-5678-9012-3456-789abcde0701' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Tree / Landscape', 'fa-solid fa-tree', 702, 'a1b2c3d4-5678-9012-3456-789abcde0702' );

INSERT INTO "Scheduler"."Icon" ( "name", "fontAwesomeCode", "sequence", "objectGuid" ) VALUES  ( 'Default', 'fa-solid fa-circle', 999, 'a1b2c3d4-5678-9012-3456-789abcde0999' );


-- The master list of salutations
CREATE TABLE "Scheduler"."Salutation"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the Salutation table's name field.
CREATE INDEX "I_Salutation_name" ON "Scheduler"."Salutation" ("name")
;

-- Index on the Salutation table's active field.
CREATE INDEX "I_Salutation_active" ON "Scheduler"."Salutation" ("active")
;

-- Index on the Salutation table's deleted field.
CREATE INDEX "I_Salutation_deleted" ON "Scheduler"."Salutation" ("deleted")
;

INSERT INTO "Scheduler"."Salutation" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Mr.', 'Mister', 1, '0e2c9a70-3a90-49f7-9f0a-539fb232a667' );

INSERT INTO "Scheduler"."Salutation" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Mrs.', 'Mrs.', 2, '738abc0a-c637-4d45-89a1-4efb5da4e1d6' );

INSERT INTO "Scheduler"."Salutation" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Ms.', 'Ms.', 3, 'e4f9cfe6-c9dc-44a4-8977-67a8e90f94f8' );

INSERT INTO "Scheduler"."Salutation" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Dr.', 'Doctor', 4, '67be6b22-591f-4b7c-8366-bc3e7304ec90' );

INSERT INTO "Scheduler"."Salutation" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Prof.', 'Professor', 5, '8334e778-b326-4313-8891-c84cf9067d4f' );

INSERT INTO "Scheduler"."Salutation" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Rev.', 'Reverend', 6, 'f27ca1ef-1d00-4d03-9ccd-79a2f97cb2e6' );

INSERT INTO "Scheduler"."Salutation" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( '', 'No Salutation', 7, 'df674e7a-16d8-4e75-bb2b-2a965e1725f1' );


-- Tenant specific master list of resource categories.
CREATE TABLE "Scheduler"."ResourceType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"isBillable" BOOLEAN NULL DEFAULT false,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_ResourceType_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the ResourceType table's tenantGuid and name fields.
);
-- Index on the ResourceType table's tenantGuid field.
CREATE INDEX "I_ResourceType_tenantGuid" ON "Scheduler"."ResourceType" ("tenantGuid")
;

-- Index on the ResourceType table's tenantGuid,name fields.
CREATE INDEX "I_ResourceType_tenantGuid_name" ON "Scheduler"."ResourceType" ("tenantGuid", "name")
;

-- Index on the ResourceType table's tenantGuid,iconId fields.
CREATE INDEX "I_ResourceType_tenantGuid_iconId" ON "Scheduler"."ResourceType" ("tenantGuid", "iconId")
;

-- Index on the ResourceType table's tenantGuid,active fields.
CREATE INDEX "I_ResourceType_tenantGuid_active" ON "Scheduler"."ResourceType" ("tenantGuid", "active")
;

-- Index on the ResourceType table's tenantGuid,deleted fields.
CREATE INDEX "I_ResourceType_tenantGuid_deleted" ON "Scheduler"."ResourceType" ("tenantGuid", "deleted")
;

INSERT INTO "Scheduler"."ResourceType" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Person', 'Human resource (operator, supervisor, engineer, etc.)', 1, 'a1b2c3d4-5678-9012-3456-789abcde0001' );

INSERT INTO "Scheduler"."ResourceType" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Equipment', 'Heavy machinery (rollers, excavators, loaders, etc.)', 2, 'a1b2c3d4-5678-9012-3456-789abcde0002' );

INSERT INTO "Scheduler"."ResourceType" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Vehicle', 'Trucks, service vehicles, etc.', 3, 'a1b2c3d4-5678-9012-3456-789abcde0003' );

INSERT INTO "Scheduler"."ResourceType" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Tool', 'Smaller tools or shared items', 4, 'a1b2c3d4-5678-9012-3456-789abcde0004' );

INSERT INTO "Scheduler"."ResourceType" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Room', 'Meeting rooms, office spaces, etc.', 5, 'a1b2c3d4-5678-9012-3456-789abcde0005' );


-- List of priority values - Tenant configurable for flexibilty
CREATE TABLE "Scheduler"."Priority"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"iconId" INT NULL,		-- Link to the Icon table.
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_Priority_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Priority table's tenantGuid and name fields.
);
-- Index on the Priority table's tenantGuid field.
CREATE INDEX "I_Priority_tenantGuid" ON "Scheduler"."Priority" ("tenantGuid")
;

-- Index on the Priority table's tenantGuid,name fields.
CREATE INDEX "I_Priority_tenantGuid_name" ON "Scheduler"."Priority" ("tenantGuid", "name")
;

-- Index on the Priority table's tenantGuid,iconId fields.
CREATE INDEX "I_Priority_tenantGuid_iconId" ON "Scheduler"."Priority" ("tenantGuid", "iconId")
;

-- Index on the Priority table's tenantGuid,active fields.
CREATE INDEX "I_Priority_tenantGuid_active" ON "Scheduler"."Priority" ("tenantGuid", "active")
;

-- Index on the Priority table's tenantGuid,deleted fields.
CREATE INDEX "I_Priority_tenantGuid_deleted" ON "Scheduler"."Priority" ("tenantGuid", "deleted")
;

INSERT INTO "Scheduler"."Priority" ( "tenantGuid", "name", "description", "color", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'High', 'High Priority', '#FF0F0F', 1, 'bcde74de-3f66-4c62-ad38-a5941871cea2' );

INSERT INTO "Scheduler"."Priority" ( "tenantGuid", "name", "description", "color", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Medium', 'Medium Priority', '#E8E8E8', 2, 'f2058cd4-aecf-4e28-b40c-6c181e67c0f4' );

INSERT INTO "Scheduler"."Priority" ( "tenantGuid", "name", "description", "color", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Low', 'Low Priority', '#E8E8E8', 3, '25e075c3-a513-4a45-9fbc-106afc890821' );


-- List of standard contact methods
CREATE TABLE "Scheduler"."ContactMethod"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"iconId" INT NULL,		-- Link to the Icon table.
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id")		-- Foreign key to the Icon table.
);
-- Index on the ContactMethod table's name field.
CREATE INDEX "I_ContactMethod_name" ON "Scheduler"."ContactMethod" ("name")
;

-- Index on the ContactMethod table's iconId field.
CREATE INDEX "I_ContactMethod_iconId" ON "Scheduler"."ContactMethod" ("iconId")
;

-- Index on the ContactMethod table's active field.
CREATE INDEX "I_ContactMethod_active" ON "Scheduler"."ContactMethod" ("active")
;

-- Index on the ContactMethod table's deleted field.
CREATE INDEX "I_ContactMethod_deleted" ON "Scheduler"."ContactMethod" ("deleted")
;

INSERT INTO "Scheduler"."ContactMethod" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Mobile Phone', 'Mobile Phone', 1, 'c8e56688-e480-426d-b49d-f7f7e7c1802c' );

INSERT INTO "Scheduler"."ContactMethod" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Phone', 'Phone', 2, 'df379702-6082-4084-bf4e-f722893f33a2' );

INSERT INTO "Scheduler"."ContactMethod" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Email', 'Email', 3, '1fbea244-8312-4d8c-8218-b4b5d0788510' );

INSERT INTO "Scheduler"."ContactMethod" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Text', 'Text', 4, '9ad23e9b-76fe-4e35-9c9b-8a53b9037cce' );

INSERT INTO "Scheduler"."ContactMethod" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Video Call', 'Video Call', 5, 'f89b6825-fd15-419f-baef-ec6c9ae61127' );

INSERT INTO "Scheduler"."ContactMethod" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'In Person', 'In Person', 6, '91c03a84-0772-443b-8eba-e6810ec4912a' );


-- The rate types
CREATE TABLE "Scheduler"."RateType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_RateType_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the RateType table's tenantGuid and name fields.
);
-- Index on the RateType table's tenantGuid field.
CREATE INDEX "I_RateType_tenantGuid" ON "Scheduler"."RateType" ("tenantGuid")
;

-- Index on the RateType table's tenantGuid,name fields.
CREATE INDEX "I_RateType_tenantGuid_name" ON "Scheduler"."RateType" ("tenantGuid", "name")
;

-- Index on the RateType table's tenantGuid,active fields.
CREATE INDEX "I_RateType_tenantGuid_active" ON "Scheduler"."RateType" ("tenantGuid", "active")
;

-- Index on the RateType table's tenantGuid,deleted fields.
CREATE INDEX "I_RateType_tenantGuid_deleted" ON "Scheduler"."RateType" ("tenantGuid", "deleted")
;

INSERT INTO "Scheduler"."RateType" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Standard', 'Standard Billing Rate', 1, 'e0d3b9b8-2b93-45e1-8de2-dba9603c38b9' );

INSERT INTO "Scheduler"."RateType" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Overtime', 'Overtime Billing Rate', 2, '84897121-1587-4930-9d8c-4389ac0d222f' );

INSERT INTO "Scheduler"."RateType" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'DoubleTime', 'DoubleTime Billing Rate', 3, 'fad24a49-924d-403f-a013-114ceb13ae27' );

INSERT INTO "Scheduler"."RateType" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Travel', 'Travel Billing Rate', 4, 'fa0f7edd-8443-419d-9aea-229a2e61730f' );


-- Master list of interaction types.
CREATE TABLE "Scheduler"."InteractionType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id")		-- Foreign key to the Icon table.
);
-- Index on the InteractionType table's name field.
CREATE INDEX "I_InteractionType_name" ON "Scheduler"."InteractionType" ("name")
;

-- Index on the InteractionType table's iconId field.
CREATE INDEX "I_InteractionType_iconId" ON "Scheduler"."InteractionType" ("iconId")
;

-- Index on the InteractionType table's active field.
CREATE INDEX "I_InteractionType_active" ON "Scheduler"."InteractionType" ("active")
;

-- Index on the InteractionType table's deleted field.
CREATE INDEX "I_InteractionType_deleted" ON "Scheduler"."InteractionType" ("deleted")
;

INSERT INTO "Scheduler"."InteractionType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'In Person', 'In Person meeting', 1, '4a503ab2-a58e-403a-a400-027985773cb6' );

INSERT INTO "Scheduler"."InteractionType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Phone Call', 'Phone Call', 2, '16988bb1-54d3-4bb9-b6a7-bfadface573d' );

INSERT INTO "Scheduler"."InteractionType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Video Call', 'Video Call', 3, '337a67d5-53b8-4a67-ac4b-97818d0b0fa4' );

INSERT INTO "Scheduler"."InteractionType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Text Message', 'Text Message', 4, '10ea655e-07ae-46cf-bbf3-076c3643e16b' );

INSERT INTO "Scheduler"."InteractionType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Email Message', 'Email Message', 5, 'eeb14f23-857e-416e-80a0-9a2f82b57bf7' );


-- The currencies
CREATE TABLE "Scheduler"."Currency"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"code" VARCHAR(10) NOT NULL,
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"isDefault" BOOLEAN NOT NULL DEFAULT false,		-- Default currency for tenant.
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_Currency_tenantGuid_name" UNIQUE ( "tenantGuid", "name") ,		-- Uniqueness enforced on the Currency table's tenantGuid and name fields.
	CONSTRAINT "UC_Currency_tenantGuid_code" UNIQUE ( "tenantGuid", "code") 		-- Uniqueness enforced on the Currency table's tenantGuid and code fields.
);
-- Index on the Currency table's tenantGuid field.
CREATE INDEX "I_Currency_tenantGuid" ON "Scheduler"."Currency" ("tenantGuid")
;

-- Index on the Currency table's tenantGuid,name fields.
CREATE INDEX "I_Currency_tenantGuid_name" ON "Scheduler"."Currency" ("tenantGuid", "name")
;

-- Index on the Currency table's tenantGuid,active fields.
CREATE INDEX "I_Currency_tenantGuid_active" ON "Scheduler"."Currency" ("tenantGuid", "active")
;

-- Index on the Currency table's tenantGuid,deleted fields.
CREATE INDEX "I_Currency_tenantGuid_deleted" ON "Scheduler"."Currency" ("tenantGuid", "deleted")
;

INSERT INTO "Scheduler"."Currency" ( "tenantGuid", "name", "description", "code", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'US Dollar', 'United States Dollars', 'USD', 1, '5d460ce9-4cf5-41c3-ab9d-9ef104b0a276' );

INSERT INTO "Scheduler"."Currency" ( "tenantGuid", "name", "description", "code", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Canadian Dollar', 'Canadian Dollars', 'CAD', 2, 'c6673662-f1c9-4aee-b5df-867500cb8545' );


/*
====================================================================================================
 ACCOUNT TYPE
 Standard accounting classifications (Income, Expense, COGS, Asset, Liability, Equity).
 System-defined reference data — not tenant-specific.

 DESIGN NOTE: The isRevenue flag provides a single source of truth for revenue classification,
 eliminating the need for programmatic derivation from string values. The externalMapping field
 maps to external system account types (e.g., QuickBooks).
 =====================================================================================================
*/
CREATE TABLE "Scheduler"."AccountType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"isRevenue" BOOLEAN NOT NULL DEFAULT false,		-- True for revenue account types (Income), false for all others (Expense, COGS, Asset, Liability, Equity).
	"externalMapping" VARCHAR(100) NULL,		-- Maps to the account type in external systems (e.g., QuickBooks account type name).
	"color" VARCHAR(10) NULL,		-- Hex color for UI display.
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the AccountType table's name field.
CREATE INDEX "I_AccountType_name" ON "Scheduler"."AccountType" ("name")
;

-- Index on the AccountType table's active field.
CREATE INDEX "I_AccountType_active" ON "Scheduler"."AccountType" ("active")
;

-- Index on the AccountType table's deleted field.
CREATE INDEX "I_AccountType_deleted" ON "Scheduler"."AccountType" ("deleted")
;

INSERT INTO "Scheduler"."AccountType" ( "name", "description", "isRevenue", "externalMapping", "color", "sequence", "objectGuid" ) VALUES  ( 'Income', 'Revenue from operations, sales, services, grants, etc.', true, 'Income', '#4CAF50', 1, 'a1b2c3d4-0001-4000-8000-000000000001' );

INSERT INTO "Scheduler"."AccountType" ( "name", "description", "isRevenue", "externalMapping", "color", "sequence", "objectGuid" ) VALUES  ( 'Expense', 'Operating expenses, overhead, supplies, labour, etc.', false, 'Expense', '#F44336', 2, 'a1b2c3d4-0001-4000-8000-000000000002' );

INSERT INTO "Scheduler"."AccountType" ( "name", "description", "isRevenue", "externalMapping", "color", "sequence", "objectGuid" ) VALUES  ( 'COGS', 'Cost of Goods Sold — direct costs attributable to goods/services sold.', false, 'Cost of Goods Sold', '#FF9800', 3, 'a1b2c3d4-0001-4000-8000-000000000003' );

INSERT INTO "Scheduler"."AccountType" ( "name", "description", "isRevenue", "externalMapping", "color", "sequence", "objectGuid" ) VALUES  ( 'Asset', 'Resources owned — cash, equipment, accounts receivable, etc.', false, 'Other Current Asset', '#2196F3', 4, 'a1b2c3d4-0001-4000-8000-000000000004' );

INSERT INTO "Scheduler"."AccountType" ( "name", "description", "isRevenue", "externalMapping", "color", "sequence", "objectGuid" ) VALUES  ( 'Liability', 'Obligations owed — accounts payable, loans, deferred revenue, etc.', false, 'Other Current Liability', '#9C27B0', 5, 'a1b2c3d4-0001-4000-8000-000000000005' );

INSERT INTO "Scheduler"."AccountType" ( "name", "description", "isRevenue", "externalMapping", "color", "sequence", "objectGuid" ) VALUES  ( 'Equity', 'Owner''s equity, retained earnings, net assets.', false, 'Equity', '#607D8B', 6, 'a1b2c3d4-0001-4000-8000-000000000006' );


/*
====================================================================================================
 FINANCIAL OFFICE
 Represents a departmental or committee-level financial partition within a tenant.
 Enables separate books, independent exports, and role-based access for different
 organizational units (e.g., Recreation Committee vs Town Administration).

 DESIGN NOTE: Each office can have its own chart of accounts slice, transactions,
 budgets, and export configuration. A FinancialCategory or FinancialTransaction
 optionally belongs to an office. When null, it is considered tenant-wide.

 Export fields (contactName, contactEmail, exportFormat) support the workflow where
 each office exports independently to its own accountant.
 =====================================================================================================
*/
CREATE TABLE "Scheduler"."FinancialOffice"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"code" VARCHAR(50) NOT NULL,		-- Short code for the office (e.g., 'REC', 'ADMIN', 'FIRE').
	"contactName" VARCHAR(250) NULL,		-- Accountant or financial contact name for this office.
	"contactEmail" VARCHAR(250) NULL,		-- Accountant or financial contact email for export delivery.
	"exportFormat" VARCHAR(50) NULL DEFAULT 'CSV',		-- Preferred export format: CSV, QuickBooks, Xero, etc.
	"color" VARCHAR(10) NULL,		-- Hex color for UI display.
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_FinancialOffice_tenantGuid_name" UNIQUE ( "tenantGuid", "name") ,		-- Uniqueness enforced on the FinancialOffice table's tenantGuid and name fields.
	CONSTRAINT "UC_FinancialOffice_tenantGuid_code" UNIQUE ( "tenantGuid", "code") 		-- Uniqueness enforced on the FinancialOffice table's tenantGuid and code fields.
);
-- Index on the FinancialOffice table's tenantGuid field.
CREATE INDEX "I_FinancialOffice_tenantGuid" ON "Scheduler"."FinancialOffice" ("tenantGuid")
;

-- Index on the FinancialOffice table's tenantGuid,name fields.
CREATE INDEX "I_FinancialOffice_tenantGuid_name" ON "Scheduler"."FinancialOffice" ("tenantGuid", "name")
;

-- Index on the FinancialOffice table's tenantGuid,active fields.
CREATE INDEX "I_FinancialOffice_tenantGuid_active" ON "Scheduler"."FinancialOffice" ("tenantGuid", "active")
;

-- Index on the FinancialOffice table's tenantGuid,deleted fields.
CREATE INDEX "I_FinancialOffice_tenantGuid_deleted" ON "Scheduler"."FinancialOffice" ("tenantGuid", "deleted")
;


-- The change history for records from the FinancialOffice table.
CREATE TABLE "Scheduler"."FinancialOfficeChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"financialOfficeId" INT NOT NULL,		-- Link to the FinancialOffice table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "financialOfficeId" FOREIGN KEY ("financialOfficeId") REFERENCES "Scheduler"."FinancialOffice"("id")		-- Foreign key to the FinancialOffice table.
);
-- Index on the FinancialOfficeChangeHistory table's tenantGuid field.
CREATE INDEX "I_FinancialOfficeChangeHistory_tenantGuid" ON "Scheduler"."FinancialOfficeChangeHistory" ("tenantGuid")
;

-- Index on the FinancialOfficeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_FinancialOfficeChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."FinancialOfficeChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the FinancialOfficeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_FinancialOfficeChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."FinancialOfficeChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the FinancialOfficeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_FinancialOfficeChangeHistory_tenantGuid_userId" ON "Scheduler"."FinancialOfficeChangeHistory" ("tenantGuid", "userId")
;

-- Index on the FinancialOfficeChangeHistory table's tenantGuid,financialOfficeId fields.
CREATE INDEX "I_FinancialOfficeChangeHistory_tenantGuid_financialOfficeId" ON "Scheduler"."FinancialOfficeChangeHistory" ("tenantGuid", "financialOfficeId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
 FINANCIAL CATEGORY (Chart of Accounts)
 Tenant-specific chart of accounts for categorizing all income and expense transactions.
 Unlike ChargeType (which is specifically for event-linked charges), FinancialCategory represents
 general ledger items: cleaning labour, supplies, bank fees, grants, bar sales, ticket sales, etc.

 DESIGN NOTE: Supports optional hierarchy via self-referencing parentFinancialCategoryId for
 sub-categories (e.g., Bar Sales > Tips, Bar Sales > Liquor).

 accountTypeId links to AccountType for standard classification (Income, Expense, COGS, etc.)
 and derives isRevenue from the AccountType.isRevenue flag.
 =====================================================================================================
*/
CREATE TABLE "Scheduler"."FinancialCategory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"code" VARCHAR(50) NOT NULL,		-- Short code for the category (e.g., '12' for Kids Rental, '40' for Easter Brunch Supplies).
	"accountTypeId" INT NOT NULL,		-- Link to AccountType — standard accounting classification (Income, Expense, COGS, Asset, Liability, Equity). Replaces the old accountType string field.
	"financialOfficeId" INT NULL,		-- Optional link to FinancialOffice — scopes this category to a specific department/committee. When null, the category is tenant-wide.
	"parentFinancialCategoryId" INT NULL,		-- Optional parent for sub-categories.
	"isTaxApplicable" BOOLEAN NOT NULL DEFAULT false,		-- Whether HST/tax typically applies to transactions in this category.
	"defaultAmount" DECIMAL(11,2) NULL,		-- Optional default amount for common transactions in this category.
	"externalAccountId" VARCHAR(250) NULL,		-- Account ID in external system (e.g., QuickBooks account ID) for sync.
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"color" VARCHAR(10) NULL,		-- Hex color for UI display.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "accountTypeId" FOREIGN KEY ("accountTypeId") REFERENCES "Scheduler"."AccountType"("id"),		-- Foreign key to the AccountType table.
	CONSTRAINT "financialOfficeId" FOREIGN KEY ("financialOfficeId") REFERENCES "Scheduler"."FinancialOffice"("id"),		-- Foreign key to the FinancialOffice table.
	CONSTRAINT "parentFinancialCategoryId" FOREIGN KEY ("parentFinancialCategoryId") REFERENCES "Scheduler"."FinancialCategory"("id"),		-- Foreign key to the FinancialCategory table.
	CONSTRAINT "UC_FinancialCategory_tenantGuid_name" UNIQUE ( "tenantGuid", "name") ,		-- Uniqueness enforced on the FinancialCategory table's tenantGuid and name fields.
	CONSTRAINT "UC_FinancialCategory_tenantGuid_code" UNIQUE ( "tenantGuid", "code") 		-- Uniqueness enforced on the FinancialCategory table's tenantGuid and code fields.
);
-- Index on the FinancialCategory table's tenantGuid field.
CREATE INDEX "I_FinancialCategory_tenantGuid" ON "Scheduler"."FinancialCategory" ("tenantGuid")
;

-- Index on the FinancialCategory table's tenantGuid,name fields.
CREATE INDEX "I_FinancialCategory_tenantGuid_name" ON "Scheduler"."FinancialCategory" ("tenantGuid", "name")
;

-- Index on the FinancialCategory table's tenantGuid,accountTypeId fields.
CREATE INDEX "I_FinancialCategory_tenantGuid_accountTypeId" ON "Scheduler"."FinancialCategory" ("tenantGuid", "accountTypeId")
;

-- Index on the FinancialCategory table's tenantGuid,financialOfficeId fields.
CREATE INDEX "I_FinancialCategory_tenantGuid_financialOfficeId" ON "Scheduler"."FinancialCategory" ("tenantGuid", "financialOfficeId")
;

-- Index on the FinancialCategory table's tenantGuid,parentFinancialCategoryId fields.
CREATE INDEX "I_FinancialCategory_tenantGuid_parentFinancialCategoryId" ON "Scheduler"."FinancialCategory" ("tenantGuid", "parentFinancialCategoryId")
;

-- Index on the FinancialCategory table's tenantGuid,externalAccountId fields.
CREATE INDEX "I_FinancialCategory_tenantGuid_externalAccountId" ON "Scheduler"."FinancialCategory" ("tenantGuid", "externalAccountId")
;

-- Index on the FinancialCategory table's tenantGuid,active fields.
CREATE INDEX "I_FinancialCategory_tenantGuid_active" ON "Scheduler"."FinancialCategory" ("tenantGuid", "active")
;

-- Index on the FinancialCategory table's tenantGuid,deleted fields.
CREATE INDEX "I_FinancialCategory_tenantGuid_deleted" ON "Scheduler"."FinancialCategory" ("tenantGuid", "deleted")
;


-- The change history for records from the FinancialCategory table.
CREATE TABLE "Scheduler"."FinancialCategoryChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"financialCategoryId" INT NOT NULL,		-- Link to the FinancialCategory table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "financialCategoryId" FOREIGN KEY ("financialCategoryId") REFERENCES "Scheduler"."FinancialCategory"("id")		-- Foreign key to the FinancialCategory table.
);
-- Index on the FinancialCategoryChangeHistory table's tenantGuid field.
CREATE INDEX "I_FinancialCategoryChangeHistory_tenantGuid" ON "Scheduler"."FinancialCategoryChangeHistory" ("tenantGuid")
;

-- Index on the FinancialCategoryChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_FinancialCategoryChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."FinancialCategoryChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the FinancialCategoryChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_FinancialCategoryChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."FinancialCategoryChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the FinancialCategoryChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_FinancialCategoryChangeHistory_tenantGuid_userId" ON "Scheduler"."FinancialCategoryChangeHistory" ("tenantGuid", "userId")
;

-- Index on the FinancialCategoryChangeHistory table's tenantGuid,financialCategoryId fields.
CREATE INDEX "I_FinancialCategoryChangeHistory_tenantGuid_financialCategoryId" ON "Scheduler"."FinancialCategoryChangeHistory" ("tenantGuid", "financialCategoryId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
 TAX CODE
 Defines specific tax codes with their rates (e.g., 'HST-NL' at 15%, 'GST' at 5%, 'Exempt').
 This replaces the simple isTaxApplicable boolean on FinancialCategory with structured tax handling.

 DESIGN NOTE: Supports external system mapping via externalTaxCodeId for QuickBooks, Xero, etc.
 A tax code can have a zero rate (e.g., 'Exempt' or 'Zero-Rated').
 =====================================================================================================
*/
CREATE TABLE "Scheduler"."TaxCode"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"code" VARCHAR(50) NOT NULL,		-- Short tax code identifier (e.g., 'HST', 'GST', 'EXEMPT').
	"rate" NUMERIC(38,22) NOT NULL DEFAULT 0,		-- Tax rate as a percentage (e.g., 15.0 for 15% HST).
	"isDefault" BOOLEAN NOT NULL DEFAULT false,		-- Whether this is the default tax code for new transactions.
	"isExempt" BOOLEAN NOT NULL DEFAULT false,		-- True for tax-exempt codes (rate should be 0).
	"externalTaxCodeId" VARCHAR(250) NULL,		-- Tax code ID in external system (e.g., QuickBooks TaxCode ID).
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_TaxCode_tenantGuid_name" UNIQUE ( "tenantGuid", "name") ,		-- Uniqueness enforced on the TaxCode table's tenantGuid and name fields.
	CONSTRAINT "UC_TaxCode_tenantGuid_code" UNIQUE ( "tenantGuid", "code") 		-- Uniqueness enforced on the TaxCode table's tenantGuid and code fields.
);
-- Index on the TaxCode table's tenantGuid field.
CREATE INDEX "I_TaxCode_tenantGuid" ON "Scheduler"."TaxCode" ("tenantGuid")
;

-- Index on the TaxCode table's tenantGuid,name fields.
CREATE INDEX "I_TaxCode_tenantGuid_name" ON "Scheduler"."TaxCode" ("tenantGuid", "name")
;

-- Index on the TaxCode table's tenantGuid,externalTaxCodeId fields.
CREATE INDEX "I_TaxCode_tenantGuid_externalTaxCodeId" ON "Scheduler"."TaxCode" ("tenantGuid", "externalTaxCodeId")
;

-- Index on the TaxCode table's tenantGuid,active fields.
CREATE INDEX "I_TaxCode_tenantGuid_active" ON "Scheduler"."TaxCode" ("tenantGuid", "active")
;

-- Index on the TaxCode table's tenantGuid,deleted fields.
CREATE INDEX "I_TaxCode_tenantGuid_deleted" ON "Scheduler"."TaxCode" ("tenantGuid", "deleted")
;


/*
====================================================================================================
 CHARGE MASTER (Like Epic CDM)
 Master list of chargeable items (revenue or expenses). e.g., "Site Visit Fee" (revenue), "Travel Expense" (expense).
 Tied to RateType for billing context.
 =====================================================================================================
*/
CREATE TABLE "Scheduler"."ChargeType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"externalId" VARCHAR(100) NULL,
	"isRevenue" BOOLEAN NOT NULL DEFAULT true,		-- True = Revenue (billable), False = Expense (cost)
	"isTaxable" BOOLEAN NULL DEFAULT false,
	"defaultAmount" DECIMAL(11,2) NULL,		-- Optional default value for auto-drops
	"defaultDescription" VARCHAR(500) NULL,		-- sometimes auto-dropped charges need a note (e.g., "Travel to site – 45 km").
	"rateTypeId" INT NULL,		-- Link to RateType (e.g., 'Standard', 'Overtime')
	"currencyId" INT NOT NULL,		-- Link to the Currency table.
	"financialCategoryId" INT NULL,		-- Optional bridge to the general ledger. Maps this charge type to a FinancialCategory for unified Chart of Accounts reporting across both event charges and standalone transactions.
	"taxCodeId" INT NULL,		-- Optional default TaxCode for charges of this type. When set, new EventCharges auto-inherit this tax rate.
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "rateTypeId" FOREIGN KEY ("rateTypeId") REFERENCES "Scheduler"."RateType"("id"),		-- Foreign key to the RateType table.
	CONSTRAINT "currencyId" FOREIGN KEY ("currencyId") REFERENCES "Scheduler"."Currency"("id"),		-- Foreign key to the Currency table.
	CONSTRAINT "financialCategoryId" FOREIGN KEY ("financialCategoryId") REFERENCES "Scheduler"."FinancialCategory"("id"),		-- Foreign key to the FinancialCategory table.
	CONSTRAINT "taxCodeId" FOREIGN KEY ("taxCodeId") REFERENCES "Scheduler"."TaxCode"("id"),		-- Foreign key to the TaxCode table.
	CONSTRAINT "UC_ChargeType_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the ChargeType table's tenantGuid and name fields.
);
-- Index on the ChargeType table's tenantGuid field.
CREATE INDEX "I_ChargeType_tenantGuid" ON "Scheduler"."ChargeType" ("tenantGuid")
;

-- Index on the ChargeType table's tenantGuid,name fields.
CREATE INDEX "I_ChargeType_tenantGuid_name" ON "Scheduler"."ChargeType" ("tenantGuid", "name")
;

-- Index on the ChargeType table's tenantGuid,externalId fields.
CREATE INDEX "I_ChargeType_tenantGuid_externalId" ON "Scheduler"."ChargeType" ("tenantGuid", "externalId")
;

-- Index on the ChargeType table's tenantGuid,rateTypeId fields.
CREATE INDEX "I_ChargeType_tenantGuid_rateTypeId" ON "Scheduler"."ChargeType" ("tenantGuid", "rateTypeId")
;

-- Index on the ChargeType table's tenantGuid,currencyId fields.
CREATE INDEX "I_ChargeType_tenantGuid_currencyId" ON "Scheduler"."ChargeType" ("tenantGuid", "currencyId")
;

-- Index on the ChargeType table's tenantGuid,financialCategoryId fields.
CREATE INDEX "I_ChargeType_tenantGuid_financialCategoryId" ON "Scheduler"."ChargeType" ("tenantGuid", "financialCategoryId")
;

-- Index on the ChargeType table's tenantGuid,taxCodeId fields.
CREATE INDEX "I_ChargeType_tenantGuid_taxCodeId" ON "Scheduler"."ChargeType" ("tenantGuid", "taxCodeId")
;

-- Index on the ChargeType table's tenantGuid,active fields.
CREATE INDEX "I_ChargeType_tenantGuid_active" ON "Scheduler"."ChargeType" ("tenantGuid", "active")
;

-- Index on the ChargeType table's tenantGuid,deleted fields.
CREATE INDEX "I_ChargeType_tenantGuid_deleted" ON "Scheduler"."ChargeType" ("tenantGuid", "deleted")
;


-- The change history for records from the ChargeType table.
CREATE TABLE "Scheduler"."ChargeTypeChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"chargeTypeId" INT NOT NULL,		-- Link to the ChargeType table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "chargeTypeId" FOREIGN KEY ("chargeTypeId") REFERENCES "Scheduler"."ChargeType"("id")		-- Foreign key to the ChargeType table.
);
-- Index on the ChargeTypeChangeHistory table's tenantGuid field.
CREATE INDEX "I_ChargeTypeChangeHistory_tenantGuid" ON "Scheduler"."ChargeTypeChangeHistory" ("tenantGuid")
;

-- Index on the ChargeTypeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ChargeTypeChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ChargeTypeChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ChargeTypeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ChargeTypeChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ChargeTypeChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ChargeTypeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ChargeTypeChangeHistory_tenantGuid_userId" ON "Scheduler"."ChargeTypeChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ChargeTypeChangeHistory table's tenantGuid,chargeTypeId fields.
CREATE INDEX "I_ChargeTypeChangeHistory_tenantGuid_chargeTypeId" ON "Scheduler"."ChargeTypeChangeHistory" ("tenantGuid", "chargeTypeId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- List of tags
CREATE TABLE "Scheduler"."Tag"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"isSystem" BOOLEAN NULL,		-- To mark as system tag for protected / special handling.  For things like 'deceased'.
	"priorityId" INT NULL,		-- Link to the Priority table.
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "priorityId" FOREIGN KEY ("priorityId") REFERENCES "Scheduler"."Priority"("id"),		-- Foreign key to the Priority table.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_Tag_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Tag table's tenantGuid and name fields.
);
-- Index on the Tag table's tenantGuid field.
CREATE INDEX "I_Tag_tenantGuid" ON "Scheduler"."Tag" ("tenantGuid")
;

-- Index on the Tag table's tenantGuid,name fields.
CREATE INDEX "I_Tag_tenantGuid_name" ON "Scheduler"."Tag" ("tenantGuid", "name")
;

-- Index on the Tag table's tenantGuid,priorityId fields.
CREATE INDEX "I_Tag_tenantGuid_priorityId" ON "Scheduler"."Tag" ("tenantGuid", "priorityId")
;

-- Index on the Tag table's tenantGuid,iconId fields.
CREATE INDEX "I_Tag_tenantGuid_iconId" ON "Scheduler"."Tag" ("tenantGuid", "iconId")
;

-- Index on the Tag table's tenantGuid,active fields.
CREATE INDEX "I_Tag_tenantGuid_active" ON "Scheduler"."Tag" ("tenantGuid", "active")
;

-- Index on the Tag table's tenantGuid,deleted fields.
CREATE INDEX "I_Tag_tenantGuid_deleted" ON "Scheduler"."Tag" ("tenantGuid", "deleted")
;

INSERT INTO "Scheduler"."Tag" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Person', 'Human resource (operator, supervisor, engineer, etc.)', 1, 'a1b2c3d4-5678-9012-3456-789abcde0001' );


-- Time zones master data list.
CREATE TABLE "Scheduler"."TimeZone"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"ianaTimeZone" VARCHAR(50) NOT NULL,		-- e.g., 'America/St.John's' (official IANA name)
	"abbreviation" VARCHAR(50) NOT NULL,
	"abbreviationDaylightSavings" VARCHAR(50) NOT NULL,
	"supportsDaylightSavings" BOOLEAN NOT NULL DEFAULT true,
	"standardUTCOffsetHours" REAL NOT NULL,		-- The standard offset hours from UTC for this time zone.
	"dstUTCOffsetHours" REAL NOT NULL,		-- Use the same value here as the standard one for time zones that do not support DST
	"sequence" INT NULL,		-- For sorting in drop downs
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the TimeZone table's name field.
CREATE INDEX "I_TimeZone_name" ON "Scheduler"."TimeZone" ("name")
;

-- Index on the TimeZone table's active field.
CREATE INDEX "I_TimeZone_active" ON "Scheduler"."TimeZone" ("active")
;

-- Index on the TimeZone table's deleted field.
CREATE INDEX "I_TimeZone_deleted" ON "Scheduler"."TimeZone" ("deleted")
;

INSERT INTO "Scheduler"."TimeZone" ( "name", "abbreviation", "abbreviationDaylightSavings", "supportsDaylightSavings", "standardUTCOffsetHours", "dstUTCOffsetHours", "description", "ianaTimeZone", "sequence", "objectGuid" ) VALUES  ( 'Newfoundland Standard Time', 'NST', 'NDT', true, -3.5, -2.5, 'Newfoundland and southeastern Labrador (Canada)', 'America/St_Johns', 10, '27129170-81b3-4c70-a7d4-0378dce8426f' );

INSERT INTO "Scheduler"."TimeZone" ( "name", "abbreviation", "abbreviationDaylightSavings", "supportsDaylightSavings", "standardUTCOffsetHours", "dstUTCOffsetHours", "description", "ianaTimeZone", "sequence", "objectGuid" ) VALUES  ( 'Atlantic Standard Time', 'AST', 'ADT', true, -4, -3, 'Atlantic Canada (Nova Scotia, New Brunswick, PEI, parts of Quebec)', 'America/Halifax', 20, '8f3d2a1b-4c5e-4d8f-9a2b-6e7f1c3d9a0b' );

INSERT INTO "Scheduler"."TimeZone" ( "name", "abbreviation", "abbreviationDaylightSavings", "supportsDaylightSavings", "standardUTCOffsetHours", "dstUTCOffsetHours", "description", "ianaTimeZone", "sequence", "objectGuid" ) VALUES  ( 'Atlantic Standard Time (no DST)', 'AST', 'AST', false, -4, -4, 'Puerto Rico, US Virgin Islands, Dominican Republic', 'America/Puerto_Rico', 30, '648d1e27-51b2-4e9b-ae9e-06dd856022e8' );

INSERT INTO "Scheduler"."TimeZone" ( "name", "abbreviation", "abbreviationDaylightSavings", "supportsDaylightSavings", "standardUTCOffsetHours", "dstUTCOffsetHours", "description", "ianaTimeZone", "sequence", "objectGuid" ) VALUES  ( 'Eastern Standard Time', 'EST', 'EDT', true, -5, -4, 'Eastern United States, Eastern Canada (Ontario, Quebec)', 'America/New_York', 40, 'c4e5f6a7-8b9c-4d0e-1f2a-3b4c5d6e7f8a' );

INSERT INTO "Scheduler"."TimeZone" ( "name", "abbreviation", "abbreviationDaylightSavings", "supportsDaylightSavings", "standardUTCOffsetHours", "dstUTCOffsetHours", "description", "ianaTimeZone", "sequence", "objectGuid" ) VALUES  ( 'Central Standard Time', 'CST', 'CDT', true, -6, -5, 'Central United States, Central Canada, Mexico (most), Central America', 'America/Chicago', 50, 'd5e6f7a8-9c0d-4e1f-2a3b-4c5d6e7f8a9b' );

INSERT INTO "Scheduler"."TimeZone" ( "name", "abbreviation", "abbreviationDaylightSavings", "supportsDaylightSavings", "standardUTCOffsetHours", "dstUTCOffsetHours", "description", "ianaTimeZone", "sequence", "objectGuid" ) VALUES  ( 'Central Standard Time (no DST)', 'CST', 'CST', false, -6, -6, 'Central America (Guatemala, Costa Rica, Nicaragua, etc.)', 'America/Guatemala', 60, 'f2b768f4-6162-4f65-8eb8-6ae1c5a9dc88' );

INSERT INTO "Scheduler"."TimeZone" ( "name", "abbreviation", "abbreviationDaylightSavings", "supportsDaylightSavings", "standardUTCOffsetHours", "dstUTCOffsetHours", "description", "ianaTimeZone", "sequence", "objectGuid" ) VALUES  ( 'Mountain Standard Time', 'MST', 'MDT', true, -7, -6, 'Mountain United States (except Arizona), Western Canada', 'America/Denver', 70, 'e6f7a8b9-0d1e-4f2a-3b4c-5d6e7f8a9b0c' );

INSERT INTO "Scheduler"."TimeZone" ( "name", "abbreviation", "abbreviationDaylightSavings", "supportsDaylightSavings", "standardUTCOffsetHours", "dstUTCOffsetHours", "description", "ianaTimeZone", "sequence", "objectGuid" ) VALUES  ( 'Arizona Time', 'MST', 'MST', false, -7, -7, 'Arizona (United States) — does not observe Daylight Saving Time', 'America/Phoenix', 80, 'f7a8b9c0-1e2f-4a3b-5c6d-7e8f9a0b1c2d' );

INSERT INTO "Scheduler"."TimeZone" ( "name", "abbreviation", "abbreviationDaylightSavings", "supportsDaylightSavings", "standardUTCOffsetHours", "dstUTCOffsetHours", "description", "ianaTimeZone", "sequence", "objectGuid" ) VALUES  ( 'Pacific Standard Time', 'PST', 'PDT', true, -8, -7, 'Western United States, Western Canada (British Columbia)', 'America/Los_Angeles', 90, 'a8b9c0d1-2f3a-4b5c-6d7e-8f9a0b1c2d3e' );

INSERT INTO "Scheduler"."TimeZone" ( "name", "abbreviation", "abbreviationDaylightSavings", "supportsDaylightSavings", "standardUTCOffsetHours", "dstUTCOffsetHours", "description", "ianaTimeZone", "sequence", "objectGuid" ) VALUES  ( 'Alaska Standard Time', 'AKST', 'AKDT', true, -9, -8, 'Alaska (United States)', 'America/Anchorage', 100, 'b9c0d1e2-3a4b-5c6d-7e8f-9a0b1c2d3e4f' );

INSERT INTO "Scheduler"."TimeZone" ( "name", "abbreviation", "abbreviationDaylightSavings", "supportsDaylightSavings", "standardUTCOffsetHours", "dstUTCOffsetHours", "description", "ianaTimeZone", "sequence", "objectGuid" ) VALUES  ( 'Hawaii-Aleutian Standard Time', 'HST', 'HST', false, -10, -10, 'Hawaii and Aleutian Islands (United States) — no Daylight Saving Time', 'Pacific/Honolulu', 110, 'c0d1e2f3-4b5c-6d7e-8f9a-0b1c2d3e4f5a' );


-- The master list of countries
CREATE TABLE "Scheduler"."Country"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"abbreviation" VARCHAR(10) NOT NULL,
	"postalCodeFormat" VARCHAR(50) NULL,		-- The human readable postal code format for the country, if applicable.
	"postalCodeRegEx" VARCHAR(50) NULL,		-- The regular expression pattern for validation of the postal code, if applicable 
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the Country table's name field.
CREATE INDEX "I_Country_name" ON "Scheduler"."Country" ("name")
;

-- Index on the Country table's active field.
CREATE INDEX "I_Country_active" ON "Scheduler"."Country" ("active")
;

-- Index on the Country table's deleted field.
CREATE INDEX "I_Country_deleted" ON "Scheduler"."Country" ("deleted")
;

INSERT INTO "Scheduler"."Country" ( "name", "description", "abbreviation", "sequence", "postalCodeFormat", "postalCodeRegEx", "objectGuid" ) VALUES  ( 'Canada', 'Canada', 'CA', 1, 'A0A 0A0', '^[A-Z]\d[A-Z] ?\d[A-Z]\d$', '5f3f3c1d-9ba8-48cd-ae6d-4f4d8a5c2bcb' );

INSERT INTO "Scheduler"."Country" ( "name", "description", "abbreviation", "sequence", "postalCodeFormat", "postalCodeRegEx", "objectGuid" ) VALUES  ( 'USA', 'United States of America', 'US', 2, 'NNNNN or NNNNN-NNNN', '^\d{5}(-\d{4})?$'')', '9b2b1de3-719f-4c8a-bb2f-6e903d4e74b5' );


-- The master list of states
CREATE TABLE "Scheduler"."StateProvince"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"countryId" INT NOT NULL,		-- Link to the Country table.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"abbreviation" VARCHAR(10) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "countryId" FOREIGN KEY ("countryId") REFERENCES "Scheduler"."Country"("id"),		-- Foreign key to the Country table.
	CONSTRAINT "UC_StateProvince_name_countryId" UNIQUE ( "name", "countryId") ,		-- Uniqueness enforced on the StateProvince table's name and countryId fields.
	CONSTRAINT "UC_StateProvince_abbreviation_countryId" UNIQUE ( "abbreviation", "countryId") 		-- Uniqueness enforced on the StateProvince table's abbreviation and countryId fields.
);
-- Index on the StateProvince table's countryId field.
CREATE INDEX "I_StateProvince_countryId" ON "Scheduler"."StateProvince" ("countryId")
;

-- Index on the StateProvince table's name field.
CREATE INDEX "I_StateProvince_name" ON "Scheduler"."StateProvince" ("name")
;

-- Index on the StateProvince table's active field.
CREATE INDEX "I_StateProvince_active" ON "Scheduler"."StateProvince" ("active")
;

-- Index on the StateProvince table's deleted field.
CREATE INDEX "I_StateProvince_deleted" ON "Scheduler"."StateProvince" ("deleted")
;

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Newfoundland', 'Newfoundland and Labrador', 'NL', 1, ( SELECT id FROM "Country" WHERE "name" = 'Canada' LIMIT 1), 'a1eecf09-7362-42be-b5d1-90284e1c3075' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Ontario', 'Ontario', 'ON', 2, ( SELECT id FROM "Country" WHERE "name" = 'Canada' LIMIT 1), 'b2e5d8f1-897b-4563-8131-7eeb6d0c80a4' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Alberta', 'Alberta', 'AB', 3, ( SELECT id FROM "Country" WHERE "name" = 'Canada' LIMIT 1), 'c3fe34bc-9601-474f-b99f-55c7a9c71738' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'British Columbia', 'British Columbia', 'BC', 4, ( SELECT id FROM "Country" WHERE "name" = 'Canada' LIMIT 1), 'd4b7ab65-8fc6-4746-b9f6-e9bcf5b8cf91' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Manitoba', 'Manitoba', 'MB', 5, ( SELECT id FROM "Country" WHERE "name" = 'Canada' LIMIT 1), 'e5a8be2d-7a4e-43e5-83d5-d2cf77282c0d' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'New Brunswick', 'New Brunswick', 'NB', 6, ( SELECT id FROM "Country" WHERE "name" = 'Canada' LIMIT 1), 'f6f2a6f4-3963-4539-a54f-bd7ed0be2b3b' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Northwest Territories', 'Northwest Territories', 'NT', 7, ( SELECT id FROM "Country" WHERE "name" = 'Canada' LIMIT 1), '078f1d72-20a4-4b78-8b2f-9c6d6e69f29a' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Nova Scotia', 'Nova Scotia', 'NS', 8, ( SELECT id FROM "Country" WHERE "name" = 'Canada' LIMIT 1), '179fbbf1-b651-4b7a-b17e-b65d6aeb7795' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Nunavut', 'Nunavut', 'NU', 9, ( SELECT id FROM "Country" WHERE "name" = 'Canada' LIMIT 1), '28a1b2ed-7554-48b5-b7f0-b0f2bc3f0a8f' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Prince Edward Island', 'Prince Edward Island', 'PE', 10, ( SELECT id FROM "Country" WHERE "name" = 'Canada' LIMIT 1), '39b8c1de-dc77-4b3b-b0f6-e41b6a557809' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Quebec', 'Quebec', 'QC', 11, ( SELECT id FROM "Country" WHERE "name" = 'Canada' LIMIT 1), '4b9e6f87-b15f-4858-b739-dc23714b83b7' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Saskatchewan', 'Saskatchewan', 'SK', 12, ( SELECT id FROM "Country" WHERE "name" = 'Canada' LIMIT 1), '5c12c0ea-23a0-43a3-a8c9-15d032de5643' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Yukon', 'Yukon', 'YT', 13, ( SELECT id FROM "Country" WHERE "name" = 'Canada' LIMIT 1), '6d1a81eb-fc4a-4c44-9e5a-079c32074749' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Other', 'Other', 'Other', 99, ( SELECT id FROM "Country" WHERE "name" = 'Canada' LIMIT 1), '7e2f5bce-c2b0-4012-84b4-c982d78dce3e' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Alabama', 'Alabama', 'AL', 1, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'd003a92b-6cec-4d49-8baa-6b4fd8fc2f92' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Alaska', 'Alaska', 'AK', 2, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '3aff430d-2752-4d91-ae08-656934438dac' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Arizona', 'Arizona', 'AZ', 3, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '5c4ec86a-472a-4d6c-a278-b5e21352b644' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Arkansas', 'Arkansas', 'AR', 4, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'cd58100a-e5b6-4fc0-a251-2e1a22e66836' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'California', 'California', 'CA', 5, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '36a7adaa-f35a-40ca-8f24-231a3ebd1ad8' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Colorado', 'Colorado', 'CO', 6, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '0210922a-348c-4181-a9e0-6054dd7bc655' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Connecticut', 'Connecticut', 'CT', 7, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '4040cc1a-e6f4-454d-93aa-162c74fe50c6' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Delaware', 'Delaware', 'DE', 8, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '01a5dc36-c285-4216-9fb6-811d5b8e8b48' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Florida', 'Florida', 'FL', 9, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '5e0bb9f6-b6ca-4b42-832f-7c41a570fae4' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Georgia', 'Georgia', 'GA', 10, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'c57ffded-5284-471a-898c-f4969f611dd7' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Hawaii', 'Hawaii', 'HI', 11, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '9fcaa230-ded7-47a8-8a3e-dd1a756ca363' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Idaho', 'Idaho', 'ID', 12, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '796c444b-7513-4823-ab11-94dae65dc0e5' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Illinois', 'Illinois', 'IL', 13, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'd2a28ab4-09c1-437b-b70c-1424543c4128' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Indiana', 'Indiana', 'IN', 14, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '3d9f6c85-6515-4147-adec-ab7dc6e95eab' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Iowa', 'Iowa', 'IA', 15, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'cecfa624-ba4a-473e-a0fc-e91b007beab7' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Kansas', 'Kansas', 'KS', 16, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'b155c44b-c3dd-4884-b715-71ab38596e00' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Kentucky', 'Kentucky', 'KY', 17, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '152ad250-6174-45f7-a947-6c6c14a56494' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Louisiana', 'Louisiana', 'LA', 18, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'c9260be6-9840-420c-acf4-7d82ef937160' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Maine', 'Maine', 'ME', 19, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '97b79ed1-f1b0-44ef-bdd0-71caccd1465d' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Maryland', 'Maryland', 'MD', 20, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'c0cf2ae1-ed20-4845-b860-ff008427359b' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Massachusetts', 'Massachusetts', 'MA', 21, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '7801225d-a996-40cb-888e-49645ffdbb06' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Michigan', 'Michigan', 'MI', 22, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'f9324013-0a60-43ea-b672-6999a821cb15' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Minnesota', 'Minnesota', 'MN', 23, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'f43770fd-ceaf-4646-9943-08be6268c045' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Mississippi', 'Mississippi', 'MS', 24, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'b193e806-5a5e-4d46-936c-b4b3a28e59c5' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Missouri', 'Missouri', 'MO', 25, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'd57e6019-c221-465e-b92e-0b8d3da0ff80' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Montana', 'Montana', 'MT', 26, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '2f10e38c-b937-459f-89d0-60f552687c46' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Nebraska', 'Nebraska', 'NE', 27, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '85ad29eb-f1c6-4862-82bd-d4c91eea2838' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Nevada', 'Nevada', 'NV', 28, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '95ad29eb-f1c6-4862-82bd-d4c91eea2887' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'New Hampshire', 'New Hampshire', 'NH', 29, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '5e5d5651-a186-4cc1-b61a-f22c9d530e6f' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'New Jersey', 'New Jersey', 'NJ', 30, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'ee4ab53d-dab1-4ba7-8363-ed616a779567' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'New Mexico', 'New Mexico', 'NM', 31, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'be168b30-72bd-4942-b187-deff865a5e6a' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'New York', 'New York', 'NY', 32, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '7c93f785-a069-4298-93dc-2ef5e00fd0a8' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'North Carolina', 'North Carolina', 'NC', 33, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'af2af206-9f3c-419f-9731-9fc90f1bda1b' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'North Dakota', 'North Dakota', 'ND', 34, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '3a8d0072-1457-4923-bf19-12b8748098ee' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Ohio', 'Ohio', 'OH', 35, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'd1961e5f-1c25-46ef-9bca-30fe538fe5c9' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Oklahoma', 'Oklahoma', 'OK', 36, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'b2bc6d1b-32b6-4026-b648-70ec7b5063b1' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Oregon', 'Oregon', 'OR', 37, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'fbd6a82b-3f4b-49e0-b5ba-59ec47335c99' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Pennsylvania', 'Pennsylvania', 'PA', 38, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'd9b34153-fb25-403d-a13e-37b2823fbf69' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Rhode Island', 'Rhode Island', 'RI', 39, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'c1c32aa7-af93-4bf1-9acf-9ff591b1b8c5' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'South Carolina', 'South Carolina', 'SC', 40, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '9d050cab-34a0-40eb-8592-2ee2a62e21a1' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'South Dakota', 'South Dakota', 'SD', 41, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'e652bc14-13e0-4405-9feb-6b78dd0790dd' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Tennessee', 'Tennessee', 'TN', 42, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '0d7a100b-792e-46ca-81e0-eaef7e78aec2' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Texas', 'Texas', 'TX', 43, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '5384bf42-c1a8-47c8-998c-85c02838a299' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Utah', 'Utah', 'UT', 44, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '6f4755b9-8a7a-4c52-a8a2-a464de793cbd' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Vermont', 'Vermont', 'VT', 45, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '9dd23ade-bbf4-4d5a-9fd8-199af9005145' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Virginia', 'Virginia', 'VA', 46, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '6071e23d-d660-4801-894e-0ca5783d6a31' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Washington', 'Washington', 'WA', 47, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'cc5b5362-f9fc-406f-927d-d6c4e917f76d' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'West Virginia', 'West Virginia', 'WV', 48, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '06d12574-b3b8-4392-87a1-76a8c42ccf7a' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Wisconsin', 'Wisconsin', 'WI', 49, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'ebf4200d-b4f0-4a62-b2a9-256aab919241' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Wyoming', 'Wyoming', 'WY', 50, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), 'dfff135c-165b-42a9-81f9-a55f8d51c710' );

INSERT INTO "Scheduler"."StateProvince" ( "name", "description", "abbreviation", "sequence", "countryId", "objectGuid" ) VALUES  ( 'Other', 'Other', 'Other', 99, ( SELECT id FROM "Country" WHERE "name" = 'USA' LIMIT 1), '4ab041c0-9479-4a65-ba56-cbb70d82de75' );


/*
Master list of volunteer lifecycle/status values.
Examples: Prospect, Active, On Leave, Inactive, Not Re-invited.
Used to track engagement level and control visibility/assignment rules.
*/
CREATE TABLE "Scheduler"."VolunteerStatus"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"color" VARCHAR(10) NULL,		-- Suggested UI color for this status
	"iconId" INT NULL,		-- Optional icon for visual distinction
	"isActive" BOOLEAN NULL DEFAULT true,		-- Whether volunteers in this status are generally schedulable
	"preventsScheduling" BOOLEAN NOT NULL DEFAULT false,		-- Hard block: cannot be assigned to events
	"requiresApproval" BOOLEAN NOT NULL DEFAULT false,		-- New assignments need coordinator approval
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id")		-- Foreign key to the Icon table.
);
-- Index on the VolunteerStatus table's name field.
CREATE INDEX "I_VolunteerStatus_name" ON "Scheduler"."VolunteerStatus" ("name")
;

-- Index on the VolunteerStatus table's iconId field.
CREATE INDEX "I_VolunteerStatus_iconId" ON "Scheduler"."VolunteerStatus" ("iconId")
;

-- Index on the VolunteerStatus table's active field.
CREATE INDEX "I_VolunteerStatus_active" ON "Scheduler"."VolunteerStatus" ("active")
;

-- Index on the VolunteerStatus table's deleted field.
CREATE INDEX "I_VolunteerStatus_deleted" ON "Scheduler"."VolunteerStatus" ("deleted")
;

INSERT INTO "Scheduler"."VolunteerStatus" ( "name", "description", "sequence", "color", "isActive", "preventsScheduling", "requiresApproval", "objectGuid" ) VALUES  ( 'Pending', 'Self-registered volunteer awaiting admin approval', 5, '#F59E0B', false, true, true, 'a1111111-2222-3333-4444-555555555000' );

INSERT INTO "Scheduler"."VolunteerStatus" ( "name", "description", "sequence", "color", "isActive", "preventsScheduling", "objectGuid" ) VALUES  ( 'Prospect / Interested', 'Has expressed interest but not yet onboarded', 10, '#9E9E9E', false, true, 'a1111111-2222-3333-4444-555555555001' );

INSERT INTO "Scheduler"."VolunteerStatus" ( "name", "description", "sequence", "color", "isActive", "preventsScheduling", "objectGuid" ) VALUES  ( 'Active', 'Fully onboarded and available for assignments', 20, '#4CAF50', true, false, 'a1111111-2222-3333-4444-555555555002' );

INSERT INTO "Scheduler"."VolunteerStatus" ( "name", "description", "sequence", "color", "isActive", "preventsScheduling", "objectGuid" ) VALUES  ( 'On Hiatus / Leave', 'Temporary break (maternity, travel, etc.)', 30, '#FF9800', false, true, 'a1111111-2222-3333-4444-555555555003' );

INSERT INTO "Scheduler"."VolunteerStatus" ( "name", "description", "sequence", "color", "isActive", "preventsScheduling", "objectGuid" ) VALUES  ( 'Inactive', 'No longer participating, but record retained', 40, '#757575', false, true, 'a1111111-2222-3333-4444-555555555004' );

INSERT INTO "Scheduler"."VolunteerStatus" ( "name", "description", "sequence", "color", "isActive", "preventsScheduling", "objectGuid" ) VALUES  ( 'Not Re-invited', 'Previous issues; do not contact or schedule', 50, '#F44336', false, true, 'a1111111-2222-3333-4444-555555555005' );


-- the contact types
CREATE TABLE "Scheduler"."ContactType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id")		-- Foreign key to the Icon table.
);
-- Index on the ContactType table's name field.
CREATE INDEX "I_ContactType_name" ON "Scheduler"."ContactType" ("name")
;

-- Index on the ContactType table's iconId field.
CREATE INDEX "I_ContactType_iconId" ON "Scheduler"."ContactType" ("iconId")
;

-- Index on the ContactType table's active field.
CREATE INDEX "I_ContactType_active" ON "Scheduler"."ContactType" ("active")
;

-- Index on the ContactType table's deleted field.
CREATE INDEX "I_ContactType_deleted" ON "Scheduler"."ContactType" ("deleted")
;

INSERT INTO "Scheduler"."ContactType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Project Manager', 'Primary contact for project coordination', 1, '16df32e3-67e4-4012-b2e5-8810b8ab46b9' );

INSERT INTO "Scheduler"."ContactType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Billing Contact', 'Handles invoices and payments', 2, '1e92d7e0-599c-4c72-9e52-731c1129dd88' );

INSERT INTO "Scheduler"."ContactType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Site Superintendent', 'Site Superintendent', 3, 'f3397214-a488-4522-9968-69f6e9985942' );

INSERT INTO "Scheduler"."ContactType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Safety Officer', 'Health & safety representative', 4, 'cfdc40e3-36cb-4cee-863b-184a494f89bb' );

INSERT INTO "Scheduler"."ContactType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Technical Contact', 'Engineering or specs questions', 5, '9586c951-4a27-4975-94c0-70252c86880b' );

INSERT INTO "Scheduler"."ContactType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Emergency Contact', 'For urgent notifications', 6, '7ff865f4-977a-4e94-974b-e86d942a8405' );

INSERT INTO "Scheduler"."ContactType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Accounts Payable', 'Payment processing', 7, 'f42ce916-a408-44d7-bbd4-9f6fc00243e4' );

INSERT INTO "Scheduler"."ContactType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Volunteer', 'Volunteer', 8, '776395dd-6187-44aa-910e-1bf0135cc88a' );

INSERT INTO "Scheduler"."ContactType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Staff', 'Staff', 9, '5cd5bdee-ba1b-43de-8249-8909546b7d28' );

INSERT INTO "Scheduler"."ContactType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Resident', 'Resident', 10, '688ae8cf-ae9d-44f2-a3a4-a900fff70fd9' );

INSERT INTO "Scheduler"."ContactType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Other', 'Other', 99, '95b327b8-9bfc-4338-a04c-e3f61c56f397' );


-- The contact data
CREATE TABLE "Scheduler"."Contact"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"contactTypeId" INT NOT NULL,		-- Link to the ContactType table.
	"firstName" VARCHAR(250) NOT NULL,
	"middleName" VARCHAR(250) NULL,
	"lastName" VARCHAR(250) NOT NULL,
	"salutationId" INT NULL,		-- Link to the Salutation table.
	"title" VARCHAR(250) NULL,
	"birthDate" DATE NULL,
	"company" VARCHAR(250) NULL,
	"email" VARCHAR(250) NULL,
	"phone" VARCHAR(50) NULL,
	"mobile" VARCHAR(50) NULL,
	"position" VARCHAR(250) NULL,
	"webSite" VARCHAR(1000) NULL,
	"contactMethodId" INT NULL,		-- Link to the ContactMethod table.
	"notes" TEXT NULL,
	"timeZoneId" INT NULL,		-- The contact's time zone
	"attributes" TEXT NULL,		-- to store arbitrary JSON
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"avatarFileName" VARCHAR(250) NULL,		-- Part of the binary data field setup
	"avatarSize" BIGINT NULL,		-- Part of the binary data field setup
	"avatarData" BYTEA NULL,		-- Part of the binary data field setup
	"avatarMimeType" VARCHAR(100) NULL,		-- Part of the binary data field setup
	"externalId" VARCHAR(100) NULL,
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "contactTypeId" FOREIGN KEY ("contactTypeId") REFERENCES "Scheduler"."ContactType"("id"),		-- Foreign key to the ContactType table.
	CONSTRAINT "salutationId" FOREIGN KEY ("salutationId") REFERENCES "Scheduler"."Salutation"("id"),		-- Foreign key to the Salutation table.
	CONSTRAINT "contactMethodId" FOREIGN KEY ("contactMethodId") REFERENCES "Scheduler"."ContactMethod"("id"),		-- Foreign key to the ContactMethod table.
	CONSTRAINT "timeZoneId" FOREIGN KEY ("timeZoneId") REFERENCES "Scheduler"."TimeZone"("id"),		-- Foreign key to the TimeZone table.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id")		-- Foreign key to the Icon table.
);
-- Index on the Contact table's tenantGuid field.
CREATE INDEX "I_Contact_tenantGuid" ON "Scheduler"."Contact" ("tenantGuid")
;

-- Index on the Contact table's tenantGuid,contactTypeId fields.
CREATE INDEX "I_Contact_tenantGuid_contactTypeId" ON "Scheduler"."Contact" ("tenantGuid", "contactTypeId")
;

-- Index on the Contact table's tenantGuid,company fields.
CREATE INDEX "I_Contact_tenantGuid_company" ON "Scheduler"."Contact" ("tenantGuid", "company")
;

-- emails must be unique to one contact.
CREATE UNIQUE INDEX "I_Contact_tenantGuid_email" ON "Scheduler"."Contact" ("tenantGuid", "email")
 WHERE "email" IS NOT NULL;

-- Index on the Contact table's tenantGuid,phone fields.
CREATE INDEX "I_Contact_tenantGuid_phone" ON "Scheduler"."Contact" ("tenantGuid", "phone")
;

-- Index on the Contact table's tenantGuid,mobile fields.
CREATE INDEX "I_Contact_tenantGuid_mobile" ON "Scheduler"."Contact" ("tenantGuid", "mobile")
;

-- Index on the Contact table's tenantGuid,position fields.
CREATE INDEX "I_Contact_tenantGuid_position" ON "Scheduler"."Contact" ("tenantGuid", "position")
;

-- Index on the Contact table's tenantGuid,contactMethodId fields.
CREATE INDEX "I_Contact_tenantGuid_contactMethodId" ON "Scheduler"."Contact" ("tenantGuid", "contactMethodId")
;

-- Index on the Contact table's tenantGuid,timeZoneId fields.
CREATE INDEX "I_Contact_tenantGuid_timeZoneId" ON "Scheduler"."Contact" ("tenantGuid", "timeZoneId")
;

-- Index on the Contact table's tenantGuid,iconId fields.
CREATE INDEX "I_Contact_tenantGuid_iconId" ON "Scheduler"."Contact" ("tenantGuid", "iconId")
;

-- Index on the Contact table's tenantGuid,active fields.
CREATE INDEX "I_Contact_tenantGuid_active" ON "Scheduler"."Contact" ("tenantGuid", "active")
;

-- Index on the Contact table's tenantGuid,deleted fields.
CREATE INDEX "I_Contact_tenantGuid_deleted" ON "Scheduler"."Contact" ("tenantGuid", "deleted")
;

-- Index on the Contact table's tenantGuid,externalId fields.
CREATE INDEX "I_Contact_tenantGuid_externalId" ON "Scheduler"."Contact" ("tenantGuid", "externalId")
;

-- Index on the Contact table's tenantGuid,lastName,firstName fields.
CREATE INDEX "I_Contact_tenantGuid_lastName_firstName" ON "Scheduler"."Contact" ("tenantGuid", "lastName", "firstName")
;


-- The change history for records from the Contact table.
CREATE TABLE "Scheduler"."ContactChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"contactId" INT NOT NULL,		-- Link to the Contact table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "contactId" FOREIGN KEY ("contactId") REFERENCES "Scheduler"."Contact"("id")		-- Foreign key to the Contact table.
);
-- Index on the ContactChangeHistory table's tenantGuid field.
CREATE INDEX "I_ContactChangeHistory_tenantGuid" ON "Scheduler"."ContactChangeHistory" ("tenantGuid")
;

-- Index on the ContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ContactChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ContactChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ContactChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ContactChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ContactChangeHistory_tenantGuid_userId" ON "Scheduler"."ContactChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ContactChangeHistory table's tenantGuid,contactId fields.
CREATE INDEX "I_ContactChangeHistory_tenantGuid_contactId" ON "Scheduler"."ContactChangeHistory" ("tenantGuid", "contactId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- The contact Tag data
CREATE TABLE "Scheduler"."ContactTag"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"contactId" INT NOT NULL,		-- Link to the Contact table.
	"tagId" INT NOT NULL,		-- Link to the Tag table.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "contactId" FOREIGN KEY ("contactId") REFERENCES "Scheduler"."Contact"("id"),		-- Foreign key to the Contact table.
	CONSTRAINT "tagId" FOREIGN KEY ("tagId") REFERENCES "Scheduler"."Tag"("id")		-- Foreign key to the Tag table.
);
-- Index on the ContactTag table's tenantGuid field.
CREATE INDEX "I_ContactTag_tenantGuid" ON "Scheduler"."ContactTag" ("tenantGuid")
;

-- Index on the ContactTag table's tenantGuid,contactId fields.
CREATE INDEX "I_ContactTag_tenantGuid_contactId" ON "Scheduler"."ContactTag" ("tenantGuid", "contactId")
;

-- Index on the ContactTag table's tenantGuid,tagId fields.
CREATE INDEX "I_ContactTag_tenantGuid_tagId" ON "Scheduler"."ContactTag" ("tenantGuid", "tagId")
;

-- Index on the ContactTag table's tenantGuid,active fields.
CREATE INDEX "I_ContactTag_tenantGuid_active" ON "Scheduler"."ContactTag" ("tenantGuid", "active")
;

-- Index on the ContactTag table's tenantGuid,deleted fields.
CREATE INDEX "I_ContactTag_tenantGuid_deleted" ON "Scheduler"."ContactTag" ("tenantGuid", "deleted")
;


-- The change history for records from the ContactTag table.
CREATE TABLE "Scheduler"."ContactTagChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"contactTagId" INT NOT NULL,		-- Link to the ContactTag table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "contactTagId" FOREIGN KEY ("contactTagId") REFERENCES "Scheduler"."ContactTag"("id")		-- Foreign key to the ContactTag table.
);
-- Index on the ContactTagChangeHistory table's tenantGuid field.
CREATE INDEX "I_ContactTagChangeHistory_tenantGuid" ON "Scheduler"."ContactTagChangeHistory" ("tenantGuid")
;

-- Index on the ContactTagChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ContactTagChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ContactTagChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ContactTagChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ContactTagChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ContactTagChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ContactTagChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ContactTagChangeHistory_tenantGuid_userId" ON "Scheduler"."ContactTagChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ContactTagChangeHistory table's tenantGuid,contactTagId fields.
CREATE INDEX "I_ContactTagChangeHistory_tenantGuid_contactTagId" ON "Scheduler"."ContactTagChangeHistory" ("tenantGuid", "contactTagId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Master list of relationship types
CREATE TABLE "Scheduler"."RelationshipType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"isEmergencyEligible" BOOLEAN NOT NULL DEFAULT false,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id")		-- Foreign key to the Icon table.
);
-- Index on the RelationshipType table's name field.
CREATE INDEX "I_RelationshipType_name" ON "Scheduler"."RelationshipType" ("name")
;

-- Index on the RelationshipType table's iconId field.
CREATE INDEX "I_RelationshipType_iconId" ON "Scheduler"."RelationshipType" ("iconId")
;

-- Index on the RelationshipType table's active field.
CREATE INDEX "I_RelationshipType_active" ON "Scheduler"."RelationshipType" ("active")
;

-- Index on the RelationshipType table's deleted field.
CREATE INDEX "I_RelationshipType_deleted" ON "Scheduler"."RelationshipType" ("deleted")
;

INSERT INTO "Scheduler"."RelationshipType" ( "name", "description", "isEmergencyEligible", "sequence", "objectGuid" ) VALUES  ( 'Self', 'Self', false, 1, '3d4ec50a-552b-4826-9f7c-a27915134a21' );

INSERT INTO "Scheduler"."RelationshipType" ( "name", "description", "isEmergencyEligible", "sequence", "objectGuid" ) VALUES  ( 'Colleague', 'Colleague', false, 2, '968a530e-2ec8-449a-b2fa-e853bb82b2c2' );

INSERT INTO "Scheduler"."RelationshipType" ( "name", "description", "isEmergencyEligible", "sequence", "objectGuid" ) VALUES  ( 'Spouse', 'Husband/Wife/Partner', true, 3, 'e0020ae1-4b49-4d3e-a5a1-67f96ca239c8' );

INSERT INTO "Scheduler"."RelationshipType" ( "name", "description", "isEmergencyEligible", "sequence", "objectGuid" ) VALUES  ( 'Parent', 'Mother/Father', true, 4, '8622604b-c5d5-4363-9d63-b0c34f3facb2' );

INSERT INTO "Scheduler"."RelationshipType" ( "name", "description", "isEmergencyEligible", "sequence", "objectGuid" ) VALUES  ( 'Child', 'Son/Daughter', true, 5, 'd35f8329-f18b-445d-8404-0c8fafd9c43b' );

INSERT INTO "Scheduler"."RelationshipType" ( "name", "description", "isEmergencyEligible", "sequence", "objectGuid" ) VALUES  ( 'Sibling', 'Brother/Sister', true, 6, '07ed8aa5-9034-4cad-b8cc-c5564c5945d9' );

INSERT INTO "Scheduler"."RelationshipType" ( "name", "description", "isEmergencyEligible", "sequence", "objectGuid" ) VALUES  ( 'Friend', 'Close friend', true, 7, '57a2e1c3-d06e-48cf-aca5-fe5f396e968f' );

INSERT INTO "Scheduler"."RelationshipType" ( "name", "description", "isEmergencyEligible", "sequence", "objectGuid" ) VALUES  ( 'Supervisor', 'Direct manager', false, 8, '4f51e255-4c2c-41c5-92d9-b051d7d1b15a' );

INSERT INTO "Scheduler"."RelationshipType" ( "name", "description", "isEmergencyEligible", "sequence", "objectGuid" ) VALUES  ( 'Mentor', 'Mentor', false, 9, 'acfdbb6a-bc68-4753-990c-001c9800c155' );

INSERT INTO "Scheduler"."RelationshipType" ( "name", "description", "isEmergencyEligible", "sequence", "objectGuid" ) VALUES  ( 'Mechanic', 'Equipment Maintenance', false, 10, '3108554f-3943-4b8c-a196-ee8154cf9918' );

INSERT INTO "Scheduler"."RelationshipType" ( "name", "description", "isEmergencyEligible", "sequence", "objectGuid" ) VALUES  ( 'Resident', 'Resident', true, 11, '1b92d6de-a154-419e-a3dc-2f0186f029de' );

INSERT INTO "Scheduler"."RelationshipType" ( "name", "description", "isEmergencyEligible", "sequence", "objectGuid" ) VALUES  ( 'Owner', 'Owner', true, 12, 'e603de2c-8f55-44bb-9198-eaa1c1808498' );

INSERT INTO "Scheduler"."RelationshipType" ( "name", "description", "isEmergencyEligible", "sequence", "objectGuid" ) VALUES  ( 'Other', 'Other relationship', false, 99, 'b0fc78e9-ca52-4fdc-823f-0339e11dc069' );


-- The link between a contact and other contacts.
CREATE TABLE "Scheduler"."ContactContact"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"contactId" INT NOT NULL,		-- Link to the Contact table.
	"relatedContactId" INT NOT NULL,		-- Link to the Contact table.
	"isPrimary" BOOLEAN NOT NULL DEFAULT false,		-- Indicates whether or not this contact should be considered a primary contact of the contact.
	"relationshipTypeId" INT NOT NULL,		-- A description of the relationship between the contact and the contact.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "contactId" FOREIGN KEY ("contactId") REFERENCES "Scheduler"."Contact"("id"),		-- Foreign key to the Contact table.
	CONSTRAINT "relatedContactId" FOREIGN KEY ("relatedContactId") REFERENCES "Scheduler"."Contact"("id"),		-- Foreign key to the Contact table.
	CONSTRAINT "relationshipTypeId" FOREIGN KEY ("relationshipTypeId") REFERENCES "Scheduler"."RelationshipType"("id"),		-- Foreign key to the RelationshipType table.
	CONSTRAINT "UC_ContactContact_tenantGuid_contactId_relatedContactId" UNIQUE ( "tenantGuid", "contactId", "relatedContactId") 		-- Uniqueness enforced on the ContactContact table's tenantGuid and contactId and relatedContactId fields.
);
-- Index on the ContactContact table's tenantGuid field.
CREATE INDEX "I_ContactContact_tenantGuid" ON "Scheduler"."ContactContact" ("tenantGuid")
;

-- Index on the ContactContact table's tenantGuid,contactId fields.
CREATE INDEX "I_ContactContact_tenantGuid_contactId" ON "Scheduler"."ContactContact" ("tenantGuid", "contactId")
;

-- Index on the ContactContact table's tenantGuid,relatedContactId fields.
CREATE INDEX "I_ContactContact_tenantGuid_relatedContactId" ON "Scheduler"."ContactContact" ("tenantGuid", "relatedContactId")
;

-- Index on the ContactContact table's tenantGuid,relationshipTypeId fields.
CREATE INDEX "I_ContactContact_tenantGuid_relationshipTypeId" ON "Scheduler"."ContactContact" ("tenantGuid", "relationshipTypeId")
;

-- Index on the ContactContact table's tenantGuid,active fields.
CREATE INDEX "I_ContactContact_tenantGuid_active" ON "Scheduler"."ContactContact" ("tenantGuid", "active")
;

-- Index on the ContactContact table's tenantGuid,deleted fields.
CREATE INDEX "I_ContactContact_tenantGuid_deleted" ON "Scheduler"."ContactContact" ("tenantGuid", "deleted")
;


-- The change history for records from the ContactContact table.
CREATE TABLE "Scheduler"."ContactContactChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"contactContactId" INT NOT NULL,		-- Link to the ContactContact table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "contactContactId" FOREIGN KEY ("contactContactId") REFERENCES "Scheduler"."ContactContact"("id")		-- Foreign key to the ContactContact table.
);
-- Index on the ContactContactChangeHistory table's tenantGuid field.
CREATE INDEX "I_ContactContactChangeHistory_tenantGuid" ON "Scheduler"."ContactContactChangeHistory" ("tenantGuid")
;

-- Index on the ContactContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ContactContactChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ContactContactChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ContactContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ContactContactChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ContactContactChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ContactContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ContactContactChangeHistory_tenantGuid_userId" ON "Scheduler"."ContactContactChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ContactContactChangeHistory table's tenantGuid,contactContactId fields.
CREATE INDEX "I_ContactContactChangeHistory_tenantGuid_contactContactId" ON "Scheduler"."ContactContactChangeHistory" ("tenantGuid", "contactContactId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Master list of office types
CREATE TABLE "Scheduler"."OfficeType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id")		-- Foreign key to the Icon table.
);
-- Index on the OfficeType table's name field.
CREATE INDEX "I_OfficeType_name" ON "Scheduler"."OfficeType" ("name")
;

-- Index on the OfficeType table's iconId field.
CREATE INDEX "I_OfficeType_iconId" ON "Scheduler"."OfficeType" ("iconId")
;

-- Index on the OfficeType table's active field.
CREATE INDEX "I_OfficeType_active" ON "Scheduler"."OfficeType" ("active")
;

-- Index on the OfficeType table's deleted field.
CREATE INDEX "I_OfficeType_deleted" ON "Scheduler"."OfficeType" ("deleted")
;

INSERT INTO "Scheduler"."OfficeType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Headquarters ', 'Headquarters', 1, '3dc56597-1ab7-403e-bad9-8bd52c674f9d' );

INSERT INTO "Scheduler"."OfficeType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Regional Office', 'Regional Office', 2, 'f28b5678-de69-43a3-9a9e-7194df40ea32' );

INSERT INTO "Scheduler"."OfficeType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Branch Office', 'Branch Office', 3, 'd504aef3-b582-4f6d-91c8-b76142f5462a' );

INSERT INTO "Scheduler"."OfficeType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Depot / Yard', 'Depot / Yard', 4, '98b72f2e-de47-4268-885e-3ab7a63e9e8c' );

INSERT INTO "Scheduler"."OfficeType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Administrative Office', 'Administrative Office', 5, 'edc174d4-66f3-410f-a173-b15007c1ff48' );

INSERT INTO "Scheduler"."OfficeType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Warehouse', 'Warehouse', 6, 'c595846a-c3f3-4e07-9df0-af117fa5a400' );

INSERT INTO "Scheduler"."OfficeType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Hospital', 'Hospital', 7, '52a134df-ff0c-4391-ac85-93be54e9541b' );

INSERT INTO "Scheduler"."OfficeType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Clinic', 'Clinic', 8, '9bd149c1-ca03-49c1-a71f-7d8479697205' );


-- The main list of offices operated by an organization using the Scheduler.  Allows schedule and resource grouping.
CREATE TABLE "Scheduler"."Office"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"officeTypeId" INT NOT NULL,		-- Link to the OfficeType table.
	"timeZoneId" INT NOT NULL,		-- Time zone of the office.
	"currencyId" INT NOT NULL,		-- Default billing currency of the office.
	"addressLine1" VARCHAR(250) NOT NULL,
	"addressLine2" VARCHAR(250) NULL,
	"city" VARCHAR(100) NOT NULL,
	"postalCode" VARCHAR(100) NULL,
	"stateProvinceId" INT NOT NULL,		-- Link to the StateProvince table.
	"countryId" INT NOT NULL,		-- Link to the Country table.
	"phone" VARCHAR(100) NULL,
	"email" VARCHAR(250) NULL,
	"latitude" DOUBLE PRECISION NULL,		-- Optional latitude position
	"longitude" DOUBLE PRECISION NULL,		-- Optional longitude position
	"notes" TEXT NULL,
	"externalId" VARCHAR(100) NULL,		-- Optional reference to an ID in an external system 
	"color" VARCHAR(10) NULL,		-- Override of Target Type Hex color for UI display
	"attributes" TEXT NULL,		-- to store arbitrary JSON
	"avatarFileName" VARCHAR(250) NULL,		-- Part of the binary data field setup
	"avatarSize" BIGINT NULL,		-- Part of the binary data field setup
	"avatarData" BYTEA NULL,		-- Part of the binary data field setup
	"avatarMimeType" VARCHAR(100) NULL,		-- Part of the binary data field setup
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "officeTypeId" FOREIGN KEY ("officeTypeId") REFERENCES "Scheduler"."OfficeType"("id"),		-- Foreign key to the OfficeType table.
	CONSTRAINT "timeZoneId" FOREIGN KEY ("timeZoneId") REFERENCES "Scheduler"."TimeZone"("id"),		-- Foreign key to the TimeZone table.
	CONSTRAINT "currencyId" FOREIGN KEY ("currencyId") REFERENCES "Scheduler"."Currency"("id"),		-- Foreign key to the Currency table.
	CONSTRAINT "stateProvinceId" FOREIGN KEY ("stateProvinceId") REFERENCES "Scheduler"."StateProvince"("id"),		-- Foreign key to the StateProvince table.
	CONSTRAINT "countryId" FOREIGN KEY ("countryId") REFERENCES "Scheduler"."Country"("id"),		-- Foreign key to the Country table.
	CONSTRAINT "UC_Office_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Office table's tenantGuid and name fields.
);
-- Index on the Office table's tenantGuid field.
CREATE INDEX "I_Office_tenantGuid" ON "Scheduler"."Office" ("tenantGuid")
;

-- Index on the Office table's tenantGuid,name fields.
CREATE INDEX "I_Office_tenantGuid_name" ON "Scheduler"."Office" ("tenantGuid", "name")
;

-- Index on the Office table's tenantGuid,officeTypeId fields.
CREATE INDEX "I_Office_tenantGuid_officeTypeId" ON "Scheduler"."Office" ("tenantGuid", "officeTypeId")
;

-- Index on the Office table's tenantGuid,timeZoneId fields.
CREATE INDEX "I_Office_tenantGuid_timeZoneId" ON "Scheduler"."Office" ("tenantGuid", "timeZoneId")
;

-- Index on the Office table's tenantGuid,currencyId fields.
CREATE INDEX "I_Office_tenantGuid_currencyId" ON "Scheduler"."Office" ("tenantGuid", "currencyId")
;

-- Index on the Office table's tenantGuid,stateProvinceId fields.
CREATE INDEX "I_Office_tenantGuid_stateProvinceId" ON "Scheduler"."Office" ("tenantGuid", "stateProvinceId")
;

-- Index on the Office table's tenantGuid,countryId fields.
CREATE INDEX "I_Office_tenantGuid_countryId" ON "Scheduler"."Office" ("tenantGuid", "countryId")
;

-- Index on the Office table's tenantGuid,email fields.
CREATE UNIQUE INDEX "I_Office_tenantGuid_email" ON "Scheduler"."Office" ("tenantGuid", "email")
 WHERE "email" IS NOT NULL;

-- Index on the Office table's tenantGuid,active fields.
CREATE INDEX "I_Office_tenantGuid_active" ON "Scheduler"."Office" ("tenantGuid", "active")
;

-- Index on the Office table's tenantGuid,deleted fields.
CREATE INDEX "I_Office_tenantGuid_deleted" ON "Scheduler"."Office" ("tenantGuid", "deleted")
;


-- The change history for records from the Office table.
CREATE TABLE "Scheduler"."OfficeChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"officeId" INT NOT NULL,		-- Link to the Office table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "officeId" FOREIGN KEY ("officeId") REFERENCES "Scheduler"."Office"("id")		-- Foreign key to the Office table.
);
-- Index on the OfficeChangeHistory table's tenantGuid field.
CREATE INDEX "I_OfficeChangeHistory_tenantGuid" ON "Scheduler"."OfficeChangeHistory" ("tenantGuid")
;

-- Index on the OfficeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_OfficeChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."OfficeChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the OfficeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_OfficeChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."OfficeChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the OfficeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_OfficeChangeHistory_tenantGuid_userId" ON "Scheduler"."OfficeChangeHistory" ("tenantGuid", "userId")
;

-- Index on the OfficeChangeHistory table's tenantGuid,officeId fields.
CREATE INDEX "I_OfficeChangeHistory_tenantGuid_officeId" ON "Scheduler"."OfficeChangeHistory" ("tenantGuid", "officeId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- The link between contacts and offices.
CREATE TABLE "Scheduler"."OfficeContact"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"officeId" INT NOT NULL,		-- Link to the Office table.
	"contactId" INT NOT NULL,		-- Link to the Contact table.
	"isPrimary" BOOLEAN NOT NULL DEFAULT false,		-- Indicates whether or not this contact should be considered a primary contact of the office.
	"relationshipTypeId" INT NOT NULL,		-- A description of the relationship between the office and the contact.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "officeId" FOREIGN KEY ("officeId") REFERENCES "Scheduler"."Office"("id"),		-- Foreign key to the Office table.
	CONSTRAINT "contactId" FOREIGN KEY ("contactId") REFERENCES "Scheduler"."Contact"("id"),		-- Foreign key to the Contact table.
	CONSTRAINT "relationshipTypeId" FOREIGN KEY ("relationshipTypeId") REFERENCES "Scheduler"."RelationshipType"("id"),		-- Foreign key to the RelationshipType table.
	CONSTRAINT "UC_OfficeContact_tenantGuid_officeId_contactId" UNIQUE ( "tenantGuid", "officeId", "contactId") 		-- Uniqueness enforced on the OfficeContact table's tenantGuid and officeId and contactId fields.
);
-- Index on the OfficeContact table's tenantGuid field.
CREATE INDEX "I_OfficeContact_tenantGuid" ON "Scheduler"."OfficeContact" ("tenantGuid")
;

-- Index on the OfficeContact table's tenantGuid,officeId fields.
CREATE INDEX "I_OfficeContact_tenantGuid_officeId" ON "Scheduler"."OfficeContact" ("tenantGuid", "officeId")
;

-- Index on the OfficeContact table's tenantGuid,contactId fields.
CREATE INDEX "I_OfficeContact_tenantGuid_contactId" ON "Scheduler"."OfficeContact" ("tenantGuid", "contactId")
;

-- Index on the OfficeContact table's tenantGuid,relationshipTypeId fields.
CREATE INDEX "I_OfficeContact_tenantGuid_relationshipTypeId" ON "Scheduler"."OfficeContact" ("tenantGuid", "relationshipTypeId")
;

-- Index on the OfficeContact table's tenantGuid,active fields.
CREATE INDEX "I_OfficeContact_tenantGuid_active" ON "Scheduler"."OfficeContact" ("tenantGuid", "active")
;

-- Index on the OfficeContact table's tenantGuid,deleted fields.
CREATE INDEX "I_OfficeContact_tenantGuid_deleted" ON "Scheduler"."OfficeContact" ("tenantGuid", "deleted")
;


-- The change history for records from the OfficeContact table.
CREATE TABLE "Scheduler"."OfficeContactChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"officeContactId" INT NOT NULL,		-- Link to the OfficeContact table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "officeContactId" FOREIGN KEY ("officeContactId") REFERENCES "Scheduler"."OfficeContact"("id")		-- Foreign key to the OfficeContact table.
);
-- Index on the OfficeContactChangeHistory table's tenantGuid field.
CREATE INDEX "I_OfficeContactChangeHistory_tenantGuid" ON "Scheduler"."OfficeContactChangeHistory" ("tenantGuid")
;

-- Index on the OfficeContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_OfficeContactChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."OfficeContactChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the OfficeContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_OfficeContactChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."OfficeContactChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the OfficeContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_OfficeContactChangeHistory_tenantGuid_userId" ON "Scheduler"."OfficeContactChangeHistory" ("tenantGuid", "userId")
;

-- Index on the OfficeContactChangeHistory table's tenantGuid,officeContactId fields.
CREATE INDEX "I_OfficeContactChangeHistory_tenantGuid_officeContactId" ON "Scheduler"."OfficeContactChangeHistory" ("tenantGuid", "officeContactId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Optional logical grouping of events for visibility and filtering (e.g., '2026 Road Projects', 'Maintenance Calendar').
CREATE TABLE "Scheduler"."Calendar"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"officeId" INT NULL,		-- Optional office binding for the calendar
	"isDefault" BOOLEAN NULL,
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	CONSTRAINT "officeId" FOREIGN KEY ("officeId") REFERENCES "Scheduler"."Office"("id"),		-- Foreign key to the Office table.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_Calendar_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Calendar table's tenantGuid and name fields.
);
-- Index on the Calendar table's tenantGuid field.
CREATE INDEX "I_Calendar_tenantGuid" ON "Scheduler"."Calendar" ("tenantGuid")
;

-- Index on the Calendar table's tenantGuid,name fields.
CREATE INDEX "I_Calendar_tenantGuid_name" ON "Scheduler"."Calendar" ("tenantGuid", "name")
;

-- Index on the Calendar table's tenantGuid,officeId fields.
CREATE INDEX "I_Calendar_tenantGuid_officeId" ON "Scheduler"."Calendar" ("tenantGuid", "officeId")
;

-- Index on the Calendar table's tenantGuid,iconId fields.
CREATE INDEX "I_Calendar_tenantGuid_iconId" ON "Scheduler"."Calendar" ("tenantGuid", "iconId")
;

-- Index on the Calendar table's tenantGuid,active fields.
CREATE INDEX "I_Calendar_tenantGuid_active" ON "Scheduler"."Calendar" ("tenantGuid", "active")
;

-- Index on the Calendar table's tenantGuid,deleted fields.
CREATE INDEX "I_Calendar_tenantGuid_deleted" ON "Scheduler"."Calendar" ("tenantGuid", "deleted")
;


-- The change history for records from the Calendar table.
CREATE TABLE "Scheduler"."CalendarChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"calendarId" INT NOT NULL,		-- Link to the Calendar table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "calendarId" FOREIGN KEY ("calendarId") REFERENCES "Scheduler"."Calendar"("id")		-- Foreign key to the Calendar table.
);
-- Index on the CalendarChangeHistory table's tenantGuid field.
CREATE INDEX "I_CalendarChangeHistory_tenantGuid" ON "Scheduler"."CalendarChangeHistory" ("tenantGuid")
;

-- Index on the CalendarChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_CalendarChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."CalendarChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the CalendarChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_CalendarChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."CalendarChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the CalendarChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_CalendarChangeHistory_tenantGuid_userId" ON "Scheduler"."CalendarChangeHistory" ("tenantGuid", "userId")
;

-- Index on the CalendarChangeHistory table's tenantGuid,calendarId fields.
CREATE INDEX "I_CalendarChangeHistory_tenantGuid_calendarId" ON "Scheduler"."CalendarChangeHistory" ("tenantGuid", "calendarId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Master list of client types.  Used for categorizing clients.
CREATE TABLE "Scheduler"."ClientType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_ClientType_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the ClientType table's tenantGuid and name fields.
);
-- Index on the ClientType table's tenantGuid field.
CREATE INDEX "I_ClientType_tenantGuid" ON "Scheduler"."ClientType" ("tenantGuid")
;

-- Index on the ClientType table's tenantGuid,name fields.
CREATE INDEX "I_ClientType_tenantGuid_name" ON "Scheduler"."ClientType" ("tenantGuid", "name")
;

-- Index on the ClientType table's tenantGuid,iconId fields.
CREATE INDEX "I_ClientType_tenantGuid_iconId" ON "Scheduler"."ClientType" ("tenantGuid", "iconId")
;

-- Index on the ClientType table's tenantGuid,active fields.
CREATE INDEX "I_ClientType_tenantGuid_active" ON "Scheduler"."ClientType" ("tenantGuid", "active")
;

-- Index on the ClientType table's tenantGuid,deleted fields.
CREATE INDEX "I_ClientType_tenantGuid_deleted" ON "Scheduler"."ClientType" ("tenantGuid", "deleted")
;

INSERT INTO "Scheduler"."ClientType" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Construction ', 'A construction client', 1, '331c07c6-bcd1-4d8d-b796-d81216bba704' );

INSERT INTO "Scheduler"."ClientType" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Healthcare', 'A healthcare client', 2, '701001e4-4034-4b18-ab29-b514b08bc541' );


-- The main client list.  Is not directly schedulable, but provides billing details.  Contains scheduling targets which are schedulable.
CREATE TABLE "Scheduler"."Client"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"clientTypeId" INT NOT NULL,		-- Link to the ClientType table.
	"currencyId" INT NOT NULL,		-- Link to the Currency table.
	"timeZoneId" INT NOT NULL,		-- Link to the TimeZone table.
	"calendarId" INT NULL,		-- An optional default calendar for the scheduling target's belonging to the client.
	"addressLine1" VARCHAR(250) NOT NULL,
	"addressLine2" VARCHAR(250) NULL,
	"city" VARCHAR(100) NOT NULL,
	"postalCode" VARCHAR(100) NULL,
	"stateProvinceId" INT NOT NULL,		-- Link to the StateProvince table.
	"countryId" INT NOT NULL,		-- Link to the Country table.
	"phone" VARCHAR(100) NULL,
	"email" VARCHAR(250) NULL,
	"latitude" DOUBLE PRECISION NULL,		-- Optional latitude position
	"longitude" DOUBLE PRECISION NULL,		-- Optional longitude position
	"notes" TEXT NULL,
	"externalId" VARCHAR(100) NULL,		-- Optional reference to an ID in an external system
	"color" VARCHAR(10) NULL,		-- Override of Target Type Hex color for UI display
	"attributes" TEXT NULL,		-- to store arbitrary JSON
	"avatarFileName" VARCHAR(250) NULL,		-- Part of the binary data field setup
	"avatarSize" BIGINT NULL,		-- Part of the binary data field setup
	"avatarData" BYTEA NULL,		-- Part of the binary data field setup
	"avatarMimeType" VARCHAR(100) NULL,		-- Part of the binary data field setup
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "clientTypeId" FOREIGN KEY ("clientTypeId") REFERENCES "Scheduler"."ClientType"("id"),		-- Foreign key to the ClientType table.
	CONSTRAINT "currencyId" FOREIGN KEY ("currencyId") REFERENCES "Scheduler"."Currency"("id"),		-- Foreign key to the Currency table.
	CONSTRAINT "timeZoneId" FOREIGN KEY ("timeZoneId") REFERENCES "Scheduler"."TimeZone"("id"),		-- Foreign key to the TimeZone table.
	CONSTRAINT "calendarId" FOREIGN KEY ("calendarId") REFERENCES "Scheduler"."Calendar"("id"),		-- Foreign key to the Calendar table.
	CONSTRAINT "stateProvinceId" FOREIGN KEY ("stateProvinceId") REFERENCES "Scheduler"."StateProvince"("id"),		-- Foreign key to the StateProvince table.
	CONSTRAINT "countryId" FOREIGN KEY ("countryId") REFERENCES "Scheduler"."Country"("id"),		-- Foreign key to the Country table.
	CONSTRAINT "UC_Client_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Client table's tenantGuid and name fields.
);
-- Index on the Client table's tenantGuid field.
CREATE INDEX "I_Client_tenantGuid" ON "Scheduler"."Client" ("tenantGuid")
;

-- Index on the Client table's tenantGuid,name fields.
CREATE INDEX "I_Client_tenantGuid_name" ON "Scheduler"."Client" ("tenantGuid", "name")
;

-- Index on the Client table's tenantGuid,clientTypeId fields.
CREATE INDEX "I_Client_tenantGuid_clientTypeId" ON "Scheduler"."Client" ("tenantGuid", "clientTypeId")
;

-- Index on the Client table's tenantGuid,currencyId fields.
CREATE INDEX "I_Client_tenantGuid_currencyId" ON "Scheduler"."Client" ("tenantGuid", "currencyId")
;

-- Index on the Client table's tenantGuid,timeZoneId fields.
CREATE INDEX "I_Client_tenantGuid_timeZoneId" ON "Scheduler"."Client" ("tenantGuid", "timeZoneId")
;

-- Index on the Client table's tenantGuid,stateProvinceId fields.
CREATE INDEX "I_Client_tenantGuid_stateProvinceId" ON "Scheduler"."Client" ("tenantGuid", "stateProvinceId")
;

-- Index on the Client table's tenantGuid,countryId fields.
CREATE INDEX "I_Client_tenantGuid_countryId" ON "Scheduler"."Client" ("tenantGuid", "countryId")
;

-- emails must be unique to one Client.
CREATE UNIQUE INDEX "I_Client_tenantGuid_email" ON "Scheduler"."Client" ("tenantGuid", "email")
 WHERE "email" IS NOT NULL;

-- Index on the Client table's tenantGuid,active fields.
CREATE INDEX "I_Client_tenantGuid_active" ON "Scheduler"."Client" ("tenantGuid", "active")
;

-- Index on the Client table's tenantGuid,deleted fields.
CREATE INDEX "I_Client_tenantGuid_deleted" ON "Scheduler"."Client" ("tenantGuid", "deleted")
;


-- The change history for records from the Client table.
CREATE TABLE "Scheduler"."ClientChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"clientId" INT NOT NULL,		-- Link to the Client table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "clientId" FOREIGN KEY ("clientId") REFERENCES "Scheduler"."Client"("id")		-- Foreign key to the Client table.
);
-- Index on the ClientChangeHistory table's tenantGuid field.
CREATE INDEX "I_ClientChangeHistory_tenantGuid" ON "Scheduler"."ClientChangeHistory" ("tenantGuid")
;

-- Index on the ClientChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ClientChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ClientChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ClientChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ClientChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ClientChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ClientChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ClientChangeHistory_tenantGuid_userId" ON "Scheduler"."ClientChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ClientChangeHistory table's tenantGuid,clientId fields.
CREATE INDEX "I_ClientChangeHistory_tenantGuid_clientId" ON "Scheduler"."ClientChangeHistory" ("tenantGuid", "clientId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- The link between contacts and clients.
CREATE TABLE "Scheduler"."ClientContact"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"clientId" INT NOT NULL,		-- Link to the Client table.
	"contactId" INT NOT NULL,		-- Link to the Contact table.
	"isPrimary" BOOLEAN NOT NULL DEFAULT false,		-- Indicates whether or not this contact should be considered a primary contact of the client.
	"relationshipTypeId" INT NOT NULL,		-- A description of the relationship between the client and the contact.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "clientId" FOREIGN KEY ("clientId") REFERENCES "Scheduler"."Client"("id"),		-- Foreign key to the Client table.
	CONSTRAINT "contactId" FOREIGN KEY ("contactId") REFERENCES "Scheduler"."Contact"("id"),		-- Foreign key to the Contact table.
	CONSTRAINT "relationshipTypeId" FOREIGN KEY ("relationshipTypeId") REFERENCES "Scheduler"."RelationshipType"("id"),		-- Foreign key to the RelationshipType table.
	CONSTRAINT "UC_ClientContact_tenantGuid_clientId_contactId" UNIQUE ( "tenantGuid", "clientId", "contactId") 		-- Uniqueness enforced on the ClientContact table's tenantGuid and clientId and contactId fields.
);
-- Index on the ClientContact table's tenantGuid field.
CREATE INDEX "I_ClientContact_tenantGuid" ON "Scheduler"."ClientContact" ("tenantGuid")
;

-- Index on the ClientContact table's tenantGuid,clientId fields.
CREATE INDEX "I_ClientContact_tenantGuid_clientId" ON "Scheduler"."ClientContact" ("tenantGuid", "clientId")
;

-- Index on the ClientContact table's tenantGuid,contactId fields.
CREATE INDEX "I_ClientContact_tenantGuid_contactId" ON "Scheduler"."ClientContact" ("tenantGuid", "contactId")
;

-- Index on the ClientContact table's tenantGuid,relationshipTypeId fields.
CREATE INDEX "I_ClientContact_tenantGuid_relationshipTypeId" ON "Scheduler"."ClientContact" ("tenantGuid", "relationshipTypeId")
;

-- Index on the ClientContact table's tenantGuid,active fields.
CREATE INDEX "I_ClientContact_tenantGuid_active" ON "Scheduler"."ClientContact" ("tenantGuid", "active")
;

-- Index on the ClientContact table's tenantGuid,deleted fields.
CREATE INDEX "I_ClientContact_tenantGuid_deleted" ON "Scheduler"."ClientContact" ("tenantGuid", "deleted")
;


-- The change history for records from the ClientContact table.
CREATE TABLE "Scheduler"."ClientContactChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"clientContactId" INT NOT NULL,		-- Link to the ClientContact table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "clientContactId" FOREIGN KEY ("clientContactId") REFERENCES "Scheduler"."ClientContact"("id")		-- Foreign key to the ClientContact table.
);
-- Index on the ClientContactChangeHistory table's tenantGuid field.
CREATE INDEX "I_ClientContactChangeHistory_tenantGuid" ON "Scheduler"."ClientContactChangeHistory" ("tenantGuid")
;

-- Index on the ClientContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ClientContactChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ClientContactChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ClientContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ClientContactChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ClientContactChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ClientContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ClientContactChangeHistory_tenantGuid_userId" ON "Scheduler"."ClientContactChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ClientContactChangeHistory table's tenantGuid,clientContactId fields.
CREATE INDEX "I_ClientContactChangeHistory_tenantGuid_clientContactId" ON "Scheduler"."ClientContactChangeHistory" ("tenantGuid", "clientContactId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Tenant-level information. Client admins manage this data.
CREATE TABLE "Scheduler"."TenantProfile"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"companyLogoFileName" VARCHAR(250) NULL,		-- Part of the binary data field setup
	"companyLogoSize" BIGINT NULL,		-- Part of the binary data field setup
	"companyLogoData" BYTEA NULL,		-- Part of the binary data field setup
	"companyLogoMimeType" VARCHAR(100) NULL,		-- Part of the binary data field setup
	"addressLine1" VARCHAR(250) NULL,
	"addressLine2" VARCHAR(250) NULL,
	"addressLine3" VARCHAR(250) NULL,
	"city" VARCHAR(100) NULL,
	"postalCode" VARCHAR(100) NULL,
	"stateProvinceId" INT NULL,		-- Link to the StateProvince table.
	"countryId" INT NULL,		-- Link to the Country table.
	"timeZoneId" INT NULL,		-- Link to the TimeZone table.
	"phoneNumber" VARCHAR(100) NULL,
	"email" VARCHAR(250) NULL,
	"website" VARCHAR(1000) NULL,
	"latitude" DOUBLE PRECISION NULL,		-- Optional latitude position
	"longitude" DOUBLE PRECISION NULL,		-- Optional longitude position
	"primaryColor" VARCHAR(10) NULL,
	"secondaryColor" VARCHAR(10) NULL,
	"displaysMetric" BOOLEAN NOT NULL DEFAULT false,		-- True if the tenant defaults to using metric units when creating projects.    Note that this does not affect the storage units, which are always metric.
	"displaysUSTerms" BOOLEAN NOT NULL DEFAULT false,		-- True if the tenant defaults to using terms for the US market, such as Zip code,.
	"invoiceNumberMask" VARCHAR(100) NULL,		-- Format mask for auto-generating invoice numbers. Supports {YYYY} for year and {NNNN} for zero-padded sequence. Default: INV-{YYYY}-{NNNN}.
	"receiptNumberMask" VARCHAR(100) NULL,		-- Format mask for auto-generating receipt numbers. Supports {YYYY} for year and {NNNN} for zero-padded sequence. Default: REC-{YYYY}-{NNNN}.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "stateProvinceId" FOREIGN KEY ("stateProvinceId") REFERENCES "Scheduler"."StateProvince"("id"),		-- Foreign key to the StateProvince table.
	CONSTRAINT "countryId" FOREIGN KEY ("countryId") REFERENCES "Scheduler"."Country"("id"),		-- Foreign key to the Country table.
	CONSTRAINT "timeZoneId" FOREIGN KEY ("timeZoneId") REFERENCES "Scheduler"."TimeZone"("id"),		-- Foreign key to the TimeZone table.
	CONSTRAINT "UC_TenantProfile_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the TenantProfile table's tenantGuid and name fields.
);
-- Index on the TenantProfile table's tenantGuid field.
CREATE INDEX "I_TenantProfile_tenantGuid" ON "Scheduler"."TenantProfile" ("tenantGuid")
;

-- Index on the TenantProfile table's tenantGuid,name fields.
CREATE INDEX "I_TenantProfile_tenantGuid_name" ON "Scheduler"."TenantProfile" ("tenantGuid", "name")
;

-- Index on the TenantProfile table's tenantGuid,timeZoneId fields.
CREATE INDEX "I_TenantProfile_tenantGuid_timeZoneId" ON "Scheduler"."TenantProfile" ("tenantGuid", "timeZoneId")
;

-- Index on the TenantProfile table's tenantGuid,active fields.
CREATE INDEX "I_TenantProfile_tenantGuid_active" ON "Scheduler"."TenantProfile" ("tenantGuid", "active")
;

-- Index on the TenantProfile table's tenantGuid,deleted fields.
CREATE INDEX "I_TenantProfile_tenantGuid_deleted" ON "Scheduler"."TenantProfile" ("tenantGuid", "deleted")
;


-- The change history for records from the TenantProfile table.
CREATE TABLE "Scheduler"."TenantProfileChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"tenantProfileId" INT NOT NULL,		-- Link to the TenantProfile table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "tenantProfileId" FOREIGN KEY ("tenantProfileId") REFERENCES "Scheduler"."TenantProfile"("id")		-- Foreign key to the TenantProfile table.
);
-- Index on the TenantProfileChangeHistory table's tenantGuid field.
CREATE INDEX "I_TenantProfileChangeHistory_tenantGuid" ON "Scheduler"."TenantProfileChangeHistory" ("tenantGuid")
;

-- Index on the TenantProfileChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_TenantProfileChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."TenantProfileChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the TenantProfileChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_TenantProfileChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."TenantProfileChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the TenantProfileChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_TenantProfileChangeHistory_tenantGuid_userId" ON "Scheduler"."TenantProfileChangeHistory" ("tenantGuid", "userId")
;

-- Index on the TenantProfileChangeHistory table's tenantGuid,tenantProfileId fields.
CREATE INDEX "I_TenantProfileChangeHistory_tenantGuid_tenantProfileId" ON "Scheduler"."TenantProfileChangeHistory" ("tenantGuid", "tenantProfileId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Master list of qualifications, certifications, or competencies required for certain work.  Examples: RN License, Crane Operator Certification, OSHA 30, Pediatric Specialty, Confined Space Entry.
CREATE TABLE "Scheduler"."Qualification"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"isLicense" BOOLEAN NULL,		-- for special handling (e.g., expiry warnings)
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_Qualification_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Qualification table's tenantGuid and name fields.
);
-- Index on the Qualification table's tenantGuid field.
CREATE INDEX "I_Qualification_tenantGuid" ON "Scheduler"."Qualification" ("tenantGuid")
;

-- Index on the Qualification table's tenantGuid,name fields.
CREATE INDEX "I_Qualification_tenantGuid_name" ON "Scheduler"."Qualification" ("tenantGuid", "name")
;

-- Index on the Qualification table's tenantGuid,active fields.
CREATE INDEX "I_Qualification_tenantGuid_active" ON "Scheduler"."Qualification" ("tenantGuid", "active")
;

-- Index on the Qualification table's tenantGuid,deleted fields.
CREATE INDEX "I_Qualification_tenantGuid_deleted" ON "Scheduler"."Qualification" ("tenantGuid", "deleted")
;


-- Tenant-configurable roles that a resource can fulfil during an event.  Examples: Operator, Supervisor, Driver, Spotter, Safety Officer.  Used for business rule enforcement and richer reporting.
CREATE TABLE "Scheduler"."AssignmentRole"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_AssignmentRole_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the AssignmentRole table's tenantGuid and name fields.
);
-- Index on the AssignmentRole table's tenantGuid field.
CREATE INDEX "I_AssignmentRole_tenantGuid" ON "Scheduler"."AssignmentRole" ("tenantGuid")
;

-- Index on the AssignmentRole table's tenantGuid,name fields.
CREATE INDEX "I_AssignmentRole_tenantGuid_name" ON "Scheduler"."AssignmentRole" ("tenantGuid", "name")
;

-- Index on the AssignmentRole table's tenantGuid,iconId fields.
CREATE INDEX "I_AssignmentRole_tenantGuid_iconId" ON "Scheduler"."AssignmentRole" ("tenantGuid", "iconId")
;

-- Index on the AssignmentRole table's tenantGuid,active fields.
CREATE INDEX "I_AssignmentRole_tenantGuid_active" ON "Scheduler"."AssignmentRole" ("tenantGuid", "active")
;

-- Index on the AssignmentRole table's tenantGuid,deleted fields.
CREATE INDEX "I_AssignmentRole_tenantGuid_deleted" ON "Scheduler"."AssignmentRole" ("tenantGuid", "deleted")
;

INSERT INTO "Scheduler"."AssignmentRole" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Operator', 'Primary equipment operator', 1, 'b2c3d4e5-6789-0123-4567-89abcdef0001' );

INSERT INTO "Scheduler"."AssignmentRole" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Supervisor', 'Site supervisor', 2, 'b2c3d4e5-6789-0123-4567-89abcdef0002' );

INSERT INTO "Scheduler"."AssignmentRole" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Driver', 'Haul truck or service vehicle driver', 3, 'b2c3d4e5-6789-0123-4567-89abcdef0003' );

INSERT INTO "Scheduler"."AssignmentRole" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Spotter', 'Safety spotter / banksman', 4, 'b2c3d4e5-6789-0123-4567-89abcdef0004' );


-- Defines which qualifications are required to fulfill a specific AssignmentRole.  This is the most common way to enforce certification requirements.
CREATE TABLE "Scheduler"."AssignmentRoleQualificationRequirement"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"assignmentRoleId" INT NOT NULL,		-- Link to the AssignmentRole table.
	"qualificationId" INT NOT NULL,		-- Link to the Qualification table.
	"isRequired" BOOLEAN NOT NULL DEFAULT true,		-- true = mandatory to fulfill role, false = preferred/recommended
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "assignmentRoleId" FOREIGN KEY ("assignmentRoleId") REFERENCES "Scheduler"."AssignmentRole"("id"),		-- Foreign key to the AssignmentRole table.
	CONSTRAINT "qualificationId" FOREIGN KEY ("qualificationId") REFERENCES "Scheduler"."Qualification"("id"),		-- Foreign key to the Qualification table.
	CONSTRAINT "UC_AssignmentRoleQualificationRequirement_tenantGuid_assignmentRoleId_qualificationId" UNIQUE ( "tenantGuid", "assignmentRoleId", "qualificationId") 		-- Uniqueness enforced on the AssignmentRoleQualificationRequirement table's tenantGuid and assignmentRoleId and qualificationId fields.
);
-- Index on the AssignmentRoleQualificationRequirement table's tenantGuid field.
CREATE INDEX "I_AssignmentRoleQualificationRequirement_tenantGuid" ON "Scheduler"."AssignmentRoleQualificationRequirement" ("tenantGuid")
;

-- Index on the AssignmentRoleQualificationRequirement table's tenantGuid,assignmentRoleId fields.
CREATE INDEX "I_AssignmentRoleQualificationRequirement_tenantGuid_assignmentR" ON "Scheduler"."AssignmentRoleQualificationRequirement" ("tenantGuid", "assignmentRoleId")
;

-- Index on the AssignmentRoleQualificationRequirement table's tenantGuid,qualificationId fields.
CREATE INDEX "I_AssignmentRoleQualificationRequirement_tenantGuid_qualificati" ON "Scheduler"."AssignmentRoleQualificationRequirement" ("tenantGuid", "qualificationId")
;

-- Index on the AssignmentRoleQualificationRequirement table's tenantGuid,active fields.
CREATE INDEX "I_AssignmentRoleQualificationRequirement_tenantGuid_active" ON "Scheduler"."AssignmentRoleQualificationRequirement" ("tenantGuid", "active")
;

-- Index on the AssignmentRoleQualificationRequirement table's tenantGuid,deleted fields.
CREATE INDEX "I_AssignmentRoleQualificationRequirement_tenantGuid_deleted" ON "Scheduler"."AssignmentRoleQualificationRequirement" ("tenantGuid", "deleted")
;


-- The change history for records from the AssignmentRoleQualificationRequirement table.
CREATE TABLE "Scheduler"."AssignmentRoleQualificationRequirementChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"assignmentRoleQualificationRequirementId" INT NOT NULL,		-- Link to the AssignmentRoleQualificationRequirement table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "assignmentRoleQualificationRequirementId" FOREIGN KEY ("assignmentRoleQualificationRequirementId") REFERENCES "Scheduler"."AssignmentRoleQualificationRequirement"("id")		-- Foreign key to the AssignmentRoleQualificationRequirement table.
);
-- Index on the AssignmentRoleQualificationRequirementChangeHistory table's tenantGuid field.
CREATE INDEX "I_AssignmentRoleQualificationRequirementChangeHistory_tenantGui" ON "Scheduler"."AssignmentRoleQualificationRequirementChangeHistory" ("tenantGuid")
;

-- Index on the AssignmentRoleQualificationRequirementChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_AssignmentRoleQualificationRequirementChangeHistory_tenantGui" ON "Scheduler"."AssignmentRoleQualificationRequirementChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the AssignmentRoleQualificationRequirementChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_AssignmentRoleQualificationRequirementChangeHistory_tenantGui" ON "Scheduler"."AssignmentRoleQualificationRequirementChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the AssignmentRoleQualificationRequirementChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_AssignmentRoleQualificationRequirementChangeHistory_tenantGui" ON "Scheduler"."AssignmentRoleQualificationRequirementChangeHistory" ("tenantGuid", "userId")
;

-- Index on the AssignmentRoleQualificationRequirementChangeHistory table's tenantGuid,assignmentRoleQualificationRequirementId fields.
CREATE INDEX "I_AssignmentRoleQualificationRequirementChangeHistory_tenantGui" ON "Scheduler"."AssignmentRoleQualificationRequirementChangeHistory" ("tenantGuid", "assignmentRoleQualificationRequirementId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Master list of event statuses (Planned, In Progress, Completed, No-Show, Canceled, etc.)
CREATE TABLE "Scheduler"."EventStatus"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the EventStatus table's name field.
CREATE INDEX "I_EventStatus_name" ON "Scheduler"."EventStatus" ("name")
;

-- Index on the EventStatus table's active field.
CREATE INDEX "I_EventStatus_active" ON "Scheduler"."EventStatus" ("active")
;

-- Index on the EventStatus table's deleted field.
CREATE INDEX "I_EventStatus_deleted" ON "Scheduler"."EventStatus" ("deleted")
;

INSERT INTO "Scheduler"."EventStatus" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Planned', 'Scheduled but not started', 1, '005bdc39-da8e-465a-a17e-78aafffb390a' );

INSERT INTO "Scheduler"."EventStatus" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'In Progress', 'Started', 2, '513bd381-6ab9-407c-ac4d-9187f6f92e16' );

INSERT INTO "Scheduler"."EventStatus" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Completed', 'Finished successfully', 3, '6af9e244-2eff-463b-a40c-821fe00fa644' );

INSERT INTO "Scheduler"."EventStatus" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'No-Show', 'No Show', 4, 'd7e81b73-bbe3-42dd-bcf6-856a82b9fce1' );

INSERT INTO "Scheduler"."EventStatus" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Canceled', 'Explicitly canceled', 5, '01148ccb-e746-4218-88c5-8f0a5ee36adc' );


-- Master list of payment types (credit card, cheque, cash, e-transfer, etc.)
CREATE TABLE "Scheduler"."PaymentType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_PaymentType_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the PaymentType table's tenantGuid and name fields.
);
-- Index on the PaymentType table's tenantGuid field.
CREATE INDEX "I_PaymentType_tenantGuid" ON "Scheduler"."PaymentType" ("tenantGuid")
;

-- Index on the PaymentType table's tenantGuid,name fields.
CREATE INDEX "I_PaymentType_tenantGuid_name" ON "Scheduler"."PaymentType" ("tenantGuid", "name")
;

-- Index on the PaymentType table's tenantGuid,active fields.
CREATE INDEX "I_PaymentType_tenantGuid_active" ON "Scheduler"."PaymentType" ("tenantGuid", "active")
;

-- Index on the PaymentType table's tenantGuid,deleted fields.
CREATE INDEX "I_PaymentType_tenantGuid_deleted" ON "Scheduler"."PaymentType" ("tenantGuid", "deleted")
;


-- The change history for records from the PaymentType table.
CREATE TABLE "Scheduler"."PaymentTypeChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"paymentTypeId" INT NOT NULL,		-- Link to the PaymentType table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "paymentTypeId" FOREIGN KEY ("paymentTypeId") REFERENCES "Scheduler"."PaymentType"("id")		-- Foreign key to the PaymentType table.
);
-- Index on the PaymentTypeChangeHistory table's tenantGuid field.
CREATE INDEX "I_PaymentTypeChangeHistory_tenantGuid" ON "Scheduler"."PaymentTypeChangeHistory" ("tenantGuid")
;

-- Index on the PaymentTypeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_PaymentTypeChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."PaymentTypeChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the PaymentTypeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_PaymentTypeChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."PaymentTypeChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the PaymentTypeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_PaymentTypeChangeHistory_tenantGuid_userId" ON "Scheduler"."PaymentTypeChangeHistory" ("tenantGuid", "userId")
;

-- Index on the PaymentTypeChangeHistory table's tenantGuid,paymentTypeId fields.
CREATE INDEX "I_PaymentTypeChangeHistory_tenantGuid_paymentTypeId" ON "Scheduler"."PaymentTypeChangeHistory" ("tenantGuid", "paymentTypeId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Master list of receipt types (Receipted, Do Not Receipt, etc.)
CREATE TABLE "Scheduler"."ReceiptType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_ReceiptType_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the ReceiptType table's tenantGuid and name fields.
);
-- Index on the ReceiptType table's tenantGuid field.
CREATE INDEX "I_ReceiptType_tenantGuid" ON "Scheduler"."ReceiptType" ("tenantGuid")
;

-- Index on the ReceiptType table's tenantGuid,name fields.
CREATE INDEX "I_ReceiptType_tenantGuid_name" ON "Scheduler"."ReceiptType" ("tenantGuid", "name")
;

-- Index on the ReceiptType table's tenantGuid,active fields.
CREATE INDEX "I_ReceiptType_tenantGuid_active" ON "Scheduler"."ReceiptType" ("tenantGuid", "active")
;

-- Index on the ReceiptType table's tenantGuid,deleted fields.
CREATE INDEX "I_ReceiptType_tenantGuid_deleted" ON "Scheduler"."ReceiptType" ("tenantGuid", "deleted")
;


-- The change history for records from the ReceiptType table.
CREATE TABLE "Scheduler"."ReceiptTypeChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"receiptTypeId" INT NOT NULL,		-- Link to the ReceiptType table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "receiptTypeId" FOREIGN KEY ("receiptTypeId") REFERENCES "Scheduler"."ReceiptType"("id")		-- Foreign key to the ReceiptType table.
);
-- Index on the ReceiptTypeChangeHistory table's tenantGuid field.
CREATE INDEX "I_ReceiptTypeChangeHistory_tenantGuid" ON "Scheduler"."ReceiptTypeChangeHistory" ("tenantGuid")
;

-- Index on the ReceiptTypeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ReceiptTypeChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ReceiptTypeChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ReceiptTypeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ReceiptTypeChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ReceiptTypeChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ReceiptTypeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ReceiptTypeChangeHistory_tenantGuid_userId" ON "Scheduler"."ReceiptTypeChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ReceiptTypeChangeHistory table's tenantGuid,receiptTypeId fields.
CREATE INDEX "I_ReceiptTypeChangeHistory_tenantGuid_receiptTypeId" ON "Scheduler"."ReceiptTypeChangeHistory" ("tenantGuid", "receiptTypeId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Master list of booking sources ( walk-in, phone, online)
CREATE TABLE "Scheduler"."BookingSourceType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the BookingSourceType table's name field.
CREATE INDEX "I_BookingSourceType_name" ON "Scheduler"."BookingSourceType" ("name")
;

-- Index on the BookingSourceType table's active field.
CREATE INDEX "I_BookingSourceType_active" ON "Scheduler"."BookingSourceType" ("active")
;

-- Index on the BookingSourceType table's deleted field.
CREATE INDEX "I_BookingSourceType_deleted" ON "Scheduler"."BookingSourceType" ("deleted")
;

INSERT INTO "Scheduler"."BookingSourceType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Administrative', 'Administrative', 1, '3ec3e46a-ece8-4364-8396-beaf23aa0a2a' );

INSERT INTO "Scheduler"."BookingSourceType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Phone', 'Phone', 2, 'cb9c2d46-29d5-4caa-9d5c-9e84356edf86' );

INSERT INTO "Scheduler"."BookingSourceType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Walk-in', 'Walk-in', 3, 'fc0a5ebf-794d-4e61-9dce-f308da9d9ba4' );

INSERT INTO "Scheduler"."BookingSourceType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Online', 'Online', 4, '1955a3f1-adce-4bc4-99d1-86362ff98a57' );


-- Master list of assignment statuses (Planned, In Progress, Completed, No-Show, Canceled, etc.)
CREATE TABLE "Scheduler"."AssignmentStatus"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the AssignmentStatus table's name field.
CREATE INDEX "I_AssignmentStatus_name" ON "Scheduler"."AssignmentStatus" ("name")
;

-- Index on the AssignmentStatus table's active field.
CREATE INDEX "I_AssignmentStatus_active" ON "Scheduler"."AssignmentStatus" ("active")
;

-- Index on the AssignmentStatus table's deleted field.
CREATE INDEX "I_AssignmentStatus_deleted" ON "Scheduler"."AssignmentStatus" ("deleted")
;

INSERT INTO "Scheduler"."AssignmentStatus" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Planned', 'Scheduled but not started', 1, '82fff66d-f6b4-44fe-9892-c7415cd0d401' );

INSERT INTO "Scheduler"."AssignmentStatus" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'In Progress', 'Started', 2, '34183a16-1a64-4106-b28e-db454b06b5a6' );

INSERT INTO "Scheduler"."AssignmentStatus" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Completed', 'Finished successfully', 3, '765c3c6d-782b-4393-bdab-cbf2a4a34eb6' );

INSERT INTO "Scheduler"."AssignmentStatus" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'No-Show', 'Patient/resource didn''t appear', 4, '121271a6-7d93-4460-909f-2dc6e618538f' );

INSERT INTO "Scheduler"."AssignmentStatus" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Canceled', 'Explicitly canceled', 5, 'cb14a7ad-fe10-4b2b-996c-7b5598810608' );


-- Master list of scheduling target categories (e.g., Project, Patient, Customer). Used for UI grouping and filtering.
CREATE TABLE "Scheduler"."SchedulingTargetType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_SchedulingTargetType_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the SchedulingTargetType table's tenantGuid and name fields.
);
-- Index on the SchedulingTargetType table's tenantGuid field.
CREATE INDEX "I_SchedulingTargetType_tenantGuid" ON "Scheduler"."SchedulingTargetType" ("tenantGuid")
;

-- Index on the SchedulingTargetType table's tenantGuid,name fields.
CREATE INDEX "I_SchedulingTargetType_tenantGuid_name" ON "Scheduler"."SchedulingTargetType" ("tenantGuid", "name")
;

-- Index on the SchedulingTargetType table's tenantGuid,iconId fields.
CREATE INDEX "I_SchedulingTargetType_tenantGuid_iconId" ON "Scheduler"."SchedulingTargetType" ("tenantGuid", "iconId")
;

-- Index on the SchedulingTargetType table's tenantGuid,active fields.
CREATE INDEX "I_SchedulingTargetType_tenantGuid_active" ON "Scheduler"."SchedulingTargetType" ("tenantGuid", "active")
;

-- Index on the SchedulingTargetType table's tenantGuid,deleted fields.
CREATE INDEX "I_SchedulingTargetType_tenantGuid_deleted" ON "Scheduler"."SchedulingTargetType" ("tenantGuid", "deleted")
;

INSERT INTO "Scheduler"."SchedulingTargetType" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Construction Project', 'A construction job with one or more sites', 1, '0ceaf00d-c58f-48a6-a18e-9a3e07452a23' );

INSERT INTO "Scheduler"."SchedulingTargetType" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Patient', 'Healthcare patient with multiple care locations', 2, '7e14d7a8-f13d-4524-a679-6cbae24d9d97' );

INSERT INTO "Scheduler"."SchedulingTargetType" ( "tenantGuid", "name", "description", "sequence", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Service Customer', 'Field service customer with multiple service addresses', 3, '6b3aa295-a54b-45dd-bda5-d75d157f376c' );


-- The core container that ScheduledEvents are scheduled into.   Examples: a construction project, a healthcare patient, a service customer.  Supports multiple addresses and recurring scheduling patterns.
CREATE TABLE "Scheduler"."SchedulingTarget"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"officeId" INT NULL,		-- Optional office binding for a scheduling target.
	"clientId" INT NOT NULL,		-- The client that this scheduling target belongs to.
	"schedulingTargetTypeId" INT NOT NULL,		-- Link to the SchedulingTargetType table.
	"timeZoneId" INT NOT NULL,		-- Link to the TimeZone table.
	"calendarId" INT NULL,		-- An optional default calendar for this scheduling target.
	"notes" TEXT NULL,
	"externalId" VARCHAR(100) NULL,		-- Optional reference to an ID in an external system
	"color" VARCHAR(10) NULL,		-- Override of Target Type Hex color for UI display
	"attributes" TEXT NULL,		-- to store arbitrary JSON
	"avatarFileName" VARCHAR(250) NULL,		-- Part of the binary data field setup
	"avatarSize" BIGINT NULL,		-- Part of the binary data field setup
	"avatarData" BYTEA NULL,		-- Part of the binary data field setup
	"avatarMimeType" VARCHAR(100) NULL,		-- Part of the binary data field setup
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "officeId" FOREIGN KEY ("officeId") REFERENCES "Scheduler"."Office"("id"),		-- Foreign key to the Office table.
	CONSTRAINT "clientId" FOREIGN KEY ("clientId") REFERENCES "Scheduler"."Client"("id"),		-- Foreign key to the Client table.
	CONSTRAINT "schedulingTargetTypeId" FOREIGN KEY ("schedulingTargetTypeId") REFERENCES "Scheduler"."SchedulingTargetType"("id"),		-- Foreign key to the SchedulingTargetType table.
	CONSTRAINT "timeZoneId" FOREIGN KEY ("timeZoneId") REFERENCES "Scheduler"."TimeZone"("id"),		-- Foreign key to the TimeZone table.
	CONSTRAINT "calendarId" FOREIGN KEY ("calendarId") REFERENCES "Scheduler"."Calendar"("id"),		-- Foreign key to the Calendar table.
	CONSTRAINT "UC_SchedulingTarget_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the SchedulingTarget table's tenantGuid and name fields.
);
-- Index on the SchedulingTarget table's tenantGuid field.
CREATE INDEX "I_SchedulingTarget_tenantGuid" ON "Scheduler"."SchedulingTarget" ("tenantGuid")
;

-- Index on the SchedulingTarget table's tenantGuid,name fields.
CREATE INDEX "I_SchedulingTarget_tenantGuid_name" ON "Scheduler"."SchedulingTarget" ("tenantGuid", "name")
;

-- Index on the SchedulingTarget table's tenantGuid,officeId fields.
CREATE INDEX "I_SchedulingTarget_tenantGuid_officeId" ON "Scheduler"."SchedulingTarget" ("tenantGuid", "officeId")
;

-- Index on the SchedulingTarget table's tenantGuid,clientId fields.
CREATE INDEX "I_SchedulingTarget_tenantGuid_clientId" ON "Scheduler"."SchedulingTarget" ("tenantGuid", "clientId")
;

-- Index on the SchedulingTarget table's tenantGuid,schedulingTargetTypeId fields.
CREATE INDEX "I_SchedulingTarget_tenantGuid_schedulingTargetTypeId" ON "Scheduler"."SchedulingTarget" ("tenantGuid", "schedulingTargetTypeId")
;

-- Index on the SchedulingTarget table's tenantGuid,timeZoneId fields.
CREATE INDEX "I_SchedulingTarget_tenantGuid_timeZoneId" ON "Scheduler"."SchedulingTarget" ("tenantGuid", "timeZoneId")
;

-- Index on the SchedulingTarget table's tenantGuid,active fields.
CREATE INDEX "I_SchedulingTarget_tenantGuid_active" ON "Scheduler"."SchedulingTarget" ("tenantGuid", "active")
;

-- Index on the SchedulingTarget table's tenantGuid,deleted fields.
CREATE INDEX "I_SchedulingTarget_tenantGuid_deleted" ON "Scheduler"."SchedulingTarget" ("tenantGuid", "deleted")
;


-- The change history for records from the SchedulingTarget table.
CREATE TABLE "Scheduler"."SchedulingTargetChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"schedulingTargetId" INT NOT NULL,		-- Link to the SchedulingTarget table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "schedulingTargetId" FOREIGN KEY ("schedulingTargetId") REFERENCES "Scheduler"."SchedulingTarget"("id")		-- Foreign key to the SchedulingTarget table.
);
-- Index on the SchedulingTargetChangeHistory table's tenantGuid field.
CREATE INDEX "I_SchedulingTargetChangeHistory_tenantGuid" ON "Scheduler"."SchedulingTargetChangeHistory" ("tenantGuid")
;

-- Index on the SchedulingTargetChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_SchedulingTargetChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."SchedulingTargetChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the SchedulingTargetChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_SchedulingTargetChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."SchedulingTargetChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the SchedulingTargetChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_SchedulingTargetChangeHistory_tenantGuid_userId" ON "Scheduler"."SchedulingTargetChangeHistory" ("tenantGuid", "userId")
;

-- Index on the SchedulingTargetChangeHistory table's tenantGuid,schedulingTargetId fields.
CREATE INDEX "I_SchedulingTargetChangeHistory_tenantGuid_schedulingTargetId" ON "Scheduler"."SchedulingTargetChangeHistory" ("tenantGuid", "schedulingTargetId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- The link between scheduling targets and contacts.
CREATE TABLE "Scheduler"."SchedulingTargetContact"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"schedulingTargetId" INT NOT NULL,		-- Link to the SchedulingTarget table.
	"contactId" INT NOT NULL,		-- Link to the Contact table.
	"isPrimary" BOOLEAN NOT NULL DEFAULT false,		-- Indicates whether or not this contact should be considered a primary contact of the scheduling target.
	"relationshipTypeId" INT NOT NULL,		-- A description of the relationship between the scheduling target and the contact.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "schedulingTargetId" FOREIGN KEY ("schedulingTargetId") REFERENCES "Scheduler"."SchedulingTarget"("id"),		-- Foreign key to the SchedulingTarget table.
	CONSTRAINT "contactId" FOREIGN KEY ("contactId") REFERENCES "Scheduler"."Contact"("id"),		-- Foreign key to the Contact table.
	CONSTRAINT "relationshipTypeId" FOREIGN KEY ("relationshipTypeId") REFERENCES "Scheduler"."RelationshipType"("id"),		-- Foreign key to the RelationshipType table.
	CONSTRAINT "UC_SchedulingTargetContact_tenantGuid_schedulingTargetId_contactId" UNIQUE ( "tenantGuid", "schedulingTargetId", "contactId") 		-- Uniqueness enforced on the SchedulingTargetContact table's tenantGuid and schedulingTargetId and contactId fields.
);
-- Index on the SchedulingTargetContact table's tenantGuid field.
CREATE INDEX "I_SchedulingTargetContact_tenantGuid" ON "Scheduler"."SchedulingTargetContact" ("tenantGuid")
;

-- Index on the SchedulingTargetContact table's tenantGuid,schedulingTargetId fields.
CREATE INDEX "I_SchedulingTargetContact_tenantGuid_schedulingTargetId" ON "Scheduler"."SchedulingTargetContact" ("tenantGuid", "schedulingTargetId")
;

-- Index on the SchedulingTargetContact table's tenantGuid,contactId fields.
CREATE INDEX "I_SchedulingTargetContact_tenantGuid_contactId" ON "Scheduler"."SchedulingTargetContact" ("tenantGuid", "contactId")
;

-- Index on the SchedulingTargetContact table's tenantGuid,relationshipTypeId fields.
CREATE INDEX "I_SchedulingTargetContact_tenantGuid_relationshipTypeId" ON "Scheduler"."SchedulingTargetContact" ("tenantGuid", "relationshipTypeId")
;

-- Index on the SchedulingTargetContact table's tenantGuid,active fields.
CREATE INDEX "I_SchedulingTargetContact_tenantGuid_active" ON "Scheduler"."SchedulingTargetContact" ("tenantGuid", "active")
;

-- Index on the SchedulingTargetContact table's tenantGuid,deleted fields.
CREATE INDEX "I_SchedulingTargetContact_tenantGuid_deleted" ON "Scheduler"."SchedulingTargetContact" ("tenantGuid", "deleted")
;


-- The change history for records from the SchedulingTargetContact table.
CREATE TABLE "Scheduler"."SchedulingTargetContactChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"schedulingTargetContactId" INT NOT NULL,		-- Link to the SchedulingTargetContact table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "schedulingTargetContactId" FOREIGN KEY ("schedulingTargetContactId") REFERENCES "Scheduler"."SchedulingTargetContact"("id")		-- Foreign key to the SchedulingTargetContact table.
);
-- Index on the SchedulingTargetContactChangeHistory table's tenantGuid field.
CREATE INDEX "I_SchedulingTargetContactChangeHistory_tenantGuid" ON "Scheduler"."SchedulingTargetContactChangeHistory" ("tenantGuid")
;

-- Index on the SchedulingTargetContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_SchedulingTargetContactChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."SchedulingTargetContactChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the SchedulingTargetContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_SchedulingTargetContactChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."SchedulingTargetContactChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the SchedulingTargetContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_SchedulingTargetContactChangeHistory_tenantGuid_userId" ON "Scheduler"."SchedulingTargetContactChangeHistory" ("tenantGuid", "userId")
;

-- Index on the SchedulingTargetContactChangeHistory table's tenantGuid,schedulingTargetContactId fields.
CREATE INDEX "I_SchedulingTargetContactChangeHistory_tenantGuid_schedulingTar" ON "Scheduler"."SchedulingTargetContactChangeHistory" ("tenantGuid", "schedulingTargetContactId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Links SchedulingTargets to multiple addresses (e.g., multiple job sites, patient home + hospital).
CREATE TABLE "Scheduler"."SchedulingTargetAddress"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"schedulingTargetId" INT NOT NULL,		-- Primary  schuduling target for this address - could be null if there is a client linked to this, so the address would be for all targets in the client.
	"clientId" INT NULL,		-- Optional client level link.  The presence of a value here indicates that the address is to be shared across all scheduling targets for the client.
	"addressLine1" VARCHAR(250) NOT NULL,
	"addressLine2" VARCHAR(250) NULL,
	"city" VARCHAR(100) NOT NULL,
	"postalCode" VARCHAR(100) NULL,
	"stateProvinceId" INT NOT NULL,		-- Link to the StateProvince table.
	"countryId" INT NOT NULL,		-- Link to the Country table.
	"latitude" DOUBLE PRECISION NULL,		-- Optional latitude position
	"longitude" DOUBLE PRECISION NULL,		-- Optional longitude position
	"label" VARCHAR(250) NULL,		-- e.g., 'Main Site', 'Patient Home', 'Hospital Ward'
	"isPrimary" BOOLEAN NOT NULL DEFAULT false,		-- Whether or not this is the scheduling target's main address.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "schedulingTargetId" FOREIGN KEY ("schedulingTargetId") REFERENCES "Scheduler"."SchedulingTarget"("id"),		-- Foreign key to the SchedulingTarget table.
	CONSTRAINT "clientId" FOREIGN KEY ("clientId") REFERENCES "Scheduler"."Client"("id"),		-- Foreign key to the Client table.
	CONSTRAINT "stateProvinceId" FOREIGN KEY ("stateProvinceId") REFERENCES "Scheduler"."StateProvince"("id"),		-- Foreign key to the StateProvince table.
	CONSTRAINT "countryId" FOREIGN KEY ("countryId") REFERENCES "Scheduler"."Country"("id"),		-- Foreign key to the Country table.
	CONSTRAINT "UC_SchedulingTargetAddress_tenantGuid_schedulingTargetId_addressLine1_city_postalCode" UNIQUE ( "tenantGuid", "schedulingTargetId", "addressLine1", "city", "postalCode") 		-- Uniqueness enforced on the SchedulingTargetAddress table's tenantGuid and schedulingTargetId and addressLine1 and city and postalCode fields.
);
-- Index on the SchedulingTargetAddress table's tenantGuid field.
CREATE INDEX "I_SchedulingTargetAddress_tenantGuid" ON "Scheduler"."SchedulingTargetAddress" ("tenantGuid")
;

-- Index on the SchedulingTargetAddress table's tenantGuid,schedulingTargetId fields.
CREATE INDEX "I_SchedulingTargetAddress_tenantGuid_schedulingTargetId" ON "Scheduler"."SchedulingTargetAddress" ("tenantGuid", "schedulingTargetId")
;

-- Index on the SchedulingTargetAddress table's tenantGuid,clientId fields.
CREATE INDEX "I_SchedulingTargetAddress_tenantGuid_clientId" ON "Scheduler"."SchedulingTargetAddress" ("tenantGuid", "clientId")
;

-- Index on the SchedulingTargetAddress table's tenantGuid,stateProvinceId fields.
CREATE INDEX "I_SchedulingTargetAddress_tenantGuid_stateProvinceId" ON "Scheduler"."SchedulingTargetAddress" ("tenantGuid", "stateProvinceId")
;

-- Index on the SchedulingTargetAddress table's tenantGuid,countryId fields.
CREATE INDEX "I_SchedulingTargetAddress_tenantGuid_countryId" ON "Scheduler"."SchedulingTargetAddress" ("tenantGuid", "countryId")
;

-- Index on the SchedulingTargetAddress table's tenantGuid,active fields.
CREATE INDEX "I_SchedulingTargetAddress_tenantGuid_active" ON "Scheduler"."SchedulingTargetAddress" ("tenantGuid", "active")
;

-- Index on the SchedulingTargetAddress table's tenantGuid,deleted fields.
CREATE INDEX "I_SchedulingTargetAddress_tenantGuid_deleted" ON "Scheduler"."SchedulingTargetAddress" ("tenantGuid", "deleted")
;


-- The change history for records from the SchedulingTargetAddress table.
CREATE TABLE "Scheduler"."SchedulingTargetAddressChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"schedulingTargetAddressId" INT NOT NULL,		-- Link to the SchedulingTargetAddress table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "schedulingTargetAddressId" FOREIGN KEY ("schedulingTargetAddressId") REFERENCES "Scheduler"."SchedulingTargetAddress"("id")		-- Foreign key to the SchedulingTargetAddress table.
);
-- Index on the SchedulingTargetAddressChangeHistory table's tenantGuid field.
CREATE INDEX "I_SchedulingTargetAddressChangeHistory_tenantGuid" ON "Scheduler"."SchedulingTargetAddressChangeHistory" ("tenantGuid")
;

-- Index on the SchedulingTargetAddressChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_SchedulingTargetAddressChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."SchedulingTargetAddressChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the SchedulingTargetAddressChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_SchedulingTargetAddressChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."SchedulingTargetAddressChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the SchedulingTargetAddressChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_SchedulingTargetAddressChangeHistory_tenantGuid_userId" ON "Scheduler"."SchedulingTargetAddressChangeHistory" ("tenantGuid", "userId")
;

-- Index on the SchedulingTargetAddressChangeHistory table's tenantGuid,schedulingTargetAddressId fields.
CREATE INDEX "I_SchedulingTargetAddressChangeHistory_tenantGuid_schedulingTar" ON "Scheduler"."SchedulingTargetAddressChangeHistory" ("tenantGuid", "schedulingTargetAddressId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Defines which qualifications are required (or preferred) for working on a specific SchedulingTarget.  - isRequired = true then resource MUST have qualification  - isRequired = false then nice-to-have (warning only)
CREATE TABLE "Scheduler"."SchedulingTargetQualificationRequirement"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"schedulingTargetId" INT NOT NULL,		-- Link to the SchedulingTarget table.
	"qualificationId" INT NOT NULL,		-- Link to the Qualification table.
	"isRequired" BOOLEAN NOT NULL DEFAULT true,		-- true = mandatory, false = preferred
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "schedulingTargetId" FOREIGN KEY ("schedulingTargetId") REFERENCES "Scheduler"."SchedulingTarget"("id"),		-- Foreign key to the SchedulingTarget table.
	CONSTRAINT "qualificationId" FOREIGN KEY ("qualificationId") REFERENCES "Scheduler"."Qualification"("id"),		-- Foreign key to the Qualification table.
	CONSTRAINT "UC_SchedulingTargetQualificationRequirement_tenantGuid_schedulingTargetId_qualificationId" UNIQUE ( "tenantGuid", "schedulingTargetId", "qualificationId") 		-- Uniqueness enforced on the SchedulingTargetQualificationRequirement table's tenantGuid and schedulingTargetId and qualificationId fields.
);
-- Index on the SchedulingTargetQualificationRequirement table's tenantGuid field.
CREATE INDEX "I_SchedulingTargetQualificationRequirement_tenantGuid" ON "Scheduler"."SchedulingTargetQualificationRequirement" ("tenantGuid")
;

-- Index on the SchedulingTargetQualificationRequirement table's tenantGuid,schedulingTargetId fields.
CREATE INDEX "I_SchedulingTargetQualificationRequirement_tenantGuid_schedulin" ON "Scheduler"."SchedulingTargetQualificationRequirement" ("tenantGuid", "schedulingTargetId")
;

-- Index on the SchedulingTargetQualificationRequirement table's tenantGuid,qualificationId fields.
CREATE INDEX "I_SchedulingTargetQualificationRequirement_tenantGuid_qualifica" ON "Scheduler"."SchedulingTargetQualificationRequirement" ("tenantGuid", "qualificationId")
;

-- Index on the SchedulingTargetQualificationRequirement table's tenantGuid,active fields.
CREATE INDEX "I_SchedulingTargetQualificationRequirement_tenantGuid_active" ON "Scheduler"."SchedulingTargetQualificationRequirement" ("tenantGuid", "active")
;

-- Index on the SchedulingTargetQualificationRequirement table's tenantGuid,deleted fields.
CREATE INDEX "I_SchedulingTargetQualificationRequirement_tenantGuid_deleted" ON "Scheduler"."SchedulingTargetQualificationRequirement" ("tenantGuid", "deleted")
;


-- The change history for records from the SchedulingTargetQualificationRequirement table.
CREATE TABLE "Scheduler"."SchedulingTargetQualificationRequirementChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"schedulingTargetQualificationRequirementId" INT NOT NULL,		-- Link to the SchedulingTargetQualificationRequirement table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "schedulingTargetQualificationRequirementId" FOREIGN KEY ("schedulingTargetQualificationRequirementId") REFERENCES "Scheduler"."SchedulingTargetQualificationRequirement"("id")		-- Foreign key to the SchedulingTargetQualificationRequirement table.
);
-- Index on the SchedulingTargetQualificationRequirementChangeHistory table's tenantGuid field.
CREATE INDEX "I_SchedulingTargetQualificationRequirementChangeHistory_tenantG" ON "Scheduler"."SchedulingTargetQualificationRequirementChangeHistory" ("tenantGuid")
;

-- Index on the SchedulingTargetQualificationRequirementChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_SchedulingTargetQualificationRequirementChangeHistory_tenantG" ON "Scheduler"."SchedulingTargetQualificationRequirementChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the SchedulingTargetQualificationRequirementChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_SchedulingTargetQualificationRequirementChangeHistory_tenantG" ON "Scheduler"."SchedulingTargetQualificationRequirementChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the SchedulingTargetQualificationRequirementChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_SchedulingTargetQualificationRequirementChangeHistory_tenantG" ON "Scheduler"."SchedulingTargetQualificationRequirementChangeHistory" ("tenantGuid", "userId")
;

-- Index on the SchedulingTargetQualificationRequirementChangeHistory table's tenantGuid,schedulingTargetQualificationRequirementId fields.
CREATE INDEX "I_SchedulingTargetQualificationRequirementChangeHistory_tenantG" ON "Scheduler"."SchedulingTargetQualificationRequirementChangeHistory" ("tenantGuid", "schedulingTargetQualificationRequirementId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Master list of recurrence frequencies. Mirrors common iCalendar frequencies.
CREATE TABLE "Scheduler"."RecurrenceFrequency"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the RecurrenceFrequency table's name field.
CREATE INDEX "I_RecurrenceFrequency_name" ON "Scheduler"."RecurrenceFrequency" ("name")
;

-- Index on the RecurrenceFrequency table's active field.
CREATE INDEX "I_RecurrenceFrequency_active" ON "Scheduler"."RecurrenceFrequency" ("active")
;

-- Index on the RecurrenceFrequency table's deleted field.
CREATE INDEX "I_RecurrenceFrequency_deleted" ON "Scheduler"."RecurrenceFrequency" ("deleted")
;

INSERT INTO "Scheduler"."RecurrenceFrequency" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Once', 'Does not repeat', 1, 'a2e0f727-8e79-4add-af0a-495e89a4c6b7' );

INSERT INTO "Scheduler"."RecurrenceFrequency" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Daily', 'Repeats every day or every N days', 2, 'bd28a0b1-26cf-4973-9129-bcd1cc5c9f67' );

INSERT INTO "Scheduler"."RecurrenceFrequency" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Weekly', 'Repeats every week on selected days', 3, '044f3c91-7745-467a-955a-809acdc0dba7' );

INSERT INTO "Scheduler"."RecurrenceFrequency" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Monthly', 'Repeats monthly (by day of month or day of week)', 4, 'fa0a9d3f-86e2-46c1-9a14-ea3858facf09' );

INSERT INTO "Scheduler"."RecurrenceFrequency" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Yearly', 'Repeats annually', 5, '3ffeb2e0-0ced-4fc2-a268-bb31a3f5a861' );


-- Defines a recurrence pattern for a ScheduledEvent.  One ScheduledEvent can have zero or one RecurrenceRule (for recurring series).  Instances are generated on-the-fly or materialized as needed.
CREATE TABLE "Scheduler"."RecurrenceRule"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"recurrenceFrequencyId" INT NOT NULL,		-- Link to the RecurrenceFrequency table.
	"interval" INT NOT NULL DEFAULT 1,		-- How often the pattern repeats (e.g., every 2 weeks)
	"untilDateTime" TIMESTAMP NULL,		-- Recurrence ends on this date (inclusive). NULL = no end date
	"count" INT NULL,		-- Maximum number of occurrences. NULL = unlimited
	"dayOfWeekMask" INT NULL DEFAULT 0,		-- Bitmask for weekly recurrence:  1 = Sunday, 2 = Monday, 4 = Tuesday, 8 = Wednesday, 16 = Thursday, 32 = Friday, 64 = Saturday Example: Monday + Wednesday + Friday = 2 + 8 + 32 = 42
	"dayOfMonth" INT NULL,		-- For monthly: specific day (1-31). NULL if using dayOfWeekInMonth
	"dayOfWeekInMonth" INT NULL,		-- Values: 1 = first, 2 = second, 3 = third, 4 = fourth, 5 = last, -1 = second-to-last, etc. Combine with dayOfWeekMask.  
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "recurrenceFrequencyId" FOREIGN KEY ("recurrenceFrequencyId") REFERENCES "Scheduler"."RecurrenceFrequency"("id")		-- Foreign key to the RecurrenceFrequency table.
);
-- Index on the RecurrenceRule table's tenantGuid field.
CREATE INDEX "I_RecurrenceRule_tenantGuid" ON "Scheduler"."RecurrenceRule" ("tenantGuid")
;

-- Index on the RecurrenceRule table's tenantGuid,recurrenceFrequencyId fields.
CREATE INDEX "I_RecurrenceRule_tenantGuid_recurrenceFrequencyId" ON "Scheduler"."RecurrenceRule" ("tenantGuid", "recurrenceFrequencyId")
;

-- Index on the RecurrenceRule table's tenantGuid,active fields.
CREATE INDEX "I_RecurrenceRule_tenantGuid_active" ON "Scheduler"."RecurrenceRule" ("tenantGuid", "active")
;

-- Index on the RecurrenceRule table's tenantGuid,deleted fields.
CREATE INDEX "I_RecurrenceRule_tenantGuid_deleted" ON "Scheduler"."RecurrenceRule" ("tenantGuid", "deleted")
;


-- The change history for records from the RecurrenceRule table.
CREATE TABLE "Scheduler"."RecurrenceRuleChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"recurrenceRuleId" INT NOT NULL,		-- Link to the RecurrenceRule table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "recurrenceRuleId" FOREIGN KEY ("recurrenceRuleId") REFERENCES "Scheduler"."RecurrenceRule"("id")		-- Foreign key to the RecurrenceRule table.
);
-- Index on the RecurrenceRuleChangeHistory table's tenantGuid field.
CREATE INDEX "I_RecurrenceRuleChangeHistory_tenantGuid" ON "Scheduler"."RecurrenceRuleChangeHistory" ("tenantGuid")
;

-- Index on the RecurrenceRuleChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_RecurrenceRuleChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."RecurrenceRuleChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the RecurrenceRuleChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_RecurrenceRuleChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."RecurrenceRuleChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the RecurrenceRuleChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_RecurrenceRuleChangeHistory_tenantGuid_userId" ON "Scheduler"."RecurrenceRuleChangeHistory" ("tenantGuid", "userId")
;

-- Index on the RecurrenceRuleChangeHistory table's tenantGuid,recurrenceRuleId fields.
CREATE INDEX "I_RecurrenceRuleChangeHistory_tenantGuid_recurrenceRuleId" ON "Scheduler"."RecurrenceRuleChangeHistory" ("tenantGuid", "recurrenceRuleId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Reusable standard shift patterns (e.g., 'Day Shift', 'Night Shift', 'Weekend Crew').  Resources can be assigned to a pattern, or have custom overrides.
CREATE TABLE "Scheduler"."ShiftPattern"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"timeZoneId" INT NULL,		-- Link to the TimeZone table.
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "timeZoneId" FOREIGN KEY ("timeZoneId") REFERENCES "Scheduler"."TimeZone"("id"),		-- Foreign key to the TimeZone table.
	CONSTRAINT "UC_ShiftPattern_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the ShiftPattern table's tenantGuid and name fields.
);
-- Index on the ShiftPattern table's tenantGuid field.
CREATE INDEX "I_ShiftPattern_tenantGuid" ON "Scheduler"."ShiftPattern" ("tenantGuid")
;

-- Index on the ShiftPattern table's tenantGuid,name fields.
CREATE INDEX "I_ShiftPattern_tenantGuid_name" ON "Scheduler"."ShiftPattern" ("tenantGuid", "name")
;

-- Index on the ShiftPattern table's tenantGuid,timeZoneId fields.
CREATE INDEX "I_ShiftPattern_tenantGuid_timeZoneId" ON "Scheduler"."ShiftPattern" ("tenantGuid", "timeZoneId")
;

-- Index on the ShiftPattern table's tenantGuid,active fields.
CREATE INDEX "I_ShiftPattern_tenantGuid_active" ON "Scheduler"."ShiftPattern" ("tenantGuid", "active")
;

-- Index on the ShiftPattern table's tenantGuid,deleted fields.
CREATE INDEX "I_ShiftPattern_tenantGuid_deleted" ON "Scheduler"."ShiftPattern" ("tenantGuid", "deleted")
;


-- The change history for records from the ShiftPattern table.
CREATE TABLE "Scheduler"."ShiftPatternChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"shiftPatternId" INT NOT NULL,		-- Link to the ShiftPattern table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "shiftPatternId" FOREIGN KEY ("shiftPatternId") REFERENCES "Scheduler"."ShiftPattern"("id")		-- Foreign key to the ShiftPattern table.
);
-- Index on the ShiftPatternChangeHistory table's tenantGuid field.
CREATE INDEX "I_ShiftPatternChangeHistory_tenantGuid" ON "Scheduler"."ShiftPatternChangeHistory" ("tenantGuid")
;

-- Index on the ShiftPatternChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ShiftPatternChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ShiftPatternChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ShiftPatternChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ShiftPatternChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ShiftPatternChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ShiftPatternChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ShiftPatternChangeHistory_tenantGuid_userId" ON "Scheduler"."ShiftPatternChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ShiftPatternChangeHistory table's tenantGuid,shiftPatternId fields.
CREATE INDEX "I_ShiftPatternChangeHistory_tenantGuid_shiftPatternId" ON "Scheduler"."ShiftPatternChangeHistory" ("tenantGuid", "shiftPatternId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Defines the days and availability windows for a ShiftPattern.
CREATE TABLE "Scheduler"."ShiftPatternDay"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"shiftPatternId" INT NOT NULL,		-- Link to the ShiftPattern table.
	"dayOfWeek" INT NOT NULL DEFAULT 1,		-- Day this rule applies to   1=Sunday..7=Saturday
	"startTime" TIME NOT NULL,		-- Start of available window (local to pattern time zone) e.g., 07:00:00
	"hours" REAL NOT NULL DEFAULT 8,		-- Hours available from start time (handles overnight shifts cleanly)  Defaults to 8
	"label" VARCHAR(250) NULL,		-- e.g., Main Shift
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "shiftPatternId" FOREIGN KEY ("shiftPatternId") REFERENCES "Scheduler"."ShiftPattern"("id"),		-- Foreign key to the ShiftPattern table.
	CONSTRAINT "UC_ShiftPatternDay_tenantGuid_shiftPatternId_dayOfWeek" UNIQUE ( "tenantGuid", "shiftPatternId", "dayOfWeek") 		-- Uniqueness enforced on the ShiftPatternDay table's tenantGuid and shiftPatternId and dayOfWeek fields.
);
-- Index on the ShiftPatternDay table's tenantGuid field.
CREATE INDEX "I_ShiftPatternDay_tenantGuid" ON "Scheduler"."ShiftPatternDay" ("tenantGuid")
;

-- Index on the ShiftPatternDay table's tenantGuid,shiftPatternId fields.
CREATE INDEX "I_ShiftPatternDay_tenantGuid_shiftPatternId" ON "Scheduler"."ShiftPatternDay" ("tenantGuid", "shiftPatternId")
;

-- Index on the ShiftPatternDay table's tenantGuid,active fields.
CREATE INDEX "I_ShiftPatternDay_tenantGuid_active" ON "Scheduler"."ShiftPatternDay" ("tenantGuid", "active")
;

-- Index on the ShiftPatternDay table's tenantGuid,deleted fields.
CREATE INDEX "I_ShiftPatternDay_tenantGuid_deleted" ON "Scheduler"."ShiftPatternDay" ("tenantGuid", "deleted")
;


-- The change history for records from the ShiftPatternDay table.
CREATE TABLE "Scheduler"."ShiftPatternDayChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"shiftPatternDayId" INT NOT NULL,		-- Link to the ShiftPatternDay table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "shiftPatternDayId" FOREIGN KEY ("shiftPatternDayId") REFERENCES "Scheduler"."ShiftPatternDay"("id")		-- Foreign key to the ShiftPatternDay table.
);
-- Index on the ShiftPatternDayChangeHistory table's tenantGuid field.
CREATE INDEX "I_ShiftPatternDayChangeHistory_tenantGuid" ON "Scheduler"."ShiftPatternDayChangeHistory" ("tenantGuid")
;

-- Index on the ShiftPatternDayChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ShiftPatternDayChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ShiftPatternDayChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ShiftPatternDayChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ShiftPatternDayChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ShiftPatternDayChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ShiftPatternDayChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ShiftPatternDayChangeHistory_tenantGuid_userId" ON "Scheduler"."ShiftPatternDayChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ShiftPatternDayChangeHistory table's tenantGuid,shiftPatternDayId fields.
CREATE INDEX "I_ShiftPatternDayChangeHistory_tenantGuid_shiftPatternDayId" ON "Scheduler"."ShiftPatternDayChangeHistory" ("tenantGuid", "shiftPatternDayId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- The schedulable entities – people and assets.  Examples: 'John Doe (Operator)', 'CAT CP56B Roller #12', 'Conference Room A'.
CREATE TABLE "Scheduler"."Resource"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"officeId" INT NULL,		-- Optional office binding for a resource.
	"resourceTypeId" INT NOT NULL,		-- Link to the ResourceType table.
	"shiftPatternId" INT NULL,		-- Standard shift pattern this resource follows (NULL = custom shifts via ResourceShift)
	"timeZoneId" INT NOT NULL,		-- Link to the TimeZone table.
	"targetWeeklyWorkHours" REAL NULL,
	"notes" TEXT NULL,
	"externalId" VARCHAR(100) NULL,		-- Optional reference to an ID in an external system
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"attributes" TEXT NULL,		-- to store arbitrary JSON
	"avatarFileName" VARCHAR(250) NULL,		-- Part of the binary data field setup
	"avatarSize" BIGINT NULL,		-- Part of the binary data field setup
	"avatarData" BYTEA NULL,		-- Part of the binary data field setup
	"avatarMimeType" VARCHAR(100) NULL,		-- Part of the binary data field setup
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "officeId" FOREIGN KEY ("officeId") REFERENCES "Scheduler"."Office"("id"),		-- Foreign key to the Office table.
	CONSTRAINT "resourceTypeId" FOREIGN KEY ("resourceTypeId") REFERENCES "Scheduler"."ResourceType"("id"),		-- Foreign key to the ResourceType table.
	CONSTRAINT "shiftPatternId" FOREIGN KEY ("shiftPatternId") REFERENCES "Scheduler"."ShiftPattern"("id"),		-- Foreign key to the ShiftPattern table.
	CONSTRAINT "timeZoneId" FOREIGN KEY ("timeZoneId") REFERENCES "Scheduler"."TimeZone"("id"),		-- Foreign key to the TimeZone table.
	CONSTRAINT "UC_Resource_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Resource table's tenantGuid and name fields.
);
-- Index on the Resource table's tenantGuid field.
CREATE INDEX "I_Resource_tenantGuid" ON "Scheduler"."Resource" ("tenantGuid")
;

-- Index on the Resource table's tenantGuid,name fields.
CREATE INDEX "I_Resource_tenantGuid_name" ON "Scheduler"."Resource" ("tenantGuid", "name")
;

-- Index on the Resource table's tenantGuid,officeId fields.
CREATE INDEX "I_Resource_tenantGuid_officeId" ON "Scheduler"."Resource" ("tenantGuid", "officeId")
;

-- Index on the Resource table's tenantGuid,resourceTypeId fields.
CREATE INDEX "I_Resource_tenantGuid_resourceTypeId" ON "Scheduler"."Resource" ("tenantGuid", "resourceTypeId")
;

-- Index on the Resource table's tenantGuid,shiftPatternId fields.
CREATE INDEX "I_Resource_tenantGuid_shiftPatternId" ON "Scheduler"."Resource" ("tenantGuid", "shiftPatternId")
;

-- Index on the Resource table's tenantGuid,timeZoneId fields.
CREATE INDEX "I_Resource_tenantGuid_timeZoneId" ON "Scheduler"."Resource" ("tenantGuid", "timeZoneId")
;

-- Index on the Resource table's tenantGuid,active fields.
CREATE INDEX "I_Resource_tenantGuid_active" ON "Scheduler"."Resource" ("tenantGuid", "active")
;

-- Index on the Resource table's tenantGuid,deleted fields.
CREATE INDEX "I_Resource_tenantGuid_deleted" ON "Scheduler"."Resource" ("tenantGuid", "deleted")
;

-- Index on the Resource table's tenantGuid,externalId fields.
CREATE INDEX "I_Resource_tenantGuid_externalId" ON "Scheduler"."Resource" ("tenantGuid", "externalId")
;


-- The change history for records from the Resource table.
CREATE TABLE "Scheduler"."ResourceChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"resourceId" INT NOT NULL,		-- Link to the Resource table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "resourceId" FOREIGN KEY ("resourceId") REFERENCES "Scheduler"."Resource"("id")		-- Foreign key to the Resource table.
);
-- Index on the ResourceChangeHistory table's tenantGuid field.
CREATE INDEX "I_ResourceChangeHistory_tenantGuid" ON "Scheduler"."ResourceChangeHistory" ("tenantGuid")
;

-- Index on the ResourceChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ResourceChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ResourceChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ResourceChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ResourceChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ResourceChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ResourceChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ResourceChangeHistory_tenantGuid_userId" ON "Scheduler"."ResourceChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ResourceChangeHistory table's tenantGuid,resourceId fields.
CREATE INDEX "I_ResourceChangeHistory_tenantGuid_resourceId" ON "Scheduler"."ResourceChangeHistory" ("tenantGuid", "resourceId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- The link between scheduling targets and contacts.
CREATE TABLE "Scheduler"."ResourceContact"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"resourceId" INT NOT NULL,		-- Link to the Resource table.
	"contactId" INT NOT NULL,		-- Link to the Contact table.
	"isPrimary" BOOLEAN NOT NULL DEFAULT false,		-- Indicates whether or not this contact should be considered a primary contact of the resource.
	"relationshipTypeId" INT NOT NULL,		-- A description of the relationship between the resource and the contact.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "resourceId" FOREIGN KEY ("resourceId") REFERENCES "Scheduler"."Resource"("id"),		-- Foreign key to the Resource table.
	CONSTRAINT "contactId" FOREIGN KEY ("contactId") REFERENCES "Scheduler"."Contact"("id"),		-- Foreign key to the Contact table.
	CONSTRAINT "relationshipTypeId" FOREIGN KEY ("relationshipTypeId") REFERENCES "Scheduler"."RelationshipType"("id"),		-- Foreign key to the RelationshipType table.
	CONSTRAINT "UC_ResourceContact_tenantGuid_resourceId_contactId" UNIQUE ( "tenantGuid", "resourceId", "contactId") 		-- Uniqueness enforced on the ResourceContact table's tenantGuid and resourceId and contactId fields.
);
-- Index on the ResourceContact table's tenantGuid field.
CREATE INDEX "I_ResourceContact_tenantGuid" ON "Scheduler"."ResourceContact" ("tenantGuid")
;

-- Index on the ResourceContact table's tenantGuid,resourceId fields.
CREATE INDEX "I_ResourceContact_tenantGuid_resourceId" ON "Scheduler"."ResourceContact" ("tenantGuid", "resourceId")
;

-- Index on the ResourceContact table's tenantGuid,contactId fields.
CREATE INDEX "I_ResourceContact_tenantGuid_contactId" ON "Scheduler"."ResourceContact" ("tenantGuid", "contactId")
;

-- Index on the ResourceContact table's tenantGuid,relationshipTypeId fields.
CREATE INDEX "I_ResourceContact_tenantGuid_relationshipTypeId" ON "Scheduler"."ResourceContact" ("tenantGuid", "relationshipTypeId")
;

-- Index on the ResourceContact table's tenantGuid,active fields.
CREATE INDEX "I_ResourceContact_tenantGuid_active" ON "Scheduler"."ResourceContact" ("tenantGuid", "active")
;

-- Index on the ResourceContact table's tenantGuid,deleted fields.
CREATE INDEX "I_ResourceContact_tenantGuid_deleted" ON "Scheduler"."ResourceContact" ("tenantGuid", "deleted")
;


-- The change history for records from the ResourceContact table.
CREATE TABLE "Scheduler"."ResourceContactChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"resourceContactId" INT NOT NULL,		-- Link to the ResourceContact table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "resourceContactId" FOREIGN KEY ("resourceContactId") REFERENCES "Scheduler"."ResourceContact"("id")		-- Foreign key to the ResourceContact table.
);
-- Index on the ResourceContactChangeHistory table's tenantGuid field.
CREATE INDEX "I_ResourceContactChangeHistory_tenantGuid" ON "Scheduler"."ResourceContactChangeHistory" ("tenantGuid")
;

-- Index on the ResourceContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ResourceContactChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ResourceContactChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ResourceContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ResourceContactChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ResourceContactChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ResourceContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ResourceContactChangeHistory_tenantGuid_userId" ON "Scheduler"."ResourceContactChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ResourceContactChangeHistory table's tenantGuid,resourceContactId fields.
CREATE INDEX "I_ResourceContactChangeHistory_tenantGuid_resourceContactId" ON "Scheduler"."ResourceContactChangeHistory" ("tenantGuid", "resourceContactId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
Master Rate Sheet. 
Replaces simple Resource-based rating with a hierarchical lookup system.
Hierarchy Logic (System should look for the first match in this order):
1. Specific Resource on Specific Project (schedulingTargetId + resourceId)
2. Specific Role on Specific Project (schedulingTargetId + assignmentRoleId)
3. Specific Resource Global Rate (resourceId)
4. Specific Role Global Rate (assignmentRoleId)
*/
CREATE TABLE "Scheduler"."RateSheet"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"officeId" INT NULL,		-- Optional office binding for a rate sheet.
	"assignmentRoleId" INT NULL,		-- Link to AssignmentRole. If populated, applies to anyone in this role.
	"resourceId" INT NULL,		-- Link to Resource. If populated, overrides the Role rate.
	"schedulingTargetId" INT NULL,		-- Link to SchedulingTarget. If populated, applies only to this project.
	"rateTypeId" INT NOT NULL,		-- e.g., 'Standard', 'Overtime', 'DoubleTime', 'Travel', 'Standby'
	"effectiveDate" TIMESTAMP NOT NULL,		-- The date this rate becomes active. Allows for historical reporting and future price increases.
	"currencyId" INT NOT NULL,		-- Link to the Currency table.
	"costRate" DECIMAL(11,2) NOT NULL,		-- Internal Cost (payroll)
	"billingRate" DECIMAL(11,2) NOT NULL,		-- Invoicing Cost (customre)
	"notes" TEXT NULL,		-- For ad-hoc notes about the entry
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "officeId" FOREIGN KEY ("officeId") REFERENCES "Scheduler"."Office"("id"),		-- Foreign key to the Office table.
	CONSTRAINT "assignmentRoleId" FOREIGN KEY ("assignmentRoleId") REFERENCES "Scheduler"."AssignmentRole"("id"),		-- Foreign key to the AssignmentRole table.
	CONSTRAINT "resourceId" FOREIGN KEY ("resourceId") REFERENCES "Scheduler"."Resource"("id"),		-- Foreign key to the Resource table.
	CONSTRAINT "schedulingTargetId" FOREIGN KEY ("schedulingTargetId") REFERENCES "Scheduler"."SchedulingTarget"("id"),		-- Foreign key to the SchedulingTarget table.
	CONSTRAINT "rateTypeId" FOREIGN KEY ("rateTypeId") REFERENCES "Scheduler"."RateType"("id"),		-- Foreign key to the RateType table.
	CONSTRAINT "currencyId" FOREIGN KEY ("currencyId") REFERENCES "Scheduler"."Currency"("id"),		-- Foreign key to the Currency table.
	CONSTRAINT "UC_RateSheet_tenantGuid_assignmentRoleId_resourceId_schedulingTargetId_rateTypeId_effectiveDate" UNIQUE ( "tenantGuid", "assignmentRoleId", "resourceId", "schedulingTargetId", "rateTypeId", "effectiveDate") 		-- Uniqueness enforced on the RateSheet table's tenantGuid and assignmentRoleId and resourceId and schedulingTargetId and rateTypeId and effectiveDate fields.
);
-- Index on the RateSheet table's tenantGuid field.
CREATE INDEX "I_RateSheet_tenantGuid" ON "Scheduler"."RateSheet" ("tenantGuid")
;

-- Index on the RateSheet table's tenantGuid,officeId fields.
CREATE INDEX "I_RateSheet_tenantGuid_officeId" ON "Scheduler"."RateSheet" ("tenantGuid", "officeId")
;

-- Index on the RateSheet table's tenantGuid,assignmentRoleId fields.
CREATE INDEX "I_RateSheet_tenantGuid_assignmentRoleId" ON "Scheduler"."RateSheet" ("tenantGuid", "assignmentRoleId")
;

-- Index on the RateSheet table's tenantGuid,resourceId fields.
CREATE INDEX "I_RateSheet_tenantGuid_resourceId" ON "Scheduler"."RateSheet" ("tenantGuid", "resourceId")
;

-- Index on the RateSheet table's tenantGuid,schedulingTargetId fields.
CREATE INDEX "I_RateSheet_tenantGuid_schedulingTargetId" ON "Scheduler"."RateSheet" ("tenantGuid", "schedulingTargetId")
;

-- Index on the RateSheet table's tenantGuid,rateTypeId fields.
CREATE INDEX "I_RateSheet_tenantGuid_rateTypeId" ON "Scheduler"."RateSheet" ("tenantGuid", "rateTypeId")
;

-- Index on the RateSheet table's tenantGuid,currencyId fields.
CREATE INDEX "I_RateSheet_tenantGuid_currencyId" ON "Scheduler"."RateSheet" ("tenantGuid", "currencyId")
;

-- Index on the RateSheet table's tenantGuid,active fields.
CREATE INDEX "I_RateSheet_tenantGuid_active" ON "Scheduler"."RateSheet" ("tenantGuid", "active")
;

-- Index on the RateSheet table's tenantGuid,deleted fields.
CREATE INDEX "I_RateSheet_tenantGuid_deleted" ON "Scheduler"."RateSheet" ("tenantGuid", "deleted")
;

-- Index on the RateSheet table's tenantGuid,schedulingTargetId,resourceId,assignmentRoleId,rateTypeId,effectiveDate fields.
CREATE INDEX "I_RateSheet_tenantGuid_schedulingTargetId_resourceId_assignment" ON "Scheduler"."RateSheet" ("tenantGuid", "schedulingTargetId", "resourceId", "assignmentRoleId", "rateTypeId", "effectiveDate")
;


-- The change history for records from the RateSheet table.
CREATE TABLE "Scheduler"."RateSheetChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"rateSheetId" INT NOT NULL,		-- Link to the RateSheet table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "rateSheetId" FOREIGN KEY ("rateSheetId") REFERENCES "Scheduler"."RateSheet"("id")		-- Foreign key to the RateSheet table.
);
-- Index on the RateSheetChangeHistory table's tenantGuid field.
CREATE INDEX "I_RateSheetChangeHistory_tenantGuid" ON "Scheduler"."RateSheetChangeHistory" ("tenantGuid")
;

-- Index on the RateSheetChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_RateSheetChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."RateSheetChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the RateSheetChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_RateSheetChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."RateSheetChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the RateSheetChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_RateSheetChangeHistory_tenantGuid_userId" ON "Scheduler"."RateSheetChangeHistory" ("tenantGuid", "userId")
;

-- Index on the RateSheetChangeHistory table's tenantGuid,rateSheetId fields.
CREATE INDEX "I_RateSheetChangeHistory_tenantGuid_rateSheetId" ON "Scheduler"."RateSheetChangeHistory" ("tenantGuid", "rateSheetId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Links resources to qualifications they possess.  Includes expiry date, issuing authority, and notes.
CREATE TABLE "Scheduler"."ResourceQualification"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"resourceId" INT NOT NULL,		-- Link to the Resource table.
	"qualificationId" INT NOT NULL,		-- Link to the Qualification table.
	"issueDate" TIMESTAMP NULL,		-- Date qualification was granted
	"expiryDate" TIMESTAMP NULL,		-- NULL = no expiry (e.g., permanent license)
	"issuer" VARCHAR(250) NULL,		-- e.g., State Board of Nursing, NCCCO
	"notes" TEXT NULL,
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "resourceId" FOREIGN KEY ("resourceId") REFERENCES "Scheduler"."Resource"("id"),		-- Foreign key to the Resource table.
	CONSTRAINT "qualificationId" FOREIGN KEY ("qualificationId") REFERENCES "Scheduler"."Qualification"("id"),		-- Foreign key to the Qualification table.
	CONSTRAINT "UC_ResourceQualification_tenantGuid_resourceId_qualificationId" UNIQUE ( "tenantGuid", "resourceId", "qualificationId") 		-- Uniqueness enforced on the ResourceQualification table's tenantGuid and resourceId and qualificationId fields.
);
-- Index on the ResourceQualification table's tenantGuid field.
CREATE INDEX "I_ResourceQualification_tenantGuid" ON "Scheduler"."ResourceQualification" ("tenantGuid")
;

-- Index on the ResourceQualification table's tenantGuid,resourceId fields.
CREATE INDEX "I_ResourceQualification_tenantGuid_resourceId" ON "Scheduler"."ResourceQualification" ("tenantGuid", "resourceId")
;

-- Index on the ResourceQualification table's tenantGuid,qualificationId fields.
CREATE INDEX "I_ResourceQualification_tenantGuid_qualificationId" ON "Scheduler"."ResourceQualification" ("tenantGuid", "qualificationId")
;

-- Index on the ResourceQualification table's tenantGuid,expiryDate fields.
CREATE INDEX "I_ResourceQualification_tenantGuid_expiryDate" ON "Scheduler"."ResourceQualification" ("tenantGuid", "expiryDate")
;

-- Index on the ResourceQualification table's tenantGuid,active fields.
CREATE INDEX "I_ResourceQualification_tenantGuid_active" ON "Scheduler"."ResourceQualification" ("tenantGuid", "active")
;

-- Index on the ResourceQualification table's tenantGuid,deleted fields.
CREATE INDEX "I_ResourceQualification_tenantGuid_deleted" ON "Scheduler"."ResourceQualification" ("tenantGuid", "deleted")
;


-- The change history for records from the ResourceQualification table.
CREATE TABLE "Scheduler"."ResourceQualificationChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"resourceQualificationId" INT NOT NULL,		-- Link to the ResourceQualification table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "resourceQualificationId" FOREIGN KEY ("resourceQualificationId") REFERENCES "Scheduler"."ResourceQualification"("id")		-- Foreign key to the ResourceQualification table.
);
-- Index on the ResourceQualificationChangeHistory table's tenantGuid field.
CREATE INDEX "I_ResourceQualificationChangeHistory_tenantGuid" ON "Scheduler"."ResourceQualificationChangeHistory" ("tenantGuid")
;

-- Index on the ResourceQualificationChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ResourceQualificationChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ResourceQualificationChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ResourceQualificationChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ResourceQualificationChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ResourceQualificationChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ResourceQualificationChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ResourceQualificationChangeHistory_tenantGuid_userId" ON "Scheduler"."ResourceQualificationChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ResourceQualificationChangeHistory table's tenantGuid,resourceQualificationId fields.
CREATE INDEX "I_ResourceQualificationChangeHistory_tenantGuid_resourceQualifi" ON "Scheduler"."ResourceQualificationChangeHistory" ("tenantGuid", "resourceQualificationId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Defines periods when a resource is unavailable (blackouts).  Used for vacations, maintenance, training, etc.  If endDateTime is NULL the blackout is ongoing until cleared.
CREATE TABLE "Scheduler"."ResourceAvailability"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"resourceId" INT NOT NULL,		-- Link to the Resource table.
	"timeZoneId" INT NULL,		-- Link to the TimeZone table.
	"startDateTime" TIMESTAMP NOT NULL,		-- Inclusive start of the blackout period
	"endDateTime" TIMESTAMP NULL,		-- NULL = ongoing blackout
	"reason" VARCHAR(250) NULL,
	"notes" TEXT NULL,
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "resourceId" FOREIGN KEY ("resourceId") REFERENCES "Scheduler"."Resource"("id"),		-- Foreign key to the Resource table.
	CONSTRAINT "timeZoneId" FOREIGN KEY ("timeZoneId") REFERENCES "Scheduler"."TimeZone"("id")		-- Foreign key to the TimeZone table.
);
-- Index on the ResourceAvailability table's tenantGuid field.
CREATE INDEX "I_ResourceAvailability_tenantGuid" ON "Scheduler"."ResourceAvailability" ("tenantGuid")
;

-- Index on the ResourceAvailability table's tenantGuid,resourceId fields.
CREATE INDEX "I_ResourceAvailability_tenantGuid_resourceId" ON "Scheduler"."ResourceAvailability" ("tenantGuid", "resourceId")
;

-- Index on the ResourceAvailability table's tenantGuid,timeZoneId fields.
CREATE INDEX "I_ResourceAvailability_tenantGuid_timeZoneId" ON "Scheduler"."ResourceAvailability" ("tenantGuid", "timeZoneId")
;

-- Index on the ResourceAvailability table's tenantGuid,active fields.
CREATE INDEX "I_ResourceAvailability_tenantGuid_active" ON "Scheduler"."ResourceAvailability" ("tenantGuid", "active")
;

-- Index on the ResourceAvailability table's tenantGuid,deleted fields.
CREATE INDEX "I_ResourceAvailability_tenantGuid_deleted" ON "Scheduler"."ResourceAvailability" ("tenantGuid", "deleted")
;

-- Index on the ResourceAvailability table's tenantGuid,resourceId,startDateTime,endDateTime fields.
CREATE INDEX "I_ResourceAvailability_tenantGuid_resourceId_startDateTime_endD" ON "Scheduler"."ResourceAvailability" ("tenantGuid", "resourceId", "startDateTime", "endDateTime")
;


-- The change history for records from the ResourceAvailability table.
CREATE TABLE "Scheduler"."ResourceAvailabilityChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"resourceAvailabilityId" INT NOT NULL,		-- Link to the ResourceAvailability table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "resourceAvailabilityId" FOREIGN KEY ("resourceAvailabilityId") REFERENCES "Scheduler"."ResourceAvailability"("id")		-- Foreign key to the ResourceAvailability table.
);
-- Index on the ResourceAvailabilityChangeHistory table's tenantGuid field.
CREATE INDEX "I_ResourceAvailabilityChangeHistory_tenantGuid" ON "Scheduler"."ResourceAvailabilityChangeHistory" ("tenantGuid")
;

-- Index on the ResourceAvailabilityChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ResourceAvailabilityChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ResourceAvailabilityChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ResourceAvailabilityChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ResourceAvailabilityChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ResourceAvailabilityChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ResourceAvailabilityChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ResourceAvailabilityChangeHistory_tenantGuid_userId" ON "Scheduler"."ResourceAvailabilityChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ResourceAvailabilityChangeHistory table's tenantGuid,resourceAvailabilityId fields.
CREATE INDEX "I_ResourceAvailabilityChangeHistory_tenantGuid_resourceAvailabi" ON "Scheduler"."ResourceAvailabilityChangeHistory" ("tenantGuid", "resourceAvailabilityId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Defines regular working shifts for a resource (e.g., clinician hours).  Used to determine baseline availability. Blackouts (ResourceAvailability) override these for exceptions.
CREATE TABLE "Scheduler"."ResourceShift"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"resourceId" INT NOT NULL,		-- Link to the Resource table.
	"dayOfWeek" INT NOT NULL DEFAULT 1,		-- 1=Sunday through 7=Saturday
	"timeZoneId" INT NULL,		-- Link to the TimeZone table.
	"startTime" TIME NOT NULL,		-- Shift start time (e.g., 09:00:00)
	"hours" REAL NOT NULL DEFAULT 8,		-- Hours available from start time (handles overnight shifts cleanly)  Defaults to 8
	"label" VARCHAR(250) NULL,		-- e.g., 'Morning Clinic', 'On-Call'
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "resourceId" FOREIGN KEY ("resourceId") REFERENCES "Scheduler"."Resource"("id"),		-- Foreign key to the Resource table.
	CONSTRAINT "timeZoneId" FOREIGN KEY ("timeZoneId") REFERENCES "Scheduler"."TimeZone"("id"),		-- Foreign key to the TimeZone table.
	CONSTRAINT "UC_ResourceShift_tenantGuid_resourceId_dayOfWeek" UNIQUE ( "tenantGuid", "resourceId", "dayOfWeek") 		-- Uniqueness enforced on the ResourceShift table's tenantGuid and resourceId and dayOfWeek fields.
);
-- Index on the ResourceShift table's tenantGuid field.
CREATE INDEX "I_ResourceShift_tenantGuid" ON "Scheduler"."ResourceShift" ("tenantGuid")
;

-- Index on the ResourceShift table's tenantGuid,resourceId fields.
CREATE INDEX "I_ResourceShift_tenantGuid_resourceId" ON "Scheduler"."ResourceShift" ("tenantGuid", "resourceId")
;

-- Index on the ResourceShift table's tenantGuid,timeZoneId fields.
CREATE INDEX "I_ResourceShift_tenantGuid_timeZoneId" ON "Scheduler"."ResourceShift" ("tenantGuid", "timeZoneId")
;

-- Index on the ResourceShift table's tenantGuid,active fields.
CREATE INDEX "I_ResourceShift_tenantGuid_active" ON "Scheduler"."ResourceShift" ("tenantGuid", "active")
;

-- Index on the ResourceShift table's tenantGuid,deleted fields.
CREATE INDEX "I_ResourceShift_tenantGuid_deleted" ON "Scheduler"."ResourceShift" ("tenantGuid", "deleted")
;


-- The change history for records from the ResourceShift table.
CREATE TABLE "Scheduler"."ResourceShiftChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"resourceShiftId" INT NOT NULL,		-- Link to the ResourceShift table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "resourceShiftId" FOREIGN KEY ("resourceShiftId") REFERENCES "Scheduler"."ResourceShift"("id")		-- Foreign key to the ResourceShift table.
);
-- Index on the ResourceShiftChangeHistory table's tenantGuid field.
CREATE INDEX "I_ResourceShiftChangeHistory_tenantGuid" ON "Scheduler"."ResourceShiftChangeHistory" ("tenantGuid")
;

-- Index on the ResourceShiftChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ResourceShiftChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ResourceShiftChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ResourceShiftChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ResourceShiftChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ResourceShiftChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ResourceShiftChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ResourceShiftChangeHistory_tenantGuid_userId" ON "Scheduler"."ResourceShiftChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ResourceShiftChangeHistory table's tenantGuid,resourceShiftId fields.
CREATE INDEX "I_ResourceShiftChangeHistory_tenantGuid_resourceShiftId" ON "Scheduler"."ResourceShiftChangeHistory" ("tenantGuid", "resourceShiftId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Named, reusable group of resources that are typically scheduled together.  Common in construction (e.g., a roller + operator + spotter).  Crews can be assigned to events as a single unit.
CREATE TABLE "Scheduler"."Crew"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"notes" TEXT NULL,
	"officeId" INT NULL,		-- Optional office binding for a crew.
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"avatarFileName" VARCHAR(250) NULL,		-- Part of the binary data field setup
	"avatarSize" BIGINT NULL,		-- Part of the binary data field setup
	"avatarData" BYTEA NULL,		-- Part of the binary data field setup
	"avatarMimeType" VARCHAR(100) NULL,		-- Part of the binary data field setup
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "officeId" FOREIGN KEY ("officeId") REFERENCES "Scheduler"."Office"("id"),		-- Foreign key to the Office table.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_Crew_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Crew table's tenantGuid and name fields.
);
-- Index on the Crew table's tenantGuid field.
CREATE INDEX "I_Crew_tenantGuid" ON "Scheduler"."Crew" ("tenantGuid")
;

-- Index on the Crew table's tenantGuid,name fields.
CREATE INDEX "I_Crew_tenantGuid_name" ON "Scheduler"."Crew" ("tenantGuid", "name")
;

-- Index on the Crew table's tenantGuid,officeId fields.
CREATE INDEX "I_Crew_tenantGuid_officeId" ON "Scheduler"."Crew" ("tenantGuid", "officeId")
;

-- Index on the Crew table's tenantGuid,iconId fields.
CREATE INDEX "I_Crew_tenantGuid_iconId" ON "Scheduler"."Crew" ("tenantGuid", "iconId")
;

-- Index on the Crew table's tenantGuid,active fields.
CREATE INDEX "I_Crew_tenantGuid_active" ON "Scheduler"."Crew" ("tenantGuid", "active")
;

-- Index on the Crew table's tenantGuid,deleted fields.
CREATE INDEX "I_Crew_tenantGuid_deleted" ON "Scheduler"."Crew" ("tenantGuid", "deleted")
;


-- The change history for records from the Crew table.
CREATE TABLE "Scheduler"."CrewChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"crewId" INT NOT NULL,		-- Link to the Crew table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "crewId" FOREIGN KEY ("crewId") REFERENCES "Scheduler"."Crew"("id")		-- Foreign key to the Crew table.
);
-- Index on the CrewChangeHistory table's tenantGuid field.
CREATE INDEX "I_CrewChangeHistory_tenantGuid" ON "Scheduler"."CrewChangeHistory" ("tenantGuid")
;

-- Index on the CrewChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_CrewChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."CrewChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the CrewChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_CrewChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."CrewChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the CrewChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_CrewChangeHistory_tenantGuid_userId" ON "Scheduler"."CrewChangeHistory" ("tenantGuid", "userId")
;

-- Index on the CrewChangeHistory table's tenantGuid,crewId fields.
CREATE INDEX "I_CrewChangeHistory_tenantGuid_crewId" ON "Scheduler"."CrewChangeHistory" ("tenantGuid", "crewId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Membership definition for a crew.  Specifies which resource belongs to which crew, the role they play within the crew, and a display sequence.
CREATE TABLE "Scheduler"."CrewMember"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"crewId" INT NOT NULL,		-- Link to the Crew table.
	"resourceId" INT NOT NULL,		-- Link to the Resource table.
	"assignmentRoleId" INT NULL,		-- Optional default role this member fulfils when the crew is assigned
	"sequence" INT NOT NULL DEFAULT 1,		-- Display/order position within the crew (lower numbers appear first)
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "crewId" FOREIGN KEY ("crewId") REFERENCES "Scheduler"."Crew"("id"),		-- Foreign key to the Crew table.
	CONSTRAINT "resourceId" FOREIGN KEY ("resourceId") REFERENCES "Scheduler"."Resource"("id"),		-- Foreign key to the Resource table.
	CONSTRAINT "assignmentRoleId" FOREIGN KEY ("assignmentRoleId") REFERENCES "Scheduler"."AssignmentRole"("id"),		-- Foreign key to the AssignmentRole table.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_CrewMember_tenantGuid_crewId_resourceId" UNIQUE ( "tenantGuid", "crewId", "resourceId") 		-- Uniqueness enforced on the CrewMember table's tenantGuid and crewId and resourceId fields.
);
-- Index on the CrewMember table's tenantGuid field.
CREATE INDEX "I_CrewMember_tenantGuid" ON "Scheduler"."CrewMember" ("tenantGuid")
;

-- Index on the CrewMember table's tenantGuid,crewId fields.
CREATE INDEX "I_CrewMember_tenantGuid_crewId" ON "Scheduler"."CrewMember" ("tenantGuid", "crewId")
;

-- Index on the CrewMember table's tenantGuid,resourceId fields.
CREATE INDEX "I_CrewMember_tenantGuid_resourceId" ON "Scheduler"."CrewMember" ("tenantGuid", "resourceId")
;

-- Index on the CrewMember table's tenantGuid,assignmentRoleId fields.
CREATE INDEX "I_CrewMember_tenantGuid_assignmentRoleId" ON "Scheduler"."CrewMember" ("tenantGuid", "assignmentRoleId")
;

-- Index on the CrewMember table's tenantGuid,iconId fields.
CREATE INDEX "I_CrewMember_tenantGuid_iconId" ON "Scheduler"."CrewMember" ("tenantGuid", "iconId")
;

-- Index on the CrewMember table's tenantGuid,active fields.
CREATE INDEX "I_CrewMember_tenantGuid_active" ON "Scheduler"."CrewMember" ("tenantGuid", "active")
;

-- Index on the CrewMember table's tenantGuid,deleted fields.
CREATE INDEX "I_CrewMember_tenantGuid_deleted" ON "Scheduler"."CrewMember" ("tenantGuid", "deleted")
;


-- The change history for records from the CrewMember table.
CREATE TABLE "Scheduler"."CrewMemberChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"crewMemberId" INT NOT NULL,		-- Link to the CrewMember table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "crewMemberId" FOREIGN KEY ("crewMemberId") REFERENCES "Scheduler"."CrewMember"("id")		-- Foreign key to the CrewMember table.
);
-- Index on the CrewMemberChangeHistory table's tenantGuid field.
CREATE INDEX "I_CrewMemberChangeHistory_tenantGuid" ON "Scheduler"."CrewMemberChangeHistory" ("tenantGuid")
;

-- Index on the CrewMemberChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_CrewMemberChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."CrewMemberChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the CrewMemberChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_CrewMemberChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."CrewMemberChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the CrewMemberChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_CrewMemberChangeHistory_tenantGuid_userId" ON "Scheduler"."CrewMemberChangeHistory" ("tenantGuid", "userId")
;

-- Index on the CrewMemberChangeHistory table's tenantGuid,crewMemberId fields.
CREATE INDEX "I_CrewMemberChangeHistory_tenantGuid_crewMemberId" ON "Scheduler"."CrewMemberChangeHistory" ("tenantGuid", "crewMemberId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Pre-defined event templates for common appointment/activity types.   Includes default duration, required roles, default assignments, etc.
CREATE TABLE "Scheduler"."ScheduledEventTemplate"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"defaultAllDay" BOOLEAN NOT NULL,		-- Default all day flag.
	"defaultDurationMinutes" INT NOT NULL DEFAULT 60,
	"schedulingTargetTypeId" INT NULL,		-- Optional target type
	"priorityId" INT NULL,		-- Optional priority
	"defaultLocationPattern" VARCHAR(250) NULL,		-- e.g., 'Patient Home', 'Main Site'
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "schedulingTargetTypeId" FOREIGN KEY ("schedulingTargetTypeId") REFERENCES "Scheduler"."SchedulingTargetType"("id"),		-- Foreign key to the SchedulingTargetType table.
	CONSTRAINT "priorityId" FOREIGN KEY ("priorityId") REFERENCES "Scheduler"."Priority"("id"),		-- Foreign key to the Priority table.
	CONSTRAINT "UC_ScheduledEventTemplate_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the ScheduledEventTemplate table's tenantGuid and name fields.
);
-- Index on the ScheduledEventTemplate table's tenantGuid field.
CREATE INDEX "I_ScheduledEventTemplate_tenantGuid" ON "Scheduler"."ScheduledEventTemplate" ("tenantGuid")
;

-- Index on the ScheduledEventTemplate table's tenantGuid,name fields.
CREATE INDEX "I_ScheduledEventTemplate_tenantGuid_name" ON "Scheduler"."ScheduledEventTemplate" ("tenantGuid", "name")
;

-- Index on the ScheduledEventTemplate table's tenantGuid,schedulingTargetTypeId fields.
CREATE INDEX "I_ScheduledEventTemplate_tenantGuid_schedulingTargetTypeId" ON "Scheduler"."ScheduledEventTemplate" ("tenantGuid", "schedulingTargetTypeId")
;

-- Index on the ScheduledEventTemplate table's tenantGuid,active fields.
CREATE INDEX "I_ScheduledEventTemplate_tenantGuid_active" ON "Scheduler"."ScheduledEventTemplate" ("tenantGuid", "active")
;

-- Index on the ScheduledEventTemplate table's tenantGuid,deleted fields.
CREATE INDEX "I_ScheduledEventTemplate_tenantGuid_deleted" ON "Scheduler"."ScheduledEventTemplate" ("tenantGuid", "deleted")
;


-- The change history for records from the ScheduledEventTemplate table.
CREATE TABLE "Scheduler"."ScheduledEventTemplateChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduledEventTemplateId" INT NOT NULL,		-- Link to the ScheduledEventTemplate table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "scheduledEventTemplateId" FOREIGN KEY ("scheduledEventTemplateId") REFERENCES "Scheduler"."ScheduledEventTemplate"("id")		-- Foreign key to the ScheduledEventTemplate table.
);
-- Index on the ScheduledEventTemplateChangeHistory table's tenantGuid field.
CREATE INDEX "I_ScheduledEventTemplateChangeHistory_tenantGuid" ON "Scheduler"."ScheduledEventTemplateChangeHistory" ("tenantGuid")
;

-- Index on the ScheduledEventTemplateChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ScheduledEventTemplateChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ScheduledEventTemplateChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ScheduledEventTemplateChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ScheduledEventTemplateChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ScheduledEventTemplateChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ScheduledEventTemplateChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ScheduledEventTemplateChangeHistory_tenantGuid_userId" ON "Scheduler"."ScheduledEventTemplateChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ScheduledEventTemplateChangeHistory table's tenantGuid,scheduledEventTemplateId fields.
CREATE INDEX "I_ScheduledEventTemplateChangeHistory_tenantGuid_scheduledEvent" ON "Scheduler"."ScheduledEventTemplateChangeHistory" ("tenantGuid", "scheduledEventTemplateId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
 SCHEDULED EVENT TEMPLATE CHARGES (For Auto-Dropping)
 Defines default charges for ScheduledEventTemplate).
 When an event is created from a template, these charges are auto-dropped onto the event.
 ====================================================================================================
*/
CREATE TABLE "Scheduler"."ScheduledEventTemplateCharge"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduledEventTemplateId" INT NOT NULL,		-- Link to ScheduledEventTemplate
	"chargeTypeId" INT NOT NULL,		-- Link to ChargeType (the charge to drop).
	"defaultAmount" DECIMAL(11,2) NOT NULL,		-- The amount to auto-drop (can be overridden on event).
	"isRequired" BOOLEAN NOT NULL DEFAULT true,		-- some default charges might be optional (e.g., optional add-on fee).
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "scheduledEventTemplateId" FOREIGN KEY ("scheduledEventTemplateId") REFERENCES "Scheduler"."ScheduledEventTemplate"("id"),		-- Foreign key to the ScheduledEventTemplate table.
	CONSTRAINT "chargeTypeId" FOREIGN KEY ("chargeTypeId") REFERENCES "Scheduler"."ChargeType"("id")		-- Foreign key to the ChargeType table.
);
-- Index on the ScheduledEventTemplateCharge table's tenantGuid field.
CREATE INDEX "I_ScheduledEventTemplateCharge_tenantGuid" ON "Scheduler"."ScheduledEventTemplateCharge" ("tenantGuid")
;

-- Index on the ScheduledEventTemplateCharge table's tenantGuid,scheduledEventTemplateId fields.
CREATE INDEX "I_ScheduledEventTemplateCharge_tenantGuid_scheduledEventTemplat" ON "Scheduler"."ScheduledEventTemplateCharge" ("tenantGuid", "scheduledEventTemplateId")
;

-- Index on the ScheduledEventTemplateCharge table's tenantGuid,chargeTypeId fields.
CREATE INDEX "I_ScheduledEventTemplateCharge_tenantGuid_chargeTypeId" ON "Scheduler"."ScheduledEventTemplateCharge" ("tenantGuid", "chargeTypeId")
;

-- Index on the ScheduledEventTemplateCharge table's tenantGuid,active fields.
CREATE INDEX "I_ScheduledEventTemplateCharge_tenantGuid_active" ON "Scheduler"."ScheduledEventTemplateCharge" ("tenantGuid", "active")
;

-- Index on the ScheduledEventTemplateCharge table's tenantGuid,deleted fields.
CREATE INDEX "I_ScheduledEventTemplateCharge_tenantGuid_deleted" ON "Scheduler"."ScheduledEventTemplateCharge" ("tenantGuid", "deleted")
;


-- The change history for records from the ScheduledEventTemplateCharge table.
CREATE TABLE "Scheduler"."ScheduledEventTemplateChargeChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduledEventTemplateChargeId" INT NOT NULL,		-- Link to the ScheduledEventTemplateCharge table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "scheduledEventTemplateChargeId" FOREIGN KEY ("scheduledEventTemplateChargeId") REFERENCES "Scheduler"."ScheduledEventTemplateCharge"("id")		-- Foreign key to the ScheduledEventTemplateCharge table.
);
-- Index on the ScheduledEventTemplateChargeChangeHistory table's tenantGuid field.
CREATE INDEX "I_ScheduledEventTemplateChargeChangeHistory_tenantGuid" ON "Scheduler"."ScheduledEventTemplateChargeChangeHistory" ("tenantGuid")
;

-- Index on the ScheduledEventTemplateChargeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ScheduledEventTemplateChargeChangeHistory_tenantGuid_versionN" ON "Scheduler"."ScheduledEventTemplateChargeChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ScheduledEventTemplateChargeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ScheduledEventTemplateChargeChangeHistory_tenantGuid_timeStam" ON "Scheduler"."ScheduledEventTemplateChargeChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ScheduledEventTemplateChargeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ScheduledEventTemplateChargeChangeHistory_tenantGuid_userId" ON "Scheduler"."ScheduledEventTemplateChargeChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ScheduledEventTemplateChargeChangeHistory table's tenantGuid,scheduledEventTemplateChargeId fields.
CREATE INDEX "I_ScheduledEventTemplateChargeChangeHistory_tenantGuid_schedule" ON "Scheduler"."ScheduledEventTemplateChargeChangeHistory" ("tenantGuid", "scheduledEventTemplateChargeId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Default qualification requirements for events created from a template.
CREATE TABLE "Scheduler"."ScheduledEventTemplateQualificationRequirement"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduledEventTemplateId" INT NOT NULL,		-- Link to the ScheduledEventTemplate table.
	"qualificationId" INT NOT NULL,		-- Link to the Qualification table.
	"isRequired" BOOLEAN NOT NULL DEFAULT true,
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "scheduledEventTemplateId" FOREIGN KEY ("scheduledEventTemplateId") REFERENCES "Scheduler"."ScheduledEventTemplate"("id"),		-- Foreign key to the ScheduledEventTemplate table.
	CONSTRAINT "qualificationId" FOREIGN KEY ("qualificationId") REFERENCES "Scheduler"."Qualification"("id"),		-- Foreign key to the Qualification table.
	CONSTRAINT "UC_ScheduledEventTemplateQualificationRequirement_tenantGuid_scheduledEventTemplateId_qualificationId" UNIQUE ( "tenantGuid", "scheduledEventTemplateId", "qualificationId") 		-- Uniqueness enforced on the ScheduledEventTemplateQualificationRequirement table's tenantGuid and scheduledEventTemplateId and qualificationId fields.
);
-- Index on the ScheduledEventTemplateQualificationRequirement table's tenantGuid field.
CREATE INDEX "I_ScheduledEventTemplateQualificationRequirement_tenantGuid" ON "Scheduler"."ScheduledEventTemplateQualificationRequirement" ("tenantGuid")
;

-- Index on the ScheduledEventTemplateQualificationRequirement table's tenantGuid,scheduledEventTemplateId fields.
CREATE INDEX "I_ScheduledEventTemplateQualificationRequirement_tenantGuid_sch" ON "Scheduler"."ScheduledEventTemplateQualificationRequirement" ("tenantGuid", "scheduledEventTemplateId")
;

-- Index on the ScheduledEventTemplateQualificationRequirement table's tenantGuid,qualificationId fields.
CREATE INDEX "I_ScheduledEventTemplateQualificationRequirement_tenantGuid_qua" ON "Scheduler"."ScheduledEventTemplateQualificationRequirement" ("tenantGuid", "qualificationId")
;

-- Index on the ScheduledEventTemplateQualificationRequirement table's tenantGuid,active fields.
CREATE INDEX "I_ScheduledEventTemplateQualificationRequirement_tenantGuid_act" ON "Scheduler"."ScheduledEventTemplateQualificationRequirement" ("tenantGuid", "active")
;

-- Index on the ScheduledEventTemplateQualificationRequirement table's tenantGuid,deleted fields.
CREATE INDEX "I_ScheduledEventTemplateQualificationRequirement_tenantGuid_del" ON "Scheduler"."ScheduledEventTemplateQualificationRequirement" ("tenantGuid", "deleted")
;


-- The change history for records from the ScheduledEventTemplateQualificationRequirement table.
CREATE TABLE "Scheduler"."ScheduledEventTemplateQualificationRequirementChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduledEventTemplateQualificationRequirementId" INT NOT NULL,		-- Link to the ScheduledEventTemplateQualificationRequirement table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "scheduledEventTemplateQualificationRequirementId" FOREIGN KEY ("scheduledEventTemplateQualificationRequirementId") REFERENCES "Scheduler"."ScheduledEventTemplateQualificationRequirement"("id")		-- Foreign key to the ScheduledEventTemplateQualificationRequirement table.
);
-- Index on the ScheduledEventTemplateQualificationRequirementChangeHistory table's tenantGuid field.
CREATE INDEX "I_ScheduledEventTemplateQualificationRequirementChangeHistory_t" ON "Scheduler"."ScheduledEventTemplateQualificationRequirementChangeHistory" ("tenantGuid")
;

-- Index on the ScheduledEventTemplateQualificationRequirementChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ScheduledEventTemplateQualificationRequirementChangeHistory_t" ON "Scheduler"."ScheduledEventTemplateQualificationRequirementChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ScheduledEventTemplateQualificationRequirementChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ScheduledEventTemplateQualificationRequirementChangeHistory_t" ON "Scheduler"."ScheduledEventTemplateQualificationRequirementChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ScheduledEventTemplateQualificationRequirementChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ScheduledEventTemplateQualificationRequirementChangeHistory_t" ON "Scheduler"."ScheduledEventTemplateQualificationRequirementChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ScheduledEventTemplateQualificationRequirementChangeHistory table's tenantGuid,scheduledEventTemplateQualificationRequirementId fields.
CREATE INDEX "I_ScheduledEventTemplateQualificationRequirementChangeHistory_t" ON "Scheduler"."ScheduledEventTemplateQualificationRequirementChangeHistory" ("tenantGuid", "scheduledEventTemplateQualificationRequirementId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
 EVENT TYPE
 Tenant-configurable categories of scheduled events that drive the booking workflow.
 Each event type has boolean flags indicating what is required (rental agreement,
 external contact info, payment, deposit, bar service) and what is allowed (ticket sales).
 The isInternalEvent flag distinguishes committee-run events from private rentals.

 DESIGN NOTE: defaultPrice and defaultChargeTypeId enable auto-creation of EventCharge
 records when a new event is created with this type, streamlining the booking flow for
 facility rental scenarios.
 =====================================================================================================
*/
CREATE TABLE "Scheduler"."EventType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"color" VARCHAR(10) NULL,		-- Hex color for UI display and calendar event color-coding.
	"iconId" INT NULL,		-- Icon to use for UI display.
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"requiresRentalAgreement" BOOLEAN NOT NULL DEFAULT false,		-- Whether events of this type require a signed rental agreement.
	"requiresExternalContact" BOOLEAN NOT NULL DEFAULT false,		-- Whether events of this type require external contact info (name, email, phone).
	"requiresPayment" BOOLEAN NOT NULL DEFAULT false,		-- Whether events of this type require payment tracking.
	"requiresDeposit" BOOLEAN NOT NULL DEFAULT false,		-- Whether events of this type require a deposit (uses EventCharge.isDeposit).
	"requiresBarService" BOOLEAN NOT NULL DEFAULT false,		-- Whether events of this type default to needing bar service (alcohol + bartender staffing). Can be overridden per event.
	"allowsTicketSales" BOOLEAN NOT NULL DEFAULT false,		-- Whether events of this type support ticket sales tracking.
	"isInternalEvent" BOOLEAN NOT NULL DEFAULT false,		-- True for committee-run events; false for private rentals. Drives which booking flow is shown in simple mode.
	"defaultPrice" DECIMAL(11,2) NULL,		-- Default rental price for events of this type. Used to auto-populate charges in the booking flow.
	"chargeTypeId" INT NULL,		-- Default charge type for auto-created charges.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "chargeTypeId" FOREIGN KEY ("chargeTypeId") REFERENCES "Scheduler"."ChargeType"("id"),		-- Foreign key to the ChargeType table.
	CONSTRAINT "UC_EventType_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the EventType table's tenantGuid and name fields.
);
-- Index on the EventType table's tenantGuid field.
CREATE INDEX "I_EventType_tenantGuid" ON "Scheduler"."EventType" ("tenantGuid")
;

-- Index on the EventType table's tenantGuid,name fields.
CREATE INDEX "I_EventType_tenantGuid_name" ON "Scheduler"."EventType" ("tenantGuid", "name")
;

-- Index on the EventType table's tenantGuid,iconId fields.
CREATE INDEX "I_EventType_tenantGuid_iconId" ON "Scheduler"."EventType" ("tenantGuid", "iconId")
;

-- Index on the EventType table's tenantGuid,chargeTypeId fields.
CREATE INDEX "I_EventType_tenantGuid_chargeTypeId" ON "Scheduler"."EventType" ("tenantGuid", "chargeTypeId")
;

-- Index on the EventType table's tenantGuid,active fields.
CREATE INDEX "I_EventType_tenantGuid_active" ON "Scheduler"."EventType" ("tenantGuid", "active")
;

-- Index on the EventType table's tenantGuid,deleted fields.
CREATE INDEX "I_EventType_tenantGuid_deleted" ON "Scheduler"."EventType" ("tenantGuid", "deleted")
;


-- The change history for records from the EventType table.
CREATE TABLE "Scheduler"."EventTypeChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"eventTypeId" INT NOT NULL,		-- Link to the EventType table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "eventTypeId" FOREIGN KEY ("eventTypeId") REFERENCES "Scheduler"."EventType"("id")		-- Foreign key to the EventType table.
);
-- Index on the EventTypeChangeHistory table's tenantGuid field.
CREATE INDEX "I_EventTypeChangeHistory_tenantGuid" ON "Scheduler"."EventTypeChangeHistory" ("tenantGuid")
;

-- Index on the EventTypeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_EventTypeChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."EventTypeChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the EventTypeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_EventTypeChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."EventTypeChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the EventTypeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_EventTypeChangeHistory_tenantGuid_userId" ON "Scheduler"."EventTypeChangeHistory" ("tenantGuid", "userId")
;

-- Index on the EventTypeChangeHistory table's tenantGuid,eventTypeId fields.
CREATE INDEX "I_EventTypeChangeHistory_tenantGuid_eventTypeId" ON "Scheduler"."EventTypeChangeHistory" ("tenantGuid", "eventTypeId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
Core scheduling entity – any planned activity with a defined time range.  This manages recurrences with the 'Detachment Model'

How it works:
The Master: You create the Series (Event A). It has the RecurrenceRule.
The Virtuals: The UI calculates the "Ghost" instances for display.
The Exception: If you assign a specific crew to next Tuesday's instance (or move it), the system "Detaches" that instance.
It creates a new row in ScheduledEvent (Event B).
Event B is linked to Event A via a parentScheduledEventId.
You add a record to RecurrenceException for Event A saying "Skip the normal generation for Date X."
You attach the specific Crew/Resource to Event B.
*/
CREATE TABLE "Scheduler"."ScheduledEvent"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"officeId" INT NULL,		-- Snapshot of office that the first resource assigned to this event belongs to.  This should NOT be updated if a resource moves to a different office post-event assignment.  It should only change if there was an original entry error that needs to be corrected.
	"clientId" INT NULL,		-- Snapshot of client that this event belongs to.  It should be that of the scheduling target.  It should only change if there was an original entry error that needs to be corrected.
	"scheduledEventTemplateId" INT NULL,		-- Optional template/type of this scheduled event.
	"recurrenceRuleId" INT NULL,		-- Optional recurrence pattern for this event series
	"schedulingTargetId" INT NULL,		-- The SchedulingTarget (project, patient, etc.) this event is scheduled into
	"timeZoneId" INT NULL,		-- Link to the TimeZone table.
	"parentScheduledEventId" INT NULL,		-- If populated, this Event is a specific "Detached" instance of a Series
	"recurrenceInstanceDate" TIMESTAMP NULL,		-- The original date this instance represented (crucial for matching with RecurrenceException)
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"isAllDay" BOOLEAN NULL DEFAULT false,		-- Whether this is an all day event or not
	"startDateTime" TIMESTAMP NOT NULL,		-- Inclusive start of the event in UTC
	"endDateTime" TIMESTAMP NOT NULL,		-- Exclusive end of the event in UTC
	"location" VARCHAR(250) NULL,
	"eventStatusId" INT NOT NULL,		-- Status for the event
	"resourceId" INT NULL,		-- Optional primary/lead resource for the event
	"crewId" INT NULL,		-- Optional primary/lead crew for the event
	"priorityId" INT NULL,		-- Optional priority
	"bookingSourceTypeId" INT NULL,		-- Optional booking source for reservation type workflows.
	"eventTypeId" INT NULL,		-- Event type category — drives UI behavior (rental vs committee event flow, required fields, default pricing).
	"partySize" INT NULL,		-- Optional for use when running as a reservation system
	"bookingContactName" VARCHAR(250) NULL,		-- Name of the person booking (e.g., hall renter). Supports quick data entry without creating a full Contact.
	"bookingContactEmail" VARCHAR(250) NULL,		-- Email of the person booking.
	"bookingContactPhone" VARCHAR(50) NULL,		-- Phone number of the person booking.
	"notes" TEXT NULL,
	"color" VARCHAR(10) NULL,		-- Override Hex color for UI display
	"externalId" VARCHAR(100) NULL,		-- Optional link to an entity in another system
	"attributes" TEXT NULL,		-- to store arbitrary JSON
	"isOpenForVolunteers" BOOLEAN NOT NULL DEFAULT false,		-- Whether this event appears in the Volunteer Hub opportunity browser for self-sign-up
	"maxVolunteerSlots" INT NULL,		-- Maximum number of volunteer sign-ups allowed; NULL = unlimited
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "officeId" FOREIGN KEY ("officeId") REFERENCES "Scheduler"."Office"("id"),		-- Foreign key to the Office table.
	CONSTRAINT "clientId" FOREIGN KEY ("clientId") REFERENCES "Scheduler"."Client"("id"),		-- Foreign key to the Client table.
	CONSTRAINT "scheduledEventTemplateId" FOREIGN KEY ("scheduledEventTemplateId") REFERENCES "Scheduler"."ScheduledEventTemplate"("id"),		-- Foreign key to the ScheduledEventTemplate table.
	CONSTRAINT "recurrenceRuleId" FOREIGN KEY ("recurrenceRuleId") REFERENCES "Scheduler"."RecurrenceRule"("id"),		-- Foreign key to the RecurrenceRule table.
	CONSTRAINT "schedulingTargetId" FOREIGN KEY ("schedulingTargetId") REFERENCES "Scheduler"."SchedulingTarget"("id"),		-- Foreign key to the SchedulingTarget table.
	CONSTRAINT "timeZoneId" FOREIGN KEY ("timeZoneId") REFERENCES "Scheduler"."TimeZone"("id"),		-- Foreign key to the TimeZone table.
	CONSTRAINT "parentScheduledEventId" FOREIGN KEY ("parentScheduledEventId") REFERENCES "Scheduler"."ScheduledEvent"("id"),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT "eventStatusId" FOREIGN KEY ("eventStatusId") REFERENCES "Scheduler"."EventStatus"("id"),		-- Foreign key to the EventStatus table.
	CONSTRAINT "resourceId" FOREIGN KEY ("resourceId") REFERENCES "Scheduler"."Resource"("id"),		-- Foreign key to the Resource table.
	CONSTRAINT "crewId" FOREIGN KEY ("crewId") REFERENCES "Scheduler"."Crew"("id"),		-- Foreign key to the Crew table.
	CONSTRAINT "priorityId" FOREIGN KEY ("priorityId") REFERENCES "Scheduler"."Priority"("id"),		-- Foreign key to the Priority table.
	CONSTRAINT "bookingSourceTypeId" FOREIGN KEY ("bookingSourceTypeId") REFERENCES "Scheduler"."BookingSourceType"("id"),		-- Foreign key to the BookingSourceType table.
	CONSTRAINT "eventTypeId" FOREIGN KEY ("eventTypeId") REFERENCES "Scheduler"."EventType"("id"),		-- Foreign key to the EventType table.
	CONSTRAINT "UC_ScheduledEvent_tenantGuid_name_startDateTime" UNIQUE ( "tenantGuid", "name", "startDateTime") 		-- Uniqueness enforced on the ScheduledEvent table's tenantGuid and name and startDateTime fields.
);
-- Index on the ScheduledEvent table's tenantGuid field.
CREATE INDEX "I_ScheduledEvent_tenantGuid" ON "Scheduler"."ScheduledEvent" ("tenantGuid")
;

-- Index on the ScheduledEvent table's tenantGuid,officeId fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_officeId" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "officeId")
;

-- Index on the ScheduledEvent table's tenantGuid,clientId fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_clientId" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "clientId")
;

-- Index on the ScheduledEvent table's tenantGuid,scheduledEventTemplateId fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_scheduledEventTemplateId" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "scheduledEventTemplateId")
;

-- Index on the ScheduledEvent table's tenantGuid,recurrenceRuleId fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_recurrenceRuleId" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "recurrenceRuleId")
;

-- Index on the ScheduledEvent table's tenantGuid,schedulingTargetId fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_schedulingTargetId" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "schedulingTargetId")
;

-- Index on the ScheduledEvent table's tenantGuid,timeZoneId fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_timeZoneId" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "timeZoneId")
;

-- Index on the ScheduledEvent table's tenantGuid,parentScheduledEventId fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_parentScheduledEventId" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "parentScheduledEventId")
;

-- Index on the ScheduledEvent table's tenantGuid,name fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_name" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "name")
;

-- Index on the ScheduledEvent table's tenantGuid,startDateTime fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_startDateTime" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "startDateTime")
;

-- Index on the ScheduledEvent table's tenantGuid,endDateTime fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_endDateTime" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "endDateTime")
;

-- Index on the ScheduledEvent table's tenantGuid,location fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_location" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "location")
;

-- Index on the ScheduledEvent table's tenantGuid,eventStatusId fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_eventStatusId" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "eventStatusId")
;

-- Index on the ScheduledEvent table's tenantGuid,resourceId fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_resourceId" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "resourceId")
;

-- Index on the ScheduledEvent table's tenantGuid,crewId fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_crewId" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "crewId")
;

-- Index on the ScheduledEvent table's tenantGuid,priorityId fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_priorityId" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "priorityId")
;

-- Index on the ScheduledEvent table's tenantGuid,bookingSourceTypeId fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_bookingSourceTypeId" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "bookingSourceTypeId")
;

-- Index on the ScheduledEvent table's tenantGuid,eventTypeId fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_eventTypeId" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "eventTypeId")
;

-- Index on the ScheduledEvent table's tenantGuid,active fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_active" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "active")
;

-- Index on the ScheduledEvent table's tenantGuid,deleted fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_deleted" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "deleted")
;

-- Index on the ScheduledEvent table's tenantGuid,startDateTime,endDateTime fields.
CREATE INDEX "I_ScheduledEvent_tenantGuid_startDateTime_endDateTime" ON "Scheduler"."ScheduledEvent" ("tenantGuid", "startDateTime", "endDateTime")
;


-- The change history for records from the ScheduledEvent table.
CREATE TABLE "Scheduler"."ScheduledEventChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduledEventId" INT NOT NULL,		-- Link to the ScheduledEvent table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "scheduledEventId" FOREIGN KEY ("scheduledEventId") REFERENCES "Scheduler"."ScheduledEvent"("id")		-- Foreign key to the ScheduledEvent table.
);
-- Index on the ScheduledEventChangeHistory table's tenantGuid field.
CREATE INDEX "I_ScheduledEventChangeHistory_tenantGuid" ON "Scheduler"."ScheduledEventChangeHistory" ("tenantGuid")
;

-- Index on the ScheduledEventChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ScheduledEventChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ScheduledEventChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ScheduledEventChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ScheduledEventChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ScheduledEventChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ScheduledEventChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ScheduledEventChangeHistory_tenantGuid_userId" ON "Scheduler"."ScheduledEventChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ScheduledEventChangeHistory table's tenantGuid,scheduledEventId fields.
CREATE INDEX "I_ScheduledEventChangeHistory_tenantGuid_scheduledEventId" ON "Scheduler"."ScheduledEventChangeHistory" ("tenantGuid", "scheduledEventId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Master list of charge statuses (Pending, Approved, Invoiced, Void). Not tenant-specific because workflow will be tied to these values.  Could be redesigned later to get really fancy for tenant specific workflow, but out of scope for now.
CREATE TABLE "Scheduler"."ChargeStatus"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the ChargeStatus table's name field.
CREATE INDEX "I_ChargeStatus_name" ON "Scheduler"."ChargeStatus" ("name")
;

-- Index on the ChargeStatus table's active field.
CREATE INDEX "I_ChargeStatus_active" ON "Scheduler"."ChargeStatus" ("active")
;

-- Index on the ChargeStatus table's deleted field.
CREATE INDEX "I_ChargeStatus_deleted" ON "Scheduler"."ChargeStatus" ("deleted")
;

INSERT INTO "Scheduler"."ChargeStatus" ( "name", "description", "sequence", "color", "objectGuid" ) VALUES  ( 'Pending', 'Pending Approval', 1, '#B8FFC3', '1379f1da-c3cc-4149-998a-95ffa1728db6' );

INSERT INTO "Scheduler"."ChargeStatus" ( "name", "description", "sequence", "color", "objectGuid" ) VALUES  ( 'Approved', 'Approved ', 2, '#59FF6F', 'ea16c955-9ccf-4489-acc0-0757c39ac3b6' );

INSERT INTO "Scheduler"."ChargeStatus" ( "name", "description", "sequence", "color", "objectGuid" ) VALUES  ( 'Invoiced', 'Invoiced', 3, '#35A145', 'd250cc5c-51e9-49bb-91ce-4be47fc30dc0' );

INSERT INTO "Scheduler"."ChargeStatus" ( "name", "description", "color", "sequence", "objectGuid" ) VALUES  ( 'Void', 'Void - Charge Disregarded', '#C62828', 4, '19d6560f-ed85-4d1e-905f-9a6e3dfb3026' );


-- The change history for records from the ChargeStatus table.
CREATE TABLE "Scheduler"."ChargeStatusChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"chargeStatusId" INT NOT NULL,		-- Link to the ChargeStatus table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "chargeStatusId" FOREIGN KEY ("chargeStatusId") REFERENCES "Scheduler"."ChargeStatus"("id")		-- Foreign key to the ChargeStatus table.
);
-- Index on the ChargeStatusChangeHistory table's versionNumber field.
CREATE INDEX "I_ChargeStatusChangeHistory_versionNumber" ON "Scheduler"."ChargeStatusChangeHistory" ("versionNumber")
;

-- Index on the ChargeStatusChangeHistory table's timeStamp field.
CREATE INDEX "I_ChargeStatusChangeHistory_timeStamp" ON "Scheduler"."ChargeStatusChangeHistory" ("timeStamp")
;

-- Index on the ChargeStatusChangeHistory table's userId field.
CREATE INDEX "I_ChargeStatusChangeHistory_userId" ON "Scheduler"."ChargeStatusChangeHistory" ("userId")
;

-- Index on the ChargeStatusChangeHistory table's chargeStatusId field.
CREATE INDEX "I_ChargeStatusChangeHistory_chargeStatusId" ON "Scheduler"."ChargeStatusChangeHistory" ("chargeStatusId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
 EVENT CHARGES
 Stores charges dropped on ScheduledEvents (automatic or manual).
 Linked to ChargeType for categorization.
 Exportable to QuickBooks as JournalEntries or Invoices.

DESIGN NOTE: EventCharge supports both flat fees and quantity-based charges.
- Flat fee: quantity = 1, unitPrice = NULL or = extendedAmount
- Variable: quantity > 0, unitPrice set → extendedAmount = quantity × unitPrice
- The system should always store the final extendedAmount (allows manual overrides)
- Use externalId + exportedDate for idempotent GL sync

====================================================================================================
*/
CREATE TABLE "Scheduler"."EventCharge"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduledEventId" INT NOT NULL,		-- Link to the ScheduledEvent table.
	"resourceId" INT NULL,		-- Optional link to resource to bind charge to specific resources (e.g., labor cost per operator
	"chargeTypeId" INT NOT NULL,		-- Link to the ChargeType table (defines revenue/expense category).
	"chargeStatusId" INT NOT NULL,		-- Link to the ChargeStatus table.  Tracks the status of the charge from creation through invoicing or cancelling.
	"quantity" NUMERIC(38,22) NULL DEFAULT 1,		-- Quantity (hours, units, km, etc.)
	"unitPrice" DECIMAL(11,2) NULL,		-- Price per unit (can be NULL for flat fees)
	"extendedAmount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Pre-tax amount (quantity × unitPrice, or just amount for flat fees).
	"taxAmount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- The calculated tax based on TaxCode.rate.
	"totalAmount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Total amount inclusive of tax (extendedAmount + taxAmount). Consistent with FinancialTransaction.totalAmount.
	"description" VARCHAR(250) NULL,		-- Short description or label for the charge. Falls back to ChargeType.name if not set.
	"currencyId" INT NOT NULL,		-- Link to Currency table.
	"rateTypeId" INT NULL,		-- Optional link to RateType (e.g., 'Overtime').
	"notes" TEXT NULL,		-- Optional notes about the charge
	"isAutomatic" BOOLEAN NOT NULL DEFAULT true,		-- 1 = auto-dropped from event type, 0 = manual add/edit.
	"isDeposit" BOOLEAN NOT NULL DEFAULT false,		-- Marks this charge as a refundable deposit (e.g., damage deposit for hall rental).
	"depositRefundedDate" TIMESTAMP NULL,		-- When the deposit was refunded (null = not yet refunded). Only applicable when isDeposit = true.
	"exportedDate" TIMESTAMP NULL,		-- When this charge was last exported (null = not exported yet).
	"externalId" VARCHAR(100) NULL,		-- Identifier from extenral system - possibly invoice number or some other billing grouper
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	"taxCodeId" INT NULL,		-- Optional link to TaxCode. When set, indicates which tax rate was applied to calculate taxAmount. Inherited from ChargeType.taxCodeId on creation.
	CONSTRAINT "scheduledEventId" FOREIGN KEY ("scheduledEventId") REFERENCES "Scheduler"."ScheduledEvent"("id"),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT "resourceId" FOREIGN KEY ("resourceId") REFERENCES "Scheduler"."Resource"("id"),		-- Foreign key to the Resource table.
	CONSTRAINT "chargeTypeId" FOREIGN KEY ("chargeTypeId") REFERENCES "Scheduler"."ChargeType"("id"),		-- Foreign key to the ChargeType table.
	CONSTRAINT "chargeStatusId" FOREIGN KEY ("chargeStatusId") REFERENCES "Scheduler"."ChargeStatus"("id"),		-- Foreign key to the ChargeStatus table.
	CONSTRAINT "currencyId" FOREIGN KEY ("currencyId") REFERENCES "Scheduler"."Currency"("id"),		-- Foreign key to the Currency table.
	CONSTRAINT "rateTypeId" FOREIGN KEY ("rateTypeId") REFERENCES "Scheduler"."RateType"("id"),		-- Foreign key to the RateType table.
	CONSTRAINT "taxCodeId" FOREIGN KEY ("taxCodeId") REFERENCES "Scheduler"."TaxCode"("id")		-- Foreign key to the TaxCode table.
);
-- Index on the EventCharge table's tenantGuid field.
CREATE INDEX "I_EventCharge_tenantGuid" ON "Scheduler"."EventCharge" ("tenantGuid")
;

-- Index on the EventCharge table's tenantGuid,scheduledEventId fields.
CREATE INDEX "I_EventCharge_tenantGuid_scheduledEventId" ON "Scheduler"."EventCharge" ("tenantGuid", "scheduledEventId")
;

-- Index on the EventCharge table's tenantGuid,resourceId fields.
CREATE INDEX "I_EventCharge_tenantGuid_resourceId" ON "Scheduler"."EventCharge" ("tenantGuid", "resourceId")
;

-- Index on the EventCharge table's tenantGuid,chargeTypeId fields.
CREATE INDEX "I_EventCharge_tenantGuid_chargeTypeId" ON "Scheduler"."EventCharge" ("tenantGuid", "chargeTypeId")
;

-- Index on the EventCharge table's tenantGuid,chargeStatusId fields.
CREATE INDEX "I_EventCharge_tenantGuid_chargeStatusId" ON "Scheduler"."EventCharge" ("tenantGuid", "chargeStatusId")
;

-- Index on the EventCharge table's tenantGuid,currencyId fields.
CREATE INDEX "I_EventCharge_tenantGuid_currencyId" ON "Scheduler"."EventCharge" ("tenantGuid", "currencyId")
;

-- Index on the EventCharge table's tenantGuid,rateTypeId fields.
CREATE INDEX "I_EventCharge_tenantGuid_rateTypeId" ON "Scheduler"."EventCharge" ("tenantGuid", "rateTypeId")
;

-- Index on the EventCharge table's tenantGuid,externalId fields.
CREATE INDEX "I_EventCharge_tenantGuid_externalId" ON "Scheduler"."EventCharge" ("tenantGuid", "externalId")
;

-- Index on the EventCharge table's tenantGuid,active fields.
CREATE INDEX "I_EventCharge_tenantGuid_active" ON "Scheduler"."EventCharge" ("tenantGuid", "active")
;

-- Index on the EventCharge table's tenantGuid,deleted fields.
CREATE INDEX "I_EventCharge_tenantGuid_deleted" ON "Scheduler"."EventCharge" ("tenantGuid", "deleted")
;

-- Index on the EventCharge table's tenantGuid,taxCodeId fields.
CREATE INDEX "I_EventCharge_tenantGuid_taxCodeId" ON "Scheduler"."EventCharge" ("tenantGuid", "taxCodeId")
;


-- The change history for records from the EventCharge table.
CREATE TABLE "Scheduler"."EventChargeChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"eventChargeId" INT NOT NULL,		-- Link to the EventCharge table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "eventChargeId" FOREIGN KEY ("eventChargeId") REFERENCES "Scheduler"."EventCharge"("id")		-- Foreign key to the EventCharge table.
);
-- Index on the EventChargeChangeHistory table's tenantGuid field.
CREATE INDEX "I_EventChargeChangeHistory_tenantGuid" ON "Scheduler"."EventChargeChangeHistory" ("tenantGuid")
;

-- Index on the EventChargeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_EventChargeChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."EventChargeChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the EventChargeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_EventChargeChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."EventChargeChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the EventChargeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_EventChargeChangeHistory_tenantGuid_userId" ON "Scheduler"."EventChargeChangeHistory" ("tenantGuid", "userId")
;

-- Index on the EventChargeChangeHistory table's tenantGuid,eventChargeId fields.
CREATE INDEX "I_EventChargeChangeHistory_tenantGuid_eventChargeId" ON "Scheduler"."EventChargeChangeHistory" ("tenantGuid", "eventChargeId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
 PERIOD STATUS
 Fiscal period workflow states (Open, In Review, Closed).
 System-defined reference data — not tenant-specific.

 DESIGN NOTE: Controls the accounting period lifecycle. Open periods accept transactions,
 In Review periods are being reconciled, Closed periods are finalized.
 =====================================================================================================
*/
CREATE TABLE "Scheduler"."PeriodStatus"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"color" VARCHAR(10) NULL,		-- Hex color for UI display.
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the PeriodStatus table's name field.
CREATE INDEX "I_PeriodStatus_name" ON "Scheduler"."PeriodStatus" ("name")
;

-- Index on the PeriodStatus table's active field.
CREATE INDEX "I_PeriodStatus_active" ON "Scheduler"."PeriodStatus" ("active")
;

-- Index on the PeriodStatus table's deleted field.
CREATE INDEX "I_PeriodStatus_deleted" ON "Scheduler"."PeriodStatus" ("deleted")
;

INSERT INTO "Scheduler"."PeriodStatus" ( "name", "description", "color", "sequence", "objectGuid" ) VALUES  ( 'Open', 'Period is open and accepting transactions.', '#4CAF50', 1, 'b2c3d4e5-0001-4000-8000-000000000001' );

INSERT INTO "Scheduler"."PeriodStatus" ( "name", "description", "color", "sequence", "objectGuid" ) VALUES  ( 'In Review', 'Period is being reviewed and reconciled. No new transactions.', '#FF9800', 2, 'b2c3d4e5-0001-4000-8000-000000000002' );

INSERT INTO "Scheduler"."PeriodStatus" ( "name", "description", "color", "sequence", "objectGuid" ) VALUES  ( 'Closed', 'Period is finalized. No modifications allowed.', '#F44336', 3, 'b2c3d4e5-0001-4000-8000-000000000003' );


/*
====================================================================================================
 FISCAL PERIOD
 Tracks accounting periods (months, quarters, or custom periods) for financial reporting.
 Supports period-close controls to prevent modifications to finalized periods.

 DESIGN NOTE: Allows both calendar-year and fiscal-year configurations.
 periodStatusId links to PeriodStatus for workflow state (Open, In Review, Closed).
 =====================================================================================================
*/
CREATE TABLE "Scheduler"."FiscalPeriod"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"startDate" TIMESTAMP NOT NULL,		-- Period start date (inclusive).
	"endDate" TIMESTAMP NOT NULL,		-- Period end date (inclusive).
	"periodType" VARCHAR(50) NOT NULL DEFAULT 'Month',		-- Period type: Month, Quarter, Year, Custom.
	"fiscalYear" INT NOT NULL,		-- The fiscal year this period belongs to.
	"periodNumber" INT NOT NULL,		-- Period number within the fiscal year (1-12 for months, 1-4 for quarters, 1 for year).
	"periodStatusId" INT NOT NULL,		-- Link to PeriodStatus — workflow state (Open, In Review, Closed). Replaces the old periodStatus string field.
	"closedDate" TIMESTAMP NULL,		-- When the period was closed.
	"closedBy" VARCHAR(100) NULL,		-- User who closed the period.
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "periodStatusId" FOREIGN KEY ("periodStatusId") REFERENCES "Scheduler"."PeriodStatus"("id"),		-- Foreign key to the PeriodStatus table.
	CONSTRAINT "UC_FiscalPeriod_tenantGuid_name" UNIQUE ( "tenantGuid", "name") ,		-- Uniqueness enforced on the FiscalPeriod table's tenantGuid and name fields.
	CONSTRAINT "UC_FiscalPeriod_tenantGuid_fiscalYear_periodNumber" UNIQUE ( "tenantGuid", "fiscalYear", "periodNumber") 		-- Uniqueness enforced on the FiscalPeriod table's tenantGuid and fiscalYear and periodNumber fields.
);
-- Index on the FiscalPeriod table's tenantGuid field.
CREATE INDEX "I_FiscalPeriod_tenantGuid" ON "Scheduler"."FiscalPeriod" ("tenantGuid")
;

-- Index on the FiscalPeriod table's tenantGuid,name fields.
CREATE INDEX "I_FiscalPeriod_tenantGuid_name" ON "Scheduler"."FiscalPeriod" ("tenantGuid", "name")
;

-- Index on the FiscalPeriod table's tenantGuid,periodStatusId fields.
CREATE INDEX "I_FiscalPeriod_tenantGuid_periodStatusId" ON "Scheduler"."FiscalPeriod" ("tenantGuid", "periodStatusId")
;

-- Index on the FiscalPeriod table's tenantGuid,active fields.
CREATE INDEX "I_FiscalPeriod_tenantGuid_active" ON "Scheduler"."FiscalPeriod" ("tenantGuid", "active")
;

-- Index on the FiscalPeriod table's tenantGuid,deleted fields.
CREATE INDEX "I_FiscalPeriod_tenantGuid_deleted" ON "Scheduler"."FiscalPeriod" ("tenantGuid", "deleted")
;


-- The change history for records from the FiscalPeriod table.
CREATE TABLE "Scheduler"."FiscalPeriodChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"fiscalPeriodId" INT NOT NULL,		-- Link to the FiscalPeriod table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "fiscalPeriodId" FOREIGN KEY ("fiscalPeriodId") REFERENCES "Scheduler"."FiscalPeriod"("id")		-- Foreign key to the FiscalPeriod table.
);
-- Index on the FiscalPeriodChangeHistory table's tenantGuid field.
CREATE INDEX "I_FiscalPeriodChangeHistory_tenantGuid" ON "Scheduler"."FiscalPeriodChangeHistory" ("tenantGuid")
;

-- Index on the FiscalPeriodChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_FiscalPeriodChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."FiscalPeriodChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the FiscalPeriodChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_FiscalPeriodChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."FiscalPeriodChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the FiscalPeriodChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_FiscalPeriodChangeHistory_tenantGuid_userId" ON "Scheduler"."FiscalPeriodChangeHistory" ("tenantGuid", "userId")
;

-- Index on the FiscalPeriodChangeHistory table's tenantGuid,fiscalPeriodId fields.
CREATE INDEX "I_FiscalPeriodChangeHistory_tenantGuid_fiscalPeriodId" ON "Scheduler"."FiscalPeriodChangeHistory" ("tenantGuid", "fiscalPeriodId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
 FINANCIAL TRANSACTION (General Ledger)
 Records individual income and expense transactions. Unlike EventCharge (which always requires a
 ScheduledEvent), FinancialTransaction can exist independently for items like cleaning labour,
 supply purchases, bank fees, grants received, bar sales, etc.

 Optionally links to a ScheduledEvent when the transaction relates to a booking.

 DESIGN NOTE: Amount is always stored as a positive value; direction (income vs expense) is
 determined by the linked FinancialCategory.accountType.
 =====================================================================================================
*/
CREATE TABLE "Scheduler"."FinancialTransaction"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"financialCategoryId" INT NOT NULL,		-- Link to the FinancialCategory (chart of accounts entry).
	"financialOfficeId" INT NULL,		-- Optional link to FinancialOffice — scopes this transaction to a specific department/committee. Can be inherited from the FinancialCategory if not set directly.
	"scheduledEventId" INT NULL,		-- Optional link to a ScheduledEvent when the transaction relates to a booking.
	"contactId" INT NULL,		-- Optional link to the Contact who paid or was paid.
	"clientId" INT NULL,		-- Optional link to the Client (vendor, customer, or supplier) for this transaction.
	"contactRole" VARCHAR(50) NULL DEFAULT 'Customer',		-- Role of the linked contact: Customer, Vendor, Employee. Maps to QuickBooks entity types for sync.
	"taxCodeId" INT NULL,		-- Optional link to TaxCode. Overrides the category-level isTaxApplicable for precise tax handling.
	"fiscalPeriodId" INT NULL,		-- Optional link to FiscalPeriod. Auto-assigned based on transactionDate when null.
	"paymentTypeId" INT NULL,		-- Optional link to PaymentType indicating how payment was made (e-transfer, cash, cheque, card, etc.).
	"transactionDate" TIMESTAMP NOT NULL,		-- When the transaction occurred (UTC).
	"description" VARCHAR(500) NOT NULL,		-- Description of the transaction (e.g., 'Easter Brunch Food', 'DD Refund - Natasha Chafe').
	"amount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Transaction amount before tax. Always positive — direction determined by FinancialCategory.accountType.
	"taxAmount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Tax amount (e.g., HST). Calculated from TaxCode.rate when applicable.
	"totalAmount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Total amount inclusive of tax (amount + taxAmount).
	"isRevenue" BOOLEAN NOT NULL DEFAULT true,		-- Denormalized from FinancialCategory.accountType for query performance. True when accountType = 'Income', false otherwise.
	"journalEntryType" VARCHAR(50) NULL,		-- Double-entry type for accounting integration: Debit or Credit. Null = auto-determined from isRevenue.
	"referenceNumber" VARCHAR(100) NULL,		-- Cheque number, e-transfer reference, receipt number, etc.
	"notes" TEXT NULL,		-- Optional notes about the transaction.
	"currencyId" INT NOT NULL,		-- Link to Currency table.
	"exportedDate" TIMESTAMP NULL,		-- When this transaction was last exported for reporting (null = not exported yet).
	"externalId" VARCHAR(100) NULL,		-- Identifier from external system (e.g., QuickBooks Transaction ID).
	"externalSystemName" VARCHAR(50) NULL,		-- Name of the external system (e.g., 'QuickBooks', 'Xero') for multi-system tracking.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "financialCategoryId" FOREIGN KEY ("financialCategoryId") REFERENCES "Scheduler"."FinancialCategory"("id"),		-- Foreign key to the FinancialCategory table.
	CONSTRAINT "financialOfficeId" FOREIGN KEY ("financialOfficeId") REFERENCES "Scheduler"."FinancialOffice"("id"),		-- Foreign key to the FinancialOffice table.
	CONSTRAINT "scheduledEventId" FOREIGN KEY ("scheduledEventId") REFERENCES "Scheduler"."ScheduledEvent"("id"),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT "contactId" FOREIGN KEY ("contactId") REFERENCES "Scheduler"."Contact"("id"),		-- Foreign key to the Contact table.
	CONSTRAINT "clientId" FOREIGN KEY ("clientId") REFERENCES "Scheduler"."Client"("id"),		-- Foreign key to the Client table.
	CONSTRAINT "taxCodeId" FOREIGN KEY ("taxCodeId") REFERENCES "Scheduler"."TaxCode"("id"),		-- Foreign key to the TaxCode table.
	CONSTRAINT "fiscalPeriodId" FOREIGN KEY ("fiscalPeriodId") REFERENCES "Scheduler"."FiscalPeriod"("id"),		-- Foreign key to the FiscalPeriod table.
	CONSTRAINT "paymentTypeId" FOREIGN KEY ("paymentTypeId") REFERENCES "Scheduler"."PaymentType"("id"),		-- Foreign key to the PaymentType table.
	CONSTRAINT "currencyId" FOREIGN KEY ("currencyId") REFERENCES "Scheduler"."Currency"("id")		-- Foreign key to the Currency table.
);
-- Index on the FinancialTransaction table's tenantGuid field.
CREATE INDEX "I_FinancialTransaction_tenantGuid" ON "Scheduler"."FinancialTransaction" ("tenantGuid")
;

-- Index on the FinancialTransaction table's tenantGuid,financialCategoryId fields.
CREATE INDEX "I_FinancialTransaction_tenantGuid_financialCategoryId" ON "Scheduler"."FinancialTransaction" ("tenantGuid", "financialCategoryId")
;

-- Index on the FinancialTransaction table's tenantGuid,financialOfficeId fields.
CREATE INDEX "I_FinancialTransaction_tenantGuid_financialOfficeId" ON "Scheduler"."FinancialTransaction" ("tenantGuid", "financialOfficeId")
;

-- Index on the FinancialTransaction table's tenantGuid,scheduledEventId fields.
CREATE INDEX "I_FinancialTransaction_tenantGuid_scheduledEventId" ON "Scheduler"."FinancialTransaction" ("tenantGuid", "scheduledEventId")
;

-- Index on the FinancialTransaction table's tenantGuid,contactId fields.
CREATE INDEX "I_FinancialTransaction_tenantGuid_contactId" ON "Scheduler"."FinancialTransaction" ("tenantGuid", "contactId")
;

-- Index on the FinancialTransaction table's tenantGuid,clientId fields.
CREATE INDEX "I_FinancialTransaction_tenantGuid_clientId" ON "Scheduler"."FinancialTransaction" ("tenantGuid", "clientId")
;

-- Index on the FinancialTransaction table's tenantGuid,taxCodeId fields.
CREATE INDEX "I_FinancialTransaction_tenantGuid_taxCodeId" ON "Scheduler"."FinancialTransaction" ("tenantGuid", "taxCodeId")
;

-- Index on the FinancialTransaction table's tenantGuid,fiscalPeriodId fields.
CREATE INDEX "I_FinancialTransaction_tenantGuid_fiscalPeriodId" ON "Scheduler"."FinancialTransaction" ("tenantGuid", "fiscalPeriodId")
;

-- Index on the FinancialTransaction table's tenantGuid,paymentTypeId fields.
CREATE INDEX "I_FinancialTransaction_tenantGuid_paymentTypeId" ON "Scheduler"."FinancialTransaction" ("tenantGuid", "paymentTypeId")
;

-- Index on the FinancialTransaction table's tenantGuid,transactionDate fields.
CREATE INDEX "I_FinancialTransaction_tenantGuid_transactionDate" ON "Scheduler"."FinancialTransaction" ("tenantGuid", "transactionDate")
;

-- Index on the FinancialTransaction table's tenantGuid,currencyId fields.
CREATE INDEX "I_FinancialTransaction_tenantGuid_currencyId" ON "Scheduler"."FinancialTransaction" ("tenantGuid", "currencyId")
;

-- Index on the FinancialTransaction table's tenantGuid,externalId fields.
CREATE INDEX "I_FinancialTransaction_tenantGuid_externalId" ON "Scheduler"."FinancialTransaction" ("tenantGuid", "externalId")
;

-- Index on the FinancialTransaction table's tenantGuid,active fields.
CREATE INDEX "I_FinancialTransaction_tenantGuid_active" ON "Scheduler"."FinancialTransaction" ("tenantGuid", "active")
;

-- Index on the FinancialTransaction table's tenantGuid,deleted fields.
CREATE INDEX "I_FinancialTransaction_tenantGuid_deleted" ON "Scheduler"."FinancialTransaction" ("tenantGuid", "deleted")
;


-- The change history for records from the FinancialTransaction table.
CREATE TABLE "Scheduler"."FinancialTransactionChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"financialTransactionId" INT NOT NULL,		-- Link to the FinancialTransaction table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "financialTransactionId" FOREIGN KEY ("financialTransactionId") REFERENCES "Scheduler"."FinancialTransaction"("id")		-- Foreign key to the FinancialTransaction table.
);
-- Index on the FinancialTransactionChangeHistory table's tenantGuid field.
CREATE INDEX "I_FinancialTransactionChangeHistory_tenantGuid" ON "Scheduler"."FinancialTransactionChangeHistory" ("tenantGuid")
;

-- Index on the FinancialTransactionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_FinancialTransactionChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."FinancialTransactionChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the FinancialTransactionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_FinancialTransactionChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."FinancialTransactionChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the FinancialTransactionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_FinancialTransactionChangeHistory_tenantGuid_userId" ON "Scheduler"."FinancialTransactionChangeHistory" ("tenantGuid", "userId")
;

-- Index on the FinancialTransactionChangeHistory table's tenantGuid,financialTransactionId fields.
CREATE INDEX "I_FinancialTransactionChangeHistory_tenantGuid_financialTransac" ON "Scheduler"."FinancialTransactionChangeHistory" ("tenantGuid", "financialTransactionId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
 BUDGET
 Tracks budgeted amounts by FinancialCategory and FiscalPeriod.
 Enables budget-vs-actual reporting: 'We budgeted $5,000 for bar supplies this quarter,
 we have spent $3,200 so far.'

 DESIGN NOTE: One budget record per category per period. Combined with FinancialTransaction
 actuals, enables variance analysis, burn rate tracking, and forecasting.
 ====================================================================================================
*/
CREATE TABLE "Scheduler"."Budget"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"financialCategoryId" INT NOT NULL,		-- The category this budget applies to.
	"fiscalPeriodId" INT NOT NULL,		-- The fiscal period this budget covers.
	"financialOfficeId" INT NULL,		-- Optional link to FinancialOffice — scopes this budget to a specific department/committee. Can be inherited from the FinancialCategory if not set directly.
	"budgetedAmount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- The planned/budgeted amount for this category in this period.
	"revisedAmount" DECIMAL(11,2) NULL,		-- Optional revised budget amount (after mid-period adjustments).
	"notes" TEXT NULL,		-- Optional notes about the budget line (e.g., justification for revision).
	"currencyId" INT NOT NULL,		-- Link to Currency table.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "financialCategoryId" FOREIGN KEY ("financialCategoryId") REFERENCES "Scheduler"."FinancialCategory"("id"),		-- Foreign key to the FinancialCategory table.
	CONSTRAINT "fiscalPeriodId" FOREIGN KEY ("fiscalPeriodId") REFERENCES "Scheduler"."FiscalPeriod"("id"),		-- Foreign key to the FiscalPeriod table.
	CONSTRAINT "financialOfficeId" FOREIGN KEY ("financialOfficeId") REFERENCES "Scheduler"."FinancialOffice"("id"),		-- Foreign key to the FinancialOffice table.
	CONSTRAINT "currencyId" FOREIGN KEY ("currencyId") REFERENCES "Scheduler"."Currency"("id"),		-- Foreign key to the Currency table.
	CONSTRAINT "UC_Budget_tenantGuid_financialCategoryId_fiscalPeriodId" UNIQUE ( "tenantGuid", "financialCategoryId", "fiscalPeriodId") 		-- Uniqueness enforced on the Budget table's tenantGuid and financialCategoryId and fiscalPeriodId fields.
);
-- Index on the Budget table's tenantGuid field.
CREATE INDEX "I_Budget_tenantGuid" ON "Scheduler"."Budget" ("tenantGuid")
;

-- Index on the Budget table's tenantGuid,financialCategoryId fields.
CREATE INDEX "I_Budget_tenantGuid_financialCategoryId" ON "Scheduler"."Budget" ("tenantGuid", "financialCategoryId")
;

-- Index on the Budget table's tenantGuid,fiscalPeriodId fields.
CREATE INDEX "I_Budget_tenantGuid_fiscalPeriodId" ON "Scheduler"."Budget" ("tenantGuid", "fiscalPeriodId")
;

-- Index on the Budget table's tenantGuid,financialOfficeId fields.
CREATE INDEX "I_Budget_tenantGuid_financialOfficeId" ON "Scheduler"."Budget" ("tenantGuid", "financialOfficeId")
;

-- Index on the Budget table's tenantGuid,currencyId fields.
CREATE INDEX "I_Budget_tenantGuid_currencyId" ON "Scheduler"."Budget" ("tenantGuid", "currencyId")
;

-- Index on the Budget table's tenantGuid,active fields.
CREATE INDEX "I_Budget_tenantGuid_active" ON "Scheduler"."Budget" ("tenantGuid", "active")
;

-- Index on the Budget table's tenantGuid,deleted fields.
CREATE INDEX "I_Budget_tenantGuid_deleted" ON "Scheduler"."Budget" ("tenantGuid", "deleted")
;


-- The change history for records from the Budget table.
CREATE TABLE "Scheduler"."BudgetChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"budgetId" INT NOT NULL,		-- Link to the Budget table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "budgetId" FOREIGN KEY ("budgetId") REFERENCES "Scheduler"."Budget"("id")		-- Foreign key to the Budget table.
);
-- Index on the BudgetChangeHistory table's tenantGuid field.
CREATE INDEX "I_BudgetChangeHistory_tenantGuid" ON "Scheduler"."BudgetChangeHistory" ("tenantGuid")
;

-- Index on the BudgetChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_BudgetChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."BudgetChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the BudgetChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_BudgetChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."BudgetChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the BudgetChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_BudgetChangeHistory_tenantGuid_userId" ON "Scheduler"."BudgetChangeHistory" ("tenantGuid", "userId")
;

-- Index on the BudgetChangeHistory table's tenantGuid,budgetId fields.
CREATE INDEX "I_BudgetChangeHistory_tenantGuid_budgetId" ON "Scheduler"."BudgetChangeHistory" ("tenantGuid", "budgetId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
 GENERAL LEDGER ENTRY (Double-Entry Journal Entry)
 Every financial operation (expense, revenue, void) creates a balanced journal entry in this table.
 Each entry has two or more GeneralLedgerLines whose total debits must equal total credits.

 The GL is the authoritative ledger for financial reporting (trial balance, P&L, balance sheet).
 FinancialTransaction remains the source document; this table is the accounting record.

 DESIGN NOTE: reversalOfId links void/correction entries back to the original entry they reverse.
 ====================================================================================================
*/
CREATE TABLE "Scheduler"."GeneralLedgerEntry"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"journalEntryNumber" INT NOT NULL,		-- Auto-incrementing per-tenant journal entry number for human reference.
	"transactionDate" TIMESTAMP NOT NULL,		-- The date of the underlying financial event (UTC).
	"description" VARCHAR(500) NULL,		-- Description of the journal entry (e.g., 'Expense: Office Supplies', 'Revenue: Hall Rental').
	"referenceNumber" VARCHAR(100) NULL,		-- External reference — cheque number, receipt number, etc.
	"financialTransactionId" INT NULL,		-- Links back to the originating FinancialTransaction, if any.
	"fiscalPeriodId" INT NULL,		-- The fiscal period this entry belongs to.
	"financialOfficeId" INT NULL,		-- Optional link to FinancialOffice for departmental reporting.
	"postedBy" INT NOT NULL,		-- Security user id who posted this entry.
	"postedDate" TIMESTAMP NOT NULL,		-- When this entry was posted (UTC).
	"reversalOfId" INT NULL,		-- If this is a reversal/correction, points to the original GeneralLedgerEntry id.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "financialTransactionId" FOREIGN KEY ("financialTransactionId") REFERENCES "Scheduler"."FinancialTransaction"("id"),		-- Foreign key to the FinancialTransaction table.
	CONSTRAINT "fiscalPeriodId" FOREIGN KEY ("fiscalPeriodId") REFERENCES "Scheduler"."FiscalPeriod"("id"),		-- Foreign key to the FiscalPeriod table.
	CONSTRAINT "financialOfficeId" FOREIGN KEY ("financialOfficeId") REFERENCES "Scheduler"."FinancialOffice"("id")		-- Foreign key to the FinancialOffice table.
);
-- Index on the GeneralLedgerEntry table's tenantGuid field.
CREATE INDEX "I_GeneralLedgerEntry_tenantGuid" ON "Scheduler"."GeneralLedgerEntry" ("tenantGuid")
;

-- Index on the GeneralLedgerEntry table's tenantGuid,transactionDate fields.
CREATE INDEX "I_GeneralLedgerEntry_tenantGuid_transactionDate" ON "Scheduler"."GeneralLedgerEntry" ("tenantGuid", "transactionDate")
;

-- Index on the GeneralLedgerEntry table's tenantGuid,financialTransactionId fields.
CREATE INDEX "I_GeneralLedgerEntry_tenantGuid_financialTransactionId" ON "Scheduler"."GeneralLedgerEntry" ("tenantGuid", "financialTransactionId")
;

-- Index on the GeneralLedgerEntry table's tenantGuid,fiscalPeriodId fields.
CREATE INDEX "I_GeneralLedgerEntry_tenantGuid_fiscalPeriodId" ON "Scheduler"."GeneralLedgerEntry" ("tenantGuid", "fiscalPeriodId")
;

-- Index on the GeneralLedgerEntry table's tenantGuid,financialOfficeId fields.
CREATE INDEX "I_GeneralLedgerEntry_tenantGuid_financialOfficeId" ON "Scheduler"."GeneralLedgerEntry" ("tenantGuid", "financialOfficeId")
;

-- Index on the GeneralLedgerEntry table's tenantGuid,active fields.
CREATE INDEX "I_GeneralLedgerEntry_tenantGuid_active" ON "Scheduler"."GeneralLedgerEntry" ("tenantGuid", "active")
;

-- Index on the GeneralLedgerEntry table's tenantGuid,deleted fields.
CREATE INDEX "I_GeneralLedgerEntry_tenantGuid_deleted" ON "Scheduler"."GeneralLedgerEntry" ("tenantGuid", "deleted")
;


/*
====================================================================================================
 GENERAL LEDGER LINE
 Individual debit or credit line within a GeneralLedgerEntry.
 Each line posts to a specific FinancialCategory (account).

 CONSTRAINT: Within each GeneralLedgerEntry, sum(debitAmount) must equal sum(creditAmount).
 Exactly one of debitAmount/creditAmount should be non-zero per line.
 ====================================================================================================
*/
CREATE TABLE "Scheduler"."GeneralLedgerLine"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"generalLedgerEntryId" INT NOT NULL,		-- The parent journal entry this line belongs to.
	"financialCategoryId" INT NOT NULL,		-- The account (FinancialCategory) this line posts to.
	"debitAmount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Debit amount. Zero if this line is a credit.
	"creditAmount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Credit amount. Zero if this line is a debit.
	"description" VARCHAR(500) NULL,		-- Optional line-level description.
	CONSTRAINT "generalLedgerEntryId" FOREIGN KEY ("generalLedgerEntryId") REFERENCES "Scheduler"."GeneralLedgerEntry"("id"),		-- Foreign key to the GeneralLedgerEntry table.
	CONSTRAINT "financialCategoryId" FOREIGN KEY ("financialCategoryId") REFERENCES "Scheduler"."FinancialCategory"("id")		-- Foreign key to the FinancialCategory table.
);
-- Index on the GeneralLedgerLine table's generalLedgerEntryId field.
CREATE INDEX "I_GeneralLedgerLine_generalLedgerEntryId" ON "Scheduler"."GeneralLedgerLine" ("generalLedgerEntryId")
;

-- Index on the GeneralLedgerLine table's financialCategoryId field.
CREATE INDEX "I_GeneralLedgerLine_financialCategoryId" ON "Scheduler"."GeneralLedgerLine" ("financialCategoryId")
;


-- Master list of payment methods (Cash, E-Transfer, Credit Card, Debit Card, Cheque).
CREATE TABLE "Scheduler"."PaymentMethod"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"isElectronic" BOOLEAN NOT NULL DEFAULT false,		-- True for card and e-transfer, false for cash and cheque.
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"color" VARCHAR(10) NULL,		-- Hex color for UI display.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the PaymentMethod table's name field.
CREATE INDEX "I_PaymentMethod_name" ON "Scheduler"."PaymentMethod" ("name")
;

-- Index on the PaymentMethod table's active field.
CREATE INDEX "I_PaymentMethod_active" ON "Scheduler"."PaymentMethod" ("active")
;

-- Index on the PaymentMethod table's deleted field.
CREATE INDEX "I_PaymentMethod_deleted" ON "Scheduler"."PaymentMethod" ("deleted")
;

INSERT INTO "Scheduler"."PaymentMethod" ( "name", "description", "isElectronic", "sequence", "objectGuid" ) VALUES  ( 'Cash', 'Cash payment', false, 1, 'b1a1b2c3-d4e5-6789-abcd-ef0123456701' );

INSERT INTO "Scheduler"."PaymentMethod" ( "name", "description", "isElectronic", "sequence", "objectGuid" ) VALUES  ( 'E-Transfer', 'Interac e-Transfer', true, 2, 'b1a1b2c3-d4e5-6789-abcd-ef0123456702' );

INSERT INTO "Scheduler"."PaymentMethod" ( "name", "description", "isElectronic", "sequence", "objectGuid" ) VALUES  ( 'Cheque', 'Cheque payment', false, 3, 'b1a1b2c3-d4e5-6789-abcd-ef0123456703' );

INSERT INTO "Scheduler"."PaymentMethod" ( "name", "description", "isElectronic", "sequence", "objectGuid" ) VALUES  ( 'Credit Card', 'Credit card payment', true, 4, 'b1a1b2c3-d4e5-6789-abcd-ef0123456704' );

INSERT INTO "Scheduler"."PaymentMethod" ( "name", "description", "isElectronic", "sequence", "objectGuid" ) VALUES  ( 'Debit Card', 'Debit card payment', true, 5, 'b1a1b2c3-d4e5-6789-abcd-ef0123456705' );


/*
====================================================================================================
 PAYMENT PROVIDER
 Configuration for electronic payment processor integrations (Stripe, Square, or Manual).
 Stores encrypted API keys and merchant account details.

 DESIGN NOTE: Starts with a 'Manual' provider for recording cash/cheque payments.
 Add Stripe/Square providers when ready for electronic payment acceptance.
 ====================================================================================================
*/
CREATE TABLE "Scheduler"."PaymentProvider"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"providerType" VARCHAR(50) NOT NULL,		-- Provider type identifier: 'manual', 'stripe', 'square', 'moneris'.
	"isActive" BOOLEAN NOT NULL DEFAULT true,		-- Whether this provider is currently active.
	"apiKeyEncrypted" TEXT NULL,		-- Encrypted API key for the payment provider.
	"merchantId" VARCHAR(100) NULL,		-- Merchant account identifier with the provider.
	"webhookSecret" TEXT NULL,		-- Encrypted webhook validation secret for the provider.
	"processingFeePercent" NUMERIC(38,22) NULL,		-- Provider processing fee percentage (e.g., 2.9 for Stripe).
	"processingFeeFixed" DECIMAL(11,2) NULL,		-- Provider fixed processing fee per transaction (e.g., $0.30).
	"notes" TEXT NULL,		-- Optional notes about the provider configuration.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_PaymentProvider_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the PaymentProvider table's tenantGuid and name fields.
);
-- Index on the PaymentProvider table's tenantGuid field.
CREATE INDEX "I_PaymentProvider_tenantGuid" ON "Scheduler"."PaymentProvider" ("tenantGuid")
;

-- Index on the PaymentProvider table's tenantGuid,name fields.
CREATE INDEX "I_PaymentProvider_tenantGuid_name" ON "Scheduler"."PaymentProvider" ("tenantGuid", "name")
;

-- Index on the PaymentProvider table's tenantGuid,active fields.
CREATE INDEX "I_PaymentProvider_tenantGuid_active" ON "Scheduler"."PaymentProvider" ("tenantGuid", "active")
;

-- Index on the PaymentProvider table's tenantGuid,deleted fields.
CREATE INDEX "I_PaymentProvider_tenantGuid_deleted" ON "Scheduler"."PaymentProvider" ("tenantGuid", "deleted")
;


-- The change history for records from the PaymentProvider table.
CREATE TABLE "Scheduler"."PaymentProviderChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"paymentProviderId" INT NOT NULL,		-- Link to the PaymentProvider table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "paymentProviderId" FOREIGN KEY ("paymentProviderId") REFERENCES "Scheduler"."PaymentProvider"("id")		-- Foreign key to the PaymentProvider table.
);
-- Index on the PaymentProviderChangeHistory table's tenantGuid field.
CREATE INDEX "I_PaymentProviderChangeHistory_tenantGuid" ON "Scheduler"."PaymentProviderChangeHistory" ("tenantGuid")
;

-- Index on the PaymentProviderChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_PaymentProviderChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."PaymentProviderChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the PaymentProviderChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_PaymentProviderChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."PaymentProviderChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the PaymentProviderChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_PaymentProviderChangeHistory_tenantGuid_userId" ON "Scheduler"."PaymentProviderChangeHistory" ("tenantGuid", "userId")
;

-- Index on the PaymentProviderChangeHistory table's tenantGuid,paymentProviderId fields.
CREATE INDEX "I_PaymentProviderChangeHistory_tenantGuid_paymentProviderId" ON "Scheduler"."PaymentProviderChangeHistory" ("tenantGuid", "paymentProviderId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
 PAYMENT TRANSACTION
 Records individual payments received or made. Links to PaymentMethod (how) and optionally to
 PaymentProvider (which processor). Can be associated with a ScheduledEvent, FinancialTransaction,
 or EventCharge.

 DESIGN NOTE: Supports both manual recording (cash register replacement) and electronic payment
 processing (Stripe/Square integration). The providerTransactionId and providerResponse fields
 store the raw response from electronic providers for audit purposes.
 ====================================================================================================
*/
CREATE TABLE "Scheduler"."PaymentTransaction"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"paymentMethodId" INT NOT NULL,		-- How the payment was made (Cash, E-Transfer, Credit Card, etc.).
	"paymentProviderId" INT NULL,		-- Optional link to payment processor (null for cash/cheque).
	"scheduledEventId" INT NULL,		-- Optional link to a ScheduledEvent (e.g., booking payment).
	"financialTransactionId" INT NULL,		-- Optional link to a FinancialTransaction (e.g., bar tab payment).
	"eventChargeId" INT NULL,		-- Optional link to a specific EventCharge (e.g., damage deposit payment).
	"transactionDate" TIMESTAMP NOT NULL,		-- When the payment occurred (UTC).
	"amount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Gross payment amount.
	"processingFee" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Fee deducted by the payment provider.
	"netAmount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Net amount received (amount - processingFee).
	"currencyId" INT NOT NULL,		-- Link to Currency table.
	"status" VARCHAR(50) NOT NULL,		-- Payment status: pending, completed, failed, refunded.
	"providerTransactionId" VARCHAR(250) NULL,		-- Transaction ID from the payment provider (e.g., Stripe charge ID).
	"providerResponse" TEXT NULL,		-- JSON response from the payment provider for audit purposes.
	"payerName" VARCHAR(250) NULL,		-- Name of the person who paid.
	"payerEmail" VARCHAR(250) NULL,		-- Email of the payer.
	"payerPhone" VARCHAR(50) NULL,		-- Phone number of the payer.
	"receiptNumber" VARCHAR(100) NULL,		-- Generated receipt number.
	"notes" TEXT NULL,		-- Optional notes about the payment.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "paymentMethodId" FOREIGN KEY ("paymentMethodId") REFERENCES "Scheduler"."PaymentMethod"("id"),		-- Foreign key to the PaymentMethod table.
	CONSTRAINT "paymentProviderId" FOREIGN KEY ("paymentProviderId") REFERENCES "Scheduler"."PaymentProvider"("id"),		-- Foreign key to the PaymentProvider table.
	CONSTRAINT "scheduledEventId" FOREIGN KEY ("scheduledEventId") REFERENCES "Scheduler"."ScheduledEvent"("id"),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT "financialTransactionId" FOREIGN KEY ("financialTransactionId") REFERENCES "Scheduler"."FinancialTransaction"("id"),		-- Foreign key to the FinancialTransaction table.
	CONSTRAINT "eventChargeId" FOREIGN KEY ("eventChargeId") REFERENCES "Scheduler"."EventCharge"("id"),		-- Foreign key to the EventCharge table.
	CONSTRAINT "currencyId" FOREIGN KEY ("currencyId") REFERENCES "Scheduler"."Currency"("id")		-- Foreign key to the Currency table.
);
-- Index on the PaymentTransaction table's tenantGuid field.
CREATE INDEX "I_PaymentTransaction_tenantGuid" ON "Scheduler"."PaymentTransaction" ("tenantGuid")
;

-- Index on the PaymentTransaction table's tenantGuid,paymentMethodId fields.
CREATE INDEX "I_PaymentTransaction_tenantGuid_paymentMethodId" ON "Scheduler"."PaymentTransaction" ("tenantGuid", "paymentMethodId")
;

-- Index on the PaymentTransaction table's tenantGuid,paymentProviderId fields.
CREATE INDEX "I_PaymentTransaction_tenantGuid_paymentProviderId" ON "Scheduler"."PaymentTransaction" ("tenantGuid", "paymentProviderId")
;

-- Index on the PaymentTransaction table's tenantGuid,scheduledEventId fields.
CREATE INDEX "I_PaymentTransaction_tenantGuid_scheduledEventId" ON "Scheduler"."PaymentTransaction" ("tenantGuid", "scheduledEventId")
;

-- Index on the PaymentTransaction table's tenantGuid,financialTransactionId fields.
CREATE INDEX "I_PaymentTransaction_tenantGuid_financialTransactionId" ON "Scheduler"."PaymentTransaction" ("tenantGuid", "financialTransactionId")
;

-- Index on the PaymentTransaction table's tenantGuid,eventChargeId fields.
CREATE INDEX "I_PaymentTransaction_tenantGuid_eventChargeId" ON "Scheduler"."PaymentTransaction" ("tenantGuid", "eventChargeId")
;

-- Index on the PaymentTransaction table's tenantGuid,transactionDate fields.
CREATE INDEX "I_PaymentTransaction_tenantGuid_transactionDate" ON "Scheduler"."PaymentTransaction" ("tenantGuid", "transactionDate")
;

-- Index on the PaymentTransaction table's tenantGuid,currencyId fields.
CREATE INDEX "I_PaymentTransaction_tenantGuid_currencyId" ON "Scheduler"."PaymentTransaction" ("tenantGuid", "currencyId")
;

-- Index on the PaymentTransaction table's tenantGuid,providerTransactionId fields.
CREATE INDEX "I_PaymentTransaction_tenantGuid_providerTransactionId" ON "Scheduler"."PaymentTransaction" ("tenantGuid", "providerTransactionId")
;

-- Index on the PaymentTransaction table's tenantGuid,receiptNumber fields.
CREATE INDEX "I_PaymentTransaction_tenantGuid_receiptNumber" ON "Scheduler"."PaymentTransaction" ("tenantGuid", "receiptNumber")
;

-- Index on the PaymentTransaction table's tenantGuid,active fields.
CREATE INDEX "I_PaymentTransaction_tenantGuid_active" ON "Scheduler"."PaymentTransaction" ("tenantGuid", "active")
;

-- Index on the PaymentTransaction table's tenantGuid,deleted fields.
CREATE INDEX "I_PaymentTransaction_tenantGuid_deleted" ON "Scheduler"."PaymentTransaction" ("tenantGuid", "deleted")
;


-- The change history for records from the PaymentTransaction table.
CREATE TABLE "Scheduler"."PaymentTransactionChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"paymentTransactionId" INT NOT NULL,		-- Link to the PaymentTransaction table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "paymentTransactionId" FOREIGN KEY ("paymentTransactionId") REFERENCES "Scheduler"."PaymentTransaction"("id")		-- Foreign key to the PaymentTransaction table.
);
-- Index on the PaymentTransactionChangeHistory table's tenantGuid field.
CREATE INDEX "I_PaymentTransactionChangeHistory_tenantGuid" ON "Scheduler"."PaymentTransactionChangeHistory" ("tenantGuid")
;

-- Index on the PaymentTransactionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_PaymentTransactionChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."PaymentTransactionChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the PaymentTransactionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_PaymentTransactionChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."PaymentTransactionChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the PaymentTransactionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_PaymentTransactionChangeHistory_tenantGuid_userId" ON "Scheduler"."PaymentTransactionChangeHistory" ("tenantGuid", "userId")
;

-- Index on the PaymentTransactionChangeHistory table's tenantGuid,paymentTransactionId fields.
CREATE INDEX "I_PaymentTransactionChangeHistory_tenantGuid_paymentTransaction" ON "Scheduler"."PaymentTransactionChangeHistory" ("tenantGuid", "paymentTransactionId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
 INVOICE STATUS
 Workflow states for invoices (Draft, Sent, Partially Paid, Paid, Overdue, Cancelled, Void).
 System-defined reference data — not tenant-specific.
 ====================================================================================================
*/
CREATE TABLE "Scheduler"."InvoiceStatus"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"color" VARCHAR(10) NULL,		-- Hex color for UI display.
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the InvoiceStatus table's name field.
CREATE INDEX "I_InvoiceStatus_name" ON "Scheduler"."InvoiceStatus" ("name")
;

-- Index on the InvoiceStatus table's active field.
CREATE INDEX "I_InvoiceStatus_active" ON "Scheduler"."InvoiceStatus" ("active")
;

-- Index on the InvoiceStatus table's deleted field.
CREATE INDEX "I_InvoiceStatus_deleted" ON "Scheduler"."InvoiceStatus" ("deleted")
;

INSERT INTO "Scheduler"."InvoiceStatus" ( "name", "description", "color", "sequence", "objectGuid" ) VALUES  ( 'Draft', 'Invoice created but not yet sent to client', '#9E9E9E', 1, 'b1c2d3e4-0001-4000-9000-000000000001' );

INSERT INTO "Scheduler"."InvoiceStatus" ( "name", "description", "color", "sequence", "objectGuid" ) VALUES  ( 'Sent', 'Invoice has been sent to the client', '#2196F3', 2, 'b1c2d3e4-0001-4000-9000-000000000002' );

INSERT INTO "Scheduler"."InvoiceStatus" ( "name", "description", "color", "sequence", "objectGuid" ) VALUES  ( 'Partially Paid', 'Client has made a partial payment', '#FF9800', 3, 'b1c2d3e4-0001-4000-9000-000000000003' );

INSERT INTO "Scheduler"."InvoiceStatus" ( "name", "description", "color", "sequence", "objectGuid" ) VALUES  ( 'Paid', 'Invoice has been fully paid', '#4CAF50', 4, 'b1c2d3e4-0001-4000-9000-000000000004' );

INSERT INTO "Scheduler"."InvoiceStatus" ( "name", "description", "color", "sequence", "objectGuid" ) VALUES  ( 'Overdue', 'Invoice is past due date and unpaid', '#F44336', 5, 'b1c2d3e4-0001-4000-9000-000000000005' );

INSERT INTO "Scheduler"."InvoiceStatus" ( "name", "description", "color", "sequence", "objectGuid" ) VALUES  ( 'Cancelled', 'Invoice has been cancelled', '#795548', 6, 'b1c2d3e4-0001-4000-9000-000000000006' );

INSERT INTO "Scheduler"."InvoiceStatus" ( "name", "description", "color", "sequence", "objectGuid" ) VALUES  ( 'Void', 'Invoice has been voided and should be disregarded', '#607D8B', 7, 'b1c2d3e4-0001-4000-9000-000000000007' );


/*
====================================================================================================
 INVOICE
 Formal billing document issued to a client for services rendered (e.g., hall rental fees).
 Links to a Client (who is being billed), optionally to a ScheduledEvent and FinancialOffice.
 Line items are stored in InvoiceLineItem child table.

 DESIGN NOTE: The invoiceNumber is auto-generated using the tenant's invoiceNumberMask from
 TenantProfile (e.g., 'INV-2025-0001'). Generated PDF documents are stored via the Document
 table with the invoiceId FK set, providing a permanent archive of what was sent.
 ====================================================================================================
*/
CREATE TABLE "Scheduler"."Invoice"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"invoiceNumber" VARCHAR(50) NOT NULL,		-- Auto-generated sequential invoice number (e.g., 'INV-2025-0001').
	"clientId" INT NOT NULL,		-- The client being invoiced.
	"contactId" INT NULL,		-- Optional billing contact person.
	"scheduledEventId" INT NULL,		-- Optional link to the event this invoice relates to.
	"financialOfficeId" INT NULL,		-- Optional issuing financial office.
	"invoiceStatusId" INT NOT NULL,		-- Current invoice status (Draft, Sent, Paid, etc.).
	"currencyId" INT NOT NULL,		-- Currency for all amounts on this invoice.
	"taxCodeId" INT NULL,		-- Default tax code applied to line items.
	"invoiceDate" TIMESTAMP NOT NULL,		-- Date the invoice was issued (UTC).
	"dueDate" TIMESTAMP NOT NULL,		-- Payment due date (UTC).
	"subtotal" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Sum of line item amounts before tax.
	"taxAmount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Total tax amount.
	"totalAmount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Grand total (subtotal + taxAmount).
	"amountPaid" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Running total of payments received against this invoice.
	"sentDate" TIMESTAMP NULL,		-- When the invoice was sent to the client (null = not sent).
	"paidDate" TIMESTAMP NULL,		-- When the invoice was fully paid (null = not yet paid).
	"notes" TEXT NULL,		-- Optional notes or payment terms.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "clientId" FOREIGN KEY ("clientId") REFERENCES "Scheduler"."Client"("id"),		-- Foreign key to the Client table.
	CONSTRAINT "contactId" FOREIGN KEY ("contactId") REFERENCES "Scheduler"."Contact"("id"),		-- Foreign key to the Contact table.
	CONSTRAINT "scheduledEventId" FOREIGN KEY ("scheduledEventId") REFERENCES "Scheduler"."ScheduledEvent"("id"),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT "financialOfficeId" FOREIGN KEY ("financialOfficeId") REFERENCES "Scheduler"."FinancialOffice"("id"),		-- Foreign key to the FinancialOffice table.
	CONSTRAINT "invoiceStatusId" FOREIGN KEY ("invoiceStatusId") REFERENCES "Scheduler"."InvoiceStatus"("id"),		-- Foreign key to the InvoiceStatus table.
	CONSTRAINT "currencyId" FOREIGN KEY ("currencyId") REFERENCES "Scheduler"."Currency"("id"),		-- Foreign key to the Currency table.
	CONSTRAINT "taxCodeId" FOREIGN KEY ("taxCodeId") REFERENCES "Scheduler"."TaxCode"("id"),		-- Foreign key to the TaxCode table.
	CONSTRAINT "UC_Invoice_tenantGuid_invoiceNumber" UNIQUE ( "tenantGuid", "invoiceNumber") 		-- Uniqueness enforced on the Invoice table's tenantGuid and invoiceNumber fields.
);
-- Index on the Invoice table's tenantGuid field.
CREATE INDEX "I_Invoice_tenantGuid" ON "Scheduler"."Invoice" ("tenantGuid")
;

-- Index on the Invoice table's tenantGuid,invoiceNumber fields.
CREATE INDEX "I_Invoice_tenantGuid_invoiceNumber" ON "Scheduler"."Invoice" ("tenantGuid", "invoiceNumber")
;

-- Index on the Invoice table's tenantGuid,clientId fields.
CREATE INDEX "I_Invoice_tenantGuid_clientId" ON "Scheduler"."Invoice" ("tenantGuid", "clientId")
;

-- Index on the Invoice table's tenantGuid,contactId fields.
CREATE INDEX "I_Invoice_tenantGuid_contactId" ON "Scheduler"."Invoice" ("tenantGuid", "contactId")
;

-- Index on the Invoice table's tenantGuid,scheduledEventId fields.
CREATE INDEX "I_Invoice_tenantGuid_scheduledEventId" ON "Scheduler"."Invoice" ("tenantGuid", "scheduledEventId")
;

-- Index on the Invoice table's tenantGuid,financialOfficeId fields.
CREATE INDEX "I_Invoice_tenantGuid_financialOfficeId" ON "Scheduler"."Invoice" ("tenantGuid", "financialOfficeId")
;

-- Index on the Invoice table's tenantGuid,invoiceStatusId fields.
CREATE INDEX "I_Invoice_tenantGuid_invoiceStatusId" ON "Scheduler"."Invoice" ("tenantGuid", "invoiceStatusId")
;

-- Index on the Invoice table's tenantGuid,currencyId fields.
CREATE INDEX "I_Invoice_tenantGuid_currencyId" ON "Scheduler"."Invoice" ("tenantGuid", "currencyId")
;

-- Index on the Invoice table's tenantGuid,taxCodeId fields.
CREATE INDEX "I_Invoice_tenantGuid_taxCodeId" ON "Scheduler"."Invoice" ("tenantGuid", "taxCodeId")
;

-- Index on the Invoice table's tenantGuid,invoiceDate fields.
CREATE INDEX "I_Invoice_tenantGuid_invoiceDate" ON "Scheduler"."Invoice" ("tenantGuid", "invoiceDate")
;

-- Index on the Invoice table's tenantGuid,active fields.
CREATE INDEX "I_Invoice_tenantGuid_active" ON "Scheduler"."Invoice" ("tenantGuid", "active")
;

-- Index on the Invoice table's tenantGuid,deleted fields.
CREATE INDEX "I_Invoice_tenantGuid_deleted" ON "Scheduler"."Invoice" ("tenantGuid", "deleted")
;


-- The change history for records from the Invoice table.
CREATE TABLE "Scheduler"."InvoiceChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"invoiceId" INT NOT NULL,		-- Link to the Invoice table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "invoiceId" FOREIGN KEY ("invoiceId") REFERENCES "Scheduler"."Invoice"("id")		-- Foreign key to the Invoice table.
);
-- Index on the InvoiceChangeHistory table's tenantGuid field.
CREATE INDEX "I_InvoiceChangeHistory_tenantGuid" ON "Scheduler"."InvoiceChangeHistory" ("tenantGuid")
;

-- Index on the InvoiceChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_InvoiceChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."InvoiceChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the InvoiceChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_InvoiceChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."InvoiceChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the InvoiceChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_InvoiceChangeHistory_tenantGuid_userId" ON "Scheduler"."InvoiceChangeHistory" ("tenantGuid", "userId")
;

-- Index on the InvoiceChangeHistory table's tenantGuid,invoiceId fields.
CREATE INDEX "I_InvoiceChangeHistory_tenantGuid_invoiceId" ON "Scheduler"."InvoiceChangeHistory" ("tenantGuid", "invoiceId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
 INVOICE LINE ITEM
 Individual billable items on an invoice. Optionally links back to the source EventCharge
 and/or FinancialCategory for categorization and audit trail.
 ====================================================================================================
*/
CREATE TABLE "Scheduler"."InvoiceLineItem"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"invoiceId" INT NOT NULL,		-- Parent invoice.
	"eventChargeId" INT NULL,		-- Optional link to the source EventCharge this line item was created from.
	"financialCategoryId" INT NULL,		-- Optional revenue category for reporting.
	"description" VARCHAR(500) NOT NULL,		-- Line item description (e.g., 'Hall Rental - Saturday Dec 14').
	"quantity" NUMERIC(38,22) NOT NULL DEFAULT 1,		-- Quantity (hours, units, etc.).
	"unitPrice" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Price per unit.
	"amount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Extended amount (quantity × unitPrice).
	"taxAmount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Tax for this line item.
	"totalAmount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Line total (amount + taxAmount).
	"sequence" INT NULL,		-- Display order on the invoice.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "invoiceId" FOREIGN KEY ("invoiceId") REFERENCES "Scheduler"."Invoice"("id"),		-- Foreign key to the Invoice table.
	CONSTRAINT "eventChargeId" FOREIGN KEY ("eventChargeId") REFERENCES "Scheduler"."EventCharge"("id"),		-- Foreign key to the EventCharge table.
	CONSTRAINT "financialCategoryId" FOREIGN KEY ("financialCategoryId") REFERENCES "Scheduler"."FinancialCategory"("id")		-- Foreign key to the FinancialCategory table.
);
-- Index on the InvoiceLineItem table's tenantGuid field.
CREATE INDEX "I_InvoiceLineItem_tenantGuid" ON "Scheduler"."InvoiceLineItem" ("tenantGuid")
;

-- Index on the InvoiceLineItem table's tenantGuid,invoiceId fields.
CREATE INDEX "I_InvoiceLineItem_tenantGuid_invoiceId" ON "Scheduler"."InvoiceLineItem" ("tenantGuid", "invoiceId")
;

-- Index on the InvoiceLineItem table's tenantGuid,eventChargeId fields.
CREATE INDEX "I_InvoiceLineItem_tenantGuid_eventChargeId" ON "Scheduler"."InvoiceLineItem" ("tenantGuid", "eventChargeId")
;

-- Index on the InvoiceLineItem table's tenantGuid,financialCategoryId fields.
CREATE INDEX "I_InvoiceLineItem_tenantGuid_financialCategoryId" ON "Scheduler"."InvoiceLineItem" ("tenantGuid", "financialCategoryId")
;

-- Index on the InvoiceLineItem table's tenantGuid,active fields.
CREATE INDEX "I_InvoiceLineItem_tenantGuid_active" ON "Scheduler"."InvoiceLineItem" ("tenantGuid", "active")
;

-- Index on the InvoiceLineItem table's tenantGuid,deleted fields.
CREATE INDEX "I_InvoiceLineItem_tenantGuid_deleted" ON "Scheduler"."InvoiceLineItem" ("tenantGuid", "deleted")
;


/*
====================================================================================================
 RECEIPT
 Proof of payment issued when a payment is received. Optionally links to an Invoice,
 PaymentTransaction, or FinancialTransaction to provide a full audit trail.

 DESIGN NOTE: The receiptNumber is auto-generated using the tenant's receiptNumberMask from
 TenantProfile (e.g., 'REC-2025-0001'). Generated PDF documents are stored via the Document
 table with the receiptId FK set.
 ====================================================================================================
*/
CREATE TABLE "Scheduler"."Receipt"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"receiptNumber" VARCHAR(50) NOT NULL,		-- Auto-generated sequential receipt number (e.g., 'REC-2025-0001').
	"receiptTypeId" INT NOT NULL,		-- Type of receipt (Official, Summary, etc.).
	"invoiceId" INT NULL,		-- Optional link to the invoice this receipt is for.
	"paymentTransactionId" INT NULL,		-- Optional link to the payment transaction.
	"financialTransactionId" INT NULL,		-- Optional link to the financial transaction.
	"clientId" INT NULL,		-- Optional payer client.
	"contactId" INT NULL,		-- Optional payer contact.
	"currencyId" INT NOT NULL,		-- Currency for the amount.
	"receiptDate" TIMESTAMP NOT NULL,		-- Date the receipt was issued (UTC).
	"amount" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Amount received.
	"paymentMethod" VARCHAR(100) NULL,		-- How the payment was made (e.g., 'E-Transfer', 'Cash').
	"description" VARCHAR(500) NULL,		-- Description of what the payment was for.
	"notes" TEXT NULL,		-- Optional additional notes.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "receiptTypeId" FOREIGN KEY ("receiptTypeId") REFERENCES "Scheduler"."ReceiptType"("id"),		-- Foreign key to the ReceiptType table.
	CONSTRAINT "invoiceId" FOREIGN KEY ("invoiceId") REFERENCES "Scheduler"."Invoice"("id"),		-- Foreign key to the Invoice table.
	CONSTRAINT "paymentTransactionId" FOREIGN KEY ("paymentTransactionId") REFERENCES "Scheduler"."PaymentTransaction"("id"),		-- Foreign key to the PaymentTransaction table.
	CONSTRAINT "financialTransactionId" FOREIGN KEY ("financialTransactionId") REFERENCES "Scheduler"."FinancialTransaction"("id"),		-- Foreign key to the FinancialTransaction table.
	CONSTRAINT "clientId" FOREIGN KEY ("clientId") REFERENCES "Scheduler"."Client"("id"),		-- Foreign key to the Client table.
	CONSTRAINT "contactId" FOREIGN KEY ("contactId") REFERENCES "Scheduler"."Contact"("id"),		-- Foreign key to the Contact table.
	CONSTRAINT "currencyId" FOREIGN KEY ("currencyId") REFERENCES "Scheduler"."Currency"("id"),		-- Foreign key to the Currency table.
	CONSTRAINT "UC_Receipt_tenantGuid_receiptNumber" UNIQUE ( "tenantGuid", "receiptNumber") 		-- Uniqueness enforced on the Receipt table's tenantGuid and receiptNumber fields.
);
-- Index on the Receipt table's tenantGuid field.
CREATE INDEX "I_Receipt_tenantGuid" ON "Scheduler"."Receipt" ("tenantGuid")
;

-- Index on the Receipt table's tenantGuid,receiptNumber fields.
CREATE INDEX "I_Receipt_tenantGuid_receiptNumber" ON "Scheduler"."Receipt" ("tenantGuid", "receiptNumber")
;

-- Index on the Receipt table's tenantGuid,receiptTypeId fields.
CREATE INDEX "I_Receipt_tenantGuid_receiptTypeId" ON "Scheduler"."Receipt" ("tenantGuid", "receiptTypeId")
;

-- Index on the Receipt table's tenantGuid,invoiceId fields.
CREATE INDEX "I_Receipt_tenantGuid_invoiceId" ON "Scheduler"."Receipt" ("tenantGuid", "invoiceId")
;

-- Index on the Receipt table's tenantGuid,paymentTransactionId fields.
CREATE INDEX "I_Receipt_tenantGuid_paymentTransactionId" ON "Scheduler"."Receipt" ("tenantGuid", "paymentTransactionId")
;

-- Index on the Receipt table's tenantGuid,financialTransactionId fields.
CREATE INDEX "I_Receipt_tenantGuid_financialTransactionId" ON "Scheduler"."Receipt" ("tenantGuid", "financialTransactionId")
;

-- Index on the Receipt table's tenantGuid,clientId fields.
CREATE INDEX "I_Receipt_tenantGuid_clientId" ON "Scheduler"."Receipt" ("tenantGuid", "clientId")
;

-- Index on the Receipt table's tenantGuid,contactId fields.
CREATE INDEX "I_Receipt_tenantGuid_contactId" ON "Scheduler"."Receipt" ("tenantGuid", "contactId")
;

-- Index on the Receipt table's tenantGuid,currencyId fields.
CREATE INDEX "I_Receipt_tenantGuid_currencyId" ON "Scheduler"."Receipt" ("tenantGuid", "currencyId")
;

-- Index on the Receipt table's tenantGuid,receiptDate fields.
CREATE INDEX "I_Receipt_tenantGuid_receiptDate" ON "Scheduler"."Receipt" ("tenantGuid", "receiptDate")
;

-- Index on the Receipt table's tenantGuid,active fields.
CREATE INDEX "I_Receipt_tenantGuid_active" ON "Scheduler"."Receipt" ("tenantGuid", "active")
;

-- Index on the Receipt table's tenantGuid,deleted fields.
CREATE INDEX "I_Receipt_tenantGuid_deleted" ON "Scheduler"."Receipt" ("tenantGuid", "deleted")
;


-- The change history for records from the Receipt table.
CREATE TABLE "Scheduler"."ReceiptChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"receiptId" INT NOT NULL,		-- Link to the Receipt table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "receiptId" FOREIGN KEY ("receiptId") REFERENCES "Scheduler"."Receipt"("id")		-- Foreign key to the Receipt table.
);
-- Index on the ReceiptChangeHistory table's tenantGuid field.
CREATE INDEX "I_ReceiptChangeHistory_tenantGuid" ON "Scheduler"."ReceiptChangeHistory" ("tenantGuid")
;

-- Index on the ReceiptChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ReceiptChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ReceiptChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ReceiptChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ReceiptChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ReceiptChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ReceiptChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ReceiptChangeHistory_tenantGuid_userId" ON "Scheduler"."ReceiptChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ReceiptChangeHistory table's tenantGuid,receiptId fields.
CREATE INDEX "I_ReceiptChangeHistory_tenantGuid_receiptId" ON "Scheduler"."ReceiptChangeHistory" ("tenantGuid", "receiptId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- The contact interaction data
CREATE TABLE "Scheduler"."ContactInteraction"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"contactId" INT NOT NULL,		-- The contact that is the target of the interaction.
	"initiatingContactId" INT NULL,		-- Optional contact that initiated the interaction.  This would be staff of the company using the scheduler
	"interactionTypeId" INT NOT NULL,		-- Link to the InteractionType table.
	"scheduledEventId" INT NULL,		-- The optional event that the interaction is regarding.
	"startTime" TIMESTAMP NOT NULL,
	"endTime" TIMESTAMP NULL,
	"notes" TEXT NULL,		-- Optional notes about the interaction
	"location" TEXT NULL,		-- Optional location details about the interaction
	"priorityId" INT NULL,		-- Optional priority for the interaction.
	"externalId" VARCHAR(100) NULL,
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "contactId" FOREIGN KEY ("contactId") REFERENCES "Scheduler"."Contact"("id"),		-- Foreign key to the Contact table.
	CONSTRAINT "initiatingContactId" FOREIGN KEY ("initiatingContactId") REFERENCES "Scheduler"."Contact"("id"),		-- Foreign key to the Contact table.
	CONSTRAINT "interactionTypeId" FOREIGN KEY ("interactionTypeId") REFERENCES "Scheduler"."InteractionType"("id"),		-- Foreign key to the InteractionType table.
	CONSTRAINT "scheduledEventId" FOREIGN KEY ("scheduledEventId") REFERENCES "Scheduler"."ScheduledEvent"("id"),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT "priorityId" FOREIGN KEY ("priorityId") REFERENCES "Scheduler"."Priority"("id")		-- Foreign key to the Priority table.
);
-- Index on the ContactInteraction table's tenantGuid field.
CREATE INDEX "I_ContactInteraction_tenantGuid" ON "Scheduler"."ContactInteraction" ("tenantGuid")
;

-- Index on the ContactInteraction table's tenantGuid,contactId fields.
CREATE INDEX "I_ContactInteraction_tenantGuid_contactId" ON "Scheduler"."ContactInteraction" ("tenantGuid", "contactId")
;

-- Index on the ContactInteraction table's tenantGuid,initiatingContactId fields.
CREATE INDEX "I_ContactInteraction_tenantGuid_initiatingContactId" ON "Scheduler"."ContactInteraction" ("tenantGuid", "initiatingContactId")
;

-- Index on the ContactInteraction table's tenantGuid,interactionTypeId fields.
CREATE INDEX "I_ContactInteraction_tenantGuid_interactionTypeId" ON "Scheduler"."ContactInteraction" ("tenantGuid", "interactionTypeId")
;

-- Index on the ContactInteraction table's tenantGuid,scheduledEventId fields.
CREATE INDEX "I_ContactInteraction_tenantGuid_scheduledEventId" ON "Scheduler"."ContactInteraction" ("tenantGuid", "scheduledEventId")
;

-- Index on the ContactInteraction table's tenantGuid,priorityId fields.
CREATE INDEX "I_ContactInteraction_tenantGuid_priorityId" ON "Scheduler"."ContactInteraction" ("tenantGuid", "priorityId")
;

-- Index on the ContactInteraction table's tenantGuid,externalId fields.
CREATE INDEX "I_ContactInteraction_tenantGuid_externalId" ON "Scheduler"."ContactInteraction" ("tenantGuid", "externalId")
;

-- Index on the ContactInteraction table's tenantGuid,active fields.
CREATE INDEX "I_ContactInteraction_tenantGuid_active" ON "Scheduler"."ContactInteraction" ("tenantGuid", "active")
;

-- Index on the ContactInteraction table's tenantGuid,deleted fields.
CREATE INDEX "I_ContactInteraction_tenantGuid_deleted" ON "Scheduler"."ContactInteraction" ("tenantGuid", "deleted")
;

-- Index on the ContactInteraction table's tenantGuid,contactId,startTime fields.
CREATE INDEX "I_ContactInteraction_tenantGuid_contactId_startTime" ON "Scheduler"."ContactInteraction" ("tenantGuid", "contactId", "startTime")
;


-- The change history for records from the ContactInteraction table.
CREATE TABLE "Scheduler"."ContactInteractionChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"contactInteractionId" INT NOT NULL,		-- Link to the ContactInteraction table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "contactInteractionId" FOREIGN KEY ("contactInteractionId") REFERENCES "Scheduler"."ContactInteraction"("id")		-- Foreign key to the ContactInteraction table.
);
-- Index on the ContactInteractionChangeHistory table's tenantGuid field.
CREATE INDEX "I_ContactInteractionChangeHistory_tenantGuid" ON "Scheduler"."ContactInteractionChangeHistory" ("tenantGuid")
;

-- Index on the ContactInteractionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ContactInteractionChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ContactInteractionChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ContactInteractionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ContactInteractionChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ContactInteractionChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ContactInteractionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ContactInteractionChangeHistory_tenantGuid_userId" ON "Scheduler"."ContactInteractionChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ContactInteractionChangeHistory table's tenantGuid,contactInteractionId fields.
CREATE INDEX "I_ContactInteractionChangeHistory_tenantGuid_contactInteraction" ON "Scheduler"."ContactInteractionChangeHistory" ("tenantGuid", "contactInteractionId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Many-to-many relationship between events and calendars.
CREATE TABLE "Scheduler"."EventCalendar"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduledEventId" INT NOT NULL,		-- Link to the ScheduledEvent table.
	"calendarId" INT NOT NULL,		-- Link to the Calendar table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "scheduledEventId" FOREIGN KEY ("scheduledEventId") REFERENCES "Scheduler"."ScheduledEvent"("id"),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT "calendarId" FOREIGN KEY ("calendarId") REFERENCES "Scheduler"."Calendar"("id"),		-- Foreign key to the Calendar table.
	CONSTRAINT "UC_EventCalendar_tenantGuid_scheduledEventId_calendarId" UNIQUE ( "tenantGuid", "scheduledEventId", "calendarId") 		-- Uniqueness enforced on the EventCalendar table's tenantGuid and scheduledEventId and calendarId fields.
);
-- Index on the EventCalendar table's tenantGuid field.
CREATE INDEX "I_EventCalendar_tenantGuid" ON "Scheduler"."EventCalendar" ("tenantGuid")
;

-- Index on the EventCalendar table's tenantGuid,scheduledEventId fields.
CREATE INDEX "I_EventCalendar_tenantGuid_scheduledEventId" ON "Scheduler"."EventCalendar" ("tenantGuid", "scheduledEventId")
;

-- Index on the EventCalendar table's tenantGuid,calendarId fields.
CREATE INDEX "I_EventCalendar_tenantGuid_calendarId" ON "Scheduler"."EventCalendar" ("tenantGuid", "calendarId")
;

-- Index on the EventCalendar table's tenantGuid,active fields.
CREATE INDEX "I_EventCalendar_tenantGuid_active" ON "Scheduler"."EventCalendar" ("tenantGuid", "active")
;

-- Index on the EventCalendar table's tenantGuid,deleted fields.
CREATE INDEX "I_EventCalendar_tenantGuid_deleted" ON "Scheduler"."EventCalendar" ("tenantGuid", "deleted")
;


-- Master list of depedency types
CREATE TABLE "Scheduler"."DependencyType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the DependencyType table's name field.
CREATE INDEX "I_DependencyType_name" ON "Scheduler"."DependencyType" ("name")
;

-- Index on the DependencyType table's active field.
CREATE INDEX "I_DependencyType_active" ON "Scheduler"."DependencyType" ("active")
;

-- Index on the DependencyType table's deleted field.
CREATE INDEX "I_DependencyType_deleted" ON "Scheduler"."DependencyType" ("deleted")
;

INSERT INTO "Scheduler"."DependencyType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'FS', 'Finish to Start', 1, 'f08977bf-af84-4d89-9821-f8a2404028fa' );

INSERT INTO "Scheduler"."DependencyType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'SS', 'Start to Start', 2, '51398efa-2489-41ba-a1b6-77d11ce6253b' );

INSERT INTO "Scheduler"."DependencyType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'SF', 'Start to Finish', 3, '637dc30a-adc3-47ad-87fa-3c826b7d808f' );

INSERT INTO "Scheduler"."DependencyType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'FF', 'Finish to Finish', 4, 'fc7b4932-e79a-4085-9c87-404d29331f85' );


-- Dependencies that a scheduled event has that could affect it.
CREATE TABLE "Scheduler"."ScheduledEventDependency"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"predecessorEventId" INT NOT NULL,		-- The task that must happen first
	"successorEventId" INT NOT NULL,		-- The task that waits
	"dependencyTypeId" INT NOT NULL,		-- Link to the DependencyType table.
	"lagMinutes" INT NOT NULL DEFAULT 0,
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "predecessorEventId" FOREIGN KEY ("predecessorEventId") REFERENCES "Scheduler"."ScheduledEvent"("id"),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT "successorEventId" FOREIGN KEY ("successorEventId") REFERENCES "Scheduler"."ScheduledEvent"("id"),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT "dependencyTypeId" FOREIGN KEY ("dependencyTypeId") REFERENCES "Scheduler"."DependencyType"("id"),		-- Foreign key to the DependencyType table.
	CONSTRAINT "UC_ScheduledEventDependency_tenantGuid_predecessorEventId_successorEventId" UNIQUE ( "tenantGuid", "predecessorEventId", "successorEventId") 		-- Uniqueness enforced on the ScheduledEventDependency table's tenantGuid and predecessorEventId and successorEventId fields.
);
-- Index on the ScheduledEventDependency table's tenantGuid field.
CREATE INDEX "I_ScheduledEventDependency_tenantGuid" ON "Scheduler"."ScheduledEventDependency" ("tenantGuid")
;

-- Index on the ScheduledEventDependency table's tenantGuid,predecessorEventId fields.
CREATE INDEX "I_ScheduledEventDependency_tenantGuid_predecessorEventId" ON "Scheduler"."ScheduledEventDependency" ("tenantGuid", "predecessorEventId")
;

-- Index on the ScheduledEventDependency table's tenantGuid,successorEventId fields.
CREATE INDEX "I_ScheduledEventDependency_tenantGuid_successorEventId" ON "Scheduler"."ScheduledEventDependency" ("tenantGuid", "successorEventId")
;

-- Index on the ScheduledEventDependency table's tenantGuid,active fields.
CREATE INDEX "I_ScheduledEventDependency_tenantGuid_active" ON "Scheduler"."ScheduledEventDependency" ("tenantGuid", "active")
;

-- Index on the ScheduledEventDependency table's tenantGuid,deleted fields.
CREATE INDEX "I_ScheduledEventDependency_tenantGuid_deleted" ON "Scheduler"."ScheduledEventDependency" ("tenantGuid", "deleted")
;


-- The change history for records from the ScheduledEventDependency table.
CREATE TABLE "Scheduler"."ScheduledEventDependencyChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduledEventDependencyId" INT NOT NULL,		-- Link to the ScheduledEventDependency table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "scheduledEventDependencyId" FOREIGN KEY ("scheduledEventDependencyId") REFERENCES "Scheduler"."ScheduledEventDependency"("id")		-- Foreign key to the ScheduledEventDependency table.
);
-- Index on the ScheduledEventDependencyChangeHistory table's tenantGuid field.
CREATE INDEX "I_ScheduledEventDependencyChangeHistory_tenantGuid" ON "Scheduler"."ScheduledEventDependencyChangeHistory" ("tenantGuid")
;

-- Index on the ScheduledEventDependencyChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ScheduledEventDependencyChangeHistory_tenantGuid_versionNumbe" ON "Scheduler"."ScheduledEventDependencyChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ScheduledEventDependencyChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ScheduledEventDependencyChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ScheduledEventDependencyChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ScheduledEventDependencyChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ScheduledEventDependencyChangeHistory_tenantGuid_userId" ON "Scheduler"."ScheduledEventDependencyChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ScheduledEventDependencyChangeHistory table's tenantGuid,scheduledEventDependencyId fields.
CREATE INDEX "I_ScheduledEventDependencyChangeHistory_tenantGuid_scheduledEve" ON "Scheduler"."ScheduledEventDependencyChangeHistory" ("tenantGuid", "scheduledEventDependencyId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Specific qualifications required for a single event instance, overriding or adding to role/site reqs..
CREATE TABLE "Scheduler"."ScheduledEventQualificationRequirement"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduledEventId" INT NOT NULL,		-- Link to the ScheduledEvent table.
	"qualificationId" INT NOT NULL,		-- Link to the Qualification table.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "scheduledEventId" FOREIGN KEY ("scheduledEventId") REFERENCES "Scheduler"."ScheduledEvent"("id"),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT "qualificationId" FOREIGN KEY ("qualificationId") REFERENCES "Scheduler"."Qualification"("id")		-- Foreign key to the Qualification table.
);
-- Index on the ScheduledEventQualificationRequirement table's tenantGuid field.
CREATE INDEX "I_ScheduledEventQualificationRequirement_tenantGuid" ON "Scheduler"."ScheduledEventQualificationRequirement" ("tenantGuid")
;

-- Index on the ScheduledEventQualificationRequirement table's tenantGuid,scheduledEventId fields.
CREATE INDEX "I_ScheduledEventQualificationRequirement_tenantGuid_scheduledEv" ON "Scheduler"."ScheduledEventQualificationRequirement" ("tenantGuid", "scheduledEventId")
;

-- Index on the ScheduledEventQualificationRequirement table's tenantGuid,qualificationId fields.
CREATE INDEX "I_ScheduledEventQualificationRequirement_tenantGuid_qualificati" ON "Scheduler"."ScheduledEventQualificationRequirement" ("tenantGuid", "qualificationId")
;

-- Index on the ScheduledEventQualificationRequirement table's tenantGuid,active fields.
CREATE INDEX "I_ScheduledEventQualificationRequirement_tenantGuid_active" ON "Scheduler"."ScheduledEventQualificationRequirement" ("tenantGuid", "active")
;

-- Index on the ScheduledEventQualificationRequirement table's tenantGuid,deleted fields.
CREATE INDEX "I_ScheduledEventQualificationRequirement_tenantGuid_deleted" ON "Scheduler"."ScheduledEventQualificationRequirement" ("tenantGuid", "deleted")
;


-- The change history for records from the ScheduledEventQualificationRequirement table.
CREATE TABLE "Scheduler"."ScheduledEventQualificationRequirementChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduledEventQualificationRequirementId" INT NOT NULL,		-- Link to the ScheduledEventQualificationRequirement table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "scheduledEventQualificationRequirementId" FOREIGN KEY ("scheduledEventQualificationRequirementId") REFERENCES "Scheduler"."ScheduledEventQualificationRequirement"("id")		-- Foreign key to the ScheduledEventQualificationRequirement table.
);
-- Index on the ScheduledEventQualificationRequirementChangeHistory table's tenantGuid field.
CREATE INDEX "I_ScheduledEventQualificationRequirementChangeHistory_tenantGui" ON "Scheduler"."ScheduledEventQualificationRequirementChangeHistory" ("tenantGuid")
;

-- Index on the ScheduledEventQualificationRequirementChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ScheduledEventQualificationRequirementChangeHistory_tenantGui" ON "Scheduler"."ScheduledEventQualificationRequirementChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ScheduledEventQualificationRequirementChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ScheduledEventQualificationRequirementChangeHistory_tenantGui" ON "Scheduler"."ScheduledEventQualificationRequirementChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ScheduledEventQualificationRequirementChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ScheduledEventQualificationRequirementChangeHistory_tenantGui" ON "Scheduler"."ScheduledEventQualificationRequirementChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ScheduledEventQualificationRequirementChangeHistory table's tenantGuid,scheduledEventQualificationRequirementId fields.
CREATE INDEX "I_ScheduledEventQualificationRequirementChangeHistory_tenantGui" ON "Scheduler"."ScheduledEventQualificationRequirementChangeHistory" ("tenantGuid", "scheduledEventQualificationRequirementId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Exceptions to a recurring series.  Used for canceled dates or moved instances (original date + new date).
CREATE TABLE "Scheduler"."RecurrenceException"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduledEventId" INT NOT NULL,		-- Link to the ScheduledEvent table.
	"exceptionDateTime" TIMESTAMP NOT NULL,		-- The original occurrence date/time that is excepted
	"movedToDateTime" TIMESTAMP NULL,		-- NULL = canceled, non-NULL = moved to this new date/time
	"reason" VARCHAR(250) NULL,
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "scheduledEventId" FOREIGN KEY ("scheduledEventId") REFERENCES "Scheduler"."ScheduledEvent"("id"),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT "UC_RecurrenceException_tenantGuid_scheduledEventId_exceptionDateTime" UNIQUE ( "tenantGuid", "scheduledEventId", "exceptionDateTime") 		-- Uniqueness enforced on the RecurrenceException table's tenantGuid and scheduledEventId and exceptionDateTime fields.
);
-- Index on the RecurrenceException table's tenantGuid field.
CREATE INDEX "I_RecurrenceException_tenantGuid" ON "Scheduler"."RecurrenceException" ("tenantGuid")
;

-- Index on the RecurrenceException table's tenantGuid,scheduledEventId fields.
CREATE INDEX "I_RecurrenceException_tenantGuid_scheduledEventId" ON "Scheduler"."RecurrenceException" ("tenantGuid", "scheduledEventId")
;

-- Index on the RecurrenceException table's tenantGuid,active fields.
CREATE INDEX "I_RecurrenceException_tenantGuid_active" ON "Scheduler"."RecurrenceException" ("tenantGuid", "active")
;

-- Index on the RecurrenceException table's tenantGuid,deleted fields.
CREATE INDEX "I_RecurrenceException_tenantGuid_deleted" ON "Scheduler"."RecurrenceException" ("tenantGuid", "deleted")
;


-- The change history for records from the RecurrenceException table.
CREATE TABLE "Scheduler"."RecurrenceExceptionChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"recurrenceExceptionId" INT NOT NULL,		-- Link to the RecurrenceException table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "recurrenceExceptionId" FOREIGN KEY ("recurrenceExceptionId") REFERENCES "Scheduler"."RecurrenceException"("id")		-- Foreign key to the RecurrenceException table.
);
-- Index on the RecurrenceExceptionChangeHistory table's tenantGuid field.
CREATE INDEX "I_RecurrenceExceptionChangeHistory_tenantGuid" ON "Scheduler"."RecurrenceExceptionChangeHistory" ("tenantGuid")
;

-- Index on the RecurrenceExceptionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_RecurrenceExceptionChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."RecurrenceExceptionChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the RecurrenceExceptionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_RecurrenceExceptionChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."RecurrenceExceptionChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the RecurrenceExceptionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_RecurrenceExceptionChangeHistory_tenantGuid_userId" ON "Scheduler"."RecurrenceExceptionChangeHistory" ("tenantGuid", "userId")
;

-- Index on the RecurrenceExceptionChangeHistory table's tenantGuid,recurrenceExceptionId fields.
CREATE INDEX "I_RecurrenceExceptionChangeHistory_tenantGuid_recurrenceExcepti" ON "Scheduler"."RecurrenceExceptionChangeHistory" ("tenantGuid", "recurrenceExceptionId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Master list of notification types
CREATE TABLE "Scheduler"."NotificationType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the NotificationType table's name field.
CREATE INDEX "I_NotificationType_name" ON "Scheduler"."NotificationType" ("name")
;

-- Index on the NotificationType table's active field.
CREATE INDEX "I_NotificationType_active" ON "Scheduler"."NotificationType" ("active")
;

-- Index on the NotificationType table's deleted field.
CREATE INDEX "I_NotificationType_deleted" ON "Scheduler"."NotificationType" ("deleted")
;

INSERT INTO "Scheduler"."NotificationType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Email', 'Send to email address', 1, '73ff7b17-3fd7-40ce-91bf-c91daca7b4ce' );

INSERT INTO "Scheduler"."NotificationType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'SMS', 'Sent to cell phone via SMS message', 2, '89391299-4427-43f6-bcf2-0266e47e83a7' );

INSERT INTO "Scheduler"."NotificationType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Push', 'Sent to cell phone via Push notification', 3, '0395ddde-58dc-4577-9dae-07614680c386' );


-- Links resources (or entire crews) to events.  Supports partial assignments and role designation.  - If crewId is non-NULL → this row represents assignment of the whole crew - If resourceId is non-NULL and crewId is NULL → individual resource assignment - assignmentStart/End NULL → uses full event duration
CREATE TABLE "Scheduler"."NotificationSubscription"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"resourceId" INT NULL,		-- Optional resource for this notification subscription.  Needs either this or contact to be valid.
	"contactId" INT NULL,		-- Optional contact for this notification subscription.  Needs either this or resource to be valid.
	"notificationTypeId" INT NOT NULL,		-- Link to the NotificationType table.
	"triggerEvents" INT NOT NULL DEFAULT 1,		-- Bitmask: 1=Assigned, 2=Canceled, 4=Modified, 8=Reminder
	"recipientAddress" VARCHAR(250) NOT NULL,		-- Email address or Phone #
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "resourceId" FOREIGN KEY ("resourceId") REFERENCES "Scheduler"."Resource"("id"),		-- Foreign key to the Resource table.
	CONSTRAINT "contactId" FOREIGN KEY ("contactId") REFERENCES "Scheduler"."Contact"("id"),		-- Foreign key to the Contact table.
	CONSTRAINT "notificationTypeId" FOREIGN KEY ("notificationTypeId") REFERENCES "Scheduler"."NotificationType"("id")		-- Foreign key to the NotificationType table.
);
-- Index on the NotificationSubscription table's tenantGuid field.
CREATE INDEX "I_NotificationSubscription_tenantGuid" ON "Scheduler"."NotificationSubscription" ("tenantGuid")
;

-- Index on the NotificationSubscription table's tenantGuid,resourceId fields.
CREATE INDEX "I_NotificationSubscription_tenantGuid_resourceId" ON "Scheduler"."NotificationSubscription" ("tenantGuid", "resourceId")
;

-- Index on the NotificationSubscription table's tenantGuid,contactId fields.
CREATE INDEX "I_NotificationSubscription_tenantGuid_contactId" ON "Scheduler"."NotificationSubscription" ("tenantGuid", "contactId")
;

-- Index on the NotificationSubscription table's tenantGuid,notificationTypeId fields.
CREATE INDEX "I_NotificationSubscription_tenantGuid_notificationTypeId" ON "Scheduler"."NotificationSubscription" ("tenantGuid", "notificationTypeId")
;

-- Index on the NotificationSubscription table's tenantGuid,active fields.
CREATE INDEX "I_NotificationSubscription_tenantGuid_active" ON "Scheduler"."NotificationSubscription" ("tenantGuid", "active")
;

-- Index on the NotificationSubscription table's tenantGuid,deleted fields.
CREATE INDEX "I_NotificationSubscription_tenantGuid_deleted" ON "Scheduler"."NotificationSubscription" ("tenantGuid", "deleted")
;


-- The change history for records from the NotificationSubscription table.
CREATE TABLE "Scheduler"."NotificationSubscriptionChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"notificationSubscriptionId" INT NOT NULL,		-- Link to the NotificationSubscription table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "notificationSubscriptionId" FOREIGN KEY ("notificationSubscriptionId") REFERENCES "Scheduler"."NotificationSubscription"("id")		-- Foreign key to the NotificationSubscription table.
);
-- Index on the NotificationSubscriptionChangeHistory table's tenantGuid field.
CREATE INDEX "I_NotificationSubscriptionChangeHistory_tenantGuid" ON "Scheduler"."NotificationSubscriptionChangeHistory" ("tenantGuid")
;

-- Index on the NotificationSubscriptionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_NotificationSubscriptionChangeHistory_tenantGuid_versionNumbe" ON "Scheduler"."NotificationSubscriptionChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the NotificationSubscriptionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_NotificationSubscriptionChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."NotificationSubscriptionChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the NotificationSubscriptionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_NotificationSubscriptionChangeHistory_tenantGuid_userId" ON "Scheduler"."NotificationSubscriptionChangeHistory" ("tenantGuid", "userId")
;

-- Index on the NotificationSubscriptionChangeHistory table's tenantGuid,notificationSubscriptionId fields.
CREATE INDEX "I_NotificationSubscriptionChangeHistory_tenantGuid_notification" ON "Scheduler"."NotificationSubscriptionChangeHistory" ("tenantGuid", "notificationSubscriptionId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
   FUNDRAISING MASTER DATA (The "Codes" in DonorPerfect)
   DP relies on three tiers of coding:
   1. Fund (GL Code) - Where the money goes in the bank.
   2. Campaign - The broad initiative (e.g., "Capital Campaign").
   3. Appeal - The specific ask (e.g., "November Mailer").
   ====================================================================================================

-- FUNDS (General Ledger Codes)
*/
CREATE TABLE "Scheduler"."Fund"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"glCode" VARCHAR(100) NULL,		-- The accounting code
	"isRestricted" BOOLEAN NOT NULL DEFAULT false,		-- Legal restriction on funds
	"goalAmount" DECIMAL(11,2) NULL,
	"notes" TEXT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_Fund_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Fund table's tenantGuid and name fields.
);
-- Index on the Fund table's tenantGuid field.
CREATE INDEX "I_Fund_tenantGuid" ON "Scheduler"."Fund" ("tenantGuid")
;

-- Index on the Fund table's tenantGuid,name fields.
CREATE INDEX "I_Fund_tenantGuid_name" ON "Scheduler"."Fund" ("tenantGuid", "name")
;

-- Index on the Fund table's tenantGuid,iconId fields.
CREATE INDEX "I_Fund_tenantGuid_iconId" ON "Scheduler"."Fund" ("tenantGuid", "iconId")
;

-- Index on the Fund table's tenantGuid,active fields.
CREATE INDEX "I_Fund_tenantGuid_active" ON "Scheduler"."Fund" ("tenantGuid", "active")
;

-- Index on the Fund table's tenantGuid,deleted fields.
CREATE INDEX "I_Fund_tenantGuid_deleted" ON "Scheduler"."Fund" ("tenantGuid", "deleted")
;


-- The change history for records from the Fund table.
CREATE TABLE "Scheduler"."FundChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"fundId" INT NOT NULL,		-- Link to the Fund table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "fundId" FOREIGN KEY ("fundId") REFERENCES "Scheduler"."Fund"("id")		-- Foreign key to the Fund table.
);
-- Index on the FundChangeHistory table's tenantGuid field.
CREATE INDEX "I_FundChangeHistory_tenantGuid" ON "Scheduler"."FundChangeHistory" ("tenantGuid")
;

-- Index on the FundChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_FundChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."FundChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the FundChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_FundChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."FundChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the FundChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_FundChangeHistory_tenantGuid_userId" ON "Scheduler"."FundChangeHistory" ("tenantGuid", "userId")
;

-- Index on the FundChangeHistory table's tenantGuid,fundId fields.
CREATE INDEX "I_FundChangeHistory_tenantGuid_fundId" ON "Scheduler"."FundChangeHistory" ("tenantGuid", "fundId") INCLUDE ( versionNumber, timeStamp, userId )
;


--  2. CAMPAIGNS (Broad Initiatives)
CREATE TABLE "Scheduler"."Campaign"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"startDate" DATE NULL,
	"endDate" DATE NULL,
	"fundRaisingGoal" DECIMAL(11,2) NULL,
	"notes" TEXT NULL,
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_Campaign_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Campaign table's tenantGuid and name fields.
);
-- Index on the Campaign table's tenantGuid field.
CREATE INDEX "I_Campaign_tenantGuid" ON "Scheduler"."Campaign" ("tenantGuid")
;

-- Index on the Campaign table's tenantGuid,name fields.
CREATE INDEX "I_Campaign_tenantGuid_name" ON "Scheduler"."Campaign" ("tenantGuid", "name")
;

-- Index on the Campaign table's tenantGuid,iconId fields.
CREATE INDEX "I_Campaign_tenantGuid_iconId" ON "Scheduler"."Campaign" ("tenantGuid", "iconId")
;

-- Index on the Campaign table's tenantGuid,active fields.
CREATE INDEX "I_Campaign_tenantGuid_active" ON "Scheduler"."Campaign" ("tenantGuid", "active")
;

-- Index on the Campaign table's tenantGuid,deleted fields.
CREATE INDEX "I_Campaign_tenantGuid_deleted" ON "Scheduler"."Campaign" ("tenantGuid", "deleted")
;


-- The change history for records from the Campaign table.
CREATE TABLE "Scheduler"."CampaignChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"campaignId" INT NOT NULL,		-- Link to the Campaign table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "campaignId" FOREIGN KEY ("campaignId") REFERENCES "Scheduler"."Campaign"("id")		-- Foreign key to the Campaign table.
);
-- Index on the CampaignChangeHistory table's tenantGuid field.
CREATE INDEX "I_CampaignChangeHistory_tenantGuid" ON "Scheduler"."CampaignChangeHistory" ("tenantGuid")
;

-- Index on the CampaignChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_CampaignChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."CampaignChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the CampaignChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_CampaignChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."CampaignChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the CampaignChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_CampaignChangeHistory_tenantGuid_userId" ON "Scheduler"."CampaignChangeHistory" ("tenantGuid", "userId")
;

-- Index on the CampaignChangeHistory table's tenantGuid,campaignId fields.
CREATE INDEX "I_CampaignChangeHistory_tenantGuid_campaignId" ON "Scheduler"."CampaignChangeHistory" ("tenantGuid", "campaignId") INCLUDE ( versionNumber, timeStamp, userId )
;


--  3. APPEALS (Specific Solicitations)
CREATE TABLE "Scheduler"."Appeal"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"campaignId" INT NULL,		-- Optional link to parent campaign
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"costPerUnit" DECIMAL(11,2) NULL,		-- For ROI calculation (Cost vs. Raised)
	"notes" TEXT NULL,
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "campaignId" FOREIGN KEY ("campaignId") REFERENCES "Scheduler"."Campaign"("id"),		-- Foreign key to the Campaign table.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_Appeal_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Appeal table's tenantGuid and name fields.
);
-- Index on the Appeal table's tenantGuid field.
CREATE INDEX "I_Appeal_tenantGuid" ON "Scheduler"."Appeal" ("tenantGuid")
;

-- Index on the Appeal table's tenantGuid,campaignId fields.
CREATE INDEX "I_Appeal_tenantGuid_campaignId" ON "Scheduler"."Appeal" ("tenantGuid", "campaignId")
;

-- Index on the Appeal table's tenantGuid,name fields.
CREATE INDEX "I_Appeal_tenantGuid_name" ON "Scheduler"."Appeal" ("tenantGuid", "name")
;

-- Index on the Appeal table's tenantGuid,iconId fields.
CREATE INDEX "I_Appeal_tenantGuid_iconId" ON "Scheduler"."Appeal" ("tenantGuid", "iconId")
;

-- Index on the Appeal table's tenantGuid,active fields.
CREATE INDEX "I_Appeal_tenantGuid_active" ON "Scheduler"."Appeal" ("tenantGuid", "active")
;

-- Index on the Appeal table's tenantGuid,deleted fields.
CREATE INDEX "I_Appeal_tenantGuid_deleted" ON "Scheduler"."Appeal" ("tenantGuid", "deleted")
;


-- The change history for records from the Appeal table.
CREATE TABLE "Scheduler"."AppealChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"appealId" INT NOT NULL,		-- Link to the Appeal table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "appealId" FOREIGN KEY ("appealId") REFERENCES "Scheduler"."Appeal"("id")		-- Foreign key to the Appeal table.
);
-- Index on the AppealChangeHistory table's tenantGuid field.
CREATE INDEX "I_AppealChangeHistory_tenantGuid" ON "Scheduler"."AppealChangeHistory" ("tenantGuid")
;

-- Index on the AppealChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_AppealChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."AppealChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the AppealChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_AppealChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."AppealChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the AppealChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_AppealChangeHistory_tenantGuid_userId" ON "Scheduler"."AppealChangeHistory" ("tenantGuid", "userId")
;

-- Index on the AppealChangeHistory table's tenantGuid,appealId fields.
CREATE INDEX "I_AppealChangeHistory_tenantGuid_appealId" ON "Scheduler"."AppealChangeHistory" ("tenantGuid", "appealId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
   HOUSEHOLD MANAGEMENT
   Standardizes how multiple constituents are grouped for mailing, receipting, and recognition.
   This allows for "The Smith Family" recognition even if John and Jane have separate records.
   ====================================================================================================
*/
CREATE TABLE "Scheduler"."Household"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"schedulingTargetId" INT NULL,		-- Link to the SchedulingTarget table.
	"formalSalutation" VARCHAR(250) NULL,		-- ex. "Mr. and Mrs. John Smith"
	"informalSalutation" VARCHAR(250) NULL,		-- ex. "John and Jane"
	"addressee" VARCHAR(250) NULL,		-- The label for the envelope
	"totalHouseholdGiving" DECIMAL(11,2) NOT NULL DEFAULT 0,
	"lastHouseholdGiftDate" DATE NULL,
	"notes" TEXT NULL,
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"avatarFileName" VARCHAR(250) NULL,		-- Part of the binary data field setup
	"avatarSize" BIGINT NULL,		-- Part of the binary data field setup
	"avatarData" BYTEA NULL,		-- Part of the binary data field setup
	"avatarMimeType" VARCHAR(100) NULL,		-- Part of the binary data field setup
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "schedulingTargetId" FOREIGN KEY ("schedulingTargetId") REFERENCES "Scheduler"."SchedulingTarget"("id"),		-- Foreign key to the SchedulingTarget table.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_Household_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Household table's tenantGuid and name fields.
);
-- Index on the Household table's tenantGuid field.
CREATE INDEX "I_Household_tenantGuid" ON "Scheduler"."Household" ("tenantGuid")
;

-- Index on the Household table's tenantGuid,name fields.
CREATE INDEX "I_Household_tenantGuid_name" ON "Scheduler"."Household" ("tenantGuid", "name")
;

-- Index on the Household table's tenantGuid,schedulingTargetId fields.
CREATE INDEX "I_Household_tenantGuid_schedulingTargetId" ON "Scheduler"."Household" ("tenantGuid", "schedulingTargetId")
;

-- Index on the Household table's tenantGuid,iconId fields.
CREATE INDEX "I_Household_tenantGuid_iconId" ON "Scheduler"."Household" ("tenantGuid", "iconId")
;

-- Index on the Household table's tenantGuid,active fields.
CREATE INDEX "I_Household_tenantGuid_active" ON "Scheduler"."Household" ("tenantGuid", "active")
;

-- Index on the Household table's tenantGuid,deleted fields.
CREATE INDEX "I_Household_tenantGuid_deleted" ON "Scheduler"."Household" ("tenantGuid", "deleted")
;


-- The change history for records from the Household table.
CREATE TABLE "Scheduler"."HouseholdChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"householdId" INT NOT NULL,		-- Link to the Household table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "householdId" FOREIGN KEY ("householdId") REFERENCES "Scheduler"."Household"("id")		-- Foreign key to the Household table.
);
-- Index on the HouseholdChangeHistory table's tenantGuid field.
CREATE INDEX "I_HouseholdChangeHistory_tenantGuid" ON "Scheduler"."HouseholdChangeHistory" ("tenantGuid")
;

-- Index on the HouseholdChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_HouseholdChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."HouseholdChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the HouseholdChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_HouseholdChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."HouseholdChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the HouseholdChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_HouseholdChangeHistory_tenantGuid_userId" ON "Scheduler"."HouseholdChangeHistory" ("tenantGuid", "userId")
;

-- Index on the HouseholdChangeHistory table's tenantGuid,householdId fields.
CREATE INDEX "I_HouseholdChangeHistory_tenantGuid_householdId" ON "Scheduler"."HouseholdChangeHistory" ("tenantGuid", "householdId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Defines stages in a donor's journey (e.g., Target, Qualified, Cultivated, Solicited, Stewardship).
CREATE TABLE "Scheduler"."ConstituentJourneyStage"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"minLifetimeGiving" DECIMAL(11,2) NULL,		-- Optional criteria: Minimum total giving to qualify for this stage.
	"maxLifetimeGiving" DECIMAL(11,2) NULL,		-- Optional criteria: Maximum total giving
	"minSingleGiftAmount" DECIMAL(11,2) NULL,		-- Optional criteria: Min single gift size
	"isDefault" BOOLEAN NOT NULL,		-- If true, this is the default stage for new constituents.
	"minAnnualGiving" DECIMAL(11,2) NULL,		-- Optional: Minimum giving in the past 365 days.
	"maxDaysSinceLastGift" INT NULL DEFAULT 0,		-- Optional: Maximum days elapsed since the last gift (recency limit).
	"minGiftCount" INT NULL DEFAULT 0,		-- Optional: Minimum number of gifts required.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_ConstituentJourneyStage_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the ConstituentJourneyStage table's tenantGuid and name fields.
);
-- Index on the ConstituentJourneyStage table's tenantGuid field.
CREATE INDEX "I_ConstituentJourneyStage_tenantGuid" ON "Scheduler"."ConstituentJourneyStage" ("tenantGuid")
;

-- Index on the ConstituentJourneyStage table's tenantGuid,name fields.
CREATE INDEX "I_ConstituentJourneyStage_tenantGuid_name" ON "Scheduler"."ConstituentJourneyStage" ("tenantGuid", "name")
;

-- Index on the ConstituentJourneyStage table's tenantGuid,iconId fields.
CREATE INDEX "I_ConstituentJourneyStage_tenantGuid_iconId" ON "Scheduler"."ConstituentJourneyStage" ("tenantGuid", "iconId")
;

-- Index on the ConstituentJourneyStage table's tenantGuid,active fields.
CREATE INDEX "I_ConstituentJourneyStage_tenantGuid_active" ON "Scheduler"."ConstituentJourneyStage" ("tenantGuid", "active")
;

-- Index on the ConstituentJourneyStage table's tenantGuid,deleted fields.
CREATE INDEX "I_ConstituentJourneyStage_tenantGuid_deleted" ON "Scheduler"."ConstituentJourneyStage" ("tenantGuid", "deleted")
;

INSERT INTO "Scheduler"."ConstituentJourneyStage" ( "tenantGuid", "name", "description", "sequence", "isDefault", "color", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Unqualified', 'New potential donor.', 1, true, '#9E9E9E', 'd8663e5e-749c-4638-b69d-21d96078659d' );

INSERT INTO "Scheduler"."ConstituentJourneyStage" ( "tenantGuid", "name", "description", "sequence", "isDefault", "color", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Qualified', 'Donor has been qualified.', 2, false, '#2196F3', 'ad06353d-2476-4322-836f-5374825968f9' );

INSERT INTO "Scheduler"."ConstituentJourneyStage" ( "tenantGuid", "name", "description", "sequence", "isDefault", "color", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Cultivated', 'Relationship is being built.', 3, false, '#4CAF50', 'e8b60384-9336-4022-8b4b-970752538965' );

INSERT INTO "Scheduler"."ConstituentJourneyStage" ( "tenantGuid", "name", "description", "sequence", "isDefault", "color", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Solicited', 'Ask has been made.', 4, false, '#FF9800', '64319688-fd06-4074-8902-628670bf7471' );

INSERT INTO "Scheduler"."ConstituentJourneyStage" ( "tenantGuid", "name", "description", "sequence", "isDefault", "color", "objectGuid" ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Stewardship', 'Ongoing maintenance.', 5, false, '#9C27B0', '1d971578-8319-482a-9e8c-529141873837' );


-- The change history for records from the ConstituentJourneyStage table.
CREATE TABLE "Scheduler"."ConstituentJourneyStageChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"constituentJourneyStageId" INT NOT NULL,		-- Link to the ConstituentJourneyStage table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "constituentJourneyStageId" FOREIGN KEY ("constituentJourneyStageId") REFERENCES "Scheduler"."ConstituentJourneyStage"("id")		-- Foreign key to the ConstituentJourneyStage table.
);
-- Index on the ConstituentJourneyStageChangeHistory table's tenantGuid field.
CREATE INDEX "I_ConstituentJourneyStageChangeHistory_tenantGuid" ON "Scheduler"."ConstituentJourneyStageChangeHistory" ("tenantGuid")
;

-- Index on the ConstituentJourneyStageChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ConstituentJourneyStageChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ConstituentJourneyStageChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ConstituentJourneyStageChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ConstituentJourneyStageChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ConstituentJourneyStageChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ConstituentJourneyStageChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ConstituentJourneyStageChangeHistory_tenantGuid_userId" ON "Scheduler"."ConstituentJourneyStageChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ConstituentJourneyStageChangeHistory table's tenantGuid,constituentJourneyStageId fields.
CREATE INDEX "I_ConstituentJourneyStageChangeHistory_tenantGuid_constituentJo" ON "Scheduler"."ConstituentJourneyStageChangeHistory" ("tenantGuid", "constituentJourneyStageId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
 ====================================================================================================
   CONSTITUENT MANAGEMENT
   In DP, a Constituent is the heart of the system. 
   Here, we link to your existing Contact (Person) or Client (Organization) tables.
   This table stores the "Fundraising Intelligence" (RFM metrics).
   ====================================================================================================
*/
CREATE TABLE "Scheduler"."Constituent"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"contactId" INT NULL,		-- Polymorphic Link: A constituent is EITHER a generic Contact OR a Client (Company)
	"clientId" INT NULL,		-- Polymorphic Link: A constituent is EITHER a generic Contact OR a Client (Company)
	"householdId" INT NULL,		-- Links a constituent to a household
	"constituentNumber" VARCHAR(50) NOT NULL,		-- The distinct 'Donor ID'
	"doNotSolicit" BOOLEAN NOT NULL DEFAULT false,
	"doNotEmail" BOOLEAN NOT NULL DEFAULT false,
	"doNotMail" BOOLEAN NOT NULL DEFAULT false,
	"totalLifetimeGiving" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Caching Calculated Metrics(DP Style for fast reporting)
	"totalYTDGiving" DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Caching Calculated Metrics(DP Style for fast reporting)
	"lastGiftDate" DATE NULL,		-- Caching Calculated Metrics(DP Style for fast reporting)
	"lastGiftAmount" DECIMAL(11,2) NULL,		-- Caching Calculated Metrics(DP Style for fast reporting)
	"largestGiftAmount" DECIMAL(11,2) NULL,		-- Caching Calculated Metrics(DP Style for fast reporting)
	"totalGiftCount" INT NULL,		-- Caching Calculated Metrics(DP Style for fast reporting)
	"externalId" VARCHAR(100) NULL,		-- For things like QBO Customer ID
	"notes" TEXT NULL,
	"constituentJourneyStageId" INT NULL,		-- Current stage in the donor journey.
	"dateEnteredCurrentStage" TIMESTAMP NULL,		-- Date when the constituent moved to the current stage.
	"attributes" TEXT NULL,		-- to store arbitrary JSON
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"avatarFileName" VARCHAR(250) NULL,		-- Part of the binary data field setup
	"avatarSize" BIGINT NULL,		-- Part of the binary data field setup
	"avatarData" BYTEA NULL,		-- Part of the binary data field setup
	"avatarMimeType" VARCHAR(100) NULL,		-- Part of the binary data field setup
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "contactId" FOREIGN KEY ("contactId") REFERENCES "Scheduler"."Contact"("id"),		-- Foreign key to the Contact table.
	CONSTRAINT "clientId" FOREIGN KEY ("clientId") REFERENCES "Scheduler"."Client"("id"),		-- Foreign key to the Client table.
	CONSTRAINT "householdId" FOREIGN KEY ("householdId") REFERENCES "Scheduler"."Household"("id"),		-- Foreign key to the Household table.
	CONSTRAINT "constituentJourneyStageId" FOREIGN KEY ("constituentJourneyStageId") REFERENCES "Scheduler"."ConstituentJourneyStage"("id"),		-- Foreign key to the ConstituentJourneyStage table.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id")		-- Foreign key to the Icon table.
);
-- Index on the Constituent table's tenantGuid field.
CREATE INDEX "I_Constituent_tenantGuid" ON "Scheduler"."Constituent" ("tenantGuid")
;

-- Index on the Constituent table's tenantGuid,contactId fields.
CREATE INDEX "I_Constituent_tenantGuid_contactId" ON "Scheduler"."Constituent" ("tenantGuid", "contactId")
;

-- Index on the Constituent table's tenantGuid,clientId fields.
CREATE INDEX "I_Constituent_tenantGuid_clientId" ON "Scheduler"."Constituent" ("tenantGuid", "clientId")
;

-- Index on the Constituent table's tenantGuid,householdId fields.
CREATE INDEX "I_Constituent_tenantGuid_householdId" ON "Scheduler"."Constituent" ("tenantGuid", "householdId")
;

-- Index on the Constituent table's tenantGuid,constituentJourneyStageId fields.
CREATE INDEX "I_Constituent_tenantGuid_constituentJourneyStageId" ON "Scheduler"."Constituent" ("tenantGuid", "constituentJourneyStageId")
;

-- Index on the Constituent table's tenantGuid,iconId fields.
CREATE INDEX "I_Constituent_tenantGuid_iconId" ON "Scheduler"."Constituent" ("tenantGuid", "iconId")
;

-- Index on the Constituent table's tenantGuid,active fields.
CREATE INDEX "I_Constituent_tenantGuid_active" ON "Scheduler"."Constituent" ("tenantGuid", "active")
;

-- Index on the Constituent table's tenantGuid,deleted fields.
CREATE INDEX "I_Constituent_tenantGuid_deleted" ON "Scheduler"."Constituent" ("tenantGuid", "deleted")
;


-- The change history for records from the Constituent table.
CREATE TABLE "Scheduler"."ConstituentChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"constituentId" INT NOT NULL,		-- Link to the Constituent table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "constituentId" FOREIGN KEY ("constituentId") REFERENCES "Scheduler"."Constituent"("id")		-- Foreign key to the Constituent table.
);
-- Index on the ConstituentChangeHistory table's tenantGuid field.
CREATE INDEX "I_ConstituentChangeHistory_tenantGuid" ON "Scheduler"."ConstituentChangeHistory" ("tenantGuid")
;

-- Index on the ConstituentChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ConstituentChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."ConstituentChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ConstituentChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ConstituentChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."ConstituentChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ConstituentChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ConstituentChangeHistory_tenantGuid_userId" ON "Scheduler"."ConstituentChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ConstituentChangeHistory table's tenantGuid,constituentId fields.
CREATE INDEX "I_ConstituentChangeHistory_tenantGuid_constituentId" ON "Scheduler"."ConstituentChangeHistory" ("tenantGuid", "constituentId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
 ====================================================================================================
   PLEDGES
   A promise to pay. Gifts will link to this to "pay it down".
   ====================================================================================================
*/
CREATE TABLE "Scheduler"."Pledge"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"constituentId" INT NOT NULL,		-- Link to the Constituent table.
	"totalAmount" DECIMAL(11,2) NOT NULL,
	"balanceAmount" DECIMAL(11,2) NOT NULL,		-- Calculated: Total - Sum(LinkedGifts)
	"pledgeDate" DATE NOT NULL,
	"startDate" DATE NULL,
	"endDate" DATE NULL,
	"recurrenceFrequencyId" INT NULL,		-- Link to the RecurrenceFrequency table.
	"fundId" INT NOT NULL,		-- Link to the Fund table.
	"campaignId" INT NULL,		-- Link to the Campaign table.
	"appealId" INT NULL,		-- Link to the Appeal table.
	"writeOffAmount" DECIMAL(11,2) NOT NULL,		-- If they default on the pledge
	"isWrittenOff" BOOLEAN NOT NULL DEFAULT false,
	"notes" TEXT NULL,
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "constituentId" FOREIGN KEY ("constituentId") REFERENCES "Scheduler"."Constituent"("id"),		-- Foreign key to the Constituent table.
	CONSTRAINT "recurrenceFrequencyId" FOREIGN KEY ("recurrenceFrequencyId") REFERENCES "Scheduler"."RecurrenceFrequency"("id"),		-- Foreign key to the RecurrenceFrequency table.
	CONSTRAINT "fundId" FOREIGN KEY ("fundId") REFERENCES "Scheduler"."Fund"("id"),		-- Foreign key to the Fund table.
	CONSTRAINT "campaignId" FOREIGN KEY ("campaignId") REFERENCES "Scheduler"."Campaign"("id"),		-- Foreign key to the Campaign table.
	CONSTRAINT "appealId" FOREIGN KEY ("appealId") REFERENCES "Scheduler"."Appeal"("id")		-- Foreign key to the Appeal table.
);
-- Index on the Pledge table's tenantGuid field.
CREATE INDEX "I_Pledge_tenantGuid" ON "Scheduler"."Pledge" ("tenantGuid")
;

-- Index on the Pledge table's tenantGuid,constituentId fields.
CREATE INDEX "I_Pledge_tenantGuid_constituentId" ON "Scheduler"."Pledge" ("tenantGuid", "constituentId")
;

-- Index on the Pledge table's tenantGuid,recurrenceFrequencyId fields.
CREATE INDEX "I_Pledge_tenantGuid_recurrenceFrequencyId" ON "Scheduler"."Pledge" ("tenantGuid", "recurrenceFrequencyId")
;

-- Index on the Pledge table's tenantGuid,fundId fields.
CREATE INDEX "I_Pledge_tenantGuid_fundId" ON "Scheduler"."Pledge" ("tenantGuid", "fundId")
;

-- Index on the Pledge table's tenantGuid,campaignId fields.
CREATE INDEX "I_Pledge_tenantGuid_campaignId" ON "Scheduler"."Pledge" ("tenantGuid", "campaignId")
;

-- Index on the Pledge table's tenantGuid,appealId fields.
CREATE INDEX "I_Pledge_tenantGuid_appealId" ON "Scheduler"."Pledge" ("tenantGuid", "appealId")
;

-- Index on the Pledge table's tenantGuid,active fields.
CREATE INDEX "I_Pledge_tenantGuid_active" ON "Scheduler"."Pledge" ("tenantGuid", "active")
;

-- Index on the Pledge table's tenantGuid,deleted fields.
CREATE INDEX "I_Pledge_tenantGuid_deleted" ON "Scheduler"."Pledge" ("tenantGuid", "deleted")
;


-- The change history for records from the Pledge table.
CREATE TABLE "Scheduler"."PledgeChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"pledgeId" INT NOT NULL,		-- Link to the Pledge table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "pledgeId" FOREIGN KEY ("pledgeId") REFERENCES "Scheduler"."Pledge"("id")		-- Foreign key to the Pledge table.
);
-- Index on the PledgeChangeHistory table's tenantGuid field.
CREATE INDEX "I_PledgeChangeHistory_tenantGuid" ON "Scheduler"."PledgeChangeHistory" ("tenantGuid")
;

-- Index on the PledgeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_PledgeChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."PledgeChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the PledgeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_PledgeChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."PledgeChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the PledgeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_PledgeChangeHistory_tenantGuid_userId" ON "Scheduler"."PledgeChangeHistory" ("tenantGuid", "userId")
;

-- Index on the PledgeChangeHistory table's tenantGuid,pledgeId fields.
CREATE INDEX "I_PledgeChangeHistory_tenantGuid_pledgeId" ON "Scheduler"."PledgeChangeHistory" ("tenantGuid", "pledgeId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Master list of tribute types ( memory, honor, etc..)
CREATE TABLE "Scheduler"."TributeType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the TributeType table's name field.
CREATE INDEX "I_TributeType_name" ON "Scheduler"."TributeType" ("name")
;

-- Index on the TributeType table's active field.
CREATE INDEX "I_TributeType_active" ON "Scheduler"."TributeType" ("active")
;

-- Index on the TributeType table's deleted field.
CREATE INDEX "I_TributeType_deleted" ON "Scheduler"."TributeType" ("deleted")
;

INSERT INTO "Scheduler"."TributeType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'In Memory Of', 'In Memory Of', 1, '27781845-ed5e-4bba-9216-751d5a8d778a' );

INSERT INTO "Scheduler"."TributeType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'In Honor Of', 'In Honor Of', 2, '31af7566-28d1-460f-9cd9-9d70711b5983' );


/*
====================================================================================================
   BATCH CONTROL
   This prevents data entry errors by forcing the user to balance "Control Totals" vs "Actual Totals".
   ====================================================================================================
*/
CREATE TABLE "Scheduler"."BatchStatus"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the BatchStatus table's name field.
CREATE INDEX "I_BatchStatus_name" ON "Scheduler"."BatchStatus" ("name")
;

-- Index on the BatchStatus table's active field.
CREATE INDEX "I_BatchStatus_active" ON "Scheduler"."BatchStatus" ("active")
;

-- Index on the BatchStatus table's deleted field.
CREATE INDEX "I_BatchStatus_deleted" ON "Scheduler"."BatchStatus" ("deleted")
;

INSERT INTO "Scheduler"."BatchStatus" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Open', 'Data entry in progress', 1, 'd87c06b0-9b5e-4597-8968-ad5f987e2afd' );

INSERT INTO "Scheduler"."BatchStatus" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Balanced', 'Control totals match entry totals', 2, 'b5942c13-47d1-4753-a655-140454e1d0a4' );

INSERT INTO "Scheduler"."BatchStatus" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Posted', 'Transactions committed to GL / Donor History', 3, '640a7bb7-59da-423b-b2e5-a10124594331' );

INSERT INTO "Scheduler"."BatchStatus" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Closed', 'Closed', 4, '5c60e28a-ba9f-4098-9a04-50fcb139bd8c' );


-- The Batch Header for processing gifts.
CREATE TABLE "Scheduler"."Batch"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"batchNumber" VARCHAR(100) NOT NULL,		-- User-facing ID (e.g., "2026-01-15-MAIL"
	"description" VARCHAR(500) NULL,
	"dateOpened" TIMESTAMP NOT NULL,
	"datePosted" TIMESTAMP NULL,
	"batchStatusId" INT NOT NULL,		-- Link to the BatchStatus table.
	"controlAmount" DECIMAL(11,2) NOT NULL DEFAULT 0,
	"controlCount" INT NOT NULL DEFAULT 0,
	"defaultFundId" INT NULL,		-- Optional default fund
	"defaultCampaignId" INT NULL,		-- Optional default campaign
	"defaultAppealId" INT NULL,		-- Optional default appeal
	"defaultDate" DATE NULL,		-- Optional default date
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "batchStatusId" FOREIGN KEY ("batchStatusId") REFERENCES "Scheduler"."BatchStatus"("id"),		-- Foreign key to the BatchStatus table.
	CONSTRAINT "defaultFundId" FOREIGN KEY ("defaultFundId") REFERENCES "Scheduler"."Fund"("id"),		-- Foreign key to the Fund table.
	CONSTRAINT "defaultCampaignId" FOREIGN KEY ("defaultCampaignId") REFERENCES "Scheduler"."Campaign"("id"),		-- Foreign key to the Campaign table.
	CONSTRAINT "defaultAppealId" FOREIGN KEY ("defaultAppealId") REFERENCES "Scheduler"."Appeal"("id")		-- Foreign key to the Appeal table.
);
-- Index on the Batch table's tenantGuid field.
CREATE INDEX "I_Batch_tenantGuid" ON "Scheduler"."Batch" ("tenantGuid")
;

-- Index on the Batch table's tenantGuid,batchStatusId fields.
CREATE INDEX "I_Batch_tenantGuid_batchStatusId" ON "Scheduler"."Batch" ("tenantGuid", "batchStatusId")
;

-- Index on the Batch table's tenantGuid,defaultFundId fields.
CREATE INDEX "I_Batch_tenantGuid_defaultFundId" ON "Scheduler"."Batch" ("tenantGuid", "defaultFundId")
;

-- Index on the Batch table's tenantGuid,defaultCampaignId fields.
CREATE INDEX "I_Batch_tenantGuid_defaultCampaignId" ON "Scheduler"."Batch" ("tenantGuid", "defaultCampaignId")
;

-- Index on the Batch table's tenantGuid,defaultAppealId fields.
CREATE INDEX "I_Batch_tenantGuid_defaultAppealId" ON "Scheduler"."Batch" ("tenantGuid", "defaultAppealId")
;

-- Index on the Batch table's tenantGuid,active fields.
CREATE INDEX "I_Batch_tenantGuid_active" ON "Scheduler"."Batch" ("tenantGuid", "active")
;

-- Index on the Batch table's tenantGuid,deleted fields.
CREATE INDEX "I_Batch_tenantGuid_deleted" ON "Scheduler"."Batch" ("tenantGuid", "deleted")
;


-- The change history for records from the Batch table.
CREATE TABLE "Scheduler"."BatchChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"batchId" INT NOT NULL,		-- Link to the Batch table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "batchId" FOREIGN KEY ("batchId") REFERENCES "Scheduler"."Batch"("id")		-- Foreign key to the Batch table.
);
-- Index on the BatchChangeHistory table's tenantGuid field.
CREATE INDEX "I_BatchChangeHistory_tenantGuid" ON "Scheduler"."BatchChangeHistory" ("tenantGuid")
;

-- Index on the BatchChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_BatchChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."BatchChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the BatchChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_BatchChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."BatchChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the BatchChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_BatchChangeHistory_tenantGuid_userId" ON "Scheduler"."BatchChangeHistory" ("tenantGuid", "userId")
;

-- Index on the BatchChangeHistory table's tenantGuid,batchId fields.
CREATE INDEX "I_BatchChangeHistory_tenantGuid_batchId" ON "Scheduler"."BatchChangeHistory" ("tenantGuid", "batchId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- The Tribute Definition (e.g., "The John Doe Memorial Fund")
CREATE TABLE "Scheduler"."Tribute"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"tributeTypeId" INT NULL,		-- Link to the TributeType table.
	"defaultAcknowledgeeId" INT NULL,		-- Constituent to notify (e.g., the widow)
	"startDate" DATE NULL,
	"endDate" DATE NULL,
	"iconId" INT NULL,		-- Icon to use for UI display
	"color" VARCHAR(10) NULL,		-- Hex color for UI display
	"avatarFileName" VARCHAR(250) NULL,		-- Part of the binary data field setup
	"avatarSize" BIGINT NULL,		-- Part of the binary data field setup
	"avatarData" BYTEA NULL,		-- Part of the binary data field setup
	"avatarMimeType" VARCHAR(100) NULL,		-- Part of the binary data field setup
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "tributeTypeId" FOREIGN KEY ("tributeTypeId") REFERENCES "Scheduler"."TributeType"("id"),		-- Foreign key to the TributeType table.
	CONSTRAINT "defaultAcknowledgeeId" FOREIGN KEY ("defaultAcknowledgeeId") REFERENCES "Scheduler"."Constituent"("id"),		-- Foreign key to the Constituent table.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_Tribute_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Tribute table's tenantGuid and name fields.
);
-- Index on the Tribute table's tenantGuid field.
CREATE INDEX "I_Tribute_tenantGuid" ON "Scheduler"."Tribute" ("tenantGuid")
;

-- Index on the Tribute table's tenantGuid,name fields.
CREATE INDEX "I_Tribute_tenantGuid_name" ON "Scheduler"."Tribute" ("tenantGuid", "name")
;

-- Index on the Tribute table's tenantGuid,tributeTypeId fields.
CREATE INDEX "I_Tribute_tenantGuid_tributeTypeId" ON "Scheduler"."Tribute" ("tenantGuid", "tributeTypeId")
;

-- Index on the Tribute table's tenantGuid,iconId fields.
CREATE INDEX "I_Tribute_tenantGuid_iconId" ON "Scheduler"."Tribute" ("tenantGuid", "iconId")
;

-- Index on the Tribute table's tenantGuid,active fields.
CREATE INDEX "I_Tribute_tenantGuid_active" ON "Scheduler"."Tribute" ("tenantGuid", "active")
;

-- Index on the Tribute table's tenantGuid,deleted fields.
CREATE INDEX "I_Tribute_tenantGuid_deleted" ON "Scheduler"."Tribute" ("tenantGuid", "deleted")
;


-- The change history for records from the Tribute table.
CREATE TABLE "Scheduler"."TributeChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"tributeId" INT NOT NULL,		-- Link to the Tribute table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "tributeId" FOREIGN KEY ("tributeId") REFERENCES "Scheduler"."Tribute"("id")		-- Foreign key to the Tribute table.
);
-- Index on the TributeChangeHistory table's tenantGuid field.
CREATE INDEX "I_TributeChangeHistory_tenantGuid" ON "Scheduler"."TributeChangeHistory" ("tenantGuid")
;

-- Index on the TributeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_TributeChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."TributeChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the TributeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_TributeChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."TributeChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the TributeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_TributeChangeHistory_tenantGuid_userId" ON "Scheduler"."TributeChangeHistory" ("tenantGuid", "userId")
;

-- Index on the TributeChangeHistory table's tenantGuid,tributeId fields.
CREATE INDEX "I_TributeChangeHistory_tenantGuid_tributeId" ON "Scheduler"."TributeChangeHistory" ("tenantGuid", "tributeId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
  ====================================================================================================
   GIFTS (Transactions)
   The money coming in.
   ====================================================================================================
*/
CREATE TABLE "Scheduler"."Gift"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"officeId" INT NULL,		-- Which office received/owns this gift
	"constituentId" INT NOT NULL,		-- Link to the Constituent table.
	"pledgeId" INT NULL,		-- Link to the Pledge table.
	"amount" DECIMAL(11,2) NOT NULL,
	"receivedDate" TIMESTAMP NOT NULL,		-- When it was recieved
	"postedDate" TIMESTAMP NULL,		-- When it hit the GL
	"fundId" INT NOT NULL,		-- Link to the Fund table.
	"campaignId" INT NULL,		-- Link to the Campaign table.
	"appealId" INT NULL,		-- Link to the Appeal table.
	"paymentTypeId" INT NOT NULL,		-- Link to the PaymentType table.
	"referenceNumber" VARCHAR(100) NULL,		-- Check # or Transaction ID
	"batchId" INT NULL,		-- Link to processing batch
	"receiptTypeId" INT NULL,		-- Link to the ReceiptType table.
	"receiptDate" TIMESTAMP NULL,
	"tributeId" INT NULL,		-- Link to the Tribute table.
	"notes" TEXT NULL,
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "officeId" FOREIGN KEY ("officeId") REFERENCES "Scheduler"."Office"("id"),		-- Foreign key to the Office table.
	CONSTRAINT "constituentId" FOREIGN KEY ("constituentId") REFERENCES "Scheduler"."Constituent"("id"),		-- Foreign key to the Constituent table.
	CONSTRAINT "pledgeId" FOREIGN KEY ("pledgeId") REFERENCES "Scheduler"."Pledge"("id"),		-- Foreign key to the Pledge table.
	CONSTRAINT "fundId" FOREIGN KEY ("fundId") REFERENCES "Scheduler"."Fund"("id"),		-- Foreign key to the Fund table.
	CONSTRAINT "campaignId" FOREIGN KEY ("campaignId") REFERENCES "Scheduler"."Campaign"("id"),		-- Foreign key to the Campaign table.
	CONSTRAINT "appealId" FOREIGN KEY ("appealId") REFERENCES "Scheduler"."Appeal"("id"),		-- Foreign key to the Appeal table.
	CONSTRAINT "paymentTypeId" FOREIGN KEY ("paymentTypeId") REFERENCES "Scheduler"."PaymentType"("id"),		-- Foreign key to the PaymentType table.
	CONSTRAINT "batchId" FOREIGN KEY ("batchId") REFERENCES "Scheduler"."Batch"("id"),		-- Foreign key to the Batch table.
	CONSTRAINT "receiptTypeId" FOREIGN KEY ("receiptTypeId") REFERENCES "Scheduler"."ReceiptType"("id"),		-- Foreign key to the ReceiptType table.
	CONSTRAINT "tributeId" FOREIGN KEY ("tributeId") REFERENCES "Scheduler"."Tribute"("id")		-- Foreign key to the Tribute table.
);
-- Index on the Gift table's tenantGuid field.
CREATE INDEX "I_Gift_tenantGuid" ON "Scheduler"."Gift" ("tenantGuid")
;

-- Index on the Gift table's tenantGuid,officeId fields.
CREATE INDEX "I_Gift_tenantGuid_officeId" ON "Scheduler"."Gift" ("tenantGuid", "officeId")
;

-- Index on the Gift table's tenantGuid,constituentId fields.
CREATE INDEX "I_Gift_tenantGuid_constituentId" ON "Scheduler"."Gift" ("tenantGuid", "constituentId")
;

-- Index on the Gift table's tenantGuid,pledgeId fields.
CREATE INDEX "I_Gift_tenantGuid_pledgeId" ON "Scheduler"."Gift" ("tenantGuid", "pledgeId")
;

-- Index on the Gift table's tenantGuid,fundId fields.
CREATE INDEX "I_Gift_tenantGuid_fundId" ON "Scheduler"."Gift" ("tenantGuid", "fundId")
;

-- Index on the Gift table's tenantGuid,campaignId fields.
CREATE INDEX "I_Gift_tenantGuid_campaignId" ON "Scheduler"."Gift" ("tenantGuid", "campaignId")
;

-- Index on the Gift table's tenantGuid,appealId fields.
CREATE INDEX "I_Gift_tenantGuid_appealId" ON "Scheduler"."Gift" ("tenantGuid", "appealId")
;

-- Index on the Gift table's tenantGuid,paymentTypeId fields.
CREATE INDEX "I_Gift_tenantGuid_paymentTypeId" ON "Scheduler"."Gift" ("tenantGuid", "paymentTypeId")
;

-- Index on the Gift table's tenantGuid,batchId fields.
CREATE INDEX "I_Gift_tenantGuid_batchId" ON "Scheduler"."Gift" ("tenantGuid", "batchId")
;

-- Index on the Gift table's tenantGuid,receiptTypeId fields.
CREATE INDEX "I_Gift_tenantGuid_receiptTypeId" ON "Scheduler"."Gift" ("tenantGuid", "receiptTypeId")
;

-- Index on the Gift table's tenantGuid,tributeId fields.
CREATE INDEX "I_Gift_tenantGuid_tributeId" ON "Scheduler"."Gift" ("tenantGuid", "tributeId")
;

-- Index on the Gift table's tenantGuid,active fields.
CREATE INDEX "I_Gift_tenantGuid_active" ON "Scheduler"."Gift" ("tenantGuid", "active")
;

-- Index on the Gift table's tenantGuid,deleted fields.
CREATE INDEX "I_Gift_tenantGuid_deleted" ON "Scheduler"."Gift" ("tenantGuid", "deleted")
;


-- The change history for records from the Gift table.
CREATE TABLE "Scheduler"."GiftChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"giftId" INT NOT NULL,		-- Link to the Gift table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "giftId" FOREIGN KEY ("giftId") REFERENCES "Scheduler"."Gift"("id")		-- Foreign key to the Gift table.
);
-- Index on the GiftChangeHistory table's tenantGuid field.
CREATE INDEX "I_GiftChangeHistory_tenantGuid" ON "Scheduler"."GiftChangeHistory" ("tenantGuid")
;

-- Index on the GiftChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_GiftChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."GiftChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the GiftChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_GiftChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."GiftChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the GiftChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_GiftChangeHistory_tenantGuid_userId" ON "Scheduler"."GiftChangeHistory" ("tenantGuid", "userId")
;

-- Index on the GiftChangeHistory table's tenantGuid,giftId fields.
CREATE INDEX "I_GiftChangeHistory_tenantGuid_giftId" ON "Scheduler"."GiftChangeHistory" ("tenantGuid", "giftId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
   SOFT CREDITS
   Critical for DP functionality. Allows a gift from "Husband" to also show up on "Wife's" record 
   without doubling the financial totals.
   ====================================================================================================
*/
CREATE TABLE "Scheduler"."SoftCredit"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"giftId" INT NOT NULL,		-- Link to the Gift table.
	"constituentId" INT NOT NULL,		-- The person getting the soft credit
	"amount" DECIMAL(11,2) NOT NULL,		-- Might be full amount or partial
	"notes" TEXT NULL,
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "giftId" FOREIGN KEY ("giftId") REFERENCES "Scheduler"."Gift"("id"),		-- Foreign key to the Gift table.
	CONSTRAINT "constituentId" FOREIGN KEY ("constituentId") REFERENCES "Scheduler"."Constituent"("id")		-- Foreign key to the Constituent table.
);
-- Index on the SoftCredit table's tenantGuid field.
CREATE INDEX "I_SoftCredit_tenantGuid" ON "Scheduler"."SoftCredit" ("tenantGuid")
;

-- Index on the SoftCredit table's tenantGuid,giftId fields.
CREATE INDEX "I_SoftCredit_tenantGuid_giftId" ON "Scheduler"."SoftCredit" ("tenantGuid", "giftId")
;

-- Index on the SoftCredit table's tenantGuid,constituentId fields.
CREATE INDEX "I_SoftCredit_tenantGuid_constituentId" ON "Scheduler"."SoftCredit" ("tenantGuid", "constituentId")
;

-- Index on the SoftCredit table's tenantGuid,active fields.
CREATE INDEX "I_SoftCredit_tenantGuid_active" ON "Scheduler"."SoftCredit" ("tenantGuid", "active")
;

-- Index on the SoftCredit table's tenantGuid,deleted fields.
CREATE INDEX "I_SoftCredit_tenantGuid_deleted" ON "Scheduler"."SoftCredit" ("tenantGuid", "deleted")
;


-- The change history for records from the SoftCredit table.
CREATE TABLE "Scheduler"."SoftCreditChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"softCreditId" INT NOT NULL,		-- Link to the SoftCredit table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "softCreditId" FOREIGN KEY ("softCreditId") REFERENCES "Scheduler"."SoftCredit"("id")		-- Foreign key to the SoftCredit table.
);
-- Index on the SoftCreditChangeHistory table's tenantGuid field.
CREATE INDEX "I_SoftCreditChangeHistory_tenantGuid" ON "Scheduler"."SoftCreditChangeHistory" ("tenantGuid")
;

-- Index on the SoftCreditChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_SoftCreditChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."SoftCreditChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the SoftCreditChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_SoftCreditChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."SoftCreditChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the SoftCreditChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_SoftCreditChangeHistory_tenantGuid_userId" ON "Scheduler"."SoftCreditChangeHistory" ("tenantGuid", "userId")
;

-- Index on the SoftCreditChangeHistory table's tenantGuid,softCreditId fields.
CREATE INDEX "I_SoftCreditChangeHistory_tenantGuid_softCreditId" ON "Scheduler"."SoftCreditChangeHistory" ("tenantGuid", "softCreditId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
Volunteer-specific extended profile.
One-to-one with Resource — allows volunteers to be scheduled just like paid resources
while carrying volunteer-specific metadata, hours tracking, preferences, etc.
*/
CREATE TABLE "Scheduler"."VolunteerProfile"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"resourceId" INT NOT NULL,		-- The Resource this volunteer profile belongs to (1:1)
	"volunteerStatusId" INT NOT NULL,		-- Current lifecycle status of this volunteer
	"onboardedDate" DATE NULL,		-- Date volunteer was approved/onboarded
	"inactiveSince" DATE NULL,		-- If inactive, when they went inactive
	"totalHoursServed" REAL NULL DEFAULT 0,		-- Cached/rolled-up lifetime volunteer hours
	"lastActivityDate" DATE NULL,		-- Most recent event/assignment end date
	"backgroundCheckCompleted" BOOLEAN NOT NULL DEFAULT false,
	"backgroundCheckDate" DATE NULL,
	"backgroundCheckExpiry" DATE NULL,
	"confidentialityAgreementSigned" BOOLEAN NOT NULL DEFAULT false,
	"confidentialityAgreementDate" DATE NULL,
	"availabilityPreferences" TEXT NULL,		-- Free text or structured JSON: e.g. 'prefers weekends', 'no evenings after 8pm'
	"interestsAndSkillsNotes" TEXT NULL,		-- Self-reported interests, hobbies, or extra skills
	"emergencyContactNotes" TEXT NULL,		-- Any special emergency instructions or notes
	"constituentId" INT NULL,		-- Optional link to fundraising/constituent record if relevant
	"iconId" INT NULL,		-- Optional override icon for volunteer-specific UI
	"color" VARCHAR(10) NULL,		-- Optional override color
	"attributes" TEXT NULL,		-- Arbitrary JSON for future extension
	"linkedUserGuid" VARCHAR(50) NULL,		-- Security user GUID for self-service Hub access
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "resourceId" FOREIGN KEY ("resourceId") REFERENCES "Scheduler"."Resource"("id"),		-- Foreign key to the Resource table.
	CONSTRAINT "volunteerStatusId" FOREIGN KEY ("volunteerStatusId") REFERENCES "Scheduler"."VolunteerStatus"("id"),		-- Foreign key to the VolunteerStatus table.
	CONSTRAINT "constituentId" FOREIGN KEY ("constituentId") REFERENCES "Scheduler"."Constituent"("id"),		-- Foreign key to the Constituent table.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_VolunteerProfile_tenantGuid_resourceId" UNIQUE ( "tenantGuid", "resourceId") 		-- Uniqueness enforced on the VolunteerProfile table's tenantGuid and resourceId fields.
);
-- Index on the VolunteerProfile table's tenantGuid field.
CREATE INDEX "I_VolunteerProfile_tenantGuid" ON "Scheduler"."VolunteerProfile" ("tenantGuid")
;

-- Index on the VolunteerProfile table's tenantGuid,resourceId fields.
CREATE INDEX "I_VolunteerProfile_tenantGuid_resourceId" ON "Scheduler"."VolunteerProfile" ("tenantGuid", "resourceId")
;

-- Index on the VolunteerProfile table's tenantGuid,volunteerStatusId fields.
CREATE INDEX "I_VolunteerProfile_tenantGuid_volunteerStatusId" ON "Scheduler"."VolunteerProfile" ("tenantGuid", "volunteerStatusId")
;

-- Index on the VolunteerProfile table's tenantGuid,constituentId fields.
CREATE INDEX "I_VolunteerProfile_tenantGuid_constituentId" ON "Scheduler"."VolunteerProfile" ("tenantGuid", "constituentId")
;

-- Index on the VolunteerProfile table's tenantGuid,iconId fields.
CREATE INDEX "I_VolunteerProfile_tenantGuid_iconId" ON "Scheduler"."VolunteerProfile" ("tenantGuid", "iconId")
;

-- Index on the VolunteerProfile table's tenantGuid,active fields.
CREATE INDEX "I_VolunteerProfile_tenantGuid_active" ON "Scheduler"."VolunteerProfile" ("tenantGuid", "active")
;

-- Index on the VolunteerProfile table's tenantGuid,deleted fields.
CREATE INDEX "I_VolunteerProfile_tenantGuid_deleted" ON "Scheduler"."VolunteerProfile" ("tenantGuid", "deleted")
;


-- The change history for records from the VolunteerProfile table.
CREATE TABLE "Scheduler"."VolunteerProfileChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"volunteerProfileId" INT NOT NULL,		-- Link to the VolunteerProfile table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "volunteerProfileId" FOREIGN KEY ("volunteerProfileId") REFERENCES "Scheduler"."VolunteerProfile"("id")		-- Foreign key to the VolunteerProfile table.
);
-- Index on the VolunteerProfileChangeHistory table's tenantGuid field.
CREATE INDEX "I_VolunteerProfileChangeHistory_tenantGuid" ON "Scheduler"."VolunteerProfileChangeHistory" ("tenantGuid")
;

-- Index on the VolunteerProfileChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_VolunteerProfileChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."VolunteerProfileChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the VolunteerProfileChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_VolunteerProfileChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."VolunteerProfileChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the VolunteerProfileChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_VolunteerProfileChangeHistory_tenantGuid_userId" ON "Scheduler"."VolunteerProfileChangeHistory" ("tenantGuid", "userId")
;

-- Index on the VolunteerProfileChangeHistory table's tenantGuid,volunteerProfileId fields.
CREATE INDEX "I_VolunteerProfileChangeHistory_tenantGuid_volunteerProfileId" ON "Scheduler"."VolunteerProfileChangeHistory" ("tenantGuid", "volunteerProfileId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
Named, persistent groups of volunteers that are often scheduled together.
Examples: 'Saturday Soup Kitchen Team', 'Festival Setup Crew', 'Board of Directors Helpers'.
Similar to Crew table but volunteer-specific with lighter structure and volunteer-oriented metadata.
*/
CREATE TABLE "Scheduler"."VolunteerGroup"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"purpose" TEXT NULL,		-- What this group is mainly used for (e.g. 'Food distribution', 'Event setup & teardown')
	"officeId" INT NULL,		-- Optional office/branch this volunteer group is associated with
	"volunteerStatusId" INT NULL,		-- Minimum status required for members (e.g. Active only)
	"maxMembers" INT NULL,		-- Optional soft cap on group size
	"iconId" INT NULL,		-- Icon for UI display (e.g. group of people, soup bowl, hammer)
	"color" VARCHAR(10) NULL,		-- Suggested color for calendar/events
	"notes" TEXT NULL,
	"avatarFileName" VARCHAR(250) NULL,		-- Part of the binary data field setup
	"avatarSize" BIGINT NULL,		-- Part of the binary data field setup
	"avatarData" BYTEA NULL,		-- Part of the binary data field setup
	"avatarMimeType" VARCHAR(100) NULL,		-- Part of the binary data field setup
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "officeId" FOREIGN KEY ("officeId") REFERENCES "Scheduler"."Office"("id"),		-- Foreign key to the Office table.
	CONSTRAINT "volunteerStatusId" FOREIGN KEY ("volunteerStatusId") REFERENCES "Scheduler"."VolunteerStatus"("id"),		-- Foreign key to the VolunteerStatus table.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_VolunteerGroup_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the VolunteerGroup table's tenantGuid and name fields.
);
-- Index on the VolunteerGroup table's tenantGuid field.
CREATE INDEX "I_VolunteerGroup_tenantGuid" ON "Scheduler"."VolunteerGroup" ("tenantGuid")
;

-- Index on the VolunteerGroup table's tenantGuid,name fields.
CREATE INDEX "I_VolunteerGroup_tenantGuid_name" ON "Scheduler"."VolunteerGroup" ("tenantGuid", "name")
;

-- Index on the VolunteerGroup table's tenantGuid,officeId fields.
CREATE INDEX "I_VolunteerGroup_tenantGuid_officeId" ON "Scheduler"."VolunteerGroup" ("tenantGuid", "officeId")
;

-- Index on the VolunteerGroup table's tenantGuid,volunteerStatusId fields.
CREATE INDEX "I_VolunteerGroup_tenantGuid_volunteerStatusId" ON "Scheduler"."VolunteerGroup" ("tenantGuid", "volunteerStatusId")
;

-- Index on the VolunteerGroup table's tenantGuid,iconId fields.
CREATE INDEX "I_VolunteerGroup_tenantGuid_iconId" ON "Scheduler"."VolunteerGroup" ("tenantGuid", "iconId")
;

-- Index on the VolunteerGroup table's tenantGuid,active fields.
CREATE INDEX "I_VolunteerGroup_tenantGuid_active" ON "Scheduler"."VolunteerGroup" ("tenantGuid", "active")
;

-- Index on the VolunteerGroup table's tenantGuid,deleted fields.
CREATE INDEX "I_VolunteerGroup_tenantGuid_deleted" ON "Scheduler"."VolunteerGroup" ("tenantGuid", "deleted")
;


-- The change history for records from the VolunteerGroup table.
CREATE TABLE "Scheduler"."VolunteerGroupChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"volunteerGroupId" INT NOT NULL,		-- Link to the VolunteerGroup table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "volunteerGroupId" FOREIGN KEY ("volunteerGroupId") REFERENCES "Scheduler"."VolunteerGroup"("id")		-- Foreign key to the VolunteerGroup table.
);
-- Index on the VolunteerGroupChangeHistory table's tenantGuid field.
CREATE INDEX "I_VolunteerGroupChangeHistory_tenantGuid" ON "Scheduler"."VolunteerGroupChangeHistory" ("tenantGuid")
;

-- Index on the VolunteerGroupChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_VolunteerGroupChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."VolunteerGroupChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the VolunteerGroupChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_VolunteerGroupChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."VolunteerGroupChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the VolunteerGroupChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_VolunteerGroupChangeHistory_tenantGuid_userId" ON "Scheduler"."VolunteerGroupChangeHistory" ("tenantGuid", "userId")
;

-- Index on the VolunteerGroupChangeHistory table's tenantGuid,volunteerGroupId fields.
CREATE INDEX "I_VolunteerGroupChangeHistory_tenantGuid_volunteerGroupId" ON "Scheduler"."VolunteerGroupChangeHistory" ("tenantGuid", "volunteerGroupId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
Membership in a VolunteerGroup.
Links Resources (volunteers) to groups, with optional default role and sequence.
*/
CREATE TABLE "Scheduler"."VolunteerGroupMember"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"volunteerGroupId" INT NOT NULL,		-- Link to the VolunteerGroup table.
	"resourceId" INT NOT NULL,		-- The volunteer (Resource) in this group
	"assignmentRoleId" INT NULL,		-- Default role this person plays in the group (e.g. 'Team Lead', 'Driver')
	"sequence" INT NOT NULL DEFAULT 1,		-- Display/order position within the group
	"joinedDate" DATE NULL,
	"leftDate" DATE NULL,		-- If they left the group
	"notes" TEXT NULL,		-- e.g. 'Prefers kitchen duties', 'Only available 1st Saturday'
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "volunteerGroupId" FOREIGN KEY ("volunteerGroupId") REFERENCES "Scheduler"."VolunteerGroup"("id"),		-- Foreign key to the VolunteerGroup table.
	CONSTRAINT "resourceId" FOREIGN KEY ("resourceId") REFERENCES "Scheduler"."Resource"("id"),		-- Foreign key to the Resource table.
	CONSTRAINT "assignmentRoleId" FOREIGN KEY ("assignmentRoleId") REFERENCES "Scheduler"."AssignmentRole"("id"),		-- Foreign key to the AssignmentRole table.
	CONSTRAINT "UC_VolunteerGroupMember_tenantGuid_volunteerGroupId_resourceId" UNIQUE ( "tenantGuid", "volunteerGroupId", "resourceId") 		-- Uniqueness enforced on the VolunteerGroupMember table's tenantGuid and volunteerGroupId and resourceId fields.
);
-- Index on the VolunteerGroupMember table's tenantGuid field.
CREATE INDEX "I_VolunteerGroupMember_tenantGuid" ON "Scheduler"."VolunteerGroupMember" ("tenantGuid")
;

-- Index on the VolunteerGroupMember table's tenantGuid,volunteerGroupId fields.
CREATE INDEX "I_VolunteerGroupMember_tenantGuid_volunteerGroupId" ON "Scheduler"."VolunteerGroupMember" ("tenantGuid", "volunteerGroupId")
;

-- Index on the VolunteerGroupMember table's tenantGuid,resourceId fields.
CREATE INDEX "I_VolunteerGroupMember_tenantGuid_resourceId" ON "Scheduler"."VolunteerGroupMember" ("tenantGuid", "resourceId")
;

-- Index on the VolunteerGroupMember table's tenantGuid,assignmentRoleId fields.
CREATE INDEX "I_VolunteerGroupMember_tenantGuid_assignmentRoleId" ON "Scheduler"."VolunteerGroupMember" ("tenantGuid", "assignmentRoleId")
;

-- Index on the VolunteerGroupMember table's tenantGuid,active fields.
CREATE INDEX "I_VolunteerGroupMember_tenantGuid_active" ON "Scheduler"."VolunteerGroupMember" ("tenantGuid", "active")
;

-- Index on the VolunteerGroupMember table's tenantGuid,deleted fields.
CREATE INDEX "I_VolunteerGroupMember_tenantGuid_deleted" ON "Scheduler"."VolunteerGroupMember" ("tenantGuid", "deleted")
;


-- The change history for records from the VolunteerGroupMember table.
CREATE TABLE "Scheduler"."VolunteerGroupMemberChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"volunteerGroupMemberId" INT NOT NULL,		-- Link to the VolunteerGroupMember table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "volunteerGroupMemberId" FOREIGN KEY ("volunteerGroupMemberId") REFERENCES "Scheduler"."VolunteerGroupMember"("id")		-- Foreign key to the VolunteerGroupMember table.
);
-- Index on the VolunteerGroupMemberChangeHistory table's tenantGuid field.
CREATE INDEX "I_VolunteerGroupMemberChangeHistory_tenantGuid" ON "Scheduler"."VolunteerGroupMemberChangeHistory" ("tenantGuid")
;

-- Index on the VolunteerGroupMemberChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_VolunteerGroupMemberChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."VolunteerGroupMemberChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the VolunteerGroupMemberChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_VolunteerGroupMemberChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."VolunteerGroupMemberChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the VolunteerGroupMemberChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_VolunteerGroupMemberChangeHistory_tenantGuid_userId" ON "Scheduler"."VolunteerGroupMemberChangeHistory" ("tenantGuid", "userId")
;

-- Index on the VolunteerGroupMemberChangeHistory table's tenantGuid,volunteerGroupMemberId fields.
CREATE INDEX "I_VolunteerGroupMemberChangeHistory_tenantGuid_volunteerGroupMe" ON "Scheduler"."VolunteerGroupMemberChangeHistory" ("tenantGuid", "volunteerGroupMemberId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Master list of document types for classifying attachments (e.g., Rental Agreement, Receipt, Invoice, Photo).
CREATE TABLE "Scheduler"."DocumentType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"color" VARCHAR(10) NULL,		-- Hex color for UI display.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the DocumentType table's name field.
CREATE INDEX "I_DocumentType_name" ON "Scheduler"."DocumentType" ("name")
;

-- Index on the DocumentType table's active field.
CREATE INDEX "I_DocumentType_active" ON "Scheduler"."DocumentType" ("active")
;

-- Index on the DocumentType table's deleted field.
CREATE INDEX "I_DocumentType_deleted" ON "Scheduler"."DocumentType" ("deleted")
;

INSERT INTO "Scheduler"."DocumentType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Rental Agreement', 'Signed rental or usage agreement', 1, 'f1a1b2c3-d4e5-6789-abcd-ef0123456701' );

INSERT INTO "Scheduler"."DocumentType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Receipt', 'Purchase receipt or proof of payment', 2, 'f1a1b2c3-d4e5-6789-abcd-ef0123456702' );

INSERT INTO "Scheduler"."DocumentType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Invoice', 'Invoice issued or received', 3, 'f1a1b2c3-d4e5-6789-abcd-ef0123456703' );

INSERT INTO "Scheduler"."DocumentType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Photo', 'Photograph or image', 4, 'f1a1b2c3-d4e5-6789-abcd-ef0123456704' );

INSERT INTO "Scheduler"."DocumentType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Permit', 'Required permits (liquor license, fire permit, etc.)', 5, 'f1a1b2c3-d4e5-6789-abcd-ef0123456705' );

INSERT INTO "Scheduler"."DocumentType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Background Check', 'Background Check supporting documentation (police etc..)', 6, 'f1a1b2c3-d4e5-6789-abcd-ef0123456706' );

INSERT INTO "Scheduler"."DocumentType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'License', 'License that certifies a resource for a function (driving etc..)', 7, 'f1a1b2c3-d4e5-6789-abcd-ef0123456707' );

INSERT INTO "Scheduler"."DocumentType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Insurance Certificate', 'Liability insurance certificate for event coverage', 8, 'f1a1b2c3-d4e5-6789-abcd-ef0123456708' );

INSERT INTO "Scheduler"."DocumentType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Meeting Minutes', 'The notes from a meeting', 9, 'f1a1b2c3-d4e5-6789-abcd-ef0123456709' );

INSERT INTO "Scheduler"."DocumentType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Other', 'Other document type', 99, 'f1a1b2c3-d4e5-6789-abcd-ef0123456799' );


/*
====================================================================================================
 DOCUMENT FOLDER (File Manager Hierarchy)
 Provides a hierarchical folder structure for organizing documents in an Explorer-like
 file management interface.  Self-referencing via parentDocumentFolderId enables unlimited
 nesting.  Documents link to folders via Document.documentFolderId (nullable — documents
 without a folder are considered root-level / unfiled).

 A unique constraint on (tenantGuid, parentDocumentFolderId, name) prevents duplicate
 folder names at the same hierarchy level within a tenant.
 ====================================================================================================
*/
CREATE TABLE "Scheduler"."DocumentFolder"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(250) NOT NULL,		-- Folder display name.
	"description" VARCHAR(500) NULL,		-- Optional folder description.
	"parentDocumentFolderId" INT NULL,		-- Self-referencing FK for folder hierarchy. NULL = root-level folder.
	"iconId" INT NULL,		-- Optional custom folder icon for UI display.
	"color" VARCHAR(10) NULL,		-- Optional folder color for UI display.
	"sequence" INT NOT NULL DEFAULT 0,		-- Display order among sibling folders.
	"notes" TEXT NULL,		-- Optional notes about the folder.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "parentDocumentFolderId" FOREIGN KEY ("parentDocumentFolderId") REFERENCES "Scheduler"."DocumentFolder"("id"),		-- Foreign key to the DocumentFolder table.
	CONSTRAINT "iconId" FOREIGN KEY ("iconId") REFERENCES "Scheduler"."Icon"("id"),		-- Foreign key to the Icon table.
	CONSTRAINT "UC_DocumentFolder_tenantGuid_parentDocumentFolderId_name" UNIQUE ( "tenantGuid", "parentDocumentFolderId", "name") 		-- Uniqueness enforced on the DocumentFolder table's tenantGuid and parentDocumentFolderId and name fields.
);
-- Index on the DocumentFolder table's tenantGuid field.
CREATE INDEX "I_DocumentFolder_tenantGuid" ON "Scheduler"."DocumentFolder" ("tenantGuid")
;

-- Index on the DocumentFolder table's tenantGuid,parentDocumentFolderId fields.
CREATE INDEX "I_DocumentFolder_tenantGuid_parentDocumentFolderId" ON "Scheduler"."DocumentFolder" ("tenantGuid", "parentDocumentFolderId")
;

-- Index on the DocumentFolder table's tenantGuid,iconId fields.
CREATE INDEX "I_DocumentFolder_tenantGuid_iconId" ON "Scheduler"."DocumentFolder" ("tenantGuid", "iconId")
;

-- Index on the DocumentFolder table's tenantGuid,active fields.
CREATE INDEX "I_DocumentFolder_tenantGuid_active" ON "Scheduler"."DocumentFolder" ("tenantGuid", "active")
;

-- Index on the DocumentFolder table's tenantGuid,deleted fields.
CREATE INDEX "I_DocumentFolder_tenantGuid_deleted" ON "Scheduler"."DocumentFolder" ("tenantGuid", "deleted")
;


-- The change history for records from the DocumentFolder table.
CREATE TABLE "Scheduler"."DocumentFolderChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"documentFolderId" INT NOT NULL,		-- Link to the DocumentFolder table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "documentFolderId" FOREIGN KEY ("documentFolderId") REFERENCES "Scheduler"."DocumentFolder"("id")		-- Foreign key to the DocumentFolder table.
);
-- Index on the DocumentFolderChangeHistory table's tenantGuid field.
CREATE INDEX "I_DocumentFolderChangeHistory_tenantGuid" ON "Scheduler"."DocumentFolderChangeHistory" ("tenantGuid")
;

-- Index on the DocumentFolderChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_DocumentFolderChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."DocumentFolderChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the DocumentFolderChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_DocumentFolderChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."DocumentFolderChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the DocumentFolderChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_DocumentFolderChangeHistory_tenantGuid_userId" ON "Scheduler"."DocumentFolderChangeHistory" ("tenantGuid", "userId")
;

-- Index on the DocumentFolderChangeHistory table's tenantGuid,documentFolderId fields.
CREATE INDEX "I_DocumentFolderChangeHistory_tenantGuid_documentFolderId" ON "Scheduler"."DocumentFolderChangeHistory" ("tenantGuid", "documentFolderId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
 DOCUMENT TAG (Flexible Tagging)
 Lightweight tagging system for documents.  Tags complement DocumentType classification
 and folder organization by allowing multiple user-defined labels on each document
 (e.g., 'urgent', '2026 budget', 'board meeting', 'needs review').

 Tags are tenant-scoped and linked to documents via the DocumentDocumentTag junction table.
 ====================================================================================================
*/
CREATE TABLE "Scheduler"."DocumentTag"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"color" VARCHAR(10) NULL,		-- Tag badge color for UI display.
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_DocumentTag_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the DocumentTag table's tenantGuid and name fields.
);
-- Index on the DocumentTag table's tenantGuid field.
CREATE INDEX "I_DocumentTag_tenantGuid" ON "Scheduler"."DocumentTag" ("tenantGuid")
;

-- Index on the DocumentTag table's tenantGuid,name fields.
CREATE INDEX "I_DocumentTag_tenantGuid_name" ON "Scheduler"."DocumentTag" ("tenantGuid", "name")
;

-- Index on the DocumentTag table's tenantGuid,active fields.
CREATE INDEX "I_DocumentTag_tenantGuid_active" ON "Scheduler"."DocumentTag" ("tenantGuid", "active")
;

-- Index on the DocumentTag table's tenantGuid,deleted fields.
CREATE INDEX "I_DocumentTag_tenantGuid_deleted" ON "Scheduler"."DocumentTag" ("tenantGuid", "deleted")
;


-- The change history for records from the DocumentTag table.
CREATE TABLE "Scheduler"."DocumentTagChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"documentTagId" INT NOT NULL,		-- Link to the DocumentTag table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "documentTagId" FOREIGN KEY ("documentTagId") REFERENCES "Scheduler"."DocumentTag"("id")		-- Foreign key to the DocumentTag table.
);
-- Index on the DocumentTagChangeHistory table's tenantGuid field.
CREATE INDEX "I_DocumentTagChangeHistory_tenantGuid" ON "Scheduler"."DocumentTagChangeHistory" ("tenantGuid")
;

-- Index on the DocumentTagChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_DocumentTagChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."DocumentTagChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the DocumentTagChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_DocumentTagChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."DocumentTagChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the DocumentTagChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_DocumentTagChangeHistory_tenantGuid_userId" ON "Scheduler"."DocumentTagChangeHistory" ("tenantGuid", "userId")
;

-- Index on the DocumentTagChangeHistory table's tenantGuid,documentTagId fields.
CREATE INDEX "I_DocumentTagChangeHistory_tenantGuid_documentTagId" ON "Scheduler"."DocumentTagChangeHistory" ("tenantGuid", "documentTagId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
 DOCUMENT (Attachment Storage)
 Stores file attachments (images, PDFs, scans) with metadata and binary content.
 Uses polymorphic nullable FKs to link to entities across the system.
 Documents can optionally be organized into folders via documentFolderId.

 DESIGN NOTE: Binary content is stored directly in SQL Server (varbinary(max)) via AddBinaryDataFields.
 This is pragmatic for small-to-medium volumes.  The server-side IFileStorageService abstraction
 allows future migration to Azure Blob Storage or similar without changing the API surface.

 The status/statusDate/statusChangedBy fields support document workflows like rental agreement signing.
 ====================================================================================================
*/
CREATE TABLE "Scheduler"."Document"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"documentTypeId" INT NULL,		-- Optional type classification (Rental Agreement, Receipt, Photo, etc.). Nullable because general-purpose files in the file manager may not need a type.
	"documentFolderId" INT NULL,		-- Optional folder placement. NULL = root level / unfiled document.
	"name" VARCHAR(250) NOT NULL,		-- Display name for the document.
	"description" VARCHAR(500) NULL,		-- Optional description of the document.
	"fileName" VARCHAR(500) NOT NULL,		-- Original filename with extension (e.g., 'rental-agreement-smith.pdf').
	"mimeType" VARCHAR(100) NOT NULL,		-- MIME type of the file (e.g., 'application/pdf', 'image/jpeg').
	"fileSizeBytes" BIGINT NOT NULL,		-- File size in bytes for UI display.
	"fileDataFileName" VARCHAR(250) NULL,		-- Part of the binary data field setup
	"fileDataSize" BIGINT NULL,		-- Part of the binary data field setup
	"fileDataData" BYTEA NULL,		-- Part of the binary data field setup
	"fileDataMimeType" VARCHAR(100) NULL,		-- Part of the binary data field setup
	"invoiceId" INT NULL,		-- Optional link to an Invoice (e.g., generated invoice PDF).
	"receiptId" INT NULL,		-- Optional link to a Receipt (e.g., generated receipt PDF).
	"scheduledEventId" INT NULL,		-- Optional link to a ScheduledEvent (e.g., rental agreement for a booking).
	"financialTransactionId" INT NULL,		-- Optional link to a FinancialTransaction (e.g., receipt for a purchase).
	"contactId" INT NULL,		-- Optional link to a Contact.
	"resourceId" INT NULL,		-- Optional link to a Resource.
	"clientId" INT NULL,		-- Optional link to a Client.
	"officeId" INT NULL,		-- Optional link to an Office.
	"crewId" INT NULL,		-- Optional link to a Crew.
	"schedulingTargetId" INT NULL,		-- Optional link to a SchedulingTarget.
	"paymentTransactionId" INT NULL,		-- Optional link to a PaymentTransaction.
	"financialOfficeId" INT NULL,		-- Optional link to a FinancialOffice.
	"tenantProfileId" INT NULL,		-- Optional link to a TenantProfile.
	"campaignId" INT NULL,		-- Optional link to a Campaign.
	"householdId" INT NULL,		-- Optional link to a Household.
	"constituentId" INT NULL,		-- Optional link to a Constituent.
	"tributeId" INT NULL,		-- Optional link to a Tribute.
	"volunteerProfileId" INT NULL,		-- Optional link to a VolunteerProfile.
	"status" VARCHAR(50) NULL,		-- Document workflow status: pending, signed, verified, etc.
	"statusDate" TIMESTAMP NULL,		-- When the status was last changed.
	"statusChangedBy" VARCHAR(100) NULL,		-- Who changed the status.
	"uploadedDate" TIMESTAMP NOT NULL,		-- When the document was uploaded (UTC).
	"uploadedBy" VARCHAR(100) NULL,		-- User who uploaded the document.
	"notes" TEXT NULL,		-- Optional notes about the document.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "documentTypeId" FOREIGN KEY ("documentTypeId") REFERENCES "Scheduler"."DocumentType"("id"),		-- Foreign key to the DocumentType table.
	CONSTRAINT "documentFolderId" FOREIGN KEY ("documentFolderId") REFERENCES "Scheduler"."DocumentFolder"("id"),		-- Foreign key to the DocumentFolder table.
	CONSTRAINT "invoiceId" FOREIGN KEY ("invoiceId") REFERENCES "Scheduler"."Invoice"("id"),		-- Foreign key to the Invoice table.
	CONSTRAINT "receiptId" FOREIGN KEY ("receiptId") REFERENCES "Scheduler"."Receipt"("id"),		-- Foreign key to the Receipt table.
	CONSTRAINT "scheduledEventId" FOREIGN KEY ("scheduledEventId") REFERENCES "Scheduler"."ScheduledEvent"("id"),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT "financialTransactionId" FOREIGN KEY ("financialTransactionId") REFERENCES "Scheduler"."FinancialTransaction"("id"),		-- Foreign key to the FinancialTransaction table.
	CONSTRAINT "contactId" FOREIGN KEY ("contactId") REFERENCES "Scheduler"."Contact"("id"),		-- Foreign key to the Contact table.
	CONSTRAINT "resourceId" FOREIGN KEY ("resourceId") REFERENCES "Scheduler"."Resource"("id"),		-- Foreign key to the Resource table.
	CONSTRAINT "clientId" FOREIGN KEY ("clientId") REFERENCES "Scheduler"."Client"("id"),		-- Foreign key to the Client table.
	CONSTRAINT "officeId" FOREIGN KEY ("officeId") REFERENCES "Scheduler"."Office"("id"),		-- Foreign key to the Office table.
	CONSTRAINT "crewId" FOREIGN KEY ("crewId") REFERENCES "Scheduler"."Crew"("id"),		-- Foreign key to the Crew table.
	CONSTRAINT "schedulingTargetId" FOREIGN KEY ("schedulingTargetId") REFERENCES "Scheduler"."SchedulingTarget"("id"),		-- Foreign key to the SchedulingTarget table.
	CONSTRAINT "paymentTransactionId" FOREIGN KEY ("paymentTransactionId") REFERENCES "Scheduler"."PaymentTransaction"("id"),		-- Foreign key to the PaymentTransaction table.
	CONSTRAINT "financialOfficeId" FOREIGN KEY ("financialOfficeId") REFERENCES "Scheduler"."FinancialOffice"("id"),		-- Foreign key to the FinancialOffice table.
	CONSTRAINT "tenantProfileId" FOREIGN KEY ("tenantProfileId") REFERENCES "Scheduler"."TenantProfile"("id"),		-- Foreign key to the TenantProfile table.
	CONSTRAINT "campaignId" FOREIGN KEY ("campaignId") REFERENCES "Scheduler"."Campaign"("id"),		-- Foreign key to the Campaign table.
	CONSTRAINT "householdId" FOREIGN KEY ("householdId") REFERENCES "Scheduler"."Household"("id"),		-- Foreign key to the Household table.
	CONSTRAINT "constituentId" FOREIGN KEY ("constituentId") REFERENCES "Scheduler"."Constituent"("id"),		-- Foreign key to the Constituent table.
	CONSTRAINT "tributeId" FOREIGN KEY ("tributeId") REFERENCES "Scheduler"."Tribute"("id"),		-- Foreign key to the Tribute table.
	CONSTRAINT "volunteerProfileId" FOREIGN KEY ("volunteerProfileId") REFERENCES "Scheduler"."VolunteerProfile"("id")		-- Foreign key to the VolunteerProfile table.
);
-- Index on the Document table's tenantGuid field.
CREATE INDEX "I_Document_tenantGuid" ON "Scheduler"."Document" ("tenantGuid")
;

-- Index on the Document table's tenantGuid,documentTypeId fields.
CREATE INDEX "I_Document_tenantGuid_documentTypeId" ON "Scheduler"."Document" ("tenantGuid", "documentTypeId")
;

-- Index on the Document table's tenantGuid,documentFolderId fields.
CREATE INDEX "I_Document_tenantGuid_documentFolderId" ON "Scheduler"."Document" ("tenantGuid", "documentFolderId")
;

-- Index on the Document table's tenantGuid,invoiceId fields.
CREATE INDEX "I_Document_tenantGuid_invoiceId" ON "Scheduler"."Document" ("tenantGuid", "invoiceId")
;

-- Index on the Document table's tenantGuid,receiptId fields.
CREATE INDEX "I_Document_tenantGuid_receiptId" ON "Scheduler"."Document" ("tenantGuid", "receiptId")
;

-- Index on the Document table's tenantGuid,scheduledEventId fields.
CREATE INDEX "I_Document_tenantGuid_scheduledEventId" ON "Scheduler"."Document" ("tenantGuid", "scheduledEventId")
;

-- Index on the Document table's tenantGuid,financialTransactionId fields.
CREATE INDEX "I_Document_tenantGuid_financialTransactionId" ON "Scheduler"."Document" ("tenantGuid", "financialTransactionId")
;

-- Index on the Document table's tenantGuid,contactId fields.
CREATE INDEX "I_Document_tenantGuid_contactId" ON "Scheduler"."Document" ("tenantGuid", "contactId")
;

-- Index on the Document table's tenantGuid,resourceId fields.
CREATE INDEX "I_Document_tenantGuid_resourceId" ON "Scheduler"."Document" ("tenantGuid", "resourceId")
;

-- Index on the Document table's tenantGuid,clientId fields.
CREATE INDEX "I_Document_tenantGuid_clientId" ON "Scheduler"."Document" ("tenantGuid", "clientId")
;

-- Index on the Document table's tenantGuid,officeId fields.
CREATE INDEX "I_Document_tenantGuid_officeId" ON "Scheduler"."Document" ("tenantGuid", "officeId")
;

-- Index on the Document table's tenantGuid,crewId fields.
CREATE INDEX "I_Document_tenantGuid_crewId" ON "Scheduler"."Document" ("tenantGuid", "crewId")
;

-- Index on the Document table's tenantGuid,schedulingTargetId fields.
CREATE INDEX "I_Document_tenantGuid_schedulingTargetId" ON "Scheduler"."Document" ("tenantGuid", "schedulingTargetId")
;

-- Index on the Document table's tenantGuid,paymentTransactionId fields.
CREATE INDEX "I_Document_tenantGuid_paymentTransactionId" ON "Scheduler"."Document" ("tenantGuid", "paymentTransactionId")
;

-- Index on the Document table's tenantGuid,financialOfficeId fields.
CREATE INDEX "I_Document_tenantGuid_financialOfficeId" ON "Scheduler"."Document" ("tenantGuid", "financialOfficeId")
;

-- Index on the Document table's tenantGuid,tenantProfileId fields.
CREATE INDEX "I_Document_tenantGuid_tenantProfileId" ON "Scheduler"."Document" ("tenantGuid", "tenantProfileId")
;

-- Index on the Document table's tenantGuid,campaignId fields.
CREATE INDEX "I_Document_tenantGuid_campaignId" ON "Scheduler"."Document" ("tenantGuid", "campaignId")
;

-- Index on the Document table's tenantGuid,householdId fields.
CREATE INDEX "I_Document_tenantGuid_householdId" ON "Scheduler"."Document" ("tenantGuid", "householdId")
;

-- Index on the Document table's tenantGuid,constituentId fields.
CREATE INDEX "I_Document_tenantGuid_constituentId" ON "Scheduler"."Document" ("tenantGuid", "constituentId")
;

-- Index on the Document table's tenantGuid,tributeId fields.
CREATE INDEX "I_Document_tenantGuid_tributeId" ON "Scheduler"."Document" ("tenantGuid", "tributeId")
;

-- Index on the Document table's tenantGuid,volunteerProfileId fields.
CREATE INDEX "I_Document_tenantGuid_volunteerProfileId" ON "Scheduler"."Document" ("tenantGuid", "volunteerProfileId")
;

-- Index on the Document table's tenantGuid,active fields.
CREATE INDEX "I_Document_tenantGuid_active" ON "Scheduler"."Document" ("tenantGuid", "active")
;

-- Index on the Document table's tenantGuid,deleted fields.
CREATE INDEX "I_Document_tenantGuid_deleted" ON "Scheduler"."Document" ("tenantGuid", "deleted")
;


-- The change history for records from the Document table.
CREATE TABLE "Scheduler"."DocumentChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"documentId" INT NOT NULL,		-- Link to the Document table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "documentId" FOREIGN KEY ("documentId") REFERENCES "Scheduler"."Document"("id")		-- Foreign key to the Document table.
);
-- Index on the DocumentChangeHistory table's tenantGuid field.
CREATE INDEX "I_DocumentChangeHistory_tenantGuid" ON "Scheduler"."DocumentChangeHistory" ("tenantGuid")
;

-- Index on the DocumentChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_DocumentChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."DocumentChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the DocumentChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_DocumentChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."DocumentChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the DocumentChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_DocumentChangeHistory_tenantGuid_userId" ON "Scheduler"."DocumentChangeHistory" ("tenantGuid", "userId")
;

-- Index on the DocumentChangeHistory table's tenantGuid,documentId fields.
CREATE INDEX "I_DocumentChangeHistory_tenantGuid_documentId" ON "Scheduler"."DocumentChangeHistory" ("tenantGuid", "documentId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Junction table linking Documents to DocumentTags. Enables many-to-many tagging of documents.
CREATE TABLE "Scheduler"."DocumentDocumentTag"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	"documentId" INT NOT NULL,		-- The document being tagged.
	"documentTagId" INT NOT NULL,		-- The tag applied to the document.
	CONSTRAINT "documentId" FOREIGN KEY ("documentId") REFERENCES "Scheduler"."Document"("id"),		-- Foreign key to the Document table.
	CONSTRAINT "documentTagId" FOREIGN KEY ("documentTagId") REFERENCES "Scheduler"."DocumentTag"("id"),		-- Foreign key to the DocumentTag table.
	CONSTRAINT "UC_DocumentDocumentTag_tenantGuid_documentId_documentTagId" UNIQUE ( "tenantGuid", "documentId", "documentTagId") 		-- Uniqueness enforced on the DocumentDocumentTag table's tenantGuid and documentId and documentTagId fields.
);
-- Index on the DocumentDocumentTag table's tenantGuid field.
CREATE INDEX "I_DocumentDocumentTag_tenantGuid" ON "Scheduler"."DocumentDocumentTag" ("tenantGuid")
;

-- Index on the DocumentDocumentTag table's tenantGuid,active fields.
CREATE INDEX "I_DocumentDocumentTag_tenantGuid_active" ON "Scheduler"."DocumentDocumentTag" ("tenantGuid", "active")
;

-- Index on the DocumentDocumentTag table's tenantGuid,deleted fields.
CREATE INDEX "I_DocumentDocumentTag_tenantGuid_deleted" ON "Scheduler"."DocumentDocumentTag" ("tenantGuid", "deleted")
;

-- Index on the DocumentDocumentTag table's tenantGuid,documentId fields.
CREATE INDEX "I_DocumentDocumentTag_tenantGuid_documentId" ON "Scheduler"."DocumentDocumentTag" ("tenantGuid", "documentId")
;

-- Index on the DocumentDocumentTag table's tenantGuid,documentTagId fields.
CREATE INDEX "I_DocumentDocumentTag_tenantGuid_documentTagId" ON "Scheduler"."DocumentDocumentTag" ("tenantGuid", "documentTagId")
;


-- The change history for records from the DocumentDocumentTag table.
CREATE TABLE "Scheduler"."DocumentDocumentTagChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"documentDocumentTagId" INT NOT NULL,		-- Link to the DocumentDocumentTag table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "documentDocumentTagId" FOREIGN KEY ("documentDocumentTagId") REFERENCES "Scheduler"."DocumentDocumentTag"("id")		-- Foreign key to the DocumentDocumentTag table.
);
-- Index on the DocumentDocumentTagChangeHistory table's tenantGuid field.
CREATE INDEX "I_DocumentDocumentTagChangeHistory_tenantGuid" ON "Scheduler"."DocumentDocumentTagChangeHistory" ("tenantGuid")
;

-- Index on the DocumentDocumentTagChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_DocumentDocumentTagChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."DocumentDocumentTagChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the DocumentDocumentTagChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_DocumentDocumentTagChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."DocumentDocumentTagChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the DocumentDocumentTagChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_DocumentDocumentTagChangeHistory_tenantGuid_userId" ON "Scheduler"."DocumentDocumentTagChangeHistory" ("tenantGuid", "userId")
;

-- Index on the DocumentDocumentTagChangeHistory table's tenantGuid,documentDocumentTagId fields.
CREATE INDEX "I_DocumentDocumentTagChangeHistory_tenantGuid_documentDocumentT" ON "Scheduler"."DocumentDocumentTagChangeHistory" ("tenantGuid", "documentDocumentTagId") INCLUDE ( versionNumber, timeStamp, userId )
;


/*
====================================================================================================
 DOCUMENT SHARE LINK (Public File Sharing)
 Enables sharing documents with external users via GUID-based public URLs.
 Each link has a unique token used in the public download URL (e.g., /share/{token}).
 Supports optional password protection (bcrypt hash), expiry dates, and download limits.

 DESIGN NOTE: The token field is a GUID that serves as the public-facing identifier.
 It is separate from objectGuid (which is the internal entity identifier used by the
 Foundation framework).  A unique index on token ensures fast lookups for public requests.
 ====================================================================================================
*/
CREATE TABLE "Scheduler"."DocumentShareLink"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"documentId" INT NOT NULL,		-- The document this share link provides access to.
	"token" VARCHAR(50) NOT NULL,		-- Public-facing GUID token used in the share URL. Separate from objectGuid.
	"passwordHash" VARCHAR(250) NULL,		-- Optional bcrypt hash of the password required to access the download.
	"expiresAt" TIMESTAMP NULL,		-- Optional expiry date (UTC). NULL = never expires.
	"maxDownloads" INT NULL,		-- Optional download limit. NULL = unlimited downloads.
	"downloadCount" INT NOT NULL DEFAULT 0,		-- Number of times the file has been downloaded via this link.
	"createdBy" VARCHAR(250) NOT NULL,		-- User who created the share link.
	"createdDate" TIMESTAMP NOT NULL,		-- When the share link was created (UTC).
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "documentId" FOREIGN KEY ("documentId") REFERENCES "Scheduler"."Document"("id")		-- Foreign key to the Document table.
);
-- Index on the DocumentShareLink table's tenantGuid field.
CREATE INDEX "I_DocumentShareLink_tenantGuid" ON "Scheduler"."DocumentShareLink" ("tenantGuid")
;

-- Index on the DocumentShareLink table's tenantGuid,documentId fields.
CREATE INDEX "I_DocumentShareLink_tenantGuid_documentId" ON "Scheduler"."DocumentShareLink" ("tenantGuid", "documentId")
;

-- Index on the DocumentShareLink table's tenantGuid,active fields.
CREATE INDEX "I_DocumentShareLink_tenantGuid_active" ON "Scheduler"."DocumentShareLink" ("tenantGuid", "active")
;

-- Index on the DocumentShareLink table's tenantGuid,deleted fields.
CREATE INDEX "I_DocumentShareLink_tenantGuid_deleted" ON "Scheduler"."DocumentShareLink" ("tenantGuid", "deleted")
;

-- Index on the DocumentShareLink table's token field.
CREATE INDEX "I_DocumentShareLink_token" ON "Scheduler"."DocumentShareLink" ("token")
;


-- The change history for records from the DocumentShareLink table.
CREATE TABLE "Scheduler"."DocumentShareLinkChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"documentShareLinkId" INT NOT NULL,		-- Link to the DocumentShareLink table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "documentShareLinkId" FOREIGN KEY ("documentShareLinkId") REFERENCES "Scheduler"."DocumentShareLink"("id")		-- Foreign key to the DocumentShareLink table.
);
-- Index on the DocumentShareLinkChangeHistory table's tenantGuid field.
CREATE INDEX "I_DocumentShareLinkChangeHistory_tenantGuid" ON "Scheduler"."DocumentShareLinkChangeHistory" ("tenantGuid")
;

-- Index on the DocumentShareLinkChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_DocumentShareLinkChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."DocumentShareLinkChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the DocumentShareLinkChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_DocumentShareLinkChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."DocumentShareLinkChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the DocumentShareLinkChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_DocumentShareLinkChangeHistory_tenantGuid_userId" ON "Scheduler"."DocumentShareLinkChangeHistory" ("tenantGuid", "userId")
;

-- Index on the DocumentShareLinkChangeHistory table's tenantGuid,documentShareLinkId fields.
CREATE INDEX "I_DocumentShareLinkChangeHistory_tenantGuid_documentShareLinkId" ON "Scheduler"."DocumentShareLinkChangeHistory" ("tenantGuid", "documentShareLinkId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Links resources, crews, or volunteer groups o events.  Supports partial assignments and role designation.  - If crewId is non-NULL → this row represents assignment of the whole crew - If resourceId is non-NULL and crewId is NULL → individual resource assignment - assignmentStart/End NULL → uses full event duration.  only one of crewId, volunteerGroupId, resourceId should be populated per row (business rule in app layer).
CREATE TABLE "Scheduler"."EventResourceAssignment"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduledEventId" INT NOT NULL,		-- Link to the ScheduledEvent table.
	"officeId" INT NULL,		-- Snapshot of office resource assigned to this event belongs to at the time of assignment.  This should never change, and should NOT be updated if a resource moves to a different office post-event assignment.
	"resourceId" INT NULL,		-- Required for individual assignments; should be NULL when crewId is used
	"crewId" INT NULL,		-- Optional – when set, assigns the entire crew as a unit
	"volunteerGroupId" INT NULL,		-- Optional: assign an entire VolunteerGroup instead of/in addition to individual resources or Crew
	"assignmentRoleId" INT NULL,		-- Optional role for this assignment (individual or crew member default)
	"assignmentStatusId" INT NOT NULL DEFAULT 1,		-- NULL = Planned, non-NULL links to AssignmentStatus master table
	"assignmentStartDateTime" TIMESTAMP NULL,		-- NULL = starts at event start
	"assignmentEndDateTime" TIMESTAMP NULL,		-- NULL = ends at event end
	"notes" TEXT NULL,
	"isTravelRequired" BOOLEAN NULL,		-- Whether or not travel is required for the assignment
	"travelDurationMinutes" INT NULL DEFAULT 0,		-- Time required to get to the site
	"distanceKilometers" REAL NULL DEFAULT 0,		-- Useful for expense calculation
	"startLocation" VARCHAR(100) NULL,
	"actualStartDateTime" TIMESTAMP NULL,
	"actualEndDateTime" TIMESTAMP NULL,
	"actualNotes" TEXT NULL,
	"isVolunteer" BOOLEAN NOT NULL DEFAULT false,/*
True = this is a volunteer (unpaid) assignment.
Used to:
- Exclude from payroll/wage calculations
- Include in volunteer hours totals
- Apply different approval/reminder workflows
- Filter volunteer-specific reports
*/
	"reportedVolunteerHours" REAL NULL,		-- Hours the volunteer self-reported (or coordinator entered) for this assignment
	"approvedVolunteerHours" REAL NULL,		-- Approved/confirmed hours (may differ from reported if adjustments needed)
	"hoursApprovedByContactId" INT NULL,		-- Contact (usually staff/coordinator) who approved the hours
	"approvedDateTime" TIMESTAMP NULL,		-- When the hours were approved
	"reimbursementAmount" DECIMAL(11,2) NULL,		-- Optional: mileage, parking, meals, etc. — not a wage
	"chargeTypeId" INT NULL,		-- Optional: links to an expense-type ChargeType for the reimbursement (e.g. 'Mileage Reimbursement')
	"reimbursementRequested" BOOLEAN NOT NULL DEFAULT false,		-- Volunteer has flagged that they want/need reimbursement
	"volunteerNotes" TEXT NULL,		-- Volunteer-specific notes for this assignment (e.g. 'Prefers morning shifts next time', 'Brought own tools')
	"reminderSentDateTime" TIMESTAMP NULL,		-- When the last automated reminder was sent for this assignment; NULL = no reminder sent yet
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "scheduledEventId" FOREIGN KEY ("scheduledEventId") REFERENCES "Scheduler"."ScheduledEvent"("id"),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT "officeId" FOREIGN KEY ("officeId") REFERENCES "Scheduler"."Office"("id"),		-- Foreign key to the Office table.
	CONSTRAINT "resourceId" FOREIGN KEY ("resourceId") REFERENCES "Scheduler"."Resource"("id"),		-- Foreign key to the Resource table.
	CONSTRAINT "crewId" FOREIGN KEY ("crewId") REFERENCES "Scheduler"."Crew"("id"),		-- Foreign key to the Crew table.
	CONSTRAINT "volunteerGroupId" FOREIGN KEY ("volunteerGroupId") REFERENCES "Scheduler"."VolunteerGroup"("id"),		-- Foreign key to the VolunteerGroup table.
	CONSTRAINT "assignmentRoleId" FOREIGN KEY ("assignmentRoleId") REFERENCES "Scheduler"."AssignmentRole"("id"),		-- Foreign key to the AssignmentRole table.
	CONSTRAINT "assignmentStatusId" FOREIGN KEY ("assignmentStatusId") REFERENCES "Scheduler"."AssignmentStatus"("id"),		-- Foreign key to the AssignmentStatus table.
	CONSTRAINT "hoursApprovedByContactId" FOREIGN KEY ("hoursApprovedByContactId") REFERENCES "Scheduler"."Contact"("id"),		-- Foreign key to the Contact table.
	CONSTRAINT "chargeTypeId" FOREIGN KEY ("chargeTypeId") REFERENCES "Scheduler"."ChargeType"("id")		-- Foreign key to the ChargeType table.
);
-- Index on the EventResourceAssignment table's tenantGuid field.
CREATE INDEX "I_EventResourceAssignment_tenantGuid" ON "Scheduler"."EventResourceAssignment" ("tenantGuid")
;

-- Index on the EventResourceAssignment table's tenantGuid,scheduledEventId fields.
CREATE INDEX "I_EventResourceAssignment_tenantGuid_scheduledEventId" ON "Scheduler"."EventResourceAssignment" ("tenantGuid", "scheduledEventId")
;

-- Index on the EventResourceAssignment table's tenantGuid,officeId fields.
CREATE INDEX "I_EventResourceAssignment_tenantGuid_officeId" ON "Scheduler"."EventResourceAssignment" ("tenantGuid", "officeId")
;

-- Index on the EventResourceAssignment table's tenantGuid,resourceId fields.
CREATE INDEX "I_EventResourceAssignment_tenantGuid_resourceId" ON "Scheduler"."EventResourceAssignment" ("tenantGuid", "resourceId")
;

-- Index on the EventResourceAssignment table's tenantGuid,crewId fields.
CREATE INDEX "I_EventResourceAssignment_tenantGuid_crewId" ON "Scheduler"."EventResourceAssignment" ("tenantGuid", "crewId")
;

-- Index on the EventResourceAssignment table's tenantGuid,volunteerGroupId fields.
CREATE INDEX "I_EventResourceAssignment_tenantGuid_volunteerGroupId" ON "Scheduler"."EventResourceAssignment" ("tenantGuid", "volunteerGroupId")
;

-- Index on the EventResourceAssignment table's tenantGuid,assignmentRoleId fields.
CREATE INDEX "I_EventResourceAssignment_tenantGuid_assignmentRoleId" ON "Scheduler"."EventResourceAssignment" ("tenantGuid", "assignmentRoleId")
;

-- Index on the EventResourceAssignment table's tenantGuid,assignmentStatusId fields.
CREATE INDEX "I_EventResourceAssignment_tenantGuid_assignmentStatusId" ON "Scheduler"."EventResourceAssignment" ("tenantGuid", "assignmentStatusId")
;

-- Index on the EventResourceAssignment table's tenantGuid,hoursApprovedByContactId fields.
CREATE INDEX "I_EventResourceAssignment_tenantGuid_hoursApprovedByContactId" ON "Scheduler"."EventResourceAssignment" ("tenantGuid", "hoursApprovedByContactId")
;

-- Index on the EventResourceAssignment table's tenantGuid,chargeTypeId fields.
CREATE INDEX "I_EventResourceAssignment_tenantGuid_chargeTypeId" ON "Scheduler"."EventResourceAssignment" ("tenantGuid", "chargeTypeId")
;

-- Index on the EventResourceAssignment table's tenantGuid,active fields.
CREATE INDEX "I_EventResourceAssignment_tenantGuid_active" ON "Scheduler"."EventResourceAssignment" ("tenantGuid", "active")
;

-- Index on the EventResourceAssignment table's tenantGuid,deleted fields.
CREATE INDEX "I_EventResourceAssignment_tenantGuid_deleted" ON "Scheduler"."EventResourceAssignment" ("tenantGuid", "deleted")
;

-- Index on the EventResourceAssignment table's tenantGuid,resourceId,assignmentStartDateTime,assignmentEndDateTime fields.
CREATE INDEX "I_EventResourceAssignment_tenantGuid_resourceId_assignmentStart" ON "Scheduler"."EventResourceAssignment" ("tenantGuid", "resourceId", "assignmentStartDateTime", "assignmentEndDateTime")
;

-- Index on the EventResourceAssignment table's tenantGuid,crewId,assignmentStartDateTime,assignmentEndDateTime fields.
CREATE INDEX "I_EventResourceAssignment_tenantGuid_crewId_assignmentStartDate" ON "Scheduler"."EventResourceAssignment" ("tenantGuid", "crewId", "assignmentStartDateTime", "assignmentEndDateTime")
;


-- The change history for records from the EventResourceAssignment table.
CREATE TABLE "Scheduler"."EventResourceAssignmentChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"eventResourceAssignmentId" INT NOT NULL,		-- Link to the EventResourceAssignment table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "eventResourceAssignmentId" FOREIGN KEY ("eventResourceAssignmentId") REFERENCES "Scheduler"."EventResourceAssignment"("id")		-- Foreign key to the EventResourceAssignment table.
);
-- Index on the EventResourceAssignmentChangeHistory table's tenantGuid field.
CREATE INDEX "I_EventResourceAssignmentChangeHistory_tenantGuid" ON "Scheduler"."EventResourceAssignmentChangeHistory" ("tenantGuid")
;

-- Index on the EventResourceAssignmentChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_EventResourceAssignmentChangeHistory_tenantGuid_versionNumber" ON "Scheduler"."EventResourceAssignmentChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the EventResourceAssignmentChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_EventResourceAssignmentChangeHistory_tenantGuid_timeStamp" ON "Scheduler"."EventResourceAssignmentChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the EventResourceAssignmentChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_EventResourceAssignmentChangeHistory_tenantGuid_userId" ON "Scheduler"."EventResourceAssignmentChangeHistory" ("tenantGuid", "userId")
;

-- Index on the EventResourceAssignmentChangeHistory table's tenantGuid,eventResourceAssignmentId fields.
CREATE INDEX "I_EventResourceAssignmentChangeHistory_tenantGuid_eventResource" ON "Scheduler"."EventResourceAssignmentChangeHistory" ("tenantGuid", "eventResourceAssignmentId") INCLUDE ( versionNumber, timeStamp, userId )
;


