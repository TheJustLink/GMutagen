using System.Numerics;

namespace GMutagen.v6;

public class DefaultPosition : IPosition
{
    private readonly IValue<Vector2> _currentPosition;
    private readonly IValue<Vector2> _previousPosition;
    

    public DefaultPosition(IValue<Vector2> currentPosition, IValue<Vector2> previousPosition)
    {
        _currentPosition = currentPosition;
        _previousPosition = previousPosition;
    }

    public Vector2 GetCurrentPositionWithOffset(Vector2 offset)
    {
        return _currentPosition.Value + offset;
    }
    public Vector2 GetPreviousPositionWithOffset(Vector2 offset)
    {
        return _previousPosition.Value + offset;
    }
}