namespace Mekatrol.CAM.Core.Geometry.Entities;

internal class PolylineEntity : PolybaseEntity
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

    public PolylineEntity(IReadOnlyList<PointDouble> points, ITransform transform, Guid? id = null)
        : base(GeometricEntityType.Polyline, points, transform, id)
    {
    }
}
