namespace GMutagen.v6;

public abstract class ContractStub
{
    public abstract T Get<T>();
    public abstract bool TryGet<T>(out T contract);
}