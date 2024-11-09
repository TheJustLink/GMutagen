namespace DataFlow;

public interface IFlow
{
    void Schedule();
    IFlow Append(IFlow flow);
    IFlow Append(IFlow flow, IFilter filter);
}