using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Messajing
{
    public interface IMessageHandlerDelegateResolver
    {
        MessageHandlerDelegate Resolve(Type messageType);
        MessageHandlerDelegate Resolve<TMessage>() where TMessage : IMessage;
        MessageHandlerDelegate<TResult> Resolve<TResult>(Type messageType);
        MessageHandlerDelegate<TResult> Resolve<TMessage, TResult>() where TMessage : IMessage<TResult>;
    }
}
