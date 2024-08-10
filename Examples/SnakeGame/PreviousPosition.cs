namespace SnakeGame;
public class PreviousPosition : IPreviousPosition
{
    public Vector2 Value { get; set; }

    public PreviousPosition() { }
    public PreviousPosition(Vector2 value)
    {
        Value = value;
    }
}