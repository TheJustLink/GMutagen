namespace GMutagen.v6;

public class ConstantFromObjectContractStub : ContractStub
{
    private readonly Object _obj;

    public ConstantFromObjectContractStub(Object obj)
    {
        _obj = obj;
    }

    public override T Get<T>()
    {
        return _obj.Get<T>();
    }

    public override bool TryGet<T>(out T contract)
    {
        return _obj.TryGet(out contract);
    }
}