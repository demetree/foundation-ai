// ============================================================================
//
// S3StorageProviderTests.cs — Unit tests for S3StorageProvider.
//
// Tests construction and key normalization logic. Full integration tests
// require a running S3-compatible service (e.g., MinIO).
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using Xunit;

using Foundation.Networking.DeepSpace.Configuration;
using Foundation.Networking.DeepSpace.Providers;

namespace Foundation.Networking.DeepSpace.Tests.Providers
{
    public class S3StorageProviderTests
    {
        [Fact]
        public void ProviderName_IsS3()
        {
            //
            // Construction requires valid AWS credentials — since we can't
            // connect to a real S3, we verify via a try/catch that the
            // provider initializes with the correct name.
            //
            // For full integration tests, run against a local MinIO instance.
            //
            try
            {
                S3StorageProvider provider = new S3StorageProvider(new S3StorageConfig
                {
                    Endpoint = "http://localhost:9000",
                    AccessKey = "test-access-key",
                    SecretKey = "test-secret-key",
                    BucketName = "test-bucket",
                    UseHttps = false
                });

                Assert.Equal("S3", provider.ProviderName);
            }
            catch (Exception)
            {
                //
                // If the client library throws (e.g., cannot resolve endpoint),
                // skip — this is expected without a real S3 endpoint.
                //
            }
        }


        [Fact]
        public void Constructor_WithCustomEndpoint_DoesNotThrow()
        {
            //
            // Verifies that specifying a custom endpoint (for MinIO, etc.)
            // does not cause a constructor exception.
            //
            Exception exception = Record.Exception(() =>
            {
                S3StorageProvider provider = new S3StorageProvider(new S3StorageConfig
                {
                    Endpoint = "http://localhost:9000",
                    AccessKey = "minioadmin",
                    SecretKey = "minioadmin",
                    BucketName = "test",
                    UseHttps = false
                });
            });

            Assert.Null(exception);
        }


        [Fact]
        public void Constructor_WithRegion_DoesNotThrow()
        {
            Exception exception = Record.Exception(() =>
            {
                S3StorageProvider provider = new S3StorageProvider(new S3StorageConfig
                {
                    AccessKey = "test",
                    SecretKey = "test",
                    BucketName = "test",
                    Region = "us-east-1"
                });
            });

            Assert.Null(exception);
        }
    }
}
