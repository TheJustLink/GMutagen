namespace Roguelike;
public class Position : IPosition
{
    public Vector2 Value { get; set; }

    public Position() { }

    public Position(Vector2 value)
    {
        Value = value;
    }
}