using Mekatrol.CAM.Core.Render;

namespace Mekatrol.CAM.Core.Geometry.Entities;

/// <summary>
/// Create a fond description
/// </summary>
/// <param name="familyName">The font family name</param>
/// <param name="size">The font size (in mm)</param>
/// <param name="style">The font styles (if any)</param>
internal class FontDescription(string familyName, float size, FontStyle style)
{
    internal static readonly FontDescription Default = new("Microsoft Sans Serif", 8.25f, FontStyle.Regular);

    public string FamilyName { get; set; } = familyName;

    public float Size { get; set; } = size;

    public FontStyle Style { get; set; } = style;

    public override string ToString()
    {
        return $"{FamilyName},{Style},{Size}";
    }
}
