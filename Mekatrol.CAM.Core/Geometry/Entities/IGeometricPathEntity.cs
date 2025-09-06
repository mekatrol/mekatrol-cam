namespace Mekatrol.CAM.Core.Geometry.Entities;

public interface IGeometricPathEntity : IGeometricEntity
{
    /// <summary>
    /// True is the path is closed
    /// </summary>
    bool IsClosed { get; }

    IList<IGeometricEntity> Entities { get; }
}
