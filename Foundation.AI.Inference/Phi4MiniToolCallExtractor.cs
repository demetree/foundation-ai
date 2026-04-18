using System.Text.Json;

namespace Foundation.AI.Inference;

/// <summary>
/// The outcome of running <see cref="Phi4MiniToolCallExtractor.Extract"/> on a model response.
/// </summary>
/// <param name="Prose">Text outside the tool-call span, trimmed. Empty when the entire response was a tool call.</param>
/// <param name="Calls">Parsed tool invocations. Empty when no valid tool call was found.</param>
public record ToolCallExtractionResult(string Prose, IReadOnlyList<FunctionCall> Calls);

/// <summary>
/// Extracts tool/function calls from raw Phi-4-mini model output.
///
/// <para><b>Why this exists:</b> Phi-4-mini was trained to emit function calls as a JSON array
/// (or bare object) in the assistant turn — there is no dedicated output token scheme like OpenAI's
/// tool_calls field. Per the PhiCookBook reference implementation, the model emits something of the
/// form <c>[{"name":"foo","arguments":{"x":1}}]</c>, but in practice it occasionally wraps that in
/// triple-backtick fences, prefixes with conversational prose ("Sure, let me call:"), or uses the
/// <c>&lt;|tool_call|&gt;...&lt;|/tool_call|&gt;</c> sentinel tokens. This extractor absorbs those
/// quirks and normalizes to <see cref="FunctionCall"/> records that match the OpenAI-shaped
/// abstraction the rest of Foundation.AI.Inference already speaks.</para>
///
/// <para><b>Design choice — first valid blob wins:</b> If the response contains multiple
/// candidate JSON spans, only the first parseable one is extracted. Phi-4-mini's training emits
/// all calls in a single array, so multi-blob output is unusual and treating it as noise keeps
/// the extractor simple and predictable.</para>
/// </summary>
public static class Phi4MiniToolCallExtractor
{
    private const string ToolCallOpen = "<|tool_call|>";
    private const string ToolCallClose = "<|/tool_call|>";

    /// <summary>
    /// Attempt to extract tool calls from <paramref name="output"/>.
    /// Returns a result with zero calls and the original text when no tool call is present.
    /// Never throws on malformed input — treats parse failures as "no tool call".
    /// </summary>
    public static ToolCallExtractionResult Extract(string? output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return new ToolCallExtractionResult(output ?? string.Empty, Array.Empty<FunctionCall>());
        }

        var candidate = FindJsonCandidate(output);
        if (candidate is null)
        {
            return new ToolCallExtractionResult(output, Array.Empty<FunctionCall>());
        }

        var calls = TryParseCalls(candidate.Value.Json);
        if (calls.Count == 0)
        {
            return new ToolCallExtractionResult(output, Array.Empty<FunctionCall>());
        }

        var prose = (output.Substring(0, candidate.Value.Start) +
                     output.Substring(candidate.Value.Start + candidate.Value.Length)).Trim();

        return new ToolCallExtractionResult(prose, calls);
    }

    private record struct JsonSpan(int Start, int Length, string Json);

    /// <summary>
    /// Scan for the first span that parses as tool-call JSON. Handles, in order of preference:
    /// sentinel-token wrapped JSON, triple-backtick fenced JSON (with optional <c>json</c> language),
    /// and bare JSON array/object. Skips past failed candidates so prose that happens to contain
    /// a stray <c>[</c> or <c>{</c> doesn't short-circuit the scan.
    /// </summary>
    private static JsonSpan? FindJsonCandidate(string text)
    {
        int pos = 0;
        while (pos < text.Length)
        {
            int next = FindNextMarker(text, pos);
            if (next < 0) return null;

            // <|tool_call|>...<|/tool_call|> sentinel
            if (text[next] == '<' && MatchesAt(text, next, ToolCallOpen))
            {
                int bodyStart = next + ToolCallOpen.Length;
                int closeIdx = text.IndexOf(ToolCallClose, bodyStart, StringComparison.Ordinal);
                int bodyEnd = closeIdx >= 0 ? closeIdx : text.Length;
                int spanEnd = closeIdx >= 0 ? closeIdx + ToolCallClose.Length : text.Length;

                var body = text.Substring(bodyStart, bodyEnd - bodyStart).Trim();
                var innerJson = ExtractInnerJson(body);
                if (innerJson is not null && LooksLikeToolCallJson(innerJson))
                {
                    return new JsonSpan(next, spanEnd - next, innerJson);
                }
                pos = spanEnd;
                continue;
            }

            // ``` [json] ``` code fence
            if (text[next] == '`' && MatchesAt(text, next, "```"))
            {
                int contentStart = next + 3;
                // Optional language tag, e.g. "json"
                while (contentStart < text.Length && char.IsLetterOrDigit(text[contentStart]))
                {
                    contentStart++;
                }
                // Skip whitespace / newline between language and body
                while (contentStart < text.Length && (text[contentStart] == '\r' || text[contentStart] == '\n' || text[contentStart] == ' '))
                {
                    contentStart++;
                }

                int fenceClose = text.IndexOf("```", contentStart, StringComparison.Ordinal);
                if (fenceClose < 0)
                {
                    pos = next + 3;
                    continue;
                }

                var body = text.Substring(contentStart, fenceClose - contentStart).Trim();
                if (LooksLikeToolCallJson(body))
                {
                    return new JsonSpan(next, fenceClose + 3 - next, body);
                }
                pos = fenceClose + 3;
                continue;
            }

            // Bare [ or { JSON
            if (text[next] == '[' || text[next] == '{')
            {
                int end = FindMatchingBracket(text, next);
                if (end < 0)
                {
                    // Unbalanced — bail; don't keep probing every char inside a broken blob.
                    return null;
                }
                var json = text.Substring(next, end - next + 1);
                if (LooksLikeToolCallJson(json))
                {
                    return new JsonSpan(next, end - next + 1, json);
                }
                pos = end + 1;
                continue;
            }

            pos = next + 1;
        }
        return null;
    }

    /// <summary>
    /// Inside a <c>&lt;|tool_call|&gt;</c> block the body should itself be JSON, but the model
    /// sometimes still fences it. Strip one level of fence if present, else return as-is.
    /// </summary>
    private static string? ExtractInnerJson(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        var trimmed = body.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            int contentStart = 3;
            while (contentStart < trimmed.Length && char.IsLetterOrDigit(trimmed[contentStart]))
            {
                contentStart++;
            }
            while (contentStart < trimmed.Length && (trimmed[contentStart] == '\r' || trimmed[contentStart] == '\n' || trimmed[contentStart] == ' '))
            {
                contentStart++;
            }
            int fenceEnd = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (fenceEnd > contentStart)
            {
                return trimmed.Substring(contentStart, fenceEnd - contentStart).Trim();
            }
        }
        return trimmed;
    }

    private static int FindNextMarker(string text, int start)
    {
        for (int i = start; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '[' || c == '{' || c == '`') return i;
            if (c == '<' && i + 1 < text.Length && text[i + 1] == '|') return i;
        }
        return -1;
    }

    private static bool MatchesAt(string text, int pos, string needle)
    {
        if (pos + needle.Length > text.Length) return false;
        for (int i = 0; i < needle.Length; i++)
        {
            if (text[pos + i] != needle[i]) return false;
        }
        return true;
    }

    /// <summary>
    /// Find the matching closing bracket for the opening bracket at <paramref name="start"/>,
    /// respecting JSON string boundaries so that brackets inside string literals don't confuse the depth count.
    /// </summary>
    private static int FindMatchingBracket(string text, int start)
    {
        char open = text[start];
        char close = open == '[' ? ']' : '}';
        int depth = 0;
        bool inString = false;
        bool escape = false;

        for (int i = start; i < text.Length; i++)
        {
            char c = text[i];

            if (escape) { escape = false; continue; }
            if (inString)
            {
                if (c == '\\') { escape = true; continue; }
                if (c == '"') { inString = false; continue; }
                continue;
            }

            if (c == '"') { inString = true; continue; }
            if (c == open) { depth++; continue; }
            if (c == close)
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }

    private static bool LooksLikeToolCallJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in root.EnumerateArray())
                {
                    if (HasValidName(el)) return true;
                }
                return false;
            }

            return root.ValueKind == JsonValueKind.Object && HasValidName(root);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool HasValidName(JsonElement el)
    {
        if (el.ValueKind != JsonValueKind.Object) return false;
        if (!el.TryGetProperty("name", out var nameProp)) return false;
        if (nameProp.ValueKind != JsonValueKind.String) return false;
        var name = nameProp.GetString();
        return !string.IsNullOrWhiteSpace(name);
    }

    private static IReadOnlyList<FunctionCall> TryParseCalls(string json)
    {
        var result = new List<FunctionCall>();
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in root.EnumerateArray())
                {
                    AppendCall(result, el);
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                AppendCall(result, root);
            }
        }
        catch (JsonException)
        {
            // Swallow — caller treats empty list as "no tool call".
        }
        return result;
    }

    private static void AppendCall(List<FunctionCall> list, JsonElement el)
    {
        if (!HasValidName(el)) return;

        var name = el.GetProperty("name").GetString()!;
        var arguments = ReadArguments(el);
        var id = el.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String
            ? idProp.GetString()!
            : "call-" + Guid.NewGuid().ToString("n").Substring(0, 12);

        list.Add(new FunctionCall(id, name, arguments));
    }

    /// <summary>
    /// Arguments may be a nested JSON object (<c>{"x":1}</c>) or a JSON-encoded string
    /// (<c>"{\"x\":1}"</c>). Some Phi-4-mini outputs also use <c>parameters</c> as the field name.
    /// Normalize all shapes to a raw JSON string ready to be fed into a tool handler.
    /// </summary>
    private static string ReadArguments(JsonElement el)
    {
        JsonElement argProp = default;
        bool found = el.TryGetProperty("arguments", out argProp) ||
                     el.TryGetProperty("parameters", out argProp);

        if (!found) return "{}";

        return argProp.ValueKind switch
        {
            JsonValueKind.String => argProp.GetString() ?? "{}",
            JsonValueKind.Object or JsonValueKind.Array => argProp.GetRawText(),
            JsonValueKind.Null or JsonValueKind.Undefined => "{}",
            _ => argProp.GetRawText(),
        };
    }
}
