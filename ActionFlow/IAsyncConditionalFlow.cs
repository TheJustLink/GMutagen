namespace ActionFlow;

public interface IAsyncConditionalFlow<T> : IAsyncFlow<T>, IConditionalFlow
{
    Func<T, bool> Condition { get; }
}