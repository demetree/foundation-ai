using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// Foundation Transcription Provider Interface — abstracts speech-to-text services.
    /// 
    /// Phase 4 interface for future transcription support:
    ///   - Voicemail transcription
    ///   - Call recording transcription
    ///   - Real-time call captioning (if supported)
    /// 
    /// Possible implementations:
    ///   - Azure Cognitive Services Speech
    ///   - Google Cloud Speech-to-Text
    ///   - OpenAI Whisper
    ///   - Local Whisper model
    /// 
    /// AI-developed as part of Foundation.Messaging Phase 4 (Calling), March 2026.
    /// 
    /// </summary>
    public interface ITranscriptionProvider
    {
        /// <summary>
        /// Unique provider identifier (e.g., "azure-speech", "whisper").
        /// </summary>
        string ProviderId { get; }

        /// <summary>
        /// Whether this provider is enabled and configured.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Transcribes an audio file and returns the text.
        /// </summary>
        /// <param name="audioData">Raw audio bytes.</param>
        /// <param name="mimeType">Audio format (e.g., "audio/wav", "audio/webm").</param>
        /// <param name="languageCode">BCP-47 language code (e.g., "en-US"). Null for auto-detect.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Transcription result.</returns>
        Task<TranscriptionResult> TranscribeAsync(byte[] audioData, string mimeType, string languageCode = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates that the provider's configuration is correct.
        /// </summary>
        bool ValidateConfiguration();
    }


    /// <summary>
    /// Result of a transcription operation.
    /// </summary>
    public class TranscriptionResult
    {
        public bool Success { get; set; }
        public string Text { get; set; }
        public string LanguageCode { get; set; }
        public double? Confidence { get; set; }
        public string ErrorMessage { get; set; }
    }
}
