namespace Xer.Cqrs
{
    /// <summary>
    /// Process queries and delegates to handlers.
    /// </summary>
    public interface IQueryDispatcher
    {
        TResult Dispatch<TResult>(IQuery<TResult> query) where TResult : class;
    }
}
