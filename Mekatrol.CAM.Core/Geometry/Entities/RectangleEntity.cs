namespace Mekatrol.CAM.Core.Geometry.Entities;

public class RectangleEntity(double x, double y, double w, double h, double rx, double ry, GeometryTransform transform, Guid? id = null)
    : BaseEntity(GeometricEntityType.Rectangle, id, new PointDouble(x, y), transform), IGeometricEntity
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public RectangleEntity()
        : this(0, 0, 0, 0, 0, 0, new GeometryTransform())
    {

    }

    public PointDouble Size { get; set; } = new PointDouble(w, h);

    public PointDouble CornerRounding { get; set; } = new PointDouble(rx, ry);

    public override IReadOnlyList<PointDouble[]> ToPoints()
    {
        var w = Size.X;
        var h = Size.Y;
        var x = Location.X;
        var y = Location.Y;

        // Normalize corner radii
        var rx = Math.Abs(CornerRounding.X);
        var ry = Math.Abs(CornerRounding.Y);

        if (rx == 0 && ry > 0)
        {
            rx = ry;
        }

        if (ry == 0 && rx > 0)
        {
            ry = rx;
        }

        rx = Math.Min(rx, w * 0.5);
        ry = Math.Min(ry, h * 0.5);

        // Simple rectangle
        if (rx == 0 && ry == 0)
        {
            var tl = new PointDouble(x, y);
            var tr = new PointDouble(x + w, y);
            var br = new PointDouble(x + w, y + h);
            var bl = new PointDouble(x, y + h);
            return [[tl, tr, br, bl, tl]];
        }

        // Rounded rectangle: y increases downward
        var pts = new List<PointDouble>(64);

        // Helpers
        static IEnumerable<PointDouble> Arc(double cx, double cy, double arx, double ary,
                                            double startDeg, double endDeg, int segs, bool includeStart = false)
        {
            var s = Math.PI / 180.0 * startDeg;
            var e = Math.PI / 180.0 * endDeg;
            if (segs < 1)
            {
                segs = 1;
            }

            for (var i = 0; i <= segs; i++)
            {
                var t = (double)i / segs;
                var a = s + (e - s) * t;
                if (i == 0 && !includeStart)
                {
                    continue; // avoid duplicate of previous point
                }

                yield return new PointDouble(cx + arx * Math.Cos(a), cy + ary * Math.Sin(a));
            }
        }

        // Segment count based on radius (cap for speed)
        static int Segs(double ar) => Math.Clamp((int)Math.Ceiling(ar / 2.0), 3, 16);

        // Start at top edge, just after top-left arc start
        pts.Add(new PointDouble(x + rx, y));

        // Top edge to before top-right arc
        pts.Add(new PointDouble(x + w - rx, y));

        // Top-right arc: center (x+w-rx, y+ry), 270° -> 360° (CW from top to right)
        pts.AddRange(Arc(x + w - rx, y + ry, rx, ry, 270, 360, Segs(Math.Max(rx, ry))));

        // Right edge
        pts.Add(new PointDouble(x + w, y + h - ry));
        // Bottom-right arc: 0° -> 90°
        pts.AddRange(Arc(x + w - rx, y + h - ry, rx, ry, 0, 90, Segs(Math.Max(rx, ry))));

        // Bottom edge
        pts.Add(new PointDouble(x + rx, y + h));
        // Bottom-left arc: 90° -> 180°
        pts.AddRange(Arc(x + rx, y + h - ry, rx, ry, 90, 180, Segs(Math.Max(rx, ry))));

        // Left edge
        pts.Add(new PointDouble(x, y + ry));
        // Top-left arc: 180° -> 270°
        pts.AddRange(Arc(x + rx, y + ry, rx, ry, 180, 270, Segs(Math.Max(rx, ry))));

        // Close
        pts.Add(pts[0]);

        return [pts.ToArray()];
    }
}
