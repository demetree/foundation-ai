using System;
using System.Collections.Generic;
using System.Numerics;
using BMC.LDraw.Models;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Exports an LDraw mesh as a binary glTF (GLB) file.
    ///
    /// Converts the flat triangle list from GeometryResolver into an optimised GLB:
    ///   - Triangles grouped by RGBA colour → one glTF material per unique colour
    ///   - PBR metallic-roughness tuned per LDraw MaterialFinish
    ///   - Smooth per-vertex normals included when available
    ///   - Build steps encoded as separate glTF scene nodes (step0, step1, ...)
    ///   - Edge lines optionally included as LINES-mode mesh primitives
    ///   - Y-axis negated during export (LDraw Y-down → glTF Y-up)
    ///
    /// Usage:
    ///   byte[] glb = GlbExporter.Export(mesh, stepTriangleBounds, stepEdgeBounds, includeEdgeLines: true);
    ///
    /// AI-generated — Mar 2026.
    /// </summary>
    public static class GlbExporter
    {
        //
        // Material key — groups triangles by colour + finish for efficient draw calls
        //
        private struct MaterialKey : IEquatable<MaterialKey>
        {
            public byte R, G, B, A;
            public MaterialFinish Finish;

            public bool Equals(MaterialKey other)
            {
                return R == other.R && G == other.G && B == other.B && A == other.A && Finish == other.Finish;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(R, G, B, A, Finish);
            }

            public override bool Equals(object obj) => obj is MaterialKey k && Equals(k);
        }


        /// <summary>
        /// Export a resolved LDraw mesh to GLB binary, with build step support.
        ///
        /// Each build step becomes a separate named node in the glTF scene.
        /// Within each step, triangles are grouped by material (colour + finish)
        /// for minimal draw calls.
        /// </summary>
        /// <param name="mesh">Pre-resolved LDraw mesh with world-space triangles.</param>
        /// <param name="stepTriangleBounds">
        /// Cumulative triangle count after each step (from GeometryResolver).
        /// stepTriangleBounds[i] = total triangles through step i.
        /// If null or empty, all geometry is placed in a single node.
        /// </param>
        /// <param name="stepEdgeBounds">
        /// Cumulative edge line count after each step.
        /// </param>
        /// <param name="includeEdgeLines">Whether to include edge lines as separate LINES-mode meshes.</param>
        /// <returns>Complete GLB file as a byte array.</returns>
        public static byte[] Export(LDrawMesh mesh,
                                    int[] stepTriangleBounds = null,
                                    int[] stepEdgeBounds = null,
                                    bool includeEdgeLines = true)
        {
            if (mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh));
            }

            if (mesh.Triangles.Count == 0)
            {
                return Array.Empty<byte>();
            }

            SceneBuilder scene = new SceneBuilder("BMC_Model");

            //
            // If no step data, treat the whole model as one step
            //
            if (stepTriangleBounds == null || stepTriangleBounds.Length == 0)
            {
                stepTriangleBounds = new int[] { mesh.Triangles.Count };
                stepEdgeBounds = new int[] { mesh.EdgeLines.Count };
            }

            //
            // Build each step as a separate node
            //
            int prevTriEnd = 0;
            int prevEdgeEnd = 0;

            for (int step = 0; step < stepTriangleBounds.Length; step++)
            {
                int triEnd = stepTriangleBounds[step];
                int edgeEnd = (stepEdgeBounds != null && step < stepEdgeBounds.Length)
                    ? stepEdgeBounds[step]
                    : mesh.EdgeLines.Count;

                NodeBuilder stepNode = new NodeBuilder($"step_{step}");

                //
                // Store step index in node extras for client-side discovery.
                // SharpGLTF accepts System.Text.Json.Nodes.JsonNode for extras.
                //
                var extrasObj = new System.Text.Json.Nodes.JsonObject
                {
                    ["stepIndex"] = step,
                    ["totalSteps"] = stepTriangleBounds.Length
                };
                stepNode.Extras = extrasObj;

                //
                // Build triangle mesh for this step
                //
                var stepMesh = BuildStepTriangleMesh(mesh, prevTriEnd, triEnd, $"step_{step}_mesh");

                if (stepMesh != null)
                {
                    scene.AddRigidMesh(stepMesh, stepNode);
                }

                //
                // Build edge lines for this step (optional)
                //
                if (includeEdgeLines && prevEdgeEnd < edgeEnd)
                {
                    var edgeMesh = BuildStepEdgeMesh(mesh, prevEdgeEnd, edgeEnd, $"step_{step}_edges");

                    if (edgeMesh != null)
                    {
                        scene.AddRigidMesh(edgeMesh, stepNode);
                    }
                }

                prevTriEnd = triEnd;
                prevEdgeEnd = edgeEnd;
            }

            //
            // Build the glTF model and write as GLB
            //
            var model = scene.ToGltf2();

            return model.WriteGLB().ToArray();
        }


        /// <summary>
        /// Export a full mesh (no steps) to GLB binary.
        /// Convenience overload for single-part rendering.
        /// </summary>
        public static byte[] Export(LDrawMesh mesh, bool includeEdgeLines = true)
        {
            return Export(mesh, null, null, includeEdgeLines);
        }


        /// <summary>
        /// Build a glTF mesh from a range of triangles in the LDrawMesh,
        /// grouped by material (colour + finish) for efficient rendering.
        /// </summary>
        private static IMeshBuilder<MaterialBuilder> BuildStepTriangleMesh(
            LDrawMesh mesh, int fromTri, int toTri, string meshName)
        {
            if (fromTri >= toTri)
            {
                return null;
            }

            //
            // Group triangles by material key — one primitive per unique colour
            //
            Dictionary<MaterialKey, MaterialBuilder> materialCache = new Dictionary<MaterialKey, MaterialBuilder>();
            var meshBuilder = new MeshBuilder<VertexPositionNormal>(meshName);

            for (int i = fromTri; i < toTri && i < mesh.Triangles.Count; i++)
            {
                MeshTriangle tri = mesh.Triangles[i];

                MaterialKey key;
                key.R = tri.R;
                key.G = tri.G;
                key.B = tri.B;
                key.A = tri.A;
                key.Finish = tri.Finish;

                if (materialCache.TryGetValue(key, out MaterialBuilder material) == false)
                {
                    material = CreateMaterial(key);
                    materialCache[key] = material;
                }

                var primitive = meshBuilder.UsePrimitive(material);

                //
                // Determine normals — use smooth per-vertex normals when available
                //
                Vector3 n1, n2, n3;

                if (tri.HasPerVertexNormals)
                {
                    n1 = FlipY(new Vector3(tri.NX1, tri.NY1, tri.NZ1));
                    n2 = FlipY(new Vector3(tri.NX2, tri.NY2, tri.NZ2));
                    n3 = FlipY(new Vector3(tri.NX3, tri.NY3, tri.NZ3));
                }
                else
                {
                    Vector3 flatNormal = FlipY(new Vector3(tri.NX, tri.NY, tri.NZ));
                    n1 = n2 = n3 = flatNormal;
                }

                //
                // Vertex positions — negate Y to convert LDraw Y-down → glTF Y-up
                //
                var v1 = new VertexPositionNormal(FlipY(new Vector3(tri.X1, tri.Y1, tri.Z1)), n1);
                var v2 = new VertexPositionNormal(FlipY(new Vector3(tri.X2, tri.Y2, tri.Z2)), n2);
                var v3 = new VertexPositionNormal(FlipY(new Vector3(tri.X3, tri.Y3, tri.Z3)), n3);

                primitive.AddTriangle(v1, v2, v3);
            }

            return meshBuilder;
        }


        /// <summary>
        /// Build a glTF mesh from edge lines (LINES mode) for the outlined LEGO look.
        /// </summary>
        private static IMeshBuilder<MaterialBuilder> BuildStepEdgeMesh(
            LDrawMesh mesh, int fromEdge, int toEdge, string meshName)
        {
            if (fromEdge >= toEdge)
            {
                return null;
            }

            //
            // Edge lines use a single unlit dark material
            //
            var edgeMaterial = new MaterialBuilder("edges")
                .WithUnlitShader()
                .WithBaseColor(new Vector4(0.15f, 0.15f, 0.15f, 1.0f));

            var meshBuilder = new MeshBuilder<VertexPosition>(meshName);
            var primitive = meshBuilder.UsePrimitive(edgeMaterial, 2); // mode 2 = LINES

            for (int i = fromEdge; i < toEdge && i < mesh.EdgeLines.Count; i++)
            {
                MeshLine line = mesh.EdgeLines[i];

                var v1 = new VertexPosition(FlipY(new Vector3(line.X1, line.Y1, line.Z1)));
                var v2 = new VertexPosition(FlipY(new Vector3(line.X2, line.Y2, line.Z2)));

                primitive.AddLine(v1, v2);
            }

            return meshBuilder;
        }


        /// <summary>
        /// Create a PBR material tuned for the given LDraw colour and finish.
        /// </summary>
        private static MaterialBuilder CreateMaterial(MaterialKey key)
        {
            float r = key.R / 255f;
            float g = key.G / 255f;
            float b = key.B / 255f;
            float a = key.A / 255f;

            string colourHex = $"#{key.R:X2}{key.G:X2}{key.B:X2}";
            string name = $"colour_{colourHex}_{key.Finish}";

            var material = new MaterialBuilder(name)
                .WithMetallicRoughnessShader()
                .WithBaseColor(new Vector4(r, g, b, a));

            //
            // Tune PBR properties based on LDraw material finish
            //
            switch (key.Finish)
            {
                case MaterialFinish.Chrome:
                    material.WithMetallicRoughness(1.0f, 0.05f);
                    break;

                case MaterialFinish.Metal:
                    material.WithMetallicRoughness(0.8f, 0.2f);
                    break;

                case MaterialFinish.Pearlescent:
                    material.WithMetallicRoughness(0.3f, 0.3f);
                    break;

                case MaterialFinish.Rubber:
                    material.WithMetallicRoughness(0.0f, 0.9f);
                    break;

                case MaterialFinish.Transparent:
                case MaterialFinish.Milky:
                case MaterialFinish.Glitter:
                    material.WithMetallicRoughness(0.0f, 0.1f);
                    material.WithAlpha(SharpGLTF.Materials.AlphaMode.BLEND);
                    break;

                default: // Solid, Speckle
                    material.WithMetallicRoughness(0.0f, 0.4f);
                    break;
            }

            //
            // Enable alpha blending for transparent colours (regardless of finish)
            //
            if (key.A < 255)
            {
                material.WithAlpha(SharpGLTF.Materials.AlphaMode.BLEND);
            }

            //
            // Double-sided rendering — LEGO bricks can be viewed from any angle
            //
            material.WithDoubleSide(true);

            return material;
        }


        /// <summary>
        /// Negate Y axis to convert from LDraw coordinate system (Y-down)
        /// to glTF coordinate system (Y-up).
        /// </summary>
        private static Vector3 FlipY(Vector3 v)
        {
            return new Vector3(v.X, -v.Y, v.Z);
        }
    }
}
