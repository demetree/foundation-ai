CREATE DATABASE [Auditor]
GO

ALTER DATABASE [Auditor] SET RECOVERY SIMPLE
GO

USE [Auditor]
GO

CREATE SCHEMA [Auditor]
GO

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE [Auditor].[ExternalCommunicationRecipient]
-- DROP TABLE [Auditor].[ExternalCommunication]
-- DROP TABLE [Auditor].[AuditPlanB]
-- DROP TABLE [Auditor].[AuditEventErrorMessage]
-- DROP TABLE [Auditor].[AuditEventEntityState]
-- DROP TABLE [Auditor].[AuditEvent]
-- DROP TABLE [Auditor].[AuditModuleEntity]
-- DROP TABLE [Auditor].[AuditModule]
-- DROP TABLE [Auditor].[AuditResource]
-- DROP TABLE [Auditor].[AuditHostSystem]
-- DROP TABLE [Auditor].[AuditUserAgent]
-- DROP TABLE [Auditor].[AuditSource]
-- DROP TABLE [Auditor].[AuditSession]
-- DROP TABLE [Auditor].[AuditUser]
-- DROP TABLE [Auditor].[AuditAccessType]
-- DROP TABLE [Auditor].[AuditType]

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON [Auditor].[ExternalCommunicationRecipient] DISABLE
-- ALTER INDEX ALL ON [Auditor].[ExternalCommunication] DISABLE
-- ALTER INDEX ALL ON [Auditor].[AuditPlanB] DISABLE
-- ALTER INDEX ALL ON [Auditor].[AuditEventErrorMessage] DISABLE
-- ALTER INDEX ALL ON [Auditor].[AuditEventEntityState] DISABLE
-- ALTER INDEX ALL ON [Auditor].[AuditEvent] DISABLE
-- ALTER INDEX ALL ON [Auditor].[AuditModuleEntity] DISABLE
-- ALTER INDEX ALL ON [Auditor].[AuditModule] DISABLE
-- ALTER INDEX ALL ON [Auditor].[AuditResource] DISABLE
-- ALTER INDEX ALL ON [Auditor].[AuditHostSystem] DISABLE
-- ALTER INDEX ALL ON [Auditor].[AuditUserAgent] DISABLE
-- ALTER INDEX ALL ON [Auditor].[AuditSource] DISABLE
-- ALTER INDEX ALL ON [Auditor].[AuditSession] DISABLE
-- ALTER INDEX ALL ON [Auditor].[AuditUser] DISABLE
-- ALTER INDEX ALL ON [Auditor].[AuditAccessType] DISABLE
-- ALTER INDEX ALL ON [Auditor].[AuditType] DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON [Auditor].[ExternalCommunicationRecipient] REBUILD
-- ALTER INDEX ALL ON [Auditor].[ExternalCommunication] REBUILD
-- ALTER INDEX ALL ON [Auditor].[AuditPlanB] REBUILD
-- ALTER INDEX ALL ON [Auditor].[AuditEventErrorMessage] REBUILD
-- ALTER INDEX ALL ON [Auditor].[AuditEventEntityState] REBUILD
-- ALTER INDEX ALL ON [Auditor].[AuditEvent] REBUILD
-- ALTER INDEX ALL ON [Auditor].[AuditModuleEntity] REBUILD
-- ALTER INDEX ALL ON [Auditor].[AuditModule] REBUILD
-- ALTER INDEX ALL ON [Auditor].[AuditResource] REBUILD
-- ALTER INDEX ALL ON [Auditor].[AuditHostSystem] REBUILD
-- ALTER INDEX ALL ON [Auditor].[AuditUserAgent] REBUILD
-- ALTER INDEX ALL ON [Auditor].[AuditSource] REBUILD
-- ALTER INDEX ALL ON [Auditor].[AuditSession] REBUILD
-- ALTER INDEX ALL ON [Auditor].[AuditUser] REBUILD
-- ALTER INDEX ALL ON [Auditor].[AuditAccessType] REBUILD
-- ALTER INDEX ALL ON [Auditor].[AuditType] REBUILD

CREATE TABLE [Auditor].[AuditType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL
)
GO

-- Index on the AuditType table's name field.
CREATE INDEX [I_AuditType_name] ON [Auditor].[AuditType] ([name])
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Login', 'Log in to the system' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Logout', 'Log out of the system' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Read List', 'Read entity list from the system' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Read List (Redacted)', 'Read of redacted entity list from the system' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Read Entity', 'Read entity from the system' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Read Entity (Redacted)', 'Read of redacted entity from the system' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Create Entity', 'Create entity in the system' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Update Entity', 'Update entity in the system' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Delete Entity', 'Delete entity from the system' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Write List', 'Write list of entities to the system' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Load Page', 'Load page in the system' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Confirmation Requested', 'Confirmation for operation was requested of the user by the system' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Confirmation Granted', 'Confirmation for operation was granted to the system by the user' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Confirmation Denied', 'Confirmation for operation was denied to the system by the user' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Search', 'Search of the system' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Context Set', 'Application context was set by the user' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Unauthorized Access Attempt', 'An attempt was made to access an unauthorized resource' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Error', 'An error was encountered' )
GO

INSERT INTO [Auditor].[AuditType] ( [name], [description] ) VALUES  ( 'Miscellaneous', 'Miscellaneous event' )
GO


CREATE TABLE [Auditor].[AuditAccessType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL
)
GO

-- Index on the AuditAccessType table's name field.
CREATE INDEX [I_AuditAccessType_name] ON [Auditor].[AuditAccessType] ([name])
GO

INSERT INTO [Auditor].[AuditAccessType] ( [name], [description] ) VALUES  ( 'Web Browser', 'User connecting with a web browser to access the system.' )
GO

INSERT INTO [Auditor].[AuditAccessType] ( [name], [description] ) VALUES  ( 'API Request', 'Request made by other software to access the system.' )
GO

INSERT INTO [Auditor].[AuditAccessType] ( [name], [description] ) VALUES  ( 'Ambiguous', 'Ambiguous access type.  Could be end user or other software.' )
GO


CREATE TABLE [Auditor].[AuditUser]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(500) NOT NULL UNIQUE,
	[comments] NVARCHAR(1000) NULL,
	[firstAccess] DATETIME2(7) NULL
)
GO

-- Index on the AuditUser table's name field.
CREATE INDEX [I_AuditUser_name] ON [Auditor].[AuditUser] ([name])
GO


CREATE TABLE [Auditor].[AuditSession]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(500) NOT NULL UNIQUE,
	[comments] NVARCHAR(1000) NULL,
	[firstAccess] DATETIME2(7) NULL
)
GO

-- Index on the AuditSession table's name field.
CREATE INDEX [I_AuditSession_name] ON [Auditor].[AuditSession] ([name])
GO


CREATE TABLE [Auditor].[AuditSource]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(500) NOT NULL UNIQUE,
	[comments] NVARCHAR(1000) NULL,
	[firstAccess] DATETIME2(7) NULL
)
GO

-- Index on the AuditSource table's name field.
CREATE INDEX [I_AuditSource_name] ON [Auditor].[AuditSource] ([name])
GO


CREATE TABLE [Auditor].[AuditUserAgent]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(500) NOT NULL UNIQUE,
	[comments] NVARCHAR(1000) NULL,
	[firstAccess] DATETIME2(7) NULL
)
GO

-- Index on the AuditUserAgent table's name field.
CREATE INDEX [I_AuditUserAgent_name] ON [Auditor].[AuditUserAgent] ([name])
GO


CREATE TABLE [Auditor].[AuditHostSystem]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(500) NOT NULL UNIQUE,
	[comments] NVARCHAR(1000) NULL,
	[firstAccess] DATETIME2(7) NULL
)
GO

-- Index on the AuditHostSystem table's name field.
CREATE INDEX [I_AuditHostSystem_name] ON [Auditor].[AuditHostSystem] ([name])
GO


CREATE TABLE [Auditor].[AuditResource]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(850) NOT NULL UNIQUE,
	[comments] NVARCHAR(1000) NULL,
	[firstAccess] DATETIME2(7) NULL
)
GO

-- Index on the AuditResource table's name field.
CREATE INDEX [I_AuditResource_name] ON [Auditor].[AuditResource] ([name])
GO


CREATE TABLE [Auditor].[AuditModule]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(500) NOT NULL UNIQUE,
	[comments] NVARCHAR(1000) NULL,
	[firstAccess] DATETIME2(7) NULL
)
GO

-- Index on the AuditModule table's name field.
CREATE INDEX [I_AuditModule_name] ON [Auditor].[AuditModule] ([name])
GO


CREATE TABLE [Auditor].[AuditModuleEntity]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[auditModuleId] INT NOT NULL,		-- Link to the AuditModule table.
	[name] NVARCHAR(500) NOT NULL,
	[comments] NVARCHAR(1000) NULL,
	[firstAccess] DATETIME2(7) NULL
	CONSTRAINT [FK_AuditModuleEntity_AuditModule_auditModuleId] FOREIGN KEY ([auditModuleId]) REFERENCES [Auditor].[AuditModule] ([id])		-- Foreign key to the AuditModule table.
)
GO

-- Index on the AuditModuleEntity table's auditModuleId field.
CREATE INDEX [I_AuditModuleEntity_auditModuleId] ON [Auditor].[AuditModuleEntity] ([auditModuleId])
GO

-- Index on the AuditModuleEntity table's auditModuleId,name fields.
CREATE INDEX [I_AuditModuleEntity_auditModuleId_name] ON [Auditor].[AuditModuleEntity] ([auditModuleId], [name])
GO


CREATE TABLE [Auditor].[AuditEvent]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[startTime] DATETIME2(7) NOT NULL,
	[stopTime] DATETIME2(7) NOT NULL,
	[completedSuccessfully] BIT NOT NULL DEFAULT 1,
	[auditUserId] INT NOT NULL,		-- Link to the AuditUser table.
	[auditSessionId] INT NOT NULL,		-- Link to the AuditSession table.
	[auditTypeId] INT NOT NULL,		-- Link to the AuditType table.
	[auditAccessTypeId] INT NOT NULL,		-- Link to the AuditAccessType table.
	[auditSourceId] INT NOT NULL,		-- Link to the AuditSource table.
	[auditUserAgentId] INT NOT NULL,		-- Link to the AuditUserAgent table.
	[auditModuleId] INT NOT NULL,		-- Link to the AuditModule table.
	[auditModuleEntityId] INT NOT NULL,		-- Link to the AuditModuleEntity table.
	[auditResourceId] INT NOT NULL,		-- Link to the AuditResource table.
	[auditHostSystemId] INT NOT NULL,		-- Link to the AuditHostSystem table.
	[primaryKey] NVARCHAR(250) NULL,
	[threadId] INT NULL,
	[message] NVARCHAR(MAX) NOT NULL
	CONSTRAINT [FK_AuditEvent_AuditUser_auditUserId] FOREIGN KEY ([auditUserId]) REFERENCES [Auditor].[AuditUser] ([id]),		-- Foreign key to the AuditUser table.
	CONSTRAINT [FK_AuditEvent_AuditSession_auditSessionId] FOREIGN KEY ([auditSessionId]) REFERENCES [Auditor].[AuditSession] ([id]),		-- Foreign key to the AuditSession table.
	CONSTRAINT [FK_AuditEvent_AuditType_auditTypeId] FOREIGN KEY ([auditTypeId]) REFERENCES [Auditor].[AuditType] ([id]),		-- Foreign key to the AuditType table.
	CONSTRAINT [FK_AuditEvent_AuditAccessType_auditAccessTypeId] FOREIGN KEY ([auditAccessTypeId]) REFERENCES [Auditor].[AuditAccessType] ([id]),		-- Foreign key to the AuditAccessType table.
	CONSTRAINT [FK_AuditEvent_AuditSource_auditSourceId] FOREIGN KEY ([auditSourceId]) REFERENCES [Auditor].[AuditSource] ([id]),		-- Foreign key to the AuditSource table.
	CONSTRAINT [FK_AuditEvent_AuditUserAgent_auditUserAgentId] FOREIGN KEY ([auditUserAgentId]) REFERENCES [Auditor].[AuditUserAgent] ([id]),		-- Foreign key to the AuditUserAgent table.
	CONSTRAINT [FK_AuditEvent_AuditModule_auditModuleId] FOREIGN KEY ([auditModuleId]) REFERENCES [Auditor].[AuditModule] ([id]),		-- Foreign key to the AuditModule table.
	CONSTRAINT [FK_AuditEvent_AuditModuleEntity_auditModuleEntityId] FOREIGN KEY ([auditModuleEntityId]) REFERENCES [Auditor].[AuditModuleEntity] ([id]),		-- Foreign key to the AuditModuleEntity table.
	CONSTRAINT [FK_AuditEvent_AuditResource_auditResourceId] FOREIGN KEY ([auditResourceId]) REFERENCES [Auditor].[AuditResource] ([id]),		-- Foreign key to the AuditResource table.
	CONSTRAINT [FK_AuditEvent_AuditHostSystem_auditHostSystemId] FOREIGN KEY ([auditHostSystemId]) REFERENCES [Auditor].[AuditHostSystem] ([id])		-- Foreign key to the AuditHostSystem table.
)
GO

-- Index on the AuditEvent table's startTime field.
CREATE INDEX [I_AuditEvent_startTime] ON [Auditor].[AuditEvent] ([startTime])
GO

-- Index on the AuditEvent table's stopTime field.
CREATE INDEX [I_AuditEvent_stopTime] ON [Auditor].[AuditEvent] ([stopTime])
GO

-- Index on the AuditEvent table's auditUserId field.
CREATE INDEX [I_AuditEvent_auditUserId] ON [Auditor].[AuditEvent] ([auditUserId])
GO

-- Index on the AuditEvent table's auditSessionId field.
CREATE INDEX [I_AuditEvent_auditSessionId] ON [Auditor].[AuditEvent] ([auditSessionId])
GO

-- Index on the AuditEvent table's auditTypeId field.
CREATE INDEX [I_AuditEvent_auditTypeId] ON [Auditor].[AuditEvent] ([auditTypeId])
GO

-- Index on the AuditEvent table's auditAccessTypeId field.
CREATE INDEX [I_AuditEvent_auditAccessTypeId] ON [Auditor].[AuditEvent] ([auditAccessTypeId])
GO

-- Index on the AuditEvent table's auditSourceId field.
CREATE INDEX [I_AuditEvent_auditSourceId] ON [Auditor].[AuditEvent] ([auditSourceId])
GO

-- Index on the AuditEvent table's auditUserAgentId field.
CREATE INDEX [I_AuditEvent_auditUserAgentId] ON [Auditor].[AuditEvent] ([auditUserAgentId])
GO

-- Index on the AuditEvent table's auditModuleId field.
CREATE INDEX [I_AuditEvent_auditModuleId] ON [Auditor].[AuditEvent] ([auditModuleId])
GO

-- Index on the AuditEvent table's auditModuleEntityId field.
CREATE INDEX [I_AuditEvent_auditModuleEntityId] ON [Auditor].[AuditEvent] ([auditModuleEntityId])
GO

-- Index on the AuditEvent table's auditResourceId field.
CREATE INDEX [I_AuditEvent_auditResourceId] ON [Auditor].[AuditEvent] ([auditResourceId])
GO

-- Index on the AuditEvent table's auditHostSystemId field.
CREATE INDEX [I_AuditEvent_auditHostSystemId] ON [Auditor].[AuditEvent] ([auditHostSystemId])
GO

-- Index on the AuditEvent table's startTime,auditUserId fields.
CREATE INDEX [I_AuditEvent_startTime_auditUserId] ON [Auditor].[AuditEvent] ([startTime], [auditUserId])
GO

-- Index on the AuditEvent table's startTime,auditModuleId fields.
CREATE INDEX [I_AuditEvent_startTime_auditModuleId] ON [Auditor].[AuditEvent] ([startTime], [auditModuleId])
GO

-- Index on the AuditEvent table's startTime,auditTypeId fields.
CREATE INDEX [I_AuditEvent_startTime_auditTypeId] ON [Auditor].[AuditEvent] ([startTime], [auditTypeId])
GO


CREATE TABLE [Auditor].[AuditEventEntityState]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[auditEventId] INT NOT NULL,		-- Link to the AuditEvent table.
	[beforeState] NVARCHAR(MAX) NULL,
	[afterState] NVARCHAR(MAX) NULL
	CONSTRAINT [FK_AuditEventEntityState_AuditEvent_auditEventId] FOREIGN KEY ([auditEventId]) REFERENCES [Auditor].[AuditEvent] ([id])		-- Foreign key to the AuditEvent table.
)
GO

-- Index on the AuditEventEntityState table's auditEventId field.
CREATE INDEX [I_AuditEventEntityState_auditEventId] ON [Auditor].[AuditEventEntityState] ([auditEventId])
GO


CREATE TABLE [Auditor].[AuditEventErrorMessage]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[auditEventId] INT NOT NULL,		-- Link to the AuditEvent table.
	[errorMessage] NVARCHAR(MAX) NOT NULL
	CONSTRAINT [FK_AuditEventErrorMessage_AuditEvent_auditEventId] FOREIGN KEY ([auditEventId]) REFERENCES [Auditor].[AuditEvent] ([id])		-- Foreign key to the AuditEvent table.
)
GO

-- Index on the AuditEventErrorMessage table's auditEventId field.
CREATE INDEX [I_AuditEventErrorMessage_auditEventId] ON [Auditor].[AuditEventErrorMessage] ([auditEventId])
GO


CREATE TABLE [Auditor].[AuditPlanB]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[startTime] DATETIME2(7) NOT NULL,
	[stopTime] DATETIME2(7) NOT NULL,
	[completedSuccessfully] BIT NOT NULL DEFAULT 1,
	[user] NVARCHAR(100) NULL,
	[session] NVARCHAR(100) NULL,
	[type] NVARCHAR(100) NULL,
	[accessType] NVARCHAR(100) NULL,
	[source] NVARCHAR(50) NULL,
	[userAgent] NVARCHAR(100) NULL,
	[module] NVARCHAR(100) NULL,
	[moduleEntity] NVARCHAR(100) NULL,
	[resource] NVARCHAR(500) NULL,
	[hostSystem] NVARCHAR(50) NULL,
	[primaryKey] NVARCHAR(250) NULL,
	[threadId] INT NULL,
	[message] NVARCHAR(MAX) NULL,
	[beforeState] NVARCHAR(MAX) NULL,
	[afterState] NVARCHAR(MAX) NULL,
	[errorMessage] NVARCHAR(MAX) NULL,
	[exceptionText] NVARCHAR(MAX) NULL
)
GO

-- Index on the AuditPlanB table's startTime field.
CREATE INDEX [I_AuditPlanB_startTime] ON [Auditor].[AuditPlanB] ([startTime])
GO

-- Index on the AuditPlanB table's stopTime field.
CREATE INDEX [I_AuditPlanB_stopTime] ON [Auditor].[AuditPlanB] ([stopTime])
GO


CREATE TABLE [Auditor].[ExternalCommunication]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[timeStamp] DATETIME2(7) NULL,
	[auditUserId] INT NULL,		-- Link to the AuditUser table.
	[communicationType] NVARCHAR(100) NULL,
	[subject] NVARCHAR(2000) NULL,
	[message] NVARCHAR(MAX) NULL,
	[completedSuccessfully] BIT NOT NULL DEFAULT 1,
	[responseMessage] NVARCHAR(MAX) NULL,
	[exceptionText] NVARCHAR(MAX) NULL
	CONSTRAINT [FK_ExternalCommunication_AuditUser_auditUserId] FOREIGN KEY ([auditUserId]) REFERENCES [Auditor].[AuditUser] ([id])		-- Foreign key to the AuditUser table.
)
GO

-- Index on the ExternalCommunication table's auditUserId field.
CREATE INDEX [I_ExternalCommunication_auditUserId] ON [Auditor].[ExternalCommunication] ([auditUserId])
GO


CREATE TABLE [Auditor].[ExternalCommunicationRecipient]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[externalCommunicationId] INT NULL,		-- Link to the ExternalCommunication table.
	[recipient] NVARCHAR(100) NULL,
	[type] NVARCHAR(50) NULL
	CONSTRAINT [FK_ExternalCommunicationRecipient_ExternalCommunication_externalCommunicationId] FOREIGN KEY ([externalCommunicationId]) REFERENCES [Auditor].[ExternalCommunication] ([id])		-- Foreign key to the ExternalCommunication table.
)
GO

-- Index on the ExternalCommunicationRecipient table's externalCommunicationId field.
CREATE INDEX [I_ExternalCommunicationRecipient_externalCommunicationId] ON [Auditor].[ExternalCommunicationRecipient] ([externalCommunicationId])
GO


