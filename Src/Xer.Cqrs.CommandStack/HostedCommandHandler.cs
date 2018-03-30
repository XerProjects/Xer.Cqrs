using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Messaginator;

namespace Xer.Cqrs.CommandStack
{
    public abstract class HostedCommandHandler : HostedCommandHandler<object>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messageSource">Source of message.</param>
        public HostedCommandHandler(IMessageSource<object> messageSource) : base(messageSource)
        {
        }
    }

    public abstract class HostedCommandHandler<TCommand> : MessageProcessor<TCommand>,
                                                           ICommandAsyncHandler<TCommand>,
                                                           ICommandHandler<TCommand> 
                                                           where TCommand : class
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messageSource">Source of message.</param>
        public HostedCommandHandler(IMessageSource<TCommand> messageSource) : base(messageSource)
        {
        }

        /// <summary>
        /// Handle command by putting it to the command source for asynchronous processing.
        /// </summary>
        /// <param name="command">Command to handle.</param>
        /// <param name="cancellationToken">Cancellation token. This is ignored.</param>
        /// <returns>Completed task.</returns>
        Task ICommandAsyncHandler<TCommand>.HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            return MessageSource.ReceiveAsync(new MessageContainer<TCommand>(command));
        }

        /// <summary>
        /// Handle command by putting it to the command source for asynchronous processing.
        /// </summary>
        /// <param name="command">Command to handle.</param>
        void ICommandHandler<TCommand>.Handle(TCommand command)
        {
            MessageSource.ReceiveAsync(new MessageContainer<TCommand>(command)).GetAwaiter().GetResult();
        }
    }
}