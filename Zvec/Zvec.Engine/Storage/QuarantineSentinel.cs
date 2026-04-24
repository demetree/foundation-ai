// Copyright 2025-present the zvec project — Pure C# Engine

using System.Text.Json;

namespace Foundation.AI.Zvec.Engine.Storage;

/// <summary>
/// Deferred-quarantine sentinel for <see cref="ZoneTreeStorageEngine"/>.
///
/// <para><b>Problem this solves.</b> When a corrupted store is detected, the
/// engine tries to rename its sub-directory aside (<c>docs</c> →
/// <c>docs.corrupt-{ts}</c>). That rename can fail on Windows with
/// <c>Access denied</c> if the preceding <c>OpenOrCreate()</c> attempt
/// memory-mapped some of the segment files before tripping over the missing
/// segment — the OS refuses to rename a directory whose contents have live
/// handles, and those mmap handles do not drop until the owning process
/// ends (no amount of <c>GC.Collect + WaitForPendingFinalizers</c> will
/// pry them loose once the <c>MemoryMappedFile</c> objects have leaked past
/// the natural finalizer window).</para>
///
/// <para><b>Mechanism.</b> On rename failure, the engine writes a small
/// JSON sentinel file next to the source directory —
/// <c>{source}.pending-quarantine.json</c> — describing the move it wanted
/// to perform. The current process then exits (by propagating
/// <see cref="CorruptedStoreException"/>) with the stuck files still in
/// place. On the *next* process startup — which by definition has no
/// leftover handles from the prior run — the engine's constructor sees the
/// sentinel *before* it calls <c>OpenOrCreate()</c>, performs the queued
/// rename against the now-unlocked files, deletes the sentinel, and then
/// proceeds into the normal open path on a clean empty directory.</para>
///
/// <para><b>Why next-to-source rather than a central queue.</b> Co-locating
/// the sentinel with the directory it describes means no extra plumbing is
/// needed to tell the engine "here's where sentinels live" — any
/// <see cref="ZoneTreeStorageEngine"/> instance already knows its own
/// sub-paths and can check for <c>{path}.pending-quarantine.json</c>
/// directly. Each collection cleans up independently. If the collection's
/// parent dir is deleted, the sentinel goes with it — no orphans.</para>
///
/// <para><b>Maintenance notes.</b>
/// <list type="bullet">
///   <item>The sentinel's <c>Target</c> path is the *exact* filename the
///   failed run intended to produce. Re-using that name on the next run
///   keeps the corrupt-suffix timestamp monotonic with the actual failure
///   event, not the recovery event, which is usually what you want for
///   forensics.</item>
///   <item>If a sentinel already exists when a new quarantine attempt
///   fails (i.e. the previous retry also couldn't complete), we overwrite
///   it. The old queued target name is lost, but the corrupt source files
///   are still there and will be renamed under the new name instead —
///   forensic evidence is preserved, just relabelled.</item>
///   <item>Draining is best-effort. If drain itself fails (really rare —
///   would require an external process holding the files), we leave the
///   sentinel in place and let the caller's normal corruption-detection
///   path take over. That path will probably fail the same way, write the
///   same sentinel, and the cycle continues — which is the correct
///   behaviour: the user sees a loud error until they close whatever's
///   holding the handles.</item>
///   <item>The format is deliberately human-readable JSON so an operator
///   can hand-edit or hand-delete a sentinel during incident response
///   without needing the engine running.</item>
/// </list>
/// </para>
/// </summary>
internal static class QuarantineSentinel
{
    /// <summary>
    /// Filename suffix appended to the source path to locate a sentinel.
    /// Chosen to sort next to the sub-directory it refers to in
    /// alphabetical listings (both <c>docs</c> and
    /// <c>docs.pending-quarantine.json</c> cluster together).
    /// </summary>
    private const string SentinelSuffix = ".pending-quarantine.json";

    /// <summary>
    /// JSON envelope. Kept tiny on purpose — this file exists for exactly
    /// one job and shouldn't invite feature creep. If you need to store
    /// more (e.g. a retry counter), add a field here and keep back-compat
    /// by making it optional on read.
    /// </summary>
    internal sealed class Entry
    {
        /// <summary>Directory the engine wanted to move aside. Expected to
        /// exist on disk at drain time; if it doesn't, the sentinel is
        /// stale and we quietly delete it.</summary>
        public string Source { get; set; } = "";

        /// <summary>Intended post-rename path, typically
        /// <c>{source}.corrupt-{utcTimestamp}</c>. Preserved verbatim so
        /// the eventual rename uses the same forensic timestamp the user
        /// first hit the error at, not a new one from the recovery run.</summary>
        public string Target { get; set; } = "";

        /// <summary>UTC ISO-8601 timestamp of when the quarantine was
        /// queued. Informational only — not used for logic.</summary>
        public string QueuedAtUtc { get; set; } = "";

        /// <summary>First-line summary of the original exception. Handy
        /// when a human opens the sentinel to see why it's there.</summary>
        public string Reason { get; set; } = "";
    }

    /// <summary>
    /// Compute the sentinel path for a given source directory. Not stored
    /// on the <see cref="Entry"/> itself so we can move/rename the source
    /// without invalidating data inside the sentinel.
    /// </summary>
    public static string SentinelPathFor(string sourcePath) =>
        sourcePath + SentinelSuffix;

    /// <summary>
    /// Record a pending quarantine. Overwrites any existing sentinel at
    /// the same path — see the "maintenance notes" on the class doc for
    /// why that's deliberate. Best-effort: IO failures here are swallowed
    /// because we're already unwinding a failure path and don't want to
    /// mask the original error.
    /// </summary>
    public static void Write(string sourcePath, string targetPath, string reason)
    {
        try
        {
            var entry = new Entry
            {
                Source = sourcePath,
                Target = targetPath,
                QueuedAtUtc = DateTime.UtcNow.ToString("O"),
                Reason = reason
            };
            var json = JsonSerializer.SerializeToUtf8Bytes(entry, SentinelContext.Default.Entry);
            File.WriteAllBytes(SentinelPathFor(sourcePath), json);
        }
        catch (Exception ex)
        {
            // Intentionally swallowed: the caller is already about to
            // throw CorruptedStoreException and we don't want the sentinel
            // write to upstage it. Log for the operator who's digging
            // into why the deferred cleanup didn't happen.
            System.Diagnostics.Trace.TraceWarning(
                $"Zvec: Failed to write quarantine sentinel for '{sourcePath}': {ex.Message}");
        }
    }

    /// <summary>
    /// If a sentinel exists for <paramref name="sourcePath"/>, execute its
    /// queued rename and delete the sentinel on success. Called by
    /// <see cref="ZoneTreeStorageEngine"/>'s constructor *before* it
    /// attempts <c>OpenOrCreate()</c>, so the store opens against a clean
    /// state rather than tripping over the same corrupt files again.
    ///
    /// <para>Returns silently on any of: no sentinel present, sentinel
    /// present but source already gone (stale — just delete the sentinel),
    /// sentinel present but target already exists (partial prior run —
    /// generate a new timestamp-suffixed target). Re-throws only if the
    /// sentinel itself is unreadable (parse error), because at that point
    /// the operator needs to intervene.</para>
    /// </summary>
    public static void Drain(string sourcePath)
    {
        var sentinelPath = SentinelPathFor(sourcePath);
        if (!File.Exists(sentinelPath))
            return;

        Entry? entry;
        try
        {
            var bytes = File.ReadAllBytes(sentinelPath);
            entry = JsonSerializer.Deserialize(bytes, SentinelContext.Default.Entry);
        }
        catch (Exception ex)
        {
            // Unreadable sentinel is a strong signal that something is
            // actively wrong with the data directory. Surface loudly
            // rather than silently discarding it — the operator will want
            // to know, and deleting an unparseable file would erase the
            // one breadcrumb they have.
            throw new IOException(
                $"Quarantine sentinel '{sentinelPath}' exists but could not be parsed. " +
                $"Remove it manually after inspecting, or restore from backup.", ex);
        }

        if (entry is null || string.IsNullOrEmpty(entry.Source) || string.IsNullOrEmpty(entry.Target))
        {
            // Malformed but parseable — treat as stale and remove.
            TryDeleteSentinel(sentinelPath);
            return;
        }

        // Source gone → prior run already completed the rename on a
        // different attempt, or the user cleaned up manually. Either way
        // the sentinel is stale.
        if (!Directory.Exists(entry.Source))
        {
            TryDeleteSentinel(sentinelPath);
            return;
        }

        // Target already taken (extremely rare: the earlier run partially
        // completed, or the user created a directory with that exact
        // name). Append a disambiguating suffix so we still quarantine
        // rather than clobber.
        var target = entry.Target;
        if (Directory.Exists(target) || File.Exists(target))
        {
            int n = 1;
            while (Directory.Exists($"{target}-{n}") || File.Exists($"{target}-{n}"))
                n++;
            target = $"{target}-{n}";
        }

        // The whole reason we deferred to next-startup is that the *prior*
        // process held mmap handles. In this fresh process there are no
        // such handles (yet — we haven't called OpenOrCreate), so the
        // rename should just work. If it doesn't, something external is
        // holding files (VS debugger still attached to a zombie process,
        // Explorer preview pane, antivirus on-access scan, etc.) and we
        // want that error to surface, not be silently swallowed.
        Directory.Move(entry.Source, target);
        System.Diagnostics.Trace.TraceInformation(
            $"Zvec: Drained pending quarantine — moved '{entry.Source}' -> '{target}' " +
            $"(originally queued {entry.QueuedAtUtc} for: {entry.Reason}).");

        TryDeleteSentinel(sentinelPath);
    }

    /// <summary>
    /// Best-effort sentinel cleanup. A leftover sentinel after a
    /// successful rename is cosmetic — the next startup would see "source
    /// gone" and quietly remove it — so we don't propagate errors.
    /// </summary>
    private static void TryDeleteSentinel(string sentinelPath)
    {
        try { File.Delete(sentinelPath); }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceWarning(
                $"Zvec: Failed to delete drained quarantine sentinel '{sentinelPath}': {ex.Message}");
        }
    }
}

/// <summary>
/// Source-generated JSON context for <see cref="QuarantineSentinel.Entry"/>.
/// Required for AOT compatibility — Zvec.Engine compiles with source-gen
/// serialization throughout. If you add fields to <c>Entry</c>, no code
/// change is needed here; the generator picks them up automatically.
/// </summary>
[System.Text.Json.Serialization.JsonSerializable(typeof(QuarantineSentinel.Entry))]
internal partial class SentinelContext : System.Text.Json.Serialization.JsonSerializerContext { }
