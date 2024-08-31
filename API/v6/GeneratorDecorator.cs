using GMutagen.v6.Objects;

namespace GMutagen.v6;

public class GeneratorDecorator<T> : GeneratorDecorator<T, T>
{
    public GeneratorDecorator(IGenerator<T> sourceGenerator, IGenerator<T, IGenerator<T>> proxyGenerator)
        : base(sourceGenerator, proxyGenerator) { }
}