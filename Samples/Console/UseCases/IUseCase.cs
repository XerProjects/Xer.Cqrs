using System.Threading;
using System.Threading.Tasks;

namespace Console.UseCases
{
    public interface IUseCase
    {
        string Name { get; }
        Task ExecuteAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}