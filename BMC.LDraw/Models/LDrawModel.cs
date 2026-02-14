using System.Collections.Generic;

namespace BMC.LDraw.Models
{
    /// <summary>
    /// A parsed LDraw model file (.ldr or .mpd).
    /// Contains metadata and a sequence of build steps.
    /// </summary>
    public class LDrawModel
    {
        /// <summary>Model name (from header or FILE command).</summary>
        public string Name;

        /// <summary>Model author.</summary>
        public string Author;

        /// <summary>
        /// Ordered list of build steps. Each step contains the parts placed in that step.
        /// If no STEP commands are present, all parts are in a single step.
        /// </summary>
        public List<LDrawStep> Steps = new List<LDrawStep>();
    }

    /// <summary>
    /// A single build step within an LDraw model.
    /// Grouped by "0 STEP" commands in the source file.
    /// </summary>
    public class LDrawStep
    {
        /// <summary>Sub-file references (placed parts) in this build step.</summary>
        public List<LDrawSubfileReference> Parts = new List<LDrawSubfileReference>();
    }
}
