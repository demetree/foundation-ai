using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

using BMC.LDraw.Models;

namespace BMC.LDraw.Parsers
{
    /// <summary>
    ///
    /// Parses BrickLink Studio .io files to extract the embedded LDraw model data.
    ///
    /// The .io format is a ZIP archive (sometimes password-protected) containing:
    ///   - model.ldr or model2.ldr — the primary LDraw model
    ///   - THUMBNAIL.PNG — preview image
    ///   - .INFO — metadata about Studio version and part count
    ///   - model.ins — instruction XML settings
    ///   - errorPartList.err — part error report
    ///
    /// This parser extracts the LDraw content and metadata so it can be fed into
    /// the existing ModelParser for full model parsing.
    ///
    /// AI-developed code — initial implementation March 2026
    ///
    /// </summary>
    public static class StudioIoParser
    {
        //
        // Known entry names inside the .io ZIP
        //
        private const string MODEL_LDR = "model.ldr";
        private const string MODEL2_LDR = "model2.ldr";
        private const string MODELV1_LDR = "modelv1.ldr";
        private const string THUMBNAIL_PNG = "THUMBNAIL.PNG";
        private const string INFO_FILE = ".INFO";
        private const string INSTRUCTIONS_FILE = "model.ins";


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

            //
            // Open the .io file as a ZIP archive.
            // The .io format is a standard ZIP, sometimes password-protected.
            // System.IO.Compression.ZipArchive does not support password-protected ZIPs natively,
            // so if the file is password-protected, this will throw and we fall through to
            // the error handling below.
            //
            using (MemoryStream memoryStream = new MemoryStream(fileData))
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    //
                    // Step 1: Extract the LDraw model content
                    //
                    result.LDrawLines = ExtractLDrawContent(archive);

                    //
                    // Step 2: Extract the thumbnail image if present
                    //
                    result.ThumbnailData = ExtractEntryBytes(archive, THUMBNAIL_PNG);

                    //
                    // Step 3: Parse the .INFO metadata file if present
                    //
                    ParseInfoFile(archive, result);

                    //
                    // Step 4: Extract instruction settings XML if present
                    //
                    result.InstructionSettingsXml = ExtractEntryText(archive, INSTRUCTIONS_FILE);
                }
            }

            return result;
        }


        /// <summary>
        ///
        /// Extract the LDraw model content from the ZIP archive.
        /// Tries model.ldr first, then model2.ldr, then modelv1.ldr.
        /// Returns the content as an array of lines for feeding into ModelParser.ParseLines().
        ///
        /// </summary>
        private static string[] ExtractLDrawContent(ZipArchive archive)
        {
            //
            // Try each known LDraw entry name in priority order
            //
            string[] entryNames = new string[] { MODEL_LDR, MODEL2_LDR, MODELV1_LDR };

            foreach (string entryName in entryNames)
            {
                string content = ExtractEntryText(archive, entryName);

                if (content != null)
                {
                    //
                    // Split into lines, handling both Windows and Unix line endings
                    //
                    string[] lines = content.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                    return lines;
                }
            }

            //
            // If no LDraw file was found, look for any .ldr file in the archive
            //
            ZipArchiveEntry ldrEntry = archive.Entries
                .Where(e => e.FullName.EndsWith(".ldr", StringComparison.OrdinalIgnoreCase) == true)
                .FirstOrDefault();

            if (ldrEntry != null)
            {
                string content = ReadEntryText(ldrEntry);
                string[] lines = content.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                return lines;
            }

            throw new InvalidDataException("No LDraw model file (model.ldr, model2.ldr, or any .ldr) found in the .io archive.");
        }


        /// <summary>
        ///
        /// Parse the .INFO metadata file from the archive.
        /// The .INFO file contains plain text lines with key-value pairs such as:
        ///   BrickLinkStudio: 2.x.x.x (version)
        ///   PartCount: 123
        ///
        /// </summary>
        private static void ParseInfoFile(ZipArchive archive, StudioIoResult result)
        {
            string infoContent = ExtractEntryText(archive, INFO_FILE);

            if (infoContent == null)
            {
                return;
            }

            //
            // Parse each line looking for known keys
            //
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
        /// Read a ZIP archive entry as a text string.  Returns null if the entry is not found.
        /// </summary>
        private static string ExtractEntryText(ZipArchive archive, string entryName)
        {
            //
            // Try case-insensitive match since different Studio versions may vary casing
            //
            ZipArchiveEntry entry = archive.Entries
                .Where(e => string.Equals(e.FullName, entryName, StringComparison.OrdinalIgnoreCase) == true)
                .FirstOrDefault();

            if (entry == null)
            {
                return null;
            }

            return ReadEntryText(entry);
        }


        /// <summary>
        /// Read a ZIP archive entry as a byte array.  Returns null if the entry is not found.
        /// </summary>
        private static byte[] ExtractEntryBytes(ZipArchive archive, string entryName)
        {
            //
            // Try case-insensitive match
            //
            ZipArchiveEntry entry = archive.Entries
                .Where(e => string.Equals(e.FullName, entryName, StringComparison.OrdinalIgnoreCase) == true)
                .FirstOrDefault();

            if (entry == null)
            {
                return null;
            }

            using (Stream entryStream = entry.Open())
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    entryStream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }


        /// <summary>
        /// Read a ZIP entry's full text content.
        /// </summary>
        private static string ReadEntryText(ZipArchiveEntry entry)
        {
            using (Stream entryStream = entry.Open())
            {
                using (StreamReader reader = new StreamReader(entryStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
