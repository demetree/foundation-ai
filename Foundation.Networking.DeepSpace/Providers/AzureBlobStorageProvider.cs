// ============================================================================
//
// AzureBlobStorageProvider.cs — Azure Blob Storage provider.
//
// Implements IStorageProvider using the Azure.Storage.Blobs SDK.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

using Foundation.Networking.DeepSpace.Configuration;

namespace Foundation.Networking.DeepSpace.Providers
{
    /// <summary>
    ///
    /// Azure Blob Storage provider.
    ///
    /// </summary>
    public class AzureBlobStorageProvider : IStorageProvider
    {
        private readonly BlobContainerClient _containerClient;


        public AzureBlobStorageProvider(AzureBlobConfig config)
        {
            BlobServiceClient serviceClient = new BlobServiceClient(config.ConnectionString);
            _containerClient = serviceClient.GetBlobContainerClient(config.ContainerName);

            //
            // Ensure the container exists
            //
            _containerClient.CreateIfNotExists();
        }


        public string ProviderName => "AzureBlob";


        // ── Put ───────────────────────────────────────────────────────────


        public async Task<StorageResult> PutAsync(
            string key, Stream data, string contentType = null,
            Dictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                BlobClient blob = _containerClient.GetBlobClient(NormalizeKey(key));

                BlobUploadOptions options = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType ?? "application/octet-stream"
                    }
                };

                if (metadata != null && metadata.Count > 0)
                {
                    options.Metadata = metadata;
                }

                BlobContentInfo info = await blob.UploadAsync(data, options, cancellationToken);

                return new StorageResult
                {
                    Success = true,
                    Object = new StorageObject
                    {
                        Key = key,
                        ContentType = contentType ?? "application/octet-stream",
                        LastModifiedUtc = info.LastModified.UtcDateTime,
                        ETag = info.ETag.ToString().Trim('"')
                    }
                };
            }
            catch (Exception ex)
            {
                return new StorageResult { Success = false, Error = ex.Message };
            }
        }


        public async Task<StorageResult> PutBytesAsync(
            string key, byte[] data, string contentType = null,
            Dictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                return await PutAsync(key, ms, contentType, metadata, cancellationToken);
            }
        }


        // ── Get ───────────────────────────────────────────────────────────


        public Task<string> GetPresignedUrlAsync(string key, TimeSpan expires, CancellationToken cancellationToken = default)
        {
            try
            {
                BlobClient blob = _containerClient.GetBlobClient(NormalizeKey(key));

                if (blob.CanGenerateSasUri)
                {
                    Uri sasUri = blob.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expires));
                    return Task.FromResult(sasUri.ToString());
                }

                return Task.FromResult<string>(null);
            }
            catch (Exception)
            {
                return Task.FromResult<string>(null);
            }
        }

        public async Task<Stream> GetStreamAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                BlobClient blob = _containerClient.GetBlobClient(NormalizeKey(key));
                Response<BlobDownloadStreamingResult> response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
                return response.Value.Content;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }


        public async Task<byte[]> GetBytesAsync(string key, CancellationToken cancellationToken = default)
        {
            using (Stream stream = await GetStreamAsync(key, cancellationToken))
            {
                if (stream == null)
                {
                    return null;
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms, 81920, cancellationToken);
                    return ms.ToArray();
                }
            }
        }


        public async Task<StorageObject> GetMetadataAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                BlobClient blob = _containerClient.GetBlobClient(NormalizeKey(key));
                BlobProperties properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken);

                StorageObject obj = new StorageObject
                {
                    Key = key,
                    SizeBytes = properties.ContentLength,
                    ContentType = properties.ContentType ?? "application/octet-stream",
                    LastModifiedUtc = properties.LastModified.UtcDateTime,
                    ETag = properties.ETag.ToString().Trim('"')
                };

                //
                // Map Azure metadata
                //
                if (properties.Metadata != null)
                {
                    foreach (var kvp in properties.Metadata)
                    {
                        obj.Metadata[kvp.Key] = kvp.Value;
                    }
                }

                return obj;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }


        // ── Exists / Delete ───────────────────────────────────────────────


        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            BlobClient blob = _containerClient.GetBlobClient(NormalizeKey(key));
            Response<bool> response = await blob.ExistsAsync(cancellationToken);
            return response.Value;
        }


        public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                BlobClient blob = _containerClient.GetBlobClient(NormalizeKey(key));
                Response<bool> response = await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
                return response.Value;
            }
            catch (Exception)
            {
                return false;
            }
        }


        // ── List ──────────────────────────────────────────────────────────


        public async Task<ListResult> ListAsync(string prefix = "", int maxResults = 1000, CancellationToken cancellationToken = default)
        {
            ListResult result = new ListResult { Success = true };

            try
            {
                int count = 0;

                await foreach (BlobItem blobItem in _containerClient.GetBlobsAsync(
                    BlobTraits.All,
                    BlobStates.All,
                    prefix: NormalizeKey(prefix),
                    cancellationToken: cancellationToken))
                {
                    if (count >= maxResults)
                    {
                        result.HasMore = true;
                        break;
                    }

                    result.Objects.Add(new StorageObject
                    {
                        Key = blobItem.Name,
                        SizeBytes = blobItem.Properties.ContentLength ?? 0,
                        ContentType = blobItem.Properties.ContentType ?? "application/octet-stream",
                        LastModifiedUtc = blobItem.Properties.LastModified?.UtcDateTime ?? DateTime.UtcNow,
                        ETag = blobItem.Properties.ETag?.ToString().Trim('"') ?? string.Empty
                    });

                    count++;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
            }

            return result;
        }


        // ── Copy ──────────────────────────────────────────────────────────


        public async Task<StorageResult> CopyAsync(string sourceKey, string destinationKey, CancellationToken cancellationToken = default)
        {
            try
            {
                BlobClient sourceBlob = _containerClient.GetBlobClient(NormalizeKey(sourceKey));
                BlobClient destBlob = _containerClient.GetBlobClient(NormalizeKey(destinationKey));

                //
                // Check source exists
                //
                bool sourceExists = await sourceBlob.ExistsAsync(cancellationToken);
                if (sourceExists == false)
                {
                    return new StorageResult { Success = false, Error = "Source not found" };
                }

                //
                // Start the copy operation
                //
                CopyFromUriOperation operation = await destBlob.StartCopyFromUriAsync(sourceBlob.Uri, cancellationToken: cancellationToken);
                await operation.WaitForCompletionAsync(cancellationToken);

                BlobProperties props = await destBlob.GetPropertiesAsync(cancellationToken: cancellationToken);

                return new StorageResult
                {
                    Success = true,
                    Object = new StorageObject
                    {
                        Key = destinationKey,
                        SizeBytes = props.ContentLength,
                        ContentType = props.ContentType ?? "application/octet-stream",
                        LastModifiedUtc = props.LastModified.UtcDateTime,
                        ETag = props.ETag.ToString().Trim('"')
                    }
                };
            }
            catch (Exception ex)
            {
                return new StorageResult { Success = false, Error = ex.Message };
            }
        }


        // ── Internal ──────────────────────────────────────────────────────


        /// <summary>
        /// Normalizes key by removing leading slashes.
        /// </summary>
        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return key ?? string.Empty;
            }

            return key.TrimStart('/');
        }
    }
}
