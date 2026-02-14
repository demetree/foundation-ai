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

            for (int i = startLine; i < endLine && i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
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
                        && !content.StartsWith("NOFILE"))
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
                            model.Steps.Add(currentStep);
                            currentStep = new LDrawStep();
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
                        currentStep.Parts.Add(subRef);
                    }
                }

                // Types 2-5 (Line, Triangle, Quad, Optional Line) — geometry, skip for model parsing
            }

            // Add the last step if it has any parts
            if (currentStep.Parts.Count > 0)
            {
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
    }
}
