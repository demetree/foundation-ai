# BMC.Physics

Pure C# physics simulation library for BMC (Brick Machine Construction).

## Design Principles

1. **No web or database dependencies** — this is a pure computation library
2. **Framework-agnostic** — can be consumed by Server (API), Worker (background), or even compiled to WASM for client-side
3. **Domain-optimized** — designed specifically for LEGO Technic mechanical simulations, not general-purpose physics

## Core Components

| Component | Status | Purpose |
|-----------|--------|---------|
| `RigidBody` | ✅ Stub | Mass, position, velocity, angular velocity |
| `SimulationEngine` | ✅ Stub | Main loop: gravity, integration, orchestration |
| `ConstraintSolver` | 🔲 Planned | Sequential Impulse solver for all constraints |
| `GearConstraint` | 🔲 Planned | Gear ratio enforcement between rotating bodies |
| `HingeConstraint` | 🔲 Planned | Single-axis rotation (pin connections) |
| `AxleConstraint` | 🔲 Planned | Locked rotation transfer along an axle |
| `FixedConstraint` | 🔲 Planned | Rigid joint (stud connections) |
| `CollisionDetector` | 🔲 Planned | Broad-phase (AABB) + narrow-phase |

## Units

- **Distance**: LDraw units (1 LDU = 0.4mm)
- **Mass**: Grams
- **Time**: Seconds
- **Angles**: Radians
- **Gravity**: -9800 LDU/s² (equivalent to -9.8 m/s²)
