namespace Mekatrol.CAM.Core.Clipping;

public interface IClipSolution
{
    SolutonType SolutionType { get; }

    void Clear();
}
