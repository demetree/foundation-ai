# Add 5 New BMC Themes

## Theme Definitions (CSS + Service Registration)
- [x] Cyberpunk theme in `_themes.scss`
- [x] Retro Arcade theme in `_themes.scss`
- [x] Brick Builder (light) theme in `_themes.scss`
- [x] Blueprint theme in `_themes.scss`
- [x] Pastel Dreams theme in `_themes.scss`
- [x] Register all 5 themes in `theme.service.ts`

## Header Theme Picker
- [x] Add 5 new swatch gradient classes in `header.component.scss`

## Light Theme Compatibility (Brick Builder)
- [x] Replace hardcoded amber rgba values in `styles.scss` with CSS variables
- [x] Fix `.btn-close` filter for light themes
- [x] Add `--bmc-close-btn-filter` variable to all 8 themes

## Verification
- [x] `ng build` passes with no errors
- [ ] Visual review of all 8 themes in browser
