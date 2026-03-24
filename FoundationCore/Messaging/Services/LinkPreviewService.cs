using Foundation.Messaging.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// Link Preview Service - extracts URLs from messages and fetchs Open Graph metadata.
    /// 
    /// After a message is saved, this service is called asynchronously to:
    ///   1. Extract URLs from the message HTML
    ///   2. Fetch Open Graph / meta tag data for each URL
    ///   3. Store the preview records in the database
    ///   4. Return results so the caller can push them via SignalR
    /// 
    /// Includes a simple in-memory cache keyed on URL to avoid re-fetching identical URLs.
    /// 
    /// </summary>
    public class LinkPreviewService
    {
        private static readonly HttpClient _httpClient;
        private static readonly ConcurrentDictionary<string, LinkPreviewData> _urlCache = new ConcurrentDictionary<string, LinkPreviewData>();
        private static readonly TimeSpan _cacheTTL = TimeSpan.FromHours(4);

        private const int MAX_URLS_PER_MESSAGE = 5;
        private const int HTTP_TIMEOUT_SECONDS = 5;


        static LinkPreviewService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; CatalystBot/1.0; +https://compactica.com)");
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml");
        }


        #region DTOs

        public class LinkPreviewData
        {
            public string url { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public string imageUrl { get; set; }
            public string siteName { get; set; }
            public DateTime fetchedDateTime { get; set; }
        }

        public class LinkPreviewSummary
        {
            public int id { get; set; }
            public int conversationMessageId { get; set; }
            public string url { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public string imageUrl { get; set; }
            public string siteName { get; set; }
        }

        #endregion


        /// <summary>
        /// Extract URLs from message HTML, fetch Open Graph data, and save preview records.
        /// Returns the summaries for SignalR push.
        /// </summary>
        public async Task<List<LinkPreviewSummary>> CreatePreviewsForMessageAsync(int messageId, Guid tenantGuid, string messageHtml)
        {
            List<LinkPreviewSummary> results = new List<LinkPreviewSummary>();

            if (string.IsNullOrWhiteSpace(messageHtml))
            {
                return results;
            }

            List<string> urls = ExtractUrlsFromHtml(messageHtml);

            if (urls.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[LinkPreview] No URLs found in message {messageId}");
                return results;
            }

            System.Diagnostics.Debug.WriteLine($"[LinkPreview] Found {urls.Count} URL(s) in message {messageId}: {string.Join(", ", urls)}");

            // Limit URLs per message to prevent abuse
            if (urls.Count > MAX_URLS_PER_MESSAGE)
            {
                urls = urls.Take(MAX_URLS_PER_MESSAGE).ToList();
            }

            using (MessagingContext db = new MessagingContext())
            {
                foreach (string url in urls)
                {
                    try
                    {
                        LinkPreviewData previewData = await FetchPreviewAsync(url);

                        if (previewData == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[LinkPreview] FetchPreviewAsync returned null for {url}");
                            continue;
                        }

                        if (string.IsNullOrEmpty(previewData.title))
                        {
                            System.Diagnostics.Debug.WriteLine($"[LinkPreview] No title found for {url}, siteName={previewData.siteName}");
                            continue;   // skip URLs that don't return useful metadata
                        }

                        System.Diagnostics.Debug.WriteLine($"[LinkPreview] Got metadata for {url}: title={previewData.title}, siteName={previewData.siteName}");

                        ConversationMessageLinkPreview record = new ConversationMessageLinkPreview();
                        record.conversationMessageId = messageId;
                        record.tenantGuid = tenantGuid;
                        record.url = Truncate(url, 1000);
                        record.title = Truncate(previewData.title, 500);
                        record.description = Truncate(previewData.description, 1000);
                        record.imageUrl = Truncate(previewData.imageUrl, 1000);
                        record.siteName = Truncate(previewData.siteName, 250);
                        record.fetchedDateTime = DateTime.UtcNow;
                        record.versionNumber = 1;
                        record.objectGuid = Guid.NewGuid();
                        record.active = true;
                        record.deleted = false;

                        db.ConversationMessageLinkPreviews.Add(record);
                        await db.SaveChangesAsync();

                        results.Add(new LinkPreviewSummary
                        {
                            id = record.id,
                            conversationMessageId = messageId,
                            url = record.url,
                            title = record.title,
                            description = record.description,
                            imageUrl = record.imageUrl,
                            siteName = record.siteName
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LinkPreview] Error processing {url}: {ex.Message}");
                        continue;
                    }
                }
            }

            return results;
        }


        /// <summary>
        /// Get existing link previews for a set of message IDs (for batch loading in GetMessagesAsync).
        /// </summary>
        public async Task<Dictionary<int, List<LinkPreviewSummary>>> GetPreviewsForMessagesAsync(List<int> messageIds)
        {
            Dictionary<int, List<LinkPreviewSummary>> result = new Dictionary<int, List<LinkPreviewSummary>>();

            if (messageIds == null || messageIds.Count == 0)
            {
                return result;
            }

            using (MessagingContext db = new MessagingContext())
            {
                var previews = await (from lp in db.ConversationMessageLinkPreviews
                                        where messageIds.Contains(lp.conversationMessageId)
                                            && lp.active == true
                                            && lp.deleted == false
                                        select lp).ToListAsync();

                foreach (var preview in previews)
                {
                    if (!result.ContainsKey(preview.conversationMessageId))
                    {
                        result[preview.conversationMessageId] = new List<LinkPreviewSummary>();
                    }

                    result[preview.conversationMessageId].Add(new LinkPreviewSummary
                    {
                        id = preview.id,
                        conversationMessageId = preview.conversationMessageId,
                        url = preview.url,
                        title = preview.title,
                        description = preview.description,
                        imageUrl = preview.imageUrl,
                        siteName = preview.siteName
                    });
                }
            }


            return result;
        }


        /// <summary>
        /// Soft-delete a link preview (sender can dismiss unwanted previews).
        /// </summary>
        public async Task DismissPreviewAsync(int previewId, Guid tenantGuid)
        {
            using (MessagingContext db = new MessagingContext())
            {
                var preview = await db.ConversationMessageLinkPreviews
                    .FirstOrDefaultAsync(p => p.id == previewId && p.tenantGuid == tenantGuid && p.active == true && p.deleted == false);

                if (preview != null)
                {
                    preview.deleted = true;
                    await db.SaveChangesAsync();
                }
            }
        }


        // ─── URL Extraction ──────────────────────────────────────────────────────

        /// <summary>
        /// Extract http/https URLs from HTML content.
        /// Parses href attributes and plain-text URLs.
        /// </summary>
        public static List<string> ExtractUrlsFromHtml(string html)
        {
            HashSet<string> urls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(html))
            {
                return urls.ToList();
            }

            // Match href="..." attributes
            var hrefMatches = Regex.Matches(html, @"href=""(https?://[^""]+)""", RegexOptions.IgnoreCase);
            foreach (Match match in hrefMatches)
            {
                urls.Add(match.Groups[1].Value);
            }

            // Match plain-text URLs not inside tags
            string textContent = Regex.Replace(html, @"<[^>]+>", " ");
            var plainUrlMatches = Regex.Matches(textContent, @"(https?://[^\s<>""']+)", RegexOptions.IgnoreCase);
            foreach (Match match in plainUrlMatches)
            {
                urls.Add(match.Groups[1].Value);
            }

            return urls.ToList();
        }


        // ─── Open Graph Fetch ────────────────────────────────────────────────────

        /// <summary>
        /// Fetch Open Graph / meta tag data from a URL.
        /// Uses in-memory cache to avoid re-fetching the same URL.
        /// </summary>
        public async Task<LinkPreviewData> FetchPreviewAsync(string url)
        {
            // Check cache first
            if (_urlCache.TryGetValue(url, out LinkPreviewData cached))
            {
                if (DateTime.UtcNow - cached.fetchedDateTime < _cacheTTL)
                {
                    return cached;
                }
                _urlCache.TryRemove(url, out _);
            }

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                // Only parse HTML responses
                string contentType = response.Content.Headers.ContentType?.MediaType ?? "";
                if (!contentType.Contains("text/html") && !contentType.Contains("application/xhtml"))
                {
                    return null;
                }

                string html = await response.Content.ReadAsStringAsync();

                // Limit parsing to first 50KB to avoid large page issues
                if (html.Length > 50000)
                {
                    html = html.Substring(0, 50000);
                }

                LinkPreviewData data = ParseOpenGraphTags(html, url);
                data.fetchedDateTime = DateTime.UtcNow;

                // Cache the result
                _urlCache[url] = data;

                return data;
            }
            catch (TaskCanceledException)
            {
                return null;    // timeout
            }
            catch (HttpRequestException)
            {
                return null;    // network error
            }
        }


        // ─── HTML Parsing ────────────────────────────────────────────────────────

        private static LinkPreviewData ParseOpenGraphTags(string html, string url)
        {
            LinkPreviewData data = new LinkPreviewData();
            data.url = url;

            // og:title
            data.title = ExtractMetaContent(html, @"property=""og:title""")
                      ?? ExtractMetaContent(html, @"name=""og:title""")
                      ?? ExtractTitleTag(html);

            // og:description
            data.description = ExtractMetaContent(html, @"property=""og:description""")
                            ?? ExtractMetaContent(html, @"name=""description""");

            // og:image
            data.imageUrl = ExtractMetaContent(html, @"property=""og:image""");

            // og:site_name
            data.siteName = ExtractMetaContent(html, @"property=""og:site_name""");

            // Fallback site name from domain
            if (string.IsNullOrEmpty(data.siteName))
            {
                try
                {
                    Uri uri = new Uri(url);
                    data.siteName = uri.Host.Replace("www.", "");
                }
                catch { }
            }

            return data;
        }

        private static string ExtractMetaContent(string html, string attributePattern)
        {
            // Match <meta property="og:title" content="..."> or <meta content="..." property="og:title">
            string pattern1 = $@"<meta\s+{attributePattern}\s+content=""([^""]*)""\s*/?\s*>";
            string pattern2 = $@"<meta\s+content=""([^""]*)""\s+{attributePattern}\s*/?\s*>";

            Match match = Regex.Match(html, pattern1, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                return System.Net.WebUtility.HtmlDecode(match.Groups[1].Value);
            }

            match = Regex.Match(html, pattern2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                return System.Net.WebUtility.HtmlDecode(match.Groups[1].Value);
            }

            return null;
        }

        private static string ExtractTitleTag(string html)
        {
            Match match = Regex.Match(html, @"<title[^>]*>([^<]+)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                return System.Net.WebUtility.HtmlDecode(match.Groups[1].Value.Trim());
            }
            return null;
        }


        // ─── Helpers ─────────────────────────────────────────────────────────────

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
