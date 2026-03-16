/*
Community CMS database schema.
This is the content management module for the public-facing community website.
It provides page management, blog publishing, media library, navigation menus,
announcements, photo galleries, document downloads, contact form handling,
and site-wide configuration.
All operational tables include auditing and security controls.
*/
/* These drop table commands are here in a commented state as a convenience for situations where you may want to modify the tables in a schema.  They are ordered correctly to be able to delete all tables if executed as a batch, or at least in this order.  Be very careful with these. */
-- DROP TABLE "ContactSubmission"
-- DROP TABLE "DocumentDownload"
-- DROP TABLE "GalleryImage"
-- DROP TABLE "GalleryAlbum"
-- DROP TABLE "AnnouncementChangeHistory"
-- DROP TABLE "Announcement"
-- DROP TABLE "SiteSetting"
-- DROP TABLE "MenuItem"
-- DROP TABLE "Menu"
-- DROP TABLE "MediaContent"
-- DROP TABLE "MediaAsset"
-- DROP TABLE "PostTagAssignment"
-- DROP TABLE "PostTag"
-- DROP TABLE "PostChangeHistory"
-- DROP TABLE "Post"
-- DROP TABLE "PostCategory"
-- DROP TABLE "PageChangeHistory"
-- DROP TABLE "Page"

/* These disable table index commands are here in a commented state as a convenience for situations where you want to remove the indexes on a table for things like mass data loads, where indexes just slow things down.  The corresponding rebuild index commands are listed after the disable commands */
-- ALTER INDEX ALL ON "ContactSubmission" DISABLE
-- ALTER INDEX ALL ON "DocumentDownload" DISABLE
-- ALTER INDEX ALL ON "GalleryImage" DISABLE
-- ALTER INDEX ALL ON "GalleryAlbum" DISABLE
-- ALTER INDEX ALL ON "AnnouncementChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Announcement" DISABLE
-- ALTER INDEX ALL ON "SiteSetting" DISABLE
-- ALTER INDEX ALL ON "MenuItem" DISABLE
-- ALTER INDEX ALL ON "Menu" DISABLE
-- ALTER INDEX ALL ON "MediaContent" DISABLE
-- ALTER INDEX ALL ON "MediaAsset" DISABLE
-- ALTER INDEX ALL ON "PostTagAssignment" DISABLE
-- ALTER INDEX ALL ON "PostTag" DISABLE
-- ALTER INDEX ALL ON "PostChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Post" DISABLE
-- ALTER INDEX ALL ON "PostCategory" DISABLE
-- ALTER INDEX ALL ON "PageChangeHistory" DISABLE
-- ALTER INDEX ALL ON "Page" DISABLE

/* These rebuild table index commands are here in a commented state as a convenience for situations where you want to rebuild the indexes on a table after having removed them, or if you want to refresh them. */
-- ALTER INDEX ALL ON "ContactSubmission" REBUILD
-- ALTER INDEX ALL ON "DocumentDownload" REBUILD
-- ALTER INDEX ALL ON "GalleryImage" REBUILD
-- ALTER INDEX ALL ON "GalleryAlbum" REBUILD
-- ALTER INDEX ALL ON "AnnouncementChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Announcement" REBUILD
-- ALTER INDEX ALL ON "SiteSetting" REBUILD
-- ALTER INDEX ALL ON "MenuItem" REBUILD
-- ALTER INDEX ALL ON "Menu" REBUILD
-- ALTER INDEX ALL ON "MediaContent" REBUILD
-- ALTER INDEX ALL ON "MediaAsset" REBUILD
-- ALTER INDEX ALL ON "PostTagAssignment" REBUILD
-- ALTER INDEX ALL ON "PostTag" REBUILD
-- ALTER INDEX ALL ON "PostChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Post" REBUILD
-- ALTER INDEX ALL ON "PostCategory" REBUILD
-- ALTER INDEX ALL ON "PageChangeHistory" REBUILD
-- ALTER INDEX ALL ON "Page" REBUILD

-- Static content pages for the public website (e.g. About, History, FAQ, Contact, Regulations).
CREATE TABLE "Page"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"title" VARCHAR(250) NOT NULL COLLATE NOCASE,		-- Display title of the page
	"slug" VARCHAR(250) NOT NULL COLLATE NOCASE,		-- URL-friendly slug (e.g. 'about', 'history', 'faq')
	"body" TEXT NULL COLLATE NOCASE,		-- HTML or Markdown body content of the page
	"metaDescription" VARCHAR(500) NULL COLLATE NOCASE,		-- SEO meta description for search engines and social sharing
	"featuredImageUrl" VARCHAR(500) NULL COLLATE NOCASE,		-- URL or relative path to the page's featured/hero image
	"isPublished" BIT NOT NULL DEFAULT 0,		-- Whether this page is visible on the public site
	"publishedDate" DATETIME NULL,		-- When the page was first published (null if draft)
	"sortOrder" INTEGER NULL DEFAULT 0,		-- Sort order for page listings and navigation
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the Page table's tenantGuid field.
CREATE INDEX "I_Page_tenantGuid" ON "Page" ("tenantGuid")
;

-- Index on the Page table's tenantGuid,slug fields.
CREATE UNIQUE INDEX "I_Page_tenantGuid_slug" ON "Page" ("tenantGuid", "slug")
;

-- Index on the Page table's tenantGuid,active fields.
CREATE INDEX "I_Page_tenantGuid_active" ON "Page" ("tenantGuid", "active")
;

-- Index on the Page table's tenantGuid,deleted fields.
CREATE INDEX "I_Page_tenantGuid_deleted" ON "Page" ("tenantGuid", "deleted")
;


-- The change history for records from the Page table.
CREATE TABLE "PageChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"pageId" INTEGER NOT NULL,		-- Link to the Page table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("pageId") REFERENCES "Page"("id")		-- Foreign key to the Page table.
);
-- Index on the PageChangeHistory table's tenantGuid field.
CREATE INDEX "I_PageChangeHistory_tenantGuid" ON "PageChangeHistory" ("tenantGuid")
;

-- Index on the PageChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_PageChangeHistory_tenantGuid_versionNumber" ON "PageChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the PageChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_PageChangeHistory_tenantGuid_timeStamp" ON "PageChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the PageChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_PageChangeHistory_tenantGuid_userId" ON "PageChangeHistory" ("tenantGuid", "userId")
;

-- Index on the PageChangeHistory table's tenantGuid,pageId fields.
CREATE INDEX "I_PageChangeHistory_tenantGuid_pageId" ON "PageChangeHistory" ("tenantGuid", "pageId", "versionNumber", "timeStamp", "userId")
;


-- Category taxonomy for organizing blog/news posts (e.g. News, Council Updates, Community Events).
CREATE TABLE "PostCategory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name" VARCHAR(100) NOT NULL UNIQUE COLLATE NOCASE,
	"description" VARCHAR(500) NULL COLLATE NOCASE,
	"slug" VARCHAR(250) NOT NULL COLLATE NOCASE,		-- URL-friendly slug for the category
	"sequence" INTEGER NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the PostCategory table's name field.
CREATE INDEX "I_PostCategory_name" ON "PostCategory" ("name")
;

-- Index on the PostCategory table's slug field.
CREATE UNIQUE INDEX "I_PostCategory_slug" ON "PostCategory" ("slug")
;

-- Index on the PostCategory table's active field.
CREATE INDEX "I_PostCategory_active" ON "PostCategory" ("active")
;

-- Index on the PostCategory table's deleted field.
CREATE INDEX "I_PostCategory_deleted" ON "PostCategory" ("deleted")
;

INSERT INTO "PostCategory" ( "name", "description", "slug", "sequence", "objectGuid" ) VALUES  ( 'News', 'General news and updates', 'news', 1, 'c0a10001-0001-4000-8000-000000000001' );

INSERT INTO "PostCategory" ( "name", "description", "slug", "sequence", "objectGuid" ) VALUES  ( 'Council Updates', 'Updates from council meetings and decisions', 'council-updates', 2, 'c0a10001-0001-4000-8000-000000000002' );

INSERT INTO "PostCategory" ( "name", "description", "slug", "sequence", "objectGuid" ) VALUES  ( 'Community Events', 'Upcoming and past community events', 'community-events', 3, 'c0a10001-0001-4000-8000-000000000003' );

INSERT INTO "PostCategory" ( "name", "description", "slug", "sequence", "objectGuid" ) VALUES  ( 'Volunteer Spotlight', 'Highlighting volunteer contributions and stories', 'volunteer-spotlight', 4, 'c0a10001-0001-4000-8000-000000000004' );

INSERT INTO "PostCategory" ( "name", "description", "slug", "sequence", "objectGuid" ) VALUES  ( 'Announcements', 'Official announcements and notices', 'announcements', 5, 'c0a10001-0001-4000-8000-000000000005' );


-- Blog and news articles for the public website. Supports categories, featured images, and publish scheduling.
CREATE TABLE "Post"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"title" VARCHAR(250) NOT NULL COLLATE NOCASE,		-- Display title of the post
	"slug" VARCHAR(250) NOT NULL COLLATE NOCASE,		-- URL-friendly slug for the post
	"body" TEXT NULL COLLATE NOCASE,		-- Full HTML or Markdown body content of the post
	"excerpt" VARCHAR(500) NULL COLLATE NOCASE,		-- Short summary for post listings and SEO (manually entered or auto-generated)
	"authorName" VARCHAR(100) NULL COLLATE NOCASE,		-- Display name of the post author
	"postCategoryId" INTEGER NULL,		-- Category this post belongs to (null for uncategorized)
	"featuredImageUrl" VARCHAR(500) NULL COLLATE NOCASE,		-- URL or relative path to the post's featured image
	"metaDescription" VARCHAR(500) NULL COLLATE NOCASE,		-- SEO meta description for search engines
	"isPublished" BIT NOT NULL DEFAULT 0,		-- Whether this post is visible on the public site
	"publishedDate" DATETIME NULL,		-- When the post was published (null if draft, allows future-dating for scheduled publishing)
	"isFeatured" BIT NOT NULL DEFAULT 0,		-- Whether this post should appear in featured/highlighted sections
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("postCategoryId") REFERENCES "PostCategory"("id")		-- Foreign key to the PostCategory table.
);
-- Index on the Post table's tenantGuid field.
CREATE INDEX "I_Post_tenantGuid" ON "Post" ("tenantGuid")
;

-- Index on the Post table's tenantGuid,title fields.
CREATE INDEX "I_Post_tenantGuid_title" ON "Post" ("tenantGuid", "title")
;

-- Index on the Post table's tenantGuid,slug fields.
CREATE UNIQUE INDEX "I_Post_tenantGuid_slug" ON "Post" ("tenantGuid", "slug")
;

-- Index on the Post table's tenantGuid,postCategoryId fields.
CREATE INDEX "I_Post_tenantGuid_postCategoryId" ON "Post" ("tenantGuid", "postCategoryId")
;

-- Index on the Post table's tenantGuid,active fields.
CREATE INDEX "I_Post_tenantGuid_active" ON "Post" ("tenantGuid", "active")
;

-- Index on the Post table's tenantGuid,deleted fields.
CREATE INDEX "I_Post_tenantGuid_deleted" ON "Post" ("tenantGuid", "deleted")
;


-- The change history for records from the Post table.
CREATE TABLE "PostChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"postId" INTEGER NOT NULL,		-- Link to the Post table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("postId") REFERENCES "Post"("id")		-- Foreign key to the Post table.
);
-- Index on the PostChangeHistory table's tenantGuid field.
CREATE INDEX "I_PostChangeHistory_tenantGuid" ON "PostChangeHistory" ("tenantGuid")
;

-- Index on the PostChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_PostChangeHistory_tenantGuid_versionNumber" ON "PostChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the PostChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_PostChangeHistory_tenantGuid_timeStamp" ON "PostChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the PostChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_PostChangeHistory_tenantGuid_userId" ON "PostChangeHistory" ("tenantGuid", "userId")
;

-- Index on the PostChangeHistory table's tenantGuid,postId fields.
CREATE INDEX "I_PostChangeHistory_tenantGuid_postId" ON "PostChangeHistory" ("tenantGuid", "postId", "versionNumber", "timeStamp", "userId")
;


-- Freeform tags for cross-cutting post categorization (e.g. urgent, budget, recycling).
CREATE TABLE "PostTag"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL COLLATE NOCASE,
	"slug" VARCHAR(250) NOT NULL COLLATE NOCASE,		-- URL-friendly slug for the tag
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the PostTag table's tenantGuid and name fields.
);
-- Index on the PostTag table's tenantGuid field.
CREATE INDEX "I_PostTag_tenantGuid" ON "PostTag" ("tenantGuid")
;

-- Index on the PostTag table's tenantGuid,name fields.
CREATE INDEX "I_PostTag_tenantGuid_name" ON "PostTag" ("tenantGuid", "name")
;

-- Index on the PostTag table's tenantGuid,slug fields.
CREATE UNIQUE INDEX "I_PostTag_tenantGuid_slug" ON "PostTag" ("tenantGuid", "slug")
;

-- Index on the PostTag table's tenantGuid,active fields.
CREATE INDEX "I_PostTag_tenantGuid_active" ON "PostTag" ("tenantGuid", "active")
;

-- Index on the PostTag table's tenantGuid,deleted fields.
CREATE INDEX "I_PostTag_tenantGuid_deleted" ON "PostTag" ("tenantGuid", "deleted")
;


-- Many-to-many mapping between posts and tags.
CREATE TABLE "PostTagAssignment"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"postId" INTEGER NOT NULL,		-- The post being tagged
	"postTagId" INTEGER NOT NULL,		-- The tag applied to the post
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("postId") REFERENCES "Post"("id"),		-- Foreign key to the Post table.
	FOREIGN KEY ("postTagId") REFERENCES "PostTag"("id"),		-- Foreign key to the PostTag table.
	UNIQUE ( "postId", "postTagId") 		-- Uniqueness enforced on the PostTagAssignment table's postId and postTagId fields.
);
-- Index on the PostTagAssignment table's postId field.
CREATE INDEX "I_PostTagAssignment_postId" ON "PostTagAssignment" ("postId")
;

-- Index on the PostTagAssignment table's postTagId field.
CREATE INDEX "I_PostTagAssignment_postTagId" ON "PostTagAssignment" ("postTagId")
;

-- Index on the PostTagAssignment table's active field.
CREATE INDEX "I_PostTagAssignment_active" ON "PostTagAssignment" ("active")
;

-- Index on the PostTagAssignment table's deleted field.
CREATE INDEX "I_PostTagAssignment_deleted" ON "PostTagAssignment" ("deleted")
;


-- Centralized media library for uploaded images, documents, and other files used across the CMS.
CREATE TABLE "MediaAsset"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"fileName" VARCHAR(250) NOT NULL COLLATE NOCASE,		-- Original filename as uploaded (e.g. 'council-photo-2026.jpg')
	"filePath" VARCHAR(500) NOT NULL COLLATE NOCASE,		-- Server-relative storage path for the file
	"mimeType" VARCHAR(100) NOT NULL COLLATE NOCASE,		-- MIME type (e.g. 'image/jpeg', 'application/pdf')
	"altText" VARCHAR(250) NULL COLLATE NOCASE,		-- Alt text for accessibility and SEO (images)
	"caption" VARCHAR(500) NULL COLLATE NOCASE,		-- Display caption for the media item
	"fileSizeBytes" BIGINT NULL,		-- File size in bytes
	"imageWidth" INTEGER NULL,		-- Width in pixels (for images only)
	"imageHeight" INTEGER NULL,		-- Height in pixels (for images only)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the MediaAsset table's tenantGuid field.
CREATE INDEX "I_MediaAsset_tenantGuid" ON "MediaAsset" ("tenantGuid")
;

-- Index on the MediaAsset table's tenantGuid,active fields.
CREATE INDEX "I_MediaAsset_tenantGuid_active" ON "MediaAsset" ("tenantGuid", "active")
;

-- Index on the MediaAsset table's tenantGuid,deleted fields.
CREATE INDEX "I_MediaAsset_tenantGuid_deleted" ON "MediaAsset" ("tenantGuid", "deleted")
;


-- Binary storage for media asset file data. Separated from MediaAsset to keep metadata queries lightweight. One-to-one relationship with MediaAsset.
CREATE TABLE "MediaContent"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"mediaAssetId" INTEGER NOT NULL,		-- The media asset this content belongs to
	"fileData" BLOB NOT NULL,		-- Binary file content (varbinary MAX)
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("mediaAssetId") REFERENCES "MediaAsset"("id")		-- Foreign key to the MediaAsset table.
);
-- Index on the MediaContent table's tenantGuid field.
CREATE INDEX "I_MediaContent_tenantGuid" ON "MediaContent" ("tenantGuid")
;

-- Index on the MediaContent table's tenantGuid,mediaAssetId fields.
CREATE INDEX "I_MediaContent_tenantGuid_mediaAssetId" ON "MediaContent" ("tenantGuid", "mediaAssetId")
;

-- Index on the MediaContent table's tenantGuid,active fields.
CREATE INDEX "I_MediaContent_tenantGuid_active" ON "MediaContent" ("tenantGuid", "active")
;

-- Index on the MediaContent table's tenantGuid,deleted fields.
CREATE INDEX "I_MediaContent_tenantGuid_deleted" ON "MediaContent" ("tenantGuid", "deleted")
;


-- Named navigation menus for different positions on the site (e.g. header, footer, sidebar).
CREATE TABLE "Menu"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL COLLATE NOCASE,
	"location" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- Where this menu is displayed: header, footer, sidebar
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	UNIQUE ( "tenantGuid", "name") 		-- Uniqueness enforced on the Menu table's tenantGuid and name fields.
);
-- Index on the Menu table's tenantGuid field.
CREATE INDEX "I_Menu_tenantGuid" ON "Menu" ("tenantGuid")
;

-- Index on the Menu table's tenantGuid,name fields.
CREATE INDEX "I_Menu_tenantGuid_name" ON "Menu" ("tenantGuid", "name")
;

-- Index on the Menu table's tenantGuid,active fields.
CREATE INDEX "I_Menu_tenantGuid_active" ON "Menu" ("tenantGuid", "active")
;

-- Index on the Menu table's tenantGuid,deleted fields.
CREATE INDEX "I_Menu_tenantGuid_deleted" ON "Menu" ("tenantGuid", "deleted")
;

INSERT INTO "Menu" ( "tenantGuid", "name", "location", "objectGuid" ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'Main Navigation', 'header', 'c0b10001-0001-4000-8000-000000000001' );

INSERT INTO "Menu" ( "tenantGuid", "name", "location", "objectGuid" ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'Footer Links', 'footer', 'c0b10001-0001-4000-8000-000000000002' );


-- Individual navigation items within a menu. Supports tree hierarchy via self-referencing parent FK.
CREATE TABLE "MenuItem"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"menuId" INTEGER NOT NULL,		-- The menu this item belongs to
	"label" VARCHAR(250) NOT NULL COLLATE NOCASE,		-- Display text for the menu item
	"url" VARCHAR(500) NULL COLLATE NOCASE,		-- External or absolute URL (used if pageId is null)
	"pageId" INTEGER NULL,		-- Optional link to an internal CMS page (takes priority over url)
	"parentMenuItemId" INTEGER NULL,		-- Parent menu item for sub-menu hierarchy (null = top-level)
	"iconClass" VARCHAR(50) NULL COLLATE NOCASE,		-- Optional CSS icon class (e.g. 'fa-home', 'bi-calendar')
	"openInNewTab" BIT NOT NULL DEFAULT 0,		-- Whether clicking this menu item opens in a new browser tab
	"sequence" INTEGER NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("menuId") REFERENCES "Menu"("id"),		-- Foreign key to the Menu table.
	FOREIGN KEY ("pageId") REFERENCES "Page"("id"),		-- Foreign key to the Page table.
	FOREIGN KEY ("parentMenuItemId") REFERENCES "MenuItem"("id")		-- Foreign key to the MenuItem table.
);
-- Index on the MenuItem table's tenantGuid field.
CREATE INDEX "I_MenuItem_tenantGuid" ON "MenuItem" ("tenantGuid")
;

-- Index on the MenuItem table's tenantGuid,menuId fields.
CREATE INDEX "I_MenuItem_tenantGuid_menuId" ON "MenuItem" ("tenantGuid", "menuId")
;

-- Index on the MenuItem table's tenantGuid,pageId fields.
CREATE INDEX "I_MenuItem_tenantGuid_pageId" ON "MenuItem" ("tenantGuid", "pageId")
;

-- Index on the MenuItem table's tenantGuid,parentMenuItemId fields.
CREATE INDEX "I_MenuItem_tenantGuid_parentMenuItemId" ON "MenuItem" ("tenantGuid", "parentMenuItemId")
;

-- Index on the MenuItem table's tenantGuid,active fields.
CREATE INDEX "I_MenuItem_tenantGuid_active" ON "MenuItem" ("tenantGuid", "active")
;

-- Index on the MenuItem table's tenantGuid,deleted fields.
CREATE INDEX "I_MenuItem_tenantGuid_deleted" ON "MenuItem" ("tenantGuid", "deleted")
;


-- Key-value configuration for the public site (site name, tagline, logo URL, social media links, theme settings).
CREATE TABLE "SiteSetting"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"settingKey" VARCHAR(100) NOT NULL COLLATE NOCASE,		-- Unique setting identifier (e.g. 'siteName', 'tagline', 'logoUrl')
	"settingValue" TEXT NULL COLLATE NOCASE,		-- The value for this setting
	"description" VARCHAR(250) NULL COLLATE NOCASE,		-- Human-readable description of what this setting controls
	"settingGroup" VARCHAR(50) NULL COLLATE NOCASE,		-- Grouping for settings UI (e.g. 'General', 'Social', 'SEO', 'Theme')
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the SiteSetting table's tenantGuid field.
CREATE INDEX "I_SiteSetting_tenantGuid" ON "SiteSetting" ("tenantGuid")
;

-- Index on the SiteSetting table's tenantGuid,settingKey fields.
CREATE UNIQUE INDEX "I_SiteSetting_tenantGuid_settingKey" ON "SiteSetting" ("tenantGuid", "settingKey")
;

-- Index on the SiteSetting table's tenantGuid,active fields.
CREATE INDEX "I_SiteSetting_tenantGuid_active" ON "SiteSetting" ("tenantGuid", "active")
;

-- Index on the SiteSetting table's tenantGuid,deleted fields.
CREATE INDEX "I_SiteSetting_tenantGuid_deleted" ON "SiteSetting" ("tenantGuid", "deleted")
;

INSERT INTO "SiteSetting" ( "tenantGuid", "settingKey", "settingValue", "description", "settingGroup", "objectGuid" ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'siteName', 'Community', 'The name of the site displayed in the header and browser tab', 'General', 'c0c10001-0001-4000-8000-000000000001' );

INSERT INTO "SiteSetting" ( "tenantGuid", "settingKey", "settingValue", "description", "settingGroup", "objectGuid" ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'tagline', 'Welcome to our community', 'Site tagline displayed below the site name', 'General', 'c0c10001-0001-4000-8000-000000000002' );

INSERT INTO "SiteSetting" ( "tenantGuid", "settingKey", "settingValue", "description", "settingGroup", "objectGuid" ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'logoUrl', '', 'URL to the site logo image', 'General', 'c0c10001-0001-4000-8000-000000000003' );

INSERT INTO "SiteSetting" ( "tenantGuid", "settingKey", "settingValue", "description", "settingGroup", "objectGuid" ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'footerText', '© 2026 K2 Research. All rights reserved.', 'Copyright text displayed in the site footer', 'General', 'c0c10001-0001-4000-8000-000000000004' );

INSERT INTO "SiteSetting" ( "tenantGuid", "settingKey", "settingValue", "description", "settingGroup", "objectGuid" ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'contactEmail', '', 'Primary contact email address', 'General', 'c0c10001-0001-4000-8000-000000000005' );

INSERT INTO "SiteSetting" ( "tenantGuid", "settingKey", "settingValue", "description", "settingGroup", "objectGuid" ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'contactPhone', '', 'Primary contact phone number', 'General', 'c0c10001-0001-4000-8000-000000000006' );

INSERT INTO "SiteSetting" ( "tenantGuid", "settingKey", "settingValue", "description", "settingGroup", "objectGuid" ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'facebookUrl', '', 'Facebook page URL', 'Social', 'c0c10001-0001-4000-8000-000000000010' );

INSERT INTO "SiteSetting" ( "tenantGuid", "settingKey", "settingValue", "description", "settingGroup", "objectGuid" ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'twitterUrl', '', 'Twitter/X profile URL', 'Social', 'c0c10001-0001-4000-8000-000000000011' );

INSERT INTO "SiteSetting" ( "tenantGuid", "settingKey", "settingValue", "description", "settingGroup", "objectGuid" ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'instagramUrl', '', 'Instagram profile URL', 'Social', 'c0c10001-0001-4000-8000-000000000012' );

INSERT INTO "SiteSetting" ( "tenantGuid", "settingKey", "settingValue", "description", "settingGroup", "objectGuid" ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'heroTitle', 'Welcome', 'Hero section title on the home page', 'HomePage', 'c0c10001-0001-4000-8000-000000000020' );

INSERT INTO "SiteSetting" ( "tenantGuid", "settingKey", "settingValue", "description", "settingGroup", "objectGuid" ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'heroSubtitle', '', 'Hero section subtitle on the home page', 'HomePage', 'c0c10001-0001-4000-8000-000000000021' );

INSERT INTO "SiteSetting" ( "tenantGuid", "settingKey", "settingValue", "description", "settingGroup", "objectGuid" ) VALUES  ( 'd58f56c6-e3fb-4d3b-80b3-7053c66491e3', 'heroImageUrl', '', 'Hero section background image URL', 'HomePage', 'c0c10001-0001-4000-8000-000000000022' );


-- Time-bound announcements displayed on the public site (council meeting notices, public consultations, service disruptions).
CREATE TABLE "Announcement"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"title" VARCHAR(250) NOT NULL COLLATE NOCASE,		-- Announcement title
	"body" TEXT NULL COLLATE NOCASE,		-- Full announcement body content (HTML or Markdown)
	"severity" VARCHAR(50) NOT NULL DEFAULT 'info' COLLATE NOCASE,		-- Severity level: info, warning, urgent
	"startDate" DATETIME NOT NULL,		-- When the announcement becomes visible
	"endDate" DATETIME NULL,		-- When the announcement expires (null = no expiration)
	"isPinned" BIT NOT NULL DEFAULT 0,		-- Whether this announcement should always appear at the top
	"versionNumber" INTEGER NOT NULL DEFAULT 1,		-- The version number of this record.  Increased by one each time the record changes, and the change history is tracked in the table's change history table.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the Announcement table's tenantGuid field.
CREATE INDEX "I_Announcement_tenantGuid" ON "Announcement" ("tenantGuid")
;

-- Index on the Announcement table's tenantGuid,active fields.
CREATE INDEX "I_Announcement_tenantGuid_active" ON "Announcement" ("tenantGuid", "active")
;

-- Index on the Announcement table's tenantGuid,deleted fields.
CREATE INDEX "I_Announcement_tenantGuid_deleted" ON "Announcement" ("tenantGuid", "deleted")
;


-- The change history for records from the Announcement table.
CREATE TABLE "AnnouncementChangeHistory"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"announcementId" INTEGER NOT NULL,		-- Link to the Announcement table.
	"versionNumber" INTEGER NOT NULL,		-- This is the version number that is being historized.
	"timeStamp" DATETIME NOT NULL,		-- The time that the record version was created.
	"userId" INTEGER NOT NULL,
	"data" TEXT NOT NULL COLLATE NOCASE,		-- This stores the JSON representing the object's historical state.
	FOREIGN KEY ("announcementId") REFERENCES "Announcement"("id")		-- Foreign key to the Announcement table.
);
-- Index on the AnnouncementChangeHistory table's tenantGuid field.
CREATE INDEX "I_AnnouncementChangeHistory_tenantGuid" ON "AnnouncementChangeHistory" ("tenantGuid")
;

-- Index on the AnnouncementChangeHistory table's tenantGuid,versionNumber fields.
CREATE INDEX "I_AnnouncementChangeHistory_tenantGuid_versionNumber" ON "AnnouncementChangeHistory" ("tenantGuid", "versionNumber")
;

-- Index on the AnnouncementChangeHistory table's tenantGuid,timeStamp fields.
CREATE INDEX "I_AnnouncementChangeHistory_tenantGuid_timeStamp" ON "AnnouncementChangeHistory" ("tenantGuid", "timeStamp")
;

-- Index on the AnnouncementChangeHistory table's tenantGuid,userId fields.
CREATE INDEX "I_AnnouncementChangeHistory_tenantGuid_userId" ON "AnnouncementChangeHistory" ("tenantGuid", "userId")
;

-- Index on the AnnouncementChangeHistory table's tenantGuid,announcementId fields.
CREATE INDEX "I_AnnouncementChangeHistory_tenantGuid_announcementId" ON "AnnouncementChangeHistory" ("tenantGuid", "announcementId", "versionNumber", "timeStamp", "userId")
;


-- Photo albums for organizing gallery images (e.g. 'Community Day 2026', 'Town Views', 'Heritage Photos').
CREATE TABLE "GalleryAlbum"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"title" VARCHAR(250) NOT NULL COLLATE NOCASE,		-- Album title
	"slug" VARCHAR(250) NOT NULL COLLATE NOCASE,		-- URL-friendly slug for the album
	"description" TEXT NULL COLLATE NOCASE,		-- Description of the album
	"coverImageUrl" VARCHAR(500) NULL COLLATE NOCASE,		-- URL to the album cover image (or uses first image if null)
	"isPublished" BIT NOT NULL DEFAULT 1,		-- Whether this album is visible on the public site
	"sequence" INTEGER NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the GalleryAlbum table's tenantGuid field.
CREATE INDEX "I_GalleryAlbum_tenantGuid" ON "GalleryAlbum" ("tenantGuid")
;

-- Index on the GalleryAlbum table's tenantGuid,slug fields.
CREATE UNIQUE INDEX "I_GalleryAlbum_tenantGuid_slug" ON "GalleryAlbum" ("tenantGuid", "slug")
;

-- Index on the GalleryAlbum table's tenantGuid,active fields.
CREATE INDEX "I_GalleryAlbum_tenantGuid_active" ON "GalleryAlbum" ("tenantGuid", "active")
;

-- Index on the GalleryAlbum table's tenantGuid,deleted fields.
CREATE INDEX "I_GalleryAlbum_tenantGuid_deleted" ON "GalleryAlbum" ("tenantGuid", "deleted")
;


-- Individual images within a gallery album.
CREATE TABLE "GalleryImage"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"galleryAlbumId" INTEGER NOT NULL,		-- The album this image belongs to
	"imageUrl" VARCHAR(500) NOT NULL COLLATE NOCASE,		-- URL or relative path to the image file
	"caption" VARCHAR(500) NULL COLLATE NOCASE,		-- Display caption for the image
	"altText" VARCHAR(250) NULL COLLATE NOCASE,		-- Alt text for accessibility
	"sequence" INTEGER NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0,		-- Soft deletion flag.
	FOREIGN KEY ("galleryAlbumId") REFERENCES "GalleryAlbum"("id")		-- Foreign key to the GalleryAlbum table.
);
-- Index on the GalleryImage table's tenantGuid field.
CREATE INDEX "I_GalleryImage_tenantGuid" ON "GalleryImage" ("tenantGuid")
;

-- Index on the GalleryImage table's tenantGuid,galleryAlbumId fields.
CREATE INDEX "I_GalleryImage_tenantGuid_galleryAlbumId" ON "GalleryImage" ("tenantGuid", "galleryAlbumId")
;

-- Index on the GalleryImage table's tenantGuid,active fields.
CREATE INDEX "I_GalleryImage_tenantGuid_active" ON "GalleryImage" ("tenantGuid", "active")
;

-- Index on the GalleryImage table's tenantGuid,deleted fields.
CREATE INDEX "I_GalleryImage_tenantGuid_deleted" ON "GalleryImage" ("tenantGuid", "deleted")
;


-- Downloadable documents for public access (council minutes, schedules, forms, regulations). Replaces the PDF-as-image pattern.
CREATE TABLE "DocumentDownload"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"title" VARCHAR(250) NOT NULL COLLATE NOCASE,		-- Display title for the document
	"description" TEXT NULL COLLATE NOCASE,		-- Description of what this document contains
	"filePath" VARCHAR(500) NOT NULL COLLATE NOCASE,		-- Server-relative path to the downloadable file
	"fileName" VARCHAR(100) NOT NULL COLLATE NOCASE,		-- Original filename for download (e.g. 'recycling-schedule-2026.pdf')
	"mimeType" VARCHAR(100) NULL COLLATE NOCASE,		-- File MIME type
	"fileSizeBytes" BIGINT NULL,		-- File size in bytes
	"categoryName" VARCHAR(100) NULL COLLATE NOCASE,		-- Grouping category (e.g. 'Council Minutes', 'Schedules', 'Forms', 'Regulations')
	"documentDate" DATETIME NULL,		-- Date associated with the document (e.g. meeting date for council minutes)
	"isPublished" BIT NOT NULL DEFAULT 1,		-- Whether this document is visible for public download
	"sequence" INTEGER NULL,		-- Sequence to use for sorting.
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the DocumentDownload table's tenantGuid field.
CREATE INDEX "I_DocumentDownload_tenantGuid" ON "DocumentDownload" ("tenantGuid")
;

-- Index on the DocumentDownload table's tenantGuid,active fields.
CREATE INDEX "I_DocumentDownload_tenantGuid_active" ON "DocumentDownload" ("tenantGuid", "active")
;

-- Index on the DocumentDownload table's tenantGuid,deleted fields.
CREATE INDEX "I_DocumentDownload_tenantGuid_deleted" ON "DocumentDownload" ("tenantGuid", "deleted")
;


-- Captures contact form submissions from public site visitors.
CREATE TABLE "ContactSubmission"
(
	"id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"tenantGuid" VARCHAR(50) NOT NULL COLLATE NOCASE,		-- The guid for the Tenant to which this record belongs.
	"name" VARCHAR(100) NOT NULL COLLATE NOCASE,		-- Name of the person submitting the form
	"email" VARCHAR(250) NOT NULL COLLATE NOCASE,		-- Email address for reply
	"subject" VARCHAR(250) NULL COLLATE NOCASE,		-- Subject line of the message
	"message" TEXT NOT NULL COLLATE NOCASE,		-- Full message body
	"submittedDate" DATETIME NOT NULL,		-- When the form was submitted
	"isRead" BIT NOT NULL DEFAULT 0,		-- Whether an admin has read this submission
	"isArchived" BIT NOT NULL DEFAULT 0,		-- Whether this submission has been archived
	"adminNotes" VARCHAR(500) NULL COLLATE NOCASE,		-- Internal notes from admin about the submission
	"objectGuid" VARCHAR(50) NOT NULL UNIQUE COLLATE NOCASE,		-- Unique identifier for this table.
	"active" BIT NOT NULL DEFAULT 1,		-- Active from a business perspective flag.
	"deleted" BIT NOT NULL DEFAULT 0		-- Soft deletion flag.

);
-- Index on the ContactSubmission table's tenantGuid field.
CREATE INDEX "I_ContactSubmission_tenantGuid" ON "ContactSubmission" ("tenantGuid")
;

-- Index on the ContactSubmission table's tenantGuid,active fields.
CREATE INDEX "I_ContactSubmission_tenantGuid_active" ON "ContactSubmission" ("tenantGuid", "active")
;

-- Index on the ContactSubmission table's tenantGuid,deleted fields.
CREATE INDEX "I_ContactSubmission_tenantGuid_deleted" ON "ContactSubmission" ("tenantGuid", "deleted")
;


