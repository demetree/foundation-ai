# BMC 3D Builder & Physics Simulation — Research Document

> **Status:** Pre-development research  
> **Date:** March 14, 2026  
> **Goal:** Build a browser-based LEGO 3D builder with physics simulation for Technic mechanisms (gears, axles, motors)

---

## 1. Competitive Landscape

### Mecabricks (mecabricks.com)
**What it is:** Browser-based 3D LEGO modeling + rendering platform (Three.js / WebGL).

**Strengths we should adopt:**
| Pattern | Detail |
|---------|--------|
| **Snap-point system** | Every part has typed connection points (studs, anti-studs, axle holes, pin holes). Parts auto-snap to compatible points. This is the foundational data structure for both building AND physics. |
| **"Flexible" property** | Specific parts (hoses, capes, strings) are flagged as deformable. Manual posing via spline curves rather than full physics. Great Phase 1 approach. |
| **Local vs Global coords** | Transform panel supports both spaces. Critical for Technic — gears rotate on their axle axis, not world-Y. |
| **Deep category taxonomy** | `Technic → Gears → 24-Tooth Gear`. With 30,000+ parts, flat lists fail. |
| **Scene graph / hierarchy** | Right-panel tree view with groups, hide/unhide, nesting. Essential at 1,000+ parts. |
| **Keyboard shortcuts** | `M` = translate, `R` = rotate, `S` = snap. Standard 3D app conventions reduce learning curve. |

**Weaknesses we should exploit:**
| Gap | BMC Opportunity |
|-----|----------------|
| **No physics at all** | Gears don't drive each other, axles don't rotate. This is the #1 gap in every browser-based LEGO tool. **Blue ocean for BMC.** |
| **Clunky part placement** | Double-click → spawns at origin → manually position. No contextual "click a connection point and auto-place." |
| **No undo feedback** | No visual action history. Critical for debugging complex mechanisms. |
| **No mobile** | Desktop-only editor. BMC already has mobile layouts. |
| **Dated design** | ~2015 aesthetic. No dark mode, no glassmorphism. BMC is ahead visually. |

### Other Tools
| Tool | Notes |
|------|-------|
| **BrickLink Studio** | Desktop. Basic hinge animator, no gear simulation. Large user base. |
| **Virtual Robotics Toolkit** | Desktop. Mindstorms EV3 simulation with full physics. Closest to what we want, but desktop-only and Mindstorms-specific. |
| **LPub3D** | LDraw instruction editor. No building, no physics. Good reference for LDraw file handling. |
| **Build With Chrome** (discontinued) | Google + LEGO collab. No physics. Gone. |

---

## 2. Technology Stack Recommendations

### Rendering: Three.js
- Already proven by Mecabricks at scale
- BMC already has Three.js experience via the part-renderer and GLB pipeline
- Mature ecosystem, huge community, excellent TypeScript support

### Physics Engine: Rapier (WASM)

| Engine | Verdict |
|--------|---------|
| **Rapier** ✅ | **Recommended.** Fast (Rust → WASM), modern API, excellent joint/constraint system designed for robots and vehicles. Perfect for gears/axles/motors. Official JS/WASM bindings. |
| **Ammo.js** | Bullet port. Battle-tested but older API, larger binary size. Fallback option. |
| **Cannon-es** | Lightweight, good Three.js integration, but constraint system too simplistic for Technic mechanisms. |

**Why Rapier:**
- Native joint constraints: revolute (axle rotation), prismatic (linear slide), fixed, ball
- Gear ratio simulation via motorized joints with target velocities
- WASM = near-native performance in the browser
- Designed for exactly the kind of mechanical simulation we need

### Threading: Web Workers
- Physics simulation **must** run off the main thread
- SharedArrayBuffer for position/rotation sync between physics worker and Three.js renderer
- Keeps UI responsive even with 500+ parts in simulation

---

## 3. Proposed Architecture

```
┌─────────────────────────────────────────────────┐
│  BMC 3D Builder (Angular Component)              │
├──────────┬──────────────┬───────────────────────┤
│ Viewport │ Parts Palette │ Properties Panel      │
│ (Three.js│ (search,      │ - Transform (XYZ)     │
│  WebGL)  │  categories,  │ - Connection Points   │
│          │  favorites)   │ - Physics Properties  │
│          │              │ - Scene Graph Tree    │
├──────────┴──────────────┴───────────────────────┤
│  Connection Graph (source of truth)              │
│  - Snap points with typed metadata               │
│  - Compatible connection matching                │
│  - Rigid groups vs jointed connections           │
├──────────────────────────────────────────────────┤
│  Physics Layer (Rapier WASM in Web Worker)        │
│  - Gear mesh constraints (ratio-driven)          │
│  - Axle rotation joints (revolute)               │
│  - Motor torque sources (motorized joints)       │
│  - Pin connections (ball joints)                 │
│  - Friction, gravity, collision                  │
├──────────────────────────────────────────────────┤
│  LDraw Geometry Pipeline (existing)              │
│  - Part mesh loading                             │
│  - GLB caching layer                             │
│  - Connection point metadata from LDraw files    │
└──────────────────────────────────────────────────┘
```

### Key Design Decisions
1. **Connection Graph is the source of truth** — drives both rendering AND physics. When Gear A connects to Axle B, the graph knows the gear ratio and the physics engine applies it.
2. **"Build Mode" vs "Simulate Mode"** — static building (like Mecabricks), then press ▶️ Play to activate physics.
3. **Contextual placement** — click an open connection point → palette filters to compatible parts → click to auto-place. This alone beats every existing tool's UX.
4. **Action history panel** — visual undo/redo with descriptive labels ("Placed Gear 24T on Axle 5", "Rotated Beam 15°").

---

## 4. Phased Implementation Plan

### Phase 1: Static Builder (Foundation)
> Get parts on screen, connected, and looking right.

- [ ] Viewport with Three.js scene (lights, camera, orbit controls)
- [ ] Parts palette with search and LDraw category taxonomy
- [ ] Part placement via click-on-scene (spawn at origin initially)
- [ ] Transform gizmos (translate, rotate) with local/global toggle
- [ ] Snap-point system — define connection metadata per part type
- [ ] Auto-snapping when parts are near compatible connection points
- [ ] Scene graph panel (tree hierarchy, groups, hide/show)
- [ ] Undo/redo with action history
- [ ] Save/load models (to server, tied to MOCHub projects)
- [ ] Keyboard shortcuts (W/E/R for translate/rotate/scale)

### Phase 2: Flexible Properties (Manual Posing)
> Parts know their constraints but don't simulate forces.

- [ ] "Flexible" flag on specific part types (hoses, axles, Technic beams)
- [ ] Axle parts: manual rotation along their axis
- [ ] Hinge parts: manual rotation within valid range
- [ ] Gear visual meshing — gears connected via shared axle visually display together
- [ ] Connection validation — warn when parts overlap or have impossible connections
- [ ] Contextual placement — click connection point → filter palette → auto-place

### Phase 3: Physics Simulation (The Differentiator)
> Press Play and watch mechanisms come alive.

- [ ] Rapier WASM integration in a Web Worker
- [ ] Build mode ↔ Simulate mode toggle
- [ ] Revolute joints for axle rotation
- [ ] Gear ratio constraints (e.g., 8T driving 24T = 3:1 ratio)
- [ ] Motor parts as torque sources (set RPM/torque)
- [ ] Gravity + friction
- [ ] Collision detection for moving parts
- [ ] Performance optimization (LOD, instancing, sleep states for stationary parts)
- [ ] Timeline/playback controls (play, pause, step, rewind)

### Phase 4: Advanced Simulation & Polish
> Power-user features and sharing.

- [ ] Pneumatic cylinder simulation
- [ ] Linear actuator physics
- [ ] Differential gear logic
- [ ] Shareable simulation links ("see my mechanism in action")
- [ ] Export mechanism as animated GIF/video
- [ ] Mobile viewer mode (view + play simulations, no building)
- [ ] Performance: SharedArrayBuffer sync, instanced rendering

---

## 5. LDraw Connection Point Strategy

LDraw files already contain some connectivity information. The key challenge is extracting and augmenting it:

### What LDraw provides:
- Part geometry (vertices, faces) — ✅ already loaded in BMC
- Subpart references — ✅ used for rendering
- Some connection hints via naming conventions (e.g., `s\3005s01.dat` for stud subpart)

### What we need to add:
- **Typed connection points** — each part needs a list of `{ position, normal, type }` where type is:
  - `stud` / `anti-stud` (standard brick connection)
  - `axle-male` / `axle-female` (Technic axle connections)
  - `pin-male` / `pin-female` (Technic pin connections)
  - `ball` / `ball-socket` (ball joint connections)
  - `hinge-male` / `hinge-female`
- **Connection compatibility rules** — which types can connect to which
- **Physics metadata** — gear tooth count, axle length, motor torque specs

### Approach:
1. Start with a curated set of ~200 most common Technic parts with manually defined connection points
2. Build a connection point editor tool to speed up adding more parts
3. Long-term: auto-detect connection points from geometry analysis (stud detection, hole detection)

---

## 6. Performance Budget

Target: **60 FPS** with **500 parts** in Build mode, **200 parts** in Simulate mode.

| Technique | Purpose |
|-----------|---------|
| **Instanced rendering** | Many identical parts (studs, common bricks) rendered as GPU instances |
| **LOD (Level of Detail)** | Distant parts use simplified meshes |
| **Web Worker physics** | Physics off main thread via Rapier in Worker |
| **SharedArrayBuffer** | Zero-copy position/rotation sync between worker and renderer |
| **Frustum culling** | Don't render parts outside camera view |
| **Sleep states** | Physics engine ignores stationary rigid bodies |
| **GLB cache** | Existing BMC cache layer for pre-compiled geometry |

---

## 7. Key Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| LDraw connection metadata is incomplete | Parts don't snap correctly | Start with curated Technic subset; build connection editor |
| Physics performance with many gears | Frame drops in simulation | Web Worker + sleep states + LOD; cap simulated parts |
| Three.js ↔ Rapier sync complexity | Rendering jitter during simulation | SharedArrayBuffer with double buffering; interpolation |
| Scope creep (too many part types) | Never ships | Phase 1 is static builder only; physics is Phase 3 |
| User learning curve | Low adoption | Contextual placement UX; interactive tutorial; "simple mode" |

---

## 8. References

- [Mecabricks Workshop](https://www.mecabricks.com/en/workshop) — competitive reference for editor UX
- [Rapier Physics Engine](https://rapier.rs/) — recommended physics engine (WASM)
- [Three.js](https://threejs.org/) — rendering framework
- [LDraw.org](https://www.ldraw.org/) — part geometry source
- [Cannon-es + Three.js LEGO builder example](https://bionichaos.com) — proof of concept for browser LEGO physics
- [Virtual Robotics Toolkit](https://virtualroboticstoolkit.com/) — desktop Mindstorms simulation reference
