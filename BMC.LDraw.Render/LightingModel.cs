using System;
using System.Collections.Generic;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Defines a single light source for the rendering pipeline.
    ///
    /// AI-generated — Feb 2026 (Phase 1.3).
    /// </summary>
    public class Light
    {
        /// <summary>
        /// Normalized direction pointing TOWARD the light (for directional lights).
        /// For point lights, this is the world-space position.
        /// </summary>
        public float DirectionX { get; set; }
        public float DirectionY { get; set; }
        public float DirectionZ { get; set; }

        /// <summary>Light colour (0–1 per channel).</summary>
        public float ColourR { get; set; } = 1f;
        public float ColourG { get; set; } = 1f;
        public float ColourB { get; set; } = 1f;

        /// <summary>Light intensity multiplier.</summary>
        public float Intensity { get; set; } = 1f;

        /// <summary>Light type (directional or point).</summary>
        public LightType Type { get; set; } = LightType.Directional;
    }


    /// <summary>
    /// Types of light sources supported by the renderer.
    /// </summary>
    public enum LightType
    {
        /// <summary>Parallel rays from a distant source (like the sun).</summary>
        Directional,

        /// <summary>Radiates from a point in world space, with intensity falloff.</summary>
        Point
    }


    /// <summary>
    /// A complete lighting configuration for the renderer.
    ///
    /// Contains a collection of light sources, ambient lighting parameters,
    /// and specular highlight settings using the Blinn-Phong reflection model.
    ///
    /// AI-generated — Feb 2026 (Phase 1.3).
    /// </summary>
    public class LightingModel
    {
        /// <summary>All light sources in the scene.</summary>
        public List<Light> Lights { get; set; } = new List<Light>();

        /// <summary>Ambient light colour (0–1 per channel).</summary>
        public float AmbientR { get; set; } = 1f;
        public float AmbientG { get; set; } = 1f;
        public float AmbientB { get; set; } = 1f;

        /// <summary>Ambient light intensity (0–1).</summary>
        public float AmbientIntensity { get; set; } = 0.35f;

        /// <summary>
        /// Blinn-Phong specular exponent (shininess).
        /// Higher values create tighter, more focused highlights.
        /// Typical range: 16 (matte) to 128 (glossy plastic).
        /// </summary>
        public float SpecularPower { get; set; } = 32f;

        /// <summary>
        /// Specular intensity multiplier (0–1).
        /// Controls how bright specular highlights appear.
        /// </summary>
        public float SpecularIntensity { get; set; } = 0.4f;


        /// <summary>
        /// Returns the default lighting model that matches the original hardcoded behaviour:
        /// a single directional light at (0.3, -0.7, 0.6) with 0.35 ambient intensity
        /// and no specular highlights.
        /// </summary>
        public static LightingModel Default()
        {
            LightingModel model = new LightingModel();

            //
            // Normalize the original light direction
            //
            float lx = 0.3f, ly = -0.7f, lz = 0.6f;
            float len = (float)Math.Sqrt(lx * lx + ly * ly + lz * lz);
            lx /= len;
            ly /= len;
            lz /= len;

            model.Lights.Add(new Light
            {
                DirectionX = lx,
                DirectionY = ly,
                DirectionZ = lz,
                ColourR = 1f,
                ColourG = 1f,
                ColourB = 1f,
                Intensity = 1f,
                Type = LightType.Directional
            });

            model.AmbientIntensity = 0.45f;
            model.SpecularPower = 32f;
            model.SpecularIntensity = 0f; // Off by default for backward compatibility

            return model;
        }


        /// <summary>
        /// A "studio" lighting setup with a key light, fill light, and rim/back light.
        /// Produces more dynamic, visually interesting renders with depth and highlights.
        /// </summary>
        public static LightingModel Studio()
        {
            LightingModel model = new LightingModel();

            //
            // Key light — main directional light from upper-right front
            //
            AddNormalizedDirectionalLight(model, 0.5f, -0.7f, 0.5f, 1.0f, 0.98f, 0.95f, 0.85f);

            //
            // Fill light — softer light from the opposite side to reduce harsh shadows
            //
            AddNormalizedDirectionalLight(model, -0.6f, -0.3f, 0.4f, 0.85f, 0.9f, 0.95f, 0.35f);

            //
            // Rim light — from behind and above to create edge highlights
            //
            AddNormalizedDirectionalLight(model, -0.2f, -0.8f, -0.5f, 1.0f, 1.0f, 1.0f, 0.25f);

            model.AmbientIntensity = 0.2f;
            model.SpecularPower = 48f;
            model.SpecularIntensity = 0.5f;

            return model;
        }


        /// <summary>
        /// Adds a directional light with an automatically normalized direction vector.
        /// </summary>
        private static void AddNormalizedDirectionalLight(LightingModel model,
                                                          float dx, float dy, float dz,
                                                          float r, float g, float b,
                                                          float intensity)
        {
            float len = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);

            if (len > 1e-8f)
            {
                dx /= len;
                dy /= len;
                dz /= len;
            }

            model.Lights.Add(new Light
            {
                DirectionX = dx,
                DirectionY = dy,
                DirectionZ = dz,
                ColourR = r,
                ColourG = g,
                ColourB = b,
                Intensity = intensity,
                Type = LightType.Directional
            });
        }
    }
}
