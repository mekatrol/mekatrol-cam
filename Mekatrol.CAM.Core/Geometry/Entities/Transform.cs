using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry.Entities;

internal class Transform : ITransform
{
    public Transform()
    {
        Rotate = new Rotate(0.0, 0.0, 0.0);
        Scale = new PointDouble(1.0, 1.0);
        Translate = new PointDouble(0.0, 0.0);
        SkewX = 0.0;
        SkewY = 0.0;
    }

    public PointDouble Translate { get; set; }
    public PointDouble Scale { get; set; }
    public Rotate Rotate { get; set; }
    public double SkewX { get; set; }
    public double SkewY { get; set; }

    public Matrix3 GetMatrix()
    {
        var m = Matrix3.Identity;

        m *= Matrix3.CreateScale(Scale);

        m *= Matrix3.CreateRotation(GeometryUtils.DegreesToRadians(Rotate.Angle));

        m *= Matrix3.CreateTranslate(Translate);

        return m;
    }
}
