/*
Alerting and Incident Management database.
Stores services, escalation policies, on-call schedules, incidents, timeline events,
notification delivery tracking, webhook delivery tracking, and integration credentials.
Designed to be independent while sharing the central Security database for users and teams.
*/
/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE "WebhookDeliveryAttempt"
-- DROP TABLE "NotificationDeliveryAttempt"
-- DROP TABLE "IncidentNotification"
-- DROP TABLE "IncidentNoteChangeHistory"
-- DROP TABLE "IncidentNote"
-- DROP TABLE "IncidentTimelineEvent"
-- DROP TABLE "IncidentChangeHistory"
-- DROP TABLE "Incident"
-- DROP TABLE "UserPushTokenChangeHistory"
-- DROP TABLE "UserPushToken"
-- DROP TABLE "UserNotificationChannelPreferenceChangeHistory"
-- DROP TABLE "UserNotificationChannelPreference"
-- DROP TABLE "UserNotificationPreferenceChangeHistory"
-- DROP TABLE "UserNotificationPreference"
-- DROP TABLE "IntegrationCallbackIncidentEventTypeChangeHistory"
-- DROP TABLE "IntegrationCallbackIncidentEventType"
-- DROP TABLE "IntegrationChangeHistory"
-- DROP TABLE "Integration"
-- DROP TABLE "ScheduleOverrideChangeHistory"
-- DROP TABLE "ScheduleOverride"
-- DROP TABLE "ScheduleLayerMemberChangeHistory"
-- DROP TABLE "ScheduleLayerMember"
-- DROP TABLE "ScheduleLayerChangeHistory"
-- DROP TABLE "ScheduleLayer"
-- DROP TABLE "OnCallScheduleChangeHistory"
-- DROP TABLE "OnCallSchedule"
-- DROP TABLE "EscalationRuleChangeHistory"
-- DROP TABLE "EscalationRule"
-- DROP TABLE "ServiceChangeHistory"
-- DROP TABLE "Service"
-- DROP TABLE "EscalationPolicyChangeHistory"
-- DROP TABLE "EscalationPolicy"
-- DROP TABLE "ScheduleOverrideType"
-- DROP TABLE "NotificationChannelType"
-- DROP TABLE "IncidentEventType"
-- DROP TABLE "IncidentStatusType"
-- DROP TABLE "SeverityType"

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON "WebhookDeliveryAttempt" DISABLE
-- ALTER INDEX ALL ON "NotificationDeliveryAttempt" DISABLE
-- ALTER INDEX ALL ON "IncidentNotification" DISABLE
-- ALTER INDEX ALL ON "IncidentNoteChangeHistory" DISABLE
-- ALTER INDEX ALL ON "IncidentNote" DISABLE
-- ALTER INDEX ALL ON "IncidentTimelineEvent" DISABLE
-- ALTER INDEX ALL ON "IncidentChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Incident" DISABLE
-- ALTER INDEX ALL ON "UserPushTokenChangeHistory" DISABLE
-- ALTER INDEX ALL ON "UserPushToken" DISABLE
-- ALTER INDEX ALL ON "UserNotificationChannelPreferenceChangeHistory" DISABLE
-- ALTER INDEX ALL ON "UserNotificationChannelPreference" DISABLE
-- ALTER INDEX ALL ON "UserNotificationPreferenceChangeHistory" DISABLE
-- ALTER INDEX ALL ON "UserNotificationPreference" DISABLE
-- ALTER INDEX ALL ON "IntegrationCallbackIncidentEventTypeChangeHistory" DISABLE
-- ALTER INDEX ALL ON "IntegrationCallbackIncidentEventType" DISABLE
-- ALTER INDEX ALL ON "IntegrationChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Integration" DISABLE
-- ALTER INDEX ALL ON "ScheduleOverrideChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ScheduleOverride" DISABLE
-- ALTER INDEX ALL ON "ScheduleLayerMemberChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ScheduleLayerMember" DISABLE
-- ALTER INDEX ALL ON "ScheduleLayerChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ScheduleLayer" DISABLE
-- ALTER INDEX ALL ON "OnCallScheduleChangeHistory" DISABLE
-- ALTER INDEX ALL ON "OnCallSchedule" DISABLE
-- ALTER INDEX ALL ON "EscalationRuleChangeHistory" DISABLE
-- ALTER INDEX ALL ON "EscalationRule" DISABLE
-- ALTER INDEX ALL ON "ServiceChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Service" DISABLE
-- ALTER INDEX ALL ON "EscalationPolicyChangeHistory" DISABLE
-- ALTER INDEX ALL ON "EscalationPolicy" DISABLE
-- ALTER INDEX ALL ON "ScheduleOverrideType" DISABLE
-- ALTER INDEX ALL ON "NotificationChannelType" DISABLE
-- ALTER INDEX ALL ON "IncidentEventType" DISABLE
-- ALTER INDEX ALL ON "IncidentStatusType" DISABLE
-- ALTER INDEX ALL ON "SeverityType" DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON "WebhookDeliveryAttempt" REBUILD
-- ALTER INDEX ALL ON "NotificationDeliveryAttempt" REBUILD
-- ALTER INDEX ALL ON "IncidentNotification" REBUILD
-- ALTER INDEX ALL ON "IncidentNoteChangeHistory" REBUILD
-- ALTER INDEX ALL ON "IncidentNote" REBUILD
-- ALTER INDEX ALL ON "IncidentTimelineEvent" REBUILD
-- ALTER INDEX ALL ON "IncidentChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Incident" REBUILD
-- ALTER INDEX ALL ON "UserPushTokenChangeHistory" REBUILD
-- ALTER INDEX ALL ON "UserPushToken" REBUILD
-- ALTER INDEX ALL ON "UserNotificationChannelPreferenceChangeHistory" REBUILD
-- ALTER INDEX ALL ON "UserNotificationChannelPreference" REBUILD
-- ALTER INDEX ALL ON "UserNotificationPreferenceChangeHistory" REBUILD
-- ALTER INDEX ALL ON "UserNotificationPreference" REBUILD
-- ALTER INDEX ALL ON "IntegrationCallbackIncidentEventTypeChangeHistory" REBUILD
-- ALTER INDEX ALL ON "IntegrationCallbackIncidentEventType" REBUILD
-- ALTER INDEX ALL ON "IntegrationChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Integration" REBUILD
-- ALTER INDEX ALL ON "ScheduleOverrideChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ScheduleOverride" REBUILD
-- ALTER INDEX ALL ON "ScheduleLayerMemberChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ScheduleLayerMember" REBUILD
-- ALTER INDEX ALL ON "ScheduleLayerChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ScheduleLayer" REBUILD
-- ALTER INDEX ALL ON "OnCallScheduleChangeHistory" REBUILD
-- ALTER INDEX ALL ON "OnCallSchedule" REBUILD
-- ALTER INDEX ALL ON "EscalationRuleChangeHistory" REBUILD
-- ALTER INDEX ALL ON "EscalationRule" REBUILD
-- ALTER INDEX ALL ON "ServiceChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Service" REBUILD
-- ALTER INDEX ALL ON "EscalationPolicyChangeHistory" REBUILD
-- ALTER INDEX ALL ON "EscalationPolicy" REBUILD
-- ALTER INDEX ALL ON "ScheduleOverrideType" REBUILD
-- ALTER INDEX ALL ON "NotificationChannelType" REBUILD
-- ALTER INDEX ALL ON "IncidentEventType" REBUILD
-- ALTER INDEX ALL ON "IncidentStatusType" REBUILD
-- ALTER INDEX ALL ON "SeverityType" REBUILD

-- Static severity levels for incidents.
CREATE TABLE "SeverityType"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"sequence" INTEGER NOT NULL,		-- Sequence to use for sorting.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the SeverityType table's name field.
CREATE INDEX "I_SeverityType_name" ON "SeverityType" ("name")
;

-- Index on the SeverityType table's active field.
CREATE INDEX "I_SeverityType_active" ON "SeverityType" ("active")
;

-- Index on the SeverityType table's deleted field.
CREATE INDEX "I_SeverityType_deleted" ON "SeverityType" ("deleted")
;

INSERT INTO "SeverityType" ( "name", "description", "sequence" ) VALUES  ( 'Critical', 'Critical', 10 );

INSERT INTO "SeverityType" ( "name", "description", "sequence" ) VALUES  ( 'High', 'High', 20 );

INSERT INTO "SeverityType" ( "name", "description", "sequence" ) VALUES  ( 'Medium', 'Medium', 30 );

INSERT INTO "SeverityType" ( "name", "description", "sequence" ) VALUES  ( 'Low', 'Low', 40 );


-- Static status values for incidents.
CREATE TABLE "IncidentStatusType"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the IncidentStatusType table's name field.
CREATE INDEX "I_IncidentStatusType_name" ON "IncidentStatusType" ("name")
;

-- Index on the IncidentStatusType table's active field.
CREATE INDEX "I_IncidentStatusType_active" ON "IncidentStatusType" ("active")
;

-- Index on the IncidentStatusType table's deleted field.
CREATE INDEX "I_IncidentStatusType_deleted" ON "IncidentStatusType" ("deleted")
;

INSERT INTO "IncidentStatusType" ( "name", "description" ) VALUES  ( 'Triggered', 'Newly triggered incident' );

INSERT INTO "IncidentStatusType" ( "name", "description" ) VALUES  ( 'Acknowledged', 'Acknowledged by a responder' );

INSERT INTO "IncidentStatusType" ( "name", "description" ) VALUES  ( 'Resolved', 'Incident resolved' );


-- Static event types for the incident timeline.
CREATE TABLE "IncidentEventType"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the IncidentEventType table's name field.
CREATE INDEX "I_IncidentEventType_name" ON "IncidentEventType" ("name")
;

-- Index on the IncidentEventType table's active field.
CREATE INDEX "I_IncidentEventType_active" ON "IncidentEventType" ("active")
;

-- Index on the IncidentEventType table's deleted field.
CREATE INDEX "I_IncidentEventType_deleted" ON "IncidentEventType" ("deleted")
;

INSERT INTO "IncidentEventType" ( "name", "description" ) VALUES  ( 'Triggered', 'Incident was triggered' );

INSERT INTO "IncidentEventType" ( "name", "description" ) VALUES  ( 'Escalated', 'Escalation rule fired' );

INSERT INTO "IncidentEventType" ( "name", "description" ) VALUES  ( 'Acknowledged', 'Incident acknowledged' );

INSERT INTO "IncidentEventType" ( "name", "description" ) VALUES  ( 'Resolved', 'Incident resolved' );

INSERT INTO "IncidentEventType" ( "name", "description" ) VALUES  ( 'NoteAdded', 'Note added to incident' );

INSERT INTO "IncidentEventType" ( "name", "description" ) VALUES  ( 'NotificationSent', 'Notification delivery attempted' );


-- Static notification delivery channels.
CREATE TABLE "NotificationChannelType"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"defaultPriority" INTEGER NOT NULL DEFAULT 0,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the NotificationChannelType table's name field.
CREATE INDEX "I_NotificationChannelType_name" ON "NotificationChannelType" ("name")
;

-- Index on the NotificationChannelType table's active field.
CREATE INDEX "I_NotificationChannelType_active" ON "NotificationChannelType" ("active")
;

-- Index on the NotificationChannelType table's deleted field.
CREATE INDEX "I_NotificationChannelType_deleted" ON "NotificationChannelType" ("deleted")
;

INSERT INTO "NotificationChannelType" ( "name", "description", "defaultPriority" ) VALUES  ( 'Email', 'Email notification', 30 );

INSERT INTO "NotificationChannelType" ( "name", "description", "defaultPriority" ) VALUES  ( 'SMS', 'SMS text message', 10 );

INSERT INTO "NotificationChannelType" ( "name", "description", "defaultPriority" ) VALUES  ( 'VoiceCall', 'Automated voice call', 5 );

INSERT INTO "NotificationChannelType" ( "name", "description", "defaultPriority" ) VALUES  ( 'MobilePush', 'Mobile app push', 20 );

INSERT INTO "NotificationChannelType" ( "name", "description", "defaultPriority" ) VALUES  ( 'WebPush', 'Browser push notification', 25 );

INSERT INTO "NotificationChannelType" ( "name", "description", "defaultPriority" ) VALUES  ( 'Teams', 'Microsoft Teams message', 40 );


-- Static schedule override types.
CREATE TABLE "ScheduleOverrideType"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the ScheduleOverrideType table's name field.
CREATE INDEX "I_ScheduleOverrideType_name" ON "ScheduleOverrideType" ("name")
;

-- Index on the ScheduleOverrideType table's active field.
CREATE INDEX "I_ScheduleOverrideType_active" ON "ScheduleOverrideType" ("active")
;

-- Index on the ScheduleOverrideType table's deleted field.
CREATE INDEX "I_ScheduleOverrideType_deleted" ON "ScheduleOverrideType" ("deleted")
;

INSERT INTO "ScheduleOverrideType" ( "name", "description" ) VALUES  ( 'Swap', 'Swap - Two users exchange shifts' );

INSERT INTO "ScheduleOverrideType" ( "name", "description" ) VALUES  ( 'Replace', 'Replace - One user temporarily takes over for another' );

INSERT INTO "ScheduleOverrideType" ( "name", "description" ) VALUES  ( 'Remove', 'Remove - User taken off the schedule with no replacement' );


-- Escalation policies assigned to services.
CREATE TABLE "EscalationPolicy"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the EscalationPolicy table's tenantGuid and name fields.
);
-- Index on the EscalationPolicy table's tenantGuid field.
CREATE INDEX "I_EscalationPolicy_tenantGuid" ON "EscalationPolicy" ("tenantGuid")
;

-- Index on the EscalationPolicy table's tenantGuid,name fields.
CREATE INDEX "I_EscalationPolicy_tenantGuid_name" ON "EscalationPolicy" ("tenantGuid", "name")
;

-- Index on the EscalationPolicy table's tenantGuid,active fields.
CREATE INDEX "I_EscalationPolicy_tenantGuid_active" ON "EscalationPolicy" ("tenantGuid", "active")
;

-- Index on the EscalationPolicy table's tenantGuid,deleted fields.
CREATE INDEX "I_EscalationPolicy_tenantGuid_deleted" ON "EscalationPolicy" ("tenantGuid", "deleted")
;


-- The change history for records from the EscalationPolicy table.
CREATE TABLE "EscalationPolicyChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"escalationPolicyId" INTEGER NOT NULL,		-- Link to the EscalationPolicy table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("escalationPolicyId") REFERENCES "EscalationPolicy"("id")		-- Foreign key to the EscalationPolicy table.
);
-- Index on the EscalationPolicyChangeHistory table's tenantGuid field.
CREATE INDEX "I_EscalationPolicyChangeHistory_tenantGuid" ON "EscalationPolicyChangeHistory" ("tenantGuid")
;

-- Index on the EscalationPolicyChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_EscalationPolicyChangeHistory_tenantGuid_versionNumber" ON "EscalationPolicyChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the EscalationPolicyChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_EscalationPolicyChangeHistory_tenantGuid_timeStamp" ON "EscalationPolicyChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the EscalationPolicyChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_EscalationPolicyChangeHistory_tenantGuid_userId" ON "EscalationPolicyChangeHistory" ("tenantGuid", "userId")
;

-- Index on the EscalationPolicyChangeHistory table's tenantGuid,escalationPolicyId fields.
CREATE INDEX "I_EscalationPolicyChangeHistory_tenantGuid_escalationPolicyId" ON "EscalationPolicyChangeHistory" ("tenantGuid", "escalationPolicyId", "versionNumber", "timeStamp", "userId")
;


-- Monitored services/applications that can generate alerts.
CREATE TABLE "Service"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"escalationPolicyId" INTEGER NULL,		-- Link to the EscalationPolicy table.
	"name" VARCHAR(100) NOT NULL COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"ownerTeamObjectGuid" VARCHAR(50) NULL COLLATE NOCASE,		-- References Security.SecurityTeam.objectGuid - logical owner.
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("escalationPolicyId") REFERENCES "EscalationPolicy"("id"),		-- Foreign key to the EscalationPolicy table.
	UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Service table's tenantGuid and name fields.
);
-- Index on the Service table's tenantGuid field.
CREATE INDEX "I_Service_tenantGuid" ON "Service" ("tenantGuid")
;

-- Index on the Service table's tenantGuid,escalationPolicyId fields.
CREATE INDEX "I_Service_tenantGuid_escalationPolicyId" ON "Service" ("tenantGuid", "escalationPolicyId")
;

-- Index on the Service table's tenantGuid,name fields.
CREATE INDEX "I_Service_tenantGuid_name" ON "Service" ("tenantGuid", "name")
;

-- Index on the Service table's tenantGuid,active fields.
CREATE INDEX "I_Service_tenantGuid_active" ON "Service" ("tenantGuid", "active")
;

-- Index on the Service table's tenantGuid,deleted fields.
CREATE INDEX "I_Service_tenantGuid_deleted" ON "Service" ("tenantGuid", "deleted")
;


-- The change history for records from the Service table.
CREATE TABLE "ServiceChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"serviceId" INTEGER NOT NULL,		-- Link to the Service table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("serviceId") REFERENCES "Service"("id")		-- Foreign key to the Service table.
);
-- Index on the ServiceChangeHistory table's tenantGuid field.
CREATE INDEX "I_ServiceChangeHistory_tenantGuid" ON "ServiceChangeHistory" ("tenantGuid")
;

-- Index on the ServiceChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ServiceChangeHistory_tenantGuid_versionNumber" ON "ServiceChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ServiceChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ServiceChangeHistory_tenantGuid_timeStamp" ON "ServiceChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ServiceChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ServiceChangeHistory_tenantGuid_userId" ON "ServiceChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ServiceChangeHistory table's tenantGuid,serviceId fields.
CREATE INDEX "I_ServiceChangeHistory_tenantGuid_serviceId" ON "ServiceChangeHistory" ("tenantGuid", "serviceId", "versionNumber", "timeStamp", "userId")
;


-- Individual escalation rules (ordered). Supports repeat looping until acknowledgment.
CREATE TABLE "EscalationRule"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"escalationPolicyId" INTEGER NOT NULL,		-- Link to the EscalationPolicy table.
	"ruleOrder" INTEGER NOT NULL DEFAULT 0,
	"delayMinutes" INTEGER NOT NULL DEFAULT 0,
	"repeatCount" INTEGER NOT NULL DEFAULT 0,		-- How many times to repeat notification if no ack (0 = no repeat).
	"repeatDelayMinutes" INTEGER NULL,		-- Delay between repeat attempts (null = same as delayMinutes).
	"targetType" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- Valid values: User, Team, Schedule
	"targetObjectGuid" VARCHAR(50) NULL COLLATE NOCASE,		-- References Security.SecurityUser/SecurityTeam or Alerting.OnCallSchedule objectGuid.
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("escalationPolicyId") REFERENCES "EscalationPolicy"("id")		-- Foreign key to the EscalationPolicy table.
);
-- Index on the EscalationRule table's tenantGuid field.
CREATE INDEX "I_EscalationRule_tenantGuid" ON "EscalationRule" ("tenantGuid")
;

-- Index on the EscalationRule table's tenantGuid,escalationPolicyId fields.
CREATE INDEX "I_EscalationRule_tenantGuid_escalationPolicyId" ON "EscalationRule" ("tenantGuid", "escalationPolicyId")
;

-- Index on the EscalationRule table's tenantGuid,ruleOrder fields.
CREATE INDEX "I_EscalationRule_tenantGuid_ruleOrder" ON "EscalationRule" ("tenantGuid", "ruleOrder")
;

-- Index on the EscalationRule table's tenantGuid,active fields.
CREATE INDEX "I_EscalationRule_tenantGuid_active" ON "EscalationRule" ("tenantGuid", "active")
;

-- Index on the EscalationRule table's tenantGuid,deleted fields.
CREATE INDEX "I_EscalationRule_tenantGuid_deleted" ON "EscalationRule" ("tenantGuid", "deleted")
;

-- Index on the EscalationRule table's escalationPolicyId,ruleOrder fields.
CREATE INDEX "I_EscalationRule_escalationPolicyId_ruleOrder" ON "EscalationRule" ("escalationPolicyId", "ruleOrder")
;


-- The change history for records from the EscalationRule table.
CREATE TABLE "EscalationRuleChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"escalationRuleId" INTEGER NOT NULL,		-- Link to the EscalationRule table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("escalationRuleId") REFERENCES "EscalationRule"("id")		-- Foreign key to the EscalationRule table.
);
-- Index on the EscalationRuleChangeHistory table's tenantGuid field.
CREATE INDEX "I_EscalationRuleChangeHistory_tenantGuid" ON "EscalationRuleChangeHistory" ("tenantGuid")
;

-- Index on the EscalationRuleChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_EscalationRuleChangeHistory_tenantGuid_versionNumber" ON "EscalationRuleChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the EscalationRuleChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_EscalationRuleChangeHistory_tenantGuid_timeStamp" ON "EscalationRuleChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the EscalationRuleChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_EscalationRuleChangeHistory_tenantGuid_userId" ON "EscalationRuleChangeHistory" ("tenantGuid", "userId")
;

-- Index on the EscalationRuleChangeHistory table's tenantGuid,escalationRuleId fields.
CREATE INDEX "I_EscalationRuleChangeHistory_tenantGuid_escalationRuleId" ON "EscalationRuleChangeHistory" ("tenantGuid", "escalationRuleId", "versionNumber", "timeStamp", "userId")
;


-- On-call rotation schedules (dynamic targets for escalation rules).
CREATE TABLE "OnCallSchedule"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"timeZoneId" VARCHAR(50) NOT NULL DEFAULT 'UTC' COLLATE NOCASE,
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the OnCallSchedule table's tenantGuid and name fields.
);
-- Index on the OnCallSchedule table's tenantGuid field.
CREATE INDEX "I_OnCallSchedule_tenantGuid" ON "OnCallSchedule" ("tenantGuid")
;

-- Index on the OnCallSchedule table's tenantGuid,name fields.
CREATE INDEX "I_OnCallSchedule_tenantGuid_name" ON "OnCallSchedule" ("tenantGuid", "name")
;

-- Index on the OnCallSchedule table's tenantGuid,active fields.
CREATE INDEX "I_OnCallSchedule_tenantGuid_active" ON "OnCallSchedule" ("tenantGuid", "active")
;

-- Index on the OnCallSchedule table's tenantGuid,deleted fields.
CREATE INDEX "I_OnCallSchedule_tenantGuid_deleted" ON "OnCallSchedule" ("tenantGuid", "deleted")
;


-- The change history for records from the OnCallSchedule table.
CREATE TABLE "OnCallScheduleChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"onCallScheduleId" INTEGER NOT NULL,		-- Link to the OnCallSchedule table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("onCallScheduleId") REFERENCES "OnCallSchedule"("id")		-- Foreign key to the OnCallSchedule table.
);
-- Index on the OnCallScheduleChangeHistory table's tenantGuid field.
CREATE INDEX "I_OnCallScheduleChangeHistory_tenantGuid" ON "OnCallScheduleChangeHistory" ("tenantGuid")
;

-- Index on the OnCallScheduleChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_OnCallScheduleChangeHistory_tenantGuid_versionNumber" ON "OnCallScheduleChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the OnCallScheduleChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_OnCallScheduleChangeHistory_tenantGuid_timeStamp" ON "OnCallScheduleChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the OnCallScheduleChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_OnCallScheduleChangeHistory_tenantGuid_userId" ON "OnCallScheduleChangeHistory" ("tenantGuid", "userId")
;

-- Index on the OnCallScheduleChangeHistory table's tenantGuid,onCallScheduleId fields.
CREATE INDEX "I_OnCallScheduleChangeHistory_tenantGuid_onCallScheduleId" ON "OnCallScheduleChangeHistory" ("tenantGuid", "onCallScheduleId", "versionNumber", "timeStamp", "userId")
;


-- Layers within an on-call schedule (primary, secondary, etc.).
CREATE TABLE "ScheduleLayer"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"onCallScheduleId" INTEGER NOT NULL,		-- Link to the OnCallSchedule table.
	"name" VARCHAR(100) NOT NULL COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"layerLevel" INTEGER NOT NULL DEFAULT 1,
	"rotationStart" DATETIME NOT NULL,
	"rotationDays" INTEGER NOT NULL DEFAULT 7,
	"handoffTime" VARCHAR(50) NOT NULL DEFAULT '09:00' COLLATE NOCASE,
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("onCallScheduleId") REFERENCES "OnCallSchedule"("id"),		-- Foreign key to the OnCallSchedule table.
	UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the ScheduleLayer table's tenantGuid and name fields.
);
-- Index on the ScheduleLayer table's tenantGuid field.
CREATE INDEX "I_ScheduleLayer_tenantGuid" ON "ScheduleLayer" ("tenantGuid")
;

-- Index on the ScheduleLayer table's tenantGuid,onCallScheduleId fields.
CREATE INDEX "I_ScheduleLayer_tenantGuid_onCallScheduleId" ON "ScheduleLayer" ("tenantGuid", "onCallScheduleId")
;

-- Index on the ScheduleLayer table's tenantGuid,name fields.
CREATE INDEX "I_ScheduleLayer_tenantGuid_name" ON "ScheduleLayer" ("tenantGuid", "name")
;

-- Index on the ScheduleLayer table's tenantGuid,active fields.
CREATE INDEX "I_ScheduleLayer_tenantGuid_active" ON "ScheduleLayer" ("tenantGuid", "active")
;

-- Index on the ScheduleLayer table's tenantGuid,deleted fields.
CREATE INDEX "I_ScheduleLayer_tenantGuid_deleted" ON "ScheduleLayer" ("tenantGuid", "deleted")
;


-- The change history for records from the ScheduleLayer table.
CREATE TABLE "ScheduleLayerChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"scheduleLayerId" INTEGER NOT NULL,		-- Link to the ScheduleLayer table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("scheduleLayerId") REFERENCES "ScheduleLayer"("id")		-- Foreign key to the ScheduleLayer table.
);
-- Index on the ScheduleLayerChangeHistory table's tenantGuid field.
CREATE INDEX "I_ScheduleLayerChangeHistory_tenantGuid" ON "ScheduleLayerChangeHistory" ("tenantGuid")
;

-- Index on the ScheduleLayerChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ScheduleLayerChangeHistory_tenantGuid_versionNumber" ON "ScheduleLayerChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ScheduleLayerChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ScheduleLayerChangeHistory_tenantGuid_timeStamp" ON "ScheduleLayerChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ScheduleLayerChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ScheduleLayerChangeHistory_tenantGuid_userId" ON "ScheduleLayerChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ScheduleLayerChangeHistory table's tenantGuid,scheduleLayerId fields.
CREATE INDEX "I_ScheduleLayerChangeHistory_tenantGuid_scheduleLayerId" ON "ScheduleLayerChangeHistory" ("tenantGuid", "scheduleLayerId", "versionNumber", "timeStamp", "userId")
;


-- Users in a schedule layer rotation (ordered).
CREATE TABLE "ScheduleLayerMember"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"scheduleLayerId" INTEGER NOT NULL,		-- Link to the ScheduleLayer table.
	"position" INTEGER NOT NULL DEFAULT 0,
	"securityUserObjectGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- References Security.SecurityUser.objectGuid
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("scheduleLayerId") REFERENCES "ScheduleLayer"("id"),		-- Foreign key to the ScheduleLayer table.
	UNIQUE ( "tenantGuid", "scheduleLayerId", "position") 		-- Uniqueness enforced on the ScheduleLayerMember table's tenantGuid and scheduleLayerId and position fields.
);
-- Index on the ScheduleLayerMember table's tenantGuid field.
CREATE INDEX "I_ScheduleLayerMember_tenantGuid" ON "ScheduleLayerMember" ("tenantGuid")
;

-- Index on the ScheduleLayerMember table's tenantGuid,scheduleLayerId fields.
CREATE INDEX "I_ScheduleLayerMember_tenantGuid_scheduleLayerId" ON "ScheduleLayerMember" ("tenantGuid", "scheduleLayerId")
;

-- Index on the ScheduleLayerMember table's tenantGuid,active fields.
CREATE INDEX "I_ScheduleLayerMember_tenantGuid_active" ON "ScheduleLayerMember" ("tenantGuid", "active")
;

-- Index on the ScheduleLayerMember table's tenantGuid,deleted fields.
CREATE INDEX "I_ScheduleLayerMember_tenantGuid_deleted" ON "ScheduleLayerMember" ("tenantGuid", "deleted")
;


-- The change history for records from the ScheduleLayerMember table.
CREATE TABLE "ScheduleLayerMemberChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"scheduleLayerMemberId" INTEGER NOT NULL,		-- Link to the ScheduleLayerMember table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("scheduleLayerMemberId") REFERENCES "ScheduleLayerMember"("id")		-- Foreign key to the ScheduleLayerMember table.
);
-- Index on the ScheduleLayerMemberChangeHistory table's tenantGuid field.
CREATE INDEX "I_ScheduleLayerMemberChangeHistory_tenantGuid" ON "ScheduleLayerMemberChangeHistory" ("tenantGuid")
;

-- Index on the ScheduleLayerMemberChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ScheduleLayerMemberChangeHistory_tenantGuid_versionNumber" ON "ScheduleLayerMemberChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ScheduleLayerMemberChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ScheduleLayerMemberChangeHistory_tenantGuid_timeStamp" ON "ScheduleLayerMemberChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ScheduleLayerMemberChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ScheduleLayerMemberChangeHistory_tenantGuid_userId" ON "ScheduleLayerMemberChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ScheduleLayerMemberChangeHistory table's tenantGuid,scheduleLayerMemberId fields.
CREATE INDEX "I_SchdulLyrMmbrChngHstry_tnntGud_schdulLyrMmbrd" ON "ScheduleLayerMemberChangeHistory" ("tenantGuid", "scheduleLayerMemberId", "versionNumber", "timeStamp", "userId")
;


-- Temporary overrides for on-call schedules (vacations, swaps, emergency substitutions).
CREATE TABLE "ScheduleOverride"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"onCallScheduleId" INTEGER NOT NULL,		-- Link to the OnCallSchedule table.
	"scheduleLayerId" INTEGER NULL,		-- If null, override applies to all layers in the schedule.
	"startDateTime" DATETIME NOT NULL,		-- Start of override period (inclusive).
	"endDateTime" DATETIME NOT NULL,		-- End of override period (exclusive).
	"scheduleOverrideTypeId" INTEGER NOT NULL,		-- The type of override.  Will be one of Swap, Replace, or Remove
	"originalUserObjectGuid" VARCHAR(50) NULL COLLATE NOCASE,		-- The user being replaced (null for layer-wide overrides).
	"replacementUserObjectGuid" VARCHAR(50) NULL COLLATE NOCASE,		-- The substitute user (null for REMOVE type).
	"reason" VARCHAR(500) NULL COLLATE NOCASE,		-- Optional explanation (vacation, sick, training, etc.).
	"createdByUserObjectGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- References Security.SecurityUser.objectGuid - who created the override.
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("onCallScheduleId") REFERENCES "OnCallSchedule"("id"),		-- Foreign key to the OnCallSchedule table.
	FOREIGN KEY ("scheduleLayerId") REFERENCES "ScheduleLayer"("id"),		-- Foreign key to the ScheduleLayer table.
	FOREIGN KEY ("scheduleOverrideTypeId") REFERENCES "ScheduleOverrideType"("id")		-- Foreign key to the ScheduleOverrideType table.
);
-- Index on the ScheduleOverride table's tenantGuid field.
CREATE INDEX "I_ScheduleOverride_tenantGuid" ON "ScheduleOverride" ("tenantGuid")
;

-- Index on the ScheduleOverride table's tenantGuid,onCallScheduleId fields.
CREATE INDEX "I_ScheduleOverride_tenantGuid_onCallScheduleId" ON "ScheduleOverride" ("tenantGuid", "onCallScheduleId")
;

-- Index on the ScheduleOverride table's tenantGuid,scheduleLayerId fields.
CREATE INDEX "I_ScheduleOverride_tenantGuid_scheduleLayerId" ON "ScheduleOverride" ("tenantGuid", "scheduleLayerId")
;

-- Index on the ScheduleOverride table's tenantGuid,scheduleOverrideTypeId fields.
CREATE INDEX "I_ScheduleOverride_tenantGuid_scheduleOverrideTypeId" ON "ScheduleOverride" ("tenantGuid", "scheduleOverrideTypeId")
;

-- Index on the ScheduleOverride table's tenantGuid,active fields.
CREATE INDEX "I_ScheduleOverride_tenantGuid_active" ON "ScheduleOverride" ("tenantGuid", "active")
;

-- Index on the ScheduleOverride table's tenantGuid,deleted fields.
CREATE INDEX "I_ScheduleOverride_tenantGuid_deleted" ON "ScheduleOverride" ("tenantGuid", "deleted")
;

-- Index on the ScheduleOverride table's tenantGuid,onCallScheduleId,startDateTime,endDateTime fields.
CREATE INDEX "I_Schdulvrrd_tnntGud_nCllSchduld_strtDtTm_ndDtTm" ON "ScheduleOverride" ("tenantGuid", "onCallScheduleId", "startDateTime", "endDateTime")
;

-- Index on the ScheduleOverride table's tenantGuid,originalUserObjectGuid fields.
CREATE INDEX "I_ScheduleOverride_tenantGuid_originalUserObjectGuid" ON "ScheduleOverride" ("tenantGuid", "originalUserObjectGuid")
;


-- The change history for records from the ScheduleOverride table.
CREATE TABLE "ScheduleOverrideChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"scheduleOverrideId" INTEGER NOT NULL,		-- Link to the ScheduleOverride table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("scheduleOverrideId") REFERENCES "ScheduleOverride"("id")		-- Foreign key to the ScheduleOverride table.
);
-- Index on the ScheduleOverrideChangeHistory table's tenantGuid field.
CREATE INDEX "I_ScheduleOverrideChangeHistory_tenantGuid" ON "ScheduleOverrideChangeHistory" ("tenantGuid")
;

-- Index on the ScheduleOverrideChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ScheduleOverrideChangeHistory_tenantGuid_versionNumber" ON "ScheduleOverrideChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ScheduleOverrideChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ScheduleOverrideChangeHistory_tenantGuid_timeStamp" ON "ScheduleOverrideChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ScheduleOverrideChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ScheduleOverrideChangeHistory_tenantGuid_userId" ON "ScheduleOverrideChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ScheduleOverrideChangeHistory table's tenantGuid,scheduleOverrideId fields.
CREATE INDEX "I_ScheduleOverrideChangeHistory_tenantGuid_scheduleOverrideId" ON "ScheduleOverrideChangeHistory" ("tenantGuid", "scheduleOverrideId", "versionNumber", "timeStamp", "userId")
;


-- API integrations for inbound alerts and outbound status callbacks.
CREATE TABLE "Integration"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"serviceId" INTEGER NOT NULL,		-- Link to the Service table.
	"name" VARCHAR(100) NOT NULL COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"apiKeyHash" VARCHAR(250) NOT NULL UNIQUE COLLATE NOCASE,
	"callbackWebhookUrl" VARCHAR(1000) NULL COLLATE NOCASE,
	"maxRetryAttempts" INTEGER NULL DEFAULT 10,		-- How many times to retry failed deliveries
	"retryBackoffSeconds" INTEGER NULL DEFAULT 30,		-- Base seconds for backoff (30, 60, 120, 240...)
	"lastCallbackSuccessAt" DATETIME NULL,
	"consecutiveCallbackFailures" INTEGER NULL,
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("serviceId") REFERENCES "Service"("id"),		-- Foreign key to the Service table.
	UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Integration table's tenantGuid and name fields.
);
-- Index on the Integration table's tenantGuid field.
CREATE INDEX "I_Integration_tenantGuid" ON "Integration" ("tenantGuid")
;

-- Index on the Integration table's tenantGuid,serviceId fields.
CREATE INDEX "I_Integration_tenantGuid_serviceId" ON "Integration" ("tenantGuid", "serviceId")
;

-- Index on the Integration table's tenantGuid,name fields.
CREATE INDEX "I_Integration_tenantGuid_name" ON "Integration" ("tenantGuid", "name")
;

-- Index on the Integration table's tenantGuid,active fields.
CREATE INDEX "I_Integration_tenantGuid_active" ON "Integration" ("tenantGuid", "active")
;

-- Index on the Integration table's tenantGuid,deleted fields.
CREATE INDEX "I_Integration_tenantGuid_deleted" ON "Integration" ("tenantGuid", "deleted")
;


-- The change history for records from the Integration table.
CREATE TABLE "IntegrationChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"integrationId" INTEGER NOT NULL,		-- Link to the Integration table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("integrationId") REFERENCES "Integration"("id")		-- Foreign key to the Integration table.
);
-- Index on the IntegrationChangeHistory table's tenantGuid field.
CREATE INDEX "I_IntegrationChangeHistory_tenantGuid" ON "IntegrationChangeHistory" ("tenantGuid")
;

-- Index on the IntegrationChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_IntegrationChangeHistory_tenantGuid_versionNumber" ON "IntegrationChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the IntegrationChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_IntegrationChangeHistory_tenantGuid_timeStamp" ON "IntegrationChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the IntegrationChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_IntegrationChangeHistory_tenantGuid_userId" ON "IntegrationChangeHistory" ("tenantGuid", "userId")
;

-- Index on the IntegrationChangeHistory table's tenantGuid,integrationId fields.
CREATE INDEX "I_IntegrationChangeHistory_tenantGuid_integrationId" ON "IntegrationChangeHistory" ("tenantGuid", "integrationId", "versionNumber", "timeStamp", "userId")
;


-- API integrations incident event types to callback on.
CREATE TABLE "IntegrationCallbackIncidentEventType"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"integrationId" INTEGER NOT NULL,		-- Link to the Integration table.
	"incidentEventTypeId" INTEGER NOT NULL,		-- Link to the IncidentEventType table.
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("integrationId") REFERENCES "Integration"("id"),		-- Foreign key to the Integration table.
	FOREIGN KEY ("incidentEventTypeId") REFERENCES "IncidentEventType"("id")		-- Foreign key to the IncidentEventType table.
);
-- Index on the IntegrationCallbackIncidentEventType table's tenantGuid field.
CREATE INDEX "I_IntegrationCallbackIncidentEventType_tenantGuid" ON "IntegrationCallbackIncidentEventType" ("tenantGuid")
;

-- Index on the IntegrationCallbackIncidentEventType table's tenantGuid,integrationId fields.
CREATE INDEX "I_ntgrtnCllbckncdntvntTyp_tnntGud_ntgrtnd" ON "IntegrationCallbackIncidentEventType" ("tenantGuid", "integrationId")
;

-- Index on the IntegrationCallbackIncidentEventType table's tenantGuid,incidentEventTypeId fields.
CREATE INDEX "I_ntgrtnCllbckncdntvntTyp_tnntGud_ncdntvntTypd" ON "IntegrationCallbackIncidentEventType" ("tenantGuid", "incidentEventTypeId")
;

-- Index on the IntegrationCallbackIncidentEventType table's tenantGuid,active fields.
CREATE INDEX "I_IntegrationCallbackIncidentEventType_tenantGuid_active" ON "IntegrationCallbackIncidentEventType" ("tenantGuid", "active")
;

-- Index on the IntegrationCallbackIncidentEventType table's tenantGuid,deleted fields.
CREATE INDEX "I_IntegrationCallbackIncidentEventType_tenantGuid_deleted" ON "IntegrationCallbackIncidentEventType" ("tenantGuid", "deleted")
;


-- The change history for records from the IntegrationCallbackIncidentEventType table.
CREATE TABLE "IntegrationCallbackIncidentEventTypeChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"integrationCallbackIncidentEventTypeId" INTEGER NOT NULL,		-- Link to the IntegrationCallbackIncidentEventType table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("integrationCallbackIncidentEventTypeId") REFERENCES "IntegrationCallbackIncidentEventType"("id")		-- Foreign key to the IntegrationCallbackIncidentEventType table.
);
-- Index on the IntegrationCallbackIncidentEventTypeChangeHistory table's tenantGuid field.
CREATE INDEX "I_IntegrationCallbackIncidentEventTypeChangeHistory_tenantGuid" ON "IntegrationCallbackIncidentEventTypeChangeHistory" ("tenantGuid")
;

-- Index on the IntegrationCallbackIncidentEventTypeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ntgrtnCllbckncdntvntTypChngHstry_tnntGud_vrsnNumbr" ON "IntegrationCallbackIncidentEventTypeChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the IntegrationCallbackIncidentEventTypeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ntgrtnCllbckncdntvntTypChngHstry_tnntGud_tmStmp" ON "IntegrationCallbackIncidentEventTypeChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the IntegrationCallbackIncidentEventTypeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ntgrtnCllbckncdntvntTypChngHstry_tnntGud_usrd" ON "IntegrationCallbackIncidentEventTypeChangeHistory" ("tenantGuid", "userId")
;

-- Index on the IntegrationCallbackIncidentEventTypeChangeHistory table's tenantGuid,integrationCallbackIncidentEventTypeId fields.
CREATE INDEX "I_nCllbckncdntvntTypChngHstry_tnntGud_ntgrtnCllbckncdntvntTypd" ON "IntegrationCallbackIncidentEventTypeChangeHistory" ("tenantGuid", "integrationCallbackIncidentEventTypeId", "versionNumber", "timeStamp", "userId")
;


-- Per-user notification preferences (channels, quiet hours, DND, etc.). Users can edit their own preferences.
CREATE TABLE "UserNotificationPreference"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"securityUserObjectGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- References Security.SecurityUser.objectGuid - one preference row per user.
	"timeZoneId" VARCHAR(50) NULL DEFAULT 'UTC' COLLATE NOCASE,		-- User's preferred timezone for quiet hours scheduling.
	"quietHoursStart" VARCHAR(10) NULL COLLATE NOCASE,		-- HH:mm format local to timeZoneId - start of quiet hours (null = no quiet hours).
	"quietHoursEnd" VARCHAR(10) NULL COLLATE NOCASE,		-- HH:mm format local to timeZoneId - end of quiet hours.
	"isDoNotDisturb" BIT NOT NULL DEFAULT 0,		-- Global DND override - if true, no notifications except possibly critical overrides.
	"isDoNotDisturbPermanent" BIT NOT NULL DEFAULT 0,		-- If true, DND has no scheduled end (until manually cleared).
	"doNotDisturbUntil" DATETIME NULL,		-- Temporary DND end time (ignored if isDoNotDisturbPermanent = true).
	"customSettingsJson" TEXT NULL COLLATE NOCASE,		-- Flexible JSON for future extensions (e.g., per-severity overrides, custom sounds).
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE ( "tenantGuid", "securityUserObjectGuid", "active", "deleted") 		-- Uniqueness enforced on the UserNotificationPreference table's tenantGuid and securityUserObjectGuid and active and deleted fields.
);
-- Index on the UserNotificationPreference table's tenantGuid field.
CREATE INDEX "I_UserNotificationPreference_tenantGuid" ON "UserNotificationPreference" ("tenantGuid")
;

-- Index on the UserNotificationPreference table's tenantGuid,active fields.
CREATE INDEX "I_UserNotificationPreference_tenantGuid_active" ON "UserNotificationPreference" ("tenantGuid", "active")
;

-- Index on the UserNotificationPreference table's tenantGuid,deleted fields.
CREATE INDEX "I_UserNotificationPreference_tenantGuid_deleted" ON "UserNotificationPreference" ("tenantGuid", "deleted")
;

-- Index on the UserNotificationPreference table's securityUserObjectGuid field.
CREATE INDEX "I_UserNotificationPreference_securityUserObjectGuid" ON "UserNotificationPreference" ("securityUserObjectGuid")
;


-- The change history for records from the UserNotificationPreference table.
CREATE TABLE "UserNotificationPreferenceChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"userNotificationPreferenceId" INTEGER NOT NULL,		-- Link to the UserNotificationPreference table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("userNotificationPreferenceId") REFERENCES "UserNotificationPreference"("id")		-- Foreign key to the UserNotificationPreference table.
);
-- Index on the UserNotificationPreferenceChangeHistory table's tenantGuid field.
CREATE INDEX "I_UserNotificationPreferenceChangeHistory_tenantGuid" ON "UserNotificationPreferenceChangeHistory" ("tenantGuid")
;

-- Index on the UserNotificationPreferenceChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_srNtfctnPrfrncChngHstry_tnntGud_vrsnNumbr" ON "UserNotificationPreferenceChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the UserNotificationPreferenceChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_UserNotificationPreferenceChangeHistory_tenantGuid_timeStamp" ON "UserNotificationPreferenceChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the UserNotificationPreferenceChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_UserNotificationPreferenceChangeHistory_tenantGuid_userId" ON "UserNotificationPreferenceChangeHistory" ("tenantGuid", "userId")
;

-- Index on the UserNotificationPreferenceChangeHistory table's tenantGuid,userNotificationPreferenceId fields.
CREATE INDEX "I_srNtfctnPrfrncChngHstry_tnntGud_usrNtfctnPrfrncd" ON "UserNotificationPreferenceChangeHistory" ("tenantGuid", "userNotificationPreferenceId", "versionNumber", "timeStamp", "userId")
;


-- Per-user, per-channel notification preferences (enable/disable, custom priority).
CREATE TABLE "UserNotificationChannelPreference"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"userNotificationPreferenceId" INTEGER NOT NULL,		-- Link to the UserNotificationPreference table.
	"notificationChannelTypeId" INTEGER NOT NULL,		-- Link to the NotificationChannelType table.
	"isEnabled" BIT NOT NULL DEFAULT 1,		-- If false, this channel is disabled for the user (overrides system defaults).
	"priorityOverride" INTEGER NULL,		-- Optional custom priority (lower = higher urgency) - null = use channel default.
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("userNotificationPreferenceId") REFERENCES "UserNotificationPreference"("id"),		-- Foreign key to the UserNotificationPreference table.
	FOREIGN KEY ("notificationChannelTypeId") REFERENCES "NotificationChannelType"("id"),		-- Foreign key to the NotificationChannelType table.
	UNIQUE ( "tenantGuid", "userNotificationPreferenceId", "notificationChannelTypeId") 		-- Uniqueness enforced on the UserNotificationChannelPreference table's tenantGuid and userNotificationPreferenceId and notificationChannelTypeId fields.
);
-- Index on the UserNotificationChannelPreference table's tenantGuid field.
CREATE INDEX "I_UserNotificationChannelPreference_tenantGuid" ON "UserNotificationChannelPreference" ("tenantGuid")
;

-- Index on the UserNotificationChannelPreference table's tenantGuid,userNotificationPreferenceId fields.
CREATE INDEX "I_srNtfctnChnnlPrfrnc_tnntGud_usrNtfctnPrfrncd" ON "UserNotificationChannelPreference" ("tenantGuid", "userNotificationPreferenceId")
;

-- Index on the UserNotificationChannelPreference table's tenantGuid,notificationChannelTypeId fields.
CREATE INDEX "I_srNtfctnChnnlPrfrnc_tnntGud_ntfctnChnnlTypd" ON "UserNotificationChannelPreference" ("tenantGuid", "notificationChannelTypeId")
;

-- Index on the UserNotificationChannelPreference table's tenantGuid,active fields.
CREATE INDEX "I_UserNotificationChannelPreference_tenantGuid_active" ON "UserNotificationChannelPreference" ("tenantGuid", "active")
;

-- Index on the UserNotificationChannelPreference table's tenantGuid,deleted fields.
CREATE INDEX "I_UserNotificationChannelPreference_tenantGuid_deleted" ON "UserNotificationChannelPreference" ("tenantGuid", "deleted")
;


-- The change history for records from the UserNotificationChannelPreference table.
CREATE TABLE "UserNotificationChannelPreferenceChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"userNotificationChannelPreferenceId" INTEGER NOT NULL,		-- Link to the UserNotificationChannelPreference table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("userNotificationChannelPreferenceId") REFERENCES "UserNotificationChannelPreference"("id")		-- Foreign key to the UserNotificationChannelPreference table.
);
-- Index on the UserNotificationChannelPreferenceChangeHistory table's tenantGuid field.
CREATE INDEX "I_UserNotificationChannelPreferenceChangeHistory_tenantGuid" ON "UserNotificationChannelPreferenceChangeHistory" ("tenantGuid")
;

-- Index on the UserNotificationChannelPreferenceChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_srNtfctnChnnlPrfrncChngHstry_tnntGud_vrsnNumbr" ON "UserNotificationChannelPreferenceChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the UserNotificationChannelPreferenceChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_srNtfctnChnnlPrfrncChngHstry_tnntGud_tmStmp" ON "UserNotificationChannelPreferenceChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the UserNotificationChannelPreferenceChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_srNtfctnChnnlPrfrncChngHstry_tnntGud_usrd" ON "UserNotificationChannelPreferenceChangeHistory" ("tenantGuid", "userId")
;

-- Index on the UserNotificationChannelPreferenceChangeHistory table's tenantGuid,userNotificationChannelPreferenceId fields.
CREATE INDEX "I_srNtfctnChnnlPrfrncChngHstry_tnntGud_usrNtfctnChnnlPrfrncd" ON "UserNotificationChannelPreferenceChangeHistory" ("tenantGuid", "userNotificationChannelPreferenceId", "versionNumber", "timeStamp", "userId")
;


-- Push notification tokens for web and mobile devices. Each user can have multiple tokens (one per device).
CREATE TABLE "UserPushToken"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"userObjectGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- References Security.SecurityUser.objectGuid - the token owner.
	"fcmToken" VARCHAR(500) NOT NULL COLLATE NOCASE,		-- Firebase Cloud Messaging token for this device.
	"deviceFingerprint" VARCHAR(100) NOT NULL COLLATE NOCASE,		-- Unique identifier for the device/browser to prevent duplicates.
	"platform" VARCHAR(50) NOT NULL DEFAULT 'web' COLLATE NOCASE,		-- Platform: 'web', 'ios', 'android'.
	"userAgent" VARCHAR(500) NULL COLLATE NOCASE,		-- Browser/device user agent string for diagnostics.
	"registeredAt" DATETIME NOT NULL,		-- When the token was first registered.
	"lastUpdatedAt" DATETIME NOT NULL,		-- Last time the token was refreshed.
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE ( "tenantGuid", "userObjectGuid", "deviceFingerprint") 		-- Uniqueness enforced on the UserPushToken table's tenantGuid and userObjectGuid and deviceFingerprint fields.
);
-- Index on the UserPushToken table's tenantGuid field.
CREATE INDEX "I_UserPushToken_tenantGuid" ON "UserPushToken" ("tenantGuid")
;

-- Index on the UserPushToken table's tenantGuid,active fields.
CREATE INDEX "I_UserPushToken_tenantGuid_active" ON "UserPushToken" ("tenantGuid", "active")
;

-- Index on the UserPushToken table's tenantGuid,deleted fields.
CREATE INDEX "I_UserPushToken_tenantGuid_deleted" ON "UserPushToken" ("tenantGuid", "deleted")
;

-- Index on the UserPushToken table's tenantGuid,userObjectGuid fields.
CREATE INDEX "I_UserPushToken_tenantGuid_userObjectGuid" ON "UserPushToken" ("tenantGuid", "userObjectGuid")
;


-- The change history for records from the UserPushToken table.
CREATE TABLE "UserPushTokenChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"userPushTokenId" INTEGER NOT NULL,		-- Link to the UserPushToken table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("userPushTokenId") REFERENCES "UserPushToken"("id")		-- Foreign key to the UserPushToken table.
);
-- Index on the UserPushTokenChangeHistory table's tenantGuid field.
CREATE INDEX "I_UserPushTokenChangeHistory_tenantGuid" ON "UserPushTokenChangeHistory" ("tenantGuid")
;

-- Index on the UserPushTokenChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_UserPushTokenChangeHistory_tenantGuid_versionNumber" ON "UserPushTokenChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the UserPushTokenChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_UserPushTokenChangeHistory_tenantGuid_timeStamp" ON "UserPushTokenChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the UserPushTokenChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_UserPushTokenChangeHistory_tenantGuid_userId" ON "UserPushTokenChangeHistory" ("tenantGuid", "userId")
;

-- Index on the UserPushTokenChangeHistory table's tenantGuid,userPushTokenId fields.
CREATE INDEX "I_UserPushTokenChangeHistory_tenantGuid_userPushTokenId" ON "UserPushTokenChangeHistory" ("tenantGuid", "userPushTokenId", "versionNumber", "timeStamp", "userId")
;


-- Active and historical incidents.
CREATE TABLE "Incident"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"incidentKey" VARCHAR(250) NOT NULL COLLATE NOCASE,
	"serviceId" INTEGER NOT NULL,		-- Link to the Service table.
	"title" VARCHAR(250) NOT NULL COLLATE NOCASE,
	"description" TEXT NULL COLLATE NOCASE,
	"severityTypeId" INTEGER NOT NULL,		-- Link to the SeverityType table.
	"incidentStatusTypeId" INTEGER NOT NULL,		-- Link to the IncidentStatusType table.
	"createdAt" DATETIME NOT NULL,
	"escalationRuleId" INTEGER NULL,		-- Current active escalation rule (null = no active escalation, e.g., acknowledged/resolved).
	"currentRepeatCount" INTEGER NULL DEFAULT 0,		-- How many repeat notifications have been sent for the current rule (resets on rule change).
	"nextEscalationAt" DATETIME NULL,		-- Timestamp when the next escalation/repeat should fire (drives worker query).
	"acknowledgedAt" DATETIME NULL,
	"resolvedAt" DATETIME NULL,
	"currentAssigneeObjectGuid" VARCHAR(50) NULL COLLATE NOCASE,
	"sourcePayloadJson" TEXT NULL COLLATE NOCASE,
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("serviceId") REFERENCES "Service"("id"),		-- Foreign key to the Service table.
	FOREIGN KEY ("severityTypeId") REFERENCES "SeverityType"("id"),		-- Foreign key to the SeverityType table.
	FOREIGN KEY ("incidentStatusTypeId") REFERENCES "IncidentStatusType"("id"),		-- Foreign key to the IncidentStatusType table.
	FOREIGN KEY ("escalationRuleId") REFERENCES "EscalationRule"("id"),		-- Foreign key to the EscalationRule table.
	UNIQUE ( "tenantGuid", "incidentKey") 		-- Uniqueness enforced on the Incident table's tenantGuid and incidentKey fields.
);
-- Index on the Incident table's tenantGuid field.
CREATE INDEX "I_Incident_tenantGuid" ON "Incident" ("tenantGuid")
;

-- Index on the Incident table's tenantGuid,incidentKey fields.
CREATE INDEX "I_Incident_tenantGuid_incidentKey" ON "Incident" ("tenantGuid", "incidentKey")
;

-- Index on the Incident table's tenantGuid,serviceId fields.
CREATE INDEX "I_Incident_tenantGuid_serviceId" ON "Incident" ("tenantGuid", "serviceId")
;

-- Index on the Incident table's tenantGuid,severityTypeId fields.
CREATE INDEX "I_Incident_tenantGuid_severityTypeId" ON "Incident" ("tenantGuid", "severityTypeId")
;

-- Index on the Incident table's tenantGuid,incidentStatusTypeId fields.
CREATE INDEX "I_Incident_tenantGuid_incidentStatusTypeId" ON "Incident" ("tenantGuid", "incidentStatusTypeId")
;

-- Index on the Incident table's tenantGuid,createdAt fields.
CREATE INDEX "I_Incident_tenantGuid_createdAt" ON "Incident" ("tenantGuid", "createdAt")
;

-- Index on the Incident table's tenantGuid,escalationRuleId fields.
CREATE INDEX "I_Incident_tenantGuid_escalationRuleId" ON "Incident" ("tenantGuid", "escalationRuleId")
;

-- Index on the Incident table's tenantGuid,active fields.
CREATE INDEX "I_Incident_tenantGuid_active" ON "Incident" ("tenantGuid", "active")
;

-- Index on the Incident table's tenantGuid,deleted fields.
CREATE INDEX "I_Incident_tenantGuid_deleted" ON "Incident" ("tenantGuid", "deleted")
;

-- Index on the Incident table's tenantGuid,incidentStatusTypeId,createdAt fields.
CREATE INDEX "I_Incident_tenantGuid_incidentStatusTypeId_createdAt" ON "Incident" ("tenantGuid", "incidentStatusTypeId", "createdAt")
;

-- Index on the Incident table's tenantGuid,serviceId,createdAt fields.
CREATE INDEX "I_Incident_tenantGuid_serviceId_createdAt" ON "Incident" ("tenantGuid", "serviceId", "createdAt")
;

-- Index on the Incident table's tenantGuid,nextEscalationAt,incidentStatusTypeId fields.
CREATE INDEX "I_Incident_tenantGuid_nextEscalationAt_incidentStatusTypeId" ON "Incident" ("tenantGuid", "nextEscalationAt", "incidentStatusTypeId")
;


-- The change history for records from the Incident table.
CREATE TABLE "IncidentChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"incidentId" INTEGER NOT NULL,		-- Link to the Incident table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("incidentId") REFERENCES "Incident"("id")		-- Foreign key to the Incident table.
);
-- Index on the IncidentChangeHistory table's tenantGuid field.
CREATE INDEX "I_IncidentChangeHistory_tenantGuid" ON "IncidentChangeHistory" ("tenantGuid")
;

-- Index on the IncidentChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_IncidentChangeHistory_tenantGuid_versionNumber" ON "IncidentChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the IncidentChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_IncidentChangeHistory_tenantGuid_timeStamp" ON "IncidentChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the IncidentChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_IncidentChangeHistory_tenantGuid_userId" ON "IncidentChangeHistory" ("tenantGuid", "userId")
;

-- Index on the IncidentChangeHistory table's tenantGuid,incidentId fields.
CREATE INDEX "I_IncidentChangeHistory_tenantGuid_incidentId" ON "IncidentChangeHistory" ("tenantGuid", "incidentId", "versionNumber", "timeStamp", "userId")
;


-- Timeline events for incidents.
CREATE TABLE "IncidentTimelineEvent"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"incidentId" INTEGER NOT NULL,		-- Link to the Incident table.
	"incidentEventTypeId" INTEGER NOT NULL,		-- Link to the IncidentEventType table.
	"timestamp" DATETIME NOT NULL,
	"actorObjectGuid" VARCHAR(50) NULL COLLATE NOCASE,
	"detailsJson" TEXT NULL COLLATE NOCASE,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("incidentId") REFERENCES "Incident"("id"),		-- Foreign key to the Incident table.
	FOREIGN KEY ("incidentEventTypeId") REFERENCES "IncidentEventType"("id")		-- Foreign key to the IncidentEventType table.
);
-- Index on the IncidentTimelineEvent table's tenantGuid field.
CREATE INDEX "I_IncidentTimelineEvent_tenantGuid" ON "IncidentTimelineEvent" ("tenantGuid")
;

-- Index on the IncidentTimelineEvent table's tenantGuid,incidentId fields.
CREATE INDEX "I_IncidentTimelineEvent_tenantGuid_incidentId" ON "IncidentTimelineEvent" ("tenantGuid", "incidentId")
;

-- Index on the IncidentTimelineEvent table's tenantGuid,incidentEventTypeId fields.
CREATE INDEX "I_IncidentTimelineEvent_tenantGuid_incidentEventTypeId" ON "IncidentTimelineEvent" ("tenantGuid", "incidentEventTypeId")
;

-- Index on the IncidentTimelineEvent table's tenantGuid,timestamp fields.
CREATE INDEX "I_IncidentTimelineEvent_tenantGuid_timestamp" ON "IncidentTimelineEvent" ("tenantGuid", "timestamp")
;

-- Index on the IncidentTimelineEvent table's tenantGuid,active fields.
CREATE INDEX "I_IncidentTimelineEvent_tenantGuid_active" ON "IncidentTimelineEvent" ("tenantGuid", "active")
;

-- Index on the IncidentTimelineEvent table's tenantGuid,deleted fields.
CREATE INDEX "I_IncidentTimelineEvent_tenantGuid_deleted" ON "IncidentTimelineEvent" ("tenantGuid", "deleted")
;

-- Index on the IncidentTimelineEvent table's incidentId,timestamp fields.
CREATE INDEX "I_IncidentTimelineEvent_incidentId_timestamp" ON "IncidentTimelineEvent" ("incidentId", "timestamp")
;


-- Notes added to incidents by responders.
CREATE TABLE "IncidentNote"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"incidentId" INTEGER NOT NULL,		-- Link to the Incident table.
	"authorObjectGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,
	"createdAt" DATETIME NOT NULL,
	"content" TEXT NOT NULL COLLATE NOCASE,
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("incidentId") REFERENCES "Incident"("id")		-- Foreign key to the Incident table.
);
-- Index on the IncidentNote table's tenantGuid field.
CREATE INDEX "I_IncidentNote_tenantGuid" ON "IncidentNote" ("tenantGuid")
;

-- Index on the IncidentNote table's tenantGuid,incidentId fields.
CREATE INDEX "I_IncidentNote_tenantGuid_incidentId" ON "IncidentNote" ("tenantGuid", "incidentId")
;

-- Index on the IncidentNote table's tenantGuid,active fields.
CREATE INDEX "I_IncidentNote_tenantGuid_active" ON "IncidentNote" ("tenantGuid", "active")
;

-- Index on the IncidentNote table's tenantGuid,deleted fields.
CREATE INDEX "I_IncidentNote_tenantGuid_deleted" ON "IncidentNote" ("tenantGuid", "deleted")
;


-- The change history for records from the IncidentNote table.
CREATE TABLE "IncidentNoteChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"incidentNoteId" INTEGER NOT NULL,		-- Link to the IncidentNote table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("incidentNoteId") REFERENCES "IncidentNote"("id")		-- Foreign key to the IncidentNote table.
);
-- Index on the IncidentNoteChangeHistory table's tenantGuid field.
CREATE INDEX "I_IncidentNoteChangeHistory_tenantGuid" ON "IncidentNoteChangeHistory" ("tenantGuid")
;

-- Index on the IncidentNoteChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_IncidentNoteChangeHistory_tenantGuid_versionNumber" ON "IncidentNoteChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the IncidentNoteChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_IncidentNoteChangeHistory_tenantGuid_timeStamp" ON "IncidentNoteChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the IncidentNoteChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_IncidentNoteChangeHistory_tenantGuid_userId" ON "IncidentNoteChangeHistory" ("tenantGuid", "userId")
;

-- Index on the IncidentNoteChangeHistory table's tenantGuid,incidentNoteId fields.
CREATE INDEX "I_IncidentNoteChangeHistory_tenantGuid_incidentNoteId" ON "IncidentNoteChangeHistory" ("tenantGuid", "incidentNoteId", "versionNumber", "timeStamp", "userId")
;


-- Notifications sent to individual users as part of escalation (teams/schedules are resolved to users at runtime).
CREATE TABLE "IncidentNotification"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"incidentId" INTEGER NOT NULL,		-- Link to the Incident table.
	"escalationRuleId" INTEGER NULL,		-- Link to the EscalationRule table.
	"userObjectGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- Resolved Security.SecurityUser.objectGuid that was notified.
	"firstNotifiedAt" DATETIME NOT NULL,
	"lastNotifiedAt" DATETIME NULL,
	"acknowledgedAt" DATETIME NULL,
	"acknowledgedByObjectGuid" VARCHAR(50) NULL COLLATE NOCASE,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("incidentId") REFERENCES "Incident"("id"),		-- Foreign key to the Incident table.
	FOREIGN KEY ("escalationRuleId") REFERENCES "EscalationRule"("id")		-- Foreign key to the EscalationRule table.
);
-- Index on the IncidentNotification table's tenantGuid field.
CREATE INDEX "I_IncidentNotification_tenantGuid" ON "IncidentNotification" ("tenantGuid")
;

-- Index on the IncidentNotification table's tenantGuid,incidentId fields.
CREATE INDEX "I_IncidentNotification_tenantGuid_incidentId" ON "IncidentNotification" ("tenantGuid", "incidentId")
;

-- Index on the IncidentNotification table's tenantGuid,escalationRuleId fields.
CREATE INDEX "I_IncidentNotification_tenantGuid_escalationRuleId" ON "IncidentNotification" ("tenantGuid", "escalationRuleId")
;

-- Index on the IncidentNotification table's tenantGuid,active fields.
CREATE INDEX "I_IncidentNotification_tenantGuid_active" ON "IncidentNotification" ("tenantGuid", "active")
;

-- Index on the IncidentNotification table's tenantGuid,deleted fields.
CREATE INDEX "I_IncidentNotification_tenantGuid_deleted" ON "IncidentNotification" ("tenantGuid", "deleted")
;

-- Index on the IncidentNotification table's incidentId,userObjectGuid fields.
CREATE INDEX "I_IncidentNotification_incidentId_userObjectGuid" ON "IncidentNotification" ("incidentId", "userObjectGuid")
;


-- Individual delivery attempts per channel for a notification.
CREATE TABLE "NotificationDeliveryAttempt"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"incidentNotificationId" INTEGER NOT NULL,		-- Link to the IncidentNotification table.
	"notificationChannelTypeId" INTEGER NOT NULL,		-- Link to the NotificationChannelType table.
	"attemptNumber" INTEGER NOT NULL DEFAULT 1,
	"attemptedAt" DATETIME NOT NULL,
	"status" VARCHAR(50) NOT NULL DEFAULT 'Pending' COLLATE NOCASE,
	"errorMessage" TEXT NULL COLLATE NOCASE,
	"response" TEXT NULL COLLATE NOCASE,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("incidentNotificationId") REFERENCES "IncidentNotification"("id"),		-- Foreign key to the IncidentNotification table.
	FOREIGN KEY ("notificationChannelTypeId") REFERENCES "NotificationChannelType"("id")		-- Foreign key to the NotificationChannelType table.
);
-- Index on the NotificationDeliveryAttempt table's tenantGuid field.
CREATE INDEX "I_NotificationDeliveryAttempt_tenantGuid" ON "NotificationDeliveryAttempt" ("tenantGuid")
;

-- Index on the NotificationDeliveryAttempt table's tenantGuid,incidentNotificationId fields.
CREATE INDEX "I_NtfctnDlvryttmpt_tnntGud_ncdntNtfctnd" ON "NotificationDeliveryAttempt" ("tenantGuid", "incidentNotificationId")
;

-- Index on the NotificationDeliveryAttempt table's tenantGuid,notificationChannelTypeId fields.
CREATE INDEX "I_NtfctnDlvryttmpt_tnntGud_ntfctnChnnlTypd" ON "NotificationDeliveryAttempt" ("tenantGuid", "notificationChannelTypeId")
;

-- Index on the NotificationDeliveryAttempt table's tenantGuid,active fields.
CREATE INDEX "I_NotificationDeliveryAttempt_tenantGuid_active" ON "NotificationDeliveryAttempt" ("tenantGuid", "active")
;

-- Index on the NotificationDeliveryAttempt table's tenantGuid,deleted fields.
CREATE INDEX "I_NotificationDeliveryAttempt_tenantGuid_deleted" ON "NotificationDeliveryAttempt" ("tenantGuid", "deleted")
;

-- Index on the NotificationDeliveryAttempt table's tenantGuid,incidentNotificationId,notificationChannelTypeId fields.
CREATE INDEX "I_NtfctnDlvryttmpt_tnntGud_ncdntNtfctnd_ntfctnChnnlTypd" ON "NotificationDeliveryAttempt" ("tenantGuid", "incidentNotificationId", "notificationChannelTypeId")
;


-- Outbound webhook delivery attempts for incident status updates.
CREATE TABLE "WebhookDeliveryAttempt"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"incidentId" INTEGER NOT NULL,		-- Link to the Incident table.
	"integrationId" INTEGER NOT NULL,		-- Link to the Integration table.
	"incidentTimelineEventId" INTEGER NULL,		-- Link to the IncidentTimelineEvent table.
	"attemptNumber" INTEGER NOT NULL DEFAULT 1,
	"attemptedAt" DATETIME NOT NULL,
	"httpStatusCode" INTEGER NULL,
	"success" BIT NOT NULL DEFAULT 0,
	"payloadJson" TEXT NULL COLLATE NOCASE,
	"responseBody" TEXT NULL COLLATE NOCASE,
	"errorMessage" TEXT NULL COLLATE NOCASE,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("incidentId") REFERENCES "Incident"("id"),		-- Foreign key to the Incident table.
	FOREIGN KEY ("integrationId") REFERENCES "Integration"("id"),		-- Foreign key to the Integration table.
	FOREIGN KEY ("incidentTimelineEventId") REFERENCES "IncidentTimelineEvent"("id")		-- Foreign key to the IncidentTimelineEvent table.
);
-- Index on the WebhookDeliveryAttempt table's tenantGuid field.
CREATE INDEX "I_WebhookDeliveryAttempt_tenantGuid" ON "WebhookDeliveryAttempt" ("tenantGuid")
;

-- Index on the WebhookDeliveryAttempt table's tenantGuid,incidentId fields.
CREATE INDEX "I_WebhookDeliveryAttempt_tenantGuid_incidentId" ON "WebhookDeliveryAttempt" ("tenantGuid", "incidentId")
;

-- Index on the WebhookDeliveryAttempt table's tenantGuid,integrationId fields.
CREATE INDEX "I_WebhookDeliveryAttempt_tenantGuid_integrationId" ON "WebhookDeliveryAttempt" ("tenantGuid", "integrationId")
;

-- Index on the WebhookDeliveryAttempt table's tenantGuid,incidentTimelineEventId fields.
CREATE INDEX "I_WebhookDeliveryAttempt_tenantGuid_incidentTimelineEventId" ON "WebhookDeliveryAttempt" ("tenantGuid", "incidentTimelineEventId")
;

-- Index on the WebhookDeliveryAttempt table's tenantGuid,active fields.
CREATE INDEX "I_WebhookDeliveryAttempt_tenantGuid_active" ON "WebhookDeliveryAttempt" ("tenantGuid", "active")
;

-- Index on the WebhookDeliveryAttempt table's tenantGuid,deleted fields.
CREATE INDEX "I_WebhookDeliveryAttempt_tenantGuid_deleted" ON "WebhookDeliveryAttempt" ("tenantGuid", "deleted")
;

-- Index on the WebhookDeliveryAttempt table's tenantGuid,incidentId,attemptedAt fields.
CREATE INDEX "I_WebhookDeliveryAttempt_tenantGuid_incidentId_attemptedAt" ON "WebhookDeliveryAttempt" ("tenantGuid", "incidentId", "attemptedAt")
;


