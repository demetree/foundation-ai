using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BMC.LDraw.Models;

namespace BMC.LDraw.Parsers
{
    /// <summary>
    /// Parses LDraw files (.dat, .ldr, .mpd) including all geometry lines (Types 1-5)
    /// and BFC meta-commands. Produces an LDrawGeometry containing all raw geometry.
    /// </summary>
    public static class GeometryParser
    {
        /// <summary>
        /// Parse a .dat, .ldr, or .mpd file into geometry data.
        /// For .mpd files, returns multiple geometries (one per FILE section).
        /// For .dat/.ldr files, returns a single-element list.
        /// </summary>
        public static List<LDrawGeometry> ParseFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            return ParseLines(lines, Path.GetFileName(filePath));
        }

        /// <summary>
        /// Parse geometry from an array of lines.
        /// </summary>
        public static List<LDrawGeometry> ParseLines(string[] lines, string defaultName = null)
        {
            // Check if this is an MPD file
            bool isMpd = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].TrimEnd();
                if (trimmed.StartsWith("0 FILE ") || trimmed.StartsWith("0 !FILE "))
                {
                    isMpd = true;
                    break;
                }
            }

            if (isMpd)
            {
                return ParseMpd(lines);
            }
            else
            {
                LDrawGeometry geo = ParseSingle(lines, 0, lines.Length);
                if (geo.Name == null) geo.Name = defaultName;
                return new List<LDrawGeometry> { geo };
            }
        }

        /// <summary>
        /// Parse an MPD file containing multiple FILE sections.
        /// </summary>
        private static List<LDrawGeometry> ParseMpd(string[] lines)
        {
            List<LDrawGeometry> geometries = new List<LDrawGeometry>();
            int fileStart = -1;
            string currentFileName = null;

            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].TrimEnd();

                if (trimmed.StartsWith("0 FILE ") || trimmed.StartsWith("0 !FILE "))
                {
                    if (fileStart >= 0 && currentFileName != null)
                    {
                        LDrawGeometry geo = ParseSingle(lines, fileStart, i);
                        geo.Name = currentFileName;
                        geometries.Add(geo);
                    }

                    currentFileName = trimmed.StartsWith("0 FILE ") ? trimmed.Substring(7).Trim() : trimmed.Substring(8).Trim();
                    fileStart = i + 1;
                }
                else if (trimmed == "0 NOFILE" || trimmed == "0 !NOFILE")
                {
                    if (fileStart >= 0 && currentFileName != null)
                    {
                        LDrawGeometry geo = ParseSingle(lines, fileStart, i);
                        geo.Name = currentFileName;
                        geometries.Add(geo);
                    }
                    fileStart = -1;
                    currentFileName = null;
                }
            }

            // Handle final section without NOFILE
            if (fileStart >= 0 && currentFileName != null)
            {
                LDrawGeometry geo = ParseSingle(lines, fileStart, lines.Length);
                geo.Name = currentFileName;
                geometries.Add(geo);
            }

            return geometries;
        }

        /// <summary>
        /// Parse a single file/section from a range of lines.
        /// </summary>
        private static LDrawGeometry ParseSingle(string[] lines, int startLine, int endLine)
        {
            LDrawGeometry geo = new LDrawGeometry();
            bool invertNext = false;
            LDrawRotStep currentRotStep = null;  // Active ROTSTEP state

            for (int i = startLine; i < endLine && i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd();

                if (string.IsNullOrWhiteSpace(line)) continue;

                char lineType = line[0];

                if (lineType == '0')
                {
                    // Meta-command or comment
                    string content = line.Length > 2 ? line.Substring(2).TrimStart() : "";

                    // BFC commands
                    if (content.StartsWith("BFC "))
                    {
                        string bfcCommand = content.Substring(4).Trim();
                        ProcessBfc(geo, bfcCommand, ref invertNext);
                    }
                    else if (content == "BFC")
                    {
                        // bare "0 BFC" — ignore
                    }
                    // STEP meta-command — marks a build step boundary
                    else if (content == "STEP")
                    {
                        geo.StepBreaks.Add(geo.SubfileReferences.Count);
                        geo.StepRotations.Add(currentRotStep);
                    }
                    // ROTSTEP meta-command — camera rotation override per step
                    else if (content.StartsWith("ROTSTEP"))
                    {
                        string rotContent = content.Substring(7).Trim();
                        if (rotContent == "END" || string.IsNullOrEmpty(rotContent))
                        {
                            currentRotStep = null;
                        }
                        else
                        {
                            // Parse: ROTSTEP x y z ABS|REL|ADD
                            string[] rotTokens = rotContent.Split(
                                new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (rotTokens.Length >= 3
                                && TryParseFloat(rotTokens[0], out float rx)
                                && TryParseFloat(rotTokens[1], out float ry)
                                && TryParseFloat(rotTokens[2], out float rz))
                            {
                                string rotType = rotTokens.Length >= 4
                                    ? rotTokens[3].ToUpperInvariant()
                                    : "REL";
                                currentRotStep = new LDrawRotStep
                                {
                                    X = rx, Y = ry, Z = rz, Type = rotType
                                };

                                // ROTSTEP implicitly ends the previous step (like STEP)
                                geo.StepBreaks.Add(geo.SubfileReferences.Count);
                                geo.StepRotations.Add(currentRotStep);
                            }
                        }
                    }
                    // Extract name from first description line
                    else if (geo.Name == null && content.Length > 0
                        && !content.StartsWith("!") && !content.StartsWith("//")
                        && !content.StartsWith("Name:") && !content.StartsWith("Author:")
                        && !content.StartsWith("FILE") && !content.StartsWith("NOFILE"))
                    {
                        geo.Name = content;
                    }

                    continue;
                }

                if (line.Length < 3 || line[1] != ' ') continue;

                switch (lineType)
                {
                    case '1':
                        LDrawSubfileReference subRef = ParseType1(line);
                        if (subRef != null)
                        {
                            int idx = geo.SubfileReferences.Count;
                            geo.SubfileReferences.Add(subRef);
                            if (invertNext)
                            {
                                geo.InvertNextIndices.Add(idx);
                                invertNext = false;
                            }
                        }
                        break;

                    case '2':
                        LDrawLine drawLine = ParseType2(line);
                        if (drawLine != null)
                        {
                            geo.Lines.Add(drawLine);
                        }
                        invertNext = false;
                        break;

                    case '3':
                        LDrawTriangle tri = ParseType3(line);
                        if (tri != null)
                        {
                            geo.Triangles.Add(tri);
                        }
                        invertNext = false;
                        break;

                    case '4':
                        LDrawQuad quad = ParseType4(line);
                        if (quad != null)
                        {
                            geo.Quads.Add(quad);
                        }
                        invertNext = false;
                        break;

                    case '5':
                        LDrawConditionalLine cline = ParseType5(line);
                        if (cline != null)
                        {
                            geo.ConditionalLines.Add(cline);
                        }
                        invertNext = false;
                        break;

                    default:
                        invertNext = false;
                        break;
                }
            }

            //
            // Add an implicit final step if there are subfile refs after the last STEP
            //
            int lastBreak = geo.StepBreaks.Count > 0 ? geo.StepBreaks[geo.StepBreaks.Count - 1] : 0;
            if (geo.SubfileReferences.Count > lastBreak)
            {
                geo.StepBreaks.Add(geo.SubfileReferences.Count);
                geo.StepRotations.Add(null);  // No ROTSTEP for implicit final step
            }

            return geo;
        }

        private static void ProcessBfc(LDrawGeometry geo, string command, ref bool invertNext)
        {
            // Handle combined forms like "CERTIFY CCW", "CERTIFY CW"
            if (command == "CERTIFY CCW" || command == "CERTIFY")
            {
                geo.BfcCertification = BfcCertification.Certified;
                geo.WindingCCW = true;
            }
            else if (command == "CERTIFY CW")
            {
                geo.BfcCertification = BfcCertification.Certified;
                geo.WindingCCW = false;
            }
            else if (command == "NOCERTIFY")
            {
                geo.BfcCertification = BfcCertification.NotCertified;
            }
            else if (command == "CCW")
            {
                geo.WindingCCW = true;
            }
            else if (command == "CW")
            {
                geo.WindingCCW = false;
            }
            else if (command == "INVERTNEXT")
            {
                invertNext = true;
            }
            // CLIP, NOCLIP — not needed for basic rendering
        }

        // ────────────────────────────────────────────────────────
        // Type 1: Sub-file reference
        // Format: 1 colour x y z a b c d e f g h i file.dat
        // ────────────────────────────────────────────────────────
        private static LDrawSubfileReference ParseType1(string line)
        {
            string[] tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 15) return null;

            LDrawSubfileReference subRef = new LDrawSubfileReference();
            if (!TryParseColourCode(tokens[1], out int col)) return null;
            subRef.ColourCode = col;

            if (!TryParseFloat(tokens[2], out subRef.X)) return null;
            if (!TryParseFloat(tokens[3], out subRef.Y)) return null;
            if (!TryParseFloat(tokens[4], out subRef.Z)) return null;

            for (int m = 0; m < 9; m++)
            {
                if (!TryParseFloat(tokens[5 + m], out subRef.Matrix[m])) return null;
            }

            subRef.FileName = tokens.Length == 15
                ? tokens[14]
                : string.Join(" ", tokens, 14, tokens.Length - 14);

            return subRef;
        }

        // ────────────────────────────────────────────────────────
        // Type 2: Line — 2 colour x1 y1 z1 x2 y2 z2
        // ────────────────────────────────────────────────────────
        private static LDrawLine ParseType2(string line)
        {
            string[] tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 8) return null;

            LDrawLine l = new LDrawLine();
            if (!TryParseColourCode(tokens[1], out l.ColourCode)) return null;
            if (!TryParseFloat(tokens[2], out l.X1)) return null;
            if (!TryParseFloat(tokens[3], out l.Y1)) return null;
            if (!TryParseFloat(tokens[4], out l.Z1)) return null;
            if (!TryParseFloat(tokens[5], out l.X2)) return null;
            if (!TryParseFloat(tokens[6], out l.Y2)) return null;
            if (!TryParseFloat(tokens[7], out l.Z2)) return null;
            return l;
        }

        // ────────────────────────────────────────────────────────
        // Type 3: Triangle — 3 colour x1 y1 z1 x2 y2 z2 x3 y3 z3
        // ────────────────────────────────────────────────────────
        private static LDrawTriangle ParseType3(string line)
        {
            string[] tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 11) return null;

            LDrawTriangle t = new LDrawTriangle();
            if (!TryParseColourCode(tokens[1], out t.ColourCode)) return null;
            if (!TryParseFloat(tokens[2], out t.X1)) return null;
            if (!TryParseFloat(tokens[3], out t.Y1)) return null;
            if (!TryParseFloat(tokens[4], out t.Z1)) return null;
            if (!TryParseFloat(tokens[5], out t.X2)) return null;
            if (!TryParseFloat(tokens[6], out t.Y2)) return null;
            if (!TryParseFloat(tokens[7], out t.Z2)) return null;
            if (!TryParseFloat(tokens[8], out t.X3)) return null;
            if (!TryParseFloat(tokens[9], out t.Y3)) return null;
            if (!TryParseFloat(tokens[10], out t.Z3)) return null;
            return t;
        }

        // ────────────────────────────────────────────────────────
        // Type 4: Quad — 4 colour x1 y1 z1 ... x4 y4 z4
        // ────────────────────────────────────────────────────────
        private static LDrawQuad ParseType4(string line)
        {
            string[] tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 14) return null;

            LDrawQuad q = new LDrawQuad();
            if (!TryParseColourCode(tokens[1], out q.ColourCode)) return null;
            if (!TryParseFloat(tokens[2], out q.X1)) return null;
            if (!TryParseFloat(tokens[3], out q.Y1)) return null;
            if (!TryParseFloat(tokens[4], out q.Z1)) return null;
            if (!TryParseFloat(tokens[5], out q.X2)) return null;
            if (!TryParseFloat(tokens[6], out q.Y2)) return null;
            if (!TryParseFloat(tokens[7], out q.Z2)) return null;
            if (!TryParseFloat(tokens[8], out q.X3)) return null;
            if (!TryParseFloat(tokens[9], out q.Y3)) return null;
            if (!TryParseFloat(tokens[10], out q.Z3)) return null;
            if (!TryParseFloat(tokens[11], out q.X4)) return null;
            if (!TryParseFloat(tokens[12], out q.Y4)) return null;
            if (!TryParseFloat(tokens[13], out q.Z4)) return null;
            return q;
        }

        // ────────────────────────────────────────────────────────
        // Type 5: Conditional line — 5 colour x1 y1 z1 x2 y2 z2 cx1 cy1 cz1 cx2 cy2 cz2
        // ────────────────────────────────────────────────────────
        private static LDrawConditionalLine ParseType5(string line)
        {
            string[] tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 14) return null;

            LDrawConditionalLine c = new LDrawConditionalLine();
            if (!TryParseColourCode(tokens[1], out c.ColourCode)) return null;
            if (!TryParseFloat(tokens[2], out c.X1)) return null;
            if (!TryParseFloat(tokens[3], out c.Y1)) return null;
            if (!TryParseFloat(tokens[4], out c.Z1)) return null;
            if (!TryParseFloat(tokens[5], out c.X2)) return null;
            if (!TryParseFloat(tokens[6], out c.Y2)) return null;
            if (!TryParseFloat(tokens[7], out c.Z2)) return null;
            if (!TryParseFloat(tokens[8], out c.CX1)) return null;
            if (!TryParseFloat(tokens[9], out c.CY1)) return null;
            if (!TryParseFloat(tokens[10], out c.CZ1)) return null;
            if (!TryParseFloat(tokens[11], out c.CX2)) return null;
            if (!TryParseFloat(tokens[12], out c.CY2)) return null;
            if (!TryParseFloat(tokens[13], out c.CZ2)) return null;
            return c;
        }

        private static bool TryParseFloat(string s, out float value)
        {
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }


        /// <summary>
        /// Parse an LDraw colour code, handling both standard decimal codes (e.g. "4")
        /// and hex direct colour codes (e.g. "0x2FF0000" for opaque red).
        ///
        /// Direct colour format:
        ///   0x2RRGGBB — opaque direct colour
        ///   0x3RRGGBB — semi-transparent direct colour
        ///   0x4RRGGBB — transparent direct colour
        ///
        /// These are stored as the raw integer value; the GeometryResolver extracts
        /// RGB from the value when converting to RGBA.
        /// </summary>
        internal static bool TryParseColourCode(string s, out int value)
        {
            if (s != null && s.Length > 2
                && s[0] == '0' && (s[1] == 'x' || s[1] == 'X'))
            {
                //
                // Hex direct colour code
                //
                return int.TryParse(s.Substring(2),
                    NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
            }

            return int.TryParse(s, out value);
        }
    }
}
