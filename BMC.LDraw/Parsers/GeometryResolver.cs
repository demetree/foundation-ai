using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BMC.LDraw.Models;

namespace BMC.LDraw.Parsers
{
    /// <summary>
    /// Recursively resolves LDraw geometry into a flat triangle mesh.
    /// Walks subfile references, accumulates transforms, resolves colour inheritance,
    /// handles BFC winding, and produces a render-ready LDrawMesh.
    /// </summary>
    public class GeometryResolver
    {
        private readonly string _libraryPath;
        private readonly Dictionary<string, LDrawGeometry> _cache = new Dictionary<string, LDrawGeometry>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, LDrawColour> _colourTable;

        // Default colour when no colour is resolved (medium grey)
        private const byte DefaultR = 170, DefaultG = 170, DefaultB = 170, DefaultA = 255;

        /// <summary>
        /// Create a resolver with the given LDraw library path and colour table.
        /// </summary>
        /// <param name="libraryPath">Root of the LDraw library (the "ldraw" directory containing "parts", "p", etc.)</param>
        /// <param name="colours">Parsed colour table from LDConfig.ldr. Null to use a default grey for all colours.</param>
        public GeometryResolver(string libraryPath, List<LDrawColour> colours = null)
        {
            _libraryPath = libraryPath;
            _colourTable = new Dictionary<int, LDrawColour>();
            if (colours != null)
            {
                foreach (LDrawColour c in colours)
                {
                    _colourTable[c.Code] = c;
                }
            }
        }

        /// <summary>
        /// Resolve a parsed geometry into a flat triangle mesh.
        /// </summary>
        /// <param name="geometry">Parsed geometry (from GeometryParser).</param>
        /// <param name="parentColourCode">Initial colour code (16 = use part default). Use -1 for no override.</param>
        public LDrawMesh Resolve(LDrawGeometry geometry, int parentColourCode = -1)
        {
            LDrawMesh mesh = new LDrawMesh();

            // Identity 4x4 matrix
            float[] identity = new float[]
            {
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            };

            ResolveRecursive(geometry, identity, parentColourCode, 24, false, mesh, 0);
            mesh.ComputeBounds();
            return mesh;
        }

        /// <summary>
        /// Resolve a file path directly into a mesh.
        /// </summary>
        public LDrawMesh ResolveFile(string filePath, int parentColourCode = -1)
        {
            List<LDrawGeometry> geos = GeometryParser.ParseFile(filePath);
            if (geos.Count == 0) return new LDrawMesh();

            // Cache all MPD submodels
            for (int i = 1; i < geos.Count; i++)
            {
                if (geos[i].Name != null)
                {
                    _cache[geos[i].Name] = geos[i];
                }
            }

            return Resolve(geos[0], parentColourCode);
        }


        /// <summary>
        /// Resolve a file and track how many triangles/edges each top-level subfile reference produces.
        /// Used by ExplodedViewBuilder to identify part boundaries in the mesh.
        /// </summary>
        public LDrawMesh ResolveFileWithPartCounts(string filePath, int parentColourCode,
            out List<int> partTriangleCounts, out List<int> partEdgeCounts)
        {
            List<LDrawGeometry> geos = GeometryParser.ParseFile(filePath);
            partTriangleCounts = new List<int>();
            partEdgeCounts = new List<int>();

            if (geos.Count == 0) return new LDrawMesh();

            // Cache all MPD submodels
            for (int i = 1; i < geos.Count; i++)
            {
                if (geos[i].Name != null)
                {
                    _cache[geos[i].Name] = geos[i];
                }
            }

            LDrawGeometry root = geos[0];
            LDrawMesh mesh = new LDrawMesh();
            int colour = parentColourCode >= 0 ? parentColourCode : 16;

            float[] identity = new float[]
            {
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            };

            // Resolve direct geometry first (root-level tris/quads/lines; these are not "parts")
            ResolveDirectGeometry(root, identity, colour, 0, false, mesh);

            // Resolve each top-level subfile reference and track counts
            for (int i = 0; i < root.SubfileReferences.Count; i++)
            {
                int triBefore = mesh.Triangles.Count;
                int edgeBefore = mesh.EdgeLines.Count;

                LDrawSubfileReference subRef = root.SubfileReferences[i];
                LDrawGeometry subGeo = LoadFile(subRef.FileName);
                if (subGeo == null)
                {
                    partTriangleCounts.Add(0);
                    partEdgeCounts.Add(0);
                    continue;
                }

                float[] subMatrix = BuildMatrix(subRef);
                float[] worldMatrix = MultiplyMatrices(identity, subMatrix);
                int subColour = ResolveColour(subRef.ColourCode, colour);

                int subEdgeColour = 0;
                if (subRef.ColourCode != 16 && subRef.ColourCode != 24)
                {
                    subEdgeColour = subRef.ColourCode;
                }

                bool subInvert = false;
                if (root.InvertNextIndices.Contains(i))
                {
                    subInvert = !subInvert;
                }
                if (Determinant3x3(subRef.Matrix) < 0)
                {
                    subInvert = !subInvert;
                }

                ResolveRecursive(subGeo, worldMatrix, subColour, subEdgeColour, subInvert, mesh, 1);

                partTriangleCounts.Add(mesh.Triangles.Count - triBefore);
                partEdgeCounts.Add(mesh.EdgeLines.Count - edgeBefore);
            }

            return mesh;
        }


        /// <summary>
        /// Get the number of build steps in a file.
        /// Returns 0 if the file has no STEP meta-commands (or 1 implicit step).
        /// </summary>
        public int GetStepCount(string filePath)
        {
            List<LDrawGeometry> geos = GeometryParser.ParseFile(filePath);
            if (geos.Count == 0) return 0;
            return geos[0].StepBreaks.Count;
        }


        /// <summary>
        /// Resolve a file up to (and including) a specific build step.
        /// Step indices are 0-based.  Step 0 = just the first group of parts,
        /// Step N-1 = the complete model.
        /// </summary>
        public LDrawMesh ResolveFileUpToStep(string filePath, int stepIndex, int parentColourCode = -1)
        {
            List<LDrawGeometry> geos = GeometryParser.ParseFile(filePath);
            if (geos.Count == 0) return new LDrawMesh();

            // Cache all MPD submodels
            for (int i = 1; i < geos.Count; i++)
            {
                if (geos[i].Name != null)
                {
                    _cache[geos[i].Name] = geos[i];
                }
            }

            LDrawGeometry root = geos[0];

            //
            // If no steps or step is beyond range, resolve the whole file
            //
            if (root.StepBreaks.Count == 0 || stepIndex >= root.StepBreaks.Count)
            {
                return Resolve(root, parentColourCode);
            }

            //
            // Limit subfile refs to those up to the step boundary
            //
            int maxSubRef = root.StepBreaks[stepIndex];

            LDrawMesh mesh = new LDrawMesh();
            int colour = parentColourCode >= 0 ? parentColourCode : 16;

            float[] identity = new float[]
            {
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            };

            //
            // Resolve direct geometry (triangles, quads, lines) from the root
            //
            ResolveDirectGeometry(root, identity, colour, 0, false, mesh);

            //
            // Resolve only the subfile references up to the step boundary
            //
            ResolveSubfilesUpTo(root, identity, colour, 0, false, mesh, maxSubRef, 0);

            return mesh;
        }

        private void ResolveRecursive(LDrawGeometry geo, float[] parentMatrix,
            int parentColour, int parentEdgeColour, bool invertWinding,
            LDrawMesh mesh, int depth)
        {
            // Guard against infinite recursion (LDraw files shouldn't nest >100 deep)
            if (depth > 100) return;

            // Resolve direct triangles
            for (int i = 0; i < geo.Triangles.Count; i++)
            {
                LDrawTriangle tri = geo.Triangles[i];
                int col = ResolveColour(tri.ColourCode, parentColour);

                float x1, y1, z1, x2, y2, z2, x3, y3, z3;
                TransformPoint(tri.X1, tri.Y1, tri.Z1, parentMatrix, out x1, out y1, out z1);
                TransformPoint(tri.X2, tri.Y2, tri.Z2, parentMatrix, out x2, out y2, out z2);
                TransformPoint(tri.X3, tri.Y3, tri.Z3, parentMatrix, out x3, out y3, out z3);

                ColourToRgba(col, out byte r, out byte g, out byte b, out byte a);

                MeshTriangle mt;
                if (invertWinding)
                {
                    // Swap v2 and v3 to flip winding
                    mt = MakeTriangle(x1, y1, z1, x3, y3, z3, x2, y2, z2, r, g, b, a);
                }
                else
                {
                    mt = MakeTriangle(x1, y1, z1, x2, y2, z2, x3, y3, z3, r, g, b, a);
                }
                mesh.Triangles.Add(mt);
            }

            // Resolve quads → 2 triangles each
            for (int i = 0; i < geo.Quads.Count; i++)
            {
                LDrawQuad q = geo.Quads[i];
                int col = ResolveColour(q.ColourCode, parentColour);

                float x1, y1, z1, x2, y2, z2, x3, y3, z3, x4, y4, z4;
                TransformPoint(q.X1, q.Y1, q.Z1, parentMatrix, out x1, out y1, out z1);
                TransformPoint(q.X2, q.Y2, q.Z2, parentMatrix, out x2, out y2, out z2);
                TransformPoint(q.X3, q.Y3, q.Z3, parentMatrix, out x3, out y3, out z3);
                TransformPoint(q.X4, q.Y4, q.Z4, parentMatrix, out x4, out y4, out z4);

                ColourToRgba(col, out byte r, out byte g, out byte b, out byte a);

                if (invertWinding)
                {
                    mesh.Triangles.Add(MakeTriangle(x1, y1, z1, x3, y3, z3, x2, y2, z2, r, g, b, a));
                    mesh.Triangles.Add(MakeTriangle(x1, y1, z1, x4, y4, z4, x3, y3, z3, r, g, b, a));
                }
                else
                {
                    mesh.Triangles.Add(MakeTriangle(x1, y1, z1, x2, y2, z2, x3, y3, z3, r, g, b, a));
                    mesh.Triangles.Add(MakeTriangle(x1, y1, z1, x3, y3, z3, x4, y4, z4, r, g, b, a));
                }
            }

            // Resolve edge lines
            for (int i = 0; i < geo.Lines.Count; i++)
            {
                LDrawLine ln = geo.Lines[i];
                int col = ln.ColourCode == 24 ? parentEdgeColour : ResolveColour(ln.ColourCode, parentColour);

                float x1, y1, z1, x2, y2, z2;
                TransformPoint(ln.X1, ln.Y1, ln.Z1, parentMatrix, out x1, out y1, out z1);
                TransformPoint(ln.X2, ln.Y2, ln.Z2, parentMatrix, out x2, out y2, out z2);

                ColourToRgba(col, out byte r, out byte g, out byte b, out byte a);

                MeshLine ml;
                ml.X1 = x1; ml.Y1 = y1; ml.Z1 = z1;
                ml.X2 = x2; ml.Y2 = y2; ml.Z2 = z2;
                ml.R = r; ml.G = g; ml.B = b; ml.A = a;
                mesh.EdgeLines.Add(ml);
            }

            // Recurse into subfile references
            for (int i = 0; i < geo.SubfileReferences.Count; i++)
            {
                LDrawSubfileReference subRef = geo.SubfileReferences[i];
                LDrawGeometry subGeo = LoadFile(subRef.FileName);
                if (subGeo == null) continue;

                // Build 4x4 transform from the subfile reference
                float[] subMatrix = BuildMatrix(subRef);

                // Multiply parent * sub to get the world matrix for this subfile
                float[] worldMatrix = MultiplyMatrices(parentMatrix, subMatrix);

                // Resolve colour inheritance
                int subColour = ResolveColour(subRef.ColourCode, parentColour);

                // Determine edge colour for children
                int subEdgeColour = parentEdgeColour;
                if (subRef.ColourCode != 16 && subRef.ColourCode != 24)
                {
                    // Use the complement/edge colour from the colour table
                    subEdgeColour = subRef.ColourCode;
                }

                // Determine winding inversion:
                // Invert if INVERTNEXT is set, or if the transform has a negative determinant
                bool subInvert = invertWinding;
                if (geo.InvertNextIndices.Contains(i))
                {
                    subInvert = !subInvert;
                }
                if (Determinant3x3(subRef.Matrix) < 0)
                {
                    subInvert = !subInvert;
                }

                ResolveRecursive(subGeo, worldMatrix, subColour, subEdgeColour, subInvert, mesh, depth + 1);
            }
        }


        /// <summary>
        /// Resolve only the direct geometry (triangles, quads, lines) from a geometry node,
        /// without recursing into subfile references.
        /// </summary>
        private void ResolveDirectGeometry(LDrawGeometry geo, float[] parentMatrix,
            int parentColour, int parentEdgeColour, bool invertWinding, LDrawMesh mesh)
        {
            // Triangles
            for (int i = 0; i < geo.Triangles.Count; i++)
            {
                LDrawTriangle tri = geo.Triangles[i];
                int col = ResolveColour(tri.ColourCode, parentColour);

                float x1, y1, z1, x2, y2, z2, x3, y3, z3;
                TransformPoint(tri.X1, tri.Y1, tri.Z1, parentMatrix, out x1, out y1, out z1);
                TransformPoint(tri.X2, tri.Y2, tri.Z2, parentMatrix, out x2, out y2, out z2);
                TransformPoint(tri.X3, tri.Y3, tri.Z3, parentMatrix, out x3, out y3, out z3);

                ColourToRgba(col, out byte r, out byte g, out byte b, out byte a);

                MeshTriangle mt;
                if (invertWinding)
                {
                    mt = MakeTriangle(x1, y1, z1, x3, y3, z3, x2, y2, z2, r, g, b, a);
                }
                else
                {
                    mt = MakeTriangle(x1, y1, z1, x2, y2, z2, x3, y3, z3, r, g, b, a);
                }
                mesh.Triangles.Add(mt);
            }

            // Quads → 2 triangles each
            for (int i = 0; i < geo.Quads.Count; i++)
            {
                LDrawQuad q = geo.Quads[i];
                int col = ResolveColour(q.ColourCode, parentColour);

                float x1, y1, z1, x2, y2, z2, x3, y3, z3, x4, y4, z4;
                TransformPoint(q.X1, q.Y1, q.Z1, parentMatrix, out x1, out y1, out z1);
                TransformPoint(q.X2, q.Y2, q.Z2, parentMatrix, out x2, out y2, out z2);
                TransformPoint(q.X3, q.Y3, q.Z3, parentMatrix, out x3, out y3, out z3);
                TransformPoint(q.X4, q.Y4, q.Z4, parentMatrix, out x4, out y4, out z4);

                ColourToRgba(col, out byte r, out byte g, out byte b, out byte a);

                if (invertWinding)
                {
                    mesh.Triangles.Add(MakeTriangle(x1, y1, z1, x3, y3, z3, x2, y2, z2, r, g, b, a));
                    mesh.Triangles.Add(MakeTriangle(x1, y1, z1, x4, y4, z4, x3, y3, z3, r, g, b, a));
                }
                else
                {
                    mesh.Triangles.Add(MakeTriangle(x1, y1, z1, x2, y2, z2, x3, y3, z3, r, g, b, a));
                    mesh.Triangles.Add(MakeTriangle(x1, y1, z1, x3, y3, z3, x4, y4, z4, r, g, b, a));
                }
            }

            // Edge lines
            for (int i = 0; i < geo.Lines.Count; i++)
            {
                LDrawLine ln = geo.Lines[i];
                int col = ln.ColourCode == 24 ? parentEdgeColour : ResolveColour(ln.ColourCode, parentColour);

                float x1, y1, z1, x2, y2, z2;
                TransformPoint(ln.X1, ln.Y1, ln.Z1, parentMatrix, out x1, out y1, out z1);
                TransformPoint(ln.X2, ln.Y2, ln.Z2, parentMatrix, out x2, out y2, out z2);

                ColourToRgba(col, out byte r, out byte g, out byte b, out byte a);

                MeshLine ml;
                ml.X1 = x1; ml.Y1 = y1; ml.Z1 = z1;
                ml.X2 = x2; ml.Y2 = y2; ml.Z2 = z2;
                ml.R = r; ml.G = g; ml.B = b; ml.A = a;
                mesh.EdgeLines.Add(ml);
            }
        }


        /// <summary>
        /// Resolve subfile references up to (but not including) the given index.
        /// Each subfile reference is fully recursed into (no step limiting inside children).
        /// </summary>
        private void ResolveSubfilesUpTo(LDrawGeometry geo, float[] parentMatrix,
            int parentColour, int parentEdgeColour, bool invertWinding,
            LDrawMesh mesh, int maxSubRefIndex, int depth)
        {
            if (depth > 100) return;

            int limit = Math.Min(maxSubRefIndex, geo.SubfileReferences.Count);

            for (int i = 0; i < limit; i++)
            {
                LDrawSubfileReference subRef = geo.SubfileReferences[i];
                LDrawGeometry subGeo = LoadFile(subRef.FileName);
                if (subGeo == null) continue;

                float[] subMatrix = BuildMatrix(subRef);
                float[] worldMatrix = MultiplyMatrices(parentMatrix, subMatrix);
                int subColour = ResolveColour(subRef.ColourCode, parentColour);

                int subEdgeColour = parentEdgeColour;
                if (subRef.ColourCode != 16 && subRef.ColourCode != 24)
                {
                    subEdgeColour = subRef.ColourCode;
                }

                bool subInvert = invertWinding;
                if (geo.InvertNextIndices.Contains(i))
                {
                    subInvert = !subInvert;
                }
                if (Determinant3x3(subRef.Matrix) < 0)
                {
                    subInvert = !subInvert;
                }

                // Full recursive resolve for each included subfile
                ResolveRecursive(subGeo, worldMatrix, subColour, subEdgeColour, subInvert, mesh, depth + 1);
            }
        }

        /// <summary>
        /// Load and parse a part file, with caching.
        /// Searches: cache → MPD submodels → parts/ → p/ → models/ directories.
        /// </summary>
        private LDrawGeometry LoadFile(string fileName)
        {
            if (_cache.TryGetValue(fileName, out LDrawGeometry cached))
            {
                return cached;
            }

            // Normalize path separators for cross-platform
            string normalizedName = fileName.Replace('\\', Path.DirectorySeparatorChar);

            // Search paths in LDraw library
            string[] searchDirs = new string[]
            {
                Path.Combine(_libraryPath, "parts"),
                Path.Combine(_libraryPath, "p"),
                Path.Combine(_libraryPath, "models"),
                Path.Combine(_libraryPath, "parts", "s"),  // subparts
                Path.Combine(_libraryPath, "p", "48"),      // hi-res primitives
                Path.Combine(_libraryPath, "p", "8"),       // lo-res primitives
                _libraryPath, // root
            };

            foreach (string dir in searchDirs)
            {
                string fullPath = Path.Combine(dir, normalizedName);
                if (File.Exists(fullPath))
                {
                    List<LDrawGeometry> geos = GeometryParser.ParseFile(fullPath);
                    if (geos.Count > 0)
                    {
                        _cache[fileName] = geos[0];

                        // Also cache any MPD sub-models
                        for (int i = 1; i < geos.Count; i++)
                        {
                            if (geos[i].Name != null)
                            {
                                _cache[geos[i].Name] = geos[i];
                            }
                        }

                        return geos[0];
                    }
                }
            }

            // File not found — skip silently (common for unofficial parts)
            _cache[fileName] = null;
            return null;
        }

        // ────────────────────────────────────────────────────────
        // Transform helpers
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Build a 4x4 matrix from a subfile reference (position + 3x3 rotation).
        /// </summary>
        private static float[] BuildMatrix(LDrawSubfileReference subRef)
        {
            float[] m = subRef.Matrix;
            return new float[]
            {
                m[0], m[1], m[2], subRef.X,
                m[3], m[4], m[5], subRef.Y,
                m[6], m[7], m[8], subRef.Z,
                0,    0,    0,    1
            };
        }

        /// <summary>
        /// Multiply two 4x4 matrices (row-major).
        /// </summary>
        private static float[] MultiplyMatrices(float[] a, float[] b)
        {
            float[] r = new float[16];
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    float sum = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        sum += a[row * 4 + k] * b[k * 4 + col];
                    }
                    r[row * 4 + col] = sum;
                }
            }
            return r;
        }

        /// <summary>
        /// Transform a point by a 4x4 matrix.
        /// </summary>
        private static void TransformPoint(float x, float y, float z, float[] m,
            out float ox, out float oy, out float oz)
        {
            ox = m[0] * x + m[1] * y + m[2] * z + m[3];
            oy = m[4] * x + m[5] * y + m[6] * z + m[7];
            oz = m[8] * x + m[9] * y + m[10] * z + m[11];
        }

        /// <summary>
        /// Compute the determinant of a 3x3 matrix (row-major, 9 elements).
        /// Used to detect mirroring transforms that flip winding order.
        /// </summary>
        private static float Determinant3x3(float[] m)
        {
            return m[0] * (m[4] * m[8] - m[5] * m[7])
                 - m[1] * (m[3] * m[8] - m[5] * m[6])
                 + m[2] * (m[3] * m[7] - m[4] * m[6]);
        }

        // ────────────────────────────────────────────────────────
        // Colour helpers
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Resolve a colour code. 16 = inherit from parent, 24 = edge inherit.
        /// </summary>
        private static int ResolveColour(int colourCode, int parentColour)
        {
            if (colourCode == 16) return parentColour;
            if (colourCode == 24) return parentColour; // edge colour simplified
            return colourCode;
        }

        /// <summary>
        /// Convert an LDraw colour code to RGBA bytes.
        /// </summary>
        private void ColourToRgba(int colourCode, out byte r, out byte g, out byte b, out byte a)
        {
            if (_colourTable.TryGetValue(colourCode, out LDrawColour colour) && colour.HexValue != null)
            {
                ParseHexColour(colour.HexValue, out r, out g, out b);
                a = (byte)colour.Alpha;
            }
            else
            {
                // Direct/unknown colour — use default
                r = DefaultR; g = DefaultG; b = DefaultB; a = DefaultA;
            }
        }

        /// <summary>
        /// Parse a hex colour string like "#DFC176" into RGB bytes.
        /// </summary>
        private static void ParseHexColour(string hex, out byte r, out byte g, out byte b)
        {
            r = DefaultR; g = DefaultG; b = DefaultB;
            if (hex == null || hex.Length < 7 || hex[0] != '#') return;

            if (byte.TryParse(hex.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte rr)) r = rr;
            if (byte.TryParse(hex.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte gg)) g = gg;
            if (byte.TryParse(hex.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte bb)) b = bb;
        }

        // ────────────────────────────────────────────────────────
        // Triangle helpers
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Create a MeshTriangle with computed flat normal.
        /// </summary>
        private static MeshTriangle MakeTriangle(
            float x1, float y1, float z1,
            float x2, float y2, float z2,
            float x3, float y3, float z3,
            byte r, byte g, byte b, byte a)
        {
            MeshTriangle t;
            t.X1 = x1; t.Y1 = y1; t.Z1 = z1;
            t.X2 = x2; t.Y2 = y2; t.Z2 = z2;
            t.X3 = x3; t.Y3 = y3; t.Z3 = z3;
            t.R = r; t.G = g; t.B = b; t.A = a;

            // Compute flat normal from cross product of edges
            float e1x = x2 - x1, e1y = y2 - y1, e1z = z2 - z1;
            float e2x = x3 - x1, e2y = y3 - y1, e2z = z3 - z1;
            float nx = e1y * e2z - e1z * e2y;
            float ny = e1z * e2x - e1x * e2z;
            float nz = e1x * e2y - e1y * e2x;

            // Normalize
            float len = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
            if (len > 1e-8f)
            {
                t.NX = nx / len;
                t.NY = ny / len;
                t.NZ = nz / len;
            }
            else
            {
                t.NX = 0; t.NY = 1; t.NZ = 0; // degenerate — default up
            }

            // Per-vertex normals default to zero — they are populated later
            // by NormalSmoother if smooth shading is enabled.
            t.NX1 = 0; t.NY1 = 0; t.NZ1 = 0;
            t.NX2 = 0; t.NY2 = 0; t.NZ2 = 0;
            t.NX3 = 0; t.NY3 = 0; t.NZ3 = 0;
            t.HasPerVertexNormals = false;

            return t;
        }
    }
}
