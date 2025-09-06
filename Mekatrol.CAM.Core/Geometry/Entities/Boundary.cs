namespace Mekatrol.CAM.Core.Geometry.Entities;

public class Boundary : IBoundary
{
    public Boundary()
    {
        Location = new PointDouble();
        Size = new PointDouble();
    }

    public Boundary(PointDouble location, PointDouble size)
    {
        Location = location;
        Size = size;
    }

    public PointDouble Location { get; set; }

    public PointDouble Size { get; set; }

    public PointDouble BottomRight => Location + Size;

    public IList<PointDouble> ToPoints()
    {
        return new[]
        {
            new PointDouble(Location.X, Location.Y),
            new PointDouble(Location.X + Size.X, Location.Y),
            new PointDouble(Location.X + Size.X, Location.Y + Size.Y),
            new PointDouble(Location.X, Location.Y + Size.Y)
        }.ToList();
    }

    public override string ToString()
    {
        return $"({Location}:{Size}):({Location + Size})";
    }
}
