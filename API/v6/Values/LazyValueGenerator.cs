using GMutagen.v6.Objects;

namespace GMutagen.v6.Values;

public class LazyValueGenerator<T> : IGenerator<LazyValue<T>, IGenerator<IValue<T>>>
{
    public LazyValue<T> Generate(IGenerator<IValue<T>> generator) => new(generator);

    public LazyValue<T> this[IGenerator<IValue<T>> id] => Read(id);
    public LazyValue<T> Read(IGenerator<IValue<T>> id) => Generate(id);
}