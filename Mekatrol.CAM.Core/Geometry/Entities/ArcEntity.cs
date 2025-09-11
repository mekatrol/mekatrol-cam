using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry.Entities;

public class ArcEntity(
    PointDouble centerLocation,
    PointDouble startLocation,
    PointDouble endLocation,
    PointDouble radii,
    double startAngle,
    double sweepAngle,
    double ellipseRotation,
    GeometryTransform transform)
    : BaseEntity(GeometricEntityType.Arc, centerLocation, transform), IGeometricEntity
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public ArcEntity() : this(new PointDouble(0, 0), new PointDouble(1, 0), new PointDouble(-1, 0), new PointDouble(2, 2), 0, 180, 0, new GeometryTransform())
    {

    }

    public PointDouble StartLocation { get; set; } = startLocation;

    public PointDouble EndLocation { get; set; } = endLocation;

    public PointDouble Radii { get; set; } = new PointDouble(radii.X, radii.Y);

    public double EllipseRotation { get; set; } = ellipseRotation;

    public double StartAngle { get; set; } = startAngle;

    public double SweepAngle { get; set; } = sweepAngle;

    public override IReadOnlyList<PointDouble[]> ToPoints()
    {
        // We approximate the elliptical arc with a polyline whose chordal error
        // is below a small tolerance. Angles are stored in degrees on the entity,
        // but sampling is done in radians.
        //
        // Ellipse parametric form with rotation φ (in radians):
        // x(t) = cx + rx*cos(t)*cosφ - ry*sin(t)*sinφ
        // y(t) = cy + rx*cos(t)*sinφ + ry*sin(t)*cosφ
        //
        // We select the segment count using a circular worst-case bound based on
        // the larger radius so the chordal error stays ≤ maxSagitta for all t.

        var cx = Location.X;
        var cy = Location.Y;

        var rx = Math.Abs(Radii.X);
        var ry = Math.Abs(Radii.Y);

        // Degenerate cases: collapse to a line or a point if radii are tiny.
        if (rx <= double.Epsilon && ry <= double.Epsilon)
        {
            // Return a single point repeated twice for a visible but minimal arc.
            var p = new PointDouble(cx, cy);
            return [[p]];
        }

        if (rx <= double.Epsilon || ry <= double.Epsilon)
        {
            // If one radius ~0, this is effectively a rotated line segment.
            // Sample start and end only.
            var φ = GeometryUtils.DegreesToRadians(EllipseRotation);
            var t0 = GeometryUtils.DegreesToRadians(StartAngle);
            var t1 = t0 + GeometryUtils.DegreesToRadians(SweepAngle);

            PointDouble Eval1(double t)
            {
                var c = Math.Cos(t);
                var s = Math.Sin(t);
                var x = cx + rx * c * Math.Cos(φ) - ry * s * Math.Sin(φ);
                var y = cy + rx * c * Math.Sin(φ) + ry * s * Math.Cos(φ);
                return new PointDouble(x, y);
            }

            var line = new[] { Eval1(t0), Eval1(t1) };
            return [line];
        }

        // Angles in radians.
        var tStart = GeometryUtils.DegreesToRadians(StartAngle);
        var sweep = GeometryUtils.DegreesToRadians(SweepAngle);
        var φRot = GeometryUtils.DegreesToRadians(EllipseRotation);

        // Target chordal error. Smaller -> smoother, more segments.
        const double maxSagitta = 0.25;

        // Compute per-segment angle using a circle with radius rMax as a conservative bound:
        // s = r - r*cos(θ/2)  =>  θ = 2*acos(1 - s/r)
        var rMax = Math.Max(rx, ry);
        var s = Math.Min(maxSagitta, rMax);
        var cosH = 1.0 - (s / rMax);
        cosH = Math.Max(-1.0, Math.Min(1.0, cosH)); // guard for numeric drift
        var theta = 2.0 * Math.Acos(cosH);       // radians per segment

        // Segment count over |sweep|.
        var n = (int)Math.Ceiling(Math.Abs(sweep) / Math.Max(theta, 1e-9));
        n = Math.Clamp(n, 2, 720); // keep sane bounds for renderers

        var dt = sweep / n;

        var points = new PointDouble[n + 1]; // open polyline: includes both endpoints

        // Local inline evaluator for the rotated ellipse.
        PointDouble Eval2(double t)
        {
            var c = Math.Cos(t);
            var s0 = Math.Sin(t);
            var x = cx + rx * c * Math.Cos(φRot) - ry * s0 * Math.Sin(φRot);
            var y = cy + rx * c * Math.Sin(φRot) + ry * s0 * Math.Cos(φRot);
            return new PointDouble(x, y);
        }

        // Fill samples from start to end. This preserves sweep direction, including negative.
        for (var i = 0; i <= n; i++)
        {
            var t = tStart + i * dt;
            points[i] = Eval2(t);
        }

        return [points];
    }
}
