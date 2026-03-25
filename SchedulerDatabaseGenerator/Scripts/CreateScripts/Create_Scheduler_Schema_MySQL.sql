/*
Scheduler scheduling system database schema.
This is a multi-tenant resource scheduling system designed primarily for construction resource planning
but flexible enough for other use cases. It supports events, individual and crew-based resource assignments,
partial time assignments, role designation, availability blackouts, and calendar grouping.
All operational tables include multi-tenant support, versioning where appropriate, auditing, and security controls.
*/
CREATE DATABASE `Scheduler`;

USE `Scheduler`;

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE `EventResourceAssignmentChangeHistory`
-- DROP TABLE `EventResourceAssignment`
-- DROP TABLE `DocumentShareLinkChangeHistory`
-- DROP TABLE `DocumentShareLink`
-- DROP TABLE `DocumentDocumentTagChangeHistory`
-- DROP TABLE `DocumentDocumentTag`
-- DROP TABLE `DocumentChangeHistory`
-- DROP TABLE `Document`
-- DROP TABLE `DocumentTagChangeHistory`
-- DROP TABLE `DocumentTag`
-- DROP TABLE `DocumentFolderChangeHistory`
-- DROP TABLE `DocumentFolder`
-- DROP TABLE `DocumentType`
-- DROP TABLE `VolunteerGroupMemberChangeHistory`
-- DROP TABLE `VolunteerGroupMember`
-- DROP TABLE `VolunteerGroupChangeHistory`
-- DROP TABLE `VolunteerGroup`
-- DROP TABLE `VolunteerProfileChangeHistory`
-- DROP TABLE `VolunteerProfile`
-- DROP TABLE `SoftCreditChangeHistory`
-- DROP TABLE `SoftCredit`
-- DROP TABLE `GiftChangeHistory`
-- DROP TABLE `Gift`
-- DROP TABLE `TributeChangeHistory`
-- DROP TABLE `Tribute`
-- DROP TABLE `BatchChangeHistory`
-- DROP TABLE `Batch`
-- DROP TABLE `BatchStatus`
-- DROP TABLE `TributeType`
-- DROP TABLE `PledgeChangeHistory`
-- DROP TABLE `Pledge`
-- DROP TABLE `ConstituentChangeHistory`
-- DROP TABLE `Constituent`
-- DROP TABLE `ConstituentJourneyStageChangeHistory`
-- DROP TABLE `ConstituentJourneyStage`
-- DROP TABLE `HouseholdChangeHistory`
-- DROP TABLE `Household`
-- DROP TABLE `AppealChangeHistory`
-- DROP TABLE `Appeal`
-- DROP TABLE `CampaignChangeHistory`
-- DROP TABLE `Campaign`
-- DROP TABLE `FundChangeHistory`
-- DROP TABLE `Fund`
-- DROP TABLE `EventNotificationSubscriptionChangeHistory`
-- DROP TABLE `EventNotificationSubscription`
-- DROP TABLE `EventNotificationType`
-- DROP TABLE `RecurrenceExceptionChangeHistory`
-- DROP TABLE `RecurrenceException`
-- DROP TABLE `ScheduledEventQualificationRequirementChangeHistory`
-- DROP TABLE `ScheduledEventQualificationRequirement`
-- DROP TABLE `ScheduledEventDependencyChangeHistory`
-- DROP TABLE `ScheduledEventDependency`
-- DROP TABLE `DependencyType`
-- DROP TABLE `EventCalendar`
-- DROP TABLE `ContactInteractionChangeHistory`
-- DROP TABLE `ContactInteraction`
-- DROP TABLE `ReceiptChangeHistory`
-- DROP TABLE `Receipt`
-- DROP TABLE `InvoiceLineItem`
-- DROP TABLE `InvoiceChangeHistory`
-- DROP TABLE `Invoice`
-- DROP TABLE `InvoiceStatus`
-- DROP TABLE `PaymentTransactionChangeHistory`
-- DROP TABLE `PaymentTransaction`
-- DROP TABLE `PaymentProviderChangeHistory`
-- DROP TABLE `PaymentProvider`
-- DROP TABLE `PaymentMethod`
-- DROP TABLE `GeneralLedgerLine`
-- DROP TABLE `GeneralLedgerEntry`
-- DROP TABLE `BudgetChangeHistory`
-- DROP TABLE `Budget`
-- DROP TABLE `FinancialTransactionChangeHistory`
-- DROP TABLE `FinancialTransaction`
-- DROP TABLE `FiscalPeriodChangeHistory`
-- DROP TABLE `FiscalPeriod`
-- DROP TABLE `PeriodStatus`
-- DROP TABLE `EventChargeChangeHistory`
-- DROP TABLE `EventCharge`
-- DROP TABLE `ChargeStatusChangeHistory`
-- DROP TABLE `ChargeStatus`
-- DROP TABLE `ScheduledEventChangeHistory`
-- DROP TABLE `ScheduledEvent`
-- DROP TABLE `EventTypeChangeHistory`
-- DROP TABLE `EventType`
-- DROP TABLE `ScheduledEventTemplateQualificationRequirementChangeHistory`
-- DROP TABLE `ScheduledEventTemplateQualificationRequirement`
-- DROP TABLE `ScheduledEventTemplateChargeChangeHistory`
-- DROP TABLE `ScheduledEventTemplateCharge`
-- DROP TABLE `ScheduledEventTemplateChangeHistory`
-- DROP TABLE `ScheduledEventTemplate`
-- DROP TABLE `CrewMemberChangeHistory`
-- DROP TABLE `CrewMember`
-- DROP TABLE `CrewChangeHistory`
-- DROP TABLE `Crew`
-- DROP TABLE `ResourceShiftChangeHistory`
-- DROP TABLE `ResourceShift`
-- DROP TABLE `ResourceAvailabilityChangeHistory`
-- DROP TABLE `ResourceAvailability`
-- DROP TABLE `ResourceQualificationChangeHistory`
-- DROP TABLE `ResourceQualification`
-- DROP TABLE `RateSheetChangeHistory`
-- DROP TABLE `RateSheet`
-- DROP TABLE `ResourceContactChangeHistory`
-- DROP TABLE `ResourceContact`
-- DROP TABLE `ResourceChangeHistory`
-- DROP TABLE `Resource`
-- DROP TABLE `ShiftPatternDayChangeHistory`
-- DROP TABLE `ShiftPatternDay`
-- DROP TABLE `ShiftPatternChangeHistory`
-- DROP TABLE `ShiftPattern`
-- DROP TABLE `RecurrenceRuleChangeHistory`
-- DROP TABLE `RecurrenceRule`
-- DROP TABLE `RecurrenceFrequency`
-- DROP TABLE `SchedulingTargetQualificationRequirementChangeHistory`
-- DROP TABLE `SchedulingTargetQualificationRequirement`
-- DROP TABLE `SchedulingTargetAddressChangeHistory`
-- DROP TABLE `SchedulingTargetAddress`
-- DROP TABLE `SchedulingTargetContactChangeHistory`
-- DROP TABLE `SchedulingTargetContact`
-- DROP TABLE `SchedulingTargetChangeHistory`
-- DROP TABLE `SchedulingTarget`
-- DROP TABLE `SchedulingTargetType`
-- DROP TABLE `AssignmentStatus`
-- DROP TABLE `BookingSourceType`
-- DROP TABLE `ReceiptTypeChangeHistory`
-- DROP TABLE `ReceiptType`
-- DROP TABLE `PaymentTypeChangeHistory`
-- DROP TABLE `PaymentType`
-- DROP TABLE `EventStatus`
-- DROP TABLE `AssignmentRoleQualificationRequirementChangeHistory`
-- DROP TABLE `AssignmentRoleQualificationRequirement`
-- DROP TABLE `AssignmentRole`
-- DROP TABLE `Qualification`
-- DROP TABLE `TenantProfileChangeHistory`
-- DROP TABLE `TenantProfile`
-- DROP TABLE `ClientContactChangeHistory`
-- DROP TABLE `ClientContact`
-- DROP TABLE `ClientChangeHistory`
-- DROP TABLE `Client`
-- DROP TABLE `ClientType`
-- DROP TABLE `CalendarChangeHistory`
-- DROP TABLE `Calendar`
-- DROP TABLE `OfficeContactChangeHistory`
-- DROP TABLE `OfficeContact`
-- DROP TABLE `OfficeChangeHistory`
-- DROP TABLE `Office`
-- DROP TABLE `OfficeType`
-- DROP TABLE `ContactContactChangeHistory`
-- DROP TABLE `ContactContact`
-- DROP TABLE `RelationshipType`
-- DROP TABLE `ContactTagChangeHistory`
-- DROP TABLE `ContactTag`
-- DROP TABLE `ContactChangeHistory`
-- DROP TABLE `Contact`
-- DROP TABLE `ContactType`
-- DROP TABLE `VolunteerStatus`
-- DROP TABLE `StateProvince`
-- DROP TABLE `Country`
-- DROP TABLE `TimeZone`
-- DROP TABLE `Tag`
-- DROP TABLE `ChargeTypeChangeHistory`
-- DROP TABLE `ChargeType`
-- DROP TABLE `TaxCode`
-- DROP TABLE `FinancialCategoryChangeHistory`
-- DROP TABLE `FinancialCategory`
-- DROP TABLE `FinancialOfficeChangeHistory`
-- DROP TABLE `FinancialOffice`
-- DROP TABLE `AccountType`
-- DROP TABLE `Currency`
-- DROP TABLE `InteractionType`
-- DROP TABLE `RateType`
-- DROP TABLE `ContactMethod`
-- DROP TABLE `Priority`
-- DROP TABLE `ResourceType`
-- DROP TABLE `Salutation`
-- DROP TABLE `Icon`
-- DROP TABLE `AttributeDefinitionChangeHistory`
-- DROP TABLE `AttributeDefinition`
-- DROP TABLE `AttributeDefinitionEntity`
-- DROP TABLE `AttributeDefinitionType`
-- DROP TABLE `CallEventLog`
-- DROP TABLE `CallParticipant`
-- DROP TABLE `Call`
-- DROP TABLE `CallStatus`
-- DROP TABLE `CallType`
-- DROP TABLE `MessagingAuditLog`
-- DROP TABLE `MessageFlag`
-- DROP TABLE `PushProviderConfiguration`
-- DROP TABLE `PushDeliveryLog`
-- DROP TABLE `MessageBookmark`
-- DROP TABLE `ConversationThreadUser`
-- DROP TABLE `UserPresence`
-- DROP TABLE `ConversationMessageLinkPreviewChangeHistory`
-- DROP TABLE `ConversationMessageLinkPreview`
-- DROP TABLE `ConversationPin`
-- DROP TABLE `ConversationMessageReaction`
-- DROP TABLE `ConversationMessageUser`
-- DROP TABLE `ConversationMessageAttachmentChangeHistory`
-- DROP TABLE `ConversationMessageAttachment`
-- DROP TABLE `ConversationMessageChangeHistory`
-- DROP TABLE `ConversationMessage`
-- DROP TABLE `ConversationChannelChangeHistory`
-- DROP TABLE `ConversationChannel`
-- DROP TABLE `ConversationUser`
-- DROP TABLE `Conversation`
-- DROP TABLE `ConversationType`
-- DROP TABLE `NotificationDistribution`
-- DROP TABLE `NotificationAttachmentChangeHistory`
-- DROP TABLE `NotificationAttachment`
-- DROP TABLE `NotificationChangeHistory`
-- DROP TABLE `Notification`
-- DROP TABLE `NotificationType`

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON `EventResourceAssignmentChangeHistory` DISABLE
-- ALTER INDEX ALL ON `EventResourceAssignment` DISABLE
-- ALTER INDEX ALL ON `DocumentShareLinkChangeHistory` DISABLE
-- ALTER INDEX ALL ON `DocumentShareLink` DISABLE
-- ALTER INDEX ALL ON `DocumentDocumentTagChangeHistory` DISABLE
-- ALTER INDEX ALL ON `DocumentDocumentTag` DISABLE
-- ALTER INDEX ALL ON `DocumentChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Document` DISABLE
-- ALTER INDEX ALL ON `DocumentTagChangeHistory` DISABLE
-- ALTER INDEX ALL ON `DocumentTag` DISABLE
-- ALTER INDEX ALL ON `DocumentFolderChangeHistory` DISABLE
-- ALTER INDEX ALL ON `DocumentFolder` DISABLE
-- ALTER INDEX ALL ON `DocumentType` DISABLE
-- ALTER INDEX ALL ON `VolunteerGroupMemberChangeHistory` DISABLE
-- ALTER INDEX ALL ON `VolunteerGroupMember` DISABLE
-- ALTER INDEX ALL ON `VolunteerGroupChangeHistory` DISABLE
-- ALTER INDEX ALL ON `VolunteerGroup` DISABLE
-- ALTER INDEX ALL ON `VolunteerProfileChangeHistory` DISABLE
-- ALTER INDEX ALL ON `VolunteerProfile` DISABLE
-- ALTER INDEX ALL ON `SoftCreditChangeHistory` DISABLE
-- ALTER INDEX ALL ON `SoftCredit` DISABLE
-- ALTER INDEX ALL ON `GiftChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Gift` DISABLE
-- ALTER INDEX ALL ON `TributeChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Tribute` DISABLE
-- ALTER INDEX ALL ON `BatchChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Batch` DISABLE
-- ALTER INDEX ALL ON `BatchStatus` DISABLE
-- ALTER INDEX ALL ON `TributeType` DISABLE
-- ALTER INDEX ALL ON `PledgeChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Pledge` DISABLE
-- ALTER INDEX ALL ON `ConstituentChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Constituent` DISABLE
-- ALTER INDEX ALL ON `ConstituentJourneyStageChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ConstituentJourneyStage` DISABLE
-- ALTER INDEX ALL ON `HouseholdChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Household` DISABLE
-- ALTER INDEX ALL ON `AppealChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Appeal` DISABLE
-- ALTER INDEX ALL ON `CampaignChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Campaign` DISABLE
-- ALTER INDEX ALL ON `FundChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Fund` DISABLE
-- ALTER INDEX ALL ON `EventNotificationSubscriptionChangeHistory` DISABLE
-- ALTER INDEX ALL ON `EventNotificationSubscription` DISABLE
-- ALTER INDEX ALL ON `EventNotificationType` DISABLE
-- ALTER INDEX ALL ON `RecurrenceExceptionChangeHistory` DISABLE
-- ALTER INDEX ALL ON `RecurrenceException` DISABLE
-- ALTER INDEX ALL ON `ScheduledEventQualificationRequirementChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ScheduledEventQualificationRequirement` DISABLE
-- ALTER INDEX ALL ON `ScheduledEventDependencyChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ScheduledEventDependency` DISABLE
-- ALTER INDEX ALL ON `DependencyType` DISABLE
-- ALTER INDEX ALL ON `EventCalendar` DISABLE
-- ALTER INDEX ALL ON `ContactInteractionChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ContactInteraction` DISABLE
-- ALTER INDEX ALL ON `ReceiptChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Receipt` DISABLE
-- ALTER INDEX ALL ON `InvoiceLineItem` DISABLE
-- ALTER INDEX ALL ON `InvoiceChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Invoice` DISABLE
-- ALTER INDEX ALL ON `InvoiceStatus` DISABLE
-- ALTER INDEX ALL ON `PaymentTransactionChangeHistory` DISABLE
-- ALTER INDEX ALL ON `PaymentTransaction` DISABLE
-- ALTER INDEX ALL ON `PaymentProviderChangeHistory` DISABLE
-- ALTER INDEX ALL ON `PaymentProvider` DISABLE
-- ALTER INDEX ALL ON `PaymentMethod` DISABLE
-- ALTER INDEX ALL ON `GeneralLedgerLine` DISABLE
-- ALTER INDEX ALL ON `GeneralLedgerEntry` DISABLE
-- ALTER INDEX ALL ON `BudgetChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Budget` DISABLE
-- ALTER INDEX ALL ON `FinancialTransactionChangeHistory` DISABLE
-- ALTER INDEX ALL ON `FinancialTransaction` DISABLE
-- ALTER INDEX ALL ON `FiscalPeriodChangeHistory` DISABLE
-- ALTER INDEX ALL ON `FiscalPeriod` DISABLE
-- ALTER INDEX ALL ON `PeriodStatus` DISABLE
-- ALTER INDEX ALL ON `EventChargeChangeHistory` DISABLE
-- ALTER INDEX ALL ON `EventCharge` DISABLE
-- ALTER INDEX ALL ON `ChargeStatusChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ChargeStatus` DISABLE
-- ALTER INDEX ALL ON `ScheduledEventChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ScheduledEvent` DISABLE
-- ALTER INDEX ALL ON `EventTypeChangeHistory` DISABLE
-- ALTER INDEX ALL ON `EventType` DISABLE
-- ALTER INDEX ALL ON `ScheduledEventTemplateQualificationRequirementChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ScheduledEventTemplateQualificationRequirement` DISABLE
-- ALTER INDEX ALL ON `ScheduledEventTemplateChargeChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ScheduledEventTemplateCharge` DISABLE
-- ALTER INDEX ALL ON `ScheduledEventTemplateChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ScheduledEventTemplate` DISABLE
-- ALTER INDEX ALL ON `CrewMemberChangeHistory` DISABLE
-- ALTER INDEX ALL ON `CrewMember` DISABLE
-- ALTER INDEX ALL ON `CrewChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Crew` DISABLE
-- ALTER INDEX ALL ON `ResourceShiftChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ResourceShift` DISABLE
-- ALTER INDEX ALL ON `ResourceAvailabilityChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ResourceAvailability` DISABLE
-- ALTER INDEX ALL ON `ResourceQualificationChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ResourceQualification` DISABLE
-- ALTER INDEX ALL ON `RateSheetChangeHistory` DISABLE
-- ALTER INDEX ALL ON `RateSheet` DISABLE
-- ALTER INDEX ALL ON `ResourceContactChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ResourceContact` DISABLE
-- ALTER INDEX ALL ON `ResourceChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Resource` DISABLE
-- ALTER INDEX ALL ON `ShiftPatternDayChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ShiftPatternDay` DISABLE
-- ALTER INDEX ALL ON `ShiftPatternChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ShiftPattern` DISABLE
-- ALTER INDEX ALL ON `RecurrenceRuleChangeHistory` DISABLE
-- ALTER INDEX ALL ON `RecurrenceRule` DISABLE
-- ALTER INDEX ALL ON `RecurrenceFrequency` DISABLE
-- ALTER INDEX ALL ON `SchedulingTargetQualificationRequirementChangeHistory` DISABLE
-- ALTER INDEX ALL ON `SchedulingTargetQualificationRequirement` DISABLE
-- ALTER INDEX ALL ON `SchedulingTargetAddressChangeHistory` DISABLE
-- ALTER INDEX ALL ON `SchedulingTargetAddress` DISABLE
-- ALTER INDEX ALL ON `SchedulingTargetContactChangeHistory` DISABLE
-- ALTER INDEX ALL ON `SchedulingTargetContact` DISABLE
-- ALTER INDEX ALL ON `SchedulingTargetChangeHistory` DISABLE
-- ALTER INDEX ALL ON `SchedulingTarget` DISABLE
-- ALTER INDEX ALL ON `SchedulingTargetType` DISABLE
-- ALTER INDEX ALL ON `AssignmentStatus` DISABLE
-- ALTER INDEX ALL ON `BookingSourceType` DISABLE
-- ALTER INDEX ALL ON `ReceiptTypeChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ReceiptType` DISABLE
-- ALTER INDEX ALL ON `PaymentTypeChangeHistory` DISABLE
-- ALTER INDEX ALL ON `PaymentType` DISABLE
-- ALTER INDEX ALL ON `EventStatus` DISABLE
-- ALTER INDEX ALL ON `AssignmentRoleQualificationRequirementChangeHistory` DISABLE
-- ALTER INDEX ALL ON `AssignmentRoleQualificationRequirement` DISABLE
-- ALTER INDEX ALL ON `AssignmentRole` DISABLE
-- ALTER INDEX ALL ON `Qualification` DISABLE
-- ALTER INDEX ALL ON `TenantProfileChangeHistory` DISABLE
-- ALTER INDEX ALL ON `TenantProfile` DISABLE
-- ALTER INDEX ALL ON `ClientContactChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ClientContact` DISABLE
-- ALTER INDEX ALL ON `ClientChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Client` DISABLE
-- ALTER INDEX ALL ON `ClientType` DISABLE
-- ALTER INDEX ALL ON `CalendarChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Calendar` DISABLE
-- ALTER INDEX ALL ON `OfficeContactChangeHistory` DISABLE
-- ALTER INDEX ALL ON `OfficeContact` DISABLE
-- ALTER INDEX ALL ON `OfficeChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Office` DISABLE
-- ALTER INDEX ALL ON `OfficeType` DISABLE
-- ALTER INDEX ALL ON `ContactContactChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ContactContact` DISABLE
-- ALTER INDEX ALL ON `RelationshipType` DISABLE
-- ALTER INDEX ALL ON `ContactTagChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ContactTag` DISABLE
-- ALTER INDEX ALL ON `ContactChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Contact` DISABLE
-- ALTER INDEX ALL ON `ContactType` DISABLE
-- ALTER INDEX ALL ON `VolunteerStatus` DISABLE
-- ALTER INDEX ALL ON `StateProvince` DISABLE
-- ALTER INDEX ALL ON `Country` DISABLE
-- ALTER INDEX ALL ON `TimeZone` DISABLE
-- ALTER INDEX ALL ON `Tag` DISABLE
-- ALTER INDEX ALL ON `ChargeTypeChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ChargeType` DISABLE
-- ALTER INDEX ALL ON `TaxCode` DISABLE
-- ALTER INDEX ALL ON `FinancialCategoryChangeHistory` DISABLE
-- ALTER INDEX ALL ON `FinancialCategory` DISABLE
-- ALTER INDEX ALL ON `FinancialOfficeChangeHistory` DISABLE
-- ALTER INDEX ALL ON `FinancialOffice` DISABLE
-- ALTER INDEX ALL ON `AccountType` DISABLE
-- ALTER INDEX ALL ON `Currency` DISABLE
-- ALTER INDEX ALL ON `InteractionType` DISABLE
-- ALTER INDEX ALL ON `RateType` DISABLE
-- ALTER INDEX ALL ON `ContactMethod` DISABLE
-- ALTER INDEX ALL ON `Priority` DISABLE
-- ALTER INDEX ALL ON `ResourceType` DISABLE
-- ALTER INDEX ALL ON `Salutation` DISABLE
-- ALTER INDEX ALL ON `Icon` DISABLE
-- ALTER INDEX ALL ON `AttributeDefinitionChangeHistory` DISABLE
-- ALTER INDEX ALL ON `AttributeDefinition` DISABLE
-- ALTER INDEX ALL ON `AttributeDefinitionEntity` DISABLE
-- ALTER INDEX ALL ON `AttributeDefinitionType` DISABLE
-- ALTER INDEX ALL ON `CallEventLog` DISABLE
-- ALTER INDEX ALL ON `CallParticipant` DISABLE
-- ALTER INDEX ALL ON `Call` DISABLE
-- ALTER INDEX ALL ON `CallStatus` DISABLE
-- ALTER INDEX ALL ON `CallType` DISABLE
-- ALTER INDEX ALL ON `MessagingAuditLog` DISABLE
-- ALTER INDEX ALL ON `MessageFlag` DISABLE
-- ALTER INDEX ALL ON `PushProviderConfiguration` DISABLE
-- ALTER INDEX ALL ON `PushDeliveryLog` DISABLE
-- ALTER INDEX ALL ON `MessageBookmark` DISABLE
-- ALTER INDEX ALL ON `ConversationThreadUser` DISABLE
-- ALTER INDEX ALL ON `UserPresence` DISABLE
-- ALTER INDEX ALL ON `ConversationMessageLinkPreviewChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ConversationMessageLinkPreview` DISABLE
-- ALTER INDEX ALL ON `ConversationPin` DISABLE
-- ALTER INDEX ALL ON `ConversationMessageReaction` DISABLE
-- ALTER INDEX ALL ON `ConversationMessageUser` DISABLE
-- ALTER INDEX ALL ON `ConversationMessageAttachmentChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ConversationMessageAttachment` DISABLE
-- ALTER INDEX ALL ON `ConversationMessageChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ConversationMessage` DISABLE
-- ALTER INDEX ALL ON `ConversationChannelChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ConversationChannel` DISABLE
-- ALTER INDEX ALL ON `ConversationUser` DISABLE
-- ALTER INDEX ALL ON `Conversation` DISABLE
-- ALTER INDEX ALL ON `ConversationType` DISABLE
-- ALTER INDEX ALL ON `NotificationDistribution` DISABLE
-- ALTER INDEX ALL ON `NotificationAttachmentChangeHistory` DISABLE
-- ALTER INDEX ALL ON `NotificationAttachment` DISABLE
-- ALTER INDEX ALL ON `NotificationChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Notification` DISABLE
-- ALTER INDEX ALL ON `NotificationType` DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON `EventResourceAssignmentChangeHistory` REBUILD
-- ALTER INDEX ALL ON `EventResourceAssignment` REBUILD
-- ALTER INDEX ALL ON `DocumentShareLinkChangeHistory` REBUILD
-- ALTER INDEX ALL ON `DocumentShareLink` REBUILD
-- ALTER INDEX ALL ON `DocumentDocumentTagChangeHistory` REBUILD
-- ALTER INDEX ALL ON `DocumentDocumentTag` REBUILD
-- ALTER INDEX ALL ON `DocumentChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Document` REBUILD
-- ALTER INDEX ALL ON `DocumentTagChangeHistory` REBUILD
-- ALTER INDEX ALL ON `DocumentTag` REBUILD
-- ALTER INDEX ALL ON `DocumentFolderChangeHistory` REBUILD
-- ALTER INDEX ALL ON `DocumentFolder` REBUILD
-- ALTER INDEX ALL ON `DocumentType` REBUILD
-- ALTER INDEX ALL ON `VolunteerGroupMemberChangeHistory` REBUILD
-- ALTER INDEX ALL ON `VolunteerGroupMember` REBUILD
-- ALTER INDEX ALL ON `VolunteerGroupChangeHistory` REBUILD
-- ALTER INDEX ALL ON `VolunteerGroup` REBUILD
-- ALTER INDEX ALL ON `VolunteerProfileChangeHistory` REBUILD
-- ALTER INDEX ALL ON `VolunteerProfile` REBUILD
-- ALTER INDEX ALL ON `SoftCreditChangeHistory` REBUILD
-- ALTER INDEX ALL ON `SoftCredit` REBUILD
-- ALTER INDEX ALL ON `GiftChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Gift` REBUILD
-- ALTER INDEX ALL ON `TributeChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Tribute` REBUILD
-- ALTER INDEX ALL ON `BatchChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Batch` REBUILD
-- ALTER INDEX ALL ON `BatchStatus` REBUILD
-- ALTER INDEX ALL ON `TributeType` REBUILD
-- ALTER INDEX ALL ON `PledgeChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Pledge` REBUILD
-- ALTER INDEX ALL ON `ConstituentChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Constituent` REBUILD
-- ALTER INDEX ALL ON `ConstituentJourneyStageChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ConstituentJourneyStage` REBUILD
-- ALTER INDEX ALL ON `HouseholdChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Household` REBUILD
-- ALTER INDEX ALL ON `AppealChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Appeal` REBUILD
-- ALTER INDEX ALL ON `CampaignChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Campaign` REBUILD
-- ALTER INDEX ALL ON `FundChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Fund` REBUILD
-- ALTER INDEX ALL ON `EventNotificationSubscriptionChangeHistory` REBUILD
-- ALTER INDEX ALL ON `EventNotificationSubscription` REBUILD
-- ALTER INDEX ALL ON `EventNotificationType` REBUILD
-- ALTER INDEX ALL ON `RecurrenceExceptionChangeHistory` REBUILD
-- ALTER INDEX ALL ON `RecurrenceException` REBUILD
-- ALTER INDEX ALL ON `ScheduledEventQualificationRequirementChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ScheduledEventQualificationRequirement` REBUILD
-- ALTER INDEX ALL ON `ScheduledEventDependencyChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ScheduledEventDependency` REBUILD
-- ALTER INDEX ALL ON `DependencyType` REBUILD
-- ALTER INDEX ALL ON `EventCalendar` REBUILD
-- ALTER INDEX ALL ON `ContactInteractionChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ContactInteraction` REBUILD
-- ALTER INDEX ALL ON `ReceiptChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Receipt` REBUILD
-- ALTER INDEX ALL ON `InvoiceLineItem` REBUILD
-- ALTER INDEX ALL ON `InvoiceChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Invoice` REBUILD
-- ALTER INDEX ALL ON `InvoiceStatus` REBUILD
-- ALTER INDEX ALL ON `PaymentTransactionChangeHistory` REBUILD
-- ALTER INDEX ALL ON `PaymentTransaction` REBUILD
-- ALTER INDEX ALL ON `PaymentProviderChangeHistory` REBUILD
-- ALTER INDEX ALL ON `PaymentProvider` REBUILD
-- ALTER INDEX ALL ON `PaymentMethod` REBUILD
-- ALTER INDEX ALL ON `GeneralLedgerLine` REBUILD
-- ALTER INDEX ALL ON `GeneralLedgerEntry` REBUILD
-- ALTER INDEX ALL ON `BudgetChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Budget` REBUILD
-- ALTER INDEX ALL ON `FinancialTransactionChangeHistory` REBUILD
-- ALTER INDEX ALL ON `FinancialTransaction` REBUILD
-- ALTER INDEX ALL ON `FiscalPeriodChangeHistory` REBUILD
-- ALTER INDEX ALL ON `FiscalPeriod` REBUILD
-- ALTER INDEX ALL ON `PeriodStatus` REBUILD
-- ALTER INDEX ALL ON `EventChargeChangeHistory` REBUILD
-- ALTER INDEX ALL ON `EventCharge` REBUILD
-- ALTER INDEX ALL ON `ChargeStatusChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ChargeStatus` REBUILD
-- ALTER INDEX ALL ON `ScheduledEventChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ScheduledEvent` REBUILD
-- ALTER INDEX ALL ON `EventTypeChangeHistory` REBUILD
-- ALTER INDEX ALL ON `EventType` REBUILD
-- ALTER INDEX ALL ON `ScheduledEventTemplateQualificationRequirementChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ScheduledEventTemplateQualificationRequirement` REBUILD
-- ALTER INDEX ALL ON `ScheduledEventTemplateChargeChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ScheduledEventTemplateCharge` REBUILD
-- ALTER INDEX ALL ON `ScheduledEventTemplateChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ScheduledEventTemplate` REBUILD
-- ALTER INDEX ALL ON `CrewMemberChangeHistory` REBUILD
-- ALTER INDEX ALL ON `CrewMember` REBUILD
-- ALTER INDEX ALL ON `CrewChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Crew` REBUILD
-- ALTER INDEX ALL ON `ResourceShiftChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ResourceShift` REBUILD
-- ALTER INDEX ALL ON `ResourceAvailabilityChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ResourceAvailability` REBUILD
-- ALTER INDEX ALL ON `ResourceQualificationChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ResourceQualification` REBUILD
-- ALTER INDEX ALL ON `RateSheetChangeHistory` REBUILD
-- ALTER INDEX ALL ON `RateSheet` REBUILD
-- ALTER INDEX ALL ON `ResourceContactChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ResourceContact` REBUILD
-- ALTER INDEX ALL ON `ResourceChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Resource` REBUILD
-- ALTER INDEX ALL ON `ShiftPatternDayChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ShiftPatternDay` REBUILD
-- ALTER INDEX ALL ON `ShiftPatternChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ShiftPattern` REBUILD
-- ALTER INDEX ALL ON `RecurrenceRuleChangeHistory` REBUILD
-- ALTER INDEX ALL ON `RecurrenceRule` REBUILD
-- ALTER INDEX ALL ON `RecurrenceFrequency` REBUILD
-- ALTER INDEX ALL ON `SchedulingTargetQualificationRequirementChangeHistory` REBUILD
-- ALTER INDEX ALL ON `SchedulingTargetQualificationRequirement` REBUILD
-- ALTER INDEX ALL ON `SchedulingTargetAddressChangeHistory` REBUILD
-- ALTER INDEX ALL ON `SchedulingTargetAddress` REBUILD
-- ALTER INDEX ALL ON `SchedulingTargetContactChangeHistory` REBUILD
-- ALTER INDEX ALL ON `SchedulingTargetContact` REBUILD
-- ALTER INDEX ALL ON `SchedulingTargetChangeHistory` REBUILD
-- ALTER INDEX ALL ON `SchedulingTarget` REBUILD
-- ALTER INDEX ALL ON `SchedulingTargetType` REBUILD
-- ALTER INDEX ALL ON `AssignmentStatus` REBUILD
-- ALTER INDEX ALL ON `BookingSourceType` REBUILD
-- ALTER INDEX ALL ON `ReceiptTypeChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ReceiptType` REBUILD
-- ALTER INDEX ALL ON `PaymentTypeChangeHistory` REBUILD
-- ALTER INDEX ALL ON `PaymentType` REBUILD
-- ALTER INDEX ALL ON `EventStatus` REBUILD
-- ALTER INDEX ALL ON `AssignmentRoleQualificationRequirementChangeHistory` REBUILD
-- ALTER INDEX ALL ON `AssignmentRoleQualificationRequirement` REBUILD
-- ALTER INDEX ALL ON `AssignmentRole` REBUILD
-- ALTER INDEX ALL ON `Qualification` REBUILD
-- ALTER INDEX ALL ON `TenantProfileChangeHistory` REBUILD
-- ALTER INDEX ALL ON `TenantProfile` REBUILD
-- ALTER INDEX ALL ON `ClientContactChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ClientContact` REBUILD
-- ALTER INDEX ALL ON `ClientChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Client` REBUILD
-- ALTER INDEX ALL ON `ClientType` REBUILD
-- ALTER INDEX ALL ON `CalendarChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Calendar` REBUILD
-- ALTER INDEX ALL ON `OfficeContactChangeHistory` REBUILD
-- ALTER INDEX ALL ON `OfficeContact` REBUILD
-- ALTER INDEX ALL ON `OfficeChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Office` REBUILD
-- ALTER INDEX ALL ON `OfficeType` REBUILD
-- ALTER INDEX ALL ON `ContactContactChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ContactContact` REBUILD
-- ALTER INDEX ALL ON `RelationshipType` REBUILD
-- ALTER INDEX ALL ON `ContactTagChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ContactTag` REBUILD
-- ALTER INDEX ALL ON `ContactChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Contact` REBUILD
-- ALTER INDEX ALL ON `ContactType` REBUILD
-- ALTER INDEX ALL ON `VolunteerStatus` REBUILD
-- ALTER INDEX ALL ON `StateProvince` REBUILD
-- ALTER INDEX ALL ON `Country` REBUILD
-- ALTER INDEX ALL ON `TimeZone` REBUILD
-- ALTER INDEX ALL ON `Tag` REBUILD
-- ALTER INDEX ALL ON `ChargeTypeChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ChargeType` REBUILD
-- ALTER INDEX ALL ON `TaxCode` REBUILD
-- ALTER INDEX ALL ON `FinancialCategoryChangeHistory` REBUILD
-- ALTER INDEX ALL ON `FinancialCategory` REBUILD
-- ALTER INDEX ALL ON `FinancialOfficeChangeHistory` REBUILD
-- ALTER INDEX ALL ON `FinancialOffice` REBUILD
-- ALTER INDEX ALL ON `AccountType` REBUILD
-- ALTER INDEX ALL ON `Currency` REBUILD
-- ALTER INDEX ALL ON `InteractionType` REBUILD
-- ALTER INDEX ALL ON `RateType` REBUILD
-- ALTER INDEX ALL ON `ContactMethod` REBUILD
-- ALTER INDEX ALL ON `Priority` REBUILD
-- ALTER INDEX ALL ON `ResourceType` REBUILD
-- ALTER INDEX ALL ON `Salutation` REBUILD
-- ALTER INDEX ALL ON `Icon` REBUILD
-- ALTER INDEX ALL ON `AttributeDefinitionChangeHistory` REBUILD
-- ALTER INDEX ALL ON `AttributeDefinition` REBUILD
-- ALTER INDEX ALL ON `AttributeDefinitionEntity` REBUILD
-- ALTER INDEX ALL ON `AttributeDefinitionType` REBUILD
-- ALTER INDEX ALL ON `CallEventLog` REBUILD
-- ALTER INDEX ALL ON `CallParticipant` REBUILD
-- ALTER INDEX ALL ON `Call` REBUILD
-- ALTER INDEX ALL ON `CallStatus` REBUILD
-- ALTER INDEX ALL ON `CallType` REBUILD
-- ALTER INDEX ALL ON `MessagingAuditLog` REBUILD
-- ALTER INDEX ALL ON `MessageFlag` REBUILD
-- ALTER INDEX ALL ON `PushProviderConfiguration` REBUILD
-- ALTER INDEX ALL ON `PushDeliveryLog` REBUILD
-- ALTER INDEX ALL ON `MessageBookmark` REBUILD
-- ALTER INDEX ALL ON `ConversationThreadUser` REBUILD
-- ALTER INDEX ALL ON `UserPresence` REBUILD
-- ALTER INDEX ALL ON `ConversationMessageLinkPreviewChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ConversationMessageLinkPreview` REBUILD
-- ALTER INDEX ALL ON `ConversationPin` REBUILD
-- ALTER INDEX ALL ON `ConversationMessageReaction` REBUILD
-- ALTER INDEX ALL ON `ConversationMessageUser` REBUILD
-- ALTER INDEX ALL ON `ConversationMessageAttachmentChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ConversationMessageAttachment` REBUILD
-- ALTER INDEX ALL ON `ConversationMessageChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ConversationMessage` REBUILD
-- ALTER INDEX ALL ON `ConversationChannelChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ConversationChannel` REBUILD
-- ALTER INDEX ALL ON `ConversationUser` REBUILD
-- ALTER INDEX ALL ON `Conversation` REBUILD
-- ALTER INDEX ALL ON `ConversationType` REBUILD
-- ALTER INDEX ALL ON `NotificationDistribution` REBUILD
-- ALTER INDEX ALL ON `NotificationAttachmentChangeHistory` REBUILD
-- ALTER INDEX ALL ON `NotificationAttachment` REBUILD
-- ALTER INDEX ALL ON `NotificationChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Notification` REBUILD
-- ALTER INDEX ALL ON `NotificationType` REBUILD

-- This table defines the types of notifications that are available.  It is part of the Foundation Notification system.
CREATE TABLE `NotificationType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NULL,
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the NotificationType table's name field.
CREATE INDEX `I_NotificationType_name` ON `NotificationType` (`name`);

-- Index on the NotificationType table's active field.
CREATE INDEX `I_NotificationType_active` ON `NotificationType` (`active`);

-- Index on the NotificationType table's deleted field.
CREATE INDEX `I_NotificationType_deleted` ON `NotificationType` (`deleted`);

-- Index on the NotificationType table's id,active,deleted fields.
CREATE INDEX `I_NotificationType_id_active_deleted` ON `NotificationType` (`id`, `active`, `deleted`);

INSERT INTO `NotificationType` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Informative', 'Informative', '065c2b74-dae1-4450-b8ee-bc5500ce64eb' );

INSERT INTO `NotificationType` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Regular', 'Regular', 'e7c40dde-461f-41d1-8a9b-32e128179baa' );

INSERT INTO `NotificationType` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Urgent', 'Urgent', '825c0094-f3bb-45da-aa22-da459f4593b4' );


-- This table store Notifications.  It is part of the Foundation Notification system.
CREATE TABLE `Notification`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`notificationTypeId` INT NULL,		-- Link to the NotificationType table.
	`createdByUserId` INT NULL,		-- The user that created this notification.  Nullable so that the 'system' can create them too.  Resolved by IMessagingUserResolver.
	`message` TEXT NOT NULL,
	`priority` INT NOT NULL DEFAULT 100,		-- The intent here is that the lower the priority number, the more urgent the notification is.
	`entity` VARCHAR(250) NULL,		-- The name of the entity that this notification is about.
	`entityId` INT NULL,		-- The ID for the entity that this notification is about.
	`externalURL` VARCHAR(1000) NULL,		-- Ad-hoc external URL to be used if helpful.
	`dateTimeCreated` DATETIME NOT NULL,		-- When the notification was created.
	`dateTimeDistributed` DATETIME NULL,		-- When the notification was distributed.
	`distributionCompleted` BIT NOT NULL DEFAULT 0,		-- Control flag to mark whether or not this notification has been distributed to the notificationUser table or not
	`userId` INT NULL,		-- Optional target user for this notification.  Resolved by IMessagingUserResolver.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`notificationTypeId`) REFERENCES `NotificationType`(`id`)		-- Foreign key to the NotificationType table.
);
-- Index on the Notification table's tenantGuid field.
CREATE INDEX `I_Notification_tenantGuid` ON `Notification` (`tenantGuid`);

-- Index on the Notification table's tenantGuid,notificationTypeId fields.
CREATE INDEX `I_Notification_tenantGuid_notificationTypeId` ON `Notification` (`tenantGuid`, `notificationTypeId`);

-- Index on the Notification table's tenantGuid,active fields.
CREATE INDEX `I_Notification_tenantGuid_active` ON `Notification` (`tenantGuid`, `active`);

-- Index on the Notification table's tenantGuid,deleted fields.
CREATE INDEX `I_Notification_tenantGuid_deleted` ON `Notification` (`tenantGuid`, `deleted`);

-- Index on the Notification table's id,active,deleted fields.
CREATE INDEX `I_Notification_id_active_deleted` ON `Notification` (`id`, `active`, `deleted`);


-- The change history for records from the Notification table.
CREATE TABLE `NotificationChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`notificationId` INT NOT NULL,		-- Link to the Notification table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`notificationId`) REFERENCES `Notification`(`id`)		-- Foreign key to the Notification table.
);
-- Index on the NotificationChangeHistory table's tenantGuid field.
CREATE INDEX `I_NotificationChangeHistory_tenantGuid` ON `NotificationChangeHistory` (`tenantGuid`);

-- Index on the NotificationChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_NotificationChangeHistory_tenantGuid_versionNumber` ON `NotificationChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the NotificationChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_NotificationChangeHistory_tenantGuid_timeStamp` ON `NotificationChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the NotificationChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_NotificationChangeHistory_tenantGuid_userId` ON `NotificationChangeHistory` (`tenantGuid`, `userId`);

-- Index on the NotificationChangeHistory table's tenantGuid,notificationId fields.
CREATE INDEX `I_NotificationChangeHistory_tenantGuid_notificationId` ON `NotificationChangeHistory` (`tenantGuid`, `notificationId`, `versionNumber`, `timeStamp`, `userId`);


-- This table stores attachments for notifications.  It is part of the Foundation Notification system.
CREATE TABLE `NotificationAttachment`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`notificationId` INT NOT NULL,		-- The notification for this attachment.
	`userId` INT NOT NULL,		-- The user that created this attachment.  Resolved by IMessagingUserResolver.
	`dateTimeCreated` DATETIME NOT NULL,		-- When this notification attachment was created.
	`contentFileName` VARCHAR(250) NOT NULL,		-- Part of the binary data field setup
	`contentSize` BIGINT NOT NULL,		-- Part of the binary data field setup
	`contentData` BLOB NOT NULL,		-- Part of the binary data field setup
	`contentMimeType` VARCHAR(100) NOT NULL,		-- Part of the binary data field setup
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`notificationId`) REFERENCES `Notification`(`id`)		-- Foreign key to the Notification table.
);
-- Index on the NotificationAttachment table's tenantGuid field.
CREATE INDEX `I_NotificationAttachment_tenantGuid` ON `NotificationAttachment` (`tenantGuid`);

-- Index on the NotificationAttachment table's tenantGuid,notificationId fields.
CREATE INDEX `I_NotificationAttachment_tenantGuid_notificationId` ON `NotificationAttachment` (`tenantGuid`, `notificationId`);

-- Index on the NotificationAttachment table's tenantGuid,active fields.
CREATE INDEX `I_NotificationAttachment_tenantGuid_active` ON `NotificationAttachment` (`tenantGuid`, `active`);

-- Index on the NotificationAttachment table's tenantGuid,deleted fields.
CREATE INDEX `I_NotificationAttachment_tenantGuid_deleted` ON `NotificationAttachment` (`tenantGuid`, `deleted`);

-- Index on the NotificationAttachment table's id,active,deleted fields.
CREATE INDEX `I_NotificationAttachment_id_active_deleted` ON `NotificationAttachment` (`id`, `active`, `deleted`);


-- The change history for records from the NotificationAttachment table.
CREATE TABLE `NotificationAttachmentChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`notificationAttachmentId` INT NOT NULL,		-- Link to the NotificationAttachment table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`notificationAttachmentId`) REFERENCES `NotificationAttachment`(`id`)		-- Foreign key to the NotificationAttachment table.
);
-- Index on the NotificationAttachmentChangeHistory table's tenantGuid field.
CREATE INDEX `I_NotificationAttachmentChangeHistory_tenantGuid` ON `NotificationAttachmentChangeHistory` (`tenantGuid`);

-- Index on the NotificationAttachmentChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_NotificationAttachmentChangeHistory_tenantGuid_versionNumber` ON `NotificationAttachmentChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the NotificationAttachmentChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_NotificationAttachmentChangeHistory_tenantGuid_timeStamp` ON `NotificationAttachmentChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the NotificationAttachmentChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_NotificationAttachmentChangeHistory_tenantGuid_userId` ON `NotificationAttachmentChangeHistory` (`tenantGuid`, `userId`);

-- Index on the NotificationAttachmentChangeHistory table's tenantGuid,notificationAttachmentId fields.
CREATE INDEX `I_NotificationAttachmentChangeHistory_tenantGuid_notificationAtt` ON `NotificationAttachmentChangeHistory` (`tenantGuid`, `notificationAttachmentId`, `versionNumber`, `timeStamp`, `userId`);


-- This table defines the distribution for a notification.  It is part of the Foundation Notification system.
CREATE TABLE `NotificationDistribution`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`notificationId` INT NOT NULL,		-- The notification that is being distributed.
	`userId` INT NOT NULL,		-- The user to distribute the notification to.  Resolved by IMessagingUserResolver.
	`acknowledged` BIT NOT NULL DEFAULT 0,		-- Whether or not the notification has been acknowledged.
	`dateTimeAcknowledged` DATETIME NULL,		-- When the notification was acknowledged.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`notificationId`) REFERENCES `Notification`(`id`)		-- Foreign key to the Notification table.
);
-- Index on the NotificationDistribution table's tenantGuid field.
CREATE INDEX `I_NotificationDistribution_tenantGuid` ON `NotificationDistribution` (`tenantGuid`);

-- Index on the NotificationDistribution table's tenantGuid,notificationId fields.
CREATE INDEX `I_NotificationDistribution_tenantGuid_notificationId` ON `NotificationDistribution` (`tenantGuid`, `notificationId`);

-- Index on the NotificationDistribution table's tenantGuid,active fields.
CREATE INDEX `I_NotificationDistribution_tenantGuid_active` ON `NotificationDistribution` (`tenantGuid`, `active`);

-- Index on the NotificationDistribution table's tenantGuid,deleted fields.
CREATE INDEX `I_NotificationDistribution_tenantGuid_deleted` ON `NotificationDistribution` (`tenantGuid`, `deleted`);

-- Index on the NotificationDistribution table's id,active,deleted fields.
CREATE INDEX `I_NotificationDistribution_id_active_deleted` ON `NotificationDistribution` (`id`, `active`, `deleted`);


/*
This is the main Conversation Type table.  It provides the types of conversations that can be created.

It is part of the Foundation's Conversation/Messaging system.
*/
CREATE TABLE `ConversationType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NULL,
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the ConversationType table's name field.
CREATE INDEX `I_ConversationType_name` ON `ConversationType` (`name`);

-- Index on the ConversationType table's active field.
CREATE INDEX `I_ConversationType_active` ON `ConversationType` (`active`);

-- Index on the ConversationType table's deleted field.
CREATE INDEX `I_ConversationType_deleted` ON `ConversationType` (`deleted`);

-- Index on the ConversationType table's id,active,deleted fields.
CREATE INDEX `I_ConversationType_id_active_deleted` ON `ConversationType` (`id`, `active`, `deleted`);

INSERT INTO `ConversationType` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Regular', 'Regular', '70174fce-f8de-4c44-b11f-db68b314204b' );

INSERT INTO `ConversationType` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Urgent', 'Urgent', '987ea6eb-155a-44ed-ac57-15a72ad2ae27' );

INSERT INTO `ConversationType` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Channel', 'A persistent named conversation (like a Teams/Slack channel)', '54883c38-7860-40bf-a6e4-ce5535db7ed4' );

INSERT INTO `ConversationType` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Direct Message', 'A 1:1 or small group direct message conversation', 'd45cfb49-0dbc-4c05-a8b7-f17a6e71a926' );


-- This is the main Conversation table.  It is part of the Foundation's Conversation/Messaging system.
CREATE TABLE `Conversation`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`createdByUserId` INT NULL,		-- The user that started the conversation.  Resolved by IMessagingUserResolver.  Nullable for system-started conversations.
	`conversationTypeId` INT NULL,		-- Link to the ConversationType table.
	`priority` INT NOT NULL DEFAULT 100,		-- The intent here is that the lower the priority number, the more urgent the conversation is.
	`dateTimeCreated` DATETIME NOT NULL,		-- When the conversation was created.
	`entity` VARCHAR(250) NULL,		-- The in case the conversation is to do with an entity, it is named here
	`entityId` INT NULL,		-- The id of the entity that the conversation is about
	`externalURL` VARCHAR(1000) NULL,
	`name` VARCHAR(250) NULL,		-- The name of the conversation.  A named conversation is a channel.  Optional because not all conversations will be channels.
	`description` VARCHAR(1000) NULL,		-- The description of the channel conversation.  Optional because not all conversations need to have a description, but if it does have one, this is where it goes.
	`isPublic` BIT NULL,		-- Whether or not the conversation is public
	`userId` INT NULL,		-- Optional target user for this conversation.  Resolved by IMessagingUserResolver.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`conversationTypeId`) REFERENCES `ConversationType`(`id`)		-- Foreign key to the ConversationType table.
);
-- Index on the Conversation table's tenantGuid field.
CREATE INDEX `I_Conversation_tenantGuid` ON `Conversation` (`tenantGuid`);

-- Index on the Conversation table's tenantGuid,conversationTypeId fields.
CREATE INDEX `I_Conversation_tenantGuid_conversationTypeId` ON `Conversation` (`tenantGuid`, `conversationTypeId`);

-- Index on the Conversation table's tenantGuid,active fields.
CREATE INDEX `I_Conversation_tenantGuid_active` ON `Conversation` (`tenantGuid`, `active`);

-- Index on the Conversation table's tenantGuid,deleted fields.
CREATE INDEX `I_Conversation_tenantGuid_deleted` ON `Conversation` (`tenantGuid`, `deleted`);

-- Index on the Conversation table's id,active,deleted fields.
CREATE INDEX `I_Conversation_id_active_deleted` ON `Conversation` (`id`, `active`, `deleted`);


-- This is the ConversationUser table.  It tracks the users that belong to a conversation.  It is part of the Foundation's Conversation/Messaging system.
CREATE TABLE `ConversationUser`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`conversationId` INT NOT NULL,		-- Link to the Conversation table.
	`userId` INT NOT NULL,		-- The user in this conversation.  Resolved by IMessagingUserResolver.
	`role` VARCHAR(50) NOT NULL DEFAULT 'Member',
	`dateTimeAdded` DATETIME NOT NULL,		-- When this user was added to the conversation.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`conversationId`) REFERENCES `Conversation`(`id`)		-- Foreign key to the Conversation table.
);
-- Index on the ConversationUser table's tenantGuid field.
CREATE INDEX `I_ConversationUser_tenantGuid` ON `ConversationUser` (`tenantGuid`);

-- Index on the ConversationUser table's tenantGuid,conversationId fields.
CREATE INDEX `I_ConversationUser_tenantGuid_conversationId` ON `ConversationUser` (`tenantGuid`, `conversationId`);

-- Index on the ConversationUser table's tenantGuid,active fields.
CREATE INDEX `I_ConversationUser_tenantGuid_active` ON `ConversationUser` (`tenantGuid`, `active`);

-- Index on the ConversationUser table's tenantGuid,deleted fields.
CREATE INDEX `I_ConversationUser_tenantGuid_deleted` ON `ConversationUser` (`tenantGuid`, `deleted`);

-- Index on the ConversationUser table's id,active,deleted fields.
CREATE INDEX `I_ConversationUser_id_active_deleted` ON `ConversationUser` (`id`, `active`, `deleted`);


/*
This table extends a Conversation record to be a named Channel.  It is part of the Foundation's Messaging system.

A channel is a persistent, named conversation that users can browse and join.  It links to the base Conversation record 
and adds channel-specific fields like name, topic, and privacy.
*/
CREATE TABLE `ConversationChannel`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`conversationId` INT NOT NULL,		-- The base conversation record that this channel extends.
	`name` VARCHAR(250) NOT NULL,		-- The display name for the channel.
	`topic` VARCHAR(1000) NULL,		-- The current topic or description of the channel.  Can be changed over time.
	`isPrivate` BIT NOT NULL DEFAULT 0,		-- Whether or not this channel is private.  Private channels are invitation-only and do not appear in the channel browser.
	`isPinned` BIT NOT NULL DEFAULT 0,		-- Whether or not this channel is pinned in the UI.  Pinned channels appear at the top of the channel list.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`conversationId`) REFERENCES `Conversation`(`id`)		-- Foreign key to the Conversation table.
);
-- Index on the ConversationChannel table's tenantGuid field.
CREATE INDEX `I_ConversationChannel_tenantGuid` ON `ConversationChannel` (`tenantGuid`);

-- Index on the ConversationChannel table's tenantGuid,conversationId fields.
CREATE INDEX `I_ConversationChannel_tenantGuid_conversationId` ON `ConversationChannel` (`tenantGuid`, `conversationId`);

-- Index on the ConversationChannel table's tenantGuid,active fields.
CREATE INDEX `I_ConversationChannel_tenantGuid_active` ON `ConversationChannel` (`tenantGuid`, `active`);

-- Index on the ConversationChannel table's tenantGuid,deleted fields.
CREATE INDEX `I_ConversationChannel_tenantGuid_deleted` ON `ConversationChannel` (`tenantGuid`, `deleted`);

-- Index on the ConversationChannel table's id,active,deleted fields.
CREATE INDEX `I_ConversationChannel_id_active_deleted` ON `ConversationChannel` (`id`, `active`, `deleted`);


-- The change history for records from the ConversationChannel table.
CREATE TABLE `ConversationChannelChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`conversationChannelId` INT NOT NULL,		-- Link to the ConversationChannel table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`conversationChannelId`) REFERENCES `ConversationChannel`(`id`)		-- Foreign key to the ConversationChannel table.
);
-- Index on the ConversationChannelChangeHistory table's tenantGuid field.
CREATE INDEX `I_ConversationChannelChangeHistory_tenantGuid` ON `ConversationChannelChangeHistory` (`tenantGuid`);

-- Index on the ConversationChannelChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ConversationChannelChangeHistory_tenantGuid_versionNumber` ON `ConversationChannelChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ConversationChannelChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ConversationChannelChangeHistory_tenantGuid_timeStamp` ON `ConversationChannelChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ConversationChannelChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ConversationChannelChangeHistory_tenantGuid_userId` ON `ConversationChannelChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ConversationChannelChangeHistory table's tenantGuid,conversationChannelId fields.
CREATE INDEX `I_ConversationChannelChangeHistory_tenantGuid_conversationChanne` ON `ConversationChannelChangeHistory` (`tenantGuid`, `conversationChannelId`, `versionNumber`, `timeStamp`, `userId`);


-- This is the ConversationMessage table.  It tracks the messages that belong to a conversation.  It is part of the Foundation's Conversation/Messaging system.
CREATE TABLE `ConversationMessage`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`conversationId` INT NOT NULL,		-- Link to the Conversation table.
	`userId` INT NOT NULL,		-- The user that sent this message.  Resolved by IMessagingUserResolver.
	`parentConversationMessageId` INT NULL,		-- Link to the ConversationMessage table.
	`conversationChannelId` INT NULL,		-- Optional channel that this message belongs to.  NULL = conversation-level (no channel).
	`dateTimeCreated` DATETIME NOT NULL,		-- When this message was created.
	`message` TEXT NOT NULL,
	`messageType` VARCHAR(50) NULL,		-- The type of message: null/'text' (default), 'voice', 'video', 'call_event'. Determines how the client renders the message content.
	`entity` VARCHAR(250) NULL,		-- The in case the conversation message is to do with an entity, it is named here
	`entityId` INT NULL,		-- The id of the entity that the message is about
	`externalURL` VARCHAR(1000) NULL,
	`forwardedFromMessageId` INT NULL,		-- The ID of the original message that was forwarded.  NULL if the message is not a forward.
	`forwardedFromUserId` INT NULL,		-- The user ID of the original sender whose message was forwarded.  Resolved by IMessagingUserResolver.
	`isScheduled` BIT NOT NULL DEFAULT 0,		-- Whether this message is scheduled for future delivery.
	`scheduledDateTime` DATETIME NULL,		-- When the scheduled message should be released.  Null if not scheduled.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`conversationId`) REFERENCES `Conversation`(`id`),		-- Foreign key to the Conversation table.
	FOREIGN KEY (`parentConversationMessageId`) REFERENCES `ConversationMessage`(`id`),		-- Foreign key to the ConversationMessage table.
	FOREIGN KEY (`conversationChannelId`) REFERENCES `ConversationChannel`(`id`)		-- Foreign key to the ConversationChannel table.
);
-- Index on the ConversationMessage table's tenantGuid field.
CREATE INDEX `I_ConversationMessage_tenantGuid` ON `ConversationMessage` (`tenantGuid`);

-- Index on the ConversationMessage table's tenantGuid,conversationId fields.
CREATE INDEX `I_ConversationMessage_tenantGuid_conversationId` ON `ConversationMessage` (`tenantGuid`, `conversationId`);

-- Index on the ConversationMessage table's tenantGuid,parentConversationMessageId fields.
CREATE INDEX `I_ConversationMessage_tenantGuid_parentConversationMessageId` ON `ConversationMessage` (`tenantGuid`, `parentConversationMessageId`);

-- Index on the ConversationMessage table's tenantGuid,conversationChannelId fields.
CREATE INDEX `I_ConversationMessage_tenantGuid_conversationChannelId` ON `ConversationMessage` (`tenantGuid`, `conversationChannelId`);

-- Index on the ConversationMessage table's tenantGuid,active fields.
CREATE INDEX `I_ConversationMessage_tenantGuid_active` ON `ConversationMessage` (`tenantGuid`, `active`);

-- Index on the ConversationMessage table's tenantGuid,deleted fields.
CREATE INDEX `I_ConversationMessage_tenantGuid_deleted` ON `ConversationMessage` (`tenantGuid`, `deleted`);

-- Index on the ConversationMessage table's id,active,deleted fields.
CREATE INDEX `I_ConversationMessage_id_active_deleted` ON `ConversationMessage` (`id`, `active`, `deleted`);

-- Index on the ConversationMessage table's tenantGuid,dateTimeCreated fields.
CREATE INDEX `I_ConversationMessage_tenantGuid_dateTimeCreated` ON `ConversationMessage` (`tenantGuid`, `dateTimeCreated`);


-- The change history for records from the ConversationMessage table.
CREATE TABLE `ConversationMessageChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`conversationMessageId` INT NOT NULL,		-- Link to the ConversationMessage table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`conversationMessageId`) REFERENCES `ConversationMessage`(`id`)		-- Foreign key to the ConversationMessage table.
);
-- Index on the ConversationMessageChangeHistory table's tenantGuid field.
CREATE INDEX `I_ConversationMessageChangeHistory_tenantGuid` ON `ConversationMessageChangeHistory` (`tenantGuid`);

-- Index on the ConversationMessageChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ConversationMessageChangeHistory_tenantGuid_versionNumber` ON `ConversationMessageChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ConversationMessageChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ConversationMessageChangeHistory_tenantGuid_timeStamp` ON `ConversationMessageChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ConversationMessageChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ConversationMessageChangeHistory_tenantGuid_userId` ON `ConversationMessageChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ConversationMessageChangeHistory table's tenantGuid,conversationMessageId fields.
CREATE INDEX `I_ConversationMessageChangeHistory_tenantGuid_conversationMessag` ON `ConversationMessageChangeHistory` (`tenantGuid`, `conversationMessageId`, `versionNumber`, `timeStamp`, `userId`);


-- This is the ConversationMessageAttachment table.  It tracks the attachments that belong to a message in a conversation.  It is part of the Foundation's Conversation/Messaging system.
CREATE TABLE `ConversationMessageAttachment`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`conversationMessageId` INT NOT NULL,		-- Link to the ConversationMessage table.
	`userId` INT NOT NULL,		-- The user that uploaded this attachment.  Resolved by IMessagingUserResolver.
	`dateTimeCreated` DATETIME NOT NULL,		-- When this conversation message attachment was created.
	`contentFileName` VARCHAR(250) NOT NULL,		-- Part of the binary data field setup
	`contentSize` BIGINT NOT NULL,		-- Part of the binary data field setup
	`contentData` BLOB NOT NULL,		-- Part of the binary data field setup
	`contentMimeType` VARCHAR(100) NOT NULL,		-- Part of the binary data field setup
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`conversationMessageId`) REFERENCES `ConversationMessage`(`id`)		-- Foreign key to the ConversationMessage table.
);
-- Index on the ConversationMessageAttachment table's tenantGuid field.
CREATE INDEX `I_ConversationMessageAttachment_tenantGuid` ON `ConversationMessageAttachment` (`tenantGuid`);

-- Index on the ConversationMessageAttachment table's tenantGuid,conversationMessageId fields.
CREATE INDEX `I_ConversationMessageAttachment_tenantGuid_conversationMessageId` ON `ConversationMessageAttachment` (`tenantGuid`, `conversationMessageId`);

-- Index on the ConversationMessageAttachment table's tenantGuid,active fields.
CREATE INDEX `I_ConversationMessageAttachment_tenantGuid_active` ON `ConversationMessageAttachment` (`tenantGuid`, `active`);

-- Index on the ConversationMessageAttachment table's tenantGuid,deleted fields.
CREATE INDEX `I_ConversationMessageAttachment_tenantGuid_deleted` ON `ConversationMessageAttachment` (`tenantGuid`, `deleted`);

-- Index on the ConversationMessageAttachment table's id,active,deleted fields.
CREATE INDEX `I_ConversationMessageAttachment_id_active_deleted` ON `ConversationMessageAttachment` (`id`, `active`, `deleted`);


-- The change history for records from the ConversationMessageAttachment table.
CREATE TABLE `ConversationMessageAttachmentChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`conversationMessageAttachmentId` INT NOT NULL,		-- Link to the ConversationMessageAttachment table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`conversationMessageAttachmentId`) REFERENCES `ConversationMessageAttachment`(`id`)		-- Foreign key to the ConversationMessageAttachment table.
);
-- Index on the ConversationMessageAttachmentChangeHistory table's tenantGuid field.
CREATE INDEX `I_ConversationMessageAttachmentChangeHistory_tenantGuid` ON `ConversationMessageAttachmentChangeHistory` (`tenantGuid`);

-- Index on the ConversationMessageAttachmentChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ConversationMessageAttachmentChangeHistory_tenantGuid_versionN` ON `ConversationMessageAttachmentChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ConversationMessageAttachmentChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ConversationMessageAttachmentChangeHistory_tenantGuid_timeStam` ON `ConversationMessageAttachmentChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ConversationMessageAttachmentChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ConversationMessageAttachmentChangeHistory_tenantGuid_userId` ON `ConversationMessageAttachmentChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ConversationMessageAttachmentChangeHistory table's tenantGuid,conversationMessageAttachmentId fields.
CREATE INDEX `I_ConversationMessageAttachmentChangeHistory_tenantGuid_conversa` ON `ConversationMessageAttachmentChangeHistory` (`tenantGuid`, `conversationMessageAttachmentId`, `versionNumber`, `timeStamp`, `userId`);


-- This is the ConversationMessageUser table.  It tracks the users that belong to a message in a conversation.  It is part of the Foundation's Conversation/Messaging system.
CREATE TABLE `ConversationMessageUser`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`conversationMessageId` INT NOT NULL,		-- Link to the ConversationMessage table.
	`userId` INT NOT NULL,		-- The target user for this message.  Resolved by IMessagingUserResolver.
	`dateTimeCreated` DATETIME NOT NULL,		-- When this conversation message user was created.
	`acknowledged` BIT NOT NULL DEFAULT 0,
	`dateTimeAcknowledged` DATETIME NOT NULL,		-- When this conversation message user was acknowledge by the user.  For messages, this may be auto acknowledged once the data is read and shown.  Up to the UI to decide when to mark it as acknowledged..
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`conversationMessageId`) REFERENCES `ConversationMessage`(`id`)		-- Foreign key to the ConversationMessage table.
);
-- Index on the ConversationMessageUser table's tenantGuid field.
CREATE INDEX `I_ConversationMessageUser_tenantGuid` ON `ConversationMessageUser` (`tenantGuid`);

-- Index on the ConversationMessageUser table's tenantGuid,conversationMessageId fields.
CREATE INDEX `I_ConversationMessageUser_tenantGuid_conversationMessageId` ON `ConversationMessageUser` (`tenantGuid`, `conversationMessageId`);

-- Index on the ConversationMessageUser table's tenantGuid,active fields.
CREATE INDEX `I_ConversationMessageUser_tenantGuid_active` ON `ConversationMessageUser` (`tenantGuid`, `active`);

-- Index on the ConversationMessageUser table's tenantGuid,deleted fields.
CREATE INDEX `I_ConversationMessageUser_tenantGuid_deleted` ON `ConversationMessageUser` (`tenantGuid`, `deleted`);

-- Index on the ConversationMessageUser table's id,active,deleted fields.
CREATE INDEX `I_ConversationMessageUser_id_active_deleted` ON `ConversationMessageUser` (`id`, `active`, `deleted`);


/*
This table stores emoji reactions to conversation messages.  It is part of the Foundation's Messaging system.

Reactions provide a lightweight way for users to respond to messages without creating additional message records.  
Each reaction is a short string representing an emoji code or shortname.
*/
CREATE TABLE `ConversationMessageReaction`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`conversationMessageId` INT NOT NULL,		-- The message that this reaction is for.
	`userId` INT NOT NULL,		-- The user who reacted.  Resolved by IMessagingUserResolver.
	`reaction` VARCHAR(50) NOT NULL,		-- The emoji code or shortname for the reaction, for example 'thumbsup', 'heart', 'laughing'.
	`dateTimeCreated` DATETIME NOT NULL,		-- When this reaction was created.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`conversationMessageId`) REFERENCES `ConversationMessage`(`id`)		-- Foreign key to the ConversationMessage table.
);
-- Index on the ConversationMessageReaction table's tenantGuid field.
CREATE INDEX `I_ConversationMessageReaction_tenantGuid` ON `ConversationMessageReaction` (`tenantGuid`);

-- Index on the ConversationMessageReaction table's tenantGuid,conversationMessageId fields.
CREATE INDEX `I_ConversationMessageReaction_tenantGuid_conversationMessageId` ON `ConversationMessageReaction` (`tenantGuid`, `conversationMessageId`);

-- Index on the ConversationMessageReaction table's tenantGuid,active fields.
CREATE INDEX `I_ConversationMessageReaction_tenantGuid_active` ON `ConversationMessageReaction` (`tenantGuid`, `active`);

-- Index on the ConversationMessageReaction table's tenantGuid,deleted fields.
CREATE INDEX `I_ConversationMessageReaction_tenantGuid_deleted` ON `ConversationMessageReaction` (`tenantGuid`, `deleted`);

-- Index on the ConversationMessageReaction table's id,active,deleted fields.
CREATE INDEX `I_ConversationMessageReaction_id_active_deleted` ON `ConversationMessageReaction` (`id`, `active`, `deleted`);

-- Index on the ConversationMessageReaction table's conversationMessageId,active,deleted fields.
CREATE INDEX `I_ConversationMessageReaction_conversationMessageId_active_delet` ON `ConversationMessageReaction` (`conversationMessageId`, `active`, `deleted`);


/*
This table tracks pinned messages within a conversation.  It is part of the Foundation's Messaging system.

Pinned messages are highlighted in the conversation and can be browsed separately, providing a way to bookmark 
important messages, decisions, or reference information within a conversation.
*/
CREATE TABLE `ConversationPin`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`conversationId` INT NOT NULL,		-- The conversation that this pin belongs to.
	`conversationMessageId` INT NOT NULL,		-- The message that is pinned.
	`pinnedByUserId` INT NOT NULL,		-- The user who pinned this message.  Resolved by IMessagingUserResolver.
	`dateTimePinned` DATETIME NOT NULL,		-- When this message was pinned.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`conversationId`) REFERENCES `Conversation`(`id`),		-- Foreign key to the Conversation table.
	FOREIGN KEY (`conversationMessageId`) REFERENCES `ConversationMessage`(`id`)		-- Foreign key to the ConversationMessage table.
);
-- Index on the ConversationPin table's tenantGuid field.
CREATE INDEX `I_ConversationPin_tenantGuid` ON `ConversationPin` (`tenantGuid`);

-- Index on the ConversationPin table's tenantGuid,conversationId fields.
CREATE INDEX `I_ConversationPin_tenantGuid_conversationId` ON `ConversationPin` (`tenantGuid`, `conversationId`);

-- Index on the ConversationPin table's tenantGuid,conversationMessageId fields.
CREATE INDEX `I_ConversationPin_tenantGuid_conversationMessageId` ON `ConversationPin` (`tenantGuid`, `conversationMessageId`);

-- Index on the ConversationPin table's tenantGuid,active fields.
CREATE INDEX `I_ConversationPin_tenantGuid_active` ON `ConversationPin` (`tenantGuid`, `active`);

-- Index on the ConversationPin table's tenantGuid,deleted fields.
CREATE INDEX `I_ConversationPin_tenantGuid_deleted` ON `ConversationPin` (`tenantGuid`, `deleted`);

-- Index on the ConversationPin table's id,active,deleted fields.
CREATE INDEX `I_ConversationPin_id_active_deleted` ON `ConversationPin` (`id`, `active`, `deleted`);

-- Index on the ConversationPin table's conversationId,active,deleted fields.
CREATE INDEX `I_ConversationPin_conversationId_active_deleted` ON `ConversationPin` (`conversationId`, `active`, `deleted`);


/*
This table stores link preview data (Open Graph metadata) for URLs found in conversation messages.  It is part of the Foundation's Messaging system.

When a message containing URLs is sent, the system asynchronously fetches Open Graph / meta tag data for each URL and stores the results here.  
The preview data is then pushed to connected clients via SignalR so link preview cards appear below the message bubble.
*/
CREATE TABLE `ConversationMessageLinkPreview`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`conversationMessageId` INT NOT NULL,		-- The message that contains this URL.
	`url` VARCHAR(1000) NOT NULL,		-- The original URL found in the message.
	`title` VARCHAR(500) NULL,		-- The page title from og:title or <title> tag.
	`description` VARCHAR(1000) NULL,		-- The page description from og:description or meta description.
	`imageUrl` VARCHAR(1000) NULL,		-- The preview image URL from og:image.
	`siteName` VARCHAR(250) NULL,		-- The site name from og:site_name.
	`fetchedDateTime` DATETIME NOT NULL,		-- When the preview data was fetched from the URL.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`conversationMessageId`) REFERENCES `ConversationMessage`(`id`)		-- Foreign key to the ConversationMessage table.
);
-- Index on the ConversationMessageLinkPreview table's tenantGuid field.
CREATE INDEX `I_ConversationMessageLinkPreview_tenantGuid` ON `ConversationMessageLinkPreview` (`tenantGuid`);

-- Index on the ConversationMessageLinkPreview table's tenantGuid,conversationMessageId fields.
CREATE INDEX `I_ConversationMessageLinkPreview_tenantGuid_conversationMessageI` ON `ConversationMessageLinkPreview` (`tenantGuid`, `conversationMessageId`);

-- Index on the ConversationMessageLinkPreview table's tenantGuid,active fields.
CREATE INDEX `I_ConversationMessageLinkPreview_tenantGuid_active` ON `ConversationMessageLinkPreview` (`tenantGuid`, `active`);

-- Index on the ConversationMessageLinkPreview table's tenantGuid,deleted fields.
CREATE INDEX `I_ConversationMessageLinkPreview_tenantGuid_deleted` ON `ConversationMessageLinkPreview` (`tenantGuid`, `deleted`);

-- Index on the ConversationMessageLinkPreview table's id,active,deleted fields.
CREATE INDEX `I_ConversationMessageLinkPreview_id_active_deleted` ON `ConversationMessageLinkPreview` (`id`, `active`, `deleted`);

-- Index on the ConversationMessageLinkPreview table's conversationMessageId,active,deleted fields.
CREATE INDEX `I_ConversationMessageLinkPreview_conversationMessageId_active_de` ON `ConversationMessageLinkPreview` (`conversationMessageId`, `active`, `deleted`);


-- The change history for records from the ConversationMessageLinkPreview table.
CREATE TABLE `ConversationMessageLinkPreviewChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`conversationMessageLinkPreviewId` INT NOT NULL,		-- Link to the ConversationMessageLinkPreview table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`conversationMessageLinkPreviewId`) REFERENCES `ConversationMessageLinkPreview`(`id`)		-- Foreign key to the ConversationMessageLinkPreview table.
);
-- Index on the ConversationMessageLinkPreviewChangeHistory table's tenantGuid field.
CREATE INDEX `I_ConversationMessageLinkPreviewChangeHistory_tenantGuid` ON `ConversationMessageLinkPreviewChangeHistory` (`tenantGuid`);

-- Index on the ConversationMessageLinkPreviewChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ConversationMessageLinkPreviewChangeHistory_tenantGuid_version` ON `ConversationMessageLinkPreviewChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ConversationMessageLinkPreviewChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ConversationMessageLinkPreviewChangeHistory_tenantGuid_timeSta` ON `ConversationMessageLinkPreviewChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ConversationMessageLinkPreviewChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ConversationMessageLinkPreviewChangeHistory_tenantGuid_userId` ON `ConversationMessageLinkPreviewChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ConversationMessageLinkPreviewChangeHistory table's tenantGuid,conversationMessageLinkPreviewId fields.
CREATE INDEX `I_ConversationMessageLinkPreviewChangeHistory_tenantGuid_convers` ON `ConversationMessageLinkPreviewChangeHistory` (`tenantGuid`, `conversationMessageLinkPreviewId`, `versionNumber`, `timeStamp`, `userId`);


/*
This table tracks user online/offline status and activity for the messaging system.  It is part of the Foundation's Messaging system.

Presence records are updated when users connect to or disconnect from the MessagingHub.  The connectionCount field supports 
multi-device presence (a user connected from both a browser and a mobile app would have connectionCount = 2).  
When connectionCount drops to 0, the user is considered offline.
*/
CREATE TABLE `UserPresence`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`userId` INT NOT NULL,		-- The user whose presence is being tracked.  Resolved by IMessagingUserResolver.
	`status` VARCHAR(50) NOT NULL,		-- The current status: 'online', 'away', 'busy', 'offline', 'doNotDisturb'.
	`customStatusMessage` VARCHAR(250) NULL,		-- Optional custom status message, for example 'In a meeting until 3pm'.
	`lastSeenDateTime` DATETIME NOT NULL,		-- The last time this user was seen connected.
	`lastActivityDateTime` DATETIME NOT NULL,		-- The last time this user performed an action (sent a message, reacted, etc).
	`connectionCount` INT NOT NULL DEFAULT 0,		-- The number of active connections for this user.  Supports multi-device presence.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the UserPresence table's tenantGuid field.
CREATE INDEX `I_UserPresence_tenantGuid` ON `UserPresence` (`tenantGuid`);

-- Index on the UserPresence table's tenantGuid,active fields.
CREATE INDEX `I_UserPresence_tenantGuid_active` ON `UserPresence` (`tenantGuid`, `active`);

-- Index on the UserPresence table's tenantGuid,deleted fields.
CREATE INDEX `I_UserPresence_tenantGuid_deleted` ON `UserPresence` (`tenantGuid`, `deleted`);

-- Index on the UserPresence table's id,active,deleted fields.
CREATE INDEX `I_UserPresence_id_active_deleted` ON `UserPresence` (`id`, `active`, `deleted`);

-- Index on the UserPresence table's userId,active,deleted fields.
CREATE INDEX `I_UserPresence_userId_active_deleted` ON `UserPresence` (`userId`, `active`, `deleted`);


/*
This table tracks a user's last-read position within a message thread (reply chain).  It is part of the Foundation's Messaging system.

When a user views a thread panel, the client calls UpdateThreadReadPosition to update the lastReadMessageId.  
The unread reply count is then computed by counting messages in that thread with an ID greater than lastReadMessageId.
*/
CREATE TABLE `ConversationThreadUser`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`conversationId` INT NOT NULL,		-- The conversation that contains this thread.
	`parentConversationMessageId` INT NOT NULL,		-- The root message of the thread.
	`userId` INT NOT NULL,		-- The user being tracked.  Resolved by IMessagingUserResolver.
	`lastReadMessageId` INT NULL,		-- The last message in this thread that the user has read.
	`lastReadDateTime` DATETIME NULL,		-- When the user last read the thread.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`conversationId`) REFERENCES `Conversation`(`id`),		-- Foreign key to the Conversation table.
	FOREIGN KEY (`parentConversationMessageId`) REFERENCES `ConversationMessage`(`id`)		-- Foreign key to the ConversationMessage table.
);
-- Index on the ConversationThreadUser table's tenantGuid field.
CREATE INDEX `I_ConversationThreadUser_tenantGuid` ON `ConversationThreadUser` (`tenantGuid`);

-- Index on the ConversationThreadUser table's tenantGuid,conversationId fields.
CREATE INDEX `I_ConversationThreadUser_tenantGuid_conversationId` ON `ConversationThreadUser` (`tenantGuid`, `conversationId`);

-- Index on the ConversationThreadUser table's tenantGuid,parentConversationMessageId fields.
CREATE INDEX `I_ConversationThreadUser_tenantGuid_parentConversationMessageId` ON `ConversationThreadUser` (`tenantGuid`, `parentConversationMessageId`);

-- Index on the ConversationThreadUser table's tenantGuid,active fields.
CREATE INDEX `I_ConversationThreadUser_tenantGuid_active` ON `ConversationThreadUser` (`tenantGuid`, `active`);

-- Index on the ConversationThreadUser table's tenantGuid,deleted fields.
CREATE INDEX `I_ConversationThreadUser_tenantGuid_deleted` ON `ConversationThreadUser` (`tenantGuid`, `deleted`);

-- Index on the ConversationThreadUser table's id,active,deleted fields.
CREATE INDEX `I_ConversationThreadUser_id_active_deleted` ON `ConversationThreadUser` (`id`, `active`, `deleted`);

-- Index on the ConversationThreadUser table's userId,parentConversationMessageId fields.
CREATE INDEX `I_ConversationThreadUser_userId_parentConversationMessageId` ON `ConversationThreadUser` (`userId`, `parentConversationMessageId`);

-- Index on the ConversationThreadUser table's conversationId,active,deleted fields.
CREATE INDEX `I_ConversationThreadUser_conversationId_active_deleted` ON `ConversationThreadUser` (`conversationId`, `active`, `deleted`);


/*
This table stores personal message bookmarks.  It is part of the Foundation's Messaging system.

Users can bookmark any message for later reference and optionally add a personal note explaining why they saved it.  
Bookmarks are per-user and private — each user sees only their own bookmarks.
*/
CREATE TABLE `MessageBookmark`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`userId` INT NOT NULL,		-- The user who bookmarked the message.  Resolved by IMessagingUserResolver.
	`conversationMessageId` INT NOT NULL,		-- The bookmarked message.
	`note` VARCHAR(500) NULL,		-- Optional personal note about why the message was bookmarked.
	`dateTimeCreated` DATETIME NOT NULL,		-- When the bookmark was created.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`conversationMessageId`) REFERENCES `ConversationMessage`(`id`)		-- Foreign key to the ConversationMessage table.
);
-- Index on the MessageBookmark table's tenantGuid field.
CREATE INDEX `I_MessageBookmark_tenantGuid` ON `MessageBookmark` (`tenantGuid`);

-- Index on the MessageBookmark table's tenantGuid,conversationMessageId fields.
CREATE INDEX `I_MessageBookmark_tenantGuid_conversationMessageId` ON `MessageBookmark` (`tenantGuid`, `conversationMessageId`);

-- Index on the MessageBookmark table's tenantGuid,active fields.
CREATE INDEX `I_MessageBookmark_tenantGuid_active` ON `MessageBookmark` (`tenantGuid`, `active`);

-- Index on the MessageBookmark table's tenantGuid,deleted fields.
CREATE INDEX `I_MessageBookmark_tenantGuid_deleted` ON `MessageBookmark` (`tenantGuid`, `deleted`);

-- Index on the MessageBookmark table's id,active,deleted fields.
CREATE INDEX `I_MessageBookmark_id_active_deleted` ON `MessageBookmark` (`id`, `active`, `deleted`);

-- Index on the MessageBookmark table's userId,active,deleted fields.
CREATE INDEX `I_MessageBookmark_userId_active_deleted` ON `MessageBookmark` (`userId`, `active`, `deleted`);

-- Index on the MessageBookmark table's conversationMessageId,active,deleted fields.
CREATE INDEX `I_MessageBookmark_conversationMessageId_active_deleted` ON `MessageBookmark` (`conversationMessageId`, `active`, `deleted`);


/*
This table tracks external push delivery attempts for notifications and messages.  It is part of the Foundation's Messaging Platform system.

Each time a notification or message triggers an external delivery (email, SMS), a record is created here to track 
the delivery attempt, its success or failure, and any error messages.
*/
CREATE TABLE `PushDeliveryLog`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`userId` INT NOT NULL,		-- The target user for this delivery attempt.  Resolved by IMessagingUserResolver.
	`providerId` VARCHAR(50) NOT NULL,		-- The push provider that handled this delivery, for example 'smtp', 'sms', 'webhook'.
	`destination` VARCHAR(250) NULL,		-- The masked destination address (email or phone number).  Partially masked for privacy.
	`sourceType` VARCHAR(50) NULL,		-- The source type: 'notification' or 'message'.
	`sourceNotificationId` INT NULL,		-- The notification ID that triggered this delivery attempt, if applicable.
	`sourceConversationMessageId` INT NULL,		-- The conversation message ID that triggered this delivery attempt, if applicable.
	`success` BIT NOT NULL DEFAULT 0,		-- Whether the delivery attempt was successful.
	`externalId` VARCHAR(250) NULL,		-- The external ID returned by the provider (e.g., SMTP message ID, SMS provider ID).
	`errorMessage` VARCHAR(1000) NULL,		-- The error message if the delivery attempt failed.
	`attemptNumber` INT NOT NULL DEFAULT 1,		-- The attempt number for retry scenarios.
	`dateTimeCreated` DATETIME NOT NULL,		-- When this delivery attempt was made.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the PushDeliveryLog table's tenantGuid field.
CREATE INDEX `I_PushDeliveryLog_tenantGuid` ON `PushDeliveryLog` (`tenantGuid`);

-- Index on the PushDeliveryLog table's tenantGuid,active fields.
CREATE INDEX `I_PushDeliveryLog_tenantGuid_active` ON `PushDeliveryLog` (`tenantGuid`, `active`);

-- Index on the PushDeliveryLog table's tenantGuid,deleted fields.
CREATE INDEX `I_PushDeliveryLog_tenantGuid_deleted` ON `PushDeliveryLog` (`tenantGuid`, `deleted`);

-- Index on the PushDeliveryLog table's id,active,deleted fields.
CREATE INDEX `I_PushDeliveryLog_id_active_deleted` ON `PushDeliveryLog` (`id`, `active`, `deleted`);

-- Index on the PushDeliveryLog table's userId,active,deleted fields.
CREATE INDEX `I_PushDeliveryLog_userId_active_deleted` ON `PushDeliveryLog` (`userId`, `active`, `deleted`);

-- Index on the PushDeliveryLog table's providerId,active,deleted fields.
CREATE INDEX `I_PushDeliveryLog_providerId_active_deleted` ON `PushDeliveryLog` (`providerId`, `active`, `deleted`);

-- Index on the PushDeliveryLog table's success,active,deleted fields.
CREATE INDEX `I_PushDeliveryLog_success_active_deleted` ON `PushDeliveryLog` (`success`, `active`, `deleted`);


/*
This table stores per-tenant push provider configuration.  It is part of the Foundation's Messaging Platform system.

Each tenant can enable/disable individual push providers (SMTP, SMS, webhooks) and store provider-specific 
configuration as JSON.  This allows tenant administrators to control which external delivery channels are active.
*/
CREATE TABLE `PushProviderConfiguration`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`providerId` VARCHAR(50) NOT NULL,		-- The push provider identifier, for example 'smtp', 'sms', 'webhook'.
	`enabled` BIT NOT NULL DEFAULT 0,		-- Whether this provider is enabled for this tenant.
	`configurationJson` TEXT NULL,		-- Provider-specific configuration stored as JSON.  Structure varies by provider type.
	`dateTimeModified` DATETIME NOT NULL,		-- When this configuration was last modified.
	`modifiedByUserId` INT NOT NULL,		-- The user who last modified this configuration.  Resolved by IMessagingUserResolver.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the PushProviderConfiguration table's tenantGuid field.
CREATE INDEX `I_PushProviderConfiguration_tenantGuid` ON `PushProviderConfiguration` (`tenantGuid`);

-- Index on the PushProviderConfiguration table's tenantGuid,active fields.
CREATE INDEX `I_PushProviderConfiguration_tenantGuid_active` ON `PushProviderConfiguration` (`tenantGuid`, `active`);

-- Index on the PushProviderConfiguration table's tenantGuid,deleted fields.
CREATE INDEX `I_PushProviderConfiguration_tenantGuid_deleted` ON `PushProviderConfiguration` (`tenantGuid`, `deleted`);

-- Index on the PushProviderConfiguration table's id,active,deleted fields.
CREATE INDEX `I_PushProviderConfiguration_id_active_deleted` ON `PushProviderConfiguration` (`id`, `active`, `deleted`);

-- Index on the PushProviderConfiguration table's providerId,active,deleted fields.
CREATE INDEX `I_PushProviderConfiguration_providerId_active_deleted` ON `PushProviderConfiguration` (`providerId`, `active`, `deleted`);


/*
This table tracks message flags/reports for administrative review.  It is part of the Foundation's Messaging Platform system.

Users can flag messages that they believe violate policies or require administrative attention.  Each flag tracks 
the reason, the review status (open/reviewed/resolved/dismissed), and any resolution notes from the reviewer.
*/
CREATE TABLE `MessageFlag`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`conversationMessageId` INT NOT NULL,		-- The message that was flagged.
	`flaggedByUserId` INT NOT NULL,		-- The user who flagged the message.  Resolved by IMessagingUserResolver.
	`reason` VARCHAR(100) NOT NULL,		-- The reason for flagging: 'abuse', 'spam', 'harassment', 'inappropriate', 'other'.
	`details` VARCHAR(1000) NULL,		-- Additional details provided by the reporting user.
	`status` VARCHAR(50) NOT NULL,		-- The review status: 'open', 'reviewed', 'resolved', 'dismissed'.
	`reviewedByUserId` INT NULL,		-- The admin user who reviewed the flag.  Resolved by IMessagingUserResolver.
	`dateTimeReviewed` DATETIME NULL,		-- When the flag was reviewed.
	`resolutionNotes` VARCHAR(1000) NULL,		-- Notes from the reviewer about the resolution.
	`dateTimeCreated` DATETIME NOT NULL,		-- When the flag was created.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`conversationMessageId`) REFERENCES `ConversationMessage`(`id`)		-- Foreign key to the ConversationMessage table.
);
-- Index on the MessageFlag table's tenantGuid field.
CREATE INDEX `I_MessageFlag_tenantGuid` ON `MessageFlag` (`tenantGuid`);

-- Index on the MessageFlag table's tenantGuid,conversationMessageId fields.
CREATE INDEX `I_MessageFlag_tenantGuid_conversationMessageId` ON `MessageFlag` (`tenantGuid`, `conversationMessageId`);

-- Index on the MessageFlag table's tenantGuid,active fields.
CREATE INDEX `I_MessageFlag_tenantGuid_active` ON `MessageFlag` (`tenantGuid`, `active`);

-- Index on the MessageFlag table's tenantGuid,deleted fields.
CREATE INDEX `I_MessageFlag_tenantGuid_deleted` ON `MessageFlag` (`tenantGuid`, `deleted`);

-- Index on the MessageFlag table's id,active,deleted fields.
CREATE INDEX `I_MessageFlag_id_active_deleted` ON `MessageFlag` (`id`, `active`, `deleted`);

-- Index on the MessageFlag table's status,active,deleted fields.
CREATE INDEX `I_MessageFlag_status_active_deleted` ON `MessageFlag` (`status`, `active`, `deleted`);

-- Index on the MessageFlag table's conversationMessageId,active,deleted fields.
CREATE INDEX `I_MessageFlag_conversationMessageId_active_deleted` ON `MessageFlag` (`conversationMessageId`, `active`, `deleted`);

-- Index on the MessageFlag table's flaggedByUserId,active,deleted fields.
CREATE INDEX `I_MessageFlag_flaggedByUserId_active_deleted` ON `MessageFlag` (`flaggedByUserId`, `active`, `deleted`);


/*
This table records administrative actions within the messaging system.  It is part of the Foundation's Messaging Platform system.

All administrative actions (flag resolutions, message deletions, user bans, configuration changes) are logged here 
for accountability and compliance.  Each entry records who performed the action, what was affected, and when.
*/
CREATE TABLE `MessagingAuditLog`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`performedByUserId` INT NOT NULL,		-- The admin user who performed the action.  Resolved by IMessagingUserResolver.
	`action` VARCHAR(100) NOT NULL,		-- The action performed: 'ResolveFlag', 'DeleteMessage', 'BanUser', 'ConfigChange', etc.
	`entityType` VARCHAR(100) NULL,		-- The type of entity affected: 'MessageFlag', 'ConversationMessage', 'User', etc.
	`entityId` INT NULL,		-- The ID of the entity that was affected.
	`details` TEXT NULL,		-- JSON or descriptive details about the action taken.
	`ipAddress` VARCHAR(50) NULL,		-- The IP address of the admin when the action was performed.
	`dateTimeCreated` DATETIME NOT NULL,		-- When the action was performed.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the MessagingAuditLog table's tenantGuid field.
CREATE INDEX `I_MessagingAuditLog_tenantGuid` ON `MessagingAuditLog` (`tenantGuid`);

-- Index on the MessagingAuditLog table's tenantGuid,active fields.
CREATE INDEX `I_MessagingAuditLog_tenantGuid_active` ON `MessagingAuditLog` (`tenantGuid`, `active`);

-- Index on the MessagingAuditLog table's tenantGuid,deleted fields.
CREATE INDEX `I_MessagingAuditLog_tenantGuid_deleted` ON `MessagingAuditLog` (`tenantGuid`, `deleted`);

-- Index on the MessagingAuditLog table's id,active,deleted fields.
CREATE INDEX `I_MessagingAuditLog_id_active_deleted` ON `MessagingAuditLog` (`id`, `active`, `deleted`);

-- Index on the MessagingAuditLog table's performedByUserId,active,deleted fields.
CREATE INDEX `I_MessagingAuditLog_performedByUserId_active_deleted` ON `MessagingAuditLog` (`performedByUserId`, `active`, `deleted`);

-- Index on the MessagingAuditLog table's action,active,deleted fields.
CREATE INDEX `I_MessagingAuditLog_action_active_deleted` ON `MessagingAuditLog` (`action`, `active`, `deleted`);


/*
This table defines the types of calls that can be made.  It is part of the Foundation's Calling system.

Call types include voice, video, and screen share.
*/
CREATE TABLE `CallType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NULL,
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the CallType table's name field.
CREATE INDEX `I_CallType_name` ON `CallType` (`name`);

-- Index on the CallType table's active field.
CREATE INDEX `I_CallType_active` ON `CallType` (`active`);

-- Index on the CallType table's deleted field.
CREATE INDEX `I_CallType_deleted` ON `CallType` (`deleted`);

-- Index on the CallType table's id,active,deleted fields.
CREATE INDEX `I_CallType_id_active_deleted` ON `CallType` (`id`, `active`, `deleted`);

INSERT INTO `CallType` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Voice', 'Voice call', 'b1c3d4e5-f6a7-4890-b123-456789abcde0' );

INSERT INTO `CallType` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Video', 'Video call', 'c2d4e5f6-a7b8-4901-c234-56789abcdef1' );

INSERT INTO `CallType` ( `name`, `description`, `objectGuid` ) VALUES  ( 'ScreenShare', 'Screen sharing session', 'd3e5f6a7-b8c9-4012-d345-6789abcdef02' );


/*
This table defines the possible statuses for a call.  It is part of the Foundation's Calling system.

Call statuses track the lifecycle of a call from initiation through to completion.
*/
CREATE TABLE `CallStatus`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NULL,
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the CallStatus table's name field.
CREATE INDEX `I_CallStatus_name` ON `CallStatus` (`name`);

-- Index on the CallStatus table's active field.
CREATE INDEX `I_CallStatus_active` ON `CallStatus` (`active`);

-- Index on the CallStatus table's deleted field.
CREATE INDEX `I_CallStatus_deleted` ON `CallStatus` (`deleted`);

-- Index on the CallStatus table's id,active,deleted fields.
CREATE INDEX `I_CallStatus_id_active_deleted` ON `CallStatus` (`id`, `active`, `deleted`);

INSERT INTO `CallStatus` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Ringing', 'The call has been initiated and is ringing', 'e4f6a7b8-c9d0-4123-e456-789abcdef034' );

INSERT INTO `CallStatus` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Active', 'The call is currently active', 'f5a7b8c9-d0e1-4234-f567-89abcdef0145' );

INSERT INTO `CallStatus` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Ended', 'The call ended normally', 'a6b8c9d0-e1f2-4345-a678-9abcdef01256' );

INSERT INTO `CallStatus` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Missed', 'The call was not answered', 'b7c9d0e1-f2a3-4456-b789-abcdef012367' );

INSERT INTO `CallStatus` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Declined', 'The call was declined by the recipient', 'c8d0e1f2-a3b4-4567-c890-bcdef0123478' );

INSERT INTO `CallStatus` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Failed', 'The call failed due to a technical error', 'd9e1f2a3-b4c5-4678-d901-cdef01234589' );


/*
This is the main Call table.  It records voice, video, and screen share calls.  It is part of the Foundation's Calling system.

Each call is linked to a conversation and tracks the call provider used, timing information, and overall call state.  
The providerId field identifies which ICallProvider implementation handled the call (e.g. 'webrtc', 'azure-acs').
*/
CREATE TABLE `Call`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`callTypeId` INT NOT NULL,		-- The type of call: voice, video, or screen share.
	`callStatusId` INT NOT NULL,		-- The current status of the call.
	`providerId` VARCHAR(50) NOT NULL,		-- The call provider that is handling this call, for example 'webrtc', 'azure-acs'.
	`providerCallId` VARCHAR(250) NULL,		-- The provider-specific call identifier.  Used for correlation with external systems like Azure Communication Services.
	`conversationId` INT NOT NULL,		-- The conversation that this call belongs to.
	`initiatorUserId` INT NOT NULL,		-- The user who initiated the call.  Resolved by IMessagingUserResolver.
	`startDateTime` DATETIME NOT NULL,		-- When the call was initiated.
	`answerDateTime` DATETIME NULL,		-- When the call was answered.  Null if the call was never answered.
	`endDateTime` DATETIME NULL,		-- When the call ended.  Null if the call is still active or was never connected.
	`durationSeconds` INT NULL,		-- The duration of the call in seconds.  Calculated from answerDateTime to endDateTime.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`callTypeId`) REFERENCES `CallType`(`id`),		-- Foreign key to the CallType table.
	FOREIGN KEY (`callStatusId`) REFERENCES `CallStatus`(`id`),		-- Foreign key to the CallStatus table.
	FOREIGN KEY (`conversationId`) REFERENCES `Conversation`(`id`)		-- Foreign key to the Conversation table.
);
-- Index on the Call table's tenantGuid field.
CREATE INDEX `I_Call_tenantGuid` ON `Call` (`tenantGuid`);

-- Index on the Call table's tenantGuid,callTypeId fields.
CREATE INDEX `I_Call_tenantGuid_callTypeId` ON `Call` (`tenantGuid`, `callTypeId`);

-- Index on the Call table's tenantGuid,callStatusId fields.
CREATE INDEX `I_Call_tenantGuid_callStatusId` ON `Call` (`tenantGuid`, `callStatusId`);

-- Index on the Call table's tenantGuid,conversationId fields.
CREATE INDEX `I_Call_tenantGuid_conversationId` ON `Call` (`tenantGuid`, `conversationId`);

-- Index on the Call table's tenantGuid,active fields.
CREATE INDEX `I_Call_tenantGuid_active` ON `Call` (`tenantGuid`, `active`);

-- Index on the Call table's tenantGuid,deleted fields.
CREATE INDEX `I_Call_tenantGuid_deleted` ON `Call` (`tenantGuid`, `deleted`);

-- Index on the Call table's id,active,deleted fields.
CREATE INDEX `I_Call_id_active_deleted` ON `Call` (`id`, `active`, `deleted`);

-- Index on the Call table's conversationId,active,deleted fields.
CREATE INDEX `I_Call_conversationId_active_deleted` ON `Call` (`conversationId`, `active`, `deleted`);

-- Index on the Call table's initiatorUserId,active,deleted fields.
CREATE INDEX `I_Call_initiatorUserId_active_deleted` ON `Call` (`initiatorUserId`, `active`, `deleted`);

-- Index on the Call table's callStatusId,active,deleted fields.
CREATE INDEX `I_Call_callStatusId_active_deleted` ON `Call` (`callStatusId`, `active`, `deleted`);

-- Index on the Call table's providerId,active,deleted fields.
CREATE INDEX `I_Call_providerId_active_deleted` ON `Call` (`providerId`, `active`, `deleted`);


/*
This table tracks individual user participation in calls.  It is part of the Foundation's Calling system.

Each participant record tracks the user's role (initiator or recipient), their participation status (ringing, joined, 
declined, missed), and when they joined and left the call.  Supports multi-party calls.
*/
CREATE TABLE `CallParticipant`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`callId` INT NOT NULL,		-- The call that this participant belongs to.
	`userId` INT NOT NULL,		-- The user participating in the call.  Resolved by IMessagingUserResolver.
	`role` VARCHAR(50) NOT NULL,		-- The participant's role: 'initiator' or 'recipient'.
	`status` VARCHAR(50) NOT NULL,		-- The participant's status: 'ringing', 'joined', 'declined', 'missed', 'left'.
	`joinedDateTime` DATETIME NULL,		-- When the participant joined the call.  Null if they never joined.
	`leftDateTime` DATETIME NULL,		-- When the participant left the call.  Null if they are still in the call or never joined.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`callId`) REFERENCES `Call`(`id`)		-- Foreign key to the Call table.
);
-- Index on the CallParticipant table's tenantGuid field.
CREATE INDEX `I_CallParticipant_tenantGuid` ON `CallParticipant` (`tenantGuid`);

-- Index on the CallParticipant table's tenantGuid,callId fields.
CREATE INDEX `I_CallParticipant_tenantGuid_callId` ON `CallParticipant` (`tenantGuid`, `callId`);

-- Index on the CallParticipant table's tenantGuid,active fields.
CREATE INDEX `I_CallParticipant_tenantGuid_active` ON `CallParticipant` (`tenantGuid`, `active`);

-- Index on the CallParticipant table's tenantGuid,deleted fields.
CREATE INDEX `I_CallParticipant_tenantGuid_deleted` ON `CallParticipant` (`tenantGuid`, `deleted`);

-- Index on the CallParticipant table's id,active,deleted fields.
CREATE INDEX `I_CallParticipant_id_active_deleted` ON `CallParticipant` (`id`, `active`, `deleted`);

-- Index on the CallParticipant table's callId,active,deleted fields.
CREATE INDEX `I_CallParticipant_callId_active_deleted` ON `CallParticipant` (`callId`, `active`, `deleted`);

-- Index on the CallParticipant table's userId,active,deleted fields.
CREATE INDEX `I_CallParticipant_userId_active_deleted` ON `CallParticipant` (`userId`, `active`, `deleted`);

-- Index on the CallParticipant table's status,active,deleted fields.
CREATE INDEX `I_CallParticipant_status_active_deleted` ON `CallParticipant` (`status`, `active`, `deleted`);


/*
This table records significant events during the lifecycle of a call.  It is part of the Foundation's Calling system.

Each event records what happened (initiated, joined, left, ended, failed), who performed the action, which provider 
was involved, and any additional metadata as JSON.  Used for debugging, analytics, and compliance.
*/
CREATE TABLE `CallEventLog`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`callId` INT NOT NULL,		-- The call that this event belongs to.
	`eventType` VARCHAR(100) NOT NULL,		-- The type of event: 'initiated', 'ringing', 'joined', 'left', 'ended', 'failed', 'declined', 'missed', 'ice_connected', 'ice_failed', 'media_started', 'media_stopped'.
	`userId` INT NULL,		-- The user associated with this event.  Resolved by IMessagingUserResolver.  Nullable for system-level events.
	`providerId` VARCHAR(50) NULL,		-- The call provider associated with this event.
	`metadata` TEXT NULL,		-- Additional event metadata stored as JSON.  Structure varies by event type.
	`dateTimeCreated` DATETIME NOT NULL,		-- When the event occurred.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`callId`) REFERENCES `Call`(`id`)		-- Foreign key to the Call table.
);
-- Index on the CallEventLog table's tenantGuid field.
CREATE INDEX `I_CallEventLog_tenantGuid` ON `CallEventLog` (`tenantGuid`);

-- Index on the CallEventLog table's tenantGuid,callId fields.
CREATE INDEX `I_CallEventLog_tenantGuid_callId` ON `CallEventLog` (`tenantGuid`, `callId`);

-- Index on the CallEventLog table's tenantGuid,active fields.
CREATE INDEX `I_CallEventLog_tenantGuid_active` ON `CallEventLog` (`tenantGuid`, `active`);

-- Index on the CallEventLog table's tenantGuid,deleted fields.
CREATE INDEX `I_CallEventLog_tenantGuid_deleted` ON `CallEventLog` (`tenantGuid`, `deleted`);

-- Index on the CallEventLog table's id,active,deleted fields.
CREATE INDEX `I_CallEventLog_id_active_deleted` ON `CallEventLog` (`id`, `active`, `deleted`);

-- Index on the CallEventLog table's callId,active,deleted fields.
CREATE INDEX `I_CallEventLog_callId_active_deleted` ON `CallEventLog` (`callId`, `active`, `deleted`);

-- Index on the CallEventLog table's eventType,active,deleted fields.
CREATE INDEX `I_CallEventLog_eventType_active_deleted` ON `CallEventLog` (`eventType`, `active`, `deleted`);

-- Index on the CallEventLog table's userId,active,deleted fields.
CREATE INDEX `I_CallEventLog_userId_active_deleted` ON `CallEventLog` (`userId`, `active`, `deleted`);


-- Master list of available attribute data types.
CREATE TABLE `AttributeDefinitionType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the AttributeDefinitionType table's name field.
CREATE INDEX `I_AttributeDefinitionType_name` ON `AttributeDefinitionType` (`name`);

-- Index on the AttributeDefinitionType table's active field.
CREATE INDEX `I_AttributeDefinitionType_active` ON `AttributeDefinitionType` (`active`);

-- Index on the AttributeDefinitionType table's deleted field.
CREATE INDEX `I_AttributeDefinitionType_deleted` ON `AttributeDefinitionType` (`deleted`);

INSERT INTO `AttributeDefinitionType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Text', 'Single line text', 1, 'd1a1b2c3-1111-2222-3333-444455556661' );

INSERT INTO `AttributeDefinitionType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Number', 'Numeric value', 2, 'd1a1b2c3-1111-2222-3333-444455556662' );

INSERT INTO `AttributeDefinitionType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Date', 'Date value (no time)', 3, 'd1a1b2c3-1111-2222-3333-444455556663' );

INSERT INTO `AttributeDefinitionType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Boolean', 'True/False checkbox', 4, 'd1a1b2c3-1111-2222-3333-444455556664' );

INSERT INTO `AttributeDefinitionType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Select', 'Dropdown selection', 5, 'd1a1b2c3-1111-2222-3333-444455556665' );


-- Master list of entities that support custom attributes.
CREATE TABLE `AttributeDefinitionEntity`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the AttributeDefinitionEntity table's name field.
CREATE INDEX `I_AttributeDefinitionEntity_name` ON `AttributeDefinitionEntity` (`name`);

-- Index on the AttributeDefinitionEntity table's active field.
CREATE INDEX `I_AttributeDefinitionEntity_active` ON `AttributeDefinitionEntity` (`active`);

-- Index on the AttributeDefinitionEntity table's deleted field.
CREATE INDEX `I_AttributeDefinitionEntity_deleted` ON `AttributeDefinitionEntity` (`deleted`);

INSERT INTO `AttributeDefinitionEntity` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Contact', 'Contact Records', 'e2a1b2c3-1111-2222-3333-444455556661' );

INSERT INTO `AttributeDefinitionEntity` ( `name`, `description`, `objectGuid` ) VALUES  ( 'Constituent', 'Constituent Records', 'e2a1b2c3-1111-2222-3333-444455556662' );


-- Definitions for custom attributes on various entities (Contact, Constituent, etc.)
CREATE TABLE `AttributeDefinition`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`attributeDefinitionEntityId` INT NULL,		-- The entity this attribute applies to (e.g., Contact)
	`key` VARCHAR(100) NULL,		-- The JSON key for the attribute
	`label` VARCHAR(250) NULL,		-- The human-readable label for the attribute
	`attributeDefinitionTypeId` INT NULL,		-- Data type: Text, Number, Date, etc.
	`options` TEXT NULL,		-- JSON options for Select/MultiSelect types
	`isRequired` BIT NOT NULL DEFAULT 0,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`attributeDefinitionEntityId`) REFERENCES `AttributeDefinitionEntity`(`id`),		-- Foreign key to the AttributeDefinitionEntity table.
	FOREIGN KEY (`attributeDefinitionTypeId`) REFERENCES `AttributeDefinitionType`(`id`),		-- Foreign key to the AttributeDefinitionType table.
	UNIQUE `UC_AttributeDefinition_tenantGuid_attributeDefinitionEntityId_key_Unique`( `tenantGuid`, `attributeDefinitionEntityId`, `key` ) 		-- Uniqueness enforced on the AttributeDefinition table's tenantGuid and attributeDefinitionEntityId and key fields.
);
-- Index on the AttributeDefinition table's tenantGuid field.
CREATE INDEX `I_AttributeDefinition_tenantGuid` ON `AttributeDefinition` (`tenantGuid`);

-- Index on the AttributeDefinition table's tenantGuid,attributeDefinitionEntityId fields.
CREATE INDEX `I_AttributeDefinition_tenantGuid_attributeDefinitionEntityId` ON `AttributeDefinition` (`tenantGuid`, `attributeDefinitionEntityId`);

-- Index on the AttributeDefinition table's tenantGuid,attributeDefinitionTypeId fields.
CREATE INDEX `I_AttributeDefinition_tenantGuid_attributeDefinitionTypeId` ON `AttributeDefinition` (`tenantGuid`, `attributeDefinitionTypeId`);

-- Index on the AttributeDefinition table's tenantGuid,active fields.
CREATE INDEX `I_AttributeDefinition_tenantGuid_active` ON `AttributeDefinition` (`tenantGuid`, `active`);

-- Index on the AttributeDefinition table's tenantGuid,deleted fields.
CREATE INDEX `I_AttributeDefinition_tenantGuid_deleted` ON `AttributeDefinition` (`tenantGuid`, `deleted`);


-- The change history for records from the AttributeDefinition table.
CREATE TABLE `AttributeDefinitionChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`attributeDefinitionId` INT NOT NULL,		-- Link to the AttributeDefinition table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`attributeDefinitionId`) REFERENCES `AttributeDefinition`(`id`)		-- Foreign key to the AttributeDefinition table.
);
-- Index on the AttributeDefinitionChangeHistory table's tenantGuid field.
CREATE INDEX `I_AttributeDefinitionChangeHistory_tenantGuid` ON `AttributeDefinitionChangeHistory` (`tenantGuid`);

-- Index on the AttributeDefinitionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_AttributeDefinitionChangeHistory_tenantGuid_versionNumber` ON `AttributeDefinitionChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the AttributeDefinitionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_AttributeDefinitionChangeHistory_tenantGuid_timeStamp` ON `AttributeDefinitionChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the AttributeDefinitionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_AttributeDefinitionChangeHistory_tenantGuid_userId` ON `AttributeDefinitionChangeHistory` (`tenantGuid`, `userId`);

-- Index on the AttributeDefinitionChangeHistory table's tenantGuid,attributeDefinitionId fields.
CREATE INDEX `I_AttributeDefinitionChangeHistory_tenantGuid_attributeDefinitio` ON `AttributeDefinitionChangeHistory` (`tenantGuid`, `attributeDefinitionId`, `versionNumber`, `timeStamp`, `userId`);


-- List of icons to use on user interfaces.  Not tenant editable.
CREATE TABLE `Icon`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`fontAwesomeCode` VARCHAR(50) NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the Icon table's name field.
CREATE INDEX `I_Icon_name` ON `Icon` (`name`);

-- Index on the Icon table's active field.
CREATE INDEX `I_Icon_active` ON `Icon` (`active`);

-- Index on the Icon table's deleted field.
CREATE INDEX `I_Icon_deleted` ON `Icon` (`deleted`);

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Person', 'fa-solid fa-user', 1, 'a1b2c3d4-5678-9012-3456-789abcde0001' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'People', 'fa-solid fa-users', 2, 'a1b2c3d4-5678-9012-3456-789abcde0002' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Supervisor', 'fa-solid fa-user-tie', 3, 'a1b2c3d4-5678-9012-3456-789abcde0003' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Operator', 'fa-solid fa-hard-hat', 4, 'a1b2c3d4-5678-9012-3456-789abcde0004' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Equipment', 'fa-solid fa-truck', 10, 'a1b2c3d4-5678-9012-3456-789abcde0010' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Roller', 'fa-solid fa-road', 11, 'a1b2c3d4-5678-9012-3456-789abcde0011' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Crane', 'fa-solid fa-tower-broadcast', 12, 'a1b2c3d4-5678-9012-3456-789abcde0012' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Vehicle', 'fa-solid fa-truck-pickup', 13, 'a1b2c3d4-5678-9012-3456-789abcde0013' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Tool', 'fa-solid fa-toolbox', 14, 'a1b2c3d4-5678-9012-3456-789abcde0014' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Room', 'fa-solid fa-door-open', 15, 'a1b2c3d4-5678-9012-3456-789abcde0015' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Project', 'fa-solid fa-briefcase', 20, 'a1b2c3d4-5678-9012-3456-789abcde0020' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Construction Site', 'fa-solid fa-helmet-safety', 21, 'a1b2c3d4-5678-9012-3456-789abcde0021' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Patient', 'fa-solid fa-bed-pulse', 22, 'a1b2c3d4-5678-9012-3456-789abcde0022' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Home', 'fa-solid fa-house-medical', 23, 'a1b2c3d4-5678-9012-3456-789abcde0023' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Calendar', 'fa-solid fa-calendar-days', 30, 'a1b2c3d4-5678-9012-3456-789abcde0030' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Maintenance', 'fa-solid fa-wrench', 31, 'a1b2c3d4-5678-9012-3456-789abcde0031' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Event', 'fa-solid fa-calendar-check', 32, 'a1b2c3d4-5678-9012-3456-789abcde0032' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'High Priority', 'fa-solid fa-triangle-exclamation', 40, 'a1b2c3d4-5678-9012-3456-789abcde0040' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Medium Priority', 'fa-solid fa-circle-exclamation', 41, 'a1b2c3d4-5678-9012-3456-789abcde0041' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Low Priority', 'fa-solid fa-circle-info', 42, 'a1b2c3d4-5678-9012-3456-789abcde0042' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Assignment', 'fa-solid fa-user-check', 50, 'a1b2c3d4-5678-9012-3456-789abcde0050' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Crew', 'fa-solid fa-users-gear', 51, 'a1b2c3d4-5678-9012-3456-789abcde0051' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Qualification', 'fa-solid fa-certificate', 52, 'a1b2c3d4-5678-9012-3456-789abcde0052' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Travel', 'fa-solid fa-car', 53, 'a1b2c3d4-5678-9012-3456-789abcde0053' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Location', 'fa-solid fa-location-dot', 54, 'a1b2c3d4-5678-9012-3456-789abcde0054' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Notification', 'fa-solid fa-bell', 55, 'a1b2c3d4-5678-9012-3456-789abcde0055' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Hammer', 'fa-solid fa-hammer', 100, 'a1b2c3d4-5678-9012-3456-789abcde0100' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Wrench', 'fa-solid fa-wrench', 101, 'a1b2c3d4-5678-9012-3456-789abcde0101' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Screwdriver', 'fa-solid fa-screwdriver-wrench', 102, 'a1b2c3d4-5678-9012-3456-789abcde0102' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Paint Roller', 'fa-solid fa-paint-roller', 103, 'a1b2c3d4-5678-9012-3456-789abcde0103' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Brush', 'fa-solid fa-brush', 104, 'a1b2c3d4-5678-9012-3456-789abcde0104' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Ruler / Measurements', 'fa-solid fa-ruler-combined', 105, 'a1b2c3d4-5678-9012-3456-789abcde0105' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Drafting / Architecture', 'fa-solid fa-compass-drafting', 106, 'a1b2c3d4-5678-9012-3456-789abcde0106' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Electricity / Power', 'fa-solid fa-bolt', 107, 'a1b2c3d4-5678-9012-3456-789abcde0107' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Water / Plumbing', 'fa-solid fa-faucet-drip', 108, 'a1b2c3d4-5678-9012-3456-789abcde0108' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Trowel / Masonry', 'fa-solid fa-trowel', 109, 'a1b2c3d4-5678-9012-3456-789abcde0109' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Bucket', 'fa-solid fa-bucket', 110, 'a1b2c3d4-5678-9012-3456-789abcde0110' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Doctor', 'fa-solid fa-user-doctor', 200, 'a1b2c3d4-5678-9012-3456-789abcde0200' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Nurse', 'fa-solid fa-user-nurse', 201, 'a1b2c3d4-5678-9012-3456-789abcde0201' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Stethoscope', 'fa-solid fa-stethoscope', 202, 'a1b2c3d4-5678-9012-3456-789abcde0202' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Syringe / Vaccine', 'fa-solid fa-syringe', 203, 'a1b2c3d4-5678-9012-3456-789abcde0203' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'First Aid', 'fa-solid fa-kit-medical', 204, 'a1b2c3d4-5678-9012-3456-789abcde0204' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Pills / Medication', 'fa-solid fa-pills', 205, 'a1b2c3d4-5678-9012-3456-789abcde0205' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Hospital', 'fa-solid fa-hospital', 206, 'a1b2c3d4-5678-9012-3456-789abcde0206' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Wheelchair / Accessibility', 'fa-solid fa-wheelchair', 207, 'a1b2c3d4-5678-9012-3456-789abcde0207' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Heart / Vitals', 'fa-solid fa-heart-pulse', 208, 'a1b2c3d4-5678-9012-3456-789abcde0208' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Box / Package', 'fa-solid fa-box', 300, 'a1b2c3d4-5678-9012-3456-789abcde0300' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Pallet', 'fa-solid fa-pallet', 301, 'a1b2c3d4-5678-9012-3456-789abcde0301' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Warehouse', 'fa-solid fa-warehouse', 302, 'a1b2c3d4-5678-9012-3456-789abcde0302' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Map Pin', 'fa-solid fa-map-pin', 303, 'a1b2c3d4-5678-9012-3456-789abcde0303' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Route', 'fa-solid fa-route', 304, 'a1b2c3d4-5678-9012-3456-789abcde0304' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Ship / Marine', 'fa-solid fa-ship', 305, 'a1b2c3d4-5678-9012-3456-789abcde0305' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Plane / Air', 'fa-solid fa-plane', 306, 'a1b2c3d4-5678-9012-3456-789abcde0306' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Building / Office', 'fa-solid fa-building', 400, 'a1b2c3d4-5678-9012-3456-789abcde0400' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Money / Finance', 'fa-solid fa-money-bill-wave', 401, 'a1b2c3d4-5678-9012-3456-789abcde0401' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Credit Card', 'fa-solid fa-credit-card', 402, 'a1b2c3d4-5678-9012-3456-789abcde0402' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Contract / Document', 'fa-solid fa-file-contract', 403, 'a1b2c3d4-5678-9012-3456-789abcde0403' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Signature', 'fa-solid fa-file-signature', 404, 'a1b2c3d4-5678-9012-3456-789abcde0404' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Clipboard / Checklist', 'fa-solid fa-clipboard-list', 405, 'a1b2c3d4-5678-9012-3456-789abcde0405' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Chart / Analytics', 'fa-solid fa-chart-line', 406, 'a1b2c3d4-5678-9012-3456-789abcde0406' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Phone', 'fa-solid fa-phone', 500, 'a1b2c3d4-5678-9012-3456-789abcde0500' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Laptop', 'fa-solid fa-laptop', 501, 'a1b2c3d4-5678-9012-3456-789abcde0501' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Server / Database', 'fa-solid fa-server', 502, 'a1b2c3d4-5678-9012-3456-789abcde0502' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Wifi', 'fa-solid fa-wifi', 503, 'a1b2c3d4-5678-9012-3456-789abcde0503' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Check / Success', 'fa-solid fa-check', 600, 'a1b2c3d4-5678-9012-3456-789abcde0600' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'X / Cancel', 'fa-solid fa-xmark', 601, 'a1b2c3d4-5678-9012-3456-789abcde0601' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Ban / Blocked', 'fa-solid fa-ban', 602, 'a1b2c3d4-5678-9012-3456-789abcde0602' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Clock / Time', 'fa-solid fa-clock', 603, 'a1b2c3d4-5678-9012-3456-789abcde0603' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Hourglass / Waiting', 'fa-solid fa-hourglass-half', 604, 'a1b2c3d4-5678-9012-3456-789abcde0604' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Lock / Security', 'fa-solid fa-lock', 605, 'a1b2c3d4-5678-9012-3456-789abcde0605' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Trash / Delete', 'fa-solid fa-trash', 606, 'a1b2c3d4-5678-9012-3456-789abcde0606' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Sun / Day', 'fa-solid fa-sun', 700, 'a1b2c3d4-5678-9012-3456-789abcde0700' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Cloud', 'fa-solid fa-cloud', 701, 'a1b2c3d4-5678-9012-3456-789abcde0701' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Tree / Landscape', 'fa-solid fa-tree', 702, 'a1b2c3d4-5678-9012-3456-789abcde0702' );

INSERT INTO `Icon` ( `name`, `fontAwesomeCode`, `sequence`, `objectGuid` ) VALUES  ( 'Default', 'fa-solid fa-circle', 999, 'a1b2c3d4-5678-9012-3456-789abcde0999' );


-- The master list of salutations
CREATE TABLE `Salutation`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the Salutation table's name field.
CREATE INDEX `I_Salutation_name` ON `Salutation` (`name`);

-- Index on the Salutation table's active field.
CREATE INDEX `I_Salutation_active` ON `Salutation` (`active`);

-- Index on the Salutation table's deleted field.
CREATE INDEX `I_Salutation_deleted` ON `Salutation` (`deleted`);

INSERT INTO `Salutation` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Mr.', 'Mister', 1, '0e2c9a70-3a90-49f7-9f0a-539fb232a667' );

INSERT INTO `Salutation` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Mrs.', 'Mrs.', 2, '738abc0a-c637-4d45-89a1-4efb5da4e1d6' );

INSERT INTO `Salutation` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Ms.', 'Ms.', 3, 'e4f9cfe6-c9dc-44a4-8977-67a8e90f94f8' );

INSERT INTO `Salutation` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Dr.', 'Doctor', 4, '67be6b22-591f-4b7c-8366-bc3e7304ec90' );

INSERT INTO `Salutation` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Prof.', 'Professor', 5, '8334e778-b326-4313-8891-c84cf9067d4f' );

INSERT INTO `Salutation` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Rev.', 'Reverend', 6, 'f27ca1ef-1d00-4d03-9ccd-79a2f97cb2e6' );

INSERT INTO `Salutation` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '', 'No Salutation', 7, 'df674e7a-16d8-4e75-bb2b-2a965e1725f1' );


-- Tenant specific master list of resource categories.
CREATE TABLE `ResourceType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`isBillable` BIT NULL DEFAULT 0,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_ResourceType_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the ResourceType table's tenantGuid and name fields.
);
-- Index on the ResourceType table's tenantGuid field.
CREATE INDEX `I_ResourceType_tenantGuid` ON `ResourceType` (`tenantGuid`);

-- Index on the ResourceType table's tenantGuid,name fields.
CREATE INDEX `I_ResourceType_tenantGuid_name` ON `ResourceType` (`tenantGuid`, `name`);

-- Index on the ResourceType table's tenantGuid,iconId fields.
CREATE INDEX `I_ResourceType_tenantGuid_iconId` ON `ResourceType` (`tenantGuid`, `iconId`);

-- Index on the ResourceType table's tenantGuid,active fields.
CREATE INDEX `I_ResourceType_tenantGuid_active` ON `ResourceType` (`tenantGuid`, `active`);

-- Index on the ResourceType table's tenantGuid,deleted fields.
CREATE INDEX `I_ResourceType_tenantGuid_deleted` ON `ResourceType` (`tenantGuid`, `deleted`);

INSERT INTO `ResourceType` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Person', 'Human resource (operator, supervisor, engineer, etc.)', 1, 'a1b2c3d4-5678-9012-3456-789abcde0001' );

INSERT INTO `ResourceType` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Equipment', 'Heavy machinery (rollers, excavators, loaders, etc.)', 2, 'a1b2c3d4-5678-9012-3456-789abcde0002' );

INSERT INTO `ResourceType` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Vehicle', 'Trucks, service vehicles, etc.', 3, 'a1b2c3d4-5678-9012-3456-789abcde0003' );

INSERT INTO `ResourceType` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Tool', 'Smaller tools or shared items', 4, 'a1b2c3d4-5678-9012-3456-789abcde0004' );

INSERT INTO `ResourceType` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Room', 'Meeting rooms, office spaces, etc.', 5, 'a1b2c3d4-5678-9012-3456-789abcde0005' );


-- List of priority values - Tenant configurable for flexibilty
CREATE TABLE `Priority`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`iconId` INT NULL,		-- Link to the Icon table.
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_Priority_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the Priority table's tenantGuid and name fields.
);
-- Index on the Priority table's tenantGuid field.
CREATE INDEX `I_Priority_tenantGuid` ON `Priority` (`tenantGuid`);

-- Index on the Priority table's tenantGuid,name fields.
CREATE INDEX `I_Priority_tenantGuid_name` ON `Priority` (`tenantGuid`, `name`);

-- Index on the Priority table's tenantGuid,iconId fields.
CREATE INDEX `I_Priority_tenantGuid_iconId` ON `Priority` (`tenantGuid`, `iconId`);

-- Index on the Priority table's tenantGuid,active fields.
CREATE INDEX `I_Priority_tenantGuid_active` ON `Priority` (`tenantGuid`, `active`);

-- Index on the Priority table's tenantGuid,deleted fields.
CREATE INDEX `I_Priority_tenantGuid_deleted` ON `Priority` (`tenantGuid`, `deleted`);

INSERT INTO `Priority` ( `tenantGuid`, `name`, `description`, `color`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'High', 'High Priority', '#FF0F0F', 1, 'bcde74de-3f66-4c62-ad38-a5941871cea2' );

INSERT INTO `Priority` ( `tenantGuid`, `name`, `description`, `color`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Medium', 'Medium Priority', '#E8E8E8', 2, 'f2058cd4-aecf-4e28-b40c-6c181e67c0f4' );

INSERT INTO `Priority` ( `tenantGuid`, `name`, `description`, `color`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Low', 'Low Priority', '#E8E8E8', 3, '25e075c3-a513-4a45-9fbc-106afc890821' );


-- List of standard contact methods
CREATE TABLE `ContactMethod`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`iconId` INT NULL,		-- Link to the Icon table.
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`)		-- Foreign key to the Icon table.
);
-- Index on the ContactMethod table's name field.
CREATE INDEX `I_ContactMethod_name` ON `ContactMethod` (`name`);

-- Index on the ContactMethod table's iconId field.
CREATE INDEX `I_ContactMethod_iconId` ON `ContactMethod` (`iconId`);

-- Index on the ContactMethod table's active field.
CREATE INDEX `I_ContactMethod_active` ON `ContactMethod` (`active`);

-- Index on the ContactMethod table's deleted field.
CREATE INDEX `I_ContactMethod_deleted` ON `ContactMethod` (`deleted`);

INSERT INTO `ContactMethod` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Mobile Phone', 'Mobile Phone', 1, 'c8e56688-e480-426d-b49d-f7f7e7c1802c' );

INSERT INTO `ContactMethod` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Phone', 'Phone', 2, 'df379702-6082-4084-bf4e-f722893f33a2' );

INSERT INTO `ContactMethod` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Email', 'Email', 3, '1fbea244-8312-4d8c-8218-b4b5d0788510' );

INSERT INTO `ContactMethod` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Text', 'Text', 4, '9ad23e9b-76fe-4e35-9c9b-8a53b9037cce' );

INSERT INTO `ContactMethod` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Video Call', 'Video Call', 5, 'f89b6825-fd15-419f-baef-ec6c9ae61127' );

INSERT INTO `ContactMethod` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'In Person', 'In Person', 6, '91c03a84-0772-443b-8eba-e6810ec4912a' );


-- The rate types
CREATE TABLE `RateType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE `UC_RateType_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the RateType table's tenantGuid and name fields.
);
-- Index on the RateType table's tenantGuid field.
CREATE INDEX `I_RateType_tenantGuid` ON `RateType` (`tenantGuid`);

-- Index on the RateType table's tenantGuid,name fields.
CREATE INDEX `I_RateType_tenantGuid_name` ON `RateType` (`tenantGuid`, `name`);

-- Index on the RateType table's tenantGuid,active fields.
CREATE INDEX `I_RateType_tenantGuid_active` ON `RateType` (`tenantGuid`, `active`);

-- Index on the RateType table's tenantGuid,deleted fields.
CREATE INDEX `I_RateType_tenantGuid_deleted` ON `RateType` (`tenantGuid`, `deleted`);

INSERT INTO `RateType` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Standard', 'Standard Billing Rate', 1, 'e0d3b9b8-2b93-45e1-8de2-dba9603c38b9' );

INSERT INTO `RateType` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Overtime', 'Overtime Billing Rate', 2, '84897121-1587-4930-9d8c-4389ac0d222f' );

INSERT INTO `RateType` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'DoubleTime', 'DoubleTime Billing Rate', 3, 'fad24a49-924d-403f-a013-114ceb13ae27' );

INSERT INTO `RateType` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Travel', 'Travel Billing Rate', 4, 'fa0f7edd-8443-419d-9aea-229a2e61730f' );


-- Master list of interaction types.
CREATE TABLE `InteractionType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`)		-- Foreign key to the Icon table.
);
-- Index on the InteractionType table's name field.
CREATE INDEX `I_InteractionType_name` ON `InteractionType` (`name`);

-- Index on the InteractionType table's iconId field.
CREATE INDEX `I_InteractionType_iconId` ON `InteractionType` (`iconId`);

-- Index on the InteractionType table's active field.
CREATE INDEX `I_InteractionType_active` ON `InteractionType` (`active`);

-- Index on the InteractionType table's deleted field.
CREATE INDEX `I_InteractionType_deleted` ON `InteractionType` (`deleted`);

INSERT INTO `InteractionType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'In Person', 'In Person meeting', 1, '4a503ab2-a58e-403a-a400-027985773cb6' );

INSERT INTO `InteractionType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Phone Call', 'Phone Call', 2, '16988bb1-54d3-4bb9-b6a7-bfadface573d' );

INSERT INTO `InteractionType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Video Call', 'Video Call', 3, '337a67d5-53b8-4a67-ac4b-97818d0b0fa4' );

INSERT INTO `InteractionType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Text Message', 'Text Message', 4, '10ea655e-07ae-46cf-bbf3-076c3643e16b' );

INSERT INTO `InteractionType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Email Message', 'Email Message', 5, 'eeb14f23-857e-416e-80a0-9a2f82b57bf7' );


-- The currencies
CREATE TABLE `Currency`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`code` VARCHAR(10) NOT NULL,
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`isDefault` BIT NOT NULL DEFAULT 0,		-- Default currency for tenant.
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE `UC_Currency_tenantGuid_name_Unique`( `tenantGuid`, `name` ) ,		-- Uniqueness enforced on the Currency table's tenantGuid and name fields.
	UNIQUE `UC_Currency_tenantGuid_code_Unique`( `tenantGuid`, `code` ) 		-- Uniqueness enforced on the Currency table's tenantGuid and code fields.
);
-- Index on the Currency table's tenantGuid field.
CREATE INDEX `I_Currency_tenantGuid` ON `Currency` (`tenantGuid`);

-- Index on the Currency table's tenantGuid,name fields.
CREATE INDEX `I_Currency_tenantGuid_name` ON `Currency` (`tenantGuid`, `name`);

-- Index on the Currency table's tenantGuid,active fields.
CREATE INDEX `I_Currency_tenantGuid_active` ON `Currency` (`tenantGuid`, `active`);

-- Index on the Currency table's tenantGuid,deleted fields.
CREATE INDEX `I_Currency_tenantGuid_deleted` ON `Currency` (`tenantGuid`, `deleted`);

INSERT INTO `Currency` ( `tenantGuid`, `name`, `description`, `code`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'US Dollar', 'United States Dollars', 'USD', 1, '5d460ce9-4cf5-41c3-ab9d-9ef104b0a276' );

INSERT INTO `Currency` ( `tenantGuid`, `name`, `description`, `code`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Canadian Dollar', 'Canadian Dollars', 'CAD', 2, 'c6673662-f1c9-4aee-b5df-867500cb8545' );


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
CREATE TABLE `AccountType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`isRevenue` BIT NOT NULL DEFAULT 0,		-- True for revenue account types (Income), false for all others (Expense, COGS, Asset, Liability, Equity).
	`externalMapping` VARCHAR(100) NULL,		-- Maps to the account type in external systems (e.g., QuickBooks account type name).
	`color` VARCHAR(10) NULL,		-- Hex color for UI display.
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the AccountType table's name field.
CREATE INDEX `I_AccountType_name` ON `AccountType` (`name`);

-- Index on the AccountType table's active field.
CREATE INDEX `I_AccountType_active` ON `AccountType` (`active`);

-- Index on the AccountType table's deleted field.
CREATE INDEX `I_AccountType_deleted` ON `AccountType` (`deleted`);

INSERT INTO `AccountType` ( `name`, `description`, `isRevenue`, `externalMapping`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'Income', 'Revenue from operations, sales, services, grants, etc.', 1, 'Income', '#4CAF50', 1, 'a1b2c3d4-0001-4000-8000-000000000001' );

INSERT INTO `AccountType` ( `name`, `description`, `isRevenue`, `externalMapping`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'Expense', 'Operating expenses, overhead, supplies, labour, etc.', 0, 'Expense', '#F44336', 2, 'a1b2c3d4-0001-4000-8000-000000000002' );

INSERT INTO `AccountType` ( `name`, `description`, `isRevenue`, `externalMapping`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'COGS', 'Cost of Goods Sold — direct costs attributable to goods/services sold.', 0, 'Cost of Goods Sold', '#FF9800', 3, 'a1b2c3d4-0001-4000-8000-000000000003' );

INSERT INTO `AccountType` ( `name`, `description`, `isRevenue`, `externalMapping`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'Asset', 'Resources owned — cash, equipment, accounts receivable, etc.', 0, 'Other Current Asset', '#2196F3', 4, 'a1b2c3d4-0001-4000-8000-000000000004' );

INSERT INTO `AccountType` ( `name`, `description`, `isRevenue`, `externalMapping`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'Liability', 'Obligations owed — accounts payable, loans, deferred revenue, etc.', 0, 'Other Current Liability', '#9C27B0', 5, 'a1b2c3d4-0001-4000-8000-000000000005' );

INSERT INTO `AccountType` ( `name`, `description`, `isRevenue`, `externalMapping`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'Equity', 'Owner''s equity, retained earnings, net assets.', 0, 'Equity', '#607D8B', 6, 'a1b2c3d4-0001-4000-8000-000000000006' );


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
CREATE TABLE `FinancialOffice`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`code` VARCHAR(50) NOT NULL,		-- Short code for the office (e.g., 'REC', 'ADMIN', 'FIRE').
	`contactName` VARCHAR(250) NULL,		-- Accountant or financial contact name for this office.
	`contactEmail` VARCHAR(250) NULL,		-- Accountant or financial contact email for export delivery.
	`exportFormat` VARCHAR(50) NULL DEFAULT 'CSV',		-- Preferred export format: CSV, QuickBooks, Xero, etc.
	`color` VARCHAR(10) NULL,		-- Hex color for UI display.
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE `UC_FinancialOffice_tenantGuid_name_Unique`( `tenantGuid`, `name` ) ,		-- Uniqueness enforced on the FinancialOffice table's tenantGuid and name fields.
	UNIQUE `UC_FinancialOffice_tenantGuid_code_Unique`( `tenantGuid`, `code` ) 		-- Uniqueness enforced on the FinancialOffice table's tenantGuid and code fields.
);
-- Index on the FinancialOffice table's tenantGuid field.
CREATE INDEX `I_FinancialOffice_tenantGuid` ON `FinancialOffice` (`tenantGuid`);

-- Index on the FinancialOffice table's tenantGuid,name fields.
CREATE INDEX `I_FinancialOffice_tenantGuid_name` ON `FinancialOffice` (`tenantGuid`, `name`);

-- Index on the FinancialOffice table's tenantGuid,active fields.
CREATE INDEX `I_FinancialOffice_tenantGuid_active` ON `FinancialOffice` (`tenantGuid`, `active`);

-- Index on the FinancialOffice table's tenantGuid,deleted fields.
CREATE INDEX `I_FinancialOffice_tenantGuid_deleted` ON `FinancialOffice` (`tenantGuid`, `deleted`);


-- The change history for records from the FinancialOffice table.
CREATE TABLE `FinancialOfficeChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`financialOfficeId` INT NOT NULL,		-- Link to the FinancialOffice table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`financialOfficeId`) REFERENCES `FinancialOffice`(`id`)		-- Foreign key to the FinancialOffice table.
);
-- Index on the FinancialOfficeChangeHistory table's tenantGuid field.
CREATE INDEX `I_FinancialOfficeChangeHistory_tenantGuid` ON `FinancialOfficeChangeHistory` (`tenantGuid`);

-- Index on the FinancialOfficeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_FinancialOfficeChangeHistory_tenantGuid_versionNumber` ON `FinancialOfficeChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the FinancialOfficeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_FinancialOfficeChangeHistory_tenantGuid_timeStamp` ON `FinancialOfficeChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the FinancialOfficeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_FinancialOfficeChangeHistory_tenantGuid_userId` ON `FinancialOfficeChangeHistory` (`tenantGuid`, `userId`);

-- Index on the FinancialOfficeChangeHistory table's tenantGuid,financialOfficeId fields.
CREATE INDEX `I_FinancialOfficeChangeHistory_tenantGuid_financialOfficeId` ON `FinancialOfficeChangeHistory` (`tenantGuid`, `financialOfficeId`, `versionNumber`, `timeStamp`, `userId`);


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
CREATE TABLE `FinancialCategory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`code` VARCHAR(50) NOT NULL,		-- Short code for the category (e.g., '12' for Kids Rental, '40' for Easter Brunch Supplies).
	`accountTypeId` INT NOT NULL,		-- Link to AccountType — standard accounting classification (Income, Expense, COGS, Asset, Liability, Equity). Replaces the old accountType string field.
	`financialOfficeId` INT NULL,		-- Optional link to FinancialOffice — scopes this category to a specific department/committee. When null, the category is tenant-wide.
	`parentFinancialCategoryId` INT NULL,		-- Optional parent for sub-categories.
	`isTaxApplicable` BIT NOT NULL DEFAULT 0,		-- Whether HST/tax typically applies to transactions in this category.
	`defaultAmount` DECIMAL(11,2) NULL,		-- Optional default amount for common transactions in this category.
	`externalAccountId` VARCHAR(250) NULL,		-- Account ID in external system (e.g., QuickBooks account ID) for sync.
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`color` VARCHAR(10) NULL,		-- Hex color for UI display.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`accountTypeId`) REFERENCES `AccountType`(`id`),		-- Foreign key to the AccountType table.
	FOREIGN KEY (`financialOfficeId`) REFERENCES `FinancialOffice`(`id`),		-- Foreign key to the FinancialOffice table.
	FOREIGN KEY (`parentFinancialCategoryId`) REFERENCES `FinancialCategory`(`id`),		-- Foreign key to the FinancialCategory table.
	UNIQUE `UC_FinancialCategory_tenantGuid_name_Unique`( `tenantGuid`, `name` ) ,		-- Uniqueness enforced on the FinancialCategory table's tenantGuid and name fields.
	UNIQUE `UC_FinancialCategory_tenantGuid_code_Unique`( `tenantGuid`, `code` ) 		-- Uniqueness enforced on the FinancialCategory table's tenantGuid and code fields.
);
-- Index on the FinancialCategory table's tenantGuid field.
CREATE INDEX `I_FinancialCategory_tenantGuid` ON `FinancialCategory` (`tenantGuid`);

-- Index on the FinancialCategory table's tenantGuid,name fields.
CREATE INDEX `I_FinancialCategory_tenantGuid_name` ON `FinancialCategory` (`tenantGuid`, `name`);

-- Index on the FinancialCategory table's tenantGuid,accountTypeId fields.
CREATE INDEX `I_FinancialCategory_tenantGuid_accountTypeId` ON `FinancialCategory` (`tenantGuid`, `accountTypeId`);

-- Index on the FinancialCategory table's tenantGuid,financialOfficeId fields.
CREATE INDEX `I_FinancialCategory_tenantGuid_financialOfficeId` ON `FinancialCategory` (`tenantGuid`, `financialOfficeId`);

-- Index on the FinancialCategory table's tenantGuid,parentFinancialCategoryId fields.
CREATE INDEX `I_FinancialCategory_tenantGuid_parentFinancialCategoryId` ON `FinancialCategory` (`tenantGuid`, `parentFinancialCategoryId`);

-- Index on the FinancialCategory table's tenantGuid,externalAccountId fields.
CREATE INDEX `I_FinancialCategory_tenantGuid_externalAccountId` ON `FinancialCategory` (`tenantGuid`, `externalAccountId`);

-- Index on the FinancialCategory table's tenantGuid,active fields.
CREATE INDEX `I_FinancialCategory_tenantGuid_active` ON `FinancialCategory` (`tenantGuid`, `active`);

-- Index on the FinancialCategory table's tenantGuid,deleted fields.
CREATE INDEX `I_FinancialCategory_tenantGuid_deleted` ON `FinancialCategory` (`tenantGuid`, `deleted`);


-- The change history for records from the FinancialCategory table.
CREATE TABLE `FinancialCategoryChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`financialCategoryId` INT NOT NULL,		-- Link to the FinancialCategory table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`financialCategoryId`) REFERENCES `FinancialCategory`(`id`)		-- Foreign key to the FinancialCategory table.
);
-- Index on the FinancialCategoryChangeHistory table's tenantGuid field.
CREATE INDEX `I_FinancialCategoryChangeHistory_tenantGuid` ON `FinancialCategoryChangeHistory` (`tenantGuid`);

-- Index on the FinancialCategoryChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_FinancialCategoryChangeHistory_tenantGuid_versionNumber` ON `FinancialCategoryChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the FinancialCategoryChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_FinancialCategoryChangeHistory_tenantGuid_timeStamp` ON `FinancialCategoryChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the FinancialCategoryChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_FinancialCategoryChangeHistory_tenantGuid_userId` ON `FinancialCategoryChangeHistory` (`tenantGuid`, `userId`);

-- Index on the FinancialCategoryChangeHistory table's tenantGuid,financialCategoryId fields.
CREATE INDEX `I_FinancialCategoryChangeHistory_tenantGuid_financialCategoryId` ON `FinancialCategoryChangeHistory` (`tenantGuid`, `financialCategoryId`, `versionNumber`, `timeStamp`, `userId`);


/*
====================================================================================================
 TAX CODE
 Defines specific tax codes with their rates (e.g., 'HST-NL' at 15%, 'GST' at 5%, 'Exempt').
 This replaces the simple isTaxApplicable boolean on FinancialCategory with structured tax handling.

 DESIGN NOTE: Supports external system mapping via externalTaxCodeId for QuickBooks, Xero, etc.
 A tax code can have a zero rate (e.g., 'Exempt' or 'Zero-Rated').
 =====================================================================================================
*/
CREATE TABLE `TaxCode`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`code` VARCHAR(50) NOT NULL,		-- Short tax code identifier (e.g., 'HST', 'GST', 'EXEMPT').
	`rate` NUMERIC(38,22) NOT NULL DEFAULT 0,		-- Tax rate as a percentage (e.g., 15.0 for 15% HST).
	`isDefault` BIT NOT NULL DEFAULT 0,		-- Whether this is the default tax code for new transactions.
	`isExempt` BIT NOT NULL DEFAULT 0,		-- True for tax-exempt codes (rate should be 0).
	`externalTaxCodeId` VARCHAR(250) NULL,		-- Tax code ID in external system (e.g., QuickBooks TaxCode ID).
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE `UC_TaxCode_tenantGuid_name_Unique`( `tenantGuid`, `name` ) ,		-- Uniqueness enforced on the TaxCode table's tenantGuid and name fields.
	UNIQUE `UC_TaxCode_tenantGuid_code_Unique`( `tenantGuid`, `code` ) 		-- Uniqueness enforced on the TaxCode table's tenantGuid and code fields.
);
-- Index on the TaxCode table's tenantGuid field.
CREATE INDEX `I_TaxCode_tenantGuid` ON `TaxCode` (`tenantGuid`);

-- Index on the TaxCode table's tenantGuid,name fields.
CREATE INDEX `I_TaxCode_tenantGuid_name` ON `TaxCode` (`tenantGuid`, `name`);

-- Index on the TaxCode table's tenantGuid,externalTaxCodeId fields.
CREATE INDEX `I_TaxCode_tenantGuid_externalTaxCodeId` ON `TaxCode` (`tenantGuid`, `externalTaxCodeId`);

-- Index on the TaxCode table's tenantGuid,active fields.
CREATE INDEX `I_TaxCode_tenantGuid_active` ON `TaxCode` (`tenantGuid`, `active`);

-- Index on the TaxCode table's tenantGuid,deleted fields.
CREATE INDEX `I_TaxCode_tenantGuid_deleted` ON `TaxCode` (`tenantGuid`, `deleted`);


/*
====================================================================================================
 CHARGE MASTER (Like Epic CDM)
 Master list of chargeable items (revenue or expenses). e.g., "Site Visit Fee" (revenue), "Travel Expense" (expense).
 Tied to RateType for billing context.
 =====================================================================================================
*/
CREATE TABLE `ChargeType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`externalId` VARCHAR(100) NULL,
	`isRevenue` BIT NOT NULL DEFAULT 1,		-- True = Revenue (billable), False = Expense (cost)
	`isTaxable` BIT NULL DEFAULT 0,
	`defaultAmount` DECIMAL(11,2) NULL,		-- Optional default value for auto-drops
	`defaultDescription` VARCHAR(500) NULL,		-- sometimes auto-dropped charges need a note (e.g., "Travel to site – 45 km").
	`rateTypeId` INT NULL,		-- Link to RateType (e.g., 'Standard', 'Overtime')
	`currencyId` INT NOT NULL,		-- Link to the Currency table.
	`financialCategoryId` INT NULL,		-- Optional bridge to the general ledger. Maps this charge type to a FinancialCategory for unified Chart of Accounts reporting across both event charges and standalone transactions.
	`taxCodeId` INT NULL,		-- Optional default TaxCode for charges of this type. When set, new EventCharges auto-inherit this tax rate.
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`rateTypeId`) REFERENCES `RateType`(`id`),		-- Foreign key to the RateType table.
	FOREIGN KEY (`currencyId`) REFERENCES `Currency`(`id`),		-- Foreign key to the Currency table.
	FOREIGN KEY (`financialCategoryId`) REFERENCES `FinancialCategory`(`id`),		-- Foreign key to the FinancialCategory table.
	FOREIGN KEY (`taxCodeId`) REFERENCES `TaxCode`(`id`),		-- Foreign key to the TaxCode table.
	UNIQUE `UC_ChargeType_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the ChargeType table's tenantGuid and name fields.
);
-- Index on the ChargeType table's tenantGuid field.
CREATE INDEX `I_ChargeType_tenantGuid` ON `ChargeType` (`tenantGuid`);

-- Index on the ChargeType table's tenantGuid,name fields.
CREATE INDEX `I_ChargeType_tenantGuid_name` ON `ChargeType` (`tenantGuid`, `name`);

-- Index on the ChargeType table's tenantGuid,externalId fields.
CREATE INDEX `I_ChargeType_tenantGuid_externalId` ON `ChargeType` (`tenantGuid`, `externalId`);

-- Index on the ChargeType table's tenantGuid,rateTypeId fields.
CREATE INDEX `I_ChargeType_tenantGuid_rateTypeId` ON `ChargeType` (`tenantGuid`, `rateTypeId`);

-- Index on the ChargeType table's tenantGuid,currencyId fields.
CREATE INDEX `I_ChargeType_tenantGuid_currencyId` ON `ChargeType` (`tenantGuid`, `currencyId`);

-- Index on the ChargeType table's tenantGuid,financialCategoryId fields.
CREATE INDEX `I_ChargeType_tenantGuid_financialCategoryId` ON `ChargeType` (`tenantGuid`, `financialCategoryId`);

-- Index on the ChargeType table's tenantGuid,taxCodeId fields.
CREATE INDEX `I_ChargeType_tenantGuid_taxCodeId` ON `ChargeType` (`tenantGuid`, `taxCodeId`);

-- Index on the ChargeType table's tenantGuid,active fields.
CREATE INDEX `I_ChargeType_tenantGuid_active` ON `ChargeType` (`tenantGuid`, `active`);

-- Index on the ChargeType table's tenantGuid,deleted fields.
CREATE INDEX `I_ChargeType_tenantGuid_deleted` ON `ChargeType` (`tenantGuid`, `deleted`);


-- The change history for records from the ChargeType table.
CREATE TABLE `ChargeTypeChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`chargeTypeId` INT NOT NULL,		-- Link to the ChargeType table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`chargeTypeId`) REFERENCES `ChargeType`(`id`)		-- Foreign key to the ChargeType table.
);
-- Index on the ChargeTypeChangeHistory table's tenantGuid field.
CREATE INDEX `I_ChargeTypeChangeHistory_tenantGuid` ON `ChargeTypeChangeHistory` (`tenantGuid`);

-- Index on the ChargeTypeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ChargeTypeChangeHistory_tenantGuid_versionNumber` ON `ChargeTypeChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ChargeTypeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ChargeTypeChangeHistory_tenantGuid_timeStamp` ON `ChargeTypeChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ChargeTypeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ChargeTypeChangeHistory_tenantGuid_userId` ON `ChargeTypeChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ChargeTypeChangeHistory table's tenantGuid,chargeTypeId fields.
CREATE INDEX `I_ChargeTypeChangeHistory_tenantGuid_chargeTypeId` ON `ChargeTypeChangeHistory` (`tenantGuid`, `chargeTypeId`, `versionNumber`, `timeStamp`, `userId`);


-- List of tags
CREATE TABLE `Tag`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`isSystem` BIT NULL,		-- To mark as system tag for protected / special handling.  For things like 'deceased'.
	`priorityId` INT NULL,		-- Link to the Priority table.
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`priorityId`) REFERENCES `Priority`(`id`),		-- Foreign key to the Priority table.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_Tag_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the Tag table's tenantGuid and name fields.
);
-- Index on the Tag table's tenantGuid field.
CREATE INDEX `I_Tag_tenantGuid` ON `Tag` (`tenantGuid`);

-- Index on the Tag table's tenantGuid,name fields.
CREATE INDEX `I_Tag_tenantGuid_name` ON `Tag` (`tenantGuid`, `name`);

-- Index on the Tag table's tenantGuid,priorityId fields.
CREATE INDEX `I_Tag_tenantGuid_priorityId` ON `Tag` (`tenantGuid`, `priorityId`);

-- Index on the Tag table's tenantGuid,iconId fields.
CREATE INDEX `I_Tag_tenantGuid_iconId` ON `Tag` (`tenantGuid`, `iconId`);

-- Index on the Tag table's tenantGuid,active fields.
CREATE INDEX `I_Tag_tenantGuid_active` ON `Tag` (`tenantGuid`, `active`);

-- Index on the Tag table's tenantGuid,deleted fields.
CREATE INDEX `I_Tag_tenantGuid_deleted` ON `Tag` (`tenantGuid`, `deleted`);

INSERT INTO `Tag` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Person', 'Human resource (operator, supervisor, engineer, etc.)', 1, 'a1b2c3d4-5678-9012-3456-789abcde0001' );


-- Time zones master data list.
CREATE TABLE `TimeZone`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`ianaTimeZone` VARCHAR(50) NOT NULL,		-- e.g., 'America/St.John's' (official IANA name)
	`abbreviation` VARCHAR(50) NOT NULL,
	`abbreviationDaylightSavings` VARCHAR(50) NOT NULL,
	`supportsDaylightSavings` BIT NOT NULL DEFAULT 1,
	`standardUTCOffsetHours` FLOAT NOT NULL,		-- The standard offset hours from UTC for this time zone.
	`dstUTCOffsetHours` FLOAT NOT NULL,		-- Use the same value here as the standard one for time zones that do not support DST
	`sequence` INT NULL,		-- For sorting in drop downs
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the TimeZone table's name field.
CREATE INDEX `I_TimeZone_name` ON `TimeZone` (`name`);

-- Index on the TimeZone table's active field.
CREATE INDEX `I_TimeZone_active` ON `TimeZone` (`active`);

-- Index on the TimeZone table's deleted field.
CREATE INDEX `I_TimeZone_deleted` ON `TimeZone` (`deleted`);

INSERT INTO `TimeZone` ( `name`, `abbreviation`, `abbreviationDaylightSavings`, `supportsDaylightSavings`, `standardUTCOffsetHours`, `dstUTCOffsetHours`, `description`, `ianaTimeZone`, `sequence`, `objectGuid` ) VALUES  ( 'Newfoundland Standard Time', 'NST', 'NDT', 1, -3.5, -2.5, 'Newfoundland and southeastern Labrador (Canada)', 'America/St_Johns', 10, '27129170-81b3-4c70-a7d4-0378dce8426f' );

INSERT INTO `TimeZone` ( `name`, `abbreviation`, `abbreviationDaylightSavings`, `supportsDaylightSavings`, `standardUTCOffsetHours`, `dstUTCOffsetHours`, `description`, `ianaTimeZone`, `sequence`, `objectGuid` ) VALUES  ( 'Atlantic Standard Time', 'AST', 'ADT', 1, -4, -3, 'Atlantic Canada (Nova Scotia, New Brunswick, PEI, parts of Quebec)', 'America/Halifax', 20, '8f3d2a1b-4c5e-4d8f-9a2b-6e7f1c3d9a0b' );

INSERT INTO `TimeZone` ( `name`, `abbreviation`, `abbreviationDaylightSavings`, `supportsDaylightSavings`, `standardUTCOffsetHours`, `dstUTCOffsetHours`, `description`, `ianaTimeZone`, `sequence`, `objectGuid` ) VALUES  ( 'Atlantic Standard Time (no DST)', 'AST', 'AST', 0, -4, -4, 'Puerto Rico, US Virgin Islands, Dominican Republic', 'America/Puerto_Rico', 30, '648d1e27-51b2-4e9b-ae9e-06dd856022e8' );

INSERT INTO `TimeZone` ( `name`, `abbreviation`, `abbreviationDaylightSavings`, `supportsDaylightSavings`, `standardUTCOffsetHours`, `dstUTCOffsetHours`, `description`, `ianaTimeZone`, `sequence`, `objectGuid` ) VALUES  ( 'Eastern Standard Time', 'EST', 'EDT', 1, -5, -4, 'Eastern United States, Eastern Canada (Ontario, Quebec)', 'America/New_York', 40, 'c4e5f6a7-8b9c-4d0e-1f2a-3b4c5d6e7f8a' );

INSERT INTO `TimeZone` ( `name`, `abbreviation`, `abbreviationDaylightSavings`, `supportsDaylightSavings`, `standardUTCOffsetHours`, `dstUTCOffsetHours`, `description`, `ianaTimeZone`, `sequence`, `objectGuid` ) VALUES  ( 'Central Standard Time', 'CST', 'CDT', 1, -6, -5, 'Central United States, Central Canada, Mexico (most), Central America', 'America/Chicago', 50, 'd5e6f7a8-9c0d-4e1f-2a3b-4c5d6e7f8a9b' );

INSERT INTO `TimeZone` ( `name`, `abbreviation`, `abbreviationDaylightSavings`, `supportsDaylightSavings`, `standardUTCOffsetHours`, `dstUTCOffsetHours`, `description`, `ianaTimeZone`, `sequence`, `objectGuid` ) VALUES  ( 'Central Standard Time (no DST)', 'CST', 'CST', 0, -6, -6, 'Central America (Guatemala, Costa Rica, Nicaragua, etc.)', 'America/Guatemala', 60, 'f2b768f4-6162-4f65-8eb8-6ae1c5a9dc88' );

INSERT INTO `TimeZone` ( `name`, `abbreviation`, `abbreviationDaylightSavings`, `supportsDaylightSavings`, `standardUTCOffsetHours`, `dstUTCOffsetHours`, `description`, `ianaTimeZone`, `sequence`, `objectGuid` ) VALUES  ( 'Mountain Standard Time', 'MST', 'MDT', 1, -7, -6, 'Mountain United States (except Arizona), Western Canada', 'America/Denver', 70, 'e6f7a8b9-0d1e-4f2a-3b4c-5d6e7f8a9b0c' );

INSERT INTO `TimeZone` ( `name`, `abbreviation`, `abbreviationDaylightSavings`, `supportsDaylightSavings`, `standardUTCOffsetHours`, `dstUTCOffsetHours`, `description`, `ianaTimeZone`, `sequence`, `objectGuid` ) VALUES  ( 'Arizona Time', 'MST', 'MST', 0, -7, -7, 'Arizona (United States) — does not observe Daylight Saving Time', 'America/Phoenix', 80, 'f7a8b9c0-1e2f-4a3b-5c6d-7e8f9a0b1c2d' );

INSERT INTO `TimeZone` ( `name`, `abbreviation`, `abbreviationDaylightSavings`, `supportsDaylightSavings`, `standardUTCOffsetHours`, `dstUTCOffsetHours`, `description`, `ianaTimeZone`, `sequence`, `objectGuid` ) VALUES  ( 'Pacific Standard Time', 'PST', 'PDT', 1, -8, -7, 'Western United States, Western Canada (British Columbia)', 'America/Los_Angeles', 90, 'a8b9c0d1-2f3a-4b5c-6d7e-8f9a0b1c2d3e' );

INSERT INTO `TimeZone` ( `name`, `abbreviation`, `abbreviationDaylightSavings`, `supportsDaylightSavings`, `standardUTCOffsetHours`, `dstUTCOffsetHours`, `description`, `ianaTimeZone`, `sequence`, `objectGuid` ) VALUES  ( 'Alaska Standard Time', 'AKST', 'AKDT', 1, -9, -8, 'Alaska (United States)', 'America/Anchorage', 100, 'b9c0d1e2-3a4b-5c6d-7e8f-9a0b1c2d3e4f' );

INSERT INTO `TimeZone` ( `name`, `abbreviation`, `abbreviationDaylightSavings`, `supportsDaylightSavings`, `standardUTCOffsetHours`, `dstUTCOffsetHours`, `description`, `ianaTimeZone`, `sequence`, `objectGuid` ) VALUES  ( 'Hawaii-Aleutian Standard Time', 'HST', 'HST', 0, -10, -10, 'Hawaii and Aleutian Islands (United States) — no Daylight Saving Time', 'Pacific/Honolulu', 110, 'c0d1e2f3-4b5c-6d7e-8f9a-0b1c2d3e4f5a' );


-- The master list of countries
CREATE TABLE `Country`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`abbreviation` VARCHAR(10) NOT NULL,
	`postalCodeFormat` VARCHAR(50) NULL,		-- The human readable postal code format for the country, if applicable.
	`postalCodeRegEx` VARCHAR(50) NULL,		-- The regular expression pattern for validation of the postal code, if applicable 
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the Country table's name field.
CREATE INDEX `I_Country_name` ON `Country` (`name`);

-- Index on the Country table's active field.
CREATE INDEX `I_Country_active` ON `Country` (`active`);

-- Index on the Country table's deleted field.
CREATE INDEX `I_Country_deleted` ON `Country` (`deleted`);

INSERT INTO `Country` ( `name`, `description`, `abbreviation`, `sequence`, `postalCodeFormat`, `postalCodeRegEx`, `objectGuid` ) VALUES  ( 'Canada', 'Canada', 'CA', 1, 'A0A 0A0', '^[A-Z]\d[A-Z] ?\d[A-Z]\d$', '5f3f3c1d-9ba8-48cd-ae6d-4f4d8a5c2bcb' );

INSERT INTO `Country` ( `name`, `description`, `abbreviation`, `sequence`, `postalCodeFormat`, `postalCodeRegEx`, `objectGuid` ) VALUES  ( 'USA', 'United States of America', 'US', 2, 'NNNNN or NNNNN-NNNN', '^\d{5}(-\d{4})?$'')', '9b2b1de3-719f-4c8a-bb2f-6e903d4e74b5' );


-- The master list of states
CREATE TABLE `StateProvince`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`countryId` INT NOT NULL,		-- Link to the Country table.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`abbreviation` VARCHAR(10) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`countryId`) REFERENCES `Country`(`id`),		-- Foreign key to the Country table.
	UNIQUE `UC_StateProvince_name_countryId_Unique`( `name`, `countryId` ) ,		-- Uniqueness enforced on the StateProvince table's name and countryId fields.
	UNIQUE `UC_StateProvince_abbreviation_countryId_Unique`( `abbreviation`, `countryId` ) 		-- Uniqueness enforced on the StateProvince table's abbreviation and countryId fields.
);
-- Index on the StateProvince table's countryId field.
CREATE INDEX `I_StateProvince_countryId` ON `StateProvince` (`countryId`);

-- Index on the StateProvince table's name field.
CREATE INDEX `I_StateProvince_name` ON `StateProvince` (`name`);

-- Index on the StateProvince table's active field.
CREATE INDEX `I_StateProvince_active` ON `StateProvince` (`active`);

-- Index on the StateProvince table's deleted field.
CREATE INDEX `I_StateProvince_deleted` ON `StateProvince` (`deleted`);

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Newfoundland', 'Newfoundland and Labrador', 'NL', 1, ( SELECT id FROM `Country` WHERE `name` = 'Canada' LIMIT 1 ), 'a1eecf09-7362-42be-b5d1-90284e1c3075' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Ontario', 'Ontario', 'ON', 2, ( SELECT id FROM `Country` WHERE `name` = 'Canada' LIMIT 1 ), 'b2e5d8f1-897b-4563-8131-7eeb6d0c80a4' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Alberta', 'Alberta', 'AB', 3, ( SELECT id FROM `Country` WHERE `name` = 'Canada' LIMIT 1 ), 'c3fe34bc-9601-474f-b99f-55c7a9c71738' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'British Columbia', 'British Columbia', 'BC', 4, ( SELECT id FROM `Country` WHERE `name` = 'Canada' LIMIT 1 ), 'd4b7ab65-8fc6-4746-b9f6-e9bcf5b8cf91' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Manitoba', 'Manitoba', 'MB', 5, ( SELECT id FROM `Country` WHERE `name` = 'Canada' LIMIT 1 ), 'e5a8be2d-7a4e-43e5-83d5-d2cf77282c0d' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'New Brunswick', 'New Brunswick', 'NB', 6, ( SELECT id FROM `Country` WHERE `name` = 'Canada' LIMIT 1 ), 'f6f2a6f4-3963-4539-a54f-bd7ed0be2b3b' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Northwest Territories', 'Northwest Territories', 'NT', 7, ( SELECT id FROM `Country` WHERE `name` = 'Canada' LIMIT 1 ), '078f1d72-20a4-4b78-8b2f-9c6d6e69f29a' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Nova Scotia', 'Nova Scotia', 'NS', 8, ( SELECT id FROM `Country` WHERE `name` = 'Canada' LIMIT 1 ), '179fbbf1-b651-4b7a-b17e-b65d6aeb7795' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Nunavut', 'Nunavut', 'NU', 9, ( SELECT id FROM `Country` WHERE `name` = 'Canada' LIMIT 1 ), '28a1b2ed-7554-48b5-b7f0-b0f2bc3f0a8f' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Prince Edward Island', 'Prince Edward Island', 'PE', 10, ( SELECT id FROM `Country` WHERE `name` = 'Canada' LIMIT 1 ), '39b8c1de-dc77-4b3b-b0f6-e41b6a557809' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Quebec', 'Quebec', 'QC', 11, ( SELECT id FROM `Country` WHERE `name` = 'Canada' LIMIT 1 ), '4b9e6f87-b15f-4858-b739-dc23714b83b7' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Saskatchewan', 'Saskatchewan', 'SK', 12, ( SELECT id FROM `Country` WHERE `name` = 'Canada' LIMIT 1 ), '5c12c0ea-23a0-43a3-a8c9-15d032de5643' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Yukon', 'Yukon', 'YT', 13, ( SELECT id FROM `Country` WHERE `name` = 'Canada' LIMIT 1 ), '6d1a81eb-fc4a-4c44-9e5a-079c32074749' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Other', 'Other', 'Other', 99, ( SELECT id FROM `Country` WHERE `name` = 'Canada' LIMIT 1 ), '7e2f5bce-c2b0-4012-84b4-c982d78dce3e' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Alabama', 'Alabama', 'AL', 1, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'd003a92b-6cec-4d49-8baa-6b4fd8fc2f92' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Alaska', 'Alaska', 'AK', 2, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '3aff430d-2752-4d91-ae08-656934438dac' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Arizona', 'Arizona', 'AZ', 3, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '5c4ec86a-472a-4d6c-a278-b5e21352b644' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Arkansas', 'Arkansas', 'AR', 4, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'cd58100a-e5b6-4fc0-a251-2e1a22e66836' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'California', 'California', 'CA', 5, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '36a7adaa-f35a-40ca-8f24-231a3ebd1ad8' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Colorado', 'Colorado', 'CO', 6, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '0210922a-348c-4181-a9e0-6054dd7bc655' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Connecticut', 'Connecticut', 'CT', 7, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '4040cc1a-e6f4-454d-93aa-162c74fe50c6' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Delaware', 'Delaware', 'DE', 8, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '01a5dc36-c285-4216-9fb6-811d5b8e8b48' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Florida', 'Florida', 'FL', 9, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '5e0bb9f6-b6ca-4b42-832f-7c41a570fae4' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Georgia', 'Georgia', 'GA', 10, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'c57ffded-5284-471a-898c-f4969f611dd7' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Hawaii', 'Hawaii', 'HI', 11, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '9fcaa230-ded7-47a8-8a3e-dd1a756ca363' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Idaho', 'Idaho', 'ID', 12, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '796c444b-7513-4823-ab11-94dae65dc0e5' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Illinois', 'Illinois', 'IL', 13, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'd2a28ab4-09c1-437b-b70c-1424543c4128' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Indiana', 'Indiana', 'IN', 14, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '3d9f6c85-6515-4147-adec-ab7dc6e95eab' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Iowa', 'Iowa', 'IA', 15, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'cecfa624-ba4a-473e-a0fc-e91b007beab7' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Kansas', 'Kansas', 'KS', 16, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'b155c44b-c3dd-4884-b715-71ab38596e00' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Kentucky', 'Kentucky', 'KY', 17, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '152ad250-6174-45f7-a947-6c6c14a56494' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Louisiana', 'Louisiana', 'LA', 18, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'c9260be6-9840-420c-acf4-7d82ef937160' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Maine', 'Maine', 'ME', 19, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '97b79ed1-f1b0-44ef-bdd0-71caccd1465d' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Maryland', 'Maryland', 'MD', 20, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'c0cf2ae1-ed20-4845-b860-ff008427359b' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Massachusetts', 'Massachusetts', 'MA', 21, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '7801225d-a996-40cb-888e-49645ffdbb06' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Michigan', 'Michigan', 'MI', 22, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'f9324013-0a60-43ea-b672-6999a821cb15' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Minnesota', 'Minnesota', 'MN', 23, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'f43770fd-ceaf-4646-9943-08be6268c045' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Mississippi', 'Mississippi', 'MS', 24, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'b193e806-5a5e-4d46-936c-b4b3a28e59c5' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Missouri', 'Missouri', 'MO', 25, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'd57e6019-c221-465e-b92e-0b8d3da0ff80' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Montana', 'Montana', 'MT', 26, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '2f10e38c-b937-459f-89d0-60f552687c46' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Nebraska', 'Nebraska', 'NE', 27, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '85ad29eb-f1c6-4862-82bd-d4c91eea2838' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Nevada', 'Nevada', 'NV', 28, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '95ad29eb-f1c6-4862-82bd-d4c91eea2887' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'New Hampshire', 'New Hampshire', 'NH', 29, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '5e5d5651-a186-4cc1-b61a-f22c9d530e6f' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'New Jersey', 'New Jersey', 'NJ', 30, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'ee4ab53d-dab1-4ba7-8363-ed616a779567' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'New Mexico', 'New Mexico', 'NM', 31, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'be168b30-72bd-4942-b187-deff865a5e6a' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'New York', 'New York', 'NY', 32, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '7c93f785-a069-4298-93dc-2ef5e00fd0a8' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'North Carolina', 'North Carolina', 'NC', 33, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'af2af206-9f3c-419f-9731-9fc90f1bda1b' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'North Dakota', 'North Dakota', 'ND', 34, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '3a8d0072-1457-4923-bf19-12b8748098ee' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Ohio', 'Ohio', 'OH', 35, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'd1961e5f-1c25-46ef-9bca-30fe538fe5c9' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Oklahoma', 'Oklahoma', 'OK', 36, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'b2bc6d1b-32b6-4026-b648-70ec7b5063b1' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Oregon', 'Oregon', 'OR', 37, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'fbd6a82b-3f4b-49e0-b5ba-59ec47335c99' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Pennsylvania', 'Pennsylvania', 'PA', 38, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'd9b34153-fb25-403d-a13e-37b2823fbf69' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Rhode Island', 'Rhode Island', 'RI', 39, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'c1c32aa7-af93-4bf1-9acf-9ff591b1b8c5' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'South Carolina', 'South Carolina', 'SC', 40, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '9d050cab-34a0-40eb-8592-2ee2a62e21a1' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'South Dakota', 'South Dakota', 'SD', 41, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'e652bc14-13e0-4405-9feb-6b78dd0790dd' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Tennessee', 'Tennessee', 'TN', 42, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '0d7a100b-792e-46ca-81e0-eaef7e78aec2' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Texas', 'Texas', 'TX', 43, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '5384bf42-c1a8-47c8-998c-85c02838a299' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Utah', 'Utah', 'UT', 44, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '6f4755b9-8a7a-4c52-a8a2-a464de793cbd' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Vermont', 'Vermont', 'VT', 45, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '9dd23ade-bbf4-4d5a-9fd8-199af9005145' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Virginia', 'Virginia', 'VA', 46, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '6071e23d-d660-4801-894e-0ca5783d6a31' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Washington', 'Washington', 'WA', 47, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'cc5b5362-f9fc-406f-927d-d6c4e917f76d' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'West Virginia', 'West Virginia', 'WV', 48, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '06d12574-b3b8-4392-87a1-76a8c42ccf7a' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Wisconsin', 'Wisconsin', 'WI', 49, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'ebf4200d-b4f0-4a62-b2a9-256aab919241' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Wyoming', 'Wyoming', 'WY', 50, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), 'dfff135c-165b-42a9-81f9-a55f8d51c710' );

INSERT INTO `StateProvince` ( `name`, `description`, `abbreviation`, `sequence`, `countryId`, `objectGuid` ) VALUES  ( 'Other', 'Other', 'Other', 99, ( SELECT id FROM `Country` WHERE `name` = 'USA' LIMIT 1 ), '4ab041c0-9479-4a65-ba56-cbb70d82de75' );


/*
Master list of volunteer lifecycle/status values.
Examples: Prospect, Active, On Leave, Inactive, Not Re-invited.
Used to track engagement level and control visibility/assignment rules.
*/
CREATE TABLE `VolunteerStatus`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`color` VARCHAR(10) NULL,		-- Suggested UI color for this status
	`iconId` INT NULL,		-- Optional icon for visual distinction
	`isActive` BIT NULL DEFAULT 1,		-- Whether volunteers in this status are generally schedulable
	`preventsScheduling` BIT NOT NULL DEFAULT 0,		-- Hard block: cannot be assigned to events
	`requiresApproval` BIT NOT NULL DEFAULT 0,		-- New assignments need coordinator approval
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`)		-- Foreign key to the Icon table.
);
-- Index on the VolunteerStatus table's name field.
CREATE INDEX `I_VolunteerStatus_name` ON `VolunteerStatus` (`name`);

-- Index on the VolunteerStatus table's iconId field.
CREATE INDEX `I_VolunteerStatus_iconId` ON `VolunteerStatus` (`iconId`);

-- Index on the VolunteerStatus table's active field.
CREATE INDEX `I_VolunteerStatus_active` ON `VolunteerStatus` (`active`);

-- Index on the VolunteerStatus table's deleted field.
CREATE INDEX `I_VolunteerStatus_deleted` ON `VolunteerStatus` (`deleted`);

INSERT INTO `VolunteerStatus` ( `name`, `description`, `sequence`, `color`, `isActive`, `preventsScheduling`, `requiresApproval`, `objectGuid` ) VALUES  ( 'Pending', 'Self-registered volunteer awaiting admin approval', 5, '#F59E0B', 0, 1, 1, 'a1111111-2222-3333-4444-555555555000' );

INSERT INTO `VolunteerStatus` ( `name`, `description`, `sequence`, `color`, `isActive`, `preventsScheduling`, `objectGuid` ) VALUES  ( 'Prospect / Interested', 'Has expressed interest but not yet onboarded', 10, '#9E9E9E', 0, 1, 'a1111111-2222-3333-4444-555555555001' );

INSERT INTO `VolunteerStatus` ( `name`, `description`, `sequence`, `color`, `isActive`, `preventsScheduling`, `objectGuid` ) VALUES  ( 'Active', 'Fully onboarded and available for assignments', 20, '#4CAF50', 1, 0, 'a1111111-2222-3333-4444-555555555002' );

INSERT INTO `VolunteerStatus` ( `name`, `description`, `sequence`, `color`, `isActive`, `preventsScheduling`, `objectGuid` ) VALUES  ( 'On Hiatus / Leave', 'Temporary break (maternity, travel, etc.)', 30, '#FF9800', 0, 1, 'a1111111-2222-3333-4444-555555555003' );

INSERT INTO `VolunteerStatus` ( `name`, `description`, `sequence`, `color`, `isActive`, `preventsScheduling`, `objectGuid` ) VALUES  ( 'Inactive', 'No longer participating, but record retained', 40, '#757575', 0, 1, 'a1111111-2222-3333-4444-555555555004' );

INSERT INTO `VolunteerStatus` ( `name`, `description`, `sequence`, `color`, `isActive`, `preventsScheduling`, `objectGuid` ) VALUES  ( 'Not Re-invited', 'Previous issues; do not contact or schedule', 50, '#F44336', 0, 1, 'a1111111-2222-3333-4444-555555555005' );


-- the contact types
CREATE TABLE `ContactType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`)		-- Foreign key to the Icon table.
);
-- Index on the ContactType table's name field.
CREATE INDEX `I_ContactType_name` ON `ContactType` (`name`);

-- Index on the ContactType table's iconId field.
CREATE INDEX `I_ContactType_iconId` ON `ContactType` (`iconId`);

-- Index on the ContactType table's active field.
CREATE INDEX `I_ContactType_active` ON `ContactType` (`active`);

-- Index on the ContactType table's deleted field.
CREATE INDEX `I_ContactType_deleted` ON `ContactType` (`deleted`);

INSERT INTO `ContactType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Project Manager', 'Primary contact for project coordination', 1, '16df32e3-67e4-4012-b2e5-8810b8ab46b9' );

INSERT INTO `ContactType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Billing Contact', 'Handles invoices and payments', 2, '1e92d7e0-599c-4c72-9e52-731c1129dd88' );

INSERT INTO `ContactType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Site Superintendent', 'Site Superintendent', 3, 'f3397214-a488-4522-9968-69f6e9985942' );

INSERT INTO `ContactType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Safety Officer', 'Health & safety representative', 4, 'cfdc40e3-36cb-4cee-863b-184a494f89bb' );

INSERT INTO `ContactType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Technical Contact', 'Engineering or specs questions', 5, '9586c951-4a27-4975-94c0-70252c86880b' );

INSERT INTO `ContactType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Emergency Contact', 'For urgent notifications', 6, '7ff865f4-977a-4e94-974b-e86d942a8405' );

INSERT INTO `ContactType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Accounts Payable', 'Payment processing', 7, 'f42ce916-a408-44d7-bbd4-9f6fc00243e4' );

INSERT INTO `ContactType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Volunteer', 'Volunteer', 8, '776395dd-6187-44aa-910e-1bf0135cc88a' );

INSERT INTO `ContactType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Staff', 'Staff', 9, '5cd5bdee-ba1b-43de-8249-8909546b7d28' );

INSERT INTO `ContactType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Resident', 'Resident', 10, '688ae8cf-ae9d-44f2-a3a4-a900fff70fd9' );

INSERT INTO `ContactType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Other', 'Other', 99, '95b327b8-9bfc-4338-a04c-e3f61c56f397' );


-- The contact data
CREATE TABLE `Contact`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`contactTypeId` INT NOT NULL,		-- Link to the ContactType table.
	`firstName` VARCHAR(250) NOT NULL,
	`middleName` VARCHAR(250) NULL,
	`lastName` VARCHAR(250) NOT NULL,
	`salutationId` INT NULL,		-- Link to the Salutation table.
	`title` VARCHAR(250) NULL,
	`birthDate` DATE NULL,
	`company` VARCHAR(250) NULL,
	`email` VARCHAR(250) NULL,
	`phone` VARCHAR(50) NULL,
	`mobile` VARCHAR(50) NULL,
	`position` VARCHAR(250) NULL,
	`webSite` VARCHAR(1000) NULL,
	`contactMethodId` INT NULL,		-- Link to the ContactMethod table.
	`notes` TEXT NULL,
	`timeZoneId` INT NULL,		-- The contact's time zone
	`attributes` TEXT NULL,		-- to store arbitrary JSON
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`avatarFileName` VARCHAR(250) NULL,		-- Part of the binary data field setup
	`avatarSize` BIGINT NULL,		-- Part of the binary data field setup
	`avatarData` BLOB NULL,		-- Part of the binary data field setup
	`avatarMimeType` VARCHAR(100) NULL,		-- Part of the binary data field setup
	`externalId` VARCHAR(100) NULL,
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`contactTypeId`) REFERENCES `ContactType`(`id`),		-- Foreign key to the ContactType table.
	FOREIGN KEY (`salutationId`) REFERENCES `Salutation`(`id`),		-- Foreign key to the Salutation table.
	FOREIGN KEY (`contactMethodId`) REFERENCES `ContactMethod`(`id`),		-- Foreign key to the ContactMethod table.
	FOREIGN KEY (`timeZoneId`) REFERENCES `TimeZone`(`id`),		-- Foreign key to the TimeZone table.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`)		-- Foreign key to the Icon table.
);
-- Index on the Contact table's tenantGuid field.
CREATE INDEX `I_Contact_tenantGuid` ON `Contact` (`tenantGuid`);

-- Index on the Contact table's tenantGuid,contactTypeId fields.
CREATE INDEX `I_Contact_tenantGuid_contactTypeId` ON `Contact` (`tenantGuid`, `contactTypeId`);

-- Index on the Contact table's tenantGuid,company fields.
CREATE INDEX `I_Contact_tenantGuid_company` ON `Contact` (`tenantGuid`, `company`);

-- emails must be unique to one contact.
CREATE UNIQUE INDEX `I_Contact_tenantGuid_email` ON `Contact` (`tenantGuid`, `email`);
 WHERE `email` IS NOT NULL

-- Index on the Contact table's tenantGuid,phone fields.
CREATE INDEX `I_Contact_tenantGuid_phone` ON `Contact` (`tenantGuid`, `phone`);

-- Index on the Contact table's tenantGuid,mobile fields.
CREATE INDEX `I_Contact_tenantGuid_mobile` ON `Contact` (`tenantGuid`, `mobile`);

-- Index on the Contact table's tenantGuid,position fields.
CREATE INDEX `I_Contact_tenantGuid_position` ON `Contact` (`tenantGuid`, `position`);

-- Index on the Contact table's tenantGuid,contactMethodId fields.
CREATE INDEX `I_Contact_tenantGuid_contactMethodId` ON `Contact` (`tenantGuid`, `contactMethodId`);

-- Index on the Contact table's tenantGuid,timeZoneId fields.
CREATE INDEX `I_Contact_tenantGuid_timeZoneId` ON `Contact` (`tenantGuid`, `timeZoneId`);

-- Index on the Contact table's tenantGuid,iconId fields.
CREATE INDEX `I_Contact_tenantGuid_iconId` ON `Contact` (`tenantGuid`, `iconId`);

-- Index on the Contact table's tenantGuid,active fields.
CREATE INDEX `I_Contact_tenantGuid_active` ON `Contact` (`tenantGuid`, `active`);

-- Index on the Contact table's tenantGuid,deleted fields.
CREATE INDEX `I_Contact_tenantGuid_deleted` ON `Contact` (`tenantGuid`, `deleted`);

-- Index on the Contact table's tenantGuid,externalId fields.
CREATE INDEX `I_Contact_tenantGuid_externalId` ON `Contact` (`tenantGuid`, `externalId`);

-- Index on the Contact table's tenantGuid,lastName,firstName fields.
CREATE INDEX `I_Contact_tenantGuid_lastName_firstName` ON `Contact` (`tenantGuid`, `lastName`, `firstName`);


-- The change history for records from the Contact table.
CREATE TABLE `ContactChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`contactId` INT NOT NULL,		-- Link to the Contact table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`contactId`) REFERENCES `Contact`(`id`)		-- Foreign key to the Contact table.
);
-- Index on the ContactChangeHistory table's tenantGuid field.
CREATE INDEX `I_ContactChangeHistory_tenantGuid` ON `ContactChangeHistory` (`tenantGuid`);

-- Index on the ContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ContactChangeHistory_tenantGuid_versionNumber` ON `ContactChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ContactChangeHistory_tenantGuid_timeStamp` ON `ContactChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ContactChangeHistory_tenantGuid_userId` ON `ContactChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ContactChangeHistory table's tenantGuid,contactId fields.
CREATE INDEX `I_ContactChangeHistory_tenantGuid_contactId` ON `ContactChangeHistory` (`tenantGuid`, `contactId`, `versionNumber`, `timeStamp`, `userId`);


-- The contact Tag data
CREATE TABLE `ContactTag`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`contactId` INT NOT NULL,		-- Link to the Contact table.
	`tagId` INT NOT NULL,		-- Link to the Tag table.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`contactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`tagId`) REFERENCES `Tag`(`id`)		-- Foreign key to the Tag table.
);
-- Index on the ContactTag table's tenantGuid field.
CREATE INDEX `I_ContactTag_tenantGuid` ON `ContactTag` (`tenantGuid`);

-- Index on the ContactTag table's tenantGuid,contactId fields.
CREATE INDEX `I_ContactTag_tenantGuid_contactId` ON `ContactTag` (`tenantGuid`, `contactId`);

-- Index on the ContactTag table's tenantGuid,tagId fields.
CREATE INDEX `I_ContactTag_tenantGuid_tagId` ON `ContactTag` (`tenantGuid`, `tagId`);

-- Index on the ContactTag table's tenantGuid,active fields.
CREATE INDEX `I_ContactTag_tenantGuid_active` ON `ContactTag` (`tenantGuid`, `active`);

-- Index on the ContactTag table's tenantGuid,deleted fields.
CREATE INDEX `I_ContactTag_tenantGuid_deleted` ON `ContactTag` (`tenantGuid`, `deleted`);


-- The change history for records from the ContactTag table.
CREATE TABLE `ContactTagChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`contactTagId` INT NOT NULL,		-- Link to the ContactTag table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`contactTagId`) REFERENCES `ContactTag`(`id`)		-- Foreign key to the ContactTag table.
);
-- Index on the ContactTagChangeHistory table's tenantGuid field.
CREATE INDEX `I_ContactTagChangeHistory_tenantGuid` ON `ContactTagChangeHistory` (`tenantGuid`);

-- Index on the ContactTagChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ContactTagChangeHistory_tenantGuid_versionNumber` ON `ContactTagChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ContactTagChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ContactTagChangeHistory_tenantGuid_timeStamp` ON `ContactTagChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ContactTagChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ContactTagChangeHistory_tenantGuid_userId` ON `ContactTagChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ContactTagChangeHistory table's tenantGuid,contactTagId fields.
CREATE INDEX `I_ContactTagChangeHistory_tenantGuid_contactTagId` ON `ContactTagChangeHistory` (`tenantGuid`, `contactTagId`, `versionNumber`, `timeStamp`, `userId`);


-- Master list of relationship types
CREATE TABLE `RelationshipType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`isEmergencyEligible` BIT NOT NULL DEFAULT 0,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`)		-- Foreign key to the Icon table.
);
-- Index on the RelationshipType table's name field.
CREATE INDEX `I_RelationshipType_name` ON `RelationshipType` (`name`);

-- Index on the RelationshipType table's iconId field.
CREATE INDEX `I_RelationshipType_iconId` ON `RelationshipType` (`iconId`);

-- Index on the RelationshipType table's active field.
CREATE INDEX `I_RelationshipType_active` ON `RelationshipType` (`active`);

-- Index on the RelationshipType table's deleted field.
CREATE INDEX `I_RelationshipType_deleted` ON `RelationshipType` (`deleted`);

INSERT INTO `RelationshipType` ( `name`, `description`, `isEmergencyEligible`, `sequence`, `objectGuid` ) VALUES  ( 'Self', 'Self', 0, 1, '3d4ec50a-552b-4826-9f7c-a27915134a21' );

INSERT INTO `RelationshipType` ( `name`, `description`, `isEmergencyEligible`, `sequence`, `objectGuid` ) VALUES  ( 'Colleague', 'Colleague', 0, 2, '968a530e-2ec8-449a-b2fa-e853bb82b2c2' );

INSERT INTO `RelationshipType` ( `name`, `description`, `isEmergencyEligible`, `sequence`, `objectGuid` ) VALUES  ( 'Spouse', 'Husband/Wife/Partner', 1, 3, 'e0020ae1-4b49-4d3e-a5a1-67f96ca239c8' );

INSERT INTO `RelationshipType` ( `name`, `description`, `isEmergencyEligible`, `sequence`, `objectGuid` ) VALUES  ( 'Parent', 'Mother/Father', 1, 4, '8622604b-c5d5-4363-9d63-b0c34f3facb2' );

INSERT INTO `RelationshipType` ( `name`, `description`, `isEmergencyEligible`, `sequence`, `objectGuid` ) VALUES  ( 'Child', 'Son/Daughter', 1, 5, 'd35f8329-f18b-445d-8404-0c8fafd9c43b' );

INSERT INTO `RelationshipType` ( `name`, `description`, `isEmergencyEligible`, `sequence`, `objectGuid` ) VALUES  ( 'Sibling', 'Brother/Sister', 1, 6, '07ed8aa5-9034-4cad-b8cc-c5564c5945d9' );

INSERT INTO `RelationshipType` ( `name`, `description`, `isEmergencyEligible`, `sequence`, `objectGuid` ) VALUES  ( 'Friend', 'Close friend', 1, 7, '57a2e1c3-d06e-48cf-aca5-fe5f396e968f' );

INSERT INTO `RelationshipType` ( `name`, `description`, `isEmergencyEligible`, `sequence`, `objectGuid` ) VALUES  ( 'Supervisor', 'Direct manager', 0, 8, '4f51e255-4c2c-41c5-92d9-b051d7d1b15a' );

INSERT INTO `RelationshipType` ( `name`, `description`, `isEmergencyEligible`, `sequence`, `objectGuid` ) VALUES  ( 'Mentor', 'Mentor', 0, 9, 'acfdbb6a-bc68-4753-990c-001c9800c155' );

INSERT INTO `RelationshipType` ( `name`, `description`, `isEmergencyEligible`, `sequence`, `objectGuid` ) VALUES  ( 'Mechanic', 'Equipment Maintenance', 0, 10, '3108554f-3943-4b8c-a196-ee8154cf9918' );

INSERT INTO `RelationshipType` ( `name`, `description`, `isEmergencyEligible`, `sequence`, `objectGuid` ) VALUES  ( 'Resident', 'Resident', 1, 11, '1b92d6de-a154-419e-a3dc-2f0186f029de' );

INSERT INTO `RelationshipType` ( `name`, `description`, `isEmergencyEligible`, `sequence`, `objectGuid` ) VALUES  ( 'Owner', 'Owner', 1, 12, 'e603de2c-8f55-44bb-9198-eaa1c1808498' );

INSERT INTO `RelationshipType` ( `name`, `description`, `isEmergencyEligible`, `sequence`, `objectGuid` ) VALUES  ( 'Other', 'Other relationship', 0, 99, 'b0fc78e9-ca52-4fdc-823f-0339e11dc069' );


-- The link between a contact and other contacts.
CREATE TABLE `ContactContact`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`contactId` INT NOT NULL,		-- Link to the Contact table.
	`relatedContactId` INT NOT NULL,		-- Link to the Contact table.
	`isPrimary` BIT NOT NULL DEFAULT 0,		-- Indicates whether or not this contact should be considered a primary contact of the contact.
	`relationshipTypeId` INT NOT NULL,		-- A description of the relationship between the contact and the contact.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`contactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`relatedContactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`relationshipTypeId`) REFERENCES `RelationshipType`(`id`),		-- Foreign key to the RelationshipType table.
	UNIQUE `UC_ContactContact_tenantGuid_contactId_relatedContactId_Unique`( `tenantGuid`, `contactId`, `relatedContactId` ) 		-- Uniqueness enforced on the ContactContact table's tenantGuid and contactId and relatedContactId fields.
);
-- Index on the ContactContact table's tenantGuid field.
CREATE INDEX `I_ContactContact_tenantGuid` ON `ContactContact` (`tenantGuid`);

-- Index on the ContactContact table's tenantGuid,contactId fields.
CREATE INDEX `I_ContactContact_tenantGuid_contactId` ON `ContactContact` (`tenantGuid`, `contactId`);

-- Index on the ContactContact table's tenantGuid,relatedContactId fields.
CREATE INDEX `I_ContactContact_tenantGuid_relatedContactId` ON `ContactContact` (`tenantGuid`, `relatedContactId`);

-- Index on the ContactContact table's tenantGuid,relationshipTypeId fields.
CREATE INDEX `I_ContactContact_tenantGuid_relationshipTypeId` ON `ContactContact` (`tenantGuid`, `relationshipTypeId`);

-- Index on the ContactContact table's tenantGuid,active fields.
CREATE INDEX `I_ContactContact_tenantGuid_active` ON `ContactContact` (`tenantGuid`, `active`);

-- Index on the ContactContact table's tenantGuid,deleted fields.
CREATE INDEX `I_ContactContact_tenantGuid_deleted` ON `ContactContact` (`tenantGuid`, `deleted`);


-- The change history for records from the ContactContact table.
CREATE TABLE `ContactContactChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`contactContactId` INT NOT NULL,		-- Link to the ContactContact table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`contactContactId`) REFERENCES `ContactContact`(`id`)		-- Foreign key to the ContactContact table.
);
-- Index on the ContactContactChangeHistory table's tenantGuid field.
CREATE INDEX `I_ContactContactChangeHistory_tenantGuid` ON `ContactContactChangeHistory` (`tenantGuid`);

-- Index on the ContactContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ContactContactChangeHistory_tenantGuid_versionNumber` ON `ContactContactChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ContactContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ContactContactChangeHistory_tenantGuid_timeStamp` ON `ContactContactChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ContactContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ContactContactChangeHistory_tenantGuid_userId` ON `ContactContactChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ContactContactChangeHistory table's tenantGuid,contactContactId fields.
CREATE INDEX `I_ContactContactChangeHistory_tenantGuid_contactContactId` ON `ContactContactChangeHistory` (`tenantGuid`, `contactContactId`, `versionNumber`, `timeStamp`, `userId`);


-- Master list of office types
CREATE TABLE `OfficeType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`)		-- Foreign key to the Icon table.
);
-- Index on the OfficeType table's name field.
CREATE INDEX `I_OfficeType_name` ON `OfficeType` (`name`);

-- Index on the OfficeType table's iconId field.
CREATE INDEX `I_OfficeType_iconId` ON `OfficeType` (`iconId`);

-- Index on the OfficeType table's active field.
CREATE INDEX `I_OfficeType_active` ON `OfficeType` (`active`);

-- Index on the OfficeType table's deleted field.
CREATE INDEX `I_OfficeType_deleted` ON `OfficeType` (`deleted`);

INSERT INTO `OfficeType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Headquarters ', 'Headquarters', 1, '3dc56597-1ab7-403e-bad9-8bd52c674f9d' );

INSERT INTO `OfficeType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Regional Office', 'Regional Office', 2, 'f28b5678-de69-43a3-9a9e-7194df40ea32' );

INSERT INTO `OfficeType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Branch Office', 'Branch Office', 3, 'd504aef3-b582-4f6d-91c8-b76142f5462a' );

INSERT INTO `OfficeType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Depot / Yard', 'Depot / Yard', 4, '98b72f2e-de47-4268-885e-3ab7a63e9e8c' );

INSERT INTO `OfficeType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Administrative Office', 'Administrative Office', 5, 'edc174d4-66f3-410f-a173-b15007c1ff48' );

INSERT INTO `OfficeType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Warehouse', 'Warehouse', 6, 'c595846a-c3f3-4e07-9df0-af117fa5a400' );

INSERT INTO `OfficeType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Hospital', 'Hospital', 7, '52a134df-ff0c-4391-ac85-93be54e9541b' );

INSERT INTO `OfficeType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Clinic', 'Clinic', 8, '9bd149c1-ca03-49c1-a71f-7d8479697205' );


-- The main list of offices operated by an organization using the Scheduler.  Allows schedule and resource grouping.
CREATE TABLE `Office`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`officeTypeId` INT NOT NULL,		-- Link to the OfficeType table.
	`timeZoneId` INT NOT NULL,		-- Time zone of the office.
	`currencyId` INT NOT NULL,		-- Default billing currency of the office.
	`addressLine1` VARCHAR(250) NOT NULL,
	`addressLine2` VARCHAR(250) NULL,
	`city` VARCHAR(100) NOT NULL,
	`postalCode` VARCHAR(100) NULL,
	`stateProvinceId` INT NOT NULL,		-- Link to the StateProvince table.
	`countryId` INT NOT NULL,		-- Link to the Country table.
	`phone` VARCHAR(100) NULL,
	`email` VARCHAR(250) NULL,
	`latitude` DOUBLE NULL,		-- Optional latitude position
	`longitude` DOUBLE NULL,		-- Optional longitude position
	`notes` TEXT NULL,
	`externalId` VARCHAR(100) NULL,		-- Optional reference to an ID in an external system 
	`color` VARCHAR(10) NULL,		-- Override of Target Type Hex color for UI display
	`attributes` TEXT NULL,		-- to store arbitrary JSON
	`avatarFileName` VARCHAR(250) NULL,		-- Part of the binary data field setup
	`avatarSize` BIGINT NULL,		-- Part of the binary data field setup
	`avatarData` BLOB NULL,		-- Part of the binary data field setup
	`avatarMimeType` VARCHAR(100) NULL,		-- Part of the binary data field setup
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`officeTypeId`) REFERENCES `OfficeType`(`id`),		-- Foreign key to the OfficeType table.
	FOREIGN KEY (`timeZoneId`) REFERENCES `TimeZone`(`id`),		-- Foreign key to the TimeZone table.
	FOREIGN KEY (`currencyId`) REFERENCES `Currency`(`id`),		-- Foreign key to the Currency table.
	FOREIGN KEY (`stateProvinceId`) REFERENCES `StateProvince`(`id`),		-- Foreign key to the StateProvince table.
	FOREIGN KEY (`countryId`) REFERENCES `Country`(`id`),		-- Foreign key to the Country table.
	UNIQUE `UC_Office_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the Office table's tenantGuid and name fields.
);
-- Index on the Office table's tenantGuid field.
CREATE INDEX `I_Office_tenantGuid` ON `Office` (`tenantGuid`);

-- Index on the Office table's tenantGuid,name fields.
CREATE INDEX `I_Office_tenantGuid_name` ON `Office` (`tenantGuid`, `name`);

-- Index on the Office table's tenantGuid,officeTypeId fields.
CREATE INDEX `I_Office_tenantGuid_officeTypeId` ON `Office` (`tenantGuid`, `officeTypeId`);

-- Index on the Office table's tenantGuid,timeZoneId fields.
CREATE INDEX `I_Office_tenantGuid_timeZoneId` ON `Office` (`tenantGuid`, `timeZoneId`);

-- Index on the Office table's tenantGuid,currencyId fields.
CREATE INDEX `I_Office_tenantGuid_currencyId` ON `Office` (`tenantGuid`, `currencyId`);

-- Index on the Office table's tenantGuid,stateProvinceId fields.
CREATE INDEX `I_Office_tenantGuid_stateProvinceId` ON `Office` (`tenantGuid`, `stateProvinceId`);

-- Index on the Office table's tenantGuid,countryId fields.
CREATE INDEX `I_Office_tenantGuid_countryId` ON `Office` (`tenantGuid`, `countryId`);

-- Index on the Office table's tenantGuid,email fields.
CREATE UNIQUE INDEX `I_Office_tenantGuid_email` ON `Office` (`tenantGuid`, `email`);
 WHERE `email` IS NOT NULL

-- Index on the Office table's tenantGuid,active fields.
CREATE INDEX `I_Office_tenantGuid_active` ON `Office` (`tenantGuid`, `active`);

-- Index on the Office table's tenantGuid,deleted fields.
CREATE INDEX `I_Office_tenantGuid_deleted` ON `Office` (`tenantGuid`, `deleted`);


-- The change history for records from the Office table.
CREATE TABLE `OfficeChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`officeId` INT NOT NULL,		-- Link to the Office table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`officeId`) REFERENCES `Office`(`id`)		-- Foreign key to the Office table.
);
-- Index on the OfficeChangeHistory table's tenantGuid field.
CREATE INDEX `I_OfficeChangeHistory_tenantGuid` ON `OfficeChangeHistory` (`tenantGuid`);

-- Index on the OfficeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_OfficeChangeHistory_tenantGuid_versionNumber` ON `OfficeChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the OfficeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_OfficeChangeHistory_tenantGuid_timeStamp` ON `OfficeChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the OfficeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_OfficeChangeHistory_tenantGuid_userId` ON `OfficeChangeHistory` (`tenantGuid`, `userId`);

-- Index on the OfficeChangeHistory table's tenantGuid,officeId fields.
CREATE INDEX `I_OfficeChangeHistory_tenantGuid_officeId` ON `OfficeChangeHistory` (`tenantGuid`, `officeId`, `versionNumber`, `timeStamp`, `userId`);


-- The link between contacts and offices.
CREATE TABLE `OfficeContact`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`officeId` INT NOT NULL,		-- Link to the Office table.
	`contactId` INT NOT NULL,		-- Link to the Contact table.
	`isPrimary` BIT NOT NULL DEFAULT 0,		-- Indicates whether or not this contact should be considered a primary contact of the office.
	`relationshipTypeId` INT NOT NULL,		-- A description of the relationship between the office and the contact.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`officeId`) REFERENCES `Office`(`id`),		-- Foreign key to the Office table.
	FOREIGN KEY (`contactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`relationshipTypeId`) REFERENCES `RelationshipType`(`id`),		-- Foreign key to the RelationshipType table.
	UNIQUE `UC_OfficeContact_tenantGuid_officeId_contactId_Unique`( `tenantGuid`, `officeId`, `contactId` ) 		-- Uniqueness enforced on the OfficeContact table's tenantGuid and officeId and contactId fields.
);
-- Index on the OfficeContact table's tenantGuid field.
CREATE INDEX `I_OfficeContact_tenantGuid` ON `OfficeContact` (`tenantGuid`);

-- Index on the OfficeContact table's tenantGuid,officeId fields.
CREATE INDEX `I_OfficeContact_tenantGuid_officeId` ON `OfficeContact` (`tenantGuid`, `officeId`);

-- Index on the OfficeContact table's tenantGuid,contactId fields.
CREATE INDEX `I_OfficeContact_tenantGuid_contactId` ON `OfficeContact` (`tenantGuid`, `contactId`);

-- Index on the OfficeContact table's tenantGuid,relationshipTypeId fields.
CREATE INDEX `I_OfficeContact_tenantGuid_relationshipTypeId` ON `OfficeContact` (`tenantGuid`, `relationshipTypeId`);

-- Index on the OfficeContact table's tenantGuid,active fields.
CREATE INDEX `I_OfficeContact_tenantGuid_active` ON `OfficeContact` (`tenantGuid`, `active`);

-- Index on the OfficeContact table's tenantGuid,deleted fields.
CREATE INDEX `I_OfficeContact_tenantGuid_deleted` ON `OfficeContact` (`tenantGuid`, `deleted`);


-- The change history for records from the OfficeContact table.
CREATE TABLE `OfficeContactChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`officeContactId` INT NOT NULL,		-- Link to the OfficeContact table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`officeContactId`) REFERENCES `OfficeContact`(`id`)		-- Foreign key to the OfficeContact table.
);
-- Index on the OfficeContactChangeHistory table's tenantGuid field.
CREATE INDEX `I_OfficeContactChangeHistory_tenantGuid` ON `OfficeContactChangeHistory` (`tenantGuid`);

-- Index on the OfficeContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_OfficeContactChangeHistory_tenantGuid_versionNumber` ON `OfficeContactChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the OfficeContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_OfficeContactChangeHistory_tenantGuid_timeStamp` ON `OfficeContactChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the OfficeContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_OfficeContactChangeHistory_tenantGuid_userId` ON `OfficeContactChangeHistory` (`tenantGuid`, `userId`);

-- Index on the OfficeContactChangeHistory table's tenantGuid,officeContactId fields.
CREATE INDEX `I_OfficeContactChangeHistory_tenantGuid_officeContactId` ON `OfficeContactChangeHistory` (`tenantGuid`, `officeContactId`, `versionNumber`, `timeStamp`, `userId`);


-- Optional logical grouping of events for visibility and filtering (e.g., '2026 Road Projects', 'Maintenance Calendar').
CREATE TABLE `Calendar`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`officeId` INT NULL,		-- Optional office binding for the calendar
	`isDefault` BIT NULL,
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	FOREIGN KEY (`officeId`) REFERENCES `Office`(`id`),		-- Foreign key to the Office table.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_Calendar_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the Calendar table's tenantGuid and name fields.
);
-- Index on the Calendar table's tenantGuid field.
CREATE INDEX `I_Calendar_tenantGuid` ON `Calendar` (`tenantGuid`);

-- Index on the Calendar table's tenantGuid,name fields.
CREATE INDEX `I_Calendar_tenantGuid_name` ON `Calendar` (`tenantGuid`, `name`);

-- Index on the Calendar table's tenantGuid,officeId fields.
CREATE INDEX `I_Calendar_tenantGuid_officeId` ON `Calendar` (`tenantGuid`, `officeId`);

-- Index on the Calendar table's tenantGuid,iconId fields.
CREATE INDEX `I_Calendar_tenantGuid_iconId` ON `Calendar` (`tenantGuid`, `iconId`);

-- Index on the Calendar table's tenantGuid,active fields.
CREATE INDEX `I_Calendar_tenantGuid_active` ON `Calendar` (`tenantGuid`, `active`);

-- Index on the Calendar table's tenantGuid,deleted fields.
CREATE INDEX `I_Calendar_tenantGuid_deleted` ON `Calendar` (`tenantGuid`, `deleted`);


-- The change history for records from the Calendar table.
CREATE TABLE `CalendarChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`calendarId` INT NOT NULL,		-- Link to the Calendar table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`calendarId`) REFERENCES `Calendar`(`id`)		-- Foreign key to the Calendar table.
);
-- Index on the CalendarChangeHistory table's tenantGuid field.
CREATE INDEX `I_CalendarChangeHistory_tenantGuid` ON `CalendarChangeHistory` (`tenantGuid`);

-- Index on the CalendarChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_CalendarChangeHistory_tenantGuid_versionNumber` ON `CalendarChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the CalendarChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_CalendarChangeHistory_tenantGuid_timeStamp` ON `CalendarChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the CalendarChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_CalendarChangeHistory_tenantGuid_userId` ON `CalendarChangeHistory` (`tenantGuid`, `userId`);

-- Index on the CalendarChangeHistory table's tenantGuid,calendarId fields.
CREATE INDEX `I_CalendarChangeHistory_tenantGuid_calendarId` ON `CalendarChangeHistory` (`tenantGuid`, `calendarId`, `versionNumber`, `timeStamp`, `userId`);


-- Master list of client types.  Used for categorizing clients.
CREATE TABLE `ClientType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_ClientType_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the ClientType table's tenantGuid and name fields.
);
-- Index on the ClientType table's tenantGuid field.
CREATE INDEX `I_ClientType_tenantGuid` ON `ClientType` (`tenantGuid`);

-- Index on the ClientType table's tenantGuid,name fields.
CREATE INDEX `I_ClientType_tenantGuid_name` ON `ClientType` (`tenantGuid`, `name`);

-- Index on the ClientType table's tenantGuid,iconId fields.
CREATE INDEX `I_ClientType_tenantGuid_iconId` ON `ClientType` (`tenantGuid`, `iconId`);

-- Index on the ClientType table's tenantGuid,active fields.
CREATE INDEX `I_ClientType_tenantGuid_active` ON `ClientType` (`tenantGuid`, `active`);

-- Index on the ClientType table's tenantGuid,deleted fields.
CREATE INDEX `I_ClientType_tenantGuid_deleted` ON `ClientType` (`tenantGuid`, `deleted`);

INSERT INTO `ClientType` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Construction ', 'A construction client', 1, '331c07c6-bcd1-4d8d-b796-d81216bba704' );

INSERT INTO `ClientType` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Healthcare', 'A healthcare client', 2, '701001e4-4034-4b18-ab29-b514b08bc541' );


-- The main client list.  Is not directly schedulable, but provides billing details.  Contains scheduling targets which are schedulable.
CREATE TABLE `Client`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`clientTypeId` INT NOT NULL,		-- Link to the ClientType table.
	`currencyId` INT NOT NULL,		-- Link to the Currency table.
	`timeZoneId` INT NOT NULL,		-- Link to the TimeZone table.
	`calendarId` INT NULL,		-- An optional default calendar for the scheduling target's belonging to the client.
	`addressLine1` VARCHAR(250) NOT NULL,
	`addressLine2` VARCHAR(250) NULL,
	`city` VARCHAR(100) NOT NULL,
	`postalCode` VARCHAR(100) NULL,
	`stateProvinceId` INT NOT NULL,		-- Link to the StateProvince table.
	`countryId` INT NOT NULL,		-- Link to the Country table.
	`phone` VARCHAR(100) NULL,
	`email` VARCHAR(250) NULL,
	`latitude` DOUBLE NULL,		-- Optional latitude position
	`longitude` DOUBLE NULL,		-- Optional longitude position
	`notes` TEXT NULL,
	`externalId` VARCHAR(100) NULL,		-- Optional reference to an ID in an external system
	`color` VARCHAR(10) NULL,		-- Override of Target Type Hex color for UI display
	`attributes` TEXT NULL,		-- to store arbitrary JSON
	`avatarFileName` VARCHAR(250) NULL,		-- Part of the binary data field setup
	`avatarSize` BIGINT NULL,		-- Part of the binary data field setup
	`avatarData` BLOB NULL,		-- Part of the binary data field setup
	`avatarMimeType` VARCHAR(100) NULL,		-- Part of the binary data field setup
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`clientTypeId`) REFERENCES `ClientType`(`id`),		-- Foreign key to the ClientType table.
	FOREIGN KEY (`currencyId`) REFERENCES `Currency`(`id`),		-- Foreign key to the Currency table.
	FOREIGN KEY (`timeZoneId`) REFERENCES `TimeZone`(`id`),		-- Foreign key to the TimeZone table.
	FOREIGN KEY (`calendarId`) REFERENCES `Calendar`(`id`),		-- Foreign key to the Calendar table.
	FOREIGN KEY (`stateProvinceId`) REFERENCES `StateProvince`(`id`),		-- Foreign key to the StateProvince table.
	FOREIGN KEY (`countryId`) REFERENCES `Country`(`id`),		-- Foreign key to the Country table.
	UNIQUE `UC_Client_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the Client table's tenantGuid and name fields.
);
-- Index on the Client table's tenantGuid field.
CREATE INDEX `I_Client_tenantGuid` ON `Client` (`tenantGuid`);

-- Index on the Client table's tenantGuid,name fields.
CREATE INDEX `I_Client_tenantGuid_name` ON `Client` (`tenantGuid`, `name`);

-- Index on the Client table's tenantGuid,clientTypeId fields.
CREATE INDEX `I_Client_tenantGuid_clientTypeId` ON `Client` (`tenantGuid`, `clientTypeId`);

-- Index on the Client table's tenantGuid,currencyId fields.
CREATE INDEX `I_Client_tenantGuid_currencyId` ON `Client` (`tenantGuid`, `currencyId`);

-- Index on the Client table's tenantGuid,timeZoneId fields.
CREATE INDEX `I_Client_tenantGuid_timeZoneId` ON `Client` (`tenantGuid`, `timeZoneId`);

-- Index on the Client table's tenantGuid,stateProvinceId fields.
CREATE INDEX `I_Client_tenantGuid_stateProvinceId` ON `Client` (`tenantGuid`, `stateProvinceId`);

-- Index on the Client table's tenantGuid,countryId fields.
CREATE INDEX `I_Client_tenantGuid_countryId` ON `Client` (`tenantGuid`, `countryId`);

-- emails must be unique to one Client.
CREATE UNIQUE INDEX `I_Client_tenantGuid_email` ON `Client` (`tenantGuid`, `email`);
 WHERE `email` IS NOT NULL

-- Index on the Client table's tenantGuid,active fields.
CREATE INDEX `I_Client_tenantGuid_active` ON `Client` (`tenantGuid`, `active`);

-- Index on the Client table's tenantGuid,deleted fields.
CREATE INDEX `I_Client_tenantGuid_deleted` ON `Client` (`tenantGuid`, `deleted`);


-- The change history for records from the Client table.
CREATE TABLE `ClientChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`clientId` INT NOT NULL,		-- Link to the Client table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`clientId`) REFERENCES `Client`(`id`)		-- Foreign key to the Client table.
);
-- Index on the ClientChangeHistory table's tenantGuid field.
CREATE INDEX `I_ClientChangeHistory_tenantGuid` ON `ClientChangeHistory` (`tenantGuid`);

-- Index on the ClientChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ClientChangeHistory_tenantGuid_versionNumber` ON `ClientChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ClientChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ClientChangeHistory_tenantGuid_timeStamp` ON `ClientChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ClientChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ClientChangeHistory_tenantGuid_userId` ON `ClientChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ClientChangeHistory table's tenantGuid,clientId fields.
CREATE INDEX `I_ClientChangeHistory_tenantGuid_clientId` ON `ClientChangeHistory` (`tenantGuid`, `clientId`, `versionNumber`, `timeStamp`, `userId`);


-- The link between contacts and clients.
CREATE TABLE `ClientContact`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`clientId` INT NOT NULL,		-- Link to the Client table.
	`contactId` INT NOT NULL,		-- Link to the Contact table.
	`isPrimary` BIT NOT NULL DEFAULT 0,		-- Indicates whether or not this contact should be considered a primary contact of the client.
	`relationshipTypeId` INT NOT NULL,		-- A description of the relationship between the client and the contact.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`clientId`) REFERENCES `Client`(`id`),		-- Foreign key to the Client table.
	FOREIGN KEY (`contactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`relationshipTypeId`) REFERENCES `RelationshipType`(`id`),		-- Foreign key to the RelationshipType table.
	UNIQUE `UC_ClientContact_tenantGuid_clientId_contactId_Unique`( `tenantGuid`, `clientId`, `contactId` ) 		-- Uniqueness enforced on the ClientContact table's tenantGuid and clientId and contactId fields.
);
-- Index on the ClientContact table's tenantGuid field.
CREATE INDEX `I_ClientContact_tenantGuid` ON `ClientContact` (`tenantGuid`);

-- Index on the ClientContact table's tenantGuid,clientId fields.
CREATE INDEX `I_ClientContact_tenantGuid_clientId` ON `ClientContact` (`tenantGuid`, `clientId`);

-- Index on the ClientContact table's tenantGuid,contactId fields.
CREATE INDEX `I_ClientContact_tenantGuid_contactId` ON `ClientContact` (`tenantGuid`, `contactId`);

-- Index on the ClientContact table's tenantGuid,relationshipTypeId fields.
CREATE INDEX `I_ClientContact_tenantGuid_relationshipTypeId` ON `ClientContact` (`tenantGuid`, `relationshipTypeId`);

-- Index on the ClientContact table's tenantGuid,active fields.
CREATE INDEX `I_ClientContact_tenantGuid_active` ON `ClientContact` (`tenantGuid`, `active`);

-- Index on the ClientContact table's tenantGuid,deleted fields.
CREATE INDEX `I_ClientContact_tenantGuid_deleted` ON `ClientContact` (`tenantGuid`, `deleted`);


-- The change history for records from the ClientContact table.
CREATE TABLE `ClientContactChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`clientContactId` INT NOT NULL,		-- Link to the ClientContact table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`clientContactId`) REFERENCES `ClientContact`(`id`)		-- Foreign key to the ClientContact table.
);
-- Index on the ClientContactChangeHistory table's tenantGuid field.
CREATE INDEX `I_ClientContactChangeHistory_tenantGuid` ON `ClientContactChangeHistory` (`tenantGuid`);

-- Index on the ClientContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ClientContactChangeHistory_tenantGuid_versionNumber` ON `ClientContactChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ClientContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ClientContactChangeHistory_tenantGuid_timeStamp` ON `ClientContactChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ClientContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ClientContactChangeHistory_tenantGuid_userId` ON `ClientContactChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ClientContactChangeHistory table's tenantGuid,clientContactId fields.
CREATE INDEX `I_ClientContactChangeHistory_tenantGuid_clientContactId` ON `ClientContactChangeHistory` (`tenantGuid`, `clientContactId`, `versionNumber`, `timeStamp`, `userId`);


-- Tenant-level information. Client admins manage this data.
CREATE TABLE `TenantProfile`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`companyLogoFileName` VARCHAR(250) NULL,		-- Part of the binary data field setup
	`companyLogoSize` BIGINT NULL,		-- Part of the binary data field setup
	`companyLogoData` BLOB NULL,		-- Part of the binary data field setup
	`companyLogoMimeType` VARCHAR(100) NULL,		-- Part of the binary data field setup
	`addressLine1` VARCHAR(250) NULL,
	`addressLine2` VARCHAR(250) NULL,
	`addressLine3` VARCHAR(250) NULL,
	`city` VARCHAR(100) NULL,
	`postalCode` VARCHAR(100) NULL,
	`stateProvinceId` INT NULL,		-- Link to the StateProvince table.
	`countryId` INT NULL,		-- Link to the Country table.
	`timeZoneId` INT NULL,		-- Link to the TimeZone table.
	`phoneNumber` VARCHAR(100) NULL,
	`email` VARCHAR(250) NULL,
	`website` VARCHAR(1000) NULL,
	`latitude` DOUBLE NULL,		-- Optional latitude position
	`longitude` DOUBLE NULL,		-- Optional longitude position
	`primaryColor` VARCHAR(10) NULL,
	`secondaryColor` VARCHAR(10) NULL,
	`displaysMetric` BIT NOT NULL DEFAULT 0,		-- True if the tenant defaults to using metric units when creating projects.    Note that this does not affect the storage units, which are always metric.
	`displaysUSTerms` BIT NOT NULL DEFAULT 0,		-- True if the tenant defaults to using terms for the US market, such as Zip code,.
	`invoiceNumberMask` VARCHAR(100) NULL,		-- Format mask for auto-generating invoice numbers. Supports {YYYY} for year and {NNNN} for zero-padded sequence. Default: INV-{YYYY}-{NNNN}.
	`receiptNumberMask` VARCHAR(100) NULL,		-- Format mask for auto-generating receipt numbers. Supports {YYYY} for year and {NNNN} for zero-padded sequence. Default: REC-{YYYY}-{NNNN}.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`stateProvinceId`) REFERENCES `StateProvince`(`id`),		-- Foreign key to the StateProvince table.
	FOREIGN KEY (`countryId`) REFERENCES `Country`(`id`),		-- Foreign key to the Country table.
	FOREIGN KEY (`timeZoneId`) REFERENCES `TimeZone`(`id`),		-- Foreign key to the TimeZone table.
	UNIQUE `UC_TenantProfile_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the TenantProfile table's tenantGuid and name fields.
);
-- Index on the TenantProfile table's tenantGuid field.
CREATE INDEX `I_TenantProfile_tenantGuid` ON `TenantProfile` (`tenantGuid`);

-- Index on the TenantProfile table's tenantGuid,name fields.
CREATE INDEX `I_TenantProfile_tenantGuid_name` ON `TenantProfile` (`tenantGuid`, `name`);

-- Index on the TenantProfile table's tenantGuid,timeZoneId fields.
CREATE INDEX `I_TenantProfile_tenantGuid_timeZoneId` ON `TenantProfile` (`tenantGuid`, `timeZoneId`);

-- Index on the TenantProfile table's tenantGuid,active fields.
CREATE INDEX `I_TenantProfile_tenantGuid_active` ON `TenantProfile` (`tenantGuid`, `active`);

-- Index on the TenantProfile table's tenantGuid,deleted fields.
CREATE INDEX `I_TenantProfile_tenantGuid_deleted` ON `TenantProfile` (`tenantGuid`, `deleted`);


-- The change history for records from the TenantProfile table.
CREATE TABLE `TenantProfileChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`tenantProfileId` INT NOT NULL,		-- Link to the TenantProfile table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`tenantProfileId`) REFERENCES `TenantProfile`(`id`)		-- Foreign key to the TenantProfile table.
);
-- Index on the TenantProfileChangeHistory table's tenantGuid field.
CREATE INDEX `I_TenantProfileChangeHistory_tenantGuid` ON `TenantProfileChangeHistory` (`tenantGuid`);

-- Index on the TenantProfileChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_TenantProfileChangeHistory_tenantGuid_versionNumber` ON `TenantProfileChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the TenantProfileChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_TenantProfileChangeHistory_tenantGuid_timeStamp` ON `TenantProfileChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the TenantProfileChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_TenantProfileChangeHistory_tenantGuid_userId` ON `TenantProfileChangeHistory` (`tenantGuid`, `userId`);

-- Index on the TenantProfileChangeHistory table's tenantGuid,tenantProfileId fields.
CREATE INDEX `I_TenantProfileChangeHistory_tenantGuid_tenantProfileId` ON `TenantProfileChangeHistory` (`tenantGuid`, `tenantProfileId`, `versionNumber`, `timeStamp`, `userId`);


-- Master list of qualifications, certifications, or competencies required for certain work.  Examples: RN License, Crane Operator Certification, OSHA 30, Pediatric Specialty, Confined Space Entry.
CREATE TABLE `Qualification`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`isLicense` BIT NULL,		-- for special handling (e.g., expiry warnings)
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE `UC_Qualification_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the Qualification table's tenantGuid and name fields.
);
-- Index on the Qualification table's tenantGuid field.
CREATE INDEX `I_Qualification_tenantGuid` ON `Qualification` (`tenantGuid`);

-- Index on the Qualification table's tenantGuid,name fields.
CREATE INDEX `I_Qualification_tenantGuid_name` ON `Qualification` (`tenantGuid`, `name`);

-- Index on the Qualification table's tenantGuid,active fields.
CREATE INDEX `I_Qualification_tenantGuid_active` ON `Qualification` (`tenantGuid`, `active`);

-- Index on the Qualification table's tenantGuid,deleted fields.
CREATE INDEX `I_Qualification_tenantGuid_deleted` ON `Qualification` (`tenantGuid`, `deleted`);


-- Tenant-configurable roles that a resource can fulfil during an event.  Examples: Operator, Supervisor, Driver, Spotter, Safety Officer.  Used for business rule enforcement and richer reporting.
CREATE TABLE `AssignmentRole`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_AssignmentRole_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the AssignmentRole table's tenantGuid and name fields.
);
-- Index on the AssignmentRole table's tenantGuid field.
CREATE INDEX `I_AssignmentRole_tenantGuid` ON `AssignmentRole` (`tenantGuid`);

-- Index on the AssignmentRole table's tenantGuid,name fields.
CREATE INDEX `I_AssignmentRole_tenantGuid_name` ON `AssignmentRole` (`tenantGuid`, `name`);

-- Index on the AssignmentRole table's tenantGuid,iconId fields.
CREATE INDEX `I_AssignmentRole_tenantGuid_iconId` ON `AssignmentRole` (`tenantGuid`, `iconId`);

-- Index on the AssignmentRole table's tenantGuid,active fields.
CREATE INDEX `I_AssignmentRole_tenantGuid_active` ON `AssignmentRole` (`tenantGuid`, `active`);

-- Index on the AssignmentRole table's tenantGuid,deleted fields.
CREATE INDEX `I_AssignmentRole_tenantGuid_deleted` ON `AssignmentRole` (`tenantGuid`, `deleted`);

INSERT INTO `AssignmentRole` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Operator', 'Primary equipment operator', 1, 'b2c3d4e5-6789-0123-4567-89abcdef0001' );

INSERT INTO `AssignmentRole` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Supervisor', 'Site supervisor', 2, 'b2c3d4e5-6789-0123-4567-89abcdef0002' );

INSERT INTO `AssignmentRole` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Driver', 'Haul truck or service vehicle driver', 3, 'b2c3d4e5-6789-0123-4567-89abcdef0003' );

INSERT INTO `AssignmentRole` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Spotter', 'Safety spotter / banksman', 4, 'b2c3d4e5-6789-0123-4567-89abcdef0004' );


-- Defines which qualifications are required to fulfill a specific AssignmentRole.  This is the most common way to enforce certification requirements.
CREATE TABLE `AssignmentRoleQualificationRequirement`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`assignmentRoleId` INT NOT NULL,		-- Link to the AssignmentRole table.
	`qualificationId` INT NOT NULL,		-- Link to the Qualification table.
	`isRequired` BIT NOT NULL DEFAULT 1,		-- true = mandatory to fulfill role, false = preferred/recommended
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`assignmentRoleId`) REFERENCES `AssignmentRole`(`id`),		-- Foreign key to the AssignmentRole table.
	FOREIGN KEY (`qualificationId`) REFERENCES `Qualification`(`id`),		-- Foreign key to the Qualification table.
	UNIQUE `UC_AssignmentRoleQualificationRequirement_tenantGuid_assignmentRoleId_qualificationId_Unique`( `tenantGuid`, `assignmentRoleId`, `qualificationId` ) 		-- Uniqueness enforced on the AssignmentRoleQualificationRequirement table's tenantGuid and assignmentRoleId and qualificationId fields.
);
-- Index on the AssignmentRoleQualificationRequirement table's tenantGuid field.
CREATE INDEX `I_AssignmentRoleQualificationRequirement_tenantGuid` ON `AssignmentRoleQualificationRequirement` (`tenantGuid`);

-- Index on the AssignmentRoleQualificationRequirement table's tenantGuid,assignmentRoleId fields.
CREATE INDEX `I_AssignmentRoleQualificationRequirement_tenantGuid_assignmentRo` ON `AssignmentRoleQualificationRequirement` (`tenantGuid`, `assignmentRoleId`);

-- Index on the AssignmentRoleQualificationRequirement table's tenantGuid,qualificationId fields.
CREATE INDEX `I_AssignmentRoleQualificationRequirement_tenantGuid_qualificatio` ON `AssignmentRoleQualificationRequirement` (`tenantGuid`, `qualificationId`);

-- Index on the AssignmentRoleQualificationRequirement table's tenantGuid,active fields.
CREATE INDEX `I_AssignmentRoleQualificationRequirement_tenantGuid_active` ON `AssignmentRoleQualificationRequirement` (`tenantGuid`, `active`);

-- Index on the AssignmentRoleQualificationRequirement table's tenantGuid,deleted fields.
CREATE INDEX `I_AssignmentRoleQualificationRequirement_tenantGuid_deleted` ON `AssignmentRoleQualificationRequirement` (`tenantGuid`, `deleted`);


-- The change history for records from the AssignmentRoleQualificationRequirement table.
CREATE TABLE `AssignmentRoleQualificationRequirementChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`assignmentRoleQualificationRequirementId` INT NOT NULL,		-- Link to the AssignmentRoleQualificationRequirement table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`assignmentRoleQualificationRequirementId`) REFERENCES `AssignmentRoleQualificationRequirement`(`id`)		-- Foreign key to the AssignmentRoleQualificationRequirement table.
);
-- Index on the AssignmentRoleQualificationRequirementChangeHistory table's tenantGuid field.
CREATE INDEX `I_AssignmentRoleQualificationRequirementChangeHistory_tenantGuid` ON `AssignmentRoleQualificationRequirementChangeHistory` (`tenantGuid`);

-- Index on the AssignmentRoleQualificationRequirementChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_AssignmentRoleQualificationRequirementChangeHistory_tenantGuid` ON `AssignmentRoleQualificationRequirementChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the AssignmentRoleQualificationRequirementChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_AssignmentRoleQualificationRequirementChangeHistory_tenantGuid` ON `AssignmentRoleQualificationRequirementChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the AssignmentRoleQualificationRequirementChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_AssignmentRoleQualificationRequirementChangeHistory_tenantGuid` ON `AssignmentRoleQualificationRequirementChangeHistory` (`tenantGuid`, `userId`);

-- Index on the AssignmentRoleQualificationRequirementChangeHistory table's tenantGuid,assignmentRoleQualificationRequirementId fields.
CREATE INDEX `I_AssignmentRoleQualificationRequirementChangeHistory_tenantGuid` ON `AssignmentRoleQualificationRequirementChangeHistory` (`tenantGuid`, `assignmentRoleQualificationRequirementId`, `versionNumber`, `timeStamp`, `userId`);


-- Master list of event statuses (Planned, In Progress, Completed, No-Show, Canceled, etc.)
CREATE TABLE `EventStatus`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the EventStatus table's name field.
CREATE INDEX `I_EventStatus_name` ON `EventStatus` (`name`);

-- Index on the EventStatus table's active field.
CREATE INDEX `I_EventStatus_active` ON `EventStatus` (`active`);

-- Index on the EventStatus table's deleted field.
CREATE INDEX `I_EventStatus_deleted` ON `EventStatus` (`deleted`);

INSERT INTO `EventStatus` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Planned', 'Scheduled but not started', 1, '005bdc39-da8e-465a-a17e-78aafffb390a' );

INSERT INTO `EventStatus` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'In Progress', 'Started', 2, '513bd381-6ab9-407c-ac4d-9187f6f92e16' );

INSERT INTO `EventStatus` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Completed', 'Finished successfully', 3, '6af9e244-2eff-463b-a40c-821fe00fa644' );

INSERT INTO `EventStatus` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'No-Show', 'No Show', 4, 'd7e81b73-bbe3-42dd-bcf6-856a82b9fce1' );

INSERT INTO `EventStatus` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Canceled', 'Explicitly canceled', 5, '01148ccb-e746-4218-88c5-8f0a5ee36adc' );


-- Master list of payment types (credit card, cheque, cash, e-transfer, etc.)
CREATE TABLE `PaymentType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE `UC_PaymentType_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the PaymentType table's tenantGuid and name fields.
);
-- Index on the PaymentType table's tenantGuid field.
CREATE INDEX `I_PaymentType_tenantGuid` ON `PaymentType` (`tenantGuid`);

-- Index on the PaymentType table's tenantGuid,name fields.
CREATE INDEX `I_PaymentType_tenantGuid_name` ON `PaymentType` (`tenantGuid`, `name`);

-- Index on the PaymentType table's tenantGuid,active fields.
CREATE INDEX `I_PaymentType_tenantGuid_active` ON `PaymentType` (`tenantGuid`, `active`);

-- Index on the PaymentType table's tenantGuid,deleted fields.
CREATE INDEX `I_PaymentType_tenantGuid_deleted` ON `PaymentType` (`tenantGuid`, `deleted`);


-- The change history for records from the PaymentType table.
CREATE TABLE `PaymentTypeChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`paymentTypeId` INT NOT NULL,		-- Link to the PaymentType table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`paymentTypeId`) REFERENCES `PaymentType`(`id`)		-- Foreign key to the PaymentType table.
);
-- Index on the PaymentTypeChangeHistory table's tenantGuid field.
CREATE INDEX `I_PaymentTypeChangeHistory_tenantGuid` ON `PaymentTypeChangeHistory` (`tenantGuid`);

-- Index on the PaymentTypeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_PaymentTypeChangeHistory_tenantGuid_versionNumber` ON `PaymentTypeChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the PaymentTypeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_PaymentTypeChangeHistory_tenantGuid_timeStamp` ON `PaymentTypeChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the PaymentTypeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_PaymentTypeChangeHistory_tenantGuid_userId` ON `PaymentTypeChangeHistory` (`tenantGuid`, `userId`);

-- Index on the PaymentTypeChangeHistory table's tenantGuid,paymentTypeId fields.
CREATE INDEX `I_PaymentTypeChangeHistory_tenantGuid_paymentTypeId` ON `PaymentTypeChangeHistory` (`tenantGuid`, `paymentTypeId`, `versionNumber`, `timeStamp`, `userId`);


-- Master list of receipt types (Receipted, Do Not Receipt, etc.)
CREATE TABLE `ReceiptType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE `UC_ReceiptType_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the ReceiptType table's tenantGuid and name fields.
);
-- Index on the ReceiptType table's tenantGuid field.
CREATE INDEX `I_ReceiptType_tenantGuid` ON `ReceiptType` (`tenantGuid`);

-- Index on the ReceiptType table's tenantGuid,name fields.
CREATE INDEX `I_ReceiptType_tenantGuid_name` ON `ReceiptType` (`tenantGuid`, `name`);

-- Index on the ReceiptType table's tenantGuid,active fields.
CREATE INDEX `I_ReceiptType_tenantGuid_active` ON `ReceiptType` (`tenantGuid`, `active`);

-- Index on the ReceiptType table's tenantGuid,deleted fields.
CREATE INDEX `I_ReceiptType_tenantGuid_deleted` ON `ReceiptType` (`tenantGuid`, `deleted`);


-- The change history for records from the ReceiptType table.
CREATE TABLE `ReceiptTypeChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`receiptTypeId` INT NOT NULL,		-- Link to the ReceiptType table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`receiptTypeId`) REFERENCES `ReceiptType`(`id`)		-- Foreign key to the ReceiptType table.
);
-- Index on the ReceiptTypeChangeHistory table's tenantGuid field.
CREATE INDEX `I_ReceiptTypeChangeHistory_tenantGuid` ON `ReceiptTypeChangeHistory` (`tenantGuid`);

-- Index on the ReceiptTypeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ReceiptTypeChangeHistory_tenantGuid_versionNumber` ON `ReceiptTypeChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ReceiptTypeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ReceiptTypeChangeHistory_tenantGuid_timeStamp` ON `ReceiptTypeChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ReceiptTypeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ReceiptTypeChangeHistory_tenantGuid_userId` ON `ReceiptTypeChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ReceiptTypeChangeHistory table's tenantGuid,receiptTypeId fields.
CREATE INDEX `I_ReceiptTypeChangeHistory_tenantGuid_receiptTypeId` ON `ReceiptTypeChangeHistory` (`tenantGuid`, `receiptTypeId`, `versionNumber`, `timeStamp`, `userId`);


-- Master list of booking sources ( walk-in, phone, online)
CREATE TABLE `BookingSourceType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the BookingSourceType table's name field.
CREATE INDEX `I_BookingSourceType_name` ON `BookingSourceType` (`name`);

-- Index on the BookingSourceType table's active field.
CREATE INDEX `I_BookingSourceType_active` ON `BookingSourceType` (`active`);

-- Index on the BookingSourceType table's deleted field.
CREATE INDEX `I_BookingSourceType_deleted` ON `BookingSourceType` (`deleted`);

INSERT INTO `BookingSourceType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Administrative', 'Administrative', 1, '3ec3e46a-ece8-4364-8396-beaf23aa0a2a' );

INSERT INTO `BookingSourceType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Phone', 'Phone', 2, 'cb9c2d46-29d5-4caa-9d5c-9e84356edf86' );

INSERT INTO `BookingSourceType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Walk-in', 'Walk-in', 3, 'fc0a5ebf-794d-4e61-9dce-f308da9d9ba4' );

INSERT INTO `BookingSourceType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Online', 'Online', 4, '1955a3f1-adce-4bc4-99d1-86362ff98a57' );


-- Master list of assignment statuses (Planned, In Progress, Completed, No-Show, Canceled, etc.)
CREATE TABLE `AssignmentStatus`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the AssignmentStatus table's name field.
CREATE INDEX `I_AssignmentStatus_name` ON `AssignmentStatus` (`name`);

-- Index on the AssignmentStatus table's active field.
CREATE INDEX `I_AssignmentStatus_active` ON `AssignmentStatus` (`active`);

-- Index on the AssignmentStatus table's deleted field.
CREATE INDEX `I_AssignmentStatus_deleted` ON `AssignmentStatus` (`deleted`);

INSERT INTO `AssignmentStatus` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Planned', 'Scheduled but not started', 1, '82fff66d-f6b4-44fe-9892-c7415cd0d401' );

INSERT INTO `AssignmentStatus` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'In Progress', 'Started', 2, '34183a16-1a64-4106-b28e-db454b06b5a6' );

INSERT INTO `AssignmentStatus` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Completed', 'Finished successfully', 3, '765c3c6d-782b-4393-bdab-cbf2a4a34eb6' );

INSERT INTO `AssignmentStatus` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'No-Show', 'Patient/resource didn''t appear', 4, '121271a6-7d93-4460-909f-2dc6e618538f' );

INSERT INTO `AssignmentStatus` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Canceled', 'Explicitly canceled', 5, 'cb14a7ad-fe10-4b2b-996c-7b5598810608' );


-- Master list of scheduling target categories (e.g., Project, Patient, Customer). Used for UI grouping and filtering.
CREATE TABLE `SchedulingTargetType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_SchedulingTargetType_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the SchedulingTargetType table's tenantGuid and name fields.
);
-- Index on the SchedulingTargetType table's tenantGuid field.
CREATE INDEX `I_SchedulingTargetType_tenantGuid` ON `SchedulingTargetType` (`tenantGuid`);

-- Index on the SchedulingTargetType table's tenantGuid,name fields.
CREATE INDEX `I_SchedulingTargetType_tenantGuid_name` ON `SchedulingTargetType` (`tenantGuid`, `name`);

-- Index on the SchedulingTargetType table's tenantGuid,iconId fields.
CREATE INDEX `I_SchedulingTargetType_tenantGuid_iconId` ON `SchedulingTargetType` (`tenantGuid`, `iconId`);

-- Index on the SchedulingTargetType table's tenantGuid,active fields.
CREATE INDEX `I_SchedulingTargetType_tenantGuid_active` ON `SchedulingTargetType` (`tenantGuid`, `active`);

-- Index on the SchedulingTargetType table's tenantGuid,deleted fields.
CREATE INDEX `I_SchedulingTargetType_tenantGuid_deleted` ON `SchedulingTargetType` (`tenantGuid`, `deleted`);

INSERT INTO `SchedulingTargetType` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Construction Project', 'A construction job with one or more sites', 1, '0ceaf00d-c58f-48a6-a18e-9a3e07452a23' );

INSERT INTO `SchedulingTargetType` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Patient', 'Healthcare patient with multiple care locations', 2, '7e14d7a8-f13d-4524-a679-6cbae24d9d97' );

INSERT INTO `SchedulingTargetType` ( `tenantGuid`, `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Service Customer', 'Field service customer with multiple service addresses', 3, '6b3aa295-a54b-45dd-bda5-d75d157f376c' );


-- The core container that ScheduledEvents are scheduled into.   Examples: a construction project, a healthcare patient, a service customer.  Supports multiple addresses and recurring scheduling patterns.
CREATE TABLE `SchedulingTarget`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`officeId` INT NULL,		-- Optional office binding for a scheduling target.
	`clientId` INT NOT NULL,		-- The client that this scheduling target belongs to.
	`schedulingTargetTypeId` INT NOT NULL,		-- Link to the SchedulingTargetType table.
	`timeZoneId` INT NOT NULL,		-- Link to the TimeZone table.
	`calendarId` INT NULL,		-- An optional default calendar for this scheduling target.
	`notes` TEXT NULL,
	`externalId` VARCHAR(100) NULL,		-- Optional reference to an ID in an external system
	`color` VARCHAR(10) NULL,		-- Override of Target Type Hex color for UI display
	`attributes` TEXT NULL,		-- to store arbitrary JSON
	`avatarFileName` VARCHAR(250) NULL,		-- Part of the binary data field setup
	`avatarSize` BIGINT NULL,		-- Part of the binary data field setup
	`avatarData` BLOB NULL,		-- Part of the binary data field setup
	`avatarMimeType` VARCHAR(100) NULL,		-- Part of the binary data field setup
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`officeId`) REFERENCES `Office`(`id`),		-- Foreign key to the Office table.
	FOREIGN KEY (`clientId`) REFERENCES `Client`(`id`),		-- Foreign key to the Client table.
	FOREIGN KEY (`schedulingTargetTypeId`) REFERENCES `SchedulingTargetType`(`id`),		-- Foreign key to the SchedulingTargetType table.
	FOREIGN KEY (`timeZoneId`) REFERENCES `TimeZone`(`id`),		-- Foreign key to the TimeZone table.
	FOREIGN KEY (`calendarId`) REFERENCES `Calendar`(`id`),		-- Foreign key to the Calendar table.
	UNIQUE `UC_SchedulingTarget_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the SchedulingTarget table's tenantGuid and name fields.
);
-- Index on the SchedulingTarget table's tenantGuid field.
CREATE INDEX `I_SchedulingTarget_tenantGuid` ON `SchedulingTarget` (`tenantGuid`);

-- Index on the SchedulingTarget table's tenantGuid,name fields.
CREATE INDEX `I_SchedulingTarget_tenantGuid_name` ON `SchedulingTarget` (`tenantGuid`, `name`);

-- Index on the SchedulingTarget table's tenantGuid,officeId fields.
CREATE INDEX `I_SchedulingTarget_tenantGuid_officeId` ON `SchedulingTarget` (`tenantGuid`, `officeId`);

-- Index on the SchedulingTarget table's tenantGuid,clientId fields.
CREATE INDEX `I_SchedulingTarget_tenantGuid_clientId` ON `SchedulingTarget` (`tenantGuid`, `clientId`);

-- Index on the SchedulingTarget table's tenantGuid,schedulingTargetTypeId fields.
CREATE INDEX `I_SchedulingTarget_tenantGuid_schedulingTargetTypeId` ON `SchedulingTarget` (`tenantGuid`, `schedulingTargetTypeId`);

-- Index on the SchedulingTarget table's tenantGuid,timeZoneId fields.
CREATE INDEX `I_SchedulingTarget_tenantGuid_timeZoneId` ON `SchedulingTarget` (`tenantGuid`, `timeZoneId`);

-- Index on the SchedulingTarget table's tenantGuid,active fields.
CREATE INDEX `I_SchedulingTarget_tenantGuid_active` ON `SchedulingTarget` (`tenantGuid`, `active`);

-- Index on the SchedulingTarget table's tenantGuid,deleted fields.
CREATE INDEX `I_SchedulingTarget_tenantGuid_deleted` ON `SchedulingTarget` (`tenantGuid`, `deleted`);


-- The change history for records from the SchedulingTarget table.
CREATE TABLE `SchedulingTargetChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`schedulingTargetId` INT NOT NULL,		-- Link to the SchedulingTarget table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`schedulingTargetId`) REFERENCES `SchedulingTarget`(`id`)		-- Foreign key to the SchedulingTarget table.
);
-- Index on the SchedulingTargetChangeHistory table's tenantGuid field.
CREATE INDEX `I_SchedulingTargetChangeHistory_tenantGuid` ON `SchedulingTargetChangeHistory` (`tenantGuid`);

-- Index on the SchedulingTargetChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_SchedulingTargetChangeHistory_tenantGuid_versionNumber` ON `SchedulingTargetChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the SchedulingTargetChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_SchedulingTargetChangeHistory_tenantGuid_timeStamp` ON `SchedulingTargetChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the SchedulingTargetChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_SchedulingTargetChangeHistory_tenantGuid_userId` ON `SchedulingTargetChangeHistory` (`tenantGuid`, `userId`);

-- Index on the SchedulingTargetChangeHistory table's tenantGuid,schedulingTargetId fields.
CREATE INDEX `I_SchedulingTargetChangeHistory_tenantGuid_schedulingTargetId` ON `SchedulingTargetChangeHistory` (`tenantGuid`, `schedulingTargetId`, `versionNumber`, `timeStamp`, `userId`);


-- The link between scheduling targets and contacts.
CREATE TABLE `SchedulingTargetContact`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`schedulingTargetId` INT NOT NULL,		-- Link to the SchedulingTarget table.
	`contactId` INT NOT NULL,		-- Link to the Contact table.
	`isPrimary` BIT NOT NULL DEFAULT 0,		-- Indicates whether or not this contact should be considered a primary contact of the scheduling target.
	`relationshipTypeId` INT NOT NULL,		-- A description of the relationship between the scheduling target and the contact.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`schedulingTargetId`) REFERENCES `SchedulingTarget`(`id`),		-- Foreign key to the SchedulingTarget table.
	FOREIGN KEY (`contactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`relationshipTypeId`) REFERENCES `RelationshipType`(`id`),		-- Foreign key to the RelationshipType table.
	UNIQUE `UC_SchedulingTargetContact_tenantGuid_schedulingTargetId_contactId_Unique`( `tenantGuid`, `schedulingTargetId`, `contactId` ) 		-- Uniqueness enforced on the SchedulingTargetContact table's tenantGuid and schedulingTargetId and contactId fields.
);
-- Index on the SchedulingTargetContact table's tenantGuid field.
CREATE INDEX `I_SchedulingTargetContact_tenantGuid` ON `SchedulingTargetContact` (`tenantGuid`);

-- Index on the SchedulingTargetContact table's tenantGuid,schedulingTargetId fields.
CREATE INDEX `I_SchedulingTargetContact_tenantGuid_schedulingTargetId` ON `SchedulingTargetContact` (`tenantGuid`, `schedulingTargetId`);

-- Index on the SchedulingTargetContact table's tenantGuid,contactId fields.
CREATE INDEX `I_SchedulingTargetContact_tenantGuid_contactId` ON `SchedulingTargetContact` (`tenantGuid`, `contactId`);

-- Index on the SchedulingTargetContact table's tenantGuid,relationshipTypeId fields.
CREATE INDEX `I_SchedulingTargetContact_tenantGuid_relationshipTypeId` ON `SchedulingTargetContact` (`tenantGuid`, `relationshipTypeId`);

-- Index on the SchedulingTargetContact table's tenantGuid,active fields.
CREATE INDEX `I_SchedulingTargetContact_tenantGuid_active` ON `SchedulingTargetContact` (`tenantGuid`, `active`);

-- Index on the SchedulingTargetContact table's tenantGuid,deleted fields.
CREATE INDEX `I_SchedulingTargetContact_tenantGuid_deleted` ON `SchedulingTargetContact` (`tenantGuid`, `deleted`);


-- The change history for records from the SchedulingTargetContact table.
CREATE TABLE `SchedulingTargetContactChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`schedulingTargetContactId` INT NOT NULL,		-- Link to the SchedulingTargetContact table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`schedulingTargetContactId`) REFERENCES `SchedulingTargetContact`(`id`)		-- Foreign key to the SchedulingTargetContact table.
);
-- Index on the SchedulingTargetContactChangeHistory table's tenantGuid field.
CREATE INDEX `I_SchedulingTargetContactChangeHistory_tenantGuid` ON `SchedulingTargetContactChangeHistory` (`tenantGuid`);

-- Index on the SchedulingTargetContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_SchedulingTargetContactChangeHistory_tenantGuid_versionNumber` ON `SchedulingTargetContactChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the SchedulingTargetContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_SchedulingTargetContactChangeHistory_tenantGuid_timeStamp` ON `SchedulingTargetContactChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the SchedulingTargetContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_SchedulingTargetContactChangeHistory_tenantGuid_userId` ON `SchedulingTargetContactChangeHistory` (`tenantGuid`, `userId`);

-- Index on the SchedulingTargetContactChangeHistory table's tenantGuid,schedulingTargetContactId fields.
CREATE INDEX `I_SchedulingTargetContactChangeHistory_tenantGuid_schedulingTarg` ON `SchedulingTargetContactChangeHistory` (`tenantGuid`, `schedulingTargetContactId`, `versionNumber`, `timeStamp`, `userId`);


-- Links SchedulingTargets to multiple addresses (e.g., multiple job sites, patient home + hospital).
CREATE TABLE `SchedulingTargetAddress`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`schedulingTargetId` INT NOT NULL,		-- Primary  schuduling target for this address - could be null if there is a client linked to this, so the address would be for all targets in the client.
	`clientId` INT NULL,		-- Optional client level link.  The presence of a value here indicates that the address is to be shared across all scheduling targets for the client.
	`addressLine1` VARCHAR(250) NOT NULL,
	`addressLine2` VARCHAR(250) NULL,
	`city` VARCHAR(100) NOT NULL,
	`postalCode` VARCHAR(100) NULL,
	`stateProvinceId` INT NOT NULL,		-- Link to the StateProvince table.
	`countryId` INT NOT NULL,		-- Link to the Country table.
	`latitude` DOUBLE NULL,		-- Optional latitude position
	`longitude` DOUBLE NULL,		-- Optional longitude position
	`label` VARCHAR(250) NULL,		-- e.g., 'Main Site', 'Patient Home', 'Hospital Ward'
	`isPrimary` BIT NOT NULL DEFAULT 0,		-- Whether or not this is the scheduling target's main address.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`schedulingTargetId`) REFERENCES `SchedulingTarget`(`id`),		-- Foreign key to the SchedulingTarget table.
	FOREIGN KEY (`clientId`) REFERENCES `Client`(`id`),		-- Foreign key to the Client table.
	FOREIGN KEY (`stateProvinceId`) REFERENCES `StateProvince`(`id`),		-- Foreign key to the StateProvince table.
	FOREIGN KEY (`countryId`) REFERENCES `Country`(`id`),		-- Foreign key to the Country table.
	UNIQUE `UC_SchedulingTargetAddress_tenantGuid_schedulingTargetId_addressLine1_city_postalCode_Unique`( `tenantGuid`, `schedulingTargetId`, `addressLine1`, `city`, `postalCode` ) 		-- Uniqueness enforced on the SchedulingTargetAddress table's tenantGuid and schedulingTargetId and addressLine1 and city and postalCode fields.
);
-- Index on the SchedulingTargetAddress table's tenantGuid field.
CREATE INDEX `I_SchedulingTargetAddress_tenantGuid` ON `SchedulingTargetAddress` (`tenantGuid`);

-- Index on the SchedulingTargetAddress table's tenantGuid,schedulingTargetId fields.
CREATE INDEX `I_SchedulingTargetAddress_tenantGuid_schedulingTargetId` ON `SchedulingTargetAddress` (`tenantGuid`, `schedulingTargetId`);

-- Index on the SchedulingTargetAddress table's tenantGuid,clientId fields.
CREATE INDEX `I_SchedulingTargetAddress_tenantGuid_clientId` ON `SchedulingTargetAddress` (`tenantGuid`, `clientId`);

-- Index on the SchedulingTargetAddress table's tenantGuid,stateProvinceId fields.
CREATE INDEX `I_SchedulingTargetAddress_tenantGuid_stateProvinceId` ON `SchedulingTargetAddress` (`tenantGuid`, `stateProvinceId`);

-- Index on the SchedulingTargetAddress table's tenantGuid,countryId fields.
CREATE INDEX `I_SchedulingTargetAddress_tenantGuid_countryId` ON `SchedulingTargetAddress` (`tenantGuid`, `countryId`);

-- Index on the SchedulingTargetAddress table's tenantGuid,active fields.
CREATE INDEX `I_SchedulingTargetAddress_tenantGuid_active` ON `SchedulingTargetAddress` (`tenantGuid`, `active`);

-- Index on the SchedulingTargetAddress table's tenantGuid,deleted fields.
CREATE INDEX `I_SchedulingTargetAddress_tenantGuid_deleted` ON `SchedulingTargetAddress` (`tenantGuid`, `deleted`);


-- The change history for records from the SchedulingTargetAddress table.
CREATE TABLE `SchedulingTargetAddressChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`schedulingTargetAddressId` INT NOT NULL,		-- Link to the SchedulingTargetAddress table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`schedulingTargetAddressId`) REFERENCES `SchedulingTargetAddress`(`id`)		-- Foreign key to the SchedulingTargetAddress table.
);
-- Index on the SchedulingTargetAddressChangeHistory table's tenantGuid field.
CREATE INDEX `I_SchedulingTargetAddressChangeHistory_tenantGuid` ON `SchedulingTargetAddressChangeHistory` (`tenantGuid`);

-- Index on the SchedulingTargetAddressChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_SchedulingTargetAddressChangeHistory_tenantGuid_versionNumber` ON `SchedulingTargetAddressChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the SchedulingTargetAddressChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_SchedulingTargetAddressChangeHistory_tenantGuid_timeStamp` ON `SchedulingTargetAddressChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the SchedulingTargetAddressChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_SchedulingTargetAddressChangeHistory_tenantGuid_userId` ON `SchedulingTargetAddressChangeHistory` (`tenantGuid`, `userId`);

-- Index on the SchedulingTargetAddressChangeHistory table's tenantGuid,schedulingTargetAddressId fields.
CREATE INDEX `I_SchedulingTargetAddressChangeHistory_tenantGuid_schedulingTarg` ON `SchedulingTargetAddressChangeHistory` (`tenantGuid`, `schedulingTargetAddressId`, `versionNumber`, `timeStamp`, `userId`);


-- Defines which qualifications are required (or preferred) for working on a specific SchedulingTarget.  - isRequired = true then resource MUST have qualification  - isRequired = false then nice-to-have (warning only)
CREATE TABLE `SchedulingTargetQualificationRequirement`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`schedulingTargetId` INT NOT NULL,		-- Link to the SchedulingTarget table.
	`qualificationId` INT NOT NULL,		-- Link to the Qualification table.
	`isRequired` BIT NOT NULL DEFAULT 1,		-- true = mandatory, false = preferred
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`schedulingTargetId`) REFERENCES `SchedulingTarget`(`id`),		-- Foreign key to the SchedulingTarget table.
	FOREIGN KEY (`qualificationId`) REFERENCES `Qualification`(`id`),		-- Foreign key to the Qualification table.
	UNIQUE `UC_SchedulingTargetQualificationRequirement_tenantGuid_schedulingTargetId_qualificationId_Unique`( `tenantGuid`, `schedulingTargetId`, `qualificationId` ) 		-- Uniqueness enforced on the SchedulingTargetQualificationRequirement table's tenantGuid and schedulingTargetId and qualificationId fields.
);
-- Index on the SchedulingTargetQualificationRequirement table's tenantGuid field.
CREATE INDEX `I_SchedulingTargetQualificationRequirement_tenantGuid` ON `SchedulingTargetQualificationRequirement` (`tenantGuid`);

-- Index on the SchedulingTargetQualificationRequirement table's tenantGuid,schedulingTargetId fields.
CREATE INDEX `I_SchedulingTargetQualificationRequirement_tenantGuid_scheduling` ON `SchedulingTargetQualificationRequirement` (`tenantGuid`, `schedulingTargetId`);

-- Index on the SchedulingTargetQualificationRequirement table's tenantGuid,qualificationId fields.
CREATE INDEX `I_SchedulingTargetQualificationRequirement_tenantGuid_qualificat` ON `SchedulingTargetQualificationRequirement` (`tenantGuid`, `qualificationId`);

-- Index on the SchedulingTargetQualificationRequirement table's tenantGuid,active fields.
CREATE INDEX `I_SchedulingTargetQualificationRequirement_tenantGuid_active` ON `SchedulingTargetQualificationRequirement` (`tenantGuid`, `active`);

-- Index on the SchedulingTargetQualificationRequirement table's tenantGuid,deleted fields.
CREATE INDEX `I_SchedulingTargetQualificationRequirement_tenantGuid_deleted` ON `SchedulingTargetQualificationRequirement` (`tenantGuid`, `deleted`);


-- The change history for records from the SchedulingTargetQualificationRequirement table.
CREATE TABLE `SchedulingTargetQualificationRequirementChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`schedulingTargetQualificationRequirementId` INT NOT NULL,		-- Link to the SchedulingTargetQualificationRequirement table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`schedulingTargetQualificationRequirementId`) REFERENCES `SchedulingTargetQualificationRequirement`(`id`)		-- Foreign key to the SchedulingTargetQualificationRequirement table.
);
-- Index on the SchedulingTargetQualificationRequirementChangeHistory table's tenantGuid field.
CREATE INDEX `I_SchedulingTargetQualificationRequirementChangeHistory_tenantGu` ON `SchedulingTargetQualificationRequirementChangeHistory` (`tenantGuid`);

-- Index on the SchedulingTargetQualificationRequirementChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_SchedulingTargetQualificationRequirementChangeHistory_tenantGu` ON `SchedulingTargetQualificationRequirementChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the SchedulingTargetQualificationRequirementChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_SchedulingTargetQualificationRequirementChangeHistory_tenantGu` ON `SchedulingTargetQualificationRequirementChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the SchedulingTargetQualificationRequirementChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_SchedulingTargetQualificationRequirementChangeHistory_tenantGu` ON `SchedulingTargetQualificationRequirementChangeHistory` (`tenantGuid`, `userId`);

-- Index on the SchedulingTargetQualificationRequirementChangeHistory table's tenantGuid,schedulingTargetQualificationRequirementId fields.
CREATE INDEX `I_SchedulingTargetQualificationRequirementChangeHistory_tenantGu` ON `SchedulingTargetQualificationRequirementChangeHistory` (`tenantGuid`, `schedulingTargetQualificationRequirementId`, `versionNumber`, `timeStamp`, `userId`);


-- Master list of recurrence frequencies. Mirrors common iCalendar frequencies.
CREATE TABLE `RecurrenceFrequency`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the RecurrenceFrequency table's name field.
CREATE INDEX `I_RecurrenceFrequency_name` ON `RecurrenceFrequency` (`name`);

-- Index on the RecurrenceFrequency table's active field.
CREATE INDEX `I_RecurrenceFrequency_active` ON `RecurrenceFrequency` (`active`);

-- Index on the RecurrenceFrequency table's deleted field.
CREATE INDEX `I_RecurrenceFrequency_deleted` ON `RecurrenceFrequency` (`deleted`);

INSERT INTO `RecurrenceFrequency` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Once', 'Does not repeat', 1, 'a2e0f727-8e79-4add-af0a-495e89a4c6b7' );

INSERT INTO `RecurrenceFrequency` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Daily', 'Repeats every day or every N days', 2, 'bd28a0b1-26cf-4973-9129-bcd1cc5c9f67' );

INSERT INTO `RecurrenceFrequency` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Weekly', 'Repeats every week on selected days', 3, '044f3c91-7745-467a-955a-809acdc0dba7' );

INSERT INTO `RecurrenceFrequency` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Monthly', 'Repeats monthly (by day of month or day of week)', 4, 'fa0a9d3f-86e2-46c1-9a14-ea3858facf09' );

INSERT INTO `RecurrenceFrequency` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Yearly', 'Repeats annually', 5, '3ffeb2e0-0ced-4fc2-a268-bb31a3f5a861' );


-- Defines a recurrence pattern for a ScheduledEvent.  One ScheduledEvent can have zero or one RecurrenceRule (for recurring series).  Instances are generated on-the-fly or materialized as needed.
CREATE TABLE `RecurrenceRule`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`recurrenceFrequencyId` INT NOT NULL,		-- Link to the RecurrenceFrequency table.
	`interval` INT NOT NULL DEFAULT 1,		-- How often the pattern repeats (e.g., every 2 weeks)
	`untilDateTime` DATETIME NULL,		-- Recurrence ends on this date (inclusive). NULL = no end date
	`count` INT NULL,		-- Maximum number of occurrences. NULL = unlimited
	`dayOfWeekMask` INT NULL DEFAULT 0,		-- Bitmask for weekly recurrence:  1 = Sunday, 2 = Monday, 4 = Tuesday, 8 = Wednesday, 16 = Thursday, 32 = Friday, 64 = Saturday Example: Monday + Wednesday + Friday = 2 + 8 + 32 = 42
	`dayOfMonth` INT NULL,		-- For monthly: specific day (1-31). NULL if using dayOfWeekInMonth
	`dayOfWeekInMonth` INT NULL,		-- Values: 1 = first, 2 = second, 3 = third, 4 = fourth, 5 = last, -1 = second-to-last, etc. Combine with dayOfWeekMask.  
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`recurrenceFrequencyId`) REFERENCES `RecurrenceFrequency`(`id`)		-- Foreign key to the RecurrenceFrequency table.
);
-- Index on the RecurrenceRule table's tenantGuid field.
CREATE INDEX `I_RecurrenceRule_tenantGuid` ON `RecurrenceRule` (`tenantGuid`);

-- Index on the RecurrenceRule table's tenantGuid,recurrenceFrequencyId fields.
CREATE INDEX `I_RecurrenceRule_tenantGuid_recurrenceFrequencyId` ON `RecurrenceRule` (`tenantGuid`, `recurrenceFrequencyId`);

-- Index on the RecurrenceRule table's tenantGuid,active fields.
CREATE INDEX `I_RecurrenceRule_tenantGuid_active` ON `RecurrenceRule` (`tenantGuid`, `active`);

-- Index on the RecurrenceRule table's tenantGuid,deleted fields.
CREATE INDEX `I_RecurrenceRule_tenantGuid_deleted` ON `RecurrenceRule` (`tenantGuid`, `deleted`);


-- The change history for records from the RecurrenceRule table.
CREATE TABLE `RecurrenceRuleChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`recurrenceRuleId` INT NOT NULL,		-- Link to the RecurrenceRule table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`recurrenceRuleId`) REFERENCES `RecurrenceRule`(`id`)		-- Foreign key to the RecurrenceRule table.
);
-- Index on the RecurrenceRuleChangeHistory table's tenantGuid field.
CREATE INDEX `I_RecurrenceRuleChangeHistory_tenantGuid` ON `RecurrenceRuleChangeHistory` (`tenantGuid`);

-- Index on the RecurrenceRuleChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_RecurrenceRuleChangeHistory_tenantGuid_versionNumber` ON `RecurrenceRuleChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the RecurrenceRuleChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_RecurrenceRuleChangeHistory_tenantGuid_timeStamp` ON `RecurrenceRuleChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the RecurrenceRuleChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_RecurrenceRuleChangeHistory_tenantGuid_userId` ON `RecurrenceRuleChangeHistory` (`tenantGuid`, `userId`);

-- Index on the RecurrenceRuleChangeHistory table's tenantGuid,recurrenceRuleId fields.
CREATE INDEX `I_RecurrenceRuleChangeHistory_tenantGuid_recurrenceRuleId` ON `RecurrenceRuleChangeHistory` (`tenantGuid`, `recurrenceRuleId`, `versionNumber`, `timeStamp`, `userId`);


-- Reusable standard shift patterns (e.g., 'Day Shift', 'Night Shift', 'Weekend Crew').  Resources can be assigned to a pattern, or have custom overrides.
CREATE TABLE `ShiftPattern`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`timeZoneId` INT NULL,		-- Link to the TimeZone table.
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`timeZoneId`) REFERENCES `TimeZone`(`id`),		-- Foreign key to the TimeZone table.
	UNIQUE `UC_ShiftPattern_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the ShiftPattern table's tenantGuid and name fields.
);
-- Index on the ShiftPattern table's tenantGuid field.
CREATE INDEX `I_ShiftPattern_tenantGuid` ON `ShiftPattern` (`tenantGuid`);

-- Index on the ShiftPattern table's tenantGuid,name fields.
CREATE INDEX `I_ShiftPattern_tenantGuid_name` ON `ShiftPattern` (`tenantGuid`, `name`);

-- Index on the ShiftPattern table's tenantGuid,timeZoneId fields.
CREATE INDEX `I_ShiftPattern_tenantGuid_timeZoneId` ON `ShiftPattern` (`tenantGuid`, `timeZoneId`);

-- Index on the ShiftPattern table's tenantGuid,active fields.
CREATE INDEX `I_ShiftPattern_tenantGuid_active` ON `ShiftPattern` (`tenantGuid`, `active`);

-- Index on the ShiftPattern table's tenantGuid,deleted fields.
CREATE INDEX `I_ShiftPattern_tenantGuid_deleted` ON `ShiftPattern` (`tenantGuid`, `deleted`);


-- The change history for records from the ShiftPattern table.
CREATE TABLE `ShiftPatternChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`shiftPatternId` INT NOT NULL,		-- Link to the ShiftPattern table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`shiftPatternId`) REFERENCES `ShiftPattern`(`id`)		-- Foreign key to the ShiftPattern table.
);
-- Index on the ShiftPatternChangeHistory table's tenantGuid field.
CREATE INDEX `I_ShiftPatternChangeHistory_tenantGuid` ON `ShiftPatternChangeHistory` (`tenantGuid`);

-- Index on the ShiftPatternChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ShiftPatternChangeHistory_tenantGuid_versionNumber` ON `ShiftPatternChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ShiftPatternChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ShiftPatternChangeHistory_tenantGuid_timeStamp` ON `ShiftPatternChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ShiftPatternChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ShiftPatternChangeHistory_tenantGuid_userId` ON `ShiftPatternChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ShiftPatternChangeHistory table's tenantGuid,shiftPatternId fields.
CREATE INDEX `I_ShiftPatternChangeHistory_tenantGuid_shiftPatternId` ON `ShiftPatternChangeHistory` (`tenantGuid`, `shiftPatternId`, `versionNumber`, `timeStamp`, `userId`);


-- Defines the days and availability windows for a ShiftPattern.
CREATE TABLE `ShiftPatternDay`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`shiftPatternId` INT NOT NULL,		-- Link to the ShiftPattern table.
	`dayOfWeek` INT NOT NULL DEFAULT 1,		-- Day this rule applies to   1=Sunday..7=Saturday
	`startTime` TIME NOT NULL,		-- Start of available window (local to pattern time zone) e.g., 07:00:00
	`hours` FLOAT NOT NULL DEFAULT 8,		-- Hours available from start time (handles overnight shifts cleanly)  Defaults to 8
	`label` VARCHAR(250) NULL,		-- e.g., Main Shift
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`shiftPatternId`) REFERENCES `ShiftPattern`(`id`),		-- Foreign key to the ShiftPattern table.
	UNIQUE `UC_ShiftPatternDay_tenantGuid_shiftPatternId_dayOfWeek_Unique`( `tenantGuid`, `shiftPatternId`, `dayOfWeek` ) 		-- Uniqueness enforced on the ShiftPatternDay table's tenantGuid and shiftPatternId and dayOfWeek fields.
);
-- Index on the ShiftPatternDay table's tenantGuid field.
CREATE INDEX `I_ShiftPatternDay_tenantGuid` ON `ShiftPatternDay` (`tenantGuid`);

-- Index on the ShiftPatternDay table's tenantGuid,shiftPatternId fields.
CREATE INDEX `I_ShiftPatternDay_tenantGuid_shiftPatternId` ON `ShiftPatternDay` (`tenantGuid`, `shiftPatternId`);

-- Index on the ShiftPatternDay table's tenantGuid,active fields.
CREATE INDEX `I_ShiftPatternDay_tenantGuid_active` ON `ShiftPatternDay` (`tenantGuid`, `active`);

-- Index on the ShiftPatternDay table's tenantGuid,deleted fields.
CREATE INDEX `I_ShiftPatternDay_tenantGuid_deleted` ON `ShiftPatternDay` (`tenantGuid`, `deleted`);


-- The change history for records from the ShiftPatternDay table.
CREATE TABLE `ShiftPatternDayChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`shiftPatternDayId` INT NOT NULL,		-- Link to the ShiftPatternDay table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`shiftPatternDayId`) REFERENCES `ShiftPatternDay`(`id`)		-- Foreign key to the ShiftPatternDay table.
);
-- Index on the ShiftPatternDayChangeHistory table's tenantGuid field.
CREATE INDEX `I_ShiftPatternDayChangeHistory_tenantGuid` ON `ShiftPatternDayChangeHistory` (`tenantGuid`);

-- Index on the ShiftPatternDayChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ShiftPatternDayChangeHistory_tenantGuid_versionNumber` ON `ShiftPatternDayChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ShiftPatternDayChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ShiftPatternDayChangeHistory_tenantGuid_timeStamp` ON `ShiftPatternDayChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ShiftPatternDayChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ShiftPatternDayChangeHistory_tenantGuid_userId` ON `ShiftPatternDayChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ShiftPatternDayChangeHistory table's tenantGuid,shiftPatternDayId fields.
CREATE INDEX `I_ShiftPatternDayChangeHistory_tenantGuid_shiftPatternDayId` ON `ShiftPatternDayChangeHistory` (`tenantGuid`, `shiftPatternDayId`, `versionNumber`, `timeStamp`, `userId`);


-- The schedulable entities – people and assets.  Examples: 'John Doe (Operator)', 'CAT CP56B Roller #12', 'Conference Room A'.
CREATE TABLE `Resource`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`officeId` INT NULL,		-- Optional office binding for a resource.
	`resourceTypeId` INT NOT NULL,		-- Link to the ResourceType table.
	`shiftPatternId` INT NULL,		-- Standard shift pattern this resource follows (NULL = custom shifts via ResourceShift)
	`timeZoneId` INT NOT NULL,		-- Link to the TimeZone table.
	`targetWeeklyWorkHours` FLOAT NULL,
	`notes` TEXT NULL,
	`externalId` VARCHAR(100) NULL,		-- Optional reference to an ID in an external system
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`attributes` TEXT NULL,		-- to store arbitrary JSON
	`avatarFileName` VARCHAR(250) NULL,		-- Part of the binary data field setup
	`avatarSize` BIGINT NULL,		-- Part of the binary data field setup
	`avatarData` BLOB NULL,		-- Part of the binary data field setup
	`avatarMimeType` VARCHAR(100) NULL,		-- Part of the binary data field setup
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`officeId`) REFERENCES `Office`(`id`),		-- Foreign key to the Office table.
	FOREIGN KEY (`resourceTypeId`) REFERENCES `ResourceType`(`id`),		-- Foreign key to the ResourceType table.
	FOREIGN KEY (`shiftPatternId`) REFERENCES `ShiftPattern`(`id`),		-- Foreign key to the ShiftPattern table.
	FOREIGN KEY (`timeZoneId`) REFERENCES `TimeZone`(`id`),		-- Foreign key to the TimeZone table.
	UNIQUE `UC_Resource_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the Resource table's tenantGuid and name fields.
);
-- Index on the Resource table's tenantGuid field.
CREATE INDEX `I_Resource_tenantGuid` ON `Resource` (`tenantGuid`);

-- Index on the Resource table's tenantGuid,name fields.
CREATE INDEX `I_Resource_tenantGuid_name` ON `Resource` (`tenantGuid`, `name`);

-- Index on the Resource table's tenantGuid,officeId fields.
CREATE INDEX `I_Resource_tenantGuid_officeId` ON `Resource` (`tenantGuid`, `officeId`);

-- Index on the Resource table's tenantGuid,resourceTypeId fields.
CREATE INDEX `I_Resource_tenantGuid_resourceTypeId` ON `Resource` (`tenantGuid`, `resourceTypeId`);

-- Index on the Resource table's tenantGuid,shiftPatternId fields.
CREATE INDEX `I_Resource_tenantGuid_shiftPatternId` ON `Resource` (`tenantGuid`, `shiftPatternId`);

-- Index on the Resource table's tenantGuid,timeZoneId fields.
CREATE INDEX `I_Resource_tenantGuid_timeZoneId` ON `Resource` (`tenantGuid`, `timeZoneId`);

-- Index on the Resource table's tenantGuid,active fields.
CREATE INDEX `I_Resource_tenantGuid_active` ON `Resource` (`tenantGuid`, `active`);

-- Index on the Resource table's tenantGuid,deleted fields.
CREATE INDEX `I_Resource_tenantGuid_deleted` ON `Resource` (`tenantGuid`, `deleted`);

-- Index on the Resource table's tenantGuid,externalId fields.
CREATE INDEX `I_Resource_tenantGuid_externalId` ON `Resource` (`tenantGuid`, `externalId`);


-- The change history for records from the Resource table.
CREATE TABLE `ResourceChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`resourceId` INT NOT NULL,		-- Link to the Resource table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`)		-- Foreign key to the Resource table.
);
-- Index on the ResourceChangeHistory table's tenantGuid field.
CREATE INDEX `I_ResourceChangeHistory_tenantGuid` ON `ResourceChangeHistory` (`tenantGuid`);

-- Index on the ResourceChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ResourceChangeHistory_tenantGuid_versionNumber` ON `ResourceChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ResourceChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ResourceChangeHistory_tenantGuid_timeStamp` ON `ResourceChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ResourceChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ResourceChangeHistory_tenantGuid_userId` ON `ResourceChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ResourceChangeHistory table's tenantGuid,resourceId fields.
CREATE INDEX `I_ResourceChangeHistory_tenantGuid_resourceId` ON `ResourceChangeHistory` (`tenantGuid`, `resourceId`, `versionNumber`, `timeStamp`, `userId`);


-- The link between scheduling targets and contacts.
CREATE TABLE `ResourceContact`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`resourceId` INT NOT NULL,		-- Link to the Resource table.
	`contactId` INT NOT NULL,		-- Link to the Contact table.
	`isPrimary` BIT NOT NULL DEFAULT 0,		-- Indicates whether or not this contact should be considered a primary contact of the resource.
	`relationshipTypeId` INT NOT NULL,		-- A description of the relationship between the resource and the contact.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`),		-- Foreign key to the Resource table.
	FOREIGN KEY (`contactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`relationshipTypeId`) REFERENCES `RelationshipType`(`id`),		-- Foreign key to the RelationshipType table.
	UNIQUE `UC_ResourceContact_tenantGuid_resourceId_contactId_Unique`( `tenantGuid`, `resourceId`, `contactId` ) 		-- Uniqueness enforced on the ResourceContact table's tenantGuid and resourceId and contactId fields.
);
-- Index on the ResourceContact table's tenantGuid field.
CREATE INDEX `I_ResourceContact_tenantGuid` ON `ResourceContact` (`tenantGuid`);

-- Index on the ResourceContact table's tenantGuid,resourceId fields.
CREATE INDEX `I_ResourceContact_tenantGuid_resourceId` ON `ResourceContact` (`tenantGuid`, `resourceId`);

-- Index on the ResourceContact table's tenantGuid,contactId fields.
CREATE INDEX `I_ResourceContact_tenantGuid_contactId` ON `ResourceContact` (`tenantGuid`, `contactId`);

-- Index on the ResourceContact table's tenantGuid,relationshipTypeId fields.
CREATE INDEX `I_ResourceContact_tenantGuid_relationshipTypeId` ON `ResourceContact` (`tenantGuid`, `relationshipTypeId`);

-- Index on the ResourceContact table's tenantGuid,active fields.
CREATE INDEX `I_ResourceContact_tenantGuid_active` ON `ResourceContact` (`tenantGuid`, `active`);

-- Index on the ResourceContact table's tenantGuid,deleted fields.
CREATE INDEX `I_ResourceContact_tenantGuid_deleted` ON `ResourceContact` (`tenantGuid`, `deleted`);


-- The change history for records from the ResourceContact table.
CREATE TABLE `ResourceContactChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`resourceContactId` INT NOT NULL,		-- Link to the ResourceContact table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`resourceContactId`) REFERENCES `ResourceContact`(`id`)		-- Foreign key to the ResourceContact table.
);
-- Index on the ResourceContactChangeHistory table's tenantGuid field.
CREATE INDEX `I_ResourceContactChangeHistory_tenantGuid` ON `ResourceContactChangeHistory` (`tenantGuid`);

-- Index on the ResourceContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ResourceContactChangeHistory_tenantGuid_versionNumber` ON `ResourceContactChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ResourceContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ResourceContactChangeHistory_tenantGuid_timeStamp` ON `ResourceContactChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ResourceContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ResourceContactChangeHistory_tenantGuid_userId` ON `ResourceContactChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ResourceContactChangeHistory table's tenantGuid,resourceContactId fields.
CREATE INDEX `I_ResourceContactChangeHistory_tenantGuid_resourceContactId` ON `ResourceContactChangeHistory` (`tenantGuid`, `resourceContactId`, `versionNumber`, `timeStamp`, `userId`);


/*
Master Rate Sheet. 
Replaces simple Resource-based rating with a hierarchical lookup system.
Hierarchy Logic (System should look for the first match in this order):
1. Specific Resource on Specific Project (schedulingTargetId + resourceId)
2. Specific Role on Specific Project (schedulingTargetId + assignmentRoleId)
3. Specific Resource Global Rate (resourceId)
4. Specific Role Global Rate (assignmentRoleId)
*/
CREATE TABLE `RateSheet`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`officeId` INT NULL,		-- Optional office binding for a rate sheet.
	`assignmentRoleId` INT NULL,		-- Link to AssignmentRole. If populated, applies to anyone in this role.
	`resourceId` INT NULL,		-- Link to Resource. If populated, overrides the Role rate.
	`schedulingTargetId` INT NULL,		-- Link to SchedulingTarget. If populated, applies only to this project.
	`rateTypeId` INT NOT NULL,		-- e.g., 'Standard', 'Overtime', 'DoubleTime', 'Travel', 'Standby'
	`effectiveDate` DATETIME NOT NULL,		-- The date this rate becomes active. Allows for historical reporting and future price increases.
	`currencyId` INT NOT NULL,		-- Link to the Currency table.
	`costRate` DECIMAL(11,2) NOT NULL,		-- Internal Cost (payroll)
	`billingRate` DECIMAL(11,2) NOT NULL,		-- Invoicing Cost (customre)
	`notes` TEXT NULL,		-- For ad-hoc notes about the entry
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`officeId`) REFERENCES `Office`(`id`),		-- Foreign key to the Office table.
	FOREIGN KEY (`assignmentRoleId`) REFERENCES `AssignmentRole`(`id`),		-- Foreign key to the AssignmentRole table.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`),		-- Foreign key to the Resource table.
	FOREIGN KEY (`schedulingTargetId`) REFERENCES `SchedulingTarget`(`id`),		-- Foreign key to the SchedulingTarget table.
	FOREIGN KEY (`rateTypeId`) REFERENCES `RateType`(`id`),		-- Foreign key to the RateType table.
	FOREIGN KEY (`currencyId`) REFERENCES `Currency`(`id`),		-- Foreign key to the Currency table.
	UNIQUE `UC_RateSheet_tenantGuid_assignmentRoleId_resourceId_schedulingTargetId_rateTypeId_effectiveDate_Unique`( `tenantGuid`, `assignmentRoleId`, `resourceId`, `schedulingTargetId`, `rateTypeId`, `effectiveDate` ) 		-- Uniqueness enforced on the RateSheet table's tenantGuid and assignmentRoleId and resourceId and schedulingTargetId and rateTypeId and effectiveDate fields.
);
-- Index on the RateSheet table's tenantGuid field.
CREATE INDEX `I_RateSheet_tenantGuid` ON `RateSheet` (`tenantGuid`);

-- Index on the RateSheet table's tenantGuid,officeId fields.
CREATE INDEX `I_RateSheet_tenantGuid_officeId` ON `RateSheet` (`tenantGuid`, `officeId`);

-- Index on the RateSheet table's tenantGuid,assignmentRoleId fields.
CREATE INDEX `I_RateSheet_tenantGuid_assignmentRoleId` ON `RateSheet` (`tenantGuid`, `assignmentRoleId`);

-- Index on the RateSheet table's tenantGuid,resourceId fields.
CREATE INDEX `I_RateSheet_tenantGuid_resourceId` ON `RateSheet` (`tenantGuid`, `resourceId`);

-- Index on the RateSheet table's tenantGuid,schedulingTargetId fields.
CREATE INDEX `I_RateSheet_tenantGuid_schedulingTargetId` ON `RateSheet` (`tenantGuid`, `schedulingTargetId`);

-- Index on the RateSheet table's tenantGuid,rateTypeId fields.
CREATE INDEX `I_RateSheet_tenantGuid_rateTypeId` ON `RateSheet` (`tenantGuid`, `rateTypeId`);

-- Index on the RateSheet table's tenantGuid,currencyId fields.
CREATE INDEX `I_RateSheet_tenantGuid_currencyId` ON `RateSheet` (`tenantGuid`, `currencyId`);

-- Index on the RateSheet table's tenantGuid,active fields.
CREATE INDEX `I_RateSheet_tenantGuid_active` ON `RateSheet` (`tenantGuid`, `active`);

-- Index on the RateSheet table's tenantGuid,deleted fields.
CREATE INDEX `I_RateSheet_tenantGuid_deleted` ON `RateSheet` (`tenantGuid`, `deleted`);

-- Index on the RateSheet table's tenantGuid,schedulingTargetId,resourceId,assignmentRoleId,rateTypeId,effectiveDate fields.
CREATE INDEX `I_RateSheet_tenantGuid_schedulingTargetId_resourceId_assignmentR` ON `RateSheet` (`tenantGuid`, `schedulingTargetId`, `resourceId`, `assignmentRoleId`, `rateTypeId`, `effectiveDate`);


-- The change history for records from the RateSheet table.
CREATE TABLE `RateSheetChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`rateSheetId` INT NOT NULL,		-- Link to the RateSheet table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`rateSheetId`) REFERENCES `RateSheet`(`id`)		-- Foreign key to the RateSheet table.
);
-- Index on the RateSheetChangeHistory table's tenantGuid field.
CREATE INDEX `I_RateSheetChangeHistory_tenantGuid` ON `RateSheetChangeHistory` (`tenantGuid`);

-- Index on the RateSheetChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_RateSheetChangeHistory_tenantGuid_versionNumber` ON `RateSheetChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the RateSheetChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_RateSheetChangeHistory_tenantGuid_timeStamp` ON `RateSheetChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the RateSheetChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_RateSheetChangeHistory_tenantGuid_userId` ON `RateSheetChangeHistory` (`tenantGuid`, `userId`);

-- Index on the RateSheetChangeHistory table's tenantGuid,rateSheetId fields.
CREATE INDEX `I_RateSheetChangeHistory_tenantGuid_rateSheetId` ON `RateSheetChangeHistory` (`tenantGuid`, `rateSheetId`, `versionNumber`, `timeStamp`, `userId`);


-- Links resources to qualifications they possess.  Includes expiry date, issuing authority, and notes.
CREATE TABLE `ResourceQualification`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`resourceId` INT NOT NULL,		-- Link to the Resource table.
	`qualificationId` INT NOT NULL,		-- Link to the Qualification table.
	`issueDate` DATETIME NULL,		-- Date qualification was granted
	`expiryDate` DATETIME NULL,		-- NULL = no expiry (e.g., permanent license)
	`issuer` VARCHAR(250) NULL,		-- e.g., State Board of Nursing, NCCCO
	`notes` TEXT NULL,
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`),		-- Foreign key to the Resource table.
	FOREIGN KEY (`qualificationId`) REFERENCES `Qualification`(`id`),		-- Foreign key to the Qualification table.
	UNIQUE `UC_ResourceQualification_tenantGuid_resourceId_qualificationId_Unique`( `tenantGuid`, `resourceId`, `qualificationId` ) 		-- Uniqueness enforced on the ResourceQualification table's tenantGuid and resourceId and qualificationId fields.
);
-- Index on the ResourceQualification table's tenantGuid field.
CREATE INDEX `I_ResourceQualification_tenantGuid` ON `ResourceQualification` (`tenantGuid`);

-- Index on the ResourceQualification table's tenantGuid,resourceId fields.
CREATE INDEX `I_ResourceQualification_tenantGuid_resourceId` ON `ResourceQualification` (`tenantGuid`, `resourceId`);

-- Index on the ResourceQualification table's tenantGuid,qualificationId fields.
CREATE INDEX `I_ResourceQualification_tenantGuid_qualificationId` ON `ResourceQualification` (`tenantGuid`, `qualificationId`);

-- Index on the ResourceQualification table's tenantGuid,expiryDate fields.
CREATE INDEX `I_ResourceQualification_tenantGuid_expiryDate` ON `ResourceQualification` (`tenantGuid`, `expiryDate`);

-- Index on the ResourceQualification table's tenantGuid,active fields.
CREATE INDEX `I_ResourceQualification_tenantGuid_active` ON `ResourceQualification` (`tenantGuid`, `active`);

-- Index on the ResourceQualification table's tenantGuid,deleted fields.
CREATE INDEX `I_ResourceQualification_tenantGuid_deleted` ON `ResourceQualification` (`tenantGuid`, `deleted`);


-- The change history for records from the ResourceQualification table.
CREATE TABLE `ResourceQualificationChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`resourceQualificationId` INT NOT NULL,		-- Link to the ResourceQualification table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`resourceQualificationId`) REFERENCES `ResourceQualification`(`id`)		-- Foreign key to the ResourceQualification table.
);
-- Index on the ResourceQualificationChangeHistory table's tenantGuid field.
CREATE INDEX `I_ResourceQualificationChangeHistory_tenantGuid` ON `ResourceQualificationChangeHistory` (`tenantGuid`);

-- Index on the ResourceQualificationChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ResourceQualificationChangeHistory_tenantGuid_versionNumber` ON `ResourceQualificationChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ResourceQualificationChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ResourceQualificationChangeHistory_tenantGuid_timeStamp` ON `ResourceQualificationChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ResourceQualificationChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ResourceQualificationChangeHistory_tenantGuid_userId` ON `ResourceQualificationChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ResourceQualificationChangeHistory table's tenantGuid,resourceQualificationId fields.
CREATE INDEX `I_ResourceQualificationChangeHistory_tenantGuid_resourceQualific` ON `ResourceQualificationChangeHistory` (`tenantGuid`, `resourceQualificationId`, `versionNumber`, `timeStamp`, `userId`);


-- Defines periods when a resource is unavailable (blackouts).  Used for vacations, maintenance, training, etc.  If endDateTime is NULL the blackout is ongoing until cleared.
CREATE TABLE `ResourceAvailability`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`resourceId` INT NOT NULL,		-- Link to the Resource table.
	`timeZoneId` INT NULL,		-- Link to the TimeZone table.
	`startDateTime` DATETIME NOT NULL,		-- Inclusive start of the blackout period
	`endDateTime` DATETIME NULL,		-- NULL = ongoing blackout
	`reason` VARCHAR(250) NULL,
	`notes` TEXT NULL,
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`),		-- Foreign key to the Resource table.
	FOREIGN KEY (`timeZoneId`) REFERENCES `TimeZone`(`id`)		-- Foreign key to the TimeZone table.
);
-- Index on the ResourceAvailability table's tenantGuid field.
CREATE INDEX `I_ResourceAvailability_tenantGuid` ON `ResourceAvailability` (`tenantGuid`);

-- Index on the ResourceAvailability table's tenantGuid,resourceId fields.
CREATE INDEX `I_ResourceAvailability_tenantGuid_resourceId` ON `ResourceAvailability` (`tenantGuid`, `resourceId`);

-- Index on the ResourceAvailability table's tenantGuid,timeZoneId fields.
CREATE INDEX `I_ResourceAvailability_tenantGuid_timeZoneId` ON `ResourceAvailability` (`tenantGuid`, `timeZoneId`);

-- Index on the ResourceAvailability table's tenantGuid,active fields.
CREATE INDEX `I_ResourceAvailability_tenantGuid_active` ON `ResourceAvailability` (`tenantGuid`, `active`);

-- Index on the ResourceAvailability table's tenantGuid,deleted fields.
CREATE INDEX `I_ResourceAvailability_tenantGuid_deleted` ON `ResourceAvailability` (`tenantGuid`, `deleted`);

-- Index on the ResourceAvailability table's tenantGuid,resourceId,startDateTime,endDateTime fields.
CREATE INDEX `I_ResourceAvailability_tenantGuid_resourceId_startDateTime_endDa` ON `ResourceAvailability` (`tenantGuid`, `resourceId`, `startDateTime`, `endDateTime`);


-- The change history for records from the ResourceAvailability table.
CREATE TABLE `ResourceAvailabilityChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`resourceAvailabilityId` INT NOT NULL,		-- Link to the ResourceAvailability table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`resourceAvailabilityId`) REFERENCES `ResourceAvailability`(`id`)		-- Foreign key to the ResourceAvailability table.
);
-- Index on the ResourceAvailabilityChangeHistory table's tenantGuid field.
CREATE INDEX `I_ResourceAvailabilityChangeHistory_tenantGuid` ON `ResourceAvailabilityChangeHistory` (`tenantGuid`);

-- Index on the ResourceAvailabilityChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ResourceAvailabilityChangeHistory_tenantGuid_versionNumber` ON `ResourceAvailabilityChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ResourceAvailabilityChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ResourceAvailabilityChangeHistory_tenantGuid_timeStamp` ON `ResourceAvailabilityChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ResourceAvailabilityChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ResourceAvailabilityChangeHistory_tenantGuid_userId` ON `ResourceAvailabilityChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ResourceAvailabilityChangeHistory table's tenantGuid,resourceAvailabilityId fields.
CREATE INDEX `I_ResourceAvailabilityChangeHistory_tenantGuid_resourceAvailabil` ON `ResourceAvailabilityChangeHistory` (`tenantGuid`, `resourceAvailabilityId`, `versionNumber`, `timeStamp`, `userId`);


-- Defines regular working shifts for a resource (e.g., clinician hours).  Used to determine baseline availability. Blackouts (ResourceAvailability) override these for exceptions.
CREATE TABLE `ResourceShift`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`resourceId` INT NOT NULL,		-- Link to the Resource table.
	`dayOfWeek` INT NOT NULL DEFAULT 1,		-- 1=Sunday through 7=Saturday
	`timeZoneId` INT NULL,		-- Link to the TimeZone table.
	`startTime` TIME NOT NULL,		-- Shift start time (e.g., 09:00:00)
	`hours` FLOAT NOT NULL DEFAULT 8,		-- Hours available from start time (handles overnight shifts cleanly)  Defaults to 8
	`label` VARCHAR(250) NULL,		-- e.g., 'Morning Clinic', 'On-Call'
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`),		-- Foreign key to the Resource table.
	FOREIGN KEY (`timeZoneId`) REFERENCES `TimeZone`(`id`),		-- Foreign key to the TimeZone table.
	UNIQUE `UC_ResourceShift_tenantGuid_resourceId_dayOfWeek_Unique`( `tenantGuid`, `resourceId`, `dayOfWeek` ) 		-- Uniqueness enforced on the ResourceShift table's tenantGuid and resourceId and dayOfWeek fields.
);
-- Index on the ResourceShift table's tenantGuid field.
CREATE INDEX `I_ResourceShift_tenantGuid` ON `ResourceShift` (`tenantGuid`);

-- Index on the ResourceShift table's tenantGuid,resourceId fields.
CREATE INDEX `I_ResourceShift_tenantGuid_resourceId` ON `ResourceShift` (`tenantGuid`, `resourceId`);

-- Index on the ResourceShift table's tenantGuid,timeZoneId fields.
CREATE INDEX `I_ResourceShift_tenantGuid_timeZoneId` ON `ResourceShift` (`tenantGuid`, `timeZoneId`);

-- Index on the ResourceShift table's tenantGuid,active fields.
CREATE INDEX `I_ResourceShift_tenantGuid_active` ON `ResourceShift` (`tenantGuid`, `active`);

-- Index on the ResourceShift table's tenantGuid,deleted fields.
CREATE INDEX `I_ResourceShift_tenantGuid_deleted` ON `ResourceShift` (`tenantGuid`, `deleted`);


-- The change history for records from the ResourceShift table.
CREATE TABLE `ResourceShiftChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`resourceShiftId` INT NOT NULL,		-- Link to the ResourceShift table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`resourceShiftId`) REFERENCES `ResourceShift`(`id`)		-- Foreign key to the ResourceShift table.
);
-- Index on the ResourceShiftChangeHistory table's tenantGuid field.
CREATE INDEX `I_ResourceShiftChangeHistory_tenantGuid` ON `ResourceShiftChangeHistory` (`tenantGuid`);

-- Index on the ResourceShiftChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ResourceShiftChangeHistory_tenantGuid_versionNumber` ON `ResourceShiftChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ResourceShiftChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ResourceShiftChangeHistory_tenantGuid_timeStamp` ON `ResourceShiftChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ResourceShiftChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ResourceShiftChangeHistory_tenantGuid_userId` ON `ResourceShiftChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ResourceShiftChangeHistory table's tenantGuid,resourceShiftId fields.
CREATE INDEX `I_ResourceShiftChangeHistory_tenantGuid_resourceShiftId` ON `ResourceShiftChangeHistory` (`tenantGuid`, `resourceShiftId`, `versionNumber`, `timeStamp`, `userId`);


-- Named, reusable group of resources that are typically scheduled together.  Common in construction (e.g., a roller + operator + spotter).  Crews can be assigned to events as a single unit.
CREATE TABLE `Crew`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`notes` TEXT NULL,
	`officeId` INT NULL,		-- Optional office binding for a crew.
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`avatarFileName` VARCHAR(250) NULL,		-- Part of the binary data field setup
	`avatarSize` BIGINT NULL,		-- Part of the binary data field setup
	`avatarData` BLOB NULL,		-- Part of the binary data field setup
	`avatarMimeType` VARCHAR(100) NULL,		-- Part of the binary data field setup
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`officeId`) REFERENCES `Office`(`id`),		-- Foreign key to the Office table.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_Crew_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the Crew table's tenantGuid and name fields.
);
-- Index on the Crew table's tenantGuid field.
CREATE INDEX `I_Crew_tenantGuid` ON `Crew` (`tenantGuid`);

-- Index on the Crew table's tenantGuid,name fields.
CREATE INDEX `I_Crew_tenantGuid_name` ON `Crew` (`tenantGuid`, `name`);

-- Index on the Crew table's tenantGuid,officeId fields.
CREATE INDEX `I_Crew_tenantGuid_officeId` ON `Crew` (`tenantGuid`, `officeId`);

-- Index on the Crew table's tenantGuid,iconId fields.
CREATE INDEX `I_Crew_tenantGuid_iconId` ON `Crew` (`tenantGuid`, `iconId`);

-- Index on the Crew table's tenantGuid,active fields.
CREATE INDEX `I_Crew_tenantGuid_active` ON `Crew` (`tenantGuid`, `active`);

-- Index on the Crew table's tenantGuid,deleted fields.
CREATE INDEX `I_Crew_tenantGuid_deleted` ON `Crew` (`tenantGuid`, `deleted`);


-- The change history for records from the Crew table.
CREATE TABLE `CrewChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`crewId` INT NOT NULL,		-- Link to the Crew table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`crewId`) REFERENCES `Crew`(`id`)		-- Foreign key to the Crew table.
);
-- Index on the CrewChangeHistory table's tenantGuid field.
CREATE INDEX `I_CrewChangeHistory_tenantGuid` ON `CrewChangeHistory` (`tenantGuid`);

-- Index on the CrewChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_CrewChangeHistory_tenantGuid_versionNumber` ON `CrewChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the CrewChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_CrewChangeHistory_tenantGuid_timeStamp` ON `CrewChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the CrewChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_CrewChangeHistory_tenantGuid_userId` ON `CrewChangeHistory` (`tenantGuid`, `userId`);

-- Index on the CrewChangeHistory table's tenantGuid,crewId fields.
CREATE INDEX `I_CrewChangeHistory_tenantGuid_crewId` ON `CrewChangeHistory` (`tenantGuid`, `crewId`, `versionNumber`, `timeStamp`, `userId`);


-- Membership definition for a crew.  Specifies which resource belongs to which crew, the role they play within the crew, and a display sequence.
CREATE TABLE `CrewMember`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`crewId` INT NOT NULL,		-- Link to the Crew table.
	`resourceId` INT NOT NULL,		-- Link to the Resource table.
	`assignmentRoleId` INT NULL,		-- Optional default role this member fulfils when the crew is assigned
	`sequence` INT NOT NULL DEFAULT 1,		-- Display/order position within the crew (lower numbers appear first)
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`crewId`) REFERENCES `Crew`(`id`),		-- Foreign key to the Crew table.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`),		-- Foreign key to the Resource table.
	FOREIGN KEY (`assignmentRoleId`) REFERENCES `AssignmentRole`(`id`),		-- Foreign key to the AssignmentRole table.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_CrewMember_tenantGuid_crewId_resourceId_Unique`( `tenantGuid`, `crewId`, `resourceId` ) 		-- Uniqueness enforced on the CrewMember table's tenantGuid and crewId and resourceId fields.
);
-- Index on the CrewMember table's tenantGuid field.
CREATE INDEX `I_CrewMember_tenantGuid` ON `CrewMember` (`tenantGuid`);

-- Index on the CrewMember table's tenantGuid,crewId fields.
CREATE INDEX `I_CrewMember_tenantGuid_crewId` ON `CrewMember` (`tenantGuid`, `crewId`);

-- Index on the CrewMember table's tenantGuid,resourceId fields.
CREATE INDEX `I_CrewMember_tenantGuid_resourceId` ON `CrewMember` (`tenantGuid`, `resourceId`);

-- Index on the CrewMember table's tenantGuid,assignmentRoleId fields.
CREATE INDEX `I_CrewMember_tenantGuid_assignmentRoleId` ON `CrewMember` (`tenantGuid`, `assignmentRoleId`);

-- Index on the CrewMember table's tenantGuid,iconId fields.
CREATE INDEX `I_CrewMember_tenantGuid_iconId` ON `CrewMember` (`tenantGuid`, `iconId`);

-- Index on the CrewMember table's tenantGuid,active fields.
CREATE INDEX `I_CrewMember_tenantGuid_active` ON `CrewMember` (`tenantGuid`, `active`);

-- Index on the CrewMember table's tenantGuid,deleted fields.
CREATE INDEX `I_CrewMember_tenantGuid_deleted` ON `CrewMember` (`tenantGuid`, `deleted`);


-- The change history for records from the CrewMember table.
CREATE TABLE `CrewMemberChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`crewMemberId` INT NOT NULL,		-- Link to the CrewMember table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`crewMemberId`) REFERENCES `CrewMember`(`id`)		-- Foreign key to the CrewMember table.
);
-- Index on the CrewMemberChangeHistory table's tenantGuid field.
CREATE INDEX `I_CrewMemberChangeHistory_tenantGuid` ON `CrewMemberChangeHistory` (`tenantGuid`);

-- Index on the CrewMemberChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_CrewMemberChangeHistory_tenantGuid_versionNumber` ON `CrewMemberChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the CrewMemberChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_CrewMemberChangeHistory_tenantGuid_timeStamp` ON `CrewMemberChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the CrewMemberChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_CrewMemberChangeHistory_tenantGuid_userId` ON `CrewMemberChangeHistory` (`tenantGuid`, `userId`);

-- Index on the CrewMemberChangeHistory table's tenantGuid,crewMemberId fields.
CREATE INDEX `I_CrewMemberChangeHistory_tenantGuid_crewMemberId` ON `CrewMemberChangeHistory` (`tenantGuid`, `crewMemberId`, `versionNumber`, `timeStamp`, `userId`);


-- Pre-defined event templates for common appointment/activity types.   Includes default duration, required roles, default assignments, etc.
CREATE TABLE `ScheduledEventTemplate`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`defaultAllDay` BIT NOT NULL,		-- Default all day flag.
	`defaultDurationMinutes` INT NOT NULL DEFAULT 60,
	`schedulingTargetTypeId` INT NULL,		-- Optional target type
	`priorityId` INT NULL,		-- Optional priority
	`defaultLocationPattern` VARCHAR(250) NULL,		-- e.g., 'Patient Home', 'Main Site'
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`schedulingTargetTypeId`) REFERENCES `SchedulingTargetType`(`id`),		-- Foreign key to the SchedulingTargetType table.
	FOREIGN KEY (`priorityId`) REFERENCES `Priority`(`id`),		-- Foreign key to the Priority table.
	UNIQUE `UC_ScheduledEventTemplate_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the ScheduledEventTemplate table's tenantGuid and name fields.
);
-- Index on the ScheduledEventTemplate table's tenantGuid field.
CREATE INDEX `I_ScheduledEventTemplate_tenantGuid` ON `ScheduledEventTemplate` (`tenantGuid`);

-- Index on the ScheduledEventTemplate table's tenantGuid,name fields.
CREATE INDEX `I_ScheduledEventTemplate_tenantGuid_name` ON `ScheduledEventTemplate` (`tenantGuid`, `name`);

-- Index on the ScheduledEventTemplate table's tenantGuid,schedulingTargetTypeId fields.
CREATE INDEX `I_ScheduledEventTemplate_tenantGuid_schedulingTargetTypeId` ON `ScheduledEventTemplate` (`tenantGuid`, `schedulingTargetTypeId`);

-- Index on the ScheduledEventTemplate table's tenantGuid,active fields.
CREATE INDEX `I_ScheduledEventTemplate_tenantGuid_active` ON `ScheduledEventTemplate` (`tenantGuid`, `active`);

-- Index on the ScheduledEventTemplate table's tenantGuid,deleted fields.
CREATE INDEX `I_ScheduledEventTemplate_tenantGuid_deleted` ON `ScheduledEventTemplate` (`tenantGuid`, `deleted`);


-- The change history for records from the ScheduledEventTemplate table.
CREATE TABLE `ScheduledEventTemplateChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`scheduledEventTemplateId` INT NOT NULL,		-- Link to the ScheduledEventTemplate table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`scheduledEventTemplateId`) REFERENCES `ScheduledEventTemplate`(`id`)		-- Foreign key to the ScheduledEventTemplate table.
);
-- Index on the ScheduledEventTemplateChangeHistory table's tenantGuid field.
CREATE INDEX `I_ScheduledEventTemplateChangeHistory_tenantGuid` ON `ScheduledEventTemplateChangeHistory` (`tenantGuid`);

-- Index on the ScheduledEventTemplateChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ScheduledEventTemplateChangeHistory_tenantGuid_versionNumber` ON `ScheduledEventTemplateChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ScheduledEventTemplateChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ScheduledEventTemplateChangeHistory_tenantGuid_timeStamp` ON `ScheduledEventTemplateChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ScheduledEventTemplateChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ScheduledEventTemplateChangeHistory_tenantGuid_userId` ON `ScheduledEventTemplateChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ScheduledEventTemplateChangeHistory table's tenantGuid,scheduledEventTemplateId fields.
CREATE INDEX `I_ScheduledEventTemplateChangeHistory_tenantGuid_scheduledEventT` ON `ScheduledEventTemplateChangeHistory` (`tenantGuid`, `scheduledEventTemplateId`, `versionNumber`, `timeStamp`, `userId`);


/*
====================================================================================================
 SCHEDULED EVENT TEMPLATE CHARGES (For Auto-Dropping)
 Defines default charges for ScheduledEventTemplate).
 When an event is created from a template, these charges are auto-dropped onto the event.
 ====================================================================================================
*/
CREATE TABLE `ScheduledEventTemplateCharge`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`scheduledEventTemplateId` INT NOT NULL,		-- Link to ScheduledEventTemplate
	`chargeTypeId` INT NOT NULL,		-- Link to ChargeType (the charge to drop).
	`defaultAmount` DECIMAL(11,2) NOT NULL,		-- The amount to auto-drop (can be overridden on event).
	`isRequired` BIT NOT NULL DEFAULT 1,		-- some default charges might be optional (e.g., optional add-on fee).
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`scheduledEventTemplateId`) REFERENCES `ScheduledEventTemplate`(`id`),		-- Foreign key to the ScheduledEventTemplate table.
	FOREIGN KEY (`chargeTypeId`) REFERENCES `ChargeType`(`id`)		-- Foreign key to the ChargeType table.
);
-- Index on the ScheduledEventTemplateCharge table's tenantGuid field.
CREATE INDEX `I_ScheduledEventTemplateCharge_tenantGuid` ON `ScheduledEventTemplateCharge` (`tenantGuid`);

-- Index on the ScheduledEventTemplateCharge table's tenantGuid,scheduledEventTemplateId fields.
CREATE INDEX `I_ScheduledEventTemplateCharge_tenantGuid_scheduledEventTemplate` ON `ScheduledEventTemplateCharge` (`tenantGuid`, `scheduledEventTemplateId`);

-- Index on the ScheduledEventTemplateCharge table's tenantGuid,chargeTypeId fields.
CREATE INDEX `I_ScheduledEventTemplateCharge_tenantGuid_chargeTypeId` ON `ScheduledEventTemplateCharge` (`tenantGuid`, `chargeTypeId`);

-- Index on the ScheduledEventTemplateCharge table's tenantGuid,active fields.
CREATE INDEX `I_ScheduledEventTemplateCharge_tenantGuid_active` ON `ScheduledEventTemplateCharge` (`tenantGuid`, `active`);

-- Index on the ScheduledEventTemplateCharge table's tenantGuid,deleted fields.
CREATE INDEX `I_ScheduledEventTemplateCharge_tenantGuid_deleted` ON `ScheduledEventTemplateCharge` (`tenantGuid`, `deleted`);


-- The change history for records from the ScheduledEventTemplateCharge table.
CREATE TABLE `ScheduledEventTemplateChargeChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`scheduledEventTemplateChargeId` INT NOT NULL,		-- Link to the ScheduledEventTemplateCharge table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`scheduledEventTemplateChargeId`) REFERENCES `ScheduledEventTemplateCharge`(`id`)		-- Foreign key to the ScheduledEventTemplateCharge table.
);
-- Index on the ScheduledEventTemplateChargeChangeHistory table's tenantGuid field.
CREATE INDEX `I_ScheduledEventTemplateChargeChangeHistory_tenantGuid` ON `ScheduledEventTemplateChargeChangeHistory` (`tenantGuid`);

-- Index on the ScheduledEventTemplateChargeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ScheduledEventTemplateChargeChangeHistory_tenantGuid_versionNu` ON `ScheduledEventTemplateChargeChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ScheduledEventTemplateChargeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ScheduledEventTemplateChargeChangeHistory_tenantGuid_timeStamp` ON `ScheduledEventTemplateChargeChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ScheduledEventTemplateChargeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ScheduledEventTemplateChargeChangeHistory_tenantGuid_userId` ON `ScheduledEventTemplateChargeChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ScheduledEventTemplateChargeChangeHistory table's tenantGuid,scheduledEventTemplateChargeId fields.
CREATE INDEX `I_ScheduledEventTemplateChargeChangeHistory_tenantGuid_scheduled` ON `ScheduledEventTemplateChargeChangeHistory` (`tenantGuid`, `scheduledEventTemplateChargeId`, `versionNumber`, `timeStamp`, `userId`);


-- Default qualification requirements for events created from a template.
CREATE TABLE `ScheduledEventTemplateQualificationRequirement`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`scheduledEventTemplateId` INT NOT NULL,		-- Link to the ScheduledEventTemplate table.
	`qualificationId` INT NOT NULL,		-- Link to the Qualification table.
	`isRequired` BIT NOT NULL DEFAULT 1,
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`scheduledEventTemplateId`) REFERENCES `ScheduledEventTemplate`(`id`),		-- Foreign key to the ScheduledEventTemplate table.
	FOREIGN KEY (`qualificationId`) REFERENCES `Qualification`(`id`),		-- Foreign key to the Qualification table.
	UNIQUE `UC_ScheduledEventTemplateQualificationRequirement_tenantGuid_scheduledEventTemplateId_qualificationId_Unique`( `tenantGuid`, `scheduledEventTemplateId`, `qualificationId` ) 		-- Uniqueness enforced on the ScheduledEventTemplateQualificationRequirement table's tenantGuid and scheduledEventTemplateId and qualificationId fields.
);
-- Index on the ScheduledEventTemplateQualificationRequirement table's tenantGuid field.
CREATE INDEX `I_ScheduledEventTemplateQualificationRequirement_tenantGuid` ON `ScheduledEventTemplateQualificationRequirement` (`tenantGuid`);

-- Index on the ScheduledEventTemplateQualificationRequirement table's tenantGuid,scheduledEventTemplateId fields.
CREATE INDEX `I_ScheduledEventTemplateQualificationRequirement_tenantGuid_sche` ON `ScheduledEventTemplateQualificationRequirement` (`tenantGuid`, `scheduledEventTemplateId`);

-- Index on the ScheduledEventTemplateQualificationRequirement table's tenantGuid,qualificationId fields.
CREATE INDEX `I_ScheduledEventTemplateQualificationRequirement_tenantGuid_qual` ON `ScheduledEventTemplateQualificationRequirement` (`tenantGuid`, `qualificationId`);

-- Index on the ScheduledEventTemplateQualificationRequirement table's tenantGuid,active fields.
CREATE INDEX `I_ScheduledEventTemplateQualificationRequirement_tenantGuid_acti` ON `ScheduledEventTemplateQualificationRequirement` (`tenantGuid`, `active`);

-- Index on the ScheduledEventTemplateQualificationRequirement table's tenantGuid,deleted fields.
CREATE INDEX `I_ScheduledEventTemplateQualificationRequirement_tenantGuid_dele` ON `ScheduledEventTemplateQualificationRequirement` (`tenantGuid`, `deleted`);


-- The change history for records from the ScheduledEventTemplateQualificationRequirement table.
CREATE TABLE `ScheduledEventTemplateQualificationRequirementChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`scheduledEventTemplateQualificationRequirementId` INT NOT NULL,		-- Link to the ScheduledEventTemplateQualificationRequirement table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`scheduledEventTemplateQualificationRequirementId`) REFERENCES `ScheduledEventTemplateQualificationRequirement`(`id`)		-- Foreign key to the ScheduledEventTemplateQualificationRequirement table.
);
-- Index on the ScheduledEventTemplateQualificationRequirementChangeHistory table's tenantGuid field.
CREATE INDEX `I_ScheduledEventTemplateQualificationRequirementChangeHistory_te` ON `ScheduledEventTemplateQualificationRequirementChangeHistory` (`tenantGuid`);

-- Index on the ScheduledEventTemplateQualificationRequirementChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ScheduledEventTemplateQualificationRequirementChangeHistory_te` ON `ScheduledEventTemplateQualificationRequirementChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ScheduledEventTemplateQualificationRequirementChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ScheduledEventTemplateQualificationRequirementChangeHistory_te` ON `ScheduledEventTemplateQualificationRequirementChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ScheduledEventTemplateQualificationRequirementChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ScheduledEventTemplateQualificationRequirementChangeHistory_te` ON `ScheduledEventTemplateQualificationRequirementChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ScheduledEventTemplateQualificationRequirementChangeHistory table's tenantGuid,scheduledEventTemplateQualificationRequirementId fields.
CREATE INDEX `I_ScheduledEventTemplateQualificationRequirementChangeHistory_te` ON `ScheduledEventTemplateQualificationRequirementChangeHistory` (`tenantGuid`, `scheduledEventTemplateQualificationRequirementId`, `versionNumber`, `timeStamp`, `userId`);


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
CREATE TABLE `EventType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`color` VARCHAR(10) NULL,		-- Hex color for UI display and calendar event color-coding.
	`iconId` INT NULL,		-- Icon to use for UI display.
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`requiresRentalAgreement` BIT NOT NULL DEFAULT 0,		-- Whether events of this type require a signed rental agreement.
	`requiresExternalContact` BIT NOT NULL DEFAULT 0,		-- Whether events of this type require external contact info (name, email, phone).
	`requiresPayment` BIT NOT NULL DEFAULT 0,		-- Whether events of this type require payment tracking.
	`requiresDeposit` BIT NOT NULL DEFAULT 0,		-- Whether events of this type require a deposit (uses EventCharge.isDeposit).
	`requiresBarService` BIT NOT NULL DEFAULT 0,		-- Whether events of this type default to needing bar service (alcohol + bartender staffing). Can be overridden per event.
	`allowsTicketSales` BIT NOT NULL DEFAULT 0,		-- Whether events of this type support ticket sales tracking.
	`isInternalEvent` BIT NOT NULL DEFAULT 0,		-- True for committee-run events; false for private rentals. Drives which booking flow is shown in simple mode.
	`defaultPrice` DECIMAL(11,2) NULL,		-- Default rental price for events of this type. Used to auto-populate charges in the booking flow.
	`chargeTypeId` INT NULL,		-- Default charge type for auto-created charges.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	FOREIGN KEY (`chargeTypeId`) REFERENCES `ChargeType`(`id`),		-- Foreign key to the ChargeType table.
	UNIQUE `UC_EventType_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the EventType table's tenantGuid and name fields.
);
-- Index on the EventType table's tenantGuid field.
CREATE INDEX `I_EventType_tenantGuid` ON `EventType` (`tenantGuid`);

-- Index on the EventType table's tenantGuid,name fields.
CREATE INDEX `I_EventType_tenantGuid_name` ON `EventType` (`tenantGuid`, `name`);

-- Index on the EventType table's tenantGuid,iconId fields.
CREATE INDEX `I_EventType_tenantGuid_iconId` ON `EventType` (`tenantGuid`, `iconId`);

-- Index on the EventType table's tenantGuid,chargeTypeId fields.
CREATE INDEX `I_EventType_tenantGuid_chargeTypeId` ON `EventType` (`tenantGuid`, `chargeTypeId`);

-- Index on the EventType table's tenantGuid,active fields.
CREATE INDEX `I_EventType_tenantGuid_active` ON `EventType` (`tenantGuid`, `active`);

-- Index on the EventType table's tenantGuid,deleted fields.
CREATE INDEX `I_EventType_tenantGuid_deleted` ON `EventType` (`tenantGuid`, `deleted`);


-- The change history for records from the EventType table.
CREATE TABLE `EventTypeChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`eventTypeId` INT NOT NULL,		-- Link to the EventType table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`eventTypeId`) REFERENCES `EventType`(`id`)		-- Foreign key to the EventType table.
);
-- Index on the EventTypeChangeHistory table's tenantGuid field.
CREATE INDEX `I_EventTypeChangeHistory_tenantGuid` ON `EventTypeChangeHistory` (`tenantGuid`);

-- Index on the EventTypeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_EventTypeChangeHistory_tenantGuid_versionNumber` ON `EventTypeChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the EventTypeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_EventTypeChangeHistory_tenantGuid_timeStamp` ON `EventTypeChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the EventTypeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_EventTypeChangeHistory_tenantGuid_userId` ON `EventTypeChangeHistory` (`tenantGuid`, `userId`);

-- Index on the EventTypeChangeHistory table's tenantGuid,eventTypeId fields.
CREATE INDEX `I_EventTypeChangeHistory_tenantGuid_eventTypeId` ON `EventTypeChangeHistory` (`tenantGuid`, `eventTypeId`, `versionNumber`, `timeStamp`, `userId`);


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
CREATE TABLE `ScheduledEvent`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`officeId` INT NULL,		-- Snapshot of office that the first resource assigned to this event belongs to.  This should NOT be updated if a resource moves to a different office post-event assignment.  It should only change if there was an original entry error that needs to be corrected.
	`clientId` INT NULL,		-- Snapshot of client that this event belongs to.  It should be that of the scheduling target.  It should only change if there was an original entry error that needs to be corrected.
	`scheduledEventTemplateId` INT NULL,		-- Optional template/type of this scheduled event.
	`recurrenceRuleId` INT NULL,		-- Optional recurrence pattern for this event series
	`schedulingTargetId` INT NULL,		-- The SchedulingTarget (project, patient, etc.) this event is scheduled into
	`timeZoneId` INT NULL,		-- Link to the TimeZone table.
	`parentScheduledEventId` INT NULL,		-- If populated, this Event is a specific "Detached" instance of a Series
	`recurrenceInstanceDate` DATETIME NULL,		-- The original date this instance represented (crucial for matching with RecurrenceException)
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`isAllDay` BIT NULL DEFAULT 0,		-- Whether this is an all day event or not
	`startDateTime` DATETIME NOT NULL,		-- Inclusive start of the event in UTC
	`endDateTime` DATETIME NOT NULL,		-- Exclusive end of the event in UTC
	`location` VARCHAR(250) NULL,
	`eventStatusId` INT NOT NULL,		-- Status for the event
	`resourceId` INT NULL,		-- Optional primary/lead resource for the event
	`crewId` INT NULL,		-- Optional primary/lead crew for the event
	`priorityId` INT NULL,		-- Optional priority
	`bookingSourceTypeId` INT NULL,		-- Optional booking source for reservation type workflows.
	`eventTypeId` INT NULL,		-- Event type category — drives UI behavior (rental vs committee event flow, required fields, default pricing).
	`partySize` INT NULL,		-- Optional for use when running as a reservation system
	`bookingContactName` VARCHAR(250) NULL,		-- Name of the person booking (e.g., hall renter). Supports quick data entry without creating a full Contact.
	`bookingContactEmail` VARCHAR(250) NULL,		-- Email of the person booking.
	`bookingContactPhone` VARCHAR(50) NULL,		-- Phone number of the person booking.
	`notes` TEXT NULL,
	`color` VARCHAR(10) NULL,		-- Override Hex color for UI display
	`externalId` VARCHAR(100) NULL,		-- Optional link to an entity in another system
	`attributes` TEXT NULL,		-- to store arbitrary JSON
	`isOpenForVolunteers` BIT NOT NULL DEFAULT 0,		-- Whether this event appears in the Volunteer Hub opportunity browser for self-sign-up
	`maxVolunteerSlots` INT NULL,		-- Maximum number of volunteer sign-ups allowed; NULL = unlimited
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`officeId`) REFERENCES `Office`(`id`),		-- Foreign key to the Office table.
	FOREIGN KEY (`clientId`) REFERENCES `Client`(`id`),		-- Foreign key to the Client table.
	FOREIGN KEY (`scheduledEventTemplateId`) REFERENCES `ScheduledEventTemplate`(`id`),		-- Foreign key to the ScheduledEventTemplate table.
	FOREIGN KEY (`recurrenceRuleId`) REFERENCES `RecurrenceRule`(`id`),		-- Foreign key to the RecurrenceRule table.
	FOREIGN KEY (`schedulingTargetId`) REFERENCES `SchedulingTarget`(`id`),		-- Foreign key to the SchedulingTarget table.
	FOREIGN KEY (`timeZoneId`) REFERENCES `TimeZone`(`id`),		-- Foreign key to the TimeZone table.
	FOREIGN KEY (`parentScheduledEventId`) REFERENCES `ScheduledEvent`(`id`),		-- Foreign key to the ScheduledEvent table.
	FOREIGN KEY (`eventStatusId`) REFERENCES `EventStatus`(`id`),		-- Foreign key to the EventStatus table.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`),		-- Foreign key to the Resource table.
	FOREIGN KEY (`crewId`) REFERENCES `Crew`(`id`),		-- Foreign key to the Crew table.
	FOREIGN KEY (`priorityId`) REFERENCES `Priority`(`id`),		-- Foreign key to the Priority table.
	FOREIGN KEY (`bookingSourceTypeId`) REFERENCES `BookingSourceType`(`id`),		-- Foreign key to the BookingSourceType table.
	FOREIGN KEY (`eventTypeId`) REFERENCES `EventType`(`id`),		-- Foreign key to the EventType table.
	UNIQUE `UC_ScheduledEvent_tenantGuid_name_startDateTime_Unique`( `tenantGuid`, `name`, `startDateTime` ) 		-- Uniqueness enforced on the ScheduledEvent table's tenantGuid and name and startDateTime fields.
);
-- Index on the ScheduledEvent table's tenantGuid field.
CREATE INDEX `I_ScheduledEvent_tenantGuid` ON `ScheduledEvent` (`tenantGuid`);

-- Index on the ScheduledEvent table's tenantGuid,officeId fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_officeId` ON `ScheduledEvent` (`tenantGuid`, `officeId`);

-- Index on the ScheduledEvent table's tenantGuid,clientId fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_clientId` ON `ScheduledEvent` (`tenantGuid`, `clientId`);

-- Index on the ScheduledEvent table's tenantGuid,scheduledEventTemplateId fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_scheduledEventTemplateId` ON `ScheduledEvent` (`tenantGuid`, `scheduledEventTemplateId`);

-- Index on the ScheduledEvent table's tenantGuid,recurrenceRuleId fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_recurrenceRuleId` ON `ScheduledEvent` (`tenantGuid`, `recurrenceRuleId`);

-- Index on the ScheduledEvent table's tenantGuid,schedulingTargetId fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_schedulingTargetId` ON `ScheduledEvent` (`tenantGuid`, `schedulingTargetId`);

-- Index on the ScheduledEvent table's tenantGuid,timeZoneId fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_timeZoneId` ON `ScheduledEvent` (`tenantGuid`, `timeZoneId`);

-- Index on the ScheduledEvent table's tenantGuid,parentScheduledEventId fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_parentScheduledEventId` ON `ScheduledEvent` (`tenantGuid`, `parentScheduledEventId`);

-- Index on the ScheduledEvent table's tenantGuid,name fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_name` ON `ScheduledEvent` (`tenantGuid`, `name`);

-- Index on the ScheduledEvent table's tenantGuid,startDateTime fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_startDateTime` ON `ScheduledEvent` (`tenantGuid`, `startDateTime`);

-- Index on the ScheduledEvent table's tenantGuid,endDateTime fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_endDateTime` ON `ScheduledEvent` (`tenantGuid`, `endDateTime`);

-- Index on the ScheduledEvent table's tenantGuid,location fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_location` ON `ScheduledEvent` (`tenantGuid`, `location`);

-- Index on the ScheduledEvent table's tenantGuid,eventStatusId fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_eventStatusId` ON `ScheduledEvent` (`tenantGuid`, `eventStatusId`);

-- Index on the ScheduledEvent table's tenantGuid,resourceId fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_resourceId` ON `ScheduledEvent` (`tenantGuid`, `resourceId`);

-- Index on the ScheduledEvent table's tenantGuid,crewId fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_crewId` ON `ScheduledEvent` (`tenantGuid`, `crewId`);

-- Index on the ScheduledEvent table's tenantGuid,priorityId fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_priorityId` ON `ScheduledEvent` (`tenantGuid`, `priorityId`);

-- Index on the ScheduledEvent table's tenantGuid,bookingSourceTypeId fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_bookingSourceTypeId` ON `ScheduledEvent` (`tenantGuid`, `bookingSourceTypeId`);

-- Index on the ScheduledEvent table's tenantGuid,eventTypeId fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_eventTypeId` ON `ScheduledEvent` (`tenantGuid`, `eventTypeId`);

-- Index on the ScheduledEvent table's tenantGuid,active fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_active` ON `ScheduledEvent` (`tenantGuid`, `active`);

-- Index on the ScheduledEvent table's tenantGuid,deleted fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_deleted` ON `ScheduledEvent` (`tenantGuid`, `deleted`);

-- Index on the ScheduledEvent table's tenantGuid,startDateTime,endDateTime fields.
CREATE INDEX `I_ScheduledEvent_tenantGuid_startDateTime_endDateTime` ON `ScheduledEvent` (`tenantGuid`, `startDateTime`, `endDateTime`);


-- The change history for records from the ScheduledEvent table.
CREATE TABLE `ScheduledEventChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`scheduledEventId` INT NOT NULL,		-- Link to the ScheduledEvent table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`scheduledEventId`) REFERENCES `ScheduledEvent`(`id`)		-- Foreign key to the ScheduledEvent table.
);
-- Index on the ScheduledEventChangeHistory table's tenantGuid field.
CREATE INDEX `I_ScheduledEventChangeHistory_tenantGuid` ON `ScheduledEventChangeHistory` (`tenantGuid`);

-- Index on the ScheduledEventChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ScheduledEventChangeHistory_tenantGuid_versionNumber` ON `ScheduledEventChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ScheduledEventChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ScheduledEventChangeHistory_tenantGuid_timeStamp` ON `ScheduledEventChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ScheduledEventChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ScheduledEventChangeHistory_tenantGuid_userId` ON `ScheduledEventChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ScheduledEventChangeHistory table's tenantGuid,scheduledEventId fields.
CREATE INDEX `I_ScheduledEventChangeHistory_tenantGuid_scheduledEventId` ON `ScheduledEventChangeHistory` (`tenantGuid`, `scheduledEventId`, `versionNumber`, `timeStamp`, `userId`);


-- Master list of charge statuses (Pending, Approved, Invoiced, Void). Not tenant-specific because workflow will be tied to these values.  Could be redesigned later to get really fancy for tenant specific workflow, but out of scope for now.
CREATE TABLE `ChargeStatus`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the ChargeStatus table's name field.
CREATE INDEX `I_ChargeStatus_name` ON `ChargeStatus` (`name`);

-- Index on the ChargeStatus table's active field.
CREATE INDEX `I_ChargeStatus_active` ON `ChargeStatus` (`active`);

-- Index on the ChargeStatus table's deleted field.
CREATE INDEX `I_ChargeStatus_deleted` ON `ChargeStatus` (`deleted`);

INSERT INTO `ChargeStatus` ( `name`, `description`, `sequence`, `color`, `objectGuid` ) VALUES  ( 'Pending', 'Pending Approval', 1, '#B8FFC3', '1379f1da-c3cc-4149-998a-95ffa1728db6' );

INSERT INTO `ChargeStatus` ( `name`, `description`, `sequence`, `color`, `objectGuid` ) VALUES  ( 'Approved', 'Approved ', 2, '#59FF6F', 'ea16c955-9ccf-4489-acc0-0757c39ac3b6' );

INSERT INTO `ChargeStatus` ( `name`, `description`, `sequence`, `color`, `objectGuid` ) VALUES  ( 'Invoiced', 'Invoiced', 3, '#35A145', 'd250cc5c-51e9-49bb-91ce-4be47fc30dc0' );

INSERT INTO `ChargeStatus` ( `name`, `description`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'Void', 'Void - Charge Disregarded', '#C62828', 4, '19d6560f-ed85-4d1e-905f-9a6e3dfb3026' );


-- The change history for records from the ChargeStatus table.
CREATE TABLE `ChargeStatusChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`chargeStatusId` INT NOT NULL,		-- Link to the ChargeStatus table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`chargeStatusId`) REFERENCES `ChargeStatus`(`id`)		-- Foreign key to the ChargeStatus table.
);
-- Index on the ChargeStatusChangeHistory table's versionNumber field.
CREATE INDEX `I_ChargeStatusChangeHistory_versionNumber` ON `ChargeStatusChangeHistory` (`versionNumber`);

-- Index on the ChargeStatusChangeHistory table's timeStamp field.
CREATE INDEX `I_ChargeStatusChangeHistory_timeStamp` ON `ChargeStatusChangeHistory` (`timeStamp`);

-- Index on the ChargeStatusChangeHistory table's userId field.
CREATE INDEX `I_ChargeStatusChangeHistory_userId` ON `ChargeStatusChangeHistory` (`userId`);

-- Index on the ChargeStatusChangeHistory table's chargeStatusId field.
CREATE INDEX `I_ChargeStatusChangeHistory_chargeStatusId` ON `ChargeStatusChangeHistory` (`chargeStatusId`, `versionNumber`, `timeStamp`, `userId`);


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
CREATE TABLE `EventCharge`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`scheduledEventId` INT NOT NULL,		-- Link to the ScheduledEvent table.
	`resourceId` INT NULL,		-- Optional link to resource to bind charge to specific resources (e.g., labor cost per operator
	`chargeTypeId` INT NOT NULL,		-- Link to the ChargeType table (defines revenue/expense category).
	`chargeStatusId` INT NOT NULL,		-- Link to the ChargeStatus table.  Tracks the status of the charge from creation through invoicing or cancelling.
	`quantity` NUMERIC(38,22) NULL DEFAULT 1,		-- Quantity (hours, units, km, etc.)
	`unitPrice` DECIMAL(11,2) NULL,		-- Price per unit (can be NULL for flat fees)
	`extendedAmount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Pre-tax amount (quantity × unitPrice, or just amount for flat fees).
	`taxAmount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- The calculated tax based on TaxCode.rate.
	`totalAmount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Total amount inclusive of tax (extendedAmount + taxAmount). Consistent with FinancialTransaction.totalAmount.
	`description` VARCHAR(250) NULL,		-- Short description or label for the charge. Falls back to ChargeType.name if not set.
	`currencyId` INT NOT NULL,		-- Link to Currency table.
	`rateTypeId` INT NULL,		-- Optional link to RateType (e.g., 'Overtime').
	`notes` TEXT NULL,		-- Optional notes about the charge
	`isAutomatic` BIT NOT NULL DEFAULT 1,		-- 1 = auto-dropped from event type, 0 = manual add/edit.
	`isDeposit` BIT NOT NULL DEFAULT 0,		-- Marks this charge as a refundable deposit (e.g., damage deposit for hall rental).
	`depositRefundedDate` DATETIME NULL,		-- When the deposit was refunded (null = not yet refunded). Only applicable when isDeposit = true.
	`exportedDate` DATETIME NULL,		-- When this charge was last exported (null = not exported yet).
	`externalId` VARCHAR(100) NULL,		-- Identifier from extenral system - possibly invoice number or some other billing grouper
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	`taxCodeId` INT NULL,		-- Optional link to TaxCode. When set, indicates which tax rate was applied to calculate taxAmount. Inherited from ChargeType.taxCodeId on creation.
	FOREIGN KEY (`scheduledEventId`) REFERENCES `ScheduledEvent`(`id`),		-- Foreign key to the ScheduledEvent table.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`),		-- Foreign key to the Resource table.
	FOREIGN KEY (`chargeTypeId`) REFERENCES `ChargeType`(`id`),		-- Foreign key to the ChargeType table.
	FOREIGN KEY (`chargeStatusId`) REFERENCES `ChargeStatus`(`id`),		-- Foreign key to the ChargeStatus table.
	FOREIGN KEY (`currencyId`) REFERENCES `Currency`(`id`),		-- Foreign key to the Currency table.
	FOREIGN KEY (`rateTypeId`) REFERENCES `RateType`(`id`),		-- Foreign key to the RateType table.
	FOREIGN KEY (`taxCodeId`) REFERENCES `TaxCode`(`id`)		-- Foreign key to the TaxCode table.
);
-- Index on the EventCharge table's tenantGuid field.
CREATE INDEX `I_EventCharge_tenantGuid` ON `EventCharge` (`tenantGuid`);

-- Index on the EventCharge table's tenantGuid,scheduledEventId fields.
CREATE INDEX `I_EventCharge_tenantGuid_scheduledEventId` ON `EventCharge` (`tenantGuid`, `scheduledEventId`);

-- Index on the EventCharge table's tenantGuid,resourceId fields.
CREATE INDEX `I_EventCharge_tenantGuid_resourceId` ON `EventCharge` (`tenantGuid`, `resourceId`);

-- Index on the EventCharge table's tenantGuid,chargeTypeId fields.
CREATE INDEX `I_EventCharge_tenantGuid_chargeTypeId` ON `EventCharge` (`tenantGuid`, `chargeTypeId`);

-- Index on the EventCharge table's tenantGuid,chargeStatusId fields.
CREATE INDEX `I_EventCharge_tenantGuid_chargeStatusId` ON `EventCharge` (`tenantGuid`, `chargeStatusId`);

-- Index on the EventCharge table's tenantGuid,currencyId fields.
CREATE INDEX `I_EventCharge_tenantGuid_currencyId` ON `EventCharge` (`tenantGuid`, `currencyId`);

-- Index on the EventCharge table's tenantGuid,rateTypeId fields.
CREATE INDEX `I_EventCharge_tenantGuid_rateTypeId` ON `EventCharge` (`tenantGuid`, `rateTypeId`);

-- Index on the EventCharge table's tenantGuid,externalId fields.
CREATE INDEX `I_EventCharge_tenantGuid_externalId` ON `EventCharge` (`tenantGuid`, `externalId`);

-- Index on the EventCharge table's tenantGuid,active fields.
CREATE INDEX `I_EventCharge_tenantGuid_active` ON `EventCharge` (`tenantGuid`, `active`);

-- Index on the EventCharge table's tenantGuid,deleted fields.
CREATE INDEX `I_EventCharge_tenantGuid_deleted` ON `EventCharge` (`tenantGuid`, `deleted`);

-- Index on the EventCharge table's tenantGuid,taxCodeId fields.
CREATE INDEX `I_EventCharge_tenantGuid_taxCodeId` ON `EventCharge` (`tenantGuid`, `taxCodeId`);


-- The change history for records from the EventCharge table.
CREATE TABLE `EventChargeChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`eventChargeId` INT NOT NULL,		-- Link to the EventCharge table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`eventChargeId`) REFERENCES `EventCharge`(`id`)		-- Foreign key to the EventCharge table.
);
-- Index on the EventChargeChangeHistory table's tenantGuid field.
CREATE INDEX `I_EventChargeChangeHistory_tenantGuid` ON `EventChargeChangeHistory` (`tenantGuid`);

-- Index on the EventChargeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_EventChargeChangeHistory_tenantGuid_versionNumber` ON `EventChargeChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the EventChargeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_EventChargeChangeHistory_tenantGuid_timeStamp` ON `EventChargeChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the EventChargeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_EventChargeChangeHistory_tenantGuid_userId` ON `EventChargeChangeHistory` (`tenantGuid`, `userId`);

-- Index on the EventChargeChangeHistory table's tenantGuid,eventChargeId fields.
CREATE INDEX `I_EventChargeChangeHistory_tenantGuid_eventChargeId` ON `EventChargeChangeHistory` (`tenantGuid`, `eventChargeId`, `versionNumber`, `timeStamp`, `userId`);


/*
====================================================================================================
 PERIOD STATUS
 Fiscal period workflow states (Open, In Review, Closed).
 System-defined reference data — not tenant-specific.

 DESIGN NOTE: Controls the accounting period lifecycle. Open periods accept transactions,
 In Review periods are being reconciled, Closed periods are finalized.
 =====================================================================================================
*/
CREATE TABLE `PeriodStatus`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`color` VARCHAR(10) NULL,		-- Hex color for UI display.
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the PeriodStatus table's name field.
CREATE INDEX `I_PeriodStatus_name` ON `PeriodStatus` (`name`);

-- Index on the PeriodStatus table's active field.
CREATE INDEX `I_PeriodStatus_active` ON `PeriodStatus` (`active`);

-- Index on the PeriodStatus table's deleted field.
CREATE INDEX `I_PeriodStatus_deleted` ON `PeriodStatus` (`deleted`);

INSERT INTO `PeriodStatus` ( `name`, `description`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'Open', 'Period is open and accepting transactions.', '#4CAF50', 1, 'b2c3d4e5-0001-4000-8000-000000000001' );

INSERT INTO `PeriodStatus` ( `name`, `description`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'In Review', 'Period is being reviewed and reconciled. No new transactions.', '#FF9800', 2, 'b2c3d4e5-0001-4000-8000-000000000002' );

INSERT INTO `PeriodStatus` ( `name`, `description`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'Closed', 'Period is finalized. No modifications allowed.', '#F44336', 3, 'b2c3d4e5-0001-4000-8000-000000000003' );


/*
====================================================================================================
 FISCAL PERIOD
 Tracks accounting periods (months, quarters, or custom periods) for financial reporting.
 Supports period-close controls to prevent modifications to finalized periods.

 DESIGN NOTE: Allows both calendar-year and fiscal-year configurations.
 periodStatusId links to PeriodStatus for workflow state (Open, In Review, Closed).
 =====================================================================================================
*/
CREATE TABLE `FiscalPeriod`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`startDate` DATETIME NOT NULL,		-- Period start date (inclusive).
	`endDate` DATETIME NOT NULL,		-- Period end date (inclusive).
	`periodType` VARCHAR(50) NOT NULL DEFAULT 'Month',		-- Period type: Month, Quarter, Year, Custom.
	`fiscalYear` INT NOT NULL,		-- The fiscal year this period belongs to.
	`periodNumber` INT NOT NULL,		-- Period number within the fiscal year (1-12 for months, 1-4 for quarters, 1 for year).
	`periodStatusId` INT NOT NULL,		-- Link to PeriodStatus — workflow state (Open, In Review, Closed). Replaces the old periodStatus string field.
	`closedDate` DATETIME NULL,		-- When the period was closed.
	`closedBy` VARCHAR(100) NULL,		-- User who closed the period.
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`periodStatusId`) REFERENCES `PeriodStatus`(`id`),		-- Foreign key to the PeriodStatus table.
	UNIQUE `UC_FiscalPeriod_tenantGuid_name_Unique`( `tenantGuid`, `name` ) ,		-- Uniqueness enforced on the FiscalPeriod table's tenantGuid and name fields.
	UNIQUE `UC_FiscalPeriod_tenantGuid_fiscalYear_periodNumber_Unique`( `tenantGuid`, `fiscalYear`, `periodNumber` ) 		-- Uniqueness enforced on the FiscalPeriod table's tenantGuid and fiscalYear and periodNumber fields.
);
-- Index on the FiscalPeriod table's tenantGuid field.
CREATE INDEX `I_FiscalPeriod_tenantGuid` ON `FiscalPeriod` (`tenantGuid`);

-- Index on the FiscalPeriod table's tenantGuid,name fields.
CREATE INDEX `I_FiscalPeriod_tenantGuid_name` ON `FiscalPeriod` (`tenantGuid`, `name`);

-- Index on the FiscalPeriod table's tenantGuid,periodStatusId fields.
CREATE INDEX `I_FiscalPeriod_tenantGuid_periodStatusId` ON `FiscalPeriod` (`tenantGuid`, `periodStatusId`);

-- Index on the FiscalPeriod table's tenantGuid,active fields.
CREATE INDEX `I_FiscalPeriod_tenantGuid_active` ON `FiscalPeriod` (`tenantGuid`, `active`);

-- Index on the FiscalPeriod table's tenantGuid,deleted fields.
CREATE INDEX `I_FiscalPeriod_tenantGuid_deleted` ON `FiscalPeriod` (`tenantGuid`, `deleted`);


-- The change history for records from the FiscalPeriod table.
CREATE TABLE `FiscalPeriodChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`fiscalPeriodId` INT NOT NULL,		-- Link to the FiscalPeriod table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`fiscalPeriodId`) REFERENCES `FiscalPeriod`(`id`)		-- Foreign key to the FiscalPeriod table.
);
-- Index on the FiscalPeriodChangeHistory table's tenantGuid field.
CREATE INDEX `I_FiscalPeriodChangeHistory_tenantGuid` ON `FiscalPeriodChangeHistory` (`tenantGuid`);

-- Index on the FiscalPeriodChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_FiscalPeriodChangeHistory_tenantGuid_versionNumber` ON `FiscalPeriodChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the FiscalPeriodChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_FiscalPeriodChangeHistory_tenantGuid_timeStamp` ON `FiscalPeriodChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the FiscalPeriodChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_FiscalPeriodChangeHistory_tenantGuid_userId` ON `FiscalPeriodChangeHistory` (`tenantGuid`, `userId`);

-- Index on the FiscalPeriodChangeHistory table's tenantGuid,fiscalPeriodId fields.
CREATE INDEX `I_FiscalPeriodChangeHistory_tenantGuid_fiscalPeriodId` ON `FiscalPeriodChangeHistory` (`tenantGuid`, `fiscalPeriodId`, `versionNumber`, `timeStamp`, `userId`);


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
CREATE TABLE `FinancialTransaction`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`financialCategoryId` INT NOT NULL,		-- Link to the FinancialCategory (chart of accounts entry).
	`financialOfficeId` INT NULL,		-- Optional link to FinancialOffice — scopes this transaction to a specific department/committee. Can be inherited from the FinancialCategory if not set directly.
	`scheduledEventId` INT NULL,		-- Optional link to a ScheduledEvent when the transaction relates to a booking.
	`contactId` INT NULL,		-- Optional link to the Contact who paid or was paid.
	`clientId` INT NULL,		-- Optional link to the Client (vendor, customer, or supplier) for this transaction.
	`contactRole` VARCHAR(50) NULL DEFAULT 'Customer',		-- Role of the linked contact: Customer, Vendor, Employee. Maps to QuickBooks entity types for sync.
	`taxCodeId` INT NULL,		-- Optional link to TaxCode. Overrides the category-level isTaxApplicable for precise tax handling.
	`fiscalPeriodId` INT NULL,		-- Optional link to FiscalPeriod. Auto-assigned based on transactionDate when null.
	`paymentTypeId` INT NULL,		-- Optional link to PaymentType indicating how payment was made (e-transfer, cash, cheque, card, etc.).
	`transactionDate` DATETIME NOT NULL,		-- When the transaction occurred (UTC).
	`description` VARCHAR(500) NOT NULL,		-- Description of the transaction (e.g., 'Easter Brunch Food', 'DD Refund - Natasha Chafe').
	`amount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Transaction amount before tax. Always positive — direction determined by FinancialCategory.accountType.
	`taxAmount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Tax amount (e.g., HST). Calculated from TaxCode.rate when applicable.
	`totalAmount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Total amount inclusive of tax (amount + taxAmount).
	`isRevenue` BIT NOT NULL DEFAULT 1,		-- Denormalized from FinancialCategory.accountType for query performance. True when accountType = 'Income', false otherwise.
	`journalEntryType` VARCHAR(50) NULL,		-- Double-entry type for accounting integration: Debit or Credit. Null = auto-determined from isRevenue.
	`referenceNumber` VARCHAR(100) NULL,		-- Cheque number, e-transfer reference, receipt number, etc.
	`notes` TEXT NULL,		-- Optional notes about the transaction.
	`currencyId` INT NOT NULL,		-- Link to Currency table.
	`exportedDate` DATETIME NULL,		-- When this transaction was last exported for reporting (null = not exported yet).
	`externalId` VARCHAR(100) NULL,		-- Identifier from external system (e.g., QuickBooks Transaction ID).
	`externalSystemName` VARCHAR(50) NULL,		-- Name of the external system (e.g., 'QuickBooks', 'Xero') for multi-system tracking.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`financialCategoryId`) REFERENCES `FinancialCategory`(`id`),		-- Foreign key to the FinancialCategory table.
	FOREIGN KEY (`financialOfficeId`) REFERENCES `FinancialOffice`(`id`),		-- Foreign key to the FinancialOffice table.
	FOREIGN KEY (`scheduledEventId`) REFERENCES `ScheduledEvent`(`id`),		-- Foreign key to the ScheduledEvent table.
	FOREIGN KEY (`contactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`clientId`) REFERENCES `Client`(`id`),		-- Foreign key to the Client table.
	FOREIGN KEY (`taxCodeId`) REFERENCES `TaxCode`(`id`),		-- Foreign key to the TaxCode table.
	FOREIGN KEY (`fiscalPeriodId`) REFERENCES `FiscalPeriod`(`id`),		-- Foreign key to the FiscalPeriod table.
	FOREIGN KEY (`paymentTypeId`) REFERENCES `PaymentType`(`id`),		-- Foreign key to the PaymentType table.
	FOREIGN KEY (`currencyId`) REFERENCES `Currency`(`id`)		-- Foreign key to the Currency table.
);
-- Index on the FinancialTransaction table's tenantGuid field.
CREATE INDEX `I_FinancialTransaction_tenantGuid` ON `FinancialTransaction` (`tenantGuid`);

-- Index on the FinancialTransaction table's tenantGuid,financialCategoryId fields.
CREATE INDEX `I_FinancialTransaction_tenantGuid_financialCategoryId` ON `FinancialTransaction` (`tenantGuid`, `financialCategoryId`);

-- Index on the FinancialTransaction table's tenantGuid,financialOfficeId fields.
CREATE INDEX `I_FinancialTransaction_tenantGuid_financialOfficeId` ON `FinancialTransaction` (`tenantGuid`, `financialOfficeId`);

-- Index on the FinancialTransaction table's tenantGuid,scheduledEventId fields.
CREATE INDEX `I_FinancialTransaction_tenantGuid_scheduledEventId` ON `FinancialTransaction` (`tenantGuid`, `scheduledEventId`);

-- Index on the FinancialTransaction table's tenantGuid,contactId fields.
CREATE INDEX `I_FinancialTransaction_tenantGuid_contactId` ON `FinancialTransaction` (`tenantGuid`, `contactId`);

-- Index on the FinancialTransaction table's tenantGuid,clientId fields.
CREATE INDEX `I_FinancialTransaction_tenantGuid_clientId` ON `FinancialTransaction` (`tenantGuid`, `clientId`);

-- Index on the FinancialTransaction table's tenantGuid,taxCodeId fields.
CREATE INDEX `I_FinancialTransaction_tenantGuid_taxCodeId` ON `FinancialTransaction` (`tenantGuid`, `taxCodeId`);

-- Index on the FinancialTransaction table's tenantGuid,fiscalPeriodId fields.
CREATE INDEX `I_FinancialTransaction_tenantGuid_fiscalPeriodId` ON `FinancialTransaction` (`tenantGuid`, `fiscalPeriodId`);

-- Index on the FinancialTransaction table's tenantGuid,paymentTypeId fields.
CREATE INDEX `I_FinancialTransaction_tenantGuid_paymentTypeId` ON `FinancialTransaction` (`tenantGuid`, `paymentTypeId`);

-- Index on the FinancialTransaction table's tenantGuid,transactionDate fields.
CREATE INDEX `I_FinancialTransaction_tenantGuid_transactionDate` ON `FinancialTransaction` (`tenantGuid`, `transactionDate`);

-- Index on the FinancialTransaction table's tenantGuid,currencyId fields.
CREATE INDEX `I_FinancialTransaction_tenantGuid_currencyId` ON `FinancialTransaction` (`tenantGuid`, `currencyId`);

-- Index on the FinancialTransaction table's tenantGuid,externalId fields.
CREATE INDEX `I_FinancialTransaction_tenantGuid_externalId` ON `FinancialTransaction` (`tenantGuid`, `externalId`);

-- Index on the FinancialTransaction table's tenantGuid,active fields.
CREATE INDEX `I_FinancialTransaction_tenantGuid_active` ON `FinancialTransaction` (`tenantGuid`, `active`);

-- Index on the FinancialTransaction table's tenantGuid,deleted fields.
CREATE INDEX `I_FinancialTransaction_tenantGuid_deleted` ON `FinancialTransaction` (`tenantGuid`, `deleted`);


-- The change history for records from the FinancialTransaction table.
CREATE TABLE `FinancialTransactionChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`financialTransactionId` INT NOT NULL,		-- Link to the FinancialTransaction table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`financialTransactionId`) REFERENCES `FinancialTransaction`(`id`)		-- Foreign key to the FinancialTransaction table.
);
-- Index on the FinancialTransactionChangeHistory table's tenantGuid field.
CREATE INDEX `I_FinancialTransactionChangeHistory_tenantGuid` ON `FinancialTransactionChangeHistory` (`tenantGuid`);

-- Index on the FinancialTransactionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_FinancialTransactionChangeHistory_tenantGuid_versionNumber` ON `FinancialTransactionChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the FinancialTransactionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_FinancialTransactionChangeHistory_tenantGuid_timeStamp` ON `FinancialTransactionChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the FinancialTransactionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_FinancialTransactionChangeHistory_tenantGuid_userId` ON `FinancialTransactionChangeHistory` (`tenantGuid`, `userId`);

-- Index on the FinancialTransactionChangeHistory table's tenantGuid,financialTransactionId fields.
CREATE INDEX `I_FinancialTransactionChangeHistory_tenantGuid_financialTransact` ON `FinancialTransactionChangeHistory` (`tenantGuid`, `financialTransactionId`, `versionNumber`, `timeStamp`, `userId`);


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
CREATE TABLE `Budget`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`financialCategoryId` INT NOT NULL,		-- The category this budget applies to.
	`fiscalPeriodId` INT NOT NULL,		-- The fiscal period this budget covers.
	`financialOfficeId` INT NULL,		-- Optional link to FinancialOffice — scopes this budget to a specific department/committee. Can be inherited from the FinancialCategory if not set directly.
	`budgetedAmount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- The planned/budgeted amount for this category in this period.
	`revisedAmount` DECIMAL(11,2) NULL,		-- Optional revised budget amount (after mid-period adjustments).
	`notes` TEXT NULL,		-- Optional notes about the budget line (e.g., justification for revision).
	`currencyId` INT NOT NULL,		-- Link to Currency table.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`financialCategoryId`) REFERENCES `FinancialCategory`(`id`),		-- Foreign key to the FinancialCategory table.
	FOREIGN KEY (`fiscalPeriodId`) REFERENCES `FiscalPeriod`(`id`),		-- Foreign key to the FiscalPeriod table.
	FOREIGN KEY (`financialOfficeId`) REFERENCES `FinancialOffice`(`id`),		-- Foreign key to the FinancialOffice table.
	FOREIGN KEY (`currencyId`) REFERENCES `Currency`(`id`),		-- Foreign key to the Currency table.
	UNIQUE `UC_Budget_tenantGuid_financialCategoryId_fiscalPeriodId_Unique`( `tenantGuid`, `financialCategoryId`, `fiscalPeriodId` ) 		-- Uniqueness enforced on the Budget table's tenantGuid and financialCategoryId and fiscalPeriodId fields.
);
-- Index on the Budget table's tenantGuid field.
CREATE INDEX `I_Budget_tenantGuid` ON `Budget` (`tenantGuid`);

-- Index on the Budget table's tenantGuid,financialCategoryId fields.
CREATE INDEX `I_Budget_tenantGuid_financialCategoryId` ON `Budget` (`tenantGuid`, `financialCategoryId`);

-- Index on the Budget table's tenantGuid,fiscalPeriodId fields.
CREATE INDEX `I_Budget_tenantGuid_fiscalPeriodId` ON `Budget` (`tenantGuid`, `fiscalPeriodId`);

-- Index on the Budget table's tenantGuid,financialOfficeId fields.
CREATE INDEX `I_Budget_tenantGuid_financialOfficeId` ON `Budget` (`tenantGuid`, `financialOfficeId`);

-- Index on the Budget table's tenantGuid,currencyId fields.
CREATE INDEX `I_Budget_tenantGuid_currencyId` ON `Budget` (`tenantGuid`, `currencyId`);

-- Index on the Budget table's tenantGuid,active fields.
CREATE INDEX `I_Budget_tenantGuid_active` ON `Budget` (`tenantGuid`, `active`);

-- Index on the Budget table's tenantGuid,deleted fields.
CREATE INDEX `I_Budget_tenantGuid_deleted` ON `Budget` (`tenantGuid`, `deleted`);


-- The change history for records from the Budget table.
CREATE TABLE `BudgetChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`budgetId` INT NOT NULL,		-- Link to the Budget table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`budgetId`) REFERENCES `Budget`(`id`)		-- Foreign key to the Budget table.
);
-- Index on the BudgetChangeHistory table's tenantGuid field.
CREATE INDEX `I_BudgetChangeHistory_tenantGuid` ON `BudgetChangeHistory` (`tenantGuid`);

-- Index on the BudgetChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_BudgetChangeHistory_tenantGuid_versionNumber` ON `BudgetChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the BudgetChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_BudgetChangeHistory_tenantGuid_timeStamp` ON `BudgetChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the BudgetChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_BudgetChangeHistory_tenantGuid_userId` ON `BudgetChangeHistory` (`tenantGuid`, `userId`);

-- Index on the BudgetChangeHistory table's tenantGuid,budgetId fields.
CREATE INDEX `I_BudgetChangeHistory_tenantGuid_budgetId` ON `BudgetChangeHistory` (`tenantGuid`, `budgetId`, `versionNumber`, `timeStamp`, `userId`);


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
CREATE TABLE `GeneralLedgerEntry`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`journalEntryNumber` INT NOT NULL,		-- Auto-incrementing per-tenant journal entry number for human reference.
	`transactionDate` DATETIME NOT NULL,		-- The date of the underlying financial event (UTC).
	`description` VARCHAR(500) NULL,		-- Description of the journal entry (e.g., 'Expense: Office Supplies', 'Revenue: Hall Rental').
	`referenceNumber` VARCHAR(100) NULL,		-- External reference — cheque number, receipt number, etc.
	`financialTransactionId` INT NULL,		-- Links back to the originating FinancialTransaction, if any.
	`fiscalPeriodId` INT NULL,		-- The fiscal period this entry belongs to.
	`financialOfficeId` INT NULL,		-- Optional link to FinancialOffice for departmental reporting.
	`postedBy` INT NOT NULL,		-- Security user id who posted this entry.
	`postedDate` DATETIME NOT NULL,		-- When this entry was posted (UTC).
	`reversalOfId` INT NULL,		-- If this is a reversal/correction, points to the original GeneralLedgerEntry id.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`financialTransactionId`) REFERENCES `FinancialTransaction`(`id`),		-- Foreign key to the FinancialTransaction table.
	FOREIGN KEY (`fiscalPeriodId`) REFERENCES `FiscalPeriod`(`id`),		-- Foreign key to the FiscalPeriod table.
	FOREIGN KEY (`financialOfficeId`) REFERENCES `FinancialOffice`(`id`)		-- Foreign key to the FinancialOffice table.
);
-- Index on the GeneralLedgerEntry table's tenantGuid field.
CREATE INDEX `I_GeneralLedgerEntry_tenantGuid` ON `GeneralLedgerEntry` (`tenantGuid`);

-- Index on the GeneralLedgerEntry table's tenantGuid,transactionDate fields.
CREATE INDEX `I_GeneralLedgerEntry_tenantGuid_transactionDate` ON `GeneralLedgerEntry` (`tenantGuid`, `transactionDate`);

-- Index on the GeneralLedgerEntry table's tenantGuid,financialTransactionId fields.
CREATE INDEX `I_GeneralLedgerEntry_tenantGuid_financialTransactionId` ON `GeneralLedgerEntry` (`tenantGuid`, `financialTransactionId`);

-- Index on the GeneralLedgerEntry table's tenantGuid,fiscalPeriodId fields.
CREATE INDEX `I_GeneralLedgerEntry_tenantGuid_fiscalPeriodId` ON `GeneralLedgerEntry` (`tenantGuid`, `fiscalPeriodId`);

-- Index on the GeneralLedgerEntry table's tenantGuid,financialOfficeId fields.
CREATE INDEX `I_GeneralLedgerEntry_tenantGuid_financialOfficeId` ON `GeneralLedgerEntry` (`tenantGuid`, `financialOfficeId`);

-- Index on the GeneralLedgerEntry table's tenantGuid,active fields.
CREATE INDEX `I_GeneralLedgerEntry_tenantGuid_active` ON `GeneralLedgerEntry` (`tenantGuid`, `active`);

-- Index on the GeneralLedgerEntry table's tenantGuid,deleted fields.
CREATE INDEX `I_GeneralLedgerEntry_tenantGuid_deleted` ON `GeneralLedgerEntry` (`tenantGuid`, `deleted`);


/*
====================================================================================================
 GENERAL LEDGER LINE
 Individual debit or credit line within a GeneralLedgerEntry.
 Each line posts to a specific FinancialCategory (account).

 CONSTRAINT: Within each GeneralLedgerEntry, sum(debitAmount) must equal sum(creditAmount).
 Exactly one of debitAmount/creditAmount should be non-zero per line.
 ====================================================================================================
*/
CREATE TABLE `GeneralLedgerLine`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`generalLedgerEntryId` INT NOT NULL,		-- The parent journal entry this line belongs to.
	`financialCategoryId` INT NOT NULL,		-- The account (FinancialCategory) this line posts to.
	`debitAmount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Debit amount. Zero if this line is a credit.
	`creditAmount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Credit amount. Zero if this line is a debit.
	`description` VARCHAR(500) NULL,		-- Optional line-level description.
	FOREIGN KEY (`generalLedgerEntryId`) REFERENCES `GeneralLedgerEntry`(`id`),		-- Foreign key to the GeneralLedgerEntry table.
	FOREIGN KEY (`financialCategoryId`) REFERENCES `FinancialCategory`(`id`)		-- Foreign key to the FinancialCategory table.
);
-- Index on the GeneralLedgerLine table's generalLedgerEntryId field.
CREATE INDEX `I_GeneralLedgerLine_generalLedgerEntryId` ON `GeneralLedgerLine` (`generalLedgerEntryId`);

-- Index on the GeneralLedgerLine table's financialCategoryId field.
CREATE INDEX `I_GeneralLedgerLine_financialCategoryId` ON `GeneralLedgerLine` (`financialCategoryId`);


-- Master list of payment methods (Cash, E-Transfer, Credit Card, Debit Card, Cheque).
CREATE TABLE `PaymentMethod`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`isElectronic` BIT NOT NULL DEFAULT 0,		-- True for card and e-transfer, false for cash and cheque.
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`color` VARCHAR(10) NULL,		-- Hex color for UI display.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the PaymentMethod table's name field.
CREATE INDEX `I_PaymentMethod_name` ON `PaymentMethod` (`name`);

-- Index on the PaymentMethod table's active field.
CREATE INDEX `I_PaymentMethod_active` ON `PaymentMethod` (`active`);

-- Index on the PaymentMethod table's deleted field.
CREATE INDEX `I_PaymentMethod_deleted` ON `PaymentMethod` (`deleted`);

INSERT INTO `PaymentMethod` ( `name`, `description`, `isElectronic`, `sequence`, `objectGuid` ) VALUES  ( 'Cash', 'Cash payment', 0, 1, 'b1a1b2c3-d4e5-6789-abcd-ef0123456701' );

INSERT INTO `PaymentMethod` ( `name`, `description`, `isElectronic`, `sequence`, `objectGuid` ) VALUES  ( 'E-Transfer', 'Interac e-Transfer', 1, 2, 'b1a1b2c3-d4e5-6789-abcd-ef0123456702' );

INSERT INTO `PaymentMethod` ( `name`, `description`, `isElectronic`, `sequence`, `objectGuid` ) VALUES  ( 'Cheque', 'Cheque payment', 0, 3, 'b1a1b2c3-d4e5-6789-abcd-ef0123456703' );

INSERT INTO `PaymentMethod` ( `name`, `description`, `isElectronic`, `sequence`, `objectGuid` ) VALUES  ( 'Credit Card', 'Credit card payment', 1, 4, 'b1a1b2c3-d4e5-6789-abcd-ef0123456704' );

INSERT INTO `PaymentMethod` ( `name`, `description`, `isElectronic`, `sequence`, `objectGuid` ) VALUES  ( 'Debit Card', 'Debit card payment', 1, 5, 'b1a1b2c3-d4e5-6789-abcd-ef0123456705' );


/*
====================================================================================================
 PAYMENT PROVIDER
 Configuration for electronic payment processor integrations (Stripe, Square, or Manual).
 Stores encrypted API keys and merchant account details.

 DESIGN NOTE: Starts with a 'Manual' provider for recording cash/cheque payments.
 Add Stripe/Square providers when ready for electronic payment acceptance.
 ====================================================================================================
*/
CREATE TABLE `PaymentProvider`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`providerType` VARCHAR(50) NOT NULL,		-- Provider type identifier: 'manual', 'stripe', 'square', 'moneris'.
	`isActive` BIT NOT NULL DEFAULT 1,		-- Whether this provider is currently active.
	`apiKeyEncrypted` TEXT NULL,		-- Encrypted API key for the payment provider.
	`merchantId` VARCHAR(100) NULL,		-- Merchant account identifier with the provider.
	`webhookSecret` TEXT NULL,		-- Encrypted webhook validation secret for the provider.
	`processingFeePercent` NUMERIC(38,22) NULL,		-- Provider processing fee percentage (e.g., 2.9 for Stripe).
	`processingFeeFixed` DECIMAL(11,2) NULL,		-- Provider fixed processing fee per transaction (e.g., $0.30).
	`notes` TEXT NULL,		-- Optional notes about the provider configuration.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE `UC_PaymentProvider_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the PaymentProvider table's tenantGuid and name fields.
);
-- Index on the PaymentProvider table's tenantGuid field.
CREATE INDEX `I_PaymentProvider_tenantGuid` ON `PaymentProvider` (`tenantGuid`);

-- Index on the PaymentProvider table's tenantGuid,name fields.
CREATE INDEX `I_PaymentProvider_tenantGuid_name` ON `PaymentProvider` (`tenantGuid`, `name`);

-- Index on the PaymentProvider table's tenantGuid,active fields.
CREATE INDEX `I_PaymentProvider_tenantGuid_active` ON `PaymentProvider` (`tenantGuid`, `active`);

-- Index on the PaymentProvider table's tenantGuid,deleted fields.
CREATE INDEX `I_PaymentProvider_tenantGuid_deleted` ON `PaymentProvider` (`tenantGuid`, `deleted`);


-- The change history for records from the PaymentProvider table.
CREATE TABLE `PaymentProviderChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`paymentProviderId` INT NOT NULL,		-- Link to the PaymentProvider table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`paymentProviderId`) REFERENCES `PaymentProvider`(`id`)		-- Foreign key to the PaymentProvider table.
);
-- Index on the PaymentProviderChangeHistory table's tenantGuid field.
CREATE INDEX `I_PaymentProviderChangeHistory_tenantGuid` ON `PaymentProviderChangeHistory` (`tenantGuid`);

-- Index on the PaymentProviderChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_PaymentProviderChangeHistory_tenantGuid_versionNumber` ON `PaymentProviderChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the PaymentProviderChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_PaymentProviderChangeHistory_tenantGuid_timeStamp` ON `PaymentProviderChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the PaymentProviderChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_PaymentProviderChangeHistory_tenantGuid_userId` ON `PaymentProviderChangeHistory` (`tenantGuid`, `userId`);

-- Index on the PaymentProviderChangeHistory table's tenantGuid,paymentProviderId fields.
CREATE INDEX `I_PaymentProviderChangeHistory_tenantGuid_paymentProviderId` ON `PaymentProviderChangeHistory` (`tenantGuid`, `paymentProviderId`, `versionNumber`, `timeStamp`, `userId`);


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
CREATE TABLE `PaymentTransaction`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`paymentMethodId` INT NOT NULL,		-- How the payment was made (Cash, E-Transfer, Credit Card, etc.).
	`paymentProviderId` INT NULL,		-- Optional link to payment processor (null for cash/cheque).
	`scheduledEventId` INT NULL,		-- Optional link to a ScheduledEvent (e.g., booking payment).
	`financialTransactionId` INT NULL,		-- Optional link to a FinancialTransaction (e.g., bar tab payment).
	`eventChargeId` INT NULL,		-- Optional link to a specific EventCharge (e.g., damage deposit payment).
	`transactionDate` DATETIME NOT NULL,		-- When the payment occurred (UTC).
	`amount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Gross payment amount.
	`processingFee` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Fee deducted by the payment provider.
	`netAmount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Net amount received (amount - processingFee).
	`currencyId` INT NOT NULL,		-- Link to Currency table.
	`status` VARCHAR(50) NOT NULL,		-- Payment status: pending, completed, failed, refunded.
	`providerTransactionId` VARCHAR(250) NULL,		-- Transaction ID from the payment provider (e.g., Stripe charge ID).
	`providerResponse` TEXT NULL,		-- JSON response from the payment provider for audit purposes.
	`payerName` VARCHAR(250) NULL,		-- Name of the person who paid.
	`payerEmail` VARCHAR(250) NULL,		-- Email of the payer.
	`payerPhone` VARCHAR(50) NULL,		-- Phone number of the payer.
	`receiptNumber` VARCHAR(100) NULL,		-- Generated receipt number.
	`notes` TEXT NULL,		-- Optional notes about the payment.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`paymentMethodId`) REFERENCES `PaymentMethod`(`id`),		-- Foreign key to the PaymentMethod table.
	FOREIGN KEY (`paymentProviderId`) REFERENCES `PaymentProvider`(`id`),		-- Foreign key to the PaymentProvider table.
	FOREIGN KEY (`scheduledEventId`) REFERENCES `ScheduledEvent`(`id`),		-- Foreign key to the ScheduledEvent table.
	FOREIGN KEY (`financialTransactionId`) REFERENCES `FinancialTransaction`(`id`),		-- Foreign key to the FinancialTransaction table.
	FOREIGN KEY (`eventChargeId`) REFERENCES `EventCharge`(`id`),		-- Foreign key to the EventCharge table.
	FOREIGN KEY (`currencyId`) REFERENCES `Currency`(`id`)		-- Foreign key to the Currency table.
);
-- Index on the PaymentTransaction table's tenantGuid field.
CREATE INDEX `I_PaymentTransaction_tenantGuid` ON `PaymentTransaction` (`tenantGuid`);

-- Index on the PaymentTransaction table's tenantGuid,paymentMethodId fields.
CREATE INDEX `I_PaymentTransaction_tenantGuid_paymentMethodId` ON `PaymentTransaction` (`tenantGuid`, `paymentMethodId`);

-- Index on the PaymentTransaction table's tenantGuid,paymentProviderId fields.
CREATE INDEX `I_PaymentTransaction_tenantGuid_paymentProviderId` ON `PaymentTransaction` (`tenantGuid`, `paymentProviderId`);

-- Index on the PaymentTransaction table's tenantGuid,scheduledEventId fields.
CREATE INDEX `I_PaymentTransaction_tenantGuid_scheduledEventId` ON `PaymentTransaction` (`tenantGuid`, `scheduledEventId`);

-- Index on the PaymentTransaction table's tenantGuid,financialTransactionId fields.
CREATE INDEX `I_PaymentTransaction_tenantGuid_financialTransactionId` ON `PaymentTransaction` (`tenantGuid`, `financialTransactionId`);

-- Index on the PaymentTransaction table's tenantGuid,eventChargeId fields.
CREATE INDEX `I_PaymentTransaction_tenantGuid_eventChargeId` ON `PaymentTransaction` (`tenantGuid`, `eventChargeId`);

-- Index on the PaymentTransaction table's tenantGuid,transactionDate fields.
CREATE INDEX `I_PaymentTransaction_tenantGuid_transactionDate` ON `PaymentTransaction` (`tenantGuid`, `transactionDate`);

-- Index on the PaymentTransaction table's tenantGuid,currencyId fields.
CREATE INDEX `I_PaymentTransaction_tenantGuid_currencyId` ON `PaymentTransaction` (`tenantGuid`, `currencyId`);

-- Index on the PaymentTransaction table's tenantGuid,providerTransactionId fields.
CREATE INDEX `I_PaymentTransaction_tenantGuid_providerTransactionId` ON `PaymentTransaction` (`tenantGuid`, `providerTransactionId`);

-- Index on the PaymentTransaction table's tenantGuid,receiptNumber fields.
CREATE INDEX `I_PaymentTransaction_tenantGuid_receiptNumber` ON `PaymentTransaction` (`tenantGuid`, `receiptNumber`);

-- Index on the PaymentTransaction table's tenantGuid,active fields.
CREATE INDEX `I_PaymentTransaction_tenantGuid_active` ON `PaymentTransaction` (`tenantGuid`, `active`);

-- Index on the PaymentTransaction table's tenantGuid,deleted fields.
CREATE INDEX `I_PaymentTransaction_tenantGuid_deleted` ON `PaymentTransaction` (`tenantGuid`, `deleted`);


-- The change history for records from the PaymentTransaction table.
CREATE TABLE `PaymentTransactionChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`paymentTransactionId` INT NOT NULL,		-- Link to the PaymentTransaction table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`paymentTransactionId`) REFERENCES `PaymentTransaction`(`id`)		-- Foreign key to the PaymentTransaction table.
);
-- Index on the PaymentTransactionChangeHistory table's tenantGuid field.
CREATE INDEX `I_PaymentTransactionChangeHistory_tenantGuid` ON `PaymentTransactionChangeHistory` (`tenantGuid`);

-- Index on the PaymentTransactionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_PaymentTransactionChangeHistory_tenantGuid_versionNumber` ON `PaymentTransactionChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the PaymentTransactionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_PaymentTransactionChangeHistory_tenantGuid_timeStamp` ON `PaymentTransactionChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the PaymentTransactionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_PaymentTransactionChangeHistory_tenantGuid_userId` ON `PaymentTransactionChangeHistory` (`tenantGuid`, `userId`);

-- Index on the PaymentTransactionChangeHistory table's tenantGuid,paymentTransactionId fields.
CREATE INDEX `I_PaymentTransactionChangeHistory_tenantGuid_paymentTransactionI` ON `PaymentTransactionChangeHistory` (`tenantGuid`, `paymentTransactionId`, `versionNumber`, `timeStamp`, `userId`);


/*
====================================================================================================
 INVOICE STATUS
 Workflow states for invoices (Draft, Sent, Partially Paid, Paid, Overdue, Cancelled, Void).
 System-defined reference data — not tenant-specific.
 ====================================================================================================
*/
CREATE TABLE `InvoiceStatus`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`color` VARCHAR(10) NULL,		-- Hex color for UI display.
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the InvoiceStatus table's name field.
CREATE INDEX `I_InvoiceStatus_name` ON `InvoiceStatus` (`name`);

-- Index on the InvoiceStatus table's active field.
CREATE INDEX `I_InvoiceStatus_active` ON `InvoiceStatus` (`active`);

-- Index on the InvoiceStatus table's deleted field.
CREATE INDEX `I_InvoiceStatus_deleted` ON `InvoiceStatus` (`deleted`);

INSERT INTO `InvoiceStatus` ( `name`, `description`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'Draft', 'Invoice created but not yet sent to client', '#9E9E9E', 1, 'b1c2d3e4-0001-4000-9000-000000000001' );

INSERT INTO `InvoiceStatus` ( `name`, `description`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'Sent', 'Invoice has been sent to the client', '#2196F3', 2, 'b1c2d3e4-0001-4000-9000-000000000002' );

INSERT INTO `InvoiceStatus` ( `name`, `description`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'Partially Paid', 'Client has made a partial payment', '#FF9800', 3, 'b1c2d3e4-0001-4000-9000-000000000003' );

INSERT INTO `InvoiceStatus` ( `name`, `description`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'Paid', 'Invoice has been fully paid', '#4CAF50', 4, 'b1c2d3e4-0001-4000-9000-000000000004' );

INSERT INTO `InvoiceStatus` ( `name`, `description`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'Overdue', 'Invoice is past due date and unpaid', '#F44336', 5, 'b1c2d3e4-0001-4000-9000-000000000005' );

INSERT INTO `InvoiceStatus` ( `name`, `description`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'Cancelled', 'Invoice has been cancelled', '#795548', 6, 'b1c2d3e4-0001-4000-9000-000000000006' );

INSERT INTO `InvoiceStatus` ( `name`, `description`, `color`, `sequence`, `objectGuid` ) VALUES  ( 'Void', 'Invoice has been voided and should be disregarded', '#607D8B', 7, 'b1c2d3e4-0001-4000-9000-000000000007' );


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
CREATE TABLE `Invoice`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`invoiceNumber` VARCHAR(50) NOT NULL,		-- Auto-generated sequential invoice number (e.g., 'INV-2025-0001').
	`clientId` INT NOT NULL,		-- The client being invoiced.
	`contactId` INT NULL,		-- Optional billing contact person.
	`scheduledEventId` INT NULL,		-- Optional link to the event this invoice relates to.
	`financialOfficeId` INT NULL,		-- Optional issuing financial office.
	`invoiceStatusId` INT NOT NULL,		-- Current invoice status (Draft, Sent, Paid, etc.).
	`currencyId` INT NOT NULL,		-- Currency for all amounts on this invoice.
	`taxCodeId` INT NULL,		-- Default tax code applied to line items.
	`invoiceDate` DATETIME NOT NULL,		-- Date the invoice was issued (UTC).
	`dueDate` DATETIME NOT NULL,		-- Payment due date (UTC).
	`subtotal` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Sum of line item amounts before tax.
	`taxAmount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Total tax amount.
	`totalAmount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Grand total (subtotal + taxAmount).
	`amountPaid` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Running total of payments received against this invoice.
	`sentDate` DATETIME NULL,		-- When the invoice was sent to the client (null = not sent).
	`paidDate` DATETIME NULL,		-- When the invoice was fully paid (null = not yet paid).
	`notes` TEXT NULL,		-- Optional notes or payment terms.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`clientId`) REFERENCES `Client`(`id`),		-- Foreign key to the Client table.
	FOREIGN KEY (`contactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`scheduledEventId`) REFERENCES `ScheduledEvent`(`id`),		-- Foreign key to the ScheduledEvent table.
	FOREIGN KEY (`financialOfficeId`) REFERENCES `FinancialOffice`(`id`),		-- Foreign key to the FinancialOffice table.
	FOREIGN KEY (`invoiceStatusId`) REFERENCES `InvoiceStatus`(`id`),		-- Foreign key to the InvoiceStatus table.
	FOREIGN KEY (`currencyId`) REFERENCES `Currency`(`id`),		-- Foreign key to the Currency table.
	FOREIGN KEY (`taxCodeId`) REFERENCES `TaxCode`(`id`),		-- Foreign key to the TaxCode table.
	UNIQUE `UC_Invoice_tenantGuid_invoiceNumber_Unique`( `tenantGuid`, `invoiceNumber` ) 		-- Uniqueness enforced on the Invoice table's tenantGuid and invoiceNumber fields.
);
-- Index on the Invoice table's tenantGuid field.
CREATE INDEX `I_Invoice_tenantGuid` ON `Invoice` (`tenantGuid`);

-- Index on the Invoice table's tenantGuid,invoiceNumber fields.
CREATE INDEX `I_Invoice_tenantGuid_invoiceNumber` ON `Invoice` (`tenantGuid`, `invoiceNumber`);

-- Index on the Invoice table's tenantGuid,clientId fields.
CREATE INDEX `I_Invoice_tenantGuid_clientId` ON `Invoice` (`tenantGuid`, `clientId`);

-- Index on the Invoice table's tenantGuid,contactId fields.
CREATE INDEX `I_Invoice_tenantGuid_contactId` ON `Invoice` (`tenantGuid`, `contactId`);

-- Index on the Invoice table's tenantGuid,scheduledEventId fields.
CREATE INDEX `I_Invoice_tenantGuid_scheduledEventId` ON `Invoice` (`tenantGuid`, `scheduledEventId`);

-- Index on the Invoice table's tenantGuid,financialOfficeId fields.
CREATE INDEX `I_Invoice_tenantGuid_financialOfficeId` ON `Invoice` (`tenantGuid`, `financialOfficeId`);

-- Index on the Invoice table's tenantGuid,invoiceStatusId fields.
CREATE INDEX `I_Invoice_tenantGuid_invoiceStatusId` ON `Invoice` (`tenantGuid`, `invoiceStatusId`);

-- Index on the Invoice table's tenantGuid,currencyId fields.
CREATE INDEX `I_Invoice_tenantGuid_currencyId` ON `Invoice` (`tenantGuid`, `currencyId`);

-- Index on the Invoice table's tenantGuid,taxCodeId fields.
CREATE INDEX `I_Invoice_tenantGuid_taxCodeId` ON `Invoice` (`tenantGuid`, `taxCodeId`);

-- Index on the Invoice table's tenantGuid,invoiceDate fields.
CREATE INDEX `I_Invoice_tenantGuid_invoiceDate` ON `Invoice` (`tenantGuid`, `invoiceDate`);

-- Index on the Invoice table's tenantGuid,active fields.
CREATE INDEX `I_Invoice_tenantGuid_active` ON `Invoice` (`tenantGuid`, `active`);

-- Index on the Invoice table's tenantGuid,deleted fields.
CREATE INDEX `I_Invoice_tenantGuid_deleted` ON `Invoice` (`tenantGuid`, `deleted`);


-- The change history for records from the Invoice table.
CREATE TABLE `InvoiceChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`invoiceId` INT NOT NULL,		-- Link to the Invoice table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`invoiceId`) REFERENCES `Invoice`(`id`)		-- Foreign key to the Invoice table.
);
-- Index on the InvoiceChangeHistory table's tenantGuid field.
CREATE INDEX `I_InvoiceChangeHistory_tenantGuid` ON `InvoiceChangeHistory` (`tenantGuid`);

-- Index on the InvoiceChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_InvoiceChangeHistory_tenantGuid_versionNumber` ON `InvoiceChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the InvoiceChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_InvoiceChangeHistory_tenantGuid_timeStamp` ON `InvoiceChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the InvoiceChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_InvoiceChangeHistory_tenantGuid_userId` ON `InvoiceChangeHistory` (`tenantGuid`, `userId`);

-- Index on the InvoiceChangeHistory table's tenantGuid,invoiceId fields.
CREATE INDEX `I_InvoiceChangeHistory_tenantGuid_invoiceId` ON `InvoiceChangeHistory` (`tenantGuid`, `invoiceId`, `versionNumber`, `timeStamp`, `userId`);


/*
====================================================================================================
 INVOICE LINE ITEM
 Individual billable items on an invoice. Optionally links back to the source EventCharge
 and/or FinancialCategory for categorization and audit trail.
 ====================================================================================================
*/
CREATE TABLE `InvoiceLineItem`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`invoiceId` INT NOT NULL,		-- Parent invoice.
	`eventChargeId` INT NULL,		-- Optional link to the source EventCharge this line item was created from.
	`financialCategoryId` INT NULL,		-- Optional revenue category for reporting.
	`description` VARCHAR(500) NOT NULL,		-- Line item description (e.g., 'Hall Rental - Saturday Dec 14').
	`quantity` NUMERIC(38,22) NOT NULL DEFAULT 1,		-- Quantity (hours, units, etc.).
	`unitPrice` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Price per unit.
	`amount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Extended amount (quantity × unitPrice).
	`taxAmount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Tax for this line item.
	`totalAmount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Line total (amount + taxAmount).
	`sequence` INT NULL,		-- Display order on the invoice.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`invoiceId`) REFERENCES `Invoice`(`id`),		-- Foreign key to the Invoice table.
	FOREIGN KEY (`eventChargeId`) REFERENCES `EventCharge`(`id`),		-- Foreign key to the EventCharge table.
	FOREIGN KEY (`financialCategoryId`) REFERENCES `FinancialCategory`(`id`)		-- Foreign key to the FinancialCategory table.
);
-- Index on the InvoiceLineItem table's tenantGuid field.
CREATE INDEX `I_InvoiceLineItem_tenantGuid` ON `InvoiceLineItem` (`tenantGuid`);

-- Index on the InvoiceLineItem table's tenantGuid,invoiceId fields.
CREATE INDEX `I_InvoiceLineItem_tenantGuid_invoiceId` ON `InvoiceLineItem` (`tenantGuid`, `invoiceId`);

-- Index on the InvoiceLineItem table's tenantGuid,eventChargeId fields.
CREATE INDEX `I_InvoiceLineItem_tenantGuid_eventChargeId` ON `InvoiceLineItem` (`tenantGuid`, `eventChargeId`);

-- Index on the InvoiceLineItem table's tenantGuid,financialCategoryId fields.
CREATE INDEX `I_InvoiceLineItem_tenantGuid_financialCategoryId` ON `InvoiceLineItem` (`tenantGuid`, `financialCategoryId`);

-- Index on the InvoiceLineItem table's tenantGuid,active fields.
CREATE INDEX `I_InvoiceLineItem_tenantGuid_active` ON `InvoiceLineItem` (`tenantGuid`, `active`);

-- Index on the InvoiceLineItem table's tenantGuid,deleted fields.
CREATE INDEX `I_InvoiceLineItem_tenantGuid_deleted` ON `InvoiceLineItem` (`tenantGuid`, `deleted`);


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
CREATE TABLE `Receipt`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`receiptNumber` VARCHAR(50) NOT NULL,		-- Auto-generated sequential receipt number (e.g., 'REC-2025-0001').
	`receiptTypeId` INT NOT NULL,		-- Type of receipt (Official, Summary, etc.).
	`invoiceId` INT NULL,		-- Optional link to the invoice this receipt is for.
	`paymentTransactionId` INT NULL,		-- Optional link to the payment transaction.
	`financialTransactionId` INT NULL,		-- Optional link to the financial transaction.
	`clientId` INT NULL,		-- Optional payer client.
	`contactId` INT NULL,		-- Optional payer contact.
	`currencyId` INT NOT NULL,		-- Currency for the amount.
	`receiptDate` DATETIME NOT NULL,		-- Date the receipt was issued (UTC).
	`amount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Amount received.
	`paymentMethod` VARCHAR(100) NULL,		-- How the payment was made (e.g., 'E-Transfer', 'Cash').
	`description` VARCHAR(500) NULL,		-- Description of what the payment was for.
	`notes` TEXT NULL,		-- Optional additional notes.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`receiptTypeId`) REFERENCES `ReceiptType`(`id`),		-- Foreign key to the ReceiptType table.
	FOREIGN KEY (`invoiceId`) REFERENCES `Invoice`(`id`),		-- Foreign key to the Invoice table.
	FOREIGN KEY (`paymentTransactionId`) REFERENCES `PaymentTransaction`(`id`),		-- Foreign key to the PaymentTransaction table.
	FOREIGN KEY (`financialTransactionId`) REFERENCES `FinancialTransaction`(`id`),		-- Foreign key to the FinancialTransaction table.
	FOREIGN KEY (`clientId`) REFERENCES `Client`(`id`),		-- Foreign key to the Client table.
	FOREIGN KEY (`contactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`currencyId`) REFERENCES `Currency`(`id`),		-- Foreign key to the Currency table.
	UNIQUE `UC_Receipt_tenantGuid_receiptNumber_Unique`( `tenantGuid`, `receiptNumber` ) 		-- Uniqueness enforced on the Receipt table's tenantGuid and receiptNumber fields.
);
-- Index on the Receipt table's tenantGuid field.
CREATE INDEX `I_Receipt_tenantGuid` ON `Receipt` (`tenantGuid`);

-- Index on the Receipt table's tenantGuid,receiptNumber fields.
CREATE INDEX `I_Receipt_tenantGuid_receiptNumber` ON `Receipt` (`tenantGuid`, `receiptNumber`);

-- Index on the Receipt table's tenantGuid,receiptTypeId fields.
CREATE INDEX `I_Receipt_tenantGuid_receiptTypeId` ON `Receipt` (`tenantGuid`, `receiptTypeId`);

-- Index on the Receipt table's tenantGuid,invoiceId fields.
CREATE INDEX `I_Receipt_tenantGuid_invoiceId` ON `Receipt` (`tenantGuid`, `invoiceId`);

-- Index on the Receipt table's tenantGuid,paymentTransactionId fields.
CREATE INDEX `I_Receipt_tenantGuid_paymentTransactionId` ON `Receipt` (`tenantGuid`, `paymentTransactionId`);

-- Index on the Receipt table's tenantGuid,financialTransactionId fields.
CREATE INDEX `I_Receipt_tenantGuid_financialTransactionId` ON `Receipt` (`tenantGuid`, `financialTransactionId`);

-- Index on the Receipt table's tenantGuid,clientId fields.
CREATE INDEX `I_Receipt_tenantGuid_clientId` ON `Receipt` (`tenantGuid`, `clientId`);

-- Index on the Receipt table's tenantGuid,contactId fields.
CREATE INDEX `I_Receipt_tenantGuid_contactId` ON `Receipt` (`tenantGuid`, `contactId`);

-- Index on the Receipt table's tenantGuid,currencyId fields.
CREATE INDEX `I_Receipt_tenantGuid_currencyId` ON `Receipt` (`tenantGuid`, `currencyId`);

-- Index on the Receipt table's tenantGuid,receiptDate fields.
CREATE INDEX `I_Receipt_tenantGuid_receiptDate` ON `Receipt` (`tenantGuid`, `receiptDate`);

-- Index on the Receipt table's tenantGuid,active fields.
CREATE INDEX `I_Receipt_tenantGuid_active` ON `Receipt` (`tenantGuid`, `active`);

-- Index on the Receipt table's tenantGuid,deleted fields.
CREATE INDEX `I_Receipt_tenantGuid_deleted` ON `Receipt` (`tenantGuid`, `deleted`);


-- The change history for records from the Receipt table.
CREATE TABLE `ReceiptChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`receiptId` INT NOT NULL,		-- Link to the Receipt table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`receiptId`) REFERENCES `Receipt`(`id`)		-- Foreign key to the Receipt table.
);
-- Index on the ReceiptChangeHistory table's tenantGuid field.
CREATE INDEX `I_ReceiptChangeHistory_tenantGuid` ON `ReceiptChangeHistory` (`tenantGuid`);

-- Index on the ReceiptChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ReceiptChangeHistory_tenantGuid_versionNumber` ON `ReceiptChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ReceiptChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ReceiptChangeHistory_tenantGuid_timeStamp` ON `ReceiptChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ReceiptChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ReceiptChangeHistory_tenantGuid_userId` ON `ReceiptChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ReceiptChangeHistory table's tenantGuid,receiptId fields.
CREATE INDEX `I_ReceiptChangeHistory_tenantGuid_receiptId` ON `ReceiptChangeHistory` (`tenantGuid`, `receiptId`, `versionNumber`, `timeStamp`, `userId`);


-- The contact interaction data
CREATE TABLE `ContactInteraction`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`contactId` INT NOT NULL,		-- The contact that is the target of the interaction.
	`initiatingContactId` INT NULL,		-- Optional contact that initiated the interaction.  This would be staff of the company using the scheduler
	`interactionTypeId` INT NOT NULL,		-- Link to the InteractionType table.
	`scheduledEventId` INT NULL,		-- The optional event that the interaction is regarding.
	`startTime` DATETIME NOT NULL,
	`endTime` DATETIME NULL,
	`notes` TEXT NULL,		-- Optional notes about the interaction
	`location` TEXT NULL,		-- Optional location details about the interaction
	`priorityId` INT NULL,		-- Optional priority for the interaction.
	`externalId` VARCHAR(100) NULL,
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`contactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`initiatingContactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`interactionTypeId`) REFERENCES `InteractionType`(`id`),		-- Foreign key to the InteractionType table.
	FOREIGN KEY (`scheduledEventId`) REFERENCES `ScheduledEvent`(`id`),		-- Foreign key to the ScheduledEvent table.
	FOREIGN KEY (`priorityId`) REFERENCES `Priority`(`id`)		-- Foreign key to the Priority table.
);
-- Index on the ContactInteraction table's tenantGuid field.
CREATE INDEX `I_ContactInteraction_tenantGuid` ON `ContactInteraction` (`tenantGuid`);

-- Index on the ContactInteraction table's tenantGuid,contactId fields.
CREATE INDEX `I_ContactInteraction_tenantGuid_contactId` ON `ContactInteraction` (`tenantGuid`, `contactId`);

-- Index on the ContactInteraction table's tenantGuid,initiatingContactId fields.
CREATE INDEX `I_ContactInteraction_tenantGuid_initiatingContactId` ON `ContactInteraction` (`tenantGuid`, `initiatingContactId`);

-- Index on the ContactInteraction table's tenantGuid,interactionTypeId fields.
CREATE INDEX `I_ContactInteraction_tenantGuid_interactionTypeId` ON `ContactInteraction` (`tenantGuid`, `interactionTypeId`);

-- Index on the ContactInteraction table's tenantGuid,scheduledEventId fields.
CREATE INDEX `I_ContactInteraction_tenantGuid_scheduledEventId` ON `ContactInteraction` (`tenantGuid`, `scheduledEventId`);

-- Index on the ContactInteraction table's tenantGuid,priorityId fields.
CREATE INDEX `I_ContactInteraction_tenantGuid_priorityId` ON `ContactInteraction` (`tenantGuid`, `priorityId`);

-- Index on the ContactInteraction table's tenantGuid,externalId fields.
CREATE INDEX `I_ContactInteraction_tenantGuid_externalId` ON `ContactInteraction` (`tenantGuid`, `externalId`);

-- Index on the ContactInteraction table's tenantGuid,active fields.
CREATE INDEX `I_ContactInteraction_tenantGuid_active` ON `ContactInteraction` (`tenantGuid`, `active`);

-- Index on the ContactInteraction table's tenantGuid,deleted fields.
CREATE INDEX `I_ContactInteraction_tenantGuid_deleted` ON `ContactInteraction` (`tenantGuid`, `deleted`);

-- Index on the ContactInteraction table's tenantGuid,contactId,startTime fields.
CREATE INDEX `I_ContactInteraction_tenantGuid_contactId_startTime` ON `ContactInteraction` (`tenantGuid`, `contactId`, `startTime`);


-- The change history for records from the ContactInteraction table.
CREATE TABLE `ContactInteractionChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`contactInteractionId` INT NOT NULL,		-- Link to the ContactInteraction table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`contactInteractionId`) REFERENCES `ContactInteraction`(`id`)		-- Foreign key to the ContactInteraction table.
);
-- Index on the ContactInteractionChangeHistory table's tenantGuid field.
CREATE INDEX `I_ContactInteractionChangeHistory_tenantGuid` ON `ContactInteractionChangeHistory` (`tenantGuid`);

-- Index on the ContactInteractionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ContactInteractionChangeHistory_tenantGuid_versionNumber` ON `ContactInteractionChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ContactInteractionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ContactInteractionChangeHistory_tenantGuid_timeStamp` ON `ContactInteractionChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ContactInteractionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ContactInteractionChangeHistory_tenantGuid_userId` ON `ContactInteractionChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ContactInteractionChangeHistory table's tenantGuid,contactInteractionId fields.
CREATE INDEX `I_ContactInteractionChangeHistory_tenantGuid_contactInteractionI` ON `ContactInteractionChangeHistory` (`tenantGuid`, `contactInteractionId`, `versionNumber`, `timeStamp`, `userId`);


-- Many-to-many relationship between events and calendars.
CREATE TABLE `EventCalendar`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`scheduledEventId` INT NOT NULL,		-- Link to the ScheduledEvent table.
	`calendarId` INT NOT NULL,		-- Link to the Calendar table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`scheduledEventId`) REFERENCES `ScheduledEvent`(`id`),		-- Foreign key to the ScheduledEvent table.
	FOREIGN KEY (`calendarId`) REFERENCES `Calendar`(`id`),		-- Foreign key to the Calendar table.
	UNIQUE `UC_EventCalendar_tenantGuid_scheduledEventId_calendarId_Unique`( `tenantGuid`, `scheduledEventId`, `calendarId` ) 		-- Uniqueness enforced on the EventCalendar table's tenantGuid and scheduledEventId and calendarId fields.
);
-- Index on the EventCalendar table's tenantGuid field.
CREATE INDEX `I_EventCalendar_tenantGuid` ON `EventCalendar` (`tenantGuid`);

-- Index on the EventCalendar table's tenantGuid,scheduledEventId fields.
CREATE INDEX `I_EventCalendar_tenantGuid_scheduledEventId` ON `EventCalendar` (`tenantGuid`, `scheduledEventId`);

-- Index on the EventCalendar table's tenantGuid,calendarId fields.
CREATE INDEX `I_EventCalendar_tenantGuid_calendarId` ON `EventCalendar` (`tenantGuid`, `calendarId`);

-- Index on the EventCalendar table's tenantGuid,active fields.
CREATE INDEX `I_EventCalendar_tenantGuid_active` ON `EventCalendar` (`tenantGuid`, `active`);

-- Index on the EventCalendar table's tenantGuid,deleted fields.
CREATE INDEX `I_EventCalendar_tenantGuid_deleted` ON `EventCalendar` (`tenantGuid`, `deleted`);


-- Master list of depedency types
CREATE TABLE `DependencyType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the DependencyType table's name field.
CREATE INDEX `I_DependencyType_name` ON `DependencyType` (`name`);

-- Index on the DependencyType table's active field.
CREATE INDEX `I_DependencyType_active` ON `DependencyType` (`active`);

-- Index on the DependencyType table's deleted field.
CREATE INDEX `I_DependencyType_deleted` ON `DependencyType` (`deleted`);

INSERT INTO `DependencyType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'FS', 'Finish to Start', 1, 'f08977bf-af84-4d89-9821-f8a2404028fa' );

INSERT INTO `DependencyType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'SS', 'Start to Start', 2, '51398efa-2489-41ba-a1b6-77d11ce6253b' );

INSERT INTO `DependencyType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'SF', 'Start to Finish', 3, '637dc30a-adc3-47ad-87fa-3c826b7d808f' );

INSERT INTO `DependencyType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'FF', 'Finish to Finish', 4, 'fc7b4932-e79a-4085-9c87-404d29331f85' );


-- Dependencies that a scheduled event has that could affect it.
CREATE TABLE `ScheduledEventDependency`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`predecessorEventId` INT NOT NULL,		-- The task that must happen first
	`successorEventId` INT NOT NULL,		-- The task that waits
	`dependencyTypeId` INT NOT NULL,		-- Link to the DependencyType table.
	`lagMinutes` INT NOT NULL DEFAULT 0,
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`predecessorEventId`) REFERENCES `ScheduledEvent`(`id`),		-- Foreign key to the ScheduledEvent table.
	FOREIGN KEY (`successorEventId`) REFERENCES `ScheduledEvent`(`id`),		-- Foreign key to the ScheduledEvent table.
	FOREIGN KEY (`dependencyTypeId`) REFERENCES `DependencyType`(`id`),		-- Foreign key to the DependencyType table.
	UNIQUE `UC_ScheduledEventDependency_tenantGuid_predecessorEventId_successorEventId_Unique`( `tenantGuid`, `predecessorEventId`, `successorEventId` ) 		-- Uniqueness enforced on the ScheduledEventDependency table's tenantGuid and predecessorEventId and successorEventId fields.
);
-- Index on the ScheduledEventDependency table's tenantGuid field.
CREATE INDEX `I_ScheduledEventDependency_tenantGuid` ON `ScheduledEventDependency` (`tenantGuid`);

-- Index on the ScheduledEventDependency table's tenantGuid,predecessorEventId fields.
CREATE INDEX `I_ScheduledEventDependency_tenantGuid_predecessorEventId` ON `ScheduledEventDependency` (`tenantGuid`, `predecessorEventId`);

-- Index on the ScheduledEventDependency table's tenantGuid,successorEventId fields.
CREATE INDEX `I_ScheduledEventDependency_tenantGuid_successorEventId` ON `ScheduledEventDependency` (`tenantGuid`, `successorEventId`);

-- Index on the ScheduledEventDependency table's tenantGuid,active fields.
CREATE INDEX `I_ScheduledEventDependency_tenantGuid_active` ON `ScheduledEventDependency` (`tenantGuid`, `active`);

-- Index on the ScheduledEventDependency table's tenantGuid,deleted fields.
CREATE INDEX `I_ScheduledEventDependency_tenantGuid_deleted` ON `ScheduledEventDependency` (`tenantGuid`, `deleted`);


-- The change history for records from the ScheduledEventDependency table.
CREATE TABLE `ScheduledEventDependencyChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`scheduledEventDependencyId` INT NOT NULL,		-- Link to the ScheduledEventDependency table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`scheduledEventDependencyId`) REFERENCES `ScheduledEventDependency`(`id`)		-- Foreign key to the ScheduledEventDependency table.
);
-- Index on the ScheduledEventDependencyChangeHistory table's tenantGuid field.
CREATE INDEX `I_ScheduledEventDependencyChangeHistory_tenantGuid` ON `ScheduledEventDependencyChangeHistory` (`tenantGuid`);

-- Index on the ScheduledEventDependencyChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ScheduledEventDependencyChangeHistory_tenantGuid_versionNumber` ON `ScheduledEventDependencyChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ScheduledEventDependencyChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ScheduledEventDependencyChangeHistory_tenantGuid_timeStamp` ON `ScheduledEventDependencyChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ScheduledEventDependencyChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ScheduledEventDependencyChangeHistory_tenantGuid_userId` ON `ScheduledEventDependencyChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ScheduledEventDependencyChangeHistory table's tenantGuid,scheduledEventDependencyId fields.
CREATE INDEX `I_ScheduledEventDependencyChangeHistory_tenantGuid_scheduledEven` ON `ScheduledEventDependencyChangeHistory` (`tenantGuid`, `scheduledEventDependencyId`, `versionNumber`, `timeStamp`, `userId`);


-- Specific qualifications required for a single event instance, overriding or adding to role/site reqs..
CREATE TABLE `ScheduledEventQualificationRequirement`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`scheduledEventId` INT NOT NULL,		-- Link to the ScheduledEvent table.
	`qualificationId` INT NOT NULL,		-- Link to the Qualification table.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`scheduledEventId`) REFERENCES `ScheduledEvent`(`id`),		-- Foreign key to the ScheduledEvent table.
	FOREIGN KEY (`qualificationId`) REFERENCES `Qualification`(`id`)		-- Foreign key to the Qualification table.
);
-- Index on the ScheduledEventQualificationRequirement table's tenantGuid field.
CREATE INDEX `I_ScheduledEventQualificationRequirement_tenantGuid` ON `ScheduledEventQualificationRequirement` (`tenantGuid`);

-- Index on the ScheduledEventQualificationRequirement table's tenantGuid,scheduledEventId fields.
CREATE INDEX `I_ScheduledEventQualificationRequirement_tenantGuid_scheduledEve` ON `ScheduledEventQualificationRequirement` (`tenantGuid`, `scheduledEventId`);

-- Index on the ScheduledEventQualificationRequirement table's tenantGuid,qualificationId fields.
CREATE INDEX `I_ScheduledEventQualificationRequirement_tenantGuid_qualificatio` ON `ScheduledEventQualificationRequirement` (`tenantGuid`, `qualificationId`);

-- Index on the ScheduledEventQualificationRequirement table's tenantGuid,active fields.
CREATE INDEX `I_ScheduledEventQualificationRequirement_tenantGuid_active` ON `ScheduledEventQualificationRequirement` (`tenantGuid`, `active`);

-- Index on the ScheduledEventQualificationRequirement table's tenantGuid,deleted fields.
CREATE INDEX `I_ScheduledEventQualificationRequirement_tenantGuid_deleted` ON `ScheduledEventQualificationRequirement` (`tenantGuid`, `deleted`);


-- The change history for records from the ScheduledEventQualificationRequirement table.
CREATE TABLE `ScheduledEventQualificationRequirementChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`scheduledEventQualificationRequirementId` INT NOT NULL,		-- Link to the ScheduledEventQualificationRequirement table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`scheduledEventQualificationRequirementId`) REFERENCES `ScheduledEventQualificationRequirement`(`id`)		-- Foreign key to the ScheduledEventQualificationRequirement table.
);
-- Index on the ScheduledEventQualificationRequirementChangeHistory table's tenantGuid field.
CREATE INDEX `I_ScheduledEventQualificationRequirementChangeHistory_tenantGuid` ON `ScheduledEventQualificationRequirementChangeHistory` (`tenantGuid`);

-- Index on the ScheduledEventQualificationRequirementChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ScheduledEventQualificationRequirementChangeHistory_tenantGuid` ON `ScheduledEventQualificationRequirementChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ScheduledEventQualificationRequirementChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ScheduledEventQualificationRequirementChangeHistory_tenantGuid` ON `ScheduledEventQualificationRequirementChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ScheduledEventQualificationRequirementChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ScheduledEventQualificationRequirementChangeHistory_tenantGuid` ON `ScheduledEventQualificationRequirementChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ScheduledEventQualificationRequirementChangeHistory table's tenantGuid,scheduledEventQualificationRequirementId fields.
CREATE INDEX `I_ScheduledEventQualificationRequirementChangeHistory_tenantGuid` ON `ScheduledEventQualificationRequirementChangeHistory` (`tenantGuid`, `scheduledEventQualificationRequirementId`, `versionNumber`, `timeStamp`, `userId`);


-- Exceptions to a recurring series.  Used for canceled dates or moved instances (original date + new date).
CREATE TABLE `RecurrenceException`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`scheduledEventId` INT NOT NULL,		-- Link to the ScheduledEvent table.
	`exceptionDateTime` DATETIME NOT NULL,		-- The original occurrence date/time that is excepted
	`movedToDateTime` DATETIME NULL,		-- NULL = canceled, non-NULL = moved to this new date/time
	`reason` VARCHAR(250) NULL,
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`scheduledEventId`) REFERENCES `ScheduledEvent`(`id`),		-- Foreign key to the ScheduledEvent table.
	UNIQUE `UC_RecurrenceException_tenantGuid_scheduledEventId_exceptionDateTime_Unique`( `tenantGuid`, `scheduledEventId`, `exceptionDateTime` ) 		-- Uniqueness enforced on the RecurrenceException table's tenantGuid and scheduledEventId and exceptionDateTime fields.
);
-- Index on the RecurrenceException table's tenantGuid field.
CREATE INDEX `I_RecurrenceException_tenantGuid` ON `RecurrenceException` (`tenantGuid`);

-- Index on the RecurrenceException table's tenantGuid,scheduledEventId fields.
CREATE INDEX `I_RecurrenceException_tenantGuid_scheduledEventId` ON `RecurrenceException` (`tenantGuid`, `scheduledEventId`);

-- Index on the RecurrenceException table's tenantGuid,active fields.
CREATE INDEX `I_RecurrenceException_tenantGuid_active` ON `RecurrenceException` (`tenantGuid`, `active`);

-- Index on the RecurrenceException table's tenantGuid,deleted fields.
CREATE INDEX `I_RecurrenceException_tenantGuid_deleted` ON `RecurrenceException` (`tenantGuid`, `deleted`);


-- The change history for records from the RecurrenceException table.
CREATE TABLE `RecurrenceExceptionChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`recurrenceExceptionId` INT NOT NULL,		-- Link to the RecurrenceException table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`recurrenceExceptionId`) REFERENCES `RecurrenceException`(`id`)		-- Foreign key to the RecurrenceException table.
);
-- Index on the RecurrenceExceptionChangeHistory table's tenantGuid field.
CREATE INDEX `I_RecurrenceExceptionChangeHistory_tenantGuid` ON `RecurrenceExceptionChangeHistory` (`tenantGuid`);

-- Index on the RecurrenceExceptionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_RecurrenceExceptionChangeHistory_tenantGuid_versionNumber` ON `RecurrenceExceptionChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the RecurrenceExceptionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_RecurrenceExceptionChangeHistory_tenantGuid_timeStamp` ON `RecurrenceExceptionChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the RecurrenceExceptionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_RecurrenceExceptionChangeHistory_tenantGuid_userId` ON `RecurrenceExceptionChangeHistory` (`tenantGuid`, `userId`);

-- Index on the RecurrenceExceptionChangeHistory table's tenantGuid,recurrenceExceptionId fields.
CREATE INDEX `I_RecurrenceExceptionChangeHistory_tenantGuid_recurrenceExceptio` ON `RecurrenceExceptionChangeHistory` (`tenantGuid`, `recurrenceExceptionId`, `versionNumber`, `timeStamp`, `userId`);


-- Master list of event notification types
CREATE TABLE `EventNotificationType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the EventNotificationType table's name field.
CREATE INDEX `I_EventNotificationType_name` ON `EventNotificationType` (`name`);

-- Index on the EventNotificationType table's active field.
CREATE INDEX `I_EventNotificationType_active` ON `EventNotificationType` (`active`);

-- Index on the EventNotificationType table's deleted field.
CREATE INDEX `I_EventNotificationType_deleted` ON `EventNotificationType` (`deleted`);

INSERT INTO `EventNotificationType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Email', 'Send to email address', 1, '73ff7b17-3fd7-40ce-91bf-c91daca7b4ce' );

INSERT INTO `EventNotificationType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'SMS', 'Sent to cell phone via SMS message', 2, '89391299-4427-43f6-bcf2-0266e47e83a7' );

INSERT INTO `EventNotificationType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Push', 'Sent to cell phone via Push notification', 3, '0395ddde-58dc-4577-9dae-07614680c386' );


-- Links resources (or entire crews) to events.  Supports partial assignments and role designation.  - If crewId is non-NULL → this row represents assignment of the whole crew - If resourceId is non-NULL and crewId is NULL → individual resource assignment - assignmentStart/End NULL → uses full event duration
CREATE TABLE `EventNotificationSubscription`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`resourceId` INT NULL,		-- Optional resource for this notification subscription.  Needs either this or contact to be valid.
	`contactId` INT NULL,		-- Optional contact for this notification subscription.  Needs either this or resource to be valid.
	`eventNotificationTypeId` INT NOT NULL,		-- Link to the EventNotificationType table.
	`triggerEvents` INT NOT NULL DEFAULT 1,		-- Bitmask: 1=Assigned, 2=Canceled, 4=Modified, 8=Reminder
	`recipientAddress` VARCHAR(250) NOT NULL,		-- Email address or Phone #
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`),		-- Foreign key to the Resource table.
	FOREIGN KEY (`contactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`eventNotificationTypeId`) REFERENCES `EventNotificationType`(`id`)		-- Foreign key to the EventNotificationType table.
);
-- Index on the EventNotificationSubscription table's tenantGuid field.
CREATE INDEX `I_EventNotificationSubscription_tenantGuid` ON `EventNotificationSubscription` (`tenantGuid`);

-- Index on the EventNotificationSubscription table's tenantGuid,resourceId fields.
CREATE INDEX `I_EventNotificationSubscription_tenantGuid_resourceId` ON `EventNotificationSubscription` (`tenantGuid`, `resourceId`);

-- Index on the EventNotificationSubscription table's tenantGuid,contactId fields.
CREATE INDEX `I_EventNotificationSubscription_tenantGuid_contactId` ON `EventNotificationSubscription` (`tenantGuid`, `contactId`);

-- Index on the EventNotificationSubscription table's tenantGuid,eventNotificationTypeId fields.
CREATE INDEX `I_EventNotificationSubscription_tenantGuid_eventNotificationType` ON `EventNotificationSubscription` (`tenantGuid`, `eventNotificationTypeId`);

-- Index on the EventNotificationSubscription table's tenantGuid,active fields.
CREATE INDEX `I_EventNotificationSubscription_tenantGuid_active` ON `EventNotificationSubscription` (`tenantGuid`, `active`);

-- Index on the EventNotificationSubscription table's tenantGuid,deleted fields.
CREATE INDEX `I_EventNotificationSubscription_tenantGuid_deleted` ON `EventNotificationSubscription` (`tenantGuid`, `deleted`);


-- The change history for records from the EventNotificationSubscription table.
CREATE TABLE `EventNotificationSubscriptionChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`eventNotificationSubscriptionId` INT NOT NULL,		-- Link to the EventNotificationSubscription table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`eventNotificationSubscriptionId`) REFERENCES `EventNotificationSubscription`(`id`)		-- Foreign key to the EventNotificationSubscription table.
);
-- Index on the EventNotificationSubscriptionChangeHistory table's tenantGuid field.
CREATE INDEX `I_EventNotificationSubscriptionChangeHistory_tenantGuid` ON `EventNotificationSubscriptionChangeHistory` (`tenantGuid`);

-- Index on the EventNotificationSubscriptionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_EventNotificationSubscriptionChangeHistory_tenantGuid_versionN` ON `EventNotificationSubscriptionChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the EventNotificationSubscriptionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_EventNotificationSubscriptionChangeHistory_tenantGuid_timeStam` ON `EventNotificationSubscriptionChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the EventNotificationSubscriptionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_EventNotificationSubscriptionChangeHistory_tenantGuid_userId` ON `EventNotificationSubscriptionChangeHistory` (`tenantGuid`, `userId`);

-- Index on the EventNotificationSubscriptionChangeHistory table's tenantGuid,eventNotificationSubscriptionId fields.
CREATE INDEX `I_EventNotificationSubscriptionChangeHistory_tenantGuid_eventNot` ON `EventNotificationSubscriptionChangeHistory` (`tenantGuid`, `eventNotificationSubscriptionId`, `versionNumber`, `timeStamp`, `userId`);


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
CREATE TABLE `Fund`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`glCode` VARCHAR(100) NULL,		-- The accounting code
	`isRestricted` BIT NOT NULL DEFAULT 0,		-- Legal restriction on funds
	`goalAmount` DECIMAL(11,2) NULL,
	`notes` TEXT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_Fund_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the Fund table's tenantGuid and name fields.
);
-- Index on the Fund table's tenantGuid field.
CREATE INDEX `I_Fund_tenantGuid` ON `Fund` (`tenantGuid`);

-- Index on the Fund table's tenantGuid,name fields.
CREATE INDEX `I_Fund_tenantGuid_name` ON `Fund` (`tenantGuid`, `name`);

-- Index on the Fund table's tenantGuid,iconId fields.
CREATE INDEX `I_Fund_tenantGuid_iconId` ON `Fund` (`tenantGuid`, `iconId`);

-- Index on the Fund table's tenantGuid,active fields.
CREATE INDEX `I_Fund_tenantGuid_active` ON `Fund` (`tenantGuid`, `active`);

-- Index on the Fund table's tenantGuid,deleted fields.
CREATE INDEX `I_Fund_tenantGuid_deleted` ON `Fund` (`tenantGuid`, `deleted`);


-- The change history for records from the Fund table.
CREATE TABLE `FundChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`fundId` INT NOT NULL,		-- Link to the Fund table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`fundId`) REFERENCES `Fund`(`id`)		-- Foreign key to the Fund table.
);
-- Index on the FundChangeHistory table's tenantGuid field.
CREATE INDEX `I_FundChangeHistory_tenantGuid` ON `FundChangeHistory` (`tenantGuid`);

-- Index on the FundChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_FundChangeHistory_tenantGuid_versionNumber` ON `FundChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the FundChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_FundChangeHistory_tenantGuid_timeStamp` ON `FundChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the FundChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_FundChangeHistory_tenantGuid_userId` ON `FundChangeHistory` (`tenantGuid`, `userId`);

-- Index on the FundChangeHistory table's tenantGuid,fundId fields.
CREATE INDEX `I_FundChangeHistory_tenantGuid_fundId` ON `FundChangeHistory` (`tenantGuid`, `fundId`, `versionNumber`, `timeStamp`, `userId`);


--  2. CAMPAIGNS (Broad Initiatives)
CREATE TABLE `Campaign`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`startDate` DATE NULL,
	`endDate` DATE NULL,
	`fundRaisingGoal` DECIMAL(11,2) NULL,
	`notes` TEXT NULL,
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_Campaign_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the Campaign table's tenantGuid and name fields.
);
-- Index on the Campaign table's tenantGuid field.
CREATE INDEX `I_Campaign_tenantGuid` ON `Campaign` (`tenantGuid`);

-- Index on the Campaign table's tenantGuid,name fields.
CREATE INDEX `I_Campaign_tenantGuid_name` ON `Campaign` (`tenantGuid`, `name`);

-- Index on the Campaign table's tenantGuid,iconId fields.
CREATE INDEX `I_Campaign_tenantGuid_iconId` ON `Campaign` (`tenantGuid`, `iconId`);

-- Index on the Campaign table's tenantGuid,active fields.
CREATE INDEX `I_Campaign_tenantGuid_active` ON `Campaign` (`tenantGuid`, `active`);

-- Index on the Campaign table's tenantGuid,deleted fields.
CREATE INDEX `I_Campaign_tenantGuid_deleted` ON `Campaign` (`tenantGuid`, `deleted`);


-- The change history for records from the Campaign table.
CREATE TABLE `CampaignChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`campaignId` INT NOT NULL,		-- Link to the Campaign table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`campaignId`) REFERENCES `Campaign`(`id`)		-- Foreign key to the Campaign table.
);
-- Index on the CampaignChangeHistory table's tenantGuid field.
CREATE INDEX `I_CampaignChangeHistory_tenantGuid` ON `CampaignChangeHistory` (`tenantGuid`);

-- Index on the CampaignChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_CampaignChangeHistory_tenantGuid_versionNumber` ON `CampaignChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the CampaignChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_CampaignChangeHistory_tenantGuid_timeStamp` ON `CampaignChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the CampaignChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_CampaignChangeHistory_tenantGuid_userId` ON `CampaignChangeHistory` (`tenantGuid`, `userId`);

-- Index on the CampaignChangeHistory table's tenantGuid,campaignId fields.
CREATE INDEX `I_CampaignChangeHistory_tenantGuid_campaignId` ON `CampaignChangeHistory` (`tenantGuid`, `campaignId`, `versionNumber`, `timeStamp`, `userId`);


--  3. APPEALS (Specific Solicitations)
CREATE TABLE `Appeal`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`campaignId` INT NULL,		-- Optional link to parent campaign
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`costPerUnit` DECIMAL(11,2) NULL,		-- For ROI calculation (Cost vs. Raised)
	`notes` TEXT NULL,
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`campaignId`) REFERENCES `Campaign`(`id`),		-- Foreign key to the Campaign table.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_Appeal_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the Appeal table's tenantGuid and name fields.
);
-- Index on the Appeal table's tenantGuid field.
CREATE INDEX `I_Appeal_tenantGuid` ON `Appeal` (`tenantGuid`);

-- Index on the Appeal table's tenantGuid,campaignId fields.
CREATE INDEX `I_Appeal_tenantGuid_campaignId` ON `Appeal` (`tenantGuid`, `campaignId`);

-- Index on the Appeal table's tenantGuid,name fields.
CREATE INDEX `I_Appeal_tenantGuid_name` ON `Appeal` (`tenantGuid`, `name`);

-- Index on the Appeal table's tenantGuid,iconId fields.
CREATE INDEX `I_Appeal_tenantGuid_iconId` ON `Appeal` (`tenantGuid`, `iconId`);

-- Index on the Appeal table's tenantGuid,active fields.
CREATE INDEX `I_Appeal_tenantGuid_active` ON `Appeal` (`tenantGuid`, `active`);

-- Index on the Appeal table's tenantGuid,deleted fields.
CREATE INDEX `I_Appeal_tenantGuid_deleted` ON `Appeal` (`tenantGuid`, `deleted`);


-- The change history for records from the Appeal table.
CREATE TABLE `AppealChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`appealId` INT NOT NULL,		-- Link to the Appeal table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`appealId`) REFERENCES `Appeal`(`id`)		-- Foreign key to the Appeal table.
);
-- Index on the AppealChangeHistory table's tenantGuid field.
CREATE INDEX `I_AppealChangeHistory_tenantGuid` ON `AppealChangeHistory` (`tenantGuid`);

-- Index on the AppealChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_AppealChangeHistory_tenantGuid_versionNumber` ON `AppealChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the AppealChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_AppealChangeHistory_tenantGuid_timeStamp` ON `AppealChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the AppealChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_AppealChangeHistory_tenantGuid_userId` ON `AppealChangeHistory` (`tenantGuid`, `userId`);

-- Index on the AppealChangeHistory table's tenantGuid,appealId fields.
CREATE INDEX `I_AppealChangeHistory_tenantGuid_appealId` ON `AppealChangeHistory` (`tenantGuid`, `appealId`, `versionNumber`, `timeStamp`, `userId`);


/*
====================================================================================================
   HOUSEHOLD MANAGEMENT
   Standardizes how multiple constituents are grouped for mailing, receipting, and recognition.
   This allows for "The Smith Family" recognition even if John and Jane have separate records.
   ====================================================================================================
*/
CREATE TABLE `Household`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`schedulingTargetId` INT NULL,		-- Link to the SchedulingTarget table.
	`formalSalutation` VARCHAR(250) NULL,		-- ex. "Mr. and Mrs. John Smith"
	`informalSalutation` VARCHAR(250) NULL,		-- ex. "John and Jane"
	`addressee` VARCHAR(250) NULL,		-- The label for the envelope
	`totalHouseholdGiving` DECIMAL(11,2) NOT NULL DEFAULT 0,
	`lastHouseholdGiftDate` DATE NULL,
	`notes` TEXT NULL,
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`avatarFileName` VARCHAR(250) NULL,		-- Part of the binary data field setup
	`avatarSize` BIGINT NULL,		-- Part of the binary data field setup
	`avatarData` BLOB NULL,		-- Part of the binary data field setup
	`avatarMimeType` VARCHAR(100) NULL,		-- Part of the binary data field setup
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`schedulingTargetId`) REFERENCES `SchedulingTarget`(`id`),		-- Foreign key to the SchedulingTarget table.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_Household_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the Household table's tenantGuid and name fields.
);
-- Index on the Household table's tenantGuid field.
CREATE INDEX `I_Household_tenantGuid` ON `Household` (`tenantGuid`);

-- Index on the Household table's tenantGuid,name fields.
CREATE INDEX `I_Household_tenantGuid_name` ON `Household` (`tenantGuid`, `name`);

-- Index on the Household table's tenantGuid,schedulingTargetId fields.
CREATE INDEX `I_Household_tenantGuid_schedulingTargetId` ON `Household` (`tenantGuid`, `schedulingTargetId`);

-- Index on the Household table's tenantGuid,iconId fields.
CREATE INDEX `I_Household_tenantGuid_iconId` ON `Household` (`tenantGuid`, `iconId`);

-- Index on the Household table's tenantGuid,active fields.
CREATE INDEX `I_Household_tenantGuid_active` ON `Household` (`tenantGuid`, `active`);

-- Index on the Household table's tenantGuid,deleted fields.
CREATE INDEX `I_Household_tenantGuid_deleted` ON `Household` (`tenantGuid`, `deleted`);


-- The change history for records from the Household table.
CREATE TABLE `HouseholdChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`householdId` INT NOT NULL,		-- Link to the Household table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`householdId`) REFERENCES `Household`(`id`)		-- Foreign key to the Household table.
);
-- Index on the HouseholdChangeHistory table's tenantGuid field.
CREATE INDEX `I_HouseholdChangeHistory_tenantGuid` ON `HouseholdChangeHistory` (`tenantGuid`);

-- Index on the HouseholdChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_HouseholdChangeHistory_tenantGuid_versionNumber` ON `HouseholdChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the HouseholdChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_HouseholdChangeHistory_tenantGuid_timeStamp` ON `HouseholdChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the HouseholdChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_HouseholdChangeHistory_tenantGuid_userId` ON `HouseholdChangeHistory` (`tenantGuid`, `userId`);

-- Index on the HouseholdChangeHistory table's tenantGuid,householdId fields.
CREATE INDEX `I_HouseholdChangeHistory_tenantGuid_householdId` ON `HouseholdChangeHistory` (`tenantGuid`, `householdId`, `versionNumber`, `timeStamp`, `userId`);


-- Defines stages in a donor's journey (e.g., Target, Qualified, Cultivated, Solicited, Stewardship).
CREATE TABLE `ConstituentJourneyStage`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`minLifetimeGiving` DECIMAL(11,2) NULL,		-- Optional criteria: Minimum total giving to qualify for this stage.
	`maxLifetimeGiving` DECIMAL(11,2) NULL,		-- Optional criteria: Maximum total giving
	`minSingleGiftAmount` DECIMAL(11,2) NULL,		-- Optional criteria: Min single gift size
	`isDefault` BIT NOT NULL,		-- If true, this is the default stage for new constituents.
	`minAnnualGiving` DECIMAL(11,2) NULL,		-- Optional: Minimum giving in the past 365 days.
	`maxDaysSinceLastGift` INT NULL DEFAULT 0,		-- Optional: Maximum days elapsed since the last gift (recency limit).
	`minGiftCount` INT NULL DEFAULT 0,		-- Optional: Minimum number of gifts required.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_ConstituentJourneyStage_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the ConstituentJourneyStage table's tenantGuid and name fields.
);
-- Index on the ConstituentJourneyStage table's tenantGuid field.
CREATE INDEX `I_ConstituentJourneyStage_tenantGuid` ON `ConstituentJourneyStage` (`tenantGuid`);

-- Index on the ConstituentJourneyStage table's tenantGuid,name fields.
CREATE INDEX `I_ConstituentJourneyStage_tenantGuid_name` ON `ConstituentJourneyStage` (`tenantGuid`, `name`);

-- Index on the ConstituentJourneyStage table's tenantGuid,iconId fields.
CREATE INDEX `I_ConstituentJourneyStage_tenantGuid_iconId` ON `ConstituentJourneyStage` (`tenantGuid`, `iconId`);

-- Index on the ConstituentJourneyStage table's tenantGuid,active fields.
CREATE INDEX `I_ConstituentJourneyStage_tenantGuid_active` ON `ConstituentJourneyStage` (`tenantGuid`, `active`);

-- Index on the ConstituentJourneyStage table's tenantGuid,deleted fields.
CREATE INDEX `I_ConstituentJourneyStage_tenantGuid_deleted` ON `ConstituentJourneyStage` (`tenantGuid`, `deleted`);

INSERT INTO `ConstituentJourneyStage` ( `tenantGuid`, `name`, `description`, `sequence`, `isDefault`, `color`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Unqualified', 'New potential donor.', 1, 1, '#9E9E9E', 'd8663e5e-749c-4638-b69d-21d96078659d' );

INSERT INTO `ConstituentJourneyStage` ( `tenantGuid`, `name`, `description`, `sequence`, `isDefault`, `color`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Qualified', 'Donor has been qualified.', 2, 0, '#2196F3', 'ad06353d-2476-4322-836f-5374825968f9' );

INSERT INTO `ConstituentJourneyStage` ( `tenantGuid`, `name`, `description`, `sequence`, `isDefault`, `color`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Cultivated', 'Relationship is being built.', 3, 0, '#4CAF50', 'e8b60384-9336-4022-8b4b-970752538965' );

INSERT INTO `ConstituentJourneyStage` ( `tenantGuid`, `name`, `description`, `sequence`, `isDefault`, `color`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Solicited', 'Ask has been made.', 4, 0, '#FF9800', '64319688-fd06-4074-8902-628670bf7471' );

INSERT INTO `ConstituentJourneyStage` ( `tenantGuid`, `name`, `description`, `sequence`, `isDefault`, `color`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Stewardship', 'Ongoing maintenance.', 5, 0, '#9C27B0', '1d971578-8319-482a-9e8c-529141873837' );


-- The change history for records from the ConstituentJourneyStage table.
CREATE TABLE `ConstituentJourneyStageChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`constituentJourneyStageId` INT NOT NULL,		-- Link to the ConstituentJourneyStage table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`constituentJourneyStageId`) REFERENCES `ConstituentJourneyStage`(`id`)		-- Foreign key to the ConstituentJourneyStage table.
);
-- Index on the ConstituentJourneyStageChangeHistory table's tenantGuid field.
CREATE INDEX `I_ConstituentJourneyStageChangeHistory_tenantGuid` ON `ConstituentJourneyStageChangeHistory` (`tenantGuid`);

-- Index on the ConstituentJourneyStageChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ConstituentJourneyStageChangeHistory_tenantGuid_versionNumber` ON `ConstituentJourneyStageChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ConstituentJourneyStageChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ConstituentJourneyStageChangeHistory_tenantGuid_timeStamp` ON `ConstituentJourneyStageChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ConstituentJourneyStageChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ConstituentJourneyStageChangeHistory_tenantGuid_userId` ON `ConstituentJourneyStageChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ConstituentJourneyStageChangeHistory table's tenantGuid,constituentJourneyStageId fields.
CREATE INDEX `I_ConstituentJourneyStageChangeHistory_tenantGuid_constituentJou` ON `ConstituentJourneyStageChangeHistory` (`tenantGuid`, `constituentJourneyStageId`, `versionNumber`, `timeStamp`, `userId`);


/*
 ====================================================================================================
   CONSTITUENT MANAGEMENT
   In DP, a Constituent is the heart of the system. 
   Here, we link to your existing Contact (Person) or Client (Organization) tables.
   This table stores the "Fundraising Intelligence" (RFM metrics).
   ====================================================================================================
*/
CREATE TABLE `Constituent`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`contactId` INT NULL,		-- Polymorphic Link: A constituent is EITHER a generic Contact OR a Client (Company)
	`clientId` INT NULL,		-- Polymorphic Link: A constituent is EITHER a generic Contact OR a Client (Company)
	`householdId` INT NULL,		-- Links a constituent to a household
	`constituentNumber` VARCHAR(50) NOT NULL,		-- The distinct 'Donor ID'
	`doNotSolicit` BIT NOT NULL DEFAULT 0,
	`doNotEmail` BIT NOT NULL DEFAULT 0,
	`doNotMail` BIT NOT NULL DEFAULT 0,
	`totalLifetimeGiving` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Caching Calculated Metrics(DP Style for fast reporting)
	`totalYTDGiving` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Caching Calculated Metrics(DP Style for fast reporting)
	`lastGiftDate` DATE NULL,		-- Caching Calculated Metrics(DP Style for fast reporting)
	`lastGiftAmount` DECIMAL(11,2) NULL,		-- Caching Calculated Metrics(DP Style for fast reporting)
	`largestGiftAmount` DECIMAL(11,2) NULL,		-- Caching Calculated Metrics(DP Style for fast reporting)
	`totalGiftCount` INT NULL,		-- Caching Calculated Metrics(DP Style for fast reporting)
	`externalId` VARCHAR(100) NULL,		-- For things like QBO Customer ID
	`notes` TEXT NULL,
	`constituentJourneyStageId` INT NULL,		-- Current stage in the donor journey.
	`dateEnteredCurrentStage` DATETIME NULL,		-- Date when the constituent moved to the current stage.
	`attributes` TEXT NULL,		-- to store arbitrary JSON
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`avatarFileName` VARCHAR(250) NULL,		-- Part of the binary data field setup
	`avatarSize` BIGINT NULL,		-- Part of the binary data field setup
	`avatarData` BLOB NULL,		-- Part of the binary data field setup
	`avatarMimeType` VARCHAR(100) NULL,		-- Part of the binary data field setup
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`contactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`clientId`) REFERENCES `Client`(`id`),		-- Foreign key to the Client table.
	FOREIGN KEY (`householdId`) REFERENCES `Household`(`id`),		-- Foreign key to the Household table.
	FOREIGN KEY (`constituentJourneyStageId`) REFERENCES `ConstituentJourneyStage`(`id`),		-- Foreign key to the ConstituentJourneyStage table.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`)		-- Foreign key to the Icon table.
);
-- Index on the Constituent table's tenantGuid field.
CREATE INDEX `I_Constituent_tenantGuid` ON `Constituent` (`tenantGuid`);

-- Index on the Constituent table's tenantGuid,contactId fields.
CREATE INDEX `I_Constituent_tenantGuid_contactId` ON `Constituent` (`tenantGuid`, `contactId`);

-- Index on the Constituent table's tenantGuid,clientId fields.
CREATE INDEX `I_Constituent_tenantGuid_clientId` ON `Constituent` (`tenantGuid`, `clientId`);

-- Index on the Constituent table's tenantGuid,householdId fields.
CREATE INDEX `I_Constituent_tenantGuid_householdId` ON `Constituent` (`tenantGuid`, `householdId`);

-- Index on the Constituent table's tenantGuid,constituentJourneyStageId fields.
CREATE INDEX `I_Constituent_tenantGuid_constituentJourneyStageId` ON `Constituent` (`tenantGuid`, `constituentJourneyStageId`);

-- Index on the Constituent table's tenantGuid,iconId fields.
CREATE INDEX `I_Constituent_tenantGuid_iconId` ON `Constituent` (`tenantGuid`, `iconId`);

-- Index on the Constituent table's tenantGuid,active fields.
CREATE INDEX `I_Constituent_tenantGuid_active` ON `Constituent` (`tenantGuid`, `active`);

-- Index on the Constituent table's tenantGuid,deleted fields.
CREATE INDEX `I_Constituent_tenantGuid_deleted` ON `Constituent` (`tenantGuid`, `deleted`);


-- The change history for records from the Constituent table.
CREATE TABLE `ConstituentChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`constituentId` INT NOT NULL,		-- Link to the Constituent table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`constituentId`) REFERENCES `Constituent`(`id`)		-- Foreign key to the Constituent table.
);
-- Index on the ConstituentChangeHistory table's tenantGuid field.
CREATE INDEX `I_ConstituentChangeHistory_tenantGuid` ON `ConstituentChangeHistory` (`tenantGuid`);

-- Index on the ConstituentChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ConstituentChangeHistory_tenantGuid_versionNumber` ON `ConstituentChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ConstituentChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ConstituentChangeHistory_tenantGuid_timeStamp` ON `ConstituentChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ConstituentChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ConstituentChangeHistory_tenantGuid_userId` ON `ConstituentChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ConstituentChangeHistory table's tenantGuid,constituentId fields.
CREATE INDEX `I_ConstituentChangeHistory_tenantGuid_constituentId` ON `ConstituentChangeHistory` (`tenantGuid`, `constituentId`, `versionNumber`, `timeStamp`, `userId`);


/*
 ====================================================================================================
   PLEDGES
   A promise to pay. Gifts will link to this to "pay it down".
   ====================================================================================================
*/
CREATE TABLE `Pledge`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`constituentId` INT NOT NULL,		-- Link to the Constituent table.
	`totalAmount` DECIMAL(11,2) NOT NULL,
	`balanceAmount` DECIMAL(11,2) NOT NULL,		-- Calculated: Total - Sum(LinkedGifts)
	`pledgeDate` DATE NOT NULL,
	`startDate` DATE NULL,
	`endDate` DATE NULL,
	`recurrenceFrequencyId` INT NULL,		-- Link to the RecurrenceFrequency table.
	`fundId` INT NOT NULL,		-- Link to the Fund table.
	`campaignId` INT NULL,		-- Link to the Campaign table.
	`appealId` INT NULL,		-- Link to the Appeal table.
	`writeOffAmount` DECIMAL(11,2) NOT NULL,		-- If they default on the pledge
	`isWrittenOff` BIT NOT NULL DEFAULT 0,
	`notes` TEXT NULL,
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`constituentId`) REFERENCES `Constituent`(`id`),		-- Foreign key to the Constituent table.
	FOREIGN KEY (`recurrenceFrequencyId`) REFERENCES `RecurrenceFrequency`(`id`),		-- Foreign key to the RecurrenceFrequency table.
	FOREIGN KEY (`fundId`) REFERENCES `Fund`(`id`),		-- Foreign key to the Fund table.
	FOREIGN KEY (`campaignId`) REFERENCES `Campaign`(`id`),		-- Foreign key to the Campaign table.
	FOREIGN KEY (`appealId`) REFERENCES `Appeal`(`id`)		-- Foreign key to the Appeal table.
);
-- Index on the Pledge table's tenantGuid field.
CREATE INDEX `I_Pledge_tenantGuid` ON `Pledge` (`tenantGuid`);

-- Index on the Pledge table's tenantGuid,constituentId fields.
CREATE INDEX `I_Pledge_tenantGuid_constituentId` ON `Pledge` (`tenantGuid`, `constituentId`);

-- Index on the Pledge table's tenantGuid,recurrenceFrequencyId fields.
CREATE INDEX `I_Pledge_tenantGuid_recurrenceFrequencyId` ON `Pledge` (`tenantGuid`, `recurrenceFrequencyId`);

-- Index on the Pledge table's tenantGuid,fundId fields.
CREATE INDEX `I_Pledge_tenantGuid_fundId` ON `Pledge` (`tenantGuid`, `fundId`);

-- Index on the Pledge table's tenantGuid,campaignId fields.
CREATE INDEX `I_Pledge_tenantGuid_campaignId` ON `Pledge` (`tenantGuid`, `campaignId`);

-- Index on the Pledge table's tenantGuid,appealId fields.
CREATE INDEX `I_Pledge_tenantGuid_appealId` ON `Pledge` (`tenantGuid`, `appealId`);

-- Index on the Pledge table's tenantGuid,active fields.
CREATE INDEX `I_Pledge_tenantGuid_active` ON `Pledge` (`tenantGuid`, `active`);

-- Index on the Pledge table's tenantGuid,deleted fields.
CREATE INDEX `I_Pledge_tenantGuid_deleted` ON `Pledge` (`tenantGuid`, `deleted`);


-- The change history for records from the Pledge table.
CREATE TABLE `PledgeChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`pledgeId` INT NOT NULL,		-- Link to the Pledge table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`pledgeId`) REFERENCES `Pledge`(`id`)		-- Foreign key to the Pledge table.
);
-- Index on the PledgeChangeHistory table's tenantGuid field.
CREATE INDEX `I_PledgeChangeHistory_tenantGuid` ON `PledgeChangeHistory` (`tenantGuid`);

-- Index on the PledgeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_PledgeChangeHistory_tenantGuid_versionNumber` ON `PledgeChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the PledgeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_PledgeChangeHistory_tenantGuid_timeStamp` ON `PledgeChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the PledgeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_PledgeChangeHistory_tenantGuid_userId` ON `PledgeChangeHistory` (`tenantGuid`, `userId`);

-- Index on the PledgeChangeHistory table's tenantGuid,pledgeId fields.
CREATE INDEX `I_PledgeChangeHistory_tenantGuid_pledgeId` ON `PledgeChangeHistory` (`tenantGuid`, `pledgeId`, `versionNumber`, `timeStamp`, `userId`);


-- Master list of tribute types ( memory, honor, etc..)
CREATE TABLE `TributeType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the TributeType table's name field.
CREATE INDEX `I_TributeType_name` ON `TributeType` (`name`);

-- Index on the TributeType table's active field.
CREATE INDEX `I_TributeType_active` ON `TributeType` (`active`);

-- Index on the TributeType table's deleted field.
CREATE INDEX `I_TributeType_deleted` ON `TributeType` (`deleted`);

INSERT INTO `TributeType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'In Memory Of', 'In Memory Of', 1, '27781845-ed5e-4bba-9216-751d5a8d778a' );

INSERT INTO `TributeType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'In Honor Of', 'In Honor Of', 2, '31af7566-28d1-460f-9cd9-9d70711b5983' );


/*
====================================================================================================
   BATCH CONTROL
   This prevents data entry errors by forcing the user to balance "Control Totals" vs "Actual Totals".
   ====================================================================================================
*/
CREATE TABLE `BatchStatus`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the BatchStatus table's name field.
CREATE INDEX `I_BatchStatus_name` ON `BatchStatus` (`name`);

-- Index on the BatchStatus table's active field.
CREATE INDEX `I_BatchStatus_active` ON `BatchStatus` (`active`);

-- Index on the BatchStatus table's deleted field.
CREATE INDEX `I_BatchStatus_deleted` ON `BatchStatus` (`deleted`);

INSERT INTO `BatchStatus` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Open', 'Data entry in progress', 1, 'd87c06b0-9b5e-4597-8968-ad5f987e2afd' );

INSERT INTO `BatchStatus` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Balanced', 'Control totals match entry totals', 2, 'b5942c13-47d1-4753-a655-140454e1d0a4' );

INSERT INTO `BatchStatus` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Posted', 'Transactions committed to GL / Donor History', 3, '640a7bb7-59da-423b-b2e5-a10124594331' );

INSERT INTO `BatchStatus` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Closed', 'Closed', 4, '5c60e28a-ba9f-4098-9a04-50fcb139bd8c' );


-- The Batch Header for processing gifts.
CREATE TABLE `Batch`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`batchNumber` VARCHAR(100) NOT NULL,		-- User-facing ID (e.g., "2026-01-15-MAIL"
	`description` VARCHAR(500) NULL,
	`dateOpened` DATETIME NOT NULL,
	`datePosted` DATETIME NULL,
	`batchStatusId` INT NOT NULL,		-- Link to the BatchStatus table.
	`controlAmount` DECIMAL(11,2) NOT NULL DEFAULT 0,
	`controlCount` INT NOT NULL DEFAULT 0,
	`defaultFundId` INT NULL,		-- Optional default fund
	`defaultCampaignId` INT NULL,		-- Optional default campaign
	`defaultAppealId` INT NULL,		-- Optional default appeal
	`defaultDate` DATE NULL,		-- Optional default date
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`batchStatusId`) REFERENCES `BatchStatus`(`id`),		-- Foreign key to the BatchStatus table.
	FOREIGN KEY (`defaultFundId`) REFERENCES `Fund`(`id`),		-- Foreign key to the Fund table.
	FOREIGN KEY (`defaultCampaignId`) REFERENCES `Campaign`(`id`),		-- Foreign key to the Campaign table.
	FOREIGN KEY (`defaultAppealId`) REFERENCES `Appeal`(`id`)		-- Foreign key to the Appeal table.
);
-- Index on the Batch table's tenantGuid field.
CREATE INDEX `I_Batch_tenantGuid` ON `Batch` (`tenantGuid`);

-- Index on the Batch table's tenantGuid,batchStatusId fields.
CREATE INDEX `I_Batch_tenantGuid_batchStatusId` ON `Batch` (`tenantGuid`, `batchStatusId`);

-- Index on the Batch table's tenantGuid,defaultFundId fields.
CREATE INDEX `I_Batch_tenantGuid_defaultFundId` ON `Batch` (`tenantGuid`, `defaultFundId`);

-- Index on the Batch table's tenantGuid,defaultCampaignId fields.
CREATE INDEX `I_Batch_tenantGuid_defaultCampaignId` ON `Batch` (`tenantGuid`, `defaultCampaignId`);

-- Index on the Batch table's tenantGuid,defaultAppealId fields.
CREATE INDEX `I_Batch_tenantGuid_defaultAppealId` ON `Batch` (`tenantGuid`, `defaultAppealId`);

-- Index on the Batch table's tenantGuid,active fields.
CREATE INDEX `I_Batch_tenantGuid_active` ON `Batch` (`tenantGuid`, `active`);

-- Index on the Batch table's tenantGuid,deleted fields.
CREATE INDEX `I_Batch_tenantGuid_deleted` ON `Batch` (`tenantGuid`, `deleted`);


-- The change history for records from the Batch table.
CREATE TABLE `BatchChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`batchId` INT NOT NULL,		-- Link to the Batch table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`batchId`) REFERENCES `Batch`(`id`)		-- Foreign key to the Batch table.
);
-- Index on the BatchChangeHistory table's tenantGuid field.
CREATE INDEX `I_BatchChangeHistory_tenantGuid` ON `BatchChangeHistory` (`tenantGuid`);

-- Index on the BatchChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_BatchChangeHistory_tenantGuid_versionNumber` ON `BatchChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the BatchChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_BatchChangeHistory_tenantGuid_timeStamp` ON `BatchChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the BatchChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_BatchChangeHistory_tenantGuid_userId` ON `BatchChangeHistory` (`tenantGuid`, `userId`);

-- Index on the BatchChangeHistory table's tenantGuid,batchId fields.
CREATE INDEX `I_BatchChangeHistory_tenantGuid_batchId` ON `BatchChangeHistory` (`tenantGuid`, `batchId`, `versionNumber`, `timeStamp`, `userId`);


-- The Tribute Definition (e.g., "The John Doe Memorial Fund")
CREATE TABLE `Tribute`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`tributeTypeId` INT NULL,		-- Link to the TributeType table.
	`defaultAcknowledgeeId` INT NULL,		-- Constituent to notify (e.g., the widow)
	`startDate` DATE NULL,
	`endDate` DATE NULL,
	`iconId` INT NULL,		-- Icon to use for UI display
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`avatarFileName` VARCHAR(250) NULL,		-- Part of the binary data field setup
	`avatarSize` BIGINT NULL,		-- Part of the binary data field setup
	`avatarData` BLOB NULL,		-- Part of the binary data field setup
	`avatarMimeType` VARCHAR(100) NULL,		-- Part of the binary data field setup
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`tributeTypeId`) REFERENCES `TributeType`(`id`),		-- Foreign key to the TributeType table.
	FOREIGN KEY (`defaultAcknowledgeeId`) REFERENCES `Constituent`(`id`),		-- Foreign key to the Constituent table.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_Tribute_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the Tribute table's tenantGuid and name fields.
);
-- Index on the Tribute table's tenantGuid field.
CREATE INDEX `I_Tribute_tenantGuid` ON `Tribute` (`tenantGuid`);

-- Index on the Tribute table's tenantGuid,name fields.
CREATE INDEX `I_Tribute_tenantGuid_name` ON `Tribute` (`tenantGuid`, `name`);

-- Index on the Tribute table's tenantGuid,tributeTypeId fields.
CREATE INDEX `I_Tribute_tenantGuid_tributeTypeId` ON `Tribute` (`tenantGuid`, `tributeTypeId`);

-- Index on the Tribute table's tenantGuid,iconId fields.
CREATE INDEX `I_Tribute_tenantGuid_iconId` ON `Tribute` (`tenantGuid`, `iconId`);

-- Index on the Tribute table's tenantGuid,active fields.
CREATE INDEX `I_Tribute_tenantGuid_active` ON `Tribute` (`tenantGuid`, `active`);

-- Index on the Tribute table's tenantGuid,deleted fields.
CREATE INDEX `I_Tribute_tenantGuid_deleted` ON `Tribute` (`tenantGuid`, `deleted`);


-- The change history for records from the Tribute table.
CREATE TABLE `TributeChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`tributeId` INT NOT NULL,		-- Link to the Tribute table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`tributeId`) REFERENCES `Tribute`(`id`)		-- Foreign key to the Tribute table.
);
-- Index on the TributeChangeHistory table's tenantGuid field.
CREATE INDEX `I_TributeChangeHistory_tenantGuid` ON `TributeChangeHistory` (`tenantGuid`);

-- Index on the TributeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_TributeChangeHistory_tenantGuid_versionNumber` ON `TributeChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the TributeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_TributeChangeHistory_tenantGuid_timeStamp` ON `TributeChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the TributeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_TributeChangeHistory_tenantGuid_userId` ON `TributeChangeHistory` (`tenantGuid`, `userId`);

-- Index on the TributeChangeHistory table's tenantGuid,tributeId fields.
CREATE INDEX `I_TributeChangeHistory_tenantGuid_tributeId` ON `TributeChangeHistory` (`tenantGuid`, `tributeId`, `versionNumber`, `timeStamp`, `userId`);


/*
  ====================================================================================================
   GIFTS (Transactions)
   The money coming in.
   ====================================================================================================
*/
CREATE TABLE `Gift`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`officeId` INT NULL,		-- Which office received/owns this gift
	`constituentId` INT NOT NULL,		-- Link to the Constituent table.
	`pledgeId` INT NULL,		-- Link to the Pledge table.
	`amount` DECIMAL(11,2) NOT NULL,
	`receivedDate` DATETIME NOT NULL,		-- When it was recieved
	`postedDate` DATETIME NULL,		-- When it hit the GL
	`fundId` INT NOT NULL,		-- Link to the Fund table.
	`campaignId` INT NULL,		-- Link to the Campaign table.
	`appealId` INT NULL,		-- Link to the Appeal table.
	`paymentTypeId` INT NOT NULL,		-- Link to the PaymentType table.
	`referenceNumber` VARCHAR(100) NULL,		-- Check # or Transaction ID
	`batchId` INT NULL,		-- Link to processing batch
	`receiptTypeId` INT NULL,		-- Link to the ReceiptType table.
	`receiptDate` DATETIME NULL,
	`tributeId` INT NULL,		-- Link to the Tribute table.
	`notes` TEXT NULL,
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`officeId`) REFERENCES `Office`(`id`),		-- Foreign key to the Office table.
	FOREIGN KEY (`constituentId`) REFERENCES `Constituent`(`id`),		-- Foreign key to the Constituent table.
	FOREIGN KEY (`pledgeId`) REFERENCES `Pledge`(`id`),		-- Foreign key to the Pledge table.
	FOREIGN KEY (`fundId`) REFERENCES `Fund`(`id`),		-- Foreign key to the Fund table.
	FOREIGN KEY (`campaignId`) REFERENCES `Campaign`(`id`),		-- Foreign key to the Campaign table.
	FOREIGN KEY (`appealId`) REFERENCES `Appeal`(`id`),		-- Foreign key to the Appeal table.
	FOREIGN KEY (`paymentTypeId`) REFERENCES `PaymentType`(`id`),		-- Foreign key to the PaymentType table.
	FOREIGN KEY (`batchId`) REFERENCES `Batch`(`id`),		-- Foreign key to the Batch table.
	FOREIGN KEY (`receiptTypeId`) REFERENCES `ReceiptType`(`id`),		-- Foreign key to the ReceiptType table.
	FOREIGN KEY (`tributeId`) REFERENCES `Tribute`(`id`)		-- Foreign key to the Tribute table.
);
-- Index on the Gift table's tenantGuid field.
CREATE INDEX `I_Gift_tenantGuid` ON `Gift` (`tenantGuid`);

-- Index on the Gift table's tenantGuid,officeId fields.
CREATE INDEX `I_Gift_tenantGuid_officeId` ON `Gift` (`tenantGuid`, `officeId`);

-- Index on the Gift table's tenantGuid,constituentId fields.
CREATE INDEX `I_Gift_tenantGuid_constituentId` ON `Gift` (`tenantGuid`, `constituentId`);

-- Index on the Gift table's tenantGuid,pledgeId fields.
CREATE INDEX `I_Gift_tenantGuid_pledgeId` ON `Gift` (`tenantGuid`, `pledgeId`);

-- Index on the Gift table's tenantGuid,fundId fields.
CREATE INDEX `I_Gift_tenantGuid_fundId` ON `Gift` (`tenantGuid`, `fundId`);

-- Index on the Gift table's tenantGuid,campaignId fields.
CREATE INDEX `I_Gift_tenantGuid_campaignId` ON `Gift` (`tenantGuid`, `campaignId`);

-- Index on the Gift table's tenantGuid,appealId fields.
CREATE INDEX `I_Gift_tenantGuid_appealId` ON `Gift` (`tenantGuid`, `appealId`);

-- Index on the Gift table's tenantGuid,paymentTypeId fields.
CREATE INDEX `I_Gift_tenantGuid_paymentTypeId` ON `Gift` (`tenantGuid`, `paymentTypeId`);

-- Index on the Gift table's tenantGuid,batchId fields.
CREATE INDEX `I_Gift_tenantGuid_batchId` ON `Gift` (`tenantGuid`, `batchId`);

-- Index on the Gift table's tenantGuid,receiptTypeId fields.
CREATE INDEX `I_Gift_tenantGuid_receiptTypeId` ON `Gift` (`tenantGuid`, `receiptTypeId`);

-- Index on the Gift table's tenantGuid,tributeId fields.
CREATE INDEX `I_Gift_tenantGuid_tributeId` ON `Gift` (`tenantGuid`, `tributeId`);

-- Index on the Gift table's tenantGuid,active fields.
CREATE INDEX `I_Gift_tenantGuid_active` ON `Gift` (`tenantGuid`, `active`);

-- Index on the Gift table's tenantGuid,deleted fields.
CREATE INDEX `I_Gift_tenantGuid_deleted` ON `Gift` (`tenantGuid`, `deleted`);


-- The change history for records from the Gift table.
CREATE TABLE `GiftChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`giftId` INT NOT NULL,		-- Link to the Gift table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`giftId`) REFERENCES `Gift`(`id`)		-- Foreign key to the Gift table.
);
-- Index on the GiftChangeHistory table's tenantGuid field.
CREATE INDEX `I_GiftChangeHistory_tenantGuid` ON `GiftChangeHistory` (`tenantGuid`);

-- Index on the GiftChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_GiftChangeHistory_tenantGuid_versionNumber` ON `GiftChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the GiftChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_GiftChangeHistory_tenantGuid_timeStamp` ON `GiftChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the GiftChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_GiftChangeHistory_tenantGuid_userId` ON `GiftChangeHistory` (`tenantGuid`, `userId`);

-- Index on the GiftChangeHistory table's tenantGuid,giftId fields.
CREATE INDEX `I_GiftChangeHistory_tenantGuid_giftId` ON `GiftChangeHistory` (`tenantGuid`, `giftId`, `versionNumber`, `timeStamp`, `userId`);


/*
====================================================================================================
   SOFT CREDITS
   Critical for DP functionality. Allows a gift from "Husband" to also show up on "Wife's" record 
   without doubling the financial totals.
   ====================================================================================================
*/
CREATE TABLE `SoftCredit`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`giftId` INT NOT NULL,		-- Link to the Gift table.
	`constituentId` INT NOT NULL,		-- The person getting the soft credit
	`amount` DECIMAL(11,2) NOT NULL,		-- Might be full amount or partial
	`notes` TEXT NULL,
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`giftId`) REFERENCES `Gift`(`id`),		-- Foreign key to the Gift table.
	FOREIGN KEY (`constituentId`) REFERENCES `Constituent`(`id`)		-- Foreign key to the Constituent table.
);
-- Index on the SoftCredit table's tenantGuid field.
CREATE INDEX `I_SoftCredit_tenantGuid` ON `SoftCredit` (`tenantGuid`);

-- Index on the SoftCredit table's tenantGuid,giftId fields.
CREATE INDEX `I_SoftCredit_tenantGuid_giftId` ON `SoftCredit` (`tenantGuid`, `giftId`);

-- Index on the SoftCredit table's tenantGuid,constituentId fields.
CREATE INDEX `I_SoftCredit_tenantGuid_constituentId` ON `SoftCredit` (`tenantGuid`, `constituentId`);

-- Index on the SoftCredit table's tenantGuid,active fields.
CREATE INDEX `I_SoftCredit_tenantGuid_active` ON `SoftCredit` (`tenantGuid`, `active`);

-- Index on the SoftCredit table's tenantGuid,deleted fields.
CREATE INDEX `I_SoftCredit_tenantGuid_deleted` ON `SoftCredit` (`tenantGuid`, `deleted`);


-- The change history for records from the SoftCredit table.
CREATE TABLE `SoftCreditChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`softCreditId` INT NOT NULL,		-- Link to the SoftCredit table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`softCreditId`) REFERENCES `SoftCredit`(`id`)		-- Foreign key to the SoftCredit table.
);
-- Index on the SoftCreditChangeHistory table's tenantGuid field.
CREATE INDEX `I_SoftCreditChangeHistory_tenantGuid` ON `SoftCreditChangeHistory` (`tenantGuid`);

-- Index on the SoftCreditChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_SoftCreditChangeHistory_tenantGuid_versionNumber` ON `SoftCreditChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the SoftCreditChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_SoftCreditChangeHistory_tenantGuid_timeStamp` ON `SoftCreditChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the SoftCreditChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_SoftCreditChangeHistory_tenantGuid_userId` ON `SoftCreditChangeHistory` (`tenantGuid`, `userId`);

-- Index on the SoftCreditChangeHistory table's tenantGuid,softCreditId fields.
CREATE INDEX `I_SoftCreditChangeHistory_tenantGuid_softCreditId` ON `SoftCreditChangeHistory` (`tenantGuid`, `softCreditId`, `versionNumber`, `timeStamp`, `userId`);


/*
Volunteer-specific extended profile.
One-to-one with Resource — allows volunteers to be scheduled just like paid resources
while carrying volunteer-specific metadata, hours tracking, preferences, etc.
*/
CREATE TABLE `VolunteerProfile`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`resourceId` INT NOT NULL,		-- The Resource this volunteer profile belongs to (1:1)
	`volunteerStatusId` INT NOT NULL,		-- Current lifecycle status of this volunteer
	`onboardedDate` DATE NULL,		-- Date volunteer was approved/onboarded
	`inactiveSince` DATE NULL,		-- If inactive, when they went inactive
	`totalHoursServed` FLOAT NULL DEFAULT 0,		-- Cached/rolled-up lifetime volunteer hours
	`lastActivityDate` DATE NULL,		-- Most recent event/assignment end date
	`backgroundCheckCompleted` BIT NOT NULL DEFAULT 0,
	`backgroundCheckDate` DATE NULL,
	`backgroundCheckExpiry` DATE NULL,
	`confidentialityAgreementSigned` BIT NOT NULL DEFAULT 0,
	`confidentialityAgreementDate` DATE NULL,
	`availabilityPreferences` TEXT NULL,		-- Free text or structured JSON: e.g. 'prefers weekends', 'no evenings after 8pm'
	`interestsAndSkillsNotes` TEXT NULL,		-- Self-reported interests, hobbies, or extra skills
	`emergencyContactNotes` TEXT NULL,		-- Any special emergency instructions or notes
	`constituentId` INT NULL,		-- Optional link to fundraising/constituent record if relevant
	`iconId` INT NULL,		-- Optional override icon for volunteer-specific UI
	`color` VARCHAR(10) NULL,		-- Optional override color
	`attributes` TEXT NULL,		-- Arbitrary JSON for future extension
	`linkedUserGuid` CHAR(38) NULL,		-- Security user GUID for self-service Hub access
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`),		-- Foreign key to the Resource table.
	FOREIGN KEY (`volunteerStatusId`) REFERENCES `VolunteerStatus`(`id`),		-- Foreign key to the VolunteerStatus table.
	FOREIGN KEY (`constituentId`) REFERENCES `Constituent`(`id`),		-- Foreign key to the Constituent table.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_VolunteerProfile_tenantGuid_resourceId_Unique`( `tenantGuid`, `resourceId` ) 		-- Uniqueness enforced on the VolunteerProfile table's tenantGuid and resourceId fields.
);
-- Index on the VolunteerProfile table's tenantGuid field.
CREATE INDEX `I_VolunteerProfile_tenantGuid` ON `VolunteerProfile` (`tenantGuid`);

-- Index on the VolunteerProfile table's tenantGuid,resourceId fields.
CREATE INDEX `I_VolunteerProfile_tenantGuid_resourceId` ON `VolunteerProfile` (`tenantGuid`, `resourceId`);

-- Index on the VolunteerProfile table's tenantGuid,volunteerStatusId fields.
CREATE INDEX `I_VolunteerProfile_tenantGuid_volunteerStatusId` ON `VolunteerProfile` (`tenantGuid`, `volunteerStatusId`);

-- Index on the VolunteerProfile table's tenantGuid,constituentId fields.
CREATE INDEX `I_VolunteerProfile_tenantGuid_constituentId` ON `VolunteerProfile` (`tenantGuid`, `constituentId`);

-- Index on the VolunteerProfile table's tenantGuid,iconId fields.
CREATE INDEX `I_VolunteerProfile_tenantGuid_iconId` ON `VolunteerProfile` (`tenantGuid`, `iconId`);

-- Index on the VolunteerProfile table's tenantGuid,active fields.
CREATE INDEX `I_VolunteerProfile_tenantGuid_active` ON `VolunteerProfile` (`tenantGuid`, `active`);

-- Index on the VolunteerProfile table's tenantGuid,deleted fields.
CREATE INDEX `I_VolunteerProfile_tenantGuid_deleted` ON `VolunteerProfile` (`tenantGuid`, `deleted`);


-- The change history for records from the VolunteerProfile table.
CREATE TABLE `VolunteerProfileChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`volunteerProfileId` INT NOT NULL,		-- Link to the VolunteerProfile table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`volunteerProfileId`) REFERENCES `VolunteerProfile`(`id`)		-- Foreign key to the VolunteerProfile table.
);
-- Index on the VolunteerProfileChangeHistory table's tenantGuid field.
CREATE INDEX `I_VolunteerProfileChangeHistory_tenantGuid` ON `VolunteerProfileChangeHistory` (`tenantGuid`);

-- Index on the VolunteerProfileChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_VolunteerProfileChangeHistory_tenantGuid_versionNumber` ON `VolunteerProfileChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the VolunteerProfileChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_VolunteerProfileChangeHistory_tenantGuid_timeStamp` ON `VolunteerProfileChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the VolunteerProfileChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_VolunteerProfileChangeHistory_tenantGuid_userId` ON `VolunteerProfileChangeHistory` (`tenantGuid`, `userId`);

-- Index on the VolunteerProfileChangeHistory table's tenantGuid,volunteerProfileId fields.
CREATE INDEX `I_VolunteerProfileChangeHistory_tenantGuid_volunteerProfileId` ON `VolunteerProfileChangeHistory` (`tenantGuid`, `volunteerProfileId`, `versionNumber`, `timeStamp`, `userId`);


/*
Named, persistent groups of volunteers that are often scheduled together.
Examples: 'Saturday Soup Kitchen Team', 'Festival Setup Crew', 'Board of Directors Helpers'.
Similar to Crew table but volunteer-specific with lighter structure and volunteer-oriented metadata.
*/
CREATE TABLE `VolunteerGroup`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`purpose` TEXT NULL,		-- What this group is mainly used for (e.g. 'Food distribution', 'Event setup & teardown')
	`officeId` INT NULL,		-- Optional office/branch this volunteer group is associated with
	`volunteerStatusId` INT NULL,		-- Minimum status required for members (e.g. Active only)
	`maxMembers` INT NULL,		-- Optional soft cap on group size
	`iconId` INT NULL,		-- Icon for UI display (e.g. group of people, soup bowl, hammer)
	`color` VARCHAR(10) NULL,		-- Suggested color for calendar/events
	`notes` TEXT NULL,
	`avatarFileName` VARCHAR(250) NULL,		-- Part of the binary data field setup
	`avatarSize` BIGINT NULL,		-- Part of the binary data field setup
	`avatarData` BLOB NULL,		-- Part of the binary data field setup
	`avatarMimeType` VARCHAR(100) NULL,		-- Part of the binary data field setup
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`officeId`) REFERENCES `Office`(`id`),		-- Foreign key to the Office table.
	FOREIGN KEY (`volunteerStatusId`) REFERENCES `VolunteerStatus`(`id`),		-- Foreign key to the VolunteerStatus table.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_VolunteerGroup_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the VolunteerGroup table's tenantGuid and name fields.
);
-- Index on the VolunteerGroup table's tenantGuid field.
CREATE INDEX `I_VolunteerGroup_tenantGuid` ON `VolunteerGroup` (`tenantGuid`);

-- Index on the VolunteerGroup table's tenantGuid,name fields.
CREATE INDEX `I_VolunteerGroup_tenantGuid_name` ON `VolunteerGroup` (`tenantGuid`, `name`);

-- Index on the VolunteerGroup table's tenantGuid,officeId fields.
CREATE INDEX `I_VolunteerGroup_tenantGuid_officeId` ON `VolunteerGroup` (`tenantGuid`, `officeId`);

-- Index on the VolunteerGroup table's tenantGuid,volunteerStatusId fields.
CREATE INDEX `I_VolunteerGroup_tenantGuid_volunteerStatusId` ON `VolunteerGroup` (`tenantGuid`, `volunteerStatusId`);

-- Index on the VolunteerGroup table's tenantGuid,iconId fields.
CREATE INDEX `I_VolunteerGroup_tenantGuid_iconId` ON `VolunteerGroup` (`tenantGuid`, `iconId`);

-- Index on the VolunteerGroup table's tenantGuid,active fields.
CREATE INDEX `I_VolunteerGroup_tenantGuid_active` ON `VolunteerGroup` (`tenantGuid`, `active`);

-- Index on the VolunteerGroup table's tenantGuid,deleted fields.
CREATE INDEX `I_VolunteerGroup_tenantGuid_deleted` ON `VolunteerGroup` (`tenantGuid`, `deleted`);


-- The change history for records from the VolunteerGroup table.
CREATE TABLE `VolunteerGroupChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`volunteerGroupId` INT NOT NULL,		-- Link to the VolunteerGroup table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`volunteerGroupId`) REFERENCES `VolunteerGroup`(`id`)		-- Foreign key to the VolunteerGroup table.
);
-- Index on the VolunteerGroupChangeHistory table's tenantGuid field.
CREATE INDEX `I_VolunteerGroupChangeHistory_tenantGuid` ON `VolunteerGroupChangeHistory` (`tenantGuid`);

-- Index on the VolunteerGroupChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_VolunteerGroupChangeHistory_tenantGuid_versionNumber` ON `VolunteerGroupChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the VolunteerGroupChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_VolunteerGroupChangeHistory_tenantGuid_timeStamp` ON `VolunteerGroupChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the VolunteerGroupChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_VolunteerGroupChangeHistory_tenantGuid_userId` ON `VolunteerGroupChangeHistory` (`tenantGuid`, `userId`);

-- Index on the VolunteerGroupChangeHistory table's tenantGuid,volunteerGroupId fields.
CREATE INDEX `I_VolunteerGroupChangeHistory_tenantGuid_volunteerGroupId` ON `VolunteerGroupChangeHistory` (`tenantGuid`, `volunteerGroupId`, `versionNumber`, `timeStamp`, `userId`);


/*
Membership in a VolunteerGroup.
Links Resources (volunteers) to groups, with optional default role and sequence.
*/
CREATE TABLE `VolunteerGroupMember`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`volunteerGroupId` INT NOT NULL,		-- Link to the VolunteerGroup table.
	`resourceId` INT NOT NULL,		-- The volunteer (Resource) in this group
	`assignmentRoleId` INT NULL,		-- Default role this person plays in the group (e.g. 'Team Lead', 'Driver')
	`sequence` INT NOT NULL DEFAULT 1,		-- Display/order position within the group
	`joinedDate` DATE NULL,
	`leftDate` DATE NULL,		-- If they left the group
	`notes` TEXT NULL,		-- e.g. 'Prefers kitchen duties', 'Only available 1st Saturday'
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`volunteerGroupId`) REFERENCES `VolunteerGroup`(`id`),		-- Foreign key to the VolunteerGroup table.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`),		-- Foreign key to the Resource table.
	FOREIGN KEY (`assignmentRoleId`) REFERENCES `AssignmentRole`(`id`),		-- Foreign key to the AssignmentRole table.
	UNIQUE `UC_VolunteerGroupMember_tenantGuid_volunteerGroupId_resourceId_Unique`( `tenantGuid`, `volunteerGroupId`, `resourceId` ) 		-- Uniqueness enforced on the VolunteerGroupMember table's tenantGuid and volunteerGroupId and resourceId fields.
);
-- Index on the VolunteerGroupMember table's tenantGuid field.
CREATE INDEX `I_VolunteerGroupMember_tenantGuid` ON `VolunteerGroupMember` (`tenantGuid`);

-- Index on the VolunteerGroupMember table's tenantGuid,volunteerGroupId fields.
CREATE INDEX `I_VolunteerGroupMember_tenantGuid_volunteerGroupId` ON `VolunteerGroupMember` (`tenantGuid`, `volunteerGroupId`);

-- Index on the VolunteerGroupMember table's tenantGuid,resourceId fields.
CREATE INDEX `I_VolunteerGroupMember_tenantGuid_resourceId` ON `VolunteerGroupMember` (`tenantGuid`, `resourceId`);

-- Index on the VolunteerGroupMember table's tenantGuid,assignmentRoleId fields.
CREATE INDEX `I_VolunteerGroupMember_tenantGuid_assignmentRoleId` ON `VolunteerGroupMember` (`tenantGuid`, `assignmentRoleId`);

-- Index on the VolunteerGroupMember table's tenantGuid,active fields.
CREATE INDEX `I_VolunteerGroupMember_tenantGuid_active` ON `VolunteerGroupMember` (`tenantGuid`, `active`);

-- Index on the VolunteerGroupMember table's tenantGuid,deleted fields.
CREATE INDEX `I_VolunteerGroupMember_tenantGuid_deleted` ON `VolunteerGroupMember` (`tenantGuid`, `deleted`);


-- The change history for records from the VolunteerGroupMember table.
CREATE TABLE `VolunteerGroupMemberChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`volunteerGroupMemberId` INT NOT NULL,		-- Link to the VolunteerGroupMember table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`volunteerGroupMemberId`) REFERENCES `VolunteerGroupMember`(`id`)		-- Foreign key to the VolunteerGroupMember table.
);
-- Index on the VolunteerGroupMemberChangeHistory table's tenantGuid field.
CREATE INDEX `I_VolunteerGroupMemberChangeHistory_tenantGuid` ON `VolunteerGroupMemberChangeHistory` (`tenantGuid`);

-- Index on the VolunteerGroupMemberChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_VolunteerGroupMemberChangeHistory_tenantGuid_versionNumber` ON `VolunteerGroupMemberChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the VolunteerGroupMemberChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_VolunteerGroupMemberChangeHistory_tenantGuid_timeStamp` ON `VolunteerGroupMemberChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the VolunteerGroupMemberChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_VolunteerGroupMemberChangeHistory_tenantGuid_userId` ON `VolunteerGroupMemberChangeHistory` (`tenantGuid`, `userId`);

-- Index on the VolunteerGroupMemberChangeHistory table's tenantGuid,volunteerGroupMemberId fields.
CREATE INDEX `I_VolunteerGroupMemberChangeHistory_tenantGuid_volunteerGroupMem` ON `VolunteerGroupMemberChangeHistory` (`tenantGuid`, `volunteerGroupMemberId`, `versionNumber`, `timeStamp`, `userId`);


-- Master list of document types for classifying attachments (e.g., Rental Agreement, Receipt, Invoice, Photo).
CREATE TABLE `DocumentType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`color` VARCHAR(10) NULL,		-- Hex color for UI display.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the DocumentType table's name field.
CREATE INDEX `I_DocumentType_name` ON `DocumentType` (`name`);

-- Index on the DocumentType table's active field.
CREATE INDEX `I_DocumentType_active` ON `DocumentType` (`active`);

-- Index on the DocumentType table's deleted field.
CREATE INDEX `I_DocumentType_deleted` ON `DocumentType` (`deleted`);

INSERT INTO `DocumentType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Rental Agreement', 'Signed rental or usage agreement', 1, 'f1a1b2c3-d4e5-6789-abcd-ef0123456701' );

INSERT INTO `DocumentType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Receipt', 'Purchase receipt or proof of payment', 2, 'f1a1b2c3-d4e5-6789-abcd-ef0123456702' );

INSERT INTO `DocumentType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Invoice', 'Invoice issued or received', 3, 'f1a1b2c3-d4e5-6789-abcd-ef0123456703' );

INSERT INTO `DocumentType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Photo', 'Photograph or image', 4, 'f1a1b2c3-d4e5-6789-abcd-ef0123456704' );

INSERT INTO `DocumentType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Permit', 'Required permits (liquor license, fire permit, etc.)', 5, 'f1a1b2c3-d4e5-6789-abcd-ef0123456705' );

INSERT INTO `DocumentType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Background Check', 'Background Check supporting documentation (police etc..)', 6, 'f1a1b2c3-d4e5-6789-abcd-ef0123456706' );

INSERT INTO `DocumentType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'License', 'License that certifies a resource for a function (driving etc..)', 7, 'f1a1b2c3-d4e5-6789-abcd-ef0123456707' );

INSERT INTO `DocumentType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Insurance Certificate', 'Liability insurance certificate for event coverage', 8, 'f1a1b2c3-d4e5-6789-abcd-ef0123456708' );

INSERT INTO `DocumentType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Meeting Minutes', 'The notes from a meeting', 9, 'f1a1b2c3-d4e5-6789-abcd-ef0123456709' );

INSERT INTO `DocumentType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Other', 'Other document type', 99, 'f1a1b2c3-d4e5-6789-abcd-ef0123456799' );


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
CREATE TABLE `DocumentFolder`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(250) NOT NULL,		-- Folder display name.
	`description` VARCHAR(500) NULL,		-- Optional folder description.
	`parentDocumentFolderId` INT NULL,		-- Self-referencing FK for folder hierarchy. NULL = root-level folder.
	`iconId` INT NULL,		-- Optional custom folder icon for UI display.
	`color` VARCHAR(10) NULL,		-- Optional folder color for UI display.
	`sequence` INT NOT NULL DEFAULT 0,		-- Display order among sibling folders.
	`notes` TEXT NULL,		-- Optional notes about the folder.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`parentDocumentFolderId`) REFERENCES `DocumentFolder`(`id`),		-- Foreign key to the DocumentFolder table.
	FOREIGN KEY (`iconId`) REFERENCES `Icon`(`id`),		-- Foreign key to the Icon table.
	UNIQUE `UC_DocumentFolder_tenantGuid_parentDocumentFolderId_name_Unique`( `tenantGuid`, `parentDocumentFolderId`, `name` ) 		-- Uniqueness enforced on the DocumentFolder table's tenantGuid and parentDocumentFolderId and name fields.
);
-- Index on the DocumentFolder table's tenantGuid field.
CREATE INDEX `I_DocumentFolder_tenantGuid` ON `DocumentFolder` (`tenantGuid`);

-- Index on the DocumentFolder table's tenantGuid,parentDocumentFolderId fields.
CREATE INDEX `I_DocumentFolder_tenantGuid_parentDocumentFolderId` ON `DocumentFolder` (`tenantGuid`, `parentDocumentFolderId`);

-- Index on the DocumentFolder table's tenantGuid,iconId fields.
CREATE INDEX `I_DocumentFolder_tenantGuid_iconId` ON `DocumentFolder` (`tenantGuid`, `iconId`);

-- Index on the DocumentFolder table's tenantGuid,active fields.
CREATE INDEX `I_DocumentFolder_tenantGuid_active` ON `DocumentFolder` (`tenantGuid`, `active`);

-- Index on the DocumentFolder table's tenantGuid,deleted fields.
CREATE INDEX `I_DocumentFolder_tenantGuid_deleted` ON `DocumentFolder` (`tenantGuid`, `deleted`);


-- The change history for records from the DocumentFolder table.
CREATE TABLE `DocumentFolderChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`documentFolderId` INT NOT NULL,		-- Link to the DocumentFolder table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`documentFolderId`) REFERENCES `DocumentFolder`(`id`)		-- Foreign key to the DocumentFolder table.
);
-- Index on the DocumentFolderChangeHistory table's tenantGuid field.
CREATE INDEX `I_DocumentFolderChangeHistory_tenantGuid` ON `DocumentFolderChangeHistory` (`tenantGuid`);

-- Index on the DocumentFolderChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_DocumentFolderChangeHistory_tenantGuid_versionNumber` ON `DocumentFolderChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the DocumentFolderChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_DocumentFolderChangeHistory_tenantGuid_timeStamp` ON `DocumentFolderChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the DocumentFolderChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_DocumentFolderChangeHistory_tenantGuid_userId` ON `DocumentFolderChangeHistory` (`tenantGuid`, `userId`);

-- Index on the DocumentFolderChangeHistory table's tenantGuid,documentFolderId fields.
CREATE INDEX `I_DocumentFolderChangeHistory_tenantGuid_documentFolderId` ON `DocumentFolderChangeHistory` (`tenantGuid`, `documentFolderId`, `versionNumber`, `timeStamp`, `userId`);


/*
====================================================================================================
 DOCUMENT TAG (Flexible Tagging)
 Lightweight tagging system for documents.  Tags complement DocumentType classification
 and folder organization by allowing multiple user-defined labels on each document
 (e.g., 'urgent', '2026 budget', 'board meeting', 'needs review').

 Tags are tenant-scoped and linked to documents via the DocumentDocumentTag junction table.
 ====================================================================================================
*/
CREATE TABLE `DocumentTag`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NULL,
	`color` VARCHAR(10) NULL,		-- Tag badge color for UI display.
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE `UC_DocumentTag_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the DocumentTag table's tenantGuid and name fields.
);
-- Index on the DocumentTag table's tenantGuid field.
CREATE INDEX `I_DocumentTag_tenantGuid` ON `DocumentTag` (`tenantGuid`);

-- Index on the DocumentTag table's tenantGuid,name fields.
CREATE INDEX `I_DocumentTag_tenantGuid_name` ON `DocumentTag` (`tenantGuid`, `name`);

-- Index on the DocumentTag table's tenantGuid,active fields.
CREATE INDEX `I_DocumentTag_tenantGuid_active` ON `DocumentTag` (`tenantGuid`, `active`);

-- Index on the DocumentTag table's tenantGuid,deleted fields.
CREATE INDEX `I_DocumentTag_tenantGuid_deleted` ON `DocumentTag` (`tenantGuid`, `deleted`);


-- The change history for records from the DocumentTag table.
CREATE TABLE `DocumentTagChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`documentTagId` INT NOT NULL,		-- Link to the DocumentTag table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`documentTagId`) REFERENCES `DocumentTag`(`id`)		-- Foreign key to the DocumentTag table.
);
-- Index on the DocumentTagChangeHistory table's tenantGuid field.
CREATE INDEX `I_DocumentTagChangeHistory_tenantGuid` ON `DocumentTagChangeHistory` (`tenantGuid`);

-- Index on the DocumentTagChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_DocumentTagChangeHistory_tenantGuid_versionNumber` ON `DocumentTagChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the DocumentTagChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_DocumentTagChangeHistory_tenantGuid_timeStamp` ON `DocumentTagChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the DocumentTagChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_DocumentTagChangeHistory_tenantGuid_userId` ON `DocumentTagChangeHistory` (`tenantGuid`, `userId`);

-- Index on the DocumentTagChangeHistory table's tenantGuid,documentTagId fields.
CREATE INDEX `I_DocumentTagChangeHistory_tenantGuid_documentTagId` ON `DocumentTagChangeHistory` (`tenantGuid`, `documentTagId`, `versionNumber`, `timeStamp`, `userId`);


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
CREATE TABLE `Document`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`documentTypeId` INT NULL,		-- Optional type classification (Rental Agreement, Receipt, Photo, etc.). Nullable because general-purpose files in the file manager may not need a type.
	`documentFolderId` INT NULL,		-- Optional folder placement. NULL = root level / unfiled document.
	`name` VARCHAR(250) NOT NULL,		-- Display name for the document.
	`description` VARCHAR(500) NULL,		-- Optional description of the document.
	`fileName` VARCHAR(500) NOT NULL,		-- Original filename with extension (e.g., 'rental-agreement-smith.pdf').
	`mimeType` VARCHAR(100) NOT NULL,		-- MIME type of the file (e.g., 'application/pdf', 'image/jpeg').
	`fileSizeBytes` BIGINT NOT NULL,		-- File size in bytes for UI display.
	`fileDataFileName` VARCHAR(250) NULL,		-- Part of the binary data field setup
	`fileDataSize` BIGINT NULL,		-- Part of the binary data field setup
	`fileDataData` BLOB NULL,		-- Part of the binary data field setup
	`fileDataMimeType` VARCHAR(100) NULL,		-- Part of the binary data field setup
	`storageKey` VARCHAR(500) NULL,		-- Optionaa key into the storage system that maintains the data for this document
	`invoiceId` INT NULL,		-- Optional link to an Invoice (e.g., generated invoice PDF).
	`receiptId` INT NULL,		-- Optional link to a Receipt (e.g., generated receipt PDF).
	`scheduledEventId` INT NULL,		-- Optional link to a ScheduledEvent (e.g., rental agreement for a booking).
	`financialTransactionId` INT NULL,		-- Optional link to a FinancialTransaction (e.g., receipt for a purchase).
	`contactId` INT NULL,		-- Optional link to a Contact.
	`resourceId` INT NULL,		-- Optional link to a Resource.
	`clientId` INT NULL,		-- Optional link to a Client.
	`officeId` INT NULL,		-- Optional link to an Office.
	`crewId` INT NULL,		-- Optional link to a Crew.
	`schedulingTargetId` INT NULL,		-- Optional link to a SchedulingTarget.
	`paymentTransactionId` INT NULL,		-- Optional link to a PaymentTransaction.
	`financialOfficeId` INT NULL,		-- Optional link to a FinancialOffice.
	`tenantProfileId` INT NULL,		-- Optional link to a TenantProfile.
	`campaignId` INT NULL,		-- Optional link to a Campaign.
	`householdId` INT NULL,		-- Optional link to a Household.
	`constituentId` INT NULL,		-- Optional link to a Constituent.
	`tributeId` INT NULL,		-- Optional link to a Tribute.
	`volunteerProfileId` INT NULL,		-- Optional link to a VolunteerProfile.
	`status` VARCHAR(50) NULL,		-- Document workflow status: pending, signed, verified, etc.
	`statusDate` DATETIME NULL,		-- When the status was last changed.
	`statusChangedBy` VARCHAR(100) NULL,		-- Who changed the status.
	`uploadedDate` DATETIME NOT NULL,		-- When the document was uploaded (UTC).
	`uploadedBy` VARCHAR(100) NULL,		-- User who uploaded the document.
	`notes` TEXT NULL,		-- Optional notes about the document.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`documentTypeId`) REFERENCES `DocumentType`(`id`),		-- Foreign key to the DocumentType table.
	FOREIGN KEY (`documentFolderId`) REFERENCES `DocumentFolder`(`id`),		-- Foreign key to the DocumentFolder table.
	FOREIGN KEY (`invoiceId`) REFERENCES `Invoice`(`id`),		-- Foreign key to the Invoice table.
	FOREIGN KEY (`receiptId`) REFERENCES `Receipt`(`id`),		-- Foreign key to the Receipt table.
	FOREIGN KEY (`scheduledEventId`) REFERENCES `ScheduledEvent`(`id`),		-- Foreign key to the ScheduledEvent table.
	FOREIGN KEY (`financialTransactionId`) REFERENCES `FinancialTransaction`(`id`),		-- Foreign key to the FinancialTransaction table.
	FOREIGN KEY (`contactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`),		-- Foreign key to the Resource table.
	FOREIGN KEY (`clientId`) REFERENCES `Client`(`id`),		-- Foreign key to the Client table.
	FOREIGN KEY (`officeId`) REFERENCES `Office`(`id`),		-- Foreign key to the Office table.
	FOREIGN KEY (`crewId`) REFERENCES `Crew`(`id`),		-- Foreign key to the Crew table.
	FOREIGN KEY (`schedulingTargetId`) REFERENCES `SchedulingTarget`(`id`),		-- Foreign key to the SchedulingTarget table.
	FOREIGN KEY (`paymentTransactionId`) REFERENCES `PaymentTransaction`(`id`),		-- Foreign key to the PaymentTransaction table.
	FOREIGN KEY (`financialOfficeId`) REFERENCES `FinancialOffice`(`id`),		-- Foreign key to the FinancialOffice table.
	FOREIGN KEY (`tenantProfileId`) REFERENCES `TenantProfile`(`id`),		-- Foreign key to the TenantProfile table.
	FOREIGN KEY (`campaignId`) REFERENCES `Campaign`(`id`),		-- Foreign key to the Campaign table.
	FOREIGN KEY (`householdId`) REFERENCES `Household`(`id`),		-- Foreign key to the Household table.
	FOREIGN KEY (`constituentId`) REFERENCES `Constituent`(`id`),		-- Foreign key to the Constituent table.
	FOREIGN KEY (`tributeId`) REFERENCES `Tribute`(`id`),		-- Foreign key to the Tribute table.
	FOREIGN KEY (`volunteerProfileId`) REFERENCES `VolunteerProfile`(`id`)		-- Foreign key to the VolunteerProfile table.
);
-- Index on the Document table's tenantGuid field.
CREATE INDEX `I_Document_tenantGuid` ON `Document` (`tenantGuid`);

-- Index on the Document table's tenantGuid,documentTypeId fields.
CREATE INDEX `I_Document_tenantGuid_documentTypeId` ON `Document` (`tenantGuid`, `documentTypeId`);

-- Index on the Document table's tenantGuid,documentFolderId fields.
CREATE INDEX `I_Document_tenantGuid_documentFolderId` ON `Document` (`tenantGuid`, `documentFolderId`);

-- Index on the Document table's tenantGuid,invoiceId fields.
CREATE INDEX `I_Document_tenantGuid_invoiceId` ON `Document` (`tenantGuid`, `invoiceId`);

-- Index on the Document table's tenantGuid,receiptId fields.
CREATE INDEX `I_Document_tenantGuid_receiptId` ON `Document` (`tenantGuid`, `receiptId`);

-- Index on the Document table's tenantGuid,scheduledEventId fields.
CREATE INDEX `I_Document_tenantGuid_scheduledEventId` ON `Document` (`tenantGuid`, `scheduledEventId`);

-- Index on the Document table's tenantGuid,financialTransactionId fields.
CREATE INDEX `I_Document_tenantGuid_financialTransactionId` ON `Document` (`tenantGuid`, `financialTransactionId`);

-- Index on the Document table's tenantGuid,contactId fields.
CREATE INDEX `I_Document_tenantGuid_contactId` ON `Document` (`tenantGuid`, `contactId`);

-- Index on the Document table's tenantGuid,resourceId fields.
CREATE INDEX `I_Document_tenantGuid_resourceId` ON `Document` (`tenantGuid`, `resourceId`);

-- Index on the Document table's tenantGuid,clientId fields.
CREATE INDEX `I_Document_tenantGuid_clientId` ON `Document` (`tenantGuid`, `clientId`);

-- Index on the Document table's tenantGuid,officeId fields.
CREATE INDEX `I_Document_tenantGuid_officeId` ON `Document` (`tenantGuid`, `officeId`);

-- Index on the Document table's tenantGuid,crewId fields.
CREATE INDEX `I_Document_tenantGuid_crewId` ON `Document` (`tenantGuid`, `crewId`);

-- Index on the Document table's tenantGuid,schedulingTargetId fields.
CREATE INDEX `I_Document_tenantGuid_schedulingTargetId` ON `Document` (`tenantGuid`, `schedulingTargetId`);

-- Index on the Document table's tenantGuid,paymentTransactionId fields.
CREATE INDEX `I_Document_tenantGuid_paymentTransactionId` ON `Document` (`tenantGuid`, `paymentTransactionId`);

-- Index on the Document table's tenantGuid,financialOfficeId fields.
CREATE INDEX `I_Document_tenantGuid_financialOfficeId` ON `Document` (`tenantGuid`, `financialOfficeId`);

-- Index on the Document table's tenantGuid,tenantProfileId fields.
CREATE INDEX `I_Document_tenantGuid_tenantProfileId` ON `Document` (`tenantGuid`, `tenantProfileId`);

-- Index on the Document table's tenantGuid,campaignId fields.
CREATE INDEX `I_Document_tenantGuid_campaignId` ON `Document` (`tenantGuid`, `campaignId`);

-- Index on the Document table's tenantGuid,householdId fields.
CREATE INDEX `I_Document_tenantGuid_householdId` ON `Document` (`tenantGuid`, `householdId`);

-- Index on the Document table's tenantGuid,constituentId fields.
CREATE INDEX `I_Document_tenantGuid_constituentId` ON `Document` (`tenantGuid`, `constituentId`);

-- Index on the Document table's tenantGuid,tributeId fields.
CREATE INDEX `I_Document_tenantGuid_tributeId` ON `Document` (`tenantGuid`, `tributeId`);

-- Index on the Document table's tenantGuid,volunteerProfileId fields.
CREATE INDEX `I_Document_tenantGuid_volunteerProfileId` ON `Document` (`tenantGuid`, `volunteerProfileId`);

-- Index on the Document table's tenantGuid,active fields.
CREATE INDEX `I_Document_tenantGuid_active` ON `Document` (`tenantGuid`, `active`);

-- Index on the Document table's tenantGuid,deleted fields.
CREATE INDEX `I_Document_tenantGuid_deleted` ON `Document` (`tenantGuid`, `deleted`);


-- The change history for records from the Document table.
CREATE TABLE `DocumentChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`documentId` INT NOT NULL,		-- Link to the Document table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`documentId`) REFERENCES `Document`(`id`)		-- Foreign key to the Document table.
);
-- Index on the DocumentChangeHistory table's tenantGuid field.
CREATE INDEX `I_DocumentChangeHistory_tenantGuid` ON `DocumentChangeHistory` (`tenantGuid`);

-- Index on the DocumentChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_DocumentChangeHistory_tenantGuid_versionNumber` ON `DocumentChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the DocumentChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_DocumentChangeHistory_tenantGuid_timeStamp` ON `DocumentChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the DocumentChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_DocumentChangeHistory_tenantGuid_userId` ON `DocumentChangeHistory` (`tenantGuid`, `userId`);

-- Index on the DocumentChangeHistory table's tenantGuid,documentId fields.
CREATE INDEX `I_DocumentChangeHistory_tenantGuid_documentId` ON `DocumentChangeHistory` (`tenantGuid`, `documentId`, `versionNumber`, `timeStamp`, `userId`);


-- Junction table linking Documents to DocumentTags. Enables many-to-many tagging of documents.
CREATE TABLE `DocumentDocumentTag`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	`documentId` INT NOT NULL,		-- The document being tagged.
	`documentTagId` INT NOT NULL,		-- The tag applied to the document.
	FOREIGN KEY (`documentId`) REFERENCES `Document`(`id`),		-- Foreign key to the Document table.
	FOREIGN KEY (`documentTagId`) REFERENCES `DocumentTag`(`id`),		-- Foreign key to the DocumentTag table.
	UNIQUE `UC_DocumentDocumentTag_tenantGuid_documentId_documentTagId_Unique`( `tenantGuid`, `documentId`, `documentTagId` ) 		-- Uniqueness enforced on the DocumentDocumentTag table's tenantGuid and documentId and documentTagId fields.
);
-- Index on the DocumentDocumentTag table's tenantGuid field.
CREATE INDEX `I_DocumentDocumentTag_tenantGuid` ON `DocumentDocumentTag` (`tenantGuid`);

-- Index on the DocumentDocumentTag table's tenantGuid,active fields.
CREATE INDEX `I_DocumentDocumentTag_tenantGuid_active` ON `DocumentDocumentTag` (`tenantGuid`, `active`);

-- Index on the DocumentDocumentTag table's tenantGuid,deleted fields.
CREATE INDEX `I_DocumentDocumentTag_tenantGuid_deleted` ON `DocumentDocumentTag` (`tenantGuid`, `deleted`);

-- Index on the DocumentDocumentTag table's tenantGuid,documentId fields.
CREATE INDEX `I_DocumentDocumentTag_tenantGuid_documentId` ON `DocumentDocumentTag` (`tenantGuid`, `documentId`);

-- Index on the DocumentDocumentTag table's tenantGuid,documentTagId fields.
CREATE INDEX `I_DocumentDocumentTag_tenantGuid_documentTagId` ON `DocumentDocumentTag` (`tenantGuid`, `documentTagId`);


-- The change history for records from the DocumentDocumentTag table.
CREATE TABLE `DocumentDocumentTagChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`documentDocumentTagId` INT NOT NULL,		-- Link to the DocumentDocumentTag table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`documentDocumentTagId`) REFERENCES `DocumentDocumentTag`(`id`)		-- Foreign key to the DocumentDocumentTag table.
);
-- Index on the DocumentDocumentTagChangeHistory table's tenantGuid field.
CREATE INDEX `I_DocumentDocumentTagChangeHistory_tenantGuid` ON `DocumentDocumentTagChangeHistory` (`tenantGuid`);

-- Index on the DocumentDocumentTagChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_DocumentDocumentTagChangeHistory_tenantGuid_versionNumber` ON `DocumentDocumentTagChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the DocumentDocumentTagChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_DocumentDocumentTagChangeHistory_tenantGuid_timeStamp` ON `DocumentDocumentTagChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the DocumentDocumentTagChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_DocumentDocumentTagChangeHistory_tenantGuid_userId` ON `DocumentDocumentTagChangeHistory` (`tenantGuid`, `userId`);

-- Index on the DocumentDocumentTagChangeHistory table's tenantGuid,documentDocumentTagId fields.
CREATE INDEX `I_DocumentDocumentTagChangeHistory_tenantGuid_documentDocumentTa` ON `DocumentDocumentTagChangeHistory` (`tenantGuid`, `documentDocumentTagId`, `versionNumber`, `timeStamp`, `userId`);


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
CREATE TABLE `DocumentShareLink`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`documentId` INT NOT NULL,		-- The document this share link provides access to.
	`token` CHAR(38) NOT NULL,		-- Public-facing GUID token used in the share URL. Separate from objectGuid.
	`passwordHash` VARCHAR(250) NULL,		-- Optional bcrypt hash of the password required to access the download.
	`expiresAt` DATETIME NULL,		-- Optional expiry date (UTC). NULL = never expires.
	`maxDownloads` INT NULL,		-- Optional download limit. NULL = unlimited downloads.
	`downloadCount` INT NOT NULL DEFAULT 0,		-- Number of times the file has been downloaded via this link.
	`createdBy` VARCHAR(250) NOT NULL,		-- User who created the share link.
	`createdDate` DATETIME NOT NULL,		-- When the share link was created (UTC).
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`documentId`) REFERENCES `Document`(`id`)		-- Foreign key to the Document table.
);
-- Index on the DocumentShareLink table's tenantGuid field.
CREATE INDEX `I_DocumentShareLink_tenantGuid` ON `DocumentShareLink` (`tenantGuid`);

-- Index on the DocumentShareLink table's tenantGuid,documentId fields.
CREATE INDEX `I_DocumentShareLink_tenantGuid_documentId` ON `DocumentShareLink` (`tenantGuid`, `documentId`);

-- Index on the DocumentShareLink table's tenantGuid,active fields.
CREATE INDEX `I_DocumentShareLink_tenantGuid_active` ON `DocumentShareLink` (`tenantGuid`, `active`);

-- Index on the DocumentShareLink table's tenantGuid,deleted fields.
CREATE INDEX `I_DocumentShareLink_tenantGuid_deleted` ON `DocumentShareLink` (`tenantGuid`, `deleted`);

-- Index on the DocumentShareLink table's token field.
CREATE INDEX `I_DocumentShareLink_token` ON `DocumentShareLink` (`token`);


-- The change history for records from the DocumentShareLink table.
CREATE TABLE `DocumentShareLinkChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`documentShareLinkId` INT NOT NULL,		-- Link to the DocumentShareLink table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`documentShareLinkId`) REFERENCES `DocumentShareLink`(`id`)		-- Foreign key to the DocumentShareLink table.
);
-- Index on the DocumentShareLinkChangeHistory table's tenantGuid field.
CREATE INDEX `I_DocumentShareLinkChangeHistory_tenantGuid` ON `DocumentShareLinkChangeHistory` (`tenantGuid`);

-- Index on the DocumentShareLinkChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_DocumentShareLinkChangeHistory_tenantGuid_versionNumber` ON `DocumentShareLinkChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the DocumentShareLinkChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_DocumentShareLinkChangeHistory_tenantGuid_timeStamp` ON `DocumentShareLinkChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the DocumentShareLinkChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_DocumentShareLinkChangeHistory_tenantGuid_userId` ON `DocumentShareLinkChangeHistory` (`tenantGuid`, `userId`);

-- Index on the DocumentShareLinkChangeHistory table's tenantGuid,documentShareLinkId fields.
CREATE INDEX `I_DocumentShareLinkChangeHistory_tenantGuid_documentShareLinkId` ON `DocumentShareLinkChangeHistory` (`tenantGuid`, `documentShareLinkId`, `versionNumber`, `timeStamp`, `userId`);


-- Links resources, crews, or volunteer groups o events.  Supports partial assignments and role designation.  - If crewId is non-NULL → this row represents assignment of the whole crew - If resourceId is non-NULL and crewId is NULL → individual resource assignment - assignmentStart/End NULL → uses full event duration.  only one of crewId, volunteerGroupId, resourceId should be populated per row (business rule in app layer).
CREATE TABLE `EventResourceAssignment`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`scheduledEventId` INT NOT NULL,		-- Link to the ScheduledEvent table.
	`officeId` INT NULL,		-- Snapshot of office resource assigned to this event belongs to at the time of assignment.  This should never change, and should NOT be updated if a resource moves to a different office post-event assignment.
	`resourceId` INT NULL,		-- Required for individual assignments; should be NULL when crewId is used
	`crewId` INT NULL,		-- Optional – when set, assigns the entire crew as a unit
	`volunteerGroupId` INT NULL,		-- Optional: assign an entire VolunteerGroup instead of/in addition to individual resources or Crew
	`assignmentRoleId` INT NULL,		-- Optional role for this assignment (individual or crew member default)
	`assignmentStatusId` INT NOT NULL DEFAULT 1,		-- NULL = Planned, non-NULL links to AssignmentStatus master table
	`assignmentStartDateTime` DATETIME NULL,		-- NULL = starts at event start
	`assignmentEndDateTime` DATETIME NULL,		-- NULL = ends at event end
	`notes` TEXT NULL,
	`isTravelRequired` BIT NULL,		-- Whether or not travel is required for the assignment
	`travelDurationMinutes` INT NULL DEFAULT 0,		-- Time required to get to the site
	`distanceKilometers` FLOAT NULL DEFAULT 0,		-- Useful for expense calculation
	`startLocation` VARCHAR(100) NULL,
	`actualStartDateTime` DATETIME NULL,
	`actualEndDateTime` DATETIME NULL,
	`actualNotes` TEXT NULL,
	`isVolunteer` BIT NOT NULL DEFAULT 0,/*
True = this is a volunteer (unpaid) assignment.
Used to:
- Exclude from payroll/wage calculations
- Include in volunteer hours totals
- Apply different approval/reminder workflows
- Filter volunteer-specific reports
*/
	`reportedVolunteerHours` FLOAT NULL,		-- Hours the volunteer self-reported (or coordinator entered) for this assignment
	`approvedVolunteerHours` FLOAT NULL,		-- Approved/confirmed hours (may differ from reported if adjustments needed)
	`hoursApprovedByContactId` INT NULL,		-- Contact (usually staff/coordinator) who approved the hours
	`approvedDateTime` DATETIME NULL,		-- When the hours were approved
	`reimbursementAmount` DECIMAL(11,2) NULL,		-- Optional: mileage, parking, meals, etc. — not a wage
	`chargeTypeId` INT NULL,		-- Optional: links to an expense-type ChargeType for the reimbursement (e.g. 'Mileage Reimbursement')
	`reimbursementRequested` BIT NOT NULL DEFAULT 0,		-- Volunteer has flagged that they want/need reimbursement
	`volunteerNotes` TEXT NULL,		-- Volunteer-specific notes for this assignment (e.g. 'Prefers morning shifts next time', 'Brought own tools')
	`reminderSentDateTime` DATETIME NULL,		-- When the last automated reminder was sent for this assignment; NULL = no reminder sent yet
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`scheduledEventId`) REFERENCES `ScheduledEvent`(`id`),		-- Foreign key to the ScheduledEvent table.
	FOREIGN KEY (`officeId`) REFERENCES `Office`(`id`),		-- Foreign key to the Office table.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`),		-- Foreign key to the Resource table.
	FOREIGN KEY (`crewId`) REFERENCES `Crew`(`id`),		-- Foreign key to the Crew table.
	FOREIGN KEY (`volunteerGroupId`) REFERENCES `VolunteerGroup`(`id`),		-- Foreign key to the VolunteerGroup table.
	FOREIGN KEY (`assignmentRoleId`) REFERENCES `AssignmentRole`(`id`),		-- Foreign key to the AssignmentRole table.
	FOREIGN KEY (`assignmentStatusId`) REFERENCES `AssignmentStatus`(`id`),		-- Foreign key to the AssignmentStatus table.
	FOREIGN KEY (`hoursApprovedByContactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`chargeTypeId`) REFERENCES `ChargeType`(`id`)		-- Foreign key to the ChargeType table.
);
-- Index on the EventResourceAssignment table's tenantGuid field.
CREATE INDEX `I_EventResourceAssignment_tenantGuid` ON `EventResourceAssignment` (`tenantGuid`);

-- Index on the EventResourceAssignment table's tenantGuid,scheduledEventId fields.
CREATE INDEX `I_EventResourceAssignment_tenantGuid_scheduledEventId` ON `EventResourceAssignment` (`tenantGuid`, `scheduledEventId`);

-- Index on the EventResourceAssignment table's tenantGuid,officeId fields.
CREATE INDEX `I_EventResourceAssignment_tenantGuid_officeId` ON `EventResourceAssignment` (`tenantGuid`, `officeId`);

-- Index on the EventResourceAssignment table's tenantGuid,resourceId fields.
CREATE INDEX `I_EventResourceAssignment_tenantGuid_resourceId` ON `EventResourceAssignment` (`tenantGuid`, `resourceId`);

-- Index on the EventResourceAssignment table's tenantGuid,crewId fields.
CREATE INDEX `I_EventResourceAssignment_tenantGuid_crewId` ON `EventResourceAssignment` (`tenantGuid`, `crewId`);

-- Index on the EventResourceAssignment table's tenantGuid,volunteerGroupId fields.
CREATE INDEX `I_EventResourceAssignment_tenantGuid_volunteerGroupId` ON `EventResourceAssignment` (`tenantGuid`, `volunteerGroupId`);

-- Index on the EventResourceAssignment table's tenantGuid,assignmentRoleId fields.
CREATE INDEX `I_EventResourceAssignment_tenantGuid_assignmentRoleId` ON `EventResourceAssignment` (`tenantGuid`, `assignmentRoleId`);

-- Index on the EventResourceAssignment table's tenantGuid,assignmentStatusId fields.
CREATE INDEX `I_EventResourceAssignment_tenantGuid_assignmentStatusId` ON `EventResourceAssignment` (`tenantGuid`, `assignmentStatusId`);

-- Index on the EventResourceAssignment table's tenantGuid,hoursApprovedByContactId fields.
CREATE INDEX `I_EventResourceAssignment_tenantGuid_hoursApprovedByContactId` ON `EventResourceAssignment` (`tenantGuid`, `hoursApprovedByContactId`);

-- Index on the EventResourceAssignment table's tenantGuid,chargeTypeId fields.
CREATE INDEX `I_EventResourceAssignment_tenantGuid_chargeTypeId` ON `EventResourceAssignment` (`tenantGuid`, `chargeTypeId`);

-- Index on the EventResourceAssignment table's tenantGuid,active fields.
CREATE INDEX `I_EventResourceAssignment_tenantGuid_active` ON `EventResourceAssignment` (`tenantGuid`, `active`);

-- Index on the EventResourceAssignment table's tenantGuid,deleted fields.
CREATE INDEX `I_EventResourceAssignment_tenantGuid_deleted` ON `EventResourceAssignment` (`tenantGuid`, `deleted`);

-- Index on the EventResourceAssignment table's tenantGuid,resourceId,assignmentStartDateTime,assignmentEndDateTime fields.
CREATE INDEX `I_EventResourceAssignment_tenantGuid_resourceId_assignmentStartD` ON `EventResourceAssignment` (`tenantGuid`, `resourceId`, `assignmentStartDateTime`, `assignmentEndDateTime`);

-- Index on the EventResourceAssignment table's tenantGuid,crewId,assignmentStartDateTime,assignmentEndDateTime fields.
CREATE INDEX `I_EventResourceAssignment_tenantGuid_crewId_assignmentStartDateT` ON `EventResourceAssignment` (`tenantGuid`, `crewId`, `assignmentStartDateTime`, `assignmentEndDateTime`);


-- The change history for records from the EventResourceAssignment table.
CREATE TABLE `EventResourceAssignmentChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`eventResourceAssignmentId` INT NOT NULL,		-- Link to the EventResourceAssignment table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`eventResourceAssignmentId`) REFERENCES `EventResourceAssignment`(`id`)		-- Foreign key to the EventResourceAssignment table.
);
-- Index on the EventResourceAssignmentChangeHistory table's tenantGuid field.
CREATE INDEX `I_EventResourceAssignmentChangeHistory_tenantGuid` ON `EventResourceAssignmentChangeHistory` (`tenantGuid`);

-- Index on the EventResourceAssignmentChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_EventResourceAssignmentChangeHistory_tenantGuid_versionNumber` ON `EventResourceAssignmentChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the EventResourceAssignmentChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_EventResourceAssignmentChangeHistory_tenantGuid_timeStamp` ON `EventResourceAssignmentChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the EventResourceAssignmentChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_EventResourceAssignmentChangeHistory_tenantGuid_userId` ON `EventResourceAssignmentChangeHistory` (`tenantGuid`, `userId`);

-- Index on the EventResourceAssignmentChangeHistory table's tenantGuid,eventResourceAssignmentId fields.
CREATE INDEX `I_EventResourceAssignmentChangeHistory_tenantGuid_eventResourceA` ON `EventResourceAssignmentChangeHistory` (`tenantGuid`, `eventResourceAssignmentId`, `versionNumber`, `timeStamp`, `userId`);


