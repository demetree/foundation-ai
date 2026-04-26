namespace Foundation.AI.Examples.AgentLoop;

// Per-query mutable state shared across every tool invocation within one
// agent run. The agent loop in AgentRunner watches TerminalStatus and stops
// as soon as any tool sets it (in practice that's only finalize()).
//
// Modeled after the HuntContext in BmcTools/LegoHunter -- same idea (tools
// signal the loop via shared state), narrower scope (no domain-specific
// fields, just question / status / answer).
public sealed class KbContext
{
    // The user's original question. Available to tools that want to consult
    // it directly (e.g. for relevance scoring, prompt construction).
    public string Question { get; init; } = "";

    // Set by finalize() to "answered". The loop terminates on the next
    // iteration check when this becomes non-null.
    public string? TerminalStatus { get; set; }

    // The agent's final synthesized answer, set by finalize().
    public string? FinalAnswer { get; set; }
}
