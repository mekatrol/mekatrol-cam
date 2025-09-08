using Mekatrol.CAM.Core.Render;
using System.Text.Json.Serialization;

namespace Mekatrol.CAM.Core.Geometry.Entities;

public abstract class BaseEntity(GeometricEntityType type, Guid? id, PointDouble location, GeometryTransform transform) : IGeometricEntity
{
    protected PointDouble _minUntransformed = new();
    protected PointDouble _maxUntransformed = new();
    protected PointDouble _minTransformed = new();
    protected PointDouble _maxTransformed = new();

    // The points before transforms applied
    protected List<PointDouble> _untransformedPoints = [];

    // The points once all transforms applied
    protected List<PointDouble> _transformedPoints = [];

    public GeometricEntityType Type { get; set; } = type;

    public Guid Id { get; set; } = (id != null && id != Guid.Empty) ? id.Value : Guid.NewGuid();

    public IReadOnlyList<PointDouble> UntransformedPoints { get { return _untransformedPoints; } }

    public IReadOnlyList<PointDouble> TransformedPoints { get { return _transformedPoints; } }

    public virtual PointDouble Location { get; set; } = new PointDouble(location.X, location.Y);

    public PointDouble MinUntransformed => _minUntransformed;

    public PointDouble MaxUntransformed => _maxUntransformed;

    public PointDouble MinTransformed => _minTransformed;

    public PointDouble MaxTransformed => _maxTransformed;

    [JsonIgnore]
    public IBoundary BoundaryUntransformed { get; set; } = new Boundary(new PointDouble(0, 0), new PointDouble(0, 0));

    [JsonIgnore]
    public IBoundary BoundaryTransformed { get; set; } = new Boundary(new PointDouble(0, 0), new PointDouble(0, 0));

    public GeometryTransform Transform { get; set; } = transform;

    /// <summary>
    /// Shift location and transformed points by the translation offset
    /// </summary>
    public virtual void TranslateLocation(PointDouble translate)
    {
        Location += translate;

        for (var i = 0; i < _transformedPoints.Count; i++)
        {
            var point = _transformedPoints[i];
            point.X += translate.X;
            point.Y += translate.Y;
        }
    }

    public virtual void UpdateBoundary()
    {
        BoundaryUntransformed = GeometryUtils.GetBoundary(_minUntransformed.X, _minUntransformed.Y, _maxUntransformed.X, _maxUntransformed.Y);
        BoundaryTransformed = GeometryUtils.GetBoundary(_minTransformed.X, _minTransformed.Y, _maxTransformed.X, _maxTransformed.Y);
    }

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

    public abstract IReadOnlyList<PointDouble[]> ToPoints();

    public virtual void InitializeState(GeometryTransform ancestorCumulativeTransform)
    {
        var points = ToPoints();
        _transformedPoints = [];

        // Calculate transform with ancestor and this
        var cumulativeTransformMatrix = (Transform * ancestorCumulativeTransform).GetMatrix();

        _minUntransformed = new PointDouble(double.MaxValue, double.MaxValue);
        _maxUntransformed = new PointDouble(double.MinValue, double.MinValue);
        _minTransformed = new PointDouble(double.MaxValue, double.MaxValue);
        _maxTransformed = new PointDouble(double.MinValue, double.MinValue);

        foreach(var poly in points)
        {
            _untransformedPoints = poly.ToList();

            foreach (var point in poly)
            {
                var transformed = point * cumulativeTransformMatrix;

                _minUntransformed = _minUntransformed.Min(point);
                _maxUntransformed = _maxUntransformed.Max(point);

                _minTransformed = _minTransformed.Min(point);
                _maxTransformed = _maxTransformed.Max(point);

                _transformedPoints.Add(transformed);
            }
        }

        // Now we can calculate bounds
        UpdateBoundary();
    }
}
