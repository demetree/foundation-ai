using Foundation.AI.Zvec.Engine.Index;

namespace Foundation.AI.Zvec;

/// <summary>
/// Base class for query parameter configurations.
/// Now a pure managed POCO — no native handles.
/// </summary>
public abstract class QueryParams : IDisposable
{
    internal VectorQueryParams ToEngineParams() => BuildParams();
    protected abstract VectorQueryParams BuildParams();
    public void Dispose() { GC.SuppressFinalize(this); }
    ~QueryParams() => Dispose();
}

/// <summary>
/// HNSW query parameters — controls the recall/speed tradeoff at search time.
/// </summary>
public sealed class HnswQueryParams : QueryParams
{
    private readonly int _ef;
    private readonly float _radius;
    private readonly bool _useLinearSearch;

    /// <param name="ef">
    /// Search beam width. Higher ef = better recall but slower search. Default: 0 (uses index efConstruction).
    /// Start with 64. Values above 200 give diminishing returns.
    /// Setting ef ≥ collection size degenerates to exact search.
    /// </param>
    /// <param name="radius">
    /// Optional distance radius filter. Only return results within this distance.
    /// Set 0 to disable (return top-k regardless of distance).
    /// </param>
    /// <param name="useLinearSearch">
    /// Force brute-force scan instead of graph traversal. Useful for very small
    /// collections or when exact results are required.
    /// </param>
    public HnswQueryParams(int ef = 0, float radius = 0f, bool useLinearSearch = false)
    {
        _ef = ef;
        _radius = radius;
        _useLinearSearch = useLinearSearch;
    }

    protected override VectorQueryParams BuildParams() => new()
    {
        Ef = _ef,
        Radius = _radius,
        UseLinearSearch = _useLinearSearch
    };
}

/// <summary>
/// Flat (brute-force) query parameters.
/// Since Flat performs exact search, the only tunable is the distance radius filter.
/// </summary>
public sealed class FlatQueryParams : QueryParams
{
    private readonly float _radius;

    public FlatQueryParams(float radius = 0f)
    {
        _radius = radius;
    }

    protected override VectorQueryParams BuildParams() => new()
    {
        Radius = _radius
    };
}

/// <summary>
/// IVF query parameters — controls cluster scan breadth.
/// </summary>
public sealed class IvfQueryParams : QueryParams
{
    private readonly int _nprobe;
    private readonly float _radius;

    /// <param name="nprobe">
    /// Number of clusters to scan per query. Higher = better recall, slower.
    /// Default: 0 (uses index default, typically 8). Start with nlist/10.
    /// nprobe=nlist gives exact search (scans all clusters).
    /// </param>
    /// <param name="radius">
    /// Optional distance radius filter. Set 0 to disable.
    /// </param>
    public IvfQueryParams(int nprobe = 0, float radius = 0f)
    {
        _nprobe = nprobe;
        _radius = radius;
    }

    protected override VectorQueryParams BuildParams() => new()
    {
        NProbe = _nprobe,
        Radius = _radius
    };
}
