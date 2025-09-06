using Mekatrol.CAM.Core.Geometry;
using Mekatrol.CAM.Core.Geometry.Entities;
using System.Drawing;

namespace Mekatrol.CAM.Core.Render;

internal static class GraphicsExtensions
{
    public const float DefaultFontSize = 30.0f;
    public const string DefaultFontFamilyName = "Arial";

    internal static void Draw(this Graphics g, IGeometricEntity geometry, Color color, float scale)
    {
        switch (geometry.Type)
        {
            case GeometricEntityType.Arc:
                g.Draw((ArcEntity)geometry, color, scale);
                break;

            case GeometricEntityType.Circle:
                g.Draw((CircleEntity)geometry, color, scale);
                break;

            case GeometricEntityType.CubicBezier:
                g.Draw((CubicBezierEntity)geometry, color, scale);
                break;

            case GeometricEntityType.Ellipse:
                g.Draw((EllipseEntity)geometry, color, scale);
                break;

            case GeometricEntityType.Line:
                g.Draw((LineEntity)geometry, color, scale);
                break;

            case GeometricEntityType.Path:
                var path = (PathEntity)geometry;
                foreach (var childGeometry in path.Entities)
                {
                    g.Draw(childGeometry, color, scale);
                }
                break;

            case GeometricEntityType.Polygon:
                g.Draw((PolybaseEntity)geometry, color, scale);
                break;

            case GeometricEntityType.Polyline:
                g.Draw((PolybaseEntity)geometry, color, scale);
                break;

            case GeometricEntityType.QuadraticBezier:
                g.Draw((QuadraticBezier)geometry, color, scale);
                break;

            case GeometricEntityType.Rectangle:
                g.Draw((RectangleEntity)geometry, color, scale);
                break;

            case GeometricEntityType.Text:
                g.Draw((TextEntity)geometry, color, scale);
                break;
        }
    }

    internal static void Draw(this Graphics g, ArcEntity arc, Color color, float scale)
    {
        if (arc == null)
        {
            return;
        }

        using Pen pen = new(color);

        var (points, minX, minY, maxX, maxY) = GeometryUtils.PlotEllipse(
            arc.Location.X, arc.Location.Y,
            arc.Radii.X, arc.Radii.Y,
            GeometryUtils.DegreesToRadians(arc.StartAngle),
            GeometryUtils.DegreesToRadians(arc.SweepAngle),
            GeometryUtils.DegreesToRadians(arc.EllipseRotation));

        for (var i = 1; i < points.Count; i++)
        {
            g.DrawLine(pen, points[i - 1], points[i - 0], scale);
        }
    }

    internal static void Draw(this Graphics g, CircleEntity circle, Color color, float scale, bool fill = false)
    {
        if (circle == null)
        {
            return;
        }

        // Calc diameter
        var d = (float)(circle.Radius * 2.0) * scale;

        // Calculate top left position for ellipse (keep location at center of drawn ellipse)
        var x = (float)(circle.Location.X - circle.Radius) * scale;
        var y = (float)(circle.Location.Y - circle.Radius) * scale;

        if (fill)
        {
            using Brush brush = new SolidBrush(color);
            g.FillEllipse(brush, x, y, d, d);
        }
        else
        {
            using Pen pen = new(color);
            g.DrawEllipse(pen, x, y, d, d);
        }
    }

    internal static void Draw(this Graphics g, EllipseEntity ellipse, Color color, float scale, bool fill = false)
    {
        if (ellipse == null)
        {
            return;
        }

        // Calc width
        var w = (float)(ellipse.Radius.X * 2.0) * scale;

        // Calc height
        var h = (float)(ellipse.Radius.Y * 2.0) * scale;

        // Calculate top left position for ellipse (keep location at center of drawn ellipse)
        var x = (float)(ellipse.Location.X - ellipse.Radius.X) * scale;
        var y = (float)(ellipse.Location.Y - ellipse.Radius.Y) * scale;

        if (fill)
        {
            using Brush brush = new SolidBrush(color);
            g.FillEllipse(brush, x, y, w, h);
        }
        else
        {
            using Pen pen = new(color);
            g.DrawEllipse(pen, x, y, w, h);
        }
    }

    internal static void Draw(this Graphics g, RectangleEntity rect, Color color, float scale, bool fill = false)
    {
        if (rect == null)
        {
            return;
        }

        if (fill)
        {
            using Brush brush = new SolidBrush(color);
            g.FillRectangle(brush, (float)rect.Location.X * scale, (float)rect.Location.Y * scale, (float)rect.Size.X * scale, (float)rect.Size.Y * scale);
        }
        else
        {
            using Pen pen = new(color);
            for (var i = 0; i < rect.Points.Count; i++)
            {
                var i2 = (i + 1) % rect.Points.Count;
                g.DrawLine(pen, rect.Points[i], rect.Points[i2], scale);
            }
        }
    }

    internal static void Draw(this Graphics g, LineEntity line, Color color, float scale)
    {
        if (line == null)
        {
            return;
        }

        using var pen = new Pen(color);
        g.DrawLine(pen, line.Location, line.EndLocation, scale);
    }

    internal static void Draw(this Graphics g, PolybaseEntity poly, Color color, float scale)
    {
        if (poly == null || poly.Points.Count == 0)
        {
            return;
        }

        using Pen pen = new(color);
        for (var i = 1; i < poly.Points.Count; i++)
        {
            g.DrawLine(pen, poly.Points[i - 1], poly.Points[i], scale);
        }
    }

    internal static void Draw(this Graphics g, CubicBezierEntity bezier, Color color, float scale)
    {
        if (bezier == null)
        {
            return;
        }

        using Pen pen = new(color);

        var points = BezierSpline.PlotCubicBezier(bezier);
        for (var i = 1; i < points.Count; i++)
        {
            g.DrawLine(pen, points[i - 1], points[i - 0], scale);
        }
    }

    internal static void Draw(this Graphics g, QuadraticBezier bezier, Color color, float scale)
    {
        if (bezier == null)
        {
            return;
        }

        var cubic = bezier.ToCubic();
        Draw(g, cubic, color, scale);
    }

    internal static void Draw(this Graphics g, TextEntity text, Color color, float scale)
    {
        if (text == null)
        {
            return;
        }

        // Create a default start point
        var startPoint = text.Location;

        using var pen = new Pen(color);

        for (var i = 0; i < text.Points.Count; i++)
        {
            var pointType = text.PointTypes[i];

            // Get this (v2) and the next (v1) point
            var v1 = text.Points[i];

            // The next point is the start point if close path marker (or start location if last point in list)
            var v2 = i == text.Points.Count - 1 || (pointType & PointType.ClosePoint) != 0
                ? startPoint
                : text.Points[i + 1];

            switch (pointType & PointType.LowOrderMask)
            {
                case PointType.StartOfFigure:
                    startPoint = v1;

                    // We draw a line between this point and the next
                    g.DrawLine(pen, v1, v2, scale);
                    break;

                case PointType.BezierPoint:
                case PointType.LinePoint:
                    // We draw a line between this point and the next
                    g.DrawLine(pen, v1, v2, scale);
                    break;

                case PointType.Marker:
                case PointType.ClosePoint:
                default:
                    // Ignore it
                    continue;
            }
        }
    }

    internal static RectangleF ToDrawingRect(this IBoundary b)
    {
        return new RectangleF((float)b.Location.X, (float)b.Location.Y, (float)b.Size.X, (float)b.Size.Y);
    }

    public static void DrawLine(this Graphics g, Pen pen, PointDouble v1, PointDouble v2, float scale)
    {
        var p1 = new PointF((float)v1.X * scale, (float)v1.Y * scale);
        var p2 = new PointF((float)v2.X * scale, (float)v2.Y * scale);
        g.DrawLine(pen, p1, p2);
    }

    public static FontFamily BestFontFamily(string? fontFamilySuggested = null)
    {
        InstalledFontCollection installedFontCollection = new();
        var fontFamilies = installedFontCollection.Families.ToList();

        if (!string.IsNullOrWhiteSpace(fontFamilySuggested))
        {
            // Get rid of any single quotes surrounding family name
            fontFamilySuggested = fontFamilySuggested.TrimStart('\'').TrimEnd('\'');
        }

        if (!string.IsNullOrWhiteSpace(fontFamilySuggested))
        {
            IList<string> suggestedValues = fontFamilySuggested
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().TrimStart('"').TrimEnd('"'))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            foreach (var value in suggestedValues)
            {
                var matchingFontFamily = fontFamilies.FirstOrDefault(f => f.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
                if (matchingFontFamily != null)
                {
                    return matchingFontFamily;
                }

                // Remove any hyphens
                var alternateValue = value.Replace("-", " ");

                // Try contains
                matchingFontFamily = fontFamilies.FirstOrDefault(f => f.Name.Contains(alternateValue, StringComparison.OrdinalIgnoreCase));
                if (matchingFontFamily != null)
                {
                    return matchingFontFamily;
                }
            }
        }

        // No matches, so try and find our preferred, else the first font found
        var preferredFonts = new[] { DefaultFontFamilyName, "Monospace" };

        var fontFamily =
            fontFamilies.FirstOrDefault(f => preferredFonts.Contains(f.Name, StringComparer.OrdinalIgnoreCase)) ??
            fontFamilies.First();

        return fontFamily;
    }

    public static float ConvertGraphicSizeToMM(float size, string unit)
    {
        unit = unit.ToLower().Trim();

        return unit switch
        {
            // 1px = 25.4 mm / 96 dpi
            "px" => (size * 25.4f) / 96.0f,

            // 1 em = 4.21752 mm
            "em" => size * 4.21752f,

            // 1 cm = 10 mm
            "cm" => size * 10f,

            // 1 inch = 25.4 mm
            "in" => size * 25.4f,

            // 1 pt = 1/72 of an inch
            "pt" => size * 25.4f / 72.0f,

            // 1 pica = 12pt
            "pc" => size * 12f * 25.4f / 72.0f,

            // % = % of DefaultFontSize
            "%" => size / 100.0f * DefaultFontSize,

            // If not valid unit found assume size already is in mm.
            _ => size,
        };
    }

    public static float ConvertMMToPixels(float size)
    {
        return (size / 25.4f) * 96.0f;
    }
}
