namespace GMutagen.v6;

public interface IValue<T> : IValue
{
    T Value { get; set; }
}