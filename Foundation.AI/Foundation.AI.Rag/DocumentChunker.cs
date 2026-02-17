namespace Foundation.AI.Rag;

/// <summary>
/// Splits documents into overlapping chunks suitable for embedding and retrieval.
/// </summary>
public interface IDocumentChunker
{
    /// <summary>
    /// Split a document into chunks.
    /// </summary>
    /// <param name="text">Full document text.</param>
    /// <param name="docId">Document identifier (used to generate chunk IDs).</param>
    /// <returns>Ordered list of chunks with IDs and text.</returns>
    IReadOnlyList<DocumentChunk> Chunk(string text, string docId);
}

/// <summary>A chunk of a document, ready for embedding.</summary>
/// <param name="ChunkId">Unique ID: "{docId}::chunk-{index}"</param>
/// <param name="DocId">Parent document ID.</param>
/// <param name="Text">Chunk text content.</param>
/// <param name="Index">Zero-based chunk index within the document.</param>
public record DocumentChunk(string ChunkId, string DocId, string Text, int Index);

/// <summary>
/// Character-based document chunker with configurable size and overlap.
///
/// <para><b>Strategy:</b>
/// Splits text at natural boundaries (paragraph breaks, sentence endings, whitespace)
/// into chunks of approximately <see cref="ChunkSize"/> characters with
/// <see cref="ChunkOverlap"/> characters of overlap between consecutive chunks.</para>
///
/// <para><b>Why overlap?</b>
/// Overlap ensures that information at chunk boundaries isn't lost.
/// A sentence split across two chunks will appear in full in at least one.</para>
/// </summary>
public sealed class TextChunker : IDocumentChunker
{
    /// <summary>Target chunk size in characters. Default: 1000.</summary>
    public int ChunkSize { get; set; } = 1000;

    /// <summary>Overlap between chunks in characters. Default: 200.</summary>
    public int ChunkOverlap { get; set; } = 200;

    public IReadOnlyList<DocumentChunk> Chunk(string text, string docId)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        var chunks = new List<DocumentChunk>();
        int pos = 0;
        int index = 0;

        while (pos < text.Length)
        {
            int end = Math.Min(pos + ChunkSize, text.Length);

            // Try to break at a natural boundary (paragraph, sentence, whitespace)
            if (end < text.Length)
                end = FindBreakPoint(text, pos, end);

            var chunkText = text[pos..end].Trim();

            if (chunkText.Length > 0)
            {
                chunks.Add(new DocumentChunk(
                    ChunkId: $"{docId}::chunk-{index}",
                    DocId: docId,
                    Text: chunkText,
                    Index: index));
                index++;
            }

            // Advance with overlap
            int advance = end - pos - ChunkOverlap;
            if (advance <= 0) advance = end - pos; // prevent infinite loop
            pos += advance;
        }

        return chunks;
    }

    /// <summary>
    /// Find the best break point near the target end position.
    /// Preference: paragraph break > sentence end > whitespace > exact position.
    /// </summary>
    private static int FindBreakPoint(string text, int start, int targetEnd)
    {
        // Search backward from targetEnd for a good break point
        int searchStart = Math.Max(start, targetEnd - 200);

        // Look for paragraph break (double newline)
        int paraBreak = text.LastIndexOf("\n\n", targetEnd, targetEnd - searchStart);
        if (paraBreak > searchStart)
            return paraBreak + 2; // include the break

        // Look for sentence ending (. ! ?)
        for (int i = targetEnd - 1; i >= searchStart; i--)
        {
            if ((text[i] == '.' || text[i] == '!' || text[i] == '?') &&
                i + 1 < text.Length && char.IsWhiteSpace(text[i + 1]))
            {
                return i + 1;
            }
        }

        // Look for whitespace
        int lastSpace = text.LastIndexOf(' ', targetEnd - 1, targetEnd - searchStart);
        if (lastSpace > searchStart)
            return lastSpace + 1;

        return targetEnd;
    }
}
