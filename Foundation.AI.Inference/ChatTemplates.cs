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
/// <para><b>Supported templates:</b> <c>Phi3</c> (default), <c>ChatML</c>, <c>Phi4Mini</c>, <c>Qwen3</c>.
/// Phi-4-mini and Qwen3 are the templates that understand tool schemas (injected into the
/// system segment per each model's published chat_template). Phi-4-mini uses
/// <c>&lt;|tool|&gt;…&lt;|/tool|&gt;</c> framing; Qwen3 uses ChatML-based
/// <c>&lt;tool_call&gt;…&lt;/tool_call&gt;</c> framing.</para>
/// </summary>
public static class ChatTemplates
{
    public const string Phi3 = "Phi3";
    public const string ChatML = "ChatML";
    public const string Phi4Mini = "Phi4Mini";
    public const string Qwen3 = "Qwen3";

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
    /// True when <paramref name="template"/> identifies the Qwen3 family (accepts the canonical
    /// name plus common aliases <c>qwen-3</c>, <c>qwen3-instruct</c>). Used by providers that need
    /// to apply Qwen3-specific post-processing, e.g. tool-call extraction.
    /// </summary>
    public static bool IsQwen3(string? template)
    {
        if (string.IsNullOrEmpty(template)) return false;
        var normalized = template.ToLowerInvariant();
        return normalized is "qwen3" or "qwen-3" or "qwen3-instruct";
    }

    /// <summary>
    /// Render a chat conversation into a raw prompt string for the specified template.
    /// </summary>
    /// <param name="messages">Conversation history in chronological order.</param>
    /// <param name="template">Template identifier (<see cref="Phi3"/>, <see cref="ChatML"/>, <see cref="Phi4Mini"/>, <see cref="Qwen3"/>). Unknown values fall back to Phi-3.</param>
    /// <param name="tools">Optional tool schemas. Only honored by templates that support tool injection (Phi-4-mini, Qwen3).</param>
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
            "qwen3" or "qwen-3" or "qwen3-instruct" => RenderQwen3(messages, tools),
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

    /// <summary>
    /// Render for Qwen3 per Qwen/Qwen3 published chat_template.
    ///
    /// <para>Base shape is ChatML (<c>&lt;|im_start|&gt;role\n…&lt;|im_end|&gt;</c>). When
    /// <paramref name="tools"/> are provided, tool definitions are injected into the system
    /// turn inside <c>&lt;tools&gt;…&lt;/tools&gt;</c> tags along with instructions to reply
    /// inside <c>&lt;tool_call&gt;…&lt;/tool_call&gt;</c>. Tool results are returned in a
    /// <c>user</c> turn wrapped in <c>&lt;tool_response&gt;…&lt;/tool_response&gt;</c> — this
    /// is the notable divergence from Phi-4-mini's dedicated <c>&lt;|tool|&gt;</c> role.</para>
    ///
    /// <para><c>/no_think</c> is appended to the system content to suppress Qwen3's default
    /// <c>&lt;think&gt;…&lt;/think&gt;</c> reasoning blocks. Those blocks inflate prefill on
    /// every hop of an agentic loop and occasionally leak into the tool-call span; agentic
    /// callers don't need them.</para>
    /// </summary>
    private static string RenderQwen3(
        IReadOnlyList<ChatMessage> messages,
        IReadOnlyList<ToolSchema>? tools)
    {
        var sb = new StringBuilder();
        bool systemEmitted = false;

        for (int i = 0; i < messages.Count; i++)
        {
            var msg = messages[i];

            if (string.Equals(msg.Role, "system", StringComparison.OrdinalIgnoreCase))
            {
                AppendQwen3System(sb, msg.Content, tools);
                systemEmitted = true;
                continue;
            }

            if (!systemEmitted)
            {
                AppendQwen3System(sb, string.Empty, tools);
                systemEmitted = true;
            }

            if (string.Equals(msg.Role, "tool", StringComparison.OrdinalIgnoreCase))
            {
                AppendQwen3ToolResponse(sb, msg);
                continue;
            }

            if (string.Equals(msg.Role, "assistant", StringComparison.OrdinalIgnoreCase)
                && msg.FunctionCalls is { Count: > 0 })
            {
                AppendQwen3AssistantToolCall(sb, msg);
                continue;
            }

            sb.Append("<|im_start|>").Append(msg.Role.ToLowerInvariant()).Append('\n')
              .Append(msg.Content)
              .Append("<|im_end|>\n");
        }

        if (!systemEmitted)
        {
            var prefixed = new StringBuilder();
            AppendQwen3System(prefixed, string.Empty, tools);
            prefixed.Append(sb);
            sb = prefixed;
        }

        sb.Append("<|im_start|>assistant\n");
        return sb.ToString();
    }

    private static void AppendQwen3System(StringBuilder sb, string content, IReadOnlyList<ToolSchema>? tools)
    {
        sb.Append("<|im_start|>system\n");

        var trimmed = (content ?? string.Empty).TrimEnd();
        if (trimmed.Length > 0) sb.Append(trimmed).Append('\n');
        sb.Append("/no_think\n");

        if (tools is { Count: > 0 })
        {
            sb.Append('\n').Append("# Tools\n\n")
              .Append("You may call one or more functions to assist with the user query.\n\n")
              .Append("You are provided with function signatures within <tools></tools> XML tags:\n")
              .Append("<tools>\n");

            foreach (var t in tools)
            {
                sb.Append("{\"type\":\"function\",\"function\":")
                  .Append("{\"name\":\"").Append(EscapeJsonString(t.Name))
                  .Append("\",\"description\":\"").Append(EscapeJsonString(t.Description))
                  .Append("\",\"parameters\":")
                  .Append(string.IsNullOrWhiteSpace(t.ParametersSchema) ? "{}" : t.ParametersSchema)
                  .Append("}}\n");
            }

            sb.Append("</tools>\n\n")
              .Append("For each function call, return a json object with function name and ")
              .Append("arguments within <tool_call></tool_call> XML tags:\n")
              .Append("<tool_call>\n{\"name\": \"...\", \"arguments\": {...}}\n</tool_call>\n");
        }

        sb.Append("<|im_end|>\n");
    }

    private static void AppendQwen3AssistantToolCall(StringBuilder sb, ChatMessage msg)
    {
        sb.Append("<|im_start|>assistant\n");
        if (!string.IsNullOrEmpty(msg.Content)) sb.Append(msg.Content).Append('\n');

        foreach (var c in msg.FunctionCalls!)
        {
            sb.Append("<tool_call>\n")
              .Append("{\"name\":\"").Append(EscapeJsonString(c.Name ?? ""))
              .Append("\",\"arguments\":")
              .Append(string.IsNullOrWhiteSpace(c.Arguments) ? "{}" : c.Arguments)
              .Append("}\n")
              .Append("</tool_call>\n");
        }
        sb.Append("<|im_end|>\n");
    }

    private static void AppendQwen3ToolResponse(StringBuilder sb, ChatMessage msg)
    {
        sb.Append("<|im_start|>user\n")
          .Append("<tool_response>\n")
          .Append(msg.Content)
          .Append("\n</tool_response>")
          .Append("<|im_end|>\n");
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
