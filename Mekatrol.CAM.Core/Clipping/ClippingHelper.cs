using Mekatrol.CAM.Core.Geometry;
using Mekatrol.CAM.Core.Geometry.Algorithms;

namespace Mekatrol.CAM.Core.Clipping;

internal static class ClippingHelper
{
    public const double Horizontal = -3.4E+38;
    public const int Skip = -2;
    public const int Unassigned = -1;
    public const double Tolerance = 1.0E-20;

    private const double Scale = GeometryHelper.PolygonScaleConstant;

    internal static bool Contains(PolygonLongPath subject, PolygonLongPath clip)
    {
        // The subjectPath polygon path contains the clipPath polygon path if:
        // 1. The union operation must result in one polygon result
        // 2. The area of the union equals subjectPath area.
        // 3. The area of the clipPath must be <= than the area of subjectPath.

        var solution = new PolygonLongPath();
        var clipper = new PolygonClipper();
        if (!clipper.Execute(ClipOperation.Union, subject, clip, solution))
        {
            return false;
        }

        if (solution.Count != 1)
        {
            return false;
        }

        if (!GeometryHelper.NearZero(subject.Area - solution.Area))
        {
            return false;
        }

        return clip.Area <= subject.Area;
    }

    internal static bool SimplifyPolygon<T>(PolygonLong polygon, T solution) where T : IClipSolution
    {
        return SimplifyPolygon(new PolygonLongPath(polygon), solution);
    }

    internal static bool SimplifyPolygon<T>(PolygonLongPath paths, T solution) where T : IClipSolution
    {
        var clipper = new PolygonClipper();
        return clipper.Execute(ClipOperation.Union, paths, null, solution, true);
    }

    internal static IList<PointDouble> GetBoundary(IList<PointDouble> path, double offset)
    {
        // Boundary is empty if the path is empty
        if (path.Count == 0)
        {
            return Array.Empty<PointDouble>();
        }

        // Create hull path around input path points
        var longPoints = path.Select(p => new PointLong(p * Scale)).ToList();
        var grahamsHull = new GrahamsHull();
        var hull = grahamsHull.GetHull(longPoints);

        // Offset path using clipper offset
        var pathLong = new PolygonLongPath([.. hull]);
        var clipperOffset = new ClipperOffset(0);
        clipperOffset.AddPaths(pathLong, JoinType.Square, EndType.ClosedLine);

        var solution = new PolygonLongPath();

        clipperOffset.Execute(ref solution, offset * Scale);
        if (solution.Count == 0)
        {
            return Array.Empty<PointDouble>();
        }

        // Scale down boundary points
        var boundaryPath = solution[0]
            .Select(p => new PointDouble(p.X / Scale, p.Y / Scale))
            .ToList();

        // Return boundary
        return boundaryPath;
    }
}
