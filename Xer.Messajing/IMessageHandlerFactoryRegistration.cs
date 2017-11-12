using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Messajing
{
    public interface IMessageHandlerFactoryRegistration
    {
        /// <summary>
        /// Register message async handler.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to be handled.</typeparam>
        /// <param name="messageAsyncHandlerFactory">Asynchronous handler which can process the message.</param>
        void Register<TMessage>(Func<IMessageAsyncHandler<TMessage>> messageAsyncHandlerFactory) where TMessage : IMessage;

        /// <summary>
        /// Register command handler.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to be handled.</typeparam>
        /// <param name="messageHandlerFactory">Synchronous handler which can process the message.</param>
        void Register<TMessage>(Func<IMessageHandler<TMessage>> messageHandlerFactory) where TMessage : IMessage;

        /// <summary>
        /// Register message async handler.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to be handled.</typeparam>
        /// <typeparam name="TResult">Type of result of message processing.</typeparam>
        /// <param name="messageAsyncHandlerFactory">Asynchronous handler which can process the message.</param>
        void Register<TMessage, TResult>(Func<IMessageAsyncHandler<TMessage, TResult>> messageAsyncHandlerFactory) where TMessage : IMessage<TResult>;

        /// <summary>
        /// Register command handler.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to be handled.</typeparam>
        /// <typeparam name="TResult">Type of result of message processing.</typeparam>
        /// <param name="messageHandlerFactory">Synchronous handler which can process the message.</param>
        void Register<TMessage, TResult>(Func<IMessageHandler<TMessage, TResult>> messageHandlerFactory) where TMessage : IMessage<TResult>;

        /// <summary>
        /// Build a message handler delegate resolver.
        /// </summary>
        /// <returns>Instance of IMessageHandlerDelegateResolver containing the registered message handlers.</returns>
        IMessageHandlerDelegateResolver BuildResolver();
    }
}
