// Copyright 2025-present the zvec project — Pure C# Engine

namespace Foundation.AI.Zvec.Engine.Storage;

/// <summary>
/// Thrown when a ZoneTree-backed store cannot be opened because its on-disk
/// state is inconsistent — typically a manifest referencing segment files that
/// are missing, truncated, or unreadable. The most common cause is a process
/// that was killed (or debugger-stopped) mid-compaction before the new segment
/// files finished persisting, leaving a manifest pointing at a segment ID that
/// was never fully written.
///
/// <para>Callers can opt into destructive self-recovery by setting
/// <c>EngineCollectionOptions.AllowDestructiveRecovery = true</c>; when enabled,
/// the storage engine renames the corrupted sub-directory to
/// <c>{name}.corrupt-{utcTimestamp}</c> and re-creates an empty store. The
/// quarantined copy is left on disk for post-mortem rather than deleted.</para>
/// </summary>
public sealed class CorruptedStoreException : Exception
{
    public string StorePath { get; }
    public string? QuarantinedPath { get; }

    public CorruptedStoreException(string storePath, string message, Exception? inner = null)
        : base(message, inner)
    {
        StorePath = storePath;
    }

    public CorruptedStoreException(string storePath, string quarantinedPath, string message, Exception? inner = null)
        : base(message, inner)
    {
        StorePath = storePath;
        QuarantinedPath = quarantinedPath;
    }
}
