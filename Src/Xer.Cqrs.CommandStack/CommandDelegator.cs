using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Delegator;
using Xer.Delegator.Resolvers;

namespace Xer.Cqrs.CommandStack
{
    /// <summary>
    /// Represents an object that delegates commands to a command handler.
    /// </summary>
    public class CommandDelegator : MessageDelegator, ICommandDelegator
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// This constructor will decorate passed-in message handler resolver with <see cref="Xer.Delegator.Resolvers.RequiredMessageHandlerResolver"/>
        /// if the message handler resolver is not already of the same type.
        /// </remarks>
        /// <param name="messageHandlerResolver">Message handler resolver.</param>
        public CommandDelegator(IMessageHandlerResolver messageHandlerResolver) 
            : base(ToRequiredMessageHandlerResolver(messageHandlerResolver))
        {
        }

        /// <summary>
        /// Decorate the message handler with <see cref="Xer.Delegator.Resolvers.RequiredMessageHandlerResolver"/>
        /// if the message handler resolver is not already of that type.
        /// </summary>
        /// <param name="messageHandlerResolver">Message handler resolver.</param>
        /// <returns>Instance of <see cref="Xer.Delegator.Resolvers.RequiredMessageHandlerResolver"/>.</returns>
        private static IMessageHandlerResolver ToRequiredMessageHandlerResolver(IMessageHandlerResolver messageHandlerResolver)
        {
            if (messageHandlerResolver == null)
            {
                throw new ArgumentNullException(nameof(messageHandlerResolver));
            }

            return messageHandlerResolver is RequiredMessageHandlerResolver ? messageHandlerResolver : new RequiredMessageHandlerResolver(messageHandlerResolver);
        }
    }
}