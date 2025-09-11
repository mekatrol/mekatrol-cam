using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry.Entities;

public class PathEntity(double x, double y, IList<IGeometricEntity> entities, bool closed, GeometryTransform transform, Guid? id = null)
    : BaseEntity(GeometricEntityType.Path, id, new PointDouble(x, y), transform), IGeometricPathEntity
{
    public PathEntity()
        : this(0, 0, [], false, new GeometryTransform(), null)
    {
    }

    public bool IsClosed { get; set; } = closed;

    public IList<IGeometricEntity> Entities { get; set; } = entities;

    public override IReadOnlyList<PointDouble[]> ToPoints()
    {
        return Entities
            .SelectMany(e => e.ToPoints())
            .ToList();
    }

    /// <summary>
    /// Shift location and transformed points by the translation offset
    /// </summary>
    public override void TranslateLocation(PointDouble translate)
    {
        Location += translate;

        foreach (var entity in Entities)
        {
            entity.TranslateLocation(translate);
        }
    }

    public override string ToString()
    {
        return $"Path at {Location} with {Entities.Count} children";
    }

    public override void UpdateBoundary()
    {
        if (Entities.Count == 0)
        {
            _minUntransformed = new PointDouble(0, 0);
            _maxUntransformed = new PointDouble(0, 0);
            _minTransformed = new PointDouble(0, 0);
            _maxTransformed = new PointDouble(0, 0);

            BoundaryUntransformed = GeometryUtils.GetBoundary(0, 0, 0, 0);
            BoundaryTransformed = GeometryUtils.GetBoundary(0, 0, 0, 0);
        }

        foreach (var entity in Entities)
        {
            entity.UpdateBoundary();

            _minUntransformed = _minUntransformed.Min(entity.MinUntransformed);
            _maxUntransformed = _maxUntransformed.Max(entity.MaxUntransformed);
            _minTransformed = _minTransformed.Min(entity.MinTransformed);
            _maxTransformed = _maxTransformed.Max(entity.MaxTransformed);
        }

        base.UpdateBoundary();
    }

    public override void InitializeState(GeometryTransform ancestorCumulativeTransform)
    {
        // Calculate transform with ancestor and this
        var cumulativeTransform = Transform * ancestorCumulativeTransform;

        foreach (var entity in Entities)
        {
            // Transform children
            entity.InitializeState(cumulativeTransform);
        }

        UpdateBoundary();
    }
}
