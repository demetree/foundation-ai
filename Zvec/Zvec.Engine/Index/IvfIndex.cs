// Copyright 2025-present the zvec project — Pure C# Engine
// IVF (Inverted File) Index — cluster-based approximate nearest neighbor search

using Foundation.AI.Zvec.Engine.Core;
using Foundation.AI.Zvec.Engine.Math;

namespace Foundation.AI.Zvec.Engine.Index;

/// <summary>
/// IVF (Inverted File) index for approximate nearest neighbor search.
///
/// <para><b>Algorithm Overview:</b>
/// IVF partitions the vector space into clusters (Voronoi cells) using k-means.
/// Each cluster has a centroid and an inverted list of vectors assigned to it.
/// At query time, only the nearest <c>nprobe</c> clusters are searched, reducing
/// the search space from O(n) to O(nprobe × n/nlist).</para>
///
/// <para><b>Training:</b>
/// IVF requires a training phase (<see cref="Train"/>) to compute centroids via k-means.
/// Before training completes, search falls back to brute-force linear scan.
/// Training should be triggered after a sufficient number of vectors are inserted
/// (minimum 10, but more vectors yield better cluster quality).</para>
///
/// <para><b>Quantization:</b>
/// Supports optional quantized inverted lists (INT8/INT4/FP16). When enabled,
/// both FP32 and quantized copies are maintained per cluster. The search path
/// uses ADC (Asymmetric Distance Computation): query stays FP32, stored vectors
/// are decompressed on-the-fly. Calibration happens during training.</para>
///
/// <para><b>Concurrency:</b>
/// Uses ReaderWriterLockSlim — multiple concurrent searches with single-writer
/// insert/delete/train.</para>
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

    /// <summary>Create a new IVF index.</summary>
    /// <param name="metric">Distance metric (must match your embedding model).</param>
    /// <param name="nlist">
    /// Number of clusters (Voronoi cells). More clusters = smaller inverted lists = faster search,
    /// but too many clusters relative to data size causes underpopulated lists.
    /// Rule of thumb: nlist ≈ √n where n is the expected number of vectors.
    /// </param>
    /// <param name="nprobe">
    /// Number of clusters to scan per query (default: 8).
    /// Higher nprobe = better recall but slower search. nprobe=1 is fastest but least accurate.
    /// nprobe=nlist degenerates to exact search. Start with nprobe = nlist/10.
    /// </param>
    /// <param name="quantize">
    /// Optional vector quantization type. Quantized inverted lists are maintained
    /// alongside FP32 lists. Calibration occurs during <see cref="Train"/>.
    /// </param>
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
                Lists = new List<IvfEntry>[_lists.Length],
                Int8CalMin = _int8Cal.MinVal,
                Int8CalMax = _int8Cal.MaxVal,
                Int4CalMin = _int4Cal.MinVal,
                Int4CalMax = _int4Cal.MaxVal,
                QCalibrated = _qCalibrated,
                Dimension = _dimension
            };

            for (int i = 0; i < _lists.Length; i++)
            {
                var entries = new List<IvfEntry>(_lists[i].Count);
                for (int j = 0; j < _lists[i].Count; j++)
                {
                    var (docId, vector) = _lists[i][j];
                    byte[]? qvec = (i < _qLists.Length && j < _qLists[i].Count)
                        ? _qLists[i][j].QVec : null;
                    entries.Add(new IvfEntry { DocId = docId, Vector = vector, QVec = qvec });
                }
                snapshot.Lists[i] = entries;
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

            // Restore quantization calibration
            _int8Cal = new Quantization.Int8Calibration(snapshot.Int8CalMin, snapshot.Int8CalMax);
            _int4Cal = new Quantization.Int4Calibration(snapshot.Int4CalMin, snapshot.Int4CalMax);
            _qCalibrated = snapshot.QCalibrated;
            if (snapshot.Dimension > 0)
                _dimension = snapshot.Dimension;

            // Restore centroids
            for (int i = 0; i < snapshot.Centroids.Length && i < _centroids.Length; i++)
                _centroids[i] = snapshot.Centroids[i];

            // Restore inverted lists, quantized lists, and doc map
            _docMap.Clear();
            for (int i = 0; i < _lists.Length; i++)
            {
                _lists[i].Clear();
                _qLists[i].Clear();
            }

            for (int i = 0; i < snapshot.Lists.Length && i < _lists.Length; i++)
            {
                foreach (var entry in snapshot.Lists[i])
                {
                    _lists[i].Add((entry.DocId, entry.Vector));
                    _docMap[entry.DocId] = (entry.Vector, i);
                    if (_dimension == 0 && entry.Vector.Length > 0)
                        _dimension = entry.Vector.Length;

                    // Restore quantized vector if present
                    if (entry.QVec != null)
                        _qLists[i].Add((entry.DocId, entry.QVec));
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
