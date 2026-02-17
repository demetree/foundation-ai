namespace Foundation.AI.Zvec;

/// <summary>
/// Base class for index parameter configurations.
/// Now a pure managed POCO — no native handles.
/// </summary>
public abstract class IndexParams : IDisposable
{
    internal Zvec.Engine.Core.IndexConfig ToEngineConfig() => BuildConfig();
    protected abstract Zvec.Engine.Core.IndexConfig BuildConfig();
    public void Dispose() { GC.SuppressFinalize(this); }
    ~IndexParams() => Dispose();
}

/// <summary>
/// HNSW index parameters — best general-purpose choice for most workloads.
///
/// <para><b>When to use:</b> Default choice for datasets under ~10M vectors.
/// Offers the best recall/speed tradeoff of all index types.
/// Supports incremental inserts (no training phase required).</para>
/// </summary>
public sealed class HnswIndexParams : IndexParams
{
    private readonly MetricType _metric;
    private readonly int _m;
    private readonly int _efConstruction;
    private readonly QuantizeType _quantize;

    /// <param name="metric">Distance metric (default: IP). Must match your embedding model.</param>
    /// <param name="m">
    /// Max connections per node (default: auto/16). Higher M = better recall, more memory.
    /// Recommended: 12–48. Set 0 for engine default (16).
    /// </param>
    /// <param name="efConstruction">
    /// Build-time beam width (default: 200). Higher = better graph quality, slower build.
    /// Recommended: 100–500. This does NOT affect search speed.
    /// </param>
    /// <param name="quantize">Optional compression. Reduces memory 2–8× with small accuracy cost.</param>
    public HnswIndexParams(
        MetricType metric = MetricType.IP,
        int m = 0,
        int efConstruction = 0,
        QuantizeType quantize = QuantizeType.Undefined)
    {
        _metric = metric;
        _m = m;
        _efConstruction = efConstruction;
        _quantize = quantize;
    }

    protected override Engine.Core.IndexConfig BuildConfig() => new()
    {
        Type = Engine.Core.IndexType.Hnsw,
        Metric = MapMetric(_metric),
        M = _m,
        EfConstruction = _efConstruction == 0 ? 200 : _efConstruction,
        Quantize = (Engine.Core.QuantizeType)(int)_quantize,
    };

    internal static Engine.Math.MetricType MapMetric(MetricType m) => m switch
    {
        MetricType.L2 => Engine.Math.MetricType.L2,
        MetricType.IP => Engine.Math.MetricType.InnerProduct,
        MetricType.Cosine => Engine.Math.MetricType.Cosine,
        _ => Engine.Math.MetricType.InnerProduct
    };
}

/// <summary>
/// Flat (brute-force) index parameters — exact search, no approximation.
///
/// <para><b>When to use:</b> Small datasets (&lt; 10K vectors) where exact results are needed,
/// or as a baseline for benchmarking ANN recall. No build phase required.
/// O(n) per query — becomes impractical above ~100K vectors.</para>
/// </summary>
public sealed class FlatIndexParams : IndexParams
{
    private readonly MetricType _metric;
    private readonly QuantizeType _quantize;

    public FlatIndexParams(
        MetricType metric = MetricType.IP,
        QuantizeType quantize = QuantizeType.Undefined)
    {
        _metric = metric;
        _quantize = quantize;
    }

    protected override Engine.Core.IndexConfig BuildConfig() => new()
    {
        Type = Engine.Core.IndexType.Flat,
        Metric = HnswIndexParams.MapMetric(_metric),
        Quantize = (Engine.Core.QuantizeType)(int)_quantize,
    };
}

/// <summary>
/// IVF (Inverted File) index parameters — cluster-based search.
///
/// <para><b>When to use:</b> Large datasets (1M+ vectors) with batch insert patterns.
/// Requires a training phase (<c>Optimize()</c>) before clustered search works.
/// Before training, falls back to brute-force scan.</para>
/// </summary>
public sealed class IvfIndexParams : IndexParams
{
    private readonly MetricType _metric;
    private readonly int _nList;
    private readonly int _nIters;
    private readonly QuantizeType _quantize;

    /// <param name="metric">Distance metric (default: IP). Must match your embedding model.</param>
    /// <param name="nList">
    /// Number of clusters. Rule of thumb: nlist ≈ √(n).
    /// Set 0 for engine default (128).
    /// </param>
    /// <param name="nIters">
    /// K-means training iterations. Set 0 for engine default (20).
    /// More iterations improve cluster quality at diminishing returns.
    /// </param>
    /// <param name="quantize">Optional compression for inverted lists.</param>
    public IvfIndexParams(
        MetricType metric = MetricType.IP,
        int nList = 0,
        int nIters = 0,
        QuantizeType quantize = QuantizeType.Undefined)
    {
        _metric = metric;
        _nList = nList;
        _nIters = nIters;
        _quantize = quantize;
    }

    protected override Engine.Core.IndexConfig BuildConfig() => new()
    {
        Type = Engine.Core.IndexType.Ivf,
        Metric = HnswIndexParams.MapMetric(_metric),
        Nlist = _nList,
        NIters = _nIters,
        Quantize = (Engine.Core.QuantizeType)(int)_quantize,
    };
}

/// <summary>
/// Inverted index parameters for scalar fields — enables filtered queries.
///
/// <para><b>When to use:</b> Add to scalar fields (strings, numbers) that you want
/// to filter on during vector searches. Range optimization enables efficient
/// greater-than / less-than comparisons on numeric fields.</para>
/// </summary>
public sealed class InvertIndexParams : IndexParams
{
    private readonly bool _enableRangeOptimization;

    public InvertIndexParams(bool enableRangeOptimization = true)
    {
        _enableRangeOptimization = enableRangeOptimization;
    }

    protected override Engine.Core.IndexConfig BuildConfig() => new()
    {
        Type = Engine.Core.IndexType.Invert,
        EnableRangeOptimization = _enableRangeOptimization,
    };
}
