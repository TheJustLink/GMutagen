namespace SnakeGame;

public class Direction : IDirection
{
    public Vector2 Value { get; set; }

    public Direction() 
    {

    }

    public Direction(Vector2 value)
    {
        Value = value;
    }
}