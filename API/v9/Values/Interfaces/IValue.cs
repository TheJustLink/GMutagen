using GMutagen.v9.Events;

namespace GMutagen.v9.Values.Interfaces;

public interface IValueWithEvents<T> : IValue<T>, IValueWithEvents
{
    
}

public interface IValueWithEvents : IValue
{
    IValueEvents Events { get; }
}

public interface IValue<T> : IValue
{
    T Value { get; set; }
}
public interface IValue { }