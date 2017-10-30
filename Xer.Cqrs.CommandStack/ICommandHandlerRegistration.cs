using System;

namespace Xer.Cqrs.CommandStack
{
    public interface ICommandHandlerRegistration
    {
        /// <summary>
        /// Register command async handler.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <param name="commandAsyncHandler">Asynchronous handler which can process the command.</param>
        void Register<TCommand>(Func<ICommandAsyncHandler<TCommand>> commandAsyncHandlerFactory) where TCommand : class, ICommand;

        /// <summary>
        /// Register command handler.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to be handled.</typeparam>
        /// <param name="commandHandlerFactory">Synchronous handler which can process the command.</param>
        void Register<TCommand>(Func<ICommandHandler<TCommand>> commandHandlerFactory) where TCommand : class, ICommand;
    }
}
