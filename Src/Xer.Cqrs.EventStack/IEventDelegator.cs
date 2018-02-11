using Xer.Delegator;

namespace Xer.Cqrs.EventStack
{
    /// <summary>
    /// Represents an object that delegates events to a one or more message handlers.
    /// </summary>
    public interface IEventDelegator : IMessageDelegator
    {
    }
}