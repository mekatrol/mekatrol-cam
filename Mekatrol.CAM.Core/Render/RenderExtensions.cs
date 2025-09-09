using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Mekatrol.CAM.Core.Geometry;
using Mekatrol.CAM.Core.Geometry.Entities;
using static System.Net.Mime.MediaTypeNames;

namespace Mekatrol.CAM.Core.Render;

public static class RenderExtensions
{
    public const string DefaultFontFamilyName = "Arial";

    // Entry from a Control.OnRender(DrawingContext dc)
    public static void Draw(this DrawingContext dc, IGeometricEntity g, Color color, float penSize, float viewScale, Matrix3 accumulatedTransform)
    {
        switch (g.Type)
        {
            case GeometricEntityType.Arc: dc.Draw((ArcEntity)g, color, penSize, viewScale, accumulatedTransform); break;
            case GeometricEntityType.Circle: dc.Draw((CircleEntity)g, color, penSize, viewScale, accumulatedTransform); break;
            case GeometricEntityType.CubicBezier: dc.Draw((CubicBezierEntity)g, color, penSize, viewScale, accumulatedTransform); break;
            case GeometricEntityType.Ellipse: dc.Draw((EllipseEntity)g, color, penSize, viewScale, accumulatedTransform); break;
            case GeometricEntityType.Line: dc.Draw((LineEntity)g, color, penSize, viewScale, accumulatedTransform); break;
            case GeometricEntityType.Path: dc.Draw((PathEntity)g, color, penSize, viewScale, accumulatedTransform); break;
            case GeometricEntityType.Polygon:
            case GeometricEntityType.Polyline: dc.Draw((PolybaseEntity)g, color, penSize, viewScale, accumulatedTransform); break;
            case GeometricEntityType.QuadraticBezier: dc.Draw((QuadraticBezierEntity)g, color, penSize, viewScale, accumulatedTransform); break;
            case GeometricEntityType.Rectangle: dc.Draw((RectangleEntity)g, color, penSize, viewScale, accumulatedTransform); break;
            case GeometricEntityType.Text: dc.Draw((TextEntity)g, color, penSize, viewScale, accumulatedTransform); break;
        }
    }

    public static void Draw(this DrawingContext dc, PathEntity path, Color color, float penSize, float viewScale, Matrix3 accumulatedTransform)
    {
        var m = path.Transform.GetMatrix() * accumulatedTransform;

        foreach (var c in path.Entities)
        {
            dc.Draw(c, color, penSize, viewScale, m);
        }
    }

    public static void Draw(this DrawingContext dc, ArcEntity arc, Color color, float penSize, float viewScale, Matrix3 accumulatedTransform)
    {
        dc.DrawTransformed(arc, color, penSize, viewScale, accumulatedTransform,
            (pen) =>
            {
                foreach (var poly in arc.TransformedPolylines)
                {
                    for (var i = 1; i < poly.Length; i++)
                    {
                        dc.DrawLine(pen, poly[i - 1].ToPt(), poly[i].ToPt());
                    }
                }
            });
    }

    public static void Draw(this DrawingContext dc, CircleEntity circle, Color color, float penSize, float viewScale, Matrix3 accumulatedTransform)
    {
        dc.DrawTransformed(circle, color, penSize, viewScale, accumulatedTransform,
            (pen) =>
            {
                var circleCentre = new Point(circle.Location.X, circle.Location.Y);
                var radius = circle.Radius;

                dc.DrawEllipse(null, pen, circleCentre, radius, radius);
            });
    }

    public static void Draw(this DrawingContext dc, EllipseEntity ellipse, Color color, float penSize, float viewScale, Matrix3 accumulatedTransform)
    {
        dc.DrawTransformed(ellipse, color, penSize, viewScale, accumulatedTransform,
            (pen) =>
            {
                var cx = ellipse.Location.X;
                var cy = ellipse.Location.Y;
                var rx = ellipse.Radius.X;
                var ry = ellipse.Radius.Y;

                // Axis-aligned in local coords; transform handles rotation
                var rect = new Rect(cx - rx, cy - ry, rx * 2, ry * 2);
                dc.DrawEllipse(null, pen, rect);
            });
    }

    public static void Draw(this DrawingContext dc, RectangleEntity rect, Color color, float penSize, float viewScale, Matrix3 accumulatedTransform)
    {
        dc.DrawTransformed(rect, color, penSize, viewScale, accumulatedTransform,
            (pen) =>
            {
                foreach (var poly in rect.UntransformedPolylines)
                {
                    for (var i = 0; i < poly.Length; i++)
                    {
                        var j = (i + 1) % poly.Length;

                        var p1 = poly[i];
                        var p2 = poly[j];

                        dc.DrawLine(pen, p1.ToPt(), p2.ToPt());
                    }
                }
            });
    }

    public static void Draw(this DrawingContext dc, LineEntity line, Color color, float penSize, float viewScale, Matrix3 accumulatedTransform)
    {
        dc.DrawTransformed(line, color, penSize, viewScale, accumulatedTransform,
            (pen) =>
            {
                dc.DrawLine(pen, line.Location.ToPt(), line.EndLocation.ToPt());
            });
    }

    public static void Draw(this DrawingContext dc, PolybaseEntity poly, Color color, float penSize, float viewScale, Matrix3 accumulatedTransform)
    {
        if (poly.Points.Count == 0)
        {
            return;
        }

        dc.DrawTransformed(poly, color, penSize, viewScale, accumulatedTransform,
            (pen) =>
            {
                for (var i = 1; i < poly.Points.Count; i++)
                {
                    var p1 = poly.Points[i - 1];
                    var p2 = poly.Points[i];

                    dc.DrawLine(pen, p1.ToPt(), p2.ToPt());
                }

                // We need to automatically close a polygon if it is not already closed
                if (poly is PolygonEntity polygon)
                {
                    var p1 = polygon.Points[^1];
                    var p2 = polygon.Points[0];

                    if (p1 != p2)
                    {
                        dc.DrawLine(pen, p1.ToPt(), p2.ToPt());
                    }
                }
            });
    }

    public static void Draw(this DrawingContext dc, CubicBezierEntity bezier, Color color, float penSize, float viewScale, Matrix3 accumulatedTransform)
    {
        dc.DrawTransformed(bezier, color, penSize, viewScale, accumulatedTransform,
            (pen) =>
            {
                foreach (var poly in bezier.UntransformedPolylines)
                {
                    for (var i = 1; i < poly.Length; i++)
                    {
                        var p1 = poly[i - 1];
                        var p2 = poly[i];

                        dc.DrawLine(pen, p1.ToPt(), p2.ToPt());
                    }
                }
            });
    }

    public static void Draw(this DrawingContext dc, QuadraticBezierEntity quadratic, Color color, float penSize, float viewScale, Matrix3 accumulatedTransform)
    {
        dc.DrawTransformed(quadratic, color, penSize, viewScale, accumulatedTransform,
            (pen) =>
            {
                foreach (var poly in quadratic.UntransformedPolylines)
                {
                    for (var i = 1; i < poly.Length; i++)
                    {
                        var p1 = poly[i - 1];
                        var p2 = poly[i];
                        dc.DrawLine(pen, p1.ToPt(), p2.ToPt());
                    }
                }
            });
    }

    public static void Draw(this DrawingContext dc, TextEntity text, Color color, float penSize, float viewScale, Matrix3 accumulatedTransform)
    {
        dc.DrawTransformed(text, color, penSize, viewScale, accumulatedTransform,
            (pen) =>
            {
                foreach (var poly in text.UntransformedPolylines)
                {
                    for (var i = 1; i < poly.Length; i++)
                    {
                        var p1 = poly[i - 1];
                        var p2 = poly[i];
                        dc.DrawLine(pen, p1.ToPt(), p2.ToPt());
                    }

                    var first = poly[0];
                    var last = poly[^1];

                    if(last != first)
                    {
                        dc.DrawLine(pen, last.ToPt(), first.ToPt());
                    }
                }
            });
    }

    // Helpers
    public static Point ToPt(this PointDouble p) => new((float)p.X, (float)p.Y);

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

    private static void DrawTransformed(
        this DrawingContext dc,
        IGeometricEntity entity,
        Color color,
        float penSize,
        float viewScale,
        Matrix3 accumulatedTransform,
        Action<Pen> render)
    {
        // Local -> world/device
        var m = entity.Transform.GetMatrix() * accumulatedTransform;

        // Keep pen width ~penSize px on screen if scaled
        var pen = new Pen(new SolidColorBrush(color), penSize / viewScale);

        using (dc.PushTransform(m.ToAvaloniaMatrix()))
        {
            render(pen);
        }
    }
}
