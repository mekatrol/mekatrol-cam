namespace Mekatrol.CAM.Core.Geometry.Entities;

public class GeometryRotate(double a, double x, double y)
{
    public static readonly GeometryRotate Zero = new(0, 0, 0);

    public GeometryRotate()
        : this(0, 0, 0)
    {
    }

    public double Angle { get; set; } = a;

    public double X { get; set; } = x;

    public double Y { get; set; } = y;

    public static GeometryRotate operator +(GeometryRotate left, GeometryRotate right)
    {
        return new GeometryRotate(left.Angle + right.Angle, left.X + right.X, left.Y + right.Y);
    }

    public static GeometryRotate operator -(GeometryRotate left, GeometryRotate right)
    {
        return new GeometryRotate(left.Angle - right.Angle, left.X - right.X, left.Y - right.Y);
    }

    public static GeometryRotate operator *(GeometryRotate left, GeometryRotate right)
    {
        return new GeometryRotate(left.Angle * right.Angle, left.X * right.X, left.Y * right.Y);
    }

    public static GeometryRotate operator /(GeometryRotate left, GeometryRotate right)
    {
        return new GeometryRotate(left.Angle / right.Angle, left.X / right.X, left.Y / right.Y);
    }

    public static GeometryRotate operator *(GeometryRotate left, double right)
    {
        return new GeometryRotate(left.Angle * right, left.X * right, left.Y * right);
    }

    public static GeometryRotate operator /(GeometryRotate left, double right)
    {
        return new GeometryRotate(left.Angle / right, left.X / right, left.Y / right);
    }

    public static GeometryRotate operator *(double left, GeometryRotate right)
    {
        return new GeometryRotate(left * right.Angle, left * right.X, left * right.Y);
    }

    public static GeometryRotate operator /(double left, GeometryRotate right)
    {
        return new GeometryRotate(left / right.Angle, left / right.X, left / right.Y);
    }

    public static bool operator ==(GeometryRotate? left, GeometryRotate? right)
    {
        // Object reference equals comparison
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null && right is null)
        {
            return true;
        }

        if (left is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(GeometryRotate? left, GeometryRotate? right)
    {
        return !(left == right);
    }

    public static GeometryRotate operator -(GeometryRotate v)
    {
        return new GeometryRotate(-v.Angle, -v.X, -v.Y);
    }

    public override string ToString()
    {
        return $"{{A={Angle},X={X},Y={Y})}}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (GetType() != obj.GetType())
        {
            return false;
        }

        var other = (GeometryRotate)obj;

        return
            Math.Abs(Angle - other.Angle) < GeometryConstants.Tolerance &&
            Math.Abs(X - other.X) < GeometryConstants.Tolerance &&
            Math.Abs(Y - other.Y) < GeometryConstants.Tolerance;
    }

    public override int GetHashCode()
    {
        return ((int)Angle) ^ ((int)X) ^ ((int)Y);
    }
}
