using Xer.Cqrs.Commands;
using Xer.Cqrs.Infrastructure.Commands;

namespace Xer.Cqrs.Infrastructure.Dispatchers
{
    /// <summary>
    /// Process commands and delegates to handlers.
    /// </summary>
    public interface ICommandDispatcher
    {
        void Dispatch(ICommand command);
        void RegisterHandler<TCommand>(ICommandHandler<TCommand> commandHandler) where TCommand : ICommand;
    }
}
