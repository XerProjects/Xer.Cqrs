using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs
{
    public interface ICommandAsyncHandler<TCommand> where TCommand : ICommand
    {
        Task HandleAsync(TCommand command, CancellationToken cancellationToken = default(CancellationToken));
    }
}
