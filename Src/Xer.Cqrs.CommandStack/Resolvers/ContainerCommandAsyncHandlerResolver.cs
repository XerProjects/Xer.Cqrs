using System;

namespace Xer.Cqrs.CommandStack.Resolvers
{
    public class ContainerCommandAsyncHandlerResolver : ICommandHandlerResolver
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
        public CommandHandlerDelegate ResolveCommandHandler<TCommand>() where TCommand : class, ICommand
        {
            try
            {
                // Try to resolve async handler first.
                ICommandAsyncHandler<TCommand> commandAsyncHandler = _containerAdapter.Resolve<ICommandAsyncHandler<TCommand>>();

                if (commandAsyncHandler == null)
                {
                    // No handlers are resolved. Throw exception.
                    throw ExceptionBuilder.NoCommandHandlerResolvedException(typeof(TCommand));
                }

                return CommandHandlerDelegateBuilder.FromCommandHandler(commandAsyncHandler);
            }
            catch(Exception ex)
            {
                // No handlers are resolved. Throw exception.
                throw ExceptionBuilder.NoCommandHandlerResolvedException(typeof(TCommand), ex);
            }
        }
    }
}