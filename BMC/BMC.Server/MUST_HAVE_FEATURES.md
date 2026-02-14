**To pull users from the big players (BrickLink Studio, LeoCAD, Mecabricks, and the LDraw ecosystem) on baseline functionality alone, you need to match the "table-stakes" features that AFOLs have come to expect after years of using those tools.** These aren't flashy differentiators like your physics sim—they're the foundational things that make a LEGO CAD feel *complete* and worth switching to (or adding to their workflow).

I pulled this from recent 2025–2026 discussions on Reddit (r/lego, r/DigitalLego), Eurobricks, YouTube comparisons, and official docs. Studio is the 800-pound gorilla right now, so beating it on these (or at least equaling it) is key. LeoCAD wins on cross-platform and power-user controls; Mecabricks on ease and renders. Here's the prioritized must-have list, grouped by category:

### 1. Parts Catalog & Library (Your Screenshot is Already Strong Here)
This is non-negotiable—users will bail instantly if it feels incomplete.
- **Huge, always-current LDraw-based library** (10k+ parts, unofficial parts optional, auto-updates).
- **Powerful search**: By name, ID (e.g., 1744), keywords, category, tags, dimensions. Filters for color availability, part type (System, Technic, etc.).
- **Rich previews**: 3D rotatable view (like your 1744 dome), thumbnail gallery, color swatches.
- **"My Collection" style palettes**: Import from BrickLink/Rebrickable or manual—let users build only with what they own (Studio's killer feature for real-world MOCs).
- **Bonus baseline**: Quick-add from set inventories (pull official LEGO set parts lists directly).

### 2. Building Canvas & Interface
The daily driver—has to feel buttery and familiar.
- **Drag-and-drop + smart snapping**: Studs, anti-studs, Technic pins/holes, clips, hinges, bars—all automatic and reliable (Studio's "clutch" feel is legendary; LeoCAD is more manual but precise).
- **Multi-viewport workflow**: Perspective + multiple ortho views side-by-side, easy camera controls (orbit, pan, zoom, fly mode).
- **Selection & manipulation tools**: Box/lasso select, group, hide/isolate, precise rotate/move (with axis locks and numeric input), undo/redo (deep history).
- **Submodels & steps**: Native support for hierarchical builds (like official instructions)—critical for complex MOCs.
- **UI polish**: Keyboard shortcuts that match Studio/LeoCAD (users hate relearning), dark mode (you nailed this), customizable panels, reference image overlay (huge in Studio for proportioning).

### 3. Instructions & Documentation Tools
This is the #1 reason many AFOLs stick with Studio—it's *the* best in class.
- **Auto + manual step generator**: One-click step creation, drag to reorder, submodel support, callouts/arrows.
- **Page designer**: Layout control, exploded views, part lists per step, export to PDF/HTML/interactive.
- **Speed & quality**: Fast preview/rendering of pages (Studio's Instruction Maker is praised for being "lightning quick" compared to LPub).

### 4. Rendering & Visualization
Users want to *show off* their MOCs.
- **Real-time high-quality preview** (PBR materials, lighting).
- **Photorealistic renderer**: Export studio-quality images (Studio's Eyesight, Mecabricks' textures—both are benchmarks).
- **Easy exports**: PNG/JPG at high res, maybe basic animation or turntable.

### 5. Import/Export & Ecosystem Integration
Your LDraw-native core is a massive advantage here—lean into it.
- **Seamless LDraw round-tripping** (.ldr/.mpd full support, including subfiles).
- **BOM/wanted list export**: CSV/XML formatted for BrickLink/Rebrickable (one-click "order these parts").
- **Set import**: Pull any official set's inventory directly (Studio does this brilliantly).
- **Other formats**: .dae/.obj for Blender, maybe basic .io export if feasible.

### 6. Performance & Polish (The "Feels Professional" Stuff)
- **Handles big models** (5k–10k+ parts) without choking.
- **Cross-platform** (Windows/Mac at minimum; Linux support wins LeoCAD fans).
- **Stability basics**: No random crashes, consistent snapping, part collision detection (pre-physics).
- **Help & onboarding**: Built-in tutorials, tooltips, quick-start templates.

### Prioritization for You (Based on Your Current State)
You're already ahead with the parts catalog + LDraw-native DB. Next logical builds:
1. **Building canvas + snapping** (core loop).
2. **Instructions generator** (huge "wow" factor for Studio migrants).
3. **Rendering + BOM export** (makes it feel production-ready).
4. **Set import + palettes** (hooks users who want to recreate or order real bricks).

If you nail these, BMC becomes the "Studio alternative with better physics" instead of "another CAD." Users will try it because it *feels* like the tools they know, then stay for the sim.

Anything here surprise you, or want me to expand on implementation ideas for a specific one (e.g., how Studio's instruction engine works under the hood)? This is the baseline to get them in the door—your physics will be the hook that keeps them. 🚀