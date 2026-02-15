# BMC Community Platform — Schema Expansion

Expand `BmcDatabaseGenerator.cs` with new tables to support the community platform vision. This adds ~25 new tables across 6 new regions, building on the existing ~30 tables for parts, building, sets, collections, instructions, and rendering.

> [!IMPORTANT]
> This is **schema definition only** — no database scripts, EF models, or UI code will be generated in this session. The user will run SchedulerTools manually to produce output when ready.

## Proposed Changes

### BMC Database Generator

#### [MODIFY] [BmcDatabaseGenerator.cs](file:///d:/source/repos/scheduler/BmcDatabaseGenerator/BmcDatabaseGenerator.cs)

**New Permission Levels & Roles:**
- `BMC_COMMUNITY_WRITER_PERMISSION_LEVEL = 1` — social interactions (follows, likes, comments)
- `BMC_MODERATOR_PERMISSION_LEVEL = 100` — content moderation
- `BMC Community Writer` custom role
- `BMC Moderator` custom role

**Region: User Profiles & Identity** (~4 tables)

| Table | Purpose | Key Fields |
|---|---|---|
| `UserProfile` | Public builder profile, one per tenant | displayName, bio, location, avatarImagePath, profileBannerImagePath, websiteUrl, isPublic |
| `UserProfileLink` | External links (BrickLink store, Flickr, YouTube, etc.) | FK→UserProfile, linkType, url, displayLabel |
| `UserSetOwnership` | Track owned sets with status | FK→LegoSet, status (Owned/Built/Wanted/Display), acquiredDate, personalRating, notes |
| `UserProfileStat` | Cached aggregate stats for fast display | FK→UserProfile, totalPartsOwned, totalSetsOwned, totalMocsPublished, totalFollowers, totalFollowing, lastCalculatedDate |

**Region: Social Graph** (~3 tables)

| Table | Purpose | Key Fields |
|---|---|---|
| `UserFollow` | Follow relationships between users | followerTenantGuid, followedTenantGuid, followedDate |
| `ActivityEventType` | Lookup for activity types | name (PublishedMoc, AddedSet, CompletedChallenge, etc.) |
| `ActivityEvent` | Activity feed events | FK→ActivityEventType, tenantGuid, title, description, relatedEntityType, relatedEntityId, eventDate |

**Region: Content Sharing & Gallery** (~6 tables)

| Table | Purpose | Key Fields |
|---|---|---|
| `PublishedMoc` | A MOC published to the gallery | FK→Project, title, description, thumbnailImagePath, isPublished, publishedDate, viewCount, likeCount, tags |
| `PublishedMocImage` | Gallery images for a published MOC | FK→PublishedMoc, imagePath, caption, sequence |
| `MocLike` | Likes on published MOCs | FK→PublishedMoc, tenantGuid, likedDate |
| `MocComment` | Comments on published MOCs | FK→PublishedMoc, tenantGuid, commentText, postedDate, FK→parent MocComment (threading) |
| `MocFavourite` | User's favourited MOCs (bookmarks) | FK→PublishedMoc, tenantGuid, favouritedDate |
| `SharedInstruction` | Published instruction manuals (BMC format, PDF, or images) | FK→BuildManual (nullable), title, description, formatType, filePath, isPublished, publishedDate, downloadCount |

**Region: Gamification & Achievements** (~4 tables)

| Table | Purpose | Key Fields |
|---|---|---|
| `AchievementCategory` | Groups of achievements (Collection, Building, Social) | name, description, iconPath, sequence |
| `Achievement` | Achievement definitions | FK→AchievementCategory, name, description, iconPath, criteria, pointValue, rarity |
| `UserAchievement` | Achievements earned by users | FK→Achievement, tenantGuid, earnedDate |
| `BuildChallenge` | Community build challenges | title, description, rules, startDate, endDate, thumbnailImagePath, isActive, isFeatured |
| `BuildChallengeEntry` | User entries into a challenge | FK→BuildChallenge, FK→PublishedMoc, tenantGuid, submittedDate |

**Region: Moderation & Admin** (~4 tables)

| Table | Purpose | Key Fields |
|---|---|---|
| `ContentReportReason` | Lookup of report reasons | name (Spam, Inappropriate, Copyright, etc.) |
| `ContentReport` | User-submitted content reports | FK→ContentReportReason, reporterTenantGuid, reportedEntityType, reportedEntityId, description, status (Pending/Reviewed/Dismissed/ActionTaken), reviewedDate |
| `ModerationAction` | Log of moderator actions | moderatorTenantGuid, actionType (Warning/ContentRemoved/UserSuspended/UserBanned), targetTenantGuid, reason, actionDate |
| `PlatformAnnouncement` | Admin announcements shown on landing page | title, body, startDate, endDate, isActive, priority |

**Region: Public API Management** (~2 tables)

| Table | Purpose | Key Fields |
|---|---|---|
| `ApiKey` | API keys for public API consumers | tenantGuid, keyHash, name, description, isActive, createdDate, lastUsedDate, rateLimitPerHour |
| `ApiRequestLog` | Audit log of API requests | FK→ApiKey, endpoint, httpMethod, responseStatus, requestDate, durationMs |

---

## Verification Plan

### Build Verification
```powershell
dotnet build d:\source\repos\scheduler\BmcDatabaseGenerator\BmcDatabaseGenerator.csproj
```

This confirms that:
- All table definitions use valid `DatabaseGenerator` API calls
- All FK references point to tables that exist in the schema
- All seed data dictionaries match table column definitions
- The C# compiles without errors
