# Full .io Format Compatibility — Import/Export

**Date:** 2026-03-10

## Summary

Implemented complete BrickLink Studio `.io` file format compatibility for both import and export. Tested against a real `.io` file (1,418 parts, Studio 2.1.6) and achieved full round-trip fidelity.

## Changes Made

- **StudioIoParser.cs** — Complete rewrite: multi-password support (`soho0909` + `soN7pnHFRH`), JSON and key-value `.info` parsing, UTF-8 BOM stripping, `CustomParts/` extraction, case-insensitive entry matching
- **StudioIoResult.cs** — Added `CustomParts` dictionary, `UsedPassword` property, updated doc comments
- **StudioIoWriter.cs** — Updated password to `soho0909`, lowercase entry names, JSON `.info` format, `CustomParts/` directory support, default version `2.1.6_4`
- **StudioIoWriteRequest.cs** — Added `CustomParts` dictionary
- **BMC.LDraw.csproj** — Added `SharpZipLib` NuGet dependency

## Key Decisions

- **Multi-password approach**: Studio changed passwords between versions. Parser tries `soho0909` (current) first, falls back to `soN7pnHFRH` (legacy), then tries no password
- **JSON `.info` format**: Real Studio files use JSON (`{"version":"...","total_parts":...}`), not key-value. Writer now outputs JSON to match
- **CustomParts preservation**: `.io` files embed complete LDraw geometry for non-standard parts. These are extracted into a typed dictionary and preserved through round-trip
- **Lightweight JSON parsing**: Used manual string extraction to avoid adding System.Text.Json dependency to BMC.LDraw

## Testing / Verification

- Build: BMC.LDraw 0 errors ✅, BMC.Server 0 errors ✅
- Real file test (`45544+45560.io`): all 53 entries extracted, 47 custom parts parsed, version/thumbnail/part count all correct
- Round-trip test: parse → write → re-parse with identical data (1,424 lines, 47 custom parts, 632 KB thumbnail)
