using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack
{
    public interface ICommandHandlerResolver
    {
        /// <summary>
        /// Get the registered command handler delegate to handle the command of the specified type.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <returns>Instance of invokeable CommandHandlerDelegate.</returns>
        CommandHandlerDelegate ResolveCommandHandler<TCommand>() where TCommand : class, ICommand;
    }

    /// <summary>
    /// Delegate to handle command.
    /// </summary>
    /// <param name="command">Command to handle.</param>
    public delegate Task CommandHandlerDelegate(ICommand command, CancellationToken cancellationToken = default(CancellationToken));
}
