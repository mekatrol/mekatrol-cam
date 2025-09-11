namespace Mekatrol.CAM.Core.Geometry.Entities;

public abstract class PolybaseEntity(GeometricEntityType entityType, IReadOnlyList<PointDouble> points, GeometryTransform transform, Guid? id = null)
    : BaseEntity(entityType, id, points.Count == 0 ? new PointDouble(0, 0) : points[0], transform), IGeometricEntity
{
    public IReadOnlyList<PointDouble> Points { get; set; } = points;

    public override IReadOnlyList<PointDouble[]> ToPoints()
    {
        return [Points.ToArray()];
    }
}
