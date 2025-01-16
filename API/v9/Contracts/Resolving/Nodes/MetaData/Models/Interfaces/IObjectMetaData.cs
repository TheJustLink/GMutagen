using GMutagen.v9.Contracts.Resolving.Contexts;

namespace GMutagen.v9.Contracts.Resolving.Nodes.MetaData.Models.Interfaces;

public interface IObjectMetaData
{
    void Store(Context context, object contractMetaDataObj);
}