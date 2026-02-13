/*
BMC (Brick Machine Construction) database schema.
This is a multi-tenant interactive building and physics simulation platform for LEGO and LEGO Technic models.
It supports a parts catalog with connector metadata, 3D model building with placement and connection tracking,
and physics simulation of mechanical assemblies.
All operational tables include multi-tenant support, versioning where appropriate, auditing, and security controls.
*/
CREATE DATABASE [BMC]
GO

ALTER DATABASE [BMC] SET RECOVERY SIMPLE
GO

USE [BMC]
GO

CREATE SCHEMA [BMC]
GO

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE [BMC].[BrickConnection]
-- DROP TABLE [BMC].[PlacedBrickChangeHistory]
-- DROP TABLE [BMC].[PlacedBrick]
-- DROP TABLE [BMC].[ProjectChangeHistory]
-- DROP TABLE [BMC].[Project]
-- DROP TABLE [BMC].[BrickPartColour]
-- DROP TABLE [BMC].[BrickPartConnector]
-- DROP TABLE [BMC].[BrickPartChangeHistory]
-- DROP TABLE [BMC].[BrickPart]
-- DROP TABLE [BMC].[BrickColour]
-- DROP TABLE [BMC].[ConnectorType]
-- DROP TABLE [BMC].[BrickCategory]

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON [BMC].[BrickConnection] DISABLE
-- ALTER INDEX ALL ON [BMC].[PlacedBrickChangeHistory] DISABLE
-- ALTER INDEX ALL ON [BMC].[PlacedBrick] DISABLE
-- ALTER INDEX ALL ON [BMC].[ProjectChangeHistory] DISABLE
-- ALTER INDEX ALL ON [BMC].[Project] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickPartColour] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickPartConnector] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickPartChangeHistory] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickPart] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickColour] DISABLE
-- ALTER INDEX ALL ON [BMC].[ConnectorType] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickCategory] DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON [BMC].[BrickConnection] REBUILD
-- ALTER INDEX ALL ON [BMC].[PlacedBrickChangeHistory] REBUILD
-- ALTER INDEX ALL ON [BMC].[PlacedBrick] REBUILD
-- ALTER INDEX ALL ON [BMC].[ProjectChangeHistory] REBUILD
-- ALTER INDEX ALL ON [BMC].[Project] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickPartColour] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickPartConnector] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickPartChangeHistory] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickPart] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickColour] REBUILD
-- ALTER INDEX ALL ON [BMC].[ConnectorType] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickCategory] REBUILD

-- Master list of part categories (Beam, Plate, Gear, Axle, Motor, Pneumatic, etc.)
CREATE TABLE [BMC].[BrickCategory]
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

-- Index on the BrickCategory table's name field.
CREATE INDEX [I_BrickCategory_name] ON [BMC].[BrickCategory] ([name])
GO

-- Index on the BrickCategory table's active field.
CREATE INDEX [I_BrickCategory_active] ON [BMC].[BrickCategory] ([active])
GO

-- Index on the BrickCategory table's deleted field.
CREATE INDEX [I_BrickCategory_deleted] ON [BMC].[BrickCategory] ([deleted])
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Plate', 'Standard plates of various sizes', 1, 'b1c10001-0001-4000-8000-000000000001' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Brick', 'Standard bricks of various sizes', 2, 'b1c10001-0001-4000-8000-000000000002' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Beam', 'Technic beams and liftarms', 10, 'b1c10001-0001-4000-8000-000000000010' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Pin', 'Technic pins and connectors', 11, 'b1c10001-0001-4000-8000-000000000011' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Axle', 'Technic axles of various lengths', 12, 'b1c10001-0001-4000-8000-000000000012' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Gear', 'Technic gears of various tooth counts', 13, 'b1c10001-0001-4000-8000-000000000013' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Motor', 'Powered motors (Power Functions, Powered Up)', 14, 'b1c10001-0001-4000-8000-000000000014' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Pneumatic', 'Pneumatic cylinders, pumps, and tubing', 15, 'b1c10001-0001-4000-8000-000000000015' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Differential', 'Differential gear assemblies', 16, 'b1c10001-0001-4000-8000-000000000016' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Panel', 'Panels, fairings, and body pieces', 20, 'b1c10001-0001-4000-8000-000000000020' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Wheel', 'Wheels, tyres, and rims', 21, 'b1c10001-0001-4000-8000-000000000021' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Decorative', 'Decorative, printed, and sticker parts', 30, 'b1c10001-0001-4000-8000-000000000030' )
GO


-- Master list of physical connection types that define how parts can join together.
CREATE TABLE [BMC].[ConnectorType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[degreesOfFreedom] INT NULL,		-- Number of degrees of freedom when connected (0=fixed, 1=rotation, 2=rotation+slide)
	[allowsRotation] BIT NOT NULL DEFAULT 0,		-- Whether this connection allows rotation around its axis
	[allowsSlide] BIT NOT NULL DEFAULT 0,		-- Whether this connection allows sliding along its axis
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the ConnectorType table's name field.
CREATE INDEX [I_ConnectorType_name] ON [BMC].[ConnectorType] ([name])
GO

-- Index on the ConnectorType table's active field.
CREATE INDEX [I_ConnectorType_active] ON [BMC].[ConnectorType] ([active])
GO

-- Index on the ConnectorType table's deleted field.
CREATE INDEX [I_ConnectorType_deleted] ON [BMC].[ConnectorType] ([deleted])
GO

INSERT INTO [BMC].[ConnectorType] ( [name], [description], [degreesOfFreedom], [allowsRotation], [allowsSlide], [sequence], [objectGuid] ) VALUES  ( 'Stud', 'Standard LEGO stud (male connector)', 0, 0, 0, 1, 'c0110001-0001-4000-8000-000000000001' )
GO

INSERT INTO [BMC].[ConnectorType] ( [name], [description], [degreesOfFreedom], [allowsRotation], [allowsSlide], [sequence], [objectGuid] ) VALUES  ( 'AntiStud', 'Standard LEGO anti-stud receptacle (female connector)', 0, 0, 0, 2, 'c0110001-0001-4000-8000-000000000002' )
GO

INSERT INTO [BMC].[ConnectorType] ( [name], [description], [degreesOfFreedom], [allowsRotation], [allowsSlide], [sequence], [objectGuid] ) VALUES  ( 'PinHole', 'Technic pin hole — accepts a pin for rotational connection', 1, 1, 0, 10, 'c0110001-0001-4000-8000-000000000010' )
GO

INSERT INTO [BMC].[ConnectorType] ( [name], [description], [degreesOfFreedom], [allowsRotation], [allowsSlide], [sequence], [objectGuid] ) VALUES  ( 'Pin', 'Technic pin — inserts into a pin hole', 1, 1, 0, 11, 'c0110001-0001-4000-8000-000000000011' )
GO

INSERT INTO [BMC].[ConnectorType] ( [name], [description], [degreesOfFreedom], [allowsRotation], [allowsSlide], [sequence], [objectGuid] ) VALUES  ( 'AxleHole', 'Technic axle hole — accepts an axle for locked rotational transfer', 1, 1, 0, 12, 'c0110001-0001-4000-8000-000000000012' )
GO

INSERT INTO [BMC].[ConnectorType] ( [name], [description], [degreesOfFreedom], [allowsRotation], [allowsSlide], [sequence], [objectGuid] ) VALUES  ( 'AxleEnd', 'End of a Technic axle — inserts into an axle hole', 1, 1, 0, 13, 'c0110001-0001-4000-8000-000000000013' )
GO

INSERT INTO [BMC].[ConnectorType] ( [name], [description], [degreesOfFreedom], [allowsRotation], [allowsSlide], [sequence], [objectGuid] ) VALUES  ( 'BallJointSocket', 'Ball joint socket — accepts a ball joint for multi-axis rotation', 2, 1, 0, 20, 'c0110001-0001-4000-8000-000000000020' )
GO

INSERT INTO [BMC].[ConnectorType] ( [name], [description], [degreesOfFreedom], [allowsRotation], [allowsSlide], [sequence], [objectGuid] ) VALUES  ( 'BallJoint', 'Ball joint — inserts into a ball joint socket', 2, 1, 0, 21, 'c0110001-0001-4000-8000-000000000021' )
GO


-- Colour definitions for brick parts. Compatible with the LDraw colour standard.
CREATE TABLE [BMC].[BrickColour]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[ldrawColourCode] INT NOT NULL,		-- LDraw standard colour code number
	[hexRgb] NVARCHAR(10) NULL,		-- Hex RGB colour value (e.g. #FF0000)
	[alpha] INT NULL,		-- Alpha transparency value (0-255, 255 = fully opaque)
	[isTransparent] BIT NOT NULL DEFAULT 0,		-- Whether this colour is transparent
	[isMetallic] BIT NOT NULL DEFAULT 0,		-- Whether this colour has a metallic finish
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_BrickColour_ldrawColourCode] UNIQUE ( [ldrawColourCode]) 		-- Uniqueness enforced on the BrickColour table's ldrawColourCode field.
)
GO

-- Index on the BrickColour table's name field.
CREATE INDEX [I_BrickColour_name] ON [BMC].[BrickColour] ([name])
GO

-- Index on the BrickColour table's active field.
CREATE INDEX [I_BrickColour_active] ON [BMC].[BrickColour] ([active])
GO

-- Index on the BrickColour table's deleted field.
CREATE INDEX [I_BrickColour_deleted] ON [BMC].[BrickColour] ([deleted])
GO

INSERT INTO [BMC].[BrickColour] ( [name], [ldrawColourCode], [hexRgb], [alpha], [isTransparent], [isMetallic], [sequence], [objectGuid] ) VALUES  ( 'Black', 0, '#1B2A34', 255, 0, 0, 1, 'c0100001-0001-4000-8000-000000000001' )
GO

INSERT INTO [BMC].[BrickColour] ( [name], [ldrawColourCode], [hexRgb], [alpha], [isTransparent], [isMetallic], [sequence], [objectGuid] ) VALUES  ( 'Blue', 1, '#1E5AA8', 255, 0, 0, 2, 'c0100001-0001-4000-8000-000000000002' )
GO

INSERT INTO [BMC].[BrickColour] ( [name], [ldrawColourCode], [hexRgb], [alpha], [isTransparent], [isMetallic], [sequence], [objectGuid] ) VALUES  ( 'Green', 2, '#00852B', 255, 0, 0, 3, 'c0100001-0001-4000-8000-000000000003' )
GO

INSERT INTO [BMC].[BrickColour] ( [name], [ldrawColourCode], [hexRgb], [alpha], [isTransparent], [isMetallic], [sequence], [objectGuid] ) VALUES  ( 'Red', 4, '#B40000', 255, 0, 0, 4, 'c0100001-0001-4000-8000-000000000004' )
GO

INSERT INTO [BMC].[BrickColour] ( [name], [ldrawColourCode], [hexRgb], [alpha], [isTransparent], [isMetallic], [sequence], [objectGuid] ) VALUES  ( 'Yellow', 14, '#FFD67F', 255, 0, 0, 5, 'c0100001-0001-4000-8000-000000000005' )
GO

INSERT INTO [BMC].[BrickColour] ( [name], [ldrawColourCode], [hexRgb], [alpha], [isTransparent], [isMetallic], [sequence], [objectGuid] ) VALUES  ( 'White', 15, '#F4F4F4', 255, 0, 0, 6, 'c0100001-0001-4000-8000-000000000006' )
GO

INSERT INTO [BMC].[BrickColour] ( [name], [ldrawColourCode], [hexRgb], [alpha], [isTransparent], [isMetallic], [sequence], [objectGuid] ) VALUES  ( 'Light Bluish Grey', 71, '#A0A5A9', 255, 0, 0, 7, 'c0100001-0001-4000-8000-000000000007' )
GO

INSERT INTO [BMC].[BrickColour] ( [name], [ldrawColourCode], [hexRgb], [alpha], [isTransparent], [isMetallic], [sequence], [objectGuid] ) VALUES  ( 'Dark Bluish Grey', 72, '#6C6E68', 255, 0, 0, 8, 'c0100001-0001-4000-8000-000000000008' )
GO


-- Individual brick part definitions. Each row represents a unique part shape (independent of colour).
CREATE TABLE [BMC].[BrickPart]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[ldrawPartId] NVARCHAR(100) NOT NULL,		-- LDraw part ID (e.g. 3001, 32523) — the canonical identifier in the LDraw parts library
	[brickCategoryId] INT NULL,		-- The category this part belongs to
	[widthLdu] REAL NOT NULL,		-- Part width in LDraw units
	[heightLdu] REAL NOT NULL,		-- Part height in LDraw units
	[depthLdu] REAL NOT NULL,		-- Part depth in LDraw units
	[massGrams] REAL NOT NULL,		-- Part mass in grams (for physics simulation)
	[geometryFilePath] NVARCHAR(250) NULL,		-- Relative path to the LDraw .dat geometry file
	[toothCount] INT NULL,		-- For gears: number of teeth. Null for non-gear parts.
	[gearRatio] REAL NULL,		-- For gears: effective gear ratio relative to a base gear. Null for non-gear parts.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_BrickPart_BrickCategory_brickCategoryId] FOREIGN KEY ([brickCategoryId]) REFERENCES [BMC].[BrickCategory] ([id]),		-- Foreign key to the BrickCategory table.
	CONSTRAINT [UC_BrickPart_ldrawPartId] UNIQUE ( [ldrawPartId]) 		-- Uniqueness enforced on the BrickPart table's ldrawPartId field.
)
GO

-- Index on the BrickPart table's name field.
CREATE INDEX [I_BrickPart_name] ON [BMC].[BrickPart] ([name])
GO

-- Index on the BrickPart table's brickCategoryId field.
CREATE INDEX [I_BrickPart_brickCategoryId] ON [BMC].[BrickPart] ([brickCategoryId])
GO

-- Index on the BrickPart table's active field.
CREATE INDEX [I_BrickPart_active] ON [BMC].[BrickPart] ([active])
GO

-- Index on the BrickPart table's deleted field.
CREATE INDEX [I_BrickPart_deleted] ON [BMC].[BrickPart] ([deleted])
GO


-- The change history for records from the BrickPart table.
CREATE TABLE [BMC].[BrickPartChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[brickPartId] INT NOT NULL,		-- Link to the BrickPart table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_BrickPartChangeHistory_BrickPart_brickPartId] FOREIGN KEY ([brickPartId]) REFERENCES [BMC].[BrickPart] ([id])		-- Foreign key to the BrickPart table.
)
GO

-- Index on the BrickPartChangeHistory table's versionNumber field.
CREATE INDEX [I_BrickPartChangeHistory_versionNumber] ON [BMC].[BrickPartChangeHistory] ([versionNumber])
GO

-- Index on the BrickPartChangeHistory table's timeStamp field.
CREATE INDEX [I_BrickPartChangeHistory_timeStamp] ON [BMC].[BrickPartChangeHistory] ([timeStamp])
GO

-- Index on the BrickPartChangeHistory table's userId field.
CREATE INDEX [I_BrickPartChangeHistory_userId] ON [BMC].[BrickPartChangeHistory] ([userId])
GO

-- Index on the BrickPartChangeHistory table's brickPartId field.
CREATE INDEX [I_BrickPartChangeHistory_brickPartId] ON [BMC].[BrickPartChangeHistory] ([brickPartId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Defines the physical connection points on each brick part, including position and connector type.
CREATE TABLE [BMC].[BrickPartConnector]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[brickPartId] INT NULL,		-- The part this connector belongs to
	[connectorTypeId] INT NULL,		-- The type of connector (Stud, PinHole, AxleHole, etc.)
	[positionX] REAL NULL,		-- X position of connector relative to part origin (LDU)
	[positionY] REAL NULL,		-- Y position of connector relative to part origin (LDU)
	[positionZ] REAL NULL,		-- Z position of connector relative to part origin (LDU)
	[orientationX] REAL NULL,		-- X component of connector direction unit vector
	[orientationY] REAL NULL,		-- Y component of connector direction unit vector
	[orientationZ] REAL NULL,		-- Z component of connector direction unit vector
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_BrickPartConnector_BrickPart_brickPartId] FOREIGN KEY ([brickPartId]) REFERENCES [BMC].[BrickPart] ([id]),		-- Foreign key to the BrickPart table.
	CONSTRAINT [FK_BrickPartConnector_ConnectorType_connectorTypeId] FOREIGN KEY ([connectorTypeId]) REFERENCES [BMC].[ConnectorType] ([id])		-- Foreign key to the ConnectorType table.
)
GO

-- Index on the BrickPartConnector table's brickPartId field.
CREATE INDEX [I_BrickPartConnector_brickPartId] ON [BMC].[BrickPartConnector] ([brickPartId])
GO

-- Index on the BrickPartConnector table's connectorTypeId field.
CREATE INDEX [I_BrickPartConnector_connectorTypeId] ON [BMC].[BrickPartConnector] ([connectorTypeId])
GO

-- Index on the BrickPartConnector table's active field.
CREATE INDEX [I_BrickPartConnector_active] ON [BMC].[BrickPartConnector] ([active])
GO

-- Index on the BrickPartConnector table's deleted field.
CREATE INDEX [I_BrickPartConnector_deleted] ON [BMC].[BrickPartConnector] ([deleted])
GO


-- Maps which colours each brick part is available in. A part can exist in multiple colours.
CREATE TABLE [BMC].[BrickPartColour]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[brickPartId] INT NOT NULL,		-- The brick part
	[brickColourId] INT NOT NULL,		-- The colour this part is available in
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_BrickPartColour_BrickPart_brickPartId] FOREIGN KEY ([brickPartId]) REFERENCES [BMC].[BrickPart] ([id]),		-- Foreign key to the BrickPart table.
	CONSTRAINT [FK_BrickPartColour_BrickColour_brickColourId] FOREIGN KEY ([brickColourId]) REFERENCES [BMC].[BrickColour] ([id]),		-- Foreign key to the BrickColour table.
	CONSTRAINT [UC_BrickPartColour_brickPartId_brickColourId] UNIQUE ( [brickPartId], [brickColourId]) 		-- Uniqueness enforced on the BrickPartColour table's brickPartId and brickColourId fields.
)
GO

-- Index on the BrickPartColour table's brickPartId field.
CREATE INDEX [I_BrickPartColour_brickPartId] ON [BMC].[BrickPartColour] ([brickPartId])
GO

-- Index on the BrickPartColour table's brickColourId field.
CREATE INDEX [I_BrickPartColour_brickColourId] ON [BMC].[BrickPartColour] ([brickColourId])
GO

-- Index on the BrickPartColour table's active field.
CREATE INDEX [I_BrickPartColour_active] ON [BMC].[BrickPartColour] ([active])
GO

-- Index on the BrickPartColour table's deleted field.
CREATE INDEX [I_BrickPartColour_deleted] ON [BMC].[BrickPartColour] ([deleted])
GO


-- A user's building project. Contains placed bricks and their connections to form a model.
CREATE TABLE [BMC].[Project]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[notes] NVARCHAR(MAX) NULL,		-- Free-form notes about the project
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_Project_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Project table's tenantGuid and name fields.
)
GO

-- Index on the Project table's tenantGuid field.
CREATE INDEX [I_Project_tenantGuid] ON [BMC].[Project] ([tenantGuid])
GO

-- Index on the Project table's tenantGuid,name fields.
CREATE INDEX [I_Project_tenantGuid_name] ON [BMC].[Project] ([tenantGuid], [name])
GO

-- Index on the Project table's tenantGuid,active fields.
CREATE INDEX [I_Project_tenantGuid_active] ON [BMC].[Project] ([tenantGuid], [active])
GO

-- Index on the Project table's tenantGuid,deleted fields.
CREATE INDEX [I_Project_tenantGuid_deleted] ON [BMC].[Project] ([tenantGuid], [deleted])
GO


-- The change history for records from the Project table.
CREATE TABLE [BMC].[ProjectChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[projectId] INT NOT NULL,		-- Link to the Project table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ProjectChangeHistory_Project_projectId] FOREIGN KEY ([projectId]) REFERENCES [BMC].[Project] ([id])		-- Foreign key to the Project table.
)
GO

-- Index on the ProjectChangeHistory table's tenantGuid field.
CREATE INDEX [I_ProjectChangeHistory_tenantGuid] ON [BMC].[ProjectChangeHistory] ([tenantGuid])
GO

-- Index on the ProjectChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ProjectChangeHistory_tenantGuid_versionNumber] ON [BMC].[ProjectChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ProjectChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ProjectChangeHistory_tenantGuid_timeStamp] ON [BMC].[ProjectChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ProjectChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ProjectChangeHistory_tenantGuid_userId] ON [BMC].[ProjectChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ProjectChangeHistory table's tenantGuid,projectId fields.
CREATE INDEX [I_ProjectChangeHistory_tenantGuid_projectId] ON [BMC].[ProjectChangeHistory] ([tenantGuid], [projectId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- An instance of a brick part placed within a project. Tracks position, rotation, and colour.
CREATE TABLE [BMC].[PlacedBrick]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[projectId] INT NULL,		-- The project this brick is placed in
	[brickPartId] INT NULL,		-- The part definition being placed
	[brickColourId] INT NULL,		-- The colour of this placed brick instance
	[positionX] REAL NULL,		-- X position in world coordinates (LDU)
	[positionY] REAL NULL,		-- Y position in world coordinates (LDU)
	[positionZ] REAL NULL,		-- Z position in world coordinates (LDU)
	[rotationX] REAL NULL,		-- Quaternion X component
	[rotationY] REAL NULL,		-- Quaternion Y component
	[rotationZ] REAL NULL,		-- Quaternion Z component
	[rotationW] REAL NULL,		-- Quaternion W component
	[buildStepNumber] INT NULL,		-- Optional build step number for instruction ordering
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_PlacedBrick_Project_projectId] FOREIGN KEY ([projectId]) REFERENCES [BMC].[Project] ([id]),		-- Foreign key to the Project table.
	CONSTRAINT [FK_PlacedBrick_BrickPart_brickPartId] FOREIGN KEY ([brickPartId]) REFERENCES [BMC].[BrickPart] ([id]),		-- Foreign key to the BrickPart table.
	CONSTRAINT [FK_PlacedBrick_BrickColour_brickColourId] FOREIGN KEY ([brickColourId]) REFERENCES [BMC].[BrickColour] ([id])		-- Foreign key to the BrickColour table.
)
GO

-- Index on the PlacedBrick table's tenantGuid field.
CREATE INDEX [I_PlacedBrick_tenantGuid] ON [BMC].[PlacedBrick] ([tenantGuid])
GO

-- Index on the PlacedBrick table's tenantGuid,projectId fields.
CREATE INDEX [I_PlacedBrick_tenantGuid_projectId] ON [BMC].[PlacedBrick] ([tenantGuid], [projectId])
GO

-- Index on the PlacedBrick table's tenantGuid,brickPartId fields.
CREATE INDEX [I_PlacedBrick_tenantGuid_brickPartId] ON [BMC].[PlacedBrick] ([tenantGuid], [brickPartId])
GO

-- Index on the PlacedBrick table's tenantGuid,brickColourId fields.
CREATE INDEX [I_PlacedBrick_tenantGuid_brickColourId] ON [BMC].[PlacedBrick] ([tenantGuid], [brickColourId])
GO

-- Index on the PlacedBrick table's tenantGuid,active fields.
CREATE INDEX [I_PlacedBrick_tenantGuid_active] ON [BMC].[PlacedBrick] ([tenantGuid], [active])
GO

-- Index on the PlacedBrick table's tenantGuid,deleted fields.
CREATE INDEX [I_PlacedBrick_tenantGuid_deleted] ON [BMC].[PlacedBrick] ([tenantGuid], [deleted])
GO


-- The change history for records from the PlacedBrick table.
CREATE TABLE [BMC].[PlacedBrickChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[placedBrickId] INT NOT NULL,		-- Link to the PlacedBrick table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_PlacedBrickChangeHistory_PlacedBrick_placedBrickId] FOREIGN KEY ([placedBrickId]) REFERENCES [BMC].[PlacedBrick] ([id])		-- Foreign key to the PlacedBrick table.
)
GO

-- Index on the PlacedBrickChangeHistory table's tenantGuid field.
CREATE INDEX [I_PlacedBrickChangeHistory_tenantGuid] ON [BMC].[PlacedBrickChangeHistory] ([tenantGuid])
GO

-- Index on the PlacedBrickChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_PlacedBrickChangeHistory_tenantGuid_versionNumber] ON [BMC].[PlacedBrickChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the PlacedBrickChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_PlacedBrickChangeHistory_tenantGuid_timeStamp] ON [BMC].[PlacedBrickChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the PlacedBrickChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_PlacedBrickChangeHistory_tenantGuid_userId] ON [BMC].[PlacedBrickChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the PlacedBrickChangeHistory table's tenantGuid,placedBrickId fields.
CREATE INDEX [I_PlacedBrickChangeHistory_tenantGuid_placedBrickId] ON [BMC].[PlacedBrickChangeHistory] ([tenantGuid], [placedBrickId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Records which connector on one placed brick is joined to which connector on another placed brick.
CREATE TABLE [BMC].[BrickConnection]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[projectId] INT NULL,		-- The project this connection belongs to
	[sourcePlacedBrickId] BIGINT NULL,		-- FK to the source PlacedBrick
	[sourceConnectorId] BIGINT NULL,		-- FK to the BrickPartConnector on the source brick
	[targetPlacedBrickId] BIGINT NULL,		-- FK to the target PlacedBrick
	[targetConnectorId] BIGINT NULL,		-- FK to the BrickPartConnector on the target brick
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_BrickConnection_Project_projectId] FOREIGN KEY ([projectId]) REFERENCES [BMC].[Project] ([id])		-- Foreign key to the Project table.
)
GO

-- Index on the BrickConnection table's tenantGuid field.
CREATE INDEX [I_BrickConnection_tenantGuid] ON [BMC].[BrickConnection] ([tenantGuid])
GO

-- Index on the BrickConnection table's tenantGuid,projectId fields.
CREATE INDEX [I_BrickConnection_tenantGuid_projectId] ON [BMC].[BrickConnection] ([tenantGuid], [projectId])
GO

-- Index on the BrickConnection table's tenantGuid,active fields.
CREATE INDEX [I_BrickConnection_tenantGuid_active] ON [BMC].[BrickConnection] ([tenantGuid], [active])
GO

-- Index on the BrickConnection table's tenantGuid,deleted fields.
CREATE INDEX [I_BrickConnection_tenantGuid_deleted] ON [BMC].[BrickConnection] ([tenantGuid], [deleted])
GO


