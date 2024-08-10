namespace SnakeGame;

class ChildPosition : IPosition
{
    private readonly IPreviousPosition _target;

    public ChildPosition(IPreviousPosition target) => _target = target;

    public Vector2 Value
    {
        get => _target.Value;
        set => throw new NotSupportedException();
    }
}