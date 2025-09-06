using Mekatrol.CAM.Core.Geometry.Entities;

namespace Mekatrol.CAM.Core.Geometry;

internal class BezierSpline
{
    private static readonly double[] Factorials;

    static double Factorial(int n)
    {
        if (n == 1)
        {
            return 1;
        }

        return n * Factorial(n - 1);
    }

    static BezierSpline()
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        const int numberOfFactorials = 200;
        Factorials = new double[numberOfFactorials];

        for (var n = numberOfFactorials - 1; n >= 0; n--)
        {
            Factorials[n] = Factorial(n + 1);
        }

        stopwatch.Stop();
        System.Diagnostics.Debug.WriteLine(string.Format("Took {0} ms.", stopwatch.ElapsedMilliseconds));
    }

    private static double Ni(int n, int i)
    {
        var a1 = Factorials[n];
        var a2 = Factorials[i];
        var a3 = Factorials[n - i];
        var ni = a1 / (a2 * a3);
        return ni;
    }

    private static double Bernstein(int n, int i, double t)
    {
        double ti; /* t^i */
        double tni; /* (1 - t)^i */

        const double tolerance = 1E-12;
        if (Math.Abs(t) < tolerance && i == 0)
        {
            ti = 1.0;
        }
        else
        {
            ti = Math.Pow(t, i);
        }

        if (n == i && Math.Abs(t - 1.0) < tolerance)
        {
            tni = 1.0;
        }
        else
        {
            tni = Math.Pow((1 - t), (n - i));
        }

        // Bernstein basis
        var basis = Ni(n, i) * ti * tni;
        return basis;
    }

    public static IList<PointDouble> CalculateSplineCurvePoints(IList<PointDouble> controlPoints, int pointCount)
    {
        var calculatedPoints = new PointDouble[pointCount];

        var outputIndex = 0;
        var t = 0.0;
        var step = 1.0 / (pointCount - 1);

        for (var i1 = 0; i1 != pointCount; i1++)
        {
            if ((1.0 - t) < 5e-6)
            {
                t = 1.0;
            }

            var jcount = 0;

            calculatedPoints[outputIndex] = new PointDouble(0, 0);

            for (var i = 0; i != controlPoints.Count; i++)
            {
                var basis = Bernstein(controlPoints.Count - 1, i, t);

                calculatedPoints[outputIndex] = calculatedPoints[outputIndex] + (basis * controlPoints[jcount]);
                jcount++;
            }

            outputIndex++;
            t += step;
        }

        return calculatedPoints;
    }

    public static PointDouble GetPoint(double t, PointDouble p0, PointDouble p1, PointDouble p2, PointDouble p3)
    {
        // P(t) = (1 - t) ^ 3P0 + 3(1 - t) ^ 2tP1 + 3(1 - t)t ^ 2P2 + t ^ 3P3

        var cx = 3 * (p1.X - p0.X);
        var cy = 3 * (p1.Y - p0.Y);
        var bx = 3 * (p2.X - p1.X) - cx;
        var by = 3 * (p2.Y - p1.Y) - cy;
        var ax = p3.X - p0.X - cx - bx;
        var ay = p3.Y - p0.Y - cy - by;
        var t2 = t * t;
        var t3 = t2 * t;

        var x = (ax * t3) + (bx * t2) + (cx * t) + p0.X;
        var y = (ay * t3) + (by * t2) + (cy * t) + p0.Y;

        return new PointDouble(x, y);
    }

    public static IList<PointDouble> PlotCubicBezier(CubicBezierEntity bezier)
    {
        var points = new List<PointDouble>();
        for (float t = 0; t <= 1.0f; t += 0.01f)
        {
            var p = GetPoint(t, bezier.Location, bezier.Control1, bezier.Control2, bezier.EndLocation);
            points.Add(p);
        }

        return points;
    }
}
