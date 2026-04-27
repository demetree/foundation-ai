using System.Text;
using System.Text.Json;
using Foundation.AI.Embed;
using Foundation.AI.Inference;
using Foundation.AI.MarkItDown;
using Foundation.AI.VectorStore;

namespace Foundation.AI.Examples.AgentLoop;

// The five tools the agent has at its disposal, plus the dispatcher that
// routes incoming FunctionCalls to their handlers.
//
// Tool surface design rationale:
//
//   list_topics    -- the agent's "table of contents" view. Cheap, gives the
//                     model an entry point when it has no other context.
//   read_doc       -- get the full text of one document. The "drill in" move.
//   search_excerpt -- semantic search across all chunks; equivalent to what
//                     DocsQ does internally for one-shot RAG. Lets the agent
//                     find passages without reading whole docs.
//   compare        -- LLM-on-LLM: re-enters the same inference provider with
//                     a structured comparison prompt. The "thinking tool" --
//                     this is the most ambitious of the five, and the one
//                     that most clearly shows multi-hop budget cost.
//   finalize       -- terminal. Sets KbContext.TerminalStatus = "answered"
//                     and ends the loop.
//
// All tool I/O is via plain strings: JSON arguments in, agent-friendly text
// out. Agents do not see exceptions -- those are caught and surfaced as a
// human-readable string so the model can recover by trying a different tool.
public sealed class KbTools
{
    private const string CollectionName = "agentloop-kb";

    private readonly IEmbeddingProvider _embedder;
    private readonly IVectorStore _store;
    private readonly IInferenceProvider _inference;
    private readonly IMarkItDown _markItDown;

    // name (lowercased, with or without extension) -> doc record. Lookups in
    // read_doc / compare go through this map so the agent can name a doc
    // either way.
    private readonly Dictionary<string, KbDoc> _docs =
        new(StringComparer.OrdinalIgnoreCase);

    public KbTools(IEmbeddingProvider embedder, IVectorStore store, IInferenceProvider inference, IMarkItDown markItDown)
    {
        _embedder = embedder;
        _store = store;
        _inference = inference;
        _markItDown = markItDown;
    }

    // ─── Indexing (called once at startup from Program.cs) ─────────────────
    //
    // Reads every .md file under corpusDir, splits each into paragraph chunks,
    // embeds the chunks, and stores them in a Zvec collection. Also captures
    // each doc's first heading + first paragraph for use by list_topics.
    public async Task IndexCorpusAsync(string corpusDir, CancellationToken ct = default)
    {
        if (!Directory.Exists(corpusDir))
            throw new DirectoryNotFoundException($"Corpus not found: {corpusDir}");

        string[] files = Directory.GetFiles(corpusDir, "*", SearchOption.TopDirectoryOnly)
            .Where(IsSupported)
            .OrderBy(f => f)
            .ToArray();
        if (files.Length == 0)
            throw new InvalidOperationException(
                $"No supported files in corpus dir: {corpusDir}. " +
                "Drop .pdf, .docx, .pptx, .xlsx, .html, .md, .txt, .csv, .json, or .xml files in there and re-run.");

        // Build the in-memory doc index first -- list_topics and read_doc
        // serve straight from this without touching Zvec at all.
        //
        // Every file routes through MarkItDown so the corpus can mix formats
        // -- PDF / Office / HTML / etc. -- not just markdown. For .md / .txt
        // this is effectively a pass-through; for the heavier formats it's
        // where the conversion happens.
        var allChunks = new List<(string DocName, int ChunkIdx, string Text)>();
        foreach (string file in files)
        {
            string name = Path.GetFileName(file);
            string text;
            try
            {
                var converted = await _markItDown.ConvertFileAsync(file, ct);
                text = converted.Markdown;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ! KbTools: skipping {name}: {ex.Message}");
                continue;
            }

            if (string.IsNullOrWhiteSpace(text)) continue;

            string title = ExtractTitle(text);
            string summary = ExtractSummary(text);

            _docs[name] = new KbDoc
            {
                Name = name,
                Title = title,
                Summary = summary,
                Text = text
            };
            // Also accept name without extension.
            string nameNoExt = Path.GetFileNameWithoutExtension(name);
            if (!_docs.ContainsKey(nameNoExt))
                _docs[nameNoExt] = _docs[name];

            // Paragraph-level chunks (split on blank lines). Tiny chunks
            // discarded so single-line headings don't pollute search results.
            int chunkIdx = 0;
            foreach (string para in text.Split("\n\n", StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = para.Trim();
                if (trimmed.Length < 80) continue;
                allChunks.Add((name, chunkIdx++, trimmed));
            }
        }

        // (Re)create the collection with the right dimensionality.
        if (await _store.CollectionExistsAsync(CollectionName, ct))
            await _store.DeleteCollectionAsync(CollectionName, ct);
        await _store.CreateCollectionAsync(CollectionName, _embedder.Dimension, null, ct);

        // Batch-embed everything in one pass. With 5 short docs we get
        // ~30-40 chunks; well under the embedder's batch limits.
        string[] texts = allChunks.Select(c => c.Text).ToArray();
        float[][] vectors = await _embedder.EmbedBatchAsync(texts, ct);

        var docs = new VectorDocument[allChunks.Count];
        for (int i = 0; i < allChunks.Count; i++)
        {
            var (docName, chunkIdx, text) = allChunks[i];
            docs[i] = new VectorDocument(
                Id: $"{docName}#{chunkIdx}",
                Vector: vectors[i],
                Metadata: new Dictionary<string, object>
                {
                    ["doc"] = docName,
                    ["text"] = text
                });
        }
        await _store.UpsertBatchAsync(CollectionName, docs, ct);
        await _store.FlushAsync(CollectionName, ct);
    }

    // ─── Schemas shown to the model ────────────────────────────────────────

    public IReadOnlyList<ToolSchema> GetSchemas() => new[]
    {
        new ToolSchema(
            Name: "list_topics",
            Description:
                "List every document available in the knowledge base, with each document's filename and one-line summary. " +
                "Useful as a starting move when you have no idea which documents are relevant. Takes no arguments.",
            ParametersSchema:
                """{"type":"object","properties":{}}"""),

        new ToolSchema(
            Name: "read_doc",
            Description:
                "Return the full text of one document. Use after list_topics or search_excerpt has identified a document worth " +
                "reading in depth. The name argument may be given with or without the .md extension.",
            ParametersSchema:
                """
                {
                  "type": "object",
                  "properties": {
                    "name": { "type": "string", "description": "Document filename, e.g. 'baroque-counterpoint' or 'baroque-counterpoint.md'" }
                  },
                  "required": ["name"]
                }
                """),

        new ToolSchema(
            Name: "search_excerpt",
            Description:
                "Semantic search across every paragraph of every document. Returns the top-K most relevant excerpts with " +
                "their source document and a similarity score. Useful when you need supporting evidence on a specific topic " +
                "but do not want to read whole documents.",
            ParametersSchema:
                """
                {
                  "type": "object",
                  "properties": {
                    "query": { "type": "string", "description": "Natural-language search query" },
                    "top_k": { "type": "integer", "description": "How many hits to return (default 3, max 5)" }
                  },
                  "required": ["query"]
                }
                """),

        new ToolSchema(
            Name: "compare",
            Description:
                "Generate a structured comparison between two documents along a named dimension. Calls back into the same " +
                "language model with a comparison prompt. Returns 3-5 bullet points. Use sparingly -- this is the most " +
                "expensive tool because it triggers another full LLM generation.",
            ParametersSchema:
                """
                {
                  "type": "object",
                  "properties": {
                    "topic_a":   { "type": "string", "description": "Name of first document" },
                    "topic_b":   { "type": "string", "description": "Name of second document" },
                    "dimension": { "type": "string", "description": "Aspect to compare on (e.g. 'tone', 'technical depth', 'practical examples', 'recommended audience')" }
                  },
                  "required": ["topic_a", "topic_b", "dimension"]
                }
                """),

        new ToolSchema(
            Name: "finalize",
            Description:
                "Commit the final answer and end the agent loop. Call this exactly once when you have gathered enough " +
                "information to answer the user's original question. After this tool returns, the loop terminates.",
            ParametersSchema:
                """
                {
                  "type": "object",
                  "properties": {
                    "answer": { "type": "string", "description": "The final synthesized answer to the user's question" }
                  },
                  "required": ["answer"]
                }
                """),
    };

    // ─── Dispatcher ─────────────────────────────────────────────────────────

    public async Task<string> DispatchAsync(FunctionCall call, KbContext ctx, CancellationToken ct = default)
    {
        try
        {
            return call.Name switch
            {
                "list_topics"    => DoListTopics(),
                "read_doc"       => DoReadDoc(call.Arguments),
                "search_excerpt" => await DoSearchExcerptAsync(call.Arguments, ct),
                "compare"        => await DoCompareAsync(call.Arguments, ct),
                "finalize"       => DoFinalize(call.Arguments, ctx),
                _                => $"Unknown tool: {call.Name}. Available: list_topics, read_doc, search_excerpt, compare, finalize."
            };
        }
        catch (JsonException jx)
        {
            return $"Tool '{call.Name}' received invalid arguments: {jx.Message}. Arguments were: {Truncate(call.Arguments, 300)}";
        }
        catch (Exception ex)
        {
            return $"Tool '{call.Name}' threw {ex.GetType().Name}: {ex.Message}";
        }
    }

    // ─── Tool implementations ───────────────────────────────────────────────

    private string DoListTopics()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Documents in the knowledge base:");
        int n = 1;
        // Iterate the canonical (with-extension) entries only, in alphabetical order.
        // Path.HasExtension is the cheap way to filter the with-extension keys we
        // intentionally added in IndexCorpusAsync from their without-extension aliases.
        foreach (var doc in _docs.Values
            .Where(d => Path.HasExtension(d.Name))
            .DistinctBy(d => d.Name)
            .OrderBy(d => d.Name))
        {
            sb.AppendLine($"  {n++}. {doc.Name} -- {doc.Title}");
        }
        return sb.ToString().TrimEnd();
    }

    private string DoReadDoc(string argsJson)
    {
        var args = JsonSerializer.Deserialize<JsonElement>(argsJson);
        string name = args.GetProperty("name").GetString() ?? "";
        if (string.IsNullOrWhiteSpace(name))
            return "read_doc: missing 'name' argument.";

        if (!_docs.TryGetValue(name, out KbDoc? doc))
        {
            string available = string.Join(", ", _docs.Values
                .Where(d => Path.HasExtension(d.Name))
                .Select(d => d.Name)
                .Distinct());
            return $"Document '{name}' not found. Available: {available}";
        }
        return doc.Text;
    }

    private async Task<string> DoSearchExcerptAsync(string argsJson, CancellationToken ct)
    {
        var args = JsonSerializer.Deserialize<JsonElement>(argsJson);
        string query = args.GetProperty("query").GetString() ?? "";
        if (string.IsNullOrWhiteSpace(query))
            return "search_excerpt: missing 'query' argument.";

        int topK = 3;
        if (args.TryGetProperty("top_k", out var topKEl) && topKEl.ValueKind == JsonValueKind.Number)
            topK = Math.Clamp(topKEl.GetInt32(), 1, 5);

        float[] queryVec = await _embedder.EmbedAsync(query, ct);
        var results = await _store.SearchAsync(CollectionName, queryVec, topK, null, ct);

        if (results.Count == 0) return $"No results for query: {query}";

        var sb = new StringBuilder();
        sb.AppendLine($"Top {results.Count} excerpt(s) for: {query}");
        int rank = 1;
        foreach (var r in results)
        {
            string docName = r.Metadata?.GetValueOrDefault("doc") as string ?? r.Id;
            string text = r.Metadata?.GetValueOrDefault("text") as string ?? "";
            sb.AppendLine($"[{rank++}] (score {r.Score:F3}) {docName}");
            sb.AppendLine($"    {Truncate(text.Replace("\n", " "), 300)}");
        }
        return sb.ToString().TrimEnd();
    }

    private async Task<string> DoCompareAsync(string argsJson, CancellationToken ct)
    {
        var args = JsonSerializer.Deserialize<JsonElement>(argsJson);
        string topicA = args.GetProperty("topic_a").GetString() ?? "";
        string topicB = args.GetProperty("topic_b").GetString() ?? "";
        string dimension = args.GetProperty("dimension").GetString() ?? "";

        if (!_docs.TryGetValue(topicA, out KbDoc? docA))
            return $"compare: document '{topicA}' not found. Try list_topics first.";
        if (!_docs.TryGetValue(topicB, out KbDoc? docB))
            return $"compare: document '{topicB}' not found. Try list_topics first.";

        // Cap each side at ~600 words so the comparison fits in a reasonable
        // context budget regardless of which inference backend the user
        // selected. The corpus's docs are short anyway -- this is a guard.
        string capA = TrimToWords(docA.Text, 600);
        string capB = TrimToWords(docB.Text, 600);

        string prompt =
            $$"""
            Compare the following two documents along the dimension of: {{dimension}}.
            Return 3-5 concise bullet points. Be specific. Do not include preamble.

            === {{docA.Name}} ===
            {{capA}}

            === {{docB.Name}} ===
            {{capB}}

            Comparison:
            """;

        // Cap response generation so compare doesn't blow the per-hop budget
        // on slow CPU backends. 300 tokens is enough for tight bullet-list
        // output.
        InferenceResponse response = await _inference.GenerateAsync(prompt,
            new InferenceOptions
            {
                Temperature = 0.3f,
                MaxTokens = 300,
                RepetitionPenalty = 1.1f
            }, ct);

        return $"Comparison of {docA.Name} vs {docB.Name} on '{dimension}':\n\n{response.Content.Trim()}";
    }

    private static string DoFinalize(string argsJson, KbContext ctx)
    {
        var args = JsonSerializer.Deserialize<JsonElement>(argsJson);
        string answer = args.GetProperty("answer").GetString() ?? "";
        if (string.IsNullOrWhiteSpace(answer))
            return "finalize: missing or empty 'answer' argument. Provide the final answer text.";

        ctx.FinalAnswer = answer;
        ctx.TerminalStatus = "answered";
        return "Answer recorded. Loop will terminate.";
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private static string ExtractTitle(string text)
    {
        // First # heading, or first non-empty line, capped.
        foreach (string line in text.Split('\n'))
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("# "))
                return trimmed.Substring(2).Trim();
        }
        foreach (string line in text.Split('\n'))
        {
            string trimmed = line.Trim();
            if (trimmed.Length > 0)
                return Truncate(trimmed, 80);
        }
        return "(untitled)";
    }

    private static string ExtractSummary(string text)
    {
        // First paragraph after the title, capped to one sentence (or 200 chars).
        bool seenTitle = false;
        foreach (string para in text.Split("\n\n"))
        {
            string trimmed = para.Trim();
            if (trimmed.StartsWith("# ")) { seenTitle = true; continue; }
            if (!seenTitle) continue;
            if (trimmed.Length == 0) continue;
            // First sentence, or first 200 chars.
            int dot = trimmed.IndexOf('.');
            return dot > 0 && dot < 200 ? trimmed.Substring(0, dot + 1) : Truncate(trimmed, 200);
        }
        return "";
    }

    private static string TrimToWords(string text, int maxWords)
    {
        string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= maxWords) return text;
        return string.Join(' ', words.Take(maxWords)) + " …";
    }

    private static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Length <= max ? s : s.Substring(0, max) + "…";
    }

    private static bool IsSupported(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext is ".pdf" or ".docx" or ".pptx" or ".xlsx"
            or ".html" or ".htm" or ".md" or ".txt"
            or ".csv" or ".json" or ".xml";
    }
}

internal sealed class KbDoc
{
    public string Name { get; init; } = "";
    public string Title { get; init; } = "";
    public string Summary { get; init; } = "";
    public string Text { get; init; } = "";
}
