using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xer.Cqrs.Registrations.CommandHandlers
{
    public partial class CommandHandlerFactoryRegistration : ICommandHandlerProvider, ICommandHandlerFactoryRegistration
    {
        #region Declarations

        private readonly IDictionary<Type, CommandAsyncHandlerDelegate> _commandHandlerDelegatesByCommandType = new Dictionary<Type, CommandAsyncHandlerDelegate>();

        #endregion Declarations

        #region ICommandAsyncHandlerFactoryRegistration Implementation

        /// <summary>
        /// Register command handler.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <param name="commandHandlerFactory">Synchronous handler which can process the command.</param>
        public void Register<TCommand>(Func<ICommandHandler<TCommand>> commandHandlerFactory) where TCommand : ICommand
        {
            Type commandType = typeof(TCommand);

            CommandAsyncHandlerDelegate handleCommandDelegate;

            if (_commandHandlerDelegatesByCommandType.TryGetValue(commandType, out handleCommandDelegate))
            {
                throw new InvalidOperationException($"Duplicate command async handler registered for {commandType.Name}.");
            }

            CommandAsyncHandlerDelegate newHandleCommandDelegate = (c, ct) =>
            {
                ICommandHandler<TCommand> commandHandlerInstance = commandHandlerFactory.Invoke();

                if (commandHandlerInstance == null)
                {
                    throw new InvalidOperationException($"Failed to create a command handler instance for {c.GetType().Name}");
                }

                commandHandlerInstance.Handle((TCommand)c);

                return TaskUtility.CompletedTask;
            };

            _commandHandlerDelegatesByCommandType.Add(commandType, newHandleCommandDelegate);
        }

        /// <summary>
        /// Register command async handler.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <param name="commandAsyncHandlerFactory">Asynchronous handler which can process the command.</param>
        public void Register<TCommand>(Func<ICommandAsyncHandler<TCommand>> commandAsyncHandlerFactory) where TCommand : ICommand
        {
            Type commandType = typeof(TCommand);

            CommandAsyncHandlerDelegate handleCommandDelegate;

            if (_commandHandlerDelegatesByCommandType.TryGetValue(commandType, out handleCommandDelegate))
            {
                throw new InvalidOperationException($"Duplicate command async handler registered for {commandType.Name}.");
            }

            CommandAsyncHandlerDelegate newHandleCommandDelegate = (c, ct) =>
            {
                ICommandAsyncHandler<TCommand> commandHandlerInstance = commandAsyncHandlerFactory.Invoke();

                if (commandHandlerInstance == null)
                {
                    throw new InvalidOperationException($"Failed to create a command handler instance for {c.GetType().Name}");
                }

                return commandHandlerInstance.HandleAsync((TCommand)c, ct);
            };

            _commandHandlerDelegatesByCommandType.Add(commandType, newHandleCommandDelegate);
        }

        #endregion ICommandAsyncHandlerFactoryRegistration Implementation

        #region ICommandAsyncHandlerProvider Implementation

        /// <summary>
        /// Get the registered command handler delegate to handle the command of the specified type.
        /// </summary>
        /// <param name="commandType">Type of command to be handled.</param>
        /// <returns>Instance of invokeable CommandAsyncHandlerDelegate.</returns>
        public CommandAsyncHandlerDelegate GetCommandHandler(Type commandType)
        {
            CommandAsyncHandlerDelegate handleCommandDelegate;

            if (!_commandHandlerDelegatesByCommandType.TryGetValue(commandType, out handleCommandDelegate))
            {
                throw new HandlerNotFoundException($"No command handler is registered to handle command of type: { commandType.Name }");
            }

            return handleCommandDelegate;
        }

        #endregion ICommandAsyncHandlerProvider Implementation
    }
}
