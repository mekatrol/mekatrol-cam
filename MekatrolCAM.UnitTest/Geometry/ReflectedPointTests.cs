using Mekatrol.CAM.Core.Geometry;
using Mekatrol.CAM.Core.Render;

namespace MekatrolCAM.UnitTest.Geometry;

[TestClass]
public class ReflectedPointTests
{
    [TestMethod]
    public void AllZero()
    {
        var l1 = new PointDouble(0, 0);
        var l2 = new PointDouble(0, 0);
        var p1 = new PointDouble(0, 0);
        var p2 = GeometryUtils.GetReflectedPoint(p1, l1, l2);
        AssertEx.WithinTolerance(0, p2.X);
        AssertEx.WithinTolerance(0, p2.Y);
    }

    [TestMethod]
    public void Horizontal()
    {
        // All positive
        var l1 = new PointDouble(1, 1);
        var l2 = new PointDouble(3, 1);
        var p1 = new PointDouble(2, 2);
        var p2 = GeometryUtils.GetReflectedPoint(p1, l1, l2);
        AssertEx.WithinTolerance(2, p2.X);
        AssertEx.WithinTolerance(0, p2.Y);

        // Line negative Y
        l1 = new PointDouble(1, -1);
        l2 = new PointDouble(3, -1);
        p1 = new PointDouble(2, 2);
        p2 = GeometryUtils.GetReflectedPoint(p1, l1, l2);
        AssertEx.WithinTolerance(2, p2.X);
        AssertEx.WithinTolerance(-4, p2.Y);

        // Line and point negative Y
        l1 = new PointDouble(1, -1);
        l2 = new PointDouble(3, -1);
        p1 = new PointDouble(2, -3);
        p2 = GeometryUtils.GetReflectedPoint(p1, l1, l2);
        AssertEx.WithinTolerance(2, p2.X);
        AssertEx.WithinTolerance(1, p2.Y);

        // Point negative
        l1 = new PointDouble(1, 1);
        l2 = new PointDouble(3, 1);
        p1 = new PointDouble(2, -2);
        p2 = GeometryUtils.GetReflectedPoint(p1, l1, l2);
        AssertEx.WithinTolerance(2, p2.X);
        AssertEx.WithinTolerance(4, p2.Y);
    }

    [TestMethod]
    public void Vertical()
    {
        // All positive
        var l1 = new PointDouble(1, 1);
        var l2 = new PointDouble(1, 3);
        var p1 = new PointDouble(0, 2);
        var p2 = GeometryUtils.GetReflectedPoint(p1, l1, l2);
        AssertEx.WithinTolerance(2, p2.X);
        AssertEx.WithinTolerance(2, p2.Y);

        // Line negative X
        l1 = new PointDouble(-1, 1);
        l2 = new PointDouble(-1, 3);
        p1 = new PointDouble(2, 2);
        p2 = GeometryUtils.GetReflectedPoint(p1, l1, l2);
        AssertEx.WithinTolerance(-4, p2.X);
        AssertEx.WithinTolerance(2, p2.Y);

        // Line and point negative Y
        l1 = new PointDouble(-1, 1);
        l2 = new PointDouble(-1, 3);
        p1 = new PointDouble(-3, 2);
        p2 = GeometryUtils.GetReflectedPoint(p1, l1, l2);
        AssertEx.WithinTolerance(1, p2.X);
        AssertEx.WithinTolerance(2, p2.Y);

        // Point negative
        l1 = new PointDouble(1, 1);
        l2 = new PointDouble(1, 3);
        p1 = new PointDouble(-3, 2);
        p2 = GeometryUtils.GetReflectedPoint(p1, l1, l2);
        AssertEx.WithinTolerance(5, p2.X);
        AssertEx.WithinTolerance(2, p2.Y);
    }

    [TestMethod]
    public void PositiveValuesPositiveSlope()
    {
        var l1 = new PointDouble(2, 3);
        var l2 = new PointDouble(12, 12);
        var p1 = new PointDouble(4, 7);
        var p2 = GeometryUtils.GetReflectedPoint(p1, l1, l2);
        AssertEx.WithinTolerance(6.1878453038674, p2.X);
        AssertEx.WithinTolerance(4.5690607734807, p2.Y);
    }

    [TestMethod]
    public void PositiveValuesNegativeSlope()
    {
        var l1 = new PointDouble(2, 30);
        var l2 = new PointDouble(12, 12);
        var p1 = new PointDouble(4, 7);
        var p2 = GeometryUtils.GetReflectedPoint(p1, l1, l2);
        AssertEx.WithinTolerance(20.4716981132075, p2.X);
        AssertEx.WithinTolerance(16.1509433962264, p2.Y);
    }

    [TestMethod]
    public void NegativeValuesPositiveSlope()
    {
        var l1 = new PointDouble(-2, -3);
        var l2 = new PointDouble(-12, -12);
        var p1 = new PointDouble(-4, -7);
        var p2 = GeometryUtils.GetReflectedPoint(p1, l1, l2);
        AssertEx.WithinTolerance(-6.1878453038674, p2.X);
        AssertEx.WithinTolerance(-4.5690607734807, p2.Y);
    }

    [TestMethod]
    public void NegativeValuesNegativeSlope()
    {
        var l1 = new PointDouble(-2, -30);
        var l2 = new PointDouble(-12, -12);
        var p1 = new PointDouble(-4, -7);
        var p2 = GeometryUtils.GetReflectedPoint(p1, l1, l2);
        AssertEx.WithinTolerance(-20.4716981132075, p2.X);
        AssertEx.WithinTolerance(-16.1509433962264, p2.Y);
    }
}
