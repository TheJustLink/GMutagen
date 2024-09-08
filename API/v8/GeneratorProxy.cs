using GMutagen.v8.Objects;

namespace GMutagen.v8;

public class GeneratorOverGenerator<T> : GeneratorProxy<T, T>
{
    public GeneratorOverGenerator(IGenerator<T> sourceGenerator, IGenerator<T, IGenerator<T>> proxyGenerator)
        : base(sourceGenerator, proxyGenerator) { }
}

public class GeneratorProxy<TIn, TOut> : IGenerator<TOut>
{
    private readonly IGenerator<TIn> _sourceGenerator;
    private readonly IGenerator<TOut, IGenerator<TIn>> _proxyGenerator;

    public GeneratorProxy(IGenerator<TIn> sourceGenerator, IGenerator<TOut, IGenerator<TIn>> proxyGenerator)
    {
        _sourceGenerator = sourceGenerator;
        _proxyGenerator = proxyGenerator;
    }

    public TOut Generate() => _proxyGenerator.Generate(_sourceGenerator);
}
