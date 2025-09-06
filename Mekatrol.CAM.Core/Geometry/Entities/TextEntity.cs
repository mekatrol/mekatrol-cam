using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry.Entities;

public class TextEntity : PointsEntity, IGeometricEntity
{
    /// <summary>
    /// This empty constructor is used by the serializer
    /// </summary>
    public TextEntity()
        : this(0, 0, string.Empty, FontDescription.Default, StringAlignment.Near, new GeometryTransform())
    {

    }

    public TextEntity(
        double x,
        double y,
        string value,
        FontDescription font,
        StringAlignment alignment,
        ITransform transform,
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

        // Translate the location of the text
        SetUntransformedPoints(points.Select(p => p + Location).ToList());

        PointTypes = pointTypes;
    }

    public string Value { get; set; }

    public FontDescription Font { get; set; }

    public StringAlignment Alignment { get; }

    public IList<PointType> PointTypes { get; set; }

    public override IList<PointDouble[]> ToPoints()
    {
        // A piece of text can be made up of multiple polygons depending on the font
        // We need to break the point sets into multiple polygons

        var polygons = new List<PointDouble[]>();
        var polygon = new List<PointDouble>();

        for (var i = 0; i < PointTypes.Count; i++)
        {
            var pt = PointTypes[i];
            var p = Points[i];

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

        return polygons;
    }
}
