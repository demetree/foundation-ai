using System;
using System.Collections.Generic;
using System.Linq;
using Foundation.BMC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    ///
    /// Public showcase controller — serves read-only, anonymous data for the
    /// landing page. All endpoints are heavily cached in memory and require
    /// no authentication.
    ///
    /// This controller does NOT extend SecureWebAPIController, keeping it
    /// fully outside the Foundation enterprise authorization pipeline.
    ///
    /// </summary>
    [ApiController]
    [Route("api/public")]
    [AllowAnonymous]
    public class PublicShowcaseController : ControllerBase
    {
        private readonly SetExplorerService _setExplorerService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PublicShowcaseController> _logger;

        private static readonly Random _rng = new Random();


        public PublicShowcaseController(
            SetExplorerService setExplorerService,
            IMemoryCache cache,
            ILogger<PublicShowcaseController> logger
        )
        {
            _setExplorerService = setExplorerService;
            _cache = cache;
            _logger = logger;
        }


        /// <summary>
        ///
        /// GET /api/public/stats
        ///
        /// Returns aggregate counts for the landing page hero stats section.
        /// Cached for 1 hour.
        ///
        /// </summary>
        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            //
            // Check the service first — if data isn't ready yet, return 503 immediately
            // WITHOUT caching the null result.  Previous code used GetOrCreate which
            // cached null for 1 hour, causing the landing page to be broken until the
            // cache entry expired even though the data was available seconds later.
            //
            var sets = _setExplorerService.GetCachedSets();

            if (sets == null || sets.Count == 0)
            {
                return StatusCode(503, new { message = "Data is still being computed. Please try again shortly." });
            }

            var stats = _cache.GetOrCreate("public:stats", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

                return new
                {
                    totalSets = sets.Count,
                    totalParts = sets.Sum(s => s.PartCount),
                    totalThemes = sets.Where(s => s.ThemeId.HasValue).Select(s => s.ThemeId).Distinct().Count(),
                    newestYear = sets.Max(s => s.Year),
                    oldestYear = sets.Where(s => s.Year > 0).Min(s => s.Year)
                };
            });

            return Ok(stats);
        }


        /// <summary>
        ///
        /// GET /api/public/featured-sets
        ///
        /// Returns the top 12 sets by part count for the "Most Epic Sets" showcase.
        /// Cached for 1 hour.
        ///
        /// </summary>
        [HttpGet("featured-sets")]
        public IActionResult GetFeaturedSets()
        {
            var sets = _setExplorerService.GetCachedSets();
            if (sets == null)
                return StatusCode(503, new { message = "Data is still being computed." });

            var featured = _cache.GetOrCreate("public:featured-sets", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

                return sets
                    .OrderByDescending(s => s.PartCount)
                    .Take(12)
                    .Select(s => new
                    {
                        s.Id, s.Name, s.SetNumber, s.Year,
                        s.PartCount, s.ImageUrl, s.ThemeName
                    })
                    .ToList();
            });

            return Ok(featured);
        }


        /// <summary>
        ///
        /// GET /api/public/recent-sets
        ///
        /// Returns the 12 most recently added sets.
        /// Cached for 15 minutes.
        ///
        /// </summary>
        [HttpGet("recent-sets")]
        public IActionResult GetRecentSets()
        {
            var sets = _setExplorerService.GetCachedSets();
            if (sets == null)
                return StatusCode(503, new { message = "Data is still being computed." });

            var recent = _cache.GetOrCreate("public:recent-sets", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

                return sets
                    .Take(12)
                    .Select(s => new
                    {
                        s.Id, s.Name, s.SetNumber, s.Year,
                        s.PartCount, s.ImageUrl, s.ThemeName
                    })
                    .ToList();
            });

            return Ok(recent);
        }


        /// <summary>
        ///
        /// GET /api/public/decades
        ///
        /// Returns decade-bucketed set and theme counts for the "Explore by Decade" section.
        /// Cached for 24 hours.
        ///
        /// </summary>
        [HttpGet("decades")]
        public IActionResult GetDecades()
        {
            var sets = _setExplorerService.GetCachedSets();
            if (sets == null)
                return StatusCode(503, new { message = "Data is still being computed." });

            var decades = _cache.GetOrCreate("public:decades", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);

                return sets
                    .Where(s => s.Year > 0)
                    .GroupBy(s => (s.Year / 10) * 10)
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        label = $"{g.Key}s",
                        startYear = g.Key,
                        endYear = g.Key + 9,
                        setCount = g.Count(),
                        themeCount = g.Where(s => s.ThemeId.HasValue).Select(s => s.ThemeId).Distinct().Count()
                    })
                    .ToList();
            });

            return Ok(decades);
        }


        /// <summary>
        ///
        /// GET /api/public/random-discovery
        ///
        /// Returns 6 randomly selected sets for the "Surprise Me" section.
        /// Not cached — always fresh.
        ///
        /// </summary>
        [HttpGet("random-discovery")]
        public IActionResult GetRandomDiscovery()
        {
            var sets = _setExplorerService.GetCachedSets();
            if (sets == null)
                return StatusCode(503, new { message = "Data is still being computed." });

            // Fisher–Yates partial shuffle for 6 random items
            var pool = sets.Where(s => !string.IsNullOrEmpty(s.ImageUrl)).ToList();
            int count = Math.Min(6, pool.Count);
            var result = new List<object>(count);

            for (int i = 0; i < count; i++)
            {
                int j = _rng.Next(i, pool.Count);
                (pool[i], pool[j]) = (pool[j], pool[i]);
                var s = pool[i];
                result.Add(new
                {
                    s.Id, s.Name, s.SetNumber, s.Year,
                    s.PartCount, s.ImageUrl, s.ThemeName
                });
            }

            return Ok(result);
        }
    }
}
