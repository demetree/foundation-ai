using Foundation.Controllers;
using Foundation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Foundation.Services.TileManagementService;

namespace Foundation.Controllers.WebAPI
{
    [Route("api")]
    [ApiController]
    public class TileProxyController : ControllerBase
    {
        private readonly TileManagementService _tileService;
        private readonly ILogger<TileProxyController> _logger;
        private readonly IConfiguration _configuration;

        private readonly bool _requireAuthentication;               // If true, all endpoints require an authenticated user
        private readonly bool _drawDebugMarkersOnImages;            // If true, the tile service will draw debug markers on images it generates


        public TileProxyController(TileManagementService tileService,
                                   IConfiguration configuration,
                                   ILogger<TileProxyController> logger)
        {
            _tileService = tileService;
            _logger = logger;
            _configuration = configuration;

            _requireAuthentication = configuration.GetValue<bool>("TileProxy:RequireAuthentication", false);
            _drawDebugMarkersOnImages = configuration.GetValue<bool>("TileProxy:DrawDebugMarkersOnImages", false);

            _tileService.DrawDebugMarkersOnImages = _drawDebugMarkersOnImages;
        }


        /// <summary>
        /// 
        /// This gets a satellite tile for a web client at the provided zoom, x, and y values.
        /// 
        /// </summary>
        /// <param name="z">Zoom</param>
        /// <param name="x">X Position</param>
        /// <param name="y">Y Position</param>
        /// <returns></returns>
        [RateLimit(RateLimitOption.OneHundredPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpGet("Tiles/Satellite/{z}/{x}/{y}")]
        public async Task<IActionResult> GetSatelliteTile(double z, int x, int y)
        {
            //
            // If authentication is required, ensure the user is authenticated
            //
            // Note - that in Swagger, this won't work if authentication is required because our authentication is checked here, rather than by using the [Authorize] decorator.
            // That means that Swagger doesn't know that it needs to send the auth header, and the request will return with a 401/Unauthorized on the check here.
            //
            if (_requireAuthentication && !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            // Limit zoom to 2 decimal places
            z = Math.Round(z, 2);

            TileResponse tileResponse = await _tileService.GetSatelliteTileAsync(z, x, y);
            if (tileResponse.ErrorStatusCode.HasValue)
            {
                // Tell the client not to cache error tile responses 
                Response.Headers.Add("Cache-Control", "no-cache");

                return StatusCode(tileResponse.ErrorStatusCode.Value, tileResponse.ErrorMessage);
            }

            return await ReturnImage(tileResponse.TileData, tileResponse.ETag, tileResponse.AllowClientSideCaching);
        }


        /// <summary>
        /// 
        /// This gets a non-satellite tile for a web client at the provided zoom, x, and y values.
        /// 
        /// </summary>
        /// <param name="z">Zoom</param>
        /// <param name="x">X Position</param>
        /// <param name="y">Y Position</param>
        /// <returns></returns>
        [RateLimit(RateLimitOption.OneHundredPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpGet("Tiles/NonSatellite/{z}/{x}/{y}")]
        public async Task<IActionResult> GetNonSatelliteTile(double z, int x, int y)
        {
            //
            // If authentication is required, ensure the user is authenticated
            //
            // Note - that in Swagger, this won't work if authentication is required because our authentication is checked here, rather than by using the [Authorize] decorator.
            // That means that Swagger doesn't know that it needs to send the auth header, and the request will return with a 401/Unauthorized on the check here.
            //
            if (_requireAuthentication && !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            // Limit zoom to 2 decimal places
            z = Math.Round(z, 2);

            TileResponse tileResponse = await _tileService.GetNonSatelliteTileAsync(z, x, y);
            if (tileResponse.ErrorStatusCode.HasValue)
            {
                Response.Headers.Add("Cache-Control", "no-cache");

                return StatusCode(tileResponse.ErrorStatusCode.Value, tileResponse.ErrorMessage);
            }

            return await ReturnImage(tileResponse.TileData, tileResponse.ETag, tileResponse.AllowClientSideCaching);
        }


        /// <summary>
        /// 
        /// This gets an image centered at the provided latitude and longitude, with the specified width, height, and zoom level.
        /// 
        /// Additional overlay images are not supported with this end point.
        /// 
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="satellite">whether or not to use satellite background imagery</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        [RateLimit(RateLimitOption.FiftyPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpGet("MapImages/Point")]
        public async Task<IActionResult> GetImageAtPoint(double latitude,
                                                                  double longitude,
                                                                  bool satellite = true,
                                                                  int width = 800,
                                                                  int height = 600,
                                                                  int zoom = 10,
                                                                  double opacity = 1)
        {
            //
            // If authentication is required, ensure the user is authenticated
            //
            // Note - that in Swagger, this won't work if authentication is required because our authentication is checked here, rather than by using the [Authorize] decorator.
            // That means that Swagger doesn't know that it needs to send the auth header, and the request will return with a 401/Unauthorized on the check here.
            //
            if (_requireAuthentication && !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            CenteredImageResult imageResponse = await _tileService.GetCenteredImageAsync(latitude,
                                                                                         longitude,
                                                                                         width,
                                                                                         height,
                                                                                         zoom,
                                                                                         satellite,
                                                                                         opacity);


            if (imageResponse.ErrorStatusCode.HasValue)
            {
                // Tell the client not to cache error tile responses 
                Response.Headers.Add("Cache-Control", "no-cache");

                return StatusCode(imageResponse.ErrorStatusCode.Value, imageResponse.ErrorMessage);
            }

            // return image wih headers to allow client side caching
            return await ReturnImage(imageResponse.ImageData, null, true);
        }


        /// <summary>
        /// 
        /// This function returns an image that can contain a box represented by the provided points, with an optional margin percentage
        /// 
        /// Additional overlay images are not supported with this end point.
        /// 
        /// </summary>
        /// <param name="latitude1"></param>
        /// <param name="longitude1"></param>
        /// <param name="latitude2"></param>
        /// <param name="longitude2"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="marginPercent"></param>
        /// <returns></returns>
        [RateLimit(RateLimitOption.FiftyPerSecond, Scope = RateLimitScope.PerUser)]
        [HttpGet("MapImages/Box")]
        public async Task<IActionResult> GetImageFittedAroundBox(double latitude1,
                                                                 double longitude1,
                                                                 double latitude2,
                                                                 double longitude2,
                                                                 bool satellite = true,
                                                                 int width = 800,
                                                                 int height = 600,
                                                                 double marginPercent = 10,
                                                                 int? zoom = null,
                                                                 double opacity = 1)
        {
            //
            // If authentication is required, ensure the user is authenticated
            //
            // Note - that in Swagger, this won't work if authentication is required because our authentication is checked here, rather than by using the [Authorize] decorator.
            // That means that Swagger doesn't know that it needs to send the auth header, and the request will return with a 401/Unauthorized on the check here.
            //
            if (_requireAuthentication && !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }


            FittedImageResult imageResponse = await _tileService.GetFittedImageAsync(latitude1,
                                                                                     longitude1,
                                                                                     latitude2,
                                                                                     longitude2,
                                                                                     width,
                                                                                     height,
                                                                                     marginPercent,
                                                                                     satellite,
                                                                                     opacity,
                                                                                     zoom);

            if (imageResponse.ErrorStatusCode.HasValue)
            {
                // Tell the client not to cache error tile responses 
                Response.Headers.Add("Cache-Control", "no-cache");

                return StatusCode(imageResponse.ErrorStatusCode.Value, imageResponse.ErrorMessage);
            }

            // return image wih headers to allow client side caching
            return await ReturnImage(imageResponse.ImageData, null, true);
        }


        /// <summary>
        /// 
        /// This clears the cache of tiles.  Consider limiting access to this function.  Leaving here for now....
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpDelete("Tiles/ClearTileCache")]
        [RateLimit(RateLimitOption.OnePerMinute)]
        public async Task<IActionResult> ClearTileCache()
        {
            //
            // If authentication is required, ensure the user is authenticated
            //
            // Note - that in Swagger, this won't work if authentication is required because our authentication is checked here, rather than by using the [Authorize] decorator.
            // That means that Swagger doesn't know that it needs to send the auth header, and the request will return with a 401/Unauthorized on the check here.
            //
            if (_requireAuthentication && !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }


            bool success = await _tileService.ClearCacheAsync();

            if (success)
            {
                return Ok("Cache cleared successfully.");
            }
            Response.Headers.Add("Cache-Control", "no-cache");

            return Problem("Failed to clear tile cache.");
        }

        /// <summary>
        /// 
        /// This configures the Response object with a byte array containing image data, based on the compression and cache capabilities of the client.
        /// 
        /// It will return a File with content type of image/png
        /// 
        /// </summary>
        /// <param name="imageData"></param>
        /// <param name="eTag"></param>
        /// <returns></returns>
        private async Task<IActionResult> ReturnImage(byte[] imageData, string eTag = null, bool allowClientSideCaching = true)
        {
            //
            // Did the client give us an ETag we can try to match with what we have?
            //
            if (eTag != null && Request.Headers["If-None-Match"] == $"\"{eTag}\"")
            {
                // 7 days (604800 seconds)
                Response.Headers.Add("Cache-Control", "public, max-age=604800");
                Response.Headers.Add("Expires", DateTime.UtcNow.AddSeconds(604800).ToString("R"));


                Response.Headers.Add("ETag", $"\"{eTag}\"");

                _logger.LogDebug($"Returning 304 Not Modified for tile with ETag {eTag}");

                return StatusCode(304);
            }


            //
            // Use the client's capabilities from its Accept-Encoding header to compress the tile data, or not.
            //
            string acceptEncoding = Request.Headers["Accept-Encoding"].ToString().ToLowerInvariant();
            try
            {
                _logger.LogDebug($"Processing tile with Accept-Encoding: {acceptEncoding}, User-Agent: {Request.Headers["User-Agent"]}");

                CompressedDataResult compressionResult = await _tileService.CompressData(imageData, acceptEncoding);

                if (compressionResult.Data == null)
                {
                    Response.Headers.Add("Cache-Control", "no-cache");

                    return Problem("Compression failed.");
                }


                //
                // Setup Cache rules and, put on the ETag, and the content length
                //
                // We require an eTag from the tile provider to add cache details.  If there is no eTag, we will tell the client not to cache the 
                // result as it likely means we're sending a placeholder tile we don't want them to reuse.
                //
                if (string.IsNullOrEmpty(eTag) == false && allowClientSideCaching == true)
                {
                    // 7 days (604800 seconds)
                    Response.Headers.Add("Cache-Control", "public, max-age=604800");
                    Response.Headers.Add("Expires", DateTime.UtcNow.AddSeconds(604800).ToString("R"));

                    Response.Headers.Add("ETag", $"\"{eTag}\"");
                }
                else
                {
                    //
                    // We don't want the client to cache this particular response.
                    //
                    Response.Headers.Add("Cache-Control", "no-cache");
                }


                // Only put on the vary header if the tile service could be using compression
                if (_tileService.CompressionEnabled == true)
                {
                    Response.Headers.Add("Vary", "Accept-Encoding");
                }

                Response.Headers.Add("Content-Length", compressionResult.ContentLength.ToString());

                //
                // If we've compressed the data, put on its encoding.
                //
                if (!string.IsNullOrEmpty(compressionResult.ContentEncoding))
                {
                    Response.Headers.Add("Content-Encoding", compressionResult.ContentEncoding);
                }

                return File(compressionResult.Data, "image/png");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error in ReturnCompressedTile: {ex.Message}");

                Response.Headers.Add("Cache-Control", "no-cache");

                return Problem("Unexpected error while processing tile.");
            }
        }
    }
}