# BMC Logo Redesign — Isometric Brick Identity System

Replace all current branding (FontAwesome icons, CSS rectangles, gradient text) with a cohesive isometric brick identity based on a Slate Indigo + Signal Orange palette.

## User Review Required

> [!IMPORTANT]
> **Theme-aware SVG fills**: Since all three themes use dark backgrounds, the inline SVGs (header, login) will use CSS custom properties for brick fills and stud colors — not hardcoded hex. This lets each theme tint the logo naturally (e.g., matrix studs in green, miami-vice studs in pink). The **favicon** remains hardcoded since `<link>` SVGs can't read CSS variables.

> [!IMPORTANT]
> **Montserrat font**: Your design specifies Montserrat for the "BMC" wordmark. The app currently loads **Inter**. I'll add Montserrat alongside Inter (not replace it) so body text stays Inter and only the brand text uses Montserrat.

---

## Proposed Changes

### SVG Logo Assets

#### [NEW] [favicon.svg](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/favicon.svg)
Replace existing warm-amber brick stack with your isometric single-stud design (Slate Indigo body, Signal Orange stud). Hardcoded colors since favicons can't use CSS vars.

#### [NEW] [assets/ directory](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/assets)
Create directory. Add `logo-horizontal.svg` (header lockup) and `logo-hero.svg` (login hero) as static reference copies, though the actual in-app usage will be inline SVGs.

---

### Header Component

#### [MODIFY] [header.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/header/header.component.html)
- Replace the FontAwesome `<i class="fas fa-cubes">` brand icon with an inline isometric brick stack SVG
- SVG uses `currentColor` / CSS variables for fills so it adapts per-theme
- Keep the "BMC" text and "Brick Machine Construction" subtitle, but apply Montserrat to the brand text

#### [MODIFY] [header.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/header/header.component.scss)
- Remove `.brand-icon` gradient background (the SVG provides its own visual)
- Add brand SVG sizing (height 32px)
- Add theme-aware CSS vars for brick faces and stud accent:
  - `--bmc-brick-face` (defaults to `#4A5D6A` — lighter slate for dark backgrounds)
  - `--bmc-brick-shadow` / `--bmc-brick-light` for side faces
  - `--bmc-stud` maps to existing `--bmc-primary`

---

### Login Component

#### [MODIFY] [login.component.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/login/login.component.html)
- Replace the three CSS `<div class="brick">` elements with the inline hero mark SVG (larger isometric stack with soft shadow and glossy stud)
- Keep `<h1>BMC</h1>` and subtitle below the SVG

#### [MODIFY] [login.component.scss](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/app/components/login/login.component.scss)
- Remove `.brand-brick-stack`, `.brick`, `.brick-1/2/3` CSS rules (replaced by SVG)
- Add `.brand-logo-hero` sizing/spacing class
- Keep existing `brand-title` and `brand-subtitle` styles

---

### Pre-Bootstrap Loader (`index.html`)

#### [MODIFY] [index.html](file:///g:/source/repos/Scheduler/BMC/BMC.Client/src/index.html)
- Replace the `.cs-brick-stack` CSS animation with the new "auto-build" physics loader
- Bricks drop/click into place with bounce easing, cycle animation resets
- Update the theme palette variables in `<script>` block to include the new brick/stud colors
- Add Montserrat font `<link>` alongside existing Inter import
- Loader uses the same `--pbl-*` variable pattern for theme consistency

---

## Verification Plan

### Build Verification
```powershell
cd g:\source\repos\Scheduler\BMC\BMC.Client
npx ng build --configuration=development
```
Must complete with zero errors.

### Visual Verification (Manual — Browser)
After starting the dev server (`npx ng serve`):

1. **Favicon**: Check browser tab — should show isometric single-stud in Slate Indigo/Signal Orange
2. **Pre-bootstrap loader**: Hard-refresh the page — watch the brick drop/click/cycle animation before Angular loads
3. **Login page**: Navigate to `/login` — verify the hero SVG shows the isometric stepped stack with glossy stud, centered above "BMC" text
4. **Header**: Log in — verify the navbar shows the horizontal isometric brick lockup + "BMC" wordmark in Montserrat
5. **Theme switching**: Use the theme picker in the header to cycle through default/miami-vice/matrix — confirm the logo SVG tints adapt per theme
