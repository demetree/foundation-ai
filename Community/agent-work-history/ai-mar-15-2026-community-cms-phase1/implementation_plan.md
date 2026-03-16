# Community CMS — Phase 1 Implementation Plan

Get the Community public site rendering real pages and posts from the database, with a working Angular frontend served by the .NET backend.

## Current State

| Layer | Status |
|-------|--------|
| **CommunityDatabase** | ✅ 18 EF entities + `CommunityContext` scaffolded via EF Core Power Tools |
| **Community.Server** | ✅ Program.cs with Foundation startup, OIDC, schema validation |
| DataControllers (server) | ⚠️ 17 auto-generated controllers **exist in files but are NOT registered** in `Program.cs` |
| PublicContentController | ⚠️ Stub implementations returning placeholder data — needs real DB queries |
| **Community.Client** | ⚠️ Auto-generated data services (18) + data components (17) — **no Angular project bootstrap** (no angular.json, package.json, index.html, etc.) |

## User Review Required

> [!IMPORTANT]
> The Community.Client has no Angular project configuration. We need to bootstrap a new Angular 17 project. Two options:
> 1. **Initialize fresh with `ng new`** inside `Community.Client/` — then move the existing `src/app/community-data-*` folders into the new project
> 2. **Manually create** the project config files (angular.json, package.json, tsconfig, main.ts, index.html, etc.) to wrap around the existing code
>
> Option 2 avoids shuffling files. I recommend Option 2 since we already have auto-generated code in the right place.

> [!IMPORTANT]
> Should we start SSR from day one, or ship a client-only SPA first and add SSR in Phase 2? SSR adds complexity but is critical for SEO. I'd suggest: **skip SSR for Phase 1** (get the site working first), then add SSR in a focused pass once content exists.

---

## Proposed Changes

### 1. Register DataControllers in Program.cs

#### [MODIFY] [Program.cs](file:///g:/source/repos/Scheduler/Community/Community.Server/Program.cs)

Add all 17 DataControllers to the `controllers` list (lines 181-184):

```csharp
controllers.Add(typeof(DataControllers.AnnouncementsController));
controllers.Add(typeof(DataControllers.AnnouncementChangeHistoriesController));
controllers.Add(typeof(DataControllers.ContactSubmissionsController));
// ... all 17
```

---

### 2. Wire PublicContentController to Real DB

#### [MODIFY] [PublicContentController.cs](file:///g:/source/repos/Scheduler/Community/Community.Server/Controllers/PublicContentController.cs)

Replace all stub/placeholder responses with real EF Core queries against `CommunityContext`:
- `GetPageBySlug` → query `Pages` where `IsPublished && Slug == slug`
- `GetPublishedPosts` → query `Posts` with pagination, category filter, includes
- `GetMenuByLocation` → query `Menus`/`MenuItems` with tree structure
- `GetSiteSettings` → query `SiteSettings` key-value pairs
- `GetGalleryAlbums` / `GetGalleryAlbumBySlug` → query albums/images
- `GetDocuments` → query `DocumentDownloads`
- `SubmitContactForm` → save to `ContactSubmissions`

---

### 3. Bootstrap Angular Project

#### [NEW] Community.Client project files

Create the Angular 17 project skeleton around the existing auto-generated code:

| File | Purpose |
|------|---------|
| `angular.json` | Angular CLI workspace config (port 4202, proxy to server) |
| `package.json` | Dependencies (@angular/*, etc.) |
| `tsconfig.json`, `tsconfig.app.json` | TypeScript config |
| `src/main.ts` | Bootstrap entry |
| `src/index.html` | SPA shell with meta tags |
| `src/styles.scss` | Global design system |
| `src/environments/` | API base URLs |
| `proxy.conf.json` | Dev proxy to Community.Server |

---

### 4. Build Public Site Components (Phase 1 Scope)

#### Core Layout
| Component | Route | Description |
|-----------|-------|-------------|
| `AppComponent` | — | Shell with router-outlet |
| `HeaderComponent` | — | Navigation bar, site logo, menu from API |
| `FooterComponent` | — | Copyright, social links, footer menu |

#### Pages
| Component | Route | Data Source |
|-----------|-------|-------------|
| `HomePageComponent` | `/` | Announcements + latest posts + site settings hero |
| `PageViewComponent` | `/:slug` | CMS Page by slug |
| `PostListComponent` | `/news` | Posts with category filter, pagination |
| `PostDetailComponent` | `/news/:slug` | Single post |
| `ContactComponent` | `/contact` | Contact form → API |

#### Services
| Service | Purpose |
|---------|---------|
| `PublicContentService` | HTTP client for all `/api/PublicContent/*` endpoints |
| `SeoService` | Dynamic `<title>` and `<meta>` tag updates |

---

### 5. Design System

Modern, premium public-facing design inspired by the best small-town municipal websites:
- **Google Fonts**: Inter for body, Outfit for headings
- **Color palette**: Deep navy primary, warm accent, neutral grays
- **Hero section**: Full-width gradient with town imagery
- **Cards**: Rounded corners, subtle shadows, hover animations
- **Mobile-first** responsive breakpoints
- **Smooth page transitions** via Angular animations

---

## Verification Plan

### Build Verification
- `Community.Server` builds with all controllers registered
- `Community.Client` runs with `ng serve --proxy-config proxy.conf.json`
- API proxy works (browser at `localhost:4202/api/PublicContent/Settings` returns data)

### Functional Verification
- Home page renders with site settings (name, tagline)
- Navigation menu renders from API data
- Page view displays CMS content by slug
- Post list shows published posts with pagination
- Contact form submits successfully
- Responsive layout works on mobile viewport
