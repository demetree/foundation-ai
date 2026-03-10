using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using ICSharpCode.SharpZipLib.Zip;

using BMC.LDraw.Models;

namespace BMC.LDraw.Parsers
{
    /// <summary>
    ///
    /// Creates valid BrickLink Studio .io archive files.
    ///
    /// The .io format is a password-protected ZIP archive containing:
    ///   - model.ldr — the primary LDraw model
    ///   - model2.ldr — MPD variant with embedded submodels (optional)
    ///   - modelv1.ldr — step-by-step variant (optional)
    ///   - thumbnail.png — preview image (optional)
    ///   - .info — JSON metadata about Studio version and part count
    ///   - model.ins — instruction XML settings (optional)
    ///   - errorPartList.err — part error report (optional)
    ///   - CustomParts/ — embedded LDraw geometry for non-standard parts (optional)
    ///
    /// This writer produces archives that BrickLink Studio can open natively.
    ///
    /// AI-developed code — initial implementation March 2026
    ///
    /// </summary>
    public static class StudioIoWriter
    {
        //
        // Entry names matching real Studio output (lowercase)
        //
        private const string MODEL_LDR = "model.ldr";
        private const string MODEL2_LDR = "model2.ldr";
        private const string MODELV1_LDR = "modelv1.ldr";
        private const string THUMBNAIL_PNG = "thumbnail.png";
        private const string INFO_FILE = ".info";
        private const string INSTRUCTIONS_FILE = "model.ins";
        private const string ERROR_PART_LIST_FILE = "errorPartList.err";
        private const string CUSTOM_PARTS_PREFIX = "CustomParts/";

        //
        // Current Studio password (Studio 2.1+)
        //
        private const string STUDIO_PASSWORD = "soho0909";

        //
        // Default Studio version to report if none specified
        //
        private const string DEFAULT_STUDIO_VERSION = "2.1.6_4";


        /// <summary>
        ///
        /// Create a BrickLink Studio .io archive from the given content.
        ///
        /// Returns the raw bytes of the ZIP archive ready for saving to disk
        /// or sending as a download response.
        ///
        /// </summary>
        /// <param name="request">Content and settings for the .io archive</param>
        /// <returns>Raw bytes of the password-protected ZIP archive</returns>
        public static byte[] WriteToBytes(StudioIoWriteRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.LDrawContent) == true)
            {
                throw new ArgumentException("LDrawContent is required to create a .io file.", nameof(request));
            }

            using (MemoryStream outputStream = new MemoryStream())
            {
                using (ZipOutputStream zipStream = new ZipOutputStream(outputStream))
                {
                    //
                    // Set the password if requested
                    //
                    if (request.UsePassword == true)
                    {
                        zipStream.Password = STUDIO_PASSWORD;
                    }

                    //
                    // Use Deflate compression for all entries
                    //
                    zipStream.SetLevel(6);

                    //
                    // Track which entry names we've written so AdditionalEntries don't override them
                    //
                    HashSet<string> writtenEntries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    //
                    // Entry 1: model.ldr (required)
                    //
                    WriteTextEntry(zipStream, MODEL_LDR, request.LDrawContent);
                    writtenEntries.Add(MODEL_LDR);

                    //
                    // Entry 2: model2.ldr (optional)
                    //
                    if (string.IsNullOrEmpty(request.Model2LdrContent) == false)
                    {
                        WriteTextEntry(zipStream, MODEL2_LDR, request.Model2LdrContent);
                        writtenEntries.Add(MODEL2_LDR);
                    }

                    //
                    // Entry 3: modelv1.ldr (optional)
                    //
                    if (string.IsNullOrEmpty(request.ModelV1LdrContent) == false)
                    {
                        WriteTextEntry(zipStream, MODELV1_LDR, request.ModelV1LdrContent);
                        writtenEntries.Add(MODELV1_LDR);
                    }

                    //
                    // Entry 4: CustomParts/ (optional — write before thumbnail so order matches Studio)
                    //
                    if (request.CustomParts != null && request.CustomParts.Count > 0)
                    {
                        foreach (KeyValuePair<string, byte[]> customPart in request.CustomParts)
                        {
                            string entryName = CUSTOM_PARTS_PREFIX + customPart.Key;
                            WriteBinaryEntry(zipStream, entryName, customPart.Value);
                            writtenEntries.Add(entryName);
                        }
                    }

                    //
                    // Entry 5: thumbnail.png (optional)
                    //
                    if (request.ThumbnailData != null && request.ThumbnailData.Length > 0)
                    {
                        WriteBinaryEntry(zipStream, THUMBNAIL_PNG, request.ThumbnailData);
                        writtenEntries.Add(THUMBNAIL_PNG);
                    }

                    //
                    // Entry 6: errorPartList.err (optional)
                    //
                    if (string.IsNullOrEmpty(request.ErrorPartList) == false)
                    {
                        WriteTextEntry(zipStream, ERROR_PART_LIST_FILE, request.ErrorPartList);
                        writtenEntries.Add(ERROR_PART_LIST_FILE);
                    }

                    //
                    // Entry 7: .info (always generated — written last like Studio does)
                    //
                    string infoContent = BuildInfoContent(request);
                    WriteTextEntry(zipStream, INFO_FILE, infoContent);
                    writtenEntries.Add(INFO_FILE);

                    //
                    // Entry 8: model.ins (optional)
                    //
                    if (string.IsNullOrEmpty(request.InstructionSettingsXml) == false)
                    {
                        WriteTextEntry(zipStream, INSTRUCTIONS_FILE, request.InstructionSettingsXml);
                        writtenEntries.Add(INSTRUCTIONS_FILE);
                    }

                    //
                    // Additional entries for round-trip fidelity
                    //
                    if (request.AdditionalEntries != null)
                    {
                        foreach (KeyValuePair<string, byte[]> entry in request.AdditionalEntries)
                        {
                            //
                            // Don't override any standard entries we already wrote
                            //
                            if (writtenEntries.Contains(entry.Key) == true)
                            {
                                continue;
                            }

                            WriteBinaryEntry(zipStream, entry.Key, entry.Value);
                            writtenEntries.Add(entry.Key);
                        }
                    }
                }

                return outputStream.ToArray();
            }
        }


        /// <summary>
        ///
        /// Create a .io file from a previously parsed StudioIoResult.
        ///
        /// This enables lossless round-trip: parse a .io file, modify the LDraw content
        /// or metadata, then write it back out as a valid .io file preserving all
        /// original entries including CustomParts.
        ///
        /// </summary>
        /// <param name="parseResult">The result from StudioIoParser.ParseBytes()</param>
        /// <param name="updatedLDrawContent">Optional updated LDraw content (uses original if null)</param>
        /// <returns>Raw bytes of the .io archive</returns>
        public static byte[] WriteFromParseResult(StudioIoResult parseResult, string updatedLDrawContent = null)
        {
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            //
            // Build the LDraw content: use updated content if provided, otherwise reconstruct from the parsed lines
            //
            string lDrawContent = updatedLDrawContent;

            if (string.IsNullOrEmpty(lDrawContent) == true && parseResult.LDrawLines != null)
            {
                lDrawContent = string.Join("\r\n", parseResult.LDrawLines);
            }

            //
            // Build additional entries from the AllEntries dictionary,
            // excluding the standard entries that we handle explicitly
            // and excluding CustomParts/ (handled separately)
            //
            Dictionary<string, byte[]> additionalEntries = null;

            if (parseResult.AllEntries != null && parseResult.AllEntries.Count > 0)
            {
                additionalEntries = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

                HashSet<string> standardEntries = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "model.ldr", "model2.ldr", "modelv1.ldr",
                    "thumbnail.png", ".info", "model.ins", "errorPartList.err"
                };

                foreach (KeyValuePair<string, byte[]> entry in parseResult.AllEntries)
                {
                    //
                    // Skip standard entries (handled via explicit fields)
                    // Skip CustomParts/ entries (handled via CustomParts dictionary)
                    //
                    if (standardEntries.Contains(entry.Key) == true)
                    {
                        continue;
                    }

                    if (entry.Key.StartsWith(CUSTOM_PARTS_PREFIX, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        continue;
                    }

                    additionalEntries[entry.Key] = entry.Value;
                }
            }

            StudioIoWriteRequest request = new StudioIoWriteRequest
            {
                LDrawContent = lDrawContent,
                Model2LdrContent = parseResult.Model2LdrContent,
                ModelV1LdrContent = parseResult.ModelV1LdrContent,
                ThumbnailData = parseResult.ThumbnailData,
                InstructionSettingsXml = parseResult.InstructionSettingsXml,
                ErrorPartList = parseResult.ErrorPartList,
                StudioVersion = parseResult.StudioVersion,
                PartCount = parseResult.ReportedPartCount,
                UsePassword = parseResult.WasPasswordProtected,
                CustomParts = parseResult.CustomParts,
                AdditionalEntries = additionalEntries
            };

            return WriteToBytes(request);
        }


        /// <summary>
        /// Build the .info file content in JSON format to match Studio 2.1+.
        /// Format: {"version":"2.1.6_4","total_parts":1418}
        /// </summary>
        private static string BuildInfoContent(StudioIoWriteRequest request)
        {
            string version = string.IsNullOrEmpty(request.StudioVersion) == false
                ? request.StudioVersion
                : DEFAULT_STUDIO_VERSION;

            StringBuilder sb = new StringBuilder();
            sb.Append("{\"version\":\"");
            sb.Append(version);
            sb.Append("\"");

            if (request.PartCount.HasValue == true)
            {
                sb.Append(",\"total_parts\":");
                sb.Append(request.PartCount.Value.ToString(CultureInfo.InvariantCulture));
            }

            sb.Append("}");

            return sb.ToString();
        }


        /// <summary>
        /// Write a text entry to the ZIP stream.
        /// </summary>
        private static void WriteTextEntry(ZipOutputStream zipStream, string entryName, string content)
        {
            byte[] data = Encoding.UTF8.GetBytes(content);
            WriteBinaryEntry(zipStream, entryName, data);
        }


        /// <summary>
        /// Write a binary entry to the ZIP stream.
        /// </summary>
        private static void WriteBinaryEntry(ZipOutputStream zipStream, string entryName, byte[] data)
        {
            ZipEntry entry = new ZipEntry(entryName);
            entry.DateTime = DateTime.UtcNow;
            entry.Size = data.Length;

            zipStream.PutNextEntry(entry);
            zipStream.Write(data, 0, data.Length);
            zipStream.CloseEntry();
        }
    }
}
