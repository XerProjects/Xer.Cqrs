using System;
using Xer.Delegator;
using Xer.Delegator.Exceptions;

namespace Xer.Cqrs.CommandStack.Resolvers
{
    public class ContainerCommandAsyncHandlerResolver : IMessageHandlerResolver
    {
        private readonly IContainerAdapter _containerAdapter;

        public ContainerCommandAsyncHandlerResolver(IContainerAdapter containerAdapter)
        {
            _containerAdapter = containerAdapter;
        }

        /// <summary>
        /// <para>Resolves an instance of ICommandAsyncHandler<TCommand> from the container</para>
        /// <para>and converts it to a command handler delegate which can be invoked to process the command.</para>
        /// </summary>
        /// <typeparamref name="TCommand">Type of command which is handled by the command handler.</typeparamref>
        /// <returns>Instance of <see cref="CommandHandlerDelegate"/> which executes the command handler processing.</returns>
        public MessageHandlerDelegate<TCommand> ResolveMessageHandler<TCommand>() where TCommand : class
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