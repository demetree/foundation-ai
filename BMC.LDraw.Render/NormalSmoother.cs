using System;
using System.Collections.Generic;
using BMC.LDraw.Models;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Post-processes an LDrawMesh to compute per-vertex normals for smooth (Gouraud) shading.
    ///
    /// Groups triangles that share vertex positions (within a spatial epsilon) and averages their
    /// face normals at each shared vertex.  A crease-angle threshold prevents normals from being
    /// averaged across sharp edges — for example, the top face of a brick will stay flat even
    /// though its vertices are shared with the side walls.
    ///
    /// AI-generated — Feb 2026 (Phase 1.2).
    /// </summary>
    public static class NormalSmoother
    {
        //
        // Spatial epsilon for grouping vertices.  Two positions within this distance
        // are considered the same vertex for normal averaging purposes.
        //
        private const float POSITION_EPSILON = 0.1f;

        //
        // Default crease angle in degrees.  Face-normal pairs whose angle exceeds this
        // threshold are NOT averaged together — the vertex keeps its flat normal for that
        // face.  60° works well for LEGO bricks, which have both sharp box edges and smooth
        // curved surfaces (cylinders, cones).
        //
        private const float DEFAULT_CREASE_ANGLE_DEGREES = 60f;


        /// <summary>
        /// Compute per-vertex smooth normals on all triangles in the mesh using the default
        /// crease angle of 60°.
        /// </summary>
        public static void Smooth(LDrawMesh mesh)
        {
            Smooth(mesh, DEFAULT_CREASE_ANGLE_DEGREES);
        }


        /// <summary>
        /// Compute per-vertex smooth normals on all triangles in the mesh.
        /// </summary>
        /// <param name="mesh">The mesh whose triangles will be updated in-place.</param>
        /// <param name="creaseAngleDegrees">Angle threshold (in degrees) above which edges are treated as hard/creased.</param>
        public static void Smooth(LDrawMesh mesh, float creaseAngleDegrees)
        {
            if (mesh.Triangles.Count == 0)
            {
                return;
            }

            float creaseAngleRadians = creaseAngleDegrees * (float)Math.PI / 180f;
            float creaseAngleCosine = (float)Math.Cos(creaseAngleRadians);

            //
            // Step 1 — Build a spatial index of all triangle vertices.
            // Each unique position maps to a list of (triangleIndex, vertexIndex) pairs.
            //
            Dictionary<long, List<VertexReference>> spatialMap = BuildSpatialMap(mesh);

            //
            // Step 2 — For each triangle vertex, average the face normals of all triangles sharing
            //           that vertex position, respecting the crease angle.
            //
            List<MeshTriangle> triangleList = mesh.Triangles;
            MeshTriangle[] smoothedTriangleArray = new MeshTriangle[triangleList.Count];

            for (int triangleIndex = 0; triangleIndex < triangleList.Count; triangleIndex++)
            {
                smoothedTriangleArray[triangleIndex] = triangleList[triangleIndex];
            }

            //
            // Process each triangle and compute smooth normals per-vertex
            //
            for (int triangleIndex = 0; triangleIndex < smoothedTriangleArray.Length; triangleIndex++)
            {
                MeshTriangle currentTriangle = smoothedTriangleArray[triangleIndex];

                //
                // Vertex 1
                //
                ComputeSmoothedNormal(triangleIndex: triangleIndex,
                                      vertexIndex: 0,
                                      vertexX: currentTriangle.X1,
                                      vertexY: currentTriangle.Y1,
                                      vertexZ: currentTriangle.Z1,
                                      faceNormalX: currentTriangle.NX,
                                      faceNormalY: currentTriangle.NY,
                                      faceNormalZ: currentTriangle.NZ,
                                      spatialMap: spatialMap,
                                      triangleArray: smoothedTriangleArray,
                                      creaseAngleCosine: creaseAngleCosine,
                                      outNX: out float nx1,
                                      outNY: out float ny1,
                                      outNZ: out float nz1);

                //
                // Vertex 2
                //
                ComputeSmoothedNormal(triangleIndex: triangleIndex,
                                      vertexIndex: 1,
                                      vertexX: currentTriangle.X2,
                                      vertexY: currentTriangle.Y2,
                                      vertexZ: currentTriangle.Z2,
                                      faceNormalX: currentTriangle.NX,
                                      faceNormalY: currentTriangle.NY,
                                      faceNormalZ: currentTriangle.NZ,
                                      spatialMap: spatialMap,
                                      triangleArray: smoothedTriangleArray,
                                      creaseAngleCosine: creaseAngleCosine,
                                      outNX: out float nx2,
                                      outNY: out float ny2,
                                      outNZ: out float nz2);

                //
                // Vertex 3
                //
                ComputeSmoothedNormal(triangleIndex: triangleIndex,
                                      vertexIndex: 2,
                                      vertexX: currentTriangle.X3,
                                      vertexY: currentTriangle.Y3,
                                      vertexZ: currentTriangle.Z3,
                                      faceNormalX: currentTriangle.NX,
                                      faceNormalY: currentTriangle.NY,
                                      faceNormalZ: currentTriangle.NZ,
                                      spatialMap: spatialMap,
                                      triangleArray: smoothedTriangleArray,
                                      creaseAngleCosine: creaseAngleCosine,
                                      outNX: out float nx3,
                                      outNY: out float ny3,
                                      outNZ: out float nz3);

                //
                // Apply the smoothed normals
                //
                smoothedTriangleArray[triangleIndex].NX1 = nx1;
                smoothedTriangleArray[triangleIndex].NY1 = ny1;
                smoothedTriangleArray[triangleIndex].NZ1 = nz1;
                smoothedTriangleArray[triangleIndex].NX2 = nx2;
                smoothedTriangleArray[triangleIndex].NY2 = ny2;
                smoothedTriangleArray[triangleIndex].NZ2 = nz2;
                smoothedTriangleArray[triangleIndex].NX3 = nx3;
                smoothedTriangleArray[triangleIndex].NY3 = ny3;
                smoothedTriangleArray[triangleIndex].NZ3 = nz3;
                smoothedTriangleArray[triangleIndex].HasPerVertexNormals = true;
            }

            //
            // Step 3 — Replace the mesh triangle list with the smoothed results
            //
            mesh.Triangles.Clear();
            mesh.Triangles.AddRange(smoothedTriangleArray);
        }


        /// <summary>
        /// Build a spatial hash map grouping vertex positions.
        /// The key is a quantized hash of the position; the value is every
        /// triangle+vertex at that position.
        /// </summary>
        private static Dictionary<long, List<VertexReference>> BuildSpatialMap(LDrawMesh mesh)
        {
            Dictionary<long, List<VertexReference>> spatialMap = new Dictionary<long, List<VertexReference>>();

            for (int triangleIndex = 0; triangleIndex < mesh.Triangles.Count; triangleIndex++)
            {
                MeshTriangle triangle = mesh.Triangles[triangleIndex];

                AddToSpatialMap(spatialMap, triangle.X1, triangle.Y1, triangle.Z1, triangleIndex, 0);
                AddToSpatialMap(spatialMap, triangle.X2, triangle.Y2, triangle.Z2, triangleIndex, 1);
                AddToSpatialMap(spatialMap, triangle.X3, triangle.Y3, triangle.Z3, triangleIndex, 2);
            }

            return spatialMap;
        }


        /// <summary>
        /// Add a single vertex reference to the spatial map.
        /// </summary>
        private static void AddToSpatialMap(Dictionary<long, List<VertexReference>> spatialMap,
                                            float x, float y, float z,
                                            int triangleIndex, int vertexIndex)
        {
            long key = QuantizePosition(x, y, z);

            if (spatialMap.TryGetValue(key, out List<VertexReference> referenceList) == false)
            {
                referenceList = new List<VertexReference>();
                spatialMap[key] = referenceList;
            }

            referenceList.Add(new VertexReference(triangleIndex, vertexIndex));
        }


        /// <summary>
        /// Quantize a 3D position into a spatial hash key.
        /// Positions within POSITION_EPSILON of each other will hash to the same bucket.
        /// </summary>
        private static long QuantizePosition(float x, float y, float z)
        {
            //
            // Scale and round to the nearest grid cell
            //
            float invEpsilon = 1f / POSITION_EPSILON;
            int qx = (int)Math.Round(x * invEpsilon);
            int qy = (int)Math.Round(y * invEpsilon);
            int qz = (int)Math.Round(z * invEpsilon);

            //
            // Combine into a single 64-bit key (21 bits per axis is plenty for LDraw-scale models)
            //
            long key = ((long)(qx & 0x1FFFFF)) |
                        ((long)(qy & 0x1FFFFF) << 21) |
                        ((long)(qz & 0x1FFFFF) << 42);

            return key;
        }


        /// <summary>
        /// Compute the smoothed normal for a single vertex of a triangle.
        /// Averages the face normals of all neighboring triangles that share this
        /// vertex position and whose face normals are within the crease angle.
        /// </summary>
        private static void ComputeSmoothedNormal(int triangleIndex,
                                                   int vertexIndex,
                                                   float vertexX, float vertexY, float vertexZ,
                                                   float faceNormalX, float faceNormalY, float faceNormalZ,
                                                   Dictionary<long, List<VertexReference>> spatialMap,
                                                   MeshTriangle[] triangleArray,
                                                   float creaseAngleCosine,
                                                   out float outNX, out float outNY, out float outNZ)
        {
            //
            // Start with this triangle's own face normal
            //
            float sumNX = faceNormalX;
            float sumNY = faceNormalY;
            float sumNZ = faceNormalZ;

            //
            // Look up all vertices at this position
            //
            long key = QuantizePosition(vertexX, vertexY, vertexZ);

            if (spatialMap.TryGetValue(key, out List<VertexReference> neighborList) == true)
            {
                for (int i = 0; i < neighborList.Count; i++)
                {
                    VertexReference neighbor = neighborList[i];

                    //
                    // Skip this triangle's own contribution (already added above)
                    //
                    if (neighbor.TriangleIndex == triangleIndex)
                    {
                        continue;
                    }

                    MeshTriangle neighborTriangle = triangleArray[neighbor.TriangleIndex];

                    //
                    // Only average if the position truly matches (handles hash collisions)
                    //
                    float nvx = GetVertexX(ref neighborTriangle, neighbor.VertexIndex);
                    float nvy = GetVertexY(ref neighborTriangle, neighbor.VertexIndex);
                    float nvz = GetVertexZ(ref neighborTriangle, neighbor.VertexIndex);

                    float distanceSq = (nvx - vertexX) * (nvx - vertexX) +
                                       (nvy - vertexY) * (nvy - vertexY) +
                                       (nvz - vertexZ) * (nvz - vertexZ);

                    if (distanceSq > POSITION_EPSILON * POSITION_EPSILON)
                    {
                        continue;
                    }

                    //
                    // Check crease angle — dot product of the face normals.
                    // If the angle between them exceeds the threshold, do not average.
                    //
                    float dotProduct = faceNormalX * neighborTriangle.NX +
                                       faceNormalY * neighborTriangle.NY +
                                       faceNormalZ * neighborTriangle.NZ;

                    if (dotProduct < creaseAngleCosine)
                    {
                        continue;
                    }

                    //
                    // Accumulate the neighbor's face normal
                    //
                    sumNX += neighborTriangle.NX;
                    sumNY += neighborTriangle.NY;
                    sumNZ += neighborTriangle.NZ;
                }
            }

            //
            // Normalize the accumulated result
            //
            float length = (float)Math.Sqrt(sumNX * sumNX + sumNY * sumNY + sumNZ * sumNZ);

            if (length > 1e-8f)
            {
                outNX = sumNX / length;
                outNY = sumNY / length;
                outNZ = sumNZ / length;
            }
            else
            {
                //
                // Fallback to the flat normal if the sum is degenerate
                //
                outNX = faceNormalX;
                outNY = faceNormalY;
                outNZ = faceNormalZ;
            }
        }


        // ── Vertex accessor helpers ──

        private static float GetVertexX(ref MeshTriangle tri, int vertexIndex)
        {
            if (vertexIndex == 0) return tri.X1;
            if (vertexIndex == 1) return tri.X2;
            return tri.X3;
        }


        private static float GetVertexY(ref MeshTriangle tri, int vertexIndex)
        {
            if (vertexIndex == 0) return tri.Y1;
            if (vertexIndex == 1) return tri.Y2;
            return tri.Y3;
        }


        private static float GetVertexZ(ref MeshTriangle tri, int vertexIndex)
        {
            if (vertexIndex == 0) return tri.Z1;
            if (vertexIndex == 1) return tri.Z2;
            return tri.Z3;
        }


        // ── Helper type ──

        /// <summary>
        /// References a specific vertex within a specific triangle.
        /// </summary>
        private struct VertexReference
        {
            public int TriangleIndex;
            public int VertexIndex;

            public VertexReference(int triangleIndex, int vertexIndex)
            {
                TriangleIndex = triangleIndex;
                VertexIndex = vertexIndex;
            }
        }
    }
}
