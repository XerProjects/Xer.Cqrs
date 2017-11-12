using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Messajing
{
    public delegate Task MessageHandlerDelegate(IMessage message, CancellationToken cancellationToken = default(CancellationToken));
    public delegate Task<TResult> MessageHandlerDelegate<TResult>(IMessage<TResult> message, CancellationToken cancellationToken = default(CancellationToken));
}
