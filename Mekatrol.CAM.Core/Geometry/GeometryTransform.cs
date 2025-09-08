using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry;

public class GeometryTransform
{
    public GeometryTransform()
    {
        Rotate = new GeometryRotate(0.0, 0.0, 0.0);
        Scale = new PointDouble(1.0, 1.0);
        Translate = new PointDouble(0.0, 0.0);
        SkewX = 0.0;
        SkewY = 0.0;
    }

    public static GeometryTransform Identity => new();

    public PointDouble Translate { get; set; }

    public PointDouble Scale { get; set; }

    public GeometryRotate Rotate { get; set; }

    public double SkewX { get; set; }

    public double SkewY { get; set; }

    /// <summary>
    /// Compose as Scale → Rotate → Translate.
    /// </summary>
    public Matrix3 GetMatrix()
    {
        var m = Matrix3.Identity;
        m *= Matrix3.CreateScale(Scale);
        var p = new PointDouble(Rotate.X, Rotate.Y);
        m *= Matrix3.CreateTranslate(new PointDouble(-p.X, -p.Y));
        m *= Matrix3.CreateRotation(GeometryUtils.DegreesToRadians(Rotate.Angle));
        m *= Matrix3.CreateTranslate(p);
        m *= Matrix3.CreateTranslate(Translate);
        return m;
    }

    /// <summary>
    /// Compose two transforms. Order: first <paramref name="left"/>, then <paramref name="right"/>.
    /// Equivalent to left.GetMatrix() * right.GetMatrix().
    /// </summary>
    public static GeometryTransform operator *(GeometryTransform left, GeometryTransform right)
        => FromMatrix(left.GetMatrix() * right.GetMatrix());

    /// <summary>
    /// Pre-multiply by a matrix. Resulting matrix = <paramref name="m"/> * t.
    /// </summary>
    public static Matrix3 operator *(Matrix3 m, GeometryTransform t) => m * t.GetMatrix();

    /// <summary>
    /// Post-multiply by a matrix. Resulting matrix = t * <paramref name="m"/>.
    /// </summary>
    public static Matrix3 operator *(GeometryTransform t, Matrix3 m) => t.GetMatrix() * m;

    /// <summary>
    /// Helper: rebuild a GeometryTransform from a Matrix3 using available decomposers.
    /// Skips skew since GetMatrix doesn't encode it.
    /// </summary>
    public static GeometryTransform FromMatrix(Matrix3 m)
    {
        return new GeometryTransform
        {
            Translate = m.GetTranslation(),
            Scale = m.GetScale(),
            Rotate = new GeometryRotate(GeometryUtils.RadiansToDegrees(m.GetRotation()), 0.0, 0.0),
            SkewX = 0.0,
            SkewY = 0.0
        };
    }
}
