using System.Collections.Generic;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Abstraction for manual document output format.
    /// Implementations produce HTML, PDF, or other output from a sequence
    /// of build steps and rendered images.
    /// </summary>
    public interface IManualDocumentBuilder
    {
        /// <summary>
        /// Start a new document.
        /// </summary>
        /// <param name="modelName">Display name for the model.</param>
        /// <param name="plan">The complete build plan with all steps.</param>
        /// <param name="options">Manual generation options.</param>
        void BeginDocument(string modelName, ManualBuildPlan plan, ManualOptions options);

        /// <summary>
        /// Add the cover page with the final rendered model image.
        /// </summary>
        /// <param name="finalModelImage">PNG bytes of the completed model.</param>
        void AddCoverPage(byte[] finalModelImage);

        /// <summary>
        /// Begin a submodel callout section.
        /// All subsequent AddStep calls will be grouped in this callout
        /// until EndSubmodelCallout is called.
        /// </summary>
        /// <param name="submodelName">Display name of the submodel.</param>
        /// <param name="totalSubmodelSteps">Total steps in this submodel.</param>
        void BeginSubmodelCallout(string submodelName, int totalSubmodelSteps);

        /// <summary>
        /// Add a build step (works for both root steps and submodel steps).
        /// </summary>
        /// <param name="step">Step metadata.</param>
        /// <param name="stepImage">Rendered PNG image bytes for this step.</param>
        /// <param name="partImages">
        /// PLI thumbnail cache: key is "fileName|colourCode", value is PNG bytes.
        /// May be null if PLI rendering is disabled.
        /// </param>
        void AddStep(ManualBuildStep step, byte[] stepImage,
            Dictionary<string, byte[]> partImages);

        /// <summary>
        /// End the current submodel callout section.
        /// </summary>
        void EndSubmodelCallout();

        /// <summary>
        /// Add a Bill of Materials page summarising all unique parts.
        /// </summary>
        /// <param name="allUniqueParts">
        /// Aggregated list of unique part × colour entries with total quantities.
        /// </param>
        /// <param name="partImages">PLI thumbnail cache (same as AddStep).</param>
        void AddBillOfMaterials(List<StepPartInfo> allUniqueParts,
            Dictionary<string, byte[]> partImages);

        /// <summary>
        /// Finalize and return the assembled document.
        /// </summary>
        ManualGenerationResult Build();
    }
}
