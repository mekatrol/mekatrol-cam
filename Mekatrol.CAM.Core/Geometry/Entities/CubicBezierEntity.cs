namespace Mekatrol.CAM.Core.Geometry.Entities;

public class CubicBezierEntity(PointDouble location, PointDouble control1, PointDouble control2, PointDouble endLocation, GeometryTransform transform, Guid? id = null) : BaseEntity(GeometricEntityType.CubicBezier, id, location, transform), IGeometricEntity
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public CubicBezierEntity()
        : this(new PointDouble(), new PointDouble(), new PointDouble(), new PointDouble(), new GeometryTransform())
    {
    }

    public PointDouble EndLocation { get; set; } = endLocation;

    public PointDouble Control1 { get; set; } = control1;

    public PointDouble Control2 { get; set; } = control2;

    public override IReadOnlyList<PointDouble[]> ToPoints()
    {
        var points = this.PlotCubicBezier().Select(p => new List<PointDouble>([p]).ToArray()).ToList();
        return points;
    }
}
