using System.Threading;
using System.Threading.Tasks;

namespace Xer.Worker.Async
{
    public interface IAsyncWork
    {
        Task ExecuteAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
