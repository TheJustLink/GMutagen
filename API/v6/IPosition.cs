using System.Numerics;

namespace GMutagen.v6;

public interface IPosition
{
    Vector2 GetCurrentPositionWithOffset(Vector2 offset);
    Vector2 GetPreviousPositionWithOffset(Vector2 offset);
}