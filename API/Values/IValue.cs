namespace GMutagen.Values;

public interface IValue<T> : IValue
{
    T Value { get; set; }
}
public interface IValue { }