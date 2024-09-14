using System;

namespace GMutagen.v8.Test;

class ObjectId : Id
{
    public ObjectId(Type type, object value) : base(type, value) { }
}