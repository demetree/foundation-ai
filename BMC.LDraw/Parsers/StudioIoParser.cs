using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using ICSharpCode.SharpZipLib.Zip;

using BMC.LDraw.Models;

namespace BMC.LDraw.Parsers
{
    /// <summary>
    ///
    /// Parses BrickLink Studio .io files to extract the embedded LDraw model data.
    ///
    /// The .io format is a password-protected ZIP archive containing:
    ///   - model.ldr — the primary LDraw model (flat, single-file)
    ///   - model2.ldr — full MPD variant with embedded submodels (richest data)
    ///   - modelv1.ldr — step-by-step variant with extended line format
    ///   - thumbnail.png — preview image
    ///   - .info — JSON metadata about Studio version and part count
    ///   - model.ins — instruction XML settings (optional)
    ///   - errorPartList.err — part error report (optional)
    ///   - CustomParts/ — embedded LDraw geometry for non-standard parts and primitives
    ///
    /// This parser uses SharpZipLib to handle the password-protected ZIP format.
    /// Studio has used different passwords across versions:
    ///   - "soho0909" (Studio 2.1+)
    ///   - "soN7pnHFRH" (older versions)
    ///
    /// AI-developed code — initial implementation March 2026
    ///
    /// </summary>
    public static class StudioIoParser
    {
        //
        // Known entry names inside the .io ZIP (case-insensitive matching used)
        //
        private const string MODEL_LDR = "model.ldr";
        private const string MODEL2_LDR = "model2.ldr";
        private const string MODELV1_LDR = "modelv1.ldr";
        private const string THUMBNAIL_PNG = "thumbnail.png";
        private const string INFO_FILE = ".info";
        private const string INSTRUCTIONS_FILE = "model.ins";
        private const string ERROR_PART_LIST_FILE = "errorPartList.err";

        //
        // Prefix for custom parts directory entries
        //
        private const string CUSTOM_PARTS_PREFIX = "CustomParts/";

        //
        // Known passwords for BrickLink Studio .io files (tried in order)
        //
        private static readonly string[] STUDIO_PASSWORDS = new string[]
        {
            "soho0909",       // Studio 2.1+ (current)
            "soN7pnHFRH"     // Older Studio versions
        };


        /// <summary>
        ///
        /// Parse a BrickLink Studio .io file from a file path.
        ///
        /// Opens the file, extracts the LDraw model data, thumbnail, and metadata.
        /// Returns a StudioIoResult containing all extracted content.
        ///
        /// </summary>
        /// <param name="filePath">Full path to the .io file</param>
        /// <returns>Extracted content from the .io archive</returns>
        public static StudioIoResult ParseFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) == true)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (File.Exists(filePath) == false)
            {
                throw new FileNotFoundException("Studio .io file not found.", filePath);
            }

            byte[] fileData = File.ReadAllBytes(filePath);
            string originalFileName = Path.GetFileName(filePath);

            return ParseBytes(fileData, originalFileName);
        }


        /// <summary>
        ///
        /// Parse a BrickLink Studio .io file from a byte array.
        ///
        /// This overload is useful for processing uploaded files that arrive as streams or byte arrays
        /// rather than files on disk.
        ///
        /// Handles both password-protected and unprotected archives:
        ///   1. Tries each known Studio password in order
        ///   2. Falls back to opening without a password if all fail
        ///   3. Extracts ALL entries from the archive for complete round-trip fidelity
        ///
        /// </summary>
        /// <param name="fileData">Raw bytes of the .io file</param>
        /// <param name="originalFileName">Original filename for reference</param>
        /// <returns>Extracted content from the .io archive</returns>
        public static StudioIoResult ParseBytes(byte[] fileData, string originalFileName)
        {
            if (fileData == null || fileData.Length == 0)
            {
                throw new ArgumentException("File data cannot be null or empty.", nameof(fileData));
            }

            StudioIoResult result = new StudioIoResult();
            result.OriginalFileName = originalFileName;
            result.AllEntries = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            result.CustomParts = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

            //
            // Open the .io file as a ZIP archive using SharpZipLib.
            //
            // Try each known password in order, then fall back to no password.
            //
            using (MemoryStream memoryStream = new MemoryStream(fileData))
            {
                ZipFile zipFile = null;

                try
                {
                    zipFile = new ZipFile(memoryStream);

                    //
                    // Try each known password in order
                    //
                    bool foundWorkingPassword = false;

                    foreach (string password in STUDIO_PASSWORDS)
                    {
                        zipFile.Password = password;

                        if (TestPasswordAccess(zipFile) == true)
                        {
                            result.WasPasswordProtected = true;
                            result.UsedPassword = password;
                            foundWorkingPassword = true;
                            break;
                        }
                    }

                    if (foundWorkingPassword == false)
                    {
                        //
                        // None of the known passwords worked — try without password
                        //
                        zipFile.Password = null;
                        result.WasPasswordProtected = false;
                    }
                }
                catch (Exception)
                {
                    //
                    // If anything goes wrong, try reopening without password
                    //
                    if (zipFile != null)
                    {
                        zipFile.Close();
                    }

                    memoryStream.Position = 0;
                    zipFile = new ZipFile(memoryStream);
                    result.WasPasswordProtected = false;
                }

                try
                {
                    //
                    // Step 1: Extract ALL entries into the AllEntries dictionary
                    //
                    ExtractAllEntries(zipFile, result);

                    //
                    // Step 2: Extract the primary LDraw model content
                    //
                    result.LDrawLines = ExtractLDrawContent(result.AllEntries);

                    //
                    // Step 3: Extract the alternate LDraw content for enrichment
                    //
                    if (result.AllEntries.ContainsKey(MODEL2_LDR) == true)
                    {
                        result.Model2LdrContent = BytesToString(result.AllEntries[MODEL2_LDR]);
                    }

                    if (result.AllEntries.ContainsKey(MODELV1_LDR) == true)
                    {
                        result.ModelV1LdrContent = BytesToString(result.AllEntries[MODELV1_LDR]);
                    }

                    //
                    // Step 4: Extract the thumbnail image if present
                    //
                    if (result.AllEntries.ContainsKey(THUMBNAIL_PNG) == true)
                    {
                        result.ThumbnailData = result.AllEntries[THUMBNAIL_PNG];
                    }

                    //
                    // Step 5: Parse the .info metadata file if present
                    //
                    ParseInfoFile(result);

                    //
                    // Step 6: Extract instruction settings XML if present
                    //
                    if (result.AllEntries.ContainsKey(INSTRUCTIONS_FILE) == true)
                    {
                        result.InstructionSettingsXml = BytesToString(result.AllEntries[INSTRUCTIONS_FILE]);
                    }

                    //
                    // Step 7: Extract error part list if present
                    //
                    if (result.AllEntries.ContainsKey(ERROR_PART_LIST_FILE) == true)
                    {
                        result.ErrorPartList = BytesToString(result.AllEntries[ERROR_PART_LIST_FILE]);
                    }

                    //
                    // Step 8: Extract custom parts from CustomParts/ directory
                    //
                    ExtractCustomParts(result);
                }
                finally
                {
                    if (zipFile != null)
                    {
                        zipFile.Close();
                    }
                }
            }

            return result;
        }


        /// <summary>
        ///
        /// Test whether the current password on a ZipFile allows reading entries.
        /// Returns true if the password works, false otherwise.
        ///
        /// </summary>
        private static bool TestPasswordAccess(ZipFile zipFile)
        {
            try
            {
                foreach (ZipEntry entry in zipFile)
                {
                    if (entry.IsFile == false)
                    {
                        continue;
                    }

                    //
                    // Try to read a few bytes from the first file entry to validate the password
                    //
                    using (Stream stream = zipFile.GetInputStream(entry))
                    {
                        byte[] buffer = new byte[1];
                        stream.Read(buffer, 0, 1);
                    }

                    return true;
                }

                //
                // No file entries — password is irrelevant
                //
                return true;
            }
            catch (ZipException)
            {
                return false;
            }
        }


        /// <summary>
        ///
        /// Extract ALL entries from the ZIP archive into the AllEntries dictionary.
        ///
        /// This ensures complete round-trip fidelity — even entries we don't explicitly
        /// parse are preserved so they can be written back when exporting to .io format.
        ///
        /// </summary>
        private static void ExtractAllEntries(ZipFile zipFile, StudioIoResult result)
        {
            foreach (ZipEntry entry in zipFile)
            {
                if (entry.IsFile == false)
                {
                    continue;
                }

                using (Stream entryStream = zipFile.GetInputStream(entry))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        entryStream.CopyTo(memoryStream);
                        result.AllEntries[entry.Name] = memoryStream.ToArray();
                    }
                }
            }
        }


        /// <summary>
        ///
        /// Extract custom part geometry files from the CustomParts/ directory.
        ///
        /// Studio embeds LDraw .dat geometry files for parts and primitives that
        /// may not exist in the standard LDraw library (custom parts, modified parts,
        /// and their dependent primitives like cylinders and edges).
        ///
        /// The structure mirrors the LDraw library layout:
        ///   CustomParts/88323.dat          — Custom part geometry
        ///   CustomParts/s/57518s01.dat      — Subpart geometry
        ///   CustomParts/p/rect.dat          — Standard-res primitives
        ///   CustomParts/p/48/1-4cyli.dat    — Hi-res primitives
        ///
        /// </summary>
        private static void ExtractCustomParts(StudioIoResult result)
        {
            foreach (KeyValuePair<string, byte[]> entry in result.AllEntries)
            {
                if (entry.Key.StartsWith(CUSTOM_PARTS_PREFIX, StringComparison.OrdinalIgnoreCase) == true)
                {
                    //
                    // Strip the "CustomParts/" prefix to get the relative path
                    // e.g. "CustomParts/p/rect.dat" → "p/rect.dat"
                    //
                    string relativePath = entry.Key.Substring(CUSTOM_PARTS_PREFIX.Length);

                    if (string.IsNullOrEmpty(relativePath) == false)
                    {
                        result.CustomParts[relativePath] = entry.Value;
                    }
                }
            }
        }


        /// <summary>
        ///
        /// Extract the LDraw model content from the extracted entries.
        /// Tries model.ldr first, then model2.ldr, then modelv1.ldr.
        /// Returns the content as an array of lines for feeding into ModelParser.ParseLines().
        ///
        /// </summary>
        private static string[] ExtractLDrawContent(Dictionary<string, byte[]> entries)
        {
            //
            // Try each known LDraw entry name in priority order
            //
            string[] entryNames = new string[] { MODEL_LDR, MODEL2_LDR, MODELV1_LDR };

            foreach (string entryName in entryNames)
            {
                if (entries.ContainsKey(entryName) == true)
                {
                    string content = BytesToString(entries[entryName]);
                    string[] lines = content.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                    return lines;
                }
            }

            //
            // If no standard LDraw file was found, look for any .ldr file in the entries
            //
            string ldrKey = entries.Keys
                .Where(k => k.EndsWith(".ldr", StringComparison.OrdinalIgnoreCase) == true)
                .FirstOrDefault();

            if (ldrKey != null)
            {
                string content = BytesToString(entries[ldrKey]);
                string[] lines = content.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                return lines;
            }

            throw new InvalidDataException("No LDraw model file (model.ldr, model2.ldr, or any .ldr) found in the .io archive.");
        }


        /// <summary>
        ///
        /// Parse the .info metadata file from the extracted entries.
        ///
        /// Supports two formats:
        ///   1. JSON (Studio 2.1+): {"version":"2.1.6_4","total_parts":1418}
        ///   2. Key-value (older): BrickLinkStudio: 2.x.x.x / PartCount: 123
        ///
        /// </summary>
        private static void ParseInfoFile(StudioIoResult result)
        {
            if (result.AllEntries.ContainsKey(INFO_FILE) == false)
            {
                return;
            }

            string infoContent = BytesToString(result.AllEntries[INFO_FILE]).Trim();

            //
            // Detect JSON format (starts with '{')
            //
            if (infoContent.StartsWith("{") == true)
            {
                ParseInfoJson(infoContent, result);
            }
            else
            {
                ParseInfoKeyValue(infoContent, result);
            }
        }


        /// <summary>
        ///
        /// Parse JSON-format .info content.
        /// Format: {"version":"2.1.6_4","total_parts":1418}
        ///
        /// Uses manual parsing to avoid a System.Text.Json dependency in this library.
        ///
        /// </summary>
        private static void ParseInfoJson(string json, StudioIoResult result)
        {
            //
            // Extract "version" value
            //
            string versionValue = ExtractJsonStringValue(json, "version");

            if (string.IsNullOrEmpty(versionValue) == false)
            {
                //
                // Clean up the version string — Studio sometimes embeds stray CR/LF or
                // literal "\r" escape sequences inside the JSON value
                //
                result.StudioVersion = versionValue
                    .Replace("\\r", "")
                    .Replace("\\n", "")
                    .Trim()
                    .TrimEnd('\r', '\n');
            }

            //
            // Extract "total_parts" value
            //
            string partsValue = ExtractJsonNumberValue(json, "total_parts");

            if (string.IsNullOrEmpty(partsValue) == false)
            {
                if (int.TryParse(partsValue.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int partCount) == true)
                {
                    result.ReportedPartCount = partCount;
                }
            }
        }


        /// <summary>
        /// Extract a string value from a JSON object by key name.
        /// Lightweight parser — avoids dependency on System.Text.Json.
        /// </summary>
        private static string ExtractJsonStringValue(string json, string key)
        {
            string searchKey = "\"" + key + "\"";
            int keyIndex = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase);

            if (keyIndex < 0)
            {
                return null;
            }

            int colonIndex = json.IndexOf(':', keyIndex + searchKey.Length);

            if (colonIndex < 0)
            {
                return null;
            }

            int openQuote = json.IndexOf('"', colonIndex + 1);

            if (openQuote < 0)
            {
                return null;
            }

            int closeQuote = json.IndexOf('"', openQuote + 1);

            if (closeQuote < 0)
            {
                return null;
            }

            return json.Substring(openQuote + 1, closeQuote - openQuote - 1);
        }


        /// <summary>
        /// Extract a numeric value from a JSON object by key name.
        /// </summary>
        private static string ExtractJsonNumberValue(string json, string key)
        {
            string searchKey = "\"" + key + "\"";
            int keyIndex = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase);

            if (keyIndex < 0)
            {
                return null;
            }

            int colonIndex = json.IndexOf(':', keyIndex + searchKey.Length);

            if (colonIndex < 0)
            {
                return null;
            }

            //
            // Find the start of the number (skip whitespace)
            //
            int numStart = colonIndex + 1;

            while (numStart < json.Length && (json[numStart] == ' ' || json[numStart] == '\t'))
            {
                numStart++;
            }

            //
            // Read digits
            //
            int numEnd = numStart;

            while (numEnd < json.Length && (char.IsDigit(json[numEnd]) || json[numEnd] == '-' || json[numEnd] == '.'))
            {
                numEnd++;
            }

            if (numEnd > numStart)
            {
                return json.Substring(numStart, numEnd - numStart);
            }

            return null;
        }


        /// <summary>
        /// Parse key-value format .info content (older Studio versions).
        /// Format: BrickLinkStudio: 2.x.x.x / PartCount: 123
        /// </summary>
        private static void ParseInfoKeyValue(string infoContent, StudioIoResult result)
        {
            string[] infoLines = infoContent.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in infoLines)
            {
                string trimmedLine = line.Trim();

                //
                // Look for the Studio version line
                //
                if (trimmedLine.StartsWith("BrickLinkStudio", StringComparison.OrdinalIgnoreCase) == true)
                {
                    int colonIndex = trimmedLine.IndexOf(':');

                    if (colonIndex >= 0 && colonIndex < trimmedLine.Length - 1)
                    {
                        result.StudioVersion = trimmedLine.Substring(colonIndex + 1).Trim();
                    }
                }

                //
                // Look for the part count line
                //
                if (trimmedLine.StartsWith("PartCount", StringComparison.OrdinalIgnoreCase) == true)
                {
                    int colonIndex = trimmedLine.IndexOf(':');

                    if (colonIndex >= 0 && colonIndex < trimmedLine.Length - 1)
                    {
                        string countStr = trimmedLine.Substring(colonIndex + 1).Trim();

                        if (int.TryParse(countStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int partCount) == true)
                        {
                            result.ReportedPartCount = partCount;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Convert a byte array to a UTF-8 string, stripping the BOM if present.
        /// </summary>
        private static string BytesToString(byte[] data)
        {
            //
            // Strip UTF-8 BOM (0xEF, 0xBB, 0xBF) if present
            //
            if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
            {
                return System.Text.Encoding.UTF8.GetString(data, 3, data.Length - 3);
            }

            return System.Text.Encoding.UTF8.GetString(data);
        }
    }
}
