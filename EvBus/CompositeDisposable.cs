namespace EventBus;

public class CompositeDisposable(IEnumerable<IDisposable> disposables) : IDisposable
{
    private readonly List<IDisposable> _disposables = disposables.ToList();

    public void Dispose()
    {
        foreach (var disposable in _disposables)
            disposable.Dispose();
    }
}