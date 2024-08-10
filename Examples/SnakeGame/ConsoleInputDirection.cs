namespace SnakeGame;

public class ConsoleInputDirection : IDirection
{
    public Vector2 Value
    {
        get => GetDirection();
        set => throw new NotSupportedException();
    }

    private Vector2 GetDirection()
    {
        var inputKey = Console.ReadKey(true);

        var xOffset = 0;
        var yOffset = 0;

        if (inputKey.Key == ConsoleKey.W)
            yOffset--;
        if (inputKey.Key == ConsoleKey.S)
            yOffset++;

        if (inputKey.Key == ConsoleKey.A)
            xOffset--;
        if (inputKey.Key == ConsoleKey.D)
            xOffset++;

        return new Vector2(xOffset, yOffset);
    }
}