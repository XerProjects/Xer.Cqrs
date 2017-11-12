using System;
using Xer.Messajing.Internals;

namespace Xer.Messajing.Resolvers
{
    public class MessageHandlerDelegateResolver : IMessageHandlerDelegateResolver
    {
        #region Declarations

        private readonly MessageHandlerDelegateStore _messageHandlerDelegateStore;

        #endregion Declarations

        #region Constructors

        internal MessageHandlerDelegateResolver(MessageHandlerDelegateStore messageHandlerDelegateStore)
        {
            _messageHandlerDelegateStore = messageHandlerDelegateStore ?? throw new ArgumentNullException(nameof(messageHandlerDelegateStore));
        }

        #endregion Constructors

        #region IMessageHandlerDelegateResolver Implementation

        /// <summary>
        /// Get the registered command handler delegate to handle the command of the specified type.
        /// </summary>
        /// <param name="messageType">Type of command to be handled.</param>
        /// <returns>Instance of invokeable CommandAsyncHandlerDelegate.</returns>
        public MessageHandlerDelegate Resolve<TMessage>() where TMessage : IMessage
        {
            return Resolve(typeof(TMessage));
        }

        /// <summary>
        /// Get the registered command handler delegate to handle the command of the specified type.
        /// </summary>
        /// <param name="messageType">Type of command to be handled.</param>
        /// <returns>Instance of invokeable CommandAsyncHandlerDelegate.</returns>
        public MessageHandlerDelegate Resolve(Type messageType)
        {
            MessageHandlerDelegate handleCommandDelegate;

            if (!_messageHandlerDelegateStore.TryGetValue(messageType, out handleCommandDelegate))
            {
                throw new MessageHandlerNotRegisteredException($"No message handler is registered to handle message of type: { messageType.Name }");
            }

            return handleCommandDelegate;
        }

        public MessageHandlerDelegate<TResult> Resolve<TMessage, TResult>() where TMessage : IMessage<TResult>
        {
            return Resolve<TResult>(typeof(TMessage));
        }

        public MessageHandlerDelegate<TResult> Resolve<TResult>(Type messageType)
        {
            MessageHandlerDelegate<TResult> handleCommandDelegate;

            if (!_messageHandlerDelegateStore.TryGetValue(messageType, out handleCommandDelegate))
            {
                throw new MessageHandlerNotRegisteredException($"No message handler is registered to handle message of type: { messageType.Name }");
            }

            return handleCommandDelegate;
        }

        #endregion IMessageHandlerDelegateResolver Implementation
    }
}
