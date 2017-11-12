using System;
using System.Threading.Tasks;
using Xer.Messajing.Internals;
using Xer.Messajing.Resolvers;

namespace Xer.Messajing.Registrations
{
    public class MessageHandlerFactoryRegistration : IMessageHandlerFactoryRegistration
    {
        #region Declarations

        private readonly MessageHandlerDelegateStore _messageHandlerDelegateStore = new MessageHandlerDelegateStore();

        #endregion Declarations

        #region IMessageAsyncHandlerFactoryRegistration Implementation

        /// <summary>
        /// Register command handler.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to be handled.</typeparam>
        /// <param name="messageHandlerFactory">Synchronous handler which can process the message.</param>
        public void Register<TMessage>(Func<IMessageHandler<TMessage>> messageHandlerFactory) where TMessage : IMessage
        {
            Type messageType = typeof(TMessage);

            MessageHandlerDelegate handleCommandDelegate;

            if (_messageHandlerDelegateStore.TryGetValue(messageType, out handleCommandDelegate))
            {
                throw new InvalidOperationException($"Duplicate message handler registered for {messageType.Name}.");
            }

            MessageHandlerDelegate newHandleCommandDelegate = (c, ct) =>
            {
                IMessageHandler<TMessage> messageHandlerInstance = messageHandlerFactory.Invoke();

                if (messageHandlerInstance == null)
                {
                    throw new InvalidOperationException($"Failed to create a message handler instance for {c.GetType().Name}");
                }

                messageHandlerInstance.Handle((TMessage)c);

                return TaskUtility.CompletedTask;
            };

            _messageHandlerDelegateStore.Add(messageType, newHandleCommandDelegate);
        }

        /// <summary>
        /// Register message async handler.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to be handled.</typeparam>
        /// <param name="messageAsyncHandlerFactory">Asynchronous handler which can process the message.</param>
        public void Register<TMessage>(Func<IMessageAsyncHandler<TMessage>> messageAsyncHandlerFactory) where TMessage : IMessage
        {
            Type messageType = typeof(TMessage);

            MessageHandlerDelegate handleCommandDelegate;

            if (_messageHandlerDelegateStore.TryGetValue(messageType, out handleCommandDelegate))
            {
                throw new InvalidOperationException($"Duplicate message async handler registered for {messageType.Name}.");
            }

            MessageHandlerDelegate newMessageHandlerDelegate = (c, ct) =>
            {
                IMessageAsyncHandler<TMessage> commandHandlerInstance = messageAsyncHandlerFactory.Invoke();

                if (commandHandlerInstance == null)
                {
                    throw new InvalidOperationException($"Failed to create a message handler instance for {c.GetType().Name}");
                }

                return commandHandlerInstance.HandleAsync((TMessage)c, ct);
            };

            _messageHandlerDelegateStore.Add(messageType, newMessageHandlerDelegate);
        }

        public void Register<TMessage, TResult>(Func<IMessageAsyncHandler<TMessage, TResult>> messageAsyncHandlerFactory) where TMessage : IMessage<TResult>
        {
            Type messageType = typeof(TMessage);

            MessageHandlerDelegate handleCommandDelegate;

            if (_messageHandlerDelegateStore.TryGetValue(messageType, out handleCommandDelegate))
            {
                throw new InvalidOperationException($"Duplicate message async handler registered for {messageType.Name}.");
            }

            MessageHandlerDelegate newMessageHandlerDelegate = (c, ct) =>
            {
                IMessageAsyncHandler<TMessage, TResult> commandHandlerInstance = messageAsyncHandlerFactory.Invoke();

                if (commandHandlerInstance == null)
                {
                    throw new InvalidOperationException($"Failed to create a message handler instance for {c.GetType().Name}");
                }

                return commandHandlerInstance.HandleAsync((TMessage)c, ct);
            };

            _messageHandlerDelegateStore.Add(messageType, newMessageHandlerDelegate);
        }

        public void Register<TMessage, TResult>(Func<IMessageHandler<TMessage, TResult>> messageHandlerFactory) where TMessage : IMessage<TResult>
        {
            Type messageType = typeof(TMessage);

            MessageHandlerDelegate handleCommandDelegate;

            if (_messageHandlerDelegateStore.TryGetValue(messageType, out handleCommandDelegate))
            {
                throw new InvalidOperationException($"Duplicate message async handler registered for {messageType.Name}.");
            }

            MessageHandlerDelegate newMessageHandlerDelegate = (c, ct) =>
            {
                IMessageHandler<TMessage, TResult> commandHandlerInstance = messageHandlerFactory.Invoke();

                if (commandHandlerInstance == null)
                {
                    throw new InvalidOperationException($"Failed to create a message handler instance for {c.GetType().Name}");
                }

                TResult result = commandHandlerInstance.Handle((TMessage)c);

                return Task.FromResult(result);
            };

            _messageHandlerDelegateStore.Add(messageType, newMessageHandlerDelegate);
        }

        /// <summary>
        /// Build a message handler delegate resolver.
        /// </summary>
        /// <returns>Instance of IMessageHandlerDelegateResolver containing the registered message handlers.</returns>
        public IMessageHandlerDelegateResolver BuildResolver()
        {
            return new MessageHandlerDelegateResolver(_messageHandlerDelegateStore);
        }

        #endregion IMessageAsyncHandlerFactoryRegistration Implementation
    }
}
