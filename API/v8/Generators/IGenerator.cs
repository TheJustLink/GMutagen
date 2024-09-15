namespace GMutagen.v8.Generators;

public interface IGenerator<out T>
{
    T Generate();
}