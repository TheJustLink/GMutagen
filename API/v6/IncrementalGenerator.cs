using System.Numerics;

namespace GMutagen.v6;

public class IncrementalGenerator<T> : IValue<T>, IGenerator<T> where T : IAdditionOperators<T, int, T>
{
    public IncrementalGenerator() : this(default(T))
    {
    }

    public IncrementalGenerator(T value)
    {
        Value = value;
    }

    public T Value { get; set; }

    public T Generate()
    {
        var id = Value;

        Value += 1;

        return id;
    }
}