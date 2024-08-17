using System;
using GMutagen.v5.Container;

namespace GMutagen.v5;

public interface IAddAsContext
{
    IAddContext Add<T>();
    IContainer FromConstructorGenerators(IGenerator<object>[] generators);
    IContainer FromGenerator(IGenerator<object> generator);
    IContainer FromInstance(object instance, bool registerAllContracts = true, bool shouldOverride = true);
}

public class AddAsContext : ContainerContext, IAddAsContext
{
    public Type Type { get; set; }

    public AddAsContext(ObjectTemplateContainer container) : base(container)
    {
    }

    public IAddContext Add<T>()
    {
        return Container.Add<T>();
    }

    public IContainer FromConstructorGenerators(IGenerator<object>[] generators)
    {
        var generatorBindingsOption = new GeneratorConstructorBindings(Type, generators);
        Container[KeyType].Set(OptionType.ResolveFrom, generatorBindingsOption);
        return Container;
    }

    public IContainer FromGenerator(IGenerator<object> generator)
    {
        var generatorBindingsOption = new GeneratorBindingsOption(generator);
        Container[KeyType].Set(OptionType.ResolveFrom, generatorBindingsOption);
        return Container;
    }

    public IContainer FromInstance(object instance, bool registerAllContracts = true, bool shouldOverride = true)
    {
        var instanceType = instance.GetType();

        if (instance.GetType() != Type)
            throw new Exception("Types are not the same");

        if (!TypeCanBeConvertedTo(instanceType, KeyType))
            throw new Exception("Is not instance of key type");

        var instanceBindingsOption = new InstanceBindingsOption(instance);

        Container[KeyType].Set(OptionType.ResolveFrom, instanceBindingsOption);
        
        if (!registerAllContracts)
            return Container;

        RegisterInterfaces(instanceType, instanceBindingsOption, shouldOverride);
        RegisterBaseTypes(instanceType, instanceBindingsOption, shouldOverride);

        return Container;
    }
    
    private void RegisterInterfaces(Type instanceType, InstanceBindingsOption instanceBindingsOption, bool shouldOverride)
    {
        foreach (var interfaceType in instanceType.GetInterfaces())
        {
            if (!Container.Dictionary.TryGetValue(interfaceType, out var bindings))
            {
                Container.Add(interfaceType);
                Container[interfaceType].Set(OptionType.ResolveFrom, instanceBindingsOption);
            }
            else
            {
                if (shouldOverride)
                    bindings.Set(OptionType.ResolveFrom, instanceBindingsOption);
            }
        }
    }

    private void RegisterBaseTypes(Type instanceType, InstanceBindingsOption instanceBindingsOption, bool shouldOverride)
    {
        var baseType = instanceType.BaseType;
        while (baseType != typeof(object))
        {
            if (!Container.Dictionary.TryGetValue(baseType, out var bindings))
            {
                Container.Add(baseType);
                Container[baseType].Set(OptionType.ResolveFrom, instanceBindingsOption);
            }
            else
            {
                if (shouldOverride)
                    bindings.Set(OptionType.ResolveFrom, instanceBindingsOption);
            }

            baseType = instanceType.BaseType;
        }
    }

    private bool TypeCanBeConvertedTo(Type instanceType, Type targetType)
    {
        return instanceType.IsSubclassOf(targetType) || targetType.IsAssignableFrom(instanceType);
    }
}