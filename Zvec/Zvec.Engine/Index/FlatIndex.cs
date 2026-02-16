// Copyright 2025-present the zvec project — Pure C# Engine
// Flat (brute-force) vector index

using Foundation.AI.Zvec.Engine.Core;
using Foundation.AI.Zvec.Engine.Math;

namespace Foundation.AI.Zvec.Engine.Index;

/// <summary>
/// Brute-force vector index. Compares every stored vector against the query
/// using SIMD-accelerated distance functions. Simple, exact, and correct —
/// suitable as the baseline implementation and for small collections.
/// </summary>
public sealed class FlatIndex : IVectorIndex
{
    private readonly Func<ReadOnlySpan<float>, ReadOnlySpan<float>, float> _distFunc;
    private readonly bool _lowerIsBetter;
    private readonly List<(long DocId, float[] Vector)> _vectors = [];
    private readonly object _lock = new();

    public IndexType Type => IndexType.Flat;

    public int Count
    {
        get { lock (_lock) return _vectors.Count; }
    }

    public FlatIndex(MetricType metric)
    {
        _distFunc = DistanceFunction.Get(metric);
        _lowerIsBetter = DistanceFunction.IsLowerBetter(metric);
    }

    public void Add(long docId, ReadOnlySpan<float> vector)
    {
        var copy = vector.ToArray();
        lock (_lock)
        {
            _vectors.Add((docId, copy));
        }
    }

    public IReadOnlyList<SearchHit> Search(
        ReadOnlySpan<float> query,
        int topk,
        VectorQueryParams? queryParams = null,
        HashSet<long>? deletedFilter = null)
    {
        // PriorityQueue dequeues the element with the LOWEST priority first.
        // We want a bounded heap that keeps the top-k "best" results.
        //
        // For "lower is better" (L2/cosine): best = smallest distance.
        //   We want to evict the LARGEST (worst) distance → max-heap → reverse comparer.
        //   PQ dequeues largest first, so EnqueueDequeue replaces the worst.
        //
        // For "higher is better" (IP): best = largest score.
        //   We want to evict the SMALLEST (worst) score → min-heap → default comparer.
        //   PQ dequeues smallest first, so EnqueueDequeue replaces the worst.
        var comparer = _lowerIsBetter
            ? Comparer<float>.Create((a, b) => b.CompareTo(a)) // max-heap: dequeue largest
            : Comparer<float>.Default;                          // min-heap: dequeue smallest

        var heap = new PriorityQueue<long, float>(comparer);

        lock (_lock)
        {
            foreach (var (docId, vec) in _vectors)
            {
                if (deletedFilter != null && deletedFilter.Contains(docId))
                    continue;

                float dist = _distFunc(query, vec);

                if (heap.Count < topk)
                {
                    heap.Enqueue(docId, dist);
                }
                else
                {
                    heap.EnqueueDequeue(docId, dist);
                }
            }
        }

        // Drain heap into results
        var results = new List<SearchHit>(heap.Count);
        while (heap.Count > 0)
        {
            heap.TryDequeue(out long id, out float score);
            results.Add(new SearchHit(id, score));
        }

        // Sort: best first
        if (_lowerIsBetter)
            results.Sort((a, b) => a.Score.CompareTo(b.Score));
        else
            results.Sort((a, b) => b.Score.CompareTo(a.Score));

        return results;
    }

    public void Remove(long docId)
    {
        lock (_lock)
        {
            _vectors.RemoveAll(v => v.DocId == docId);
        }
    }

    public void Dispose()
    {
        // No unmanaged resources to release
    }
}
