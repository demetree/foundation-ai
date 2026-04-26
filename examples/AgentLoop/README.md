# AgentLoop

> A console sample showing Foundation.AI's **tool-calling** surface: a small research agent with five tools that can browse, read, search, compare, and finalize over a local markdown knowledge base. Demonstrates the `IInferenceProvider` multi-turn `ChatAsync` + `FunctionCall` dispatcher pattern, with the same three pluggable inference backends as DocsQ.

## What this shows that DocsQ doesn't

DocsQ is **one-shot RAG**: retrieve top-K chunks, generate one answer. AgentLoop is **agentic**: the model picks its own tools across multiple hops -- list_topics, then read_doc, then search_excerpt, then compare, then finalize -- iterating until it has enough information to commit to an answer.

The cleanest way to see the difference: ask DocsQ and AgentLoop the same question against the same `sample-docs/` corpus. DocsQ retrieves and answers in one shot. AgentLoop browses, reasons, and decides when it has enough.

## What this also shows

The **multi-hop ceiling** of small quantised models, live and reproducible. Set `AGENTLOOP_BACKEND=onnx` and most multi-hop questions cause Phi-4-mini-cpu-int4 to either go off-format around hop 3-4 or burn its 8-hop budget without finalizing. Set `AGENTLOOP_BACKEND=ollama` (or `llamasharp` if you have the GGUF) and the same questions complete cleanly. This is the documented finding that drove the "use Qwen3:8b for long agentic loops" decision in the parent project's `BmcTools/LegoHunter` agent.

## Run it

```powershell
cd examples/AgentLoop

# Default backend is 'ollama' here (not 'onnx' like in DocsQ).
# Reason: multi-hop reasoning is exactly where Phi-4-mini falls down,
# so making it the default would create a poor first impression.
dotnet run
```

To try the other backends:

```powershell
$env:AGENTLOOP_BACKEND = "onnx";       dotnet run   # Phi-4-mini ONNX. Will likely fail interestingly.
$env:AGENTLOOP_BACKEND = "llamasharp"; dotnet run   # Qwen3-8B GGUF (needs DocsQ-fetched GGUF on disk first).
```

Ollama prerequisites: install Ollama and run `ollama pull qwen3:8b` once.

## The five tools

| Tool | What it does |
|---|---|
| `list_topics()` | Returns every document name + one-line summary. The agent's "table of contents". |
| `read_doc(name)` | Returns the full text of a named document. The "drill in" move. |
| `search_excerpt(query, top_k)` | Semantic search across all paragraphs. Same Embed + Zvec path DocsQ uses for one-shot RAG. |
| `compare(topic_a, topic_b, dimension)` | LLM-on-LLM: reenters the inference provider with a structured comparison prompt. The "thinking tool". |
| `finalize(answer)` | Terminal. Sets `KbContext.TerminalStatus = "answered"` and ends the loop. |

All tool I/O is plain strings: JSON arguments in, agent-friendly text out. Exceptions are caught and surfaced as text so the model can recover by trying a different tool.

## Try these multi-hop questions

The included `sample-docs/` (copied from DocsQ) has 5 substantial markdown files. Each of these questions deliberately requires multiple hops -- they are NOT answerable from a single retrieval:

| Question | Why it needs multi-hop |
|---|---|
| `Compare what each document says about model selection.` | Cross-document synthesis. Has to browse topics, read at least 2-3 docs, generate a comparison. |
| `Which two of the included documents would be most useful preparation for understanding RAG?` | Metacognition + relevance judgment. Has to consider all topics and pick the best two. |
| `Find any sentence in the corpus that mentions both 'cantus firmus' and 'composer'.` | Search + verification. Has to search, then verify the result actually meets both criteria. |

When you run with `AGENTLOOP_BACKEND=ollama` you should see clean execution traces ending in `finalize()`. When you run with `AGENTLOOP_BACKEND=onnx` you should see Phi-4 either go off-format or burn its hop budget without finalizing -- this is honest data about the model's capability ceiling, not a bug.

## What you'll see (verbose trace per hop)

```
  [hop 1/8] calling model...
    . thinking:
      <think>I should start by seeing what documents are available.</think>
    -> list_topics({})
       result: Documents in the knowledge base:
                 1. baroque-counterpoint.md -- Counterpoint in the Late Baroque
                 2. dotnet-10-highlights.md -- .NET 10 Highlights for AI-Adjacent Workloads
                 ...

  [hop 2/8] calling model...
    -> search_excerpt({"query":"cantus firmus composer","top_k":3})
       result: Top 3 excerpt(s) for: cantus firmus composer
                 [1] (score 0.412) baroque-counterpoint.md
                     The five species are: note against note ... cantus firmus ... Mozart, Beethoven ...
                 ...

  [hop 3/8] calling model...
    -> finalize({"answer":"The relevant sentence is: 'Composers like Mozart, Beethoven, and Brahms studied these exercises ...'"})
       result: Answer recorded. Loop will terminate.

==============================================================================
FINAL (answered in 47823 ms)
==============================================================================
The relevant sentence is: 'Composers like Mozart, Beethoven, and Brahms studied these exercises'
```

## Adapting to your project

The pattern is straightforward to lift:

1. Define your tools as `ToolSchema` records (name, description, JSON schema for parameters).
2. Write a dispatcher that takes a `FunctionCall` and returns a `string` result.
3. Set up a `for` loop that calls `IInferenceProvider.ChatAsync(messages, options)`, dispatches each `FunctionCall` in `response.FunctionCalls`, appends `ChatMessage.Tool(result, callId, callName)` to messages, and stops when a terminal tool sets your context flag (or you hit a hop ceiling).

`AgentRunner.cs` is ~140 lines and is the closest reference. `KbTools.cs` is ~340 lines and shows the dispatcher + tool implementations. `BmcTools/LegoHunter/LegoMpdHunterAgent.cs` in the parent monorepo is the production version of this same pattern -- if you want a longer-running, web-tool-equipped variant, that's the reference.

## License

Same as Foundation.AI: [Apache-2.0](../../LICENSE).
