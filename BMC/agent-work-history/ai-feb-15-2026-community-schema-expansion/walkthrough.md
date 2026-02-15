# BMC Community Platform Schema — Walkthrough

## What Was Done

Expanded [BmcDatabaseGenerator.cs](file:///d:/source/repos/scheduler/BmcDatabaseGenerator/BmcDatabaseGenerator.cs) with **27 new tables** across 6 regions, plus new permission levels and custom roles, to support the full community platform vision.

render_diffs(file:///d:/source/repos/scheduler/BmcDatabaseGenerator/BmcDatabaseGenerator.cs)

## Schema Regions

| Region | Tables | Purpose |
|--------|--------|---------|
| **User Profiles & Identity** | UserProfile, UserProfileLinkType, UserProfileLink, UserSetOwnership, UserProfileStat | Builder profiles, external links, set ownership tracking, cached stats |
| **Social Graph** | UserFollow, ActivityEventType, ActivityEvent | Follow relationships and activity feeds |
| **Content Sharing & Gallery** | PublishedMoc, PublishedMocImage, MocLike, MocComment, MocFavourite, SharedInstruction | MOC gallery with full social interactions + instruction sharing |
| **Gamification** | AchievementCategory, Achievement, UserAchievement, UserBadge, UserBadgeAssignment, BuildChallenge, BuildChallengeEntry | Achievements, badges, and community build challenges |
| **Moderation & Admin** | ContentReportReason, ContentReport, ModerationAction, PlatformAnnouncement | Content reporting, mod audit log, platform announcements |
| **Public API** | ApiKey, ApiRequestLog | API key management and request auditing |

## Permission Model

| Level | Value | Purpose |
|-------|-------|---------|
| `BMC_COMMUNITY_WRITER` | 20 | Standard community actions (profile, posts, follows) |
| `BMC_COLLECTION_WRITER` | 25 | Collection management (set ownership, parts) |
| `BMC_MODERATOR` | 60 | Challenge creation, content moderation, announcements |

## Design Decisions

- **Tenant-per-user model** — Aligns with Scheduler's `TenantProfile` pattern; each BMC user is a tenant with `AddMultiTenantSupport()` on all user-scoped tables
- **Cross-tenant references via GUID fields** — Social graph (follows, likes, comments) uses `GuidField` for referencing other tenants instead of FK so the data lives in the actor's tenant
- **Image paths over binary data** — Matches BMC's existing convention (LDraw file paths vs embedded blobs), better for a content-heavy platform with CDN potential
- **Seed data** — All lookup tables seeded with real data (7 link types, 7 activity types, 5 achievement categories, 5 starter achievements, 5 badges, 6 report reasons)

## Verification

- ✅ `dotnet build BmcDatabaseGenerator.csproj` — succeeded in 6.3s with no errors
- ✅ TenantProfile pattern alignment reviewed — BMC UserProfile follows the same multi-tenant conventions
- ✅ All FK references resolve to existing tables (legoSetTable, projectTable, buildManualTable, etc.)
