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
        var contours = GeometryUtils.CreateTextContours(
            Value,
            Font,
            Alignment,
            (float)Location.X,
            (float)Location.Y);

        // A piece of text can be made up of multiple polygons depending on the font
        // We need to break the point sets into multiple polygons

        // For example the letter 'A' can be made up of two polygons depending on font

        //        /\        
        //       /  \       
        //      / /\ \      ╔═══╗
        //     / /--\ \     ║   ║
        //    /        \    ╠═══╣
        //   / /------\ \   ║   ║
        //  /_/        \_\  ╨   ╨

        var polygons = new List<PointDouble[]>();
        var polygon = new List<PointDouble>();

        foreach (var contour in contours)
        {
            foreach (var contourSegment in contour.Segments)
            {
                var pts = contourSegment.Points;
                switch (contourSegment.SegmentType)
                {
                    case ControurSegmentType.OpenContour:
                        // Add starting point
                        polygon.Add(pts[0]);
                        break;

                    case ControurSegmentType.LineTo:
                        // Add line end point ([0] is start point)
                        polygon.Add(pts[1]);
                        break;

                    case ControurSegmentType.Cubic:
                        {
                            var cubic = new CubicBezierEntity(pts[0], pts[1], pts[2], pts[3], GeometryTransform.Identity);
                            var cubicPoints = cubic.ToPoints();
                            polygon.AddRange(cubicPoints[0].Skip(1)); // Skip first point because it is the same as the prev point
                        }
                        break;

                    case ControurSegmentType.Quadratic:
                        {
                            var quad = new QuadraticBezierEntity(pts[0], pts[1], pts[2], GeometryTransform.Identity);
                            var quadPoints = quad.ToPoints();
                            polygon.AddRange(quadPoints[0].Skip(1)); // Skip first point because it is the same as the prev point
                        }
                        break;

                    case ControurSegmentType.CloseContour:
                        // Add start new
                        polygons.Add(polygon.ToArray());

                        // Start new
                        polygon.Clear();
                        break;

                    default:
                        break;
                }
            }
        }

        return polygons.AsReadOnly();
    }
}
