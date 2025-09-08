using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Mekatrol.CAM.Core.Geometry;
using Mekatrol.CAM.Core.Geometry.Entities;

namespace Mekatrol.CAM.Core.Render;

public static class RenderExtensions
{
    public const float DefaultFontSize = 30.0f; // mm
    public const string DefaultFontFamilyName = "Arial";

    // Entry from a Control.OnRender(DrawingContext dc)
    public static void Draw(this DrawingContext dc, IGeometricEntity g, Color color, float scale, Matrix3 accumulatedTransform)
    {
        switch (g.Type)
        {
            case GeometricEntityType.Arc: dc.Draw((ArcEntity)g, color, scale); break;
            case GeometricEntityType.Circle: dc.Draw((CircleEntity)g, color, scale, accumulatedTransform); break;
            case GeometricEntityType.CubicBezier: dc.Draw((CubicBezierEntity)g, color, scale, accumulatedTransform); break;
            case GeometricEntityType.Ellipse: dc.Draw((EllipseEntity)g, color, scale, accumulatedTransform); break;
            case GeometricEntityType.Line: dc.Draw((LineEntity)g, color, scale, accumulatedTransform); break;
            case GeometricEntityType.Path: dc.Draw((PathEntity)g, color, scale, accumulatedTransform); break;
            case GeometricEntityType.Polygon:
            case GeometricEntityType.Polyline: dc.Draw((PolybaseEntity)g, color, scale, accumulatedTransform); break;
            case GeometricEntityType.QuadraticBezier: dc.Draw(((QuadraticBezier)g).ToCubic(), color, scale, accumulatedTransform); break;
            case GeometricEntityType.Rectangle: dc.Draw((RectangleEntity)g, color, scale, accumulatedTransform); break;
            case GeometricEntityType.Text: dc.Draw((TextEntity)g, color, scale); break;
        }
    }

    public static void Draw(this DrawingContext dc, ArcEntity arc, Color color, float scale)
    {
        var pen = new Pen(new SolidColorBrush(color), 1);

        var pts = arc.TransformedPoints;

        for (var i = 1; i < pts.Count; i++)
        {
            dc.DrawLine(pen, pts[i - 1].ToPt(scale), pts[i].ToPt(scale));
        }
    }

    public static void Draw(this DrawingContext dc, PathEntity path, Color color, float scale, Matrix3 accumulatedTransform)
    {
        var m = path.Transform.GetMatrix() * accumulatedTransform;

        foreach (var c in path.Entities)
        {
            dc.Draw(c, color, scale, m);
        }
    }

    public static void Draw(this DrawingContext dc, CircleEntity circle, Color color, float scale, Matrix3 accumulatedTransform)
    {
        var m = circle.Transform.GetMatrix() * accumulatedTransform;

        var circleCentre = new PointDouble(circle.Location.X, circle.Location.Y) * m;
        var radius = circle.Radius * m.GetScale().X; // Circle radius is equal in X and Y direction so can pick either

        var pen = new Pen(new SolidColorBrush(color), 1);
        dc.DrawEllipse(null, pen, circleCentre.ToPt(scale), radius * scale, radius * scale);
    }

    public static void Draw(this DrawingContext dc, EllipseEntity ellipse, Color color, float scale, Matrix3 accumulatedTransform)
    {
        // Local -> world/device (keep your order)
        var m = ellipse.Transform.GetMatrix() * accumulatedTransform;

        // Keep stroke ~1px on screen if you scale the scene
        var pen = new Pen(new SolidColorBrush(color), 1.0 / scale);

        using (dc.PushTransform(m.ToAvaloniaMatrix()))          // apply rotation + translate (+ scale/shear if any)
        using (dc.PushTransform(Matrix.CreateScale(scale, scale))) // viewport scale
        {
            var cx = ellipse.Location.X;
            var cy = ellipse.Location.Y;
            var rx = ellipse.Radius.X;
            var ry = ellipse.Radius.Y;

            // Axis-aligned in local coords; transform handles rotation
            var rect = new Rect(cx - rx, cy - ry, rx * 2, ry * 2);
            dc.DrawEllipse(null, pen, rect);
        }
    }

    public static void Draw(this DrawingContext dc, RectangleEntity rect, Color color, float scale, Matrix3 accumulatedTransform)
    {
        var m = rect.Transform.GetMatrix() * accumulatedTransform;

        var pen = new Pen(new SolidColorBrush(color), 1);
        for (var i = 0; i < rect.UntransformedPoints.Count; i++)
        {
            var j = (i + 1) % rect.UntransformedPoints.Count;

            var p1 = rect.UntransformedPoints[i] * m;
            var p2 = rect.UntransformedPoints[j] * m;

            dc.DrawLine(pen, p1.ToPt(scale), p2.ToPt(scale));
        }
    }

    public static void Draw(this DrawingContext dc, LineEntity line, Color color, float scale, Matrix3 accumulatedTransform)
    {
        var m = line.Transform.GetMatrix() * accumulatedTransform;
        var p1 = line.Location * m;
        var p2 = line.EndLocation * m;

        var pen = new Pen(new SolidColorBrush(color), 1);
        dc.DrawLine(pen, p1.ToPt(scale), p2.ToPt(scale));
    }

    public static void Draw(this DrawingContext dc, PolybaseEntity poly, Color color, float scale, Matrix3 accumulatedTransform)
    {
        if (poly.Points.Count == 0)
        {
            return;
        }

        var m = poly.Transform.GetMatrix() * accumulatedTransform;

        var pen = new Pen(new SolidColorBrush(color), 1);
        for (var i = 1; i < poly.Points.Count; i++)
        {
            var p1 = poly.Points[i - 1] * m;
            var p2 = poly.Points[i] * m;

            dc.DrawLine(pen, p1.ToPt(scale), p2.ToPt(scale));
        }

        // We need to automatically close a polygon if it is not already closed
        if (poly is PolygonEntity polygon)
        {
            var p1 = polygon.Points[^1] * m;
            var p2 = polygon.Points[0] * m;

            if (p1 != p2)
            {
                dc.DrawLine(pen, p1.ToPt(scale), p2.ToPt(scale));
            }
        }
    }

    public static void Draw(this DrawingContext dc, CubicBezierEntity bezier, Color color, float scale, Matrix3 accumulatedTransform)
    {
        var pen = new Pen(new SolidColorBrush(color), 1);
        var pts = bezier.PlotCubicBezier();

        var m = bezier.Transform.GetMatrix() * accumulatedTransform;

        for (var i = 1; i < pts.Count; i++)
        {
            var p1 = pts[i - 1] * m;
            var p2 = pts[i] * m;

            dc.DrawLine(pen, p1.ToPt(scale), p2.ToPt(scale));
        }
    }

    public static void Draw(this DrawingContext dc, TextEntity text, Color color, float scale)
    {
        var pen = new Pen(new SolidColorBrush(color), 1);
        var start = text.Location;

        for (var i = 0; i < text.UntransformedPoints.Count; i++)
        {
            var kind = text.PointTypes[i];
            var v1 = text.UntransformedPoints[i];
            var v2 = (i == text.UntransformedPoints.Count - 1 || (kind & PointType.ClosePoint) != 0) ? start : text.UntransformedPoints[i + 1];

            switch (kind & PointType.LowOrderMask)
            {
                case PointType.StartOfFigure:
                    start = v1;
                    dc.DrawLine(pen, v1.ToPt(scale), v2.ToPt(scale));
                    break;
                case PointType.BezierPoint:
                case PointType.LinePoint:
                    dc.DrawLine(pen, v1.ToPt(scale), v2.ToPt(scale));
                    break;
                default:
                    continue;
            }
        }
    }

    // Helpers
    public static Point ToPt(this PointDouble p, float scale) =>
        new((float)p.X * scale, (float)p.Y * scale);

    public static Size MeasureString(
        string text,
        string familyName,
        double fontSize,
        FontStyle style = FontStyle.Normal,
        FontWeight weight = FontWeight.Normal,
        double maxWidth = double.PositiveInfinity)
    {
        var typeface = new Typeface(new FontFamily(familyName), style, weight);

        var layout = new TextLayout(
            text,
            typeface,
            fontSize,
            Brushes.Transparent,
            TextAlignment.Left,
            maxWidth < double.PositiveInfinity ? TextWrapping.Wrap : TextWrapping.NoWrap,
            null,
            null,
            FlowDirection.LeftToRight,
            maxWidth,
            double.PositiveInfinity,
            double.NaN,
            0,
            0,
            null
        );

        return new Size(layout.Height, layout.Width);
    }

    public static FontFamily BestFontFamily(string? suggestFontFamily = null)
    {
        if (string.IsNullOrWhiteSpace(suggestFontFamily))
        {
            suggestFontFamily = "Arial";
        }

        // All family names
        var fontFamilyNames = FontManager.Current
            .SystemFonts               // IFontCollection<FontFamily>
            .Select(ff => ff.Name)     // primary family name
            .OrderBy(n => n)
            .ToList();

        if (!string.IsNullOrWhiteSpace(suggestFontFamily))
        {
            // Get rid of any single quotes surrounding family name
            suggestFontFamily = suggestFontFamily.TrimStart('\'').TrimEnd('\'');
        }

        if (!string.IsNullOrWhiteSpace(suggestFontFamily))
        {
            IList<string> suggestedValues = suggestFontFamily
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().TrimStart('"').TrimEnd('"'))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            foreach (var value in suggestedValues)
            {
                var matchingFontFamily = fontFamilyNames.FirstOrDefault(f => f.Equals(value, StringComparison.OrdinalIgnoreCase));
                if (matchingFontFamily != null)
                {
                    return matchingFontFamily;
                }

                // Remove any hyphens
                var alternateValue = value.Replace("-", " ");

                // Try contains
                matchingFontFamily = fontFamilyNames.FirstOrDefault(f => f.Contains(alternateValue, StringComparison.OrdinalIgnoreCase));
                if (matchingFontFamily != null)
                {
                    return matchingFontFamily;
                }
            }
        }

        // No matches, so try and find our preferred, else the first font found
        var preferredFonts = new[] { DefaultFontFamilyName, "Monospace" };

        // Pick one that exists on Windows, Linux and Mac
        return new FontFamily("Arial, Segoe UI, Sans-Serif");
    }

    public static float ConvertGraphicSizeToMM(float size, string unit) => unit.ToLower().Trim() switch
    {
        "px" => (size * 25.4f) / 96.0f,
        "em" => size * 4.21752f,
        "cm" => size * 10f,
        "in" => size * 25.4f,
        "pt" => size * 25.4f / 72.0f,
        "pc" => size * 12f * 25.4f / 72.0f,
        "%" => size / 100.0f * DefaultFontSize,
        _ => size,
    };

    public static float ConvertMMToPixels(float mm) => (mm / 25.4f) * 96.0f;

    public static Matrix ToAvaloniaMatrix(this Matrix3 m)
    {
        // a, b, c, d, tx, ty
        return new Matrix(
            m.Data[0], // a = m00
            m.Data[3], // b = m10
            m.Data[1], // c = m01
            m.Data[4], // d = m11
            m.Data[2], // tx = m02
            m.Data[5]  // ty = m12
        );
    }
}
