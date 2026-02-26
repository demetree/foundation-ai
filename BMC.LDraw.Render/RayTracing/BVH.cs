using System;
using System.Collections.Generic;
using BMC.LDraw.Models;

namespace BMC.LDraw.Render.RayTracing
{
    /// <summary>
    /// Bounding Volume Hierarchy for fast ray-triangle intersection.
    ///
    /// Builds a binary tree of axis-aligned bounding boxes around the mesh
    /// triangles. Primary ray queries go from O(N) to O(log N), making ray
    /// tracing practical for meshes with hundreds of thousands of triangles.
    ///
    /// Build strategy: recursive top-down midpoint split on the longest axis.
    /// Leaf nodes contain ≤ MAX_LEAF_SIZE triangles.
    /// </summary>
    public class BVH
    {
        private const int MAX_LEAF_SIZE = 4;

        private BVHNode _root;
        private MeshTriangle[] _triangles; // reference to the triangle array

        /// <summary>Access the triangle data (for material lookups).</summary>
        public MeshTriangle[] Triangles => _triangles;


        /// <summary>
        /// Build the BVH from a list of mesh triangles.
        /// </summary>
        public void Build(List<MeshTriangle> triangles)
        {
            _triangles = triangles.ToArray();

            //
            // Build an index array (we sort indices, not triangles)
            //
            int[] indices = new int[_triangles.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = i;
            }

            //
            // Precompute per-triangle centroids for split decisions
            //
            float[] centroids = new float[_triangles.Length * 3];
            for (int i = 0; i < _triangles.Length; i++)
            {
                MeshTriangle t = _triangles[i];
                centroids[i * 3 + 0] = (t.X1 + t.X2 + t.X3) / 3f;
                centroids[i * 3 + 1] = (t.Y1 + t.Y2 + t.Y3) / 3f;
                centroids[i * 3 + 2] = (t.Z1 + t.Z2 + t.Z3) / 3f;
            }

            _root = BuildRecursive(indices, 0, indices.Length, centroids);
        }


        /// <summary>
        /// Find the closest intersection of a ray with the mesh.
        /// Returns true if a hit was found, and fills in the HitRecord.
        /// </summary>
        public bool Intersect(ref Ray ray, out HitRecord hit)
        {
            hit = default;
            hit.T = float.MaxValue;
            return IntersectNode(ref ray, _root, ref hit, 0.0001f, float.MaxValue);
        }


        /// <summary>
        /// Test if a ray hits anything within [0, maxDist].
        /// Used for shadow rays — returns as soon as any hit is found (early-out).
        /// </summary>
        public bool IntersectShadow(ref Ray ray, float maxDist)
        {
            return ShadowNode(ref ray, _root, 0.001f, maxDist);
        }


        // ── Build ──

        private BVHNode BuildRecursive(int[] indices, int start, int end, float[] centroids)
        {
            BVHNode node = new BVHNode();
            int count = end - start;

            //
            // Compute the AABB for all triangles in this range
            //
            node.Bounds = ComputeBounds(indices, start, end);

            //
            // Leaf node
            //
            if (count <= MAX_LEAF_SIZE)
            {
                node.TriStart = start;
                node.TriCount = count;
                node.Indices = indices;
                return node;
            }

            //
            // Find split axis (longest dimension of the bounding box)
            //
            int axis = node.Bounds.LongestAxis();

            //
            // Compute midpoint of centroids along the split axis
            //
            float mid = 0f;
            for (int i = start; i < end; i++)
            {
                mid += centroids[indices[i] * 3 + axis];
            }
            mid /= count;

            //
            // Partition indices around the midpoint
            //
            int splitIdx = Partition(indices, start, end, centroids, axis, mid);

            //
            // Fallback: if partition failed (all on one side), split in half
            //
            if (splitIdx == start || splitIdx == end)
            {
                splitIdx = start + count / 2;
            }

            //
            // Recurse
            //
            node.Left = BuildRecursive(indices, start, splitIdx, centroids);
            node.Right = BuildRecursive(indices, splitIdx, end, centroids);

            return node;
        }


        private int Partition(int[] indices, int start, int end, float[] centroids, int axis, float mid)
        {
            int left = start;
            int right = end - 1;

            while (left <= right)
            {
                if (centroids[indices[left] * 3 + axis] < mid)
                {
                    left++;
                }
                else
                {
                    // Swap
                    int tmp = indices[left];
                    indices[left] = indices[right];
                    indices[right] = tmp;
                    right--;
                }
            }

            return left;
        }


        private AABB ComputeBounds(int[] indices, int start, int end)
        {
            AABB bounds = AABB.Empty();

            for (int i = start; i < end; i++)
            {
                MeshTriangle t = _triangles[indices[i]];
                bounds.Include(t.X1, t.Y1, t.Z1);
                bounds.Include(t.X2, t.Y2, t.Z2);
                bounds.Include(t.X3, t.Y3, t.Z3);
            }

            return bounds;
        }


        // ── Traversal ──

        private bool IntersectNode(ref Ray ray, BVHNode node, ref HitRecord closest, float tMin, float tMax)
        {
            //
            // Test ray against this node's bounding box
            //
            if (!node.Bounds.Intersect(ref ray, tMin, tMax))
            {
                return false;
            }

            //
            // Leaf node: test all triangles
            //
            if (node.Left == null)
            {
                bool anyHit = false;

                for (int i = node.TriStart; i < node.TriStart + node.TriCount; i++)
                {
                    int triIdx = node.Indices[i];

                    if (IntersectTriangle(ref ray, ref _triangles[triIdx], tMin, closest.T, out float t, out float nx, out float ny, out float nz, out float hitU, out float hitV))
                    {
                        closest.T = t;
                        closest.NX = nx;
                        closest.NY = ny;
                        closest.NZ = nz;
                        closest.U = hitU;
                        closest.V = hitV;
                        closest.TriIndex = triIdx;
                        closest.R = _triangles[triIdx].R;
                        closest.G = _triangles[triIdx].G;
                        closest.B = _triangles[triIdx].B;
                        closest.A = _triangles[triIdx].A;
                        anyHit = true;
                    }
                }

                return anyHit;
            }

            //
            // Interior node: recurse into both children
            //
            bool hitLeft = IntersectNode(ref ray, node.Left, ref closest, tMin, closest.T);
            bool hitRight = IntersectNode(ref ray, node.Right, ref closest, tMin, closest.T);

            return hitLeft || hitRight;
        }


        private bool ShadowNode(ref Ray ray, BVHNode node, float tMin, float tMax)
        {
            if (!node.Bounds.Intersect(ref ray, tMin, tMax))
            {
                return false;
            }

            //
            // Leaf: test triangles — return true as soon as ANY hit is found
            //
            if (node.Left == null)
            {
                for (int i = node.TriStart; i < node.TriStart + node.TriCount; i++)
                {
                    int triIdx = node.Indices[i];

                    if (IntersectTriangle(ref ray, ref _triangles[triIdx], tMin, tMax, out _, out _, out _, out _, out _, out _))
                    {
                        return true;
                    }
                }

                return false;
            }

            //
            // Short-circuit: if left child hits, skip right
            //
            if (ShadowNode(ref ray, node.Left, tMin, tMax)) return true;
            if (ShadowNode(ref ray, node.Right, tMin, tMax)) return true;

            return false;
        }


        // ── Möller–Trumbore ray-triangle intersection ──

        /// <summary>
        /// Fast ray-triangle intersection using the Möller–Trumbore algorithm.
        /// Returns true if the ray hits the triangle within [tMin, tMax].
        /// Outputs: distance t, surface normal (nx, ny, nz).
        /// </summary>
        private static bool IntersectTriangle(ref Ray ray, ref MeshTriangle tri,
            float tMin, float tMax,
            out float t, out float nx, out float ny, out float nz,
            out float outU, out float outV)
        {
            t = 0; nx = 0; ny = 0; nz = 0;
            outU = 0; outV = 0;

            // Edge vectors
            float e1x = tri.X2 - tri.X1;
            float e1y = tri.Y2 - tri.Y1;
            float e1z = tri.Z2 - tri.Z1;

            float e2x = tri.X3 - tri.X1;
            float e2y = tri.Y3 - tri.Y1;
            float e2z = tri.Z3 - tri.Z1;

            // P = D × E2
            float px = ray.DirY * e2z - ray.DirZ * e2y;
            float py = ray.DirZ * e2x - ray.DirX * e2z;
            float pz = ray.DirX * e2y - ray.DirY * e2x;

            // Determinant
            float det = e1x * px + e1y * py + e1z * pz;

            //
            // No back-face culling in the ray tracer — we test both sides.
            // If determinant is near zero, the ray is parallel to the triangle.
            //
            if (det > -1e-7f && det < 1e-7f)
            {
                return false;
            }

            float invDet = 1f / det;

            // T vector (ray origin to vertex 0)
            float tvx = ray.OriginX - tri.X1;
            float tvy = ray.OriginY - tri.Y1;
            float tvz = ray.OriginZ - tri.Z1;

            // U parameter
            float u = (tvx * px + tvy * py + tvz * pz) * invDet;
            if (u < 0f || u > 1f) return false;

            // Q = T × E1
            float qx = tvy * e1z - tvz * e1y;
            float qy = tvz * e1x - tvx * e1z;
            float qz = tvx * e1y - tvy * e1x;

            // V parameter
            float v = (ray.DirX * qx + ray.DirY * qy + ray.DirZ * qz) * invDet;
            if (v < 0f || u + v > 1f) return false;

            // Distance
            t = (e2x * qx + e2y * qy + e2z * qz) * invDet;

            if (t < tMin || t > tMax) return false;

            outU = u;
            outV = v;

            //
            // Compute face normal (cross product of edges)
            //
            nx = e1y * e2z - e1z * e2y;
            ny = e1z * e2x - e1x * e2z;
            nz = e1x * e2y - e1y * e2x;

            float nlen = MathF.Sqrt(nx * nx + ny * ny + nz * nz);
            if (nlen > 1e-8f)
            {
                nx /= nlen;
                ny /= nlen;
                nz /= nlen;
            }

            //
            // Ensure normal faces toward the ray (flip if back-facing)
            //
            float ndotd = nx * ray.DirX + ny * ray.DirY + nz * ray.DirZ;
            if (ndotd > 0)
            {
                nx = -nx;
                ny = -ny;
                nz = -nz;
            }

            return true;
        }


        // ── BVH node ──

        private class BVHNode
        {
            public AABB Bounds;
            public BVHNode Left;
            public BVHNode Right;

            // Leaf data (only set when Left == null)
            public int TriStart;
            public int TriCount;
            public int[] Indices;  // shared reference to the index array
        }
    }
}
