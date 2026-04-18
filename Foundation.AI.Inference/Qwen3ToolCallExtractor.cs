using System.Text.Json;
using System.Text.RegularExpressions;

namespace Foundation.AI.Inference;

/// <summary>
/// Extracts tool/function calls from raw Qwen3 model output.
///
/// <para><b>Why this exists:</b> Qwen3 was trained to emit each function call inside a
/// <c>&lt;tool_call&gt;{"name":"...","arguments":{...}}&lt;/tool_call&gt;</c> span. Unlike
/// Phi-4-mini's free-form JSON blob, the delimiters are stable — but the extractor still
/// needs to tolerate stray <c>&lt;think&gt;...&lt;/think&gt;</c> reasoning blocks when
/// <c>/no_think</c> is ignored, triple-backtick fences around the JSON body, and multiple
/// sequential tool calls in one assistant turn.</para>
///
/// <para><b>Design choice — every parseable call is returned:</b> Qwen3's training allows
/// multiple <c>&lt;tool_call&gt;</c> spans in one turn and the model sometimes fires 2–3
/// lookups in parallel. Unlike the Phi-4-mini extractor (which takes the first valid span),
/// this one emits all of them so the caller can run them in parallel if it wants.</para>
/// </summary>
public static class Qwen3ToolCallExtractor
{
    private static readonly Regex ToolCallPattern = new(
        @"<tool_call>\s*(?<body>.*?)\s*</tool_call>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex ThinkPattern = new(
        @"<think>.*?</think>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>
    /// Attempt to extract tool calls from <paramref name="output"/>.
    /// Returns a result with zero calls and the original text (minus any think-blocks) when no
    /// tool call is present. Never throws on malformed input — parse failures silently skip.
    /// </summary>
    public static ToolCallExtractionResult Extract(string? output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return new ToolCallExtractionResult(output ?? string.Empty, Array.Empty<FunctionCall>());
        }

        // <think>…</think> is reasoning exhaust, not prose the caller should see or feed back.
        var stripped = ThinkPattern.Replace(output, string.Empty);

        var matches = ToolCallPattern.Matches(stripped);
        if (matches.Count == 0)
        {
            return new ToolCallExtractionResult(stripped.Trim(), Array.Empty<FunctionCall>());
        }

        var calls = new List<FunctionCall>(matches.Count);
        foreach (Match m in matches)
        {
            var body = m.Groups["body"].Value.Trim();
            body = UnfenceIfNeeded(body);
            if (TryParse(body, out var call))
            {
                calls.Add(call);
            }
        }

        var prose = ToolCallPattern.Replace(stripped, string.Empty).Trim();
        return new ToolCallExtractionResult(prose, calls);
    }

    private static string UnfenceIfNeeded(string body)
    {
        if (!body.StartsWith("```", StringComparison.Ordinal)) return body;

        int contentStart = 3;
        while (contentStart < body.Length && char.IsLetterOrDigit(body[contentStart])) contentStart++;
        while (contentStart < body.Length && (body[contentStart] is '\r' or '\n' or ' ')) contentStart++;

        int fenceEnd = body.LastIndexOf("```", StringComparison.Ordinal);
        return fenceEnd > contentStart ? body.Substring(contentStart, fenceEnd - contentStart).Trim() : body;
    }

    private static bool TryParse(string json, out FunctionCall call)
    {
        call = default!;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return false;
            if (!root.TryGetProperty("name", out var nameProp) || nameProp.ValueKind != JsonValueKind.String)
                return false;

            var name = nameProp.GetString();
            if (string.IsNullOrWhiteSpace(name)) return false;

            string arguments = "{}";
            if (root.TryGetProperty("arguments", out var argProp))
            {
                arguments = argProp.ValueKind switch
                {
                    JsonValueKind.String => argProp.GetString() ?? "{}",
                    JsonValueKind.Object or JsonValueKind.Array => argProp.GetRawText(),
                    JsonValueKind.Null or JsonValueKind.Undefined => "{}",
                    _ => argProp.GetRawText(),
                };
            }

            var id = "call-" + Guid.NewGuid().ToString("n").Substring(0, 12);
            call = new FunctionCall(id, name!, arguments);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
