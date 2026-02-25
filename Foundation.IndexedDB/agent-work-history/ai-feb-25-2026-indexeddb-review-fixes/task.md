# Foundation.IndexedDB — Review Fixes

## Bug Fixes
- [x] Fix `AddListAsync` serializing entire list instead of individual value (`IDBObjectStore.cs:229`)
- [x] Fix `GetAllAsync` passing `count.HasValue` (bool) instead of `count.Value` (int) (`IDBIndex.cs:207`)
- [x] Fix `DeleteIndexAsync` — make truly async or remove `Async` suffix (`IDBObjectStore.cs:482`)
- [x] Implement `IAsyncDisposable` on `IDBCursor` (`IDBCursor.cs`)

## Cleanup
- [x] Remove unused `_storesToCreate` tuple field (`DexterDatabase.cs:105`)
- [x] Remove double semicolons (`IDBContext.cs:25,36`)

## Style Fixes
- [x] Fix explicit boolean checks (`IDBTransaction.cs`, `IDBObjectStore.cs`)
- [x] Add braces to single-line conditionals (`IDBObjectStore.cs:635`)
- [x] Replace `var` with explicit types where appropriate
- [x] Fix function spacing (two blank lines between functions)
- [x] Fix C# brace style issues (`IDBContext.cs`)

## Verification
- [x] Build the project and confirm 0 errors
