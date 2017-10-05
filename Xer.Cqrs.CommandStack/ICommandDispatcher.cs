using System.Threading;

namespace Xer.Cqrs.CommandStack
{
    public interface ICommandDispatcher
    {
        /// <summary>
        /// Dispatch the command to the registered command handlers.
        /// </summary>
        /// <param name="command">Command to dispatch.</param>
        void Dispatch(ICommand command);
    }
}
