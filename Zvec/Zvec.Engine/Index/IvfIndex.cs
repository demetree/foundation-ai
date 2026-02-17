// Copyright 2025-present the zvec project — Pure C# Engine
// IVF (Inverted File) Index — cluster-based approximate nearest neighbor search

using Foundation.AI.Zvec.Engine.Core;
using Foundation.AI.Zvec.Engine.Math;

namespace Foundation.AI.Zvec.Engine.Index;

/// <summary>
/// IVF (Inverted File) index for approximate nearest neighbor search.
/// Partitions vectors into clusters via mini-batch k-means, then
/// searches only the nearest nprobe clusters at query time.
/// </summary>
public sealed class IvfIndex : IVectorIndex
{
    private readonly Func<ReadOnlySpan<float>, ReadOnlySpan<float>, float> _distFunc;
    private readonly bool _lowerIsBetter;
    private readonly int _nlist;
    private readonly int _nprobe;

    // Quantization
    private readonly QuantizeType _quantize;
    private Quantization.Int8Calibration _int8Cal;
    private Quantization.Int4Calibration _int4Cal;
    private bool _qCalibrated;

    // Cluster centroids (always FP32 for partition accuracy)
    private float[][] _centroids;

    // Inverted lists: _lists[clusterIdx] = list of (docId, vector)
    private readonly List<(long DocId, float[] Vector)>[] _lists;

    // All vectors stored for training
    private readonly Dictionary<long, (float[] Vector, int ClusterIdx)> _docMap = new();

    // Quantized storage: parallel to _lists entries
    private readonly List<(long DocId, byte[] QVec)>[] _qLists;

    private readonly ReaderWriterLockSlim _lock = new();
    private bool _trained;
    private int _dimension;

    private const int MaxKMeansIterations = 20;
    private const int MinTrainingSamples = 10;

    public IvfIndex(MetricType metric, int nlist = 128, int nprobe = 8,
                    QuantizeType quantize = QuantizeType.Undefined)
    {
        _distFunc = DistanceFunction.Get(metric);
        _lowerIsBetter = DistanceFunction.IsLowerBetter(metric);
        _nlist = nlist;
        _nprobe = nprobe;
        _quantize = quantize;
        _centroids = new float[nlist][];
        _lists = new List<(long, float[])>[nlist];
        _qLists = new List<(long, byte[])>[nlist];
        for (int i = 0; i < nlist; i++)
        {
            _lists[i] = new List<(long, float[])>();
            _qLists[i] = new List<(long, byte[])>();
        }
    }

    public IndexType Type => IndexType.Ivf;
    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try { return _docMap.Count; }
            finally { _lock.ExitReadLock(); }
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Add
    // ═══════════════════════════════════════════════════════════════════

    public void Add(long docId, ReadOnlySpan<float> vector)
    {
        var copy = vector.ToArray();
        if (_dimension == 0) _dimension = copy.Length;

        // Auto-calibrate quantization
        if (IsQuantized && !_qCalibrated)
            CalibrateQ(copy);

        _lock.EnterWriteLock();
        try
        {
            if (!_trained)
            {
                _docMap[docId] = (copy, -1);
                return;
            }

            int cluster = FindNearestCentroid(copy);
            _lists[cluster].Add((docId, copy));
            _docMap[docId] = (copy, cluster);

            // Also store quantized version
            if (IsQuantized)
                _qLists[cluster].Add((docId, QuantizeVec(copy)));
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Search
    // ═══════════════════════════════════════════════════════════════════

    public IReadOnlyList<SearchHit> Search(
        ReadOnlySpan<float> query,
        int topk,
        VectorQueryParams? queryParams = null,
        HashSet<long>? deletedFilter = null)
    {
        int nprobe = queryParams?.NProbe > 0 ? queryParams.NProbe : _nprobe;

        _lock.EnterReadLock();
        try
        {
            if (!_trained || _docMap.Count == 0)
            {
                // Fallback to linear scan of all stored vectors
                return LinearSearch(query, topk, deletedFilter);
            }

            // 1. Find nearest nprobe centroids
            var centroidDists = new (int ClusterIdx, float Distance)[_nlist];
            for (int i = 0; i < _nlist; i++)
            {
                float d = _distFunc(query, _centroids[i]);
                centroidDists[i] = (i, d);
            }

            Array.Sort(centroidDists, (a, b) =>
                _lowerIsBetter ? a.Distance.CompareTo(b.Distance)
                               : b.Distance.CompareTo(a.Distance));

            // 2. Search within the top nprobe clusters
            var heap = new PriorityQueue<SearchHit, float>();
            int actualProbe = System.Math.Min(nprobe, _nlist);

            for (int p = 0; p < actualProbe; p++)
            {
                int clusterIdx = centroidDists[p].ClusterIdx;

                if (IsQuantized && _qLists[clusterIdx].Count == _lists[clusterIdx].Count)
                {
                    // Use quantized vectors for distance computation (ADC)
                    foreach (var (docId, qvec) in _qLists[clusterIdx])
                    {
                        if (deletedFilter != null && deletedFilter.Contains(docId))
                            continue;

                        var decompressed = DecompressVec(qvec);
                        float dist = _distFunc(query, decompressed);

                        if (heap.Count < topk)
                            heap.Enqueue(new SearchHit(docId, dist),
                                _lowerIsBetter ? -dist : dist);
                        else
                            heap.EnqueueDequeue(new SearchHit(docId, dist),
                                _lowerIsBetter ? -dist : dist);
                    }
                }
                else
                {
                    // Fallback to FP32 vectors
                    foreach (var (docId, vec) in _lists[clusterIdx])
                    {
                        if (deletedFilter != null && deletedFilter.Contains(docId))
                            continue;

                        float dist = _distFunc(query, vec);

                        if (heap.Count < topk)
                            heap.Enqueue(new SearchHit(docId, dist),
                                _lowerIsBetter ? -dist : dist);
                        else
                            heap.EnqueueDequeue(new SearchHit(docId, dist),
                                _lowerIsBetter ? -dist : dist);
                    }
                }
            }

            // Extract results in best-first order
            var results = new SearchHit[heap.Count];
            for (int i = results.Length - 1; i >= 0; i--)
                results[i] = heap.Dequeue();
            return results;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Training (k-means clustering)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Train the IVF index by running k-means on all stored vectors.
    /// This must be called before search can use clusters.
    /// </summary>
    public void Train()
    {
        _lock.EnterWriteLock();
        try
        {
            var allVectors = _docMap.Values.Select(v => v.Vector).ToList();
            if (allVectors.Count < MinTrainingSamples)
                return;

            int actualClusters = System.Math.Min(_nlist, allVectors.Count);
            _centroids = KMeans(allVectors, actualClusters, _dimension);

            // Calibrate quantization from all vectors
            if (IsQuantized)
            {
                switch (_quantize)
                {
                    case QuantizeType.Int8:
                        _int8Cal = Quantization.CalibrateInt8Batch(allVectors);
                        break;
                    case QuantizeType.Int4:
                        _int4Cal = Quantization.CalibrateInt4Batch(allVectors);
                        break;
                }
                _qCalibrated = true;
            }

            // Clear and reassign all vectors to clusters
            for (int i = 0; i < _lists.Length; i++)
            {
                _lists[i].Clear();
                _qLists[i].Clear();
            }

            var updatedMap = new Dictionary<long, (float[] Vector, int ClusterIdx)>();
            foreach (var (docId, (vec, _)) in _docMap)
            {
                int cluster = FindNearestCentroid(vec);
                _lists[cluster].Add((docId, vec));
                updatedMap[docId] = (vec, cluster);

                if (IsQuantized)
                    _qLists[cluster].Add((docId, QuantizeVec(vec)));
            }

            foreach (var (k, v) in updatedMap)
                _docMap[k] = v;

            _trained = true;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Remove
    // ═══════════════════════════════════════════════════════════════════

    public void Remove(long docId)
    {
        _lock.EnterWriteLock();
        try
        {
            if (_docMap.TryGetValue(docId, out var entry))
            {
                _docMap.Remove(docId);
                if (entry.ClusterIdx >= 0 && entry.ClusterIdx < _lists.Length)
                {
                    _lists[entry.ClusterIdx].RemoveAll(x => x.DocId == docId);
                    _qLists[entry.ClusterIdx].RemoveAll(x => x.DocId == docId);
                }
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // Snapshot (for persistence)
    // ═══════════════════════════════════════════════════════════════════

    public IvfSnapshot GetSnapshot()
    {
        _lock.EnterReadLock();
        try
        {
            var snapshot = new IvfSnapshot
            {
                Trained = _trained,
                Centroids = _centroids.Where(c => c != null).ToArray(),
                Lists = new List<IvfEntry>[_lists.Length]
            };

            for (int i = 0; i < _lists.Length; i++)
            {
                snapshot.Lists[i] = _lists[i]
                    .Select(x => new IvfEntry { DocId = x.DocId, Vector = x.Vector })
                    .ToList();
            }

            return snapshot;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void LoadSnapshot(IvfSnapshot snapshot)
    {
        _lock.EnterWriteLock();
        try
        {
            _trained = snapshot.Trained;

            // Restore centroids
            for (int i = 0; i < snapshot.Centroids.Length && i < _centroids.Length; i++)
                _centroids[i] = snapshot.Centroids[i];

            // Restore inverted lists and doc map
            _docMap.Clear();
            for (int i = 0; i < _lists.Length; i++)
                _lists[i].Clear();

            for (int i = 0; i < snapshot.Lists.Length && i < _lists.Length; i++)
            {
                foreach (var entry in snapshot.Lists[i])
                {
                    _lists[i].Add((entry.DocId, entry.Vector));
                    _docMap[entry.DocId] = (entry.Vector, i);
                    if (_dimension == 0 && entry.Vector.Length > 0)
                        _dimension = entry.Vector.Length;
                }
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }

    // ═══════════════════════════════════════════════════════════════════
    // Private helpers
    // ═══════════════════════════════════════════════════════════════════

    private int FindNearestCentroid(float[] vector)
    {
        int bestIdx = 0;
        float bestDist = _distFunc(vector, _centroids[0]);

        for (int i = 1; i < _centroids.Length; i++)
        {
            if (_centroids[i] == null) break; // fewer actual clusters than _nlist
            float d = _distFunc(vector, _centroids[i]);

            bool isBetter = _lowerIsBetter ? d < bestDist : d > bestDist;
            if (isBetter)
            {
                bestDist = d;
                bestIdx = i;
            }
        }

        return bestIdx;
    }

    private IReadOnlyList<SearchHit> LinearSearch(
        ReadOnlySpan<float> query, int topk, HashSet<long>? deletedFilter)
    {
        var heap = new PriorityQueue<SearchHit, float>();

        foreach (var (docId, (vec, _)) in _docMap)
        {
            if (deletedFilter != null && deletedFilter.Contains(docId))
                continue;

            float dist = _distFunc(query, vec);

            if (heap.Count < topk)
                heap.Enqueue(new SearchHit(docId, dist),
                    _lowerIsBetter ? -dist : dist);
            else
                heap.EnqueueDequeue(new SearchHit(docId, dist),
                    _lowerIsBetter ? -dist : dist);
        }

        var results = new SearchHit[heap.Count];
        for (int i = results.Length - 1; i >= 0; i--)
            results[i] = heap.Dequeue();
        return results;
    }

    /// <summary>
    /// Simple k-means clustering. Returns centroid vectors.
    /// </summary>
    private float[][] KMeans(List<float[]> vectors, int k, int dim)
    {
        var rng = new Random(42);
        var centroids = new float[k][];

        // Initialize centroids with k-means++ style initialization
        // First centroid: random
        centroids[0] = (float[])vectors[rng.Next(vectors.Count)].Clone();

        for (int c = 1; c < k; c++)
        {
            // Pick next centroid weighted by distance from nearest existing centroid
            var distances = new float[vectors.Count];
            float totalDist = 0;
            for (int v = 0; v < vectors.Count; v++)
            {
                float minDist = float.MaxValue;
                for (int ci = 0; ci < c; ci++)
                {
                    float d = SimdDistance.EuclideanSquared(vectors[v], centroids[ci]);
                    if (d < minDist) minDist = d;
                }
                distances[v] = minDist;
                totalDist += minDist;
            }

            // Roulette wheel selection
            float target = (float)(rng.NextDouble() * totalDist);
            float cumulative = 0;
            for (int v = 0; v < vectors.Count; v++)
            {
                cumulative += distances[v];
                if (cumulative >= target)
                {
                    centroids[c] = (float[])vectors[v].Clone();
                    break;
                }
            }
            centroids[c] ??= (float[])vectors[rng.Next(vectors.Count)].Clone();
        }

        // Lloyd's algorithm
        var assignments = new int[vectors.Count];
        for (int iter = 0; iter < MaxKMeansIterations; iter++)
        {
            bool changed = false;

            // Assignment step: assign each vector to nearest centroid
            for (int v = 0; v < vectors.Count; v++)
            {
                int best = 0;
                float bestDist = SimdDistance.EuclideanSquared(vectors[v], centroids[0]);
                for (int c = 1; c < k; c++)
                {
                    float d = SimdDistance.EuclideanSquared(vectors[v], centroids[c]);
                    if (d < bestDist) { bestDist = d; best = c; }
                }
                if (assignments[v] != best) { assignments[v] = best; changed = true; }
            }

            if (!changed) break; // converged

            // Update step: recompute centroids
            var sums = new float[k][];
            var counts = new int[k];
            for (int c = 0; c < k; c++)
                sums[c] = new float[dim];

            for (int v = 0; v < vectors.Count; v++)
            {
                int cluster = assignments[v];
                counts[cluster]++;
                for (int d = 0; d < dim; d++)
                    sums[cluster][d] += vectors[v][d];
            }

            for (int c = 0; c < k; c++)
            {
                if (counts[c] > 0)
                {
                    for (int d = 0; d < dim; d++)
                        centroids[c][d] = sums[c][d] / counts[c];
                }
            }
        }

        return centroids;
    }

    // ═══════════════════════════════════════════════════════════════════
    // Quantization helpers
    // ═══════════════════════════════════════════════════════════════════

    private bool IsQuantized => _quantize != QuantizeType.Undefined;

    private void CalibrateQ(float[] vector)
    {
        switch (_quantize)
        {
            case QuantizeType.Int8:
                _int8Cal = Quantization.CalibrateInt8(vector);
                _qCalibrated = true;
                break;
            case QuantizeType.Int4:
                _int4Cal = Quantization.CalibrateInt4(vector);
                _qCalibrated = true;
                break;
            case QuantizeType.FP16:
                _qCalibrated = true;
                break;
        }
    }

    private byte[] QuantizeVec(float[] vector) => _quantize switch
    {
        QuantizeType.FP16 => FP16ToBytes(Quantization.ToFP16(vector)),
        QuantizeType.Int8 => Quantization.ToInt8(vector, _int8Cal),
        QuantizeType.Int4 => Quantization.ToInt4(vector, _int4Cal),
        _ => throw new InvalidOperationException($"Unsupported: {_quantize}")
    };

    private float[] DecompressVec(byte[] quantized) => _quantize switch
    {
        QuantizeType.FP16 => Quantization.FromFP16(BytesToFP16(quantized)),
        QuantizeType.Int8 => Quantization.FromInt8(quantized, _int8Cal),
        QuantizeType.Int4 => Quantization.FromInt4(quantized, _dimension, _int4Cal),
        _ => throw new InvalidOperationException($"Unsupported: {_quantize}")
    };

    private static byte[] FP16ToBytes(Half[] halfs)
    {
        var bytes = new byte[halfs.Length * 2];
        Buffer.BlockCopy(halfs, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    private static Half[] BytesToFP16(byte[] bytes)
    {
        var halfs = new Half[bytes.Length / 2];
        Buffer.BlockCopy(bytes, 0, halfs, 0, bytes.Length);
        return halfs;
    }
}
