using Logger.Common;
using Logger.Logger;

namespace Logger.Messages;

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