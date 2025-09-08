namespace Mekatrol.CAM.Core.Geometry.Entities;

public class QuadraticBezierEntity : BaseEntity, IGeometricEntity
{
    public QuadraticBezierEntity(PointDouble location, PointDouble control, PointDouble endLocation, GeometryTransform transform, Guid? id = null)
        : base(GeometricEntityType.QuadraticBezier, id, location, transform)
    {
        EndLocation = endLocation;
        Control = control;

        var cubic = this.ToCubic();
        BoundaryUntransformed = cubic.BoundaryUntransformed;
    }

    public PointDouble EndLocation { get; set; }

    public PointDouble Control { get; set; }

    public override IReadOnlyList<PointDouble[]> ToPoints()
    {
        var points = this.PlotQuadraticBezier().Select(p => new List<PointDouble>([p]).ToArray()).ToList();
        return points;
    }
}
