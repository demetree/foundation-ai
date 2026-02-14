using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BMC.LDraw.Models;

namespace BMC.LDraw.Parsers
{
    /// <summary>
    /// Parses LDConfig.ldr to extract all colour definitions.
    /// </summary>
    public static class ColourConfigParser
    {
        /// <summary>
        /// Parse an LDConfig.ldr file and return all colour definitions.
        /// </summary>
        public static List<LDrawColour> ParseFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            return ParseLines(lines);
        }

        /// <summary>
        /// Parse colour definitions from an array of lines (for testing).
        /// </summary>
        public static List<LDrawColour> ParseLines(string[] lines)
        {
            List<LDrawColour> colours = new List<LDrawColour>();

            // Track the most recent LEGOID comment so we can attach it to the next !COLOUR
            int? pendingLegoId = null;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd();

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // Look for LEGOID comments: "0                              // LEGOID  26 - Black"
                if (line.StartsWith("0") && line.Contains("// LEGOID"))
                {
                    pendingLegoId = ExtractLegoId(line);
                    continue;
                }

                // Look for !COLOUR definitions
                if (!line.Contains("!COLOUR"))
                {
                    // Reset pending LEGO ID if we hit a non-LEGOID, non-COLOUR line
                    if (!line.Contains("// LEGO") && !line.Contains("// MODULEX"))
                    {
                        // Keep pendingLegoId for LEGO/MODULEX comments that aren't LEGOID format
                    }
                    continue;
                }

                LDrawColour colour = ParseColourLine(line, pendingLegoId);
                if (colour != null)
                {
                    colours.Add(colour);
                }

                // Reset after consuming
                pendingLegoId = null;
            }

            return colours;
        }

        /// <summary>
        /// Parse a single "0 !COLOUR ..." line into an LDrawColour.
        /// </summary>
        private static LDrawColour ParseColourLine(string line, int? legoId)
        {
            LDrawColour colour = new LDrawColour();
            colour.LegoId = legoId;

            // Default values
            colour.Alpha = 255;
            colour.FinishType = "Solid";

            // Tokenize the line by whitespace
            string[] tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            // Expected minimum structure: 0 !COLOUR <Name> CODE <n> VALUE <hex> EDGE <hex> [ALPHA <n>] [LUMINANCE <n>] [CHROME|PEARLESCENT|METAL|RUBBER] [MATERIAL ...]
            if (tokens.Length < 9)
            {
                return null;
            }

            // tokens[0] = "0"
            // tokens[1] = "!COLOUR"
            // tokens[2] = Name
            colour.Name = tokens[2];

            // Parse remaining tokens as key-value pairs and flags
            for (int t = 3; t < tokens.Length; t++)
            {
                switch (tokens[t])
                {
                    case "CODE":
                        if (t + 1 < tokens.Length && int.TryParse(tokens[t + 1], out int code))
                        {
                            colour.Code = code;
                            t++;
                        }
                        break;

                    case "VALUE":
                        if (t + 1 < tokens.Length && tokens[t + 1].StartsWith("#"))
                        {
                            colour.HexValue = tokens[t + 1];
                            t++;
                        }
                        break;

                    case "EDGE":
                        if (t + 1 < tokens.Length && tokens[t + 1].StartsWith("#"))
                        {
                            colour.HexEdge = tokens[t + 1];
                            t++;
                        }
                        break;

                    case "ALPHA":
                        if (t + 1 < tokens.Length && int.TryParse(tokens[t + 1], out int alpha))
                        {
                            colour.Alpha = alpha;
                            t++;
                        }
                        break;

                    case "LUMINANCE":
                        if (t + 1 < tokens.Length && int.TryParse(tokens[t + 1], out int luminance))
                        {
                            colour.Luminance = luminance;
                            t++;
                        }
                        break;

                    // Direct finish type keywords (no MATERIAL prefix)
                    case "CHROME":
                        colour.FinishType = "Chrome";
                        break;

                    case "PEARLESCENT":
                        colour.FinishType = "Pearlescent";
                        break;

                    case "METAL":
                        colour.FinishType = "Metal";
                        break;

                    case "RUBBER":
                        colour.FinishType = "Rubber";
                        break;

                    // MATERIAL prefix for compound finish types
                    case "MATERIAL":
                        if (t + 1 < tokens.Length)
                        {
                            string materialType = tokens[t + 1];
                            switch (materialType)
                            {
                                case "GLITTER":
                                    colour.FinishType = "Glitter";
                                    break;
                                case "SPECKLE":
                                    colour.FinishType = "Speckle";
                                    break;
                                default:
                                    colour.FinishType = materialType;
                                    break;
                            }
                        }
                        // Skip remaining MATERIAL parameters (VALUE, FRACTION, etc.)
                        t = tokens.Length;
                        break;
                }
            }

            // Derive convenience flags
            colour.IsTransparent = colour.Alpha < 255;
            colour.IsMetallic = colour.FinishType == "Chrome"
                             || colour.FinishType == "Metal"
                             || colour.FinishType == "Pearlescent";

            // Determine finish type for transparent and milky colours that don't have explicit material keywords
            if (colour.FinishType == "Solid")
            {
                if (colour.Alpha > 0 && colour.Alpha < 255 && colour.Luminance.HasValue)
                {
                    // Milky/glow colours have sub-255 alpha plus luminance
                    colour.FinishType = "Milky";
                }
                else if (colour.Alpha < 255)
                {
                    colour.FinishType = "Transparent";
                }
            }

            return colour;
        }

        /// <summary>
        /// Extract LEGO ID from a comment line like "// LEGOID  26 - Black".
        /// </summary>
        private static int? ExtractLegoId(string line)
        {
            int idx = line.IndexOf("LEGOID");
            if (idx < 0) return null;

            // Skip past "LEGOID" + whitespace
            string after = line.Substring(idx + 6).TrimStart();

            // Parse the number (stops at first non-digit character)
            string numberStr = "";
            foreach (char c in after)
            {
                if (char.IsDigit(c))
                {
                    numberStr += c;
                }
                else if (numberStr.Length > 0)
                {
                    break;
                }
            }

            if (int.TryParse(numberStr, out int id))
            {
                return id;
            }

            return null;
        }
    }
}
