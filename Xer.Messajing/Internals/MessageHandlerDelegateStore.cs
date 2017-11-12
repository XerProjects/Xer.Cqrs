using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Messajing.Internals
{
    /// <summary>
    /// This class takes care of storing different query delegates with different return types.
    /// </summary>
    internal class MessageHandlerDelegateStore
    {
        private readonly IDictionary<Type, MessageHandlerDelegate> _messageHandlerDelegatesByMessageType = new Dictionary<Type, MessageHandlerDelegate>();

        private readonly IDictionary<Type, object> _messageHandlerWithResultDelegatesByMessageType = new Dictionary<Type, object>();

        public void Add(Type messageType, MessageHandlerDelegate messageHandlerDelegate)
        {
            _messageHandlerDelegatesByMessageType.Add(messageType, messageHandlerDelegate);
        }

        public bool TryGetValue(Type messageType, out MessageHandlerDelegate queryHandlerDelegate)
        {
            MessageHandlerDelegate value;
            if (_messageHandlerDelegatesByMessageType.TryGetValue(messageType, out value))
            {
                queryHandlerDelegate = value;
                return true;
            }

            queryHandlerDelegate = default(MessageHandlerDelegate);
            return false;
        }

        public void Add<TResult>(Type messageType, MessageHandlerDelegate<TResult> messageHandlerDelegate)
        {
            _messageHandlerWithResultDelegatesByMessageType.Add(messageType, messageHandlerDelegate);
        }

        public bool TryGetValue<TResult>(Type queryType, out MessageHandlerDelegate<TResult> queryHandlerDelegate)
        {
            object value;
            if (_messageHandlerWithResultDelegatesByMessageType.TryGetValue(queryType, out value))
            {
                queryHandlerDelegate = (MessageHandlerDelegate<TResult>)value;
                return true;
            }

            queryHandlerDelegate = default(MessageHandlerDelegate<TResult>);
            return false;
        }
    }
}
