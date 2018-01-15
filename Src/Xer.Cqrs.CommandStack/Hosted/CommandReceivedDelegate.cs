using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack.Hosted
{
    public delegate Task CommandReceivedDelegate<TCommand>(TCommand command, CancellationToken cancellationToken = default(CancellationToken));
}