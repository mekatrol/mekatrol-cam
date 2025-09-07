using Mekatrol.CAM.Core.Geometry.Entities;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Mekatrol.CAM.Core.Data;

public sealed class GeometricPathStore : IGeometricPathStore, IDisposable
{
    private readonly BehaviorSubject<GeometricPathDataSnapshot> _snap = new(new GeometricPathDataSnapshot(new PathEntity()));

    public IObservable<GeometricPathDataSnapshot> Snapshot => _snap.AsObservable();

    public Task UpdateGeometricPath(IGeometricPathEntity path, CancellationToken cancellationToken = default)
    {
        var snapshot = new GeometricPathDataSnapshot(path ?? new PathEntity());
        _snap.OnNext(snapshot);
        return Task.CompletedTask;
    }

    public void Dispose() => _snap.Dispose();
}
