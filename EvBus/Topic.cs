namespace EventBus;

public class Topic
{
    public string Pattern { get; }
    private readonly string[] _parts;

    public Topic(string pattern)
    {
        Pattern = pattern;
        _parts = pattern.Split(Constants.DELIMITER_CHAR);
    }

    public bool Matches(Topic other)
    {
        int p = 0, t = 0;
        while (p < _parts.Length && t < other._parts.Length)
        {
            if (_parts[p] == Constants.MANY_STRING)
                return p == _parts.Length - 1;

            if (_parts[p] == Constants.ANY_ONE_STRING || _parts[p] == other._parts[t])
            {
                p++;
                t++;
            }
            else return false;
        }

        return p == _parts.Length && t == other._parts.Length;
    }

    public static Topic Parse(string pattern) => new(pattern);

    public static TopicBuilder Create() => new();

    public override string ToString() => Pattern;

    public static implicit operator Topic(string pattern)
    {
        return Topic.Parse(pattern);
    }
}