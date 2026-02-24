using System;
using System.Collections.Generic;
using BMC.LDraw.Models;
using BMC.LDraw.Parsers;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Facade that ties parsing, resolution, and rendering together.
    /// Provides a single-call API to render an LDraw file to a PNG image.
    ///
    /// AI-generated — Phase 1.1 (edges) and Phase 1.2 (smooth shading) added Feb 2026.
    /// </summary>
    public class RenderService
    {
        private readonly string _libraryPath;
        private List<LDrawColour> _colours;


        /// <summary>
        /// Create the render service with the path to the LDraw library.
        /// </summary>
        /// <param name="libraryPath">Root of the LDraw library (the "ldraw" directory).</param>
        public RenderService(string libraryPath)
        {
            _libraryPath = libraryPath;
        }


        /// <summary>
        /// Render an LDraw file to a PNG file.
        /// </summary>
        /// <param name="inputPath">Path to the .ldr, .mpd, or .dat file.</param>
        /// <param name="outputPath">Path for the output PNG file.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="colourCode">Override colour code. Use -1 for default (colour 4 = red).</param>
        /// <param name="renderEdges">When true, edge lines are drawn on top of the rasterized triangles for the classic LEGO instruction look.</param>
        /// <param name="smoothShading">When true, per-vertex normals are computed for Gouraud smooth shading on curved surfaces.</param>
        /// <param name="antiAliasMode">Anti-aliasing mode.  SSAA2x/4x render at higher resolution and downsample for smoother edges.</param>
        /// <param name="backgroundHex">Solid background colour as hex string (e.g. "#FFFFFF").  Null = transparent.</param>
        /// <param name="gradientTopHex">Top colour for gradient background (e.g. "#1A1A2E").  Both top and bottom must be set.</param>
        /// <param name="gradientBottomHex">Bottom colour for gradient background (e.g. "#16213E").  Both top and bottom must be set.</param>
        public void RenderToFile(string inputPath,
                                 string outputPath,
                                 int width = 512,
                                 int height = 512,
                                 int colourCode = -1,
                                 float elevation = 30f,
                                 float azimuth = -45f,
                                 bool renderEdges = true,
                                 bool smoothShading = true,
                                 AntiAliasMode antiAliasMode = AntiAliasMode.None,
                                 string backgroundHex = null,
                                 string gradientTopHex = null,
                                 string gradientBottomHex = null)
        {
            byte[] pixels = RenderToPixels(inputPath: inputPath,
                                           width: width,
                                           height: height,
                                           colourCode: colourCode,
                                           elevation: elevation,
                                           azimuth: azimuth,
                                           renderEdges: renderEdges,
                                           smoothShading: smoothShading,
                                           antiAliasMode: antiAliasMode,
                                           backgroundHex: backgroundHex,
                                           gradientTopHex: gradientTopHex,
                                           gradientBottomHex: gradientBottomHex);

            ImageExporter.SaveToPng(pixels, width, height, outputPath);
        }


        /// <summary>
        /// Render an LDraw file to PNG bytes (in-memory).
        /// </summary>
        public byte[] RenderToPng(string inputPath,
                                  int width = 512,
                                  int height = 512,
                                  int colourCode = -1,
                                  float elevation = 30f,
                                  float azimuth = -45f,
                                  bool renderEdges = true,
                                  bool smoothShading = true,
                                  AntiAliasMode antiAliasMode = AntiAliasMode.None,
                                  string backgroundHex = null,
                                  string gradientTopHex = null,
                                  string gradientBottomHex = null)
        {
            byte[] pixels = RenderToPixels(inputPath: inputPath,
                                           width: width,
                                           height: height,
                                           colourCode: colourCode,
                                           elevation: elevation,
                                           azimuth: azimuth,
                                           renderEdges: renderEdges,
                                           smoothShading: smoothShading,
                                           antiAliasMode: antiAliasMode,
                                           backgroundHex: backgroundHex,
                                           gradientTopHex: gradientTopHex,
                                           gradientBottomHex: gradientBottomHex);

            return ImageExporter.ToPngBytes(pixels, width, height);
        }


        /// <summary>
        /// Render an LDraw file to WebP bytes (in-memory).
        /// </summary>
        /// <param name="quality">WebP quality (1–100).  Higher = better quality, larger file.</param>
        public byte[] RenderToWebP(string inputPath,
                                   int width = 512,
                                   int height = 512,
                                   int colourCode = -1,
                                   float elevation = 30f,
                                   float azimuth = -45f,
                                   bool renderEdges = true,
                                   bool smoothShading = true,
                                   AntiAliasMode antiAliasMode = AntiAliasMode.None,
                                   string backgroundHex = null,
                                   string gradientTopHex = null,
                                   string gradientBottomHex = null,
                                   int quality = 90)
        {
            byte[] pixels = RenderToPixels(inputPath: inputPath,
                                           width: width,
                                           height: height,
                                           colourCode: colourCode,
                                           elevation: elevation,
                                           azimuth: azimuth,
                                           renderEdges: renderEdges,
                                           smoothShading: smoothShading,
                                           antiAliasMode: antiAliasMode,
                                           backgroundHex: backgroundHex,
                                           gradientTopHex: gradientTopHex,
                                           gradientBottomHex: gradientBottomHex);

            return ImageExporter.ToWebPBytes(pixels, width, height, quality);
        }


        /// <summary>
        /// Render an LDraw file to raw RGBA pixels.
        /// </summary>
        public byte[] RenderToPixels(string inputPath,
                                     int width = 512,
                                     int height = 512,
                                     int colourCode = -1,
                                     float elevation = 30f,
                                     float azimuth = -45f,
                                     bool renderEdges = true,
                                     bool smoothShading = true,
                                     AntiAliasMode antiAliasMode = AntiAliasMode.None,
                                     string backgroundHex = null,
                                     string gradientTopHex = null,
                                     string gradientBottomHex = null)
        {
            EnsureColours();

            //
            // Default colour: 4 = Red (a recognizable default for LDraw parts)
            //
            int effectiveColour = colourCode >= 0 ? colourCode : 4;

            //
            // Parse and resolve
            //
            GeometryResolver resolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh mesh = resolver.ResolveFile(inputPath, effectiveColour);

            if (mesh.Triangles.Count == 0)
            {
                //
                // Empty mesh — return transparent image
                //
                return new byte[width * height * 4];
            }

            //
            // Smooth normals (must run before rendering, after resolution)
            //
            if (smoothShading == true)
            {
                NormalSmoother.Smooth(mesh);
            }

            //
            // Set up camera with user-specified viewing angle
            //
            Camera camera = new Camera();
            camera.AutoFrame(mesh, elevation, azimuth);

            //
            // Determine render resolution (SSAA renders at a higher internal resolution)
            //
            int ssaaFactor = 1;
            if (antiAliasMode == AntiAliasMode.SSAA2x) ssaaFactor = 2;
            else if (antiAliasMode == AntiAliasMode.SSAA4x) ssaaFactor = 4;

            int renderW = width * ssaaFactor;
            int renderH = height * ssaaFactor;

            //
            // Render
            //
            SoftwareRenderer renderer = new SoftwareRenderer(renderW, renderH);
            renderer.RenderEdges = renderEdges;
            renderer.SmoothShading = smoothShading;

            //
            // Configure background
            //
            if (gradientTopHex != null && gradientBottomHex != null)
            {
                ParseHex(gradientTopHex, out byte tr, out byte tg, out byte tb);
                ParseHex(gradientBottomHex, out byte br, out byte bg, out byte bb);
                renderer.SetGradientBackground(tr, tg, tb, br, bg, bb);
            }
            else if (backgroundHex != null)
            {
                ParseHex(backgroundHex, out byte bgr, out byte bgg, out byte bgb);
                renderer.SetBackground(bgr, bgg, bgb, 255);
            }

            byte[] pixels = renderer.Render(mesh, camera);

            //
            // Downsample if SSAA is enabled
            //
            if (ssaaFactor > 1)
            {
                pixels = PostProcess.Downsample(pixels, renderW, renderH, width, height);
            }

            return pixels;
        }


        /// <summary>
        /// Render an animated turntable GIF of an LDraw file.
        /// </summary>
        /// <param name="frameCount">Number of frames in the loop (e.g. 36 for 10° per frame).</param>
        /// <param name="frameDelayMs">Delay between frames in milliseconds (default 80ms ≈ 12.5 fps).</param>
        public byte[] RenderTurntableGif(string inputPath,
                                          int width = 256,
                                          int height = 256,
                                          int colourCode = -1,
                                          int frameCount = 36,
                                          float elevation = 30f,
                                          int frameDelayMs = 80,
                                          bool renderEdges = true,
                                          bool smoothShading = true)
        {
            EnsureColours();

            int effectiveColour = colourCode >= 0 ? colourCode : 4;

            GeometryResolver resolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh mesh = resolver.ResolveFile(inputPath, effectiveColour);

            if (mesh.Triangles.Count == 0)
            {
                return System.Array.Empty<byte>();
            }

            if (smoothShading == true)
            {
                NormalSmoother.Smooth(mesh);
            }

            return TurntableRenderer.RenderToGif(
                mesh: mesh,
                width: width,
                height: height,
                frameCount: frameCount,
                elevation: elevation,
                frameDelayMs: frameDelayMs,
                renderEdges: renderEdges,
                smoothShading: smoothShading);
        }


        /// <summary>
        /// Render an LDraw file to an SVG vector document.
        /// </summary>
        public string RenderToSvg(string inputPath,
                                   int width = 512,
                                   int height = 512,
                                   int colourCode = -1,
                                   float elevation = 30f,
                                   float azimuth = -45f,
                                   bool renderEdges = true,
                                   bool smoothShading = true)
        {
            EnsureColours();

            int effectiveColour = colourCode >= 0 ? colourCode : 4;

            GeometryResolver resolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh mesh = resolver.ResolveFile(inputPath, effectiveColour);

            if (mesh.Triangles.Count == 0)
            {
                return string.Format("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{0}\" height=\"{1}\"></svg>",
                                     width, height);
            }

            Camera camera = new Camera();
            camera.AutoFrame(mesh, elevation, azimuth);

            return SvgExporter.RenderToSvg(mesh, camera, width, height, renderEdges);
        }


        /// <summary>
        /// Get the number of build steps in an LDraw file.
        /// </summary>
        public int GetStepCount(string inputPath)
        {
            EnsureColours();
            GeometryResolver resolver = new GeometryResolver(_libraryPath, _colours);
            return resolver.GetStepCount(inputPath);
        }


        /// <summary>
        /// Render a single build step of an LDraw file (cumulative — shows all parts up to this step).
        /// Step indices are 0-based.
        /// </summary>
        public byte[] RenderStep(string inputPath,
                                  int stepIndex,
                                  int width = 512,
                                  int height = 512,
                                  int colourCode = -1,
                                  float elevation = 30f,
                                  float azimuth = -45f,
                                  bool renderEdges = true,
                                  bool smoothShading = true,
                                  AntiAliasMode antiAliasMode = AntiAliasMode.None)
        {
            EnsureColours();

            int effectiveColour = colourCode >= 0 ? colourCode : 4;

            GeometryResolver resolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh mesh = resolver.ResolveFileUpToStep(inputPath, stepIndex, effectiveColour);

            if (mesh.Triangles.Count == 0)
            {
                return System.Array.Empty<byte>();
            }

            if (smoothShading)
            {
                NormalSmoother.Smooth(mesh);
            }

            //
            // Render
            //
            int ssaaFactor = 1;
            if (antiAliasMode == AntiAliasMode.SSAA2x) ssaaFactor = 2;
            else if (antiAliasMode == AntiAliasMode.SSAA4x) ssaaFactor = 4;

            int renderW = width * ssaaFactor;
            int renderH = height * ssaaFactor;

            Camera camera = new Camera();
            camera.AutoFrame(mesh, elevation, azimuth);

            SoftwareRenderer renderer = new SoftwareRenderer(renderW, renderH);
            renderer.RenderEdges = renderEdges;
            renderer.SmoothShading = smoothShading;

            byte[] pixels = renderer.Render(mesh, camera);

            if (ssaaFactor > 1)
            {
                pixels = PostProcess.Downsample(pixels, renderW, renderH, width, height);
            }

            return ImageExporter.ToPngBytes(pixels, width, height);
        }


        /// <summary>
        /// Render all build steps as a list of PNG byte arrays.
        /// Each image is cumulative — step 0 shows the first group, step N shows the complete model.
        /// </summary>
        public List<byte[]> RenderAllSteps(string inputPath,
                                            int width = 512,
                                            int height = 512,
                                            int colourCode = -1,
                                            float elevation = 30f,
                                            float azimuth = -45f,
                                            bool renderEdges = true,
                                            bool smoothShading = true)
        {
            int stepCount = GetStepCount(inputPath);
            List<byte[]> results = new List<byte[]>(stepCount);

            for (int i = 0; i < stepCount; i++)
            {
                byte[] stepPng = RenderStep(inputPath, i, width, height,
                                            colourCode, elevation, azimuth,
                                            renderEdges, smoothShading);
                results.Add(stepPng);
            }

            return results;
        }


        /// <summary>
        /// Render an exploded view of an LDraw file.
        /// Parts are pushed radially outward from the model centroid.
        /// </summary>
        /// <param name="explosionFactor">
        /// How much to spread parts.  0 = normal, 1.0 = moderate, 2.0 = wide.
        /// </param>
        public byte[] RenderExplodedView(string inputPath,
                                          float explosionFactor = 1.0f,
                                          int width = 512,
                                          int height = 512,
                                          int colourCode = -1,
                                          float elevation = 30f,
                                          float azimuth = -45f,
                                          bool renderEdges = true,
                                          bool smoothShading = true,
                                          AntiAliasMode antiAliasMode = AntiAliasMode.None)
        {
            EnsureColours();

            int effectiveColour = colourCode >= 0 ? colourCode : 4;

            GeometryResolver resolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh mesh = resolver.ResolveFileWithPartCounts(
                inputPath, effectiveColour,
                out List<int> partTriCounts, out List<int> partEdgeCounts);

            if (mesh.Triangles.Count == 0)
            {
                return System.Array.Empty<byte>();
            }

            //
            // Apply explosion
            //
            mesh = ExplodedViewBuilder.Explode(mesh, partTriCounts, partEdgeCounts, explosionFactor);

            if (smoothShading)
            {
                NormalSmoother.Smooth(mesh);
            }

            //
            // Render
            //
            int ssaaFactor = 1;
            if (antiAliasMode == AntiAliasMode.SSAA2x) ssaaFactor = 2;
            else if (antiAliasMode == AntiAliasMode.SSAA4x) ssaaFactor = 4;

            int renderW = width * ssaaFactor;
            int renderH = height * ssaaFactor;

            Camera camera = new Camera();
            camera.AutoFrame(mesh, elevation, azimuth);

            SoftwareRenderer renderer = new SoftwareRenderer(renderW, renderH);
            renderer.RenderEdges = renderEdges;
            renderer.SmoothShading = smoothShading;

            byte[] pixels = renderer.Render(mesh, camera);

            if (ssaaFactor > 1)
            {
                pixels = PostProcess.Downsample(pixels, renderW, renderH, width, height);
            }

            return ImageExporter.ToPngBytes(pixels, width, height);
        }


        // ════════════════════════════════════════════════════════════════
        //  Content-Based Overloads (accept string[] lines — no file I/O)
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Render LDraw content (lines) to raw RGBA pixels — no temp file needed.
        /// </summary>
        public byte[] RenderToPixels(string[] lines, string fileName,
                                     int width = 512,
                                     int height = 512,
                                     int colourCode = -1,
                                     float elevation = 30f,
                                     float azimuth = -45f,
                                     bool renderEdges = true,
                                     bool smoothShading = true,
                                     AntiAliasMode antiAliasMode = AntiAliasMode.None,
                                     string backgroundHex = null,
                                     string gradientTopHex = null,
                                     string gradientBottomHex = null)
        {
            EnsureColours();

            int effectiveColour = colourCode >= 0 ? colourCode : 4;

            GeometryResolver resolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh mesh = resolver.ResolveFromContent(lines, fileName, effectiveColour);

            if (mesh.Triangles.Count == 0)
            {
                return new byte[width * height * 4];
            }

            if (smoothShading)
            {
                NormalSmoother.Smooth(mesh);
            }

            Camera camera = new Camera();
            camera.AutoFrame(mesh, elevation, azimuth);

            int ssaaFactor = 1;
            if (antiAliasMode == AntiAliasMode.SSAA2x) ssaaFactor = 2;
            else if (antiAliasMode == AntiAliasMode.SSAA4x) ssaaFactor = 4;

            int renderW = width * ssaaFactor;
            int renderH = height * ssaaFactor;

            SoftwareRenderer renderer = new SoftwareRenderer(renderW, renderH);
            renderer.RenderEdges = renderEdges;
            renderer.SmoothShading = smoothShading;

            if (gradientTopHex != null && gradientBottomHex != null)
            {
                ParseHex(gradientTopHex, out byte tr, out byte tg, out byte tb);
                ParseHex(gradientBottomHex, out byte br, out byte bg, out byte bb);
                renderer.SetGradientBackground(tr, tg, tb, br, bg, bb);
            }
            else if (backgroundHex != null)
            {
                ParseHex(backgroundHex, out byte bgr, out byte bgg, out byte bgb);
                renderer.SetBackground(bgr, bgg, bgb, 255);
            }

            byte[] pixels = renderer.Render(mesh, camera);

            if (ssaaFactor > 1)
            {
                pixels = PostProcess.Downsample(pixels, renderW, renderH, width, height);
            }

            return pixels;
        }


        /// <summary>
        /// Render LDraw content to PNG bytes — no temp file needed.
        /// </summary>
        public byte[] RenderToPng(string[] lines, string fileName,
                                  int width = 512,
                                  int height = 512,
                                  int colourCode = -1,
                                  float elevation = 30f,
                                  float azimuth = -45f,
                                  bool renderEdges = true,
                                  bool smoothShading = true,
                                  AntiAliasMode antiAliasMode = AntiAliasMode.None,
                                  string backgroundHex = null,
                                  string gradientTopHex = null,
                                  string gradientBottomHex = null)
        {
            byte[] pixels = RenderToPixels(lines: lines,
                                           fileName: fileName,
                                           width: width,
                                           height: height,
                                           colourCode: colourCode,
                                           elevation: elevation,
                                           azimuth: azimuth,
                                           renderEdges: renderEdges,
                                           smoothShading: smoothShading,
                                           antiAliasMode: antiAliasMode,
                                           backgroundHex: backgroundHex,
                                           gradientTopHex: gradientTopHex,
                                           gradientBottomHex: gradientBottomHex);

            return ImageExporter.ToPngBytes(pixels, width, height);
        }


        /// <summary>
        /// Render LDraw content to WebP bytes — no temp file needed.
        /// </summary>
        public byte[] RenderToWebP(string[] lines, string fileName,
                                   int width = 512,
                                   int height = 512,
                                   int colourCode = -1,
                                   float elevation = 30f,
                                   float azimuth = -45f,
                                   bool renderEdges = true,
                                   bool smoothShading = true,
                                   AntiAliasMode antiAliasMode = AntiAliasMode.None,
                                   string backgroundHex = null,
                                   string gradientTopHex = null,
                                   string gradientBottomHex = null,
                                   int quality = 90)
        {
            byte[] pixels = RenderToPixels(lines: lines,
                                           fileName: fileName,
                                           width: width,
                                           height: height,
                                           colourCode: colourCode,
                                           elevation: elevation,
                                           azimuth: azimuth,
                                           renderEdges: renderEdges,
                                           smoothShading: smoothShading,
                                           antiAliasMode: antiAliasMode,
                                           backgroundHex: backgroundHex,
                                           gradientTopHex: gradientTopHex,
                                           gradientBottomHex: gradientBottomHex);

            return ImageExporter.ToWebPBytes(pixels, width, height, quality);
        }


        /// <summary>
        /// Render LDraw content to SVG — no temp file needed.
        /// </summary>
        public string RenderToSvg(string[] lines, string fileName,
                                   int width = 512,
                                   int height = 512,
                                   int colourCode = -1,
                                   float elevation = 30f,
                                   float azimuth = -45f,
                                   bool renderEdges = true,
                                   bool smoothShading = true)
        {
            EnsureColours();

            int effectiveColour = colourCode >= 0 ? colourCode : 4;

            GeometryResolver resolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh mesh = resolver.ResolveFromContent(lines, fileName, effectiveColour);

            if (mesh.Triangles.Count == 0)
            {
                return string.Format("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{0}\" height=\"{1}\"></svg>",
                                     width, height);
            }

            Camera camera = new Camera();
            camera.AutoFrame(mesh, elevation, azimuth);

            return SvgExporter.RenderToSvg(mesh, camera, width, height, renderEdges);
        }


        /// <summary>
        /// Render LDraw content as animated turntable GIF — no temp file needed.
        /// </summary>
        public byte[] RenderTurntableGif(string[] lines, string fileName,
                                          int width = 256,
                                          int height = 256,
                                          int colourCode = -1,
                                          int frameCount = 36,
                                          float elevation = 30f,
                                          int frameDelayMs = 80,
                                          bool renderEdges = true,
                                          bool smoothShading = true)
        {
            EnsureColours();

            int effectiveColour = colourCode >= 0 ? colourCode : 4;

            GeometryResolver resolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh mesh = resolver.ResolveFromContent(lines, fileName, effectiveColour);

            if (mesh.Triangles.Count == 0)
            {
                return System.Array.Empty<byte>();
            }

            if (smoothShading)
            {
                NormalSmoother.Smooth(mesh);
            }

            return TurntableRenderer.RenderToGif(
                mesh: mesh,
                width: width,
                height: height,
                frameCount: frameCount,
                elevation: elevation,
                frameDelayMs: frameDelayMs,
                renderEdges: renderEdges,
                smoothShading: smoothShading);
        }


        /// <summary>
        /// Render an exploded view of LDraw content — no temp file needed.
        /// </summary>
        public byte[] RenderExplodedView(string[] lines, string fileName,
                                          float explosionFactor = 1.0f,
                                          int width = 512,
                                          int height = 512,
                                          int colourCode = -1,
                                          float elevation = 30f,
                                          float azimuth = -45f,
                                          bool renderEdges = true,
                                          bool smoothShading = true,
                                          AntiAliasMode antiAliasMode = AntiAliasMode.None)
        {
            EnsureColours();

            int effectiveColour = colourCode >= 0 ? colourCode : 4;

            GeometryResolver resolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh mesh = resolver.ResolveFromContentWithPartCounts(
                lines, fileName, effectiveColour,
                out List<int> partTriCounts, out List<int> partEdgeCounts);

            if (mesh.Triangles.Count == 0)
            {
                return System.Array.Empty<byte>();
            }

            mesh = ExplodedViewBuilder.Explode(mesh, partTriCounts, partEdgeCounts, explosionFactor);

            if (smoothShading)
            {
                NormalSmoother.Smooth(mesh);
            }

            int ssaaFactor = 1;
            if (antiAliasMode == AntiAliasMode.SSAA2x) ssaaFactor = 2;
            else if (antiAliasMode == AntiAliasMode.SSAA4x) ssaaFactor = 4;

            int renderW = width * ssaaFactor;
            int renderH = height * ssaaFactor;

            Camera camera = new Camera();
            camera.AutoFrame(mesh, elevation, azimuth);

            SoftwareRenderer renderer = new SoftwareRenderer(renderW, renderH);
            renderer.RenderEdges = renderEdges;
            renderer.SmoothShading = smoothShading;

            byte[] pixels = renderer.Render(mesh, camera);

            if (ssaaFactor > 1)
            {
                pixels = PostProcess.Downsample(pixels, renderW, renderH, width, height);
            }

            return ImageExporter.ToPngBytes(pixels, width, height);
        }


        /// <summary>
        /// Parse a hex colour string like "#DFC176" into RGB bytes.
        /// </summary>
        private static void ParseHex(string hex, out byte r, out byte g, out byte b)
        {
            r = 0; g = 0; b = 0;
            if (hex == null || hex.Length < 7 || hex[0] != '#') return;

            byte.TryParse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber,
                          System.Globalization.CultureInfo.InvariantCulture, out r);
            byte.TryParse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber,
                          System.Globalization.CultureInfo.InvariantCulture, out g);
            byte.TryParse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber,
                          System.Globalization.CultureInfo.InvariantCulture, out b);
        }


        /// <summary>
        /// Load and cache the LDraw colour table.
        /// </summary>
        private void EnsureColours()
        {
            if (_colours != null)
            {
                return;
            }

            string configPath = System.IO.Path.Combine(_libraryPath, "LDConfig.ldr");

            if (System.IO.File.Exists(configPath) == true)
            {
                _colours = ColourConfigParser.ParseFile(configPath);
            }
            else
            {
                _colours = new List<LDrawColour>();
            }
        }
    }
}

