using GMutagen.v5;

namespace GMutagen.v6;

public class IntValue : IValue<int>
{
    private int _value;

    public int Value
    {
        get => _value;
        set => _value = value;
    }
}