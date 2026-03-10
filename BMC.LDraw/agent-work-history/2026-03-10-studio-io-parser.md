# StudioIoParser and StudioIoResult — .io File Support

**Date:** 2026-03-10

## Summary

Added BrickLink Studio `.io` file format support to BMC.LDraw by creating a parser that extracts LDraw content from `.io` ZIP archives.

## Changes Made

- **Created** `Models/StudioIoResult.cs` — Data model holding extracted LDraw lines, thumbnail bytes, Studio version, reported part count, and instruction XML
- **Created** `Parsers/StudioIoParser.cs` — Static parser class that handles ZIP extraction with multiple entry name variants, thumbnail extraction, `.INFO` metadata parsing, and instruction XML extraction

## Key Decisions

- `.io` files are ZIP archives containing `model.ldr` (standard LDraw). The parser extracts this and delegates to the existing `ModelParser` for LDraw parsing.
- Multiple entry name variants supported (`model.ldr`, `model2.ldr`, case-insensitive `.ldr` fallback) for robustness across Studio versions.
- Password-protected `.io` files are not supported natively by `System.IO.Compression.ZipArchive` — would require a NuGet package like `SharpCompress` if needed.

## Testing / Verification

- `dotnet build BMC.LDraw/BMC.LDraw.csproj` — ✅ 0 errors
