namespace DotExporterApi;

public class DotEdgeBuilder(DotBuilder parent, string from, string to)
{
    private readonly DotEdge _edge = new(from, to);

    public DotEdgeBuilder Set(DotAttribute attr, string value)
    {
        _edge.Set(attr, value);
        return this;
    }

    public DotEdgeBuilder Label(string val) => Set(DotAttribute.Label, val);
    public DotEdgeBuilder Color(DotColor color) => Set(DotAttribute.Color, color.ToAttributeKey());
    public DotEdgeBuilder Style(DotStyle style) => Set(DotAttribute.Style, style.ToAttributeKey());

    public DotBuilder Build()
    {
        parent.AddEdgeInternal(_edge);
        return parent;
    }
}