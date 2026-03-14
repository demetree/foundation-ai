using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BMC.LDraw.Models;

namespace BMC.LDraw.Parsers
{
    /// <summary>
    /// Parses LDraw model files (.ldr and .mpd) into structured model data.
    /// </summary>
    public static class ModelParser
    {
        /// <summary>
        /// Parse an .ldr or .mpd model file.
        /// For .mpd files, returns a list of models (one per FILE section).
        /// For .ldr files, returns a single-element list.
        /// </summary>
        public static List<LDrawModel> ParseFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            return ParseLines(lines);
        }

        /// <summary>
        /// Parse model data from an array of lines (for testing).
        /// </summary>
        public static List<LDrawModel> ParseLines(string[] lines)
        {
            List<LDrawModel> models = new List<LDrawModel>();

            // Check if this is an MPD file (contains "0 FILE" directives)
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
                models = ParseMpd(lines);
            }
            else
            {
                LDrawModel model = ParseSingleModel(lines, 0, lines.Length);
                models.Add(model);
            }

            return models;
        }

        /// <summary>
        /// Parse an MPD file that contains multiple FILE sections.
        /// </summary>
        private static List<LDrawModel> ParseMpd(string[] lines)
        {
            List<LDrawModel> models = new List<LDrawModel>();
            int fileStart = -1;
            string currentFileName = null;

            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].TrimEnd();

                // "0 FILE <filename>" or "0 !FILE <filename>" starts a new sub-model
                if (trimmed.StartsWith("0 FILE ") || trimmed.StartsWith("0 !FILE "))
                {
                    // Save previous section
                    if (fileStart >= 0 && currentFileName != null)
                    {
                        LDrawModel model = ParseSingleModel(lines, fileStart, i);
                        model.Name = currentFileName;
                        models.Add(model);
                    }

                    // Start new section
                    currentFileName = trimmed.StartsWith("0 FILE ") ? trimmed.Substring(7).Trim() : trimmed.Substring(8).Trim();
                    fileStart = i + 1;
                }
                else if (trimmed == "0 NOFILE" || trimmed == "0 !NOFILE")
                {
                    // End of current FILE section
                    if (fileStart >= 0 && currentFileName != null)
                    {
                        LDrawModel model = ParseSingleModel(lines, fileStart, i);
                        model.Name = currentFileName;
                        models.Add(model);
                    }
                    fileStart = -1;
                    currentFileName = null;
                }
            }

            // Handle final section without NOFILE
            if (fileStart >= 0 && currentFileName != null)
            {
                LDrawModel model = ParseSingleModel(lines, fileStart, lines.Length);
                model.Name = currentFileName;
                models.Add(model);
            }

            return models;
        }

        /// <summary>
        /// Parse a single model from a range of lines.
        /// </summary>
        private static LDrawModel ParseSingleModel(string[] lines, int startLine, int endLine)
        {
            LDrawModel model = new LDrawModel();
            LDrawStep currentStep = new LDrawStep();
            bool nameParsed = false;

            // Pending ROTSTEP state — carries rotation forward until ROTSTEP END
            float? pendingRotX = null;
            float? pendingRotY = null;
            float? pendingRotZ = null;
            string pendingRotType = null;

            // Callout tracking state
            bool insideCallout = false;
            string pendingCalloutModelName = null;

            // LDCad state tracking
            List<int> pendingGroupIds = null;   // GROUP_NXT ids to apply to the next sub-file ref
            bool insideGenerated = false;        // Skip auto-generated fallback geometry

            for (int i = startLine; i < endLine && i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                //
                // If inside a GENERATED fallback section, skip lines until we hit
                // another "0 !LDCAD" meta (which ends the generated block) or end of file/subfile.
                //
                if (insideGenerated)
                {
                    if (line.StartsWith("0") && line.Contains("!LDCAD"))
                    {
                        insideGenerated = false;
                        // Fall through to process this meta line normally
                    }
                    else
                    {
                        continue; // Skip generated fallback geometry
                    }
                }

                // Type 0 — Comment or meta-command
                if (line.StartsWith("0"))
                {
                    string content = line.Length > 2 ? line.Substring(2).TrimStart() : "";

                    // Extract model title (first non-keyword comment)
                    if (!nameParsed && content.Length > 0 && !content.StartsWith("!")
                        && !content.StartsWith("//") && !content.StartsWith("Name:")
                        && !content.StartsWith("Author:") && content != "STEP"
                        && !content.StartsWith("BFC") && !content.StartsWith("FILE")
                        && !content.StartsWith("NOFILE") && !content.StartsWith("ROTSTEP"))
                    {
                        if (model.Name == null)
                        {
                            model.Name = content;
                        }
                        nameParsed = true;
                    }
                    else if (content.StartsWith("Author:"))
                    {
                        model.Author = content.Substring(7).Trim();
                    }
                    else if (content == "STEP")
                    {
                        // Commit current step and start a new one
                        if (currentStep.Parts.Count > 0)
                        {
                            // Apply any pending rotation from a previous ROTSTEP
                            if (pendingRotX.HasValue)
                            {
                                currentStep.RotStepX = pendingRotX;
                                currentStep.RotStepY = pendingRotY;
                                currentStep.RotStepZ = pendingRotZ;
                                currentStep.RotStepType = pendingRotType;
                            }

                            // Apply callout state
                            if (insideCallout)
                            {
                                currentStep.IsCallout = true;
                                currentStep.CalloutModelName = pendingCalloutModelName;
                            }

                            model.Steps.Add(currentStep);
                            currentStep = new LDrawStep();
                        }
                    }
                    //
                    // ROTSTEP — camera rotation override per step
                    //
                    // "0 ROTSTEP x y z TYPE" — acts as both a STEP break and a
                    // camera rotation for the current step.
                    // "0 ROTSTEP END" — clears rotation for subsequent steps.
                    //
                    else if (content.StartsWith("ROTSTEP"))
                    {
                        string rotContent = content.Substring(7).Trim();

                        if (rotContent.Equals("END", StringComparison.OrdinalIgnoreCase))
                        {
                            // Clear pending rotation
                            pendingRotX = null;
                            pendingRotY = null;
                            pendingRotZ = null;
                            pendingRotType = null;
                        }
                        else
                        {
                            // Parse: ROTSTEP x y z ABS|REL|ADD
                            string[] rotTokens = rotContent.Split(
                                new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                            if (rotTokens.Length >= 4)
                            {
                                if (float.TryParse(rotTokens[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float rx)
                                    && float.TryParse(rotTokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float ry)
                                    && float.TryParse(rotTokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float rz))
                                {
                                    // ROTSTEP acts as an implicit step break
                                    if (currentStep.Parts.Count > 0)
                                    {
                                        currentStep.RotStepX = rx;
                                        currentStep.RotStepY = ry;
                                        currentStep.RotStepZ = rz;
                                        currentStep.RotStepType = rotTokens[3].ToUpperInvariant();

                                        // Apply callout state
                                        if (insideCallout)
                                        {
                                            currentStep.IsCallout = true;
                                            currentStep.CalloutModelName = pendingCalloutModelName;
                                        }

                                        model.Steps.Add(currentStep);
                                        currentStep = new LDrawStep();
                                    }

                                    // Store as pending rotation for the next step
                                    pendingRotX = rx;
                                    pendingRotY = ry;
                                    pendingRotZ = rz;
                                    pendingRotType = rotTokens[3].ToUpperInvariant();
                                }
                            }
                        }
                    }
                    //
                    // !LPUB — LPub3D meta-commands
                    //
                    else if (content.StartsWith("!LPUB "))
                    {
                        string lpubContent = content.Substring(6).TrimStart();

                        //
                        // PAGE SIZE <width> <height>
                        //
                        if (lpubContent.StartsWith("PAGE SIZE ", StringComparison.OrdinalIgnoreCase))
                        {
                            string sizeStr = lpubContent.Substring(10).Trim();
                            string[] sizeParts = sizeStr.Split(
                                new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                            if (sizeParts.Length >= 2
                                && float.TryParse(sizeParts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float pw)
                                && float.TryParse(sizeParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float ph))
                            {
                                model.PageWidthMm = pw;
                                model.PageHeightMm = ph;
                            }
                        }
                        //
                        // PAGE ORIENTATION LANDSCAPE|PORTRAIT
                        //
                        else if (lpubContent.StartsWith("PAGE ORIENTATION ", StringComparison.OrdinalIgnoreCase))
                        {
                            string orient = lpubContent.Substring(17).Trim();
                            model.Landscape = orient.Equals("LANDSCAPE", StringComparison.OrdinalIgnoreCase);
                        }
                        //
                        // INSERT COVER_PAGE
                        //
                        else if (lpubContent.StartsWith("INSERT COVER_PAGE", StringComparison.OrdinalIgnoreCase))
                        {
                            model.HasCoverPage = true;
                        }
                        //
                        // INSERT PAGE — force page break before this step
                        //
                        else if (lpubContent.Equals("INSERT PAGE", StringComparison.OrdinalIgnoreCase))
                        {
                            currentStep.IsPageBreak = true;
                        }
                        //
                        // INSERT BOM — bill of materials page requested
                        //
                        else if (lpubContent.StartsWith("INSERT BOM", StringComparison.OrdinalIgnoreCase))
                        {
                            model.HasBillOfMaterials = true;
                        }
                        //
                        // END_OF_FILE — stop parsing immediately
                        //
                        else if (lpubContent.Equals("END_OF_FILE", StringComparison.OrdinalIgnoreCase))
                        {
                            break; // Exit the for loop — stops parsing this model
                        }
                        //
                        // FADE_PREV_STEP ON|OFF
                        //
                        else if (lpubContent.StartsWith("FADE_PREV_STEP ", StringComparison.OrdinalIgnoreCase))
                        {
                            string fadeValue = lpubContent.Substring(15).Trim();
                            currentStep.FadePrevStep = fadeValue.Equals("ON", StringComparison.OrdinalIgnoreCase);
                        }
                        //
                        // PAGE BACKGROUND COLOR <hex>
                        //
                        else if (lpubContent.StartsWith("PAGE BACKGROUND COLOR ", StringComparison.OrdinalIgnoreCase))
                        {
                            string colorHex = lpubContent.Substring(22).Trim();
                            if (!string.IsNullOrWhiteSpace(colorHex))
                            {
                                model.PageBackgroundColorHex = colorHex;
                            }
                        }
                        //
                        // CALLOUT BEGIN — marks subsequent steps as callout steps
                        //
                        else if (lpubContent.StartsWith("CALLOUT BEGIN", StringComparison.OrdinalIgnoreCase))
                        {
                            insideCallout = true;
                            pendingCalloutModelName = null;
                        }
                        //
                        // CALLOUT END — clears callout state
                        //
                        else if (lpubContent.StartsWith("CALLOUT END", StringComparison.OrdinalIgnoreCase))
                        {
                            insideCallout = false;
                            pendingCalloutModelName = null;
                        }
                        //
                        // PLI PER_STEP ON|OFF — show/hide Parts List Image per step
                        //
                        else if (lpubContent.StartsWith("PLI PER_STEP ", StringComparison.OrdinalIgnoreCase))
                        {
                            string pliValue = lpubContent.Substring(13).Trim();
                            currentStep.ShowPartsListImage = pliValue.Equals("ON", StringComparison.OrdinalIgnoreCase);
                        }
                    }
                    //
                    // !LDCAD — LDCad meta-commands (grouping + generated fallback)
                    //
                    else if (content.StartsWith("!LDCAD "))
                    {
                        string ldcadContent = content.Substring(7).TrimStart();

                        //
                        // GROUP_DEF — defines a group in this file
                        // Syntax: GROUP_DEF [topLevel=true] [LID=0] [GID=EjSe9B4luQa] [name=Group 1] [center=0 12 0]
                        //
                        if (ldcadContent.StartsWith("GROUP_DEF "))
                        {
                            var group = new LDCadGroup();
                            string propsStr = ldcadContent.Substring(10);

                            group.GID = ExtractLDCadParam(propsStr, "GID");
                            group.Name = ExtractLDCadParam(propsStr, "name");

                            string topLevelStr = ExtractLDCadParam(propsStr, "topLevel");
                            group.TopLevel = topLevelStr != null && topLevelStr.Equals("true", StringComparison.OrdinalIgnoreCase);

                            string lidStr = ExtractLDCadParam(propsStr, "LID");
                            if (lidStr != null && int.TryParse(lidStr, out int lid))
                            {
                                group.LocalId = lid;
                            }

                            string centerStr = ExtractLDCadParam(propsStr, "center");
                            if (centerStr != null)
                            {
                                string[] cParts = centerStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                if (cParts.Length >= 3
                                    && float.TryParse(cParts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float cx)
                                    && float.TryParse(cParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float cy)
                                    && float.TryParse(cParts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float cz))
                                {
                                    group.Center = new[] { cx, cy, cz };
                                }
                            }

                            if (!string.IsNullOrEmpty(group.GID))
                            {
                                model.Groups[group.GID] = group;
                            }
                        }
                        //
                        // GROUP_NXT — tag the next line (sub-file reference) with group membership
                        // Syntax: GROUP_NXT [ids=0] [nrs=2]
                        //
                        else if (ldcadContent.StartsWith("GROUP_NXT "))
                        {
                            string idsStr = ExtractLDCadParam(ldcadContent.Substring(10), "ids");
                            if (idsStr != null)
                            {
                                pendingGroupIds = new List<int>();
                                foreach (string idPart in idsStr.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (int.TryParse(idPart, out int gid))
                                    {
                                        pendingGroupIds.Add(gid);
                                    }
                                }
                            }
                        }
                        //
                        // GENERATED — marks start of auto-generated fallback geometry
                        // Skip everything until the next "0 !LDCAD" or end of range
                        //
                        else if (ldcadContent.StartsWith("GENERATED "))
                        {
                            model.HasGeneratedFallback = true;
                            insideGenerated = true;
                        }
                    }

                    continue;
                }

                // Type 1 — Sub-file reference
                if (line[0] == '1' && line.Length > 2 && line[1] == ' ')
                {
                    LDrawSubfileReference subRef = ParseType1Line(line);
                    if (subRef != null)
                    {
                        // Apply pending LDCad group IDs from a preceding GROUP_NXT meta
                        if (pendingGroupIds != null && pendingGroupIds.Count > 0)
                        {
                            subRef.GroupLocalIds = pendingGroupIds;
                            pendingGroupIds = null;
                        }

                        currentStep.Parts.Add(subRef);

                        // Capture the first sub-file reference inside a callout as the callout model name
                        if (insideCallout && pendingCalloutModelName == null && subRef.FileName != null)
                        {
                            pendingCalloutModelName = subRef.FileName;
                        }
                    }
                }

                // Types 2-5 (Line, Triangle, Quad, Optional Line) — geometry, skip for model parsing
            }

            // Add the last step if it has any parts
            if (currentStep.Parts.Count > 0)
            {
                // Apply any pending rotation from a previous ROTSTEP
                if (pendingRotX.HasValue)
                {
                    currentStep.RotStepX = pendingRotX;
                    currentStep.RotStepY = pendingRotY;
                    currentStep.RotStepZ = pendingRotZ;
                    currentStep.RotStepType = pendingRotType;
                }

                // Apply callout state
                if (insideCallout)
                {
                    currentStep.IsCallout = true;
                    currentStep.CalloutModelName = pendingCalloutModelName;
                }

                model.Steps.Add(currentStep);
            }

            return model;
        }

        /// <summary>
        /// Parse a Type 1 (sub-file reference) line.
        /// Format: 1 colour x y z a b c d e f g h i file.dat
        /// </summary>
        private static LDrawSubfileReference ParseType1Line(string line)
        {
            string[] tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            // Minimum: 1 + colour + x,y,z + 9 matrix values + filename = 15 tokens
            if (tokens.Length < 15)
            {
                return null;
            }

            LDrawSubfileReference subRef = new LDrawSubfileReference();

            // tokens[0] = "1" (line type)
            if (!int.TryParse(tokens[1], out int colourCode)) return null;
            subRef.ColourCode = colourCode;

            // Position
            if (!float.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float x)) return null;
            if (!float.TryParse(tokens[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float y)) return null;
            if (!float.TryParse(tokens[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float z)) return null;
            subRef.X = x;
            subRef.Y = y;
            subRef.Z = z;

            // 3x3 transformation matrix (a b c d e f g h i)
            for (int m = 0; m < 9; m++)
            {
                if (!float.TryParse(tokens[5 + m], NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                {
                    return null;
                }
                subRef.Matrix[m] = val;
            }

            // File name — may contain spaces, so join remaining tokens
            if (tokens.Length == 15)
            {
                subRef.FileName = tokens[14];
            }
            else
            {
                // File names with spaces (rare but possible)
                subRef.FileName = string.Join(" ", tokens, 14, tokens.Length - 14);
            }

            return subRef;
        }


        /// <summary>
        /// Extract a parameter value from LDCad's [key=value] format.
        /// Returns null if the key is not found.
        /// </summary>
        private static string ExtractLDCadParam(string propsStr, string key)
        {
            // Look for [key=value] pattern
            string searchKey = "[" + key + "=";
            int startIdx = propsStr.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase);
            if (startIdx < 0) return null;

            int valueStart = startIdx + searchKey.Length;
            int endIdx = propsStr.IndexOf(']', valueStart);
            if (endIdx < 0) return null;

            return propsStr.Substring(valueStart, endIdx - valueStart);
        }
    }
}
