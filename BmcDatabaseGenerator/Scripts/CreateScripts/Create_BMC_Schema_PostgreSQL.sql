/*
BMC (Brick Machine Construction) database schema.
This is a multi-tenant interactive building and physics simulation platform for LEGO and LEGO Technic models.
It supports a parts catalog with connector metadata, 3D model building with placement and connection tracking,
and physics simulation of mechanical assemblies.
All operational tables include multi-tenant support, versioning where appropriate, auditing, and security controls.
*/
CREATE DATABASE "BMC"
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    TEMPLATE=template0;

\connect "BMC"   -- Run the create database first, then connect to it, and run the rest if running in a query tool rather than through psql scripting.



CREATE SCHEMA "BMC"

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE "BMC"."BrickConnection"
-- DROP TABLE "BMC"."PlacedBrickChangeHistory"
-- DROP TABLE "BMC"."PlacedBrick"
-- DROP TABLE "BMC"."ProjectChangeHistory"
-- DROP TABLE "BMC"."Project"
-- DROP TABLE "BMC"."BrickPartColour"
-- DROP TABLE "BMC"."BrickPartConnector"
-- DROP TABLE "BMC"."BrickPartChangeHistory"
-- DROP TABLE "BMC"."BrickPart"
-- DROP TABLE "BMC"."BrickColour"
-- DROP TABLE "BMC"."ConnectorType"
-- DROP TABLE "BMC"."BrickCategory"

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON "BrickConnection" DISABLE
-- ALTER INDEX ALL ON "PlacedBrickChangeHistory" DISABLE
-- ALTER INDEX ALL ON "PlacedBrick" DISABLE
-- ALTER INDEX ALL ON "ProjectChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Project" DISABLE
-- ALTER INDEX ALL ON "BrickPartColour" DISABLE
-- ALTER INDEX ALL ON "BrickPartConnector" DISABLE
-- ALTER INDEX ALL ON "BrickPartChangeHistory" DISABLE
-- ALTER INDEX ALL ON "BrickPart" DISABLE
-- ALTER INDEX ALL ON "BrickColour" DISABLE
-- ALTER INDEX ALL ON "ConnectorType" DISABLE
-- ALTER INDEX ALL ON "BrickCategory" DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON "BrickConnection" REBUILD
-- ALTER INDEX ALL ON "PlacedBrickChangeHistory" REBUILD
-- ALTER INDEX ALL ON "PlacedBrick" REBUILD
-- ALTER INDEX ALL ON "ProjectChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Project" REBUILD
-- ALTER INDEX ALL ON "BrickPartColour" REBUILD
-- ALTER INDEX ALL ON "BrickPartConnector" REBUILD
-- ALTER INDEX ALL ON "BrickPartChangeHistory" REBUILD
-- ALTER INDEX ALL ON "BrickPart" REBUILD
-- ALTER INDEX ALL ON "BrickColour" REBUILD
-- ALTER INDEX ALL ON "ConnectorType" REBUILD
-- ALTER INDEX ALL ON "BrickCategory" REBUILD

-- Master list of part categories (Beam, Plate, Gear, Axle, Motor, Pneumatic, etc.)
CREATE TABLE "BMC"."BrickCategory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the BrickCategory table's name field.
CREATE INDEX "I_BrickCategory_name" ON "BMC"."BrickCategory" ("name")
;

-- Index on the BrickCategory table's active field.
CREATE INDEX "I_BrickCategory_active" ON "BMC"."BrickCategory" ("active")
;

-- Index on the BrickCategory table's deleted field.
CREATE INDEX "I_BrickCategory_deleted" ON "BMC"."BrickCategory" ("deleted")
;

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Plate', 'Standard plates of various sizes', 1, 'b1c10001-0001-4000-8000-000000000001' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Brick', 'Standard bricks of various sizes', 2, 'b1c10001-0001-4000-8000-000000000002' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Tile', 'Smooth-top tiles without studs', 3, 'b1c10001-0001-4000-8000-000000000003' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Slope', 'Angled slope bricks and roof pieces', 4, 'b1c10001-0001-4000-8000-000000000004' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Wedge', 'Wedge-shaped plates and bricks', 5, 'b1c10001-0001-4000-8000-000000000005' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Arch', 'Arched bricks and curved elements', 6, 'b1c10001-0001-4000-8000-000000000006' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Cylinder', 'Round bricks, cylinders, and cones', 7, 'b1c10001-0001-4000-8000-000000000007' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Cone', 'Cone-shaped parts', 8, 'b1c10001-0001-4000-8000-000000000008' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Bracket', 'Angle brackets for sideways building', 9, 'b1c10001-0001-4000-8000-000000000009' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Beam', 'Technic beams and liftarms', 10, 'b1c10001-0001-4000-8000-000000000010' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Pin', 'Technic pins and connectors', 11, 'b1c10001-0001-4000-8000-000000000011' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Axle', 'Technic axles of various lengths', 12, 'b1c10001-0001-4000-8000-000000000012' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Gear', 'Technic gears of various tooth counts', 13, 'b1c10001-0001-4000-8000-000000000013' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Motor', 'Powered motors (Power Functions, Powered Up)', 14, 'b1c10001-0001-4000-8000-000000000014' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Pneumatic', 'Pneumatic cylinders, pumps, and tubing', 15, 'b1c10001-0001-4000-8000-000000000015' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Differential', 'Differential gear assemblies', 16, 'b1c10001-0001-4000-8000-000000000016' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Hinge', 'Hinge bricks, plates, and click hinges', 17, 'b1c10001-0001-4000-8000-000000000017' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Panel', 'Panels, fairings, and body pieces', 20, 'b1c10001-0001-4000-8000-000000000020' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Wheel', 'Wheels, tyres, and rims', 21, 'b1c10001-0001-4000-8000-000000000021' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Window', 'Windows, glass, and frames', 22, 'b1c10001-0001-4000-8000-000000000022' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Door', 'Doors and door frames', 23, 'b1c10001-0001-4000-8000-000000000023' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Fence', 'Fences, railings, and barriers', 24, 'b1c10001-0001-4000-8000-000000000024' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Baseplate', 'Baseplates and road plates', 25, 'b1c10001-0001-4000-8000-000000000025' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Bar', 'Bars, antennas, and clips', 26, 'b1c10001-0001-4000-8000-000000000026' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Support', 'Support structures, columns, and pillars', 27, 'b1c10001-0001-4000-8000-000000000027' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Container', 'Boxes, crates, and storage containers', 28, 'b1c10001-0001-4000-8000-000000000028' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Decorative', 'Decorative, printed, and sticker parts', 30, 'b1c10001-0001-4000-8000-000000000030' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Electric', 'Electrical components, lights, and sensors', 31, 'b1c10001-0001-4000-8000-000000000031' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Propeller', 'Propellers, rotors, and blades', 32, 'b1c10001-0001-4000-8000-000000000032' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Wing', 'Wings and aircraft body parts', 33, 'b1c10001-0001-4000-8000-000000000033' );

INSERT INTO "BMC"."BrickCategory" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Train', 'Train track, wheels, and specialized train parts', 34, 'b1c10001-0001-4000-8000-000000000034' );


-- Master list of physical connection types that define how parts can join together.
CREATE TABLE "BMC"."ConnectorType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"degreesOfFreedom" INT NULL,		-- Number of degrees of freedom when connected (0=fixed, 1=rotation, 2=rotation+slide)
	"allowsRotation" BOOLEAN NOT NULL DEFAULT false,		-- Whether this connection allows rotation around its axis
	"allowsSlide" BOOLEAN NOT NULL DEFAULT false,		-- Whether this connection allows sliding along its axis
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the ConnectorType table's name field.
CREATE INDEX "I_ConnectorType_name" ON "BMC"."ConnectorType" ("name")
;

-- Index on the ConnectorType table's active field.
CREATE INDEX "I_ConnectorType_active" ON "BMC"."ConnectorType" ("active")
;

-- Index on the ConnectorType table's deleted field.
CREATE INDEX "I_ConnectorType_deleted" ON "BMC"."ConnectorType" ("deleted")
;

INSERT INTO "BMC"."ConnectorType" ( "name", "description", "degreesOfFreedom", "allowsRotation", "allowsSlide", "sequence", "objectGuid" ) VALUES  ( 'Stud', 'Standard LEGO stud (male connector)', 0, false, false, 1, 'c0110001-0001-4000-8000-000000000001' );

INSERT INTO "BMC"."ConnectorType" ( "name", "description", "degreesOfFreedom", "allowsRotation", "allowsSlide", "sequence", "objectGuid" ) VALUES  ( 'AntiStud', 'Standard LEGO anti-stud receptacle (female connector)', 0, false, false, 2, 'c0110001-0001-4000-8000-000000000002' );

INSERT INTO "BMC"."ConnectorType" ( "name", "description", "degreesOfFreedom", "allowsRotation", "allowsSlide", "sequence", "objectGuid" ) VALUES  ( 'PinHole', 'Technic pin hole — accepts a pin for rotational connection', 1, false, false, 10, 'c0110001-0001-4000-8000-000000000010' );

INSERT INTO "BMC"."ConnectorType" ( "name", "description", "degreesOfFreedom", "allowsRotation", "allowsSlide", "sequence", "objectGuid" ) VALUES  ( 'Pin', 'Technic pin — inserts into a pin hole', 1, false, false, 11, 'c0110001-0001-4000-8000-000000000011' );

INSERT INTO "BMC"."ConnectorType" ( "name", "description", "degreesOfFreedom", "allowsRotation", "allowsSlide", "sequence", "objectGuid" ) VALUES  ( 'AxleHole', 'Technic axle hole — accepts an axle for locked rotational transfer', 1, false, false, 12, 'c0110001-0001-4000-8000-000000000012' );

INSERT INTO "BMC"."ConnectorType" ( "name", "description", "degreesOfFreedom", "allowsRotation", "allowsSlide", "sequence", "objectGuid" ) VALUES  ( 'AxleEnd', 'End of a Technic axle — inserts into an axle hole', 1, false, false, 13, 'c0110001-0001-4000-8000-000000000013' );

INSERT INTO "BMC"."ConnectorType" ( "name", "description", "degreesOfFreedom", "allowsRotation", "allowsSlide", "sequence", "objectGuid" ) VALUES  ( 'BallJointSocket', 'Ball joint socket — accepts a ball joint for multi-axis rotation', 2, false, false, 20, 'c0110001-0001-4000-8000-000000000020' );

INSERT INTO "BMC"."ConnectorType" ( "name", "description", "degreesOfFreedom", "allowsRotation", "allowsSlide", "sequence", "objectGuid" ) VALUES  ( 'BallJoint', 'Ball joint — inserts into a ball joint socket', 2, false, false, 21, 'c0110001-0001-4000-8000-000000000021' );


-- Colour definitions for brick parts. Compatible with the LDraw colour standard.
CREATE TABLE "BMC"."BrickColour"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"ldrawColourCode" INT NOT NULL,		-- LDraw standard colour code number
	"hexRgb" VARCHAR(10) NULL,		-- Hex RGB colour value (e.g. #FF0000)
	"hexEdgeColour" VARCHAR(10) NULL,		-- LDraw edge/contrast colour hex value for wireframe and outline rendering
	"alpha" INT NULL,		-- Alpha transparency value (0-255, 255 = fully opaque)
	"isTransparent" BOOLEAN NOT NULL DEFAULT false,		-- Whether this colour is transparent (convenience flag derived from alpha)
	"isMetallic" BOOLEAN NOT NULL DEFAULT false,		-- Whether this colour has a metallic finish (convenience flag derived from finishType)
	"finishType" VARCHAR(50) NULL,		-- Material finish type: Solid, Chrome, Pearlescent, Metal, Rubber, Glitter, Speckle, Fabric, Milky
	"luminance" INT NULL,		-- Glow brightness (0-255) for glow-in-the-dark colours. Null for non-glowing.
	"legoColourId" INT NULL,		-- Official LEGO colour number for cross-referencing with LEGO catalogues
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_BrickColour_ldrawColourCode" UNIQUE ( "ldrawColourCode") 		-- Uniqueness enforced on the BrickColour table's ldrawColourCode field.
);
-- Index on the BrickColour table's name field.
CREATE INDEX "I_BrickColour_name" ON "BMC"."BrickColour" ("name")
;

-- Index on the BrickColour table's active field.
CREATE INDEX "I_BrickColour_active" ON "BMC"."BrickColour" ("active")
;

-- Index on the BrickColour table's deleted field.
CREATE INDEX "I_BrickColour_deleted" ON "BMC"."BrickColour" ("deleted")
;

INSERT INTO "BMC"."BrickColour" ( "name", "ldrawColourCode", "hexRgb", "hexEdgeColour", "alpha", "isTransparent", "isMetallic", "finishType", "legoColourId", "sequence", "objectGuid" ) VALUES  ( 'Black', 0, '#1B2A34', '#808080', 255, false, false, 'Solid', 26, 1, 'c0100001-0001-4000-8000-000000000001' );

INSERT INTO "BMC"."BrickColour" ( "name", "ldrawColourCode", "hexRgb", "hexEdgeColour", "alpha", "isTransparent", "isMetallic", "finishType", "legoColourId", "sequence", "objectGuid" ) VALUES  ( 'Blue', 1, '#1E5AA8', '#333333', 255, false, false, 'Solid', 23, 2, 'c0100001-0001-4000-8000-000000000002' );

INSERT INTO "BMC"."BrickColour" ( "name", "ldrawColourCode", "hexRgb", "hexEdgeColour", "alpha", "isTransparent", "isMetallic", "finishType", "legoColourId", "sequence", "objectGuid" ) VALUES  ( 'Green', 2, '#00852B', '#333333', 255, false, false, 'Solid', 28, 3, 'c0100001-0001-4000-8000-000000000003' );

INSERT INTO "BMC"."BrickColour" ( "name", "ldrawColourCode", "hexRgb", "hexEdgeColour", "alpha", "isTransparent", "isMetallic", "finishType", "legoColourId", "sequence", "objectGuid" ) VALUES  ( 'Red', 4, '#B40000', '#333333', 255, false, false, 'Solid', 21, 4, 'c0100001-0001-4000-8000-000000000004' );

INSERT INTO "BMC"."BrickColour" ( "name", "ldrawColourCode", "hexRgb", "hexEdgeColour", "alpha", "isTransparent", "isMetallic", "finishType", "legoColourId", "sequence", "objectGuid" ) VALUES  ( 'Yellow', 14, '#FAC80A', '#333333', 255, false, false, 'Solid', 24, 5, 'c0100001-0001-4000-8000-000000000005' );

INSERT INTO "BMC"."BrickColour" ( "name", "ldrawColourCode", "hexRgb", "hexEdgeColour", "alpha", "isTransparent", "isMetallic", "finishType", "legoColourId", "sequence", "objectGuid" ) VALUES  ( 'White', 15, '#F4F4F4', '#333333', 255, false, false, 'Solid', 1, 6, 'c0100001-0001-4000-8000-000000000006' );

INSERT INTO "BMC"."BrickColour" ( "name", "ldrawColourCode", "hexRgb", "hexEdgeColour", "alpha", "isTransparent", "isMetallic", "finishType", "legoColourId", "sequence", "objectGuid" ) VALUES  ( 'Light Bluish Grey', 71, '#969696', '#333333', 255, false, false, 'Solid', 194, 7, 'c0100001-0001-4000-8000-000000000007' );

INSERT INTO "BMC"."BrickColour" ( "name", "ldrawColourCode", "hexRgb", "hexEdgeColour", "alpha", "isTransparent", "isMetallic", "finishType", "legoColourId", "sequence", "objectGuid" ) VALUES  ( 'Dark Bluish Grey', 72, '#646464', '#333333', 255, false, false, 'Solid', 199, 8, 'c0100001-0001-4000-8000-000000000008' );


-- Individual brick part definitions. Each row represents a unique part shape (independent of colour).
CREATE TABLE "BMC"."BrickPart"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"ldrawPartId" VARCHAR(100) NOT NULL,		-- LDraw part ID (e.g. 3001, 32523) — the canonical identifier in the LDraw parts library
	"ldrawTitle" VARCHAR(250) NULL,		-- Raw title from the LDraw .dat file (e.g. 'Brick  2 x  4', 'Technic Beam  3')
	"ldrawCategory" VARCHAR(100) NULL,		-- Part category from LDraw !CATEGORY meta or inferred from title first word
	"partType" VARCHAR(50) NULL,		-- LDraw part type: Part, Subpart, Primitive, Shortcut, Alias, etc.
	"keywords" TEXT NULL,		-- Comma-separated keywords from LDraw !KEYWORDS meta lines for search
	"author" VARCHAR(100) NULL,		-- Part author from the LDraw Author: header line
	"brickCategoryId" INT NULL,		-- The category this part belongs to
	"widthLdu" REAL NOT NULL,		-- Part width in LDraw units
	"heightLdu" REAL NOT NULL,		-- Part height in LDraw units
	"depthLdu" REAL NOT NULL,		-- Part depth in LDraw units
	"massGrams" REAL NOT NULL,		-- Part mass in grams (for physics simulation)
	"geometryFilePath" VARCHAR(250) NULL,		-- Relative path to the LDraw .dat geometry file
	"toothCount" INT NULL,		-- For gears: number of teeth. Null for non-gear parts.
	"gearRatio" REAL NULL,		-- For gears: effective gear ratio relative to a base gear. Null for non-gear parts.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "brickCategoryId" FOREIGN KEY ("brickCategoryId") REFERENCES "BMC"."BrickCategory"("id"),		-- Foreign key to the BrickCategory table.
	CONSTRAINT "UC_BrickPart_ldrawPartId" UNIQUE ( "ldrawPartId") 		-- Uniqueness enforced on the BrickPart table's ldrawPartId field.
);
-- Index on the BrickPart table's name field.
CREATE INDEX "I_BrickPart_name" ON "BMC"."BrickPart" ("name")
;

-- Index on the BrickPart table's brickCategoryId field.
CREATE INDEX "I_BrickPart_brickCategoryId" ON "BMC"."BrickPart" ("brickCategoryId")
;

-- Index on the BrickPart table's active field.
CREATE INDEX "I_BrickPart_active" ON "BMC"."BrickPart" ("active")
;

-- Index on the BrickPart table's deleted field.
CREATE INDEX "I_BrickPart_deleted" ON "BMC"."BrickPart" ("deleted")
;


-- The change history for records from the BrickPart table.
CREATE TABLE "BMC"."BrickPartChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"brickPartId" INT NOT NULL,		-- Link to the BrickPart table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "brickPartId" FOREIGN KEY ("brickPartId") REFERENCES "BMC"."BrickPart"("id")		-- Foreign key to the BrickPart table.
);
-- Index on the BrickPartChangeHistory table's versionNumber field.
CREATE INDEX "I_BrickPartChangeHistory_versionNumber" ON "BMC"."BrickPartChangeHistory" ("versionNumber")
;

-- Index on the BrickPartChangeHistory table's timeStamp field.
CREATE INDEX "I_BrickPartChangeHistory_timeStamp" ON "BMC"."BrickPartChangeHistory" ("timeStamp")
;

-- Index on the BrickPartChangeHistory table's userId field.
CREATE INDEX "I_BrickPartChangeHistory_userId" ON "BMC"."BrickPartChangeHistory" ("userId")
;

-- Index on the BrickPartChangeHistory table's brickPartId field.
CREATE INDEX "I_BrickPartChangeHistory_brickPartId" ON "BMC"."BrickPartChangeHistory" ("brickPartId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Defines the physical connection points on each brick part, including position and connector type.
CREATE TABLE "BMC"."BrickPartConnector"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"brickPartId" INT NULL,		-- The part this connector belongs to
	"connectorTypeId" INT NULL,		-- The type of connector (Stud, PinHole, AxleHole, etc.)
	"positionX" REAL NULL,		-- X position of connector relative to part origin (LDU)
	"positionY" REAL NULL,		-- Y position of connector relative to part origin (LDU)
	"positionZ" REAL NULL,		-- Z position of connector relative to part origin (LDU)
	"orientationX" REAL NULL,		-- X component of connector direction unit vector
	"orientationY" REAL NULL,		-- Y component of connector direction unit vector
	"orientationZ" REAL NULL,		-- Z component of connector direction unit vector
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "brickPartId" FOREIGN KEY ("brickPartId") REFERENCES "BMC"."BrickPart"("id"),		-- Foreign key to the BrickPart table.
	CONSTRAINT "connectorTypeId" FOREIGN KEY ("connectorTypeId") REFERENCES "BMC"."ConnectorType"("id")		-- Foreign key to the ConnectorType table.
);
-- Index on the BrickPartConnector table's brickPartId field.
CREATE INDEX "I_BrickPartConnector_brickPartId" ON "BMC"."BrickPartConnector" ("brickPartId")
;

-- Index on the BrickPartConnector table's connectorTypeId field.
CREATE INDEX "I_BrickPartConnector_connectorTypeId" ON "BMC"."BrickPartConnector" ("connectorTypeId")
;

-- Index on the BrickPartConnector table's active field.
CREATE INDEX "I_BrickPartConnector_active" ON "BMC"."BrickPartConnector" ("active")
;

-- Index on the BrickPartConnector table's deleted field.
CREATE INDEX "I_BrickPartConnector_deleted" ON "BMC"."BrickPartConnector" ("deleted")
;


-- Maps which colours each brick part is available in. A part can exist in multiple colours.
CREATE TABLE "BMC"."BrickPartColour"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"brickPartId" INT NOT NULL,		-- The brick part
	"brickColourId" INT NOT NULL,		-- The colour this part is available in
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "brickPartId" FOREIGN KEY ("brickPartId") REFERENCES "BMC"."BrickPart"("id"),		-- Foreign key to the BrickPart table.
	CONSTRAINT "brickColourId" FOREIGN KEY ("brickColourId") REFERENCES "BMC"."BrickColour"("id"),		-- Foreign key to the BrickColour table.
	CONSTRAINT "UC_BrickPartColour_brickPartId_brickColourId" UNIQUE ( "brickPartId", "brickColourId") 		-- Uniqueness enforced on the BrickPartColour table's brickPartId and brickColourId fields.
);
-- Index on the BrickPartColour table's brickPartId field.
CREATE INDEX "I_BrickPartColour_brickPartId" ON "BMC"."BrickPartColour" ("brickPartId")
;

-- Index on the BrickPartColour table's brickColourId field.
CREATE INDEX "I_BrickPartColour_brickColourId" ON "BMC"."BrickPartColour" ("brickColourId")
;

-- Index on the BrickPartColour table's active field.
CREATE INDEX "I_BrickPartColour_active" ON "BMC"."BrickPartColour" ("active")
;

-- Index on the BrickPartColour table's deleted field.
CREATE INDEX "I_BrickPartColour_deleted" ON "BMC"."BrickPartColour" ("deleted")
;


-- A user's building project. Contains placed bricks and their connections to form a model.
CREATE TABLE "BMC"."Project"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"notes" TEXT NULL,		-- Free-form notes about the project
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_Project_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Project table's tenantGuid and name fields.
);
-- Index on the Project table's tenantGuid field.
CREATE INDEX "I_Project_tenantGuid" ON "BMC"."Project" ("tenantGuid")
;

-- Index on the Project table's tenantGuid,name fields.
CREATE INDEX "I_Project_tenantGuid_name" ON "BMC"."Project" ("tenantGuid", "name")
;

-- Index on the Project table's tenantGuid,active fields.
CREATE INDEX "I_Project_tenantGuid_active" ON "BMC"."Project" ("tenantGuid", "active")
;

-- Index on the Project table's tenantGuid,deleted fields.
CREATE INDEX "I_Project_tenantGuid_deleted" ON "BMC"."Project" ("tenantGuid", "deleted")
;


-- The change history for records from the Project table.
CREATE TABLE "BMC"."ProjectChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"projectId" INT NOT NULL,		-- Link to the Project table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "projectId" FOREIGN KEY ("projectId") REFERENCES "BMC"."Project"("id")		-- Foreign key to the Project table.
);
-- Index on the ProjectChangeHistory table's tenantGuid field.
CREATE INDEX "I_ProjectChangeHistory_tenantGuid" ON "BMC"."ProjectChangeHistory" ("tenantGuid")
;

-- Index on the ProjectChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ProjectChangeHistory_tenantGuid_versionNumber" ON "BMC"."ProjectChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ProjectChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ProjectChangeHistory_tenantGuid_timeStamp" ON "BMC"."ProjectChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ProjectChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ProjectChangeHistory_tenantGuid_userId" ON "BMC"."ProjectChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ProjectChangeHistory table's tenantGuid,projectId fields.
CREATE INDEX "I_ProjectChangeHistory_tenantGuid_projectId" ON "BMC"."ProjectChangeHistory" ("tenantGuid", "projectId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- An instance of a brick part placed within a project. Tracks position, rotation, and colour.
CREATE TABLE "BMC"."PlacedBrick"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"projectId" INT NULL,		-- The project this brick is placed in
	"brickPartId" INT NULL,		-- The part definition being placed
	"brickColourId" INT NULL,		-- The colour of this placed brick instance
	"positionX" REAL NULL,		-- X position in world coordinates (LDU)
	"positionY" REAL NULL,		-- Y position in world coordinates (LDU)
	"positionZ" REAL NULL,		-- Z position in world coordinates (LDU)
	"rotationX" REAL NULL,		-- Quaternion X component
	"rotationY" REAL NULL,		-- Quaternion Y component
	"rotationZ" REAL NULL,		-- Quaternion Z component
	"rotationW" REAL NULL,		-- Quaternion W component
	"buildStepNumber" INT NULL,		-- Optional build step number for instruction ordering
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "projectId" FOREIGN KEY ("projectId") REFERENCES "BMC"."Project"("id"),		-- Foreign key to the Project table.
	CONSTRAINT "brickPartId" FOREIGN KEY ("brickPartId") REFERENCES "BMC"."BrickPart"("id"),		-- Foreign key to the BrickPart table.
	CONSTRAINT "brickColourId" FOREIGN KEY ("brickColourId") REFERENCES "BMC"."BrickColour"("id")		-- Foreign key to the BrickColour table.
);
-- Index on the PlacedBrick table's tenantGuid field.
CREATE INDEX "I_PlacedBrick_tenantGuid" ON "BMC"."PlacedBrick" ("tenantGuid")
;

-- Index on the PlacedBrick table's tenantGuid,projectId fields.
CREATE INDEX "I_PlacedBrick_tenantGuid_projectId" ON "BMC"."PlacedBrick" ("tenantGuid", "projectId")
;

-- Index on the PlacedBrick table's tenantGuid,brickPartId fields.
CREATE INDEX "I_PlacedBrick_tenantGuid_brickPartId" ON "BMC"."PlacedBrick" ("tenantGuid", "brickPartId")
;

-- Index on the PlacedBrick table's tenantGuid,brickColourId fields.
CREATE INDEX "I_PlacedBrick_tenantGuid_brickColourId" ON "BMC"."PlacedBrick" ("tenantGuid", "brickColourId")
;

-- Index on the PlacedBrick table's tenantGuid,active fields.
CREATE INDEX "I_PlacedBrick_tenantGuid_active" ON "BMC"."PlacedBrick" ("tenantGuid", "active")
;

-- Index on the PlacedBrick table's tenantGuid,deleted fields.
CREATE INDEX "I_PlacedBrick_tenantGuid_deleted" ON "BMC"."PlacedBrick" ("tenantGuid", "deleted")
;


-- The change history for records from the PlacedBrick table.
CREATE TABLE "BMC"."PlacedBrickChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"placedBrickId" INT NOT NULL,		-- Link to the PlacedBrick table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "placedBrickId" FOREIGN KEY ("placedBrickId") REFERENCES "BMC"."PlacedBrick"("id")		-- Foreign key to the PlacedBrick table.
);
-- Index on the PlacedBrickChangeHistory table's tenantGuid field.
CREATE INDEX "I_PlacedBrickChangeHistory_tenantGuid" ON "BMC"."PlacedBrickChangeHistory" ("tenantGuid")
;

-- Index on the PlacedBrickChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_PlacedBrickChangeHistory_tenantGuid_versionNumber" ON "BMC"."PlacedBrickChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the PlacedBrickChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_PlacedBrickChangeHistory_tenantGuid_timeStamp" ON "BMC"."PlacedBrickChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the PlacedBrickChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_PlacedBrickChangeHistory_tenantGuid_userId" ON "BMC"."PlacedBrickChangeHistory" ("tenantGuid", "userId")
;

-- Index on the PlacedBrickChangeHistory table's tenantGuid,placedBrickId fields.
CREATE INDEX "I_PlacedBrickChangeHistory_tenantGuid_placedBrickId" ON "BMC"."PlacedBrickChangeHistory" ("tenantGuid", "placedBrickId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Records which connector on one placed brick is joined to which connector on another placed brick.
CREATE TABLE "BMC"."BrickConnection"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"projectId" INT NULL,		-- The project this connection belongs to
	"sourcePlacedBrickId" BIGINT NULL,		-- FK to the source PlacedBrick
	"sourceConnectorId" BIGINT NULL,		-- FK to the BrickPartConnector on the source brick
	"targetPlacedBrickId" BIGINT NULL,		-- FK to the target PlacedBrick
	"targetConnectorId" BIGINT NULL,		-- FK to the BrickPartConnector on the target brick
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "projectId" FOREIGN KEY ("projectId") REFERENCES "BMC"."Project"("id")		-- Foreign key to the Project table.
);
-- Index on the BrickConnection table's tenantGuid field.
CREATE INDEX "I_BrickConnection_tenantGuid" ON "BMC"."BrickConnection" ("tenantGuid")
;

-- Index on the BrickConnection table's tenantGuid,projectId fields.
CREATE INDEX "I_BrickConnection_tenantGuid_projectId" ON "BMC"."BrickConnection" ("tenantGuid", "projectId")
;

-- Index on the BrickConnection table's tenantGuid,active fields.
CREATE INDEX "I_BrickConnection_tenantGuid_active" ON "BMC"."BrickConnection" ("tenantGuid", "active")
;

-- Index on the BrickConnection table's tenantGuid,deleted fields.
CREATE INDEX "I_BrickConnection_tenantGuid_deleted" ON "BMC"."BrickConnection" ("tenantGuid", "deleted")
;


