namespace Mekatrol.CAM.Core.Clipping;

internal struct RectangleLong
{
    public long Left;
    public long Top;
    public long Right;
    public long Bottom;

    public RectangleLong(long left, long top, long right, long bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public RectangleLong(RectangleLong rect)
    {
        Left = rect.Left;
        Top = rect.Top;
        Right = rect.Right;
        Bottom = rect.Bottom;
    }
}
