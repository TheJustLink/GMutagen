namespace GMutagen.v6;

public class Pair<T, T1>
{
    public T First { get; }
    public T1 Second { get; }

    public Pair(T first, T1 second)
    {
        First = first;
        Second = second;
    }
}