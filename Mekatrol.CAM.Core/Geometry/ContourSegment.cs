namespace Mekatrol.CAM.Core.Geometry;

public readonly struct ContourSegment(ControurSegmentType segmentType, IEnumerable<PointDouble> points)
{
    public ControurSegmentType SegmentType { get; } = segmentType;

    public IList<PointDouble> Points { get; } = points.ToList();

    public override string ToString()
    {
        return $"{SegmentType}: {string.Join(',', Points.Select(p => $"({p.X},{p.Y})"))}";
    }
}
