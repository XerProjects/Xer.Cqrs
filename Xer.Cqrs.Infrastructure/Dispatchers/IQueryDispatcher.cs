using Xer.Cqrs.Infrastructure.Queries;
using Xer.Cqrs.Queries;

namespace Xer.Cqrs.Infrastructure.Dispatchers
{
    /// <summary>
    /// Process queries and delegates to handlers.
    /// </summary>
    public interface IQueryDispatcher
    {
        TResult Dispatch<TResult>(IQuery<TResult> query);
        void RegisterHandler<TQuery, TResult>(IQueryHandler<TQuery, TResult> handler) where TQuery : IQuery<TResult>;
    }
}
