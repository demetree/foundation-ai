using System;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Stub PDF manual builder.
    /// Phase 1 implementation — produces a placeholder result.
    /// Will be fully implemented with PDFSharp in a future phase.
    /// </summary>
    public class PdfManualBuilder : IManualDocumentBuilder
    {
        private ManualBuildPlan _plan;
        private int _totalParts;

        public void BeginDocument(string modelName, ManualBuildPlan plan, ManualOptions options)
        {
            _plan = plan;
        }

        public void AddCoverPage(byte[] finalModelImage)
        {
            // PDF generation not yet implemented
        }

        public void BeginSubmodelCallout(string submodelName, int totalSubmodelSteps)
        {
            // PDF generation not yet implemented
        }

        public void AddStep(ManualBuildStep step, byte[] stepImage)
        {
            _totalParts = Math.Max(_totalParts, step.CumulativePartCount);
        }

        public void EndSubmodelCallout()
        {
            // PDF generation not yet implemented
        }

        public ManualGenerationResult Build()
        {
            return new ManualGenerationResult
            {
                Html = null,
                DocumentBytes = null, // Will contain PDF bytes when implemented
                TotalSteps = _plan?.TotalSteps ?? 0,
                TotalParts = _totalParts
            };
        }
    }
}
