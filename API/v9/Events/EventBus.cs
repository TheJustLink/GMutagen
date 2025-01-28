using System.Text;
using GMutagen.IO;
using GMutagen.Values;

namespace GMutagen.v9.Events;

using System;
using System.Collections.Generic;

public class EventBus
{
    private readonly Dictionary<string, Event> _subscribers = new();

    public void Subscribe(EventDescriptor descriptor, Action handler)
    {
        if (descriptor == null)
            throw new ArgumentNullException(nameof(descriptor));

        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        string eventPath = descriptor.Id;

        if (!_subscribers.ContainsKey(eventPath))
        {
            _subscribers[eventPath].Subscribe(handler);
        }
    }

    public void Unsubscribe(EventDescriptor descriptor, Action handler)
    {
        if (descriptor == null)
            throw new ArgumentNullException(nameof(descriptor));

        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        string eventPath = descriptor.Id;

        if (_subscribers.ContainsKey(eventPath))
        {
            _subscribers[eventPath].Unsubscribe(handler);
        }
    }

    public void PublishExact(EventDescriptor descriptor)
    {
        if (descriptor == null)
            throw new ArgumentNullException(nameof(descriptor));

        string eventPath = descriptor.Id;

        if (_subscribers.ContainsKey(eventPath))
        {
            _subscribers[eventPath]?.Fire();
        }
    }

    public void PublishToAll(EventDescriptor ev)
    {
        if (ev == null)
            throw new ArgumentNullException(nameof(ev));

        string eventPath = ev.Id;

        PublishExact(ev);

        var segments = eventPath.Split('/');
        for (int i = segments.Length - 1; i > 0; i--)
        {
            var parentPath = string.Join("/", segments, 0, i);
            var parentEvent = new EventDescriptor(parentPath);
            PublishExact(parentEvent);
        }
    }
}

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

public class EventDescriptor(string id)
{
    public Path Id { get; set; } = id;
}

public class Event(Action listeners = null) : IEvent
{
    public IEvent Fire()
    {
        listeners?.Invoke();
        return this;
    }

    public IEvent Subscribe(Action action)
    {
        listeners += action;
        return this;
    }

    public IEvent Unsubscribe(Action action)
    {
        listeners -= action;
        return this;
    }
}

public interface IEvent
{
    IEvent Fire();
    IEvent Subscribe(Action action);
    IEvent Unsubscribe(Action action);
}

public class ValueEventsStorage<TId>(IRead<IValue<TId>, IValueEvents> reader, IWrite<IValue<TId>, IValueEvents> writer)
    : ReadWrite<IValue<TId>, IValueEvents>(reader, writer)
    where TId : notnull
{
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

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class GenerateValueEventsAttribute : Attribute
{
}

