# Full .io Format Import/Export Services

**Date:** 2026-03-10

## Summary

Created export services and API endpoints for `.io`, `.ldr`, and `.mpd` formats. Updated import service to persist all `.io` metadata. Added database schema columns for Studio-specific data.

## Changes Made

- **ModelExportService.cs** [NEW] — Converts Project → `.ldr`/`.mpd`/`.io` with quaternion-to-matrix conversion, round-trip and native export paths
- **MocExportController.cs** [NEW] — API endpoints: `GET /api/moc/export/{id}/ldr|mpd|io`
- **ModelImportService.cs** — Persist `thumbnailData` on Project, `studioVersion`/`instructionSettingsXml`/`errorPartList` on ModelDocument
- **Program.cs** — Registered `ModelExportService` in DI
- **BmcDatabaseGenerator.cs** — Added `thumbnailData` (binary) to Project, `studioVersion`/`instructionSettingsXml`/`errorPartList` to ModelDocument
- **Project.cs** / **ModelDocument.cs** — Entity properties added (rescaffold completed)

## Key Decisions

- **Two export strategies**: Round-trip (uses stored `.io` data from `sourceFileData` for max fidelity) and Native (reconstructs from `PlacedBrick` entities)
- **Quaternion → matrix**: Export service includes utility for converting stored quaternion rotations back to 3x3 matrix format for LDraw Type 1 lines
- **Thumbnail as blob**: Stored directly in `Project.thumbnailData` rather than filesystem path
- **Same security pattern**: Export controller mirrors import controller's security and tenant handling

## Testing / Verification

- Build: BMC.Server 0 errors ✅
- Database rescaffold completed successfully
- Real `.io` file verified end-to-end via scratch test project
