# BMC Logo Redesign

## Planning
- [x] Audit existing branding touchpoints
- [x] Review user's design system and SVG specifications
- [x] Write implementation plan
- [x] Get user approval on plan

## Execution
- [x] Create `src/assets/` directory and add SVG logo assets
  - [x] `favicon.svg` — isometric single stud (16×16)
  - [x] `logo-horizontal.svg` — header lockup (32px height)
  - [x] `logo-hero.svg` — login hero mark (120×120)
- [x] Update `src/favicon.svg` with new isometric design
- [x] Update header component
  - [x] Replace FontAwesome `fa-cubes` with inline SVG logo
  - [x] Add theme-aware CSS variables for brick/stud fills
- [x] Update login component
  - [x] Replace CSS brick-stack with inline SVG hero mark
  - [x] Update brand styling
- [x] Update `index.html` pre-bootstrap
  - [x] Replace CSS brick animation with new auto-build loader
  - [x] Update theme palette overrides for new colors
- [x] Add Montserrat font import

## Verification
- [x] Run `ng build` to confirm no errors
- [ ] Visual browser check of all touchpoints
