using System;
using System.Collections.Generic;

namespace BMC.Physics
{
    /// <summary>
    /// The main orchestrator for the BMC physics simulation.
    /// 
    /// SimulationEngine manages a collection of rigid bodies and constraints,
    /// stepping the simulation forward in time using a fixed timestep.
    /// 
    /// Architecture:
    /// - Uses a semi-implicit Euler integration scheme
    /// - Constraints are solved iteratively using Sequential Impulse
    /// - Collision detection uses broad-phase (AABB) followed by narrow-phase
    /// 
    /// Usage:
    ///   var engine = new SimulationEngine();
    ///   engine.AddRigidBody(body);
    ///   engine.StepSimulation(deltaTimeSeconds);
    /// </summary>
    public class SimulationEngine
    {
        // ── Configuration ──

        /// <summary>
        /// Gravity acceleration in LDU per second squared (default: -9800 LDU/s² ≈ -9.8 m/s² at 0.4mm/LDU).
        /// </summary>
        public float GravityY { get; set; } = -9800f;

        /// <summary>
        /// Number of solver iterations per step. Higher = more accurate constraints but slower.
        /// </summary>
        public int SolverIterations { get; set; } = 8;

        /// <summary>
        /// Whether the simulation is currently running.
        /// </summary>
        public bool IsRunning { get; private set; } = false;

        /// <summary>
        /// The current simulation time in seconds.
        /// </summary>
        public double SimulationTime { get; private set; } = 0.0;

        /// <summary>
        /// Total number of steps executed since the simulation started.
        /// </summary>
        public long StepCount { get; private set; } = 0;


        // ── State ──

        private readonly List<RigidBody> _bodies = new List<RigidBody>();


        /// <summary>
        /// Gets the current list of rigid bodies in the simulation (read-only).
        /// </summary>
        public IReadOnlyList<RigidBody> Bodies => _bodies.AsReadOnly();


        /// <summary>
        /// Creates a new simulation engine with default settings.
        /// </summary>
        public SimulationEngine()
        {
        }


        /// <summary>
        /// Adds a rigid body to the simulation.
        /// </summary>
        /// <param name="body">The rigid body to add.</param>
        public void AddRigidBody(RigidBody body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));

            _bodies.Add(body);
        }


        /// <summary>
        /// Removes a rigid body from the simulation.
        /// </summary>
        /// <param name="body">The rigid body to remove.</param>
        /// <returns>True if the body was found and removed.</returns>
        public bool RemoveRigidBody(RigidBody body)
        {
            return _bodies.Remove(body);
        }


        /// <summary>
        /// Steps the simulation forward by the specified time delta.
        /// 
        /// This performs the following in order:
        /// 1. Apply external forces (gravity)
        /// 2. Broad-phase collision detection
        /// 3. Narrow-phase collision detection  
        /// 4. Solve constraints (iterative)
        /// 5. Integrate velocities → positions
        /// </summary>
        /// <param name="deltaTimeSeconds">Time step in seconds. Typical game values: 1/60 or 1/120.</param>
        public void StepSimulation(float deltaTimeSeconds)
        {
            if (deltaTimeSeconds <= 0f) return;

            // Phase 1: Apply gravity to all dynamic bodies
            ApplyGravity(deltaTimeSeconds);

            // Phase 2: Collision detection (TODO)
            // DetectCollisions();

            // Phase 3: Solve constraints (TODO)
            // SolveConstraints(deltaTimeSeconds);

            // Phase 4: Integrate - update positions from velocities
            Integrate(deltaTimeSeconds);

            SimulationTime += deltaTimeSeconds;
            StepCount++;
        }


        /// <summary>
        /// Starts the simulation.
        /// </summary>
        public void Start()
        {
            IsRunning = true;
        }

        /// <summary>
        /// Pauses the simulation.
        /// </summary>
        public void Pause()
        {
            IsRunning = false;
        }

        /// <summary>
        /// Resets the simulation to its initial state.
        /// </summary>
        public void Reset()
        {
            IsRunning = false;
            SimulationTime = 0.0;
            StepCount = 0;
            // Note: does not remove bodies — call Clear() for that.
        }

        /// <summary>
        /// Removes all rigid bodies and resets the simulation.
        /// </summary>
        public void Clear()
        {
            _bodies.Clear();
            Reset();
        }


        // ── Private Methods ──

        private void ApplyGravity(float dt)
        {
            for (int i = 0; i < _bodies.Count; i++)
            {
                RigidBody body = _bodies[i];
                if (body.IsStatic) continue;

                // Apply gravitational acceleration: v += g * dt
                body.VelocityY += GravityY * dt;
            }
        }

        private void Integrate(float dt)
        {
            for (int i = 0; i < _bodies.Count; i++)
            {
                RigidBody body = _bodies[i];
                if (body.IsStatic) continue;

                // Update position: p += v * dt
                body.PositionX += body.VelocityX * dt;
                body.PositionY += body.VelocityY * dt;
                body.PositionZ += body.VelocityZ * dt;

                // Update orientation from angular velocity (simplified — proper quaternion integration in future)
                // For now, using small-angle approximation: q += 0.5 * dt * omega * q
                float halfDt = 0.5f * dt;
                float ox = body.AngularVelocityX;
                float oy = body.AngularVelocityY;
                float oz = body.AngularVelocityZ;

                float qx = body.RotationX;
                float qy = body.RotationY;
                float qz = body.RotationZ;
                float qw = body.RotationW;

                body.RotationX += halfDt * (oy * qz - oz * qy + ox * qw);
                body.RotationY += halfDt * (oz * qx - ox * qz + oy * qw);
                body.RotationZ += halfDt * (ox * qy - oy * qx + oz * qw);
                body.RotationW += halfDt * (-ox * qx - oy * qy - oz * qz);

                // Re-normalize the quaternion to prevent drift
                float len = (float)Math.Sqrt(
                    body.RotationX * body.RotationX +
                    body.RotationY * body.RotationY +
                    body.RotationZ * body.RotationZ +
                    body.RotationW * body.RotationW);

                if (len > 0f)
                {
                    float invLen = 1f / len;
                    body.RotationX *= invLen;
                    body.RotationY *= invLen;
                    body.RotationZ *= invLen;
                    body.RotationW *= invLen;
                }
            }
        }
    }
}
