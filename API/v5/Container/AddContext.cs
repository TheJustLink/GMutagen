using System;

namespace GMutagen.v5;

public interface IAddContext
{
    IAddAsContext As<T>();
    IContainer FromInstance(object instance, bool registerAllContracts = false, bool shouldOverride = false);
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
        Container[KeyType] = type;
        _addAsContext.KeyType = KeyType;
        _addAsContext.Type = type;
        return _addAsContext;
    }

    public IContainer FromInstance(object instance, bool registerAllContracts = true, bool shouldOverride = true)
    {
        var instanceType = instance.GetType();

        if (!TypeCanBeConvertedTo(instanceType, KeyType))
            throw new Exception("Is not instance of key type");

        if (!registerAllContracts)
        {
            Container[KeyType] = instance;
            return Container;
        }

        foreach (var interfaceType in instanceType.GetInterfaces())
        {
            if(!Container.Dictionary.TryAdd(interfaceType, instance) && shouldOverride);
                Container[interfaceType] = instance;
        }

        var baseType = instanceType.BaseType;
        while (baseType != typeof(object))
        {
            if (!Container.Dictionary.TryAdd(baseType, instance) && shouldOverride)
                Container[baseType] = instance;

            baseType = instanceType.BaseType;
        }

        return Container;
    }

    private bool TypeCanBeConvertedTo(Type instanceType, Type targetType)
    {
        return instanceType.IsSubclassOf(targetType) || targetType.IsAssignableFrom(instanceType);
    }
}