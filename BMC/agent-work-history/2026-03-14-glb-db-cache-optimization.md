# GLB Database Persistence Performance Optimization

**Date:** 2026-03-14

## Summary

Dramatically improved the server-side loading time of the `moc-viewer` when retrieving precompiled GLB 3D models from the Tier 2 Database Cache (which occurs on the first load after an application pool/server restart). Resolved an 11-second delay for massive models (like the 244MB Titanic GLB) that made the cache hit feel like a cache miss.

## Changes Made

- **`BMC.Server/Services/GlbCacheService.cs`**
  - Replaced the Entity Framework Core `FirstOrDefaultAsync` lookup inside `GetOrBuildGlbAsync`'s db-tier cache retrieval logic.
  - Rewrote the DB hit logic using raw ADO.NET and a `SqlCommand` utilizing `ExecuteReaderAsync` with `CommandBehavior.SequentialAccess`.
  - Configured a `MemoryStream` to directly stream the `glbData` blob from the DB via `reader.GetStream(0)`, pushing bytes without buffering the entire column.

## Key Decisions

- **Bypassing EF Core for Large Object (LOB) columns:** Entity Framework Core performs poorly when mapping massive `varbinary(max)` columns to C# byte-arrays on model deserialization due to intermediate buffering and extreme allocations on the Large Object Heap (LOH). 
- **SequentialAccess:** Utilizing `CommandBehavior.SequentialAccess` is imperative for streaming large blobs over local SQL connections, as it streams data directly into the memory stream efficiently without waiting for the entire row to load into the driver's memory.

## Testing / Verification

- Measured SQL Server execution speed locally via `Command-Measure` with `sqlcmd`, confirming the DB query itself took <50ms, isolating the issue to the application layer.
- Server tested via Visual Studio compile and debug cycle. The 11-second delay experienced by the WebGL viewer on initial browser reload is eliminated, acting nearly identically to a Tier 1 RAM hit.
