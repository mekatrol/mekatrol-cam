using Mekatrol.CAM.Core.Geometry.Entities;

namespace Mekatrol.CAM.Core.Parsers.Svg;

public interface IGeometryParser
{
    IGeometricPathEntity Parse(StreamReader stream, bool translateToZero = false);
}

