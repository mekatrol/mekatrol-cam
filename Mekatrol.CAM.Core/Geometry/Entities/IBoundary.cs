namespace Mekatrol.CAM.Core.Geometry.Entities;

public interface IBoundary
{
    /// <summary>
    /// The location of the boundary box
    /// </summary>
    PointDouble Location { get; }

    /// <summary>
    /// The size of the boundary box
    /// </summary>
    PointDouble Size { get; }

    /// <summary>
    /// The value of Location + Size
    /// </summary>
    PointDouble BottomRight { get; }

    IList<PointDouble> ToPoints();
}
