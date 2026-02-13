/*
BMC (Brick Machine Construction) database schema.
This is a multi-tenant interactive building and physics simulation platform for LEGO and LEGO Technic models.
It supports a parts catalog with connector metadata, 3D model building with placement and connection tracking,
and physics simulation of mechanical assemblies.
All operational tables include multi-tenant support, versioning where appropriate, auditing, and security controls.
*/
CREATE DATABASE `BMC`;

USE `BMC`;

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE `BrickConnection`
-- DROP TABLE `PlacedBrickChangeHistory`
-- DROP TABLE `PlacedBrick`
-- DROP TABLE `ProjectChangeHistory`
-- DROP TABLE `Project`
-- DROP TABLE `BrickPartColour`
-- DROP TABLE `BrickPartConnector`
-- DROP TABLE `BrickPartChangeHistory`
-- DROP TABLE `BrickPart`
-- DROP TABLE `BrickColour`
-- DROP TABLE `ConnectorType`
-- DROP TABLE `BrickCategory`

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON `BrickConnection` DISABLE
-- ALTER INDEX ALL ON `PlacedBrickChangeHistory` DISABLE
-- ALTER INDEX ALL ON `PlacedBrick` DISABLE
-- ALTER INDEX ALL ON `ProjectChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Project` DISABLE
-- ALTER INDEX ALL ON `BrickPartColour` DISABLE
-- ALTER INDEX ALL ON `BrickPartConnector` DISABLE
-- ALTER INDEX ALL ON `BrickPartChangeHistory` DISABLE
-- ALTER INDEX ALL ON `BrickPart` DISABLE
-- ALTER INDEX ALL ON `BrickColour` DISABLE
-- ALTER INDEX ALL ON `ConnectorType` DISABLE
-- ALTER INDEX ALL ON `BrickCategory` DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON `BrickConnection` REBUILD
-- ALTER INDEX ALL ON `PlacedBrickChangeHistory` REBUILD
-- ALTER INDEX ALL ON `PlacedBrick` REBUILD
-- ALTER INDEX ALL ON `ProjectChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Project` REBUILD
-- ALTER INDEX ALL ON `BrickPartColour` REBUILD
-- ALTER INDEX ALL ON `BrickPartConnector` REBUILD
-- ALTER INDEX ALL ON `BrickPartChangeHistory` REBUILD
-- ALTER INDEX ALL ON `BrickPart` REBUILD
-- ALTER INDEX ALL ON `BrickColour` REBUILD
-- ALTER INDEX ALL ON `ConnectorType` REBUILD
-- ALTER INDEX ALL ON `BrickCategory` REBUILD

-- Master list of part categories (Beam, Plate, Gear, Axle, Motor, Pneumatic, etc.)
CREATE TABLE `BrickCategory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the BrickCategory table's name field.
CREATE INDEX `I_BrickCategory_name` ON `BrickCategory` (`name`);

-- Index on the BrickCategory table's active field.
CREATE INDEX `I_BrickCategory_active` ON `BrickCategory` (`active`);

-- Index on the BrickCategory table's deleted field.
CREATE INDEX `I_BrickCategory_deleted` ON `BrickCategory` (`deleted`);

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Plate', 'Standard plates of various sizes', 1, 'b1c10001-0001-4000-8000-000000000001' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Brick', 'Standard bricks of various sizes', 2, 'b1c10001-0001-4000-8000-000000000002' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Beam', 'Technic beams and liftarms', 10, 'b1c10001-0001-4000-8000-000000000010' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Pin', 'Technic pins and connectors', 11, 'b1c10001-0001-4000-8000-000000000011' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Axle', 'Technic axles of various lengths', 12, 'b1c10001-0001-4000-8000-000000000012' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Gear', 'Technic gears of various tooth counts', 13, 'b1c10001-0001-4000-8000-000000000013' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Motor', 'Powered motors (Power Functions, Powered Up)', 14, 'b1c10001-0001-4000-8000-000000000014' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Pneumatic', 'Pneumatic cylinders, pumps, and tubing', 15, 'b1c10001-0001-4000-8000-000000000015' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Differential', 'Differential gear assemblies', 16, 'b1c10001-0001-4000-8000-000000000016' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Panel', 'Panels, fairings, and body pieces', 20, 'b1c10001-0001-4000-8000-000000000020' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Wheel', 'Wheels, tyres, and rims', 21, 'b1c10001-0001-4000-8000-000000000021' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Decorative', 'Decorative, printed, and sticker parts', 30, 'b1c10001-0001-4000-8000-000000000030' );


-- Master list of physical connection types that define how parts can join together.
CREATE TABLE `ConnectorType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`degreesOfFreedom` INT NULL,		-- Number of degrees of freedom when connected (0=fixed, 1=rotation, 2=rotation+slide)
	`allowsRotation` BIT NOT NULL DEFAULT 0,		-- Whether this connection allows rotation around its axis
	`allowsSlide` BIT NOT NULL DEFAULT 0,		-- Whether this connection allows sliding along its axis
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the ConnectorType table's name field.
CREATE INDEX `I_ConnectorType_name` ON `ConnectorType` (`name`);

-- Index on the ConnectorType table's active field.
CREATE INDEX `I_ConnectorType_active` ON `ConnectorType` (`active`);

-- Index on the ConnectorType table's deleted field.
CREATE INDEX `I_ConnectorType_deleted` ON `ConnectorType` (`deleted`);

INSERT INTO `ConnectorType` ( `name`, `description`, `degreesOfFreedom`, `allowsRotation`, `allowsSlide`, `sequence`, `objectGuid` ) VALUES  ( 'Stud', 'Standard LEGO stud (male connector)', 0, false, false, 1, 'c0110001-0001-4000-8000-000000000001' );

INSERT INTO `ConnectorType` ( `name`, `description`, `degreesOfFreedom`, `allowsRotation`, `allowsSlide`, `sequence`, `objectGuid` ) VALUES  ( 'AntiStud', 'Standard LEGO anti-stud receptacle (female connector)', 0, false, false, 2, 'c0110001-0001-4000-8000-000000000002' );

INSERT INTO `ConnectorType` ( `name`, `description`, `degreesOfFreedom`, `allowsRotation`, `allowsSlide`, `sequence`, `objectGuid` ) VALUES  ( 'PinHole', 'Technic pin hole — accepts a pin for rotational connection', 1, true, false, 10, 'c0110001-0001-4000-8000-000000000010' );

INSERT INTO `ConnectorType` ( `name`, `description`, `degreesOfFreedom`, `allowsRotation`, `allowsSlide`, `sequence`, `objectGuid` ) VALUES  ( 'Pin', 'Technic pin — inserts into a pin hole', 1, true, false, 11, 'c0110001-0001-4000-8000-000000000011' );

INSERT INTO `ConnectorType` ( `name`, `description`, `degreesOfFreedom`, `allowsRotation`, `allowsSlide`, `sequence`, `objectGuid` ) VALUES  ( 'AxleHole', 'Technic axle hole — accepts an axle for locked rotational transfer', 1, true, false, 12, 'c0110001-0001-4000-8000-000000000012' );

INSERT INTO `ConnectorType` ( `name`, `description`, `degreesOfFreedom`, `allowsRotation`, `allowsSlide`, `sequence`, `objectGuid` ) VALUES  ( 'AxleEnd', 'End of a Technic axle — inserts into an axle hole', 1, true, false, 13, 'c0110001-0001-4000-8000-000000000013' );

INSERT INTO `ConnectorType` ( `name`, `description`, `degreesOfFreedom`, `allowsRotation`, `allowsSlide`, `sequence`, `objectGuid` ) VALUES  ( 'BallJointSocket', 'Ball joint socket — accepts a ball joint for multi-axis rotation', 2, true, false, 20, 'c0110001-0001-4000-8000-000000000020' );

INSERT INTO `ConnectorType` ( `name`, `description`, `degreesOfFreedom`, `allowsRotation`, `allowsSlide`, `sequence`, `objectGuid` ) VALUES  ( 'BallJoint', 'Ball joint — inserts into a ball joint socket', 2, true, false, 21, 'c0110001-0001-4000-8000-000000000021' );


-- Colour definitions for brick parts. Compatible with the LDraw colour standard.
CREATE TABLE `BrickColour`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`ldrawColourCode` INT NOT NULL,		-- LDraw standard colour code number
	`hexRgb` VARCHAR(10) NULL,		-- Hex RGB colour value (e.g. #FF0000)
	`alpha` INT NULL,		-- Alpha transparency value (0-255, 255 = fully opaque)
	`isTransparent` BIT NOT NULL DEFAULT 0,		-- Whether this colour is transparent
	`isMetallic` BIT NOT NULL DEFAULT 0,		-- Whether this colour has a metallic finish
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE `UC_BrickColour_ldrawColourCode_Unique`( `ldrawColourCode` ) 		-- Uniqueness enforced on the BrickColour table's ldrawColourCode field.
);
-- Index on the BrickColour table's name field.
CREATE INDEX `I_BrickColour_name` ON `BrickColour` (`name`);

-- Index on the BrickColour table's active field.
CREATE INDEX `I_BrickColour_active` ON `BrickColour` (`active`);

-- Index on the BrickColour table's deleted field.
CREATE INDEX `I_BrickColour_deleted` ON `BrickColour` (`deleted`);

INSERT INTO `BrickColour` ( `name`, `ldrawColourCode`, `hexRgb`, `alpha`, `isTransparent`, `isMetallic`, `sequence`, `objectGuid` ) VALUES  ( 'Black', 0, '#1B2A34', 255, false, false, 1, 'c0100001-0001-4000-8000-000000000001' );

INSERT INTO `BrickColour` ( `name`, `ldrawColourCode`, `hexRgb`, `alpha`, `isTransparent`, `isMetallic`, `sequence`, `objectGuid` ) VALUES  ( 'Blue', 1, '#1E5AA8', 255, false, false, 2, 'c0100001-0001-4000-8000-000000000002' );

INSERT INTO `BrickColour` ( `name`, `ldrawColourCode`, `hexRgb`, `alpha`, `isTransparent`, `isMetallic`, `sequence`, `objectGuid` ) VALUES  ( 'Green', 2, '#00852B', 255, false, false, 3, 'c0100001-0001-4000-8000-000000000003' );

INSERT INTO `BrickColour` ( `name`, `ldrawColourCode`, `hexRgb`, `alpha`, `isTransparent`, `isMetallic`, `sequence`, `objectGuid` ) VALUES  ( 'Red', 4, '#B40000', 255, false, false, 4, 'c0100001-0001-4000-8000-000000000004' );

INSERT INTO `BrickColour` ( `name`, `ldrawColourCode`, `hexRgb`, `alpha`, `isTransparent`, `isMetallic`, `sequence`, `objectGuid` ) VALUES  ( 'Yellow', 14, '#FFD67F', 255, false, false, 5, 'c0100001-0001-4000-8000-000000000005' );

INSERT INTO `BrickColour` ( `name`, `ldrawColourCode`, `hexRgb`, `alpha`, `isTransparent`, `isMetallic`, `sequence`, `objectGuid` ) VALUES  ( 'White', 15, '#F4F4F4', 255, false, false, 6, 'c0100001-0001-4000-8000-000000000006' );

INSERT INTO `BrickColour` ( `name`, `ldrawColourCode`, `hexRgb`, `alpha`, `isTransparent`, `isMetallic`, `sequence`, `objectGuid` ) VALUES  ( 'Light Bluish Grey', 71, '#A0A5A9', 255, false, false, 7, 'c0100001-0001-4000-8000-000000000007' );

INSERT INTO `BrickColour` ( `name`, `ldrawColourCode`, `hexRgb`, `alpha`, `isTransparent`, `isMetallic`, `sequence`, `objectGuid` ) VALUES  ( 'Dark Bluish Grey', 72, '#6C6E68', 255, false, false, 8, 'c0100001-0001-4000-8000-000000000008' );


-- Individual brick part definitions. Each row represents a unique part shape (independent of colour).
CREATE TABLE `BrickPart`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`ldrawPartId` VARCHAR(100) NOT NULL,		-- LDraw part ID (e.g. 3001, 32523) — the canonical identifier in the LDraw parts library
	`brickCategoryId` INT NULL,		-- The category this part belongs to
	`widthLdu` FLOAT NOT NULL,		-- Part width in LDraw units
	`heightLdu` FLOAT NOT NULL,		-- Part height in LDraw units
	`depthLdu` FLOAT NOT NULL,		-- Part depth in LDraw units
	`massGrams` FLOAT NOT NULL,		-- Part mass in grams (for physics simulation)
	`geometryFilePath` VARCHAR(250) NULL,		-- Relative path to the LDraw .dat geometry file
	`toothCount` INT NULL,		-- For gears: number of teeth. Null for non-gear parts.
	`gearRatio` FLOAT NULL,		-- For gears: effective gear ratio relative to a base gear. Null for non-gear parts.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`brickCategoryId`) REFERENCES `BrickCategory`(`id`),		-- Foreign key to the BrickCategory table.
	UNIQUE `UC_BrickPart_ldrawPartId_Unique`( `ldrawPartId` ) 		-- Uniqueness enforced on the BrickPart table's ldrawPartId field.
);
-- Index on the BrickPart table's name field.
CREATE INDEX `I_BrickPart_name` ON `BrickPart` (`name`);

-- Index on the BrickPart table's brickCategoryId field.
CREATE INDEX `I_BrickPart_brickCategoryId` ON `BrickPart` (`brickCategoryId`);

-- Index on the BrickPart table's active field.
CREATE INDEX `I_BrickPart_active` ON `BrickPart` (`active`);

-- Index on the BrickPart table's deleted field.
CREATE INDEX `I_BrickPart_deleted` ON `BrickPart` (`deleted`);


-- The change history for records from the BrickPart table.
CREATE TABLE `BrickPartChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`brickPartId` INT NOT NULL,		-- Link to the BrickPart table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`brickPartId`) REFERENCES `BrickPart`(`id`)		-- Foreign key to the BrickPart table.
);
-- Index on the BrickPartChangeHistory table's versionNumber field.
CREATE INDEX `I_BrickPartChangeHistory_versionNumber` ON `BrickPartChangeHistory` (`versionNumber`);

-- Index on the BrickPartChangeHistory table's timeStamp field.
CREATE INDEX `I_BrickPartChangeHistory_timeStamp` ON `BrickPartChangeHistory` (`timeStamp`);

-- Index on the BrickPartChangeHistory table's userId field.
CREATE INDEX `I_BrickPartChangeHistory_userId` ON `BrickPartChangeHistory` (`userId`);

-- Index on the BrickPartChangeHistory table's brickPartId field.
CREATE INDEX `I_BrickPartChangeHistory_brickPartId` ON `BrickPartChangeHistory` (`brickPartId`, `versionNumber`, `timeStamp`, `userId`);


-- Defines the physical connection points on each brick part, including position and connector type.
CREATE TABLE `BrickPartConnector`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`brickPartId` INT NULL,		-- The part this connector belongs to
	`connectorTypeId` INT NULL,		-- The type of connector (Stud, PinHole, AxleHole, etc.)
	`positionX` FLOAT NULL,		-- X position of connector relative to part origin (LDU)
	`positionY` FLOAT NULL,		-- Y position of connector relative to part origin (LDU)
	`positionZ` FLOAT NULL,		-- Z position of connector relative to part origin (LDU)
	`orientationX` FLOAT NULL,		-- X component of connector direction unit vector
	`orientationY` FLOAT NULL,		-- Y component of connector direction unit vector
	`orientationZ` FLOAT NULL,		-- Z component of connector direction unit vector
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`brickPartId`) REFERENCES `BrickPart`(`id`),		-- Foreign key to the BrickPart table.
	FOREIGN KEY (`connectorTypeId`) REFERENCES `ConnectorType`(`id`)		-- Foreign key to the ConnectorType table.
);
-- Index on the BrickPartConnector table's brickPartId field.
CREATE INDEX `I_BrickPartConnector_brickPartId` ON `BrickPartConnector` (`brickPartId`);

-- Index on the BrickPartConnector table's connectorTypeId field.
CREATE INDEX `I_BrickPartConnector_connectorTypeId` ON `BrickPartConnector` (`connectorTypeId`);

-- Index on the BrickPartConnector table's active field.
CREATE INDEX `I_BrickPartConnector_active` ON `BrickPartConnector` (`active`);

-- Index on the BrickPartConnector table's deleted field.
CREATE INDEX `I_BrickPartConnector_deleted` ON `BrickPartConnector` (`deleted`);


-- Maps which colours each brick part is available in. A part can exist in multiple colours.
CREATE TABLE `BrickPartColour`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`brickPartId` INT NOT NULL,		-- The brick part
	`brickColourId` INT NOT NULL,		-- The colour this part is available in
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`brickPartId`) REFERENCES `BrickPart`(`id`),		-- Foreign key to the BrickPart table.
	FOREIGN KEY (`brickColourId`) REFERENCES `BrickColour`(`id`),		-- Foreign key to the BrickColour table.
	UNIQUE `UC_BrickPartColour_brickPartId_brickColourId_Unique`( `brickPartId`, `brickColourId` ) 		-- Uniqueness enforced on the BrickPartColour table's brickPartId and brickColourId fields.
);
-- Index on the BrickPartColour table's brickPartId field.
CREATE INDEX `I_BrickPartColour_brickPartId` ON `BrickPartColour` (`brickPartId`);

-- Index on the BrickPartColour table's brickColourId field.
CREATE INDEX `I_BrickPartColour_brickColourId` ON `BrickPartColour` (`brickColourId`);

-- Index on the BrickPartColour table's active field.
CREATE INDEX `I_BrickPartColour_active` ON `BrickPartColour` (`active`);

-- Index on the BrickPartColour table's deleted field.
CREATE INDEX `I_BrickPartColour_deleted` ON `BrickPartColour` (`deleted`);


-- A user's building project. Contains placed bricks and their connections to form a model.
CREATE TABLE `Project`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`notes` TEXT NULL,		-- Free-form notes about the project
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE `UC_Project_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the Project table's tenantGuid and name fields.
);
-- Index on the Project table's tenantGuid field.
CREATE INDEX `I_Project_tenantGuid` ON `Project` (`tenantGuid`);

-- Index on the Project table's tenantGuid,name fields.
CREATE INDEX `I_Project_tenantGuid_name` ON `Project` (`tenantGuid`, `name`);

-- Index on the Project table's tenantGuid,active fields.
CREATE INDEX `I_Project_tenantGuid_active` ON `Project` (`tenantGuid`, `active`);

-- Index on the Project table's tenantGuid,deleted fields.
CREATE INDEX `I_Project_tenantGuid_deleted` ON `Project` (`tenantGuid`, `deleted`);


-- The change history for records from the Project table.
CREATE TABLE `ProjectChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`projectId` INT NOT NULL,		-- Link to the Project table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`projectId`) REFERENCES `Project`(`id`)		-- Foreign key to the Project table.
);
-- Index on the ProjectChangeHistory table's tenantGuid field.
CREATE INDEX `I_ProjectChangeHistory_tenantGuid` ON `ProjectChangeHistory` (`tenantGuid`);

-- Index on the ProjectChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_ProjectChangeHistory_tenantGuid_versionNumber` ON `ProjectChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the ProjectChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_ProjectChangeHistory_tenantGuid_timeStamp` ON `ProjectChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the ProjectChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_ProjectChangeHistory_tenantGuid_userId` ON `ProjectChangeHistory` (`tenantGuid`, `userId`);

-- Index on the ProjectChangeHistory table's tenantGuid,projectId fields.
CREATE INDEX `I_ProjectChangeHistory_tenantGuid_projectId` ON `ProjectChangeHistory` (`tenantGuid`, `projectId`, `versionNumber`, `timeStamp`, `userId`);


-- An instance of a brick part placed within a project. Tracks position, rotation, and colour.
CREATE TABLE `PlacedBrick`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`projectId` INT NULL,		-- The project this brick is placed in
	`brickPartId` INT NULL,		-- The part definition being placed
	`brickColourId` INT NULL,		-- The colour of this placed brick instance
	`positionX` FLOAT NULL,		-- X position in world coordinates (LDU)
	`positionY` FLOAT NULL,		-- Y position in world coordinates (LDU)
	`positionZ` FLOAT NULL,		-- Z position in world coordinates (LDU)
	`rotationX` FLOAT NULL,		-- Quaternion X component
	`rotationY` FLOAT NULL,		-- Quaternion Y component
	`rotationZ` FLOAT NULL,		-- Quaternion Z component
	`rotationW` FLOAT NULL,		-- Quaternion W component
	`buildStepNumber` INT NULL,		-- Optional build step number for instruction ordering
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`projectId`) REFERENCES `Project`(`id`),		-- Foreign key to the Project table.
	FOREIGN KEY (`brickPartId`) REFERENCES `BrickPart`(`id`),		-- Foreign key to the BrickPart table.
	FOREIGN KEY (`brickColourId`) REFERENCES `BrickColour`(`id`)		-- Foreign key to the BrickColour table.
);
-- Index on the PlacedBrick table's tenantGuid field.
CREATE INDEX `I_PlacedBrick_tenantGuid` ON `PlacedBrick` (`tenantGuid`);

-- Index on the PlacedBrick table's tenantGuid,projectId fields.
CREATE INDEX `I_PlacedBrick_tenantGuid_projectId` ON `PlacedBrick` (`tenantGuid`, `projectId`);

-- Index on the PlacedBrick table's tenantGuid,brickPartId fields.
CREATE INDEX `I_PlacedBrick_tenantGuid_brickPartId` ON `PlacedBrick` (`tenantGuid`, `brickPartId`);

-- Index on the PlacedBrick table's tenantGuid,brickColourId fields.
CREATE INDEX `I_PlacedBrick_tenantGuid_brickColourId` ON `PlacedBrick` (`tenantGuid`, `brickColourId`);

-- Index on the PlacedBrick table's tenantGuid,active fields.
CREATE INDEX `I_PlacedBrick_tenantGuid_active` ON `PlacedBrick` (`tenantGuid`, `active`);

-- Index on the PlacedBrick table's tenantGuid,deleted fields.
CREATE INDEX `I_PlacedBrick_tenantGuid_deleted` ON `PlacedBrick` (`tenantGuid`, `deleted`);


-- The change history for records from the PlacedBrick table.
CREATE TABLE `PlacedBrickChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`placedBrickId` INT NOT NULL,		-- Link to the PlacedBrick table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`placedBrickId`) REFERENCES `PlacedBrick`(`id`)		-- Foreign key to the PlacedBrick table.
);
-- Index on the PlacedBrickChangeHistory table's tenantGuid field.
CREATE INDEX `I_PlacedBrickChangeHistory_tenantGuid` ON `PlacedBrickChangeHistory` (`tenantGuid`);

-- Index on the PlacedBrickChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_PlacedBrickChangeHistory_tenantGuid_versionNumber` ON `PlacedBrickChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the PlacedBrickChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_PlacedBrickChangeHistory_tenantGuid_timeStamp` ON `PlacedBrickChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the PlacedBrickChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_PlacedBrickChangeHistory_tenantGuid_userId` ON `PlacedBrickChangeHistory` (`tenantGuid`, `userId`);

-- Index on the PlacedBrickChangeHistory table's tenantGuid,placedBrickId fields.
CREATE INDEX `I_PlacedBrickChangeHistory_tenantGuid_placedBrickId` ON `PlacedBrickChangeHistory` (`tenantGuid`, `placedBrickId`, `versionNumber`, `timeStamp`, `userId`);


-- Records which connector on one placed brick is joined to which connector on another placed brick.
CREATE TABLE `BrickConnection`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`projectId` INT NULL,		-- The project this connection belongs to
	`sourcePlacedBrickId` BIGINT NULL,		-- FK to the source PlacedBrick
	`sourceConnectorId` BIGINT NULL,		-- FK to the BrickPartConnector on the source brick
	`targetPlacedBrickId` BIGINT NULL,		-- FK to the target PlacedBrick
	`targetConnectorId` BIGINT NULL,		-- FK to the BrickPartConnector on the target brick
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`projectId`) REFERENCES `Project`(`id`)		-- Foreign key to the Project table.
);
-- Index on the BrickConnection table's tenantGuid field.
CREATE INDEX `I_BrickConnection_tenantGuid` ON `BrickConnection` (`tenantGuid`);

-- Index on the BrickConnection table's tenantGuid,projectId fields.
CREATE INDEX `I_BrickConnection_tenantGuid_projectId` ON `BrickConnection` (`tenantGuid`, `projectId`);

-- Index on the BrickConnection table's tenantGuid,active fields.
CREATE INDEX `I_BrickConnection_tenantGuid_active` ON `BrickConnection` (`tenantGuid`, `active`);

-- Index on the BrickConnection table's tenantGuid,deleted fields.
CREATE INDEX `I_BrickConnection_tenantGuid_deleted` ON `BrickConnection` (`tenantGuid`, `deleted`);


