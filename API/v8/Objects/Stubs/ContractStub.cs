namespace GMutagen.v8.Objects.Stubs;

public abstract class ContractStub
{
    public abstract T Get<T>();
    public abstract bool TryGet<T>(out T contract);
}