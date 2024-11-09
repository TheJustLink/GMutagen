namespace DataFlow;

internal class TestFilter : IFilter
{
    public bool Equals(IFilter other)
    {
        return false;
    }

    public bool Execute()
    {
        return false;
    }
}