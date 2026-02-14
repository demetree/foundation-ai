using System.Collections.Generic;

namespace BMC.LDraw.Models
{
    /// <summary>
    /// A parsed colour definition from LDConfig.ldr.
    /// </summary>
    public class LDrawColour
    {
        /// <summary>LDraw colour name (e.g. "Chrome_Gold", "Trans_Dark_Blue").</summary>
        public string Name;

        /// <summary>LDraw colour code number (e.g. 0 = Black, 4 = Red, 334 = Chrome Gold).</summary>
        public int Code;

        /// <summary>Hex RGB value (e.g. "#DFC176").</summary>
        public string HexValue;

        /// <summary>Hex edge/contrast colour for wireframe rendering (e.g. "#C2982E").</summary>
        public string HexEdge;

        /// <summary>Alpha transparency (0-255, 255 = fully opaque).</summary>
        public int Alpha;

        /// <summary>Material finish type: Solid, Chrome, Pearlescent, Metal, Rubber, Glitter, Speckle, Milky, Transparent.</summary>
        public string FinishType;

        /// <summary>Glow brightness for glow-in-the-dark colours (0-255). Null if not a glowing colour.</summary>
        public int? Luminance;

        /// <summary>Official LEGO colour number from LEGOID comment. Null if not specified.</summary>
        public int? LegoId;

        /// <summary>
        /// Whether this colour is transparent (derived from Alpha &lt; 255).
        /// </summary>
        public bool IsTransparent;

        /// <summary>
        /// Whether this colour has a metallic appearance (derived from FinishType being Chrome, Metal, or Pearlescent).
        /// </summary>
        public bool IsMetallic;
    }
}
