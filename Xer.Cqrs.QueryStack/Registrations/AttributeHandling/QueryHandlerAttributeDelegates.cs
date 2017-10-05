using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.QueryStack;

namespace Xer.Cqrs.QueryStack.Registrations.AttributeHandling
{
    /// <summary>
    /// Delatate to handle command.
    /// </summary>
    internal delegate void AttributedCommandHandlerDelegate<TAttributed, TCommand>(TAttributed attributedInstance, TCommand command);

    /// <summary>
    /// Delatate to handle command.
    /// </summary>
    internal delegate Task AttributedCommandAsyncHandlerDelegate<TAttributed, TCommand>(TAttributed attributedInstance, TCommand command);

    /// <summary>
    /// Delatate to handle command.
    /// </summary>
    internal delegate Task AttributedCommandAsyncHandlerCancellableDelegate<TAttributed, TCommand>(TAttributed attributedInstance, TCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Delegate to handle queries.
    /// </summary>
    internal delegate TResult AttributedQueryHandlerDelegate<TAttributed, TQuery, TResult>(TAttributed attributedInstance, TQuery query) where TQuery : IQuery<TResult>;

    /// <summary>
    /// Delegate to handle queries.
    /// </summary>
    internal delegate Task<TResult> AttributedQueryAsyncHandlerDelegate<TAttributed, TQuery, TResult>(TAttributed attributedInstance, TQuery query) where TQuery : IQuery<TResult>;

    /// <summary>
    /// Delegate to handle queries.
    /// </summary>
    internal delegate Task<TResult> AttributedQueryAsyncHandlerCancellableDelegate<TAttributed, TQuery, TResult>(TAttributed attributedInstance, TQuery query, CancellationToken cancellationToken = default(CancellationToken)) where TQuery : IQuery<TResult>;
}
