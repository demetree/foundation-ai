using Foundation.AI.Rag;

namespace Foundation.AI.MarkItDown;

/// <summary>
/// Convenience extension methods that bridge MarkItDown and RAG services,
/// enabling single-call file-to-vector indexing.
///
/// <para><b>Usage:</b>
/// <code>
/// // Both IMarkItDown and IRagService must be registered in DI
/// var markItDown = serviceProvider.GetRequiredService&lt;IMarkItDown&gt;();
/// var ragService = serviceProvider.GetRequiredService&lt;IRagService&gt;();
///
/// // Convert file to Markdown, then chunk + embed + store in one call
/// await ragService.IndexFileAsync(markItDown, "policies", "doc-1", "handbook.pdf");
/// </code></para>
///
/// <para><b>AI-developed:</b> Bridge between Foundation.AI.MarkItDown and Foundation.AI.Rag.</para>
/// </summary>
public static class RagMarkItDownExtensions
{
    /// <summary>
    /// Convert a file to Markdown using MarkItDown, then index it into the RAG pipeline.
    ///
    /// <para>Pipeline: File -> MarkItDown (Markdown) -> TextChunker -> Embedder -> VectorStore</para>
    /// </summary>
    /// <param name="ragService">The RAG service instance.</param>
    /// <param name="markItDown">The MarkItDown conversion service.</param>
    /// <param name="collection">Vector collection name for storage.</param>
    /// <param name="docId">Unique document identifier (used as prefix for chunk IDs).</param>
    /// <param name="filePath">Path to the file to convert and index.</param>
    /// <param name="metadata">Optional metadata applied to all chunks.</param>
    /// <param name="ct">Cancellation token.</param>
    public static async Task IndexFileAsync(
        this IRagService ragService,
        IMarkItDown markItDown,
        string collection,
        string docId,
        string filePath,
        Dictionary<string, object>? metadata = null,
        CancellationToken ct = default)
    {
        //
        // Convert the file to Markdown
        //
        ConversionResult conversionResult = await markItDown.ConvertFileAsync(filePath, ct);

        //
        // Merge conversion metadata with caller-provided metadata
        //
        Dictionary<string, object> mergedMetadata = new(metadata ?? new());
        mergedMetadata.TryAdd("source_file", Path.GetFileName(filePath));
        mergedMetadata.TryAdd("source_format", Path.GetExtension(filePath));

        if (conversionResult.Title != null)
        {
            mergedMetadata.TryAdd("title", conversionResult.Title);
        }

        //
        // Index the Markdown content through the RAG pipeline
        // (chunk -> embed -> store)
        //
        await ragService.IndexDocumentAsync(
            collection: collection,
            docId: docId,
            text: conversionResult.Markdown,
            metadata: mergedMetadata,
            ct: ct);
    }


    /// <summary>
    /// Convert a stream to Markdown using MarkItDown, then index it into the RAG pipeline.
    /// </summary>
    /// <param name="ragService">The RAG service instance.</param>
    /// <param name="markItDown">The MarkItDown conversion service.</param>
    /// <param name="collection">Vector collection name for storage.</param>
    /// <param name="docId">Unique document identifier.</param>
    /// <param name="stream">The input file stream.</param>
    /// <param name="streamInfo">Optional format hints (MIME type, extension, filename).</param>
    /// <param name="metadata">Optional metadata applied to all chunks.</param>
    /// <param name="ct">Cancellation token.</param>
    public static async Task IndexStreamAsync(
        this IRagService ragService,
        IMarkItDown markItDown,
        string collection,
        string docId,
        Stream stream,
        StreamInfo? streamInfo = null,
        Dictionary<string, object>? metadata = null,
        CancellationToken ct = default)
    {
        //
        // Convert the stream to Markdown
        //
        ConversionResult conversionResult = await markItDown.ConvertAsync(stream, streamInfo, ct);

        //
        // Merge conversion metadata with caller-provided metadata
        //
        Dictionary<string, object> mergedMetadata = new(metadata ?? new());

        if (streamInfo?.FileName != null)
        {
            mergedMetadata.TryAdd("source_file", streamInfo.FileName);
        }

        if (streamInfo?.Extension != null)
        {
            mergedMetadata.TryAdd("source_format", streamInfo.Extension);
        }

        if (conversionResult.Title != null)
        {
            mergedMetadata.TryAdd("title", conversionResult.Title);
        }

        //
        // Index through the RAG pipeline
        //
        await ragService.IndexDocumentAsync(
            collection: collection,
            docId: docId,
            text: conversionResult.Markdown,
            metadata: mergedMetadata,
            ct: ct);
    }
}
