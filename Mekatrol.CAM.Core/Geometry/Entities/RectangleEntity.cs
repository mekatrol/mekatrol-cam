namespace Mekatrol.CAM.Core.Geometry.Entities;

public class RectangleEntity : PointsEntity, IGeometricEntity
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public RectangleEntity()
        : this(0, 0, 0, 0, 0, 0, new Transform())
    {

    }

    public RectangleEntity(double x, double y, double w, double h, double rx, double ry, ITransform transform, Guid? id = null)
        : base(GeometricEntityType.Rectangle, id, new PointDouble(x, y), transform)
    {
        Size = new PointDouble(w, h);
        CornerRounding = new PointDouble(rx, ry);

        var points = new[]
        {
            new PointDouble(Location.X, Location.Y),
            new PointDouble(Location.X + Size.X, Location.Y),
            new PointDouble(Location.X + Size.X, Location.Y + Size.Y),
            new PointDouble(Location.X, Location.Y + Size.Y)
        }.ToList();

        SetUntransformedPoints(points);
    }

    public PointDouble Size { get; set; }

    public PointDouble CornerRounding { get; set; }

    public override IList<PointDouble[]> ToPoints()
    {
        return [_transformedPoints.ToArray()];
    }

    protected override void TransformGeometry(Matrix3 m)
    {
        Size *= m.GetScale();
    }
}
