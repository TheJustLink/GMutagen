namespace GMutagen.v9.Contracts.Resolving.Nodes.MetaData;

public class ValueMetaData<TId>
{
    public TId Id { get; private set; }
    public ValueMetaData(TId id)
    {
        Id = id;
    }
}