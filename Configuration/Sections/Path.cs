namespace Configuration.Sections;

public readonly struct Path<TKey> : IEquatable<Path<TKey>> 
    where TKey : notnull
{
    public static readonly Path<TKey> Empty = new();
    
    private readonly TKey[] _segments;

    public IReadOnlyList<TKey> Segments => _segments ?? Array.Empty<TKey>();
    public int Depth => Segments.Count;
    public TKey LastSegment => Segments[^1];

    public Path() : this(Array.Empty<TKey>())
    {
    }

    public Path(IEnumerable<TKey> segments)
    {
        _segments = segments.ToArray();
    }

    public Path(params TKey[] segments)
    {
        _segments = segments;
    }

    public Path<TKey> Append(TKey key) => new(Segments.Concat([key]));

    public TKey GetSegmentAt(int index) => Segments[index];

    public bool StartsWithPath(Path<TKey> prefix)
    {
        if (Depth < prefix.Depth) return false;
        
        for (int i = 0; i < prefix.Depth; i++)
        {
            if (!AreSegmentsEqual(Segments[i], prefix.Segments[i]))
                return false;
        }
        
        return true;
    }

    public bool IsChildOf(Path<TKey> parentPath)
    {
        return Depth > parentPath.Depth && StartsWithPath(parentPath);
    }

    public bool Equals(Path<TKey> other)
    {
        if (Depth != other.Depth) return false;
        
        for (int i = 0; i < Depth; i++)
        {
            if (!AreSegmentsEqual(Segments[i], other.Segments[i]))
                return false;
        }
        
        return true;
    }

    public override bool Equals(object? obj) => 
        obj is Path<TKey> other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        
        foreach (var segment in Segments)
        {
            hash.Add(segment);
        }
        
        return hash.ToHashCode();
    }

    public override string ToString() => 
        string.Join(":", Segments);

    private static bool AreSegmentsEqual(TKey first, TKey second) =>
        EqualityComparer<TKey>.Default.Equals(first, second);
}