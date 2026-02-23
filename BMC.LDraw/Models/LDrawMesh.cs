using System.Collections.Generic;

namespace BMC.LDraw.Models
{
    /// <summary>
    /// A single resolved triangle in world space, ready for rendering.
    /// </summary>
    public struct MeshTriangle
    {
        // Vertex positions (world-space)
        public float X1, Y1, Z1;
        public float X2, Y2, Z2;
        public float X3, Y3, Z3;

        // Flat normal (computed from cross product)
        public float NX, NY, NZ;

        // Face colour (RGBA, 0–255)
        public byte R, G, B, A;
    }

    /// <summary>
    /// A resolved line segment in world space, for edge rendering.
    /// </summary>
    public struct MeshLine
    {
        public float X1, Y1, Z1;
        public float X2, Y2, Z2;
        public byte R, G, B, A;
    }

    /// <summary>
    /// A fully resolved, world-space triangle mesh ready for rendering.
    /// Produced by GeometryResolver from raw LDraw geometry.
    /// </summary>
    public class LDrawMesh
    {
        /// <summary>All resolved triangles in world space.</summary>
        public List<MeshTriangle> Triangles = new List<MeshTriangle>();

        /// <summary>All resolved edge lines in world space.</summary>
        public List<MeshLine> EdgeLines = new List<MeshLine>();

        // Bounding box (computed during resolution)
        public float MinX, MinY, MinZ;
        public float MaxX, MaxY, MaxZ;

        /// <summary>
        /// Recompute the bounding box from all triangle vertices.
        /// </summary>
        public void ComputeBounds()
        {
            if (Triangles.Count == 0) return;

            MinX = float.MaxValue; MinY = float.MaxValue; MinZ = float.MaxValue;
            MaxX = float.MinValue; MaxY = float.MinValue; MaxZ = float.MinValue;

            for (int i = 0; i < Triangles.Count; i++)
            {
                MeshTriangle t = Triangles[i];
                Expand(t.X1, t.Y1, t.Z1);
                Expand(t.X2, t.Y2, t.Z2);
                Expand(t.X3, t.Y3, t.Z3);
            }
        }

        private void Expand(float x, float y, float z)
        {
            if (x < MinX) MinX = x;
            if (y < MinY) MinY = y;
            if (z < MinZ) MinZ = z;
            if (x > MaxX) MaxX = x;
            if (y > MaxY) MaxY = y;
            if (z > MaxZ) MaxZ = z;
        }

        /// <summary>Center of the bounding box.</summary>
        public void GetCenter(out float cx, out float cy, out float cz)
        {
            cx = (MinX + MaxX) * 0.5f;
            cy = (MinY + MaxY) * 0.5f;
            cz = (MinZ + MaxZ) * 0.5f;
        }

        /// <summary>Maximum extent (largest dimension of the bounding box).</summary>
        public float GetMaxExtent()
        {
            float dx = MaxX - MinX;
            float dy = MaxY - MinY;
            float dz = MaxZ - MinZ;
            float max = dx;
            if (dy > max) max = dy;
            if (dz > max) max = dz;
            return max;
        }
    }
}
