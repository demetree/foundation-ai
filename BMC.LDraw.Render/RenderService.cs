using System;
using System.Collections.Generic;
using BMC.LDraw.Models;
using BMC.LDraw.Parsers;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Facade that ties parsing, resolution, and rendering together.
    /// Provides a single-call API to render an LDraw file to a PNG image.
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
        public void RenderToFile(string inputPath, string outputPath, int width = 512, int height = 512, int colourCode = -1)
        {
            byte[] pixels = RenderToPixels(inputPath, width, height, colourCode);
            PngExporter.SaveToPng(pixels, width, height, outputPath);
        }

        /// <summary>
        /// Render an LDraw file to PNG bytes (in-memory).
        /// </summary>
        public byte[] RenderToPng(string inputPath, int width = 512, int height = 512, int colourCode = -1)
        {
            byte[] pixels = RenderToPixels(inputPath, width, height, colourCode);
            return PngExporter.ToPngBytes(pixels, width, height);
        }

        /// <summary>
        /// Render an LDraw file to raw RGBA pixels.
        /// </summary>
        public byte[] RenderToPixels(string inputPath, int width = 512, int height = 512, int colourCode = -1)
        {
            EnsureColours();

            // Default colour: 4 = Red (a recognizable default for LDraw parts)
            int effectiveColour = colourCode >= 0 ? colourCode : 4;

            // Parse and resolve
            GeometryResolver resolver = new GeometryResolver(_libraryPath, _colours);
            LDrawMesh mesh = resolver.ResolveFile(inputPath, effectiveColour);

            if (mesh.Triangles.Count == 0)
            {
                // Empty mesh — return transparent image
                return new byte[width * height * 4];
            }

            // Set up camera
            Camera camera = new Camera();
            camera.AutoFrame(mesh);

            // Render
            SoftwareRenderer renderer = new SoftwareRenderer(width, height);
            return renderer.Render(mesh, camera);
        }

        /// <summary>
        /// Load and cache the LDraw colour table.
        /// </summary>
        private void EnsureColours()
        {
            if (_colours != null) return;

            string configPath = System.IO.Path.Combine(_libraryPath, "LDConfig.ldr");
            if (System.IO.File.Exists(configPath))
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
