# DeepSpace CRUD Persistence

## Planning
- [x] Review StorageManager, DeepSpaceDatabaseManager, DB schema
- [x] Write implementation plan
- [x] Add metadata sidecar file design

## Implementation
- [x] Create `StorageObjectSidecar.cs` (POCO + sidecar key helpers)
- [x] Add private helpers to StorageManager:
  - [x] `EnsureStorageProvider` (find-or-create provider row)
  - [x] `EnsureDefaultStorageTier` (find-or-create tier row)
  - [x] `RecordPutMetadata` (StorageObject upsert + StorageObjectVersion insert)
  - [x] `WriteSidecarAsync` (serialize + put via provider)
- [x] Add DB persistence to `PutAsync` and `PutStreamAsync`
- [x] Add DB persistence to `GetAsync` and `GetStreamAsync` (access tracking)
- [x] Add DB persistence to `DeleteAsync` (soft-delete + remove sidecar)
- [x] Filter `.deepspace.json` from `ListAsync` results
- [x] Add `RebuildFromProvidersAsync` recovery method

## Verification
- [x] Build Foundation.Networking.DeepSpace — ✅ 0 errors
- [x] Build Foundation.Networking.DeepSpace.Host — ✅ 0 errors
