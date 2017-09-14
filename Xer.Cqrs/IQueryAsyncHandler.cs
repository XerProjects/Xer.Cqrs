using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs
{
    public interface IQueryAsyncHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
                                                            where TResult : class
    {
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default(CancellationToken));
    }
}
