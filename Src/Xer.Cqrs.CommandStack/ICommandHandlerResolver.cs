using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack
{
    public interface ICommandHandlerResolver
    {
        /// <summary>
        /// Get the registered command handler delegate which handles the command of the specified type.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <returns>Instance of <see cref="CommandHandlerDelegate"/> which executes the command handler processing.</returns>
        CommandHandlerDelegate ResolveCommandHandler<TCommand>() where TCommand : class, ICommand;
    }

    /// <summary>
    /// Delegate to handle command.
    /// </summary>
    /// <param name="command">Command to handle.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <return>Asynchronous task which completes after the command handler has processed the command.</return>
    public delegate Task CommandHandlerDelegate(ICommand command, CancellationToken cancellationToken = default(CancellationToken));
}
