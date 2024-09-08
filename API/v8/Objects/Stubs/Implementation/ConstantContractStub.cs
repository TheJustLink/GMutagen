namespace GMutagen.v8.Objects.Stubs.Implementation;

public class ConstantContractStub : ContractStub
{
    private readonly object _contract;

    public ConstantContractStub(object contract)
    {
        _contract = contract;
    }

    public override T Get<T>()
    {
        return (T)_contract;
    }

    public override bool TryGet<T>(out T contract)
    {
        contract = (T)_contract;
        return true;
    }
}