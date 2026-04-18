using System.Text;

namespace Foundation.AI.Inference;

/// <summary>
/// Renders <see cref="ChatMessage"/> sequences into model-specific prompt strings.
///
/// <para><b>Why this lives here:</b>
/// Template rendering is pure string-manipulation and has no ONNX/HTTP dependency,
/// so it sits in the abstractions project where it can be reused by
/// <c>OnnxInferenceProvider</c>, a future <c>BitNetInferenceProvider</c>, or anything
/// else that speaks completion-at-a-prompt-boundary.</para>
///
/// <para><b>Supported templates:</b> <c>Phi3</c> (default), <c>ChatML</c>, <c>Phi4Mini</c>.
/// Phi-4-mini is the only template that understands tool schemas (injected into the
/// system segment per Microsoft's published chat_template).</para>
/// </summary>
public static class ChatTemplates
{
    public const string Phi3 = "Phi3";
    public const string ChatML = "ChatML";
    public const string Phi4Mini = "Phi4Mini";

    /// <summary>
    /// True when <paramref name="template"/> identifies the Phi-4-mini family (accepts the canonical
    /// name plus common aliases <c>phi4</c> and <c>phi-4-mini</c>). Used by providers that need to
    /// apply Phi-4-mini-specific post-processing, e.g. tool-call extraction.
    /// </summary>
    public static bool IsPhi4Mini(string? template)
    {
        if (string.IsNullOrEmpty(template)) return false;
        var normalized = template.ToLowerInvariant();
        return normalized is "phi4mini" or "phi4" or "phi-4-mini";
    }

    /// <summary>
    /// Render a chat conversation into a raw prompt string for the specified template.
    /// </summary>
    /// <param name="messages">Conversation history in chronological order.</param>
    /// <param name="template">Template identifier (<see cref="Phi3"/>, <see cref="ChatML"/>, <see cref="Phi4Mini"/>). Unknown values fall back to Phi-3.</param>
    /// <param name="tools">Optional tool schemas. Only honored by templates that support tool injection (Phi-4-mini).</param>
    public static string Render(
        IReadOnlyList<ChatMessage> messages,
        string? template = null,
        IReadOnlyList<ToolSchema>? tools = null)
    {
        if (messages is null) throw new ArgumentNullException(nameof(messages));

        return (template ?? Phi3).ToLowerInvariant() switch
        {
            "chatml" => RenderChatML(messages),
            "phi4mini" or "phi4" or "phi-4-mini" => RenderPhi4Mini(messages, tools),
            _ => RenderPhi3(messages),
        };
    }

    private static string RenderPhi3(IReadOnlyList<ChatMessage> messages)
    {
        var sb = new StringBuilder();
        foreach (var msg in messages)
        {
            sb.Append('<').Append('|').Append(msg.Role).Append('|').Append('>').Append('\n')
              .Append(msg.Content).Append("<|end|>\n");
        }
        sb.Append("<|assistant|>\n");
        return sb.ToString();
    }

    private static string RenderChatML(IReadOnlyList<ChatMessage> messages)
    {
        var sb = new StringBuilder();
        foreach (var msg in messages)
        {
            sb.Append("<|im_start|>").Append(msg.Role).Append('\n')
              .Append(msg.Content).Append("<|im_end|>\n");
        }
        sb.Append("<|im_start|>assistant\n");
        return sb.ToString();
    }

    /// <summary>
    /// Render for Phi-4-mini-instruct per the official tokenizer_config chat_template.
    ///
    /// <para>Base shape: <c>&lt;|role|&gt;content&lt;|end|&gt;</c> for every message.
    /// When <paramref name="tools"/> are provided, they are serialized as a JSON array
    /// and spliced into the first system message using
    /// <c>&lt;|system|&gt;content&lt;|tool|&gt;[...]&lt;|/tool|&gt;&lt;|end|&gt;</c>.
    /// If no system message exists, a synthetic empty one is prepended to carry the tools.</para>
    ///
    /// <para>Tool-response messages (role="tool") render as <c>&lt;|tool|&gt;content&lt;|end|&gt;</c>,
    /// matching the jinja's else branch. When <see cref="ChatMessage.ToolName"/> is set,
    /// we wrap the content as <c>{"name":"...","content":...}</c> so the model can link the
    /// response back to the call it issued — this is a small departure from the jinja
    /// (which drops tool_call_id/name entirely) but improves multi-tool-call disambiguation
    /// without breaking the token stream the model was trained on.</para>
    /// </summary>
    private static string RenderPhi4Mini(
        IReadOnlyList<ChatMessage> messages,
        IReadOnlyList<ToolSchema>? tools)
    {
        var sb = new StringBuilder();
        bool toolsInjected = tools is null || tools.Count == 0;

        for (int i = 0; i < messages.Count; i++)
        {
            var msg = messages[i];
            bool isSystem = string.Equals(msg.Role, "system", StringComparison.OrdinalIgnoreCase);

            if (isSystem && !toolsInjected)
            {
                AppendSystemWithTools(sb, msg.Content, tools!);
                toolsInjected = true;
                continue;
            }

            if (string.Equals(msg.Role, "tool", StringComparison.OrdinalIgnoreCase))
            {
                AppendToolResponse(sb, msg);
                continue;
            }

            if (string.Equals(msg.Role, "assistant", StringComparison.OrdinalIgnoreCase)
                && msg.FunctionCalls is { Count: > 0 })
            {
                AppendAssistantToolCall(sb, msg);
                continue;
            }

            AppendPlainMessage(sb, msg);
        }

        // No system message existed — emit a synthetic one so tool defs aren't lost.
        if (!toolsInjected)
        {
            var prefixed = new StringBuilder();
            AppendSystemWithTools(prefixed, string.Empty, tools!);
            prefixed.Append(sb);
            sb = prefixed;
        }

        sb.Append("<|assistant|>");
        return sb.ToString();
    }

    private static void AppendPlainMessage(StringBuilder sb, ChatMessage msg)
    {
        sb.Append('<').Append('|').Append(msg.Role).Append('|').Append('>')
          .Append(msg.Content)
          .Append("<|end|>");
    }

    private static void AppendSystemWithTools(StringBuilder sb, string content, IReadOnlyList<ToolSchema> tools)
    {
        sb.Append("<|system|>")
          .Append(content)
          .Append("<|tool|>")
          .Append(SerializeTools(tools))
          .Append("<|/tool|>")
          .Append("<|end|>");
    }

    /// <summary>
    /// Render an assistant turn that produced tool calls. The call JSON must appear in the
    /// rendered stream so that, when the subsequent <c>&lt;|tool|&gt;</c> result message is rendered,
    /// the model can correlate result ↔ call. Without this the result appears orphaned and the
    /// model slips into confused prose instead of emitting the next tool call.
    /// Shape: <c>&lt;|assistant|&gt;{prose}[{"name":"...","arguments":{...}},...]&lt;|end|&gt;</c>.
    /// </summary>
    private static void AppendAssistantToolCall(StringBuilder sb, ChatMessage msg)
    {
        sb.Append("<|assistant|>");
        if (!string.IsNullOrEmpty(msg.Content))
            sb.Append(msg.Content);

        sb.Append('[');
        var calls = msg.FunctionCalls!;
        for (int i = 0; i < calls.Count; i++)
        {
            if (i > 0) sb.Append(',');
            var c = calls[i];
            sb.Append("{\"name\":\"").Append(EscapeJsonString(c.Name ?? ""))
              .Append("\",\"arguments\":")
              .Append(string.IsNullOrWhiteSpace(c.Arguments) ? "{}" : c.Arguments)
              .Append('}');
        }
        sb.Append(']').Append("<|end|>");
    }

    private static void AppendToolResponse(StringBuilder sb, ChatMessage msg)
    {
        sb.Append("<|tool|>");
        if (!string.IsNullOrEmpty(msg.ToolName))
        {
            sb.Append("{\"name\":\"").Append(EscapeJsonString(msg.ToolName!))
              .Append("\",\"content\":").Append(WrapContentAsJson(msg.Content)).Append('}');
        }
        else
        {
            sb.Append(msg.Content);
        }
        sb.Append("<|end|>");
    }

    /// <summary>
    /// Build the tools JSON array that Phi-4-mini expects between <c>&lt;|tool|&gt;</c> and <c>&lt;|/tool|&gt;</c>.
    /// Each element is <c>{"name":...,"description":...,"parameters":&lt;raw schema&gt;}</c>.
    /// <see cref="ToolSchema.ParametersSchema"/> is already a JSON string, so it's spliced in verbatim.
    /// </summary>
    private static string SerializeTools(IReadOnlyList<ToolSchema> tools)
    {
        var sb = new StringBuilder();
        sb.Append('[');
        for (int i = 0; i < tools.Count; i++)
        {
            if (i > 0) sb.Append(',');
            var t = tools[i];
            sb.Append("{\"name\":\"").Append(EscapeJsonString(t.Name))
              .Append("\",\"description\":\"").Append(EscapeJsonString(t.Description))
              .Append("\",\"parameters\":")
              .Append(string.IsNullOrWhiteSpace(t.ParametersSchema) ? "{}" : t.ParametersSchema)
              .Append('}');
        }
        sb.Append(']');
        return sb.ToString();
    }

    /// <summary>
    /// If <paramref name="content"/> already parses as JSON, return it verbatim; otherwise wrap it as a JSON string literal.
    /// Prevents double-encoding when a tool returns structured JSON.
    /// </summary>
    private static string WrapContentAsJson(string content)
    {
        if (string.IsNullOrEmpty(content)) return "\"\"";
        var trimmed = content.TrimStart();
        if (trimmed.Length > 0 && (trimmed[0] == '{' || trimmed[0] == '[' || trimmed[0] == '"' ||
                                   trimmed[0] == 't' || trimmed[0] == 'f' || trimmed[0] == 'n' ||
                                   char.IsDigit(trimmed[0]) || trimmed[0] == '-'))
        {
            try
            {
                using var _ = System.Text.Json.JsonDocument.Parse(content);
                return content;
            }
            catch (System.Text.Json.JsonException)
            {
                // fall through to string-wrap
            }
        }
        return "\"" + EscapeJsonString(content) + "\"";
    }

    private static string EscapeJsonString(string s)
    {
        var sb = new StringBuilder(s.Length + 8);
        foreach (var c in s)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '"': sb.Append("\\\""); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                    else sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }
}
