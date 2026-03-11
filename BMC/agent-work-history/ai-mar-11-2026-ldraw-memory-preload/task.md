# LDraw Library — In-Memory Preload & Request Elimination

- [x] Create `LDrawFileService` — preloads all .dat/.ldr files into RAM at startup
- [x] Rewrite `LDrawController` — pure `ControllerBase` with O(1) dictionary lookups, no auth/DB
- [x] Update `ModelExportService` — inject `LDrawFileService`, remove disk I/O bundler
- [x] Register `LDrawFileService` as singleton hosted service in `Program.cs`
- [x] Client: `fetchData` monkey-patch throws immediately for unbundled files (no 12-attempt HTTP flood)
- [x] Client: Simplified `preloadMaterials` — one HTTP call for LDConfig.ldr (served instantly from memory)
- [x] Server build verification — 0 errors
- [ ] User test with 1600-part model
