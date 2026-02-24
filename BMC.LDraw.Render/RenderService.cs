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

            PngExporter.SaveToPng(pixels, width, height, outputPath);
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

            return PngExporter.ToPngBytes(pixels, width, height);
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

