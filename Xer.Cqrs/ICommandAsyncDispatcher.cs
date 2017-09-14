using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs
{
    public interface ICommandAsyncDispatcher
    {
        Task DispatchAsync(ICommand command, CancellationToken cancellationToken = default(CancellationToken));
    }
}
