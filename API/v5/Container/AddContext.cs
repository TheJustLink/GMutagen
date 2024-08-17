using GMutagen.v5.Container;

namespace GMutagen.v5;

public interface IAddContext
{
    IAddAsContext As<T>();
    IContainer FromInstance(object instance, bool registerAllContracts = true, bool shouldOverride = true);
}

public class AddContext : ContainerContext, IAddContext
{
    private readonly AddAsContext _addAsContext;

    public AddContext(ObjectTemplateContainer container) : base(container)
    {
        _addAsContext = new AddAsContext(container);
    }

    public IAddAsContext As<T>()
    {
        var type = typeof(T);
        Container[KeyType].Set(OptionType.ResolveFrom, new ReflectionBindingsOption(type));
        _addAsContext.KeyType = KeyType;
        _addAsContext.Type = type;
        return _addAsContext;
    }

    public IContainer FromInstance(object instance, bool registerAllContracts = true, bool shouldOverride = true)
    {
        _addAsContext.KeyType = KeyType;
        _addAsContext.Type = instance.GetType(); 
        return _addAsContext.FromInstance(instance, registerAllContracts, shouldOverride);
    }
    
}