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
-- DROP TABLE `ProjectExport`
-- DROP TABLE `ExportFormat`
-- DROP TABLE `ProjectRender`
-- DROP TABLE `RenderPreset`
-- DROP TABLE `BuildStepAnnotation`
-- DROP TABLE `BuildStepAnnotationType`
-- DROP TABLE `BuildStepPart`
-- DROP TABLE `BuildManualStep`
-- DROP TABLE `BuildManualPage`
-- DROP TABLE `BuildManualChangeHistory`
-- DROP TABLE `BuildManual`
-- DROP TABLE `UserCollectionSetImport`
-- DROP TABLE `UserWishlistItem`
-- DROP TABLE `UserCollectionPart`
-- DROP TABLE `UserCollectionChangeHistory`
-- DROP TABLE `UserCollection`
-- DROP TABLE `LegoSetPart`
-- DROP TABLE `LegoSet`
-- DROP TABLE `LegoTheme`
-- DROP TABLE `ProjectReferenceImage`
-- DROP TABLE `ProjectCameraPreset`
-- DROP TABLE `ProjectTagAssignment`
-- DROP TABLE `ProjectTag`
-- DROP TABLE `SubmodelPlacedBrick`
-- DROP TABLE `SubmodelChangeHistory`
-- DROP TABLE `Submodel`
-- DROP TABLE `BrickConnection`
-- DROP TABLE `PlacedBrickChangeHistory`
-- DROP TABLE `PlacedBrick`
-- DROP TABLE `ProjectChangeHistory`
-- DROP TABLE `Project`
-- DROP TABLE `BrickPartColour`
-- DROP TABLE `BrickPartConnector`
-- DROP TABLE `BrickPartChangeHistory`
-- DROP TABLE `BrickPart`
-- DROP TABLE `PartType`
-- DROP TABLE `BrickColour`
-- DROP TABLE `ColourFinish`
-- DROP TABLE `ConnectorType`
-- DROP TABLE `BrickCategory`

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON `ProjectExport` DISABLE
-- ALTER INDEX ALL ON `ExportFormat` DISABLE
-- ALTER INDEX ALL ON `ProjectRender` DISABLE
-- ALTER INDEX ALL ON `RenderPreset` DISABLE
-- ALTER INDEX ALL ON `BuildStepAnnotation` DISABLE
-- ALTER INDEX ALL ON `BuildStepAnnotationType` DISABLE
-- ALTER INDEX ALL ON `BuildStepPart` DISABLE
-- ALTER INDEX ALL ON `BuildManualStep` DISABLE
-- ALTER INDEX ALL ON `BuildManualPage` DISABLE
-- ALTER INDEX ALL ON `BuildManualChangeHistory` DISABLE
-- ALTER INDEX ALL ON `BuildManual` DISABLE
-- ALTER INDEX ALL ON `UserCollectionSetImport` DISABLE
-- ALTER INDEX ALL ON `UserWishlistItem` DISABLE
-- ALTER INDEX ALL ON `UserCollectionPart` DISABLE
-- ALTER INDEX ALL ON `UserCollectionChangeHistory` DISABLE
-- ALTER INDEX ALL ON `UserCollection` DISABLE
-- ALTER INDEX ALL ON `LegoSetPart` DISABLE
-- ALTER INDEX ALL ON `LegoSet` DISABLE
-- ALTER INDEX ALL ON `LegoTheme` DISABLE
-- ALTER INDEX ALL ON `ProjectReferenceImage` DISABLE
-- ALTER INDEX ALL ON `ProjectCameraPreset` DISABLE
-- ALTER INDEX ALL ON `ProjectTagAssignment` DISABLE
-- ALTER INDEX ALL ON `ProjectTag` DISABLE
-- ALTER INDEX ALL ON `SubmodelPlacedBrick` DISABLE
-- ALTER INDEX ALL ON `SubmodelChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Submodel` DISABLE
-- ALTER INDEX ALL ON `BrickConnection` DISABLE
-- ALTER INDEX ALL ON `PlacedBrickChangeHistory` DISABLE
-- ALTER INDEX ALL ON `PlacedBrick` DISABLE
-- ALTER INDEX ALL ON `ProjectChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Project` DISABLE
-- ALTER INDEX ALL ON `BrickPartColour` DISABLE
-- ALTER INDEX ALL ON `BrickPartConnector` DISABLE
-- ALTER INDEX ALL ON `BrickPartChangeHistory` DISABLE
-- ALTER INDEX ALL ON `BrickPart` DISABLE
-- ALTER INDEX ALL ON `PartType` DISABLE
-- ALTER INDEX ALL ON `BrickColour` DISABLE
-- ALTER INDEX ALL ON `ColourFinish` DISABLE
-- ALTER INDEX ALL ON `ConnectorType` DISABLE
-- ALTER INDEX ALL ON `BrickCategory` DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON `ProjectExport` REBUILD
-- ALTER INDEX ALL ON `ExportFormat` REBUILD
-- ALTER INDEX ALL ON `ProjectRender` REBUILD
-- ALTER INDEX ALL ON `RenderPreset` REBUILD
-- ALTER INDEX ALL ON `BuildStepAnnotation` REBUILD
-- ALTER INDEX ALL ON `BuildStepAnnotationType` REBUILD
-- ALTER INDEX ALL ON `BuildStepPart` REBUILD
-- ALTER INDEX ALL ON `BuildManualStep` REBUILD
-- ALTER INDEX ALL ON `BuildManualPage` REBUILD
-- ALTER INDEX ALL ON `BuildManualChangeHistory` REBUILD
-- ALTER INDEX ALL ON `BuildManual` REBUILD
-- ALTER INDEX ALL ON `UserCollectionSetImport` REBUILD
-- ALTER INDEX ALL ON `UserWishlistItem` REBUILD
-- ALTER INDEX ALL ON `UserCollectionPart` REBUILD
-- ALTER INDEX ALL ON `UserCollectionChangeHistory` REBUILD
-- ALTER INDEX ALL ON `UserCollection` REBUILD
-- ALTER INDEX ALL ON `LegoSetPart` REBUILD
-- ALTER INDEX ALL ON `LegoSet` REBUILD
-- ALTER INDEX ALL ON `LegoTheme` REBUILD
-- ALTER INDEX ALL ON `ProjectReferenceImage` REBUILD
-- ALTER INDEX ALL ON `ProjectCameraPreset` REBUILD
-- ALTER INDEX ALL ON `ProjectTagAssignment` REBUILD
-- ALTER INDEX ALL ON `ProjectTag` REBUILD
-- ALTER INDEX ALL ON `SubmodelPlacedBrick` REBUILD
-- ALTER INDEX ALL ON `SubmodelChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Submodel` REBUILD
-- ALTER INDEX ALL ON `BrickConnection` REBUILD
-- ALTER INDEX ALL ON `PlacedBrickChangeHistory` REBUILD
-- ALTER INDEX ALL ON `PlacedBrick` REBUILD
-- ALTER INDEX ALL ON `ProjectChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Project` REBUILD
-- ALTER INDEX ALL ON `BrickPartColour` REBUILD
-- ALTER INDEX ALL ON `BrickPartConnector` REBUILD
-- ALTER INDEX ALL ON `BrickPartChangeHistory` REBUILD
-- ALTER INDEX ALL ON `BrickPart` REBUILD
-- ALTER INDEX ALL ON `PartType` REBUILD
-- ALTER INDEX ALL ON `BrickColour` REBUILD
-- ALTER INDEX ALL ON `ColourFinish` REBUILD
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

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Tile', 'Smooth-top tiles without studs', 3, 'b1c10001-0001-4000-8000-000000000003' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Slope', 'Angled slope bricks and roof pieces', 4, 'b1c10001-0001-4000-8000-000000000004' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Wedge', 'Wedge-shaped plates and bricks', 5, 'b1c10001-0001-4000-8000-000000000005' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Arch', 'Arched bricks and curved elements', 6, 'b1c10001-0001-4000-8000-000000000006' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Cylinder', 'Round bricks, cylinders, and cones', 7, 'b1c10001-0001-4000-8000-000000000007' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Cone', 'Cone-shaped parts', 8, 'b1c10001-0001-4000-8000-000000000008' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Bracket', 'Angle brackets for sideways building', 9, 'b1c10001-0001-4000-8000-000000000009' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Beam', 'Technic beams and liftarms', 10, 'b1c10001-0001-4000-8000-000000000010' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Pin', 'Technic pins and connectors', 11, 'b1c10001-0001-4000-8000-000000000011' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Axle', 'Technic axles of various lengths', 12, 'b1c10001-0001-4000-8000-000000000012' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Gear', 'Technic gears of various tooth counts', 13, 'b1c10001-0001-4000-8000-000000000013' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Motor', 'Powered motors (Power Functions, Powered Up)', 14, 'b1c10001-0001-4000-8000-000000000014' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Pneumatic', 'Pneumatic cylinders, pumps, and tubing', 15, 'b1c10001-0001-4000-8000-000000000015' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Differential', 'Differential gear assemblies', 16, 'b1c10001-0001-4000-8000-000000000016' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Hinge', 'Hinge bricks, plates, and click hinges', 17, 'b1c10001-0001-4000-8000-000000000017' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Panel', 'Panels, fairings, and body pieces', 20, 'b1c10001-0001-4000-8000-000000000020' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Wheel', 'Wheels, tyres, and rims', 21, 'b1c10001-0001-4000-8000-000000000021' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Window', 'Windows, glass, and frames', 22, 'b1c10001-0001-4000-8000-000000000022' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Door', 'Doors and door frames', 23, 'b1c10001-0001-4000-8000-000000000023' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Fence', 'Fences, railings, and barriers', 24, 'b1c10001-0001-4000-8000-000000000024' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Baseplate', 'Baseplates and road plates', 25, 'b1c10001-0001-4000-8000-000000000025' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Bar', 'Bars, antennas, and clips', 26, 'b1c10001-0001-4000-8000-000000000026' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Support', 'Support structures, columns, and pillars', 27, 'b1c10001-0001-4000-8000-000000000027' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Container', 'Boxes, crates, and storage containers', 28, 'b1c10001-0001-4000-8000-000000000028' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Decorative', 'Decorative, printed, and sticker parts', 30, 'b1c10001-0001-4000-8000-000000000030' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Electric', 'Electrical components, lights, and sensors', 31, 'b1c10001-0001-4000-8000-000000000031' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Propeller', 'Propellers, rotors, and blades', 32, 'b1c10001-0001-4000-8000-000000000032' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Wing', 'Wings and aircraft body parts', 33, 'b1c10001-0001-4000-8000-000000000033' );

INSERT INTO `BrickCategory` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Train', 'Train track, wheels, and specialized train parts', 34, 'b1c10001-0001-4000-8000-000000000034' );


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


-- Lookup table of material finish types that define how a colour is rendered (e.g. Solid, Chrome, Rubber).
CREATE TABLE `ColourFinish`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`requiresEnvironmentMap` BIT NOT NULL DEFAULT 0,		-- Whether this finish needs environment mapping for reflections (Chrome, Metal)
	`isMatte` BIT NOT NULL DEFAULT 0,		-- Whether this finish has a matte/non-glossy appearance (Rubber)
	`defaultAlpha` INT NULL,		-- Default alpha for this finish type, null = use colour-specific alpha
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the ColourFinish table's name field.
CREATE INDEX `I_ColourFinish_name` ON `ColourFinish` (`name`);

-- Index on the ColourFinish table's active field.
CREATE INDEX `I_ColourFinish_active` ON `ColourFinish` (`active`);

-- Index on the ColourFinish table's deleted field.
CREATE INDEX `I_ColourFinish_deleted` ON `ColourFinish` (`deleted`);

INSERT INTO `ColourFinish` ( `name`, `description`, `requiresEnvironmentMap`, `isMatte`, `sequence`, `objectGuid` ) VALUES  ( 'Solid', 'Standard opaque plastic finish', false, false, 1, 'cf100001-0001-4000-8000-000000000001' );

INSERT INTO `ColourFinish` ( `name`, `description`, `requiresEnvironmentMap`, `isMatte`, `defaultAlpha`, `sequence`, `objectGuid` ) VALUES  ( 'Transparent', 'See-through plastic finish', false, false, 128, 2, 'cf100001-0001-4000-8000-000000000002' );

INSERT INTO `ColourFinish` ( `name`, `description`, `requiresEnvironmentMap`, `isMatte`, `sequence`, `objectGuid` ) VALUES  ( 'Chrome', 'Highly reflective chrome-plated metal finish', true, false, 3, 'cf100001-0001-4000-8000-000000000003' );

INSERT INTO `ColourFinish` ( `name`, `description`, `requiresEnvironmentMap`, `isMatte`, `sequence`, `objectGuid` ) VALUES  ( 'Pearlescent', 'Iridescent pearl-like plastic finish', true, false, 4, 'cf100001-0001-4000-8000-000000000004' );

INSERT INTO `ColourFinish` ( `name`, `description`, `requiresEnvironmentMap`, `isMatte`, `sequence`, `objectGuid` ) VALUES  ( 'Metal', 'Metallic paint or lacquer finish', true, false, 5, 'cf100001-0001-4000-8000-000000000005' );

INSERT INTO `ColourFinish` ( `name`, `description`, `requiresEnvironmentMap`, `isMatte`, `sequence`, `objectGuid` ) VALUES  ( 'Rubber', 'Matte rubber or soft-touch finish', false, true, 6, 'cf100001-0001-4000-8000-000000000006' );

INSERT INTO `ColourFinish` ( `name`, `description`, `requiresEnvironmentMap`, `isMatte`, `defaultAlpha`, `sequence`, `objectGuid` ) VALUES  ( 'Glitter', 'Transparent plastic with embedded glitter particles', false, false, 128, 7, 'cf100001-0001-4000-8000-000000000007' );

INSERT INTO `ColourFinish` ( `name`, `description`, `requiresEnvironmentMap`, `isMatte`, `sequence`, `objectGuid` ) VALUES  ( 'Speckle', 'Solid plastic with embedded speckle particles', false, false, 8, 'cf100001-0001-4000-8000-000000000008' );

INSERT INTO `ColourFinish` ( `name`, `description`, `requiresEnvironmentMap`, `isMatte`, `defaultAlpha`, `sequence`, `objectGuid` ) VALUES  ( 'Milky', 'Semi-translucent milky or glow-in-the-dark finish', false, false, 240, 9, 'cf100001-0001-4000-8000-000000000009' );

INSERT INTO `ColourFinish` ( `name`, `description`, `requiresEnvironmentMap`, `isMatte`, `sequence`, `objectGuid` ) VALUES  ( 'Fabric', 'Fabric or cloth material finish for flags, capes, and similar elements', false, true, 10, 'cf100001-0001-4000-8000-000000000010' );


-- Colour definitions for brick parts. Compatible with the LDraw colour standard.
CREATE TABLE `BrickColour`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`ldrawColourCode` INT NOT NULL,		-- LDraw standard colour code number
	`hexRgb` VARCHAR(10) NULL,		-- Hex RGB colour value (e.g. #FF0000)
	`hexEdgeColour` VARCHAR(10) NULL,		-- LDraw edge/contrast colour hex value for wireframe and outline rendering
	`alpha` INT NULL,		-- Alpha transparency value (0-255, 255 = fully opaque)
	`isTransparent` BIT NOT NULL DEFAULT 0,		-- Whether this colour is transparent (convenience flag derived from alpha)
	`isMetallic` BIT NOT NULL DEFAULT 0,		-- Whether this colour has a metallic finish (convenience flag)
	`colourFinishId` INT NOT NULL,		-- Material finish type — FK to ColourFinish lookup table
	`luminance` INT NULL,		-- Glow brightness (0-255) for glow-in-the-dark colours. Null for non-glowing.
	`legoColourId` INT NULL,		-- Official LEGO colour number for cross-referencing with LEGO catalogues
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`colourFinishId`) REFERENCES `ColourFinish`(`id`),		-- Foreign key to the ColourFinish table.
	UNIQUE `UC_BrickColour_ldrawColourCode_Unique`( `ldrawColourCode` ) 		-- Uniqueness enforced on the BrickColour table's ldrawColourCode field.
);
-- Index on the BrickColour table's name field.
CREATE INDEX `I_BrickColour_name` ON `BrickColour` (`name`);

-- Index on the BrickColour table's colourFinishId field.
CREATE INDEX `I_BrickColour_colourFinishId` ON `BrickColour` (`colourFinishId`);

-- Index on the BrickColour table's active field.
CREATE INDEX `I_BrickColour_active` ON `BrickColour` (`active`);

-- Index on the BrickColour table's deleted field.
CREATE INDEX `I_BrickColour_deleted` ON `BrickColour` (`deleted`);

INSERT INTO `BrickColour` ( `name`, `ldrawColourCode`, `hexRgb`, `hexEdgeColour`, `alpha`, `isTransparent`, `isMetallic`, `legoColourId`, `sequence`, `objectGuid` ) VALUES  ( 'Black', 0, '#1B2A34', '#808080', 255, false, false, 26, 1, 'c0100001-0001-4000-8000-000000000001' );

INSERT INTO `BrickColour` ( `name`, `ldrawColourCode`, `hexRgb`, `hexEdgeColour`, `alpha`, `isTransparent`, `isMetallic`, `legoColourId`, `sequence`, `objectGuid` ) VALUES  ( 'Blue', 1, '#1E5AA8', '#333333', 255, false, false, 23, 2, 'c0100001-0001-4000-8000-000000000002' );

INSERT INTO `BrickColour` ( `name`, `ldrawColourCode`, `hexRgb`, `hexEdgeColour`, `alpha`, `isTransparent`, `isMetallic`, `legoColourId`, `sequence`, `objectGuid` ) VALUES  ( 'Green', 2, '#00852B', '#333333', 255, false, false, 28, 3, 'c0100001-0001-4000-8000-000000000003' );

INSERT INTO `BrickColour` ( `name`, `ldrawColourCode`, `hexRgb`, `hexEdgeColour`, `alpha`, `isTransparent`, `isMetallic`, `legoColourId`, `sequence`, `objectGuid` ) VALUES  ( 'Red', 4, '#B40000', '#333333', 255, false, false, 21, 4, 'c0100001-0001-4000-8000-000000000004' );

INSERT INTO `BrickColour` ( `name`, `ldrawColourCode`, `hexRgb`, `hexEdgeColour`, `alpha`, `isTransparent`, `isMetallic`, `legoColourId`, `sequence`, `objectGuid` ) VALUES  ( 'Yellow', 14, '#FAC80A', '#333333', 255, false, false, 24, 5, 'c0100001-0001-4000-8000-000000000005' );

INSERT INTO `BrickColour` ( `name`, `ldrawColourCode`, `hexRgb`, `hexEdgeColour`, `alpha`, `isTransparent`, `isMetallic`, `legoColourId`, `sequence`, `objectGuid` ) VALUES  ( 'White', 15, '#F4F4F4', '#333333', 255, false, false, 1, 6, 'c0100001-0001-4000-8000-000000000006' );

INSERT INTO `BrickColour` ( `name`, `ldrawColourCode`, `hexRgb`, `hexEdgeColour`, `alpha`, `isTransparent`, `isMetallic`, `legoColourId`, `sequence`, `objectGuid` ) VALUES  ( 'Light Bluish Grey', 71, '#969696', '#333333', 255, false, false, 194, 7, 'c0100001-0001-4000-8000-000000000007' );

INSERT INTO `BrickColour` ( `name`, `ldrawColourCode`, `hexRgb`, `hexEdgeColour`, `alpha`, `isTransparent`, `isMetallic`, `legoColourId`, `sequence`, `objectGuid` ) VALUES  ( 'Dark Bluish Grey', 72, '#646464', '#333333', 255, false, false, 199, 8, 'c0100001-0001-4000-8000-000000000008' );


-- Lookup table of LDraw part classification types (Part, Subpart, Primitive, etc.).
CREATE TABLE `PartType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`isUserVisible` BIT NOT NULL DEFAULT 1,		-- Whether parts of this type should appear in the user-facing part picker
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the PartType table's name field.
CREATE INDEX `I_PartType_name` ON `PartType` (`name`);

-- Index on the PartType table's active field.
CREATE INDEX `I_PartType_active` ON `PartType` (`active`);

-- Index on the PartType table's deleted field.
CREATE INDEX `I_PartType_deleted` ON `PartType` (`deleted`);

INSERT INTO `PartType` ( `name`, `description`, `isUserVisible`, `sequence`, `objectGuid` ) VALUES  ( 'Part', 'A complete, standalone part (e.g. Brick 2x4)', true, 1, 'df6fb298-9f61-41ce-aad2-37c00bc14efd' );

INSERT INTO `PartType` ( `name`, `description`, `isUserVisible`, `sequence`, `objectGuid` ) VALUES  ( 'Subpart', 'A reusable component used internally by other parts', false, 2, '71ed658f-8695-44df-9448-669348bcfab4' );

INSERT INTO `PartType` ( `name`, `description`, `isUserVisible`, `sequence`, `objectGuid` ) VALUES  ( 'Primitive', 'A low-level geometric primitive (cylinder, stud shape)', false, 3, 'cae03dfa-930b-47e3-acd0-83241eaae69d' );

INSERT INTO `PartType` ( `name`, `description`, `isUserVisible`, `sequence`, `objectGuid` ) VALUES  ( 'Shortcut', 'A convenience combination of multiple parts (e.g. hinge assembly)', true, 4, 'a800b3c0-e7d1-46f3-830d-f2c93f7f8e4d' );

INSERT INTO `PartType` ( `name`, `description`, `isUserVisible`, `sequence`, `objectGuid` ) VALUES  ( 'Alias', 'An alternate ID that maps to another part', false, 5, '9c5c8f5c-6397-4233-b360-0292adc30304' );


-- Individual brick part definitions. Each row represents a unique part shape (independent of colour).
CREATE TABLE `BrickPart`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`ldrawPartId` VARCHAR(100) NOT NULL,		-- LDraw part ID (e.g. 3001, 32523) — the canonical identifier in the LDraw parts library
	`ldrawTitle` VARCHAR(250) NULL,		-- Raw title from the LDraw .dat file (e.g. 'Brick  2 x  4', 'Technic Beam  3')
	`ldrawCategory` VARCHAR(100) NULL,		-- Part category from LDraw !CATEGORY meta or inferred from title first word
	`partTypeId` INT NOT NULL,		-- LDraw part classification — FK to PartType lookup table
	`keywords` TEXT NULL,		-- Comma-separated keywords from LDraw !KEYWORDS meta lines for search
	`author` VARCHAR(100) NULL,		-- Part author from the LDraw Author: header line
	`brickCategoryId` INT NOT NULL,		-- The category this part belongs to
	`widthLdu` FLOAT NULL,		-- Part width in LDraw units (null if not yet computed)
	`heightLdu` FLOAT NULL,		-- Part height in LDraw units (null if not yet computed)
	`depthLdu` FLOAT NULL,		-- Part depth in LDraw units (null if not yet computed)
	`massGrams` FLOAT NULL,		-- Part mass in grams (for physics simulation, null if unknown)
	`geometryFilePath` VARCHAR(250) NULL,		-- Relative path to the LDraw .dat geometry file
	`toothCount` INT NULL,		-- For gears: number of teeth. Null for non-gear parts.
	`gearRatio` FLOAT NULL,		-- For gears: effective gear ratio relative to a base gear. Null for non-gear parts.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`partTypeId`) REFERENCES `PartType`(`id`),		-- Foreign key to the PartType table.
	FOREIGN KEY (`brickCategoryId`) REFERENCES `BrickCategory`(`id`),		-- Foreign key to the BrickCategory table.
	UNIQUE `UC_BrickPart_ldrawPartId_Unique`( `ldrawPartId` ) 		-- Uniqueness enforced on the BrickPart table's ldrawPartId field.
);
-- Index on the BrickPart table's name field.
CREATE INDEX `I_BrickPart_name` ON `BrickPart` (`name`);

-- Index on the BrickPart table's partTypeId field.
CREATE INDEX `I_BrickPart_partTypeId` ON `BrickPart` (`partTypeId`);

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
	`brickPartId` INT NOT NULL,		-- The part this connector belongs to
	`connectorTypeId` INT NOT NULL,		-- The type of connector (Stud, PinHole, AxleHole, etc.)
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
	`thumbnailImagePath` VARCHAR(250) NULL,		-- Relative path to project thumbnail image for listings
	`partCount` INT NULL,		-- Cached total part count for quick display without querying PlacedBrick
	`lastBuildDate` DATETIME NULL,		-- When the user last modified the build (placed or moved a brick)
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
	`projectId` INT NOT NULL,		-- The project this brick is placed in
	`brickPartId` INT NOT NULL,		-- The part definition being placed
	`brickColourId` INT NOT NULL,		-- The colour of this placed brick instance
	`positionX` FLOAT NULL,		-- X position in world coordinates (LDU)
	`positionY` FLOAT NULL,		-- Y position in world coordinates (LDU)
	`positionZ` FLOAT NULL,		-- Z position in world coordinates (LDU)
	`rotationX` FLOAT NULL,		-- Quaternion X component
	`rotationY` FLOAT NULL,		-- Quaternion Y component
	`rotationZ` FLOAT NULL,		-- Quaternion Z component
	`rotationW` FLOAT NULL,		-- Quaternion W component
	`buildStepNumber` INT NULL,		-- Optional build step number for instruction ordering
	`isHidden` BIT NOT NULL DEFAULT 0,		-- Whether this brick is temporarily hidden in the editor
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
	`projectId` INT NOT NULL,		-- The project this connection belongs to
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


-- A named sub-assembly within a project, similar to LDraw subfiles. Allows hierarchical builds.
CREATE TABLE `Submodel`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`projectId` INT NOT NULL,		-- The project this submodel belongs to
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`submodelId` INT NULL,		-- Optional parent submodel for nested sub-assemblies (self-referencing FK, null = top-level)
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`projectId`) REFERENCES `Project`(`id`),		-- Foreign key to the Project table.
	FOREIGN KEY (`submodelId`) REFERENCES `Submodel`(`id`),		-- Foreign key to the Submodel table.
	UNIQUE `UC_Submodel_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the Submodel table's tenantGuid and name fields.
);
-- Index on the Submodel table's tenantGuid field.
CREATE INDEX `I_Submodel_tenantGuid` ON `Submodel` (`tenantGuid`);

-- Index on the Submodel table's tenantGuid,projectId fields.
CREATE INDEX `I_Submodel_tenantGuid_projectId` ON `Submodel` (`tenantGuid`, `projectId`);

-- Index on the Submodel table's tenantGuid,name fields.
CREATE INDEX `I_Submodel_tenantGuid_name` ON `Submodel` (`tenantGuid`, `name`);

-- Index on the Submodel table's tenantGuid,submodelId fields.
CREATE INDEX `I_Submodel_tenantGuid_submodelId` ON `Submodel` (`tenantGuid`, `submodelId`);

-- Index on the Submodel table's tenantGuid,active fields.
CREATE INDEX `I_Submodel_tenantGuid_active` ON `Submodel` (`tenantGuid`, `active`);

-- Index on the Submodel table's tenantGuid,deleted fields.
CREATE INDEX `I_Submodel_tenantGuid_deleted` ON `Submodel` (`tenantGuid`, `deleted`);


-- The change history for records from the Submodel table.
CREATE TABLE `SubmodelChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`submodelId` INT NOT NULL,		-- Link to the Submodel table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`submodelId`) REFERENCES `Submodel`(`id`)		-- Foreign key to the Submodel table.
);
-- Index on the SubmodelChangeHistory table's tenantGuid field.
CREATE INDEX `I_SubmodelChangeHistory_tenantGuid` ON `SubmodelChangeHistory` (`tenantGuid`);

-- Index on the SubmodelChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_SubmodelChangeHistory_tenantGuid_versionNumber` ON `SubmodelChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the SubmodelChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_SubmodelChangeHistory_tenantGuid_timeStamp` ON `SubmodelChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the SubmodelChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_SubmodelChangeHistory_tenantGuid_userId` ON `SubmodelChangeHistory` (`tenantGuid`, `userId`);

-- Index on the SubmodelChangeHistory table's tenantGuid,submodelId fields.
CREATE INDEX `I_SubmodelChangeHistory_tenantGuid_submodelId` ON `SubmodelChangeHistory` (`tenantGuid`, `submodelId`, `versionNumber`, `timeStamp`, `userId`);


-- Maps placed bricks to the submodel they belong to. A placed brick can only belong to one submodel.
CREATE TABLE `SubmodelPlacedBrick`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`submodelId` INT NOT NULL,		-- The submodel this brick belongs to
	`placedBrickId` INT NOT NULL,		-- The placed brick assigned to this submodel
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`submodelId`) REFERENCES `Submodel`(`id`),		-- Foreign key to the Submodel table.
	FOREIGN KEY (`placedBrickId`) REFERENCES `PlacedBrick`(`id`),		-- Foreign key to the PlacedBrick table.
	UNIQUE `UC_SubmodelPlacedBrick_tenantGuid_placedBrickId_Unique`( `tenantGuid`, `placedBrickId` ) 		-- Uniqueness enforced on the SubmodelPlacedBrick table's tenantGuid and placedBrickId fields.
);
-- Index on the SubmodelPlacedBrick table's tenantGuid field.
CREATE INDEX `I_SubmodelPlacedBrick_tenantGuid` ON `SubmodelPlacedBrick` (`tenantGuid`);

-- Index on the SubmodelPlacedBrick table's tenantGuid,submodelId fields.
CREATE INDEX `I_SubmodelPlacedBrick_tenantGuid_submodelId` ON `SubmodelPlacedBrick` (`tenantGuid`, `submodelId`);

-- Index on the SubmodelPlacedBrick table's tenantGuid,placedBrickId fields.
CREATE INDEX `I_SubmodelPlacedBrick_tenantGuid_placedBrickId` ON `SubmodelPlacedBrick` (`tenantGuid`, `placedBrickId`);

-- Index on the SubmodelPlacedBrick table's tenantGuid,active fields.
CREATE INDEX `I_SubmodelPlacedBrick_tenantGuid_active` ON `SubmodelPlacedBrick` (`tenantGuid`, `active`);

-- Index on the SubmodelPlacedBrick table's tenantGuid,deleted fields.
CREATE INDEX `I_SubmodelPlacedBrick_tenantGuid_deleted` ON `SubmodelPlacedBrick` (`tenantGuid`, `deleted`);


-- User-defined tags for categorizing and filtering projects (e.g. Technic, MOC, WIP).
CREATE TABLE `ProjectTag`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE `UC_ProjectTag_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the ProjectTag table's tenantGuid and name fields.
);
-- Index on the ProjectTag table's tenantGuid field.
CREATE INDEX `I_ProjectTag_tenantGuid` ON `ProjectTag` (`tenantGuid`);

-- Index on the ProjectTag table's tenantGuid,name fields.
CREATE INDEX `I_ProjectTag_tenantGuid_name` ON `ProjectTag` (`tenantGuid`, `name`);

-- Index on the ProjectTag table's tenantGuid,active fields.
CREATE INDEX `I_ProjectTag_tenantGuid_active` ON `ProjectTag` (`tenantGuid`, `active`);

-- Index on the ProjectTag table's tenantGuid,deleted fields.
CREATE INDEX `I_ProjectTag_tenantGuid_deleted` ON `ProjectTag` (`tenantGuid`, `deleted`);


-- Many-to-many mapping between projects and tags.
CREATE TABLE `ProjectTagAssignment`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`projectId` INT NOT NULL,		-- The project being tagged
	`projectTagId` INT NOT NULL,		-- The tag applied to the project
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`projectId`) REFERENCES `Project`(`id`),		-- Foreign key to the Project table.
	FOREIGN KEY (`projectTagId`) REFERENCES `ProjectTag`(`id`),		-- Foreign key to the ProjectTag table.
	UNIQUE `UC_ProjectTagAssignment_tenantGuid_projectId_projectTagId_Unique`( `tenantGuid`, `projectId`, `projectTagId` ) 		-- Uniqueness enforced on the ProjectTagAssignment table's tenantGuid and projectId and projectTagId fields.
);
-- Index on the ProjectTagAssignment table's tenantGuid field.
CREATE INDEX `I_ProjectTagAssignment_tenantGuid` ON `ProjectTagAssignment` (`tenantGuid`);

-- Index on the ProjectTagAssignment table's tenantGuid,projectId fields.
CREATE INDEX `I_ProjectTagAssignment_tenantGuid_projectId` ON `ProjectTagAssignment` (`tenantGuid`, `projectId`);

-- Index on the ProjectTagAssignment table's tenantGuid,projectTagId fields.
CREATE INDEX `I_ProjectTagAssignment_tenantGuid_projectTagId` ON `ProjectTagAssignment` (`tenantGuid`, `projectTagId`);

-- Index on the ProjectTagAssignment table's tenantGuid,active fields.
CREATE INDEX `I_ProjectTagAssignment_tenantGuid_active` ON `ProjectTagAssignment` (`tenantGuid`, `active`);

-- Index on the ProjectTagAssignment table's tenantGuid,deleted fields.
CREATE INDEX `I_ProjectTagAssignment_tenantGuid_deleted` ON `ProjectTagAssignment` (`tenantGuid`, `deleted`);


-- Saved camera positions and orientations for quick viewport recall in the 3D editor.
CREATE TABLE `ProjectCameraPreset`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`projectId` INT NOT NULL,		-- The project this camera preset belongs to
	`name` VARCHAR(100) NOT NULL,
	`positionX` FLOAT NULL,		-- Camera X position in world coordinates (LDU)
	`positionY` FLOAT NULL,		-- Camera Y position in world coordinates (LDU)
	`positionZ` FLOAT NULL,		-- Camera Z position in world coordinates (LDU)
	`targetX` FLOAT NULL,		-- Camera target X position (look-at point)
	`targetY` FLOAT NULL,		-- Camera target Y position (look-at point)
	`targetZ` FLOAT NULL,		-- Camera target Z position (look-at point)
	`zoom` FLOAT NULL,		-- Camera zoom level / field of view
	`isPerspective` BIT NOT NULL DEFAULT 1,		-- True for perspective projection, false for orthographic
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`projectId`) REFERENCES `Project`(`id`),		-- Foreign key to the Project table.
	UNIQUE `UC_ProjectCameraPreset_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the ProjectCameraPreset table's tenantGuid and name fields.
);
-- Index on the ProjectCameraPreset table's tenantGuid field.
CREATE INDEX `I_ProjectCameraPreset_tenantGuid` ON `ProjectCameraPreset` (`tenantGuid`);

-- Index on the ProjectCameraPreset table's tenantGuid,projectId fields.
CREATE INDEX `I_ProjectCameraPreset_tenantGuid_projectId` ON `ProjectCameraPreset` (`tenantGuid`, `projectId`);

-- Index on the ProjectCameraPreset table's tenantGuid,name fields.
CREATE INDEX `I_ProjectCameraPreset_tenantGuid_name` ON `ProjectCameraPreset` (`tenantGuid`, `name`);

-- Index on the ProjectCameraPreset table's tenantGuid,active fields.
CREATE INDEX `I_ProjectCameraPreset_tenantGuid_active` ON `ProjectCameraPreset` (`tenantGuid`, `active`);

-- Index on the ProjectCameraPreset table's tenantGuid,deleted fields.
CREATE INDEX `I_ProjectCameraPreset_tenantGuid_deleted` ON `ProjectCameraPreset` (`tenantGuid`, `deleted`);


-- Reference images overlaid in the 3D editor for proportioning and tracing.
CREATE TABLE `ProjectReferenceImage`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`projectId` INT NOT NULL,		-- The project this reference image belongs to
	`name` VARCHAR(100) NOT NULL,
	`imageFilePath` VARCHAR(250) NULL,		-- Relative path to the uploaded reference image file
	`opacity` FLOAT NULL,		-- Opacity of the overlay (0.0 = invisible, 1.0 = fully opaque)
	`positionX` FLOAT NULL,		-- X position of the image plane in world coordinates (LDU)
	`positionY` FLOAT NULL,		-- Y position of the image plane in world coordinates (LDU)
	`positionZ` FLOAT NULL,		-- Z position of the image plane in world coordinates (LDU)
	`scaleX` FLOAT NULL,		-- Horizontal scale factor for the reference image
	`scaleY` FLOAT NULL,		-- Vertical scale factor for the reference image
	`isVisible` BIT NOT NULL DEFAULT 1,		-- Whether the reference image is currently visible in the editor
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`projectId`) REFERENCES `Project`(`id`),		-- Foreign key to the Project table.
	UNIQUE `UC_ProjectReferenceImage_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the ProjectReferenceImage table's tenantGuid and name fields.
);
-- Index on the ProjectReferenceImage table's tenantGuid field.
CREATE INDEX `I_ProjectReferenceImage_tenantGuid` ON `ProjectReferenceImage` (`tenantGuid`);

-- Index on the ProjectReferenceImage table's tenantGuid,projectId fields.
CREATE INDEX `I_ProjectReferenceImage_tenantGuid_projectId` ON `ProjectReferenceImage` (`tenantGuid`, `projectId`);

-- Index on the ProjectReferenceImage table's tenantGuid,name fields.
CREATE INDEX `I_ProjectReferenceImage_tenantGuid_name` ON `ProjectReferenceImage` (`tenantGuid`, `name`);

-- Index on the ProjectReferenceImage table's tenantGuid,active fields.
CREATE INDEX `I_ProjectReferenceImage_tenantGuid_active` ON `ProjectReferenceImage` (`tenantGuid`, `active`);

-- Index on the ProjectReferenceImage table's tenantGuid,deleted fields.
CREATE INDEX `I_ProjectReferenceImage_tenantGuid_deleted` ON `ProjectReferenceImage` (`tenantGuid`, `deleted`);


-- Hierarchical tree of official LEGO themes (e.g. City → Police, Technic → Bionicle). Bulk-loaded from Rebrickable or similar sources.
CREATE TABLE `LegoTheme`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`legoThemeId` INT NULL,		-- Parent theme for hierarchical nesting (self-referencing FK, null = top-level)
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`legoThemeId`) REFERENCES `LegoTheme`(`id`)		-- Foreign key to the LegoTheme table.
);
-- Index on the LegoTheme table's name field.
CREATE INDEX `I_LegoTheme_name` ON `LegoTheme` (`name`);

-- Index on the LegoTheme table's legoThemeId field.
CREATE INDEX `I_LegoTheme_legoThemeId` ON `LegoTheme` (`legoThemeId`);

-- Index on the LegoTheme table's active field.
CREATE INDEX `I_LegoTheme_active` ON `LegoTheme` (`active`);

-- Index on the LegoTheme table's deleted field.
CREATE INDEX `I_LegoTheme_deleted` ON `LegoTheme` (`deleted`);


-- Official LEGO set definitions. Each row represents a distinct set release (e.g. 42131-1 Liebherr Crawler Crane).
CREATE TABLE `LegoSet`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`setNumber` VARCHAR(100) NOT NULL,		-- Official set number including variant suffix (e.g. '42131-1', '10302-1')
	`year` INT NOT NULL,		-- Release year of the set
	`partCount` INT NOT NULL,		-- Total number of parts in the set (as listed by LEGO)
	`legoThemeId` INT NULL,		-- The theme this set belongs to (null if theme not yet categorized)
	`imageUrl` VARCHAR(250) NULL,		-- URL to the set's official box art or primary image
	`brickLinkUrl` VARCHAR(250) NULL,		-- URL to the set's BrickLink catalogue page
	`rebrickableUrl` VARCHAR(250) NULL,		-- URL to the set's Rebrickable page
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`legoThemeId`) REFERENCES `LegoTheme`(`id`),		-- Foreign key to the LegoTheme table.
	UNIQUE `UC_LegoSet_setNumber_Unique`( `setNumber` ) 		-- Uniqueness enforced on the LegoSet table's setNumber field.
);
-- Index on the LegoSet table's name field.
CREATE INDEX `I_LegoSet_name` ON `LegoSet` (`name`);

-- Index on the LegoSet table's legoThemeId field.
CREATE INDEX `I_LegoSet_legoThemeId` ON `LegoSet` (`legoThemeId`);

-- Index on the LegoSet table's active field.
CREATE INDEX `I_LegoSet_active` ON `LegoSet` (`active`);

-- Index on the LegoSet table's deleted field.
CREATE INDEX `I_LegoSet_deleted` ON `LegoSet` (`deleted`);


-- Parts inventory for each official LEGO set. Maps set → part → colour → quantity.
CREATE TABLE `LegoSetPart`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`legoSetId` INT NOT NULL,		-- The set this inventory line belongs to
	`brickPartId` INT NOT NULL,		-- The part included in this set
	`brickColourId` INT NOT NULL,		-- The colour of this part in the set
	`quantity` INT NULL,		-- Number of this part+colour combination included in the set
	`isSpare` BIT NOT NULL DEFAULT 0,		-- Whether this is a spare part (included as extra in the bag, not used in the build)
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`legoSetId`) REFERENCES `LegoSet`(`id`),		-- Foreign key to the LegoSet table.
	FOREIGN KEY (`brickPartId`) REFERENCES `BrickPart`(`id`),		-- Foreign key to the BrickPart table.
	FOREIGN KEY (`brickColourId`) REFERENCES `BrickColour`(`id`)		-- Foreign key to the BrickColour table.
);
-- Index on the LegoSetPart table's legoSetId field.
CREATE INDEX `I_LegoSetPart_legoSetId` ON `LegoSetPart` (`legoSetId`);

-- Index on the LegoSetPart table's brickPartId field.
CREATE INDEX `I_LegoSetPart_brickPartId` ON `LegoSetPart` (`brickPartId`);

-- Index on the LegoSetPart table's brickColourId field.
CREATE INDEX `I_LegoSetPart_brickColourId` ON `LegoSetPart` (`brickColourId`);

-- Index on the LegoSetPart table's active field.
CREATE INDEX `I_LegoSetPart_active` ON `LegoSetPart` (`active`);

-- Index on the LegoSetPart table's deleted field.
CREATE INDEX `I_LegoSetPart_deleted` ON `LegoSetPart` (`deleted`);


-- A user's named part collection or palette. Users can have multiple collections (e.g. 'My Inventory', 'Technic Parts', 'Parts for MOC #5').
CREATE TABLE `UserCollection`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`isDefault` BIT NOT NULL DEFAULT 0,		-- Whether this is the user's primary / default collection
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE `UC_UserCollection_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the UserCollection table's tenantGuid and name fields.
);
-- Index on the UserCollection table's tenantGuid field.
CREATE INDEX `I_UserCollection_tenantGuid` ON `UserCollection` (`tenantGuid`);

-- Index on the UserCollection table's tenantGuid,name fields.
CREATE INDEX `I_UserCollection_tenantGuid_name` ON `UserCollection` (`tenantGuid`, `name`);

-- Index on the UserCollection table's tenantGuid,active fields.
CREATE INDEX `I_UserCollection_tenantGuid_active` ON `UserCollection` (`tenantGuid`, `active`);

-- Index on the UserCollection table's tenantGuid,deleted fields.
CREATE INDEX `I_UserCollection_tenantGuid_deleted` ON `UserCollection` (`tenantGuid`, `deleted`);


-- The change history for records from the UserCollection table.
CREATE TABLE `UserCollectionChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`userCollectionId` INT NOT NULL,		-- Link to the UserCollection table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`userCollectionId`) REFERENCES `UserCollection`(`id`)		-- Foreign key to the UserCollection table.
);
-- Index on the UserCollectionChangeHistory table's tenantGuid field.
CREATE INDEX `I_UserCollectionChangeHistory_tenantGuid` ON `UserCollectionChangeHistory` (`tenantGuid`);

-- Index on the UserCollectionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_UserCollectionChangeHistory_tenantGuid_versionNumber` ON `UserCollectionChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the UserCollectionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_UserCollectionChangeHistory_tenantGuid_timeStamp` ON `UserCollectionChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the UserCollectionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_UserCollectionChangeHistory_tenantGuid_userId` ON `UserCollectionChangeHistory` (`tenantGuid`, `userId`);

-- Index on the UserCollectionChangeHistory table's tenantGuid,userCollectionId fields.
CREATE INDEX `I_UserCollectionChangeHistory_tenantGuid_userCollectionId` ON `UserCollectionChangeHistory` (`tenantGuid`, `userCollectionId`, `versionNumber`, `timeStamp`, `userId`);


-- Individual part+colour entries within a user collection, with quantity owned and quantity currently allocated to projects.
CREATE TABLE `UserCollectionPart`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`userCollectionId` INT NOT NULL,		-- The collection this part entry belongs to
	`brickPartId` INT NOT NULL,		-- The part definition
	`brickColourId` INT NOT NULL,		-- The specific colour of this part
	`quantityOwned` INT NULL,		-- Total number of this part+colour the user owns
	`quantityUsed` INT NULL,		-- Number currently allocated to projects (for build-with-what-you-own filtering)
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`userCollectionId`) REFERENCES `UserCollection`(`id`),		-- Foreign key to the UserCollection table.
	FOREIGN KEY (`brickPartId`) REFERENCES `BrickPart`(`id`),		-- Foreign key to the BrickPart table.
	FOREIGN KEY (`brickColourId`) REFERENCES `BrickColour`(`id`),		-- Foreign key to the BrickColour table.
	UNIQUE `UC_UserCollectionPart_tenantGuid_userCollectionId_brickPartId_brickColourId_Unique`( `tenantGuid`, `userCollectionId`, `brickPartId`, `brickColourId` ) 		-- Uniqueness enforced on the UserCollectionPart table's tenantGuid and userCollectionId and brickPartId and brickColourId fields.
);
-- Index on the UserCollectionPart table's tenantGuid field.
CREATE INDEX `I_UserCollectionPart_tenantGuid` ON `UserCollectionPart` (`tenantGuid`);

-- Index on the UserCollectionPart table's tenantGuid,userCollectionId fields.
CREATE INDEX `I_UserCollectionPart_tenantGuid_userCollectionId` ON `UserCollectionPart` (`tenantGuid`, `userCollectionId`);

-- Index on the UserCollectionPart table's tenantGuid,brickPartId fields.
CREATE INDEX `I_UserCollectionPart_tenantGuid_brickPartId` ON `UserCollectionPart` (`tenantGuid`, `brickPartId`);

-- Index on the UserCollectionPart table's tenantGuid,brickColourId fields.
CREATE INDEX `I_UserCollectionPart_tenantGuid_brickColourId` ON `UserCollectionPart` (`tenantGuid`, `brickColourId`);

-- Index on the UserCollectionPart table's tenantGuid,active fields.
CREATE INDEX `I_UserCollectionPart_tenantGuid_active` ON `UserCollectionPart` (`tenantGuid`, `active`);

-- Index on the UserCollectionPart table's tenantGuid,deleted fields.
CREATE INDEX `I_UserCollectionPart_tenantGuid_deleted` ON `UserCollectionPart` (`tenantGuid`, `deleted`);


-- Parts the user wants to acquire. Can optionally specify a colour or leave null for any colour.
CREATE TABLE `UserWishlistItem`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`userCollectionId` INT NOT NULL,		-- The collection this wishlist item is associated with
	`brickPartId` INT NOT NULL,		-- The desired part
	`brickColourId` INT NULL,		-- The desired colour (null = any colour)
	`quantityDesired` INT NULL,		-- Number of this part the user wants to acquire
	`notes` TEXT NULL,		-- Free-form notes about the wishlist item (e.g. source, priority)
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`userCollectionId`) REFERENCES `UserCollection`(`id`),		-- Foreign key to the UserCollection table.
	FOREIGN KEY (`brickPartId`) REFERENCES `BrickPart`(`id`),		-- Foreign key to the BrickPart table.
	FOREIGN KEY (`brickColourId`) REFERENCES `BrickColour`(`id`)		-- Foreign key to the BrickColour table.
);
-- Index on the UserWishlistItem table's tenantGuid field.
CREATE INDEX `I_UserWishlistItem_tenantGuid` ON `UserWishlistItem` (`tenantGuid`);

-- Index on the UserWishlistItem table's tenantGuid,userCollectionId fields.
CREATE INDEX `I_UserWishlistItem_tenantGuid_userCollectionId` ON `UserWishlistItem` (`tenantGuid`, `userCollectionId`);

-- Index on the UserWishlistItem table's tenantGuid,brickPartId fields.
CREATE INDEX `I_UserWishlistItem_tenantGuid_brickPartId` ON `UserWishlistItem` (`tenantGuid`, `brickPartId`);

-- Index on the UserWishlistItem table's tenantGuid,brickColourId fields.
CREATE INDEX `I_UserWishlistItem_tenantGuid_brickColourId` ON `UserWishlistItem` (`tenantGuid`, `brickColourId`);

-- Index on the UserWishlistItem table's tenantGuid,active fields.
CREATE INDEX `I_UserWishlistItem_tenantGuid_active` ON `UserWishlistItem` (`tenantGuid`, `active`);

-- Index on the UserWishlistItem table's tenantGuid,deleted fields.
CREATE INDEX `I_UserWishlistItem_tenantGuid_deleted` ON `UserWishlistItem` (`tenantGuid`, `deleted`);


-- Tracks which official LEGO sets have been imported into a user's collection, with quantity (e.g. 2 copies of set 42131).
CREATE TABLE `UserCollectionSetImport`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`userCollectionId` INT NOT NULL,		-- The collection the set was imported into
	`legoSetId` INT NOT NULL,		-- The set that was imported
	`quantity` INT NULL,		-- Number of copies of this set imported (e.g. user owns 2 copies)
	`importedDate` DATETIME NULL,		-- Date/time the set was imported into the collection
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`userCollectionId`) REFERENCES `UserCollection`(`id`),		-- Foreign key to the UserCollection table.
	FOREIGN KEY (`legoSetId`) REFERENCES `LegoSet`(`id`),		-- Foreign key to the LegoSet table.
	UNIQUE `UC_UserCollectionSetImport_tenantGuid_userCollectionId_legoSetId_Unique`( `tenantGuid`, `userCollectionId`, `legoSetId` ) 		-- Uniqueness enforced on the UserCollectionSetImport table's tenantGuid and userCollectionId and legoSetId fields.
);
-- Index on the UserCollectionSetImport table's tenantGuid field.
CREATE INDEX `I_UserCollectionSetImport_tenantGuid` ON `UserCollectionSetImport` (`tenantGuid`);

-- Index on the UserCollectionSetImport table's tenantGuid,userCollectionId fields.
CREATE INDEX `I_UserCollectionSetImport_tenantGuid_userCollectionId` ON `UserCollectionSetImport` (`tenantGuid`, `userCollectionId`);

-- Index on the UserCollectionSetImport table's tenantGuid,legoSetId fields.
CREATE INDEX `I_UserCollectionSetImport_tenantGuid_legoSetId` ON `UserCollectionSetImport` (`tenantGuid`, `legoSetId`);

-- Index on the UserCollectionSetImport table's tenantGuid,active fields.
CREATE INDEX `I_UserCollectionSetImport_tenantGuid_active` ON `UserCollectionSetImport` (`tenantGuid`, `active`);

-- Index on the UserCollectionSetImport table's tenantGuid,deleted fields.
CREATE INDEX `I_UserCollectionSetImport_tenantGuid_deleted` ON `UserCollectionSetImport` (`tenantGuid`, `deleted`);


-- A complete instruction booklet for a building project. A project can have multiple manuals (e.g. one per bag/booklet).
CREATE TABLE `BuildManual`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`projectId` INT NOT NULL,		-- The project this manual documents
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`pageWidthMm` FLOAT NULL,		-- Page width in millimetres for PDF/print layout (e.g. 210 for A4)
	`pageHeightMm` FLOAT NULL,		-- Page height in millimetres for PDF/print layout (e.g. 297 for A4)
	`isPublished` BIT NOT NULL DEFAULT 0,		-- Whether this manual is marked as published/final
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`projectId`) REFERENCES `Project`(`id`),		-- Foreign key to the Project table.
	UNIQUE `UC_BuildManual_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the BuildManual table's tenantGuid and name fields.
);
-- Index on the BuildManual table's tenantGuid field.
CREATE INDEX `I_BuildManual_tenantGuid` ON `BuildManual` (`tenantGuid`);

-- Index on the BuildManual table's tenantGuid,projectId fields.
CREATE INDEX `I_BuildManual_tenantGuid_projectId` ON `BuildManual` (`tenantGuid`, `projectId`);

-- Index on the BuildManual table's tenantGuid,name fields.
CREATE INDEX `I_BuildManual_tenantGuid_name` ON `BuildManual` (`tenantGuid`, `name`);

-- Index on the BuildManual table's tenantGuid,active fields.
CREATE INDEX `I_BuildManual_tenantGuid_active` ON `BuildManual` (`tenantGuid`, `active`);

-- Index on the BuildManual table's tenantGuid,deleted fields.
CREATE INDEX `I_BuildManual_tenantGuid_deleted` ON `BuildManual` (`tenantGuid`, `deleted`);


-- The change history for records from the BuildManual table.
CREATE TABLE `BuildManualChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`buildManualId` INT NOT NULL,		-- Link to the BuildManual table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`buildManualId`) REFERENCES `BuildManual`(`id`)		-- Foreign key to the BuildManual table.
);
-- Index on the BuildManualChangeHistory table's tenantGuid field.
CREATE INDEX `I_BuildManualChangeHistory_tenantGuid` ON `BuildManualChangeHistory` (`tenantGuid`);

-- Index on the BuildManualChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX `I_BuildManualChangeHistory_tenantGuid_versionNumber` ON `BuildManualChangeHistory` (`tenantGuid`, `versionNumber`);

-- Index on the BuildManualChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX `I_BuildManualChangeHistory_tenantGuid_timeStamp` ON `BuildManualChangeHistory` (`tenantGuid`, `timeStamp`);

-- Index on the BuildManualChangeHistory table's tenantGuid,userId fields.
CREATE INDEX `I_BuildManualChangeHistory_tenantGuid_userId` ON `BuildManualChangeHistory` (`tenantGuid`, `userId`);

-- Index on the BuildManualChangeHistory table's tenantGuid,buildManualId fields.
CREATE INDEX `I_BuildManualChangeHistory_tenantGuid_buildManualId` ON `BuildManualChangeHistory` (`tenantGuid`, `buildManualId`, `versionNumber`, `timeStamp`, `userId`);


-- A single page within a build manual. Contains one or more build steps.
CREATE TABLE `BuildManualPage`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`buildManualId` INT NOT NULL,		-- The manual this page belongs to
	`pageNumber` INT NULL,		-- Sequential page number within the manual
	`title` VARCHAR(250) NULL,		-- Optional page title (e.g. 'Bag 1', 'Chassis Assembly')
	`notes` TEXT NULL,		-- Optional internal notes about this page
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`buildManualId`) REFERENCES `BuildManual`(`id`)		-- Foreign key to the BuildManual table.
);
-- Index on the BuildManualPage table's tenantGuid field.
CREATE INDEX `I_BuildManualPage_tenantGuid` ON `BuildManualPage` (`tenantGuid`);

-- Index on the BuildManualPage table's tenantGuid,buildManualId fields.
CREATE INDEX `I_BuildManualPage_tenantGuid_buildManualId` ON `BuildManualPage` (`tenantGuid`, `buildManualId`);

-- Index on the BuildManualPage table's tenantGuid,active fields.
CREATE INDEX `I_BuildManualPage_tenantGuid_active` ON `BuildManualPage` (`tenantGuid`, `active`);

-- Index on the BuildManualPage table's tenantGuid,deleted fields.
CREATE INDEX `I_BuildManualPage_tenantGuid_deleted` ON `BuildManualPage` (`tenantGuid`, `deleted`);


-- A single build step within a manual page. Defines the camera angle and display options for that step's rendered view.
CREATE TABLE `BuildManualStep`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`buildManualPageId` INT NOT NULL,		-- The page this step appears on
	`stepNumber` INT NULL,		-- Sequential step number within the page
	`cameraPositionX` FLOAT NULL,		-- Camera X position for this step's rendered view
	`cameraPositionY` FLOAT NULL,		-- Camera Y position for this step's rendered view
	`cameraPositionZ` FLOAT NULL,		-- Camera Z position for this step's rendered view
	`cameraTargetX` FLOAT NULL,		-- Camera look-at target X for this step
	`cameraTargetY` FLOAT NULL,		-- Camera look-at target Y for this step
	`cameraTargetZ` FLOAT NULL,		-- Camera look-at target Z for this step
	`cameraZoom` FLOAT NULL,		-- Camera zoom / field of view for this step
	`showExplodedView` BIT NOT NULL DEFAULT 0,		-- Whether to render the step with newly-added parts pulled apart for clarity
	`explodedDistance` FLOAT NULL,		-- Distance in LDU to pull apart exploded parts (null = use default)
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`buildManualPageId`) REFERENCES `BuildManualPage`(`id`)		-- Foreign key to the BuildManualPage table.
);
-- Index on the BuildManualStep table's tenantGuid field.
CREATE INDEX `I_BuildManualStep_tenantGuid` ON `BuildManualStep` (`tenantGuid`);

-- Index on the BuildManualStep table's tenantGuid,buildManualPageId fields.
CREATE INDEX `I_BuildManualStep_tenantGuid_buildManualPageId` ON `BuildManualStep` (`tenantGuid`, `buildManualPageId`);

-- Index on the BuildManualStep table's tenantGuid,active fields.
CREATE INDEX `I_BuildManualStep_tenantGuid_active` ON `BuildManualStep` (`tenantGuid`, `active`);

-- Index on the BuildManualStep table's tenantGuid,deleted fields.
CREATE INDEX `I_BuildManualStep_tenantGuid_deleted` ON `BuildManualStep` (`tenantGuid`, `deleted`);


-- Maps which placed bricks are added during a specific build step. Links to the actual PlacedBrick in the project.
CREATE TABLE `BuildStepPart`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`buildManualStepId` INT NOT NULL,		-- The build step this part is added during
	`placedBrickId` INT NOT NULL,		-- The placed brick in the project that is added in this step
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`buildManualStepId`) REFERENCES `BuildManualStep`(`id`),		-- Foreign key to the BuildManualStep table.
	FOREIGN KEY (`placedBrickId`) REFERENCES `PlacedBrick`(`id`)		-- Foreign key to the PlacedBrick table.
);
-- Index on the BuildStepPart table's tenantGuid field.
CREATE INDEX `I_BuildStepPart_tenantGuid` ON `BuildStepPart` (`tenantGuid`);

-- Index on the BuildStepPart table's tenantGuid,buildManualStepId fields.
CREATE INDEX `I_BuildStepPart_tenantGuid_buildManualStepId` ON `BuildStepPart` (`tenantGuid`, `buildManualStepId`);

-- Index on the BuildStepPart table's tenantGuid,placedBrickId fields.
CREATE INDEX `I_BuildStepPart_tenantGuid_placedBrickId` ON `BuildStepPart` (`tenantGuid`, `placedBrickId`);

-- Index on the BuildStepPart table's tenantGuid,active fields.
CREATE INDEX `I_BuildStepPart_tenantGuid_active` ON `BuildStepPart` (`tenantGuid`, `active`);

-- Index on the BuildStepPart table's tenantGuid,deleted fields.
CREATE INDEX `I_BuildStepPart_tenantGuid_deleted` ON `BuildStepPart` (`tenantGuid`, `deleted`);


-- Lookup table of annotation types available for build steps (Arrow, Callout, Label, Quantity Callout, Submodel Callout).
CREATE TABLE `BuildStepAnnotationType`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the BuildStepAnnotationType table's name field.
CREATE INDEX `I_BuildStepAnnotationType_name` ON `BuildStepAnnotationType` (`name`);

-- Index on the BuildStepAnnotationType table's active field.
CREATE INDEX `I_BuildStepAnnotationType_active` ON `BuildStepAnnotationType` (`active`);

-- Index on the BuildStepAnnotationType table's deleted field.
CREATE INDEX `I_BuildStepAnnotationType_deleted` ON `BuildStepAnnotationType` (`deleted`);

INSERT INTO `BuildStepAnnotationType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Arrow', 'Directional arrow indicating placement direction or connection point', 1, 'ba100001-0001-4000-8000-000000000001' );

INSERT INTO `BuildStepAnnotationType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Callout', 'Callout box highlighting a sub-assembly built separately', 2, 'ba100001-0001-4000-8000-000000000002' );

INSERT INTO `BuildStepAnnotationType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Label', 'Text label providing additional context or instruction', 3, 'ba100001-0001-4000-8000-000000000003' );

INSERT INTO `BuildStepAnnotationType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Quantity Callout', 'Quantity indicator showing how many of a part are needed in this step', 4, 'ba100001-0001-4000-8000-000000000004' );

INSERT INTO `BuildStepAnnotationType` ( `name`, `description`, `sequence`, `objectGuid` ) VALUES  ( 'Submodel Callout', 'Callout referencing a submodel that should be built as part of this step', 5, 'ba100001-0001-4000-8000-000000000005' );


-- Visual annotations (arrows, callouts, labels) placed on a build step's rendered view.
CREATE TABLE `BuildStepAnnotation`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`buildManualStepId` INT NOT NULL,		-- The build step this annotation belongs to
	`buildStepAnnotationTypeId` INT NOT NULL,		-- The type of annotation (Arrow, Callout, Label, etc.)
	`positionX` FLOAT NULL,		-- X position on the rendered page (normalised 0-1 or pixel coordinates)
	`positionY` FLOAT NULL,		-- Y position on the rendered page
	`width` FLOAT NULL,		-- Width of the annotation element (null = auto-size)
	`height` FLOAT NULL,		-- Height of the annotation element (null = auto-size)
	`text` TEXT NULL,		-- Optional text content for labels and callouts
	`placedBrickId` INT NULL,		-- Optional target placed brick that this annotation points to or highlights
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`buildManualStepId`) REFERENCES `BuildManualStep`(`id`),		-- Foreign key to the BuildManualStep table.
	FOREIGN KEY (`buildStepAnnotationTypeId`) REFERENCES `BuildStepAnnotationType`(`id`),		-- Foreign key to the BuildStepAnnotationType table.
	FOREIGN KEY (`placedBrickId`) REFERENCES `PlacedBrick`(`id`)		-- Foreign key to the PlacedBrick table.
);
-- Index on the BuildStepAnnotation table's tenantGuid field.
CREATE INDEX `I_BuildStepAnnotation_tenantGuid` ON `BuildStepAnnotation` (`tenantGuid`);

-- Index on the BuildStepAnnotation table's tenantGuid,buildManualStepId fields.
CREATE INDEX `I_BuildStepAnnotation_tenantGuid_buildManualStepId` ON `BuildStepAnnotation` (`tenantGuid`, `buildManualStepId`);

-- Index on the BuildStepAnnotation table's tenantGuid,buildStepAnnotationTypeId fields.
CREATE INDEX `I_BuildStepAnnotation_tenantGuid_buildStepAnnotationTypeId` ON `BuildStepAnnotation` (`tenantGuid`, `buildStepAnnotationTypeId`);

-- Index on the BuildStepAnnotation table's tenantGuid,placedBrickId fields.
CREATE INDEX `I_BuildStepAnnotation_tenantGuid_placedBrickId` ON `BuildStepAnnotation` (`tenantGuid`, `placedBrickId`);

-- Index on the BuildStepAnnotation table's tenantGuid,active fields.
CREATE INDEX `I_BuildStepAnnotation_tenantGuid_active` ON `BuildStepAnnotation` (`tenantGuid`, `active`);

-- Index on the BuildStepAnnotation table's tenantGuid,deleted fields.
CREATE INDEX `I_BuildStepAnnotation_tenantGuid_deleted` ON `BuildStepAnnotation` (`tenantGuid`, `deleted`);


-- Reusable rendering presets that define resolution, lighting, and quality settings for producing images of models.
CREATE TABLE `RenderPreset`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`name` VARCHAR(100) NOT NULL,
	`description` VARCHAR(500) NOT NULL,
	`resolutionWidth` INT NULL,		-- Output image width in pixels
	`resolutionHeight` INT NULL,		-- Output image height in pixels
	`backgroundColorHex` VARCHAR(10) NULL,		-- Background colour in hex (e.g. #FFFFFF for white, #000000 for black)
	`enableShadows` BIT NOT NULL DEFAULT 1,		-- Whether to render drop shadows
	`enableReflections` BIT NOT NULL DEFAULT 0,		-- Whether to render environment reflections on metallic/chrome parts
	`lightingMode` VARCHAR(100) NULL,		-- Lighting preset name: studio, outdoor, dramatic, custom
	`antiAliasLevel` INT NULL,		-- Anti-aliasing level (1=none, 2=2x, 4=4x, 8=8x)
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE `UC_RenderPreset_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the RenderPreset table's tenantGuid and name fields.
);
-- Index on the RenderPreset table's tenantGuid field.
CREATE INDEX `I_RenderPreset_tenantGuid` ON `RenderPreset` (`tenantGuid`);

-- Index on the RenderPreset table's tenantGuid,name fields.
CREATE INDEX `I_RenderPreset_tenantGuid_name` ON `RenderPreset` (`tenantGuid`, `name`);

-- Index on the RenderPreset table's tenantGuid,active fields.
CREATE INDEX `I_RenderPreset_tenantGuid_active` ON `RenderPreset` (`tenantGuid`, `active`);

-- Index on the RenderPreset table's tenantGuid,deleted fields.
CREATE INDEX `I_RenderPreset_tenantGuid_deleted` ON `RenderPreset` (`tenantGuid`, `deleted`);


-- Records of rendered images produced from a project, with the preset used and output metadata.
CREATE TABLE `ProjectRender`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`projectId` INT NOT NULL,		-- The project this render was produced from
	`renderPresetId` INT NULL,		-- The render preset used (null = custom/one-off settings)
	`name` VARCHAR(100) NOT NULL,
	`outputFilePath` VARCHAR(250) NULL,		-- Relative path to the rendered image file
	`resolutionWidth` INT NULL,		-- Actual output width in pixels
	`resolutionHeight` INT NULL,		-- Actual output height in pixels
	`renderedDate` DATETIME NULL,		-- Date/time the render was produced
	`renderDurationSeconds` FLOAT NULL,		-- Time taken to produce the render in seconds
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`projectId`) REFERENCES `Project`(`id`),		-- Foreign key to the Project table.
	FOREIGN KEY (`renderPresetId`) REFERENCES `RenderPreset`(`id`),		-- Foreign key to the RenderPreset table.
	UNIQUE `UC_ProjectRender_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the ProjectRender table's tenantGuid and name fields.
);
-- Index on the ProjectRender table's tenantGuid field.
CREATE INDEX `I_ProjectRender_tenantGuid` ON `ProjectRender` (`tenantGuid`);

-- Index on the ProjectRender table's tenantGuid,projectId fields.
CREATE INDEX `I_ProjectRender_tenantGuid_projectId` ON `ProjectRender` (`tenantGuid`, `projectId`);

-- Index on the ProjectRender table's tenantGuid,renderPresetId fields.
CREATE INDEX `I_ProjectRender_tenantGuid_renderPresetId` ON `ProjectRender` (`tenantGuid`, `renderPresetId`);

-- Index on the ProjectRender table's tenantGuid,name fields.
CREATE INDEX `I_ProjectRender_tenantGuid_name` ON `ProjectRender` (`tenantGuid`, `name`);

-- Index on the ProjectRender table's tenantGuid,active fields.
CREATE INDEX `I_ProjectRender_tenantGuid_active` ON `ProjectRender` (`tenantGuid`, `active`);

-- Index on the ProjectRender table's tenantGuid,deleted fields.
CREATE INDEX `I_ProjectRender_tenantGuid_deleted` ON `ProjectRender` (`tenantGuid`, `deleted`);


-- Lookup table of supported file export formats for models, instructions, and parts lists.
CREATE TABLE `ExportFormat`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NOT NULL,
	`fileExtension` VARCHAR(50) NULL,		-- File extension including dot (e.g. '.ldr', '.pdf', '.xml')
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the ExportFormat table's name field.
CREATE INDEX `I_ExportFormat_name` ON `ExportFormat` (`name`);

-- Index on the ExportFormat table's active field.
CREATE INDEX `I_ExportFormat_active` ON `ExportFormat` (`active`);

-- Index on the ExportFormat table's deleted field.
CREATE INDEX `I_ExportFormat_deleted` ON `ExportFormat` (`deleted`);

INSERT INTO `ExportFormat` ( `name`, `description`, `fileExtension`, `sequence`, `objectGuid` ) VALUES  ( 'LDraw', 'LDraw single-model file format', '.ldr', 1, 'ef100001-0001-4000-8000-000000000001' );

INSERT INTO `ExportFormat` ( `name`, `description`, `fileExtension`, `sequence`, `objectGuid` ) VALUES  ( 'LDraw Multi-Part', 'LDraw multi-part document containing submodels', '.mpd', 2, 'ef100001-0001-4000-8000-000000000002' );

INSERT INTO `ExportFormat` ( `name`, `description`, `fileExtension`, `sequence`, `objectGuid` ) VALUES  ( 'Wavefront OBJ', 'Wavefront OBJ 3D model format for Blender and other 3D tools', '.obj', 3, 'ef100001-0001-4000-8000-000000000003' );

INSERT INTO `ExportFormat` ( `name`, `description`, `fileExtension`, `sequence`, `objectGuid` ) VALUES  ( 'COLLADA', 'COLLADA 3D asset exchange format', '.dae', 4, 'ef100001-0001-4000-8000-000000000004' );

INSERT INTO `ExportFormat` ( `name`, `description`, `fileExtension`, `sequence`, `objectGuid` ) VALUES  ( 'BrickLink XML', 'BrickLink wanted-list XML format for ordering parts', '.xml', 5, 'ef100001-0001-4000-8000-000000000005' );

INSERT INTO `ExportFormat` ( `name`, `description`, `fileExtension`, `sequence`, `objectGuid` ) VALUES  ( 'Rebrickable CSV', 'Rebrickable-compatible CSV parts list', '.csv', 6, 'ef100001-0001-4000-8000-000000000006' );

INSERT INTO `ExportFormat` ( `name`, `description`, `fileExtension`, `sequence`, `objectGuid` ) VALUES  ( 'PDF Instructions', 'PDF export of build manual instructions', '.pdf', 7, 'ef100001-0001-4000-8000-000000000007' );


-- Log of exports produced from a project, tracking what was exported and when.
CREATE TABLE `ProjectExport`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`tenantGuid` CHAR(38) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	`projectId` INT NOT NULL,		-- The project this export was produced from
	`exportFormatId` INT NOT NULL,		-- The format used for the export
	`name` VARCHAR(100) NOT NULL,
	`outputFilePath` VARCHAR(250) NULL,		-- Relative path to the exported file
	`exportedDate` DATETIME NULL,		-- Date/time the export was produced
	`includeInstructions` BIT NOT NULL DEFAULT 0,		-- Whether build instructions were included in the export
	`includePartsList` BIT NOT NULL DEFAULT 0,		-- Whether a bill of materials / parts list was included in the export
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`projectId`) REFERENCES `Project`(`id`),		-- Foreign key to the Project table.
	FOREIGN KEY (`exportFormatId`) REFERENCES `ExportFormat`(`id`),		-- Foreign key to the ExportFormat table.
	UNIQUE `UC_ProjectExport_tenantGuid_name_Unique`( `tenantGuid`, `name` ) 		-- Uniqueness enforced on the ProjectExport table's tenantGuid and name fields.
);
-- Index on the ProjectExport table's tenantGuid field.
CREATE INDEX `I_ProjectExport_tenantGuid` ON `ProjectExport` (`tenantGuid`);

-- Index on the ProjectExport table's tenantGuid,projectId fields.
CREATE INDEX `I_ProjectExport_tenantGuid_projectId` ON `ProjectExport` (`tenantGuid`, `projectId`);

-- Index on the ProjectExport table's tenantGuid,exportFormatId fields.
CREATE INDEX `I_ProjectExport_tenantGuid_exportFormatId` ON `ProjectExport` (`tenantGuid`, `exportFormatId`);

-- Index on the ProjectExport table's tenantGuid,name fields.
CREATE INDEX `I_ProjectExport_tenantGuid_name` ON `ProjectExport` (`tenantGuid`, `name`);

-- Index on the ProjectExport table's tenantGuid,active fields.
CREATE INDEX `I_ProjectExport_tenantGuid_active` ON `ProjectExport` (`tenantGuid`, `active`);

-- Index on the ProjectExport table's tenantGuid,deleted fields.
CREATE INDEX `I_ProjectExport_tenantGuid_deleted` ON `ProjectExport` (`tenantGuid`, `deleted`);


