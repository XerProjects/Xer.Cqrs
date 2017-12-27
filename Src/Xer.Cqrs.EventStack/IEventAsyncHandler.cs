using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.EventStack
{
    public interface IEventAsyncHandler<TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Make an operation based on the event asynchronously.
        /// </summary>
        /// <param name="event">Event to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task which completes after processing the event.</returns>
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default(CancellationToken));
    }
}
