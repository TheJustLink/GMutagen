namespace GMutagen.v6;

public interface IGenerator<out TOut, in TIn> : IRead<TIn, TOut>
{
    TOut Generate(TIn input);
}