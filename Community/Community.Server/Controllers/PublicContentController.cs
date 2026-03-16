using Foundation.Community.Database;
using Foundation.Community.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Foundation.Community.Controllers
{
    /// <summary>
    /// 
    /// Unauthenticated controller serving public content for the Community website.
    /// 
    /// These endpoints are consumed by the Community.Client Angular application
    /// and must NOT require authentication.
    /// 
    /// All queries are scoped to the resolved tenant (from HTTP Host header).
    /// If no tenant is resolved, a 404 is returned with a helpful message.
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class PublicContentController : ControllerBase
    {
        private readonly CommunityContext _context;
        private readonly TenantContext _tenantContext;


        public PublicContentController(CommunityContext context, TenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }


        /// <summary>
        /// Returns 404 if no tenant was resolved from the Host header.
        /// </summary>
        private IActionResult TenantNotResolved()
        {
            return NotFound(new { error = "No community site is configured for this hostname." });
        }


        // ─────────────────────────────────────────────────────────────
        // Pages
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Get a published page by its URL slug.
        /// </summary>
        [HttpGet("Pages/{slug}")]
        public async Task<IActionResult> GetPageBySlug(string slug)
        {
            if (!_tenantContext.IsResolved) return TenantNotResolved();
            var tg = _tenantContext.TenantGuid.Value;

            var page = await _context.Pages
                .Where(p => p.tenantGuid == tg && p.slug == slug && p.isPublished && p.active && !p.deleted)
                .Select(p => new
                {
                    p.title,
                    p.slug,
                    p.body,
                    p.metaDescription,
                    p.featuredImageUrl,
                    p.isPublished,
                    p.publishedDate
                })
                .FirstOrDefaultAsync();

            if (page == null)
            {
                return NotFound(new { error = "Page not found." });
            }

            return Ok(page);
        }

        /// <summary>
        /// Get all published pages (for sitemap/navigation building).
        /// </summary>
        [HttpGet("Pages")]
        public async Task<IActionResult> GetPublishedPages()
        {
            if (!_tenantContext.IsResolved) return TenantNotResolved();
            var tg = _tenantContext.TenantGuid.Value;

            var pages = await _context.Pages
                .Where(p => p.tenantGuid == tg && p.isPublished && p.active && !p.deleted)
                .OrderBy(p => p.sortOrder)
                .ThenBy(p => p.title)
                .Select(p => new
                {
                    p.title,
                    p.slug,
                    p.metaDescription,
                    p.sortOrder,
                    p.publishedDate
                })
                .ToListAsync();

            return Ok(pages);
        }


        // ─────────────────────────────────────────────────────────────
        // Posts
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Get published posts with optional category filter, paginated.
        /// </summary>
        [HttpGet("Posts")]
        public async Task<IActionResult> GetPublishedPosts(
            [FromQuery] string category = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!_tenantContext.IsResolved) return TenantNotResolved();
            var tg = _tenantContext.TenantGuid.Value;

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var query = _context.Posts
                .Include(p => p.postCategory)
                .Where(p => p.tenantGuid == tg && p.isPublished && p.active && !p.deleted);

            //
            // Optional category filter (by slug)
            //
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.postCategory != null && p.postCategory.slug == category);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.publishedDate)
                .ThenByDescending(p => p.id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.title,
                    p.slug,
                    p.excerpt,
                    p.authorName,
                    p.featuredImageUrl,
                    p.isPublished,
                    p.publishedDate,
                    p.isFeatured,
                    category = p.postCategory == null ? null : new
                    {
                        p.postCategory.name,
                        p.postCategory.slug
                    }
                })
                .ToListAsync();

            return Ok(new
            {
                items,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        /// <summary>
        /// Get a published post by its URL slug.
        /// </summary>
        [HttpGet("Posts/{slug}")]
        public async Task<IActionResult> GetPostBySlug(string slug)
        {
            if (!_tenantContext.IsResolved) return TenantNotResolved();
            var tg = _tenantContext.TenantGuid.Value;

            var post = await _context.Posts
                .Include(p => p.postCategory)
                .Where(p => p.tenantGuid == tg && p.slug == slug && p.isPublished && p.active && !p.deleted)
                .Select(p => new
                {
                    p.title,
                    p.slug,
                    p.body,
                    p.excerpt,
                    p.authorName,
                    p.featuredImageUrl,
                    p.metaDescription,
                    p.isPublished,
                    p.publishedDate,
                    p.isFeatured,
                    category = p.postCategory == null ? null : new
                    {
                        p.postCategory.name,
                        p.postCategory.slug
                    }
                })
                .FirstOrDefaultAsync();

            if (post == null)
            {
                return NotFound(new { error = "Post not found." });
            }

            return Ok(post);
        }


        // ─────────────────────────────────────────────────────────────
        // Announcements
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Get active announcements (not expired, ordered by severity and date).
        /// </summary>
        [HttpGet("Announcements")]
        public async Task<IActionResult> GetActiveAnnouncements()
        {
            if (!_tenantContext.IsResolved) return TenantNotResolved();
            var tg = _tenantContext.TenantGuid.Value;

            var now = DateTime.UtcNow;

            var announcements = await _context.Announcements
                .Where(a => a.tenantGuid == tg && a.active && !a.deleted
                    && a.startDate <= now
                    && (a.endDate == null || a.endDate > now))
                .OrderByDescending(a => a.isPinned)
                .ThenByDescending(a => a.severity == "urgent" ? 3 : a.severity == "warning" ? 2 : 1)
                .ThenByDescending(a => a.startDate)
                .Select(a => new
                {
                    a.title,
                    a.body,
                    a.severity,
                    a.startDate,
                    a.endDate,
                    a.isPinned
                })
                .ToListAsync();

            return Ok(announcements);
        }


        // ─────────────────────────────────────────────────────────────
        // Menus
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Get a navigation menu by its location (header, footer, sidebar).
        /// Returns a tree structure with nested children.
        /// </summary>
        [HttpGet("Menu/{location}")]
        public async Task<IActionResult> GetMenuByLocation(string location)
        {
            if (!_tenantContext.IsResolved) return TenantNotResolved();
            var tg = _tenantContext.TenantGuid.Value;

            var menu = await _context.Menus
                .Where(m => m.tenantGuid == tg && m.location == location && m.active && !m.deleted)
                .FirstOrDefaultAsync();

            if (menu == null)
            {
                //
                // Return an empty menu structure instead of 404 so the frontend always has something
                //
                return Ok(new
                {
                    name = location + " Menu",
                    location = location,
                    items = new List<object>()
                });
            }

            //
            // Load all menu items for this menu
            //
            var allItems = await _context.MenuItems
                .Include(mi => mi.page)
                .Where(mi => mi.menuId == menu.id && mi.active && !mi.deleted)
                .OrderBy(mi => mi.sequence)
                .ToListAsync();

            //
            // Build tree from flat list
            //
            var rootItems = allItems
                .Where(mi => mi.parentMenuItemId == null)
                .Select(mi => BuildMenuItemTree(mi, allItems))
                .ToList();

            return Ok(new
            {
                name = menu.name,
                location = menu.location,
                items = rootItems
            });
        }

        private object BuildMenuItemTree(MenuItem item, List<MenuItem> allItems)
        {
            var children = allItems
                .Where(mi => mi.parentMenuItemId == item.id)
                .Select(mi => BuildMenuItemTree(mi, allItems))
                .ToList();

            return new
            {
                label = item.label,
                url = item.url,
                pageSlug = item.page?.slug,
                iconClass = item.iconClass,
                openInNewTab = item.openInNewTab,
                sortOrder = item.sequence ?? 0,
                children = children.Count > 0 ? children : null
            };
        }


        // ─────────────────────────────────────────────────────────────
        // Site Settings
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Get all public site settings (name, tagline, logo, social links, etc.)
        /// as a key-value dictionary.
        /// </summary>
        [HttpGet("Settings")]
        public async Task<IActionResult> GetSiteSettings()
        {
            if (!_tenantContext.IsResolved) return TenantNotResolved();
            var tg = _tenantContext.TenantGuid.Value;

            var settings = await _context.SiteSettings
                .Where(s => s.tenantGuid == tg && s.active && !s.deleted)
                .ToDictionaryAsync(s => s.settingKey, s => s.settingValue ?? "");

            return Ok(settings);
        }


        // ─────────────────────────────────────────────────────────────
        // Gallery
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Get published gallery albums.
        /// </summary>
        [HttpGet("Gallery")]
        public async Task<IActionResult> GetGalleryAlbums()
        {
            if (!_tenantContext.IsResolved) return TenantNotResolved();
            var tg = _tenantContext.TenantGuid.Value;

            var albums = await _context.GalleryAlbums
                .Where(a => a.tenantGuid == tg && a.isPublished && a.active && !a.deleted)
                .OrderBy(a => a.sequence)
                .ThenBy(a => a.title)
                .Select(a => new
                {
                    a.title,
                    a.slug,
                    a.description,
                    a.coverImageUrl,
                    imageCount = a.GalleryImages.Count(i => i.active && !i.deleted)
                })
                .ToListAsync();

            return Ok(albums);
        }

        /// <summary>
        /// Get images for a specific gallery album.
        /// </summary>
        [HttpGet("Gallery/{slug}")]
        public async Task<IActionResult> GetGalleryAlbumBySlug(string slug)
        {
            if (!_tenantContext.IsResolved) return TenantNotResolved();
            var tg = _tenantContext.TenantGuid.Value;

            var album = await _context.GalleryAlbums
                .Where(a => a.tenantGuid == tg && a.slug == slug && a.isPublished && a.active && !a.deleted)
                .Select(a => new
                {
                    a.title,
                    a.slug,
                    a.description,
                    a.coverImageUrl,
                    images = a.GalleryImages
                        .Where(i => i.active && !i.deleted)
                        .OrderBy(i => i.sequence)
                        .Select(i => new
                        {
                            i.imageUrl,
                            i.caption,
                            i.altText,
                            sortOrder = i.sequence ?? 0
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (album == null)
            {
                return NotFound(new { error = "Album not found." });
            }

            return Ok(album);
        }


        // ─────────────────────────────────────────────────────────────
        // Documents
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Get published documents for download, optionally filtered by category.
        /// </summary>
        [HttpGet("Documents")]
        public async Task<IActionResult> GetDocuments([FromQuery] string category = null)
        {
            if (!_tenantContext.IsResolved) return TenantNotResolved();
            var tg = _tenantContext.TenantGuid.Value;

            var query = _context.DocumentDownloads
                .Where(d => d.tenantGuid == tg && d.isPublished && d.active && !d.deleted);

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(d => d.categoryName == category);
            }

            var documents = await query
                .OrderBy(d => d.categoryName)
                .ThenBy(d => d.sequence)
                .ThenBy(d => d.title)
                .Select(d => new
                {
                    d.title,
                    d.description,
                    d.fileName,
                    d.filePath,
                    d.mimeType,
                    d.fileSizeBytes,
                    d.categoryName,
                    d.documentDate
                })
                .ToListAsync();

            return Ok(documents);
        }


        // ─────────────────────────────────────────────────────────────
        // Contact
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Submit a contact form.
        /// </summary>
        [HttpPost("Contact")]
        public async Task<IActionResult> SubmitContactForm([FromBody] ContactFormDto dto)
        {
            if (!_tenantContext.IsResolved) return TenantNotResolved();
            var tg = _tenantContext.TenantGuid.Value;

            if (dto == null || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Message))
            {
                return BadRequest(new { error = "Name, email, and message are required." });
            }

            var submission = new ContactSubmission
            {
                tenantGuid = tg,
                name = dto.Name.Trim(),
                email = dto.Email.Trim(),
                subject = dto.Subject?.Trim(),
                message = dto.Message.Trim(),
                submittedDate = DateTime.UtcNow,
                isRead = false,
                isArchived = false,
                objectGuid = Guid.NewGuid(),
                active = true,
                deleted = false
            };

            _context.ContactSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Thank you for your message. We will get back to you soon." });
        }
    }


    /// <summary>
    /// DTO for contact form submissions.
    /// </summary>
    public class ContactFormDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}
