using System;
using System.Threading.Tasks;
using BMC.LDraw.Models;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// CPU-based triangle rasterizer.  Renders an LDrawMesh to an RGBA pixel buffer
    /// using scanline triangle filling with Z-buffer depth testing, flat or smooth
    /// (Gouraud) shading, configurable multi-light Blinn-Phong lighting, and optional
    /// edge / outline rendering.
    ///
    /// AI-generated — Phase 1.1 edges, Phase 1.2 smooth shading, Phase 1.3 enhanced
    /// lighting added Feb 2026.
    /// </summary>
    public class SoftwareRenderer
    {
        private readonly int _width;
        private readonly int _height;

        //
        // Framebuffer
        //
        private byte[] _colourBuffer;
        private float[] _depthBuffer;

        //
        // Background colour (transparent by default)
        //
        private byte _bgR = 0, _bgG = 0, _bgB = 0, _bgA = 0;

        //
        // Gradient background (when _hasGradient is true, overrides solid bg)
        //
        private bool _hasGradient = false;
        private byte _gradTopR, _gradTopG, _gradTopB;
        private byte _gradBotR, _gradBotG, _gradBotB;

        //
        // Camera eye position (needed for specular highlights).  Set during each Render call.
        //
        private float _eyeX, _eyeY, _eyeZ;

        //
        // Alpha blending state — set to true during the transparent pass.
        // When true, pixel writes use src-alpha blending and skip Z-buffer writes.
        //
        private bool _isTransparentPass = false;

        //
        // Edge rendering options
        //

        /// <summary>
        /// When true, edge lines from the mesh are drawn on top of the rasterized triangles
        /// using Bresenham's line algorithm with a small Z-bias to prevent Z-fighting.
        /// Default is true for the classic LEGO instruction-manual look.
        /// </summary>
        public bool RenderEdges { get; set; } = true;

        /// <summary>
        /// When true, triangles with per-vertex normals use Gouraud shading (per-pixel
        /// normal interpolation) instead of flat shading.
        /// Default is true.
        /// </summary>
        public bool SmoothShading { get; set; } = true;

        /// <summary>
        /// The lighting configuration.  Defaults to LightingModel.Default() which
        /// matches the original single-light rendering behaviour.
        /// </summary>
        public LightingModel Lighting { get; set; } = LightingModel.Default();

        //
        // Z-bias applied to edge line depth values so they render slightly in front of
        // the surface they sit on.  This prevents Z-fighting between co-planar triangles
        // and the edge lines that trace their outlines.
        //
        private const float EDGE_DEPTH_BIAS = -0.0005f;


        public SoftwareRenderer(int width, int height)
        {
            _width = width;
            _height = height;
            _colourBuffer = new byte[width * height * 4];
            _depthBuffer = new float[width * height];
        }


        /// <summary>
        /// Clear the colour and depth buffers to their default state.
        /// </summary>
        private void ClearBuffers()
        {
            Array.Fill(_depthBuffer, float.MaxValue);

            if (_hasGradient)
            {
                //
                // Gradient fill: interpolate per-scanline from top to bottom colour
                //
                for (int y = 0; y < _height; y++)
                {
                    float t = (float)y / MathF.Max(_height - 1, 1);
                    byte r = (byte)(_gradTopR + (_gradBotR - _gradTopR) * t);
                    byte g = (byte)(_gradTopG + (_gradBotG - _gradTopG) * t);
                    byte b = (byte)(_gradTopB + (_gradBotB - _gradTopB) * t);

                    int rowStart = y * _width * 4;
                    for (int x = 0; x < _width; x++)
                    {
                        int idx = rowStart + x * 4;
                        _colourBuffer[idx]     = r;
                        _colourBuffer[idx + 1] = g;
                        _colourBuffer[idx + 2] = b;
                        _colourBuffer[idx + 3] = 255;
                    }
                }
            }
            else
            {
                //
                // Solid fill
                //
                for (int i = 0; i < _colourBuffer.Length; i += 4)
                {
                    _colourBuffer[i]     = _bgR;
                    _colourBuffer[i + 1] = _bgG;
                    _colourBuffer[i + 2] = _bgB;
                    _colourBuffer[i + 3] = _bgA;
                }
            }
        }


        /// <summary>
        /// Sort a list of transparent triangles back-to-front by their centroid depth
        /// in clip space.  Farthest triangles render first (painter's algorithm).
        /// </summary>
        private void SortBackToFront(System.Collections.Generic.List<MeshTriangle> triangles, float[] vpMatrix)
        {
            //
            // Compute centroid depth for each triangle for sorting
            //
            float[] depths = new float[triangles.Count];
            for (int i = 0; i < triangles.Count; i++)
            {
                MeshTriangle t = triangles[i];
                float cx = (t.X1 + t.X2 + t.X3) / 3f;
                float cy = (t.Y1 + t.Y2 + t.Y3) / 3f;
                float cz = (t.Z1 + t.Z2 + t.Z3) / 3f;

                //
                // Transform centroid to clip space to get a comparable depth
                //
                float clipZ = vpMatrix[8] * cx + vpMatrix[9] * cy + vpMatrix[10] * cz + vpMatrix[11];
                float clipW = vpMatrix[12] * cx + vpMatrix[13] * cy + vpMatrix[14] * cz + vpMatrix[15];

                depths[i] = (clipW > 0.001f) ? clipZ / clipW : float.MaxValue;
            }

            //
            // Simple insertion sort (fast for small N, stable, in-place)
            //
            for (int i = 1; i < triangles.Count; i++)
            {
                float keyDepth = depths[i];
                MeshTriangle keyTri = triangles[i];
                int j = i - 1;

                while (j >= 0 && depths[j] < keyDepth) // farthest first (largest depth first)
                {
                    depths[j + 1] = depths[j];
                    triangles[j + 1] = triangles[j];
                    j--;
                }

                depths[j + 1] = keyDepth;
                triangles[j + 1] = keyTri;
            }
        }


        /// <summary>
        /// Write a pixel to the framebuffer, respecting the current pass mode.
        /// During opaque pass: standard Z-buffer overwrite.
        /// During transparent pass: alpha blend with existing pixel, no Z-buffer write.
        /// </summary>
        private void WritePixel(int pixelIdx, float depth, byte r, byte g, byte b, byte a)
        {
            if (_isTransparentPass == true)
            {
                //
                // Transparent pass: read Z-buffer for occlusion but don't write.
                // Alpha-blend with existing framebuffer contents.
                //
                if (depth < _depthBuffer[pixelIdx])
                {
                    int bufIdx = pixelIdx * 4;
                    float srcA = a / 255f;
                    float invSrcA = 1f - srcA;

                    _colourBuffer[bufIdx]     = (byte)(r * srcA + _colourBuffer[bufIdx]     * invSrcA);
                    _colourBuffer[bufIdx + 1] = (byte)(g * srcA + _colourBuffer[bufIdx + 1] * invSrcA);
                    _colourBuffer[bufIdx + 2] = (byte)(b * srcA + _colourBuffer[bufIdx + 2] * invSrcA);

                    //
                    // Output alpha: src + dst * (1 - src)
                    //
                    float dstA = _colourBuffer[bufIdx + 3] / 255f;
                    float outA = srcA + dstA * invSrcA;
                    _colourBuffer[bufIdx + 3] = (byte)(outA * 255f);
                }
            }
            else
            {
                //
                // Opaque pass: standard Z-buffer overwrite
                //
                if (depth < _depthBuffer[pixelIdx])
                {
                    _depthBuffer[pixelIdx] = depth;
                    int bufIdx = pixelIdx * 4;
                    _colourBuffer[bufIdx]     = r;
                    _colourBuffer[bufIdx + 1] = g;
                    _colourBuffer[bufIdx + 2] = b;
                    _colourBuffer[bufIdx + 3] = a;
                }
            }
        }
        /// <summary>Set the background colour (default is transparent black).</summary>
        public void SetBackground(byte r, byte g, byte b, byte a)
        {
            _bgR = r; _bgG = g; _bgB = b; _bgA = a;
            _hasGradient = false;
        }


        /// <summary>
        /// Set a vertical gradient background.  The top colour linearly blends to
        /// the bottom colour across scanlines.  Alpha is set to 255.
        /// </summary>
        public void SetGradientBackground(byte topR, byte topG, byte topB,
                                           byte bottomR, byte bottomG, byte bottomB)
        {
            _hasGradient = true;
            _gradTopR = topR; _gradTopG = topG; _gradTopB = topB;
            _gradBotR = bottomR; _gradBotG = bottomG; _gradBotB = bottomB;
        }


        /// <summary>
        /// Render a mesh using the given camera.  Returns the RGBA pixel buffer.
        ///
        /// Rendering order:
        ///   1. All opaque triangles (A==255) are rasterized with Z-buffer depth testing.
        ///   2. Transparent triangles (A&lt;255) are sorted back-to-front and drawn with alpha blending.
        ///   3. If RenderEdges is true, edge lines are drawn on top with a small depth bias.
        /// </summary>
        public byte[] Render(LDrawMesh mesh, Camera camera)
        {
            //
            // Clear buffers
            //
            ClearBuffers();

            //
            // Build the combined view-projection matrix
            //
            float aspect = (float)_width / _height;
            float[] viewMatrix = camera.GetViewMatrix();
            float[] projMatrix = camera.GetProjectionMatrix(aspect);
            float[] vpMatrix = MultiplyMatrices(projMatrix, viewMatrix);

            //
            // Capture the camera eye position for specular highlight computation
            //
            _eyeX = camera.EyeX;
            _eyeY = camera.EyeY;
            _eyeZ = camera.EyeZ;

            //
            // Split triangles into opaque and transparent groups
            //
            mesh.SplitByTransparency(out System.Collections.Generic.List<MeshTriangle> opaqueTriangles,
                                     out System.Collections.Generic.List<MeshTriangle> transparentTriangles);

            //
            // Pass 1 — Render all opaque triangles with Z-buffer (parallel)
            //
            // Each triangle writes only to its own bounding-box region.
            // Rare pixel-level races at triangle edges are benign — both
            // candidates are valid depth values.
            //
            _isTransparentPass = false;
            Parallel.For(0, opaqueTriangles.Count, i =>
            {
                MeshTriangle tri = opaqueTriangles[i];
                RasterizeTriangle(ref tri, vpMatrix);
            });

            //
            // Pass 2 — Render transparent triangles, sorted back-to-front (painter's algorithm)
            //
            if (transparentTriangles.Count > 0)
            {
                SortBackToFront(transparentTriangles, vpMatrix);

                _isTransparentPass = true;
                for (int i = 0; i < transparentTriangles.Count; i++)
                {
                    MeshTriangle tri = transparentTriangles[i];
                    RasterizeTriangle(ref tri, vpMatrix);
                }
                _isTransparentPass = false;
            }

            //
            // Pass 3 — Render edge lines on top of the filled triangles
            //
            if (RenderEdges == true && mesh.EdgeLines.Count > 0)
            {
                for (int i = 0; i < mesh.EdgeLines.Count; i++)
                {
                    MeshLine edge = mesh.EdgeLines[i];
                    RasterizeEdgeLine(ref edge, vpMatrix);
                }
            }

            return _colourBuffer;
        }


        /// <summary>Width of the render target.</summary>
        public int Width => _width;

        /// <summary>Height of the render target.</summary>
        public int Height => _height;


        // ── Triangle rasterization ──

        private void RasterizeTriangle(ref MeshTriangle tri, float[] vpMatrix)
        {
            //
            // Project vertices to clip space
            //
            float cx1, cy1, cz1, cw1;
            float cx2, cy2, cz2, cw2;
            float cx3, cy3, cz3, cw3;

            ProjectPoint(tri.X1, tri.Y1, tri.Z1, vpMatrix, out cx1, out cy1, out cz1, out cw1);
            ProjectPoint(tri.X2, tri.Y2, tri.Z2, vpMatrix, out cx2, out cy2, out cz2, out cw2);
            ProjectPoint(tri.X3, tri.Y3, tri.Z3, vpMatrix, out cx3, out cy3, out cz3, out cw3);

            //
            // Simple near-plane clipping: skip if any vertex is behind the camera
            //
            if (cw1 <= 0.001f || cw2 <= 0.001f || cw3 <= 0.001f)
            {
                return;
            }

            //
            // Perspective divide → NDC (-1 to 1)
            //
            float nx1 = cx1 / cw1, ny1 = cy1 / cw1, nz1 = cz1 / cw1;
            float nx2 = cx2 / cw2, ny2 = cy2 / cw2, nz2 = cz2 / cw2;
            float nx3 = cx3 / cw3, ny3 = cy3 / cw3, nz3 = cz3 / cw3;

            //
            // NDC → screen coordinates
            //
            float sx1 = (nx1 + 1f) * 0.5f * _width;
            float sy1 = (1f - ny1) * 0.5f * _height; // flip Y (screen Y is top-down)
            float sx2 = (nx2 + 1f) * 0.5f * _width;
            float sy2 = (1f - ny2) * 0.5f * _height;
            float sx3 = (nx3 + 1f) * 0.5f * _width;
            float sy3 = (1f - ny3) * 0.5f * _height;

            //
            // Back-face culling (screen-space winding order).
            // Only applied when CullBackFace is set — i.e. the triangle comes from
            // a BFC-certified part and is opaque.  Non-certified and transparent
            // triangles render both sides.
            //
            float cross2d = (sx2 - sx1) * (sy3 - sy1) - (sy2 - sy1) * (sx3 - sx1);
            if (tri.CullBackFace == true && cross2d >= 0)
            {
                return; // CW in screen space = back face
            }

            //
            // Choose shading mode
            //
            if (SmoothShading == true && tri.HasPerVertexNormals == true)
            {
                //
                // Gouraud shading — pass per-vertex normals through to FillTriangle
                // for per-pixel lighting interpolation
                //
                FillTriangleSmooth(
                    sx1, sy1, nz1, sx2, sy2, nz2, sx3, sy3, nz3,
                    tri.NX1, tri.NY1, tri.NZ1,
                    tri.NX2, tri.NY2, tri.NZ2,
                    tri.NX3, tri.NY3, tri.NZ3,
                    tri.X1, tri.Y1, tri.Z1,
                    tri.X2, tri.Y2, tri.Z2,
                    tri.X3, tri.Y3, tri.Z3,
                    tri.R, tri.G, tri.B, tri.A);
            }
            else
            {
                //
                // Flat shading — compute lighting once per triangle using centroid
                //
                float centroidX = (tri.X1 + tri.X2 + tri.X3) / 3f;
                float centroidY = (tri.Y1 + tri.Y2 + tri.Y3) / 3f;
                float centroidZ = (tri.Z1 + tri.Z2 + tri.Z3) / 3f;

                ComputeLighting(tri.NX, tri.NY, tri.NZ,
                                centroidX, centroidY, centroidZ,
                                tri.R, tri.G, tri.B,
                                out byte sr, out byte sg, out byte sb);

                FillTriangleFlat(sx1, sy1, nz1, sx2, sy2, nz2, sx3, sy3, nz3, sr, sg, sb, tri.A);
            }
        }


        /// <summary>
        /// Flat-shaded triangle rasterization.  Colour has already been computed
        /// once per triangle using the flat face normal.
        /// </summary>
        private void FillTriangleFlat(
            float x1, float y1, float z1,
            float x2, float y2, float z2,
            float x3, float y3, float z3,
            byte r, byte g, byte b, byte a)
        {
            //
            // Bounding box (clamped to screen)
            //
            int minX = Math.Max(0, (int)MathF.Floor(MathF.Min(x1, MathF.Min(x2, x3))));
            int maxX = Math.Min(_width - 1, (int)MathF.Ceiling(MathF.Max(x1, MathF.Max(x2, x3))));
            int minY = Math.Max(0, (int)MathF.Floor(MathF.Min(y1, MathF.Min(y2, y3))));
            int maxY = Math.Min(_height - 1, (int)MathF.Ceiling(MathF.Max(y1, MathF.Max(y2, y3))));

            if (minX > maxX || minY > maxY)
            {
                return;
            }

            //
            // Precompute edge function denominators
            //
            float denom = (y2 - y3) * (x1 - x3) + (x3 - x2) * (y1 - y3);
            if (MathF.Abs(denom) < 1e-8f)
            {
                return; // degenerate triangle
            }

            float invDenom = 1f / denom;

            for (int py = minY; py <= maxY; py++)
            {
                for (int px = minX; px <= maxX; px++)
                {
                    float pxf = px + 0.5f;
                    float pyf = py + 0.5f;

                    //
                    // Barycentric coordinates
                    //
                    float w1 = ((y2 - y3) * (pxf - x3) + (x3 - x2) * (pyf - y3)) * invDenom;
                    float w2 = ((y3 - y1) * (pxf - x3) + (x1 - x3) * (pyf - y3)) * invDenom;
                    float w3 = 1f - w1 - w2;

                    //
                    // Inside test (with small epsilon for edge coverage)
                    //
                    if (w1 < -0.001f || w2 < -0.001f || w3 < -0.001f)
                    {
                        continue;
                    }

                    //
                    // Interpolate depth
                    //
                    float depth = w1 * z1 + w2 * z2 + w3 * z3;

                    int pixelIdx = py * _width + px;
                    WritePixel(pixelIdx, depth, r, g, b, a);
                }
            }
        }


        /// <summary>
        /// Gouraud-shaded triangle rasterization.  Per-vertex normals are interpolated
        /// across the triangle using barycentric coordinates, and lighting is computed
        /// per-pixel for smooth shading across curved surfaces.
        /// </summary>
        private void FillTriangleSmooth(
            float x1, float y1, float z1,
            float x2, float y2, float z2,
            float x3, float y3, float z3,
            float vnx1, float vny1, float vnz1,
            float vnx2, float vny2, float vnz2,
            float vnx3, float vny3, float vnz3,
            float wx1, float wy1, float wz1,
            float wx2, float wy2, float wz2,
            float wx3, float wy3, float wz3,
            byte baseR, byte baseG, byte baseB, byte baseA)
        {
            //
            // Bounding box (clamped to screen)
            //
            int minX = Math.Max(0, (int)MathF.Floor(MathF.Min(x1, MathF.Min(x2, x3))));
            int maxX = Math.Min(_width - 1, (int)MathF.Ceiling(MathF.Max(x1, MathF.Max(x2, x3))));
            int minY = Math.Max(0, (int)MathF.Floor(MathF.Min(y1, MathF.Min(y2, y3))));
            int maxY = Math.Min(_height - 1, (int)MathF.Ceiling(MathF.Max(y1, MathF.Max(y2, y3))));

            if (minX > maxX || minY > maxY)
            {
                return;
            }

            //
            // Precompute edge function denominators
            //
            float denom = (y2 - y3) * (x1 - x3) + (x3 - x2) * (y1 - y3);
            if (Math.Abs(denom) < 1e-8f)
            {
                return; // degenerate triangle
            }

            float invDenom = 1f / denom;

            for (int py = minY; py <= maxY; py++)
            {
                for (int px = minX; px <= maxX; px++)
                {
                    float pxf = px + 0.5f;
                    float pyf = py + 0.5f;

                    //
                    // Barycentric coordinates
                    //
                    float w1 = ((y2 - y3) * (pxf - x3) + (x3 - x2) * (pyf - y3)) * invDenom;
                    float w2 = ((y3 - y1) * (pxf - x3) + (x1 - x3) * (pyf - y3)) * invDenom;
                    float w3 = 1f - w1 - w2;

                    //
                    // Inside test
                    //
                    if (w1 < -0.001f || w2 < -0.001f || w3 < -0.001f)
                    {
                        continue;
                    }

                    //
                    // Interpolate depth
                    //
                    float depth = w1 * z1 + w2 * z2 + w3 * z3;

                    int pixelIdx = py * _width + px;
                    if (depth < _depthBuffer[pixelIdx])
                    {
                        //
                        // Interpolate the normal across the triangle using barycentric coords
                        //
                        float inx = w1 * vnx1 + w2 * vnx2 + w3 * vnx3;
                        float iny = w1 * vny1 + w2 * vny2 + w3 * vny3;
                        float inz = w1 * vnz1 + w2 * vnz2 + w3 * vnz3;

                        //
                        // Normalize the interpolated normal
                        //
                        float nlen = MathF.Sqrt(inx * inx + iny * iny + inz * inz);
                        if (nlen > 1e-8f)
                        {
                            inx /= nlen;
                            iny /= nlen;
                            inz /= nlen;
                        }

                        //
                        // Interpolate world-space position for specular computation
                        //
                        float wpx = w1 * wx1 + w2 * wx2 + w3 * wx3;
                        float wpy = w1 * wy1 + w2 * wy2 + w3 * wy3;
                        float wpz = w1 * wz1 + w2 * wz2 + w3 * wz3;

                        //
                        // Compute lighting using the interpolated normal and world position
                        //
                        ComputeLighting(inx, iny, inz, wpx, wpy, wpz,
                                        baseR, baseG, baseB,
                                        out byte finalR, out byte finalG, out byte finalB);

                        WritePixel(pixelIdx, depth, finalR, finalG, finalB, baseA);
                    }
                }
            }
        }


        // ── Multi-light Blinn-Phong lighting ──

        /// <summary>
        /// Compute the final lit colour at a surface point using the active LightingModel.
        ///
        /// For each light source:
        ///   - Diffuse:  max(0, N·L)  × light colour × light intensity
        ///   - Specular: pow(max(0, N·H), shininess)  × specular intensity  (Blinn-Phong)
        ///
        /// The results are accumulated across all lights, combined with the ambient term,
        /// multiplied with the base surface colour, and clamped to [0, 255].
        /// </summary>
        private void ComputeLighting(float nx, float ny, float nz,
                                     float worldX, float worldY, float worldZ,
                                     byte baseR, byte baseG, byte baseB,
                                     out byte outR, out byte outG, out byte outB)
        {
            LightingModel lighting = Lighting;

            //
            // Start with ambient contribution
            //
            float totalR = lighting.AmbientR * lighting.AmbientIntensity;
            float totalG = lighting.AmbientG * lighting.AmbientIntensity;
            float totalB = lighting.AmbientB * lighting.AmbientIntensity;

            //
            // View direction (from surface point toward the camera eye)
            //
            float viewDirX = _eyeX - worldX;
            float viewDirY = _eyeY - worldY;
            float viewDirZ = _eyeZ - worldZ;
            float vlen = MathF.Sqrt(viewDirX * viewDirX + viewDirY * viewDirY + viewDirZ * viewDirZ);

            if (vlen > 1e-8f)
            {
                viewDirX /= vlen;
                viewDirY /= vlen;
                viewDirZ /= vlen;
            }

            //
            // Accumulate contributions from each light source
            //
            for (int i = 0; i < lighting.Lights.Count; i++)
            {
                Light light = lighting.Lights[i];

                float lx = light.DirectionX;
                float ly = light.DirectionY;
                float lz = light.DirectionZ;

                //
                // For point lights, compute the direction from the surface to the light position
                //
                if (light.Type == LightType.Point)
                {
                    lx = light.DirectionX - worldX;
                    ly = light.DirectionY - worldY;
                    lz = light.DirectionZ - worldZ;
                    float plen = MathF.Sqrt(lx * lx + ly * ly + lz * lz);

                    if (plen > 1e-8f)
                    {
                        lx /= plen;
                        ly /= plen;
                        lz /= plen;
                    }
                }

                //
                // Diffuse (Lambertian)
                //
                float ndotl = nx * lx + ny * ly + nz * lz;

                if (ndotl < 0f)
                {
                    ndotl = 0f;
                }

                float diffR = ndotl * light.ColourR * light.Intensity;
                float diffG = ndotl * light.ColourG * light.Intensity;
                float diffB = ndotl * light.ColourB * light.Intensity;

                totalR += diffR;
                totalG += diffG;
                totalB += diffB;

                //
                // Specular (Blinn-Phong) — only if specular intensity is non-zero
                //
                if (lighting.SpecularIntensity > 0f && ndotl > 0f)
                {
                    //
                    // Half-vector between light direction and view direction
                    //
                    float hx = lx + viewDirX;
                    float hy = ly + viewDirY;
                    float hz = lz + viewDirZ;
                    float hlen = MathF.Sqrt(hx * hx + hy * hy + hz * hz);

                    if (hlen > 1e-8f)
                    {
                        hx /= hlen;
                        hy /= hlen;
                        hz /= hlen;
                    }

                    float ndoth = nx * hx + ny * hy + nz * hz;

                    if (ndoth > 0f)
                    {
                        float specFactor = MathF.Pow(ndoth, lighting.SpecularPower);
                        float specContrib = specFactor * lighting.SpecularIntensity * light.Intensity;

                        totalR += specContrib * light.ColourR;
                        totalG += specContrib * light.ColourG;
                        totalB += specContrib * light.ColourB;
                    }
                }
            }

            //
            // Multiply with surface colour and clamp to [0, 255]
            //
            int ir = (int)(baseR * totalR);
            int ig = (int)(baseG * totalG);
            int ib = (int)(baseB * totalB);

            outR = (byte)(ir > 255 ? 255 : (ir < 0 ? 0 : ir));
            outG = (byte)(ig > 255 ? 255 : (ig < 0 ? 0 : ig));
            outB = (byte)(ib > 255 ? 255 : (ib < 0 ? 0 : ib));
        }



        /// <summary>
        /// Rasterize a single edge line segment using Bresenham's line algorithm.
        /// The line is drawn with a small Z-bias so it sits just in front of the
        /// underlying triangle surface, preventing Z-fighting on co-planar geometry.
        /// </summary>
        private void RasterizeEdgeLine(ref MeshLine edge, float[] vpMatrix)
        {
            //
            // Project both endpoints to clip space
            //
            float cx1, cy1, cz1, cw1;
            float cx2, cy2, cz2, cw2;

            ProjectPoint(edge.X1, edge.Y1, edge.Z1, vpMatrix, out cx1, out cy1, out cz1, out cw1);
            ProjectPoint(edge.X2, edge.Y2, edge.Z2, vpMatrix, out cx2, out cy2, out cz2, out cw2);

            //
            // Skip if either endpoint is behind the near plane
            //
            if (cw1 <= 0.001f || cw2 <= 0.001f)
            {
                return;
            }

            //
            // Perspective divide → NDC
            //
            float nz1 = cz1 / cw1;
            float nz2 = cz2 / cw2;

            //
            // NDC → screen coordinates
            //
            float sx1 = (cx1 / cw1 + 1f) * 0.5f * _width;
            float sy1 = (1f - cy1 / cw1) * 0.5f * _height;
            float sx2 = (cx2 / cw2 + 1f) * 0.5f * _width;
            float sy2 = (1f - cy2 / cw2) * 0.5f * _height;

            //
            // Draw the line using Bresenham's algorithm with depth-tested pixel writes
            //
            DrawLine(sx1, sy1, nz1, sx2, sy2, nz2, edge.R, edge.G, edge.B, edge.A);
        }


        /// <summary>
        /// Bresenham-style line drawing with per-pixel depth interpolation.
        /// Each pixel is only drawn if it passes the Z-buffer test (with bias applied).
        /// </summary>
        private void DrawLine(
            float fx1, float fy1, float z1,
            float fx2, float fy2, float z2,
            byte r, byte g, byte b, byte a)
        {
            //
            // Convert float screen coords to integer pixel positions
            //
            int x1 = (int)MathF.Round(fx1);
            int y1 = (int)MathF.Round(fy1);
            int x2 = (int)MathF.Round(fx2);
            int y2 = (int)MathF.Round(fy2);

            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int stepX = (x1 < x2) ? 1 : -1;
            int stepY = (y1 < y2) ? 1 : -1;

            //
            // Total number of steps for depth interpolation
            //
            int totalSteps = Math.Max(dx, dy);
            if (totalSteps == 0)
            {
                //
                // Degenerate line (single pixel)
                //
                WriteEdgePixel(x1, y1, z1 + EDGE_DEPTH_BIAS, r, g, b, a);
                return;
            }

            float invTotalSteps = 1f / totalSteps;

            //
            // Bresenham's line algorithm
            //
            int error = dx - dy;
            int currentX = x1;
            int currentY = y1;

            for (int step = 0; step <= totalSteps; step++)
            {
                //
                // Interpolate depth along the line
                //
                float t = step * invTotalSteps;
                float depth = z1 + (z2 - z1) * t + EDGE_DEPTH_BIAS;

                WriteEdgePixel(currentX, currentY, depth, r, g, b, a);

                //
                // Advance to the next pixel along the line
                //
                int error2 = 2 * error;

                if (error2 > -dy)
                {
                    error -= dy;
                    currentX += stepX;
                }

                if (error2 < dx)
                {
                    error += dx;
                    currentY += stepY;
                }
            }
        }


        /// <summary>
        /// Write a single edge pixel if it is within screen bounds and passes the Z-buffer test.
        /// </summary>
        private void WriteEdgePixel(int px, int py, float depth, byte r, byte g, byte b, byte a)
        {
            //
            // Bounds check
            //
            if (px < 0 || px >= _width || py < 0 || py >= _height)
            {
                return;
            }

            int pixelIdx = py * _width + px;

            //
            // Z-buffer test — edge pixels use <= so they draw on top of co-planar surfaces
            //
            if (depth <= _depthBuffer[pixelIdx])
            {
                _depthBuffer[pixelIdx] = depth;
                int bufIdx = pixelIdx * 4;
                _colourBuffer[bufIdx] = r;
                _colourBuffer[bufIdx + 1] = g;
                _colourBuffer[bufIdx + 2] = b;
                _colourBuffer[bufIdx + 3] = a;
            }
        }


        // ── Projection helpers ──

        private static void ProjectPoint(float x, float y, float z, float[] m,
                                         out float ox, out float oy, out float oz, out float ow)
        {
            ox = m[0] * x + m[1] * y + m[2] * z + m[3];
            oy = m[4] * x + m[5] * y + m[6] * z + m[7];
            oz = m[8] * x + m[9] * y + m[10] * z + m[11];
            ow = m[12] * x + m[13] * y + m[14] * z + m[15];
        }


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
    }
}

