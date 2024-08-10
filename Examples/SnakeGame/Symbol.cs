namespace SnakeGame;

public class Symbol : ISymbol
{
    public char Value { get; set; }

    public Symbol()
    { 

    }

    public Symbol(char value)
    {
        Value = value;
    }
}