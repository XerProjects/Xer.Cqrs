using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack.Registrations.AttributeHandling
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
}
