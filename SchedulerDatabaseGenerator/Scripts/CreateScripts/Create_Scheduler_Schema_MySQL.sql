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
-- DROP TABLE `HouseholdChangeHistory`
-- DROP TABLE `Household`
-- DROP TABLE `AppealChangeHistory`
-- DROP TABLE `Appeal`
-- DROP TABLE `CampaignChangeHistory`
-- DROP TABLE `Campaign`
-- DROP TABLE `FundChangeHistory`
-- DROP TABLE `Fund`
-- DROP TABLE `NotificationSubscriptionChangeHistory`
-- DROP TABLE `NotificationSubscription`
-- DROP TABLE `NotificationType`
-- DROP TABLE `EventResourceAssignmentChangeHistory`
-- DROP TABLE `EventResourceAssignment`
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
-- DROP TABLE `EventChargeChangeHistory`
-- DROP TABLE `EventCharge`
-- DROP TABLE `ChargeStatus`
-- DROP TABLE `ScheduledEventChangeHistory`
-- DROP TABLE `ScheduledEvent`
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
-- DROP TABLE `ReceiptType`
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
-- DROP TABLE `StateProvince`
-- DROP TABLE `Country`
-- DROP TABLE `TimeZone`
-- DROP TABLE `Tag`
-- DROP TABLE `ChargeTypeChangeHistory`
-- DROP TABLE `ChargeType`
-- DROP TABLE `Currency`
-- DROP TABLE `InteractionType`
-- DROP TABLE `RateType`
-- DROP TABLE `ContactMethod`
-- DROP TABLE `Priority`
-- DROP TABLE `ResourceType`
-- DROP TABLE `Salutation`
-- DROP TABLE `Icon`

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
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
-- ALTER INDEX ALL ON `HouseholdChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Household` DISABLE
-- ALTER INDEX ALL ON `AppealChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Appeal` DISABLE
-- ALTER INDEX ALL ON `CampaignChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Campaign` DISABLE
-- ALTER INDEX ALL ON `FundChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Fund` DISABLE
-- ALTER INDEX ALL ON `NotificationSubscriptionChangeHistory` DISABLE
-- ALTER INDEX ALL ON `NotificationSubscription` DISABLE
-- ALTER INDEX ALL ON `NotificationType` DISABLE
-- ALTER INDEX ALL ON `EventResourceAssignmentChangeHistory` DISABLE
-- ALTER INDEX ALL ON `EventResourceAssignment` DISABLE
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
-- ALTER INDEX ALL ON `EventChargeChangeHistory` DISABLE
-- ALTER INDEX ALL ON `EventCharge` DISABLE
-- ALTER INDEX ALL ON `ChargeStatus` DISABLE
-- ALTER INDEX ALL ON `ScheduledEventChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ScheduledEvent` DISABLE
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
-- ALTER INDEX ALL ON `ReceiptType` DISABLE
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
-- ALTER INDEX ALL ON `StateProvince` DISABLE
-- ALTER INDEX ALL ON `Country` DISABLE
-- ALTER INDEX ALL ON `TimeZone` DISABLE
-- ALTER INDEX ALL ON `Tag` DISABLE
-- ALTER INDEX ALL ON `ChargeTypeChangeHistory` DISABLE
-- ALTER INDEX ALL ON `ChargeType` DISABLE
-- ALTER INDEX ALL ON `Currency` DISABLE
-- ALTER INDEX ALL ON `InteractionType` DISABLE
-- ALTER INDEX ALL ON `RateType` DISABLE
-- ALTER INDEX ALL ON `ContactMethod` DISABLE
-- ALTER INDEX ALL ON `Priority` DISABLE
-- ALTER INDEX ALL ON `ResourceType` DISABLE
-- ALTER INDEX ALL ON `Salutation` DISABLE
-- ALTER INDEX ALL ON `Icon` DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
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
-- ALTER INDEX ALL ON `HouseholdChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Household` REBUILD
-- ALTER INDEX ALL ON `AppealChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Appeal` REBUILD
-- ALTER INDEX ALL ON `CampaignChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Campaign` REBUILD
-- ALTER INDEX ALL ON `FundChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Fund` REBUILD
-- ALTER INDEX ALL ON `NotificationSubscriptionChangeHistory` REBUILD
-- ALTER INDEX ALL ON `NotificationSubscription` REBUILD
-- ALTER INDEX ALL ON `NotificationType` REBUILD
-- ALTER INDEX ALL ON `EventResourceAssignmentChangeHistory` REBUILD
-- ALTER INDEX ALL ON `EventResourceAssignment` REBUILD
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
-- ALTER INDEX ALL ON `EventChargeChangeHistory` REBUILD
-- ALTER INDEX ALL ON `EventCharge` REBUILD
-- ALTER INDEX ALL ON `ChargeStatus` REBUILD
-- ALTER INDEX ALL ON `ScheduledEventChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ScheduledEvent` REBUILD
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
-- ALTER INDEX ALL ON `ReceiptType` REBUILD
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
-- ALTER INDEX ALL ON `StateProvince` REBUILD
-- ALTER INDEX ALL ON `Country` REBUILD
-- ALTER INDEX ALL ON `TimeZone` REBUILD
-- ALTER INDEX ALL ON `Tag` REBUILD
-- ALTER INDEX ALL ON `ChargeTypeChangeHistory` REBUILD
-- ALTER INDEX ALL ON `ChargeType` REBUILD
-- ALTER INDEX ALL ON `Currency` REBUILD
-- ALTER INDEX ALL ON `InteractionType` REBUILD
-- ALTER INDEX ALL ON `RateType` REBUILD
-- ALTER INDEX ALL ON `ContactMethod` REBUILD
-- ALTER INDEX ALL ON `Priority` REBUILD
-- ALTER INDEX ALL ON `ResourceType` REBUILD
-- ALTER INDEX ALL ON `Salutation` REBUILD
-- ALTER INDEX ALL ON `Icon` REBUILD

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

INSERT INTO `Currency` ( `tenantGuid`, `name`, `description`, `code`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'US Dollas', 'United States Dollars', 'USD', 1, '5d460ce9-4cf5-41c3-ab9d-9ef104b0a276' );

INSERT INTO `Currency` ( `tenantGuid`, `name`, `description`, `code`, `sequence`, `objectGuid` ) VALUES  ( '00000000-0000-0000-0000-000000000000', 'Canadian', 'Canadian Dollars', 'CAD', 2, 'c6673662-f1c9-4aee-b5df-867500cb8545' );


/*
====================================================================================================
 CHARGE MASTER (Like Epic CDM)
 Master list of chargeable items (revenue or expenses). e.g., "Site Visit Fee" (revenue), "Travel Expense" (expense).
 Tied to RateType for billing context.
 ====================================================================================================
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
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`rateTypeId`) REFERENCES `RateType`(`id`),		-- Foreign key to the RateType table.
	FOREIGN KEY (`currencyId`) REFERENCES `Currency`(`id`),		-- Foreign key to the Currency table.
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


-- Tenant specific master list of tags.
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


-- Master list of office types.  Used for categorizing offices.  Not tenant specific
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


-- Master list of office types.  Used for categorizing offices.  Not tenant specific
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


-- Master list of office types.  Used for categorizing offices.  Not tenant specific
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
	`externalId` VARCHAR(100) NULL,		-- Optional reference to an ID in an external system (e.g., Basecamp Project ID)
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
	`externalId` VARCHAR(100) NULL,		-- Optional reference to an ID in an external system (e.g., Basecamp Project ID)
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


-- Master list of payment types ( credit card, check, cash, etc..)
CREATE TABLE `PaymentType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the PaymentType table's name field.
CREATE INDEX `I_PaymentType_name` ON `PaymentType` (`name`);

-- Index on the PaymentType table's active field.
CREATE INDEX `I_PaymentType_active` ON `PaymentType` (`active`);

-- Index on the PaymentType table's deleted field.
CREATE INDEX `I_PaymentType_deleted` ON `PaymentType` (`deleted`);

INSERT INTO `PaymentType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Credit Card', 'Credit Card', 1, '3353a9f0-1b8e-4170-a20a-d35eab81fab8' );

INSERT INTO `PaymentType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Check', 'Check', 2, '19376f2d-87c0-4eb5-a11c-f02cb4f9b412' );

INSERT INTO `PaymentType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Cash', 'Cash', 3, 'dca9c876-bb7d-4c33-8ef4-96a955dacbb0' );

INSERT INTO `PaymentType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Crypto', 'Crypto', 4, '8be012ff-f305-45cd-bedb-cc5b9f11f3ef' );

INSERT INTO `PaymentType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Stock', 'Stock', 5, '427451dc-b522-4613-aa3a-57593b6d4d03' );


-- Master list of receipt types
CREATE TABLE `ReceiptType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the ReceiptType table's name field.
CREATE INDEX `I_ReceiptType_name` ON `ReceiptType` (`name`);

-- Index on the ReceiptType table's active field.
CREATE INDEX `I_ReceiptType_active` ON `ReceiptType` (`active`);

-- Index on the ReceiptType table's deleted field.
CREATE INDEX `I_ReceiptType_deleted` ON `ReceiptType` (`deleted`);

INSERT INTO `ReceiptType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Receipted', 'Receipted', 1, 'b0a794eb-afa9-4791-b164-e28e5ed21a35' );

INSERT INTO `ReceiptType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Do Not Receipt', 'Do Not Receipt', 2, 'd6ceb144-aced-4e2a-9407-a2b0c995c795' );


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
	`externalId` VARCHAR(100) NULL,		-- Optional reference to an ID in an external system (e.g., Basecamp Project ID)
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
	`externalId` VARCHAR(100) NULL,		-- Optional reference to an ID in an external system (e.g., Equipment.id from Basecamp)
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
CREATE TABLE `ScheduledEvent`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`officeId` INT NULL,		-- Snapshot of office that the first resource assigned to this event belongs to.  This should NOT be updated if a resource moves to a different office post event assignment.  It should only change if there was an original entry error that needs to be corrected.
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
	`partySize` INT NULL,		-- Optional for use when running as a reservation system
	`notes` TEXT NULL,
	`color` VARCHAR(10) NULL,		-- Override Hex color for UI display
	`externalId` VARCHAR(100) NULL,		-- Optional link to an entity in another system
	`attributes` TEXT NULL,		-- to store arbitrary JSON
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
	UNIQUE `UC_ScheduledEvent_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the ScheduledEvent table's tenantGuid and name fields.
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


-- Master list of charge statuses (Pending, Approved, Invoiced, Void)
CREATE TABLE `ChargeStatus`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
	`sequence` INT NULL,		-- Sequence to use for sorting.
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
	`extendedAmount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- Always the final calculated/total amount (quantity × unitPrice, or just amount) Does not include taxes.
	`taxAmount` DECIMAL(11,2) NOT NULL DEFAULT 0,		-- The calculated tax based on isTaxable
	`currencyId` INT NOT NULL,		-- Link to Currency table.
	`rateTypeId` INT NULL,		-- Optional link to RateType (e.g., 'Overtime').
	`notes` TEXT NULL,		-- Optional notes about the charge
	`isAutomatic` BIT NOT NULL DEFAULT 1,		-- 1 = auto-dropped from event type, 0 = manual add/edit.
	`exportedDate` DATETIME NULL,		-- When this charge was last exported (null = not exported yet).
	`externalId` VARCHAR(100) NULL,		-- Identifier from extenral system - possibly invoice number or some other billing grouper
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`scheduledEventId`) REFERENCES `ScheduledEvent`(`id`),		-- Foreign key to the ScheduledEvent table.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`),		-- Foreign key to the Resource table.
	FOREIGN KEY (`chargeTypeId`) REFERENCES `ChargeType`(`id`),		-- Foreign key to the ChargeType table.
	FOREIGN KEY (`chargeStatusId`) REFERENCES `ChargeStatus`(`id`),		-- Foreign key to the ChargeStatus table.
	FOREIGN KEY (`currencyId`) REFERENCES `Currency`(`id`),		-- Foreign key to the Currency table.
	FOREIGN KEY (`rateTypeId`) REFERENCES `RateType`(`id`)		-- Foreign key to the RateType table.
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


-- Links resources (or entire crews) to events.  Supports partial assignments and role designation.  - If crewId is non-NULL → this row represents assignment of the whole crew - If resourceId is non-NULL and crewId is NULL → individual resource assignment - assignmentStart/End NULL → uses full event duration
CREATE TABLE `EventResourceAssignment`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`scheduledEventId` INT NOT NULL,		-- Link to the ScheduledEvent table.
	`officeId` INT NULL,		-- Snapshot of office resource assigned to this event belongs to at the time of assignment.  This should never change, and should NOT be updated if a resource moves to a different office post event assignment.
	`resourceId` INT NULL,		-- Required for individual assignments; should be NULL when crewId is used
	`crewId` INT NULL,		-- Optional – when set, assigns the entire crew as a unit
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
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`scheduledEventId`) REFERENCES `ScheduledEvent`(`id`),		-- Foreign key to the ScheduledEvent table.
	FOREIGN KEY (`officeId`) REFERENCES `Office`(`id`),		-- Foreign key to the Office table.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`),		-- Foreign key to the Resource table.
	FOREIGN KEY (`crewId`) REFERENCES `Crew`(`id`),		-- Foreign key to the Crew table.
	FOREIGN KEY (`assignmentRoleId`) REFERENCES `AssignmentRole`(`id`),		-- Foreign key to the AssignmentRole table.
	FOREIGN KEY (`assignmentStatusId`) REFERENCES `AssignmentStatus`(`id`)		-- Foreign key to the AssignmentStatus table.
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

-- Index on the EventResourceAssignment table's tenantGuid,assignmentRoleId fields.
CREATE INDEX `I_EventResourceAssignment_tenantGuid_assignmentRoleId` ON `EventResourceAssignment` (`tenantGuid`, `assignmentRoleId`);

-- Index on the EventResourceAssignment table's tenantGuid,assignmentStatusId fields.
CREATE INDEX `I_EventResourceAssignment_tenantGuid_assignmentStatusId` ON `EventResourceAssignment` (`tenantGuid`, `assignmentStatusId`);

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


-- Master list of notification types
CREATE TABLE `NotificationType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`color` VARCHAR(10) NULL,		-- Hex color for UI display
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

INSERT INTO `NotificationType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Email', 'Send to email address', 1, '73ff7b17-3fd7-40ce-91bf-c91daca7b4ce' );

INSERT INTO `NotificationType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'SMS', 'Sent to cell phone via SMS message', 2, '89391299-4427-43f6-bcf2-0266e47e83a7' );

INSERT INTO `NotificationType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Push', 'Sent to cell phone via Push notification', 3, '0395ddde-58dc-4577-9dae-07614680c386' );


-- Links resources (or entire crews) to events.  Supports partial assignments and role designation.  - If crewId is non-NULL → this row represents assignment of the whole crew - If resourceId is non-NULL and crewId is NULL → individual resource assignment - assignmentStart/End NULL → uses full event duration
CREATE TABLE `NotificationSubscription`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`resourceId` INT NULL,		-- Optional resource for this notification subscription.  Needs either this or contact to be valid.
	`contactId` INT NULL,		-- Optional contact for this notification subscription.  Needs either this or resource to be valid.
	`notificationTypeId` INT NOT NULL,		-- Link to the NotificationType table.
	`triggerEvents` INT NOT NULL DEFAULT 1,		-- Bitmask: 1=Assigned, 2=Canceled, 4=Modified, 8=Reminder
	`recipientAddress` VARCHAR(250) NOT NULL,		-- Email address or Phone #
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`resourceId`) REFERENCES `Resource`(`id`),		-- Foreign key to the Resource table.
	FOREIGN KEY (`contactId`) REFERENCES `Contact`(`id`),		-- Foreign key to the Contact table.
	FOREIGN KEY (`notificationTypeId`) REFERENCES `NotificationType`(`id`)		-- Foreign key to the NotificationType table.
);
-- Index on the NotificationSubscription table's tenantGuid field.
CREATE INDEX `I_NotificationSubscription_tenantGuid` ON `NotificationSubscription` (`tenantGuid`);

-- Index on the NotificationSubscription table's tenantGuid,resourceId fields.
CREATE INDEX `I_NotificationSubscription_tenantGuid_resourceId` ON `NotificationSubscription` (`tenantGuid`, `resourceId`);

-- Index on the NotificationSubscription table's tenantGuid,contactId fields.
CREATE INDEX `I_NotificationSubscription_tenantGuid_contactId` ON `NotificationSubscription` (`tenantGuid`, `contactId`);

-- Index on the NotificationSubscription table's tenantGuid,notificationTypeId fields.
CREATE INDEX `I_NotificationSubscription_tenantGuid_notificationTypeId` ON `NotificationSubscription` (`tenantGuid`, `notificationTypeId`);

-- Index on the NotificationSubscription table's tenantGuid,active fields.
CREATE INDEX `I_NotificationSubscription_tenantGuid_active` ON `NotificationSubscription` (`tenantGuid`, `active`);

-- Index on the NotificationSubscription table's tenantGuid,deleted fields.
CREATE INDEX `I_NotificationSubscription_tenantGuid_deleted` ON `NotificationSubscription` (`tenantGuid`, `deleted`);


-- The change history for records from the NotificationSubscription table.
CREATE TABLE `NotificationSubscriptionChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`notificationSubscriptionId` INT NOT NULL,		-- Link to the NotificationSubscription table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`notificationSubscriptionId`) REFERENCES `NotificationSubscription`(`id`)		-- Foreign key to the NotificationSubscription table.
);
-- Index on the NotificationSubscriptionChangeHistory table's tenantGuid field.
CREATE INDEX `I_NotificationSubscriptionChangeHistory_tenantGuid` ON `NotificationSubscriptionChangeHistory` (`tenantGuid`);

-- Index on the NotificationSubscriptionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_NotificationSubscriptionChangeHistory_tenantGuid_versionNumber` ON `NotificationSubscriptionChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the NotificationSubscriptionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_NotificationSubscriptionChangeHistory_tenantGuid_timeStamp` ON `NotificationSubscriptionChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the NotificationSubscriptionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_NotificationSubscriptionChangeHistory_tenantGuid_userId` ON `NotificationSubscriptionChangeHistory` (`tenantGuid`, `userId`);

-- Index on the NotificationSubscriptionChangeHistory table's tenantGuid,notificationSubscriptionId fields.
CREATE INDEX `I_NotificationSubscriptionChangeHistory_tenantGuid_notificationS` ON `NotificationSubscriptionChangeHistory` (`tenantGuid`, `notificationSubscriptionId`, `versionNumber`, `timeStamp`, `userId`);


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


