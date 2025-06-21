namespace DotExporterApi;

public class DotSubgraphBuilder(DotBuilder parent, string name)
{
    internal readonly DotSubgraph _subgraph = new(name);

    public DotSubgraphBuilder Set(DotAttribute attr, string value)
    {
        _subgraph.Set(attr, value);
        return this;
    }

    public DotSubgraphBuilder Label(string val) => Set(DotAttribute.Label, val);
    public DotSubgraphBuilder Color(DotColor color) => Set(DotAttribute.Color, color.ToAttributeKey());
    public DotSubgraphBuilder Style(DotStyle style) => Set(DotAttribute.Style, style.ToAttributeKey());

    public DotNodeBuilderWrapper AddNode(string id) => new(this, id);

    public DotBuilder Build()
    {
        parent.AddSubgraphInternal(_subgraph);
        return parent;
    }

    public class DotNodeBuilderWrapper : BaseNodeBuilder<DotNodeBuilderWrapper>
    {
        private readonly DotSubgraphBuilder _subParent;
        private bool _isBuilt;

        public DotNodeBuilderWrapper(DotSubgraphBuilder subParent, string id) : base(id)
        {
            _subParent = subParent;
        }

        public DotSubgraphBuilder Build()
        {
            if (_isBuilt)
                throw new InvalidOperationException("This node has already been built.");
            _subParent._subgraph.AddNode(Node);
            _isBuilt = true;
            return _subParent;
        }
    }
}