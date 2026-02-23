# Lego Universe Improvements — Master Checklist

## Phase 1: Quick Wins (Low effort, high impact)
- [x] Part count range filter in Set Explorer
- [x] Minifig "appears in X sets" count on gallery cards
- [x] Minifig Detail — show theme badges
- [x] Theme Detail — hero visual (use first set image as banner)
- [x] Decade cards on Lego Universe dashboard
- [x] Print-friendly parts list (Set Detail)

## Phase 2: Explorer & Detail Upgrades
- [x] Grid/table view toggle in Set Explorer
  - [x] Add `viewMode` field + localStorage persistence
  - [x] Add toggle buttons in header
  - [x] Add table view HTML + SCSS
- [x] Theme Detail — minifig section
  - [x] Fetch unique minifigs for the theme
  - [x] Add minifig card grid in HTML + SCSS
- [x] Theme Detail — CDK virtual scroll for large themes
  - [x] Remove pageSize cap, load all sets
  - [x] Add client-side search + CDK virtual scroll viewport
  - [x] Add search + viewport SCSS
- [x] Parts Catalog — "used in X sets" link from part detail
- [x] Keyboard shortcuts (S=search, ←/→=carousel, Esc=close)

## Phase 3: Relationship & Discovery Features
- [ ] Similar Sets recommendation engine (Set Detail)
- [ ] Set comparison feature (compare up to 4 sets)
- [ ] Set Timeline / history view (release/retirement dates)

## Phase 4: Collection Integration
- [ ] Own/Want toggle on Set Detail + Minifig Detail heroes
- [ ] Owned/Wanted badges on Set Explorer + Minifig Gallery cards
- [ ] Collection stats on Universe dashboard ("You own X of Y")
- [ ] Theme completion % metric

## Phase 5: Polish & Delight
- [ ] Share links with Open Graph social preview metadata
- [ ] Colour Library — colour timeline/family tree visualization
- [ ] Global brick stats in Parts Universe (total parts, rarest colours, etc.)
- [ ] Alternate builds section on Set Detail (if data exists in schema)

## Deferred
- [ ] Price/market value data — user prefers not to add $ vibe
- [ ] Instructions link — needs data source (user will source data first)
