namespace GMutagen.v6;

public class LazyValue<T> : IValue<T>
{
    private readonly IGenerator<IValue<T>> _valueGenerator;
    private IValue<T>? _cachedValue;

    public LazyValue(IGenerator<IValue<T>> valueGenerator)
    {
        _valueGenerator = valueGenerator;
    }

    public T Value
    {
        get => (_cachedValue ??= _valueGenerator.Generate()).Value;
        set => (_cachedValue ??= _valueGenerator.Generate()).Value = value;
    }
}