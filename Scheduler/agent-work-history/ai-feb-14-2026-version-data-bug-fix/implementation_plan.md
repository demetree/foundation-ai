# Fix Audit History Version Data Bug

The change history viewer shows `data: null` for every version because of a bug in the code generator's `GetAllVersionsAsync` template.

## Root Cause

In [EntityExtensionGenerator.cs](file:///d:/source/repos/scheduler/CodeGenerationCore/EntityExtensionGenerator.cs#L658-L661), the generated `GetAllVersionsAsync` method fetches version data with:

```csharp
version.data = await chts.GetVersionAsync(this, 1).ConfigureAwait(false);
//                                              ^ hardcoded to 1!
```

This should be `versionAudit.versionNumber`, not `1`. Every version snapshot incorrectly returns v1's data.

## User Review Required

> [!IMPORTANT]
> This fix changes the **code generator template**, meaning all future regenerations will produce correct code. However, the **already-generated entity extensions** also have this bug baked in. Two options:
> 1. Fix the generator + regenerate all entity extensions (clean but requires running the generator)
> 2. Fix the generator + manually patch the generated output files
>
> Which approach do you prefer? If you can run the code generator, option 1 is cleanest. Otherwise I can find and patch the generated files directly.

## Proposed Changes

### Code Generator

#### [MODIFY] [EntityExtensionGenerator.cs](file:///d:/source/repos/scheduler/CodeGenerationCore/EntityExtensionGenerator.cs)

**Line 660**: Change hardcoded `1` to `versionAudit.versionNumber`:

```diff
-                    version.data = await chts.GetVersionAsync(this, 1).ConfigureAwait(false);
+                    version.data = await chts.GetVersionAsync(this, versionAudit.versionNumber).ConfigureAwait(false);
```

### Generated Entity Extensions

All generated entity extension files that implement `GetAllVersionsAsync` need the same fix in their generated output. This affects every version-controlled entity in the system (~50 entities).

## Verification Plan

### Automated Tests
- `dotnet build` on the server project to verify compilation

### Manual Verification
- Navigate to `/resource/1?tab=history` — history entries should now show diff badges and expand icons
- Click expand icon → modal should show Changes and Snapshot tabs with actual data
