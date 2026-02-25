using System.Collections.Generic;

namespace BMC.LDraw.Models
{
    /// <summary>
    /// BFC certification state for a parsed file.
    /// </summary>
    public enum BfcCertification
    {
        /// <summary>No BFC statement found — winding is unknown.</summary>
        Unknown,
        /// <summary>File is BFC certified (0 BFC CERTIFY).</summary>
        Certified,
        /// <summary>File explicitly declares no BFC (0 BFC NOCERTIFY).</summary>
        NotCertified
    }

    /// <summary>
    /// All parsed geometry and metadata from a single LDraw file.
    /// Contains raw geometry (not yet resolved/flattened).
    /// </summary>
    public class LDrawGeometry
    {
        /// <summary>File name or model name.</summary>
        public string Name;

        /// <summary>BFC certification state.</summary>
        public BfcCertification BfcCertification = BfcCertification.Unknown;

        /// <summary>
        /// Whether the file uses counter-clockwise winding (true = CCW, false = CW).
        /// Only meaningful when BfcCertification == Certified.
        /// </summary>
        public bool WindingCCW = true;

        /// <summary>Type 1 — sub-file references (parts placed in this file).</summary>
        public List<LDrawSubfileReference> SubfileReferences = new List<LDrawSubfileReference>();

        /// <summary>Type 2 — line segments.</summary>
        public List<LDrawLine> Lines = new List<LDrawLine>();

        /// <summary>Type 3 — triangles.</summary>
        public List<LDrawTriangle> Triangles = new List<LDrawTriangle>();

        /// <summary>Type 4 — quads.</summary>
        public List<LDrawQuad> Quads = new List<LDrawQuad>();

        /// <summary>Type 5 — conditional lines (optional edges).</summary>
        public List<LDrawConditionalLine> ConditionalLines = new List<LDrawConditionalLine>();

        /// <summary>
        /// Indices of SubfileReferences that have INVERTNEXT applied.
        /// Used by the resolver to flip winding for the referenced subfile.
        /// </summary>
        public HashSet<int> InvertNextIndices = new HashSet<int>();

        /// <summary>
        /// Step boundaries from "0 STEP" meta-commands.
        /// Each entry is the SubfileReferences count at the point the STEP was parsed.
        /// For example, [3, 7, 10] means:
        ///   Step 0 = subfile refs 0..2,  Step 1 = refs 3..6,  Step 2 = refs 7..9.
        /// Cumulative: to render up to step N, resolve refs 0..StepBreaks[N]-1.
        /// </summary>
        public List<int> StepBreaks = new List<int>();

        /// <summary>
        /// Per-step camera rotation overrides from "0 ROTSTEP" meta-commands.
        /// Index corresponds to StepBreaks — StepRotations[i] applies to step i.
        /// A null entry means no rotation override for that step.
        /// </summary>
        public List<LDrawRotStep> StepRotations = new List<LDrawRotStep>();
    }


    /// <summary>
    /// Camera rotation override parsed from a "0 ROTSTEP x y z TYPE" meta-command.
    /// </summary>
    public class LDrawRotStep
    {
        /// <summary>Rotation around X axis in degrees.</summary>
        public float X;

        /// <summary>Rotation around Y axis in degrees.</summary>
        public float Y;

        /// <summary>Rotation around Z axis in degrees.</summary>
        public float Z;

        /// <summary>Rotation type: "ABS" (absolute), "REL" (relative), or "ADD".</summary>
        public string Type;
    }
}
