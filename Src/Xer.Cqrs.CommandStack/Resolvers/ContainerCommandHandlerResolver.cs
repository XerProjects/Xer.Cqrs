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

        /// <summary>
        /// <para>Resolves an instance of ICommandHandler<TCommand> from the container</para>
        /// <para>and converts it to a command handler delegate which can be invoked to process the command.</para>
        /// </summary>
        /// <typeparamref name="TCommand">Type of command which is handled by the command handler.</typeparamref>
        /// <returns>Instance of <see cref="CommandHandlerDelegate"/> which executes the command handler processing.</returns>
        public CommandHandlerDelegate ResolveCommandHandler<TCommand>() where TCommand : class, ICommand
        {
            try
            {
                // Try to resolve sync handler next, if no async handler is found.
                ICommandHandler<TCommand> commandHandler = _containerAdapter.Resolve<ICommandHandler<TCommand>>();

                if (commandHandler == null)
                {
                    // No handlers are resolved. Throw exception.
                    throw ExceptionBuilder.NoCommandHandlerResolvedException(typeof(TCommand));
                }

                return CommandHandlerDelegateBuilder.FromCommandHandler(commandHandler);
            }
            catch(Exception ex)
            {
                // No handlers are resolved. Throw exception.
                throw ExceptionBuilder.NoCommandHandlerResolvedException(typeof(TCommand), ex);
            }
        }
    }
}
