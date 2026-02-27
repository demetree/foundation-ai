using System;

namespace BMC.LDraw.Render.RayTracing
{
    /// <summary>
    /// Physically-Based Rendering (PBR) shading using the Cook-Torrance microfacet BRDF.
    ///
    /// Replaces the legacy Blinn-Phong shading model with:
    ///   D — GGX/Trowbridge-Reitz normal distribution
    ///   G — Smith GGX geometry (height-correlated)
    ///   F — Schlick Fresnel approximation
    ///
    /// Combined with importance-sampled IBL when an environment map is available.
    /// </summary>
    public static class PbrShading
    {
        /// <summary>
        /// Shade a hit point using Cook-Torrance PBR.
        ///
        /// Returns linear HDR colour values (may exceed 1.0 — tone mapping
        /// is applied later).
        /// </summary>
        public static void Shade(
            LightingModel lighting,
            BVH bvh,
            ref HitRecord hit,
            float hitX, float hitY, float hitZ,
            float eyeX, float eyeY, float eyeZ,
            PbrMaterial mat,
            int shadowSamples, Random rng,
            out float outR, out float outG, out float outB)
        {
            float nx = hit.NX, ny = hit.NY, nz = hit.NZ;

            // Albedo in linear space (approximate — sRGB input would need
            // de-gamma, but LDraw colours are perceptually close enough)
            float albedoR = hit.R / 255f;
            float albedoG = hit.G / 255f;
            float albedoB = hit.B / 255f;

            //
            // View direction
            //
            float viewX = eyeX - hitX;
            float viewY = eyeY - hitY;
            float viewZ = eyeZ - hitZ;
            Normalize(ref viewX, ref viewY, ref viewZ);

            float ndotv = nx * viewX + ny * viewY + nz * viewZ;
            if (ndotv < 0f) ndotv = 0f;

            //
            // F0: base reflectance — metals use albedo, dielectrics use constant
            //
            float f0R, f0G, f0B;

            if (mat.Metalness > 0.5f)
            {
                f0R = albedoR * mat.F0 + (1f - mat.Metalness) * 0.04f;
                f0G = albedoG * mat.F0 + (1f - mat.Metalness) * 0.04f;
                f0B = albedoB * mat.F0 + (1f - mat.Metalness) * 0.04f;
            }
            else
            {
                f0R = mat.F0;
                f0G = mat.F0;
                f0B = mat.F0;
            }

            //
            // Ambient contribution (will be replaced by IBL later)
            //
            float ambR = lighting.AmbientR * lighting.AmbientIntensity;
            float ambG = lighting.AmbientG * lighting.AmbientIntensity;
            float ambB = lighting.AmbientB * lighting.AmbientIntensity;

            // Metals tint ambient by albedo; dielectrics use full ambient
            float diffuseWeight = 1f - mat.Metalness;
            outR = albedoR * ambR * diffuseWeight;
            outG = albedoG * ambG * diffuseWeight;
            outB = albedoB * ambB * diffuseWeight;

            //
            // Jitter radius for soft shadows
            //
            const float SHADOW_JITTER = 0.015f;

            //
            // Per-light contributions
            //
            for (int i = 0; i < lighting.Lights.Count; i++)
            {
                Light light = lighting.Lights[i];

                float lx = light.DirectionX;
                float ly = light.DirectionY;
                float lz = light.DirectionZ;
                float shadowDist = float.MaxValue;

                if (light.Type == LightType.Point)
                {
                    lx = light.DirectionX - hitX;
                    ly = light.DirectionY - hitY;
                    lz = light.DirectionZ - hitZ;
                    shadowDist = MathF.Sqrt(lx * lx + ly * ly + lz * lz);
                    Normalize(ref lx, ref ly, ref lz);
                }

                //
                // Shadow test
                //
                float shadowFactor = ComputeShadow(bvh, nx, ny, nz,
                    hitX, hitY, hitZ, lx, ly, lz,
                    shadowDist, shadowSamples, SHADOW_JITTER, rng);

                if (shadowFactor <= 0f) continue;

                float ndotl = nx * lx + ny * ly + nz * lz;
                if (ndotl <= 0f) continue;

                //
                // Half vector
                //
                float hx = lx + viewX;
                float hy = ly + viewY;
                float hz = lz + viewZ;
                Normalize(ref hx, ref hy, ref hz);

                float ndoth = nx * hx + ny * hy + nz * hz;
                if (ndoth < 0f) ndoth = 0f;

                float vdoth = viewX * hx + viewY * hy + viewZ * hz;
                if (vdoth < 0f) vdoth = 0f;

                //
                // Cook-Torrance BRDF terms
                //
                float D = DistributionGGX(ndoth, mat.Roughness);
                float G = GeometrySmith(ndotv, ndotl, mat.Roughness);
                float fresnelR, fresnelG, fresnelB;
                FresnelSchlick(vdoth, f0R, f0G, f0B, out fresnelR, out fresnelG, out fresnelB);

                //
                // Specular: DGF / (4 * NdotV * NdotL)
                //
                float denom = 4f * ndotv * ndotl + 0.0001f;
                float specScale = D * G / denom;

                float specR = specScale * fresnelR;
                float specG = specScale * fresnelG;
                float specB = specScale * fresnelB;

                //
                // Diffuse: Lambertian * (1 - F) * (1 - metalness)
                // Metals have no diffuse — all energy goes to specular
                //
                float kDiffR = (1f - fresnelR) * diffuseWeight;
                float kDiffG = (1f - fresnelG) * diffuseWeight;
                float kDiffB = (1f - fresnelB) * diffuseWeight;

                float diffR = kDiffR * albedoR * INV_PI;
                float diffG = kDiffG * albedoG * INV_PI;
                float diffB = kDiffB * albedoB * INV_PI;

                //
                // Combine and accumulate
                //
                float lightScale = ndotl * light.Intensity * shadowFactor;

                outR += (diffR + specR) * light.ColourR * lightScale;
                outG += (diffG + specG) * light.ColourG * lightScale;
                outB += (diffB + specB) * light.ColourB * lightScale;
            }
        }


        // ── Cook-Torrance BRDF Components ──

        private const float PI = MathF.PI;
        private const float INV_PI = 1f / MathF.PI;


        /// <summary>
        /// GGX/Trowbridge-Reitz normal distribution function.
        /// Describes the probability of microfacets being aligned with the half-vector.
        /// </summary>
        private static float DistributionGGX(float ndoth, float roughness)
        {
            float a = roughness * roughness;
            float a2 = a * a;
            float ndoth2 = ndoth * ndoth;

            float denom = ndoth2 * (a2 - 1f) + 1f;
            denom = PI * denom * denom;

            return a2 / MathF.Max(denom, 0.0001f);
        }


        /// <summary>
        /// Smith GGX geometry function (height-correlated).
        /// Models self-shadowing of microfacets from both light and view directions.
        /// </summary>
        private static float GeometrySmith(float ndotv, float ndotl, float roughness)
        {
            float r = roughness + 1f;
            float k = (r * r) / 8f;

            float ggx1 = ndotv / (ndotv * (1f - k) + k);
            float ggx2 = ndotl / (ndotl * (1f - k) + k);

            return ggx1 * ggx2;
        }


        /// <summary>
        /// Schlick Fresnel approximation (per-channel for coloured metals).
        /// Reflectance increases at grazing angles — this is what makes
        /// the edges of LEGO bricks catch the light.
        /// </summary>
        private static void FresnelSchlick(float cosTheta,
            float f0R, float f0G, float f0B,
            out float fR, out float fG, out float fB)
        {
            float t = 1f - cosTheta;
            float t2 = t * t;
            float t5 = t2 * t2 * t;

            fR = f0R + (1f - f0R) * t5;
            fG = f0G + (1f - f0G) * t5;
            fB = f0B + (1f - f0B) * t5;
        }


        // ── Shadow helpers ──

        /// <summary>
        /// Multi-sample shadow test (reused from the original ray tracer logic).
        /// </summary>
        private static float ComputeShadow(BVH bvh,
            float nx, float ny, float nz,
            float hitX, float hitY, float hitZ,
            float lx, float ly, float lz,
            float shadowDist, int shadowSamples,
            float jitterRadius, Random rng)
        {
            if (shadowSamples <= 1)
            {
                Ray sr = new Ray(
                    hitX + nx * 0.01f,
                    hitY + ny * 0.01f,
                    hitZ + nz * 0.01f,
                    lx, ly, lz);

                return bvh.IntersectShadow(ref sr, shadowDist) ? 0f : 1f;
            }

            int litCount = 0;

            for (int s = 0; s < shadowSamples; s++)
            {
                float jx = lx + (float)(rng.NextDouble() - 0.5) * jitterRadius * 2f;
                float jy = ly + (float)(rng.NextDouble() - 0.5) * jitterRadius * 2f;
                float jz = lz + (float)(rng.NextDouble() - 0.5) * jitterRadius * 2f;
                Normalize(ref jx, ref jy, ref jz);

                Ray sr = new Ray(
                    hitX + nx * 0.01f,
                    hitY + ny * 0.01f,
                    hitZ + nz * 0.01f,
                    jx, jy, jz);

                if (!bvh.IntersectShadow(ref sr, shadowDist))
                    litCount++;
            }

            return (float)litCount / shadowSamples;
        }


        // ── Vector utilities ──

        private static void Normalize(ref float x, ref float y, ref float z)
        {
            float len = MathF.Sqrt(x * x + y * y + z * z);
            if (len > 1e-8f) { x /= len; y /= len; z /= len; }
        }
    }
}
