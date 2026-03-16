using Foundation.CodeGeneration;
using System;
using System.Collections.Generic;
using static Foundation.CodeGeneration.DatabaseGenerator;


namespace Foundation.Community.Database
{
    /// <summary>
    /// Complete database schema generator for the Community CMS system.
    /// 
    /// Community is a public-facing content management module that sits alongside the Scheduler.
    /// It provides:
    ///   - Static page management (About, History, FAQ, etc.)
    ///   - Blog/news post publishing with categories and tags
    ///   - Media asset library for uploaded images and documents
    ///   - Navigation menu management
    ///   - Announcements with time-based visibility
    ///   - Photo gallery with albums
    ///   - Downloadable document library
    ///   - Contact form submissions
    ///   - Site-wide settings (name, tagline, logo, social links)
    /// 
    /// The public-facing site also reads from the Scheduler database for:
    ///   - Public events calendar
    ///   - Office/location information
    ///   - Volunteer group listings
    ///   - Team member/resource profiles
    /// 
    /// Reference: http://www.pettyharbourmaddoxcove.ca/ (to match and exceed)
    /// </summary>
    public class CommunityDatabaseGenerator : DatabaseGenerator
    {
        // ── Permission levels ──

        // Read level for all public content tables
        private const int COMMUNITY_READER_PERMISSION_LEVEL = 1;

        // Write levels – tiered by functional area
        private const int COMMUNITY_CONTENT_WRITER_PERMISSION_LEVEL = 10;        // Pages, posts, media, announcements
        private const int COMMUNITY_NAVIGATION_WRITER_PERMISSION_LEVEL = 50;     // Menus, site structure
        private const int COMMUNITY_ADMIN_PERMISSION_LEVEL = 100;                // Site settings, moderation
        private const int COMMUNITY_SUPER_ADMIN_PERMISSION_LEVEL = 255;          // System admin only


        // ── Custom role names ──
        private const string COMMUNITY_CONTENT_WRITER_ROLE = "Community Content Writer";
        private const string COMMUNITY_NAVIGATION_WRITER_ROLE = "Community Navigation Writer";
        private const string COMMUNITY_ADMIN_ROLE = "Community Admin";

        private static readonly Guid PHMCTenantGuid = Guid.Parse("d58f56c6-e3fb-4d3b-80b3-7053c66491e3");

        public CommunityDatabaseGenerator() : base("Community", "Community")
        {
            database.comment = @"Community CMS database schema.
This is the content management module for the public-facing community website.
It provides page management, blog publishing, media library, navigation menus,
announcements, photo galleries, document downloads, contact form handling,
and site-wide configuration.
All operational tables include auditing and security controls.";

            this.database.SetSchemaName("Community");

            //
            // Register custom roles for granular write access control
            //
            this.database.AddCustomRole(COMMUNITY_CONTENT_WRITER_ROLE, $"{COMMUNITY_CONTENT_WRITER_ROLE} Role");
            this.database.AddCustomRole(COMMUNITY_NAVIGATION_WRITER_ROLE, $"{COMMUNITY_NAVIGATION_WRITER_ROLE} Role");
            this.database.AddCustomRole(COMMUNITY_ADMIN_ROLE, $"{COMMUNITY_ADMIN_ROLE} Role");


            #region Content Pages

            // -------------------------------------------------
            // Page — Static content pages (About, History, FAQ, etc.)
            // -------------------------------------------------
            Database.Table pageTable = database.AddTable("Page");
            pageTable.comment = "Static content pages for the public website (e.g. About, History, FAQ, Contact, Regulations).";
            pageTable.SetMinimumPermissionLevels(COMMUNITY_READER_PERMISSION_LEVEL, COMMUNITY_CONTENT_WRITER_PERMISSION_LEVEL);
            pageTable.customWriteAccessRole = COMMUNITY_CONTENT_WRITER_ROLE;
            pageTable.AddIdField();
            pageTable.AddMultiTenantSupport();
            pageTable.AddString250Field("title", false).AddScriptComments("Display title of the page");
            pageTable.AddString250Field("slug", false).AddScriptComments("URL-friendly slug (e.g. 'about', 'history', 'faq')").CreateIndex(true);
            pageTable.AddTextField("body").AddScriptComments("HTML or Markdown body content of the page");
            pageTable.AddString500Field("metaDescription").AddScriptComments("SEO meta description for search engines and social sharing");
            pageTable.AddString500Field("featuredImageUrl").AddScriptComments("URL or relative path to the page's featured/hero image");
            pageTable.AddBoolField("isPublished", false, false).AddScriptComments("Whether this page is visible on the public site");
            pageTable.AddDateTimeField("publishedDate", true).AddScriptComments("When the page was first published (null if draft)");
            pageTable.AddIntField("sortOrder", true, 0).AddScriptComments("Sort order for page listings and navigation");

            pageTable.AddVersionControl();
            pageTable.AddControlFields();

            #endregion


            #region Blog / News

            // -------------------------------------------------
            // PostCategory — Category taxonomy for blog posts - Not multi tenanted on purpose because this is a Community focussed CMS, and these categories are universal in that regard.  Forcing each tenant to likely define the same set is counter intuitive.
            // -------------------------------------------------
            Database.Table postCategoryTable = database.AddTable("PostCategory");
            postCategoryTable.comment = "Category taxonomy for organizing blog/news posts (e.g. News, Council Updates, Community Events).";
            postCategoryTable.SetMinimumPermissionLevels(COMMUNITY_READER_PERMISSION_LEVEL, COMMUNITY_CONTENT_WRITER_PERMISSION_LEVEL);
            postCategoryTable.customWriteAccessRole = COMMUNITY_CONTENT_WRITER_ROLE;
            postCategoryTable.AddIdField();
            
            postCategoryTable.AddNameAndDescriptionFields(true, true, true);
            postCategoryTable.AddString250Field("slug", false).AddScriptComments("URL-friendly slug for the category").CreateIndex(true);
            postCategoryTable.AddSequenceField();
            postCategoryTable.AddControlFields();

            // Seed default categories
            postCategoryTable.AddData(new Dictionary<string, string> { { "name", "News" }, { "description", "General news and updates" }, { "slug", "news" }, { "sequence", "1" }, { "objectGuid", "c0a10001-0001-4000-8000-000000000001" } });
            postCategoryTable.AddData(new Dictionary<string, string> { { "name", "Council Updates" }, { "description", "Updates from council meetings and decisions" }, { "slug", "council-updates" }, { "sequence", "2" }, { "objectGuid", "c0a10001-0001-4000-8000-000000000002" } });
            postCategoryTable.AddData(new Dictionary<string, string> { { "name", "Community Events" }, { "description", "Upcoming and past community events" }, { "slug", "community-events" }, { "sequence", "3" }, { "objectGuid", "c0a10001-0001-4000-8000-000000000003" } });
            postCategoryTable.AddData(new Dictionary<string, string> { { "name", "Volunteer Spotlight" }, { "description", "Highlighting volunteer contributions and stories" }, { "slug", "volunteer-spotlight" }, { "sequence", "4" }, { "objectGuid", "c0a10001-0001-4000-8000-000000000004" } });
            postCategoryTable.AddData(new Dictionary<string, string> { { "name", "Announcements" }, { "description", "Official announcements and notices" }, { "slug", "announcements" }, { "sequence", "5" }, { "objectGuid", "c0a10001-0001-4000-8000-000000000005" } });


            // -------------------------------------------------
            // Post — Blog/news articles
            // -------------------------------------------------
            Database.Table postTable = database.AddTable("Post");
            postTable.comment = "Blog and news articles for the public website. Supports categories, featured images, and publish scheduling.";
            postTable.SetMinimumPermissionLevels(COMMUNITY_READER_PERMISSION_LEVEL, COMMUNITY_CONTENT_WRITER_PERMISSION_LEVEL);
            postTable.customWriteAccessRole = COMMUNITY_CONTENT_WRITER_ROLE;
            postTable.AddIdField();
            postTable.AddMultiTenantSupport();
            postTable.AddString250Field("title", false).AddScriptComments("Display title of the post").CreateIndex();
            postTable.AddString250Field("slug", false).AddScriptComments("URL-friendly slug for the post").CreateIndex(true);
            postTable.AddTextField("body").AddScriptComments("Full HTML or Markdown body content of the post");
            postTable.AddString500Field("excerpt").AddScriptComments("Short summary for post listings and SEO (manually entered or auto-generated)");
            postTable.AddString100Field("authorName").AddScriptComments("Display name of the post author");
            postTable.AddForeignKeyField(postCategoryTable, true).AddScriptComments("Category this post belongs to (null for uncategorized)");
            postTable.AddString500Field("featuredImageUrl").AddScriptComments("URL or relative path to the post's featured image");
            postTable.AddString500Field("metaDescription").AddScriptComments("SEO meta description for search engines");
            postTable.AddBoolField("isPublished", false, false).AddScriptComments("Whether this post is visible on the public site");
            postTable.AddDateTimeField("publishedDate", true).AddScriptComments("When the post was published (null if draft, allows future-dating for scheduled publishing)");
            postTable.AddBoolField("isFeatured", false, false).AddScriptComments("Whether this post should appear in featured/highlighted sections");

            postTable.AddVersionControl();
            postTable.AddControlFields();


            // -------------------------------------------------
            // PostTag — Tag taxonomy for posts
            // -------------------------------------------------
            Database.Table postTagTable = database.AddTable("PostTag");
            postTagTable.comment = "Freeform tags for cross-cutting post categorization (e.g. urgent, budget, recycling).";
            postTagTable.SetMinimumPermissionLevels(COMMUNITY_READER_PERMISSION_LEVEL, COMMUNITY_CONTENT_WRITER_PERMISSION_LEVEL);
            postTagTable.customWriteAccessRole = COMMUNITY_CONTENT_WRITER_ROLE;
            postTagTable.AddIdField();
            postTagTable.AddMultiTenantSupport();

            postTagTable.AddNameField(true, true);
            postTagTable.AddString250Field("slug", false).AddScriptComments("URL-friendly slug for the tag").CreateIndex(true);
            postTagTable.AddControlFields();


            // -------------------------------------------------
            // PostTagAssignment — Many-to-many: posts ↔ tags
            // -------------------------------------------------
            Database.Table postTagAssignmentTable = database.AddTable("PostTagAssignment");
            postTagAssignmentTable.comment = "Many-to-many mapping between posts and tags.";
            postTagAssignmentTable.SetMinimumPermissionLevels(COMMUNITY_READER_PERMISSION_LEVEL, COMMUNITY_CONTENT_WRITER_PERMISSION_LEVEL);
            postTagAssignmentTable.customWriteAccessRole = COMMUNITY_CONTENT_WRITER_ROLE;
            postTagAssignmentTable.AddIdField();

            postTagAssignmentTable.AddForeignKeyField(postTable, false).AddScriptComments("The post being tagged");
            postTagAssignmentTable.AddForeignKeyField(postTagTable, false).AddScriptComments("The tag applied to the post");
            postTagAssignmentTable.AddControlFields();

            postTagAssignmentTable.AddUniqueConstraint(new List<string>() { "postId", "postTagId" }, false);

            #endregion


            #region Media Library

            // -------------------------------------------------
            // MediaAsset — Uploaded files (images, documents, videos)
            // -------------------------------------------------
            Database.Table mediaAssetTable = database.AddTable("MediaAsset");
            mediaAssetTable.comment = "Centralized media library for uploaded images, documents, and other files used across the CMS.";
            mediaAssetTable.SetMinimumPermissionLevels(COMMUNITY_READER_PERMISSION_LEVEL, COMMUNITY_CONTENT_WRITER_PERMISSION_LEVEL);
            mediaAssetTable.customWriteAccessRole = COMMUNITY_CONTENT_WRITER_ROLE;
            mediaAssetTable.AddIdField();
            mediaAssetTable.AddMultiTenantSupport();
            mediaAssetTable.AddString250Field("fileName", false).AddScriptComments("Original filename as uploaded (e.g. 'council-photo-2026.jpg')");
            mediaAssetTable.AddString500Field("filePath", false).AddScriptComments("Server-relative storage path for the file");
            mediaAssetTable.AddString100Field("mimeType", false).AddScriptComments("MIME type (e.g. 'image/jpeg', 'application/pdf')");
            mediaAssetTable.AddString250Field("altText").AddScriptComments("Alt text for accessibility and SEO (images)");
            mediaAssetTable.AddString500Field("caption").AddScriptComments("Display caption for the media item");
            mediaAssetTable.AddLongField("fileSizeBytes", true).AddScriptComments("File size in bytes");
            mediaAssetTable.AddIntField("imageWidth", true).AddScriptComments("Width in pixels (for images only)");
            mediaAssetTable.AddIntField("imageHeight", true).AddScriptComments("Height in pixels (for images only)");

            mediaAssetTable.AddControlFields();


            // -------------------------------------------------
            // MediaContent — Binary file data for media assets
            // -------------------------------------------------
            Database.Table mediaContentTable = database.AddTable("MediaContent");
            mediaContentTable.comment = "Binary storage for media asset file data. Separated from MediaAsset to keep metadata queries lightweight. One-to-one relationship with MediaAsset.";
            mediaContentTable.SetMinimumPermissionLevels(COMMUNITY_READER_PERMISSION_LEVEL, COMMUNITY_CONTENT_WRITER_PERMISSION_LEVEL);
            mediaContentTable.customWriteAccessRole = COMMUNITY_CONTENT_WRITER_ROLE;
            mediaContentTable.AddIdField();
            mediaContentTable.AddMultiTenantSupport();
            mediaContentTable.AddForeignKeyField(mediaAssetTable, false).AddScriptComments("The media asset this content belongs to");
            mediaContentTable.AddBinaryField("fileData", false).AddScriptComments("Binary file content (varbinary MAX)");
            mediaContentTable.AddControlFields();

            #endregion


            #region Navigation

            // -------------------------------------------------
            // Menu — Named navigation menus
            // -------------------------------------------------
            Database.Table menuTable = database.AddTable("Menu");
            menuTable.comment = "Named navigation menus for different positions on the site (e.g. header, footer, sidebar).";
            menuTable.SetMinimumPermissionLevels(COMMUNITY_READER_PERMISSION_LEVEL, COMMUNITY_NAVIGATION_WRITER_PERMISSION_LEVEL);
            menuTable.customWriteAccessRole = COMMUNITY_NAVIGATION_WRITER_ROLE;
            menuTable.AddIdField();
            menuTable.AddMultiTenantSupport();
            menuTable.AddNameField(true, true);
            menuTable.AddString50Field("location", false).AddScriptComments("Where this menu is displayed: header, footer, sidebar");
            menuTable.AddControlFields();

            // Seed default menus
            menuTable.AddData(new Dictionary<string, string> { { "tenantGuid", PHMCTenantGuid.ToString()}, { "name", "Main Navigation" }, { "location", "header" }, { "objectGuid", "c0b10001-0001-4000-8000-000000000001" } });
            menuTable.AddData(new Dictionary<string, string> { { "tenantGuid", PHMCTenantGuid.ToString()}, { "name", "Footer Links" }, { "location", "footer" }, { "objectGuid", "c0b10001-0001-4000-8000-000000000002" } });


            // -------------------------------------------------
            // MenuItem — Individual items within a menu (tree structure)
            // -------------------------------------------------
            Database.Table menuItemTable = database.AddTable("MenuItem");
            menuItemTable.comment = "Individual navigation items within a menu. Supports tree hierarchy via self-referencing parent FK.";
            menuItemTable.SetMinimumPermissionLevels(COMMUNITY_READER_PERMISSION_LEVEL, COMMUNITY_NAVIGATION_WRITER_PERMISSION_LEVEL);
            menuItemTable.customWriteAccessRole = COMMUNITY_NAVIGATION_WRITER_ROLE;
            menuItemTable.AddIdField();
            menuItemTable.AddMultiTenantSupport();
            menuItemTable.AddForeignKeyField(menuTable, false).AddScriptComments("The menu this item belongs to");
            menuItemTable.AddString250Field("label", false).AddScriptComments("Display text for the menu item");
            menuItemTable.AddString500Field("url").AddScriptComments("External or absolute URL (used if pageId is null)");
            menuItemTable.AddForeignKeyField(pageTable, true).AddScriptComments("Optional link to an internal CMS page (takes priority over url)");
            menuItemTable.AddForeignKeyField("parentMenuItemId", menuItemTable, true).AddScriptComments("Parent menu item for sub-menu hierarchy (null = top-level)");
            menuItemTable.AddString50Field("iconClass").AddScriptComments("Optional CSS icon class (e.g. 'fa-home', 'bi-calendar')");
            menuItemTable.AddBoolField("openInNewTab", false, false).AddScriptComments("Whether clicking this menu item opens in a new browser tab");
            menuItemTable.AddSequenceField();
            menuItemTable.AddControlFields();

            #endregion


            #region Site Configuration

            // -------------------------------------------------
            // SiteSetting — Key-value pairs for global site configuration
            // -------------------------------------------------
            Database.Table siteSettingTable = database.AddTable("SiteSetting");
            siteSettingTable.comment = "Key-value configuration for the public site (site name, tagline, logo URL, social media links, theme settings).";
            siteSettingTable.SetMinimumPermissionLevels(COMMUNITY_READER_PERMISSION_LEVEL, COMMUNITY_ADMIN_PERMISSION_LEVEL);
            siteSettingTable.customWriteAccessRole = COMMUNITY_ADMIN_ROLE;
            siteSettingTable.AddIdField();
            siteSettingTable.AddMultiTenantSupport();
            siteSettingTable.AddString100Field("settingKey", false).AddScriptComments("Unique setting identifier (e.g. 'siteName', 'tagline', 'logoUrl')").CreateIndex(true);
            siteSettingTable.AddTextField("settingValue").AddScriptComments("The value for this setting");
            siteSettingTable.AddString250Field("description").AddScriptComments("Human-readable description of what this setting controls");
            siteSettingTable.AddString50Field("settingGroup").AddScriptComments("Grouping for settings UI (e.g. 'General', 'Social', 'SEO', 'Theme')");
            siteSettingTable.AddControlFields();

            // Seed essential settings
            siteSettingTable.AddData(new Dictionary<string, string> { { "tenantGuid", PHMCTenantGuid.ToString() }, { "settingKey", "siteName" }, { "settingValue", "Community" }, { "description", "The name of the site displayed in the header and browser tab" }, { "settingGroup", "General" }, { "objectGuid", "c0c10001-0001-4000-8000-000000000001" } });
            siteSettingTable.AddData(new Dictionary<string, string> { { "tenantGuid", PHMCTenantGuid.ToString() }, { "settingKey", "tagline" }, { "settingValue", "Welcome to our community" }, { "description", "Site tagline displayed below the site name" }, { "settingGroup", "General" }, { "objectGuid", "c0c10001-0001-4000-8000-000000000002" } });
            siteSettingTable.AddData(new Dictionary<string, string> { { "tenantGuid", PHMCTenantGuid.ToString() }, { "settingKey", "logoUrl" }, { "settingValue", "" }, { "description", "URL to the site logo image" }, { "settingGroup", "General" }, { "objectGuid", "c0c10001-0001-4000-8000-000000000003" } });
            siteSettingTable.AddData(new Dictionary<string, string> { { "tenantGuid", PHMCTenantGuid.ToString() }, { "settingKey", "footerText" }, { "settingValue", "© 2026 K2 Research. All rights reserved." }, { "description", "Copyright text displayed in the site footer" }, { "settingGroup", "General" }, { "objectGuid", "c0c10001-0001-4000-8000-000000000004" } });
            siteSettingTable.AddData(new Dictionary<string, string> { { "tenantGuid", PHMCTenantGuid.ToString() }, { "settingKey", "contactEmail" }, { "settingValue", "" }, { "description", "Primary contact email address" }, { "settingGroup", "General" }, { "objectGuid", "c0c10001-0001-4000-8000-000000000005" } });
            siteSettingTable.AddData(new Dictionary<string, string> { { "tenantGuid", PHMCTenantGuid.ToString() }, { "settingKey", "contactPhone" }, { "settingValue", "" }, { "description", "Primary contact phone number" }, { "settingGroup", "General" }, { "objectGuid", "c0c10001-0001-4000-8000-000000000006" } });
            siteSettingTable.AddData(new Dictionary<string, string> { { "tenantGuid", PHMCTenantGuid.ToString() }, { "settingKey", "facebookUrl" }, { "settingValue", "" }, { "description", "Facebook page URL" }, { "settingGroup", "Social" }, { "objectGuid", "c0c10001-0001-4000-8000-000000000010" } });
            siteSettingTable.AddData(new Dictionary<string, string> { { "tenantGuid", PHMCTenantGuid.ToString() }, { "settingKey", "twitterUrl" }, { "settingValue", "" }, { "description", "Twitter/X profile URL" }, { "settingGroup", "Social" }, { "objectGuid", "c0c10001-0001-4000-8000-000000000011" } });
            siteSettingTable.AddData(new Dictionary<string, string> { { "tenantGuid", PHMCTenantGuid.ToString() }, { "settingKey", "instagramUrl" }, { "settingValue", "" }, { "description", "Instagram profile URL" }, { "settingGroup", "Social" }, { "objectGuid", "c0c10001-0001-4000-8000-000000000012" } });
            siteSettingTable.AddData(new Dictionary<string, string> { { "tenantGuid", PHMCTenantGuid.ToString() }, { "settingKey", "heroTitle" }, { "settingValue", "Welcome" }, { "description", "Hero section title on the home page" }, { "settingGroup", "HomePage" }, { "objectGuid", "c0c10001-0001-4000-8000-000000000020" } });
            siteSettingTable.AddData(new Dictionary<string, string> { { "tenantGuid", PHMCTenantGuid.ToString() }, { "settingKey", "heroSubtitle" }, { "settingValue", "" }, { "description", "Hero section subtitle on the home page" }, { "settingGroup", "HomePage" }, { "objectGuid", "c0c10001-0001-4000-8000-000000000021" } });
            siteSettingTable.AddData(new Dictionary<string, string> { { "tenantGuid", PHMCTenantGuid.ToString() }, { "settingKey", "heroImageUrl" }, { "settingValue", "" }, { "description", "Hero section background image URL" }, { "settingGroup", "HomePage" }, { "objectGuid", "c0c10001-0001-4000-8000-000000000022" } });

            #endregion


            #region Announcements

            // -------------------------------------------------
            // Announcement — Time-bound public notices
            // -------------------------------------------------
            Database.Table announcementTable = database.AddTable("Announcement");
            announcementTable.comment = "Time-bound announcements displayed on the public site (council meeting notices, public consultations, service disruptions).";
            announcementTable.SetMinimumPermissionLevels(COMMUNITY_READER_PERMISSION_LEVEL, COMMUNITY_CONTENT_WRITER_PERMISSION_LEVEL);
            announcementTable.customWriteAccessRole = COMMUNITY_CONTENT_WRITER_ROLE;
            announcementTable.AddIdField();
            announcementTable.AddMultiTenantSupport();
            announcementTable.AddString250Field("title", false).AddScriptComments("Announcement title");
            announcementTable.AddTextField("body").AddScriptComments("Full announcement body content (HTML or Markdown)");
            announcementTable.AddString50Field("severity", false, "info").AddScriptComments("Severity level: info, warning, urgent");
            announcementTable.AddDateTimeField("startDate", false).AddScriptComments("When the announcement becomes visible");
            announcementTable.AddDateTimeField("endDate", true).AddScriptComments("When the announcement expires (null = no expiration)");
            announcementTable.AddBoolField("isPinned", false, false).AddScriptComments("Whether this announcement should always appear at the top");

            announcementTable.AddVersionControl();
            announcementTable.AddControlFields();

            #endregion


            #region Photo Gallery

            // -------------------------------------------------
            // GalleryAlbum — Photo album groupings
            // -------------------------------------------------
            Database.Table galleryAlbumTable = database.AddTable("GalleryAlbum");
            galleryAlbumTable.comment = "Photo albums for organizing gallery images (e.g. 'Community Day 2026', 'Town Views', 'Heritage Photos').";
            galleryAlbumTable.SetMinimumPermissionLevels(COMMUNITY_READER_PERMISSION_LEVEL, COMMUNITY_CONTENT_WRITER_PERMISSION_LEVEL);
            galleryAlbumTable.customWriteAccessRole = COMMUNITY_CONTENT_WRITER_ROLE;
            galleryAlbumTable.AddIdField();
            galleryAlbumTable.AddMultiTenantSupport();
            galleryAlbumTable.AddString250Field("title", false).AddScriptComments("Album title");
            galleryAlbumTable.AddString250Field("slug", false).AddScriptComments("URL-friendly slug for the album").CreateIndex(true);
            galleryAlbumTable.AddTextField("description").AddScriptComments("Description of the album");
            galleryAlbumTable.AddString500Field("coverImageUrl").AddScriptComments("URL to the album cover image (or uses first image if null)");
            galleryAlbumTable.AddBoolField("isPublished", false, true).AddScriptComments("Whether this album is visible on the public site");
            galleryAlbumTable.AddSequenceField();
            galleryAlbumTable.AddControlFields();


            // -------------------------------------------------
            // GalleryImage — Individual images within an album
            // -------------------------------------------------
            Database.Table galleryImageTable = database.AddTable("GalleryImage");
            galleryImageTable.comment = "Individual images within a gallery album.";
            galleryImageTable.SetMinimumPermissionLevels(COMMUNITY_READER_PERMISSION_LEVEL, COMMUNITY_CONTENT_WRITER_PERMISSION_LEVEL);
            galleryImageTable.customWriteAccessRole = COMMUNITY_CONTENT_WRITER_ROLE;
            galleryImageTable.AddIdField();
            galleryImageTable.AddMultiTenantSupport();
            galleryImageTable.AddForeignKeyField(galleryAlbumTable, false).AddScriptComments("The album this image belongs to");
            galleryImageTable.AddString500Field("imageUrl", false).AddScriptComments("URL or relative path to the image file");
            galleryImageTable.AddString500Field("caption").AddScriptComments("Display caption for the image");
            galleryImageTable.AddString250Field("altText").AddScriptComments("Alt text for accessibility");
            galleryImageTable.AddSequenceField();
            galleryImageTable.AddControlFields();

            #endregion


            #region Documents

            // -------------------------------------------------
            // DocumentDownload — Downloadable files (PDFs, schedules, forms)
            // -------------------------------------------------
            Database.Table documentDownloadTable = database.AddTable("DocumentDownload");
            documentDownloadTable.comment = "Downloadable documents for public access (council minutes, schedules, forms, regulations). Replaces the PDF-as-image pattern.";
            documentDownloadTable.SetMinimumPermissionLevels(COMMUNITY_READER_PERMISSION_LEVEL, COMMUNITY_CONTENT_WRITER_PERMISSION_LEVEL);
            documentDownloadTable.customWriteAccessRole = COMMUNITY_CONTENT_WRITER_ROLE;
            documentDownloadTable.AddIdField();
            documentDownloadTable.AddMultiTenantSupport();
            documentDownloadTable.AddString250Field("title", false).AddScriptComments("Display title for the document");
            documentDownloadTable.AddTextField("description").AddScriptComments("Description of what this document contains");
            documentDownloadTable.AddString500Field("filePath", false).AddScriptComments("Server-relative path to the downloadable file");
            documentDownloadTable.AddString100Field("fileName", false).AddScriptComments("Original filename for download (e.g. 'recycling-schedule-2026.pdf')");
            documentDownloadTable.AddString100Field("mimeType").AddScriptComments("File MIME type");
            documentDownloadTable.AddLongField("fileSizeBytes", true).AddScriptComments("File size in bytes");
            documentDownloadTable.AddString100Field("categoryName").AddScriptComments("Grouping category (e.g. 'Council Minutes', 'Schedules', 'Forms', 'Regulations')");
            documentDownloadTable.AddDateTimeField("documentDate", true).AddScriptComments("Date associated with the document (e.g. meeting date for council minutes)");
            documentDownloadTable.AddBoolField("isPublished", false, true).AddScriptComments("Whether this document is visible for public download");
            documentDownloadTable.AddSequenceField();
            documentDownloadTable.AddControlFields();

            #endregion


            #region Contact

            // -------------------------------------------------
            // ContactSubmission — Contact form submissions
            // -------------------------------------------------
            Database.Table contactSubmissionTable = database.AddTable("ContactSubmission");
            contactSubmissionTable.comment = "Captures contact form submissions from public site visitors.";
            contactSubmissionTable.SetMinimumPermissionLevels(COMMUNITY_ADMIN_PERMISSION_LEVEL, COMMUNITY_READER_PERMISSION_LEVEL);
            contactSubmissionTable.customReadAccessRole = COMMUNITY_ADMIN_ROLE;
            contactSubmissionTable.AddIdField();
            contactSubmissionTable.AddMultiTenantSupport();
            contactSubmissionTable.AddString100Field("name", false).AddScriptComments("Name of the person submitting the form");
            contactSubmissionTable.AddString250Field("email", false).AddScriptComments("Email address for reply");
            contactSubmissionTable.AddString250Field("subject").AddScriptComments("Subject line of the message");
            contactSubmissionTable.AddTextField("message", false).AddScriptComments("Full message body");
            contactSubmissionTable.AddDateTimeField("submittedDate", false).AddScriptComments("When the form was submitted");
            contactSubmissionTable.AddBoolField("isRead", false, false).AddScriptComments("Whether an admin has read this submission");
            contactSubmissionTable.AddBoolField("isArchived", false, false).AddScriptComments("Whether this submission has been archived");
            contactSubmissionTable.AddString500Field("adminNotes").AddScriptComments("Internal notes from admin about the submission");

            contactSubmissionTable.AddControlFields();

            #endregion
        }
    }
}
