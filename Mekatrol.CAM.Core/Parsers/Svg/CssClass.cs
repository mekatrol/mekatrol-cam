using Mekatrol.CAM.Core.Geometry;

namespace Mekatrol.CAM.Core.Parsers.Svg;

public class CssClass(string name)
{
    public string Name { get; set; } = name;

    public FontDescription? Font { get; set; }
}
