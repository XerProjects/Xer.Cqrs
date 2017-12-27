using System;

namespace Xer.Cqrs.CommandStack
{
    public interface ICommandHandlerRegistration
    {
        /// <summary>
        /// Register command async handler.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <param name="commandAsyncHandlerFactory">Factory which will provide an instance of a command handler that handles the specified <typeparamref name="TCommand"/> command.</param>
        void Register<TCommand>(Func<ICommandAsyncHandler<TCommand>> commandAsyncHandlerFactory) where TCommand : class, ICommand;

        /// <summary>
        /// Register command handler.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <param name="commandHandlerFactory">Factory which will provide an instance of a command handler that handles the specified <typeparamref name="TCommand"/> command.</param>
        void Register<TCommand>(Func<ICommandHandler<TCommand>> commandHandlerFactory) where TCommand : class, ICommand;
    }
}
