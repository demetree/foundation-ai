CREATE DATABASE "Auditor"
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    TEMPLATE=template0;

\connect "Auditor"   -- Run the create database first, then connect to it, and run the rest if running in a query tool rather than through psql scripting.



CREATE SCHEMA "Auditor"

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE "Auditor"."ExternalCommunicationRecipient"
-- DROP TABLE "Auditor"."ExternalCommunication"
-- DROP TABLE "Auditor"."AuditPlanB"
-- DROP TABLE "Auditor"."AuditEventErrorMessage"
-- DROP TABLE "Auditor"."AuditEventEntityState"
-- DROP TABLE "Auditor"."AuditEvent"
-- DROP TABLE "Auditor"."AuditModuleEntity"
-- DROP TABLE "Auditor"."AuditModule"
-- DROP TABLE "Auditor"."AuditResource"
-- DROP TABLE "Auditor"."AuditHostSystem"
-- DROP TABLE "Auditor"."AuditUserAgent"
-- DROP TABLE "Auditor"."AuditSource"
-- DROP TABLE "Auditor"."AuditSession"
-- DROP TABLE "Auditor"."AuditUser"
-- DROP TABLE "Auditor"."AuditAccessType"
-- DROP TABLE "Auditor"."AuditType"

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON "ExternalCommunicationRecipient" DISABLE
-- ALTER INDEX ALL ON "ExternalCommunication" DISABLE
-- ALTER INDEX ALL ON "AuditPlanB" DISABLE
-- ALTER INDEX ALL ON "AuditEventErrorMessage" DISABLE
-- ALTER INDEX ALL ON "AuditEventEntityState" DISABLE
-- ALTER INDEX ALL ON "AuditEvent" DISABLE
-- ALTER INDEX ALL ON "AuditModuleEntity" DISABLE
-- ALTER INDEX ALL ON "AuditModule" DISABLE
-- ALTER INDEX ALL ON "AuditResource" DISABLE
-- ALTER INDEX ALL ON "AuditHostSystem" DISABLE
-- ALTER INDEX ALL ON "AuditUserAgent" DISABLE
-- ALTER INDEX ALL ON "AuditSource" DISABLE
-- ALTER INDEX ALL ON "AuditSession" DISABLE
-- ALTER INDEX ALL ON "AuditUser" DISABLE
-- ALTER INDEX ALL ON "AuditAccessType" DISABLE
-- ALTER INDEX ALL ON "AuditType" DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON "ExternalCommunicationRecipient" REBUILD
-- ALTER INDEX ALL ON "ExternalCommunication" REBUILD
-- ALTER INDEX ALL ON "AuditPlanB" REBUILD
-- ALTER INDEX ALL ON "AuditEventErrorMessage" REBUILD
-- ALTER INDEX ALL ON "AuditEventEntityState" REBUILD
-- ALTER INDEX ALL ON "AuditEvent" REBUILD
-- ALTER INDEX ALL ON "AuditModuleEntity" REBUILD
-- ALTER INDEX ALL ON "AuditModule" REBUILD
-- ALTER INDEX ALL ON "AuditResource" REBUILD
-- ALTER INDEX ALL ON "AuditHostSystem" REBUILD
-- ALTER INDEX ALL ON "AuditUserAgent" REBUILD
-- ALTER INDEX ALL ON "AuditSource" REBUILD
-- ALTER INDEX ALL ON "AuditSession" REBUILD
-- ALTER INDEX ALL ON "AuditUser" REBUILD
-- ALTER INDEX ALL ON "AuditAccessType" REBUILD
-- ALTER INDEX ALL ON "AuditType" REBUILD

CREATE TABLE "Auditor"."AuditType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NULL
);
-- Index on the AuditType table's name field.
CREATE INDEX "I_AuditType_name" ON "Auditor"."AuditType" ("name")
;

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Login', 'Log in to the system' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Logout', 'Log out of the system' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Read List', 'Read entity list from the system' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Read List (Redacted)', 'Read of redacted entity list from the system' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Read Entity', 'Read entity from the system' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Read Entity (Redacted)', 'Read of redacted entity from the system' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Create Entity', 'Create entity in the system' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Update Entity', 'Update entity in the system' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Delete Entity', 'Delete entity from the system' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Write List', 'Write list of entities to the system' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Load Page', 'Load page in the system' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Confirmation Requested', 'Confirmation for operation was requested of the user by the system' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Confirmation Granted', 'Confirmation for operation was granted to the system by the user' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Confirmation Denied', 'Confirmation for operation was denied to the system by the user' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Search', 'Search of the system' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Context Set', 'Application context was set by the user' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Unauthorized Access Attempt', 'An attempt was made to access an unauthorized resource' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Error', 'An error was encountered' );

INSERT INTO "Auditor"."AuditType" ( "name", "description" ) VALUES  ( 'Miscellaneous', 'Miscellaneous event' );


CREATE TABLE "Auditor"."AuditAccessType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NULL
);
-- Index on the AuditAccessType table's name field.
CREATE INDEX "I_AuditAccessType_name" ON "Auditor"."AuditAccessType" ("name")
;

INSERT INTO "Auditor"."AuditAccessType" ( "name", "description" ) VALUES  ( 'Web Browser', 'User connecting with a web browser to access the system.' );

INSERT INTO "Auditor"."AuditAccessType" ( "name", "description" ) VALUES  ( 'API Request', 'Request made by other software to access the system.' );

INSERT INTO "Auditor"."AuditAccessType" ( "name", "description" ) VALUES  ( 'Ambiguous', 'Ambiguous access type.  Could be end user or other software.' );


CREATE TABLE "Auditor"."AuditUser"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(500) NOT NULL UNIQUE,
	"comments" VARCHAR(1000) NULL,
	"firstAccess" TIMESTAMP NULL
);
-- Index on the AuditUser table's name field.
CREATE INDEX "I_AuditUser_name" ON "Auditor"."AuditUser" ("name")
;


CREATE TABLE "Auditor"."AuditSession"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(500) NOT NULL UNIQUE,
	"comments" VARCHAR(1000) NULL,
	"firstAccess" TIMESTAMP NULL
);
-- Index on the AuditSession table's name field.
CREATE INDEX "I_AuditSession_name" ON "Auditor"."AuditSession" ("name")
;


CREATE TABLE "Auditor"."AuditSource"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(500) NOT NULL UNIQUE,
	"comments" VARCHAR(1000) NULL,
	"firstAccess" TIMESTAMP NULL
);
-- Index on the AuditSource table's name field.
CREATE INDEX "I_AuditSource_name" ON "Auditor"."AuditSource" ("name")
;


CREATE TABLE "Auditor"."AuditUserAgent"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(500) NOT NULL UNIQUE,
	"comments" VARCHAR(1000) NULL,
	"firstAccess" TIMESTAMP NULL
);
-- Index on the AuditUserAgent table's name field.
CREATE INDEX "I_AuditUserAgent_name" ON "Auditor"."AuditUserAgent" ("name")
;


CREATE TABLE "Auditor"."AuditHostSystem"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(500) NOT NULL UNIQUE,
	"comments" VARCHAR(1000) NULL,
	"firstAccess" TIMESTAMP NULL
);
-- Index on the AuditHostSystem table's name field.
CREATE INDEX "I_AuditHostSystem_name" ON "Auditor"."AuditHostSystem" ("name")
;


CREATE TABLE "Auditor"."AuditResource"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(850) NOT NULL UNIQUE,
	"comments" VARCHAR(1000) NULL,
	"firstAccess" TIMESTAMP NULL
);
-- Index on the AuditResource table's name field.
CREATE INDEX "I_AuditResource_name" ON "Auditor"."AuditResource" ("name")
;


CREATE TABLE "Auditor"."AuditModule"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(500) NOT NULL UNIQUE,
	"comments" VARCHAR(1000) NULL,
	"firstAccess" TIMESTAMP NULL
);
-- Index on the AuditModule table's name field.
CREATE INDEX "I_AuditModule_name" ON "Auditor"."AuditModule" ("name")
;


CREATE TABLE "Auditor"."AuditModuleEntity"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"auditModuleId" INT NOT NULL,		-- Link to the AuditModule table.
	"name" VARCHAR(500) NOT NULL,
	"comments" VARCHAR(1000) NULL,
	"firstAccess" TIMESTAMP NULL,
	CONSTRAINT "auditModuleId" FOREIGN KEY ("auditModuleId") REFERENCES "Auditor"."AuditModule"("id")		-- Foreign key to the AuditModule table.
);
-- Index on the AuditModuleEntity table's auditModuleId field.
CREATE INDEX "I_AuditModuleEntity_auditModuleId" ON "Auditor"."AuditModuleEntity" ("auditModuleId")
;

-- Index on the AuditModuleEntity table's auditModuleId,name fields.
CREATE INDEX "I_AuditModuleEntity_auditModuleId_name" ON "Auditor"."AuditModuleEntity" ("auditModuleId", "name")
;


CREATE TABLE "Auditor"."AuditEvent"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"startTime" TIMESTAMP NOT NULL,
	"stopTime" TIMESTAMP NOT NULL,
	"completedSuccessfully" BOOLEAN NOT NULL DEFAULT true,
	"auditUserId" INT NOT NULL,		-- Link to the AuditUser table.
	"auditSessionId" INT NOT NULL,		-- Link to the AuditSession table.
	"auditTypeId" INT NOT NULL,		-- Link to the AuditType table.
	"auditAccessTypeId" INT NOT NULL,		-- Link to the AuditAccessType table.
	"auditSourceId" INT NOT NULL,		-- Link to the AuditSource table.
	"auditUserAgentId" INT NOT NULL,		-- Link to the AuditUserAgent table.
	"auditModuleId" INT NOT NULL,		-- Link to the AuditModule table.
	"auditModuleEntityId" INT NOT NULL,		-- Link to the AuditModuleEntity table.
	"auditResourceId" INT NOT NULL,		-- Link to the AuditResource table.
	"auditHostSystemId" INT NOT NULL,		-- Link to the AuditHostSystem table.
	"primaryKey" VARCHAR(250) NULL,
	"threadId" INT NULL,
	"message" TEXT NOT NULL,
	CONSTRAINT "auditUserId" FOREIGN KEY ("auditUserId") REFERENCES "Auditor"."AuditUser"("id"),		-- Foreign key to the AuditUser table.
	CONSTRAINT "auditSessionId" FOREIGN KEY ("auditSessionId") REFERENCES "Auditor"."AuditSession"("id"),		-- Foreign key to the AuditSession table.
	CONSTRAINT "auditTypeId" FOREIGN KEY ("auditTypeId") REFERENCES "Auditor"."AuditType"("id"),		-- Foreign key to the AuditType table.
	CONSTRAINT "auditAccessTypeId" FOREIGN KEY ("auditAccessTypeId") REFERENCES "Auditor"."AuditAccessType"("id"),		-- Foreign key to the AuditAccessType table.
	CONSTRAINT "auditSourceId" FOREIGN KEY ("auditSourceId") REFERENCES "Auditor"."AuditSource"("id"),		-- Foreign key to the AuditSource table.
	CONSTRAINT "auditUserAgentId" FOREIGN KEY ("auditUserAgentId") REFERENCES "Auditor"."AuditUserAgent"("id"),		-- Foreign key to the AuditUserAgent table.
	CONSTRAINT "auditModuleId" FOREIGN KEY ("auditModuleId") REFERENCES "Auditor"."AuditModule"("id"),		-- Foreign key to the AuditModule table.
	CONSTRAINT "auditModuleEntityId" FOREIGN KEY ("auditModuleEntityId") REFERENCES "Auditor"."AuditModuleEntity"("id"),		-- Foreign key to the AuditModuleEntity table.
	CONSTRAINT "auditResourceId" FOREIGN KEY ("auditResourceId") REFERENCES "Auditor"."AuditResource"("id"),		-- Foreign key to the AuditResource table.
	CONSTRAINT "auditHostSystemId" FOREIGN KEY ("auditHostSystemId") REFERENCES "Auditor"."AuditHostSystem"("id")		-- Foreign key to the AuditHostSystem table.
);
-- Index on the AuditEvent table's startTime field.
CREATE INDEX "I_AuditEvent_startTime" ON "Auditor"."AuditEvent" ("startTime")
;

-- Index on the AuditEvent table's stopTime field.
CREATE INDEX "I_AuditEvent_stopTime" ON "Auditor"."AuditEvent" ("stopTime")
;

-- Index on the AuditEvent table's auditUserId field.
CREATE INDEX "I_AuditEvent_auditUserId" ON "Auditor"."AuditEvent" ("auditUserId")
;

-- Index on the AuditEvent table's auditSessionId field.
CREATE INDEX "I_AuditEvent_auditSessionId" ON "Auditor"."AuditEvent" ("auditSessionId")
;

-- Index on the AuditEvent table's auditTypeId field.
CREATE INDEX "I_AuditEvent_auditTypeId" ON "Auditor"."AuditEvent" ("auditTypeId")
;

-- Index on the AuditEvent table's auditAccessTypeId field.
CREATE INDEX "I_AuditEvent_auditAccessTypeId" ON "Auditor"."AuditEvent" ("auditAccessTypeId")
;

-- Index on the AuditEvent table's auditSourceId field.
CREATE INDEX "I_AuditEvent_auditSourceId" ON "Auditor"."AuditEvent" ("auditSourceId")
;

-- Index on the AuditEvent table's auditUserAgentId field.
CREATE INDEX "I_AuditEvent_auditUserAgentId" ON "Auditor"."AuditEvent" ("auditUserAgentId")
;

-- Index on the AuditEvent table's auditModuleId field.
CREATE INDEX "I_AuditEvent_auditModuleId" ON "Auditor"."AuditEvent" ("auditModuleId")
;

-- Index on the AuditEvent table's auditModuleEntityId field.
CREATE INDEX "I_AuditEvent_auditModuleEntityId" ON "Auditor"."AuditEvent" ("auditModuleEntityId")
;

-- Index on the AuditEvent table's auditResourceId field.
CREATE INDEX "I_AuditEvent_auditResourceId" ON "Auditor"."AuditEvent" ("auditResourceId")
;

-- Index on the AuditEvent table's auditHostSystemId field.
CREATE INDEX "I_AuditEvent_auditHostSystemId" ON "Auditor"."AuditEvent" ("auditHostSystemId")
;

-- Index on the AuditEvent table's startTime,auditUserId fields.
CREATE INDEX "I_AuditEvent_startTime_auditUserId" ON "Auditor"."AuditEvent" ("startTime", "auditUserId")
;

-- Index on the AuditEvent table's startTime,auditModuleId fields.
CREATE INDEX "I_AuditEvent_startTime_auditModuleId" ON "Auditor"."AuditEvent" ("startTime", "auditModuleId")
;

-- Index on the AuditEvent table's startTime,auditTypeId fields.
CREATE INDEX "I_AuditEvent_startTime_auditTypeId" ON "Auditor"."AuditEvent" ("startTime", "auditTypeId")
;


CREATE TABLE "Auditor"."AuditEventEntityState"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"auditEventId" INT NOT NULL,		-- Link to the AuditEvent table.
	"beforeState" TEXT NULL,
	"afterState" TEXT NULL,
	CONSTRAINT "auditEventId" FOREIGN KEY ("auditEventId") REFERENCES "Auditor"."AuditEvent"("id")		-- Foreign key to the AuditEvent table.
);
-- Index on the AuditEventEntityState table's auditEventId field.
CREATE INDEX "I_AuditEventEntityState_auditEventId" ON "Auditor"."AuditEventEntityState" ("auditEventId")
;


CREATE TABLE "Auditor"."AuditEventErrorMessage"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"auditEventId" INT NOT NULL,		-- Link to the AuditEvent table.
	"errorMessage" TEXT NOT NULL,
	CONSTRAINT "auditEventId" FOREIGN KEY ("auditEventId") REFERENCES "Auditor"."AuditEvent"("id")		-- Foreign key to the AuditEvent table.
);
-- Index on the AuditEventErrorMessage table's auditEventId field.
CREATE INDEX "I_AuditEventErrorMessage_auditEventId" ON "Auditor"."AuditEventErrorMessage" ("auditEventId")
;


CREATE TABLE "Auditor"."AuditPlanB"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"startTime" TIMESTAMP NOT NULL,
	"stopTime" TIMESTAMP NOT NULL,
	"completedSuccessfully" BOOLEAN NOT NULL DEFAULT true,
	"user" VARCHAR(100) NULL,
	"session" VARCHAR(100) NULL,
	"type" VARCHAR(100) NULL,
	"accessType" VARCHAR(100) NULL,
	"source" VARCHAR(50) NULL,
	"userAgent" VARCHAR(100) NULL,
	"module" VARCHAR(100) NULL,
	"moduleEntity" VARCHAR(100) NULL,
	"resource" VARCHAR(500) NULL,
	"hostSystem" VARCHAR(50) NULL,
	"primaryKey" VARCHAR(250) NULL,
	"threadId" INT NULL,
	"message" TEXT NULL,
	"beforeState" TEXT NULL,
	"afterState" TEXT NULL,
	"errorMessage" TEXT NULL,
	"exceptionText" TEXT NULL
);
-- Index on the AuditPlanB table's startTime field.
CREATE INDEX "I_AuditPlanB_startTime" ON "Auditor"."AuditPlanB" ("startTime")
;

-- Index on the AuditPlanB table's stopTime field.
CREATE INDEX "I_AuditPlanB_stopTime" ON "Auditor"."AuditPlanB" ("stopTime")
;


CREATE TABLE "Auditor"."ExternalCommunication"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"timeStamp" TIMESTAMP NULL,
	"auditUserId" INT NULL,		-- Link to the AuditUser table.
	"communicationType" VARCHAR(100) NULL,
	"subject" VARCHAR(2000) NULL,
	"message" TEXT NULL,
	"completedSuccessfully" BOOLEAN NOT NULL DEFAULT true,
	"responseMessage" TEXT NULL,
	"exceptionText" TEXT NULL,
	CONSTRAINT "auditUserId" FOREIGN KEY ("auditUserId") REFERENCES "Auditor"."AuditUser"("id")		-- Foreign key to the AuditUser table.
);
-- Index on the ExternalCommunication table's auditUserId field.
CREATE INDEX "I_ExternalCommunication_auditUserId" ON "Auditor"."ExternalCommunication" ("auditUserId")
;


CREATE TABLE "Auditor"."ExternalCommunicationRecipient"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"externalCommunicationId" INT NULL,		-- Link to the ExternalCommunication table.
	"recipient" VARCHAR(100) NULL,
	"type" VARCHAR(50) NULL,
	CONSTRAINT "externalCommunicationId" FOREIGN KEY ("externalCommunicationId") REFERENCES "Auditor"."ExternalCommunication"("id")		-- Foreign key to the ExternalCommunication table.
);
-- Index on the ExternalCommunicationRecipient table's externalCommunicationId field.
CREATE INDEX "I_ExternalCommunicationRecipient_externalCommunicationId" ON "Auditor"."ExternalCommunicationRecipient" ("externalCommunicationId")
;


