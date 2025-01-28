using GMutagen.v9.Contracts.Interfaces;
using GMutagen.v9.IO.Factories;
using GMutagen.v9.IO.Interfaces;
using GMutagen.v9.Values.Interfaces;

namespace GMutagen.v9.Resolving.Nodes.Internal.Common;

public class ObjectValueStorage : ReadWrite<string, IValue>, IContract
{
    public ObjectValueStorage(IRead<string, IValue> reader, IWrite<string, IValue> writer) : base(reader, writer)
    {
    }
}