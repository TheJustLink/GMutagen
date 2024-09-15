using System;

namespace GMutagen.v8.Contracts.Resolving.BuildServices;

class ObjectId : Id
{
    public ObjectId(Type type, object value) : base(type, value) { }
}