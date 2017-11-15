using System.Threading;

namespace Xer.Cqrs.CommandStack
{
    public interface ICommandDispatcher
    {
        /// <summary>
        /// Dispatch the command to the registered command handler.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to dispatch.</typeparam>
        /// <param name="command">Command to dispatch.</param>
        void Dispatch<TCommand>(TCommand command) where TCommand : class, ICommand;
    }
}
