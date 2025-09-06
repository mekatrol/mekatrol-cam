using Mekatrol.CAM.Core.Geometry.Entities;

namespace Mekatrol.CAM.Core.Parsers.Svg;

internal class CssClass(string name)
{
    public string Name { get; set; } = name;

    public FontDescription? Font { get; set; }
}
