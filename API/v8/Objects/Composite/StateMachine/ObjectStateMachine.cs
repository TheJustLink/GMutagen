using GMutagen.v8.Objects.Interface;

namespace GMutagen.v8.Objects.Composite.StateMachine;

public class ObjectStateMachine : IObject
{
    private readonly Object[] _states;
    public int Index { get; set; }

    public ObjectStateMachine(Object[] states, int index)
    {
        _states = states;
        Index = index;
    }

    public T Get<T>()
    {
        return _states[Index].Get<T>();
    }

    public bool TryGet<T>(out T contract)
    {
        return _states[Index].TryGet(out contract);
    }
}