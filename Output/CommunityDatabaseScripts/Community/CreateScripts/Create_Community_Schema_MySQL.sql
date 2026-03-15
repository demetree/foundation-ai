-- Community CMS database schema.
This is the content management module for the public-facing community website.
It provides page management, blog publishing, media library, navigation menus,
announcements, photo galleries, document downloads, contact form handling,
and site-wide configuration.
All operational tables include auditing and security controls.
CREATE DATABASE `Community`;

USE `Community`;

/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE `ContactSubmission`
-- DROP TABLE `DocumentDownload`
-- DROP TABLE `GalleryImage`
-- DROP TABLE `GalleryAlbum`
-- DROP TABLE `AnnouncementChangeHistory`
-- DROP TABLE `Announcement`
-- DROP TABLE `SiteSetting`
-- DROP TABLE `MenuItem`
-- DROP TABLE `Menu`
-- DROP TABLE `MediaAsset`
-- DROP TABLE `PostTagAssignment`
-- DROP TABLE `PostTag`
-- DROP TABLE `PostChangeHistory`
-- DROP TABLE `Post`
-- DROP TABLE `PostCategory`
-- DROP TABLE `PageChangeHistory`
-- DROP TABLE `Page`

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON `ContactSubmission` DISABLE
-- ALTER INDEX ALL ON `DocumentDownload` DISABLE
-- ALTER INDEX ALL ON `GalleryImage` DISABLE
-- ALTER INDEX ALL ON `GalleryAlbum` DISABLE
-- ALTER INDEX ALL ON `AnnouncementChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Announcement` DISABLE
-- ALTER INDEX ALL ON `SiteSetting` DISABLE
-- ALTER INDEX ALL ON `MenuItem` DISABLE
-- ALTER INDEX ALL ON `Menu` DISABLE
-- ALTER INDEX ALL ON `MediaAsset` DISABLE
-- ALTER INDEX ALL ON `PostTagAssignment` DISABLE
-- ALTER INDEX ALL ON `PostTag` DISABLE
-- ALTER INDEX ALL ON `PostChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Post` DISABLE
-- ALTER INDEX ALL ON `PostCategory` DISABLE
-- ALTER INDEX ALL ON `PageChangeHistory` DISABLE
-- ALTER INDEX ALL ON `Page` DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON `ContactSubmission` REBUILD
-- ALTER INDEX ALL ON `DocumentDownload` REBUILD
-- ALTER INDEX ALL ON `GalleryImage` REBUILD
-- ALTER INDEX ALL ON `GalleryAlbum` REBUILD
-- ALTER INDEX ALL ON `AnnouncementChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Announcement` REBUILD
-- ALTER INDEX ALL ON `SiteSetting` REBUILD
-- ALTER INDEX ALL ON `MenuItem` REBUILD
-- ALTER INDEX ALL ON `Menu` REBUILD
-- ALTER INDEX ALL ON `MediaAsset` REBUILD
-- ALTER INDEX ALL ON `PostTagAssignment` REBUILD
-- ALTER INDEX ALL ON `PostTag` REBUILD
-- ALTER INDEX ALL ON `PostChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Post` REBUILD
-- ALTER INDEX ALL ON `PostCategory` REBUILD
-- ALTER INDEX ALL ON `PageChangeHistory` REBUILD
-- ALTER INDEX ALL ON `Page` REBUILD

-- Static content pages for the public website (e.g. About, History, FAQ, Contact, Regulations).
CREATE TABLE `Page`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`title` VARCHAR(250) NOT NULL,		-- Display title of the page
	`slug` VARCHAR(250) NOT NULL,		-- URL-friendly slug (e.g. 'about', 'history', 'faq')
	`body` TEXT NULL,		-- HTML or Markdown body content of the page
	`metaDescription` VARCHAR(500) NULL,		-- SEO meta description for search engines and social sharing
	`featuredImageUrl` VARCHAR(500) NULL,		-- URL or relative path to the page's featured/hero image
	`isPublished` BIT NOT NULL DEFAULT 0,		-- Whether this page is visible on the public site
	`publishedDate` DATETIME NULL,		-- When the page was first published (null if draft)
	`sortOrder` INT NULL DEFAULT 0,		-- Sort order for page listings and navigation
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the Page table's slug field.
CREATE UNIQUE INDEX `I_Page_slug` ON `Page` (`slug`);

-- Index on the Page table's active field.
CREATE INDEX `I_Page_active` ON `Page` (`active`);

-- Index on the Page table's deleted field.
CREATE INDEX `I_Page_deleted` ON `Page` (`deleted`);


-- The change history for records from the Page table.
CREATE TABLE `PageChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`pageId` INT NOT NULL,		-- Link to the Page table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`pageId`) REFERENCES `Page`(`id`)		-- Foreign key to the Page table.
);
-- Index on the PageChangeHistory table's versionNumber field.
CREATE INDEX `I_PageChangeHistory_versionNumber` ON `PageChangeHistory` (`versionNumber`);

-- Index on the PageChangeHistory table's timeStamp field.
CREATE INDEX `I_PageChangeHistory_timeStamp` ON `PageChangeHistory` (`timeStamp`);

-- Index on the PageChangeHistory table's userId field.
CREATE INDEX `I_PageChangeHistory_userId` ON `PageChangeHistory` (`userId`);

-- Index on the PageChangeHistory table's pageId field.
CREATE INDEX `I_PageChangeHistory_pageId` ON `PageChangeHistory` (`pageId`, `versionNumber`, `timeStamp`, `userId`);


-- Category taxonomy for organizing blog/news posts (e.g. News, Council Updates, Community Events).
CREATE TABLE `PostCategory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`description` VARCHAR(500) NULL,
	`slug` VARCHAR(250) NOT NULL,		-- URL-friendly slug for the category
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the PostCategory table's name field.
CREATE INDEX `I_PostCategory_name` ON `PostCategory` (`name`);

-- Index on the PostCategory table's slug field.
CREATE UNIQUE INDEX `I_PostCategory_slug` ON `PostCategory` (`slug`);

-- Index on the PostCategory table's active field.
CREATE INDEX `I_PostCategory_active` ON `PostCategory` (`active`);

-- Index on the PostCategory table's deleted field.
CREATE INDEX `I_PostCategory_deleted` ON `PostCategory` (`deleted`);

INSERT INTO `PostCategory` ( `name`, `description`, `slug`, `sequence`, `objectGuid` ) VALUES  ( 'News', 'General news and updates', 'news', 1, 'c0a10001-0001-4000-8000-000000000001' );

INSERT INTO `PostCategory` ( `name`, `description`, `slug`, `sequence`, `objectGuid` ) VALUES  ( 'Council Updates', 'Updates from council meetings and decisions', 'council-updates', 2, 'c0a10001-0001-4000-8000-000000000002' );

INSERT INTO `PostCategory` ( `name`, `description`, `slug`, `sequence`, `objectGuid` ) VALUES  ( 'Community Events', 'Upcoming and past community events', 'community-events', 3, 'c0a10001-0001-4000-8000-000000000003' );

INSERT INTO `PostCategory` ( `name`, `description`, `slug`, `sequence`, `objectGuid` ) VALUES  ( 'Volunteer Spotlight', 'Highlighting volunteer contributions and stories', 'volunteer-spotlight', 4, 'c0a10001-0001-4000-8000-000000000004' );

INSERT INTO `PostCategory` ( `name`, `description`, `slug`, `sequence`, `objectGuid` ) VALUES  ( 'Announcements', 'Official announcements and notices', 'announcements', 5, 'c0a10001-0001-4000-8000-000000000005' );


-- Blog and news articles for the public website. Supports categories, featured images, and publish scheduling.
CREATE TABLE `Post`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`title` VARCHAR(250) NOT NULL,		-- Display title of the post
	`slug` VARCHAR(250) NOT NULL,		-- URL-friendly slug for the post
	`body` TEXT NULL,		-- Full HTML or Markdown body content of the post
	`excerpt` VARCHAR(500) NULL,		-- Short summary for post listings and SEO (manually entered or auto-generated)
	`authorName` VARCHAR(100) NULL,		-- Display name of the post author
	`postCategoryId` INT NULL,		-- Category this post belongs to (null for uncategorized)
	`featuredImageUrl` VARCHAR(500) NULL,		-- URL or relative path to the post's featured image
	`metaDescription` VARCHAR(500) NULL,		-- SEO meta description for search engines
	`isPublished` BIT NOT NULL DEFAULT 0,		-- Whether this post is visible on the public site
	`publishedDate` DATETIME NULL,		-- When the post was published (null if draft, allows future-dating for scheduled publishing)
	`isFeatured` BIT NOT NULL DEFAULT 0,		-- Whether this post should appear in featured/highlighted sections
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`postCategoryId`) REFERENCES `PostCategory`(`id`)		-- Foreign key to the PostCategory table.
);
-- Index on the Post table's title field.
CREATE INDEX `I_Post_title` ON `Post` (`title`);

-- Index on the Post table's slug field.
CREATE UNIQUE INDEX `I_Post_slug` ON `Post` (`slug`);

-- Index on the Post table's postCategoryId field.
CREATE INDEX `I_Post_postCategoryId` ON `Post` (`postCategoryId`);

-- Index on the Post table's active field.
CREATE INDEX `I_Post_active` ON `Post` (`active`);

-- Index on the Post table's deleted field.
CREATE INDEX `I_Post_deleted` ON `Post` (`deleted`);


-- The change history for records from the Post table.
CREATE TABLE `PostChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`postId` INT NOT NULL,		-- Link to the Post table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`postId`) REFERENCES `Post`(`id`)		-- Foreign key to the Post table.
);
-- Index on the PostChangeHistory table's versionNumber field.
CREATE INDEX `I_PostChangeHistory_versionNumber` ON `PostChangeHistory` (`versionNumber`);

-- Index on the PostChangeHistory table's timeStamp field.
CREATE INDEX `I_PostChangeHistory_timeStamp` ON `PostChangeHistory` (`timeStamp`);

-- Index on the PostChangeHistory table's userId field.
CREATE INDEX `I_PostChangeHistory_userId` ON `PostChangeHistory` (`userId`);

-- Index on the PostChangeHistory table's postId field.
CREATE INDEX `I_PostChangeHistory_postId` ON `PostChangeHistory` (`postId`, `versionNumber`, `timeStamp`, `userId`);


-- Freeform tags for cross-cutting post categorization (e.g. urgent, budget, recycling).
CREATE TABLE `PostTag`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`slug` VARCHAR(250) NOT NULL,		-- URL-friendly slug for the tag
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the PostTag table's name field.
CREATE INDEX `I_PostTag_name` ON `PostTag` (`name`);

-- Index on the PostTag table's slug field.
CREATE UNIQUE INDEX `I_PostTag_slug` ON `PostTag` (`slug`);

-- Index on the PostTag table's active field.
CREATE INDEX `I_PostTag_active` ON `PostTag` (`active`);

-- Index on the PostTag table's deleted field.
CREATE INDEX `I_PostTag_deleted` ON `PostTag` (`deleted`);


-- Many-to-many mapping between posts and tags.
CREATE TABLE `PostTagAssignment`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`postId` INT NOT NULL,		-- The post being tagged
	`postTagId` INT NOT NULL,		-- The tag applied to the post
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`postId`) REFERENCES `Post`(`id`),		-- Foreign key to the Post table.
	FOREIGN KEY (`postTagId`) REFERENCES `PostTag`(`id`),		-- Foreign key to the PostTag table.
	UNIQUE `UC_PostTagAssignment_postId_postTagId_Unique`( `postId`, `postTagId` ) 		-- Uniqueness enforced on the PostTagAssignment table's postId and postTagId fields.
);
-- Index on the PostTagAssignment table's postId field.
CREATE INDEX `I_PostTagAssignment_postId` ON `PostTagAssignment` (`postId`);

-- Index on the PostTagAssignment table's postTagId field.
CREATE INDEX `I_PostTagAssignment_postTagId` ON `PostTagAssignment` (`postTagId`);

-- Index on the PostTagAssignment table's active field.
CREATE INDEX `I_PostTagAssignment_active` ON `PostTagAssignment` (`active`);

-- Index on the PostTagAssignment table's deleted field.
CREATE INDEX `I_PostTagAssignment_deleted` ON `PostTagAssignment` (`deleted`);


-- Centralized media library for uploaded images, documents, and other files used across the CMS.
CREATE TABLE `MediaAsset`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`fileName` VARCHAR(250) NOT NULL,		-- Original filename as uploaded (e.g. 'council-photo-2026.jpg')
	`filePath` VARCHAR(500) NOT NULL,		-- Server-relative storage path for the file
	`mimeType` VARCHAR(100) NOT NULL,		-- MIME type (e.g. 'image/jpeg', 'application/pdf')
	`altText` VARCHAR(250) NULL,		-- Alt text for accessibility and SEO (images)
	`caption` VARCHAR(500) NULL,		-- Display caption for the media item
	`fileSizeBytes` BIGINT NULL,		-- File size in bytes
	`imageWidth` INT NULL,		-- Width in pixels (for images only)
	`imageHeight` INT NULL,		-- Height in pixels (for images only)
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the MediaAsset table's active field.
CREATE INDEX `I_MediaAsset_active` ON `MediaAsset` (`active`);

-- Index on the MediaAsset table's deleted field.
CREATE INDEX `I_MediaAsset_deleted` ON `MediaAsset` (`deleted`);


-- Named navigation menus for different positions on the site (e.g. header, footer, sidebar).
CREATE TABLE `Menu`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL UNIQUE,
	`location` VARCHAR(50) NOT NULL,		-- Where this menu is displayed: header, footer, sidebar
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the Menu table's name field.
CREATE INDEX `I_Menu_name` ON `Menu` (`name`);

-- Index on the Menu table's active field.
CREATE INDEX `I_Menu_active` ON `Menu` (`active`);

-- Index on the Menu table's deleted field.
CREATE INDEX `I_Menu_deleted` ON `Menu` (`deleted`);

INSERT INTO `Menu` ( `name`, `location`, `objectGuid` ) VALUES  ( 'Main Navigation', 'header', 'c0b10001-0001-4000-8000-000000000001' );

INSERT INTO `Menu` ( `name`, `location`, `objectGuid` ) VALUES  ( 'Footer Links', 'footer', 'c0b10001-0001-4000-8000-000000000002' );


-- Individual navigation items within a menu. Supports tree hierarchy via self-referencing parent FK.
CREATE TABLE `MenuItem`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`menuId` INT NOT NULL,		-- The menu this item belongs to
	`label` VARCHAR(250) NOT NULL,		-- Display text for the menu item
	`url` VARCHAR(500) NULL,		-- External or absolute URL (used if pageId is null)
	`pageId` INT NULL,		-- Optional link to an internal CMS page (takes priority over url)
	`parentMenuItemId` INT NULL,		-- Parent menu item for sub-menu hierarchy (null = top-level)
	`iconClass` VARCHAR(50) NULL,		-- Optional CSS icon class (e.g. 'fa-home', 'bi-calendar')
	`openInNewTab` BIT NOT NULL DEFAULT 0,		-- Whether clicking this menu item opens in a new browser tab
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`menuId`) REFERENCES `Menu`(`id`),		-- Foreign key to the Menu table.
	FOREIGN KEY (`pageId`) REFERENCES `Page`(`id`),		-- Foreign key to the Page table.
	FOREIGN KEY (`parentMenuItemId`) REFERENCES `MenuItem`(`id`)		-- Foreign key to the MenuItem table.
);
-- Index on the MenuItem table's menuId field.
CREATE INDEX `I_MenuItem_menuId` ON `MenuItem` (`menuId`);

-- Index on the MenuItem table's pageId field.
CREATE INDEX `I_MenuItem_pageId` ON `MenuItem` (`pageId`);

-- Index on the MenuItem table's parentMenuItemId field.
CREATE INDEX `I_MenuItem_parentMenuItemId` ON `MenuItem` (`parentMenuItemId`);

-- Index on the MenuItem table's active field.
CREATE INDEX `I_MenuItem_active` ON `MenuItem` (`active`);

-- Index on the MenuItem table's deleted field.
CREATE INDEX `I_MenuItem_deleted` ON `MenuItem` (`deleted`);


-- Key-value configuration for the public site (site name, tagline, logo URL, social media links, theme settings).
CREATE TABLE `SiteSetting`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`settingKey` VARCHAR(100) NOT NULL,		-- Unique setting identifier (e.g. 'siteName', 'tagline', 'logoUrl')
	`settingValue` TEXT NULL,		-- The value for this setting
	`description` VARCHAR(250) NULL,		-- Human-readable description of what this setting controls
	`settingGroup` VARCHAR(50) NULL,		-- Grouping for settings UI (e.g. 'General', 'Social', 'SEO', 'Theme')
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the SiteSetting table's settingKey field.
CREATE UNIQUE INDEX `I_SiteSetting_settingKey` ON `SiteSetting` (`settingKey`);

-- Index on the SiteSetting table's active field.
CREATE INDEX `I_SiteSetting_active` ON `SiteSetting` (`active`);

-- Index on the SiteSetting table's deleted field.
CREATE INDEX `I_SiteSetting_deleted` ON `SiteSetting` (`deleted`);

INSERT INTO `SiteSetting` ( `settingKey`, `settingValue`, `description`, `settingGroup`, `objectGuid` ) VALUES  ( 'siteName', 'Community', 'The name of the site displayed in the header and browser tab', 'General', 'c0c10001-0001-4000-8000-000000000001' );

INSERT INTO `SiteSetting` ( `settingKey`, `settingValue`, `description`, `settingGroup`, `objectGuid` ) VALUES  ( 'tagline', 'Welcome to our community', 'Site tagline displayed below the site name', 'General', 'c0c10001-0001-4000-8000-000000000002' );

INSERT INTO `SiteSetting` ( `settingKey`, `settingValue`, `description`, `settingGroup`, `objectGuid` ) VALUES  ( 'logoUrl', '', 'URL to the site logo image', 'General', 'c0c10001-0001-4000-8000-000000000003' );

INSERT INTO `SiteSetting` ( `settingKey`, `settingValue`, `description`, `settingGroup`, `objectGuid` ) VALUES  ( 'footerText', '© 2026 K2 Research. All rights reserved.', 'Copyright text displayed in the site footer', 'General', 'c0c10001-0001-4000-8000-000000000004' );

INSERT INTO `SiteSetting` ( `settingKey`, `settingValue`, `description`, `settingGroup`, `objectGuid` ) VALUES  ( 'contactEmail', '', 'Primary contact email address', 'General', 'c0c10001-0001-4000-8000-000000000005' );

INSERT INTO `SiteSetting` ( `settingKey`, `settingValue`, `description`, `settingGroup`, `objectGuid` ) VALUES  ( 'contactPhone', '', 'Primary contact phone number', 'General', 'c0c10001-0001-4000-8000-000000000006' );

INSERT INTO `SiteSetting` ( `settingKey`, `settingValue`, `description`, `settingGroup`, `objectGuid` ) VALUES  ( 'facebookUrl', '', 'Facebook page URL', 'Social', 'c0c10001-0001-4000-8000-000000000010' );

INSERT INTO `SiteSetting` ( `settingKey`, `settingValue`, `description`, `settingGroup`, `objectGuid` ) VALUES  ( 'twitterUrl', '', 'Twitter/X profile URL', 'Social', 'c0c10001-0001-4000-8000-000000000011' );

INSERT INTO `SiteSetting` ( `settingKey`, `settingValue`, `description`, `settingGroup`, `objectGuid` ) VALUES  ( 'instagramUrl', '', 'Instagram profile URL', 'Social', 'c0c10001-0001-4000-8000-000000000012' );

INSERT INTO `SiteSetting` ( `settingKey`, `settingValue`, `description`, `settingGroup`, `objectGuid` ) VALUES  ( 'heroTitle', 'Welcome', 'Hero section title on the home page', 'HomePage', 'c0c10001-0001-4000-8000-000000000020' );

INSERT INTO `SiteSetting` ( `settingKey`, `settingValue`, `description`, `settingGroup`, `objectGuid` ) VALUES  ( 'heroSubtitle', '', 'Hero section subtitle on the home page', 'HomePage', 'c0c10001-0001-4000-8000-000000000021' );

INSERT INTO `SiteSetting` ( `settingKey`, `settingValue`, `description`, `settingGroup`, `objectGuid` ) VALUES  ( 'heroImageUrl', '', 'Hero section background image URL', 'HomePage', 'c0c10001-0001-4000-8000-000000000022' );


-- Time-bound announcements displayed on the public site (council meeting notices, public consultations, service disruptions).
CREATE TABLE `Announcement`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`title` VARCHAR(250) NOT NULL,		-- Announcement title
	`body` TEXT NULL,		-- Full announcement body content (HTML or Markdown)
	`severity` VARCHAR(50) NOT NULL DEFAULT 'info',		-- Severity level: info, warning, urgent
	`startDate` DATETIME NOT NULL,		-- When the announcement becomes visible
	`endDate` DATETIME NULL,		-- When the announcement expires (null = no expiration)
	`isPinned` BIT NOT NULL DEFAULT 0,		-- Whether this announcement should always appear at the top
	`versionNumber` INT NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the Announcement table's active field.
CREATE INDEX `I_Announcement_active` ON `Announcement` (`active`);

-- Index on the Announcement table's deleted field.
CREATE INDEX `I_Announcement_deleted` ON `Announcement` (`deleted`);


-- The change history for records from the Announcement table.
CREATE TABLE `AnnouncementChangeHistory`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`announcementId` INT NOT NULL,		-- Link to the Announcement table.
	`versionNumber` INT NOT NULL,		-- This is the version number that is being historized.
	`timeStamp` DATETIME NOT NULL,		-- The time that the record version was created.
	`userId` INT NOT NULL,
	`data` TEXT NOT NULL,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY (`announcementId`) REFERENCES `Announcement`(`id`)		-- Foreign key to the Announcement table.
);
-- Index on the AnnouncementChangeHistory table's versionNumber field.
CREATE INDEX `I_AnnouncementChangeHistory_versionNumber` ON `AnnouncementChangeHistory` (`versionNumber`);

-- Index on the AnnouncementChangeHistory table's timeStamp field.
CREATE INDEX `I_AnnouncementChangeHistory_timeStamp` ON `AnnouncementChangeHistory` (`timeStamp`);

-- Index on the AnnouncementChangeHistory table's userId field.
CREATE INDEX `I_AnnouncementChangeHistory_userId` ON `AnnouncementChangeHistory` (`userId`);

-- Index on the AnnouncementChangeHistory table's announcementId field.
CREATE INDEX `I_AnnouncementChangeHistory_announcementId` ON `AnnouncementChangeHistory` (`announcementId`, `versionNumber`, `timeStamp`, `userId`);


-- Photo albums for organizing gallery images (e.g. 'Community Day 2026', 'Town Views', 'Heritage Photos').
CREATE TABLE `GalleryAlbum`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`title` VARCHAR(250) NOT NULL,		-- Album title
	`slug` VARCHAR(250) NOT NULL,		-- URL-friendly slug for the album
	`description` TEXT NULL,		-- Description of the album
	`coverImageUrl` VARCHAR(500) NULL,		-- URL to the album cover image (or uses first image if null)
	`isPublished` BIT NOT NULL DEFAULT 1,		-- Whether this album is visible on the public site
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the GalleryAlbum table's slug field.
CREATE UNIQUE INDEX `I_GalleryAlbum_slug` ON `GalleryAlbum` (`slug`);

-- Index on the GalleryAlbum table's active field.
CREATE INDEX `I_GalleryAlbum_active` ON `GalleryAlbum` (`active`);

-- Index on the GalleryAlbum table's deleted field.
CREATE INDEX `I_GalleryAlbum_deleted` ON `GalleryAlbum` (`deleted`);


-- Individual images within a gallery album.
CREATE TABLE `GalleryImage`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`galleryAlbumId` INT NOT NULL,		-- The album this image belongs to
	`imageUrl` VARCHAR(500) NOT NULL,		-- URL or relative path to the image file
	`caption` VARCHAR(500) NULL,		-- Display caption for the image
	`altText` VARCHAR(250) NULL,		-- Alt text for accessibility
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY (`galleryAlbumId`) REFERENCES `GalleryAlbum`(`id`)		-- Foreign key to the GalleryAlbum table.
);
-- Index on the GalleryImage table's galleryAlbumId field.
CREATE INDEX `I_GalleryImage_galleryAlbumId` ON `GalleryImage` (`galleryAlbumId`);

-- Index on the GalleryImage table's active field.
CREATE INDEX `I_GalleryImage_active` ON `GalleryImage` (`active`);

-- Index on the GalleryImage table's deleted field.
CREATE INDEX `I_GalleryImage_deleted` ON `GalleryImage` (`deleted`);


-- Downloadable documents for public access (council minutes, schedules, forms, regulations). Replaces the PDF-as-image pattern.
CREATE TABLE `DocumentDownload`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`title` VARCHAR(250) NOT NULL,		-- Display title for the document
	`description` TEXT NULL,		-- Description of what this document contains
	`filePath` VARCHAR(500) NOT NULL,		-- Server-relative path to the downloadable file
	`fileName` VARCHAR(100) NOT NULL,		-- Original filename for download (e.g. 'recycling-schedule-2026.pdf')
	`mimeType` VARCHAR(100) NULL,		-- File MIME type
	`fileSizeBytes` BIGINT NULL,		-- File size in bytes
	`categoryName` VARCHAR(100) NULL,		-- Grouping category (e.g. 'Council Minutes', 'Schedules', 'Forms', 'Regulations')
	`documentDate` DATETIME NULL,		-- Date associated with the document (e.g. meeting date for council minutes)
	`isPublished` BIT NOT NULL DEFAULT 1,		-- Whether this document is visible for public download
	`sequence` INT NULL,		-- Sequence to use for sorting.
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the DocumentDownload table's active field.
CREATE INDEX `I_DocumentDownload_active` ON `DocumentDownload` (`active`);

-- Index on the DocumentDownload table's deleted field.
CREATE INDEX `I_DocumentDownload_deleted` ON `DocumentDownload` (`deleted`);


-- Captures contact form submissions from public site visitors.
CREATE TABLE `ContactSubmission`(
	`id` INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(100) NOT NULL,		-- Name of the person submitting the form
	`email` VARCHAR(250) NOT NULL,		-- Email address for reply
	`subject` VARCHAR(250) NULL,		-- Subject line of the message
	`message` TEXT NOT NULL,		-- Full message body
	`submittedDate` DATETIME NOT NULL,		-- When the form was submitted
	`isRead` BIT NOT NULL DEFAULT 0,		-- Whether an admin has read this submission
	`isArchived` BIT NOT NULL DEFAULT 0,		-- Whether this submission has been archived
	`adminNotes` VARCHAR(500) NULL,		-- Internal notes from admin about the submission
	`objectGuid` CHAR(38) NOT NULL UNIQUE,		-- Unique identifier for this table.
	`active` BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	`deleted` BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the ContactSubmission table's active field.
CREATE INDEX `I_ContactSubmission_active` ON `ContactSubmission` (`active`);

-- Index on the ContactSubmission table's deleted field.
CREATE INDEX `I_ContactSubmission_deleted` ON `ContactSubmission` (`deleted`);


