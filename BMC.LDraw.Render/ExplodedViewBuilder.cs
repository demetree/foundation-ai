using System;
using System.Collections.Generic;
using BMC.LDraw.Models;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Produces an "exploded view" of an LDraw mesh by pushing parts radially
    /// outward from the model's centroid.
    ///
    /// Each top-level subfile reference becomes a separate "part group" whose
    /// triangles and edge lines are offset along the vector from the model
    /// centroid to the part-group centroid.
    ///
    /// AI-generated — Phase 4.3, Feb 2026.
    /// </summary>
    public static class ExplodedViewBuilder
    {
        /// <summary>
        /// Build an exploded mesh from a resolved model.
        /// </summary>
        /// <param name="mesh">The fully resolved mesh (all triangles in world space).</param>
        /// <param name="partTriangleCounts">
        /// Number of mesh triangles contributed by each top-level subfile reference (in order).
        /// Sum should equal mesh.Triangles.Count (or less, with the remainder being root-level geometry).
        /// </param>
        /// <param name="partEdgeCounts">
        /// Number of mesh edge lines contributed by each top-level subfile reference (in order).
        /// </param>
        /// <param name="explosionFactor">
        /// How far to push parts apart.  0 = no change, 1.0 = offset by one bounding-radius,
        /// 2.0 = offset by two radii, etc.
        /// </param>
        /// <returns>A new mesh with translated triangle/edge positions.</returns>
        public static LDrawMesh Explode(
            LDrawMesh mesh,
            List<int> partTriangleCounts,
            List<int> partEdgeCounts,
            float explosionFactor = 1.0f)
        {
            if (explosionFactor <= 0f || partTriangleCounts == null || partTriangleCounts.Count == 0)
            {
                return mesh; // Nothing to explode
            }

            //
            // Compute model centroid from all triangles
            //
            mesh.GetCenter(out float modelCX, out float modelCY, out float modelCZ);
            float modelExtent = mesh.GetMaxExtent();

            //
            // Build the exploded mesh
            //
            LDrawMesh exploded = new LDrawMesh();

            int triOffset = 0;
            int edgeOffset = 0;

            for (int p = 0; p < partTriangleCounts.Count; p++)
            {
                int triCount = partTriangleCounts[p];
                int edgeCount = (p < partEdgeCounts.Count) ? partEdgeCounts[p] : 0;

                //
                // Compute this part's centroid
                //
                float pcx = 0, pcy = 0, pcz = 0;
                int count = 0;
                for (int t = triOffset; t < triOffset + triCount && t < mesh.Triangles.Count; t++)
                {
                    MeshTriangle tri = mesh.Triangles[t];
                    pcx += (tri.X1 + tri.X2 + tri.X3);
                    pcy += (tri.Y1 + tri.Y2 + tri.Y3);
                    pcz += (tri.Z1 + tri.Z2 + tri.Z3);
                    count += 3;
                }

                float offsetX = 0, offsetY = 0, offsetZ = 0;

                if (count > 0)
                {
                    pcx /= count;
                    pcy /= count;
                    pcz /= count;

                    //
                    // Direction from model centroid to part centroid
                    //
                    float dx = pcx - modelCX;
                    float dy = pcy - modelCY;
                    float dz = pcz - modelCZ;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);

                    if (dist > 0.01f)
                    {
                        float scale = explosionFactor * modelExtent * 0.3f / dist;
                        offsetX = dx * scale;
                        offsetY = dy * scale;
                        offsetZ = dz * scale;
                    }
                }

                //
                // Offset triangles
                //
                for (int t = triOffset; t < triOffset + triCount && t < mesh.Triangles.Count; t++)
                {
                    MeshTriangle tri = mesh.Triangles[t];
                    MeshTriangle mt = tri;
                    mt.X1 += offsetX; mt.Y1 += offsetY; mt.Z1 += offsetZ;
                    mt.X2 += offsetX; mt.Y2 += offsetY; mt.Z2 += offsetZ;
                    mt.X3 += offsetX; mt.Y3 += offsetY; mt.Z3 += offsetZ;
                    exploded.Triangles.Add(mt);
                }

                //
                // Offset edge lines
                //
                for (int e = edgeOffset; e < edgeOffset + edgeCount && e < mesh.EdgeLines.Count; e++)
                {
                    MeshLine ml = mesh.EdgeLines[e];
                    ml.X1 += offsetX; ml.Y1 += offsetY; ml.Z1 += offsetZ;
                    ml.X2 += offsetX; ml.Y2 += offsetY; ml.Z2 += offsetZ;
                    exploded.EdgeLines.Add(ml);
                }

                triOffset += triCount;
                edgeOffset += edgeCount;
            }

            //
            // Add any remaining root-level geometry (not associated with a subfile ref)
            //
            for (int t = triOffset; t < mesh.Triangles.Count; t++)
            {
                exploded.Triangles.Add(mesh.Triangles[t]);
            }
            for (int e = edgeOffset; e < mesh.EdgeLines.Count; e++)
            {
                exploded.EdgeLines.Add(mesh.EdgeLines[e]);
            }

            return exploded;
        }
    }
}
