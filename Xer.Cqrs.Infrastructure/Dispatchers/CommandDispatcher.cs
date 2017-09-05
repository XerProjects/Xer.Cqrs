using System;
using System.Collections.Generic;
using System.Text;
using Xer.Cqrs.Commands;
using Xer.Cqrs.Infrastructure.Commands;
using Xer.Cqrs.Validation;

namespace Xer.Cqrs.Infrastructure.Dispatchers
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly Dictionary<Type, Action<ICommand>> _commandHandlersByCommandType = new Dictionary<Type, Action<ICommand>>();

        public void Dispatch(ICommand command)
        {
            Type commandType = command.GetType();

            Action<ICommand> handleCommandAction;

            if(!_commandHandlersByCommandType.TryGetValue(commandType, out handleCommandAction))
            {
                throw new NotSupportedException($"No command handler is registered for command of type: { commandType.Name }");
            }

            handleCommandAction.Invoke(command);
        }

        public void RegisterHandler<TCommand>(ICommandHandler<TCommand> commandHandler) where TCommand : ICommand
        {
            Type commandType = typeof(TCommand);

            Action<ICommand> handleCommandAction = new Action<ICommand>((c) => commandHandler.Handle((TCommand)c));

            _commandHandlersByCommandType.Add(commandType, handleCommandAction);
        }
    }
}
