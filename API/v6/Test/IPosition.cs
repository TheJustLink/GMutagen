using System.Numerics;

namespace GMutagen.v6.Test;

public interface IPosition
{
    Vector2 GetCurrentPositionWithOffset(Vector2 offset);
    Vector2 GetPreviousPositionWithOffset(Vector2 offset);
}