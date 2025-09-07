using Avalonia.Media;
using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry.Entities;

public class TextEntity : BaseEntity
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public TextEntity()
        : this(0, 0, string.Empty, FontDescription.Default, TextAlignment.Left, new GeometryTransform())
    {

    }

    public TextEntity(
        double x,
        double y,
        string value,
        FontDescription font,
        TextAlignment alignment,
        GeometryTransform transform,
        Guid? id = null)
        : base(GeometricEntityType.Text, id, new PointDouble(x, y), transform)
    {
        Value = value;
        Font = font;
        Alignment = alignment;

        // Create untransformed text points
        var (points, pointTypes) = GeometryUtils.PlotText(
            Value,
            Font,
            Alignment,
            0,
            0,
            new Matrix3());

        _untransformedPoints = points;
        PointTypes = pointTypes;
    }

    public string Value { get; set; }

    public FontDescription Font { get; set; }

    public TextAlignment Alignment { get; }

    public IList<PointType> PointTypes { get; set; }

    public override IReadOnlyList<PointDouble[]> ToPoints()
    {
        // A piece of text can be made up of multiple polygons depending on the font
        // We need to break the point sets into multiple polygons

        var polygons = new List<PointDouble[]>();
        var polygon = new List<PointDouble>();

        for (var i = 0; i < PointTypes.Count; i++)
        {
            var pt = PointTypes[i];
            var p = UntransformedPoints[i];

            if (pt == PointType.StartOfFigure)
            {
                if (polygon.Count > 0)
                {
                    polygons.Add(polygon.ToArray());
                }

                polygon.Clear();
            }

            polygon.Add(p);
        }

        if (polygon.Count > 0)
        {
            polygons.Add(polygon.ToArray());
        }

        return polygons.AsReadOnly();
    }
}
