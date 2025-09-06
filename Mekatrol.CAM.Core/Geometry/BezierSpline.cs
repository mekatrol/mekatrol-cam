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
        // Bernstein basis polynomial B_i^n(t) = C(n,i) * t^i * (1−t)^(n−i)
        // n = degree, i = basis index, t ∈ [0,1].
        // Uses endpoint guards to avoid 0^0 from Math.Pow.

        double ti;   // t^i
        double tni;  // (1−t)^(n−i)

        const double tolerance = 1E-12;

        // Guard t=0: for i=0, t^0 = 1 exactly; avoid Math.Pow(0,0).
        ti = (Math.Abs(t) < tolerance && i == 0)
            ? 1.0
            : Math.Pow(t, i);

        // Guard t=1: for n==i, (1−t)^0 = 1 exactly; avoid Math.Pow(0,0).
        tni = (n == i && Math.Abs(t - 1.0) < tolerance)
            ? 1.0
            : Math.Pow(1 - t, n - i);

        // C(n,i) is the binomial coefficient (computed in Ni).
        var basis = Ni(n, i) * ti * tni;
        return basis;
    }

    public static IList<PointDouble> CalculateSplineCurvePoints(IList<PointDouble> controlPoints, int pointCount)
    {
        // Evaluates a Bézier curve of degree (n = controlPoints.Count - 1)
        // using the Bernstein basis at 'pointCount' uniformly spaced parameter values.
        // Result: 'pointCount' points from t=0 to t=1 inclusive.

        var calculatedPoints = new PointDouble[pointCount];

        var outputIndex = 0;

        // Parametric position along the curve. Starts at 0, ends at 1.
        var t = 0.0;

        // Uniform step so that the last sample hits t≈1.0.
        var step = 1.0 / (pointCount - 1);

        for (var i1 = 0; i1 != pointCount; i1++)
        {
            // Clamp final sample to exactly t=1 to avoid tiny FP error near 1.0.
            if ((1.0 - t) < 5e-6)
            {
                t = 1.0;
            }

            // Accumulator for the current point P(t)
            calculatedPoints[outputIndex] = new PointDouble(0, 0);

            // Sum_i [ B_i^n(t) * P_i ]
            // where B_i^n(t) = C(n,i) * t^i * (1-t)^(n-i)
            var jcount = 0; // tracks control point index (same as 'i'; kept to match original flow)

            for (var i = 0; i != controlPoints.Count; i++)
            {
                var basis = Bernstein(controlPoints.Count - 1, i, t); // B_i^n(t)
                calculatedPoints[outputIndex] = calculatedPoints[outputIndex] + (basis * controlPoints[jcount]);
                jcount++;
            }

            outputIndex++;
            t += step;
        }

        return calculatedPoints;
    }

    public static PointDouble GetCubicPoint(double t, PointDouble p0, PointDouble p1, PointDouble p2, PointDouble p3)
    {
        // Cubic Bézier with control points p0 (start), p1, p2, p3 (end).
        // Param t ∈ [0,1]. Position P(t) in Bernstein form:
        // P(t) = (1−t)^3 p0 + 3(1−t)^2 t p1 + 3(1−t) t^2 p2 + t^3 p3

        // Convert from Bernstein basis to power basis: P(t) = a t^3 + b t^2 + c t + p0
        // Coefficient c = 3*(p1 − p0)
        var cx = 3 * (p1.X - p0.X);
        var cy = 3 * (p1.Y - p0.Y);

        // Coefficient b = 3*(p2 − 2p1 + p0)  (written as 3*(p2 − p1) − c)
        var bx = 3 * (p2.X - p1.X) - cx;
        var by = 3 * (p2.Y - p1.Y) - cy;

        // Coefficient a = p3 − p0 − c − b
        var ax = p3.X - p0.X - cx - bx;
        var ay = p3.Y - p0.Y - cy - by;

        // Precompute powers
        var t2 = t * t;
        var t3 = t2 * t;

        // Evaluate the cubic polynomial separately for X and Y
        var x = (ax * t3) + (bx * t2) + (cx * t) + p0.X;
        var y = (ay * t3) + (by * t2) + (cy * t) + p0.Y;

        // Note: equivalent Horner form is slightly cheaper:
        // x = ((ax * t + bx) * t + cx) * t + p0.X;  same for y.

        return new PointDouble(x, y);
    }

    public static IList<PointDouble> PlotCubicBezier(CubicBezierEntity bezier)
    {
        var points = new List<PointDouble>();

        // Sample the cubic Bézier at uniform parameter steps.
        // t ∈ [0,1] with Δt = 0.01. This is simple uniform-in-t sampling
        // (not arc-length uniform), good for quick plotting and bounds.
        for (float t = 0; t <= 1.0f; t += 0.01f)
        {
            // Evaluate P(t) using control points:
            // p0 = bezier.Location, p1 = bezier.Control1,
            // p2 = bezier.Control2, p3 = bezier.EndLocation.
            var p = GetCubicPoint(t, bezier.Location, bezier.Control1, bezier.Control2, bezier.EndLocation);
            points.Add(p);
        }

        // Note:
        // - Floating-point stepping may skip the exact t=1 sample.
        //   If you must guarantee the last point equals p3, append it explicitly.
        // - For higher curvature fidelity, reduce the step or use adaptive sampling.

        return points;
    }

    public static PointDouble GetQuadraticPoint(double t, PointDouble p0, PointDouble p1, PointDouble p2)
    {
        // Quadratic Bézier with control points:
        // p0 = start, p1 = control, p2 = end. Parameter t ∈ [0,1].
        // Bernstein form:
        // P(t) = (1−t)^2 p0 + 2(1−t)t p1 + t^2 p2

        var u = 1 - t;                 // u = (1−t) for reuse

        // Evaluate X and Y independently using the Bernstein basis
        var x = u * u * p0.X           // (1−t)^2 * p0.X
              + 2 * u * t * p1.X       // 2(1−t)t * p1.X
              + t * t * p2.X;          // t^2 * p2.X

        var y = u * u * p0.Y           // (1−t)^2 * p0.Y
              + 2 * u * t * p1.Y       // 2(1−t)t * p1.Y
              + t * t * p2.Y;          // t^2 * p2.Y

        // Equivalent power-basis (Horner) form for fewer mults:
        // x = ((p0.X - 2*p1.X + p2.X) * t + 2*(p1.X - p0.X)) * t + p0.X
        // y = ((p0.Y - 2*p1.Y + p2.Y) * t + 2*(p1.Y - p0.Y)) * t + p0.Y

        return new PointDouble(x, y);
    }

    public static IList<PointDouble> PlotQuadraticBezier(QuadraticBezier bezier, double step = 0.01)
    {
        // Samples a quadratic Bézier curve at uniform parameter intervals.
        // step: Δt in [0,1]. Smaller step → more points (denser curve).
        // Note: This is uniform-in-t sampling, not uniform in arc length.

        var points = new List<PointDouble>();

        // March t from 0 to 1 by 'step'. Floating-point addition may not hit t==1 exactly.
        for (double t = 0; t <= 1.0; t += step)
        {
            // Evaluate P(t) = (1−t)^2 p0 + 2(1−t)t p1 + t^2 p2
            points.Add(GetQuadraticPoint(t, bezier.Location, bezier.Control, bezier.EndLocation));
        }

        // Guard: ensure the last sample equals the exact end point p2.
        // Because of FP stepping, the loop might miss t==1 or stop slightly before.
        if (points.Count == 0 || points[^1].X != bezier.EndLocation.X || points[^1].Y != bezier.EndLocation.Y)
        {
            points.Add(bezier.EndLocation);
        }

        // Notes:
        // - If you need better fidelity on sharp curves, reduce 'step' or use adaptive sampling.
        // - If 'step' <= 0, this loop would not progress; validate upstream if needed.

        return points;
    }

    public static CubicBezierEntity ToCubic(QuadraticBezier bezier)
    {
        // A quadratic bezier of the form:
        //      [P1, C, P2]
        // can be converted to a cubic bezier of the form:
        //      [P1, C1, C2, P2]
        // using:
        //      C1 = P1 + 2/3 * (C - P1)
        //      C2 = P2 + 2/3 * (C - P2)
        const double twoThirds = 2.0 / 3.0;

        var control1 = bezier.Location + twoThirds * (bezier.Control - bezier.Location);
        var control2 = bezier.EndLocation + twoThirds * (bezier.Control - bezier.EndLocation);

        return new CubicBezierEntity(bezier.Location, control1, control2, bezier.EndLocation, new GeometryTransform());
    }
}
