using System;
using System.Collections.Generic;

namespace GMutagen.v5.Container;

public class ObjectTemplate
{
    private readonly ObjectTemplateContainer _container;
    private readonly HashSet<Type> _contracts;

    public ObjectTemplate() : this(new ObjectTemplateContainer(), new HashSet<Type>())
    {
    }

    public ObjectTemplate(ObjectTemplateContainer container) : this(container, new HashSet<Type>())
    {
    }

    public ObjectTemplate(params ObjectTemplate[] baseTemplates)
    {
        _contracts = new HashSet<Type>();
        _container = new ObjectTemplateContainer();

        foreach (var objectTemplate in baseTemplates)
        {
            _contracts.UnionWith(objectTemplate._contracts);
            _container.Add(objectTemplate._container);
        }
    }

    public ObjectTemplate(ObjectTemplateContainer container, HashSet<Type> contracts)
    {
        _container = container;
        _contracts = contracts;
    }

    public Object Create()
    {
        var instanceContracts = new Dictionary<Type, object>();

        foreach (var contractType in _contracts)
        {
            var instance = _container.Resolve(contractType);
            instanceContracts.Add(contractType, instance);
        }

        return new Object(instanceContracts);
    }

    public ObjectTemplate Add<T>()
    {
        var targetType = typeof(T);
        if (_contracts.Contains(targetType))
            return this;

        _contracts.Add(targetType);
        _container.Add<T>(false);
        return this;
    }

    public ObjectTemplate Add<T>(Type type)
    {
        var targetType = typeof(T);
        if (_contracts.Contains(targetType))
            return this;

        _contracts.Add(targetType);
        _container.Add<T>(false).As(type, false);
        return this;
    }

    public ObjectTemplate Add<T>(object instance)
    {
        var targetType = typeof(T);
        if (_contracts.Contains(targetType))
            return this;

        _contracts.Add(targetType);
        _container.Add<T>(false).As(instance.GetType(), false);
        return this;
    }

    public ObjectTemplate Set<T>(object value)
    {
        var targetType = typeof(T);
        if (_contracts.Contains(targetType))
            _contracts.Remove(targetType);


        _contracts.Add(targetType);
        _container.Add<T>(false).As(value.GetType(), false);
        return this;
    }
}