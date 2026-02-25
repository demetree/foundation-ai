using System.Collections.Generic;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// A complete, linearised build plan for an MPD file.
    /// Steps are flattened in render order — submodel steps appear inline
    /// before the root step that references them.
    /// </summary>
    public class ManualBuildPlan
    {
        /// <summary>Display name of the top-level model.</summary>
        public string ModelName;

        /// <summary>Flattened steps in render order (root + submodel interleaved).</summary>
        public List<ManualBuildStep> Steps = new List<ManualBuildStep>();

        /// <summary>Total number of steps in the plan.</summary>
        public int TotalSteps => Steps.Count;

        /// <summary>
        /// Names of distinct submodels that have their own build steps.
        /// Used to pre-resolve/pre-smooth each submodel's mesh.
        /// </summary>
        public List<string> SubmodelsWithSteps = new List<string>();
    }


    /// <summary>
    /// A single step in the manual, which may be a root step or a submodel step.
    /// </summary>
    public class ManualBuildStep
    {
        /// <summary>Global step number (1-based), shown in the output.</summary>
        public int GlobalStepIndex;

        /// <summary>Step index within its own model (0-based).</summary>
        public int LocalStepIndex;

        /// <summary>Which model/submodel this step belongs to.</summary>
        public string ModelName;

        /// <summary>True if this step is inside a submodel callout.</summary>
        public bool IsSubmodelStep;

        /// <summary>Name of the parent model (null for root model steps).</summary>
        public string ParentModelName;

        /// <summary>Nesting depth: 0 = root, 1 = first-level submodel, etc.</summary>
        public int SubmodelDepth;

        /// <summary>Parts newly added in this step (grouped by fileName + colourCode).</summary>
        public List<StepPartInfo> NewParts = new List<StepPartInfo>();

        /// <summary>Cumulative part count up to and including this step (within its model).</summary>
        public int CumulativePartCount;

        /// <summary>Camera rotation override from ROTSTEP (null = use defaults).</summary>
        public RotStepData RotStep;

        /// <summary>
        /// Computed optimal azimuth to face newly-added parts (null = use default).
        /// Set by the hub after mesh analysis; ignored when RotStep is present.
        /// </summary>
        public float? AutoAzimuth;
    }


    /// <summary>
    /// Camera rotation override from a ROTSTEP meta-command.
    /// </summary>
    public class RotStepData
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
