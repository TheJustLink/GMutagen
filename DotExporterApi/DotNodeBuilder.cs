namespace DotExporterApi;

public class DotNodeBuilder : BaseNodeBuilder<DotNodeBuilder>
{
    private readonly DotBuilder _parent;
    private bool _isBuilt;

    public DotNodeBuilder(DotBuilder parent, string id) : base(id)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
    }

    public DotBuilder Build()
    {
        if (_isBuilt)
            throw new InvalidOperationException("This node has already been built.");
        _parent.AddNodeInternal(Node);
        _isBuilt = true;
        return _parent;
    }
}