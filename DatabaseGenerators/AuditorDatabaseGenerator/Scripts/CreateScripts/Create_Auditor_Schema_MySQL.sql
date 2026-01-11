CREATE DATABASE `Auditor`;

USE `Auditor`;

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE `ExternalCommunicationRecipient`
-- DROP TABLE `ExternalCommunication`
-- DROP TABLE `AuditPlanB`
-- DROP TABLE `AuditEventErrorMessage`
-- DROP TABLE `AuditEventEntityState`
-- DROP TABLE `AuditEvent`
-- DROP TABLE `AuditModuleEntity`
-- DROP TABLE `AuditModule`
-- DROP TABLE `AuditResource`
-- DROP TABLE `AuditHostSystem`
-- DROP TABLE `AuditUserAgent`
-- DROP TABLE `AuditSource`
-- DROP TABLE `AuditSession`
-- DROP TABLE `AuditUser`
-- DROP TABLE `AuditAccessType`
-- DROP TABLE `AuditType`

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON `ExternalCommunicationRecipient` DISABLE
-- ALTER INDEX ALL ON `ExternalCommunication` DISABLE
-- ALTER INDEX ALL ON `AuditPlanB` DISABLE
-- ALTER INDEX ALL ON `AuditEventErrorMessage` DISABLE
-- ALTER INDEX ALL ON `AuditEventEntityState` DISABLE
-- ALTER INDEX ALL ON `AuditEvent` DISABLE
-- ALTER INDEX ALL ON `AuditModuleEntity` DISABLE
-- ALTER INDEX ALL ON `AuditModule` DISABLE
-- ALTER INDEX ALL ON `AuditResource` DISABLE
-- ALTER INDEX ALL ON `AuditHostSystem` DISABLE
-- ALTER INDEX ALL ON `AuditUserAgent` DISABLE
-- ALTER INDEX ALL ON `AuditSource` DISABLE
-- ALTER INDEX ALL ON `AuditSession` DISABLE
-- ALTER INDEX ALL ON `AuditUser` DISABLE
-- ALTER INDEX ALL ON `AuditAccessType` DISABLE
-- ALTER INDEX ALL ON `AuditType` DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON `ExternalCommunicationRecipient` REBUILD
-- ALTER INDEX ALL ON `ExternalCommunication` REBUILD
-- ALTER INDEX ALL ON `AuditPlanB` REBUILD
-- ALTER INDEX ALL ON `AuditEventErrorMessage` REBUILD
-- ALTER INDEX ALL ON `AuditEventEntityState` REBUILD
-- ALTER INDEX ALL ON `AuditEvent` REBUILD
-- ALTER INDEX ALL ON `AuditModuleEntity` REBUILD
-- ALTER INDEX ALL ON `AuditModule` REBUILD
-- ALTER INDEX ALL ON `AuditResource` REBUILD
-- ALTER INDEX ALL ON `AuditHostSystem` REBUILD
-- ALTER INDEX ALL ON `AuditUserAgent` REBUILD
-- ALTER INDEX ALL ON `AuditSource` REBUILD
-- ALTER INDEX ALL ON `AuditSession` REBUILD
-- ALTER INDEX ALL ON `AuditUser` REBUILD
-- ALTER INDEX ALL ON `AuditAccessType` REBUILD
-- ALTER INDEX ALL ON `AuditType` REBUILD

CREATE TABLE `AuditType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NULL
);
-- Index on the AuditType table's name field.
CREATE INDEX `I_AuditType_name` ON `AuditType` (`name`);

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Login', 'Log in to the system' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Logout', 'Log out of the system' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Read List', 'Read entity list from the system' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Read List (Redacted)', 'Read of redacted entity list from the system' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Read Entity', 'Read entity from the system' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Read Entity (Redacted)', 'Read of redacted entity from the system' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Create Entity', 'Create entity in the system' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Update Entity', 'Update entity in the system' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Delete Entity', 'Delete entity from the system' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Write List', 'Write list of entities to the system' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Load Page', 'Load page in the system' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Confirmation Requested', 'Confirmation for operation was requested of the user by the system' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Confirmation Granted', 'Confirmation for operation was granted to the system by the user' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Confirmation Denied', 'Confirmation for operation was denied to the system by the user' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Search', 'Search of the system' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Context Set', 'Application context was set by the user' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Unauthorized Access Attempt', 'An attempt was made to access an unauthorized resource' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Error', 'An error was encountered' );

INSERT INTO `AuditType` ( `name`, `description` ) VALUES  ( 'Miscellaneous', 'Miscellaneous event' );


CREATE TABLE `AuditAccessType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NULL
);
-- Index on the AuditAccessType table's name field.
CREATE INDEX `I_AuditAccessType_name` ON `AuditAccessType` (`name`);

INSERT INTO `AuditAccessType` ( `name`, `description` ) VALUES  ( 'Web Browser', 'User connecting with a web browser to access the system.' );

INSERT INTO `AuditAccessType` ( `name`, `description` ) VALUES  ( 'API Request', 'Request made by other software to access the system.' );

INSERT INTO `AuditAccessType` ( `name`, `description` ) VALUES  ( 'Ambiguous', 'Ambiguous access type.  Could be end user or other software.' );


CREATE TABLE `AuditUser`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(500) NOT NULL UNIQUE,
	`comments` VARCHAR(1000) NULL,
	`firstAccess` DATETIME NULL
);
-- Index on the AuditUser table's name field.
CREATE INDEX `I_AuditUser_name` ON `AuditUser` (`name`);


CREATE TABLE `AuditSession`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(500) NOT NULL UNIQUE,
	`comments` VARCHAR(1000) NULL,
	`firstAccess` DATETIME NULL
);
-- Index on the AuditSession table's name field.
CREATE INDEX `I_AuditSession_name` ON `AuditSession` (`name`);


CREATE TABLE `AuditSource`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(500) NOT NULL UNIQUE,
	`comments` VARCHAR(1000) NULL,
	`firstAccess` DATETIME NULL
);
-- Index on the AuditSource table's name field.
CREATE INDEX `I_AuditSource_name` ON `AuditSource` (`name`);


CREATE TABLE `AuditUserAgent`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(500) NOT NULL UNIQUE,
	`comments` VARCHAR(1000) NULL,
	`firstAccess` DATETIME NULL
);
-- Index on the AuditUserAgent table's name field.
CREATE INDEX `I_AuditUserAgent_name` ON `AuditUserAgent` (`name`);


CREATE TABLE `AuditHostSystem`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(500) NOT NULL UNIQUE,
	`comments` VARCHAR(1000) NULL,
	`firstAccess` DATETIME NULL
);
-- Index on the AuditHostSystem table's name field.
CREATE INDEX `I_AuditHostSystem_name` ON `AuditHostSystem` (`name`);


CREATE TABLE `AuditResource`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(850) NOT NULL UNIQUE,
	`comments` VARCHAR(1000) NULL,
	`firstAccess` DATETIME NULL
);
-- Index on the AuditResource table's name field.
CREATE INDEX `I_AuditResource_name` ON `AuditResource` (`name`);


CREATE TABLE `AuditModule`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(500) NOT NULL UNIQUE,
	`comments` VARCHAR(1000) NULL,
	`firstAccess` DATETIME NULL
);
-- Index on the AuditModule table's name field.
CREATE INDEX `I_AuditModule_name` ON `AuditModule` (`name`);


CREATE TABLE `AuditModuleEntity`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`auditModuleId` INT NOT NULL,		-- Link to the AuditModule table.
	`name` VARCHAR(500) NOT NULL,
	`comments` VARCHAR(1000) NULL,
	`firstAccess` DATETIME NULL,
	FOREIGN KEY (`auditModuleId`) REFERENCES `AuditModule`(`id`)		-- Foreign key to the AuditModule table.
);
-- Index on the AuditModuleEntity table's auditModuleId field.
CREATE INDEX `I_AuditModuleEntity_auditModuleId` ON `AuditModuleEntity` (`auditModuleId`);

-- Index on the AuditModuleEntity table's auditModuleId,name fields.
CREATE INDEX `I_AuditModuleEntity_auditModuleId_name` ON `AuditModuleEntity` (`auditModuleId`, `name`);


CREATE TABLE `AuditEvent`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`startTime` DATETIME NOT NULL,
	`stopTime` DATETIME NOT NULL,
	`completedSuccessfully` BIT NOT NULL DEFAULT 1,
	`auditUserId` INT NOT NULL,		-- Link to the AuditUser table.
	`auditSessionId` INT NOT NULL,		-- Link to the AuditSession table.
	`auditTypeId` INT NOT NULL,		-- Link to the AuditType table.
	`auditAccessTypeId` INT NOT NULL,		-- Link to the AuditAccessType table.
	`auditSourceId` INT NOT NULL,		-- Link to the AuditSource table.
	`auditUserAgentId` INT NOT NULL,		-- Link to the AuditUserAgent table.
	`auditModuleId` INT NOT NULL,		-- Link to the AuditModule table.
	`auditModuleEntityId` INT NOT NULL,		-- Link to the AuditModuleEntity table.
	`auditResourceId` INT NOT NULL,		-- Link to the AuditResource table.
	`auditHostSystemId` INT NOT NULL,		-- Link to the AuditHostSystem table.
	`primaryKey` VARCHAR(250) NULL,
	`threadId` INT NULL,
	`message` TEXT NOT NULL,
	FOREIGN KEY (`auditUserId`) REFERENCES `AuditUser`(`id`),		-- Foreign key to the AuditUser table.
	FOREIGN KEY (`auditSessionId`) REFERENCES `AuditSession`(`id`),		-- Foreign key to the AuditSession table.
	FOREIGN KEY (`auditTypeId`) REFERENCES `AuditType`(`id`),		-- Foreign key to the AuditType table.
	FOREIGN KEY (`auditAccessTypeId`) REFERENCES `AuditAccessType`(`id`),		-- Foreign key to the AuditAccessType table.
	FOREIGN KEY (`auditSourceId`) REFERENCES `AuditSource`(`id`),		-- Foreign key to the AuditSource table.
	FOREIGN KEY (`auditUserAgentId`) REFERENCES `AuditUserAgent`(`id`),		-- Foreign key to the AuditUserAgent table.
	FOREIGN KEY (`auditModuleId`) REFERENCES `AuditModule`(`id`),		-- Foreign key to the AuditModule table.
	FOREIGN KEY (`auditModuleEntityId`) REFERENCES `AuditModuleEntity`(`id`),		-- Foreign key to the AuditModuleEntity table.
	FOREIGN KEY (`auditResourceId`) REFERENCES `AuditResource`(`id`),		-- Foreign key to the AuditResource table.
	FOREIGN KEY (`auditHostSystemId`) REFERENCES `AuditHostSystem`(`id`)		-- Foreign key to the AuditHostSystem table.
);
-- Index on the AuditEvent table's startTime field.
CREATE INDEX `I_AuditEvent_startTime` ON `AuditEvent` (`startTime`);

-- Index on the AuditEvent table's stopTime field.
CREATE INDEX `I_AuditEvent_stopTime` ON `AuditEvent` (`stopTime`);

-- Index on the AuditEvent table's auditUserId field.
CREATE INDEX `I_AuditEvent_auditUserId` ON `AuditEvent` (`auditUserId`);

-- Index on the AuditEvent table's auditSessionId field.
CREATE INDEX `I_AuditEvent_auditSessionId` ON `AuditEvent` (`auditSessionId`);

-- Index on the AuditEvent table's auditTypeId field.
CREATE INDEX `I_AuditEvent_auditTypeId` ON `AuditEvent` (`auditTypeId`);

-- Index on the AuditEvent table's auditAccessTypeId field.
CREATE INDEX `I_AuditEvent_auditAccessTypeId` ON `AuditEvent` (`auditAccessTypeId`);

-- Index on the AuditEvent table's auditSourceId field.
CREATE INDEX `I_AuditEvent_auditSourceId` ON `AuditEvent` (`auditSourceId`);

-- Index on the AuditEvent table's auditUserAgentId field.
CREATE INDEX `I_AuditEvent_auditUserAgentId` ON `AuditEvent` (`auditUserAgentId`);

-- Index on the AuditEvent table's auditModuleId field.
CREATE INDEX `I_AuditEvent_auditModuleId` ON `AuditEvent` (`auditModuleId`);

-- Index on the AuditEvent table's auditModuleEntityId field.
CREATE INDEX `I_AuditEvent_auditModuleEntityId` ON `AuditEvent` (`auditModuleEntityId`);

-- Index on the AuditEvent table's auditResourceId field.
CREATE INDEX `I_AuditEvent_auditResourceId` ON `AuditEvent` (`auditResourceId`);

-- Index on the AuditEvent table's auditHostSystemId field.
CREATE INDEX `I_AuditEvent_auditHostSystemId` ON `AuditEvent` (`auditHostSystemId`);


CREATE TABLE `AuditEventEntityState`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`auditEventId` INT NOT NULL,		-- Link to the AuditEvent table.
	`beforeState` TEXT NULL,
	`afterState` TEXT NULL,
	FOREIGN KEY (`auditEventId`) REFERENCES `AuditEvent`(`id`)		-- Foreign key to the AuditEvent table.
);
-- Index on the AuditEventEntityState table's auditEventId field.
CREATE INDEX `I_AuditEventEntityState_auditEventId` ON `AuditEventEntityState` (`auditEventId`);


CREATE TABLE `AuditEventErrorMessage`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`auditEventId` INT NOT NULL,		-- Link to the AuditEvent table.
	`errorMessage` TEXT NOT NULL,
	FOREIGN KEY (`auditEventId`) REFERENCES `AuditEvent`(`id`)		-- Foreign key to the AuditEvent table.
);
-- Index on the AuditEventErrorMessage table's auditEventId field.
CREATE INDEX `I_AuditEventErrorMessage_auditEventId` ON `AuditEventErrorMessage` (`auditEventId`);


CREATE TABLE `AuditPlanB`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`startTime` DATETIME NOT NULL,
	`stopTime` DATETIME NOT NULL,
	`completedSuccessfully` BIT NOT NULL DEFAULT 1,
	`user` VARCHAR(100) NULL,
	`session` VARCHAR(100) NULL,
	`type` VARCHAR(100) NULL,
	`accessType` VARCHAR(100) NULL,
	`source` VARCHAR(50) NULL,
	`userAgent` VARCHAR(100) NULL,
	`module` VARCHAR(100) NULL,
	`moduleEntity` VARCHAR(100) NULL,
	`resource` VARCHAR(500) NULL,
	`hostSystem` VARCHAR(50) NULL,
	`primaryKey` VARCHAR(250) NULL,
	`threadId` INT NULL,
	`message` TEXT NULL,
	`beforeState` TEXT NULL,
	`afterState` TEXT NULL,
	`errorMessage` TEXT NULL,
	`exceptionText` TEXT NULL
);
-- Index on the AuditPlanB table's startTime field.
CREATE INDEX `I_AuditPlanB_startTime` ON `AuditPlanB` (`startTime`);

-- Index on the AuditPlanB table's stopTime field.
CREATE INDEX `I_AuditPlanB_stopTime` ON `AuditPlanB` (`stopTime`);


CREATE TABLE `ExternalCommunication`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`timeStamp` DATETIME NULL,
	`auditUserId` INT NULL,		-- Link to the AuditUser table.
	`communicationType` VARCHAR(100) NULL,
	`subject` VARCHAR(2000) NULL,
	`message` TEXT NULL,
	`completedSuccessfully` BIT NOT NULL DEFAULT 1,
	`responseMessage` TEXT NULL,
	`exceptionText` TEXT NULL,
	FOREIGN KEY (`auditUserId`) REFERENCES `AuditUser`(`id`)		-- Foreign key to the AuditUser table.
);
-- Index on the ExternalCommunication table's auditUserId field.
CREATE INDEX `I_ExternalCommunication_auditUserId` ON `ExternalCommunication` (`auditUserId`);


CREATE TABLE `ExternalCommunicationRecipient`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`externalCommunicationId` INT NULL,		-- Link to the ExternalCommunication table.
	`recipient` VARCHAR(100) NULL,
	`type` VARCHAR(50) NULL,
	FOREIGN KEY (`externalCommunicationId`) REFERENCES `ExternalCommunication`(`id`)		-- Foreign key to the ExternalCommunication table.
);
-- Index on the ExternalCommunicationRecipient table's externalCommunicationId field.
CREATE INDEX `I_ExternalCommunicationRecipient_externalCommunicationId` ON `ExternalCommunicationRecipient` (`externalCommunicationId`);


