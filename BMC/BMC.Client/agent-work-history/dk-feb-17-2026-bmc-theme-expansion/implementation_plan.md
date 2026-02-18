# Add 5 New BMC Themes

Add Cyberpunk, Retro Arcade, Brick Builder (light), Blueprint, and Pastel Dreams themes to the existing theme system, bringing the total from 3 to 8. The existing architecture (CSS custom properties on `body[data-theme]`, `ThemeService`, header swatch picker) requires no structural changes — just additive CSS blocks and service registration.

## User Review Required

> [!IMPORTANT]
> **Brick Builder is a light theme** — the first one in the app. A few places in [styles.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/styles.scss) have hardcoded dark-theme assumptions (amber rgba values in badge, outline button, form focus glow, and a `.btn-close { filter: invert(1) }` that would double-invert on light backgrounds). The plan includes fixes to make these theme-aware.

> [!NOTE]
> **No background images for new themes.** Only Miami Vice uses a background image. New themes will use CSS-only overlays/patterns (grid for Blueprint, pixel-grid for Retro Arcade, scanlines for Cyberpunk, soft gradient for Pastel Dreams). Brick Builder has no overlay.

---

## Proposed Changes

### Themes CSS

#### [MODIFY] [_themes.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/assets/styles/_themes.scss)

Append 5 new `body[data-theme="..."]` blocks (each ~100 lines), following the exact same variable structure as existing themes. Color palettes:

| Theme | Primary | Accent | Base BG | Overlay |
|-------|---------|--------|---------|---------|
| `cyberpunk` | `#fcee09` electric yellow | `#ff003c` neon red | `#121212` charcoal | Horizontal noise lines |
| `retro-arcade` | `#ff00ff` magenta | `#00e5ff` cyan | `#050505` pure black | Pixel grid pattern |
| `brick-builder` | `#E3000B` LEGO red | `#006CB7` LEGO blue | `#f5f5f0` cream white | None |
| `blueprint` | `#4fc3f7` tech blue | `#b0bec5` silver | `#0a1628` dark navy | Blueprint grid lines |
| `pastel-dreams` | `#b388ff` lavender | `#f48fb1` pink | `#1a1025` deep plum | Soft pastel gradient wash |

Each block defines all ~40 CSS custom properties: brand colors, brick fills, surfaces, glass, text, borders, shadows, gradients, header/sidebar, scrollbar, table, buttons, radial accents, overlay, skyline, and panel tokens.

---

### Theme Service

#### [MODIFY] [theme.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/theme.service.ts)

Add 5 entries to `availableThemes` array with appropriate `id`, `label`, `icon`, and `swatchColors`:

| id | label | icon | Swatch colors |
|----|-------|------|--------------|
| `cyberpunk` | Cyberpunk | `fas fa-bolt` | `#fcee09, #ff003c, #121212` |
| `retro-arcade` | Retro Arcade | `fas fa-gamepad` | `#ff00ff, #00e5ff, #050505` |
| `brick-builder` | Brick Builder | `fas fa-shapes` | `#E3000B, #006CB7, #FFCD03` |
| `blueprint` | Blueprint | `fas fa-drafting-compass` | `#4fc3f7, #b0bec5, #0a1628` |
| `pastel-dreams` | Pastel Dreams | `fas fa-moon` | `#b388ff, #f48fb1, #1a1025` |

---

### Header Swatch Styles

#### [MODIFY] [header.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/header/header.component.scss)

Add 5 new `.swatch-*` gradient classes inside `.theme-swatch`:

- `.swatch-cyberpunk` — yellow → red gradient
- `.swatch-retro-arcade` — magenta → cyan gradient
- `.swatch-brick-builder` — LEGO red → blue → yellow (3-stop)
- `.swatch-blueprint` — tech blue → dark navy gradient
- `.swatch-pastel-dreams` — lavender → pink gradient

---

### Light Theme Compatibility

#### [MODIFY] [styles.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/styles.scss)

Replace hardcoded amber rgba values with theme-aware CSS variables:

1. `.badge-bmc` — use `var(--bmc-accent-glow-soft)` and `var(--bmc-border-strong)` instead of hardcoded `rgba(255, 167, 38, ...)`
2. `.btn-bmc-outline:hover` — use `var(--bmc-surface-hover)` instead of `rgba(255, 167, 38, 0.08)`
3. `.form-control:focus` / `.form-select:focus` — use `var(--bmc-accent-glow-soft)` for box-shadow
4. `.glow-amber` — use `var(--bmc-glow)` instead of hardcoded rgba
5. `.btn-close` — use `var(--bmc-close-btn-filter)` variable (dark themes set `invert(1) brightness(0.8)`, light theme sets `none`)

#### [MODIFY] [_themes.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/assets/styles/_themes.scss)

Add `--bmc-close-btn-filter` to all 8 theme blocks (existing 3 + new 5). Dark themes: `invert(1) brightness(0.8)`. Light theme (Brick Builder): `none`.

---

## Verification Plan

### Automated Build
- Run `ng build` from `g:\source\repos\Scheduler\BMC\BMC.Client` to confirm zero compilation errors

### Manual Browser Verification
- Serve the app with `ng serve` and cycle through all 8 themes using the header swatch picker
- Verify each theme changes the full UI (header, sidebar, cards, buttons, text, scrollbar, borders)
- Specifically verify the Brick Builder light theme renders readable text, correct form focus rings, and the modal close button is visible
