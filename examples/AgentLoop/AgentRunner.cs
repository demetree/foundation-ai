using Foundation.AI.Inference;

namespace Foundation.AI.Examples.AgentLoop;

// The hop loop. Mirrors the structure of BmcTools/LegoHunter/LegoMpdHunterAgent
// (which is the production reference for this pattern), simplified to remove
// the LEGO/MPD specifics and the database-backed trace logging.
//
// One run = one user question -> up to MaxHops (8) tool calls -> a final answer
// (when finalize() is called) or an "exhausted" / "gave_up" terminal state.
//
// Two prompt variants are kept because ONNX Phi-4-mini has no native
// tool-calling channel and needs heavy output-shape coaching to emit literal
// JSON tool calls. OpenAI / Ollama / LLamaSharp providers handle tool-call
// serialization themselves and the coaching just adds noise. See
// HunterInferenceFactory comments for the longer justification.
public sealed class AgentRunner
{
    private const int   MaxHops           = 8;
    private const float Temperature       = 0.2f;
    private const float RepetitionPenalty = 1.3f;
    private const int   MaxResponseTokens = 1024;

    private readonly IInferenceProvider _provider;
    private readonly KbTools _tools;
    private readonly bool _isOnnxPhi4;
    private readonly bool _verbose;

    public AgentRunner(IInferenceProvider provider, KbTools tools, bool isOnnxPhi4, bool verbose = true)
    {
        _provider   = provider;
        _tools      = tools;
        _isOnnxPhi4 = isOnnxPhi4;
        _verbose    = verbose;
    }

    public async Task<KbContext> RunAsync(string question, CancellationToken ct = default)
    {
        KbContext ctx = new() { Question = question };

        List<ChatMessage> messages = new()
        {
            ChatMessage.System(_isOnnxPhi4 ? BuildPromptPhi4() : BuildPromptNative()),
            ChatMessage.User(question),
        };

        InferenceOptions options = new()
        {
            // PromptTemplate is ONNX-only. OpenAI-compat / LLamaSharp ignore it.
            PromptTemplate    = _isOnnxPhi4 ? "Phi4Mini" : "",
            Temperature       = Temperature,
            RepetitionPenalty = RepetitionPenalty,
            MaxTokens         = MaxResponseTokens,
            Tools             = _tools.GetSchemas(),
        };

        for (int hop = 0; hop < MaxHops && ctx.TerminalStatus is null; hop++)
        {
            if (_verbose) Console.WriteLine($"\n  [hop {hop + 1}/{MaxHops}] calling model...");

            InferenceResponse response;
            try
            {
                response = await _provider.ChatAsync(messages, options, ct);
            }
            catch (Exception ex)
            {
                ctx.TerminalStatus = "error";
                ctx.FinalAnswer    = $"Inference error on hop {hop + 1}: {ex.Message}";
                if (_verbose) Console.WriteLine($"    ! {ctx.FinalAnswer}");
                break;
            }

            if (response.FunctionCalls is null || response.FunctionCalls.Count == 0)
            {
                // The model produced free-text instead of a tool call. For
                // small / over-quantised models this typically means it has
                // either decided the loop is "done" without saying so, or
                // gone off-format mid-conversation.
                ctx.TerminalStatus = "gave_up";
                ctx.FinalAnswer    = "Agent stopped without calling a tool. " +
                                     "Last raw output:\n" + (response.Content?.Trim() ?? "");
                if (_verbose)
                {
                    Console.WriteLine("    ! No tool call extracted. Raw output:");
                    foreach (string line in (response.Content ?? "").Split('\n'))
                        Console.WriteLine("      " + line.TrimEnd('\r'));
                }
                break;
            }

            // Append the assistant's turn (reasoning + the tool-call request)
            // before dispatching, so the message history reflects the model's
            // own view of the conversation when we ask it for the next hop.
            messages.Add(ChatMessage.Assistant(response.Content, response.FunctionCalls));

            string reasoning = response.Content?.Trim() ?? "";
            if (_verbose && reasoning.Length > 0)
            {
                // Qwen3 in particular wraps private chain-of-thought in
                // <think>...</think> blocks; we surface it verbatim because
                // for a SAMPLE that's the whole point -- the user wants to
                // see the agent reason.
                Console.WriteLine("    . thinking:");
                foreach (string line in reasoning.Split('\n'))
                    Console.WriteLine("      " + line.TrimEnd('\r'));
            }

            foreach (FunctionCall call in response.FunctionCalls)
            {
                if (_verbose) Console.WriteLine($"    -> {call.Name}({Truncate(call.Arguments, 120)})");

                string result = await _tools.DispatchAsync(call, ctx, ct);
                messages.Add(ChatMessage.Tool(result, call.Id, call.Name));

                if (_verbose) Console.WriteLine($"       result: {Truncate(result, 200)}");

                // Re-prime Phi-4-int4 only. Larger models with native tool
                // calling chain cleanly without nudges; an injected user turn
                // just adds noise.
                if (ctx.TerminalStatus is null && _isOnnxPhi4)
                {
                    messages.Add(ChatMessage.User(
                        "Continue. Reply with EXACTLY ONE tool call as JSON -- no prose:\n" +
                        "[{\"name\":\"<tool>\",\"arguments\":{...}}]"));
                }

                if (ctx.TerminalStatus is not null) break;
            }
        }

        if (ctx.TerminalStatus is null)
        {
            ctx.TerminalStatus = "exhausted";
            ctx.FinalAnswer    = $"Reached the {MaxHops}-hop limit without finalising an answer. " +
                                 "This usually means the model is too small for multi-step reasoning over this corpus -- " +
                                 "try a stronger backend (AGENTLOOP_BACKEND=ollama or llamasharp).";
        }

        return ctx;
    }

    // ─── Prompts ────────────────────────────────────────────────────────────

    // Native tool-calling providers (OpenAI, Ollama, LLamaSharp) get a clean
    // strategy-only prompt. The provider handles tool-call serialization for us
    // via its own response field, so we don't need to coach output shape.
    private static string BuildPromptNative() =>
"""
You are a research agent. Answer the user's question by exploring a small knowledge base of markdown documents.

Tools available:
- list_topics()                          -- See what documents exist.
- read_doc(name)                         -- Read a document in full.
- search_excerpt(query, top_k)           -- Semantic search across all paragraphs.
- compare(topic_a, topic_b, dimension)   -- Generate a structured comparison (expensive -- triggers another LLM call).
- finalize(answer)                       -- Commit your final answer (terminates the loop).

Strategy:
1. Start with list_topics if you do not know the corpus.
2. Use search_excerpt for targeted evidence; use read_doc when you need a whole document.
3. Use compare sparingly.
4. Call finalize exactly once when you have enough information to answer.

You have a hard ceiling of 8 hops. Be efficient -- do not browse everything when one search would do.
Do not narrate at length between tool calls.
""";

    // ONNX Phi-4-mini-int4 has no native tool-calling channel; it must emit
    // tool calls as literal JSON in the assistant content, which Foundation.AI's
    // Phi4MiniToolCallExtractor then parses. Heavy output-shape coaching --
    // explicit examples and explicit "what NOT to emit" -- significantly
    // raises the rate at which Phi-4 stays on-format. Even with this,
    // multi-hop quality degrades past ~3 hops (per the documented
    // feedback_phi4_multihop_ceiling.md memory entry).
    private static string BuildPromptPhi4() =>
"""
You are a research agent. Answer the user's question by exploring a small knowledge base of markdown documents.

You have these tools:
- list_topics()                          -- list all documents
- read_doc(name)                         -- read a document in full
- search_excerpt(query, top_k)           -- semantic search across paragraphs
- compare(topic_a, topic_b, dimension)   -- structured comparison (expensive)
- finalize(answer)                       -- commit the final answer (ends the loop)

## OUTPUT FORMAT -- VERY IMPORTANT

On every turn you MUST respond with EXACTLY ONE tool call as a JSON array of a
single object, and nothing else. No prose, no markdown fences. The ONLY keys
allowed per call are "name" and "arguments".

Correct examples -- copy this shape exactly:

[{"name":"list_topics","arguments":{}}]

[{"name":"read_doc","arguments":{"name":"baroque-counterpoint.md"}}]

[{"name":"search_excerpt","arguments":{"query":"vector embeddings","top_k":3}}]

[{"name":"compare","arguments":{"topic_a":"docA","topic_b":"docB","dimension":"tone"}}]

[{"name":"finalize","arguments":{"answer":"Your final answer here."}}]

WRONG -- do NOT include "description" or "parameters" keys (those appear in
the tool schema you were given but must NOT appear in your response). Do NOT
emit two tool calls at once. Do NOT repeat the tool definitions back.

## STRATEGY

1. Start with list_topics() if you do not know the corpus.
2. Use search_excerpt for targeted evidence.
3. Call finalize exactly once when ready to commit your answer.
4. You have a hard ceiling of 8 hops; be efficient.
""";

    private static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return s ?? "";
        return s.Length <= max ? s : s.Substring(0, max) + "...";
    }
}
