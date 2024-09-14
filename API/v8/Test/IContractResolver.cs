namespace GMutagen.v8.Test;

public interface IContractResolver
{
    object Resolve<TId>(ContractDescriptor contract, TId id) where TId : notnull;
}