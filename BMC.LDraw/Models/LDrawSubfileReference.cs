using System.Collections.Generic;

namespace BMC.LDraw.Models
{
    /// <summary>
    /// A Type 1 line — a reference to a sub-file placed within a model or part.
    /// Represents a single placed part instance with position and orientation.
    /// </summary>
    public class LDrawSubfileReference
    {
        /// <summary>LDraw colour code for this instance.</summary>
        public int ColourCode;

        /// <summary>X position in LDraw units.</summary>
        public float X;

        /// <summary>Y position in LDraw units.</summary>
        public float Y;

        /// <summary>Z position in LDraw units.</summary>
        public float Z;

        /// <summary>
        /// 3x3 transformation matrix as 9 floats in row-major order:
        /// [a, b, c, d, e, f, g, h, i] representing the matrix:
        ///   | a b c |
        ///   | d e f |
        ///   | g h i |
        /// </summary>
        public float[] Matrix = new float[9];

        /// <summary>Referenced sub-file name (e.g. "3001.dat").</summary>
        public string FileName;

        // ─── LDCad Group Membership (stub for future CAD editor) ─────────

        /// <summary>
        /// LDCad group local IDs this part belongs to (from "0 !LDCAD GROUP_NXT").
        /// Empty if the part is not grouped. Multiple IDs = part belongs to multiple groups.
        /// </summary>
        public List<int> GroupLocalIds;
    }
}
