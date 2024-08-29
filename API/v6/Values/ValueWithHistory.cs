namespace GMutagen.v6.Values;

public class ValueWithHistory<T> : IValue<T>
{
    private readonly IValue<T> _source;
    private T _previousValue;

    public ValueWithHistory(IValue<T> source, bool preInit = false)
    {
        _source = source;

        if (preInit)
            _previousValue = source.Value;
    }

    public T Value
    {
        get => _previousValue;
        set
        {
            _previousValue = _source.Value;
            _source.Value = value;
        }
    }
}