namespace GMutagen.v9.Generators;

public interface IGenerator<out T>
{
    T Generate();
}