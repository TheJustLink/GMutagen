using System.Text;

namespace DotExporterApi;

public class DotEdge(string from, string to)
{
    private readonly Dictionary<DotAttribute, string> _attributes = new();

    public void Set(DotAttribute key, string value) => _attributes[key] = value;

    public string Build(bool directed)
    {
        var op = directed ? DotConstants.ARROW : DotConstants.LINE;
        var sb = new StringBuilder();
        sb.Append($"{from} {op} {to}");
        if (_attributes.Count > 0)
        {
            sb.Append(DotConstants.L_BRACKET);
            bool first = true;
            foreach (var attr in _attributes)
            {
                if (!first) sb.Append(DotConstants.COMMA);
                sb.Append($"{attr.Key.ToAttributeKey()}{DotConstants.EQUALS}{DotConstants.QUOTE}{attr.Value}{DotConstants.QUOTE}");
                first = false;
            }
            sb.Append(DotConstants.R_BRACKET);
        }
        sb.Append(DotConstants.SEMICOLON);
        return sb.ToString();
    }
}