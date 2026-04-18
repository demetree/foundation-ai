using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Foundation.AI.Inference;

/// <summary>
/// In-memory ring buffer of recent AI errors (ingestion failures, chat
/// dispatch exceptions, index rebuild crashes). The diagnostics UI polls
/// <c>GET /api/ai/errors/recent</c> so an operator can see what broke
/// without tailing server logs. Process-local by design — this is an
/// operational breadcrumb trail, not an audit log.
/// </summary>
public sealed class AiErrorLog
{
    private const int CAPACITY = 50;

    private readonly ConcurrentQueue<AiErrorEntry> _entries = new();
    private readonly object _trimLock = new();

    public void Record(string source, string message, Exception? exception = null)
    {
        _entries.Enqueue(new AiErrorEntry(
            TimestampUtc: DateTime.UtcNow,
            Source: source ?? "(unknown)",
            Message: message ?? exception?.Message ?? "(no message)",
            ExceptionType: exception?.GetType().FullName));

        // Cheap trim. Over-count is fine — the queue just grows by at most
        // a handful before a concurrent enqueue triggers the next trim.
        if (_entries.Count > CAPACITY)
        {
            lock (_trimLock)
            {
                while (_entries.Count > CAPACITY && _entries.TryDequeue(out _)) { }
            }
        }
    }

    public IReadOnlyList<AiErrorEntry> GetRecent(int limit = CAPACITY)
    {
        if (limit < 1) limit = 1;
        if (limit > CAPACITY) limit = CAPACITY;
        return _entries.ToArray()
            .Reverse()
            .Take(limit)
            .ToList();
    }
}

public sealed record AiErrorEntry(
    DateTime TimestampUtc,
    string Source,
    string Message,
    string ExceptionType);
