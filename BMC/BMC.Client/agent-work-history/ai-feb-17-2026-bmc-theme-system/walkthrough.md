# BMC.Client Theme System — Walkthrough

## Summary

Ported the BCDC multi-theme system into BMC.Client, giving users a visual theme picker (Default amber, Miami Vice, Matrix) accessible from the header navbar.

## Changes Made

### New Files

| File | Purpose |
|------|---------|
| [_themes.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/assets/styles/_themes.scss) | Three `[data-theme]` CSS custom property blocks defining all `--bmc-*` design tokens |
| [theme.service.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/services/theme.service.ts) | `ThemeService` — manages `data-theme` attribute on `<body>`, persists to localStorage (`bmc-theme`) |

### Modified Files

| File | What Changed |
|------|-------------|
| [styles.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/styles.scss) | Moved `--bmc-*` vars from `:root` → `_themes.scss` import; scrollbar/table/button shadows now use theme vars |
| [header.component.ts](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/header/header.component.ts) | Injects `ThemeService`, adds `setTheme()` |
| [header.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/header/header.component.html) | Theme picker (palette icon + 3 swatch buttons) added before nav actions |
| [header.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/header/header.component.scss) | Navbar uses `--bmc-header-bg`/`--bmc-header-border`; brand icon/text use `--bmc-primary*`; theme picker/swatch styles added |

## Build Verification

```
npx ng build --configuration production
```

- ✅ Build completed in **21.6 seconds**
- ✅ Bundle output: `G:\source\repos\Scheduler\BMC\BMC.Client\dist\bmc.client`
- ⚠️ 1 pre-existing CSS warning from Bootstrap (`.form-floating>~label` selector) — unrelated to our changes

## Manual Testing Checklist

After running `ng serve`:

1. **Default theme** should render the familiar warm amber appearance (no regression)
2. **Theme picker** visible in the header — palette icon + three coloured circles
3. **Click Miami Vice** — UI shifts to pink/purple/orange
4. **Click Matrix** — UI shifts to green-on-black
5. **Refresh the page** — theme persists via localStorage
6. **Click Default** — restores amber palette
