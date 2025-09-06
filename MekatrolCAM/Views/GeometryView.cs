using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Render;
using System.Collections.Generic;

namespace MekatrolCAM.Views;

public sealed class GeometryView : Control
{
    public static readonly DirectProperty<GeometryView, IReadOnlyList<IGeometricEntity>?> EntitiesProperty =
        AvaloniaProperty.RegisterDirect<GeometryView, IReadOnlyList<IGeometricEntity>?>(
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
        AvaloniaProperty.Register<GeometryView, double>(nameof(Scale), 1.0);

    public double Scale
    {
        get => GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public static readonly StyledProperty<double> PointRadiusProperty =
       AvaloniaProperty.Register<GeometryView, double>(nameof(PointRadius), 0.5);
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

        var boundsPen = new Pen(Brushes.DarkGray, 0.2);

        var i = 0;
        foreach (var entity in Entities)
        {
            var (min, max) = entity.GetMinMax();
            var rect = new Rect(
                x: min.X * Scale,
                y: min.Y * Scale,
                width: (max.X - min.X) * Scale,
                height: (max.Y - min.Y) * Scale);

            context.DrawRectangle(null, boundsPen, rect);

            var color = RenderColors.Palette[i++];
            context.Draw(entity, color, 1.0f);

            if (i >= RenderColors.Palette.Length)
            {
                i = 0;
            }
        }
    }
}
