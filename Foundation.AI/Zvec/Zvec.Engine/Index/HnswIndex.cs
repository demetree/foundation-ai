// Copyright 2025-present the zvec project — Pure C# Engine
// HNSW (Hierarchical Navigable Small World) graph index
//
// References:
//   - Malkov & Yashunin, "Efficient and robust approximate nearest neighbor
//     using Hierarchical Navigable Small World graphs", 2018
//   - https://arxiv.org/abs/1603.09320

using System.Runtime.CompilerServices;
using Foundation.AI.Zvec.Engine.Core;
using Foundation.AI.Zvec.Engine.Math;

namespace Foundation.AI.Zvec.Engine.Index;

/// <summary>
/// HNSW vector index — graph-based approximate nearest neighbor search.
/// Supports concurrent search with single-writer insert.
/// </summary>
public sealed class HnswIndex : IVectorIndex
{
    // ── Configuration ───────────────────────────────────────────────────
    private readonly int _m;              // max connections per node (layer > 0)
    private readonly int _m0;             // max connections at layer 0 (typically 2*M)
    private readonly int _efConstruction; // beam width during insert
    private readonly int _maxLevel;       // calculated from M
    private readonly double _levelMult;   // 1 / ln(M)

    private readonly Func<ReadOnlySpan<float>, ReadOnlySpan<float>, float> _distFunc;
    private readonly bool _lowerIsBetter;

    // ── Graph storage ───────────────────────────────────────────────────
    private readonly List<HnswNode> _nodes = [];       // dense node list
    private readonly Dictionary<long, int> _docIdMap = []; // docId → node index
    private int _entryPoint = -1;                      // entry point node index
    private int _maxLayerReached;                       // current max layer in graph
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Random _rng = new();

    public IndexType Type => IndexType.Hnsw;
    public int Count => _nodes.Count;

    /// <summary>
    /// Create a new HNSW index.
    /// </summary>
    /// <param name="metric">Distance metric type.</param>
    /// <param name="m">Max connections per layer (default 16).</param>
    /// <param name="efConstruction">Build-time beam width (default 200).</param>
    public HnswIndex(MetricType metric, int m = 16, int efConstruction = 200)
    {
        _m = m <= 0 ? 16 : m;
        _m0 = _m * 2;
        _efConstruction = efConstruction <= 0 ? 200 : efConstruction;
        _levelMult = 1.0 / System.Math.Log(_m);
        _maxLevel = 30; // practical ceiling

        _distFunc = DistanceFunction.Get(metric);
        _lowerIsBetter = DistanceFunction.IsLowerBetter(metric);
    }

    // ====================================================================
    // Insert
    // ====================================================================

    public void Add(long docId, ReadOnlySpan<float> vector)
    {
        var vecCopy = vector.ToArray();
        int nodeLevel = RandomLevel();

        _lock.EnterWriteLock();
        try
        {
            int nodeIdx = _nodes.Count;
            var node = new HnswNode(docId, vecCopy, nodeLevel);
            _nodes.Add(node);
            _docIdMap[docId] = nodeIdx;

            // First node — just set as entry point
            if (_entryPoint < 0)
            {
                _entryPoint = nodeIdx;
                _maxLayerReached = nodeLevel;
                return;
            }

            int epIdx = _entryPoint;

            // Phase 1: Greedily descend from top layer to nodeLevel + 1
            for (int layer = _maxLayerReached; layer > nodeLevel; layer--)
            {
                epIdx = GreedyClosest(vecCopy, epIdx, layer);
            }

            // Phase 2: Insert at each layer from min(nodeLevel, maxLayer) down to 0
            for (int layer = System.Math.Min(nodeLevel, _maxLayerReached); layer >= 0; layer--)
            {
                var candidates = SearchLayer(vecCopy, epIdx, _efConstruction, layer);
                var neighbors = SelectNeighbors(candidates, layer == 0 ? _m0 : _m);

                // Connect new node to neighbors
                foreach (var (nIdx, _) in neighbors)
                {
                    node.AddConnection(layer, nIdx);
                    _nodes[nIdx].AddConnection(layer, nodeIdx);

                    // Shrink neighbor connections if over limit
                    int maxConn = layer == 0 ? _m0 : _m;
                    PruneConnections(_nodes[nIdx], layer, maxConn, vecCopy);
                }

                if (candidates.Count > 0)
                    epIdx = candidates[0].NodeIdx;
            }

            // Update entry point if new node has a higher level
            if (nodeLevel > _maxLayerReached)
            {
                _entryPoint = nodeIdx;
                _maxLayerReached = nodeLevel;
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    // ====================================================================
    // Search
    // ====================================================================

    public IReadOnlyList<SearchHit> Search(
        ReadOnlySpan<float> query,
        int topk,
        VectorQueryParams? queryParams = null,
        HashSet<long>? deletedFilter = null)
    {
        int ef = queryParams?.Ef > 0 ? queryParams.Ef : System.Math.Max(topk, _efConstruction);

        // Optionally fall back to linear scan
        if (queryParams?.UseLinearSearch == true)
            return LinearSearch(query, topk, deletedFilter);

        _lock.EnterReadLock();
        try
        {
            if (_entryPoint < 0 || _nodes.Count == 0)
                return [];

            int epIdx = _entryPoint;

            // Phase 1: Greedy descent from top layer to layer 1
            for (int layer = _maxLayerReached; layer >= 1; layer--)
            {
                epIdx = GreedyClosest(query, epIdx, layer);
            }

            // Phase 2: Search at layer 0 with beam width ef
            var candidates = SearchLayer(query, epIdx, ef, 0);

            // Filter deleted and collect top-k
            var results = new List<SearchHit>();
            foreach (var (nIdx, dist) in candidates)
            {
                if (deletedFilter != null && deletedFilter.Contains(_nodes[nIdx].DocId))
                    continue;
                results.Add(new SearchHit(_nodes[nIdx].DocId, dist));
                if (results.Count >= topk)
                    break;
            }

            // Sort by relevance
            if (_lowerIsBetter)
                results.Sort((a, b) => a.Score.CompareTo(b.Score));
            else
                results.Sort((a, b) => b.Score.CompareTo(a.Score));

            return results;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    // ====================================================================
    // Remove
    // ====================================================================

    public void Remove(long docId)
    {
        // HNSW doesn't support true deletion without rebuild.
        // We mark for exclusion; the caller tracks deleted IDs.
        // For correctness, we remove from the docId map.
        _lock.EnterWriteLock();
        try
        {
            _docIdMap.Remove(docId);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    // ====================================================================
    // Internal: Search a single layer with beam search
    // ====================================================================

    /// <summary>
    /// Search a single layer using greedy beam search. Returns candidates
    /// sorted by distance (best first for the configured metric).
    /// </summary>
    private List<(int NodeIdx, float Dist)> SearchLayer(
        ReadOnlySpan<float> query, int entryIdx, int ef, int layer)
    {
        float entryDist = Distance(query, _nodes[entryIdx].Vector);

        // candidates: best-first (sorted ascending for lower-is-better)
        var candidates = new SortedSet<(float Dist, int Idx)>(
            Comparer<(float, int)>.Create((a, b) =>
            {
                int c = a.Item1.CompareTo(b.Item1);
                return c != 0 ? c : a.Item2.CompareTo(b.Item2);
            }));

        var visited = new HashSet<int> { entryIdx };
        candidates.Add((entryDist, entryIdx));

        // result set: track the ef-nearest
        var result = new SortedSet<(float Dist, int Idx)>(candidates.Comparer);
        result.Add((entryDist, entryIdx));

        while (candidates.Count > 0)
        {
            // Pick best unprocessed candidate
            var (cDist, cIdx) = _lowerIsBetter ? candidates.Min : candidates.Max;
            candidates.Remove((cDist, cIdx));

            // Check stopping condition:
            // If this candidate is worse than the worst in our result set, stop
            var worst = _lowerIsBetter ? result.Max : result.Min;
            if (_lowerIsBetter ? cDist > worst.Dist : cDist < worst.Dist)
            {
                if (result.Count >= ef) break;
            }

            // Expand neighbors
            var connections = _nodes[cIdx].GetConnections(layer);
            foreach (int nIdx in connections)
            {
                if (!visited.Add(nIdx)) continue;

                float nDist = Distance(query, _nodes[nIdx].Vector);
                var worstResult = _lowerIsBetter ? result.Max : result.Min;

                if (result.Count < ef ||
                    (_lowerIsBetter ? nDist < worstResult.Dist : nDist > worstResult.Dist))
                {
                    candidates.Add((nDist, nIdx));
                    result.Add((nDist, nIdx));

                    if (result.Count > ef)
                    {
                        // Remove the worst
                        if (_lowerIsBetter)
                            result.Remove(result.Max);
                        else
                            result.Remove(result.Min);
                    }
                }
            }
        }

        // Return sorted best-first
        var sorted = result.ToList();
        if (!_lowerIsBetter)
            sorted.Reverse(); // for IP, Max is best, but SortedSet has Min first
        return sorted.Select(x => (x.Idx, x.Dist)).ToList();
    }

    /// <summary>
    /// Greedy walk to the single closest node at a given layer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GreedyClosest(ReadOnlySpan<float> query, int epIdx, int layer)
    {
        float bestDist = Distance(query, _nodes[epIdx].Vector);
        bool changed = true;

        while (changed)
        {
            changed = false;
            foreach (int nIdx in _nodes[epIdx].GetConnections(layer))
            {
                float d = Distance(query, _nodes[nIdx].Vector);
                if (IsBetter(d, bestDist))
                {
                    bestDist = d;
                    epIdx = nIdx;
                    changed = true;
                }
            }
        }

        return epIdx;
    }

    // ====================================================================
    // Internal: Neighbor selection and pruning
    // ====================================================================

    /// <summary>
    /// Simple neighbor selection — pick the best M neighbors from candidates.
    /// </summary>
    private List<(int NodeIdx, float Dist)> SelectNeighbors(
        List<(int NodeIdx, float Dist)> candidates, int maxNeighbors)
    {
        // candidates are already sorted best-first
        return candidates.Take(maxNeighbors).ToList();
    }

    /// <summary>
    /// Prune a node's connections if they exceed the maximum.
    /// Keeps the best connections by distance.
    /// </summary>
    private void PruneConnections(HnswNode node, int layer, int maxConn,
                                   ReadOnlySpan<float> _)
    {
        var connections = node.GetConnections(layer);
        if (connections.Count <= maxConn) return;

        // Score all connections by distance to the node
        var scored = new List<(int Idx, float Dist)>(connections.Count);
        foreach (int nIdx in connections)
        {
            float d = Distance(node.Vector, _nodes[nIdx].Vector);
            scored.Add((nIdx, d));
        }

        // Sort best-first and keep top maxConn
        if (_lowerIsBetter)
            scored.Sort((a, b) => a.Dist.CompareTo(b.Dist));
        else
            scored.Sort((a, b) => b.Dist.CompareTo(a.Dist));

        node.SetConnections(layer, scored.Take(maxConn).Select(s => s.Idx).ToList());
    }

    // ====================================================================
    // Internal: Linear (brute-force) fallback
    // ====================================================================

    private IReadOnlyList<SearchHit> LinearSearch(
        ReadOnlySpan<float> query, int topk, HashSet<long>? deletedFilter)
    {
        // Reuse FlatIndex logic
        var comparer = _lowerIsBetter
            ? Comparer<float>.Create((a, b) => b.CompareTo(a))
            : Comparer<float>.Default;

        var heap = new PriorityQueue<long, float>(comparer);

        foreach (var node in _nodes)
        {
            if (deletedFilter != null && deletedFilter.Contains(node.DocId))
                continue;

            float dist = Distance(query, node.Vector);
            if (heap.Count < topk)
                heap.Enqueue(node.DocId, dist);
            else
                heap.EnqueueDequeue(node.DocId, dist);
        }

        var results = new List<SearchHit>(heap.Count);
        while (heap.Count > 0)
        {
            heap.TryDequeue(out long id, out float score);
            results.Add(new SearchHit(id, score));
        }

        if (_lowerIsBetter)
            results.Sort((a, b) => a.Score.CompareTo(b.Score));
        else
            results.Sort((a, b) => b.Score.CompareTo(a.Score));

        return results;
    }

    // ====================================================================
    // Helpers
    // ====================================================================

    private int RandomLevel()
    {
        double r = _rng.NextDouble();
        int level = (int)(-System.Math.Log(r) * _levelMult);
        return System.Math.Min(level, _maxLevel);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float Distance(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
        => _distFunc(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBetter(float candidate, float current)
        => _lowerIsBetter ? candidate < current : candidate > current;

    public void Dispose()
    {
        _lock.Dispose();
    }
}

// ========================================================================
// HNSW Node
// ========================================================================

/// <summary>
/// A single node in the HNSW graph. Holds the vector, doc ID, and
/// per-layer connection lists.
/// </summary>
internal sealed class HnswNode
{
    public readonly long DocId;
    public readonly float[] Vector;
    public readonly int Level;

    // connections[layer] = list of neighbor node indices
    private readonly List<int>[] _connections;

    public HnswNode(long docId, float[] vector, int level)
    {
        DocId = docId;
        Vector = vector;
        Level = level;
        _connections = new List<int>[level + 1];
        for (int i = 0; i <= level; i++)
            _connections[i] = [];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<int> GetConnections(int layer)
    {
        if (layer >= _connections.Length)
            return [];
        return _connections[layer];
    }

    public void AddConnection(int layer, int nodeIdx)
    {
        if (layer >= _connections.Length) return;
        var conn = _connections[layer];
        if (!conn.Contains(nodeIdx))
            conn.Add(nodeIdx);
    }

    public void SetConnections(int layer, List<int> connections)
    {
        if (layer < _connections.Length)
            _connections[layer] = connections;
    }
}
