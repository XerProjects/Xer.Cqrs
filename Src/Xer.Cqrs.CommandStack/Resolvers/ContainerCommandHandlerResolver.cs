using System;
using System.Threading.Tasks;
using Xer.Delegator;
using Xer.Delegator.Exceptions;

namespace Xer.Cqrs.CommandStack.Resolvers
{
    public class ContainerCommandHandlerResolver : IMessageHandlerResolver
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
        public MessageHandlerDelegate<TCommand> ResolveMessageHandler<TCommand>() where TCommand : class
        {
            try
            {
                // Try to resolve sync handler next, if no async handler is found.
                ICommandHandler<TCommand> commandHandler = _containerAdapter.Resolve<ICommandHandler<TCommand>>();

                if (commandHandler != null)
                {
                    return CommandHandlerDelegateBuilder.FromCommandHandler(commandHandler);
                }
            }
            catch(Exception ex)
            {
                // Exception while resolving handler. Throw exception.
                throw new NoMessageHandlerResolvedException($"Error occurred while trying to resolve message handler for { typeof(TCommand).Name }.", typeof(TCommand), ex);
            }

            // No handlers are resolved. Throw exception.
            throw new NoMessageHandlerResolvedException($"Unable to resolve message handler for { typeof(TCommand).Name }.", typeof(TCommand));
        }
    }
}
