using System.Numerics;
using GMutagen.v6.IO;
using GMutagen.v6.Values;
using ObjectTemplate = GMutagen.v6.Objects.Template.ObjectTemplate;


var shipTemplate = new ObjectTemplate()
    .Add<IHealth, DefaultHealth>()
    .Add<IPosition, DefaultPosition>()
    .Add<>();

public class ExternalProjectValue<T> : ExternalValue<int, T>
{
    public ExternalProjectValue(int id, IReadWrite<int, T> readWrite) : base(id, readWrite)
    {
    }

    public ExternalProjectValue(int id, IRead<int, T> reader, IWrite<int, T> writer) : base(id, reader, writer)
    {
    }
}

public class DefaultHealth : IHealth
{
    private int _value;

    public int Value
    {
        get => _value;
        set => _value = value;
    }

    public DefaultHealth(int value)
    {
        _value = value;
    }
}

public interface IHealth : GMutagen.v6.Values.IValue<int>
{
    
}

public interface IPosition : IValue<Vector2>
{
}

public class DefaultPosition : IPosition
{
    private Vector2 _value;

    public Vector2 Value
    {
        get => _value;
        set => _value = value;
    }
}