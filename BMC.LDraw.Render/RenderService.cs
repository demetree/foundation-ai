using System;
using System.Collections.Generic;
using BMC.LDraw.Models;
using BMC.LDraw.Parsers;
using Foundation.Imaging;
using Foundation.Imaging.Processing;

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

        /// <summary>Root path of the LDraw library (for part file lookups).</summary>
        public string LibraryPath => _libraryPath;


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
                                 string gradientBottomHex = null,
                                 float zoom = 1.0f)
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
                                           gradientBottomHex: gradientBottomHex,
                                           zoom: zoom);

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
                                  string gradientBottomHex = null,
                                  RendererType rendererType = RendererType.Rasterizer,
                                  bool enablePbr = true,
                                  float exposure = 1.0f,
                                  float aperture = 0f,
                                  float zoom = 1.0f)
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
                                           gradientBottomHex: gradientBottomHex,
                                           rendererType: rendererType,
                                           enablePbr: enablePbr,
                                           exposure: exposure,
                                           aperture: aperture,
                                           zoom: zoom);

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
                                   int quality = 90,
                                   bool enablePbr = true,
                                   float exposure = 1.0f,
                                   float aperture = 0f,
                                   float zoom = 1.0f)
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
                                           gradientBottomHex: gradientBottomHex,
                                           enablePbr: enablePbr,
                                           exposure: exposure,
                                           aperture: aperture,
                                           zoom: zoom);

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
                                     string gradientBottomHex = null,
                                     RendererType rendererType = RendererType.Rasterizer,
                                     bool enablePbr = true,
                                     float exposure = 1.0f,
                                     float aperture = 0f,
                                     float zoom = 1.0f)
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
            // Set up camera with FOV/aspect-aware framing
            //
            Camera camera = new Camera();
            float aspect = (float)width / Math.Max(height, 1);
            camera.AutoFrame(mesh, elevation, azimuth, aspect, zoom);

            //
            // Determine render resolution (SSAA renders at a higher internal resolution)
            //
            int ssaaFactor = 1;
            if (antiAliasMode == AntiAliasMode.SSAA2x) ssaaFactor = 2;
            else if (antiAliasMode == AntiAliasMode.SSAA4x) ssaaFactor = 4;

            int renderW = width * ssaaFactor;
            int renderH = height * ssaaFactor;

            //
            // Create renderer via IRenderer interface
            //
            IRenderer renderer;

            if (rendererType == RendererType.RayTracer)
            {
                var rt = new RayTracing.RayTraceRenderer(renderW, renderH);
                rt.Environment = new RayTracing.ProceduralSky();
                rt.Lighting = LightingModel.Studio();
                rt.EnablePbr = enablePbr;
                rt.Exposure = exposure;
                camera.Aperture = aperture;
                renderer = rt;
            }
            else
            {
                SoftwareRenderer rasterizer = new SoftwareRenderer(renderW, renderH);
                rasterizer.RenderEdges = renderEdges;
                rasterizer.SmoothShading = smoothShading;
                renderer = rasterizer;
            }

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
            float aspect = (float)width / Math.Max(height, 1);
            camera.AutoFrame(mesh, elevation, azimuth, aspect, 1f);

            return SvgExporter.RenderToSvg(mesh, camera, width, height, renderEdges);
        }


        /// <summary>
        /// Export an LDraw file to STL (stereolithography) bytes.
        /// Supports both binary (compact) and ASCII (human-readable) formats.
        ///
        /// AI-generated — Mar 2026.
        /// </summary>
        /// <param name="inputPath">Path to the .ldr, .mpd, or .dat file.</param>
        /// <param name="colourCode">LDraw colour code for the part.  -1 uses the default (red).</param>
        /// <param name="ascii">When true, returns UTF-8 encoded ASCII STL; otherwise binary STL.</param>
        /// <returns>Complete STL file as a byte array.</returns>
        public byte[] ExportToStl(string inputPath,
                                   int colourCode = -1,
                                   bool ascii = false)
        {
            EnsureColours();

            int effectiveColour = colourCode >= 0 ? colourCode : 4;

            GeometryResolver resolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh mesh = resolver.ResolveFile(inputPath, effectiveColour);

            if (mesh.Triangles.Count == 0)
            {
                return System.Array.Empty<byte>();
            }

            if (ascii == true)
            {
                string stlText = StlExporter.ExportAscii(mesh);
                return System.Text.Encoding.UTF8.GetBytes(stlText);
            }

            return StlExporter.ExportBinary(mesh);
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

            // Resolve the FULL model for consistent camera framing across all steps
            GeometryResolver fullResolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh fullMesh = fullResolver.ResolveFile(inputPath, effectiveColour);

            // Resolve the step mesh for actual rendering
            GeometryResolver stepResolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh mesh = stepResolver.ResolveFileUpToStep(inputPath, stepIndex, effectiveColour);

            if (mesh.Triangles.Count == 0)
            {
                return System.Array.Empty<byte>();
            }

            if (smoothShading)
            {
                NormalSmoother.Smooth(mesh);
            }

            int ssaaFactor = 1;
            if (antiAliasMode == AntiAliasMode.SSAA2x) ssaaFactor = 2;
            else if (antiAliasMode == AntiAliasMode.SSAA4x) ssaaFactor = 4;

            int renderW = width * ssaaFactor;
            int renderH = height * ssaaFactor;

            // Frame camera using hybrid step framing (step center, clamped zoom)
            Camera camera = new Camera();
            camera.AutoFrameStep(mesh, fullMesh, elevation, azimuth);

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
        /// Get the number of build steps from file content (for uploaded files).
        /// </summary>
        public int GetStepCount(string[] lines, string fileName)
        {
            EnsureColours();
            GeometryResolver resolver = new GeometryResolver(_libraryPath, _colours);
            return resolver.GetStepCountFromContent(lines, fileName);
        }

        /// <summary>
        /// Debug method: returns step resolution diagnostics for the first N steps.
        /// </summary>
        public List<Dictionary<string, object>> DebugSteps(string[] lines, string fileName, int maxSteps = 25)
        {
            EnsureColours();

            var geos = GeometryParser.ParseLines(lines, fileName);
            if (geos.Count == 0) return new List<Dictionary<string, object>>();

            var root = geos[0];
            var results = new List<Dictionary<string, object>>();
            int count = Math.Min(root.StepBreaks.Count, maxSteps);

            for (int i = 0; i < count; i++)
            {
                var resolver = new GeometryResolver(_libraryPath, _colours);
                var mesh = resolver.ResolveContentUpToStep(lines, fileName, i, 4);
                mesh.GetCenter(out float cx, out float cy, out float cz);

                results.Add(new Dictionary<string, object>
                {
                    ["step"] = i,
                    ["maxSubRef"] = root.StepBreaks[i],
                    ["triangleCount"] = mesh.Triangles.Count,
                    ["edgeLineCount"] = mesh.EdgeLines.Count,
                    ["extent"] = mesh.GetMaxExtent(),
                    ["centerX"] = cx,
                    ["centerY"] = cy,
                    ["centerZ"] = cz
                });
            }

            return results;
        }


        /// <summary>
        /// Render a single build step from file content (for uploaded files).
        /// </summary>
        public byte[] RenderStep(string[] lines,
                                  string fileName,
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

            // Resolve the FULL model for consistent camera framing across all steps
            GeometryResolver fullResolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh fullMesh = fullResolver.ResolveContentUpToStep(lines, fileName, int.MaxValue, effectiveColour);

            // Resolve the step mesh for actual rendering
            GeometryResolver stepResolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh mesh = stepResolver.ResolveContentUpToStep(lines, fileName, stepIndex, effectiveColour);

            if (mesh.Triangles.Count == 0)
            {
                return System.Array.Empty<byte>();
            }

            if (smoothShading)
            {
                NormalSmoother.Smooth(mesh);
            }

            int ssaaFactor = 1;
            if (antiAliasMode == AntiAliasMode.SSAA2x) ssaaFactor = 2;
            else if (antiAliasMode == AntiAliasMode.SSAA4x) ssaaFactor = 4;

            int renderW = width * ssaaFactor;
            int renderH = height * ssaaFactor;

            // Frame camera using hybrid step framing (step center, clamped zoom)
            Camera camera = new Camera();
            camera.AutoFrameStep(mesh, fullMesh, elevation, azimuth);

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
                                     string gradientBottomHex = null,
                                     RendererType rendererType = RendererType.Rasterizer,
                                     bool enablePbr = true,
                                     float exposure = 1.0f,
                                     float aperture = 0f,
                                     float zoom = 1.0f)
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
            float aspect = (float)width / Math.Max(height, 1);
            camera.AutoFrame(mesh, elevation, azimuth, aspect, zoom);

            int ssaaFactor = 1;
            if (antiAliasMode == AntiAliasMode.SSAA2x) ssaaFactor = 2;
            else if (antiAliasMode == AntiAliasMode.SSAA4x) ssaaFactor = 4;

            int renderW = width * ssaaFactor;
            int renderH = height * ssaaFactor;

            IRenderer renderer;

            if (rendererType == RendererType.RayTracer)
            {
                var rt = new RayTracing.RayTraceRenderer(renderW, renderH);
                rt.Environment = new RayTracing.ProceduralSky();
                rt.Lighting = LightingModel.Studio();
                rt.EnablePbr = enablePbr;
                rt.Exposure = exposure;
                camera.Aperture = aperture;
                renderer = rt;
            }
            else
            {
                SoftwareRenderer rasterizer = new SoftwareRenderer(renderW, renderH);
                rasterizer.RenderEdges = renderEdges;
                rasterizer.SmoothShading = smoothShading;
                renderer = rasterizer;
            }

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
                                  string gradientBottomHex = null,
                                  RendererType rendererType = RendererType.Rasterizer,
                                  bool enablePbr = true,
                                  float exposure = 1.0f,
                                  float aperture = 0f,
                                  float zoom = 1.0f)
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
                                           gradientBottomHex: gradientBottomHex,
                                           rendererType: rendererType,
                                           enablePbr: enablePbr,
                                           exposure: exposure,
                                           aperture: aperture,
                                           zoom: zoom);

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
                                   int quality = 90,
                                   bool enablePbr = true,
                                   float exposure = 1.0f,
                                   float aperture = 0f)
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
                                           gradientBottomHex: gradientBottomHex,
                                           enablePbr: enablePbr,
                                           exposure: exposure,
                                           aperture: aperture);

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
        /// Export LDraw content to STL bytes — no temp file needed.
        /// Supports both binary (compact) and ASCII (human-readable) formats.
        ///
        /// AI-generated — Mar 2026.
        /// </summary>
        /// <param name="lines">LDraw file content as lines.</param>
        /// <param name="fileName">Original filename (used for multi-part document lookup).</param>
        /// <param name="colourCode">LDraw colour code.  -1 uses the default (red).</param>
        /// <param name="ascii">When true, returns UTF-8 encoded ASCII STL; otherwise binary STL.</param>
        /// <returns>Complete STL file as a byte array.</returns>
        public byte[] ExportToStl(string[] lines, string fileName,
                                   int colourCode = -1,
                                   bool ascii = false)
        {
            EnsureColours();

            int effectiveColour = colourCode >= 0 ? colourCode : 4;

            GeometryResolver resolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh mesh = resolver.ResolveFromContent(lines, fileName, effectiveColour);

            if (mesh.Triangles.Count == 0)
            {
                return System.Array.Empty<byte>();
            }

            if (ascii == true)
            {
                string stlText = StlExporter.ExportAscii(mesh);
                return System.Text.Encoding.UTF8.GetBytes(stlText);
            }

            return StlExporter.ExportBinary(mesh);
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


        // ════════════════════════════════════════════════════════════════
        //  Manual Generation — Step Diff Analysis & Highlighted Rendering
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Analyse an uploaded LDraw file and return per-step metadata:
        /// which parts are new at each step and cumulative counts.
        /// This is a FAST metadata-only pass — no geometry resolution is performed.
        /// </summary>
        public List<ManualStepAnalysis> AnalyseSteps(string[] lines, string fileName)
        {
            EnsureColours();

            var geos = GeometryParser.ParseLines(lines, fileName);
            if (geos.Count == 0) return new List<ManualStepAnalysis>();

            var root = geos[0];
            int stepCount = root.StepBreaks.Count;
            if (stepCount == 0) return new List<ManualStepAnalysis>();

            var results = new List<ManualStepAnalysis>(stepCount);
            int cumulativeParts = 0;

            for (int i = 0; i < stepCount; i++)
            {
                int prevMax = i > 0 ? root.StepBreaks[i - 1] : 0;
                int curMax = root.StepBreaks[i];

                // Identify new subfile references in this step
                var partGroups = new Dictionary<string, StepPartInfo>();
                for (int j = prevMax; j < curMax && j < root.SubfileReferences.Count; j++)
                {
                    var subRef = root.SubfileReferences[j];
                    string key = subRef.FileName + "|" + subRef.ColourCode;

                    if (partGroups.TryGetValue(key, out StepPartInfo existing))
                    {
                        existing.Quantity++;
                    }
                    else
                    {
                        partGroups[key] = new StepPartInfo
                        {
                            FileName = subRef.FileName,
                            ColourCode = subRef.ColourCode,
                            Quantity = 1
                        };
                    }
                }

                cumulativeParts += (curMax - prevMax);

                var analysis = new ManualStepAnalysis
                {
                    StepIndex = i,
                    NewParts = new List<StepPartInfo>(partGroups.Values),
                    CumulativePartCount = cumulativeParts,
                    CumulativeTriangleCount = 0 // populated during render phase if needed
                };

                results.Add(analysis);
            }

            return results;
        }


        /// <summary>
        /// Recursively analyse all steps across root model and submodels.
        /// Returns a flattened ManualBuildPlan in render order — submodel steps
        /// appear inline before the root step that references them.
        /// 
        /// This is a metadata-only pass; no geometry resolution is performed.
        /// </summary>
        public ManualBuildPlan AnalyseStepsRecursive(string[] lines, string fileName)
        {
            EnsureColours();

            var geos = GeometryParser.ParseLines(lines, fileName);
            if (geos.Count == 0)
                return new ManualBuildPlan { ModelName = System.IO.Path.GetFileNameWithoutExtension(fileName) };

            // Build a lookup of all parsed geometries by name
            var geoLookup = new Dictionary<string, LDrawGeometry>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < geos.Count; i++)
            {
                if (geos[i].Name != null && !geoLookup.ContainsKey(geos[i].Name))
                {
                    geoLookup[geos[i].Name] = geos[i];
                }
            }

            var root = geos[0];
            var plan = new ManualBuildPlan
            {
                ModelName = System.IO.Path.GetFileNameWithoutExtension(fileName)
            };

            // Track which submodels have steps (for pre-resolution)
            var submodelsWithSteps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Track already-expanded submodels to avoid duplicating callouts
            var expandedSubmodels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int globalStep = 1;

            ExpandStepsRecursive(
                root, root.Name ?? fileName,
                null, // parent model name
                0,    // depth
                geoLookup, submodelsWithSteps, expandedSubmodels,
                plan.Steps, ref globalStep);

            plan.SubmodelsWithSteps = new List<string>(submodelsWithSteps);
            return plan;
        }


        /// <summary>
        /// Recursively expand steps from a geometry, inserting submodel callout steps
        /// before the root step that first references each submodel.
        /// </summary>
        private void ExpandStepsRecursive(
            LDrawGeometry geo,
            string modelName,
            string parentModelName,
            int depth,
            Dictionary<string, LDrawGeometry> geoLookup,
            HashSet<string> submodelsWithSteps,
            HashSet<string> expandedSubmodels,
            List<ManualBuildStep> steps,
            ref int globalStep)
        {
            int stepCount = geo.StepBreaks.Count;
            if (stepCount == 0) return;

            bool isSubmodel = depth > 0;
            int cumulativeParts = 0;

            for (int i = 0; i < stepCount; i++)
            {
                int prevMax = i > 0 ? geo.StepBreaks[i - 1] : 0;
                int curMax = geo.StepBreaks[i];

                // Before emitting this step, check if any subfile refs in this step
                // point to submodels with their own build steps.
                // If so, expand them first (callout).
                for (int j = prevMax; j < curMax && j < geo.SubfileReferences.Count; j++)
                {
                    var subRef = geo.SubfileReferences[j];
                    if (geoLookup.TryGetValue(subRef.FileName, out LDrawGeometry subGeo)
                        && subGeo.StepBreaks.Count > 1  // only expand multi-step submodels
                        && !expandedSubmodels.Contains(subRef.FileName))
                    {
                        expandedSubmodels.Add(subRef.FileName);
                        submodelsWithSteps.Add(subRef.FileName);

                        ExpandStepsRecursive(
                            subGeo, subRef.FileName,
                            modelName,
                            depth + 1,
                            geoLookup, submodelsWithSteps, expandedSubmodels,
                            steps, ref globalStep);
                    }
                }

                // Now emit this step
                var partGroups = new Dictionary<string, StepPartInfo>();
                for (int j = prevMax; j < curMax && j < geo.SubfileReferences.Count; j++)
                {
                    var subRef = geo.SubfileReferences[j];
                    string key = subRef.FileName + "|" + subRef.ColourCode;

                    if (partGroups.TryGetValue(key, out StepPartInfo existing))
                    {
                        existing.Quantity++;
                    }
                    else
                    {
                        partGroups[key] = new StepPartInfo
                        {
                            FileName = subRef.FileName,
                            ColourCode = subRef.ColourCode,
                            Quantity = 1
                        };
                    }
                }

                cumulativeParts += (curMax - prevMax);

                // Get ROTSTEP data if available
                RotStepData rotStep = null;
                if (i < geo.StepRotations.Count && geo.StepRotations[i] != null)
                {
                    var src = geo.StepRotations[i];
                    rotStep = new RotStepData
                    {
                        X = src.X, Y = src.Y, Z = src.Z, Type = src.Type
                    };
                }

                steps.Add(new ManualBuildStep
                {
                    GlobalStepIndex = globalStep++,
                    LocalStepIndex = i,
                    ModelName = modelName,
                    IsSubmodelStep = isSubmodel,
                    ParentModelName = parentModelName,
                    SubmodelDepth = depth,
                    NewParts = new List<StepPartInfo>(partGroups.Values),
                    CumulativePartCount = cumulativeParts,
                    RotStep = rotStep
                });
            }
        }


        /// <summary>
        /// Resolve a single submodel's mesh with per-step triangle/edge boundaries.
        /// Used for rendering submodel steps with the pre-smoothed mesh approach.
        /// 
        /// Parses and caches all MPD sections so submodel part references resolve correctly.
        /// </summary>
        public (LDrawMesh mesh, int[] triBounds, int[] edgeBounds)
            ResolveSubmodelWithStepBoundaries(
                string[] lines, string fileName, string submodelName,
                int effectiveColour = 4)
        {
            EnsureColours();

            var geos = GeometryParser.ParseLines(lines, fileName);

            // Find the target submodel geometry
            LDrawGeometry targetGeo = null;
            for (int i = 0; i < geos.Count; i++)
            {
                if (string.Equals(geos[i].Name, submodelName, System.StringComparison.OrdinalIgnoreCase))
                {
                    targetGeo = geos[i];
                    break;
                }
            }

            if (targetGeo == null || targetGeo.StepBreaks.Count == 0)
            {
                return (new LDrawMesh(), System.Array.Empty<int>(), System.Array.Empty<int>());
            }

            // Build a resolver with all MPD sections cached
            GeometryResolver subResolver = new GeometryResolver(_libraryPath, _colours);
            subResolver.PrepareContentForIncrementalResolve(lines, fileName);

            // Build the mesh step by step
            LDrawMesh mesh = new LDrawMesh();
            int totalSteps = targetGeo.StepBreaks.Count;
            int[] triBounds = new int[totalSteps];
            int[] edgeBounds = new int[totalSteps];

            int prevSubRefEnd = 0;
            for (int step = 0; step < totalSteps; step++)
            {
                int subRefEnd = targetGeo.StepBreaks[step];

                subResolver.ResolveContentStepRange(
                    targetGeo, prevSubRefEnd, subRefEnd,
                    effectiveColour, mesh,
                    includeDirectGeometry: step == 0);

                triBounds[step] = mesh.Triangles.Count;
                edgeBounds[step] = mesh.EdgeLines.Count;
                prevSubRefEnd = subRefEnd;
            }

            mesh.ComputeBounds();
            return (mesh, triBounds, edgeBounds);
        }

        /// <summary>
        /// Pre-resolve the full model mesh (all steps) for camera framing purposes.
        /// Also returns the shared GeometryResolver and parsed root geometry for
        /// use with incremental step rendering.
        /// </summary>
        public LDrawMesh ResolveFullModel(string[] lines, string fileName,
            out GeometryResolver sharedResolver, out LDrawGeometry rootGeometry,
            int effectiveColour = 4)
        {
            EnsureColours();
            sharedResolver = new GeometryResolver(_libraryPath, _colours);
            rootGeometry = sharedResolver.PrepareContentForIncrementalResolve(lines, fileName);

            if (rootGeometry == null) return new LDrawMesh();

            // Resolve the full model using the traditional path (for camera framing)
            return sharedResolver.ResolveContentUpToStep(lines, fileName, int.MaxValue, effectiveColour);
        }

        /// <summary>
        /// Simple overload for callers that don't need incremental resolution.
        /// </summary>
        public LDrawMesh ResolveFullModel(string[] lines, string fileName, int effectiveColour = 4)
        {
            EnsureColours();
            GeometryResolver fullResolver = new GeometryResolver(_libraryPath, _colours);
            return fullResolver.ResolveContentUpToStep(lines, fileName, int.MaxValue, effectiveColour);
        }


        /// <summary>
        /// Resolve the full model with per-step triangle/edge boundaries.
        /// Returns a mesh suitable for pre-smoothing and per-step slicing.
        ///
        /// stepTriangleBounds[i] = cumulative triangle count AFTER step i.
        /// stepEdgeBounds[i]     = cumulative edge count AFTER step i.
        /// </summary>
        public LDrawMesh ResolveFullModelWithStepBoundaries(
            string[] lines, string fileName,
            out int[] stepTriangleBounds, out int[] stepEdgeBounds,
            int effectiveColour = 4)
        {
            EnsureColours();
            GeometryResolver resolver = new GeometryResolver(_libraryPath, _colours);
            return resolver.ResolveContentAllWithStepBoundaries(
                lines, fileName, effectiveColour,
                out stepTriangleBounds, out stepEdgeBounds);
        }


        /// <summary>
        /// Incremental overload: render a step by resolving ONLY the delta subfile
        /// references (from the previous step boundary to the current one) and
        /// appending to an accumulating mesh.
        ///
        /// The caller must maintain:
        /// - sharedResolver: a single GeometryResolver reused across all steps (preserves file cache)
        /// - rootGeometry: the parsed root from PrepareContentForIncrementalResolve
        /// - accumulatingMesh: the mesh that grows incrementally step-by-step
        /// - prevTriCount/prevEdgeCount: counts BEFORE this step's delta was added
        ///
        /// Returns a StepRenderResult with the rendered PNG and the mesh counts
        /// AFTER this step (for the caller to cache as prevTriCount/prevEdgeCount
        /// for the next step).
        /// </summary>
        public StepRenderResult RenderStepHighlightedIncremental(
            GeometryResolver sharedResolver,
            LDrawGeometry rootGeometry,
            LDrawMesh accumulatingMesh,
            int stepIndex,
            int width, int height,
            float elevation, float azimuth,
            bool renderEdges, bool smoothShading,
            LDrawMesh precomputedFullMesh,
            int prevTriCount, int prevEdgeCount)
        {
            int effectiveColour = 4;

            // Determine the subfile reference range for this step
            int fromSubRef = stepIndex > 0 ? rootGeometry.StepBreaks[stepIndex - 1] : 0;
            int toSubRef = rootGeometry.StepBreaks[stepIndex];

            // Resolve ONLY the new delta subfile references, appending to the accumulating mesh
            sharedResolver.ResolveContentStepRange(
                rootGeometry, fromSubRef, toSubRef, effectiveColour, accumulatingMesh,
                includeDirectGeometry: stepIndex == 0);

            if (accumulatingMesh.Triangles.Count == 0)
            {
                return new StepRenderResult
                {
                    PngBytes = System.Array.Empty<byte>(),
                    TriangleCount = 0,
                    EdgeCount = 0
                };
            }

            // Capture counts AFTER delta resolution (for caller to cache)
            int currentTriCount = accumulatingMesh.Triangles.Count;
            int currentEdgeCount = accumulatingMesh.EdgeLines.Count;

            // Create a render copy — we need to dim old triangles without
            // mutating the accumulating mesh (which is reused next step)
            var renderTris = new List<MeshTriangle>(accumulatingMesh.Triangles);
            var renderEdgeLines = new List<MeshLine>(accumulatingMesh.EdgeLines);

            // Dim old triangles to ~40% opacity
            for (int t = 0; t < prevTriCount && t < renderTris.Count; t++)
            {
                MeshTriangle tri = renderTris[t];
                byte grey = 180;
                tri.R = (byte)((tri.R * 0.4f) + (grey * 0.6f));
                tri.G = (byte)((tri.G * 0.4f) + (grey * 0.6f));
                tri.B = (byte)((tri.B * 0.4f) + (grey * 0.6f));
                tri.A = 255; // Keep opaque! A<255 routes through sequential transparent pass
                renderTris[t] = tri;
            }

            for (int e = 0; e < prevEdgeCount && e < renderEdgeLines.Count; e++)
            {
                MeshLine line = renderEdgeLines[e];
                line.R = 200; line.G = 200; line.B = 200; line.A = 80;
                renderEdgeLines[e] = line;
            }

            // Build a render-only mesh with the dimmed copies
            var renderMesh = new LDrawMesh();
            renderMesh.Triangles = renderTris;
            renderMesh.EdgeLines = renderEdgeLines;
            renderMesh.MinX = accumulatingMesh.MinX; renderMesh.MinY = accumulatingMesh.MinY; renderMesh.MinZ = accumulatingMesh.MinZ;
            renderMesh.MaxX = accumulatingMesh.MaxX; renderMesh.MaxY = accumulatingMesh.MaxY; renderMesh.MaxZ = accumulatingMesh.MaxZ;

            if (smoothShading)
            {
                NormalSmoother.Smooth(renderMesh);
            }

            Camera camera = new Camera();
            camera.AutoFrameStep(renderMesh, precomputedFullMesh, elevation, azimuth);

            SoftwareRenderer renderer = new SoftwareRenderer(width, height);
            renderer.RenderEdges = renderEdges;
            renderer.SmoothShading = smoothShading;

            byte[] pixels = renderer.Render(renderMesh, camera);
            byte[] png = ImageExporter.ToPngBytes(pixels, width, height);

            return new StepRenderResult
            {
                PngBytes = png,
                TriangleCount = currentTriCount,
                EdgeCount = currentEdgeCount
            };
        }


        /// <summary>
        /// Render a step using a pre-resolved, pre-smoothed full mesh.
        ///
        /// Per-step cost is O(totalTrisUpToStep) for the copy + dim,
        /// but crucially does NO geometry resolution and NO normal smoothing.
        /// The heavy O(T²) work (resolve + smooth) is done once up front.
        ///
        /// stepTriangleBounds/stepEdgeBounds: cumulative counts from
        /// GeometryResolver.ResolveContentAllWithStepBoundaries.
        /// </summary>
        public StepRenderResult RenderStepFromPreSmoothedMesh(
            LDrawMesh preSmoothedFullMesh,
            int stepIndex,
            int[] stepTriangleBounds,
            int[] stepEdgeBounds,
            int width, int height,
            float elevation, float azimuth,
            bool renderEdges, bool smoothShading,
            RendererType rendererType = RendererType.Rasterizer)
        {
            // Determine tri/edge range for this step
            int triEnd = stepTriangleBounds[stepIndex];
            int edgeEnd = stepEdgeBounds[stepIndex];
            int prevTriEnd = stepIndex > 0 ? stepTriangleBounds[stepIndex - 1] : 0;
            int prevEdgeEnd = stepIndex > 0 ? stepEdgeBounds[stepIndex - 1] : 0;

            if (triEnd == 0)
            {
                return new StepRenderResult
                {
                    PngBytes = System.Array.Empty<byte>(),
                    TriangleCount = 0,
                    EdgeCount = 0
                };
            }

            // Build render mesh by copying pre-smoothed tris up to this step
            var renderTris = new List<MeshTriangle>(triEnd);
            for (int t = 0; t < triEnd; t++)
            {
                renderTris.Add(preSmoothedFullMesh.Triangles[t]);
            }

            var renderEdgeLines = new List<MeshLine>(edgeEnd);
            for (int e = 0; e < edgeEnd; e++)
            {
                renderEdgeLines.Add(preSmoothedFullMesh.EdgeLines[e]);
            }

            // Dim old steps — push strongly toward light grey so new parts pop.
            // Keep A=255 so they stay in the fast parallel opaque path.
            // Using high grey + high blend factor makes old geometry nearly ghosted,
            // providing maximum contrast even for white-on-white new parts.
            for (int t = 0; t < prevTriEnd; t++)
            {
                MeshTriangle tri = renderTris[t];
                byte grey = 210;                // Light grey target
                tri.R = (byte)((tri.R * 0.2f) + (grey * 0.8f));
                tri.G = (byte)((tri.G * 0.2f) + (grey * 0.8f));
                tri.B = (byte)((tri.B * 0.2f) + (grey * 0.8f));
                tri.A = 255;
                renderTris[t] = tri;
            }

            for (int e = 0; e < prevEdgeEnd; e++)
            {
                MeshLine line = renderEdgeLines[e];
                line.R = 220; line.G = 220; line.B = 220; line.A = 50;
                renderEdgeLines[e] = line;
            }

            // Highlight new-step edges with solid dark outlines.
            // This makes new parts pop regardless of their surface colour,
            // similar to bold outlines in official LEGO instructions.
            for (int e = prevEdgeEnd; e < edgeEnd; e++)
            {
                MeshLine line = renderEdgeLines[e];
                line.R = 50; line.G = 50; line.B = 50; line.A = 255;
                renderEdgeLines[e] = line;
            }

            // Build the render-only mesh (normals already present from pre-smoothing)
            var renderMesh = new LDrawMesh();
            renderMesh.Triangles = renderTris;
            renderMesh.EdgeLines = renderEdgeLines;
            renderMesh.ComputeBounds();

            // Build a focused mesh from ONLY the new-step triangles for tighter camera framing.
            // This prevents the camera from zooming out to fit the entire accumulated model.
            var stepOnlyTris = new List<MeshTriangle>(triEnd - prevTriEnd);
            for (int t = prevTriEnd; t < triEnd; t++)
            {
                stepOnlyTris.Add(renderTris[t]);
            }
            var stepOnlyMesh = new LDrawMesh();
            stepOnlyMesh.Triangles = stepOnlyTris;
            stepOnlyMesh.ComputeBounds();

            Camera camera = new Camera();
            camera.AutoFrameStep(stepOnlyMesh, preSmoothedFullMesh, elevation, azimuth);

            // Dynamically size the image width based on the model's projected shape.
            // Project the render mesh bounding box to find the natural aspect ratio,
            // then adapt width so wide models get wide images and tall models stay compact.
            float projAR = camera.GetProjectedAspectRatio(renderMesh);
            int dynamicWidth = (int)(height * Math.Max(1f, Math.Min(projAR, 2.5f)));
            // Snap to even for PNG encoding
            dynamicWidth = (dynamicWidth + 1) & ~1;

            IRenderer renderer;
            if (rendererType == RendererType.RayTracer)
            {
                renderer = new RayTracing.RayTraceRenderer(dynamicWidth, height);
            }
            else
            {
                SoftwareRenderer rasterizer = new SoftwareRenderer(dynamicWidth, height);
                rasterizer.RenderEdges = renderEdges;
                rasterizer.SmoothShading = smoothShading;
                renderer = rasterizer;
            }

            byte[] pixels = renderer.Render(renderMesh, camera);
            byte[] png = ImageExporter.ToPngBytes(pixels, dynamicWidth, height);

            return new StepRenderResult
            {
                PngBytes = png,
                TriangleCount = triEnd,
                EdgeCount = edgeEnd
            };
        }


        /// <summary>
        /// Render a single build step with new parts highlighted.
        /// For rendering multiple steps, prefer RenderAllStepsHighlighted() which
        /// is significantly faster due to caching.
        /// </summary>
        public byte[] RenderStepHighlighted(string[] lines,
                                             string fileName,
                                             int stepIndex,
                                             int width = 512,
                                             int height = 512,
                                             float elevation = 30f,
                                             float azimuth = -45f,
                                             bool renderEdges = true,
                                             bool smoothShading = true)
        {
            EnsureColours();

            int effectiveColour = 4; // default red

            // Resolve the FULL model for camera framing
            GeometryResolver fullResolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh fullMesh = fullResolver.ResolveContentUpToStep(lines, fileName, int.MaxValue, effectiveColour);

            // Resolve the current step mesh (cumulative)
            GeometryResolver stepResolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh stepMesh = stepResolver.ResolveContentUpToStep(lines, fileName, stepIndex, effectiveColour);

            if (stepMesh.Triangles.Count == 0)
            {
                return System.Array.Empty<byte>();
            }

            // Determine which triangles are "old" (from previous step)
            int oldTriCount = 0;
            int oldEdgeCount = 0;
            if (stepIndex > 0)
            {
                GeometryResolver prevResolver = new GeometryResolver(_libraryPath, _colours);
                LDrawMesh prevMesh = prevResolver.ResolveContentUpToStep(lines, fileName, stepIndex - 1, effectiveColour);
                oldTriCount = prevMesh.Triangles.Count;
                oldEdgeCount = prevMesh.EdgeLines.Count;
            }

            // Dim the old triangles to ~40% opacity (grey tint)
            for (int t = 0; t < oldTriCount && t < stepMesh.Triangles.Count; t++)
            {
                MeshTriangle tri = stepMesh.Triangles[t];

                // Blend toward grey: mix 40% of original, 60% towards a muted grey
                byte grey = 180;
                tri.R = (byte)((tri.R * 0.4f) + (grey * 0.6f));
                tri.G = (byte)((tri.G * 0.4f) + (grey * 0.6f));
                tri.B = (byte)((tri.B * 0.4f) + (grey * 0.6f));
                tri.A = 100; // semi-transparent

                stepMesh.Triangles[t] = tri;
            }

            // Dim old edge lines similarly
            for (int e = 0; e < oldEdgeCount && e < stepMesh.EdgeLines.Count; e++)
            {
                MeshLine line = stepMesh.EdgeLines[e];
                line.R = 200; line.G = 200; line.B = 200; line.A = 80;
                stepMesh.EdgeLines[e] = line;
            }

            if (smoothShading)
            {
                NormalSmoother.Smooth(stepMesh);
            }

            // Frame camera with focus on the new parts area
            Camera camera = new Camera();
            camera.AutoFrameStep(stepMesh, fullMesh, elevation, azimuth);

            // Dynamic aspect ratio: adapt width to the model's projected shape
            float projAR = camera.GetProjectedAspectRatio(stepMesh);
            int dynamicWidth = (int)(height * Math.Max(1f, Math.Min(projAR, 2.5f)));
            dynamicWidth = (dynamicWidth + 1) & ~1;

            SoftwareRenderer renderer = new SoftwareRenderer(dynamicWidth, height);
            renderer.RenderEdges = renderEdges;
            renderer.SmoothShading = smoothShading;

            byte[] pixels = renderer.Render(stepMesh, camera);

            return ImageExporter.ToPngBytes(pixels, dynamicWidth, height);
        }


        /// <summary>
        /// Helper: expand bounding box with a triangle's vertices.
        /// </summary>
        private static void ExpandBounds(ref float minX, ref float minY, ref float minZ,
                                          ref float maxX, ref float maxY, ref float maxZ,
                                          MeshTriangle tri)
        {
            if (tri.X1 < minX) minX = tri.X1; if (tri.Y1 < minY) minY = tri.Y1; if (tri.Z1 < minZ) minZ = tri.Z1;
            if (tri.X1 > maxX) maxX = tri.X1; if (tri.Y1 > maxY) maxY = tri.Y1; if (tri.Z1 > maxZ) maxZ = tri.Z1;
            if (tri.X2 < minX) minX = tri.X2; if (tri.Y2 < minY) minY = tri.Y2; if (tri.Z2 < minZ) minZ = tri.Z2;
            if (tri.X2 > maxX) maxX = tri.X2; if (tri.Y2 > maxY) maxY = tri.Y2; if (tri.Z2 > maxZ) maxZ = tri.Z2;
            if (tri.X3 < minX) minX = tri.X3; if (tri.Y3 < minY) minY = tri.Y3; if (tri.Z3 < minZ) minZ = tri.Z3;
            if (tri.X3 > maxX) maxX = tri.X3; if (tri.Y3 > maxY) maxY = tri.Y3; if (tri.Z3 > maxZ) maxZ = tri.Z3;
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

        /// <summary>
        /// Public wrapper: ensure the colour table is loaded.
        /// Call before parallel rendering to avoid thread-safety issues with lazy init.
        /// </summary>
        public void EnsureColoursPublic() => EnsureColours();


        /// <summary>
        /// Compute the centroid of newly-added triangles for a given step.
        /// Returns false if the step has no new triangles.
        /// </summary>
        public static bool ComputeNewPartsCentroid(
            LDrawMesh mesh, int[] stepTriangleBounds, int stepIndex,
            out float cx, out float cy, out float cz)
        {
            cx = cy = cz = 0f;
            int prevEnd = stepIndex > 0 ? stepTriangleBounds[stepIndex - 1] : 0;
            int curEnd = stepTriangleBounds[stepIndex];
            int newTriCount = curEnd - prevEnd;

            if (newTriCount <= 0) return false;

            double sx = 0, sy = 0, sz = 0;
            for (int t = prevEnd; t < curEnd; t++)
            {
                var tri = mesh.Triangles[t];
                // Average triangle centre (mean of 3 vertices)
                sx += (tri.X1 + tri.X2 + tri.X3);
                sy += (tri.Y1 + tri.Y2 + tri.Y3);
                sz += (tri.Z1 + tri.Z2 + tri.Z3);
            }

            double invCount = 1.0 / (newTriCount * 3); // 3 vertices per triangle
            cx = (float)(sx * invCount);
            cy = (float)(sy * invCount);
            cz = (float)(sz * invCount);
            return true;
        }


        /// <summary>
        /// Compute the optimal camera azimuth to face newly-added parts.
        ///
        /// Projects the vector from model centre → new-parts centroid onto the XZ plane
        /// and returns the angle in degrees that would point the camera at those parts.
        ///
        /// The result is blended with the default azimuth to prevent jarring jumps:
        ///   finalAzimuth = defaultAzimuth + blendFactor × shortestAngleDelta
        /// </summary>
        /// <param name="mesh">Pre-smoothed full mesh (for model centre).</param>
        /// <param name="stepTriangleBounds">Cumulative triangle counts per step.</param>
        /// <param name="stepIndex">Current step index.</param>
        /// <param name="defaultAzimuth">User's configured default azimuth (degrees).</param>
        /// <param name="blendFactor">0.0 = always default, 1.0 = always face new parts. Recommended: 0.6.</param>
        /// <returns>Blended azimuth in degrees, or null if unable to compute.</returns>
        public static float? ComputeAutoAzimuth(
            LDrawMesh mesh, int[] stepTriangleBounds, int stepIndex,
            float defaultAzimuth, float blendFactor = 0.6f)
        {
            if (!ComputeNewPartsCentroid(mesh, stepTriangleBounds, stepIndex,
                out float ncx, out float ncy, out float ncz))
                return null;

            mesh.GetCenter(out float mcx, out float mcy, out float mcz);

            // Offset vector from model centre to new-parts centroid (XZ plane)
            float dx = ncx - mcx;
            float dz = ncz - mcz;

            // If the offset is negligible (parts are at the centre), keep default
            float dist = (float)System.Math.Sqrt(dx * dx + dz * dz);
            float modelExtent = mesh.GetMaxExtent();
            if (dist < modelExtent * 0.02f)
                return null; // Parts too central to determine a direction

            // atan2 gives the angle from +Z axis toward +X axis
            // The camera looks FROM this direction, so the optimal azimuth
            // places the camera on the same side as the new parts
            float optimalAzimuthRad = (float)System.Math.Atan2(dx, dz);
            float optimalAzimuthDeg = optimalAzimuthRad * 180f / (float)System.Math.PI;

            // Compute shortest angular delta (handles wrapping around ±180°)
            float delta = optimalAzimuthDeg - defaultAzimuth;
            while (delta > 180f) delta -= 360f;
            while (delta < -180f) delta += 360f;

            float blendedAzimuth = defaultAzimuth + blendFactor * delta;
            return blendedAzimuth;
        }


        /// <summary>
        /// Compute the optimal camera elevation to face newly-added parts.
        ///
        /// When new parts are significantly above or below the model centre (Y axis),
        /// the elevation is adjusted to tilt the camera toward them.
        /// In LDraw, Y points down — higher Y = lower physical position (bottom of model).
        /// </summary>
        /// <param name="mesh">Pre-smoothed full mesh.</param>
        /// <param name="stepTriangleBounds">Cumulative triangle counts per step.</param>
        /// <param name="stepIndex">Current step index.</param>
        /// <param name="defaultElevation">User's configured default elevation (degrees).</param>
        /// <param name="blendFactor">0.0 = always default, 1.0 = fully tilt. Recommended: 0.6.</param>
        /// <returns>Blended elevation in degrees, or null if no significant Y offset.</returns>
        public static float? ComputeAutoElevation(
            LDrawMesh mesh, int[] stepTriangleBounds, int stepIndex,
            float defaultElevation, float blendFactor = 0.6f)
        {
            if (!ComputeNewPartsCentroid(mesh, stepTriangleBounds, stepIndex,
                out float ncx, out float ncy, out float ncz))
                return null;

            mesh.GetCenter(out float mcx, out float mcy, out float mcz);

            // Y offset: positive dy means new parts are BELOW model centre (LDraw Y-down)
            float dy = ncy - mcy;
            float modelExtent = mesh.GetMaxExtent();

            // Only adjust if the vertical offset is significant (>5% of model extent)
            if (System.Math.Abs(dy) < modelExtent * 0.05f)
                return null;

            // Compute how much to tilt:
            // Parts below centre (dy > 0) → reduce elevation (look from lower angle)
            // Parts above centre (dy < 0) → increase elevation (look from higher angle)
            // Scale: at dy = half-extent, tilt toward 0° or 60° respectively
            float halfExtent = modelExtent * 0.5f;
            float normalizedOffset = dy / halfExtent; // -1 to +1 range roughly
            normalizedOffset = System.Math.Clamp(normalizedOffset, -1f, 1f);

            // Target elevation: 0° (eye-level) for bottom parts, 60° for top parts
            float optimalElevation = defaultElevation - normalizedOffset * 25f;
            // Clamp to reasonable range (5° to 75°)
            optimalElevation = System.Math.Clamp(optimalElevation, 5f, 75f);

            float delta = optimalElevation - defaultElevation;
            float blendedElevation = defaultElevation + blendFactor * delta;
            return blendedElevation;
        }
    }
}

