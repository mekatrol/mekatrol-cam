using Mekatrol.CAM.Core.Render;
using System.Text.Json.Serialization;

namespace Mekatrol.CAM.Core.Geometry.Entities;

public abstract class BaseEntity(GeometricEntityType type, PointDouble location, GeometryTransform transform) : IGeometricEntity
{
    protected PointDouble _minUntransformed = new();
    protected PointDouble _maxUntransformed = new();
    protected PointDouble _minTransformed = new();
    protected PointDouble _maxTransformed = new();

    // The polyline points before transforms applied
    protected List<PointDouble[]> _untransformedPolylines = [];

    // The polyline points once all transforms applied
    protected List<PointDouble[]> _transformedPolylines = [];

    public GeometricEntityType Type { get; set; } = type;

    public IReadOnlyList<PointDouble[]> UntransformedPolylines { get { return _untransformedPolylines; } }

    public IReadOnlyList<PointDouble[]> TransformedPolylines { get { return _transformedPolylines; } }

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

        foreach (var poly in _transformedPolylines)
        {
            for (var i = 0; i < poly.Length; i++)
            {
                var point = poly[i];
                point.X += translate.X;
                point.Y += translate.Y;
            }
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
        var polylines = ToPoints();
        var n = polylines.Count;

        // Create each polyline set with the number of polylines
        _untransformedPolylines = new List<PointDouble[]>(capacity: n);
        _transformedPolylines = new List<PointDouble[]>(capacity: n);

        // Compute transform with ancestor and this
        var cumulativeTransformMatrix = (Transform * ancestorCumulativeTransform).GetMatrix();

        _minUntransformed = new PointDouble(double.MaxValue, double.MaxValue);
        _maxUntransformed = new PointDouble(double.MinValue, double.MinValue);
        _minTransformed = new PointDouble(double.MaxValue, double.MaxValue);
        _maxTransformed = new PointDouble(double.MinValue, double.MinValue);

        for (var i = 0; i < n; i++)
        {
            var poly = polylines[i];

            // Allocate point array within polyline
            _untransformedPolylines.Add(new PointDouble[poly.Length]);
            _transformedPolylines.Add(new PointDouble[poly.Length]);

            for (var j = 0; j < poly.Length; j++)
            {
                var pointUntransformed = poly[j];
                var pointTransformed = pointUntransformed * cumulativeTransformMatrix;

                _minUntransformed = _minUntransformed.Min(pointUntransformed);
                _maxUntransformed = _maxUntransformed.Max(pointUntransformed);

                _minTransformed = _minTransformed.Min(pointTransformed);
                _maxTransformed = _maxTransformed.Max(pointTransformed);

                _untransformedPolylines[i][j] = pointUntransformed;
                _transformedPolylines[i][j] = pointTransformed;
            }
        }

        // Now we can calculate bounds
        UpdateBoundary();
    }
}
