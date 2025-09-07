namespace Mekatrol.CAM.Core.Geometry;

public struct PointLong
{
    public long X;
    public long Y;

    public readonly double Length => Math.Sqrt(X * X + Y * Y);

    public PointLong(long x, long y)
    {
        X = x;
        Y = y;
    }

    public PointLong(double x, double y)
    {
        X = (long)x;
        Y = (long)y;
    }

    public PointLong(PointLong pt)
    {
        X = pt.X;
        Y = pt.Y;
    }

    public PointLong(PointDouble point) : this(point.X, point.Y)
    {
    }

    public readonly bool Equals(PointLong other)
    {
        return X == other.X && Y == other.Y;
    }

    public readonly override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        return obj is PointLong point && Equals(point);
    }

    public readonly override int GetHashCode()
    {
        unchecked
        {
            return (X.GetHashCode() * 397) ^ Y.GetHashCode();
        }
    }

    public readonly override string ToString()
    {
        return $"({X}, {Y})";
    }

    public static bool operator ==(PointLong a, PointLong b)
    {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(PointLong a, PointLong b)
    {
        return !(a == b);
    }

    public static PointDouble operator +(PointLong a, PointLong b)
    {
        return new PointDouble(a.X + b.X, a.Y + b.Y);
    }

    public static PointLong operator -(PointLong a, PointLong b)
    {
        return new PointLong(a.X - b.X, a.Y - b.Y);
    }

    public static PointLong operator *(PointLong a, PointLong b)
    {
        return new PointLong(a.X * b.X, a.Y * b.Y);
    }

    public static PointLong operator *(PointLong a, long b)
    {
        return new PointLong(a.X * b, a.Y * b);
    }

    public static PointLong operator *(PointLong a, double b)
    {
        return new PointLong(a.X * b, a.Y * b);
    }

    public static PointLong operator /(PointLong a, PointLong b)
    {
        return new PointLong(a.X / b.X, a.Y / b.Y);
    }

    public static PointLong operator /(PointLong a, long b)
    {
        var scale = 1.0 / b;
        return a * scale;
    }

    public static PointLong operator /(PointLong a, double b)
    {
        var scale = 1.0 / b;
        return a * scale;
    }
}
