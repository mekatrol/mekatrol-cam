namespace Mekatrol.CAM.Core.Geometry.Entities;

public class LineEntity(double x1, double y1, double x2, double y2, GeometryTransform transform, Guid? id = null) : BaseEntity(GeometricEntityType.Line, id, new PointDouble(x1, y1), transform), IGeometricEntity
{
    public LineEntity(PointDouble startLocation, PointDouble endLocation, GeometryTransform transform, Guid? id = null)
        : this(startLocation.X, startLocation.Y, endLocation.X, endLocation.Y, transform, id)
    {
    }

    public PointDouble EndLocation { get; set; } = new PointDouble(x2, y2);

    public override IReadOnlyList<PointDouble[]> ToPoints()
    {
        return [[Location, EndLocation]];
    }

    public override string ToString()
    {
        return $"({Location}:{EndLocation})";
    }
}
