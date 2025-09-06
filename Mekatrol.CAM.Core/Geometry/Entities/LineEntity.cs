using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry.Entities;

internal class LineEntity : PointsEntity, IGeometricEntity
{
    public LineEntity(PointDouble startLocation, PointDouble endLocation, ITransform transform, Guid? id = null)
        : this(startLocation.X, startLocation.Y, endLocation.X, endLocation.Y, transform, id)
    {
    }

    public LineEntity(double x1, double y1, double x2, double y2, ITransform transform, Guid? id = null)
        : base(GeometricEntityType.Line, id, new PointDouble(x1, y1), transform)
    {
        EndLocation = new PointDouble(x2, y2);

        SetUntransformedPoints([Location, EndLocation]);
    }

    public PointDouble EndLocation { get; set; }

    public override IList<PointDouble[]> ToPoints()
    {
        return [_rotatedBoundary.ToArray()];
    }

    public override string ToString()
    {
        return $"({Location}:{EndLocation})";
    }

    protected override void TransformGeometry(Matrix3 m)
    {
        IList<PointDouble> points = new[] { Location, EndLocation }.ToList();
        points = GeometryUtils.RotatePoints(points, m.GetRotation());

        Location = points[0];
        EndLocation = points[1];
    }
}
