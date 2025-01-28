using System;
using System.Collections;
using System.Collections.Generic;

namespace GMutagen.v9.Resolving.Contexts.Key;

public class Keys : IEnumerable
{
    private static readonly int[] Types;

    private readonly Dictionary<KeyType, object?> _pairs;

    static Keys()
    {
        Types = (int[])Enum.GetValues(typeof(KeyType));
    }
    
    public Keys() : this(new Dictionary<KeyType, object?>(Types.Length))
    {
    }

    public Keys(int size) : this(new Dictionary<KeyType, object?>(size))
    {
    }

    private Keys(Dictionary<KeyType, object?> pairs)
    {
        _pairs = pairs;
    }

    public Dictionary<KeyType, object?>.ValueCollection Values => _pairs.Values; 
    public Dictionary<KeyType, object?>.KeyCollection KeyTypes => _pairs.Keys;

    public Keys Add(KeyType keyType, object? key)
    {
        _pairs.TryAdd(keyType, key);
        return this;
    }

    public IEnumerator GetEnumerator()
    {
        foreach (var type in Types)
        {
            if (_pairs.TryGetValue((KeyType)type, out var key))
                yield return key;
        }
    }

    public object? this[int i]
    {
        get
        {
            var index = 0;
            foreach (var type in Types)
            {
                if (_pairs.TryGetValue((KeyType)type, out var key) is false)
                    continue;
                
                if (index == i)
                    return key;

                index++;
            }

            return null;
        }
    }

    public object? this[KeyType keyType] => _pairs.GetValueOrDefault(keyType);

    public bool TryGetKey<T>(KeyType keyType, out T key)
    {
        if(_pairs.TryGetValue(keyType, out var optionObj))
        {
            key = (T)optionObj!;
            return true;
        }

        key = default;
        return false;
    }
}