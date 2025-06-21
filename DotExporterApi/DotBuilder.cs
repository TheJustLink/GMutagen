using System.Text;

namespace DotExporterApi;

public class DotBuilder
{
    private readonly List<DotNode> _nodes = new();
    private readonly List<DotEdge> _edges = new();
    private readonly List<DotSubgraph> _subgraphs = new();
    private readonly Dictionary<DotAttribute, string> _attributes = new();
    private readonly bool _directed;
    private readonly string _graphName;

    public DotBuilder(string graphName = "G", bool directed = true)
    {
        _graphName = graphName;
        _directed = directed;
    }

    public DotBuilder RankDir(DotRankDir direction)
    {
        _attributes[DotAttribute.RankDir] = direction.ToAttributeKey();
        return this;
    }

    public DotNodeBuilder AddNode(string id) => new(this, id);

    public DotEdgeBuilder AddEdge(string from, string to) => new(this, from, to);

    public DotSubgraphBuilder AddSubgraph(string name) => new(this, name);

    internal void AddNodeInternal(DotNode node) => _nodes.Add(node);
    internal void AddEdgeInternal(DotEdge edge) => _edges.Add(edge);
    internal void AddSubgraphInternal(DotSubgraph subgraph) => _subgraphs.Add(subgraph);

    public string Build()
    {
        var sb = new StringBuilder();
        sb.AppendLine((_directed ? DotConstants.DIGRAPH : DotConstants.GRAPH) + _graphName + " {");

        foreach (var attr in _attributes)
            sb.AppendLine($"{DotConstants.INDENT}{attr.Key.ToAttributeKey()}{DotConstants.EQUALS}{DotConstants.QUOTE}{attr.Value}{DotConstants.QUOTE}{DotConstants.SEMICOLON}");

        foreach (var node in _nodes)
            sb.AppendLine($"{DotConstants.INDENT}{node.Build()}");

        foreach (var edge in _edges)
            sb.AppendLine($"{DotConstants.INDENT}{edge.Build(_directed)}");

        foreach (var sub in _subgraphs)
            sb.AppendLine(sub.Build(_directed));

        sb.AppendLine("}");
        return sb.ToString();
    }
}