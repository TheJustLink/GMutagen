using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GMutagen.v7;

public static class ObjectExtensions
{
    public static dynamic Execute<TContract>(this IObject @object, params dynamic[] parameters)
    {
        var contract = @object.Get<TContract>();

        var methods = typeof(TContract).GetRuntimeMethods();
        foreach (var method in methods)
        {
            var methodParameters = method.GetParameters();
            if (methodParameters.Length != parameters.Length) continue;

            var isIdenticalParameters = true;
            for (var i = 0; i < methodParameters.Length; i++)
            {
                var parameterType = methodParameters[i].ParameterType;
                if (parameterType == parameters[i].GetType()) continue;

                isIdenticalParameters = false;
            }

            if (isIdenticalParameters)
                return method.Invoke(contract, parameters)!;
        }

        throw new ArgumentException();
    }
}
public interface IObject : IContractContainer
{
    int Id { get; }

    TContract Get<TContract>();
    void Set<TContract>() where TContract : new();
    void Set<TContract, TImplementation>() where TImplementation : TContract, new();
    void Set<TContract>(dynamic implementation);
}

public class Object : IObject
{
    public int Id { get; }

    private readonly IContractContainer _contracts;

    public Object(int id, IContractContainer contracts)
    {
        Id = id;
        _contracts = contracts;
    }

    public TContract Get<TContract>()
    {
        return _contracts.Get<TContract>(Id);
    }
    public void Set<TContract>() where TContract : new()
    {
        Set<TContract>(new TContract());
    }
    public void Set<TContract, TImplementation>() where TImplementation : TContract, new()
    {
        Set<TContract>(new TImplementation());
    }
    public void Set<TContract>(dynamic implementation)
    {
        _contracts.Set<TContract>(Id, implementation);
    }

    void IContractContainer.Set<TContract>(int holderId, dynamic implementation)
    {
        _contracts.Set<TContract>(holderId, implementation);
    }
    TContract IContractContainer.Get<TContract>(int holderId)
    {
        return _contracts.Get<TContract>(holderId);
    }
}
public interface IContractContainer
{
    void Set<TContract>(int holderId, dynamic implementation);
    TContract Get<TContract>(int holderId);
}
public class Scene : IContractContainer
{
    private readonly Dictionary<int, Dictionary<Type, dynamic>> _objectsMap = new();
    private int _incrementalId;

    public Object AddObject()
    {
        var id = _incrementalId++;
        var contracts = new Dictionary<Type, dynamic>();

        _objectsMap[id] = contracts;
        return new Object(id, this);
    }
    public void RemoveObject(Object @object)
    {
        _objectsMap.Remove(@object.Id);
    }

    TContract IContractContainer.Get<TContract>(int holderId)
    {
        const int MaxHolderDepth = 10;

        for (var i = 0; i < MaxHolderDepth; i++)
        {
            var contracts = _objectsMap[holderId];
            if (contracts.TryGetValue(typeof(TContract), out var contract) is false)
                contract = contracts.Values.First(c => c is TContract);

            switch (contract)
            {
                case TContract implementation: return implementation;
                case int contractHolderId: holderId = contractHolderId; continue;
                case IObject contractHolderObject: return contractHolderObject.Get<TContract>();
            }

            break;
        }

        throw new InvalidOperationException();
    }
    void IContractContainer.Set<TContract>(int holderId, dynamic implementation)
    {
        _objectsMap[holderId][typeof(TContract)] = implementation;
    }
}