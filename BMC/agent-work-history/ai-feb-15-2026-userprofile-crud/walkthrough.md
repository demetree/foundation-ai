# UserProfile CRUD — Walkthrough

## What Was Built

### Server — [ProfileController.cs](file:///d:/source/repos/scheduler/BMC/BMC.Server/Controllers/ProfileController.cs)

Custom composite controller (5 endpoints) following `CollectionController` patterns:

| Endpoint | Purpose |
|---|---|
| `GET /api/profile/mine` | Get-or-create profile with stats + links |
| `PUT /api/profile/mine` | Update profile fields |
| `GET /api/profile/mine/links` | Get profile links with type info |
| `PUT /api/profile/mine/links` | Bulk save links (soft-delete + recreate) |
| `GET /api/profile/link-types` | Get available link types for dropdown |

Auto-creates profile + stats row on first access. Security: read = BMC Reader, write = BMC Community Writer.

---

### Client — Profile Page

| File | Purpose |
|---|---|
| [profile.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/profile/profile.component.ts) | Data loading, stats builder |
| [profile.component.html](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/profile/profile.component.html) | Hero banner, avatar, bio, links, stats, tabs |
| [profile.component.scss](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/profile/profile.component.scss) | Premium styling |

### Client — Profile Settings

| File | Purpose |
|---|---|
| [profile-settings.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/profile-settings/profile-settings.component.ts) | Form logic, save/cancel |
| [profile-settings.component.html](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/profile-settings/profile-settings.component.html) | Form cards + social links editor |
| [profile-settings.component.scss](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/profile-settings/profile-settings.component.scss) | Premium form styling |

### Integration

- [sidebar.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/sidebar/sidebar.component.ts) — Added "My Profile" nav item
- [app-routing.module.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app-routing.module.ts) — Added `/profile` and `/profile/settings` routes
- [app.module.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app.module.ts) — Registered both components

## Verification

- **Server build**: `dotnet build` — ✅ 0 errors, 0 warnings
- **Client build**: `ng build` — ✅ 0 errors, clean bundle generation (33s)
