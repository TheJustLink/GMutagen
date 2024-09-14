using System.Numerics;

using GMutagen.v8.IO;
using GMutagen.v8.Objects;
using GMutagen.v8.Values;

namespace GMutagen.v8.Test;

public class DefaultPositionGenerator : IGenerator<IPosition, ITypeRead<IGenerator<object>>>
{
    public IPosition Generate(ITypeRead<IGenerator<object>> input)
    {
        var currentPosition = input.Read<IGenerator<IValue<Vector2>>>().Generate();
        var previousPosition = new ValueWithHistory<Vector2>(currentPosition);

        return new DefaultPosition(currentPosition, previousPosition);
    }

    public IPosition this[ITypeRead<IGenerator<object>> id] => Read(id);
    public IPosition Read(ITypeRead<IGenerator<object>> id) => Generate(id);
    public bool Contains(ITypeRead<IGenerator<object>> id) => false; // TODO ???
}