using Mekatrol.CAM.Core.Geometry;
using Mekatrol.CAM.Core.Render;

namespace MekatrolCAM.UnitTest.Geometry;

[TestClass]
public class MatrixTests
{
    [TestMethod]
    public void TestRotation()
    {
        var tests = new[]
        {
            new { Angle = +000.0, Input = new PointDouble (+1, +0), Expected = new PointDouble(+1, +0) },
            new { Angle = -000.0, Input = new PointDouble (-1, +0), Expected = new PointDouble(-1, +0) },
            new { Angle = +090.0, Input = new PointDouble (+1, +0), Expected = new PointDouble(+0, +1) },
            new { Angle = -090.0, Input = new PointDouble (+1, +0), Expected = new PointDouble(+0, -1) },
            new { Angle = +180.0, Input = new PointDouble (+1, +0), Expected = new PointDouble(-1, +0) },
            new { Angle = -180.0, Input = new PointDouble (+1, +0), Expected = new PointDouble(-1, +0) },
            new { Angle = +270.0, Input = new PointDouble (+1, +0), Expected = new PointDouble(+0, -1) },
            new { Angle = -270.0, Input = new PointDouble (+1, +0), Expected = new PointDouble(+0, +1) }
        };

        foreach (var test in tests)
        {
            var rotation = GeometryUtils.DegreesToRadians(test.Angle);

            var cosTheta = (float)Math.Cos(rotation);
            var sinTheta = (float)Math.Sin(rotation);

            // Rotate point about the origin
            PointDouble manuallyRotated = new(
                test.Input.X * cosTheta - test.Input.Y * sinTheta,
                test.Input.X * sinTheta + test.Input.Y * cosTheta);

            var m = Matrix3.CreateRotation(rotation);

            var r = test.Input * m;

            AssertEx.WithinTolerance(test.Expected.X, r.X);
            AssertEx.WithinTolerance(test.Expected.Y, r.Y);
            AssertEx.WithinTolerance(manuallyRotated.X, r.X);
            AssertEx.WithinTolerance(manuallyRotated.Y, r.Y);

            var extractedRotation = m.GetRotation();
            AssertEx.WithinTolerance(GeometryUtils.NormaliseAngleRadians(rotation), GeometryUtils.NormaliseAngleRadians(extractedRotation));
        }
    }

    [TestMethod]
    public void TestScale()
    {
        var tests = new[]
        {
            new { Scale = new PointDouble (+2.0, -1.0), Input = new PointDouble (+1, +1), Expected = new PointDouble(+2.0, -1.0), ExpectedRotation = +180.0 },
            new { Scale = new PointDouble (+2.0, +0.0), Input = new PointDouble (+1, +0), Expected = new PointDouble(+2.0, +0.0), ExpectedRotation = +000.0 },
            new { Scale = new PointDouble (+2.0, +0.0), Input = new PointDouble (-1, +0), Expected = new PointDouble(-2.0, +0.0), ExpectedRotation = +000.0 },
            new { Scale = new PointDouble (+1.0, +2.0), Input = new PointDouble (+1, +1), Expected = new PointDouble(+1.0, +2.0), ExpectedRotation = +000.0 },
            new { Scale = new PointDouble (+1.0, +2.0), Input = new PointDouble (+1, -1), Expected = new PointDouble(+1.0, -2.0), ExpectedRotation = +000.0 },
            new { Scale = new PointDouble (+0.5, +0.3), Input = new PointDouble (+1, +1), Expected = new PointDouble(+0.5, +0.3), ExpectedRotation = +000.0 },
            new { Scale = new PointDouble (-3.0, -0.5), Input = new PointDouble (+1, +1), Expected = new PointDouble(-3.0, -0.5), ExpectedRotation = +180.0 },
            new { Scale = new PointDouble (-2.0, -3.0), Input = new PointDouble (-1, -1), Expected = new PointDouble(+2.0, +3.0), ExpectedRotation = +180.0 }
        };

        foreach (var test in tests)
        {
            PointDouble manuallyScaled = new(
                test.Input.X * test.Scale.X,
                test.Input.Y * test.Scale.Y);

            var m = Matrix3.CreateScale(test.Scale);

            var r = test.Input * m;
            var scale = m.GetScale();
            var rotation = m.GetRotation();
            var rotationDegrees = GeometryUtils.NormaliseAngleDegrees(GeometryUtils.RadiansToDegrees(rotation));

            AssertEx.WithinTolerance(test.Expected.X, r.X);
            AssertEx.WithinTolerance(test.Expected.Y, r.Y);
            AssertEx.WithinTolerance(manuallyScaled.X, r.X);
            AssertEx.WithinTolerance(manuallyScaled.Y, r.Y);
            AssertEx.WithinTolerance(Math.Abs(test.Scale.X), scale.X);
            AssertEx.WithinTolerance(Math.Abs(test.Scale.Y), scale.Y);
            AssertEx.WithinTolerance(test.ExpectedRotation, rotationDegrees);
        }
    }

    [TestMethod]
    public void TestTranslate()
    {
        var tests = new[]
        {
            new { Translate = new PointDouble (+1.0, +0.5), Input = new PointDouble (+0, +0), Expected = new PointDouble(+1.0, +0.5) },
            new { Translate = new PointDouble (+0.5, +1.0), Input = new PointDouble (+0, +0), Expected = new PointDouble(+0.5, +1.0) },
            new { Translate = new PointDouble (-1.0, +0.0), Input = new PointDouble (+0, +0), Expected = new PointDouble(-1.0, +0.0) },
            new { Translate = new PointDouble (+0.0, -1.0), Input = new PointDouble (+0, +0), Expected = new PointDouble(+0.0, -1.0) },
            new { Translate = new PointDouble (+1.0, +1.0), Input = new PointDouble (+0, +0), Expected = new PointDouble(+1.0, +1.0) },
            new { Translate = new PointDouble (-2.0, +2.0), Input = new PointDouble (+0, +0), Expected = new PointDouble(-2.0, +2.0) },
            new { Translate = new PointDouble (-1.0, -1.0), Input = new PointDouble (+0, +0), Expected = new PointDouble(-1.0, -1.0) },
            new { Translate = new PointDouble (+0.0, +0.0), Input = new PointDouble (+0, +0), Expected = new PointDouble(+0.0, +0.0) }
        };

        foreach (var test in tests)
        {
            PointDouble manuallyTranslated = new(
                test.Input.X + test.Translate.X,
                test.Input.Y + test.Translate.Y);

            var m = Matrix3.CreateTranslate(test.Translate);

            var r = test.Input * m;

            AssertEx.WithinTolerance(test.Expected.X, r.X);
            AssertEx.WithinTolerance(test.Expected.Y, r.Y);
            AssertEx.WithinTolerance(manuallyTranslated.X, r.X);
            AssertEx.WithinTolerance(manuallyTranslated.Y, r.Y);

            var translate = m.GetTranslation();
            AssertEx.WithinTolerance(test.Translate.X, translate.X);
            AssertEx.WithinTolerance(test.Translate.Y, translate.Y);
        }
    }

    [TestMethod]
    public void TestTransform()
    {
        var tests = new[]
        {
            // Nothing changed
            new { Scale = +1.0, Angle = 0.0, Translate = new PointDouble (+0.0, +0.0), Input = new PointDouble (+1, +0), Expected = new PointDouble(+1.0, +0.0) },
            new { Scale = +1.0, Angle = 0.0, Translate = new PointDouble (+0.0, +0.0), Input = new PointDouble (+1, +1), Expected = new PointDouble(+1.0, +1.0) },
            new { Scale = +1.0, Angle = 0.0, Translate = new PointDouble (+0.0, +0.0), Input = new PointDouble (-1, -1), Expected = new PointDouble(-1.0, -1.0) },

            // Positive angle
            new { Scale = +2.0, Angle = 90.0, Translate = new PointDouble (+1.0, +2.0), Input = new PointDouble (+1, +0), Expected = new PointDouble(+1.0, +4.0) },
            new { Scale = +2.0, Angle = 90.0, Translate = new PointDouble (+2.0, +1.0), Input = new PointDouble (+0, +1), Expected = new PointDouble(+0.0, +1.0) },
            new { Scale = +0.5, Angle = 90.0, Translate = new PointDouble (+3.0, +2.0), Input = new PointDouble (+1, +1), Expected = new PointDouble(+2.5, +2.5) },

            // Negative angle
            new { Scale = +2.0, Angle = -90.0, Translate = new PointDouble (+1.0, +2.0), Input = new PointDouble (+1, +0), Expected = new PointDouble(+1.0, +0.0) },
            new { Scale = +2.0, Angle = -90.0, Translate = new PointDouble (+2.0, +1.0), Input = new PointDouble (+0, +1), Expected = new PointDouble(+4.0, +1.0) },
            new { Scale = +0.5, Angle = -90.0, Translate = new PointDouble (+3.0, +2.0), Input = new PointDouble (+1, +1), Expected = new PointDouble(+3.5, +1.5) },
        };

        foreach (var test in tests)
        {
            var scale = Matrix3.CreateScale(test.Scale);
            var rotate = Matrix3.CreateRotation(GeometryUtils.DegreesToRadians(test.Angle));
            var translate = Matrix3.CreateTranslate(test.Translate);

            var result1 = test.Input * scale * rotate * translate;
            AssertEx.WithinTolerance(test.Expected.X, result1.X);
            AssertEx.WithinTolerance(test.Expected.Y, result1.Y);

            var result2 = test.Input * rotate * scale * translate;
            AssertEx.WithinTolerance(test.Expected.X, result2.X);
            AssertEx.WithinTolerance(test.Expected.Y, result2.Y);

            var result3 = test.Input * (scale * rotate * translate);
            AssertEx.WithinTolerance(test.Expected.X, result3.X);
            AssertEx.WithinTolerance(test.Expected.Y, result3.Y);

            var result4 = test.Input * (rotate * scale * translate);
            AssertEx.WithinTolerance(test.Expected.X, result4.X);
            AssertEx.WithinTolerance(test.Expected.Y, result4.Y);

            var transform = Matrix3.CreateTransform(test.Scale, GeometryUtils.DegreesToRadians(test.Angle), test.Translate);
            var result5 = test.Input * transform;
            AssertEx.WithinTolerance(test.Expected.X, result5.X);
            AssertEx.WithinTolerance(test.Expected.Y, result5.Y);
        }
    }

    [TestMethod]
    public void TestRotationWithNegativeScale()
    {
        var m = Matrix3.Identity * Matrix3.CreateScale(new PointDouble(1, -1)) * Matrix3.CreateRotation(GeometryUtils.DegreesToRadians(45));

        var scale = m.GetScale();
        var rotationRadians = m.GetRotation();
        var rotation = GeometryUtils.RadiansToDegrees(rotationRadians);

        // Scale is positive
        AssertEx.WithinTolerance(1, scale.X);
        AssertEx.WithinTolerance(1, scale.Y);

        // Becuase the Y scale was negative then the angle is reflected 
        // about the origin.
        AssertEx.WithinTolerance(-135, rotation);
        AssertEx.WithinTolerance(360 - 135, GeometryUtils.NormaliseAngleDegrees(rotation));
    }
}

