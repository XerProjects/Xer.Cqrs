using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.Events
{
    public interface IEventAsyncHandler<TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Make an operation based on the event asynchronously.
        /// </summary>
        /// <param name="event">Event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default(CancellationToken));
    }
}
