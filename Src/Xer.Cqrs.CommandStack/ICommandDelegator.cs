using Xer.Delegator;

namespace Xer.Cqrs.CommandStack
{
    /// <summary>
    /// Represents an object that delegates commands to a command handler.
    /// </summary>
    public interface ICommandDelegator : IMessageDelegator
    {
         
    }
}