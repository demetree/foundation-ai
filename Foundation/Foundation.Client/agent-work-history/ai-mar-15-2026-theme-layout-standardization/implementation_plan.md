# Foundation.Client Theme Migration

Apply the same theming system from Scheduler to Foundation.Client, using `--fnd-*` CSS custom property prefix.

> [!NOTE]
> The code generator already passes `applicationThemePrefix: "fnd"` for Foundation's data components (Auditor, Security, Telemetry). Those just need regeneration — no code change needed.

## Proposed Changes

### Phase 1 — Theme Foundation

#### [NEW] [_themes.scss](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/assets/styles/_themes.scss)
- CSS custom property definitions for `--fnd-*` tokens
- Same themes as Scheduler: `default`, `warm`, `midnight`, `slate`, `ocean`
- Tokens: `--fnd-bg`, `--fnd-panel-bg`, `--fnd-text-primary`, `--fnd-border`, `--fnd-primary`, `--fnd-header-bg`, `--fnd-sidebar-bg`, etc.

#### [NEW] [theme.service.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/services/theme.service.ts)
- Copy Scheduler pattern: `data-theme` attribute, localStorage, user/tenant cascade
- Use existing `UserSettingsService` and `TenantSettingsService`
- Storage key: `foundation-theme`

#### [MODIFY] [styles.scss](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/styles.scss)
- Import `_themes.scss`
- Replace hardcoded `#f9f9f9` post-login bg → `var(--fnd-bg)`
- Replace structural colors in `.health-card`, `.metric-row`, `.drive-card`

---

### Phase 2 — Core Components

#### [MODIFY] [header.component.scss](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/header/header.component.scss)
- Replace `$header-gradient-start`/`$header-gradient-end` → `var(--fnd-header-bg)` and `var(--fnd-header-gradient-end)`
- Theme-aware dropdown, profile button, mobile menu

#### [MODIFY] [sidebar.component.scss](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/sidebar/sidebar.component.scss)
- Replace `$sidebar-gradient-start`/`$sidebar-gradient-end` → `var(--fnd-sidebar-bg)` etc.
- Theme-aware accent colors, hover states, admin submenu

#### [MODIFY] [app.component.scss](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/app.component.scss)
- Replace hardcoded black navbar, dark footer, `#343a40` modal header → tokens

#### [MODIFY] [overview.component.scss](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/overview/overview.component.scss)
- Replace `#f8fafc`, `white`, `#e5e7eb`, `#6b7280`, `#374151` → tokens

#### [MODIFY] [app.component.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/app.component.ts)
- Inject ThemeService, call `initializeAfterLogin()` after auth

#### [MODIFY] [header.component.ts](file:///g:/source/repos/Scheduler/Foundation/Foundation.Client/src/app/components/header/header.component.ts)
- Add theme picker dropdown (same pattern as Scheduler)

---

### Phase 3 — Custom Components (batch)
- Batch-replace `#f4f6f9`, `#f8fafc`, `white`, `#f9f9f9` page backgrounds → `var(--fnd-bg)`
- ~58 SCSS files in `components/`

### Phase 4 — Code-Generated Data Components
- Regenerate Auditor, Security, Telemetry data components (already has `fnd` prefix)
- Or batch-replace existing files (same approach as Scheduler)

## Verification Plan

### Automated Tests
- `ng build --configuration=development` — zero errors after each phase
