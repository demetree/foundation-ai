using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Foundation.Auditor;
using Foundation.BMC.Database;
using Foundation.BMC.Services;
using Foundation.Controllers;
using Foundation.Security;
using Foundation.Security.Database;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    ///
    /// MOCHub controller — the main API surface for the GitHub-style MOC publishing platform.
    ///
    /// Public endpoints (anonymous):
    ///   - Explore / search / browse public MOCs
    ///   - View MOC detail pages, version history, fork networks
    ///   - View user MOC directories
    ///
    /// Authenticated endpoints:
    ///   - Publish a project as a MOC
    ///   - Commit new versions
    ///   - Fork a MOC
    ///   - Update MOC metadata
    ///   - Manage collaborators
    ///
    /// Public endpoints extend ControllerBase with [AllowAnonymous] (same pattern as PublicBrowseController).
    /// Authenticated endpoints use SecureWebAPIController patterns.
    ///
    /// AI-developed code — initial implementation March 2026
    ///
    /// </summary>
    [ApiController]
    [Route("api/mochub")]
    public class MocHubController : SecureWebAPIController
    {
        private readonly BMCContext _context;
        private readonly MocVersioningService _versioningService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<MocHubController> _logger;

        private static readonly TimeSpan ShortCache = TimeSpan.FromMinutes(2);


        public MocHubController(
            BMCContext context,
            MocVersioningService versioningService,
            IMemoryCache cache,
            ILogger<MocHubController> logger)
            : base("BMC", "MocHub")
        {
            _context = context;
            _versioningService = versioningService;
            _cache = cache;
            _logger = logger;

            _context.Database.SetCommandTimeout(30);
        }


        // ────────────────────────────────────────────────────────────────
        //  Public Endpoints (anonymous access)
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        ///
        /// GET /api/mochub/explore
        ///
        /// Returns trending, recent, and featured public MOCs for the explore page.
        ///
        /// </summary>
        [HttpGet("explore")]
        [AllowAnonymous]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.Global)]
        public async Task<IActionResult> Explore(
            string sort = "trending",
            int pageSize = 20,
            int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            pageSize = Math.Clamp(pageSize, 1, 50);
            pageNumber = Math.Max(1, pageNumber);

            IQueryable<PublishedMoc> query = _context.PublishedMocs
                .Where(pm => pm.visibility == "Public"
                          && pm.isPublished == true
                          && pm.active == true
                          && pm.deleted == false);

            query = sort switch
            {
                "recent" => query.OrderByDescending(pm => pm.publishedDate),
                "stars" => query.OrderByDescending(pm => pm.likeCount).ThenByDescending(pm => pm.publishedDate),
                "forks" => query.OrderByDescending(pm => pm.forkCount).ThenByDescending(pm => pm.publishedDate),
                "parts" => query.OrderByDescending(pm => pm.partCount).ThenByDescending(pm => pm.publishedDate),
                _ => query.OrderByDescending(pm => pm.likeCount + pm.forkCount + pm.viewCount) // trending
                          .ThenByDescending(pm => pm.publishedDate)
            };

            int totalCount = await query.CountAsync(cancellationToken);

            var mocs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(pm => new
                {
                    pm.id,
                    pm.name,
                    pm.slug,
                    pm.description,
                    pm.thumbnailImagePath,
                    pm.tags,
                    pm.partCount,
                    pm.likeCount,
                    pm.commentCount,
                    pm.forkCount,
                    pm.viewCount,
                    pm.publishedDate,
                    pm.licenseName,
                    pm.isFeatured,
                    pm.tenantGuid,
                    forkedFromMocName = pm.forkedFromMoc != null ? pm.forkedFromMoc.name : null
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                items = mocs,
                totalCount,
                pageNumber,
                pageSize
            });
        }


        /// <summary>
        ///
        /// GET /api/mochub/explore/search
        ///
        /// Full-text search across public MOCs with filters.
        ///
        /// </summary>
        [HttpGet("explore/search")]
        [AllowAnonymous]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.Global)]
        public async Task<IActionResult> Search(
            string q = null,
            string tags = null,
            int? minParts = null,
            int? maxParts = null,
            string sort = "relevance",
            int pageSize = 20,
            int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            pageSize = Math.Clamp(pageSize, 1, 50);
            pageNumber = Math.Max(1, pageNumber);

            IQueryable<PublishedMoc> query = _context.PublishedMocs
                .Where(pm => pm.visibility == "Public"
                          && pm.isPublished == true
                          && pm.active == true
                          && pm.deleted == false);

            //
            // Text search across name, description, and tags
            //
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(pm =>
                    pm.name.Contains(q)
                    || pm.description.Contains(q)
                    || pm.tags.Contains(q)
                    || pm.readmeMarkdown.Contains(q));
            }

            //
            // Tag filter
            //
            if (!string.IsNullOrWhiteSpace(tags))
            {
                string[] tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (string tag in tagList)
                {
                    string tagLower = tag.ToLowerInvariant();
                    query = query.Where(pm => pm.tags.ToLower().Contains(tagLower));
                }
            }

            //
            // Part count range filters
            //
            if (minParts.HasValue)
            {
                query = query.Where(pm => pm.partCount >= minParts.Value);
            }
            if (maxParts.HasValue)
            {
                query = query.Where(pm => pm.partCount <= maxParts.Value);
            }

            //
            // Sort
            //
            query = sort switch
            {
                "recent" => query.OrderByDescending(pm => pm.publishedDate),
                "stars" => query.OrderByDescending(pm => pm.likeCount),
                "forks" => query.OrderByDescending(pm => pm.forkCount),
                "parts" => query.OrderByDescending(pm => pm.partCount),
                _ => query.OrderByDescending(pm => pm.likeCount + pm.viewCount) // relevance
            };

            int totalCount = await query.CountAsync(cancellationToken);

            var mocs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(pm => new
                {
                    pm.id,
                    pm.name,
                    pm.slug,
                    pm.description,
                    pm.thumbnailImagePath,
                    pm.tags,
                    pm.partCount,
                    pm.likeCount,
                    pm.forkCount,
                    pm.viewCount,
                    pm.publishedDate,
                    pm.tenantGuid
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                items = mocs,
                totalCount,
                pageNumber,
                pageSize,
                query = q
            });
        }


        /// <summary>
        ///
        /// GET /api/mochub/moc/{id}
        ///
        /// Get the full detail of a published MOC (the "repository" page).
        /// Public MOCs are accessible anonymously; private/unlisted require auth.
        ///
        /// </summary>
        [HttpGet("moc/{id}")]
        [AllowAnonymous]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.Global)]
        public async Task<IActionResult> GetMocDetail(int id, CancellationToken cancellationToken = default)
        {
            PublishedMoc moc = await _context.PublishedMocs
                .Where(pm => pm.id == id && pm.active == true && pm.deleted == false)
                .Include(pm => pm.project)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (moc == null)
            {
                return NotFound("MOC not found.");
            }

            // Only public and unlisted MOCs are accessible anonymously
            if (moc.visibility == "Private")
            {
                return NotFound("MOC not found.");
            }

            // Get the latest version info
            var latestVersion = await _context.MocVersions
                .Where(mv => mv.publishedMocId == id && mv.active == true)
                .OrderByDescending(mv => mv.versionNumber)
                .Select(mv => new
                {
                    mv.versionNumber,
                    mv.commitMessage,
                    mv.snapshotDate,
                    mv.partCount
                })
                .FirstOrDefaultAsync(cancellationToken);

            // Get total version count
            int versionCount = await _context.MocVersions
                .Where(mv => mv.publishedMocId == id && mv.active == true)
                .CountAsync(cancellationToken);

            // Increment view count (fire-and-forget — MUST be after all DbContext queries to avoid threading issues)
            _ = IncrementViewCountAsync(id);

            return Ok(new
            {
                moc.id,
                moc.name,
                moc.slug,
                moc.description,
                moc.readmeMarkdown,
                moc.thumbnailImagePath,
                moc.tags,
                moc.visibility,
                moc.licenseName,
                moc.allowForking,
                moc.partCount,
                moc.likeCount,
                moc.commentCount,
                moc.favouriteCount,
                moc.forkCount,
                moc.viewCount,
                moc.publishedDate,
                moc.isFeatured,
                moc.forkedFromMocId,
                moc.tenantGuid,
                projectId = moc.projectId,
                latestVersion,
                versionCount
            });
        }


        /// <summary>
        ///
        /// GET /api/mochub/moc/{id}/versions
        ///
        /// Get the version history (commit log) for a MOC.
        ///
        /// </summary>
        [HttpGet("moc/{id}/versions")]
        [AllowAnonymous]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.Global)]
        public async Task<IActionResult> GetVersionHistory(
            int id,
            int pageSize = 20,
            int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            pageSize = Math.Clamp(pageSize, 1, 50);
            pageNumber = Math.Max(1, pageNumber);

            // Verify the MOC exists and is publicly accessible
            bool isPublic = await _context.PublishedMocs
                .AnyAsync(pm => pm.id == id
                             && pm.visibility != "Private"
                             && pm.active == true
                             && pm.deleted == false,
                    cancellationToken);

            if (!isPublic)
            {
                return NotFound("MOC not found.");
            }

            int totalCount = await _context.MocVersions
                .Where(mv => mv.publishedMocId == id && mv.active == true)
                .CountAsync(cancellationToken);

            var versions = await _context.MocVersions
                .Where(mv => mv.publishedMocId == id && mv.active == true)
                .OrderByDescending(mv => mv.versionNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(mv => new
                {
                    mv.id,
                    mv.versionNumber,
                    mv.commitMessage,
                    mv.partCount,
                    mv.addedPartCount,
                    mv.removedPartCount,
                    mv.modifiedPartCount,
                    mv.snapshotDate,
                    mv.authorTenantGuid
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                items = versions,
                totalCount,
                pageNumber,
                pageSize
            });
        }


        /// <summary>
        ///
        /// GET /api/mochub/moc/{id}/versions/{versionNum}/diff
        ///
        /// Get a diff between two versions. Defaults to comparing against the previous version.
        ///
        /// </summary>
        [HttpGet("moc/{id}/versions/{versionNum}/diff")]
        [AllowAnonymous]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.Global)]
        public async Task<IActionResult> GetVersionDiff(
            int id,
            int versionNum,
            int? compareWith = null,
            CancellationToken cancellationToken = default)
        {
            int fromVersion = compareWith ?? (versionNum - 1);

            if (fromVersion < 1)
            {
                return BadRequest("Cannot diff the first version — there is no prior version to compare against.");
            }

            try
            {
                var diff = await _versioningService.ComputeDiffSummaryAsync(id, fromVersion, versionNum, cancellationToken);
                return Ok(diff);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }


        /// <summary>
        ///
        /// GET /api/mochub/moc/{id}/versions/{versionNum}/mpd
        ///
        /// Returns the raw MPD snapshot text for a specific version.
        /// Used by the client 3D viewer to render a historical version of a MOC.
        ///
        /// </summary>
        [HttpGet("moc/{id}/versions/{versionNum}/mpd")]
        [AllowAnonymous]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.Global)]
        public async Task<IActionResult> GetVersionMpd(
            int id,
            int versionNum,
            CancellationToken cancellationToken = default)
        {
            //
            // Verify the MOC is publicly accessible
            //
            bool isPublic = await _context.PublishedMocs
                .AnyAsync(pm => pm.id == id
                             && pm.visibility != "Private"
                             && pm.active == true
                             && pm.deleted == false,
                    cancellationToken);

            if (isPublic == false)
            {
                return NotFound("MOC not found.");
            }

            string mpdSnapshot = await _versioningService.GetVersionMpdAsync(id, versionNum, cancellationToken);

            if (string.IsNullOrEmpty(mpdSnapshot))
            {
                return NotFound("Version not found.");
            }

            return Content(mpdSnapshot, "text/plain");
        }


        /// <summary>
        ///
        /// GET /api/mochub/moc/{id}/versions/{versionNum}/diff-mpd
        ///
        /// Returns a colour-coded diff MPD that visualises changes between two versions.
        /// Added bricks are coloured green (LDraw 2), removed bricks translucent red (LDraw 36),
        /// unchanged bricks keep their original colours.  The output is a valid LDraw MPD file
        /// that can be loaded directly by LDrawLoader for 3D rendering.
        ///
        /// </summary>
        [HttpGet("moc/{id}/versions/{versionNum}/diff-mpd")]
        [AllowAnonymous]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.Global)]
        public async Task<IActionResult> GetDiffMpd(
            int id,
            int versionNum,
            int? compareWith = null,
            CancellationToken cancellationToken = default)
        {
            int fromVersion = compareWith ?? (versionNum - 1);

            if (fromVersion < 1)
            {
                return BadRequest("Cannot generate diff for the first version — no prior version.");
            }

            //
            // Verify the MOC is publicly accessible
            //
            bool isPublic = await _context.PublishedMocs
                .AnyAsync(pm => pm.id == id
                             && pm.visibility != "Private"
                             && pm.active == true
                             && pm.deleted == false,
                    cancellationToken);

            if (isPublic == false)
            {
                return NotFound("MOC not found.");
            }

            try
            {
                string diffMpd = await _versioningService.GenerateDiffMpdAsync(
                    id, fromVersion, versionNum, cancellationToken);

                return Content(diffMpd, "text/plain");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        ///
        /// GET /api/mochub/moc/{id}/forks
        ///
        /// Get the list of forks for a MOC.
        ///
        /// </summary>
        [HttpGet("moc/{id}/forks")]
        [AllowAnonymous]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.Global)]
        public async Task<IActionResult> GetForks(int id, CancellationToken cancellationToken = default)
        {
            var forks = await _context.MocForks
                .Where(mf => mf.sourceMocId == id && mf.active == true)
                .Select(mf => new
                {
                    mf.forkedMocId,
                    forkedMocName = _context.PublishedMocs
                        .Where(pm => pm.id == mf.forkedMocId)
                        .Select(pm => pm.name)
                        .FirstOrDefault(),
                    forkedMocSlug = _context.PublishedMocs
                        .Where(pm => pm.id == mf.forkedMocId)
                        .Select(pm => pm.slug)
                        .FirstOrDefault(),
                    mf.forkerTenantGuid,
                    mf.forkedDate,
                    sourceVersionNumber = mf.mocVersion != null ? mf.mocVersion.versionNumber : (int?)null
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Ok(forks);
        }


        /// <summary>
        ///
        /// DELETE /api/mochub/moc/{id}
        ///
        /// Unpublish (soft-delete) a MOC.  Owner-only.
        ///
        /// </summary>
        [HttpDelete("moc/{id}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.Global)]
        public async Task<IActionResult> UnpublishMoc(int id, CancellationToken cancellationToken = default)
        {
            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);

            if (securityUser == null)
            {
                return Unauthorized("Not authenticated.");
            }

            Guid ownerGuid;
            try
            {
                ownerGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch
            {
                return Problem("Your user account is not configured with a tenant.");
            }

            var moc = await _context.PublishedMocs
                .FirstOrDefaultAsync(pm => pm.id == id
                                        && pm.tenantGuid == ownerGuid
                                        && pm.deleted == false,
                    cancellationToken);

            if (moc == null)
            {
                return NotFound("MOC not found or you do not have permission to delete it.");
            }

            moc.deleted = true;
            moc.active = false;
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }


        /// <summary>
        ///
        /// GET /api/mochub/moc/{id}/thumbnail
        ///
        /// Returns the thumbnail image for a public/unlisted MOC (served from the project's binary thumbnail data).
        /// Cached for 10 minutes to reduce database pressure on the explore page.
        ///
        /// </summary>
        [HttpGet("moc/{id}/thumbnail")]
        [AllowAnonymous]
        [RateLimit(RateLimitOption.TenPerSecond, Scope = RateLimitScope.Global)]
        [ResponseCache(Duration = 600)]
        public async Task<IActionResult> GetThumbnail(int id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"mochub_thumb_{id}";

            if (_cache.TryGetValue(cacheKey, out byte[] cachedData))
            {
                if (cachedData == null || cachedData.Length == 0)
                {
                    return NotFound();
                }
                return File(cachedData, "image/png");
            }

            var moc = await _context.PublishedMocs
                .Where(pm => pm.id == id
                          && pm.visibility != "Private"
                          && pm.active == true
                          && pm.deleted == false)
                .Select(pm => new { pm.projectId, pm.tenantGuid })
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (moc == null)
            {
                return NotFound();
            }

            byte[] thumbnailData = await _context.Projects
                .Where(p => p.id == moc.projectId
                         && p.tenantGuid == moc.tenantGuid
                         && p.active == true)
                .Select(p => p.thumbnailData)
                .FirstOrDefaultAsync(cancellationToken);

            _cache.Set(cacheKey, thumbnailData, TimeSpan.FromMinutes(10));

            if (thumbnailData == null || thumbnailData.Length == 0)
            {
                return NotFound();
            }

            return File(thumbnailData, "image/png");
        }


        // ────────────────────────────────────────────────────────────────
        //  Authenticated Endpoints
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        ///
        /// POST /api/mochub/publish
        ///
        /// Publish an existing project as a MOC on MOCHub.
        /// Creates a PublishedMoc record and an initial version snapshot.
        ///
        /// </summary>
        [HttpPost("publish")]
        [RateLimit(RateLimitOption.OnePerSecond, Scope = RateLimitScope.PerUser)]
        public async Task<IActionResult> PublishMoc(
            [FromBody] PublishMocRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            StartAuditEventClock();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false
                && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception)
            {
                return Problem("Your user account is not configured with a tenant.");
            }

            //
            // Verify the project exists and belongs to the user
            //
            Project project = await _context.Projects
                .Where(p => p.id == request.ProjectId
                         && p.tenantGuid == userTenantGuid
                         && p.active == true
                         && p.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (project == null)
            {
                return NotFound("Project not found.");
            }

            //
            // Generate slug and ensure uniqueness
            //
            string slug = MocVersioningService.GenerateSlug(request.Name ?? project.name);

            string baseSlug = slug;
            int slugSuffix = 1;
            while (await _context.PublishedMocs.AnyAsync(
                pm => pm.tenantGuid == userTenantGuid && pm.slug == slug && pm.active == true,
                cancellationToken))
            {
                slug = $"{baseSlug}-{slugSuffix}";
                slugSuffix++;
            }

            //
            // Create the PublishedMoc
            //
            PublishedMoc moc = new PublishedMoc
            {
                projectId = request.ProjectId,
                tenantGuid = userTenantGuid,
                name = request.Name ?? project.name,
                description = request.Description ?? project.description,
                tags = request.Tags,
                visibility = request.Visibility ?? "Public",
                licenseName = request.LicenseName ?? "AllRightsReserved",
                readmeMarkdown = request.ReadmeMarkdown,
                slug = slug,
                allowForking = request.AllowForking ?? true,
                isPublished = true,
                publishedDate = DateTime.UtcNow,
                partCount = project.partCount,
                thumbnailImagePath = project.thumbnailImagePath,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.PublishedMocs.Add(moc);
            await _context.SaveChangesAsync(cancellationToken);

            //
            // Create the initial version snapshot
            //
            var snapshot = await _versioningService.CreateSnapshotAsync(
                moc.id,
                request.CommitMessage ?? "Initial publish",
                userTenantGuid,
                cancellationToken);

            await CreateAuditEventAsync(
                AuditEngine.AuditType.CreateEntity,
                $"MOCHub: Published MOC '{moc.name}' (id={moc.id}), slug='{slug}'",
                moc.id.ToString());

            return Ok(new
            {
                publishedMocId = moc.id,
                slug,
                versionNumber = snapshot.VersionNumber,
                partCount = snapshot.PartCount
            });
        }


        /// <summary>
        ///
        /// POST /api/mochub/moc/{id}/commit
        ///
        /// Create a new version snapshot (commit) of the MOC's current state.
        ///
        /// </summary>
        [HttpPost("moc/{id}/commit")]
        [RateLimit(RateLimitOption.OnePerSecond, Scope = RateLimitScope.PerUser)]
        public async Task<IActionResult> CommitVersion(
            int id,
            [FromBody] CommitRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CommitMessage))
            {
                return BadRequest("A commit message is required.");
            }

            StartAuditEventClock();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false
                && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception)
            {
                return Problem("Your user account is not configured with a tenant.");
            }

            //
            // Verify ownership or collaborator access
            //
            PublishedMoc moc = await _context.PublishedMocs
                .Where(pm => pm.id == id && pm.active == true && pm.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (moc == null)
            {
                return NotFound("MOC not found.");
            }

            bool isOwner = moc.tenantGuid == userTenantGuid;
            bool isCollaborator = !isOwner && await _context.MocCollaborators
                .AnyAsync(mc => mc.publishedMocId == id
                             && mc.collaboratorTenantGuid == userTenantGuid
                             && (mc.accessLevel == "Write" || mc.accessLevel == "Admin")
                             && mc.isAccepted == true
                             && mc.active == true,
                    cancellationToken);

            if (!isOwner && !isCollaborator)
            {
                return Forbid();
            }

            try
            {
                var snapshot = await _versioningService.CreateSnapshotAsync(
                    id,
                    request.CommitMessage,
                    userTenantGuid,
                    cancellationToken);

                await CreateAuditEventAsync(
                    AuditEngine.AuditType.UpdateEntity,
                    $"MOCHub: Committed version {snapshot.VersionNumber} for MOC {id}",
                    id.ToString());

                return Ok(snapshot);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        ///
        /// POST /api/mochub/moc/{id}/fork
        ///
        /// Fork a MOC into the current user's account.
        ///
        /// </summary>
        [HttpPost("moc/{id}/fork")]
        [RateLimit(RateLimitOption.OnePerSecond, Scope = RateLimitScope.PerUser)]
        public async Task<IActionResult> ForkMoc(int id, CancellationToken cancellationToken = default)
        {
            StartAuditEventClock();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false
                && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception)
            {
                return Problem("Your user account is not configured with a tenant.");
            }

            try
            {
                var result = await _versioningService.ForkMocAsync(id, userTenantGuid, cancellationToken);

                await CreateAuditEventAsync(
                    AuditEngine.AuditType.CreateEntity,
                    $"MOCHub: Forked MOC {id} → {result.NewPublishedMocId}",
                    result.NewPublishedMocId.ToString());

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        ///
        /// PUT /api/mochub/moc/{id}
        ///
        /// Update MOC metadata (name, description, visibility, license, tags, README).
        ///
        /// </summary>
        [HttpPut("moc/{id}")]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        public async Task<IActionResult> UpdateMocMetadata(
            int id,
            [FromBody] UpdateMocRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            StartAuditEventClock();

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false
                && await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception)
            {
                return Problem("Your user account is not configured with a tenant.");
            }

            PublishedMoc moc = await _context.PublishedMocs
                .Where(pm => pm.id == id
                          && pm.tenantGuid == userTenantGuid
                          && pm.active == true
                          && pm.deleted == false)
                .FirstOrDefaultAsync(cancellationToken);

            if (moc == null)
            {
                return NotFound("MOC not found.");
            }

            //
            // Apply updates
            //
            if (request.Name != null) moc.name = request.Name;
            if (request.Description != null) moc.description = request.Description;
            if (request.Tags != null) moc.tags = request.Tags;
            if (request.Visibility != null) moc.visibility = request.Visibility;
            if (request.LicenseName != null) moc.licenseName = request.LicenseName;
            if (request.ReadmeMarkdown != null) moc.readmeMarkdown = request.ReadmeMarkdown;
            if (request.AllowForking.HasValue) moc.allowForking = request.AllowForking.Value;

            await _context.SaveChangesAsync(cancellationToken);

            await CreateAuditEventAsync(
                AuditEngine.AuditType.UpdateEntity,
                $"MOCHub: Updated metadata for MOC '{moc.name}' (id={id})",
                id.ToString());

            return Ok(new { moc.id, moc.name, moc.slug, moc.visibility });
        }


        // ────────────────────────────────────────────────────────────────
        //  Request DTOs
        // ────────────────────────────────────────────────────────────────

        public class PublishMocRequest
        {
            public int ProjectId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Tags { get; set; }
            public string Visibility { get; set; }
            public string LicenseName { get; set; }
            public string ReadmeMarkdown { get; set; }
            public string CommitMessage { get; set; }
            public bool? AllowForking { get; set; }
        }


        public class CommitRequest
        {
            public string CommitMessage { get; set; }
        }


        public class UpdateMocRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Tags { get; set; }
            public string Visibility { get; set; }
            public string LicenseName { get; set; }
            public string ReadmeMarkdown { get; set; }
            public bool? AllowForking { get; set; }
        }


        // ────────────────────────────────────────────────────────────────
        //  Helpers
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Fire-and-forget view count increment.
        /// </summary>
        private async Task IncrementViewCountAsync(int publishedMocId)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE [BMC].[PublishedMoc] SET [viewCount] = [viewCount] + 1 WHERE [id] = {0}",
                    publishedMocId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to increment view count for MOC {Id}", publishedMocId);
            }
        }
    }
}
