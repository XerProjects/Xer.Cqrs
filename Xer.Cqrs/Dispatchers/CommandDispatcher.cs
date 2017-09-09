using System;
using System.Collections.Generic;

namespace Xer.Cqrs.Dispatchers
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly Dictionary<Type, Action<ICommand>> _commandHandlerActionsByCommandType = new Dictionary<Type, Action<ICommand>>();

        public virtual void Dispatch(ICommand command)
        {
            Type commandType = command.GetType();

            Action<ICommand> handleCommandAction;

            if(!_commandHandlerActionsByCommandType.TryGetValue(commandType, out handleCommandAction))
            {
                throw new NotSupportedException($"No command handler is registered to handle commands of type: { commandType.Name }");
            }

            handleCommandAction.Invoke(command);
        }

        public void RegisterHandler<TCommand>(ICommandHandler<TCommand> commandHandler) where TCommand : ICommand
        {
            Type commandType = typeof(TCommand);

            Action<ICommand> handleCommandAction = (c) => commandHandler.Handle((TCommand)c);

            _commandHandlerActionsByCommandType.Add(commandType, handleCommandAction);
        }
    }
}
