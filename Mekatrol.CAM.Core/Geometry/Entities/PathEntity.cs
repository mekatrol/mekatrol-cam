using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry.Entities;

public class PathEntity : BaseEntity, IGeometricPathEntity
{
    public PathEntity()
        : this(0, 0, [], false, new GeometryTransform(), null)
    {
    }

    public PathEntity(double x, double y, IList<IGeometricEntity> entities, bool closed, ITransform transform, Guid? id = null)
        : base(GeometricEntityType.Path, id, new PointDouble(x, y), transform)
    {
        Entities = entities;
        IsClosed = closed;

        UpdateBoundary();
    }

    public bool IsClosed { get; set; }

    public IList<IGeometricEntity> Entities { get; set; }

    public override IList<PointDouble[]> ToPoints()
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return $"Path at {Location} with {Entities.Count} children";
    }

    public override void UpdateBoundary()
    {
        if (Entities.Count == 0)
        {
            Boundary = GeometryUtils.GetBoundary(0, 0, 0, 0);
            return;
        }

        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;

        foreach (var entity in Entities)
        {
            var b = entity.Boundary;

            if (b.Location.X < minX)
            {
                minX = b.Location.X;
            }

            if (b.Location.Y < minY)
            {
                minY = b.Location.Y;
            }

            if (b.Location.X + b.Size.X > maxX)
            {
                maxX = b.Location.X + b.Size.X;
            }

            if (b.Location.Y + b.Size.Y > maxY)
            {
                maxY = b.Location.Y + b.Size.Y;
            }
        }

        Boundary = GeometryUtils.GetBoundary(minX, minY, maxX, maxY);
    }

    protected override void TransformGeometry(Matrix3 m)
    {
        foreach (var entity in Entities)
        {
            entity.TransformBy(m);
        }
    }
}
