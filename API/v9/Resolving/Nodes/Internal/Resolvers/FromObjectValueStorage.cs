using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Contexts.Option;
using GMutagen.v9.Resolving.Nodes.Decorator;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using GMutagen.v9.Resolving.Nodes.Internal.Common;
using GMutagen.v9.Values.Interfaces;

namespace GMutagen.v9.Resolving.Nodes.Internal.Resolvers;

public class FromObjectValueStorage(IResolverNode resolver) : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IValue)))
            return false;

        var objectValueStorageContext = new Context(typeof(ObjectValueStorage));
        var result = Resolver.Resolve(objectValueStorageContext);

        if (!result)
            return false;

        var objectValueStorage = (ObjectValueStorage)objectValueStorageContext.Instance!;

        var nameResult = context.TryGetOptionFallback(out string name, OptionType.SemanticName, OptionType.Name);
        if (!nameResult)
            return false;

        if (!objectValueStorage.Contains(name))
            return false;

        var value = objectValueStorage.Read(name);
        context.Instance = value;
        return true;
    }
}