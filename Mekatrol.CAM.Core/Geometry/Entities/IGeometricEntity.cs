namespace Mekatrol.CAM.Core.Geometry.Entities;

public interface IGeometricEntity
{
    GeometricEntityType Type { get; }

    Guid Id { get; }

    PointDouble Location { get; }

    IReadOnlyList<PointDouble> UntransformedPoints { get; }

    IReadOnlyList<PointDouble> TransformedPoints { get; }

    PointDouble MinUntransformed { get; }

    PointDouble MaxUntransformed { get; }

    PointDouble MinTransformed { get; }

    PointDouble MaxTransformed { get; }

    IBoundary BoundaryUntransformed { get; }

    IBoundary BoundaryTransformed { get; }

    GeometryTransform Transform { get; }

    void UpdateBoundary();

    IReadOnlyList<PointDouble[]> ToPoints();

    /// <summary>
    /// Generate untransformed and transformed points by applying the entity transform by the parent transform, also update boundaries.
    /// </summary>
    void InitializeState(GeometryTransform ancestorCumulativeTransform);

    /// <summary>
    /// Shift location and transformed points by the translation offset
    /// </summary>
    void TranslateLocation(PointDouble translate);
}
