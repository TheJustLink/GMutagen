namespace GMutagen.v8.Values;

public interface IValue<T> : IValue
{
    T Value { get; set; }
}
public interface IValue { }