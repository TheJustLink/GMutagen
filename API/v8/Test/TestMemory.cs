using System.Collections.Generic;
using GMutagen.v8.Generators;
using GMutagen.v8.IO;

namespace GMutagen.v8.Test;

public class TestMemory<TValue> : IReadWrite<int, TValue>, IGenerator<int>
{
    private readonly List<TValue> _memory = new();
    private readonly LinkedList<int> _nextFree = new();

    public int Count => _memory.Count;
    public TValue this[int id]
    {
        get => Read(id);
        set => Write(id, value);
    }

    public TValue Read(int id)
    {
        return _memory[id];
    }

    public void Write(int id, TValue value)
    {
        if (id < _memory.Count)
            _memory[id] = value;
        else
            _memory.Add(value);
    }

    public void Release(int id)
    {
        _nextFree.AddFirst(id);
    }

    public int Generate()
    {
        if (_nextFree.First == null)
            return _memory.Count;

        var value = _nextFree.First.Value;
        _nextFree.RemoveFirst();
        return value;
    }

    public bool Contains(int id)
    {
        return id < _memory.Count;
    }
}