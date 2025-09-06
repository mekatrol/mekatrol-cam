namespace Mekatrol.CAM.Core.Geometry.Entities;

public class Rotate(double a, double x, double y)
{
    public static readonly Rotate Zero = new(0, 0, 0);

    public Rotate()
        : this(0, 0, 0)
    {
    }

    public double Angle { get; set; } = a;

    public double X { get; set; } = x;

    public double Y { get; set; } = y;

    public static Rotate operator +(Rotate left, Rotate right)
    {
        return new Rotate(left.Angle + right.Angle, left.X + right.X, left.Y + right.Y);
    }

    public static Rotate operator -(Rotate left, Rotate right)
    {
        return new Rotate(left.Angle - right.Angle, left.X - right.X, left.Y - right.Y);
    }

    public static Rotate operator *(Rotate left, Rotate right)
    {
        return new Rotate(left.Angle * right.Angle, left.X * right.X, left.Y * right.Y);
    }

    public static Rotate operator /(Rotate left, Rotate right)
    {
        return new Rotate(left.Angle / right.Angle, left.X / right.X, left.Y / right.Y);
    }

    public static Rotate operator *(Rotate left, double right)
    {
        return new Rotate(left.Angle * right, left.X * right, left.Y * right);
    }

    public static Rotate operator /(Rotate left, double right)
    {
        return new Rotate(left.Angle / right, left.X / right, left.Y / right);
    }

    public static Rotate operator *(double left, Rotate right)
    {
        return new Rotate(left * right.Angle, left * right.X, left * right.Y);
    }

    public static Rotate operator /(double left, Rotate right)
    {
        return new Rotate(left / right.Angle, left / right.X, left / right.Y);
    }

    public static bool operator ==(Rotate? left, Rotate? right)
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

    public static bool operator !=(Rotate? left, Rotate? right)
    {
        return !(left == right);
    }

    public static Rotate operator -(Rotate v)
    {
        return new Rotate(-v.Angle, -v.X, -v.Y);
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

        var other = (Rotate)obj;

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
