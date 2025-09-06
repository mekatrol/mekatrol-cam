using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry.Entities;

public class CircleEntity : BaseEntity, IGeometricEntity
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public CircleEntity()
        : this(0, 0, 1, new Transform(), null)
    {

    }

    public CircleEntity(PointDouble center, double radius, Guid? id)
        : this(center.X, center.Y, radius, new Transform(), id)
    {
    }

    public CircleEntity(double centerX, double centerY, double radius, ITransform transform, Guid? id = null)
        : base(GeometricEntityType.Circle, id, new PointDouble(centerX, centerY), transform)
    {
        Radius = radius;

        Boundary = new Boundary();
        UpdateBoundary();
    }

    public double Radius { get; set; }

    public override IList<PointDouble[]> ToPoints()
    {
        throw new NotImplementedException();
    }

    public override void UpdateBoundary()
    {
        Boundary = GeometryUtils.GetBoundary(Location.X - Radius, Location.Y - Radius, Location.X + Radius, Location.Y + Radius);
    }

    protected override void TransformGeometry(Matrix3 m)
    {
        // We avergage the scale between X and Y,
        // though for circle operations it is likely that 
        // the scale will have equal X and Y components
        var scale = m.GetScale();
        var scaleAvg = (scale.X + scale.Y) / 2;
        Radius *= scaleAvg;
    }
}
