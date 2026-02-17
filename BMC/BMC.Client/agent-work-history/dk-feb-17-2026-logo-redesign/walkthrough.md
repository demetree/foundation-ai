# BMC Logo Redesign — Walkthrough

## What Changed

Replaced all branding across four touchpoints with a cohesive isometric brick identity (Slate Indigo + Signal Orange palette).

### Files Modified/Created

| File | Change |
|------|--------|
| [favicon.svg](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/favicon.svg) | New isometric single-stud design |
| [logo-horizontal.svg](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/assets/logo-horizontal.svg) | [NEW] Static reference SVG for header lockup |
| [logo-hero.svg](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/assets/logo-hero.svg) | [NEW] Static reference SVG for login hero |
| [header.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/header/header.component.html) | FontAwesome `fa-cubes` → inline isometric SVG |
| [header.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/header/header.component.scss) | `.brand-icon` box → `.brand-logo` SVG fills + Montserrat font |
| [login.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/login/login.component.html) | CSS brick divs → inline hero SVG with soft shadow |
| [login.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/login/login.component.scss) | `.brick-1/2/3` rules → `.brand-logo-hero` SVG fills |
| [index.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/index.html) | Flat brick animation → physics auto-build loader + Montserrat |
| [_themes.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/assets/styles/_themes.scss) | Added `--bmc-brick-face/shadow/light/stroke` to all 3 themes |

### Theme Adaptations

| Theme | Brick Face | Stud Color |
|-------|-----------|------------|
| Default | `#4A5D6A` (slate) | `#ffa726` (amber) |
| Miami Vice | `#5A4A6A` (purple-slate) | `#ff6ec7` (pink) |
| Matrix | `#3A5A3A` (green-slate) | `#00ff41` (phosphor) |

## Build Verification

```
✅ ng build --configuration=development
   Application bundle generation complete. [12.801 seconds]
   Zero errors
```

## Visual Verification (Manual)

Run `npx ng serve` and check:
1. **Browser tab** — Isometric stud favicon
2. **Hard refresh** — Auto-build loader (base drops → middle clicks → top clicks with accent stud)
3. **`/login`** — Hero SVG centered above "BMC" in Montserrat
4. **Header** — Isometric brick stack + "BMC" wordmark
5. **Theme picker** — Brick/stud colors shift per theme
