using Mekatrol.CAM.Core.Render;
using System.Text.Json.Serialization;

namespace Mekatrol.CAM.Core.Geometry.Entities;

public abstract class BaseEntity : IGeometricEntity
{
    public BaseEntity(GeometricEntityType type, Guid? id, PointDouble location, ITransform transform)
    {
        Type = type;
        Id = (id != null && id != Guid.Empty) ? id.Value : Guid.NewGuid();
        Location = new PointDouble(location.X, location.Y);
        Transform = transform;
    }

    public GeometricEntityType Type { get; set; }

    public Guid Id { get; set; }

    public virtual PointDouble Location { get; set; }

    [JsonIgnore]
    public IBoundary Boundary { get; set; } = new Boundary(new PointDouble(0, 0), new PointDouble(0, 0));

    public ITransform Transform { get; set; }

    public bool BoundsContains(PointDouble point)
    {
        var topLeft = Boundary.Location;
        var bottomRight = Boundary.BottomRight;

        return
            point.X >= topLeft.X &&
            point.Y >= topLeft.Y &&
            point.X <= bottomRight.X &&
            point.Y <= bottomRight.Y;
    }

    public virtual bool Contains(PointDouble point)
    {
        var topLeft = Boundary.Location;
        var bottomRight = Boundary.BottomRight;

        var result = GeometryUtils.PointInPolygon(
            point,
            [
                new[]
                {
                    new PointDouble(topLeft.X, topLeft.Y) ,
                    new PointDouble(topLeft.X, bottomRight.Y),
                    new PointDouble(bottomRight.X, bottomRight.Y),
                    new PointDouble(bottomRight.X, topLeft.Y),
                }.ToArray()
            ]);

        return IsInPolygonResult(result);
    }

    public virtual bool ContainsWithBoundaryCheck(PointDouble point)
    {
        // Do quick test of boundary contains then only do more 
        // intensive check if bounds contains point
        return BoundsContains(point) && Contains(point);
    }

    public (PointDouble min, PointDouble max) GetMinMax()
    {
        return (Boundary.Location, Location + Boundary.Size);
    }

    public virtual void TransformBy(Matrix3 m)
    {
        var mTransformed = Transform.GetMatrix() * m;

        Transform.Scale = mTransformed.GetScale();
        Transform.Rotate = new GeometryRotate(GeometryUtils.RadiansToDegrees(mTransformed.GetRotation()), 0, 0);
        Transform.Translate = mTransformed.GetTranslation();

        Location *= m;
        TransformGeometry(m);

        UpdateBoundary();
    }

    public abstract void UpdateBoundary();

    protected abstract void TransformGeometry(Matrix3 m);

    protected static bool IsInPolygonResult(PointInPolgygonResult result)
    {
        return result switch
        {
            PointInPolgygonResult.Inside => true,
            PointInPolgygonResult.Vertex => true,
            PointInPolgygonResult.Edge => true,
            PointInPolgygonResult.Outside => false,
            _ => false,
        };
    }

    public abstract IList<PointDouble[]> ToPoints();

    public virtual IList<PointDouble> GetRotatedBoundary()
    {
        var boundary = OffsetBoundary(Boundary);

        var points = boundary.ToPoints();
        points = GeometryUtils.RotatePoints(points, GeometryUtils.DegreesToRadians(Transform.Rotate?.Angle ?? 0));
        return points;
    }

    protected virtual IBoundary OffsetBoundary(IBoundary boundary)
    {
        var offset = 3;
        boundary = new Boundary(boundary.Location + new PointDouble(-offset, -offset), boundary.Size + new PointDouble(offset * 2, offset * 2));
        return boundary;
    }
}
