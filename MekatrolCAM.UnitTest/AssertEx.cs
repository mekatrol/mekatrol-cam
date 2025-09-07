using Mekatrol.CAM.Core.Render;

namespace MekatrolCAM.UnitTest;

public class AssertEx
{
    public static void WithinTolerance(double expected, double actual, double tolerance = 1E7)
    {
        if (!GeometryUtils.NearEqual(expected, actual, tolerance))
        {
            throw new Exception("The actual value is not within tolerance");
        }
    }
}
