namespace Mekatrol.CAM.Core.Geometry.Entities;

public class ViewEntity(PointDouble location, PointDouble size, IList<IGeometricEntity> entities, GeometryTransform transform)
    : PathEntity(location.X, location.Y, entities, false, transform)
{
    public PointDouble Size { get; set; } = size;
}
