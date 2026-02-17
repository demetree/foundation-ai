using Foundation.AI.Embed;
using Foundation.AI.VectorStore;
using Foundation.BMC.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BMC.AI;

/// <summary>
/// Indexes BMC data (parts, sets, themes) into the Foundation.AI vector store
/// for semantic search and RAG retrieval.
///
/// Builds rich text descriptions from each record's fields so that
/// natural language queries like "red gear with 24 teeth" match on
/// meaning rather than exact keywords.
/// </summary>
public class BmcSearchIndex
{
    private readonly BMCContext _db;
    private readonly IEmbeddingProvider _embed;
    private readonly IVectorStore _vectorStore;
    private readonly ILogger<BmcSearchIndex> _logger;

    public const string PartsCollection = "bmc-parts";
    public const string SetsCollection = "bmc-sets";

    public BmcSearchIndex(
        BMCContext db,
        IEmbeddingProvider embed,
        IVectorStore vectorStore,
        ILogger<BmcSearchIndex> logger)
    {
        _db = db;
        _embed = embed;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    // ───────────────────────────────────────────────────────────
    //  Part Indexing
    // ───────────────────────────────────────────────────────────

    /// <summary>
    /// Indexes all active, non-deleted, user-visible brick parts into the vector store.
    /// Each part is embedded as a rich text description combining name,
    /// category, LDraw title, keywords, dimensions, and mechanical properties.
    /// </summary>
    public async Task IndexPartsAsync(int batchSize = 500, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting part indexing...");

        var parts = await _db.BrickParts
            .Include(p => p.brickCategory)
            .Include(p => p.partType)
            .Where(p => p.active && !p.deleted)
            .Where(p => p.partType != null && p.partType.isUserVisible)
            .AsNoTracking()
            .ToListAsync(ct);

        _logger.LogInformation("Found {Count} user-visible parts to index", parts.Count);

        // Build text + metadata, then batch embed
        var items = parts.Select(p => new
        {
            Id = $"part-{p.id}",
            Text = BuildPartDescription(p),
            Meta = new Dictionary<string, object>
            {
                ["type"] = "part",
                ["partId"] = p.id.ToString(),
                ["ldrawPartId"] = p.ldrawPartId ?? "",
                ["category"] = p.brickCategory?.name ?? "",
                ["name"] = p.name ?? ""
            }
        }).ToList();

        await EnsureCollectionAsync(PartsCollection);

        for (int i = 0; i < items.Count; i += batchSize)
        {
            var batch = items.Skip(i).Take(batchSize).ToList();
            var texts = batch.Select(b => b.Text).ToList();
            var embeddings = await _embed.EmbedBatchAsync(texts, ct);

            var docs = batch.Zip(embeddings, (b, vec) =>
                new VectorDocument(b.Id, vec, b.Meta)).ToList();

            await _vectorStore.UpsertBatchAsync(PartsCollection, docs, ct);
            _logger.LogInformation("Indexed parts {Start}-{End} of {Total}",
                i + 1, Math.Min(i + batchSize, items.Count), items.Count);
        }

        _logger.LogInformation("Part indexing complete: {Count} parts indexed", items.Count);
    }

    // ───────────────────────────────────────────────────────────
    //  Set Indexing
    // ───────────────────────────────────────────────────────────

    /// <summary>
    /// Indexes all active, non-deleted LEGO sets into the vector store.
    /// Each set is embedded as a rich text description combining name,
    /// set number, year, part count, theme, and related URLs.
    /// </summary>
    public async Task IndexSetsAsync(int batchSize = 500, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting set indexing...");

        var sets = await _db.LegoSets
            .Include(s => s.legoTheme)
            .Where(s => s.active && !s.deleted)
            .AsNoTracking()
            .ToListAsync(ct);

        _logger.LogInformation("Found {Count} sets to index", sets.Count);

        var items = sets.Select(s => new
        {
            Id = $"set-{s.id}",
            Text = BuildSetDescription(s),
            Meta = new Dictionary<string, object>
            {
                ["type"] = "set",
                ["setId"] = s.id.ToString(),
                ["setNumber"] = s.setNumber ?? "",
                ["year"] = s.year.ToString(),
                ["theme"] = s.legoTheme?.name ?? "",
                ["name"] = s.name ?? ""
            }
        }).ToList();

        await EnsureCollectionAsync(SetsCollection);

        for (int i = 0; i < items.Count; i += batchSize)
        {
            var batch = items.Skip(i).Take(batchSize).ToList();
            var texts = batch.Select(b => b.Text).ToList();
            var embeddings = await _embed.EmbedBatchAsync(texts, ct);

            var docs = batch.Zip(embeddings, (b, vec) =>
                new VectorDocument(b.Id, vec, b.Meta)).ToList();

            await _vectorStore.UpsertBatchAsync(SetsCollection, docs, ct);
            _logger.LogInformation("Indexed sets {Start}-{End} of {Total}",
                i + 1, Math.Min(i + batchSize, items.Count), items.Count);
        }

        _logger.LogInformation("Set indexing complete: {Count} sets indexed", items.Count);
    }

    // ───────────────────────────────────────────────────────────
    //  Full re-index
    // ───────────────────────────────────────────────────────────

    /// <summary>
    /// Indexes both parts and sets. Convenience wrapper for full re-index.
    /// </summary>
    public async Task IndexAllAsync(int batchSize = 500, CancellationToken ct = default)
    {
        await IndexPartsAsync(batchSize, ct);
        await IndexSetsAsync(batchSize, ct);
    }

    // ───────────────────────────────────────────────────────────
    //  Text description builders (public for unit testing)
    // ───────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a rich text description of a brick part suitable for embedding.
    /// Combines human-readable fields into a natural language paragraph that
    /// maximises semantic matching quality.
    /// </summary>
    public static string BuildPartDescription(BrickPart part)
    {
        var lines = new List<string>();

        // Primary identifier
        var title = !string.IsNullOrWhiteSpace(part.ldrawTitle)
            ? part.ldrawTitle
            : part.name;
        lines.Add(title);

        // Category
        if (part.brickCategory != null)
            lines.Add($"Category: {part.brickCategory.name} — {part.brickCategory.description}");

        // Part IDs for exact-match fallback
        if (!string.IsNullOrWhiteSpace(part.ldrawPartId))
            lines.Add($"LDraw ID: {part.ldrawPartId}");
        if (!string.IsNullOrWhiteSpace(part.rebrickablePartNum))
            lines.Add($"Rebrickable: {part.rebrickablePartNum}");

        // Keywords (already comma-separated from LDraw)
        if (!string.IsNullOrWhiteSpace(part.keywords))
            lines.Add($"Keywords: {part.keywords}");

        // Physical dimensions
        if (part.widthLdu.HasValue && part.heightLdu.HasValue && part.depthLdu.HasValue)
            lines.Add($"Dimensions: {part.widthLdu:F0} × {part.heightLdu:F0} × {part.depthLdu:F0} LDU");

        if (part.massGrams.HasValue)
            lines.Add($"Mass: {part.massGrams:F1}g");

        // Technic-specific
        if (part.toothCount.HasValue)
            lines.Add($"Teeth: {part.toothCount} (gear)");
        if (part.gearRatio.HasValue)
            lines.Add($"Gear ratio: {part.gearRatio:F2}");

        return string.Join(". ", lines);
    }

    /// <summary>
    /// Builds a rich text description of a LEGO set suitable for embedding.
    /// </summary>
    public static string BuildSetDescription(LegoSet set)
    {
        var lines = new List<string>();

        lines.Add($"{set.name} (Set {set.setNumber})");
        lines.Add($"Released: {set.year}");
        lines.Add($"Parts: {set.partCount}");

        if (set.legoTheme != null)
        {
            lines.Add($"Theme: {set.legoTheme.name}");
            if (!string.IsNullOrWhiteSpace(set.legoTheme.description))
                lines.Add(set.legoTheme.description);
        }

        return string.Join(". ", lines);
    }

    // ───────────────────────────────────────────────────────────
    //  Helpers
    // ───────────────────────────────────────────────────────────

    private async Task EnsureCollectionAsync(string name)
    {
        if (!await _vectorStore.CollectionExistsAsync(name))
        {
            await _vectorStore.CreateCollectionAsync(name, _embed.Dimension);
            _logger.LogInformation("Created vector collection '{Collection}' (dim={Dim})", name, _embed.Dimension);
        }
    }
}
