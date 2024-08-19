using Object = GMutagen.v1.Object;

namespace SnakeGame;

public class ConsoleView : IView
{
    private readonly ScreenMatrix _matrix;

    private readonly Object _displayObject;
    private readonly char _displaySymbol;

    public void Display()
    {
        var color = _displayObject.Get<Color>().Value;
        var position = _displayObject.Get<Position>().Value;
        
        //_matrix.Set(position, _displaySymbol, color);
    }
}