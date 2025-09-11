namespace Mekatrol.CAM.Core.Geometry.Entities;

public class EllipseEntity(double x, double y, double rx, double ry, GeometryTransform transform)
    : BaseEntity(GeometricEntityType.Ellipse, new PointDouble(x, y), transform), IGeometricEntity
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>

    public EllipseEntity()
        : this(0, 0, 0, 0, new GeometryTransform())
    {

    }

    public PointDouble Radius { get; set; } = new PointDouble(rx, ry);

    public override IReadOnlyList<PointDouble[]> ToPoints()
    {
        // Approximate the ellipse with a closed polyline whose chordal error (sagitta)
        // is below a small tolerance in model units. Sampling uses the parametric form:
        // x(t) = cx + rx * cos(t), y(t) = cy + ry * sin(t), t ∈ [0, 2π).
        //
        // Important:
        // - Matrix effects are already applied elsewhere: TransformGeometry scales
        //   Radius.X and Radius.Y; BaseEntity updates Location. Do not reapply transforms.
        // - We choose the segment count using the worst-case radius to bound error.

        var cx = Location.X;
        var cy = Location.Y;

        // Use non-negative radii for sampling.
        var rx = Math.Abs(Radius.X);
        var ry = Math.Abs(Radius.Y);

        // Degenerate case: both radii ~ 0 → produce a tiny closed diamond to avoid empty geometry.
        if (rx <= double.Epsilon && ry <= double.Epsilon)
        {
            var tiny = Math.Max(1e-9, 1e-6);
            var pts0 = new PointDouble[]
            {
            new(cx + tiny, cy),
            new(cx,        cy + tiny),
            new(cx - tiny, cy),
            new(cx,        cy - tiny),
            new(cx + tiny, cy) // close
            };
            return [pts0];
        }

        // Choose a conservative radius for error control.
        var rMax = Math.Max(rx, ry);

        // Maximum allowed sagitta (arc-to-chord midpoint distance). Smaller → smoother.
        const double maxSagitta = 0.25;

        // Bound s to (0, rMax].
        var s = Math.Min(maxSagitta, rMax);

        // For a circle of radius rMax, sagitta s for central angle θ per segment:
        // s = rMax - rMax * cos(θ/2) ⇒ cos(θ/2) = 1 - s/rMax.
        var cosHalf = 1.0 - (s / rMax);
        cosHalf = Math.Max(-1.0, Math.Min(1.0, cosHalf)); // numeric guard
        var theta = 2.0 * Math.Acos(cosHalf);          // radians per segment

        // Number of segments around 2π.
        var n = (int)Math.Ceiling((2.0 * Math.PI) / Math.Max(theta, 1e-9));

        // Clamp to avoid undersampling or runaway counts.
        n = Math.Max(12, Math.Min(n, 720));

        var pts = new PointDouble[n + 1]; // +1 to explicitly close the loop
        var step = (2.0 * Math.PI) / n;

        for (var i = 0; i < n; i++)
        {
            var t = i * step;
            var x = cx + rx * Math.Cos(t);
            var y = cy + ry * Math.Sin(t);
            pts[i] = new PointDouble(x, y);
        }

        // Close the polyline.
        pts[n] = pts[0];

        return [pts];
    }
}
