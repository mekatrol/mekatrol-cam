using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry.Entities;

internal abstract class PolybaseEntity : BaseEntity, IGeometricEntity
{
    protected PolybaseEntity(GeometricEntityType entityType, IReadOnlyList<PointDouble> points, ITransform transform, Guid? id = null)
        : base(entityType, id, points.Count == 0 ? new PointDouble(0, 0) : points[0], transform)
    {
        Points = points;
        Boundary = new Boundary();
        UpdateBoundary();
    }

    public IReadOnlyList<PointDouble> Points { get; set; }

    public override void UpdateBoundary()
    {
        Boundary = GeometryUtils.GetBoundary(Points.ToList());
    }

    protected override void TransformGeometry(Matrix3 m)
    {
        Points = Points.Select(x => x * m).ToList();
    }

    public override IList<PointDouble[]> ToPoints()
    {
        return [Points.ToArray()];
    }
}
