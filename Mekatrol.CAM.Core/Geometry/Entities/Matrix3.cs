namespace Mekatrol.CAM.Core.Geometry.Entities;

public class Matrix3
{
    private const int R0C0 = 0;
    private const int R0C1 = 1;
    private const int R0C2 = 2;
    private const int R1C0 = 3;
    private const int R1C1 = 4;
    private const int R1C2 = 5;
    private const int R2C0 = 6;
    private const int R2C1 = 7;
    private const int R2C2 = 8;

    public static readonly Matrix3 Identity = new();

    public Matrix3()
    {
        // Create the identity matrix in default constructor
        Data =
        [
        //  X, Y, Z
            1, 0, 0,
            0, 1, 0,
            0, 0, 1,
        ];
    }

    public Matrix3(double[] m)
    {
        if (m.Length != 9)
        {
            throw new(nameof(m));
        }

        Data = m;
    }

    public double[] Data { get; }

    public double this[int index]
    {
        get { return Data[index]; }
        set { Data[index] = value; }
    }

    public double GetDeterminant()
    {
        // m [Row] [Col]
        var m00 = Data[0];
        var m01 = Data[1];
        var m02 = Data[2];
        var m10 = Data[3];
        var m11 = Data[4];
        var m12 = Data[5];
        var m20 = Data[6];
        var m21 = Data[7];
        var m22 = Data[8];

        return
            (m00 * m11 * m22) +
            (m01 * m12 * m20) +
            (m02 * m10 * m21) -
            (m02 * m11 * m20) -
            (m00 * m12 * m21) -
            (m01 * m10 * m22);
    }

    public Matrix3 Normalized()
    {
        var m = this;
        m.Normalize();
        return m;
    }

    public void Normalize()
    {
        var determinant = GetDeterminant();

        if (Math.Abs(determinant) > 0.0000000001)
        {
            Data[0] /= determinant;
            Data[1] /= determinant;
            Data[2] /= determinant;
            Data[3] /= determinant;
            Data[4] /= determinant;
            Data[5] /= determinant;
            Data[6] /= determinant;
            Data[7] /= determinant;
            Data[8] /= determinant;
        }
    }

    public double GetRotation()
    {
        var m = Normalized();
        var radians = Math.Atan2(m.Data[3], m.Data[0]);
        return radians;
    }

    public PointDouble GetScale()
    {
        var row0Length = new PointDouble(Data[0], Data[1]).Length;
        var row1Length = new PointDouble(Data[3], Data[4]).Length;
        return new PointDouble(row0Length, row1Length);
    }

    public PointDouble GetTranslation()
    {
        return new PointDouble(Data[2], Data[5]);
    }

    public static Matrix3 CreateRotation(double angle)
    {
        var cosTheta = Math.Cos(angle);
        var sinTheta = Math.Sin(angle);

        // Create identity with rotation such that:
        // ┌  cosφ sinφ 0 ┐
        // | -sinφ cosφ 0 |
        // └  0    0    1 ┘
        var m = new Matrix3(
        [
            cosTheta, -sinTheta, 0,
            sinTheta,  cosTheta, 0,
            0,         0,        1
        ]);

        return m;
    }

    public static Matrix3 CreateScale(PointDouble scale)
    {
        // Create identity with scale such that:
        // ┌ scaleX 0      0 ┐
        // | 0      scaleY 0 |
        // └ 0      0      1 ┘
        var m = new Matrix3(
        [
            scale.X, 0,       0,
            0,       scale.Y, 0,
            0,       0,       1
        ]);

        return m;
    }

    public static Matrix3 CreateScale(double scale)
    {
        // Create identity with scale such that:
        // ┌ scaleX 0      0 ┐
        // | 0      scaleY 0 |
        // └ 0      0      1 ┘
        var m = new Matrix3(
        [
            scale, 0,     0,
            0,     scale, 0,
            0,     0,     1
        ]);

        return m;
    }

    public static Matrix3 CreateTranslate(PointDouble translation)
    {
        // Create identity with rotation such that:
        // ┌ 1 0 translateX ┐
        // | 0 1 translateY |
        // └ 0 0 1          ┘
        var m = new Matrix3(
        [
            1, 0, translation.X,
            0, 1, translation.Y,
            0, 0, 1
        ]);

        return m;
    }

    /// <summary>
    /// Create a combination of scale, rotation (angle) and translation.
    /// Note that the scale needs to be the same for both X and Y so that the
    /// scale * angle matrix operation is commutative (if the scale was different
    /// in X,Y then the order of operations becomes important).
    /// The order of operations performed are:
    /// 1. Scale
    /// 2. Rotate
    /// 3. Translate
    /// </summary>
    /// <param name="scale">The amount to scale the X and Y components.</param>
    /// <param name="rotationAngle">The amount (in radians) to rotate.</param>
    /// <param name="translation">The amount to translate in X,Y</param>
    /// <returns>A transform matrix for the specified parameters</returns>
    public static Matrix3 CreateTransform(double scale, double rotationAngle, PointDouble translation)
    {
        return CreateScale(new PointDouble(scale, scale)) * CreateRotation(rotationAngle) * CreateTranslate(translation);
    }

    public static PointDouble operator *(PointDouble left, Matrix3 right)
    {
        // You can't multiply a 1x2 matrix by a 3x3 matrix, however we can assume the 1x2 matrix is
        // a 1x3 matix such that (x, y) => (x, y, z) where z == 1, ie (x, y, 1)

        // So multiply the 1x3 matrix ┌ x ┐ by the 3x3 matrix ┌ R0C0 R0C1 R0C2 ┐
        //                            | y |                   | R1C0 R1C1 R1C2 |
        //                            └ z ┘                   └ R2C0 R2C1 R2C2 ┘
        // Noting that multiplying the z is not neccessary for 2 dimensions, 
        // however the right[R0C2] and right[R1C2] components still need to be added for the 
        // point to be correctly translated (z = 1 is implied)

        return new PointDouble(
            left.X * right[R0C0] + left.Y * right[R0C1] + right[R0C2],
            left.X * right[R1C0] + left.Y * right[R1C1] + right[R1C2]);
    }

    public static Matrix3 operator *(Matrix3 left, Matrix3 right)
    {
        // Multiply the 3x3 matrix left ┌ R0C0 R0C1 R0C2 ┐ by the 3x3 matrix right ┌ R0C0 R0C1 R0C2 ┐
        //                              | R1C0 R1C1 R1C2 |                         | R1C0 R1C1 R1C2 |
        //                              └ R2C0 R2C1 R2C2 ┘                         └ R2C0 R2C1 R2C2 ┘

        return new Matrix3(
        [
            right.Data[R0C0] * left.Data[R0C0] + right.Data[R0C1] * left.Data[R1C0] + right.Data[R0C2] * left.Data[R2C0],
            right.Data[R0C0] * left.Data[R0C1] + right.Data[R0C1] * left.Data[R1C1] + right.Data[R0C2] * left.Data[R2C1],
            right.Data[R0C0] * left.Data[R0C2] + right.Data[R0C1] * left.Data[R1C2] + right.Data[R0C2] * left.Data[R2C2],
            right.Data[R1C0] * left.Data[R0C0] + right.Data[R1C1] * left.Data[R1C0] + right.Data[R1C2] * left.Data[R2C0],
            right.Data[R1C0] * left.Data[R0C1] + right.Data[R1C1] * left.Data[R1C1] + right.Data[R1C2] * left.Data[R2C1],
            right.Data[R1C0] * left.Data[R0C2] + right.Data[R1C1] * left.Data[R1C2] + right.Data[R1C2] * left.Data[R2C2],
            right.Data[R2C0] * left.Data[R0C0] + right.Data[R2C1] * left.Data[R1C0] + right.Data[R2C2] * left.Data[R2C0],
            right.Data[R2C0] * left.Data[R0C1] + right.Data[R2C1] * left.Data[R1C1] + right.Data[R2C2] * left.Data[R2C1],
            right.Data[R2C0] * left.Data[R0C2] + right.Data[R2C1] * left.Data[R1C2] + right.Data[R2C2] * left.Data[R2C2]
        ]);
    }

}
