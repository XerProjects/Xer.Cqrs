using System;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack.Resolvers
{
    public class ContainerCommandHandlerResolver : ICommandHandlerResolver
    {
        private readonly IContainerAdapter _containerAdapter;

        public ContainerCommandHandlerResolver(IContainerAdapter containerAdapter)
        {
            _containerAdapter = containerAdapter;
        }

        public CommandHandlerDelegate ResolveCommandHandler<TCommand>() where TCommand : ICommand
        {
            // Try to resolve async handler first.
            ICommandAsyncHandler<TCommand> commandAsyncHandler = _containerAdapter.Resolve<ICommandAsyncHandler<TCommand>>();

            if (commandAsyncHandler != null)
            {
                return new CommandHandlerDelegate((c, ct) =>
                {
                    return commandAsyncHandler.HandleAsync((TCommand)c, ct);
                });
            }

            // Try to resolve sync handler next, if no async handler is found.
            ICommandHandler<TCommand> commandHandler = _containerAdapter.Resolve<ICommandHandler<TCommand>>();

            if (commandHandler != null)
            {
                return new CommandHandlerDelegate((c, ct) =>
                {
                    commandHandler.Handle((TCommand)c);

                    return TaskUtility.CompletedTask;
                });
            }

            throw new CommandNotHandledException($"No command handler registered in the container to handle command of type: { typeof(Command).Name }.");
        }
    }

    public interface IContainerAdapter
    {
        /// <summary>
        /// Resolve instance from a container.
        /// </summary>
        /// <typeparam name="T">Type of object to resolve from container.</typeparam>
        /// <returns>Instance of requested object.</returns>
        T Resolve<T>();
    }
}
