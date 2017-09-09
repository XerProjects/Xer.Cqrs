namespace Xer.Cqrs
{
    /// <summary>
    /// Process queries and delegates to handlers.
    /// </summary>
    public interface IQueryDispatcher
    {
        TResult Dispatch<TResult>(IQuery<TResult> query) where TResult : class;
        void RegisterHandler<TQuery, TResult>(IQueryHandler<TQuery, TResult> handler) where TQuery : IQuery<TResult>
                                                                                      where TResult : class;
    }
}
