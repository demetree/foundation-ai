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
-- DROP TABLE [BMC].[MarketDataCache]
-- DROP TABLE [BMC].[BrickOwlTransaction]
-- DROP TABLE [BMC].[BrickEconomyTransaction]
-- DROP TABLE [BMC].[BrickLinkTransaction]
-- DROP TABLE [BMC].[BrickOwlUserLink]
-- DROP TABLE [BMC].[BrickEconomyUserLink]
-- DROP TABLE [BMC].[BrickLinkUserLink]
-- DROP TABLE [BMC].[PendingRegistration]
-- DROP TABLE [BMC].[ApiRequestLog]
-- DROP TABLE [BMC].[ApiKey]
-- DROP TABLE [BMC].[PlatformAnnouncement]
-- DROP TABLE [BMC].[ModerationAction]
-- DROP TABLE [BMC].[ContentReport]
-- DROP TABLE [BMC].[ContentReportReason]
-- DROP TABLE [BMC].[BuildChallengeEntry]
-- DROP TABLE [BMC].[BuildChallengeChangeHistory]
-- DROP TABLE [BMC].[BuildChallenge]
-- DROP TABLE [BMC].[UserBadgeAssignment]
-- DROP TABLE [BMC].[UserBadge]
-- DROP TABLE [BMC].[UserAchievement]
-- DROP TABLE [BMC].[Achievement]
-- DROP TABLE [BMC].[AchievementCategory]
-- DROP TABLE [BMC].[SharedInstructionChangeHistory]
-- DROP TABLE [BMC].[SharedInstruction]
-- DROP TABLE [BMC].[MocFavourite]
-- DROP TABLE [BMC].[MocComment]
-- DROP TABLE [BMC].[MocLike]
-- DROP TABLE [BMC].[PublishedMocImage]
-- DROP TABLE [BMC].[PublishedMocChangeHistory]
-- DROP TABLE [BMC].[PublishedMoc]
-- DROP TABLE [BMC].[ActivityEvent]
-- DROP TABLE [BMC].[ActivityEventType]
-- DROP TABLE [BMC].[UserFollow]
-- DROP TABLE [BMC].[UserProfileStat]
-- DROP TABLE [BMC].[UserSetOwnership]
-- DROP TABLE [BMC].[UserProfilePreferredTheme]
-- DROP TABLE [BMC].[UserProfileLink]
-- DROP TABLE [BMC].[UserProfileLinkType]
-- DROP TABLE [BMC].[UserProfileChangeHistory]
-- DROP TABLE [BMC].[UserProfile]
-- DROP TABLE [BMC].[ProjectExport]
-- DROP TABLE [BMC].[ExportFormat]
-- DROP TABLE [BMC].[ProjectRender]
-- DROP TABLE [BMC].[RenderPreset]
-- DROP TABLE [BMC].[BuildStepAnnotation]
-- DROP TABLE [BMC].[BuildStepAnnotationType]
-- DROP TABLE [BMC].[BuildStepPart]
-- DROP TABLE [BMC].[BuildManualStep]
-- DROP TABLE [BMC].[BuildManualPage]
-- DROP TABLE [BMC].[BuildManualChangeHistory]
-- DROP TABLE [BMC].[BuildManual]
-- DROP TABLE [BMC].[UserLostPart]
-- DROP TABLE [BMC].[UserSetListItem]
-- DROP TABLE [BMC].[UserSetListChangeHistory]
-- DROP TABLE [BMC].[UserSetList]
-- DROP TABLE [BMC].[UserPartListItem]
-- DROP TABLE [BMC].[UserPartListChangeHistory]
-- DROP TABLE [BMC].[UserPartList]
-- DROP TABLE [BMC].[BrickSetSetReview]
-- DROP TABLE [BMC].[BrickSetTransaction]
-- DROP TABLE [BMC].[BrickSetUserLink]
-- DROP TABLE [BMC].[RebrickableSyncQueue]
-- DROP TABLE [BMC].[RebrickableTransaction]
-- DROP TABLE [BMC].[RebrickableUserLink]
-- DROP TABLE [BMC].[UserCollectionSetImport]
-- DROP TABLE [BMC].[UserWishlistItem]
-- DROP TABLE [BMC].[UserCollectionPart]
-- DROP TABLE [BMC].[UserCollectionChangeHistory]
-- DROP TABLE [BMC].[UserCollection]
-- DROP TABLE [BMC].[LegoSetSubset]
-- DROP TABLE [BMC].[LegoSetMinifig]
-- DROP TABLE [BMC].[LegoMinifig]
-- DROP TABLE [BMC].[BrickElement]
-- DROP TABLE [BMC].[BrickPartRelationship]
-- DROP TABLE [BMC].[LegoSetPart]
-- DROP TABLE [BMC].[LegoSet]
-- DROP TABLE [BMC].[LegoTheme]
-- DROP TABLE [BMC].[ModelStepPart]
-- DROP TABLE [BMC].[ModelBuildStep]
-- DROP TABLE [BMC].[ModelSubFile]
-- DROP TABLE [BMC].[ModelDocumentChangeHistory]
-- DROP TABLE [BMC].[ModelDocument]
-- DROP TABLE [BMC].[ProjectReferenceImage]
-- DROP TABLE [BMC].[ProjectCameraPreset]
-- DROP TABLE [BMC].[ProjectTagAssignment]
-- DROP TABLE [BMC].[ProjectTag]
-- DROP TABLE [BMC].[SubmodelInstance]
-- DROP TABLE [BMC].[SubmodelPlacedBrick]
-- DROP TABLE [BMC].[SubmodelChangeHistory]
-- DROP TABLE [BMC].[Submodel]
-- DROP TABLE [BMC].[BrickConnection]
-- DROP TABLE [BMC].[PlacedBrickChangeHistory]
-- DROP TABLE [BMC].[PlacedBrick]
-- DROP TABLE [BMC].[ProjectChangeHistory]
-- DROP TABLE [BMC].[Project]
-- DROP TABLE [BMC].[PartSubFileReference]
-- DROP TABLE [BMC].[BrickPartColour]
-- DROP TABLE [BMC].[BrickPartConnector]
-- DROP TABLE [BMC].[BrickPartChangeHistory]
-- DROP TABLE [BMC].[BrickPart]
-- DROP TABLE [BMC].[PartType]
-- DROP TABLE [BMC].[BrickColour]
-- DROP TABLE [BMC].[ColourFinish]
-- DROP TABLE [BMC].[ConnectorTypeCompatibility]
-- DROP TABLE [BMC].[ConnectorType]
-- DROP TABLE [BMC].[BrickCategory]

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON [BMC].[MarketDataCache] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickOwlTransaction] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickEconomyTransaction] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickLinkTransaction] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickOwlUserLink] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickEconomyUserLink] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickLinkUserLink] DISABLE
-- ALTER INDEX ALL ON [BMC].[PendingRegistration] DISABLE
-- ALTER INDEX ALL ON [BMC].[ApiRequestLog] DISABLE
-- ALTER INDEX ALL ON [BMC].[ApiKey] DISABLE
-- ALTER INDEX ALL ON [BMC].[PlatformAnnouncement] DISABLE
-- ALTER INDEX ALL ON [BMC].[ModerationAction] DISABLE
-- ALTER INDEX ALL ON [BMC].[ContentReport] DISABLE
-- ALTER INDEX ALL ON [BMC].[ContentReportReason] DISABLE
-- ALTER INDEX ALL ON [BMC].[BuildChallengeEntry] DISABLE
-- ALTER INDEX ALL ON [BMC].[BuildChallengeChangeHistory] DISABLE
-- ALTER INDEX ALL ON [BMC].[BuildChallenge] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserBadgeAssignment] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserBadge] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserAchievement] DISABLE
-- ALTER INDEX ALL ON [BMC].[Achievement] DISABLE
-- ALTER INDEX ALL ON [BMC].[AchievementCategory] DISABLE
-- ALTER INDEX ALL ON [BMC].[SharedInstructionChangeHistory] DISABLE
-- ALTER INDEX ALL ON [BMC].[SharedInstruction] DISABLE
-- ALTER INDEX ALL ON [BMC].[MocFavourite] DISABLE
-- ALTER INDEX ALL ON [BMC].[MocComment] DISABLE
-- ALTER INDEX ALL ON [BMC].[MocLike] DISABLE
-- ALTER INDEX ALL ON [BMC].[PublishedMocImage] DISABLE
-- ALTER INDEX ALL ON [BMC].[PublishedMocChangeHistory] DISABLE
-- ALTER INDEX ALL ON [BMC].[PublishedMoc] DISABLE
-- ALTER INDEX ALL ON [BMC].[ActivityEvent] DISABLE
-- ALTER INDEX ALL ON [BMC].[ActivityEventType] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserFollow] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserProfileStat] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserSetOwnership] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserProfilePreferredTheme] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserProfileLink] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserProfileLinkType] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserProfileChangeHistory] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserProfile] DISABLE
-- ALTER INDEX ALL ON [BMC].[ProjectExport] DISABLE
-- ALTER INDEX ALL ON [BMC].[ExportFormat] DISABLE
-- ALTER INDEX ALL ON [BMC].[ProjectRender] DISABLE
-- ALTER INDEX ALL ON [BMC].[RenderPreset] DISABLE
-- ALTER INDEX ALL ON [BMC].[BuildStepAnnotation] DISABLE
-- ALTER INDEX ALL ON [BMC].[BuildStepAnnotationType] DISABLE
-- ALTER INDEX ALL ON [BMC].[BuildStepPart] DISABLE
-- ALTER INDEX ALL ON [BMC].[BuildManualStep] DISABLE
-- ALTER INDEX ALL ON [BMC].[BuildManualPage] DISABLE
-- ALTER INDEX ALL ON [BMC].[BuildManualChangeHistory] DISABLE
-- ALTER INDEX ALL ON [BMC].[BuildManual] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserLostPart] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserSetListItem] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserSetListChangeHistory] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserSetList] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserPartListItem] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserPartListChangeHistory] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserPartList] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickSetSetReview] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickSetTransaction] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickSetUserLink] DISABLE
-- ALTER INDEX ALL ON [BMC].[RebrickableSyncQueue] DISABLE
-- ALTER INDEX ALL ON [BMC].[RebrickableTransaction] DISABLE
-- ALTER INDEX ALL ON [BMC].[RebrickableUserLink] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserCollectionSetImport] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserWishlistItem] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserCollectionPart] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserCollectionChangeHistory] DISABLE
-- ALTER INDEX ALL ON [BMC].[UserCollection] DISABLE
-- ALTER INDEX ALL ON [BMC].[LegoSetSubset] DISABLE
-- ALTER INDEX ALL ON [BMC].[LegoSetMinifig] DISABLE
-- ALTER INDEX ALL ON [BMC].[LegoMinifig] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickElement] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickPartRelationship] DISABLE
-- ALTER INDEX ALL ON [BMC].[LegoSetPart] DISABLE
-- ALTER INDEX ALL ON [BMC].[LegoSet] DISABLE
-- ALTER INDEX ALL ON [BMC].[LegoTheme] DISABLE
-- ALTER INDEX ALL ON [BMC].[ModelStepPart] DISABLE
-- ALTER INDEX ALL ON [BMC].[ModelBuildStep] DISABLE
-- ALTER INDEX ALL ON [BMC].[ModelSubFile] DISABLE
-- ALTER INDEX ALL ON [BMC].[ModelDocumentChangeHistory] DISABLE
-- ALTER INDEX ALL ON [BMC].[ModelDocument] DISABLE
-- ALTER INDEX ALL ON [BMC].[ProjectReferenceImage] DISABLE
-- ALTER INDEX ALL ON [BMC].[ProjectCameraPreset] DISABLE
-- ALTER INDEX ALL ON [BMC].[ProjectTagAssignment] DISABLE
-- ALTER INDEX ALL ON [BMC].[ProjectTag] DISABLE
-- ALTER INDEX ALL ON [BMC].[SubmodelInstance] DISABLE
-- ALTER INDEX ALL ON [BMC].[SubmodelPlacedBrick] DISABLE
-- ALTER INDEX ALL ON [BMC].[SubmodelChangeHistory] DISABLE
-- ALTER INDEX ALL ON [BMC].[Submodel] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickConnection] DISABLE
-- ALTER INDEX ALL ON [BMC].[PlacedBrickChangeHistory] DISABLE
-- ALTER INDEX ALL ON [BMC].[PlacedBrick] DISABLE
-- ALTER INDEX ALL ON [BMC].[ProjectChangeHistory] DISABLE
-- ALTER INDEX ALL ON [BMC].[Project] DISABLE
-- ALTER INDEX ALL ON [BMC].[PartSubFileReference] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickPartColour] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickPartConnector] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickPartChangeHistory] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickPart] DISABLE
-- ALTER INDEX ALL ON [BMC].[PartType] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickColour] DISABLE
-- ALTER INDEX ALL ON [BMC].[ColourFinish] DISABLE
-- ALTER INDEX ALL ON [BMC].[ConnectorTypeCompatibility] DISABLE
-- ALTER INDEX ALL ON [BMC].[ConnectorType] DISABLE
-- ALTER INDEX ALL ON [BMC].[BrickCategory] DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON [BMC].[MarketDataCache] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickOwlTransaction] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickEconomyTransaction] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickLinkTransaction] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickOwlUserLink] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickEconomyUserLink] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickLinkUserLink] REBUILD
-- ALTER INDEX ALL ON [BMC].[PendingRegistration] REBUILD
-- ALTER INDEX ALL ON [BMC].[ApiRequestLog] REBUILD
-- ALTER INDEX ALL ON [BMC].[ApiKey] REBUILD
-- ALTER INDEX ALL ON [BMC].[PlatformAnnouncement] REBUILD
-- ALTER INDEX ALL ON [BMC].[ModerationAction] REBUILD
-- ALTER INDEX ALL ON [BMC].[ContentReport] REBUILD
-- ALTER INDEX ALL ON [BMC].[ContentReportReason] REBUILD
-- ALTER INDEX ALL ON [BMC].[BuildChallengeEntry] REBUILD
-- ALTER INDEX ALL ON [BMC].[BuildChallengeChangeHistory] REBUILD
-- ALTER INDEX ALL ON [BMC].[BuildChallenge] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserBadgeAssignment] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserBadge] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserAchievement] REBUILD
-- ALTER INDEX ALL ON [BMC].[Achievement] REBUILD
-- ALTER INDEX ALL ON [BMC].[AchievementCategory] REBUILD
-- ALTER INDEX ALL ON [BMC].[SharedInstructionChangeHistory] REBUILD
-- ALTER INDEX ALL ON [BMC].[SharedInstruction] REBUILD
-- ALTER INDEX ALL ON [BMC].[MocFavourite] REBUILD
-- ALTER INDEX ALL ON [BMC].[MocComment] REBUILD
-- ALTER INDEX ALL ON [BMC].[MocLike] REBUILD
-- ALTER INDEX ALL ON [BMC].[PublishedMocImage] REBUILD
-- ALTER INDEX ALL ON [BMC].[PublishedMocChangeHistory] REBUILD
-- ALTER INDEX ALL ON [BMC].[PublishedMoc] REBUILD
-- ALTER INDEX ALL ON [BMC].[ActivityEvent] REBUILD
-- ALTER INDEX ALL ON [BMC].[ActivityEventType] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserFollow] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserProfileStat] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserSetOwnership] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserProfilePreferredTheme] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserProfileLink] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserProfileLinkType] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserProfileChangeHistory] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserProfile] REBUILD
-- ALTER INDEX ALL ON [BMC].[ProjectExport] REBUILD
-- ALTER INDEX ALL ON [BMC].[ExportFormat] REBUILD
-- ALTER INDEX ALL ON [BMC].[ProjectRender] REBUILD
-- ALTER INDEX ALL ON [BMC].[RenderPreset] REBUILD
-- ALTER INDEX ALL ON [BMC].[BuildStepAnnotation] REBUILD
-- ALTER INDEX ALL ON [BMC].[BuildStepAnnotationType] REBUILD
-- ALTER INDEX ALL ON [BMC].[BuildStepPart] REBUILD
-- ALTER INDEX ALL ON [BMC].[BuildManualStep] REBUILD
-- ALTER INDEX ALL ON [BMC].[BuildManualPage] REBUILD
-- ALTER INDEX ALL ON [BMC].[BuildManualChangeHistory] REBUILD
-- ALTER INDEX ALL ON [BMC].[BuildManual] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserLostPart] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserSetListItem] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserSetListChangeHistory] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserSetList] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserPartListItem] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserPartListChangeHistory] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserPartList] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickSetSetReview] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickSetTransaction] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickSetUserLink] REBUILD
-- ALTER INDEX ALL ON [BMC].[RebrickableSyncQueue] REBUILD
-- ALTER INDEX ALL ON [BMC].[RebrickableTransaction] REBUILD
-- ALTER INDEX ALL ON [BMC].[RebrickableUserLink] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserCollectionSetImport] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserWishlistItem] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserCollectionPart] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserCollectionChangeHistory] REBUILD
-- ALTER INDEX ALL ON [BMC].[UserCollection] REBUILD
-- ALTER INDEX ALL ON [BMC].[LegoSetSubset] REBUILD
-- ALTER INDEX ALL ON [BMC].[LegoSetMinifig] REBUILD
-- ALTER INDEX ALL ON [BMC].[LegoMinifig] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickElement] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickPartRelationship] REBUILD
-- ALTER INDEX ALL ON [BMC].[LegoSetPart] REBUILD
-- ALTER INDEX ALL ON [BMC].[LegoSet] REBUILD
-- ALTER INDEX ALL ON [BMC].[LegoTheme] REBUILD
-- ALTER INDEX ALL ON [BMC].[ModelStepPart] REBUILD
-- ALTER INDEX ALL ON [BMC].[ModelBuildStep] REBUILD
-- ALTER INDEX ALL ON [BMC].[ModelSubFile] REBUILD
-- ALTER INDEX ALL ON [BMC].[ModelDocumentChangeHistory] REBUILD
-- ALTER INDEX ALL ON [BMC].[ModelDocument] REBUILD
-- ALTER INDEX ALL ON [BMC].[ProjectReferenceImage] REBUILD
-- ALTER INDEX ALL ON [BMC].[ProjectCameraPreset] REBUILD
-- ALTER INDEX ALL ON [BMC].[ProjectTagAssignment] REBUILD
-- ALTER INDEX ALL ON [BMC].[ProjectTag] REBUILD
-- ALTER INDEX ALL ON [BMC].[SubmodelInstance] REBUILD
-- ALTER INDEX ALL ON [BMC].[SubmodelPlacedBrick] REBUILD
-- ALTER INDEX ALL ON [BMC].[SubmodelChangeHistory] REBUILD
-- ALTER INDEX ALL ON [BMC].[Submodel] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickConnection] REBUILD
-- ALTER INDEX ALL ON [BMC].[PlacedBrickChangeHistory] REBUILD
-- ALTER INDEX ALL ON [BMC].[PlacedBrick] REBUILD
-- ALTER INDEX ALL ON [BMC].[ProjectChangeHistory] REBUILD
-- ALTER INDEX ALL ON [BMC].[Project] REBUILD
-- ALTER INDEX ALL ON [BMC].[PartSubFileReference] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickPartColour] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickPartConnector] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickPartChangeHistory] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickPart] REBUILD
-- ALTER INDEX ALL ON [BMC].[PartType] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickColour] REBUILD
-- ALTER INDEX ALL ON [BMC].[ColourFinish] REBUILD
-- ALTER INDEX ALL ON [BMC].[ConnectorTypeCompatibility] REBUILD
-- ALTER INDEX ALL ON [BMC].[ConnectorType] REBUILD
-- ALTER INDEX ALL ON [BMC].[BrickCategory] REBUILD

-- Master list of part categories (Beam, Plate, Gear, Axle, Motor, Pneumatic, etc.)
CREATE TABLE [BMC].[BrickCategory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[rebrickablePartCategoryId] INT NULL,		-- Rebrickable part_cat_id for cross-referencing during bulk import
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

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Tile', 'Smooth-top tiles without studs', 3, 'b1c10001-0001-4000-8000-000000000003' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Slope', 'Angled slope bricks and roof pieces', 4, 'b1c10001-0001-4000-8000-000000000004' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Wedge', 'Wedge-shaped plates and bricks', 5, 'b1c10001-0001-4000-8000-000000000005' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Arch', 'Arched bricks and curved elements', 6, 'b1c10001-0001-4000-8000-000000000006' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Cylinder', 'Round bricks, cylinders, and cones', 7, 'b1c10001-0001-4000-8000-000000000007' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Cone', 'Cone-shaped parts', 8, 'b1c10001-0001-4000-8000-000000000008' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Bracket', 'Angle brackets for sideways building', 9, 'b1c10001-0001-4000-8000-000000000009' )
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

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Hinge', 'Hinge bricks, plates, and click hinges', 17, 'b1c10001-0001-4000-8000-000000000017' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Panel', 'Panels, fairings, and body pieces', 20, 'b1c10001-0001-4000-8000-000000000020' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Wheel', 'Wheels, tyres, and rims', 21, 'b1c10001-0001-4000-8000-000000000021' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Window', 'Windows, glass, and frames', 22, 'b1c10001-0001-4000-8000-000000000022' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Door', 'Doors and door frames', 23, 'b1c10001-0001-4000-8000-000000000023' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Fence', 'Fences, railings, and barriers', 24, 'b1c10001-0001-4000-8000-000000000024' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Baseplate', 'Baseplates and road plates', 25, 'b1c10001-0001-4000-8000-000000000025' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Bar', 'Bars, antennas, and clips', 26, 'b1c10001-0001-4000-8000-000000000026' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Support', 'Support structures, columns, and pillars', 27, 'b1c10001-0001-4000-8000-000000000027' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Container', 'Boxes, crates, and storage containers', 28, 'b1c10001-0001-4000-8000-000000000028' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Decorative', 'Decorative, printed, and sticker parts', 30, 'b1c10001-0001-4000-8000-000000000030' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Electric', 'Electrical components, lights, and sensors', 31, 'b1c10001-0001-4000-8000-000000000031' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Propeller', 'Propellers, rotors, and blades', 32, 'b1c10001-0001-4000-8000-000000000032' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Wing', 'Wings and aircraft body parts', 33, 'b1c10001-0001-4000-8000-000000000033' )
GO

INSERT INTO [BMC].[BrickCategory] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Train', 'Train track, wheels, and specialized train parts', 34, 'b1c10001-0001-4000-8000-000000000034' )
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
	[minAngleDegrees] REAL NULL,		-- Minimum angle of rotation for hinge-type connectors (null = no minimum)
	[maxAngleDegrees] REAL NULL,		-- Maximum angle of rotation for hinge-type connectors (null = no maximum)
	[snapIncrementDegrees] REAL NULL,		-- Snap increment for click hinges (e.g. 22.5 degrees, null = continuous)
	[clutchForceNewtons] REAL NULL,		-- Force needed to disconnect this connector type (null = unknown)
	[maleOrFemale] NVARCHAR(10) NULL,		-- Pairing role: Male, Female, or Both (for compatibility matching logic)
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


-- Defines which connector types can physically connect to each other (e.g. Stud↔AntiStud, Pin↔PinHole).
CREATE TABLE [BMC].[ConnectorTypeCompatibility]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[maleConnectorTypeId] INT NOT NULL,		-- The male/inserting connector type
	[femaleConnectorTypeId] INT NOT NULL,		-- The female/receiving connector type
	[connectionStrength] NVARCHAR(50) NOT NULL,		-- Connection strength: Tight, Friction, Free
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ConnectorTypeCompatibility_ConnectorType_maleConnectorTypeId] FOREIGN KEY ([maleConnectorTypeId]) REFERENCES [BMC].[ConnectorType] ([id]),		-- Foreign key to the ConnectorType table.
	CONSTRAINT [FK_ConnectorTypeCompatibility_ConnectorType_femaleConnectorTypeId] FOREIGN KEY ([femaleConnectorTypeId]) REFERENCES [BMC].[ConnectorType] ([id]),		-- Foreign key to the ConnectorType table.
	CONSTRAINT [UC_ConnectorTypeCompatibility_maleConnectorTypeId_femaleConnectorTypeId] UNIQUE ( [maleConnectorTypeId], [femaleConnectorTypeId]) 		-- Uniqueness enforced on the ConnectorTypeCompatibility table's maleConnectorTypeId and femaleConnectorTypeId fields.
)
GO

-- Index on the ConnectorTypeCompatibility table's maleConnectorTypeId field.
CREATE INDEX [I_ConnectorTypeCompatibility_maleConnectorTypeId] ON [BMC].[ConnectorTypeCompatibility] ([maleConnectorTypeId])
GO

-- Index on the ConnectorTypeCompatibility table's femaleConnectorTypeId field.
CREATE INDEX [I_ConnectorTypeCompatibility_femaleConnectorTypeId] ON [BMC].[ConnectorTypeCompatibility] ([femaleConnectorTypeId])
GO

-- Index on the ConnectorTypeCompatibility table's active field.
CREATE INDEX [I_ConnectorTypeCompatibility_active] ON [BMC].[ConnectorTypeCompatibility] ([active])
GO

-- Index on the ConnectorTypeCompatibility table's deleted field.
CREATE INDEX [I_ConnectorTypeCompatibility_deleted] ON [BMC].[ConnectorTypeCompatibility] ([deleted])
GO

INSERT INTO [BMC].[ConnectorTypeCompatibility] ( [connectionStrength], [maleConnectorTypeId], [femaleConnectorTypeId], [objectGuid] ) VALUES  ( 'Tight', ( SELECT TOP 1 id FROM [BMC].[ConnectorType] WHERE [name] = 'Stud' ), ( SELECT TOP 1 id FROM [BMC].[ConnectorType] WHERE [name] = 'AntiStud' ), 'cc100001-0001-4000-8000-000000000001' )
GO

INSERT INTO [BMC].[ConnectorTypeCompatibility] ( [connectionStrength], [maleConnectorTypeId], [femaleConnectorTypeId], [objectGuid] ) VALUES  ( 'Friction', ( SELECT TOP 1 id FROM [BMC].[ConnectorType] WHERE [name] = 'Pin' ), ( SELECT TOP 1 id FROM [BMC].[ConnectorType] WHERE [name] = 'PinHole' ), 'cc100001-0001-4000-8000-000000000002' )
GO

INSERT INTO [BMC].[ConnectorTypeCompatibility] ( [connectionStrength], [maleConnectorTypeId], [femaleConnectorTypeId], [objectGuid] ) VALUES  ( 'Tight', ( SELECT TOP 1 id FROM [BMC].[ConnectorType] WHERE [name] = 'AxleEnd' ), ( SELECT TOP 1 id FROM [BMC].[ConnectorType] WHERE [name] = 'AxleHole' ), 'cc100001-0001-4000-8000-000000000003' )
GO

INSERT INTO [BMC].[ConnectorTypeCompatibility] ( [connectionStrength], [maleConnectorTypeId], [femaleConnectorTypeId], [objectGuid] ) VALUES  ( 'Friction', ( SELECT TOP 1 id FROM [BMC].[ConnectorType] WHERE [name] = 'BallJoint' ), ( SELECT TOP 1 id FROM [BMC].[ConnectorType] WHERE [name] = 'BallJointSocket' ), 'cc100001-0001-4000-8000-000000000004' )
GO


-- Lookup table of material finish types that define how a colour is rendered (e.g. Solid, Chrome, Rubber).
CREATE TABLE [BMC].[ColourFinish]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[requiresEnvironmentMap] BIT NOT NULL DEFAULT 0,		-- Whether this finish needs environment mapping for reflections (Chrome, Metal)
	[isMatte] BIT NOT NULL DEFAULT 0,		-- Whether this finish has a matte/non-glossy appearance (Rubber)
	[defaultAlpha] INT NULL,		-- Default alpha for this finish type, null = use colour-specific alpha
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the ColourFinish table's name field.
CREATE INDEX [I_ColourFinish_name] ON [BMC].[ColourFinish] ([name])
GO

-- Index on the ColourFinish table's active field.
CREATE INDEX [I_ColourFinish_active] ON [BMC].[ColourFinish] ([active])
GO

-- Index on the ColourFinish table's deleted field.
CREATE INDEX [I_ColourFinish_deleted] ON [BMC].[ColourFinish] ([deleted])
GO

INSERT INTO [BMC].[ColourFinish] ( [name], [description], [requiresEnvironmentMap], [isMatte], [sequence], [objectGuid] ) VALUES  ( 'Solid', 'Standard opaque plastic finish', 0, 0, 1, 'cf100001-0001-4000-8000-000000000001' )
GO

INSERT INTO [BMC].[ColourFinish] ( [name], [description], [requiresEnvironmentMap], [isMatte], [defaultAlpha], [sequence], [objectGuid] ) VALUES  ( 'Transparent', 'See-through plastic finish', 0, 0, 128, 2, 'cf100001-0001-4000-8000-000000000002' )
GO

INSERT INTO [BMC].[ColourFinish] ( [name], [description], [requiresEnvironmentMap], [isMatte], [sequence], [objectGuid] ) VALUES  ( 'Chrome', 'Highly reflective chrome-plated metal finish', 1, 0, 3, 'cf100001-0001-4000-8000-000000000003' )
GO

INSERT INTO [BMC].[ColourFinish] ( [name], [description], [requiresEnvironmentMap], [isMatte], [sequence], [objectGuid] ) VALUES  ( 'Pearlescent', 'Iridescent pearl-like plastic finish', 1, 0, 4, 'cf100001-0001-4000-8000-000000000004' )
GO

INSERT INTO [BMC].[ColourFinish] ( [name], [description], [requiresEnvironmentMap], [isMatte], [sequence], [objectGuid] ) VALUES  ( 'Metal', 'Metallic paint or lacquer finish', 1, 0, 5, 'cf100001-0001-4000-8000-000000000005' )
GO

INSERT INTO [BMC].[ColourFinish] ( [name], [description], [requiresEnvironmentMap], [isMatte], [sequence], [objectGuid] ) VALUES  ( 'Rubber', 'Matte rubber or soft-touch finish', 0, 1, 6, 'cf100001-0001-4000-8000-000000000006' )
GO

INSERT INTO [BMC].[ColourFinish] ( [name], [description], [requiresEnvironmentMap], [isMatte], [defaultAlpha], [sequence], [objectGuid] ) VALUES  ( 'Glitter', 'Transparent plastic with embedded glitter particles', 0, 0, 128, 7, 'cf100001-0001-4000-8000-000000000007' )
GO

INSERT INTO [BMC].[ColourFinish] ( [name], [description], [requiresEnvironmentMap], [isMatte], [sequence], [objectGuid] ) VALUES  ( 'Speckle', 'Solid plastic with embedded speckle particles', 0, 0, 8, 'cf100001-0001-4000-8000-000000000008' )
GO

INSERT INTO [BMC].[ColourFinish] ( [name], [description], [requiresEnvironmentMap], [isMatte], [defaultAlpha], [sequence], [objectGuid] ) VALUES  ( 'Milky', 'Semi-translucent milky or glow-in-the-dark finish', 0, 0, 240, 9, 'cf100001-0001-4000-8000-000000000009' )
GO

INSERT INTO [BMC].[ColourFinish] ( [name], [description], [requiresEnvironmentMap], [isMatte], [sequence], [objectGuid] ) VALUES  ( 'Fabric', 'Fabric or cloth material finish for flags, capes, and similar elements', 0, 1, 10, 'cf100001-0001-4000-8000-000000000010' )
GO


-- Colour definitions for brick parts. Compatible with the LDraw colour standard.
CREATE TABLE [BMC].[BrickColour]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[rebrickableColorId] INT NOT NULL,		-- Rebrickable color ID — primary lookup key for API sync
	[ldrawColourCode] INT NULL,		-- LDraw standard colour code number (nullable — some Rebrickable colors may not map to LDraw)
	[bricklinkColorId] INT NULL,		-- BrickLink color ID for cross-referencing
	[brickowlColorId] INT NULL,		-- BrickOwl color ID for cross-referencing
	[hexRgb] NVARCHAR(10) NULL,		-- Hex RGB colour value (e.g. #FF0000)
	[hexEdgeColour] NVARCHAR(10) NULL,		-- LDraw edge/contrast colour hex value for wireframe and outline rendering
	[alpha] INT NULL,		-- Alpha transparency value (0-255, 255 = fully opaque)
	[isTransparent] BIT NOT NULL DEFAULT 0,		-- Whether this colour is transparent (convenience flag derived from alpha)
	[isMetallic] BIT NOT NULL DEFAULT 0,		-- Whether this colour has a metallic finish (convenience flag)
	[colourFinishId] INT NOT NULL,		-- Material finish type — FK to ColourFinish lookup table
	[luminance] INT NULL,		-- Glow brightness (0-255) for glow-in-the-dark colours. Null for non-glowing.
	[legoColourId] INT NULL,		-- Official LEGO colour number for cross-referencing with LEGO catalogues
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_BrickColour_ColourFinish_colourFinishId] FOREIGN KEY ([colourFinishId]) REFERENCES [BMC].[ColourFinish] ([id]),		-- Foreign key to the ColourFinish table.
	CONSTRAINT [UC_BrickColour_rebrickableColorId] UNIQUE ( [rebrickableColorId]) 		-- Uniqueness enforced on the BrickColour table's rebrickableColorId field.
)
GO

-- Index on the BrickColour table's name field.
CREATE INDEX [I_BrickColour_name] ON [BMC].[BrickColour] ([name])
GO

-- Index on the BrickColour table's colourFinishId field.
CREATE INDEX [I_BrickColour_colourFinishId] ON [BMC].[BrickColour] ([colourFinishId])
GO

-- Index on the BrickColour table's active field.
CREATE INDEX [I_BrickColour_active] ON [BMC].[BrickColour] ([active])
GO

-- Index on the BrickColour table's deleted field.
CREATE INDEX [I_BrickColour_deleted] ON [BMC].[BrickColour] ([deleted])
GO


-- Lookup table of LDraw part classification types (Part, Subpart, Primitive, etc.).
CREATE TABLE [BMC].[PartType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[isUserVisible] BIT NOT NULL DEFAULT 1,		-- Whether parts of this type should appear in the user-facing part picker
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the PartType table's name field.
CREATE INDEX [I_PartType_name] ON [BMC].[PartType] ([name])
GO

-- Index on the PartType table's active field.
CREATE INDEX [I_PartType_active] ON [BMC].[PartType] ([active])
GO

-- Index on the PartType table's deleted field.
CREATE INDEX [I_PartType_deleted] ON [BMC].[PartType] ([deleted])
GO

INSERT INTO [BMC].[PartType] ( [name], [description], [isUserVisible], [sequence], [objectGuid] ) VALUES  ( 'Part', 'A complete, standalone part (e.g. Brick 2x4)', 1, 1, 'df6fb298-9f61-41ce-aad2-37c00bc14efd' )
GO

INSERT INTO [BMC].[PartType] ( [name], [description], [isUserVisible], [sequence], [objectGuid] ) VALUES  ( 'Subpart', 'A reusable component used internally by other parts', 0, 2, '71ed658f-8695-44df-9448-669348bcfab4' )
GO

INSERT INTO [BMC].[PartType] ( [name], [description], [isUserVisible], [sequence], [objectGuid] ) VALUES  ( 'Primitive', 'A low-level geometric primitive (cylinder, stud shape)', 0, 3, 'cae03dfa-930b-47e3-acd0-83241eaae69d' )
GO

INSERT INTO [BMC].[PartType] ( [name], [description], [isUserVisible], [sequence], [objectGuid] ) VALUES  ( 'Shortcut', 'A convenience combination of multiple parts (e.g. hinge assembly)', 1, 4, 'a800b3c0-e7d1-46f3-830d-f2c93f7f8e4d' )
GO

INSERT INTO [BMC].[PartType] ( [name], [description], [isUserVisible], [sequence], [objectGuid] ) VALUES  ( 'Alias', 'An alternate ID that maps to another part', 0, 5, '9c5c8f5c-6397-4233-b360-0292adc30304' )
GO


-- Individual brick part definitions. Each row represents a unique part shape (independent of colour).
CREATE TABLE [BMC].[BrickPart]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[rebrickablePartNum] NVARCHAR(100) NOT NULL,		-- Rebrickable part_num — primary lookup key for the parts catalog
	[rebrickablePartUrl] NVARCHAR(250) NULL,		-- URL to part page on Rebrickable
	[rebrickableImgUrl] NVARCHAR(250) NULL,		-- URL to part image on Rebrickable
	[ldrawPartId] NVARCHAR(100) NULL,		-- LDraw part ID (e.g. 3001, 32523) — nullable, some Rebrickable parts have no LDraw file
	[bricklinkId] NVARCHAR(100) NULL,		-- BrickLink part ID for cross-referencing
	[brickowlId] NVARCHAR(100) NULL,		-- BrickOwl part ID for cross-referencing
	[legoDesignId] NVARCHAR(100) NULL,		-- Official LEGO design number for cross-referencing
	[ldrawTitle] NVARCHAR(250) NULL,		-- Raw title from the LDraw .dat file (e.g. 'Brick  2 x  4', 'Technic Beam  3')
	[ldrawCategory] NVARCHAR(100) NULL,		-- Part category from LDraw !CATEGORY meta or inferred from title first word
	[partTypeId] INT NOT NULL,		-- LDraw part classification — FK to PartType lookup table
	[keywords] NVARCHAR(MAX) NULL,		-- Comma-separated keywords from LDraw !KEYWORDS meta lines for search
	[author] NVARCHAR(100) NULL,		-- Part author from the LDraw Author: header line
	[brickCategoryId] INT NOT NULL,		-- The category this part belongs to
	[widthLdu] REAL NULL,		-- Part width in LDraw units (null if not yet computed)
	[heightLdu] REAL NULL,		-- Part height in LDraw units (null if not yet computed)
	[depthLdu] REAL NULL,		-- Part depth in LDraw units (null if not yet computed)
	[massGrams] REAL NULL,		-- Part mass in grams (for physics simulation, null if unknown)
	[momentOfInertiaX] REAL NULL,		-- Rotational inertia about X axis (kg·m², null if unknown)
	[momentOfInertiaY] REAL NULL,		-- Rotational inertia about Y axis (kg·m², null if unknown)
	[momentOfInertiaZ] REAL NULL,		-- Rotational inertia about Z axis (kg·m², null if unknown)
	[frictionCoefficient] REAL NULL,		-- Surface friction coefficient for physics (null if unknown)
	[materialType] NVARCHAR(50) NULL,		-- Material type: ABS, Rubber, Metal, Fabric, etc. (null if unknown)
	[centerOfMassX] REAL NULL,		-- Center of mass X offset from part origin in LDU (null if unknown)
	[centerOfMassY] REAL NULL,		-- Center of mass Y offset from part origin in LDU (null if unknown)
	[centerOfMassZ] REAL NULL,		-- Center of mass Z offset from part origin in LDU (null if unknown)
	[geometryFileName] NVARCHAR(250) NULL,		-- Part of the binary data field setup
	[geometrySize] BIGINT NULL,		-- Part of the binary data field setup
	[geometryData] VARBINARY(MAX) NULL,		-- Part of the binary data field setup
	[geometryMimeType] NVARCHAR(100) NULL,		-- Part of the binary data field setup
	[geometryFileFormat] NVARCHAR(50) NULL,		-- Format of stored geometry: LDraw, ProcessedBinary, etc.
	[geometryOriginalFileName] NVARCHAR(250) NULL,		-- Original LDraw filename for reference (e.g. '3001.dat')
	[boundingBoxMinX] REAL NULL,		-- Axis-aligned bounding box minimum X in LDU
	[boundingBoxMinY] REAL NULL,		-- Axis-aligned bounding box minimum Y in LDU
	[boundingBoxMinZ] REAL NULL,		-- Axis-aligned bounding box minimum Z in LDU
	[boundingBoxMaxX] REAL NULL,		-- Axis-aligned bounding box maximum X in LDU
	[boundingBoxMaxY] REAL NULL,		-- Axis-aligned bounding box maximum Y in LDU
	[boundingBoxMaxZ] REAL NULL,		-- Axis-aligned bounding box maximum Z in LDU
	[subFileCount] INT NULL,		-- Number of LDraw sub-file references in this part (for instancing)
	[polygonCount] INT NULL,		-- Total polygon count for LOD decisions and performance budgets
	[toothCount] INT NULL,		-- For gears: number of teeth. Null for non-gear parts.
	[gearRatio] REAL NULL,		-- For gears: effective gear ratio relative to a base gear. Null for non-gear parts.
	[lastModifiedDate] DATETIME2(7) NULL,		-- Last modification date for incremental sync with Rebrickable
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_BrickPart_PartType_partTypeId] FOREIGN KEY ([partTypeId]) REFERENCES [BMC].[PartType] ([id]),		-- Foreign key to the PartType table.
	CONSTRAINT [FK_BrickPart_BrickCategory_brickCategoryId] FOREIGN KEY ([brickCategoryId]) REFERENCES [BMC].[BrickCategory] ([id]),		-- Foreign key to the BrickCategory table.
	CONSTRAINT [UC_BrickPart_rebrickablePartNum] UNIQUE ( [rebrickablePartNum]) 		-- Uniqueness enforced on the BrickPart table's rebrickablePartNum field.
)
GO

-- Index on the BrickPart table's name field.
CREATE INDEX [I_BrickPart_name] ON [BMC].[BrickPart] ([name])
GO

-- Index on the BrickPart table's partTypeId field.
CREATE INDEX [I_BrickPart_partTypeId] ON [BMC].[BrickPart] ([partTypeId])
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
	[brickPartId] INT NOT NULL,		-- The part this connector belongs to
	[connectorTypeId] INT NOT NULL,		-- The type of connector (Stud, PinHole, AxleHole, etc.)
	[positionX] REAL NULL,		-- X position of connector relative to part origin (LDU)
	[positionY] REAL NULL,		-- Y position of connector relative to part origin (LDU)
	[positionZ] REAL NULL,		-- Z position of connector relative to part origin (LDU)
	[orientationX] REAL NULL,		-- X component of connector direction unit vector
	[orientationY] REAL NULL,		-- Y component of connector direction unit vector
	[orientationZ] REAL NULL,		-- Z component of connector direction unit vector
	[connectorGroupId] INT NULL,		-- Groups connectors that act together (e.g. all studs on top of a 2x4 share a group ID)
	[isAutoExtracted] BIT NOT NULL DEFAULT 0,		-- Whether this connector position was auto-extracted from LDraw geometry analysis
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


-- Models how LDraw parts reference sub-files hierarchically. Crucial for instanced rendering and understanding part decomposition.
CREATE TABLE [BMC].[PartSubFileReference]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[parentBrickPartId] INT NOT NULL,		-- The part containing the sub-file reference
	[referencedBrickPartId] INT NULL,		-- The referenced sub-file as a BrickPart (null if sub-file is not cataloged)
	[referencedFileName] NVARCHAR(250) NOT NULL,		-- Original LDraw sub-file filename (e.g. 'stud.dat', 's/3001s01.dat')
	[transformMatrix] NVARCHAR(500) NOT NULL,		-- 4x3 transform matrix as space-delimited floats (x y z a b c d e f g h i)
	[colorCode] INT NULL,		-- LDraw color code override (16=inherit parent, null=inherit parent)
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_PartSubFileReference_BrickPart_parentBrickPartId] FOREIGN KEY ([parentBrickPartId]) REFERENCES [BMC].[BrickPart] ([id]),		-- Foreign key to the BrickPart table.
	CONSTRAINT [FK_PartSubFileReference_BrickPart_referencedBrickPartId] FOREIGN KEY ([referencedBrickPartId]) REFERENCES [BMC].[BrickPart] ([id])		-- Foreign key to the BrickPart table.
)
GO

-- Index on the PartSubFileReference table's parentBrickPartId field.
CREATE INDEX [I_PartSubFileReference_parentBrickPartId] ON [BMC].[PartSubFileReference] ([parentBrickPartId])
GO

-- Index on the PartSubFileReference table's referencedBrickPartId field.
CREATE INDEX [I_PartSubFileReference_referencedBrickPartId] ON [BMC].[PartSubFileReference] ([referencedBrickPartId])
GO

-- Index on the PartSubFileReference table's active field.
CREATE INDEX [I_PartSubFileReference_active] ON [BMC].[PartSubFileReference] ([active])
GO

-- Index on the PartSubFileReference table's deleted field.
CREATE INDEX [I_PartSubFileReference_deleted] ON [BMC].[PartSubFileReference] ([deleted])
GO


-- A user's building project. Contains placed bricks and their connections to form a model.
CREATE TABLE [BMC].[Project]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userId] INT NULL,		-- Cross-database reference to SecurityUser.id — the user who owns this project (nullable for legacy data)
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[notes] NVARCHAR(MAX) NULL,		-- Free-form notes about the project
	[thumbnailImagePath] NVARCHAR(250) NULL,		-- Relative path to project thumbnail image for listings
	[thumbnailData] VARBINARY(MAX) NULL,		-- PNG thumbnail image data from .io import or renders
	[partCount] INT NULL,		-- Cached total part count for quick display without querying PlacedBrick
	[lastBuildDate] DATETIME2(7) NULL,		-- When the user last modified the build (placed or moved a brick)
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
	[projectId] INT NOT NULL,		-- The project this brick is placed in
	[brickPartId] INT NOT NULL,		-- The part definition being placed
	[brickColourId] INT NOT NULL,		-- The colour of this placed brick instance
	[positionX] REAL NULL,		-- X position in world coordinates (LDU)
	[positionY] REAL NULL,		-- Y position in world coordinates (LDU)
	[positionZ] REAL NULL,		-- Z position in world coordinates (LDU)
	[rotationX] REAL NULL,		-- Quaternion X component
	[rotationY] REAL NULL,		-- Quaternion Y component
	[rotationZ] REAL NULL,		-- Quaternion Z component
	[rotationW] REAL NULL,		-- Quaternion W component
	[buildStepNumber] INT NULL,		-- Optional build step number for instruction ordering
	[isHidden] BIT NOT NULL DEFAULT 0,		-- Whether this brick is temporarily hidden in the editor
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
	[projectId] INT NOT NULL,		-- The project this connection belongs to
	[sourcePlacedBrickId] INT NOT NULL,		-- FK to the source PlacedBrick
	[sourceConnectorId] INT NOT NULL,		-- FK to the BrickPartConnector on the source brick
	[targetPlacedBrickId] INT NOT NULL,		-- FK to the target PlacedBrick
	[targetConnectorId] INT NOT NULL,		-- FK to the BrickPartConnector on the target brick
	[connectionStrength] NVARCHAR(50) NULL,		-- Connection strength: Snapped, Friction, Free (null = unknown)
	[isLocked] BIT NOT NULL DEFAULT 0,		-- Whether the user has locked this connection from being broken in the editor
	[angleDegrees] REAL NULL,		-- Current angle for rotational connections (null = default/fixed)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_BrickConnection_Project_projectId] FOREIGN KEY ([projectId]) REFERENCES [BMC].[Project] ([id]),		-- Foreign key to the Project table.
	CONSTRAINT [FK_BrickConnection_PlacedBrick_sourcePlacedBrickId] FOREIGN KEY ([sourcePlacedBrickId]) REFERENCES [BMC].[PlacedBrick] ([id]),		-- Foreign key to the PlacedBrick table.
	CONSTRAINT [FK_BrickConnection_BrickPartConnector_sourceConnectorId] FOREIGN KEY ([sourceConnectorId]) REFERENCES [BMC].[BrickPartConnector] ([id]),		-- Foreign key to the BrickPartConnector table.
	CONSTRAINT [FK_BrickConnection_PlacedBrick_targetPlacedBrickId] FOREIGN KEY ([targetPlacedBrickId]) REFERENCES [BMC].[PlacedBrick] ([id]),		-- Foreign key to the PlacedBrick table.
	CONSTRAINT [FK_BrickConnection_BrickPartConnector_targetConnectorId] FOREIGN KEY ([targetConnectorId]) REFERENCES [BMC].[BrickPartConnector] ([id])		-- Foreign key to the BrickPartConnector table.
)
GO

-- Index on the BrickConnection table's tenantGuid field.
CREATE INDEX [I_BrickConnection_tenantGuid] ON [BMC].[BrickConnection] ([tenantGuid])
GO

-- Index on the BrickConnection table's tenantGuid,projectId fields.
CREATE INDEX [I_BrickConnection_tenantGuid_projectId] ON [BMC].[BrickConnection] ([tenantGuid], [projectId])
GO

-- Index on the BrickConnection table's tenantGuid,sourcePlacedBrickId fields.
CREATE INDEX [I_BrickConnection_tenantGuid_sourcePlacedBrickId] ON [BMC].[BrickConnection] ([tenantGuid], [sourcePlacedBrickId])
GO

-- Index on the BrickConnection table's tenantGuid,sourceConnectorId fields.
CREATE INDEX [I_BrickConnection_tenantGuid_sourceConnectorId] ON [BMC].[BrickConnection] ([tenantGuid], [sourceConnectorId])
GO

-- Index on the BrickConnection table's tenantGuid,targetPlacedBrickId fields.
CREATE INDEX [I_BrickConnection_tenantGuid_targetPlacedBrickId] ON [BMC].[BrickConnection] ([tenantGuid], [targetPlacedBrickId])
GO

-- Index on the BrickConnection table's tenantGuid,targetConnectorId fields.
CREATE INDEX [I_BrickConnection_tenantGuid_targetConnectorId] ON [BMC].[BrickConnection] ([tenantGuid], [targetConnectorId])
GO

-- Index on the BrickConnection table's tenantGuid,active fields.
CREATE INDEX [I_BrickConnection_tenantGuid_active] ON [BMC].[BrickConnection] ([tenantGuid], [active])
GO

-- Index on the BrickConnection table's tenantGuid,deleted fields.
CREATE INDEX [I_BrickConnection_tenantGuid_deleted] ON [BMC].[BrickConnection] ([tenantGuid], [deleted])
GO


-- A named sub-assembly within a project, similar to LDraw subfiles. Allows hierarchical builds.
CREATE TABLE [BMC].[Submodel]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[projectId] INT NOT NULL,		-- The project this submodel belongs to
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[submodelId] INT NULL,		-- Optional parent submodel for nested sub-assemblies (self-referencing FK, null = top-level)
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Submodel_Project_projectId] FOREIGN KEY ([projectId]) REFERENCES [BMC].[Project] ([id]),		-- Foreign key to the Project table.
	CONSTRAINT [FK_Submodel_Submodel_submodelId] FOREIGN KEY ([submodelId]) REFERENCES [BMC].[Submodel] ([id]),		-- Foreign key to the Submodel table.
	CONSTRAINT [UC_Submodel_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Submodel table's tenantGuid and name fields.
)
GO

-- Index on the Submodel table's tenantGuid field.
CREATE INDEX [I_Submodel_tenantGuid] ON [BMC].[Submodel] ([tenantGuid])
GO

-- Index on the Submodel table's tenantGuid,projectId fields.
CREATE INDEX [I_Submodel_tenantGuid_projectId] ON [BMC].[Submodel] ([tenantGuid], [projectId])
GO

-- Index on the Submodel table's tenantGuid,name fields.
CREATE INDEX [I_Submodel_tenantGuid_name] ON [BMC].[Submodel] ([tenantGuid], [name])
GO

-- Index on the Submodel table's tenantGuid,submodelId fields.
CREATE INDEX [I_Submodel_tenantGuid_submodelId] ON [BMC].[Submodel] ([tenantGuid], [submodelId])
GO

-- Index on the Submodel table's tenantGuid,active fields.
CREATE INDEX [I_Submodel_tenantGuid_active] ON [BMC].[Submodel] ([tenantGuid], [active])
GO

-- Index on the Submodel table's tenantGuid,deleted fields.
CREATE INDEX [I_Submodel_tenantGuid_deleted] ON [BMC].[Submodel] ([tenantGuid], [deleted])
GO


-- The change history for records from the Submodel table.
CREATE TABLE [BMC].[SubmodelChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[submodelId] INT NOT NULL,		-- Link to the Submodel table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_SubmodelChangeHistory_Submodel_submodelId] FOREIGN KEY ([submodelId]) REFERENCES [BMC].[Submodel] ([id])		-- Foreign key to the Submodel table.
)
GO

-- Index on the SubmodelChangeHistory table's tenantGuid field.
CREATE INDEX [I_SubmodelChangeHistory_tenantGuid] ON [BMC].[SubmodelChangeHistory] ([tenantGuid])
GO

-- Index on the SubmodelChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_SubmodelChangeHistory_tenantGuid_versionNumber] ON [BMC].[SubmodelChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the SubmodelChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_SubmodelChangeHistory_tenantGuid_timeStamp] ON [BMC].[SubmodelChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the SubmodelChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_SubmodelChangeHistory_tenantGuid_userId] ON [BMC].[SubmodelChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the SubmodelChangeHistory table's tenantGuid,submodelId fields.
CREATE INDEX [I_SubmodelChangeHistory_tenantGuid_submodelId] ON [BMC].[SubmodelChangeHistory] ([tenantGuid], [submodelId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Maps placed bricks to the submodel they belong to. A placed brick can only belong to one submodel.
CREATE TABLE [BMC].[SubmodelPlacedBrick]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[submodelId] INT NOT NULL,		-- The submodel this brick belongs to
	[placedBrickId] INT NOT NULL,		-- The placed brick assigned to this submodel
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SubmodelPlacedBrick_Submodel_submodelId] FOREIGN KEY ([submodelId]) REFERENCES [BMC].[Submodel] ([id]),		-- Foreign key to the Submodel table.
	CONSTRAINT [FK_SubmodelPlacedBrick_PlacedBrick_placedBrickId] FOREIGN KEY ([placedBrickId]) REFERENCES [BMC].[PlacedBrick] ([id]),		-- Foreign key to the PlacedBrick table.
	CONSTRAINT [UC_SubmodelPlacedBrick_tenantGuid_placedBrickId] UNIQUE ( [tenantGuid], [placedBrickId]) 		-- Uniqueness enforced on the SubmodelPlacedBrick table's tenantGuid and placedBrickId fields.
)
GO

-- Index on the SubmodelPlacedBrick table's tenantGuid field.
CREATE INDEX [I_SubmodelPlacedBrick_tenantGuid] ON [BMC].[SubmodelPlacedBrick] ([tenantGuid])
GO

-- Index on the SubmodelPlacedBrick table's tenantGuid,submodelId fields.
CREATE INDEX [I_SubmodelPlacedBrick_tenantGuid_submodelId] ON [BMC].[SubmodelPlacedBrick] ([tenantGuid], [submodelId])
GO

-- Index on the SubmodelPlacedBrick table's tenantGuid,placedBrickId fields.
CREATE INDEX [I_SubmodelPlacedBrick_tenantGuid_placedBrickId] ON [BMC].[SubmodelPlacedBrick] ([tenantGuid], [placedBrickId])
GO

-- Index on the SubmodelPlacedBrick table's tenantGuid,active fields.
CREATE INDEX [I_SubmodelPlacedBrick_tenantGuid_active] ON [BMC].[SubmodelPlacedBrick] ([tenantGuid], [active])
GO

-- Index on the SubmodelPlacedBrick table's tenantGuid,deleted fields.
CREATE INDEX [I_SubmodelPlacedBrick_tenantGuid_deleted] ON [BMC].[SubmodelPlacedBrick] ([tenantGuid], [deleted])
GO


-- Tracks where a submodel assembly is placed in the parent model. Stores the LDraw type-1 reference line transform data so the model can be faithfully reconstructed. A single Submodel can have multiple instances (e.g. left/right wheel assemblies at different positions).
CREATE TABLE [BMC].[SubmodelInstance]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[submodelId] INT NOT NULL,		-- The submodel definition being placed
	[parentSubmodelId] INT NULL,		-- The parent submodel this instance is placed WITHIN (null = placed directly in the main model)
	[positionX] REAL NULL,		-- X position from the LDraw type 1 reference line (LDU)
	[positionY] REAL NULL,		-- Y position from the LDraw type 1 reference line (LDU)
	[positionZ] REAL NULL,		-- Z position from the LDraw type 1 reference line (LDU)
	[rotationX] REAL NULL,		-- Quaternion X component (from 3x3 rotation matrix)
	[rotationY] REAL NULL,		-- Quaternion Y component
	[rotationZ] REAL NULL,		-- Quaternion Z component
	[rotationW] REAL NULL,		-- Quaternion W component (1 = no rotation)
	[colourCode] INT NOT NULL,		-- LDraw colour code from the type-1 reference line (usually 16 = inherit)
	[buildStepNumber] INT NOT NULL,		-- Build step number in the parent model where this instance is placed
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SubmodelInstance_Submodel_submodelId] FOREIGN KEY ([submodelId]) REFERENCES [BMC].[Submodel] ([id]),		-- Foreign key to the Submodel table.
	CONSTRAINT [FK_SubmodelInstance_Submodel_parentSubmodelId] FOREIGN KEY ([parentSubmodelId]) REFERENCES [BMC].[Submodel] ([id])		-- Foreign key to the Submodel table.
)
GO

-- Index on the SubmodelInstance table's tenantGuid field.
CREATE INDEX [I_SubmodelInstance_tenantGuid] ON [BMC].[SubmodelInstance] ([tenantGuid])
GO

-- Index on the SubmodelInstance table's tenantGuid,submodelId fields.
CREATE INDEX [I_SubmodelInstance_tenantGuid_submodelId] ON [BMC].[SubmodelInstance] ([tenantGuid], [submodelId])
GO

-- Index on the SubmodelInstance table's tenantGuid,parentSubmodelId fields.
CREATE INDEX [I_SubmodelInstance_tenantGuid_parentSubmodelId] ON [BMC].[SubmodelInstance] ([tenantGuid], [parentSubmodelId])
GO

-- Index on the SubmodelInstance table's tenantGuid,active fields.
CREATE INDEX [I_SubmodelInstance_tenantGuid_active] ON [BMC].[SubmodelInstance] ([tenantGuid], [active])
GO

-- Index on the SubmodelInstance table's tenantGuid,deleted fields.
CREATE INDEX [I_SubmodelInstance_tenantGuid_deleted] ON [BMC].[SubmodelInstance] ([tenantGuid], [deleted])
GO


-- User-defined tags for categorizing and filtering projects (e.g. Technic, MOC, WIP).
CREATE TABLE [BMC].[ProjectTag]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_ProjectTag_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the ProjectTag table's tenantGuid and name fields.
)
GO

-- Index on the ProjectTag table's tenantGuid field.
CREATE INDEX [I_ProjectTag_tenantGuid] ON [BMC].[ProjectTag] ([tenantGuid])
GO

-- Index on the ProjectTag table's tenantGuid,name fields.
CREATE INDEX [I_ProjectTag_tenantGuid_name] ON [BMC].[ProjectTag] ([tenantGuid], [name])
GO

-- Index on the ProjectTag table's tenantGuid,active fields.
CREATE INDEX [I_ProjectTag_tenantGuid_active] ON [BMC].[ProjectTag] ([tenantGuid], [active])
GO

-- Index on the ProjectTag table's tenantGuid,deleted fields.
CREATE INDEX [I_ProjectTag_tenantGuid_deleted] ON [BMC].[ProjectTag] ([tenantGuid], [deleted])
GO


-- Many-to-many mapping between projects and tags.
CREATE TABLE [BMC].[ProjectTagAssignment]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[projectId] INT NOT NULL,		-- The project being tagged
	[projectTagId] INT NOT NULL,		-- The tag applied to the project
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ProjectTagAssignment_Project_projectId] FOREIGN KEY ([projectId]) REFERENCES [BMC].[Project] ([id]),		-- Foreign key to the Project table.
	CONSTRAINT [FK_ProjectTagAssignment_ProjectTag_projectTagId] FOREIGN KEY ([projectTagId]) REFERENCES [BMC].[ProjectTag] ([id]),		-- Foreign key to the ProjectTag table.
	CONSTRAINT [UC_ProjectTagAssignment_tenantGuid_projectId_projectTagId] UNIQUE ( [tenantGuid], [projectId], [projectTagId]) 		-- Uniqueness enforced on the ProjectTagAssignment table's tenantGuid and projectId and projectTagId fields.
)
GO

-- Index on the ProjectTagAssignment table's tenantGuid field.
CREATE INDEX [I_ProjectTagAssignment_tenantGuid] ON [BMC].[ProjectTagAssignment] ([tenantGuid])
GO

-- Index on the ProjectTagAssignment table's tenantGuid,projectId fields.
CREATE INDEX [I_ProjectTagAssignment_tenantGuid_projectId] ON [BMC].[ProjectTagAssignment] ([tenantGuid], [projectId])
GO

-- Index on the ProjectTagAssignment table's tenantGuid,projectTagId fields.
CREATE INDEX [I_ProjectTagAssignment_tenantGuid_projectTagId] ON [BMC].[ProjectTagAssignment] ([tenantGuid], [projectTagId])
GO

-- Index on the ProjectTagAssignment table's tenantGuid,active fields.
CREATE INDEX [I_ProjectTagAssignment_tenantGuid_active] ON [BMC].[ProjectTagAssignment] ([tenantGuid], [active])
GO

-- Index on the ProjectTagAssignment table's tenantGuid,deleted fields.
CREATE INDEX [I_ProjectTagAssignment_tenantGuid_deleted] ON [BMC].[ProjectTagAssignment] ([tenantGuid], [deleted])
GO


-- Saved camera positions and orientations for quick viewport recall in the 3D editor.
CREATE TABLE [BMC].[ProjectCameraPreset]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[projectId] INT NOT NULL,		-- The project this camera preset belongs to
	[name] NVARCHAR(100) NOT NULL,
	[positionX] REAL NULL,		-- Camera X position in world coordinates (LDU)
	[positionY] REAL NULL,		-- Camera Y position in world coordinates (LDU)
	[positionZ] REAL NULL,		-- Camera Z position in world coordinates (LDU)
	[targetX] REAL NULL,		-- Camera target X position (look-at point)
	[targetY] REAL NULL,		-- Camera target Y position (look-at point)
	[targetZ] REAL NULL,		-- Camera target Z position (look-at point)
	[zoom] REAL NULL,		-- Camera zoom level / field of view
	[isPerspective] BIT NOT NULL DEFAULT 1,		-- True for perspective projection, false for orthographic
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ProjectCameraPreset_Project_projectId] FOREIGN KEY ([projectId]) REFERENCES [BMC].[Project] ([id]),		-- Foreign key to the Project table.
	CONSTRAINT [UC_ProjectCameraPreset_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the ProjectCameraPreset table's tenantGuid and name fields.
)
GO

-- Index on the ProjectCameraPreset table's tenantGuid field.
CREATE INDEX [I_ProjectCameraPreset_tenantGuid] ON [BMC].[ProjectCameraPreset] ([tenantGuid])
GO

-- Index on the ProjectCameraPreset table's tenantGuid,projectId fields.
CREATE INDEX [I_ProjectCameraPreset_tenantGuid_projectId] ON [BMC].[ProjectCameraPreset] ([tenantGuid], [projectId])
GO

-- Index on the ProjectCameraPreset table's tenantGuid,name fields.
CREATE INDEX [I_ProjectCameraPreset_tenantGuid_name] ON [BMC].[ProjectCameraPreset] ([tenantGuid], [name])
GO

-- Index on the ProjectCameraPreset table's tenantGuid,active fields.
CREATE INDEX [I_ProjectCameraPreset_tenantGuid_active] ON [BMC].[ProjectCameraPreset] ([tenantGuid], [active])
GO

-- Index on the ProjectCameraPreset table's tenantGuid,deleted fields.
CREATE INDEX [I_ProjectCameraPreset_tenantGuid_deleted] ON [BMC].[ProjectCameraPreset] ([tenantGuid], [deleted])
GO


-- Reference images overlaid in the 3D editor for proportioning and tracing.
CREATE TABLE [BMC].[ProjectReferenceImage]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[projectId] INT NOT NULL,		-- The project this reference image belongs to
	[name] NVARCHAR(100) NOT NULL,
	[imageFilePath] NVARCHAR(250) NULL,		-- Relative path to the uploaded reference image file
	[opacity] REAL NULL,		-- Opacity of the overlay (0.0 = invisible, 1.0 = fully opaque)
	[positionX] REAL NULL,		-- X position of the image plane in world coordinates (LDU)
	[positionY] REAL NULL,		-- Y position of the image plane in world coordinates (LDU)
	[positionZ] REAL NULL,		-- Z position of the image plane in world coordinates (LDU)
	[scaleX] REAL NULL,		-- Horizontal scale factor for the reference image
	[scaleY] REAL NULL,		-- Vertical scale factor for the reference image
	[isVisible] BIT NOT NULL DEFAULT 1,		-- Whether the reference image is currently visible in the editor
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ProjectReferenceImage_Project_projectId] FOREIGN KEY ([projectId]) REFERENCES [BMC].[Project] ([id]),		-- Foreign key to the Project table.
	CONSTRAINT [UC_ProjectReferenceImage_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the ProjectReferenceImage table's tenantGuid and name fields.
)
GO

-- Index on the ProjectReferenceImage table's tenantGuid field.
CREATE INDEX [I_ProjectReferenceImage_tenantGuid] ON [BMC].[ProjectReferenceImage] ([tenantGuid])
GO

-- Index on the ProjectReferenceImage table's tenantGuid,projectId fields.
CREATE INDEX [I_ProjectReferenceImage_tenantGuid_projectId] ON [BMC].[ProjectReferenceImage] ([tenantGuid], [projectId])
GO

-- Index on the ProjectReferenceImage table's tenantGuid,name fields.
CREATE INDEX [I_ProjectReferenceImage_tenantGuid_name] ON [BMC].[ProjectReferenceImage] ([tenantGuid], [name])
GO

-- Index on the ProjectReferenceImage table's tenantGuid,active fields.
CREATE INDEX [I_ProjectReferenceImage_tenantGuid_active] ON [BMC].[ProjectReferenceImage] ([tenantGuid], [active])
GO

-- Index on the ProjectReferenceImage table's tenantGuid,deleted fields.
CREATE INDEX [I_ProjectReferenceImage_tenantGuid_deleted] ON [BMC].[ProjectReferenceImage] ([tenantGuid], [deleted])
GO


-- Persistent representation of an imported or authored multi-part model document (MPD/LDR). Top-level container for complex models with build steps.
CREATE TABLE [BMC].[ModelDocument]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[projectId] INT NULL,		-- Optional link to a BMC project if authored in BMC (null for imported documents)
	[name] NVARCHAR(250) NOT NULL,		-- Document name or title
	[description] NVARCHAR(MAX) NULL,		-- Description of the model document
	[sourceFormat] NVARCHAR(50) NOT NULL,		-- Source file format: MPD, LDR, StudioIO, BMCNative
	[sourceFileName] NVARCHAR(250) NULL,		-- Original filename if imported (e.g. 'crane_42131.mpd')
	[sourceFileFileName] NVARCHAR(250) NULL,		-- Part of the binary data field setup
	[sourceFileSize] BIGINT NULL,		-- Part of the binary data field setup
	[sourceFileData] VARBINARY(MAX) NULL,		-- Part of the binary data field setup
	[sourceFileMimeType] NVARCHAR(100) NULL,		-- Part of the binary data field setup
	[author] NVARCHAR(100) NULL,		-- Model author from the file header
	[totalPartCount] INT NULL,		-- Cached total part count across all sub-files
	[totalStepCount] INT NULL,		-- Cached total build step count across all sub-files
	[studioVersion] NVARCHAR(100) NULL,		-- BrickLink Studio version that created this document (from .io .INFO file)
	[instructionSettingsXml] NVARCHAR(MAX) NULL,		-- Raw XML instruction configuration from .io model.ins file
	[errorPartList] NVARCHAR(MAX) NULL,		-- Content of errorPartList.err from .io archive listing problematic or missing parts
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ModelDocument_Project_projectId] FOREIGN KEY ([projectId]) REFERENCES [BMC].[Project] ([id])		-- Foreign key to the Project table.
)
GO

-- Index on the ModelDocument table's tenantGuid field.
CREATE INDEX [I_ModelDocument_tenantGuid] ON [BMC].[ModelDocument] ([tenantGuid])
GO

-- Index on the ModelDocument table's tenantGuid,projectId fields.
CREATE INDEX [I_ModelDocument_tenantGuid_projectId] ON [BMC].[ModelDocument] ([tenantGuid], [projectId])
GO

-- Index on the ModelDocument table's tenantGuid,name fields.
CREATE INDEX [I_ModelDocument_tenantGuid_name] ON [BMC].[ModelDocument] ([tenantGuid], [name])
GO

-- Index on the ModelDocument table's tenantGuid,active fields.
CREATE INDEX [I_ModelDocument_tenantGuid_active] ON [BMC].[ModelDocument] ([tenantGuid], [active])
GO

-- Index on the ModelDocument table's tenantGuid,deleted fields.
CREATE INDEX [I_ModelDocument_tenantGuid_deleted] ON [BMC].[ModelDocument] ([tenantGuid], [deleted])
GO


-- The change history for records from the ModelDocument table.
CREATE TABLE [BMC].[ModelDocumentChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[modelDocumentId] INT NOT NULL,		-- Link to the ModelDocument table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_ModelDocumentChangeHistory_ModelDocument_modelDocumentId] FOREIGN KEY ([modelDocumentId]) REFERENCES [BMC].[ModelDocument] ([id])		-- Foreign key to the ModelDocument table.
)
GO

-- Index on the ModelDocumentChangeHistory table's tenantGuid field.
CREATE INDEX [I_ModelDocumentChangeHistory_tenantGuid] ON [BMC].[ModelDocumentChangeHistory] ([tenantGuid])
GO

-- Index on the ModelDocumentChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_ModelDocumentChangeHistory_tenantGuid_versionNumber] ON [BMC].[ModelDocumentChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the ModelDocumentChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_ModelDocumentChangeHistory_tenantGuid_timeStamp] ON [BMC].[ModelDocumentChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the ModelDocumentChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_ModelDocumentChangeHistory_tenantGuid_userId] ON [BMC].[ModelDocumentChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the ModelDocumentChangeHistory table's tenantGuid,modelDocumentId fields.
CREATE INDEX [I_ModelDocumentChangeHistory_tenantGuid_modelDocumentId] ON [BMC].[ModelDocumentChangeHistory] ([tenantGuid], [modelDocumentId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Individual sub-files within a model document (MPD). Each represents a sub-assembly or the main model. Maps to LDraw '0 FILE' blocks.
CREATE TABLE [BMC].[ModelSubFile]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[modelDocumentId] INT NOT NULL,		-- The document this sub-file belongs to
	[fileName] NVARCHAR(250) NOT NULL,		-- Sub-file name as declared in '0 FILE' (e.g. 'main.ldr', 'wheel_assembly.ldr')
	[isMainModel] BIT NOT NULL DEFAULT 0,		-- Whether this is the main (first) model in the MPD — only rendered sub-files are those referenced by this
	[parentModelSubFileId] INT NULL,		-- Optional parent sub-file for nested sub-assemblies (null = top-level)
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ModelSubFile_ModelDocument_modelDocumentId] FOREIGN KEY ([modelDocumentId]) REFERENCES [BMC].[ModelDocument] ([id]),		-- Foreign key to the ModelDocument table.
	CONSTRAINT [FK_ModelSubFile_ModelSubFile_parentModelSubFileId] FOREIGN KEY ([parentModelSubFileId]) REFERENCES [BMC].[ModelSubFile] ([id]),		-- Foreign key to the ModelSubFile table.
	CONSTRAINT [UC_ModelSubFile_tenantGuid_modelDocumentId_fileName] UNIQUE ( [tenantGuid], [modelDocumentId], [fileName]) 		-- Uniqueness enforced on the ModelSubFile table's tenantGuid and modelDocumentId and fileName fields.
)
GO

-- Index on the ModelSubFile table's tenantGuid field.
CREATE INDEX [I_ModelSubFile_tenantGuid] ON [BMC].[ModelSubFile] ([tenantGuid])
GO

-- Index on the ModelSubFile table's tenantGuid,modelDocumentId fields.
CREATE INDEX [I_ModelSubFile_tenantGuid_modelDocumentId] ON [BMC].[ModelSubFile] ([tenantGuid], [modelDocumentId])
GO

-- Index on the ModelSubFile table's tenantGuid,parentModelSubFileId fields.
CREATE INDEX [I_ModelSubFile_tenantGuid_parentModelSubFileId] ON [BMC].[ModelSubFile] ([tenantGuid], [parentModelSubFileId])
GO

-- Index on the ModelSubFile table's tenantGuid,active fields.
CREATE INDEX [I_ModelSubFile_tenantGuid_active] ON [BMC].[ModelSubFile] ([tenantGuid], [active])
GO

-- Index on the ModelSubFile table's tenantGuid,deleted fields.
CREATE INDEX [I_ModelSubFile_tenantGuid_deleted] ON [BMC].[ModelSubFile] ([tenantGuid], [deleted])
GO


-- Individual build steps within a model sub-file. Modeled from LDraw '0 STEP' and '0 ROTSTEP' meta-commands.
CREATE TABLE [BMC].[ModelBuildStep]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[modelSubFileId] INT NOT NULL,		-- The sub-file this step belongs to
	[stepNumber] INT NOT NULL,		-- Sequential step number within this sub-file
	[rotationType] NVARCHAR(10) NULL,		-- ROTSTEP rotation type: REL (relative), ABS (absolute), ADD (additive). Null = no rotation.
	[rotationX] REAL NULL,		-- ROTSTEP X rotation angle in degrees
	[rotationY] REAL NULL,		-- ROTSTEP Y rotation angle in degrees
	[rotationZ] REAL NULL,		-- ROTSTEP Z rotation angle in degrees
	[description] NVARCHAR(MAX) NULL,		-- Optional step description or annotation
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ModelBuildStep_ModelSubFile_modelSubFileId] FOREIGN KEY ([modelSubFileId]) REFERENCES [BMC].[ModelSubFile] ([id])		-- Foreign key to the ModelSubFile table.
)
GO

-- Index on the ModelBuildStep table's tenantGuid field.
CREATE INDEX [I_ModelBuildStep_tenantGuid] ON [BMC].[ModelBuildStep] ([tenantGuid])
GO

-- Index on the ModelBuildStep table's tenantGuid,modelSubFileId fields.
CREATE INDEX [I_ModelBuildStep_tenantGuid_modelSubFileId] ON [BMC].[ModelBuildStep] ([tenantGuid], [modelSubFileId])
GO

-- Index on the ModelBuildStep table's tenantGuid,active fields.
CREATE INDEX [I_ModelBuildStep_tenantGuid_active] ON [BMC].[ModelBuildStep] ([tenantGuid], [active])
GO

-- Index on the ModelBuildStep table's tenantGuid,deleted fields.
CREATE INDEX [I_ModelBuildStep_tenantGuid_deleted] ON [BMC].[ModelBuildStep] ([tenantGuid], [deleted])
GO


-- Parts placed during a specific build step. Represents LDraw type 1 sub-file reference lines between STEP commands.
CREATE TABLE [BMC].[ModelStepPart]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[modelBuildStepId] INT NOT NULL,		-- The build step this part placement belongs to
	[brickPartId] INT NULL,		-- FK to BrickPart if this part is in the catalog (null if not yet cataloged)
	[brickColourId] INT NULL,		-- FK to BrickColour if the color is mapped (null if unmapped)
	[partFileName] NVARCHAR(250) NOT NULL,		-- Original LDraw part filename from the type 1 line (e.g. '3001.dat')
	[colorCode] INT NOT NULL,		-- LDraw color code used in the file
	[positionX] REAL NULL,		-- X position from the LDraw type 1 reference line (LDU)
	[positionY] REAL NULL,		-- Y position from the LDraw type 1 reference line (LDU)
	[positionZ] REAL NULL,		-- Z position from the LDraw type 1 reference line (LDU)
	[transformMatrix] NVARCHAR(500) NOT NULL,		-- 3x3 rotation matrix as space-delimited floats (a b c d e f g h i)
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ModelStepPart_ModelBuildStep_modelBuildStepId] FOREIGN KEY ([modelBuildStepId]) REFERENCES [BMC].[ModelBuildStep] ([id]),		-- Foreign key to the ModelBuildStep table.
	CONSTRAINT [FK_ModelStepPart_BrickPart_brickPartId] FOREIGN KEY ([brickPartId]) REFERENCES [BMC].[BrickPart] ([id]),		-- Foreign key to the BrickPart table.
	CONSTRAINT [FK_ModelStepPart_BrickColour_brickColourId] FOREIGN KEY ([brickColourId]) REFERENCES [BMC].[BrickColour] ([id])		-- Foreign key to the BrickColour table.
)
GO

-- Index on the ModelStepPart table's tenantGuid field.
CREATE INDEX [I_ModelStepPart_tenantGuid] ON [BMC].[ModelStepPart] ([tenantGuid])
GO

-- Index on the ModelStepPart table's tenantGuid,modelBuildStepId fields.
CREATE INDEX [I_ModelStepPart_tenantGuid_modelBuildStepId] ON [BMC].[ModelStepPart] ([tenantGuid], [modelBuildStepId])
GO

-- Index on the ModelStepPart table's tenantGuid,brickPartId fields.
CREATE INDEX [I_ModelStepPart_tenantGuid_brickPartId] ON [BMC].[ModelStepPart] ([tenantGuid], [brickPartId])
GO

-- Index on the ModelStepPart table's tenantGuid,brickColourId fields.
CREATE INDEX [I_ModelStepPart_tenantGuid_brickColourId] ON [BMC].[ModelStepPart] ([tenantGuid], [brickColourId])
GO

-- Index on the ModelStepPart table's tenantGuid,active fields.
CREATE INDEX [I_ModelStepPart_tenantGuid_active] ON [BMC].[ModelStepPart] ([tenantGuid], [active])
GO

-- Index on the ModelStepPart table's tenantGuid,deleted fields.
CREATE INDEX [I_ModelStepPart_tenantGuid_deleted] ON [BMC].[ModelStepPart] ([tenantGuid], [deleted])
GO


-- Hierarchical tree of official LEGO themes (e.g. City → Police, Technic → Bionicle). Bulk-loaded from Rebrickable or similar sources.
CREATE TABLE [BMC].[LegoTheme]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[legoThemeId] INT NULL,		-- Parent theme for hierarchical nesting (self-referencing FK, null = top-level)
	[rebrickableThemeId] INT NOT NULL,		-- Rebrickable theme ID — source of truth for theme identity
	[brickSetThemeName] NVARCHAR(100) NULL,		-- BrickSet theme name for API calls — may differ from Rebrickable theme name (null if not mapped)
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_LegoTheme_LegoTheme_legoThemeId] FOREIGN KEY ([legoThemeId]) REFERENCES [BMC].[LegoTheme] ([id])		-- Foreign key to the LegoTheme table.
)
GO

-- Index on the LegoTheme table's name field.
CREATE INDEX [I_LegoTheme_name] ON [BMC].[LegoTheme] ([name])
GO

-- Index on the LegoTheme table's legoThemeId field.
CREATE INDEX [I_LegoTheme_legoThemeId] ON [BMC].[LegoTheme] ([legoThemeId])
GO

-- Index on the LegoTheme table's active field.
CREATE INDEX [I_LegoTheme_active] ON [BMC].[LegoTheme] ([active])
GO

-- Index on the LegoTheme table's deleted field.
CREATE INDEX [I_LegoTheme_deleted] ON [BMC].[LegoTheme] ([deleted])
GO


-- Official LEGO set definitions. Each row represents a distinct set release (e.g. 42131-1 Liebherr Crawler Crane).
CREATE TABLE [BMC].[LegoSet]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(500) NOT NULL,		-- For really long set names
	[setNumber] NVARCHAR(100) NOT NULL,		-- Official set number including variant suffix (e.g. '42131-1', '10302-1')
	[year] INT NOT NULL,		-- Release year of the set
	[partCount] INT NOT NULL,		-- Total number of parts in the set (as listed by LEGO)
	[legoThemeId] INT NULL,		-- The theme this set belongs to (null if theme not yet categorized)
	[imageUrl] NVARCHAR(250) NULL,		-- URL to the set's official box art or primary image
	[brickLinkUrl] NVARCHAR(250) NULL,		-- URL to the set's BrickLink catalogue page
	[rebrickableUrl] NVARCHAR(250) NULL,		-- URL to the set's Rebrickable page
	[rebrickableSetNum] NVARCHAR(100) NULL,		-- Explicit Rebrickable set number if it differs from setNumber
	[lastModifiedDate] DATETIME2(7) NULL,		-- Last modification date for incremental sync with Rebrickable
	[brickSetId] INT NULL,		-- BrickSet internal set ID — used for API calls (null if not yet enriched from BrickSet)
	[brickSetUrl] NVARCHAR(250) NULL,		-- URL to the set's BrickSet page
	[retailPriceUS] MONEY NULL,		-- US retail price in USD from BrickSet (null if not available)
	[retailPriceUK] MONEY NULL,		-- UK retail price in GBP from BrickSet (null if not available)
	[retailPriceCA] MONEY NULL,		-- Canadian retail price in CAD from BrickSet (null if not available)
	[retailPriceEU] MONEY NULL,		-- EU retail price in EUR from BrickSet (null if not available)
	[instructionsUrl] NVARCHAR(500) NULL,		-- URL to the set's official building instructions PDF (sourced from BrickSet)
	[subtheme] NVARCHAR(100) NULL,		-- Subtheme name from BrickSet (e.g. 'Police' under City)
	[availability] NVARCHAR(50) NULL,		-- Current availability status from BrickSet (e.g. 'Retail', 'Retired', 'LEGO exclusive')
	[minifigCount] INT NULL,		-- Number of minifigs included in the set (from BrickSet, null if unknown)
	[brickSetRating] REAL NULL,		-- Average community rating on BrickSet (1.0-5.0, null if no reviews)
	[brickSetReviewCount] INT NULL,		-- Number of community reviews on BrickSet (null if unknown)
	[brickSetLastEnrichedDate] DATETIME2(7) NULL,		-- When this set was last enriched with BrickSet data (null = never enriched)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_LegoSet_LegoTheme_legoThemeId] FOREIGN KEY ([legoThemeId]) REFERENCES [BMC].[LegoTheme] ([id]),		-- Foreign key to the LegoTheme table.
	CONSTRAINT [UC_LegoSet_setNumber] UNIQUE ( [setNumber]) 		-- Uniqueness enforced on the LegoSet table's setNumber field.
)
GO

-- Index on the LegoSet table's name field.
CREATE INDEX [I_LegoSet_name] ON [BMC].[LegoSet] ([name])
GO

-- Index on the LegoSet table's legoThemeId field.
CREATE INDEX [I_LegoSet_legoThemeId] ON [BMC].[LegoSet] ([legoThemeId])
GO

-- Index on the LegoSet table's active field.
CREATE INDEX [I_LegoSet_active] ON [BMC].[LegoSet] ([active])
GO

-- Index on the LegoSet table's deleted field.
CREATE INDEX [I_LegoSet_deleted] ON [BMC].[LegoSet] ([deleted])
GO


-- Parts inventory for each official LEGO set. Maps set → part → colour → quantity.
CREATE TABLE [BMC].[LegoSetPart]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[legoSetId] INT NOT NULL,		-- The set this inventory line belongs to
	[brickPartId] INT NOT NULL,		-- The part included in this set
	[brickColourId] INT NOT NULL,		-- The colour of this part in the set
	[quantity] INT NULL,		-- Number of this part+colour combination included in the set
	[isSpare] BIT NOT NULL DEFAULT 0,		-- Whether this is a spare part (included as extra in the bag, not used in the build)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_LegoSetPart_LegoSet_legoSetId] FOREIGN KEY ([legoSetId]) REFERENCES [BMC].[LegoSet] ([id]),		-- Foreign key to the LegoSet table.
	CONSTRAINT [FK_LegoSetPart_BrickPart_brickPartId] FOREIGN KEY ([brickPartId]) REFERENCES [BMC].[BrickPart] ([id]),		-- Foreign key to the BrickPart table.
	CONSTRAINT [FK_LegoSetPart_BrickColour_brickColourId] FOREIGN KEY ([brickColourId]) REFERENCES [BMC].[BrickColour] ([id])		-- Foreign key to the BrickColour table.
)
GO

-- Index on the LegoSetPart table's legoSetId field.
CREATE INDEX [I_LegoSetPart_legoSetId] ON [BMC].[LegoSetPart] ([legoSetId])
GO

-- Index on the LegoSetPart table's brickPartId field.
CREATE INDEX [I_LegoSetPart_brickPartId] ON [BMC].[LegoSetPart] ([brickPartId])
GO

-- Index on the LegoSetPart table's brickColourId field.
CREATE INDEX [I_LegoSetPart_brickColourId] ON [BMC].[LegoSetPart] ([brickColourId])
GO

-- Index on the LegoSetPart table's active field.
CREATE INDEX [I_LegoSetPart_active] ON [BMC].[LegoSetPart] ([active])
GO

-- Index on the LegoSetPart table's deleted field.
CREATE INDEX [I_LegoSetPart_deleted] ON [BMC].[LegoSetPart] ([deleted])
GO


-- Relationships between parts: alternates, molds, prints, pairs, sub-parts, and patterns. Bulk-loaded from Rebrickable part_relationships.csv.
CREATE TABLE [BMC].[BrickPartRelationship]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[childBrickPartId] INT NOT NULL,		-- The child part in the relationship
	[parentBrickPartId] INT NOT NULL,		-- The parent part in the relationship
	[relationshipType] NVARCHAR(50) NOT NULL,		-- Type of relationship: Print, Pair, SubPart, Mold, Pattern, or Alternate
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_BrickPartRelationship_BrickPart_childBrickPartId] FOREIGN KEY ([childBrickPartId]) REFERENCES [BMC].[BrickPart] ([id]),		-- Foreign key to the BrickPart table.
	CONSTRAINT [FK_BrickPartRelationship_BrickPart_parentBrickPartId] FOREIGN KEY ([parentBrickPartId]) REFERENCES [BMC].[BrickPart] ([id])		-- Foreign key to the BrickPart table.
)
GO

-- Index on the BrickPartRelationship table's childBrickPartId field.
CREATE INDEX [I_BrickPartRelationship_childBrickPartId] ON [BMC].[BrickPartRelationship] ([childBrickPartId])
GO

-- Index on the BrickPartRelationship table's parentBrickPartId field.
CREATE INDEX [I_BrickPartRelationship_parentBrickPartId] ON [BMC].[BrickPartRelationship] ([parentBrickPartId])
GO

-- Index on the BrickPartRelationship table's active field.
CREATE INDEX [I_BrickPartRelationship_active] ON [BMC].[BrickPartRelationship] ([active])
GO

-- Index on the BrickPartRelationship table's deleted field.
CREATE INDEX [I_BrickPartRelationship_deleted] ON [BMC].[BrickPartRelationship] ([deleted])
GO


-- LEGO element IDs representing specific part+colour combinations. Used for cross-referencing with official LEGO catalogues and BrickLink.
CREATE TABLE [BMC].[BrickElement]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[elementId] NVARCHAR(50) NOT NULL,		-- Official LEGO element ID (unique identifier for a specific part+colour combination)
	[brickPartId] INT NOT NULL,		-- The part this element represents
	[brickColourId] INT NOT NULL,		-- The colour of this element
	[designId] NVARCHAR(50) NULL,		-- LEGO design ID (null if not available)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_BrickElement_BrickPart_brickPartId] FOREIGN KEY ([brickPartId]) REFERENCES [BMC].[BrickPart] ([id]),		-- Foreign key to the BrickPart table.
	CONSTRAINT [FK_BrickElement_BrickColour_brickColourId] FOREIGN KEY ([brickColourId]) REFERENCES [BMC].[BrickColour] ([id]),		-- Foreign key to the BrickColour table.
	CONSTRAINT [UC_BrickElement_elementId] UNIQUE ( [elementId]) 		-- Uniqueness enforced on the BrickElement table's elementId field.
)
GO

-- Index on the BrickElement table's brickPartId field.
CREATE INDEX [I_BrickElement_brickPartId] ON [BMC].[BrickElement] ([brickPartId])
GO

-- Index on the BrickElement table's brickColourId field.
CREATE INDEX [I_BrickElement_brickColourId] ON [BMC].[BrickElement] ([brickColourId])
GO

-- Index on the BrickElement table's active field.
CREATE INDEX [I_BrickElement_active] ON [BMC].[BrickElement] ([active])
GO

-- Index on the BrickElement table's deleted field.
CREATE INDEX [I_BrickElement_deleted] ON [BMC].[BrickElement] ([deleted])
GO


-- Official LEGO minifigure definitions. Each row represents a distinct minifig (e.g. fig-000001 Han Solo).
CREATE TABLE [BMC].[LegoMinifig]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(500) NOT NULL,		-- Minifig name — can be long descriptive text from Rebrickable
	[figNumber] NVARCHAR(100) NOT NULL,		-- Rebrickable minifig number (e.g. 'fig-000001')
	[partCount] INT NOT NULL,		-- Total number of parts in the minifig
	[imageUrl] NVARCHAR(250) NULL,		-- URL to the minifig's image
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_LegoMinifig_figNumber] UNIQUE ( [figNumber]) 		-- Uniqueness enforced on the LegoMinifig table's figNumber field.
)
GO

-- Index on the LegoMinifig table's active field.
CREATE INDEX [I_LegoMinifig_active] ON [BMC].[LegoMinifig] ([active])
GO

-- Index on the LegoMinifig table's deleted field.
CREATE INDEX [I_LegoMinifig_deleted] ON [BMC].[LegoMinifig] ([deleted])
GO


-- Minifigs included in each official LEGO set's inventory, with quantities.
CREATE TABLE [BMC].[LegoSetMinifig]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[legoSetId] INT NOT NULL,		-- The set this minifig belongs to
	[legoMinifigId] INT NOT NULL,		-- The minifig included in the set
	[quantity] INT NULL,		-- Number of this minifig included in the set
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_LegoSetMinifig_LegoSet_legoSetId] FOREIGN KEY ([legoSetId]) REFERENCES [BMC].[LegoSet] ([id]),		-- Foreign key to the LegoSet table.
	CONSTRAINT [FK_LegoSetMinifig_LegoMinifig_legoMinifigId] FOREIGN KEY ([legoMinifigId]) REFERENCES [BMC].[LegoMinifig] ([id])		-- Foreign key to the LegoMinifig table.
)
GO

-- Index on the LegoSetMinifig table's legoSetId field.
CREATE INDEX [I_LegoSetMinifig_legoSetId] ON [BMC].[LegoSetMinifig] ([legoSetId])
GO

-- Index on the LegoSetMinifig table's legoMinifigId field.
CREATE INDEX [I_LegoSetMinifig_legoMinifigId] ON [BMC].[LegoSetMinifig] ([legoMinifigId])
GO

-- Index on the LegoSetMinifig table's active field.
CREATE INDEX [I_LegoSetMinifig_active] ON [BMC].[LegoSetMinifig] ([active])
GO

-- Index on the LegoSetMinifig table's deleted field.
CREATE INDEX [I_LegoSetMinifig_deleted] ON [BMC].[LegoSetMinifig] ([deleted])
GO


-- Sets included within other sets (e.g. polybags inside a larger set). Derived from Rebrickable inventory_sets.csv.
CREATE TABLE [BMC].[LegoSetSubset]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[parentLegoSetId] INT NOT NULL,		-- The parent set that contains the subset
	[childLegoSetId] INT NOT NULL,		-- The subset included within the parent set
	[quantity] INT NULL,		-- Number of copies of the subset included in the parent
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_LegoSetSubset_LegoSet_parentLegoSetId] FOREIGN KEY ([parentLegoSetId]) REFERENCES [BMC].[LegoSet] ([id]),		-- Foreign key to the LegoSet table.
	CONSTRAINT [FK_LegoSetSubset_LegoSet_childLegoSetId] FOREIGN KEY ([childLegoSetId]) REFERENCES [BMC].[LegoSet] ([id])		-- Foreign key to the LegoSet table.
)
GO

-- Index on the LegoSetSubset table's parentLegoSetId field.
CREATE INDEX [I_LegoSetSubset_parentLegoSetId] ON [BMC].[LegoSetSubset] ([parentLegoSetId])
GO

-- Index on the LegoSetSubset table's childLegoSetId field.
CREATE INDEX [I_LegoSetSubset_childLegoSetId] ON [BMC].[LegoSetSubset] ([childLegoSetId])
GO

-- Index on the LegoSetSubset table's active field.
CREATE INDEX [I_LegoSetSubset_active] ON [BMC].[LegoSetSubset] ([active])
GO

-- Index on the LegoSetSubset table's deleted field.
CREATE INDEX [I_LegoSetSubset_deleted] ON [BMC].[LegoSetSubset] ([deleted])
GO


-- A user's named part collection or palette. Users can have multiple collections (e.g. 'My Inventory', 'Technic Parts', 'Parts for MOC #5').
CREATE TABLE [BMC].[UserCollection]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[isDefault] BIT NOT NULL DEFAULT 0,		-- Whether this is the user's primary / default collection
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_UserCollection_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the UserCollection table's tenantGuid and name fields.
)
GO

-- Index on the UserCollection table's tenantGuid field.
CREATE INDEX [I_UserCollection_tenantGuid] ON [BMC].[UserCollection] ([tenantGuid])
GO

-- Index on the UserCollection table's tenantGuid,name fields.
CREATE INDEX [I_UserCollection_tenantGuid_name] ON [BMC].[UserCollection] ([tenantGuid], [name])
GO

-- Index on the UserCollection table's tenantGuid,active fields.
CREATE INDEX [I_UserCollection_tenantGuid_active] ON [BMC].[UserCollection] ([tenantGuid], [active])
GO

-- Index on the UserCollection table's tenantGuid,deleted fields.
CREATE INDEX [I_UserCollection_tenantGuid_deleted] ON [BMC].[UserCollection] ([tenantGuid], [deleted])
GO


-- The change history for records from the UserCollection table.
CREATE TABLE [BMC].[UserCollectionChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userCollectionId] INT NOT NULL,		-- Link to the UserCollection table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_UserCollectionChangeHistory_UserCollection_userCollectionId] FOREIGN KEY ([userCollectionId]) REFERENCES [BMC].[UserCollection] ([id])		-- Foreign key to the UserCollection table.
)
GO

-- Index on the UserCollectionChangeHistory table's tenantGuid field.
CREATE INDEX [I_UserCollectionChangeHistory_tenantGuid] ON [BMC].[UserCollectionChangeHistory] ([tenantGuid])
GO

-- Index on the UserCollectionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_UserCollectionChangeHistory_tenantGuid_versionNumber] ON [BMC].[UserCollectionChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the UserCollectionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_UserCollectionChangeHistory_tenantGuid_timeStamp] ON [BMC].[UserCollectionChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the UserCollectionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_UserCollectionChangeHistory_tenantGuid_userId] ON [BMC].[UserCollectionChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the UserCollectionChangeHistory table's tenantGuid,userCollectionId fields.
CREATE INDEX [I_UserCollectionChangeHistory_tenantGuid_userCollectionId] ON [BMC].[UserCollectionChangeHistory] ([tenantGuid], [userCollectionId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Individual part+colour entries within a user collection, with quantity owned and quantity currently allocated to projects.
CREATE TABLE [BMC].[UserCollectionPart]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userCollectionId] INT NOT NULL,		-- The collection this part entry belongs to
	[brickPartId] INT NOT NULL,		-- The part definition
	[brickColourId] INT NOT NULL,		-- The specific colour of this part
	[quantityOwned] INT NULL,		-- Total number of this part+colour the user owns
	[quantityUsed] INT NULL,		-- Number currently allocated to projects (for build-with-what-you-own filtering)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_UserCollectionPart_UserCollection_userCollectionId] FOREIGN KEY ([userCollectionId]) REFERENCES [BMC].[UserCollection] ([id]),		-- Foreign key to the UserCollection table.
	CONSTRAINT [FK_UserCollectionPart_BrickPart_brickPartId] FOREIGN KEY ([brickPartId]) REFERENCES [BMC].[BrickPart] ([id]),		-- Foreign key to the BrickPart table.
	CONSTRAINT [FK_UserCollectionPart_BrickColour_brickColourId] FOREIGN KEY ([brickColourId]) REFERENCES [BMC].[BrickColour] ([id]),		-- Foreign key to the BrickColour table.
	CONSTRAINT [UC_UserCollectionPart_tenantGuid_userCollectionId_brickPartId_brickColourId] UNIQUE ( [tenantGuid], [userCollectionId], [brickPartId], [brickColourId]) 		-- Uniqueness enforced on the UserCollectionPart table's tenantGuid and userCollectionId and brickPartId and brickColourId fields.
)
GO

-- Index on the UserCollectionPart table's tenantGuid field.
CREATE INDEX [I_UserCollectionPart_tenantGuid] ON [BMC].[UserCollectionPart] ([tenantGuid])
GO

-- Index on the UserCollectionPart table's tenantGuid,userCollectionId fields.
CREATE INDEX [I_UserCollectionPart_tenantGuid_userCollectionId] ON [BMC].[UserCollectionPart] ([tenantGuid], [userCollectionId])
GO

-- Index on the UserCollectionPart table's tenantGuid,brickPartId fields.
CREATE INDEX [I_UserCollectionPart_tenantGuid_brickPartId] ON [BMC].[UserCollectionPart] ([tenantGuid], [brickPartId])
GO

-- Index on the UserCollectionPart table's tenantGuid,brickColourId fields.
CREATE INDEX [I_UserCollectionPart_tenantGuid_brickColourId] ON [BMC].[UserCollectionPart] ([tenantGuid], [brickColourId])
GO

-- Index on the UserCollectionPart table's tenantGuid,active fields.
CREATE INDEX [I_UserCollectionPart_tenantGuid_active] ON [BMC].[UserCollectionPart] ([tenantGuid], [active])
GO

-- Index on the UserCollectionPart table's tenantGuid,deleted fields.
CREATE INDEX [I_UserCollectionPart_tenantGuid_deleted] ON [BMC].[UserCollectionPart] ([tenantGuid], [deleted])
GO


-- Parts the user wants to acquire. Can optionally specify a colour or leave null for any colour.
CREATE TABLE [BMC].[UserWishlistItem]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userCollectionId] INT NOT NULL,		-- The collection this wishlist item is associated with
	[brickPartId] INT NOT NULL,		-- The desired part
	[brickColourId] INT NULL,		-- The desired colour (null = any colour)
	[quantityDesired] INT NULL,		-- Number of this part the user wants to acquire
	[notes] NVARCHAR(MAX) NULL,		-- Free-form notes about the wishlist item (e.g. source, priority)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_UserWishlistItem_UserCollection_userCollectionId] FOREIGN KEY ([userCollectionId]) REFERENCES [BMC].[UserCollection] ([id]),		-- Foreign key to the UserCollection table.
	CONSTRAINT [FK_UserWishlistItem_BrickPart_brickPartId] FOREIGN KEY ([brickPartId]) REFERENCES [BMC].[BrickPart] ([id]),		-- Foreign key to the BrickPart table.
	CONSTRAINT [FK_UserWishlistItem_BrickColour_brickColourId] FOREIGN KEY ([brickColourId]) REFERENCES [BMC].[BrickColour] ([id])		-- Foreign key to the BrickColour table.
)
GO

-- Index on the UserWishlistItem table's tenantGuid field.
CREATE INDEX [I_UserWishlistItem_tenantGuid] ON [BMC].[UserWishlistItem] ([tenantGuid])
GO

-- Index on the UserWishlistItem table's tenantGuid,userCollectionId fields.
CREATE INDEX [I_UserWishlistItem_tenantGuid_userCollectionId] ON [BMC].[UserWishlistItem] ([tenantGuid], [userCollectionId])
GO

-- Index on the UserWishlistItem table's tenantGuid,brickPartId fields.
CREATE INDEX [I_UserWishlistItem_tenantGuid_brickPartId] ON [BMC].[UserWishlistItem] ([tenantGuid], [brickPartId])
GO

-- Index on the UserWishlistItem table's tenantGuid,brickColourId fields.
CREATE INDEX [I_UserWishlistItem_tenantGuid_brickColourId] ON [BMC].[UserWishlistItem] ([tenantGuid], [brickColourId])
GO

-- Index on the UserWishlistItem table's tenantGuid,active fields.
CREATE INDEX [I_UserWishlistItem_tenantGuid_active] ON [BMC].[UserWishlistItem] ([tenantGuid], [active])
GO

-- Index on the UserWishlistItem table's tenantGuid,deleted fields.
CREATE INDEX [I_UserWishlistItem_tenantGuid_deleted] ON [BMC].[UserWishlistItem] ([tenantGuid], [deleted])
GO


-- Tracks which official LEGO sets have been imported into a user's collection, with quantity (e.g. 2 copies of set 42131).
CREATE TABLE [BMC].[UserCollectionSetImport]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userCollectionId] INT NOT NULL,		-- The collection the set was imported into
	[legoSetId] INT NOT NULL,		-- The set that was imported
	[quantity] INT NULL,		-- Number of copies of this set imported (e.g. user owns 2 copies)
	[importedDate] DATETIME2(7) NULL,		-- Date/time the set was imported into the collection
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_UserCollectionSetImport_UserCollection_userCollectionId] FOREIGN KEY ([userCollectionId]) REFERENCES [BMC].[UserCollection] ([id]),		-- Foreign key to the UserCollection table.
	CONSTRAINT [FK_UserCollectionSetImport_LegoSet_legoSetId] FOREIGN KEY ([legoSetId]) REFERENCES [BMC].[LegoSet] ([id]),		-- Foreign key to the LegoSet table.
	CONSTRAINT [UC_UserCollectionSetImport_tenantGuid_userCollectionId_legoSetId] UNIQUE ( [tenantGuid], [userCollectionId], [legoSetId]) 		-- Uniqueness enforced on the UserCollectionSetImport table's tenantGuid and userCollectionId and legoSetId fields.
)
GO

-- Index on the UserCollectionSetImport table's tenantGuid field.
CREATE INDEX [I_UserCollectionSetImport_tenantGuid] ON [BMC].[UserCollectionSetImport] ([tenantGuid])
GO

-- Index on the UserCollectionSetImport table's tenantGuid,userCollectionId fields.
CREATE INDEX [I_UserCollectionSetImport_tenantGuid_userCollectionId] ON [BMC].[UserCollectionSetImport] ([tenantGuid], [userCollectionId])
GO

-- Index on the UserCollectionSetImport table's tenantGuid,legoSetId fields.
CREATE INDEX [I_UserCollectionSetImport_tenantGuid_legoSetId] ON [BMC].[UserCollectionSetImport] ([tenantGuid], [legoSetId])
GO

-- Index on the UserCollectionSetImport table's tenantGuid,active fields.
CREATE INDEX [I_UserCollectionSetImport_tenantGuid_active] ON [BMC].[UserCollectionSetImport] ([tenantGuid], [active])
GO

-- Index on the UserCollectionSetImport table's tenantGuid,deleted fields.
CREATE INDEX [I_UserCollectionSetImport_tenantGuid_deleted] ON [BMC].[UserCollectionSetImport] ([tenantGuid], [deleted])
GO


-- Stores each user's Rebrickable credentials/token and sync configuration. One link per tenant. Supports three auth modes: ApiToken, EncryptedCredentials, SessionOnly.
CREATE TABLE [BMC].[RebrickableUserLink]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[rebrickableUsername] NVARCHAR(100) NOT NULL,		-- User's Rebrickable username for display and reference
	[encryptedApiToken] NVARCHAR(500) NOT NULL,		-- Encrypted Rebrickable user token — used for API calls on behalf of the user
	[authMode] NVARCHAR(50) NOT NULL,		-- Auth trust level: ApiToken, EncryptedCredentials, SessionOnly
	[encryptedPassword] NVARCHAR(500) NULL,		-- Encrypted Rebrickable password — only used in EncryptedCredentials auth mode (null otherwise)
	[syncEnabled] BIT NOT NULL DEFAULT 1,		-- Whether automatic sync is enabled for this user
	[syncDirectionFlags] NVARCHAR(50) NOT NULL,		-- Integration mode: None, RealTime, PushOnly, ImportOnly
	[pullIntervalMinutes] INT NULL,		-- Periodic pull interval in minutes for RealTime mode (null = manual only)
	[lastSyncDate] DATETIME2(7) NULL,		-- Date/time of last successful sync with Rebrickable (legacy — kept for compatibility)
	[lastPullDate] DATETIME2(7) NULL,		-- Date/time of last successful pull from Rebrickable
	[lastPushDate] DATETIME2(7) NULL,		-- Date/time of last successful push to Rebrickable
	[lastSyncError] NVARCHAR(MAX) NULL,		-- Last sync error message for display to the user (null = no error)
	[tokenExpiryDays] INT NULL,		-- User-configurable auto-clear interval in days (null = never auto-clear)
	[tokenStoredDate] DATETIME2(7) NULL,		-- When the token was last stored or refreshed — used with tokenExpiryDays for auto-expiry
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_RebrickableUserLink_tenantGuid] UNIQUE ( [tenantGuid]) 		-- Uniqueness enforced on the RebrickableUserLink table's tenantGuid field.
)
GO

-- Index on the RebrickableUserLink table's tenantGuid field.
CREATE INDEX [I_RebrickableUserLink_tenantGuid] ON [BMC].[RebrickableUserLink] ([tenantGuid])
GO

-- Index on the RebrickableUserLink table's tenantGuid,active fields.
CREATE INDEX [I_RebrickableUserLink_tenantGuid_active] ON [BMC].[RebrickableUserLink] ([tenantGuid], [active])
GO

-- Index on the RebrickableUserLink table's tenantGuid,deleted fields.
CREATE INDEX [I_RebrickableUserLink_tenantGuid_deleted] ON [BMC].[RebrickableUserLink] ([tenantGuid], [deleted])
GO


-- Full audit log of every Rebrickable API call BMC makes on behalf of a user. Enables the Communications Panel for total transparency. Every push, pull, login, and error is recorded.
CREATE TABLE [BMC].[RebrickableTransaction]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[transactionDate] DATETIME2(7) NULL,		-- Date/time the API call was made
	[direction] NVARCHAR(50) NOT NULL,		-- Direction of data flow: Push, Pull
	[httpMethod] NVARCHAR(50) NOT NULL,		-- HTTP method used: GET, POST, PUT, PATCH, DELETE
	[endpoint] NVARCHAR(500) NOT NULL,		-- The Rebrickable API URL that was called
	[requestSummary] NVARCHAR(MAX) NULL,		-- Human-readable description of the operation, e.g. 'Added set 42131-1 x1'
	[responseStatusCode] INT NULL,		-- HTTP status code returned by Rebrickable
	[responseBody] NVARCHAR(MAX) NULL,		-- Raw response body from Rebrickable (for debugging — may be null for large responses)
	[success] BIT NOT NULL DEFAULT 1,		-- Whether the API call completed successfully
	[errorMessage] NVARCHAR(MAX) NULL,		-- Error details if the call failed (null on success)
	[triggeredBy] NVARCHAR(100) NOT NULL,		-- What initiated this call: UserAction, PeriodicSync, ManualPull, SessionLogin
	[recordCount] INT NULL,		-- Number of rows retrieved or affected by this API call
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the RebrickableTransaction table's tenantGuid field.
CREATE INDEX [I_RebrickableTransaction_tenantGuid] ON [BMC].[RebrickableTransaction] ([tenantGuid])
GO

-- Index on the RebrickableTransaction table's tenantGuid,active fields.
CREATE INDEX [I_RebrickableTransaction_tenantGuid_active] ON [BMC].[RebrickableTransaction] ([tenantGuid], [active])
GO

-- Index on the RebrickableTransaction table's tenantGuid,deleted fields.
CREATE INDEX [I_RebrickableTransaction_tenantGuid_deleted] ON [BMC].[RebrickableTransaction] ([tenantGuid], [deleted])
GO


-- Outbound sync queue for resilient Rebrickable API writes. When a user modifies collection data locally, a queue entry is created. A background service picks up pending items and pushes them to Rebrickable. Retries on failure with exponential backoff.
CREATE TABLE [BMC].[RebrickableSyncQueue]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[operationType] NVARCHAR(50) NOT NULL,		-- Sync operation: Create, Update, Delete
	[entityType] NVARCHAR(50) NOT NULL,		-- Entity being synced: SetList, SetListItem, PartList, PartListItem, LostPart
	[entityId] BIGINT NOT NULL,		-- Database ID of the entity being synced
	[payload] NVARCHAR(MAX) NULL,		-- JSON-serialized data needed by the Rebrickable API call
	[status] NVARCHAR(50) NOT NULL,		-- Queue status: Pending, InProgress, Completed, Failed, Abandoned
	[createdDate] DATETIME2(7) NULL,		-- When this queue entry was created
	[lastAttemptDate] DATETIME2(7) NULL,		-- When the last processing attempt occurred (null = never attempted)
	[completedDate] DATETIME2(7) NULL,		-- When processing completed successfully (null = not yet completed)
	[attemptCount] INT NOT NULL,		-- Number of processing attempts so far
	[maxAttempts] INT NOT NULL,		-- Maximum retry attempts before marking as Abandoned (default: 5)
	[errorMessage] NVARCHAR(MAX) NULL,		-- Last error message from a failed attempt (null on success)
	[responseBody] NVARCHAR(MAX) NULL,		-- Last response body from Rebrickable for debugging (null on success)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the RebrickableSyncQueue table's tenantGuid field.
CREATE INDEX [I_RebrickableSyncQueue_tenantGuid] ON [BMC].[RebrickableSyncQueue] ([tenantGuid])
GO

-- Index on the RebrickableSyncQueue table's tenantGuid,active fields.
CREATE INDEX [I_RebrickableSyncQueue_tenantGuid_active] ON [BMC].[RebrickableSyncQueue] ([tenantGuid], [active])
GO

-- Index on the RebrickableSyncQueue table's tenantGuid,deleted fields.
CREATE INDEX [I_RebrickableSyncQueue_tenantGuid_deleted] ON [BMC].[RebrickableSyncQueue] ([tenantGuid], [deleted])
GO


-- Stores each user's BrickSet userHash and sync configuration. One link per tenant. BrickSet auth uses an app-level API key (in appsettings.json) plus a session-based userHash obtained via login.
CREATE TABLE [BMC].[BrickSetUserLink]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[brickSetUsername] NVARCHAR(100) NOT NULL,		-- User's BrickSet username for display and reference
	[encryptedUserHash] NVARCHAR(500) NOT NULL,		-- Encrypted BrickSet userHash — obtained via login, used for collection API calls
	[encryptedPassword] NVARCHAR(500) NULL,		-- Encrypted BrickSet password — stored for re-authentication when userHash expires (null if user chose not to store)
	[syncEnabled] BIT NOT NULL DEFAULT 1,		-- Whether automatic sync is enabled for this user
	[syncDirection] NVARCHAR(50) NOT NULL,		-- Integration mode: None, PullOnly, PushOnly, Bidirectional
	[lastSyncDate] DATETIME2(7) NULL,		-- Date/time of last successful sync with BrickSet
	[lastPullDate] DATETIME2(7) NULL,		-- Date/time of last successful pull from BrickSet
	[lastPushDate] DATETIME2(7) NULL,		-- Date/time of last successful push to BrickSet
	[lastSyncError] NVARCHAR(MAX) NULL,		-- Last sync error message for display to the user (null = no error)
	[userHashStoredDate] DATETIME2(7) NULL,		-- When the userHash was last stored or refreshed — used for session expiry tracking
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_BrickSetUserLink_tenantGuid] UNIQUE ( [tenantGuid]) 		-- Uniqueness enforced on the BrickSetUserLink table's tenantGuid field.
)
GO

-- Index on the BrickSetUserLink table's tenantGuid field.
CREATE INDEX [I_BrickSetUserLink_tenantGuid] ON [BMC].[BrickSetUserLink] ([tenantGuid])
GO

-- Index on the BrickSetUserLink table's tenantGuid,active fields.
CREATE INDEX [I_BrickSetUserLink_tenantGuid_active] ON [BMC].[BrickSetUserLink] ([tenantGuid], [active])
GO

-- Index on the BrickSetUserLink table's tenantGuid,deleted fields.
CREATE INDEX [I_BrickSetUserLink_tenantGuid_deleted] ON [BMC].[BrickSetUserLink] ([tenantGuid], [deleted])
GO


-- Full audit log of every BrickSet API call BMC makes on behalf of a user. Mirrors the RebrickableTransaction pattern for complete transparency.
CREATE TABLE [BMC].[BrickSetTransaction]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[transactionDate] DATETIME2(7) NULL,		-- Date/time the API call was made
	[direction] NVARCHAR(50) NOT NULL,		-- Direction of data flow: Push, Pull, Enrich
	[methodName] NVARCHAR(100) NOT NULL,		-- BrickSet API method name (e.g. 'getSets', 'setCollection', 'getInstructions')
	[requestSummary] NVARCHAR(MAX) NULL,		-- Human-readable description of the operation, e.g. 'Enriched set 42131-1 with pricing data'
	[success] BIT NOT NULL DEFAULT 1,		-- Whether the API call completed successfully
	[errorMessage] NVARCHAR(MAX) NULL,		-- Error details if the call failed (null on success)
	[triggeredBy] NVARCHAR(100) NOT NULL,		-- What initiated this call: UserAction, SetDetailView, ManualEnrich, CollectionSync
	[recordCount] INT NULL,		-- Number of rows retrieved or affected by this API call
	[apiCallsRemaining] INT NULL,		-- Daily API call quota remaining after this call (from getKeyUsageStats)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the BrickSetTransaction table's tenantGuid field.
CREATE INDEX [I_BrickSetTransaction_tenantGuid] ON [BMC].[BrickSetTransaction] ([tenantGuid])
GO

-- Index on the BrickSetTransaction table's tenantGuid,active fields.
CREATE INDEX [I_BrickSetTransaction_tenantGuid_active] ON [BMC].[BrickSetTransaction] ([tenantGuid], [active])
GO

-- Index on the BrickSetTransaction table's tenantGuid,deleted fields.
CREATE INDEX [I_BrickSetTransaction_tenantGuid_deleted] ON [BMC].[BrickSetTransaction] ([tenantGuid], [deleted])
GO


-- Cached community reviews from BrickSet for official LEGO sets. Pulled periodically via the getReviews API method. Reviews are read-only reference data, not user-editable.
CREATE TABLE [BMC].[BrickSetSetReview]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[legoSetId] INT NOT NULL,		-- The set this review is for
	[reviewAuthor] NVARCHAR(100) NOT NULL,		-- BrickSet username of the reviewer
	[reviewDate] DATETIME2(7) NULL,		-- When the review was posted on BrickSet
	[reviewTitle] NVARCHAR(MAX) NULL,		-- Review title/heading
	[reviewBody] NVARCHAR(MAX) NULL,		-- Full review text
	[overallRating] INT NULL,		-- Overall rating (1-5)
	[buildingExperienceRating] INT NULL,		-- Building experience rating (1-5, null if not rated)
	[valueForMoneyRating] INT NULL,		-- Value for money rating (1-5, null if not rated)
	[partsRating] INT NULL,		-- Parts/pieces rating (1-5, null if not rated)
	[playabilityRating] INT NULL,		-- Playability rating (1-5, null if not rated)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_BrickSetSetReview_LegoSet_legoSetId] FOREIGN KEY ([legoSetId]) REFERENCES [BMC].[LegoSet] ([id])		-- Foreign key to the LegoSet table.
)
GO

-- Index on the BrickSetSetReview table's legoSetId field.
CREATE INDEX [I_BrickSetSetReview_legoSetId] ON [BMC].[BrickSetSetReview] ([legoSetId])
GO

-- Index on the BrickSetSetReview table's active field.
CREATE INDEX [I_BrickSetSetReview_active] ON [BMC].[BrickSetSetReview] ([active])
GO

-- Index on the BrickSetSetReview table's deleted field.
CREATE INDEX [I_BrickSetSetReview_deleted] ON [BMC].[BrickSetSetReview] ([deleted])
GO


-- Named part lists, mirroring Rebrickable's partlists/ endpoint. Users can have multiple named lists for organizing parts.
CREATE TABLE [BMC].[UserPartList]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[isBuildable] BIT NOT NULL DEFAULT 0,		-- Whether this list represents buildable parts (for build matching)
	[rebrickableListId] INT NULL,		-- Rebrickable list ID for bidirectional sync (null = BMC-only list)
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_UserPartList_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the UserPartList table's tenantGuid and name fields.
)
GO

-- Index on the UserPartList table's tenantGuid field.
CREATE INDEX [I_UserPartList_tenantGuid] ON [BMC].[UserPartList] ([tenantGuid])
GO

-- Index on the UserPartList table's tenantGuid,name fields.
CREATE INDEX [I_UserPartList_tenantGuid_name] ON [BMC].[UserPartList] ([tenantGuid], [name])
GO

-- Index on the UserPartList table's tenantGuid,active fields.
CREATE INDEX [I_UserPartList_tenantGuid_active] ON [BMC].[UserPartList] ([tenantGuid], [active])
GO

-- Index on the UserPartList table's tenantGuid,deleted fields.
CREATE INDEX [I_UserPartList_tenantGuid_deleted] ON [BMC].[UserPartList] ([tenantGuid], [deleted])
GO


-- The change history for records from the UserPartList table.
CREATE TABLE [BMC].[UserPartListChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userPartListId] INT NOT NULL,		-- Link to the UserPartList table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_UserPartListChangeHistory_UserPartList_userPartListId] FOREIGN KEY ([userPartListId]) REFERENCES [BMC].[UserPartList] ([id])		-- Foreign key to the UserPartList table.
)
GO

-- Index on the UserPartListChangeHistory table's tenantGuid field.
CREATE INDEX [I_UserPartListChangeHistory_tenantGuid] ON [BMC].[UserPartListChangeHistory] ([tenantGuid])
GO

-- Index on the UserPartListChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_UserPartListChangeHistory_tenantGuid_versionNumber] ON [BMC].[UserPartListChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the UserPartListChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_UserPartListChangeHistory_tenantGuid_timeStamp] ON [BMC].[UserPartListChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the UserPartListChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_UserPartListChangeHistory_tenantGuid_userId] ON [BMC].[UserPartListChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the UserPartListChangeHistory table's tenantGuid,userPartListId fields.
CREATE INDEX [I_UserPartListChangeHistory_tenantGuid_userPartListId] ON [BMC].[UserPartListChangeHistory] ([tenantGuid], [userPartListId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Individual part+colour entries within a user's named part list. Mirrors Rebrickable's partlists/{id}/parts/ endpoint.
CREATE TABLE [BMC].[UserPartListItem]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userPartListId] INT NOT NULL,		-- The part list this item belongs to
	[brickPartId] INT NOT NULL,		-- The part definition
	[brickColourId] INT NOT NULL,		-- The specific colour of this part
	[quantity] INT NOT NULL,		-- Number of this part+colour in the list
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_UserPartListItem_UserPartList_userPartListId] FOREIGN KEY ([userPartListId]) REFERENCES [BMC].[UserPartList] ([id]),		-- Foreign key to the UserPartList table.
	CONSTRAINT [FK_UserPartListItem_BrickPart_brickPartId] FOREIGN KEY ([brickPartId]) REFERENCES [BMC].[BrickPart] ([id]),		-- Foreign key to the BrickPart table.
	CONSTRAINT [FK_UserPartListItem_BrickColour_brickColourId] FOREIGN KEY ([brickColourId]) REFERENCES [BMC].[BrickColour] ([id]),		-- Foreign key to the BrickColour table.
	CONSTRAINT [UC_UserPartListItem_tenantGuid_userPartListId_brickPartId_brickColourId] UNIQUE ( [tenantGuid], [userPartListId], [brickPartId], [brickColourId]) 		-- Uniqueness enforced on the UserPartListItem table's tenantGuid and userPartListId and brickPartId and brickColourId fields.
)
GO

-- Index on the UserPartListItem table's tenantGuid field.
CREATE INDEX [I_UserPartListItem_tenantGuid] ON [BMC].[UserPartListItem] ([tenantGuid])
GO

-- Index on the UserPartListItem table's tenantGuid,userPartListId fields.
CREATE INDEX [I_UserPartListItem_tenantGuid_userPartListId] ON [BMC].[UserPartListItem] ([tenantGuid], [userPartListId])
GO

-- Index on the UserPartListItem table's tenantGuid,brickPartId fields.
CREATE INDEX [I_UserPartListItem_tenantGuid_brickPartId] ON [BMC].[UserPartListItem] ([tenantGuid], [brickPartId])
GO

-- Index on the UserPartListItem table's tenantGuid,brickColourId fields.
CREATE INDEX [I_UserPartListItem_tenantGuid_brickColourId] ON [BMC].[UserPartListItem] ([tenantGuid], [brickColourId])
GO

-- Index on the UserPartListItem table's tenantGuid,active fields.
CREATE INDEX [I_UserPartListItem_tenantGuid_active] ON [BMC].[UserPartListItem] ([tenantGuid], [active])
GO

-- Index on the UserPartListItem table's tenantGuid,deleted fields.
CREATE INDEX [I_UserPartListItem_tenantGuid_deleted] ON [BMC].[UserPartListItem] ([tenantGuid], [deleted])
GO


-- Named set lists, mirroring Rebrickable's setlists/ endpoint. Users can have multiple named lists for organizing sets.
CREATE TABLE [BMC].[UserSetList]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[isBuildable] BIT NOT NULL DEFAULT 0,		-- Whether this list represents buildable sets (for build matching)
	[rebrickableListId] INT NULL,		-- Rebrickable list ID for bidirectional sync (null = BMC-only list)
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_UserSetList_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the UserSetList table's tenantGuid and name fields.
)
GO

-- Index on the UserSetList table's tenantGuid field.
CREATE INDEX [I_UserSetList_tenantGuid] ON [BMC].[UserSetList] ([tenantGuid])
GO

-- Index on the UserSetList table's tenantGuid,name fields.
CREATE INDEX [I_UserSetList_tenantGuid_name] ON [BMC].[UserSetList] ([tenantGuid], [name])
GO

-- Index on the UserSetList table's tenantGuid,active fields.
CREATE INDEX [I_UserSetList_tenantGuid_active] ON [BMC].[UserSetList] ([tenantGuid], [active])
GO

-- Index on the UserSetList table's tenantGuid,deleted fields.
CREATE INDEX [I_UserSetList_tenantGuid_deleted] ON [BMC].[UserSetList] ([tenantGuid], [deleted])
GO


-- The change history for records from the UserSetList table.
CREATE TABLE [BMC].[UserSetListChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userSetListId] INT NOT NULL,		-- Link to the UserSetList table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_UserSetListChangeHistory_UserSetList_userSetListId] FOREIGN KEY ([userSetListId]) REFERENCES [BMC].[UserSetList] ([id])		-- Foreign key to the UserSetList table.
)
GO

-- Index on the UserSetListChangeHistory table's tenantGuid field.
CREATE INDEX [I_UserSetListChangeHistory_tenantGuid] ON [BMC].[UserSetListChangeHistory] ([tenantGuid])
GO

-- Index on the UserSetListChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_UserSetListChangeHistory_tenantGuid_versionNumber] ON [BMC].[UserSetListChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the UserSetListChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_UserSetListChangeHistory_tenantGuid_timeStamp] ON [BMC].[UserSetListChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the UserSetListChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_UserSetListChangeHistory_tenantGuid_userId] ON [BMC].[UserSetListChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the UserSetListChangeHistory table's tenantGuid,userSetListId fields.
CREATE INDEX [I_UserSetListChangeHistory_tenantGuid_userSetListId] ON [BMC].[UserSetListChangeHistory] ([tenantGuid], [userSetListId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Individual set entries within a user's named set list. Mirrors Rebrickable's setlists/{id}/sets/ endpoint.
CREATE TABLE [BMC].[UserSetListItem]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userSetListId] INT NOT NULL,		-- The set list this item belongs to
	[legoSetId] INT NOT NULL,		-- The set in this list
	[quantity] INT NOT NULL DEFAULT 1,		-- Number of copies of this set in the list
	[includeSpares] BIT NOT NULL DEFAULT 1,		-- Whether to include spare parts from this set in build matching
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_UserSetListItem_UserSetList_userSetListId] FOREIGN KEY ([userSetListId]) REFERENCES [BMC].[UserSetList] ([id]),		-- Foreign key to the UserSetList table.
	CONSTRAINT [FK_UserSetListItem_LegoSet_legoSetId] FOREIGN KEY ([legoSetId]) REFERENCES [BMC].[LegoSet] ([id]),		-- Foreign key to the LegoSet table.
	CONSTRAINT [UC_UserSetListItem_tenantGuid_userSetListId_legoSetId] UNIQUE ( [tenantGuid], [userSetListId], [legoSetId]) 		-- Uniqueness enforced on the UserSetListItem table's tenantGuid and userSetListId and legoSetId fields.
)
GO

-- Index on the UserSetListItem table's tenantGuid field.
CREATE INDEX [I_UserSetListItem_tenantGuid] ON [BMC].[UserSetListItem] ([tenantGuid])
GO

-- Index on the UserSetListItem table's tenantGuid,userSetListId fields.
CREATE INDEX [I_UserSetListItem_tenantGuid_userSetListId] ON [BMC].[UserSetListItem] ([tenantGuid], [userSetListId])
GO

-- Index on the UserSetListItem table's tenantGuid,legoSetId fields.
CREATE INDEX [I_UserSetListItem_tenantGuid_legoSetId] ON [BMC].[UserSetListItem] ([tenantGuid], [legoSetId])
GO

-- Index on the UserSetListItem table's tenantGuid,active fields.
CREATE INDEX [I_UserSetListItem_tenantGuid_active] ON [BMC].[UserSetListItem] ([tenantGuid], [active])
GO

-- Index on the UserSetListItem table's tenantGuid,deleted fields.
CREATE INDEX [I_UserSetListItem_tenantGuid_deleted] ON [BMC].[UserSetListItem] ([tenantGuid], [deleted])
GO


-- Parts lost from sets, mirroring Rebrickable's lost_parts/ endpoint. Tracks which parts are missing from a user's collection.
CREATE TABLE [BMC].[UserLostPart]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[brickPartId] INT NOT NULL,		-- The part that was lost
	[brickColourId] INT NOT NULL,		-- The colour of the lost part
	[legoSetId] INT NULL,		-- The set the part was lost from (null if unknown)
	[lostQuantity] INT NOT NULL,		-- Number of this part+colour that were lost
	[rebrickableInvPartId] INT NULL,		-- Rebrickable inventory_part ID for bidirectional sync (null = BMC-only entry)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_UserLostPart_BrickPart_brickPartId] FOREIGN KEY ([brickPartId]) REFERENCES [BMC].[BrickPart] ([id]),		-- Foreign key to the BrickPart table.
	CONSTRAINT [FK_UserLostPart_BrickColour_brickColourId] FOREIGN KEY ([brickColourId]) REFERENCES [BMC].[BrickColour] ([id]),		-- Foreign key to the BrickColour table.
	CONSTRAINT [FK_UserLostPart_LegoSet_legoSetId] FOREIGN KEY ([legoSetId]) REFERENCES [BMC].[LegoSet] ([id])		-- Foreign key to the LegoSet table.
)
GO

-- Index on the UserLostPart table's tenantGuid field.
CREATE INDEX [I_UserLostPart_tenantGuid] ON [BMC].[UserLostPart] ([tenantGuid])
GO

-- Index on the UserLostPart table's tenantGuid,brickPartId fields.
CREATE INDEX [I_UserLostPart_tenantGuid_brickPartId] ON [BMC].[UserLostPart] ([tenantGuid], [brickPartId])
GO

-- Index on the UserLostPart table's tenantGuid,brickColourId fields.
CREATE INDEX [I_UserLostPart_tenantGuid_brickColourId] ON [BMC].[UserLostPart] ([tenantGuid], [brickColourId])
GO

-- Index on the UserLostPart table's tenantGuid,legoSetId fields.
CREATE INDEX [I_UserLostPart_tenantGuid_legoSetId] ON [BMC].[UserLostPart] ([tenantGuid], [legoSetId])
GO

-- Index on the UserLostPart table's tenantGuid,active fields.
CREATE INDEX [I_UserLostPart_tenantGuid_active] ON [BMC].[UserLostPart] ([tenantGuid], [active])
GO

-- Index on the UserLostPart table's tenantGuid,deleted fields.
CREATE INDEX [I_UserLostPart_tenantGuid_deleted] ON [BMC].[UserLostPart] ([tenantGuid], [deleted])
GO


-- A complete instruction booklet for a building project. A project can have multiple manuals (e.g. one per bag/booklet).
CREATE TABLE [BMC].[BuildManual]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[projectId] INT NOT NULL,		-- The project this manual documents
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[pageWidthMm] REAL NULL,		-- Page width in millimetres for PDF/print layout (e.g. 210 for A4)
	[pageHeightMm] REAL NULL,		-- Page height in millimetres for PDF/print layout (e.g. 297 for A4)
	[isPublished] BIT NOT NULL DEFAULT 0,		-- Whether this manual is marked as published/final
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_BuildManual_Project_projectId] FOREIGN KEY ([projectId]) REFERENCES [BMC].[Project] ([id]),		-- Foreign key to the Project table.
	CONSTRAINT [UC_BuildManual_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the BuildManual table's tenantGuid and name fields.
)
GO

-- Index on the BuildManual table's tenantGuid field.
CREATE INDEX [I_BuildManual_tenantGuid] ON [BMC].[BuildManual] ([tenantGuid])
GO

-- Index on the BuildManual table's tenantGuid,projectId fields.
CREATE INDEX [I_BuildManual_tenantGuid_projectId] ON [BMC].[BuildManual] ([tenantGuid], [projectId])
GO

-- Index on the BuildManual table's tenantGuid,name fields.
CREATE INDEX [I_BuildManual_tenantGuid_name] ON [BMC].[BuildManual] ([tenantGuid], [name])
GO

-- Index on the BuildManual table's tenantGuid,active fields.
CREATE INDEX [I_BuildManual_tenantGuid_active] ON [BMC].[BuildManual] ([tenantGuid], [active])
GO

-- Index on the BuildManual table's tenantGuid,deleted fields.
CREATE INDEX [I_BuildManual_tenantGuid_deleted] ON [BMC].[BuildManual] ([tenantGuid], [deleted])
GO


-- The change history for records from the BuildManual table.
CREATE TABLE [BMC].[BuildManualChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[buildManualId] INT NOT NULL,		-- Link to the BuildManual table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_BuildManualChangeHistory_BuildManual_buildManualId] FOREIGN KEY ([buildManualId]) REFERENCES [BMC].[BuildManual] ([id])		-- Foreign key to the BuildManual table.
)
GO

-- Index on the BuildManualChangeHistory table's tenantGuid field.
CREATE INDEX [I_BuildManualChangeHistory_tenantGuid] ON [BMC].[BuildManualChangeHistory] ([tenantGuid])
GO

-- Index on the BuildManualChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_BuildManualChangeHistory_tenantGuid_versionNumber] ON [BMC].[BuildManualChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the BuildManualChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_BuildManualChangeHistory_tenantGuid_timeStamp] ON [BMC].[BuildManualChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the BuildManualChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_BuildManualChangeHistory_tenantGuid_userId] ON [BMC].[BuildManualChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the BuildManualChangeHistory table's tenantGuid,buildManualId fields.
CREATE INDEX [I_BuildManualChangeHistory_tenantGuid_buildManualId] ON [BMC].[BuildManualChangeHistory] ([tenantGuid], [buildManualId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- A single page within a build manual. Contains one or more build steps.
CREATE TABLE [BMC].[BuildManualPage]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[buildManualId] INT NOT NULL,		-- The manual this page belongs to
	[pageNum] INT NULL,		-- Sequential page number within the manual.  Note purposely not called pageNumber to not clash with code generated parameter
	[title] NVARCHAR(250) NULL,		-- Optional page title (e.g. 'Bag 1', 'Chassis Assembly')
	[notes] NVARCHAR(MAX) NULL,		-- Optional internal notes about this page
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_BuildManualPage_BuildManual_buildManualId] FOREIGN KEY ([buildManualId]) REFERENCES [BMC].[BuildManual] ([id])		-- Foreign key to the BuildManual table.
)
GO

-- Index on the BuildManualPage table's tenantGuid field.
CREATE INDEX [I_BuildManualPage_tenantGuid] ON [BMC].[BuildManualPage] ([tenantGuid])
GO

-- Index on the BuildManualPage table's tenantGuid,buildManualId fields.
CREATE INDEX [I_BuildManualPage_tenantGuid_buildManualId] ON [BMC].[BuildManualPage] ([tenantGuid], [buildManualId])
GO

-- Index on the BuildManualPage table's tenantGuid,active fields.
CREATE INDEX [I_BuildManualPage_tenantGuid_active] ON [BMC].[BuildManualPage] ([tenantGuid], [active])
GO

-- Index on the BuildManualPage table's tenantGuid,deleted fields.
CREATE INDEX [I_BuildManualPage_tenantGuid_deleted] ON [BMC].[BuildManualPage] ([tenantGuid], [deleted])
GO


-- A single build step within a manual page. Defines the camera angle and display options for that step's rendered view.
CREATE TABLE [BMC].[BuildManualStep]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[buildManualPageId] INT NOT NULL,		-- The page this step appears on
	[stepNumber] INT NULL,		-- Sequential step number within the page
	[cameraPositionX] REAL NULL,		-- Camera X position for this step's rendered view
	[cameraPositionY] REAL NULL,		-- Camera Y position for this step's rendered view
	[cameraPositionZ] REAL NULL,		-- Camera Z position for this step's rendered view
	[cameraTargetX] REAL NULL,		-- Camera look-at target X for this step
	[cameraTargetY] REAL NULL,		-- Camera look-at target Y for this step
	[cameraTargetZ] REAL NULL,		-- Camera look-at target Z for this step
	[cameraZoom] REAL NULL,		-- Camera zoom / field of view for this step
	[showExplodedView] BIT NOT NULL DEFAULT 0,		-- Whether to render the step with newly-added parts pulled apart for clarity
	[explodedDistance] REAL NULL,		-- Distance in LDU to pull apart exploded parts (null = use default)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_BuildManualStep_BuildManualPage_buildManualPageId] FOREIGN KEY ([buildManualPageId]) REFERENCES [BMC].[BuildManualPage] ([id])		-- Foreign key to the BuildManualPage table.
)
GO

-- Index on the BuildManualStep table's tenantGuid field.
CREATE INDEX [I_BuildManualStep_tenantGuid] ON [BMC].[BuildManualStep] ([tenantGuid])
GO

-- Index on the BuildManualStep table's tenantGuid,buildManualPageId fields.
CREATE INDEX [I_BuildManualStep_tenantGuid_buildManualPageId] ON [BMC].[BuildManualStep] ([tenantGuid], [buildManualPageId])
GO

-- Index on the BuildManualStep table's tenantGuid,active fields.
CREATE INDEX [I_BuildManualStep_tenantGuid_active] ON [BMC].[BuildManualStep] ([tenantGuid], [active])
GO

-- Index on the BuildManualStep table's tenantGuid,deleted fields.
CREATE INDEX [I_BuildManualStep_tenantGuid_deleted] ON [BMC].[BuildManualStep] ([tenantGuid], [deleted])
GO


-- Maps which placed bricks are added during a specific build step. Links to the actual PlacedBrick in the project.
CREATE TABLE [BMC].[BuildStepPart]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[buildManualStepId] INT NOT NULL,		-- The build step this part is added during
	[placedBrickId] INT NOT NULL,		-- The placed brick in the project that is added in this step
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_BuildStepPart_BuildManualStep_buildManualStepId] FOREIGN KEY ([buildManualStepId]) REFERENCES [BMC].[BuildManualStep] ([id]),		-- Foreign key to the BuildManualStep table.
	CONSTRAINT [FK_BuildStepPart_PlacedBrick_placedBrickId] FOREIGN KEY ([placedBrickId]) REFERENCES [BMC].[PlacedBrick] ([id])		-- Foreign key to the PlacedBrick table.
)
GO

-- Index on the BuildStepPart table's tenantGuid field.
CREATE INDEX [I_BuildStepPart_tenantGuid] ON [BMC].[BuildStepPart] ([tenantGuid])
GO

-- Index on the BuildStepPart table's tenantGuid,buildManualStepId fields.
CREATE INDEX [I_BuildStepPart_tenantGuid_buildManualStepId] ON [BMC].[BuildStepPart] ([tenantGuid], [buildManualStepId])
GO

-- Index on the BuildStepPart table's tenantGuid,placedBrickId fields.
CREATE INDEX [I_BuildStepPart_tenantGuid_placedBrickId] ON [BMC].[BuildStepPart] ([tenantGuid], [placedBrickId])
GO

-- Index on the BuildStepPart table's tenantGuid,active fields.
CREATE INDEX [I_BuildStepPart_tenantGuid_active] ON [BMC].[BuildStepPart] ([tenantGuid], [active])
GO

-- Index on the BuildStepPart table's tenantGuid,deleted fields.
CREATE INDEX [I_BuildStepPart_tenantGuid_deleted] ON [BMC].[BuildStepPart] ([tenantGuid], [deleted])
GO


-- Lookup table of annotation types available for build steps (Arrow, Callout, Label, Quantity Callout, Submodel Callout).
CREATE TABLE [BMC].[BuildStepAnnotationType]
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

-- Index on the BuildStepAnnotationType table's name field.
CREATE INDEX [I_BuildStepAnnotationType_name] ON [BMC].[BuildStepAnnotationType] ([name])
GO

-- Index on the BuildStepAnnotationType table's active field.
CREATE INDEX [I_BuildStepAnnotationType_active] ON [BMC].[BuildStepAnnotationType] ([active])
GO

-- Index on the BuildStepAnnotationType table's deleted field.
CREATE INDEX [I_BuildStepAnnotationType_deleted] ON [BMC].[BuildStepAnnotationType] ([deleted])
GO

INSERT INTO [BMC].[BuildStepAnnotationType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Arrow', 'Directional arrow indicating placement direction or connection point', 1, 'ba100001-0001-4000-8000-000000000001' )
GO

INSERT INTO [BMC].[BuildStepAnnotationType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Callout', 'Callout box highlighting a sub-assembly built separately', 2, 'ba100001-0001-4000-8000-000000000002' )
GO

INSERT INTO [BMC].[BuildStepAnnotationType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Label', 'Text label providing additional context or instruction', 3, 'ba100001-0001-4000-8000-000000000003' )
GO

INSERT INTO [BMC].[BuildStepAnnotationType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Quantity Callout', 'Quantity indicator showing how many of a part are needed in this step', 4, 'ba100001-0001-4000-8000-000000000004' )
GO

INSERT INTO [BMC].[BuildStepAnnotationType] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Submodel Callout', 'Callout referencing a submodel that should be built as part of this step', 5, 'ba100001-0001-4000-8000-000000000005' )
GO


-- Visual annotations (arrows, callouts, labels) placed on a build step's rendered view.
CREATE TABLE [BMC].[BuildStepAnnotation]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[buildManualStepId] INT NOT NULL,		-- The build step this annotation belongs to
	[buildStepAnnotationTypeId] INT NOT NULL,		-- The type of annotation (Arrow, Callout, Label, etc.)
	[positionX] REAL NULL,		-- X position on the rendered page (normalised 0-1 or pixel coordinates)
	[positionY] REAL NULL,		-- Y position on the rendered page
	[width] REAL NULL,		-- Width of the annotation element (null = auto-size)
	[height] REAL NULL,		-- Height of the annotation element (null = auto-size)
	[text] NVARCHAR(MAX) NULL,		-- Optional text content for labels and callouts
	[placedBrickId] INT NULL,		-- Optional target placed brick that this annotation points to or highlights
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_BuildStepAnnotation_BuildManualStep_buildManualStepId] FOREIGN KEY ([buildManualStepId]) REFERENCES [BMC].[BuildManualStep] ([id]),		-- Foreign key to the BuildManualStep table.
	CONSTRAINT [FK_BuildStepAnnotation_BuildStepAnnotationType_buildStepAnnotationTypeId] FOREIGN KEY ([buildStepAnnotationTypeId]) REFERENCES [BMC].[BuildStepAnnotationType] ([id]),		-- Foreign key to the BuildStepAnnotationType table.
	CONSTRAINT [FK_BuildStepAnnotation_PlacedBrick_placedBrickId] FOREIGN KEY ([placedBrickId]) REFERENCES [BMC].[PlacedBrick] ([id])		-- Foreign key to the PlacedBrick table.
)
GO

-- Index on the BuildStepAnnotation table's tenantGuid field.
CREATE INDEX [I_BuildStepAnnotation_tenantGuid] ON [BMC].[BuildStepAnnotation] ([tenantGuid])
GO

-- Index on the BuildStepAnnotation table's tenantGuid,buildManualStepId fields.
CREATE INDEX [I_BuildStepAnnotation_tenantGuid_buildManualStepId] ON [BMC].[BuildStepAnnotation] ([tenantGuid], [buildManualStepId])
GO

-- Index on the BuildStepAnnotation table's tenantGuid,buildStepAnnotationTypeId fields.
CREATE INDEX [I_BuildStepAnnotation_tenantGuid_buildStepAnnotationTypeId] ON [BMC].[BuildStepAnnotation] ([tenantGuid], [buildStepAnnotationTypeId])
GO

-- Index on the BuildStepAnnotation table's tenantGuid,placedBrickId fields.
CREATE INDEX [I_BuildStepAnnotation_tenantGuid_placedBrickId] ON [BMC].[BuildStepAnnotation] ([tenantGuid], [placedBrickId])
GO

-- Index on the BuildStepAnnotation table's tenantGuid,active fields.
CREATE INDEX [I_BuildStepAnnotation_tenantGuid_active] ON [BMC].[BuildStepAnnotation] ([tenantGuid], [active])
GO

-- Index on the BuildStepAnnotation table's tenantGuid,deleted fields.
CREATE INDEX [I_BuildStepAnnotation_tenantGuid_deleted] ON [BMC].[BuildStepAnnotation] ([tenantGuid], [deleted])
GO


-- Reusable rendering presets that define resolution, lighting, and quality settings for producing images of models.
CREATE TABLE [BMC].[RenderPreset]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(500) NOT NULL,
	[resolutionWidth] INT NULL,		-- Output image width in pixels
	[resolutionHeight] INT NULL,		-- Output image height in pixels
	[backgroundColorHex] NVARCHAR(10) NULL,		-- Background colour in hex (e.g. #FFFFFF for white, #000000 for black)
	[enableShadows] BIT NOT NULL DEFAULT 1,		-- Whether to render drop shadows
	[enableReflections] BIT NOT NULL DEFAULT 0,		-- Whether to render environment reflections on metallic/chrome parts
	[lightingMode] NVARCHAR(100) NULL,		-- Lighting preset name: studio, outdoor, dramatic, custom
	[antiAliasLevel] INT NULL,		-- Anti-aliasing level (1=none, 2=2x, 4=4x, 8=8x)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_RenderPreset_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the RenderPreset table's tenantGuid and name fields.
)
GO

-- Index on the RenderPreset table's tenantGuid field.
CREATE INDEX [I_RenderPreset_tenantGuid] ON [BMC].[RenderPreset] ([tenantGuid])
GO

-- Index on the RenderPreset table's tenantGuid,name fields.
CREATE INDEX [I_RenderPreset_tenantGuid_name] ON [BMC].[RenderPreset] ([tenantGuid], [name])
GO

-- Index on the RenderPreset table's tenantGuid,active fields.
CREATE INDEX [I_RenderPreset_tenantGuid_active] ON [BMC].[RenderPreset] ([tenantGuid], [active])
GO

-- Index on the RenderPreset table's tenantGuid,deleted fields.
CREATE INDEX [I_RenderPreset_tenantGuid_deleted] ON [BMC].[RenderPreset] ([tenantGuid], [deleted])
GO


-- Records of rendered images produced from a project, with the preset used and output metadata.
CREATE TABLE [BMC].[ProjectRender]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[projectId] INT NOT NULL,		-- The project this render was produced from
	[renderPresetId] INT NULL,		-- The render preset used (null = custom/one-off settings)
	[name] NVARCHAR(100) NOT NULL,
	[outputFilePath] NVARCHAR(250) NULL,		-- Relative path to the rendered image file
	[resolutionWidth] INT NULL,		-- Actual output width in pixels
	[resolutionHeight] INT NULL,		-- Actual output height in pixels
	[renderedDate] DATETIME2(7) NULL,		-- Date/time the render was produced
	[renderDurationSeconds] REAL NULL,		-- Time taken to produce the render in seconds
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ProjectRender_Project_projectId] FOREIGN KEY ([projectId]) REFERENCES [BMC].[Project] ([id]),		-- Foreign key to the Project table.
	CONSTRAINT [FK_ProjectRender_RenderPreset_renderPresetId] FOREIGN KEY ([renderPresetId]) REFERENCES [BMC].[RenderPreset] ([id]),		-- Foreign key to the RenderPreset table.
	CONSTRAINT [UC_ProjectRender_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the ProjectRender table's tenantGuid and name fields.
)
GO

-- Index on the ProjectRender table's tenantGuid field.
CREATE INDEX [I_ProjectRender_tenantGuid] ON [BMC].[ProjectRender] ([tenantGuid])
GO

-- Index on the ProjectRender table's tenantGuid,projectId fields.
CREATE INDEX [I_ProjectRender_tenantGuid_projectId] ON [BMC].[ProjectRender] ([tenantGuid], [projectId])
GO

-- Index on the ProjectRender table's tenantGuid,renderPresetId fields.
CREATE INDEX [I_ProjectRender_tenantGuid_renderPresetId] ON [BMC].[ProjectRender] ([tenantGuid], [renderPresetId])
GO

-- Index on the ProjectRender table's tenantGuid,name fields.
CREATE INDEX [I_ProjectRender_tenantGuid_name] ON [BMC].[ProjectRender] ([tenantGuid], [name])
GO

-- Index on the ProjectRender table's tenantGuid,active fields.
CREATE INDEX [I_ProjectRender_tenantGuid_active] ON [BMC].[ProjectRender] ([tenantGuid], [active])
GO

-- Index on the ProjectRender table's tenantGuid,deleted fields.
CREATE INDEX [I_ProjectRender_tenantGuid_deleted] ON [BMC].[ProjectRender] ([tenantGuid], [deleted])
GO


-- Lookup table of supported file export formats for models, instructions, and parts lists.
CREATE TABLE [BMC].[ExportFormat]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[fileExtension] NVARCHAR(50) NULL,		-- File extension including dot (e.g. '.ldr', '.pdf', '.xml')
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the ExportFormat table's name field.
CREATE INDEX [I_ExportFormat_name] ON [BMC].[ExportFormat] ([name])
GO

-- Index on the ExportFormat table's active field.
CREATE INDEX [I_ExportFormat_active] ON [BMC].[ExportFormat] ([active])
GO

-- Index on the ExportFormat table's deleted field.
CREATE INDEX [I_ExportFormat_deleted] ON [BMC].[ExportFormat] ([deleted])
GO

INSERT INTO [BMC].[ExportFormat] ( [name], [description], [fileExtension], [sequence], [objectGuid] ) VALUES  ( 'LDraw', 'LDraw single-model file format', '.ldr', 1, 'ef100001-0001-4000-8000-000000000001' )
GO

INSERT INTO [BMC].[ExportFormat] ( [name], [description], [fileExtension], [sequence], [objectGuid] ) VALUES  ( 'LDraw Multi-Part', 'LDraw multi-part document containing submodels', '.mpd', 2, 'ef100001-0001-4000-8000-000000000002' )
GO

INSERT INTO [BMC].[ExportFormat] ( [name], [description], [fileExtension], [sequence], [objectGuid] ) VALUES  ( 'Wavefront OBJ', 'Wavefront OBJ 3D model format for Blender and other 3D tools', '.obj', 3, 'ef100001-0001-4000-8000-000000000003' )
GO

INSERT INTO [BMC].[ExportFormat] ( [name], [description], [fileExtension], [sequence], [objectGuid] ) VALUES  ( 'COLLADA', 'COLLADA 3D asset exchange format', '.dae', 4, 'ef100001-0001-4000-8000-000000000004' )
GO

INSERT INTO [BMC].[ExportFormat] ( [name], [description], [fileExtension], [sequence], [objectGuid] ) VALUES  ( 'BrickLink XML', 'BrickLink wanted-list XML format for ordering parts', '.xml', 5, 'ef100001-0001-4000-8000-000000000005' )
GO

INSERT INTO [BMC].[ExportFormat] ( [name], [description], [fileExtension], [sequence], [objectGuid] ) VALUES  ( 'Rebrickable CSV', 'Rebrickable-compatible CSV parts list', '.csv', 6, 'ef100001-0001-4000-8000-000000000006' )
GO

INSERT INTO [BMC].[ExportFormat] ( [name], [description], [fileExtension], [sequence], [objectGuid] ) VALUES  ( 'PDF Instructions', 'PDF export of build manual instructions', '.pdf', 7, 'ef100001-0001-4000-8000-000000000007' )
GO

INSERT INTO [BMC].[ExportFormat] ( [name], [description], [fileExtension], [sequence], [objectGuid] ) VALUES  ( 'BrickLink Studio', 'BrickLink Studio project file format', '.io', 8, 'ef100001-0001-4000-8000-000000000008' )
GO

INSERT INTO [BMC].[ExportFormat] ( [name], [description], [fileExtension], [sequence], [objectGuid] ) VALUES  ( 'STL', 'Stereolithography format for 3D printing', '.stl', 9, 'ef100001-0001-4000-8000-000000000009' )
GO


-- Log of exports produced from a project, tracking what was exported and when.
CREATE TABLE [BMC].[ProjectExport]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[projectId] INT NOT NULL,		-- The project this export was produced from
	[exportFormatId] INT NOT NULL,		-- The format used for the export
	[name] NVARCHAR(100) NOT NULL,
	[outputFilePath] NVARCHAR(250) NULL,		-- Relative path to the exported file
	[exportedDate] DATETIME2(7) NULL,		-- Date/time the export was produced
	[includeInstructions] BIT NOT NULL DEFAULT 0,		-- Whether build instructions were included in the export
	[includePartsList] BIT NOT NULL DEFAULT 0,		-- Whether a bill of materials / parts list was included in the export
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ProjectExport_Project_projectId] FOREIGN KEY ([projectId]) REFERENCES [BMC].[Project] ([id]),		-- Foreign key to the Project table.
	CONSTRAINT [FK_ProjectExport_ExportFormat_exportFormatId] FOREIGN KEY ([exportFormatId]) REFERENCES [BMC].[ExportFormat] ([id]),		-- Foreign key to the ExportFormat table.
	CONSTRAINT [UC_ProjectExport_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the ProjectExport table's tenantGuid and name fields.
)
GO

-- Index on the ProjectExport table's tenantGuid field.
CREATE INDEX [I_ProjectExport_tenantGuid] ON [BMC].[ProjectExport] ([tenantGuid])
GO

-- Index on the ProjectExport table's tenantGuid,projectId fields.
CREATE INDEX [I_ProjectExport_tenantGuid_projectId] ON [BMC].[ProjectExport] ([tenantGuid], [projectId])
GO

-- Index on the ProjectExport table's tenantGuid,exportFormatId fields.
CREATE INDEX [I_ProjectExport_tenantGuid_exportFormatId] ON [BMC].[ProjectExport] ([tenantGuid], [exportFormatId])
GO

-- Index on the ProjectExport table's tenantGuid,name fields.
CREATE INDEX [I_ProjectExport_tenantGuid_name] ON [BMC].[ProjectExport] ([tenantGuid], [name])
GO

-- Index on the ProjectExport table's tenantGuid,active fields.
CREATE INDEX [I_ProjectExport_tenantGuid_active] ON [BMC].[ProjectExport] ([tenantGuid], [active])
GO

-- Index on the ProjectExport table's tenantGuid,deleted fields.
CREATE INDEX [I_ProjectExport_tenantGuid_deleted] ON [BMC].[ProjectExport] ([tenantGuid], [deleted])
GO


-- Public builder profile for community features. One profile per tenant (user). Decoupled from Foundation user/tenant tables to keep BMC concerns independent.
CREATE TABLE [BMC].[UserProfile]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[displayName] NVARCHAR(100) NOT NULL,		-- Public display name shown in the community (distinct from auth username)
	[bio] NVARCHAR(MAX) NULL,		-- Free-form biography / about-me text
	[location] NVARCHAR(100) NULL,		-- User's declared location (city, country, or free-form)
	[avatarFileName] NVARCHAR(250) NULL,		-- Part of the binary data field setup
	[avatarSize] BIGINT NULL,		-- Part of the binary data field setup
	[avatarData] VARBINARY(MAX) NULL,		-- Part of the binary data field setup
	[avatarMimeType] NVARCHAR(100) NULL,		-- Part of the binary data field setup
	[bannerFileName] NVARCHAR(250) NULL,		-- Part of the binary data field setup
	[bannerSize] BIGINT NULL,		-- Part of the binary data field setup
	[bannerData] VARBINARY(MAX) NULL,		-- Part of the binary data field setup
	[bannerMimeType] NVARCHAR(100) NULL,		-- Part of the binary data field setup
	[websiteUrl] NVARCHAR(250) NULL,		-- Optional personal website or portfolio URL
	[isPublic] BIT NOT NULL DEFAULT 1,		-- Whether this profile is visible to unauthenticated visitors
	[memberSinceDate] DATETIME2(7) NULL,		-- Date the user first created their profile (for display purposes)
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the UserProfile table's tenantGuid field.
CREATE INDEX [I_UserProfile_tenantGuid] ON [BMC].[UserProfile] ([tenantGuid])
GO

-- Index on the UserProfile table's tenantGuid,active fields.
CREATE INDEX [I_UserProfile_tenantGuid_active] ON [BMC].[UserProfile] ([tenantGuid], [active])
GO

-- Index on the UserProfile table's tenantGuid,deleted fields.
CREATE INDEX [I_UserProfile_tenantGuid_deleted] ON [BMC].[UserProfile] ([tenantGuid], [deleted])
GO


-- The change history for records from the UserProfile table.
CREATE TABLE [BMC].[UserProfileChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userProfileId] INT NOT NULL,		-- Link to the UserProfile table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_UserProfileChangeHistory_UserProfile_userProfileId] FOREIGN KEY ([userProfileId]) REFERENCES [BMC].[UserProfile] ([id])		-- Foreign key to the UserProfile table.
)
GO

-- Index on the UserProfileChangeHistory table's tenantGuid field.
CREATE INDEX [I_UserProfileChangeHistory_tenantGuid] ON [BMC].[UserProfileChangeHistory] ([tenantGuid])
GO

-- Index on the UserProfileChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_UserProfileChangeHistory_tenantGuid_versionNumber] ON [BMC].[UserProfileChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the UserProfileChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_UserProfileChangeHistory_tenantGuid_timeStamp] ON [BMC].[UserProfileChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the UserProfileChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_UserProfileChangeHistory_tenantGuid_userId] ON [BMC].[UserProfileChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the UserProfileChangeHistory table's tenantGuid,userProfileId fields.
CREATE INDEX [I_UserProfileChangeHistory_tenantGuid_userProfileId] ON [BMC].[UserProfileChangeHistory] ([tenantGuid], [userProfileId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Lookup table of external link types a user can add to their profile (e.g. BrickLink Store, Flickr, YouTube, Instagram, Rebrickable).
CREATE TABLE [BMC].[UserProfileLinkType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[iconCssClass] NVARCHAR(100) NULL,		-- CSS class for the link type icon (e.g. 'fab fa-youtube')
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the UserProfileLinkType table's name field.
CREATE INDEX [I_UserProfileLinkType_name] ON [BMC].[UserProfileLinkType] ([name])
GO

-- Index on the UserProfileLinkType table's active field.
CREATE INDEX [I_UserProfileLinkType_active] ON [BMC].[UserProfileLinkType] ([active])
GO

-- Index on the UserProfileLinkType table's deleted field.
CREATE INDEX [I_UserProfileLinkType_deleted] ON [BMC].[UserProfileLinkType] ([deleted])
GO

INSERT INTO [BMC].[UserProfileLinkType] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'BrickLink Store', 'Link to the user''s BrickLink seller store', 'fas fa-store', 1, 'a0100001-0001-4000-8000-000000000001' )
GO

INSERT INTO [BMC].[UserProfileLinkType] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'Rebrickable', 'Link to the user''s Rebrickable profile', 'fas fa-cubes', 2, 'a0100001-0001-4000-8000-000000000002' )
GO

INSERT INTO [BMC].[UserProfileLinkType] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'Flickr', 'Link to the user''s Flickr photostream', 'fab fa-flickr', 3, 'a0100001-0001-4000-8000-000000000003' )
GO

INSERT INTO [BMC].[UserProfileLinkType] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'YouTube', 'Link to the user''s YouTube channel', 'fab fa-youtube', 4, 'a0100001-0001-4000-8000-000000000004' )
GO

INSERT INTO [BMC].[UserProfileLinkType] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'Instagram', 'Link to the user''s Instagram profile', 'fab fa-instagram', 5, 'a0100001-0001-4000-8000-000000000005' )
GO

INSERT INTO [BMC].[UserProfileLinkType] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'Personal Website', 'Link to the user''s personal website or blog', 'fas fa-globe', 6, 'a0100001-0001-4000-8000-000000000006' )
GO

INSERT INTO [BMC].[UserProfileLinkType] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'Eurobricks', 'Link to the user''s Eurobricks forum profile', 'fas fa-comments', 7, 'a0100001-0001-4000-8000-000000000007' )
GO


-- External links displayed on a user's public profile (BrickLink store, Flickr, YouTube, etc.).
CREATE TABLE [BMC].[UserProfileLink]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userProfileId] INT NOT NULL,		-- The profile this link belongs to
	[userProfileLinkTypeId] INT NOT NULL,		-- The type of link (BrickLink, YouTube, etc.)
	[url] NVARCHAR(500) NOT NULL,		-- The full URL to the external resource
	[displayLabel] NVARCHAR(100) NULL,		-- Optional custom label to display instead of the URL (e.g. 'My BL Store')
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_UserProfileLink_UserProfile_userProfileId] FOREIGN KEY ([userProfileId]) REFERENCES [BMC].[UserProfile] ([id]),		-- Foreign key to the UserProfile table.
	CONSTRAINT [FK_UserProfileLink_UserProfileLinkType_userProfileLinkTypeId] FOREIGN KEY ([userProfileLinkTypeId]) REFERENCES [BMC].[UserProfileLinkType] ([id])		-- Foreign key to the UserProfileLinkType table.
)
GO

-- Index on the UserProfileLink table's tenantGuid field.
CREATE INDEX [I_UserProfileLink_tenantGuid] ON [BMC].[UserProfileLink] ([tenantGuid])
GO

-- Index on the UserProfileLink table's tenantGuid,userProfileId fields.
CREATE INDEX [I_UserProfileLink_tenantGuid_userProfileId] ON [BMC].[UserProfileLink] ([tenantGuid], [userProfileId])
GO

-- Index on the UserProfileLink table's tenantGuid,userProfileLinkTypeId fields.
CREATE INDEX [I_UserProfileLink_tenantGuid_userProfileLinkTypeId] ON [BMC].[UserProfileLink] ([tenantGuid], [userProfileLinkTypeId])
GO

-- Index on the UserProfileLink table's tenantGuid,active fields.
CREATE INDEX [I_UserProfileLink_tenantGuid_active] ON [BMC].[UserProfileLink] ([tenantGuid], [active])
GO

-- Index on the UserProfileLink table's tenantGuid,deleted fields.
CREATE INDEX [I_UserProfileLink_tenantGuid_deleted] ON [BMC].[UserProfileLink] ([tenantGuid], [deleted])
GO


-- Junction table linking a user profile to their preferred LEGO themes (e.g. Star Wars, Technic, City). Used to personalise the experience and display theme interests on the public profile.
CREATE TABLE [BMC].[UserProfilePreferredTheme]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userProfileId] INT NOT NULL,		-- The profile this preference belongs to
	[legoThemeId] INT NOT NULL,		-- The LEGO theme the user prefers
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_UserProfilePreferredTheme_UserProfile_userProfileId] FOREIGN KEY ([userProfileId]) REFERENCES [BMC].[UserProfile] ([id]),		-- Foreign key to the UserProfile table.
	CONSTRAINT [FK_UserProfilePreferredTheme_LegoTheme_legoThemeId] FOREIGN KEY ([legoThemeId]) REFERENCES [BMC].[LegoTheme] ([id]),		-- Foreign key to the LegoTheme table.
	CONSTRAINT [UC_UserProfilePreferredTheme_tenantGuid_userProfileId_legoThemeId] UNIQUE ( [tenantGuid], [userProfileId], [legoThemeId]) 		-- Uniqueness enforced on the UserProfilePreferredTheme table's tenantGuid and userProfileId and legoThemeId fields.
)
GO

-- Index on the UserProfilePreferredTheme table's tenantGuid field.
CREATE INDEX [I_UserProfilePreferredTheme_tenantGuid] ON [BMC].[UserProfilePreferredTheme] ([tenantGuid])
GO

-- Index on the UserProfilePreferredTheme table's tenantGuid,userProfileId fields.
CREATE INDEX [I_UserProfilePreferredTheme_tenantGuid_userProfileId] ON [BMC].[UserProfilePreferredTheme] ([tenantGuid], [userProfileId])
GO

-- Index on the UserProfilePreferredTheme table's tenantGuid,legoThemeId fields.
CREATE INDEX [I_UserProfilePreferredTheme_tenantGuid_legoThemeId] ON [BMC].[UserProfilePreferredTheme] ([tenantGuid], [legoThemeId])
GO

-- Index on the UserProfilePreferredTheme table's tenantGuid,active fields.
CREATE INDEX [I_UserProfilePreferredTheme_tenantGuid_active] ON [BMC].[UserProfilePreferredTheme] ([tenantGuid], [active])
GO

-- Index on the UserProfilePreferredTheme table's tenantGuid,deleted fields.
CREATE INDEX [I_UserProfilePreferredTheme_tenantGuid_deleted] ON [BMC].[UserProfilePreferredTheme] ([tenantGuid], [deleted])
GO


-- Tracks a user's relationship with official LEGO sets for their collector showcase. Distinct from UserCollectionSetImport which tracks parts inventory.
CREATE TABLE [BMC].[UserSetOwnership]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[legoSetId] INT NOT NULL,		-- The official LEGO set
	[status] NVARCHAR(50) NOT NULL,		-- Ownership status: Owned, Built, Wanted, WishList, ForDisplay, ForSale
	[acquiredDate] DATETIME2(7) NULL,		-- Date the user acquired this set (null if unknown or wanted)
	[personalRating] INT NULL,		-- User's personal rating of the set (1-5 stars, null if not rated)
	[notes] NVARCHAR(MAX) NULL,		-- Free-form notes about this set (e.g. condition, where purchased, modifications)
	[quantity] INT NOT NULL DEFAULT 1,		-- Number of copies owned
	[isPublic] BIT NOT NULL DEFAULT 1,		-- Whether this ownership record is visible on the user's public profile
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_UserSetOwnership_LegoSet_legoSetId] FOREIGN KEY ([legoSetId]) REFERENCES [BMC].[LegoSet] ([id]),		-- Foreign key to the LegoSet table.
	CONSTRAINT [UC_UserSetOwnership_tenantGuid_legoSetId] UNIQUE ( [tenantGuid], [legoSetId]) 		-- Uniqueness enforced on the UserSetOwnership table's tenantGuid and legoSetId fields.
)
GO

-- Index on the UserSetOwnership table's tenantGuid field.
CREATE INDEX [I_UserSetOwnership_tenantGuid] ON [BMC].[UserSetOwnership] ([tenantGuid])
GO

-- Index on the UserSetOwnership table's tenantGuid,legoSetId fields.
CREATE INDEX [I_UserSetOwnership_tenantGuid_legoSetId] ON [BMC].[UserSetOwnership] ([tenantGuid], [legoSetId])
GO

-- Index on the UserSetOwnership table's tenantGuid,active fields.
CREATE INDEX [I_UserSetOwnership_tenantGuid_active] ON [BMC].[UserSetOwnership] ([tenantGuid], [active])
GO

-- Index on the UserSetOwnership table's tenantGuid,deleted fields.
CREATE INDEX [I_UserSetOwnership_tenantGuid_deleted] ON [BMC].[UserSetOwnership] ([tenantGuid], [deleted])
GO


-- Cached aggregate statistics for a user's profile. Periodically recalculated by background worker to avoid expensive real-time queries.
CREATE TABLE [BMC].[UserProfileStat]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userProfileId] INT NOT NULL,		-- The profile these stats belong to
	[totalPartsOwned] INT NOT NULL DEFAULT 0,		-- Total number of individual parts across all collections
	[totalUniquePartsOwned] INT NOT NULL DEFAULT 0,		-- Total number of unique part+colour combinations owned
	[totalSetsOwned] INT NOT NULL DEFAULT 0,		-- Total number of sets with Owned or Built status
	[totalMocsPublished] INT NOT NULL DEFAULT 0,		-- Total number of MOCs published to the gallery
	[totalFollowers] INT NOT NULL DEFAULT 0,		-- Number of users following this profile
	[totalFollowing] INT NOT NULL DEFAULT 0,		-- Number of users this profile is following
	[totalLikesReceived] INT NOT NULL DEFAULT 0,		-- Total likes received across all published MOCs
	[totalAchievementPoints] INT NOT NULL DEFAULT 0,		-- Sum of achievement point values earned
	[lastCalculatedDate] DATETIME2(7) NULL,		-- When these stats were last recalculated by the background worker
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_UserProfileStat_UserProfile_userProfileId] FOREIGN KEY ([userProfileId]) REFERENCES [BMC].[UserProfile] ([id])		-- Foreign key to the UserProfile table.
)
GO

-- Index on the UserProfileStat table's tenantGuid field.
CREATE INDEX [I_UserProfileStat_tenantGuid] ON [BMC].[UserProfileStat] ([tenantGuid])
GO

-- Index on the UserProfileStat table's tenantGuid,userProfileId fields.
CREATE INDEX [I_UserProfileStat_tenantGuid_userProfileId] ON [BMC].[UserProfileStat] ([tenantGuid], [userProfileId])
GO

-- Index on the UserProfileStat table's tenantGuid,active fields.
CREATE INDEX [I_UserProfileStat_tenantGuid_active] ON [BMC].[UserProfileStat] ([tenantGuid], [active])
GO

-- Index on the UserProfileStat table's tenantGuid,deleted fields.
CREATE INDEX [I_UserProfileStat_tenantGuid_deleted] ON [BMC].[UserProfileStat] ([tenantGuid], [deleted])
GO


-- Follow relationships between users. A follower subscribes to activity updates from the followed user.
CREATE TABLE [BMC].[UserFollow]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[followerTenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- Tenant GUID of the user who is following
	[followedTenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- Tenant GUID of the user being followed
	[followedDate] DATETIME2(7) NOT NULL,		-- Date/time the follow relationship was created
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_UserFollow_followerTenantGuid_followedTenantGuid] UNIQUE ( [followerTenantGuid], [followedTenantGuid]) 		-- Uniqueness enforced on the UserFollow table's followerTenantGuid and followedTenantGuid fields.
)
GO

-- Index on the UserFollow table's active field.
CREATE INDEX [I_UserFollow_active] ON [BMC].[UserFollow] ([active])
GO

-- Index on the UserFollow table's deleted field.
CREATE INDEX [I_UserFollow_deleted] ON [BMC].[UserFollow] ([deleted])
GO


-- Lookup table of activity event types that appear in users' activity feeds.
CREATE TABLE [BMC].[ActivityEventType]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[iconCssClass] NVARCHAR(100) NULL,		-- CSS class for the event type icon in the activity feed
	[accentColor] NVARCHAR(10) NULL,		-- Optional accent colour for this event type in the feed
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the ActivityEventType table's name field.
CREATE INDEX [I_ActivityEventType_name] ON [BMC].[ActivityEventType] ([name])
GO

-- Index on the ActivityEventType table's active field.
CREATE INDEX [I_ActivityEventType_active] ON [BMC].[ActivityEventType] ([active])
GO

-- Index on the ActivityEventType table's deleted field.
CREATE INDEX [I_ActivityEventType_deleted] ON [BMC].[ActivityEventType] ([deleted])
GO

INSERT INTO [BMC].[ActivityEventType] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'PublishedMoc', 'User published a MOC to the gallery', 'fas fa-rocket', 1, 'ae100001-0001-4000-8000-000000000001' )
GO

INSERT INTO [BMC].[ActivityEventType] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'AddedSet', 'User added a set to their collection', 'fas fa-box-open', 2, 'ae100001-0001-4000-8000-000000000002' )
GO

INSERT INTO [BMC].[ActivityEventType] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'EarnedAchievement', 'User earned an achievement', 'fas fa-trophy', 3, 'ae100001-0001-4000-8000-000000000003' )
GO

INSERT INTO [BMC].[ActivityEventType] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'JoinedChallenge', 'User submitted an entry to a build challenge', 'fas fa-flag-checkered', 4, 'ae100001-0001-4000-8000-000000000004' )
GO

INSERT INTO [BMC].[ActivityEventType] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'SharedInstruction', 'User published build instructions', 'fas fa-book', 5, 'ae100001-0001-4000-8000-000000000005' )
GO

INSERT INTO [BMC].[ActivityEventType] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'CollectionMilestone', 'User reached a collection milestone', 'fas fa-gem', 6, 'ae100001-0001-4000-8000-000000000006' )
GO

INSERT INTO [BMC].[ActivityEventType] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'FollowedUser', 'User followed another builder', 'fas fa-user-plus', 7, 'ae100001-0001-4000-8000-000000000007' )
GO


-- Individual activity feed events generated by user actions. Used to build the community activity feed and individual user activity histories.
CREATE TABLE [BMC].[ActivityEvent]
(
	[id] BIGINT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[activityEventTypeId] INT NOT NULL,		-- The type of activity event
	[title] NVARCHAR(250) NOT NULL,		-- Short display title for the event (e.g. 'Published Technic Crane MOC')
	[description] NVARCHAR(MAX) NULL,		-- Optional longer description or context for the event
	[relatedEntityType] NVARCHAR(100) NULL,		-- Type name of the related entity (e.g. 'PublishedMoc', 'LegoSet', 'Achievement')
	[relatedEntityId] BIGINT NULL,		-- ID of the related entity for deep linking (null if not applicable)
	[eventDate] DATETIME2(7) NOT NULL,		-- Date/time the activity occurred
	[isPublic] BIT NOT NULL DEFAULT 1,		-- Whether this event is visible on the public activity feed
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ActivityEvent_ActivityEventType_activityEventTypeId] FOREIGN KEY ([activityEventTypeId]) REFERENCES [BMC].[ActivityEventType] ([id])		-- Foreign key to the ActivityEventType table.
)
GO

-- Index on the ActivityEvent table's tenantGuid field.
CREATE INDEX [I_ActivityEvent_tenantGuid] ON [BMC].[ActivityEvent] ([tenantGuid])
GO

-- Index on the ActivityEvent table's tenantGuid,activityEventTypeId fields.
CREATE INDEX [I_ActivityEvent_tenantGuid_activityEventTypeId] ON [BMC].[ActivityEvent] ([tenantGuid], [activityEventTypeId])
GO

-- Index on the ActivityEvent table's tenantGuid,active fields.
CREATE INDEX [I_ActivityEvent_tenantGuid_active] ON [BMC].[ActivityEvent] ([tenantGuid], [active])
GO

-- Index on the ActivityEvent table's tenantGuid,deleted fields.
CREATE INDEX [I_ActivityEvent_tenantGuid_deleted] ON [BMC].[ActivityEvent] ([tenantGuid], [deleted])
GO


-- A MOC (My Own Creation) published to the community gallery. Links to the underlying project for parts list and 3D model data.
CREATE TABLE [BMC].[PublishedMoc]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[projectId] INT NOT NULL,		-- The underlying project containing the model data
	[name] NVARCHAR(100) NOT NULL,		-- Public-facing title of the MOC
	[description] NVARCHAR(MAX) NULL,		-- Rich description of the MOC, build story, or design notes
	[thumbnailImagePath] NVARCHAR(250) NULL,		-- Relative path to the primary thumbnail image
	[tags] NVARCHAR(MAX) NULL,		-- Comma-separated tags for search and categorization (e.g. 'technic, crane, vehicle')
	[isPublished] BIT NOT NULL DEFAULT 0,		-- Whether this MOC is visible in the public gallery (draft vs published)
	[isFeatured] BIT NOT NULL DEFAULT 0,		-- Whether this MOC is featured / editor's pick (set by moderators)
	[publishedDate] DATETIME2(7) NULL,		-- Date/time the MOC was first published
	[viewCount] INT NOT NULL DEFAULT 0,		-- Number of times this MOC has been viewed
	[likeCount] INT NOT NULL DEFAULT 0,		-- Cached like count for fast sorting and display
	[commentCount] INT NOT NULL DEFAULT 0,		-- Cached comment count for fast display
	[favouriteCount] INT NOT NULL DEFAULT 0,		-- Cached favourite/bookmark count for fast display
	[partCount] INT NULL,		-- Cached total part count from the underlying project
	[allowForking] BIT NOT NULL DEFAULT 1,		-- Whether other users can fork (copy) this MOC as a starting point
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_PublishedMoc_Project_projectId] FOREIGN KEY ([projectId]) REFERENCES [BMC].[Project] ([id]),		-- Foreign key to the Project table.
	CONSTRAINT [UC_PublishedMoc_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the PublishedMoc table's tenantGuid and name fields.
)
GO

-- Index on the PublishedMoc table's tenantGuid field.
CREATE INDEX [I_PublishedMoc_tenantGuid] ON [BMC].[PublishedMoc] ([tenantGuid])
GO

-- Index on the PublishedMoc table's tenantGuid,projectId fields.
CREATE INDEX [I_PublishedMoc_tenantGuid_projectId] ON [BMC].[PublishedMoc] ([tenantGuid], [projectId])
GO

-- Index on the PublishedMoc table's tenantGuid,name fields.
CREATE INDEX [I_PublishedMoc_tenantGuid_name] ON [BMC].[PublishedMoc] ([tenantGuid], [name])
GO

-- Index on the PublishedMoc table's tenantGuid,active fields.
CREATE INDEX [I_PublishedMoc_tenantGuid_active] ON [BMC].[PublishedMoc] ([tenantGuid], [active])
GO

-- Index on the PublishedMoc table's tenantGuid,deleted fields.
CREATE INDEX [I_PublishedMoc_tenantGuid_deleted] ON [BMC].[PublishedMoc] ([tenantGuid], [deleted])
GO


-- The change history for records from the PublishedMoc table.
CREATE TABLE [BMC].[PublishedMocChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[publishedMocId] INT NOT NULL,		-- Link to the PublishedMoc table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_PublishedMocChangeHistory_PublishedMoc_publishedMocId] FOREIGN KEY ([publishedMocId]) REFERENCES [BMC].[PublishedMoc] ([id])		-- Foreign key to the PublishedMoc table.
)
GO

-- Index on the PublishedMocChangeHistory table's tenantGuid field.
CREATE INDEX [I_PublishedMocChangeHistory_tenantGuid] ON [BMC].[PublishedMocChangeHistory] ([tenantGuid])
GO

-- Index on the PublishedMocChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_PublishedMocChangeHistory_tenantGuid_versionNumber] ON [BMC].[PublishedMocChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the PublishedMocChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_PublishedMocChangeHistory_tenantGuid_timeStamp] ON [BMC].[PublishedMocChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the PublishedMocChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_PublishedMocChangeHistory_tenantGuid_userId] ON [BMC].[PublishedMocChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the PublishedMocChangeHistory table's tenantGuid,publishedMocId fields.
CREATE INDEX [I_PublishedMocChangeHistory_tenantGuid_publishedMocId] ON [BMC].[PublishedMocChangeHistory] ([tenantGuid], [publishedMocId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Additional gallery images for a published MOC. The thumbnail is on the PublishedMoc itself; these are supplementary views and renders.
CREATE TABLE [BMC].[PublishedMocImage]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[publishedMocId] INT NOT NULL,		-- The published MOC this image belongs to
	[imagePath] NVARCHAR(250) NOT NULL,		-- Relative path to the image file
	[caption] NVARCHAR(250) NULL,		-- Optional caption describing the image or the angle shown
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_PublishedMocImage_PublishedMoc_publishedMocId] FOREIGN KEY ([publishedMocId]) REFERENCES [BMC].[PublishedMoc] ([id])		-- Foreign key to the PublishedMoc table.
)
GO

-- Index on the PublishedMocImage table's tenantGuid field.
CREATE INDEX [I_PublishedMocImage_tenantGuid] ON [BMC].[PublishedMocImage] ([tenantGuid])
GO

-- Index on the PublishedMocImage table's tenantGuid,publishedMocId fields.
CREATE INDEX [I_PublishedMocImage_tenantGuid_publishedMocId] ON [BMC].[PublishedMocImage] ([tenantGuid], [publishedMocId])
GO

-- Index on the PublishedMocImage table's tenantGuid,active fields.
CREATE INDEX [I_PublishedMocImage_tenantGuid_active] ON [BMC].[PublishedMocImage] ([tenantGuid], [active])
GO

-- Index on the PublishedMocImage table's tenantGuid,deleted fields.
CREATE INDEX [I_PublishedMocImage_tenantGuid_deleted] ON [BMC].[PublishedMocImage] ([tenantGuid], [deleted])
GO


-- User likes on published MOCs. One like per user per MOC.
CREATE TABLE [BMC].[MocLike]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[publishedMocId] INT NOT NULL,		-- The MOC being liked
	[likerTenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- Tenant GUID of the user who liked
	[likedDate] DATETIME2(7) NOT NULL,		-- Date/time the like was registered
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_MocLike_PublishedMoc_publishedMocId] FOREIGN KEY ([publishedMocId]) REFERENCES [BMC].[PublishedMoc] ([id]),		-- Foreign key to the PublishedMoc table.
	CONSTRAINT [UC_MocLike_publishedMocId_likerTenantGuid] UNIQUE ( [publishedMocId], [likerTenantGuid]) 		-- Uniqueness enforced on the MocLike table's publishedMocId and likerTenantGuid fields.
)
GO

-- Index on the MocLike table's publishedMocId field.
CREATE INDEX [I_MocLike_publishedMocId] ON [BMC].[MocLike] ([publishedMocId])
GO

-- Index on the MocLike table's active field.
CREATE INDEX [I_MocLike_active] ON [BMC].[MocLike] ([active])
GO

-- Index on the MocLike table's deleted field.
CREATE INDEX [I_MocLike_deleted] ON [BMC].[MocLike] ([deleted])
GO


-- User comments on published MOCs. Supports threaded replies via self-referencing parent FK.
CREATE TABLE [BMC].[MocComment]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[publishedMocId] INT NOT NULL,		-- The MOC being commented on
	[commenterTenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- Tenant GUID of the user who posted the comment
	[commentText] NVARCHAR(MAX) NOT NULL,		-- The comment content
	[postedDate] DATETIME2(7) NOT NULL,		-- Date/time the comment was posted
	[mocCommentId] INT NULL,		-- Optional parent comment for threaded replies (null = top-level comment)
	[isEdited] BIT NOT NULL DEFAULT 0,		-- Whether this comment has been edited after posting
	[isHidden] BIT NOT NULL DEFAULT 0,		-- Whether this comment has been hidden by a moderator
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_MocComment_PublishedMoc_publishedMocId] FOREIGN KEY ([publishedMocId]) REFERENCES [BMC].[PublishedMoc] ([id]),		-- Foreign key to the PublishedMoc table.
	CONSTRAINT [FK_MocComment_MocComment_mocCommentId] FOREIGN KEY ([mocCommentId]) REFERENCES [BMC].[MocComment] ([id])		-- Foreign key to the MocComment table.
)
GO

-- Index on the MocComment table's publishedMocId field.
CREATE INDEX [I_MocComment_publishedMocId] ON [BMC].[MocComment] ([publishedMocId])
GO

-- Index on the MocComment table's mocCommentId field.
CREATE INDEX [I_MocComment_mocCommentId] ON [BMC].[MocComment] ([mocCommentId])
GO

-- Index on the MocComment table's active field.
CREATE INDEX [I_MocComment_active] ON [BMC].[MocComment] ([active])
GO

-- Index on the MocComment table's deleted field.
CREATE INDEX [I_MocComment_deleted] ON [BMC].[MocComment] ([deleted])
GO


-- User's favourited (bookmarked) MOCs for quick access. Separate from likes — favourites are private bookmarks, likes are public endorsements.
CREATE TABLE [BMC].[MocFavourite]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[publishedMocId] INT NOT NULL,		-- The MOC being favourited
	[userTenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- Tenant GUID of the user who favourited
	[favouritedDate] DATETIME2(7) NOT NULL,		-- Date/time the favourite was added
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_MocFavourite_PublishedMoc_publishedMocId] FOREIGN KEY ([publishedMocId]) REFERENCES [BMC].[PublishedMoc] ([id]),		-- Foreign key to the PublishedMoc table.
	CONSTRAINT [UC_MocFavourite_publishedMocId_userTenantGuid] UNIQUE ( [publishedMocId], [userTenantGuid]) 		-- Uniqueness enforced on the MocFavourite table's publishedMocId and userTenantGuid fields.
)
GO

-- Index on the MocFavourite table's publishedMocId field.
CREATE INDEX [I_MocFavourite_publishedMocId] ON [BMC].[MocFavourite] ([publishedMocId])
GO

-- Index on the MocFavourite table's active field.
CREATE INDEX [I_MocFavourite_active] ON [BMC].[MocFavourite] ([active])
GO

-- Index on the MocFavourite table's deleted field.
CREATE INDEX [I_MocFavourite_deleted] ON [BMC].[MocFavourite] ([deleted])
GO


-- Published instruction manuals shared with the community. Can be BMC-native format (linked to BuildManual), uploaded PDF, or image-based.
CREATE TABLE [BMC].[SharedInstruction]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[buildManualId] INT NULL,		-- Optional link to a BMC-native BuildManual (null for uploaded PDF/image instructions)
	[publishedMocId] INT NULL,		-- Optional link to the published MOC these instructions are for
	[name] NVARCHAR(100) NOT NULL,		-- Public-facing title of the instruction document
	[description] NVARCHAR(MAX) NULL,		-- Description of what these instructions cover
	[formatType] NVARCHAR(50) NOT NULL,		-- Format of the instruction: BMCNative, PDF, ImageSet
	[filePath] NVARCHAR(250) NULL,		-- Relative path to the instruction file (PDF) or folder (image set). Null for BMC-native.
	[isPublished] BIT NOT NULL DEFAULT 0,		-- Whether these instructions are visible in the community
	[publishedDate] DATETIME2(7) NULL,		-- Date/time the instructions were first published
	[downloadCount] INT NOT NULL DEFAULT 0,		-- Number of times these instructions have been downloaded
	[pageCount] INT NULL,		-- Total number of pages (for display purposes)
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_SharedInstruction_BuildManual_buildManualId] FOREIGN KEY ([buildManualId]) REFERENCES [BMC].[BuildManual] ([id]),		-- Foreign key to the BuildManual table.
	CONSTRAINT [FK_SharedInstruction_PublishedMoc_publishedMocId] FOREIGN KEY ([publishedMocId]) REFERENCES [BMC].[PublishedMoc] ([id]),		-- Foreign key to the PublishedMoc table.
	CONSTRAINT [UC_SharedInstruction_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the SharedInstruction table's tenantGuid and name fields.
)
GO

-- Index on the SharedInstruction table's tenantGuid field.
CREATE INDEX [I_SharedInstruction_tenantGuid] ON [BMC].[SharedInstruction] ([tenantGuid])
GO

-- Index on the SharedInstruction table's tenantGuid,buildManualId fields.
CREATE INDEX [I_SharedInstruction_tenantGuid_buildManualId] ON [BMC].[SharedInstruction] ([tenantGuid], [buildManualId])
GO

-- Index on the SharedInstruction table's tenantGuid,publishedMocId fields.
CREATE INDEX [I_SharedInstruction_tenantGuid_publishedMocId] ON [BMC].[SharedInstruction] ([tenantGuid], [publishedMocId])
GO

-- Index on the SharedInstruction table's tenantGuid,name fields.
CREATE INDEX [I_SharedInstruction_tenantGuid_name] ON [BMC].[SharedInstruction] ([tenantGuid], [name])
GO

-- Index on the SharedInstruction table's tenantGuid,active fields.
CREATE INDEX [I_SharedInstruction_tenantGuid_active] ON [BMC].[SharedInstruction] ([tenantGuid], [active])
GO

-- Index on the SharedInstruction table's tenantGuid,deleted fields.
CREATE INDEX [I_SharedInstruction_tenantGuid_deleted] ON [BMC].[SharedInstruction] ([tenantGuid], [deleted])
GO


-- The change history for records from the SharedInstruction table.
CREATE TABLE [BMC].[SharedInstructionChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[sharedInstructionId] INT NOT NULL,		-- Link to the SharedInstruction table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_SharedInstructionChangeHistory_SharedInstruction_sharedInstructionId] FOREIGN KEY ([sharedInstructionId]) REFERENCES [BMC].[SharedInstruction] ([id])		-- Foreign key to the SharedInstruction table.
)
GO

-- Index on the SharedInstructionChangeHistory table's tenantGuid field.
CREATE INDEX [I_SharedInstructionChangeHistory_tenantGuid] ON [BMC].[SharedInstructionChangeHistory] ([tenantGuid])
GO

-- Index on the SharedInstructionChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_SharedInstructionChangeHistory_tenantGuid_versionNumber] ON [BMC].[SharedInstructionChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the SharedInstructionChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_SharedInstructionChangeHistory_tenantGuid_timeStamp] ON [BMC].[SharedInstructionChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the SharedInstructionChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_SharedInstructionChangeHistory_tenantGuid_userId] ON [BMC].[SharedInstructionChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the SharedInstructionChangeHistory table's tenantGuid,sharedInstructionId fields.
CREATE INDEX [I_SharedInstructionChangeHistory_tenantGuid_sharedInstructionId] ON [BMC].[SharedInstructionChangeHistory] ([tenantGuid], [sharedInstructionId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Groups of achievements for organization and display (e.g. Collection, Building, Social, Exploration).
CREATE TABLE [BMC].[AchievementCategory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[iconCssClass] NVARCHAR(100) NULL,		-- CSS class for the category icon
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the AchievementCategory table's name field.
CREATE INDEX [I_AchievementCategory_name] ON [BMC].[AchievementCategory] ([name])
GO

-- Index on the AchievementCategory table's active field.
CREATE INDEX [I_AchievementCategory_active] ON [BMC].[AchievementCategory] ([active])
GO

-- Index on the AchievementCategory table's deleted field.
CREATE INDEX [I_AchievementCategory_deleted] ON [BMC].[AchievementCategory] ([deleted])
GO

INSERT INTO [BMC].[AchievementCategory] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'Collection', 'Achievements related to building and managing your parts collection', 'fas fa-cubes', 1, 'ac100001-0001-4000-8000-000000000001' )
GO

INSERT INTO [BMC].[AchievementCategory] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'Building', 'Achievements related to creating and publishing MOCs', 'fas fa-hammer', 2, 'ac100001-0001-4000-8000-000000000002' )
GO

INSERT INTO [BMC].[AchievementCategory] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'Social', 'Achievements related to community engagement and social interactions', 'fas fa-users', 3, 'ac100001-0001-4000-8000-000000000003' )
GO

INSERT INTO [BMC].[AchievementCategory] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'Exploration', 'Achievements related to exploring the parts catalog and set database', 'fas fa-compass', 4, 'ac100001-0001-4000-8000-000000000004' )
GO

INSERT INTO [BMC].[AchievementCategory] ( [name], [description], [iconCssClass], [sequence], [objectGuid] ) VALUES  ( 'Challenge', 'Achievements earned by competing in build challenges', 'fas fa-medal', 5, 'ac100001-0001-4000-8000-000000000005' )
GO


-- Individual achievement definitions. Each achievement has criteria, point value, and rarity classification.
CREATE TABLE [BMC].[Achievement]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[achievementCategoryId] INT NOT NULL,		-- The category this achievement belongs to
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[iconCssClass] NVARCHAR(100) NULL,		-- CSS class for the achievement icon/badge
	[iconImagePath] NVARCHAR(250) NULL,		-- Optional path to a custom badge image (overrides CSS icon)
	[criteria] NVARCHAR(MAX) NULL,		-- Human-readable description of how to earn this achievement
	[criteriaCode] NVARCHAR(250) NULL,		-- Machine-readable criteria code for automatic detection (e.g. 'parts_owned >= 10000')
	[pointValue] INT NOT NULL DEFAULT 10,		-- Point value when earned — contributes to the user's total achievement score
	[rarity] NVARCHAR(50) NOT NULL,		-- Rarity classification: Common, Uncommon, Rare, Epic, Legendary
	[isActive] BIT NOT NULL DEFAULT 1,		-- Whether this achievement can currently be earned
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Achievement_AchievementCategory_achievementCategoryId] FOREIGN KEY ([achievementCategoryId]) REFERENCES [BMC].[AchievementCategory] ([id])		-- Foreign key to the AchievementCategory table.
)
GO

-- Index on the Achievement table's achievementCategoryId field.
CREATE INDEX [I_Achievement_achievementCategoryId] ON [BMC].[Achievement] ([achievementCategoryId])
GO

-- Index on the Achievement table's name field.
CREATE INDEX [I_Achievement_name] ON [BMC].[Achievement] ([name])
GO

-- Index on the Achievement table's active field.
CREATE INDEX [I_Achievement_active] ON [BMC].[Achievement] ([active])
GO

-- Index on the Achievement table's deleted field.
CREATE INDEX [I_Achievement_deleted] ON [BMC].[Achievement] ([deleted])
GO

INSERT INTO [BMC].[Achievement] ( [name], [description], [iconCssClass], [criteria], [criteriaCode], [pointValue], [rarity], [isActive], [sequence], [achievementCategoryId], [objectGuid] ) VALUES  ( 'First Brick', 'Added your first part to your collection', 'fas fa-cube', 'Add at least 1 part to any collection', 'parts_owned >= 1', 5, 'Common', 1, 1, ( SELECT TOP 1 id FROM [BMC].[AchievementCategory] WHERE [name] = 'Collection' ), 'a1100001-0001-4000-8000-000000000001' )
GO

INSERT INTO [BMC].[Achievement] ( [name], [description], [iconCssClass], [criteria], [criteriaCode], [pointValue], [rarity], [isActive], [sequence], [achievementCategoryId], [objectGuid] ) VALUES  ( 'Brick Enthusiast', 'Own 1,000 parts across all collections', 'fas fa-cubes', 'Total parts owned reaches 1,000', 'parts_owned >= 1000', 25, 'Uncommon', 1, 2, ( SELECT TOP 1 id FROM [BMC].[AchievementCategory] WHERE [name] = 'Collection' ), 'a1100001-0001-4000-8000-000000000002' )
GO

INSERT INTO [BMC].[Achievement] ( [name], [description], [iconCssClass], [criteria], [criteriaCode], [pointValue], [rarity], [isActive], [sequence], [achievementCategoryId], [objectGuid] ) VALUES  ( 'Brick Master', 'Own 10,000 parts across all collections', 'fas fa-warehouse', 'Total parts owned reaches 10,000', 'parts_owned >= 10000', 100, 'Rare', 1, 3, ( SELECT TOP 1 id FROM [BMC].[AchievementCategory] WHERE [name] = 'Collection' ), 'a1100001-0001-4000-8000-000000000003' )
GO

INSERT INTO [BMC].[Achievement] ( [name], [description], [iconCssClass], [criteria], [criteriaCode], [pointValue], [rarity], [isActive], [sequence], [achievementCategoryId], [objectGuid] ) VALUES  ( 'First Creation', 'Published your first MOC to the gallery', 'fas fa-rocket', 'Publish at least 1 MOC to the gallery', 'mocs_published >= 1', 15, 'Common', 1, 10, ( SELECT TOP 1 id FROM [BMC].[AchievementCategory] WHERE [name] = 'Building' ), 'a1100001-0001-4000-8000-000000000010' )
GO

INSERT INTO [BMC].[Achievement] ( [name], [description], [iconCssClass], [criteria], [criteriaCode], [pointValue], [rarity], [isActive], [sequence], [achievementCategoryId], [objectGuid] ) VALUES  ( 'Community Builder', 'Gained 10 followers', 'fas fa-user-friends', 'Reach 10 followers on your profile', 'followers >= 10', 20, 'Uncommon', 1, 20, ( SELECT TOP 1 id FROM [BMC].[AchievementCategory] WHERE [name] = 'Social' ), 'a1100001-0001-4000-8000-000000000020' )
GO


-- Records of achievements earned by users. Created when a user meets an achievement's criteria.
CREATE TABLE [BMC].[UserAchievement]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[achievementId] INT NOT NULL,		-- The achievement earned
	[earnedDate] DATETIME2(7) NOT NULL,		-- Date/time the achievement was earned
	[isDisplayed] BIT NOT NULL DEFAULT 1,		-- Whether this achievement is displayed on the user's public profile showcase
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_UserAchievement_Achievement_achievementId] FOREIGN KEY ([achievementId]) REFERENCES [BMC].[Achievement] ([id]),		-- Foreign key to the Achievement table.
	CONSTRAINT [UC_UserAchievement_tenantGuid_achievementId] UNIQUE ( [tenantGuid], [achievementId]) 		-- Uniqueness enforced on the UserAchievement table's tenantGuid and achievementId fields.
)
GO

-- Index on the UserAchievement table's tenantGuid field.
CREATE INDEX [I_UserAchievement_tenantGuid] ON [BMC].[UserAchievement] ([tenantGuid])
GO

-- Index on the UserAchievement table's tenantGuid,achievementId fields.
CREATE INDEX [I_UserAchievement_tenantGuid_achievementId] ON [BMC].[UserAchievement] ([tenantGuid], [achievementId])
GO

-- Index on the UserAchievement table's tenantGuid,active fields.
CREATE INDEX [I_UserAchievement_tenantGuid_active] ON [BMC].[UserAchievement] ([tenantGuid], [active])
GO

-- Index on the UserAchievement table's tenantGuid,deleted fields.
CREATE INDEX [I_UserAchievement_tenantGuid_deleted] ON [BMC].[UserAchievement] ([tenantGuid], [deleted])
GO


-- Special display badges that can be awarded to users by moderators or earned through special events. Displayed prominently on user profiles.
CREATE TABLE [BMC].[UserBadge]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NOT NULL,
	[iconCssClass] NVARCHAR(100) NULL,		-- CSS class for the badge icon
	[iconImagePath] NVARCHAR(250) NULL,		-- Optional path to a custom badge image
	[badgeColor] NVARCHAR(10) NULL,		-- Optional accent colour for the badge display
	[isAutomatic] BIT NOT NULL DEFAULT 0,		-- Whether this badge is automatically awarded (vs. manually by moderators)
	[automaticCriteriaCode] NVARCHAR(250) NULL,		-- Machine-readable criteria for automatic badges (null for manual badges)
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the UserBadge table's name field.
CREATE INDEX [I_UserBadge_name] ON [BMC].[UserBadge] ([name])
GO

-- Index on the UserBadge table's active field.
CREATE INDEX [I_UserBadge_active] ON [BMC].[UserBadge] ([active])
GO

-- Index on the UserBadge table's deleted field.
CREATE INDEX [I_UserBadge_deleted] ON [BMC].[UserBadge] ([deleted])
GO

INSERT INTO [BMC].[UserBadge] ( [name], [description], [iconCssClass], [isAutomatic], [sequence], [objectGuid] ) VALUES  ( 'Early Adopter', 'Joined the BMC community during the early access period', 'fas fa-star', 0, 1, 'ab100001-0001-4000-8000-000000000001' )
GO

INSERT INTO [BMC].[UserBadge] ( [name], [description], [iconCssClass], [isAutomatic], [sequence], [objectGuid] ) VALUES  ( 'Verified Builder', 'Identity verified by the BMC team', 'fas fa-check-circle', 0, 2, 'ab100001-0001-4000-8000-000000000002' )
GO

INSERT INTO [BMC].[UserBadge] ( [name], [description], [iconCssClass], [isAutomatic], [sequence], [objectGuid] ) VALUES  ( 'Top Contributor', 'One of the most active community contributors this month', 'fas fa-crown', 0, 3, 'ab100001-0001-4000-8000-000000000003' )
GO

INSERT INTO [BMC].[UserBadge] ( [name], [description], [iconCssClass], [isAutomatic], [sequence], [objectGuid] ) VALUES  ( 'Challenge Winner', 'Won a community build challenge', 'fas fa-award', 0, 4, 'ab100001-0001-4000-8000-000000000004' )
GO

INSERT INTO [BMC].[UserBadge] ( [name], [description], [iconCssClass], [isAutomatic], [sequence], [objectGuid] ) VALUES  ( 'Moderator', 'Community moderator trusted to help maintain quality', 'fas fa-shield-alt', 0, 5, 'ab100001-0001-4000-8000-000000000005' )
GO


-- Maps badges to users. A badge can be awarded multiple times conceptually, but one unique assignment per user per badge.
CREATE TABLE [BMC].[UserBadgeAssignment]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[userBadgeId] INT NOT NULL,		-- The badge awarded
	[awardedDate] DATETIME2(7) NOT NULL,		-- Date/time the badge was awarded
	[awardedByTenantGuid] UNIQUEIDENTIFIER NULL,		-- Tenant GUID of the moderator who awarded the badge (null for automatic badges)
	[reason] NVARCHAR(MAX) NULL,		-- Optional reason or context for awarding the badge
	[isDisplayed] BIT NOT NULL DEFAULT 1,		-- Whether this badge is displayed on the user's profile
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_UserBadgeAssignment_UserBadge_userBadgeId] FOREIGN KEY ([userBadgeId]) REFERENCES [BMC].[UserBadge] ([id]),		-- Foreign key to the UserBadge table.
	CONSTRAINT [UC_UserBadgeAssignment_tenantGuid_userBadgeId] UNIQUE ( [tenantGuid], [userBadgeId]) 		-- Uniqueness enforced on the UserBadgeAssignment table's tenantGuid and userBadgeId fields.
)
GO

-- Index on the UserBadgeAssignment table's tenantGuid field.
CREATE INDEX [I_UserBadgeAssignment_tenantGuid] ON [BMC].[UserBadgeAssignment] ([tenantGuid])
GO

-- Index on the UserBadgeAssignment table's tenantGuid,userBadgeId fields.
CREATE INDEX [I_UserBadgeAssignment_tenantGuid_userBadgeId] ON [BMC].[UserBadgeAssignment] ([tenantGuid], [userBadgeId])
GO

-- Index on the UserBadgeAssignment table's tenantGuid,active fields.
CREATE INDEX [I_UserBadgeAssignment_tenantGuid_active] ON [BMC].[UserBadgeAssignment] ([tenantGuid], [active])
GO

-- Index on the UserBadgeAssignment table's tenantGuid,deleted fields.
CREATE INDEX [I_UserBadgeAssignment_tenantGuid_deleted] ON [BMC].[UserBadgeAssignment] ([tenantGuid], [deleted])
GO


-- Community build challenges with themes, rules, and time windows. Created by moderators or admins.
CREATE TABLE [BMC].[BuildChallenge]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,		-- Title of the challenge (e.g. 'Under 100 Parts Technic Vehicle')
	[description] NVARCHAR(MAX) NULL,		-- Full description of the challenge theme and goals
	[rules] NVARCHAR(MAX) NULL,		-- Detailed rules and constraints for entries
	[thumbnailImagePath] NVARCHAR(250) NULL,		-- Promotional image for the challenge
	[startDate] DATETIME2(7) NOT NULL,		-- When submissions open
	[endDate] DATETIME2(7) NOT NULL,		-- When submissions close
	[votingEndDate] DATETIME2(7) NULL,		-- When community voting closes (null if no voting period)
	[isActive] BIT NOT NULL DEFAULT 1,		-- Whether this challenge is currently active and accepting entries
	[isFeatured] BIT NOT NULL DEFAULT 0,		-- Whether this challenge should be prominently displayed on the landing page
	[entryCount] INT NOT NULL DEFAULT 0,		-- Cached count of submitted entries
	[maxPartsLimit] INT NULL,		-- Optional maximum part count constraint for entries (null = no limit)
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the BuildChallenge table's name field.
CREATE INDEX [I_BuildChallenge_name] ON [BMC].[BuildChallenge] ([name])
GO

-- Index on the BuildChallenge table's active field.
CREATE INDEX [I_BuildChallenge_active] ON [BMC].[BuildChallenge] ([active])
GO

-- Index on the BuildChallenge table's deleted field.
CREATE INDEX [I_BuildChallenge_deleted] ON [BMC].[BuildChallenge] ([deleted])
GO


-- The change history for records from the BuildChallenge table.
CREATE TABLE [BMC].[BuildChallengeChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[buildChallengeId] INT NOT NULL,		-- Link to the BuildChallenge table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_BuildChallengeChangeHistory_BuildChallenge_buildChallengeId] FOREIGN KEY ([buildChallengeId]) REFERENCES [BMC].[BuildChallenge] ([id])		-- Foreign key to the BuildChallenge table.
)
GO

-- Index on the BuildChallengeChangeHistory table's versionNumber field.
CREATE INDEX [I_BuildChallengeChangeHistory_versionNumber] ON [BMC].[BuildChallengeChangeHistory] ([versionNumber])
GO

-- Index on the BuildChallengeChangeHistory table's timeStamp field.
CREATE INDEX [I_BuildChallengeChangeHistory_timeStamp] ON [BMC].[BuildChallengeChangeHistory] ([timeStamp])
GO

-- Index on the BuildChallengeChangeHistory table's userId field.
CREATE INDEX [I_BuildChallengeChangeHistory_userId] ON [BMC].[BuildChallengeChangeHistory] ([userId])
GO

-- Index on the BuildChallengeChangeHistory table's buildChallengeId field.
CREATE INDEX [I_BuildChallengeChangeHistory_buildChallengeId] ON [BMC].[BuildChallengeChangeHistory] ([buildChallengeId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- User-submitted entries into a build challenge. Links to a published MOC.
CREATE TABLE [BMC].[BuildChallengeEntry]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[buildChallengeId] INT NOT NULL,		-- The challenge being entered
	[publishedMocId] INT NOT NULL,		-- The published MOC submitted as an entry
	[submittedDate] DATETIME2(7) NOT NULL,		-- Date/time the entry was submitted
	[entryNotes] NVARCHAR(MAX) NULL,		-- Optional notes from the builder about their entry
	[voteCount] INT NOT NULL DEFAULT 0,		-- Cached community vote count
	[isWinner] BIT NOT NULL DEFAULT 0,		-- Whether this entry was selected as a winner
	[isDisqualified] BIT NOT NULL DEFAULT 0,		-- Whether this entry was disqualified by moderators
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_BuildChallengeEntry_BuildChallenge_buildChallengeId] FOREIGN KEY ([buildChallengeId]) REFERENCES [BMC].[BuildChallenge] ([id]),		-- Foreign key to the BuildChallenge table.
	CONSTRAINT [FK_BuildChallengeEntry_PublishedMoc_publishedMocId] FOREIGN KEY ([publishedMocId]) REFERENCES [BMC].[PublishedMoc] ([id]),		-- Foreign key to the PublishedMoc table.
	CONSTRAINT [UC_BuildChallengeEntry_tenantGuid_buildChallengeId] UNIQUE ( [tenantGuid], [buildChallengeId]) 		-- Uniqueness enforced on the BuildChallengeEntry table's tenantGuid and buildChallengeId fields.
)
GO

-- Index on the BuildChallengeEntry table's tenantGuid field.
CREATE INDEX [I_BuildChallengeEntry_tenantGuid] ON [BMC].[BuildChallengeEntry] ([tenantGuid])
GO

-- Index on the BuildChallengeEntry table's tenantGuid,buildChallengeId fields.
CREATE INDEX [I_BuildChallengeEntry_tenantGuid_buildChallengeId] ON [BMC].[BuildChallengeEntry] ([tenantGuid], [buildChallengeId])
GO

-- Index on the BuildChallengeEntry table's tenantGuid,publishedMocId fields.
CREATE INDEX [I_BuildChallengeEntry_tenantGuid_publishedMocId] ON [BMC].[BuildChallengeEntry] ([tenantGuid], [publishedMocId])
GO

-- Index on the BuildChallengeEntry table's tenantGuid,active fields.
CREATE INDEX [I_BuildChallengeEntry_tenantGuid_active] ON [BMC].[BuildChallengeEntry] ([tenantGuid], [active])
GO

-- Index on the BuildChallengeEntry table's tenantGuid,deleted fields.
CREATE INDEX [I_BuildChallengeEntry_tenantGuid_deleted] ON [BMC].[BuildChallengeEntry] ([tenantGuid], [deleted])
GO


-- Lookup table of reasons a user can report community content (Spam, Inappropriate, Copyright, etc.).
CREATE TABLE [BMC].[ContentReportReason]
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

-- Index on the ContentReportReason table's name field.
CREATE INDEX [I_ContentReportReason_name] ON [BMC].[ContentReportReason] ([name])
GO

-- Index on the ContentReportReason table's active field.
CREATE INDEX [I_ContentReportReason_active] ON [BMC].[ContentReportReason] ([active])
GO

-- Index on the ContentReportReason table's deleted field.
CREATE INDEX [I_ContentReportReason_deleted] ON [BMC].[ContentReportReason] ([deleted])
GO

INSERT INTO [BMC].[ContentReportReason] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Spam', 'Content is spam, advertising, or promotional', 1, 'c4100001-0001-4000-8000-000000000001' )
GO

INSERT INTO [BMC].[ContentReportReason] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Inappropriate', 'Content is offensive, vulgar, or inappropriate', 2, 'c4100001-0001-4000-8000-000000000002' )
GO

INSERT INTO [BMC].[ContentReportReason] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Copyright', 'Content violates copyright or intellectual property', 3, 'c4100001-0001-4000-8000-000000000003' )
GO

INSERT INTO [BMC].[ContentReportReason] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Harassment', 'Content constitutes harassment or bullying', 4, 'c4100001-0001-4000-8000-000000000004' )
GO

INSERT INTO [BMC].[ContentReportReason] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Misinformation', 'Content contains misleading or false information', 5, 'c4100001-0001-4000-8000-000000000005' )
GO

INSERT INTO [BMC].[ContentReportReason] ( [name], [description], [sequence], [objectGuid] ) VALUES  ( 'Other', 'Other reason not covered above', 99, 'c4100001-0001-4000-8000-000000000099' )
GO


-- User-submitted reports of problematic community content. Reviewed by moderators via the BMC Admin project.
CREATE TABLE [BMC].[ContentReport]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[contentReportReasonId] INT NOT NULL,		-- The reason for the report
	[reporterTenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- Tenant GUID of the user submitting the report
	[reportedEntityType] NVARCHAR(100) NOT NULL,		-- Type of the reported content (e.g. 'PublishedMoc', 'MocComment', 'UserProfile')
	[reportedEntityId] BIGINT NOT NULL,		-- ID of the reported entity
	[description] NVARCHAR(MAX) NULL,		-- Additional details provided by the reporter
	[status] NVARCHAR(50) NOT NULL,		-- Report status: Pending, UnderReview, Dismissed, ActionTaken
	[reportedDate] DATETIME2(7) NOT NULL,		-- Date/time the report was submitted
	[reviewedDate] DATETIME2(7) NULL,		-- Date/time a moderator reviewed the report (null if pending)
	[reviewerTenantGuid] UNIQUEIDENTIFIER NULL,		-- Tenant GUID of the moderator who reviewed (null if pending)
	[reviewNotes] NVARCHAR(MAX) NULL,		-- Moderator notes on the review decision
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ContentReport_ContentReportReason_contentReportReasonId] FOREIGN KEY ([contentReportReasonId]) REFERENCES [BMC].[ContentReportReason] ([id])		-- Foreign key to the ContentReportReason table.
)
GO

-- Index on the ContentReport table's contentReportReasonId field.
CREATE INDEX [I_ContentReport_contentReportReasonId] ON [BMC].[ContentReport] ([contentReportReasonId])
GO

-- Index on the ContentReport table's active field.
CREATE INDEX [I_ContentReport_active] ON [BMC].[ContentReport] ([active])
GO

-- Index on the ContentReport table's deleted field.
CREATE INDEX [I_ContentReport_deleted] ON [BMC].[ContentReport] ([deleted])
GO


-- Audit log of actions taken by moderators. Immutable record for accountability.
CREATE TABLE [BMC].[ModerationAction]
(
	[id] BIGINT IDENTITY PRIMARY KEY NOT NULL,
	[moderatorTenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- Tenant GUID of the moderator who took the action
	[actionType] NVARCHAR(100) NOT NULL,		-- Type of action: Warning, ContentRemoved, ContentHidden, UserSuspended, UserBanned, BadgeAwarded
	[targetTenantGuid] UNIQUEIDENTIFIER NULL,		-- Tenant GUID of the user the action was taken against (null for content-only actions)
	[targetEntityType] NVARCHAR(100) NULL,		-- Type of the target entity (e.g. 'PublishedMoc', 'MocComment', 'UserProfile')
	[targetEntityId] BIGINT NULL,		-- ID of the target entity (null for user-level actions)
	[reason] NVARCHAR(MAX) NULL,		-- Reason for the moderation action
	[actionDate] DATETIME2(7) NOT NULL,		-- Date/time the action was taken
	[contentReportId] INT NULL,		-- Optional link to the content report that triggered this action
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ModerationAction_ContentReport_contentReportId] FOREIGN KEY ([contentReportId]) REFERENCES [BMC].[ContentReport] ([id])		-- Foreign key to the ContentReport table.
)
GO

-- Index on the ModerationAction table's contentReportId field.
CREATE INDEX [I_ModerationAction_contentReportId] ON [BMC].[ModerationAction] ([contentReportId])
GO

-- Index on the ModerationAction table's active field.
CREATE INDEX [I_ModerationAction_active] ON [BMC].[ModerationAction] ([active])
GO

-- Index on the ModerationAction table's deleted field.
CREATE INDEX [I_ModerationAction_deleted] ON [BMC].[ModerationAction] ([deleted])
GO


-- Admin-created announcements displayed on the public landing page and/or dashboard. Time-windowed with priority ordering.
CREATE TABLE [BMC].[PlatformAnnouncement]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,		-- Announcement headline/title
	[body] NVARCHAR(MAX) NULL,		-- Full announcement content (supports markdown or HTML)
	[announcementType] NVARCHAR(50) NULL,		-- Type for styling: Info, Warning, Celebration, Maintenance
	[startDate] DATETIME2(7) NOT NULL,		-- When the announcement becomes visible
	[endDate] DATETIME2(7) NULL,		-- When the announcement expires (null = no expiry)
	[isActive] BIT NOT NULL DEFAULT 1,		-- Whether the announcement is currently active
	[priority] INT NOT NULL DEFAULT 0,		-- Display priority (higher = more prominent)
	[showOnLandingPage] BIT NOT NULL DEFAULT 1,		-- Whether to show on the public landing page
	[showOnDashboard] BIT NOT NULL DEFAULT 1,		-- Whether to show on the authenticated user dashboard
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the PlatformAnnouncement table's name field.
CREATE INDEX [I_PlatformAnnouncement_name] ON [BMC].[PlatformAnnouncement] ([name])
GO

-- Index on the PlatformAnnouncement table's active field.
CREATE INDEX [I_PlatformAnnouncement_active] ON [BMC].[PlatformAnnouncement] ([active])
GO

-- Index on the PlatformAnnouncement table's deleted field.
CREATE INDEX [I_PlatformAnnouncement_deleted] ON [BMC].[PlatformAnnouncement] ([deleted])
GO


-- API keys issued to users or external integrators for accessing the BMC Public API. Keys are stored as hashes for security.
CREATE TABLE [BMC].[ApiKey]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[keyHash] NVARCHAR(250) NOT NULL,		-- SHA-256 hash of the API key (the plain key is shown once at creation, then discarded)
	[keyPrefix] NVARCHAR(100) NOT NULL,		-- First 8 characters of the key for identification without exposing the full key
	[name] NVARCHAR(100) NOT NULL,		-- User-defined name for the key (e.g. 'My BrickLink Integration')
	[description] NVARCHAR(MAX) NULL,		-- Optional description of what this key is used for
	[isActive] BIT NOT NULL DEFAULT 1,		-- Whether this key is active and can authenticate requests
	[createdDate] DATETIME2(7) NOT NULL,		-- Date/time the key was created
	[lastUsedDate] DATETIME2(7) NULL,		-- Date/time the key was last used to make a request
	[expiresDate] DATETIME2(7) NULL,		-- Optional expiry date (null = no expiry)
	[rateLimitPerHour] INT NOT NULL DEFAULT 1000,		-- Maximum API requests allowed per hour with this key
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_ApiKey_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the ApiKey table's tenantGuid and name fields.
)
GO

-- Index on the ApiKey table's tenantGuid field.
CREATE INDEX [I_ApiKey_tenantGuid] ON [BMC].[ApiKey] ([tenantGuid])
GO

-- Index on the ApiKey table's tenantGuid,name fields.
CREATE INDEX [I_ApiKey_tenantGuid_name] ON [BMC].[ApiKey] ([tenantGuid], [name])
GO

-- Index on the ApiKey table's tenantGuid,active fields.
CREATE INDEX [I_ApiKey_tenantGuid_active] ON [BMC].[ApiKey] ([tenantGuid], [active])
GO

-- Index on the ApiKey table's tenantGuid,deleted fields.
CREATE INDEX [I_ApiKey_tenantGuid_deleted] ON [BMC].[ApiKey] ([tenantGuid], [deleted])
GO


-- Audit log of requests made through the BMC Public API. Used for rate limiting, usage analytics, and abuse detection.
CREATE TABLE [BMC].[ApiRequestLog]
(
	[id] BIGINT IDENTITY PRIMARY KEY NOT NULL,
	[apiKeyId] INT NOT NULL,		-- The API key used for this request
	[endpoint] NVARCHAR(250) NOT NULL,		-- The API endpoint that was called (e.g. '/api/v1/parts/3001')
	[httpMethod] NVARCHAR(10) NOT NULL,		-- HTTP method (GET, POST, PUT, DELETE)
	[responseStatus] INT NOT NULL,		-- HTTP response status code (200, 401, 429, etc.)
	[requestDate] DATETIME2(7) NOT NULL,		-- Date/time of the request
	[durationMs] INT NULL,		-- Request processing duration in milliseconds
	[clientIpAddress] NVARCHAR(100) NULL,		-- IP address of the client making the request
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_ApiRequestLog_ApiKey_apiKeyId] FOREIGN KEY ([apiKeyId]) REFERENCES [BMC].[ApiKey] ([id])		-- Foreign key to the ApiKey table.
)
GO

-- Index on the ApiRequestLog table's apiKeyId field.
CREATE INDEX [I_ApiRequestLog_apiKeyId] ON [BMC].[ApiRequestLog] ([apiKeyId])
GO

-- Index on the ApiRequestLog table's active field.
CREATE INDEX [I_ApiRequestLog_active] ON [BMC].[ApiRequestLog] ([active])
GO

-- Index on the ApiRequestLog table's deleted field.
CREATE INDEX [I_ApiRequestLog_deleted] ON [BMC].[ApiRequestLog] ([deleted])
GO


-- Tracks self-service user registrations through the two-step email verification process. Stores pending registrations until email verification is completed, then provisions the SecurityUser, SecurityTenant, and UserProfile. Designed for auditing and reporting on the registration funnel.
CREATE TABLE [BMC].[PendingRegistration]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[accountName] NVARCHAR(250) NOT NULL,		-- The requested username for the new account
	[emailAddress] NVARCHAR(100) NOT NULL,		-- The email address to verify
	[displayName] NVARCHAR(250) NULL,		-- Optional display name for the profile (defaults to accountName if not provided)
	[passwordHash] NVARCHAR(250) NOT NULL,		-- Pre-hashed password stored during the pending period
	[verificationCode] NVARCHAR(50) NOT NULL,		-- The code or token sent to the user for verification (email, SMS, OTP)
	[codeExpiresAt] DATETIME2(7) NOT NULL,		-- When the verification code expires (default 15 minutes from creation)
	[verificationAttempts] INT NOT NULL DEFAULT 0,		-- Number of times the user has attempted to enter the verification code
	[status] NVARCHAR(50) NOT NULL,		-- Registration status: Pending, Verified, Provisioned, Expired, Failed
	[createdAt] DATETIME2(7) NOT NULL,		-- When the registration was initiated
	[verifiedAt] DATETIME2(7) NULL,		-- When the verification code was successfully validated
	[provisionedAt] DATETIME2(7) NULL,		-- When the SecurityUser and SecurityTenant were created
	[ipAddress] NVARCHAR(100) NULL,		-- Client IP address for security auditing
	[userAgent] NVARCHAR(500) NULL,		-- Client user agent for security auditing
	[verificationChannel] NVARCHAR(50) NULL,		-- Channel used for verification: Email, SMS, OTP (default Email)
	[failureReason] NVARCHAR(1000) NULL,		-- Reason for failure if status is Failed
	[provisionedSecurityUserId] INT NULL,		-- The SecurityUser.id created on successful provisioning, for cross-referencing
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the PendingRegistration table's accountName field.
CREATE INDEX [I_PendingRegistration_accountName] ON [BMC].[PendingRegistration] ([accountName])
GO

-- Index on the PendingRegistration table's emailAddress field.
CREATE INDEX [I_PendingRegistration_emailAddress] ON [BMC].[PendingRegistration] ([emailAddress])
GO

-- Index on the PendingRegistration table's verificationCode field.
CREATE INDEX [I_PendingRegistration_verificationCode] ON [BMC].[PendingRegistration] ([verificationCode])
GO

-- Index on the PendingRegistration table's codeExpiresAt field.
CREATE INDEX [I_PendingRegistration_codeExpiresAt] ON [BMC].[PendingRegistration] ([codeExpiresAt])
GO

-- Index on the PendingRegistration table's status field.
CREATE INDEX [I_PendingRegistration_status] ON [BMC].[PendingRegistration] ([status])
GO

-- Index on the PendingRegistration table's createdAt field.
CREATE INDEX [I_PendingRegistration_createdAt] ON [BMC].[PendingRegistration] ([createdAt])
GO

-- Index on the PendingRegistration table's active field.
CREATE INDEX [I_PendingRegistration_active] ON [BMC].[PendingRegistration] ([active])
GO

-- Index on the PendingRegistration table's deleted field.
CREATE INDEX [I_PendingRegistration_deleted] ON [BMC].[PendingRegistration] ([deleted])
GO

-- Index on the PendingRegistration table's status,codeExpiresAt,active,deleted fields.
CREATE INDEX [I_PendingRegistration_status_codeExpiresAt_active_deleted] ON [BMC].[PendingRegistration] ([status], [codeExpiresAt], [active], [deleted])
GO


-- Stores per-tenant BrickLink OAuth 1.0 token credentials and sync state. The consumer key/secret are stored in appsettings.json; the per-user token value/secret are stored here encrypted.
CREATE TABLE [BMC].[BrickLinkUserLink]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[encryptedTokenValue] NVARCHAR(500) NULL,		-- Encrypted OAuth 1.0 token value — encrypted via ASP.NET Data Protection
	[encryptedTokenSecret] NVARCHAR(500) NULL,		-- Encrypted OAuth 1.0 token secret — encrypted via ASP.NET Data Protection
	[syncEnabled] BIT NOT NULL DEFAULT 0,		-- Whether automatic sync is enabled for this tenant
	[syncDirection] NVARCHAR(50) NULL,		-- Sync direction: Pull, Push, or Both (null = Pull)
	[lastSyncDate] DATETIME2(7) NULL,		-- Date/time of the last successful sync operation
	[lastPullDate] DATETIME2(7) NULL,		-- Date/time of the last successful pull from BrickLink
	[lastPushDate] DATETIME2(7) NULL,		-- Date/time of the last successful push to BrickLink
	[lastSyncError] NVARCHAR(1000) NULL,		-- Error message from the last failed sync attempt (null = no errors)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_BrickLinkUserLink_tenantGuid] UNIQUE ( [tenantGuid]) 		-- Uniqueness enforced on the BrickLinkUserLink table's tenantGuid field.
)
GO

-- Index on the BrickLinkUserLink table's tenantGuid field.
CREATE INDEX [I_BrickLinkUserLink_tenantGuid] ON [BMC].[BrickLinkUserLink] ([tenantGuid])
GO

-- Index on the BrickLinkUserLink table's tenantGuid,active fields.
CREATE INDEX [I_BrickLinkUserLink_tenantGuid_active] ON [BMC].[BrickLinkUserLink] ([tenantGuid], [active])
GO

-- Index on the BrickLinkUserLink table's tenantGuid,deleted fields.
CREATE INDEX [I_BrickLinkUserLink_tenantGuid_deleted] ON [BMC].[BrickLinkUserLink] ([tenantGuid], [deleted])
GO


-- Stores per-tenant BrickEconomy Premium API key and sync state. BrickEconomy provides AI/ML-powered set and minifig valuations. Rate limited to 100 requests per day.
CREATE TABLE [BMC].[BrickEconomyUserLink]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[encryptedApiKey] NVARCHAR(500) NULL,		-- Encrypted BrickEconomy Premium API key — encrypted via ASP.NET Data Protection
	[syncEnabled] BIT NOT NULL DEFAULT 0,		-- Whether automatic sync is enabled for this tenant
	[lastSyncDate] DATETIME2(7) NULL,		-- Date/time of the last successful sync operation
	[lastSyncError] NVARCHAR(1000) NULL,		-- Error message from the last failed sync attempt (null = no errors)
	[dailyQuotaUsed] INT NULL,		-- Number of API requests used today against the 100/day quota (reset at 00:00 UTC)
	[quotaResetDate] DATETIME2(7) NULL,		-- Date when the daily quota counter was last reset
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_BrickEconomyUserLink_tenantGuid] UNIQUE ( [tenantGuid]) 		-- Uniqueness enforced on the BrickEconomyUserLink table's tenantGuid field.
)
GO

-- Index on the BrickEconomyUserLink table's tenantGuid field.
CREATE INDEX [I_BrickEconomyUserLink_tenantGuid] ON [BMC].[BrickEconomyUserLink] ([tenantGuid])
GO

-- Index on the BrickEconomyUserLink table's tenantGuid,active fields.
CREATE INDEX [I_BrickEconomyUserLink_tenantGuid_active] ON [BMC].[BrickEconomyUserLink] ([tenantGuid], [active])
GO

-- Index on the BrickEconomyUserLink table's tenantGuid,deleted fields.
CREATE INDEX [I_BrickEconomyUserLink_tenantGuid_deleted] ON [BMC].[BrickEconomyUserLink] ([tenantGuid], [deleted])
GO


-- Stores per-tenant Brick Owl API key and sync state. Brick Owl is the second-largest LEGO marketplace, providing catalog lookups, cross-platform ID mapping, and collection management.
CREATE TABLE [BMC].[BrickOwlUserLink]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[encryptedApiKey] NVARCHAR(500) NULL,		-- Encrypted Brick Owl API key — encrypted via ASP.NET Data Protection
	[syncEnabled] BIT NOT NULL DEFAULT 0,		-- Whether automatic sync is enabled for this tenant
	[syncDirection] NVARCHAR(50) NULL,		-- Sync direction: Pull, Push, or Both (null = Pull)
	[lastSyncDate] DATETIME2(7) NULL,		-- Date/time of the last successful sync operation
	[lastPullDate] DATETIME2(7) NULL,		-- Date/time of the last successful pull from Brick Owl
	[lastPushDate] DATETIME2(7) NULL,		-- Date/time of the last successful push to Brick Owl
	[lastSyncError] NVARCHAR(1000) NULL,		-- Error message from the last failed sync attempt (null = no errors)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_BrickOwlUserLink_tenantGuid] UNIQUE ( [tenantGuid]) 		-- Uniqueness enforced on the BrickOwlUserLink table's tenantGuid field.
)
GO

-- Index on the BrickOwlUserLink table's tenantGuid field.
CREATE INDEX [I_BrickOwlUserLink_tenantGuid] ON [BMC].[BrickOwlUserLink] ([tenantGuid])
GO

-- Index on the BrickOwlUserLink table's tenantGuid,active fields.
CREATE INDEX [I_BrickOwlUserLink_tenantGuid_active] ON [BMC].[BrickOwlUserLink] ([tenantGuid], [active])
GO

-- Index on the BrickOwlUserLink table's tenantGuid,deleted fields.
CREATE INDEX [I_BrickOwlUserLink_tenantGuid_deleted] ON [BMC].[BrickOwlUserLink] ([tenantGuid], [deleted])
GO


-- Full audit log of every BrickLink API call BMC makes on behalf of a user. Mirrors the BrickSetTransaction pattern for complete transparency.
CREATE TABLE [BMC].[BrickLinkTransaction]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[transactionDate] DATETIME2(7) NULL,		-- Date/time the API call was made
	[direction] NVARCHAR(50) NOT NULL,		-- Direction of data flow: Push, Pull, Enrich
	[methodName] NVARCHAR(100) NOT NULL,		-- BrickLink API method name (e.g. 'getItem', 'getPriceGuide', 'getSubsets')
	[requestSummary] NVARCHAR(MAX) NULL,		-- Human-readable description of the operation
	[success] BIT NOT NULL DEFAULT 1,		-- Whether the API call completed successfully
	[errorMessage] NVARCHAR(MAX) NULL,		-- Error details if the call failed (null on success)
	[triggeredBy] NVARCHAR(100) NOT NULL,		-- What initiated this call: UserAction, SetDetailView, PriceGuide
	[recordCount] INT NULL,		-- Number of rows retrieved or affected by this API call
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the BrickLinkTransaction table's tenantGuid field.
CREATE INDEX [I_BrickLinkTransaction_tenantGuid] ON [BMC].[BrickLinkTransaction] ([tenantGuid])
GO

-- Index on the BrickLinkTransaction table's tenantGuid,active fields.
CREATE INDEX [I_BrickLinkTransaction_tenantGuid_active] ON [BMC].[BrickLinkTransaction] ([tenantGuid], [active])
GO

-- Index on the BrickLinkTransaction table's tenantGuid,deleted fields.
CREATE INDEX [I_BrickLinkTransaction_tenantGuid_deleted] ON [BMC].[BrickLinkTransaction] ([tenantGuid], [deleted])
GO


-- Full audit log of every BrickEconomy API call BMC makes on behalf of a user. Tracks daily quota usage for the 100-request-per-day limit.
CREATE TABLE [BMC].[BrickEconomyTransaction]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[transactionDate] DATETIME2(7) NULL,		-- Date/time the API call was made
	[direction] NVARCHAR(50) NOT NULL,		-- Direction of data flow: Pull, Enrich
	[methodName] NVARCHAR(100) NOT NULL,		-- BrickEconomy API method name (e.g. 'getSet', 'getMinifig', 'getSalesLedger')
	[requestSummary] NVARCHAR(MAX) NULL,		-- Human-readable description of the operation
	[success] BIT NOT NULL DEFAULT 1,		-- Whether the API call completed successfully
	[errorMessage] NVARCHAR(MAX) NULL,		-- Error details if the call failed (null on success)
	[triggeredBy] NVARCHAR(100) NOT NULL,		-- What initiated this call: UserAction, SetDetailView, Valuation
	[recordCount] INT NULL,		-- Number of rows retrieved or affected by this API call
	[dailyQuotaRemaining] INT NULL,		-- Daily API quota remaining after this call (out of 100)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the BrickEconomyTransaction table's tenantGuid field.
CREATE INDEX [I_BrickEconomyTransaction_tenantGuid] ON [BMC].[BrickEconomyTransaction] ([tenantGuid])
GO

-- Index on the BrickEconomyTransaction table's tenantGuid,active fields.
CREATE INDEX [I_BrickEconomyTransaction_tenantGuid_active] ON [BMC].[BrickEconomyTransaction] ([tenantGuid], [active])
GO

-- Index on the BrickEconomyTransaction table's tenantGuid,deleted fields.
CREATE INDEX [I_BrickEconomyTransaction_tenantGuid_deleted] ON [BMC].[BrickEconomyTransaction] ([tenantGuid], [deleted])
GO


-- Full audit log of every Brick Owl API call BMC makes on behalf of a user. Mirrors the BrickSetTransaction pattern for complete transparency.
CREATE TABLE [BMC].[BrickOwlTransaction]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[transactionDate] DATETIME2(7) NULL,		-- Date/time the API call was made
	[direction] NVARCHAR(50) NOT NULL,		-- Direction of data flow: Push, Pull
	[methodName] NVARCHAR(100) NOT NULL,		-- Brick Owl API method name (e.g. 'catalogLookup', 'idLookup', 'getCollection')
	[requestSummary] NVARCHAR(MAX) NULL,		-- Human-readable description of the operation
	[success] BIT NOT NULL DEFAULT 1,		-- Whether the API call completed successfully
	[errorMessage] NVARCHAR(MAX) NULL,		-- Error details if the call failed (null on success)
	[triggeredBy] NVARCHAR(100) NOT NULL,		-- What initiated this call: UserAction, CatalogLookup, IdMapping
	[recordCount] INT NULL,		-- Number of rows retrieved or affected by this API call
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the BrickOwlTransaction table's tenantGuid field.
CREATE INDEX [I_BrickOwlTransaction_tenantGuid] ON [BMC].[BrickOwlTransaction] ([tenantGuid])
GO

-- Index on the BrickOwlTransaction table's tenantGuid,active fields.
CREATE INDEX [I_BrickOwlTransaction_tenantGuid_active] ON [BMC].[BrickOwlTransaction] ([tenantGuid], [active])
GO

-- Index on the BrickOwlTransaction table's tenantGuid,deleted fields.
CREATE INDEX [I_BrickOwlTransaction_tenantGuid_deleted] ON [BMC].[BrickOwlTransaction] ([tenantGuid], [deleted])
GO


-- Caches API responses from external marketplaces (BrickLink, BrickEconomy, BrickOwl) to reduce external calls and improve Brickberg Terminal performance. TTL-based expiry with configurable per-source cache durations. Not multi-tenant — cache is shared across all users for the same item.
CREATE TABLE [BMC].[MarketDataCache]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[source] NVARCHAR(50) NOT NULL,		-- Marketplace source: BrickLink, BrickEconomy, BrickOwl
	[itemType] NVARCHAR(50) NOT NULL,		-- Item type: SET, MINIFIG, PART
	[itemNumber] NVARCHAR(100) NOT NULL,		-- Item identifier (e.g. '42131-1', 'fig-001234')
	[condition] NVARCHAR(50) NULL,		-- Condition qualifier: N (new), U (used), null for non-BrickLink sources
	[responseJson] NVARCHAR(MAX) NULL,		-- Serialized JSON API response payload
	[fetchedDate] DATETIME2(7) NOT NULL,		-- UTC timestamp when the response was fetched from the source API
	[expiresDate] DATETIME2(7) NOT NULL,		-- UTC timestamp when this cache entry expires and should be refreshed
	[ttlMinutes] INT NOT NULL,		-- TTL in minutes that was used when caching this entry (for diagnostics/auditing)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_MarketDataCache_source_itemType_itemNumber_condition] UNIQUE ( [source], [itemType], [itemNumber], [condition]) 		-- Uniqueness enforced on the MarketDataCache table's source and itemType and itemNumber and condition fields.
)
GO

-- Index on the MarketDataCache table's active field.
CREATE INDEX [I_MarketDataCache_active] ON [BMC].[MarketDataCache] ([active])
GO

-- Index on the MarketDataCache table's deleted field.
CREATE INDEX [I_MarketDataCache_deleted] ON [BMC].[MarketDataCache] ([deleted])
GO


