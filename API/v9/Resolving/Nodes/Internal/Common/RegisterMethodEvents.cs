//TODO: Easier invocation of events, need to patch methods

using System;
using System.Reflection;
using GMutagen.v9.Contracts.Interfaces;
using GMutagen.v9.Extensions;
using GMutagen.v9.Resolving.Contexts;
using GMutagen.v9.Resolving.Nodes.Interfaces;

namespace GMutagen.v9.Resolving.Nodes.Internal.Common;

public class RegisterMethodEvents : IResolverNode
{
    public bool Resolve(Context context)
    {
        if (!context.Type.IsAssignableTo(typeof(IContract)))
            return false;

        var type = context.Type;
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        foreach (var method in methods)
        {
            var methodAttributes = method.GetCustomAttributes();
            if(!methodAttributes.Contains<MethodEventAttribute>())
                continue;

            var methodEventAttribute = methodAttributes.Get<MethodEventAttribute>();
            var options = methodEventAttribute!.Options;

            if (options == MethodEventOptions.Before)
                CreateBefore(method);
            if (options == MethodEventOptions.After)
                CreateAfter(method);
            if (options == MethodEventOptions.BeforeNAfter)
                CreateBeforeNAfter(method);
        }

        return false;
    }

    private void CreateBeforeNAfter(MethodInfo method)
    {
        CreateBefore(method);
        CreateAfter(method);
    }

    private void CreateAfter(MethodInfo method)
    {
        //TODO: Creating variable, store in storage, generate events, patch method 
    }

    private void CreateBefore(MethodInfo method)
    {
        //TODO: Creating variable, store in storage, generate events, patch method 
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class MethodEventAttribute(MethodEventOptions options) : Attribute
{
    public MethodEventOptions Options { get; } = options;
}

public enum MethodEventOptions
{
    Before,
    After,
    BeforeNAfter,
}