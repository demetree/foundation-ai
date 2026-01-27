using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Foundation.Concurrent;


namespace Foundation.Services
{
    /// <summary>
    /// 
    /// This interacts with a internet based tile service providers and abstracts their interface with a method for getting satellite tiles, and another for
    /// non satellite tiles.
    /// 
    /// It caches the results to avoid repeated requests for the same information.
    /// 
    /// </summary>
    public class TileManagementService : IDisposable
    {
        private const double HTTP_CLIENT_TIMEOUT_SECONDS = 10;          // This should be plenty of time to get data back from the source

        private const int GOOGLE_REQUEST_SLEEP_MILLISECONDS = 50;
        private const int OSM_REQUEST_SLEEP_MILLISECONDS = 100;

        private const int MAX_ZOOM = 25;                    // This is the maximum zoom level returned by this service.  Upscaling will be done if the provider doesn't support it natively

        private const int OSM_MAX_ZOOM = 19;
        private const int GOOGLE_MAX_ZOOM = 21;             // 21 seems to be the max consistent zoom level for Google satellite imagery

        private const int TILE_SIZE = 256;                  // Standard tile size

        private const string USER_AGENT = "FoundationTileProxy/1.0 (https://www.k2research.ca; info@k2research.ca)";
        private const string REFERRER = "https://www.k2research.ca";

        private bool _enableImageCompression = false;         // PNG files are already compressed, so recompressing them provides minimal benefit.  Default is off.
        private bool _useNoTileImageOnError = true;           // If true, then when a tile request fails, it will return a no tile image instead of an error code.  Default is true.

        public bool CompressionEnabled { get { return _enableImageCompression; } }

        private readonly ILogger<TileManagementService> _logger;
        private readonly HttpClient _httpClient;

        private string _cacheDirectory;
        private readonly string _googleApiKey;
        private readonly bool _allowCacheClearing;

        private bool _drawDebugMarkersOnImages;
        public bool DrawDebugMarkersOnImages
        {
            get { return _drawDebugMarkersOnImages; }
            set { _drawDebugMarkersOnImages = value; }
        }


        private ConcurrentDictionary<string, DateTime> _lastOSMRequestPerUrl = new ConcurrentDictionary<string, DateTime>();
        private ConcurrentDictionary<string, DateTime> _lastGoogleRequestPerUrl = new ConcurrentDictionary<string, DateTime>();


        // To prevent reentrancy to tile requests - this will allow us to limit requests at the entrance to the function, and management of the caches
        private readonly SemaphoreSlim _satelliteTileCacheSemaphore = new SemaphoreSlim(6, 6);      // Double OSM given the assumption that google is more gracious with multiple requests
        private readonly SemaphoreSlim _nonSatelliteTileCacheSemaphore = new SemaphoreSlim(3, 3);   // Allow 3 concurrent non-satellite tile requests.  This will let us hit each of the 3 OSM servers at the same time.   - they will round robin through the 3 OSM server


        // to limit concurrent requests to actual tile sources - guarded by the reentrancy semaphore as well, but keeping here for future change flexibility, and intermixing of requests to actual providers
        private static readonly SemaphoreSlim _googleSemaphore = new SemaphoreSlim(6, 6);
        private static readonly SemaphoreSlim _osmSemaphore = new SemaphoreSlim(3, 3);

        // 12 hour caches in memory with sliding expiration
        private static readonly ExpiringCache<string, byte[]> _satelliteTileMemoryCache = new ExpiringCache<string, byte[]>(43200, true, false);
        private static readonly ExpiringCache<string, byte[]> _nonSatelliteTileMemoryCache = new ExpiringCache<string, byte[]>(43200, true, false);

        // Per-file locks to prevent concurrent read/write conflicts on the same cache file
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _fileLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

        private static readonly Random _random = new Random();

        private static readonly string[] _osmSubdomains = { "a", "b", "c" };
        private static int _osmIndex = 0;

        private static readonly string[] _googleSubdomains = { "mt0", "mt1", "mt2", "mt3" };
        private static int _googleIndex = 0;

        private readonly string _resampler = "Lanczos3";     // NearestNeighbor or Lanczos3;

        public record PNGWithEnvelope
        {
            public double minX { get; init; }               // bottom left X in GPS degrees
            public double minY { get; init; }               // bottom left Y in GPS degrees
            public double width { get; init; }              // width in in GPS degrees
            public double height { get; init; }             // height in GPS degrees
            public byte[] pngData { get; init; }            // png data bytes


            public string resampler = "box";      // name of the resampler to user for the image resizing - box, lanczos3, nearestneighbor etc..
            public float opacity { get; init; } = 1.0f; // Opacity (0.0f to 1.0f)

            public PNGWithEnvelope(double minX, double minY, double width, double height, byte[] pngData, float opacity = 1.0f, string resampler = "box")
            {
                this.minX = minX;
                this.minY = minY;
                this.width = width;
                this.height = height;
                this.pngData = pngData;
                this.opacity = Math.Clamp(opacity, 0.0f, 1.0f); // Ensure opacity is within valid range
                this.resampler = resampler;
            }
        }


        public record GeoJsonWithInstructions
        {
            public JsonDocument GeoJsonData { get; init; }    // GeoJson data string
            public float lineWidth { get; init; }       // line width in pixels
            public Color color { get; init; }           // line color in hex format #RRGGBB or #RRGGBBAA


            public GeoJsonWithInstructions(string geoJsonData, float lineWidth = 1.0f, string colorInHex = "#FFFFFF", float opacity = 1f)
            {
                JsonDocument geoJsonToOverlay = JsonDocument.Parse(geoJsonData);

                this.GeoJsonData = geoJsonToOverlay;

                // Make sure line width is positive an greater than or equal to 1f

                if (lineWidth < 1.0f)
                {
                    this.lineWidth = 1.0f;
                }
                else
                {
                    this.lineWidth = lineWidth;
                }

                // Make convert passed in color to a SixLabour.ImageSharp.Color object for later use
                this.color = Color.ParseHex(colorInHex);

                this.color = this.color.WithAlpha(opacity);
            }
        }


        public record ColorAndGradientsWithPosition
        {
            public enum LegendPosition
            {
                Left = 0,
                Right = 1,
                Top = 2,
                Bottom = 3,
            }

            public LegendPosition legendPosition { get; set; }

            public List<string> colorGradients { get; set; }

            public double targetValue { get; set; }

            public double minTargetValue { get; set; }

            public string imageMode { get; set; }

        }


        /// <summary>
        /// Response for a centered image request, containing the image data.
        /// </summary>
        public struct CenteredImageResult
        {
            public byte[] ImageData;
            public string ErrorMessage;
            public int? ErrorStatusCode;

            public CenteredImageResult(byte[] imageData, string errorMessage, int? errorStatusCode)
            {
                ImageData = imageData;
                ErrorMessage = errorMessage;
                ErrorStatusCode = errorStatusCode;
            }
        }


        /// <summary>
        /// Response for a fitted image request, containing the image data.
        /// </summary>
        public struct FittedImageResult
        {
            public byte[] ImageData;
            public string ErrorMessage;
            public int? ErrorStatusCode;
            public int SelectedZoom;

            public FittedImageResult(byte[] imageData, string errorMessage, int? errorStatusCode, int selectedZoom = 0)
            {
                ImageData = imageData;
                ErrorMessage = errorMessage;
                ErrorStatusCode = errorStatusCode;
                SelectedZoom = selectedZoom;
            }
        }


        /// <summary>
        /// 
        /// This is the response to a tile request.
        /// 
        /// Successful requests will have tile data, an eTag, and no error status code.
        /// 
        /// Error requests will have the HTTP error status code, and their description set.
        /// 
        /// </summary>
        public struct TileResponse
        {
            public byte[] TileData;
            public string ETag;
            public int? ErrorStatusCode;
            public string ErrorMessage;
            public bool AllowClientSideCaching;

            public TileResponse(byte[] tileData, string eTag, int? errorStatusCode, string errorMessage, bool allowClientSideCaching)
            {
                TileData = tileData;
                ETag = eTag;
                ErrorStatusCode = errorStatusCode;
                ErrorMessage = errorMessage;
                this.AllowClientSideCaching = allowClientSideCaching;
            }
        }


        /// <summary>
        /// 
        /// This is the response to a tile request,  It has compressed data, the compression used if any, the length of it, and the compression algorithm used.
        /// 
        /// </summary>
        public struct CompressedDataResult
        {
            public byte[] Data;
            public string ContentEncoding;
            public int ContentLength;

            public CompressedDataResult(byte[] compressedData, string contentEncoding, int contentLength)
            {
                Data = compressedData;
                ContentEncoding = contentEncoding;
                ContentLength = contentLength;
            }
        }


        public TileManagementService()
        {
            _logger = null;

            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(HTTP_CLIENT_TIMEOUT_SECONDS);      // Allow 1 seconds to try and get the tile data.  If services are up, this should be quite possible.

            _allowCacheClearing = false;

            _googleApiKey = null;

            EstablishCacheDirectory();
        }


        public TileManagementService(ILogger<TileManagementService> logger)
        {
            _logger = logger;

            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(HTTP_CLIENT_TIMEOUT_SECONDS);      // Allow 1 seconds to try and get the tile data.  If services are up, this should be quite possible.

            _googleApiKey = null;

            _allowCacheClearing = false;

            EstablishCacheDirectory();

        }


        public TileManagementService(ILogger<TileManagementService> logger,
                                     IConfiguration configuration)
        {
            _logger = logger;

            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(HTTP_CLIENT_TIMEOUT_SECONDS);      // Allow 1 seconds to try and get the tile data.  If services are up, this should be quite possible.

            _googleApiKey = configuration["GoogleMaps:ApiKey"];

            _resampler = configuration.GetValue<string>("Settings:UpscaleResampler", "Lanczos3");


            string allowCacheClearSetting = configuration["Settings:AllowTileCacheClearing"];

            if (bool.TryParse(allowCacheClearSetting, out bool result) == true)
            {
                _allowCacheClearing = result;
            }
            else
            {
                _allowCacheClearing = false;
            }

            EstablishCacheDirectory();
        }


        //
        // Dependency injection construction
        //
        public TileManagementService(IHttpClientFactory httpClientFactory,
                                     ILogger<TileManagementService> logger,
                                     IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(HTTP_CLIENT_TIMEOUT_SECONDS);      // Allow 1 seconds to try and get the tile data.  If services are up, this should be quite possible.

            _googleApiKey = configuration["GoogleMaps:ApiKey"];

            string allowCacheClearSetting = configuration["Settings:AllowTileCacheClearing"];

            if (bool.TryParse(allowCacheClearSetting, out bool result) == true)
            {
                _allowCacheClearing = result;
            }
            else
            {
                _allowCacheClearing = false;
            }

            EstablishCacheDirectory();
        }


        private void EstablishCacheDirectory()
        {
            //
            // Try to create and use a 'MapTiles' directory in the current application directory.  If that doesn't work, then fall back to the system temp folder.
            //
            try
            {
                // start off by setting the default log information
                string currentPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "";

                _cacheDirectory = System.IO.Path.Combine(currentPath, "MapTiles");

                Directory.CreateDirectory(_cacheDirectory);
            }
            catch (Exception)
            {
                _cacheDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MapTiles");
                Directory.CreateDirectory(_cacheDirectory);
            }
        }


        /// <summary>
        /// Creates an image centered on the provided latitude and longitude with specified width, height, and zoom level.
        /// 
        /// Overlays optional PNG images with geospatial envelopes if provided.
        /// 
        /// </summary>
        /// <param name="latitude">Latitude in degrees (-90 to 90).</param>
        /// <param name="longitude">Longitude in degrees (-180 to 180).</param>
        /// <param name="width">Desired image width in pixels.</param>
        /// <param name="height">Desired image height in pixels.</param>
        /// <param name="zoom">Zoom level (0 to MAX_ZOOM).</param>
        /// <param name="isSatellite">True for satellite imagery, false for non-satellite (OSM).</param>
        /// <param name="imagesToOverlay">Optional list of PNG images with geospatial envelopes to overlay.  List order defines the Z Order.</param>
        /// <returns>A CenteredImageResult containing the image data or error details.</returns>
        public async Task<CenteredImageResult> GetCenteredImageAsync(double latitude,
                                                                    double longitude,
                                                                    int width,
                                                                    int height,
                                                                    int zoom,
                                                                    bool isSatellite,
                                                                    double opacity = 1,
                                                                    List<PNGWithEnvelope> imagesToOverlay = null,
                                                                    List<GeoJsonWithInstructions> geoJsonsToOverlay = null)
        {
            // Validate inputs
            if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
            {
                _logger?.LogWarning($"Invalid lat/lon: latitude={latitude}, longitude={longitude}");
                return new CenteredImageResult(null, "Invalid latitude or longitude.", 400);
            }

            if (width <= 0 || height <= 0 ||
                width > 10000 || height > 10000)
            {
                _logger?.LogWarning($"Invalid dimensions: width={width}, height={height}");
                return new CenteredImageResult(null, "Invalid width or height.", 400);
            }

            if (opacity < 0.0f || opacity > 1.0f)
            {
                _logger?.LogWarning($"Invalid opacity: {opacity}");
                return new CenteredImageResult(null, "Image opacity must be between 0.0 and 1.0.", 400);
            }

            if (!IsValidTileParameters(zoom, 0, 0, out string errorMessage))
            {
                _logger?.LogWarning($"Invalid zoom: {zoom}");
                return new CenteredImageResult(null, errorMessage, 400);
            }

            // Validate overlay images
            if (imagesToOverlay != null)
            {
                foreach (PNGWithEnvelope overlay in imagesToOverlay)
                {
                    if (overlay == null || overlay.pngData == null || overlay.pngData.Length == 0)
                    {
                        _logger?.LogWarning("Invalid overlay image: null or empty PNG data.");
                        return new CenteredImageResult(null, "Invalid overlay image data.", 400);
                    }

                    if (overlay.minX < -180 || overlay.minX > 180 || overlay.minY < -90 || overlay.minY > 90 ||
                        overlay.width <= 0 || overlay.height <= 0)
                    {
                        _logger?.LogWarning($"Invalid overlay envelope: minX={overlay.minX}, minY={overlay.minY}, width={overlay.width}, height={overlay.height}");
                        return new CenteredImageResult(null, "Invalid overlay envelope coordinates or dimensions.", 400);
                    }

                    if (overlay.opacity < 0.0f || overlay.opacity > 1.0f)
                    {
                        _logger?.LogWarning($"Invalid overlay opacity: {overlay.opacity}");
                        return new CenteredImageResult(null, "Overlay opacity must be between 0.0 and 1.0.", 400);
                    }
                }
            }

            // Validate overlay geojson
            if (geoJsonsToOverlay != null && geoJsonsToOverlay.Count >= 1)
            {
                foreach (GeoJsonWithInstructions overlay in geoJsonsToOverlay)
                {
                    if (overlay == null || overlay.GeoJsonData == null)
                    {
                        _logger?.LogWarning("Invalid overlay GeoJson: null or empty GeoJson data.");
                        return new CenteredImageResult(null, "Invalid overlay GeoJson data.", 400);
                    }
                }
            }

            try
            {
                // Convert lat/lon to tile coordinates (Mercator projection)
                double tileCount = 1 << (int)zoom; // 2^zoom
                double lonRad = longitude * Math.PI / 180;
                double latRad = latitude * Math.PI / 180;
                double xTile = (longitude + 180) / 360 * tileCount;
                double yTile = (1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2 * tileCount;

                // Calculate tile bounds for the requested image size
                int maxTiles = (int)Math.Min(tileCount, 32); // Cap tiles to prevent excessive fetching
                int tilesAcross = Math.Min((int)Math.Ceiling((double)width / TILE_SIZE + 2), maxTiles); // Add buffer
                int tilesDown = Math.Min((int)Math.Ceiling((double)height / TILE_SIZE + 2), maxTiles);
                int xMin = (int)Math.Floor(xTile - tilesAcross / 2.0);
                int xMax = xMin + tilesAcross - 1;
                int yMin = (int)Math.Floor(yTile - tilesDown / 2.0);
                int yMax = yMin + tilesDown - 1;

                // Validate tile coordinates
                int maxTile = (int)tileCount;
                if (xMin < 0 || xMax >= maxTile || yMin < 0 || yMax >= maxTile)
                {
                    _logger?.LogWarning($"Tile bounds out of range: xMin={xMin}, xMax={xMax}, yMin={yMin}, yMax={yMax}, maxTile={maxTile}");
                    return new CenteredImageResult(null, "Requested area exceeds tile bounds.", 400);
                }


                // Create composite image
                using var compositeImage = new Image<Rgba32>((xMax - xMin + 1) * TILE_SIZE, (yMax - yMin + 1) * TILE_SIZE);

                // Log composite size
                System.Diagnostics.Debug.WriteLine($"Composite image size: {compositeImage.Width}x{compositeImage.Height}");


                // Fetch and place tiles
                for (int x = xMin; x <= xMax; x++)
                {
                    for (int y = yMin; y <= yMax; y++)
                    {
                        TileResponse tileResponse = isSatellite
                            ? await GetSatelliteTileAsync(zoom, x, y).ConfigureAwait(false)
                            : await GetNonSatelliteTileAsync(zoom, x, y).ConfigureAwait(false);

                        if (tileResponse.TileData == null || tileResponse.ErrorStatusCode.HasValue)
                        {
                            _logger?.LogWarning($"Failed to fetch tile z={zoom}, x={x}, y={y}: {tileResponse.ErrorMessage}");
                            return new CenteredImageResult(null, $"Failed to fetch tile: {tileResponse.ErrorMessage}", tileResponse.ErrorStatusCode ?? 500);
                        }

                        using var tileImage = Image.Load<Rgba32>(tileResponse.TileData);
                        compositeImage.Mutate(ctx => ctx.DrawImage(tileImage, new Point((x - xMin) * TILE_SIZE, (y - yMin) * TILE_SIZE), (float)opacity));
                    }
                }

                // Calculate center pixel for reference
                double centerXPixel = (xTile - xMin) * TILE_SIZE;
                double centerYPixel = (yTile - yMin) * TILE_SIZE;

                // Overlay PNG images if provided
                if (imagesToOverlay != null && imagesToOverlay.Count > 0)
                {
                    AddOverlayImagesToCompositeImage(imagesToOverlay, tileCount, TILE_SIZE, xMin, yMin, compositeImage, centerXPixel, centerYPixel);
                }

                // Overlay GeoJSON if provided
                if (geoJsonsToOverlay != null)
                {
                    AddOverlayGeoJsonToCompositeImage(geoJsonsToOverlay, tileCount, TILE_SIZE, xMin, yMin, compositeImage, centerXPixel, centerYPixel);
                }
                if (_drawDebugMarkersOnImages == true)
                {
                    // Draw red 5x5 pixel square at center for debugging
                    int centerX = (int)Math.Round(centerXPixel);
                    int centerY = (int)Math.Round(centerYPixel);
                    const int markerSize = 5; // 5x5 pixel square
                    for (int dy = -markerSize / 2; dy <= markerSize / 2; dy++)
                    {
                        for (int dx = -markerSize / 2; dx <= markerSize / 2; dx++)
                        {
                            int px = centerX + dx;
                            int py = centerY + dy;

                            if (px >= 0 && px < compositeImage.Width && py >= 0 && py < compositeImage.Height)
                            {
                                compositeImage[px, py] = new Rgba32(255, 0, 0, 255); // Red, fully opaque
                            }
                        }
                    }
                }

                // Log center for verification
                System.Diagnostics.Debug.WriteLine($"Center pixel in composite: ({centerXPixel}, {centerYPixel})");

                // Crop and resize, ensuring center alignment
                int cropX = (int)Math.Round(centerXPixel - width / 2.0);
                int cropY = (int)Math.Round(centerYPixel - height / 2.0);
                int cropWidth = width;
                int cropHeight = height;

                // Adjust crop to fit composite bounds while preserving center
                if (cropX < 0)
                {
                    cropWidth += cropX; // Reduce width
                    cropX = 0;
                }
                else if (cropX + width > compositeImage.Width)
                {
                    cropWidth = compositeImage.Width - cropX;
                }

                if (cropY < 0)
                {
                    cropHeight += cropY; // Reduce height
                    cropY = 0;
                }
                else if (cropY + height > compositeImage.Height)
                {
                    cropHeight = compositeImage.Height - cropY;
                }

                if (cropWidth <= 0 || cropHeight <= 0)
                {
                    _logger?.LogWarning($"Invalid crop dimensions: cropWidth={cropWidth}, cropHeight={cropHeight}");
                    return new CenteredImageResult(null, "Invalid crop dimensions.", 400);
                }

                // Log if crop is adjusted
                if (cropWidth < width || cropHeight < height)
                {
                    _logger?.LogWarning($"Crop adjusted due to composite bounds: requested {width}x{height}, got {cropWidth}x{cropHeight}");
                }

                // Log crop center for verification
                System.Diagnostics.Debug.WriteLine($"Crop center: ({cropX + cropWidth / 2.0}, {cropY + cropHeight / 2.0})");

                using var croppedImage = (cropX + cropWidth < compositeImage.Width && cropY + cropHeight < compositeImage.Height) ?
                                                compositeImage.Clone(ctx => ctx.Crop(new Rectangle(cropX, cropY, cropWidth, cropHeight))) :     // OK to crop
                                                compositeImage.Clone();                                                                         // crop will fail

                // Create resampler for final resize
                IResampler resampler = _resampler.ToLower() switch
                {
                    "box" => KnownResamplers.Box,
                    "lanczos3" => KnownResamplers.Lanczos3,
                    "nearestneighbor" => KnownResamplers.NearestNeighbor,
                    _ => KnownResamplers.Lanczos3
                };

                using Image<Rgba32> resizedImage = croppedImage.Clone(ctx => ctx.Resize(width, height, resampler));

                using MemoryStream outputStream = new MemoryStream();

                resizedImage.SaveAsPng(outputStream);
                byte[] imageData = outputStream.ToArray();

                _logger?.LogInformation($"Generated centered image for lat={latitude}, lon={longitude}, z={zoom}, {width}x{height}, satellite={isSatellite}");

                return new CenteredImageResult(imageData, null, null);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error generating centered image for lat={latitude}, lon={longitude}, z={zoom}: {ex.Message}");
                return new CenteredImageResult(null, "Failed to generate image.", 500);
            }
        }

        private void AddOverlayImagesToCompositeImage(List<PNGWithEnvelope> imagesToOverlay,
                                                      double tileCount,
                                                      int tileSize,
                                                      int xMin,
                                                      int yMin,
                                                      Image<Rgba32> compositeImage,
                                                      double centerXPixel,
                                                      double centerYPixel)
        {
            foreach (PNGWithEnvelope overlay in imagesToOverlay)
            {
                System.Diagnostics.Debug.WriteLine($"Processing overlay with minX of {overlay.minX} and minY of {overlay.minY} and width of {overlay.width} and height of {overlay.height}.");

                try
                {
                    // Convert overlay envelope coordinates to pixel coordinates in composite image
                    double worldSize = tileSize * tileCount;

                    // X-coordinates (longitude)
                    double overlayMinXPixel = (overlay.minX + 180) / 360 * worldSize - xMin * tileSize;
                    double overlayMaxXPixel = ((overlay.minX + overlay.width) + 180) / 360 * worldSize - xMin * tileSize;

                    // Y-coordinates (latitude, assuming minY is bottom latitude)
                    double overlayBottomLatRad = overlay.minY * Math.PI / 180; // Bottom latitude (minY)
                    double overlayTopLatRad = (overlay.minY + overlay.height) * Math.PI / 180; // Top latitude (minY + height)
                    double overlayTopYPixel = (1 - Math.Log(Math.Tan(overlayTopLatRad) + 1 / Math.Cos(overlayTopLatRad)) / Math.PI) / 2 * worldSize - yMin * tileSize;
                    double overlayBottomYPixel = (1 - Math.Log(Math.Tan(overlayBottomLatRad) + 1 / Math.Cos(overlayBottomLatRad)) / Math.PI) / 2 * worldSize - yMin * tileSize;

                    // Calculate overlay dimensions in pixels
                    int overlayWidth = (int)Math.Round(overlayMaxXPixel - overlayMinXPixel);
                    int overlayHeight = (int)Math.Round(overlayBottomYPixel - overlayTopYPixel); // Bottom - Top (y increases downward)

                    // Log overlay pixel coordinates for debugging
                    System.Diagnostics.Debug.WriteLine($"Overlay pixel bounds: minXPixel={overlayMinXPixel}, topYPixel={overlayTopYPixel}, maxXPixel={overlayMaxXPixel}, bottomYPixel={overlayBottomYPixel}, width={overlayWidth}, height={overlayHeight}");

                    if (overlayWidth <= 0 || overlayHeight <= 0)
                    {
                        _logger?.LogWarning($"Invalid overlay dimensions: width={overlayWidth}, height={overlayHeight} for envelope minX={overlay.minX}, minY={overlay.minY}");
                        continue; // Skip invalid overlay
                    }

                    // Check if overlay is within composite bounds
                    if (overlayMinXPixel >= compositeImage.Width || overlayMaxXPixel < 0 ||
                        overlayTopYPixel >= compositeImage.Height || overlayBottomYPixel < 0)
                    {
                        _logger?.LogDebug($"Overlay outside composite bounds: minXPixel={overlayMinXPixel}, topYPixel={overlayTopYPixel}, maxXPixel={overlayMaxXPixel}, bottomYPixel={overlayBottomYPixel}");
                        continue; // Skip overlay outside bounds
                    }

                    // Verify overlay center alignment
                    double overlayCenterLat = overlay.minY + overlay.height / 2; // Center latitude (minY is bottom)
                    double overlayCenterLon = overlay.minX + overlay.width / 2;
                    double overlayCenterLatRad = overlayCenterLat * Math.PI / 180;
                    double overlayCenterXTile = (overlayCenterLon + 180) / 360 * tileCount;
                    double overlayCenterYTile = (1 - Math.Log(Math.Tan(overlayCenterLatRad) + 1 / Math.Cos(overlayCenterLatRad)) / Math.PI) / 2 * tileCount;
                    double overlayCenterXPixel = (overlayCenterXTile - xMin) * tileSize;
                    double overlayCenterYPixel = (overlayCenterYTile - yMin) * tileSize;

                    System.Diagnostics.Debug.WriteLine($"Overlay center pixel: ({overlayCenterXPixel}, {overlayCenterYPixel}), Map center pixel: ({centerXPixel}, {centerYPixel})");

                    // Load overlay image
                    using var overlayImage = Image.Load<Rgba32>(overlay.pngData);


                    // Create resampler for the image overlay.  Each overlay can specify its own resampler.  Defaults to box.
                    if (string.IsNullOrEmpty(overlay.resampler))
                    {
                        overlay.resampler = "box"; // Default
                    }

                    IResampler overlayResampler = overlay.resampler.ToLower() switch
                    {
                        "box" => KnownResamplers.Box,
                        "lanczos3" => KnownResamplers.Lanczos3,
                        "nearestneighbor" => KnownResamplers.NearestNeighbor,
                        _ => KnownResamplers.Lanczos3
                    };


                    using Image<Rgba32> resizedOverlay = overlayImage.Clone(ctx => ctx.Resize(overlayWidth, overlayHeight, overlayResampler));

                    // Calculate position in composite image
                    int posX = (int)Math.Round(overlayMinXPixel);
                    int posY = (int)Math.Round(overlayTopYPixel); // Top y (smaller pixel value)

                    // Adjust for partial overlap
                    int drawWidth = overlayWidth;
                    int drawHeight = overlayHeight;
                    int drawX = posX;
                    int drawY = posY;

                    if (posX < 0)
                    {
                        drawWidth += posX;
                        drawX = 0;
                    }
                    else if (posX + overlayWidth > compositeImage.Width)
                    {
                        drawWidth = compositeImage.Width - posX;
                    }

                    if (posY < 0)
                    {
                        drawHeight += posY;
                        drawY = 0;
                    }
                    else if (posY + overlayHeight > compositeImage.Height)
                    {
                        drawHeight = compositeImage.Height - posY;
                    }

                    if (drawWidth <= 0 || drawHeight <= 0)
                    {
                        _logger?.LogDebug($"Overlay crop dimensions invalid: drawWidth={drawWidth}, drawHeight={drawHeight}");
                        continue;
                    }

                    // Crop overlay if necessary
                    Image<Rgba32> croppedOverlay = resizedOverlay;
                    if (drawWidth < overlayWidth || drawHeight < overlayHeight)
                    {
                        int cropXOverlay = posX < 0 ? -posX : 0;
                        int cropYOverlay = posY < 0 ? -posY : 0;
                        croppedOverlay = resizedOverlay.Clone(ctx => ctx.Crop(new Rectangle(cropXOverlay, cropYOverlay, drawWidth, drawHeight)));
                    }

                    // Draw overlay on composite image with specified opacity
                    compositeImage.Mutate(ctx => ctx.DrawImage(croppedOverlay, new Point(drawX, drawY), overlay.opacity));


                    // Add debug border for overlay
                    if (_drawDebugMarkersOnImages == true)
                    {
                        int traceXMin = (int)Math.Round(overlayMinXPixel);
                        int traceXMax = (int)Math.Round(overlayMaxXPixel);
                        int traceYMin = (int)Math.Round(overlayTopYPixel);
                        int traceYMax = (int)Math.Round(overlayBottomYPixel);
                        int traceThickness = 3;

                        Rgba32 borderColor = new Rgba32(0, 255, 0, 255); // Green
                        for (int x = traceXMin; x <= traceXMax; x++)
                        {
                            for (int dy = 0; dy < traceThickness; dy++)
                            {
                                if (x >= 0 && x < compositeImage.Width)
                                {
                                    if (traceYMin + dy >= 0 && traceYMin + dy < compositeImage.Height)
                                    {
                                        compositeImage[x, traceYMin + dy] = borderColor;
                                    }

                                    if (traceYMax - dy >= 0 && traceYMax - dy < compositeImage.Height)
                                    {
                                        compositeImage[x, traceYMax - dy] = borderColor;
                                    }
                                }
                            }
                        }
                        for (int y = traceYMin; y <= traceYMax; y++)
                        {
                            for (int dx = 0; dx < traceThickness; dx++)
                            {
                                if (y >= 0 && y < compositeImage.Height)
                                {
                                    if (traceXMin + dx >= 0 && traceXMin + dx < compositeImage.Width)
                                    {
                                        compositeImage[traceXMin + dx, y] = borderColor;
                                    }

                                    if (traceXMax - dx >= 0 && traceXMax - dx < compositeImage.Width)
                                    {
                                        compositeImage[traceXMax - dx, y] = borderColor;
                                    }
                                }
                            }
                        }
                    }


                    _logger?.LogDebug($"Overlay applied: minX={overlay.minX}, minY={overlay.minY}, width={overlayWidth}px, height={overlayHeight}px at posX={drawX}, posY={drawY}, opacity={overlay.opacity}");
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning($"Failed to apply overlay for envelope minX={overlay.minX}, minY={overlay.minY}: {ex.Message}");
                    continue; // Skip failed overlay, continue with others
                }
            }
        }


        private void AddOverlayGeoJsonToCompositeImage(List<GeoJsonWithInstructions> geoJsonsToOverlay,
                                                      double tileCount,
                                                      int tileSize,
                                                      int xMin,
                                                      int yMin,
                                                      Image<Rgba32> compositeImage,
                                                      double centerXPixel,
                                                      double centerYPixel)
        {
            try
            {

                foreach (GeoJsonWithInstructions overlay in geoJsonsToOverlay)
                {
                    JsonElement root = overlay.GeoJsonData.RootElement;

                    if (root.GetProperty("type").GetString() == "FeatureCollection")
                    {
                        foreach (var feature in root.GetProperty("features").EnumerateArray())
                        {
                            var geometry = feature.GetProperty("geometry");
                            var properties = feature.GetProperty("properties");

                            compositeImage.Mutate(ctx =>
                            {
                                DrawGeometry(geometry, ctx, overlay.color, overlay.lineWidth, xMin, yMin, tileSize, tileCount, properties);

                            });
                        }
                    }
                    else if (root.GetProperty("type").GetString() == "Feature")
                    {
                        var geometry = root.GetProperty("geometry");
                        var properties = root.GetProperty("properties");
                        compositeImage.Mutate(ctx =>
                        {
                            DrawGeometry(geometry, ctx, overlay.color, overlay.lineWidth, xMin, yMin, tileSize, tileCount, properties);
                        });
                    }
                    else if (root.GetProperty("type").GetString() == "GeometryCollection")
                    {
                        foreach (var geometry in root.GetProperty("geometries").EnumerateArray())
                        {
                            compositeImage.Mutate(ctx =>
                            {
                                DrawGeometry(geometry, ctx, overlay.color, overlay.lineWidth, xMin, yMin, tileSize, tileCount);
                            });
                        }
                    }
                    else
                    {
                        // Geometry-only GeoJSON
                        compositeImage.Mutate(ctx =>
                        {
                            DrawGeometry(root, ctx, overlay.color, overlay.lineWidth, xMin, yMin, tileSize, tileCount);
                        });
                    }

                }

            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Failed to apply overlay {ex.Message}");
            }

        }

        /// <summary>
        /// Helper function to project lon/lat to pixel coordinates in the composite image.
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <param name="tileSize"></param>
        /// <param name="tileCount"></param>
        /// <param name="xMin"></param>
        /// <param name="yMin"></param>
        /// <returns></returns>
        private PointF ProjectToPixel(double lon, double lat, int tileSize, double tileCount, int xMin, int yMin)
        {
            double worldSize = tileSize * tileCount;
            double x = (lon + 180) / 360 * worldSize - xMin * tileSize;
            double y = (1 - Math.Log(Math.Tan(Math.PI * lat / 180) + 1 / Math.Cos(Math.PI * lat / 180)) / Math.PI) / 2 * worldSize - yMin * TILE_SIZE;
            return new PointF((float)x, (float)y);
        }

        /// <summary>
        /// This function takes in "geometry" as defined by the https://geojson.org/ and https://datatracker.ietf.org/doc/html/rfc7946
        /// to draw on top of the provided composite image, with the provided color and lineWidth 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="ctx"></param>
        /// <param name="color"></param>
        /// <param name="lineWidth"></param>
        /// <param name="xMin"></param>
        /// <param name="yMin"></param>
        /// <param name="tileSize"></param>
        /// <param name="tileCount"></param>
        private void DrawGeometry(JsonElement geometry,
                                IImageProcessingContext ctx,
                                Color color, float lineWidth,
                                int xMin, int yMin,
                                int tileSize, double tileCount,
                                JsonElement properties = default)
        {
            var type = geometry.GetProperty("type").GetString();

            switch (type)
            {
                //
                // Wrap every GeoJson feature into it's try catch block and made it fail gracefully so that it deos not cascade into
                // failing other features as well
                //
                case "Point":
                    {
                        try
                        {
                            var label = properties.GetProperty("label").ToString();
                            var pinColor = properties.GetProperty("color").ToString();
                            var coords = geometry.GetProperty("coordinates").EnumerateArray().ToArray();
                            double lon = coords[0].GetDouble();
                            double lat = coords[1].GetDouble();
                            var pt = ProjectToPixel(lon, lat, tileSize, tileCount, xMin, yMin);

                            // Decide on a font size
                            // Currently only supporting upto 3 digits per Point, because cannot think of a label that will higher than 999 and only digits are expected to be in the label
                            // because right now putting anything other than numbers will not be making sense, as the idea is to have a accompanying table that describes these labels
                            // for future implementations we can add "types" to the properties object in the GeoJson.
                            // Example:
                            // 1) As of right now the property object looks like:
                            // properties:{
                            // label: "1"
                            //  }
                            // 2) Future implementation:
                            //  properties: {
                            //  label: {
                            //              type: "Digit Marker"
                            //              value: "1"
                            //          }
                            //  }
                            // OR
                            // 2) Future implementation:
                            //  properties: {
                            //  label: {
                            //              type: "String Maker"
                            //              value: "abc"
                            //          }
                            //  }

                            Font font;

                            int countOfDigits = Regex.Matches(label, @"\d").Count;

                            switch (countOfDigits)
                            {

                                case 1:
                                    {
                                        font = SystemFonts.CreateFont("Arial", 16, FontStyle.Bold);
                                        break;
                                    }

                                case 2:
                                    {
                                        font = SystemFonts.CreateFont("Arial", 14, FontStyle.Bold);
                                        break;
                                    }

                                case 3:
                                    {
                                        font = SystemFonts.CreateFont("Arial", 14, FontStyle.Bold);
                                        break;
                                    }

                                default:
                                    {
                                        font = SystemFonts.CreateFont("Arial", 12, FontStyle.Bold);
                                        break;
                                    }

                            }

                            var textSize = TextMeasurer.MeasureSize(label, new TextOptions(font));

                            // Construct a path for the pin shape

                            var pinShape = new EllipsePolygon(pt, 14);

                            if (string.IsNullOrEmpty(pinColor) == false) // choose color provided by the geojson or use the default color
                            {
                                ctx.Fill(Color.ParseHex(pinColor), pinShape);
                            }
                            else
                            {
                                ctx.Fill(color, pinShape);
                            }

                            ctx.Draw(Color.White, 4, pinShape);

                            ctx.DrawText(label, font, Color.White, new PointF(pt.X - 1 - textSize.Width / 2, pt.Y - textSize.Height / 2));

                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning($"Failed to apply overlay {ex.Message}");
                            break;
                        }
                    }
                case "MultiPoint":
                    {
                        try
                        {
                            foreach (var coord in geometry.GetProperty("coordinates").EnumerateArray())
                            {
                                var arr = coord.EnumerateArray().ToArray();
                                double lon = arr[0].GetDouble();
                                double lat = arr[1].GetDouble();
                                var pt = ProjectToPixel(lon, lat, tileSize, tileCount, xMin, yMin);
                                var circle = new EllipsePolygon(pt, lineWidth);
                                ctx.Fill(color, circle);
                            }
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning($"Failed to apply overlay {ex.Message}");
                            break;
                        }
                    }
                case "LineString":
                    {
                        try
                        {
                            var coords = geometry.GetProperty("coordinates").EnumerateArray();

                            var points = new List<PointF>();

                            var path = new PathBuilder();

                            foreach (var c in coords)
                            {
                                var arr = c.EnumerateArray();
                                double lon = arr.ElementAt(0).GetDouble();
                                double lat = arr.ElementAt(1).GetDouble();
                                points.Add(ProjectToPixel(lon, lat, tileSize, tileCount, xMin, yMin));
                            }
                            //
                            // De-Dupe the the list of points retrieved from line string in case of circular patter where the points might have the same start or end points
                            // duplicate points could cause "degenerate paths" (a path of length 0)
                            //
                            var uniquePoints = points.Distinct().ToArray();

                            //
                            // Even after de-dupe we need to make sure that the list of unique points do not overlap to closely 
                            // So go over two points at a time and plot them
                            //

                            for (int i = 0; i < uniquePoints.Length - 1;)
                            {
                                PointF first = uniquePoints[i];
                                PointF second = uniquePoints[i + 1];

                                path.AddLines(first, second);

                                // If at the second-last element of an odd-length list, overlap the last two pairs
                                if (uniquePoints.Length % 2 == 1 && i == uniquePoints.Length - 3)
                                    i++; // overlap last pair (advance by 1)
                                else
                                    i += 2; // normal step (advance by 2)
                            }

                            // Draw the path on the image as the final step
                            ctx.Draw(color, lineWidth, path.Build());

                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning($"Failed to apply overlay {ex.Message}");
                            break;
                        }

                    }

                case "MultiLineString":
                    {
                        try
                        {
                            foreach (var line in geometry.GetProperty("coordinates").EnumerateArray())
                            {
                                var points = new List<PointF>();

                                var path = new PathBuilder();

                                foreach (var c in line.EnumerateArray())
                                {
                                    var arr = c.EnumerateArray();
                                    double lon = arr.ElementAt(0).GetDouble();
                                    double lat = arr.ElementAt(1).GetDouble();
                                    points.Add(ProjectToPixel(lon, lat, tileSize, tileCount, xMin, yMin));
                                }

                                //
                                // De-Dupe the the list of points retrieved from line string in case of circular patter where the points might have the same start or end points
                                // duplicate points could cause "degenerate paths" (a path of length 0)
                                //
                                var uniquePoints = points.Distinct().ToArray();


                                //
                                // Even after de-dupe we need to make sure that the list of unique points do not overlap to closely 
                                // So go over two points at a time and plot them
                                //
                                for (int i = 0; i < uniquePoints.Length - 1;)
                                {
                                    PointF first = uniquePoints[i];
                                    PointF second = uniquePoints[i + 1];

                                    path.AddLines(first, second);

                                    // If at the second-last element of an odd-length list, overlap the last two pairs
                                    if (uniquePoints.Length % 2 == 1 && i == uniquePoints.Length - 3)
                                        i++; // overlap last pair (advance by 1)
                                    else
                                        i += 2; // normal step (advance by 2)
                                }

                                // Draw the path on the image as the final step
                                ctx.Draw(color, lineWidth, path.Build());
                            }
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning($"Failed to apply overlay {ex.Message}");
                            break;
                        }

                    }
                case "Polygon":
                    {
                        try
                        {
                            // GeoJSON polygons = array of linear rings
                            // First ring = outer boundary, others = holes
                            var rings = geometry.GetProperty("coordinates").EnumerateArray();

                            var polygon = new PathBuilder();
                            bool first = true;
                            foreach (var ring in rings)
                            {
                                var points = new List<PointF>();
                                foreach (var coord in ring.EnumerateArray())
                                {
                                    var arr = coord.EnumerateArray().ToArray();
                                    double lon = arr[0].GetDouble();
                                    double lat = arr[1].GetDouble();
                                    points.Add(ProjectToPixel(lon, lat, tileSize, tileCount, xMin, yMin));
                                }

                                if (first)
                                    polygon.StartFigure();
                                else
                                    polygon.CloseFigure(); // ensure proper ring closure

                                polygon.AddLines(points);
                                first = false;
                            }

                            var builtPolygon = polygon.Build();
                            ctx.Fill(color.WithAlpha(0.3f), builtPolygon); // semi-transparent fill
                            ctx.Draw(color, lineWidth, builtPolygon);
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning($"Failed to apply overlay {ex.Message}");
                            break;
                        }
                    }

                case "MultiPolygon":
                    {
                        try
                        {
                            // Array of polygons
                            foreach (var poly in geometry.GetProperty("coordinates").EnumerateArray())
                            {
                                var polygon = new PathBuilder();
                                bool first = true;

                                foreach (var ring in poly.EnumerateArray())
                                {
                                    var points = new List<PointF>();
                                    foreach (var coord in ring.EnumerateArray())
                                    {
                                        var arr = coord.EnumerateArray().ToArray();
                                        double lon = arr[0].GetDouble();
                                        double lat = arr[1].GetDouble();
                                        points.Add(ProjectToPixel(lon, lat, tileSize, tileCount, xMin, yMin));
                                    }

                                    if (first)
                                        polygon.StartFigure();
                                    else
                                        polygon.CloseFigure();

                                    polygon.AddLines(points);
                                    first = false;
                                }

                                var builtPolygon = polygon.Build();
                                ctx.Fill(color.WithAlpha(0.3f), builtPolygon);
                                ctx.Draw(color, lineWidth, builtPolygon);
                            }
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning($"Failed to apply overlay {ex.Message}");
                            break;
                        }

                    }
                case "GeometryCollection":
                    foreach (var geom in geometry.GetProperty("geometries").EnumerateArray())
                        DrawGeometry(geom, ctx, color, lineWidth, xMin, yMin, tileSize, tileCount);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Creates an image fitted to the bounding box defined by two corners, with optional margin, automatically selecting the zoom level.
        /// </summary>
        /// <param name="lat1">Latitude of first corner in degrees (-90 to 90).</param>
        /// <param name="lon1">Longitude of first corner in degrees (-180 to 180).</param>
        /// <param name="lat2">Latitude of second corner in degrees (-90 to 90).</param>
        /// <param name="lon2">Longitude of second corner in degrees (-180 to 180).</param>
        /// <param name="width">Desired image width in pixels.</param>
        /// <param name="height">Desired image height in pixels.</param>
        /// <param name="marginPercent">Margin percentage around the bounding box (0-50).</param>
        /// <param name="isSatellite">True for satellite imagery, false for non-satellite (OSM).</param>
        /// <param name="opacity">The opacity level of the image.  Between 0 and 1.</param>
        /// <param name="zoom">Optional zoom level to force zoom.  Zoom is auto calculated if this is null.</param>
        /// <param name="imagesToOverlay">Optional list of PNG images with geospatial envelopes to overlay.  List order defines the Z Order.</param>
        /// <returns>A FittedImageResult containing the image data, selected zoom, or error details.</returns>
        public async Task<FittedImageResult> GetFittedImageAsync(double lat1,
                                                                 double lon1,
                                                                 double lat2,
                                                                 double lon2,
                                                                 int width,
                                                                 int height,
                                                                 double marginPercent,
                                                                 bool isSatellite,
                                                                 double opacity = 1,
                                                                 int? zoom = null,
                                                                 List<PNGWithEnvelope> imagesToOverlay = null,
                                                                 List<GeoJsonWithInstructions> geoJsonsToOverlay = null,
                                                                 ColorAndGradientsWithPosition colorAndGradientsWithPosition = null)
        {
            // Validate inputs
            if (lat1 < -90 || lat1 > 90 || lon1 < -180 || lon1 > 180 ||
                lat2 < -90 || lat2 > 90 || lon2 < -180 || lon2 > 180)
            {
                _logger?.LogWarning($"Invalid lat/lon: ({lat1}, {lon1}) to ({lat2}, {lon2})");
                return new FittedImageResult(null, "Invalid latitude or longitude.", 400, 0);
            }

            if (width <= 0 || height <= 0 ||
                width > 10000 || height > 10000)
            {
                _logger?.LogWarning($"Invalid dimensions: width={width}, height={height}");
                return new FittedImageResult(null, "Invalid width or height.", 400, 0);
            }

            if (opacity < 0.0f || opacity > 1.0f)
            {
                _logger?.LogWarning($"Invalid opacity: {opacity}");
                return new FittedImageResult(null, "Image opacity must be between 0.0 and 1.0.", 400);
            }

            if (marginPercent < 0 || marginPercent > 200)
            {
                _logger?.LogWarning($"Invalid margin percent: {marginPercent}");
                return new FittedImageResult(null, "Margin percent must be between 0 and 200.", 400, 0);
            }

            // Validate overlay images
            if (imagesToOverlay != null)
            {
                foreach (PNGWithEnvelope overlay in imagesToOverlay)
                {
                    if (overlay == null || overlay.pngData == null || overlay.pngData.Length == 0)
                    {
                        _logger?.LogWarning("Invalid overlay image: null or empty PNG data.");
                        return new FittedImageResult(null, "Invalid overlay image data.", 400);
                    }

                    if (overlay.minX < -180 || overlay.minX > 180 || overlay.minY < -90 || overlay.minY > 90 ||
                        overlay.width <= 0 || overlay.height <= 0)
                    {
                        _logger?.LogWarning($"Invalid overlay envelope: minX={overlay.minX}, minY={overlay.minY}, width={overlay.width}, height={overlay.height}");
                        return new FittedImageResult(null, "Invalid overlay envelope coordinates or dimensions.", 400);
                    }

                    if (overlay.opacity < 0.0f || overlay.opacity > 1.0f)
                    {
                        _logger?.LogWarning($"Invalid overlay opacity: {overlay.opacity}");
                        return new FittedImageResult(null, "Overlay opacity must be between 0.0 and 1.0.", 400);
                    }
                }
            }

            // Validate overlay geojson
            if (geoJsonsToOverlay != null)
            {
                foreach (GeoJsonWithInstructions overlay in geoJsonsToOverlay)
                {
                    if (overlay == null || overlay.GeoJsonData == null)
                    {
                        _logger?.LogWarning("Invalid overlay GeoJson: null or empty GeoJson data.");
                        return new FittedImageResult(null, "Invalid overlay GeoJson data.", 400);
                    }
                }
            }



            try
            {
                // Compute bounding box
                double minLat = Math.Min(lat1, lat2);
                double maxLat = Math.Max(lat1, lat2);
                double minLon = Math.Min(lon1, lon2);
                double maxLon = Math.Max(lon1, lon2);

                // Apply margin
                double latRange = maxLat - minLat;
                double lonRange = maxLon - minLon;
                double marginLat = latRange * marginPercent / 100;
                double marginLon = lonRange * marginPercent / 100;
                minLat -= marginLat;
                maxLat += marginLat;
                minLon -= marginLon;
                maxLon += marginLon;

                // Clamp to world bounds
                minLat = Math.Max(-90, minLat);
                maxLat = Math.Min(90, maxLat);
                minLon = Math.Max(-180, minLon);
                maxLon = Math.Min(180, maxLon);


                // Find optimal zoom (highest zoom where box occupies ~95% of image dimensions)
                int selectedZoom = 2; // Fallback
                const int tileSize = 256; // Use lowercase for consistency with code style

                const double targetOccupancyRatio = 0.95; // Target 95% of image width or height

                int maxZoom = isSatellite ? GOOGLE_MAX_ZOOM : OSM_MAX_ZOOM; // Respect provider native tile limits without upscaling

                // Ensure minimum coordinate differences to avoid precision issues
                const double minLatDiff = 0.0001; // ~10 meters at equator
                const double minLonDiff = 0.0001;
                if (Math.Abs(maxLat - minLat) < minLatDiff)
                {
                    double midLat = (minLat + maxLat) / 2;
                    minLat = midLat - minLatDiff / 2;
                    maxLat = midLat + minLatDiff / 2;
                    _logger?.LogDebug($"Adjusted latitude range to minimum {minLatDiff} degrees: minLat={minLat}, maxLat={maxLat}");
                }
                if (Math.Abs(maxLon - minLon) < minLonDiff)
                {
                    double midLon = (minLon + maxLon) / 2;
                    minLon = midLon - minLonDiff / 2;
                    maxLon = midLon + minLonDiff / 2;
                    _logger?.LogDebug($"Adjusted longitude range to minimum {minLonDiff} degrees: minLon={minLon}, maxLon={maxLon}");
                }

                //
                // Do we need to calculate the zoom?  If we have no zoom parameter value, then we do.
                //
                if (zoom.HasValue == false)
                {
                    for (int zoomLevelCheck = maxZoom; zoomLevelCheck >= 2; zoomLevelCheck--)
                    {
                        double worldSizeZoomSeek = tileSize * (1L << zoomLevelCheck); // Use long to avoid overflow
                        double pxMinLonZoomSeek = (minLon + 180) / 360 * worldSizeZoomSeek;
                        double pxMaxLonZoomSeek = (maxLon + 180) / 360 * worldSizeZoomSeek;
                        double boxWidthZoomSeek = Math.Abs(pxMaxLonZoomSeek - pxMinLonZoomSeek);

                        // Mercator projection for latitude
                        double maxLatRad = Math.PI * maxLat / 180;
                        double minLatRad = Math.PI * minLat / 180;
                        double pyMaxLatZoomSeek, pyMinLatZoomSeek;

                        try
                        {
                            double tanMaxLat = Math.Tan(maxLatRad);
                            double cosMaxLat = Math.Cos(maxLatRad);

                            if (double.IsInfinity(tanMaxLat) || double.IsInfinity(1 / cosMaxLat))
                            {
                                throw new ArithmeticException("Invalid latitude for Mercator projection");
                            }

                            pyMaxLatZoomSeek = (1 - Math.Log(tanMaxLat + 1 / cosMaxLat) / Math.PI) / 2 * worldSizeZoomSeek;

                            double tanMinLat = Math.Tan(minLatRad);
                            double cosMinLat = Math.Cos(minLatRad);

                            if (double.IsInfinity(tanMinLat) || double.IsInfinity(1 / cosMinLat))
                            {
                                throw new ArithmeticException("Invalid latitude for Mercator projection");
                            }

                            pyMinLatZoomSeek = (1 - Math.Log(tanMinLat + 1 / cosMinLat) / Math.PI) / 2 * worldSizeZoomSeek;
                        }
                        catch (ArithmeticException ex)
                        {
                            _logger?.LogWarning($"Skipping zoom {zoomLevelCheck} due to projection error: {ex.Message}");
                            continue;
                        }

                        double boxHeightZoomSeek = Math.Abs(pyMinLatZoomSeek - pyMaxLatZoomSeek);

                        _logger?.LogDebug($"Zoom {zoomLevelCheck}: boxWidth={boxWidthZoomSeek:F2}px, boxHeight={boxHeightZoomSeek:F2}px, targetWidth={width * targetOccupancyRatio:F2}px, targetHeight={height * targetOccupancyRatio:F2}px");

                        // Skip invalid box sizes
                        if (boxWidthZoomSeek <= 0 || boxHeightZoomSeek <= 0)
                        {
                            _logger?.LogWarning($"Invalid box dimensions at zoom {zoomLevelCheck}: width={boxWidthZoomSeek:F2}, height={boxHeightZoomSeek:F2}");
                            continue;
                        }

                        // Check if the box size is within the target occupancy ratio
                        if (boxWidthZoomSeek <= width * targetOccupancyRatio && boxHeightZoomSeek <= height * targetOccupancyRatio)
                        {
                            selectedZoom = zoomLevelCheck;
                            break;
                        }
                    }
                }
                else
                {
                    selectedZoom = zoom.Value;
                }

                if (selectedZoom == 2)
                {
                    _logger?.LogWarning($"Bounding box too large or invalid for any reasonable zoom; using zoom 2");
                }


                _logger?.LogDebug($"Selected zoom {selectedZoom} for box ({minLat}, {minLon}) to ({maxLat}, {maxLon})");

                // Compute center for tile fetching
                double centerLat = (minLat + maxLat) / 2;
                double centerLon = (minLon + maxLon) / 2;
                double tileCount = 1 << selectedZoom;
                double xTileCenter = (centerLon + 180) / 360 * tileCount;
                double yTileCenter = (1 - Math.Log(Math.Tan(Math.PI * centerLat / 180) + 1 / Math.Cos(Math.PI * centerLat / 180)) / Math.PI) / 2 * tileCount;

                // Compute tile range to cover the box and requested image size
                int maxTiles = (int)Math.Min(tileCount, 32); // Cap tiles
                int minTilesAcross = (int)Math.Ceiling((double)width / TILE_SIZE + 2); // Ensure enough space
                int minTilesDown = (int)Math.Ceiling((double)height / TILE_SIZE + 2);

                double xTileMin = (minLon + 180) / 360 * tileCount;
                double xTileMax = (maxLon + 180) / 360 * tileCount;
                double yTileMin = (1 - Math.Log(Math.Tan(Math.PI * minLat / 180) + 1 / Math.Cos(Math.PI * minLat / 180)) / Math.PI) / 2 * tileCount;
                double yTileMax = (1 - Math.Log(Math.Tan(Math.PI * maxLat / 180) + 1 / Math.Cos(Math.PI * maxLat / 180)) / Math.PI) / 2 * tileCount;

                int xMin = (int)Math.Floor(xTileMin);
                int xMax = (int)Math.Ceiling(xTileMax) - 1;
                int yMin = (int)Math.Floor(yTileMin);
                int yMax = (int)Math.Ceiling(yTileMax) - 1;

                // Ensure composite is large enough
                int tilesAcross = Math.Min(Math.Max(xMax - xMin + 1, minTilesAcross), maxTiles);
                int tilesDown = Math.Min(Math.Max(yMax - yMin + 1, minTilesDown), maxTiles);

                xMin = (int)Math.Floor(xTileCenter - tilesAcross / 2.0);
                xMax = xMin + tilesAcross - 1;
                yMin = (int)Math.Floor(yTileCenter - tilesDown / 2.0);
                yMax = yMin + tilesDown - 1;

                // Validate tiles
                int maxTile = (int)tileCount;
                if (xMin < 0 || xMax >= maxTile || yMin < 0 || yMax >= maxTile)
                {
                    _logger?.LogWarning($"Tile bounds out of range: xMin={xMin}, xMax={xMax}, yMin={yMin}, yMax={yMax}, maxTile={maxTile}");
                    return new FittedImageResult(null, "Requested area exceeds tile bounds.", 400, selectedZoom);
                }

                // Create composite image
                using var compositeImage = new Image<Rgba32>(tilesAcross * TILE_SIZE, tilesDown * TILE_SIZE);

                // Fetch and place tiles
                for (int tx = xMin; tx <= xMax; tx++)
                {
                    for (int ty = yMin; ty <= yMax; ty++)
                    {
                        TileResponse tileResponse = isSatellite
                            ? await GetSatelliteTileAsync(selectedZoom, tx, ty).ConfigureAwait(false)
                            : await GetNonSatelliteTileAsync(selectedZoom, tx, ty).ConfigureAwait(false);

                        if (tileResponse.TileData == null || tileResponse.ErrorStatusCode.HasValue)
                        {
                            _logger?.LogWarning($"Failed to fetch tile z={selectedZoom}, x={tx}, y={ty}: {tileResponse.ErrorMessage}");
                            return new FittedImageResult(null, $"Failed to fetch tile: {tileResponse.ErrorMessage}", tileResponse.ErrorStatusCode ?? 500, selectedZoom);
                        }

                        using var tileImage = Image.Load<Rgba32>(tileResponse.TileData);
                        int posX = (tx - xMin) * TILE_SIZE;
                        int posY = (ty - yMin) * TILE_SIZE;

                        compositeImage.Mutate(ctx => ctx.DrawImage(tileImage, new Point(posX, posY), (float)opacity));
                    }
                }

                // Compute pixel bounds of the expanded box in composite
                double worldSize = TILE_SIZE * tileCount;
                double pxMinLon = (minLon + 180) / 360 * worldSize - xMin * TILE_SIZE;
                double pxMaxLon = (maxLon + 180) / 360 * worldSize - xMin * TILE_SIZE;
                double pyMinLat = (1 - Math.Log(Math.Tan(Math.PI * minLat / 180) + 1 / Math.Cos(Math.PI * minLat / 180)) / Math.PI) / 2 * worldSize - yMin * TILE_SIZE;
                double pyMaxLat = (1 - Math.Log(Math.Tan(Math.PI * maxLat / 180) + 1 / Math.Cos(Math.PI * maxLat / 180)) / Math.PI) / 2 * worldSize - yMin * TILE_SIZE;

                // Compute center pixel for debugging
                double centerXPixel = (xTileCenter - xMin) * TILE_SIZE;
                double centerYPixel = (yTileCenter - yMin) * TILE_SIZE;


                // Overlay PNG images if provided
                if (imagesToOverlay != null && imagesToOverlay.Count > 0)
                {
                    AddOverlayImagesToCompositeImage(imagesToOverlay, tileCount, TILE_SIZE, xMin, yMin, compositeImage, centerXPixel, centerYPixel);
                }

                // Overlay GeoJSON if provided
                if (geoJsonsToOverlay != null)
                {
                    AddOverlayGeoJsonToCompositeImage(geoJsonsToOverlay, tileCount, TILE_SIZE, xMin, yMin, compositeImage, centerXPixel, centerYPixel);
                }

                // Draw debug markers if enabled
                if (_drawDebugMarkersOnImages == true)
                {
                    // Draw red square at center
                    int centerX = (int)Math.Round(centerXPixel);
                    int centerY = (int)Math.Round(centerYPixel);
                    int markerSize = width >= 6000 || height >= 4000 ? 15 : 5; // Larger for big images
                    for (int dy = -markerSize / 2; dy <= markerSize / 2; dy++)
                    {
                        for (int dx = -markerSize / 2; dx <= markerSize / 2; dx++)
                        {
                            int px = centerX + dx;
                            int py = centerY + dy;
                            if (px >= 0 && px < compositeImage.Width && py >= 0 && py < compositeImage.Height)
                            {
                                compositeImage[px, py] = new Rgba32(255, 0, 0, 255); // Red, fully opaque
                            }
                        }
                    }

                    // Draw blue rectangle tracing the bounding box using direct pixel updates
                    int traceXMin = (int)Math.Round(pxMinLon);
                    int traceXMax = (int)Math.Round(pxMaxLon);
                    int traceYMin = (int)Math.Round(pyMaxLat); // Top (smaller y)
                    int traceYMax = (int)Math.Round(pyMinLat); // Bottom (larger y)
                    int traceThickness = width >= 6000 || height >= 4000 ? 5 : 3; // Thicker for large images
                    if (traceXMin < compositeImage.Width && traceXMax >= 0 && traceYMin < compositeImage.Height && traceYMax >= 0)
                    {
                        // Top and bottom borders
                        for (int x = traceXMin; x <= traceXMax; x++)
                        {
                            for (int dy = 0; dy < traceThickness; dy++)
                            {
                                int yTop = traceYMin + dy;
                                int yBottom = traceYMax - dy;
                                if (x >= 0 && x < compositeImage.Width)
                                {
                                    if (yTop >= 0 && yTop < compositeImage.Height)
                                    {
                                        compositeImage[x, yTop] = new Rgba32(0, 0, 255, 255); // Blue
                                    }
                                    if (yBottom >= 0 && yBottom < compositeImage.Height)
                                    {
                                        compositeImage[x, yBottom] = new Rgba32(0, 0, 255, 255);
                                    }
                                }
                            }
                        }
                        // Left and right borders (excluding corners to avoid overlap)
                        for (int y = traceYMin + traceThickness; y <= traceYMax - traceThickness; y++)
                        {
                            for (int dx = 0; dx < traceThickness; dx++)
                            {
                                int xLeft = traceXMin + dx;
                                int xRight = traceXMax - dx;

                                if (y >= 0 && y < compositeImage.Height)
                                {
                                    if (xLeft >= 0 && xLeft < compositeImage.Width)
                                        compositeImage[xLeft, y] = new Rgba32(0, 0, 255, 255);
                                    if (xRight >= 0 && xRight < compositeImage.Width)
                                        compositeImage[xRight, y] = new Rgba32(0, 0, 255, 255);
                                }
                            }
                        }
                    }
                }

                // Log center for verification
                System.Diagnostics.Debug.WriteLine($"Center pixel in composite: ({centerXPixel}, {centerYPixel})");
                System.Diagnostics.Debug.WriteLine($"Bounding box in composite: ({pxMinLon}, {pyMaxLat}) to ({pxMaxLon}, {pyMinLat})");

                // Crop to center the bounding box
                double boxWidth = pxMaxLon - pxMinLon;
                double boxHeight = pyMinLat - pyMaxLat; // y increases downward
                int cropX = (int)Math.Round(centerXPixel - width / 2.0);
                int cropY = (int)Math.Round(centerYPixel - height / 2.0);
                int cropWidth = width;
                int cropHeight = height;

                // Adjust crop to fit composite bounds while preserving center
                if (cropX < 0)
                {
                    cropWidth += cropX; // Reduce width
                    cropX = 0;
                }
                else if (cropX + width > compositeImage.Width)
                {
                    cropWidth = compositeImage.Width - cropX;
                }

                if (cropY < 0)
                {
                    cropHeight += cropY; // Reduce height
                    cropY = 0;
                }
                else if (cropY + height > compositeImage.Height)
                {
                    cropHeight = compositeImage.Height - cropY;
                }

                if (cropWidth <= 0 || cropHeight <= 0)
                {
                    _logger?.LogWarning($"Invalid crop dimensions: cropWidth={cropWidth}, cropHeight={cropHeight}");
                    return new FittedImageResult(null, "Invalid crop dimensions.", 400, selectedZoom);
                }

                // Log if crop is adjusted
                if (cropWidth < width || cropHeight < height)
                {
                    _logger?.LogWarning($"Crop adjusted due to composite bounds: requested {width}x{height}, got {cropWidth}x{cropHeight}");
                }

                // Log crop center for verification
                System.Diagnostics.Debug.WriteLine($"Crop center: ({cropX + cropWidth / 2.0}, {cropY + cropHeight / 2.0})");

                using var croppedImage = (cropX + cropWidth < compositeImage.Width && cropY + cropHeight < compositeImage.Height) ?
                                compositeImage.Clone(ctx => ctx.Crop(new Rectangle(cropX, cropY, cropWidth, cropHeight))) :     // OK to crop
                                compositeImage.Clone();                                                                         // crop will fail


                IResampler resampler = _resampler.ToLower() switch
                {
                    "box" => KnownResamplers.Box,
                    "lanczos3" => KnownResamplers.Lanczos3,
                    "nearestneighbor" => KnownResamplers.NearestNeighbor,
                    _ => KnownResamplers.Lanczos3
                };
                using var resizedImage = croppedImage.Clone(ctx => ctx.Resize(width, height, resampler));


                // validate color gradients and targets

                if (colorAndGradientsWithPosition != null)
                {
                    AddOverlayLegends(colorAndGradientsWithPosition, resizedImage);
                }


                using var outputStream = new MemoryStream();
                resizedImage.SaveAsPng(outputStream);
                byte[] imageData = outputStream.ToArray();

                _logger?.LogInformation($"Generated fitted image for box ({lat1}, {lon1}) to ({lat2}, {lon2}), margin={marginPercent}%, z={selectedZoom}, {width}x{height}, satellite={isSatellite}");
                return new FittedImageResult(imageData, null, null, selectedZoom);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error generating fitted image for box ({lat1}, {lon1}) to ({lat2}, {lon2}): {ex.Message}");
                return new FittedImageResult(null, "Failed to generate image.", 500, 0);
            }
        }


        private void AddOverlayLegends(ColorAndGradientsWithPosition colorAndGradientsWithPosition, Image<Rgba32> backgroundImage)
        {
            try
            {

                Point legendPosition;

                SixLabors.ImageSharp.Size legendSize;

                // 
                // Predetermined Legend image dimensions in pixels
                //

                int landscapeGradientHeight = 30;
                int landscapeLegendHeight = 60;

                int portraitGradientWidth = 30;
                int portraitLegendWidth = 95;

                int horizontalTextPadding = 10;
                int verticalTextPadding = 10;




                switch (colorAndGradientsWithPosition.legendPosition)
                {
                    case ColorAndGradientsWithPosition.LegendPosition.Left:
                        {
                            legendPosition = new Point(0, 0);

                            // Adjust legend width if passcount mode is active
                            if (colorAndGradientsWithPosition.imageMode == "passcount")
                                portraitLegendWidth += 30;

                            // Define legend size and image
                            legendSize = new SixLabors.ImageSharp.Size(portraitLegendWidth, backgroundImage.Height);
                            using var legendImage = new Image<Rgba32>(legendSize.Width, legendSize.Height, Color.White);

                            // Load font
                            var font = SystemFonts.CreateFont("Arial", 18, FontStyle.Italic);

                            // Prepare distinct gradient colors
                            var listofGradients = colorAndGradientsWithPosition.colorGradients.Distinct().ToArray();

                            // --- PASSCOUNT MODE -----------------
                            if (colorAndGradientsWithPosition.imageMode == "passcount")
                            {
                                // Construc the gradient rectangle ,
                                Rectangle gradientRectangle = new Rectangle(
                                    (portraitLegendWidth - portraitGradientWidth),
                                    0,
                                    portraitGradientWidth,
                                    backgroundImage.Height
                                );

                                // Create vertical gradient brush
                                var brush = new LinearGradientBrush(
                                    new PointF(gradientRectangle.X, 0),
                                    new PointF(gradientRectangle.X, gradientRectangle.Height),
                                    GradientRepetitionMode.None,
                                    new ColorStop(0, Color.Red),
                                    new ColorStop(0.25f, Color.Orange),
                                    new ColorStop(0.50f, Color.Yellow),
                                    new ColorStop(0.75f, Color.LightGreen),
                                    new ColorStop(1f, Color.DarkOliveGreen)
                                );

                                legendImage.Mutate(ctx => ctx.Fill(brush, gradientRectangle));

                                // Generate pass count labels 
                                List<string> passCountLabels = new List<string>();

                                if (colorAndGradientsWithPosition.targetValue >= 4)
                                {
                                    for (int i = 1; i <= 4; i++)
                                    {
                                        var passNumber = (colorAndGradientsWithPosition.targetValue * (i / 4f));
                                        passCountLabels.Add($"{passNumber} passes");
                                    }
                                }
                                else
                                {
                                    for (int i = 1; i <= colorAndGradientsWithPosition.targetValue; i++)
                                        passCountLabels.Add($"{i} passes");
                                }

                                // Draw pass count labels
                                string[] labels = passCountLabels.ToArray();
                                for (int i = 0; i < labels.Length; i++)
                                {
                                    // Compute the normalized horizontal position (0 to 1)
                                    float t = i / ((float)labels.Length - 1);

                                    var textSize = TextMeasurer.MeasureSize(labels[i], new TextOptions(font));

                                    float y = (float)(legendSize.Height * 0.35) + (t * (float)(legendSize.Height * 0.65));
                                    y = Math.Clamp(y, (float)(legendSize.Height * 0.35), legendSize.Height - textSize.Height - verticalTextPadding);

                                    // Compute the x axis values by taking 35% of the legend width as starting point and 65% of the legend width as the range to distribute the labels
                                    // This avoids placing the labels too close to the left edge where the not enough data label is located
                                    // Math.Clamp to ensure the labels do not go out of bounds
                                    float x = (portraitLegendWidth - gradientRectangle.Width - textSize.Width - horizontalTextPadding);

                                    legendImage.Mutate(ctx => ctx.DrawText(labels[i], font, Color.Black, new PointF(x, y)));
                                }
                            }

                            // --- NOT ENOUGH DATA MODE -----
                            else if (listofGradients[0].Contains("9FBAC"))
                            {
                                string notEnoughDataLabel = "Not\nEnough\nData";

                                // Measure label height and add padding
                                var notEnoughDataLabelSize = TextMeasurer.MeasureSize(notEnoughDataLabel, new TextOptions(font));
                                var notEnoughDataLabelHeightWithPadding =
                                    (int)notEnoughDataLabelSize.Height + (verticalTextPadding * 2);

                                // Draw "Not enough data" background block
                                legendImage.Mutate(ctx => ctx.Fill(
                                    Color.ParseHex(listofGradients[0]),
                                    new Rectangle(
                                        (portraitLegendWidth - portraitGradientWidth),
                                        0,
                                        portraitGradientWidth,
                                        notEnoughDataLabelHeightWithPadding
                                    )));

                                // Gradient rectangle (below the "Not enough data" section)
                                Rectangle gradientRectangle = new Rectangle(
                                    (portraitLegendWidth - portraitGradientWidth),
                                    notEnoughDataLabelHeightWithPadding,
                                    portraitGradientWidth,
                                    (backgroundImage.Height - notEnoughDataLabelHeightWithPadding)
                                );

                                // Gradient brush
                                var brush = new LinearGradientBrush(
                                    new PointF(gradientRectangle.X, gradientRectangle.Y),
                                    new PointF(gradientRectangle.X, gradientRectangle.Height),
                                    GradientRepetitionMode.None,
                                    new ColorStop(0, Color.Red),
                                    new ColorStop(0.25f, Color.Orange),
                                    new ColorStop(0.50f, Color.Yellow),
                                    new ColorStop(0.75f, Color.LightGreen),
                                    new ColorStop(1f, Color.DarkOliveGreen)
                                );

                                legendImage.Mutate(ctx => ctx.Fill(brush, gradientRectangle));

                                // Draw "Not enough data" text
                                legendImage.Mutate(ctx => ctx.DrawText(
                                    notEnoughDataLabel, font, Color.Black,
                                    new PointF(
                                        portraitLegendWidth - gradientRectangle.Width - notEnoughDataLabelSize.Width,
                                        verticalTextPadding
                                    )));

                                // Draw percentage labels
                                string[] labels = new string[] { "75%", "85%", "95%", "100%" };
                                for (int i = 0; i < labels.Length; i++)
                                {
                                    float t = i / ((float)labels.Length - 1);
                                    var textSize = TextMeasurer.MeasureSize(labels[i], new TextOptions(font));

                                    float y = (float)(legendSize.Height * 0.35) + (t * (float)(legendSize.Height * 0.65));
                                    y = Math.Clamp(y, (float)(legendSize.Height * 0.35),
                                                   legendSize.Height - textSize.Height - verticalTextPadding);

                                    // Compute the x axis values by taking 35% of the legend width as starting point and 65% of the legend width as the range to distribute the labels
                                    // This avoids placing the labels too close to the left edge where the not enough data label is located
                                    // Math.Clamp to ensure the labels do not go out of bounds
                                    float x = (portraitLegendWidth - gradientRectangle.Width - textSize.Width - horizontalTextPadding);

                                    legendImage.Mutate(ctx => ctx.DrawText(labels[i], font, Color.Black, new PointF(x, y)));
                                }
                            }

                            // --- DEFAULT MODE -------
                            else
                            {
                                Rectangle gradientRectangle = new Rectangle(
                                    (portraitLegendWidth - portraitGradientWidth),
                                    0,
                                    portraitGradientWidth,
                                    backgroundImage.Height
                                );

                                // Draw standard gradient
                                var brush = new LinearGradientBrush(
                                    new PointF(gradientRectangle.X, 0),
                                    new PointF(gradientRectangle.X, gradientRectangle.Height),
                                    GradientRepetitionMode.None,
                                    new ColorStop(0, Color.Red),
                                    new ColorStop(0.25f, Color.Orange),
                                    new ColorStop(0.50f, Color.Yellow),
                                    new ColorStop(0.75f, Color.LightGreen),
                                    new ColorStop(1f, Color.DarkOliveGreen)
                                );

                                legendImage.Mutate(ctx => ctx.Fill(brush, gradientRectangle));

                                // Draw percentage labels
                                string[] labels = new string[] { "75%", "85%", "95%", "100%" };
                                for (int i = 0; i < labels.Length; i++)
                                {
                                    float t = i / ((float)labels.Length - 1);
                                    var textSize = TextMeasurer.MeasureSize(labels[i], new TextOptions(font));

                                    float y = (float)(legendSize.Height * 0.35) + (t * (float)(legendSize.Height * 0.65));
                                    y = Math.Clamp(y, (float)(legendSize.Height * 0.35),
                                                   legendSize.Height - textSize.Height - verticalTextPadding);

                                    // Compute the x axis values by taking 35% of the legend width as starting point and 65% of the legend width as the range to distribute the labels
                                    // This avoids placing the labels too close to the left edge where the not enough data label is located
                                    // Math.Clamp to ensure the labels do not go out of bounds
                                    float x = (portraitLegendWidth - gradientRectangle.Width - textSize.Width - horizontalTextPadding);

                                    legendImage.Mutate(ctx => ctx.DrawText(labels[i], font, Color.Black, new PointF(x, y)));
                                }
                            }

                            // Draw legend onto main background
                            backgroundImage.Mutate(ctx => ctx.DrawImage(legendImage, new Point(0, 0), 1f));

                            break;
                        }

                    case ColorAndGradientsWithPosition.LegendPosition.Top:

                        {

                            legendPosition = new Point(0, 0);
                            legendSize = new SixLabors.ImageSharp.Size(backgroundImage.Width, landscapeLegendHeight);

                            // Construct the legend blank image
                            using var legendImage = new Image<Rgba32>(legendSize.Width, legendSize.Height, Color.White);

                            // Load a font
                            var font = SystemFonts.CreateFont("Arial", 18, FontStyle.Italic);

                            // Construct the gradiet based on the list of gradients provided by the colorAndGradientsWithPosition

                            var listofGradients = colorAndGradientsWithPosition.colorGradients.Distinct().ToArray();

                            if (colorAndGradientsWithPosition.imageMode == "passcount")
                            {
                                // Construc the gradient rectangle
                                Rectangle gradientRectangle = new Rectangle(0, landscapeGradientHeight, backgroundImage.Width, landscapeGradientHeight);

                                var brush = new LinearGradientBrush(
                                            new PointF(0, landscapeGradientHeight),
                                            new PointF(gradientRectangle.Width, landscapeGradientHeight), // horizontal gradient
                                            GradientRepetitionMode.None,
                                            new ColorStop(0, Color.Red),
                                            new ColorStop(0.25f, Color.Orange),
                                            new ColorStop(0.50f, Color.Yellow),
                                            new ColorStop(0.75f, Color.LightGreen),
                                            new ColorStop(1f, Color.DarkOliveGreen)
                                            );

                                legendImage.Mutate(ctx => ctx.Fill(brush, gradientRectangle));

                                List<string> passCountLabels = new List<string>();

                                if (colorAndGradientsWithPosition.targetValue >= 4)
                                {

                                    for (int i = 1; i <= 4; i++)
                                    {
                                        var passNumber = (int)(colorAndGradientsWithPosition.targetValue * (i / 4f));

                                        passCountLabels.Add($"{passNumber} passes");
                                    }
                                }
                                else
                                {
                                    for (int i = 1; i <= colorAndGradientsWithPosition.targetValue; i++)
                                    {
                                        passCountLabels.Add($"{i} passes");
                                    }
                                }

                                string[] labels = passCountLabels.ToArray();

                                for (int i = 0; i < labels.Length; i++)
                                {
                                    // Compute the normalized horizontal position (0 to 1)
                                    float t = i / ((float)labels.Length);

                                    var textSize = TextMeasurer.MeasureSize(labels[i], new TextOptions(font));

                                    // Compute the x axis values by taking 35% of the legend width as starting point and 65% of the legend width as the range to distribute the labels
                                    // This avoids placing the labels too close to the left edge where the not enough data label is located
                                    // Math.Clamp to ensure the labels do not go out of bounds
                                    float x = (float)(legendSize.Width * 0.35) + (t * (float)(legendSize.Width * 0.65));

                                    x = Math.Clamp(x, (float)(legendSize.Width * 0.35), legendSize.Width - textSize.Width - horizontalTextPadding);

                                    float y = verticalTextPadding;

                                    // Draw
                                    legendImage.Mutate(ctx => ctx.DrawText(labels[i], font, Color.Black, new PointF(x, y)));
                                }
                            }
                            //
                            // PassCount imagery does not show up with "Not enough data yet" so safe to assume that if listofGradients[0].Contains("9FBAC") the image mode is not passCount
                            //
                            else if (listofGradients[0].Contains("9FBAC"))
                            {
                                string notEnoughDataLabel = "Not Enough Data";
                                // Measure how much space the "Not Enough Data" label will take
                                var notEnoughDataLabelSize = TextMeasurer.MeasureSize(notEnoughDataLabel, new TextOptions(font));

                                // Add some padding
                                var notEnoughDataLabelWidthWithPadding = (int)notEnoughDataLabelSize.Width + (horizontalTextPadding * 2);

                                // Fill the first part of the gradient rectangle wit the not enough data color with some padding
                                legendImage.Mutate(ctx => ctx.Fill(Color.ParseHex(listofGradients[0]), new Rectangle(0, (legendImage.Height - landscapeGradientHeight), notEnoughDataLabelWidthWithPadding, landscapeGradientHeight)));

                                // Construc the gradient rectangle after the not enough data label, take into account the padding
                                Rectangle gradientRectangle = new Rectangle(notEnoughDataLabelWidthWithPadding, (legendImage.Height - landscapeGradientHeight), (backgroundImage.Width - notEnoughDataLabelWidthWithPadding), landscapeGradientHeight);

                                var brush = new LinearGradientBrush(
                                            new PointF(notEnoughDataLabelWidthWithPadding, (legendImage.Height - landscapeGradientHeight)),
                                            new PointF(gradientRectangle.Width, (legendImage.Height - landscapeGradientHeight)), // horizontal gradient
                                            GradientRepetitionMode.None,
                                            new ColorStop(0, Color.Red),
                                            new ColorStop(0.25f, Color.Orange),
                                            new ColorStop(0.50f, Color.Yellow),
                                            new ColorStop(0.75f, Color.LightGreen),
                                            new ColorStop(1f, Color.DarkOliveGreen)
                                            );

                                legendImage.Mutate(ctx => ctx.Fill(brush, gradientRectangle));

                                // Draw not enough dta label
                                legendImage.Mutate(ctx => ctx.DrawText(notEnoughDataLabel, font, Color.Black, new PointF(horizontalTextPadding, verticalTextPadding)));

                                string[] labels = new string[] { "75%", "85%", "95%", "100%" };

                                for (int i = 0; i < labels.Length; i++)
                                {
                                    // Compute the normalized horizontal position (0 to 1) use one less than length to avoid placing last label out of bounds
                                    float t = i / ((float)labels.Length - 1);


                                    var textSize = TextMeasurer.MeasureSize(labels[i], new TextOptions(font));

                                    // Compute the x axis values by taking 35% of the legend width as starting point and 65% of the legend width as the range to distribute the labels
                                    // This avoids placing the labels too close to the left edge where the not enough data label is located
                                    // Math.Clamp to ensure the labels do not go out of bounds
                                    float x = (float)(legendSize.Width * 0.35) + (t * (float)(legendSize.Width * 0.65));

                                    x = Math.Clamp(x, (float)(legendSize.Width * 0.35), legendSize.Width - textSize.Width - horizontalTextPadding);

                                    float y = verticalTextPadding;

                                    // Draw
                                    legendImage.Mutate(ctx => ctx.DrawText(labels[i], font, Color.Black, new PointF(x, y)));
                                }


                            }
                            else
                            {
                                // Construc the gradient rectangle after the not enough data label, take into account the padding
                                Rectangle gradientRectangle = new Rectangle(0, 0, backgroundImage.Width, landscapeGradientHeight);

                                var brush = new LinearGradientBrush(
                                            new PointF(0, 0),
                                            new PointF(gradientRectangle.Width, 0), // horizontal gradient
                                            GradientRepetitionMode.None,
                                            new ColorStop(0, Color.Red),
                                            new ColorStop(0.25f, Color.Orange),
                                            new ColorStop(0.50f, Color.Yellow),
                                            new ColorStop(0.75f, Color.LightGreen),
                                            new ColorStop(1f, Color.DarkOliveGreen)
                                            );

                                legendImage.Mutate(ctx => ctx.Fill(brush, gradientRectangle));

                                string[] labels = new string[] { "75%", "85%", "95%", "100%" };

                                for (int i = 0; i < labels.Length; i++)
                                {
                                    // Compute the normalized horizontal position (0 to 1) use one less than length to avoid placing last label out of bounds
                                    float t = i / ((float)labels.Length - 1);


                                    var textSize = TextMeasurer.MeasureSize(labels[i], new TextOptions(font));

                                    // Compute the x axis values by taking 35% of the legend width as starting point and 65% of the legend width as the range to distribute the labels
                                    // This avoids placing the labels too close to the left edge where the not enough data label is located
                                    // Math.Clamp to ensure the labels do not go out of bounds
                                    float x = (float)(legendSize.Width * 0.35) + (t * (float)(legendSize.Width * 0.65));

                                    x = Math.Clamp(x, (float)(legendSize.Width * 0.35), legendSize.Width - textSize.Width - horizontalTextPadding);

                                    float y = verticalTextPadding;

                                    // Draw
                                    legendImage.Mutate(ctx => ctx.DrawText(labels[i], font, Color.Black, new PointF(x, y)));
                                }
                            }
                            backgroundImage.Mutate(ctx => ctx.DrawImage(legendImage, new Point(0, 0), 1f));
                            break;
                        }


                    case ColorAndGradientsWithPosition.LegendPosition.Right:
                        {

                            // Adjust legend width if passcount mode is active
                            if (colorAndGradientsWithPosition.imageMode == "passcount")
                                portraitLegendWidth += 30;

                            legendPosition = new Point((backgroundImage.Width - portraitLegendWidth), 0);

                            // Define legend size and image
                            legendSize = new SixLabors.ImageSharp.Size(portraitLegendWidth, backgroundImage.Height);
                            using var legendImage = new Image<Rgba32>(legendSize.Width, legendSize.Height, Color.White);

                            // Load a font
                            var font = SystemFonts.CreateFont("Arial", 18, FontStyle.Italic);

                            // Construct the gradiet based on the list of gradients provided by the colorAndGradientsWithPosition

                            var listofGradients = colorAndGradientsWithPosition.colorGradients.Distinct().ToArray();

                            if (colorAndGradientsWithPosition.imageMode == "passcount")
                            {
                                // Construc the gradient rectangle , 
                                Rectangle gradientRectangle = new Rectangle(0, 0, portraitGradientWidth, backgroundImage.Height);

                                var brush = new LinearGradientBrush(
                                            new PointF(gradientRectangle.X, 0),
                                            new PointF(gradientRectangle.X, gradientRectangle.Height), // horizontal gradient
                                            GradientRepetitionMode.None,
                                            new ColorStop(0, Color.Red),
                                            new ColorStop(0.25f, Color.Orange),
                                            new ColorStop(0.50f, Color.Yellow),
                                            new ColorStop(0.75f, Color.LightGreen),
                                            new ColorStop(1f, Color.DarkOliveGreen)
                                            );

                                legendImage.Mutate(ctx => ctx.Fill(brush, gradientRectangle));

                                List<string> passCountLabels = new List<string>();

                                if (colorAndGradientsWithPosition.targetValue >= 4)
                                {

                                    for (int i = 1; i <= 4; i++)
                                    {
                                        var passNumber = (int)(colorAndGradientsWithPosition.targetValue * (i / 4f));

                                        passCountLabels.Add($"{passNumber} passes");
                                    }
                                }
                                else
                                {
                                    for (int i = 1; i <= colorAndGradientsWithPosition.targetValue; i++)
                                    {
                                        passCountLabels.Add($"{i} passes");
                                    }
                                }

                                string[] labels = passCountLabels.ToArray();

                                for (int i = 0; i < labels.Length; i++)
                                {
                                    // Compute the normalized horizontal position (0 to 1)
                                    float t = i / ((float)labels.Length - 1);

                                    var textSize = TextMeasurer.MeasureSize(labels[i], new TextOptions(font));

                                    float y = (float)(legendSize.Height * 0.35) + (t * (float)(legendSize.Height * 0.65));

                                    y = Math.Clamp(y, (float)(legendSize.Height * 0.35), legendSize.Height - textSize.Height - verticalTextPadding);

                                    // Compute the x axis values by taking 35% of the legend width as starting point and 65% of the legend width as the range to distribute the labels
                                    // This avoids placing the labels too close to the left edge where the not enough data label is located
                                    // Math.Clamp to ensure the labels do not go out of bounds
                                    float x = (gradientRectangle.Width + horizontalTextPadding);

                                    // Draw
                                    legendImage.Mutate(ctx => ctx.DrawText(labels[i], font, Color.Black, new PointF(x, y)));
                                }
                            }
                            //
                            // PassCount imagery does not show up with "Not enough data yet" so safe to assume that if listofGradients[0].Contains("9FBAC") the image mode is not passCount
                            //
                            else if (listofGradients[0].Contains("9FBAC"))
                            {
                                string notEnoughDataLabel = "Not\nEnough\nData";
                                // Measure how much space the "Not Enough Data" label will take
                                var notEnoughDataLabelSize = TextMeasurer.MeasureSize(notEnoughDataLabel, new TextOptions(font));

                                // Add some padding
                                var notEnoughDataLabelHeightWithPadding = (int)notEnoughDataLabelSize.Height + (verticalTextPadding * 2);

                                // Fill the first part of the gradient rectangle wit the not enough data color with some padding
                                legendImage.Mutate(ctx => ctx.Fill(Color.ParseHex(listofGradients[0]), new Rectangle(0, 0, portraitGradientWidth, notEnoughDataLabelHeightWithPadding)));

                                // Construc the gradient rectangle after the not enough data label, take into account the padding
                                Rectangle gradientRectangle = new Rectangle(0, notEnoughDataLabelHeightWithPadding, portraitGradientWidth, (backgroundImage.Height - notEnoughDataLabelHeightWithPadding));

                                var brush = new LinearGradientBrush(
                                            new PointF(0, notEnoughDataLabelHeightWithPadding),
                                            new PointF(0, gradientRectangle.Height), // horizontal gradient
                                            GradientRepetitionMode.None,
                                            new ColorStop(0, Color.Red),
                                            new ColorStop(0.25f, Color.Orange),
                                            new ColorStop(0.50f, Color.Yellow),
                                            new ColorStop(0.75f, Color.LightGreen),
                                            new ColorStop(1f, Color.DarkOliveGreen)
                                            );

                                legendImage.Mutate(ctx => ctx.Fill(brush, gradientRectangle));

                                // Draw not enough data label
                                legendImage.Mutate(ctx => ctx.DrawText(notEnoughDataLabel, font, Color.Black, new PointF(gradientRectangle.Width, verticalTextPadding)));

                                string[] labels = new string[] { "75%", "85%", "95%", "100%" };

                                for (int i = 0; i < labels.Length; i++)
                                {
                                    // Compute the normalized horizontal position (0 to 1) use one less than length to avoid placing last label out of bounds
                                    float t = i / ((float)labels.Length - 1);


                                    var textSize = TextMeasurer.MeasureSize(labels[i], new TextOptions(font));

                                    float y = (float)(legendSize.Height * 0.35) + (t * (float)(legendSize.Height * 0.65));

                                    y = Math.Clamp(y, (float)(legendSize.Height * 0.35), legendSize.Height - textSize.Height - verticalTextPadding);

                                    // Compute the x axis values by taking 35% of the legend width as starting point and 65% of the legend width as the range to distribute the labels
                                    // This avoids placing the labels too close to the left edge where the not enough data label is located
                                    // Math.Clamp to ensure the labels do not go out of bounds
                                    float x = (gradientRectangle.Width + horizontalTextPadding);

                                    // Draw
                                    legendImage.Mutate(ctx => ctx.DrawText(labels[i], font, Color.Black, new PointF(x, y)));
                                }
                            }
                            else
                            {

                                // Construc the gradient rectangle
                                Rectangle gradientRectangle = new Rectangle(0, 0, portraitGradientWidth, backgroundImage.Height);

                                var brush = new LinearGradientBrush(
                                            new PointF(0, 0),
                                            new PointF(0, gradientRectangle.Height), // horizontal gradient
                                            GradientRepetitionMode.None,
                                            new ColorStop(0, Color.Red),
                                            new ColorStop(0.25f, Color.Orange),
                                            new ColorStop(0.50f, Color.Yellow),
                                            new ColorStop(0.75f, Color.LightGreen),
                                            new ColorStop(1f, Color.DarkOliveGreen)
                                            );

                                legendImage.Mutate(ctx => ctx.Fill(brush, gradientRectangle));

                                string[] labels = new string[] { "75%", "85%", "95%", "100%" };

                                for (int i = 0; i < labels.Length; i++)
                                {
                                    // Compute the normalized horizontal position (0 to 1) use one less than length to avoid placing last label out of bounds
                                    float t = i / ((float)labels.Length - 1);

                                    var textSize = TextMeasurer.MeasureSize(labels[i], new TextOptions(font));

                                    float y = (float)(legendSize.Height * 0.35) + (t * (float)(legendSize.Height * 0.65));

                                    y = Math.Clamp(y, (float)(legendSize.Height * 0.35), legendSize.Height - textSize.Height - verticalTextPadding);

                                    // Compute the x axis values by taking 35% of the legend width as starting point and 65% of the legend width as the range to distribute the labels
                                    // This avoids placing the labels too close to the left edge where the not enough data label is located
                                    // Math.Clamp to ensure the labels do not go out of bounds
                                    float x = (gradientRectangle.Width + horizontalTextPadding);

                                    // Draw
                                    legendImage.Mutate(ctx => ctx.DrawText(labels[i], font, Color.Black, new PointF(x, y)));
                                }
                            }
                            backgroundImage.Mutate(ctx => ctx.DrawImage(legendImage, legendPosition, 1f));
                            break;
                        }
                    case ColorAndGradientsWithPosition.LegendPosition.Bottom:
                        {

                            legendPosition = new Point(0, (backgroundImage.Height - landscapeLegendHeight));
                            legendSize = new SixLabors.ImageSharp.Size(backgroundImage.Width, landscapeLegendHeight);

                            // Construct the legend blank image
                            using var legendImage = new Image<Rgba32>(legendSize.Width, legendSize.Height, Color.White);

                            // Load a font
                            var font = SystemFonts.CreateFont("Arial", 18, FontStyle.Italic);

                            // Construct the gradiet based on the list of gradients provided by the colorAndGradientsWithPosition

                            var listofGradients = colorAndGradientsWithPosition.colorGradients.Distinct().ToArray();

                            if (colorAndGradientsWithPosition.imageMode == "passcount")
                            {
                                // Construc the gradient rectangle
                                Rectangle gradientRectangle = new Rectangle(0, 0, backgroundImage.Width, landscapeGradientHeight);

                                var brush = new LinearGradientBrush(
                                            new PointF(0, 0),
                                            new PointF(gradientRectangle.Width, 0), // horizontal gradient
                                            GradientRepetitionMode.None,
                                            new ColorStop(0, Color.Red),
                                            new ColorStop(0.25f, Color.Orange),
                                            new ColorStop(0.50f, Color.Yellow),
                                            new ColorStop(0.75f, Color.LightGreen),
                                            new ColorStop(1f, Color.DarkOliveGreen)
                                            );

                                legendImage.Mutate(ctx => ctx.Fill(brush, gradientRectangle));

                                List<string> passCountLabels = new List<string>();

                                if (colorAndGradientsWithPosition.targetValue >= 4)
                                {

                                    for (int i = 1; i <= 4; i++)
                                    {
                                        var passNumber = (int)(colorAndGradientsWithPosition.targetValue * (i / 4f));

                                        passCountLabels.Add($"{passNumber} passes");
                                    }
                                }
                                else
                                {
                                    for (int i = 1; i <= colorAndGradientsWithPosition.targetValue; i++)
                                    {
                                        passCountLabels.Add($"{i} passes");
                                    }
                                }

                                string[] labels = passCountLabels.ToArray();

                                for (int i = 0; i < labels.Length; i++)
                                {
                                    // Compute the normalized horizontal position (0 to 1)
                                    float t = i / ((float)labels.Length);

                                    var textSize = TextMeasurer.MeasureSize(labels[i], new TextOptions(font));

                                    // Compute the x axis values by taking 35% of the legend width as starting point and 65% of the legend width as the range to distribute the labels
                                    // This avoids placing the labels too close to the left edge where the not enough data label is located
                                    // Math.Clamp to ensure the labels do not go out of bounds
                                    float x = (float)(legendSize.Width * 0.35) + (t * (float)(legendSize.Width * 0.65));

                                    x = Math.Clamp(x, (float)(legendSize.Width * 0.35), legendSize.Width - textSize.Width - horizontalTextPadding);

                                    float y = (landscapeGradientHeight + verticalTextPadding);

                                    // Draw
                                    legendImage.Mutate(ctx => ctx.DrawText(labels[i], font, Color.Black, new PointF(x, y)));
                                }
                            }
                            else if (colorAndGradientsWithPosition.imageMode == "altitude")
                            {
                                // Construct the gradient rectangle
                                Rectangle gradientRectangle = new Rectangle(0, 0, backgroundImage.Width, landscapeGradientHeight);

                                var brush = new LinearGradientBrush(
                                            new PointF(0, 0),
                                            new PointF(gradientRectangle.Width, 0), // horizontal gradient
                                            GradientRepetitionMode.None,
                                            new ColorStop(0, Color.White),
                                            new ColorStop(0.50f, Color.ParseHex("#C0CCF8")),
                                            new ColorStop(1f, Color.ParseHex("#0F41F3"))
                                            );

                                legendImage.Mutate(ctx => ctx.Fill(brush, gradientRectangle));

                                List<string> altitudeLabels = new List<string>();

                                float stepSize = (float)((colorAndGradientsWithPosition.targetValue - colorAndGradientsWithPosition.minTargetValue) / 4);

                                for (int i = 0; i < 5; i++)
                                {
                                    float value = (float)colorAndGradientsWithPosition.minTargetValue + (stepSize * i);
                                    altitudeLabels.Add($"{value.ToString("F2")}m");
                                }

                                string[] labels = altitudeLabels.ToArray();

                                for (int i = 0; i < labels.Length; i++)
                                {
                                    // Compute the normalized horizontal position (0 to 1) use one less than length to avoid placing last label out of bounds
                                    float t = i / ((float)labels.Length - 1);

                                    var textSize = TextMeasurer.MeasureSize(labels[i], new TextOptions(font));

                                    // Compute the x axis values by taking 35% of the legend width as starting point and 65% of the legend width as the range to distribute the labels
                                    // This avoids placing the labels too close to the left edge where the not enough data label is located
                                    // Math.Clamp to ensure the labels do not go out of bounds
                                    float x = (float)(legendSize.Width * 0.05) + (t * (float)(legendSize.Width * 0.95));

                                    x = Math.Clamp(x, (float)(legendSize.Width * 0.05), legendSize.Width - textSize.Width - horizontalTextPadding);

                                    float y = (landscapeGradientHeight + verticalTextPadding);

                                    // Draw
                                    legendImage.Mutate(ctx => ctx.DrawText(labels[i], font, Color.Black, new PointF(x, y)));
                                }
                            }
                            //
                            // PassCount imagery does not show up with "Not enough data yet" so safe to assume that if listofGradients[0].Contains("9FBAC") the image mode is not passCount
                            //
                            else if (listofGradients[0].Contains("9FBAC"))
                            {
                                string notEnoughDataLabel = "Not Enough Data";
                                // Measure how much space the "Not Enough Data" label will take
                                var notEnoughDataLabelSize = TextMeasurer.MeasureSize(notEnoughDataLabel, new TextOptions(font));

                                // Add some padding
                                var notEnoughDataLabelWidthWithPadding = (int)notEnoughDataLabelSize.Width + (horizontalTextPadding * 2);

                                // Fill the first part of the gradient rectangle wit the not enough data color with some padding
                                legendImage.Mutate(ctx => ctx.Fill(Color.ParseHex(listofGradients[0]), new Rectangle(0, 0, notEnoughDataLabelWidthWithPadding, landscapeGradientHeight)));

                                // Construc the gradient rectangle after the not enough data label, take into account the padding
                                Rectangle gradientRectangle = new Rectangle(notEnoughDataLabelWidthWithPadding, 0, (backgroundImage.Width - notEnoughDataLabelWidthWithPadding), landscapeGradientHeight);

                                var brush = new LinearGradientBrush(
                                            new PointF(notEnoughDataLabelWidthWithPadding, 0),
                                            new PointF(gradientRectangle.Width, 0), // horizontal gradient
                                            GradientRepetitionMode.None,
                                            new ColorStop(0, Color.Red),
                                            new ColorStop(0.25f, Color.Orange),
                                            new ColorStop(0.50f, Color.Yellow),
                                            new ColorStop(0.75f, Color.LightGreen),
                                            new ColorStop(1f, Color.DarkOliveGreen)
                                            );

                                legendImage.Mutate(ctx => ctx.Fill(brush, gradientRectangle));


                                // Draw not enough dta label
                                legendImage.Mutate(ctx => ctx.DrawText(notEnoughDataLabel, font, Color.Black, new PointF(horizontalTextPadding, (landscapeGradientHeight + verticalTextPadding))));

                                string[] labels = new string[] { "75%", "85%", "95%", "100%" };

                                for (int i = 0; i < labels.Length; i++)
                                {
                                    // Compute the normalized horizontal position (0 to 1) use one less than length to avoid placing last label out of bounds
                                    float t = i / ((float)labels.Length - 1);


                                    var textSize = TextMeasurer.MeasureSize(labels[i], new TextOptions(font));

                                    // Compute the x axis values by taking 35% of the legend width as starting point and 65% of the legend width as the range to distribute the labels
                                    // This avoids placing the labels too close to the left edge where the not enough data label is located
                                    // Math.Clamp to ensure the labels do not go out of bounds on the right side
                                    // legendSize.Width - textSize.Width - horizontalTextPadding computes the maximum x position for the text to fit within the legend area
                                    float x = (float)(legendSize.Width * 0.35) + (t * (float)(legendSize.Width * 0.65));

                                    x = Math.Clamp(x, (float)(legendSize.Width * 0.35), legendSize.Width - textSize.Width - horizontalTextPadding);

                                    float y = (landscapeGradientHeight + verticalTextPadding);

                                    // Draw
                                    legendImage.Mutate(ctx => ctx.DrawText(labels[i], font, Color.Black, new PointF(x, y)));
                                }


                            }
                            else
                            {
                                // Construc the gradient rectangle after the not enough data label, take into account the padding
                                Rectangle gradientRectangle = new Rectangle(0, 0, backgroundImage.Width, landscapeGradientHeight);

                                var brush = new LinearGradientBrush(
                                            new PointF(0, 0),
                                            new PointF(gradientRectangle.Width, 0), // horizontal gradient
                                            GradientRepetitionMode.None,
                                            new ColorStop(0, Color.Red),
                                            new ColorStop(0.25f, Color.Orange),
                                            new ColorStop(0.50f, Color.Yellow),
                                            new ColorStop(0.75f, Color.LightGreen),
                                            new ColorStop(1f, Color.DarkOliveGreen)
                                            );

                                legendImage.Mutate(ctx => ctx.Fill(brush, gradientRectangle));

                                string[] labels = new string[] { "75%", "85%", "95%", "100%" };

                                for (int i = 0; i < labels.Length; i++)
                                {
                                    // Compute the normalized horizontal position (0 to 1) use one less than length to avoid placing last label out of bounds
                                    float t = i / ((float)labels.Length - 1);


                                    var textSize = TextMeasurer.MeasureSize(labels[i], new TextOptions(font));

                                    // Compute the x axis values by taking 35% of the legend width as starting point and 65% of the legend width as the range to distribute the labels
                                    // This avoids placing the labels too close to the left edge where the not enough data label is located
                                    // Math.Clamp to ensure the labels do not go out of bounds on the right side
                                    // legendSize.Width - textSize.Width - horizontalTextPadding computes the maximum x position for the text to fit within the legend area
                                    float x = (float)(legendSize.Width * 0.35) + (t * (float)(legendSize.Width * 0.65));

                                    x = Math.Clamp(x, (float)(legendSize.Width * 0.35), legendSize.Width - textSize.Width - horizontalTextPadding);

                                    float y = (landscapeGradientHeight + verticalTextPadding);

                                    // Draw
                                    legendImage.Mutate(ctx => ctx.DrawText(labels[i], font, Color.Black, new PointF(x, y)));
                                }
                            }
                            backgroundImage.Mutate(ctx => ctx.DrawImage(legendImage, new Point(0, (backgroundImage.Height - landscapeLegendHeight)), 1f));
                            break;
                        }

                    default:
                        {

                            legendPosition = new Point(0, (backgroundImage.Height - landscapeLegendHeight));
                            legendSize = new SixLabors.ImageSharp.Size(backgroundImage.Width, landscapeLegendHeight);

                            // Construct the legend blank image
                            using var legendImage = new Image<Rgba32>(legendSize.Width, legendSize.Height, Color.White);

                            // Load a font
                            var font = SystemFonts.CreateFont("Arial", 18, FontStyle.Italic);

                            // Construct the gradiet based on the list of gradients provided by the colorAndGradientsWithPosition

                            var listofGradients = colorAndGradientsWithPosition.colorGradients.Distinct().ToArray();


                            if (colorAndGradientsWithPosition.imageMode == "passcount")
                            {
                                // Construc the gradient rectangle
                                Rectangle gradientRectangle = new Rectangle(0, 0, backgroundImage.Width, landscapeGradientHeight);

                                var brush = new LinearGradientBrush(
                                            new PointF(0, 0),
                                            new PointF(gradientRectangle.Width, 0), // horizontal gradient
                                            GradientRepetitionMode.None,
                                            new ColorStop(0, Color.Red),
                                            new ColorStop(0.25f, Color.Orange),
                                            new ColorStop(0.50f, Color.Yellow),
                                            new ColorStop(0.75f, Color.LightGreen),
                                            new ColorStop(1f, Color.DarkOliveGreen)
                                            );

                                legendImage.Mutate(ctx => ctx.Fill(brush, gradientRectangle));

                                List<string> passCountLabels = new List<string>();

                                if (colorAndGradientsWithPosition.targetValue >= 4)
                                {

                                    for (int i = 1; i <= 4; i++)
                                    {
                                        var passNumber = (int)(colorAndGradientsWithPosition.targetValue * (i / 4f));

                                        passCountLabels.Add($"{passNumber} passes");
                                    }
                                }
                                else
                                {
                                    for (int i = 1; i <= colorAndGradientsWithPosition.targetValue; i++)
                                    {
                                        passCountLabels.Add($"{i} passes");
                                    }
                                }

                                string[] labels = passCountLabels.ToArray();

                                for (int i = 0; i < labels.Length; i++)
                                {
                                    // Compute the normalized horizontal position (0 to 1)
                                    float t = i / ((float)labels.Length);

                                    var textSize = TextMeasurer.MeasureSize(labels[i], new TextOptions(font));

                                    // Compute the x axis values by taking 35% of the legend width as starting point and 65% of the legend width as the range to distribute the labels
                                    // This avoids placing the labels too close to the left edge where the not enough data label is located
                                    // Math.Clamp to ensure the labels do not go out of bounds on the right side
                                    // legendSize.Width - textSize.Width - horizontalTextPadding computes the maximum x position for the text to fit within the legend area
                                    float x = (float)(legendSize.Width * 0.35) + (t * (float)(legendSize.Width * 0.65));

                                    x = Math.Clamp(x, (float)(legendSize.Width * 0.35), legendSize.Width - textSize.Width - horizontalTextPadding);

                                    float y = (landscapeGradientHeight + verticalTextPadding);

                                    // Draw
                                    legendImage.Mutate(ctx => ctx.DrawText(labels[i], font, Color.Black, new PointF(x, y)));
                                }
                            }
                            //
                            // PassCount imagery does not show up with "Not enough data yet" so safe to assume that if listofGradients[0].Contains("9FBAC") the image mode is not passCount
                            //
                            else if (listofGradients[0].Contains("9FBAC"))
                            {
                                string notEnoughDataLabel = "Not Enough Data";
                                // Measure how much space the "Not Enough Data" label will take
                                var notEnoughDataLabelSize = TextMeasurer.MeasureSize(notEnoughDataLabel, new TextOptions(font));

                                // Add some padding
                                var notEnoughDataLabelWidthWithPadding = (int)notEnoughDataLabelSize.Width + (horizontalTextPadding * 2);

                                // Fill the first part of the gradient rectangle wit the not enough data color with some padding
                                legendImage.Mutate(ctx => ctx.Fill(Color.ParseHex(listofGradients[0]), new Rectangle(0, 0, notEnoughDataLabelWidthWithPadding, landscapeGradientHeight)));

                                // Construc the gradient rectangle after the not enough data label, take into account the padding
                                Rectangle gradientRectangle = new Rectangle(notEnoughDataLabelWidthWithPadding, 0, (backgroundImage.Width - notEnoughDataLabelWidthWithPadding), landscapeGradientHeight);

                                var brush = new LinearGradientBrush(
                                            new PointF(notEnoughDataLabelWidthWithPadding, 0),
                                            new PointF(gradientRectangle.Width, 0), // horizontal gradient
                                            GradientRepetitionMode.None,
                                            new ColorStop(0, Color.Red),
                                            new ColorStop(0.25f, Color.Orange),
                                            new ColorStop(0.50f, Color.Yellow),
                                            new ColorStop(0.75f, Color.LightGreen),
                                            new ColorStop(1f, Color.DarkOliveGreen)
                                            );

                                legendImage.Mutate(ctx => ctx.Fill(brush, gradientRectangle));


                                // Draw not enough dta label
                                legendImage.Mutate(ctx => ctx.DrawText(notEnoughDataLabel, font, Color.Black, new PointF(horizontalTextPadding, (landscapeGradientHeight + verticalTextPadding))));

                                string[] labels = new string[] { "75%", "85%", "95%", "100%" };

                                for (int i = 0; i < labels.Length; i++)
                                {
                                    // Compute the normalized horizontal position (0 to 1) use one less than length to avoid placing last label out of bounds
                                    float t = i / ((float)labels.Length - 1);


                                    var textSize = TextMeasurer.MeasureSize(labels[i], new TextOptions(font));

                                    // Compute the x axis values by taking 35% of the legend width as starting point and 65% of the legend width as the range to distribute the labels
                                    // This avoids placing the labels too close to the left edge where the not enough data label is located
                                    // Math.Clamp to ensure the labels do not go out of bounds on the right side
                                    // legendSize.Width - textSize.Width - horizontalTextPadding computes the maximum x position for the text to fit within the legend area
                                    float x = (float)(legendSize.Width * 0.35) + (t * (float)(legendSize.Width * 0.65));

                                    x = Math.Clamp(x, (float)(legendSize.Width * 0.35), legendSize.Width - textSize.Width - horizontalTextPadding);

                                    float y = (landscapeGradientHeight + verticalTextPadding);

                                    // Draw
                                    legendImage.Mutate(ctx => ctx.DrawText(labels[i], font, Color.Black, new PointF(x, y)));
                                }


                            }
                            else
                            {
                                // Construc the gradient rectangle after the not enough data label, take into account the padding
                                Rectangle gradientRectangle = new Rectangle(0, 0, backgroundImage.Width, landscapeGradientHeight);

                                var brush = new LinearGradientBrush(
                                            new PointF(0, 0),
                                            new PointF(gradientRectangle.Width, 0), // horizontal gradient
                                            GradientRepetitionMode.None,
                                            new ColorStop(0, Color.Red),
                                            new ColorStop(0.25f, Color.Orange),
                                            new ColorStop(0.50f, Color.Yellow),
                                            new ColorStop(0.75f, Color.LightGreen),
                                            new ColorStop(1f, Color.DarkOliveGreen)
                                            );

                                legendImage.Mutate(ctx => ctx.Fill(brush, gradientRectangle));

                                string[] labels = new string[] { "75%", "85%", "95%", "100%" };

                                for (int i = 0; i < labels.Length; i++)
                                {
                                    // Compute the normalized horizontal position (0 to 1) use one less than length to avoid placing last label out of bounds
                                    float t = i / ((float)labels.Length - 1);


                                    var textSize = TextMeasurer.MeasureSize(labels[i], new TextOptions(font));

                                    // Compute the x axis values by taking 35% of the legend width as starting point and 65% of the legend width as the range to distribute the labels
                                    // This avoids placing the labels too close to the left edge where the not enough data label is located
                                    // Math.Clamp to ensure the labels do not go out of bounds on the right side
                                    // legendSize.Width - textSize.Width - horizontalTextPadding computes the maximum x position for the text to fit within the legend area
                                    float x = (float)(legendSize.Width * 0.35) + (t * (float)(legendSize.Width * 0.65));

                                    x = Math.Clamp(x, (float)(legendSize.Width * 0.35), legendSize.Width - textSize.Width - horizontalTextPadding);

                                    float y = (landscapeGradientHeight + verticalTextPadding);

                                    // Draw
                                    legendImage.Mutate(ctx => ctx.DrawText(labels[i], font, Color.Black, new PointF(x, y)));
                                }
                            }
                            backgroundImage.Mutate(ctx => ctx.DrawImage(legendImage, new Point(0, (backgroundImage.Height - landscapeLegendHeight)), 1f));
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to add legends, {ex.Message}");
            }
        }



        /// <summary>
        /// 
        /// This will get a satellite tile for the provided zoom, x, and y values.
        /// 
        /// </summary>
        /// <param name="z"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public async Task<TileResponse> GetSatelliteTileAsync(double z, int x, int y)
        {
            if (!IsValidTileParameters(z, x, y, out string errorMessage))
            {
                if (_logger != null)
                {
                    _logger.LogWarning($"Invalid tile parameters: z={z}, x={x}, y={y}");
                }

                return new TileResponse(null, null, 400, errorMessage, false);
            }

            //
            // Define the cache file names and paths we're going to use
            //
            string memoryCacheKey = $"{z}_{x}_{y}";

            string cacheFileName = $"satellite_{z}_{x}_{y}.png";
            string cacheFilePath = System.IO.Path.Combine(_cacheDirectory, cacheFileName);
            string cacheFailureFilePath = System.IO.Path.Combine(_cacheDirectory, $"satellite_{z}_{x}_{y}_failed.txt");


            //
            // Check the cache first, to see if we can serve this request from the memory cache and/or disk caches.
            //
            (bool flowControl, TileResponse value) = await AttemptToServeTileRequestFromCache(_satelliteTileMemoryCache, z, x, y, memoryCacheKey, cacheFileName, cacheFilePath, cacheFailureFilePath).ConfigureAwait(false);

            //
            // Return the cached data if we have it.
            //
            if (flowControl == false)
            {
                return value;
            }

            //
            // Control reentrancy by allowing limited executions of this at a particular time.
            //
            await _satelliteTileCacheSemaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                //
                // Check the cache again after acquiring the semaphore, as it may have been added while we were waiting
                //
                (flowControl, value) = await AttemptToServeTileRequestFromCache(_satelliteTileMemoryCache, z, x, y, memoryCacheKey, cacheFileName, cacheFilePath, cacheFailureFilePath).ConfigureAwait(false);

                //
                // Return the cached data if we have it.
                //
                if (flowControl == false)
                {
                    return value;
                }

                byte[] tileData = null;

                if (z <= GOOGLE_MAX_ZOOM)
                {
                    // Fetch directly from Google
                    tileData = await FetchFromGoogleAsync(z, x, y).ConfigureAwait(false);

                    if (tileData != null && tileData.Length > 0)
                    {
                        // Cache and return
                        await System.IO.File.WriteAllBytesAsync(cacheFilePath, tileData).ConfigureAwait(false);

                        _satelliteTileMemoryCache.Add(memoryCacheKey, tileData);

                        return new TileResponse(tileData, GenerateETag(tileData), null, null, true);
                    }

                    // Fallback to OSM if applicable
                    if (z <= OSM_MAX_ZOOM)
                    {
                        var (osmData, osmStatusCode) = await FetchFromOsmAsync(z, x, y).ConfigureAwait(false);

                        if (osmData != null && osmData.Length > 0)
                        {
                            // Cache and return (no client caching for fallback)
                            await System.IO.File.WriteAllBytesAsync(cacheFilePath, osmData).ConfigureAwait(false);

                            _satelliteTileMemoryCache.Add(memoryCacheKey, osmData);

                            return new TileResponse(osmData, GenerateETag(osmData), null, null, false);
                        }
                        else if (osmStatusCode.HasValue && osmStatusCode != 418)
                        {
                            // Persist non-throttled failure
                            await System.IO.File.WriteAllTextAsync(cacheFailureFilePath, osmStatusCode.Value.ToString()).ConfigureAwait(false);
                        }
                        // For 418 or other failures: fall through to no-tile/error
                    }
                }
                else
                {
                    // Limit max upscale
                    if (z > MAX_ZOOM)
                    {
                        _logger?.LogWarning($"Zoom {z} too high for upscaling.  Max zoom is {MAX_ZOOM}; returning unsupported.");

                        await System.IO.File.WriteAllTextAsync(cacheFailureFilePath, "400").ConfigureAwait(false);

                        return new TileResponse(null, null, 400, "Zoom level unsupported", false);
                    }

                    // Calculate parent tile coordinates at max zoom (22)
                    int delta = (int)z - GOOGLE_MAX_ZOOM;
                    int factor = 1 << delta;
                    int parent_x = x >> delta; // Equivalent to x / factor
                    int parent_y = y >> delta; // Equivalent to y / factor

                    // Validate parent tile coordinates
                    int maxTilesAtMaxZoom = 1 << GOOGLE_MAX_ZOOM; // 2^22 = 4,194,304
                    if (parent_x < 0 || parent_x >= maxTilesAtMaxZoom || parent_y < 0 || parent_y >= maxTilesAtMaxZoom)
                    {
                        _logger?.LogWarning($"Invalid parent tile coordinates at z={GOOGLE_MAX_ZOOM}: parent_x={parent_x}, parent_y={parent_y}")
                            ;
                        await System.IO.File.WriteAllTextAsync(cacheFailureFilePath, "400").ConfigureAwait(false);

                        return new TileResponse(null, null, 400, "Invalid parent tile coordinates", false);
                    }

                    //
                    // Fetch parent tile
                    //
                    // Check memory cache for parent data first.  If not there, then get it from provider and cache it.
                    //
                    string parentMemoryCacheKey = $"{GOOGLE_MAX_ZOOM}_{parent_x}_{parent_y}";
                    byte[] parentData = null;

                    if (_satelliteTileMemoryCache.TryGetValue(parentMemoryCacheKey, out parentData) && parentData != null && parentData.Length > 0)
                    {
                        _logger?.LogTrace($"Parent tile cache hit for z={GOOGLE_MAX_ZOOM}, x={parent_x}, y={parent_y}");
                    }
                    else
                    {
                        // Fetch and cache parent
                        var fetchedParentData = await FetchFromGoogleAsync(GOOGLE_MAX_ZOOM, parent_x, parent_y).ConfigureAwait(false);

                        if (fetchedParentData == null || fetchedParentData.Length == 0)
                        {
                            _logger?.LogWarning($"Failed to fetch parent tile at z={GOOGLE_MAX_ZOOM}, x={parent_x}, y={parent_y}");

                            // Note - Don't cache the No Tile image, or any response info here.
                            if (_useNoTileImageOnError)
                            {
                                tileData = ReadNoTileImage();

                                return new TileResponse(tileData, GenerateETag(tileData), null, null, true);
                            }
                            else
                            {
                                return new TileResponse(null, null, 400, "Failed to fetch parent tile", false);
                            }
                        }
                        else
                        {
                            parentData = fetchedParentData;

                            // Cache the parent (if not already)
                            _satelliteTileMemoryCache.Add(parentMemoryCacheKey, parentData);
                        }
                    }


                    if (parentData == null || parentData.Length == 0)
                    {
                        _logger?.LogWarning($"Failed to fetch parent tile at z={GOOGLE_MAX_ZOOM}, x={parent_x}, y={parent_y}");

                        // Note - Don't cache the No Tile image, or any response info here.
                        if (_useNoTileImageOnError)
                        {
                            tileData = ReadNoTileImage();
                            return new TileResponse(tileData, GenerateETag(tileData), null, null, true);
                        }
                        else
                        {
                            return new TileResponse(null, null, 400, "Failed to fetch parent tile", false);
                        }
                    }

                    // Generate upscaled tile
                    try
                    {
                        using Image<Rgba32> parentImage = Image.Load<Rgba32>(parentData);

                        int upscaleSize = 256 * factor;

                        IResampler resampler = _resampler.ToLower() switch
                        {
                            "box" => KnownResamplers.Box,
                            "lanczos3" => KnownResamplers.Lanczos3,
                            "nearestneighbor" => KnownResamplers.NearestNeighbor,
                            _ => KnownResamplers.Lanczos3
                        };


                        using Image<Rgba32> upscaled = parentImage.Clone(context => context.Resize(upscaleSize, upscaleSize, resampler));

                        int mod_x = x & (factor - 1); // Equivalent to x % factor
                        int mod_y = y & (factor - 1); // Equivalent to y % factor
                        int offset_x = mod_x * 256;
                        int offset_y = mod_y * 256;

                        // Validate offsets
                        if (offset_x >= upscaleSize || offset_y >= upscaleSize)
                        {
                            _logger?.LogWarning($"Invalid crop offsets for z={z}, x={x}, y={y}: offset_x={offset_x}, offset_y={offset_y}, upscaleSize={upscaleSize}");
                            await System.IO.File.WriteAllTextAsync(cacheFailureFilePath, "400").ConfigureAwait(false);
                            return new TileResponse(null, null, 400, "Invalid crop offsets", false);
                        }

                        using var cropped = upscaled.Clone(context => context.Crop(new Rectangle(offset_x, offset_y, 256, 256)));
                        using var outputStream = new MemoryStream();
                        cropped.SaveAsPng(outputStream);
                        tileData = outputStream.ToArray();

                        // Cache generated tile
                        await System.IO.File.WriteAllBytesAsync(cacheFilePath, tileData).ConfigureAwait(false);

                        _satelliteTileMemoryCache.Add(memoryCacheKey, tileData);

                        return new TileResponse(tileData, GenerateETag(tileData), null, null, true);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"Failed to process parent tile image at z={GOOGLE_MAX_ZOOM}, x={parent_x}, y={parent_y}: {ex.Message}");

                        // Note - Don't cache the No Tile image, or any response info here.
                        if (_useNoTileImageOnError)
                        {
                            tileData = ReadNoTileImage();
                            return new TileResponse(tileData, GenerateETag(tileData), null, null, true);
                        }
                        else
                        {
                            return new TileResponse(null, null, 400, "Invalid parent tile image", false);
                        }
                    }
                }

                // Fallback to no-tile or error (for z <= SATELLITE_PROVIDER_MAX_ZOOM case)
                if (_useNoTileImageOnError)
                {
                    tileData = ReadNoTileImage();
                    return new TileResponse(tileData, GenerateETag(tileData), null, null, true);
                }
                else
                {
                    return new TileResponse(null, null, 400, "Failed to fetch tile", false);
                }
            }
            catch (Exception)
            {
                if (_useNoTileImageOnError == true)
                {
                    byte[] noTileImage = ReadNoTileImage();

                    return new TileResponse(noTileImage, GenerateETag(noTileImage), null, null, false);
                }
                else
                {
                    return new TileResponse(null, null, 400, "Zoom level unsupported", false);
                }
            }
            finally
            {
                _satelliteTileCacheSemaphore.Release();
            }
        }


        private async Task<byte[]> FetchFromGoogleAsync(double z, int x, int y)
        {
            string url = GetGoogleTileUrl(x, y, z);

            await ManageGoogleSleep(url).ConfigureAwait(false);

            HttpResponseMessage response = null;
            try
            {
                await _googleSemaphore.WaitAsync().ConfigureAwait(false);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.UserAgent.ParseAdd(USER_AGENT);
                request.Headers.Referrer = new Uri(REFERRER);
                request.Headers.Add("Accept", "image/png,image/*;q=0.8,*/*;q=0.5");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");

                response = await _httpClient.SendAsync(request).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    _logger?.LogError($"Google satellite tile request failed for {z}/{x}/{y}: {response.StatusCode}. Content: {errorContent}");
                    return null;
                }
            }
            finally
            {
                _googleSemaphore.Release();
                response?.Dispose();
            }
        }


        private async Task<(byte[] Data, int? StatusCode)> FetchFromOsmAsync(double z, int x, int y)
        {
            string url = GetOsmTileUrl(x, y, z);

            await ManageOSMSleep(url).ConfigureAwait(false);

            HttpResponseMessage response = null;
            try
            {
                await _osmSemaphore.WaitAsync().ConfigureAwait(false);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.UserAgent.ParseAdd(USER_AGENT);
                request.Headers.Referrer = new Uri(REFERRER);
                request.Headers.Add("Accept", "image/png,image/*;q=0.8,*/*;q=0.5");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");

                response = await _httpClient.SendAsync(request).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    return (await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false), null);
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    int statusCode = (int)response.StatusCode;
                    if (statusCode == 418)
                    {
                        _logger?.LogError($"OSM tile request throttled for {z}/{x}/{y}: 418. Content: {errorContent}");
                    }
                    else
                    {
                        _logger?.LogError($"OSM tile request failed for {z}/{x}/{y}: {statusCode}. Content: {errorContent}");
                    }
                    return (null, statusCode);
                }
            }
            finally
            {
                _osmSemaphore.Release();
                response?.Dispose();
            }
        }


        private async Task ManageOSMSleep(string url)
        {
            //
            // use the first 10 characters of the URL as a key to track last request time.  Really, all we care about is the letter of the sub server, like a, b, or c.
            //
            string key = url.Substring(0, Math.Min(10, url.Length));

            if (_lastOSMRequestPerUrl.TryGetValue(key, out DateTime lastOSMRequestTime) == true)
            {
                double millisecondsSinceLastServerRequest = DateTime.UtcNow.Subtract(lastOSMRequestTime).TotalMilliseconds;

                if (millisecondsSinceLastServerRequest >= 0 &&
                    millisecondsSinceLastServerRequest < OSM_REQUEST_SLEEP_MILLISECONDS)
                {
                    await Task.Delay((int)(OSM_REQUEST_SLEEP_MILLISECONDS - millisecondsSinceLastServerRequest + Random.Shared.Next(0, 51))).ConfigureAwait(false);
                }
            }

            //
            // Store the current time for this URL key
            //
            _lastOSMRequestPerUrl[key] = DateTime.UtcNow;
        }


        private async Task ManageGoogleSleep(string url)
        {
            //
            // use the first 10 characters of the URL as a key to track last request time.  Really, all we care about is the name of the sub server, like mt0, mt1, et
            //
            string key = url.Substring(0, Math.Min(10, url.Length));

            if (_lastGoogleRequestPerUrl.TryGetValue(key, out DateTime lastGoogleRequestTime) == true)
            {
                double millisecondsSinceLastServerRequest = DateTime.UtcNow.Subtract(lastGoogleRequestTime).TotalMilliseconds;

                if (millisecondsSinceLastServerRequest >= 0 &&
                    millisecondsSinceLastServerRequest < GOOGLE_REQUEST_SLEEP_MILLISECONDS)
                {
                    await Task.Delay((int)(GOOGLE_REQUEST_SLEEP_MILLISECONDS - millisecondsSinceLastServerRequest + Random.Shared.Next(0, 51))).ConfigureAwait(false);
                }
            }

            //
            // Store the current time for this URL key
            //
            _lastGoogleRequestPerUrl[key] = DateTime.UtcNow;
        }


        /// <summary>
        /// 
        /// This will get a non-satellite tile for the provided zoom, x, and y values.
        /// 
        /// </summary>
        /// <param name="z"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public async Task<TileResponse> GetNonSatelliteTileAsync(double z, int x, int y)
        {
            if (!IsValidTileParameters(z, x, y, out string errorMessage))
            {
                _logger?.LogWarning($"Invalid tile parameters: z={z}, x={x}, y={y}");
                return new TileResponse(null, null, 400, errorMessage, false);
            }

            string memoryCacheKey = $"{z}_{x}_{y}";
            string cacheFileName = $"non_satellite_{z}_{x}_{y}.png";
            string cacheFilePath = System.IO.Path.Combine(_cacheDirectory, cacheFileName);
            string cacheFailureFilePath = System.IO.Path.Combine(_cacheDirectory, $"non_satellite_{z}_{x}_{y}_failed.txt");

            // Check cache first
            (bool flowControl, TileResponse value) = await AttemptToServeTileRequestFromCache(_nonSatelliteTileMemoryCache, z, x, y, memoryCacheKey, cacheFileName, cacheFilePath, cacheFailureFilePath).ConfigureAwait(false);
            if (!flowControl)
            {
                return value;
            }

            // Control reentrancy by allowing limited executions of this at a particular time

            await _nonSatelliteTileCacheSemaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                // Check cache again after semaphore
                (flowControl, value) = await AttemptToServeTileRequestFromCache(_nonSatelliteTileMemoryCache, z, x, y, memoryCacheKey, cacheFileName, cacheFilePath, cacheFailureFilePath).ConfigureAwait(false);

                // Serve from cached value if we have it.
                if (flowControl == false)
                {
                    return value;
                }

                byte[] tileData = null;

                if (z <= OSM_MAX_ZOOM)
                {
                    // Fetch directly from OSM
                    var (osmData, osmStatusCode) = await FetchFromOsmAsync(z, x, y).ConfigureAwait(false);
                    if (osmData != null && osmData.Length > 0)
                    {
                        // Cache and return
                        await File.WriteAllBytesAsync(cacheFilePath, osmData).ConfigureAwait(false);

                        _nonSatelliteTileMemoryCache.Add(memoryCacheKey, osmData);

                        return new TileResponse(osmData, GenerateETag(osmData), null, null, true);
                    }
                    else if (osmStatusCode.HasValue && osmStatusCode != 418)
                    {
                        // Persist non-throttled failure
                        await File.WriteAllTextAsync(cacheFailureFilePath, osmStatusCode.Value.ToString()).ConfigureAwait(false);
                    }
                    // For 418 or other failures: fall through to no-tile/error
                }
                else
                {
                    // Limit max upscale
                    if (z > MAX_ZOOM)
                    {
                        _logger?.LogWarning($"Zoom {z} too high for upscaling. Max zoom is {MAX_ZOOM}; returning unsupported.");

                        await File.WriteAllTextAsync(cacheFailureFilePath, "400").ConfigureAwait(false);

                        return new TileResponse(null, null, 400, "Zoom level unsupported", false);
                    }

                    // Calculate parent tile coordinates at max zoom (19)
                    int delta = (int)(z - OSM_MAX_ZOOM);
                    int factor = 1 << delta;
                    int parent_x = x >> delta; // Equivalent to x / factor
                    int parent_y = y >> delta; // Equivalent to y / factor

                    // Validate parent tile coordinates
                    int maxTilesAtMaxZoom = 1 << OSM_MAX_ZOOM; // 2^19 = 524,288
                    if (parent_x < 0 || parent_x >= maxTilesAtMaxZoom || parent_y < 0 || parent_y >= maxTilesAtMaxZoom)
                    {
                        _logger?.LogWarning($"Invalid parent tile coordinates at z={OSM_MAX_ZOOM}: parent_x={parent_x}, parent_y={parent_y}");
                        await File.WriteAllTextAsync(cacheFailureFilePath, "400").ConfigureAwait(false);
                        return new TileResponse(null, null, 400, "Invalid parent tile coordinates", false);
                    }

                    //
                    // Fetch parent tile
                    //
                    // Check memory cache for parent data first.  If not there, then get it from OSM and cache it.
                    //
                    string parentMemoryCacheKey = $"{OSM_MAX_ZOOM}_{parent_x}_{parent_y}";
                    byte[] parentData = null;

                    if (_nonSatelliteTileMemoryCache.TryGetValue(parentMemoryCacheKey, out parentData) && parentData != null && parentData.Length > 0)
                    {
                        _logger?.LogTrace($"Parent tile cache hit for z={OSM_MAX_ZOOM}, x={parent_x}, y={parent_y}");
                    }
                    else
                    {
                        // Fetch and cache parent (as before)
                        var (fetchedParentData, parentStatusCode) = await FetchFromOsmAsync(OSM_MAX_ZOOM, parent_x, parent_y).ConfigureAwait(false);

                        if (fetchedParentData == null || fetchedParentData.Length == 0)
                        {
                            _logger?.LogWarning($"Failed to fetch parent tile at z={OSM_MAX_ZOOM}, x={parent_x}, y={parent_y}");
                            if (_useNoTileImageOnError)
                            {
                                tileData = ReadNoTileImage();

                                return new TileResponse(tileData, GenerateETag(tileData), null, null, true);
                            }
                            else
                            {
                                return new TileResponse(null, null, 400, "Failed to fetch parent tile", false);
                            }
                        }
                        parentData = fetchedParentData;

                        // Cache the parent (if not already)
                        _nonSatelliteTileMemoryCache.Add(parentMemoryCacheKey, parentData);
                    }


                    // Generate upscaled tile
                    try
                    {
                        using Image<Rgba32> parentImage = Image.Load<Rgba32>(parentData);
                        int upscaleSize = 256 * factor;

                        IResampler resampler = _resampler.ToLower() switch
                        {
                            "box" => KnownResamplers.Box,
                            "lanczos3" => KnownResamplers.Lanczos3,
                            "nearestneighbor" => KnownResamplers.NearestNeighbor,
                            _ => KnownResamplers.Lanczos3
                        };

                        using Image<Rgba32> upscaled = parentImage.Clone(context => context.Resize(upscaleSize, upscaleSize, resampler));
                        int mod_x = x & (factor - 1); // Equivalent to x % factor
                        int mod_y = y & (factor - 1); // Equivalent to y % factor
                        int offset_x = mod_x * 256;
                        int offset_y = mod_y * 256;

                        // Validate offsets
                        if (offset_x >= upscaleSize || offset_y >= upscaleSize)
                        {
                            _logger?.LogWarning($"Invalid crop offsets for z={z}, x={x}, y={y}: offset_x={offset_x}, offset_y={offset_y}, upscaleSize={upscaleSize}");

                            await File.WriteAllTextAsync(cacheFailureFilePath, "400").ConfigureAwait(false);

                            return new TileResponse(null, null, 400, "Invalid crop offsets", false);
                        }

                        using var cropped = upscaled.Clone(context => context.Crop(new Rectangle(offset_x, offset_y, 256, 256)));
                        using var outputStream = new MemoryStream();
                        cropped.SaveAsPng(outputStream);
                        tileData = outputStream.ToArray();

                        // Cache generated tile
                        await File.WriteAllBytesAsync(cacheFilePath, tileData).ConfigureAwait(false);

                        _nonSatelliteTileMemoryCache.Add(memoryCacheKey, tileData);

                        return new TileResponse(tileData, GenerateETag(tileData), null, null, true);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"Failed to process parent tile image at z={OSM_MAX_ZOOM}, x={parent_x}, y={parent_y}: {ex.Message}");
                        if (_useNoTileImageOnError)
                        {
                            tileData = ReadNoTileImage();

                            return new TileResponse(tileData, GenerateETag(tileData), null, null, true);
                        }
                        else
                        {
                            return new TileResponse(null, null, 400, "Invalid parent tile image", false);
                        }
                    }
                }

                // Fallback to no-tile or error
                if (_useNoTileImageOnError)
                {
                    tileData = ReadNoTileImage();

                    return new TileResponse(tileData, GenerateETag(tileData), null, null, true);
                }
                else
                {
                    return new TileResponse(null, null, 400, "Failed to fetch tile", false);
                }
            }
            catch (TaskCanceledException tex)
            {
                _logger?.LogWarning($"Timeout error fetching non-satellite tile {z}/{x}/{y}: {tex.Message}");

                return new TileResponse(null, null, 504, "Timeout error while fetching non-satellite tile.", false);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error fetching non-satellite tile {z}/{x}/{y}: {ex.Message}");

                return new TileResponse(null, null, 500, "Internal server error while fetching non-satellite tile.", false);
            }
            finally
            {
                _nonSatelliteTileCacheSemaphore.Release();
            }
        }


        private async Task<(bool flowControl, TileResponse value)> AttemptToServeTileRequestFromCache(ExpiringCache<string, byte[]> memoryTileCache, double z, int x, int y, string memoryCacheKey, string cacheFileName, string cacheFilePath, string cacheFailureFilePath)
        {
            if (memoryTileCache.TryGetValue(memoryCacheKey, out byte[] cachedBytes))
            {
                if (_logger != null)
                {

                    _logger.LogTrace($"Memory cache hit for tile {memoryCacheKey}");
                }
                return (flowControl: false, value: new TileResponse(cachedBytes, GenerateETag(cachedBytes), null, null, true));
            }

            //
            // Check if we have a disk cached error response for this tile
            //
            if (System.IO.File.Exists(cacheFailureFilePath))
            {
                // Use per-file lock to prevent concurrent access conflicts
                var failureLock = _fileLocks.GetOrAdd(cacheFailureFilePath, _ => new SemaphoreSlim(1, 1));
                await failureLock.WaitAsync().ConfigureAwait(false);
                try
                {
                    // Re-check after acquiring lock
                    if (!System.IO.File.Exists(cacheFailureFilePath))
                    {
                        // File was deleted by another thread, continue to normal flow
                    }
                    else
                    {
                        FileInfo fileInfo = new FileInfo(cacheFailureFilePath);
                        if (fileInfo.LastWriteTimeUtc < DateTime.UtcNow.AddHours(-24))
                        {
                            fileInfo.Delete();

                            if (_logger != null)
                            {
                                _logger.LogInformation($"Deleted expired failure file: {cacheFailureFilePath}");
                            }
                        }
                        else
                        {
                            string failureResponse = await System.IO.File.ReadAllTextAsync(cacheFailureFilePath).ConfigureAwait(false);
                            if (int.TryParse(failureResponse, out int responseCode))
                            {
                                if (_logger != null)
                                {
                                    _logger.LogTrace($"Returning cached failure for tile {z}/{x}/{y}: {responseCode}");
                                }

                                return (flowControl: false, value: new TileResponse(null, null, responseCode, "Cached failure response.", false));
                            }
                            else
                            {
                                if (_logger != null)
                                {
                                    _logger.LogWarning($"Invalid failure code in {cacheFailureFilePath}: '{failureResponse}'");
                                }

                                System.IO.File.Delete(cacheFailureFilePath);
                                return (flowControl: false, value: new TileResponse(null, null, 400, "Invalid cached failure code.", false));
                            }
                        }
                    }
                }
                finally
                {
                    failureLock.Release();
                }
            }

            //
            // Check if we have a cached file for this request
            //
            if (System.IO.File.Exists(cacheFilePath))
            {
                if (_logger != null)
                {
                    _logger.LogTrace($"Disk cache hit for non-satellite tile {cacheFileName}");
                }

                // Use per-file lock to prevent concurrent read/write conflicts
                var fileLock = _fileLocks.GetOrAdd(cacheFilePath, _ => new SemaphoreSlim(1, 1));
                await fileLock.WaitAsync().ConfigureAwait(false);
                try
                {
                    // Re-check file exists after acquiring lock (another thread may have deleted it)
                    if (!System.IO.File.Exists(cacheFilePath))
                    {
                        return (flowControl: true, value: default);
                    }

                    byte[] tileData = await System.IO.File.ReadAllBytesAsync(cacheFilePath).ConfigureAwait(false);
                    if (tileData == null || tileData.Length == 0)
                    {
                        if (_logger != null)
                        {
                            _logger.LogError($"Invalid tile data from cache: {cacheFilePath}");
                        }

                        return (flowControl: false, value: new TileResponse(null, null, 500, "Invalid tile data from cache.", false));
                    }

                    //
                    // Put the data we read from the disk cache into the memory cache
                    //
                    memoryTileCache.Add(memoryCacheKey, tileData);

                    return (flowControl: false, value: new TileResponse(tileData, GenerateETag(tileData), null, null, true));
                }
                finally
                {
                    fileLock.Release();
                }
            }

            return (flowControl: true, value: default);
        }


        public async Task<bool> ClearCacheAsync()
        {
            if (_allowCacheClearing == false)
            {
                return false;
            }

            try
            {
                _satelliteTileMemoryCache.Clear();
                _nonSatelliteTileMemoryCache.Clear();

                if (Directory.Exists(_cacheDirectory))
                {
                    Directory.Delete(_cacheDirectory, true);
                    Directory.CreateDirectory(_cacheDirectory);
                }

                if (_logger != null)
                {
                    _logger.LogInformation("Cache cleared successfully.");
                }

                return true;
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.LogError($"Failed to clear cache: {ex.Message}");
                }

                return false;
            }
        }


        public void CleanUpCache(int days = 365)
        {
            if (days > 0)
            {
                try
                {
                    var files = Directory.GetFiles(_cacheDirectory);
                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-1 * days))
                        {
                            fileInfo.Delete();

                            if (_logger != null)
                            {
                                _logger.LogInformation($"Deleted expired cache file: {file}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                    {
                        _logger.LogError($"Failed to clean cache: {ex.Message}");
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// This function will selectively compress the image data provided, based upon the acceptEncoding value provided.
        /// 
        /// if acceptEncoding is null, the image data will not be compressed.
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="acceptEncoding"></param>
        /// <returns></returns>
        public async Task<CompressedDataResult> CompressData(byte[] data, string acceptEncoding = null)
        {
            if (data == null || data.Length == 0)
            {
                if (_logger != null)
                {
                    _logger.LogError("Data is null or empty.");
                }

                return new CompressedDataResult(null, null, 0);
            }

            if (_enableImageCompression == true &&
                acceptEncoding != null)
            {
                acceptEncoding = acceptEncoding?.ToLowerInvariant() ?? string.Empty;

                try
                {
                    //
                    // Does the client support Brotli compression?
                    //
                    if (acceptEncoding.Contains("br"))
                    {
                        using (var compressedStream = new MemoryStream())
                        {
                            using (var brotliStream = new BrotliStream(compressedStream, CompressionLevel.Fastest, true))
                            {
                                await brotliStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                                await brotliStream.FlushAsync().ConfigureAwait(false);
                            }
                            byte[] compressedData = compressedStream.ToArray();
                            if (compressedData.Length == 0)
                            {
                                if (_logger != null)
                                {
                                    _logger.LogError("Brotli compression produced empty data.");
                                }

                                return new CompressedDataResult(null, null, 0);
                            }

                            if (_logger != null)
                            {
                                _logger.LogDebug($"Compressed tile with Brotli, original size: {data.Length}, compressed size: {compressedData.Length}");
                            }

                            return new CompressedDataResult(compressedData, "br", compressedData.Length);
                        }
                    }
                    else if (acceptEncoding.Contains("gzip"))          // Does the client support gzip compression?
                    {
                        using (var compressedStream = new MemoryStream())
                        {
                            using (var gzipStream = new GZipStream(compressedStream, CompressionLevel.Fastest, true))
                            {
                                await gzipStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                                await gzipStream.FlushAsync().ConfigureAwait(false);
                            }
                            byte[] compressedData = compressedStream.ToArray();
                            if (compressedData.Length == 0)
                            {
                                if (_logger != null)
                                {
                                    _logger.LogError("Gzip compression produced empty data.");
                                }

                                return new CompressedDataResult(null, null, 0);
                            }

                            if (_logger != null)
                            {
                                _logger.LogDebug($"Compressed tile with Gzip, original size: {data.Length}, compressed size: {compressedData.Length}");
                            }

                            return new CompressedDataResult(compressedData, "gzip", compressedData.Length);
                        }
                    }
                    else        // Do not use any compression
                    {
                        if (_logger != null)
                        {
                            _logger.LogDebug($"Returning uncompressed tile, size: {data.Length}");
                        }

                        return new CompressedDataResult(data, null, data.Length);
                    }
                }
                catch (IOException ex)
                {
                    if (_logger != null)
                    {
                        _logger.LogError($"Error compressing tile: {ex.Message}");
                    }

                    return new CompressedDataResult(data, null, data.Length);
                }
            }
            else
            {
                if (_logger != null)
                {
                    _logger.LogDebug($"Returning uncompressed tile, size: {data.Length}");
                }

                return new CompressedDataResult(data, null, data.Length);
            }
        }


        private string GenerateETag(byte[] tileData)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(tileData);
                return Convert.ToBase64String(hash);
            }
        }


        /// <summary>
        /// 
        /// This gets an open street map data access URL, with a random choice of the sub domain to use in order to help spread the load on their systems.
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private string GetOsmTileUrl(int x, int y, double z)
        {
            lock (_osmSubdomains)
            {
                if (_osmIndex >= _osmSubdomains.Length || _osmIndex < 0)
                {
                    _osmIndex = 0;
                }
                string subdomain = _osmSubdomains[_osmIndex];

                _osmIndex++;

                return $"https://{subdomain}.tile.openstreetmap.org/{z}/{x}/{y}.png";
            }
        }


        /// <summary>
        /// 
        /// This gets an open street map data access URL, with a random choice of the sub domain to use in order to help spread the load on their systems.
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private string GetGoogleTileUrl(int x, int y, double z)
        {
            lock (_googleSubdomains)
            {
                if (_googleIndex >= _googleSubdomains.Length || _googleIndex < 0)
                {
                    _googleIndex = 0;
                }
                string subdomain = _googleSubdomains[_googleIndex];

                _googleIndex++;


                if (_googleApiKey != null)
                {
                    return $"https://{subdomain}.google.com/vt/lyrs=s&x={x}&y={y}&z={z}&key={_googleApiKey}";
                }
                else
                {
                    return $"https://{subdomain}.google.com/vt/lyrs=s&x={x}&y={y}&z={z}";
                }
            }
        }


        /// <summary>
        /// 
        /// Perform an initial sanity check on the parameters for order of magnitude type validation.
        /// 
        /// </summary>
        /// <param name="z"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private bool IsValidTileParameters(double z, int x, int y, out string errorMessage)
        {
            if (z < 0 ||
                z > MAX_ZOOM ||
                x < 0 ||
                y < 0 ||
                x >= (1 << (int)z) ||
                y >= (1 << (int)z))
            {
                errorMessage = "Invalid tile coordinates.";
                return false;
            }

            errorMessage = null;
            return true;
        }


        /// <summary>
        /// 
        /// This pulls the no title image resource from the assembly and returns it as a byte array.
        /// 
        /// It assumes that a resource called 'no_tile_grey.png' is present in the assembly.
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        private static byte[] ReadNoTileImage()
        {
            // Get the current assembly
            var assembly = Assembly.GetExecutingAssembly();

            string resourceName = $"{assembly.GetName().Name}.Resources.no_tile_grey.png";

            try
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        throw new FileNotFoundException($"Embedded resource {resourceName} not found.");
                    }

                    byte[] buffer = new byte[stream.Length];
                    stream.ReadExactly(buffer);

                    return buffer;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _satelliteTileCacheSemaphore?.Dispose();
            _nonSatelliteTileCacheSemaphore?.Dispose();
            _googleSemaphore?.Dispose();
            _osmSemaphore?.Dispose();
        }
    }
}