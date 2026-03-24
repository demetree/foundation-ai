// ============================================================================
//
// DeepSpaceConfiguration.cs — Configuration for the Deep Space storage system.
//
// AI-Developed | Gemini
//
// ============================================================================

using System.Collections.Generic;

namespace Foundation.Networking.DeepSpace.Configuration
{
    /// <summary>
    ///
    /// Configuration for the Deep Space cloud storage abstraction.
    ///
    /// </summary>
    public class DeepSpaceConfiguration
    {
        /// <summary>
        /// Default storage provider: "Local", "S3", "AzureBlob".
        /// </summary>
        public string DefaultProvider { get; set; } = "Local";

        /// <summary>
        /// Directory for the DeepSpace SQLite metadata database.
        /// Defaults to a "DeepSpace" subdirectory of the application base if empty.
        /// </summary>
        public string DatabaseDirectory { get; set; } = "";

        /// <summary>
        /// Local filesystem provider settings.
        /// </summary>
        public LocalStorageConfig LocalStorage { get; set; } = new LocalStorageConfig();

        /// <summary>
        /// S3-compatible provider settings.
        /// </summary>
        public S3StorageConfig S3Storage { get; set; } = new S3StorageConfig();

        /// <summary>
        /// Azure Blob provider settings.
        /// </summary>
        public AzureBlobConfig AzureBlob { get; set; } = new AzureBlobConfig();
    }


    /// <summary>
    /// Local filesystem storage configuration.
    /// </summary>
    public class LocalStorageConfig
    {
        /// <summary>
        /// Root directory for local storage.
        /// </summary>
        public string RootPath { get; set; } = "./storage";

        /// <summary>
        /// Maximum file size in bytes (0 = unlimited).
        /// </summary>
        public long MaxFileSizeBytes { get; set; } = 0;
    }


    /// <summary>
    /// S3-compatible storage configuration.
    /// </summary>
    public class S3StorageConfig
    {
        public string Endpoint { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public string Region { get; set; } = "us-east-1";
        public bool UseHttps { get; set; } = true;
    }


    /// <summary>
    /// Azure Blob storage configuration.
    /// </summary>
    public class AzureBlobConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
    }
}
