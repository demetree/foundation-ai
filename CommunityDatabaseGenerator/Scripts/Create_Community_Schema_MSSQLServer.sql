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

-- Index on the Page table's slug field.
CREATE UNIQUE INDEX [I_Page_slug] ON [Community].[Page] ([slug])
GO

-- Index on the Page table's active field.
CREATE INDEX [I_Page_active] ON [Community].[Page] ([active])
GO

-- Index on the Page table's deleted field.
CREATE INDEX [I_Page_deleted] ON [Community].[Page] ([deleted])
GO


-- The change history for records from the Page table.
CREATE TABLE [Community].[PageChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[pageId] INT NOT NULL,		-- Link to the Page table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_PageChangeHistory_Page_pageId] FOREIGN KEY ([pageId]) REFERENCES [Community].[Page] ([id])		-- Foreign key to the Page table.
)
GO

-- Index on the PageChangeHistory table's versionNumber field.
CREATE INDEX [I_PageChangeHistory_versionNumber] ON [Community].[PageChangeHistory] ([versionNumber])
GO

-- Index on the PageChangeHistory table's timeStamp field.
CREATE INDEX [I_PageChangeHistory_timeStamp] ON [Community].[PageChangeHistory] ([timeStamp])
GO

-- Index on the PageChangeHistory table's userId field.
CREATE INDEX [I_PageChangeHistory_userId] ON [Community].[PageChangeHistory] ([userId])
GO

-- Index on the PageChangeHistory table's pageId field.
CREATE INDEX [I_PageChangeHistory_pageId] ON [Community].[PageChangeHistory] ([pageId]) INCLUDE ( versionNumber, timeStamp, userId )
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

-- Index on the Post table's title field.
CREATE INDEX [I_Post_title] ON [Community].[Post] ([title])
GO

-- Index on the Post table's slug field.
CREATE UNIQUE INDEX [I_Post_slug] ON [Community].[Post] ([slug])
GO

-- Index on the Post table's postCategoryId field.
CREATE INDEX [I_Post_postCategoryId] ON [Community].[Post] ([postCategoryId])
GO

-- Index on the Post table's active field.
CREATE INDEX [I_Post_active] ON [Community].[Post] ([active])
GO

-- Index on the Post table's deleted field.
CREATE INDEX [I_Post_deleted] ON [Community].[Post] ([deleted])
GO


-- The change history for records from the Post table.
CREATE TABLE [Community].[PostChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[postId] INT NOT NULL,		-- Link to the Post table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_PostChangeHistory_Post_postId] FOREIGN KEY ([postId]) REFERENCES [Community].[Post] ([id])		-- Foreign key to the Post table.
)
GO

-- Index on the PostChangeHistory table's versionNumber field.
CREATE INDEX [I_PostChangeHistory_versionNumber] ON [Community].[PostChangeHistory] ([versionNumber])
GO

-- Index on the PostChangeHistory table's timeStamp field.
CREATE INDEX [I_PostChangeHistory_timeStamp] ON [Community].[PostChangeHistory] ([timeStamp])
GO

-- Index on the PostChangeHistory table's userId field.
CREATE INDEX [I_PostChangeHistory_userId] ON [Community].[PostChangeHistory] ([userId])
GO

-- Index on the PostChangeHistory table's postId field.
CREATE INDEX [I_PostChangeHistory_postId] ON [Community].[PostChangeHistory] ([postId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Freeform tags for cross-cutting post categorization (e.g. urgent, budget, recycling).
CREATE TABLE [Community].[PostTag]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[slug] NVARCHAR(250) NOT NULL,		-- URL-friendly slug for the tag
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the PostTag table's name field.
CREATE INDEX [I_PostTag_name] ON [Community].[PostTag] ([name])
GO

-- Index on the PostTag table's slug field.
CREATE UNIQUE INDEX [I_PostTag_slug] ON [Community].[PostTag] ([slug])
GO

-- Index on the PostTag table's active field.
CREATE INDEX [I_PostTag_active] ON [Community].[PostTag] ([active])
GO

-- Index on the PostTag table's deleted field.
CREATE INDEX [I_PostTag_deleted] ON [Community].[PostTag] ([deleted])
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

-- Index on the MediaAsset table's active field.
CREATE INDEX [I_MediaAsset_active] ON [Community].[MediaAsset] ([active])
GO

-- Index on the MediaAsset table's deleted field.
CREATE INDEX [I_MediaAsset_deleted] ON [Community].[MediaAsset] ([deleted])
GO


-- Named navigation menus for different positions on the site (e.g. header, footer, sidebar).
CREATE TABLE [Community].[Menu]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[name] NVARCHAR(100) NOT NULL UNIQUE,
	[location] NVARCHAR(50) NOT NULL,		-- Where this menu is displayed: header, footer, sidebar
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the Menu table's name field.
CREATE INDEX [I_Menu_name] ON [Community].[Menu] ([name])
GO

-- Index on the Menu table's active field.
CREATE INDEX [I_Menu_active] ON [Community].[Menu] ([active])
GO

-- Index on the Menu table's deleted field.
CREATE INDEX [I_Menu_deleted] ON [Community].[Menu] ([deleted])
GO

INSERT INTO [Community].[Menu] ( [name], [location], [objectGuid] ) VALUES  ( 'Main Navigation', 'header', 'c0b10001-0001-4000-8000-000000000001' )
GO

INSERT INTO [Community].[Menu] ( [name], [location], [objectGuid] ) VALUES  ( 'Footer Links', 'footer', 'c0b10001-0001-4000-8000-000000000002' )
GO


-- Individual navigation items within a menu. Supports tree hierarchy via self-referencing parent FK.
CREATE TABLE [Community].[MenuItem]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
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

-- Index on the MenuItem table's menuId field.
CREATE INDEX [I_MenuItem_menuId] ON [Community].[MenuItem] ([menuId])
GO

-- Index on the MenuItem table's pageId field.
CREATE INDEX [I_MenuItem_pageId] ON [Community].[MenuItem] ([pageId])
GO

-- Index on the MenuItem table's parentMenuItemId field.
CREATE INDEX [I_MenuItem_parentMenuItemId] ON [Community].[MenuItem] ([parentMenuItemId])
GO

-- Index on the MenuItem table's active field.
CREATE INDEX [I_MenuItem_active] ON [Community].[MenuItem] ([active])
GO

-- Index on the MenuItem table's deleted field.
CREATE INDEX [I_MenuItem_deleted] ON [Community].[MenuItem] ([deleted])
GO


-- Key-value configuration for the public site (site name, tagline, logo URL, social media links, theme settings).
CREATE TABLE [Community].[SiteSetting]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[settingKey] NVARCHAR(100) NOT NULL,		-- Unique setting identifier (e.g. 'siteName', 'tagline', 'logoUrl')
	[settingValue] NVARCHAR(MAX) NULL,		-- The value for this setting
	[description] NVARCHAR(250) NULL,		-- Human-readable description of what this setting controls
	[settingGroup] NVARCHAR(50) NULL,		-- Grouping for settings UI (e.g. 'General', 'Social', 'SEO', 'Theme')
	[objectGuid] UNIQUEIDENTIFIER NOT NULL UNIQUE,		-- Unique identifier for this table.
	[active] BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	[deleted] BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

)
GO

-- Index on the SiteSetting table's settingKey field.
CREATE UNIQUE INDEX [I_SiteSetting_settingKey] ON [Community].[SiteSetting] ([settingKey])
GO

-- Index on the SiteSetting table's active field.
CREATE INDEX [I_SiteSetting_active] ON [Community].[SiteSetting] ([active])
GO

-- Index on the SiteSetting table's deleted field.
CREATE INDEX [I_SiteSetting_deleted] ON [Community].[SiteSetting] ([deleted])
GO

INSERT INTO [Community].[SiteSetting] ( [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'siteName', 'Community', 'The name of the site displayed in the header and browser tab', 'General', 'c0c10001-0001-4000-8000-000000000001' )
GO

INSERT INTO [Community].[SiteSetting] ( [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'tagline', 'Welcome to our community', 'Site tagline displayed below the site name', 'General', 'c0c10001-0001-4000-8000-000000000002' )
GO

INSERT INTO [Community].[SiteSetting] ( [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'logoUrl', '', 'URL to the site logo image', 'General', 'c0c10001-0001-4000-8000-000000000003' )
GO

INSERT INTO [Community].[SiteSetting] ( [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'footerText', '© 2026 K2 Research. All rights reserved.', 'Copyright text displayed in the site footer', 'General', 'c0c10001-0001-4000-8000-000000000004' )
GO

INSERT INTO [Community].[SiteSetting] ( [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'contactEmail', '', 'Primary contact email address', 'General', 'c0c10001-0001-4000-8000-000000000005' )
GO

INSERT INTO [Community].[SiteSetting] ( [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'contactPhone', '', 'Primary contact phone number', 'General', 'c0c10001-0001-4000-8000-000000000006' )
GO

INSERT INTO [Community].[SiteSetting] ( [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'facebookUrl', '', 'Facebook page URL', 'Social', 'c0c10001-0001-4000-8000-000000000010' )
GO

INSERT INTO [Community].[SiteSetting] ( [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'twitterUrl', '', 'Twitter/X profile URL', 'Social', 'c0c10001-0001-4000-8000-000000000011' )
GO

INSERT INTO [Community].[SiteSetting] ( [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'instagramUrl', '', 'Instagram profile URL', 'Social', 'c0c10001-0001-4000-8000-000000000012' )
GO

INSERT INTO [Community].[SiteSetting] ( [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'heroTitle', 'Welcome', 'Hero section title on the home page', 'HomePage', 'c0c10001-0001-4000-8000-000000000020' )
GO

INSERT INTO [Community].[SiteSetting] ( [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'heroSubtitle', '', 'Hero section subtitle on the home page', 'HomePage', 'c0c10001-0001-4000-8000-000000000021' )
GO

INSERT INTO [Community].[SiteSetting] ( [settingKey], [settingValue], [description], [settingGroup], [objectGuid] ) VALUES  ( 'heroImageUrl', '', 'Hero section background image URL', 'HomePage', 'c0c10001-0001-4000-8000-000000000022' )
GO


-- Time-bound announcements displayed on the public site (council meeting notices, public consultations, service disruptions).
CREATE TABLE [Community].[Announcement]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
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

-- Index on the Announcement table's active field.
CREATE INDEX [I_Announcement_active] ON [Community].[Announcement] ([active])
GO

-- Index on the Announcement table's deleted field.
CREATE INDEX [I_Announcement_deleted] ON [Community].[Announcement] ([deleted])
GO


-- The change history for records from the Announcement table.
CREATE TABLE [Community].[AnnouncementChangeHistory]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
	[announcementId] INT NOT NULL,		-- Link to the Announcement table.
	[versionNumber] INT NOT NULL,		-- This is the version number that is being historized.
	[timeStamp] DATETIME2(7) NOT NULL,		-- The time that the record version was created.
	[userId] INT NOT NULL,
	[data] NVARCHAR(MAX) NOT NULL		-- This stores the JSON representing the object's historical state.

	CONSTRAINT [FK_AnnouncementChangeHistory_Announcement_announcementId] FOREIGN KEY ([announcementId]) REFERENCES [Community].[Announcement] ([id])		-- Foreign key to the Announcement table.
)
GO

-- Index on the AnnouncementChangeHistory table's versionNumber field.
CREATE INDEX [I_AnnouncementChangeHistory_versionNumber] ON [Community].[AnnouncementChangeHistory] ([versionNumber])
GO

-- Index on the AnnouncementChangeHistory table's timeStamp field.
CREATE INDEX [I_AnnouncementChangeHistory_timeStamp] ON [Community].[AnnouncementChangeHistory] ([timeStamp])
GO

-- Index on the AnnouncementChangeHistory table's userId field.
CREATE INDEX [I_AnnouncementChangeHistory_userId] ON [Community].[AnnouncementChangeHistory] ([userId])
GO

-- Index on the AnnouncementChangeHistory table's announcementId field.
CREATE INDEX [I_AnnouncementChangeHistory_announcementId] ON [Community].[AnnouncementChangeHistory] ([announcementId]) INCLUDE ( versionNumber, timeStamp, userId )
GO


-- Photo albums for organizing gallery images (e.g. 'Community Day 2026', 'Town Views', 'Heritage Photos').
CREATE TABLE [Community].[GalleryAlbum]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
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

-- Index on the GalleryAlbum table's slug field.
CREATE UNIQUE INDEX [I_GalleryAlbum_slug] ON [Community].[GalleryAlbum] ([slug])
GO

-- Index on the GalleryAlbum table's active field.
CREATE INDEX [I_GalleryAlbum_active] ON [Community].[GalleryAlbum] ([active])
GO

-- Index on the GalleryAlbum table's deleted field.
CREATE INDEX [I_GalleryAlbum_deleted] ON [Community].[GalleryAlbum] ([deleted])
GO


-- Individual images within a gallery album.
CREATE TABLE [Community].[GalleryImage]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
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

-- Index on the GalleryImage table's galleryAlbumId field.
CREATE INDEX [I_GalleryImage_galleryAlbumId] ON [Community].[GalleryImage] ([galleryAlbumId])
GO

-- Index on the GalleryImage table's active field.
CREATE INDEX [I_GalleryImage_active] ON [Community].[GalleryImage] ([active])
GO

-- Index on the GalleryImage table's deleted field.
CREATE INDEX [I_GalleryImage_deleted] ON [Community].[GalleryImage] ([deleted])
GO


-- Downloadable documents for public access (council minutes, schedules, forms, regulations). Replaces the PDF-as-image pattern.
CREATE TABLE [Community].[DocumentDownload]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
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

-- Index on the DocumentDownload table's active field.
CREATE INDEX [I_DocumentDownload_active] ON [Community].[DocumentDownload] ([active])
GO

-- Index on the DocumentDownload table's deleted field.
CREATE INDEX [I_DocumentDownload_deleted] ON [Community].[DocumentDownload] ([deleted])
GO


-- Captures contact form submissions from public site visitors.
CREATE TABLE [Community].[ContactSubmission]
(
	[id] INT IDENTITY PRIMARY KEY NOT NULL,
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

-- Index on the ContactSubmission table's active field.
CREATE INDEX [I_ContactSubmission_active] ON [Community].[ContactSubmission] ([active])
GO

-- Index on the ContactSubmission table's deleted field.
CREATE INDEX [I_ContactSubmission_deleted] ON [Community].[ContactSubmission] ([deleted])
GO


