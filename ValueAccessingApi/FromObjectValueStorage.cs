using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Contexts.Option;
using GMutagen.v9.Resolving.Nodes.Decorator;
using GMutagen.v9.Resolving.Nodes.Interfaces;
using GMutagen.v9.Values.Interfaces;
using Identity.Realizations;

namespace VariableAccessingApi;

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

        var semanticToValueTranslation = (ObjectValueStorage)objectValueStorageContext.Instance!;

        var nameResult = context.TryGetOptionFallback(out string name, OptionType.SemanticName);
        if (!nameResult)
            return false;

        Id<string> id = name;
        if (!semanticToValueTranslation.Contains(id))
            return false;

        var value = semanticToValueTranslation.Read(id);
        context.Instance = value;
        return true;
    }
}