namespace DataFlow;

public static class FlowExtensions
{
    public static IFlow SubscribeOn(this IFlow flow, IFlow target)
    {
        target.Append(flow);
        return flow;
    }
}