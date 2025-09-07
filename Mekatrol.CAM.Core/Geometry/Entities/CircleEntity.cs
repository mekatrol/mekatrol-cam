namespace Mekatrol.CAM.Core.Geometry.Entities;

public class CircleEntity(double centerX, double centerY, double radius, GeometryTransform transform, Guid? id = null) : BaseEntity(GeometricEntityType.Circle, id, new PointDouble(centerX, centerY), transform), IGeometricEntity
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public CircleEntity()
        : this(0, 0, 1, new GeometryTransform(), null)
    {

    }

    public CircleEntity(PointDouble center, double radius, Guid? id)
        : this(center.X, center.Y, radius, new GeometryTransform(), id)
    {
    }

    public double Radius { get; set; } = radius;

    public override IReadOnlyList<PointDouble[]> ToPoints()
    {
        // We approximate the circle with a polyline whose chordal error (sagitta) is
        // below a small tolerance in model units. This keeps small circles light and
        // large circles smooth without a hardcoded segment count.

        // Center in current coordinates. BaseEntity already tracks Location post-transform.
        var cx = Location.X;
        var cy = Location.Y;

        // Always use a non-negative radius for sampling.
        var r = Math.Abs(Radius);

        // Handle the degenerate case: zero or near-zero radius.
        if (r <= double.Epsilon)
        {
            // Represent as a tiny 4-point diamond to avoid empty geometry.
            var tiny = Math.Max(1e-9, 1e-6); // pick a scale-safe epsilon
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

        // Maximum allowed sagitta (distance from arc to chord midpoint).
        // Tune as needed for your renderer. Smaller = smoother.
        const double maxSagitta = 0.25;

        // Bound sagitta so 0 < s ≤ r to avoid invalid acos inputs.
        var s = Math.Min(maxSagitta, r);

        // For a circular arc with central angle θ per segment:
        // sagitta s = r - r*cos(θ/2)  ⇒  cos(θ/2) = 1 - s/r
        var cosHalf = 1.0 - (s / r);
        // Numeric guard for acos domain.
        cosHalf = Math.Max(-1.0, Math.Min(1.0, cosHalf));
        var theta = 2.0 * Math.Acos(cosHalf); // radians per segment

        // Number of segments to complete 2π.
        var n = (int)Math.Ceiling((2.0 * Math.PI) / Math.Max(theta, 1e-9));

        // Clamp for sanity so we neither undersample nor explode point counts.
        n = Math.Max(12, Math.Min(n, 720));

        var points = new PointDouble[n + 1]; // +1 to explicitly close the loop
                                          // Uniform angle step for the full circle.
        var step = (2.0 * Math.PI) / n;

        for (var i = 0; i < n; i++)
        {
            var a = i * step;
            var x = cx + r * Math.Cos(a);
            var y = cy + r * Math.Sin(a);
            points[i] = new PointDouble(x, y);
        }

        // Duplicate the first vertex at the end to mark a closed path.
        points[n] = points[0];

        return [points];
    }
}
