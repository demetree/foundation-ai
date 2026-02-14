# Fix: BMC.Client Listing Components Showing Headers But No Table Content

## Root Cause

The auto-generated table components use several shared utilities that existed in Scheduler.Client but were never brought over to BMC.Client:

| Missing Piece | What It Does | Impact |
|---|---|---|
| `*showSpinner` directive | Structural directive wrapping the table body | **Table body not rendered at all** — this was the primary culprit |
| `<boolean-icon>` component | Renders ✓/✗ icons for booleans | Boolean columns would throw errors |
| `bigNumberFormat` pipe | Formats large numbers | Numeric columns in commented templates |
| `ScrollingModule` | CDK virtual scrolling | `cdk-virtual-scroll-viewport` wouldn't work |

The `*showSpinner` directive is the key: it wraps the entire `<div class="col">` containing the table. Without it registered, Angular silently skips the element — headers render (they're outside the directive) but the table body doesn't.

## Changes Made

### New Files (copied from Scheduler.Client)

- [spinner.directive.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/directives/spinner.directive.ts)
- [spinner.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/directives/spinner.component.ts)
- [boolean-icon.component.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/components/controls/boolean-icon.component.ts)
- [big-number-format.pipe.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/pipes/big-number-format.pipe.ts)

### Modified Files

- [app.module.ts](file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app.module.ts) — added imports, declarations, `ScrollingModule`, and `exports: [SpinnerDirective]`

render_diffs(file:///d:/source/repos/scheduler/BMC/BMC.Client/src/app/app.module.ts)

## Verification

Build completed successfully:
```
Application bundle generation complete. [10.063 seconds]
Exit code: 0
```

> [!NOTE]
> Manual verification needed: navigate to a listing route (e.g., Brick Colours) in the browser to confirm table rows now appear below the headers.
