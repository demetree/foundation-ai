/*
Community CMS database schema.
This is the content management module for the public-facing community website.
It provides page management, blog publishing, media library, navigation menus,
announcements, photo galleries, document downloads, contact form handling,
and site-wide configuration.
All operational tables include auditing and security controls.
*/
CREATE DATABASE [Community]
GO

ALTER DATABASE [Community] SET RECOVERY SIMPLE
GO

USE [Community]
GO

CREATE SCHEMA [Community]
GO

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE [Community].[ContactSubmission]
-- DROP TABLE [Community].[DocumentDownload]
-- DROP TABLE [Community].[GalleryImage]
-- DROP TABLE [Community].[GalleryAlbum]
-- DROP TABLE [Community].[AnnouncementChangeHistory]
-- DROP TABLE [Community].[Announcement]
-- DROP TABLE [Community].[SiteSetting]
-- DROP TABLE [Community].[MenuItem]
-- DROP TABLE [Community].[Menu]
-- DROP TABLE [Community].[MediaContent]
-- DROP TABLE [Community].[MediaAsset]
-- DROP TABLE [Community].[PostTagAssignment]
-- DROP TABLE [Community].[PostTag]
-- DROP TABLE [Community].[PostChangeHistory]
-- DROP TABLE [Community].[Post]
-- DROP TABLE [Community].[PostCategory]
-- DROP TABLE [Community].[PageChangeHistory]
-- DROP TABLE [Community].[Page]

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON [Community].[ContactSubmission] DISABLE
-- ALTER INDEX ALL ON [Community].[DocumentDownload] DISABLE
-- ALTER INDEX ALL ON [Community].[GalleryImage] DISABLE
-- ALTER INDEX ALL ON [Community].[GalleryAlbum] DISABLE
-- ALTER INDEX ALL ON [Community].[AnnouncementChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Community].[Announcement] DISABLE
-- ALTER INDEX ALL ON [Community].[SiteSetting] DISABLE
-- ALTER INDEX ALL ON [Community].[MenuItem] DISABLE
-- ALTER INDEX ALL ON [Community].[Menu] DISABLE
-- ALTER INDEX ALL ON [Community].[MediaContent] DISABLE
-- ALTER INDEX ALL ON [Community].[MediaAsset] DISABLE
-- ALTER INDEX ALL ON [Community].[PostTagAssignment] DISABLE
-- ALTER INDEX ALL ON [Community].[PostTag] DISABLE
-- ALTER INDEX ALL ON [Community].[PostChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Community].[Post] DISABLE
-- ALTER INDEX ALL ON [Community].[PostCategory] DISABLE
-- ALTER INDEX ALL ON [Community].[PageChangeHistory] DISABLE
-- ALTER INDEX ALL ON [Community].[Page] DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON [Community].[ContactSubmission] REBUILD
-- ALTER INDEX ALL ON [Community].[DocumentDownload] REBUILD
-- ALTER INDEX ALL ON [Community].[GalleryImage] REBUILD
-- ALTER INDEX ALL ON [Community].[GalleryAlbum] REBUILD
-- ALTER INDEX ALL ON [Community].[AnnouncementChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Community].[Announcement] REBUILD
-- ALTER INDEX ALL ON [Community].[SiteSetting] REBUILD
-- ALTER INDEX ALL ON [Community].[MenuItem] REBUILD
-- ALTER INDEX ALL ON [Community].[Menu] REBUILD
-- ALTER INDEX ALL ON [Community].[MediaContent] REBUILD
-- ALTER INDEX ALL ON [Community].[MediaAsset] REBUILD
-- ALTER INDEX ALL ON [Community].[PostTagAssignment] REBUILD
-- ALTER INDEX ALL ON [Community].[PostTag] REBUILD
-- ALTER INDEX ALL ON [Community].[PostChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Community].[Post] REBUILD
-- ALTER INDEX ALL ON [Community].[PostCategory] REBUILD
-- ALTER INDEX ALL ON [Community].[PageChangeHistory] REBUILD
-- ALTER INDEX ALL ON [Community].[Page] REBUILD

-- Static content pages for the public website (e.g. About, History, FAQ, Contact, Regulations).
CREATE TABLE [Community].[Page]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[title] NVARCHAR(250) NOT NULL,		-- Display title of the page
	[slug] NVARCHAR(250) NOT NULL,		-- URL-friendly slug (e.g. 'about', 'history', 'faq')
	[body] NVARCHAR(MAX) NULL,		-- HTML or Markdown body content of the page
	[metaDescription] NVARCHAR(500) NULL,		-- SEO meta description for search engines and social sharing
	[featuredImageUrl] NVARCHAR(500) NULL,		-- URL or relative path to the page's featured/hero image
	[isPublished] BIT NOT NULL DEFAULT 0,		-- Whether this page is visible on the public site
	[publishedDate] DATETIME2(7) NULL,		-- When the page was first published (null if draft)
	[sortOrder] INT NULL DEFAULT 0,		-- Sort order for page listings and navigation
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the Page table's tenantGuid field.
CREATE INDEX [I_Page_tenantGuid] ON [Community].[Page] ([tenantGuid])
GO

-- Index on the Page table's tenantGuid,slug fields.
CREATE UNIQUE INDEX [I_Page_tenantGuid_slug] ON [Community].[Page] ([tenantGuid], [slug])
GO

-- Index on the Page table's tenantGuid,active fields.
CREATE INDEX [I_Page_tenantGuid_active] ON [Community].[Page] ([tenantGuid], [active])
GO

-- Index on the Page table's tenantGuid,deleted fields.
CREATE INDEX [I_Page_tenantGuid_deleted] ON [Community].[Page] ([tenantGuid], [deleted])
GO


-- The change history for records from the Page table.
CREATE TABLE [Community].[PageChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[pageId] INT NOT NULL,		-- Link to the Page table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_PageChangeHistory_Page_pageId] FOREIGN KEY ([pageId]) REFERENCES [Community].[Page] ([id])		-- Foreign key to the Page table.
)
GO

-- Index on the PageChangeHistory table's tenantGuid field.
CREATE INDEX [I_PageChangeHistory_tenantGuid] ON [Community].[PageChangeHistory] ([tenantGuid])
GO

-- Index on the PageChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_PageChangeHistory_tenantGuid_versionNumber] ON [Community].[PageChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the PageChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_PageChangeHistory_tenantGuid_timeStamp] ON [Community].[PageChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the PageChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_PageChangeHistory_tenantGuid_userId] ON [Community].[PageChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the PageChangeHistory table's tenantGuid,pageId fields.
CREATE INDEX [I_PageChangeHistory_tenantGuid_pageId] ON [Community].[PageChangeHistory] ([tenantGuid], [pageId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Category taxonomy for organizing blog/news posts (e.g. News, Council Updates, Community Events).
CREATE TABLE [Community].[PostCategory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[description] NVARCHAR(500) NULL,
	[slug] NVARCHAR(250) NOT NULL,		-- URL-friendly slug for the category
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the PostCategory table's name field.
CREATE INDEX [I_PostCategory_name] ON [Community].[PostCategory] ([name])
GO

-- Index on the PostCategory table's slug field.
CREATE UNIQUE INDEX [I_PostCategory_slug] ON [Community].[PostCategory] ([slug])
GO

-- Index on the PostCategory table's active field.
CREATE INDEX [I_PostCategory_active] ON [Community].[PostCategory] ([active])
GO

-- Index on the PostCategory table's deleted field.
CREATE INDEX [I_PostCategory_deleted] ON [Community].[PostCategory] ([deleted])
GO

INSERT INTO [Community].[PostCategory] ( [name], [description], [slug], [sequence], [objectGuid] ) VALUES  ( 'News', 'General news and updates', 'news', 1, 'c0a10001-0001-4000-8000-000000000001' )
GO

INSERT INTO [Community].[PostCategory] ( [name], [description], [slug], [sequence], [objectGuid] ) VALUES  ( 'Council Updates', 'Updates from council meetings and decisions', 'council-updates', 2, 'c0a10001-0001-4000-8000-000000000002' )
GO

INSERT INTO [Community].[PostCategory] ( [name], [description], [slug], [sequence], [objectGuid] ) VALUES  ( 'Community Events', 'Upcoming and past community events', 'community-events', 3, 'c0a10001-0001-4000-8000-000000000003' )
GO

INSERT INTO [Community].[PostCategory] ( [name], [description], [slug], [sequence], [objectGuid] ) VALUES  ( 'Volunteer Spotlight', 'Highlighting volunteer contributions and stories', 'volunteer-spotlight', 4, 'c0a10001-0001-4000-8000-000000000004' )
GO

INSERT INTO [Community].[PostCategory] ( [name], [description], [slug], [sequence], [objectGuid] ) VALUES  ( 'Announcements', 'Official announcements and notices', 'announcements', 5, 'c0a10001-0001-4000-8000-000000000005' )
GO


-- Blog and news articles for the public website. Supports categories, featured images, and publish scheduling.
CREATE TABLE [Community].[Post]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[title] NVARCHAR(250) NOT NULL,		-- Display title of the post
	[slug] NVARCHAR(250) NOT NULL,		-- URL-friendly slug for the post
	[body] NVARCHAR(MAX) NULL,		-- Full HTML or Markdown body content of the post
	[excerpt] NVARCHAR(500) NULL,		-- Short summary for post listings and SEO (manually entered or auto-generated)
	[authorName] NVARCHAR(100) NULL,		-- Display name of the post author
	[postCategoryId] INT NULL,		-- Category this post belongs to (null for uncategorized)
	[featuredImageUrl] NVARCHAR(500) NULL,		-- URL or relative path to the post's featured image
	[metaDescription] NVARCHAR(500) NULL,		-- SEO meta description for search engines
	[isPublished] BIT NOT NULL DEFAULT 0,		-- Whether this post is visible on the public site
	[publishedDate] DATETIME2(7) NULL,		-- When the post was published (null if draft, allows future-dating for scheduled publishing)
	[isFeatured] BIT NOT NULL DEFAULT 0,		-- Whether this post should appear in featured/highlighted sections
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_Post_PostCategory_postCategoryId] FOREIGN KEY ([postCategoryId]) REFERENCES [Community].[PostCategory] ([id])		-- Foreign key to the PostCategory table.
)
GO

-- Index on the Post table's tenantGuid field.
CREATE INDEX [I_Post_tenantGuid] ON [Community].[Post] ([tenantGuid])
GO

-- Index on the Post table's tenantGuid,title fields.
CREATE INDEX [I_Post_tenantGuid_title] ON [Community].[Post] ([tenantGuid], [title])
GO

-- Index on the Post table's tenantGuid,slug fields.
CREATE UNIQUE INDEX [I_Post_tenantGuid_slug] ON [Community].[Post] ([tenantGuid], [slug])
GO

-- Index on the Post table's tenantGuid,postCategoryId fields.
CREATE INDEX [I_Post_tenantGuid_postCategoryId] ON [Community].[Post] ([tenantGuid], [postCategoryId])
GO

-- Index on the Post table's tenantGuid,active fields.
CREATE INDEX [I_Post_tenantGuid_active] ON [Community].[Post] ([tenantGuid], [active])
GO

-- Index on the Post table's tenantGuid,deleted fields.
CREATE INDEX [I_Post_tenantGuid_deleted] ON [Community].[Post] ([tenantGuid], [deleted])
GO


-- The change history for records from the Post table.
CREATE TABLE [Community].[PostChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[postId] INT NOT NULL,		-- Link to the Post table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_PostChangeHistory_Post_postId] FOREIGN KEY ([postId]) REFERENCES [Community].[Post] ([id])		-- Foreign key to the Post table.
)
GO

-- Index on the PostChangeHistory table's tenantGuid field.
CREATE INDEX [I_PostChangeHistory_tenantGuid] ON [Community].[PostChangeHistory] ([tenantGuid])
GO

-- Index on the PostChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_PostChangeHistory_tenantGuid_versionNumber] ON [Community].[PostChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the PostChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_PostChangeHistory_tenantGuid_timeStamp] ON [Community].[PostChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the PostChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_PostChangeHistory_tenantGuid_userId] ON [Community].[PostChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the PostChangeHistory table's tenantGuid,postId fields.
CREATE INDEX [I_PostChangeHistory_tenantGuid_postId] ON [Community].[PostChangeHistory] ([tenantGuid], [postId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Freeform tags for cross-cutting post categorization (e.g. urgent, budget, recycling).
CREATE TABLE [Community].[PostTag]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[slug] NVARCHAR(250) NOT NULL,		-- URL-friendly slug for the tag
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_PostTag_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the PostTag table's tenantGuid and name fields.
)
GO

-- Index on the PostTag table's tenantGuid field.
CREATE INDEX [I_PostTag_tenantGuid] ON [Community].[PostTag] ([tenantGuid])
GO

-- Index on the PostTag table's tenantGuid,name fields.
CREATE INDEX [I_PostTag_tenantGuid_name] ON [Community].[PostTag] ([tenantGuid], [name])
GO

-- Index on the PostTag table's tenantGuid,slug fields.
CREATE UNIQUE INDEX [I_PostTag_tenantGuid_slug] ON [Community].[PostTag] ([tenantGuid], [slug])
GO

-- Index on the PostTag table's tenantGuid,active fields.
CREATE INDEX [I_PostTag_tenantGuid_active] ON [Community].[PostTag] ([tenantGuid], [active])
GO

-- Index on the PostTag table's tenantGuid,deleted fields.
CREATE INDEX [I_PostTag_tenantGuid_deleted] ON [Community].[PostTag] ([tenantGuid], [deleted])
GO


-- Many-to-many mapping between posts and tags.
CREATE TABLE [Community].[PostTagAssignment]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[postId] INT NOT NULL,		-- The post being tagged
	[postTagId] INT NOT NULL,		-- The tag applied to the post
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_PostTagAssignment_Post_postId] FOREIGN KEY ([postId]) REFERENCES [Community].[Post] ([id]),		-- Foreign key to the Post table.
	CONSTRAINT [FK_PostTagAssignment_PostTag_postTagId] FOREIGN KEY ([postTagId]) REFERENCES [Community].[PostTag] ([id]),		-- Foreign key to the PostTag table.
	CONSTRAINT [UC_PostTagAssignment_postId_postTagId] UNIQUE ( [postId], [postTagId]) 		-- Uniqueness enforced on the PostTagAssignment table's postId and postTagId fields.
)
GO

-- Index on the PostTagAssignment table's postId field.
CREATE INDEX [I_PostTagAssignment_postId] ON [Community].[PostTagAssignment] ([postId])
GO

-- Index on the PostTagAssignment table's postTagId field.
CREATE INDEX [I_PostTagAssignment_postTagId] ON [Community].[PostTagAssignment] ([postTagId])
GO

-- Index on the PostTagAssignment table's active field.
CREATE INDEX [I_PostTagAssignment_active] ON [Community].[PostTagAssignment] ([active])
GO

-- Index on the PostTagAssignment table's deleted field.
CREATE INDEX [I_PostTagAssignment_deleted] ON [Community].[PostTagAssignment] ([deleted])
GO


-- Centralized media library for uploaded images, documents, and other files used across the CMS.
CREATE TABLE [Community].[MediaAsset]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[fileName] NVARCHAR(250) NOT NULL,		-- Original filename as uploaded (e.g. 'council-photo-2026.jpg')
	[filePath] NVARCHAR(500) NOT NULL,		-- Server-relative storage path for the file
	[mimeType] NVARCHAR(100) NOT NULL,		-- MIME type (e.g. 'image/jpeg', 'application/pdf')
	[altText] NVARCHAR(250) NULL,		-- Alt text for accessibility and SEO (images)
	[caption] NVARCHAR(500) NULL,		-- Display caption for the media item
	[fileSizeBytes] BIGINT NULL,		-- File size in bytes
	[imageWidth] INT NULL,		-- Width in pixels (for images only)
	[imageHeight] INT NULL,		-- Height in pixels (for images only)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the MediaAsset table's tenantGuid field.
CREATE INDEX [I_MediaAsset_tenantGuid] ON [Community].[MediaAsset] ([tenantGuid])
GO

-- Index on the MediaAsset table's tenantGuid,active fields.
CREATE INDEX [I_MediaAsset_tenantGuid_active] ON [Community].[MediaAsset] ([tenantGuid], [active])
GO

-- Index on the MediaAsset table's tenantGuid,deleted fields.
CREATE INDEX [I_MediaAsset_tenantGuid_deleted] ON [Community].[MediaAsset] ([tenantGuid], [deleted])
GO


-- Binary storage for media asset file data. Separated from MediaAsset to keep metadata queries lightweight. One-to-one relationship with MediaAsset.
CREATE TABLE [Community].[MediaContent]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[mediaAssetId] INT NOT NULL,		-- The media asset this content belongs to
	[fileData] VARBINARY(MAX) NOT NULL,		-- Binary file content (varbinary MAX)
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_MediaContent_MediaAsset_mediaAssetId] FOREIGN KEY ([mediaAssetId]) REFERENCES [Community].[MediaAsset] ([id])		-- Foreign key to the MediaAsset table.
)
GO

-- Index on the MediaContent table's tenantGuid field.
CREATE INDEX [I_MediaContent_tenantGuid] ON [Community].[MediaContent] ([tenantGuid])
GO

-- Index on the MediaContent table's tenantGuid,mediaAssetId fields.
CREATE INDEX [I_MediaContent_tenantGuid_mediaAssetId] ON [Community].[MediaContent] ([tenantGuid], [mediaAssetId])
GO

-- Index on the MediaContent table's tenantGuid,active fields.
CREATE INDEX [I_MediaContent_tenantGuid_active] ON [Community].[MediaContent] ([tenantGuid], [active])
GO

-- Index on the MediaContent table's tenantGuid,deleted fields.
CREATE INDEX [I_MediaContent_tenantGuid_deleted] ON [Community].[MediaContent] ([tenantGuid], [deleted])
GO


-- Named navigation menus for different positions on the site (e.g. header, footer, sidebar).
CREATE TABLE [Community].[Menu]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,
	[location] NVARCHAR(50) NOT NULL,		-- Where this menu is displayed: header, footer, sidebar
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [UC_Menu_tenantGuid_name] UNIQUE ( [tenantGuid], [name]) 		-- Uniqueness enforced on the Menu table's tenantGuid and name fields.
)
GO

-- Index on the Menu table's tenantGuid field.
CREATE INDEX [I_Menu_tenantGuid] ON [Community].[Menu] ([tenantGuid])
GO

-- Index on the Menu table's tenantGuid,name fields.
CREATE INDEX [I_Menu_tenantGuid_name] ON [Community].[Menu] ([tenantGuid], [name])
GO

-- Index on the Menu table's tenantGuid,active fields.
CREATE INDEX [I_Menu_tenantGuid_active] ON [Community].[Menu] ([tenantGuid], [active])
GO

-- Index on the Menu table's tenantGuid,deleted fields.
CREATE INDEX [I_Menu_tenantGuid_deleted] ON [Community].[Menu] ([tenantGuid], [deleted])
GO

INSERT INTO [Community].[Menu] ( [tenantGuid], [name], [location], [objectGuid] ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'Main Navigation', 'header', 'c0b10001-0001-4000-8000-000000000001' )
GO

INSERT INTO [Community].[Menu] ( [tenantGuid], [name], [location], [objectGuid] ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'Footer Links', 'footer', 'c0b10001-0001-4000-8000-000000000002' )
GO


-- Individual navigation items within a menu. Supports tree hierarchy via self-referencing parent FK.
CREATE TABLE [Community].[MenuItem]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[menuId] INT NOT NULL,		-- The menu this item belongs to
	[label] NVARCHAR(250) NOT NULL,		-- Display text for the menu item
	[url] NVARCHAR(500) NULL,		-- External or absolute URL (used if pageId is null)
	[pageId] INT NULL,		-- Optional link to an internal CMS page (takes priority over url)
	[parentMenuItemId] INT NULL,		-- Parent menu item for sub-menu hierarchy (null = top-level)
	[iconClass] NVARCHAR(50) NULL,		-- Optional CSS icon class (e.g. 'fa-home', 'bi-calendar')
	[openInNewTab] BIT NOT NULL DEFAULT 0,		-- Whether clicking this menu item opens in a new browser tab
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_MenuItem_Menu_menuId] FOREIGN KEY ([menuId]) REFERENCES [Community].[Menu] ([id]),		-- Foreign key to the Menu table.
	CONSTRAINT [FK_MenuItem_Page_pageId] FOREIGN KEY ([pageId]) REFERENCES [Community].[Page] ([id]),		-- Foreign key to the Page table.
	CONSTRAINT [FK_MenuItem_MenuItem_parentMenuItemId] FOREIGN KEY ([parentMenuItemId]) REFERENCES [Community].[MenuItem] ([id])		-- Foreign key to the MenuItem table.
)
GO

-- Index on the MenuItem table's tenantGuid field.
CREATE INDEX [I_MenuItem_tenantGuid] ON [Community].[MenuItem] ([tenantGuid])
GO

-- Index on the MenuItem table's tenantGuid,menuId fields.
CREATE INDEX [I_MenuItem_tenantGuid_menuId] ON [Community].[MenuItem] ([tenantGuid], [menuId])
GO

-- Index on the MenuItem table's tenantGuid,pageId fields.
CREATE INDEX [I_MenuItem_tenantGuid_pageId] ON [Community].[MenuItem] ([tenantGuid], [pageId])
GO

-- Index on the MenuItem table's tenantGuid,parentMenuItemId fields.
CREATE INDEX [I_MenuItem_tenantGuid_parentMenuItemId] ON [Community].[MenuItem] ([tenantGuid], [parentMenuItemId])
GO

-- Index on the MenuItem table's tenantGuid,active fields.
CREATE INDEX [I_MenuItem_tenantGuid_active] ON [Community].[MenuItem] ([tenantGuid], [active])
GO

-- Index on the MenuItem table's tenantGuid,deleted fields.
CREATE INDEX [I_MenuItem_tenantGuid_deleted] ON [Community].[MenuItem] ([tenantGuid], [deleted])
GO


-- Key-value configuration for the public site (site name, tagline, logo URL, social media links, theme settings).
CREATE TABLE [Community].[SiteSetting]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[settingKey] NVARCHAR(100) NOT NULL,		-- Unique setting identifier (e.g. 'siteName', 'tagline', 'logoUrl')
	[settingValue] NVARCHAR(MAX) NULL,		-- The value for this setting
	[description] NVARCHAR(250) NULL,		-- Human-readable description of what this setting controls
	[settingGroup] NVARCHAR(50) NULL,		-- Grouping for settings UI (e.g. 'General', 'Social', 'SEO', 'Theme')
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the SiteSetting table's tenantGuid field.
CREATE INDEX [I_SiteSetting_tenantGuid] ON [Community].[SiteSetting] ([tenantGuid])
GO

-- Index on the SiteSetting table's tenantGuid,settingKey fields.
CREATE UNIQUE INDEX [I_SiteSetting_tenantGuid_settingKey] ON [Community].[SiteSetting] ([tenantGuid], [settingKey])
GO

-- Index on the SiteSetting table's tenantGuid,active fields.
CREATE INDEX [I_SiteSetting_tenantGuid_active] ON [Community].[SiteSetting] ([tenantGuid], [active])
GO

-- Index on the SiteSetting table's tenantGuid,deleted fields.
CREATE INDEX [I_SiteSetting_tenantGuid_deleted] ON [Community].[SiteSetting] ([tenantGuid], [deleted])
GO

INSERT INTO [Community].[SiteSetting] ( [tenantGuid], [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'siteName', 'Community', 'The name of the site displayed in the header and browser tab', 'General', 'c0c10001-0001-4000-8000-000000000001' )
GO

INSERT INTO [Community].[SiteSetting] ( [tenantGuid], [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'tagline', 'Welcome to our community', 'Site tagline displayed below the site name', 'General', 'c0c10001-0001-4000-8000-000000000002' )
GO

INSERT INTO [Community].[SiteSetting] ( [tenantGuid], [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'logoUrl', '', 'URL to the site logo image', 'General', 'c0c10001-0001-4000-8000-000000000003' )
GO

INSERT INTO [Community].[SiteSetting] ( [tenantGuid], [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'footerText', '© 2026 K2 Research. All rights reserved.', 'Copyright text displayed in the site footer', 'General', 'c0c10001-0001-4000-8000-000000000004' )
GO

INSERT INTO [Community].[SiteSetting] ( [tenantGuid], [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'contactEmail', '', 'Primary contact email address', 'General', 'c0c10001-0001-4000-8000-000000000005' )
GO

INSERT INTO [Community].[SiteSetting] ( [tenantGuid], [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'contactPhone', '', 'Primary contact phone number', 'General', 'c0c10001-0001-4000-8000-000000000006' )
GO

INSERT INTO [Community].[SiteSetting] ( [tenantGuid], [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'facebookUrl', '', 'Facebook page URL', 'Social', 'c0c10001-0001-4000-8000-000000000010' )
GO

INSERT INTO [Community].[SiteSetting] ( [tenantGuid], [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'twitterUrl', '', 'Twitter/X profile URL', 'Social', 'c0c10001-0001-4000-8000-000000000011' )
GO

INSERT INTO [Community].[SiteSetting] ( [tenantGuid], [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'instagramUrl', '', 'Instagram profile URL', 'Social', 'c0c10001-0001-4000-8000-000000000012' )
GO

INSERT INTO [Community].[SiteSetting] ( [tenantGuid], [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'heroTitle', 'Welcome', 'Hero section title on the home page', 'HomePage', 'c0c10001-0001-4000-8000-000000000020' )
GO

INSERT INTO [Community].[SiteSetting] ( [tenantGuid], [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'heroSubtitle', '', 'Hero section subtitle on the home page', 'HomePage', 'c0c10001-0001-4000-8000-000000000021' )
GO

INSERT INTO [Community].[SiteSetting] ( [tenantGuid], [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'heroImageUrl', '', 'Hero section background image URL', 'HomePage', 'c0c10001-0001-4000-8000-000000000022' )
GO


-- Time-bound announcements displayed on the public site (council meeting notices, public consultations, service disruptions).
CREATE TABLE [Community].[Announcement]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[title] NVARCHAR(250) NOT NULL,		-- Announcement title
	[body] NVARCHAR(MAX) NULL,		-- Full announcement body content (HTML or Markdown)
	[severity] NVARCHAR(50) NOT NULL DEFAULT 'info',		-- Severity level: info, warning, urgent
	[startDate] DATETIME2(7) NOT NULL,		-- When the announcement becomes visible
	[endDate] DATETIME2(7) NULL,		-- When the announcement expires (null = no expiration)
	[isPinned] BIT NOT NULL DEFAULT 0,		-- Whether this announcement should always appear at the top
	[versionNumber] INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the Announcement table's tenantGuid field.
CREATE INDEX [I_Announcement_tenantGuid] ON [Community].[Announcement] ([tenantGuid])
GO

-- Index on the Announcement table's tenantGuid,active fields.
CREATE INDEX [I_Announcement_tenantGuid_active] ON [Community].[Announcement] ([tenantGuid], [active])
GO

-- Index on the Announcement table's tenantGuid,deleted fields.
CREATE INDEX [I_Announcement_tenantGuid_deleted] ON [Community].[Announcement] ([tenantGuid], [deleted])
GO


-- The change history for records from the Announcement table.
CREATE TABLE [Community].[AnnouncementChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[announcementId] INT NOT NULL,		-- Link to the Announcement table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_AnnouncementChangeHistory_Announcement_announcementId] FOREIGN KEY ([announcementId]) REFERENCES [Community].[Announcement] ([id])		-- Foreign key to the Announcement table.
)
GO

-- Index on the AnnouncementChangeHistory table's tenantGuid field.
CREATE INDEX [I_AnnouncementChangeHistory_tenantGuid] ON [Community].[AnnouncementChangeHistory] ([tenantGuid])
GO

-- Index on the AnnouncementChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX [I_AnnouncementChangeHistory_tenantGuid_versionNumber] ON [Community].[AnnouncementChangeHistory] ([tenantGuid], [versionNumber])
GO

-- Index on the AnnouncementChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX [I_AnnouncementChangeHistory_tenantGuid_timeStamp] ON [Community].[AnnouncementChangeHistory] ([tenantGuid], [timeStamp])
GO

-- Index on the AnnouncementChangeHistory table's tenantGuid,userId fields.
CREATE INDEX [I_AnnouncementChangeHistory_tenantGuid_userId] ON [Community].[AnnouncementChangeHistory] ([tenantGuid], [userId])
GO

-- Index on the AnnouncementChangeHistory table's tenantGuid,announcementId fields.
CREATE INDEX [I_AnnouncementChangeHistory_tenantGuid_announcementId] ON [Community].[AnnouncementChangeHistory] ([tenantGuid], [announcementId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Photo albums for organizing gallery images (e.g. 'Community Day 2026', 'Town Views', 'Heritage Photos').
CREATE TABLE [Community].[GalleryAlbum]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[title] NVARCHAR(250) NOT NULL,		-- Album title
	[slug] NVARCHAR(250) NOT NULL,		-- URL-friendly slug for the album
	[description] NVARCHAR(MAX) NULL,		-- Description of the album
	[coverImageUrl] NVARCHAR(500) NULL,		-- URL to the album cover image (or uses first image if null)
	[isPublished] BIT NOT NULL DEFAULT 1,		-- Whether this album is visible on the public site
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the GalleryAlbum table's tenantGuid field.
CREATE INDEX [I_GalleryAlbum_tenantGuid] ON [Community].[GalleryAlbum] ([tenantGuid])
GO

-- Index on the GalleryAlbum table's tenantGuid,slug fields.
CREATE UNIQUE INDEX [I_GalleryAlbum_tenantGuid_slug] ON [Community].[GalleryAlbum] ([tenantGuid], [slug])
GO

-- Index on the GalleryAlbum table's tenantGuid,active fields.
CREATE INDEX [I_GalleryAlbum_tenantGuid_active] ON [Community].[GalleryAlbum] ([tenantGuid], [active])
GO

-- Index on the GalleryAlbum table's tenantGuid,deleted fields.
CREATE INDEX [I_GalleryAlbum_tenantGuid_deleted] ON [Community].[GalleryAlbum] ([tenantGuid], [deleted])
GO


-- Individual images within a gallery album.
CREATE TABLE [Community].[GalleryImage]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[galleryAlbumId] INT NOT NULL,		-- The album this image belongs to
	[imageUrl] NVARCHAR(500) NOT NULL,		-- URL or relative path to the image file
	[caption] NVARCHAR(500) NULL,		-- Display caption for the image
	[altText] NVARCHAR(250) NULL,		-- Alt text for accessibility
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

	CONSTRAINT [FK_GalleryImage_GalleryAlbum_galleryAlbumId] FOREIGN KEY ([galleryAlbumId]) REFERENCES [Community].[GalleryAlbum] ([id])		-- Foreign key to the GalleryAlbum table.
)
GO

-- Index on the GalleryImage table's tenantGuid field.
CREATE INDEX [I_GalleryImage_tenantGuid] ON [Community].[GalleryImage] ([tenantGuid])
GO

-- Index on the GalleryImage table's tenantGuid,galleryAlbumId fields.
CREATE INDEX [I_GalleryImage_tenantGuid_galleryAlbumId] ON [Community].[GalleryImage] ([tenantGuid], [galleryAlbumId])
GO

-- Index on the GalleryImage table's tenantGuid,active fields.
CREATE INDEX [I_GalleryImage_tenantGuid_active] ON [Community].[GalleryImage] ([tenantGuid], [active])
GO

-- Index on the GalleryImage table's tenantGuid,deleted fields.
CREATE INDEX [I_GalleryImage_tenantGuid_deleted] ON [Community].[GalleryImage] ([tenantGuid], [deleted])
GO


-- Downloadable documents for public access (council minutes, schedules, forms, regulations). Replaces the PDF-as-image pattern.
CREATE TABLE [Community].[DocumentDownload]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[title] NVARCHAR(250) NOT NULL,		-- Display title for the document
	[description] NVARCHAR(MAX) NULL,		-- Description of what this document contains
	[filePath] NVARCHAR(500) NOT NULL,		-- Server-relative path to the downloadable file
	[fileName] NVARCHAR(100) NOT NULL,		-- Original filename for download (e.g. 'recycling-schedule-2026.pdf')
	[mimeType] NVARCHAR(100) NULL,		-- File MIME type
	[fileSizeBytes] BIGINT NULL,		-- File size in bytes
	[categoryName] NVARCHAR(100) NULL,		-- Grouping category (e.g. 'Council Minutes', 'Schedules', 'Forms', 'Regulations')
	[documentDate] DATETIME2(7) NULL,		-- Date associated with the document (e.g. meeting date for council minutes)
	[isPublished] BIT NOT NULL DEFAULT 1,		-- Whether this document is visible for public download
	[sequence] INT NULL,		-- Sequence to use for sorting.
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the DocumentDownload table's tenantGuid field.
CREATE INDEX [I_DocumentDownload_tenantGuid] ON [Community].[DocumentDownload] ([tenantGuid])
GO

-- Index on the DocumentDownload table's tenantGuid,active fields.
CREATE INDEX [I_DocumentDownload_tenantGuid_active] ON [Community].[DocumentDownload] ([tenantGuid], [active])
GO

-- Index on the DocumentDownload table's tenantGuid,deleted fields.
CREATE INDEX [I_DocumentDownload_tenantGuid_deleted] ON [Community].[DocumentDownload] ([tenantGuid], [deleted])
GO


-- Captures contact form submissions from public site visitors.
CREATE TABLE [Community].[ContactSubmission]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[tenantGuid] UNIQUEIDENTIFIER NOT NULL,		-- The guid for the Tenant to which this record belongs.
	[name] NVARCHAR(100) NOT NULL,		-- Name of the person submitting the form
	[email] NVARCHAR(250) NOT NULL,		-- Email address for reply
	[subject] NVARCHAR(250) NULL,		-- Subject line of the message
	[message] NVARCHAR(MAX) NOT NULL,		-- Full message body
	[submittedDate] DATETIME2(7) NOT NULL,		-- When the form was submitted
	[isRead] BIT NOT NULL DEFAULT 0,		-- Whether an admin has read this submission
	[isArchived] BIT NOT NULL DEFAULT 0,		-- Whether this submission has been archived
	[adminNotes] NVARCHAR(500) NULL,		-- Internal notes from admin about the submission
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the ContactSubmission table's tenantGuid field.
CREATE INDEX [I_ContactSubmission_tenantGuid] ON [Community].[ContactSubmission] ([tenantGuid])
GO

-- Index on the ContactSubmission table's tenantGuid,active fields.
CREATE INDEX [I_ContactSubmission_tenantGuid_active] ON [Community].[ContactSubmission] ([tenantGuid], [active])
GO

-- Index on the ContactSubmission table's tenantGuid,deleted fields.
CREATE INDEX [I_ContactSubmission_tenantGuid_deleted] ON [Community].[ContactSubmission] ([tenantGuid], [deleted])
GO


