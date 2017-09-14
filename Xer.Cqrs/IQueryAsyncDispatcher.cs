using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs
{
    public interface IQueryAsyncDispatcher
    {
        Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default(CancellationToken)) where TResult : class;
    }
}
