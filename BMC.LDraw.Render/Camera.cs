using System;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Camera for 3D rendering with view and projection matrix computation.
    /// All matrices are 4x4 row-major float arrays.
    /// </summary>
    public class Camera
    {
        /// <summary>Camera world position.</summary>
        public float EyeX, EyeY, EyeZ;

        /// <summary>Point the camera is looking at.</summary>
        public float TargetX, TargetY, TargetZ;

        /// <summary>Up vector.</summary>
        public float UpX, UpY, UpZ;

        /// <summary>Field of view in degrees (for perspective projection).</summary>
        public float FieldOfView = 45f;

        /// <summary>Near clipping plane distance.</summary>
        public float NearPlane = 1f;

        /// <summary>Far clipping plane distance.</summary>
        public float FarPlane = 10000f;

        /// <summary>Whether to use orthographic (true) or perspective (false) projection.</summary>
        public bool Orthographic = false;

        /// <summary>Orthographic view width (only used when Orthographic = true).</summary>
        public float OrthoWidth = 100f;

        public Camera()
        {
            EyeX = 0; EyeY = 0; EyeZ = 100;
            TargetX = 0; TargetY = 0; TargetZ = 0;
            UpX = 0; UpY = -1; UpZ = 0; // LDraw Y-axis points down
        }

        /// <summary>
        /// Compute the view matrix (LookAt).
        /// </summary>
        public float[] GetViewMatrix()
        {
            // Forward direction (camera looks along -Z in its own space)
            float fx = TargetX - EyeX;
            float fy = TargetY - EyeY;
            float fz = TargetZ - EyeZ;
            Normalize(ref fx, ref fy, ref fz);

            // Right = forward × up
            float rx, ry, rz;
            Cross(fx, fy, fz, UpX, UpY, UpZ, out rx, out ry, out rz);
            Normalize(ref rx, ref ry, ref rz);

            // Recomputed up = right × forward
            float ux, uy, uz;
            Cross(rx, ry, rz, fx, fy, fz, out ux, out uy, out uz);

            return new float[]
            {
                rx, ry, rz, -(rx * EyeX + ry * EyeY + rz * EyeZ),
                ux, uy, uz, -(ux * EyeX + uy * EyeY + uz * EyeZ),
                -fx, -fy, -fz, (fx * EyeX + fy * EyeY + fz * EyeZ),
                0, 0, 0, 1
            };
        }

        /// <summary>
        /// Compute the projection matrix.
        /// </summary>
        public float[] GetProjectionMatrix(float aspectRatio)
        {
            if (Orthographic)
            {
                return GetOrthographicMatrix(aspectRatio);
            }
            return GetPerspectiveMatrix(aspectRatio);
        }

        private float[] GetPerspectiveMatrix(float aspect)
        {
            float fovRad = FieldOfView * (float)Math.PI / 180f;
            float tanHalfFov = (float)Math.Tan(fovRad * 0.5f);
            float f = 1f / tanHalfFov;
            float rangeInv = 1f / (NearPlane - FarPlane);

            return new float[]
            {
                f / aspect, 0, 0, 0,
                0, f, 0, 0,
                0, 0, (FarPlane + NearPlane) * rangeInv, 2f * FarPlane * NearPlane * rangeInv,
                0, 0, -1, 0
            };
        }

        private float[] GetOrthographicMatrix(float aspect)
        {
            float halfW = OrthoWidth * 0.5f;
            float halfH = halfW / aspect;
            float rangeInv = 1f / (NearPlane - FarPlane);

            return new float[]
            {
                1f / halfW, 0, 0, 0,
                0, 1f / halfH, 0, 0,
                0, 0, 2f * rangeInv, (FarPlane + NearPlane) * rangeInv,
                0, 0, 0, 1
            };
        }

        /// <summary>
        /// Automatically position the camera to frame a mesh's bounding box.
        /// Uses the default isometric-like viewing angle (30° elevation, -45° azimuth).
        /// </summary>
        public void AutoFrame(Models.LDrawMesh mesh)
        {
            AutoFrame(mesh, 30f, -45f);
        }

        /// <summary>
        /// Automatically position the camera to frame a mesh's bounding box
        /// using the specified elevation and azimuth angles.
        /// </summary>
        /// <param name="mesh">The mesh to frame.</param>
        /// <param name="elevationDeg">Camera elevation angle in degrees (0 = eye level, 90 = top-down).</param>
        /// <param name="azimuthDeg">Camera azimuth angle in degrees (0 = front, -90 = right side).</param>
        public void AutoFrame(Models.LDrawMesh mesh, float elevationDeg, float azimuthDeg)
        {
            AutoFrame(mesh, elevationDeg, azimuthDeg, 1.8f);
        }

        /// <summary>
        /// Automatically position the camera with a custom distance multiplier.
        /// </summary>
        public void AutoFrame(Models.LDrawMesh mesh, float elevationDeg, float azimuthDeg, float distanceMultiplier)
        {
            mesh.GetCenter(out float cx, out float cy, out float cz);
            float extent = mesh.GetMaxExtent();
            if (extent < 1f) extent = 100f;

            TargetX = cx;
            TargetY = cy;
            TargetZ = cz;

            float distance = extent * distanceMultiplier;
            float elevAngle = elevationDeg * (float)Math.PI / 180f;
            float azimAngle = azimuthDeg * (float)Math.PI / 180f;

            EyeX = cx + distance * (float)Math.Cos(elevAngle) * (float)Math.Sin(azimAngle);
            EyeY = cy - distance * (float)Math.Sin(elevAngle);
            EyeZ = cz + distance * (float)Math.Cos(elevAngle) * (float)Math.Cos(azimAngle);

            NearPlane = distance * 0.01f;
            FarPlane = distance * 10f;
            OrthoWidth = extent * 1.5f;
        }

        /// <summary>
        /// Frame the camera for build step rendering.
        /// Centers on the step mesh and uses its natural extent with a generous
        /// padding buffer.  A floor prevents extreme zoom-in on tiny sub-assemblies.
        /// </summary>
        public void AutoFrameStep(Models.LDrawMesh stepMesh, Models.LDrawMesh fullMesh,
            float elevationDeg, float azimuthDeg)
        {
            stepMesh.GetCenter(out float cx, out float cy, out float cz);
            float stepExtent = stepMesh.GetMaxExtent();
            float fullExtent = fullMesh.GetMaxExtent();
            if (fullExtent < 1f) fullExtent = 100f;

            // Use the step's own extent with generous padding (2.5x) so parts
            // are well-framed.  Apply a floor of 10% of the full model so that
            // very small sub-assemblies don't zoom in to sub-stud level.
            float minExtent = fullExtent * 0.10f;
            float extent = Math.Max(stepExtent * 2.5f, minExtent);
            // Cap at full model extent so we never zoom out further than the full render
            extent = Math.Min(extent, fullExtent);
            if (extent < 1f) extent = 100f;

            TargetX = cx;
            TargetY = cy;
            TargetZ = cz;

            float distance = extent * 1.6f;
            float elevAngle = elevationDeg * (float)Math.PI / 180f;
            float azimAngle = azimuthDeg * (float)Math.PI / 180f;

            EyeX = cx + distance * (float)Math.Cos(elevAngle) * (float)Math.Sin(azimAngle);
            EyeY = cy - distance * (float)Math.Sin(elevAngle);
            EyeZ = cz + distance * (float)Math.Cos(elevAngle) * (float)Math.Cos(azimAngle);

            NearPlane = distance * 0.01f;
            FarPlane = distance * 10f;
            OrthoWidth = extent * 1.5f;
        }

        // ── Vector math helpers ──

        private static void Normalize(ref float x, ref float y, ref float z)
        {
            float len = (float)Math.Sqrt(x * x + y * y + z * z);
            if (len > 1e-8f)
            {
                x /= len; y /= len; z /= len;
            }
        }

        private static void Cross(float ax, float ay, float az, float bx, float by, float bz,
            out float cx, out float cy, out float cz)
        {
            cx = ay * bz - az * by;
            cy = az * bx - ax * bz;
            cz = ax * by - ay * bx;
        }
    }
}
