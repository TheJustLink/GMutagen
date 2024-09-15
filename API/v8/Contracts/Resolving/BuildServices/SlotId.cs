using System;

namespace GMutagen.v8.Contracts.Resolving.BuildServices;

class SlotId : Id
{
    public SlotId(Type type, object value) : base(type, value) { }
}