using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.EventStack.Hosted.EventSources
{
    public abstract class PollingEventSource : IEventSource
    {
        private CancellationToken _receiveCancellationToken;
        private Task _pollingTask;

        /// <summary>
        /// Event source's state.
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
        /// Event where received events are published.
        /// </summary>
        public event EventHandlerDelegate EventReceived;

        /// <summary>
        /// Start receiving events from the source.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Completed asynchronous task.</returns>
        public Task StartReceiving(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (State != PollingState.Started)
            {          
                State = PollingState.Started;

                OnStart();

                _receiveCancellationToken = cancellationToken;
                _pollingTask = StartPolling(cancellationToken);
            }

            return TaskUtility.CompletedTask;
        }

        /// <summary>
        /// Stop receiving any events from the source.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task which can be awaited until the last received event has finished processing.</returns>
        public Task StopReceiving(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Only change to Stopped state if event source has been started.
            if (State == PollingState.Started)
            {
                State = PollingState.Stopped;

                OnStop();
                EventReceived = null;
            }

            // Return polling task so that caller can await
            // until the last received event has finished processing.
            return _pollingTask;
        }

        /// <summary>
        /// Receive event and publish for processing.
        /// </summary>
        /// <param name="@event">Event to receive.</param>
        /// <returns>Asynchronous task.</returns>
        public Task Receive(IEvent @event)
        {
            if (@event == null)
            {
                return TaskUtility.FromException(new ArgumentNullException(nameof(@event)));
            }

            // Cancel this when the cancellation token passed in the 
            // StartReceiving method is cancelled. 
            OnEventReceived(@event, _receiveCancellationToken);

            return TaskUtility.CompletedTask;
        }

        /// <summary>
        /// Get next event from the source.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        protected abstract Task<IEvent> GetNextEventAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Start polling the source for any events.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        protected virtual async Task StartPolling(CancellationToken cancellationToken)
        {
            while (!IsTimeToStop && !cancellationToken.IsCancellationRequested)
            {
                IEvent receivedEvent = await GetNextEventAsync(cancellationToken).ConfigureAwait(false);
                if (receivedEvent != null)
                {
                    OnEventReceived(receivedEvent, cancellationToken);
                }

                if (!IsTimeToStop && !cancellationToken.IsCancellationRequested)
                {
                    // Only delay if not cancelled or stopped at this point.
                    await Task.Delay(Interval, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Hook that is executed before event source starts receiving any events.
        /// </summary>
        protected virtual void OnStart()
        {
        }

        /// <summary>
        /// Hook that is executed before event source stops receiving any events.
        /// </summary>
        protected virtual void OnStop()
        {
        }

        /// <summary>
        /// Publishes received event.
        /// </summary>
        /// <param name="receivedEvent">Received event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        private void OnEventReceived(IEvent receivedEvent, CancellationToken cancellationToken)
        {
            if(EventReceived != null)
            {
                EventReceived(receivedEvent, cancellationToken);
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