/*
Alerting and Incident Management database.
Stores services, escalation policies, on-call schedules, incidents, timeline events,
notification delivery tracking, webhook delivery tracking, and integration credentials.
Designed to be independent while sharing the central Security database for users and teams.
*/
CREATE DATABASE [Alerting]
GO

ALTER DATABASE [Alerting] SET RECOVERY SIMPLE
GO

USE [Alerting]
GO

CREATE SCHEMA [Alerting]
GO

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE [Alerting].[WebhookDeliveryAttempt]
-- DROP TABLE [Alerting].[NotificationDeliveryAttempt]
-- DROP TABLE [Alerting].[IncidentNotification]
-- DROP TABLE [Alerting].[IncidentNoteChangeHistory]
-- DROP TABLE [Alerting].[IncidentNote]
-- DROP TABLE [Alerting].[IncidentTimelineEvent]
-- DROP TABLE [Alerting].[IncidentChangeHistory]
-- DROP TABLE [Alerting].[Incident]
-- DROP TABLE [Alerting].[UserPushTokenChangeHistory]
-- DROP TABLE [Alerting].[UserPushToken]
-- DROP TABLE [Alerting].[UserNotificationChannelPreferenceChangeHistory]
-- DROP TABLE [Alerting].[UserNotificationChannelPreference]
-- DROP TABLE [Alerting].[UserNotificationPreferenceChangeHistory]
-- DROP TABLE [Alerting].[UserNotificationPreference]
-- DROP TABLE [Alerting].[IntegrationCallbackIncidentEventTypeChangeHistory]
-- DROP TABLE [Alerting].[IntegrationCallbackIncidentEventType]
-- DROP TABLE [Alerting].[IntegrationChangeHistory]
-- DROP TABLE [Alerting].[Integration]
-- DROP TABLE [Alerting].[ScheduleOverrideChangeHistory]
-- DROP TABLE [Alerting].[ScheduleOverride]
-- DROP TABLE [Alerting].[ScheduleLayerMemberChangeHistory]
-- DROP TABLE [Alerting].[ScheduleLayerMember]
-- DROP TABLE [Alerting].[ScheduleLayerChangeHistory]
-- DROP TABLE [Alerting].[ScheduleLayer]
-- DROP TABLE [Alerting].[OnCallScheduleChangeHistory]
-- DROP TABLE [Alerting].[OnCallSchedule]
-- DROP TABLE [Alerting].[EscalationRuleChangeHistory]
-- DROP TABLE [Alerting].[EscalationRule]
-- DROP TABLE [Alerting].[ServiceChangeHistory]
-- DROP TABLE [Alerting].[Service]
-- DROP TABLE [Alerting].[EscalationPolicyChangeHistory]
-- DROP TABLE [Alerting].[EscalationPolicy]
-- DROP TABLE [Alerting].[ScheduleOverrideType]
-- DROP TABLE [Alerting].[NotificationChannelType]
-- DROP TABLE [Alerting].[IncidentEventType]
-- DROP TABLE [Alerting].[IncidentStatusType]
-- DROP TABLE [Alerting].[SeverityType]

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON [Alerting].[WebhookDeliveryAttempt] DISABLE
-- ALTER INDEX ALL ON [Alerting].[NotificationDeliveryAttempt] DISABLE
-- ALTER INDEX ALL ON [Alerting].[IncidentNotification] DISABLE
-- ALTER INDEX ALL ON [Alerting].[IncidentNoteChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Alerting].[IncidentNote] DISABLE
-- ALTER INDEX ALL ON [Alerting].[IncidentTimelineEvent] DISABLE
-- ALTER INDEX ALL ON [Alerting].[IncidentChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Alerting].[Incident] DISABLE
-- ALTER INDEX ALL ON [Alerting].[UserPushTokenChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Alerting].[UserPushToken] DISABLE
-- ALTER INDEX ALL ON [Alerting].[UserNotificationChannelPreferenceChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Alerting].[UserNotificationChannelPreference] DISABLE
-- ALTER INDEX ALL ON [Alerting].[UserNotificationPreferenceChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Alerting].[UserNotificationPreference] DISABLE
-- ALTER INDEX ALL ON [Alerting].[IntegrationCallbackIncidentEventTypeChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Alerting].[IntegrationCallbackIncidentEventType] DISABLE
-- ALTER INDEX ALL ON [Alerting].[IntegrationChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Alerting].[Integration] DISABLE
-- ALTER INDEX ALL ON [Alerting].[ScheduleOverrideChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Alerting].[ScheduleOverride] DISABLE
-- ALTER INDEX ALL ON [Alerting].[ScheduleLayerMemberChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Alerting].[ScheduleLayerMember] DISABLE
-- ALTER INDEX ALL ON [Alerting].[ScheduleLayerChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Alerting].[ScheduleLayer] DISABLE
-- ALTER INDEX ALL ON [Alerting].[OnCallScheduleChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Alerting].[OnCallSchedule] DISABLE
-- ALTER INDEX ALL ON [Alerting].[EscalationRuleChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Alerting].[EscalationRule] DISABLE
-- ALTER INDEX ALL ON [Alerting].[ServiceChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Alerting].[Service] DISABLE
-- ALTER INDEX ALL ON [Alerting].[EscalationPolicyChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Alerting].[EscalationPolicy] DISABLE
-- ALTER INDEX ALL ON [Alerting].[ScheduleOverrideType] DISABLE
-- ALTER INDEX ALL ON [Alerting].[NotificationChannelType] DISABLE
-- ALTER INDEX ALL ON [Alerting].[IncidentEventType] DISABLE
-- ALTER INDEX ALL ON [Alerting].[IncidentStatusType] DISABLE
-- ALTER INDEX ALL ON [Alerting].[SeverityType] DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON [Alerting].[WebhookDeliveryAttempt] REBUILD
-- ALTER INDEX ALL ON [Alerting].[NotificationDeliveryAttempt] REBUILD
-- ALTER INDEX ALL ON [Alerting].[IncidentNotification] REBUILD
-- ALTER INDEX ALL ON [Alerting].[IncidentNoteChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Alerting].[IncidentNote] REBUILD
-- ALTER INDEX ALL ON [Alerting].[IncidentTimelineEvent] REBUILD
-- ALTER INDEX ALL ON [Alerting].[IncidentChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Alerting].[Incident] REBUILD
-- ALTER INDEX ALL ON [Alerting].[UserPushTokenChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Alerting].[UserPushToken] REBUILD
-- ALTER INDEX ALL ON [Alerting].[UserNotificationChannelPreferenceChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Alerting].[UserNotificationChannelPreference] REBUILD
-- ALTER INDEX ALL ON [Alerting].[UserNotificationPreferenceChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Alerting].[UserNotificationPreference] REBUILD
-- ALTER INDEX ALL ON [Alerting].[IntegrationCallbackIncidentEventTypeChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Alerting].[IntegrationCallbackIncidentEventType] REBUILD
-- ALTER INDEX ALL ON [Alerting].[IntegrationChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Alerting].[Integration] REBUILD
-- ALTER INDEX ALL ON [Alerting].[ScheduleOverrideChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Alerting].[ScheduleOverride] REBUILD
-- ALTER INDEX ALL ON [Alerting].[ScheduleLayerMemberChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Alerting].[ScheduleLayerMember] REBUILD
-- ALTER INDEX ALL ON [Alerting].[ScheduleLayerChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Alerting].[ScheduleLayer] REBUILD
-- ALTER INDEX ALL ON [Alerting].[OnCallScheduleChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Alerting].[OnCallSchedule] REBUILD
-- ALTER INDEX ALL ON [Alerting].[EscalationRuleChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Alerting].[EscalationRule] REBUILD
-- ALTER INDEX ALL ON [Alerting].[ServiceChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Alerting].[Service] REBUILD
-- ALTER INDEX ALL ON [Alerting].[EscalationPolicyChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Alerting].[EscalationPolicy] REBUILD
-- ALTER INDEX ALL ON [Alerting].[ScheduleOverrideType] REBUILD
-- ALTER INDEX ALL ON [Alerting].[NotificationChannelType] REBUILD
-- ALTER INDEX ALL ON [Alerting].[IncidentEventType] REBUILD
-- ALTER INDEX ALL ON [Alerting].[IncidentStatusType] REBUILD
-- ALTER INDEX ALL ON [Alerting].[SeverityType] REBUILD

-- Static severity levels for incidents.
CREATE TABLE [Alerting].[SeverityType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL,
	[sequence] INT NOT NULL,		-- Sequence to use for sorting.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the SeverityType table's name field.
CREATE INDEX [I_SeverityType_name] ON [Alerting].[SeverityType] ([name])
GO

-- Index on the SeverityType table's active field.
CREATE INDEX [I_SeverityType_active] ON [Alerting].[SeverityType] ([active])
GO

-- Index on the SeverityType table's deleted field.
CREATE INDEX [I_SeverityType_deleted] ON [Alerting].[SeverityType] ([deleted])
GO

INSERT INTO [Alerting].[SeverityType] ( [name], [description], [sequence] ) VALUES  ( 'Critical', 'Critical', 10 )
GO

INSERT INTO [Alerting].[SeverityType] ( [name], [description], [sequence] ) VALUES  ( 'High', 'High', 20 )
GO

INSERT INTO [Alerting].[SeverityType] ( [name], [description], [sequence] ) VALUES  ( 'Medium', 'Medium', 30 )
GO

INSERT INTO [Alerting].[SeverityType] ( [name], [description], [sequence] ) VALUES  ( 'Low', 'Low', 40 )
GO


-- Static status values for incidents.
CREATE TABLE [Alerting].[IncidentStatusType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the IncidentStatusType table's name field.
CREATE INDEX [I_IncidentStatusType_name] ON [Alerting].[IncidentStatusType] ([name])
GO

-- Index on the IncidentStatusType table's active field.
CREATE INDEX [I_IncidentStatusType_active] ON [Alerting].[IncidentStatusType] ([active])
GO

-- Index on the IncidentStatusType table's deleted field.
CREATE INDEX [I_IncidentStatusType_deleted] ON [Alerting].[IncidentStatusType] ([deleted])
GO

INSERT INTO [Alerting].[IncidentStatusType] ( [name], [description] ) VALUES  ( 'Triggered', 'Newly triggered incident' )
GO

INSERT INTO [Alerting].[IncidentStatusType] ( [name], [description] ) VALUES  ( 'Acknowledged', 'Acknowledged by a responder' )
GO

INSERT INTO [Alerting].[IncidentStatusType] ( [name], [description] ) VALUES  ( 'Resolved', 'Incident resolved' )
GO


-- Static event types for the incident timeline.
CREATE TABLE [Alerting].[IncidentEventType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the IncidentEventType table's name field.
CREATE INDEX [I_IncidentEventType_name] ON [Alerting].[IncidentEventType] ([name])
GO

-- Index on the IncidentEventType table's active field.
CREATE INDEX [I_IncidentEventType_active] ON [Alerting].[IncidentEventType] ([active])
GO

-- Index on the IncidentEventType table's deleted field.
CREATE INDEX [I_IncidentEventType_deleted] ON [Alerting].[IncidentEventType] ([deleted])
GO

INSERT INTO [Alerting].[IncidentEventType] ( [name], [description] ) VALUES  ( 'Triggered', 'Incident was triggered' )
GO

INSERT INTO [Alerting].[IncidentEventType] ( [name], [description] ) VALUES  ( 'Escalated', 'Escalation rule fired' )
GO

INSERT INTO [Alerting].[IncidentEventType] ( [name], [description] ) VALUES  ( 'Acknowledged', 'Incident acknowledged' )
GO

INSERT INTO [Alerting].[IncidentEventType] ( [name], [description] ) VALUES  ( 'Resolved', 'Incident resolved' )
GO

INSERT INTO [Alerting].[IncidentEventType] ( [name], [description] ) VALUES  ( 'NoteAdded', 'Note added to incident' )
GO

INSERT INTO [Alerting].[IncidentEventType] ( [name], [description] ) VALUES  ( 'NotificationSent', 'Notification delivery attempted' )
GO


-- Static notification delivery channels.
CREATE TABLE [Alerting].[NotificationChannelType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL,
	[defaultPriority] INT NOT NULL DEFAULT 0,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the NotificationChannelType table's name field.
CREATE INDEX [I_NotificationChannelType_name] ON [Alerting].[NotificationChannelType] ([name])
GO

-- Index on the NotificationChannelType table's active field.
CREATE INDEX [I_NotificationChannelType_active] ON [Alerting].[NotificationChannelType] ([active])
GO

-- Index on the NotificationChannelType table's deleted field.
CREATE INDEX [I_NotificationChannelType_deleted] ON [Alerting].[NotificationChannelType] ([deleted])
GO

INSERT INTO [Alerting].[NotificationChannelType] ( [name], [description], [defaultPriority] ) VALUES  ( 'Email', 'Email notification', 30 )
GO

INSERT INTO [Alerting].[NotificationChannelType] ( [name], [description], [defaultPriority] ) VALUES  ( 'SMS', 'SMS text message', 10 )
GO

INSERT INTO [Alerting].[NotificationChannelType] ( [name], [description], [defaultPriority] ) VALUES  ( 'VoiceCall', 'Automated voice call', 5 )
GO

INSERT INTO [Alerting].[NotificationChannelType] ( [name], [description], [defaultPriority] ) VALUES  ( 'MobilePush', 'Mobile app push', 20 )
GO

INSERT INTO [Alerting].[NotificationChannelType] ( [name], [description], [defaultPriority] ) VALUES  ( 'WebPush', 'Browser push notification', 25 )
GO

INSERT INTO [Alerting].[NotificationChannelType] ( [name], [description], [defaultPriority] ) VALUES  ( 'Teams', 'Microsoft Teams message', 40 )
GO


-- Static schedule override types.
CREATE TABLE [Alerting].[ScheduleOverrideType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL,
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the ScheduleOverrideType table's name field.
CREATE INDEX [I_ScheduleOverrideType_name] ON [Alerting].[ScheduleOverrideType] ([name])
GO

-- Index on the ScheduleOverrideType table's active field.
CREATE INDEX [I_ScheduleOverrideType_active] ON [Alerting].[ScheduleOverrideType] ([active])
GO

-- Index on the ScheduleOverrideType table's deleted field.
CREATE INDEX [I_ScheduleOverrideType_deleted] ON [Alerting].[ScheduleOverrideType] ([deleted])
GO

INSERT INTO [Alerting].[ScheduleOverrideType] ( [name], [description] ) VALUES  ( 'Swap', 'Swap - Two users exchange shifts' )
GO

INSERT INTO [Alerting].[ScheduleOverrideType] ( [name], [description] ) VALUES  ( 'Replace', 'Replace - One user temporarily takes over for another' )
GO

INSERT INTO [Alerting].[ScheduleOverrideType] ( [name], [description] ) VALUES  ( 'Remove', 'Remove - User taken off the schedule with no replacement' )
GO


-- Escalation policies assigned to services.
CREATE TABLE [Alerting].[EscalationPolicy]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_EscalationPolicy_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the EscalationPolicy table's tenantGuid and name fields.
)
GO

-- Index on the EscalationPolicy table's tenantGuid field.
CREATE INDEX [I_EscalationPolicy_tenantGuid] ON [Alerting].[EscalationPolicy] ([tenantGuid])
GO

-- Index on the EscalationPolicy table's tenantGuid,name fields.
CREATE INDEX [I_EscalationPolicy_tenantGuid_name] ON [Alerting].[EscalationPolicy] ([tenantGuid], [name])
GO

-- Index on the EscalationPolicy table's tenantGuid,active fields.
CREATE INDEX [I_EscalationPolicy_tenantGuid_active] ON [Alerting].[EscalationPolicy] ([tenantGuid], [active])
GO

-- Index on the EscalationPolicy table's tenantGuid,deleted fields.
CREATE INDEX [I_EscalationPolicy_tenantGuid_deleted] ON [Alerting].[EscalationPolicy] ([tenantGuid], [deleted])
GO


-- The change history for records from the EscalationPolicy table.
CREATE TABLE [Alerting].[EscalationPolicyChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[escalationPolicyId] INT NOT NULL,		-- Link to the EscalationPolicy table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_EscalationPolicyChangeHistory_EscalationPolicy_escalationPolicyId] FOREIGN KEY ([escalationPolicyId]) REFERENCES [Alerting].[EscalationPolicy] ([id])		-- Foreign key to the EscalationPolicy table.
)
GO

-- Index on the EscalationPolicyChangeHistory table's tenantGuid field.
CREATE INDEX [I_EscalationPolicyChangeHistory_tenantGuid] ON [Alerting].[EscalationPolicyChangeHistory] ([tenantGuid])
GO

-- Index on the EscalationPolicyChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_EscalationPolicyChangeHistory_tenantGuid_versionNumber] ON [Alerting].[EscalationPolicyChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the EscalationPolicyChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_EscalationPolicyChangeHistory_tenantGuid_timeStamp] ON [Alerting].[EscalationPolicyChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the EscalationPolicyChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_EscalationPolicyChangeHistory_tenantGuid_userId] ON [Alerting].[EscalationPolicyChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the EscalationPolicyChangeHistory table's tenantGuid,escalationPolicyId fields.
CREATE INDEX [I_EscalationPolicyChangeHistory_tenantGuid_escalationPolicyId] ON [Alerting].[EscalationPolicyChangeHistory] ([tenantGuid], [escalationPolicyId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Monitored services/applications that can generate alerts.
CREATE TABLE [Alerting].[Service]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[escalationPolicyId] INT NULL,		-- Link to the EscalationPolicy table.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[ownerTeamObjectGuid] UNIQUEIDENTIFIER NULL,		-- References Security.SecurityTeam.objectGuid - logical owner.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Service_EscalationPolicy_escalationPolicyId] FOREIGN KEY ([escalationPolicyId]) REFERENCES [Alerting].[EscalationPolicy] ([id]),		-- Foreign key to the EscalationPolicy table.
	CONSTRAINT [UC_Service_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Service table's tenantGuid and name fields.
)
GO

-- Index on the Service table's tenantGuid field.
CREATE INDEX [I_Service_tenantGuid] ON [Alerting].[Service] ([tenantGuid])
GO

-- Index on the Service table's tenantGuid,escalationPolicyId fields.
CREATE INDEX [I_Service_tenantGuid_escalationPolicyId] ON [Alerting].[Service] ([tenantGuid], [escalationPolicyId])
GO

-- Index on the Service table's tenantGuid,name fields.
CREATE INDEX [I_Service_tenantGuid_name] ON [Alerting].[Service] ([tenantGuid], [name])
GO

-- Index on the Service table's tenantGuid,active fields.
CREATE INDEX [I_Service_tenantGuid_active] ON [Alerting].[Service] ([tenantGuid], [active])
GO

-- Index on the Service table's tenantGuid,deleted fields.
CREATE INDEX [I_Service_tenantGuid_deleted] ON [Alerting].[Service] ([tenantGuid], [deleted])
GO


-- The change history for records from the Service table.
CREATE TABLE [Alerting].[ServiceChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[serviceId] INT NOT NULL,		-- Link to the Service table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ServiceChangeHistory_Service_serviceId] FOREIGN KEY ([serviceId]) REFERENCES [Alerting].[Service] ([id])		-- Foreign key to the Service table.
)
GO

-- Index on the ServiceChangeHistory table's tenantGuid field.
CREATE INDEX [I_ServiceChangeHistory_tenantGuid] ON [Alerting].[ServiceChangeHistory] ([tenantGuid])
GO

-- Index on the ServiceChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ServiceChangeHistory_tenantGuid_versionNumber] ON [Alerting].[ServiceChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ServiceChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ServiceChangeHistory_tenantGuid_timeStamp] ON [Alerting].[ServiceChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ServiceChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ServiceChangeHistory_tenantGuid_userId] ON [Alerting].[ServiceChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ServiceChangeHistory table's tenantGuid,serviceId fields.
CREATE INDEX [I_ServiceChangeHistory_tenantGuid_serviceId] ON [Alerting].[ServiceChangeHistory] ([tenantGuid], [serviceId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Individual escalation rules (ordered). Supports repeat looping until acknowledgment.
CREATE TABLE [Alerting].[EscalationRule]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[escalationPolicyId] INT NOT NULL,		-- Link to the EscalationPolicy table.
	[ruleOrder] INT NOT NULL DEFAULT 0,
	[delayMinutes] INT NOT NULL DEFAULT 0,
	[repeatCount] INT NOT NULL DEFAULT 0,		-- How many times to repeat notification if no ack (0 = no repeat).
	[repeatDelayMinutes] INT NULL,		-- Delay between repeat attempts (null = same as delayMinutes).
	[targetType] NVARCHAR(50) NOT NULL,		-- Valid values: User, Team, Schedule
	[targetObjectGuid] UNIQUEIDENTIFIER NULL,		-- References Security.SecurityUser/SecurityTeam or Alerting.OnCallSchedule objectGuid.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_EscalationRule_EscalationPolicy_escalationPolicyId] FOREIGN KEY ([escalationPolicyId]) REFERENCES [Alerting].[EscalationPolicy] ([id])		-- Foreign key to the EscalationPolicy table.
)
GO

-- Index on the EscalationRule table's tenantGuid field.
CREATE INDEX [I_EscalationRule_tenantGuid] ON [Alerting].[EscalationRule] ([tenantGuid])
GO

-- Index on the EscalationRule table's tenantGuid,escalationPolicyId fields.
CREATE INDEX [I_EscalationRule_tenantGuid_escalationPolicyId] ON [Alerting].[EscalationRule] ([tenantGuid], [escalationPolicyId])
GO

-- Index on the EscalationRule table's tenantGuid,ruleOrder fields.
CREATE INDEX [I_EscalationRule_tenantGuid_ruleOrder] ON [Alerting].[EscalationRule] ([tenantGuid], [ruleOrder])
GO

-- Index on the EscalationRule table's tenantGuid,active fields.
CREATE INDEX [I_EscalationRule_tenantGuid_active] ON [Alerting].[EscalationRule] ([tenantGuid], [active])
GO

-- Index on the EscalationRule table's tenantGuid,deleted fields.
CREATE INDEX [I_EscalationRule_tenantGuid_deleted] ON [Alerting].[EscalationRule] ([tenantGuid], [deleted])
GO

-- Index on the EscalationRule table's escalationPolicyId,ruleOrder fields.
CREATE INDEX [I_EscalationRule_escalationPolicyId_ruleOrder] ON [Alerting].[EscalationRule] ([escalationPolicyId], [ruleOrder])
GO


-- The change history for records from the EscalationRule table.
CREATE TABLE [Alerting].[EscalationRuleChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[escalationRuleId] INT NOT NULL,		-- Link to the EscalationRule table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_EscalationRuleChangeHistory_EscalationRule_escalationRuleId] FOREIGN KEY ([escalationRuleId]) REFERENCES [Alerting].[EscalationRule] ([id])		-- Foreign key to the EscalationRule table.
)
GO

-- Index on the EscalationRuleChangeHistory table's tenantGuid field.
CREATE INDEX [I_EscalationRuleChangeHistory_tenantGuid] ON [Alerting].[EscalationRuleChangeHistory] ([tenantGuid])
GO

-- Index on the EscalationRuleChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_EscalationRuleChangeHistory_tenantGuid_versionNumber] ON [Alerting].[EscalationRuleChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the EscalationRuleChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_EscalationRuleChangeHistory_tenantGuid_timeStamp] ON [Alerting].[EscalationRuleChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the EscalationRuleChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_EscalationRuleChangeHistory_tenantGuid_userId] ON [Alerting].[EscalationRuleChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the EscalationRuleChangeHistory table's tenantGuid,escalationRuleId fields.
CREATE INDEX [I_EscalationRuleChangeHistory_tenantGuid_escalationRuleId] ON [Alerting].[EscalationRuleChangeHistory] ([tenantGuid], [escalationRuleId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- On-call rotation schedules (dynamic targets for escalation rules).
CREATE TABLE [Alerting].[OnCallSchedule]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[timeZoneId] NVARCHAR(50) NOT NULL DEFAULT 'UTC',
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_OnCallSchedule_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the OnCallSchedule table's tenantGuid and name fields.
)
GO

-- Index on the OnCallSchedule table's tenantGuid field.
CREATE INDEX [I_OnCallSchedule_tenantGuid] ON [Alerting].[OnCallSchedule] ([tenantGuid])
GO

-- Index on the OnCallSchedule table's tenantGuid,name fields.
CREATE INDEX [I_OnCallSchedule_tenantGuid_name] ON [Alerting].[OnCallSchedule] ([tenantGuid], [name])
GO

-- Index on the OnCallSchedule table's tenantGuid,active fields.
CREATE INDEX [I_OnCallSchedule_tenantGuid_active] ON [Alerting].[OnCallSchedule] ([tenantGuid], [active])
GO

-- Index on the OnCallSchedule table's tenantGuid,deleted fields.
CREATE INDEX [I_OnCallSchedule_tenantGuid_deleted] ON [Alerting].[OnCallSchedule] ([tenantGuid], [deleted])
GO


-- The change history for records from the OnCallSchedule table.
CREATE TABLE [Alerting].[OnCallScheduleChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[onCallScheduleId] INT NOT NULL,		-- Link to the OnCallSchedule table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_OnCallScheduleChangeHistory_OnCallSchedule_onCallScheduleId] FOREIGN KEY ([onCallScheduleId]) REFERENCES [Alerting].[OnCallSchedule] ([id])		-- Foreign key to the OnCallSchedule table.
)
GO

-- Index on the OnCallScheduleChangeHistory table's tenantGuid field.
CREATE INDEX [I_OnCallScheduleChangeHistory_tenantGuid] ON [Alerting].[OnCallScheduleChangeHistory] ([tenantGuid])
GO

-- Index on the OnCallScheduleChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_OnCallScheduleChangeHistory_tenantGuid_versionNumber] ON [Alerting].[OnCallScheduleChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the OnCallScheduleChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_OnCallScheduleChangeHistory_tenantGuid_timeStamp] ON [Alerting].[OnCallScheduleChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the OnCallScheduleChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_OnCallScheduleChangeHistory_tenantGuid_userId] ON [Alerting].[OnCallScheduleChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the OnCallScheduleChangeHistory table's tenantGuid,onCallScheduleId fields.
CREATE INDEX [I_OnCallScheduleChangeHistory_tenantGuid_onCallScheduleId] ON [Alerting].[OnCallScheduleChangeHistory] ([tenantGuid], [onCallScheduleId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Layers within an on-call schedule (primary, secondary, etc.).
CREATE TABLE [Alerting].[ScheduleLayer]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[onCallScheduleId] INT NOT NULL,		-- Link to the OnCallSchedule table.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[layerLevel] INT NOT NULL DEFAULT 1,
	[rotationStart] DATETIME2(7) NOT NULL,
	[rotationDays] INT NOT NULL DEFAULT 7,
	[handoffTime] NVARCHAR(50) NOT NULL DEFAULT '09:00',
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ScheduleLayer_OnCallSchedule_onCallScheduleId] FOREIGN KEY ([onCallScheduleId]) REFERENCES [Alerting].[OnCallSchedule] ([id]),		-- Foreign key to the OnCallSchedule table.
	CONSTRAINT [UC_ScheduleLayer_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the ScheduleLayer table's tenantGuid and name fields.
)
GO

-- Index on the ScheduleLayer table's tenantGuid field.
CREATE INDEX [I_ScheduleLayer_tenantGuid] ON [Alerting].[ScheduleLayer] ([tenantGuid])
GO

-- Index on the ScheduleLayer table's tenantGuid,onCallScheduleId fields.
CREATE INDEX [I_ScheduleLayer_tenantGuid_onCallScheduleId] ON [Alerting].[ScheduleLayer] ([tenantGuid], [onCallScheduleId])
GO

-- Index on the ScheduleLayer table's tenantGuid,name fields.
CREATE INDEX [I_ScheduleLayer_tenantGuid_name] ON [Alerting].[ScheduleLayer] ([tenantGuid], [name])
GO

-- Index on the ScheduleLayer table's tenantGuid,active fields.
CREATE INDEX [I_ScheduleLayer_tenantGuid_active] ON [Alerting].[ScheduleLayer] ([tenantGuid], [active])
GO

-- Index on the ScheduleLayer table's tenantGuid,deleted fields.
CREATE INDEX [I_ScheduleLayer_tenantGuid_deleted] ON [Alerting].[ScheduleLayer] ([tenantGuid], [deleted])
GO


-- The change history for records from the ScheduleLayer table.
CREATE TABLE [Alerting].[ScheduleLayerChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduleLayerId] INT NOT NULL,		-- Link to the ScheduleLayer table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ScheduleLayerChangeHistory_ScheduleLayer_scheduleLayerId] FOREIGN KEY ([scheduleLayerId]) REFERENCES [Alerting].[ScheduleLayer] ([id])		-- Foreign key to the ScheduleLayer table.
)
GO

-- Index on the ScheduleLayerChangeHistory table's tenantGuid field.
CREATE INDEX [I_ScheduleLayerChangeHistory_tenantGuid] ON [Alerting].[ScheduleLayerChangeHistory] ([tenantGuid])
GO

-- Index on the ScheduleLayerChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ScheduleLayerChangeHistory_tenantGuid_versionNumber] ON [Alerting].[ScheduleLayerChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ScheduleLayerChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ScheduleLayerChangeHistory_tenantGuid_timeStamp] ON [Alerting].[ScheduleLayerChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ScheduleLayerChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ScheduleLayerChangeHistory_tenantGuid_userId] ON [Alerting].[ScheduleLayerChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ScheduleLayerChangeHistory table's tenantGuid,scheduleLayerId fields.
CREATE INDEX [I_ScheduleLayerChangeHistory_tenantGuid_scheduleLayerId] ON [Alerting].[ScheduleLayerChangeHistory] ([tenantGuid], [scheduleLayerId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Users in a schedule layer rotation (ordered).
CREATE TABLE [Alerting].[ScheduleLayerMember]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduleLayerId] INT NOT NULL,		-- Link to the ScheduleLayer table.
	[position] INT NOT NULL DEFAULT 0,
	[securityUserObjectGuid] UNIQUEIDENTIFIER NOT NULL,		-- References Security.SecurityUser.objectGuid
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ScheduleLayerMember_ScheduleLayer_scheduleLayerId] FOREIGN KEY ([scheduleLayerId]) REFERENCES [Alerting].[ScheduleLayer] ([id]),		-- Foreign key to the ScheduleLayer table.
	CONSTRAINT [UC_ScheduleLayerMember_tenantGuid_scheduleLayerId_position] UNIQUE ( [tenantGuid], [scheduleLayerId], [position]) 		-- Uniqueness enforced on the ScheduleLayerMember table's tenantGuid and scheduleLayerId and position fields.
)
GO

-- Index on the ScheduleLayerMember table's tenantGuid field.
CREATE INDEX [I_ScheduleLayerMember_tenantGuid] ON [Alerting].[ScheduleLayerMember] ([tenantGuid])
GO

-- Index on the ScheduleLayerMember table's tenantGuid,scheduleLayerId fields.
CREATE INDEX [I_ScheduleLayerMember_tenantGuid_scheduleLayerId] ON [Alerting].[ScheduleLayerMember] ([tenantGuid], [scheduleLayerId])
GO

-- Index on the ScheduleLayerMember table's tenantGuid,active fields.
CREATE INDEX [I_ScheduleLayerMember_tenantGuid_active] ON [Alerting].[ScheduleLayerMember] ([tenantGuid], [active])
GO

-- Index on the ScheduleLayerMember table's tenantGuid,deleted fields.
CREATE INDEX [I_ScheduleLayerMember_tenantGuid_deleted] ON [Alerting].[ScheduleLayerMember] ([tenantGuid], [deleted])
GO


-- The change history for records from the ScheduleLayerMember table.
CREATE TABLE [Alerting].[ScheduleLayerMemberChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduleLayerMemberId] INT NOT NULL,		-- Link to the ScheduleLayerMember table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ScheduleLayerMemberChangeHistory_ScheduleLayerMember_scheduleLayerMemberId] FOREIGN KEY ([scheduleLayerMemberId]) REFERENCES [Alerting].[ScheduleLayerMember] ([id])		-- Foreign key to the ScheduleLayerMember table.
)
GO

-- Index on the ScheduleLayerMemberChangeHistory table's tenantGuid field.
CREATE INDEX [I_ScheduleLayerMemberChangeHistory_tenantGuid] ON [Alerting].[ScheduleLayerMemberChangeHistory] ([tenantGuid])
GO

-- Index on the ScheduleLayerMemberChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ScheduleLayerMemberChangeHistory_tenantGuid_versionNumber] ON [Alerting].[ScheduleLayerMemberChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ScheduleLayerMemberChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ScheduleLayerMemberChangeHistory_tenantGuid_timeStamp] ON [Alerting].[ScheduleLayerMemberChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ScheduleLayerMemberChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ScheduleLayerMemberChangeHistory_tenantGuid_userId] ON [Alerting].[ScheduleLayerMemberChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ScheduleLayerMemberChangeHistory table's tenantGuid,scheduleLayerMemberId fields.
CREATE INDEX [I_ScheduleLayerMemberChangeHistory_tenantGuid_scheduleLayerMemberId] ON [Alerting].[ScheduleLayerMemberChangeHistory] ([tenantGuid], [scheduleLayerMemberId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Temporary overrides for on-call schedules (vacations, swaps, emergency substitutions).
CREATE TABLE [Alerting].[ScheduleOverride]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[onCallScheduleId] INT NOT NULL,		-- Link to the OnCallSchedule table.
	[scheduleLayerId] INT NULL,		-- If null, override applies to all layers in the schedule.
	[startDateTime] DATETIME2(7) NOT NULL,		-- Start of override period (inclusive).
	[endDateTime] DATETIME2(7) NOT NULL,		-- End of override period (exclusive).
	[scheduleOverrideTypeId] INT NOT NULL,		-- The type of override.  Will be one of Swap, Replace, or Remove
	[originalUserObjectGuid] UNIQUEIDENTIFIER NULL,		-- The user being replaced (null for layer-wide overrides).
	[replacementUserObjectGuid] UNIQUEIDENTIFIER NULL,		-- The substitute user (null for REMOVE type).
	[reason] NVARCHAR(500) NULL,		-- Optional explanation (vacation, sick, training, etc.).
	[createdByUserObjectGuid] UNIQUEIDENTIFIER NOT NULL,		-- References Security.SecurityUser.objectGuid - who created the override.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ScheduleOverride_OnCallSchedule_onCallScheduleId] FOREIGN KEY ([onCallScheduleId]) REFERENCES [Alerting].[OnCallSchedule] ([id]),		-- Foreign key to the OnCallSchedule table.
	CONSTRAINT [FK_ScheduleOverride_ScheduleLayer_scheduleLayerId] FOREIGN KEY ([scheduleLayerId]) REFERENCES [Alerting].[ScheduleLayer] ([id]),		-- Foreign key to the ScheduleLayer table.
	CONSTRAINT [FK_ScheduleOverride_ScheduleOverrideType_scheduleOverrideTypeId] FOREIGN KEY ([scheduleOverrideTypeId]) REFERENCES [Alerting].[ScheduleOverrideType] ([id])		-- Foreign key to the ScheduleOverrideType table.
)
GO

-- Index on the ScheduleOverride table's tenantGuid field.
CREATE INDEX [I_ScheduleOverride_tenantGuid] ON [Alerting].[ScheduleOverride] ([tenantGuid])
GO

-- Index on the ScheduleOverride table's tenantGuid,onCallScheduleId fields.
CREATE INDEX [I_ScheduleOverride_tenantGuid_onCallScheduleId] ON [Alerting].[ScheduleOverride] ([tenantGuid], [onCallScheduleId])
GO

-- Index on the ScheduleOverride table's tenantGuid,scheduleLayerId fields.
CREATE INDEX [I_ScheduleOverride_tenantGuid_scheduleLayerId] ON [Alerting].[ScheduleOverride] ([tenantGuid], [scheduleLayerId])
GO

-- Index on the ScheduleOverride table's tenantGuid,scheduleOverrideTypeId fields.
CREATE INDEX [I_ScheduleOverride_tenantGuid_scheduleOverrideTypeId] ON [Alerting].[ScheduleOverride] ([tenantGuid], [scheduleOverrideTypeId])
GO

-- Index on the ScheduleOverride table's tenantGuid,active fields.
CREATE INDEX [I_ScheduleOverride_tenantGuid_active] ON [Alerting].[ScheduleOverride] ([tenantGuid], [active])
GO

-- Index on the ScheduleOverride table's tenantGuid,deleted fields.
CREATE INDEX [I_ScheduleOverride_tenantGuid_deleted] ON [Alerting].[ScheduleOverride] ([tenantGuid], [deleted])
GO

-- Index on the ScheduleOverride table's tenantGuid,onCallScheduleId,startDateTime,endDateTime fields.
CREATE INDEX [I_ScheduleOverride_tenantGuid_onCallScheduleId_startDateTime_endDateTime] ON [Alerting].[ScheduleOverride] ([tenantGuid], [onCallScheduleId], [startDateTime], [endDateTime])
GO

-- Index on the ScheduleOverride table's tenantGuid,originalUserObjectGuid fields.
CREATE INDEX [I_ScheduleOverride_tenantGuid_originalUserObjectGuid] ON [Alerting].[ScheduleOverride] ([tenantGuid], [originalUserObjectGuid])
GO


-- The change history for records from the ScheduleOverride table.
CREATE TABLE [Alerting].[ScheduleOverrideChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[scheduleOverrideId] INT NOT NULL,		-- Link to the ScheduleOverride table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ScheduleOverrideChangeHistory_ScheduleOverride_scheduleOverrideId] FOREIGN KEY ([scheduleOverrideId]) REFERENCES [Alerting].[ScheduleOverride] ([id])		-- Foreign key to the ScheduleOverride table.
)
GO

-- Index on the ScheduleOverrideChangeHistory table's tenantGuid field.
CREATE INDEX [I_ScheduleOverrideChangeHistory_tenantGuid] ON [Alerting].[ScheduleOverrideChangeHistory] ([tenantGuid])
GO

-- Index on the ScheduleOverrideChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ScheduleOverrideChangeHistory_tenantGuid_versionNumber] ON [Alerting].[ScheduleOverrideChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ScheduleOverrideChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ScheduleOverrideChangeHistory_tenantGuid_timeStamp] ON [Alerting].[ScheduleOverrideChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ScheduleOverrideChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ScheduleOverrideChangeHistory_tenantGuid_userId] ON [Alerting].[ScheduleOverrideChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ScheduleOverrideChangeHistory table's tenantGuid,scheduleOverrideId fields.
CREATE INDEX [I_ScheduleOverrideChangeHistory_tenantGuid_scheduleOverrideId] ON [Alerting].[ScheduleOverrideChangeHistory] ([tenantGuid], [scheduleOverrideId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- API integrations for inbound alerts and outbound status callbacks.
CREATE TABLE [Alerting].[Integration]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[serviceId] INT NOT NULL,		-- Link to the Service table.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NULL,
	[apiKeyHash] NVARCHAR(250) NOT NULL UNIQUE,
	[callbackWebhookUrl] NVARCHAR(1000) NULL,
	[maxRetryAttempts] INT NULL DEFAULT 10,		-- How many times to retry failed deliveries
	[retryBackoffSeconds] INT NULL DEFAULT 30,		-- Base seconds for backoff (30, 60, 120, 240...)
	[lastCallbackSuccessAt] DATETIME2(7) NULL,
	[consecutiveCallbackFailures] INT NULL,
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Integration_Service_serviceId] FOREIGN KEY ([serviceId]) REFERENCES [Alerting].[Service] ([id]),		-- Foreign key to the Service table.
	CONSTRAINT [UC_Integration_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Integration table's tenantGuid and name fields.
)
GO

-- Index on the Integration table's tenantGuid field.
CREATE INDEX [I_Integration_tenantGuid] ON [Alerting].[Integration] ([tenantGuid])
GO

-- Index on the Integration table's tenantGuid,serviceId fields.
CREATE INDEX [I_Integration_tenantGuid_serviceId] ON [Alerting].[Integration] ([tenantGuid], [serviceId])
GO

-- Index on the Integration table's tenantGuid,name fields.
CREATE INDEX [I_Integration_tenantGuid_name] ON [Alerting].[Integration] ([tenantGuid], [name])
GO

-- Index on the Integration table's tenantGuid,active fields.
CREATE INDEX [I_Integration_tenantGuid_active] ON [Alerting].[Integration] ([tenantGuid], [active])
GO

-- Index on the Integration table's tenantGuid,deleted fields.
CREATE INDEX [I_Integration_tenantGuid_deleted] ON [Alerting].[Integration] ([tenantGuid], [deleted])
GO


-- The change history for records from the Integration table.
CREATE TABLE [Alerting].[IntegrationChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[integrationId] INT NOT NULL,		-- Link to the Integration table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_IntegrationChangeHistory_Integration_integrationId] FOREIGN KEY ([integrationId]) REFERENCES [Alerting].[Integration] ([id])		-- Foreign key to the Integration table.
)
GO

-- Index on the IntegrationChangeHistory table's tenantGuid field.
CREATE INDEX [I_IntegrationChangeHistory_tenantGuid] ON [Alerting].[IntegrationChangeHistory] ([tenantGuid])
GO

-- Index on the IntegrationChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_IntegrationChangeHistory_tenantGuid_versionNumber] ON [Alerting].[IntegrationChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the IntegrationChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_IntegrationChangeHistory_tenantGuid_timeStamp] ON [Alerting].[IntegrationChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the IntegrationChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_IntegrationChangeHistory_tenantGuid_userId] ON [Alerting].[IntegrationChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the IntegrationChangeHistory table's tenantGuid,integrationId fields.
CREATE INDEX [I_IntegrationChangeHistory_tenantGuid_integrationId] ON [Alerting].[IntegrationChangeHistory] ([tenantGuid], [integrationId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- API integrations incident event types to callback on.
CREATE TABLE [Alerting].[IntegrationCallbackIncidentEventType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[integrationId] INT NOT NULL,		-- Link to the Integration table.
	[incidentEventTypeId] INT NOT NULL,		-- Link to the IncidentEventType table.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_IntegrationCallbackIncidentEventType_Integration_integrationId] FOREIGN KEY ([integrationId]) REFERENCES [Alerting].[Integration] ([id]),		-- Foreign key to the Integration table.
	CONSTRAINT [FK_IntegrationCallbackIncidentEventType_IncidentEventType_incidentEventTypeId] FOREIGN KEY ([incidentEventTypeId]) REFERENCES [Alerting].[IncidentEventType] ([id])		-- Foreign key to the IncidentEventType table.
)
GO

-- Index on the IntegrationCallbackIncidentEventType table's tenantGuid field.
CREATE INDEX [I_IntegrationCallbackIncidentEventType_tenantGuid] ON [Alerting].[IntegrationCallbackIncidentEventType] ([tenantGuid])
GO

-- Index on the IntegrationCallbackIncidentEventType table's tenantGuid,integrationId fields.
CREATE INDEX [I_IntegrationCallbackIncidentEventType_tenantGuid_integrationId] ON [Alerting].[IntegrationCallbackIncidentEventType] ([tenantGuid], [integrationId])
GO

-- Index on the IntegrationCallbackIncidentEventType table's tenantGuid,incidentEventTypeId fields.
CREATE INDEX [I_IntegrationCallbackIncidentEventType_tenantGuid_incidentEventTypeId] ON [Alerting].[IntegrationCallbackIncidentEventType] ([tenantGuid], [incidentEventTypeId])
GO

-- Index on the IntegrationCallbackIncidentEventType table's tenantGuid,active fields.
CREATE INDEX [I_IntegrationCallbackIncidentEventType_tenantGuid_active] ON [Alerting].[IntegrationCallbackIncidentEventType] ([tenantGuid], [active])
GO

-- Index on the IntegrationCallbackIncidentEventType table's tenantGuid,deleted fields.
CREATE INDEX [I_IntegrationCallbackIncidentEventType_tenantGuid_deleted] ON [Alerting].[IntegrationCallbackIncidentEventType] ([tenantGuid], [deleted])
GO


-- The change history for records from the IntegrationCallbackIncidentEventType table.
CREATE TABLE [Alerting].[IntegrationCallbackIncidentEventTypeChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[integrationCallbackIncidentEventTypeId] INT NOT NULL,		-- Link to the IntegrationCallbackIncidentEventType table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_IntegrationCallbackIncidentEventTypeChangeHistory_IntegrationCallbackIncidentEventType_integrationCallbackIncidentEventTypeId] FOREIGN KEY ([integrationCallbackIncidentEventTypeId]) REFERENCES [Alerting].[IntegrationCallbackIncidentEventType] ([id])		-- Foreign key to the IntegrationCallbackIncidentEventType table.
)
GO

-- Index on the IntegrationCallbackIncidentEventTypeChangeHistory table's tenantGuid field.
CREATE INDEX [I_IntegrationCallbackIncidentEventTypeChangeHistory_tenantGuid] ON [Alerting].[IntegrationCallbackIncidentEventTypeChangeHistory] ([tenantGuid])
GO

-- Index on the IntegrationCallbackIncidentEventTypeChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_IntegrationCallbackIncidentEventTypeChangeHistory_tenantGuid_versionNumber] ON [Alerting].[IntegrationCallbackIncidentEventTypeChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the IntegrationCallbackIncidentEventTypeChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_IntegrationCallbackIncidentEventTypeChangeHistory_tenantGuid_timeStamp] ON [Alerting].[IntegrationCallbackIncidentEventTypeChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the IntegrationCallbackIncidentEventTypeChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_IntegrationCallbackIncidentEventTypeChangeHistory_tenantGuid_userId] ON [Alerting].[IntegrationCallbackIncidentEventTypeChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the IntegrationCallbackIncidentEventTypeChangeHistory table's tenantGuid,integrationCallbackIncidentEventTypeId fields.
CREATE INDEX [I_IntegrationCallbackIncidentEventTypeChangeHistory_tenantGuid_integrationCallbackIncidentEventTypeId] ON [Alerting].[IntegrationCallbackIncidentEventTypeChangeHistory] ([tenantGuid], [integrationCallbackIncidentEventTypeId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Per-user notification preferences (channels, quiet hours, DND, etc.). Users can edit their own preferences.
CREATE TABLE [Alerting].[UserNotificationPreference]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[securityUserObjectGuid] UNIQUEIDENTIFIER NOT NULL,		-- References Security.SecurityUser.objectGuid - one preference row per user.
	[timeZoneId] NVARCHAR(50) NULL DEFAULT 'UTC',		-- User's preferred timezone for quiet hours scheduling.
	[quietHoursStart] NVARCHAR(10) NULL,		-- HH:mm format local to timeZoneId - start of quiet hours (null = no quiet hours).
	[quietHoursEnd] NVARCHAR(10) NULL,		-- HH:mm format local to timeZoneId - end of quiet hours.
	[isDoNotDisturb] BIT NOT NULL DEFAULT 0,		-- Global DND override - if true, no notifications except possibly critical overrides.
	[isDoNotDisturbPermanent] BIT NOT NULL DEFAULT 0,		-- If true, DND has no scheduled end (until manually cleared).
	[doNotDisturbUntil] DATETIME2(7) NULL,		-- Temporary DND end time (ignored if isDoNotDisturbPermanent = true).
	[customSettingsJson] NVARCHAR(MAX) NULL,		-- Flexible JSON for future extensions (e.g., per-severity overrides, custom sounds).
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_UserNotificationPreference_tenantGuid_securityUserObjectGuid_active_deleted] UNIQUE ( [tenantGuid], [securityUserObjectGuid], [active], [deleted]) 		-- Uniqueness enforced on the UserNotificationPreference table's tenantGuid and securityUserObjectGuid and active and deleted fields.
)
GO

-- Index on the UserNotificationPreference table's tenantGuid field.
CREATE INDEX [I_UserNotificationPreference_tenantGuid] ON [Alerting].[UserNotificationPreference] ([tenantGuid])
GO

-- Index on the UserNotificationPreference table's tenantGuid,active fields.
CREATE INDEX [I_UserNotificationPreference_tenantGuid_active] ON [Alerting].[UserNotificationPreference] ([tenantGuid], [active])
GO

-- Index on the UserNotificationPreference table's tenantGuid,deleted fields.
CREATE INDEX [I_UserNotificationPreference_tenantGuid_deleted] ON [Alerting].[UserNotificationPreference] ([tenantGuid], [deleted])
GO

-- Index on the UserNotificationPreference table's securityUserObjectGuid field.
CREATE INDEX [I_UserNotificationPreference_securityUserObjectGuid] ON [Alerting].[UserNotificationPreference] ([securityUserObjectGuid])
GO


-- The change history for records from the UserNotificationPreference table.
CREATE TABLE [Alerting].[UserNotificationPreferenceChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userNotificationPreferenceId] INT NOT NULL,		-- Link to the UserNotificationPreference table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_UserNotificationPreferenceChangeHistory_UserNotificationPreference_userNotificationPreferenceId] FOREIGN KEY ([userNotificationPreferenceId]) REFERENCES [Alerting].[UserNotificationPreference] ([id])		-- Foreign key to the UserNotificationPreference table.
)
GO

-- Index on the UserNotificationPreferenceChangeHistory table's tenantGuid field.
CREATE INDEX [I_UserNotificationPreferenceChangeHistory_tenantGuid] ON [Alerting].[UserNotificationPreferenceChangeHistory] ([tenantGuid])
GO

-- Index on the UserNotificationPreferenceChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_UserNotificationPreferenceChangeHistory_tenantGuid_versionNumber] ON [Alerting].[UserNotificationPreferenceChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the UserNotificationPreferenceChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_UserNotificationPreferenceChangeHistory_tenantGuid_timeStamp] ON [Alerting].[UserNotificationPreferenceChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the UserNotificationPreferenceChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_UserNotificationPreferenceChangeHistory_tenantGuid_userId] ON [Alerting].[UserNotificationPreferenceChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the UserNotificationPreferenceChangeHistory table's tenantGuid,userNotificationPreferenceId fields.
CREATE INDEX [I_UserNotificationPreferenceChangeHistory_tenantGuid_userNotificationPreferenceId] ON [Alerting].[UserNotificationPreferenceChangeHistory] ([tenantGuid], [userNotificationPreferenceId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Per-user, per-channel notification preferences (enable/disable, custom priority).
CREATE TABLE [Alerting].[UserNotificationChannelPreference]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userNotificationPreferenceId] INT NOT NULL,		-- Link to the UserNotificationPreference table.
	[notificationChannelTypeId] INT NOT NULL,		-- Link to the NotificationChannelType table.
	[isEnabled] BIT NOT NULL DEFAULT 1,		-- If false, this channel is disabled for the user (overrides system defaults).
	[priorityOverride] INT NULL,		-- Optional custom priority (lower = higher urgency) - null = use channel default.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_UserNotificationChannelPreference_UserNotificationPreference_userNotificationPreferenceId] FOREIGN KEY ([userNotificationPreferenceId]) REFERENCES [Alerting].[UserNotificationPreference] ([id]),		-- Foreign key to the UserNotificationPreference table.
	CONSTRAINT [FK_UserNotificationChannelPreference_NotificationChannelType_notificationChannelTypeId] FOREIGN KEY ([notificationChannelTypeId]) REFERENCES [Alerting].[NotificationChannelType] ([id]),		-- Foreign key to the NotificationChannelType table.
	CONSTRAINT [UC_UserNotificationChannelPreference_tenantGuid_userNotificationPreferenceId_notificationChannelTypeId] UNIQUE ( [tenantGuid], [userNotificationPreferenceId], [notificationChannelTypeId]) 		-- Uniqueness enforced on the UserNotificationChannelPreference table's tenantGuid and userNotificationPreferenceId and notificationChannelTypeId fields.
)
GO

-- Index on the UserNotificationChannelPreference table's tenantGuid field.
CREATE INDEX [I_UserNotificationChannelPreference_tenantGuid] ON [Alerting].[UserNotificationChannelPreference] ([tenantGuid])
GO

-- Index on the UserNotificationChannelPreference table's tenantGuid,userNotificationPreferenceId fields.
CREATE INDEX [I_UserNotificationChannelPreference_tenantGuid_userNotificationPreferenceId] ON [Alerting].[UserNotificationChannelPreference] ([tenantGuid], [userNotificationPreferenceId])
GO

-- Index on the UserNotificationChannelPreference table's tenantGuid,notificationChannelTypeId fields.
CREATE INDEX [I_UserNotificationChannelPreference_tenantGuid_notificationChannelTypeId] ON [Alerting].[UserNotificationChannelPreference] ([tenantGuid], [notificationChannelTypeId])
GO

-- Index on the UserNotificationChannelPreference table's tenantGuid,active fields.
CREATE INDEX [I_UserNotificationChannelPreference_tenantGuid_active] ON [Alerting].[UserNotificationChannelPreference] ([tenantGuid], [active])
GO

-- Index on the UserNotificationChannelPreference table's tenantGuid,deleted fields.
CREATE INDEX [I_UserNotificationChannelPreference_tenantGuid_deleted] ON [Alerting].[UserNotificationChannelPreference] ([tenantGuid], [deleted])
GO


-- The change history for records from the UserNotificationChannelPreference table.
CREATE TABLE [Alerting].[UserNotificationChannelPreferenceChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userNotificationChannelPreferenceId] INT NOT NULL,		-- Link to the UserNotificationChannelPreference table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_UserNotificationChannelPreferenceChangeHistory_UserNotificationChannelPreference_userNotificationChannelPreferenceId] FOREIGN KEY ([userNotificationChannelPreferenceId]) REFERENCES [Alerting].[UserNotificationChannelPreference] ([id])		-- Foreign key to the UserNotificationChannelPreference table.
)
GO

-- Index on the UserNotificationChannelPreferenceChangeHistory table's tenantGuid field.
CREATE INDEX [I_UserNotificationChannelPreferenceChangeHistory_tenantGuid] ON [Alerting].[UserNotificationChannelPreferenceChangeHistory] ([tenantGuid])
GO

-- Index on the UserNotificationChannelPreferenceChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_UserNotificationChannelPreferenceChangeHistory_tenantGuid_versionNumber] ON [Alerting].[UserNotificationChannelPreferenceChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the UserNotificationChannelPreferenceChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_UserNotificationChannelPreferenceChangeHistory_tenantGuid_timeStamp] ON [Alerting].[UserNotificationChannelPreferenceChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the UserNotificationChannelPreferenceChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_UserNotificationChannelPreferenceChangeHistory_tenantGuid_userId] ON [Alerting].[UserNotificationChannelPreferenceChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the UserNotificationChannelPreferenceChangeHistory table's tenantGuid,userNotificationChannelPreferenceId fields.
CREATE INDEX [I_UserNotificationChannelPreferenceChangeHistory_tenantGuid_userNotificationChannelPreferenceId] ON [Alerting].[UserNotificationChannelPreferenceChangeHistory] ([tenantGuid], [userNotificationChannelPreferenceId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Push notification tokens for web and mobile devices. Each user can have multiple tokens (one per device).
CREATE TABLE [Alerting].[UserPushToken]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userObjectGuid] UNIQUEIDENTIFIER NOT NULL,		-- References Security.SecurityUser.objectGuid - the token owner.
	[fcmToken] NVARCHAR(500) NOT NULL,		-- Firebase Cloud Messaging token for this device.
	[deviceFingerprint] NVARCHAR(100) NOT NULL,		-- Unique identifier for the device/browser to prevent duplicates.
	[platform] NVARCHAR(50) NOT NULL DEFAULT 'web',		-- Platform: 'web', 'ios', 'android'.
	[userAgent] NVARCHAR(500) NULL,		-- Browser/device user agent string for diagnostics.
	[registeredAt] DATETIME2(7) NOT NULL,		-- When the token was first registered.
	[lastUpdatedAt] DATETIME2(7) NOT NULL,		-- Last time the token was refreshed.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_UserPushToken_tenantGuid_userObjectGuid_deviceFingerprint] UNIQUE ( [tenantGuid], [userObjectGuid], [deviceFingerprint]) 		-- Uniqueness enforced on the UserPushToken table's tenantGuid and userObjectGuid and deviceFingerprint fields.
)
GO

-- Index on the UserPushToken table's tenantGuid field.
CREATE INDEX [I_UserPushToken_tenantGuid] ON [Alerting].[UserPushToken] ([tenantGuid])
GO

-- Index on the UserPushToken table's tenantGuid,active fields.
CREATE INDEX [I_UserPushToken_tenantGuid_active] ON [Alerting].[UserPushToken] ([tenantGuid], [active])
GO

-- Index on the UserPushToken table's tenantGuid,deleted fields.
CREATE INDEX [I_UserPushToken_tenantGuid_deleted] ON [Alerting].[UserPushToken] ([tenantGuid], [deleted])
GO

-- Index on the UserPushToken table's tenantGuid,userObjectGuid fields.
CREATE INDEX [I_UserPushToken_tenantGuid_userObjectGuid] ON [Alerting].[UserPushToken] ([tenantGuid], [userObjectGuid])
GO


-- The change history for records from the UserPushToken table.
CREATE TABLE [Alerting].[UserPushTokenChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userPushTokenId] INT NOT NULL,		-- Link to the UserPushToken table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_UserPushTokenChangeHistory_UserPushToken_userPushTokenId] FOREIGN KEY ([userPushTokenId]) REFERENCES [Alerting].[UserPushToken] ([id])		-- Foreign key to the UserPushToken table.
)
GO

-- Index on the UserPushTokenChangeHistory table's tenantGuid field.
CREATE INDEX [I_UserPushTokenChangeHistory_tenantGuid] ON [Alerting].[UserPushTokenChangeHistory] ([tenantGuid])
GO

-- Index on the UserPushTokenChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_UserPushTokenChangeHistory_tenantGuid_versionNumber] ON [Alerting].[UserPushTokenChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the UserPushTokenChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_UserPushTokenChangeHistory_tenantGuid_timeStamp] ON [Alerting].[UserPushTokenChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the UserPushTokenChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_UserPushTokenChangeHistory_tenantGuid_userId] ON [Alerting].[UserPushTokenChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the UserPushTokenChangeHistory table's tenantGuid,userPushTokenId fields.
CREATE INDEX [I_UserPushTokenChangeHistory_tenantGuid_userPushTokenId] ON [Alerting].[UserPushTokenChangeHistory] ([tenantGuid], [userPushTokenId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Active and historical incidents.
CREATE TABLE [Alerting].[Incident]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[incidentKey] NVARCHAR(250) NOT NULL,
	[serviceId] INT NOT NULL,		-- Link to the Service table.
	[title] NVARCHAR(250) NOT NULL,
	[description] NVARCHAR(MAX) NULL,
	[severityTypeId] INT NOT NULL,		-- Link to the SeverityType table.
	[incidentStatusTypeId] INT NOT NULL,		-- Link to the IncidentStatusType table.
	[createdAt] DATETIME2(7) NOT NULL,
	[escalationRuleId] INT NULL,		-- Current active escalation rule (null = no active escalation, e.g., acknowledged/resolved).
	[currentRepeatCount] INT NULL DEFAULT 0,		-- How many repeat notifications have been sent for the current rule (resets on rule change).
	[nextEscalationAt] DATETIME2(7) NULL,		-- Timestamp when the next escalation/repeat should fire (drives worker query).
	[acknowledgedAt] DATETIME2(7) NULL,
	[resolvedAt] DATETIME2(7) NULL,
	[currentAssigneeObjectGuid] UNIQUEIDENTIFIER NULL,
	[sourcePayloadJson] NVARCHAR(MAX) NULL,
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Incident_Service_serviceId] FOREIGN KEY ([serviceId]) REFERENCES [Alerting].[Service] ([id]),		-- Foreign key to the Service table.
	CONSTRAINT [FK_Incident_SeverityType_severityTypeId] FOREIGN KEY ([severityTypeId]) REFERENCES [Alerting].[SeverityType] ([id]),		-- Foreign key to the SeverityType table.
	CONSTRAINT [FK_Incident_IncidentStatusType_incidentStatusTypeId] FOREIGN KEY ([incidentStatusTypeId]) REFERENCES [Alerting].[IncidentStatusType] ([id]),		-- Foreign key to the IncidentStatusType table.
	CONSTRAINT [FK_Incident_EscalationRule_escalationRuleId] FOREIGN KEY ([escalationRuleId]) REFERENCES [Alerting].[EscalationRule] ([id]),		-- Foreign key to the EscalationRule table.
	CONSTRAINT [UC_Incident_tenantGuid_incidentKey] UNIQUE ( [tenantGuid], [incidentKey]) 		-- Uniqueness enforced on the Incident table's tenantGuid and incidentKey fields.
)
GO

-- Index on the Incident table's tenantGuid field.
CREATE INDEX [I_Incident_tenantGuid] ON [Alerting].[Incident] ([tenantGuid])
GO

-- Index on the Incident table's tenantGuid,incidentKey fields.
CREATE INDEX [I_Incident_tenantGuid_incidentKey] ON [Alerting].[Incident] ([tenantGuid], [incidentKey])
GO

-- Index on the Incident table's tenantGuid,serviceId fields.
CREATE INDEX [I_Incident_tenantGuid_serviceId] ON [Alerting].[Incident] ([tenantGuid], [serviceId])
GO

-- Index on the Incident table's tenantGuid,severityTypeId fields.
CREATE INDEX [I_Incident_tenantGuid_severityTypeId] ON [Alerting].[Incident] ([tenantGuid], [severityTypeId])
GO

-- Index on the Incident table's tenantGuid,incidentStatusTypeId fields.
CREATE INDEX [I_Incident_tenantGuid_incidentStatusTypeId] ON [Alerting].[Incident] ([tenantGuid], [incidentStatusTypeId])
GO

-- Index on the Incident table's tenantGuid,createdAt fields.
CREATE INDEX [I_Incident_tenantGuid_createdAt] ON [Alerting].[Incident] ([tenantGuid], [createdAt])
GO

-- Index on the Incident table's tenantGuid,escalationRuleId fields.
CREATE INDEX [I_Incident_tenantGuid_escalationRuleId] ON [Alerting].[Incident] ([tenantGuid], [escalationRuleId])
GO

-- Index on the Incident table's tenantGuid,active fields.
CREATE INDEX [I_Incident_tenantGuid_active] ON [Alerting].[Incident] ([tenantGuid], [active])
GO

-- Index on the Incident table's tenantGuid,deleted fields.
CREATE INDEX [I_Incident_tenantGuid_deleted] ON [Alerting].[Incident] ([tenantGuid], [deleted])
GO

-- Index on the Incident table's tenantGuid,incidentStatusTypeId,createdAt fields.
CREATE INDEX [I_Incident_tenantGuid_incidentStatusTypeId_createdAt] ON [Alerting].[Incident] ([tenantGuid], [incidentStatusTypeId], [createdAt])
GO

-- Index on the Incident table's tenantGuid,serviceId,createdAt fields.
CREATE INDEX [I_Incident_tenantGuid_serviceId_createdAt] ON [Alerting].[Incident] ([tenantGuid], [serviceId], [createdAt])
GO

-- Index on the Incident table's tenantGuid,nextEscalationAt,incidentStatusTypeId fields.
CREATE INDEX [I_Incident_tenantGuid_nextEscalationAt_incidentStatusTypeId] ON [Alerting].[Incident] ([tenantGuid], [nextEscalationAt], [incidentStatusTypeId])
GO


-- The change history for records from the Incident table.
CREATE TABLE [Alerting].[IncidentChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[incidentId] INT NOT NULL,		-- Link to the Incident table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_IncidentChangeHistory_Incident_incidentId] FOREIGN KEY ([incidentId]) REFERENCES [Alerting].[Incident] ([id])		-- Foreign key to the Incident table.
)
GO

-- Index on the IncidentChangeHistory table's tenantGuid field.
CREATE INDEX [I_IncidentChangeHistory_tenantGuid] ON [Alerting].[IncidentChangeHistory] ([tenantGuid])
GO

-- Index on the IncidentChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_IncidentChangeHistory_tenantGuid_versionNumber] ON [Alerting].[IncidentChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the IncidentChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_IncidentChangeHistory_tenantGuid_timeStamp] ON [Alerting].[IncidentChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the IncidentChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_IncidentChangeHistory_tenantGuid_userId] ON [Alerting].[IncidentChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the IncidentChangeHistory table's tenantGuid,incidentId fields.
CREATE INDEX [I_IncidentChangeHistory_tenantGuid_incidentId] ON [Alerting].[IncidentChangeHistory] ([tenantGuid], [incidentId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Timeline events for incidents.
CREATE TABLE [Alerting].[IncidentTimelineEvent]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[incidentId] INT NOT NULL,		-- Link to the Incident table.
	[incidentEventTypeId] INT NOT NULL,		-- Link to the IncidentEventType table.
	[timestamp] DATETIME2(7) NOT NULL,
	[actorObjectGuid] UNIQUEIDENTIFIER NULL,
	[detailsJson] NVARCHAR(MAX) NULL,
	[notes] NVARCHAR(500) NULL,		-- Human-readable context for this event (e.g., 'Escalation rule 1 fired - notifying on-call schedule').
	[source] NVARCHAR(50) NULL DEFAULT 'system',		-- Event source: 'system', 'user', 'api', 'webhook'.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_IncidentTimelineEvent_Incident_incidentId] FOREIGN KEY ([incidentId]) REFERENCES [Alerting].[Incident] ([id]),		-- Foreign key to the Incident table.
	CONSTRAINT [FK_IncidentTimelineEvent_IncidentEventType_incidentEventTypeId] FOREIGN KEY ([incidentEventTypeId]) REFERENCES [Alerting].[IncidentEventType] ([id])		-- Foreign key to the IncidentEventType table.
)
GO

-- Index on the IncidentTimelineEvent table's tenantGuid field.
CREATE INDEX [I_IncidentTimelineEvent_tenantGuid] ON [Alerting].[IncidentTimelineEvent] ([tenantGuid])
GO

-- Index on the IncidentTimelineEvent table's tenantGuid,incidentId fields.
CREATE INDEX [I_IncidentTimelineEvent_tenantGuid_incidentId] ON [Alerting].[IncidentTimelineEvent] ([tenantGuid], [incidentId])
GO

-- Index on the IncidentTimelineEvent table's tenantGuid,incidentEventTypeId fields.
CREATE INDEX [I_IncidentTimelineEvent_tenantGuid_incidentEventTypeId] ON [Alerting].[IncidentTimelineEvent] ([tenantGuid], [incidentEventTypeId])
GO

-- Index on the IncidentTimelineEvent table's tenantGuid,timestamp fields.
CREATE INDEX [I_IncidentTimelineEvent_tenantGuid_timestamp] ON [Alerting].[IncidentTimelineEvent] ([tenantGuid], [timestamp])
GO

-- Index on the IncidentTimelineEvent table's tenantGuid,active fields.
CREATE INDEX [I_IncidentTimelineEvent_tenantGuid_active] ON [Alerting].[IncidentTimelineEvent] ([tenantGuid], [active])
GO

-- Index on the IncidentTimelineEvent table's tenantGuid,deleted fields.
CREATE INDEX [I_IncidentTimelineEvent_tenantGuid_deleted] ON [Alerting].[IncidentTimelineEvent] ([tenantGuid], [deleted])
GO

-- Index on the IncidentTimelineEvent table's incidentId,timestamp fields.
CREATE INDEX [I_IncidentTimelineEvent_incidentId_timestamp] ON [Alerting].[IncidentTimelineEvent] ([incidentId], [timestamp])
GO


-- Notes added to incidents by responders.
CREATE TABLE [Alerting].[IncidentNote]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[incidentId] INT NOT NULL,		-- Link to the Incident table.
	[authorObjectGuid] UNIQUEIDENTIFIER NOT NULL,
	[createdAt] DATETIME2(7) NOT NULL,
	[content] NVARCHAR(MAX) NOT NULL,
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_IncidentNote_Incident_incidentId] FOREIGN KEY ([incidentId]) REFERENCES [Alerting].[Incident] ([id])		-- Foreign key to the Incident table.
)
GO

-- Index on the IncidentNote table's tenantGuid field.
CREATE INDEX [I_IncidentNote_tenantGuid] ON [Alerting].[IncidentNote] ([tenantGuid])
GO

-- Index on the IncidentNote table's tenantGuid,incidentId fields.
CREATE INDEX [I_IncidentNote_tenantGuid_incidentId] ON [Alerting].[IncidentNote] ([tenantGuid], [incidentId])
GO

-- Index on the IncidentNote table's tenantGuid,active fields.
CREATE INDEX [I_IncidentNote_tenantGuid_active] ON [Alerting].[IncidentNote] ([tenantGuid], [active])
GO

-- Index on the IncidentNote table's tenantGuid,deleted fields.
CREATE INDEX [I_IncidentNote_tenantGuid_deleted] ON [Alerting].[IncidentNote] ([tenantGuid], [deleted])
GO


-- The change history for records from the IncidentNote table.
CREATE TABLE [Alerting].[IncidentNoteChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[incidentNoteId] INT NOT NULL,		-- Link to the IncidentNote table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_IncidentNoteChangeHistory_IncidentNote_incidentNoteId] FOREIGN KEY ([incidentNoteId]) REFERENCES [Alerting].[IncidentNote] ([id])		-- Foreign key to the IncidentNote table.
)
GO

-- Index on the IncidentNoteChangeHistory table's tenantGuid field.
CREATE INDEX [I_IncidentNoteChangeHistory_tenantGuid] ON [Alerting].[IncidentNoteChangeHistory] ([tenantGuid])
GO

-- Index on the IncidentNoteChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_IncidentNoteChangeHistory_tenantGuid_versionNumber] ON [Alerting].[IncidentNoteChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the IncidentNoteChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_IncidentNoteChangeHistory_tenantGuid_timeStamp] ON [Alerting].[IncidentNoteChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the IncidentNoteChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_IncidentNoteChangeHistory_tenantGuid_userId] ON [Alerting].[IncidentNoteChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the IncidentNoteChangeHistory table's tenantGuid,incidentNoteId fields.
CREATE INDEX [I_IncidentNoteChangeHistory_tenantGuid_incidentNoteId] ON [Alerting].[IncidentNoteChangeHistory] ([tenantGuid], [incidentNoteId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Notifications sent to individual users as part of escalation (teams/schedules are resolved to users at runtime).
CREATE TABLE [Alerting].[IncidentNotification]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[incidentId] INT NOT NULL,		-- Link to the Incident table.
	[escalationRuleId] INT NULL,		-- Link to the EscalationRule table.
	[userObjectGuid] UNIQUEIDENTIFIER NOT NULL,		-- Resolved Security.SecurityUser.objectGuid that was notified.
	[firstNotifiedAt] DATETIME2(7) NOT NULL,
	[lastNotifiedAt] DATETIME2(7) NULL,
	[acknowledgedAt] DATETIME2(7) NULL,
	[acknowledgedByObjectGuid] UNIQUEIDENTIFIER NULL,
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_IncidentNotification_Incident_incidentId] FOREIGN KEY ([incidentId]) REFERENCES [Alerting].[Incident] ([id]),		-- Foreign key to the Incident table.
	CONSTRAINT [FK_IncidentNotification_EscalationRule_escalationRuleId] FOREIGN KEY ([escalationRuleId]) REFERENCES [Alerting].[EscalationRule] ([id])		-- Foreign key to the EscalationRule table.
)
GO

-- Index on the IncidentNotification table's tenantGuid field.
CREATE INDEX [I_IncidentNotification_tenantGuid] ON [Alerting].[IncidentNotification] ([tenantGuid])
GO

-- Index on the IncidentNotification table's tenantGuid,incidentId fields.
CREATE INDEX [I_IncidentNotification_tenantGuid_incidentId] ON [Alerting].[IncidentNotification] ([tenantGuid], [incidentId])
GO

-- Index on the IncidentNotification table's tenantGuid,escalationRuleId fields.
CREATE INDEX [I_IncidentNotification_tenantGuid_escalationRuleId] ON [Alerting].[IncidentNotification] ([tenantGuid], [escalationRuleId])
GO

-- Index on the IncidentNotification table's tenantGuid,active fields.
CREATE INDEX [I_IncidentNotification_tenantGuid_active] ON [Alerting].[IncidentNotification] ([tenantGuid], [active])
GO

-- Index on the IncidentNotification table's tenantGuid,deleted fields.
CREATE INDEX [I_IncidentNotification_tenantGuid_deleted] ON [Alerting].[IncidentNotification] ([tenantGuid], [deleted])
GO

-- Index on the IncidentNotification table's incidentId,userObjectGuid fields.
CREATE INDEX [I_IncidentNotification_incidentId_userObjectGuid] ON [Alerting].[IncidentNotification] ([incidentId], [userObjectGuid])
GO


-- Individual delivery attempts per channel for a notification.
CREATE TABLE [Alerting].[NotificationDeliveryAttempt]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[incidentNotificationId] INT NOT NULL,		-- Link to the IncidentNotification table.
	[notificationChannelTypeId] INT NOT NULL,		-- Link to the NotificationChannelType table.
	[attemptNumber] INT NOT NULL DEFAULT 1,
	[attemptedAt] DATETIME2(7) NOT NULL,
	[status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
	[errorMessage] NVARCHAR(MAX) NULL,
	[response] NVARCHAR(MAX) NULL,
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_NotificationDeliveryAttempt_IncidentNotification_incidentNotificationId] FOREIGN KEY ([incidentNotificationId]) REFERENCES [Alerting].[IncidentNotification] ([id]),		-- Foreign key to the IncidentNotification table.
	CONSTRAINT [FK_NotificationDeliveryAttempt_NotificationChannelType_notificationChannelTypeId] FOREIGN KEY ([notificationChannelTypeId]) REFERENCES [Alerting].[NotificationChannelType] ([id])		-- Foreign key to the NotificationChannelType table.
)
GO

-- Index on the NotificationDeliveryAttempt table's tenantGuid field.
CREATE INDEX [I_NotificationDeliveryAttempt_tenantGuid] ON [Alerting].[NotificationDeliveryAttempt] ([tenantGuid])
GO

-- Index on the NotificationDeliveryAttempt table's tenantGuid,incidentNotificationId fields.
CREATE INDEX [I_NotificationDeliveryAttempt_tenantGuid_incidentNotificationId] ON [Alerting].[NotificationDeliveryAttempt] ([tenantGuid], [incidentNotificationId])
GO

-- Index on the NotificationDeliveryAttempt table's tenantGuid,notificationChannelTypeId fields.
CREATE INDEX [I_NotificationDeliveryAttempt_tenantGuid_notificationChannelTypeId] ON [Alerting].[NotificationDeliveryAttempt] ([tenantGuid], [notificationChannelTypeId])
GO

-- Index on the NotificationDeliveryAttempt table's tenantGuid,active fields.
CREATE INDEX [I_NotificationDeliveryAttempt_tenantGuid_active] ON [Alerting].[NotificationDeliveryAttempt] ([tenantGuid], [active])
GO

-- Index on the NotificationDeliveryAttempt table's tenantGuid,deleted fields.
CREATE INDEX [I_NotificationDeliveryAttempt_tenantGuid_deleted] ON [Alerting].[NotificationDeliveryAttempt] ([tenantGuid], [deleted])
GO

-- Index on the NotificationDeliveryAttempt table's tenantGuid,incidentNotificationId,notificationChannelTypeId fields.
CREATE INDEX [I_NotificationDeliveryAttempt_tenantGuid_incidentNotificationId_notificationChannelTypeId] ON [Alerting].[NotificationDeliveryAttempt] ([tenantGuid], [incidentNotificationId], [notificationChannelTypeId])
GO


-- Outbound webhook delivery attempts for incident status updates.
CREATE TABLE [Alerting].[WebhookDeliveryAttempt]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[incidentId] INT NOT NULL,		-- Link to the Incident table.
	[integrationId] INT NOT NULL,		-- Link to the Integration table.
	[incidentTimelineEventId] INT NULL,		-- Link to the IncidentTimelineEvent table.
	[attemptNumber] INT NOT NULL DEFAULT 1,
	[attemptedAt] DATETIME2(7) NOT NULL,
	[httpStatusCode] INT NULL,
	[success] BIT NOT NULL DEFAULT 0,
	[payloadJson] NVARCHAR(MAX) NULL,
	[responseBody] NVARCHAR(MAX) NULL,
	[errorMessage] NVARCHAR(MAX) NULL,
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_WebhookDeliveryAttempt_Incident_incidentId] FOREIGN KEY ([incidentId]) REFERENCES [Alerting].[Incident] ([id]),		-- Foreign key to the Incident table.
	CONSTRAINT [FK_WebhookDeliveryAttempt_Integration_integrationId] FOREIGN KEY ([integrationId]) REFERENCES [Alerting].[Integration] ([id]),		-- Foreign key to the Integration table.
	CONSTRAINT [FK_WebhookDeliveryAttempt_IncidentTimelineEvent_incidentTimelineEventId] FOREIGN KEY ([incidentTimelineEventId]) REFERENCES [Alerting].[IncidentTimelineEvent] ([id])		-- Foreign key to the IncidentTimelineEvent table.
)
GO

-- Index on the WebhookDeliveryAttempt table's tenantGuid field.
CREATE INDEX [I_WebhookDeliveryAttempt_tenantGuid] ON [Alerting].[WebhookDeliveryAttempt] ([tenantGuid])
GO

-- Index on the WebhookDeliveryAttempt table's tenantGuid,incidentId fields.
CREATE INDEX [I_WebhookDeliveryAttempt_tenantGuid_incidentId] ON [Alerting].[WebhookDeliveryAttempt] ([tenantGuid], [incidentId])
GO

-- Index on the WebhookDeliveryAttempt table's tenantGuid,integrationId fields.
CREATE INDEX [I_WebhookDeliveryAttempt_tenantGuid_integrationId] ON [Alerting].[WebhookDeliveryAttempt] ([tenantGuid], [integrationId])
GO

-- Index on the WebhookDeliveryAttempt table's tenantGuid,incidentTimelineEventId fields.
CREATE INDEX [I_WebhookDeliveryAttempt_tenantGuid_incidentTimelineEventId] ON [Alerting].[WebhookDeliveryAttempt] ([tenantGuid], [incidentTimelineEventId])
GO

-- Index on the WebhookDeliveryAttempt table's tenantGuid,active fields.
CREATE INDEX [I_WebhookDeliveryAttempt_tenantGuid_active] ON [Alerting].[WebhookDeliveryAttempt] ([tenantGuid], [active])
GO

-- Index on the WebhookDeliveryAttempt table's tenantGuid,deleted fields.
CREATE INDEX [I_WebhookDeliveryAttempt_tenantGuid_deleted] ON [Alerting].[WebhookDeliveryAttempt] ([tenantGuid], [deleted])
GO

-- Index on the WebhookDeliveryAttempt table's tenantGuid,incidentId,attemptedAt fields.
CREATE INDEX [I_WebhookDeliveryAttempt_tenantGuid_incidentId_attemptedAt] ON [Alerting].[WebhookDeliveryAttempt] ([tenantGuid], [incidentId], [attemptedAt])
GO


