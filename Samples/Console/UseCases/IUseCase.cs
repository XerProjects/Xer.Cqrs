using System.Threading;
using System.Threading.Tasks;

namespace Console.UseCases
{
    public interface IUseCase
    {
        Task ExecuteAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}