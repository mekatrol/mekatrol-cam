namespace Mekatrol.CAM.Core.Geometry;

public struct PointDouble
{
    public double X { get; set; }
    public double Y { get; set; }

    public readonly double Length => Math.Sqrt(X * X + Y * Y);

    public PointDouble(double x, double y)
    {
        X = x;
        Y = y;
    }

    public PointDouble(PointDouble point)
    {
        X = point.X;
        Y = point.Y;
    }

    public readonly PointDouble Normalized()
    {
        var scale = 1.0 / Length;
        return new PointDouble(X * scale, Y * scale);
    }

    public readonly PointDouble Abs()
    {
        var p = new PointDouble(X, Y);

        if (p.X < 0)
        {
            p.X = -1;
        }

        if (p.Y < 0)
        {
            p.Y = -1;
        }

        return p;
    }

    public readonly PointDouble Min(PointDouble other)
    {
        return new PointDouble(Math.Min(X, other.X), Math.Min(Y, other.Y));
    }

    public readonly PointDouble Max(PointDouble other)
    {
        return new PointDouble(Math.Max(X, other.X), Math.Max(Y, other.Y));
    }

    public readonly bool Equals(PointDouble other)
    {
        return
            Math.Abs(X - other.X) < GeometryConstants.Tolerance &&
            Math.Abs(Y - other.Y) < GeometryConstants.Tolerance;
    }

    public override readonly bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        return obj is PointDouble point && Equals(point);
    }

    public override readonly string ToString()
    {
        return $"{{X={X},Y={Y})}}";
    }

    public override readonly int GetHashCode()
    {
        unchecked
        {
            return X.GetHashCode() * 397 ^ Y.GetHashCode();
        }
    }

    public static bool operator ==(PointDouble a, PointDouble b)
    {
        return
            Math.Abs(a.X - b.X) < GeometryConstants.Tolerance &&
            Math.Abs(a.Y - b.Y) < GeometryConstants.Tolerance;
    }

    public static bool operator !=(PointDouble a, PointDouble b)
    {
        return !(a == b);
    }

    public static PointDouble operator +(PointDouble a, PointDouble b)
    {
        return new PointDouble(a.X + b.X, a.Y + b.Y);
    }

    public static PointDouble operator +(PointDouble a, double b)
    {
        return new PointDouble(a.X + b, a.Y + b);
    }

    public static PointDouble operator -(PointDouble a, PointDouble b)
    {
        return new PointDouble(a.X - b.X, a.Y - b.Y);
    }

    public static PointDouble operator -(PointDouble a, double b)
    {
        return new PointDouble(a.X - b, a.Y - b);
    }

    public static PointDouble operator *(PointDouble a, PointDouble b)
    {
        return new PointDouble(a.X * b.X, a.Y * b.Y);
    }

    public static PointDouble operator *(PointDouble a, long b)
    {
        return new PointDouble(a.X * b, a.Y * b);
    }

    public static PointDouble operator *(PointDouble a, double b)
    {
        return new PointDouble(a.X * b, a.Y * b);
    }

    public static PointDouble operator *(double a, PointDouble b)
    {
        return new PointDouble(a * b.X, a * b.Y);
    }

    public static PointDouble operator /(PointDouble a, PointDouble b)
    {
        return new PointDouble(a.X / b.X, a.Y / b.Y);
    }

    public static PointDouble operator /(PointDouble a, long b)
    {
        var scale = 1.0 / b;
        return a * scale;
    }

    public static PointDouble operator /(PointDouble a, double b)
    {
        var scale = 1.0 / b;
        return a * scale;
    }

    public static PointDouble operator /(double a, PointDouble b)
    {
        var scale = 1.0 / b;
        return a * scale;
    }

    public static PointDouble operator -(PointDouble p)
    {
        return new PointDouble(-p.X, -p.Y);
    }
}
