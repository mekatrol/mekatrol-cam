namespace Mekatrol.CAM.Core.Geometry.Entities;

public class PolygonEntity(IReadOnlyList<PointDouble> points, ITransform transform, Guid? id = null)
    : PolybaseEntity(GeometricEntityType.Polygon, points, transform, id)
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public PolygonEntity() : this(new Transform(), null)
    {

    }

    public PolygonEntity(ITransform transform, Guid? id = null) : this([], transform, id)
    {
    }
}
