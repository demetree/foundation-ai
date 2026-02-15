# UserProfile CRUD Feature

Build the community profile page and settings, following the `CollectionController` pattern for the server and BMC's premium component conventions for the client.

## Proposed Changes

### Server — ProfileController

#### [NEW] [ProfileController.cs](file:///d:/source/repos/scheduler/BMC/BMC.Server/Controllers/ProfileController.cs)

A custom composite controller (`SecureWebAPIController`) that aggregates across UserProfile, UserProfileLink, UserProfileLinkType, and UserProfileStat.

**Endpoints:**

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/profile/mine` | Get-or-create the current user's profile (display name, bio, avatar, etc.) + links + stats. Auto-creates on first access like `CollectionController.GetMyCollections`. |
| `PUT` | `/api/profile/mine` | Update profile fields (display name, bio, location, website, avatar path, banner path, isPublic). |
| `GET` | `/api/profile/mine/links` | Get the user's profile links with link type info. |
| `PUT` | `/api/profile/mine/links` | Bulk save profile links (delete + recreate pattern for simplicity). |
| `GET` | `/api/profile/link-types` | Get all active link types (for the dropdown/selector). |

**DTOs** (inline, matching `CollectionController` pattern):
- `ProfileDto` — display name, bio, location, avatar, banner, website, isPublic, memberSinceDate, stats, links
- `UpdateProfileRequest` — writable profile fields
- `ProfileLinkDto` — link type id, link type name, icon, url
- `SaveLinksRequest` — array of `{ linkTypeId, url }`

**Security:** Read = `BMC_READER` (level 1). Write = `BMC Community Writer` custom role check.

---

### Client — Profile Page Component

#### [NEW] [profile/](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/profile/)

Premium profile page with a Steam-inspired layout:

- `profile.component.ts/html/scss` — Public profile view
  - Hero banner + avatar overlay
  - Display name, bio, location, member since
  - Social links row (with icons from UserProfileLinkType)
  - Stats cards (MOCs published, followers, parts owned, likes received)
  - Tabbed section for: MOCs | Collection Sets | Achievements (empty shells for now)

#### [NEW] [profile-settings/](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/profile-settings/)

- `profile-settings.component.ts/html/scss` — Edit profile form
  - Reactive form for display name, bio, location, website
  - Avatar/banner image path fields (image upload comes later)
  - Public/private toggle
  - Social links editor: add/remove rows with link type dropdown + URL input
  - Save button → `PUT /api/profile/mine` + `PUT /api/profile/mine/links`

---

### Client — Integration

#### [MODIFY] [app-routing.module.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app-routing.module.ts)

Add routes:
- `{ path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] }`
- `{ path: 'profile/settings', component: ProfileSettingsComponent, canActivate: [AuthGuard] }`

#### [MODIFY] [app.module.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app.module.ts)

Register `ProfileComponent` and `ProfileSettingsComponent` in declarations.

#### [MODIFY] [sidebar.component](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/sidebar/)

Add "My Profile" link to the sidebar navigation.

## Verification Plan

### Automated Tests
- `dotnet build` on `BMC.Server` project to verify controller compiles
- `ng build` on `BMC.Client` to verify Angular compilation

### Manual Verification
- Navigate to `/profile` and confirm auto-creation of profile
- Edit profile fields in `/profile/settings` and verify data persists
- Verify social links CRUD works end-to-end
