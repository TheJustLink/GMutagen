using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GMutagen.v9.Logging.Logger;

namespace GMutagen.v9.Logging.Messages;

public class Message
{
    protected readonly InterpolationString InterpolationString;
    protected readonly Dictionary<string, string> Placeholders;

    public Message( 
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "")
    {
        InterpolationString = new InterpolationString($"[{{{nameof(PlaceHolders.Sender)}}}] {{{nameof(PlaceHolders.Message)}}}");
        Placeholders = new Dictionary<string, string>()
            .AddDebugInfo(filePath, lineNumber, memberName);
    }

    public string Get() => InterpolationString.Interpolate(Placeholders);

    public static implicit operator string(Message message)
    {
        return message.Get();
    }
}