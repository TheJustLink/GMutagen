using System;

namespace GMutagen.v8.Test;

class ValueId : Id
{
    public ValueId(Type type, object value) : base(type, value) { }
}