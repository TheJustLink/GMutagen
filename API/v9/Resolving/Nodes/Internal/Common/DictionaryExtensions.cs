using System;
using System.Collections.Generic;

namespace GMutagen.v9.Resolving.Nodes.Internal.Common;

public static class DictionaryExtensions
{
    public static Dictionary<TValue, TKey> Reverse<TKey, TValue>(this Dictionary<TKey, TValue> dict)
    {
        var reversedDict = new Dictionary<TValue, TKey>();
        
        foreach (var kvp in dict)
        {
            if (!reversedDict.ContainsKey(kvp.Value))
            {
                reversedDict.Add(kvp.Value, kvp.Key);
            }
            else
            {
                throw new ArgumentException("Duplicate values are not allowed.");
            }
        }
        
        return reversedDict;
    }
}