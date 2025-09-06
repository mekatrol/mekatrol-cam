namespace Mekatrol.CAM.Core.Geometry.Entities;

public class CubicBezierEntity : BaseEntity, IGeometricEntity
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public CubicBezierEntity()
        : this(new PointDouble(), new PointDouble(), new PointDouble(), new PointDouble(), new GeometryTransform())
    {
    }

    public CubicBezierEntity(PointDouble location, PointDouble control1, PointDouble control2, PointDouble endLocation, GeometryTransform transform, Guid? id = null)
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
        var points = this.PlotCubicBezier().Select(p => new List<PointDouble>([p]).ToArray()).ToList();
        return points;
    }

    public override void UpdateBoundary()
    {
        var (min, max) = this.GetExtentsAnalytical();
        Boundary = new Boundary { Location = min, Size = new PointDouble(max.X - min.X, max.Y - min.Y) };
    }

    protected override void TransformGeometry(Matrix3 m)
    {
        EndLocation *= m;
        Control1 *= m;
        Control2 *= m;
    }
}
