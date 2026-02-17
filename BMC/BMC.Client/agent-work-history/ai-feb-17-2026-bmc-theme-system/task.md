# BMC.Client Theme System

## Tasks

- [x] Plan the theme system porting from BCDC to BMC.Client
- [x] Create `_themes.scss` with Default (Warm Amber), Miami Vice, and Matrix theme blocks
- [x] Refactor `styles.scss` — move `:root` vars into the Default theme block, import `_themes.scss`
- [x] Create `theme.service.ts` (adapted for BMC with `bmc-theme` storage key)
- [x] Update `header.component.ts` — inject `ThemeService`, add `setTheme()` method
- [ ] Update `header.component.html` — add theme picker swatches
- [ ] Update `header.component.scss` — add theme picker styling + make navbar theme-aware
- [ ] Verify via `ng build`
