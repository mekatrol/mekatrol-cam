using Mekatrol.CAM.Core.Geometry.Entities;

namespace Mekatrol.CAM.Core.Parsers.Svg;

public interface IGeometryParser
{
    IReadOnlyList<IGeometricEntity> Parse(StreamReader stream, bool translateToZero = false);
}

