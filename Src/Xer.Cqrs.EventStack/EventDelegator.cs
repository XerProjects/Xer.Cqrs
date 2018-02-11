using Xer.Delegator;

namespace Xer.Cqrs.EventStack
{
    /// <summary>
    /// Represents an object that delegates events to a one or more message handlers.
    /// </summary>
    public class EventDelegator : MessageDelegator
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messageHandlerResolver">Message handler resolver.</param>
        public EventDelegator(IMessageHandlerResolver messageHandlerResolver) 
            : base(messageHandlerResolver)
        {
        }
    }
}