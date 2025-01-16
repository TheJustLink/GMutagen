namespace GMutagen.v9.Values.Interfaces;

public interface IValue<T> : IValue
{
    T Value { get; set; }
}
public interface IValue { }