using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.QueryStack.Projections
{
    public interface IProjection<TId, TViewModel> where TViewModel : class
    {
        TId ProjectionId { get; }
        DateTime LastUpdated { get; }

        Task UpdateAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<TViewModel> GetAsync(TId projectionId, CancellationToken cancellationToken = default(CancellationToken));
    }
}
