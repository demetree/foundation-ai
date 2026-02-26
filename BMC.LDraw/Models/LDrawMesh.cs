using System.Collections.Generic;

namespace BMC.LDraw.Models
{
    /// <summary>
    /// A single resolved triangle in world space, ready for rendering.
    ///
    /// Contains both a flat face normal (NX, NY, NZ) and optional per-vertex normals
    /// (NX1..NZ3) for smooth Gouraud shading.  When HasPerVertexNormals is false,
    /// the renderer uses the flat normal for the entire face.
    ///
    /// AI-generated — per-vertex normals added Feb 2026 (Phase 1.2).
    /// </summary>
    public struct MeshTriangle
    {
        //
        // Vertex positions (world-space)
        //
        public float X1, Y1, Z1;
        public float X2, Y2, Z2;
        public float X3, Y3, Z3;

        //
        // Flat normal (computed from cross product of triangle edges)
        //
        public float NX, NY, NZ;

        //
        // Per-vertex normals for smooth (Gouraud) shading.
        // Only meaningful when HasPerVertexNormals is true.
        //
        public float NX1, NY1, NZ1;
        public float NX2, NY2, NZ2;
        public float NX3, NY3, NZ3;

        /// <summary>
        /// When true, the renderer should use per-vertex normals (NX1..NZ3) for
        /// lighting interpolation instead of the flat normal (NX, NY, NZ).
        /// </summary>
        public bool HasPerVertexNormals;

        /// <summary>
        /// When true, the renderer should back-face cull this triangle (skip if
        /// screen-space winding is clockwise).  Set to false for non-BFC-certified
        /// parts and transparent surfaces so both sides are always rendered.
        /// </summary>
        public bool CullBackFace;

        //
        // Face colour (RGBA, 0–255)
        //
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
    /// A resolved conditional line (LDraw Type 5) in world space.
    ///
    /// The line from (X1,Y1,Z1) to (X2,Y2,Z2) is only drawn when the two
    /// control points are on opposite sides of the line in screen space.
    /// This produces silhouette edges at boundaries between front-facing
    /// and back-facing surfaces — edges that naturally appear only at the
    /// outline of curved geometry.
    /// </summary>
    public struct MeshConditionalLine
    {
        // Line endpoints
        public float X1, Y1, Z1;
        public float X2, Y2, Z2;

        // Control points for the visibility test
        public float CX1, CY1, CZ1;
        public float CX2, CY2, CZ2;

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

        /// <summary>All resolved edge lines in world space (Type 2).</summary>
        public List<MeshLine> EdgeLines = new List<MeshLine>();

        /// <summary>All resolved conditional lines in world space (Type 5).</summary>
        public List<MeshConditionalLine> ConditionalLines = new List<MeshConditionalLine>();

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
        /// <summary>
        /// Split triangles into opaque and transparent groups for two-pass rendering.
        /// Opaque triangles have A == 255; transparent triangles have A &lt; 255.
        /// </summary>
        public void SplitByTransparency(out List<MeshTriangle> opaque, out List<MeshTriangle> transparent)
        {
            opaque = new List<MeshTriangle>();
            transparent = new List<MeshTriangle>();

            for (int i = 0; i < Triangles.Count; i++)
            {
                if (Triangles[i].A == 255)
                {
                    opaque.Add(Triangles[i]);
                }
                else
                {
                    transparent.Add(Triangles[i]);
                }
            }
        }
    }
}
