using System;
using System.Collections.Generic;
using System.IO;
using BMC.LDraw.Models;

namespace BMC.LDraw.Parsers
{
    /// <summary>
    /// Parses the header section of an LDraw .dat part file to extract metadata.
    /// Only reads header lines — stops at the first geometry line for performance.
    /// </summary>
    public static class PartHeaderParser
    {
        /// <summary>
        /// Parse a .dat file and return only the header metadata.
        /// </summary>
        public static LDrawPartHeader ParseFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            return ParseLines(lines);
        }

        /// <summary>
        /// Parse header metadata from an array of lines (for testing).
        /// </summary>
        public static LDrawPartHeader ParseLines(string[] lines)
        {
            LDrawPartHeader header = new LDrawPartHeader();
            bool titleParsed = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd();

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // Stop at the first geometry line (Type 1-5 lines start with 1, 2, 3, 4, or 5)
                if (line.Length > 1 && line[0] >= '1' && line[0] <= '5' && line[1] == ' ')
                {
                    break;
                }

                // All header lines start with "0"
                if (!line.StartsWith("0"))
                {
                    continue;
                }

                // Get the content after "0 "
                string content = line.Length > 2 ? line.Substring(2).TrimStart() : "";

                // First non-empty comment line (without a keyword prefix) = Title
                if (!titleParsed && content.Length > 0 && !content.StartsWith("!") && !content.StartsWith("//") && !content.StartsWith("Name:") && !content.StartsWith("Author:"))
                {
                    header.Title = content;
                    titleParsed = true;
                    continue;
                }

                // Parse known header keywords
                if (content.StartsWith("Name:"))
                {
                    header.FileName = content.Substring(5).Trim();
                }
                else if (content.StartsWith("Author:"))
                {
                    header.Author = content.Substring(7).Trim();
                }
                else if (content.StartsWith("!LDRAW_ORG"))
                {
                    header.PartType = ExtractPartType(content);
                }
                else if (content.StartsWith("!LICENSE"))
                {
                    header.License = content.Substring(8).Trim();
                }
                else if (content.StartsWith("!CATEGORY"))
                {
                    header.Category = content.Substring(9).Trim();
                }
                else if (content.StartsWith("!KEYWORDS"))
                {
                    string kw = content.Substring(9).Trim();
                    ParseKeywords(kw, header.Keywords);
                }
                else if (content.StartsWith("!HISTORY"))
                {
                    header.History.Add(content.Substring(8).Trim());
                }
                else if (content == "BFC CERTIFY CCW" || content == "BFC CERTIFY CW" || content == "BFC NOCERTIFY")
                {
                    // Known BFC line — skip but don't stop parsing
                }
            }

            // If no explicit !CATEGORY, infer from the first word of the title
            if (string.IsNullOrEmpty(header.Category) && !string.IsNullOrEmpty(header.Title))
            {
                header.Category = InferCategoryFromTitle(header.Title);
            }

            return header;
        }

        /// <summary>
        /// Extract the part type from a "!LDRAW_ORG" line.
        /// Examples:
        ///   "!LDRAW_ORG Part UPDATE 2004-03"          → "Part"
        ///   "!LDRAW_ORG Subpart"                      → "Subpart"
        ///   "!LDRAW_ORG Primitive"                     → "Primitive"
        ///   "!LDRAW_ORG Shortcut UPDATE 2012-02"      → "Shortcut"
        ///   "!LDRAW_ORG Configuration UPDATE 2026-01"  → "Configuration"
        /// </summary>
        private static string ExtractPartType(string content)
        {
            // Content is "!LDRAW_ORG <PartType> [UPDATE <date>]" or similar
            string afterPrefix = content.Substring(10).Trim(); // Skip "!LDRAW_ORG"

            if (string.IsNullOrEmpty(afterPrefix))
            {
                return null;
            }

            // First word is the part type
            int spaceIdx = afterPrefix.IndexOf(' ');
            if (spaceIdx > 0)
            {
                return afterPrefix.Substring(0, spaceIdx);
            }

            return afterPrefix;
        }

        /// <summary>
        /// Parse comma-separated keywords from a !KEYWORDS line value.
        /// Keywords may have leading/trailing whitespace and trailing commas.
        /// </summary>
        private static void ParseKeywords(string keywordsText, List<string> keywords)
        {
            string[] parts = keywordsText.Split(',');
            foreach (string part in parts)
            {
                string trimmed = part.Trim();
                if (trimmed.Length > 0)
                {
                    keywords.Add(trimmed);
                }
            }
        }

        /// <summary>
        /// Infer part category from the first word of the title.
        /// LDraw convention: the first word of the title is typically the category.
        /// Examples: "Brick  2 x  4" → "Brick", "Technic Beam  3" → "Technic"
        /// </summary>
        private static string InferCategoryFromTitle(string title)
        {
            string trimmed = title.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                return null;
            }

            // Handle titles starting with "~" (moved/renamed parts) or "=" (alias)
            if (trimmed.StartsWith("~") || trimmed.StartsWith("=") || trimmed.StartsWith("_"))
            {
                trimmed = trimmed.Substring(1).TrimStart();
            }

            int spaceIdx = trimmed.IndexOf(' ');
            if (spaceIdx > 0)
            {
                return trimmed.Substring(0, spaceIdx);
            }

            return trimmed;
        }
    }
}
