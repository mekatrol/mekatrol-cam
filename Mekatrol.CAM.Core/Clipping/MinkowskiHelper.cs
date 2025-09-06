using Mekatrol.CAM.Core.Geometry;

namespace Mekatrol.CAM.Core.Clipping;

internal static class MinkowskiHelper
{
    public static PolygonLongPath Minkowski(PolygonLong pattern, PolygonLong path, bool isSum, bool isClosed)
    {
        var delta = isClosed ? 1 : 0;
        var patternCount = pattern.Count;
        var pathCount = path.Count;
        var solution = new PolygonLongPath(pathCount);

        if (isSum)
        {
            for (var i = 0; i < pathCount; i++)
            {
                var polygon = new PolygonLong(patternCount);
                polygon.AddRange(
                    pattern.Select(p => new PointLong(path[i].X + p.X, path[i].Y + p.Y)));
                solution.Add(polygon);
            }
        }
        else
        {
            for (var i = 0; i < pathCount; i++)
            {
                var polygon = new PolygonLong(patternCount);
                polygon.AddRange(pattern.Select(p => new PointLong(path[i].X - p.X, path[i].Y - p.Y)));
                solution.Add(polygon);
            }
        }

        var quads = new PolygonLongPath((pathCount + delta) * (patternCount + 1));

        for (var i = 0; i < pathCount - 1 + delta; i++)
        {
            for (var j = 0; j < patternCount; j++)
            {
                var quad = new PolygonLong(4)
                {
                    solution[i % pathCount][j % patternCount],
                    solution[(i + 1) % pathCount][j % patternCount],
                    solution[(i + 1) % pathCount][(j + 1) % patternCount],
                    solution[i % pathCount][(j + 1) % patternCount]
                };

                if (quad.Orientation == PolygonOrientation.Clockwise)
                {
                    quad.Reverse();
                }
                quads.Add(quad);
            }
        }

        return quads;
    }

    public static PolygonLongPath MinkowskiSum(PolygonLong pattern, PolygonLong path, bool pathIsClosed)
    {
        var polygonPath = Minkowski(pattern, path, true, pathIsClosed);
        var clipper = new PolygonClipper();
        clipper.AddPath(polygonPath, PolygonKind.Subject);
        clipper.Execute(ClipOperation.Union, polygonPath, PolygonFillType.NonZero, PolygonFillType.NonZero);
        return polygonPath;
    }

    public static PolygonLongPath MinkowskiSum(PolygonLong pattern, PolygonLongPath paths, bool pathIsClosed)
    {
        var solution = new PolygonLongPath();
        var clipper = new PolygonClipper();

        foreach (var polygon in paths)
        {
            var tmp = Minkowski(pattern, polygon, true, pathIsClosed);
            clipper.AddPath(tmp, PolygonKind.Subject);
            if (!pathIsClosed)
            {
                continue;
            }

            var translated = polygon.Translated(pattern[0]);
            clipper.AddPath(translated, PolygonKind.Clip);
        }

        clipper.Execute(ClipOperation.Union, solution, PolygonFillType.NonZero, PolygonFillType.NonZero);

        return solution;
    }

    public static PolygonLongPath MinkowskiDiff(PolygonLong poly1, PolygonLong poly2)
    {
        var path = Minkowski(poly1, poly2, false, true);
        var clipper = new PolygonClipper();
        clipper.AddPath(path, PolygonKind.Subject);
        clipper.Execute(ClipOperation.Union, path, PolygonFillType.NonZero, PolygonFillType.NonZero);
        return path;
    }
}
