using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack.Hosted
{
    public abstract class HostedCommandHandler<TCommand> : ICommandAsyncHandler<TCommand>,
                                                           ICommandHandler<TCommand> 
                                                           where TCommand : class, ICommand
    {
        /// <summary>
        /// Internal command source derived from CommandSource.
        /// </summary>
        private ICommandSource _internalCommandSource;

        /// <summary>
        /// Command source where command handler will subscribe to for commands.
        /// </summary>
        protected abstract ICommandSource CommandSource { get; }

        /// <summary>
        /// Start hosted command handler.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Completed asynchronous task.</returns>
        public virtual Task Start(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get an instance provided by child class and store in a private property. 
            _internalCommandSource = CommandSource;

            if(_internalCommandSource == null)
            {
                throw new InvalidOperationException("Hosted command handler has no command source.");
            }

            OnStart();

            // Subscribe.
            _internalCommandSource.CommandReceived += (receivedCommand, ct) =>
            {
                TCommand command = receivedCommand as TCommand;
                if(command != null)
                {
                    ProcessCommandAsync(command, cancellationToken);
                }

                return TaskUtility.CompletedTask;
            };

            _internalCommandSource.StartReceiving(cancellationToken);

            return TaskUtility.CompletedTask;
        }

        /// <summary>
        /// Stop hosted command handler.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task which can be awaited until the last received command has finished processing.</returns>
        public virtual Task Stop(CancellationToken cancellationToken = default(CancellationToken))
        {
            OnStop();

            return _internalCommandSource.StopReceiving(cancellationToken);
        }

        /// <summary>
        /// Process command asynchronously.
        /// </summary>
        /// <remarks>
        /// It is recommended to not let any exceptions exit this method because
        /// any exceptions that exits this method will not be handled.
        /// </remarks>
        /// <param name="receivedCommand">Command received.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        protected abstract Task ProcessCommandAsync(TCommand receivedCommand, CancellationToken cancellationToken);
        
        /// <summary>
        /// Handle command by putting it to the command source for asynchronous processing.
        /// </summary>
        /// <param name="command">Command to handle.</param>
        /// <param name="cancellationToken">Cancellation token. This is ignored.</param>
        /// <returns>Completed task.</returns>
        Task ICommandAsyncHandler<TCommand>.HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            _internalCommandSource.Receive(command, cancellationToken);
            return TaskUtility.CompletedTask;
        }

        /// <summary>
        /// Handle command by putting it to the command source for asynchronous processing.
        /// </summary>
        /// <param name="command">Command to handle.</param>
        void ICommandHandler<TCommand>.Handle(TCommand command)
        {
            _internalCommandSource.Receive(command);
        }

        /// <summary>
        /// Hook that is executed before hosted command handler is started.
        /// </summary>
        protected virtual void OnStart()
        {
        }

        /// <summary>
        /// Hook that is executed before hosted command handler is stopped.
        /// </summary>
        protected virtual void OnStop()
        {
        }
    }
}