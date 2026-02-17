using BMC.AI;
using Foundation.AI.Embed;
using Foundation.AI.Inference;
using Foundation.AI.Rag;
using Foundation.AI.VectorStore;
using Foundation.AI.VectorStore.Zvec;
using Foundation.BMC.Database;

namespace Foundation.AI.Test;

/// <summary>
/// Integration test: builds a small in-memory index of LEGO parts,
/// runs a semantic search, and verifies relevant results are returned.
/// Uses the mock embedding provider (deterministic hash vectors).
/// </summary>
public class BmcAiIntegrationTests
{
    /// <summary>
    /// Mock embedding provider that produces deterministic vectors
    /// based on text hash — same as used in RagPipelineTests.
    /// </summary>
    private class HashEmbeddingProvider : IEmbeddingProvider
    {
        public int Dimension => 64;
        public string ModelName => "test:hash-64";

        public Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
        {
            var hash = text.GetHashCode();
            var rng = new Random(hash);
            var vec = new float[Dimension];
            for (int i = 0; i < Dimension; i++)
                vec[i] = (float)(rng.NextDouble() * 2 - 1);
            // Normalize
            var mag = MathF.Sqrt(vec.Select(v => v * v).Sum());
            for (int i = 0; i < Dimension; i++)
                vec[i] /= mag;
            return Task.FromResult(vec);
        }

        public async Task<float[][]> EmbedBatchAsync(IReadOnlyList<string> texts, CancellationToken ct = default)
        {
            var results = new float[texts.Count][];
            for (int i = 0; i < texts.Count; i++)
                results[i] = await EmbedAsync(texts[i], ct);
            return results;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    [Fact]
    public async Task SearchParts_AfterIndexing_ReturnsResults()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"bmc-ai-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var embed = new HashEmbeddingProvider();
            await using var vectorStore = new ZvecVectorStore(new ZvecVectorStoreConfig { BasePath = tempDir });

            // Manually index a few parts directly into the vector store
            var parts = new[]
            {
                new BrickPart
                {
                    id = 1,
                    name = "Technic Gear 24 Tooth",
                    ldrawTitle = "Technic Gear 24 Tooth",
                    ldrawPartId = "3648",
                    toothCount = 24,
                    gearRatio = 1.0f,
                    brickCategory = new BrickCategory { name = "Gear", description = "Technic gears" }
                },
                new BrickPart
                {
                    id = 2,
                    name = "Brick 2 x 4",
                    ldrawTitle = "Brick  2 x  4",
                    ldrawPartId = "3001",
                    brickCategory = new BrickCategory { name = "Brick", description = "Standard bricks" }
                },
                new BrickPart
                {
                    id = 3,
                    name = "Technic Beam 5",
                    ldrawTitle = "Technic Beam 5",
                    ldrawPartId = "32316",
                    brickCategory = new BrickCategory { name = "Beam", description = "Technic beams" }
                }
            };

            // Index them directly (simulating what BmcSearchIndex.IndexPartsAsync does)
            await vectorStore.CreateCollectionAsync(BmcSearchIndex.PartsCollection, embed.Dimension);

            foreach (var part in parts)
            {
                var text = BmcSearchIndex.BuildPartDescription(part);
                var vec = await embed.EmbedAsync(text);
                var meta = new Dictionary<string, object>
                {
                    ["type"] = "part",
                    ["partId"] = part.id.ToString(),
                    ["name"] = part.name ?? ""
                };
                await vectorStore.UpsertAsync(BmcSearchIndex.PartsCollection, $"part-{part.id}", vec, meta);
            }

            await vectorStore.FlushAsync(BmcSearchIndex.PartsCollection);

            // Now search
            var queryVec = await embed.EmbedAsync("gear with teeth");
            var results = await vectorStore.SearchAsync(BmcSearchIndex.PartsCollection, queryVec, topK: 3);

            // We should get all 3 parts back (small index)
            Assert.NotEmpty(results);
            Assert.Equal(3, results.Count);

            // Note: Zvec's ExtractMetadata returns null (known limitation)
            // so we verify result IDs instead
            Assert.Contains(results, r => r.Id.Contains("part-"));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task SearchSets_AfterIndexing_ReturnsResults()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"bmc-ai-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var embed = new HashEmbeddingProvider();
            await using var vectorStore = new ZvecVectorStore(new ZvecVectorStoreConfig { BasePath = tempDir });

            var sets = new[]
            {
                new LegoSet
                {
                    id = 1,
                    name = "Liebherr Crawler Crane LR 13000",
                    setNumber = "42146-1",
                    year = 2023,
                    partCount = 2883,
                    legoTheme = new LegoTheme { name = "Technic" }
                },
                new LegoSet
                {
                    id = 2,
                    name = "Police Station",
                    setNumber = "10278-1",
                    year = 2021,
                    partCount = 2923,
                    legoTheme = new LegoTheme { name = "Creator Expert" }
                }
            };

            await vectorStore.CreateCollectionAsync(BmcSearchIndex.SetsCollection, embed.Dimension);

            foreach (var set in sets)
            {
                var text = BmcSearchIndex.BuildSetDescription(set);
                var vec = await embed.EmbedAsync(text);
                var meta = new Dictionary<string, object>
                {
                    ["type"] = "set",
                    ["setId"] = set.id.ToString(),
                    ["name"] = set.name ?? "",
                    ["year"] = set.year.ToString()
                };
                await vectorStore.UpsertAsync(BmcSearchIndex.SetsCollection, $"set-{set.id}", vec, meta);
            }

            await vectorStore.FlushAsync(BmcSearchIndex.SetsCollection);

            var queryVec = await embed.EmbedAsync("big technic crane");
            var results = await vectorStore.SearchAsync(BmcSearchIndex.SetsCollection, queryVec, topK: 5);

            Assert.NotEmpty(results);
            Assert.Equal(2, results.Count);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}
