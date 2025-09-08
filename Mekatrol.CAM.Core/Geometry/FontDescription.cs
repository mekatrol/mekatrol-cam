using Avalonia.Media;

namespace Mekatrol.CAM.Core.Geometry;

/// <summary>
/// Create a fond description
/// </summary>
/// <param name="familyName">The font family name</param>
/// <param name="size">The font size (in mm)</param>
/// <param name="style">The font styles (if any)</param>
public class FontDescription(string familyName, float size, FontStyle style, FontWeight weight)
{
    internal static readonly FontDescription Default = new("Microsoft Sans Serif", 8.25f, FontStyle.Normal, FontWeight.Normal);

    public string FamilyName { get; set; } = familyName;

    public float Size { get; set; } = size;

    public FontStyle Style { get; set; } = style;

    public FontWeight Weight { get; set; } = weight;

    public override string ToString()
    {
        return $"{FamilyName},{Style},{Size}";
    }
}
