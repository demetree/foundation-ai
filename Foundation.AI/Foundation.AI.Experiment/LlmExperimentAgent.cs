using System.Text;
using Foundation.AI.Inference;
using Microsoft.Extensions.Logging;

namespace Foundation.AI.Experiment;

/// <summary>
/// Uses an LLM (<see cref="IInferenceProvider"/>) to propose code modifications
/// for autonomous experimentation. The agent analyzes the current code and
/// experiment history, then generates a targeted change aimed at improving
/// the target metric.
///
/// <para><b>How it works:</b>
/// <list type="number">
/// <item>Builds a conversation with the strategy prompt (system), current code, and history</item>
/// <item>Asks the LLM to produce the complete modified file content</item>
/// <item>Extracts the code from the response (handles markdown code fences)</item>
/// <item>Returns the modification with a description</item>
/// </list></para>
/// </summary>
public sealed class LlmExperimentAgent : IExperimentAgent
{
    private readonly IInferenceProvider _inference;
    private readonly ILogger<LlmExperimentAgent> _logger;

    private const string DefaultStrategyPrompt =
        """
        You are an autonomous ML researcher. Your goal is to improve the training
        code to achieve a lower validation metric (val_bpb = bits per byte).

        You modify a single Python training file. Everything is fair game: model
        architecture, optimizer, hyperparameters, training loop, batch size, model size.

        Rules:
        - Output the COMPLETE modified file content (not a diff)
        - Make one focused change per experiment
        - All else being equal, simpler is better
        - Don't add external dependencies
        - The code must run without crashing
        """;

    public LlmExperimentAgent(IInferenceProvider inference,
        ILogger<LlmExperimentAgent> logger)
    {
        _inference = inference;
        _logger = logger;
    }

    public async Task<CodeModification> ProposeModificationAsync(
        string currentCode,
        IReadOnlyList<ExperimentResult> history,
        string strategyPrompt,
        CancellationToken ct = default)
    {
        var systemPrompt = string.IsNullOrWhiteSpace(strategyPrompt)
            ? DefaultStrategyPrompt
            : strategyPrompt;

        var userPrompt = BuildUserPrompt(currentCode, history);

        _logger.LogInformation("Requesting code modification from LLM ({Model})",
            _inference.ModelName);

        var messages = new List<ChatMessage>
        {
            ChatMessage.System(systemPrompt),
            ChatMessage.User(userPrompt),
        };

        var response = await _inference.ChatAsync(messages, new InferenceOptions
        {
            Temperature = 0.7f,
            MaxTokens = 16384,
        }, ct);

        var (code, description) = ParseResponse(response.Content, currentCode);

        _logger.LogInformation("LLM proposed modification: {Description}", description);

        return new CodeModification(code, description);
    }

    // ─── Private Helpers ───────────────────────────────────────────────

    private static string BuildUserPrompt(string currentCode,
        IReadOnlyList<ExperimentResult> history)
    {
        var sb = new StringBuilder();

        // Experiment history
        if (history.Count > 0)
        {
            sb.AppendLine("## Experiment History (most recent last)");
            sb.AppendLine();

            // Show at most the last 20 experiments to avoid context overflow
            var recent = history.Count > 20
                ? history.Skip(history.Count - 20).ToList()
                : history;

            foreach (var result in recent)
            {
                var statusIcon = result.Status switch
                {
                    ExperimentStatus.Keep => "✓",
                    ExperimentStatus.Discard => "✗",
                    ExperimentStatus.Crash => "💥",
                    _ => "?"
                };
                sb.AppendLine($"- [{statusIcon} {result.Status}] {result.Description} " +
                    $"→ val_bpb={result.MetricValue:F6}, memory={result.PeakMemoryGb:F1}GB");
            }

            if (history.Count > 20)
                sb.AppendLine($"  (showing last 20 of {history.Count} total experiments)");

            sb.AppendLine();

            // Highlight the current best
            var bestKept = history
                .Where(r => r.Status == ExperimentStatus.Keep)
                .OrderBy(r => r.MetricValue)
                .FirstOrDefault();

            if (bestKept is not null)
            {
                sb.AppendLine($"**Current best:** val_bpb={bestKept.MetricValue:F6} " +
                    $"({bestKept.Description})");
                sb.AppendLine();
            }
        }
        else
        {
            sb.AppendLine("## No experiment history yet");
            sb.AppendLine("This is the first experiment. Propose an improvement to the code below.");
            sb.AppendLine();
        }

        // Current code
        sb.AppendLine("## Current Code");
        sb.AppendLine();
        sb.AppendLine("```python");
        sb.AppendLine(currentCode);
        sb.AppendLine("```");
        sb.AppendLine();

        // Instructions
        sb.AppendLine("## Your Task");
        sb.AppendLine();
        sb.AppendLine("Propose ONE focused modification to improve the metric.");
        sb.AppendLine("First, write a brief one-line description of what you're changing and why.");
        sb.AppendLine("Then output the COMPLETE modified file wrapped in ```python code fences.");
        sb.AppendLine("Do NOT output a diff — output the full file content.");

        return sb.ToString();
    }

    private static (string Code, string Description) ParseResponse(
        string response, string fallbackCode)
    {
        // Extract description (text before the first code fence)
        var description = "LLM-proposed modification";
        var fenceStart = response.IndexOf("```", StringComparison.Ordinal);
        if (fenceStart > 0)
        {
            var descText = response[..fenceStart].Trim();
            // Take the last non-empty line before the code fence as description
            var descLines = descText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (descLines.Length > 0)
            {
                description = descLines[^1].Trim().TrimStart('#', '-', '*', ' ');
                if (string.IsNullOrWhiteSpace(description) && descLines.Length > 1)
                    description = descLines[^2].Trim().TrimStart('#', '-', '*', ' ');
            }
        }

        // Extract code from markdown fences
        var code = ExtractCodeBlock(response);

        // Fallback: if we couldn't extract code, use the full response
        // (maybe the LLM didn't use code fences)
        if (string.IsNullOrWhiteSpace(code))
        {
            // If the response looks like Python code (has imports/def/class), use it directly
            if (response.Contains("import ") || response.Contains("def ") || response.Contains("class "))
                code = response;
            else
                code = fallbackCode; // Complete failure — return original code
        }

        // Truncate description to a reasonable length
        if (description.Length > 200)
            description = description[..200] + "...";

        return (code, description);
    }

    private static string ExtractCodeBlock(string text)
    {
        // Find ```python or ``` followed by code
        var patterns = new[] { "```python\n", "```python\r\n", "```py\n", "```py\r\n", "```\n", "```\r\n" };

        foreach (var pattern in patterns)
        {
            var start = text.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (start < 0) continue;

            start += pattern.Length;
            var end = text.IndexOf("```", start, StringComparison.Ordinal);
            if (end < 0) continue;

            return text[start..end].Trim();
        }

        return "";
    }
}
