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

    public static readonly StyledProperty<float> ScaleProperty =
        AvaloniaProperty.Register<GeometryView, float>(nameof(Scale), 1.0f);

    public float Scale
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
            var bounds = entity.Boundary;

            var rect = new Rect(
                x: bounds.Location.X * Scale,
                y: bounds.Location.Y * Scale,
                width: bounds.Size.X * Scale,
                height: bounds.Size.Y * Scale);

            context.DrawRectangle(null, boundsPen, rect);

            var color = RenderColors.Palette[i++];
            context.Draw(entity, color, Scale);

            if (i >= RenderColors.Palette.Length)
            {
                i = 0;
            }
        }
    }
}
