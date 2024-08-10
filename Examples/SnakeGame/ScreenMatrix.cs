namespace SnakeGame;

public class ScreenMatrix
{ 
    public static void Draw(Vector2 position, char displaySymbol, ConsoleColor color)
    {
        var oldColor = Console.ForegroundColor;

        Console.ForegroundColor = color;
        Console.SetCursorPosition(position.X, position.Y);
        Console.Write(displaySymbol);
    }
}