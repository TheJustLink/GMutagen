namespace GMutagen.v9.Values;

public interface IValue<T> : IValue
{
    T Value { get; set; }
}
public interface IValue { }