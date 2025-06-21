using System.Text;

namespace DotExporterApi;

public class DotNode(string id)
{
    private readonly Dictionary<DotAttribute, string> _attributes = new();
    public string Id => id;

    public void Set(DotAttribute key, string value) => _attributes[key] = value;

    public string Build()
    {
        if (_attributes.Count == 0) return id + DotConstants.SEMICOLON;

        var sb = new StringBuilder();
        sb.Append(id);
        sb.Append(DotConstants.L_BRACKET);
        bool first = true;
        foreach (var attr in _attributes)
        {
            if (!first) sb.Append(DotConstants.COMMA);
            sb.Append($"{attr.Key.ToAttributeKey()}{DotConstants.EQUALS}{DotConstants.QUOTE}{attr.Value}{DotConstants.QUOTE}");
            first = false;
        }
        sb.Append(DotConstants.R_BRACKET);
        sb.Append(DotConstants.SEMICOLON);
        return sb.ToString();
    }
}