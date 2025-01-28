using GMutagen.v9.Resolving.Contexts;

namespace GMutagen.v9.Resolving.Nodes.MetaData.Models.Interfaces;

public interface IObjectMetaData
{
    void Store(Context context, object contractMetaDataObj);
}