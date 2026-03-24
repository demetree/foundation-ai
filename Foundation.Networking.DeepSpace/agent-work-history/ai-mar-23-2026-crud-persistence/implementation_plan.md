# CRUD Persistence for DeepSpace StorageManager

Record storage operation metadata to the DeepSpace SQLite database alongside provider operations, and write metadata sidecar files for disaster recovery.

## Proposed Changes

### Foundation.Networking.DeepSpace

#### [MODIFY] [StorageManager.cs](file:///g:/source/repos/Scheduler/Foundation.Networking.DeepSpace/StorageManager.cs)

**DB persistence after successful provider operations:**

**`PutAsync` / `PutStreamAsync`** — After successful provider put:
1. Find-or-create `StorageProvider` row (lookup by `provider.ProviderName`)
2. Find-or-create `StorageObject` row (lookup by `key` + provider id)
   - New: insert with size, contentType, md5Hash, `objectGuid = Guid.NewGuid()`, `versionNumber = 1`
   - Existing: update size/contentType/hash, increment `versionNumber`
3. Insert `StorageObjectVersion` row (capturing the version snapshot)
4. Write `.deepspace.json` sidecar file via the provider

**`GetAsync` / `GetStreamAsync`** — After successful provider get:
1. Lookup `StorageObject` by key, update `lastAccessedUtc` and `accessCount`

**`DeleteAsync`** — After successful provider delete:
1. Lookup `StorageObject` by key, set `isDeleted = true`, `deletedUtc = DateTime.UtcNow`
2. Delete the `.deepspace.json` sidecar file

**`ListAsync`** — Filter out `*.deepspace.json` files from results.

All DB writes use `_databaseManager.ExecuteWrite()` for proper SQLite write-locking. All persistence is **fire-and-forget** — DB/sidecar failures are logged but don't fail the storage operation.

---

### Metadata Sidecar Files (`.deepspace.json`)

For disaster recovery: if the SQLite database is lost, scan storage providers for sidecar files to rebuild the catalog.

**Convention:** For an object stored at key `documents/report.pdf`, a companion sidecar file is written at `documents/report.pdf.deepspace.json`.

**Sidecar content:**
```json
{
  "key": "documents/report.pdf",
  "contentType": "application/pdf",
  "sizeBytes": 245760,
  "md5Hash": "abc123...",
  "sha256Hash": null,
  "provider": "Local",
  "tier": "Standard",
  "versionNumber": 3,
  "createdUtc": "2026-03-23T22:00:00Z",
  "createdByUserGuid": "a1b2c3...",
  "objectGuid": "d4e5f6..."
}
```

#### [NEW] [StorageObjectSidecar.cs](file:///g:/source/repos/Scheduler/Foundation.Networking.DeepSpace/StorageObjectSidecar.cs)

POCO model matching the sidecar JSON shape, plus:
- `const string SIDECAR_EXTENSION = ".deepspace.json"` 
- `static string GetSidecarKey(string objectKey)` → appends the extension
- `static bool IsSidecarKey(string key)` → checks if a key ends with `.deepspace.json`

---

### Disaster Recovery

#### [NEW] Static method on `StorageManager`: `RebuildFromProvidersAsync`

Scans all registered providers for `*.deepspace.json` files, deserializes each sidecar, and bulk-inserts `StorageObject` + `StorageObjectVersion` rows into a fresh database. This is an admin-only recovery tool, not called during normal operation.

---

## Verification Plan

### Automated Tests
```powershell
dotnet build Foundation.Networking.DeepSpace\Foundation.Networking.DeepSpace.csproj
dotnet build Foundation.Networking.DeepSpace.Host\Foundation.Networking.DeepSpace.Host.csproj
```
