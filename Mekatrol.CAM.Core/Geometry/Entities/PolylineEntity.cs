namespace Mekatrol.CAM.Core.Geometry.Entities;

public class PolylineEntity(IReadOnlyList<PointDouble> points, GeometryTransform transform)
    : PolybaseEntity(GeometricEntityType.Polyline, points, transform)
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public PolylineEntity() : this(new GeometryTransform())
    {

    }

    public PolylineEntity(GeometryTransform transform) : this([], transform)
    {
    }
}
