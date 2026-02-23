using System;
using BMC.LDraw.Models;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// CPU-based triangle rasterizer. Renders an LDrawMesh to an RGBA pixel buffer
    /// using scanline triangle filling with Z-buffer depth testing and flat shading.
    /// </summary>
    public class SoftwareRenderer
    {
        private readonly int _width;
        private readonly int _height;

        // Framebuffer
        private byte[] _colourBuffer;
        private float[] _depthBuffer;

        // Directional light (normalized direction pointing TOWARD the light)
        private float _lightX = 0.3f, _lightY = -0.7f, _lightZ = 0.6f;

        // Ambient light intensity (0–1)
        private float _ambient = 0.35f;

        // Background colour (transparent by default)
        private byte _bgR = 0, _bgG = 0, _bgB = 0, _bgA = 0;

        public SoftwareRenderer(int width, int height)
        {
            _width = width;
            _height = height;
            _colourBuffer = new byte[width * height * 4];
            _depthBuffer = new float[width * height];

            // Normalize light direction
            float len = (float)Math.Sqrt(_lightX * _lightX + _lightY * _lightY + _lightZ * _lightZ);
            _lightX /= len; _lightY /= len; _lightZ /= len;
        }

        /// <summary>Set the background colour (default is transparent black).</summary>
        public void SetBackground(byte r, byte g, byte b, byte a)
        {
            _bgR = r; _bgG = g; _bgB = b; _bgA = a;
        }

        /// <summary>
        /// Render a mesh using the given camera. Returns the RGBA pixel buffer.
        /// </summary>
        public byte[] Render(LDrawMesh mesh, Camera camera)
        {
            // Clear buffers
            for (int i = 0; i < _depthBuffer.Length; i++)
            {
                _depthBuffer[i] = float.MaxValue;
            }
            for (int i = 0; i < _colourBuffer.Length; i += 4)
            {
                _colourBuffer[i] = _bgR;
                _colourBuffer[i + 1] = _bgG;
                _colourBuffer[i + 2] = _bgB;
                _colourBuffer[i + 3] = _bgA;
            }

            float aspect = (float)_width / _height;
            float[] viewMatrix = camera.GetViewMatrix();
            float[] projMatrix = camera.GetProjectionMatrix(aspect);
            float[] vpMatrix = MultiplyMatrices(projMatrix, viewMatrix);

            // Render all triangles
            for (int i = 0; i < mesh.Triangles.Count; i++)
            {
                MeshTriangle tri = mesh.Triangles[i];
                RasterizeTriangle(ref tri, vpMatrix);
            }

            return _colourBuffer;
        }

        /// <summary>Width of the render target.</summary>
        public int Width => _width;

        /// <summary>Height of the render target.</summary>
        public int Height => _height;

        private void RasterizeTriangle(ref MeshTriangle tri, float[] vpMatrix)
        {
            // Project vertices to clip space
            float cx1, cy1, cz1, cw1;
            float cx2, cy2, cz2, cw2;
            float cx3, cy3, cz3, cw3;

            ProjectPoint(tri.X1, tri.Y1, tri.Z1, vpMatrix, out cx1, out cy1, out cz1, out cw1);
            ProjectPoint(tri.X2, tri.Y2, tri.Z2, vpMatrix, out cx2, out cy2, out cz2, out cw2);
            ProjectPoint(tri.X3, tri.Y3, tri.Z3, vpMatrix, out cx3, out cy3, out cz3, out cw3);

            // Simple near-plane clipping: skip if any vertex is behind the camera
            if (cw1 <= 0.001f || cw2 <= 0.001f || cw3 <= 0.001f) return;

            // Perspective divide → NDC (-1 to 1)
            float nx1 = cx1 / cw1, ny1 = cy1 / cw1, nz1 = cz1 / cw1;
            float nx2 = cx2 / cw2, ny2 = cy2 / cw2, nz2 = cz2 / cw2;
            float nx3 = cx3 / cw3, ny3 = cy3 / cw3, nz3 = cz3 / cw3;

            // NDC → screen coordinates
            float sx1 = (nx1 + 1f) * 0.5f * _width;
            float sy1 = (1f - ny1) * 0.5f * _height; // flip Y (screen Y is top-down)
            float sx2 = (nx2 + 1f) * 0.5f * _width;
            float sy2 = (1f - ny2) * 0.5f * _height;
            float sx3 = (nx3 + 1f) * 0.5f * _width;
            float sy3 = (1f - ny3) * 0.5f * _height;

            // Back-face culling (screen-space winding order)
            float cross2d = (sx2 - sx1) * (sy3 - sy1) - (sy2 - sy1) * (sx3 - sx1);
            if (cross2d >= 0) return; // CW in screen space = back face

            // Compute flat shading intensity
            float ndotl = tri.NX * _lightX + tri.NY * _lightY + tri.NZ * _lightZ;
            if (ndotl < 0) ndotl = 0;
            float intensity = Math.Min(1f, _ambient + (1f - _ambient) * ndotl);

            byte sr = (byte)(tri.R * intensity);
            byte sg = (byte)(tri.G * intensity);
            byte sb = (byte)(tri.B * intensity);
            byte sa = tri.A;

            // Rasterize with bounding box + barycentric coordinates
            FillTriangle(sx1, sy1, nz1, sx2, sy2, nz2, sx3, sy3, nz3, sr, sg, sb, sa);
        }

        private void FillTriangle(
            float x1, float y1, float z1,
            float x2, float y2, float z2,
            float x3, float y3, float z3,
            byte r, byte g, byte b, byte a)
        {
            // Bounding box (clamped to screen)
            int minX = Math.Max(0, (int)Math.Floor(Math.Min(x1, Math.Min(x2, x3))));
            int maxX = Math.Min(_width - 1, (int)Math.Ceiling(Math.Max(x1, Math.Max(x2, x3))));
            int minY = Math.Max(0, (int)Math.Floor(Math.Min(y1, Math.Min(y2, y3))));
            int maxY = Math.Min(_height - 1, (int)Math.Ceiling(Math.Max(y1, Math.Max(y2, y3))));

            if (minX > maxX || minY > maxY) return;

            // Precompute edge function denominators
            float denom = (y2 - y3) * (x1 - x3) + (x3 - x2) * (y1 - y3);
            if (Math.Abs(denom) < 1e-8f) return; // degenerate triangle
            float invDenom = 1f / denom;

            for (int py = minY; py <= maxY; py++)
            {
                for (int px = minX; px <= maxX; px++)
                {
                    float pxf = px + 0.5f;
                    float pyf = py + 0.5f;

                    // Barycentric coordinates
                    float w1 = ((y2 - y3) * (pxf - x3) + (x3 - x2) * (pyf - y3)) * invDenom;
                    float w2 = ((y3 - y1) * (pxf - x3) + (x1 - x3) * (pyf - y3)) * invDenom;
                    float w3 = 1f - w1 - w2;

                    // Inside test (with small epsilon for edge coverage)
                    if (w1 < -0.001f || w2 < -0.001f || w3 < -0.001f) continue;

                    // Interpolate depth
                    float depth = w1 * z1 + w2 * z2 + w3 * z3;

                    int pixelIdx = py * _width + px;
                    if (depth < _depthBuffer[pixelIdx])
                    {
                        _depthBuffer[pixelIdx] = depth;
                        int bufIdx = pixelIdx * 4;
                        _colourBuffer[bufIdx] = r;
                        _colourBuffer[bufIdx + 1] = g;
                        _colourBuffer[bufIdx + 2] = b;
                        _colourBuffer[bufIdx + 3] = a;
                    }
                }
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
