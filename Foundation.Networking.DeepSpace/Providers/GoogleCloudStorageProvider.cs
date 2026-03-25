// ============================================================================
//
// GoogleCloudStorageProvider.cs — Google Cloud Storage provider.
//
// Implements IStorageProvider using the Google.Cloud.Storage.V1 SDK.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Object = Google.Apis.Storage.v1.Data.Object;
using Bucket = Google.Apis.Storage.v1.Data.Bucket;
using Google.Apis.Storage.v1.Data;

using Foundation.Networking.DeepSpace.Configuration;

namespace Foundation.Networking.DeepSpace.Providers
{
    public class GoogleCloudStorageProvider : IStorageProvider
    {
        private readonly StorageClient _client;
        private readonly string _bucketName;
        private readonly string _projectId;
        private readonly GoogleCredential _credential;


        public GoogleCloudStorageProvider(GoogleCloudStorageConfig config)
        {
            _bucketName = config.BucketName;
            _projectId = config.ProjectId;

            if (string.IsNullOrEmpty(config.CredentialsFilePath) == false)
            {
                _credential = GoogleCredential.FromFile(config.CredentialsFilePath);
                _client = StorageClient.Create(_credential);
            }
            else
            {
                _credential = GoogleCredential.GetApplicationDefault();
                _client = StorageClient.Create(_credential);
            }
        }


        public string ProviderName => "GCS";


        // ── Put ───────────────────────────────────────────────────────────


        public async Task<StorageResult> PutAsync(
            string key, Stream data, string contentType = null,
            Dictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Object obj = new Object
                {
                    Bucket = _bucketName,
                    Name = NormalizeKey(key),
                    ContentType = contentType ?? "application/octet-stream"
                };

                if (metadata != null && metadata.Count > 0)
                {
                    obj.Metadata = metadata;
                }

                Object result = await _client.UploadObjectAsync(obj, data, cancellationToken: cancellationToken);

                return new StorageResult
                {
                    Success = true,
                    Object = new StorageObject
                    {
                        Key = key,
                        ContentType = result.ContentType,
                        SizeBytes = (long)(result.Size ?? 0),
                        LastModifiedUtc = result.UpdatedDateTimeOffset?.UtcDateTime ?? DateTime.UtcNow,
                        ETag = result.ETag
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
                UrlSigner signer = UrlSigner.FromCredential(_credential);
                string url = signer.Sign(_bucketName, NormalizeKey(key), expires, HttpMethod.Get);
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
                MemoryStream ms = new MemoryStream();
                await _client.DownloadObjectAsync(_bucketName, NormalizeKey(key), ms, cancellationToken: cancellationToken);
                ms.Position = 0;
                return ms;
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<byte[]> GetBytesAsync(string key, CancellationToken cancellationToken = default)
        {
            using (Stream stream = await GetStreamAsync(key, cancellationToken))
            {
                if (stream == null) return null;

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
                Object obj = await _client.GetObjectAsync(_bucketName, NormalizeKey(key), cancellationToken: cancellationToken);

                StorageObject storageObject = new StorageObject
                {
                    Key = key,
                    SizeBytes = (long)(obj.Size ?? 0),
                    ContentType = obj.ContentType ?? "application/octet-stream",
                    LastModifiedUtc = obj.UpdatedDateTimeOffset?.UtcDateTime ?? DateTime.UtcNow,
                    ETag = obj.ETag ?? string.Empty
                };

                if (obj.Metadata != null)
                {
                    foreach (var kvp in obj.Metadata)
                    {
                        storageObject.Metadata[kvp.Key] = kvp.Value;
                    }
                }

                return storageObject;
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }


        // ── Exists / Delete ───────────────────────────────────────────────


        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return (await GetMetadataAsync(key, cancellationToken)) != null;
        }

        public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _client.DeleteObjectAsync(_bucketName, NormalizeKey(key), cancellationToken: cancellationToken);
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
                ListObjectsOptions options = new ListObjectsOptions { PageSize = maxResults };
                
                var results = _client.ListObjectsAsync(_bucketName, NormalizeKey(prefix), options);
                
                int count = 0;
                await foreach (var obj in results.WithCancellation(cancellationToken))
                {
                    if (count >= maxResults)
                    {
                        result.HasMore = true;
                        break;
                    }

                    result.Objects.Add(new StorageObject
                    {
                        Key = obj.Name,
                        SizeBytes = (long)(obj.Size ?? 0),
                        ContentType = obj.ContentType ?? "application/octet-stream",
                        LastModifiedUtc = obj.UpdatedDateTimeOffset?.UtcDateTime ?? DateTime.UtcNow,
                        ETag = obj.ETag ?? string.Empty
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
                Object dest = await _client.CopyObjectAsync(_bucketName, NormalizeKey(sourceKey), _bucketName, NormalizeKey(destinationKey), cancellationToken: cancellationToken);

                return new StorageResult
                {
                    Success = true,
                    Object = new StorageObject
                    {
                        Key = destinationKey,
                        SizeBytes = (long)(dest.Size ?? 0),
                        ContentType = dest.ContentType ?? "application/octet-stream",
                        LastModifiedUtc = dest.UpdatedDateTimeOffset?.UtcDateTime ?? DateTime.UtcNow,
                        ETag = dest.ETag ?? string.Empty
                    }
                };
            }
            catch (Exception ex)
            {
                return new StorageResult { Success = false, Error = ex.Message };
            }
        }


        // ── Advanced Operations (Bucket, Lifecycle, Metadata) ─────────────


        public async Task<bool> CreateBucketAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(_projectId)) return false;

                await _client.CreateBucketAsync(_projectId, new Bucket { Name = bucketName }, cancellationToken: cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<string>> ListBucketsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(_projectId)) return new List<string>();

                List<string> buckets = new List<string>();
                var bucketStream = _client.ListBucketsAsync(_projectId);
                
                await foreach (var b in bucketStream.WithCancellation(cancellationToken))
                {
                    buckets.Add(b.Name);
                }

                return buckets;
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public async Task<bool> DeleteBucketAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            try
            {
                await _client.DeleteBucketAsync(bucketName, cancellationToken: cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> SetExpirationLifecycleAsync(int expirationDays, CancellationToken cancellationToken = default)
        {
            try
            {
                Bucket bucket = await _client.GetBucketAsync(_bucketName, cancellationToken: cancellationToken);

                if (bucket.Lifecycle == null)
                {
                    bucket.Lifecycle = new Bucket.LifecycleData();
                }
                
                if (bucket.Lifecycle.Rule == null)
                {
                    bucket.Lifecycle.Rule = new List<Bucket.LifecycleData.RuleData>();
                }

                bucket.Lifecycle.Rule.Add(new Bucket.LifecycleData.RuleData
                {
                    Action = new Bucket.LifecycleData.RuleData.ActionData { Type = "Delete" },
                    Condition = new Bucket.LifecycleData.RuleData.ConditionData { Age = expirationDays }
                });

                await _client.PatchBucketAsync(bucket, cancellationToken: cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateMetadataAsync(string key, Dictionary<string, string> metadata, CancellationToken cancellationToken = default)
        {
            try
            {
                Object obj = await _client.GetObjectAsync(_bucketName, NormalizeKey(key), cancellationToken: cancellationToken);
                
                if (metadata != null)
                {
                    obj.Metadata = metadata;
                    await _client.PatchObjectAsync(obj, cancellationToken: cancellationToken);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        // ── Internal ──────────────────────────────────────────────────────


        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            return key.TrimStart('/');
        }
    }
}
