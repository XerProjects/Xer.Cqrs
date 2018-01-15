using System;
using Xer.Cqrs.CommandStack;

namespace Xer.Delegator.Registrations
{
    public static partial class CommandHandlerRegistrationExtensions
    {
        #region Methods
        
        /// <summary>
        /// Register command handler.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <param name="registration">Command handler registration.</param>
        /// <param name="commandHandlerFactory">Factory which will provide an instance of a command handler that handles the specified <typeparamref name="TCommand"/> command.</param>
        public static void RegisterCommandHandler<TCommand>(this SingleMessageHandlerRegistration registration, 
                                                            Func<ICommandHandler<TCommand>> commandHandlerFactory) 
                                                            where TCommand : class
        {
            if (commandHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(commandHandlerFactory));
            }

            // Create delegate.
            MessageHandlerDelegate<TCommand> newHandleCommandDelegate = CommandHandlerDelegateBuilder.FromCommandHandlerFactory(commandHandlerFactory);

            registration.Register<TCommand>(newHandleCommandDelegate);
        }

        /// <summary>
        /// Register command async handler.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <param name="registration">Command handler registration.</param>
        /// <param name="commandAsyncHandlerFactory">Factory which will provide an instance of a command handler that handles the specified <typeparamref name="TCommand"/> command.</param>
        public static void RegisterCommandHandler<TCommand>(this SingleMessageHandlerRegistration registration, 
                                                            Func<ICommandAsyncHandler<TCommand>> commandAsyncHandlerFactory) 
                                                            where TCommand : class
        {
            if(commandAsyncHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(commandAsyncHandlerFactory));
            }

            // Create delegate.
            MessageHandlerDelegate<TCommand> newHandleCommandDelegate = CommandHandlerDelegateBuilder.FromCommandHandlerFactory(commandAsyncHandlerFactory);
        
            // Register.
            registration.Register<TCommand>(newHandleCommandDelegate);
        }

        #endregion Methods
    }
}