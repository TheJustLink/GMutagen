namespace DataFlow;

public class FlowComparer : IEqualityComparer<IFilter>
{
    private readonly IEqualityComparer<IFilter> _defaultComparer;

    public FlowComparer() : this(EqualityComparer<IFilter>.Default)
    {
    }

    public FlowComparer(IEqualityComparer<IFilter> defaultComparer)
    {
        _defaultComparer = defaultComparer;
    }

    public bool Equals(IFilter x, IFilter y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x != null && x.Equals(y))
            return true;

        if (_defaultComparer.Equals(x, y))
            return true;
            
        return false;
    }

    public int GetHashCode(IFilter obj)
    {
        return _defaultComparer.GetHashCode(obj);
    }
}