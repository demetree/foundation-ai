using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Foundation.BMC.Services;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Serves raw LDraw geometry files (.dat, .ldr) for 3D rendering.
    ///
    /// All files are served from the in-memory LDrawFileService cache,
    /// which preloads the entire LDraw parts library at application startup.
    ///
    /// This controller intentionally does NOT extend SecureWebAPIController.
    /// LDraw geometry files are public data from the LDraw parts library.
    /// Requiring authentication for each request caused crippling database
    /// contention because the Three.js LDrawLoader fires 60+ concurrent
    /// requests (trial-and-error across parts/, p/, models/ directories).
    ///
    /// File resolution is O(1) — a pure dictionary lookup with zero disk I/O.
    /// Missing files return 404 instantly (no directory scanning).
    /// </summary>
    [ApiController]
    public class LDrawController : ControllerBase
    {
        private readonly LDrawFileService _fileService;
        private readonly ILogger<LDrawController> _logger;


        public LDrawController(LDrawFileService fileService, ILogger<LDrawController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }


        /// <summary>
        /// Returns the raw LDraw file content via query parameter.
        /// Example: GET /api/ldraw/file?path=parts/3001.dat
        /// </summary>
        [HttpGet]
        [Route("api/ldraw/file")]
        public IActionResult GetLDrawFile(string path)
        {
            return ServeFile(path);
        }


        /// <summary>
        /// Returns the raw LDraw file content via path segments.
        /// Used by the Three.js LDrawLoader which resolves sub-files by trial and error.
        ///
        /// Example: GET /api/ldraw/file/parts/3001.dat
        /// Example: GET /api/ldraw/file/p/48/4-4cyli.dat
        /// Example: GET /api/ldraw/file/LDConfig.ldr
        /// </summary>
        [HttpGet]
        [Route("api/ldraw/file/{**filePath}")]
        public IActionResult GetLDrawFileByPath(string filePath)
        {
            return ServeFile(filePath);
        }


        /// <summary>
        /// Shared file-serving logic — pure in-memory lookup, zero disk I/O.
        /// Completely synchronous (no async, no DB, no auth).
        /// </summary>
        private IActionResult ServeFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("Path parameter is required.");
            }

            //
            // Path traversal protection
            //
            if (path.Contains("..") || System.IO.Path.IsPathRooted(path))
            {
                return BadRequest("Invalid path.");
            }

            //
            // O(1) lookup in the preloaded cache
            //
            string content = _fileService.TryGetFile(path);

            if (content == null)
            {
                return NotFound();
            }

            return Content(content, "text/plain");
        }
    }
}
