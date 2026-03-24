# Messaging UI Fixes, Theme Audit & Feature Toggle

**Date:** 2026-03-24

## Summary

Fixed two messaging system issues after porting from Catalyst, audited and migrated all messaging SCSS from hardcoded Catalyst colors to Scheduler's `--sch-*` theme system, and implemented a feature toggle for the entire messaging/notification subsystem driven by `appsettings.json` with per-tenant override support.

## Changes Made

### Messaging Display Mode & SignalR Fix
- **sidebar.component.html** — Added `<app-messaging>` off-canvas panel (matching Catalyst's sidebar pattern)
- **sidebar.component.ts** — Added `toggleMessaging()` method and `unreadMessageCount` getter
- **header.component.ts** — Reverted messages button to emit `toggleMessaging` event instead of navigating to `/messaging` route
- **app.component.html** — Wired header `toggleMessaging` event to sidebar's `toggleMessaging()` via ViewChild `#sidebarRef`
- **app.component.ts** — Added `MessagingSignalRService.connect()` on login and `disconnect()` on logout

### Theme Audit (SCSS Migration)
- **_messaging-common.scss** — Migrated 8 design tokens from hardcoded hex to `--sch-*` CSS custom properties:
  - `$bg-dark` → `var(--sch-bg-body)`, `$bg-darker` → `var(--sch-sidebar-bg)`, `$bg-panel` → `var(--sch-bg-card)`
  - `$bg-hover` → `var(--sch-surface-hover)`, `$border-color` → `var(--sch-border)`
  - `$text-primary/secondary/muted` → `var(--sch-text-primary/secondary/muted)`
  - Kept `$accent`, `$danger`, `$online`, `$dnd` as SCSS hex for `rgba()` function compatibility
- **messaging.component.scss** — Fixed 2 `lighten(msg.$bg-panel, 4%)` calls → `var(--sch-surface-subtle)`

### Messaging Feature Toggle
- **appsettings.json** — Added `"MessagingEnabled": true` under `Settings`
- **FeatureConfigController.cs** — Rewrote with `ResolveToggle()` helper supporting tenant override (tenant JSON settings → appsettings fallback); all 5 toggles now support tenant override
- **feature-config.service.ts** — Added `messagingEnabled` to interface + `isMessagingEnabled$` getter
- **header.component.ts/html** — Gated messages icon + notification bell behind `isMessagingEnabled$`
- **sidebar.component.ts/html** — Gated `<app-messaging>` panel behind `isMessagingEnabled$`
- **app.component.ts** — Gated `messagingSignalR.connect()` behind `isMessagingEnabled$.pipe(take(1))`

## Key Decisions

- **Panel vs Route:** Messaging opens as an off-canvas sidebar panel (matching Catalyst UX) rather than a full-page `/messaging` route
- **SignalR Lifecycle:** Centralized in `AppComponent` — `connect()` on login, `disconnect()` on logout
- **Theme Token Strategy:** SCSS `$variables` holding `var()` CSS expressions work for simple CSS properties, but SCSS color functions (`rgba()`, `lighten()`) require compile-time hex values — kept `$accent`/`$danger` as hex for this reason
- **Feature Toggle Resolution:** Tenant settings JSON override → appsettings.json fallback. Bonus: all 5 existing feature toggles now also support tenant override
- **Unauthenticated Access:** `FeatureConfigController` remains a public endpoint; tenant override only applies when the user is authenticated

## Testing / Verification

- **Client build:** `ng build` — zero TypeScript/SCSS errors (exit code 1 from pre-existing budget warnings only)
- **Server build:** `dotnet build` — zero CS compilation errors (exit code 1 from `MSB3021` file-lock only, dev server holding DLLs)
