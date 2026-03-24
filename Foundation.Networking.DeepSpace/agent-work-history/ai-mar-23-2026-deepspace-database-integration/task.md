# DeepSpace Database Integration

## Integration (Phase 1) ✅
- [x] Add `DatabaseDirectory` to `DeepSpaceConfiguration.cs`
- [x] Add `DeepSpaceDatabase` reference to `Foundation.Networking.DeepSpace.csproj`
- [x] Register `DeepSpaceDatabaseManager` in DI
- [x] Wire `DeepSpaceDatabaseManager` into `StorageManager`
- [x] Resolve `StorageObject` naming ambiguity via using alias

## Host Build Fix ✅
- [x] Add `Foundation.Web`, `DeepSpaceDatabase`, `Hangfire.Core` references to Host .csproj
- [x] Delete duplicate `MigrationJobStatusesController.cs` (code-gen artifact)
- [x] Remove `StorageObjectVersionChangeHistory` refs from `StorageObjectVersionsController.cs`
  - [x] Put method: removed change history transaction block
  - [x] Post method: removed change history transaction block
  - [x] Delete method: removed change history block
  - [x] Removed 5 endpoint methods: Rollback, ChangeMetadata, AuditHistory, GetVersion, GetStateAtTime
- [x] Bump all Microsoft.Extensions.* packages to 10.0.5 (DeepSpace + DeepSpaceDatabase)

## Remaining Work
- [ ] Implement CRUD persistence in `StorageManager` (PutAsync/DeleteAsync → DB)
- [ ] Fix code generator to avoid producing `StorageObjectVersionChangeHistory` code
- [ ] Connect `FileManagerController` to updated `StorageManager`
