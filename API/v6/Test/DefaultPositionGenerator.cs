using System.Numerics;
using GMutagen.v6.IO;
using GMutagen.v6.Values;

namespace GMutagen.v6.Test.Test;

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
}