using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Render;

namespace MekatrolCAM.Views;

public sealed class GeometryView : Control
{
    public static readonly DirectProperty<GeometryView, IGeometricPathEntity> PathProperty =
        AvaloniaProperty.RegisterDirect<GeometryView, IGeometricPathEntity>(
            nameof(Path),
            o => o.Path,
            (o, v) => o.Path = v);

    private IGeometricPathEntity _path = new PathEntity();

    public IGeometricPathEntity Path
    {
        get => _path;
        set { SetAndRaise(PathProperty, ref _path, value); InvalidateVisual(); }
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

        if (Path.Entities is null || Path.Entities.Count == 0)
        {
            return;
        }

        var boundsPen = new Pen(Brushes.DarkGray, 0.2);

        var i = 0;
        foreach (var entity in Path.Entities)
        {
            var bounds = entity.BoundaryUntransformed;

            var rect = new Rect(
                x: bounds.Location.X * Scale,
                y: bounds.Location.Y * Scale,
                width: bounds.Size.X * Scale,
                height: bounds.Size.Y * Scale);

            context.DrawRectangle(null, boundsPen, rect);

            var color = RenderColors.Palette[i++];
            context.Draw(entity, color, Scale, entity.Transform.GetMatrix());

            if (i >= RenderColors.Palette.Length)
            {
                i = 0;
            }
        }
    }
}
