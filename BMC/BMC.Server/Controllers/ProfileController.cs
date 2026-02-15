using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Foundation.Controllers;
using Foundation.Security;
using Foundation.Security.Database;
using Foundation.BMC.Database;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// Custom composite controller for community User Profile workflows.
    /// Provides higher-level endpoints that aggregate across UserProfile,
    /// UserProfileLink, UserProfileLinkType, and UserProfileStat tables
    /// to power the profile page and settings UI.
    ///
    /// All endpoints are tenant-scoped and require at minimum BMC read permission.
    /// Write operations require the "BMC Community Writer" custom role.
    /// </summary>
    public class ProfileController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;

        private readonly BMCContext _context;
        private readonly ILogger<ProfileController> _logger;


        public ProfileController(BMCContext context, ILogger<ProfileController> logger) : base("BMC", "Profile")
        {
            _context = context;
            _logger = logger;

            _context.Database.SetCommandTimeout(60);
        }


        #region DTOs

        /// <summary>
        /// Full profile DTO returned by GET /api/profile/mine.
        /// Includes flattened stats and social links.
        /// </summary>
        public class ProfileDto
        {
            public int id { get; set; }
            public string displayName { get; set; }
            public string bio { get; set; }
            public string location { get; set; }
            public string avatarImagePath { get; set; }
            public string profileBannerImagePath { get; set; }
            public string websiteUrl { get; set; }
            public bool isPublic { get; set; }
            public DateTime? memberSinceDate { get; set; }

            // Flattened stats
            public int totalPartsOwned { get; set; }
            public int totalUniquePartsOwned { get; set; }
            public int totalSetsOwned { get; set; }
            public int totalMocsPublished { get; set; }
            public int totalFollowers { get; set; }
            public int totalFollowing { get; set; }
            public int totalLikesReceived { get; set; }
            public int totalAchievementPoints { get; set; }

            // Nested links
            public List<ProfileLinkDto> links { get; set; } = new();
        }

        /// <summary>
        /// Profile link DTO with link type info denormalized.
        /// </summary>
        public class ProfileLinkDto
        {
            public int id { get; set; }
            public int userProfileLinkTypeId { get; set; }
            public string linkTypeName { get; set; }
            public string iconCssClass { get; set; }
            public string url { get; set; }
            public string displayLabel { get; set; }
            public int? sequence { get; set; }
        }

        /// <summary>
        /// Request body for PUT /api/profile/mine.
        /// </summary>
        public class UpdateProfileRequest
        {
            public string displayName { get; set; }
            public string bio { get; set; }
            public string location { get; set; }
            public string avatarImagePath { get; set; }
            public string profileBannerImagePath { get; set; }
            public string websiteUrl { get; set; }
            public bool isPublic { get; set; }
        }

        /// <summary>
        /// Request body for PUT /api/profile/mine/links — array of these.
        /// </summary>
        public class SaveLinkRequest
        {
            public int userProfileLinkTypeId { get; set; }
            public string url { get; set; }
            public string displayLabel { get; set; }
            public int? sequence { get; set; }
        }

        #endregion


        /// <summary>
        /// GET /api/profile/mine
        ///
        /// Returns the current user's profile with stats and links.
        /// If no profile exists, auto-creates one (like CollectionController).
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/profile/mine")]
        public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            //
            // Look for existing profile
            //
            UserProfile profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.tenantGuid == userTenantGuid && p.active == true && p.deleted == false, cancellationToken);

            //
            // Auto-create if no profile exists
            //
            if (profile == null)
            {
                profile = new UserProfile
                {
                    tenantGuid = userTenantGuid,
                    displayName = securityUser.accountName ?? "New Builder",
                    bio = "",
                    location = "",
                    avatarImagePath = "",
                    profileBannerImagePath = "",
                    websiteUrl = "",
                    isPublic = true,
                    memberSinceDate = DateTime.UtcNow,
                    versionNumber = 1,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync(cancellationToken);

                // Also create default stats row
                var stats = new UserProfileStat
                {
                    tenantGuid = userTenantGuid,
                    userProfileId = profile.id,
                    totalPartsOwned = 0,
                    totalUniquePartsOwned = 0,
                    totalSetsOwned = 0,
                    totalMocsPublished = 0,
                    totalFollowers = 0,
                    totalFollowing = 0,
                    totalLikesReceived = 0,
                    totalAchievementPoints = 0,
                    lastCalculatedDate = DateTime.UtcNow,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.UserProfileStats.Add(stats);
                await _context.SaveChangesAsync(cancellationToken);
            }

            //
            // Load stats
            //
            UserProfileStat profileStat = await _context.UserProfileStats
                .FirstOrDefaultAsync(s => s.userProfileId == profile.id && s.active == true && s.deleted == false, cancellationToken);

            //
            // Load links with type info
            //
            List<ProfileLinkDto> links = await _context.UserProfileLinks
                .Where(l => l.userProfileId == profile.id && l.active == true && l.deleted == false)
                .OrderBy(l => l.sequence)
                .Select(l => new ProfileLinkDto
                {
                    id = l.id,
                    userProfileLinkTypeId = l.userProfileLinkTypeId,
                    linkTypeName = l.userProfileLinkType.name,
                    iconCssClass = l.userProfileLinkType.iconCssClass,
                    url = l.url,
                    displayLabel = l.displayLabel,
                    sequence = l.sequence
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            //
            // Build composite DTO
            //
            var dto = new ProfileDto
            {
                id = profile.id,
                displayName = profile.displayName,
                bio = profile.bio,
                location = profile.location,
                avatarImagePath = profile.avatarImagePath,
                profileBannerImagePath = profile.profileBannerImagePath,
                websiteUrl = profile.websiteUrl,
                isPublic = profile.isPublic,
                memberSinceDate = profile.memberSinceDate,
                totalPartsOwned = profileStat?.totalPartsOwned ?? 0,
                totalUniquePartsOwned = profileStat?.totalUniquePartsOwned ?? 0,
                totalSetsOwned = profileStat?.totalSetsOwned ?? 0,
                totalMocsPublished = profileStat?.totalMocsPublished ?? 0,
                totalFollowers = profileStat?.totalFollowers ?? 0,
                totalFollowing = profileStat?.totalFollowing ?? 0,
                totalLikesReceived = profileStat?.totalLikesReceived ?? 0,
                totalAchievementPoints = profileStat?.totalAchievementPoints ?? 0,
                links = links
            };

            return Ok(dto);
        }


        /// <summary>
        /// PUT /api/profile/mine
        ///
        /// Updates the current user's profile fields.
        /// </summary>
        [HttpPut]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/profile/mine")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return BadRequest();
            }

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            UserProfile profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.tenantGuid == userTenantGuid && p.active == true && p.deleted == false, cancellationToken);

            if (profile == null)
            {
                return NotFound("Profile not found. Visit your profile page first to auto-create it.");
            }

            //
            // Validate display name
            //
            if (string.IsNullOrWhiteSpace(request.displayName))
            {
                return BadRequest("Display name is required.");
            }

            if (request.displayName.Length > 50)
            {
                return BadRequest("Display name must be 50 characters or fewer.");
            }

            //
            // Apply updates
            //
            profile.displayName = request.displayName.Trim();
            profile.bio = request.bio?.Trim() ?? "";
            profile.location = request.location?.Trim() ?? "";
            profile.avatarImagePath = request.avatarImagePath?.Trim() ?? "";
            profile.profileBannerImagePath = request.profileBannerImagePath?.Trim() ?? "";
            profile.websiteUrl = request.websiteUrl?.Trim() ?? "";
            profile.isPublic = request.isPublic;

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { action = "updated" });
        }


        /// <summary>
        /// GET /api/profile/mine/links
        ///
        /// Returns the user's profile links with link type info.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/profile/mine/links")]
        public async Task<IActionResult> GetMyLinks(CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
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
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            UserProfile profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.tenantGuid == userTenantGuid && p.active == true && p.deleted == false, cancellationToken);

            if (profile == null)
            {
                return Ok(new List<ProfileLinkDto>());
            }

            List<ProfileLinkDto> links = await _context.UserProfileLinks
                .Where(l => l.userProfileId == profile.id && l.active == true && l.deleted == false)
                .OrderBy(l => l.sequence)
                .Select(l => new ProfileLinkDto
                {
                    id = l.id,
                    userProfileLinkTypeId = l.userProfileLinkTypeId,
                    linkTypeName = l.userProfileLinkType.name,
                    iconCssClass = l.userProfileLinkType.iconCssClass,
                    url = l.url,
                    displayLabel = l.displayLabel,
                    sequence = l.sequence
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Ok(links);
        }


        /// <summary>
        /// PUT /api/profile/mine/links
        ///
        /// Bulk-saves profile links (soft-delete existing + recreate).
        /// This is simpler than individual add/remove for the settings form.
        /// </summary>
        [HttpPut]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/profile/mine/links")]
        public async Task<IActionResult> SaveMyLinks([FromBody] List<SaveLinkRequest> request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return BadRequest();
            }

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
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
                return Problem("Your user account is not configured with a tenant, so this operation is not allowed.");
            }

            UserProfile profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.tenantGuid == userTenantGuid && p.active == true && p.deleted == false, cancellationToken);

            if (profile == null)
            {
                return NotFound("Profile not found.");
            }

            //
            // Soft-delete all existing links for this profile
            //
            List<UserProfileLink> existingLinks = await _context.UserProfileLinks
                .Where(l => l.userProfileId == profile.id && l.deleted == false)
                .ToListAsync(cancellationToken);

            foreach (var link in existingLinks)
            {
                link.deleted = true;
                link.active = false;
            }

            //
            // Validate link types exist
            //
            List<int> linkTypeIds = request.Select(r => r.userProfileLinkTypeId).Distinct().ToList();

            List<int> validLinkTypeIds = await _context.UserProfileLinkTypes
                .Where(lt => linkTypeIds.Contains(lt.id) && lt.active == true && lt.deleted == false)
                .Select(lt => lt.id)
                .ToListAsync(cancellationToken);

            //
            // Create new links
            //
            int seq = 0;
            foreach (var item in request)
            {
                if (!validLinkTypeIds.Contains(item.userProfileLinkTypeId))
                {
                    continue; // Skip invalid link types
                }

                if (string.IsNullOrWhiteSpace(item.url))
                {
                    continue; // Skip empty URLs
                }

                var newLink = new UserProfileLink
                {
                    tenantGuid = userTenantGuid,
                    userProfileId = profile.id,
                    userProfileLinkTypeId = item.userProfileLinkTypeId,
                    url = item.url.Trim(),
                    displayLabel = item.displayLabel?.Trim() ?? "",
                    sequence = item.sequence ?? seq,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.UserProfileLinks.Add(newLink);
                seq++;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { action = "saved", linkCount = request.Count(r => validLinkTypeIds.Contains(r.userProfileLinkTypeId) && !string.IsNullOrWhiteSpace(r.url)) });
        }


        /// <summary>
        /// GET /api/profile/link-types
        ///
        /// Returns all active link types for the social link selector dropdown.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/profile/link-types")]
        public async Task<IActionResult> GetLinkTypes(CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            var linkTypes = await _context.UserProfileLinkTypes
                .Where(lt => lt.active == true && lt.deleted == false)
                .OrderBy(lt => lt.sequence)
                .Select(lt => new
                {
                    lt.id,
                    lt.name,
                    lt.description,
                    lt.iconCssClass,
                    lt.sequence
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Ok(linkTypes);
        }
    }
}
