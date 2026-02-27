using System;
using System.Threading.Tasks;
using BMC.LDraw.Models;
using BMC.LDraw.Render;

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
        private float[] _depthBuffer;   // for edge detection
        private float[] _normalBuffer;  // nx, ny, nz per pixel

        // Background
        private byte _bgR, _bgG, _bgB, _bgA;
        private bool _hasGradient;
        private byte _gradTopR, _gradTopG, _gradTopB;
        private byte _gradBotR, _gradBotG, _gradBotB;


        // HDR accumulation buffer (3 floats per pixel: R, G, B in linear space)
        private float[] _hdrBuffer;
        private byte[] _alphaBuffer;


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

        /// <summary>Anti-aliasing samples per pixel (1 = off, 4 = 2×2, 9 = 3×3).</summary>
        public int AaSamples { get; set; } = 1;

        /// <summary>Whether to render edge lines via post-process edge detection.</summary>
        public bool RenderEdges { get; set; } = true;

        /// <summary>Colour of detected edges (0–255 grey value). Lower = darker edges.</summary>
        public byte EdgeDarkness { get; set; } = 40;


        // ── PBR Feature Toggles ──

        /// <summary>Enable PBR shading (Cook-Torrance GGX).  When false, uses legacy Blinn-Phong.</summary>
        public bool EnablePbr { get; set; } = true;

        /// <summary>Enable ACES filmic tone mapping + sRGB gamma. When false, clamps directly to [0, 255].</summary>
        public bool EnableToneMapping { get; set; } = true;

        /// <summary>Exposure multiplier for tone mapping.  Higher = brighter overall image.</summary>
        public float Exposure { get; set; } = 1.0f;

        /// <summary>Environment map for sky/reflections.  Null = use flat background colour.</summary>
        public IEnvironmentMap Environment { get; set; }


        public RayTraceRenderer(int width, int height)
        {
            _width = width;
            _height = height;
            _colourBuffer = new byte[width * height * 4];
            _depthBuffer = new float[width * height];
            _normalBuffer = new float[width * height * 3];
            _hdrBuffer = new float[width * height * 3];
            _alphaBuffer = new byte[width * height];
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
            int aaSamples = AaSamples;
            bool usePbr = EnablePbr;
            bool useToneMapping = EnableToneMapping;
            IEnvironmentMap envMap = Environment;

            // DoF settings
            float aperture = camera.Aperture;
            bool dofEnabled = aperture > 0f;
            float focusDist = dofEnabled ? camera.GetEffectiveFocusDistance() : 0f;

            // Precompute AA sub-pixel grid
            int aaGridSize = (int)MathF.Ceiling(MathF.Sqrt(aaSamples));
            if (aaGridSize < 1) aaGridSize = 1;
            float aaStep = 1f / aaGridSize;

            //
            // Initialize depth buffer to max
            //
            for (int i = 0; i < _depthBuffer.Length; i++)
                _depthBuffer[i] = float.MaxValue;

            //
            // Normalise background to [0, 1] floats for HDR mixing
            // (environment map may return HDR values > 1.0)
            //
            float rowBgRNorm = _bgR / 255f;
            float rowBgGNorm = _bgG / 255f;
            float rowBgBNorm = _bgB / 255f;

            //
            // Ray trace — one scanline at a time in parallel
            // Each thread gets its own RNG to avoid contention.
            //
            // Parallelism is capped at half the logical processors so that
            // a single render doesn't starve the web server, SignalR, or
            // other concurrent requests.  See RenderConcurrency for rationale.
            //
            var rtParallelOpts = new ParallelOptions
            {
                MaxDegreeOfParallelism = RenderConcurrency.MaxThreads
            };
            Parallel.For(0, _height, rtParallelOpts, () => new Random(System.Environment.CurrentManagedThreadId * 31 + System.Environment.TickCount),
                (y, state, rng) =>
            {
                // Compute per-scanline background colour (gradient or flat) as normalised floats
                float rowBgR, rowBgG, rowBgB;
                if (_hasGradient)
                {
                    float gt = (float)y / MathF.Max(_height - 1, 1);
                    rowBgR = (_gradTopR + (_gradBotR - _gradTopR) * gt) / 255f;
                    rowBgG = (_gradTopG + (_gradBotG - _gradTopG) * gt) / 255f;
                    rowBgB = (_gradTopB + (_gradBotB - _gradTopB) * gt) / 255f;
                }
                else
                {
                    rowBgR = _bgR / 255f;
                    rowBgG = _bgG / 255f;
                    rowBgB = _bgB / 255f;
                }

                for (int x = 0; x < _width; x++)
                {
                    float accumR = 0, accumG = 0, accumB = 0, accumA = 0;
                    float closestT = float.MaxValue;
                    float bestNX = 0, bestNY = 0, bestNZ = 0;
                    int sampleCount = 0;

                    for (int sy = 0; sy < aaGridSize; sy++)
                    {
                        for (int sx = 0; sx < aaGridSize; sx++)
                        {
                            // Jittered sub-pixel offset
                            float jitterX = (aaSamples > 1) ? (float)rng.NextDouble() * aaStep : 0.5f;
                            float jitterY = (aaSamples > 1) ? (float)rng.NextDouble() * aaStep : 0.5f;
                            float px = x + (sx + jitterX) * aaStep;
                            float py = y + (sy + jitterY) * aaStep;

                            float u = (2f * px / _width - 1f);
                            float v = (1f - 2f * py / _height);

                            Ray ray;

                            if (ortho)
                            {
                                float ox = eyeX + rightX * u * orthoHalfW + upX * v * orthoHalfH;
                                float oy = eyeY + rightY * u * orthoHalfW + upY * v * orthoHalfH;
                                float oz = eyeZ + rightZ * u * orthoHalfW + upZ * v * orthoHalfH;
                                ray = new Ray(ox, oy, oz, fwdX, fwdY, fwdZ);
                            }
                            else
                            {
                                float dirX = fwdX + rightX * u * halfW + upX * v * halfH;
                                float dirY = fwdY + rightY * u * halfW + upY * v * halfH;
                                float dirZ = fwdZ + rightZ * u * halfW + upZ * v * halfH;
                                Normalize(ref dirX, ref dirY, ref dirZ);
                                ray = new Ray(eyeX, eyeY, eyeZ, dirX, dirY, dirZ);
                            }
                            //
                            // Trace through transparent layers
                            // Front-to-back compositing: accumulate colour and reduce remaining opacity
                            //
                            const int MAX_TRANSPARENT_LAYERS = 8;
                            float layerR = 0, layerG = 0, layerB = 0;
                            float remainingAlpha = 1f;
                            float firstT = float.MaxValue;
                            float firstNX = 0, firstNY = 0, firstNZ = 0;

                            //
                            // DoF: jitter ray origin on aperture disk
                            //
                            Ray traceRay;

                            if (dofEnabled)
                            {
                                // Compute the point on the focal plane this ray aims at
                                float focusX, focusY, focusZ;

                                if (ortho)
                                {
                                    focusX = ray.OriginX + ray.DirX * focusDist;
                                    focusY = ray.OriginY + ray.DirY * focusDist;
                                    focusZ = ray.OriginZ + ray.DirZ * focusDist;
                                }
                                else
                                {
                                    focusX = eyeX + ray.DirX * focusDist;
                                    focusY = eyeY + ray.DirY * focusDist;
                                    focusZ = eyeZ + ray.DirZ * focusDist;
                                }

                                // Random point on aperture disk
                                float angle = (float)(rng.NextDouble() * 2.0 * Math.PI);
                                float radius = aperture * MathF.Sqrt((float)rng.NextDouble());
                                float offX = MathF.Cos(angle) * radius;
                                float offY = MathF.Sin(angle) * radius;

                                float newOriginX = ray.OriginX + rightX * offX + upX * offY;
                                float newOriginY = ray.OriginY + rightY * offX + upY * offY;
                                float newOriginZ = ray.OriginZ + rightZ * offX + upZ * offY;

                                float newDirX = focusX - newOriginX;
                                float newDirY = focusY - newOriginY;
                                float newDirZ = focusZ - newOriginZ;
                                Normalize(ref newDirX, ref newDirY, ref newDirZ);

                                traceRay = new Ray(newOriginX, newOriginY, newOriginZ,
                                                   newDirX, newDirY, newDirZ);
                            }
                            else
                            {
                                traceRay = ray;
                            }

                            for (int layer = 0; layer < MAX_TRANSPARENT_LAYERS; layer++)
                            {
                                if (!bvh.Intersect(ref traceRay, out HitRecord hit))
                                {
                                    //
                                    // Ray miss — sample environment or use background
                                    //
                                    if (envMap != null)
                                    {
                                        envMap.Sample(traceRay.DirX, traceRay.DirY, traceRay.DirZ,
                                                      out float envR, out float envG, out float envB);
                                        layerR += remainingAlpha * envR;
                                        layerG += remainingAlpha * envG;
                                        layerB += remainingAlpha * envB;
                                    }
                                    else
                                    {
                                        layerR += remainingAlpha * rowBgR;
                                        layerG += remainingAlpha * rowBgG;
                                        layerB += remainingAlpha * rowBgB;
                                    }
                                    break;
                                }

                                float hitX = traceRay.OriginX + traceRay.DirX * hit.T;
                                float hitY = traceRay.OriginY + traceRay.DirY * hit.T;
                                float hitZ = traceRay.OriginZ + traceRay.DirZ * hit.T;

                                MaterialFinish finish = bvh.Triangles[hit.TriIndex].Finish;

                                // Interpolate per-vertex normals for smooth shading
                                if (bvh.Triangles[hit.TriIndex].HasPerVertexNormals)
                                {
                                    ref MeshTriangle tri = ref bvh.Triangles[hit.TriIndex];
                                    float bw = 1f - hit.U - hit.V;
                                    float snx = bw * tri.NX1 + hit.U * tri.NX2 + hit.V * tri.NX3;
                                    float sny = bw * tri.NY1 + hit.U * tri.NY2 + hit.V * tri.NY3;
                                    float snz = bw * tri.NZ1 + hit.U * tri.NZ2 + hit.V * tri.NZ3;
                                    Normalize(ref snx, ref sny, ref snz);

                                    float ndotd = snx * traceRay.DirX + sny * traceRay.DirY + snz * traceRay.DirZ;
                                    if (ndotd > 0) { snx = -snx; sny = -sny; snz = -snz; }

                                    hit.NX = snx;
                                    hit.NY = sny;
                                    hit.NZ = snz;
                                }

                                //
                                // Shade: PBR or legacy Blinn-Phong
                                //
                                float sR, sG, sB;

                                if (usePbr)
                                {
                                    PbrMaterial mat = PbrMaterial.FromFinish(finish);
                                    PbrShading.Shade(lighting, bvh, ref hit,
                                        hitX, hitY, hitZ,
                                        eyeX, eyeY, eyeZ,
                                        mat, shadowSamples, rng,
                                        out sR, out sG, out sB);

                                    // Environment-based reflections for glossy/metallic surfaces
                                    if (envMap != null && mat.Metalness > 0.1f && layer == 0)
                                    {
                                        float nx2 = hit.NX, ny2 = hit.NY, nz2 = hit.NZ;
                                        float dDotn = traceRay.DirX * nx2 + traceRay.DirY * ny2 + traceRay.DirZ * nz2;
                                        float reflDirX = traceRay.DirX - 2f * dDotn * nx2;
                                        float reflDirY = traceRay.DirY - 2f * dDotn * ny2;
                                        float reflDirZ = traceRay.DirZ - 2f * dDotn * nz2;
                                        Normalize(ref reflDirX, ref reflDirY, ref reflDirZ);

                                        Ray reflRay = new Ray(
                                            hitX + nx2 * 0.01f, hitY + ny2 * 0.01f, hitZ + nz2 * 0.01f,
                                            reflDirX, reflDirY, reflDirZ);

                                        float envReflR, envReflG, envReflB;

                                        if (bvh.Intersect(ref reflRay, out HitRecord reflHit))
                                        {
                                            float rHitX = reflRay.OriginX + reflRay.DirX * reflHit.T;
                                            float rHitY = reflRay.OriginY + reflRay.DirY * reflHit.T;
                                            float rHitZ = reflRay.OriginZ + reflRay.DirZ * reflHit.T;
                                            PbrMaterial rMat = PbrMaterial.FromFinish(bvh.Triangles[reflHit.TriIndex].Finish);
                                            PbrShading.Shade(lighting, bvh, ref reflHit,
                                                rHitX, rHitY, rHitZ, hitX, hitY, hitZ,
                                                rMat, 1, rng, out envReflR, out envReflG, out envReflB);
                                        }
                                        else
                                        {
                                            envMap.Sample(reflDirX, reflDirY, reflDirZ,
                                                          out envReflR, out envReflG, out envReflB);
                                        }

                                        float reflectivity = mat.Metalness * (1f - mat.Roughness);
                                        float albedoR = hit.R / 255f;
                                        float albedoG = hit.G / 255f;
                                        float albedoB = hit.B / 255f;

                                        sR = sR * (1f - reflectivity) + envReflR * reflectivity * albedoR;
                                        sG = sG * (1f - reflectivity) + envReflG * reflectivity * albedoG;
                                        sB = sB * (1f - reflectivity) + envReflB * reflectivity * albedoB;
                                    }
                                }
                                else
                                {
                                    // Legacy Blinn-Phong path
                                    ShadePixel(lighting, bvh, ref traceRay, ref hit,
                                               hitX, hitY, hitZ,
                                               eyeX, eyeY, eyeZ,
                                               finish, 0,
                                               shadowSamples, aoSamples, aoRadius, aoIntensity, rng,
                                               out byte bpR, out byte bpG, out byte bpB);
                                    sR = bpR / 255f;
                                    sG = bpG / 255f;
                                    sB = bpB / 255f;
                                }

                                //
                                // Ambient occlusion (applied to both PBR and legacy)
                                //
                                if (aoSamples > 0 && aoIntensity > 0f && layer == 0)
                                {
                                    float ao = ComputeAO(bvh, ref hit, hitX, hitY, hitZ,
                                                         aoSamples, aoRadius, rng);
                                    float aoFactor = 1f - ao * aoIntensity;
                                    sR *= aoFactor;
                                    sG *= aoFactor;
                                    sB *= aoFactor;
                                }

                                // Track first hit for edge detection G-buffer
                                if (layer == 0)
                                {
                                    firstT = hit.T;
                                    firstNX = hit.NX;
                                    firstNY = hit.NY;
                                    firstNZ = hit.NZ;
                                }

                                float surfAlpha = hit.A / 255f;

                                if (surfAlpha >= 1f)
                                {
                                    layerR += remainingAlpha * sR;
                                    layerG += remainingAlpha * sG;
                                    layerB += remainingAlpha * sB;
                                    remainingAlpha = 0f;
                                    break;
                                }

                                // Transparent surface — blend and continue
                                layerR += remainingAlpha * surfAlpha * sR;
                                layerG += remainingAlpha * surfAlpha * sG;
                                layerB += remainingAlpha * surfAlpha * sB;
                                remainingAlpha *= (1f - surfAlpha);

                                if (remainingAlpha < 0.01f) break;

                                traceRay = new Ray(
                                    hitX + traceRay.DirX * 0.01f,
                                    hitY + traceRay.DirY * 0.01f,
                                    hitZ + traceRay.DirZ * 0.01f,
                                    traceRay.DirX, traceRay.DirY, traceRay.DirZ);
                            }

                            // If we still have remaining alpha, blend in background/env
                            if (remainingAlpha > 0.01f)
                            {
                                if (envMap != null)
                                {
                                    envMap.Sample(traceRay.DirX, traceRay.DirY, traceRay.DirZ,
                                                  out float ebgR, out float ebgG, out float ebgB);
                                    layerR += remainingAlpha * ebgR;
                                    layerG += remainingAlpha * ebgG;
                                    layerB += remainingAlpha * ebgB;
                                }
                                else
                                {
                                    layerR += remainingAlpha * rowBgR;
                                    layerG += remainingAlpha * rowBgG;
                                    layerB += remainingAlpha * rowBgB;
                                }
                            }

                            accumR += layerR;
                            accumG += layerG;
                            accumB += layerB;
                            accumA += (firstT < float.MaxValue) ? 255f : _bgA;

                            if (firstT < closestT)
                            {
                                closestT = firstT;
                                bestNX = firstNX;
                                bestNY = firstNY;
                                bestNZ = firstNZ;
                            }

                            sampleCount++;
                        }
                    }

                    // Average samples and write to HDR buffer
                    float invCount = 1f / sampleCount;
                    int pixIdx = y * _width + x;
                    int hdrIdx = pixIdx * 3;

                    _hdrBuffer[hdrIdx]     = accumR * invCount;
                    _hdrBuffer[hdrIdx + 1] = accumG * invCount;
                    _hdrBuffer[hdrIdx + 2] = accumB * invCount;
                    _alphaBuffer[pixIdx] = (byte)(accumA * invCount + 0.5f);

                    // Store G-buffer data for edge detection
                    _depthBuffer[pixIdx] = closestT;
                    int nBufIdx = pixIdx * 3;
                    _normalBuffer[nBufIdx]     = bestNX;
                    _normalBuffer[nBufIdx + 1] = bestNY;
                    _normalBuffer[nBufIdx + 2] = bestNZ;
                }

                return rng;
            },
            _ => { }); // finalizer

            //
            // Post-process: tone mapping (HDR → LDR byte buffer)
            //
            if (useToneMapping)
            {
                ToneMapper.Apply(_hdrBuffer, _alphaBuffer, _colourBuffer,
                                _width * _height, Exposure);
            }
            else
            {
                // Direct clamp (legacy behaviour)
                for (int i = 0; i < _width * _height; i++)
                {
                    int hIdx = i * 3;
                    int cIdx = i * 4;
                    int ir = (int)(_hdrBuffer[hIdx] * 255f + 0.5f);
                    int ig = (int)(_hdrBuffer[hIdx + 1] * 255f + 0.5f);
                    int ib = (int)(_hdrBuffer[hIdx + 2] * 255f + 0.5f);
                    _colourBuffer[cIdx]     = (byte)(ir > 255 ? 255 : (ir < 0 ? 0 : ir));
                    _colourBuffer[cIdx + 1] = (byte)(ig > 255 ? 255 : (ig < 0 ? 0 : ig));
                    _colourBuffer[cIdx + 2] = (byte)(ib > 255 ? 255 : (ib < 0 ? 0 : ib));
                    _colourBuffer[cIdx + 3] = _alphaBuffer[i];
                }
            }

            //
            // Post-process: edge detection
            //
            if (RenderEdges)
            {
                ApplyEdgeDetection();
            }

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


        /// <summary>
        /// Post-process: detect edges using depth and normal discontinuities
        /// and darken those pixels to simulate edge lines.
        /// </summary>
        private void ApplyEdgeDetection()
        {
            byte edgeVal = EdgeDarkness;

            // Thresholds for edge detection
            const float DEPTH_THRESHOLD = 0.02f;   // relative depth difference
            const float NORMAL_THRESHOLD = 0.3f;    // normal dot product difference

            for (int y = 1; y < _height - 1; y++)
            {
                for (int x = 1; x < _width - 1; x++)
                {
                    int idx = y * _width + x;
                    float centerDepth = _depthBuffer[idx];

                    // Skip background pixels
                    if (centerDepth >= float.MaxValue * 0.5f) continue;

                    bool isEdge = false;

                    // Check 4-connected neighbours for depth and normal discontinuities
                    int[] dx = { -1, 1, 0, 0 };
                    int[] dy = { 0, 0, -1, 1 };

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        int nIdx = ny * _width + nx;

                        float neighborDepth = _depthBuffer[nIdx];

                        // Depth edge: significant depth discontinuity
                        if (neighborDepth >= float.MaxValue * 0.5f)
                        {
                            // Neighbour is background — silhouette edge
                            isEdge = true;
                            break;
                        }

                        float depthDiff = MathF.Abs(centerDepth - neighborDepth) / MathF.Max(centerDepth, 1f);
                        if (depthDiff > DEPTH_THRESHOLD)
                        {
                            isEdge = true;
                            break;
                        }

                        // Normal edge: sharp crease between surfaces
                        int cnIdx = idx * 3;
                        int nnIdx = nIdx * 3;
                        float dot = _normalBuffer[cnIdx] * _normalBuffer[nnIdx]
                                  + _normalBuffer[cnIdx + 1] * _normalBuffer[nnIdx + 1]
                                  + _normalBuffer[cnIdx + 2] * _normalBuffer[nnIdx + 2];

                        if (dot < (1f - NORMAL_THRESHOLD))
                        {
                            isEdge = true;
                            break;
                        }
                    }

                    if (isEdge)
                    {
                        // Darken this pixel toward edge colour
                        int bufIdx = idx * 4;
                        _colourBuffer[bufIdx] = (byte)((_colourBuffer[bufIdx] + edgeVal) / 2);
                        _colourBuffer[bufIdx + 1] = (byte)((_colourBuffer[bufIdx + 1] + edgeVal) / 2);
                        _colourBuffer[bufIdx + 2] = (byte)((_colourBuffer[bufIdx + 2] + edgeVal) / 2);
                    }
                }
            }
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
