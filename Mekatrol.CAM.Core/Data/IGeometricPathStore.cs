using Mekatrol.CAM.Core.Geometry.Entities;

namespace Mekatrol.CAM.Core.Data;

public sealed record GeometricPathDataSnapshot(IGeometricPathEntity Path);

public interface IGeometricPathStore
{
    IObservable<GeometricPathDataSnapshot> Snapshot { get; }

    Task UpdateGeometricPath(IGeometricPathEntity path, CancellationToken cancellationToken = default);
}
