using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry.Entities;

public class EllipseEntity : BaseEntity, IGeometricEntity
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>

    public EllipseEntity()
        : this(0, 0, 0, 0, new GeometryTransform())
    {

    }

    public EllipseEntity(double x, double y, double rx, double ry, ITransform transform, Guid? id = null)
        : base(GeometricEntityType.Ellipse, id, new PointDouble(x, y), transform)
    {
        Radius = new PointDouble(rx, ry);
        Boundary = new Boundary();
        UpdateBoundary();
    }

    public PointDouble Radius { get; set; }

    public override IList<PointDouble[]> ToPoints()
    {
        throw new NotImplementedException();
    }

    public override void UpdateBoundary()
    {
        Boundary = GeometryUtils.GetBoundary(Location.X - Radius.X, Location.Y - Radius.Y, Location.X + Radius.X, Location.Y + Radius.Y);
    }

    protected override void TransformGeometry(Matrix3 m)
    {
        Radius *= m.GetScale();
    }
}
