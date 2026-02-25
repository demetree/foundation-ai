# Foundation.IndexedDB

A .NET 10 class library that provides an **IndexedDB-like API** over SQLite using Entity Framework Core.  It lets you define object stores (tables), perform CRUD operations, and query via indexes and key ranges — all with a familiar API for developers accustomed to browser IndexedDB or [Dexie.js](https://dexie.org/).

---

## Architecture Overview

The library is arranged in two abstraction layers, with shared infrastructure underneath.

```
┌──────────────────────────────────────────────────────────────┐
│                       Consumer Code                          │
├──────────────────────────────────────────────────────────────┤
│  Dexter Layer (high-level, Dexie.js-style fluent API)        │
│  ┌──────────────┐ ┌──────────────┐ ┌────────────────────┐   │
│  │DexterDatabase│ │ DexterTable  │ │DexterWhereClause   │   │
│  │              │ │  <T, TKey>   │ │ <T, TKey, TProp>   │   │
│  └──────┬───────┘ └──────┬───────┘ └─────────┬──────────┘   │
│         │                │                   │               │
│  ┌──────┴──────┐  ┌──────┴──────────────────┐│               │
│  │DexterVersion│  │  DexterCollection       ││               │
│  │  Builder    │  │   <T, TKey>             │┘               │
│  └─────────────┘  └─────────────────────────┘               │
├──────────────────────────────────────────────────────────────┤
│  Core IDB* Layer (mirrors browser IndexedDB API)             │
│  ┌──────────┐ ┌──────────────┐ ┌─────────┐ ┌────────────┐  │
│  │IDBFactory│ │IDBObjectStore│ │IDBIndex │ │IDBCursor<T>│  │
│  └────┬─────┘ └──────┬───────┘ └────┬────┘ └────────────┘  │
│       │              │              │                        │
│  ┌────┴─────┐ ┌──────┴──────┐ ┌────┴──────┐                │
│  │IDBDatabase│ │IDBTransaction│ │IDBKeyRange│                │
│  └────┬──────┘ └─────────────┘ └───────────┘                │
├───────┼──────────────────────────────────────────────────────┤
│  Infrastructure                                              │
│  ┌────┴──────┐ ┌───────────┐ ┌──────────────────────────┐   │
│  │ IDBContext │ │ IDBCommon │ │ SqliteWALModeInterceptor │   │
│  │ (EF Core) │ │ (JSON,    │ │ (PRAGMA optimizations)   │   │
│  │           │ │ Exceptions)│ │                          │   │
│  └───────────┘ └───────────┘ └──────────────────────────┘   │
├──────────────────────────────────────────────────────────────┤
│  SQLite (.sqlite file)                                       │
└──────────────────────────────────────────────────────────────┘
```

---

## Data Model

All records are stored as JSON in a single `Data` table. A separate `Metadata` table holds schema configuration and version info.

### Data Table

| Column      | Type     | Purpose                                         |
|-------------|----------|--------------------------------------------------|
| `id`        | `long`   | Auto-increment primary key (internal)            |
| `storeName` | `string` | Logical object store this record belongs to      |
| `keyJson`   | `string` | Serialized primary key (JSON)                    |
| `valueJson` | `string` | Serialized record value (JSON)                   |

A unique composite index on `(storeName, keyJson)` enforces key uniqueness within a store.

### Metadata Table

| Column  | Type     | Purpose                                      |
|---------|----------|----------------------------------------------|
| `Key`   | `string` | Metadata identifier (e.g. `version`, `store_users`) |
| `Value` | `string` | JSON-serialized configuration                |

Secondary indexes are implemented as **SQLite expression indexes** using `json_extract`:

```sql
CREATE INDEX idx_users_email
    ON Data (json_extract(ValueJson, '$.email'))
    WHERE StoreName = 'users'
```

---

## Core Classes

### IDBFactory

Entry point for creating or opening databases.

- `OpenAsync(name, version, upgradeNeededHandler)` — creates the SQLite file, configures EF Core with WAL mode, handles version upgrades
- `DeleteDatabase(name)` — clears the connection pool and deletes the `.sqlite`, `-wal`, and `-shm` files

### IDBDatabase

Represents a single database instance.

- Holds the `IDBContext` (EF Core DbContext) and a `SemaphoreSlim` for thread-safe access
- Manages `ObjectStoreConfig` metadata for each store (KeyPath, AutoIncrement, Indexes)
- Creates/deletes object stores at schema-upgrade time
- Provides `Transaction()` to start read-only or read-write transactions
- `ExecuteWithLockAsync()` — centralised semaphore-protected database access

### IDBObjectStore

Represents an object store (logical table) within the database.

- **CRUD**: `AddAsync`, `PutAsync`, `GetAsync`, `DeleteAsync`, `ClearAsync`
- **Bulk**: `AddListAsync`, `PutListAsync` — uses `EFCore.BulkExtensions` for throughput
- **Cursors**: `OpenCursor<T>()` for streaming iteration
- **Indexes**: `CreateIndexAsync`, `DeleteIndexAsync` — creates SQLite expression indexes
- **Keys**: Supports auto-increment, explicit keys, and keyPath extraction via reflection

### IDBIndex

Represents a secondary index for efficient querying on JSON properties.

- `GetAsync<T>(query)` — retrieves first match
- `GetAllAsync<T>(query, count?)` — retrieves all matches with optional limit
- `OpenCursor<T>()` — streaming iteration over indexed values
- Internally constructs SQL with `json_extract` for filtering

### IDBCursor\<T\>

Iterates over records from an object store or index query.

- `ContinueAsync()` — advances to the next record, deserializes key and value
- Implements both `IDisposable` and `IAsyncDisposable` for proper cleanup

### IDBKeyRange

Defines ranges for queries, used by both `IDBIndex` and `IDBObjectStore`.

- `Only(value)` — exact match
- `LowerBound(value, open?)` — greater than (or equal)
- `UpperBound(value, open?)` — less than (or equal)
- `Bound(lower, upper, lowerOpen?, upperOpen?)` — between

### IDBTransaction

Manages database transactions.

- **Modes**: `ReadOnly` (no EF Core transaction) and `ReadWrite` (wraps `IDbContextTransaction`)
- **Lifecycle**: Requires explicit `Commit()` or `Abort()` in read-write mode; auto-rollbacks on `Dispose()` if not finalized
- **Constraint**: Only one active read-write transaction is allowed at a time

---

## Dexter Layer (Dexie.js-Style API)

Higher-level, strongly-typed wrappers built on the core IDB* classes.

### DexterDatabase

Abstract base class for defining database schemas. Inherit from this to create your typed database:

```csharp
public class MyDatabase : DexterDatabase
{
    public DexterTable<User, int> Users { get; private set; }

    public MyDatabase(IDBDatabase indexedDB) : base(indexedDB)
    {
        Users = Table<User, int>("users");
    }

    public async Task SetupSchema()
    {
        await Version(1).DefineStores(new Dictionary<string, string>
        {
            { "users", "++id, name, &email" }
        });
    }
}
```

### Schema Definition DSL

The `DefineStores` method accepts a schema string per store:

| Prefix | Meaning                        | Example     |
|--------|--------------------------------|-------------|
| `++`   | Auto-increment primary key     | `++id`      |
| `__`   | Non-auto-increment primary key | `__jobId`   |
| `&`    | Unique index                   | `&email`    |
| *(none)* | Regular (non-unique) index   | `name`      |

> **Note:** Unlike Dexie.js, the first field is NOT automatically the primary key.  You must use `++` or `__` to designate one.

### DexterTable\<T, TKey\>

Strongly-typed wrapper around `IDBObjectStore`:

- `AddAsync(entity)` / `PutAsync(entity)` — typed insert/upsert
- `AddListAsync(list)` / `PutListAsync(list)` — bulk operations
- `GetAsync(key)` / `DeleteAsync(key)` / `ClearAsync()`
- `ToListAsync()` — retrieves all records via cursor
- `Where(x => x.Property)` — starts a fluent query chain
- `CountAsync(range?)` — record count

### DexterWhereClause\<T, TKey, TProperty\>

Fluent filtering on indexed properties:

- `.Equals(value)` — exact match
- `.Above(value)` / `.AboveOrEqual(value)` — lower bound queries
- `.Below(value)` / `.BelowOrEqual(value)` — upper bound queries
- `.Between(lower, upper, includeLower?, includeUpper?)` — range queries
- `.StartsWith(prefix)` — string prefix matching via key range

### DexterCollection\<T, TKey\>

Query result set with further operations:

- `.First()` — first matching record
- `.ToArray()` — all matching records as a list
- `.Limit(count)` — cap result count
- `.Count()` — number of matches

---

## Concurrency Model

EF Core's `DbContext` is not thread-safe.  This library protects it with a single `SemaphoreSlim(1, 1)` held by `IDBDatabase`:

```
Thread A ──► semaphore.WaitAsync() ──► DbContext operation ──► semaphore.Release()
Thread B ──► semaphore.WaitAsync() ──► (waits) ──────────────► DbContext operation ──► semaphore.Release()
```

All database-touching methods (`Add`, `Put`, `Get`, `Delete`, cursor iteration, index queries, meta updates) acquire the semaphore before accessing the context.  The centralised `ExecuteWithLockAsync()` helper standardises this pattern.

---

## SQLite Optimization

The `SqliteWALModeInterceptor` applies performance-tuned PRAGMAs on every connection open:

| PRAGMA                        | Value            | Purpose                                   |
|-------------------------------|------------------|-------------------------------------------|
| `journal_mode`                | `WAL`            | Write-ahead logging for concurrent reads  |
| `auto_vacuum`                 | `NONE`           | Skip auto-vacuum (manual only)            |
| `wal_autocheckpoint`          | `2000`           | Passive checkpoint at ~8 MB               |
| `journal_size_limit`          | `104857600`      | Cap WAL at 100 MB                         |
| `synchronous`                 | `NORMAL`         | Balance of safety and performance         |
| `temp_store`                  | `MEMORY`         | Temp tables in memory                     |

---

## Dependencies

| Package                                   | Purpose                           |
|-------------------------------------------|-----------------------------------|
| `EFCore.BulkExtensions`                   | High-throughput bulk insert/update |
| `Microsoft.EntityFrameworkCore.Sqlite`     | SQLite provider for EF Core       |
| `Microsoft.EntityFrameworkCore.Relational` | Relational EF Core abstractions   |
| `Microsoft.EntityFrameworkCore.Proxies`    | Lazy loading proxy support        |
| `Foundation` (project reference)          | Compactica core library           |

---

## File Structure

```
Foundation.IndexedDB/
├── IDBFactory.cs           — Database creation, versioning, upgrade handling
├── IDBDatabase.cs          — Database instance, store management, transactions
├── IDBObjectStore.cs       — Object store CRUD, bulk ops, cursor, indexes
├── IDBIndex.cs             — Secondary index queries via json_extract
├── IDBCursor.cs            — Record iteration with semaphore protection
├── IDBTransaction.cs       — Transaction lifecycle (commit/abort/auto-rollback)
├── IDBKeyRange.cs          — Query range definitions
├── IDBRequest.cs           — Request/response model with events
├── IDBContext.cs           — EF Core DbContext (Data + Metadata tables)
├── IDBCommon.cs            — Shared JSON options, custom exceptions
├── Dexter/
│   ├── DexterDatabase.cs   — Abstract base + DexterVersionBuilder (schema DSL)
│   ├── DexterTable.cs      — Strongly-typed table wrapper
│   ├── DexterWhereClause.cs— Fluent where clause builder
│   └── DexterCollection.cs — Query result operations (First, ToArray, Limit)
├── Services/
│   └── IDBacheService.cs   — Concept: caching service built on IndexedDB
├── Utility/
│   └── SqliteWALInterceptor.cs — WAL mode + PRAGMA configuration
└── README.md
```

---

## Usage Example

```csharp
//
// 1. Create the factory and open a database
//
IDBFactory factory = new IDBFactory("./data");

IDBOpenDBRequest request = await factory.OpenAsync("MyApp", version: 1,
    upgradeNeededHandler: async (db, oldVer, newVer) =>
    {
        IDBObjectStore store = await db.CreateObjectStoreAsync("users",
            new ObjectStoreOptions { KeyPath = "id", AutoIncrement = true });

        await store.CreateIndexAsync("email", "email",
            new IDBObjectStore.IndexOptions { Unique = true });
    });

IDBDatabase db = request.Result;


//
// 2. Add a record
//
IDBObjectStore users = new IDBObjectStore(db, "users");
object key = await users.AddAsync(new { name = "Alice", email = "alice@example.com" });


//
// 3. Query by index
//
IDBIndex emailIndex = users.Index("email");
User alice = await emailIndex.GetAsync<User>("alice@example.com");


//
// 4. Using the Dexter layer
//
MyDatabase myDb = new MyDatabase(db);
await myDb.Users.Where(u => u.Email).Equals("alice@example.com").First();
```

---

## Potential Areas for Improvement

- **Complex Indexing** — compound indexes (`[firstName+lastName]`) and multi-entry indexes (`*tags`) are not yet supported
- **Query Expressiveness** — additional Dexie-like operators (`anyOf`, `notEqual`, `noneOf`, `or`) could be added to `DexterWhereClause`
- **Schema Migration** — the `upgradeNeededHandler` is present, but data migration logic when KeyPath or index definitions change is left to the implementer
- **Error Handling** — custom exceptions exist but not all IndexedDB-specific error codes are mapped