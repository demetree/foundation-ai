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

        // ─── LPub3D Document-Level Metadata ──────────────────────────────
        //
        // Populated from "0 !LPUB PAGE SIZE", "0 !LPUB PAGE ORIENTATION",
        // and "0 !LPUB INSERT COVER_PAGE" meta-commands.
        //

        /// <summary>Page width in mm (from "0 !LPUB PAGE SIZE w h").</summary>
        public float? PageWidthMm;

        /// <summary>Page height in mm (from "0 !LPUB PAGE SIZE w h").</summary>
        public float? PageHeightMm;

        /// <summary>Landscape orientation (from "0 !LPUB PAGE ORIENTATION").</summary>
        public bool? Landscape;

        /// <summary>Whether a cover page was requested (from "0 !LPUB INSERT COVER_PAGE").</summary>
        public bool HasCoverPage;

        /// <summary>Default page background colour hex (from "0 !LPUB PAGE BACKGROUND COLOR").</summary>
        public string PageBackgroundColorHex;

        /// <summary>Whether a Bill of Materials page was requested (from "0 !LPUB INSERT BOM").</summary>
        public bool HasBillOfMaterials;

        // ─── LDCad Metadata (stubs for future CAD editor) ────────────────

        /// <summary>
        /// LDCad group definitions parsed from "0 !LDCAD GROUP_DEF".
        /// Key = GID (global identifier string), Value = group info.
        /// Populated during import for future use by the CAD editor.
        /// </summary>
        public Dictionary<string, LDCadGroup> Groups = new Dictionary<string, LDCadGroup>();

        /// <summary>Whether the file contains "0 !LDCAD GENERATED" fallback sections.</summary>
        public bool HasGeneratedFallback;
    }

    /// <summary>
    /// An LDCad group definition, parsed from "0 !LDCAD GROUP_DEF".
    /// Represents a logical grouping of parts authored in LDCad.
    /// Stored for future use by the BMC CAD editor.
    /// </summary>
    public class LDCadGroup
    {
        /// <summary>Global group identifier (unique string like "EjSe9B4luQa").</summary>
        public string GID;

        /// <summary>Human-readable group name (e.g. "Wheel Assembly").</summary>
        public string Name;

        /// <summary>Whether this is a top-level group (not nested inside another).</summary>
        public bool TopLevel;

        /// <summary>Local numeric ID used within a single file to reference this group.</summary>
        public int LocalId;

        /// <summary>Group center position as [x, y, z] in LDraw coordinates (optional).</summary>
        public float[] Center;
    }

    /// <summary>
    /// A single build step within an LDraw model.
    /// Grouped by "0 STEP" or "0 ROTSTEP" commands in the source file.
    /// </summary>
    public class LDrawStep
    {
        /// <summary>Sub-file references (placed parts) in this build step.</summary>
        public List<LDrawSubfileReference> Parts = new List<LDrawSubfileReference>();

        // ─── LPub3D / LDraw Step-Level Metadata ─────────────────────────
        //
        // Populated from "0 ROTSTEP", "0 !LPUB FADE_PREV_STEP",
        // and "0 !LPUB INSERT PAGE" meta-commands.
        //

        /// <summary>Camera rotation X from "0 ROTSTEP x y z TYPE".</summary>
        public float? RotStepX;

        /// <summary>Camera rotation Y from "0 ROTSTEP x y z TYPE".</summary>
        public float? RotStepY;

        /// <summary>Camera rotation Z from "0 ROTSTEP x y z TYPE".</summary>
        public float? RotStepZ;

        /// <summary>Rotation type: "ABS", "REL", or "ADD".</summary>
        public string RotStepType;

        /// <summary>Fade previous step parts (from "0 !LPUB FADE_PREV_STEP ON|OFF").</summary>
        public bool? FadePrevStep;

        /// <summary>Force a page break before this step (from "0 !LPUB INSERT PAGE").</summary>
        public bool IsPageBreak;

        // ─── Tier 2 LPub3D Meta-Commands ─────────────────────────────────

        /// <summary>Whether this step is a callout (submodel assembly) from "0 !LPUB CALLOUT BEGIN".</summary>
        public bool IsCallout;

        /// <summary>Name of the submodel referenced by this callout.</summary>
        public string CalloutModelName;

        /// <summary>Whether to show the Parts List Image for this step (from "0 !LPUB PLI PER_STEP").</summary>
        public bool? ShowPartsListImage;
    }
}
