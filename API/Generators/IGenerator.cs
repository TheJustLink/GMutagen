namespace GMutagen.Generators;

public interface IGenerator<out T>
{
    T Generate();
}