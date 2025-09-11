namespace Mekatrol.CAM.Core.Geometry.Entities;

public class PolygonEntity(IReadOnlyList<PointDouble> points, GeometryTransform transform)
    : PolybaseEntity(GeometricEntityType.Polygon, points, transform)
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public PolygonEntity() : this(new GeometryTransform())
    {

    }

    public PolygonEntity(GeometryTransform transform) : this([], transform)
    {
    }
}
