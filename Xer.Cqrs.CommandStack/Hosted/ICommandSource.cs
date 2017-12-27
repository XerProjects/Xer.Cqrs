using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack.Hosted
{
    public interface ICommandSource
    {
        event CommandHandlerDelegate CommandReceived;

        Task StartReceiving(CancellationToken cancellationToken = default(CancellationToken));
        Task StopReceiving(CancellationToken cancellationToken = default(CancellationToken));
        void Receive(ICommand command, CancellationToken cancellationToken = default(CancellationToken));
    }
}