using Foundation.Community.Database;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Foundation.Community.CodeGeneration
{
    /// <summary>
    /// 
    /// Scrapes content from the PHMC Weebly site (http://www.pettyharbourmaddoxcove.ca/)
    /// and seeds it into the Community database for the PHMC tenant.
    /// 
    /// This is idempotent — it checks for existing records by slug/key + tenantGuid
    /// before inserting, so it can be re-run safely.
    /// 
    /// Images referenced in page HTML are downloaded and stored as MediaAsset + MediaContent
    /// records, and the HTML src attributes are rewritten to point to the local media endpoint.
    /// 
    /// </summary>
    public class PHMCScraper
    {
        private static readonly Guid PHMCTenantGuid = Guid.Parse("d58f56c6-e3fb-4d3b-80b3-7053c66491e3");
        private const string BASE_URL = "http://www.pettyharbourmaddoxcove.ca";
        private const string MEDIA_ENDPOINT_TEMPLATE = "/api/PublicContent/Media/{0}";

        private readonly HttpClient _httpClient;

        //
        // Track downloaded images to avoid duplicates (sourceUrl → objectGuid)
        //
        private readonly Dictionary<string, Guid> _downloadedImages = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        //
        // Pages to scrape — tuple of (URL path, slug, title, sortOrder)
        //
        private static readonly List<(string path, string slug, string title, int sortOrder)> PageDefinitions = new List<(string, string, string, int)>
        {
            // Visiting section
            ("", "home", "Home", 1),
            ("/interview-annie-lee.html", "interview-annie-lee", "Interview: Annie Lee", 10),
            ("/interview-robert-howlett.html", "interview-robert-howlett", "Interview: Robert Howlett", 11),
            ("/interview-robert-chafe.html", "interview-robert-chafe", "Interview: Robert Chafe", 12),
            ("/interview-richard-clements.html", "interview-richard-clements", "Interview: Richard Clements", 13),
            ("/attractions-and-accomodations.html", "attractions-and-accommodations", "Attractions and Accommodations", 20),
            ("/official-flag.html", "official-flag", "Official Flag", 21),
            ("/photo-gallery.html", "photo-gallery", "Photo Gallery", 22),
            ("/guestbook-archive.html", "guestbook-archive", "Guestbook Archive", 23),

            // Council section
            ("/development--business-applications.html", "development-business-applications", "Development & Business Applications", 30),
            ("/town-council.html", "town-council", "Town Council", 31),
            ("/town-council-minutes.html", "town-council-minutes", "Town Council Minutes", 32),
            ("/agendas.html", "agendas", "Agendas", 33),
            ("/schedules.html", "schedules", "Schedules", 34),
            ("/budget-and-taxes.html", "budget-and-taxes", "Budget and Taxes", 35),
            ("/regulations--town-plan.html", "regulations-town-plan", "Regulations / Town Plan", 36),
            ("/bizpal.html", "bizpal", "BizPal", 37),
            ("/faq.html", "faq", "FAQ", 38),
            ("/links.html", "links", "Links", 39),
            ("/recycling.html", "recycling", "Recycling", 40),
        };


        public PHMCScraper()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CommunityTools-PHMCScraper/1.0");
        }


        //
        // Main entry point
        //

        public async Task ScrapeAndSeedAsync()
        {
            Console.WriteLine();
            Console.WriteLine("=== PHMC Website Scraper ===");
            Console.WriteLine($"Source: {BASE_URL}");
            Console.WriteLine($"Tenant GUID: {PHMCTenantGuid}");
            Console.WriteLine();

            using (CommunityContext context = new CommunityContext())
            {
                //
                // Phase 1: Scrape pages → Page entities
                //
                Console.WriteLine("--- Phase 1: Scraping Pages ---");
                await ScrapePages(context);

                //
                // Phase 2: Build navigation menus
                //
                Console.WriteLine();
                Console.WriteLine("--- Phase 2: Building Navigation ---");
                BuildNavigation(context);

                //
                // Phase 3: Seed site settings
                //
                Console.WriteLine();
                Console.WriteLine("--- Phase 3: Seeding Site Settings ---");
                SeedSiteSettings(context);

                //
                // Phase 4: Catalogue document downloads (PDFs)
                //
                Console.WriteLine();
                Console.WriteLine("--- Phase 4: Cataloguing Documents ---");
                await CatalogueDocuments(context);

                Console.WriteLine();
                Console.WriteLine("=== Scraping Complete ===");
                Console.WriteLine($"  Images downloaded: {_downloadedImages.Count}");
            }
        }


        // ─── Phase 1: Page Scraping ─────────────────────────────────────

        private async Task ScrapePages(CommunityContext context)
        {
            int scraped = 0;
            int skipped = 0;

            foreach (var pageDef in PageDefinitions)
            {
                //
                // Check if page already exists
                //
                bool exists = context.Pages.Any(p => p.slug == pageDef.slug && p.tenantGuid == PHMCTenantGuid);
                if (exists)
                {
                    Console.WriteLine($"  SKIP (exists): {pageDef.title}");
                    skipped++;
                    continue;
                }

                //
                // Fetch and parse the page
                //
                string url = BASE_URL + pageDef.path;
                Console.Write($"  Scraping: {pageDef.title} ... ");

                try
                {
                    string html = await FetchPageAsync(url);
                    string bodyContent = ExtractMainContent(html);

                    //
                    // Download images and rewrite URLs
                    //
                    bodyContent = await LocalizeImages(bodyContent, context);

                    //
                    // Create the Page entity
                    //
                    Page page = new Page
                    {
                        tenantGuid = PHMCTenantGuid,
                        title = pageDef.title,
                        slug = pageDef.slug,
                        body = bodyContent,
                        metaDescription = $"{pageDef.title} - Town of Petty Harbour Maddox Cove",
                        isPublished = true,
                        publishedDate = DateTime.UtcNow,
                        sortOrder = pageDef.sortOrder,
                        versionNumber = 1,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };

                    context.Pages.Add(page);
                    context.SaveChanges();

                    Console.WriteLine("OK");
                    scraped++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FAILED: {ex.Message}");
                }

                //
                // Polite delay between requests
                //
                await Task.Delay(500);
            }

            Console.WriteLine($"  Pages scraped: {scraped}, skipped: {skipped}");
        }


        // ─── Phase 2: Navigation ────────────────────────────────────────

        private void BuildNavigation(CommunityContext context)
        {
            //
            // Use the seeded "Main Navigation" menu, or create if not found
            //
            Menu headerMenu = context.Menus.FirstOrDefault(m => m.location == "header" && m.tenantGuid == PHMCTenantGuid);

            if (headerMenu == null)
            {
                headerMenu = new Menu
                {
                    tenantGuid = PHMCTenantGuid,
                    name = "Main Navigation",
                    location = "header",
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };
                context.Menus.Add(headerMenu);
                context.SaveChanges();
                Console.WriteLine("  Created 'Main Navigation' menu");
            }

            //
            // Check if menu items already exist
            //
            bool hasItems = context.MenuItems.Any(mi => mi.menuId == headerMenu.id && mi.tenantGuid == PHMCTenantGuid);
            if (hasItems)
            {
                Console.WriteLine("  SKIP: Menu items already exist");
                return;
            }

            //
            // Build the nav tree:
            //   Home
            //   Visiting >
            //     History Interviews (sub-items)
            //     Attractions & Accommodations
            //     Official Flag
            //     Photo Gallery
            //   Council >
            //     Town Council
            //     Minutes
            //     Agendas
            //     Schedules
            //     Budget & Taxes
            //     Regulations
            //     Recycling
            //     FAQ
            //     Links
            //   Contact
            //

            int seq = 1;

            // Home
            AddMenuItem(context, headerMenu.id, "Home", "home", null, seq++);

            // Visiting
            MenuItem visitingItem = AddMenuItem(context, headerMenu.id, "Visiting", null, null, seq++, isParent: true);
            int subSeq = 1;
            AddMenuItem(context, headerMenu.id, "History: Annie Lee", "interview-annie-lee", visitingItem.id, subSeq++);
            AddMenuItem(context, headerMenu.id, "History: Robert Howlett", "interview-robert-howlett", visitingItem.id, subSeq++);
            AddMenuItem(context, headerMenu.id, "History: Robert Chafe", "interview-robert-chafe", visitingItem.id, subSeq++);
            AddMenuItem(context, headerMenu.id, "History: Richard Clements", "interview-richard-clements", visitingItem.id, subSeq++);
            AddMenuItem(context, headerMenu.id, "Attractions & Accommodations", "attractions-and-accommodations", visitingItem.id, subSeq++);
            AddMenuItem(context, headerMenu.id, "Official Flag", "official-flag", visitingItem.id, subSeq++);
            AddMenuItem(context, headerMenu.id, "Photo Gallery", "photo-gallery", visitingItem.id, subSeq++);

            // Council
            MenuItem councilItem = AddMenuItem(context, headerMenu.id, "Council", null, null, seq++, isParent: true);
            subSeq = 1;
            AddMenuItem(context, headerMenu.id, "Town Council", "town-council", councilItem.id, subSeq++);
            AddMenuItem(context, headerMenu.id, "Council Minutes", "town-council-minutes", councilItem.id, subSeq++);
            AddMenuItem(context, headerMenu.id, "Agendas", "agendas", councilItem.id, subSeq++);
            AddMenuItem(context, headerMenu.id, "Schedules", "schedules", councilItem.id, subSeq++);
            AddMenuItem(context, headerMenu.id, "Budget & Taxes", "budget-and-taxes", councilItem.id, subSeq++);
            AddMenuItem(context, headerMenu.id, "Regulations / Town Plan", "regulations-town-plan", councilItem.id, subSeq++);
            AddMenuItem(context, headerMenu.id, "Dev & Business Apps", "development-business-applications", councilItem.id, subSeq++);
            AddMenuItem(context, headerMenu.id, "Recycling", "recycling", councilItem.id, subSeq++);
            AddMenuItem(context, headerMenu.id, "FAQ", "faq", councilItem.id, subSeq++);
            AddMenuItem(context, headerMenu.id, "Links", "links", councilItem.id, subSeq++);

            // Contact (external link — no slug yet)
            AddMenuItemWithUrl(context, headerMenu.id, "Contact", null, null, seq++);

            context.SaveChanges();

            Console.WriteLine($"  Created {context.ChangeTracker.Entries<MenuItem>().Count()} menu items");
        }


        private MenuItem AddMenuItem(CommunityContext context, int menuId, string label, string pageSlug, int? parentId, int sequence, bool isParent = false)
        {
            //
            // Look up page by slug to link it
            //
            int? pageId = null;
            if (!string.IsNullOrEmpty(pageSlug))
            {
                pageId = context.Pages.Where(p => p.slug == pageSlug && p.tenantGuid == PHMCTenantGuid).Select(p => (int?)p.id).FirstOrDefault();
            }

            MenuItem item = new MenuItem
            {
                tenantGuid = PHMCTenantGuid,
                menuId = menuId,
                label = label,
                pageId = pageId,
                parentMenuItemId = parentId,
                openInNewTab = false,
                sequence = sequence,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            context.MenuItems.Add(item);
            context.SaveChanges();

            return item;
        }


        private MenuItem AddMenuItemWithUrl(CommunityContext context, int menuId, string label, string url, int? parentId, int sequence)
        {
            MenuItem item = new MenuItem
            {
                tenantGuid = PHMCTenantGuid,
                menuId = menuId,
                label = label,
                url = url,
                parentMenuItemId = parentId,
                openInNewTab = false,
                sequence = sequence,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            context.MenuItems.Add(item);
            context.SaveChanges();

            return item;
        }


        // ─── Phase 3: Site Settings ─────────────────────────────────────

        private void SeedSiteSettings(CommunityContext context)
        {
            var settings = new Dictionary<string, (string value, string description, string group)>
            {
                ["siteName"] = ("Town of Petty Harbour Maddox Cove", "The name of the site displayed in the header and browser tab", "General"),
                ["tagline"] = ("A picturesque town on the eastern shore of the Avalon Peninsula", "Site tagline displayed below the site name", "General"),
                ["contactPhone"] = ("709 368 3959", "Primary contact phone number", "General"),
                ["contactEmail"] = ("info@pettyharbourmaddoxcove.ca", "Primary contact email address", "General"),
                ["footerText"] = ("© 2026 Town of Petty Harbour Maddox Cove. All rights reserved.", "Copyright text displayed in the site footer", "General"),
                ["heroTitle"] = ("Welcome to Petty Harbour Maddox Cove", "Hero section title on the home page", "HomePage"),
                ["heroSubtitle"] = ("A picturesque town of approximately 950 people located on the eastern shore of the Avalon Peninsula in the province of Newfoundland and Labrador.", "Hero section subtitle on the home page", "HomePage"),
            };

            int updated = 0;
            int created = 0;

            foreach (var kvp in settings)
            {
                SiteSetting existing = context.SiteSettings.FirstOrDefault(s => s.settingKey == kvp.Key && s.tenantGuid == PHMCTenantGuid);

                if (existing != null)
                {
                    //
                    // Update the value if it's still the default placeholder
                    //
                    if (string.IsNullOrWhiteSpace(existing.settingValue) || existing.settingValue == "Community" || existing.settingValue == "Welcome" || existing.settingValue == "Welcome to our community")
                    {
                        existing.settingValue = kvp.Value.value;
                        existing.description = kvp.Value.description;
                        context.SaveChanges();
                        updated++;
                        Console.WriteLine($"  UPDATED: {kvp.Key} = {kvp.Value.value}");
                    }
                    else
                    {
                        Console.WriteLine($"  SKIP (has value): {kvp.Key}");
                    }
                }
                else
                {
                    SiteSetting newSetting = new SiteSetting
                    {
                        tenantGuid = PHMCTenantGuid,
                        settingKey = kvp.Key,
                        settingValue = kvp.Value.value,
                        description = kvp.Value.description,
                        settingGroup = kvp.Value.group,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };
                    context.SiteSettings.Add(newSetting);
                    context.SaveChanges();
                    created++;
                    Console.WriteLine($"  CREATED: {kvp.Key} = {kvp.Value.value}");
                }
            }

            Console.WriteLine($"  Settings updated: {updated}, created: {created}");
        }


        // ─── Phase 4: Document Cataloguing ──────────────────────────────

        private async Task CatalogueDocuments(CommunityContext context)
        {
            //
            // Known PDF documents on the PHMC site
            //
            var documents = new List<(string url, string title, string category, string fileName)>
            {
                ("/uploads/4/8/9/5/48959877/put_waste_in_its_place.pdf", "Put Waste in Its Place", "Recycling", "put_waste_in_its_place.pdf"),
                ("/uploads/4/8/9/5/48959877/how_to_reduce_reuse_recycle.pdf", "How to Reduce, Reuse & Recycle", "Recycling", "how_to_reduce_reuse_recycle.pdf"),
                ("/uploads/4/8/9/5/48959877/how_to_recycle.pdf", "How to Recycle (Step by Step)", "Recycling", "how_to_recycle.pdf"),
                ("/uploads/4/8/9/5/48959877/what_can_be_recycled.pdf", "What Can Be Recycled", "Recycling", "what_can_be_recycled.pdf"),
                ("/uploads/4/8/9/5/48959877/recycling_schedule_2022.pdf", "Recycling Schedule", "Recycling", "recycling_schedule_2022.pdf"),
            };

            int created = 0;
            int skipped = 0;

            foreach (var doc in documents)
            {
                string slug = doc.fileName.Replace(".pdf", "");

                bool exists = context.DocumentDownloads.Any(d => d.fileName == doc.fileName && d.tenantGuid == PHMCTenantGuid);
                if (exists)
                {
                    Console.WriteLine($"  SKIP (exists): {doc.title}");
                    skipped++;
                    continue;
                }

                //
                // Download the PDF and store as MediaAsset + MediaContent
                //
                try
                {
                    string fullUrl = BASE_URL + doc.url;
                    Console.Write($"  Downloading: {doc.title} ... ");

                    byte[] pdfBytes = await _httpClient.GetByteArrayAsync(fullUrl);

                    //
                    // Create MediaAsset for the PDF
                    //
                    Guid pdfGuid = Guid.NewGuid();
                    MediaAsset asset = new MediaAsset
                    {
                        tenantGuid = PHMCTenantGuid,
                        fileName = doc.fileName,
                        filePath = string.Format(MEDIA_ENDPOINT_TEMPLATE, pdfGuid),
                        mimeType = "application/pdf",
                        altText = doc.title,
                        fileSizeBytes = pdfBytes.Length,
                        objectGuid = pdfGuid,
                        active = true,
                        deleted = false
                    };
                    context.MediaAssets.Add(asset);
                    context.SaveChanges();

                    //
                    // Create MediaContent with the binary data
                    //
                    MediaContent content = new MediaContent
                    {
                        tenantGuid = PHMCTenantGuid,
                        mediaAssetId = asset.id,
                        fileData = pdfBytes,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };
                    context.MediaContents.Add(content);
                    context.SaveChanges();

                    //
                    // Create DocumentDownload entry
                    //
                    DocumentDownload download = new DocumentDownload
                    {
                        tenantGuid = PHMCTenantGuid,
                        title = doc.title,
                        filePath = asset.filePath,
                        fileName = doc.fileName,
                        mimeType = "application/pdf",
                        fileSizeBytes = pdfBytes.Length,
                        categoryName = doc.category,
                        isPublished = true,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };
                    context.DocumentDownloads.Add(download);
                    context.SaveChanges();

                    Console.WriteLine($"OK ({pdfBytes.Length:N0} bytes)");
                    created++;

                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FAILED: {ex.Message}");
                }
            }

            Console.WriteLine($"  Documents created: {created}, skipped: {skipped}");
        }


        // ─── HTML Parsing Helpers ───────────────────────────────────────

        private async Task<string> FetchPageAsync(string url)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }


        /// <summary>
        /// Extracts the main content area from a Weebly page, stripping navigation,
        /// header, footer, and other site chrome.
        /// </summary>
        private string ExtractMainContent(string rawHtml)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(rawHtml);

            //
            // Weebly uses a container div for the main content.
            // Try several known selectors in order of specificity.
            //
            string[] contentSelectors = new string[]
            {
                "//div[contains(@class, 'wsite-section-wrap')]",
                "//div[@id='wsite-content']",
                "//div[contains(@class, 'container')]//div[contains(@class, 'wsite-section')]",
                "//div[contains(@class, 'main-wrap')]",
                "//div[@role='main']",
                "//article",
                "//main",
            };

            HtmlNode contentNode = null;

            foreach (string selector in contentSelectors)
            {
                HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(selector);
                if (nodes != null && nodes.Count > 0)
                {
                    //
                    // If multiple sections found, combine them
                    //
                    if (nodes.Count > 1)
                    {
                        string combined = string.Join("\n", nodes.Select(n => n.OuterHtml));
                        return CleanHtml(combined);
                    }
                    contentNode = nodes[0];
                    break;
                }
            }

            if (contentNode == null)
            {
                //
                // Fallback: get the body content and strip nav/footer
                //
                HtmlNode body = doc.DocumentNode.SelectSingleNode("//body");
                if (body != null)
                {
                    //
                    // Remove known non-content elements
                    //
                    RemoveNodes(body, "//nav");
                    RemoveNodes(body, "//header");
                    RemoveNodes(body, "//footer");
                    RemoveNodes(body, "//script");
                    RemoveNodes(body, "//style");
                    RemoveNodes(body, "//div[contains(@class, 'header')]");
                    RemoveNodes(body, "//div[contains(@class, 'footer')]");
                    RemoveNodes(body, "//div[contains(@class, 'nav')]");
                    RemoveNodes(body, "//div[contains(@class, 'hamburger')]");
                    RemoveNodes(body, "//div[contains(@class, 'birdseye')]");

                    return CleanHtml(body.InnerHtml);
                }
            }

            return CleanHtml(contentNode?.OuterHtml ?? rawHtml);
        }


        private void RemoveNodes(HtmlNode parent, string xpath)
        {
            HtmlNodeCollection nodes = parent.SelectNodes(xpath);
            if (nodes != null)
            {
                foreach (HtmlNode node in nodes.ToList())
                {
                    node.Remove();
                }
            }
        }


        private string CleanHtml(string html)
        {
            //
            // Remove inline Weebly tracking scripts and style blocks
            //
            html = Regex.Replace(html, @"<script[^>]*>[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<style[^>]*>[\s\S]*?</style>", "", RegexOptions.IgnoreCase);

            //
            // Remove HTML comments
            //
            html = Regex.Replace(html, @"<!--[\s\S]*?-->", "");

            //
            // Remove Weebly-specific attributes
            //
            html = Regex.Replace(html, @"\s*data-wsite-[a-z-]+=""[^""]*""", "", RegexOptions.IgnoreCase);

            //
            // Collapse excessive whitespace
            //
            html = Regex.Replace(html, @"\n{3,}", "\n\n");

            return html.Trim();
        }


        // ─── Image Localization ─────────────────────────────────────────

        /// <summary>
        /// Finds all image references in the HTML, downloads them, stores as
        /// MediaAsset + MediaContent, and rewrites src URLs to the local media endpoint.
        /// </summary>
        private async Task<string> LocalizeImages(string html, CommunityContext context)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNodeCollection imgNodes = doc.DocumentNode.SelectNodes("//img[@src]");
            if (imgNodes == null)
            {
                return html;
            }

            foreach (HtmlNode img in imgNodes)
            {
                string src = img.GetAttributeValue("src", "");
                if (string.IsNullOrWhiteSpace(src))
                {
                    continue;
                }

                //
                // Normalize the URL
                //
                string absoluteUrl = NormalizeImageUrl(src);
                if (string.IsNullOrEmpty(absoluteUrl))
                {
                    continue;
                }

                //
                // Skip data URIs and already-localized URLs
                //
                if (absoluteUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                    absoluteUrl.Contains("/api/PublicContent/Media/"))
                {
                    continue;
                }

                try
                {
                    Guid mediaGuid;

                    //
                    // Check if we've already downloaded this image
                    //
                    if (_downloadedImages.TryGetValue(absoluteUrl, out mediaGuid))
                    {
                        img.SetAttributeValue("src", string.Format(MEDIA_ENDPOINT_TEMPLATE, mediaGuid));
                        continue;
                    }

                    //
                    // Download the image
                    //
                    byte[] imageBytes = await _httpClient.GetByteArrayAsync(absoluteUrl);
                    if (imageBytes == null || imageBytes.Length == 0)
                    {
                        continue;
                    }

                    //
                    // Determine mime type and filename from URL
                    //
                    string fileName = ExtractFileName(absoluteUrl);
                    string mimeType = GuessMimeType(fileName);
                    string altText = img.GetAttributeValue("alt", fileName);

                    mediaGuid = Guid.NewGuid();

                    //
                    // Create MediaAsset
                    //
                    MediaAsset asset = new MediaAsset
                    {
                        tenantGuid = PHMCTenantGuid,
                        fileName = fileName,
                        filePath = string.Format(MEDIA_ENDPOINT_TEMPLATE, mediaGuid),
                        mimeType = mimeType,
                        altText = altText,
                        fileSizeBytes = imageBytes.Length,
                        objectGuid = mediaGuid,
                        active = true,
                        deleted = false
                    };
                    context.MediaAssets.Add(asset);
                    context.SaveChanges();

                    //
                    // Create MediaContent with the binary data
                    //
                    MediaContent content = new MediaContent
                    {
                        tenantGuid = PHMCTenantGuid,
                        mediaAssetId = asset.id,
                        fileData = imageBytes,
                        objectGuid = Guid.NewGuid(),
                        active = true,
                        deleted = false
                    };
                    context.MediaContents.Add(content);
                    context.SaveChanges();

                    //
                    // Track and rewrite
                    //
                    _downloadedImages[absoluteUrl] = mediaGuid;
                    img.SetAttributeValue("src", string.Format(MEDIA_ENDPOINT_TEMPLATE, mediaGuid));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"    [IMG WARN] Failed to download {absoluteUrl}: {ex.Message}");
                }
            }

            return doc.DocumentNode.OuterHtml;
        }


        private string NormalizeImageUrl(string src)
        {
            //
            // Protocol-relative URLs
            //
            if (src.StartsWith("//"))
            {
                return "http:" + src;
            }

            //
            // Relative URLs
            //
            if (src.StartsWith("/"))
            {
                return BASE_URL + src;
            }

            //
            // Already absolute
            //
            if (src.StartsWith("http://") || src.StartsWith("https://"))
            {
                return src;
            }

            //
            // Relative to current page (best effort)
            //
            return BASE_URL + "/" + src;
        }


        private string ExtractFileName(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                string path = uri.AbsolutePath;
                string fileName = Path.GetFileName(path);

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return "image_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                }

                return fileName;
            }
            catch
            {
                return "image_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            }
        }


        private string GuessMimeType(string fileName)
        {
            string ext = Path.GetExtension(fileName)?.ToLowerInvariant() ?? "";
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                ".ico" => "image/x-icon",
                ".bmp" => "image/bmp",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream",
            };
        }
    }
}
