using System;
using System.Collections;
using System.Collections.Generic;

namespace GMutagen.v9.Contracts.Resolving.Contexts.Option;

public class Options
{
    private static readonly int[] Types;

    private readonly Dictionary<OptionType, object?> _pairs;

    static Options()
    {
        Types = (int[])Enum.GetValues(typeof(OptionType));
    }

    public Options() : this(new Dictionary<OptionType, object?>(Types.Length))
    {
    }
    
    public Options(int size) : this(new Dictionary<OptionType, object?>(size))
    {
    }

    private Options(Dictionary<OptionType, object?> pairs)
    {
        _pairs = pairs;
    }

    public Options Add(OptionType id, object? key)
    {
        _pairs.Add(id, key);
        return this;
    }

    public IEnumerator GetEnumerator()
    {
        foreach (var type in Types)
        {
            if (_pairs.TryGetValue((OptionType)type, out var option))
                yield return option;
        }
    }

    public object? this[int i]
    {
        get
        {
            var index = 0;
            foreach (var type in Types)
            {
                if (_pairs.TryGetValue((OptionType)type, out var key))
                {
                    if (index == i)
                        return key;

                    index++;
                }
            }

            return null;
        }
    }

    public object? this[OptionType optionType]
    {
        get
        {
            if (_pairs.TryGetValue(optionType, out var option))
                return option;

            return null;
        }
    }

    public bool TryGetOption<T>(OptionType optionType, out T option)
    {
        if(_pairs.TryGetValue(optionType, out var optionObj))
        {
            option = (T)optionObj!;
            return true;
        }

        option = default;
        return false;
    }
}