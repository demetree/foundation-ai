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
/// HNSW query parameters.
/// </summary>
public sealed class HnswQueryParams : QueryParams
{
    private readonly int _ef;
    private readonly float _radius;
    private readonly bool _useLinearSearch;

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
/// Flat query parameters.
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
/// IVF query parameters.
/// </summary>
public sealed class IvfQueryParams : QueryParams
{
    private readonly int _nprobe;
    private readonly float _radius;

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
