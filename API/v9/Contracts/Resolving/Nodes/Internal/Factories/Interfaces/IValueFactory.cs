namespace GMutagen.v9.Contracts.Resolving.Nodes.Internal.Factories.Interfaces;

public interface IValueFactory<TValueId>
{
    object Create(TValueId id, object storage);
}