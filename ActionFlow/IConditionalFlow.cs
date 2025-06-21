namespace ActionFlow;

public interface IConditionalFlow
{
    
}

public interface IConditionalFlow<T> : IFlow<T>, IConditionalFlow
{
    Func<T, bool> Condition { get; }
}

