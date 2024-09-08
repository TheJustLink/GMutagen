using System;

using GMutagen.v6.Objects.Template;
using GMutagen.v6.Values;

namespace Roguelike;

static class Program
{
    public static void Main(string[] args)
    {
        var memoryVector2Generator = DefaultGenerators.GetExternalValueGenerator<Vector2>();

        var defaultPositionTemplate = new ObjectTemplate()
            .Add<IPosition, DefaultPosition>()
            .AddFromGenerator<IValue<Vector2>>(memoryVector2Generator);

        var rabbitTemplate = new ObjectTemplate()
            .AddFromAnotherTemplate<IPosition>(defaultPositionTemplate);


        var rabbit = rabbitTemplate.Create();
        var position = rabbit.Get<IPosition>();

        Console.WriteLine(position.Value.ToString());
        position.Value = new Vector2(1, 1);
        Console.WriteLine(position.Value.ToString());
    }
}

class DefaultPosition : IPosition
{
    private readonly IValue<Vector2> _value;

    public DefaultPosition(IValue<Vector2> value)
    {
        _value = value;
    }

    public Vector2 Value
    {
        get => _value.Value;
        set => _value.Value = value;
    }
}