using System;
using System.Reflection;

using GMutagen.v8.Objects.Template;
using GMutagen.v8.Values;

using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v8.Test;

public class ContractResolveBuilder
{
    private readonly ServiceCollection _serviceCollection = new();
    
    private object? _resolveKey = null;


    // _buildServices.AddTransient<TContract>(provider =>
    // {
    //     var type = typeof(TImplementation);
    //     var valueType = typeof(IValue<>);
    //
    //     foreach (var constructor in type.GetConstructors())
    //     {
    //         if (constructor.GetCustomAttribute<InjectAttribute>() is null)
    //             continue;
    //
    //         var parameters = constructor.GetParameters();
    //         var resultParameters = new object[parameters.Length];
    //
    //         for (var i = 0; i < parameters.Length; i++)
    //         {
    //             var parameter = parameters[i];
    //
    //             if (parameter.ParameterType != valueType)
    //             {
    //                 resultParameters[i] = provider.GetRequiredService(parameter.ParameterType);
    //             }
    //             else
    //             {
    //                 var locationKey = parameter.GetCustomAttribute<ValueLocationAttribute>();
    //                 var storage = locationKey is not null
    //                 ? provider.GetRequiredKeyedService<IStorage<TId>>(locationKey)
    //                     : provider.GetRequiredService<IStorage<TId>>();
    //
    //                 var genericValueType = parameter.ParameterType.GenericTypeArguments[0];
    //                 var externalValueType = typeof(ExternalValue<,>).MakeGenericType(typeof(TId), genericValueType);
    //
    //                 Activator.CreateInstance(externalValueType, );
    //             }
    //         }
    //
    //         foreach (var parameter in constructor.GetParameters())
    //         {
    //
    //         }
    //         foreach (var constructorAttribute in constructor.GetCustomAttributes(true))
    //         {
    //             if (constructorAttribute is not InjectAttribute)
    //                 continue;
    //
    //             var parameters = constructor.GetParameters();
    //             var resultParameters = new object[parameters.Length];
    //             for (int i = 0; i < parameters.Length; i++)
    //                 resultParameters[i] = provider.GetRequiredService(parameters[i].ParameterType);
    //
    //             constructor.Invoke(instance, parameters);
    //             return instance!;
    //         }
    //     }
    //
    //     throw new Exception("Constructor with InjectAttribute was not found");
    // });

    public IContractResolverChain Build() 
    {
        var provider = _serviceCollection.BuildServiceProvider();
        var resolveKey = _resolveKey;
        //return new ContractResolverChain(provider, resolveKey);
        throw new NotImplementedException();
    }

    public ContractResolveBuilder SetResolveKey(object? resolveKey)
    {
        _resolveKey = resolveKey;
        return this;
    }

    public ContractResolveBuilder Add<TInterface, TImplementation>(object? key = null)
    {
        if (key == null)
            _serviceCollection.AddTransient(typeof(TInterface), ResolveFromProvider<TImplementation>);
        else
            _serviceCollection.AddKeyedTransient(typeof(TInterface), key, ResolveFromProvider<TImplementation>);
        
        return this;
    }

    public ContractResolveBuilder AddResolution<TInterface, TImplementation>(object? key = null)
    {
        if (key == null)
            _serviceCollection.AddTransient(typeof(TInterface), typeof(TImplementation));
        else
            _serviceCollection.AddKeyedTransient(typeof(TInterface), key, typeof(TImplementation));

        return this;
    }

    public ContractResolveBuilder ResolveFromAnotherKey<TType>(object? key, object? targetKey)
    {
        _serviceCollection.AddKeyedTransient(typeof(TType), key, ResolveFromAnotherKey<TType>(targetKey));
        return this;
    }

    private object ResolveFromProvider<TImplementation>(IServiceProvider serviceProvider)
    {
        var type = typeof(TImplementation);
        var valueType = typeof(IValue<>);
        var constructors = type.GetConstructors();
        
        foreach (var constructor in constructors)
        {
            if (constructor.GetCustomAttribute<InjectAttribute>() is null || constructors.Length != 1)
                continue;
        
            var parameters = constructor.GetParameters();
            var resultParameters = new object[parameters.Length];
        
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
        
                if (parameter.ParameterType != valueType)
                {
                    resultParameters[i] = serviceProvider.GetRequiredService(parameter.ParameterType);
                }
                else
                {
                    var locationKey = parameter.GetCustomAttribute<ValueLocationAttribute>();
                    // var storage = locationKey is not null
                    //     ? serviceProvider.GetRequiredKeyedService<IStorage<TImplementation>>(locationKey)
                    //     : serviceProvider.GetRequiredService<IStorage<TImplementation>>();
                    //
                    // var genericValueType = parameter.ParameterType.GenericTypeArguments[0];
                    // var externalValueType = typeof(ExternalValue<,>).MakeGenericType(typeof(TImplementation), genericValueType,);

                    // var instance = Activator.CreateInstance(externalValueType,);
                    // constructor.Invoke(instance, resultParameters);
                    // return instance!;
                }
            }
        }
        
        throw new Exception("Constructor with InjectAttribute was not found");
    }
    
    private object ResolveFromProvider<TImplementation>(IServiceProvider serviceProvider, object? key)
    {
        var type = typeof(TImplementation);
        var valueType = typeof(IValue<>);
        var constructors = type.GetConstructors();
        
        foreach (var constructor in constructors)
        {
            if (constructor.GetCustomAttribute<InjectAttribute>() is null || constructors.Length != 1)
                continue;
        
            var parameters = constructor.GetParameters();
            var resultParameters = new object[parameters.Length];
        
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
        
                if (parameter.ParameterType != valueType)
                {
                    resultParameters[i] = serviceProvider.GetRequiredKeyedService(parameter.ParameterType, key);
                }
                else
                {
                    // var locationKey = parameter.GetCustomAttribute<ValueLocationAttribute>();
                    // var storage = locationKey is not null
                    // ? serviceProvider.GetRequiredKeyedService<IStorage<TImplementation>>(locationKey)
                    //     : serviceProvider.GetRequiredService<IStorage<TImplementation>>();
                    //
                    // var genericValueType = parameter.ParameterType.GenericTypeArguments[0];
                    //var externalValueType = typeof(ExternalValue<,>).MakeGenericType(typeof(TImplementation), genericValueType,);
        
                    //var instance = Activator.CreateInstance(externalValueType,);
                    //constructor.Invoke(instance, resultParameters);
                    //return instance!;
                }
            }
        }
        
        throw new Exception("Constructor with InjectAttribute was not found");
    }
    
    private Func<IServiceProvider, object?, object?> ResolveFromAnotherKey<TType>(object? targetKey)
    {
        return ResolveFromProviderFromAnotherKey<TType>;
        
        object ResolveFromProviderFromAnotherKey<TImplementation>(IServiceProvider serviceProvider, object? key)
        {
            return serviceProvider.GetRequiredKeyedService(typeof(TImplementation), targetKey);
        }
    }
}