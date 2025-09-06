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
    public static void Draw(this DrawingContext dc, IGeometricEntity g, Color color, float scale)
    {
        switch (g.Type)
        {
            case GeometricEntityType.Arc: dc.Draw((ArcEntity)g, color, scale); break;
            case GeometricEntityType.Circle: dc.Draw((CircleEntity)g, color, scale); break;
            case GeometricEntityType.CubicBezier: dc.Draw((CubicBezierEntity)g, color, scale); break;
            case GeometricEntityType.Ellipse: dc.Draw((EllipseEntity)g, color, scale); break;
            case GeometricEntityType.Line: dc.Draw((LineEntity)g, color, scale); break;
            case GeometricEntityType.Path: foreach (var c in ((PathEntity)g).Entities) { dc.Draw(c, color, scale); } break;
            case GeometricEntityType.Polygon:
            case GeometricEntityType.Polyline: dc.Draw((PolybaseEntity)g, color, scale); break;
            case GeometricEntityType.QuadraticBezier: dc.Draw(((QuadraticBezier)g).ToCubic(), color, scale); break;
            case GeometricEntityType.Rectangle: dc.Draw((RectangleEntity)g, color, scale); break;
            case GeometricEntityType.Text: dc.Draw((TextEntity)g, color, scale); break;
        }
    }

    public static void Draw(this DrawingContext dc, ArcEntity arc, Color color, float scale)
    {
        if (arc is null)
        {
            return;
        }

        var pen = new Pen(new SolidColorBrush(color), 1);
        var (pts, _, _, _, _) = GeometryUtils.PlotEllipse(
            arc.Location.X, arc.Location.Y, arc.Radii.X, arc.Radii.Y,
            GeometryUtils.DegreesToRadians(arc.StartAngle),
            GeometryUtils.DegreesToRadians(arc.SweepAngle),
            GeometryUtils.DegreesToRadians(arc.EllipseRotation));
        for (var i = 1; i < pts.Count; i++)
        {
            dc.DrawLine(pen, pts[i - 1].ToPt(scale), pts[i].ToPt(scale));
        }
    }

    public static void Draw(this DrawingContext dc, CircleEntity c, Color color, float scale, bool fill = false)
    {
        if (c is null)
        {
            return;
        }

        var d = (float)(c.Radius * 2) * scale;
        var x = (float)(c.Location.X - c.Radius) * scale;
        var y = (float)(c.Location.Y - c.Radius) * scale;
        var r = new Rect(x, y, d, d);
        var brush = fill ? new SolidColorBrush(color) : null;
        var pen = fill ? null : new Pen(new SolidColorBrush(color), 1);
        dc.DrawEllipse(brush, pen, r.Center, r.Width / 2, r.Height / 2);
    }

    public static void Draw(this DrawingContext dc, EllipseEntity e, Color color, float scale, bool fill = false)
    {
        if (e is null)
        {
            return;
        }

        var w = (float)(e.Radius.X * 2) * scale;
        var h = (float)(e.Radius.Y * 2) * scale;
        var x = (float)(e.Location.X - e.Radius.X) * scale;
        var y = (float)(e.Location.Y - e.Radius.Y) * scale;
        var r = new Rect(x, y, w, h);
        var brush = fill ? new SolidColorBrush(color) : null;
        var pen = fill ? null : new Pen(new SolidColorBrush(color), 1);
        dc.DrawEllipse(brush, pen, r.Center, r.Width / 2, r.Height / 2);
    }

    public static void Draw(this DrawingContext dc, RectangleEntity re, Color color, float scale, bool fill = false)
    {
        if (re is null)
        {
            return;
        }

        if (fill)
        {
            var rect = new Rect((float)re.Location.X * scale, (float)re.Location.Y * scale,
                                (float)re.Size.X * scale, (float)re.Size.Y * scale);
            dc.DrawRectangle(new SolidColorBrush(color), null, rect);
        }
        else
        {
            var pen = new Pen(new SolidColorBrush(color), 1);
            for (var i = 0; i < re.Points.Count; i++)
            {
                var j = (i + 1) % re.Points.Count;
                dc.DrawLine(pen, re.Points[i].ToPt(scale), re.Points[j].ToPt(scale));
            }
        }
    }

    public static void Draw(this DrawingContext dc, LineEntity l, Color color, float scale)
    {
        if (l is null)
        {
            return;
        }

        var pen = new Pen(new SolidColorBrush(color), 1);
        dc.DrawLine(pen, l.Location.ToPt(scale), l.EndLocation.ToPt(scale));
    }

    public static void Draw(this DrawingContext dc, PolybaseEntity p, Color color, float scale)
    {
        if (p is null || p.Points.Count == 0)
        {
            return;
        }

        var pen = new Pen(new SolidColorBrush(color), 1);
        for (var i = 1; i < p.Points.Count; i++)
        {
            dc.DrawLine(pen, p.Points[i - 1].ToPt(scale), p.Points[i].ToPt(scale));
        }
    }

    public static void Draw(this DrawingContext dc, CubicBezierEntity b, Color color, float scale)
    {
        if (b is null)
        {
            return;
        }

        var pen = new Pen(new SolidColorBrush(color), 1);
        var pts = b.PlotCubicBezier();
        for (var i = 1; i < pts.Count; i++)
        {
            dc.DrawLine(pen, pts[i - 1].ToPt(scale), pts[i].ToPt(scale));
        }
    }

    public static void Draw(this DrawingContext dc, TextEntity t, Color color, float scale)
    {
        if (t is null)
        {
            return;
        }

        var pen = new Pen(new SolidColorBrush(color), 1);
        var start = t.Location;

        for (var i = 0; i < t.Points.Count; i++)
        {
            var kind = t.PointTypes[i];
            var v1 = t.Points[i];
            var v2 = (i == t.Points.Count - 1 || (kind & PointType.ClosePoint) != 0) ? start : t.Points[i + 1];

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

    public static FontFamily BestFontFamily(string? suggested = null)
    {
        if (!string.IsNullOrWhiteSpace(suggested))
        {
            return new FontFamily(suggested);
        }

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
}
