using Mekatrol.CAM.Core.Geometry.Entities;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Mekatrol.CAM.Core.Data;

public sealed class DataStore : IDataStore, IDisposable
{
    private readonly BehaviorSubject<DataSnapshot> _snap = new(new DataSnapshot([]));

    public IObservable<DataSnapshot> Snapshot => _snap.AsObservable();

    public Task UpdateDataAsync(IReadOnlyList<IGeometricEntity> entities, CancellationToken cancellationToken = default)
    {
        var snapshot = new DataSnapshot(entities?.ToArray() ?? []);
        _snap.OnNext(snapshot);
        return Task.CompletedTask;
    }

    public void Dispose() => _snap.Dispose();
}
