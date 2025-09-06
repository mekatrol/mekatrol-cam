namespace Mekatrol.CAM.Core.Clipping;

internal class PolygonLongPath : List<PolygonLong>, IClipSolution
{
    public SolutonType SolutionType => SolutonType.Path;

    internal PolygonLongPath() { }

    internal PolygonLongPath(PolygonLong polygon) : base(1)
    {
        if (polygon == null)
        {
            throw new ArgumentNullException(nameof(polygon));
        }

        Add(polygon);
    }

    internal PolygonLongPath(IEnumerable<PolygonLong> polygons) : base(polygons)
    {
    }

    internal PolygonLongPath(int capacity) : base(capacity) { }

    internal double Area
    {
        get { return this.Sum(polygon => polygon.Area); }
    }

    internal void ReversePolygonOrientations()
    {
        foreach (var polygon in this)
        {
            polygon.Reverse();
        }
    }

    internal PolygonLongPath Cleaned(double distance = 1.415)
    {
        var cleaned = new PolygonLongPath(Count);
        cleaned.AddRange(this.Select(polygon => polygon.Cleaned(distance)));
        return cleaned;
    }

    internal static PolygonLongPath FromTree(PolygonTree tree, NodeType nodeType = NodeType.Any)
    {
        return new PolygonLongPath(tree.Children.Count) { { tree, nodeType } };
    }

    private void Add(PolygonNode treeNode, NodeType nodeType)
    {
        var match = true;

        switch (nodeType)
        {
            case NodeType.Open: return;
            case NodeType.Closed: match = !treeNode.IsOpen; break;
        }

        if (treeNode.Polygon.Count > 0 && match)
        {
            Add(treeNode.Polygon);
        }

        foreach (var polyNode in treeNode.Children)
        {
            Add(polyNode, nodeType);
        }
    }
}
