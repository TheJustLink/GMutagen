using System;

namespace GMutagen.v5;

public interface IAddAsContext
{
    IContainer FromInstance(object instance);
}

public class AddAsContext : ContainerContext, IAddAsContext
{
    public Type Type { get; set; }

    public AddAsContext(ObjectTemplateContainer container) : base(container)
    {
    }

    public IContainer FromInstance(object instance)
    {
        var instanceType = instance.GetType();

        if (instance.GetType() != Type)
            throw new Exception("Types are not the same");
        
        if (!TypeCanBeConvertedTo(instanceType, KeyType))
            throw new Exception("Is not instance of key type");

        Container[KeyType] = instance;
        return Container;
    }
    
    private bool TypeCanBeConvertedTo(Type instanceType, Type targetType)
    {
        return !instanceType.IsSubclassOf(targetType) && !targetType.IsAssignableFrom(instanceType);
    }
}