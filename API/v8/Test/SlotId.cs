using System;

namespace GMutagen.v8.Test;

class SlotId : Id
{
    public SlotId(Type type, object value) : base(type, value) { }
}