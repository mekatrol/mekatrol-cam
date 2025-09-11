using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Render;
using System;

namespace MekatrolCAM.Views;

public sealed class GeometryView : Control
{
    public static readonly DirectProperty<GeometryView, IGeometricPathEntity> PathProperty =
        AvaloniaProperty.RegisterDirect<GeometryView, IGeometricPathEntity>(
            nameof(Path), o => o.Path, (o, v) => o.Path = v);

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

    public static readonly StyledProperty<Vector> PanProperty = AvaloniaProperty.Register<GeometryView, Vector>(nameof(Pan), default);

    public Vector Pan
    {
        get => GetValue(PanProperty);
        set => SetValue(PanProperty, value);
    }

    public static readonly StyledProperty<double> PointRadiusProperty = AvaloniaProperty.Register<GeometryView, double>(nameof(PointRadius), 0.5);

    public double PointRadius
    {
        get => GetValue(PointRadiusProperty);
        set => SetValue(PointRadiusProperty, value);
    }

    private bool _panning;
    private Point _lastScreen;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _panning = true;
            _lastScreen = e.GetPosition(this);
            e.Pointer.Capture(this);
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_panning && e.Pointer.Captured == this)
        {
            var p = e.GetPosition(this);
            var d = p - _lastScreen;
            _lastScreen = p;
            Pan = new Vector(Pan.X + d.X, Pan.Y + d.Y);
            InvalidateVisual();
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_panning && !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _panning = false;
            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var p = e.GetPosition(this);
        var oldScale = (double)Scale;

        // invert direction so roll up zooms in
        var zoom = Math.Pow(1.2, e.Delta.Y);

        var newScale = Math.Clamp(oldScale * zoom, 0.01, 100.0);
        if (Math.Abs(newScale - oldScale) < 1e-12) { return; }

        var wx = (p.X - Pan.X) / oldScale;
        var wy = (p.Y - Pan.Y) / oldScale;

        Pan = new Vector(p.X - wx * newScale, p.Y - wy * newScale);
        Scale = (float)newScale;

        InvalidateVisual();
        e.Handled = true;
    }

    public override void Render(DrawingContext context)
    {
        using (context.PushClip(Bounds))
        {
            var w = Bounds.Width;
            var h = Bounds.Height;

            // ensure we have a hit-test surface
            context.FillRectangle(new SolidColorBrush(Colors.Transparent), Bounds);

            if (Scale <= 0)
            {
                return;
            }

            // world-space drawing (grid + entities)
            using (context.PushTransform(Matrix.CreateTranslation(Pan.X, Pan.Y)))
            using (context.PushTransform(Matrix.CreateScale(Scale, Scale)))
            {
                // viewport in world units
                var inv = 1.0 / Scale;
                var worldLeft = -Pan.X * inv;
                var worldTop = -Pan.Y * inv;
                var worldRight = worldLeft + w * inv;
                var worldBottom = worldTop + h * inv;

                // grid step in world units
                const double step = 20.0; // = 20 px at Scale = 1

                // start positions snapped to step
                static double Snap(double v, double s) => Math.Floor(v / s) * s;
                var x0 = Snap(worldLeft, step);
                var y0 = Snap(worldTop, step);

                // 50% opacity brush, 1px screen pen => thickness = 1/Scale
                var gridColor = (Application.Current!.Resources["OrangeBrush"] as SolidColorBrush)?.Color ?? Colors.Orange;
                var gridPen = new Pen(new SolidColorBrush(gridColor, 0.5), 1.0 / Scale);

                for (var x = x0; x <= worldRight; x += step)
                {
                    context.DrawLine(gridPen, new Point(x, worldTop), new Point(x, worldBottom));
                }

                for (var y = y0; y <= worldBottom; y += step)
                {
                    context.DrawLine(gridPen, new Point(worldLeft, y), new Point(worldRight, y));
                }

                // geometry
                if (Path.Entities is { Count: > 0 })
                {
                    var boundsPen = new Pen(Brushes.DarkGray, 0.5 / Scale);
                    var color = Colors.GreenYellow;
                    var penSize = 2.0f;

                    foreach (var entity in Path.Entities)
                    {
                        var b = entity.BoundaryUntransformed;
                        var rect = new Rect(b.Location.X, b.Location.Y, b.Size.X, b.Size.Y);
                        context.DrawRectangle(null, boundsPen, rect);

                        context.Draw(entity, color, penSize, Scale, entity.Transform.GetMatrix());
                    }
                }
            }
        }
    }
}
