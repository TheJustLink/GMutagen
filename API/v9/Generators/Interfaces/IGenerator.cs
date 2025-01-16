namespace GMutagen.v9.Generators.Interfaces;

public interface IGenerator<out T>
{
    T Generate();
}