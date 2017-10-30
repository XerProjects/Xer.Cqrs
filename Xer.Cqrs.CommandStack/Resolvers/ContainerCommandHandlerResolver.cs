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

        public CommandHandlerDelegate ResolveCommandHandler<TCommand>() where TCommand : class, ICommand
        {
            try
            {
                // Try to resolve async handler first.
                ICommandAsyncHandler<TCommand> commandAsyncHandler = _containerAdapter.Resolve<ICommandAsyncHandler<TCommand>>();

                if (commandAsyncHandler != null)
                {
                    return CommandHandlerDelegateBuilder.FromCommandHandler(commandAsyncHandler);
                }
            }
            catch(Exception)
            {
                // Do nothing.
                // Some containers may throw exception when no instance is resolved.
            }

            try
            {
                // Try to resolve sync handler next, if no async handler is found.
                ICommandHandler<TCommand> commandHandler = _containerAdapter.Resolve<ICommandHandler<TCommand>>();

                if (commandHandler != null)
                {
                    return CommandHandlerDelegateBuilder.FromCommandHandler(commandHandler);
                }
            }
            catch(Exception)
            {
                // Do nothing.
                // Some containers may throw exception when no instance is resolved.
            }

            // No handlers are resolved. Throw exception.
            throw new CommandNotHandledException($"Unable to resolve a command handler from the container to handle command of type: { typeof(Command).Name }.");
        }
    }

    public interface IContainerAdapter
    {
        /// <summary>
        /// Resolve instance from a container.
        /// </summary>
        /// <typeparam name="T">Type of object to resolve from container.</typeparam>
        /// <returns>Instance of requested object.</returns>
        T Resolve<T>() where T : class;
    }
}
