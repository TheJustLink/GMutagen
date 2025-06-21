using System.Text;
using EventBus;
using GMutagen.v9.IO.Interfaces;
using GMutagen.v9.Values.Interfaces;
using Identity.Interfaces;


namespace GMutagen.v9.Events;

using System;
using System.Collections.Generic;

public class Path
{
    private readonly StringBuilder _path;
    private readonly char _separator;

    public Path(char separator = '/')
    {
        _path = new StringBuilder();
        _separator = separator;
    }

    public Path(string initialPath, char separator = '/')
    {
        _path = new StringBuilder(initialPath);
        _separator = separator;
    }

    public void AppendSegment(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
            throw new ArgumentException("Segment cannot be null or whitespace.", nameof(segment));

        if (_path.Length > 0 && !_path.ToString().EndsWith(_separator))
            _path.Append(_separator);

        _path.Append(segment);
    }

    public string GetSegment(int index)
    {
        var segments = _path.ToString().Split(_separator, StringSplitOptions.RemoveEmptyEntries);
        if (index < 0 || index >= segments.Length)
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");

        return segments[index];
    }

    public IEnumerable<string> GetAllSegments()
    {
        return _path.ToString().Split(_separator, StringSplitOptions.RemoveEmptyEntries);
    }

    public override string ToString()
    {
        return _path.ToString();
    }

    public static implicit operator string(Path path)
    {
        return path.ToString();
    }

    public static implicit operator Path(string path)
    {
        return new Path(path);
    }
}


public class ValueEventsStorage(Dictionary<ISingleId, IValueEvents> translationMap) : IReadWrite<ISingleId, IValueEvents>
{
    private readonly Dictionary<ISingleId, IValueEvents> _translationMap = translationMap;

    public int Count { get; }

    public IValueEvents this[ISingleId id]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public void Write(ISingleId id, IValueEvents value)
    {
        throw new NotImplementedException();
    }

    public IValueEvents Read(ISingleId id)
    {
        throw new NotImplementedException();
    }

    public bool Contains(ISingleId id)
    {
        throw new NotImplementedException();
    }
}

public class ValueEvents<T> : ValueDecorator<T>, IValueEvents
{
    private readonly Event _refresh;
    private readonly Event _beforeChanged;
    private readonly Event _afterChanged;

    public ValueEvents(IValue<T> child) : this(child, new Event(), new Event(), new Event())
    {
    }

    private ValueEvents(IValue<T> child, Event refresh, Event beforeChanged, Event afterChanged) : base(child)
    {
        _refresh = refresh;
        _beforeChanged = beforeChanged;
        _afterChanged = afterChanged;
    }

    public Event Refresh => _refresh;
    public Event BeforeChanged => _beforeChanged;
    public Event AfterChanged => _afterChanged;

    public override T Value
    {
        get
        {
            return Child.Value;
        }
        set
        {
            BeforeChanged?.Fire();
            Child.Value = value;
            AfterChanged?.Fire();
        }
    }
}

public interface IValueEvents
{
    Event Refresh { get; }
    Event BeforeChanged { get; }
    Event AfterChanged { get; }
}

public abstract class ValueDecorator<T>(IValue<T> child) : IValue<T>
{
    public readonly IValue<T> Child = child;

    public abstract T Value { get; set; }
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class GenerateValueEventsAttribute : Attribute
{
}


