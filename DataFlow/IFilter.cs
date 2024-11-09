namespace DataFlow;

public interface IFilter : IEquatable<IFilter>
{
    bool Execute();
}