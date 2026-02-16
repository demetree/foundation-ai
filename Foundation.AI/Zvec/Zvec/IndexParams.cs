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
/// HNSW index parameters.
/// </summary>
public sealed class HnswIndexParams : IndexParams
{
    private readonly MetricType _metric;
    private readonly int _m;
    private readonly int _efConstruction;
    private readonly QuantizeType _quantize;

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
/// Flat (brute-force) index parameters.
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
/// IVF index parameters.
/// </summary>
public sealed class IvfIndexParams : IndexParams
{
    private readonly MetricType _metric;
    private readonly int _nList;
    private readonly int _nIters;
    private readonly QuantizeType _quantize;

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
/// Inverted index parameters (for scalar fields).
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
