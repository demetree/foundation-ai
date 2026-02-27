using System;

namespace BMC.LDraw.Render.RayTracing
{
    /// <summary>
    /// Procedural sky environment map — Preetham-style gradient with a sun disk.
    ///
    /// Generates a studio-like environment with:
    ///   - Cool blue zenith transitioning to warm golden horizon
    ///   - A bright sun disk in the configured direction
    ///   - Slightly desaturated ground (below horizon)
    ///
    /// Produces linear HDR values — bright areas (sun disk) exceed 1.0 and
    /// rely on tone mapping for natural highlight rolloff.
    /// </summary>
    public class ProceduralSky : IEnvironmentMap
    {
        /// <summary>Normalised sun direction (toward the sun).</summary>
        public float SunDirX { get; set; }
        public float SunDirY { get; set; }
        public float SunDirZ { get; set; }

        /// <summary>Angular radius of the sun disk in radians.</summary>
        public float SunRadius { get; set; } = 0.04f;

        /// <summary>Sun intensity multiplier (HDR — can exceed 1.0).</summary>
        public float SunIntensity { get; set; } = 8.0f;

        /// <summary>Sun colour (linear).</summary>
        public float SunR { get; set; } = 1.0f;
        public float SunG { get; set; } = 0.95f;
        public float SunB { get; set; } = 0.85f;

        /// <summary>Overall sky brightness multiplier.</summary>
        public float SkyIntensity { get; set; } = 1.0f;


        /// <summary>
        /// Create a procedural sky with a default studio lighting direction.
        /// Sun comes from upper-right front — matches the Studio lighting preset.
        /// </summary>
        public ProceduralSky()
        {
            //
            // Default sun direction: upper-right front (matches Studio key light)
            //
            float dx = 0.5f, dy = -0.7f, dz = 0.5f;
            float len = MathF.Sqrt(dx * dx + dy * dy + dz * dz);
            SunDirX = dx / len;
            SunDirY = dy / len;
            SunDirZ = dz / len;
        }


        /// <summary>
        /// Sample the procedural sky in the given direction.
        /// </summary>
        public void Sample(float dirX, float dirY, float dirZ,
                           out float r, out float g, out float b)
        {
            //
            // Y axis is vertical in LDraw (Y-down), so negate for sky calculations.
            // upness = 1 at zenith, 0 at horizon, negative below horizon.
            //
            float upness = -dirY;

            if (upness >= 0f)
            {
                //
                // Sky gradient: horizon → zenith
                //
                float t = MathF.Sqrt(upness); // sqrt makes horizon band thicker

                // Zenith: cool blue
                float zenithR = 0.15f, zenithG = 0.25f, zenithB = 0.55f;

                // Horizon: warm golden
                float horizonR = 0.65f, horizonG = 0.55f, horizonB = 0.45f;

                r = (horizonR + (zenithR - horizonR) * t) * SkyIntensity;
                g = (horizonG + (zenithG - horizonG) * t) * SkyIntensity;
                b = (horizonB + (zenithB - horizonB) * t) * SkyIntensity;
            }
            else
            {
                //
                // Ground: desaturated warm grey, darkening downward
                //
                float groundFade = MathF.Min(1f, -upness * 3f);

                float groundR = 0.3f, groundG = 0.28f, groundB = 0.25f;
                float darkR = 0.12f, darkG = 0.11f, darkB = 0.10f;

                r = (groundR + (darkR - groundR) * groundFade) * SkyIntensity;
                g = (groundG + (darkG - groundG) * groundFade) * SkyIntensity;
                b = (groundB + (darkB - groundB) * groundFade) * SkyIntensity;
            }

            //
            // Sun disk: bright HDR circle around the sun direction
            //
            float sunDot = dirX * SunDirX + dirY * SunDirY + dirZ * SunDirZ;

            if (sunDot > 0f)
            {
                //
                // Soft-edged sun disk using smoothstep-like falloff
                //
                float cosRadius = MathF.Cos(SunRadius);
                float cosOuter = MathF.Cos(SunRadius * 3f); // soft glow ring

                if (sunDot > cosOuter)
                {
                    float sunFactor;

                    if (sunDot > cosRadius)
                    {
                        // Inside the sun disk core — full intensity
                        sunFactor = 1f;
                    }
                    else
                    {
                        // Glow ring — smooth falloff
                        sunFactor = (sunDot - cosOuter) / (cosRadius - cosOuter);
                        sunFactor = sunFactor * sunFactor; // quadratic falloff
                    }

                    r += SunR * SunIntensity * sunFactor;
                    g += SunG * SunIntensity * sunFactor;
                    b += SunB * SunIntensity * sunFactor;
                }
            }
        }
    }
}
