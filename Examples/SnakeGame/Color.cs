namespace SnakeGame;

public class Color : IColor
{
    public ConsoleColor Value { get; set; }

    public Color()
    {

    }

    public Color(ConsoleColor value)
    {
        Value = value;
    }
}