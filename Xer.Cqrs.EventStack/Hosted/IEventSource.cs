using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.EventStack.Hosted
{
    public interface IEventSource
    {
        event EventHandlerDelegate EventReceived;

        Task StartReceiving(CancellationToken cancellationToken = default(CancellationToken));
        Task StopReceiving(CancellationToken cancellationToken = default(CancellationToken));
        void Receive(IEvent @event, CancellationToken cancellationToken = default(CancellationToken));
    }
}