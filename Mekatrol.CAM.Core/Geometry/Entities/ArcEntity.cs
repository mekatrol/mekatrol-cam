using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry.Entities;

public class ArcEntity : BaseEntity, IGeometricEntity
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public ArcEntity() : this(new PointDouble(0, 0), new PointDouble(1, 0), new PointDouble(-1, 0), new PointDouble(2, 2), 0, 180, 0, new GeometryTransform())
    {

    }

    public ArcEntity(
        PointDouble centerLocation,
        PointDouble startLocation,
        PointDouble endLocation,
        PointDouble radii,
        double startAngle,
        double sweepAngle,
        double ellipseRotation,
        GeometryTransform transform,
        Guid? id = null) : base(GeometricEntityType.Arc, id, centerLocation, transform)
    {
        Radii = new PointDouble(radii.X, radii.Y);
        StartLocation = startLocation;
        EndLocation = endLocation;
        StartAngle = startAngle;
        SweepAngle = sweepAngle;
        EllipseRotation = ellipseRotation;

        Boundary = new Boundary();
        UpdateBoundary();
    }

    public PointDouble StartLocation { get; set; }

    public PointDouble EndLocation { get; set; }

    public PointDouble Radii { get; set; }

    public double EllipseRotation { get; set; }

    public double StartAngle { get; set; }

    public double SweepAngle { get; set; }

    public override IList<PointDouble[]> ToPoints()
    {
        throw new NotImplementedException();
    }

    public override void UpdateBoundary()
    {
        var (_, minX, minY, maxX, maxY) = GeometryUtils.PlotEllipse(
            Location.X, Location.Y,
            Radii.X, Radii.Y,
            GeometryUtils.DegreesToRadians(StartAngle),
            GeometryUtils.DegreesToRadians(SweepAngle),
            GeometryUtils.DegreesToRadians(EllipseRotation));

        Boundary = GeometryUtils.GetBoundary(minX, minY, maxX, maxY);
    }

    protected override void TransformGeometry(Matrix3 m)
    {
        StartLocation *= m;
        EndLocation *= m;
        Radii *= m.GetScale();
        StartAngle += GeometryUtils.RadiansToDegrees(m.GetRotation());
    }
}
