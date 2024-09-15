using System;

namespace GMutagen.v8.Contracts.Resolving.BuildServices;

class ValueId : Id
{
    public ValueId(Type type, object value) : base(type, value) { }
}