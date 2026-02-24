using System;
using System.Collections.Generic;
using System.Text;
using BMC.LDraw.Models;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Exports an LDraw mesh as a scalable vector graphics (SVG) document.
    ///
    /// Projects triangles to 2D using the same camera transforms as the raster renderer,
    /// sorts them by depth (painter's algorithm), and emits SVG polygon/line elements.
    /// Produces clean, scalable output suitable for print, instructions, or web embedding.
    ///
    /// AI-generated — Phase 3.3, Feb 2026.
    /// </summary>
    public static class SvgExporter
    {
        /// <summary>
        /// Render a mesh to an SVG string.
        /// </summary>
        /// <param name="mesh">Pre-resolved LDraw mesh.</param>
        /// <param name="camera">Camera for the view/projection transform.</param>
        /// <param name="width">SVG viewport width in pixels.</param>
        /// <param name="height">SVG viewport height in pixels.</param>
        /// <param name="renderEdges">Whether to include edge lines.</param>
        /// <param name="lighting">Optional lighting model for computing face colours.  Null uses default.</param>
        /// <returns>Complete SVG document as a string.</returns>
        public static string RenderToSvg(
            LDrawMesh mesh,
            Camera camera,
            int width,
            int height,
            bool renderEdges = true,
            LightingModel lighting = null)
        {
            if (lighting == null)
            {
                lighting = LightingModel.Default();
            }

            //
            // Build the combined view-projection matrix
            //
            float aspect = (float)width / height;
            float[] viewMatrix = camera.GetViewMatrix();
            float[] projMatrix = camera.GetProjectionMatrix(aspect);
            float[] vpMatrix = MultiplyMatrices(projMatrix, viewMatrix);

            //
            // Project all triangles to 2D and compute their depth for sorting
            //
            List<ProjectedTriangle> projected = new List<ProjectedTriangle>(mesh.Triangles.Count);

            for (int i = 0; i < mesh.Triangles.Count; i++)
            {
                MeshTriangle tri = mesh.Triangles[i];

                //
                // Project 3 vertices to screen-space
                //
                if (!ProjectPoint(tri.X1, tri.Y1, tri.Z1, vpMatrix, width, height,
                                  out float sx1, out float sy1, out float sz1)) continue;
                if (!ProjectPoint(tri.X2, tri.Y2, tri.Z2, vpMatrix, width, height,
                                  out float sx2, out float sy2, out float sz2)) continue;
                if (!ProjectPoint(tri.X3, tri.Y3, tri.Z3, vpMatrix, width, height,
                                  out float sx3, out float sy3, out float sz3)) continue;

                //
                // Compute flat-shaded colour using the face normal
                //
                float nx = tri.NX, ny = tri.NY, nz = tri.NZ;

                // Centroid for point-light position and depth
                float cx = (tri.X1 + tri.X2 + tri.X3) / 3f;
                float cy = (tri.Y1 + tri.Y2 + tri.Y3) / 3f;
                float cz = (tri.Z1 + tri.Z2 + tri.Z3) / 3f;

                ComputeFlatLighting(nx, ny, nz, cx, cy, cz,
                                    camera.EyeX, camera.EyeY, camera.EyeZ,
                                    tri.R, tri.G, tri.B,
                                    lighting,
                                    out byte litR, out byte litG, out byte litB);

                float depth = (sz1 + sz2 + sz3) / 3f;

                projected.Add(new ProjectedTriangle
                {
                    SX1 = sx1, SY1 = sy1,
                    SX2 = sx2, SY2 = sy2,
                    SX3 = sx3, SY3 = sy3,
                    R = litR, G = litG, B = litB, A = tri.A,
                    Depth = depth
                });
            }

            //
            // Sort back-to-front (farthest first — largest depth value first)
            //
            projected.Sort((a, b) => b.Depth.CompareTo(a.Depth));

            //
            // Build the SVG document
            //
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendFormat("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{0}\" height=\"{1}\" viewBox=\"0 0 {0} {1}\">",
                            width, height);
            sb.AppendLine();

            //
            // Background (transparent by default for SVG)
            //

            //
            // Draw triangles as polygons
            //
            for (int i = 0; i < projected.Count; i++)
            {
                ProjectedTriangle pt = projected[i];

                string fill;
                if (pt.A < 255)
                {
                    float opacity = pt.A / 255f;
                    fill = string.Format("fill=\"rgb({0},{1},{2})\" fill-opacity=\"{3:F2}\"",
                                         pt.R, pt.G, pt.B, opacity);
                }
                else
                {
                    fill = string.Format("fill=\"rgb({0},{1},{2})\"", pt.R, pt.G, pt.B);
                }

                sb.AppendFormat("  <polygon points=\"{0:F1},{1:F1} {2:F1},{3:F1} {4:F1},{5:F1}\" {6} stroke=\"none\"/>",
                                pt.SX1, pt.SY1, pt.SX2, pt.SY2, pt.SX3, pt.SY3, fill);
                sb.AppendLine();
            }

            //
            // Draw edge lines
            //
            if (renderEdges == true)
            {
                for (int i = 0; i < mesh.EdgeLines.Count; i++)
                {
                    MeshLine edge = mesh.EdgeLines[i];

                    if (!ProjectPoint(edge.X1, edge.Y1, edge.Z1, vpMatrix, width, height,
                                      out float ex1, out float ey1, out float _)) continue;
                    if (!ProjectPoint(edge.X2, edge.Y2, edge.Z2, vpMatrix, width, height,
                                      out float ex2, out float ey2, out float _2)) continue;

                    sb.AppendFormat("  <line x1=\"{0:F1}\" y1=\"{1:F1}\" x2=\"{2:F1}\" y2=\"{3:F1}\" stroke=\"rgb({4},{5},{6})\" stroke-width=\"0.5\"/>",
                                    ex1, ey1, ex2, ey2, edge.R, edge.G, edge.B);
                    sb.AppendLine();
                }
            }

            sb.AppendLine("</svg>");
            return sb.ToString();
        }


        // ────────────────────────────────────────────────────────
        // Internal types
        // ────────────────────────────────────────────────────────

        private struct ProjectedTriangle
        {
            public float SX1, SY1, SX2, SY2, SX3, SY3;
            public byte R, G, B, A;
            public float Depth;
        }


        // ────────────────────────────────────────────────────────
        // Projection
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Project a world-space point to screen-space (pixel coordinates).
        /// Returns false if the point is behind the camera (clipped).
        /// </summary>
        private static bool ProjectPoint(float x, float y, float z, float[] vpMatrix,
                                          int screenW, int screenH,
                                          out float sx, out float sy, out float sz)
        {
            float clipX = vpMatrix[0] * x + vpMatrix[1] * y + vpMatrix[2] * z + vpMatrix[3];
            float clipY = vpMatrix[4] * x + vpMatrix[5] * y + vpMatrix[6] * z + vpMatrix[7];
            float clipZ = vpMatrix[8] * x + vpMatrix[9] * y + vpMatrix[10] * z + vpMatrix[11];
            float clipW = vpMatrix[12] * x + vpMatrix[13] * y + vpMatrix[14] * z + vpMatrix[15];

            if (clipW < 0.001f)
            {
                sx = sy = sz = 0;
                return false;
            }

            float ndcX = clipX / clipW;
            float ndcY = clipY / clipW;
            float ndcZ = clipZ / clipW;

            sx = (ndcX + 1f) * 0.5f * screenW;
            sy = (1f - ndcY) * 0.5f * screenH;
            sz = ndcZ;
            return true;
        }


        // ────────────────────────────────────────────────────────
        // Lighting (simplified — flat per-face)
        // ────────────────────────────────────────────────────────

        private static void ComputeFlatLighting(
            float nx, float ny, float nz,
            float posX, float posY, float posZ,
            float eyeX, float eyeY, float eyeZ,
            byte baseR, byte baseG, byte baseB,
            LightingModel lighting,
            out byte outR, out byte outG, out byte outB)
        {
            float totalR = lighting.AmbientR * lighting.AmbientIntensity;
            float totalG = lighting.AmbientG * lighting.AmbientIntensity;
            float totalB = lighting.AmbientB * lighting.AmbientIntensity;

            //
            // View direction (for specular)
            //
            float viewDirX = eyeX - posX;
            float viewDirY = eyeY - posY;
            float viewDirZ = eyeZ - posZ;
            float vLen = (float)Math.Sqrt(viewDirX * viewDirX + viewDirY * viewDirY + viewDirZ * viewDirZ);
            if (vLen > 1e-8f)
            {
                viewDirX /= vLen; viewDirY /= vLen; viewDirZ /= vLen;
            }

            for (int i = 0; i < lighting.Lights.Count; i++)
            {
                Light light = lighting.Lights[i];

                float lx, ly, lz;
                if (light.Type == LightType.Point)
                {
                    lx = light.DirectionX - posX;
                    ly = light.DirectionY - posY;
                    lz = light.DirectionZ - posZ;
                    float len = (float)Math.Sqrt(lx * lx + ly * ly + lz * lz);
                    if (len > 1e-8f) { lx /= len; ly /= len; lz /= len; }
                }
                else
                {
                    lx = light.DirectionX;
                    ly = light.DirectionY;
                    lz = light.DirectionZ;
                }

                // Diffuse
                float ndotl = nx * lx + ny * ly + nz * lz;
                if (ndotl < 0f) ndotl = 0f;

                totalR += ndotl * light.ColourR * light.Intensity;
                totalG += ndotl * light.ColourG * light.Intensity;
                totalB += ndotl * light.ColourB * light.Intensity;

                // Specular (Blinn-Phong)
                if (lighting.SpecularIntensity > 0)
                {
                    float hx = lx + viewDirX;
                    float hy = ly + viewDirY;
                    float hz = lz + viewDirZ;
                    float hLen = (float)Math.Sqrt(hx * hx + hy * hy + hz * hz);
                    if (hLen > 1e-8f)
                    {
                        hx /= hLen; hy /= hLen; hz /= hLen;
                        float ndoth = nx * hx + ny * hy + nz * hz;
                        if (ndoth < 0f) ndoth = 0f;
                        float spec = (float)Math.Pow(ndoth, lighting.SpecularPower)
                                     * lighting.SpecularIntensity * light.Intensity;
                        totalR += spec;
                        totalG += spec;
                        totalB += spec;
                    }
                }
            }

            int ir = (int)(baseR * totalR);
            int ig = (int)(baseG * totalG);
            int ib = (int)(baseB * totalB);

            if (ir > 255) ir = 255;
            if (ig > 255) ig = 255;
            if (ib > 255) ib = 255;

            outR = (byte)ir;
            outG = (byte)ig;
            outB = (byte)ib;
        }


        // ────────────────────────────────────────────────────────
        // Matrix math (duplicated from SoftwareRenderer to avoid coupling)
        // ────────────────────────────────────────────────────────

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
