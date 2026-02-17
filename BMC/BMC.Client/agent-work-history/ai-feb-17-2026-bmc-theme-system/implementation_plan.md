# BMC.Client Theme System — Port from BCDC

Port the multi-theme system (Default / Miami Vice / Matrix) from `BasecampDataCollector.client` into `BMC.Client`, adapted at every level for BMC's warm amber / brick-builder identity.

## User Review Required

> [!IMPORTANT]
> The existing `--bmc-*` CSS custom properties in `:root` will be moved into a `body, body[data-theme="default"]` block. This means any component currently using `var(--bmc-primary)` etc. will continue to work **unchanged** — but the variable *values* will switch when the user picks a different theme. This is entirely cosmetic; no functional behaviour changes.

> [!NOTE]
> The BCDC Miami Vice and Matrix themes reference background images (`miami-skyline.jpg`, `matrix-rain.jpg`). For BMC these will be omitted in the initial implementation — the `--theme-skyline` properties will be set to `none`. Background images can be added later if desired.

---

## Proposed Changes

### Theme Styles

#### [NEW] [_themes.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/assets/styles/_themes.scss)

Create a new SCSS partial defining three `[data-theme]` blocks that override all `--bmc-*` CSS custom properties.

- **Default theme** — the current warm amber/orange palette (values lifted from `:root` in `styles.scss`)
- **Miami Vice theme** — hot pink / orange / cyan palette adapted for BMC's dark surfaces
- **Matrix theme** — phosphor green on black, matching the BCDC version

Each block will define the full set of `--bmc-*` variables: brand, surface, text, border, shadow, and transition tokens.

---

#### [MODIFY] [styles.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/styles.scss)

- Import the new `_themes.scss` partial (before the existing Inter font import)
- Remove the `:root { ... }` block (lines 11–55) — those variables now live in `_themes.scss` Default block
- Keep everything else (base resets, cards, buttons, badges, forms, tables, modals, animations, utilities, toastr overrides)

---

### Theme Service

#### [NEW] [theme.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/theme.service.ts)

Adapted from BCDC's `theme.service.ts`:

- `providedIn: 'root'` — no manual module registration needed
- Storage key: `bmc-theme`
- `BehaviorSubject<string>` for reactive theme tracking
- `availableThemes` array with swatch colours appropriate for BMC's palettes
- `setTheme()` applies `[data-theme]` on `<body>`, persists to `localStorage`

---

### Header Integration

#### [MODIFY] [header.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/header/header.component.ts)

- Import and inject `ThemeService`
- Add `setTheme(themeId: string)` method

#### [MODIFY] [header.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/header/header.component.html)

- Add a theme picker section (palette icon + 3 swatch buttons) between the brand and the right-side actions

#### [MODIFY] [header.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/header/header.component.scss)

- Convert the hardcoded amber `background` on `.bmc-navbar` to use `var(--bmc-bg-dark)` / gradient
- Convert hardcoded `border-bottom` and `box-shadow` colours to use `--bmc-border`
- Add `.theme-picker`, `.theme-swatch`, and swatch variant styles (matching BCDC's pattern)
- Convert `.brand-icon` gradient and `.brand-text` gradient to use `--bmc-*` variables

---

## Verification Plan

### Automated Tests

No existing unit tests were found in the BMC.Client project. Verification will rely on the Angular production build:

```bash
cd g:\source\repos\Scheduler\BMC\BMC.Client
npx ng build --configuration production
```

A successful build confirms:
- All imports resolve correctly
- `_themes.scss` is valid SCSS and the import chain works
- `ThemeService` compiles and is injectable
- Header component template and TypeScript compile without errors

### Manual Verification

After running `ng serve`, the user can verify in the browser:
1. **Default theme** loads with the current warm amber appearance (no visual regression)
2. **Theme picker** visible in the header — three coloured swatches
3. **Clicking Miami Vice** switches the entire UI to pink/orange/cyan palette
4. **Clicking Matrix** switches to green-on-black
5. **Refreshing the page** remembers the last chosen theme (localStorage persistence)
6. **Clicking Default** restores the original amber palette
