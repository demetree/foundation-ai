/*
Scheduler scheduling system database schema.
This is a multi-tenant resource scheduling system designed primarily for construction resource planning
but flexible enough for other use cases. It supports events, individual and crew-based resource assignments,
partial time assignments, role designation, availability blackouts, and calendar grouping.
All operational tables include multi-tenant support, versioning where appropriate, auditing, and security controls.
*/
CREATE DATABASE [Scheduler]
GO

ALTER DATABASE [Scheduler] SET RECOVERY SIMPLE
GO

USE [Scheduler]
GO

CREATE SCHEMA [Scheduler]
GO

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE [Scheduler].[EventResourceAssignmentChangeHistory]
-- DROP TABLE [Scheduler].[EventResourceAssignment]
-- DROP TABLE [Scheduler].[VolunteerGroupMemberChangeHistory]
-- DROP TABLE [Scheduler].[VolunteerGroupMember]
-- DROP TABLE [Scheduler].[VolunteerGroupChangeHistory]
-- DROP TABLE [Scheduler].[VolunteerGroup]
-- DROP TABLE [Scheduler].[VolunteerProfileChangeHistory]
-- DROP TABLE [Scheduler].[VolunteerProfile]
-- DROP TABLE [Scheduler].[SoftCreditChangeHistory]
-- DROP TABLE [Scheduler].[SoftCredit]
-- DROP TABLE [Scheduler].[GiftChangeHistory]
-- DROP TABLE [Scheduler].[Gift]
-- DROP TABLE [Scheduler].[TributeChangeHistory]
-- DROP TABLE [Scheduler].[Tribute]
-- DROP TABLE [Scheduler].[BatchChangeHistory]
-- DROP TABLE [Scheduler].[Batch]
-- DROP TABLE [Scheduler].[BatchStatus]
-- DROP TABLE [Scheduler].[TributeType]
-- DROP TABLE [Scheduler].[PledgeChangeHistory]
-- DROP TABLE [Scheduler].[Pledge]
-- DROP TABLE [Scheduler].[ConstituentChangeHistory]
-- DROP TABLE [Scheduler].[Constituent]
-- DROP TABLE [Scheduler].[ConstituentJourneyStageChangeHistory]
-- DROP TABLE [Scheduler].[ConstituentJourneyStage]
-- DROP TABLE [Scheduler].[HouseholdChangeHistory]
-- DROP TABLE [Scheduler].[Household]
-- DROP TABLE [Scheduler].[AppealChangeHistory]
-- DROP TABLE [Scheduler].[Appeal]
-- DROP TABLE [Scheduler].[CampaignChangeHistory]
-- DROP TABLE [Scheduler].[Campaign]
-- DROP TABLE [Scheduler].[FundChangeHistory]
-- DROP TABLE [Scheduler].[Fund]
-- DROP TABLE [Scheduler].[NotificationSubscriptionChangeHistory]
-- DROP TABLE [Scheduler].[NotificationSubscription]
-- DROP TABLE [Scheduler].[NotificationType]
-- DROP TABLE [Scheduler].[RecurrenceExceptionChangeHistory]
-- DROP TABLE [Scheduler].[RecurrenceException]
-- DROP TABLE [Scheduler].[ScheduledEventQualificationRequirementChangeHistory]
-- DROP TABLE [Scheduler].[ScheduledEventQualificationRequirement]
-- DROP TABLE [Scheduler].[ScheduledEventDependencyChangeHistory]
-- DROP TABLE [Scheduler].[ScheduledEventDependency]
-- DROP TABLE [Scheduler].[DependencyType]
-- DROP TABLE [Scheduler].[EventCalendar]
-- DROP TABLE [Scheduler].[ContactInteractionChangeHistory]
-- DROP TABLE [Scheduler].[ContactInteraction]
-- DROP TABLE [Scheduler].[EventChargeChangeHistory]
-- DROP TABLE [Scheduler].[EventCharge]
-- DROP TABLE [Scheduler].[ChargeStatus]
-- DROP TABLE [Scheduler].[ScheduledEventChangeHistory]
-- DROP TABLE [Scheduler].[ScheduledEvent]
-- DROP TABLE [Scheduler].[ScheduledEventTemplateQualificationRequirementChangeHistory]
-- DROP TABLE [Scheduler].[ScheduledEventTemplateQualificationRequirement]
-- DROP TABLE [Scheduler].[ScheduledEventTemplateChargeChangeHistory]
-- DROP TABLE [Scheduler].[ScheduledEventTemplateCharge]
-- DROP TABLE [Scheduler].[ScheduledEventTemplateChangeHistory]
-- DROP TABLE [Scheduler].[ScheduledEventTemplate]
-- DROP TABLE [Scheduler].[CrewMemberChangeHistory]
-- DROP TABLE [Scheduler].[CrewMember]
-- DROP TABLE [Scheduler].[CrewChangeHistory]
-- DROP TABLE [Scheduler].[Crew]
-- DROP TABLE [Scheduler].[ResourceShiftChangeHistory]
-- DROP TABLE [Scheduler].[ResourceShift]
-- DROP TABLE [Scheduler].[ResourceAvailabilityChangeHistory]
-- DROP TABLE [Scheduler].[ResourceAvailability]
-- DROP TABLE [Scheduler].[ResourceQualificationChangeHistory]
-- DROP TABLE [Scheduler].[ResourceQualification]
-- DROP TABLE [Scheduler].[RateSheetChangeHistory]
-- DROP TABLE [Scheduler].[RateSheet]
-- DROP TABLE [Scheduler].[ResourceContactChangeHistory]
-- DROP TABLE [Scheduler].[ResourceContact]
-- DROP TABLE [Scheduler].[ResourceChangeHistory]
-- DROP TABLE [Scheduler].[Resource]
-- DROP TABLE [Scheduler].[ShiftPatternDayChangeHistory]
-- DROP TABLE [Scheduler].[ShiftPatternDay]
-- DROP TABLE [Scheduler].[ShiftPatternChangeHistory]
-- DROP TABLE [Scheduler].[ShiftPattern]
-- DROP TABLE [Scheduler].[RecurrenceRuleChangeHistory]
-- DROP TABLE [Scheduler].[RecurrenceRule]
-- DROP TABLE [Scheduler].[RecurrenceFrequency]
-- DROP TABLE [Scheduler].[SchedulingTargetQualificationRequirementChangeHistory]
-- DROP TABLE [Scheduler].[SchedulingTargetQualificationRequirement]
-- DROP TABLE [Scheduler].[SchedulingTargetAddressChangeHistory]
-- DROP TABLE [Scheduler].[SchedulingTargetAddress]
-- DROP TABLE [Scheduler].[SchedulingTargetContactChangeHistory]
-- DROP TABLE [Scheduler].[SchedulingTargetContact]
-- DROP TABLE [Scheduler].[SchedulingTargetChangeHistory]
-- DROP TABLE [Scheduler].[SchedulingTarget]
-- DROP TABLE [Scheduler].[SchedulingTargetType]
-- DROP TABLE [Scheduler].[AssignmentStatus]
-- DROP TABLE [Scheduler].[BookingSourceType]
-- DROP TABLE [Scheduler].[ReceiptType]
-- DROP TABLE [Scheduler].[PaymentType]
-- DROP TABLE [Scheduler].[EventStatus]
-- DROP TABLE [Scheduler].[AssignmentRoleQualificationRequirementChangeHistory]
-- DROP TABLE [Scheduler].[AssignmentRoleQualificationRequirement]
-- DROP TABLE [Scheduler].[AssignmentRole]
-- DROP TABLE [Scheduler].[Qualification]
-- DROP TABLE [Scheduler].[TenantProfileChangeHistory]
-- DROP TABLE [Scheduler].[TenantProfile]
-- DROP TABLE [Scheduler].[ClientContactChangeHistory]
-- DROP TABLE [Scheduler].[ClientContact]
-- DROP TABLE [Scheduler].[ClientChangeHistory]
-- DROP TABLE [Scheduler].[Client]
-- DROP TABLE [Scheduler].[ClientType]
-- DROP TABLE [Scheduler].[CalendarChangeHistory]
-- DROP TABLE [Scheduler].[Calendar]
-- DROP TABLE [Scheduler].[OfficeContactChangeHistory]
-- DROP TABLE [Scheduler].[OfficeContact]
-- DROP TABLE [Scheduler].[OfficeChangeHistory]
-- DROP TABLE [Scheduler].[Office]
-- DROP TABLE [Scheduler].[OfficeType]
-- DROP TABLE [Scheduler].[ContactContactChangeHistory]
-- DROP TABLE [Scheduler].[ContactContact]
-- DROP TABLE [Scheduler].[RelationshipType]
-- DROP TABLE [Scheduler].[ContactTagChangeHistory]
-- DROP TABLE [Scheduler].[ContactTag]
-- DROP TABLE [Scheduler].[ContactChangeHistory]
-- DROP TABLE [Scheduler].[Contact]
-- DROP TABLE [Scheduler].[ContactType]
-- DROP TABLE [Scheduler].[VolunteerStatus]
-- DROP TABLE [Scheduler].[StateProvince]
-- DROP TABLE [Scheduler].[Country]
-- DROP TABLE [Scheduler].[TimeZone]
-- DROP TABLE [Scheduler].[Tag]
-- DROP TABLE [Scheduler].[ChargeTypeChangeHistory]
-- DROP TABLE [Scheduler].[ChargeType]
-- DROP TABLE [Scheduler].[Currency]
-- DROP TABLE [Scheduler].[InteractionType]
-- DROP TABLE [Scheduler].[RateType]
-- DROP TABLE [Scheduler].[ContactMethod]
-- DROP TABLE [Scheduler].[Priority]
-- DROP TABLE [Scheduler].[ResourceType]
-- DROP TABLE [Scheduler].[Salutation]
-- DROP TABLE [Scheduler].[Icon]
-- DROP TABLE [Scheduler].[AttributeDefinitionChangeHistory]
-- DROP TABLE [Scheduler].[AttributeDefinition]
-- DROP TABLE [Scheduler].[AttributeDefinitionEntity]
-- DROP TABLE [Scheduler].[AttributeDefinitionType]

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON [Scheduler].[EventResourceAssignmentChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[EventResourceAssignment] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[VolunteerGroupMemberChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[VolunteerGroupMember] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[VolunteerGroupChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[VolunteerGroup] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[VolunteerProfileChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[VolunteerProfile] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[SoftCreditChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[SoftCredit] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[GiftChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Gift] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[TributeChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Tribute] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[BatchChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Batch] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[BatchStatus] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[TributeType] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[PledgeChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Pledge] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ConstituentChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Constituent] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ConstituentJourneyStageChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ConstituentJourneyStage] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[HouseholdChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Household] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[AppealChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Appeal] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[CampaignChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Campaign] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[FundChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Fund] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[NotificationSubscriptionChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[NotificationSubscription] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[NotificationType] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[RecurrenceExceptionChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[RecurrenceException] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventQualificationRequirementChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventQualificationRequirement] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventDependencyChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventDependency] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[DependencyType] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[EventCalendar] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ContactInteractionChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ContactInteraction] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[EventChargeChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[EventCharge] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ChargeStatus] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEvent] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventTemplateQualificationRequirementChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventTemplateQualificationRequirement] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventTemplateChargeChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventTemplateCharge] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventTemplateChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventTemplate] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[CrewMemberChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[CrewMember] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[CrewChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Crew] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ResourceShiftChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ResourceShift] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ResourceAvailabilityChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ResourceAvailability] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ResourceQualificationChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ResourceQualification] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[RateSheetChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[RateSheet] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ResourceContactChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ResourceContact] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ResourceChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Resource] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ShiftPatternDayChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ShiftPatternDay] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ShiftPatternChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ShiftPattern] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[RecurrenceRuleChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[RecurrenceRule] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[RecurrenceFrequency] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTargetQualificationRequirementChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTargetQualificationRequirement] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTargetAddressChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTargetAddress] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTargetContactChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTargetContact] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTargetChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTarget] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTargetType] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[AssignmentStatus] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[BookingSourceType] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ReceiptType] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[PaymentType] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[EventStatus] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[AssignmentRoleQualificationRequirementChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[AssignmentRoleQualificationRequirement] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[AssignmentRole] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Qualification] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[TenantProfileChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[TenantProfile] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ClientContactChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ClientContact] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ClientChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Client] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ClientType] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[CalendarChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Calendar] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[OfficeContactChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[OfficeContact] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[OfficeChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Office] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[OfficeType] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ContactContactChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ContactContact] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[RelationshipType] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ContactTagChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ContactTag] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ContactChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Contact] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ContactType] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[VolunteerStatus] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[StateProvince] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Country] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[TimeZone] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Tag] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ChargeTypeChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ChargeType] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Currency] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[InteractionType] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[RateType] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ContactMethod] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Priority] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[ResourceType] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Salutation] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[Icon] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[AttributeDefinitionChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[AttributeDefinition] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[AttributeDefinitionEntity] DISABLE
-- ALTER INDEX ALL ON [Scheduler].[AttributeDefinitionType] DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON [Scheduler].[EventResourceAssignmentChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[EventResourceAssignment] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[VolunteerGroupMemberChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[VolunteerGroupMember] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[VolunteerGroupChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[VolunteerGroup] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[VolunteerProfileChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[VolunteerProfile] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[SoftCreditChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[SoftCredit] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[GiftChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Gift] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[TributeChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Tribute] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[BatchChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Batch] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[BatchStatus] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[TributeType] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[PledgeChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Pledge] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ConstituentChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Constituent] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ConstituentJourneyStageChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ConstituentJourneyStage] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[HouseholdChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Household] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[AppealChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Appeal] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[CampaignChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Campaign] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[FundChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Fund] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[NotificationSubscriptionChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[NotificationSubscription] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[NotificationType] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[RecurrenceExceptionChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[RecurrenceException] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventQualificationRequirementChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventQualificationRequirement] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventDependencyChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventDependency] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[DependencyType] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[EventCalendar] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ContactInteractionChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ContactInteraction] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[EventChargeChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[EventCharge] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ChargeStatus] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEvent] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventTemplateQualificationRequirementChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventTemplateQualificationRequirement] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventTemplateChargeChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventTemplateCharge] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventTemplateChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ScheduledEventTemplate] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[CrewMemberChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[CrewMember] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[CrewChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Crew] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ResourceShiftChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ResourceShift] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ResourceAvailabilityChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ResourceAvailability] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ResourceQualificationChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ResourceQualification] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[RateSheetChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[RateSheet] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ResourceContactChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ResourceContact] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ResourceChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Resource] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ShiftPatternDayChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ShiftPatternDay] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ShiftPatternChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ShiftPattern] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[RecurrenceRuleChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[RecurrenceRule] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[RecurrenceFrequency] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTargetQualificationRequirementChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTargetQualificationRequirement] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTargetAddressChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTargetAddress] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTargetContactChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTargetContact] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTargetChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTarget] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[SchedulingTargetType] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[AssignmentStatus] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[BookingSourceType] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ReceiptType] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[PaymentType] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[EventStatus] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[AssignmentRoleQualificationRequirementChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[AssignmentRoleQualificationRequirement] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[AssignmentRole] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Qualification] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[TenantProfileChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[TenantProfile] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ClientContactChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ClientContact] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ClientChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Client] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ClientType] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[CalendarChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Calendar] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[OfficeContactChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[OfficeContact] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[OfficeChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Office] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[OfficeType] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ContactContactChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ContactContact] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[RelationshipType] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ContactTagChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ContactTag] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ContactChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Contact] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ContactType] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[VolunteerStatus] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[StateProvince] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Country] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[TimeZone] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Tag] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ChargeTypeChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ChargeType] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Currency] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[InteractionType] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[RateType] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ContactMethod] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Priority] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[ResourceType] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Salutation] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[Icon] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[AttributeDefinitionChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[AttributeDefinition] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[AttributeDefinitionEntity] REBUILD
-- ALTER INDEX ALL ON [Scheduler].[AttributeDefinitionType] REBUILD

-- Master list of available attribute data types.
CREATE TABLE [Scheduler].[AttributeDefinitionType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the AttributeDefinitionType table's name field.
CREATE INDEX [I_AttributeDefinitionType_name] ON [Scheduler].[AttributeDefinitionType] ([name])
GO

-- Index on the AttributeDefinitionType table's active field.
CREATE INDEX [I_AttributeDefinitionType_active] ON [Scheduler].[AttributeDefinitionType] ([active])
GO

-- Index on the AttributeDefinitionType table's deleted field.
CREATE INDEX [I_AttributeDefinitionType_deleted] ON [Scheduler].[AttributeDefinitionType] ([deleted])
GO

INSERT INTO [Scheduler].[AttributeDefinitionType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Text', 'Single line text', 1, 'd1a1b2c3-1111-2222-3333-444455556661' )
GO

INSERT INTO [Scheduler].[AttributeDefinitionType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Number', 'Numeric value', 2, 'd1a1b2c3-1111-2222-3333-444455556662' )
GO

INSERT INTO [Scheduler].[AttributeDefinitionType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Date', 'Date value (no time)', 3, 'd1a1b2c3-1111-2222-3333-444455556663' )
GO

INSERT INTO [Scheduler].[AttributeDefinitionType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Boolean', 'True/False checkbox', 4, 'd1a1b2c3-1111-2222-3333-444455556664' )
GO

INSERT INTO [Scheduler].[AttributeDefinitionType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Select', 'Dropdown selection', 5, 'd1a1b2c3-1111-2222-3333-444455556665' )
GO


-- Master list of entities that support custom attributes.
CREATE TABLE [Scheduler].[AttributeDefinitionEntity]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the AttributeDefinitionEntity table's name field.
CREATE INDEX [I_AttributeDefinitionEntity_name] ON [Scheduler].[AttributeDefinitionEntity] ([name])
GO

-- Index on the AttributeDefinitionEntity table's active field.
CREATE INDEX [I_AttributeDefinitionEntity_active] ON [Scheduler].[AttributeDefinitionEntity] ([active])
GO

-- Index on the AttributeDefinitionEntity table's deleted field.
CREATE INDEX [I_AttributeDefinitionEntity_deleted] ON [Scheduler].[AttributeDefinitionEntity] ([deleted])
GO

INSERT INTO [Scheduler].[AttributeDefinitionEntity] ( [name], [description], [objectGuid] ) VALUES  ( 'Contact', 'Contact Records', 'e2a1b2c3-1111-2222-3333-444455556661' )
GO

INSERT INTO [Scheduler].[AttributeDefinitionEntity] ( [name], [description], [objectGuid] ) VALUES  ( 'Constituent', 'Constituent Records', 'e2a1b2c3-1111-2222-3333-444455556662' )
GO


-- Definitions for custom attributes on various entities (Contact, Constituent, etc.)
CREATE TABLE [Scheduler].[AttributeDefinition]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[attributeDefinitionEntityId] INT NULL,		-- The entity this attribute applies to (e.g., Contact)
	[key] NVARCHAR(100) NULL,		-- The JSON key for the attribute
	[label] NVARCHAR(250) NULL,		-- The human-readable label for the attribute
	[attributeDefinitionTypeId] INT NULL,		-- Data type: Text, Number, Date, etc.
	[options] NVARCHAR(MAX) NULL,		-- JSON options for Select/MultiSelect types
	[isRequired] BIT NOT NULL DEFAULT 0,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_AttributeDefinition_AttributeDefinitionEntity_attributeDefinitionEntityId] FOREIGN KEY ([attributeDefinitionEntityId]) REFERENCES [Scheduler].[AttributeDefinitionEntity] ([id]),		-- Foreign key to the AttributeDefinitionEntity table.
	CONSTRAINT [FK_AttributeDefinition_AttributeDefinitionType_attributeDefinitionTypeId] FOREIGN KEY ([attributeDefinitionTypeId]) REFERENCES [Scheduler].[AttributeDefinitionType] ([id]),		-- Foreign key to the AttributeDefinitionType table.
	CONSTRAINT [UC_AttributeDefinition_tenantGuid_attributeDefinitionEntityId_key] UNIQUE ( [tenantGuid], [attributeDefinitionEntityId], [key]) 		-- Uniqueness enforced on the AttributeDefinition table's tenantGuid and attributeDefinitionEntityId and key fields.
)
GO

-- Index on the AttributeDefinition table's tenantGuid field.
CREATE INDEX [I_AttributeDefinition_tenantGuid] ON [Scheduler].[AttributeDefinition] ([tenantGuid])
GO

-- Index on the AttributeDefinition table's tenantGuid,attributeDefinitionEntityId fields.
CREATE INDEX [I_AttributeDefinition_tenantGuid_attributeDefinitionEntityId] ON [Scheduler].[AttributeDefinition] ([tenantGuid], [attributeDefinitionEntityId])
GO

-- Index on the AttributeDefinition table's tenantGuid,attributeDefinitionTypeId fields.
CREATE INDEX [I_AttributeDefinition_tenantGuid_attributeDefinitionTypeId] ON [Scheduler].[AttributeDefinition] ([tenantGuid], [attributeDefinitionTypeId])
GO

-- Index on the AttributeDefinition table's tenantGuid,active fields.
CREATE INDEX [I_AttributeDefinition_tenantGuid_active] ON [Scheduler].[AttributeDefinition] ([tenantGuid], [active])
GO

-- Index on the AttributeDefinition table's tenantGuid,deleted fields.
CREATE INDEX [I_AttributeDefinition_tenantGuid_deleted] ON [Scheduler].[AttributeDefinition] ([tenantGuid], [deleted])
GO


-- The change history for records from the AttributeDefinition table.
CREATE TABLE [Scheduler].[AttributeDefinitionChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[attributeDefinitionId] INT NOT NULL,		-- Link to the AttributeDefinition table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_AttributeDefinitionChangeHistory_AttributeDefinition_attributeDefinitionId] FOREIGN KEY ([attributeDefinitionId]) REFERENCES [Scheduler].[AttributeDefinition] ([id])		-- Foreign key to the AttributeDefinition table.
)
GO

-- Index on the AttributeDefinitionChangeHistory table's tenantGuid field.
CREATE INDEX [I_AttributeDefinitionChangeHistory_tenantGuid] ON [Scheduler].[AttributeDefinitionChangeHistory] ([tenantGuid])
GO

-- Index on the AttributeDefinitionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_AttributeDefinitionChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[AttributeDefinitionChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the AttributeDefinitionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_AttributeDefinitionChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[AttributeDefinitionChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the AttributeDefinitionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_AttributeDefinitionChangeHistory_tenantGuid_userId] ON [Scheduler].[AttributeDefinitionChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the AttributeDefinitionChangeHistory table's tenantGuid,attributeDefinitionId fields.
CREATE INDEX [I_AttributeDefinitionChangeHistory_tenantGuid_attributeDefinitionId] ON [Scheduler].[AttributeDefinitionChangeHistory] ([tenantGuid], [attributeDefinitionId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- List of icons to use on user interfaces.  Not tenant editable.
CREATE TABLE [Scheduler].[Icon]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[fontAwesomeCode] NVARCHAR(50) NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the Icon table's name field.
CREATE INDEX [I_Icon_name] ON [Scheduler].[Icon] ([name])
GO

-- Index on the Icon table's active field.
CREATE INDEX [I_Icon_active] ON [Scheduler].[Icon] ([active])
GO

-- Index on the Icon table's deleted field.
CREATE INDEX [I_Icon_deleted] ON [Scheduler].[Icon] ([deleted])
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Person', 'fa-solid fa-user', 1, 'a1b2c3d4-5678-9012-3456-789abcde0001' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'People', 'fa-solid fa-users', 2, 'a1b2c3d4-5678-9012-3456-789abcde0002' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Supervisor', 'fa-solid fa-user-tie', 3, 'a1b2c3d4-5678-9012-3456-789abcde0003' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Operator', 'fa-solid fa-hard-hat', 4, 'a1b2c3d4-5678-9012-3456-789abcde0004' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Equipment', 'fa-solid fa-truck', 10, 'a1b2c3d4-5678-9012-3456-789abcde0010' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Roller', 'fa-solid fa-road', 11, 'a1b2c3d4-5678-9012-3456-789abcde0011' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Crane', 'fa-solid fa-tower-broadcast', 12, 'a1b2c3d4-5678-9012-3456-789abcde0012' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Vehicle', 'fa-solid fa-truck-pickup', 13, 'a1b2c3d4-5678-9012-3456-789abcde0013' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Tool', 'fa-solid fa-toolbox', 14, 'a1b2c3d4-5678-9012-3456-789abcde0014' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Room', 'fa-solid fa-door-open', 15, 'a1b2c3d4-5678-9012-3456-789abcde0015' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Project', 'fa-solid fa-briefcase', 20, 'a1b2c3d4-5678-9012-3456-789abcde0020' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Construction Site', 'fa-solid fa-helmet-safety', 21, 'a1b2c3d4-5678-9012-3456-789abcde0021' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Patient', 'fa-solid fa-bed-pulse', 22, 'a1b2c3d4-5678-9012-3456-789abcde0022' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Home', 'fa-solid fa-house-medical', 23, 'a1b2c3d4-5678-9012-3456-789abcde0023' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Calendar', 'fa-solid fa-calendar-days', 30, 'a1b2c3d4-5678-9012-3456-789abcde0030' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Maintenance', 'fa-solid fa-wrench', 31, 'a1b2c3d4-5678-9012-3456-789abcde0031' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Event', 'fa-solid fa-calendar-check', 32, 'a1b2c3d4-5678-9012-3456-789abcde0032' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'High Priority', 'fa-solid fa-triangle-exclamation', 40, 'a1b2c3d4-5678-9012-3456-789abcde0040' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Medium Priority', 'fa-solid fa-circle-exclamation', 41, 'a1b2c3d4-5678-9012-3456-789abcde0041' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Low Priority', 'fa-solid fa-circle-info', 42, 'a1b2c3d4-5678-9012-3456-789abcde0042' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Assignment', 'fa-solid fa-user-check', 50, 'a1b2c3d4-5678-9012-3456-789abcde0050' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Crew', 'fa-solid fa-users-gear', 51, 'a1b2c3d4-5678-9012-3456-789abcde0051' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Qualification', 'fa-solid fa-certificate', 52, 'a1b2c3d4-5678-9012-3456-789abcde0052' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Travel', 'fa-solid fa-car', 53, 'a1b2c3d4-5678-9012-3456-789abcde0053' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Location', 'fa-solid fa-location-dot', 54, 'a1b2c3d4-5678-9012-3456-789abcde0054' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Notification', 'fa-solid fa-bell', 55, 'a1b2c3d4-5678-9012-3456-789abcde0055' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Hammer', 'fa-solid fa-hammer', 100, 'a1b2c3d4-5678-9012-3456-789abcde0100' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Wrench', 'fa-solid fa-wrench', 101, 'a1b2c3d4-5678-9012-3456-789abcde0101' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Screwdriver', 'fa-solid fa-screwdriver-wrench', 102, 'a1b2c3d4-5678-9012-3456-789abcde0102' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Paint Roller', 'fa-solid fa-paint-roller', 103, 'a1b2c3d4-5678-9012-3456-789abcde0103' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Brush', 'fa-solid fa-brush', 104, 'a1b2c3d4-5678-9012-3456-789abcde0104' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Ruler / Measurements', 'fa-solid fa-ruler-combined', 105, 'a1b2c3d4-5678-9012-3456-789abcde0105' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Drafting / Architecture', 'fa-solid fa-compass-drafting', 106, 'a1b2c3d4-5678-9012-3456-789abcde0106' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Electricity / Power', 'fa-solid fa-bolt', 107, 'a1b2c3d4-5678-9012-3456-789abcde0107' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Water / Plumbing', 'fa-solid fa-faucet-drip', 108, 'a1b2c3d4-5678-9012-3456-789abcde0108' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Trowel / Masonry', 'fa-solid fa-trowel', 109, 'a1b2c3d4-5678-9012-3456-789abcde0109' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Bucket', 'fa-solid fa-bucket', 110, 'a1b2c3d4-5678-9012-3456-789abcde0110' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Doctor', 'fa-solid fa-user-doctor', 200, 'a1b2c3d4-5678-9012-3456-789abcde0200' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Nurse', 'fa-solid fa-user-nurse', 201, 'a1b2c3d4-5678-9012-3456-789abcde0201' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Stethoscope', 'fa-solid fa-stethoscope', 202, 'a1b2c3d4-5678-9012-3456-789abcde0202' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Syringe / Vaccine', 'fa-solid fa-syringe', 203, 'a1b2c3d4-5678-9012-3456-789abcde0203' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'First Aid', 'fa-solid fa-kit-medical', 204, 'a1b2c3d4-5678-9012-3456-789abcde0204' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Pills / Medication', 'fa-solid fa-pills', 205, 'a1b2c3d4-5678-9012-3456-789abcde0205' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Hospital', 'fa-solid fa-hospital', 206, 'a1b2c3d4-5678-9012-3456-789abcde0206' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Wheelchair / Accessibility', 'fa-solid fa-wheelchair', 207, 'a1b2c3d4-5678-9012-3456-789abcde0207' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Heart / Vitals', 'fa-solid fa-heart-pulse', 208, 'a1b2c3d4-5678-9012-3456-789abcde0208' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Box / Package', 'fa-solid fa-box', 300, 'a1b2c3d4-5678-9012-3456-789abcde0300' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Pallet', 'fa-solid fa-pallet', 301, 'a1b2c3d4-5678-9012-3456-789abcde0301' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Warehouse', 'fa-solid fa-warehouse', 302, 'a1b2c3d4-5678-9012-3456-789abcde0302' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Map Pin', 'fa-solid fa-map-pin', 303, 'a1b2c3d4-5678-9012-3456-789abcde0303' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Route', 'fa-solid fa-route', 304, 'a1b2c3d4-5678-9012-3456-789abcde0304' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Ship / Marine', 'fa-solid fa-ship', 305, 'a1b2c3d4-5678-9012-3456-789abcde0305' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Plane / Air', 'fa-solid fa-plane', 306, 'a1b2c3d4-5678-9012-3456-789abcde0306' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Building / Office', 'fa-solid fa-building', 400, 'a1b2c3d4-5678-9012-3456-789abcde0400' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Money / Finance', 'fa-solid fa-money-bill-wave', 401, 'a1b2c3d4-5678-9012-3456-789abcde0401' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Credit Card', 'fa-solid fa-credit-card', 402, 'a1b2c3d4-5678-9012-3456-789abcde0402' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Contract / Document', 'fa-solid fa-file-contract', 403, 'a1b2c3d4-5678-9012-3456-789abcde0403' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Signature', 'fa-solid fa-file-signature', 404, 'a1b2c3d4-5678-9012-3456-789abcde0404' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Clipboard / Checklist', 'fa-solid fa-clipboard-list', 405, 'a1b2c3d4-5678-9012-3456-789abcde0405' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Chart / Analytics', 'fa-solid fa-chart-line', 406, 'a1b2c3d4-5678-9012-3456-789abcde0406' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Phone', 'fa-solid fa-phone', 500, 'a1b2c3d4-5678-9012-3456-789abcde0500' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Laptop', 'fa-solid fa-laptop', 501, 'a1b2c3d4-5678-9012-3456-789abcde0501' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Server / Database', 'fa-solid fa-server', 502, 'a1b2c3d4-5678-9012-3456-789abcde0502' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Wifi', 'fa-solid fa-wifi', 503, 'a1b2c3d4-5678-9012-3456-789abcde0503' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Check / Success', 'fa-solid fa-check', 600, 'a1b2c3d4-5678-9012-3456-789abcde0600' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'X / Cancel', 'fa-solid fa-xmark', 601, 'a1b2c3d4-5678-9012-3456-789abcde0601' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Ban / Blocked', 'fa-solid fa-ban', 602, 'a1b2c3d4-5678-9012-3456-789abcde0602' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Clock / Time', 'fa-solid fa-clock', 603, 'a1b2c3d4-5678-9012-3456-789abcde0603' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Hourglass / Waiting', 'fa-solid fa-hourglass-half', 604, 'a1b2c3d4-5678-9012-3456-789abcde0604' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Lock / Security', 'fa-solid fa-lock', 605, 'a1b2c3d4-5678-9012-3456-789abcde0605' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Trash / Delete', 'fa-solid fa-trash', 606, 'a1b2c3d4-5678-9012-3456-789abcde0606' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Sun / Day', 'fa-solid fa-sun', 700, 'a1b2c3d4-5678-9012-3456-789abcde0700' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Cloud', 'fa-solid fa-cloud', 701, 'a1b2c3d4-5678-9012-3456-789abcde0701' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Tree / Landscape', 'fa-solid fa-tree', 702, 'a1b2c3d4-5678-9012-3456-789abcde0702' )
GO

INSERT INTO [Scheduler].[Icon] ( [name], [fontAwesomeCode], [sequence], [objectGuid] ) VALUES  ( 'Default', 'fa-solid fa-circle', 999, 'a1b2c3d4-5678-9012-3456-789abcde0999' )
GO


-- The master list of salutations
CREATE TABLE [Scheduler].[Salutation]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the Salutation table's name field.
CREATE INDEX [I_Salutation_name] ON [Scheduler].[Salutation] ([name])
GO

-- Index on the Salutation table's active field.
CREATE INDEX [I_Salutation_active] ON [Scheduler].[Salutation] ([active])
GO

-- Index on the Salutation table's deleted field.
CREATE INDEX [I_Salutation_deleted] ON [Scheduler].[Salutation] ([deleted])
GO

INSERT INTO [Scheduler].[Salutation] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Mr.', 'Mister', 1, '0e2c9a70-3a90-49f7-9f0a-539fb232a667' )
GO

INSERT INTO [Scheduler].[Salutation] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Mrs.', 'Mrs.', 2, '738abc0a-c637-4d45-89a1-4efb5da4e1d6' )
GO

INSERT INTO [Scheduler].[Salutation] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Ms.', 'Ms.', 3, 'e4f9cfe6-c9dc-44a4-8977-67a8e90f94f8' )
GO

INSERT INTO [Scheduler].[Salutation] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Dr.', 'Doctor', 4, '67be6b22-591f-4b7c-8366-bc3e7304ec90' )
GO

INSERT INTO [Scheduler].[Salutation] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Prof.', 'Professor', 5, '8334e778-b326-4313-8891-c84cf9067d4f' )
GO

INSERT INTO [Scheduler].[Salutation] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Rev.', 'Reverend', 6, 'f27ca1ef-1d00-4d03-9ccd-79a2f97cb2e6' )
GO

INSERT INTO [Scheduler].[Salutation] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( '', 'No Salutation', 7, 'df674e7a-16d8-4e75-bb2b-2a965e1725f1' )
GO


-- Tenant specific master list of resource categories.
CREATE TABLE [Scheduler].[ResourceType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[isBillable] BIT NULL DEFAULT 0,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ResourceType_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_ResourceType_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the ResourceType table's tenantGuid and name fields.
)
GO

-- Index on the ResourceType table's tenantGuid field.
CREATE INDEX [I_ResourceType_tenantGuid] ON [Scheduler].[ResourceType] ([tenantGuid])
GO

-- Index on the ResourceType table's tenantGuid,name fields.
CREATE INDEX [I_ResourceType_tenantGuid_name] ON [Scheduler].[ResourceType] ([tenantGuid], [name])
GO

-- Index on the ResourceType table's tenantGuid,iconId fields.
CREATE INDEX [I_ResourceType_tenantGuid_iconId] ON [Scheduler].[ResourceType] ([tenantGuid], [iconId])
GO

-- Index on the ResourceType table's tenantGuid,active fields.
CREATE INDEX [I_ResourceType_tenantGuid_active] ON [Scheduler].[ResourceType] ([tenantGuid], [active])
GO

-- Index on the ResourceType table's tenantGuid,deleted fields.
CREATE INDEX [I_ResourceType_tenantGuid_deleted] ON [Scheduler].[ResourceType] ([tenantGuid], [deleted])
GO

INSERT INTO [Scheduler].[ResourceType] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Person', 'Human resource (operator, supervisor, engineer, etc.)', 1, 'a1b2c3d4-5678-9012-3456-789abcde0001' )
GO

INSERT INTO [Scheduler].[ResourceType] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Equipment', 'Heavy machinery (rollers, excavators, loaders, etc.)', 2, 'a1b2c3d4-5678-9012-3456-789abcde0002' )
GO

INSERT INTO [Scheduler].[ResourceType] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Vehicle', 'Trucks, service vehicles, etc.', 3, 'a1b2c3d4-5678-9012-3456-789abcde0003' )
GO

INSERT INTO [Scheduler].[ResourceType] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Tool', 'Smaller tools or shared items', 4, 'a1b2c3d4-5678-9012-3456-789abcde0004' )
GO

INSERT INTO [Scheduler].[ResourceType] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Room', 'Meeting rooms, office spaces, etc.', 5, 'a1b2c3d4-5678-9012-3456-789abcde0005' )
GO


-- List of priority values - Tenant configurable for flexibilty
CREATE TABLE [Scheduler].[Priority]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[iconId] INT NULL,		-- Link to the Icon table.
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Priority_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_Priority_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Priority table's tenantGuid and name fields.
)
GO

-- Index on the Priority table's tenantGuid field.
CREATE INDEX [I_Priority_tenantGuid] ON [Scheduler].[Priority] ([tenantGuid])
GO

-- Index on the Priority table's tenantGuid,name fields.
CREATE INDEX [I_Priority_tenantGuid_name] ON [Scheduler].[Priority] ([tenantGuid], [name])
GO

-- Index on the Priority table's tenantGuid,iconId fields.
CREATE INDEX [I_Priority_tenantGuid_iconId] ON [Scheduler].[Priority] ([tenantGuid], [iconId])
GO

-- Index on the Priority table's tenantGuid,active fields.
CREATE INDEX [I_Priority_tenantGuid_active] ON [Scheduler].[Priority] ([tenantGuid], [active])
GO

-- Index on the Priority table's tenantGuid,deleted fields.
CREATE INDEX [I_Priority_tenantGuid_deleted] ON [Scheduler].[Priority] ([tenantGuid], [deleted])
GO

INSERT INTO [Scheduler].[Priority] ( [tenantGuid], [name], [description], [color], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'High', 'High Priority', '#FF0F0F', 1, 'bcde74de-3f66-4c62-ad38-a5941871cea2' )
GO

INSERT INTO [Scheduler].[Priority] ( [tenantGuid], [name], [description], [color], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Medium', 'Medium Priority', '#E8E8E8', 2, 'f2058cd4-aecf-4e28-b40c-6c181e67c0f4' )
GO

INSERT INTO [Scheduler].[Priority] ( [tenantGuid], [name], [description], [color], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Low', 'Low Priority', '#E8E8E8', 3, '25e075c3-a513-4a45-9fbc-106afc890821' )
GO


-- List of standard contact methods
CREATE TABLE [Scheduler].[ContactMethod]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[iconId] INT NULL,		-- Link to the Icon table.
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ContactMethod_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id])		-- Foreign key to the Icon table.
)
GO

-- Index on the ContactMethod table's name field.
CREATE INDEX [I_ContactMethod_name] ON [Scheduler].[ContactMethod] ([name])
GO

-- Index on the ContactMethod table's iconId field.
CREATE INDEX [I_ContactMethod_iconId] ON [Scheduler].[ContactMethod] ([iconId])
GO

-- Index on the ContactMethod table's active field.
CREATE INDEX [I_ContactMethod_active] ON [Scheduler].[ContactMethod] ([active])
GO

-- Index on the ContactMethod table's deleted field.
CREATE INDEX [I_ContactMethod_deleted] ON [Scheduler].[ContactMethod] ([deleted])
GO

INSERT INTO [Scheduler].[ContactMethod] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Mobile Phone', 'Mobile Phone', 1, 'c8e56688-e480-426d-b49d-f7f7e7c1802c' )
GO

INSERT INTO [Scheduler].[ContactMethod] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Phone', 'Phone', 2, 'df379702-6082-4084-bf4e-f722893f33a2' )
GO

INSERT INTO [Scheduler].[ContactMethod] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Email', 'Email', 3, '1fbea244-8312-4d8c-8218-b4b5d0788510' )
GO

INSERT INTO [Scheduler].[ContactMethod] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Text', 'Text', 4, '9ad23e9b-76fe-4e35-9c9b-8a53b9037cce' )
GO

INSERT INTO [Scheduler].[ContactMethod] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Video Call', 'Video Call', 5, 'f89b6825-fd15-419f-baef-ec6c9ae61127' )
GO

INSERT INTO [Scheduler].[ContactMethod] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'In Person', 'In Person', 6, '91c03a84-0772-443b-8eba-e6810ec4912a' )
GO


-- The rate types
CREATE TABLE [Scheduler].[RateType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_RateType_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the RateType table's tenantGuid and name fields.
)
GO

-- Index on the RateType table's tenantGuid field.
CREATE INDEX [I_RateType_tenantGuid] ON [Scheduler].[RateType] ([tenantGuid])
GO

-- Index on the RateType table's tenantGuid,name fields.
CREATE INDEX [I_RateType_tenantGuid_name] ON [Scheduler].[RateType] ([tenantGuid], [name])
GO

-- Index on the RateType table's tenantGuid,active fields.
CREATE INDEX [I_RateType_tenantGuid_active] ON [Scheduler].[RateType] ([tenantGuid], [active])
GO

-- Index on the RateType table's tenantGuid,deleted fields.
CREATE INDEX [I_RateType_tenantGuid_deleted] ON [Scheduler].[RateType] ([tenantGuid], [deleted])
GO

INSERT INTO [Scheduler].[RateType] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Standard', 'Standard Billing Rate', 1, 'e0d3b9b8-2b93-45e1-8de2-dba9603c38b9' )
GO

INSERT INTO [Scheduler].[RateType] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Overtime', 'Overtime Billing Rate', 2, '84897121-1587-4930-9d8c-4389ac0d222f' )
GO

INSERT INTO [Scheduler].[RateType] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'DoubleTime', 'DoubleTime Billing Rate', 3, 'fad24a49-924d-403f-a013-114ceb13ae27' )
GO

INSERT INTO [Scheduler].[RateType] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Travel', 'Travel Billing Rate', 4, 'fa0f7edd-8443-419d-9aea-229a2e61730f' )
GO


-- Master list of interaction types.
CREATE TABLE [Scheduler].[InteractionType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_InteractionType_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id])		-- Foreign key to the Icon table.
)
GO

-- Index on the InteractionType table's name field.
CREATE INDEX [I_InteractionType_name] ON [Scheduler].[InteractionType] ([name])
GO

-- Index on the InteractionType table's iconId field.
CREATE INDEX [I_InteractionType_iconId] ON [Scheduler].[InteractionType] ([iconId])
GO

-- Index on the InteractionType table's active field.
CREATE INDEX [I_InteractionType_active] ON [Scheduler].[InteractionType] ([active])
GO

-- Index on the InteractionType table's deleted field.
CREATE INDEX [I_InteractionType_deleted] ON [Scheduler].[InteractionType] ([deleted])
GO

INSERT INTO [Scheduler].[InteractionType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'In Person', 'In Person meeting', 1, '4a503ab2-a58e-403a-a400-027985773cb6' )
GO

INSERT INTO [Scheduler].[InteractionType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Phone Call', 'Phone Call', 2, '16988bb1-54d3-4bb9-b6a7-bfadface573d' )
GO

INSERT INTO [Scheduler].[InteractionType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Video Call', 'Video Call', 3, '337a67d5-53b8-4a67-ac4b-97818d0b0fa4' )
GO

INSERT INTO [Scheduler].[InteractionType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Text Message', 'Text Message', 4, '10ea655e-07ae-46cf-bbf3-076c3643e16b' )
GO

INSERT INTO [Scheduler].[InteractionType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Email Message', 'Email Message', 5, 'eeb14f23-857e-416e-80a0-9a2f82b57bf7' )
GO


-- The currencies
CREATE TABLE [Scheduler].[Currency]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[code] NVARCHAR(10) NOT NULL,
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[isDefault] BIT NOT NULL DEFAULT 0,		-- Default currency for tenant.
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_Currency_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) ,		-- Uniqueness enforced on the Currency table's tenantGuid and name fields.
	CONSTRAINT [UC_Currency_tenantGuid_code] UNIQUE ( [tenantGuid], [code]) 		-- Uniqueness enforced on the Currency table's tenantGuid and code fields.
)
GO

-- Index on the Currency table's tenantGuid field.
CREATE INDEX [I_Currency_tenantGuid] ON [Scheduler].[Currency] ([tenantGuid])
GO

-- Index on the Currency table's tenantGuid,name fields.
CREATE INDEX [I_Currency_tenantGuid_name] ON [Scheduler].[Currency] ([tenantGuid], [name])
GO

-- Index on the Currency table's tenantGuid,active fields.
CREATE INDEX [I_Currency_tenantGuid_active] ON [Scheduler].[Currency] ([tenantGuid], [active])
GO

-- Index on the Currency table's tenantGuid,deleted fields.
CREATE INDEX [I_Currency_tenantGuid_deleted] ON [Scheduler].[Currency] ([tenantGuid], [deleted])
GO

INSERT INTO [Scheduler].[Currency] ( [tenantGuid], [name], [description], [code], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'US Dollas', 'United States Dollars', 'USD', 1, '5d460ce9-4cf5-41c3-ab9d-9ef104b0a276' )
GO

INSERT INTO [Scheduler].[Currency] ( [tenantGuid], [name], [description], [code], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Canadian', 'Canadian Dollars', 'CAD', 2, 'c6673662-f1c9-4aee-b5df-867500cb8545' )
GO


/*
====================================================================================================
 CHARGE MASTER (Like Epic CDM)
 Master list of chargeable items (revenue or expenses). e.g., "Site Visit Fee" (revenue), "Travel Expense" (expense).
 Tied to RateType for billing context.
 ====================================================================================================
*/
CREATE TABLE [Scheduler].[ChargeType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[externalId] NVARCHAR(100) NULL,
	[isRevenue] BIT NOT NULL DEFAULT 1,		-- True = Revenue (billable), False = Expense (cost)
	[isTaxable] BIT NULL DEFAULT 0,
	[defaultAmount] MONEY NULL,		-- Optional default value for auto-drops
	[defaultDescription] NVARCHAR(500) NULL,		-- sometimes auto-dropped charges need a note (e.g., "Travel to site – 45 km").
	[rateTypeId] INT NULL,		-- Link to RateType (e.g., 'Standard', 'Overtime')
	[currencyId] INT NOT NULL,		-- Link to the Currency table.
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ChargeType_RateType_rateTypeId] FOREIGN KEY ([rateTypeId]) REFERENCES [Scheduler].[RateType] ([id]),		-- Foreign key to the RateType table.
	CONSTRAINT [FK_ChargeType_Currency_currencyId] FOREIGN KEY ([currencyId]) REFERENCES [Scheduler].[Currency] ([id]),		-- Foreign key to the Currency table.
	CONSTRAINT [UC_ChargeType_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the ChargeType table's tenantGuid and name fields.
)
GO

-- Index on the ChargeType table's tenantGuid field.
CREATE INDEX [I_ChargeType_tenantGuid] ON [Scheduler].[ChargeType] ([tenantGuid])
GO

-- Index on the ChargeType table's tenantGuid,name fields.
CREATE INDEX [I_ChargeType_tenantGuid_name] ON [Scheduler].[ChargeType] ([tenantGuid], [name])
GO

-- Index on the ChargeType table's tenantGuid,externalId fields.
CREATE INDEX [I_ChargeType_tenantGuid_externalId] ON [Scheduler].[ChargeType] ([tenantGuid], [externalId])
GO

-- Index on the ChargeType table's tenantGuid,rateTypeId fields.
CREATE INDEX [I_ChargeType_tenantGuid_rateTypeId] ON [Scheduler].[ChargeType] ([tenantGuid], [rateTypeId])
GO

-- Index on the ChargeType table's tenantGuid,currencyId fields.
CREATE INDEX [I_ChargeType_tenantGuid_currencyId] ON [Scheduler].[ChargeType] ([tenantGuid], [currencyId])
GO

-- Index on the ChargeType table's tenantGuid,active fields.
CREATE INDEX [I_ChargeType_tenantGuid_active] ON [Scheduler].[ChargeType] ([tenantGuid], [active])
GO

-- Index on the ChargeType table's tenantGuid,deleted fields.
CREATE INDEX [I_ChargeType_tenantGuid_deleted] ON [Scheduler].[ChargeType] ([tenantGuid], [deleted])
GO


-- The change history for records from the ChargeType table.
CREATE TABLE [Scheduler].[ChargeTypeChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[chargeTypeId] INT NOT NULL,		-- Link to the ChargeType table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ChargeTypeChangeHistory_ChargeType_chargeTypeId] FOREIGN KEY ([chargeTypeId]) REFERENCES [Scheduler].[ChargeType] ([id])		-- Foreign key to the ChargeType table.
)
GO

-- Index on the ChargeTypeChangeHistory table's tenantGuid field.
CREATE INDEX [I_ChargeTypeChangeHistory_tenantGuid] ON [Scheduler].[ChargeTypeChangeHistory] ([tenantGuid])
GO

-- Index on the ChargeTypeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ChargeTypeChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ChargeTypeChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ChargeTypeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ChargeTypeChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ChargeTypeChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ChargeTypeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ChargeTypeChangeHistory_tenantGuid_userId] ON [Scheduler].[ChargeTypeChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ChargeTypeChangeHistory table's tenantGuid,chargeTypeId fields.
CREATE INDEX [I_ChargeTypeChangeHistory_tenantGuid_chargeTypeId] ON [Scheduler].[ChargeTypeChangeHistory] ([tenantGuid], [chargeTypeId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Tenant specific master list of tags.
CREATE TABLE [Scheduler].[Tag]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[isSystem] BIT NULL,		-- To mark as system tag for protected / special handling.  For things like 'deceased'.
	[priorityId] INT NULL,		-- Link to the Priority table.
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Tag_Priority_priorityId] FOREIGN KEY ([priorityId]) REFERENCES [Scheduler].[Priority] ([id]),		-- Foreign key to the Priority table.
	CONSTRAINT [FK_Tag_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_Tag_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Tag table's tenantGuid and name fields.
)
GO

-- Index on the Tag table's tenantGuid field.
CREATE INDEX [I_Tag_tenantGuid] ON [Scheduler].[Tag] ([tenantGuid])
GO

-- Index on the Tag table's tenantGuid,name fields.
CREATE INDEX [I_Tag_tenantGuid_name] ON [Scheduler].[Tag] ([tenantGuid], [name])
GO

-- Index on the Tag table's tenantGuid,priorityId fields.
CREATE INDEX [I_Tag_tenantGuid_priorityId] ON [Scheduler].[Tag] ([tenantGuid], [priorityId])
GO

-- Index on the Tag table's tenantGuid,iconId fields.
CREATE INDEX [I_Tag_tenantGuid_iconId] ON [Scheduler].[Tag] ([tenantGuid], [iconId])
GO

-- Index on the Tag table's tenantGuid,active fields.
CREATE INDEX [I_Tag_tenantGuid_active] ON [Scheduler].[Tag] ([tenantGuid], [active])
GO

-- Index on the Tag table's tenantGuid,deleted fields.
CREATE INDEX [I_Tag_tenantGuid_deleted] ON [Scheduler].[Tag] ([tenantGuid], [deleted])
GO

INSERT INTO [Scheduler].[Tag] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Person', 'Human resource (operator, supervisor, engineer, etc.)', 1, 'a1b2c3d4-5678-9012-3456-789abcde0001' )
GO


-- Time zones master data list.
CREATE TABLE [Scheduler].[TimeZone]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[ianaTimeZone] NVARCHAR(50) NOT NULL,		-- e.g., 'America/St.John's' (official IANA name)
	[abbreviation] NVARCHAR(50) NOT NULL,
	[abbreviationDaylightSavings] NVARCHAR(50) NOT NULL,
	[supportsDaylightSavings] BIT NOT NULL DEFAULT 1,
	[standardUTCOffsetHours] REAL NOT NULL,		-- The standard offset hours from UTC for this time zone.
	[dstUTCOffsetHours] REAL NOT NULL,		-- Use the same value here as the standard one for time zones that do not support DST
	[sequence] INT NULL,		-- For sorting in drop downs
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the TimeZone table's name field.
CREATE INDEX [I_TimeZone_name] ON [Scheduler].[TimeZone] ([name])
GO

-- Index on the TimeZone table's active field.
CREATE INDEX [I_TimeZone_active] ON [Scheduler].[TimeZone] ([active])
GO

-- Index on the TimeZone table's deleted field.
CREATE INDEX [I_TimeZone_deleted] ON [Scheduler].[TimeZone] ([deleted])
GO

INSERT INTO [Scheduler].[TimeZone] ( [name], [abbreviation], [abbreviationDaylightSavings], [supportsDaylightSavings], [standardUTCOffsetHours], [dstUTCOffsetHours], [description], [ianaTimeZone], [sequence], [objectGuid] ) VALUES  ( 'Newfoundland Standard Time', 'NST', 'NDT', 1, -3.5, -2.5, 'Newfoundland and southeastern Labrador (Canada)', 'America/St_Johns', 10, '27129170-81b3-4c70-a7d4-0378dce8426f' )
GO

INSERT INTO [Scheduler].[TimeZone] ( [name], [abbreviation], [abbreviationDaylightSavings], [supportsDaylightSavings], [standardUTCOffsetHours], [dstUTCOffsetHours], [description], [ianaTimeZone], [sequence], [objectGuid] ) VALUES  ( 'Atlantic Standard Time', 'AST', 'ADT', 1, -4, -3, 'Atlantic Canada (Nova Scotia, New Brunswick, PEI, parts of Quebec)', 'America/Halifax', 20, '8f3d2a1b-4c5e-4d8f-9a2b-6e7f1c3d9a0b' )
GO

INSERT INTO [Scheduler].[TimeZone] ( [name], [abbreviation], [abbreviationDaylightSavings], [supportsDaylightSavings], [standardUTCOffsetHours], [dstUTCOffsetHours], [description], [ianaTimeZone], [sequence], [objectGuid] ) VALUES  ( 'Atlantic Standard Time (no DST)', 'AST', 'AST', 0, -4, -4, 'Puerto Rico, US Virgin Islands, Dominican Republic', 'America/Puerto_Rico', 30, '648d1e27-51b2-4e9b-ae9e-06dd856022e8' )
GO

INSERT INTO [Scheduler].[TimeZone] ( [name], [abbreviation], [abbreviationDaylightSavings], [supportsDaylightSavings], [standardUTCOffsetHours], [dstUTCOffsetHours], [description], [ianaTimeZone], [sequence], [objectGuid] ) VALUES  ( 'Eastern Standard Time', 'EST', 'EDT', 1, -5, -4, 'Eastern United States, Eastern Canada (Ontario, Quebec)', 'America/New_York', 40, 'c4e5f6a7-8b9c-4d0e-1f2a-3b4c5d6e7f8a' )
GO

INSERT INTO [Scheduler].[TimeZone] ( [name], [abbreviation], [abbreviationDaylightSavings], [supportsDaylightSavings], [standardUTCOffsetHours], [dstUTCOffsetHours], [description], [ianaTimeZone], [sequence], [objectGuid] ) VALUES  ( 'Central Standard Time', 'CST', 'CDT', 1, -6, -5, 'Central United States, Central Canada, Mexico (most), Central America', 'America/Chicago', 50, 'd5e6f7a8-9c0d-4e1f-2a3b-4c5d6e7f8a9b' )
GO

INSERT INTO [Scheduler].[TimeZone] ( [name], [abbreviation], [abbreviationDaylightSavings], [supportsDaylightSavings], [standardUTCOffsetHours], [dstUTCOffsetHours], [description], [ianaTimeZone], [sequence], [objectGuid] ) VALUES  ( 'Central Standard Time (no DST)', 'CST', 'CST', 0, -6, -6, 'Central America (Guatemala, Costa Rica, Nicaragua, etc.)', 'America/Guatemala', 60, 'f2b768f4-6162-4f65-8eb8-6ae1c5a9dc88' )
GO

INSERT INTO [Scheduler].[TimeZone] ( [name], [abbreviation], [abbreviationDaylightSavings], [supportsDaylightSavings], [standardUTCOffsetHours], [dstUTCOffsetHours], [description], [ianaTimeZone], [sequence], [objectGuid] ) VALUES  ( 'Mountain Standard Time', 'MST', 'MDT', 1, -7, -6, 'Mountain United States (except Arizona), Western Canada', 'America/Denver', 70, 'e6f7a8b9-0d1e-4f2a-3b4c-5d6e7f8a9b0c' )
GO

INSERT INTO [Scheduler].[TimeZone] ( [name], [abbreviation], [abbreviationDaylightSavings], [supportsDaylightSavings], [standardUTCOffsetHours], [dstUTCOffsetHours], [description], [ianaTimeZone], [sequence], [objectGuid] ) VALUES  ( 'Arizona Time', 'MST', 'MST', 0, -7, -7, 'Arizona (United States) — does not observe Daylight Saving Time', 'America/Phoenix', 80, 'f7a8b9c0-1e2f-4a3b-5c6d-7e8f9a0b1c2d' )
GO

INSERT INTO [Scheduler].[TimeZone] ( [name], [abbreviation], [abbreviationDaylightSavings], [supportsDaylightSavings], [standardUTCOffsetHours], [dstUTCOffsetHours], [description], [ianaTimeZone], [sequence], [objectGuid] ) VALUES  ( 'Pacific Standard Time', 'PST', 'PDT', 1, -8, -7, 'Western United States, Western Canada (British Columbia)', 'America/Los_Angeles', 90, 'a8b9c0d1-2f3a-4b5c-6d7e-8f9a0b1c2d3e' )
GO

INSERT INTO [Scheduler].[TimeZone] ( [name], [abbreviation], [abbreviationDaylightSavings], [supportsDaylightSavings], [standardUTCOffsetHours], [dstUTCOffsetHours], [description], [ianaTimeZone], [sequence], [objectGuid] ) VALUES  ( 'Alaska Standard Time', 'AKST', 'AKDT', 1, -9, -8, 'Alaska (United States)', 'America/Anchorage', 100, 'b9c0d1e2-3a4b-5c6d-7e8f-9a0b1c2d3e4f' )
GO

INSERT INTO [Scheduler].[TimeZone] ( [name], [abbreviation], [abbreviationDaylightSavings], [supportsDaylightSavings], [standardUTCOffsetHours], [dstUTCOffsetHours], [description], [ianaTimeZone], [sequence], [objectGuid] ) VALUES  ( 'Hawaii-Aleutian Standard Time', 'HST', 'HST', 0, -10, -10, 'Hawaii and Aleutian Islands (United States) — no Daylight Saving Time', 'Pacific/Honolulu', 110, 'c0d1e2f3-4b5c-6d7e-8f9a-0b1c2d3e4f5a' )
GO


-- The master list of countries
CREATE TABLE [Scheduler].[Country]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[abbreviation] NVARCHAR(10) NOT NULL,
	[postalCodeFormat] NVARCHAR(50) NULL,		-- The human readable postal code format for the country, if applicable.
	[postalCodeRegEx] NVARCHAR(50) NULL,		-- The regular expression pattern for validation of the postal code, if applicable 
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the Country table's name field.
CREATE INDEX [I_Country_name] ON [Scheduler].[Country] ([name])
GO

-- Index on the Country table's active field.
CREATE INDEX [I_Country_active] ON [Scheduler].[Country] ([active])
GO

-- Index on the Country table's deleted field.
CREATE INDEX [I_Country_deleted] ON [Scheduler].[Country] ([deleted])
GO

INSERT INTO [Scheduler].[Country] ( [name], [description], [abbreviation], [sequence], [postalCodeFormat], [postalCodeRegEx], [objectGuid] ) VALUES  ( 'Canada', 'Canada', 'CA', 1, 'A0A 0A0', '^[A-Z]\d[A-Z] ?\d[A-Z]\d$', '5f3f3c1d-9ba8-48cd-ae6d-4f4d8a5c2bcb' )
GO

INSERT INTO [Scheduler].[Country] ( [name], [description], [abbreviation], [sequence], [postalCodeFormat], [postalCodeRegEx], [objectGuid] ) VALUES  ( 'USA', 'United States of America', 'US', 2, 'NNNNN or NNNNN-NNNN', '^\d{5}(-\d{4})?$'')', '9b2b1de3-719f-4c8a-bb2f-6e903d4e74b5' )
GO


-- The master list of states
CREATE TABLE [Scheduler].[StateProvince]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[countryId] INT NOT NULL,		-- Link to the Country table.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[abbreviation] NVARCHAR(10) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_StateProvince_Country_countryId] FOREIGN KEY ([countryId]) REFERENCES [Scheduler].[Country] ([id]),		-- Foreign key to the Country table.
	CONSTRAINT [UC_StateProvince_name_countryId] UNIQUE ( [name], [countryId]) ,		-- Uniqueness enforced on the StateProvince table's name and countryId fields.
	CONSTRAINT [UC_StateProvince_abbreviation_countryId] UNIQUE ( [abbreviation], [countryId]) 		-- Uniqueness enforced on the StateProvince table's abbreviation and countryId fields.
)
GO

-- Index on the StateProvince table's countryId field.
CREATE INDEX [I_StateProvince_countryId] ON [Scheduler].[StateProvince] ([countryId])
GO

-- Index on the StateProvince table's name field.
CREATE INDEX [I_StateProvince_name] ON [Scheduler].[StateProvince] ([name])
GO

-- Index on the StateProvince table's active field.
CREATE INDEX [I_StateProvince_active] ON [Scheduler].[StateProvince] ([active])
GO

-- Index on the StateProvince table's deleted field.
CREATE INDEX [I_StateProvince_deleted] ON [Scheduler].[StateProvince] ([deleted])
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Newfoundland', 'Newfoundland and Labrador', 'NL', 1, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'Canada' ), 'a1eecf09-7362-42be-b5d1-90284e1c3075' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Ontario', 'Ontario', 'ON', 2, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'Canada' ), 'b2e5d8f1-897b-4563-8131-7eeb6d0c80a4' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Alberta', 'Alberta', 'AB', 3, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'Canada' ), 'c3fe34bc-9601-474f-b99f-55c7a9c71738' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'British Columbia', 'British Columbia', 'BC', 4, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'Canada' ), 'd4b7ab65-8fc6-4746-b9f6-e9bcf5b8cf91' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Manitoba', 'Manitoba', 'MB', 5, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'Canada' ), 'e5a8be2d-7a4e-43e5-83d5-d2cf77282c0d' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'New Brunswick', 'New Brunswick', 'NB', 6, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'Canada' ), 'f6f2a6f4-3963-4539-a54f-bd7ed0be2b3b' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Northwest Territories', 'Northwest Territories', 'NT', 7, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'Canada' ), '078f1d72-20a4-4b78-8b2f-9c6d6e69f29a' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Nova Scotia', 'Nova Scotia', 'NS', 8, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'Canada' ), '179fbbf1-b651-4b7a-b17e-b65d6aeb7795' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Nunavut', 'Nunavut', 'NU', 9, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'Canada' ), '28a1b2ed-7554-48b5-b7f0-b0f2bc3f0a8f' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Prince Edward Island', 'Prince Edward Island', 'PE', 10, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'Canada' ), '39b8c1de-dc77-4b3b-b0f6-e41b6a557809' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Quebec', 'Quebec', 'QC', 11, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'Canada' ), '4b9e6f87-b15f-4858-b739-dc23714b83b7' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Saskatchewan', 'Saskatchewan', 'SK', 12, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'Canada' ), '5c12c0ea-23a0-43a3-a8c9-15d032de5643' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Yukon', 'Yukon', 'YT', 13, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'Canada' ), '6d1a81eb-fc4a-4c44-9e5a-079c32074749' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Other', 'Other', 'Other', 99, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'Canada' ), '7e2f5bce-c2b0-4012-84b4-c982d78dce3e' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Alabama', 'Alabama', 'AL', 1, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'd003a92b-6cec-4d49-8baa-6b4fd8fc2f92' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Alaska', 'Alaska', 'AK', 2, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '3aff430d-2752-4d91-ae08-656934438dac' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Arizona', 'Arizona', 'AZ', 3, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '5c4ec86a-472a-4d6c-a278-b5e21352b644' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Arkansas', 'Arkansas', 'AR', 4, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'cd58100a-e5b6-4fc0-a251-2e1a22e66836' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'California', 'California', 'CA', 5, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '36a7adaa-f35a-40ca-8f24-231a3ebd1ad8' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Colorado', 'Colorado', 'CO', 6, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '0210922a-348c-4181-a9e0-6054dd7bc655' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Connecticut', 'Connecticut', 'CT', 7, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '4040cc1a-e6f4-454d-93aa-162c74fe50c6' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Delaware', 'Delaware', 'DE', 8, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '01a5dc36-c285-4216-9fb6-811d5b8e8b48' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Florida', 'Florida', 'FL', 9, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '5e0bb9f6-b6ca-4b42-832f-7c41a570fae4' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Georgia', 'Georgia', 'GA', 10, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'c57ffded-5284-471a-898c-f4969f611dd7' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Hawaii', 'Hawaii', 'HI', 11, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '9fcaa230-ded7-47a8-8a3e-dd1a756ca363' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Idaho', 'Idaho', 'ID', 12, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '796c444b-7513-4823-ab11-94dae65dc0e5' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Illinois', 'Illinois', 'IL', 13, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'd2a28ab4-09c1-437b-b70c-1424543c4128' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Indiana', 'Indiana', 'IN', 14, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '3d9f6c85-6515-4147-adec-ab7dc6e95eab' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Iowa', 'Iowa', 'IA', 15, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'cecfa624-ba4a-473e-a0fc-e91b007beab7' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Kansas', 'Kansas', 'KS', 16, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'b155c44b-c3dd-4884-b715-71ab38596e00' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Kentucky', 'Kentucky', 'KY', 17, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '152ad250-6174-45f7-a947-6c6c14a56494' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Louisiana', 'Louisiana', 'LA', 18, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'c9260be6-9840-420c-acf4-7d82ef937160' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Maine', 'Maine', 'ME', 19, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '97b79ed1-f1b0-44ef-bdd0-71caccd1465d' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Maryland', 'Maryland', 'MD', 20, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'c0cf2ae1-ed20-4845-b860-ff008427359b' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Massachusetts', 'Massachusetts', 'MA', 21, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '7801225d-a996-40cb-888e-49645ffdbb06' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Michigan', 'Michigan', 'MI', 22, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'f9324013-0a60-43ea-b672-6999a821cb15' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Minnesota', 'Minnesota', 'MN', 23, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'f43770fd-ceaf-4646-9943-08be6268c045' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Mississippi', 'Mississippi', 'MS', 24, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'b193e806-5a5e-4d46-936c-b4b3a28e59c5' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Missouri', 'Missouri', 'MO', 25, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'd57e6019-c221-465e-b92e-0b8d3da0ff80' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Montana', 'Montana', 'MT', 26, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '2f10e38c-b937-459f-89d0-60f552687c46' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Nebraska', 'Nebraska', 'NE', 27, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '85ad29eb-f1c6-4862-82bd-d4c91eea2838' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Nevada', 'Nevada', 'NV', 28, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '95ad29eb-f1c6-4862-82bd-d4c91eea2887' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'New Hampshire', 'New Hampshire', 'NH', 29, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '5e5d5651-a186-4cc1-b61a-f22c9d530e6f' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'New Jersey', 'New Jersey', 'NJ', 30, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'ee4ab53d-dab1-4ba7-8363-ed616a779567' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'New Mexico', 'New Mexico', 'NM', 31, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'be168b30-72bd-4942-b187-deff865a5e6a' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'New York', 'New York', 'NY', 32, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '7c93f785-a069-4298-93dc-2ef5e00fd0a8' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'North Carolina', 'North Carolina', 'NC', 33, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'af2af206-9f3c-419f-9731-9fc90f1bda1b' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'North Dakota', 'North Dakota', 'ND', 34, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '3a8d0072-1457-4923-bf19-12b8748098ee' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Ohio', 'Ohio', 'OH', 35, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'd1961e5f-1c25-46ef-9bca-30fe538fe5c9' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Oklahoma', 'Oklahoma', 'OK', 36, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'b2bc6d1b-32b6-4026-b648-70ec7b5063b1' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Oregon', 'Oregon', 'OR', 37, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'fbd6a82b-3f4b-49e0-b5ba-59ec47335c99' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Pennsylvania', 'Pennsylvania', 'PA', 38, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'd9b34153-fb25-403d-a13e-37b2823fbf69' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Rhode Island', 'Rhode Island', 'RI', 39, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'c1c32aa7-af93-4bf1-9acf-9ff591b1b8c5' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'South Carolina', 'South Carolina', 'SC', 40, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '9d050cab-34a0-40eb-8592-2ee2a62e21a1' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'South Dakota', 'South Dakota', 'SD', 41, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'e652bc14-13e0-4405-9feb-6b78dd0790dd' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Tennessee', 'Tennessee', 'TN', 42, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '0d7a100b-792e-46ca-81e0-eaef7e78aec2' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Texas', 'Texas', 'TX', 43, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '5384bf42-c1a8-47c8-998c-85c02838a299' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Utah', 'Utah', 'UT', 44, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '6f4755b9-8a7a-4c52-a8a2-a464de793cbd' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Vermont', 'Vermont', 'VT', 45, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '9dd23ade-bbf4-4d5a-9fd8-199af9005145' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Virginia', 'Virginia', 'VA', 46, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '6071e23d-d660-4801-894e-0ca5783d6a31' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Washington', 'Washington', 'WA', 47, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'cc5b5362-f9fc-406f-927d-d6c4e917f76d' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'West Virginia', 'West Virginia', 'WV', 48, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '06d12574-b3b8-4392-87a1-76a8c42ccf7a' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Wisconsin', 'Wisconsin', 'WI', 49, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'ebf4200d-b4f0-4a62-b2a9-256aab919241' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Wyoming', 'Wyoming', 'WY', 50, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), 'dfff135c-165b-42a9-81f9-a55f8d51c710' )
GO

INSERT INTO [Scheduler].[StateProvince] ( [name], [description], [abbreviation], [sequence], [countryId], [objectGuid] ) VALUES  ( 'Other', 'Other', 'Other', 99, ( SELECT TOP 1 id FROM [Scheduler].[Country] WHERE [name] = 'USA' ), '4ab041c0-9479-4a65-ba56-cbb70d82de75' )
GO


/*
Master list of volunteer lifecycle/status values.
Examples: Prospect, Active, On Leave, Inactive, Not Re-invited.
Used to track engagement level and control visibility/assignment rules.
*/
CREATE TABLE [Scheduler].[VolunteerStatus]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[color] NVARCHAR(10) NULL,		-- Suggested UI color for this status
	[iconId] INT NULL,		-- Optional icon for visual distinction
	[isActive] BIT NULL DEFAULT 1,		-- Whether volunteers in this status are generally schedulable
	[preventsScheduling] BIT NOT NULL DEFAULT 0,		-- Hard block: cannot be assigned to events
	[requiresApproval] BIT NOT NULL DEFAULT 0,		-- New assignments need coordinator approval
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_VolunteerStatus_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id])		-- Foreign key to the Icon table.
)
GO

-- Index on the VolunteerStatus table's name field.
CREATE INDEX [I_VolunteerStatus_name] ON [Scheduler].[VolunteerStatus] ([name])
GO

-- Index on the VolunteerStatus table's iconId field.
CREATE INDEX [I_VolunteerStatus_iconId] ON [Scheduler].[VolunteerStatus] ([iconId])
GO

-- Index on the VolunteerStatus table's active field.
CREATE INDEX [I_VolunteerStatus_active] ON [Scheduler].[VolunteerStatus] ([active])
GO

-- Index on the VolunteerStatus table's deleted field.
CREATE INDEX [I_VolunteerStatus_deleted] ON [Scheduler].[VolunteerStatus] ([deleted])
GO

INSERT INTO [Scheduler].[VolunteerStatus] ( [name], [description], [sequence], [color], [isActive], [preventsScheduling], [objectGuid] ) VALUES  ( 'Prospect / Interested', 'Has expressed interest but not yet onboarded', 10, '#9E9E9E', 0, 1, 'a1111111-2222-3333-4444-555555555001' )
GO

INSERT INTO [Scheduler].[VolunteerStatus] ( [name], [description], [sequence], [color], [isActive], [preventsScheduling], [objectGuid] ) VALUES  ( 'Active', 'Fully onboarded and available for assignments', 20, '#4CAF50', 1, 0, 'a1111111-2222-3333-4444-555555555002' )
GO

INSERT INTO [Scheduler].[VolunteerStatus] ( [name], [description], [sequence], [color], [isActive], [preventsScheduling], [objectGuid] ) VALUES  ( 'On Hiatus / Leave', 'Temporary break (maternity, travel, etc.)', 30, '#FF9800', 0, 1, 'a1111111-2222-3333-4444-555555555003' )
GO

INSERT INTO [Scheduler].[VolunteerStatus] ( [name], [description], [sequence], [color], [isActive], [preventsScheduling], [objectGuid] ) VALUES  ( 'Inactive', 'No longer participating, but record retained', 40, '#757575', 0, 1, 'a1111111-2222-3333-4444-555555555004' )
GO

INSERT INTO [Scheduler].[VolunteerStatus] ( [name], [description], [sequence], [color], [isActive], [preventsScheduling], [objectGuid] ) VALUES  ( 'Not Re-invited', 'Previous issues; do not contact or schedule', 50, '#F44336', 0, 1, 'a1111111-2222-3333-4444-555555555005' )
GO


-- Master list of office types.  Used for categorizing offices.  Not tenant specific
CREATE TABLE [Scheduler].[ContactType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ContactType_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id])		-- Foreign key to the Icon table.
)
GO

-- Index on the ContactType table's name field.
CREATE INDEX [I_ContactType_name] ON [Scheduler].[ContactType] ([name])
GO

-- Index on the ContactType table's iconId field.
CREATE INDEX [I_ContactType_iconId] ON [Scheduler].[ContactType] ([iconId])
GO

-- Index on the ContactType table's active field.
CREATE INDEX [I_ContactType_active] ON [Scheduler].[ContactType] ([active])
GO

-- Index on the ContactType table's deleted field.
CREATE INDEX [I_ContactType_deleted] ON [Scheduler].[ContactType] ([deleted])
GO

INSERT INTO [Scheduler].[ContactType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Project Manager', 'Primary contact for project coordination', 1, '16df32e3-67e4-4012-b2e5-8810b8ab46b9' )
GO

INSERT INTO [Scheduler].[ContactType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Billing Contact', 'Handles invoices and payments', 2, '1e92d7e0-599c-4c72-9e52-731c1129dd88' )
GO

INSERT INTO [Scheduler].[ContactType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Site Superintendent', 'Site Superintendent', 3, 'f3397214-a488-4522-9968-69f6e9985942' )
GO

INSERT INTO [Scheduler].[ContactType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Safety Officer', 'Health & safety representative', 4, 'cfdc40e3-36cb-4cee-863b-184a494f89bb' )
GO

INSERT INTO [Scheduler].[ContactType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Technical Contact', 'Engineering or specs questions', 5, '9586c951-4a27-4975-94c0-70252c86880b' )
GO

INSERT INTO [Scheduler].[ContactType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Emergency Contact', 'For urgent notifications', 6, '7ff865f4-977a-4e94-974b-e86d942a8405' )
GO

INSERT INTO [Scheduler].[ContactType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Accounts Payable', 'Payment processing', 7, 'f42ce916-a408-44d7-bbd4-9f6fc00243e4' )
GO

INSERT INTO [Scheduler].[ContactType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Volunteer', 'Volunteer', 8, '776395dd-6187-44aa-910e-1bf0135cc88a' )
GO

INSERT INTO [Scheduler].[ContactType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Staff', 'Staff', 9, '5cd5bdee-ba1b-43de-8249-8909546b7d28' )
GO

INSERT INTO [Scheduler].[ContactType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Resident', 'Resident', 10, '688ae8cf-ae9d-44f2-a3a4-a900fff70fd9' )
GO

INSERT INTO [Scheduler].[ContactType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Other', 'Other', 99, '95b327b8-9bfc-4338-a04c-e3f61c56f397' )
GO


-- The contact data
CREATE TABLE [Scheduler].[Contact]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[contactTypeId] INT NOT NULL,		-- Link to the ContactType table.
	[firstName] NVARCHAR(250) NOT NULL,
	[middleName] NVARCHAR(250) NULL,
	[lastName] NVARCHAR(250) NOT NULL,
	[salutationId] INT NULL,		-- Link to the Salutation table.
	[title] NVARCHAR(250) NULL,
	[birthDate] DATE NULL,
	[company] NVARCHAR(250) NULL,
	[email] NVARCHAR(250) NULL,
	[phone] NVARCHAR(50) NULL,
	[mobile] NVARCHAR(50) NULL,
	[position] NVARCHAR(250) NULL,
	[webSite] NVARCHAR(1000) NULL,
	[contactMethodId] INT NULL,		-- Link to the ContactMethod table.
	[notes] NVARCHAR(MAX) NULL,
	[timeZoneId] INT NULL,		-- The contact's time zone
	[attributes] NVARCHAR(MAX) NULL,		-- to store arbitrary JSON
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[avatarFileName] NVARCHAR(250) NULL,		-- Part of the binary data field setup
	[avatarSize] BIGINT NULL,		-- Part of the binary data field setup
	[avatarData] VARBINARY(MAX) NULL,		-- Part of the binary data field setup
	[avatarMimeType] NVARCHAR(100) NULL,		-- Part of the binary data field setup
	[externalId] NVARCHAR(100) NULL,
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Contact_ContactType_contactTypeId] FOREIGN KEY ([contactTypeId]) REFERENCES [Scheduler].[ContactType] ([id]),		-- Foreign key to the ContactType table.
	CONSTRAINT [FK_Contact_Salutation_salutationId] FOREIGN KEY ([salutationId]) REFERENCES [Scheduler].[Salutation] ([id]),		-- Foreign key to the Salutation table.
	CONSTRAINT [FK_Contact_ContactMethod_contactMethodId] FOREIGN KEY ([contactMethodId]) REFERENCES [Scheduler].[ContactMethod] ([id]),		-- Foreign key to the ContactMethod table.
	CONSTRAINT [FK_Contact_TimeZone_timeZoneId] FOREIGN KEY ([timeZoneId]) REFERENCES [Scheduler].[TimeZone] ([id]),		-- Foreign key to the TimeZone table.
	CONSTRAINT [FK_Contact_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id])		-- Foreign key to the Icon table.
)
GO

-- Index on the Contact table's tenantGuid field.
CREATE INDEX [I_Contact_tenantGuid] ON [Scheduler].[Contact] ([tenantGuid])
GO

-- Index on the Contact table's tenantGuid,contactTypeId fields.
CREATE INDEX [I_Contact_tenantGuid_contactTypeId] ON [Scheduler].[Contact] ([tenantGuid], [contactTypeId])
GO

-- Index on the Contact table's tenantGuid,company fields.
CREATE INDEX [I_Contact_tenantGuid_company] ON [Scheduler].[Contact] ([tenantGuid], [company])
GO

-- emails must be unique to one contact.
CREATE UNIQUE INDEX [I_Contact_tenantGuid_email] ON [Scheduler].[Contact] ([tenantGuid], [email])
 WHERE [email] IS NOT NULL
GO

-- Index on the Contact table's tenantGuid,phone fields.
CREATE INDEX [I_Contact_tenantGuid_phone] ON [Scheduler].[Contact] ([tenantGuid], [phone])
GO

-- Index on the Contact table's tenantGuid,mobile fields.
CREATE INDEX [I_Contact_tenantGuid_mobile] ON [Scheduler].[Contact] ([tenantGuid], [mobile])
GO

-- Index on the Contact table's tenantGuid,position fields.
CREATE INDEX [I_Contact_tenantGuid_position] ON [Scheduler].[Contact] ([tenantGuid], [position])
GO

-- Index on the Contact table's tenantGuid,contactMethodId fields.
CREATE INDEX [I_Contact_tenantGuid_contactMethodId] ON [Scheduler].[Contact] ([tenantGuid], [contactMethodId])
GO

-- Index on the Contact table's tenantGuid,timeZoneId fields.
CREATE INDEX [I_Contact_tenantGuid_timeZoneId] ON [Scheduler].[Contact] ([tenantGuid], [timeZoneId])
GO

-- Index on the Contact table's tenantGuid,iconId fields.
CREATE INDEX [I_Contact_tenantGuid_iconId] ON [Scheduler].[Contact] ([tenantGuid], [iconId])
GO

-- Index on the Contact table's tenantGuid,active fields.
CREATE INDEX [I_Contact_tenantGuid_active] ON [Scheduler].[Contact] ([tenantGuid], [active])
GO

-- Index on the Contact table's tenantGuid,deleted fields.
CREATE INDEX [I_Contact_tenantGuid_deleted] ON [Scheduler].[Contact] ([tenantGuid], [deleted])
GO

-- Index on the Contact table's tenantGuid,externalId fields.
CREATE INDEX [I_Contact_tenantGuid_externalId] ON [Scheduler].[Contact] ([tenantGuid], [externalId])
GO

-- Index on the Contact table's tenantGuid,lastName,firstName fields.
CREATE INDEX [I_Contact_tenantGuid_lastName_firstName] ON [Scheduler].[Contact] ([tenantGuid], [lastName], [firstName])
GO


-- The change history for records from the Contact table.
CREATE TABLE [Scheduler].[ContactChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[contactId] INT NOT NULL,		-- Link to the Contact table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ContactChangeHistory_Contact_contactId] FOREIGN KEY ([contactId]) REFERENCES [Scheduler].[Contact] ([id])		-- Foreign key to the Contact table.
)
GO

-- Index on the ContactChangeHistory table's tenantGuid field.
CREATE INDEX [I_ContactChangeHistory_tenantGuid] ON [Scheduler].[ContactChangeHistory] ([tenantGuid])
GO

-- Index on the ContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ContactChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ContactChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ContactChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ContactChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ContactChangeHistory_tenantGuid_userId] ON [Scheduler].[ContactChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ContactChangeHistory table's tenantGuid,contactId fields.
CREATE INDEX [I_ContactChangeHistory_tenantGuid_contactId] ON [Scheduler].[ContactChangeHistory] ([tenantGuid], [contactId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- The contact Tag data
CREATE TABLE [Scheduler].[ContactTag]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[contactId] INT NOT NULL,		-- Link to the Contact table.
	[tagId] INT NOT NULL,		-- Link to the Tag table.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ContactTag_Contact_contactId] FOREIGN KEY ([contactId]) REFERENCES [Scheduler].[Contact] ([id]),		-- Foreign key to the Contact table.
	CONSTRAINT [FK_ContactTag_Tag_tagId] FOREIGN KEY ([tagId]) REFERENCES [Scheduler].[Tag] ([id])		-- Foreign key to the Tag table.
)
GO

-- Index on the ContactTag table's tenantGuid field.
CREATE INDEX [I_ContactTag_tenantGuid] ON [Scheduler].[ContactTag] ([tenantGuid])
GO

-- Index on the ContactTag table's tenantGuid,contactId fields.
CREATE INDEX [I_ContactTag_tenantGuid_contactId] ON [Scheduler].[ContactTag] ([tenantGuid], [contactId])
GO

-- Index on the ContactTag table's tenantGuid,tagId fields.
CREATE INDEX [I_ContactTag_tenantGuid_tagId] ON [Scheduler].[ContactTag] ([tenantGuid], [tagId])
GO

-- Index on the ContactTag table's tenantGuid,active fields.
CREATE INDEX [I_ContactTag_tenantGuid_active] ON [Scheduler].[ContactTag] ([tenantGuid], [active])
GO

-- Index on the ContactTag table's tenantGuid,deleted fields.
CREATE INDEX [I_ContactTag_tenantGuid_deleted] ON [Scheduler].[ContactTag] ([tenantGuid], [deleted])
GO


-- The change history for records from the ContactTag table.
CREATE TABLE [Scheduler].[ContactTagChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[contactTagId] INT NOT NULL,		-- Link to the ContactTag table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ContactTagChangeHistory_ContactTag_contactTagId] FOREIGN KEY ([contactTagId]) REFERENCES [Scheduler].[ContactTag] ([id])		-- Foreign key to the ContactTag table.
)
GO

-- Index on the ContactTagChangeHistory table's tenantGuid field.
CREATE INDEX [I_ContactTagChangeHistory_tenantGuid] ON [Scheduler].[ContactTagChangeHistory] ([tenantGuid])
GO

-- Index on the ContactTagChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ContactTagChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ContactTagChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ContactTagChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ContactTagChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ContactTagChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ContactTagChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ContactTagChangeHistory_tenantGuid_userId] ON [Scheduler].[ContactTagChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ContactTagChangeHistory table's tenantGuid,contactTagId fields.
CREATE INDEX [I_ContactTagChangeHistory_tenantGuid_contactTagId] ON [Scheduler].[ContactTagChangeHistory] ([tenantGuid], [contactTagId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Master list of office types.  Used for categorizing offices.  Not tenant specific
CREATE TABLE [Scheduler].[RelationshipType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[isEmergencyEligible] BIT NOT NULL DEFAULT 0,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_RelationshipType_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id])		-- Foreign key to the Icon table.
)
GO

-- Index on the RelationshipType table's name field.
CREATE INDEX [I_RelationshipType_name] ON [Scheduler].[RelationshipType] ([name])
GO

-- Index on the RelationshipType table's iconId field.
CREATE INDEX [I_RelationshipType_iconId] ON [Scheduler].[RelationshipType] ([iconId])
GO

-- Index on the RelationshipType table's active field.
CREATE INDEX [I_RelationshipType_active] ON [Scheduler].[RelationshipType] ([active])
GO

-- Index on the RelationshipType table's deleted field.
CREATE INDEX [I_RelationshipType_deleted] ON [Scheduler].[RelationshipType] ([deleted])
GO

INSERT INTO [Scheduler].[RelationshipType] ( [name], [description], [isEmergencyEligible], [sequence], [objectGuid] ) VALUES  ( 'Self', 'Self', 0, 1, '3d4ec50a-552b-4826-9f7c-a27915134a21' )
GO

INSERT INTO [Scheduler].[RelationshipType] ( [name], [description], [isEmergencyEligible], [sequence], [objectGuid] ) VALUES  ( 'Colleague', 'Colleague', 0, 2, '968a530e-2ec8-449a-b2fa-e853bb82b2c2' )
GO

INSERT INTO [Scheduler].[RelationshipType] ( [name], [description], [isEmergencyEligible], [sequence], [objectGuid] ) VALUES  ( 'Spouse', 'Husband/Wife/Partner', 1, 3, 'e0020ae1-4b49-4d3e-a5a1-67f96ca239c8' )
GO

INSERT INTO [Scheduler].[RelationshipType] ( [name], [description], [isEmergencyEligible], [sequence], [objectGuid] ) VALUES  ( 'Parent', 'Mother/Father', 1, 4, '8622604b-c5d5-4363-9d63-b0c34f3facb2' )
GO

INSERT INTO [Scheduler].[RelationshipType] ( [name], [description], [isEmergencyEligible], [sequence], [objectGuid] ) VALUES  ( 'Child', 'Son/Daughter', 1, 5, 'd35f8329-f18b-445d-8404-0c8fafd9c43b' )
GO

INSERT INTO [Scheduler].[RelationshipType] ( [name], [description], [isEmergencyEligible], [sequence], [objectGuid] ) VALUES  ( 'Sibling', 'Brother/Sister', 1, 6, '07ed8aa5-9034-4cad-b8cc-c5564c5945d9' )
GO

INSERT INTO [Scheduler].[RelationshipType] ( [name], [description], [isEmergencyEligible], [sequence], [objectGuid] ) VALUES  ( 'Friend', 'Close friend', 1, 7, '57a2e1c3-d06e-48cf-aca5-fe5f396e968f' )
GO

INSERT INTO [Scheduler].[RelationshipType] ( [name], [description], [isEmergencyEligible], [sequence], [objectGuid] ) VALUES  ( 'Supervisor', 'Direct manager', 0, 8, '4f51e255-4c2c-41c5-92d9-b051d7d1b15a' )
GO

INSERT INTO [Scheduler].[RelationshipType] ( [name], [description], [isEmergencyEligible], [sequence], [objectGuid] ) VALUES  ( 'Mentor', 'Mentor', 0, 9, 'acfdbb6a-bc68-4753-990c-001c9800c155' )
GO

INSERT INTO [Scheduler].[RelationshipType] ( [name], [description], [isEmergencyEligible], [sequence], [objectGuid] ) VALUES  ( 'Mechanic', 'Equipment Maintenance', 0, 10, '3108554f-3943-4b8c-a196-ee8154cf9918' )
GO

INSERT INTO [Scheduler].[RelationshipType] ( [name], [description], [isEmergencyEligible], [sequence], [objectGuid] ) VALUES  ( 'Resident', 'Resident', 1, 11, '1b92d6de-a154-419e-a3dc-2f0186f029de' )
GO

INSERT INTO [Scheduler].[RelationshipType] ( [name], [description], [isEmergencyEligible], [sequence], [objectGuid] ) VALUES  ( 'Owner', 'Owner', 1, 12, 'e603de2c-8f55-44bb-9198-eaa1c1808498' )
GO

INSERT INTO [Scheduler].[RelationshipType] ( [name], [description], [isEmergencyEligible], [sequence], [objectGuid] ) VALUES  ( 'Other', 'Other relationship', 0, 99, 'b0fc78e9-ca52-4fdc-823f-0339e11dc069' )
GO


-- The link between a contact and other contacts.
CREATE TABLE [Scheduler].[ContactContact]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[contactId] INT NOT NULL,		-- Link to the Contact table.
	[relatedContactId] INT NOT NULL,		-- Link to the Contact table.
	[isPrimary] BIT NOT NULL DEFAULT 0,		-- Indicates whether or not this contact should be considered a primary contact of the contact.
	[relationshipTypeId] INT NOT NULL,		-- A description of the relationship between the contact and the contact.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ContactContact_Contact_contactId] FOREIGN KEY ([contactId]) REFERENCES [Scheduler].[Contact] ([id]),		-- Foreign key to the Contact table.
	CONSTRAINT [FK_ContactContact_Contact_relatedContactId] FOREIGN KEY ([relatedContactId]) REFERENCES [Scheduler].[Contact] ([id]),		-- Foreign key to the Contact table.
	CONSTRAINT [FK_ContactContact_RelationshipType_relationshipTypeId] FOREIGN KEY ([relationshipTypeId]) REFERENCES [Scheduler].[RelationshipType] ([id]),		-- Foreign key to the RelationshipType table.
	CONSTRAINT [UC_ContactContact_tenantGuid_contactId_relatedContactId] UNIQUE ( [tenantGuid], [contactId], [relatedContactId]) 		-- Uniqueness enforced on the ContactContact table's tenantGuid and contactId and relatedContactId fields.
)
GO

-- Index on the ContactContact table's tenantGuid field.
CREATE INDEX [I_ContactContact_tenantGuid] ON [Scheduler].[ContactContact] ([tenantGuid])
GO

-- Index on the ContactContact table's tenantGuid,contactId fields.
CREATE INDEX [I_ContactContact_tenantGuid_contactId] ON [Scheduler].[ContactContact] ([tenantGuid], [contactId])
GO

-- Index on the ContactContact table's tenantGuid,relatedContactId fields.
CREATE INDEX [I_ContactContact_tenantGuid_relatedContactId] ON [Scheduler].[ContactContact] ([tenantGuid], [relatedContactId])
GO

-- Index on the ContactContact table's tenantGuid,relationshipTypeId fields.
CREATE INDEX [I_ContactContact_tenantGuid_relationshipTypeId] ON [Scheduler].[ContactContact] ([tenantGuid], [relationshipTypeId])
GO

-- Index on the ContactContact table's tenantGuid,active fields.
CREATE INDEX [I_ContactContact_tenantGuid_active] ON [Scheduler].[ContactContact] ([tenantGuid], [active])
GO

-- Index on the ContactContact table's tenantGuid,deleted fields.
CREATE INDEX [I_ContactContact_tenantGuid_deleted] ON [Scheduler].[ContactContact] ([tenantGuid], [deleted])
GO


-- The change history for records from the ContactContact table.
CREATE TABLE [Scheduler].[ContactContactChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[contactContactId] INT NOT NULL,		-- Link to the ContactContact table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ContactContactChangeHistory_ContactContact_contactContactId] FOREIGN KEY ([contactContactId]) REFERENCES [Scheduler].[ContactContact] ([id])		-- Foreign key to the ContactContact table.
)
GO

-- Index on the ContactContactChangeHistory table's tenantGuid field.
CREATE INDEX [I_ContactContactChangeHistory_tenantGuid] ON [Scheduler].[ContactContactChangeHistory] ([tenantGuid])
GO

-- Index on the ContactContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ContactContactChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ContactContactChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ContactContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ContactContactChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ContactContactChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ContactContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ContactContactChangeHistory_tenantGuid_userId] ON [Scheduler].[ContactContactChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ContactContactChangeHistory table's tenantGuid,contactContactId fields.
CREATE INDEX [I_ContactContactChangeHistory_tenantGuid_contactContactId] ON [Scheduler].[ContactContactChangeHistory] ([tenantGuid], [contactContactId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Master list of office types.  Used for categorizing offices.  Not tenant specific
CREATE TABLE [Scheduler].[OfficeType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_OfficeType_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id])		-- Foreign key to the Icon table.
)
GO

-- Index on the OfficeType table's name field.
CREATE INDEX [I_OfficeType_name] ON [Scheduler].[OfficeType] ([name])
GO

-- Index on the OfficeType table's iconId field.
CREATE INDEX [I_OfficeType_iconId] ON [Scheduler].[OfficeType] ([iconId])
GO

-- Index on the OfficeType table's active field.
CREATE INDEX [I_OfficeType_active] ON [Scheduler].[OfficeType] ([active])
GO

-- Index on the OfficeType table's deleted field.
CREATE INDEX [I_OfficeType_deleted] ON [Scheduler].[OfficeType] ([deleted])
GO

INSERT INTO [Scheduler].[OfficeType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Headquarters ', 'Headquarters', 1, '3dc56597-1ab7-403e-bad9-8bd52c674f9d' )
GO

INSERT INTO [Scheduler].[OfficeType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Regional Office', 'Regional Office', 2, 'f28b5678-de69-43a3-9a9e-7194df40ea32' )
GO

INSERT INTO [Scheduler].[OfficeType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Branch Office', 'Branch Office', 3, 'd504aef3-b582-4f6d-91c8-b76142f5462a' )
GO

INSERT INTO [Scheduler].[OfficeType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Depot / Yard', 'Depot / Yard', 4, '98b72f2e-de47-4268-885e-3ab7a63e9e8c' )
GO

INSERT INTO [Scheduler].[OfficeType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Administrative Office', 'Administrative Office', 5, 'edc174d4-66f3-410f-a173-b15007c1ff48' )
GO

INSERT INTO [Scheduler].[OfficeType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Warehouse', 'Warehouse', 6, 'c595846a-c3f3-4e07-9df0-af117fa5a400' )
GO

INSERT INTO [Scheduler].[OfficeType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Hospital', 'Hospital', 7, '52a134df-ff0c-4391-ac85-93be54e9541b' )
GO

INSERT INTO [Scheduler].[OfficeType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Clinic', 'Clinic', 8, '9bd149c1-ca03-49c1-a71f-7d8479697205' )
GO


-- The main list of offices operated by an organization using the Scheduler.  Allows schedule and resource grouping.
CREATE TABLE [Scheduler].[Office]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[officeTypeId] INT NOT NULL,		-- Link to the OfficeType table.
	[timeZoneId] INT NOT NULL,		-- Time zone of the office.
	[currencyId] INT NOT NULL,		-- Default billing currency of the office.
	[addressLine1] NVARCHAR(250) NOT NULL,
	[addressLine2] NVARCHAR(250) NULL,
	[city] NVARCHAR(100) NOT NULL,
	[postalCode] NVARCHAR(100) NULL,
	[stateProvinceId] INT NOT NULL,		-- Link to the StateProvince table.
	[countryId] INT NOT NULL,		-- Link to the Country table.
	[phone] NVARCHAR(100) NULL,
	[email] NVARCHAR(250) NULL,
	[latitude] FLOAT NULL,		-- Optional latitude position
	[longitude] FLOAT NULL,		-- Optional longitude position
	[notes] NVARCHAR(MAX) NULL,
	[externalId] NVARCHAR(100) NULL,		-- Optional reference to an ID in an external system 
	[color] NVARCHAR(10) NULL,		-- Override of Target Type Hex color for UI display
	[attributes] NVARCHAR(MAX) NULL,		-- to store arbitrary JSON
	[avatarFileName] NVARCHAR(250) NULL,		-- Part of the binary data field setup
	[avatarSize] BIGINT NULL,		-- Part of the binary data field setup
	[avatarData] VARBINARY(MAX) NULL,		-- Part of the binary data field setup
	[avatarMimeType] NVARCHAR(100) NULL,		-- Part of the binary data field setup
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Office_OfficeType_officeTypeId] FOREIGN KEY ([officeTypeId]) REFERENCES [Scheduler].[OfficeType] ([id]),		-- Foreign key to the OfficeType table.
	CONSTRAINT [FK_Office_TimeZone_timeZoneId] FOREIGN KEY ([timeZoneId]) REFERENCES [Scheduler].[TimeZone] ([id]),		-- Foreign key to the TimeZone table.
	CONSTRAINT [FK_Office_Currency_currencyId] FOREIGN KEY ([currencyId]) REFERENCES [Scheduler].[Currency] ([id]),		-- Foreign key to the Currency table.
	CONSTRAINT [FK_Office_StateProvince_stateProvinceId] FOREIGN KEY ([stateProvinceId]) REFERENCES [Scheduler].[StateProvince] ([id]),		-- Foreign key to the StateProvince table.
	CONSTRAINT [FK_Office_Country_countryId] FOREIGN KEY ([countryId]) REFERENCES [Scheduler].[Country] ([id]),		-- Foreign key to the Country table.
	CONSTRAINT [UC_Office_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Office table's tenantGuid and name fields.
)
GO

-- Index on the Office table's tenantGuid field.
CREATE INDEX [I_Office_tenantGuid] ON [Scheduler].[Office] ([tenantGuid])
GO

-- Index on the Office table's tenantGuid,name fields.
CREATE INDEX [I_Office_tenantGuid_name] ON [Scheduler].[Office] ([tenantGuid], [name])
GO

-- Index on the Office table's tenantGuid,officeTypeId fields.
CREATE INDEX [I_Office_tenantGuid_officeTypeId] ON [Scheduler].[Office] ([tenantGuid], [officeTypeId])
GO

-- Index on the Office table's tenantGuid,timeZoneId fields.
CREATE INDEX [I_Office_tenantGuid_timeZoneId] ON [Scheduler].[Office] ([tenantGuid], [timeZoneId])
GO

-- Index on the Office table's tenantGuid,currencyId fields.
CREATE INDEX [I_Office_tenantGuid_currencyId] ON [Scheduler].[Office] ([tenantGuid], [currencyId])
GO

-- Index on the Office table's tenantGuid,stateProvinceId fields.
CREATE INDEX [I_Office_tenantGuid_stateProvinceId] ON [Scheduler].[Office] ([tenantGuid], [stateProvinceId])
GO

-- Index on the Office table's tenantGuid,countryId fields.
CREATE INDEX [I_Office_tenantGuid_countryId] ON [Scheduler].[Office] ([tenantGuid], [countryId])
GO

-- Index on the Office table's tenantGuid,email fields.
CREATE UNIQUE INDEX [I_Office_tenantGuid_email] ON [Scheduler].[Office] ([tenantGuid], [email])
 WHERE [email] IS NOT NULL
GO

-- Index on the Office table's tenantGuid,active fields.
CREATE INDEX [I_Office_tenantGuid_active] ON [Scheduler].[Office] ([tenantGuid], [active])
GO

-- Index on the Office table's tenantGuid,deleted fields.
CREATE INDEX [I_Office_tenantGuid_deleted] ON [Scheduler].[Office] ([tenantGuid], [deleted])
GO


-- The change history for records from the Office table.
CREATE TABLE [Scheduler].[OfficeChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[officeId] INT NOT NULL,		-- Link to the Office table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_OfficeChangeHistory_Office_officeId] FOREIGN KEY ([officeId]) REFERENCES [Scheduler].[Office] ([id])		-- Foreign key to the Office table.
)
GO

-- Index on the OfficeChangeHistory table's tenantGuid field.
CREATE INDEX [I_OfficeChangeHistory_tenantGuid] ON [Scheduler].[OfficeChangeHistory] ([tenantGuid])
GO

-- Index on the OfficeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_OfficeChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[OfficeChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the OfficeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_OfficeChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[OfficeChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the OfficeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_OfficeChangeHistory_tenantGuid_userId] ON [Scheduler].[OfficeChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the OfficeChangeHistory table's tenantGuid,officeId fields.
CREATE INDEX [I_OfficeChangeHistory_tenantGuid_officeId] ON [Scheduler].[OfficeChangeHistory] ([tenantGuid], [officeId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- The link between contacts and offices.
CREATE TABLE [Scheduler].[OfficeContact]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[officeId] INT NOT NULL,		-- Link to the Office table.
	[contactId] INT NOT NULL,		-- Link to the Contact table.
	[isPrimary] BIT NOT NULL DEFAULT 0,		-- Indicates whether or not this contact should be considered a primary contact of the office.
	[relationshipTypeId] INT NOT NULL,		-- A description of the relationship between the office and the contact.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_OfficeContact_Office_officeId] FOREIGN KEY ([officeId]) REFERENCES [Scheduler].[Office] ([id]),		-- Foreign key to the Office table.
	CONSTRAINT [FK_OfficeContact_Contact_contactId] FOREIGN KEY ([contactId]) REFERENCES [Scheduler].[Contact] ([id]),		-- Foreign key to the Contact table.
	CONSTRAINT [FK_OfficeContact_RelationshipType_relationshipTypeId] FOREIGN KEY ([relationshipTypeId]) REFERENCES [Scheduler].[RelationshipType] ([id]),		-- Foreign key to the RelationshipType table.
	CONSTRAINT [UC_OfficeContact_tenantGuid_officeId_contactId] UNIQUE ( [tenantGuid], [officeId], [contactId]) 		-- Uniqueness enforced on the OfficeContact table's tenantGuid and officeId and contactId fields.
)
GO

-- Index on the OfficeContact table's tenantGuid field.
CREATE INDEX [I_OfficeContact_tenantGuid] ON [Scheduler].[OfficeContact] ([tenantGuid])
GO

-- Index on the OfficeContact table's tenantGuid,officeId fields.
CREATE INDEX [I_OfficeContact_tenantGuid_officeId] ON [Scheduler].[OfficeContact] ([tenantGuid], [officeId])
GO

-- Index on the OfficeContact table's tenantGuid,contactId fields.
CREATE INDEX [I_OfficeContact_tenantGuid_contactId] ON [Scheduler].[OfficeContact] ([tenantGuid], [contactId])
GO

-- Index on the OfficeContact table's tenantGuid,relationshipTypeId fields.
CREATE INDEX [I_OfficeContact_tenantGuid_relationshipTypeId] ON [Scheduler].[OfficeContact] ([tenantGuid], [relationshipTypeId])
GO

-- Index on the OfficeContact table's tenantGuid,active fields.
CREATE INDEX [I_OfficeContact_tenantGuid_active] ON [Scheduler].[OfficeContact] ([tenantGuid], [active])
GO

-- Index on the OfficeContact table's tenantGuid,deleted fields.
CREATE INDEX [I_OfficeContact_tenantGuid_deleted] ON [Scheduler].[OfficeContact] ([tenantGuid], [deleted])
GO


-- The change history for records from the OfficeContact table.
CREATE TABLE [Scheduler].[OfficeContactChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[officeContactId] INT NOT NULL,		-- Link to the OfficeContact table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_OfficeContactChangeHistory_OfficeContact_officeContactId] FOREIGN KEY ([officeContactId]) REFERENCES [Scheduler].[OfficeContact] ([id])		-- Foreign key to the OfficeContact table.
)
GO

-- Index on the OfficeContactChangeHistory table's tenantGuid field.
CREATE INDEX [I_OfficeContactChangeHistory_tenantGuid] ON [Scheduler].[OfficeContactChangeHistory] ([tenantGuid])
GO

-- Index on the OfficeContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_OfficeContactChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[OfficeContactChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the OfficeContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_OfficeContactChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[OfficeContactChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the OfficeContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_OfficeContactChangeHistory_tenantGuid_userId] ON [Scheduler].[OfficeContactChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the OfficeContactChangeHistory table's tenantGuid,officeContactId fields.
CREATE INDEX [I_OfficeContactChangeHistory_tenantGuid_officeContactId] ON [Scheduler].[OfficeContactChangeHistory] ([tenantGuid], [officeContactId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Optional logical grouping of events for visibility and filtering (e.g., '2026 Road Projects', 'Maintenance Calendar').
CREATE TABLE [Scheduler].[Calendar]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[officeId] INT NULL,		-- Optional office binding for the calendar
	[isDefault] BIT NULL,
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	[versionNumber] INT NOT NULL DEFAULT 1		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.

	CONSTRAINT [FK_Calendar_Office_officeId] FOREIGN KEY ([officeId]) REFERENCES [Scheduler].[Office] ([id]),		-- Foreign key to the Office table.
	CONSTRAINT [FK_Calendar_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_Calendar_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Calendar table's tenantGuid and name fields.
)
GO

-- Index on the Calendar table's tenantGuid field.
CREATE INDEX [I_Calendar_tenantGuid] ON [Scheduler].[Calendar] ([tenantGuid])
GO

-- Index on the Calendar table's tenantGuid,name fields.
CREATE INDEX [I_Calendar_tenantGuid_name] ON [Scheduler].[Calendar] ([tenantGuid], [name])
GO

-- Index on the Calendar table's tenantGuid,officeId fields.
CREATE INDEX [I_Calendar_tenantGuid_officeId] ON [Scheduler].[Calendar] ([tenantGuid], [officeId])
GO

-- Index on the Calendar table's tenantGuid,iconId fields.
CREATE INDEX [I_Calendar_tenantGuid_iconId] ON [Scheduler].[Calendar] ([tenantGuid], [iconId])
GO

-- Index on the Calendar table's tenantGuid,active fields.
CREATE INDEX [I_Calendar_tenantGuid_active] ON [Scheduler].[Calendar] ([tenantGuid], [active])
GO

-- Index on the Calendar table's tenantGuid,deleted fields.
CREATE INDEX [I_Calendar_tenantGuid_deleted] ON [Scheduler].[Calendar] ([tenantGuid], [deleted])
GO


-- The change history for records from the Calendar table.
CREATE TABLE [Scheduler].[CalendarChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[calendarId] INT NOT NULL,		-- Link to the Calendar table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_CalendarChangeHistory_Calendar_calendarId] FOREIGN KEY ([calendarId]) REFERENCES [Scheduler].[Calendar] ([id])		-- Foreign key to the Calendar table.
)
GO

-- Index on the CalendarChangeHistory table's tenantGuid field.
CREATE INDEX [I_CalendarChangeHistory_tenantGuid] ON [Scheduler].[CalendarChangeHistory] ([tenantGuid])
GO

-- Index on the CalendarChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_CalendarChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[CalendarChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the CalendarChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_CalendarChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[CalendarChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the CalendarChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_CalendarChangeHistory_tenantGuid_userId] ON [Scheduler].[CalendarChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the CalendarChangeHistory table's tenantGuid,calendarId fields.
CREATE INDEX [I_CalendarChangeHistory_tenantGuid_calendarId] ON [Scheduler].[CalendarChangeHistory] ([tenantGuid], [calendarId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Master list of client types.  Used for categorizing clients.
CREATE TABLE [Scheduler].[ClientType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ClientType_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_ClientType_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the ClientType table's tenantGuid and name fields.
)
GO

-- Index on the ClientType table's tenantGuid field.
CREATE INDEX [I_ClientType_tenantGuid] ON [Scheduler].[ClientType] ([tenantGuid])
GO

-- Index on the ClientType table's tenantGuid,name fields.
CREATE INDEX [I_ClientType_tenantGuid_name] ON [Scheduler].[ClientType] ([tenantGuid], [name])
GO

-- Index on the ClientType table's tenantGuid,iconId fields.
CREATE INDEX [I_ClientType_tenantGuid_iconId] ON [Scheduler].[ClientType] ([tenantGuid], [iconId])
GO

-- Index on the ClientType table's tenantGuid,active fields.
CREATE INDEX [I_ClientType_tenantGuid_active] ON [Scheduler].[ClientType] ([tenantGuid], [active])
GO

-- Index on the ClientType table's tenantGuid,deleted fields.
CREATE INDEX [I_ClientType_tenantGuid_deleted] ON [Scheduler].[ClientType] ([tenantGuid], [deleted])
GO

INSERT INTO [Scheduler].[ClientType] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Construction ', 'A construction client', 1, '331c07c6-bcd1-4d8d-b796-d81216bba704' )
GO

INSERT INTO [Scheduler].[ClientType] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Healthcare', 'A healthcare client', 2, '701001e4-4034-4b18-ab29-b514b08bc541' )
GO


-- The main client list.  Is not directly schedulable, but provides billing details.  Contains scheduling targets which are schedulable.
CREATE TABLE [Scheduler].[Client]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[clientTypeId] INT NOT NULL,		-- Link to the ClientType table.
	[currencyId] INT NOT NULL,		-- Link to the Currency table.
	[timeZoneId] INT NOT NULL,		-- Link to the TimeZone table.
	[calendarId] INT NULL,		-- An optional default calendar for the scheduling target's belonging to the client.
	[addressLine1] NVARCHAR(250) NOT NULL,
	[addressLine2] NVARCHAR(250) NULL,
	[city] NVARCHAR(100) NOT NULL,
	[postalCode] NVARCHAR(100) NULL,
	[stateProvinceId] INT NOT NULL,		-- Link to the StateProvince table.
	[countryId] INT NOT NULL,		-- Link to the Country table.
	[phone] NVARCHAR(100) NULL,
	[email] NVARCHAR(250) NULL,
	[latitude] FLOAT NULL,		-- Optional latitude position
	[longitude] FLOAT NULL,		-- Optional longitude position
	[notes] NVARCHAR(MAX) NULL,
	[externalId] NVARCHAR(100) NULL,		-- Optional reference to an ID in an external system
	[color] NVARCHAR(10) NULL,		-- Override of Target Type Hex color for UI display
	[attributes] NVARCHAR(MAX) NULL,		-- to store arbitrary JSON
	[avatarFileName] NVARCHAR(250) NULL,		-- Part of the binary data field setup
	[avatarSize] BIGINT NULL,		-- Part of the binary data field setup
	[avatarData] VARBINARY(MAX) NULL,		-- Part of the binary data field setup
	[avatarMimeType] NVARCHAR(100) NULL,		-- Part of the binary data field setup
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Client_ClientType_clientTypeId] FOREIGN KEY ([clientTypeId]) REFERENCES [Scheduler].[ClientType] ([id]),		-- Foreign key to the ClientType table.
	CONSTRAINT [FK_Client_Currency_currencyId] FOREIGN KEY ([currencyId]) REFERENCES [Scheduler].[Currency] ([id]),		-- Foreign key to the Currency table.
	CONSTRAINT [FK_Client_TimeZone_timeZoneId] FOREIGN KEY ([timeZoneId]) REFERENCES [Scheduler].[TimeZone] ([id]),		-- Foreign key to the TimeZone table.
	CONSTRAINT [FK_Client_Calendar_calendarId] FOREIGN KEY ([calendarId]) REFERENCES [Scheduler].[Calendar] ([id]),		-- Foreign key to the Calendar table.
	CONSTRAINT [FK_Client_StateProvince_stateProvinceId] FOREIGN KEY ([stateProvinceId]) REFERENCES [Scheduler].[StateProvince] ([id]),		-- Foreign key to the StateProvince table.
	CONSTRAINT [FK_Client_Country_countryId] FOREIGN KEY ([countryId]) REFERENCES [Scheduler].[Country] ([id]),		-- Foreign key to the Country table.
	CONSTRAINT [UC_Client_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Client table's tenantGuid and name fields.
)
GO

-- Index on the Client table's tenantGuid field.
CREATE INDEX [I_Client_tenantGuid] ON [Scheduler].[Client] ([tenantGuid])
GO

-- Index on the Client table's tenantGuid,name fields.
CREATE INDEX [I_Client_tenantGuid_name] ON [Scheduler].[Client] ([tenantGuid], [name])
GO

-- Index on the Client table's tenantGuid,clientTypeId fields.
CREATE INDEX [I_Client_tenantGuid_clientTypeId] ON [Scheduler].[Client] ([tenantGuid], [clientTypeId])
GO

-- Index on the Client table's tenantGuid,currencyId fields.
CREATE INDEX [I_Client_tenantGuid_currencyId] ON [Scheduler].[Client] ([tenantGuid], [currencyId])
GO

-- Index on the Client table's tenantGuid,timeZoneId fields.
CREATE INDEX [I_Client_tenantGuid_timeZoneId] ON [Scheduler].[Client] ([tenantGuid], [timeZoneId])
GO

-- Index on the Client table's tenantGuid,stateProvinceId fields.
CREATE INDEX [I_Client_tenantGuid_stateProvinceId] ON [Scheduler].[Client] ([tenantGuid], [stateProvinceId])
GO

-- Index on the Client table's tenantGuid,countryId fields.
CREATE INDEX [I_Client_tenantGuid_countryId] ON [Scheduler].[Client] ([tenantGuid], [countryId])
GO

-- emails must be unique to one Client.
CREATE UNIQUE INDEX [I_Client_tenantGuid_email] ON [Scheduler].[Client] ([tenantGuid], [email])
 WHERE [email] IS NOT NULL
GO

-- Index on the Client table's tenantGuid,active fields.
CREATE INDEX [I_Client_tenantGuid_active] ON [Scheduler].[Client] ([tenantGuid], [active])
GO

-- Index on the Client table's tenantGuid,deleted fields.
CREATE INDEX [I_Client_tenantGuid_deleted] ON [Scheduler].[Client] ([tenantGuid], [deleted])
GO


-- The change history for records from the Client table.
CREATE TABLE [Scheduler].[ClientChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[clientId] INT NOT NULL,		-- Link to the Client table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ClientChangeHistory_Client_clientId] FOREIGN KEY ([clientId]) REFERENCES [Scheduler].[Client] ([id])		-- Foreign key to the Client table.
)
GO

-- Index on the ClientChangeHistory table's tenantGuid field.
CREATE INDEX [I_ClientChangeHistory_tenantGuid] ON [Scheduler].[ClientChangeHistory] ([tenantGuid])
GO

-- Index on the ClientChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ClientChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ClientChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ClientChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ClientChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ClientChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ClientChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ClientChangeHistory_tenantGuid_userId] ON [Scheduler].[ClientChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ClientChangeHistory table's tenantGuid,clientId fields.
CREATE INDEX [I_ClientChangeHistory_tenantGuid_clientId] ON [Scheduler].[ClientChangeHistory] ([tenantGuid], [clientId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- The link between contacts and clients.
CREATE TABLE [Scheduler].[ClientContact]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[clientId] INT NOT NULL,		-- Link to the Client table.
	[contactId] INT NOT NULL,		-- Link to the Contact table.
	[isPrimary] BIT NOT NULL DEFAULT 0,		-- Indicates whether or not this contact should be considered a primary contact of the client.
	[relationshipTypeId] INT NOT NULL,		-- A description of the relationship between the client and the contact.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ClientContact_Client_clientId] FOREIGN KEY ([clientId]) REFERENCES [Scheduler].[Client] ([id]),		-- Foreign key to the Client table.
	CONSTRAINT [FK_ClientContact_Contact_contactId] FOREIGN KEY ([contactId]) REFERENCES [Scheduler].[Contact] ([id]),		-- Foreign key to the Contact table.
	CONSTRAINT [FK_ClientContact_RelationshipType_relationshipTypeId] FOREIGN KEY ([relationshipTypeId]) REFERENCES [Scheduler].[RelationshipType] ([id]),		-- Foreign key to the RelationshipType table.
	CONSTRAINT [UC_ClientContact_tenantGuid_clientId_contactId] UNIQUE ( [tenantGuid], [clientId], [contactId]) 		-- Uniqueness enforced on the ClientContact table's tenantGuid and clientId and contactId fields.
)
GO

-- Index on the ClientContact table's tenantGuid field.
CREATE INDEX [I_ClientContact_tenantGuid] ON [Scheduler].[ClientContact] ([tenantGuid])
GO

-- Index on the ClientContact table's tenantGuid,clientId fields.
CREATE INDEX [I_ClientContact_tenantGuid_clientId] ON [Scheduler].[ClientContact] ([tenantGuid], [clientId])
GO

-- Index on the ClientContact table's tenantGuid,contactId fields.
CREATE INDEX [I_ClientContact_tenantGuid_contactId] ON [Scheduler].[ClientContact] ([tenantGuid], [contactId])
GO

-- Index on the ClientContact table's tenantGuid,relationshipTypeId fields.
CREATE INDEX [I_ClientContact_tenantGuid_relationshipTypeId] ON [Scheduler].[ClientContact] ([tenantGuid], [relationshipTypeId])
GO

-- Index on the ClientContact table's tenantGuid,active fields.
CREATE INDEX [I_ClientContact_tenantGuid_active] ON [Scheduler].[ClientContact] ([tenantGuid], [active])
GO

-- Index on the ClientContact table's tenantGuid,deleted fields.
CREATE INDEX [I_ClientContact_tenantGuid_deleted] ON [Scheduler].[ClientContact] ([tenantGuid], [deleted])
GO


-- The change history for records from the ClientContact table.
CREATE TABLE [Scheduler].[ClientContactChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[clientContactId] INT NOT NULL,		-- Link to the ClientContact table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ClientContactChangeHistory_ClientContact_clientContactId] FOREIGN KEY ([clientContactId]) REFERENCES [Scheduler].[ClientContact] ([id])		-- Foreign key to the ClientContact table.
)
GO

-- Index on the ClientContactChangeHistory table's tenantGuid field.
CREATE INDEX [I_ClientContactChangeHistory_tenantGuid] ON [Scheduler].[ClientContactChangeHistory] ([tenantGuid])
GO

-- Index on the ClientContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ClientContactChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ClientContactChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ClientContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ClientContactChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ClientContactChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ClientContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ClientContactChangeHistory_tenantGuid_userId] ON [Scheduler].[ClientContactChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ClientContactChangeHistory table's tenantGuid,clientContactId fields.
CREATE INDEX [I_ClientContactChangeHistory_tenantGuid_clientContactId] ON [Scheduler].[ClientContactChangeHistory] ([tenantGuid], [clientContactId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Tenant-level information. Client admins manage this data.
CREATE TABLE [Scheduler].[TenantProfile]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[companyLogoFileName] NVARCHAR(250) NULL,		-- Part of the binary data field setup
	[companyLogoSize] BIGINT NULL,		-- Part of the binary data field setup
	[companyLogoData] VARBINARY(MAX) NULL,		-- Part of the binary data field setup
	[companyLogoMimeType] NVARCHAR(100) NULL,		-- Part of the binary data field setup
	[addressLine1] NVARCHAR(250) NULL,
	[addressLine2] NVARCHAR(250) NULL,
	[addressLine3] NVARCHAR(250) NULL,
	[city] NVARCHAR(100) NULL,
	[postalCode] NVARCHAR(100) NULL,
	[stateProvinceId] INT NULL,		-- Link to the StateProvince table.
	[countryId] INT NULL,		-- Link to the Country table.
	[timeZoneId] INT NULL,		-- Link to the TimeZone table.
	[phoneNumber] NVARCHAR(100) NULL,
	[email] NVARCHAR(250) NULL,
	[website] NVARCHAR(1000) NULL,
	[latitude] FLOAT NULL,		-- Optional latitude position
	[longitude] FLOAT NULL,		-- Optional longitude position
	[primaryColor] NVARCHAR(10) NULL,
	[secondaryColor] NVARCHAR(10) NULL,
	[displaysMetric] BIT NOT NULL DEFAULT 0,		-- True if the tenant defaults to using metric units when creating projects.    Note that this does not affect the storage units, which are always metric.
	[displaysUSTerms] BIT NOT NULL DEFAULT 0,		-- True if the tenant defaults to using terms for the US market, such as Zip code,.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_TenantProfile_StateProvince_stateProvinceId] FOREIGN KEY ([stateProvinceId]) REFERENCES [Scheduler].[StateProvince] ([id]),		-- Foreign key to the StateProvince table.
	CONSTRAINT [FK_TenantProfile_Country_countryId] FOREIGN KEY ([countryId]) REFERENCES [Scheduler].[Country] ([id]),		-- Foreign key to the Country table.
	CONSTRAINT [FK_TenantProfile_TimeZone_timeZoneId] FOREIGN KEY ([timeZoneId]) REFERENCES [Scheduler].[TimeZone] ([id]),		-- Foreign key to the TimeZone table.
	CONSTRAINT [UC_TenantProfile_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the TenantProfile table's tenantGuid and name fields.
)
GO

-- Index on the TenantProfile table's tenantGuid field.
CREATE INDEX [I_TenantProfile_tenantGuid] ON [Scheduler].[TenantProfile] ([tenantGuid])
GO

-- Index on the TenantProfile table's tenantGuid,name fields.
CREATE INDEX [I_TenantProfile_tenantGuid_name] ON [Scheduler].[TenantProfile] ([tenantGuid], [name])
GO

-- Index on the TenantProfile table's tenantGuid,timeZoneId fields.
CREATE INDEX [I_TenantProfile_tenantGuid_timeZoneId] ON [Scheduler].[TenantProfile] ([tenantGuid], [timeZoneId])
GO

-- Index on the TenantProfile table's tenantGuid,active fields.
CREATE INDEX [I_TenantProfile_tenantGuid_active] ON [Scheduler].[TenantProfile] ([tenantGuid], [active])
GO

-- Index on the TenantProfile table's tenantGuid,deleted fields.
CREATE INDEX [I_TenantProfile_tenantGuid_deleted] ON [Scheduler].[TenantProfile] ([tenantGuid], [deleted])
GO


-- The change history for records from the TenantProfile table.
CREATE TABLE [Scheduler].[TenantProfileChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[tenantProfileId] INT NOT NULL,		-- Link to the TenantProfile table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_TenantProfileChangeHistory_TenantProfile_tenantProfileId] FOREIGN KEY ([tenantProfileId]) REFERENCES [Scheduler].[TenantProfile] ([id])		-- Foreign key to the TenantProfile table.
)
GO

-- Index on the TenantProfileChangeHistory table's tenantGuid field.
CREATE INDEX [I_TenantProfileChangeHistory_tenantGuid] ON [Scheduler].[TenantProfileChangeHistory] ([tenantGuid])
GO

-- Index on the TenantProfileChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_TenantProfileChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[TenantProfileChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the TenantProfileChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_TenantProfileChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[TenantProfileChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the TenantProfileChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_TenantProfileChangeHistory_tenantGuid_userId] ON [Scheduler].[TenantProfileChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the TenantProfileChangeHistory table's tenantGuid,tenantProfileId fields.
CREATE INDEX [I_TenantProfileChangeHistory_tenantGuid_tenantProfileId] ON [Scheduler].[TenantProfileChangeHistory] ([tenantGuid], [tenantProfileId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Master list of qualifications, certifications, or competencies required for certain work.  Examples: RN License, Crane Operator Certification, OSHA 30, Pediatric Specialty, Confined Space Entry.
CREATE TABLE [Scheduler].[Qualification]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[isLicense] BIT NULL,		-- for special handling (e.g., expiry warnings)
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_Qualification_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Qualification table's tenantGuid and name fields.
)
GO

-- Index on the Qualification table's tenantGuid field.
CREATE INDEX [I_Qualification_tenantGuid] ON [Scheduler].[Qualification] ([tenantGuid])
GO

-- Index on the Qualification table's tenantGuid,name fields.
CREATE INDEX [I_Qualification_tenantGuid_name] ON [Scheduler].[Qualification] ([tenantGuid], [name])
GO

-- Index on the Qualification table's tenantGuid,active fields.
CREATE INDEX [I_Qualification_tenantGuid_active] ON [Scheduler].[Qualification] ([tenantGuid], [active])
GO

-- Index on the Qualification table's tenantGuid,deleted fields.
CREATE INDEX [I_Qualification_tenantGuid_deleted] ON [Scheduler].[Qualification] ([tenantGuid], [deleted])
GO


-- Tenant-configurable roles that a resource can fulfil during an event.  Examples: Operator, Supervisor, Driver, Spotter, Safety Officer.  Used for business rule enforcement and richer reporting.
CREATE TABLE [Scheduler].[AssignmentRole]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_AssignmentRole_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_AssignmentRole_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the AssignmentRole table's tenantGuid and name fields.
)
GO

-- Index on the AssignmentRole table's tenantGuid field.
CREATE INDEX [I_AssignmentRole_tenantGuid] ON [Scheduler].[AssignmentRole] ([tenantGuid])
GO

-- Index on the AssignmentRole table's tenantGuid,name fields.
CREATE INDEX [I_AssignmentRole_tenantGuid_name] ON [Scheduler].[AssignmentRole] ([tenantGuid], [name])
GO

-- Index on the AssignmentRole table's tenantGuid,iconId fields.
CREATE INDEX [I_AssignmentRole_tenantGuid_iconId] ON [Scheduler].[AssignmentRole] ([tenantGuid], [iconId])
GO

-- Index on the AssignmentRole table's tenantGuid,active fields.
CREATE INDEX [I_AssignmentRole_tenantGuid_active] ON [Scheduler].[AssignmentRole] ([tenantGuid], [active])
GO

-- Index on the AssignmentRole table's tenantGuid,deleted fields.
CREATE INDEX [I_AssignmentRole_tenantGuid_deleted] ON [Scheduler].[AssignmentRole] ([tenantGuid], [deleted])
GO

INSERT INTO [Scheduler].[AssignmentRole] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Operator', 'Primary equipment operator', 1, 'b2c3d4e5-6789-0123-4567-89abcdef0001' )
GO

INSERT INTO [Scheduler].[AssignmentRole] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Supervisor', 'Site supervisor', 2, 'b2c3d4e5-6789-0123-4567-89abcdef0002' )
GO

INSERT INTO [Scheduler].[AssignmentRole] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Driver', 'Haul truck or service vehicle driver', 3, 'b2c3d4e5-6789-0123-4567-89abcdef0003' )
GO

INSERT INTO [Scheduler].[AssignmentRole] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Spotter', 'Safety spotter / banksman', 4, 'b2c3d4e5-6789-0123-4567-89abcdef0004' )
GO


-- Defines which qualifications are required to fulfill a specific AssignmentRole.  This is the most common way to enforce certification requirements.
CREATE TABLE [Scheduler].[AssignmentRoleQualificationRequirement]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[assignmentRoleId] INT NOT NULL,		-- Link to the AssignmentRole table.
	[qualificationId] INT NOT NULL,		-- Link to the Qualification table.
	[isRequired] BIT NOT NULL DEFAULT 1,		-- true = mandatory to fulfill role, false = preferred/recommended
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_AssignmentRoleQualificationRequirement_AssignmentRole_assignmentRoleId] FOREIGN KEY ([assignmentRoleId]) REFERENCES [Scheduler].[AssignmentRole] ([id]),		-- Foreign key to the AssignmentRole table.
	CONSTRAINT [FK_AssignmentRoleQualificationRequirement_Qualification_qualificationId] FOREIGN KEY ([qualificationId]) REFERENCES [Scheduler].[Qualification] ([id]),		-- Foreign key to the Qualification table.
	CONSTRAINT [UC_AssignmentRoleQualificationRequirement_tenantGuid_assignmentRoleId_qualificationId] UNIQUE ( [tenantGuid], [assignmentRoleId], [qualificationId]) 		-- Uniqueness enforced on the AssignmentRoleQualificationRequirement table's tenantGuid and assignmentRoleId and qualificationId fields.
)
GO

-- Index on the AssignmentRoleQualificationRequirement table's tenantGuid field.
CREATE INDEX [I_AssignmentRoleQualificationRequirement_tenantGuid] ON [Scheduler].[AssignmentRoleQualificationRequirement] ([tenantGuid])
GO

-- Index on the AssignmentRoleQualificationRequirement table's tenantGuid,assignmentRoleId fields.
CREATE INDEX [I_AssignmentRoleQualificationRequirement_tenantGuid_assignmentRoleId] ON [Scheduler].[AssignmentRoleQualificationRequirement] ([tenantGuid], [assignmentRoleId])
GO

-- Index on the AssignmentRoleQualificationRequirement table's tenantGuid,qualificationId fields.
CREATE INDEX [I_AssignmentRoleQualificationRequirement_tenantGuid_qualificationId] ON [Scheduler].[AssignmentRoleQualificationRequirement] ([tenantGuid], [qualificationId])
GO

-- Index on the AssignmentRoleQualificationRequirement table's tenantGuid,active fields.
CREATE INDEX [I_AssignmentRoleQualificationRequirement_tenantGuid_active] ON [Scheduler].[AssignmentRoleQualificationRequirement] ([tenantGuid], [active])
GO

-- Index on the AssignmentRoleQualificationRequirement table's tenantGuid,deleted fields.
CREATE INDEX [I_AssignmentRoleQualificationRequirement_tenantGuid_deleted] ON [Scheduler].[AssignmentRoleQualificationRequirement] ([tenantGuid], [deleted])
GO


-- The change history for records from the AssignmentRoleQualificationRequirement table.
CREATE TABLE [Scheduler].[AssignmentRoleQualificationRequirementChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[assignmentRoleQualificationRequirementId] INT NOT NULL,		-- Link to the AssignmentRoleQualificationRequirement table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ssgnmntRlQulfctnRqurmntChngHstry_ssgnmntRlQulfctnRqurmnt_ssgnmntRlQulfctnRqurmntd] FOREIGN KEY ([assignmentRoleQualificationRequirementId]) REFERENCES [Scheduler].[AssignmentRoleQualificationRequirement] ([id])		-- Foreign key to the AssignmentRoleQualificationRequirement table.
)
GO

-- Index on the AssignmentRoleQualificationRequirementChangeHistory table's tenantGuid field.
CREATE INDEX [I_AssignmentRoleQualificationRequirementChangeHistory_tenantGuid] ON [Scheduler].[AssignmentRoleQualificationRequirementChangeHistory] ([tenantGuid])
GO

-- Index on the AssignmentRoleQualificationRequirementChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_AssignmentRoleQualificationRequirementChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[AssignmentRoleQualificationRequirementChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the AssignmentRoleQualificationRequirementChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_AssignmentRoleQualificationRequirementChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[AssignmentRoleQualificationRequirementChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the AssignmentRoleQualificationRequirementChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_AssignmentRoleQualificationRequirementChangeHistory_tenantGuid_userId] ON [Scheduler].[AssignmentRoleQualificationRequirementChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the AssignmentRoleQualificationRequirementChangeHistory table's tenantGuid,assignmentRoleQualificationRequirementId fields.
CREATE INDEX [I_AssignmentRoleQualificationRequirementChangeHistory_tenantGuid_assignmentRoleQualificationRequirementId] ON [Scheduler].[AssignmentRoleQualificationRequirementChangeHistory] ([tenantGuid], [assignmentRoleQualificationRequirementId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Master list of event statuses (Planned, In Progress, Completed, No-Show, Canceled, etc.)
CREATE TABLE [Scheduler].[EventStatus]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the EventStatus table's name field.
CREATE INDEX [I_EventStatus_name] ON [Scheduler].[EventStatus] ([name])
GO

-- Index on the EventStatus table's active field.
CREATE INDEX [I_EventStatus_active] ON [Scheduler].[EventStatus] ([active])
GO

-- Index on the EventStatus table's deleted field.
CREATE INDEX [I_EventStatus_deleted] ON [Scheduler].[EventStatus] ([deleted])
GO

INSERT INTO [Scheduler].[EventStatus] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Planned', 'Scheduled but not started', 1, '005bdc39-da8e-465a-a17e-78aafffb390a' )
GO

INSERT INTO [Scheduler].[EventStatus] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'In Progress', 'Started', 2, '513bd381-6ab9-407c-ac4d-9187f6f92e16' )
GO

INSERT INTO [Scheduler].[EventStatus] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Completed', 'Finished successfully', 3, '6af9e244-2eff-463b-a40c-821fe00fa644' )
GO

INSERT INTO [Scheduler].[EventStatus] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'No-Show', 'No Show', 4, 'd7e81b73-bbe3-42dd-bcf6-856a82b9fce1' )
GO

INSERT INTO [Scheduler].[EventStatus] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Canceled', 'Explicitly canceled', 5, '01148ccb-e746-4218-88c5-8f0a5ee36adc' )
GO


-- Master list of payment types ( credit card, check, cash, etc..)
CREATE TABLE [Scheduler].[PaymentType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the PaymentType table's name field.
CREATE INDEX [I_PaymentType_name] ON [Scheduler].[PaymentType] ([name])
GO

-- Index on the PaymentType table's active field.
CREATE INDEX [I_PaymentType_active] ON [Scheduler].[PaymentType] ([active])
GO

-- Index on the PaymentType table's deleted field.
CREATE INDEX [I_PaymentType_deleted] ON [Scheduler].[PaymentType] ([deleted])
GO

INSERT INTO [Scheduler].[PaymentType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Credit Card', 'Credit Card', 1, '3353a9f0-1b8e-4170-a20a-d35eab81fab8' )
GO

INSERT INTO [Scheduler].[PaymentType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Check', 'Check', 2, '19376f2d-87c0-4eb5-a11c-f02cb4f9b412' )
GO

INSERT INTO [Scheduler].[PaymentType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Cash', 'Cash', 3, 'dca9c876-bb7d-4c33-8ef4-96a955dacbb0' )
GO

INSERT INTO [Scheduler].[PaymentType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Crypto', 'Crypto', 4, '8be012ff-f305-45cd-bedb-cc5b9f11f3ef' )
GO

INSERT INTO [Scheduler].[PaymentType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Stock', 'Stock', 5, '427451dc-b522-4613-aa3a-57593b6d4d03' )
GO


-- Master list of receipt types
CREATE TABLE [Scheduler].[ReceiptType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the ReceiptType table's name field.
CREATE INDEX [I_ReceiptType_name] ON [Scheduler].[ReceiptType] ([name])
GO

-- Index on the ReceiptType table's active field.
CREATE INDEX [I_ReceiptType_active] ON [Scheduler].[ReceiptType] ([active])
GO

-- Index on the ReceiptType table's deleted field.
CREATE INDEX [I_ReceiptType_deleted] ON [Scheduler].[ReceiptType] ([deleted])
GO

INSERT INTO [Scheduler].[ReceiptType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Receipted', 'Receipted', 1, 'b0a794eb-afa9-4791-b164-e28e5ed21a35' )
GO

INSERT INTO [Scheduler].[ReceiptType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Do Not Receipt', 'Do Not Receipt', 2, 'd6ceb144-aced-4e2a-9407-a2b0c995c795' )
GO


-- Master list of booking sources ( walk-in, phone, online)
CREATE TABLE [Scheduler].[BookingSourceType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the BookingSourceType table's name field.
CREATE INDEX [I_BookingSourceType_name] ON [Scheduler].[BookingSourceType] ([name])
GO

-- Index on the BookingSourceType table's active field.
CREATE INDEX [I_BookingSourceType_active] ON [Scheduler].[BookingSourceType] ([active])
GO

-- Index on the BookingSourceType table's deleted field.
CREATE INDEX [I_BookingSourceType_deleted] ON [Scheduler].[BookingSourceType] ([deleted])
GO

INSERT INTO [Scheduler].[BookingSourceType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Administrative', 'Administrative', 1, '3ec3e46a-ece8-4364-8396-beaf23aa0a2a' )
GO

INSERT INTO [Scheduler].[BookingSourceType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Phone', 'Phone', 2, 'cb9c2d46-29d5-4caa-9d5c-9e84356edf86' )
GO

INSERT INTO [Scheduler].[BookingSourceType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Walk-in', 'Walk-in', 3, 'fc0a5ebf-794d-4e61-9dce-f308da9d9ba4' )
GO

INSERT INTO [Scheduler].[BookingSourceType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Online', 'Online', 4, '1955a3f1-adce-4bc4-99d1-86362ff98a57' )
GO


-- Master list of assignment statuses (Planned, In Progress, Completed, No-Show, Canceled, etc.)
CREATE TABLE [Scheduler].[AssignmentStatus]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the AssignmentStatus table's name field.
CREATE INDEX [I_AssignmentStatus_name] ON [Scheduler].[AssignmentStatus] ([name])
GO

-- Index on the AssignmentStatus table's active field.
CREATE INDEX [I_AssignmentStatus_active] ON [Scheduler].[AssignmentStatus] ([active])
GO

-- Index on the AssignmentStatus table's deleted field.
CREATE INDEX [I_AssignmentStatus_deleted] ON [Scheduler].[AssignmentStatus] ([deleted])
GO

INSERT INTO [Scheduler].[AssignmentStatus] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Planned', 'Scheduled but not started', 1, '82fff66d-f6b4-44fe-9892-c7415cd0d401' )
GO

INSERT INTO [Scheduler].[AssignmentStatus] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'In Progress', 'Started', 2, '34183a16-1a64-4106-b28e-db454b06b5a6' )
GO

INSERT INTO [Scheduler].[AssignmentStatus] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Completed', 'Finished successfully', 3, '765c3c6d-782b-4393-bdab-cbf2a4a34eb6' )
GO

INSERT INTO [Scheduler].[AssignmentStatus] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'No-Show', 'Patient/resource didn''t appear', 4, '121271a6-7d93-4460-909f-2dc6e618538f' )
GO

INSERT INTO [Scheduler].[AssignmentStatus] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Canceled', 'Explicitly canceled', 5, 'cb14a7ad-fe10-4b2b-996c-7b5598810608' )
GO


-- Master list of scheduling target categories (e.g., Project, Patient, Customer). Used for UI grouping and filtering.
CREATE TABLE [Scheduler].[SchedulingTargetType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SchedulingTargetType_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_SchedulingTargetType_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the SchedulingTargetType table's tenantGuid and name fields.
)
GO

-- Index on the SchedulingTargetType table's tenantGuid field.
CREATE INDEX [I_SchedulingTargetType_tenantGuid] ON [Scheduler].[SchedulingTargetType] ([tenantGuid])
GO

-- Index on the SchedulingTargetType table's tenantGuid,name fields.
CREATE INDEX [I_SchedulingTargetType_tenantGuid_name] ON [Scheduler].[SchedulingTargetType] ([tenantGuid], [name])
GO

-- Index on the SchedulingTargetType table's tenantGuid,iconId fields.
CREATE INDEX [I_SchedulingTargetType_tenantGuid_iconId] ON [Scheduler].[SchedulingTargetType] ([tenantGuid], [iconId])
GO

-- Index on the SchedulingTargetType table's tenantGuid,active fields.
CREATE INDEX [I_SchedulingTargetType_tenantGuid_active] ON [Scheduler].[SchedulingTargetType] ([tenantGuid], [active])
GO

-- Index on the SchedulingTargetType table's tenantGuid,deleted fields.
CREATE INDEX [I_SchedulingTargetType_tenantGuid_deleted] ON [Scheduler].[SchedulingTargetType] ([tenantGuid], [deleted])
GO

INSERT INTO [Scheduler].[SchedulingTargetType] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Construction Project', 'A construction job with one or more sites', 1, '0ceaf00d-c58f-48a6-a18e-9a3e07452a23' )
GO

INSERT INTO [Scheduler].[SchedulingTargetType] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Patient', 'Healthcare patient with multiple care locations', 2, '7e14d7a8-f13d-4524-a679-6cbae24d9d97' )
GO

INSERT INTO [Scheduler].[SchedulingTargetType] ( [tenantGuid], [name], [description], [sequence], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Service Customer', 'Field service customer with multiple service addresses', 3, '6b3aa295-a54b-45dd-bda5-d75d157f376c' )
GO


-- The core container that ScheduledEvents are scheduled into.   Examples: a construction project, a healthcare patient, a service customer.  Supports multiple addresses and recurring scheduling patterns.
CREATE TABLE [Scheduler].[SchedulingTarget]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[officeId] INT NULL,		-- Optional office binding for a scheduling target.
	[clientId] INT NOT NULL,		-- The client that this scheduling target belongs to.
	[schedulingTargetTypeId] INT NOT NULL,		-- Link to the SchedulingTargetType table.
	[timeZoneId] INT NOT NULL,		-- Link to the TimeZone table.
	[calendarId] INT NULL,		-- An optional default calendar for this scheduling target.
	[notes] NVARCHAR(MAX) NULL,
	[externalId] NVARCHAR(100) NULL,		-- Optional reference to an ID in an external system
	[color] NVARCHAR(10) NULL,		-- Override of Target Type Hex color for UI display
	[attributes] NVARCHAR(MAX) NULL,		-- to store arbitrary JSON
	[avatarFileName] NVARCHAR(250) NULL,		-- Part of the binary data field setup
	[avatarSize] BIGINT NULL,		-- Part of the binary data field setup
	[avatarData] VARBINARY(MAX) NULL,		-- Part of the binary data field setup
	[avatarMimeType] NVARCHAR(100) NULL,		-- Part of the binary data field setup
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SchedulingTarget_Office_officeId] FOREIGN KEY ([officeId]) REFERENCES [Scheduler].[Office] ([id]),		-- Foreign key to the Office table.
	CONSTRAINT [FK_SchedulingTarget_Client_clientId] FOREIGN KEY ([clientId]) REFERENCES [Scheduler].[Client] ([id]),		-- Foreign key to the Client table.
	CONSTRAINT [FK_SchedulingTarget_SchedulingTargetType_schedulingTargetTypeId] FOREIGN KEY ([schedulingTargetTypeId]) REFERENCES [Scheduler].[SchedulingTargetType] ([id]),		-- Foreign key to the SchedulingTargetType table.
	CONSTRAINT [FK_SchedulingTarget_TimeZone_timeZoneId] FOREIGN KEY ([timeZoneId]) REFERENCES [Scheduler].[TimeZone] ([id]),		-- Foreign key to the TimeZone table.
	CONSTRAINT [FK_SchedulingTarget_Calendar_calendarId] FOREIGN KEY ([calendarId]) REFERENCES [Scheduler].[Calendar] ([id]),		-- Foreign key to the Calendar table.
	CONSTRAINT [UC_SchedulingTarget_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the SchedulingTarget table's tenantGuid and name fields.
)
GO

-- Index on the SchedulingTarget table's tenantGuid field.
CREATE INDEX [I_SchedulingTarget_tenantGuid] ON [Scheduler].[SchedulingTarget] ([tenantGuid])
GO

-- Index on the SchedulingTarget table's tenantGuid,name fields.
CREATE INDEX [I_SchedulingTarget_tenantGuid_name] ON [Scheduler].[SchedulingTarget] ([tenantGuid], [name])
GO

-- Index on the SchedulingTarget table's tenantGuid,officeId fields.
CREATE INDEX [I_SchedulingTarget_tenantGuid_officeId] ON [Scheduler].[SchedulingTarget] ([tenantGuid], [officeId])
GO

-- Index on the SchedulingTarget table's tenantGuid,clientId fields.
CREATE INDEX [I_SchedulingTarget_tenantGuid_clientId] ON [Scheduler].[SchedulingTarget] ([tenantGuid], [clientId])
GO

-- Index on the SchedulingTarget table's tenantGuid,schedulingTargetTypeId fields.
CREATE INDEX [I_SchedulingTarget_tenantGuid_schedulingTargetTypeId] ON [Scheduler].[SchedulingTarget] ([tenantGuid], [schedulingTargetTypeId])
GO

-- Index on the SchedulingTarget table's tenantGuid,timeZoneId fields.
CREATE INDEX [I_SchedulingTarget_tenantGuid_timeZoneId] ON [Scheduler].[SchedulingTarget] ([tenantGuid], [timeZoneId])
GO

-- Index on the SchedulingTarget table's tenantGuid,active fields.
CREATE INDEX [I_SchedulingTarget_tenantGuid_active] ON [Scheduler].[SchedulingTarget] ([tenantGuid], [active])
GO

-- Index on the SchedulingTarget table's tenantGuid,deleted fields.
CREATE INDEX [I_SchedulingTarget_tenantGuid_deleted] ON [Scheduler].[SchedulingTarget] ([tenantGuid], [deleted])
GO


-- The change history for records from the SchedulingTarget table.
CREATE TABLE [Scheduler].[SchedulingTargetChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[schedulingTargetId] INT NOT NULL,		-- Link to the SchedulingTarget table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_SchedulingTargetChangeHistory_SchedulingTarget_schedulingTargetId] FOREIGN KEY ([schedulingTargetId]) REFERENCES [Scheduler].[SchedulingTarget] ([id])		-- Foreign key to the SchedulingTarget table.
)
GO

-- Index on the SchedulingTargetChangeHistory table's tenantGuid field.
CREATE INDEX [I_SchedulingTargetChangeHistory_tenantGuid] ON [Scheduler].[SchedulingTargetChangeHistory] ([tenantGuid])
GO

-- Index on the SchedulingTargetChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_SchedulingTargetChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[SchedulingTargetChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the SchedulingTargetChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_SchedulingTargetChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[SchedulingTargetChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the SchedulingTargetChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_SchedulingTargetChangeHistory_tenantGuid_userId] ON [Scheduler].[SchedulingTargetChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the SchedulingTargetChangeHistory table's tenantGuid,schedulingTargetId fields.
CREATE INDEX [I_SchedulingTargetChangeHistory_tenantGuid_schedulingTargetId] ON [Scheduler].[SchedulingTargetChangeHistory] ([tenantGuid], [schedulingTargetId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- The link between scheduling targets and contacts.
CREATE TABLE [Scheduler].[SchedulingTargetContact]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[schedulingTargetId] INT NOT NULL,		-- Link to the SchedulingTarget table.
	[contactId] INT NOT NULL,		-- Link to the Contact table.
	[isPrimary] BIT NOT NULL DEFAULT 0,		-- Indicates whether or not this contact should be considered a primary contact of the scheduling target.
	[relationshipTypeId] INT NOT NULL,		-- A description of the relationship between the scheduling target and the contact.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SchedulingTargetContact_SchedulingTarget_schedulingTargetId] FOREIGN KEY ([schedulingTargetId]) REFERENCES [Scheduler].[SchedulingTarget] ([id]),		-- Foreign key to the SchedulingTarget table.
	CONSTRAINT [FK_SchedulingTargetContact_Contact_contactId] FOREIGN KEY ([contactId]) REFERENCES [Scheduler].[Contact] ([id]),		-- Foreign key to the Contact table.
	CONSTRAINT [FK_SchedulingTargetContact_RelationshipType_relationshipTypeId] FOREIGN KEY ([relationshipTypeId]) REFERENCES [Scheduler].[RelationshipType] ([id]),		-- Foreign key to the RelationshipType table.
	CONSTRAINT [UC_SchedulingTargetContact_tenantGuid_schedulingTargetId_contactId] UNIQUE ( [tenantGuid], [schedulingTargetId], [contactId]) 		-- Uniqueness enforced on the SchedulingTargetContact table's tenantGuid and schedulingTargetId and contactId fields.
)
GO

-- Index on the SchedulingTargetContact table's tenantGuid field.
CREATE INDEX [I_SchedulingTargetContact_tenantGuid] ON [Scheduler].[SchedulingTargetContact] ([tenantGuid])
GO

-- Index on the SchedulingTargetContact table's tenantGuid,schedulingTargetId fields.
CREATE INDEX [I_SchedulingTargetContact_tenantGuid_schedulingTargetId] ON [Scheduler].[SchedulingTargetContact] ([tenantGuid], [schedulingTargetId])
GO

-- Index on the SchedulingTargetContact table's tenantGuid,contactId fields.
CREATE INDEX [I_SchedulingTargetContact_tenantGuid_contactId] ON [Scheduler].[SchedulingTargetContact] ([tenantGuid], [contactId])
GO

-- Index on the SchedulingTargetContact table's tenantGuid,relationshipTypeId fields.
CREATE INDEX [I_SchedulingTargetContact_tenantGuid_relationshipTypeId] ON [Scheduler].[SchedulingTargetContact] ([tenantGuid], [relationshipTypeId])
GO

-- Index on the SchedulingTargetContact table's tenantGuid,active fields.
CREATE INDEX [I_SchedulingTargetContact_tenantGuid_active] ON [Scheduler].[SchedulingTargetContact] ([tenantGuid], [active])
GO

-- Index on the SchedulingTargetContact table's tenantGuid,deleted fields.
CREATE INDEX [I_SchedulingTargetContact_tenantGuid_deleted] ON [Scheduler].[SchedulingTargetContact] ([tenantGuid], [deleted])
GO


-- The change history for records from the SchedulingTargetContact table.
CREATE TABLE [Scheduler].[SchedulingTargetContactChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[schedulingTargetContactId] INT NOT NULL,		-- Link to the SchedulingTargetContact table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_SchedulingTargetContactChangeHistory_SchedulingTargetContact_schedulingTargetContactId] FOREIGN KEY ([schedulingTargetContactId]) REFERENCES [Scheduler].[SchedulingTargetContact] ([id])		-- Foreign key to the SchedulingTargetContact table.
)
GO

-- Index on the SchedulingTargetContactChangeHistory table's tenantGuid field.
CREATE INDEX [I_SchedulingTargetContactChangeHistory_tenantGuid] ON [Scheduler].[SchedulingTargetContactChangeHistory] ([tenantGuid])
GO

-- Index on the SchedulingTargetContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_SchedulingTargetContactChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[SchedulingTargetContactChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the SchedulingTargetContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_SchedulingTargetContactChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[SchedulingTargetContactChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the SchedulingTargetContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_SchedulingTargetContactChangeHistory_tenantGuid_userId] ON [Scheduler].[SchedulingTargetContactChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the SchedulingTargetContactChangeHistory table's tenantGuid,schedulingTargetContactId fields.
CREATE INDEX [I_SchedulingTargetContactChangeHistory_tenantGuid_schedulingTargetContactId] ON [Scheduler].[SchedulingTargetContactChangeHistory] ([tenantGuid], [schedulingTargetContactId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Links SchedulingTargets to multiple addresses (e.g., multiple job sites, patient home + hospital).
CREATE TABLE [Scheduler].[SchedulingTargetAddress]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[schedulingTargetId] INT NOT NULL,		-- Primary  schuduling target for this address - could be null if there is a client linked to this, so the address would be for all targets in the client.
	[clientId] INT NULL,		-- Optional client level link.  The presence of a value here indicates that the address is to be shared across all scheduling targets for the client.
	[addressLine1] NVARCHAR(250) NOT NULL,
	[addressLine2] NVARCHAR(250) NULL,
	[city] NVARCHAR(100) NOT NULL,
	[postalCode] NVARCHAR(100) NULL,
	[stateProvinceId] INT NOT NULL,		-- Link to the StateProvince table.
	[countryId] INT NOT NULL,		-- Link to the Country table.
	[latitude] FLOAT NULL,		-- Optional latitude position
	[longitude] FLOAT NULL,		-- Optional longitude position
	[label] NVARCHAR(250) NULL,		-- e.g., 'Main Site', 'Patient Home', 'Hospital Ward'
	[isPrimary] BIT NOT NULL DEFAULT 0,		-- Whether or not this is the scheduling target's main address.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SchedulingTargetAddress_SchedulingTarget_schedulingTargetId] FOREIGN KEY ([schedulingTargetId]) REFERENCES [Scheduler].[SchedulingTarget] ([id]),		-- Foreign key to the SchedulingTarget table.
	CONSTRAINT [FK_SchedulingTargetAddress_Client_clientId] FOREIGN KEY ([clientId]) REFERENCES [Scheduler].[Client] ([id]),		-- Foreign key to the Client table.
	CONSTRAINT [FK_SchedulingTargetAddress_StateProvince_stateProvinceId] FOREIGN KEY ([stateProvinceId]) REFERENCES [Scheduler].[StateProvince] ([id]),		-- Foreign key to the StateProvince table.
	CONSTRAINT [FK_SchedulingTargetAddress_Country_countryId] FOREIGN KEY ([countryId]) REFERENCES [Scheduler].[Country] ([id]),		-- Foreign key to the Country table.
	CONSTRAINT [UC_SchedulingTargetAddress_tenantGuid_schedulingTargetId_addressLine1_city_postalCode] UNIQUE ( [tenantGuid], [schedulingTargetId], [addressLine1], [city], [postalCode]) 		-- Uniqueness enforced on the SchedulingTargetAddress table's tenantGuid and schedulingTargetId and addressLine1 and city and postalCode fields.
)
GO

-- Index on the SchedulingTargetAddress table's tenantGuid field.
CREATE INDEX [I_SchedulingTargetAddress_tenantGuid] ON [Scheduler].[SchedulingTargetAddress] ([tenantGuid])
GO

-- Index on the SchedulingTargetAddress table's tenantGuid,schedulingTargetId fields.
CREATE INDEX [I_SchedulingTargetAddress_tenantGuid_schedulingTargetId] ON [Scheduler].[SchedulingTargetAddress] ([tenantGuid], [schedulingTargetId])
GO

-- Index on the SchedulingTargetAddress table's tenantGuid,clientId fields.
CREATE INDEX [I_SchedulingTargetAddress_tenantGuid_clientId] ON [Scheduler].[SchedulingTargetAddress] ([tenantGuid], [clientId])
GO

-- Index on the SchedulingTargetAddress table's tenantGuid,stateProvinceId fields.
CREATE INDEX [I_SchedulingTargetAddress_tenantGuid_stateProvinceId] ON [Scheduler].[SchedulingTargetAddress] ([tenantGuid], [stateProvinceId])
GO

-- Index on the SchedulingTargetAddress table's tenantGuid,countryId fields.
CREATE INDEX [I_SchedulingTargetAddress_tenantGuid_countryId] ON [Scheduler].[SchedulingTargetAddress] ([tenantGuid], [countryId])
GO

-- Index on the SchedulingTargetAddress table's tenantGuid,active fields.
CREATE INDEX [I_SchedulingTargetAddress_tenantGuid_active] ON [Scheduler].[SchedulingTargetAddress] ([tenantGuid], [active])
GO

-- Index on the SchedulingTargetAddress table's tenantGuid,deleted fields.
CREATE INDEX [I_SchedulingTargetAddress_tenantGuid_deleted] ON [Scheduler].[SchedulingTargetAddress] ([tenantGuid], [deleted])
GO


-- The change history for records from the SchedulingTargetAddress table.
CREATE TABLE [Scheduler].[SchedulingTargetAddressChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[schedulingTargetAddressId] INT NOT NULL,		-- Link to the SchedulingTargetAddress table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_SchedulingTargetAddressChangeHistory_SchedulingTargetAddress_schedulingTargetAddressId] FOREIGN KEY ([schedulingTargetAddressId]) REFERENCES [Scheduler].[SchedulingTargetAddress] ([id])		-- Foreign key to the SchedulingTargetAddress table.
)
GO

-- Index on the SchedulingTargetAddressChangeHistory table's tenantGuid field.
CREATE INDEX [I_SchedulingTargetAddressChangeHistory_tenantGuid] ON [Scheduler].[SchedulingTargetAddressChangeHistory] ([tenantGuid])
GO

-- Index on the SchedulingTargetAddressChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_SchedulingTargetAddressChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[SchedulingTargetAddressChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the SchedulingTargetAddressChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_SchedulingTargetAddressChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[SchedulingTargetAddressChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the SchedulingTargetAddressChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_SchedulingTargetAddressChangeHistory_tenantGuid_userId] ON [Scheduler].[SchedulingTargetAddressChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the SchedulingTargetAddressChangeHistory table's tenantGuid,schedulingTargetAddressId fields.
CREATE INDEX [I_SchedulingTargetAddressChangeHistory_tenantGuid_schedulingTargetAddressId] ON [Scheduler].[SchedulingTargetAddressChangeHistory] ([tenantGuid], [schedulingTargetAddressId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Defines which qualifications are required (or preferred) for working on a specific SchedulingTarget.  - isRequired = true then resource MUST have qualification  - isRequired = false then nice-to-have (warning only)
CREATE TABLE [Scheduler].[SchedulingTargetQualificationRequirement]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[schedulingTargetId] INT NOT NULL,		-- Link to the SchedulingTarget table.
	[qualificationId] INT NOT NULL,		-- Link to the Qualification table.
	[isRequired] BIT NOT NULL DEFAULT 1,		-- true = mandatory, false = preferred
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SchedulingTargetQualificationRequirement_SchedulingTarget_schedulingTargetId] FOREIGN KEY ([schedulingTargetId]) REFERENCES [Scheduler].[SchedulingTarget] ([id]),		-- Foreign key to the SchedulingTarget table.
	CONSTRAINT [FK_SchedulingTargetQualificationRequirement_Qualification_qualificationId] FOREIGN KEY ([qualificationId]) REFERENCES [Scheduler].[Qualification] ([id]),		-- Foreign key to the Qualification table.
	CONSTRAINT [UC_SchedulingTargetQualificationRequirement_tenantGuid_schedulingTargetId_qualificationId] UNIQUE ( [tenantGuid], [schedulingTargetId], [qualificationId]) 		-- Uniqueness enforced on the SchedulingTargetQualificationRequirement table's tenantGuid and schedulingTargetId and qualificationId fields.
)
GO

-- Index on the SchedulingTargetQualificationRequirement table's tenantGuid field.
CREATE INDEX [I_SchedulingTargetQualificationRequirement_tenantGuid] ON [Scheduler].[SchedulingTargetQualificationRequirement] ([tenantGuid])
GO

-- Index on the SchedulingTargetQualificationRequirement table's tenantGuid,schedulingTargetId fields.
CREATE INDEX [I_SchedulingTargetQualificationRequirement_tenantGuid_schedulingTargetId] ON [Scheduler].[SchedulingTargetQualificationRequirement] ([tenantGuid], [schedulingTargetId])
GO

-- Index on the SchedulingTargetQualificationRequirement table's tenantGuid,qualificationId fields.
CREATE INDEX [I_SchedulingTargetQualificationRequirement_tenantGuid_qualificationId] ON [Scheduler].[SchedulingTargetQualificationRequirement] ([tenantGuid], [qualificationId])
GO

-- Index on the SchedulingTargetQualificationRequirement table's tenantGuid,active fields.
CREATE INDEX [I_SchedulingTargetQualificationRequirement_tenantGuid_active] ON [Scheduler].[SchedulingTargetQualificationRequirement] ([tenantGuid], [active])
GO

-- Index on the SchedulingTargetQualificationRequirement table's tenantGuid,deleted fields.
CREATE INDEX [I_SchedulingTargetQualificationRequirement_tenantGuid_deleted] ON [Scheduler].[SchedulingTargetQualificationRequirement] ([tenantGuid], [deleted])
GO


-- The change history for records from the SchedulingTargetQualificationRequirement table.
CREATE TABLE [Scheduler].[SchedulingTargetQualificationRequirementChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[schedulingTargetQualificationRequirementId] INT NOT NULL,		-- Link to the SchedulingTargetQualificationRequirement table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_SchdulngTrgtQulfctnRqurmntChngHstry_SchdulngTrgtQulfctnRqurmnt_schdulngTrgtQulfctnRqurmntd] FOREIGN KEY ([schedulingTargetQualificationRequirementId]) REFERENCES [Scheduler].[SchedulingTargetQualificationRequirement] ([id])		-- Foreign key to the SchedulingTargetQualificationRequirement table.
)
GO

-- Index on the SchedulingTargetQualificationRequirementChangeHistory table's tenantGuid field.
CREATE INDEX [I_SchedulingTargetQualificationRequirementChangeHistory_tenantGuid] ON [Scheduler].[SchedulingTargetQualificationRequirementChangeHistory] ([tenantGuid])
GO

-- Index on the SchedulingTargetQualificationRequirementChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_SchedulingTargetQualificationRequirementChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[SchedulingTargetQualificationRequirementChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the SchedulingTargetQualificationRequirementChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_SchedulingTargetQualificationRequirementChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[SchedulingTargetQualificationRequirementChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the SchedulingTargetQualificationRequirementChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_SchedulingTargetQualificationRequirementChangeHistory_tenantGuid_userId] ON [Scheduler].[SchedulingTargetQualificationRequirementChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the SchedulingTargetQualificationRequirementChangeHistory table's tenantGuid,schedulingTargetQualificationRequirementId fields.
CREATE INDEX [I_SchedulingTargetQualificationRequirementChangeHistory_tenantGuid_schedulingTargetQualificationRequirementId] ON [Scheduler].[SchedulingTargetQualificationRequirementChangeHistory] ([tenantGuid], [schedulingTargetQualificationRequirementId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Master list of recurrence frequencies. Mirrors common iCalendar frequencies.
CREATE TABLE [Scheduler].[RecurrenceFrequency]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the RecurrenceFrequency table's name field.
CREATE INDEX [I_RecurrenceFrequency_name] ON [Scheduler].[RecurrenceFrequency] ([name])
GO

-- Index on the RecurrenceFrequency table's active field.
CREATE INDEX [I_RecurrenceFrequency_active] ON [Scheduler].[RecurrenceFrequency] ([active])
GO

-- Index on the RecurrenceFrequency table's deleted field.
CREATE INDEX [I_RecurrenceFrequency_deleted] ON [Scheduler].[RecurrenceFrequency] ([deleted])
GO

INSERT INTO [Scheduler].[RecurrenceFrequency] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Once', 'Does not repeat', 1, 'a2e0f727-8e79-4add-af0a-495e89a4c6b7' )
GO

INSERT INTO [Scheduler].[RecurrenceFrequency] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Daily', 'Repeats every day or every N days', 2, 'bd28a0b1-26cf-4973-9129-bcd1cc5c9f67' )
GO

INSERT INTO [Scheduler].[RecurrenceFrequency] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Weekly', 'Repeats every week on selected days', 3, '044f3c91-7745-467a-955a-809acdc0dba7' )
GO

INSERT INTO [Scheduler].[RecurrenceFrequency] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Monthly', 'Repeats monthly (by day of month or day of week)', 4, 'fa0a9d3f-86e2-46c1-9a14-ea3858facf09' )
GO

INSERT INTO [Scheduler].[RecurrenceFrequency] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Yearly', 'Repeats annually', 5, '3ffeb2e0-0ced-4fc2-a268-bb31a3f5a861' )
GO


-- Defines a recurrence pattern for a ScheduledEvent.  One ScheduledEvent can have zero or one RecurrenceRule (for recurring series).  Instances are generated on-the-fly or materialized as needed.
CREATE TABLE [Scheduler].[RecurrenceRule]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[recurrenceFrequencyId] INT NOT NULL,		-- Link to the RecurrenceFrequency table.
	[interval] INT NOT NULL DEFAULT 1,		-- How often the pattern repeats (e.g., every 2 weeks)
	[untilDateTime] DATETIME2(7) NULL,		-- Recurrence ends on this date (inclusive). NULL = no end date
	[count] INT NULL,		-- Maximum number of occurrences. NULL = unlimited
	[dayOfWeekMask] INT NULL DEFAULT 0,		-- Bitmask for weekly recurrence:  1 = Sunday, 2 = Monday, 4 = Tuesday, 8 = Wednesday, 16 = Thursday, 32 = Friday, 64 = Saturday Example: Monday + Wednesday + Friday = 2 + 8 + 32 = 42
	[dayOfMonth] INT NULL,		-- For monthly: specific day (1-31). NULL if using dayOfWeekInMonth
	[dayOfWeekInMonth] INT NULL,		-- Values: 1 = first, 2 = second, 3 = third, 4 = fourth, 5 = last, -1 = second-to-last, etc. Combine with dayOfWeekMask.  
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_RecurrenceRule_RecurrenceFrequency_recurrenceFrequencyId] FOREIGN KEY ([recurrenceFrequencyId]) REFERENCES [Scheduler].[RecurrenceFrequency] ([id])		-- Foreign key to the RecurrenceFrequency table.
)
GO

-- Index on the RecurrenceRule table's tenantGuid field.
CREATE INDEX [I_RecurrenceRule_tenantGuid] ON [Scheduler].[RecurrenceRule] ([tenantGuid])
GO

-- Index on the RecurrenceRule table's tenantGuid,recurrenceFrequencyId fields.
CREATE INDEX [I_RecurrenceRule_tenantGuid_recurrenceFrequencyId] ON [Scheduler].[RecurrenceRule] ([tenantGuid], [recurrenceFrequencyId])
GO

-- Index on the RecurrenceRule table's tenantGuid,active fields.
CREATE INDEX [I_RecurrenceRule_tenantGuid_active] ON [Scheduler].[RecurrenceRule] ([tenantGuid], [active])
GO

-- Index on the RecurrenceRule table's tenantGuid,deleted fields.
CREATE INDEX [I_RecurrenceRule_tenantGuid_deleted] ON [Scheduler].[RecurrenceRule] ([tenantGuid], [deleted])
GO


-- The change history for records from the RecurrenceRule table.
CREATE TABLE [Scheduler].[RecurrenceRuleChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[recurrenceRuleId] INT NOT NULL,		-- Link to the RecurrenceRule table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_RecurrenceRuleChangeHistory_RecurrenceRule_recurrenceRuleId] FOREIGN KEY ([recurrenceRuleId]) REFERENCES [Scheduler].[RecurrenceRule] ([id])		-- Foreign key to the RecurrenceRule table.
)
GO

-- Index on the RecurrenceRuleChangeHistory table's tenantGuid field.
CREATE INDEX [I_RecurrenceRuleChangeHistory_tenantGuid] ON [Scheduler].[RecurrenceRuleChangeHistory] ([tenantGuid])
GO

-- Index on the RecurrenceRuleChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_RecurrenceRuleChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[RecurrenceRuleChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the RecurrenceRuleChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_RecurrenceRuleChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[RecurrenceRuleChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the RecurrenceRuleChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_RecurrenceRuleChangeHistory_tenantGuid_userId] ON [Scheduler].[RecurrenceRuleChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the RecurrenceRuleChangeHistory table's tenantGuid,recurrenceRuleId fields.
CREATE INDEX [I_RecurrenceRuleChangeHistory_tenantGuid_recurrenceRuleId] ON [Scheduler].[RecurrenceRuleChangeHistory] ([tenantGuid], [recurrenceRuleId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Reusable standard shift patterns (e.g., 'Day Shift', 'Night Shift', 'Weekend Crew').  Resources can be assigned to a pattern, or have custom overrides.
CREATE TABLE [Scheduler].[ShiftPattern]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[timeZoneId] INT NULL,		-- Link to the TimeZone table.
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ShiftPattern_TimeZone_timeZoneId] FOREIGN KEY ([timeZoneId]) REFERENCES [Scheduler].[TimeZone] ([id]),		-- Foreign key to the TimeZone table.
	CONSTRAINT [UC_ShiftPattern_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the ShiftPattern table's tenantGuid and name fields.
)
GO

-- Index on the ShiftPattern table's tenantGuid field.
CREATE INDEX [I_ShiftPattern_tenantGuid] ON [Scheduler].[ShiftPattern] ([tenantGuid])
GO

-- Index on the ShiftPattern table's tenantGuid,name fields.
CREATE INDEX [I_ShiftPattern_tenantGuid_name] ON [Scheduler].[ShiftPattern] ([tenantGuid], [name])
GO

-- Index on the ShiftPattern table's tenantGuid,timeZoneId fields.
CREATE INDEX [I_ShiftPattern_tenantGuid_timeZoneId] ON [Scheduler].[ShiftPattern] ([tenantGuid], [timeZoneId])
GO

-- Index on the ShiftPattern table's tenantGuid,active fields.
CREATE INDEX [I_ShiftPattern_tenantGuid_active] ON [Scheduler].[ShiftPattern] ([tenantGuid], [active])
GO

-- Index on the ShiftPattern table's tenantGuid,deleted fields.
CREATE INDEX [I_ShiftPattern_tenantGuid_deleted] ON [Scheduler].[ShiftPattern] ([tenantGuid], [deleted])
GO


-- The change history for records from the ShiftPattern table.
CREATE TABLE [Scheduler].[ShiftPatternChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[shiftPatternId] INT NOT NULL,		-- Link to the ShiftPattern table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ShiftPatternChangeHistory_ShiftPattern_shiftPatternId] FOREIGN KEY ([shiftPatternId]) REFERENCES [Scheduler].[ShiftPattern] ([id])		-- Foreign key to the ShiftPattern table.
)
GO

-- Index on the ShiftPatternChangeHistory table's tenantGuid field.
CREATE INDEX [I_ShiftPatternChangeHistory_tenantGuid] ON [Scheduler].[ShiftPatternChangeHistory] ([tenantGuid])
GO

-- Index on the ShiftPatternChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ShiftPatternChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ShiftPatternChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ShiftPatternChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ShiftPatternChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ShiftPatternChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ShiftPatternChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ShiftPatternChangeHistory_tenantGuid_userId] ON [Scheduler].[ShiftPatternChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ShiftPatternChangeHistory table's tenantGuid,shiftPatternId fields.
CREATE INDEX [I_ShiftPatternChangeHistory_tenantGuid_shiftPatternId] ON [Scheduler].[ShiftPatternChangeHistory] ([tenantGuid], [shiftPatternId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Defines the days and availability windows for a ShiftPattern.
CREATE TABLE [Scheduler].[ShiftPatternDay]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[shiftPatternId] INT NOT NULL,		-- Link to the ShiftPattern table.
	[dayOfWeek] INT NOT NULL DEFAULT 1,		-- Day this rule applies to   1=Sunday..7=Saturday
	[startTime] TIME(7) NOT NULL,		-- Start of available window (local to pattern time zone) e.g., 07:00:00
	[hours] REAL NOT NULL DEFAULT 8,		-- Hours available from start time (handles overnight shifts cleanly)  Defaults to 8
	[label] NVARCHAR(250) NULL,		-- e.g., Main Shift
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ShiftPatternDay_ShiftPattern_shiftPatternId] FOREIGN KEY ([shiftPatternId]) REFERENCES [Scheduler].[ShiftPattern] ([id]),		-- Foreign key to the ShiftPattern table.
	CONSTRAINT [UC_ShiftPatternDay_tenantGuid_shiftPatternId_dayOfWeek] UNIQUE ( [tenantGuid], [shiftPatternId], [dayOfWeek]) 		-- Uniqueness enforced on the ShiftPatternDay table's tenantGuid and shiftPatternId and dayOfWeek fields.
)
GO

-- Index on the ShiftPatternDay table's tenantGuid field.
CREATE INDEX [I_ShiftPatternDay_tenantGuid] ON [Scheduler].[ShiftPatternDay] ([tenantGuid])
GO

-- Index on the ShiftPatternDay table's tenantGuid,shiftPatternId fields.
CREATE INDEX [I_ShiftPatternDay_tenantGuid_shiftPatternId] ON [Scheduler].[ShiftPatternDay] ([tenantGuid], [shiftPatternId])
GO

-- Index on the ShiftPatternDay table's tenantGuid,active fields.
CREATE INDEX [I_ShiftPatternDay_tenantGuid_active] ON [Scheduler].[ShiftPatternDay] ([tenantGuid], [active])
GO

-- Index on the ShiftPatternDay table's tenantGuid,deleted fields.
CREATE INDEX [I_ShiftPatternDay_tenantGuid_deleted] ON [Scheduler].[ShiftPatternDay] ([tenantGuid], [deleted])
GO


-- The change history for records from the ShiftPatternDay table.
CREATE TABLE [Scheduler].[ShiftPatternDayChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[shiftPatternDayId] INT NOT NULL,		-- Link to the ShiftPatternDay table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ShiftPatternDayChangeHistory_ShiftPatternDay_shiftPatternDayId] FOREIGN KEY ([shiftPatternDayId]) REFERENCES [Scheduler].[ShiftPatternDay] ([id])		-- Foreign key to the ShiftPatternDay table.
)
GO

-- Index on the ShiftPatternDayChangeHistory table's tenantGuid field.
CREATE INDEX [I_ShiftPatternDayChangeHistory_tenantGuid] ON [Scheduler].[ShiftPatternDayChangeHistory] ([tenantGuid])
GO

-- Index on the ShiftPatternDayChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ShiftPatternDayChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ShiftPatternDayChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ShiftPatternDayChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ShiftPatternDayChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ShiftPatternDayChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ShiftPatternDayChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ShiftPatternDayChangeHistory_tenantGuid_userId] ON [Scheduler].[ShiftPatternDayChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ShiftPatternDayChangeHistory table's tenantGuid,shiftPatternDayId fields.
CREATE INDEX [I_ShiftPatternDayChangeHistory_tenantGuid_shiftPatternDayId] ON [Scheduler].[ShiftPatternDayChangeHistory] ([tenantGuid], [shiftPatternDayId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- The schedulable entities – people and assets.  Examples: 'John Doe (Operator)', 'CAT CP56B Roller #12', 'Conference Room A'.
CREATE TABLE [Scheduler].[Resource]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[officeId] INT NULL,		-- Optional office binding for a resource.
	[resourceTypeId] INT NOT NULL,		-- Link to the ResourceType table.
	[shiftPatternId] INT NULL,		-- Standard shift pattern this resource follows (NULL = custom shifts via ResourceShift)
	[timeZoneId] INT NOT NULL,		-- Link to the TimeZone table.
	[targetWeeklyWorkHours] REAL NULL,
	[notes] NVARCHAR(MAX) NULL,
	[externalId] NVARCHAR(100) NULL,		-- Optional reference to an ID in an external system
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[attributes] NVARCHAR(MAX) NULL,		-- to store arbitrary JSON
	[avatarFileName] NVARCHAR(250) NULL,		-- Part of the binary data field setup
	[avatarSize] BIGINT NULL,		-- Part of the binary data field setup
	[avatarData] VARBINARY(MAX) NULL,		-- Part of the binary data field setup
	[avatarMimeType] NVARCHAR(100) NULL,		-- Part of the binary data field setup
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Resource_Office_officeId] FOREIGN KEY ([officeId]) REFERENCES [Scheduler].[Office] ([id]),		-- Foreign key to the Office table.
	CONSTRAINT [FK_Resource_ResourceType_resourceTypeId] FOREIGN KEY ([resourceTypeId]) REFERENCES [Scheduler].[ResourceType] ([id]),		-- Foreign key to the ResourceType table.
	CONSTRAINT [FK_Resource_ShiftPattern_shiftPatternId] FOREIGN KEY ([shiftPatternId]) REFERENCES [Scheduler].[ShiftPattern] ([id]),		-- Foreign key to the ShiftPattern table.
	CONSTRAINT [FK_Resource_TimeZone_timeZoneId] FOREIGN KEY ([timeZoneId]) REFERENCES [Scheduler].[TimeZone] ([id]),		-- Foreign key to the TimeZone table.
	CONSTRAINT [UC_Resource_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Resource table's tenantGuid and name fields.
)
GO

-- Index on the Resource table's tenantGuid field.
CREATE INDEX [I_Resource_tenantGuid] ON [Scheduler].[Resource] ([tenantGuid])
GO

-- Index on the Resource table's tenantGuid,name fields.
CREATE INDEX [I_Resource_tenantGuid_name] ON [Scheduler].[Resource] ([tenantGuid], [name])
GO

-- Index on the Resource table's tenantGuid,officeId fields.
CREATE INDEX [I_Resource_tenantGuid_officeId] ON [Scheduler].[Resource] ([tenantGuid], [officeId])
GO

-- Index on the Resource table's tenantGuid,resourceTypeId fields.
CREATE INDEX [I_Resource_tenantGuid_resourceTypeId] ON [Scheduler].[Resource] ([tenantGuid], [resourceTypeId])
GO

-- Index on the Resource table's tenantGuid,shiftPatternId fields.
CREATE INDEX [I_Resource_tenantGuid_shiftPatternId] ON [Scheduler].[Resource] ([tenantGuid], [shiftPatternId])
GO

-- Index on the Resource table's tenantGuid,timeZoneId fields.
CREATE INDEX [I_Resource_tenantGuid_timeZoneId] ON [Scheduler].[Resource] ([tenantGuid], [timeZoneId])
GO

-- Index on the Resource table's tenantGuid,active fields.
CREATE INDEX [I_Resource_tenantGuid_active] ON [Scheduler].[Resource] ([tenantGuid], [active])
GO

-- Index on the Resource table's tenantGuid,deleted fields.
CREATE INDEX [I_Resource_tenantGuid_deleted] ON [Scheduler].[Resource] ([tenantGuid], [deleted])
GO

-- Index on the Resource table's tenantGuid,externalId fields.
CREATE INDEX [I_Resource_tenantGuid_externalId] ON [Scheduler].[Resource] ([tenantGuid], [externalId])
GO


-- The change history for records from the Resource table.
CREATE TABLE [Scheduler].[ResourceChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[resourceId] INT NOT NULL,		-- Link to the Resource table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ResourceChangeHistory_Resource_resourceId] FOREIGN KEY ([resourceId]) REFERENCES [Scheduler].[Resource] ([id])		-- Foreign key to the Resource table.
)
GO

-- Index on the ResourceChangeHistory table's tenantGuid field.
CREATE INDEX [I_ResourceChangeHistory_tenantGuid] ON [Scheduler].[ResourceChangeHistory] ([tenantGuid])
GO

-- Index on the ResourceChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ResourceChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ResourceChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ResourceChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ResourceChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ResourceChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ResourceChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ResourceChangeHistory_tenantGuid_userId] ON [Scheduler].[ResourceChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ResourceChangeHistory table's tenantGuid,resourceId fields.
CREATE INDEX [I_ResourceChangeHistory_tenantGuid_resourceId] ON [Scheduler].[ResourceChangeHistory] ([tenantGuid], [resourceId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- The link between scheduling targets and contacts.
CREATE TABLE [Scheduler].[ResourceContact]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[resourceId] INT NOT NULL,		-- Link to the Resource table.
	[contactId] INT NOT NULL,		-- Link to the Contact table.
	[isPrimary] BIT NOT NULL DEFAULT 0,		-- Indicates whether or not this contact should be considered a primary contact of the resource.
	[relationshipTypeId] INT NOT NULL,		-- A description of the relationship between the resource and the contact.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ResourceContact_Resource_resourceId] FOREIGN KEY ([resourceId]) REFERENCES [Scheduler].[Resource] ([id]),		-- Foreign key to the Resource table.
	CONSTRAINT [FK_ResourceContact_Contact_contactId] FOREIGN KEY ([contactId]) REFERENCES [Scheduler].[Contact] ([id]),		-- Foreign key to the Contact table.
	CONSTRAINT [FK_ResourceContact_RelationshipType_relationshipTypeId] FOREIGN KEY ([relationshipTypeId]) REFERENCES [Scheduler].[RelationshipType] ([id]),		-- Foreign key to the RelationshipType table.
	CONSTRAINT [UC_ResourceContact_tenantGuid_resourceId_contactId] UNIQUE ( [tenantGuid], [resourceId], [contactId]) 		-- Uniqueness enforced on the ResourceContact table's tenantGuid and resourceId and contactId fields.
)
GO

-- Index on the ResourceContact table's tenantGuid field.
CREATE INDEX [I_ResourceContact_tenantGuid] ON [Scheduler].[ResourceContact] ([tenantGuid])
GO

-- Index on the ResourceContact table's tenantGuid,resourceId fields.
CREATE INDEX [I_ResourceContact_tenantGuid_resourceId] ON [Scheduler].[ResourceContact] ([tenantGuid], [resourceId])
GO

-- Index on the ResourceContact table's tenantGuid,contactId fields.
CREATE INDEX [I_ResourceContact_tenantGuid_contactId] ON [Scheduler].[ResourceContact] ([tenantGuid], [contactId])
GO

-- Index on the ResourceContact table's tenantGuid,relationshipTypeId fields.
CREATE INDEX [I_ResourceContact_tenantGuid_relationshipTypeId] ON [Scheduler].[ResourceContact] ([tenantGuid], [relationshipTypeId])
GO

-- Index on the ResourceContact table's tenantGuid,active fields.
CREATE INDEX [I_ResourceContact_tenantGuid_active] ON [Scheduler].[ResourceContact] ([tenantGuid], [active])
GO

-- Index on the ResourceContact table's tenantGuid,deleted fields.
CREATE INDEX [I_ResourceContact_tenantGuid_deleted] ON [Scheduler].[ResourceContact] ([tenantGuid], [deleted])
GO


-- The change history for records from the ResourceContact table.
CREATE TABLE [Scheduler].[ResourceContactChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[resourceContactId] INT NOT NULL,		-- Link to the ResourceContact table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ResourceContactChangeHistory_ResourceContact_resourceContactId] FOREIGN KEY ([resourceContactId]) REFERENCES [Scheduler].[ResourceContact] ([id])		-- Foreign key to the ResourceContact table.
)
GO

-- Index on the ResourceContactChangeHistory table's tenantGuid field.
CREATE INDEX [I_ResourceContactChangeHistory_tenantGuid] ON [Scheduler].[ResourceContactChangeHistory] ([tenantGuid])
GO

-- Index on the ResourceContactChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ResourceContactChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ResourceContactChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ResourceContactChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ResourceContactChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ResourceContactChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ResourceContactChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ResourceContactChangeHistory_tenantGuid_userId] ON [Scheduler].[ResourceContactChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ResourceContactChangeHistory table's tenantGuid,resourceContactId fields.
CREATE INDEX [I_ResourceContactChangeHistory_tenantGuid_resourceContactId] ON [Scheduler].[ResourceContactChangeHistory] ([tenantGuid], [resourceContactId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


/*
Master Rate Sheet. 
Replaces simple Resource-based rating with a hierarchical lookup system.
Hierarchy Logic (System should look for the first match in this order):
1. Specific Resource on Specific Project (schedulingTargetId + resourceId)
2. Specific Role on Specific Project (schedulingTargetId + assignmentRoleId)
3. Specific Resource Global Rate (resourceId)
4. Specific Role Global Rate (assignmentRoleId)
*/
CREATE TABLE [Scheduler].[RateSheet]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[officeId] INT NULL,		-- Optional office binding for a rate sheet.
	[assignmentRoleId] INT NULL,		-- Link to AssignmentRole. If populated, applies to anyone in this role.
	[resourceId] INT NULL,		-- Link to Resource. If populated, overrides the Role rate.
	[schedulingTargetId] INT NULL,		-- Link to SchedulingTarget. If populated, applies only to this project.
	[rateTypeId] INT NOT NULL,		-- e.g., 'Standard', 'Overtime', 'DoubleTime', 'Travel', 'Standby'
	[effectiveDate] DATETIME2(7) NOT NULL,		-- The date this rate becomes active. Allows for historical reporting and future price increases.
	[currencyId] INT NOT NULL,		-- Link to the Currency table.
	[costRate] MONEY NOT NULL,		-- Internal Cost (payroll)
	[billingRate] MONEY NOT NULL,		-- Invoicing Cost (customre)
	[notes] NVARCHAR(MAX) NULL,		-- For ad-hoc notes about the entry
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_RateSheet_Office_officeId] FOREIGN KEY ([officeId]) REFERENCES [Scheduler].[Office] ([id]),		-- Foreign key to the Office table.
	CONSTRAINT [FK_RateSheet_AssignmentRole_assignmentRoleId] FOREIGN KEY ([assignmentRoleId]) REFERENCES [Scheduler].[AssignmentRole] ([id]),		-- Foreign key to the AssignmentRole table.
	CONSTRAINT [FK_RateSheet_Resource_resourceId] FOREIGN KEY ([resourceId]) REFERENCES [Scheduler].[Resource] ([id]),		-- Foreign key to the Resource table.
	CONSTRAINT [FK_RateSheet_SchedulingTarget_schedulingTargetId] FOREIGN KEY ([schedulingTargetId]) REFERENCES [Scheduler].[SchedulingTarget] ([id]),		-- Foreign key to the SchedulingTarget table.
	CONSTRAINT [FK_RateSheet_RateType_rateTypeId] FOREIGN KEY ([rateTypeId]) REFERENCES [Scheduler].[RateType] ([id]),		-- Foreign key to the RateType table.
	CONSTRAINT [FK_RateSheet_Currency_currencyId] FOREIGN KEY ([currencyId]) REFERENCES [Scheduler].[Currency] ([id]),		-- Foreign key to the Currency table.
	CONSTRAINT [UC_RateSheet_tenantGuid_assignmentRoleId_resourceId_schedulingTargetId_rateTypeId_effectiveDate] UNIQUE ( [tenantGuid], [assignmentRoleId], [resourceId], [schedulingTargetId], [rateTypeId], [effectiveDate]) 		-- Uniqueness enforced on the RateSheet table's tenantGuid and assignmentRoleId and resourceId and schedulingTargetId and rateTypeId and effectiveDate fields.
)
GO

-- Index on the RateSheet table's tenantGuid field.
CREATE INDEX [I_RateSheet_tenantGuid] ON [Scheduler].[RateSheet] ([tenantGuid])
GO

-- Index on the RateSheet table's tenantGuid,officeId fields.
CREATE INDEX [I_RateSheet_tenantGuid_officeId] ON [Scheduler].[RateSheet] ([tenantGuid], [officeId])
GO

-- Index on the RateSheet table's tenantGuid,assignmentRoleId fields.
CREATE INDEX [I_RateSheet_tenantGuid_assignmentRoleId] ON [Scheduler].[RateSheet] ([tenantGuid], [assignmentRoleId])
GO

-- Index on the RateSheet table's tenantGuid,resourceId fields.
CREATE INDEX [I_RateSheet_tenantGuid_resourceId] ON [Scheduler].[RateSheet] ([tenantGuid], [resourceId])
GO

-- Index on the RateSheet table's tenantGuid,schedulingTargetId fields.
CREATE INDEX [I_RateSheet_tenantGuid_schedulingTargetId] ON [Scheduler].[RateSheet] ([tenantGuid], [schedulingTargetId])
GO

-- Index on the RateSheet table's tenantGuid,rateTypeId fields.
CREATE INDEX [I_RateSheet_tenantGuid_rateTypeId] ON [Scheduler].[RateSheet] ([tenantGuid], [rateTypeId])
GO

-- Index on the RateSheet table's tenantGuid,currencyId fields.
CREATE INDEX [I_RateSheet_tenantGuid_currencyId] ON [Scheduler].[RateSheet] ([tenantGuid], [currencyId])
GO

-- Index on the RateSheet table's tenantGuid,active fields.
CREATE INDEX [I_RateSheet_tenantGuid_active] ON [Scheduler].[RateSheet] ([tenantGuid], [active])
GO

-- Index on the RateSheet table's tenantGuid,deleted fields.
CREATE INDEX [I_RateSheet_tenantGuid_deleted] ON [Scheduler].[RateSheet] ([tenantGuid], [deleted])
GO

-- Index on the RateSheet table's tenantGuid,schedulingTargetId,resourceId,assignmentRoleId,rateTypeId,effectiveDate fields.
CREATE INDEX [I_RateSheet_tenantGuid_schedulingTargetId_resourceId_assignmentRoleId_rateTypeId_effectiveDate] ON [Scheduler].[RateSheet] ([tenantGuid], [schedulingTargetId], [resourceId], [assignmentRoleId], [rateTypeId], [effectiveDate])
GO


-- The change history for records from the RateSheet table.
CREATE TABLE [Scheduler].[RateSheetChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[rateSheetId] INT NOT NULL,		-- Link to the RateSheet table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_RateSheetChangeHistory_RateSheet_rateSheetId] FOREIGN KEY ([rateSheetId]) REFERENCES [Scheduler].[RateSheet] ([id])		-- Foreign key to the RateSheet table.
)
GO

-- Index on the RateSheetChangeHistory table's tenantGuid field.
CREATE INDEX [I_RateSheetChangeHistory_tenantGuid] ON [Scheduler].[RateSheetChangeHistory] ([tenantGuid])
GO

-- Index on the RateSheetChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_RateSheetChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[RateSheetChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the RateSheetChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_RateSheetChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[RateSheetChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the RateSheetChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_RateSheetChangeHistory_tenantGuid_userId] ON [Scheduler].[RateSheetChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the RateSheetChangeHistory table's tenantGuid,rateSheetId fields.
CREATE INDEX [I_RateSheetChangeHistory_tenantGuid_rateSheetId] ON [Scheduler].[RateSheetChangeHistory] ([tenantGuid], [rateSheetId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Links resources to qualifications they possess.  Includes expiry date, issuing authority, and notes.
CREATE TABLE [Scheduler].[ResourceQualification]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[resourceId] INT NOT NULL,		-- Link to the Resource table.
	[qualificationId] INT NOT NULL,		-- Link to the Qualification table.
	[issueDate] DATETIME2(7) NULL,		-- Date qualification was granted
	[expiryDate] DATETIME2(7) NULL,		-- NULL = no expiry (e.g., permanent license)
	[issuer] NVARCHAR(250) NULL,		-- e.g., State Board of Nursing, NCCCO
	[notes] NVARCHAR(MAX) NULL,
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ResourceQualification_Resource_resourceId] FOREIGN KEY ([resourceId]) REFERENCES [Scheduler].[Resource] ([id]),		-- Foreign key to the Resource table.
	CONSTRAINT [FK_ResourceQualification_Qualification_qualificationId] FOREIGN KEY ([qualificationId]) REFERENCES [Scheduler].[Qualification] ([id]),		-- Foreign key to the Qualification table.
	CONSTRAINT [UC_ResourceQualification_tenantGuid_resourceId_qualificationId] UNIQUE ( [tenantGuid], [resourceId], [qualificationId]) 		-- Uniqueness enforced on the ResourceQualification table's tenantGuid and resourceId and qualificationId fields.
)
GO

-- Index on the ResourceQualification table's tenantGuid field.
CREATE INDEX [I_ResourceQualification_tenantGuid] ON [Scheduler].[ResourceQualification] ([tenantGuid])
GO

-- Index on the ResourceQualification table's tenantGuid,resourceId fields.
CREATE INDEX [I_ResourceQualification_tenantGuid_resourceId] ON [Scheduler].[ResourceQualification] ([tenantGuid], [resourceId])
GO

-- Index on the ResourceQualification table's tenantGuid,qualificationId fields.
CREATE INDEX [I_ResourceQualification_tenantGuid_qualificationId] ON [Scheduler].[ResourceQualification] ([tenantGuid], [qualificationId])
GO

-- Index on the ResourceQualification table's tenantGuid,expiryDate fields.
CREATE INDEX [I_ResourceQualification_tenantGuid_expiryDate] ON [Scheduler].[ResourceQualification] ([tenantGuid], [expiryDate])
GO

-- Index on the ResourceQualification table's tenantGuid,active fields.
CREATE INDEX [I_ResourceQualification_tenantGuid_active] ON [Scheduler].[ResourceQualification] ([tenantGuid], [active])
GO

-- Index on the ResourceQualification table's tenantGuid,deleted fields.
CREATE INDEX [I_ResourceQualification_tenantGuid_deleted] ON [Scheduler].[ResourceQualification] ([tenantGuid], [deleted])
GO


-- The change history for records from the ResourceQualification table.
CREATE TABLE [Scheduler].[ResourceQualificationChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[resourceQualificationId] INT NOT NULL,		-- Link to the ResourceQualification table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ResourceQualificationChangeHistory_ResourceQualification_resourceQualificationId] FOREIGN KEY ([resourceQualificationId]) REFERENCES [Scheduler].[ResourceQualification] ([id])		-- Foreign key to the ResourceQualification table.
)
GO

-- Index on the ResourceQualificationChangeHistory table's tenantGuid field.
CREATE INDEX [I_ResourceQualificationChangeHistory_tenantGuid] ON [Scheduler].[ResourceQualificationChangeHistory] ([tenantGuid])
GO

-- Index on the ResourceQualificationChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ResourceQualificationChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ResourceQualificationChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ResourceQualificationChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ResourceQualificationChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ResourceQualificationChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ResourceQualificationChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ResourceQualificationChangeHistory_tenantGuid_userId] ON [Scheduler].[ResourceQualificationChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ResourceQualificationChangeHistory table's tenantGuid,resourceQualificationId fields.
CREATE INDEX [I_ResourceQualificationChangeHistory_tenantGuid_resourceQualificationId] ON [Scheduler].[ResourceQualificationChangeHistory] ([tenantGuid], [resourceQualificationId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Defines periods when a resource is unavailable (blackouts).  Used for vacations, maintenance, training, etc.  If endDateTime is NULL the blackout is ongoing until cleared.
CREATE TABLE [Scheduler].[ResourceAvailability]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[resourceId] INT NOT NULL,		-- Link to the Resource table.
	[timeZoneId] INT NULL,		-- Link to the TimeZone table.
	[startDateTime] DATETIME2(7) NOT NULL,		-- Inclusive start of the blackout period
	[endDateTime] DATETIME2(7) NULL,		-- NULL = ongoing blackout
	[reason] NVARCHAR(250) NULL,
	[notes] NVARCHAR(MAX) NULL,
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ResourceAvailability_Resource_resourceId] FOREIGN KEY ([resourceId]) REFERENCES [Scheduler].[Resource] ([id]),		-- Foreign key to the Resource table.
	CONSTRAINT [FK_ResourceAvailability_TimeZone_timeZoneId] FOREIGN KEY ([timeZoneId]) REFERENCES [Scheduler].[TimeZone] ([id])		-- Foreign key to the TimeZone table.
)
GO

-- Index on the ResourceAvailability table's tenantGuid field.
CREATE INDEX [I_ResourceAvailability_tenantGuid] ON [Scheduler].[ResourceAvailability] ([tenantGuid])
GO

-- Index on the ResourceAvailability table's tenantGuid,resourceId fields.
CREATE INDEX [I_ResourceAvailability_tenantGuid_resourceId] ON [Scheduler].[ResourceAvailability] ([tenantGuid], [resourceId])
GO

-- Index on the ResourceAvailability table's tenantGuid,timeZoneId fields.
CREATE INDEX [I_ResourceAvailability_tenantGuid_timeZoneId] ON [Scheduler].[ResourceAvailability] ([tenantGuid], [timeZoneId])
GO

-- Index on the ResourceAvailability table's tenantGuid,active fields.
CREATE INDEX [I_ResourceAvailability_tenantGuid_active] ON [Scheduler].[ResourceAvailability] ([tenantGuid], [active])
GO

-- Index on the ResourceAvailability table's tenantGuid,deleted fields.
CREATE INDEX [I_ResourceAvailability_tenantGuid_deleted] ON [Scheduler].[ResourceAvailability] ([tenantGuid], [deleted])
GO

-- Index on the ResourceAvailability table's tenantGuid,resourceId,startDateTime,endDateTime fields.
CREATE INDEX [I_ResourceAvailability_tenantGuid_resourceId_startDateTime_endDateTime] ON [Scheduler].[ResourceAvailability] ([tenantGuid], [resourceId], [startDateTime], [endDateTime])
GO


-- The change history for records from the ResourceAvailability table.
CREATE TABLE [Scheduler].[ResourceAvailabilityChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[resourceAvailabilityId] INT NOT NULL,		-- Link to the ResourceAvailability table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ResourceAvailabilityChangeHistory_ResourceAvailability_resourceAvailabilityId] FOREIGN KEY ([resourceAvailabilityId]) REFERENCES [Scheduler].[ResourceAvailability] ([id])		-- Foreign key to the ResourceAvailability table.
)
GO

-- Index on the ResourceAvailabilityChangeHistory table's tenantGuid field.
CREATE INDEX [I_ResourceAvailabilityChangeHistory_tenantGuid] ON [Scheduler].[ResourceAvailabilityChangeHistory] ([tenantGuid])
GO

-- Index on the ResourceAvailabilityChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ResourceAvailabilityChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ResourceAvailabilityChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ResourceAvailabilityChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ResourceAvailabilityChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ResourceAvailabilityChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ResourceAvailabilityChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ResourceAvailabilityChangeHistory_tenantGuid_userId] ON [Scheduler].[ResourceAvailabilityChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ResourceAvailabilityChangeHistory table's tenantGuid,resourceAvailabilityId fields.
CREATE INDEX [I_ResourceAvailabilityChangeHistory_tenantGuid_resourceAvailabilityId] ON [Scheduler].[ResourceAvailabilityChangeHistory] ([tenantGuid], [resourceAvailabilityId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Defines regular working shifts for a resource (e.g., clinician hours).  Used to determine baseline availability. Blackouts (ResourceAvailability) override these for exceptions.
CREATE TABLE [Scheduler].[ResourceShift]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[resourceId] INT NOT NULL,		-- Link to the Resource table.
	[dayOfWeek] INT NOT NULL DEFAULT 1,		-- 1=Sunday through 7=Saturday
	[timeZoneId] INT NULL,		-- Link to the TimeZone table.
	[startTime] TIME(7) NOT NULL,		-- Shift start time (e.g., 09:00:00)
	[hours] REAL NOT NULL DEFAULT 8,		-- Hours available from start time (handles overnight shifts cleanly)  Defaults to 8
	[label] NVARCHAR(250) NULL,		-- e.g., 'Morning Clinic', 'On-Call'
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ResourceShift_Resource_resourceId] FOREIGN KEY ([resourceId]) REFERENCES [Scheduler].[Resource] ([id]),		-- Foreign key to the Resource table.
	CONSTRAINT [FK_ResourceShift_TimeZone_timeZoneId] FOREIGN KEY ([timeZoneId]) REFERENCES [Scheduler].[TimeZone] ([id]),		-- Foreign key to the TimeZone table.
	CONSTRAINT [UC_ResourceShift_tenantGuid_resourceId_dayOfWeek] UNIQUE ( [tenantGuid], [resourceId], [dayOfWeek]) 		-- Uniqueness enforced on the ResourceShift table's tenantGuid and resourceId and dayOfWeek fields.
)
GO

-- Index on the ResourceShift table's tenantGuid field.
CREATE INDEX [I_ResourceShift_tenantGuid] ON [Scheduler].[ResourceShift] ([tenantGuid])
GO

-- Index on the ResourceShift table's tenantGuid,resourceId fields.
CREATE INDEX [I_ResourceShift_tenantGuid_resourceId] ON [Scheduler].[ResourceShift] ([tenantGuid], [resourceId])
GO

-- Index on the ResourceShift table's tenantGuid,timeZoneId fields.
CREATE INDEX [I_ResourceShift_tenantGuid_timeZoneId] ON [Scheduler].[ResourceShift] ([tenantGuid], [timeZoneId])
GO

-- Index on the ResourceShift table's tenantGuid,active fields.
CREATE INDEX [I_ResourceShift_tenantGuid_active] ON [Scheduler].[ResourceShift] ([tenantGuid], [active])
GO

-- Index on the ResourceShift table's tenantGuid,deleted fields.
CREATE INDEX [I_ResourceShift_tenantGuid_deleted] ON [Scheduler].[ResourceShift] ([tenantGuid], [deleted])
GO


-- The change history for records from the ResourceShift table.
CREATE TABLE [Scheduler].[ResourceShiftChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[resourceShiftId] INT NOT NULL,		-- Link to the ResourceShift table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ResourceShiftChangeHistory_ResourceShift_resourceShiftId] FOREIGN KEY ([resourceShiftId]) REFERENCES [Scheduler].[ResourceShift] ([id])		-- Foreign key to the ResourceShift table.
)
GO

-- Index on the ResourceShiftChangeHistory table's tenantGuid field.
CREATE INDEX [I_ResourceShiftChangeHistory_tenantGuid] ON [Scheduler].[ResourceShiftChangeHistory] ([tenantGuid])
GO

-- Index on the ResourceShiftChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ResourceShiftChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ResourceShiftChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ResourceShiftChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ResourceShiftChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ResourceShiftChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ResourceShiftChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ResourceShiftChangeHistory_tenantGuid_userId] ON [Scheduler].[ResourceShiftChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ResourceShiftChangeHistory table's tenantGuid,resourceShiftId fields.
CREATE INDEX [I_ResourceShiftChangeHistory_tenantGuid_resourceShiftId] ON [Scheduler].[ResourceShiftChangeHistory] ([tenantGuid], [resourceShiftId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Named, reusable group of resources that are typically scheduled together.  Common in construction (e.g., a roller + operator + spotter).  Crews can be assigned to events as a single unit.
CREATE TABLE [Scheduler].[Crew]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[notes] NVARCHAR(MAX) NULL,
	[officeId] INT NULL,		-- Optional office binding for a crew.
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[avatarFileName] NVARCHAR(250) NULL,		-- Part of the binary data field setup
	[avatarSize] BIGINT NULL,		-- Part of the binary data field setup
	[avatarData] VARBINARY(MAX) NULL,		-- Part of the binary data field setup
	[avatarMimeType] NVARCHAR(100) NULL,		-- Part of the binary data field setup
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Crew_Office_officeId] FOREIGN KEY ([officeId]) REFERENCES [Scheduler].[Office] ([id]),		-- Foreign key to the Office table.
	CONSTRAINT [FK_Crew_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_Crew_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Crew table's tenantGuid and name fields.
)
GO

-- Index on the Crew table's tenantGuid field.
CREATE INDEX [I_Crew_tenantGuid] ON [Scheduler].[Crew] ([tenantGuid])
GO

-- Index on the Crew table's tenantGuid,name fields.
CREATE INDEX [I_Crew_tenantGuid_name] ON [Scheduler].[Crew] ([tenantGuid], [name])
GO

-- Index on the Crew table's tenantGuid,officeId fields.
CREATE INDEX [I_Crew_tenantGuid_officeId] ON [Scheduler].[Crew] ([tenantGuid], [officeId])
GO

-- Index on the Crew table's tenantGuid,iconId fields.
CREATE INDEX [I_Crew_tenantGuid_iconId] ON [Scheduler].[Crew] ([tenantGuid], [iconId])
GO

-- Index on the Crew table's tenantGuid,active fields.
CREATE INDEX [I_Crew_tenantGuid_active] ON [Scheduler].[Crew] ([tenantGuid], [active])
GO

-- Index on the Crew table's tenantGuid,deleted fields.
CREATE INDEX [I_Crew_tenantGuid_deleted] ON [Scheduler].[Crew] ([tenantGuid], [deleted])
GO


-- The change history for records from the Crew table.
CREATE TABLE [Scheduler].[CrewChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[crewId] INT NOT NULL,		-- Link to the Crew table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_CrewChangeHistory_Crew_crewId] FOREIGN KEY ([crewId]) REFERENCES [Scheduler].[Crew] ([id])		-- Foreign key to the Crew table.
)
GO

-- Index on the CrewChangeHistory table's tenantGuid field.
CREATE INDEX [I_CrewChangeHistory_tenantGuid] ON [Scheduler].[CrewChangeHistory] ([tenantGuid])
GO

-- Index on the CrewChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_CrewChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[CrewChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the CrewChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_CrewChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[CrewChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the CrewChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_CrewChangeHistory_tenantGuid_userId] ON [Scheduler].[CrewChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the CrewChangeHistory table's tenantGuid,crewId fields.
CREATE INDEX [I_CrewChangeHistory_tenantGuid_crewId] ON [Scheduler].[CrewChangeHistory] ([tenantGuid], [crewId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Membership definition for a crew.  Specifies which resource belongs to which crew, the role they play within the crew, and a display sequence.
CREATE TABLE [Scheduler].[CrewMember]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[crewId] INT NOT NULL,		-- Link to the Crew table.
	[resourceId] INT NOT NULL,		-- Link to the Resource table.
	[assignmentRoleId] INT NULL,		-- Optional default role this member fulfils when the crew is assigned
	[sequence] INT NOT NULL DEFAULT 1,		-- Display/order position within the crew (lower numbers appear first)
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_CrewMember_Crew_crewId] FOREIGN KEY ([crewId]) REFERENCES [Scheduler].[Crew] ([id]),		-- Foreign key to the Crew table.
	CONSTRAINT [FK_CrewMember_Resource_resourceId] FOREIGN KEY ([resourceId]) REFERENCES [Scheduler].[Resource] ([id]),		-- Foreign key to the Resource table.
	CONSTRAINT [FK_CrewMember_AssignmentRole_assignmentRoleId] FOREIGN KEY ([assignmentRoleId]) REFERENCES [Scheduler].[AssignmentRole] ([id]),		-- Foreign key to the AssignmentRole table.
	CONSTRAINT [FK_CrewMember_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_CrewMember_tenantGuid_crewId_resourceId] UNIQUE ( [tenantGuid], [crewId], [resourceId]) 		-- Uniqueness enforced on the CrewMember table's tenantGuid and crewId and resourceId fields.
)
GO

-- Index on the CrewMember table's tenantGuid field.
CREATE INDEX [I_CrewMember_tenantGuid] ON [Scheduler].[CrewMember] ([tenantGuid])
GO

-- Index on the CrewMember table's tenantGuid,crewId fields.
CREATE INDEX [I_CrewMember_tenantGuid_crewId] ON [Scheduler].[CrewMember] ([tenantGuid], [crewId])
GO

-- Index on the CrewMember table's tenantGuid,resourceId fields.
CREATE INDEX [I_CrewMember_tenantGuid_resourceId] ON [Scheduler].[CrewMember] ([tenantGuid], [resourceId])
GO

-- Index on the CrewMember table's tenantGuid,assignmentRoleId fields.
CREATE INDEX [I_CrewMember_tenantGuid_assignmentRoleId] ON [Scheduler].[CrewMember] ([tenantGuid], [assignmentRoleId])
GO

-- Index on the CrewMember table's tenantGuid,iconId fields.
CREATE INDEX [I_CrewMember_tenantGuid_iconId] ON [Scheduler].[CrewMember] ([tenantGuid], [iconId])
GO

-- Index on the CrewMember table's tenantGuid,active fields.
CREATE INDEX [I_CrewMember_tenantGuid_active] ON [Scheduler].[CrewMember] ([tenantGuid], [active])
GO

-- Index on the CrewMember table's tenantGuid,deleted fields.
CREATE INDEX [I_CrewMember_tenantGuid_deleted] ON [Scheduler].[CrewMember] ([tenantGuid], [deleted])
GO


-- The change history for records from the CrewMember table.
CREATE TABLE [Scheduler].[CrewMemberChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[crewMemberId] INT NOT NULL,		-- Link to the CrewMember table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_CrewMemberChangeHistory_CrewMember_crewMemberId] FOREIGN KEY ([crewMemberId]) REFERENCES [Scheduler].[CrewMember] ([id])		-- Foreign key to the CrewMember table.
)
GO

-- Index on the CrewMemberChangeHistory table's tenantGuid field.
CREATE INDEX [I_CrewMemberChangeHistory_tenantGuid] ON [Scheduler].[CrewMemberChangeHistory] ([tenantGuid])
GO

-- Index on the CrewMemberChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_CrewMemberChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[CrewMemberChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the CrewMemberChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_CrewMemberChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[CrewMemberChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the CrewMemberChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_CrewMemberChangeHistory_tenantGuid_userId] ON [Scheduler].[CrewMemberChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the CrewMemberChangeHistory table's tenantGuid,crewMemberId fields.
CREATE INDEX [I_CrewMemberChangeHistory_tenantGuid_crewMemberId] ON [Scheduler].[CrewMemberChangeHistory] ([tenantGuid], [crewMemberId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Pre-defined event templates for common appointment/activity types.   Includes default duration, required roles, default assignments, etc.
CREATE TABLE [Scheduler].[ScheduledEventTemplate]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[defaultAllDay] BIT NOT NULL,		-- Default all day flag.
	[defaultDurationMinutes] INT NOT NULL DEFAULT 60,
	[schedulingTargetTypeId] INT NULL,		-- Optional target type
	[priorityId] INT NULL,		-- Optional priority
	[defaultLocationPattern] NVARCHAR(250) NULL,		-- e.g., 'Patient Home', 'Main Site'
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ScheduledEventTemplate_SchedulingTargetType_schedulingTargetTypeId] FOREIGN KEY ([schedulingTargetTypeId]) REFERENCES [Scheduler].[SchedulingTargetType] ([id]),		-- Foreign key to the SchedulingTargetType table.
	CONSTRAINT [FK_ScheduledEventTemplate_Priority_priorityId] FOREIGN KEY ([priorityId]) REFERENCES [Scheduler].[Priority] ([id]),		-- Foreign key to the Priority table.
	CONSTRAINT [UC_ScheduledEventTemplate_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the ScheduledEventTemplate table's tenantGuid and name fields.
)
GO

-- Index on the ScheduledEventTemplate table's tenantGuid field.
CREATE INDEX [I_ScheduledEventTemplate_tenantGuid] ON [Scheduler].[ScheduledEventTemplate] ([tenantGuid])
GO

-- Index on the ScheduledEventTemplate table's tenantGuid,name fields.
CREATE INDEX [I_ScheduledEventTemplate_tenantGuid_name] ON [Scheduler].[ScheduledEventTemplate] ([tenantGuid], [name])
GO

-- Index on the ScheduledEventTemplate table's tenantGuid,schedulingTargetTypeId fields.
CREATE INDEX [I_ScheduledEventTemplate_tenantGuid_schedulingTargetTypeId] ON [Scheduler].[ScheduledEventTemplate] ([tenantGuid], [schedulingTargetTypeId])
GO

-- Index on the ScheduledEventTemplate table's tenantGuid,active fields.
CREATE INDEX [I_ScheduledEventTemplate_tenantGuid_active] ON [Scheduler].[ScheduledEventTemplate] ([tenantGuid], [active])
GO

-- Index on the ScheduledEventTemplate table's tenantGuid,deleted fields.
CREATE INDEX [I_ScheduledEventTemplate_tenantGuid_deleted] ON [Scheduler].[ScheduledEventTemplate] ([tenantGuid], [deleted])
GO


-- The change history for records from the ScheduledEventTemplate table.
CREATE TABLE [Scheduler].[ScheduledEventTemplateChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduledEventTemplateId] INT NOT NULL,		-- Link to the ScheduledEventTemplate table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ScheduledEventTemplateChangeHistory_ScheduledEventTemplate_scheduledEventTemplateId] FOREIGN KEY ([scheduledEventTemplateId]) REFERENCES [Scheduler].[ScheduledEventTemplate] ([id])		-- Foreign key to the ScheduledEventTemplate table.
)
GO

-- Index on the ScheduledEventTemplateChangeHistory table's tenantGuid field.
CREATE INDEX [I_ScheduledEventTemplateChangeHistory_tenantGuid] ON [Scheduler].[ScheduledEventTemplateChangeHistory] ([tenantGuid])
GO

-- Index on the ScheduledEventTemplateChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ScheduledEventTemplateChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ScheduledEventTemplateChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ScheduledEventTemplateChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ScheduledEventTemplateChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ScheduledEventTemplateChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ScheduledEventTemplateChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ScheduledEventTemplateChangeHistory_tenantGuid_userId] ON [Scheduler].[ScheduledEventTemplateChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ScheduledEventTemplateChangeHistory table's tenantGuid,scheduledEventTemplateId fields.
CREATE INDEX [I_ScheduledEventTemplateChangeHistory_tenantGuid_scheduledEventTemplateId] ON [Scheduler].[ScheduledEventTemplateChangeHistory] ([tenantGuid], [scheduledEventTemplateId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


/*
====================================================================================================
 SCHEDULED EVENT TEMPLATE CHARGES (For Auto-Dropping)
 Defines default charges for ScheduledEventTemplate).
 When an event is created from a template, these charges are auto-dropped onto the event.
 ====================================================================================================
*/
CREATE TABLE [Scheduler].[ScheduledEventTemplateCharge]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduledEventTemplateId] INT NOT NULL,		-- Link to ScheduledEventTemplate
	[chargeTypeId] INT NOT NULL,		-- Link to ChargeType (the charge to drop).
	[defaultAmount] MONEY NOT NULL,		-- The amount to auto-drop (can be overridden on event).
	[isRequired] BIT NOT NULL DEFAULT 1,		-- some default charges might be optional (e.g., optional add-on fee).
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ScheduledEventTemplateCharge_ScheduledEventTemplate_scheduledEventTemplateId] FOREIGN KEY ([scheduledEventTemplateId]) REFERENCES [Scheduler].[ScheduledEventTemplate] ([id]),		-- Foreign key to the ScheduledEventTemplate table.
	CONSTRAINT [FK_ScheduledEventTemplateCharge_ChargeType_chargeTypeId] FOREIGN KEY ([chargeTypeId]) REFERENCES [Scheduler].[ChargeType] ([id])		-- Foreign key to the ChargeType table.
)
GO

-- Index on the ScheduledEventTemplateCharge table's tenantGuid field.
CREATE INDEX [I_ScheduledEventTemplateCharge_tenantGuid] ON [Scheduler].[ScheduledEventTemplateCharge] ([tenantGuid])
GO

-- Index on the ScheduledEventTemplateCharge table's tenantGuid,scheduledEventTemplateId fields.
CREATE INDEX [I_ScheduledEventTemplateCharge_tenantGuid_scheduledEventTemplateId] ON [Scheduler].[ScheduledEventTemplateCharge] ([tenantGuid], [scheduledEventTemplateId])
GO

-- Index on the ScheduledEventTemplateCharge table's tenantGuid,chargeTypeId fields.
CREATE INDEX [I_ScheduledEventTemplateCharge_tenantGuid_chargeTypeId] ON [Scheduler].[ScheduledEventTemplateCharge] ([tenantGuid], [chargeTypeId])
GO

-- Index on the ScheduledEventTemplateCharge table's tenantGuid,active fields.
CREATE INDEX [I_ScheduledEventTemplateCharge_tenantGuid_active] ON [Scheduler].[ScheduledEventTemplateCharge] ([tenantGuid], [active])
GO

-- Index on the ScheduledEventTemplateCharge table's tenantGuid,deleted fields.
CREATE INDEX [I_ScheduledEventTemplateCharge_tenantGuid_deleted] ON [Scheduler].[ScheduledEventTemplateCharge] ([tenantGuid], [deleted])
GO


-- The change history for records from the ScheduledEventTemplateCharge table.
CREATE TABLE [Scheduler].[ScheduledEventTemplateChargeChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduledEventTemplateChargeId] INT NOT NULL,		-- Link to the ScheduledEventTemplateCharge table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ScheduledEventTemplateChargeChangeHistory_ScheduledEventTemplateCharge_scheduledEventTemplateChargeId] FOREIGN KEY ([scheduledEventTemplateChargeId]) REFERENCES [Scheduler].[ScheduledEventTemplateCharge] ([id])		-- Foreign key to the ScheduledEventTemplateCharge table.
)
GO

-- Index on the ScheduledEventTemplateChargeChangeHistory table's tenantGuid field.
CREATE INDEX [I_ScheduledEventTemplateChargeChangeHistory_tenantGuid] ON [Scheduler].[ScheduledEventTemplateChargeChangeHistory] ([tenantGuid])
GO

-- Index on the ScheduledEventTemplateChargeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ScheduledEventTemplateChargeChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ScheduledEventTemplateChargeChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ScheduledEventTemplateChargeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ScheduledEventTemplateChargeChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ScheduledEventTemplateChargeChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ScheduledEventTemplateChargeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ScheduledEventTemplateChargeChangeHistory_tenantGuid_userId] ON [Scheduler].[ScheduledEventTemplateChargeChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ScheduledEventTemplateChargeChangeHistory table's tenantGuid,scheduledEventTemplateChargeId fields.
CREATE INDEX [I_ScheduledEventTemplateChargeChangeHistory_tenantGuid_scheduledEventTemplateChargeId] ON [Scheduler].[ScheduledEventTemplateChargeChangeHistory] ([tenantGuid], [scheduledEventTemplateChargeId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Default qualification requirements for events created from a template.
CREATE TABLE [Scheduler].[ScheduledEventTemplateQualificationRequirement]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduledEventTemplateId] INT NOT NULL,		-- Link to the ScheduledEventTemplate table.
	[qualificationId] INT NOT NULL,		-- Link to the Qualification table.
	[isRequired] BIT NOT NULL DEFAULT 1,
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ScheduledEventTemplateQualificationRequirement_ScheduledEventTemplate_scheduledEventTemplateId] FOREIGN KEY ([scheduledEventTemplateId]) REFERENCES [Scheduler].[ScheduledEventTemplate] ([id]),		-- Foreign key to the ScheduledEventTemplate table.
	CONSTRAINT [FK_ScheduledEventTemplateQualificationRequirement_Qualification_qualificationId] FOREIGN KEY ([qualificationId]) REFERENCES [Scheduler].[Qualification] ([id]),		-- Foreign key to the Qualification table.
	CONSTRAINT [UC_ScheduledEventTemplateQualificationRequirement_tenantGuid_scheduledEventTemplateId_qualificationId] UNIQUE ( [tenantGuid], [scheduledEventTemplateId], [qualificationId]) 		-- Uniqueness enforced on the ScheduledEventTemplateQualificationRequirement table's tenantGuid and scheduledEventTemplateId and qualificationId fields.
)
GO

-- Index on the ScheduledEventTemplateQualificationRequirement table's tenantGuid field.
CREATE INDEX [I_ScheduledEventTemplateQualificationRequirement_tenantGuid] ON [Scheduler].[ScheduledEventTemplateQualificationRequirement] ([tenantGuid])
GO

-- Index on the ScheduledEventTemplateQualificationRequirement table's tenantGuid,scheduledEventTemplateId fields.
CREATE INDEX [I_ScheduledEventTemplateQualificationRequirement_tenantGuid_scheduledEventTemplateId] ON [Scheduler].[ScheduledEventTemplateQualificationRequirement] ([tenantGuid], [scheduledEventTemplateId])
GO

-- Index on the ScheduledEventTemplateQualificationRequirement table's tenantGuid,qualificationId fields.
CREATE INDEX [I_ScheduledEventTemplateQualificationRequirement_tenantGuid_qualificationId] ON [Scheduler].[ScheduledEventTemplateQualificationRequirement] ([tenantGuid], [qualificationId])
GO

-- Index on the ScheduledEventTemplateQualificationRequirement table's tenantGuid,active fields.
CREATE INDEX [I_ScheduledEventTemplateQualificationRequirement_tenantGuid_active] ON [Scheduler].[ScheduledEventTemplateQualificationRequirement] ([tenantGuid], [active])
GO

-- Index on the ScheduledEventTemplateQualificationRequirement table's tenantGuid,deleted fields.
CREATE INDEX [I_ScheduledEventTemplateQualificationRequirement_tenantGuid_deleted] ON [Scheduler].[ScheduledEventTemplateQualificationRequirement] ([tenantGuid], [deleted])
GO


-- The change history for records from the ScheduledEventTemplateQualificationRequirement table.
CREATE TABLE [Scheduler].[ScheduledEventTemplateQualificationRequirementChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduledEventTemplateQualificationRequirementId] INT NOT NULL,		-- Link to the ScheduledEventTemplateQualificationRequirement table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_SchduldvntTmpltQulfctnRqurmntChngHstry_SchduldvntTmpltQulfctnRqurmnt_schduldvntTmpltQulfctnRqurmntd] FOREIGN KEY ([scheduledEventTemplateQualificationRequirementId]) REFERENCES [Scheduler].[ScheduledEventTemplateQualificationRequirement] ([id])		-- Foreign key to the ScheduledEventTemplateQualificationRequirement table.
)
GO

-- Index on the ScheduledEventTemplateQualificationRequirementChangeHistory table's tenantGuid field.
CREATE INDEX [I_ScheduledEventTemplateQualificationRequirementChangeHistory_tenantGuid] ON [Scheduler].[ScheduledEventTemplateQualificationRequirementChangeHistory] ([tenantGuid])
GO

-- Index on the ScheduledEventTemplateQualificationRequirementChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ScheduledEventTemplateQualificationRequirementChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ScheduledEventTemplateQualificationRequirementChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ScheduledEventTemplateQualificationRequirementChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ScheduledEventTemplateQualificationRequirementChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ScheduledEventTemplateQualificationRequirementChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ScheduledEventTemplateQualificationRequirementChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ScheduledEventTemplateQualificationRequirementChangeHistory_tenantGuid_userId] ON [Scheduler].[ScheduledEventTemplateQualificationRequirementChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ScheduledEventTemplateQualificationRequirementChangeHistory table's tenantGuid,scheduledEventTemplateQualificationRequirementId fields.
CREATE INDEX [I_ScheduledEventTemplateQualificationRequirementChangeHistory_tenantGuid_scheduledEventTemplateQualificationRequirementId] ON [Scheduler].[ScheduledEventTemplateQualificationRequirementChangeHistory] ([tenantGuid], [scheduledEventTemplateQualificationRequirementId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


/*
Core scheduling entity – any planned activity with a defined time range.  This managest recurrences with the 'Detachment Model'

How it works:
The Master: You create the Series (Event A). It has the RecurrenceRule.
The Virtuals: The UI calculates the "Ghost" instances for display.
The Exception: If you assign a specific crew to next Tuesday's instance (or move it), the system "Detaches" that instance.
It creates a new row in ScheduledEvent (Event B).
Event B is linked to Event A via a parentScheduledEventId.
You add a record to RecurrenceException for Event A saying "Skip the normal generation for Date X."
You attach the specific Crew/Resource to Event B.
*/
CREATE TABLE [Scheduler].[ScheduledEvent]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[officeId] INT NULL,		-- Snapshot of office that the first resource assigned to this event belongs to.  This should NOT be updated if a resource moves to a different office post-event assignment.  It should only change if there was an original entry error that needs to be corrected.
	[clientId] INT NULL,		-- Snapshot of client that this event belongs to.  It should be that of the scheduling target.  It should only change if there was an original entry error that needs to be corrected.
	[scheduledEventTemplateId] INT NULL,		-- Optional template/type of this scheduled event.
	[recurrenceRuleId] INT NULL,		-- Optional recurrence pattern for this event series
	[schedulingTargetId] INT NULL,		-- The SchedulingTarget (project, patient, etc.) this event is scheduled into
	[timeZoneId] INT NULL,		-- Link to the TimeZone table.
	[parentScheduledEventId] INT NULL,		-- If populated, this Event is a specific "Detached" instance of a Series
	[recurrenceInstanceDate] DATETIME2(7) NULL,		-- The original date this instance represented (crucial for matching with RecurrenceException)
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[isAllDay] BIT NULL DEFAULT 0,		-- Whether this is an all day event or not
	[startDateTime] DATETIME2(7) NOT NULL,		-- Inclusive start of the event in UTC
	[endDateTime] DATETIME2(7) NOT NULL,		-- Exclusive end of the event in UTC
	[location] NVARCHAR(250) NULL,
	[eventStatusId] INT NOT NULL,		-- Status for the event
	[resourceId] INT NULL,		-- Optional primary/lead resource for the event
	[crewId] INT NULL,		-- Optional primary/lead crew for the event
	[priorityId] INT NULL,		-- Optional priority
	[bookingSourceTypeId] INT NULL,		-- Optional booking source for reservation type workflows.
	[partySize] INT NULL,		-- Optional for use when running as a reservation system
	[notes] NVARCHAR(MAX) NULL,
	[color] NVARCHAR(10) NULL,		-- Override Hex color for UI display
	[externalId] NVARCHAR(100) NULL,		-- Optional link to an entity in another system
	[attributes] NVARCHAR(MAX) NULL,		-- to store arbitrary JSON
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ScheduledEvent_Office_officeId] FOREIGN KEY ([officeId]) REFERENCES [Scheduler].[Office] ([id]),		-- Foreign key to the Office table.
	CONSTRAINT [FK_ScheduledEvent_Client_clientId] FOREIGN KEY ([clientId]) REFERENCES [Scheduler].[Client] ([id]),		-- Foreign key to the Client table.
	CONSTRAINT [FK_ScheduledEvent_ScheduledEventTemplate_scheduledEventTemplateId] FOREIGN KEY ([scheduledEventTemplateId]) REFERENCES [Scheduler].[ScheduledEventTemplate] ([id]),		-- Foreign key to the ScheduledEventTemplate table.
	CONSTRAINT [FK_ScheduledEvent_RecurrenceRule_recurrenceRuleId] FOREIGN KEY ([recurrenceRuleId]) REFERENCES [Scheduler].[RecurrenceRule] ([id]),		-- Foreign key to the RecurrenceRule table.
	CONSTRAINT [FK_ScheduledEvent_SchedulingTarget_schedulingTargetId] FOREIGN KEY ([schedulingTargetId]) REFERENCES [Scheduler].[SchedulingTarget] ([id]),		-- Foreign key to the SchedulingTarget table.
	CONSTRAINT [FK_ScheduledEvent_TimeZone_timeZoneId] FOREIGN KEY ([timeZoneId]) REFERENCES [Scheduler].[TimeZone] ([id]),		-- Foreign key to the TimeZone table.
	CONSTRAINT [FK_ScheduledEvent_ScheduledEvent_parentScheduledEventId] FOREIGN KEY ([parentScheduledEventId]) REFERENCES [Scheduler].[ScheduledEvent] ([id]),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT [FK_ScheduledEvent_EventStatus_eventStatusId] FOREIGN KEY ([eventStatusId]) REFERENCES [Scheduler].[EventStatus] ([id]),		-- Foreign key to the EventStatus table.
	CONSTRAINT [FK_ScheduledEvent_Resource_resourceId] FOREIGN KEY ([resourceId]) REFERENCES [Scheduler].[Resource] ([id]),		-- Foreign key to the Resource table.
	CONSTRAINT [FK_ScheduledEvent_Crew_crewId] FOREIGN KEY ([crewId]) REFERENCES [Scheduler].[Crew] ([id]),		-- Foreign key to the Crew table.
	CONSTRAINT [FK_ScheduledEvent_Priority_priorityId] FOREIGN KEY ([priorityId]) REFERENCES [Scheduler].[Priority] ([id]),		-- Foreign key to the Priority table.
	CONSTRAINT [FK_ScheduledEvent_BookingSourceType_bookingSourceTypeId] FOREIGN KEY ([bookingSourceTypeId]) REFERENCES [Scheduler].[BookingSourceType] ([id]),		-- Foreign key to the BookingSourceType table.
	CONSTRAINT [UC_ScheduledEvent_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the ScheduledEvent table's tenantGuid and name fields.
)
GO

-- Index on the ScheduledEvent table's tenantGuid field.
CREATE INDEX [I_ScheduledEvent_tenantGuid] ON [Scheduler].[ScheduledEvent] ([tenantGuid])
GO

-- Index on the ScheduledEvent table's tenantGuid,officeId fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_officeId] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [officeId])
GO

-- Index on the ScheduledEvent table's tenantGuid,clientId fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_clientId] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [clientId])
GO

-- Index on the ScheduledEvent table's tenantGuid,scheduledEventTemplateId fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_scheduledEventTemplateId] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [scheduledEventTemplateId])
GO

-- Index on the ScheduledEvent table's tenantGuid,recurrenceRuleId fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_recurrenceRuleId] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [recurrenceRuleId])
GO

-- Index on the ScheduledEvent table's tenantGuid,schedulingTargetId fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_schedulingTargetId] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [schedulingTargetId])
GO

-- Index on the ScheduledEvent table's tenantGuid,timeZoneId fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_timeZoneId] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [timeZoneId])
GO

-- Index on the ScheduledEvent table's tenantGuid,parentScheduledEventId fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_parentScheduledEventId] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [parentScheduledEventId])
GO

-- Index on the ScheduledEvent table's tenantGuid,name fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_name] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [name])
GO

-- Index on the ScheduledEvent table's tenantGuid,startDateTime fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_startDateTime] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [startDateTime])
GO

-- Index on the ScheduledEvent table's tenantGuid,endDateTime fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_endDateTime] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [endDateTime])
GO

-- Index on the ScheduledEvent table's tenantGuid,location fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_location] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [location])
GO

-- Index on the ScheduledEvent table's tenantGuid,eventStatusId fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_eventStatusId] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [eventStatusId])
GO

-- Index on the ScheduledEvent table's tenantGuid,resourceId fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_resourceId] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [resourceId])
GO

-- Index on the ScheduledEvent table's tenantGuid,crewId fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_crewId] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [crewId])
GO

-- Index on the ScheduledEvent table's tenantGuid,priorityId fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_priorityId] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [priorityId])
GO

-- Index on the ScheduledEvent table's tenantGuid,bookingSourceTypeId fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_bookingSourceTypeId] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [bookingSourceTypeId])
GO

-- Index on the ScheduledEvent table's tenantGuid,active fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_active] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [active])
GO

-- Index on the ScheduledEvent table's tenantGuid,deleted fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_deleted] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [deleted])
GO

-- Index on the ScheduledEvent table's tenantGuid,startDateTime,endDateTime fields.
CREATE INDEX [I_ScheduledEvent_tenantGuid_startDateTime_endDateTime] ON [Scheduler].[ScheduledEvent] ([tenantGuid], [startDateTime], [endDateTime])
GO


-- The change history for records from the ScheduledEvent table.
CREATE TABLE [Scheduler].[ScheduledEventChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduledEventId] INT NOT NULL,		-- Link to the ScheduledEvent table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ScheduledEventChangeHistory_ScheduledEvent_scheduledEventId] FOREIGN KEY ([scheduledEventId]) REFERENCES [Scheduler].[ScheduledEvent] ([id])		-- Foreign key to the ScheduledEvent table.
)
GO

-- Index on the ScheduledEventChangeHistory table's tenantGuid field.
CREATE INDEX [I_ScheduledEventChangeHistory_tenantGuid] ON [Scheduler].[ScheduledEventChangeHistory] ([tenantGuid])
GO

-- Index on the ScheduledEventChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ScheduledEventChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ScheduledEventChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ScheduledEventChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ScheduledEventChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ScheduledEventChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ScheduledEventChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ScheduledEventChangeHistory_tenantGuid_userId] ON [Scheduler].[ScheduledEventChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ScheduledEventChangeHistory table's tenantGuid,scheduledEventId fields.
CREATE INDEX [I_ScheduledEventChangeHistory_tenantGuid_scheduledEventId] ON [Scheduler].[ScheduledEventChangeHistory] ([tenantGuid], [scheduledEventId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Master list of charge statuses (Pending, Approved, Invoiced, Void)
CREATE TABLE [Scheduler].[ChargeStatus]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the ChargeStatus table's name field.
CREATE INDEX [I_ChargeStatus_name] ON [Scheduler].[ChargeStatus] ([name])
GO

-- Index on the ChargeStatus table's active field.
CREATE INDEX [I_ChargeStatus_active] ON [Scheduler].[ChargeStatus] ([active])
GO

-- Index on the ChargeStatus table's deleted field.
CREATE INDEX [I_ChargeStatus_deleted] ON [Scheduler].[ChargeStatus] ([deleted])
GO

INSERT INTO [Scheduler].[ChargeStatus] ( [name], [description], [sequence], [color], [objectGuid] ) VALUES  ( 'Pending', 'Pending Approval', 1, '#B8FFC3', '1379f1da-c3cc-4149-998a-95ffa1728db6' )
GO

INSERT INTO [Scheduler].[ChargeStatus] ( [name], [description], [sequence], [color], [objectGuid] ) VALUES  ( 'Approved', 'Approved ', 2, '#59FF6F', 'ea16c955-9ccf-4489-acc0-0757c39ac3b6' )
GO

INSERT INTO [Scheduler].[ChargeStatus] ( [name], [description], [sequence], [color], [objectGuid] ) VALUES  ( 'Invoiced', 'Invoiced', 3, '#35A145', 'd250cc5c-51e9-49bb-91ce-4be47fc30dc0' )
GO

INSERT INTO [Scheduler].[ChargeStatus] ( [name], [description], [color], [sequence], [objectGuid] ) VALUES  ( 'Void', 'Void - Charge Disregarded', '#C62828', 4, '19d6560f-ed85-4d1e-905f-9a6e3dfb3026' )
GO


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
CREATE TABLE [Scheduler].[EventCharge]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduledEventId] INT NOT NULL,		-- Link to the ScheduledEvent table.
	[resourceId] INT NULL,		-- Optional link to resource to bind charge to specific resources (e.g., labor cost per operator
	[chargeTypeId] INT NOT NULL,		-- Link to the ChargeType table (defines revenue/expense category).
	[chargeStatusId] INT NOT NULL,		-- Link to the ChargeStatus table.  Tracks the status of the charge from creation through invoicing or cancelling.
	[quantity] NUMERIC(38,22) NULL DEFAULT 1,		-- Quantity (hours, units, km, etc.)
	[unitPrice] MONEY NULL,		-- Price per unit (can be NULL for flat fees)
	[extendedAmount] MONEY NOT NULL DEFAULT 0,		-- Always the final calculated/total amount (quantity × unitPrice, or just amount) Does not include taxes.
	[taxAmount] MONEY NOT NULL DEFAULT 0,		-- The calculated tax based on isTaxable
	[currencyId] INT NOT NULL,		-- Link to Currency table.
	[rateTypeId] INT NULL,		-- Optional link to RateType (e.g., 'Overtime').
	[notes] NVARCHAR(MAX) NULL,		-- Optional notes about the charge
	[isAutomatic] BIT NOT NULL DEFAULT 1,		-- 1 = auto-dropped from event type, 0 = manual add/edit.
	[exportedDate] DATETIME2(7) NULL,		-- When this charge was last exported (null = not exported yet).
	[externalId] NVARCHAR(100) NULL,		-- Identifier from extenral system - possibly invoice number or some other billing grouper
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_EventCharge_ScheduledEvent_scheduledEventId] FOREIGN KEY ([scheduledEventId]) REFERENCES [Scheduler].[ScheduledEvent] ([id]),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT [FK_EventCharge_Resource_resourceId] FOREIGN KEY ([resourceId]) REFERENCES [Scheduler].[Resource] ([id]),		-- Foreign key to the Resource table.
	CONSTRAINT [FK_EventCharge_ChargeType_chargeTypeId] FOREIGN KEY ([chargeTypeId]) REFERENCES [Scheduler].[ChargeType] ([id]),		-- Foreign key to the ChargeType table.
	CONSTRAINT [FK_EventCharge_ChargeStatus_chargeStatusId] FOREIGN KEY ([chargeStatusId]) REFERENCES [Scheduler].[ChargeStatus] ([id]),		-- Foreign key to the ChargeStatus table.
	CONSTRAINT [FK_EventCharge_Currency_currencyId] FOREIGN KEY ([currencyId]) REFERENCES [Scheduler].[Currency] ([id]),		-- Foreign key to the Currency table.
	CONSTRAINT [FK_EventCharge_RateType_rateTypeId] FOREIGN KEY ([rateTypeId]) REFERENCES [Scheduler].[RateType] ([id])		-- Foreign key to the RateType table.
)
GO

-- Index on the EventCharge table's tenantGuid field.
CREATE INDEX [I_EventCharge_tenantGuid] ON [Scheduler].[EventCharge] ([tenantGuid])
GO

-- Index on the EventCharge table's tenantGuid,scheduledEventId fields.
CREATE INDEX [I_EventCharge_tenantGuid_scheduledEventId] ON [Scheduler].[EventCharge] ([tenantGuid], [scheduledEventId])
GO

-- Index on the EventCharge table's tenantGuid,resourceId fields.
CREATE INDEX [I_EventCharge_tenantGuid_resourceId] ON [Scheduler].[EventCharge] ([tenantGuid], [resourceId])
GO

-- Index on the EventCharge table's tenantGuid,chargeTypeId fields.
CREATE INDEX [I_EventCharge_tenantGuid_chargeTypeId] ON [Scheduler].[EventCharge] ([tenantGuid], [chargeTypeId])
GO

-- Index on the EventCharge table's tenantGuid,chargeStatusId fields.
CREATE INDEX [I_EventCharge_tenantGuid_chargeStatusId] ON [Scheduler].[EventCharge] ([tenantGuid], [chargeStatusId])
GO

-- Index on the EventCharge table's tenantGuid,currencyId fields.
CREATE INDEX [I_EventCharge_tenantGuid_currencyId] ON [Scheduler].[EventCharge] ([tenantGuid], [currencyId])
GO

-- Index on the EventCharge table's tenantGuid,rateTypeId fields.
CREATE INDEX [I_EventCharge_tenantGuid_rateTypeId] ON [Scheduler].[EventCharge] ([tenantGuid], [rateTypeId])
GO

-- Index on the EventCharge table's tenantGuid,externalId fields.
CREATE INDEX [I_EventCharge_tenantGuid_externalId] ON [Scheduler].[EventCharge] ([tenantGuid], [externalId])
GO

-- Index on the EventCharge table's tenantGuid,active fields.
CREATE INDEX [I_EventCharge_tenantGuid_active] ON [Scheduler].[EventCharge] ([tenantGuid], [active])
GO

-- Index on the EventCharge table's tenantGuid,deleted fields.
CREATE INDEX [I_EventCharge_tenantGuid_deleted] ON [Scheduler].[EventCharge] ([tenantGuid], [deleted])
GO


-- The change history for records from the EventCharge table.
CREATE TABLE [Scheduler].[EventChargeChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[eventChargeId] INT NOT NULL,		-- Link to the EventCharge table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_EventChargeChangeHistory_EventCharge_eventChargeId] FOREIGN KEY ([eventChargeId]) REFERENCES [Scheduler].[EventCharge] ([id])		-- Foreign key to the EventCharge table.
)
GO

-- Index on the EventChargeChangeHistory table's tenantGuid field.
CREATE INDEX [I_EventChargeChangeHistory_tenantGuid] ON [Scheduler].[EventChargeChangeHistory] ([tenantGuid])
GO

-- Index on the EventChargeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_EventChargeChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[EventChargeChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the EventChargeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_EventChargeChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[EventChargeChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the EventChargeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_EventChargeChangeHistory_tenantGuid_userId] ON [Scheduler].[EventChargeChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the EventChargeChangeHistory table's tenantGuid,eventChargeId fields.
CREATE INDEX [I_EventChargeChangeHistory_tenantGuid_eventChargeId] ON [Scheduler].[EventChargeChangeHistory] ([tenantGuid], [eventChargeId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- The contact interaction data
CREATE TABLE [Scheduler].[ContactInteraction]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[contactId] INT NOT NULL,		-- The contact that is the target of the interaction.
	[initiatingContactId] INT NULL,		-- Optional contact that initiated the interaction.  This would be staff of the company using the scheduler
	[interactionTypeId] INT NOT NULL,		-- Link to the InteractionType table.
	[scheduledEventId] INT NULL,		-- The optional event that the interaction is regarding.
	[startTime] DATETIME2(7) NOT NULL,
	[endTime] DATETIME2(7) NULL,
	[notes] NVARCHAR(MAX) NULL,		-- Optional notes about the interaction
	[location] NVARCHAR(MAX) NULL,		-- Optional location details about the interaction
	[priorityId] INT NULL,		-- Optional priority for the interaction.
	[externalId] NVARCHAR(100) NULL,
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ContactInteraction_Contact_contactId] FOREIGN KEY ([contactId]) REFERENCES [Scheduler].[Contact] ([id]),		-- Foreign key to the Contact table.
	CONSTRAINT [FK_ContactInteraction_Contact_initiatingContactId] FOREIGN KEY ([initiatingContactId]) REFERENCES [Scheduler].[Contact] ([id]),		-- Foreign key to the Contact table.
	CONSTRAINT [FK_ContactInteraction_InteractionType_interactionTypeId] FOREIGN KEY ([interactionTypeId]) REFERENCES [Scheduler].[InteractionType] ([id]),		-- Foreign key to the InteractionType table.
	CONSTRAINT [FK_ContactInteraction_ScheduledEvent_scheduledEventId] FOREIGN KEY ([scheduledEventId]) REFERENCES [Scheduler].[ScheduledEvent] ([id]),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT [FK_ContactInteraction_Priority_priorityId] FOREIGN KEY ([priorityId]) REFERENCES [Scheduler].[Priority] ([id])		-- Foreign key to the Priority table.
)
GO

-- Index on the ContactInteraction table's tenantGuid field.
CREATE INDEX [I_ContactInteraction_tenantGuid] ON [Scheduler].[ContactInteraction] ([tenantGuid])
GO

-- Index on the ContactInteraction table's tenantGuid,contactId fields.
CREATE INDEX [I_ContactInteraction_tenantGuid_contactId] ON [Scheduler].[ContactInteraction] ([tenantGuid], [contactId])
GO

-- Index on the ContactInteraction table's tenantGuid,initiatingContactId fields.
CREATE INDEX [I_ContactInteraction_tenantGuid_initiatingContactId] ON [Scheduler].[ContactInteraction] ([tenantGuid], [initiatingContactId])
GO

-- Index on the ContactInteraction table's tenantGuid,interactionTypeId fields.
CREATE INDEX [I_ContactInteraction_tenantGuid_interactionTypeId] ON [Scheduler].[ContactInteraction] ([tenantGuid], [interactionTypeId])
GO

-- Index on the ContactInteraction table's tenantGuid,scheduledEventId fields.
CREATE INDEX [I_ContactInteraction_tenantGuid_scheduledEventId] ON [Scheduler].[ContactInteraction] ([tenantGuid], [scheduledEventId])
GO

-- Index on the ContactInteraction table's tenantGuid,priorityId fields.
CREATE INDEX [I_ContactInteraction_tenantGuid_priorityId] ON [Scheduler].[ContactInteraction] ([tenantGuid], [priorityId])
GO

-- Index on the ContactInteraction table's tenantGuid,externalId fields.
CREATE INDEX [I_ContactInteraction_tenantGuid_externalId] ON [Scheduler].[ContactInteraction] ([tenantGuid], [externalId])
GO

-- Index on the ContactInteraction table's tenantGuid,active fields.
CREATE INDEX [I_ContactInteraction_tenantGuid_active] ON [Scheduler].[ContactInteraction] ([tenantGuid], [active])
GO

-- Index on the ContactInteraction table's tenantGuid,deleted fields.
CREATE INDEX [I_ContactInteraction_tenantGuid_deleted] ON [Scheduler].[ContactInteraction] ([tenantGuid], [deleted])
GO

-- Index on the ContactInteraction table's tenantGuid,contactId,startTime fields.
CREATE INDEX [I_ContactInteraction_tenantGuid_contactId_startTime] ON [Scheduler].[ContactInteraction] ([tenantGuid], [contactId], [startTime])
GO


-- The change history for records from the ContactInteraction table.
CREATE TABLE [Scheduler].[ContactInteractionChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[contactInteractionId] INT NOT NULL,		-- Link to the ContactInteraction table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ContactInteractionChangeHistory_ContactInteraction_contactInteractionId] FOREIGN KEY ([contactInteractionId]) REFERENCES [Scheduler].[ContactInteraction] ([id])		-- Foreign key to the ContactInteraction table.
)
GO

-- Index on the ContactInteractionChangeHistory table's tenantGuid field.
CREATE INDEX [I_ContactInteractionChangeHistory_tenantGuid] ON [Scheduler].[ContactInteractionChangeHistory] ([tenantGuid])
GO

-- Index on the ContactInteractionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ContactInteractionChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ContactInteractionChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ContactInteractionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ContactInteractionChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ContactInteractionChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ContactInteractionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ContactInteractionChangeHistory_tenantGuid_userId] ON [Scheduler].[ContactInteractionChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ContactInteractionChangeHistory table's tenantGuid,contactInteractionId fields.
CREATE INDEX [I_ContactInteractionChangeHistory_tenantGuid_contactInteractionId] ON [Scheduler].[ContactInteractionChangeHistory] ([tenantGuid], [contactInteractionId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Many-to-many relationship between events and calendars.
CREATE TABLE [Scheduler].[EventCalendar]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduledEventId] INT NOT NULL,		-- Link to the ScheduledEvent table.
	[calendarId] INT NOT NULL,		-- Link to the Calendar table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_EventCalendar_ScheduledEvent_scheduledEventId] FOREIGN KEY ([scheduledEventId]) REFERENCES [Scheduler].[ScheduledEvent] ([id]),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT [FK_EventCalendar_Calendar_calendarId] FOREIGN KEY ([calendarId]) REFERENCES [Scheduler].[Calendar] ([id]),		-- Foreign key to the Calendar table.
	CONSTRAINT [UC_EventCalendar_tenantGuid_scheduledEventId_calendarId] UNIQUE ( [tenantGuid], [scheduledEventId], [calendarId]) 		-- Uniqueness enforced on the EventCalendar table's tenantGuid and scheduledEventId and calendarId fields.
)
GO

-- Index on the EventCalendar table's tenantGuid field.
CREATE INDEX [I_EventCalendar_tenantGuid] ON [Scheduler].[EventCalendar] ([tenantGuid])
GO

-- Index on the EventCalendar table's tenantGuid,scheduledEventId fields.
CREATE INDEX [I_EventCalendar_tenantGuid_scheduledEventId] ON [Scheduler].[EventCalendar] ([tenantGuid], [scheduledEventId])
GO

-- Index on the EventCalendar table's tenantGuid,calendarId fields.
CREATE INDEX [I_EventCalendar_tenantGuid_calendarId] ON [Scheduler].[EventCalendar] ([tenantGuid], [calendarId])
GO

-- Index on the EventCalendar table's tenantGuid,active fields.
CREATE INDEX [I_EventCalendar_tenantGuid_active] ON [Scheduler].[EventCalendar] ([tenantGuid], [active])
GO

-- Index on the EventCalendar table's tenantGuid,deleted fields.
CREATE INDEX [I_EventCalendar_tenantGuid_deleted] ON [Scheduler].[EventCalendar] ([tenantGuid], [deleted])
GO


-- Master list of depedency types
CREATE TABLE [Scheduler].[DependencyType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the DependencyType table's name field.
CREATE INDEX [I_DependencyType_name] ON [Scheduler].[DependencyType] ([name])
GO

-- Index on the DependencyType table's active field.
CREATE INDEX [I_DependencyType_active] ON [Scheduler].[DependencyType] ([active])
GO

-- Index on the DependencyType table's deleted field.
CREATE INDEX [I_DependencyType_deleted] ON [Scheduler].[DependencyType] ([deleted])
GO

INSERT INTO [Scheduler].[DependencyType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'FS', 'Finish to Start', 1, 'f08977bf-af84-4d89-9821-f8a2404028fa' )
GO

INSERT INTO [Scheduler].[DependencyType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'SS', 'Start to Start', 2, '51398efa-2489-41ba-a1b6-77d11ce6253b' )
GO

INSERT INTO [Scheduler].[DependencyType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'SF', 'Start to Finish', 3, '637dc30a-adc3-47ad-87fa-3c826b7d808f' )
GO

INSERT INTO [Scheduler].[DependencyType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'FF', 'Finish to Finish', 4, 'fc7b4932-e79a-4085-9c87-404d29331f85' )
GO


-- Dependencies that a scheduled event has that could affect it.
CREATE TABLE [Scheduler].[ScheduledEventDependency]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[predecessorEventId] INT NOT NULL,		-- The task that must happen first
	[successorEventId] INT NOT NULL,		-- The task that waits
	[dependencyTypeId] INT NOT NULL,		-- Link to the DependencyType table.
	[lagMinutes] INT NOT NULL DEFAULT 0,
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ScheduledEventDependency_ScheduledEvent_predecessorEventId] FOREIGN KEY ([predecessorEventId]) REFERENCES [Scheduler].[ScheduledEvent] ([id]),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT [FK_ScheduledEventDependency_ScheduledEvent_successorEventId] FOREIGN KEY ([successorEventId]) REFERENCES [Scheduler].[ScheduledEvent] ([id]),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT [FK_ScheduledEventDependency_DependencyType_dependencyTypeId] FOREIGN KEY ([dependencyTypeId]) REFERENCES [Scheduler].[DependencyType] ([id]),		-- Foreign key to the DependencyType table.
	CONSTRAINT [UC_ScheduledEventDependency_tenantGuid_predecessorEventId_successorEventId] UNIQUE ( [tenantGuid], [predecessorEventId], [successorEventId]) 		-- Uniqueness enforced on the ScheduledEventDependency table's tenantGuid and predecessorEventId and successorEventId fields.
)
GO

-- Index on the ScheduledEventDependency table's tenantGuid field.
CREATE INDEX [I_ScheduledEventDependency_tenantGuid] ON [Scheduler].[ScheduledEventDependency] ([tenantGuid])
GO

-- Index on the ScheduledEventDependency table's tenantGuid,predecessorEventId fields.
CREATE INDEX [I_ScheduledEventDependency_tenantGuid_predecessorEventId] ON [Scheduler].[ScheduledEventDependency] ([tenantGuid], [predecessorEventId])
GO

-- Index on the ScheduledEventDependency table's tenantGuid,successorEventId fields.
CREATE INDEX [I_ScheduledEventDependency_tenantGuid_successorEventId] ON [Scheduler].[ScheduledEventDependency] ([tenantGuid], [successorEventId])
GO

-- Index on the ScheduledEventDependency table's tenantGuid,active fields.
CREATE INDEX [I_ScheduledEventDependency_tenantGuid_active] ON [Scheduler].[ScheduledEventDependency] ([tenantGuid], [active])
GO

-- Index on the ScheduledEventDependency table's tenantGuid,deleted fields.
CREATE INDEX [I_ScheduledEventDependency_tenantGuid_deleted] ON [Scheduler].[ScheduledEventDependency] ([tenantGuid], [deleted])
GO


-- The change history for records from the ScheduledEventDependency table.
CREATE TABLE [Scheduler].[ScheduledEventDependencyChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduledEventDependencyId] INT NOT NULL,		-- Link to the ScheduledEventDependency table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ScheduledEventDependencyChangeHistory_ScheduledEventDependency_scheduledEventDependencyId] FOREIGN KEY ([scheduledEventDependencyId]) REFERENCES [Scheduler].[ScheduledEventDependency] ([id])		-- Foreign key to the ScheduledEventDependency table.
)
GO

-- Index on the ScheduledEventDependencyChangeHistory table's tenantGuid field.
CREATE INDEX [I_ScheduledEventDependencyChangeHistory_tenantGuid] ON [Scheduler].[ScheduledEventDependencyChangeHistory] ([tenantGuid])
GO

-- Index on the ScheduledEventDependencyChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ScheduledEventDependencyChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ScheduledEventDependencyChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ScheduledEventDependencyChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ScheduledEventDependencyChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ScheduledEventDependencyChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ScheduledEventDependencyChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ScheduledEventDependencyChangeHistory_tenantGuid_userId] ON [Scheduler].[ScheduledEventDependencyChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ScheduledEventDependencyChangeHistory table's tenantGuid,scheduledEventDependencyId fields.
CREATE INDEX [I_ScheduledEventDependencyChangeHistory_tenantGuid_scheduledEventDependencyId] ON [Scheduler].[ScheduledEventDependencyChangeHistory] ([tenantGuid], [scheduledEventDependencyId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Specific qualifications required for a single event instance, overriding or adding to role/site reqs..
CREATE TABLE [Scheduler].[ScheduledEventQualificationRequirement]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduledEventId] INT NOT NULL,		-- Link to the ScheduledEvent table.
	[qualificationId] INT NOT NULL,		-- Link to the Qualification table.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ScheduledEventQualificationRequirement_ScheduledEvent_scheduledEventId] FOREIGN KEY ([scheduledEventId]) REFERENCES [Scheduler].[ScheduledEvent] ([id]),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT [FK_ScheduledEventQualificationRequirement_Qualification_qualificationId] FOREIGN KEY ([qualificationId]) REFERENCES [Scheduler].[Qualification] ([id])		-- Foreign key to the Qualification table.
)
GO

-- Index on the ScheduledEventQualificationRequirement table's tenantGuid field.
CREATE INDEX [I_ScheduledEventQualificationRequirement_tenantGuid] ON [Scheduler].[ScheduledEventQualificationRequirement] ([tenantGuid])
GO

-- Index on the ScheduledEventQualificationRequirement table's tenantGuid,scheduledEventId fields.
CREATE INDEX [I_ScheduledEventQualificationRequirement_tenantGuid_scheduledEventId] ON [Scheduler].[ScheduledEventQualificationRequirement] ([tenantGuid], [scheduledEventId])
GO

-- Index on the ScheduledEventQualificationRequirement table's tenantGuid,qualificationId fields.
CREATE INDEX [I_ScheduledEventQualificationRequirement_tenantGuid_qualificationId] ON [Scheduler].[ScheduledEventQualificationRequirement] ([tenantGuid], [qualificationId])
GO

-- Index on the ScheduledEventQualificationRequirement table's tenantGuid,active fields.
CREATE INDEX [I_ScheduledEventQualificationRequirement_tenantGuid_active] ON [Scheduler].[ScheduledEventQualificationRequirement] ([tenantGuid], [active])
GO

-- Index on the ScheduledEventQualificationRequirement table's tenantGuid,deleted fields.
CREATE INDEX [I_ScheduledEventQualificationRequirement_tenantGuid_deleted] ON [Scheduler].[ScheduledEventQualificationRequirement] ([tenantGuid], [deleted])
GO


-- The change history for records from the ScheduledEventQualificationRequirement table.
CREATE TABLE [Scheduler].[ScheduledEventQualificationRequirementChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduledEventQualificationRequirementId] INT NOT NULL,		-- Link to the ScheduledEventQualificationRequirement table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_SchduldvntQulfctnRqurmntChngHstry_SchduldvntQulfctnRqurmnt_schduldvntQulfctnRqurmntd] FOREIGN KEY ([scheduledEventQualificationRequirementId]) REFERENCES [Scheduler].[ScheduledEventQualificationRequirement] ([id])		-- Foreign key to the ScheduledEventQualificationRequirement table.
)
GO

-- Index on the ScheduledEventQualificationRequirementChangeHistory table's tenantGuid field.
CREATE INDEX [I_ScheduledEventQualificationRequirementChangeHistory_tenantGuid] ON [Scheduler].[ScheduledEventQualificationRequirementChangeHistory] ([tenantGuid])
GO

-- Index on the ScheduledEventQualificationRequirementChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ScheduledEventQualificationRequirementChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ScheduledEventQualificationRequirementChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ScheduledEventQualificationRequirementChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ScheduledEventQualificationRequirementChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ScheduledEventQualificationRequirementChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ScheduledEventQualificationRequirementChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ScheduledEventQualificationRequirementChangeHistory_tenantGuid_userId] ON [Scheduler].[ScheduledEventQualificationRequirementChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ScheduledEventQualificationRequirementChangeHistory table's tenantGuid,scheduledEventQualificationRequirementId fields.
CREATE INDEX [I_ScheduledEventQualificationRequirementChangeHistory_tenantGuid_scheduledEventQualificationRequirementId] ON [Scheduler].[ScheduledEventQualificationRequirementChangeHistory] ([tenantGuid], [scheduledEventQualificationRequirementId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Exceptions to a recurring series.  Used for canceled dates or moved instances (original date + new date).
CREATE TABLE [Scheduler].[RecurrenceException]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduledEventId] INT NOT NULL,		-- Link to the ScheduledEvent table.
	[exceptionDateTime] DATETIME2(7) NOT NULL,		-- The original occurrence date/time that is excepted
	[movedToDateTime] DATETIME2(7) NULL,		-- NULL = canceled, non-NULL = moved to this new date/time
	[reason] NVARCHAR(250) NULL,
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_RecurrenceException_ScheduledEvent_scheduledEventId] FOREIGN KEY ([scheduledEventId]) REFERENCES [Scheduler].[ScheduledEvent] ([id]),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT [UC_RecurrenceException_tenantGuid_scheduledEventId_exceptionDateTime] UNIQUE ( [tenantGuid], [scheduledEventId], [exceptionDateTime]) 		-- Uniqueness enforced on the RecurrenceException table's tenantGuid and scheduledEventId and exceptionDateTime fields.
)
GO

-- Index on the RecurrenceException table's tenantGuid field.
CREATE INDEX [I_RecurrenceException_tenantGuid] ON [Scheduler].[RecurrenceException] ([tenantGuid])
GO

-- Index on the RecurrenceException table's tenantGuid,scheduledEventId fields.
CREATE INDEX [I_RecurrenceException_tenantGuid_scheduledEventId] ON [Scheduler].[RecurrenceException] ([tenantGuid], [scheduledEventId])
GO

-- Index on the RecurrenceException table's tenantGuid,active fields.
CREATE INDEX [I_RecurrenceException_tenantGuid_active] ON [Scheduler].[RecurrenceException] ([tenantGuid], [active])
GO

-- Index on the RecurrenceException table's tenantGuid,deleted fields.
CREATE INDEX [I_RecurrenceException_tenantGuid_deleted] ON [Scheduler].[RecurrenceException] ([tenantGuid], [deleted])
GO


-- The change history for records from the RecurrenceException table.
CREATE TABLE [Scheduler].[RecurrenceExceptionChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[recurrenceExceptionId] INT NOT NULL,		-- Link to the RecurrenceException table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_RecurrenceExceptionChangeHistory_RecurrenceException_recurrenceExceptionId] FOREIGN KEY ([recurrenceExceptionId]) REFERENCES [Scheduler].[RecurrenceException] ([id])		-- Foreign key to the RecurrenceException table.
)
GO

-- Index on the RecurrenceExceptionChangeHistory table's tenantGuid field.
CREATE INDEX [I_RecurrenceExceptionChangeHistory_tenantGuid] ON [Scheduler].[RecurrenceExceptionChangeHistory] ([tenantGuid])
GO

-- Index on the RecurrenceExceptionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_RecurrenceExceptionChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[RecurrenceExceptionChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the RecurrenceExceptionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_RecurrenceExceptionChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[RecurrenceExceptionChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the RecurrenceExceptionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_RecurrenceExceptionChangeHistory_tenantGuid_userId] ON [Scheduler].[RecurrenceExceptionChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the RecurrenceExceptionChangeHistory table's tenantGuid,recurrenceExceptionId fields.
CREATE INDEX [I_RecurrenceExceptionChangeHistory_tenantGuid_recurrenceExceptionId] ON [Scheduler].[RecurrenceExceptionChangeHistory] ([tenantGuid], [recurrenceExceptionId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Master list of notification types
CREATE TABLE [Scheduler].[NotificationType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the NotificationType table's name field.
CREATE INDEX [I_NotificationType_name] ON [Scheduler].[NotificationType] ([name])
GO

-- Index on the NotificationType table's active field.
CREATE INDEX [I_NotificationType_active] ON [Scheduler].[NotificationType] ([active])
GO

-- Index on the NotificationType table's deleted field.
CREATE INDEX [I_NotificationType_deleted] ON [Scheduler].[NotificationType] ([deleted])
GO

INSERT INTO [Scheduler].[NotificationType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Email', 'Send to email address', 1, '73ff7b17-3fd7-40ce-91bf-c91daca7b4ce' )
GO

INSERT INTO [Scheduler].[NotificationType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'SMS', 'Sent to cell phone via SMS message', 2, '89391299-4427-43f6-bcf2-0266e47e83a7' )
GO

INSERT INTO [Scheduler].[NotificationType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Push', 'Sent to cell phone via Push notification', 3, '0395ddde-58dc-4577-9dae-07614680c386' )
GO


-- Links resources (or entire crews) to events.  Supports partial assignments and role designation.  - If crewId is non-NULL → this row represents assignment of the whole crew - If resourceId is non-NULL and crewId is NULL → individual resource assignment - assignmentStart/End NULL → uses full event duration
CREATE TABLE [Scheduler].[NotificationSubscription]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[resourceId] INT NULL,		-- Optional resource for this notification subscription.  Needs either this or contact to be valid.
	[contactId] INT NULL,		-- Optional contact for this notification subscription.  Needs either this or resource to be valid.
	[notificationTypeId] INT NOT NULL,		-- Link to the NotificationType table.
	[triggerEvents] INT NOT NULL DEFAULT 1,		-- Bitmask: 1=Assigned, 2=Canceled, 4=Modified, 8=Reminder
	[recipientAddress] NVARCHAR(250) NOT NULL,		-- Email address or Phone #
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_NotificationSubscription_Resource_resourceId] FOREIGN KEY ([resourceId]) REFERENCES [Scheduler].[Resource] ([id]),		-- Foreign key to the Resource table.
	CONSTRAINT [FK_NotificationSubscription_Contact_contactId] FOREIGN KEY ([contactId]) REFERENCES [Scheduler].[Contact] ([id]),		-- Foreign key to the Contact table.
	CONSTRAINT [FK_NotificationSubscription_NotificationType_notificationTypeId] FOREIGN KEY ([notificationTypeId]) REFERENCES [Scheduler].[NotificationType] ([id])		-- Foreign key to the NotificationType table.
)
GO

-- Index on the NotificationSubscription table's tenantGuid field.
CREATE INDEX [I_NotificationSubscription_tenantGuid] ON [Scheduler].[NotificationSubscription] ([tenantGuid])
GO

-- Index on the NotificationSubscription table's tenantGuid,resourceId fields.
CREATE INDEX [I_NotificationSubscription_tenantGuid_resourceId] ON [Scheduler].[NotificationSubscription] ([tenantGuid], [resourceId])
GO

-- Index on the NotificationSubscription table's tenantGuid,contactId fields.
CREATE INDEX [I_NotificationSubscription_tenantGuid_contactId] ON [Scheduler].[NotificationSubscription] ([tenantGuid], [contactId])
GO

-- Index on the NotificationSubscription table's tenantGuid,notificationTypeId fields.
CREATE INDEX [I_NotificationSubscription_tenantGuid_notificationTypeId] ON [Scheduler].[NotificationSubscription] ([tenantGuid], [notificationTypeId])
GO

-- Index on the NotificationSubscription table's tenantGuid,active fields.
CREATE INDEX [I_NotificationSubscription_tenantGuid_active] ON [Scheduler].[NotificationSubscription] ([tenantGuid], [active])
GO

-- Index on the NotificationSubscription table's tenantGuid,deleted fields.
CREATE INDEX [I_NotificationSubscription_tenantGuid_deleted] ON [Scheduler].[NotificationSubscription] ([tenantGuid], [deleted])
GO


-- The change history for records from the NotificationSubscription table.
CREATE TABLE [Scheduler].[NotificationSubscriptionChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[notificationSubscriptionId] INT NOT NULL,		-- Link to the NotificationSubscription table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_NotificationSubscriptionChangeHistory_NotificationSubscription_notificationSubscriptionId] FOREIGN KEY ([notificationSubscriptionId]) REFERENCES [Scheduler].[NotificationSubscription] ([id])		-- Foreign key to the NotificationSubscription table.
)
GO

-- Index on the NotificationSubscriptionChangeHistory table's tenantGuid field.
CREATE INDEX [I_NotificationSubscriptionChangeHistory_tenantGuid] ON [Scheduler].[NotificationSubscriptionChangeHistory] ([tenantGuid])
GO

-- Index on the NotificationSubscriptionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_NotificationSubscriptionChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[NotificationSubscriptionChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the NotificationSubscriptionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_NotificationSubscriptionChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[NotificationSubscriptionChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the NotificationSubscriptionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_NotificationSubscriptionChangeHistory_tenantGuid_userId] ON [Scheduler].[NotificationSubscriptionChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the NotificationSubscriptionChangeHistory table's tenantGuid,notificationSubscriptionId fields.
CREATE INDEX [I_NotificationSubscriptionChangeHistory_tenantGuid_notificationSubscriptionId] ON [Scheduler].[NotificationSubscriptionChangeHistory] ([tenantGuid], [notificationSubscriptionId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


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
CREATE TABLE [Scheduler].[Fund]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[glCode] NVARCHAR(100) NULL,		-- The accounting code
	[isRestricted] BIT NOT NULL DEFAULT 0,		-- Legal restriction on funds
	[goalAmount] MONEY NULL,
	[notes] NVARCHAR(MAX) NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Fund_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_Fund_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Fund table's tenantGuid and name fields.
)
GO

-- Index on the Fund table's tenantGuid field.
CREATE INDEX [I_Fund_tenantGuid] ON [Scheduler].[Fund] ([tenantGuid])
GO

-- Index on the Fund table's tenantGuid,name fields.
CREATE INDEX [I_Fund_tenantGuid_name] ON [Scheduler].[Fund] ([tenantGuid], [name])
GO

-- Index on the Fund table's tenantGuid,iconId fields.
CREATE INDEX [I_Fund_tenantGuid_iconId] ON [Scheduler].[Fund] ([tenantGuid], [iconId])
GO

-- Index on the Fund table's tenantGuid,active fields.
CREATE INDEX [I_Fund_tenantGuid_active] ON [Scheduler].[Fund] ([tenantGuid], [active])
GO

-- Index on the Fund table's tenantGuid,deleted fields.
CREATE INDEX [I_Fund_tenantGuid_deleted] ON [Scheduler].[Fund] ([tenantGuid], [deleted])
GO


-- The change history for records from the Fund table.
CREATE TABLE [Scheduler].[FundChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[fundId] INT NOT NULL,		-- Link to the Fund table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_FundChangeHistory_Fund_fundId] FOREIGN KEY ([fundId]) REFERENCES [Scheduler].[Fund] ([id])		-- Foreign key to the Fund table.
)
GO

-- Index on the FundChangeHistory table's tenantGuid field.
CREATE INDEX [I_FundChangeHistory_tenantGuid] ON [Scheduler].[FundChangeHistory] ([tenantGuid])
GO

-- Index on the FundChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_FundChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[FundChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the FundChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_FundChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[FundChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the FundChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_FundChangeHistory_tenantGuid_userId] ON [Scheduler].[FundChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the FundChangeHistory table's tenantGuid,fundId fields.
CREATE INDEX [I_FundChangeHistory_tenantGuid_fundId] ON [Scheduler].[FundChangeHistory] ([tenantGuid], [fundId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


--  2. CAMPAIGNS (Broad Initiatives)
CREATE TABLE [Scheduler].[Campaign]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[startDate] DATE NULL,
	[endDate] DATE NULL,
	[fundRaisingGoal] MONEY NULL,
	[notes] NVARCHAR(MAX) NULL,
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Campaign_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_Campaign_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Campaign table's tenantGuid and name fields.
)
GO

-- Index on the Campaign table's tenantGuid field.
CREATE INDEX [I_Campaign_tenantGuid] ON [Scheduler].[Campaign] ([tenantGuid])
GO

-- Index on the Campaign table's tenantGuid,name fields.
CREATE INDEX [I_Campaign_tenantGuid_name] ON [Scheduler].[Campaign] ([tenantGuid], [name])
GO

-- Index on the Campaign table's tenantGuid,iconId fields.
CREATE INDEX [I_Campaign_tenantGuid_iconId] ON [Scheduler].[Campaign] ([tenantGuid], [iconId])
GO

-- Index on the Campaign table's tenantGuid,active fields.
CREATE INDEX [I_Campaign_tenantGuid_active] ON [Scheduler].[Campaign] ([tenantGuid], [active])
GO

-- Index on the Campaign table's tenantGuid,deleted fields.
CREATE INDEX [I_Campaign_tenantGuid_deleted] ON [Scheduler].[Campaign] ([tenantGuid], [deleted])
GO


-- The change history for records from the Campaign table.
CREATE TABLE [Scheduler].[CampaignChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[campaignId] INT NOT NULL,		-- Link to the Campaign table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_CampaignChangeHistory_Campaign_campaignId] FOREIGN KEY ([campaignId]) REFERENCES [Scheduler].[Campaign] ([id])		-- Foreign key to the Campaign table.
)
GO

-- Index on the CampaignChangeHistory table's tenantGuid field.
CREATE INDEX [I_CampaignChangeHistory_tenantGuid] ON [Scheduler].[CampaignChangeHistory] ([tenantGuid])
GO

-- Index on the CampaignChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_CampaignChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[CampaignChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the CampaignChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_CampaignChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[CampaignChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the CampaignChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_CampaignChangeHistory_tenantGuid_userId] ON [Scheduler].[CampaignChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the CampaignChangeHistory table's tenantGuid,campaignId fields.
CREATE INDEX [I_CampaignChangeHistory_tenantGuid_campaignId] ON [Scheduler].[CampaignChangeHistory] ([tenantGuid], [campaignId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


--  3. APPEALS (Specific Solicitations)
CREATE TABLE [Scheduler].[Appeal]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[campaignId] INT NULL,		-- Optional link to parent campaign
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[costPerUnit] MONEY NULL,		-- For ROI calculation (Cost vs. Raised)
	[notes] NVARCHAR(MAX) NULL,
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Appeal_Campaign_campaignId] FOREIGN KEY ([campaignId]) REFERENCES [Scheduler].[Campaign] ([id]),		-- Foreign key to the Campaign table.
	CONSTRAINT [FK_Appeal_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_Appeal_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Appeal table's tenantGuid and name fields.
)
GO

-- Index on the Appeal table's tenantGuid field.
CREATE INDEX [I_Appeal_tenantGuid] ON [Scheduler].[Appeal] ([tenantGuid])
GO

-- Index on the Appeal table's tenantGuid,campaignId fields.
CREATE INDEX [I_Appeal_tenantGuid_campaignId] ON [Scheduler].[Appeal] ([tenantGuid], [campaignId])
GO

-- Index on the Appeal table's tenantGuid,name fields.
CREATE INDEX [I_Appeal_tenantGuid_name] ON [Scheduler].[Appeal] ([tenantGuid], [name])
GO

-- Index on the Appeal table's tenantGuid,iconId fields.
CREATE INDEX [I_Appeal_tenantGuid_iconId] ON [Scheduler].[Appeal] ([tenantGuid], [iconId])
GO

-- Index on the Appeal table's tenantGuid,active fields.
CREATE INDEX [I_Appeal_tenantGuid_active] ON [Scheduler].[Appeal] ([tenantGuid], [active])
GO

-- Index on the Appeal table's tenantGuid,deleted fields.
CREATE INDEX [I_Appeal_tenantGuid_deleted] ON [Scheduler].[Appeal] ([tenantGuid], [deleted])
GO


-- The change history for records from the Appeal table.
CREATE TABLE [Scheduler].[AppealChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[appealId] INT NOT NULL,		-- Link to the Appeal table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_AppealChangeHistory_Appeal_appealId] FOREIGN KEY ([appealId]) REFERENCES [Scheduler].[Appeal] ([id])		-- Foreign key to the Appeal table.
)
GO

-- Index on the AppealChangeHistory table's tenantGuid field.
CREATE INDEX [I_AppealChangeHistory_tenantGuid] ON [Scheduler].[AppealChangeHistory] ([tenantGuid])
GO

-- Index on the AppealChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_AppealChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[AppealChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the AppealChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_AppealChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[AppealChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the AppealChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_AppealChangeHistory_tenantGuid_userId] ON [Scheduler].[AppealChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the AppealChangeHistory table's tenantGuid,appealId fields.
CREATE INDEX [I_AppealChangeHistory_tenantGuid_appealId] ON [Scheduler].[AppealChangeHistory] ([tenantGuid], [appealId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


/*
====================================================================================================
   HOUSEHOLD MANAGEMENT
   Standardizes how multiple constituents are grouped for mailing, receipting, and recognition.
   This allows for "The Smith Family" recognition even if John and Jane have separate records.
   ====================================================================================================
*/
CREATE TABLE [Scheduler].[Household]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[schedulingTargetId] INT NULL,		-- Link to the SchedulingTarget table.
	[formalSalutation] NVARCHAR(250) NULL,		-- ex. "Mr. and Mrs. John Smith"
	[informalSalutation] NVARCHAR(250) NULL,		-- ex. "John and Jane"
	[addressee] NVARCHAR(250) NULL,		-- The label for the envelope
	[totalHouseholdGiving] MONEY NOT NULL DEFAULT 0,
	[lastHouseholdGiftDate] DATE NULL,
	[notes] NVARCHAR(MAX) NULL,
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[avatarFileName] NVARCHAR(250) NULL,		-- Part of the binary data field setup
	[avatarSize] BIGINT NULL,		-- Part of the binary data field setup
	[avatarData] VARBINARY(MAX) NULL,		-- Part of the binary data field setup
	[avatarMimeType] NVARCHAR(100) NULL,		-- Part of the binary data field setup
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Household_SchedulingTarget_schedulingTargetId] FOREIGN KEY ([schedulingTargetId]) REFERENCES [Scheduler].[SchedulingTarget] ([id]),		-- Foreign key to the SchedulingTarget table.
	CONSTRAINT [FK_Household_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_Household_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Household table's tenantGuid and name fields.
)
GO

-- Index on the Household table's tenantGuid field.
CREATE INDEX [I_Household_tenantGuid] ON [Scheduler].[Household] ([tenantGuid])
GO

-- Index on the Household table's tenantGuid,name fields.
CREATE INDEX [I_Household_tenantGuid_name] ON [Scheduler].[Household] ([tenantGuid], [name])
GO

-- Index on the Household table's tenantGuid,schedulingTargetId fields.
CREATE INDEX [I_Household_tenantGuid_schedulingTargetId] ON [Scheduler].[Household] ([tenantGuid], [schedulingTargetId])
GO

-- Index on the Household table's tenantGuid,iconId fields.
CREATE INDEX [I_Household_tenantGuid_iconId] ON [Scheduler].[Household] ([tenantGuid], [iconId])
GO

-- Index on the Household table's tenantGuid,active fields.
CREATE INDEX [I_Household_tenantGuid_active] ON [Scheduler].[Household] ([tenantGuid], [active])
GO

-- Index on the Household table's tenantGuid,deleted fields.
CREATE INDEX [I_Household_tenantGuid_deleted] ON [Scheduler].[Household] ([tenantGuid], [deleted])
GO


-- The change history for records from the Household table.
CREATE TABLE [Scheduler].[HouseholdChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[householdId] INT NOT NULL,		-- Link to the Household table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_HouseholdChangeHistory_Household_householdId] FOREIGN KEY ([householdId]) REFERENCES [Scheduler].[Household] ([id])		-- Foreign key to the Household table.
)
GO

-- Index on the HouseholdChangeHistory table's tenantGuid field.
CREATE INDEX [I_HouseholdChangeHistory_tenantGuid] ON [Scheduler].[HouseholdChangeHistory] ([tenantGuid])
GO

-- Index on the HouseholdChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_HouseholdChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[HouseholdChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the HouseholdChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_HouseholdChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[HouseholdChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the HouseholdChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_HouseholdChangeHistory_tenantGuid_userId] ON [Scheduler].[HouseholdChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the HouseholdChangeHistory table's tenantGuid,householdId fields.
CREATE INDEX [I_HouseholdChangeHistory_tenantGuid_householdId] ON [Scheduler].[HouseholdChangeHistory] ([tenantGuid], [householdId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Defines stages in a donor's journey (e.g., Target, Qualified, Cultivated, Solicited, Stewardship).
CREATE TABLE [Scheduler].[ConstituentJourneyStage]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[minLifetimeGiving] MONEY NULL,		-- Optional criteria: Minimum total giving to qualify for this stage.
	[maxLifetimeGiving] MONEY NULL,		-- Optional criteria: Maximum total giving
	[minSingleGiftAmount] MONEY NULL,		-- Optional criteria: Min single gift size
	[isDefault] BIT NOT NULL,		-- If true, this is the default stage for new constituents.
	[minAnnualGiving] MONEY NULL,		-- Optional: Minimum giving in the past 365 days.
	[maxDaysSinceLastGift] INT NULL DEFAULT 0,		-- Optional: Maximum days elapsed since the last gift (recency limit).
	[minGiftCount] INT NULL DEFAULT 0,		-- Optional: Minimum number of gifts required.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ConstituentJourneyStage_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_ConstituentJourneyStage_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the ConstituentJourneyStage table's tenantGuid and name fields.
)
GO

-- Index on the ConstituentJourneyStage table's tenantGuid field.
CREATE INDEX [I_ConstituentJourneyStage_tenantGuid] ON [Scheduler].[ConstituentJourneyStage] ([tenantGuid])
GO

-- Index on the ConstituentJourneyStage table's tenantGuid,name fields.
CREATE INDEX [I_ConstituentJourneyStage_tenantGuid_name] ON [Scheduler].[ConstituentJourneyStage] ([tenantGuid], [name])
GO

-- Index on the ConstituentJourneyStage table's tenantGuid,iconId fields.
CREATE INDEX [I_ConstituentJourneyStage_tenantGuid_iconId] ON [Scheduler].[ConstituentJourneyStage] ([tenantGuid], [iconId])
GO

-- Index on the ConstituentJourneyStage table's tenantGuid,active fields.
CREATE INDEX [I_ConstituentJourneyStage_tenantGuid_active] ON [Scheduler].[ConstituentJourneyStage] ([tenantGuid], [active])
GO

-- Index on the ConstituentJourneyStage table's tenantGuid,deleted fields.
CREATE INDEX [I_ConstituentJourneyStage_tenantGuid_deleted] ON [Scheduler].[ConstituentJourneyStage] ([tenantGuid], [deleted])
GO

INSERT INTO [Scheduler].[ConstituentJourneyStage] ( [tenantGuid], [name], [description], [sequence], [isDefault], [color], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Unqualified', 'New potential donor.', 1, 1, '#9E9E9E', 'd8663e5e-749c-4638-b69d-21d96078659d' )
GO

INSERT INTO [Scheduler].[ConstituentJourneyStage] ( [tenantGuid], [name], [description], [sequence], [isDefault], [color], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Qualified', 'Donor has been qualified.', 2, 0, '#2196F3', 'ad06353d-2476-4322-836f-5374825968f9' )
GO

INSERT INTO [Scheduler].[ConstituentJourneyStage] ( [tenantGuid], [name], [description], [sequence], [isDefault], [color], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Cultivated', 'Relationship is being built.', 3, 0, '#4CAF50', 'e8b60384-9336-4022-8b4b-970752538965' )
GO

INSERT INTO [Scheduler].[ConstituentJourneyStage] ( [tenantGuid], [name], [description], [sequence], [isDefault], [color], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Solicited', 'Ask has been made.', 4, 0, '#FF9800', '64319688-fd06-4074-8902-628670bf7471' )
GO

INSERT INTO [Scheduler].[ConstituentJourneyStage] ( [tenantGuid], [name], [description], [sequence], [isDefault], [color], [objectGuid] ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Stewardship', 'Ongoing maintenance.', 5, 0, '#9C27B0', '1d971578-8319-482a-9e8c-529141873837' )
GO


-- The change history for records from the ConstituentJourneyStage table.
CREATE TABLE [Scheduler].[ConstituentJourneyStageChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[constituentJourneyStageId] INT NOT NULL,		-- Link to the ConstituentJourneyStage table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ConstituentJourneyStageChangeHistory_ConstituentJourneyStage_constituentJourneyStageId] FOREIGN KEY ([constituentJourneyStageId]) REFERENCES [Scheduler].[ConstituentJourneyStage] ([id])		-- Foreign key to the ConstituentJourneyStage table.
)
GO

-- Index on the ConstituentJourneyStageChangeHistory table's tenantGuid field.
CREATE INDEX [I_ConstituentJourneyStageChangeHistory_tenantGuid] ON [Scheduler].[ConstituentJourneyStageChangeHistory] ([tenantGuid])
GO

-- Index on the ConstituentJourneyStageChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ConstituentJourneyStageChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ConstituentJourneyStageChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ConstituentJourneyStageChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ConstituentJourneyStageChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ConstituentJourneyStageChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ConstituentJourneyStageChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ConstituentJourneyStageChangeHistory_tenantGuid_userId] ON [Scheduler].[ConstituentJourneyStageChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ConstituentJourneyStageChangeHistory table's tenantGuid,constituentJourneyStageId fields.
CREATE INDEX [I_ConstituentJourneyStageChangeHistory_tenantGuid_constituentJourneyStageId] ON [Scheduler].[ConstituentJourneyStageChangeHistory] ([tenantGuid], [constituentJourneyStageId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


/*
 ====================================================================================================
   CONSTITUENT MANAGEMENT
   In DP, a Constituent is the heart of the system. 
   Here, we link to your existing Contact (Person) or Client (Organization) tables.
   This table stores the "Fundraising Intelligence" (RFM metrics).
   ====================================================================================================
*/
CREATE TABLE [Scheduler].[Constituent]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[contactId] INT NULL,		-- Polymorphic Link: A constituent is EITHER a generic Contact OR a Client (Company)
	[clientId] INT NULL,		-- Polymorphic Link: A constituent is EITHER a generic Contact OR a Client (Company)
	[householdId] INT NULL,		-- Links a constituent to a household
	[constituentNumber] NVARCHAR(50) NOT NULL,		-- The distinct 'Donor ID'
	[doNotSolicit] BIT NOT NULL DEFAULT 0,
	[doNotEmail] BIT NOT NULL DEFAULT 0,
	[doNotMail] BIT NOT NULL DEFAULT 0,
	[totalLifetimeGiving] MONEY NOT NULL DEFAULT 0,		-- Caching Calculated Metrics(DP Style for fast reporting)
	[totalYTDGiving] MONEY NOT NULL DEFAULT 0,		-- Caching Calculated Metrics(DP Style for fast reporting)
	[lastGiftDate] DATE NULL,		-- Caching Calculated Metrics(DP Style for fast reporting)
	[lastGiftAmount] MONEY NULL,		-- Caching Calculated Metrics(DP Style for fast reporting)
	[largestGiftAmount] MONEY NULL,		-- Caching Calculated Metrics(DP Style for fast reporting)
	[totalGiftCount] INT NULL,		-- Caching Calculated Metrics(DP Style for fast reporting)
	[externalId] NVARCHAR(100) NULL,		-- For things like QBO Customer ID
	[notes] NVARCHAR(MAX) NULL,
	[constituentJourneyStageId] INT NULL,		-- Current stage in the donor journey.
	[dateEnteredCurrentStage] DATETIME2(7) NULL,		-- Date when the constituent moved to the current stage.
	[attributes] NVARCHAR(MAX) NULL,		-- to store arbitrary JSON
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[avatarFileName] NVARCHAR(250) NULL,		-- Part of the binary data field setup
	[avatarSize] BIGINT NULL,		-- Part of the binary data field setup
	[avatarData] VARBINARY(MAX) NULL,		-- Part of the binary data field setup
	[avatarMimeType] NVARCHAR(100) NULL,		-- Part of the binary data field setup
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Constituent_Contact_contactId] FOREIGN KEY ([contactId]) REFERENCES [Scheduler].[Contact] ([id]),		-- Foreign key to the Contact table.
	CONSTRAINT [FK_Constituent_Client_clientId] FOREIGN KEY ([clientId]) REFERENCES [Scheduler].[Client] ([id]),		-- Foreign key to the Client table.
	CONSTRAINT [FK_Constituent_Household_householdId] FOREIGN KEY ([householdId]) REFERENCES [Scheduler].[Household] ([id]),		-- Foreign key to the Household table.
	CONSTRAINT [FK_Constituent_ConstituentJourneyStage_constituentJourneyStageId] FOREIGN KEY ([constituentJourneyStageId]) REFERENCES [Scheduler].[ConstituentJourneyStage] ([id]),		-- Foreign key to the ConstituentJourneyStage table.
	CONSTRAINT [FK_Constituent_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id])		-- Foreign key to the Icon table.
)
GO

-- Index on the Constituent table's tenantGuid field.
CREATE INDEX [I_Constituent_tenantGuid] ON [Scheduler].[Constituent] ([tenantGuid])
GO

-- Index on the Constituent table's tenantGuid,contactId fields.
CREATE INDEX [I_Constituent_tenantGuid_contactId] ON [Scheduler].[Constituent] ([tenantGuid], [contactId])
GO

-- Index on the Constituent table's tenantGuid,clientId fields.
CREATE INDEX [I_Constituent_tenantGuid_clientId] ON [Scheduler].[Constituent] ([tenantGuid], [clientId])
GO

-- Index on the Constituent table's tenantGuid,householdId fields.
CREATE INDEX [I_Constituent_tenantGuid_householdId] ON [Scheduler].[Constituent] ([tenantGuid], [householdId])
GO

-- Index on the Constituent table's tenantGuid,constituentJourneyStageId fields.
CREATE INDEX [I_Constituent_tenantGuid_constituentJourneyStageId] ON [Scheduler].[Constituent] ([tenantGuid], [constituentJourneyStageId])
GO

-- Index on the Constituent table's tenantGuid,iconId fields.
CREATE INDEX [I_Constituent_tenantGuid_iconId] ON [Scheduler].[Constituent] ([tenantGuid], [iconId])
GO

-- Index on the Constituent table's tenantGuid,active fields.
CREATE INDEX [I_Constituent_tenantGuid_active] ON [Scheduler].[Constituent] ([tenantGuid], [active])
GO

-- Index on the Constituent table's tenantGuid,deleted fields.
CREATE INDEX [I_Constituent_tenantGuid_deleted] ON [Scheduler].[Constituent] ([tenantGuid], [deleted])
GO


-- The change history for records from the Constituent table.
CREATE TABLE [Scheduler].[ConstituentChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[constituentId] INT NOT NULL,		-- Link to the Constituent table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ConstituentChangeHistory_Constituent_constituentId] FOREIGN KEY ([constituentId]) REFERENCES [Scheduler].[Constituent] ([id])		-- Foreign key to the Constituent table.
)
GO

-- Index on the ConstituentChangeHistory table's tenantGuid field.
CREATE INDEX [I_ConstituentChangeHistory_tenantGuid] ON [Scheduler].[ConstituentChangeHistory] ([tenantGuid])
GO

-- Index on the ConstituentChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ConstituentChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[ConstituentChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ConstituentChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ConstituentChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[ConstituentChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ConstituentChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ConstituentChangeHistory_tenantGuid_userId] ON [Scheduler].[ConstituentChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ConstituentChangeHistory table's tenantGuid,constituentId fields.
CREATE INDEX [I_ConstituentChangeHistory_tenantGuid_constituentId] ON [Scheduler].[ConstituentChangeHistory] ([tenantGuid], [constituentId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


/*
 ====================================================================================================
   PLEDGES
   A promise to pay. Gifts will link to this to "pay it down".
   ====================================================================================================
*/
CREATE TABLE [Scheduler].[Pledge]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[constituentId] INT NOT NULL,		-- Link to the Constituent table.
	[totalAmount] MONEY NOT NULL,
	[balanceAmount] MONEY NOT NULL,		-- Calculated: Total - Sum(LinkedGifts)
	[pledgeDate] DATE NOT NULL,
	[startDate] DATE NULL,
	[endDate] DATE NULL,
	[recurrenceFrequencyId] INT NULL,		-- Link to the RecurrenceFrequency table.
	[fundId] INT NOT NULL,		-- Link to the Fund table.
	[campaignId] INT NULL,		-- Link to the Campaign table.
	[appealId] INT NULL,		-- Link to the Appeal table.
	[writeOffAmount] MONEY NOT NULL,		-- If they default on the pledge
	[isWrittenOff] BIT NOT NULL DEFAULT 0,
	[notes] NVARCHAR(MAX) NULL,
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Pledge_Constituent_constituentId] FOREIGN KEY ([constituentId]) REFERENCES [Scheduler].[Constituent] ([id]),		-- Foreign key to the Constituent table.
	CONSTRAINT [FK_Pledge_RecurrenceFrequency_recurrenceFrequencyId] FOREIGN KEY ([recurrenceFrequencyId]) REFERENCES [Scheduler].[RecurrenceFrequency] ([id]),		-- Foreign key to the RecurrenceFrequency table.
	CONSTRAINT [FK_Pledge_Fund_fundId] FOREIGN KEY ([fundId]) REFERENCES [Scheduler].[Fund] ([id]),		-- Foreign key to the Fund table.
	CONSTRAINT [FK_Pledge_Campaign_campaignId] FOREIGN KEY ([campaignId]) REFERENCES [Scheduler].[Campaign] ([id]),		-- Foreign key to the Campaign table.
	CONSTRAINT [FK_Pledge_Appeal_appealId] FOREIGN KEY ([appealId]) REFERENCES [Scheduler].[Appeal] ([id])		-- Foreign key to the Appeal table.
)
GO

-- Index on the Pledge table's tenantGuid field.
CREATE INDEX [I_Pledge_tenantGuid] ON [Scheduler].[Pledge] ([tenantGuid])
GO

-- Index on the Pledge table's tenantGuid,constituentId fields.
CREATE INDEX [I_Pledge_tenantGuid_constituentId] ON [Scheduler].[Pledge] ([tenantGuid], [constituentId])
GO

-- Index on the Pledge table's tenantGuid,recurrenceFrequencyId fields.
CREATE INDEX [I_Pledge_tenantGuid_recurrenceFrequencyId] ON [Scheduler].[Pledge] ([tenantGuid], [recurrenceFrequencyId])
GO

-- Index on the Pledge table's tenantGuid,fundId fields.
CREATE INDEX [I_Pledge_tenantGuid_fundId] ON [Scheduler].[Pledge] ([tenantGuid], [fundId])
GO

-- Index on the Pledge table's tenantGuid,campaignId fields.
CREATE INDEX [I_Pledge_tenantGuid_campaignId] ON [Scheduler].[Pledge] ([tenantGuid], [campaignId])
GO

-- Index on the Pledge table's tenantGuid,appealId fields.
CREATE INDEX [I_Pledge_tenantGuid_appealId] ON [Scheduler].[Pledge] ([tenantGuid], [appealId])
GO

-- Index on the Pledge table's tenantGuid,active fields.
CREATE INDEX [I_Pledge_tenantGuid_active] ON [Scheduler].[Pledge] ([tenantGuid], [active])
GO

-- Index on the Pledge table's tenantGuid,deleted fields.
CREATE INDEX [I_Pledge_tenantGuid_deleted] ON [Scheduler].[Pledge] ([tenantGuid], [deleted])
GO


-- The change history for records from the Pledge table.
CREATE TABLE [Scheduler].[PledgeChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[pledgeId] INT NOT NULL,		-- Link to the Pledge table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_PledgeChangeHistory_Pledge_pledgeId] FOREIGN KEY ([pledgeId]) REFERENCES [Scheduler].[Pledge] ([id])		-- Foreign key to the Pledge table.
)
GO

-- Index on the PledgeChangeHistory table's tenantGuid field.
CREATE INDEX [I_PledgeChangeHistory_tenantGuid] ON [Scheduler].[PledgeChangeHistory] ([tenantGuid])
GO

-- Index on the PledgeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_PledgeChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[PledgeChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the PledgeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_PledgeChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[PledgeChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the PledgeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_PledgeChangeHistory_tenantGuid_userId] ON [Scheduler].[PledgeChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the PledgeChangeHistory table's tenantGuid,pledgeId fields.
CREATE INDEX [I_PledgeChangeHistory_tenantGuid_pledgeId] ON [Scheduler].[PledgeChangeHistory] ([tenantGuid], [pledgeId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Master list of tribute types ( memory, honor, etc..)
CREATE TABLE [Scheduler].[TributeType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the TributeType table's name field.
CREATE INDEX [I_TributeType_name] ON [Scheduler].[TributeType] ([name])
GO

-- Index on the TributeType table's active field.
CREATE INDEX [I_TributeType_active] ON [Scheduler].[TributeType] ([active])
GO

-- Index on the TributeType table's deleted field.
CREATE INDEX [I_TributeType_deleted] ON [Scheduler].[TributeType] ([deleted])
GO

INSERT INTO [Scheduler].[TributeType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'In Memory Of', 'In Memory Of', 1, '27781845-ed5e-4bba-9216-751d5a8d778a' )
GO

INSERT INTO [Scheduler].[TributeType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'In Honor Of', 'In Honor Of', 2, '31af7566-28d1-460f-9cd9-9d70711b5983' )
GO


/*
====================================================================================================
   BATCH CONTROL
   This prevents data entry errors by forcing the user to balance "Control Totals" vs "Actual Totals".
   ====================================================================================================
*/
CREATE TABLE [Scheduler].[BatchStatus]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the BatchStatus table's name field.
CREATE INDEX [I_BatchStatus_name] ON [Scheduler].[BatchStatus] ([name])
GO

-- Index on the BatchStatus table's active field.
CREATE INDEX [I_BatchStatus_active] ON [Scheduler].[BatchStatus] ([active])
GO

-- Index on the BatchStatus table's deleted field.
CREATE INDEX [I_BatchStatus_deleted] ON [Scheduler].[BatchStatus] ([deleted])
GO

INSERT INTO [Scheduler].[BatchStatus] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Open', 'Data entry in progress', 1, 'd87c06b0-9b5e-4597-8968-ad5f987e2afd' )
GO

INSERT INTO [Scheduler].[BatchStatus] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Balanced', 'Control totals match entry totals', 2, 'b5942c13-47d1-4753-a655-140454e1d0a4' )
GO

INSERT INTO [Scheduler].[BatchStatus] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Posted', 'Transactions committed to GL / Donor History', 3, '640a7bb7-59da-423b-b2e5-a10124594331' )
GO

INSERT INTO [Scheduler].[BatchStatus] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Closed', 'Closed', 4, '5c60e28a-ba9f-4098-9a04-50fcb139bd8c' )
GO


-- The Batch Header for processing gifts.
CREATE TABLE [Scheduler].[Batch]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[batchNumber] NVARCHAR(100) NOT NULL,		-- User-facing ID (e.g., "2026-01-15-MAIL"
	[description] NVARCHAR(500) NULL,
	[dateOpened] DATETIME2(7) NOT NULL,
	[datePosted] DATETIME2(7) NULL,
	[batchStatusId] INT NOT NULL,		-- Link to the BatchStatus table.
	[controlAmount] MONEY NOT NULL DEFAULT 0,
	[controlCount] INT NOT NULL DEFAULT 0,
	[defaultFundId] INT NULL,		-- Optional default fund
	[defaultCampaignId] INT NULL,		-- Optional default campaign
	[defaultAppealId] INT NULL,		-- Optional default appeal
	[defaultDate] DATE NULL,		-- Optional default date
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Batch_BatchStatus_batchStatusId] FOREIGN KEY ([batchStatusId]) REFERENCES [Scheduler].[BatchStatus] ([id]),		-- Foreign key to the BatchStatus table.
	CONSTRAINT [FK_Batch_Fund_defaultFundId] FOREIGN KEY ([defaultFundId]) REFERENCES [Scheduler].[Fund] ([id]),		-- Foreign key to the Fund table.
	CONSTRAINT [FK_Batch_Campaign_defaultCampaignId] FOREIGN KEY ([defaultCampaignId]) REFERENCES [Scheduler].[Campaign] ([id]),		-- Foreign key to the Campaign table.
	CONSTRAINT [FK_Batch_Appeal_defaultAppealId] FOREIGN KEY ([defaultAppealId]) REFERENCES [Scheduler].[Appeal] ([id])		-- Foreign key to the Appeal table.
)
GO

-- Index on the Batch table's tenantGuid field.
CREATE INDEX [I_Batch_tenantGuid] ON [Scheduler].[Batch] ([tenantGuid])
GO

-- Index on the Batch table's tenantGuid,batchStatusId fields.
CREATE INDEX [I_Batch_tenantGuid_batchStatusId] ON [Scheduler].[Batch] ([tenantGuid], [batchStatusId])
GO

-- Index on the Batch table's tenantGuid,defaultFundId fields.
CREATE INDEX [I_Batch_tenantGuid_defaultFundId] ON [Scheduler].[Batch] ([tenantGuid], [defaultFundId])
GO

-- Index on the Batch table's tenantGuid,defaultCampaignId fields.
CREATE INDEX [I_Batch_tenantGuid_defaultCampaignId] ON [Scheduler].[Batch] ([tenantGuid], [defaultCampaignId])
GO

-- Index on the Batch table's tenantGuid,defaultAppealId fields.
CREATE INDEX [I_Batch_tenantGuid_defaultAppealId] ON [Scheduler].[Batch] ([tenantGuid], [defaultAppealId])
GO

-- Index on the Batch table's tenantGuid,active fields.
CREATE INDEX [I_Batch_tenantGuid_active] ON [Scheduler].[Batch] ([tenantGuid], [active])
GO

-- Index on the Batch table's tenantGuid,deleted fields.
CREATE INDEX [I_Batch_tenantGuid_deleted] ON [Scheduler].[Batch] ([tenantGuid], [deleted])
GO


-- The change history for records from the Batch table.
CREATE TABLE [Scheduler].[BatchChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[batchId] INT NOT NULL,		-- Link to the Batch table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_BatchChangeHistory_Batch_batchId] FOREIGN KEY ([batchId]) REFERENCES [Scheduler].[Batch] ([id])		-- Foreign key to the Batch table.
)
GO

-- Index on the BatchChangeHistory table's tenantGuid field.
CREATE INDEX [I_BatchChangeHistory_tenantGuid] ON [Scheduler].[BatchChangeHistory] ([tenantGuid])
GO

-- Index on the BatchChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_BatchChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[BatchChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the BatchChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_BatchChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[BatchChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the BatchChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_BatchChangeHistory_tenantGuid_userId] ON [Scheduler].[BatchChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the BatchChangeHistory table's tenantGuid,batchId fields.
CREATE INDEX [I_BatchChangeHistory_tenantGuid_batchId] ON [Scheduler].[BatchChangeHistory] ([tenantGuid], [batchId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- The Tribute Definition (e.g., "The John Doe Memorial Fund")
CREATE TABLE [Scheduler].[Tribute]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[tributeTypeId] INT NULL,		-- Link to the TributeType table.
	[defaultAcknowledgeeId] INT NULL,		-- Constituent to notify (e.g., the widow)
	[startDate] DATE NULL,
	[endDate] DATE NULL,
	[iconId] INT NULL,		-- Icon to use for UI display
	[color] NVARCHAR(10) NULL,		-- Hex color for UI display
	[avatarFileName] NVARCHAR(250) NULL,		-- Part of the binary data field setup
	[avatarSize] BIGINT NULL,		-- Part of the binary data field setup
	[avatarData] VARBINARY(MAX) NULL,		-- Part of the binary data field setup
	[avatarMimeType] NVARCHAR(100) NULL,		-- Part of the binary data field setup
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Tribute_TributeType_tributeTypeId] FOREIGN KEY ([tributeTypeId]) REFERENCES [Scheduler].[TributeType] ([id]),		-- Foreign key to the TributeType table.
	CONSTRAINT [FK_Tribute_Constituent_defaultAcknowledgeeId] FOREIGN KEY ([defaultAcknowledgeeId]) REFERENCES [Scheduler].[Constituent] ([id]),		-- Foreign key to the Constituent table.
	CONSTRAINT [FK_Tribute_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_Tribute_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Tribute table's tenantGuid and name fields.
)
GO

-- Index on the Tribute table's tenantGuid field.
CREATE INDEX [I_Tribute_tenantGuid] ON [Scheduler].[Tribute] ([tenantGuid])
GO

-- Index on the Tribute table's tenantGuid,name fields.
CREATE INDEX [I_Tribute_tenantGuid_name] ON [Scheduler].[Tribute] ([tenantGuid], [name])
GO

-- Index on the Tribute table's tenantGuid,tributeTypeId fields.
CREATE INDEX [I_Tribute_tenantGuid_tributeTypeId] ON [Scheduler].[Tribute] ([tenantGuid], [tributeTypeId])
GO

-- Index on the Tribute table's tenantGuid,iconId fields.
CREATE INDEX [I_Tribute_tenantGuid_iconId] ON [Scheduler].[Tribute] ([tenantGuid], [iconId])
GO

-- Index on the Tribute table's tenantGuid,active fields.
CREATE INDEX [I_Tribute_tenantGuid_active] ON [Scheduler].[Tribute] ([tenantGuid], [active])
GO

-- Index on the Tribute table's tenantGuid,deleted fields.
CREATE INDEX [I_Tribute_tenantGuid_deleted] ON [Scheduler].[Tribute] ([tenantGuid], [deleted])
GO


-- The change history for records from the Tribute table.
CREATE TABLE [Scheduler].[TributeChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[tributeId] INT NOT NULL,		-- Link to the Tribute table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_TributeChangeHistory_Tribute_tributeId] FOREIGN KEY ([tributeId]) REFERENCES [Scheduler].[Tribute] ([id])		-- Foreign key to the Tribute table.
)
GO

-- Index on the TributeChangeHistory table's tenantGuid field.
CREATE INDEX [I_TributeChangeHistory_tenantGuid] ON [Scheduler].[TributeChangeHistory] ([tenantGuid])
GO

-- Index on the TributeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_TributeChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[TributeChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the TributeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_TributeChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[TributeChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the TributeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_TributeChangeHistory_tenantGuid_userId] ON [Scheduler].[TributeChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the TributeChangeHistory table's tenantGuid,tributeId fields.
CREATE INDEX [I_TributeChangeHistory_tenantGuid_tributeId] ON [Scheduler].[TributeChangeHistory] ([tenantGuid], [tributeId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


/*
  ====================================================================================================
   GIFTS (Transactions)
   The money coming in.
   ====================================================================================================
*/
CREATE TABLE [Scheduler].[Gift]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[officeId] INT NULL,		-- Which office received/owns this gift
	[constituentId] INT NOT NULL,		-- Link to the Constituent table.
	[pledgeId] INT NULL,		-- Link to the Pledge table.
	[amount] MONEY NOT NULL,
	[receivedDate] DATETIME2(7) NOT NULL,		-- When it was recieved
	[postedDate] DATETIME2(7) NULL,		-- When it hit the GL
	[fundId] INT NOT NULL,		-- Link to the Fund table.
	[campaignId] INT NULL,		-- Link to the Campaign table.
	[appealId] INT NULL,		-- Link to the Appeal table.
	[paymentTypeId] INT NOT NULL,		-- Link to the PaymentType table.
	[referenceNumber] NVARCHAR(100) NULL,		-- Check # or Transaction ID
	[batchId] INT NULL,		-- Link to processing batch
	[receiptTypeId] INT NULL,		-- Link to the ReceiptType table.
	[receiptDate] DATETIME2(7) NULL,
	[tributeId] INT NULL,		-- Link to the Tribute table.
	[notes] NVARCHAR(MAX) NULL,
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Gift_Office_officeId] FOREIGN KEY ([officeId]) REFERENCES [Scheduler].[Office] ([id]),		-- Foreign key to the Office table.
	CONSTRAINT [FK_Gift_Constituent_constituentId] FOREIGN KEY ([constituentId]) REFERENCES [Scheduler].[Constituent] ([id]),		-- Foreign key to the Constituent table.
	CONSTRAINT [FK_Gift_Pledge_pledgeId] FOREIGN KEY ([pledgeId]) REFERENCES [Scheduler].[Pledge] ([id]),		-- Foreign key to the Pledge table.
	CONSTRAINT [FK_Gift_Fund_fundId] FOREIGN KEY ([fundId]) REFERENCES [Scheduler].[Fund] ([id]),		-- Foreign key to the Fund table.
	CONSTRAINT [FK_Gift_Campaign_campaignId] FOREIGN KEY ([campaignId]) REFERENCES [Scheduler].[Campaign] ([id]),		-- Foreign key to the Campaign table.
	CONSTRAINT [FK_Gift_Appeal_appealId] FOREIGN KEY ([appealId]) REFERENCES [Scheduler].[Appeal] ([id]),		-- Foreign key to the Appeal table.
	CONSTRAINT [FK_Gift_PaymentType_paymentTypeId] FOREIGN KEY ([paymentTypeId]) REFERENCES [Scheduler].[PaymentType] ([id]),		-- Foreign key to the PaymentType table.
	CONSTRAINT [FK_Gift_Batch_batchId] FOREIGN KEY ([batchId]) REFERENCES [Scheduler].[Batch] ([id]),		-- Foreign key to the Batch table.
	CONSTRAINT [FK_Gift_ReceiptType_receiptTypeId] FOREIGN KEY ([receiptTypeId]) REFERENCES [Scheduler].[ReceiptType] ([id]),		-- Foreign key to the ReceiptType table.
	CONSTRAINT [FK_Gift_Tribute_tributeId] FOREIGN KEY ([tributeId]) REFERENCES [Scheduler].[Tribute] ([id])		-- Foreign key to the Tribute table.
)
GO

-- Index on the Gift table's tenantGuid field.
CREATE INDEX [I_Gift_tenantGuid] ON [Scheduler].[Gift] ([tenantGuid])
GO

-- Index on the Gift table's tenantGuid,officeId fields.
CREATE INDEX [I_Gift_tenantGuid_officeId] ON [Scheduler].[Gift] ([tenantGuid], [officeId])
GO

-- Index on the Gift table's tenantGuid,constituentId fields.
CREATE INDEX [I_Gift_tenantGuid_constituentId] ON [Scheduler].[Gift] ([tenantGuid], [constituentId])
GO

-- Index on the Gift table's tenantGuid,pledgeId fields.
CREATE INDEX [I_Gift_tenantGuid_pledgeId] ON [Scheduler].[Gift] ([tenantGuid], [pledgeId])
GO

-- Index on the Gift table's tenantGuid,fundId fields.
CREATE INDEX [I_Gift_tenantGuid_fundId] ON [Scheduler].[Gift] ([tenantGuid], [fundId])
GO

-- Index on the Gift table's tenantGuid,campaignId fields.
CREATE INDEX [I_Gift_tenantGuid_campaignId] ON [Scheduler].[Gift] ([tenantGuid], [campaignId])
GO

-- Index on the Gift table's tenantGuid,appealId fields.
CREATE INDEX [I_Gift_tenantGuid_appealId] ON [Scheduler].[Gift] ([tenantGuid], [appealId])
GO

-- Index on the Gift table's tenantGuid,paymentTypeId fields.
CREATE INDEX [I_Gift_tenantGuid_paymentTypeId] ON [Scheduler].[Gift] ([tenantGuid], [paymentTypeId])
GO

-- Index on the Gift table's tenantGuid,batchId fields.
CREATE INDEX [I_Gift_tenantGuid_batchId] ON [Scheduler].[Gift] ([tenantGuid], [batchId])
GO

-- Index on the Gift table's tenantGuid,receiptTypeId fields.
CREATE INDEX [I_Gift_tenantGuid_receiptTypeId] ON [Scheduler].[Gift] ([tenantGuid], [receiptTypeId])
GO

-- Index on the Gift table's tenantGuid,tributeId fields.
CREATE INDEX [I_Gift_tenantGuid_tributeId] ON [Scheduler].[Gift] ([tenantGuid], [tributeId])
GO

-- Index on the Gift table's tenantGuid,active fields.
CREATE INDEX [I_Gift_tenantGuid_active] ON [Scheduler].[Gift] ([tenantGuid], [active])
GO

-- Index on the Gift table's tenantGuid,deleted fields.
CREATE INDEX [I_Gift_tenantGuid_deleted] ON [Scheduler].[Gift] ([tenantGuid], [deleted])
GO


-- The change history for records from the Gift table.
CREATE TABLE [Scheduler].[GiftChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[giftId] INT NOT NULL,		-- Link to the Gift table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_GiftChangeHistory_Gift_giftId] FOREIGN KEY ([giftId]) REFERENCES [Scheduler].[Gift] ([id])		-- Foreign key to the Gift table.
)
GO

-- Index on the GiftChangeHistory table's tenantGuid field.
CREATE INDEX [I_GiftChangeHistory_tenantGuid] ON [Scheduler].[GiftChangeHistory] ([tenantGuid])
GO

-- Index on the GiftChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_GiftChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[GiftChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the GiftChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_GiftChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[GiftChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the GiftChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_GiftChangeHistory_tenantGuid_userId] ON [Scheduler].[GiftChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the GiftChangeHistory table's tenantGuid,giftId fields.
CREATE INDEX [I_GiftChangeHistory_tenantGuid_giftId] ON [Scheduler].[GiftChangeHistory] ([tenantGuid], [giftId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


/*
====================================================================================================
   SOFT CREDITS
   Critical for DP functionality. Allows a gift from "Husband" to also show up on "Wife's" record 
   without doubling the financial totals.
   ====================================================================================================
*/
CREATE TABLE [Scheduler].[SoftCredit]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[giftId] INT NOT NULL,		-- Link to the Gift table.
	[constituentId] INT NOT NULL,		-- The person getting the soft credit
	[amount] MONEY NOT NULL,		-- Might be full amount or partial
	[notes] NVARCHAR(MAX) NULL,
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SoftCredit_Gift_giftId] FOREIGN KEY ([giftId]) REFERENCES [Scheduler].[Gift] ([id]),		-- Foreign key to the Gift table.
	CONSTRAINT [FK_SoftCredit_Constituent_constituentId] FOREIGN KEY ([constituentId]) REFERENCES [Scheduler].[Constituent] ([id])		-- Foreign key to the Constituent table.
)
GO

-- Index on the SoftCredit table's tenantGuid field.
CREATE INDEX [I_SoftCredit_tenantGuid] ON [Scheduler].[SoftCredit] ([tenantGuid])
GO

-- Index on the SoftCredit table's tenantGuid,giftId fields.
CREATE INDEX [I_SoftCredit_tenantGuid_giftId] ON [Scheduler].[SoftCredit] ([tenantGuid], [giftId])
GO

-- Index on the SoftCredit table's tenantGuid,constituentId fields.
CREATE INDEX [I_SoftCredit_tenantGuid_constituentId] ON [Scheduler].[SoftCredit] ([tenantGuid], [constituentId])
GO

-- Index on the SoftCredit table's tenantGuid,active fields.
CREATE INDEX [I_SoftCredit_tenantGuid_active] ON [Scheduler].[SoftCredit] ([tenantGuid], [active])
GO

-- Index on the SoftCredit table's tenantGuid,deleted fields.
CREATE INDEX [I_SoftCredit_tenantGuid_deleted] ON [Scheduler].[SoftCredit] ([tenantGuid], [deleted])
GO


-- The change history for records from the SoftCredit table.
CREATE TABLE [Scheduler].[SoftCreditChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[softCreditId] INT NOT NULL,		-- Link to the SoftCredit table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_SoftCreditChangeHistory_SoftCredit_softCreditId] FOREIGN KEY ([softCreditId]) REFERENCES [Scheduler].[SoftCredit] ([id])		-- Foreign key to the SoftCredit table.
)
GO

-- Index on the SoftCreditChangeHistory table's tenantGuid field.
CREATE INDEX [I_SoftCreditChangeHistory_tenantGuid] ON [Scheduler].[SoftCreditChangeHistory] ([tenantGuid])
GO

-- Index on the SoftCreditChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_SoftCreditChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[SoftCreditChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the SoftCreditChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_SoftCreditChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[SoftCreditChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the SoftCreditChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_SoftCreditChangeHistory_tenantGuid_userId] ON [Scheduler].[SoftCreditChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the SoftCreditChangeHistory table's tenantGuid,softCreditId fields.
CREATE INDEX [I_SoftCreditChangeHistory_tenantGuid_softCreditId] ON [Scheduler].[SoftCreditChangeHistory] ([tenantGuid], [softCreditId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


/*
Volunteer-specific extended profile.
One-to-one with Resource — allows volunteers to be scheduled just like paid resources
while carrying volunteer-specific metadata, hours tracking, preferences, etc.
*/
CREATE TABLE [Scheduler].[VolunteerProfile]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[resourceId] INT NOT NULL,		-- The Resource this volunteer profile belongs to (1:1)
	[volunteerStatusId] INT NOT NULL,		-- Current lifecycle status of this volunteer
	[onboardedDate] DATE NULL,		-- Date volunteer was approved/onboarded
	[inactiveSince] DATE NULL,		-- If inactive, when they went inactive
	[totalHoursServed] REAL NULL DEFAULT 0,		-- Cached/rolled-up lifetime volunteer hours
	[lastActivityDate] DATE NULL,		-- Most recent event/assignment end date
	[backgroundCheckCompleted] BIT NOT NULL DEFAULT 0,
	[backgroundCheckDate] DATE NULL,
	[backgroundCheckExpiry] DATE NULL,
	[confidentialityAgreementSigned] BIT NOT NULL DEFAULT 0,
	[confidentialityAgreementDate] DATE NULL,
	[availabilityPreferences] NVARCHAR(MAX) NULL,		-- Free text or structured JSON: e.g. 'prefers weekends', 'no evenings after 8pm'
	[interestsAndSkillsNotes] NVARCHAR(MAX) NULL,		-- Self-reported interests, hobbies, or extra skills
	[emergencyContactNotes] NVARCHAR(MAX) NULL,		-- Any special emergency instructions or notes
	[constituentId] INT NULL,		-- Optional link to fundraising/constituent record if relevant
	[iconId] INT NULL,		-- Optional override icon for volunteer-specific UI
	[color] NVARCHAR(10) NULL,		-- Optional override color
	[attributes] NVARCHAR(MAX) NULL,		-- Arbitrary JSON for future extension
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_VolunteerProfile_Resource_resourceId] FOREIGN KEY ([resourceId]) REFERENCES [Scheduler].[Resource] ([id]),		-- Foreign key to the Resource table.
	CONSTRAINT [FK_VolunteerProfile_VolunteerStatus_volunteerStatusId] FOREIGN KEY ([volunteerStatusId]) REFERENCES [Scheduler].[VolunteerStatus] ([id]),		-- Foreign key to the VolunteerStatus table.
	CONSTRAINT [FK_VolunteerProfile_Constituent_constituentId] FOREIGN KEY ([constituentId]) REFERENCES [Scheduler].[Constituent] ([id]),		-- Foreign key to the Constituent table.
	CONSTRAINT [FK_VolunteerProfile_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_VolunteerProfile_tenantGuid_resourceId] UNIQUE ( [tenantGuid], [resourceId]) 		-- Uniqueness enforced on the VolunteerProfile table's tenantGuid and resourceId fields.
)
GO

-- Index on the VolunteerProfile table's tenantGuid field.
CREATE INDEX [I_VolunteerProfile_tenantGuid] ON [Scheduler].[VolunteerProfile] ([tenantGuid])
GO

-- Index on the VolunteerProfile table's tenantGuid,resourceId fields.
CREATE INDEX [I_VolunteerProfile_tenantGuid_resourceId] ON [Scheduler].[VolunteerProfile] ([tenantGuid], [resourceId])
GO

-- Index on the VolunteerProfile table's tenantGuid,volunteerStatusId fields.
CREATE INDEX [I_VolunteerProfile_tenantGuid_volunteerStatusId] ON [Scheduler].[VolunteerProfile] ([tenantGuid], [volunteerStatusId])
GO

-- Index on the VolunteerProfile table's tenantGuid,constituentId fields.
CREATE INDEX [I_VolunteerProfile_tenantGuid_constituentId] ON [Scheduler].[VolunteerProfile] ([tenantGuid], [constituentId])
GO

-- Index on the VolunteerProfile table's tenantGuid,iconId fields.
CREATE INDEX [I_VolunteerProfile_tenantGuid_iconId] ON [Scheduler].[VolunteerProfile] ([tenantGuid], [iconId])
GO

-- Index on the VolunteerProfile table's tenantGuid,active fields.
CREATE INDEX [I_VolunteerProfile_tenantGuid_active] ON [Scheduler].[VolunteerProfile] ([tenantGuid], [active])
GO

-- Index on the VolunteerProfile table's tenantGuid,deleted fields.
CREATE INDEX [I_VolunteerProfile_tenantGuid_deleted] ON [Scheduler].[VolunteerProfile] ([tenantGuid], [deleted])
GO


-- The change history for records from the VolunteerProfile table.
CREATE TABLE [Scheduler].[VolunteerProfileChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[volunteerProfileId] INT NOT NULL,		-- Link to the VolunteerProfile table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_VolunteerProfileChangeHistory_VolunteerProfile_volunteerProfileId] FOREIGN KEY ([volunteerProfileId]) REFERENCES [Scheduler].[VolunteerProfile] ([id])		-- Foreign key to the VolunteerProfile table.
)
GO

-- Index on the VolunteerProfileChangeHistory table's tenantGuid field.
CREATE INDEX [I_VolunteerProfileChangeHistory_tenantGuid] ON [Scheduler].[VolunteerProfileChangeHistory] ([tenantGuid])
GO

-- Index on the VolunteerProfileChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_VolunteerProfileChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[VolunteerProfileChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the VolunteerProfileChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_VolunteerProfileChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[VolunteerProfileChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the VolunteerProfileChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_VolunteerProfileChangeHistory_tenantGuid_userId] ON [Scheduler].[VolunteerProfileChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the VolunteerProfileChangeHistory table's tenantGuid,volunteerProfileId fields.
CREATE INDEX [I_VolunteerProfileChangeHistory_tenantGuid_volunteerProfileId] ON [Scheduler].[VolunteerProfileChangeHistory] ([tenantGuid], [volunteerProfileId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


/*
Named, persistent groups of volunteers that are often scheduled together.
Examples: 'Saturday Soup Kitchen Team', 'Festival Setup Crew', 'Board of Directors Helpers'.
Similar to Crew table but volunteer-specific with lighter structure and volunteer-oriented metadata.
*/
CREATE TABLE [Scheduler].[VolunteerGroup]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[purpose] NVARCHAR(MAX) NULL,		-- What this group is mainly used for (e.g. 'Food distribution', 'Event setup & teardown')
	[officeId] INT NULL,		-- Optional office/branch this volunteer group is associated with
	[volunteerStatusId] INT NULL,		-- Minimum status required for members (e.g. Active only)
	[maxMembers] INT NULL,		-- Optional soft cap on group size
	[iconId] INT NULL,		-- Icon for UI display (e.g. group of people, soup bowl, hammer)
	[color] NVARCHAR(10) NULL,		-- Suggested color for calendar/events
	[notes] NVARCHAR(MAX) NULL,
	[avatarFileName] NVARCHAR(250) NULL,		-- Part of the binary data field setup
	[avatarSize] BIGINT NULL,		-- Part of the binary data field setup
	[avatarData] VARBINARY(MAX) NULL,		-- Part of the binary data field setup
	[avatarMimeType] NVARCHAR(100) NULL,		-- Part of the binary data field setup
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_VolunteerGroup_Office_officeId] FOREIGN KEY ([officeId]) REFERENCES [Scheduler].[Office] ([id]),		-- Foreign key to the Office table.
	CONSTRAINT [FK_VolunteerGroup_VolunteerStatus_volunteerStatusId] FOREIGN KEY ([volunteerStatusId]) REFERENCES [Scheduler].[VolunteerStatus] ([id]),		-- Foreign key to the VolunteerStatus table.
	CONSTRAINT [FK_VolunteerGroup_Icon_iconId] FOREIGN KEY ([iconId]) REFERENCES [Scheduler].[Icon] ([id]),		-- Foreign key to the Icon table.
	CONSTRAINT [UC_VolunteerGroup_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the VolunteerGroup table's tenantGuid and name fields.
)
GO

-- Index on the VolunteerGroup table's tenantGuid field.
CREATE INDEX [I_VolunteerGroup_tenantGuid] ON [Scheduler].[VolunteerGroup] ([tenantGuid])
GO

-- Index on the VolunteerGroup table's tenantGuid,name fields.
CREATE INDEX [I_VolunteerGroup_tenantGuid_name] ON [Scheduler].[VolunteerGroup] ([tenantGuid], [name])
GO

-- Index on the VolunteerGroup table's tenantGuid,officeId fields.
CREATE INDEX [I_VolunteerGroup_tenantGuid_officeId] ON [Scheduler].[VolunteerGroup] ([tenantGuid], [officeId])
GO

-- Index on the VolunteerGroup table's tenantGuid,volunteerStatusId fields.
CREATE INDEX [I_VolunteerGroup_tenantGuid_volunteerStatusId] ON [Scheduler].[VolunteerGroup] ([tenantGuid], [volunteerStatusId])
GO

-- Index on the VolunteerGroup table's tenantGuid,iconId fields.
CREATE INDEX [I_VolunteerGroup_tenantGuid_iconId] ON [Scheduler].[VolunteerGroup] ([tenantGuid], [iconId])
GO

-- Index on the VolunteerGroup table's tenantGuid,active fields.
CREATE INDEX [I_VolunteerGroup_tenantGuid_active] ON [Scheduler].[VolunteerGroup] ([tenantGuid], [active])
GO

-- Index on the VolunteerGroup table's tenantGuid,deleted fields.
CREATE INDEX [I_VolunteerGroup_tenantGuid_deleted] ON [Scheduler].[VolunteerGroup] ([tenantGuid], [deleted])
GO


-- The change history for records from the VolunteerGroup table.
CREATE TABLE [Scheduler].[VolunteerGroupChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[volunteerGroupId] INT NOT NULL,		-- Link to the VolunteerGroup table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_VolunteerGroupChangeHistory_VolunteerGroup_volunteerGroupId] FOREIGN KEY ([volunteerGroupId]) REFERENCES [Scheduler].[VolunteerGroup] ([id])		-- Foreign key to the VolunteerGroup table.
)
GO

-- Index on the VolunteerGroupChangeHistory table's tenantGuid field.
CREATE INDEX [I_VolunteerGroupChangeHistory_tenantGuid] ON [Scheduler].[VolunteerGroupChangeHistory] ([tenantGuid])
GO

-- Index on the VolunteerGroupChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_VolunteerGroupChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[VolunteerGroupChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the VolunteerGroupChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_VolunteerGroupChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[VolunteerGroupChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the VolunteerGroupChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_VolunteerGroupChangeHistory_tenantGuid_userId] ON [Scheduler].[VolunteerGroupChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the VolunteerGroupChangeHistory table's tenantGuid,volunteerGroupId fields.
CREATE INDEX [I_VolunteerGroupChangeHistory_tenantGuid_volunteerGroupId] ON [Scheduler].[VolunteerGroupChangeHistory] ([tenantGuid], [volunteerGroupId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


/*
Membership in a VolunteerGroup.
Links Resources (volunteers) to groups, with optional default role and sequence.
*/
CREATE TABLE [Scheduler].[VolunteerGroupMember]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[volunteerGroupId] INT NOT NULL,		-- Link to the VolunteerGroup table.
	[resourceId] INT NOT NULL,		-- The volunteer (Resource) in this group
	[assignmentRoleId] INT NULL,		-- Default role this person plays in the group (e.g. 'Team Lead', 'Driver')
	[sequence] INT NOT NULL DEFAULT 1,		-- Display/order position within the group
	[joinedDate] DATE NULL,
	[leftDate] DATE NULL,		-- If they left the group
	[notes] NVARCHAR(MAX) NULL,		-- e.g. 'Prefers kitchen duties', 'Only available 1st Saturday'
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_VolunteerGroupMember_VolunteerGroup_volunteerGroupId] FOREIGN KEY ([volunteerGroupId]) REFERENCES [Scheduler].[VolunteerGroup] ([id]),		-- Foreign key to the VolunteerGroup table.
	CONSTRAINT [FK_VolunteerGroupMember_Resource_resourceId] FOREIGN KEY ([resourceId]) REFERENCES [Scheduler].[Resource] ([id]),		-- Foreign key to the Resource table.
	CONSTRAINT [FK_VolunteerGroupMember_AssignmentRole_assignmentRoleId] FOREIGN KEY ([assignmentRoleId]) REFERENCES [Scheduler].[AssignmentRole] ([id]),		-- Foreign key to the AssignmentRole table.
	CONSTRAINT [UC_VolunteerGroupMember_tenantGuid_volunteerGroupId_resourceId] UNIQUE ( [tenantGuid], [volunteerGroupId], [resourceId]) 		-- Uniqueness enforced on the VolunteerGroupMember table's tenantGuid and volunteerGroupId and resourceId fields.
)
GO

-- Index on the VolunteerGroupMember table's tenantGuid field.
CREATE INDEX [I_VolunteerGroupMember_tenantGuid] ON [Scheduler].[VolunteerGroupMember] ([tenantGuid])
GO

-- Index on the VolunteerGroupMember table's tenantGuid,volunteerGroupId fields.
CREATE INDEX [I_VolunteerGroupMember_tenantGuid_volunteerGroupId] ON [Scheduler].[VolunteerGroupMember] ([tenantGuid], [volunteerGroupId])
GO

-- Index on the VolunteerGroupMember table's tenantGuid,resourceId fields.
CREATE INDEX [I_VolunteerGroupMember_tenantGuid_resourceId] ON [Scheduler].[VolunteerGroupMember] ([tenantGuid], [resourceId])
GO

-- Index on the VolunteerGroupMember table's tenantGuid,assignmentRoleId fields.
CREATE INDEX [I_VolunteerGroupMember_tenantGuid_assignmentRoleId] ON [Scheduler].[VolunteerGroupMember] ([tenantGuid], [assignmentRoleId])
GO

-- Index on the VolunteerGroupMember table's tenantGuid,active fields.
CREATE INDEX [I_VolunteerGroupMember_tenantGuid_active] ON [Scheduler].[VolunteerGroupMember] ([tenantGuid], [active])
GO

-- Index on the VolunteerGroupMember table's tenantGuid,deleted fields.
CREATE INDEX [I_VolunteerGroupMember_tenantGuid_deleted] ON [Scheduler].[VolunteerGroupMember] ([tenantGuid], [deleted])
GO


-- The change history for records from the VolunteerGroupMember table.
CREATE TABLE [Scheduler].[VolunteerGroupMemberChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[volunteerGroupMemberId] INT NOT NULL,		-- Link to the VolunteerGroupMember table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_VolunteerGroupMemberChangeHistory_VolunteerGroupMember_volunteerGroupMemberId] FOREIGN KEY ([volunteerGroupMemberId]) REFERENCES [Scheduler].[VolunteerGroupMember] ([id])		-- Foreign key to the VolunteerGroupMember table.
)
GO

-- Index on the VolunteerGroupMemberChangeHistory table's tenantGuid field.
CREATE INDEX [I_VolunteerGroupMemberChangeHistory_tenantGuid] ON [Scheduler].[VolunteerGroupMemberChangeHistory] ([tenantGuid])
GO

-- Index on the VolunteerGroupMemberChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_VolunteerGroupMemberChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[VolunteerGroupMemberChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the VolunteerGroupMemberChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_VolunteerGroupMemberChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[VolunteerGroupMemberChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the VolunteerGroupMemberChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_VolunteerGroupMemberChangeHistory_tenantGuid_userId] ON [Scheduler].[VolunteerGroupMemberChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the VolunteerGroupMemberChangeHistory table's tenantGuid,volunteerGroupMemberId fields.
CREATE INDEX [I_VolunteerGroupMemberChangeHistory_tenantGuid_volunteerGroupMemberId] ON [Scheduler].[VolunteerGroupMemberChangeHistory] ([tenantGuid], [volunteerGroupMemberId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Links resources, crews, or volunteer groups o events.  Supports partial assignments and role designation.  - If crewId is non-NULL → this row represents assignment of the whole crew - If resourceId is non-NULL and crewId is NULL → individual resource assignment - assignmentStart/End NULL → uses full event duration.  only one of crewId, volunteerGroupId, resourceId should be populated per row (business rule in app layer).
CREATE TABLE [Scheduler].[EventResourceAssignment]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduledEventId] INT NOT NULL,		-- Link to the ScheduledEvent table.
	[officeId] INT NULL,		-- Snapshot of office resource assigned to this event belongs to at the time of assignment.  This should never change, and should NOT be updated if a resource moves to a different office post-event assignment.
	[resourceId] INT NULL,		-- Required for individual assignments; should be NULL when crewId is used
	[crewId] INT NULL,		-- Optional – when set, assigns the entire crew as a unit
	[volunteerGroupId] INT NULL,		-- Optional: assign an entire VolunteerGroup instead of/in addition to individual resources or Crew
	[assignmentRoleId] INT NULL,		-- Optional role for this assignment (individual or crew member default)
	[assignmentStatusId] INT NOT NULL DEFAULT 1,		-- NULL = Planned, non-NULL links to AssignmentStatus master table
	[assignmentStartDateTime] DATETIME2(7) NULL,		-- NULL = starts at event start
	[assignmentEndDateTime] DATETIME2(7) NULL,		-- NULL = ends at event end
	[notes] NVARCHAR(MAX) NULL,
	[isTravelRequired] BIT NULL,		-- Whether or not travel is required for the assignment
	[travelDurationMinutes] INT NULL DEFAULT 0,		-- Time required to get to the site
	[distanceKilometers] REAL NULL DEFAULT 0,		-- Useful for expense calculation
	[startLocation] NVARCHAR(100) NULL,
	[actualStartDateTime] DATETIME2(7) NULL,
	[actualEndDateTime] DATETIME2(7) NULL,
	[actualNotes] NVARCHAR(MAX) NULL,
	[isVolunteer] BIT NOT NULL DEFAULT 0,/*
True = this is a volunteer (unpaid) assignment.
Used to:
- Exclude from payroll/wage calculations
- Include in volunteer hours totals
- Apply different approval/reminder workflows
- Filter volunteer-specific reports
*/
	[reportedVolunteerHours] REAL NULL,		-- Hours the volunteer self-reported (or coordinator entered) for this assignment
	[approvedVolunteerHours] REAL NULL,		-- Approved/confirmed hours (may differ from reported if adjustments needed)
	[hoursApprovedByContactId] INT NULL,		-- Contact (usually staff/coordinator) who approved the hours
	[approvedDateTime] DATETIME2(7) NULL,		-- When the hours were approved
	[reimbursementAmount] MONEY NULL,		-- Optional: mileage, parking, meals, etc. — not a wage
	[chargeTypeId] INT NULL,		-- Optional: links to an expense-type ChargeType for the reimbursement (e.g. 'Mileage Reimbursement')
	[reimbursementRequested] BIT NOT NULL DEFAULT 0,		-- Volunteer has flagged that they want/need reimbursement
	[volunteerNotes] NVARCHAR(MAX) NULL,		-- Volunteer-specific notes for this assignment (e.g. 'Prefers morning shifts next time', 'Brought own tools')
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_EventResourceAssignment_ScheduledEvent_scheduledEventId] FOREIGN KEY ([scheduledEventId]) REFERENCES [Scheduler].[ScheduledEvent] ([id]),		-- Foreign key to the ScheduledEvent table.
	CONSTRAINT [FK_EventResourceAssignment_Office_officeId] FOREIGN KEY ([officeId]) REFERENCES [Scheduler].[Office] ([id]),		-- Foreign key to the Office table.
	CONSTRAINT [FK_EventResourceAssignment_Resource_resourceId] FOREIGN KEY ([resourceId]) REFERENCES [Scheduler].[Resource] ([id]),		-- Foreign key to the Resource table.
	CONSTRAINT [FK_EventResourceAssignment_Crew_crewId] FOREIGN KEY ([crewId]) REFERENCES [Scheduler].[Crew] ([id]),		-- Foreign key to the Crew table.
	CONSTRAINT [FK_EventResourceAssignment_VolunteerGroup_volunteerGroupId] FOREIGN KEY ([volunteerGroupId]) REFERENCES [Scheduler].[VolunteerGroup] ([id]),		-- Foreign key to the VolunteerGroup table.
	CONSTRAINT [FK_EventResourceAssignment_AssignmentRole_assignmentRoleId] FOREIGN KEY ([assignmentRoleId]) REFERENCES [Scheduler].[AssignmentRole] ([id]),		-- Foreign key to the AssignmentRole table.
	CONSTRAINT [FK_EventResourceAssignment_AssignmentStatus_assignmentStatusId] FOREIGN KEY ([assignmentStatusId]) REFERENCES [Scheduler].[AssignmentStatus] ([id]),		-- Foreign key to the AssignmentStatus table.
	CONSTRAINT [FK_EventResourceAssignment_Contact_hoursApprovedByContactId] FOREIGN KEY ([hoursApprovedByContactId]) REFERENCES [Scheduler].[Contact] ([id]),		-- Foreign key to the Contact table.
	CONSTRAINT [FK_EventResourceAssignment_ChargeType_chargeTypeId] FOREIGN KEY ([chargeTypeId]) REFERENCES [Scheduler].[ChargeType] ([id])		-- Foreign key to the ChargeType table.
)
GO

-- Index on the EventResourceAssignment table's tenantGuid field.
CREATE INDEX [I_EventResourceAssignment_tenantGuid] ON [Scheduler].[EventResourceAssignment] ([tenantGuid])
GO

-- Index on the EventResourceAssignment table's tenantGuid,scheduledEventId fields.
CREATE INDEX [I_EventResourceAssignment_tenantGuid_scheduledEventId] ON [Scheduler].[EventResourceAssignment] ([tenantGuid], [scheduledEventId])
GO

-- Index on the EventResourceAssignment table's tenantGuid,officeId fields.
CREATE INDEX [I_EventResourceAssignment_tenantGuid_officeId] ON [Scheduler].[EventResourceAssignment] ([tenantGuid], [officeId])
GO

-- Index on the EventResourceAssignment table's tenantGuid,resourceId fields.
CREATE INDEX [I_EventResourceAssignment_tenantGuid_resourceId] ON [Scheduler].[EventResourceAssignment] ([tenantGuid], [resourceId])
GO

-- Index on the EventResourceAssignment table's tenantGuid,crewId fields.
CREATE INDEX [I_EventResourceAssignment_tenantGuid_crewId] ON [Scheduler].[EventResourceAssignment] ([tenantGuid], [crewId])
GO

-- Index on the EventResourceAssignment table's tenantGuid,volunteerGroupId fields.
CREATE INDEX [I_EventResourceAssignment_tenantGuid_volunteerGroupId] ON [Scheduler].[EventResourceAssignment] ([tenantGuid], [volunteerGroupId])
GO

-- Index on the EventResourceAssignment table's tenantGuid,assignmentRoleId fields.
CREATE INDEX [I_EventResourceAssignment_tenantGuid_assignmentRoleId] ON [Scheduler].[EventResourceAssignment] ([tenantGuid], [assignmentRoleId])
GO

-- Index on the EventResourceAssignment table's tenantGuid,assignmentStatusId fields.
CREATE INDEX [I_EventResourceAssignment_tenantGuid_assignmentStatusId] ON [Scheduler].[EventResourceAssignment] ([tenantGuid], [assignmentStatusId])
GO

-- Index on the EventResourceAssignment table's tenantGuid,hoursApprovedByContactId fields.
CREATE INDEX [I_EventResourceAssignment_tenantGuid_hoursApprovedByContactId] ON [Scheduler].[EventResourceAssignment] ([tenantGuid], [hoursApprovedByContactId])
GO

-- Index on the EventResourceAssignment table's tenantGuid,chargeTypeId fields.
CREATE INDEX [I_EventResourceAssignment_tenantGuid_chargeTypeId] ON [Scheduler].[EventResourceAssignment] ([tenantGuid], [chargeTypeId])
GO

-- Index on the EventResourceAssignment table's tenantGuid,active fields.
CREATE INDEX [I_EventResourceAssignment_tenantGuid_active] ON [Scheduler].[EventResourceAssignment] ([tenantGuid], [active])
GO

-- Index on the EventResourceAssignment table's tenantGuid,deleted fields.
CREATE INDEX [I_EventResourceAssignment_tenantGuid_deleted] ON [Scheduler].[EventResourceAssignment] ([tenantGuid], [deleted])
GO

-- Index on the EventResourceAssignment table's tenantGuid,resourceId,assignmentStartDateTime,assignmentEndDateTime fields.
CREATE INDEX [I_EventResourceAssignment_tenantGuid_resourceId_assignmentStartDateTime_assignmentEndDateTime] ON [Scheduler].[EventResourceAssignment] ([tenantGuid], [resourceId], [assignmentStartDateTime], [assignmentEndDateTime])
GO

-- Index on the EventResourceAssignment table's tenantGuid,crewId,assignmentStartDateTime,assignmentEndDateTime fields.
CREATE INDEX [I_EventResourceAssignment_tenantGuid_crewId_assignmentStartDateTime_assignmentEndDateTime] ON [Scheduler].[EventResourceAssignment] ([tenantGuid], [crewId], [assignmentStartDateTime], [assignmentEndDateTime])
GO


-- The change history for records from the EventResourceAssignment table.
CREATE TABLE [Scheduler].[EventResourceAssignmentChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[eventResourceAssignmentId] INT NOT NULL,		-- Link to the EventResourceAssignment table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_EventResourceAssignmentChangeHistory_EventResourceAssignment_eventResourceAssignmentId] FOREIGN KEY ([eventResourceAssignmentId]) REFERENCES [Scheduler].[EventResourceAssignment] ([id])		-- Foreign key to the EventResourceAssignment table.
)
GO

-- Index on the EventResourceAssignmentChangeHistory table's tenantGuid field.
CREATE INDEX [I_EventResourceAssignmentChangeHistory_tenantGuid] ON [Scheduler].[EventResourceAssignmentChangeHistory] ([tenantGuid])
GO

-- Index on the EventResourceAssignmentChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_EventResourceAssignmentChangeHistory_tenantGuid_versionNumber] ON [Scheduler].[EventResourceAssignmentChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the EventResourceAssignmentChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_EventResourceAssignmentChangeHistory_tenantGuid_timeStamp] ON [Scheduler].[EventResourceAssignmentChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the EventResourceAssignmentChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_EventResourceAssignmentChangeHistory_tenantGuid_userId] ON [Scheduler].[EventResourceAssignmentChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the EventResourceAssignmentChangeHistory table's tenantGuid,eventResourceAssignmentId fields.
CREATE INDEX [I_EventResourceAssignmentChangeHistory_tenantGuid_eventResourceAssignmentId] ON [Scheduler].[EventResourceAssignmentChangeHistory] ([tenantGuid], [eventResourceAssignmentId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


