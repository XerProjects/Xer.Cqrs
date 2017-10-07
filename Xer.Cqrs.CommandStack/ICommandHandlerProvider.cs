using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack
{
    public interface ICommandHandlerProvider
    {
        /// <summary>
        /// Get the registered command handler delegate to handle the command of the specified type.
        /// </summary>
        /// <param name="commandType">Type of command to be handled.</param>
        /// <returns>Instance of invokeable CommandAsyncHandlerDelegate.</returns>
        CommandHandlerDelegate GetCommandHandler(Type commandType);
    }

    /// <summary>
    /// Delatate to handle command.
    /// </summary>
    /// <param name="command">Command to handle.</param>
    public delegate Task CommandHandlerDelegate(ICommand command, CancellationToken cancellationToken = default(CancellationToken));
}
