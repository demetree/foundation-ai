/*
Alerting and Incident Management database.
Stores services, escalation policies, on-call schedules, incidents, timeline events,
notification delivery tracking, webhook delivery tracking, and integration credentials.
Designed to be independent while sharing the central Security database for users and teams.
*/
CREATE DATABASE "Alerting"
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    TEMPLATE=template0;

\connect "Alerting"   -- Run the create database first, then connect to it, and run the rest if running in a query tool rather than through psql scripting.



CREATE SCHEMA "Alerting"

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE "Alerting"."WebhookDeliveryAttempt"
-- DROP TABLE "Alerting"."NotificationDeliveryAttempt"
-- DROP TABLE "Alerting"."IncidentNotification"
-- DROP TABLE "Alerting"."IncidentNoteChangeHistory"
-- DROP TABLE "Alerting"."IncidentNote"
-- DROP TABLE "Alerting"."IncidentTimelineEvent"
-- DROP TABLE "Alerting"."IncidentChangeHistory"
-- DROP TABLE "Alerting"."Incident"
-- DROP TABLE "Alerting"."UserPushTokenChangeHistory"
-- DROP TABLE "Alerting"."UserPushToken"
-- DROP TABLE "Alerting"."UserNotificationChannelPreferenceChangeHistory"
-- DROP TABLE "Alerting"."UserNotificationChannelPreference"
-- DROP TABLE "Alerting"."UserNotificationPreferenceChangeHistory"
-- DROP TABLE "Alerting"."UserNotificationPreference"
-- DROP TABLE "Alerting"."IntegrationCallbackIncidentEventTypeChangeHistory"
-- DROP TABLE "Alerting"."IntegrationCallbackIncidentEventType"
-- DROP TABLE "Alerting"."IntegrationChangeHistory"
-- DROP TABLE "Alerting"."Integration"
-- DROP TABLE "Alerting"."ScheduleOverrideChangeHistory"
-- DROP TABLE "Alerting"."ScheduleOverride"
-- DROP TABLE "Alerting"."ScheduleLayerMemberChangeHistory"
-- DROP TABLE "Alerting"."ScheduleLayerMember"
-- DROP TABLE "Alerting"."ScheduleLayerChangeHistory"
-- DROP TABLE "Alerting"."ScheduleLayer"
-- DROP TABLE "Alerting"."OnCallScheduleChangeHistory"
-- DROP TABLE "Alerting"."OnCallSchedule"
-- DROP TABLE "Alerting"."EscalationRuleChangeHistory"
-- DROP TABLE "Alerting"."EscalationRule"
-- DROP TABLE "Alerting"."ServiceChangeHistory"
-- DROP TABLE "Alerting"."Service"
-- DROP TABLE "Alerting"."EscalationPolicyChangeHistory"
-- DROP TABLE "Alerting"."EscalationPolicy"
-- DROP TABLE "Alerting"."ScheduleOverrideType"
-- DROP TABLE "Alerting"."NotificationChannelType"
-- DROP TABLE "Alerting"."IncidentEventType"
-- DROP TABLE "Alerting"."IncidentStatusType"
-- DROP TABLE "Alerting"."SeverityType"

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
CREATE TABLE "Alerting"."SeverityType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NULL,
	"sequence" INT NOT NULL,		-- Sequence to use for sorting.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the SeverityType table's name field.
CREATE INDEX "I_SeverityType_name" ON "Alerting"."SeverityType" ("name")
;

-- Index on the SeverityType table's active field.
CREATE INDEX "I_SeverityType_active" ON "Alerting"."SeverityType" ("active")
;

-- Index on the SeverityType table's deleted field.
CREATE INDEX "I_SeverityType_deleted" ON "Alerting"."SeverityType" ("deleted")
;

INSERT INTO "Alerting"."SeverityType" ( "name", "description", "sequence" ) VALUES  ( 'Critical', 'Critical', 10 );

INSERT INTO "Alerting"."SeverityType" ( "name", "description", "sequence" ) VALUES  ( 'High', 'High', 20 );

INSERT INTO "Alerting"."SeverityType" ( "name", "description", "sequence" ) VALUES  ( 'Medium', 'Medium', 30 );

INSERT INTO "Alerting"."SeverityType" ( "name", "description", "sequence" ) VALUES  ( 'Low', 'Low', 40 );


-- Static status values for incidents.
CREATE TABLE "Alerting"."IncidentStatusType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NULL,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the IncidentStatusType table's name field.
CREATE INDEX "I_IncidentStatusType_name" ON "Alerting"."IncidentStatusType" ("name")
;

-- Index on the IncidentStatusType table's active field.
CREATE INDEX "I_IncidentStatusType_active" ON "Alerting"."IncidentStatusType" ("active")
;

-- Index on the IncidentStatusType table's deleted field.
CREATE INDEX "I_IncidentStatusType_deleted" ON "Alerting"."IncidentStatusType" ("deleted")
;

INSERT INTO "Alerting"."IncidentStatusType" ( "name", "description" ) VALUES  ( 'Triggered', 'Newly triggered incident' );

INSERT INTO "Alerting"."IncidentStatusType" ( "name", "description" ) VALUES  ( 'Acknowledged', 'Acknowledged by a responder' );

INSERT INTO "Alerting"."IncidentStatusType" ( "name", "description" ) VALUES  ( 'Resolved', 'Incident resolved' );


-- Static event types for the incident timeline.
CREATE TABLE "Alerting"."IncidentEventType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NULL,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the IncidentEventType table's name field.
CREATE INDEX "I_IncidentEventType_name" ON "Alerting"."IncidentEventType" ("name")
;

-- Index on the IncidentEventType table's active field.
CREATE INDEX "I_IncidentEventType_active" ON "Alerting"."IncidentEventType" ("active")
;

-- Index on the IncidentEventType table's deleted field.
CREATE INDEX "I_IncidentEventType_deleted" ON "Alerting"."IncidentEventType" ("deleted")
;

INSERT INTO "Alerting"."IncidentEventType" ( "name", "description" ) VALUES  ( 'Triggered', 'Incident was triggered' );

INSERT INTO "Alerting"."IncidentEventType" ( "name", "description" ) VALUES  ( 'Escalated', 'Escalation rule fired' );

INSERT INTO "Alerting"."IncidentEventType" ( "name", "description" ) VALUES  ( 'Acknowledged', 'Incident acknowledged' );

INSERT INTO "Alerting"."IncidentEventType" ( "name", "description" ) VALUES  ( 'Resolved', 'Incident resolved' );

INSERT INTO "Alerting"."IncidentEventType" ( "name", "description" ) VALUES  ( 'NoteAdded', 'Note added to incident' );

INSERT INTO "Alerting"."IncidentEventType" ( "name", "description" ) VALUES  ( 'NotificationSent', 'Notification delivery attempted' );


-- Static notification delivery channels.
CREATE TABLE "Alerting"."NotificationChannelType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NULL,
	"defaultPriority" INT NOT NULL DEFAULT 0,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the NotificationChannelType table's name field.
CREATE INDEX "I_NotificationChannelType_name" ON "Alerting"."NotificationChannelType" ("name")
;

-- Index on the NotificationChannelType table's active field.
CREATE INDEX "I_NotificationChannelType_active" ON "Alerting"."NotificationChannelType" ("active")
;

-- Index on the NotificationChannelType table's deleted field.
CREATE INDEX "I_NotificationChannelType_deleted" ON "Alerting"."NotificationChannelType" ("deleted")
;

INSERT INTO "Alerting"."NotificationChannelType" ( "name", "description", "defaultPriority" ) VALUES  ( 'Email', 'Email notification', 30 );

INSERT INTO "Alerting"."NotificationChannelType" ( "name", "description", "defaultPriority" ) VALUES  ( 'SMS', 'SMS text message', 10 );

INSERT INTO "Alerting"."NotificationChannelType" ( "name", "description", "defaultPriority" ) VALUES  ( 'VoiceCall', 'Automated voice call', 5 );

INSERT INTO "Alerting"."NotificationChannelType" ( "name", "description", "defaultPriority" ) VALUES  ( 'MobilePush', 'Mobile app push', 20 );

INSERT INTO "Alerting"."NotificationChannelType" ( "name", "description", "defaultPriority" ) VALUES  ( 'WebPush', 'Browser push notification', 25 );

INSERT INTO "Alerting"."NotificationChannelType" ( "name", "description", "defaultPriority" ) VALUES  ( 'Teams', 'Microsoft Teams message', 40 );


-- Static schedule override types.
CREATE TABLE "Alerting"."ScheduleOverrideType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NULL,
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the ScheduleOverrideType table's name field.
CREATE INDEX "I_ScheduleOverrideType_name" ON "Alerting"."ScheduleOverrideType" ("name")
;

-- Index on the ScheduleOverrideType table's active field.
CREATE INDEX "I_ScheduleOverrideType_active" ON "Alerting"."ScheduleOverrideType" ("active")
;

-- Index on the ScheduleOverrideType table's deleted field.
CREATE INDEX "I_ScheduleOverrideType_deleted" ON "Alerting"."ScheduleOverrideType" ("deleted")
;

INSERT INTO "Alerting"."ScheduleOverrideType" ( "name", "description" ) VALUES  ( 'Swap', 'Swap - Two users exchange shifts' );

INSERT INTO "Alerting"."ScheduleOverrideType" ( "name", "description" ) VALUES  ( 'Replace', 'Replace - One user temporarily takes over for another' );

INSERT INTO "Alerting"."ScheduleOverrideType" ( "name", "description" ) VALUES  ( 'Remove', 'Remove - User taken off the schedule with no replacement' );


-- Escalation policies assigned to services.
CREATE TABLE "Alerting"."EscalationPolicy"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_EscalationPolicy_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the EscalationPolicy table's tenantGuid and name fields.
);
-- Index on the EscalationPolicy table's tenantGuid field.
CREATE INDEX "I_EscalationPolicy_tenantGuid" ON "Alerting"."EscalationPolicy" ("tenantGuid")
;

-- Index on the EscalationPolicy table's tenantGuid,name fields.
CREATE INDEX "I_EscalationPolicy_tenantGuid_name" ON "Alerting"."EscalationPolicy" ("tenantGuid", "name")
;

-- Index on the EscalationPolicy table's tenantGuid,active fields.
CREATE INDEX "I_EscalationPolicy_tenantGuid_active" ON "Alerting"."EscalationPolicy" ("tenantGuid", "active")
;

-- Index on the EscalationPolicy table's tenantGuid,deleted fields.
CREATE INDEX "I_EscalationPolicy_tenantGuid_deleted" ON "Alerting"."EscalationPolicy" ("tenantGuid", "deleted")
;


-- The change history for records from the EscalationPolicy table.
CREATE TABLE "Alerting"."EscalationPolicyChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"escalationPolicyId" INT NOT NULL,		-- Link to the EscalationPolicy table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "escalationPolicyId" FOREIGN KEY ("escalationPolicyId") REFERENCES "Alerting"."EscalationPolicy"("id")		-- Foreign key to the EscalationPolicy table.
);
-- Index on the EscalationPolicyChangeHistory table's tenantGuid field.
CREATE INDEX "I_EscalationPolicyChangeHistory_tenantGuid" ON "Alerting"."EscalationPolicyChangeHistory" ("tenantGuid")
;

-- Index on the EscalationPolicyChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_EscalationPolicyChangeHistory_tenantGuid_versionNumber" ON "Alerting"."EscalationPolicyChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the EscalationPolicyChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_EscalationPolicyChangeHistory_tenantGuid_timeStamp" ON "Alerting"."EscalationPolicyChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the EscalationPolicyChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_EscalationPolicyChangeHistory_tenantGuid_userId" ON "Alerting"."EscalationPolicyChangeHistory" ("tenantGuid", "userId")
;

-- Index on the EscalationPolicyChangeHistory table's tenantGuid,escalationPolicyId fields.
CREATE INDEX "I_EscalationPolicyChangeHistory_tenantGuid_escalationPolicyId" ON "Alerting"."EscalationPolicyChangeHistory" ("tenantGuid", "escalationPolicyId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Monitored services/applications that can generate alerts.
CREATE TABLE "Alerting"."Service"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"escalationPolicyId" INT NULL,		-- Link to the EscalationPolicy table.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"ownerTeamObjectGuid" VARCHAR(50) NULL,		-- References Security.SecurityTeam.objectGuid - logical owner.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "escalationPolicyId" FOREIGN KEY ("escalationPolicyId") REFERENCES "Alerting"."EscalationPolicy"("id"),		-- Foreign key to the EscalationPolicy table.
	CONSTRAINT "UC_Service_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Service table's tenantGuid and name fields.
);
-- Index on the Service table's tenantGuid field.
CREATE INDEX "I_Service_tenantGuid" ON "Alerting"."Service" ("tenantGuid")
;

-- Index on the Service table's tenantGuid,escalationPolicyId fields.
CREATE INDEX "I_Service_tenantGuid_escalationPolicyId" ON "Alerting"."Service" ("tenantGuid", "escalationPolicyId")
;

-- Index on the Service table's tenantGuid,name fields.
CREATE INDEX "I_Service_tenantGuid_name" ON "Alerting"."Service" ("tenantGuid", "name")
;

-- Index on the Service table's tenantGuid,active fields.
CREATE INDEX "I_Service_tenantGuid_active" ON "Alerting"."Service" ("tenantGuid", "active")
;

-- Index on the Service table's tenantGuid,deleted fields.
CREATE INDEX "I_Service_tenantGuid_deleted" ON "Alerting"."Service" ("tenantGuid", "deleted")
;


-- The change history for records from the Service table.
CREATE TABLE "Alerting"."ServiceChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"serviceId" INT NOT NULL,		-- Link to the Service table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "serviceId" FOREIGN KEY ("serviceId") REFERENCES "Alerting"."Service"("id")		-- Foreign key to the Service table.
);
-- Index on the ServiceChangeHistory table's tenantGuid field.
CREATE INDEX "I_ServiceChangeHistory_tenantGuid" ON "Alerting"."ServiceChangeHistory" ("tenantGuid")
;

-- Index on the ServiceChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ServiceChangeHistory_tenantGuid_versionNumber" ON "Alerting"."ServiceChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ServiceChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ServiceChangeHistory_tenantGuid_timeStamp" ON "Alerting"."ServiceChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ServiceChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ServiceChangeHistory_tenantGuid_userId" ON "Alerting"."ServiceChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ServiceChangeHistory table's tenantGuid,serviceId fields.
CREATE INDEX "I_ServiceChangeHistory_tenantGuid_serviceId" ON "Alerting"."ServiceChangeHistory" ("tenantGuid", "serviceId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Individual escalation rules (ordered). Supports repeat looping until acknowledgment.
CREATE TABLE "Alerting"."EscalationRule"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"escalationPolicyId" INT NOT NULL,		-- Link to the EscalationPolicy table.
	"ruleOrder" INT NOT NULL DEFAULT 0,
	"delayMinutes" INT NOT NULL DEFAULT 0,
	"repeatCount" INT NOT NULL DEFAULT 0,		-- How many times to repeat notification if no ack (0 = no repeat).
	"repeatDelayMinutes" INT NULL,		-- Delay between repeat attempts (null = same as delayMinutes).
	"targetType" VARCHAR(50) NOT NULL,		-- Valid values: User, Team, Schedule
	"targetObjectGuid" VARCHAR(50) NULL,		-- References Security.SecurityUser/SecurityTeam or Alerting.OnCallSchedule objectGuid.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "escalationPolicyId" FOREIGN KEY ("escalationPolicyId") REFERENCES "Alerting"."EscalationPolicy"("id")		-- Foreign key to the EscalationPolicy table.
);
-- Index on the EscalationRule table's tenantGuid field.
CREATE INDEX "I_EscalationRule_tenantGuid" ON "Alerting"."EscalationRule" ("tenantGuid")
;

-- Index on the EscalationRule table's tenantGuid,escalationPolicyId fields.
CREATE INDEX "I_EscalationRule_tenantGuid_escalationPolicyId" ON "Alerting"."EscalationRule" ("tenantGuid", "escalationPolicyId")
;

-- Index on the EscalationRule table's tenantGuid,ruleOrder fields.
CREATE INDEX "I_EscalationRule_tenantGuid_ruleOrder" ON "Alerting"."EscalationRule" ("tenantGuid", "ruleOrder")
;

-- Index on the EscalationRule table's tenantGuid,active fields.
CREATE INDEX "I_EscalationRule_tenantGuid_active" ON "Alerting"."EscalationRule" ("tenantGuid", "active")
;

-- Index on the EscalationRule table's tenantGuid,deleted fields.
CREATE INDEX "I_EscalationRule_tenantGuid_deleted" ON "Alerting"."EscalationRule" ("tenantGuid", "deleted")
;

-- Index on the EscalationRule table's escalationPolicyId,ruleOrder fields.
CREATE INDEX "I_EscalationRule_escalationPolicyId_ruleOrder" ON "Alerting"."EscalationRule" ("escalationPolicyId", "ruleOrder")
;


-- The change history for records from the EscalationRule table.
CREATE TABLE "Alerting"."EscalationRuleChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"escalationRuleId" INT NOT NULL,		-- Link to the EscalationRule table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "escalationRuleId" FOREIGN KEY ("escalationRuleId") REFERENCES "Alerting"."EscalationRule"("id")		-- Foreign key to the EscalationRule table.
);
-- Index on the EscalationRuleChangeHistory table's tenantGuid field.
CREATE INDEX "I_EscalationRuleChangeHistory_tenantGuid" ON "Alerting"."EscalationRuleChangeHistory" ("tenantGuid")
;

-- Index on the EscalationRuleChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_EscalationRuleChangeHistory_tenantGuid_versionNumber" ON "Alerting"."EscalationRuleChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the EscalationRuleChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_EscalationRuleChangeHistory_tenantGuid_timeStamp" ON "Alerting"."EscalationRuleChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the EscalationRuleChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_EscalationRuleChangeHistory_tenantGuid_userId" ON "Alerting"."EscalationRuleChangeHistory" ("tenantGuid", "userId")
;

-- Index on the EscalationRuleChangeHistory table's tenantGuid,escalationRuleId fields.
CREATE INDEX "I_EscalationRuleChangeHistory_tenantGuid_escalationRuleId" ON "Alerting"."EscalationRuleChangeHistory" ("tenantGuid", "escalationRuleId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- On-call rotation schedules (dynamic targets for escalation rules).
CREATE TABLE "Alerting"."OnCallSchedule"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"timeZoneId" VARCHAR(50) NOT NULL DEFAULT 'UTC',
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_OnCallSchedule_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the OnCallSchedule table's tenantGuid and name fields.
);
-- Index on the OnCallSchedule table's tenantGuid field.
CREATE INDEX "I_OnCallSchedule_tenantGuid" ON "Alerting"."OnCallSchedule" ("tenantGuid")
;

-- Index on the OnCallSchedule table's tenantGuid,name fields.
CREATE INDEX "I_OnCallSchedule_tenantGuid_name" ON "Alerting"."OnCallSchedule" ("tenantGuid", "name")
;

-- Index on the OnCallSchedule table's tenantGuid,active fields.
CREATE INDEX "I_OnCallSchedule_tenantGuid_active" ON "Alerting"."OnCallSchedule" ("tenantGuid", "active")
;

-- Index on the OnCallSchedule table's tenantGuid,deleted fields.
CREATE INDEX "I_OnCallSchedule_tenantGuid_deleted" ON "Alerting"."OnCallSchedule" ("tenantGuid", "deleted")
;


-- The change history for records from the OnCallSchedule table.
CREATE TABLE "Alerting"."OnCallScheduleChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"onCallScheduleId" INT NOT NULL,		-- Link to the OnCallSchedule table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "onCallScheduleId" FOREIGN KEY ("onCallScheduleId") REFERENCES "Alerting"."OnCallSchedule"("id")		-- Foreign key to the OnCallSchedule table.
);
-- Index on the OnCallScheduleChangeHistory table's tenantGuid field.
CREATE INDEX "I_OnCallScheduleChangeHistory_tenantGuid" ON "Alerting"."OnCallScheduleChangeHistory" ("tenantGuid")
;

-- Index on the OnCallScheduleChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_OnCallScheduleChangeHistory_tenantGuid_versionNumber" ON "Alerting"."OnCallScheduleChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the OnCallScheduleChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_OnCallScheduleChangeHistory_tenantGuid_timeStamp" ON "Alerting"."OnCallScheduleChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the OnCallScheduleChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_OnCallScheduleChangeHistory_tenantGuid_userId" ON "Alerting"."OnCallScheduleChangeHistory" ("tenantGuid", "userId")
;

-- Index on the OnCallScheduleChangeHistory table's tenantGuid,onCallScheduleId fields.
CREATE INDEX "I_OnCallScheduleChangeHistory_tenantGuid_onCallScheduleId" ON "Alerting"."OnCallScheduleChangeHistory" ("tenantGuid", "onCallScheduleId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Layers within an on-call schedule (primary, secondary, etc.).
CREATE TABLE "Alerting"."ScheduleLayer"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"onCallScheduleId" INT NOT NULL,		-- Link to the OnCallSchedule table.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"layerLevel" INT NOT NULL DEFAULT 1,
	"rotationStart" TIMESTAMP NOT NULL,
	"rotationDays" INT NOT NULL DEFAULT 7,
	"handoffTime" VARCHAR(50) NOT NULL DEFAULT '09:00',
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "onCallScheduleId" FOREIGN KEY ("onCallScheduleId") REFERENCES "Alerting"."OnCallSchedule"("id"),		-- Foreign key to the OnCallSchedule table.
	CONSTRAINT "UC_ScheduleLayer_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the ScheduleLayer table's tenantGuid and name fields.
);
-- Index on the ScheduleLayer table's tenantGuid field.
CREATE INDEX "I_ScheduleLayer_tenantGuid" ON "Alerting"."ScheduleLayer" ("tenantGuid")
;

-- Index on the ScheduleLayer table's tenantGuid,onCallScheduleId fields.
CREATE INDEX "I_ScheduleLayer_tenantGuid_onCallScheduleId" ON "Alerting"."ScheduleLayer" ("tenantGuid", "onCallScheduleId")
;

-- Index on the ScheduleLayer table's tenantGuid,name fields.
CREATE INDEX "I_ScheduleLayer_tenantGuid_name" ON "Alerting"."ScheduleLayer" ("tenantGuid", "name")
;

-- Index on the ScheduleLayer table's tenantGuid,active fields.
CREATE INDEX "I_ScheduleLayer_tenantGuid_active" ON "Alerting"."ScheduleLayer" ("tenantGuid", "active")
;

-- Index on the ScheduleLayer table's tenantGuid,deleted fields.
CREATE INDEX "I_ScheduleLayer_tenantGuid_deleted" ON "Alerting"."ScheduleLayer" ("tenantGuid", "deleted")
;


-- The change history for records from the ScheduleLayer table.
CREATE TABLE "Alerting"."ScheduleLayerChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduleLayerId" INT NOT NULL,		-- Link to the ScheduleLayer table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "scheduleLayerId" FOREIGN KEY ("scheduleLayerId") REFERENCES "Alerting"."ScheduleLayer"("id")		-- Foreign key to the ScheduleLayer table.
);
-- Index on the ScheduleLayerChangeHistory table's tenantGuid field.
CREATE INDEX "I_ScheduleLayerChangeHistory_tenantGuid" ON "Alerting"."ScheduleLayerChangeHistory" ("tenantGuid")
;

-- Index on the ScheduleLayerChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ScheduleLayerChangeHistory_tenantGuid_versionNumber" ON "Alerting"."ScheduleLayerChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ScheduleLayerChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ScheduleLayerChangeHistory_tenantGuid_timeStamp" ON "Alerting"."ScheduleLayerChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ScheduleLayerChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ScheduleLayerChangeHistory_tenantGuid_userId" ON "Alerting"."ScheduleLayerChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ScheduleLayerChangeHistory table's tenantGuid,scheduleLayerId fields.
CREATE INDEX "I_ScheduleLayerChangeHistory_tenantGuid_scheduleLayerId" ON "Alerting"."ScheduleLayerChangeHistory" ("tenantGuid", "scheduleLayerId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Users in a schedule layer rotation (ordered).
CREATE TABLE "Alerting"."ScheduleLayerMember"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduleLayerId" INT NOT NULL,		-- Link to the ScheduleLayer table.
	"position" INT NOT NULL DEFAULT 0,
	"securityUserObjectGuid" VARCHAR(50) NOT NULL,		-- References Security.SecurityUser.objectGuid
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "scheduleLayerId" FOREIGN KEY ("scheduleLayerId") REFERENCES "Alerting"."ScheduleLayer"("id"),		-- Foreign key to the ScheduleLayer table.
	CONSTRAINT "UC_ScheduleLayerMember_tenantGuid_scheduleLayerId_position" UNIQUE ( "tenantGuid", "scheduleLayerId", "position") 		-- Uniqueness enforced on the ScheduleLayerMember table's tenantGuid and scheduleLayerId and position fields.
);
-- Index on the ScheduleLayerMember table's tenantGuid field.
CREATE INDEX "I_ScheduleLayerMember_tenantGuid" ON "Alerting"."ScheduleLayerMember" ("tenantGuid")
;

-- Index on the ScheduleLayerMember table's tenantGuid,scheduleLayerId fields.
CREATE INDEX "I_ScheduleLayerMember_tenantGuid_scheduleLayerId" ON "Alerting"."ScheduleLayerMember" ("tenantGuid", "scheduleLayerId")
;

-- Index on the ScheduleLayerMember table's tenantGuid,active fields.
CREATE INDEX "I_ScheduleLayerMember_tenantGuid_active" ON "Alerting"."ScheduleLayerMember" ("tenantGuid", "active")
;

-- Index on the ScheduleLayerMember table's tenantGuid,deleted fields.
CREATE INDEX "I_ScheduleLayerMember_tenantGuid_deleted" ON "Alerting"."ScheduleLayerMember" ("tenantGuid", "deleted")
;


-- The change history for records from the ScheduleLayerMember table.
CREATE TABLE "Alerting"."ScheduleLayerMemberChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduleLayerMemberId" INT NOT NULL,		-- Link to the ScheduleLayerMember table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "scheduleLayerMemberId" FOREIGN KEY ("scheduleLayerMemberId") REFERENCES "Alerting"."ScheduleLayerMember"("id")		-- Foreign key to the ScheduleLayerMember table.
);
-- Index on the ScheduleLayerMemberChangeHistory table's tenantGuid field.
CREATE INDEX "I_ScheduleLayerMemberChangeHistory_tenantGuid" ON "Alerting"."ScheduleLayerMemberChangeHistory" ("tenantGuid")
;

-- Index on the ScheduleLayerMemberChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ScheduleLayerMemberChangeHistory_tenantGuid_versionNumber" ON "Alerting"."ScheduleLayerMemberChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ScheduleLayerMemberChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ScheduleLayerMemberChangeHistory_tenantGuid_timeStamp" ON "Alerting"."ScheduleLayerMemberChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ScheduleLayerMemberChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ScheduleLayerMemberChangeHistory_tenantGuid_userId" ON "Alerting"."ScheduleLayerMemberChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ScheduleLayerMemberChangeHistory table's tenantGuid,scheduleLayerMemberId fields.
CREATE INDEX "I_ScheduleLayerMemberChangeHistory_tenantGuid_scheduleLayerMemb" ON "Alerting"."ScheduleLayerMemberChangeHistory" ("tenantGuid", "scheduleLayerMemberId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Temporary overrides for on-call schedules (vacations, swaps, emergency substitutions).
CREATE TABLE "Alerting"."ScheduleOverride"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"onCallScheduleId" INT NOT NULL,		-- Link to the OnCallSchedule table.
	"scheduleLayerId" INT NULL,		-- If null, override applies to all layers in the schedule.
	"startDateTime" TIMESTAMP NOT NULL,		-- Start of override period (inclusive).
	"endDateTime" TIMESTAMP NOT NULL,		-- End of override period (exclusive).
	"scheduleOverrideTypeId" INT NOT NULL,		-- The type of override.  Will be one of Swap, Replace, or Remove
	"originalUserObjectGuid" VARCHAR(50) NULL,		-- The user being replaced (null for layer-wide overrides).
	"replacementUserObjectGuid" VARCHAR(50) NULL,		-- The substitute user (null for REMOVE type).
	"reason" VARCHAR(500) NULL,		-- Optional explanation (vacation, sick, training, etc.).
	"createdByUserObjectGuid" VARCHAR(50) NOT NULL,		-- References Security.SecurityUser.objectGuid - who created the override.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "onCallScheduleId" FOREIGN KEY ("onCallScheduleId") REFERENCES "Alerting"."OnCallSchedule"("id"),		-- Foreign key to the OnCallSchedule table.
	CONSTRAINT "scheduleLayerId" FOREIGN KEY ("scheduleLayerId") REFERENCES "Alerting"."ScheduleLayer"("id"),		-- Foreign key to the ScheduleLayer table.
	CONSTRAINT "scheduleOverrideTypeId" FOREIGN KEY ("scheduleOverrideTypeId") REFERENCES "Alerting"."ScheduleOverrideType"("id")		-- Foreign key to the ScheduleOverrideType table.
);
-- Index on the ScheduleOverride table's tenantGuid field.
CREATE INDEX "I_ScheduleOverride_tenantGuid" ON "Alerting"."ScheduleOverride" ("tenantGuid")
;

-- Index on the ScheduleOverride table's tenantGuid,onCallScheduleId fields.
CREATE INDEX "I_ScheduleOverride_tenantGuid_onCallScheduleId" ON "Alerting"."ScheduleOverride" ("tenantGuid", "onCallScheduleId")
;

-- Index on the ScheduleOverride table's tenantGuid,scheduleLayerId fields.
CREATE INDEX "I_ScheduleOverride_tenantGuid_scheduleLayerId" ON "Alerting"."ScheduleOverride" ("tenantGuid", "scheduleLayerId")
;

-- Index on the ScheduleOverride table's tenantGuid,scheduleOverrideTypeId fields.
CREATE INDEX "I_ScheduleOverride_tenantGuid_scheduleOverrideTypeId" ON "Alerting"."ScheduleOverride" ("tenantGuid", "scheduleOverrideTypeId")
;

-- Index on the ScheduleOverride table's tenantGuid,active fields.
CREATE INDEX "I_ScheduleOverride_tenantGuid_active" ON "Alerting"."ScheduleOverride" ("tenantGuid", "active")
;

-- Index on the ScheduleOverride table's tenantGuid,deleted fields.
CREATE INDEX "I_ScheduleOverride_tenantGuid_deleted" ON "Alerting"."ScheduleOverride" ("tenantGuid", "deleted")
;

-- Index on the ScheduleOverride table's tenantGuid,onCallScheduleId,startDateTime,endDateTime fields.
CREATE INDEX "I_ScheduleOverride_tenantGuid_onCallScheduleId_startDateTime_en" ON "Alerting"."ScheduleOverride" ("tenantGuid", "onCallScheduleId", "startDateTime", "endDateTime")
;

-- Index on the ScheduleOverride table's tenantGuid,originalUserObjectGuid fields.
CREATE INDEX "I_ScheduleOverride_tenantGuid_originalUserObjectGuid" ON "Alerting"."ScheduleOverride" ("tenantGuid", "originalUserObjectGuid")
;


-- The change history for records from the ScheduleOverride table.
CREATE TABLE "Alerting"."ScheduleOverrideChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"scheduleOverrideId" INT NOT NULL,		-- Link to the ScheduleOverride table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "scheduleOverrideId" FOREIGN KEY ("scheduleOverrideId") REFERENCES "Alerting"."ScheduleOverride"("id")		-- Foreign key to the ScheduleOverride table.
);
-- Index on the ScheduleOverrideChangeHistory table's tenantGuid field.
CREATE INDEX "I_ScheduleOverrideChangeHistory_tenantGuid" ON "Alerting"."ScheduleOverrideChangeHistory" ("tenantGuid")
;

-- Index on the ScheduleOverrideChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ScheduleOverrideChangeHistory_tenantGuid_versionNumber" ON "Alerting"."ScheduleOverrideChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ScheduleOverrideChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ScheduleOverrideChangeHistory_tenantGuid_timeStamp" ON "Alerting"."ScheduleOverrideChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ScheduleOverrideChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ScheduleOverrideChangeHistory_tenantGuid_userId" ON "Alerting"."ScheduleOverrideChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ScheduleOverrideChangeHistory table's tenantGuid,scheduleOverrideId fields.
CREATE INDEX "I_ScheduleOverrideChangeHistory_tenantGuid_scheduleOverrideId" ON "Alerting"."ScheduleOverrideChangeHistory" ("tenantGuid", "scheduleOverrideId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- API integrations for inbound alerts and outbound status callbacks.
CREATE TABLE "Alerting"."Integration"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"serviceId" INT NOT NULL,		-- Link to the Service table.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NULL,
	"apiKeyHash" VARCHAR(250) NOT NULL UNIQUE,
	"callbackWebhookUrl" VARCHAR(1000) NULL,
	"maxRetryAttempts" INT NULL DEFAULT 10,		-- How many times to retry failed deliveries
	"retryBackoffSeconds" INT NULL DEFAULT 30,		-- Base seconds for backoff (30, 60, 120, 240...)
	"lastCallbackSuccessAt" TIMESTAMP NULL,
	"consecutiveCallbackFailures" INT NULL,
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "serviceId" FOREIGN KEY ("serviceId") REFERENCES "Alerting"."Service"("id"),		-- Foreign key to the Service table.
	CONSTRAINT "UC_Integration_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Integration table's tenantGuid and name fields.
);
-- Index on the Integration table's tenantGuid field.
CREATE INDEX "I_Integration_tenantGuid" ON "Alerting"."Integration" ("tenantGuid")
;

-- Index on the Integration table's tenantGuid,serviceId fields.
CREATE INDEX "I_Integration_tenantGuid_serviceId" ON "Alerting"."Integration" ("tenantGuid", "serviceId")
;

-- Index on the Integration table's tenantGuid,name fields.
CREATE INDEX "I_Integration_tenantGuid_name" ON "Alerting"."Integration" ("tenantGuid", "name")
;

-- Index on the Integration table's tenantGuid,active fields.
CREATE INDEX "I_Integration_tenantGuid_active" ON "Alerting"."Integration" ("tenantGuid", "active")
;

-- Index on the Integration table's tenantGuid,deleted fields.
CREATE INDEX "I_Integration_tenantGuid_deleted" ON "Alerting"."Integration" ("tenantGuid", "deleted")
;


-- The change history for records from the Integration table.
CREATE TABLE "Alerting"."IntegrationChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"integrationId" INT NOT NULL,		-- Link to the Integration table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "integrationId" FOREIGN KEY ("integrationId") REFERENCES "Alerting"."Integration"("id")		-- Foreign key to the Integration table.
);
-- Index on the IntegrationChangeHistory table's tenantGuid field.
CREATE INDEX "I_IntegrationChangeHistory_tenantGuid" ON "Alerting"."IntegrationChangeHistory" ("tenantGuid")
;

-- Index on the IntegrationChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_IntegrationChangeHistory_tenantGuid_versionNumber" ON "Alerting"."IntegrationChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the IntegrationChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_IntegrationChangeHistory_tenantGuid_timeStamp" ON "Alerting"."IntegrationChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the IntegrationChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_IntegrationChangeHistory_tenantGuid_userId" ON "Alerting"."IntegrationChangeHistory" ("tenantGuid", "userId")
;

-- Index on the IntegrationChangeHistory table's tenantGuid,integrationId fields.
CREATE INDEX "I_IntegrationChangeHistory_tenantGuid_integrationId" ON "Alerting"."IntegrationChangeHistory" ("tenantGuid", "integrationId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- API integrations incident event types to callback on.
CREATE TABLE "Alerting"."IntegrationCallbackIncidentEventType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"integrationId" INT NOT NULL,		-- Link to the Integration table.
	"incidentEventTypeId" INT NOT NULL,		-- Link to the IncidentEventType table.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "integrationId" FOREIGN KEY ("integrationId") REFERENCES "Alerting"."Integration"("id"),		-- Foreign key to the Integration table.
	CONSTRAINT "incidentEventTypeId" FOREIGN KEY ("incidentEventTypeId") REFERENCES "Alerting"."IncidentEventType"("id")		-- Foreign key to the IncidentEventType table.
);
-- Index on the IntegrationCallbackIncidentEventType table's tenantGuid field.
CREATE INDEX "I_IntegrationCallbackIncidentEventType_tenantGuid" ON "Alerting"."IntegrationCallbackIncidentEventType" ("tenantGuid")
;

-- Index on the IntegrationCallbackIncidentEventType table's tenantGuid,integrationId fields.
CREATE INDEX "I_IntegrationCallbackIncidentEventType_tenantGuid_integrationId" ON "Alerting"."IntegrationCallbackIncidentEventType" ("tenantGuid", "integrationId")
;

-- Index on the IntegrationCallbackIncidentEventType table's tenantGuid,incidentEventTypeId fields.
CREATE INDEX "I_IntegrationCallbackIncidentEventType_tenantGuid_incidentEvent" ON "Alerting"."IntegrationCallbackIncidentEventType" ("tenantGuid", "incidentEventTypeId")
;

-- Index on the IntegrationCallbackIncidentEventType table's tenantGuid,active fields.
CREATE INDEX "I_IntegrationCallbackIncidentEventType_tenantGuid_active" ON "Alerting"."IntegrationCallbackIncidentEventType" ("tenantGuid", "active")
;

-- Index on the IntegrationCallbackIncidentEventType table's tenantGuid,deleted fields.
CREATE INDEX "I_IntegrationCallbackIncidentEventType_tenantGuid_deleted" ON "Alerting"."IntegrationCallbackIncidentEventType" ("tenantGuid", "deleted")
;


-- The change history for records from the IntegrationCallbackIncidentEventType table.
CREATE TABLE "Alerting"."IntegrationCallbackIncidentEventTypeChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"integrationCallbackIncidentEventTypeId" INT NOT NULL,		-- Link to the IntegrationCallbackIncidentEventType table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "integrationCallbackIncidentEventTypeId" FOREIGN KEY ("integrationCallbackIncidentEventTypeId") REFERENCES "Alerting"."IntegrationCallbackIncidentEventType"("id")		-- Foreign key to the IntegrationCallbackIncidentEventType table.
);
-- Index on the IntegrationCallbackIncidentEventTypeChangeHistory table's tenantGuid field.
CREATE INDEX "I_IntegrationCallbackIncidentEventTypeChangeHistory_tenantGuid" ON "Alerting"."IntegrationCallbackIncidentEventTypeChangeHistory" ("tenantGuid")
;

-- Index on the IntegrationCallbackIncidentEventTypeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_IntegrationCallbackIncidentEventTypeChangeHistory_tenantGuid_" ON "Alerting"."IntegrationCallbackIncidentEventTypeChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the IntegrationCallbackIncidentEventTypeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_IntegrationCallbackIncidentEventTypeChangeHistory_tenantGuid_" ON "Alerting"."IntegrationCallbackIncidentEventTypeChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the IntegrationCallbackIncidentEventTypeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_IntegrationCallbackIncidentEventTypeChangeHistory_tenantGuid_" ON "Alerting"."IntegrationCallbackIncidentEventTypeChangeHistory" ("tenantGuid", "userId")
;

-- Index on the IntegrationCallbackIncidentEventTypeChangeHistory table's tenantGuid,integrationCallbackIncidentEventTypeId fields.
CREATE INDEX "I_IntegrationCallbackIncidentEventTypeChangeHistory_tenantGuid_" ON "Alerting"."IntegrationCallbackIncidentEventTypeChangeHistory" ("tenantGuid", "integrationCallbackIncidentEventTypeId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Per-user notification preferences (channels, quiet hours, DND, etc.). Users can edit their own preferences.
CREATE TABLE "Alerting"."UserNotificationPreference"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"securityUserObjectGuid" VARCHAR(50) NOT NULL,		-- References Security.SecurityUser.objectGuid - one preference row per user.
	"timeZoneId" VARCHAR(50) NULL DEFAULT 'UTC',		-- User's preferred timezone for quiet hours scheduling.
	"quietHoursStart" VARCHAR(10) NULL,		-- HH:mm format local to timeZoneId - start of quiet hours (null = no quiet hours).
	"quietHoursEnd" VARCHAR(10) NULL,		-- HH:mm format local to timeZoneId - end of quiet hours.
	"isDoNotDisturb" BOOLEAN NOT NULL DEFAULT false,		-- Global DND override - if true, no notifications except possibly critical overrides.
	"isDoNotDisturbPermanent" BOOLEAN NOT NULL DEFAULT false,		-- If true, DND has no scheduled end (until manually cleared).
	"doNotDisturbUntil" TIMESTAMP NULL,		-- Temporary DND end time (ignored if isDoNotDisturbPermanent = true).
	"customSettingsJson" TEXT NULL,		-- Flexible JSON for future extensions (e.g., per-severity overrides, custom sounds).
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_UserNotificationPreference_tenantGuid_securityUserObjectGuid_active_deleted" UNIQUE ( "tenantGuid", "securityUserObjectGuid", "active", "deleted") 		-- Uniqueness enforced on the UserNotificationPreference table's tenantGuid and securityUserObjectGuid and active and deleted fields.
);
-- Index on the UserNotificationPreference table's tenantGuid field.
CREATE INDEX "I_UserNotificationPreference_tenantGuid" ON "Alerting"."UserNotificationPreference" ("tenantGuid")
;

-- Index on the UserNotificationPreference table's tenantGuid,active fields.
CREATE INDEX "I_UserNotificationPreference_tenantGuid_active" ON "Alerting"."UserNotificationPreference" ("tenantGuid", "active")
;

-- Index on the UserNotificationPreference table's tenantGuid,deleted fields.
CREATE INDEX "I_UserNotificationPreference_tenantGuid_deleted" ON "Alerting"."UserNotificationPreference" ("tenantGuid", "deleted")
;

-- Index on the UserNotificationPreference table's securityUserObjectGuid field.
CREATE INDEX "I_UserNotificationPreference_securityUserObjectGuid" ON "Alerting"."UserNotificationPreference" ("securityUserObjectGuid")
;


-- The change history for records from the UserNotificationPreference table.
CREATE TABLE "Alerting"."UserNotificationPreferenceChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userNotificationPreferenceId" INT NOT NULL,		-- Link to the UserNotificationPreference table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "userNotificationPreferenceId" FOREIGN KEY ("userNotificationPreferenceId") REFERENCES "Alerting"."UserNotificationPreference"("id")		-- Foreign key to the UserNotificationPreference table.
);
-- Index on the UserNotificationPreferenceChangeHistory table's tenantGuid field.
CREATE INDEX "I_UserNotificationPreferenceChangeHistory_tenantGuid" ON "Alerting"."UserNotificationPreferenceChangeHistory" ("tenantGuid")
;

-- Index on the UserNotificationPreferenceChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_UserNotificationPreferenceChangeHistory_tenantGuid_versionNum" ON "Alerting"."UserNotificationPreferenceChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the UserNotificationPreferenceChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_UserNotificationPreferenceChangeHistory_tenantGuid_timeStamp" ON "Alerting"."UserNotificationPreferenceChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the UserNotificationPreferenceChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_UserNotificationPreferenceChangeHistory_tenantGuid_userId" ON "Alerting"."UserNotificationPreferenceChangeHistory" ("tenantGuid", "userId")
;

-- Index on the UserNotificationPreferenceChangeHistory table's tenantGuid,userNotificationPreferenceId fields.
CREATE INDEX "I_UserNotificationPreferenceChangeHistory_tenantGuid_userNotifi" ON "Alerting"."UserNotificationPreferenceChangeHistory" ("tenantGuid", "userNotificationPreferenceId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Per-user, per-channel notification preferences (enable/disable, custom priority).
CREATE TABLE "Alerting"."UserNotificationChannelPreference"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userNotificationPreferenceId" INT NOT NULL,		-- Link to the UserNotificationPreference table.
	"notificationChannelTypeId" INT NOT NULL,		-- Link to the NotificationChannelType table.
	"isEnabled" BOOLEAN NOT NULL DEFAULT true,		-- If false, this channel is disabled for the user (overrides system defaults).
	"priorityOverride" INT NULL,		-- Optional custom priority (lower = higher urgency) - null = use channel default.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "userNotificationPreferenceId" FOREIGN KEY ("userNotificationPreferenceId") REFERENCES "Alerting"."UserNotificationPreference"("id"),		-- Foreign key to the UserNotificationPreference table.
	CONSTRAINT "notificationChannelTypeId" FOREIGN KEY ("notificationChannelTypeId") REFERENCES "Alerting"."NotificationChannelType"("id"),		-- Foreign key to the NotificationChannelType table.
	CONSTRAINT "UC_UserNotificationChannelPreference_tenantGuid_userNotificationPreferenceId_notificationChannelTypeId" UNIQUE ( "tenantGuid", "userNotificationPreferenceId", "notificationChannelTypeId") 		-- Uniqueness enforced on the UserNotificationChannelPreference table's tenantGuid and userNotificationPreferenceId and notificationChannelTypeId fields.
);
-- Index on the UserNotificationChannelPreference table's tenantGuid field.
CREATE INDEX "I_UserNotificationChannelPreference_tenantGuid" ON "Alerting"."UserNotificationChannelPreference" ("tenantGuid")
;

-- Index on the UserNotificationChannelPreference table's tenantGuid,userNotificationPreferenceId fields.
CREATE INDEX "I_UserNotificationChannelPreference_tenantGuid_userNotification" ON "Alerting"."UserNotificationChannelPreference" ("tenantGuid", "userNotificationPreferenceId")
;

-- Index on the UserNotificationChannelPreference table's tenantGuid,notificationChannelTypeId fields.
CREATE INDEX "I_UserNotificationChannelPreference_tenantGuid_notificationChan" ON "Alerting"."UserNotificationChannelPreference" ("tenantGuid", "notificationChannelTypeId")
;

-- Index on the UserNotificationChannelPreference table's tenantGuid,active fields.
CREATE INDEX "I_UserNotificationChannelPreference_tenantGuid_active" ON "Alerting"."UserNotificationChannelPreference" ("tenantGuid", "active")
;

-- Index on the UserNotificationChannelPreference table's tenantGuid,deleted fields.
CREATE INDEX "I_UserNotificationChannelPreference_tenantGuid_deleted" ON "Alerting"."UserNotificationChannelPreference" ("tenantGuid", "deleted")
;


-- The change history for records from the UserNotificationChannelPreference table.
CREATE TABLE "Alerting"."UserNotificationChannelPreferenceChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userNotificationChannelPreferenceId" INT NOT NULL,		-- Link to the UserNotificationChannelPreference table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "userNotificationChannelPreferenceId" FOREIGN KEY ("userNotificationChannelPreferenceId") REFERENCES "Alerting"."UserNotificationChannelPreference"("id")		-- Foreign key to the UserNotificationChannelPreference table.
);
-- Index on the UserNotificationChannelPreferenceChangeHistory table's tenantGuid field.
CREATE INDEX "I_UserNotificationChannelPreferenceChangeHistory_tenantGuid" ON "Alerting"."UserNotificationChannelPreferenceChangeHistory" ("tenantGuid")
;

-- Index on the UserNotificationChannelPreferenceChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_UserNotificationChannelPreferenceChangeHistory_tenantGuid_ver" ON "Alerting"."UserNotificationChannelPreferenceChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the UserNotificationChannelPreferenceChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_UserNotificationChannelPreferenceChangeHistory_tenantGuid_tim" ON "Alerting"."UserNotificationChannelPreferenceChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the UserNotificationChannelPreferenceChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_UserNotificationChannelPreferenceChangeHistory_tenantGuid_use" ON "Alerting"."UserNotificationChannelPreferenceChangeHistory" ("tenantGuid", "userId")
;

-- Index on the UserNotificationChannelPreferenceChangeHistory table's tenantGuid,userNotificationChannelPreferenceId fields.
CREATE INDEX "I_UserNotificationChannelPreferenceChangeHistory_tenantGuid_use" ON "Alerting"."UserNotificationChannelPreferenceChangeHistory" ("tenantGuid", "userNotificationChannelPreferenceId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Push notification tokens for web and mobile devices. Each user can have multiple tokens (one per device).
CREATE TABLE "Alerting"."UserPushToken"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userObjectGuid" VARCHAR(50) NOT NULL,		-- References Security.SecurityUser.objectGuid - the token owner.
	"fcmToken" VARCHAR(500) NOT NULL,		-- Firebase Cloud Messaging token for this device.
	"deviceFingerprint" VARCHAR(100) NOT NULL,		-- Unique identifier for the device/browser to prevent duplicates.
	"platform" VARCHAR(50) NOT NULL DEFAULT 'web',		-- Platform: 'web', 'ios', 'android'.
	"userAgent" VARCHAR(500) NULL,		-- Browser/device user agent string for diagnostics.
	"registeredAt" TIMESTAMP NOT NULL,		-- When the token was first registered.
	"lastUpdatedAt" TIMESTAMP NOT NULL,		-- Last time the token was refreshed.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_UserPushToken_tenantGuid_userObjectGuid_deviceFingerprint" UNIQUE ( "tenantGuid", "userObjectGuid", "deviceFingerprint") 		-- Uniqueness enforced on the UserPushToken table's tenantGuid and userObjectGuid and deviceFingerprint fields.
);
-- Index on the UserPushToken table's tenantGuid field.
CREATE INDEX "I_UserPushToken_tenantGuid" ON "Alerting"."UserPushToken" ("tenantGuid")
;

-- Index on the UserPushToken table's tenantGuid,active fields.
CREATE INDEX "I_UserPushToken_tenantGuid_active" ON "Alerting"."UserPushToken" ("tenantGuid", "active")
;

-- Index on the UserPushToken table's tenantGuid,deleted fields.
CREATE INDEX "I_UserPushToken_tenantGuid_deleted" ON "Alerting"."UserPushToken" ("tenantGuid", "deleted")
;

-- Index on the UserPushToken table's tenantGuid,userObjectGuid fields.
CREATE INDEX "I_UserPushToken_tenantGuid_userObjectGuid" ON "Alerting"."UserPushToken" ("tenantGuid", "userObjectGuid")
;


-- The change history for records from the UserPushToken table.
CREATE TABLE "Alerting"."UserPushTokenChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userPushTokenId" INT NOT NULL,		-- Link to the UserPushToken table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "userPushTokenId" FOREIGN KEY ("userPushTokenId") REFERENCES "Alerting"."UserPushToken"("id")		-- Foreign key to the UserPushToken table.
);
-- Index on the UserPushTokenChangeHistory table's tenantGuid field.
CREATE INDEX "I_UserPushTokenChangeHistory_tenantGuid" ON "Alerting"."UserPushTokenChangeHistory" ("tenantGuid")
;

-- Index on the UserPushTokenChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_UserPushTokenChangeHistory_tenantGuid_versionNumber" ON "Alerting"."UserPushTokenChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the UserPushTokenChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_UserPushTokenChangeHistory_tenantGuid_timeStamp" ON "Alerting"."UserPushTokenChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the UserPushTokenChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_UserPushTokenChangeHistory_tenantGuid_userId" ON "Alerting"."UserPushTokenChangeHistory" ("tenantGuid", "userId")
;

-- Index on the UserPushTokenChangeHistory table's tenantGuid,userPushTokenId fields.
CREATE INDEX "I_UserPushTokenChangeHistory_tenantGuid_userPushTokenId" ON "Alerting"."UserPushTokenChangeHistory" ("tenantGuid", "userPushTokenId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Active and historical incidents.
CREATE TABLE "Alerting"."Incident"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"incidentKey" VARCHAR(250) NOT NULL,
	"serviceId" INT NOT NULL,		-- Link to the Service table.
	"title" VARCHAR(250) NOT NULL,
	"description" TEXT NULL,
	"severityTypeId" INT NOT NULL,		-- Link to the SeverityType table.
	"incidentStatusTypeId" INT NOT NULL,		-- Link to the IncidentStatusType table.
	"createdAt" TIMESTAMP NOT NULL,
	"escalationRuleId" INT NULL,		-- Current active escalation rule (null = no active escalation, e.g., acknowledged/resolved).
	"currentRepeatCount" INT NULL DEFAULT 0,		-- How many repeat notifications have been sent for the current rule (resets on rule change).
	"nextEscalationAt" TIMESTAMP NULL,		-- Timestamp when the next escalation/repeat should fire (drives worker query).
	"acknowledgedAt" TIMESTAMP NULL,
	"resolvedAt" TIMESTAMP NULL,
	"currentAssigneeObjectGuid" VARCHAR(50) NULL,
	"sourcePayloadJson" TEXT NULL,
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "serviceId" FOREIGN KEY ("serviceId") REFERENCES "Alerting"."Service"("id"),		-- Foreign key to the Service table.
	CONSTRAINT "severityTypeId" FOREIGN KEY ("severityTypeId") REFERENCES "Alerting"."SeverityType"("id"),		-- Foreign key to the SeverityType table.
	CONSTRAINT "incidentStatusTypeId" FOREIGN KEY ("incidentStatusTypeId") REFERENCES "Alerting"."IncidentStatusType"("id"),		-- Foreign key to the IncidentStatusType table.
	CONSTRAINT "escalationRuleId" FOREIGN KEY ("escalationRuleId") REFERENCES "Alerting"."EscalationRule"("id"),		-- Foreign key to the EscalationRule table.
	CONSTRAINT "UC_Incident_tenantGuid_incidentKey" UNIQUE ( "tenantGuid", "incidentKey") 		-- Uniqueness enforced on the Incident table's tenantGuid and incidentKey fields.
);
-- Index on the Incident table's tenantGuid field.
CREATE INDEX "I_Incident_tenantGuid" ON "Alerting"."Incident" ("tenantGuid")
;

-- Index on the Incident table's tenantGuid,incidentKey fields.
CREATE INDEX "I_Incident_tenantGuid_incidentKey" ON "Alerting"."Incident" ("tenantGuid", "incidentKey")
;

-- Index on the Incident table's tenantGuid,serviceId fields.
CREATE INDEX "I_Incident_tenantGuid_serviceId" ON "Alerting"."Incident" ("tenantGuid", "serviceId")
;

-- Index on the Incident table's tenantGuid,severityTypeId fields.
CREATE INDEX "I_Incident_tenantGuid_severityTypeId" ON "Alerting"."Incident" ("tenantGuid", "severityTypeId")
;

-- Index on the Incident table's tenantGuid,incidentStatusTypeId fields.
CREATE INDEX "I_Incident_tenantGuid_incidentStatusTypeId" ON "Alerting"."Incident" ("tenantGuid", "incidentStatusTypeId")
;

-- Index on the Incident table's tenantGuid,createdAt fields.
CREATE INDEX "I_Incident_tenantGuid_createdAt" ON "Alerting"."Incident" ("tenantGuid", "createdAt")
;

-- Index on the Incident table's tenantGuid,escalationRuleId fields.
CREATE INDEX "I_Incident_tenantGuid_escalationRuleId" ON "Alerting"."Incident" ("tenantGuid", "escalationRuleId")
;

-- Index on the Incident table's tenantGuid,active fields.
CREATE INDEX "I_Incident_tenantGuid_active" ON "Alerting"."Incident" ("tenantGuid", "active")
;

-- Index on the Incident table's tenantGuid,deleted fields.
CREATE INDEX "I_Incident_tenantGuid_deleted" ON "Alerting"."Incident" ("tenantGuid", "deleted")
;

-- Index on the Incident table's tenantGuid,incidentStatusTypeId,createdAt fields.
CREATE INDEX "I_Incident_tenantGuid_incidentStatusTypeId_createdAt" ON "Alerting"."Incident" ("tenantGuid", "incidentStatusTypeId", "createdAt")
;

-- Index on the Incident table's tenantGuid,serviceId,createdAt fields.
CREATE INDEX "I_Incident_tenantGuid_serviceId_createdAt" ON "Alerting"."Incident" ("tenantGuid", "serviceId", "createdAt")
;

-- Index on the Incident table's tenantGuid,nextEscalationAt,incidentStatusTypeId fields.
CREATE INDEX "I_Incident_tenantGuid_nextEscalationAt_incidentStatusTypeId" ON "Alerting"."Incident" ("tenantGuid", "nextEscalationAt", "incidentStatusTypeId")
;


-- The change history for records from the Incident table.
CREATE TABLE "Alerting"."IncidentChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"incidentId" INT NOT NULL,		-- Link to the Incident table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "incidentId" FOREIGN KEY ("incidentId") REFERENCES "Alerting"."Incident"("id")		-- Foreign key to the Incident table.
);
-- Index on the IncidentChangeHistory table's tenantGuid field.
CREATE INDEX "I_IncidentChangeHistory_tenantGuid" ON "Alerting"."IncidentChangeHistory" ("tenantGuid")
;

-- Index on the IncidentChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_IncidentChangeHistory_tenantGuid_versionNumber" ON "Alerting"."IncidentChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the IncidentChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_IncidentChangeHistory_tenantGuid_timeStamp" ON "Alerting"."IncidentChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the IncidentChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_IncidentChangeHistory_tenantGuid_userId" ON "Alerting"."IncidentChangeHistory" ("tenantGuid", "userId")
;

-- Index on the IncidentChangeHistory table's tenantGuid,incidentId fields.
CREATE INDEX "I_IncidentChangeHistory_tenantGuid_incidentId" ON "Alerting"."IncidentChangeHistory" ("tenantGuid", "incidentId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Timeline events for incidents.
CREATE TABLE "Alerting"."IncidentTimelineEvent"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"incidentId" INT NOT NULL,		-- Link to the Incident table.
	"incidentEventTypeId" INT NOT NULL,		-- Link to the IncidentEventType table.
	"timestamp" TIMESTAMP NOT NULL,
	"actorObjectGuid" VARCHAR(50) NULL,
	"detailsJson" TEXT NULL,
	"notes" VARCHAR(500) NULL,		-- Human-readable context for this event (e.g., 'Escalation rule 1 fired - notifying on-call schedule').
	"source" VARCHAR(50) NULL DEFAULT 'system',		-- Event source: 'system', 'user', 'api', 'webhook'.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "incidentId" FOREIGN KEY ("incidentId") REFERENCES "Alerting"."Incident"("id"),		-- Foreign key to the Incident table.
	CONSTRAINT "incidentEventTypeId" FOREIGN KEY ("incidentEventTypeId") REFERENCES "Alerting"."IncidentEventType"("id")		-- Foreign key to the IncidentEventType table.
);
-- Index on the IncidentTimelineEvent table's tenantGuid field.
CREATE INDEX "I_IncidentTimelineEvent_tenantGuid" ON "Alerting"."IncidentTimelineEvent" ("tenantGuid")
;

-- Index on the IncidentTimelineEvent table's tenantGuid,incidentId fields.
CREATE INDEX "I_IncidentTimelineEvent_tenantGuid_incidentId" ON "Alerting"."IncidentTimelineEvent" ("tenantGuid", "incidentId")
;

-- Index on the IncidentTimelineEvent table's tenantGuid,incidentEventTypeId fields.
CREATE INDEX "I_IncidentTimelineEvent_tenantGuid_incidentEventTypeId" ON "Alerting"."IncidentTimelineEvent" ("tenantGuid", "incidentEventTypeId")
;

-- Index on the IncidentTimelineEvent table's tenantGuid,timestamp fields.
CREATE INDEX "I_IncidentTimelineEvent_tenantGuid_timestamp" ON "Alerting"."IncidentTimelineEvent" ("tenantGuid", "timestamp")
;

-- Index on the IncidentTimelineEvent table's tenantGuid,active fields.
CREATE INDEX "I_IncidentTimelineEvent_tenantGuid_active" ON "Alerting"."IncidentTimelineEvent" ("tenantGuid", "active")
;

-- Index on the IncidentTimelineEvent table's tenantGuid,deleted fields.
CREATE INDEX "I_IncidentTimelineEvent_tenantGuid_deleted" ON "Alerting"."IncidentTimelineEvent" ("tenantGuid", "deleted")
;

-- Index on the IncidentTimelineEvent table's incidentId,timestamp fields.
CREATE INDEX "I_IncidentTimelineEvent_incidentId_timestamp" ON "Alerting"."IncidentTimelineEvent" ("incidentId", "timestamp")
;


-- Notes added to incidents by responders.
CREATE TABLE "Alerting"."IncidentNote"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"incidentId" INT NOT NULL,		-- Link to the Incident table.
	"authorObjectGuid" VARCHAR(50) NOT NULL,
	"createdAt" TIMESTAMP NOT NULL,
	"content" TEXT NOT NULL,
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "incidentId" FOREIGN KEY ("incidentId") REFERENCES "Alerting"."Incident"("id")		-- Foreign key to the Incident table.
);
-- Index on the IncidentNote table's tenantGuid field.
CREATE INDEX "I_IncidentNote_tenantGuid" ON "Alerting"."IncidentNote" ("tenantGuid")
;

-- Index on the IncidentNote table's tenantGuid,incidentId fields.
CREATE INDEX "I_IncidentNote_tenantGuid_incidentId" ON "Alerting"."IncidentNote" ("tenantGuid", "incidentId")
;

-- Index on the IncidentNote table's tenantGuid,active fields.
CREATE INDEX "I_IncidentNote_tenantGuid_active" ON "Alerting"."IncidentNote" ("tenantGuid", "active")
;

-- Index on the IncidentNote table's tenantGuid,deleted fields.
CREATE INDEX "I_IncidentNote_tenantGuid_deleted" ON "Alerting"."IncidentNote" ("tenantGuid", "deleted")
;


-- The change history for records from the IncidentNote table.
CREATE TABLE "Alerting"."IncidentNoteChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"incidentNoteId" INT NOT NULL,		-- Link to the IncidentNote table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "incidentNoteId" FOREIGN KEY ("incidentNoteId") REFERENCES "Alerting"."IncidentNote"("id")		-- Foreign key to the IncidentNote table.
);
-- Index on the IncidentNoteChangeHistory table's tenantGuid field.
CREATE INDEX "I_IncidentNoteChangeHistory_tenantGuid" ON "Alerting"."IncidentNoteChangeHistory" ("tenantGuid")
;

-- Index on the IncidentNoteChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_IncidentNoteChangeHistory_tenantGuid_versionNumber" ON "Alerting"."IncidentNoteChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the IncidentNoteChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_IncidentNoteChangeHistory_tenantGuid_timeStamp" ON "Alerting"."IncidentNoteChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the IncidentNoteChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_IncidentNoteChangeHistory_tenantGuid_userId" ON "Alerting"."IncidentNoteChangeHistory" ("tenantGuid", "userId")
;

-- Index on the IncidentNoteChangeHistory table's tenantGuid,incidentNoteId fields.
CREATE INDEX "I_IncidentNoteChangeHistory_tenantGuid_incidentNoteId" ON "Alerting"."IncidentNoteChangeHistory" ("tenantGuid", "incidentNoteId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Notifications sent to individual users as part of escalation (teams/schedules are resolved to users at runtime).
CREATE TABLE "Alerting"."IncidentNotification"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"incidentId" INT NOT NULL,		-- Link to the Incident table.
	"escalationRuleId" INT NULL,		-- Link to the EscalationRule table.
	"userObjectGuid" VARCHAR(50) NOT NULL,		-- Resolved Security.SecurityUser.objectGuid that was notified.
	"firstNotifiedAt" TIMESTAMP NOT NULL,
	"lastNotifiedAt" TIMESTAMP NULL,
	"acknowledgedAt" TIMESTAMP NULL,
	"acknowledgedByObjectGuid" VARCHAR(50) NULL,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "incidentId" FOREIGN KEY ("incidentId") REFERENCES "Alerting"."Incident"("id"),		-- Foreign key to the Incident table.
	CONSTRAINT "escalationRuleId" FOREIGN KEY ("escalationRuleId") REFERENCES "Alerting"."EscalationRule"("id")		-- Foreign key to the EscalationRule table.
);
-- Index on the IncidentNotification table's tenantGuid field.
CREATE INDEX "I_IncidentNotification_tenantGuid" ON "Alerting"."IncidentNotification" ("tenantGuid")
;

-- Index on the IncidentNotification table's tenantGuid,incidentId fields.
CREATE INDEX "I_IncidentNotification_tenantGuid_incidentId" ON "Alerting"."IncidentNotification" ("tenantGuid", "incidentId")
;

-- Index on the IncidentNotification table's tenantGuid,escalationRuleId fields.
CREATE INDEX "I_IncidentNotification_tenantGuid_escalationRuleId" ON "Alerting"."IncidentNotification" ("tenantGuid", "escalationRuleId")
;

-- Index on the IncidentNotification table's tenantGuid,active fields.
CREATE INDEX "I_IncidentNotification_tenantGuid_active" ON "Alerting"."IncidentNotification" ("tenantGuid", "active")
;

-- Index on the IncidentNotification table's tenantGuid,deleted fields.
CREATE INDEX "I_IncidentNotification_tenantGuid_deleted" ON "Alerting"."IncidentNotification" ("tenantGuid", "deleted")
;

-- Index on the IncidentNotification table's incidentId,userObjectGuid fields.
CREATE INDEX "I_IncidentNotification_incidentId_userObjectGuid" ON "Alerting"."IncidentNotification" ("incidentId", "userObjectGuid")
;


-- Individual delivery attempts per channel for a notification.
CREATE TABLE "Alerting"."NotificationDeliveryAttempt"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"incidentNotificationId" INT NOT NULL,		-- Link to the IncidentNotification table.
	"notificationChannelTypeId" INT NOT NULL,		-- Link to the NotificationChannelType table.
	"attemptNumber" INT NOT NULL DEFAULT 1,
	"attemptedAt" TIMESTAMP NOT NULL,
	"status" VARCHAR(50) NOT NULL DEFAULT 'Pending',
	"errorMessage" TEXT NULL,
	"response" TEXT NULL,
	"recipientAddress" VARCHAR(250) NULL,		-- Email address, phone number, or device token that received the notification.
	"subject" VARCHAR(500) NULL,		-- Subject line for email notifications, null for other channels.
	"bodyContent" TEXT NULL,		-- Full message body content that was sent (HTML for email, plain text for SMS/voice).
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "incidentNotificationId" FOREIGN KEY ("incidentNotificationId") REFERENCES "Alerting"."IncidentNotification"("id"),		-- Foreign key to the IncidentNotification table.
	CONSTRAINT "notificationChannelTypeId" FOREIGN KEY ("notificationChannelTypeId") REFERENCES "Alerting"."NotificationChannelType"("id")		-- Foreign key to the NotificationChannelType table.
);
-- Index on the NotificationDeliveryAttempt table's tenantGuid field.
CREATE INDEX "I_NotificationDeliveryAttempt_tenantGuid" ON "Alerting"."NotificationDeliveryAttempt" ("tenantGuid")
;

-- Index on the NotificationDeliveryAttempt table's tenantGuid,incidentNotificationId fields.
CREATE INDEX "I_NotificationDeliveryAttempt_tenantGuid_incidentNotificationId" ON "Alerting"."NotificationDeliveryAttempt" ("tenantGuid", "incidentNotificationId")
;

-- Index on the NotificationDeliveryAttempt table's tenantGuid,notificationChannelTypeId fields.
CREATE INDEX "I_NotificationDeliveryAttempt_tenantGuid_notificationChannelTyp" ON "Alerting"."NotificationDeliveryAttempt" ("tenantGuid", "notificationChannelTypeId")
;

-- Index on the NotificationDeliveryAttempt table's tenantGuid,active fields.
CREATE INDEX "I_NotificationDeliveryAttempt_tenantGuid_active" ON "Alerting"."NotificationDeliveryAttempt" ("tenantGuid", "active")
;

-- Index on the NotificationDeliveryAttempt table's tenantGuid,deleted fields.
CREATE INDEX "I_NotificationDeliveryAttempt_tenantGuid_deleted" ON "Alerting"."NotificationDeliveryAttempt" ("tenantGuid", "deleted")
;

-- Index on the NotificationDeliveryAttempt table's tenantGuid,incidentNotificationId,notificationChannelTypeId fields.
CREATE INDEX "I_NotificationDeliveryAttempt_tenantGuid_incidentNotificationId" ON "Alerting"."NotificationDeliveryAttempt" ("tenantGuid", "incidentNotificationId", "notificationChannelTypeId")
;


-- Outbound webhook delivery attempts for incident status updates.
CREATE TABLE "Alerting"."WebhookDeliveryAttempt"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"incidentId" INT NOT NULL,		-- Link to the Incident table.
	"integrationId" INT NOT NULL,		-- Link to the Integration table.
	"incidentTimelineEventId" INT NULL,		-- Link to the IncidentTimelineEvent table.
	"attemptNumber" INT NOT NULL DEFAULT 1,
	"attemptedAt" TIMESTAMP NOT NULL,
	"httpStatusCode" INT NULL,
	"success" BOOLEAN NOT NULL DEFAULT false,
	"payloadJson" TEXT NULL,
	"responseBody" TEXT NULL,
	"errorMessage" TEXT NULL,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "incidentId" FOREIGN KEY ("incidentId") REFERENCES "Alerting"."Incident"("id"),		-- Foreign key to the Incident table.
	CONSTRAINT "integrationId" FOREIGN KEY ("integrationId") REFERENCES "Alerting"."Integration"("id"),		-- Foreign key to the Integration table.
	CONSTRAINT "incidentTimelineEventId" FOREIGN KEY ("incidentTimelineEventId") REFERENCES "Alerting"."IncidentTimelineEvent"("id")		-- Foreign key to the IncidentTimelineEvent table.
);
-- Index on the WebhookDeliveryAttempt table's tenantGuid field.
CREATE INDEX "I_WebhookDeliveryAttempt_tenantGuid" ON "Alerting"."WebhookDeliveryAttempt" ("tenantGuid")
;

-- Index on the WebhookDeliveryAttempt table's tenantGuid,incidentId fields.
CREATE INDEX "I_WebhookDeliveryAttempt_tenantGuid_incidentId" ON "Alerting"."WebhookDeliveryAttempt" ("tenantGuid", "incidentId")
;

-- Index on the WebhookDeliveryAttempt table's tenantGuid,integrationId fields.
CREATE INDEX "I_WebhookDeliveryAttempt_tenantGuid_integrationId" ON "Alerting"."WebhookDeliveryAttempt" ("tenantGuid", "integrationId")
;

-- Index on the WebhookDeliveryAttempt table's tenantGuid,incidentTimelineEventId fields.
CREATE INDEX "I_WebhookDeliveryAttempt_tenantGuid_incidentTimelineEventId" ON "Alerting"."WebhookDeliveryAttempt" ("tenantGuid", "incidentTimelineEventId")
;

-- Index on the WebhookDeliveryAttempt table's tenantGuid,active fields.
CREATE INDEX "I_WebhookDeliveryAttempt_tenantGuid_active" ON "Alerting"."WebhookDeliveryAttempt" ("tenantGuid", "active")
;

-- Index on the WebhookDeliveryAttempt table's tenantGuid,deleted fields.
CREATE INDEX "I_WebhookDeliveryAttempt_tenantGuid_deleted" ON "Alerting"."WebhookDeliveryAttempt" ("tenantGuid", "deleted")
;

-- Index on the WebhookDeliveryAttempt table's tenantGuid,incidentId,attemptedAt fields.
CREATE INDEX "I_WebhookDeliveryAttempt_tenantGuid_incidentId_attemptedAt" ON "Alerting"."WebhookDeliveryAttempt" ("tenantGuid", "incidentId", "attemptedAt")
;


