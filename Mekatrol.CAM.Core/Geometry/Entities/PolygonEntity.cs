namespace Mekatrol.CAM.Core.Geometry.Entities;

internal class PolygonEntity : PolybaseEntity
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

    public PolygonEntity(IReadOnlyList<PointDouble> points, ITransform transform, Guid? id = null)
        : base(GeometricEntityType.Polygon, points, transform, id)
    {
    }
}
