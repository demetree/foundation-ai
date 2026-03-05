using System;
using System.Collections.Generic;
using System.Text;

namespace Foundation.Imaging
{
    /// <summary>
    /// Generic HTML document builder implementing IDocumentBuilder.
    ///
    /// Produces a self-contained, print-ready HTML document with:
    ///   - Themed cover/closing pages with gradient backgrounds
    ///   - Content pages with images, items lists, and thumbnails
    ///   - Section grouping with callout styling
    ///   - Summary pages with grid layout
    ///   - Smart pagination (small pages share side-by-side rows)
    ///   - Page numbering via deferred placeholder replacement
    ///   - All images embedded as base64 (zero external dependencies)
    ///
    /// Theme colours are pulled from DocumentTheme; defaults to a
    /// professional dark navy/blue palette if no theme is supplied.
    /// </summary>
    public class HtmlDocumentBuilder : IDocumentBuilder
    {
        private StringBuilder _sb;
        private string _title;
        private DocumentOptions _options;
        private DocumentTheme _theme;
        private string _pageSize;
        private int _pageCount;

        // Section state
        private string _currentSectionName;
        private bool _inSection;

        // Multi-content-page state (small pages share a row)
        private bool _pageOpen;
        private int _pagesOnCurrentRow;
        private const int MaxPagesPerRow = 2;
        private const int SmallItemThreshold = 3;

        // Summary pagination
        private const int SummaryItemsPerPage = 15;


        // ═══════════════════════════════════════════════════════════
        //  IDocumentBuilder implementation
        // ═══════════════════════════════════════════════════════════

        public void BeginDocument(string title, DocumentOptions options)
        {
            _title = title ?? "";
            _options = options ?? new DocumentOptions();
            _theme = _options.Theme ?? new DocumentTheme();
            _pageSize = (_options.PageSize ?? "a4").ToLowerInvariant() == "letter"
                ? "letter" : "A4";
            _pageCount = 0;
            _pageOpen = false;
            _pagesOnCurrentRow = 0;
            _inSection = false;

            _sb = new StringBuilder();
            _sb.AppendLine("<!DOCTYPE html>");
            _sb.AppendLine("<html lang=\"en\">");
            _sb.AppendLine("<head>");
            _sb.AppendLine("<meta charset=\"UTF-8\">");
            _sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            _sb.AppendFormat("<title>{0}</title>\n",
                System.Net.WebUtility.HtmlEncode(_title));
            _sb.AppendLine("<style>");
            _sb.AppendLine(BuildCss());
            _sb.AppendLine("</style>");
            _sb.AppendLine("</head>");
            _sb.AppendLine("<body>");
        }


        public void AddCoverPage(byte[] titleImage)
        {
            CloseCurrentRow();
            _pageCount++;

            _sb.AppendLine("<div class=\"page cover-page\">");
            _sb.AppendLine("<div class=\"cover-content\">");
            _sb.AppendFormat("<h1 class=\"cover-title\">{0}</h1>\n",
                System.Net.WebUtility.HtmlEncode(_title));

            if (titleImage != null && titleImage.Length > 0)
            {
                string b64 = Convert.ToBase64String(titleImage);
                _sb.AppendFormat("<img class=\"cover-image\" src=\"data:image/png;base64,{0}\" alt=\"Cover\" />\n", b64);
            }

            _sb.AppendLine("</div>");
            _sb.AppendFormat("<div class=\"page-number page-number-light\">Page {0} of {{TOTAL_PAGES}}</div>\n", _pageCount);
            _sb.AppendLine("</div>");
        }


        public void BeginSection(string sectionName)
        {
            CloseCurrentRow();
            _currentSectionName = sectionName;
            _inSection = true;

            _pageCount++;
            _sb.AppendLine("<div class=\"page content-page\">");
            _sb.AppendLine("<div class=\"section-callout\">");
            _sb.AppendFormat("<div class=\"section-header\">{0}</div>\n",
                System.Net.WebUtility.HtmlEncode(sectionName));
        }


        public void AddContentPage(string heading, byte[] contentImage,
            List<ContentItem> items, Dictionary<string, byte[]> thumbnails)
        {
            if (_inSection)
            {
                AddSectionContentPage(heading, contentImage, items, thumbnails);
            }
            else
            {
                AddRootContentPage(heading, contentImage, items, thumbnails);
            }
        }


        public void EndSection()
        {
            if (_inSection)
            {
                _sb.AppendLine("</div>"); // close .section-callout
                _sb.AppendFormat("<div class=\"page-number\">Page {0} of {{TOTAL_PAGES}}</div>\n", _pageCount);
                _sb.AppendLine("</div>"); // close .page
                _inSection = false;
                _currentSectionName = null;
            }
        }


        public void AddSummaryPage(string heading, List<ContentItem> items,
            Dictionary<string, byte[]> thumbnails)
        {
            if (items == null || items.Count == 0) return;
            CloseCurrentRow();

            int totalItems = items.Count;
            int totalQty = 0;
            for (int i = 0; i < items.Count; i++) totalQty += items[i].Quantity;

            int pageCount = (int)Math.Ceiling((double)totalItems / SummaryItemsPerPage);

            for (int pg = 0; pg < pageCount; pg++)
            {
                _pageCount++;
                _sb.AppendLine("<div class=\"page summary-page\">");
                _sb.AppendLine("<div class=\"summary-header\">");

                if (pageCount > 1)
                    _sb.AppendFormat("<h2 class=\"summary-title\">{0} ({1}/{2})</h2>\n",
                        Enc(heading), pg + 1, pageCount);
                else
                    _sb.AppendFormat("<h2 class=\"summary-title\">{0}</h2>\n", Enc(heading));

                _sb.AppendFormat("<div class=\"summary-meta\">{0} items · {1} total</div>\n",
                    totalItems, totalQty);
                _sb.AppendLine("</div>");

                _sb.AppendLine("<div class=\"summary-grid\">");

                int start = pg * SummaryItemsPerPage;
                int end = Math.Min(start + SummaryItemsPerPage, totalItems);

                for (int idx = start; idx < end; idx++)
                {
                    var item = items[idx];
                    _sb.AppendLine("<div class=\"summary-item\">");

                    // Thumbnail
                    byte[] thumbPng = null;
                    if (thumbnails != null && item.Key != null)
                        thumbnails.TryGetValue(item.Key, out thumbPng);

                    if (thumbPng != null && thumbPng.Length > 0)
                    {
                        string b64 = Convert.ToBase64String(thumbPng);
                        _sb.AppendFormat("<img class=\"summary-thumb\" src=\"data:image/png;base64,{0}\" alt=\"{1}\" />\n",
                            b64, Enc(item.Label ?? ""));
                    }
                    else
                    {
                        _sb.AppendLine("<div class=\"summary-thumb summary-thumb-placeholder\"></div>");
                    }

                    // Quantity badge
                    _sb.AppendFormat("<div class=\"summary-qty\">{0}×</div>\n", item.Quantity);

                    // Item info
                    _sb.AppendLine("<div class=\"summary-info\">");
                    _sb.AppendFormat("<div class=\"summary-label\">{0}</div>\n", Enc(item.Label ?? ""));

                    // Colour swatch + subtitle
                    _sb.AppendLine("<div class=\"summary-colour-row\">");
                    if (!string.IsNullOrEmpty(item.ColourHex))
                    {
                        _sb.AppendFormat("<span class=\"colour-swatch\" style=\"background:{0}\"></span>\n",
                            item.ColourHex);
                    }
                    _sb.AppendFormat("<span class=\"summary-subtitle\">{0}</span>\n",
                        Enc(item.Subtitle ?? ""));
                    _sb.AppendLine("</div>");

                    _sb.AppendLine("</div>"); // summary-info
                    _sb.AppendLine("</div>"); // summary-item
                }

                _sb.AppendLine("</div>"); // summary-grid
                _sb.AppendFormat("<div class=\"page-number\">Page {0} of {{TOTAL_PAGES}}</div>\n", _pageCount);
                _sb.AppendLine("</div>"); // summary-page
            }
        }


        public void AddClosingPage(byte[] closingImage)
        {
            CloseCurrentRow();
            _pageCount++;

            _sb.AppendLine("<div class=\"page closing-page\">");
            _sb.AppendLine("<div class=\"closing-content\">");
            _sb.AppendFormat("<h1 class=\"closing-title\">{0}</h1>\n", Enc(_title));

            if (closingImage != null && closingImage.Length > 0)
            {
                string b64 = Convert.ToBase64String(closingImage);
                _sb.AppendFormat("<img class=\"closing-image\" src=\"data:image/png;base64,{0}\" alt=\"Closing\" />\n", b64);
            }

            _sb.AppendLine("</div>");
            _sb.AppendFormat("<div class=\"page-number page-number-light\">Page {0} of {{TOTAL_PAGES}}</div>\n", _pageCount);
            _sb.AppendLine("</div>");
        }


        public DocumentResult Build()
        {
            CloseCurrentRow();

            _sb.Replace("{TOTAL_PAGES}", _pageCount.ToString());
            _sb.AppendLine("</body>");
            _sb.AppendLine("</html>");

            return new DocumentResult
            {
                Html = _sb.ToString(),
                ContentType = "text/html"
            };
        }


        // ═══════════════════════════════════════════════════════════
        //  Private helpers
        // ═══════════════════════════════════════════════════════════

        private void AddRootContentPage(string heading, byte[] contentImage,
            List<ContentItem> items, Dictionary<string, byte[]> thumbnails)
        {
            bool small = items == null || items.Count <= SmallItemThreshold;

            if (small && _pageOpen && _pagesOnCurrentRow < MaxPagesPerRow)
            {
                // Add to current row
                AddContentCell(heading, contentImage, items, thumbnails);
                _pagesOnCurrentRow++;
            }
            else
            {
                CloseCurrentRow();

                if (small)
                {
                    // Start a new multi-content row
                    _pageCount++;
                    _sb.AppendLine("<div class=\"page content-page\">");
                    _sb.AppendLine("<div class=\"multi-content-row\">");
                    AddContentCell(heading, contentImage, items, thumbnails);
                    _pageOpen = true;
                    _pagesOnCurrentRow = 1;
                }
                else
                {
                    // Full page
                    _pageCount++;
                    _sb.AppendLine("<div class=\"page content-page\">");

                    _sb.AppendFormat("<div class=\"content-header\"><span class=\"content-heading\">{0}</span></div>\n",
                        Enc(heading));

                    if (contentImage != null && contentImage.Length > 0)
                    {
                        string b64 = Convert.ToBase64String(contentImage);
                        _sb.AppendFormat("<div class=\"content-image\"><img src=\"data:image/png;base64,{0}\" alt=\"{1}\" /></div>\n",
                            b64, Enc(heading));
                    }

                    AddItemsCallout(items, thumbnails);

                    _sb.AppendFormat("<div class=\"page-number\">Page {0} of {{TOTAL_PAGES}}</div>\n", _pageCount);
                    _sb.AppendLine("</div>");
                }
            }
        }


        private void AddContentCell(string heading, byte[] contentImage,
            List<ContentItem> items, Dictionary<string, byte[]> thumbnails)
        {
            _sb.AppendLine("<div class=\"content-cell\">");
            _sb.AppendFormat("<div class=\"content-cell-header\"><span class=\"content-heading\">{0}</span></div>\n",
                Enc(heading));

            if (contentImage != null && contentImage.Length > 0)
            {
                string b64 = Convert.ToBase64String(contentImage);
                _sb.AppendFormat("<div class=\"content-cell-image\"><img src=\"data:image/png;base64,{0}\" alt=\"{1}\" /></div>\n",
                    b64, Enc(heading));
            }

            AddItemsCallout(items, thumbnails);
            _sb.AppendLine("</div>");
        }


        private void AddSectionContentPage(string heading, byte[] contentImage,
            List<ContentItem> items, Dictionary<string, byte[]> thumbnails)
        {
            _sb.AppendLine("<div class=\"section-step\">");
            _sb.AppendFormat("<div class=\"section-step-heading\">{0}</div>\n", Enc(heading));

            if (contentImage != null && contentImage.Length > 0)
            {
                string b64 = Convert.ToBase64String(contentImage);
                _sb.AppendFormat("<div class=\"section-step-image\"><img src=\"data:image/png;base64,{0}\" alt=\"{1}\" /></div>\n",
                    b64, Enc(heading));
            }

            // Compact inline items for section pages
            if (items != null && items.Count > 0)
            {
                _sb.AppendLine("<div class=\"section-items-inline\">");
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    _sb.Append("<span class=\"inline-item-chip\">");

                    // Thumbnail
                    byte[] thumbPng = null;
                    if (thumbnails != null && item.Key != null)
                        thumbnails.TryGetValue(item.Key, out thumbPng);
                    if (thumbPng != null && thumbPng.Length > 0)
                    {
                        string b64 = Convert.ToBase64String(thumbPng);
                        _sb.AppendFormat("<img class=\"inline-item-thumb\" src=\"data:image/png;base64,{0}\" alt=\"\" />",
                            b64);
                    }
                    else if (!string.IsNullOrEmpty(item.ColourHex))
                    {
                        _sb.AppendFormat("<span class=\"colour-swatch\" style=\"background:{0}\"></span>",
                            item.ColourHex);
                    }

                    _sb.AppendFormat("<span class=\"inline-item-qty\">{0}×</span> ", item.Quantity);
                    _sb.AppendFormat("{0}</span>\n", Enc(item.Label ?? ""));
                }
                _sb.AppendLine("</div>");
            }

            _sb.AppendLine("</div>");
        }


        private void AddItemsCallout(List<ContentItem> items,
            Dictionary<string, byte[]> thumbnails)
        {
            if (items == null || items.Count == 0) return;

            _sb.AppendLine("<div class=\"items-callout\">");
            _sb.AppendLine("<div class=\"items-callout-title\">Items</div>");
            _sb.AppendLine("<div class=\"items-list\">");
            foreach (var item in items)
            {
                _sb.AppendLine("<div class=\"item-entry\">");

                // Thumbnail
                byte[] thumbPng = null;
                if (thumbnails != null && item.Key != null)
                    thumbnails.TryGetValue(item.Key, out thumbPng);

                if (thumbPng != null && thumbPng.Length > 0)
                {
                    string b64 = Convert.ToBase64String(thumbPng);
                    _sb.AppendFormat("<img class=\"item-thumbnail\" src=\"data:image/png;base64,{0}\" alt=\"{1}\" />\n",
                        b64, Enc(item.Label ?? ""));
                }

                // Quantity badge
                _sb.AppendFormat("<span class=\"item-qty\">{0}×</span>", item.Quantity);

                // Label
                _sb.AppendFormat("<span class=\"item-label\">{0}</span>", Enc(item.Label ?? ""));

                // Colour swatch + subtitle
                if (!string.IsNullOrEmpty(item.ColourHex))
                {
                    _sb.AppendFormat("<span class=\"colour-swatch\" style=\"background:{0}\"></span>",
                        item.ColourHex);
                }
                if (!string.IsNullOrEmpty(item.Subtitle))
                {
                    _sb.AppendFormat("<span class=\"item-subtitle\">{0}</span>",
                        Enc(item.Subtitle));
                }

                _sb.AppendLine("</div>");
            }
            _sb.AppendLine("</div>");
            _sb.AppendLine("</div>");
        }


        private void CloseCurrentRow()
        {
            if (_pageOpen)
            {
                _sb.AppendLine("</div>"); // close .multi-content-row
                _sb.AppendFormat("<div class=\"page-number\">Page {0} of {{TOTAL_PAGES}}</div>\n", _pageCount);
                _sb.AppendLine("</div>"); // close .page
                _pageOpen = false;
                _pagesOnCurrentRow = 0;
            }
        }


        private static string Enc(string s)
        {
            return System.Net.WebUtility.HtmlEncode(s ?? "");
        }


        // ═══════════════════════════════════════════════════════════
        //  CSS generation (themed)
        // ═══════════════════════════════════════════════════════════

        private string BuildCss()
        {
            var t = _theme;
            return @"
*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

@page {
    size: " + _pageSize + @";
    margin: 12mm;
}

body {
    font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
    background: #f0f2f5;
    color: " + t.BodyTextHex + @";
}

.page {
    width: 100%;
    max-width: 210mm;
    margin: 20px auto;
    background: " + t.PageBackgroundHex + @";
    box-shadow: 0 2px 12px rgba(0,0,0,0.08);
    border-radius: 8px;
    padding: 24px;
    page-break-after: always;
    min-height: 280mm;
    display: flex;
    flex-direction: column;
}

/* ═══ Cover Page ═══ */
.cover-page {
    background: linear-gradient(160deg, " + t.GradientStartHex + @" 0%, " + t.GradientEndHex + @" 100%);
    justify-content: center;
    align-items: center;
    text-align: center;
    color: " + t.CoverTextHex + @";
    position: relative;
    overflow: hidden;
}
.cover-page::before {
    content: '';
    position: absolute;
    top: -50%; left: -50%;
    width: 200%; height: 200%;
    background: radial-gradient(circle at 30% 40%, rgba(255,255,255,0.03) 0%, transparent 60%);
    pointer-events: none;
}
.cover-content {
    max-width: 96%;
    position: relative;
    z-index: 1;
}
.cover-title {
    font-size: 2.8em;
    font-weight: 800;
    letter-spacing: -0.03em;
    margin-bottom: 16px;
    text-shadow: 0 2px 20px rgba(0,0,0,0.3);
}
.cover-image {
    width: 100%;
    height: auto;
    border-radius: 12px;
    box-shadow: 0 8px 40px rgba(0,0,0,0.4);
}

/* ═══ Closing Page ═══ */
.closing-page {
    background: linear-gradient(135deg, " + t.ClosingGradientStartHex + @" 0%, " + t.ClosingGradientEndHex + @" 100%);
    justify-content: center;
    align-items: center;
    text-align: center;
    color: " + t.CoverTextHex + @";
    position: relative;
    overflow: hidden;
}
.closing-page::before {
    content: '';
    position: absolute;
    top: -50%; left: -50%;
    width: 200%; height: 200%;
    background: radial-gradient(circle at 70% 60%, rgba(255,255,255,0.04) 0%, transparent 60%);
    pointer-events: none;
}
.closing-content {
    max-width: 96%;
    position: relative;
    z-index: 1;
}
.closing-title {
    font-size: 2.2em;
    font-weight: 800;
    letter-spacing: -0.02em;
    margin-bottom: 16px;
    text-shadow: 0 2px 16px rgba(0,0,0,0.3);
}
.closing-image {
    width: 100%;
    height: auto;
    border-radius: 12px;
    box-shadow: 0 8px 40px rgba(0,0,0,0.4);
}

/* ═══ Content Pages ═══ */
.content-header, .content-cell-header {
    display: flex;
    align-items: baseline;
    gap: 8px;
    margin-bottom: 16px;
    padding-bottom: 8px;
    border-bottom: 2px solid " + t.SeparatorHex + @";
}
.content-heading {
    font-size: 1.6em;
    font-weight: 700;
    color: " + t.AccentHex + @";
}
.content-image {
    flex: 1;
    display: flex;
    justify-content: center;
    align-items: center;
    margin: 12px 0;
}
.content-image img {
    width: 100%;
    height: auto;
    border-radius: 6px;
}

/* ═══ Multi-Content Row ═══ */
.multi-content-row {
    display: flex;
    gap: 20px;
    flex: 1;
}
.content-cell {
    flex: 1;
    display: flex;
    flex-direction: column;
    border: 1px solid " + t.SeparatorHex + @";
    border-radius: 10px;
    padding: 16px;
    background: #fafbfc;
}
.content-cell-header {
    margin-bottom: 10px;
    padding-bottom: 6px;
}
.content-cell-image {
    flex: 1;
    display: flex;
    justify-content: center;
    align-items: center;
    margin-bottom: 10px;
}
.content-cell-image img {
    width: 100%;
    height: auto;
    border-radius: 4px;
}

/* ═══ Section Callout ═══ */
.section-callout {
    border: 2px solid " + t.SectionBorderHex + @";
    border-radius: 12px;
    background: " + t.SectionBackgroundHex + @";
    padding: 16px 20px;
    margin: 12px 0;
}
.section-header {
    font-size: 1.1em;
    font-weight: 700;
    color: " + t.SectionBorderHex + @";
    margin-bottom: 12px;
}
.section-step {
    background: " + t.PageBackgroundHex + @";
    border: 1px solid " + t.SeparatorHex + @";
    border-radius: 8px;
    padding: 12px;
    margin-bottom: 10px;
}
.section-step-heading {
    font-weight: 600;
    font-size: 0.9em;
    color: " + t.AccentHex + @";
    margin-bottom: 8px;
}
.section-step-image {
    display: flex;
    justify-content: center;
    margin-bottom: 8px;
}
.section-step-image img {
    max-width: 100%;
    height: auto;
    border-radius: 4px;
}

/* ═══ Inline Items (section) ═══ */
.section-items-inline {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
    margin-top: 6px;
}
.inline-item-chip {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    background: " + t.CalloutBackgroundHex + @";
    border: 1px solid " + t.SeparatorHex + @";
    border-radius: 6px;
    padding: 3px 8px;
    font-size: 0.8em;
}
.inline-item-thumb {
    width: 16px;
    height: 16px;
    border-radius: 2px;
}
.inline-item-qty {
    font-weight: 700;
    color: " + t.AccentHex + @";
}

/* ═══ Items Callout ═══ */
.items-callout {
    background: " + t.CalloutBackgroundHex + @";
    border: 1px solid " + t.SeparatorHex + @";
    border-radius: 8px;
    padding: 12px 16px;
    margin-top: 12px;
}
.items-callout-title {
    font-size: 0.75em;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: " + t.MutedTextHex + @";
    margin-bottom: 8px;
}
.items-list {
    display: flex;
    flex-direction: column;
    gap: 6px;
}
.item-entry {
    display: flex;
    align-items: center;
    gap: 8px;
}
.item-thumbnail {
    width: 24px;
    height: 24px;
    border-radius: 3px;
}
.item-qty {
    font-weight: 700;
    color: " + t.AccentHex + @";
    min-width: 28px;
}
.item-label {
    font-size: 0.9em;
}
.item-subtitle {
    font-size: 0.8em;
    color: " + t.MutedTextHex + @";
}

/* ═══ Colour Swatch ═══ */
.colour-swatch {
    display: inline-block;
    width: 10px;
    height: 10px;
    border-radius: 50%;
    border: 1px solid rgba(0,0,0,0.15);
    vertical-align: middle;
}

/* ═══ Summary Grid ═══ */
.summary-header {
    margin-bottom: 16px;
}
.summary-title {
    font-size: 1.4em;
    font-weight: 700;
    color: " + t.BodyTextHex + @";
}
.summary-meta {
    font-size: 0.9em;
    color: " + t.MutedTextHex + @";
    margin-top: 4px;
    padding-bottom: 12px;
    border-bottom: 2px solid " + t.SeparatorHex + @";
}
.summary-grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 16px;
    margin-top: 16px;
}
.summary-item {
    text-align: center;
    padding: 8px;
}
.summary-thumb {
    width: 48px;
    height: 48px;
    border-radius: 4px;
    margin-bottom: 6px;
}
.summary-thumb-placeholder {
    display: inline-block;
    background: " + t.SeparatorHex + @";
}
.summary-qty {
    font-weight: 700;
    color: " + t.AccentHex + @";
    font-size: 0.9em;
    margin-bottom: 2px;
}
.summary-info {
    font-size: 0.8em;
}
.summary-label {
    font-weight: 500;
    margin-bottom: 2px;
}
.summary-colour-row {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 4px;
}
.summary-subtitle {
    color: " + t.MutedTextHex + @";
    font-size: 0.85em;
}

/* ═══ Page Number ═══ */
.page-number {
    margin-top: auto;
    text-align: center;
    font-size: 0.75em;
    color: " + t.MutedTextHex + @";
    padding-top: 12px;
}
.page-number-light {
    color: rgba(255,255,255,0.5);
}

/* ═══ Print Styles ═══ */
@media print {
    body { background: transparent; }
    .page {
        box-shadow: none;
        border-radius: 0;
        margin: 0;
        max-width: none;
    }
}
";
        }
    }
}
