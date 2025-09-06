using Mekatrol.CAM.Core.Geometry.Entities;

namespace Mekatrol.CAM.Core.Data;

public sealed record DataSnapshot(IReadOnlyList<IGeometricEntity> Entities);

public interface IDataStore
{
    IObservable<DataSnapshot> Snapshot { get; }

    Task UpdateDataAsync(IReadOnlyList<IGeometricEntity> entities, CancellationToken cancellationToken = default);
}
