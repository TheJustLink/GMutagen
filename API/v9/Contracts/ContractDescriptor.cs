using System;

namespace GMutagen.v9.Contracts;

public class ContractDescriptor
{
    public readonly Type Type;
    public readonly Type ImplementationType;
    public readonly object? Implementation;

    public ContractDescriptor(Type type, object? implementation = null) : this(type, type, implementation)
    {
    }
    
    public ContractDescriptor(Type type, Type implementationType, object? implementation = null)
    {
        if (Type != null && !Type.IsAssignableFrom(typeof(IContract)))
            throw new Exception();
        
        Type = type;
        ImplementationType = implementationType;
        Implementation = implementation;
    }

    public override int GetHashCode() => Type.GetHashCode();

    public static ContractDescriptor Create<TContract>() where TContract : IContract => new(typeof(TContract));
    public static ContractDescriptor Create<TContract, TImplementation>() where TContract : IContract =>
        new(typeof(TContract), typeof(TImplementation));
    public static ContractDescriptor Create<TContract>(object? implementation) where TContract : IContract =>
        new(typeof(TContract), implementation?.GetType(), implementation);
}