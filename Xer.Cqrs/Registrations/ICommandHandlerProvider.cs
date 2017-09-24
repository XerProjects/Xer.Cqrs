using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.Registrations
{
    public interface ICommandHandlerProvider
    {
        /// <summary>
        /// Get the registered command handler delegate to handle the command of the specified type.
        /// </summary>
        /// <param name="commandType">Type of command to be handled.</param>
        /// <returns>Instance of invokeable CommandAsyncHandlerDelegate.</returns>
        CommandAsyncHandlerDelegate GetCommandHandler(Type commandType);
    }

    /// <summary>
    /// Delatate to handle command.
    /// </summary>
    /// <param name="command">Command to handle.</param>
    public delegate Task CommandAsyncHandlerDelegate(ICommand command, CancellationToken cancellationToken = default(CancellationToken));
}
