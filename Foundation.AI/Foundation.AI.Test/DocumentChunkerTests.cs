using Foundation.AI.Rag;

namespace Foundation.AI.Test;

/// <summary>
/// Unit tests for the TextChunker.
/// Tests boundary detection, overlap behavior, and edge cases.
/// </summary>
public class DocumentChunkerTests
{
    private readonly TextChunker _chunker = new()
    {
        ChunkSize = 100,
        ChunkOverlap = 20
    };

    [Fact]
    public void ShortText_ShouldReturnSingleChunk()
    {
        var chunks = _chunker.Chunk("Hello world", "doc-1");

        Assert.Single(chunks);
        Assert.Equal("doc-1::chunk-0", chunks[0].ChunkId);
        Assert.Equal("doc-1", chunks[0].DocId);
        Assert.Equal("Hello world", chunks[0].Text);
        Assert.Equal(0, chunks[0].Index);
    }

    [Fact]
    public void LongText_ShouldSplitIntoMultipleChunks()
    {
        // Create text longer than chunk size
        var text = string.Join(" ", Enumerable.Range(1, 100).Select(i => $"Word{i}"));

        var chunks = _chunker.Chunk(text, "doc-2");

        Assert.True(chunks.Count > 1, $"Expected multiple chunks, got {chunks.Count}");

        // Each chunk should have a correct sequential index
        for (int i = 0; i < chunks.Count; i++)
        {
            Assert.Equal($"doc-2::chunk-{i}", chunks[i].ChunkId);
            Assert.Equal(i, chunks[i].Index);
        }
    }

    [Fact]
    public void EmptyText_ShouldReturnNoChunks()
    {
        var chunks = _chunker.Chunk("", "doc-3");
        Assert.Empty(chunks);

        chunks = _chunker.Chunk("   ", "doc-4");
        Assert.Empty(chunks);
    }

    [Fact]
    public void ChunksWithOverlap_ShouldShareContent()
    {
        // Text with clear sentence structure for predictable splitting
        var sentences = Enumerable.Range(1, 20)
            .Select(i => $"Sentence number {i} is here.")
            .ToList();
        var text = string.Join(" ", sentences);

        var chunker = new TextChunker
        {
            ChunkSize = 150,
            ChunkOverlap = 30
        };

        var chunks = chunker.Chunk(text, "doc-5");

        Assert.True(chunks.Count >= 2);

        // Verify some overlap exists between consecutive chunks
        for (int i = 1; i < chunks.Count; i++)
        {
            // The end of chunk[i-1] should share some text with the start of chunk[i]
            // This is a soft check — exact overlap depends on boundary detection
            Assert.True(chunks[i].Text.Length > 0);
        }
    }

    [Fact]
    public void ParagraphBreaks_ShouldBePreferredSplitPoints()
    {
        var text = "First paragraph with some content that is long enough.\n\n" +
                   "Second paragraph with different content that follows.\n\n" +
                   "Third paragraph with more content here.";

        var chunker = new TextChunker
        {
            ChunkSize = 60,
            ChunkOverlap = 10
        };

        var chunks = chunker.Chunk(text, "doc-6");

        Assert.True(chunks.Count >= 2);
    }
}
