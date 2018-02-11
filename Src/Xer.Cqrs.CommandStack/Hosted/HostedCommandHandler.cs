using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Messaginator;

namespace Xer.Cqrs.CommandStack.Hosted
{
    public abstract class HostedCommandHandler : HostedCommandHandler<object>
    {
    }

    public abstract class HostedCommandHandler<TCommand> : MessageProcessor<TCommand>,
                                                           ICommandAsyncHandler<TCommand>,
                                                           ICommandHandler<TCommand> 
                                                           where TCommand : class
    {
        /// <summary>
        /// Handle command by putting it to the command source for asynchronous processing.
        /// </summary>
        /// <param name="command">Command to handle.</param>
        /// <param name="cancellationToken">Cancellation token. This is ignored.</param>
        /// <returns>Completed task.</returns>
        Task ICommandAsyncHandler<TCommand>.HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            return MessageSource.ReceiveAsync(command);
        }

        /// <summary>
        /// Handle command by putting it to the command source for asynchronous processing.
        /// </summary>
        /// <param name="command">Command to handle.</param>
        void ICommandHandler<TCommand>.Handle(TCommand command)
        {
            MessageSource.ReceiveAsync(command).GetAwaiter().GetResult();
        }
    }
}