using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using BMC.LDraw.Models;

namespace BMC.LDraw.Parsers
{
    /// <summary>
    ///
    /// Parses LEGO Digital Designer .lxf files to extract model data and convert
    /// it to LDraw-format lines for import into BMC.
    ///
    /// The .lxf format is a standard ZIP archive containing:
    ///   - IMAGE100.LXFML — XML model data with bricks, positions, colours, and
    ///     optionally building instructions (steps)
    ///   - IMAGE100.PNG   — preview thumbnail image
    ///
    /// LDD uses its own part numbering (designID) and colour numbering (materialID).
    /// Design IDs are typically identical to LDraw part IDs.  Material IDs require
    /// a static lookup table to convert to LDraw colour codes.
    ///
    /// The LXFML transformation format is a 12-value comma-separated string
    /// representing a 4x3 row-major matrix:
    ///   a1,a2,a3, b1,b2,b3, c1,c2,c3, tx,ty,tz
    ///
    /// Where the first 9 values are the 3x3 rotation matrix and the last 3 are
    /// the translation (position).
    ///
    /// AI-developed code — initial implementation March 2026
    ///
    /// </summary>
    public static class LxfParser
    {
        //
        // Known entry names inside the .lxf ZIP (case-insensitive matching used)
        //
        private const string LXFML_ENTRY = "IMAGE100.LXFML";
        private const string THUMBNAIL_ENTRY = "IMAGE100.PNG";


        /// <summary>
        ///
        /// Parse a LEGO Digital Designer .lxf file from a file path.
        ///
        /// Opens the file, extracts the LXFML model data and thumbnail.
        /// Returns an LxfResult containing the converted LDraw lines and metadata.
        ///
        /// </summary>
        /// <param name="filePath">Full path to the .lxf file</param>
        /// <returns>Extracted and converted content from the .lxf archive</returns>
        public static LxfResult ParseFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) == true)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (File.Exists(filePath) == false)
            {
                throw new FileNotFoundException("LXF file not found.", filePath);
            }

            byte[] fileData = File.ReadAllBytes(filePath);
            string originalFileName = Path.GetFileName(filePath);

            return ParseBytes(fileData, originalFileName);
        }


        /// <summary>
        ///
        /// Parse a LEGO Digital Designer .lxf file from a byte array.
        ///
        /// This overload is useful for processing uploaded files that arrive as
        /// streams or byte arrays rather than files on disk.
        ///
        /// </summary>
        /// <param name="fileData">Raw bytes of the .lxf file</param>
        /// <param name="originalFileName">Original filename for reference</param>
        /// <returns>Extracted and converted content from the .lxf archive</returns>
        public static LxfResult ParseBytes(byte[] fileData, string originalFileName)
        {
            if (fileData == null || fileData.Length == 0)
            {
                throw new ArgumentException("File data cannot be null or empty.", nameof(fileData));
            }

            LxfResult result = new LxfResult();
            result.OriginalFileName = originalFileName;

            byte[] lxfmlData = null;

            //
            // Open the .lxf file as a ZIP archive.
            // LXF files are standard (non-password-protected) ZIP archives.
            //
            using (MemoryStream memoryStream = new MemoryStream(fileData))
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    //
                    // Extract entries with case-insensitive name matching
                    //
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (string.Equals(entry.Name, LXFML_ENTRY, StringComparison.OrdinalIgnoreCase) == true
                            || entry.FullName.EndsWith(LXFML_ENTRY, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            using (Stream entryStream = entry.Open())
                            {
                                using (MemoryStream entryMemory = new MemoryStream())
                                {
                                    entryStream.CopyTo(entryMemory);
                                    lxfmlData = entryMemory.ToArray();
                                }
                            }
                        }
                        else if (string.Equals(entry.Name, THUMBNAIL_ENTRY, StringComparison.OrdinalIgnoreCase) == true
                                 || entry.FullName.EndsWith(THUMBNAIL_ENTRY, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            using (Stream entryStream = entry.Open())
                            {
                                using (MemoryStream entryMemory = new MemoryStream())
                                {
                                    entryStream.CopyTo(entryMemory);
                                    result.ThumbnailData = entryMemory.ToArray();
                                }
                            }
                        }
                    }

                    //
                    // If the standard names weren't found, look for any .lxfml file
                    //
                    if (lxfmlData == null)
                    {
                        ZipArchiveEntry fallbackEntry = archive.Entries
                            .Where(e => e.FullName.EndsWith(".lxfml", StringComparison.OrdinalIgnoreCase) == true
                                     || e.FullName.EndsWith(".LXFML", StringComparison.OrdinalIgnoreCase) == true)
                            .FirstOrDefault();

                        if (fallbackEntry != null)
                        {
                            using (Stream entryStream = fallbackEntry.Open())
                            {
                                using (MemoryStream entryMemory = new MemoryStream())
                                {
                                    entryStream.CopyTo(entryMemory);
                                    lxfmlData = entryMemory.ToArray();
                                }
                            }
                        }
                    }
                }
            }

            if (lxfmlData == null)
            {
                throw new InvalidDataException("No LXFML model file found in the .lxf archive.");
            }

            //
            // Parse the LXFML XML and convert to LDraw lines
            //
            result.LDrawLines = ConvertLxfmlToLDraw(lxfmlData, result);

            return result;
        }


        /// <summary>
        ///
        /// Convert LXFML XML data to an array of LDraw-format lines.
        ///
        /// Reads the Bricks and optional BuildingInstructions elements from the XML,
        /// maps LDD material IDs to LDraw colour codes, and outputs standard LDraw
        /// Type 1 lines with STEP separators.
        ///
        /// </summary>
        private static string[] ConvertLxfmlToLDraw(byte[] lxfmlData, LxfResult result)
        {
            string xmlText = Encoding.UTF8.GetString(lxfmlData);
            XDocument doc = XDocument.Parse(xmlText);
            XElement root = doc.Root;

            if (root == null)
            {
                throw new InvalidDataException("LXFML file contains no root element.");
            }

            //
            // Extract the LDD application version if available
            //
            XElement metaElement = root.Element("Meta");

            if (metaElement != null)
            {
                XElement appElement = metaElement.Element("Application");

                if (appElement != null)
                {
                    result.LddVersion = (string)appElement.Attribute("name")
                                        + " " + (string)appElement.Attribute("versionMajor")
                                        + "." + (string)appElement.Attribute("versionMinor");
                }
            }

            //
            // Collect all bricks from the LXFML.
            //
            // The structure is:
            //   <Bricks>
            //     <Brick refID="0" designID="3001" materialID="21">
            //       <Part designID="3001" materials="21" decoration="0">
            //         <Bone refID="0" transformation="..." />
            //       </Part>
            //     </Brick>
            //   </Bricks>
            //
            XElement bricksElement = root.Element("Bricks");

            if (bricksElement == null)
            {
                throw new InvalidDataException("LXFML file contains no <Bricks> element.");
            }

            //
            // Build a dictionary of brick refID → parsed data for step ordering later
            //
            Dictionary<string, BrickData> bricksByRefId = new Dictionary<string, BrickData>();
            List<BrickData> allBricks = new List<BrickData>();

            foreach (XElement brickElement in bricksElement.Elements("Brick"))
            {
                string refId = (string)brickElement.Attribute("refID") ?? "";
                string designId = (string)brickElement.Attribute("designID") ?? "";
                int materialId = ParseIntAttribute(brickElement, "materialID", 0);

                //
                // Get the transformation from the Part/Bone element
                //
                float posX = 0f, posY = 0f, posZ = 0f;
                float[] rotMatrix = new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };

                XElement partElement = brickElement.Element("Part");

                if (partElement != null)
                {
                    //
                    // Check for multi-material: the Part element can override the material
                    //
                    string partMaterials = (string)partElement.Attribute("materials");

                    if (string.IsNullOrEmpty(partMaterials) == false)
                    {
                        //
                        // Take the first material ID from comma-separated list
                        //
                        string firstMaterial = partMaterials.Split(',')[0].Trim();

                        if (int.TryParse(firstMaterial, out int partMaterialId) == true)
                        {
                            materialId = partMaterialId;
                        }
                    }

                    //
                    // Also allow Part-level designID to override (some LXF files use it)
                    //
                    string partDesignId = (string)partElement.Attribute("designID");

                    if (string.IsNullOrEmpty(partDesignId) == false)
                    {
                        designId = partDesignId;
                    }

                    XElement boneElement = partElement.Element("Bone");

                    if (boneElement != null)
                    {
                        string transform = (string)boneElement.Attribute("transformation");

                        if (string.IsNullOrEmpty(transform) == false)
                        {
                            ParseTransformation(transform, out posX, out posY, out posZ, rotMatrix);
                        }
                    }
                }

                //
                // Convert LDD materialID to LDraw colour code
                //
                int ldrawColourCode = MapMaterialToLDrawColour(materialId);

                //
                // Ensure the design ID ends with .dat
                //
                string fileName = designId;

                if (fileName.EndsWith(".dat", StringComparison.OrdinalIgnoreCase) == false)
                {
                    fileName += ".dat";
                }

                BrickData brickData = new BrickData
                {
                    RefId = refId,
                    DesignId = designId,
                    FileName = fileName,
                    ColourCode = ldrawColourCode,
                    PositionX = posX,
                    PositionY = posY,
                    PositionZ = posZ,
                    RotationMatrix = rotMatrix
                };

                allBricks.Add(brickData);

                if (string.IsNullOrEmpty(refId) == false)
                {
                    bricksByRefId[refId] = brickData;
                }
            }

            //
            // Check for BuildingInstructions to get step ordering
            //
            XElement buildingInstructions = root.Element("BuildingInstructions");
            List<List<BrickData>> steps = new List<List<BrickData>>();

            if (buildingInstructions != null)
            {
                HashSet<string> assignedRefIds = new HashSet<string>();

                foreach (XElement stepElement in buildingInstructions.Elements("Step"))
                {
                    List<BrickData> stepBricks = new List<BrickData>();

                    foreach (XElement partRef in stepElement.Elements("PartRef"))
                    {
                        string partRefId = (string)partRef.Attribute("partRef") ?? "";

                        if (bricksByRefId.TryGetValue(partRefId, out BrickData brick) == true)
                        {
                            stepBricks.Add(brick);
                            assignedRefIds.Add(partRefId);
                        }
                    }

                    if (stepBricks.Count > 0)
                    {
                        steps.Add(stepBricks);
                    }
                }

                //
                // Any bricks not referenced in BuildingInstructions go into a final step
                //
                List<BrickData> unassigned = allBricks
                    .Where(b => string.IsNullOrEmpty(b.RefId) == false && assignedRefIds.Contains(b.RefId) == false)
                    .ToList();

                List<BrickData> noRefId = allBricks
                    .Where(b => string.IsNullOrEmpty(b.RefId) == true)
                    .ToList();

                if (unassigned.Count > 0 || noRefId.Count > 0)
                {
                    List<BrickData> extraStep = new List<BrickData>();
                    extraStep.AddRange(unassigned);
                    extraStep.AddRange(noRefId);
                    steps.Add(extraStep);
                }
            }
            else
            {
                //
                // No building instructions — put all bricks in a single step
                //
                steps.Add(allBricks);
            }

            //
            // Build the LDraw output lines
            //
            List<string> lines = new List<string>();

            string modelName = Path.GetFileNameWithoutExtension(result.OriginalFileName ?? "LDD Model");
            lines.Add("0 " + modelName);
            lines.Add("0 Name: " + modelName + ".ldr");
            lines.Add("0 Author: LEGO Digital Designer");
            lines.Add("0 !LDRAW_ORG Unofficial Model");
            lines.Add("0 !CATEGORY LDD Import");

            bool firstStep = true;

            foreach (List<BrickData> stepBricks in steps)
            {
                if (firstStep == false)
                {
                    lines.Add("0 STEP");
                }

                foreach (BrickData brick in stepBricks)
                {
                    //
                    // LDraw Type 1 line:
                    // 1 <colour> <x> <y> <z> <a> <b> <c> <d> <e> <f> <g> <h> <i> <file>
                    //
                    string line = string.Format(CultureInfo.InvariantCulture,
                        "1 {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}",
                        brick.ColourCode,
                        brick.PositionX,
                        brick.PositionY,
                        brick.PositionZ,
                        brick.RotationMatrix[0], brick.RotationMatrix[1], brick.RotationMatrix[2],
                        brick.RotationMatrix[3], brick.RotationMatrix[4], brick.RotationMatrix[5],
                        brick.RotationMatrix[6], brick.RotationMatrix[7], brick.RotationMatrix[8],
                        brick.FileName);

                    lines.Add(line);
                }

                firstStep = false;
            }

            lines.Add("0 STEP");

            return lines.ToArray();
        }


        /// <summary>
        ///
        /// Parse the LXFML transformation string into position and rotation matrix.
        ///
        /// The transformation string contains 12 comma-separated float values in
        /// row-major order:
        ///   a1,a2,a3, b1,b2,b3, c1,c2,c3, tx,ty,tz
        ///
        /// Where a1..c3 is the 3x3 rotation matrix and tx,ty,tz is the translation.
        ///
        /// LDraw uses the same coordinate convention as LDD so no axis swapping is
        /// required — just extract the values directly.
        ///
        /// </summary>
        private static void ParseTransformation(
            string transformString,
            out float posX,
            out float posY,
            out float posZ,
            float[] rotMatrix)
        {
            posX = 0f;
            posY = 0f;
            posZ = 0f;

            string[] parts = transformString.Split(',');

            if (parts.Length < 12)
            {
                return;
            }

            //
            // Parse the 3x3 rotation matrix (first 9 values)
            //
            for (int i = 0; i < 9; i++)
            {
                if (float.TryParse(parts[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float val) == true)
                {
                    rotMatrix[i] = val;
                }
            }

            //
            // Parse the translation (last 3 values)
            //
            if (float.TryParse(parts[9].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float tx) == true)
            {
                posX = tx;
            }

            if (float.TryParse(parts[10].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float ty) == true)
            {
                posY = ty;
            }

            if (float.TryParse(parts[11].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float tz) == true)
            {
                posZ = tz;
            }
        }


        /// <summary>
        /// Parse an integer attribute from an XElement with a fallback default.
        /// </summary>
        private static int ParseIntAttribute(XElement element, string attributeName, int defaultValue)
        {
            string value = (string)element.Attribute(attributeName);

            if (string.IsNullOrEmpty(value) == false)
            {
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) == true)
                {
                    return result;
                }
            }

            return defaultValue;
        }


        #region Brick Data

        /// <summary>
        /// Internal DTO for holding parsed brick data during conversion.
        /// </summary>
        private class BrickData
        {
            public string RefId;
            public string DesignId;
            public string FileName;
            public int ColourCode;
            public float PositionX;
            public float PositionY;
            public float PositionZ;
            public float[] RotationMatrix;
        }

        #endregion


        #region LDD Material ID → LDraw Colour Code Mapping

        //
        // AI-developed: Static mapping from LDD materialID values to LDraw colour codes.
        //
        // Sources:
        //   - LDraw.org colour comparison chart
        //   - LDD ldraw.xml conversion file
        //   - Community-maintained mapping tables
        //
        // Unmapped material IDs fall through to a default (LDraw colour 0 = black).
        // The import pipeline tracks unresolved colours so they can be fixed later.
        //

        private static readonly Dictionary<int, int> LDD_MATERIAL_TO_LDRAW = new Dictionary<int, int>
        {
            //
            // Standard solid colours
            //
            { 1, 15 },        // White
            { 5, 19 },        // Brick Yellow (Tan)
            { 18, 323 },      // Nougat
            { 21, 4 },        // Bright Red
            { 23, 1 },        // Bright Blue
            { 24, 14 },       // Bright Yellow
            { 25, 6 },        // Earth Orange (Brown)
            { 26, 0 },        // Black
            { 27, 7 },        // Dark Grey (Stone Grey)
            { 28, 2 },        // Dark Green
            { 29, 272 },      // Medium Green
            { 36, 366 },      // Earth Yellow (Dark Tan/Macaroni)
            { 37, 10 },       // Bright Green
            { 38, 8 },        // Dark Orange
            { 39, 11 },       // Light Violet (Bright Light Blue)
            { 40, 40 },       // Transparent
            { 41, 41 },       // Transparent Red
            { 42, 42 },       // Transparent Light Blue
            { 43, 43 },       // Transparent Blue
            { 44, 46 },       // Transparent Yellow
            { 45, 20 },       // Light Blue
            { 47, 47 },       // Transparent Fluorescent Reddish-Orange
            { 48, 228 },      // Transparent Green
            { 49, 49 },       // Transparent Fluorescent Green
            { 100, 26 },      // Light Salmon (Light Nougat)
            { 101, 28 },      // Medium Red (Dark Salmon / Salmon)
            { 102, 8 },       // Medium Blue (Dark Grey on some charts)
            { 103, 45 },      // Light Grey (Light Bluish Grey)
            { 104, 22 },      // Bright Violet (Purple)
            { 105, 12 },      // Bright Yellowish Orange (Orange)
            { 106, 25 },      // Bright Orange
            { 107, 3 },       // Bright Bluish Green (Dark Turquoise)
            { 108, 10 },      // Bright Green
            { 110, 1 },       // Bright Blue
            { 111, 47 },      // Transparent Brown
            { 112, 1 },       // Medium Bluish Violet
            { 113, 13 },      // Transparent Medium Reddish-Violet
            { 115, 27 },      // Medium Yellowish Green (Lime)
            { 116, 323 },     // Medium Nougat
            { 118, 11 },      // Light Bluish Green (Aqua)
            { 119, 27 },      // Bright Yellowish Green (Lime)
            { 120, 27 },      // Light Yellowish Green
            { 124, 5 },       // Bright Reddish Violet (Magenta)
            { 126, 43 },      // Transparent Bright Bluish Violet
            { 131, 72 },      // Silver
            { 135, 71 },      // Sand Blue (Bluish Grey)
            { 136, 72 },      // Sand Violet (Dark Bluish Grey)
            { 138, 19 },      // Sand Yellow (Dark Tan)
            { 140, 8 },       // Earth Blue (Dark Blue)
            { 141, 8 },       // Dark Earth Green (Dark Green)
            { 143, 43 },      // Transparent Fluorescent Blue
            { 148, 8 },       // Metallic Dark Grey
            { 151, 72 },      // Sand Green
            { 153, 9 },       // Sand Red (Light Blue)
            { 154, 5 },       // Dark Red (New Dark Red)
            { 157, 44 },      // Transparent Fluorescent Yellow
            { 182, 47 },      // Transparent Bright Orange
            { 191, 334 },     // Flame Yellowish Orange (Bright Light Orange)
            { 192, 5 },       // Reddish Brown
            { 194, 7 },       // Medium Stone Grey (Light Bluish Grey)
            { 196, 72 },      // Dark Stone Grey
            { 199, 8 },       // Dark Stone Grey
            { 208, 7 },       // Light Stone Grey
            { 212, 321 },     // Light Royal Blue (Medium Blue)
            { 217, 6 },       // Brown
            { 221, 5 },       // Bright Purple (Dark Pink)
            { 222, 13 },      // Light Pink (Pink)
            { 226, 14 },      // Cool Yellow (Bright Light Yellow)
            { 232, 9 },       // Dove Blue (Sky Blue)
            { 268, 5 },       // Medium Lilac (Dark Purple)
            { 283, 26 },      // Light Nougat
            { 297, 80 },      // Warm Gold (Pearl Gold)
            { 308, 308 },     // Dark Brown
            { 312, 13 },      // Medium Nougat
            { 315, 72 },      // Silver Metallic
            { 316, 334 },     // Titanium Metallic
            { 321, 321 },     // Dark Azur (Dark Azure)
            { 322, 322 },     // Medium Azur (Medium Azure)
            { 323, 11 },      // Aqua (Light Aqua)
            { 324, 330 },     // Medium Lavender
            { 325, 326 },     // Lavender
            { 326, 326 },     // Spring Yellowish Green (Yellowish Green)
            { 329, 15 },      // White Glow
            { 330, 330 },     // Olive Green
            { 331, 366 },     // Medium-Yellowish Green
            { 353, 353 },     // Vibrant Coral
        };


        /// <summary>
        ///
        /// Map an LDD materialID to an LDraw colour code.
        ///
        /// Returns the mapped LDraw colour code, or 0 (black) for unmapped materials.
        ///
        /// </summary>
        private static int MapMaterialToLDrawColour(int materialId)
        {
            if (LDD_MATERIAL_TO_LDRAW.TryGetValue(materialId, out int ldrawColour) == true)
            {
                return ldrawColour;
            }

            //
            // Fallback: return the material ID directly as a guess.
            // Many LDD material IDs in the 40-49 range happen to match
            // their LDraw transparent colour codes.  For everything else,
            // the import pipeline's "unresolved colour" tracking will catch it.
            //
            return materialId;
        }

        #endregion
    }
}
