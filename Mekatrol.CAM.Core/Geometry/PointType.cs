namespace Mekatrol.CAM.Core.Geometry;

/// <summary>
/// See: https://docs.microsoft.com/en-us/dotnet/api/system.drawing.drawing2d.graphicspath.pathtypes?view=dotnet-plat-ext-6.0
/// </summary>
[Flags]
public enum PointType : byte
{
    StartOfFigure = 0x00,
    LinePoint = 0x01,
    BezierPoint = 0x03,
    LowOrderMask = 0x07,
    Marker = 0x20,
    ClosePoint = 0x80
}
