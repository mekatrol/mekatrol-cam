namespace Mekatrol.CAM.Core.Geometry.Entities;

public class PolylineEntity(IReadOnlyList<PointDouble> points, ITransform transform, Guid? id = null)
    : PolybaseEntity(GeometricEntityType.Polyline, points, transform, id)
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public PolylineEntity() : this(new Transform(), null)
    {

    }

    public PolylineEntity(ITransform transform, Guid? id = null) : this([], transform, id)
    {
    }
}
