using System;

namespace BMC.Physics
{
    /// <summary>
    /// Represents a rigid body in the physics simulation.
    /// 
    /// A rigid body has mass, position, orientation, and velocity properties.
    /// It participates in the constraint solver and collision detection systems.
    /// 
    /// In BMC, each placed brick can be treated as a rigid body for simulation purposes,
    /// and constraints between bodies represent the physical connections (studs, pins, axles, gears).
    /// </summary>
    public class RigidBody
    {
        /// <summary>
        /// Unique identifier for this rigid body.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Display name for debugging and UI purposes.
        /// </summary>
        public string Name { get; set; } = string.Empty;


        // ── Mass Properties ──

        /// <summary>
        /// Mass of the body in grams.
        /// </summary>
        public float Mass { get; set; } = 1.0f;

        /// <summary>
        /// Inverse mass (1/mass). Pre-computed for performance. Zero for static (immovable) bodies.
        /// </summary>
        public float InverseMass => IsStatic ? 0f : 1f / Mass;

        /// <summary>
        /// Whether this body is static (immovable). Static bodies have infinite mass and zero velocity.
        /// Used for ground planes, fixed anchor points, etc.
        /// </summary>
        public bool IsStatic { get; set; } = false;


        // ── Position ──

        /// <summary>X position in world coordinates (LDraw units).</summary>
        public float PositionX { get; set; }

        /// <summary>Y position in world coordinates (LDraw units).</summary>
        public float PositionY { get; set; }

        /// <summary>Z position in world coordinates (LDraw units).</summary>
        public float PositionZ { get; set; }


        // ── Orientation (quaternion) ──

        /// <summary>Quaternion X component.</summary>
        public float RotationX { get; set; } = 0f;

        /// <summary>Quaternion Y component.</summary>
        public float RotationY { get; set; } = 0f;

        /// <summary>Quaternion Z component.</summary>
        public float RotationZ { get; set; } = 0f;

        /// <summary>Quaternion W component.</summary>
        public float RotationW { get; set; } = 1f;


        // ── Linear Velocity ──

        /// <summary>Linear velocity X component (LDU per second).</summary>
        public float VelocityX { get; set; }

        /// <summary>Linear velocity Y component (LDU per second).</summary>
        public float VelocityY { get; set; }

        /// <summary>Linear velocity Z component (LDU per second).</summary>
        public float VelocityZ { get; set; }


        // ── Angular Velocity ──

        /// <summary>Angular velocity X component (radians per second).</summary>
        public float AngularVelocityX { get; set; }

        /// <summary>Angular velocity Y component (radians per second).</summary>
        public float AngularVelocityY { get; set; }

        /// <summary>Angular velocity Z component (radians per second).</summary>
        public float AngularVelocityZ { get; set; }


        /// <summary>
        /// Creates a new rigid body at the origin with default properties.
        /// </summary>
        public RigidBody()
        {
        }

        /// <summary>
        /// Creates a new rigid body with the specified name and mass.
        /// </summary>
        public RigidBody(string name, float mass)
        {
            Name = name;
            Mass = mass;
        }

        /// <summary>
        /// Creates a static (immovable) rigid body at the given position.
        /// </summary>
        public static RigidBody CreateStatic(string name, float x, float y, float z)
        {
            return new RigidBody
            {
                Name = name,
                IsStatic = true,
                PositionX = x,
                PositionY = y,
                PositionZ = z
            };
        }
    }
}
