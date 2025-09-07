namespace Mekatrol.CAM.Core.Geometry.Entities;

public class RectangleEntity(double x, double y, double w, double h, double rx, double ry, GeometryTransform transform, Guid? id = null) 
    : BaseEntity(GeometricEntityType.Rectangle, id, new PointDouble(x, y), transform), IGeometricEntity
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public RectangleEntity()
        : this(0, 0, 0, 0, 0, 0, new GeometryTransform())
    {

    }

    public PointDouble Size { get; set; } = new PointDouble(w, h);

    public PointDouble CornerRounding { get; set; } = new PointDouble(rx, ry);

    public override IReadOnlyList<PointDouble[]> ToPoints()
    {
        var topLeft = Location;
        var topRight = Location + new PointDouble(Size.X, 0);
        var bottomRight = Location + new PointDouble(Size.X, Size.Y);
        var bottomLeft = Location + new PointDouble(0, Size.Y);

        return [[topLeft, topRight, bottomRight, bottomLeft]];
    }
}
