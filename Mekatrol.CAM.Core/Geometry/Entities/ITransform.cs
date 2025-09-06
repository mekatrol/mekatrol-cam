namespace Mekatrol.CAM.Core.Geometry.Entities;

public interface ITransform
{
    public Matrix3 GetMatrix();

    public PointDouble Translate { get; set; }
    public PointDouble Scale { get; set; }
    public Rotate Rotate { get; set; }
    public double SkewX { get; set; }
    public double SkewY { get; set; }
}
