using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry;

internal class QuadraticBezier : BaseEntity, IGeometricEntity
{
    public QuadraticBezier(PointDouble location, PointDouble control, PointDouble endLocation, GeometryTransform transform, Guid? id = null)
        : base(GeometricEntityType.QuadraticBezier, id, location, transform)
    {
        EndLocation = endLocation;
        Control = control;

        var cubic = this.ToCubic();
        Boundary = cubic.Boundary;
    }

    public PointDouble EndLocation { get; set; }

    public PointDouble Control { get; set; }

    public override IList<PointDouble[]> ToPoints()
    {
        var points = this.PlotQuadraticBezier().Select(p => new List<PointDouble>([p]).ToArray()).ToList();
        return points;
    }

    public override void UpdateBoundary()
    {
        var points = this.PlotQuadraticBezier();
        Boundary = GeometryUtils.GetBoundary(points?.ToList() ?? []);
    }

    protected override void TransformGeometry(Matrix3 m)
    {
        EndLocation *= m;
        Control *= m;
    }
}
