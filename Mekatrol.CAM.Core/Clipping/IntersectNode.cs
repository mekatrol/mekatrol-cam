using Mekatrol.CAM.Core.Geometry;

namespace Mekatrol.CAM.Core.Clipping;

public class IntersectNode
{
    internal Edge? Edge1;
    internal Edge? Edge2;
    internal PointLong Point;

    public override string ToString()
    {
        return $"P:{Point}, E1: {Edge1}, E2: {Edge2}";
    }
}
