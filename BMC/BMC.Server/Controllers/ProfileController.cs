using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
    /// Avatar and banner images are stored as binary data (byte[]) in the
    /// database, following the same pattern used by Contact, Client,
    /// Resource, and other Scheduler entities.
    ///
    /// All endpoints are tenant-scoped and require at minimum BMC read permission.
    /// Write operations require the "BMC Community Writer" custom role.
    /// </summary>
    public class ProfileController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
        private const long MAX_AVATAR_SIZE = 2 * 1024 * 1024;   // 2 MB
        private const long MAX_BANNER_SIZE = 5 * 1024 * 1024;   // 5 MB

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
            public bool hasAvatar { get; set; }
            public string avatarUrl { get; set; }
            public bool hasBanner { get; set; }
            public string bannerUrl { get; set; }
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

            // Nested preferred themes
            public List<PreferredThemeDto> preferredThemes { get; set; } = new();
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
        /// Images are uploaded separately via dedicated endpoints.
        /// </summary>
        public class UpdateProfileRequest
        {
            public string displayName { get; set; }
            public string bio { get; set; }
            public string location { get; set; }
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

        /// <summary>
        /// Preferred theme DTO with theme name denormalized.
        /// </summary>
        public class PreferredThemeDto
        {
            public int id { get; set; }
            public int legoThemeId { get; set; }
            public string themeName { get; set; }
            public int? sequence { get; set; }
        }

        /// <summary>
        /// Request body for PUT /api/profile/mine/preferred-themes — array of these.
        /// </summary>
        public class SavePreferredThemeRequest
        {
            public int legoThemeId { get; set; }
            public int? sequence { get; set; }
        }

        #endregion


        #region Profile CRUD

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
            // Load preferred themes with theme name
            //
            List<PreferredThemeDto> preferredThemes = await _context.UserProfilePreferredThemes
                .Where(pt => pt.userProfileId == profile.id && pt.active == true && pt.deleted == false)
                .OrderBy(pt => pt.sequence)
                .Select(pt => new PreferredThemeDto
                {
                    id = pt.id,
                    legoThemeId = pt.legoThemeId,
                    themeName = pt.legoTheme.name,
                    sequence = pt.sequence
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
                hasAvatar = profile.avatarData != null && profile.avatarData.Length > 0,
                avatarUrl = (profile.avatarData != null && profile.avatarData.Length > 0) ? "/api/profile/mine/avatar" : null,
                hasBanner = profile.bannerData != null && profile.bannerData.Length > 0,
                bannerUrl = (profile.bannerData != null && profile.bannerData.Length > 0) ? "/api/profile/mine/banner" : null,
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
                links = links,
                preferredThemes = preferredThemes
            };

            return Ok(dto);
        }


        /// <summary>
        /// PUT /api/profile/mine
        ///
        /// Updates the current user's profile fields.
        /// Images are uploaded separately via POST avatar/banner endpoints.
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
            profile.websiteUrl = request.websiteUrl?.Trim() ?? "";
            profile.isPublic = request.isPublic;

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { action = "updated" });
        }

        #endregion


        #region Image Upload / Serve / Delete

        /// <summary>
        /// POST /api/profile/mine/avatar
        ///
        /// Upload or replace the user's avatar image.
        /// Accepts multipart/form-data with a single file field named "file".
        /// Max size: 2 MB. Must be an image MIME type.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/profile/mine/avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken cancellationToken = default)
        {
            return await UploadImage(file, "avatar", MAX_AVATAR_SIZE, cancellationToken);
        }


        /// <summary>
        /// POST /api/profile/mine/banner
        ///
        /// Upload or replace the user's banner image.
        /// Accepts multipart/form-data with a single file field named "file".
        /// Max size: 5 MB. Must be an image MIME type.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/profile/mine/banner")]
        public async Task<IActionResult> UploadBanner(IFormFile file, CancellationToken cancellationToken = default)
        {
            return await UploadImage(file, "banner", MAX_BANNER_SIZE, cancellationToken);
        }


        /// <summary>
        /// GET /api/profile/mine/avatar
        ///
        /// Serves the user's avatar image with correct MIME type.
        /// Returns 404 if no avatar is set.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/profile/mine/avatar")]
        public async Task<IActionResult> GetAvatar(CancellationToken cancellationToken = default)
        {
            return await ServeImage("avatar", cancellationToken);
        }


        /// <summary>
        /// GET /api/profile/mine/banner
        ///
        /// Serves the user's banner image with correct MIME type.
        /// Returns 404 if no banner is set.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/profile/mine/banner")]
        public async Task<IActionResult> GetBanner(CancellationToken cancellationToken = default)
        {
            return await ServeImage("banner", cancellationToken);
        }


        /// <summary>
        /// DELETE /api/profile/mine/avatar
        ///
        /// Removes the user's avatar image.
        /// </summary>
        [HttpDelete]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/profile/mine/avatar")]
        public async Task<IActionResult> DeleteAvatar(CancellationToken cancellationToken = default)
        {
            return await RemoveImage("avatar", cancellationToken);
        }


        /// <summary>
        /// DELETE /api/profile/mine/banner
        ///
        /// Removes the user's banner image.
        /// </summary>
        [HttpDelete]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/profile/mine/banner")]
        public async Task<IActionResult> DeleteBanner(CancellationToken cancellationToken = default)
        {
            return await RemoveImage("banner", cancellationToken);
        }


        /// <summary>
        /// Shared upload logic for avatar and banner.
        /// </summary>
        private async Task<IActionResult> UploadImage(IFormFile file, string imageType, long maxSize, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided.");
            }

            if (file.Length > maxSize)
            {
                return BadRequest($"File exceeds the maximum size of {maxSize / (1024 * 1024)} MB.");
            }

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("File must be an image (image/* MIME type).");
            }

            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            UserProfile profile = await GetUserProfileAsync(cancellationToken);
            if (profile == null)
            {
                return NotFound("Profile not found.");
            }

            // Read file bytes
            byte[] data;
            using (var ms = new System.IO.MemoryStream())
            {
                await file.CopyToAsync(ms, cancellationToken);
                data = ms.ToArray();
            }

            if (imageType == "avatar")
            {
                profile.avatarFileName = file.FileName;
                profile.avatarSize = file.Length;
                profile.avatarData = data;
                profile.avatarMimeType = file.ContentType;
            }
            else
            {
                profile.bannerFileName = file.FileName;
                profile.bannerSize = file.Length;
                profile.bannerData = data;
                profile.bannerMimeType = file.ContentType;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { action = "uploaded", imageType, fileName = file.FileName, size = file.Length });
        }


        /// <summary>
        /// Shared serve logic for avatar and banner.
        /// </summary>
        private async Task<IActionResult> ServeImage(string imageType, CancellationToken cancellationToken)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            UserProfile profile = await GetUserProfileAsync(cancellationToken);
            if (profile == null)
            {
                return NotFound();
            }

            byte[] data;
            string mimeType;
            string fileName;

            if (imageType == "avatar")
            {
                data = profile.avatarData;
                mimeType = profile.avatarMimeType;
                fileName = profile.avatarFileName;
            }
            else
            {
                data = profile.bannerData;
                mimeType = profile.bannerMimeType;
                fileName = profile.bannerFileName;
            }

            if (data == null || data.Length == 0)
            {
                return NotFound();
            }

            return File(data, mimeType ?? "image/png", fileName);
        }


        /// <summary>
        /// Shared delete logic for avatar and banner.
        /// </summary>
        private async Task<IActionResult> RemoveImage(string imageType, CancellationToken cancellationToken)
        {
            if (await DoesUserHaveCustomRoleSecurityCheckAsync("BMC Community Writer", cancellationToken) == false &&
                await DoesUserHaveAdminPrivilegeSecurityCheckAsync(cancellationToken) == false)
            {
                return Forbid();
            }

            UserProfile profile = await GetUserProfileAsync(cancellationToken);
            if (profile == null)
            {
                return NotFound("Profile not found.");
            }

            if (imageType == "avatar")
            {
                profile.avatarFileName = null;
                profile.avatarSize = null;
                profile.avatarData = null;
                profile.avatarMimeType = null;
            }
            else
            {
                profile.bannerFileName = null;
                profile.bannerSize = null;
                profile.bannerData = null;
                profile.bannerMimeType = null;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { action = "removed", imageType });
        }


        /// <summary>
        /// Helper: Gets the current user's profile entity.
        /// </summary>
        private async Task<UserProfile> GetUserProfileAsync(CancellationToken cancellationToken)
        {
            SecurityUser securityUser = await GetSecurityUserAsync(cancellationToken);
            Guid userTenantGuid;

            try
            {
                userTenantGuid = await UserTenantGuidAsync(securityUser, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }

            return await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.tenantGuid == userTenantGuid && p.active == true && p.deleted == false, cancellationToken);
        }

        #endregion


        #region Links

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

        #endregion


        #region Link Types

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

        #endregion


        #region Preferred Themes

        /// <summary>
        /// GET /api/profile/mine/preferred-themes
        ///
        /// Returns the user's preferred LEGO themes.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/profile/mine/preferred-themes")]
        public async Task<IActionResult> GetMyPreferredThemes(CancellationToken cancellationToken = default)
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
                return Ok(new List<PreferredThemeDto>());
            }

            List<PreferredThemeDto> themes = await _context.UserProfilePreferredThemes
                .Where(pt => pt.userProfileId == profile.id && pt.active == true && pt.deleted == false)
                .OrderBy(pt => pt.sequence)
                .Select(pt => new PreferredThemeDto
                {
                    id = pt.id,
                    legoThemeId = pt.legoThemeId,
                    themeName = pt.legoTheme.name,
                    sequence = pt.sequence
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Ok(themes);
        }


        /// <summary>
        /// PUT /api/profile/mine/preferred-themes
        ///
        /// Bulk-saves preferred themes (soft-delete existing + recreate).
        /// This is simpler than individual add/remove for the settings form.
        /// </summary>
        [HttpPut]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/profile/mine/preferred-themes")]
        public async Task<IActionResult> SaveMyPreferredThemes([FromBody] List<SavePreferredThemeRequest> request, CancellationToken cancellationToken = default)
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
            // Soft-delete all existing preferred themes for this profile
            //
            List<UserProfilePreferredTheme> existingThemes = await _context.UserProfilePreferredThemes
                .Where(pt => pt.userProfileId == profile.id && pt.deleted == false)
                .ToListAsync(cancellationToken);

            foreach (var theme in existingThemes)
            {
                theme.deleted = true;
                theme.active = false;
            }

            //
            // Validate theme IDs exist
            //
            List<int> themeIds = request.Select(r => r.legoThemeId).Distinct().ToList();

            List<int> validThemeIds = await _context.LegoThemes
                .Where(t => themeIds.Contains(t.id) && t.active == true && t.deleted == false)
                .Select(t => t.id)
                .ToListAsync(cancellationToken);

            //
            // Create new preferred theme records
            //
            int seq = 0;
            foreach (var item in request)
            {
                if (!validThemeIds.Contains(item.legoThemeId))
                {
                    continue; // Skip invalid theme IDs
                }

                var newPref = new UserProfilePreferredTheme
                {
                    tenantGuid = userTenantGuid,
                    userProfileId = profile.id,
                    legoThemeId = item.legoThemeId,
                    sequence = item.sequence ?? seq,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.UserProfilePreferredThemes.Add(newPref);
                seq++;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { action = "saved", themeCount = request.Count(r => validThemeIds.Contains(r.legoThemeId)) });
        }

        #endregion


        #region Public Profile (Unauthenticated)

        /// <summary>
        /// GET /api/profile/{id}
        ///
        /// Returns a public profile by its ID.
        /// No authentication required — returns 404 if profile is not public.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerClientIp)]
        [Route("api/profile/{id:int}")]
        public async Task<IActionResult> GetPublicProfile(int id, CancellationToken cancellationToken = default)
        {
            //
            // Look for active, non-deleted, public profile
            //
            UserProfile profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.id == id && p.isPublic == true && p.active == true && p.deleted == false, cancellationToken);

            if (profile == null)
            {
                return NotFound();
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
            // Load preferred themes with theme name
            //
            List<PreferredThemeDto> preferredThemes = await _context.UserProfilePreferredThemes
                .Where(pt => pt.userProfileId == profile.id && pt.active == true && pt.deleted == false)
                .OrderBy(pt => pt.sequence)
                .Select(pt => new PreferredThemeDto
                {
                    id = pt.id,
                    legoThemeId = pt.legoThemeId,
                    themeName = pt.legoTheme.name,
                    sequence = pt.sequence
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            //
            // Build public profile DTO — same shape as ProfileDto but with public image URLs
            //
            var dto = new ProfileDto
            {
                id = profile.id,
                displayName = profile.displayName,
                bio = profile.bio,
                location = profile.location,
                hasAvatar = profile.avatarData != null && profile.avatarData.Length > 0,
                avatarUrl = (profile.avatarData != null && profile.avatarData.Length > 0) ? $"/api/profile/{profile.id}/avatar" : null,
                hasBanner = profile.bannerData != null && profile.bannerData.Length > 0,
                bannerUrl = (profile.bannerData != null && profile.bannerData.Length > 0) ? $"/api/profile/{profile.id}/banner" : null,
                websiteUrl = profile.websiteUrl,
                isPublic = true,
                memberSinceDate = profile.memberSinceDate,
                totalPartsOwned = profileStat?.totalPartsOwned ?? 0,
                totalUniquePartsOwned = profileStat?.totalUniquePartsOwned ?? 0,
                totalSetsOwned = profileStat?.totalSetsOwned ?? 0,
                totalMocsPublished = profileStat?.totalMocsPublished ?? 0,
                totalFollowers = profileStat?.totalFollowers ?? 0,
                totalFollowing = profileStat?.totalFollowing ?? 0,
                totalLikesReceived = profileStat?.totalLikesReceived ?? 0,
                totalAchievementPoints = profileStat?.totalAchievementPoints ?? 0,
                links = links,
                preferredThemes = preferredThemes
            };

            return Ok(dto);
        }


        /// <summary>
        /// GET /api/profile/{id}/avatar
        ///
        /// Serves a public profile's avatar image.
        /// No authentication required — returns 404 if profile is not public or has no avatar.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerClientIp)]
        [Route("api/profile/{id:int}/avatar")]
        public async Task<IActionResult> GetPublicAvatar(int id, CancellationToken cancellationToken = default)
        {
            return await ServePublicImage(id, "avatar", cancellationToken);
        }


        /// <summary>
        /// GET /api/profile/{id}/banner
        ///
        /// Serves a public profile's banner image.
        /// No authentication required — returns 404 if profile is not public or has no banner.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerClientIp)]
        [Route("api/profile/{id:int}/banner")]
        public async Task<IActionResult> GetPublicBanner(int id, CancellationToken cancellationToken = default)
        {
            return await ServePublicImage(id, "banner", cancellationToken);
        }


        /// <summary>
        /// Shared serve logic for public avatar and banner.
        /// Checks isPublic — returns 404 for private profiles.
        /// </summary>
        private async Task<IActionResult> ServePublicImage(int profileId, string imageType, CancellationToken cancellationToken)
        {
            UserProfile profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.id == profileId && p.isPublic == true && p.active == true && p.deleted == false, cancellationToken);

            if (profile == null)
            {
                return NotFound();
            }

            byte[] data;
            string mimeType;
            string fileName;

            if (imageType == "avatar")
            {
                data = profile.avatarData;
                mimeType = profile.avatarMimeType;
                fileName = profile.avatarFileName;
            }
            else
            {
                data = profile.bannerData;
                mimeType = profile.bannerMimeType;
                fileName = profile.bannerFileName;
            }

            if (data == null || data.Length == 0)
            {
                return NotFound();
            }

            return File(data, mimeType ?? "image/png", fileName);
        }

        #endregion
    }
}
