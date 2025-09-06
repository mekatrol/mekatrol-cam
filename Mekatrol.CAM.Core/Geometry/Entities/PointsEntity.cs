using Mekatrol.CAM.Core.Clipping;
using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry.Entities;

public abstract class PointsEntity : BaseEntity
{
    protected List<PointDouble> _untransformedPoints = [];
    protected List<PointDouble> _transformedPoints = [];
    protected IList<PointDouble> _rotatedBoundary = [];

    protected PointsEntity(GeometricEntityType type, Guid? id, PointDouble location, GeometryTransform transform)
        : base(type, id, location, transform)
    {
        UpdateTransformedPoints();
        UpdateBoundary();
    }

    public IList<PointDouble> Points
    {
        get
        {
            return _transformedPoints;
        }
    }

    public override void UpdateBoundary()
    {
        Boundary = GeometryUtils.GetBoundary(_transformedPoints);
        _rotatedBoundary = ClippingHelper.GetBoundary(_transformedPoints, 1);
    }

    public override bool Contains(PointDouble point)
    {
        var result = GeometryUtils.PointInPolygon(point, ToPoints());
        return IsInPolygonResult(result);
    }

    public override IList<PointDouble> GetRotatedBoundary()
    {
        return _rotatedBoundary;
    }

    protected override void TransformGeometry(Matrix3 m)
    {
        UpdateTransformedPoints();
    }

    protected void SetUntransformedPoints(IList<PointDouble> points)
    {
        _untransformedPoints = points.ToList();
        UpdateTransformedPoints();
        UpdateBoundary();
    }

    protected void UpdateTransformedPoints()
    {
        var m = Transform.GetMatrix();
        _transformedPoints = _untransformedPoints.Select(p => p * m).ToList();
        UpdateBoundary();
    }
}
