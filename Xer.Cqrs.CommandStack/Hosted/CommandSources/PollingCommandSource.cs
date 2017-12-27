using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack.Hosted.CommandSources
{
    public abstract class PollingCommandSource : ICommandSource
    {
        private Task _pollingTask;

        /// <summary>
        /// Command source's polling state.
        /// </summary>
        protected PollingState State { get; private set; } = PollingState.Unstarted;

        /// <summary>
        /// Determine if polling should stop.
        /// </summary>
        protected virtual bool IsTimeToStop => State == PollingState.Stopped;

        /// <summary>
        /// Polling interval.
        /// </summary>
        protected abstract TimeSpan Interval { get; }

        /// <summary>
        /// Event where received commands are published.
        /// </summary>
        public event CommandHandlerDelegate CommandReceived;

        /// <summary>
        /// Start receiving commands from the source.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Completed asynchronous task.</returns>
        public Task StartReceiving(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (State != PollingState.Started)
            { 
                State = PollingState.Started;

                OnStart();

                _pollingTask = StartPolling(cancellationToken);
            }
            
            return TaskUtility.CompletedTask;
        }

        /// <summary>
        /// Stop receiving any commands from the source.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task which can be awaited until the last received command has finished processing.</returns>
        public Task StopReceiving(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Only change to Stopped state if command source has been started.
            if (State == PollingState.Started)
            {
                State = PollingState.Stopped;

                OnStop();
                CommandReceived = null;
            }
                        
            // Return polling task so that caller can await
            // until the last received command has finished processing.
            return _pollingTask;
        }

        /// <summary>
        /// Receive command and publish for processing.
        /// </summary>
        /// <param name="command">Command to receive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public void Receive(ICommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            OnCommandReceived(command, cancellationToken);
        }

        /// <summary>
        /// Get next command from the source.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        protected abstract Task<ICommand> GetNextCommandAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Start polling the source for any commands.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        protected virtual async Task StartPolling(CancellationToken cancellationToken)
        {
            while (!IsTimeToStop && !cancellationToken.IsCancellationRequested)
            {
                ICommand receivedCommand = await GetNextCommandAsync(cancellationToken).ConfigureAwait(false);
                if (receivedCommand != null)
                {
                    OnCommandReceived(receivedCommand, cancellationToken);
                }

                if (!IsTimeToStop && !cancellationToken.IsCancellationRequested)
                {
                    // Only delay if not cancelled or stopped at this point.
                    await Task.Delay(Interval, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Hook that is executed before command source starts receiving any commands.
        /// </summary>
        protected virtual void OnStart()
        {
        }

        /// <summary>
        /// Hook that is executed before command source stops receiving any commands.
        /// </summary>
        protected virtual void OnStop()
        {
        }

        /// <summary>
        /// Publishes received command.
        /// </summary>
        /// <param name="receivedCommand">Received command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        private void OnCommandReceived(ICommand receivedCommand, CancellationToken cancellationToken)
        {
            if(CommandReceived != null)
            {
                CommandReceived(receivedCommand, cancellationToken);
            }
        }

        protected enum PollingState
        {
            Unstarted,
            Started,
            Stopped
        }
    }
}