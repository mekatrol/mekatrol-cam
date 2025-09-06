namespace Mekatrol.CAM.Core.Parsers.Svg;

internal enum SvgPathCommand
{
    // Arc
    a, A,

    // Move
    m, M,

    // Line
    l, L, h, H, v, V,

    // Cubic Bezier
    c, C, s, S,

    // Quadratic Bezier
    q, Q, t, T,

    // Close path
    z, Z
}
