namespace DataFlow;

public class BaseFlow : IFlow, IDisposable
{
    private readonly HashSet<IFlow> _flows;
    private readonly Dictionary<IFilter, IFlow> _filteredFlows;

    public BaseFlow(Action action) : this(new HashSet<IFlow>(), new Dictionary<IFilter, IFlow>(new FlowComparer()))
    {
        _flows.Add(new ActionFlow(action));
    }
    
    public BaseFlow() : this(new HashSet<IFlow>(), new Dictionary<IFilter, IFlow>(new FlowComparer()))
    {
    }

    public BaseFlow(HashSet<IFlow> flows, Dictionary<IFilter, IFlow> filteredFlows)
    {
        _flows = flows;
        _filteredFlows = filteredFlows;
    }

    public virtual void Schedule()
    {
        foreach (var flow in _flows)
            flow.Schedule();

        foreach (var filteredFlow in _filteredFlows)
        {
            var filter = filteredFlow.Key;
            var flow = filteredFlow.Value;
                
            if(filter.Execute())
                flow.Schedule();
        }
    }

    public virtual IFlow Append(IFlow flow)
    {
        _flows.Add(flow);
        return this;
    }

    public virtual IFlow Append(IFlow flow, IFilter filter)
    {
        _filteredFlows[filter] = flow;
        return this;
    }

    public void Dispose()
    {
        _flows.Clear();
        _filteredFlows.Clear();
    }
}

public class ActionFlow : BaseFlow
{
    private readonly Action _action;
    public ActionFlow(Action action)
    {
        _action = action;
    }

    public override void Schedule()
    {
        _action();
        base.Schedule();
    }

    public static implicit operator ActionFlow(Action action)
    {
        return new ActionFlow(action);
    }
}