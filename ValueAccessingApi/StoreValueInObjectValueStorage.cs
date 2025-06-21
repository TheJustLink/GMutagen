using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Contexts.Option;
using GMutagen.v9.Resolving.Nodes.Decorator;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using GMutagen.v9.Values.Interfaces;
using Identity.Realizations;

namespace VariableAccessingApi;

public class StoreValueInObjectValueStorage(IResolverNode resolver) : RecursiveResolverNode(resolver)
{
    public override bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IValue)))
            return Resolver.Resolve(context);

        var result = Resolver.Resolve(context);
        if (result)
            Store(context);

        return result;
    }

    private void Store(Context context)
    {
        var objectValueStorageContext = new Context(typeof(ObjectValueStorage));
        var result = Resolver.Resolve(objectValueStorageContext);
        
        if(!result)
            return;

        var objectValueStorage = (ObjectValueStorage)objectValueStorageContext.Instance!;
        
        var nameResult = context.TryGetOptionFallback(out string name, OptionType.SemanticName);
        if(!nameResult)
            return;

        if(context.Instance == null)
            return;
        
        Id<string> id = name;
        objectValueStorage.Write(id, (IValue)context.Instance!);
    }
}