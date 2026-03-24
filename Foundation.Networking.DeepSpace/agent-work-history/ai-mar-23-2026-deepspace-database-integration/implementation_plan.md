# DeepSpace Database Integration — Implementation Plan

Wire the `DeepSpaceDatabaseManager` into `StorageManager` so every PutAsync/DeleteAsync records metadata in the SQLite database, and the system tracks storage providers, objects, and access logs persistently.

## User Review Required

> [!IMPORTANT]
> **Naming conflict:** The existing `Providers/IStorageProvider.cs` defines a `StorageObject` class (an in-flight DTO), and the database layer has a `Database.StorageObject` entity. These are in different namespaces (`Foundation.Networking.DeepSpace.Providers` vs `Foundation.DeepSpace.Database`) so they won't collide at compile time, but we should be aware of the semantic overlap. No rename needed — just alias where both are used.

> [!IMPORTANT]
> **Scope of this change:** This wires up the database lifecycle and makes `StorageManager` *aware* of the database. It does **not** implement full CRUD persistence for every operation yet — that's a follow-up step once we validate the plumbing compiles and the database initializes correctly at startup.

## Proposed Changes

### Configuration

---

#### [MODIFY] [DeepSpaceConfiguration.cs](file:///g:/source/repos/Scheduler/Foundation.Networking.DeepSpace/Configuration/DeepSpaceConfiguration.cs)

Add a `DatabaseDirectory` property so the database path is configurable from `appsettings.json`:

```csharp
/// <summary>
/// Directory for the DeepSpace SQLite metadata database.
/// Defaults to "./deepspace-data" relative to the application base.
/// </summary>
public string DatabaseDirectory { get; set; } = "";
```

---

### Project Reference

#### [MODIFY] [Foundation.Networking.DeepSpace.csproj](file:///g:/source/repos/Scheduler/Foundation.Networking.DeepSpace/Foundation.Networking.DeepSpace.csproj)

Add project reference:
```xml
<ProjectReference Include="..\DeepSpaceDatabase\DeepSpaceDatabase.csproj" />
```

---

### StorageManager — Database Awareness

#### [MODIFY] [StorageManager.cs](file:///g:/source/repos/Scheduler/Foundation.Networking.DeepSpace/StorageManager.cs)

- Accept `DeepSpaceDatabaseManager` as an optional constructor parameter
- Expose `DatabaseManager` property for external access
- On `PutAsync` success → record/update a `Database.StorageObject` row (future step)
- On `DeleteAsync` success → soft-delete the `Database.StorageObject` row (future step)

For now, the constructor stores the reference and logs its availability. The actual DB writes will come in a subsequent pass once we validate the plumbing.

---

### DI Registration

#### [MODIFY] [DeepSpaceServiceExtensions.cs](file:///g:/source/repos/Scheduler/Foundation.Networking.DeepSpace/DeepSpaceServiceExtensions.cs)

- Register `DeepSpaceDatabaseManager` as a singleton (with the configured database directory and logger)
- Pass it into `StorageManager`'s constructor

---

## Verification Plan

### Automated Tests
- `dotnet build DeepSpaceDatabase\DeepSpaceDatabase.csproj` — still green
- `dotnet build Foundation.Networking.DeepSpace\Foundation.Networking.DeepSpace.csproj` — compiles with new reference
- `dotnet build Foundation.Networking.slnx` — full solution builds
