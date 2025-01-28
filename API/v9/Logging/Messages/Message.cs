using System.Collections.Generic;
using GMutagen.v9.Logging.Common;
using GMutagen.v9.Logging.Logger;

namespace GMutagen.v9.Logging.Messages;

public class Message
{
    protected readonly InterpolationString InterpolationString;
    protected readonly Dictionary<string, string> Placeholders;

    public Message( 
   )
    {
        InterpolationString = new InterpolationString(
            $"[{{{nameof(PlaceHolders.Sender)}}}] " +
            $"{{{nameof(PlaceHolders.Message)}}}");
        Placeholders = new Dictionary<string, string>();
    }

    public string Get() => InterpolationString.Interpolate(Placeholders);

    public static implicit operator string(Message message)
    {
        return message.Get();
    }
}