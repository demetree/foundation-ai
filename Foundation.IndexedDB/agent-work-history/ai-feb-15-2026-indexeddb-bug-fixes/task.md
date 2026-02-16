# Foundation.IndexedDB Bug Fixes

## Bugs
- [x] Fix `IDBFactory` constructor ignoring `basePath` parameter
- [x] Fix `DeleteDatabase` not cleaning up SQLite WAL/SHM sidecar files
- [x] Remove pointless empty catch/rethrow in `DexterCollection.First()`
- [x] Fix typo in `SqliteWALInterceptor` comment ("diagnsostics")
- [x] Fix `DexterDatabase.DefineStores` double-stripping `&` prefix on unique indexes

## Verification
- [ ] Build the solution to confirm no regressions
