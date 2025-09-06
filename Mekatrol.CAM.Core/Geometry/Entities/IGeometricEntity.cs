namespace Mekatrol.CAM.Core.Geometry.Entities;

public interface IGeometricEntity
{
    GeometricEntityType Type { get; }

    Guid Id { get; }

    PointDouble Location { get; }

    IBoundary Boundary { get; }

    GeometryTransform Transform { get; }

    void UpdateBoundary();

    void TransformBy(Matrix3 m);

    (PointDouble min, PointDouble max) GetMinMax();

    IList<PointDouble[]> ToPoints();

    IList<PointDouble> GetRotatedBoundary();

    bool BoundsContains(PointDouble point);

    bool Contains(PointDouble point);

    bool ContainsWithBoundaryCheck(PointDouble point);
}
