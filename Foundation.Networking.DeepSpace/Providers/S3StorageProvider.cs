// ============================================================================
//
// S3StorageProvider.cs — Amazon S3 / S3-compatible storage provider.
//
// Implements IStorageProvider using the AWS SDK for .NET.
// Works with AWS S3, MinIO, Backblaze B2, Cloudflare R2, and any
// S3-compatible object store by configuring the ServiceURL endpoint.
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

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using Foundation.Networking.DeepSpace.Configuration;

namespace Foundation.Networking.DeepSpace.Providers
{
    /// <summary>
    ///
    /// S3-compatible storage provider.
    ///
    /// </summary>
    public class S3StorageProvider : IStorageProvider
    {
        private readonly AmazonS3Client _client;
        private readonly string _bucketName;


        public S3StorageProvider(S3StorageConfig config)
        {
            _bucketName = config.BucketName;

            AmazonS3Config s3Config = new AmazonS3Config();

            //
            // If a custom endpoint is specified, use it (MinIO, R2, B2, etc.)
            //
            if (string.IsNullOrEmpty(config.Endpoint) == false)
            {
                s3Config.ServiceURL = config.Endpoint;
                s3Config.ForcePathStyle = true;
            }
            else if (string.IsNullOrEmpty(config.Region) == false)
            {
                s3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(config.Region);
            }

            s3Config.UseHttp = (config.UseHttps == false);

            _client = new AmazonS3Client(config.AccessKey, config.SecretKey, s3Config);
        }


        public string ProviderName => "S3";


        // ── Put ───────────────────────────────────────────────────────────


        public async Task<StorageResult> PutAsync(
            string key, Stream data, string contentType = null,
            Dictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = NormalizeKey(key),
                    InputStream = data,
                    ContentType = contentType ?? "application/octet-stream",
                    AutoCloseStream = false
                };

                if (metadata != null)
                {
                    foreach (var kvp in metadata)
                    {
                        request.Metadata.Add("x-amz-meta-" + kvp.Key, kvp.Value);
                    }
                }

                PutObjectResponse response = await _client.PutObjectAsync(request, cancellationToken);

                return new StorageResult
                {
                    Success = true,
                    Object = new StorageObject
                    {
                        Key = key,
                        ETag = response.ETag?.Trim('"') ?? string.Empty,
                        ContentType = contentType ?? "application/octet-stream",
                        LastModifiedUtc = DateTime.UtcNow
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
                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = NormalizeKey(key),
                    Expires = DateTime.UtcNow.Add(expires)
                };

                string url = _client.GetPreSignedURL(request);
                return Task.FromResult(url);
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
                GetObjectResponse response = await _client.GetObjectAsync(_bucketName, NormalizeKey(key), cancellationToken);
                return response.ResponseStream;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
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
                GetObjectMetadataResponse response = await _client.GetObjectMetadataAsync(
                    _bucketName, NormalizeKey(key), cancellationToken);

                StorageObject obj = new StorageObject
                {
                    Key = key,
                    SizeBytes = response.ContentLength,
                    ContentType = response.Headers.ContentType ?? "application/octet-stream",
                    LastModifiedUtc = response.LastModified?.ToUniversalTime() ?? DateTime.UtcNow,
                    ETag = response.ETag?.Trim('"') ?? string.Empty
                };

                //
                // Map S3 user metadata (x-amz-meta-*) to our metadata dictionary
                //
                foreach (string metaKey in response.Metadata.Keys)
                {
                    string cleanKey = metaKey.Replace("x-amz-meta-", "");
                    obj.Metadata[cleanKey] = response.Metadata[metaKey];
                }

                return obj;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }


        // ── Exists / Delete ───────────────────────────────────────────────


        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _client.GetObjectMetadataAsync(_bucketName, NormalizeKey(key), cancellationToken);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }


        public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _client.DeleteObjectAsync(_bucketName, NormalizeKey(key), cancellationToken);
                return true;
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
                ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = NormalizeKey(prefix),
                    MaxKeys = maxResults
                };

                ListObjectsV2Response response = await _client.ListObjectsV2Async(request, cancellationToken);

                foreach (S3Object s3Obj in response.S3Objects)
                {
                    result.Objects.Add(new StorageObject
                    {
                        Key = s3Obj.Key,
                        SizeBytes = s3Obj.Size ?? 0,
                        LastModifiedUtc = s3Obj.LastModified?.ToUniversalTime() ?? DateTime.UtcNow,
                        ETag = s3Obj.ETag?.Trim('"') ?? string.Empty
                    });
                }

                result.HasMore = response.IsTruncated ?? false;
                result.ContinuationToken = response.NextContinuationToken ?? string.Empty;
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
                CopyObjectRequest request = new CopyObjectRequest
                {
                    SourceBucket = _bucketName,
                    SourceKey = NormalizeKey(sourceKey),
                    DestinationBucket = _bucketName,
                    DestinationKey = NormalizeKey(destinationKey)
                };

                CopyObjectResponse response = await _client.CopyObjectAsync(request, cancellationToken);

                return new StorageResult
                {
                    Success = true,
                    Object = new StorageObject
                    {
                        Key = destinationKey,
                        ETag = response.ETag?.Trim('"') ?? string.Empty,
                        LastModifiedUtc = DateTime.TryParse(response.LastModified, out DateTime parsed) ? parsed.ToUniversalTime() : DateTime.UtcNow
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
        /// Normalizes key by removing leading slashes (S3 keys should not start with /).
        /// </summary>
        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return key;
            }

            return key.TrimStart('/');
        }
    }
}
