namespace Mekatrol.CAM.Core.Converter;

public static class UnitSizeConverter
{
    public const float DefaultFontSize = 4.233f; // CSS default 16px ≈ 4.233 mm

    public static float ConvertGraphicSizeToMM(float size, string unit, float? currentFontSizeMm = null) =>
        unit.ToLower().Trim() switch
        {
            "" => size * 25.4f / 96.0f, // unitless is same as px
            "px" => size * 25.4f / 96.0f,
            "in" => size * 25.4f,
            "cm" => size * 10f,
            "mm" => size,
            "pt" => size * 25.4f / 72.0f,
            "pc" => size * 12f * 25.4f / 72.0f,
            "em" => size * (currentFontSizeMm ?? DefaultFontSize),
            "%" => (size / 100.0f) * (currentFontSizeMm ?? DefaultFontSize),
            _ => size,
        };

    public static float ConvertMMToGraphicSize(float mm, string unit, float? currentFontSizeMm = null) =>
        unit.ToLower().Trim() switch
        {
            "" => mm * 96.0f / 25.4f, // unitless is same as px
            "px" => mm * 96.0f / 25.4f,
            "in" => mm / 25.4f,
            "cm" => mm / 10.0f,
            "mm" => mm,
            "pt" => mm * 72.0f / 25.4f,
            "pc" => mm * 72.0f / (25.4f * 12.0f),
            "em" => mm / (currentFontSizeMm ?? DefaultFontSize),
            "%" => (mm / (currentFontSizeMm ?? DefaultFontSize)) * 100.0f,
            _ => mm,
        };
}
