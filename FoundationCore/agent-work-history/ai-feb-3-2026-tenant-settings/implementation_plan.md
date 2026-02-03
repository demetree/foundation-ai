# Tenant Settings Implementation Plan

Add support for tenant-level settings management, following the existing `UserSettings` pattern.

## Analysis Summary

### Review of Existing UserSettings.cs

The existing implementation is **well-structured and comprehensive**. It provides typed getters/setters for:
- String settings (sync + async)
- Int settings (sync only)
- DateTime settings (sync only)
- Bool settings (sync only)
- Object settings (sync + async)
- StringList settings (sync only)

### Minor Style Issues Identified

The following are **minor style inconsistencies** that could be corrected (optional):

| Issue | Location | Description |
|-------|----------|-------------|
| Inconsistent `!` operator | Lines 156, 754, 906, 997, 1073 | Uses `!string.IsNullOrWhiteSpace()` instead of `== false` per codebase style |
| Inconsistent `is not null` | Lines 1018, 1094, 1183, 1289, 1493 | Uses `is not null` instead of `!= null` |
| Nullable annotation | Line 1240 | Uses `object?` nullable annotation while project has nullable disabled |

> [!NOTE]
> These are **cosmetic** and **low priority**. The implementation is functionally correct. I recommend we **skip fixing these** to avoid churn in a working file, unless you prefer strict conformance.

---

## Proposed Changes

### FoundationCore/Security/BusinessLogic

#### [NEW] [TenantSettings.cs](file:///g:/source/repos/Scheduler/FoundationCore/Security/BusinessLogic/TenantSettings.cs)

New business logic class for managing tenant-level settings, following the `UserSettings.cs` pattern. Key differences:

- Works with `SecurityTenant` entity instead of `SecurityUser`
- Retrieves/updates `SecurityTenant.settings` instead of `SecurityUser.settings`
- Accepts `SecurityTenant` object as parameter instead of `SecurityUser`

Will include:
- `GetTenantSettingsAsync` / `GetTenantSettings` - Retrieve raw settings JSON
- `SetTenantSettingsAsync` / `SetTenantSettings` - Persist raw settings JSON
- `GetStringSetting` / `GetStringSettingAsync` / `SetStringSetting` / `SetStringSettingAsync`
- `GetIntSetting` / `SetIntSetting`
- `GetDateTimeSetting` / `SetDateTimeSetting`
- `GetBoolSetting` / `SetBoolSetting`
- `GetObjectSetting` / `GetObjectSettingAsync` / `SetObjectSetting` / `SetObjectSettingAsync`
- `GetStringListSetting` / `SetStringListSetting`

---

### FoundationCore.Web/Controllers/Security

#### [NEW] [TenantSettingsController.cs](file:///g:/source/repos/Scheduler/FoundationCore.Web/Controllers/Security/TenantSettingsController.cs)

New Web API controller for managing tenant settings. Key features:

- Inherits from `SecureWebAPIController`
- Module/Entity: `"Security"`, `"TenantSettings"`
- Routes: `api/TenantSettings` and `api/TenantSettings/{key}`
- Permission: **Admin level required** (unlike UserSettings which allows self-access)
- Endpoints:
  - `GET api/TenantSettings` - Get all tenant settings
  - `GET api/TenantSettings/{key}` - Get specific setting
  - `PUT api/TenantSettings/{key}` - Set specific setting

**Authorization approach**: The controller will require the caller to provide a `tenantId` parameter. Only users with admin privileges can read/write tenant settings. The tenant will be loaded from the database by ID.

---

## Verification Plan

### Build Verification

Run the following command to verify compilation:

```bash
cd g:\source\repos\Scheduler
dotnet build FoundationCore\FoundationCore.csproj
dotnet build FoundationCore.Web\FoundationCore.Web.csproj
```

### Manual Verification

After the build succeeds:

1. **API Endpoint Discovery**: Once deployed, the new endpoints will be available at:
   - `GET /api/TenantSettings?tenantId={id}`
   - `GET /api/TenantSettings/{key}?tenantId={id}`
   - `PUT /api/TenantSettings/{key}?tenantId={id}` with body `{"value": "..."}`

2. **Functional Test** (requires a running Foundation server with an authenticated admin session):
   - Create a test tenant in the database
   - Call `PUT api/TenantSettings/testKey?tenantId={testTenantId}` with `{"value": "testValue"}`
   - Call `GET api/TenantSettings/testKey?tenantId={testTenantId}` and verify response is `{"key": "testKey", "value": "testValue"}`
   - Call `GET api/TenantSettings?tenantId={testTenantId}` and verify `testKey` appears in the returned JSON object

> [!IMPORTANT]
> I did not find existing unit tests for `UserSettings.cs` in the codebase. Would you like me to also create unit tests for `TenantSettings.cs`, or is manual verification via the API sufficient?
