// ============================================================================
//
// StorageObjectSidecar.cs — Metadata sidecar model for disaster recovery.
//
// For each stored object, a companion .deepspace.json file is written
// alongside the object in the storage provider. If the metadata database
// is lost, these sidecar files can be scanned to rebuild the catalog.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Foundation.Networking.DeepSpace
{
    /// <summary>
    /// Metadata sidecar written alongside each stored object for disaster recovery.
    /// </summary>
    public class StorageObjectSidecar
    {
        /// <summary>
        /// The file extension used for sidecar metadata files.
        /// </summary>
        public const string SIDECAR_EXTENSION = ".deepspace.json";


        /// <summary>
        /// The storage key of the original object.
        /// </summary>
        [JsonPropertyName("key")]
        public string Key { get; set; }

        /// <summary>
        /// MIME content type.
        /// </summary>
        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        /// <summary>
        /// Size in bytes.
        /// </summary>
        [JsonPropertyName("sizeBytes")]
        public long SizeBytes { get; set; }

        /// <summary>
        /// MD5 hash of the content.
        /// </summary>
        [JsonPropertyName("md5Hash")]
        public string Md5Hash { get; set; }

        /// <summary>
        /// SHA-256 hash of the content (optional).
        /// </summary>
        [JsonPropertyName("sha256Hash")]
        public string Sha256Hash { get; set; }

        /// <summary>
        /// Name of the storage provider.
        /// </summary>
        [JsonPropertyName("provider")]
        public string Provider { get; set; }

        /// <summary>
        /// Storage tier name.
        /// </summary>
        [JsonPropertyName("tier")]
        public string Tier { get; set; } = "Standard";

        /// <summary>
        /// The current version number.
        /// </summary>
        [JsonPropertyName("versionNumber")]
        public int VersionNumber { get; set; } = 1;

        /// <summary>
        /// When the object was first created (UTC).
        /// </summary>
        [JsonPropertyName("createdUtc")]
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// User GUID who created the object.
        /// </summary>
        [JsonPropertyName("createdByUserGuid")]
        public Guid? CreatedByUserGuid { get; set; }

        /// <summary>
        /// Unique GUID for this object instance.
        /// </summary>
        [JsonPropertyName("objectGuid")]
        public Guid ObjectGuid { get; set; }

        /// <summary>
        /// When the sidecar was last updated (UTC).
        /// </summary>
        [JsonPropertyName("lastUpdatedUtc")]
        public DateTime LastUpdatedUtc { get; set; }


        // ── Helpers ───────────────────────────────────────────────────────


        /// <summary>
        /// Gets the sidecar key for a given object key.
        /// Example: "documents/report.pdf" → "documents/report.pdf.deepspace.json"
        /// </summary>
        public static string GetSidecarKey(string objectKey)
        {
            return objectKey + SIDECAR_EXTENSION;
        }


        /// <summary>
        /// Returns true if the key is a sidecar metadata file.
        /// </summary>
        public static bool IsSidecarKey(string key)
        {
            return key != null && key.EndsWith(SIDECAR_EXTENSION, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Serializes this sidecar to a JSON byte array.
        /// </summary>
        public byte[] ToJsonBytes()
        {
            return JsonSerializer.SerializeToUtf8Bytes(this, new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }


        /// <summary>
        /// Deserializes a sidecar from a JSON byte array.
        /// </summary>
        public static StorageObjectSidecar FromJsonBytes(byte[] json)
        {
            return JsonSerializer.Deserialize<StorageObjectSidecar>(json);
        }
    }
}
