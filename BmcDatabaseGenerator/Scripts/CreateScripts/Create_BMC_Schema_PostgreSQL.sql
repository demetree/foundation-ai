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
-- DROP TABLE "BMC"."MarketDataCache"
-- DROP TABLE "BMC"."BrickOwlTransaction"
-- DROP TABLE "BMC"."BrickEconomyTransaction"
-- DROP TABLE "BMC"."BrickLinkTransaction"
-- DROP TABLE "BMC"."BrickOwlUserLink"
-- DROP TABLE "BMC"."BrickEconomyUserLink"
-- DROP TABLE "BMC"."BrickLinkUserLink"
-- DROP TABLE "BMC"."PendingRegistration"
-- DROP TABLE "BMC"."ApiRequestLog"
-- DROP TABLE "BMC"."ApiKey"
-- DROP TABLE "BMC"."PlatformAnnouncement"
-- DROP TABLE "BMC"."ModerationAction"
-- DROP TABLE "BMC"."ContentReport"
-- DROP TABLE "BMC"."ContentReportReason"
-- DROP TABLE "BMC"."BuildChallengeEntry"
-- DROP TABLE "BMC"."BuildChallengeChangeHistory"
-- DROP TABLE "BMC"."BuildChallenge"
-- DROP TABLE "BMC"."UserBadgeAssignment"
-- DROP TABLE "BMC"."UserBadge"
-- DROP TABLE "BMC"."UserAchievement"
-- DROP TABLE "BMC"."Achievement"
-- DROP TABLE "BMC"."AchievementCategory"
-- DROP TABLE "BMC"."SharedInstructionChangeHistory"
-- DROP TABLE "BMC"."SharedInstruction"
-- DROP TABLE "BMC"."MocFavourite"
-- DROP TABLE "BMC"."MocComment"
-- DROP TABLE "BMC"."MocLike"
-- DROP TABLE "BMC"."MocCollaborator"
-- DROP TABLE "BMC"."MocFork"
-- DROP TABLE "BMC"."PublishedMocImage"
-- DROP TABLE "BMC"."MocVersion"
-- DROP TABLE "BMC"."PublishedMocChangeHistory"
-- DROP TABLE "BMC"."PublishedMoc"
-- DROP TABLE "BMC"."ActivityEvent"
-- DROP TABLE "BMC"."ActivityEventType"
-- DROP TABLE "BMC"."UserFollow"
-- DROP TABLE "BMC"."UserProfileStat"
-- DROP TABLE "BMC"."UserSetOwnership"
-- DROP TABLE "BMC"."UserProfilePreferredTheme"
-- DROP TABLE "BMC"."UserProfileLink"
-- DROP TABLE "BMC"."UserProfileLinkType"
-- DROP TABLE "BMC"."UserProfileChangeHistory"
-- DROP TABLE "BMC"."UserProfile"
-- DROP TABLE "BMC"."ProjectExport"
-- DROP TABLE "BMC"."ExportFormat"
-- DROP TABLE "BMC"."ProjectRender"
-- DROP TABLE "BMC"."RenderPreset"
-- DROP TABLE "BMC"."BuildStepAnnotation"
-- DROP TABLE "BMC"."BuildStepAnnotationType"
-- DROP TABLE "BMC"."BuildStepPart"
-- DROP TABLE "BMC"."BuildManualStep"
-- DROP TABLE "BMC"."BuildManualPage"
-- DROP TABLE "BMC"."BuildManualChangeHistory"
-- DROP TABLE "BMC"."BuildManual"
-- DROP TABLE "BMC"."UserLostPart"
-- DROP TABLE "BMC"."UserSetListItem"
-- DROP TABLE "BMC"."UserSetListChangeHistory"
-- DROP TABLE "BMC"."UserSetList"
-- DROP TABLE "BMC"."UserPartListItem"
-- DROP TABLE "BMC"."UserPartListChangeHistory"
-- DROP TABLE "BMC"."UserPartList"
-- DROP TABLE "BMC"."BrickSetSetReview"
-- DROP TABLE "BMC"."BrickSetTransaction"
-- DROP TABLE "BMC"."BrickSetUserLink"
-- DROP TABLE "BMC"."RebrickableSyncQueue"
-- DROP TABLE "BMC"."RebrickableTransaction"
-- DROP TABLE "BMC"."RebrickableUserLink"
-- DROP TABLE "BMC"."UserCollectionSetImport"
-- DROP TABLE "BMC"."UserWishlistItem"
-- DROP TABLE "BMC"."UserCollectionPart"
-- DROP TABLE "BMC"."UserCollectionChangeHistory"
-- DROP TABLE "BMC"."UserCollection"
-- DROP TABLE "BMC"."LegoSetSubset"
-- DROP TABLE "BMC"."LegoSetMinifig"
-- DROP TABLE "BMC"."LegoMinifig"
-- DROP TABLE "BMC"."BrickElement"
-- DROP TABLE "BMC"."BrickPartRelationship"
-- DROP TABLE "BMC"."LegoSetPart"
-- DROP TABLE "BMC"."LegoSet"
-- DROP TABLE "BMC"."LegoTheme"
-- DROP TABLE "BMC"."ModelStepPart"
-- DROP TABLE "BMC"."ModelBuildStep"
-- DROP TABLE "BMC"."ModelSubFile"
-- DROP TABLE "BMC"."ModelDocumentChangeHistory"
-- DROP TABLE "BMC"."ModelDocument"
-- DROP TABLE "BMC"."ProjectReferenceImage"
-- DROP TABLE "BMC"."ProjectCameraPreset"
-- DROP TABLE "BMC"."ProjectTagAssignment"
-- DROP TABLE "BMC"."ProjectTag"
-- DROP TABLE "BMC"."SubmodelInstance"
-- DROP TABLE "BMC"."SubmodelPlacedBrick"
-- DROP TABLE "BMC"."SubmodelChangeHistory"
-- DROP TABLE "BMC"."Submodel"
-- DROP TABLE "BMC"."BrickConnection"
-- DROP TABLE "BMC"."PlacedBrickChangeHistory"
-- DROP TABLE "BMC"."PlacedBrick"
-- DROP TABLE "BMC"."ProjectChangeHistory"
-- DROP TABLE "BMC"."Project"
-- DROP TABLE "BMC"."PartSubFileReference"
-- DROP TABLE "BMC"."BrickPartColour"
-- DROP TABLE "BMC"."BrickPartConnector"
-- DROP TABLE "BMC"."BrickPartChangeHistory"
-- DROP TABLE "BMC"."BrickPart"
-- DROP TABLE "BMC"."PartType"
-- DROP TABLE "BMC"."BrickColour"
-- DROP TABLE "BMC"."ColourFinish"
-- DROP TABLE "BMC"."ConnectorTypeCompatibility"
-- DROP TABLE "BMC"."ConnectorType"
-- DROP TABLE "BMC"."BrickCategory"

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON "MarketDataCache" DISABLE
-- ALTER INDEX ALL ON "BrickOwlTransaction" DISABLE
-- ALTER INDEX ALL ON "BrickEconomyTransaction" DISABLE
-- ALTER INDEX ALL ON "BrickLinkTransaction" DISABLE
-- ALTER INDEX ALL ON "BrickOwlUserLink" DISABLE
-- ALTER INDEX ALL ON "BrickEconomyUserLink" DISABLE
-- ALTER INDEX ALL ON "BrickLinkUserLink" DISABLE
-- ALTER INDEX ALL ON "PendingRegistration" DISABLE
-- ALTER INDEX ALL ON "ApiRequestLog" DISABLE
-- ALTER INDEX ALL ON "ApiKey" DISABLE
-- ALTER INDEX ALL ON "PlatformAnnouncement" DISABLE
-- ALTER INDEX ALL ON "ModerationAction" DISABLE
-- ALTER INDEX ALL ON "ContentReport" DISABLE
-- ALTER INDEX ALL ON "ContentReportReason" DISABLE
-- ALTER INDEX ALL ON "BuildChallengeEntry" DISABLE
-- ALTER INDEX ALL ON "BuildChallengeChangeHistory" DISABLE
-- ALTER INDEX ALL ON "BuildChallenge" DISABLE
-- ALTER INDEX ALL ON "UserBadgeAssignment" DISABLE
-- ALTER INDEX ALL ON "UserBadge" DISABLE
-- ALTER INDEX ALL ON "UserAchievement" DISABLE
-- ALTER INDEX ALL ON "Achievement" DISABLE
-- ALTER INDEX ALL ON "AchievementCategory" DISABLE
-- ALTER INDEX ALL ON "SharedInstructionChangeHistory" DISABLE
-- ALTER INDEX ALL ON "SharedInstruction" DISABLE
-- ALTER INDEX ALL ON "MocFavourite" DISABLE
-- ALTER INDEX ALL ON "MocComment" DISABLE
-- ALTER INDEX ALL ON "MocLike" DISABLE
-- ALTER INDEX ALL ON "MocCollaborator" DISABLE
-- ALTER INDEX ALL ON "MocFork" DISABLE
-- ALTER INDEX ALL ON "PublishedMocImage" DISABLE
-- ALTER INDEX ALL ON "MocVersion" DISABLE
-- ALTER INDEX ALL ON "PublishedMocChangeHistory" DISABLE
-- ALTER INDEX ALL ON "PublishedMoc" DISABLE
-- ALTER INDEX ALL ON "ActivityEvent" DISABLE
-- ALTER INDEX ALL ON "ActivityEventType" DISABLE
-- ALTER INDEX ALL ON "UserFollow" DISABLE
-- ALTER INDEX ALL ON "UserProfileStat" DISABLE
-- ALTER INDEX ALL ON "UserSetOwnership" DISABLE
-- ALTER INDEX ALL ON "UserProfilePreferredTheme" DISABLE
-- ALTER INDEX ALL ON "UserProfileLink" DISABLE
-- ALTER INDEX ALL ON "UserProfileLinkType" DISABLE
-- ALTER INDEX ALL ON "UserProfileChangeHistory" DISABLE
-- ALTER INDEX ALL ON "UserProfile" DISABLE
-- ALTER INDEX ALL ON "ProjectExport" DISABLE
-- ALTER INDEX ALL ON "ExportFormat" DISABLE
-- ALTER INDEX ALL ON "ProjectRender" DISABLE
-- ALTER INDEX ALL ON "RenderPreset" DISABLE
-- ALTER INDEX ALL ON "BuildStepAnnotation" DISABLE
-- ALTER INDEX ALL ON "BuildStepAnnotationType" DISABLE
-- ALTER INDEX ALL ON "BuildStepPart" DISABLE
-- ALTER INDEX ALL ON "BuildManualStep" DISABLE
-- ALTER INDEX ALL ON "BuildManualPage" DISABLE
-- ALTER INDEX ALL ON "BuildManualChangeHistory" DISABLE
-- ALTER INDEX ALL ON "BuildManual" DISABLE
-- ALTER INDEX ALL ON "UserLostPart" DISABLE
-- ALTER INDEX ALL ON "UserSetListItem" DISABLE
-- ALTER INDEX ALL ON "UserSetListChangeHistory" DISABLE
-- ALTER INDEX ALL ON "UserSetList" DISABLE
-- ALTER INDEX ALL ON "UserPartListItem" DISABLE
-- ALTER INDEX ALL ON "UserPartListChangeHistory" DISABLE
-- ALTER INDEX ALL ON "UserPartList" DISABLE
-- ALTER INDEX ALL ON "BrickSetSetReview" DISABLE
-- ALTER INDEX ALL ON "BrickSetTransaction" DISABLE
-- ALTER INDEX ALL ON "BrickSetUserLink" DISABLE
-- ALTER INDEX ALL ON "RebrickableSyncQueue" DISABLE
-- ALTER INDEX ALL ON "RebrickableTransaction" DISABLE
-- ALTER INDEX ALL ON "RebrickableUserLink" DISABLE
-- ALTER INDEX ALL ON "UserCollectionSetImport" DISABLE
-- ALTER INDEX ALL ON "UserWishlistItem" DISABLE
-- ALTER INDEX ALL ON "UserCollectionPart" DISABLE
-- ALTER INDEX ALL ON "UserCollectionChangeHistory" DISABLE
-- ALTER INDEX ALL ON "UserCollection" DISABLE
-- ALTER INDEX ALL ON "LegoSetSubset" DISABLE
-- ALTER INDEX ALL ON "LegoSetMinifig" DISABLE
-- ALTER INDEX ALL ON "LegoMinifig" DISABLE
-- ALTER INDEX ALL ON "BrickElement" DISABLE
-- ALTER INDEX ALL ON "BrickPartRelationship" DISABLE
-- ALTER INDEX ALL ON "LegoSetPart" DISABLE
-- ALTER INDEX ALL ON "LegoSet" DISABLE
-- ALTER INDEX ALL ON "LegoTheme" DISABLE
-- ALTER INDEX ALL ON "ModelStepPart" DISABLE
-- ALTER INDEX ALL ON "ModelBuildStep" DISABLE
-- ALTER INDEX ALL ON "ModelSubFile" DISABLE
-- ALTER INDEX ALL ON "ModelDocumentChangeHistory" DISABLE
-- ALTER INDEX ALL ON "ModelDocument" DISABLE
-- ALTER INDEX ALL ON "ProjectReferenceImage" DISABLE
-- ALTER INDEX ALL ON "ProjectCameraPreset" DISABLE
-- ALTER INDEX ALL ON "ProjectTagAssignment" DISABLE
-- ALTER INDEX ALL ON "ProjectTag" DISABLE
-- ALTER INDEX ALL ON "SubmodelInstance" DISABLE
-- ALTER INDEX ALL ON "SubmodelPlacedBrick" DISABLE
-- ALTER INDEX ALL ON "SubmodelChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Submodel" DISABLE
-- ALTER INDEX ALL ON "BrickConnection" DISABLE
-- ALTER INDEX ALL ON "PlacedBrickChangeHistory" DISABLE
-- ALTER INDEX ALL ON "PlacedBrick" DISABLE
-- ALTER INDEX ALL ON "ProjectChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Project" DISABLE
-- ALTER INDEX ALL ON "PartSubFileReference" DISABLE
-- ALTER INDEX ALL ON "BrickPartColour" DISABLE
-- ALTER INDEX ALL ON "BrickPartConnector" DISABLE
-- ALTER INDEX ALL ON "BrickPartChangeHistory" DISABLE
-- ALTER INDEX ALL ON "BrickPart" DISABLE
-- ALTER INDEX ALL ON "PartType" DISABLE
-- ALTER INDEX ALL ON "BrickColour" DISABLE
-- ALTER INDEX ALL ON "ColourFinish" DISABLE
-- ALTER INDEX ALL ON "ConnectorTypeCompatibility" DISABLE
-- ALTER INDEX ALL ON "ConnectorType" DISABLE
-- ALTER INDEX ALL ON "BrickCategory" DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON "MarketDataCache" REBUILD
-- ALTER INDEX ALL ON "BrickOwlTransaction" REBUILD
-- ALTER INDEX ALL ON "BrickEconomyTransaction" REBUILD
-- ALTER INDEX ALL ON "BrickLinkTransaction" REBUILD
-- ALTER INDEX ALL ON "BrickOwlUserLink" REBUILD
-- ALTER INDEX ALL ON "BrickEconomyUserLink" REBUILD
-- ALTER INDEX ALL ON "BrickLinkUserLink" REBUILD
-- ALTER INDEX ALL ON "PendingRegistration" REBUILD
-- ALTER INDEX ALL ON "ApiRequestLog" REBUILD
-- ALTER INDEX ALL ON "ApiKey" REBUILD
-- ALTER INDEX ALL ON "PlatformAnnouncement" REBUILD
-- ALTER INDEX ALL ON "ModerationAction" REBUILD
-- ALTER INDEX ALL ON "ContentReport" REBUILD
-- ALTER INDEX ALL ON "ContentReportReason" REBUILD
-- ALTER INDEX ALL ON "BuildChallengeEntry" REBUILD
-- ALTER INDEX ALL ON "BuildChallengeChangeHistory" REBUILD
-- ALTER INDEX ALL ON "BuildChallenge" REBUILD
-- ALTER INDEX ALL ON "UserBadgeAssignment" REBUILD
-- ALTER INDEX ALL ON "UserBadge" REBUILD
-- ALTER INDEX ALL ON "UserAchievement" REBUILD
-- ALTER INDEX ALL ON "Achievement" REBUILD
-- ALTER INDEX ALL ON "AchievementCategory" REBUILD
-- ALTER INDEX ALL ON "SharedInstructionChangeHistory" REBUILD
-- ALTER INDEX ALL ON "SharedInstruction" REBUILD
-- ALTER INDEX ALL ON "MocFavourite" REBUILD
-- ALTER INDEX ALL ON "MocComment" REBUILD
-- ALTER INDEX ALL ON "MocLike" REBUILD
-- ALTER INDEX ALL ON "MocCollaborator" REBUILD
-- ALTER INDEX ALL ON "MocFork" REBUILD
-- ALTER INDEX ALL ON "PublishedMocImage" REBUILD
-- ALTER INDEX ALL ON "MocVersion" REBUILD
-- ALTER INDEX ALL ON "PublishedMocChangeHistory" REBUILD
-- ALTER INDEX ALL ON "PublishedMoc" REBUILD
-- ALTER INDEX ALL ON "ActivityEvent" REBUILD
-- ALTER INDEX ALL ON "ActivityEventType" REBUILD
-- ALTER INDEX ALL ON "UserFollow" REBUILD
-- ALTER INDEX ALL ON "UserProfileStat" REBUILD
-- ALTER INDEX ALL ON "UserSetOwnership" REBUILD
-- ALTER INDEX ALL ON "UserProfilePreferredTheme" REBUILD
-- ALTER INDEX ALL ON "UserProfileLink" REBUILD
-- ALTER INDEX ALL ON "UserProfileLinkType" REBUILD
-- ALTER INDEX ALL ON "UserProfileChangeHistory" REBUILD
-- ALTER INDEX ALL ON "UserProfile" REBUILD
-- ALTER INDEX ALL ON "ProjectExport" REBUILD
-- ALTER INDEX ALL ON "ExportFormat" REBUILD
-- ALTER INDEX ALL ON "ProjectRender" REBUILD
-- ALTER INDEX ALL ON "RenderPreset" REBUILD
-- ALTER INDEX ALL ON "BuildStepAnnotation" REBUILD
-- ALTER INDEX ALL ON "BuildStepAnnotationType" REBUILD
-- ALTER INDEX ALL ON "BuildStepPart" REBUILD
-- ALTER INDEX ALL ON "BuildManualStep" REBUILD
-- ALTER INDEX ALL ON "BuildManualPage" REBUILD
-- ALTER INDEX ALL ON "BuildManualChangeHistory" REBUILD
-- ALTER INDEX ALL ON "BuildManual" REBUILD
-- ALTER INDEX ALL ON "UserLostPart" REBUILD
-- ALTER INDEX ALL ON "UserSetListItem" REBUILD
-- ALTER INDEX ALL ON "UserSetListChangeHistory" REBUILD
-- ALTER INDEX ALL ON "UserSetList" REBUILD
-- ALTER INDEX ALL ON "UserPartListItem" REBUILD
-- ALTER INDEX ALL ON "UserPartListChangeHistory" REBUILD
-- ALTER INDEX ALL ON "UserPartList" REBUILD
-- ALTER INDEX ALL ON "BrickSetSetReview" REBUILD
-- ALTER INDEX ALL ON "BrickSetTransaction" REBUILD
-- ALTER INDEX ALL ON "BrickSetUserLink" REBUILD
-- ALTER INDEX ALL ON "RebrickableSyncQueue" REBUILD
-- ALTER INDEX ALL ON "RebrickableTransaction" REBUILD
-- ALTER INDEX ALL ON "RebrickableUserLink" REBUILD
-- ALTER INDEX ALL ON "UserCollectionSetImport" REBUILD
-- ALTER INDEX ALL ON "UserWishlistItem" REBUILD
-- ALTER INDEX ALL ON "UserCollectionPart" REBUILD
-- ALTER INDEX ALL ON "UserCollectionChangeHistory" REBUILD
-- ALTER INDEX ALL ON "UserCollection" REBUILD
-- ALTER INDEX ALL ON "LegoSetSubset" REBUILD
-- ALTER INDEX ALL ON "LegoSetMinifig" REBUILD
-- ALTER INDEX ALL ON "LegoMinifig" REBUILD
-- ALTER INDEX ALL ON "BrickElement" REBUILD
-- ALTER INDEX ALL ON "BrickPartRelationship" REBUILD
-- ALTER INDEX ALL ON "LegoSetPart" REBUILD
-- ALTER INDEX ALL ON "LegoSet" REBUILD
-- ALTER INDEX ALL ON "LegoTheme" REBUILD
-- ALTER INDEX ALL ON "ModelStepPart" REBUILD
-- ALTER INDEX ALL ON "ModelBuildStep" REBUILD
-- ALTER INDEX ALL ON "ModelSubFile" REBUILD
-- ALTER INDEX ALL ON "ModelDocumentChangeHistory" REBUILD
-- ALTER INDEX ALL ON "ModelDocument" REBUILD
-- ALTER INDEX ALL ON "ProjectReferenceImage" REBUILD
-- ALTER INDEX ALL ON "ProjectCameraPreset" REBUILD
-- ALTER INDEX ALL ON "ProjectTagAssignment" REBUILD
-- ALTER INDEX ALL ON "ProjectTag" REBUILD
-- ALTER INDEX ALL ON "SubmodelInstance" REBUILD
-- ALTER INDEX ALL ON "SubmodelPlacedBrick" REBUILD
-- ALTER INDEX ALL ON "SubmodelChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Submodel" REBUILD
-- ALTER INDEX ALL ON "BrickConnection" REBUILD
-- ALTER INDEX ALL ON "PlacedBrickChangeHistory" REBUILD
-- ALTER INDEX ALL ON "PlacedBrick" REBUILD
-- ALTER INDEX ALL ON "ProjectChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Project" REBUILD
-- ALTER INDEX ALL ON "PartSubFileReference" REBUILD
-- ALTER INDEX ALL ON "BrickPartColour" REBUILD
-- ALTER INDEX ALL ON "BrickPartConnector" REBUILD
-- ALTER INDEX ALL ON "BrickPartChangeHistory" REBUILD
-- ALTER INDEX ALL ON "BrickPart" REBUILD
-- ALTER INDEX ALL ON "PartType" REBUILD
-- ALTER INDEX ALL ON "BrickColour" REBUILD
-- ALTER INDEX ALL ON "ColourFinish" REBUILD
-- ALTER INDEX ALL ON "ConnectorTypeCompatibility" REBUILD
-- ALTER INDEX ALL ON "ConnectorType" REBUILD
-- ALTER INDEX ALL ON "BrickCategory" REBUILD

-- Master list of part categories (Beam, Plate, Gear, Axle, Motor, Pneumatic, etc.)
CREATE TABLE "BMC"."BrickCategory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"rebrickablePartCategoryId" INT NULL,		-- Rebrickable part_cat_id for cross-referencing during bulk import
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
	"minAngleDegrees" REAL NULL,		-- Minimum angle of rotation for hinge-type connectors (null = no minimum)
	"maxAngleDegrees" REAL NULL,		-- Maximum angle of rotation for hinge-type connectors (null = no maximum)
	"snapIncrementDegrees" REAL NULL,		-- Snap increment for click hinges (e.g. 22.5 degrees, null = continuous)
	"clutchForceNewtons" REAL NULL,		-- Force needed to disconnect this connector type (null = unknown)
	"maleOrFemale" VARCHAR(10) NULL,		-- Pairing role: Male, Female, or Both (for compatibility matching logic)
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


-- Defines which connector types can physically connect to each other (e.g. Stud↔AntiStud, Pin↔PinHole).
CREATE TABLE "BMC"."ConnectorTypeCompatibility"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"maleConnectorTypeId" INT NOT NULL,		-- The male/inserting connector type
	"femaleConnectorTypeId" INT NOT NULL,		-- The female/receiving connector type
	"connectionStrength" VARCHAR(50) NOT NULL,		-- Connection strength: Tight, Friction, Free
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "maleConnectorTypeId" FOREIGN KEY ("maleConnectorTypeId") REFERENCES "BMC"."ConnectorType"("id"),		-- Foreign key to the ConnectorType table.
	CONSTRAINT "femaleConnectorTypeId" FOREIGN KEY ("femaleConnectorTypeId") REFERENCES "BMC"."ConnectorType"("id"),		-- Foreign key to the ConnectorType table.
	CONSTRAINT "UC_ConnectorTypeCompatibility_maleConnectorTypeId_femaleConnectorTypeId" UNIQUE ( "maleConnectorTypeId", "femaleConnectorTypeId") 		-- Uniqueness enforced on the ConnectorTypeCompatibility table's maleConnectorTypeId and femaleConnectorTypeId fields.
);
-- Index on the ConnectorTypeCompatibility table's maleConnectorTypeId field.
CREATE INDEX "I_ConnectorTypeCompatibility_maleConnectorTypeId" ON "BMC"."ConnectorTypeCompatibility" ("maleConnectorTypeId")
;

-- Index on the ConnectorTypeCompatibility table's femaleConnectorTypeId field.
CREATE INDEX "I_ConnectorTypeCompatibility_femaleConnectorTypeId" ON "BMC"."ConnectorTypeCompatibility" ("femaleConnectorTypeId")
;

-- Index on the ConnectorTypeCompatibility table's active field.
CREATE INDEX "I_ConnectorTypeCompatibility_active" ON "BMC"."ConnectorTypeCompatibility" ("active")
;

-- Index on the ConnectorTypeCompatibility table's deleted field.
CREATE INDEX "I_ConnectorTypeCompatibility_deleted" ON "BMC"."ConnectorTypeCompatibility" ("deleted")
;

INSERT INTO "BMC"."ConnectorTypeCompatibility" ( "connectionStrength", "maleConnectorTypeId", "femaleConnectorTypeId", "objectGuid" ) VALUES  ( 'Tight', ( SELECT id FROM "ConnectorType" WHERE "name" = 'Stud' LIMIT 1), ( SELECT id FROM "ConnectorType" WHERE "name" = 'AntiStud' LIMIT 1), 'cc100001-0001-4000-8000-000000000001' );

INSERT INTO "BMC"."ConnectorTypeCompatibility" ( "connectionStrength", "maleConnectorTypeId", "femaleConnectorTypeId", "objectGuid" ) VALUES  ( 'Friction', ( SELECT id FROM "ConnectorType" WHERE "name" = 'Pin' LIMIT 1), ( SELECT id FROM "ConnectorType" WHERE "name" = 'PinHole' LIMIT 1), 'cc100001-0001-4000-8000-000000000002' );

INSERT INTO "BMC"."ConnectorTypeCompatibility" ( "connectionStrength", "maleConnectorTypeId", "femaleConnectorTypeId", "objectGuid" ) VALUES  ( 'Tight', ( SELECT id FROM "ConnectorType" WHERE "name" = 'AxleEnd' LIMIT 1), ( SELECT id FROM "ConnectorType" WHERE "name" = 'AxleHole' LIMIT 1), 'cc100001-0001-4000-8000-000000000003' );

INSERT INTO "BMC"."ConnectorTypeCompatibility" ( "connectionStrength", "maleConnectorTypeId", "femaleConnectorTypeId", "objectGuid" ) VALUES  ( 'Friction', ( SELECT id FROM "ConnectorType" WHERE "name" = 'BallJoint' LIMIT 1), ( SELECT id FROM "ConnectorType" WHERE "name" = 'BallJointSocket' LIMIT 1), 'cc100001-0001-4000-8000-000000000004' );


-- Lookup table of material finish types that define how a colour is rendered (e.g. Solid, Chrome, Rubber).
CREATE TABLE "BMC"."ColourFinish"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"requiresEnvironmentMap" BOOLEAN NOT NULL DEFAULT false,		-- Whether this finish needs environment mapping for reflections (Chrome, Metal)
	"isMatte" BOOLEAN NOT NULL DEFAULT false,		-- Whether this finish has a matte/non-glossy appearance (Rubber)
	"defaultAlpha" INT NULL,		-- Default alpha for this finish type, null = use colour-specific alpha
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the ColourFinish table's name field.
CREATE INDEX "I_ColourFinish_name" ON "BMC"."ColourFinish" ("name")
;

-- Index on the ColourFinish table's active field.
CREATE INDEX "I_ColourFinish_active" ON "BMC"."ColourFinish" ("active")
;

-- Index on the ColourFinish table's deleted field.
CREATE INDEX "I_ColourFinish_deleted" ON "BMC"."ColourFinish" ("deleted")
;

INSERT INTO "BMC"."ColourFinish" ( "name", "description", "requiresEnvironmentMap", "isMatte", "sequence", "objectGuid" ) VALUES  ( 'Solid', 'Standard opaque plastic finish', false, false, 1, 'cf100001-0001-4000-8000-000000000001' );

INSERT INTO "BMC"."ColourFinish" ( "name", "description", "requiresEnvironmentMap", "isMatte", "defaultAlpha", "sequence", "objectGuid" ) VALUES  ( 'Transparent', 'See-through plastic finish', false, false, 128, 2, 'cf100001-0001-4000-8000-000000000002' );

INSERT INTO "BMC"."ColourFinish" ( "name", "description", "requiresEnvironmentMap", "isMatte", "sequence", "objectGuid" ) VALUES  ( 'Chrome', 'Highly reflective chrome-plated metal finish', false, false, 3, 'cf100001-0001-4000-8000-000000000003' );

INSERT INTO "BMC"."ColourFinish" ( "name", "description", "requiresEnvironmentMap", "isMatte", "sequence", "objectGuid" ) VALUES  ( 'Pearlescent', 'Iridescent pearl-like plastic finish', false, false, 4, 'cf100001-0001-4000-8000-000000000004' );

INSERT INTO "BMC"."ColourFinish" ( "name", "description", "requiresEnvironmentMap", "isMatte", "sequence", "objectGuid" ) VALUES  ( 'Metal', 'Metallic paint or lacquer finish', false, false, 5, 'cf100001-0001-4000-8000-000000000005' );

INSERT INTO "BMC"."ColourFinish" ( "name", "description", "requiresEnvironmentMap", "isMatte", "sequence", "objectGuid" ) VALUES  ( 'Rubber', 'Matte rubber or soft-touch finish', false, false, 6, 'cf100001-0001-4000-8000-000000000006' );

INSERT INTO "BMC"."ColourFinish" ( "name", "description", "requiresEnvironmentMap", "isMatte", "defaultAlpha", "sequence", "objectGuid" ) VALUES  ( 'Glitter', 'Transparent plastic with embedded glitter particles', false, false, 128, 7, 'cf100001-0001-4000-8000-000000000007' );

INSERT INTO "BMC"."ColourFinish" ( "name", "description", "requiresEnvironmentMap", "isMatte", "sequence", "objectGuid" ) VALUES  ( 'Speckle', 'Solid plastic with embedded speckle particles', false, false, 8, 'cf100001-0001-4000-8000-000000000008' );

INSERT INTO "BMC"."ColourFinish" ( "name", "description", "requiresEnvironmentMap", "isMatte", "defaultAlpha", "sequence", "objectGuid" ) VALUES  ( 'Milky', 'Semi-translucent milky or glow-in-the-dark finish', false, false, 240, 9, 'cf100001-0001-4000-8000-000000000009' );

INSERT INTO "BMC"."ColourFinish" ( "name", "description", "requiresEnvironmentMap", "isMatte", "sequence", "objectGuid" ) VALUES  ( 'Fabric', 'Fabric or cloth material finish for flags, capes, and similar elements', false, false, 10, 'cf100001-0001-4000-8000-000000000010' );


-- Colour definitions for brick parts. Compatible with the LDraw colour standard.
CREATE TABLE "BMC"."BrickColour"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"rebrickableColorId" INT NOT NULL,		-- Rebrickable color ID — primary lookup key for API sync
	"ldrawColourCode" INT NULL,		-- LDraw standard colour code number (nullable — some Rebrickable colors may not map to LDraw)
	"bricklinkColorId" INT NULL,		-- BrickLink color ID for cross-referencing
	"brickowlColorId" INT NULL,		-- BrickOwl color ID for cross-referencing
	"hexRgb" VARCHAR(10) NULL,		-- Hex RGB colour value (e.g. #FF0000)
	"hexEdgeColour" VARCHAR(10) NULL,		-- LDraw edge/contrast colour hex value for wireframe and outline rendering
	"alpha" INT NULL,		-- Alpha transparency value (0-255, 255 = fully opaque)
	"isTransparent" BOOLEAN NOT NULL DEFAULT false,		-- Whether this colour is transparent (convenience flag derived from alpha)
	"isMetallic" BOOLEAN NOT NULL DEFAULT false,		-- Whether this colour has a metallic finish (convenience flag)
	"colourFinishId" INT NOT NULL,		-- Material finish type — FK to ColourFinish lookup table
	"luminance" INT NULL,		-- Glow brightness (0-255) for glow-in-the-dark colours. Null for non-glowing.
	"legoColourId" INT NULL,		-- Official LEGO colour number for cross-referencing with LEGO catalogues
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "colourFinishId" FOREIGN KEY ("colourFinishId") REFERENCES "BMC"."ColourFinish"("id"),		-- Foreign key to the ColourFinish table.
	CONSTRAINT "UC_BrickColour_rebrickableColorId" UNIQUE ( "rebrickableColorId") 		-- Uniqueness enforced on the BrickColour table's rebrickableColorId field.
);
-- Index on the BrickColour table's name field.
CREATE INDEX "I_BrickColour_name" ON "BMC"."BrickColour" ("name")
;

-- Index on the BrickColour table's colourFinishId field.
CREATE INDEX "I_BrickColour_colourFinishId" ON "BMC"."BrickColour" ("colourFinishId")
;

-- Index on the BrickColour table's active field.
CREATE INDEX "I_BrickColour_active" ON "BMC"."BrickColour" ("active")
;

-- Index on the BrickColour table's deleted field.
CREATE INDEX "I_BrickColour_deleted" ON "BMC"."BrickColour" ("deleted")
;


-- Lookup table of LDraw part classification types (Part, Subpart, Primitive, etc.).
CREATE TABLE "BMC"."PartType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"isUserVisible" BOOLEAN NOT NULL DEFAULT true,		-- Whether parts of this type should appear in the user-facing part picker
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the PartType table's name field.
CREATE INDEX "I_PartType_name" ON "BMC"."PartType" ("name")
;

-- Index on the PartType table's active field.
CREATE INDEX "I_PartType_active" ON "BMC"."PartType" ("active")
;

-- Index on the PartType table's deleted field.
CREATE INDEX "I_PartType_deleted" ON "BMC"."PartType" ("deleted")
;

INSERT INTO "BMC"."PartType" ( "name", "description", "isUserVisible", "sequence", "objectGuid" ) VALUES  ( 'Part', 'A complete, standalone part (e.g. Brick 2x4)', false, 1, 'df6fb298-9f61-41ce-aad2-37c00bc14efd' );

INSERT INTO "BMC"."PartType" ( "name", "description", "isUserVisible", "sequence", "objectGuid" ) VALUES  ( 'Subpart', 'A reusable component used internally by other parts', false, 2, '71ed658f-8695-44df-9448-669348bcfab4' );

INSERT INTO "BMC"."PartType" ( "name", "description", "isUserVisible", "sequence", "objectGuid" ) VALUES  ( 'Primitive', 'A low-level geometric primitive (cylinder, stud shape)', false, 3, 'cae03dfa-930b-47e3-acd0-83241eaae69d' );

INSERT INTO "BMC"."PartType" ( "name", "description", "isUserVisible", "sequence", "objectGuid" ) VALUES  ( 'Shortcut', 'A convenience combination of multiple parts (e.g. hinge assembly)', false, 4, 'a800b3c0-e7d1-46f3-830d-f2c93f7f8e4d' );

INSERT INTO "BMC"."PartType" ( "name", "description", "isUserVisible", "sequence", "objectGuid" ) VALUES  ( 'Alias', 'An alternate ID that maps to another part', false, 5, '9c5c8f5c-6397-4233-b360-0292adc30304' );


-- Individual brick part definitions. Each row represents a unique part shape (independent of colour).
CREATE TABLE "BMC"."BrickPart"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"rebrickablePartNum" VARCHAR(100) NOT NULL,		-- Rebrickable part_num — primary lookup key for the parts catalog
	"rebrickablePartUrl" VARCHAR(250) NULL,		-- URL to part page on Rebrickable
	"rebrickableImgUrl" VARCHAR(250) NULL,		-- URL to part image on Rebrickable
	"ldrawPartId" VARCHAR(100) NULL,		-- LDraw part ID (e.g. 3001, 32523) — nullable, some Rebrickable parts have no LDraw file
	"bricklinkId" VARCHAR(100) NULL,		-- BrickLink part ID for cross-referencing
	"brickowlId" VARCHAR(100) NULL,		-- BrickOwl part ID for cross-referencing
	"legoDesignId" VARCHAR(100) NULL,		-- Official LEGO design number for cross-referencing
	"ldrawTitle" VARCHAR(250) NULL,		-- Raw title from the LDraw .dat file (e.g. 'Brick  2 x  4', 'Technic Beam  3')
	"ldrawCategory" VARCHAR(100) NULL,		-- Part category from LDraw !CATEGORY meta or inferred from title first word
	"partTypeId" INT NOT NULL,		-- LDraw part classification — FK to PartType lookup table
	"keywords" TEXT NULL,		-- Comma-separated keywords from LDraw !KEYWORDS meta lines for search
	"author" VARCHAR(100) NULL,		-- Part author from the LDraw Author: header line
	"brickCategoryId" INT NOT NULL,		-- The category this part belongs to
	"widthLdu" REAL NULL,		-- Part width in LDraw units (null if not yet computed)
	"heightLdu" REAL NULL,		-- Part height in LDraw units (null if not yet computed)
	"depthLdu" REAL NULL,		-- Part depth in LDraw units (null if not yet computed)
	"massGrams" REAL NULL,		-- Part mass in grams (for physics simulation, null if unknown)
	"momentOfInertiaX" REAL NULL,		-- Rotational inertia about X axis (kg·m², null if unknown)
	"momentOfInertiaY" REAL NULL,		-- Rotational inertia about Y axis (kg·m², null if unknown)
	"momentOfInertiaZ" REAL NULL,		-- Rotational inertia about Z axis (kg·m², null if unknown)
	"frictionCoefficient" REAL NULL,		-- Surface friction coefficient for physics (null if unknown)
	"materialType" VARCHAR(50) NULL,		-- Material type: ABS, Rubber, Metal, Fabric, etc. (null if unknown)
	"centerOfMassX" REAL NULL,		-- Center of mass X offset from part origin in LDU (null if unknown)
	"centerOfMassY" REAL NULL,		-- Center of mass Y offset from part origin in LDU (null if unknown)
	"centerOfMassZ" REAL NULL,		-- Center of mass Z offset from part origin in LDU (null if unknown)
	"geometryFileName" VARCHAR(250) NULL,		-- Part of the binary data field setup
	"geometrySize" BIGINT NULL,		-- Part of the binary data field setup
	"geometryData" BYTEA NULL,		-- Part of the binary data field setup
	"geometryMimeType" VARCHAR(100) NULL,		-- Part of the binary data field setup
	"geometryFileFormat" VARCHAR(50) NULL,		-- Format of stored geometry: LDraw, ProcessedBinary, etc.
	"geometryOriginalFileName" VARCHAR(250) NULL,		-- Original LDraw filename for reference (e.g. '3001.dat')
	"boundingBoxMinX" REAL NULL,		-- Axis-aligned bounding box minimum X in LDU
	"boundingBoxMinY" REAL NULL,		-- Axis-aligned bounding box minimum Y in LDU
	"boundingBoxMinZ" REAL NULL,		-- Axis-aligned bounding box minimum Z in LDU
	"boundingBoxMaxX" REAL NULL,		-- Axis-aligned bounding box maximum X in LDU
	"boundingBoxMaxY" REAL NULL,		-- Axis-aligned bounding box maximum Y in LDU
	"boundingBoxMaxZ" REAL NULL,		-- Axis-aligned bounding box maximum Z in LDU
	"subFileCount" INT NULL,		-- Number of LDraw sub-file references in this part (for instancing)
	"polygonCount" INT NULL,		-- Total polygon count for LOD decisions and performance budgets
	"toothCount" INT NULL,		-- For gears: number of teeth. Null for non-gear parts.
	"gearRatio" REAL NULL,		-- For gears: effective gear ratio relative to a base gear. Null for non-gear parts.
	"lastModifiedDate" TIMESTAMP NULL,		-- Last modification date for incremental sync with Rebrickable
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "partTypeId" FOREIGN KEY ("partTypeId") REFERENCES "BMC"."PartType"("id"),		-- Foreign key to the PartType table.
	CONSTRAINT "brickCategoryId" FOREIGN KEY ("brickCategoryId") REFERENCES "BMC"."BrickCategory"("id"),		-- Foreign key to the BrickCategory table.
	CONSTRAINT "UC_BrickPart_rebrickablePartNum" UNIQUE ( "rebrickablePartNum") 		-- Uniqueness enforced on the BrickPart table's rebrickablePartNum field.
);
-- Index on the BrickPart table's name field.
CREATE INDEX "I_BrickPart_name" ON "BMC"."BrickPart" ("name")
;

-- Index on the BrickPart table's partTypeId field.
CREATE INDEX "I_BrickPart_partTypeId" ON "BMC"."BrickPart" ("partTypeId")
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
	"brickPartId" INT NOT NULL,		-- The part this connector belongs to
	"connectorTypeId" INT NOT NULL,		-- The type of connector (Stud, PinHole, AxleHole, etc.)
	"positionX" REAL NULL,		-- X position of connector relative to part origin (LDU)
	"positionY" REAL NULL,		-- Y position of connector relative to part origin (LDU)
	"positionZ" REAL NULL,		-- Z position of connector relative to part origin (LDU)
	"orientationX" REAL NULL,		-- X component of connector direction unit vector
	"orientationY" REAL NULL,		-- Y component of connector direction unit vector
	"orientationZ" REAL NULL,		-- Z component of connector direction unit vector
	"connectorGroupId" INT NULL,		-- Groups connectors that act together (e.g. all studs on top of a 2x4 share a group ID)
	"isAutoExtracted" BOOLEAN NOT NULL DEFAULT false,		-- Whether this connector position was auto-extracted from LDraw geometry analysis
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


-- Models how LDraw parts reference sub-files hierarchically. Crucial for instanced rendering and understanding part decomposition.
CREATE TABLE "BMC"."PartSubFileReference"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"parentBrickPartId" INT NOT NULL,		-- The part containing the sub-file reference
	"referencedBrickPartId" INT NULL,		-- The referenced sub-file as a BrickPart (null if sub-file is not cataloged)
	"referencedFileName" VARCHAR(250) NOT NULL,		-- Original LDraw sub-file filename (e.g. 'stud.dat', 's/3001s01.dat')
	"transformMatrix" VARCHAR(500) NOT NULL,		-- 4x3 transform matrix as space-delimited floats (x y z a b c d e f g h i)
	"colorCode" INT NULL,		-- LDraw color code override (16=inherit parent, null=inherit parent)
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "parentBrickPartId" FOREIGN KEY ("parentBrickPartId") REFERENCES "BMC"."BrickPart"("id"),		-- Foreign key to the BrickPart table.
	CONSTRAINT "referencedBrickPartId" FOREIGN KEY ("referencedBrickPartId") REFERENCES "BMC"."BrickPart"("id")		-- Foreign key to the BrickPart table.
);
-- Index on the PartSubFileReference table's parentBrickPartId field.
CREATE INDEX "I_PartSubFileReference_parentBrickPartId" ON "BMC"."PartSubFileReference" ("parentBrickPartId")
;

-- Index on the PartSubFileReference table's referencedBrickPartId field.
CREATE INDEX "I_PartSubFileReference_referencedBrickPartId" ON "BMC"."PartSubFileReference" ("referencedBrickPartId")
;

-- Index on the PartSubFileReference table's active field.
CREATE INDEX "I_PartSubFileReference_active" ON "BMC"."PartSubFileReference" ("active")
;

-- Index on the PartSubFileReference table's deleted field.
CREATE INDEX "I_PartSubFileReference_deleted" ON "BMC"."PartSubFileReference" ("deleted")
;


-- A user's building project. Contains placed bricks and their connections to form a model.
CREATE TABLE "BMC"."Project"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userId" INT NULL,		-- Cross-database reference to SecurityUser.id — the user who owns this project (nullable for legacy data)
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"notes" TEXT NULL,		-- Free-form notes about the project
	"thumbnailImagePath" VARCHAR(250) NULL,		-- Relative path to project thumbnail image for listings
	"thumbnailData" BYTEA NULL,		-- PNG thumbnail image data from .io import or renders
	"partCount" INT NULL,		-- Cached total part count for quick display without querying PlacedBrick
	"lastBuildDate" TIMESTAMP NULL,		-- When the user last modified the build (placed or moved a brick)
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
	"projectId" INT NOT NULL,		-- The project this brick is placed in
	"brickPartId" INT NOT NULL,		-- The part definition being placed
	"brickColourId" INT NOT NULL,		-- The colour of this placed brick instance
	"positionX" REAL NULL,		-- X position in world coordinates (LDU)
	"positionY" REAL NULL,		-- Y position in world coordinates (LDU)
	"positionZ" REAL NULL,		-- Z position in world coordinates (LDU)
	"rotationX" REAL NULL,		-- Quaternion X component
	"rotationY" REAL NULL,		-- Quaternion Y component
	"rotationZ" REAL NULL,		-- Quaternion Z component
	"rotationW" REAL NULL,		-- Quaternion W component
	"buildStepNumber" INT NULL,		-- Optional build step number for instruction ordering
	"isHidden" BOOLEAN NOT NULL DEFAULT false,		-- Whether this brick is temporarily hidden in the editor
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
	"projectId" INT NOT NULL,		-- The project this connection belongs to
	"sourcePlacedBrickId" INT NOT NULL,		-- FK to the source PlacedBrick
	"sourceConnectorId" INT NOT NULL,		-- FK to the BrickPartConnector on the source brick
	"targetPlacedBrickId" INT NOT NULL,		-- FK to the target PlacedBrick
	"targetConnectorId" INT NOT NULL,		-- FK to the BrickPartConnector on the target brick
	"connectionStrength" VARCHAR(50) NULL,		-- Connection strength: Snapped, Friction, Free (null = unknown)
	"isLocked" BOOLEAN NOT NULL DEFAULT false,		-- Whether the user has locked this connection from being broken in the editor
	"angleDegrees" REAL NULL,		-- Current angle for rotational connections (null = default/fixed)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "projectId" FOREIGN KEY ("projectId") REFERENCES "BMC"."Project"("id"),		-- Foreign key to the Project table.
	CONSTRAINT "sourcePlacedBrickId" FOREIGN KEY ("sourcePlacedBrickId") REFERENCES "BMC"."PlacedBrick"("id"),		-- Foreign key to the PlacedBrick table.
	CONSTRAINT "sourceConnectorId" FOREIGN KEY ("sourceConnectorId") REFERENCES "BMC"."BrickPartConnector"("id"),		-- Foreign key to the BrickPartConnector table.
	CONSTRAINT "targetPlacedBrickId" FOREIGN KEY ("targetPlacedBrickId") REFERENCES "BMC"."PlacedBrick"("id"),		-- Foreign key to the PlacedBrick table.
	CONSTRAINT "targetConnectorId" FOREIGN KEY ("targetConnectorId") REFERENCES "BMC"."BrickPartConnector"("id")		-- Foreign key to the BrickPartConnector table.
);
-- Index on the BrickConnection table's tenantGuid field.
CREATE INDEX "I_BrickConnection_tenantGuid" ON "BMC"."BrickConnection" ("tenantGuid")
;

-- Index on the BrickConnection table's tenantGuid,projectId fields.
CREATE INDEX "I_BrickConnection_tenantGuid_projectId" ON "BMC"."BrickConnection" ("tenantGuid", "projectId")
;

-- Index on the BrickConnection table's tenantGuid,sourcePlacedBrickId fields.
CREATE INDEX "I_BrickConnection_tenantGuid_sourcePlacedBrickId" ON "BMC"."BrickConnection" ("tenantGuid", "sourcePlacedBrickId")
;

-- Index on the BrickConnection table's tenantGuid,sourceConnectorId fields.
CREATE INDEX "I_BrickConnection_tenantGuid_sourceConnectorId" ON "BMC"."BrickConnection" ("tenantGuid", "sourceConnectorId")
;

-- Index on the BrickConnection table's tenantGuid,targetPlacedBrickId fields.
CREATE INDEX "I_BrickConnection_tenantGuid_targetPlacedBrickId" ON "BMC"."BrickConnection" ("tenantGuid", "targetPlacedBrickId")
;

-- Index on the BrickConnection table's tenantGuid,targetConnectorId fields.
CREATE INDEX "I_BrickConnection_tenantGuid_targetConnectorId" ON "BMC"."BrickConnection" ("tenantGuid", "targetConnectorId")
;

-- Index on the BrickConnection table's tenantGuid,active fields.
CREATE INDEX "I_BrickConnection_tenantGuid_active" ON "BMC"."BrickConnection" ("tenantGuid", "active")
;

-- Index on the BrickConnection table's tenantGuid,deleted fields.
CREATE INDEX "I_BrickConnection_tenantGuid_deleted" ON "BMC"."BrickConnection" ("tenantGuid", "deleted")
;


-- A named sub-assembly within a project, similar to LDraw subfiles. Allows hierarchical builds.
CREATE TABLE "BMC"."Submodel"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"projectId" INT NOT NULL,		-- The project this submodel belongs to
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"submodelId" INT NULL,		-- Optional parent submodel for nested sub-assemblies (self-referencing FK, null = top-level)
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "projectId" FOREIGN KEY ("projectId") REFERENCES "BMC"."Project"("id"),		-- Foreign key to the Project table.
	CONSTRAINT "submodelId" FOREIGN KEY ("submodelId") REFERENCES "BMC"."Submodel"("id"),		-- Foreign key to the Submodel table.
	CONSTRAINT "UC_Submodel_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Submodel table's tenantGuid and name fields.
);
-- Index on the Submodel table's tenantGuid field.
CREATE INDEX "I_Submodel_tenantGuid" ON "BMC"."Submodel" ("tenantGuid")
;

-- Index on the Submodel table's tenantGuid,projectId fields.
CREATE INDEX "I_Submodel_tenantGuid_projectId" ON "BMC"."Submodel" ("tenantGuid", "projectId")
;

-- Index on the Submodel table's tenantGuid,name fields.
CREATE INDEX "I_Submodel_tenantGuid_name" ON "BMC"."Submodel" ("tenantGuid", "name")
;

-- Index on the Submodel table's tenantGuid,submodelId fields.
CREATE INDEX "I_Submodel_tenantGuid_submodelId" ON "BMC"."Submodel" ("tenantGuid", "submodelId")
;

-- Index on the Submodel table's tenantGuid,active fields.
CREATE INDEX "I_Submodel_tenantGuid_active" ON "BMC"."Submodel" ("tenantGuid", "active")
;

-- Index on the Submodel table's tenantGuid,deleted fields.
CREATE INDEX "I_Submodel_tenantGuid_deleted" ON "BMC"."Submodel" ("tenantGuid", "deleted")
;


-- The change history for records from the Submodel table.
CREATE TABLE "BMC"."SubmodelChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"submodelId" INT NOT NULL,		-- Link to the Submodel table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "submodelId" FOREIGN KEY ("submodelId") REFERENCES "BMC"."Submodel"("id")		-- Foreign key to the Submodel table.
);
-- Index on the SubmodelChangeHistory table's tenantGuid field.
CREATE INDEX "I_SubmodelChangeHistory_tenantGuid" ON "BMC"."SubmodelChangeHistory" ("tenantGuid")
;

-- Index on the SubmodelChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_SubmodelChangeHistory_tenantGuid_versionNumber" ON "BMC"."SubmodelChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the SubmodelChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_SubmodelChangeHistory_tenantGuid_timeStamp" ON "BMC"."SubmodelChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the SubmodelChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_SubmodelChangeHistory_tenantGuid_userId" ON "BMC"."SubmodelChangeHistory" ("tenantGuid", "userId")
;

-- Index on the SubmodelChangeHistory table's tenantGuid,submodelId fields.
CREATE INDEX "I_SubmodelChangeHistory_tenantGuid_submodelId" ON "BMC"."SubmodelChangeHistory" ("tenantGuid", "submodelId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Maps placed bricks to the submodel they belong to. A placed brick can only belong to one submodel.
CREATE TABLE "BMC"."SubmodelPlacedBrick"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"submodelId" INT NOT NULL,		-- The submodel this brick belongs to
	"placedBrickId" INT NOT NULL,		-- The placed brick assigned to this submodel
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "submodelId" FOREIGN KEY ("submodelId") REFERENCES "BMC"."Submodel"("id"),		-- Foreign key to the Submodel table.
	CONSTRAINT "placedBrickId" FOREIGN KEY ("placedBrickId") REFERENCES "BMC"."PlacedBrick"("id"),		-- Foreign key to the PlacedBrick table.
	CONSTRAINT "UC_SubmodelPlacedBrick_tenantGuid_placedBrickId" UNIQUE ( "tenantGuid", "placedBrickId") 		-- Uniqueness enforced on the SubmodelPlacedBrick table's tenantGuid and placedBrickId fields.
);
-- Index on the SubmodelPlacedBrick table's tenantGuid field.
CREATE INDEX "I_SubmodelPlacedBrick_tenantGuid" ON "BMC"."SubmodelPlacedBrick" ("tenantGuid")
;

-- Index on the SubmodelPlacedBrick table's tenantGuid,submodelId fields.
CREATE INDEX "I_SubmodelPlacedBrick_tenantGuid_submodelId" ON "BMC"."SubmodelPlacedBrick" ("tenantGuid", "submodelId")
;

-- Index on the SubmodelPlacedBrick table's tenantGuid,placedBrickId fields.
CREATE INDEX "I_SubmodelPlacedBrick_tenantGuid_placedBrickId" ON "BMC"."SubmodelPlacedBrick" ("tenantGuid", "placedBrickId")
;

-- Index on the SubmodelPlacedBrick table's tenantGuid,active fields.
CREATE INDEX "I_SubmodelPlacedBrick_tenantGuid_active" ON "BMC"."SubmodelPlacedBrick" ("tenantGuid", "active")
;

-- Index on the SubmodelPlacedBrick table's tenantGuid,deleted fields.
CREATE INDEX "I_SubmodelPlacedBrick_tenantGuid_deleted" ON "BMC"."SubmodelPlacedBrick" ("tenantGuid", "deleted")
;


-- Tracks where a submodel assembly is placed in the parent model. Stores the LDraw type-1 reference line transform data so the model can be faithfully reconstructed. A single Submodel can have multiple instances (e.g. left/right wheel assemblies at different positions).
CREATE TABLE "BMC"."SubmodelInstance"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"submodelId" INT NOT NULL,		-- The submodel definition being placed
	"parentSubmodelId" INT NULL,		-- The parent submodel this instance is placed WITHIN (null = placed directly in the main model)
	"positionX" REAL NULL,		-- X position from the LDraw type 1 reference line (LDU)
	"positionY" REAL NULL,		-- Y position from the LDraw type 1 reference line (LDU)
	"positionZ" REAL NULL,		-- Z position from the LDraw type 1 reference line (LDU)
	"rotationX" REAL NULL,		-- Quaternion X component (from 3x3 rotation matrix)
	"rotationY" REAL NULL,		-- Quaternion Y component
	"rotationZ" REAL NULL,		-- Quaternion Z component
	"rotationW" REAL NULL,		-- Quaternion W component (1 = no rotation)
	"colourCode" INT NOT NULL,		-- LDraw colour code from the type-1 reference line (usually 16 = inherit)
	"buildStepNumber" INT NOT NULL,		-- Build step number in the parent model where this instance is placed
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "submodelId" FOREIGN KEY ("submodelId") REFERENCES "BMC"."Submodel"("id"),		-- Foreign key to the Submodel table.
	CONSTRAINT "parentSubmodelId" FOREIGN KEY ("parentSubmodelId") REFERENCES "BMC"."Submodel"("id")		-- Foreign key to the Submodel table.
);
-- Index on the SubmodelInstance table's tenantGuid field.
CREATE INDEX "I_SubmodelInstance_tenantGuid" ON "BMC"."SubmodelInstance" ("tenantGuid")
;

-- Index on the SubmodelInstance table's tenantGuid,submodelId fields.
CREATE INDEX "I_SubmodelInstance_tenantGuid_submodelId" ON "BMC"."SubmodelInstance" ("tenantGuid", "submodelId")
;

-- Index on the SubmodelInstance table's tenantGuid,parentSubmodelId fields.
CREATE INDEX "I_SubmodelInstance_tenantGuid_parentSubmodelId" ON "BMC"."SubmodelInstance" ("tenantGuid", "parentSubmodelId")
;

-- Index on the SubmodelInstance table's tenantGuid,active fields.
CREATE INDEX "I_SubmodelInstance_tenantGuid_active" ON "BMC"."SubmodelInstance" ("tenantGuid", "active")
;

-- Index on the SubmodelInstance table's tenantGuid,deleted fields.
CREATE INDEX "I_SubmodelInstance_tenantGuid_deleted" ON "BMC"."SubmodelInstance" ("tenantGuid", "deleted")
;


-- User-defined tags for categorizing and filtering projects (e.g. Technic, MOC, WIP).
CREATE TABLE "BMC"."ProjectTag"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_ProjectTag_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the ProjectTag table's tenantGuid and name fields.
);
-- Index on the ProjectTag table's tenantGuid field.
CREATE INDEX "I_ProjectTag_tenantGuid" ON "BMC"."ProjectTag" ("tenantGuid")
;

-- Index on the ProjectTag table's tenantGuid,name fields.
CREATE INDEX "I_ProjectTag_tenantGuid_name" ON "BMC"."ProjectTag" ("tenantGuid", "name")
;

-- Index on the ProjectTag table's tenantGuid,active fields.
CREATE INDEX "I_ProjectTag_tenantGuid_active" ON "BMC"."ProjectTag" ("tenantGuid", "active")
;

-- Index on the ProjectTag table's tenantGuid,deleted fields.
CREATE INDEX "I_ProjectTag_tenantGuid_deleted" ON "BMC"."ProjectTag" ("tenantGuid", "deleted")
;


-- Many-to-many mapping between projects and tags.
CREATE TABLE "BMC"."ProjectTagAssignment"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"projectId" INT NOT NULL,		-- The project being tagged
	"projectTagId" INT NOT NULL,		-- The tag applied to the project
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "projectId" FOREIGN KEY ("projectId") REFERENCES "BMC"."Project"("id"),		-- Foreign key to the Project table.
	CONSTRAINT "projectTagId" FOREIGN KEY ("projectTagId") REFERENCES "BMC"."ProjectTag"("id"),		-- Foreign key to the ProjectTag table.
	CONSTRAINT "UC_ProjectTagAssignment_tenantGuid_projectId_projectTagId" UNIQUE ( "tenantGuid", "projectId", "projectTagId") 		-- Uniqueness enforced on the ProjectTagAssignment table's tenantGuid and projectId and projectTagId fields.
);
-- Index on the ProjectTagAssignment table's tenantGuid field.
CREATE INDEX "I_ProjectTagAssignment_tenantGuid" ON "BMC"."ProjectTagAssignment" ("tenantGuid")
;

-- Index on the ProjectTagAssignment table's tenantGuid,projectId fields.
CREATE INDEX "I_ProjectTagAssignment_tenantGuid_projectId" ON "BMC"."ProjectTagAssignment" ("tenantGuid", "projectId")
;

-- Index on the ProjectTagAssignment table's tenantGuid,projectTagId fields.
CREATE INDEX "I_ProjectTagAssignment_tenantGuid_projectTagId" ON "BMC"."ProjectTagAssignment" ("tenantGuid", "projectTagId")
;

-- Index on the ProjectTagAssignment table's tenantGuid,active fields.
CREATE INDEX "I_ProjectTagAssignment_tenantGuid_active" ON "BMC"."ProjectTagAssignment" ("tenantGuid", "active")
;

-- Index on the ProjectTagAssignment table's tenantGuid,deleted fields.
CREATE INDEX "I_ProjectTagAssignment_tenantGuid_deleted" ON "BMC"."ProjectTagAssignment" ("tenantGuid", "deleted")
;


-- Saved camera positions and orientations for quick viewport recall in the 3D editor.
CREATE TABLE "BMC"."ProjectCameraPreset"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"projectId" INT NOT NULL,		-- The project this camera preset belongs to
	"name" VARCHAR(100) NOT NULL,
	"positionX" REAL NULL,		-- Camera X position in world coordinates (LDU)
	"positionY" REAL NULL,		-- Camera Y position in world coordinates (LDU)
	"positionZ" REAL NULL,		-- Camera Z position in world coordinates (LDU)
	"targetX" REAL NULL,		-- Camera target X position (look-at point)
	"targetY" REAL NULL,		-- Camera target Y position (look-at point)
	"targetZ" REAL NULL,		-- Camera target Z position (look-at point)
	"zoom" REAL NULL,		-- Camera zoom level / field of view
	"isPerspective" BOOLEAN NOT NULL DEFAULT true,		-- True for perspective projection, false for orthographic
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "projectId" FOREIGN KEY ("projectId") REFERENCES "BMC"."Project"("id"),		-- Foreign key to the Project table.
	CONSTRAINT "UC_ProjectCameraPreset_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the ProjectCameraPreset table's tenantGuid and name fields.
);
-- Index on the ProjectCameraPreset table's tenantGuid field.
CREATE INDEX "I_ProjectCameraPreset_tenantGuid" ON "BMC"."ProjectCameraPreset" ("tenantGuid")
;

-- Index on the ProjectCameraPreset table's tenantGuid,projectId fields.
CREATE INDEX "I_ProjectCameraPreset_tenantGuid_projectId" ON "BMC"."ProjectCameraPreset" ("tenantGuid", "projectId")
;

-- Index on the ProjectCameraPreset table's tenantGuid,name fields.
CREATE INDEX "I_ProjectCameraPreset_tenantGuid_name" ON "BMC"."ProjectCameraPreset" ("tenantGuid", "name")
;

-- Index on the ProjectCameraPreset table's tenantGuid,active fields.
CREATE INDEX "I_ProjectCameraPreset_tenantGuid_active" ON "BMC"."ProjectCameraPreset" ("tenantGuid", "active")
;

-- Index on the ProjectCameraPreset table's tenantGuid,deleted fields.
CREATE INDEX "I_ProjectCameraPreset_tenantGuid_deleted" ON "BMC"."ProjectCameraPreset" ("tenantGuid", "deleted")
;


-- Reference images overlaid in the 3D editor for proportioning and tracing.
CREATE TABLE "BMC"."ProjectReferenceImage"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"projectId" INT NOT NULL,		-- The project this reference image belongs to
	"name" VARCHAR(100) NOT NULL,
	"imageFilePath" VARCHAR(250) NULL,		-- Relative path to the uploaded reference image file
	"opacity" REAL NULL,		-- Opacity of the overlay (0.0 = invisible, 1.0 = fully opaque)
	"positionX" REAL NULL,		-- X position of the image plane in world coordinates (LDU)
	"positionY" REAL NULL,		-- Y position of the image plane in world coordinates (LDU)
	"positionZ" REAL NULL,		-- Z position of the image plane in world coordinates (LDU)
	"scaleX" REAL NULL,		-- Horizontal scale factor for the reference image
	"scaleY" REAL NULL,		-- Vertical scale factor for the reference image
	"isVisible" BOOLEAN NOT NULL DEFAULT true,		-- Whether the reference image is currently visible in the editor
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "projectId" FOREIGN KEY ("projectId") REFERENCES "BMC"."Project"("id"),		-- Foreign key to the Project table.
	CONSTRAINT "UC_ProjectReferenceImage_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the ProjectReferenceImage table's tenantGuid and name fields.
);
-- Index on the ProjectReferenceImage table's tenantGuid field.
CREATE INDEX "I_ProjectReferenceImage_tenantGuid" ON "BMC"."ProjectReferenceImage" ("tenantGuid")
;

-- Index on the ProjectReferenceImage table's tenantGuid,projectId fields.
CREATE INDEX "I_ProjectReferenceImage_tenantGuid_projectId" ON "BMC"."ProjectReferenceImage" ("tenantGuid", "projectId")
;

-- Index on the ProjectReferenceImage table's tenantGuid,name fields.
CREATE INDEX "I_ProjectReferenceImage_tenantGuid_name" ON "BMC"."ProjectReferenceImage" ("tenantGuid", "name")
;

-- Index on the ProjectReferenceImage table's tenantGuid,active fields.
CREATE INDEX "I_ProjectReferenceImage_tenantGuid_active" ON "BMC"."ProjectReferenceImage" ("tenantGuid", "active")
;

-- Index on the ProjectReferenceImage table's tenantGuid,deleted fields.
CREATE INDEX "I_ProjectReferenceImage_tenantGuid_deleted" ON "BMC"."ProjectReferenceImage" ("tenantGuid", "deleted")
;


-- Persistent representation of an imported or authored multi-part model document (MPD/LDR). Top-level container for complex models with build steps.
CREATE TABLE "BMC"."ModelDocument"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"projectId" INT NULL,		-- Optional link to a BMC project if authored in BMC (null for imported documents)
	"name" VARCHAR(250) NOT NULL,		-- Document name or title
	"description" TEXT NULL,		-- Description of the model document
	"sourceFormat" VARCHAR(50) NOT NULL,		-- Source file format: MPD, LDR, StudioIO, BMCNative
	"sourceFileName" VARCHAR(250) NULL,		-- Original filename if imported (e.g. 'crane_42131.mpd')
	"sourceFileFileName" VARCHAR(250) NULL,		-- Part of the binary data field setup
	"sourceFileSize" BIGINT NULL,		-- Part of the binary data field setup
	"sourceFileData" BYTEA NULL,		-- Part of the binary data field setup
	"sourceFileMimeType" VARCHAR(100) NULL,		-- Part of the binary data field setup
	"author" VARCHAR(100) NULL,		-- Model author from the file header
	"totalPartCount" INT NULL,		-- Cached total part count across all sub-files
	"totalStepCount" INT NULL,		-- Cached total build step count across all sub-files
	"studioVersion" VARCHAR(100) NULL,		-- BrickLink Studio version that created this document (from .io .INFO file)
	"instructionSettingsXml" TEXT NULL,		-- Raw XML instruction configuration from .io model.ins file
	"errorPartList" TEXT NULL,		-- Content of errorPartList.err from .io archive listing problematic or missing parts
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "projectId" FOREIGN KEY ("projectId") REFERENCES "BMC"."Project"("id")		-- Foreign key to the Project table.
);
-- Index on the ModelDocument table's tenantGuid field.
CREATE INDEX "I_ModelDocument_tenantGuid" ON "BMC"."ModelDocument" ("tenantGuid")
;

-- Index on the ModelDocument table's tenantGuid,projectId fields.
CREATE INDEX "I_ModelDocument_tenantGuid_projectId" ON "BMC"."ModelDocument" ("tenantGuid", "projectId")
;

-- Index on the ModelDocument table's tenantGuid,name fields.
CREATE INDEX "I_ModelDocument_tenantGuid_name" ON "BMC"."ModelDocument" ("tenantGuid", "name")
;

-- Index on the ModelDocument table's tenantGuid,active fields.
CREATE INDEX "I_ModelDocument_tenantGuid_active" ON "BMC"."ModelDocument" ("tenantGuid", "active")
;

-- Index on the ModelDocument table's tenantGuid,deleted fields.
CREATE INDEX "I_ModelDocument_tenantGuid_deleted" ON "BMC"."ModelDocument" ("tenantGuid", "deleted")
;


-- The change history for records from the ModelDocument table.
CREATE TABLE "BMC"."ModelDocumentChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"modelDocumentId" INT NOT NULL,		-- Link to the ModelDocument table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "modelDocumentId" FOREIGN KEY ("modelDocumentId") REFERENCES "BMC"."ModelDocument"("id")		-- Foreign key to the ModelDocument table.
);
-- Index on the ModelDocumentChangeHistory table's tenantGuid field.
CREATE INDEX "I_ModelDocumentChangeHistory_tenantGuid" ON "BMC"."ModelDocumentChangeHistory" ("tenantGuid")
;

-- Index on the ModelDocumentChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_ModelDocumentChangeHistory_tenantGuid_versionNumber" ON "BMC"."ModelDocumentChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the ModelDocumentChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_ModelDocumentChangeHistory_tenantGuid_timeStamp" ON "BMC"."ModelDocumentChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the ModelDocumentChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_ModelDocumentChangeHistory_tenantGuid_userId" ON "BMC"."ModelDocumentChangeHistory" ("tenantGuid", "userId")
;

-- Index on the ModelDocumentChangeHistory table's tenantGuid,modelDocumentId fields.
CREATE INDEX "I_ModelDocumentChangeHistory_tenantGuid_modelDocumentId" ON "BMC"."ModelDocumentChangeHistory" ("tenantGuid", "modelDocumentId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Individual sub-files within a model document (MPD). Each represents a sub-assembly or the main model. Maps to LDraw '0 FILE' blocks.
CREATE TABLE "BMC"."ModelSubFile"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"modelDocumentId" INT NOT NULL,		-- The document this sub-file belongs to
	"fileName" VARCHAR(250) NOT NULL,		-- Sub-file name as declared in '0 FILE' (e.g. 'main.ldr', 'wheel_assembly.ldr')
	"isMainModel" BOOLEAN NOT NULL DEFAULT false,		-- Whether this is the main (first) model in the MPD — only rendered sub-files are those referenced by this
	"parentModelSubFileId" INT NULL,		-- Optional parent sub-file for nested sub-assemblies (null = top-level)
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "modelDocumentId" FOREIGN KEY ("modelDocumentId") REFERENCES "BMC"."ModelDocument"("id"),		-- Foreign key to the ModelDocument table.
	CONSTRAINT "parentModelSubFileId" FOREIGN KEY ("parentModelSubFileId") REFERENCES "BMC"."ModelSubFile"("id"),		-- Foreign key to the ModelSubFile table.
	CONSTRAINT "UC_ModelSubFile_tenantGuid_modelDocumentId_fileName" UNIQUE ( "tenantGuid", "modelDocumentId", "fileName") 		-- Uniqueness enforced on the ModelSubFile table's tenantGuid and modelDocumentId and fileName fields.
);
-- Index on the ModelSubFile table's tenantGuid field.
CREATE INDEX "I_ModelSubFile_tenantGuid" ON "BMC"."ModelSubFile" ("tenantGuid")
;

-- Index on the ModelSubFile table's tenantGuid,modelDocumentId fields.
CREATE INDEX "I_ModelSubFile_tenantGuid_modelDocumentId" ON "BMC"."ModelSubFile" ("tenantGuid", "modelDocumentId")
;

-- Index on the ModelSubFile table's tenantGuid,parentModelSubFileId fields.
CREATE INDEX "I_ModelSubFile_tenantGuid_parentModelSubFileId" ON "BMC"."ModelSubFile" ("tenantGuid", "parentModelSubFileId")
;

-- Index on the ModelSubFile table's tenantGuid,active fields.
CREATE INDEX "I_ModelSubFile_tenantGuid_active" ON "BMC"."ModelSubFile" ("tenantGuid", "active")
;

-- Index on the ModelSubFile table's tenantGuid,deleted fields.
CREATE INDEX "I_ModelSubFile_tenantGuid_deleted" ON "BMC"."ModelSubFile" ("tenantGuid", "deleted")
;


-- Individual build steps within a model sub-file. Modeled from LDraw '0 STEP' and '0 ROTSTEP' meta-commands.
CREATE TABLE "BMC"."ModelBuildStep"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"modelSubFileId" INT NOT NULL,		-- The sub-file this step belongs to
	"stepNumber" INT NOT NULL,		-- Sequential step number within this sub-file
	"rotationType" VARCHAR(10) NULL,		-- ROTSTEP rotation type: REL (relative), ABS (absolute), ADD (additive). Null = no rotation.
	"rotationX" REAL NULL,		-- ROTSTEP X rotation angle in degrees
	"rotationY" REAL NULL,		-- ROTSTEP Y rotation angle in degrees
	"rotationZ" REAL NULL,		-- ROTSTEP Z rotation angle in degrees
	"description" TEXT NULL,		-- Optional step description or annotation
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "modelSubFileId" FOREIGN KEY ("modelSubFileId") REFERENCES "BMC"."ModelSubFile"("id")		-- Foreign key to the ModelSubFile table.
);
-- Index on the ModelBuildStep table's tenantGuid field.
CREATE INDEX "I_ModelBuildStep_tenantGuid" ON "BMC"."ModelBuildStep" ("tenantGuid")
;

-- Index on the ModelBuildStep table's tenantGuid,modelSubFileId fields.
CREATE INDEX "I_ModelBuildStep_tenantGuid_modelSubFileId" ON "BMC"."ModelBuildStep" ("tenantGuid", "modelSubFileId")
;

-- Index on the ModelBuildStep table's tenantGuid,active fields.
CREATE INDEX "I_ModelBuildStep_tenantGuid_active" ON "BMC"."ModelBuildStep" ("tenantGuid", "active")
;

-- Index on the ModelBuildStep table's tenantGuid,deleted fields.
CREATE INDEX "I_ModelBuildStep_tenantGuid_deleted" ON "BMC"."ModelBuildStep" ("tenantGuid", "deleted")
;


-- Parts placed during a specific build step. Represents LDraw type 1 sub-file reference lines between STEP commands.
CREATE TABLE "BMC"."ModelStepPart"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"modelBuildStepId" INT NOT NULL,		-- The build step this part placement belongs to
	"brickPartId" INT NULL,		-- FK to BrickPart if this part is in the catalog (null if not yet cataloged)
	"brickColourId" INT NULL,		-- FK to BrickColour if the color is mapped (null if unmapped)
	"partFileName" VARCHAR(250) NOT NULL,		-- Original LDraw part filename from the type 1 line (e.g. '3001.dat')
	"colorCode" INT NOT NULL,		-- LDraw color code used in the file
	"positionX" REAL NULL,		-- X position from the LDraw type 1 reference line (LDU)
	"positionY" REAL NULL,		-- Y position from the LDraw type 1 reference line (LDU)
	"positionZ" REAL NULL,		-- Z position from the LDraw type 1 reference line (LDU)
	"transformMatrix" VARCHAR(500) NOT NULL,		-- 3x3 rotation matrix as space-delimited floats (a b c d e f g h i)
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "modelBuildStepId" FOREIGN KEY ("modelBuildStepId") REFERENCES "BMC"."ModelBuildStep"("id"),		-- Foreign key to the ModelBuildStep table.
	CONSTRAINT "brickPartId" FOREIGN KEY ("brickPartId") REFERENCES "BMC"."BrickPart"("id"),		-- Foreign key to the BrickPart table.
	CONSTRAINT "brickColourId" FOREIGN KEY ("brickColourId") REFERENCES "BMC"."BrickColour"("id")		-- Foreign key to the BrickColour table.
);
-- Index on the ModelStepPart table's tenantGuid field.
CREATE INDEX "I_ModelStepPart_tenantGuid" ON "BMC"."ModelStepPart" ("tenantGuid")
;

-- Index on the ModelStepPart table's tenantGuid,modelBuildStepId fields.
CREATE INDEX "I_ModelStepPart_tenantGuid_modelBuildStepId" ON "BMC"."ModelStepPart" ("tenantGuid", "modelBuildStepId")
;

-- Index on the ModelStepPart table's tenantGuid,brickPartId fields.
CREATE INDEX "I_ModelStepPart_tenantGuid_brickPartId" ON "BMC"."ModelStepPart" ("tenantGuid", "brickPartId")
;

-- Index on the ModelStepPart table's tenantGuid,brickColourId fields.
CREATE INDEX "I_ModelStepPart_tenantGuid_brickColourId" ON "BMC"."ModelStepPart" ("tenantGuid", "brickColourId")
;

-- Index on the ModelStepPart table's tenantGuid,active fields.
CREATE INDEX "I_ModelStepPart_tenantGuid_active" ON "BMC"."ModelStepPart" ("tenantGuid", "active")
;

-- Index on the ModelStepPart table's tenantGuid,deleted fields.
CREATE INDEX "I_ModelStepPart_tenantGuid_deleted" ON "BMC"."ModelStepPart" ("tenantGuid", "deleted")
;


-- Hierarchical tree of official LEGO themes (e.g. City → Police, Technic → Bionicle). Bulk-loaded from Rebrickable or similar sources.
CREATE TABLE "BMC"."LegoTheme"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"legoThemeId" INT NULL,		-- Parent theme for hierarchical nesting (self-referencing FK, null = top-level)
	"rebrickableThemeId" INT NOT NULL,		-- Rebrickable theme ID — source of truth for theme identity
	"brickSetThemeName" VARCHAR(100) NULL,		-- BrickSet theme name for API calls — may differ from Rebrickable theme name (null if not mapped)
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "legoThemeId" FOREIGN KEY ("legoThemeId") REFERENCES "BMC"."LegoTheme"("id")		-- Foreign key to the LegoTheme table.
);
-- Index on the LegoTheme table's name field.
CREATE INDEX "I_LegoTheme_name" ON "BMC"."LegoTheme" ("name")
;

-- Index on the LegoTheme table's legoThemeId field.
CREATE INDEX "I_LegoTheme_legoThemeId" ON "BMC"."LegoTheme" ("legoThemeId")
;

-- Index on the LegoTheme table's active field.
CREATE INDEX "I_LegoTheme_active" ON "BMC"."LegoTheme" ("active")
;

-- Index on the LegoTheme table's deleted field.
CREATE INDEX "I_LegoTheme_deleted" ON "BMC"."LegoTheme" ("deleted")
;


-- Official LEGO set definitions. Each row represents a distinct set release (e.g. 42131-1 Liebherr Crawler Crane).
CREATE TABLE "BMC"."LegoSet"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(500) NOT NULL,		-- For really long set names
	"setNumber" VARCHAR(100) NOT NULL,		-- Official set number including variant suffix (e.g. '42131-1', '10302-1')
	"year" INT NOT NULL,		-- Release year of the set
	"partCount" INT NOT NULL,		-- Total number of parts in the set (as listed by LEGO)
	"legoThemeId" INT NULL,		-- The theme this set belongs to (null if theme not yet categorized)
	"imageUrl" VARCHAR(250) NULL,		-- URL to the set's official box art or primary image
	"brickLinkUrl" VARCHAR(250) NULL,		-- URL to the set's BrickLink catalogue page
	"rebrickableUrl" VARCHAR(250) NULL,		-- URL to the set's Rebrickable page
	"rebrickableSetNum" VARCHAR(100) NULL,		-- Explicit Rebrickable set number if it differs from setNumber
	"lastModifiedDate" TIMESTAMP NULL,		-- Last modification date for incremental sync with Rebrickable
	"brickSetId" INT NULL,		-- BrickSet internal set ID — used for API calls (null if not yet enriched from BrickSet)
	"brickSetUrl" VARCHAR(250) NULL,		-- URL to the set's BrickSet page
	"retailPriceUS" DECIMAL(11,2) NULL,		-- US retail price in USD from BrickSet (null if not available)
	"retailPriceUK" DECIMAL(11,2) NULL,		-- UK retail price in GBP from BrickSet (null if not available)
	"retailPriceCA" DECIMAL(11,2) NULL,		-- Canadian retail price in CAD from BrickSet (null if not available)
	"retailPriceEU" DECIMAL(11,2) NULL,		-- EU retail price in EUR from BrickSet (null if not available)
	"instructionsUrl" VARCHAR(500) NULL,		-- URL to the set's official building instructions PDF (sourced from BrickSet)
	"subtheme" VARCHAR(100) NULL,		-- Subtheme name from BrickSet (e.g. 'Police' under City)
	"availability" VARCHAR(50) NULL,		-- Current availability status from BrickSet (e.g. 'Retail', 'Retired', 'LEGO exclusive')
	"minifigCount" INT NULL,		-- Number of minifigs included in the set (from BrickSet, null if unknown)
	"brickSetRating" REAL NULL,		-- Average community rating on BrickSet (1.0-5.0, null if no reviews)
	"brickSetReviewCount" INT NULL,		-- Number of community reviews on BrickSet (null if unknown)
	"brickSetLastEnrichedDate" TIMESTAMP NULL,		-- When this set was last enriched with BrickSet data (null = never enriched)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "legoThemeId" FOREIGN KEY ("legoThemeId") REFERENCES "BMC"."LegoTheme"("id"),		-- Foreign key to the LegoTheme table.
	CONSTRAINT "UC_LegoSet_setNumber" UNIQUE ( "setNumber") 		-- Uniqueness enforced on the LegoSet table's setNumber field.
);
-- Index on the LegoSet table's name field.
CREATE INDEX "I_LegoSet_name" ON "BMC"."LegoSet" ("name")
;

-- Index on the LegoSet table's legoThemeId field.
CREATE INDEX "I_LegoSet_legoThemeId" ON "BMC"."LegoSet" ("legoThemeId")
;

-- Index on the LegoSet table's active field.
CREATE INDEX "I_LegoSet_active" ON "BMC"."LegoSet" ("active")
;

-- Index on the LegoSet table's deleted field.
CREATE INDEX "I_LegoSet_deleted" ON "BMC"."LegoSet" ("deleted")
;


-- Parts inventory for each official LEGO set. Maps set → part → colour → quantity.
CREATE TABLE "BMC"."LegoSetPart"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"legoSetId" INT NOT NULL,		-- The set this inventory line belongs to
	"brickPartId" INT NOT NULL,		-- The part included in this set
	"brickColourId" INT NOT NULL,		-- The colour of this part in the set
	"quantity" INT NULL,		-- Number of this part+colour combination included in the set
	"isSpare" BOOLEAN NOT NULL DEFAULT false,		-- Whether this is a spare part (included as extra in the bag, not used in the build)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "legoSetId" FOREIGN KEY ("legoSetId") REFERENCES "BMC"."LegoSet"("id"),		-- Foreign key to the LegoSet table.
	CONSTRAINT "brickPartId" FOREIGN KEY ("brickPartId") REFERENCES "BMC"."BrickPart"("id"),		-- Foreign key to the BrickPart table.
	CONSTRAINT "brickColourId" FOREIGN KEY ("brickColourId") REFERENCES "BMC"."BrickColour"("id")		-- Foreign key to the BrickColour table.
);
-- Index on the LegoSetPart table's legoSetId field.
CREATE INDEX "I_LegoSetPart_legoSetId" ON "BMC"."LegoSetPart" ("legoSetId")
;

-- Index on the LegoSetPart table's brickPartId field.
CREATE INDEX "I_LegoSetPart_brickPartId" ON "BMC"."LegoSetPart" ("brickPartId")
;

-- Index on the LegoSetPart table's brickColourId field.
CREATE INDEX "I_LegoSetPart_brickColourId" ON "BMC"."LegoSetPart" ("brickColourId")
;

-- Index on the LegoSetPart table's active field.
CREATE INDEX "I_LegoSetPart_active" ON "BMC"."LegoSetPart" ("active")
;

-- Index on the LegoSetPart table's deleted field.
CREATE INDEX "I_LegoSetPart_deleted" ON "BMC"."LegoSetPart" ("deleted")
;


-- Relationships between parts: alternates, molds, prints, pairs, sub-parts, and patterns. Bulk-loaded from Rebrickable part_relationships.csv.
CREATE TABLE "BMC"."BrickPartRelationship"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"childBrickPartId" INT NOT NULL,		-- The child part in the relationship
	"parentBrickPartId" INT NOT NULL,		-- The parent part in the relationship
	"relationshipType" VARCHAR(50) NOT NULL,		-- Type of relationship: Print, Pair, SubPart, Mold, Pattern, or Alternate
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "childBrickPartId" FOREIGN KEY ("childBrickPartId") REFERENCES "BMC"."BrickPart"("id"),		-- Foreign key to the BrickPart table.
	CONSTRAINT "parentBrickPartId" FOREIGN KEY ("parentBrickPartId") REFERENCES "BMC"."BrickPart"("id")		-- Foreign key to the BrickPart table.
);
-- Index on the BrickPartRelationship table's childBrickPartId field.
CREATE INDEX "I_BrickPartRelationship_childBrickPartId" ON "BMC"."BrickPartRelationship" ("childBrickPartId")
;

-- Index on the BrickPartRelationship table's parentBrickPartId field.
CREATE INDEX "I_BrickPartRelationship_parentBrickPartId" ON "BMC"."BrickPartRelationship" ("parentBrickPartId")
;

-- Index on the BrickPartRelationship table's active field.
CREATE INDEX "I_BrickPartRelationship_active" ON "BMC"."BrickPartRelationship" ("active")
;

-- Index on the BrickPartRelationship table's deleted field.
CREATE INDEX "I_BrickPartRelationship_deleted" ON "BMC"."BrickPartRelationship" ("deleted")
;


-- LEGO element IDs representing specific part+colour combinations. Used for cross-referencing with official LEGO catalogues and BrickLink.
CREATE TABLE "BMC"."BrickElement"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"elementId" VARCHAR(50) NOT NULL,		-- Official LEGO element ID (unique identifier for a specific part+colour combination)
	"brickPartId" INT NOT NULL,		-- The part this element represents
	"brickColourId" INT NOT NULL,		-- The colour of this element
	"designId" VARCHAR(50) NULL,		-- LEGO design ID (null if not available)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "brickPartId" FOREIGN KEY ("brickPartId") REFERENCES "BMC"."BrickPart"("id"),		-- Foreign key to the BrickPart table.
	CONSTRAINT "brickColourId" FOREIGN KEY ("brickColourId") REFERENCES "BMC"."BrickColour"("id"),		-- Foreign key to the BrickColour table.
	CONSTRAINT "UC_BrickElement_elementId" UNIQUE ( "elementId") 		-- Uniqueness enforced on the BrickElement table's elementId field.
);
-- Index on the BrickElement table's brickPartId field.
CREATE INDEX "I_BrickElement_brickPartId" ON "BMC"."BrickElement" ("brickPartId")
;

-- Index on the BrickElement table's brickColourId field.
CREATE INDEX "I_BrickElement_brickColourId" ON "BMC"."BrickElement" ("brickColourId")
;

-- Index on the BrickElement table's active field.
CREATE INDEX "I_BrickElement_active" ON "BMC"."BrickElement" ("active")
;

-- Index on the BrickElement table's deleted field.
CREATE INDEX "I_BrickElement_deleted" ON "BMC"."BrickElement" ("deleted")
;


-- Official LEGO minifigure definitions. Each row represents a distinct minifig (e.g. fig-000001 Han Solo).
CREATE TABLE "BMC"."LegoMinifig"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(500) NOT NULL,		-- Minifig name — can be long descriptive text from Rebrickable
	"figNumber" VARCHAR(100) NOT NULL,		-- Rebrickable minifig number (e.g. 'fig-000001')
	"partCount" INT NOT NULL,		-- Total number of parts in the minifig
	"imageUrl" VARCHAR(250) NULL,		-- URL to the minifig's image
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_LegoMinifig_figNumber" UNIQUE ( "figNumber") 		-- Uniqueness enforced on the LegoMinifig table's figNumber field.
);
-- Index on the LegoMinifig table's active field.
CREATE INDEX "I_LegoMinifig_active" ON "BMC"."LegoMinifig" ("active")
;

-- Index on the LegoMinifig table's deleted field.
CREATE INDEX "I_LegoMinifig_deleted" ON "BMC"."LegoMinifig" ("deleted")
;


-- Minifigs included in each official LEGO set's inventory, with quantities.
CREATE TABLE "BMC"."LegoSetMinifig"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"legoSetId" INT NOT NULL,		-- The set this minifig belongs to
	"legoMinifigId" INT NOT NULL,		-- The minifig included in the set
	"quantity" INT NULL,		-- Number of this minifig included in the set
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "legoSetId" FOREIGN KEY ("legoSetId") REFERENCES "BMC"."LegoSet"("id"),		-- Foreign key to the LegoSet table.
	CONSTRAINT "legoMinifigId" FOREIGN KEY ("legoMinifigId") REFERENCES "BMC"."LegoMinifig"("id")		-- Foreign key to the LegoMinifig table.
);
-- Index on the LegoSetMinifig table's legoSetId field.
CREATE INDEX "I_LegoSetMinifig_legoSetId" ON "BMC"."LegoSetMinifig" ("legoSetId")
;

-- Index on the LegoSetMinifig table's legoMinifigId field.
CREATE INDEX "I_LegoSetMinifig_legoMinifigId" ON "BMC"."LegoSetMinifig" ("legoMinifigId")
;

-- Index on the LegoSetMinifig table's active field.
CREATE INDEX "I_LegoSetMinifig_active" ON "BMC"."LegoSetMinifig" ("active")
;

-- Index on the LegoSetMinifig table's deleted field.
CREATE INDEX "I_LegoSetMinifig_deleted" ON "BMC"."LegoSetMinifig" ("deleted")
;


-- Sets included within other sets (e.g. polybags inside a larger set). Derived from Rebrickable inventory_sets.csv.
CREATE TABLE "BMC"."LegoSetSubset"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"parentLegoSetId" INT NOT NULL,		-- The parent set that contains the subset
	"childLegoSetId" INT NOT NULL,		-- The subset included within the parent set
	"quantity" INT NULL,		-- Number of copies of the subset included in the parent
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "parentLegoSetId" FOREIGN KEY ("parentLegoSetId") REFERENCES "BMC"."LegoSet"("id"),		-- Foreign key to the LegoSet table.
	CONSTRAINT "childLegoSetId" FOREIGN KEY ("childLegoSetId") REFERENCES "BMC"."LegoSet"("id")		-- Foreign key to the LegoSet table.
);
-- Index on the LegoSetSubset table's parentLegoSetId field.
CREATE INDEX "I_LegoSetSubset_parentLegoSetId" ON "BMC"."LegoSetSubset" ("parentLegoSetId")
;

-- Index on the LegoSetSubset table's childLegoSetId field.
CREATE INDEX "I_LegoSetSubset_childLegoSetId" ON "BMC"."LegoSetSubset" ("childLegoSetId")
;

-- Index on the LegoSetSubset table's active field.
CREATE INDEX "I_LegoSetSubset_active" ON "BMC"."LegoSetSubset" ("active")
;

-- Index on the LegoSetSubset table's deleted field.
CREATE INDEX "I_LegoSetSubset_deleted" ON "BMC"."LegoSetSubset" ("deleted")
;


-- A user's named part collection or palette. Users can have multiple collections (e.g. 'My Inventory', 'Technic Parts', 'Parts for MOC #5').
CREATE TABLE "BMC"."UserCollection"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"isDefault" BOOLEAN NOT NULL DEFAULT false,		-- Whether this is the user's primary / default collection
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_UserCollection_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the UserCollection table's tenantGuid and name fields.
);
-- Index on the UserCollection table's tenantGuid field.
CREATE INDEX "I_UserCollection_tenantGuid" ON "BMC"."UserCollection" ("tenantGuid")
;

-- Index on the UserCollection table's tenantGuid,name fields.
CREATE INDEX "I_UserCollection_tenantGuid_name" ON "BMC"."UserCollection" ("tenantGuid", "name")
;

-- Index on the UserCollection table's tenantGuid,active fields.
CREATE INDEX "I_UserCollection_tenantGuid_active" ON "BMC"."UserCollection" ("tenantGuid", "active")
;

-- Index on the UserCollection table's tenantGuid,deleted fields.
CREATE INDEX "I_UserCollection_tenantGuid_deleted" ON "BMC"."UserCollection" ("tenantGuid", "deleted")
;


-- The change history for records from the UserCollection table.
CREATE TABLE "BMC"."UserCollectionChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userCollectionId" INT NOT NULL,		-- Link to the UserCollection table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "userCollectionId" FOREIGN KEY ("userCollectionId") REFERENCES "BMC"."UserCollection"("id")		-- Foreign key to the UserCollection table.
);
-- Index on the UserCollectionChangeHistory table's tenantGuid field.
CREATE INDEX "I_UserCollectionChangeHistory_tenantGuid" ON "BMC"."UserCollectionChangeHistory" ("tenantGuid")
;

-- Index on the UserCollectionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_UserCollectionChangeHistory_tenantGuid_versionNumber" ON "BMC"."UserCollectionChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the UserCollectionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_UserCollectionChangeHistory_tenantGuid_timeStamp" ON "BMC"."UserCollectionChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the UserCollectionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_UserCollectionChangeHistory_tenantGuid_userId" ON "BMC"."UserCollectionChangeHistory" ("tenantGuid", "userId")
;

-- Index on the UserCollectionChangeHistory table's tenantGuid,userCollectionId fields.
CREATE INDEX "I_UserCollectionChangeHistory_tenantGuid_userCollectionId" ON "BMC"."UserCollectionChangeHistory" ("tenantGuid", "userCollectionId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Individual part+colour entries within a user collection, with quantity owned and quantity currently allocated to projects.
CREATE TABLE "BMC"."UserCollectionPart"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userCollectionId" INT NOT NULL,		-- The collection this part entry belongs to
	"brickPartId" INT NOT NULL,		-- The part definition
	"brickColourId" INT NOT NULL,		-- The specific colour of this part
	"quantityOwned" INT NULL,		-- Total number of this part+colour the user owns
	"quantityUsed" INT NULL,		-- Number currently allocated to projects (for build-with-what-you-own filtering)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "userCollectionId" FOREIGN KEY ("userCollectionId") REFERENCES "BMC"."UserCollection"("id"),		-- Foreign key to the UserCollection table.
	CONSTRAINT "brickPartId" FOREIGN KEY ("brickPartId") REFERENCES "BMC"."BrickPart"("id"),		-- Foreign key to the BrickPart table.
	CONSTRAINT "brickColourId" FOREIGN KEY ("brickColourId") REFERENCES "BMC"."BrickColour"("id"),		-- Foreign key to the BrickColour table.
	CONSTRAINT "UC_UserCollectionPart_tenantGuid_userCollectionId_brickPartId_brickColourId" UNIQUE ( "tenantGuid", "userCollectionId", "brickPartId", "brickColourId") 		-- Uniqueness enforced on the UserCollectionPart table's tenantGuid and userCollectionId and brickPartId and brickColourId fields.
);
-- Index on the UserCollectionPart table's tenantGuid field.
CREATE INDEX "I_UserCollectionPart_tenantGuid" ON "BMC"."UserCollectionPart" ("tenantGuid")
;

-- Index on the UserCollectionPart table's tenantGuid,userCollectionId fields.
CREATE INDEX "I_UserCollectionPart_tenantGuid_userCollectionId" ON "BMC"."UserCollectionPart" ("tenantGuid", "userCollectionId")
;

-- Index on the UserCollectionPart table's tenantGuid,brickPartId fields.
CREATE INDEX "I_UserCollectionPart_tenantGuid_brickPartId" ON "BMC"."UserCollectionPart" ("tenantGuid", "brickPartId")
;

-- Index on the UserCollectionPart table's tenantGuid,brickColourId fields.
CREATE INDEX "I_UserCollectionPart_tenantGuid_brickColourId" ON "BMC"."UserCollectionPart" ("tenantGuid", "brickColourId")
;

-- Index on the UserCollectionPart table's tenantGuid,active fields.
CREATE INDEX "I_UserCollectionPart_tenantGuid_active" ON "BMC"."UserCollectionPart" ("tenantGuid", "active")
;

-- Index on the UserCollectionPart table's tenantGuid,deleted fields.
CREATE INDEX "I_UserCollectionPart_tenantGuid_deleted" ON "BMC"."UserCollectionPart" ("tenantGuid", "deleted")
;


-- Parts the user wants to acquire. Can optionally specify a colour or leave null for any colour.
CREATE TABLE "BMC"."UserWishlistItem"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userCollectionId" INT NOT NULL,		-- The collection this wishlist item is associated with
	"brickPartId" INT NOT NULL,		-- The desired part
	"brickColourId" INT NULL,		-- The desired colour (null = any colour)
	"quantityDesired" INT NULL,		-- Number of this part the user wants to acquire
	"notes" TEXT NULL,		-- Free-form notes about the wishlist item (e.g. source, priority)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "userCollectionId" FOREIGN KEY ("userCollectionId") REFERENCES "BMC"."UserCollection"("id"),		-- Foreign key to the UserCollection table.
	CONSTRAINT "brickPartId" FOREIGN KEY ("brickPartId") REFERENCES "BMC"."BrickPart"("id"),		-- Foreign key to the BrickPart table.
	CONSTRAINT "brickColourId" FOREIGN KEY ("brickColourId") REFERENCES "BMC"."BrickColour"("id")		-- Foreign key to the BrickColour table.
);
-- Index on the UserWishlistItem table's tenantGuid field.
CREATE INDEX "I_UserWishlistItem_tenantGuid" ON "BMC"."UserWishlistItem" ("tenantGuid")
;

-- Index on the UserWishlistItem table's tenantGuid,userCollectionId fields.
CREATE INDEX "I_UserWishlistItem_tenantGuid_userCollectionId" ON "BMC"."UserWishlistItem" ("tenantGuid", "userCollectionId")
;

-- Index on the UserWishlistItem table's tenantGuid,brickPartId fields.
CREATE INDEX "I_UserWishlistItem_tenantGuid_brickPartId" ON "BMC"."UserWishlistItem" ("tenantGuid", "brickPartId")
;

-- Index on the UserWishlistItem table's tenantGuid,brickColourId fields.
CREATE INDEX "I_UserWishlistItem_tenantGuid_brickColourId" ON "BMC"."UserWishlistItem" ("tenantGuid", "brickColourId")
;

-- Index on the UserWishlistItem table's tenantGuid,active fields.
CREATE INDEX "I_UserWishlistItem_tenantGuid_active" ON "BMC"."UserWishlistItem" ("tenantGuid", "active")
;

-- Index on the UserWishlistItem table's tenantGuid,deleted fields.
CREATE INDEX "I_UserWishlistItem_tenantGuid_deleted" ON "BMC"."UserWishlistItem" ("tenantGuid", "deleted")
;


-- Tracks which official LEGO sets have been imported into a user's collection, with quantity (e.g. 2 copies of set 42131).
CREATE TABLE "BMC"."UserCollectionSetImport"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userCollectionId" INT NOT NULL,		-- The collection the set was imported into
	"legoSetId" INT NOT NULL,		-- The set that was imported
	"quantity" INT NULL,		-- Number of copies of this set imported (e.g. user owns 2 copies)
	"importedDate" TIMESTAMP NULL,		-- Date/time the set was imported into the collection
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "userCollectionId" FOREIGN KEY ("userCollectionId") REFERENCES "BMC"."UserCollection"("id"),		-- Foreign key to the UserCollection table.
	CONSTRAINT "legoSetId" FOREIGN KEY ("legoSetId") REFERENCES "BMC"."LegoSet"("id"),		-- Foreign key to the LegoSet table.
	CONSTRAINT "UC_UserCollectionSetImport_tenantGuid_userCollectionId_legoSetId" UNIQUE ( "tenantGuid", "userCollectionId", "legoSetId") 		-- Uniqueness enforced on the UserCollectionSetImport table's tenantGuid and userCollectionId and legoSetId fields.
);
-- Index on the UserCollectionSetImport table's tenantGuid field.
CREATE INDEX "I_UserCollectionSetImport_tenantGuid" ON "BMC"."UserCollectionSetImport" ("tenantGuid")
;

-- Index on the UserCollectionSetImport table's tenantGuid,userCollectionId fields.
CREATE INDEX "I_UserCollectionSetImport_tenantGuid_userCollectionId" ON "BMC"."UserCollectionSetImport" ("tenantGuid", "userCollectionId")
;

-- Index on the UserCollectionSetImport table's tenantGuid,legoSetId fields.
CREATE INDEX "I_UserCollectionSetImport_tenantGuid_legoSetId" ON "BMC"."UserCollectionSetImport" ("tenantGuid", "legoSetId")
;

-- Index on the UserCollectionSetImport table's tenantGuid,active fields.
CREATE INDEX "I_UserCollectionSetImport_tenantGuid_active" ON "BMC"."UserCollectionSetImport" ("tenantGuid", "active")
;

-- Index on the UserCollectionSetImport table's tenantGuid,deleted fields.
CREATE INDEX "I_UserCollectionSetImport_tenantGuid_deleted" ON "BMC"."UserCollectionSetImport" ("tenantGuid", "deleted")
;


-- Stores each user's Rebrickable credentials/token and sync configuration. One link per tenant. Supports three auth modes: ApiToken, EncryptedCredentials, SessionOnly.
CREATE TABLE "BMC"."RebrickableUserLink"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"rebrickableUsername" VARCHAR(100) NOT NULL,		-- User's Rebrickable username for display and reference
	"encryptedApiToken" VARCHAR(500) NOT NULL,		-- Encrypted Rebrickable user token — used for API calls on behalf of the user
	"authMode" VARCHAR(50) NOT NULL,		-- Auth trust level: ApiToken, EncryptedCredentials, SessionOnly
	"encryptedPassword" VARCHAR(500) NULL,		-- Encrypted Rebrickable password — only used in EncryptedCredentials auth mode (null otherwise)
	"syncEnabled" BOOLEAN NOT NULL DEFAULT true,		-- Whether automatic sync is enabled for this user
	"syncDirectionFlags" VARCHAR(50) NOT NULL,		-- Integration mode: None, RealTime, PushOnly, ImportOnly
	"pullIntervalMinutes" INT NULL,		-- Periodic pull interval in minutes for RealTime mode (null = manual only)
	"lastSyncDate" TIMESTAMP NULL,		-- Date/time of last successful sync with Rebrickable (legacy — kept for compatibility)
	"lastPullDate" TIMESTAMP NULL,		-- Date/time of last successful pull from Rebrickable
	"lastPushDate" TIMESTAMP NULL,		-- Date/time of last successful push to Rebrickable
	"lastSyncError" TEXT NULL,		-- Last sync error message for display to the user (null = no error)
	"tokenExpiryDays" INT NULL,		-- User-configurable auto-clear interval in days (null = never auto-clear)
	"tokenStoredDate" TIMESTAMP NULL,		-- When the token was last stored or refreshed — used with tokenExpiryDays for auto-expiry
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_RebrickableUserLink_tenantGuid" UNIQUE ( "tenantGuid") 		-- Uniqueness enforced on the RebrickableUserLink table's tenantGuid field.
);
-- Index on the RebrickableUserLink table's tenantGuid field.
CREATE INDEX "I_RebrickableUserLink_tenantGuid" ON "BMC"."RebrickableUserLink" ("tenantGuid")
;

-- Index on the RebrickableUserLink table's tenantGuid,active fields.
CREATE INDEX "I_RebrickableUserLink_tenantGuid_active" ON "BMC"."RebrickableUserLink" ("tenantGuid", "active")
;

-- Index on the RebrickableUserLink table's tenantGuid,deleted fields.
CREATE INDEX "I_RebrickableUserLink_tenantGuid_deleted" ON "BMC"."RebrickableUserLink" ("tenantGuid", "deleted")
;


-- Full audit log of every Rebrickable API call BMC makes on behalf of a user. Enables the Communications Panel for total transparency. Every push, pull, login, and error is recorded.
CREATE TABLE "BMC"."RebrickableTransaction"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"transactionDate" TIMESTAMP NULL,		-- Date/time the API call was made
	"direction" VARCHAR(50) NOT NULL,		-- Direction of data flow: Push, Pull
	"httpMethod" VARCHAR(50) NOT NULL,		-- HTTP method used: GET, POST, PUT, PATCH, DELETE
	"endpoint" VARCHAR(500) NOT NULL,		-- The Rebrickable API URL that was called
	"requestSummary" TEXT NULL,		-- Human-readable description of the operation, e.g. 'Added set 42131-1 x1'
	"responseStatusCode" INT NULL,		-- HTTP status code returned by Rebrickable
	"responseBody" TEXT NULL,		-- Raw response body from Rebrickable (for debugging — may be null for large responses)
	"success" BOOLEAN NOT NULL DEFAULT true,		-- Whether the API call completed successfully
	"errorMessage" TEXT NULL,		-- Error details if the call failed (null on success)
	"triggeredBy" VARCHAR(100) NOT NULL,		-- What initiated this call: UserAction, PeriodicSync, ManualPull, SessionLogin
	"recordCount" INT NULL,		-- Number of rows retrieved or affected by this API call
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the RebrickableTransaction table's tenantGuid field.
CREATE INDEX "I_RebrickableTransaction_tenantGuid" ON "BMC"."RebrickableTransaction" ("tenantGuid")
;

-- Index on the RebrickableTransaction table's tenantGuid,active fields.
CREATE INDEX "I_RebrickableTransaction_tenantGuid_active" ON "BMC"."RebrickableTransaction" ("tenantGuid", "active")
;

-- Index on the RebrickableTransaction table's tenantGuid,deleted fields.
CREATE INDEX "I_RebrickableTransaction_tenantGuid_deleted" ON "BMC"."RebrickableTransaction" ("tenantGuid", "deleted")
;


-- Outbound sync queue for resilient Rebrickable API writes. When a user modifies collection data locally, a queue entry is created. A background service picks up pending items and pushes them to Rebrickable. Retries on failure with exponential backoff.
CREATE TABLE "BMC"."RebrickableSyncQueue"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"operationType" VARCHAR(50) NOT NULL,		-- Sync operation: Create, Update, Delete
	"entityType" VARCHAR(50) NOT NULL,		-- Entity being synced: SetList, SetListItem, PartList, PartListItem, LostPart
	"entityId" BIGINT NOT NULL,		-- Database ID of the entity being synced
	"payload" TEXT NULL,		-- JSON-serialized data needed by the Rebrickable API call
	"status" VARCHAR(50) NOT NULL,		-- Queue status: Pending, InProgress, Completed, Failed, Abandoned
	"createdDate" TIMESTAMP NULL,		-- When this queue entry was created
	"lastAttemptDate" TIMESTAMP NULL,		-- When the last processing attempt occurred (null = never attempted)
	"completedDate" TIMESTAMP NULL,		-- When processing completed successfully (null = not yet completed)
	"attemptCount" INT NOT NULL,		-- Number of processing attempts so far
	"maxAttempts" INT NOT NULL,		-- Maximum retry attempts before marking as Abandoned (default: 5)
	"errorMessage" TEXT NULL,		-- Last error message from a failed attempt (null on success)
	"responseBody" TEXT NULL,		-- Last response body from Rebrickable for debugging (null on success)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the RebrickableSyncQueue table's tenantGuid field.
CREATE INDEX "I_RebrickableSyncQueue_tenantGuid" ON "BMC"."RebrickableSyncQueue" ("tenantGuid")
;

-- Index on the RebrickableSyncQueue table's tenantGuid,active fields.
CREATE INDEX "I_RebrickableSyncQueue_tenantGuid_active" ON "BMC"."RebrickableSyncQueue" ("tenantGuid", "active")
;

-- Index on the RebrickableSyncQueue table's tenantGuid,deleted fields.
CREATE INDEX "I_RebrickableSyncQueue_tenantGuid_deleted" ON "BMC"."RebrickableSyncQueue" ("tenantGuid", "deleted")
;


-- Stores each user's BrickSet userHash and sync configuration. One link per tenant. BrickSet auth uses an app-level API key (in appsettings.json) plus a session-based userHash obtained via login.
CREATE TABLE "BMC"."BrickSetUserLink"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"brickSetUsername" VARCHAR(100) NOT NULL,		-- User's BrickSet username for display and reference
	"encryptedUserHash" VARCHAR(500) NOT NULL,		-- Encrypted BrickSet userHash — obtained via login, used for collection API calls
	"encryptedPassword" VARCHAR(500) NULL,		-- Encrypted BrickSet password — stored for re-authentication when userHash expires (null if user chose not to store)
	"syncEnabled" BOOLEAN NOT NULL DEFAULT true,		-- Whether automatic sync is enabled for this user
	"syncDirection" VARCHAR(50) NOT NULL,		-- Integration mode: None, PullOnly, PushOnly, Bidirectional
	"lastSyncDate" TIMESTAMP NULL,		-- Date/time of last successful sync with BrickSet
	"lastPullDate" TIMESTAMP NULL,		-- Date/time of last successful pull from BrickSet
	"lastPushDate" TIMESTAMP NULL,		-- Date/time of last successful push to BrickSet
	"lastSyncError" TEXT NULL,		-- Last sync error message for display to the user (null = no error)
	"userHashStoredDate" TIMESTAMP NULL,		-- When the userHash was last stored or refreshed — used for session expiry tracking
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_BrickSetUserLink_tenantGuid" UNIQUE ( "tenantGuid") 		-- Uniqueness enforced on the BrickSetUserLink table's tenantGuid field.
);
-- Index on the BrickSetUserLink table's tenantGuid field.
CREATE INDEX "I_BrickSetUserLink_tenantGuid" ON "BMC"."BrickSetUserLink" ("tenantGuid")
;

-- Index on the BrickSetUserLink table's tenantGuid,active fields.
CREATE INDEX "I_BrickSetUserLink_tenantGuid_active" ON "BMC"."BrickSetUserLink" ("tenantGuid", "active")
;

-- Index on the BrickSetUserLink table's tenantGuid,deleted fields.
CREATE INDEX "I_BrickSetUserLink_tenantGuid_deleted" ON "BMC"."BrickSetUserLink" ("tenantGuid", "deleted")
;


-- Full audit log of every BrickSet API call BMC makes on behalf of a user. Mirrors the RebrickableTransaction pattern for complete transparency.
CREATE TABLE "BMC"."BrickSetTransaction"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"transactionDate" TIMESTAMP NULL,		-- Date/time the API call was made
	"direction" VARCHAR(50) NOT NULL,		-- Direction of data flow: Push, Pull, Enrich
	"methodName" VARCHAR(100) NOT NULL,		-- BrickSet API method name (e.g. 'getSets', 'setCollection', 'getInstructions')
	"requestSummary" TEXT NULL,		-- Human-readable description of the operation, e.g. 'Enriched set 42131-1 with pricing data'
	"success" BOOLEAN NOT NULL DEFAULT true,		-- Whether the API call completed successfully
	"errorMessage" TEXT NULL,		-- Error details if the call failed (null on success)
	"triggeredBy" VARCHAR(100) NOT NULL,		-- What initiated this call: UserAction, SetDetailView, ManualEnrich, CollectionSync
	"recordCount" INT NULL,		-- Number of rows retrieved or affected by this API call
	"apiCallsRemaining" INT NULL,		-- Daily API call quota remaining after this call (from getKeyUsageStats)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the BrickSetTransaction table's tenantGuid field.
CREATE INDEX "I_BrickSetTransaction_tenantGuid" ON "BMC"."BrickSetTransaction" ("tenantGuid")
;

-- Index on the BrickSetTransaction table's tenantGuid,active fields.
CREATE INDEX "I_BrickSetTransaction_tenantGuid_active" ON "BMC"."BrickSetTransaction" ("tenantGuid", "active")
;

-- Index on the BrickSetTransaction table's tenantGuid,deleted fields.
CREATE INDEX "I_BrickSetTransaction_tenantGuid_deleted" ON "BMC"."BrickSetTransaction" ("tenantGuid", "deleted")
;


-- Cached community reviews from BrickSet for official LEGO sets. Pulled periodically via the getReviews API method. Reviews are read-only reference data, not user-editable.
CREATE TABLE "BMC"."BrickSetSetReview"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"legoSetId" INT NOT NULL,		-- The set this review is for
	"reviewAuthor" VARCHAR(100) NOT NULL,		-- BrickSet username of the reviewer
	"reviewDate" TIMESTAMP NULL,		-- When the review was posted on BrickSet
	"reviewTitle" TEXT NULL,		-- Review title/heading
	"reviewBody" TEXT NULL,		-- Full review text
	"overallRating" INT NULL,		-- Overall rating (1-5)
	"buildingExperienceRating" INT NULL,		-- Building experience rating (1-5, null if not rated)
	"valueForMoneyRating" INT NULL,		-- Value for money rating (1-5, null if not rated)
	"partsRating" INT NULL,		-- Parts/pieces rating (1-5, null if not rated)
	"playabilityRating" INT NULL,		-- Playability rating (1-5, null if not rated)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "legoSetId" FOREIGN KEY ("legoSetId") REFERENCES "BMC"."LegoSet"("id")		-- Foreign key to the LegoSet table.
);
-- Index on the BrickSetSetReview table's legoSetId field.
CREATE INDEX "I_BrickSetSetReview_legoSetId" ON "BMC"."BrickSetSetReview" ("legoSetId")
;

-- Index on the BrickSetSetReview table's active field.
CREATE INDEX "I_BrickSetSetReview_active" ON "BMC"."BrickSetSetReview" ("active")
;

-- Index on the BrickSetSetReview table's deleted field.
CREATE INDEX "I_BrickSetSetReview_deleted" ON "BMC"."BrickSetSetReview" ("deleted")
;


-- Named part lists, mirroring Rebrickable's partlists/ endpoint. Users can have multiple named lists for organizing parts.
CREATE TABLE "BMC"."UserPartList"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"isBuildable" BOOLEAN NOT NULL DEFAULT false,		-- Whether this list represents buildable parts (for build matching)
	"rebrickableListId" INT NULL,		-- Rebrickable list ID for bidirectional sync (null = BMC-only list)
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_UserPartList_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the UserPartList table's tenantGuid and name fields.
);
-- Index on the UserPartList table's tenantGuid field.
CREATE INDEX "I_UserPartList_tenantGuid" ON "BMC"."UserPartList" ("tenantGuid")
;

-- Index on the UserPartList table's tenantGuid,name fields.
CREATE INDEX "I_UserPartList_tenantGuid_name" ON "BMC"."UserPartList" ("tenantGuid", "name")
;

-- Index on the UserPartList table's tenantGuid,active fields.
CREATE INDEX "I_UserPartList_tenantGuid_active" ON "BMC"."UserPartList" ("tenantGuid", "active")
;

-- Index on the UserPartList table's tenantGuid,deleted fields.
CREATE INDEX "I_UserPartList_tenantGuid_deleted" ON "BMC"."UserPartList" ("tenantGuid", "deleted")
;


-- The change history for records from the UserPartList table.
CREATE TABLE "BMC"."UserPartListChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userPartListId" INT NOT NULL,		-- Link to the UserPartList table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "userPartListId" FOREIGN KEY ("userPartListId") REFERENCES "BMC"."UserPartList"("id")		-- Foreign key to the UserPartList table.
);
-- Index on the UserPartListChangeHistory table's tenantGuid field.
CREATE INDEX "I_UserPartListChangeHistory_tenantGuid" ON "BMC"."UserPartListChangeHistory" ("tenantGuid")
;

-- Index on the UserPartListChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_UserPartListChangeHistory_tenantGuid_versionNumber" ON "BMC"."UserPartListChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the UserPartListChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_UserPartListChangeHistory_tenantGuid_timeStamp" ON "BMC"."UserPartListChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the UserPartListChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_UserPartListChangeHistory_tenantGuid_userId" ON "BMC"."UserPartListChangeHistory" ("tenantGuid", "userId")
;

-- Index on the UserPartListChangeHistory table's tenantGuid,userPartListId fields.
CREATE INDEX "I_UserPartListChangeHistory_tenantGuid_userPartListId" ON "BMC"."UserPartListChangeHistory" ("tenantGuid", "userPartListId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Individual part+colour entries within a user's named part list. Mirrors Rebrickable's partlists/{id}/parts/ endpoint.
CREATE TABLE "BMC"."UserPartListItem"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userPartListId" INT NOT NULL,		-- The part list this item belongs to
	"brickPartId" INT NOT NULL,		-- The part definition
	"brickColourId" INT NOT NULL,		-- The specific colour of this part
	"quantity" INT NOT NULL,		-- Number of this part+colour in the list
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "userPartListId" FOREIGN KEY ("userPartListId") REFERENCES "BMC"."UserPartList"("id"),		-- Foreign key to the UserPartList table.
	CONSTRAINT "brickPartId" FOREIGN KEY ("brickPartId") REFERENCES "BMC"."BrickPart"("id"),		-- Foreign key to the BrickPart table.
	CONSTRAINT "brickColourId" FOREIGN KEY ("brickColourId") REFERENCES "BMC"."BrickColour"("id"),		-- Foreign key to the BrickColour table.
	CONSTRAINT "UC_UserPartListItem_tenantGuid_userPartListId_brickPartId_brickColourId" UNIQUE ( "tenantGuid", "userPartListId", "brickPartId", "brickColourId") 		-- Uniqueness enforced on the UserPartListItem table's tenantGuid and userPartListId and brickPartId and brickColourId fields.
);
-- Index on the UserPartListItem table's tenantGuid field.
CREATE INDEX "I_UserPartListItem_tenantGuid" ON "BMC"."UserPartListItem" ("tenantGuid")
;

-- Index on the UserPartListItem table's tenantGuid,userPartListId fields.
CREATE INDEX "I_UserPartListItem_tenantGuid_userPartListId" ON "BMC"."UserPartListItem" ("tenantGuid", "userPartListId")
;

-- Index on the UserPartListItem table's tenantGuid,brickPartId fields.
CREATE INDEX "I_UserPartListItem_tenantGuid_brickPartId" ON "BMC"."UserPartListItem" ("tenantGuid", "brickPartId")
;

-- Index on the UserPartListItem table's tenantGuid,brickColourId fields.
CREATE INDEX "I_UserPartListItem_tenantGuid_brickColourId" ON "BMC"."UserPartListItem" ("tenantGuid", "brickColourId")
;

-- Index on the UserPartListItem table's tenantGuid,active fields.
CREATE INDEX "I_UserPartListItem_tenantGuid_active" ON "BMC"."UserPartListItem" ("tenantGuid", "active")
;

-- Index on the UserPartListItem table's tenantGuid,deleted fields.
CREATE INDEX "I_UserPartListItem_tenantGuid_deleted" ON "BMC"."UserPartListItem" ("tenantGuid", "deleted")
;


-- Named set lists, mirroring Rebrickable's setlists/ endpoint. Users can have multiple named lists for organizing sets.
CREATE TABLE "BMC"."UserSetList"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"isBuildable" BOOLEAN NOT NULL DEFAULT false,		-- Whether this list represents buildable sets (for build matching)
	"rebrickableListId" INT NULL,		-- Rebrickable list ID for bidirectional sync (null = BMC-only list)
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_UserSetList_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the UserSetList table's tenantGuid and name fields.
);
-- Index on the UserSetList table's tenantGuid field.
CREATE INDEX "I_UserSetList_tenantGuid" ON "BMC"."UserSetList" ("tenantGuid")
;

-- Index on the UserSetList table's tenantGuid,name fields.
CREATE INDEX "I_UserSetList_tenantGuid_name" ON "BMC"."UserSetList" ("tenantGuid", "name")
;

-- Index on the UserSetList table's tenantGuid,active fields.
CREATE INDEX "I_UserSetList_tenantGuid_active" ON "BMC"."UserSetList" ("tenantGuid", "active")
;

-- Index on the UserSetList table's tenantGuid,deleted fields.
CREATE INDEX "I_UserSetList_tenantGuid_deleted" ON "BMC"."UserSetList" ("tenantGuid", "deleted")
;


-- The change history for records from the UserSetList table.
CREATE TABLE "BMC"."UserSetListChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userSetListId" INT NOT NULL,		-- Link to the UserSetList table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "userSetListId" FOREIGN KEY ("userSetListId") REFERENCES "BMC"."UserSetList"("id")		-- Foreign key to the UserSetList table.
);
-- Index on the UserSetListChangeHistory table's tenantGuid field.
CREATE INDEX "I_UserSetListChangeHistory_tenantGuid" ON "BMC"."UserSetListChangeHistory" ("tenantGuid")
;

-- Index on the UserSetListChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_UserSetListChangeHistory_tenantGuid_versionNumber" ON "BMC"."UserSetListChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the UserSetListChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_UserSetListChangeHistory_tenantGuid_timeStamp" ON "BMC"."UserSetListChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the UserSetListChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_UserSetListChangeHistory_tenantGuid_userId" ON "BMC"."UserSetListChangeHistory" ("tenantGuid", "userId")
;

-- Index on the UserSetListChangeHistory table's tenantGuid,userSetListId fields.
CREATE INDEX "I_UserSetListChangeHistory_tenantGuid_userSetListId" ON "BMC"."UserSetListChangeHistory" ("tenantGuid", "userSetListId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Individual set entries within a user's named set list. Mirrors Rebrickable's setlists/{id}/sets/ endpoint.
CREATE TABLE "BMC"."UserSetListItem"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userSetListId" INT NOT NULL,		-- The set list this item belongs to
	"legoSetId" INT NOT NULL,		-- The set in this list
	"quantity" INT NOT NULL DEFAULT 1,		-- Number of copies of this set in the list
	"includeSpares" BOOLEAN NOT NULL DEFAULT true,		-- Whether to include spare parts from this set in build matching
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "userSetListId" FOREIGN KEY ("userSetListId") REFERENCES "BMC"."UserSetList"("id"),		-- Foreign key to the UserSetList table.
	CONSTRAINT "legoSetId" FOREIGN KEY ("legoSetId") REFERENCES "BMC"."LegoSet"("id"),		-- Foreign key to the LegoSet table.
	CONSTRAINT "UC_UserSetListItem_tenantGuid_userSetListId_legoSetId" UNIQUE ( "tenantGuid", "userSetListId", "legoSetId") 		-- Uniqueness enforced on the UserSetListItem table's tenantGuid and userSetListId and legoSetId fields.
);
-- Index on the UserSetListItem table's tenantGuid field.
CREATE INDEX "I_UserSetListItem_tenantGuid" ON "BMC"."UserSetListItem" ("tenantGuid")
;

-- Index on the UserSetListItem table's tenantGuid,userSetListId fields.
CREATE INDEX "I_UserSetListItem_tenantGuid_userSetListId" ON "BMC"."UserSetListItem" ("tenantGuid", "userSetListId")
;

-- Index on the UserSetListItem table's tenantGuid,legoSetId fields.
CREATE INDEX "I_UserSetListItem_tenantGuid_legoSetId" ON "BMC"."UserSetListItem" ("tenantGuid", "legoSetId")
;

-- Index on the UserSetListItem table's tenantGuid,active fields.
CREATE INDEX "I_UserSetListItem_tenantGuid_active" ON "BMC"."UserSetListItem" ("tenantGuid", "active")
;

-- Index on the UserSetListItem table's tenantGuid,deleted fields.
CREATE INDEX "I_UserSetListItem_tenantGuid_deleted" ON "BMC"."UserSetListItem" ("tenantGuid", "deleted")
;


-- Parts lost from sets, mirroring Rebrickable's lost_parts/ endpoint. Tracks which parts are missing from a user's collection.
CREATE TABLE "BMC"."UserLostPart"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"brickPartId" INT NOT NULL,		-- The part that was lost
	"brickColourId" INT NOT NULL,		-- The colour of the lost part
	"legoSetId" INT NULL,		-- The set the part was lost from (null if unknown)
	"lostQuantity" INT NOT NULL,		-- Number of this part+colour that were lost
	"rebrickableInvPartId" INT NULL,		-- Rebrickable inventory_part ID for bidirectional sync (null = BMC-only entry)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "brickPartId" FOREIGN KEY ("brickPartId") REFERENCES "BMC"."BrickPart"("id"),		-- Foreign key to the BrickPart table.
	CONSTRAINT "brickColourId" FOREIGN KEY ("brickColourId") REFERENCES "BMC"."BrickColour"("id"),		-- Foreign key to the BrickColour table.
	CONSTRAINT "legoSetId" FOREIGN KEY ("legoSetId") REFERENCES "BMC"."LegoSet"("id")		-- Foreign key to the LegoSet table.
);
-- Index on the UserLostPart table's tenantGuid field.
CREATE INDEX "I_UserLostPart_tenantGuid" ON "BMC"."UserLostPart" ("tenantGuid")
;

-- Index on the UserLostPart table's tenantGuid,brickPartId fields.
CREATE INDEX "I_UserLostPart_tenantGuid_brickPartId" ON "BMC"."UserLostPart" ("tenantGuid", "brickPartId")
;

-- Index on the UserLostPart table's tenantGuid,brickColourId fields.
CREATE INDEX "I_UserLostPart_tenantGuid_brickColourId" ON "BMC"."UserLostPart" ("tenantGuid", "brickColourId")
;

-- Index on the UserLostPart table's tenantGuid,legoSetId fields.
CREATE INDEX "I_UserLostPart_tenantGuid_legoSetId" ON "BMC"."UserLostPart" ("tenantGuid", "legoSetId")
;

-- Index on the UserLostPart table's tenantGuid,active fields.
CREATE INDEX "I_UserLostPart_tenantGuid_active" ON "BMC"."UserLostPart" ("tenantGuid", "active")
;

-- Index on the UserLostPart table's tenantGuid,deleted fields.
CREATE INDEX "I_UserLostPart_tenantGuid_deleted" ON "BMC"."UserLostPart" ("tenantGuid", "deleted")
;


-- A complete instruction booklet for a building project. A project can have multiple manuals (e.g. one per bag/booklet).
CREATE TABLE "BMC"."BuildManual"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"projectId" INT NOT NULL,		-- The project this manual documents
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"pageWidthMm" REAL NULL,		-- Page width in millimetres for PDF/print layout (e.g. 210 for A4)
	"pageHeightMm" REAL NULL,		-- Page height in millimetres for PDF/print layout (e.g. 297 for A4)
	"isPublished" BOOLEAN NOT NULL DEFAULT false,		-- Whether this manual is marked as published/final
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "projectId" FOREIGN KEY ("projectId") REFERENCES "BMC"."Project"("id"),		-- Foreign key to the Project table.
	CONSTRAINT "UC_BuildManual_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the BuildManual table's tenantGuid and name fields.
);
-- Index on the BuildManual table's tenantGuid field.
CREATE INDEX "I_BuildManual_tenantGuid" ON "BMC"."BuildManual" ("tenantGuid")
;

-- Index on the BuildManual table's tenantGuid,projectId fields.
CREATE INDEX "I_BuildManual_tenantGuid_projectId" ON "BMC"."BuildManual" ("tenantGuid", "projectId")
;

-- Index on the BuildManual table's tenantGuid,name fields.
CREATE INDEX "I_BuildManual_tenantGuid_name" ON "BMC"."BuildManual" ("tenantGuid", "name")
;

-- Index on the BuildManual table's tenantGuid,active fields.
CREATE INDEX "I_BuildManual_tenantGuid_active" ON "BMC"."BuildManual" ("tenantGuid", "active")
;

-- Index on the BuildManual table's tenantGuid,deleted fields.
CREATE INDEX "I_BuildManual_tenantGuid_deleted" ON "BMC"."BuildManual" ("tenantGuid", "deleted")
;


-- The change history for records from the BuildManual table.
CREATE TABLE "BMC"."BuildManualChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"buildManualId" INT NOT NULL,		-- Link to the BuildManual table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "buildManualId" FOREIGN KEY ("buildManualId") REFERENCES "BMC"."BuildManual"("id")		-- Foreign key to the BuildManual table.
);
-- Index on the BuildManualChangeHistory table's tenantGuid field.
CREATE INDEX "I_BuildManualChangeHistory_tenantGuid" ON "BMC"."BuildManualChangeHistory" ("tenantGuid")
;

-- Index on the BuildManualChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_BuildManualChangeHistory_tenantGuid_versionNumber" ON "BMC"."BuildManualChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the BuildManualChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_BuildManualChangeHistory_tenantGuid_timeStamp" ON "BMC"."BuildManualChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the BuildManualChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_BuildManualChangeHistory_tenantGuid_userId" ON "BMC"."BuildManualChangeHistory" ("tenantGuid", "userId")
;

-- Index on the BuildManualChangeHistory table's tenantGuid,buildManualId fields.
CREATE INDEX "I_BuildManualChangeHistory_tenantGuid_buildManualId" ON "BMC"."BuildManualChangeHistory" ("tenantGuid", "buildManualId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- A single page within a build manual. Contains one or more build steps.
CREATE TABLE "BMC"."BuildManualPage"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"buildManualId" INT NOT NULL,		-- The manual this page belongs to
	"pageNum" INT NULL,		-- Sequential page number within the manual.  Note purposely not called pageNumber to not clash with code generated parameter
	"title" VARCHAR(250) NULL,		-- Optional page title (e.g. 'Bag 1', 'Chassis Assembly')
	"notes" TEXT NULL,		-- Optional internal notes about this page
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "buildManualId" FOREIGN KEY ("buildManualId") REFERENCES "BMC"."BuildManual"("id")		-- Foreign key to the BuildManual table.
);
-- Index on the BuildManualPage table's tenantGuid field.
CREATE INDEX "I_BuildManualPage_tenantGuid" ON "BMC"."BuildManualPage" ("tenantGuid")
;

-- Index on the BuildManualPage table's tenantGuid,buildManualId fields.
CREATE INDEX "I_BuildManualPage_tenantGuid_buildManualId" ON "BMC"."BuildManualPage" ("tenantGuid", "buildManualId")
;

-- Index on the BuildManualPage table's tenantGuid,active fields.
CREATE INDEX "I_BuildManualPage_tenantGuid_active" ON "BMC"."BuildManualPage" ("tenantGuid", "active")
;

-- Index on the BuildManualPage table's tenantGuid,deleted fields.
CREATE INDEX "I_BuildManualPage_tenantGuid_deleted" ON "BMC"."BuildManualPage" ("tenantGuid", "deleted")
;


-- A single build step within a manual page. Defines the camera angle and display options for that step's rendered view.
CREATE TABLE "BMC"."BuildManualStep"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"buildManualPageId" INT NOT NULL,		-- The page this step appears on
	"stepNumber" INT NULL,		-- Sequential step number within the page
	"cameraPositionX" REAL NULL,		-- Camera X position for this step's rendered view
	"cameraPositionY" REAL NULL,		-- Camera Y position for this step's rendered view
	"cameraPositionZ" REAL NULL,		-- Camera Z position for this step's rendered view
	"cameraTargetX" REAL NULL,		-- Camera look-at target X for this step
	"cameraTargetY" REAL NULL,		-- Camera look-at target Y for this step
	"cameraTargetZ" REAL NULL,		-- Camera look-at target Z for this step
	"cameraZoom" REAL NULL,		-- Camera zoom / field of view for this step
	"showExplodedView" BOOLEAN NOT NULL DEFAULT false,		-- Whether to render the step with newly-added parts pulled apart for clarity
	"explodedDistance" REAL NULL,		-- Distance in LDU to pull apart exploded parts (null = use default)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "buildManualPageId" FOREIGN KEY ("buildManualPageId") REFERENCES "BMC"."BuildManualPage"("id")		-- Foreign key to the BuildManualPage table.
);
-- Index on the BuildManualStep table's tenantGuid field.
CREATE INDEX "I_BuildManualStep_tenantGuid" ON "BMC"."BuildManualStep" ("tenantGuid")
;

-- Index on the BuildManualStep table's tenantGuid,buildManualPageId fields.
CREATE INDEX "I_BuildManualStep_tenantGuid_buildManualPageId" ON "BMC"."BuildManualStep" ("tenantGuid", "buildManualPageId")
;

-- Index on the BuildManualStep table's tenantGuid,active fields.
CREATE INDEX "I_BuildManualStep_tenantGuid_active" ON "BMC"."BuildManualStep" ("tenantGuid", "active")
;

-- Index on the BuildManualStep table's tenantGuid,deleted fields.
CREATE INDEX "I_BuildManualStep_tenantGuid_deleted" ON "BMC"."BuildManualStep" ("tenantGuid", "deleted")
;


-- Maps which placed bricks are added during a specific build step. Links to the actual PlacedBrick in the project.
CREATE TABLE "BMC"."BuildStepPart"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"buildManualStepId" INT NOT NULL,		-- The build step this part is added during
	"placedBrickId" INT NOT NULL,		-- The placed brick in the project that is added in this step
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "buildManualStepId" FOREIGN KEY ("buildManualStepId") REFERENCES "BMC"."BuildManualStep"("id"),		-- Foreign key to the BuildManualStep table.
	CONSTRAINT "placedBrickId" FOREIGN KEY ("placedBrickId") REFERENCES "BMC"."PlacedBrick"("id")		-- Foreign key to the PlacedBrick table.
);
-- Index on the BuildStepPart table's tenantGuid field.
CREATE INDEX "I_BuildStepPart_tenantGuid" ON "BMC"."BuildStepPart" ("tenantGuid")
;

-- Index on the BuildStepPart table's tenantGuid,buildManualStepId fields.
CREATE INDEX "I_BuildStepPart_tenantGuid_buildManualStepId" ON "BMC"."BuildStepPart" ("tenantGuid", "buildManualStepId")
;

-- Index on the BuildStepPart table's tenantGuid,placedBrickId fields.
CREATE INDEX "I_BuildStepPart_tenantGuid_placedBrickId" ON "BMC"."BuildStepPart" ("tenantGuid", "placedBrickId")
;

-- Index on the BuildStepPart table's tenantGuid,active fields.
CREATE INDEX "I_BuildStepPart_tenantGuid_active" ON "BMC"."BuildStepPart" ("tenantGuid", "active")
;

-- Index on the BuildStepPart table's tenantGuid,deleted fields.
CREATE INDEX "I_BuildStepPart_tenantGuid_deleted" ON "BMC"."BuildStepPart" ("tenantGuid", "deleted")
;


-- Lookup table of annotation types available for build steps (Arrow, Callout, Label, Quantity Callout, Submodel Callout).
CREATE TABLE "BMC"."BuildStepAnnotationType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the BuildStepAnnotationType table's name field.
CREATE INDEX "I_BuildStepAnnotationType_name" ON "BMC"."BuildStepAnnotationType" ("name")
;

-- Index on the BuildStepAnnotationType table's active field.
CREATE INDEX "I_BuildStepAnnotationType_active" ON "BMC"."BuildStepAnnotationType" ("active")
;

-- Index on the BuildStepAnnotationType table's deleted field.
CREATE INDEX "I_BuildStepAnnotationType_deleted" ON "BMC"."BuildStepAnnotationType" ("deleted")
;

INSERT INTO "BMC"."BuildStepAnnotationType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Arrow', 'Directional arrow indicating placement direction or connection point', 1, 'ba100001-0001-4000-8000-000000000001' );

INSERT INTO "BMC"."BuildStepAnnotationType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Callout', 'Callout box highlighting a sub-assembly built separately', 2, 'ba100001-0001-4000-8000-000000000002' );

INSERT INTO "BMC"."BuildStepAnnotationType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Label', 'Text label providing additional context or instruction', 3, 'ba100001-0001-4000-8000-000000000003' );

INSERT INTO "BMC"."BuildStepAnnotationType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Quantity Callout', 'Quantity indicator showing how many of a part are needed in this step', 4, 'ba100001-0001-4000-8000-000000000004' );

INSERT INTO "BMC"."BuildStepAnnotationType" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Submodel Callout', 'Callout referencing a submodel that should be built as part of this step', 5, 'ba100001-0001-4000-8000-000000000005' );


-- Visual annotations (arrows, callouts, labels) placed on a build step's rendered view.
CREATE TABLE "BMC"."BuildStepAnnotation"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"buildManualStepId" INT NOT NULL,		-- The build step this annotation belongs to
	"buildStepAnnotationTypeId" INT NOT NULL,		-- The type of annotation (Arrow, Callout, Label, etc.)
	"positionX" REAL NULL,		-- X position on the rendered page (normalised 0-1 or pixel coordinates)
	"positionY" REAL NULL,		-- Y position on the rendered page
	"width" REAL NULL,		-- Width of the annotation element (null = auto-size)
	"height" REAL NULL,		-- Height of the annotation element (null = auto-size)
	"text" TEXT NULL,		-- Optional text content for labels and callouts
	"placedBrickId" INT NULL,		-- Optional target placed brick that this annotation points to or highlights
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "buildManualStepId" FOREIGN KEY ("buildManualStepId") REFERENCES "BMC"."BuildManualStep"("id"),		-- Foreign key to the BuildManualStep table.
	CONSTRAINT "buildStepAnnotationTypeId" FOREIGN KEY ("buildStepAnnotationTypeId") REFERENCES "BMC"."BuildStepAnnotationType"("id"),		-- Foreign key to the BuildStepAnnotationType table.
	CONSTRAINT "placedBrickId" FOREIGN KEY ("placedBrickId") REFERENCES "BMC"."PlacedBrick"("id")		-- Foreign key to the PlacedBrick table.
);
-- Index on the BuildStepAnnotation table's tenantGuid field.
CREATE INDEX "I_BuildStepAnnotation_tenantGuid" ON "BMC"."BuildStepAnnotation" ("tenantGuid")
;

-- Index on the BuildStepAnnotation table's tenantGuid,buildManualStepId fields.
CREATE INDEX "I_BuildStepAnnotation_tenantGuid_buildManualStepId" ON "BMC"."BuildStepAnnotation" ("tenantGuid", "buildManualStepId")
;

-- Index on the BuildStepAnnotation table's tenantGuid,buildStepAnnotationTypeId fields.
CREATE INDEX "I_BuildStepAnnotation_tenantGuid_buildStepAnnotationTypeId" ON "BMC"."BuildStepAnnotation" ("tenantGuid", "buildStepAnnotationTypeId")
;

-- Index on the BuildStepAnnotation table's tenantGuid,placedBrickId fields.
CREATE INDEX "I_BuildStepAnnotation_tenantGuid_placedBrickId" ON "BMC"."BuildStepAnnotation" ("tenantGuid", "placedBrickId")
;

-- Index on the BuildStepAnnotation table's tenantGuid,active fields.
CREATE INDEX "I_BuildStepAnnotation_tenantGuid_active" ON "BMC"."BuildStepAnnotation" ("tenantGuid", "active")
;

-- Index on the BuildStepAnnotation table's tenantGuid,deleted fields.
CREATE INDEX "I_BuildStepAnnotation_tenantGuid_deleted" ON "BMC"."BuildStepAnnotation" ("tenantGuid", "deleted")
;


-- Reusable rendering presets that define resolution, lighting, and quality settings for producing images of models.
CREATE TABLE "BMC"."RenderPreset"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL,
	"description" VARCHAR(500) NOT NULL,
	"resolutionWidth" INT NULL,		-- Output image width in pixels
	"resolutionHeight" INT NULL,		-- Output image height in pixels
	"backgroundColorHex" VARCHAR(10) NULL,		-- Background colour in hex (e.g. #FFFFFF for white, #000000 for black)
	"enableShadows" BOOLEAN NOT NULL DEFAULT true,		-- Whether to render drop shadows
	"enableReflections" BOOLEAN NOT NULL DEFAULT false,		-- Whether to render environment reflections on metallic/chrome parts
	"lightingMode" VARCHAR(100) NULL,		-- Lighting preset name: studio, outdoor, dramatic, custom
	"antiAliasLevel" INT NULL,		-- Anti-aliasing level (1=none, 2=2x, 4=4x, 8=8x)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_RenderPreset_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the RenderPreset table's tenantGuid and name fields.
);
-- Index on the RenderPreset table's tenantGuid field.
CREATE INDEX "I_RenderPreset_tenantGuid" ON "BMC"."RenderPreset" ("tenantGuid")
;

-- Index on the RenderPreset table's tenantGuid,name fields.
CREATE INDEX "I_RenderPreset_tenantGuid_name" ON "BMC"."RenderPreset" ("tenantGuid", "name")
;

-- Index on the RenderPreset table's tenantGuid,active fields.
CREATE INDEX "I_RenderPreset_tenantGuid_active" ON "BMC"."RenderPreset" ("tenantGuid", "active")
;

-- Index on the RenderPreset table's tenantGuid,deleted fields.
CREATE INDEX "I_RenderPreset_tenantGuid_deleted" ON "BMC"."RenderPreset" ("tenantGuid", "deleted")
;


-- Records of rendered images produced from a project, with the preset used and output metadata.
CREATE TABLE "BMC"."ProjectRender"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"projectId" INT NOT NULL,		-- The project this render was produced from
	"renderPresetId" INT NULL,		-- The render preset used (null = custom/one-off settings)
	"name" VARCHAR(100) NOT NULL,
	"outputFilePath" VARCHAR(250) NULL,		-- Relative path to the rendered image file
	"resolutionWidth" INT NULL,		-- Actual output width in pixels
	"resolutionHeight" INT NULL,		-- Actual output height in pixels
	"renderedDate" TIMESTAMP NULL,		-- Date/time the render was produced
	"renderDurationSeconds" REAL NULL,		-- Time taken to produce the render in seconds
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "projectId" FOREIGN KEY ("projectId") REFERENCES "BMC"."Project"("id"),		-- Foreign key to the Project table.
	CONSTRAINT "renderPresetId" FOREIGN KEY ("renderPresetId") REFERENCES "BMC"."RenderPreset"("id"),		-- Foreign key to the RenderPreset table.
	CONSTRAINT "UC_ProjectRender_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the ProjectRender table's tenantGuid and name fields.
);
-- Index on the ProjectRender table's tenantGuid field.
CREATE INDEX "I_ProjectRender_tenantGuid" ON "BMC"."ProjectRender" ("tenantGuid")
;

-- Index on the ProjectRender table's tenantGuid,projectId fields.
CREATE INDEX "I_ProjectRender_tenantGuid_projectId" ON "BMC"."ProjectRender" ("tenantGuid", "projectId")
;

-- Index on the ProjectRender table's tenantGuid,renderPresetId fields.
CREATE INDEX "I_ProjectRender_tenantGuid_renderPresetId" ON "BMC"."ProjectRender" ("tenantGuid", "renderPresetId")
;

-- Index on the ProjectRender table's tenantGuid,name fields.
CREATE INDEX "I_ProjectRender_tenantGuid_name" ON "BMC"."ProjectRender" ("tenantGuid", "name")
;

-- Index on the ProjectRender table's tenantGuid,active fields.
CREATE INDEX "I_ProjectRender_tenantGuid_active" ON "BMC"."ProjectRender" ("tenantGuid", "active")
;

-- Index on the ProjectRender table's tenantGuid,deleted fields.
CREATE INDEX "I_ProjectRender_tenantGuid_deleted" ON "BMC"."ProjectRender" ("tenantGuid", "deleted")
;


-- Lookup table of supported file export formats for models, instructions, and parts lists.
CREATE TABLE "BMC"."ExportFormat"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"fileExtension" VARCHAR(50) NULL,		-- File extension including dot (e.g. '.ldr', '.pdf', '.xml')
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the ExportFormat table's name field.
CREATE INDEX "I_ExportFormat_name" ON "BMC"."ExportFormat" ("name")
;

-- Index on the ExportFormat table's active field.
CREATE INDEX "I_ExportFormat_active" ON "BMC"."ExportFormat" ("active")
;

-- Index on the ExportFormat table's deleted field.
CREATE INDEX "I_ExportFormat_deleted" ON "BMC"."ExportFormat" ("deleted")
;

INSERT INTO "BMC"."ExportFormat" ( "name", "description", "fileExtension", "sequence", "objectGuid" ) VALUES  ( 'LDraw', 'LDraw single-model file format', '.ldr', 1, 'ef100001-0001-4000-8000-000000000001' );

INSERT INTO "BMC"."ExportFormat" ( "name", "description", "fileExtension", "sequence", "objectGuid" ) VALUES  ( 'LDraw Multi-Part', 'LDraw multi-part document containing submodels', '.mpd', 2, 'ef100001-0001-4000-8000-000000000002' );

INSERT INTO "BMC"."ExportFormat" ( "name", "description", "fileExtension", "sequence", "objectGuid" ) VALUES  ( 'Wavefront OBJ', 'Wavefront OBJ 3D model format for Blender and other 3D tools', '.obj', 3, 'ef100001-0001-4000-8000-000000000003' );

INSERT INTO "BMC"."ExportFormat" ( "name", "description", "fileExtension", "sequence", "objectGuid" ) VALUES  ( 'COLLADA', 'COLLADA 3D asset exchange format', '.dae', 4, 'ef100001-0001-4000-8000-000000000004' );

INSERT INTO "BMC"."ExportFormat" ( "name", "description", "fileExtension", "sequence", "objectGuid" ) VALUES  ( 'BrickLink XML', 'BrickLink wanted-list XML format for ordering parts', '.xml', 5, 'ef100001-0001-4000-8000-000000000005' );

INSERT INTO "BMC"."ExportFormat" ( "name", "description", "fileExtension", "sequence", "objectGuid" ) VALUES  ( 'Rebrickable CSV', 'Rebrickable-compatible CSV parts list', '.csv', 6, 'ef100001-0001-4000-8000-000000000006' );

INSERT INTO "BMC"."ExportFormat" ( "name", "description", "fileExtension", "sequence", "objectGuid" ) VALUES  ( 'PDF Instructions', 'PDF export of build manual instructions', '.pdf', 7, 'ef100001-0001-4000-8000-000000000007' );

INSERT INTO "BMC"."ExportFormat" ( "name", "description", "fileExtension", "sequence", "objectGuid" ) VALUES  ( 'BrickLink Studio', 'BrickLink Studio project file format', '.io', 8, 'ef100001-0001-4000-8000-000000000008' );

INSERT INTO "BMC"."ExportFormat" ( "name", "description", "fileExtension", "sequence", "objectGuid" ) VALUES  ( 'STL', 'Stereolithography format for 3D printing', '.stl', 9, 'ef100001-0001-4000-8000-000000000009' );


-- Log of exports produced from a project, tracking what was exported and when.
CREATE TABLE "BMC"."ProjectExport"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"projectId" INT NOT NULL,		-- The project this export was produced from
	"exportFormatId" INT NOT NULL,		-- The format used for the export
	"name" VARCHAR(100) NOT NULL,
	"outputFilePath" VARCHAR(250) NULL,		-- Relative path to the exported file
	"exportedDate" TIMESTAMP NULL,		-- Date/time the export was produced
	"includeInstructions" BOOLEAN NOT NULL DEFAULT false,		-- Whether build instructions were included in the export
	"includePartsList" BOOLEAN NOT NULL DEFAULT false,		-- Whether a bill of materials / parts list was included in the export
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "projectId" FOREIGN KEY ("projectId") REFERENCES "BMC"."Project"("id"),		-- Foreign key to the Project table.
	CONSTRAINT "exportFormatId" FOREIGN KEY ("exportFormatId") REFERENCES "BMC"."ExportFormat"("id"),		-- Foreign key to the ExportFormat table.
	CONSTRAINT "UC_ProjectExport_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the ProjectExport table's tenantGuid and name fields.
);
-- Index on the ProjectExport table's tenantGuid field.
CREATE INDEX "I_ProjectExport_tenantGuid" ON "BMC"."ProjectExport" ("tenantGuid")
;

-- Index on the ProjectExport table's tenantGuid,projectId fields.
CREATE INDEX "I_ProjectExport_tenantGuid_projectId" ON "BMC"."ProjectExport" ("tenantGuid", "projectId")
;

-- Index on the ProjectExport table's tenantGuid,exportFormatId fields.
CREATE INDEX "I_ProjectExport_tenantGuid_exportFormatId" ON "BMC"."ProjectExport" ("tenantGuid", "exportFormatId")
;

-- Index on the ProjectExport table's tenantGuid,name fields.
CREATE INDEX "I_ProjectExport_tenantGuid_name" ON "BMC"."ProjectExport" ("tenantGuid", "name")
;

-- Index on the ProjectExport table's tenantGuid,active fields.
CREATE INDEX "I_ProjectExport_tenantGuid_active" ON "BMC"."ProjectExport" ("tenantGuid", "active")
;

-- Index on the ProjectExport table's tenantGuid,deleted fields.
CREATE INDEX "I_ProjectExport_tenantGuid_deleted" ON "BMC"."ProjectExport" ("tenantGuid", "deleted")
;


-- Public builder profile for community features. One profile per tenant (user). Decoupled from Foundation user/tenant tables to keep BMC concerns independent.
CREATE TABLE "BMC"."UserProfile"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"displayName" VARCHAR(100) NOT NULL,		-- Public display name shown in the community (distinct from auth username)
	"bio" TEXT NULL,		-- Free-form biography / about-me text
	"location" VARCHAR(100) NULL,		-- User's declared location (city, country, or free-form)
	"avatarFileName" VARCHAR(250) NULL,		-- Part of the binary data field setup
	"avatarSize" BIGINT NULL,		-- Part of the binary data field setup
	"avatarData" BYTEA NULL,		-- Part of the binary data field setup
	"avatarMimeType" VARCHAR(100) NULL,		-- Part of the binary data field setup
	"bannerFileName" VARCHAR(250) NULL,		-- Part of the binary data field setup
	"bannerSize" BIGINT NULL,		-- Part of the binary data field setup
	"bannerData" BYTEA NULL,		-- Part of the binary data field setup
	"bannerMimeType" VARCHAR(100) NULL,		-- Part of the binary data field setup
	"websiteUrl" VARCHAR(250) NULL,		-- Optional personal website or portfolio URL
	"isPublic" BOOLEAN NOT NULL DEFAULT true,		-- Whether this profile is visible to unauthenticated visitors
	"memberSinceDate" TIMESTAMP NULL,		-- Date the user first created their profile (for display purposes)
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the UserProfile table's tenantGuid field.
CREATE INDEX "I_UserProfile_tenantGuid" ON "BMC"."UserProfile" ("tenantGuid")
;

-- Index on the UserProfile table's tenantGuid,active fields.
CREATE INDEX "I_UserProfile_tenantGuid_active" ON "BMC"."UserProfile" ("tenantGuid", "active")
;

-- Index on the UserProfile table's tenantGuid,deleted fields.
CREATE INDEX "I_UserProfile_tenantGuid_deleted" ON "BMC"."UserProfile" ("tenantGuid", "deleted")
;


-- The change history for records from the UserProfile table.
CREATE TABLE "BMC"."UserProfileChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userProfileId" INT NOT NULL,		-- Link to the UserProfile table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "userProfileId" FOREIGN KEY ("userProfileId") REFERENCES "BMC"."UserProfile"("id")		-- Foreign key to the UserProfile table.
);
-- Index on the UserProfileChangeHistory table's tenantGuid field.
CREATE INDEX "I_UserProfileChangeHistory_tenantGuid" ON "BMC"."UserProfileChangeHistory" ("tenantGuid")
;

-- Index on the UserProfileChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_UserProfileChangeHistory_tenantGuid_versionNumber" ON "BMC"."UserProfileChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the UserProfileChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_UserProfileChangeHistory_tenantGuid_timeStamp" ON "BMC"."UserProfileChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the UserProfileChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_UserProfileChangeHistory_tenantGuid_userId" ON "BMC"."UserProfileChangeHistory" ("tenantGuid", "userId")
;

-- Index on the UserProfileChangeHistory table's tenantGuid,userProfileId fields.
CREATE INDEX "I_UserProfileChangeHistory_tenantGuid_userProfileId" ON "BMC"."UserProfileChangeHistory" ("tenantGuid", "userProfileId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Lookup table of external link types a user can add to their profile (e.g. BrickLink Store, Flickr, YouTube, Instagram, Rebrickable).
CREATE TABLE "BMC"."UserProfileLinkType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"iconCssClass" VARCHAR(100) NULL,		-- CSS class for the link type icon (e.g. 'fab fa-youtube')
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the UserProfileLinkType table's name field.
CREATE INDEX "I_UserProfileLinkType_name" ON "BMC"."UserProfileLinkType" ("name")
;

-- Index on the UserProfileLinkType table's active field.
CREATE INDEX "I_UserProfileLinkType_active" ON "BMC"."UserProfileLinkType" ("active")
;

-- Index on the UserProfileLinkType table's deleted field.
CREATE INDEX "I_UserProfileLinkType_deleted" ON "BMC"."UserProfileLinkType" ("deleted")
;

INSERT INTO "BMC"."UserProfileLinkType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'BrickLink Store', 'Link to the user''s BrickLink seller store', 'fas fa-store', 1, 'a0100001-0001-4000-8000-000000000001' );

INSERT INTO "BMC"."UserProfileLinkType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'Rebrickable', 'Link to the user''s Rebrickable profile', 'fas fa-cubes', 2, 'a0100001-0001-4000-8000-000000000002' );

INSERT INTO "BMC"."UserProfileLinkType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'Flickr', 'Link to the user''s Flickr photostream', 'fab fa-flickr', 3, 'a0100001-0001-4000-8000-000000000003' );

INSERT INTO "BMC"."UserProfileLinkType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'YouTube', 'Link to the user''s YouTube channel', 'fab fa-youtube', 4, 'a0100001-0001-4000-8000-000000000004' );

INSERT INTO "BMC"."UserProfileLinkType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'Instagram', 'Link to the user''s Instagram profile', 'fab fa-instagram', 5, 'a0100001-0001-4000-8000-000000000005' );

INSERT INTO "BMC"."UserProfileLinkType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'Personal Website', 'Link to the user''s personal website or blog', 'fas fa-globe', 6, 'a0100001-0001-4000-8000-000000000006' );

INSERT INTO "BMC"."UserProfileLinkType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'Eurobricks', 'Link to the user''s Eurobricks forum profile', 'fas fa-comments', 7, 'a0100001-0001-4000-8000-000000000007' );


-- External links displayed on a user's public profile (BrickLink store, Flickr, YouTube, etc.).
CREATE TABLE "BMC"."UserProfileLink"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userProfileId" INT NOT NULL,		-- The profile this link belongs to
	"userProfileLinkTypeId" INT NOT NULL,		-- The type of link (BrickLink, YouTube, etc.)
	"url" VARCHAR(500) NOT NULL,		-- The full URL to the external resource
	"displayLabel" VARCHAR(100) NULL,		-- Optional custom label to display instead of the URL (e.g. 'My BL Store')
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "userProfileId" FOREIGN KEY ("userProfileId") REFERENCES "BMC"."UserProfile"("id"),		-- Foreign key to the UserProfile table.
	CONSTRAINT "userProfileLinkTypeId" FOREIGN KEY ("userProfileLinkTypeId") REFERENCES "BMC"."UserProfileLinkType"("id")		-- Foreign key to the UserProfileLinkType table.
);
-- Index on the UserProfileLink table's tenantGuid field.
CREATE INDEX "I_UserProfileLink_tenantGuid" ON "BMC"."UserProfileLink" ("tenantGuid")
;

-- Index on the UserProfileLink table's tenantGuid,userProfileId fields.
CREATE INDEX "I_UserProfileLink_tenantGuid_userProfileId" ON "BMC"."UserProfileLink" ("tenantGuid", "userProfileId")
;

-- Index on the UserProfileLink table's tenantGuid,userProfileLinkTypeId fields.
CREATE INDEX "I_UserProfileLink_tenantGuid_userProfileLinkTypeId" ON "BMC"."UserProfileLink" ("tenantGuid", "userProfileLinkTypeId")
;

-- Index on the UserProfileLink table's tenantGuid,active fields.
CREATE INDEX "I_UserProfileLink_tenantGuid_active" ON "BMC"."UserProfileLink" ("tenantGuid", "active")
;

-- Index on the UserProfileLink table's tenantGuid,deleted fields.
CREATE INDEX "I_UserProfileLink_tenantGuid_deleted" ON "BMC"."UserProfileLink" ("tenantGuid", "deleted")
;


-- Junction table linking a user profile to their preferred LEGO themes (e.g. Star Wars, Technic, City). Used to personalise the experience and display theme interests on the public profile.
CREATE TABLE "BMC"."UserProfilePreferredTheme"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userProfileId" INT NOT NULL,		-- The profile this preference belongs to
	"legoThemeId" INT NOT NULL,		-- The LEGO theme the user prefers
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "userProfileId" FOREIGN KEY ("userProfileId") REFERENCES "BMC"."UserProfile"("id"),		-- Foreign key to the UserProfile table.
	CONSTRAINT "legoThemeId" FOREIGN KEY ("legoThemeId") REFERENCES "BMC"."LegoTheme"("id"),		-- Foreign key to the LegoTheme table.
	CONSTRAINT "UC_UserProfilePreferredTheme_tenantGuid_userProfileId_legoThemeId" UNIQUE ( "tenantGuid", "userProfileId", "legoThemeId") 		-- Uniqueness enforced on the UserProfilePreferredTheme table's tenantGuid and userProfileId and legoThemeId fields.
);
-- Index on the UserProfilePreferredTheme table's tenantGuid field.
CREATE INDEX "I_UserProfilePreferredTheme_tenantGuid" ON "BMC"."UserProfilePreferredTheme" ("tenantGuid")
;

-- Index on the UserProfilePreferredTheme table's tenantGuid,userProfileId fields.
CREATE INDEX "I_UserProfilePreferredTheme_tenantGuid_userProfileId" ON "BMC"."UserProfilePreferredTheme" ("tenantGuid", "userProfileId")
;

-- Index on the UserProfilePreferredTheme table's tenantGuid,legoThemeId fields.
CREATE INDEX "I_UserProfilePreferredTheme_tenantGuid_legoThemeId" ON "BMC"."UserProfilePreferredTheme" ("tenantGuid", "legoThemeId")
;

-- Index on the UserProfilePreferredTheme table's tenantGuid,active fields.
CREATE INDEX "I_UserProfilePreferredTheme_tenantGuid_active" ON "BMC"."UserProfilePreferredTheme" ("tenantGuid", "active")
;

-- Index on the UserProfilePreferredTheme table's tenantGuid,deleted fields.
CREATE INDEX "I_UserProfilePreferredTheme_tenantGuid_deleted" ON "BMC"."UserProfilePreferredTheme" ("tenantGuid", "deleted")
;


-- Tracks a user's relationship with official LEGO sets for their collector showcase. Distinct from UserCollectionSetImport which tracks parts inventory.
CREATE TABLE "BMC"."UserSetOwnership"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"legoSetId" INT NOT NULL,		-- The official LEGO set
	"status" VARCHAR(50) NOT NULL,		-- Ownership status: Owned, Built, Wanted, WishList, ForDisplay, ForSale
	"acquiredDate" TIMESTAMP NULL,		-- Date the user acquired this set (null if unknown or wanted)
	"personalRating" INT NULL,		-- User's personal rating of the set (1-5 stars, null if not rated)
	"notes" TEXT NULL,		-- Free-form notes about this set (e.g. condition, where purchased, modifications)
	"quantity" INT NOT NULL DEFAULT 1,		-- Number of copies owned
	"isPublic" BOOLEAN NOT NULL DEFAULT true,		-- Whether this ownership record is visible on the user's public profile
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "legoSetId" FOREIGN KEY ("legoSetId") REFERENCES "BMC"."LegoSet"("id"),		-- Foreign key to the LegoSet table.
	CONSTRAINT "UC_UserSetOwnership_tenantGuid_legoSetId" UNIQUE ( "tenantGuid", "legoSetId") 		-- Uniqueness enforced on the UserSetOwnership table's tenantGuid and legoSetId fields.
);
-- Index on the UserSetOwnership table's tenantGuid field.
CREATE INDEX "I_UserSetOwnership_tenantGuid" ON "BMC"."UserSetOwnership" ("tenantGuid")
;

-- Index on the UserSetOwnership table's tenantGuid,legoSetId fields.
CREATE INDEX "I_UserSetOwnership_tenantGuid_legoSetId" ON "BMC"."UserSetOwnership" ("tenantGuid", "legoSetId")
;

-- Index on the UserSetOwnership table's tenantGuid,active fields.
CREATE INDEX "I_UserSetOwnership_tenantGuid_active" ON "BMC"."UserSetOwnership" ("tenantGuid", "active")
;

-- Index on the UserSetOwnership table's tenantGuid,deleted fields.
CREATE INDEX "I_UserSetOwnership_tenantGuid_deleted" ON "BMC"."UserSetOwnership" ("tenantGuid", "deleted")
;


-- Cached aggregate statistics for a user's profile. Periodically recalculated by background worker to avoid expensive real-time queries.
CREATE TABLE "BMC"."UserProfileStat"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userProfileId" INT NOT NULL,		-- The profile these stats belong to
	"totalPartsOwned" INT NOT NULL DEFAULT 0,		-- Total number of individual parts across all collections
	"totalUniquePartsOwned" INT NOT NULL DEFAULT 0,		-- Total number of unique part+colour combinations owned
	"totalSetsOwned" INT NOT NULL DEFAULT 0,		-- Total number of sets with Owned or Built status
	"totalMocsPublished" INT NOT NULL DEFAULT 0,		-- Total number of MOCs published to the gallery
	"totalFollowers" INT NOT NULL DEFAULT 0,		-- Number of users following this profile
	"totalFollowing" INT NOT NULL DEFAULT 0,		-- Number of users this profile is following
	"totalLikesReceived" INT NOT NULL DEFAULT 0,		-- Total likes received across all published MOCs
	"totalAchievementPoints" INT NOT NULL DEFAULT 0,		-- Sum of achievement point values earned
	"lastCalculatedDate" TIMESTAMP NULL,		-- When these stats were last recalculated by the background worker
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "userProfileId" FOREIGN KEY ("userProfileId") REFERENCES "BMC"."UserProfile"("id")		-- Foreign key to the UserProfile table.
);
-- Index on the UserProfileStat table's tenantGuid field.
CREATE INDEX "I_UserProfileStat_tenantGuid" ON "BMC"."UserProfileStat" ("tenantGuid")
;

-- Index on the UserProfileStat table's tenantGuid,userProfileId fields.
CREATE INDEX "I_UserProfileStat_tenantGuid_userProfileId" ON "BMC"."UserProfileStat" ("tenantGuid", "userProfileId")
;

-- Index on the UserProfileStat table's tenantGuid,active fields.
CREATE INDEX "I_UserProfileStat_tenantGuid_active" ON "BMC"."UserProfileStat" ("tenantGuid", "active")
;

-- Index on the UserProfileStat table's tenantGuid,deleted fields.
CREATE INDEX "I_UserProfileStat_tenantGuid_deleted" ON "BMC"."UserProfileStat" ("tenantGuid", "deleted")
;


-- Follow relationships between users. A follower subscribes to activity updates from the followed user.
CREATE TABLE "BMC"."UserFollow"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"followerTenantGuid" VARCHAR(50) NOT NULL,		-- Tenant GUID of the user who is following
	"followedTenantGuid" VARCHAR(50) NOT NULL,		-- Tenant GUID of the user being followed
	"followedDate" TIMESTAMP NOT NULL,		-- Date/time the follow relationship was created
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_UserFollow_followerTenantGuid_followedTenantGuid" UNIQUE ( "followerTenantGuid", "followedTenantGuid") 		-- Uniqueness enforced on the UserFollow table's followerTenantGuid and followedTenantGuid fields.
);
-- Index on the UserFollow table's active field.
CREATE INDEX "I_UserFollow_active" ON "BMC"."UserFollow" ("active")
;

-- Index on the UserFollow table's deleted field.
CREATE INDEX "I_UserFollow_deleted" ON "BMC"."UserFollow" ("deleted")
;


-- Lookup table of activity event types that appear in users' activity feeds.
CREATE TABLE "BMC"."ActivityEventType"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"iconCssClass" VARCHAR(100) NULL,		-- CSS class for the event type icon in the activity feed
	"accentColor" VARCHAR(10) NULL,		-- Optional accent colour for this event type in the feed
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the ActivityEventType table's name field.
CREATE INDEX "I_ActivityEventType_name" ON "BMC"."ActivityEventType" ("name")
;

-- Index on the ActivityEventType table's active field.
CREATE INDEX "I_ActivityEventType_active" ON "BMC"."ActivityEventType" ("active")
;

-- Index on the ActivityEventType table's deleted field.
CREATE INDEX "I_ActivityEventType_deleted" ON "BMC"."ActivityEventType" ("deleted")
;

INSERT INTO "BMC"."ActivityEventType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'PublishedMoc', 'User published a MOC to the gallery', 'fas fa-rocket', 1, 'ae100001-0001-4000-8000-000000000001' );

INSERT INTO "BMC"."ActivityEventType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'AddedSet', 'User added a set to their collection', 'fas fa-box-open', 2, 'ae100001-0001-4000-8000-000000000002' );

INSERT INTO "BMC"."ActivityEventType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'EarnedAchievement', 'User earned an achievement', 'fas fa-trophy', 3, 'ae100001-0001-4000-8000-000000000003' );

INSERT INTO "BMC"."ActivityEventType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'JoinedChallenge', 'User submitted an entry to a build challenge', 'fas fa-flag-checkered', 4, 'ae100001-0001-4000-8000-000000000004' );

INSERT INTO "BMC"."ActivityEventType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'SharedInstruction', 'User published build instructions', 'fas fa-book', 5, 'ae100001-0001-4000-8000-000000000005' );

INSERT INTO "BMC"."ActivityEventType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'CollectionMilestone', 'User reached a collection milestone', 'fas fa-gem', 6, 'ae100001-0001-4000-8000-000000000006' );

INSERT INTO "BMC"."ActivityEventType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'FollowedUser', 'User followed another builder', 'fas fa-user-plus', 7, 'ae100001-0001-4000-8000-000000000007' );

INSERT INTO "BMC"."ActivityEventType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'CommittedVersion', 'User committed a new version of a MOC', 'fas fa-code-commit', 8, 'ae100001-0001-4000-8000-000000000008' );

INSERT INTO "BMC"."ActivityEventType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'ForkedMoc', 'User forked another builder''s MOC', 'fas fa-code-branch', 9, 'ae100001-0001-4000-8000-000000000009' );

INSERT INTO "BMC"."ActivityEventType" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'AddedCollaborator', 'User invited a collaborator to a MOC', 'fas fa-user-friends', 10, 'ae100001-0001-4000-8000-000000000010' );


-- Individual activity feed events generated by user actions. Used to build the community activity feed and individual user activity histories.
CREATE TABLE "BMC"."ActivityEvent"
(
	"id" BIGSERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"activityEventTypeId" INT NOT NULL,		-- The type of activity event
	"title" VARCHAR(250) NOT NULL,		-- Short display title for the event (e.g. 'Published Technic Crane MOC')
	"description" TEXT NULL,		-- Optional longer description or context for the event
	"relatedEntityType" VARCHAR(100) NULL,		-- Type name of the related entity (e.g. 'PublishedMoc', 'LegoSet', 'Achievement')
	"relatedEntityId" BIGINT NULL,		-- ID of the related entity for deep linking (null if not applicable)
	"eventDate" TIMESTAMP NOT NULL,		-- Date/time the activity occurred
	"isPublic" BOOLEAN NOT NULL DEFAULT true,		-- Whether this event is visible on the public activity feed
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "activityEventTypeId" FOREIGN KEY ("activityEventTypeId") REFERENCES "BMC"."ActivityEventType"("id")		-- Foreign key to the ActivityEventType table.
);
-- Index on the ActivityEvent table's tenantGuid field.
CREATE INDEX "I_ActivityEvent_tenantGuid" ON "BMC"."ActivityEvent" ("tenantGuid")
;

-- Index on the ActivityEvent table's tenantGuid,activityEventTypeId fields.
CREATE INDEX "I_ActivityEvent_tenantGuid_activityEventTypeId" ON "BMC"."ActivityEvent" ("tenantGuid", "activityEventTypeId")
;

-- Index on the ActivityEvent table's tenantGuid,active fields.
CREATE INDEX "I_ActivityEvent_tenantGuid_active" ON "BMC"."ActivityEvent" ("tenantGuid", "active")
;

-- Index on the ActivityEvent table's tenantGuid,deleted fields.
CREATE INDEX "I_ActivityEvent_tenantGuid_deleted" ON "BMC"."ActivityEvent" ("tenantGuid", "deleted")
;


-- A MOC (My Own Creation) published to the community gallery / MOCHub. Links to the underlying project for parts list and 3D model data. Supports GitHub-style version control, forking, and collaboration.
CREATE TABLE "BMC"."PublishedMoc"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"projectId" INT NOT NULL,		-- The underlying project containing the model data
	"name" VARCHAR(100) NOT NULL,		-- Public-facing title of the MOC
	"description" TEXT NULL,		-- Rich description of the MOC, build story, or design notes
	"thumbnailImagePath" VARCHAR(250) NULL,		-- Relative path to the primary thumbnail image
	"tags" TEXT NULL,		-- Comma-separated tags for search and categorization (e.g. 'technic, crane, vehicle')
	"isPublished" BOOLEAN NOT NULL DEFAULT false,		-- Whether this MOC is visible in the public gallery (draft vs published)
	"isFeatured" BOOLEAN NOT NULL DEFAULT false,		-- Whether this MOC is featured / editor's pick (set by moderators)
	"publishedDate" TIMESTAMP NULL,		-- Date/time the MOC was first published
	"viewCount" INT NOT NULL DEFAULT 0,		-- Number of times this MOC has been viewed
	"likeCount" INT NOT NULL DEFAULT 0,		-- Cached like count for fast sorting and display
	"commentCount" INT NOT NULL DEFAULT 0,		-- Cached comment count for fast display
	"favouriteCount" INT NOT NULL DEFAULT 0,		-- Cached favourite/bookmark count for fast display
	"partCount" INT NULL,		-- Cached total part count from the underlying project
	"allowForking" BOOLEAN NOT NULL DEFAULT true,		-- Whether other users can fork (copy) this MOC as a starting point
	"visibility" VARCHAR(50) NOT NULL DEFAULT 'Public',		-- MOCHub visibility level: Public, Private, Unlisted. Public = visible to everyone; Private = only owner and collaborators; Unlisted = accessible via direct link only
	"forkCount" INT NOT NULL DEFAULT 0,		-- Cached count of forks created from this MOC
	"forkedFromMocId" INT NULL,		-- If this MOC is a fork, the source MOC it was forked from (null = original work)
	"licenseName" VARCHAR(100) NULL,		-- Licence type for the MOC: CC-BY, CC-BY-SA, CC-BY-NC, CC-BY-NC-SA, CC0, AllRightsReserved (null = All Rights Reserved)
	"readmeMarkdown" TEXT NULL,		-- GitHub-style README displayed on the MOC repository page (supports Markdown formatting)
	"slug" VARCHAR(100) NULL,		-- URL-friendly slug for the MOC (e.g. 'technic-mobile-crane'). Unique per tenant.
	"defaultBranchName" VARCHAR(100) NULL,		-- Reserved for future branching support (default: 'main')
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "projectId" FOREIGN KEY ("projectId") REFERENCES "BMC"."Project"("id"),		-- Foreign key to the Project table.
	CONSTRAINT "forkedFromMocId" FOREIGN KEY ("forkedFromMocId") REFERENCES "BMC"."PublishedMoc"("id"),		-- Foreign key to the PublishedMoc table.
	CONSTRAINT "UC_PublishedMoc_tenantGuid_name" UNIQUE ( "tenantGuid", "name") ,		-- Uniqueness enforced on the PublishedMoc table's tenantGuid and name fields.
	CONSTRAINT "UC_PublishedMoc_tenantGuid_slug" UNIQUE ( "tenantGuid", "slug") 		-- Uniqueness enforced on the PublishedMoc table's tenantGuid and slug fields.
);
-- Index on the PublishedMoc table's tenantGuid field.
CREATE INDEX "I_PublishedMoc_tenantGuid" ON "BMC"."PublishedMoc" ("tenantGuid")
;

-- Index on the PublishedMoc table's tenantGuid,projectId fields.
CREATE INDEX "I_PublishedMoc_tenantGuid_projectId" ON "BMC"."PublishedMoc" ("tenantGuid", "projectId")
;

-- Index on the PublishedMoc table's tenantGuid,name fields.
CREATE INDEX "I_PublishedMoc_tenantGuid_name" ON "BMC"."PublishedMoc" ("tenantGuid", "name")
;

-- Index on the PublishedMoc table's tenantGuid,forkedFromMocId fields.
CREATE INDEX "I_PublishedMoc_tenantGuid_forkedFromMocId" ON "BMC"."PublishedMoc" ("tenantGuid", "forkedFromMocId")
;

-- Index on the PublishedMoc table's tenantGuid,active fields.
CREATE INDEX "I_PublishedMoc_tenantGuid_active" ON "BMC"."PublishedMoc" ("tenantGuid", "active")
;

-- Index on the PublishedMoc table's tenantGuid,deleted fields.
CREATE INDEX "I_PublishedMoc_tenantGuid_deleted" ON "BMC"."PublishedMoc" ("tenantGuid", "deleted")
;


-- The change history for records from the PublishedMoc table.
CREATE TABLE "BMC"."PublishedMocChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"publishedMocId" INT NOT NULL,		-- Link to the PublishedMoc table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "publishedMocId" FOREIGN KEY ("publishedMocId") REFERENCES "BMC"."PublishedMoc"("id")		-- Foreign key to the PublishedMoc table.
);
-- Index on the PublishedMocChangeHistory table's tenantGuid field.
CREATE INDEX "I_PublishedMocChangeHistory_tenantGuid" ON "BMC"."PublishedMocChangeHistory" ("tenantGuid")
;

-- Index on the PublishedMocChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_PublishedMocChangeHistory_tenantGuid_versionNumber" ON "BMC"."PublishedMocChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the PublishedMocChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_PublishedMocChangeHistory_tenantGuid_timeStamp" ON "BMC"."PublishedMocChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the PublishedMocChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_PublishedMocChangeHistory_tenantGuid_userId" ON "BMC"."PublishedMocChangeHistory" ("tenantGuid", "userId")
;

-- Index on the PublishedMocChangeHistory table's tenantGuid,publishedMocId fields.
CREATE INDEX "I_PublishedMocChangeHistory_tenantGuid_publishedMocId" ON "BMC"."PublishedMocChangeHistory" ("tenantGuid", "publishedMocId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Version snapshots (commits) for a published MOC. Each row stores a complete MPD text snapshot of the model at a point in time, enabling GitHub-style version history and diffing. MPD files are typically 50-200KB, so full snapshots are practical and simple.
CREATE TABLE "BMC"."MocVersion"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"publishedMocId" INT NOT NULL,		-- The MOC this version belongs to
	"versionNumber" INT NOT NULL,		-- Sequential version number within the MOC (1, 2, 3...)
	"commitMessage" VARCHAR(500) NOT NULL,		-- User-provided description of what changed in this version
	"mpdSnapshot" TEXT NOT NULL,		-- Full MPD (Multi-Part Document) text content at this version — the complete model definition
	"partCount" INT NULL,		-- Total part count at this version
	"addedPartCount" INT NULL,		-- Number of parts added since the previous version (null for first version)
	"removedPartCount" INT NULL,		-- Number of parts removed since the previous version (null for first version)
	"modifiedPartCount" INT NULL,		-- Number of parts moved or recoloured since the previous version (null for first version)
	"snapshotDate" TIMESTAMP NOT NULL,		-- Date/time this version was committed
	"authorTenantGuid" VARCHAR(50) NOT NULL,		-- Tenant GUID of the user who committed this version (may differ from MOC owner for collaborators)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "publishedMocId" FOREIGN KEY ("publishedMocId") REFERENCES "BMC"."PublishedMoc"("id"),		-- Foreign key to the PublishedMoc table.
	CONSTRAINT "UC_MocVersion_tenantGuid_publishedMocId_versionNumber" UNIQUE ( "tenantGuid", "publishedMocId", "versionNumber") 		-- Uniqueness enforced on the MocVersion table's tenantGuid and publishedMocId and versionNumber fields.
);
-- Index on the MocVersion table's tenantGuid field.
CREATE INDEX "I_MocVersion_tenantGuid" ON "BMC"."MocVersion" ("tenantGuid")
;

-- Index on the MocVersion table's tenantGuid,publishedMocId fields.
CREATE INDEX "I_MocVersion_tenantGuid_publishedMocId" ON "BMC"."MocVersion" ("tenantGuid", "publishedMocId")
;

-- Index on the MocVersion table's tenantGuid,active fields.
CREATE INDEX "I_MocVersion_tenantGuid_active" ON "BMC"."MocVersion" ("tenantGuid", "active")
;

-- Index on the MocVersion table's tenantGuid,deleted fields.
CREATE INDEX "I_MocVersion_tenantGuid_deleted" ON "BMC"."MocVersion" ("tenantGuid", "deleted")
;


-- Additional gallery images for a published MOC. The thumbnail is on the PublishedMoc itself; these are supplementary views and renders.
CREATE TABLE "BMC"."PublishedMocImage"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"publishedMocId" INT NOT NULL,		-- The published MOC this image belongs to
	"imagePath" VARCHAR(250) NOT NULL,		-- Relative path to the image file
	"caption" VARCHAR(250) NULL,		-- Optional caption describing the image or the angle shown
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "publishedMocId" FOREIGN KEY ("publishedMocId") REFERENCES "BMC"."PublishedMoc"("id")		-- Foreign key to the PublishedMoc table.
);
-- Index on the PublishedMocImage table's tenantGuid field.
CREATE INDEX "I_PublishedMocImage_tenantGuid" ON "BMC"."PublishedMocImage" ("tenantGuid")
;

-- Index on the PublishedMocImage table's tenantGuid,publishedMocId fields.
CREATE INDEX "I_PublishedMocImage_tenantGuid_publishedMocId" ON "BMC"."PublishedMocImage" ("tenantGuid", "publishedMocId")
;

-- Index on the PublishedMocImage table's tenantGuid,active fields.
CREATE INDEX "I_PublishedMocImage_tenantGuid_active" ON "BMC"."PublishedMocImage" ("tenantGuid", "active")
;

-- Index on the PublishedMocImage table's tenantGuid,deleted fields.
CREATE INDEX "I_PublishedMocImage_tenantGuid_deleted" ON "BMC"."PublishedMocImage" ("tenantGuid", "deleted")
;


-- Tracks the fork graph: which MOC was forked from which, and at what version. Enables GitHub-style 'forked from user/moc' headers and network graph visualisation.
CREATE TABLE "BMC"."MocFork"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"forkedMocId" INT NOT NULL,		-- The fork (child MOC) that was created
	"sourceMocId" INT NOT NULL,		-- The original (parent MOC) that was forked from
	"mocVersionId" INT NULL,		-- The specific version the fork was created from (null if unknown or forked from latest)
	"forkerTenantGuid" VARCHAR(50) NOT NULL,		-- Tenant GUID of the user who created the fork
	"forkedDate" TIMESTAMP NOT NULL,		-- Date/time the fork was created
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "forkedMocId" FOREIGN KEY ("forkedMocId") REFERENCES "BMC"."PublishedMoc"("id"),		-- Foreign key to the PublishedMoc table.
	CONSTRAINT "sourceMocId" FOREIGN KEY ("sourceMocId") REFERENCES "BMC"."PublishedMoc"("id"),		-- Foreign key to the PublishedMoc table.
	CONSTRAINT "mocVersionId" FOREIGN KEY ("mocVersionId") REFERENCES "BMC"."MocVersion"("id"),		-- Foreign key to the MocVersion table.
	CONSTRAINT "UC_MocFork_forkedMocId" UNIQUE ( "forkedMocId") 		-- Uniqueness enforced on the MocFork table's forkedMocId field.
);
-- Index on the MocFork table's forkedMocId field.
CREATE INDEX "I_MocFork_forkedMocId" ON "BMC"."MocFork" ("forkedMocId")
;

-- Index on the MocFork table's sourceMocId field.
CREATE INDEX "I_MocFork_sourceMocId" ON "BMC"."MocFork" ("sourceMocId")
;

-- Index on the MocFork table's mocVersionId field.
CREATE INDEX "I_MocFork_mocVersionId" ON "BMC"."MocFork" ("mocVersionId")
;

-- Index on the MocFork table's active field.
CREATE INDEX "I_MocFork_active" ON "BMC"."MocFork" ("active")
;

-- Index on the MocFork table's deleted field.
CREATE INDEX "I_MocFork_deleted" ON "BMC"."MocFork" ("deleted")
;


-- Allows MOC owners to grant other users read/write access to their MOCs, enabling collaborative building. Similar to GitHub repository collaborators.
CREATE TABLE "BMC"."MocCollaborator"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"publishedMocId" INT NOT NULL,		-- The MOC being shared
	"collaboratorTenantGuid" VARCHAR(50) NOT NULL,		-- Tenant GUID of the collaborator being granted access
	"accessLevel" VARCHAR(50) NOT NULL,		-- Access level granted: Read, Write, Admin
	"invitedDate" TIMESTAMP NOT NULL,		-- Date/time the collaborator was invited
	"acceptedDate" TIMESTAMP NULL,		-- Date/time the invitation was accepted (null = pending)
	"isAccepted" BOOLEAN NOT NULL DEFAULT false,		-- Whether the collaborator has accepted the invitation
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "publishedMocId" FOREIGN KEY ("publishedMocId") REFERENCES "BMC"."PublishedMoc"("id"),		-- Foreign key to the PublishedMoc table.
	CONSTRAINT "UC_MocCollaborator_tenantGuid_publishedMocId_collaboratorTenantGuid" UNIQUE ( "tenantGuid", "publishedMocId", "collaboratorTenantGuid") 		-- Uniqueness enforced on the MocCollaborator table's tenantGuid and publishedMocId and collaboratorTenantGuid fields.
);
-- Index on the MocCollaborator table's tenantGuid field.
CREATE INDEX "I_MocCollaborator_tenantGuid" ON "BMC"."MocCollaborator" ("tenantGuid")
;

-- Index on the MocCollaborator table's tenantGuid,publishedMocId fields.
CREATE INDEX "I_MocCollaborator_tenantGuid_publishedMocId" ON "BMC"."MocCollaborator" ("tenantGuid", "publishedMocId")
;

-- Index on the MocCollaborator table's tenantGuid,active fields.
CREATE INDEX "I_MocCollaborator_tenantGuid_active" ON "BMC"."MocCollaborator" ("tenantGuid", "active")
;

-- Index on the MocCollaborator table's tenantGuid,deleted fields.
CREATE INDEX "I_MocCollaborator_tenantGuid_deleted" ON "BMC"."MocCollaborator" ("tenantGuid", "deleted")
;


-- User likes on published MOCs. One like per user per MOC.
CREATE TABLE "BMC"."MocLike"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"publishedMocId" INT NOT NULL,		-- The MOC being liked
	"likerTenantGuid" VARCHAR(50) NOT NULL,		-- Tenant GUID of the user who liked
	"likedDate" TIMESTAMP NOT NULL,		-- Date/time the like was registered
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "publishedMocId" FOREIGN KEY ("publishedMocId") REFERENCES "BMC"."PublishedMoc"("id"),		-- Foreign key to the PublishedMoc table.
	CONSTRAINT "UC_MocLike_publishedMocId_likerTenantGuid" UNIQUE ( "publishedMocId", "likerTenantGuid") 		-- Uniqueness enforced on the MocLike table's publishedMocId and likerTenantGuid fields.
);
-- Index on the MocLike table's publishedMocId field.
CREATE INDEX "I_MocLike_publishedMocId" ON "BMC"."MocLike" ("publishedMocId")
;

-- Index on the MocLike table's active field.
CREATE INDEX "I_MocLike_active" ON "BMC"."MocLike" ("active")
;

-- Index on the MocLike table's deleted field.
CREATE INDEX "I_MocLike_deleted" ON "BMC"."MocLike" ("deleted")
;


-- User comments on published MOCs. Supports threaded replies via self-referencing parent FK.
CREATE TABLE "BMC"."MocComment"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"publishedMocId" INT NOT NULL,		-- The MOC being commented on
	"commenterTenantGuid" VARCHAR(50) NOT NULL,		-- Tenant GUID of the user who posted the comment
	"commentText" TEXT NOT NULL,		-- The comment content
	"postedDate" TIMESTAMP NOT NULL,		-- Date/time the comment was posted
	"mocCommentId" INT NULL,		-- Optional parent comment for threaded replies (null = top-level comment)
	"isEdited" BOOLEAN NOT NULL DEFAULT false,		-- Whether this comment has been edited after posting
	"isHidden" BOOLEAN NOT NULL DEFAULT false,		-- Whether this comment has been hidden by a moderator
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "publishedMocId" FOREIGN KEY ("publishedMocId") REFERENCES "BMC"."PublishedMoc"("id"),		-- Foreign key to the PublishedMoc table.
	CONSTRAINT "mocCommentId" FOREIGN KEY ("mocCommentId") REFERENCES "BMC"."MocComment"("id")		-- Foreign key to the MocComment table.
);
-- Index on the MocComment table's publishedMocId field.
CREATE INDEX "I_MocComment_publishedMocId" ON "BMC"."MocComment" ("publishedMocId")
;

-- Index on the MocComment table's mocCommentId field.
CREATE INDEX "I_MocComment_mocCommentId" ON "BMC"."MocComment" ("mocCommentId")
;

-- Index on the MocComment table's active field.
CREATE INDEX "I_MocComment_active" ON "BMC"."MocComment" ("active")
;

-- Index on the MocComment table's deleted field.
CREATE INDEX "I_MocComment_deleted" ON "BMC"."MocComment" ("deleted")
;


-- User's favourited (bookmarked) MOCs for quick access. Separate from likes — favourites are private bookmarks, likes are public endorsements.
CREATE TABLE "BMC"."MocFavourite"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"publishedMocId" INT NOT NULL,		-- The MOC being favourited
	"userTenantGuid" VARCHAR(50) NOT NULL,		-- Tenant GUID of the user who favourited
	"favouritedDate" TIMESTAMP NOT NULL,		-- Date/time the favourite was added
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "publishedMocId" FOREIGN KEY ("publishedMocId") REFERENCES "BMC"."PublishedMoc"("id"),		-- Foreign key to the PublishedMoc table.
	CONSTRAINT "UC_MocFavourite_publishedMocId_userTenantGuid" UNIQUE ( "publishedMocId", "userTenantGuid") 		-- Uniqueness enforced on the MocFavourite table's publishedMocId and userTenantGuid fields.
);
-- Index on the MocFavourite table's publishedMocId field.
CREATE INDEX "I_MocFavourite_publishedMocId" ON "BMC"."MocFavourite" ("publishedMocId")
;

-- Index on the MocFavourite table's active field.
CREATE INDEX "I_MocFavourite_active" ON "BMC"."MocFavourite" ("active")
;

-- Index on the MocFavourite table's deleted field.
CREATE INDEX "I_MocFavourite_deleted" ON "BMC"."MocFavourite" ("deleted")
;


-- Published instruction manuals shared with the community. Can be BMC-native format (linked to BuildManual), uploaded PDF, or image-based.
CREATE TABLE "BMC"."SharedInstruction"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"buildManualId" INT NULL,		-- Optional link to a BMC-native BuildManual (null for uploaded PDF/image instructions)
	"publishedMocId" INT NULL,		-- Optional link to the published MOC these instructions are for
	"name" VARCHAR(100) NOT NULL,		-- Public-facing title of the instruction document
	"description" TEXT NULL,		-- Description of what these instructions cover
	"formatType" VARCHAR(50) NOT NULL,		-- Format of the instruction: BMCNative, PDF, ImageSet
	"filePath" VARCHAR(250) NULL,		-- Relative path to the instruction file (PDF) or folder (image set). Null for BMC-native.
	"isPublished" BOOLEAN NOT NULL DEFAULT false,		-- Whether these instructions are visible in the community
	"publishedDate" TIMESTAMP NULL,		-- Date/time the instructions were first published
	"downloadCount" INT NOT NULL DEFAULT 0,		-- Number of times these instructions have been downloaded
	"pageCount" INT NULL,		-- Total number of pages (for display purposes)
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "buildManualId" FOREIGN KEY ("buildManualId") REFERENCES "BMC"."BuildManual"("id"),		-- Foreign key to the BuildManual table.
	CONSTRAINT "publishedMocId" FOREIGN KEY ("publishedMocId") REFERENCES "BMC"."PublishedMoc"("id"),		-- Foreign key to the PublishedMoc table.
	CONSTRAINT "UC_SharedInstruction_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the SharedInstruction table's tenantGuid and name fields.
);
-- Index on the SharedInstruction table's tenantGuid field.
CREATE INDEX "I_SharedInstruction_tenantGuid" ON "BMC"."SharedInstruction" ("tenantGuid")
;

-- Index on the SharedInstruction table's tenantGuid,buildManualId fields.
CREATE INDEX "I_SharedInstruction_tenantGuid_buildManualId" ON "BMC"."SharedInstruction" ("tenantGuid", "buildManualId")
;

-- Index on the SharedInstruction table's tenantGuid,publishedMocId fields.
CREATE INDEX "I_SharedInstruction_tenantGuid_publishedMocId" ON "BMC"."SharedInstruction" ("tenantGuid", "publishedMocId")
;

-- Index on the SharedInstruction table's tenantGuid,name fields.
CREATE INDEX "I_SharedInstruction_tenantGuid_name" ON "BMC"."SharedInstruction" ("tenantGuid", "name")
;

-- Index on the SharedInstruction table's tenantGuid,active fields.
CREATE INDEX "I_SharedInstruction_tenantGuid_active" ON "BMC"."SharedInstruction" ("tenantGuid", "active")
;

-- Index on the SharedInstruction table's tenantGuid,deleted fields.
CREATE INDEX "I_SharedInstruction_tenantGuid_deleted" ON "BMC"."SharedInstruction" ("tenantGuid", "deleted")
;


-- The change history for records from the SharedInstruction table.
CREATE TABLE "BMC"."SharedInstructionChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"sharedInstructionId" INT NOT NULL,		-- Link to the SharedInstruction table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "sharedInstructionId" FOREIGN KEY ("sharedInstructionId") REFERENCES "BMC"."SharedInstruction"("id")		-- Foreign key to the SharedInstruction table.
);
-- Index on the SharedInstructionChangeHistory table's tenantGuid field.
CREATE INDEX "I_SharedInstructionChangeHistory_tenantGuid" ON "BMC"."SharedInstructionChangeHistory" ("tenantGuid")
;

-- Index on the SharedInstructionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_SharedInstructionChangeHistory_tenantGuid_versionNumber" ON "BMC"."SharedInstructionChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the SharedInstructionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_SharedInstructionChangeHistory_tenantGuid_timeStamp" ON "BMC"."SharedInstructionChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the SharedInstructionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_SharedInstructionChangeHistory_tenantGuid_userId" ON "BMC"."SharedInstructionChangeHistory" ("tenantGuid", "userId")
;

-- Index on the SharedInstructionChangeHistory table's tenantGuid,sharedInstructionId fields.
CREATE INDEX "I_SharedInstructionChangeHistory_tenantGuid_sharedInstructionId" ON "BMC"."SharedInstructionChangeHistory" ("tenantGuid", "sharedInstructionId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- Groups of achievements for organization and display (e.g. Collection, Building, Social, Exploration).
CREATE TABLE "BMC"."AchievementCategory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"iconCssClass" VARCHAR(100) NULL,		-- CSS class for the category icon
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the AchievementCategory table's name field.
CREATE INDEX "I_AchievementCategory_name" ON "BMC"."AchievementCategory" ("name")
;

-- Index on the AchievementCategory table's active field.
CREATE INDEX "I_AchievementCategory_active" ON "BMC"."AchievementCategory" ("active")
;

-- Index on the AchievementCategory table's deleted field.
CREATE INDEX "I_AchievementCategory_deleted" ON "BMC"."AchievementCategory" ("deleted")
;

INSERT INTO "BMC"."AchievementCategory" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'Collection', 'Achievements related to building and managing your parts collection', 'fas fa-cubes', 1, 'ac100001-0001-4000-8000-000000000001' );

INSERT INTO "BMC"."AchievementCategory" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'Building', 'Achievements related to creating and publishing MOCs', 'fas fa-hammer', 2, 'ac100001-0001-4000-8000-000000000002' );

INSERT INTO "BMC"."AchievementCategory" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'Social', 'Achievements related to community engagement and social interactions', 'fas fa-users', 3, 'ac100001-0001-4000-8000-000000000003' );

INSERT INTO "BMC"."AchievementCategory" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'Exploration', 'Achievements related to exploring the parts catalog and set database', 'fas fa-compass', 4, 'ac100001-0001-4000-8000-000000000004' );

INSERT INTO "BMC"."AchievementCategory" ( "name", "description", "iconCssClass", "sequence", "objectGuid" ) VALUES  ( 'Challenge', 'Achievements earned by competing in build challenges', 'fas fa-medal', 5, 'ac100001-0001-4000-8000-000000000005' );


-- Individual achievement definitions. Each achievement has criteria, point value, and rarity classification.
CREATE TABLE "BMC"."Achievement"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"achievementCategoryId" INT NOT NULL,		-- The category this achievement belongs to
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"iconCssClass" VARCHAR(100) NULL,		-- CSS class for the achievement icon/badge
	"iconImagePath" VARCHAR(250) NULL,		-- Optional path to a custom badge image (overrides CSS icon)
	"criteria" TEXT NULL,		-- Human-readable description of how to earn this achievement
	"criteriaCode" VARCHAR(250) NULL,		-- Machine-readable criteria code for automatic detection (e.g. 'parts_owned >= 10000')
	"pointValue" INT NOT NULL DEFAULT 10,		-- Point value when earned — contributes to the user's total achievement score
	"rarity" VARCHAR(50) NOT NULL,		-- Rarity classification: Common, Uncommon, Rare, Epic, Legendary
	"isActive" BOOLEAN NOT NULL DEFAULT true,		-- Whether this achievement can currently be earned
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "achievementCategoryId" FOREIGN KEY ("achievementCategoryId") REFERENCES "BMC"."AchievementCategory"("id")		-- Foreign key to the AchievementCategory table.
);
-- Index on the Achievement table's achievementCategoryId field.
CREATE INDEX "I_Achievement_achievementCategoryId" ON "BMC"."Achievement" ("achievementCategoryId")
;

-- Index on the Achievement table's name field.
CREATE INDEX "I_Achievement_name" ON "BMC"."Achievement" ("name")
;

-- Index on the Achievement table's active field.
CREATE INDEX "I_Achievement_active" ON "BMC"."Achievement" ("active")
;

-- Index on the Achievement table's deleted field.
CREATE INDEX "I_Achievement_deleted" ON "BMC"."Achievement" ("deleted")
;

INSERT INTO "BMC"."Achievement" ( "name", "description", "iconCssClass", "criteria", "criteriaCode", "pointValue", "rarity", "isActive", "sequence", "achievementCategoryId", "objectGuid" ) VALUES  ( 'First Brick', 'Added your first part to your collection', 'fas fa-cube', 'Add at least 1 part to any collection', 'parts_owned >= 1', 5, 'Common', false, 1, ( SELECT id FROM "AchievementCategory" WHERE "name" = 'Collection' LIMIT 1), 'a1100001-0001-4000-8000-000000000001' );

INSERT INTO "BMC"."Achievement" ( "name", "description", "iconCssClass", "criteria", "criteriaCode", "pointValue", "rarity", "isActive", "sequence", "achievementCategoryId", "objectGuid" ) VALUES  ( 'Brick Enthusiast', 'Own 1,000 parts across all collections', 'fas fa-cubes', 'Total parts owned reaches 1,000', 'parts_owned >= 1000', 25, 'Uncommon', false, 2, ( SELECT id FROM "AchievementCategory" WHERE "name" = 'Collection' LIMIT 1), 'a1100001-0001-4000-8000-000000000002' );

INSERT INTO "BMC"."Achievement" ( "name", "description", "iconCssClass", "criteria", "criteriaCode", "pointValue", "rarity", "isActive", "sequence", "achievementCategoryId", "objectGuid" ) VALUES  ( 'Brick Master', 'Own 10,000 parts across all collections', 'fas fa-warehouse', 'Total parts owned reaches 10,000', 'parts_owned >= 10000', 100, 'Rare', false, 3, ( SELECT id FROM "AchievementCategory" WHERE "name" = 'Collection' LIMIT 1), 'a1100001-0001-4000-8000-000000000003' );

INSERT INTO "BMC"."Achievement" ( "name", "description", "iconCssClass", "criteria", "criteriaCode", "pointValue", "rarity", "isActive", "sequence", "achievementCategoryId", "objectGuid" ) VALUES  ( 'First Creation', 'Published your first MOC to the gallery', 'fas fa-rocket', 'Publish at least 1 MOC to the gallery', 'mocs_published >= 1', 15, 'Common', false, 10, ( SELECT id FROM "AchievementCategory" WHERE "name" = 'Building' LIMIT 1), 'a1100001-0001-4000-8000-000000000010' );

INSERT INTO "BMC"."Achievement" ( "name", "description", "iconCssClass", "criteria", "criteriaCode", "pointValue", "rarity", "isActive", "sequence", "achievementCategoryId", "objectGuid" ) VALUES  ( 'Community Builder', 'Gained 10 followers', 'fas fa-user-friends', 'Reach 10 followers on your profile', 'followers >= 10', 20, 'Uncommon', false, 20, ( SELECT id FROM "AchievementCategory" WHERE "name" = 'Social' LIMIT 1), 'a1100001-0001-4000-8000-000000000020' );


-- Records of achievements earned by users. Created when a user meets an achievement's criteria.
CREATE TABLE "BMC"."UserAchievement"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"achievementId" INT NOT NULL,		-- The achievement earned
	"earnedDate" TIMESTAMP NOT NULL,		-- Date/time the achievement was earned
	"isDisplayed" BOOLEAN NOT NULL DEFAULT true,		-- Whether this achievement is displayed on the user's public profile showcase
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "achievementId" FOREIGN KEY ("achievementId") REFERENCES "BMC"."Achievement"("id"),		-- Foreign key to the Achievement table.
	CONSTRAINT "UC_UserAchievement_tenantGuid_achievementId" UNIQUE ( "tenantGuid", "achievementId") 		-- Uniqueness enforced on the UserAchievement table's tenantGuid and achievementId fields.
);
-- Index on the UserAchievement table's tenantGuid field.
CREATE INDEX "I_UserAchievement_tenantGuid" ON "BMC"."UserAchievement" ("tenantGuid")
;

-- Index on the UserAchievement table's tenantGuid,achievementId fields.
CREATE INDEX "I_UserAchievement_tenantGuid_achievementId" ON "BMC"."UserAchievement" ("tenantGuid", "achievementId")
;

-- Index on the UserAchievement table's tenantGuid,active fields.
CREATE INDEX "I_UserAchievement_tenantGuid_active" ON "BMC"."UserAchievement" ("tenantGuid", "active")
;

-- Index on the UserAchievement table's tenantGuid,deleted fields.
CREATE INDEX "I_UserAchievement_tenantGuid_deleted" ON "BMC"."UserAchievement" ("tenantGuid", "deleted")
;


-- Special display badges that can be awarded to users by moderators or earned through special events. Displayed prominently on user profiles.
CREATE TABLE "BMC"."UserBadge"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"iconCssClass" VARCHAR(100) NULL,		-- CSS class for the badge icon
	"iconImagePath" VARCHAR(250) NULL,		-- Optional path to a custom badge image
	"badgeColor" VARCHAR(10) NULL,		-- Optional accent colour for the badge display
	"isAutomatic" BOOLEAN NOT NULL DEFAULT false,		-- Whether this badge is automatically awarded (vs. manually by moderators)
	"automaticCriteriaCode" VARCHAR(250) NULL,		-- Machine-readable criteria for automatic badges (null for manual badges)
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the UserBadge table's name field.
CREATE INDEX "I_UserBadge_name" ON "BMC"."UserBadge" ("name")
;

-- Index on the UserBadge table's active field.
CREATE INDEX "I_UserBadge_active" ON "BMC"."UserBadge" ("active")
;

-- Index on the UserBadge table's deleted field.
CREATE INDEX "I_UserBadge_deleted" ON "BMC"."UserBadge" ("deleted")
;

INSERT INTO "BMC"."UserBadge" ( "name", "description", "iconCssClass", "isAutomatic", "sequence", "objectGuid" ) VALUES  ( 'Early Adopter', 'Joined the BMC community during the early access period', 'fas fa-star', false, 1, 'ab100001-0001-4000-8000-000000000001' );

INSERT INTO "BMC"."UserBadge" ( "name", "description", "iconCssClass", "isAutomatic", "sequence", "objectGuid" ) VALUES  ( 'Verified Builder', 'Identity verified by the BMC team', 'fas fa-check-circle', false, 2, 'ab100001-0001-4000-8000-000000000002' );

INSERT INTO "BMC"."UserBadge" ( "name", "description", "iconCssClass", "isAutomatic", "sequence", "objectGuid" ) VALUES  ( 'Top Contributor', 'One of the most active community contributors this month', 'fas fa-crown', false, 3, 'ab100001-0001-4000-8000-000000000003' );

INSERT INTO "BMC"."UserBadge" ( "name", "description", "iconCssClass", "isAutomatic", "sequence", "objectGuid" ) VALUES  ( 'Challenge Winner', 'Won a community build challenge', 'fas fa-award', false, 4, 'ab100001-0001-4000-8000-000000000004' );

INSERT INTO "BMC"."UserBadge" ( "name", "description", "iconCssClass", "isAutomatic", "sequence", "objectGuid" ) VALUES  ( 'Moderator', 'Community moderator trusted to help maintain quality', 'fas fa-shield-alt', false, 5, 'ab100001-0001-4000-8000-000000000005' );


-- Maps badges to users. A badge can be awarded multiple times conceptually, but one unique assignment per user per badge.
CREATE TABLE "BMC"."UserBadgeAssignment"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"userBadgeId" INT NOT NULL,		-- The badge awarded
	"awardedDate" TIMESTAMP NOT NULL,		-- Date/time the badge was awarded
	"awardedByTenantGuid" VARCHAR(50) NULL,		-- Tenant GUID of the moderator who awarded the badge (null for automatic badges)
	"reason" TEXT NULL,		-- Optional reason or context for awarding the badge
	"isDisplayed" BOOLEAN NOT NULL DEFAULT true,		-- Whether this badge is displayed on the user's profile
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "userBadgeId" FOREIGN KEY ("userBadgeId") REFERENCES "BMC"."UserBadge"("id"),		-- Foreign key to the UserBadge table.
	CONSTRAINT "UC_UserBadgeAssignment_tenantGuid_userBadgeId" UNIQUE ( "tenantGuid", "userBadgeId") 		-- Uniqueness enforced on the UserBadgeAssignment table's tenantGuid and userBadgeId fields.
);
-- Index on the UserBadgeAssignment table's tenantGuid field.
CREATE INDEX "I_UserBadgeAssignment_tenantGuid" ON "BMC"."UserBadgeAssignment" ("tenantGuid")
;

-- Index on the UserBadgeAssignment table's tenantGuid,userBadgeId fields.
CREATE INDEX "I_UserBadgeAssignment_tenantGuid_userBadgeId" ON "BMC"."UserBadgeAssignment" ("tenantGuid", "userBadgeId")
;

-- Index on the UserBadgeAssignment table's tenantGuid,active fields.
CREATE INDEX "I_UserBadgeAssignment_tenantGuid_active" ON "BMC"."UserBadgeAssignment" ("tenantGuid", "active")
;

-- Index on the UserBadgeAssignment table's tenantGuid,deleted fields.
CREATE INDEX "I_UserBadgeAssignment_tenantGuid_deleted" ON "BMC"."UserBadgeAssignment" ("tenantGuid", "deleted")
;


-- Community build challenges with themes, rules, and time windows. Created by moderators or admins.
CREATE TABLE "BMC"."BuildChallenge"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,		-- Title of the challenge (e.g. 'Under 100 Parts Technic Vehicle')
	"description" TEXT NULL,		-- Full description of the challenge theme and goals
	"rules" TEXT NULL,		-- Detailed rules and constraints for entries
	"thumbnailImagePath" VARCHAR(250) NULL,		-- Promotional image for the challenge
	"startDate" TIMESTAMP NOT NULL,		-- When submissions open
	"endDate" TIMESTAMP NOT NULL,		-- When submissions close
	"votingEndDate" TIMESTAMP NULL,		-- When community voting closes (null if no voting period)
	"isActive" BOOLEAN NOT NULL DEFAULT true,		-- Whether this challenge is currently active and accepting entries
	"isFeatured" BOOLEAN NOT NULL DEFAULT false,		-- Whether this challenge should be prominently displayed on the landing page
	"entryCount" INT NOT NULL DEFAULT 0,		-- Cached count of submitted entries
	"maxPartsLimit" INT NULL,		-- Optional maximum part count constraint for entries (null = no limit)
	"versionNumber" INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the BuildChallenge table's name field.
CREATE INDEX "I_BuildChallenge_name" ON "BMC"."BuildChallenge" ("name")
;

-- Index on the BuildChallenge table's active field.
CREATE INDEX "I_BuildChallenge_active" ON "BMC"."BuildChallenge" ("active")
;

-- Index on the BuildChallenge table's deleted field.
CREATE INDEX "I_BuildChallenge_deleted" ON "BMC"."BuildChallenge" ("deleted")
;


-- The change history for records from the BuildChallenge table.
CREATE TABLE "BMC"."BuildChallengeChangeHistory"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"buildChallengeId" INT NOT NULL,		-- Link to the BuildChallenge table.
	"versionNumber" INT NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" TIMESTAMP NOT NULL,		-- The time that the record version was created.
	"userId" INT NOT NULL,
	"data" TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	CONSTRAINT "buildChallengeId" FOREIGN KEY ("buildChallengeId") REFERENCES "BMC"."BuildChallenge"("id")		-- Foreign key to the BuildChallenge table.
);
-- Index on the BuildChallengeChangeHistory table's versionNumber field.
CREATE INDEX "I_BuildChallengeChangeHistory_versionNumber" ON "BMC"."BuildChallengeChangeHistory" ("versionNumber")
;

-- Index on the BuildChallengeChangeHistory table's timeStamp field.
CREATE INDEX "I_BuildChallengeChangeHistory_timeStamp" ON "BMC"."BuildChallengeChangeHistory" ("timeStamp")
;

-- Index on the BuildChallengeChangeHistory table's userId field.
CREATE INDEX "I_BuildChallengeChangeHistory_userId" ON "BMC"."BuildChallengeChangeHistory" ("userId")
;

-- Index on the BuildChallengeChangeHistory table's buildChallengeId field.
CREATE INDEX "I_BuildChallengeChangeHistory_buildChallengeId" ON "BMC"."BuildChallengeChangeHistory" ("buildChallengeId") INCLUDE ( versionNumber, timeStamp, userId )
;


-- User-submitted entries into a build challenge. Links to a published MOC.
CREATE TABLE "BMC"."BuildChallengeEntry"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"buildChallengeId" INT NOT NULL,		-- The challenge being entered
	"publishedMocId" INT NOT NULL,		-- The published MOC submitted as an entry
	"submittedDate" TIMESTAMP NOT NULL,		-- Date/time the entry was submitted
	"entryNotes" TEXT NULL,		-- Optional notes from the builder about their entry
	"voteCount" INT NOT NULL DEFAULT 0,		-- Cached community vote count
	"isWinner" BOOLEAN NOT NULL DEFAULT false,		-- Whether this entry was selected as a winner
	"isDisqualified" BOOLEAN NOT NULL DEFAULT false,		-- Whether this entry was disqualified by moderators
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "buildChallengeId" FOREIGN KEY ("buildChallengeId") REFERENCES "BMC"."BuildChallenge"("id"),		-- Foreign key to the BuildChallenge table.
	CONSTRAINT "publishedMocId" FOREIGN KEY ("publishedMocId") REFERENCES "BMC"."PublishedMoc"("id"),		-- Foreign key to the PublishedMoc table.
	CONSTRAINT "UC_BuildChallengeEntry_tenantGuid_buildChallengeId" UNIQUE ( "tenantGuid", "buildChallengeId") 		-- Uniqueness enforced on the BuildChallengeEntry table's tenantGuid and buildChallengeId fields.
);
-- Index on the BuildChallengeEntry table's tenantGuid field.
CREATE INDEX "I_BuildChallengeEntry_tenantGuid" ON "BMC"."BuildChallengeEntry" ("tenantGuid")
;

-- Index on the BuildChallengeEntry table's tenantGuid,buildChallengeId fields.
CREATE INDEX "I_BuildChallengeEntry_tenantGuid_buildChallengeId" ON "BMC"."BuildChallengeEntry" ("tenantGuid", "buildChallengeId")
;

-- Index on the BuildChallengeEntry table's tenantGuid,publishedMocId fields.
CREATE INDEX "I_BuildChallengeEntry_tenantGuid_publishedMocId" ON "BMC"."BuildChallengeEntry" ("tenantGuid", "publishedMocId")
;

-- Index on the BuildChallengeEntry table's tenantGuid,active fields.
CREATE INDEX "I_BuildChallengeEntry_tenantGuid_active" ON "BMC"."BuildChallengeEntry" ("tenantGuid", "active")
;

-- Index on the BuildChallengeEntry table's tenantGuid,deleted fields.
CREATE INDEX "I_BuildChallengeEntry_tenantGuid_deleted" ON "BMC"."BuildChallengeEntry" ("tenantGuid", "deleted")
;


-- Lookup table of reasons a user can report community content (Spam, Inappropriate, Copyright, etc.).
CREATE TABLE "BMC"."ContentReportReason"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,
	"description" VARCHAR(500) NOT NULL,
	"sequence" INT NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the ContentReportReason table's name field.
CREATE INDEX "I_ContentReportReason_name" ON "BMC"."ContentReportReason" ("name")
;

-- Index on the ContentReportReason table's active field.
CREATE INDEX "I_ContentReportReason_active" ON "BMC"."ContentReportReason" ("active")
;

-- Index on the ContentReportReason table's deleted field.
CREATE INDEX "I_ContentReportReason_deleted" ON "BMC"."ContentReportReason" ("deleted")
;

INSERT INTO "BMC"."ContentReportReason" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Spam', 'Content is spam, advertising, or promotional', 1, 'c4100001-0001-4000-8000-000000000001' );

INSERT INTO "BMC"."ContentReportReason" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Inappropriate', 'Content is offensive, vulgar, or inappropriate', 2, 'c4100001-0001-4000-8000-000000000002' );

INSERT INTO "BMC"."ContentReportReason" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Copyright', 'Content violates copyright or intellectual property', 3, 'c4100001-0001-4000-8000-000000000003' );

INSERT INTO "BMC"."ContentReportReason" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Harassment', 'Content constitutes harassment or bullying', 4, 'c4100001-0001-4000-8000-000000000004' );

INSERT INTO "BMC"."ContentReportReason" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Misinformation', 'Content contains misleading or false information', 5, 'c4100001-0001-4000-8000-000000000005' );

INSERT INTO "BMC"."ContentReportReason" ( "name", "description", "sequence", "objectGuid" ) VALUES  ( 'Other', 'Other reason not covered above', 99, 'c4100001-0001-4000-8000-000000000099' );


-- User-submitted reports of problematic community content. Reviewed by moderators via the BMC Admin project.
CREATE TABLE "BMC"."ContentReport"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"contentReportReasonId" INT NOT NULL,		-- The reason for the report
	"reporterTenantGuid" VARCHAR(50) NOT NULL,		-- Tenant GUID of the user submitting the report
	"reportedEntityType" VARCHAR(100) NOT NULL,		-- Type of the reported content (e.g. 'PublishedMoc', 'MocComment', 'UserProfile')
	"reportedEntityId" BIGINT NOT NULL,		-- ID of the reported entity
	"description" TEXT NULL,		-- Additional details provided by the reporter
	"status" VARCHAR(50) NOT NULL,		-- Report status: Pending, UnderReview, Dismissed, ActionTaken
	"reportedDate" TIMESTAMP NOT NULL,		-- Date/time the report was submitted
	"reviewedDate" TIMESTAMP NULL,		-- Date/time a moderator reviewed the report (null if pending)
	"reviewerTenantGuid" VARCHAR(50) NULL,		-- Tenant GUID of the moderator who reviewed (null if pending)
	"reviewNotes" TEXT NULL,		-- Moderator notes on the review decision
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "contentReportReasonId" FOREIGN KEY ("contentReportReasonId") REFERENCES "BMC"."ContentReportReason"("id")		-- Foreign key to the ContentReportReason table.
);
-- Index on the ContentReport table's contentReportReasonId field.
CREATE INDEX "I_ContentReport_contentReportReasonId" ON "BMC"."ContentReport" ("contentReportReasonId")
;

-- Index on the ContentReport table's active field.
CREATE INDEX "I_ContentReport_active" ON "BMC"."ContentReport" ("active")
;

-- Index on the ContentReport table's deleted field.
CREATE INDEX "I_ContentReport_deleted" ON "BMC"."ContentReport" ("deleted")
;


-- Audit log of actions taken by moderators. Immutable record for accountability.
CREATE TABLE "BMC"."ModerationAction"
(
	"id" BIGSERIAL PRIMARY KEY NOT NULL,
	"moderatorTenantGuid" VARCHAR(50) NOT NULL,		-- Tenant GUID of the moderator who took the action
	"actionType" VARCHAR(100) NOT NULL,		-- Type of action: Warning, ContentRemoved, ContentHidden, UserSuspended, UserBanned, BadgeAwarded
	"targetTenantGuid" VARCHAR(50) NULL,		-- Tenant GUID of the user the action was taken against (null for content-only actions)
	"targetEntityType" VARCHAR(100) NULL,		-- Type of the target entity (e.g. 'PublishedMoc', 'MocComment', 'UserProfile')
	"targetEntityId" BIGINT NULL,		-- ID of the target entity (null for user-level actions)
	"reason" TEXT NULL,		-- Reason for the moderation action
	"actionDate" TIMESTAMP NOT NULL,		-- Date/time the action was taken
	"contentReportId" INT NULL,		-- Optional link to the content report that triggered this action
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "contentReportId" FOREIGN KEY ("contentReportId") REFERENCES "BMC"."ContentReport"("id")		-- Foreign key to the ContentReport table.
);
-- Index on the ModerationAction table's contentReportId field.
CREATE INDEX "I_ModerationAction_contentReportId" ON "BMC"."ModerationAction" ("contentReportId")
;

-- Index on the ModerationAction table's active field.
CREATE INDEX "I_ModerationAction_active" ON "BMC"."ModerationAction" ("active")
;

-- Index on the ModerationAction table's deleted field.
CREATE INDEX "I_ModerationAction_deleted" ON "BMC"."ModerationAction" ("deleted")
;


-- Admin-created announcements displayed on the public landing page and/or dashboard. Time-windowed with priority ordering.
CREATE TABLE "BMC"."PlatformAnnouncement"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE,		-- Announcement headline/title
	"body" TEXT NULL,		-- Full announcement content (supports markdown or HTML)
	"announcementType" VARCHAR(50) NULL,		-- Type for styling: Info, Warning, Celebration, Maintenance
	"startDate" TIMESTAMP NOT NULL,		-- When the announcement becomes visible
	"endDate" TIMESTAMP NULL,		-- When the announcement expires (null = no expiry)
	"isActive" BOOLEAN NOT NULL DEFAULT true,		-- Whether the announcement is currently active
	"priority" INT NOT NULL DEFAULT 0,		-- Display priority (higher = more prominent)
	"showOnLandingPage" BOOLEAN NOT NULL DEFAULT true,		-- Whether to show on the public landing page
	"showOnDashboard" BOOLEAN NOT NULL DEFAULT true,		-- Whether to show on the authenticated user dashboard
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the PlatformAnnouncement table's name field.
CREATE INDEX "I_PlatformAnnouncement_name" ON "BMC"."PlatformAnnouncement" ("name")
;

-- Index on the PlatformAnnouncement table's active field.
CREATE INDEX "I_PlatformAnnouncement_active" ON "BMC"."PlatformAnnouncement" ("active")
;

-- Index on the PlatformAnnouncement table's deleted field.
CREATE INDEX "I_PlatformAnnouncement_deleted" ON "BMC"."PlatformAnnouncement" ("deleted")
;


-- API keys issued to users or external integrators for accessing the BMC Public API. Keys are stored as hashes for security.
CREATE TABLE "BMC"."ApiKey"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"keyHash" VARCHAR(250) NOT NULL,		-- SHA-256 hash of the API key (the plain key is shown once at creation, then discarded)
	"keyPrefix" VARCHAR(100) NOT NULL,		-- First 8 characters of the key for identification without exposing the full key
	"name" VARCHAR(100) NOT NULL,		-- User-defined name for the key (e.g. 'My BrickLink Integration')
	"description" TEXT NULL,		-- Optional description of what this key is used for
	"isActive" BOOLEAN NOT NULL DEFAULT true,		-- Whether this key is active and can authenticate requests
	"createdDate" TIMESTAMP NOT NULL,		-- Date/time the key was created
	"lastUsedDate" TIMESTAMP NULL,		-- Date/time the key was last used to make a request
	"expiresDate" TIMESTAMP NULL,		-- Optional expiry date (null = no expiry)
	"rateLimitPerHour" INT NOT NULL DEFAULT 1000,		-- Maximum API requests allowed per hour with this key
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_ApiKey_tenantGuid_name" UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the ApiKey table's tenantGuid and name fields.
);
-- Index on the ApiKey table's tenantGuid field.
CREATE INDEX "I_ApiKey_tenantGuid" ON "BMC"."ApiKey" ("tenantGuid")
;

-- Index on the ApiKey table's tenantGuid,name fields.
CREATE INDEX "I_ApiKey_tenantGuid_name" ON "BMC"."ApiKey" ("tenantGuid", "name")
;

-- Index on the ApiKey table's tenantGuid,active fields.
CREATE INDEX "I_ApiKey_tenantGuid_active" ON "BMC"."ApiKey" ("tenantGuid", "active")
;

-- Index on the ApiKey table's tenantGuid,deleted fields.
CREATE INDEX "I_ApiKey_tenantGuid_deleted" ON "BMC"."ApiKey" ("tenantGuid", "deleted")
;


-- Audit log of requests made through the BMC Public API. Used for rate limiting, usage analytics, and abuse detection.
CREATE TABLE "BMC"."ApiRequestLog"
(
	"id" BIGSERIAL PRIMARY KEY NOT NULL,
	"apiKeyId" INT NOT NULL,		-- The API key used for this request
	"endpoint" VARCHAR(250) NOT NULL,		-- The API endpoint that was called (e.g. '/api/v1/parts/3001')
	"httpMethod" VARCHAR(10) NOT NULL,		-- HTTP method (GET, POST, PUT, DELETE)
	"responseStatus" INT NOT NULL,		-- HTTP response status code (200, 401, 429, etc.)
	"requestDate" TIMESTAMP NOT NULL,		-- Date/time of the request
	"durationMs" INT NULL,		-- Request processing duration in milliseconds
	"clientIpAddress" VARCHAR(100) NULL,		-- IP address of the client making the request
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "apiKeyId" FOREIGN KEY ("apiKeyId") REFERENCES "BMC"."ApiKey"("id")		-- Foreign key to the ApiKey table.
);
-- Index on the ApiRequestLog table's apiKeyId field.
CREATE INDEX "I_ApiRequestLog_apiKeyId" ON "BMC"."ApiRequestLog" ("apiKeyId")
;

-- Index on the ApiRequestLog table's active field.
CREATE INDEX "I_ApiRequestLog_active" ON "BMC"."ApiRequestLog" ("active")
;

-- Index on the ApiRequestLog table's deleted field.
CREATE INDEX "I_ApiRequestLog_deleted" ON "BMC"."ApiRequestLog" ("deleted")
;


-- Tracks self-service user registrations through the two-step email verification process. Stores pending registrations until email verification is completed, then provisions the SecurityUser, SecurityTenant, and UserProfile. Designed for auditing and reporting on the registration funnel.
CREATE TABLE "BMC"."PendingRegistration"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"accountName" VARCHAR(250) NOT NULL,		-- The requested username for the new account
	"emailAddress" VARCHAR(100) NOT NULL,		-- The email address to verify
	"displayName" VARCHAR(250) NULL,		-- Optional display name for the profile (defaults to accountName if not provided)
	"passwordHash" VARCHAR(250) NOT NULL,		-- Pre-hashed password stored during the pending period
	"verificationCode" VARCHAR(50) NOT NULL,		-- The code or token sent to the user for verification (email, SMS, OTP)
	"codeExpiresAt" TIMESTAMP NOT NULL,		-- When the verification code expires (default 15 minutes from creation)
	"verificationAttempts" INT NOT NULL DEFAULT 0,		-- Number of times the user has attempted to enter the verification code
	"status" VARCHAR(50) NOT NULL,		-- Registration status: Pending, Verified, Provisioned, Expired, Failed
	"createdAt" TIMESTAMP NOT NULL,		-- When the registration was initiated
	"verifiedAt" TIMESTAMP NULL,		-- When the verification code was successfully validated
	"provisionedAt" TIMESTAMP NULL,		-- When the SecurityUser and SecurityTenant were created
	"ipAddress" VARCHAR(100) NULL,		-- Client IP address for security auditing
	"userAgent" VARCHAR(500) NULL,		-- Client user agent for security auditing
	"verificationChannel" VARCHAR(50) NULL,		-- Channel used for verification: Email, SMS, OTP (default Email)
	"failureReason" VARCHAR(1000) NULL,		-- Reason for failure if status is Failed
	"provisionedSecurityUserId" INT NULL,		-- The SecurityUser.id created on successful provisioning, for cross-referencing
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the PendingRegistration table's accountName field.
CREATE INDEX "I_PendingRegistration_accountName" ON "BMC"."PendingRegistration" ("accountName")
;

-- Index on the PendingRegistration table's emailAddress field.
CREATE INDEX "I_PendingRegistration_emailAddress" ON "BMC"."PendingRegistration" ("emailAddress")
;

-- Index on the PendingRegistration table's verificationCode field.
CREATE INDEX "I_PendingRegistration_verificationCode" ON "BMC"."PendingRegistration" ("verificationCode")
;

-- Index on the PendingRegistration table's codeExpiresAt field.
CREATE INDEX "I_PendingRegistration_codeExpiresAt" ON "BMC"."PendingRegistration" ("codeExpiresAt")
;

-- Index on the PendingRegistration table's status field.
CREATE INDEX "I_PendingRegistration_status" ON "BMC"."PendingRegistration" ("status")
;

-- Index on the PendingRegistration table's createdAt field.
CREATE INDEX "I_PendingRegistration_createdAt" ON "BMC"."PendingRegistration" ("createdAt")
;

-- Index on the PendingRegistration table's active field.
CREATE INDEX "I_PendingRegistration_active" ON "BMC"."PendingRegistration" ("active")
;

-- Index on the PendingRegistration table's deleted field.
CREATE INDEX "I_PendingRegistration_deleted" ON "BMC"."PendingRegistration" ("deleted")
;

-- Index on the PendingRegistration table's status,codeExpiresAt,active,deleted fields.
CREATE INDEX "I_PendingRegistration_status_codeExpiresAt_active_deleted" ON "BMC"."PendingRegistration" ("status", "codeExpiresAt", "active", "deleted")
;


-- Stores per-tenant BrickLink OAuth 1.0 token credentials and sync state. The consumer key/secret are stored in appsettings.json; the per-user token value/secret are stored here encrypted.
CREATE TABLE "BMC"."BrickLinkUserLink"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"encryptedTokenValue" VARCHAR(500) NULL,		-- Encrypted OAuth 1.0 token value — encrypted via ASP.NET Data Protection
	"encryptedTokenSecret" VARCHAR(500) NULL,		-- Encrypted OAuth 1.0 token secret — encrypted via ASP.NET Data Protection
	"syncEnabled" BOOLEAN NOT NULL DEFAULT false,		-- Whether automatic sync is enabled for this tenant
	"syncDirection" VARCHAR(50) NULL,		-- Sync direction: Pull, Push, or Both (null = Pull)
	"lastSyncDate" TIMESTAMP NULL,		-- Date/time of the last successful sync operation
	"lastPullDate" TIMESTAMP NULL,		-- Date/time of the last successful pull from BrickLink
	"lastPushDate" TIMESTAMP NULL,		-- Date/time of the last successful push to BrickLink
	"lastSyncError" VARCHAR(1000) NULL,		-- Error message from the last failed sync attempt (null = no errors)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_BrickLinkUserLink_tenantGuid" UNIQUE ( "tenantGuid") 		-- Uniqueness enforced on the BrickLinkUserLink table's tenantGuid field.
);
-- Index on the BrickLinkUserLink table's tenantGuid field.
CREATE INDEX "I_BrickLinkUserLink_tenantGuid" ON "BMC"."BrickLinkUserLink" ("tenantGuid")
;

-- Index on the BrickLinkUserLink table's tenantGuid,active fields.
CREATE INDEX "I_BrickLinkUserLink_tenantGuid_active" ON "BMC"."BrickLinkUserLink" ("tenantGuid", "active")
;

-- Index on the BrickLinkUserLink table's tenantGuid,deleted fields.
CREATE INDEX "I_BrickLinkUserLink_tenantGuid_deleted" ON "BMC"."BrickLinkUserLink" ("tenantGuid", "deleted")
;


-- Stores per-tenant BrickEconomy Premium API key and sync state. BrickEconomy provides AI/ML-powered set and minifig valuations. Rate limited to 100 requests per day.
CREATE TABLE "BMC"."BrickEconomyUserLink"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"encryptedApiKey" VARCHAR(500) NULL,		-- Encrypted BrickEconomy Premium API key — encrypted via ASP.NET Data Protection
	"syncEnabled" BOOLEAN NOT NULL DEFAULT false,		-- Whether automatic sync is enabled for this tenant
	"lastSyncDate" TIMESTAMP NULL,		-- Date/time of the last successful sync operation
	"lastSyncError" VARCHAR(1000) NULL,		-- Error message from the last failed sync attempt (null = no errors)
	"dailyQuotaUsed" INT NULL,		-- Number of API requests used today against the 100/day quota (reset at 00:00 UTC)
	"quotaResetDate" TIMESTAMP NULL,		-- Date when the daily quota counter was last reset
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_BrickEconomyUserLink_tenantGuid" UNIQUE ( "tenantGuid") 		-- Uniqueness enforced on the BrickEconomyUserLink table's tenantGuid field.
);
-- Index on the BrickEconomyUserLink table's tenantGuid field.
CREATE INDEX "I_BrickEconomyUserLink_tenantGuid" ON "BMC"."BrickEconomyUserLink" ("tenantGuid")
;

-- Index on the BrickEconomyUserLink table's tenantGuid,active fields.
CREATE INDEX "I_BrickEconomyUserLink_tenantGuid_active" ON "BMC"."BrickEconomyUserLink" ("tenantGuid", "active")
;

-- Index on the BrickEconomyUserLink table's tenantGuid,deleted fields.
CREATE INDEX "I_BrickEconomyUserLink_tenantGuid_deleted" ON "BMC"."BrickEconomyUserLink" ("tenantGuid", "deleted")
;


-- Stores per-tenant Brick Owl API key and sync state. Brick Owl is the second-largest LEGO marketplace, providing catalog lookups, cross-platform ID mapping, and collection management.
CREATE TABLE "BMC"."BrickOwlUserLink"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"encryptedApiKey" VARCHAR(500) NULL,		-- Encrypted Brick Owl API key — encrypted via ASP.NET Data Protection
	"syncEnabled" BOOLEAN NOT NULL DEFAULT false,		-- Whether automatic sync is enabled for this tenant
	"syncDirection" VARCHAR(50) NULL,		-- Sync direction: Pull, Push, or Both (null = Pull)
	"lastSyncDate" TIMESTAMP NULL,		-- Date/time of the last successful sync operation
	"lastPullDate" TIMESTAMP NULL,		-- Date/time of the last successful pull from Brick Owl
	"lastPushDate" TIMESTAMP NULL,		-- Date/time of the last successful push to Brick Owl
	"lastSyncError" VARCHAR(1000) NULL,		-- Error message from the last failed sync attempt (null = no errors)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_BrickOwlUserLink_tenantGuid" UNIQUE ( "tenantGuid") 		-- Uniqueness enforced on the BrickOwlUserLink table's tenantGuid field.
);
-- Index on the BrickOwlUserLink table's tenantGuid field.
CREATE INDEX "I_BrickOwlUserLink_tenantGuid" ON "BMC"."BrickOwlUserLink" ("tenantGuid")
;

-- Index on the BrickOwlUserLink table's tenantGuid,active fields.
CREATE INDEX "I_BrickOwlUserLink_tenantGuid_active" ON "BMC"."BrickOwlUserLink" ("tenantGuid", "active")
;

-- Index on the BrickOwlUserLink table's tenantGuid,deleted fields.
CREATE INDEX "I_BrickOwlUserLink_tenantGuid_deleted" ON "BMC"."BrickOwlUserLink" ("tenantGuid", "deleted")
;


-- Full audit log of every BrickLink API call BMC makes on behalf of a user. Mirrors the BrickSetTransaction pattern for complete transparency.
CREATE TABLE "BMC"."BrickLinkTransaction"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"transactionDate" TIMESTAMP NULL,		-- Date/time the API call was made
	"direction" VARCHAR(50) NOT NULL,		-- Direction of data flow: Push, Pull, Enrich
	"methodName" VARCHAR(100) NOT NULL,		-- BrickLink API method name (e.g. 'getItem', 'getPriceGuide', 'getSubsets')
	"requestSummary" TEXT NULL,		-- Human-readable description of the operation
	"success" BOOLEAN NOT NULL DEFAULT true,		-- Whether the API call completed successfully
	"errorMessage" TEXT NULL,		-- Error details if the call failed (null on success)
	"triggeredBy" VARCHAR(100) NOT NULL,		-- What initiated this call: UserAction, SetDetailView, PriceGuide
	"recordCount" INT NULL,		-- Number of rows retrieved or affected by this API call
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the BrickLinkTransaction table's tenantGuid field.
CREATE INDEX "I_BrickLinkTransaction_tenantGuid" ON "BMC"."BrickLinkTransaction" ("tenantGuid")
;

-- Index on the BrickLinkTransaction table's tenantGuid,active fields.
CREATE INDEX "I_BrickLinkTransaction_tenantGuid_active" ON "BMC"."BrickLinkTransaction" ("tenantGuid", "active")
;

-- Index on the BrickLinkTransaction table's tenantGuid,deleted fields.
CREATE INDEX "I_BrickLinkTransaction_tenantGuid_deleted" ON "BMC"."BrickLinkTransaction" ("tenantGuid", "deleted")
;


-- Full audit log of every BrickEconomy API call BMC makes on behalf of a user. Tracks daily quota usage for the 100-request-per-day limit.
CREATE TABLE "BMC"."BrickEconomyTransaction"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"transactionDate" TIMESTAMP NULL,		-- Date/time the API call was made
	"direction" VARCHAR(50) NOT NULL,		-- Direction of data flow: Pull, Enrich
	"methodName" VARCHAR(100) NOT NULL,		-- BrickEconomy API method name (e.g. 'getSet', 'getMinifig', 'getSalesLedger')
	"requestSummary" TEXT NULL,		-- Human-readable description of the operation
	"success" BOOLEAN NOT NULL DEFAULT true,		-- Whether the API call completed successfully
	"errorMessage" TEXT NULL,		-- Error details if the call failed (null on success)
	"triggeredBy" VARCHAR(100) NOT NULL,		-- What initiated this call: UserAction, SetDetailView, Valuation
	"recordCount" INT NULL,		-- Number of rows retrieved or affected by this API call
	"dailyQuotaRemaining" INT NULL,		-- Daily API quota remaining after this call (out of 100)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the BrickEconomyTransaction table's tenantGuid field.
CREATE INDEX "I_BrickEconomyTransaction_tenantGuid" ON "BMC"."BrickEconomyTransaction" ("tenantGuid")
;

-- Index on the BrickEconomyTransaction table's tenantGuid,active fields.
CREATE INDEX "I_BrickEconomyTransaction_tenantGuid_active" ON "BMC"."BrickEconomyTransaction" ("tenantGuid", "active")
;

-- Index on the BrickEconomyTransaction table's tenantGuid,deleted fields.
CREATE INDEX "I_BrickEconomyTransaction_tenantGuid_deleted" ON "BMC"."BrickEconomyTransaction" ("tenantGuid", "deleted")
;


-- Full audit log of every Brick Owl API call BMC makes on behalf of a user. Mirrors the BrickSetTransaction pattern for complete transparency.
CREATE TABLE "BMC"."BrickOwlTransaction"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL,		-- The guid for the Tenant to which this record belongs.
	"transactionDate" TIMESTAMP NULL,		-- Date/time the API call was made
	"direction" VARCHAR(50) NOT NULL,		-- Direction of data flow: Push, Pull
	"methodName" VARCHAR(100) NOT NULL,		-- Brick Owl API method name (e.g. 'catalogLookup', 'idLookup', 'getCollection')
	"requestSummary" TEXT NULL,		-- Human-readable description of the operation
	"success" BOOLEAN NOT NULL DEFAULT true,		-- Whether the API call completed successfully
	"errorMessage" TEXT NULL,		-- Error details if the call failed (null on success)
	"triggeredBy" VARCHAR(100) NOT NULL,		-- What initiated this call: UserAction, CatalogLookup, IdMapping
	"recordCount" INT NULL,		-- Number of rows retrieved or affected by this API call
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false		-- Soft deletion flag.

);
-- Index on the BrickOwlTransaction table's tenantGuid field.
CREATE INDEX "I_BrickOwlTransaction_tenantGuid" ON "BMC"."BrickOwlTransaction" ("tenantGuid")
;

-- Index on the BrickOwlTransaction table's tenantGuid,active fields.
CREATE INDEX "I_BrickOwlTransaction_tenantGuid_active" ON "BMC"."BrickOwlTransaction" ("tenantGuid", "active")
;

-- Index on the BrickOwlTransaction table's tenantGuid,deleted fields.
CREATE INDEX "I_BrickOwlTransaction_tenantGuid_deleted" ON "BMC"."BrickOwlTransaction" ("tenantGuid", "deleted")
;


-- Caches API responses from external marketplaces (BrickLink, BrickEconomy, BrickOwl) to reduce external calls and improve Brickberg Terminal performance. TTL-based expiry with configurable per-source cache durations. Not multi-tenant — cache is shared across all users for the same item.
CREATE TABLE "BMC"."MarketDataCache"
(
	"id" SERIAL PRIMARY KEY NOT NULL,
	"source" VARCHAR(50) NOT NULL,		-- Marketplace source: BrickLink, BrickEconomy, BrickOwl
	"itemType" VARCHAR(50) NOT NULL,		-- Item type: SET, MINIFIG, PART
	"itemNumber" VARCHAR(100) NOT NULL,		-- Item identifier (e.g. '42131-1', 'fig-001234')
	"condition" VARCHAR(50) NULL,		-- Condition qualifier: N (new), U (used), null for non-BrickLink sources
	"responseJson" TEXT NULL,		-- Serialized JSON API response payload
	"fetchedDate" TIMESTAMP NOT NULL,		-- UTC timestamp when the response was fetched from the source API
	"expiresDate" TIMESTAMP NOT NULL,		-- UTC timestamp when this cache entry expires and should be refreshed
	"ttlMinutes" INT NOT NULL,		-- TTL in minutes that was used when caching this entry (for diagnostics/auditing)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE,		-- Unique identifier for this table.
	"active" BOOLEAN NOT NULL DEFAULT true,		-- Active from a business perspective flag.
	"deleted" BOOLEAN NOT NULL DEFAULT false,		-- Soft deletion flag.
	CONSTRAINT "UC_MarketDataCache_source_itemType_itemNumber_condition" UNIQUE ( "source", "itemType", "itemNumber", "condition") 		-- Uniqueness enforced on the MarketDataCache table's source and itemType and itemNumber and condition fields.
);
-- Index on the MarketDataCache table's active field.
CREATE INDEX "I_MarketDataCache_active" ON "BMC"."MarketDataCache" ("active")
;

-- Index on the MarketDataCache table's deleted field.
CREATE INDEX "I_MarketDataCache_deleted" ON "BMC"."MarketDataCache" ("deleted")
;


