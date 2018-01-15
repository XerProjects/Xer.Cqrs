namespace Xer.Cqrs.CommandStack
{
    public interface ICommandHandler<in TCommand> where TCommand : class
    {
        /// <summary>
        /// Handle and process the command.
        /// </summary>
        /// <param name="command">Command to process.</param>
        void Handle(TCommand command);
    }
}
