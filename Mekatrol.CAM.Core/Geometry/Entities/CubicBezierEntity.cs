using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry.Entities;

internal class CubicBezierEntity : BaseEntity, IGeometricEntity
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public CubicBezierEntity()
        : this(new PointDouble(), new PointDouble(), new PointDouble(), new PointDouble(), new Transform())
    {
    }

    public CubicBezierEntity(PointDouble location, PointDouble control1, PointDouble control2, PointDouble endLocation, ITransform transform, Guid? id = null)
        : base(GeometricEntityType.CubicBezier, id, location, transform)
    {
        EndLocation = endLocation;
        Control1 = control1;
        Control2 = control2;

        Boundary = new Boundary();
        UpdateBoundary();
    }

    public PointDouble EndLocation { get; set; }

    public PointDouble Control1 { get; set; }

    public PointDouble Control2 { get; set; }

    public override IList<PointDouble[]> ToPoints()
    {
        throw new NotImplementedException();
    }

    public override void UpdateBoundary()
    {
        var points = BezierSpline.PlotCubicBezier(this);
        Boundary = GeometryUtils.GetBoundary(points?.ToList() ?? []);
    }

    protected override void TransformGeometry(Matrix3 m)
    {
        EndLocation *= m;
        Control1 *= m;
        Control2 *= m;
    }
}
