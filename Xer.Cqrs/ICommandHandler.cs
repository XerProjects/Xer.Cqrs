namespace Xer.Cqrs
{
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// Handle and process the command.
        /// </summary>
        /// <param name="command">Command to process.</param>
        void Handle(TCommand command);
    }
}
