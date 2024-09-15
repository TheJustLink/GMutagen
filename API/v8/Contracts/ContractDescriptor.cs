using System;

namespace GMutagen.v8.Contracts;

public class ContractDescriptor
{
    public readonly Type Type;
    public readonly Type? ImplementationType;
    public readonly object? Implementation;

    public ContractDescriptor(Type type, Type? implementationType = null, object? implementation = null)
    {
        Type = type;
        ImplementationType = implementationType;
        Implementation = implementation;
    }

    public override int GetHashCode() => Type.GetHashCode();

    public static ContractDescriptor Create<TContract>() => new(typeof(TContract));
    public static ContractDescriptor Create<TContract, TImplementation>() =>
        new(typeof(TContract), typeof(TImplementation));
    public static ContractDescriptor Create<TContract>(object implementation) =>
        new(typeof(TContract), implementation.GetType(), implementation);
}