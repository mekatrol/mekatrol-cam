namespace Mekatrol.CAM.Core.Geometry.Entities;

public class ViewPortEntity(double x, double y, double w, double h) : PathEntity(x, y, [], false, GeometryTransform.Identity)
{
    public PointDouble Size { get; set; } = new PointDouble(w, h);
}
