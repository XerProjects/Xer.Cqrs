using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.CommandStack;

namespace Xer.Delegator.Registration
{
    public static class SingleMessageHandlerRegistrationExtensions
    {
        #region Methods

        /// <summary>
        /// Register command async handler.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <param name="registration">Instance of SingleMessageHandlerRegistration.</param>
        /// <param name="commandAsyncHandlerFactory">Factory which will provide an instance of a command handler that handles the specified <typeparamref name="TCommand"/> command.</param>
        public static void RegisterCommandHandler<TCommand>(this SingleMessageHandlerRegistration registration, 
                                                            Func<ICommandAsyncHandler<TCommand>> commandAsyncHandlerFactory) 
                                                            where TCommand : class
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (commandAsyncHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(commandAsyncHandlerFactory));
            }

            MessageHandlerDelegate messageHandlerDelegate = CommandHandlerDelegateBuilder.FromCommandHandlerFactory(commandAsyncHandlerFactory);
            registration.Register<TCommand>(messageHandlerDelegate.Invoke);
        }
        
        /// <summary>
        /// Register command handler.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <param name="registration">Instance of SingleMessageHandlerRegistration.</param>
        /// <param name="commandHandlerFactory">Factory which will provide an instance of a command handler that handles the specified <typeparamref name="TCommand"/> command.</param>
        /// <param name="yieldExecution">True if execution of synchronous handler should be yielded. Otherwise, false.</param>
        public static void RegisterCommandHandler<TCommand>(this SingleMessageHandlerRegistration registration, 
                                                            Func<ICommandHandler<TCommand>> commandHandlerFactory) 
                                                            where TCommand : class
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (commandHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(commandHandlerFactory));
            }

            MessageHandlerDelegate messageHandlerDelegate = CommandHandlerDelegateBuilder.FromCommandHandlerFactory(commandHandlerFactory);
            registration.Register<TCommand>(messageHandlerDelegate.Invoke);
        }

        #endregion Methods
    }
}