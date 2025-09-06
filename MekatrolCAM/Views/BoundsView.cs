using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Mekatrol.CAM.Core.Geometry.Entities;
using System.Collections.Generic;

namespace MekatrolCAM.Views;

public sealed class BoundsView : Control
{
    public static readonly DirectProperty<BoundsView, IReadOnlyList<IGeometricEntity>?> EntitiesProperty =
        AvaloniaProperty.RegisterDirect<BoundsView, IReadOnlyList<IGeometricEntity>?>(
            nameof(Entities),
            o => o.Entities,
            (o, v) => o.Entities = v);

    private IReadOnlyList<IGeometricEntity>? _entities;

    public IReadOnlyList<IGeometricEntity>? Entities
    {
        get => _entities;
        set { SetAndRaise(EntitiesProperty, ref _entities, value); InvalidateVisual(); }
    }

    public static readonly StyledProperty<double> ScaleProperty =
        AvaloniaProperty.Register<BoundsView, double>(nameof(Scale), 1.0);

    public double Scale
    {
        get => GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public static readonly StyledProperty<double> PointRadiusProperty =
       AvaloniaProperty.Register<BoundsView, double>(nameof(PointRadius), 0.5);
    public double PointRadius
    {
        get => GetValue(PointRadiusProperty); 
        set => SetValue(PointRadiusProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        // black background
        context.DrawRectangle(Brushes.Black, null, Bounds);

        if (Entities is null || Entities.Count == 0)
        {
            return;
        }

        var boundsPen = new Pen(Brushes.Yellow, 1);
        var pointBrush = Brushes.Red;
        var r = PointRadius;

        foreach (var e in Entities)
        {
            var (min, max) = e.GetMinMax(); // assumes your impl returns axis-aligned bounds
            var rect = new Rect(
                x: min.X * Scale,
                y: min.Y * Scale,
                width: (max.X - min.X) * Scale,
                height: (max.Y - min.Y) * Scale);

            context.DrawRectangle(null, boundsPen, rect);

            if (e is IGeometricPathEntity path && path.Entities is { Count: > 0 })
            {
                foreach (var child in path.Entities)
                {
                    DrawEntityPoints(context, child, Scale, r, pointBrush);
                }
            }
            else
            {
                DrawEntityPoints(context, e, Scale, r, pointBrush);
            }
        }
    }

    private static void DrawEntityPoints(DrawingContext context, IGeometricEntity ent, double scale, double radius, IBrush brush)
    {
        var contours = ent.ToPoints();
        
        if (contours is null)
        {
            return;
        }

        foreach (var arr in contours)
        {
            if (arr is null)
            {
                continue;
            }

            foreach (var p in arr)
            {
                context.DrawEllipse(brush, null, new Point(p.X * scale, p.Y * scale), radius, radius);
            }
        }
    }
}
