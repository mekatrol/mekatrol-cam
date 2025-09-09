using Avalonia.Media;
using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry.Entities;

public class TextEntity(
    double x,
    double y,
    string value,
    FontDescription font,
    TextAlignment alignment,
    GeometryTransform transform,
    Guid? id = null)
    : BaseEntity(GeometricEntityType.Text, id, new PointDouble(x, y), transform)
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public TextEntity()
        : this(0, 0, string.Empty, FontDescription.Default, TextAlignment.Left, new GeometryTransform())
    {

    }

    public string Value { get; set; } = value;

    public FontDescription Font { get; set; } = font;

    public TextAlignment Alignment { get; } = alignment;

    public override IReadOnlyList<PointDouble[]> ToPoints()
    {
        // Create untransformed text points
        var (points, pointTypes) = GeometryUtils.PlotText(
            Value,
            Font,
            Alignment,
            (float) Location.X,
            (float) Location.Y,
            new Matrix3());
        
        // A piece of text can be made up of multiple polygons depending on the font
        // We need to break the point sets into multiple polygons

        var polygons = new List<PointDouble[]>();
        var polygon = new List<PointDouble>();

        for (var i = 0; i < pointTypes.Count; i++)
        {
            var pointType = pointTypes[i];
            var point = points[i];

            if (pointType == PointType.StartOfFigure)
            {
                if (polygon.Count > 0)
                {
                    polygons.Add(polygon.ToArray());
                }

                polygon.Clear();
            }

            polygon.Add(point);
        }

        if (polygon.Count > 0)
        {
            polygons.Add(polygon.ToArray());
        }

        return polygons.AsReadOnly();
    }
}
