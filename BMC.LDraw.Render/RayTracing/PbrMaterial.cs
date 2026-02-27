using BMC.LDraw.Models;

namespace BMC.LDraw.Render.RayTracing
{
    /// <summary>
    /// PBR material parameters mapped from LDraw MaterialFinish types.
    ///
    /// Each LEGO material finish is characterised by its roughness (surface
    /// micro-detail), metalness (conductor vs dielectric), and base reflectance
    /// at normal incidence (F0).  These feed into the Cook-Torrance GGX BRDF
    /// in <see cref="PbrShading"/>.
    /// </summary>
    public struct PbrMaterial
    {
        /// <summary>Surface roughness: 0 = perfect mirror, 1 = fully matte.</summary>
        public float Roughness;

        /// <summary>Metalness: 0 = dielectric (plastic), 1 = conductor (metal).</summary>
        public float Metalness;

        /// <summary>
        /// Base reflectance at normal incidence.
        /// Dielectrics are typically ~0.04; metals use the albedo colour.
        /// </summary>
        public float F0;

        /// <summary>
        /// IOR-derived reflectance boost for transparent/milky finishes.
        /// 0 = no subsurface effect, > 0 = slight translucency glow.
        /// </summary>
        public float Translucency;


        /// <summary>
        /// Map a <see cref="MaterialFinish"/> to PBR parameters.
        /// </summary>
        public static PbrMaterial FromFinish(MaterialFinish finish)
        {
            switch (finish)
            {
                case MaterialFinish.Chrome:
                    return new PbrMaterial
                    {
                        Roughness =   0.05f,  // near-perfect mirror
                        Metalness =   1.0f,   // fully metallic
                        F0 =          0.95f,  // very high base reflectance
                        Translucency = 0f
                    };

                case MaterialFinish.Metal:
                    return new PbrMaterial
                    {
                        Roughness =   0.20f,  // slightly rough metal
                        Metalness =   0.80f,  // mostly metallic
                        F0 =          0.60f,
                        Translucency = 0f
                    };

                case MaterialFinish.Pearlescent:
                    return new PbrMaterial
                    {
                        Roughness =   0.30f,  // soft sheen
                        Metalness =   0.15f,  // slight metallic tint
                        F0 =          0.08f,  // subtle iridescent reflectance
                        Translucency = 0f
                    };

                case MaterialFinish.Rubber:
                    return new PbrMaterial
                    {
                        Roughness =   0.90f,  // very matte
                        Metalness =   0.0f,   // fully dielectric
                        F0 =          0.02f,  // minimal reflectance
                        Translucency = 0f
                    };

                case MaterialFinish.Transparent:
                    return new PbrMaterial
                    {
                        Roughness =   0.10f,  // smooth transparent plastic
                        Metalness =   0.0f,
                        F0 =          0.04f,
                        Translucency = 0.3f   // light passes through edges
                    };

                case MaterialFinish.Milky:
                    return new PbrMaterial
                    {
                        Roughness =   0.35f,  // frosted look
                        Metalness =   0.0f,
                        F0 =          0.04f,
                        Translucency = 0.5f   // significant translucency
                    };

                case MaterialFinish.Glitter:
                    return new PbrMaterial
                    {
                        Roughness =   0.25f,  // sparkly highlights
                        Metalness =   0.3f,   // embedded metallic flakes
                        F0 =          0.10f,
                        Translucency = 0.1f
                    };

                case MaterialFinish.Speckle:
                    return new PbrMaterial
                    {
                        Roughness =   0.40f,
                        Metalness =   0.2f,
                        F0 =          0.06f,
                        Translucency = 0f
                    };

                default: // Solid — standard LEGO ABS plastic
                    return new PbrMaterial
                    {
                        Roughness =   0.40f,  // slight gloss (ABS plastic)
                        Metalness =   0.0f,   // fully dielectric
                        F0 =          0.04f,  // standard plastic reflectance
                        Translucency = 0f
                    };
            }
        }
    }
}
