using System;

namespace BMC.LDraw.Render.RayTracing
{
    /// <summary>
    /// A ray with origin and direction.
    /// </summary>
    public struct Ray
    {
        public float OriginX, OriginY, OriginZ;
        public float DirX, DirY, DirZ;

        // Precomputed reciprocals of direction — avoids repeated division during BVH traversal
        public float InvDirX, InvDirY, InvDirZ;

        public Ray(float ox, float oy, float oz, float dx, float dy, float dz)
        {
            OriginX = ox; OriginY = oy; OriginZ = oz;
            DirX = dx; DirY = dy; DirZ = dz;

            InvDirX = (MathF.Abs(dx) > 1e-8f) ? 1f / dx : float.MaxValue;
            InvDirY = (MathF.Abs(dy) > 1e-8f) ? 1f / dy : float.MaxValue;
            InvDirZ = (MathF.Abs(dz) > 1e-8f) ? 1f / dz : float.MaxValue;
        }
    }


    /// <summary>
    /// Records the details of a ray-triangle intersection.
    /// </summary>
    public struct HitRecord
    {
        /// <summary>Distance from ray origin to the hit point.</summary>
        public float T;

        /// <summary>Surface normal at the hit point.</summary>
        public float NX, NY, NZ;

        /// <summary>Index into the triangle array.</summary>
        public int TriIndex;

        /// <summary>Surface colour at the hit point.</summary>
        public byte R, G, B, A;
    }


    /// <summary>
    /// Axis-Aligned Bounding Box for BVH nodes.
    /// </summary>
    public struct AABB
    {
        public float MinX, MinY, MinZ;
        public float MaxX, MaxY, MaxZ;


        /// <summary>Create an empty (invalid) AABB.</summary>
        public static AABB Empty()
        {
            return new AABB
            {
                MinX = float.MaxValue, MinY = float.MaxValue, MinZ = float.MaxValue,
                MaxX = float.MinValue, MaxY = float.MinValue, MaxZ = float.MinValue
            };
        }


        /// <summary>Expand this AABB to include a point.</summary>
        public void Include(float x, float y, float z)
        {
            if (x < MinX) MinX = x;
            if (y < MinY) MinY = y;
            if (z < MinZ) MinZ = z;
            if (x > MaxX) MaxX = x;
            if (y > MaxY) MaxY = y;
            if (z > MaxZ) MaxZ = z;
        }


        /// <summary>Expand this AABB to include another AABB.</summary>
        public void Include(AABB other)
        {
            if (other.MinX < MinX) MinX = other.MinX;
            if (other.MinY < MinY) MinY = other.MinY;
            if (other.MinZ < MinZ) MinZ = other.MinZ;
            if (other.MaxX > MaxX) MaxX = other.MaxX;
            if (other.MaxY > MaxY) MaxY = other.MaxY;
            if (other.MaxZ > MaxZ) MaxZ = other.MaxZ;
        }


        /// <summary>Returns the index of the longest axis (0=X, 1=Y, 2=Z).</summary>
        public int LongestAxis()
        {
            float dx = MaxX - MinX;
            float dy = MaxY - MinY;
            float dz = MaxZ - MinZ;

            if (dx >= dy && dx >= dz) return 0;
            if (dy >= dz) return 1;
            return 2;
        }


        /// <summary>
        /// Fast ray-AABB intersection test (slab method).
        /// Returns true if the ray hits this box within [tMin, tMax].
        /// Uses precomputed inverse direction for speed.
        /// </summary>
        public bool Intersect(ref Ray ray, float tMin, float tMax)
        {
            float t0, t1;

            // X slab
            t0 = (MinX - ray.OriginX) * ray.InvDirX;
            t1 = (MaxX - ray.OriginX) * ray.InvDirX;
            if (t0 > t1) { float tmp = t0; t0 = t1; t1 = tmp; }
            if (t0 > tMin) tMin = t0;
            if (t1 < tMax) tMax = t1;
            if (tMax < tMin) return false;

            // Y slab
            t0 = (MinY - ray.OriginY) * ray.InvDirY;
            t1 = (MaxY - ray.OriginY) * ray.InvDirY;
            if (t0 > t1) { float tmp = t0; t0 = t1; t1 = tmp; }
            if (t0 > tMin) tMin = t0;
            if (t1 < tMax) tMax = t1;
            if (tMax < tMin) return false;

            // Z slab
            t0 = (MinZ - ray.OriginZ) * ray.InvDirZ;
            t1 = (MaxZ - ray.OriginZ) * ray.InvDirZ;
            if (t0 > t1) { float tmp = t0; t0 = t1; t1 = tmp; }
            if (t0 > tMin) tMin = t0;
            if (t1 < tMax) tMax = t1;
            if (tMax < tMin) return false;

            return true;
        }
    }
}
