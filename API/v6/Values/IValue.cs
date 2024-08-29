namespace GMutagen.v6.Values;

public interface IValue<T> : IValue
{
    T Value { get; set; }
}