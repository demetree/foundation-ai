# BMC Feature Implementation — Collection, BOM Export, LDraw Round-Trip

## Phase 1: My Collection Server API
- [x] Plan server + client features
- [x] Get user approval on implementation plan
- [x] Create `CollectionController.cs` with composite endpoints
- [x] Register controller in `Program.cs`
- [x] Fix pre-existing `ColourFinishsController` typo in `Program.cs`
- [x] Verify server builds cleanly
- [x] Create Angular `CollectionService` (client)
- [x] Create `MyCollectionComponent` (client UI)

## Phase 2: BOM / Wanted List Export
- [ ] Add `ExportController.cs` — BrickLink XML + CSV endpoints
- [ ] Support both WANTEDLIST and INVENTORY XML formats (user choice)
- [ ] Create Angular export UI component

## Phase 3: LDraw Round-Trip
- [ ] Extend `BMC.LDraw` to support write-back (MPD/LDR export)
- [ ] Import batch summary with gap reconciliation (accounting-style)
- [ ] Add server endpoint for export-to-LDraw
- [ ] Wire up client UI

## Housekeeping
- [x] Save work history (schema expansion session)
