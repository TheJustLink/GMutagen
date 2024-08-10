namespace SnakeGame;

public class PositionRecorder : IPosition
{
    public readonly IPosition CurrentPosition;
    public readonly IPosition PreviousPosition;

    public Vector2 Value
    {
        get => CurrentPosition.Value;
        set
        {
            PreviousPosition.Value = CurrentPosition.Value;
            CurrentPosition.Value = value;
        }
    }

    public PositionRecorder(IPosition currentPosition, IPosition previousPosition)
    {
        CurrentPosition = currentPosition;
        PreviousPosition = previousPosition;
    }
}