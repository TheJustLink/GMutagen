namespace Configuration.Common;

public static class DictionaryExtensions
{
    public static Dictionary<TValue, TKey> Reverse<TKey, TValue>(this Dictionary<TKey, TValue> dict) where TValue : notnull where TKey : notnull
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