using System;
using System.Threading.Tasks;
using BMC.LDraw.Models;

namespace BMC.LDraw.Render.RayTracing
{
    /// <summary>
    /// CPU-based ray trace renderer implementing IRenderer.
    ///
    /// Uses a Bounding Volume Hierarchy (BVH) for fast ray-triangle intersection,
    /// Blinn-Phong shading with the shared LightingModel, and hard shadow rays.
    ///
    /// Sits alongside SoftwareRenderer as an alternative rendering backend —
    /// slower but produces shadows, correct occlusion, and a more realistic look.
    /// </summary>
    public class RayTraceRenderer : IRenderer
    {
        private readonly int _width;
        private readonly int _height;
        private byte[] _colourBuffer;

        // Background
        private byte _bgR, _bgG, _bgB, _bgA;
        private bool _hasGradient;
        private byte _gradTopR, _gradTopG, _gradTopB;
        private byte _gradBotR, _gradBotG, _gradBotB;


        public int Width => _width;
        public int Height => _height;
        public LightingModel Lighting { get; set; } = LightingModel.Default();

        /// <summary>Number of jittered shadow rays per light (1 = hard shadows, 4+ = soft).</summary>
        public int ShadowSamples { get; set; } = 4;

        /// <summary>Number of ambient occlusion rays per hit point (0 = disabled, 8+ = good quality).</summary>
        public int AoSamples { get; set; } = 8;

        /// <summary>Maximum distance for AO rays to probe (world units).</summary>
        public float AoRadius { get; set; } = 50f;

        /// <summary>Strength of the AO darkening effect (0 = off, 1 = full).</summary>
        public float AoIntensity { get; set; } = 0.6f;


        public RayTraceRenderer(int width, int height)
        {
            _width = width;
            _height = height;
            _colourBuffer = new byte[width * height * 4];
        }


        public void SetBackground(byte r, byte g, byte b, byte a)
        {
            _bgR = r; _bgG = g; _bgB = b; _bgA = a;
            _hasGradient = false;
        }


        public void SetGradientBackground(byte topR, byte topG, byte topB,
                                           byte bottomR, byte bottomG, byte bottomB)
        {
            _hasGradient = true;
            _gradTopR = topR; _gradTopG = topG; _gradTopB = topB;
            _gradBotR = bottomR; _gradBotG = bottomG; _gradBotB = bottomB;
        }


        /// <summary>
        /// Render the mesh using ray tracing.
        ///
        /// Pipeline:
        ///   1. Build BVH from mesh triangles
        ///   2. Compute camera basis vectors (right, up, forward) from Camera
        ///   3. For each pixel: cast primary ray → BVH intersection → shading + shadows
        ///   4. Return RGBA pixel buffer
        /// </summary>
        public byte[] Render(LDrawMesh mesh, Camera camera)
        {
            //
            // Clear to background
            //
            ClearBuffer();

            if (mesh.Triangles.Count == 0)
            {
                return _colourBuffer;
            }

            //
            // Build BVH
            //
            BVH bvh = new BVH();
            bvh.Build(mesh.Triangles);

            //
            // Compute camera basis for ray generation
            //
            float eyeX = camera.EyeX;
            float eyeY = camera.EyeY;
            float eyeZ = camera.EyeZ;

            // Forward direction
            float fwdX = camera.TargetX - eyeX;
            float fwdY = camera.TargetY - eyeY;
            float fwdZ = camera.TargetZ - eyeZ;
            Normalize(ref fwdX, ref fwdY, ref fwdZ);

            // Right = forward × up
            float rightX, rightY, rightZ;
            Cross(fwdX, fwdY, fwdZ, camera.UpX, camera.UpY, camera.UpZ,
                  out rightX, out rightY, out rightZ);
            Normalize(ref rightX, ref rightY, ref rightZ);

            // True up = right × forward
            float upX, upY, upZ;
            Cross(rightX, rightY, rightZ, fwdX, fwdY, fwdZ,
                  out upX, out upY, out upZ);
            Normalize(ref upX, ref upY, ref upZ);

            //
            // Compute the view plane half-extents from FOV
            //
            float aspect = (float)_width / _height;
            float fovRad = camera.FieldOfView * MathF.PI / 180f;
            float halfH = MathF.Tan(fovRad * 0.5f);
            float halfW = halfH * aspect;

            bool ortho = camera.Orthographic;
            float orthoHalfH = camera.OrthoHeight * 0.5f;
            float orthoHalfW = orthoHalfH * aspect;

            //
            // Capture settings for the closure
            //
            LightingModel lighting = Lighting;
            int shadowSamples = ShadowSamples;
            int aoSamples = AoSamples;
            float aoRadius = AoRadius;
            float aoIntensity = AoIntensity;

            //
            // Ray trace — one scanline at a time in parallel
            // Each thread gets its own RNG to avoid contention
            //
            Parallel.For(0, _height, () => new Random(Environment.CurrentManagedThreadId * 31 + Environment.TickCount),
                (y, state, rng) =>
            {
                for (int x = 0; x < _width; x++)
                {
                    //
                    // Map pixel to normalised coordinates [-1, 1]
                    //
                    float u = (2f * (x + 0.5f) / _width - 1f);
                    float v = (1f - 2f * (y + 0.5f) / _height);

                    Ray ray;

                    if (ortho)
                    {
                        //
                        // Orthographic: ray origin moves across the view plane,
                        // direction is always forward
                        //
                        float ox = eyeX + rightX * u * orthoHalfW + upX * v * orthoHalfH;
                        float oy = eyeY + rightY * u * orthoHalfW + upY * v * orthoHalfH;
                        float oz = eyeZ + rightZ * u * orthoHalfW + upZ * v * orthoHalfH;
                        ray = new Ray(ox, oy, oz, fwdX, fwdY, fwdZ);
                    }
                    else
                    {
                        //
                        // Perspective: rays originate from the eye and fan out
                        //
                        float dirX = fwdX + rightX * u * halfW + upX * v * halfH;
                        float dirY = fwdY + rightY * u * halfW + upY * v * halfH;
                        float dirZ = fwdZ + rightZ * u * halfW + upZ * v * halfH;
                        Normalize(ref dirX, ref dirY, ref dirZ);
                        ray = new Ray(eyeX, eyeY, eyeZ, dirX, dirY, dirZ);
                    }

                    //
                    // Cast primary ray
                    //
                    if (bvh.Intersect(ref ray, out HitRecord hit))
                    {
                        //
                        // Hit point in world space
                        //
                        float hitX = ray.OriginX + ray.DirX * hit.T;
                        float hitY = ray.OriginY + ray.DirY * hit.T;
                        float hitZ = ray.OriginZ + ray.DirZ * hit.T;

                        //
                        // Get material finish for this triangle
                        //
                        MaterialFinish finish = bvh.Triangles[hit.TriIndex].Finish;

                        //
                        // Compute shading with materials
                        //
                        ShadePixel(lighting, bvh, ref ray, ref hit,
                                   hitX, hitY, hitZ,
                                   eyeX, eyeY, eyeZ,
                                   finish, 0,
                                   shadowSamples, aoSamples, aoRadius, aoIntensity, rng,
                                   out byte finalR, out byte finalG, out byte finalB);

                        int bufIdx = (y * _width + x) * 4;
                        _colourBuffer[bufIdx] = finalR;
                        _colourBuffer[bufIdx + 1] = finalG;
                        _colourBuffer[bufIdx + 2] = finalB;
                        _colourBuffer[bufIdx + 3] = hit.A;
                    }
                    // else: pixel stays at background colour
                }

                return rng;
            },
            _ => { }); // finalizer — nothing to clean up

            return _colourBuffer;
        }


        /// <summary>Maximum number of reflection bounces.</summary>
        private const int MAX_BOUNCES = 3;


        /// <summary>
        /// Shade a pixel with material-aware rendering.
        /// Handles reflections for chrome/metal, matte for rubber, standard Blinn-Phong for solid.
        /// </summary>
        private static void ShadePixel(LightingModel lighting, BVH bvh,
            ref Ray incomingRay, ref HitRecord hit,
            float hitX, float hitY, float hitZ,
            float eyeX, float eyeY, float eyeZ,
            MaterialFinish finish, int depth,
            int shadowSamples, int aoSamples, float aoRadius, float aoIntensity, Random rng,
            out byte outR, out byte outG, out byte outB)
        {
            //
            // Get material properties from finish type
            //
            float reflectivity, specPower, diffuseFactor;
            GetMaterialProperties(finish, out reflectivity, out specPower, out diffuseFactor);

            //
            // Compute direct lighting (Blinn-Phong with soft shadows)
            //
            ComputeDirectLighting(lighting, bvh, ref hit,
                hitX, hitY, hitZ, eyeX, eyeY, eyeZ,
                finish, specPower, diffuseFactor, shadowSamples, rng,
                out float directR, out float directG, out float directB);

            //
            // Ambient Occlusion — darkens crevices and corners naturally
            //
            if (aoSamples > 0 && aoIntensity > 0f && depth == 0)
            {
                float ao = ComputeAO(bvh, ref hit, hitX, hitY, hitZ, aoSamples, aoRadius, rng);

                // ao = 1.0 means fully occluded, 0.0 means fully open
                float aoFactor = 1f - ao * aoIntensity;

                directR *= aoFactor;
                directG *= aoFactor;
                directB *= aoFactor;
            }

            //
            // Reflection — for chrome, metal, and pearlescent surfaces
            //
            float reflR = 0, reflG = 0, reflB = 0;

            if (reflectivity > 0f && depth < MAX_BOUNCES)
            {
                float nx = hit.NX, ny = hit.NY, nz = hit.NZ;

                // Reflection direction: R = D - 2(D·N)N
                float dDotn = incomingRay.DirX * nx + incomingRay.DirY * ny + incomingRay.DirZ * nz;
                float rx = incomingRay.DirX - 2f * dDotn * nx;
                float ry = incomingRay.DirY - 2f * dDotn * ny;
                float rz = incomingRay.DirZ - 2f * dDotn * nz;
                Normalize(ref rx, ref ry, ref rz);

                Ray reflRay = new Ray(
                    hitX + nx * 0.01f,
                    hitY + ny * 0.01f,
                    hitZ + nz * 0.01f,
                    rx, ry, rz);

                if (bvh.Intersect(ref reflRay, out HitRecord reflHit))
                {
                    float rHitX = reflRay.OriginX + reflRay.DirX * reflHit.T;
                    float rHitY = reflRay.OriginY + reflRay.DirY * reflHit.T;
                    float rHitZ = reflRay.OriginZ + reflRay.DirZ * reflHit.T;

                    MaterialFinish reflFinish = bvh.Triangles[reflHit.TriIndex].Finish;

                    // Reflection bounces use 1 shadow sample (hard shadows for performance)
                    ShadePixel(lighting, bvh, ref reflRay, ref reflHit,
                               rHitX, rHitY, rHitZ,
                               hitX, hitY, hitZ,
                               reflFinish, depth + 1,
                               1, 0, 0, 0, rng,  // no AO on reflection bounces
                               out byte rr, out byte rg, out byte rb);

                    reflR = rr / 255f;
                    reflG = rg / 255f;
                    reflB = rb / 255f;
                }
                else
                {
                    float skyT = (ry + 1f) * 0.5f;
                    reflR = 0.6f + 0.4f * skyT;
                    reflG = 0.7f + 0.3f * skyT;
                    reflB = 0.8f + 0.2f * skyT;
                }
            }

            //
            // Blend direct lighting with reflections based on material
            //
            float surfR = hit.R / 255f;
            float surfG = hit.G / 255f;
            float surfB = hit.B / 255f;

            float finalR, finalG, finalB;

            if (finish == MaterialFinish.Chrome)
            {
                finalR = directR * (1f - reflectivity) + reflR * reflectivity * surfR;
                finalG = directG * (1f - reflectivity) + reflG * reflectivity * surfG;
                finalB = directB * (1f - reflectivity) + reflB * reflectivity * surfB;
            }
            else if (finish == MaterialFinish.Metal)
            {
                finalR = directR * (1f - reflectivity) + reflR * reflectivity * surfR;
                finalG = directG * (1f - reflectivity) + reflG * reflectivity * surfG;
                finalB = directB * (1f - reflectivity) + reflB * reflectivity * surfB;
            }
            else if (finish == MaterialFinish.Pearlescent)
            {
                finalR = directR * (1f - reflectivity) + reflR * reflectivity;
                finalG = directG * (1f - reflectivity) + reflG * reflectivity;
                finalB = directB * (1f - reflectivity) + reflB * reflectivity;
            }
            else
            {
                finalR = directR;
                finalG = directG;
                finalB = directB;
            }

            int ir = (int)(finalR * 255f);
            int ig = (int)(finalG * 255f);
            int ib = (int)(finalB * 255f);

            outR = (byte)(ir > 255 ? 255 : (ir < 0 ? 0 : ir));
            outG = (byte)(ig > 255 ? 255 : (ig < 0 ? 0 : ig));
            outB = (byte)(ib > 255 ? 255 : (ib < 0 ? 0 : ib));
        }


        /// <summary>
        /// Get PBR material properties from a MaterialFinish type.
        /// </summary>
        private static void GetMaterialProperties(MaterialFinish finish,
            out float reflectivity, out float specPower, out float diffuseFactor)
        {
            switch (finish)
            {
                case MaterialFinish.Chrome:
                    reflectivity = 0.85f;  // highly reflective mirror
                    specPower = 256f;      // very tight specular highlight
                    diffuseFactor = 0.15f; // minimal diffuse — mostly reflections
                    break;

                case MaterialFinish.Metal:
                    reflectivity = 0.5f;   // moderate reflections
                    specPower = 128f;      // sharp specular
                    diffuseFactor = 0.5f;  // balanced diffuse
                    break;

                case MaterialFinish.Pearlescent:
                    reflectivity = 0.25f;  // subtle iridescent sheen
                    specPower = 64f;       // soft specular
                    diffuseFactor = 0.75f; // mostly diffuse
                    break;

                case MaterialFinish.Rubber:
                    reflectivity = 0f;     // no reflections
                    specPower = 8f;        // very broad, soft highlights
                    diffuseFactor = 1.0f;  // fully diffuse
                    break;

                default: // Solid, Transparent, Milky, Glitter, Speckle
                    reflectivity = 0f;
                    specPower = 32f;       // standard Blinn-Phong
                    diffuseFactor = 1.0f;
                    break;
            }
        }


        /// <summary>
        /// Compute direct lighting (ambient + diffuse + specular + soft shadows).
        /// Returns floating-point colour in [0, 1] range.
        /// </summary>
        private static void ComputeDirectLighting(LightingModel lighting, BVH bvh,
            ref HitRecord hit, float hitX, float hitY, float hitZ,
            float eyeX, float eyeY, float eyeZ,
            MaterialFinish finish, float specPower, float diffuseFactor,
            int shadowSamples, Random rng,
            out float outR, out float outG, out float outB)
        {
            float nx = hit.NX, ny = hit.NY, nz = hit.NZ;
            float surfR = hit.R / 255f;
            float surfG = hit.G / 255f;
            float surfB = hit.B / 255f;

            //
            // Ambient contribution
            //
            float ambR = lighting.AmbientR * lighting.AmbientIntensity;
            float ambG = lighting.AmbientG * lighting.AmbientIntensity;
            float ambB = lighting.AmbientB * lighting.AmbientIntensity;

            float totalR = surfR * ambR;
            float totalG = surfG * ambG;
            float totalB = surfB * ambB;

            //
            // View direction
            //
            float viewX = eyeX - hitX;
            float viewY = eyeY - hitY;
            float viewZ = eyeZ - hitZ;
            Normalize(ref viewX, ref viewY, ref viewZ);

            float specIntensity = (finish == MaterialFinish.Rubber) ? 0f : lighting.SpecularIntensity;

            //
            // Jitter radius for soft shadows — wider = softer penumbra
            //
            const float SHADOW_JITTER = 0.015f;

            //
            // Accumulate per-light contributions
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
                // Multi-sample shadow test
                // Cast N jittered rays around the light direction and average
                //
                float shadowFactor;

                if (shadowSamples <= 1)
                {
                    // Single sample — hard shadows (fast path)
                    Ray sr = new Ray(
                        hitX + nx * 0.01f,
                        hitY + ny * 0.01f,
                        hitZ + nz * 0.01f,
                        lx, ly, lz);
                    shadowFactor = bvh.IntersectShadow(ref sr, shadowDist) ? 0f : 1f;
                }
                else
                {
                    int litCount = 0;
                    for (int s = 0; s < shadowSamples; s++)
                    {
                        // Jitter the shadow ray direction slightly
                        float jx = lx + (float)(rng.NextDouble() - 0.5) * SHADOW_JITTER * 2f;
                        float jy = ly + (float)(rng.NextDouble() - 0.5) * SHADOW_JITTER * 2f;
                        float jz = lz + (float)(rng.NextDouble() - 0.5) * SHADOW_JITTER * 2f;
                        Normalize(ref jx, ref jy, ref jz);

                        Ray sr = new Ray(
                            hitX + nx * 0.01f,
                            hitY + ny * 0.01f,
                            hitZ + nz * 0.01f,
                            jx, jy, jz);

                        if (!bvh.IntersectShadow(ref sr, shadowDist))
                            litCount++;
                    }
                    shadowFactor = (float)litCount / shadowSamples;
                }

                if (shadowFactor <= 0f) continue;

                // Diffuse
                float ndotl = nx * lx + ny * ly + nz * lz;
                if (ndotl < 0f) ndotl = 0f;

                totalR += surfR * ndotl * light.ColourR * light.Intensity * diffuseFactor * shadowFactor;
                totalG += surfG * ndotl * light.ColourG * light.Intensity * diffuseFactor * shadowFactor;
                totalB += surfB * ndotl * light.ColourB * light.Intensity * diffuseFactor * shadowFactor;

                // Specular
                if (specIntensity > 0f && ndotl > 0f)
                {
                    float hx = lx + viewX;
                    float hy = ly + viewY;
                    float hz = lz + viewZ;
                    Normalize(ref hx, ref hy, ref hz);

                    float ndoth = nx * hx + ny * hy + nz * hz;
                    if (ndoth > 0f)
                    {
                        float spec = MathF.Pow(ndoth, specPower);
                        float specContrib = spec * specIntensity * light.Intensity * shadowFactor;

                        if (finish == MaterialFinish.Chrome || finish == MaterialFinish.Metal)
                        {
                            totalR += specContrib * light.ColourR * surfR;
                            totalG += specContrib * light.ColourG * surfG;
                            totalB += specContrib * light.ColourB * surfB;
                        }
                        else
                        {
                            totalR += specContrib * light.ColourR;
                            totalG += specContrib * light.ColourG;
                            totalB += specContrib * light.ColourB;
                        }
                    }
                }
            }

            outR = totalR;
            outG = totalG;
            outB = totalB;
        }


        /// <summary>
        /// Compute ray-traced ambient occlusion at a hit point.
        /// Casts N rays in a cosine-weighted hemisphere around the normal.
        /// Returns occlusion factor: 0 = fully open, 1 = fully occluded.
        /// </summary>
        private static float ComputeAO(BVH bvh, ref HitRecord hit,
            float hitX, float hitY, float hitZ,
            int samples, float radius, Random rng)
        {
            float nx = hit.NX, ny = hit.NY, nz = hit.NZ;

            // Build tangent frame from surface normal
            float tx, ty, tz, bx, by, bz;
            BuildTangentFrame(nx, ny, nz, out tx, out ty, out tz, out bx, out by, out bz);

            int occluded = 0;

            for (int i = 0; i < samples; i++)
            {
                // Cosine-weighted hemisphere sample
                RandomOnHemisphere(rng, out float sx, out float sy, out float sz);

                // Transform from tangent space to world space
                float dirX = sx * tx + sy * nx + sz * bx;
                float dirY = sx * ty + sy * ny + sz * by;
                float dirZ = sx * tz + sy * nz + sz * bz;
                Normalize(ref dirX, ref dirY, ref dirZ);

                // Ensure direction is in the normal hemisphere
                float dot = dirX * nx + dirY * ny + dirZ * nz;
                if (dot < 0f) { dirX = -dirX; dirY = -dirY; dirZ = -dirZ; }

                Ray aoRay = new Ray(
                    hitX + nx * 0.02f,
                    hitY + ny * 0.02f,
                    hitZ + nz * 0.02f,
                    dirX, dirY, dirZ);

                if (bvh.IntersectShadow(ref aoRay, radius))
                {
                    occluded++;
                }
            }

            return (float)occluded / samples;
        }


        /// <summary>
        /// Generate a random point on the unit hemisphere (cosine-weighted).
        /// Uses the standard cosine-weighted sampling formula.
        /// </summary>
        private static void RandomOnHemisphere(Random rng, out float x, out float y, out float z)
        {
            float u1 = (float)rng.NextDouble();
            float u2 = (float)rng.NextDouble();

            // Cosine-weighted hemisphere sampling
            float sinTheta = MathF.Sqrt(1f - u1);
            float cosTheta = MathF.Sqrt(u1);
            float phi = 2f * MathF.PI * u2;

            x = sinTheta * MathF.Cos(phi);
            y = cosTheta;  // aligned with normal direction
            z = sinTheta * MathF.Sin(phi);
        }


        /// <summary>
        /// Build an orthogonal tangent frame from a normal vector.
        /// </summary>
        private static void BuildTangentFrame(float nx, float ny, float nz,
            out float tx, out float ty, out float tz,
            out float bx, out float by, out float bz)
        {
            // Choose a vector not parallel to normal
            float ux, uy, uz;
            if (MathF.Abs(nx) > 0.9f)
            {
                ux = 0; uy = 1; uz = 0;
            }
            else
            {
                ux = 1; uy = 0; uz = 0;
            }

            // Tangent = normalize(cross(normal, up))
            Cross(nx, ny, nz, ux, uy, uz, out tx, out ty, out tz);
            Normalize(ref tx, ref ty, ref tz);

            // Bitangent = cross(normal, tangent)
            Cross(nx, ny, nz, tx, ty, tz, out bx, out by, out bz);
            Normalize(ref bx, ref by, ref bz);
        }


        // ── Buffer management ──

        private void ClearBuffer()
        {
            if (_hasGradient)
            {
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
                        _colourBuffer[idx] = r;
                        _colourBuffer[idx + 1] = g;
                        _colourBuffer[idx + 2] = b;
                        _colourBuffer[idx + 3] = 255;
                    }
                }
            }
            else
            {
                for (int i = 0; i < _colourBuffer.Length; i += 4)
                {
                    _colourBuffer[i] = _bgR;
                    _colourBuffer[i + 1] = _bgG;
                    _colourBuffer[i + 2] = _bgB;
                    _colourBuffer[i + 3] = _bgA;
                }
            }
        }


        // ── Vector helpers ──

        private static void Normalize(ref float x, ref float y, ref float z)
        {
            float len = MathF.Sqrt(x * x + y * y + z * z);
            if (len > 1e-8f) { x /= len; y /= len; z /= len; }
        }

        private static void Cross(float ax, float ay, float az,
                                   float bx, float by, float bz,
                                   out float cx, out float cy, out float cz)
        {
            cx = ay * bz - az * by;
            cy = az * bx - ax * bz;
            cz = ax * by - ay * bx;
        }
    }
}
