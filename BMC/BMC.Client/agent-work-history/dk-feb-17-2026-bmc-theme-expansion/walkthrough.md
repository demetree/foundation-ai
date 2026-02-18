# BMC Theme Expansion — 7 New Themes

Added 7 new themes to BMC.Client, bringing the total from 3 to 10. Each theme is fully defined with ~40 CSS custom properties and integrates seamlessly with the existing theme picker.

## New Themes

| Theme | Vibe | Primary | Special |
|-------|------|---------|---------|
| Cyberpunk | Sci-fi noir | 🟡 Electric yellow + red | Horizontal noise overlay |
| Retro Arcade | 80s neon | 🟣 Magenta + cyan | Pixel grid overlay |
| Brick Builder | Kid-friendly | 🔴🔵🟡 LEGO primaries | **Light theme** — first non-dark theme |
| Blueprint | CAD engineer | 🔵 Technical blue + silver | Blueprint grid overlay |
| Pastel Dreams | Whimsical | 💜 Lavender + pink | Soft pastel gradient wash |
| Blueprint Light | CAD light | 🔵 Tech blue on white | **Light** + grid overlay |
| Pastel Light | Whimsical light | 💜 Deep purple + pink | **Light** + pastel wash |

## Files Modified

| File | Change |
|------|--------|
| [_themes.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/assets/styles/_themes.scss) | Added 7 new `body[data-theme]` blocks + `--bmc-close-btn-filter` to all 10 themes |
| [theme.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/theme.service.ts) | Registered 7 new entries in `availableThemes` array |
| [header.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/header/header.component.scss) | Added 7 swatch gradient classes |
| [styles.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/styles.scss) | Replaced 5 hardcoded amber rgba values with theme-aware variables |

## Light Theme Compatibility Fixes

The Brick Builder theme exposed hardcoded amber `rgba(255, 167, 38, ...)` values in `styles.scss` that would look wrong on a light background:

- `.badge-bmc` background/border → `var(--bmc-accent-glow-soft)` / `var(--bmc-border-strong)`
- `.btn-bmc-outline:hover` background → `var(--bmc-surface-hover)`
- `.form-control:focus` box-shadow → `var(--bmc-accent-glow-soft)`
- `.btn-close` filter → `var(--bmc-close-btn-filter)` (dark themes: invert, light: none)
- `.glow-amber` → `var(--bmc-glow)`

## Build Verification

`ng build` completed successfully in 24 seconds with no errors. The only warning is a pre-existing Bootstrap CSS selector issue unrelated to this change.
