using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack.Registrations
{
    public partial class CommandHandlerRegistration : ICommandHandlerResolver, ICommandHandlerRegistration
    {
        #region Declarations

        private readonly IDictionary<Type, CommandHandlerDelegate> _commandHandlerDelegatesByCommandType = new Dictionary<Type, CommandHandlerDelegate>();

        #endregion Declarations

        #region ICommandAsyncHandlerFactoryRegistration Implementation

        /// <summary>
        /// Register command handler.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <param name="commandHandlerFactory">Factory which will provide an instance of a command handler that handles the specified <typeparamref name="TCommand"/> command.</param>
        public void Register<TCommand>(Func<ICommandHandler<TCommand>> commandHandlerFactory) where TCommand : class, ICommand
        {
            if (commandHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(commandHandlerFactory));
            }

            Type commandType = typeof(TCommand);

            CommandHandlerDelegate handleCommandDelegate;

            if (_commandHandlerDelegatesByCommandType.TryGetValue(commandType, out handleCommandDelegate))
            {
                throw new InvalidOperationException($"Duplicate command async handler registered for {commandType.Name}.");
            }

            // Create delegate.
            CommandHandlerDelegate newHandleCommandDelegate = CommandHandlerDelegateBuilder.FromFactory(commandHandlerFactory);

            _commandHandlerDelegatesByCommandType.Add(commandType, newHandleCommandDelegate);
        }

        /// <summary>
        /// Register command async handler.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <param name="commandAsyncHandlerFactory">Factory which will provide an instance of a command handler that handles the specified <typeparamref name="TCommand"/> command.</param>
        public void Register<TCommand>(Func<ICommandAsyncHandler<TCommand>> commandAsyncHandlerFactory) where TCommand : class, ICommand
        {
            if(commandAsyncHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(commandAsyncHandlerFactory));
            }

            Type commandType = typeof(TCommand);

            CommandHandlerDelegate handleCommandDelegate;

            if (_commandHandlerDelegatesByCommandType.TryGetValue(commandType, out handleCommandDelegate))
            {
                throw new InvalidOperationException($"Duplicate command async handler registered for {commandType.Name}.");
            }

            // Create delegate.
            CommandHandlerDelegate newHandleCommandDelegate = CommandHandlerDelegateBuilder.FromFactory(commandAsyncHandlerFactory);

            _commandHandlerDelegatesByCommandType.Add(commandType, newHandleCommandDelegate);
        }

        #endregion ICommandAsyncHandlerFactoryRegistration Implementation

        #region ICommandHandlerResolver Implementation

        /// <summary>
        /// Get the registered command handler delegate which handles the command of the specified type.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <returns>Instance of <see cref="CommandHandlerDelegate"/> which executes the command handler processing.</returns>
        public CommandHandlerDelegate ResolveCommandHandler<TCommand>() where TCommand : class, ICommand
        {
            Type commandType = typeof(TCommand);

            CommandHandlerDelegate commandHandlerDelegate;

            if (!_commandHandlerDelegatesByCommandType.TryGetValue(commandType, out commandHandlerDelegate))
            {
                throw new NoCommandHandlerResolvedException($"No command handler is registered to handle command of type: { commandType.Name }.");
            }

            return commandHandlerDelegate;
        }

        #endregion ICommandHandlerResolver Implementation
    }
}
