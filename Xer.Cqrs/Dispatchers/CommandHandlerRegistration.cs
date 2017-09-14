using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xer.Cqrs.Dispatchers
{
    public class CommandHandlerRegistration
    {
        private readonly Dictionary<Type, ICollection<HandleCommandDelegate>> _handleCommandDelegatesByCommandType = new Dictionary<Type, ICollection<HandleCommandDelegate>>();

        public void Register<TCommand>(ICommandHandler<TCommand> commandHandler) where TCommand : ICommand
        {
            Type commandType = typeof(TCommand);

            HandleCommandDelegate handleCommandDelegate = (c) =>
            {
                commandHandler.Handle((TCommand)c);

                return Task.CompletedTask;
            };

            ICollection<HandleCommandDelegate> handleCommandDelegates;
            if(_handleCommandDelegatesByCommandType.TryGetValue(commandType, out handleCommandDelegates))
            {
                handleCommandDelegates.Add(handleCommandDelegate);
            }
            else
            {
                _handleCommandDelegatesByCommandType.Add(commandType, 
                    new List<HandleCommandDelegate> { handleCommandDelegate });
            }
        }

        public void Register<TCommand>(ICommandAsyncHandler<TCommand> commandAsyncHandler) where TCommand : ICommand
        {
            Type commandType = typeof(TCommand);

            HandleCommandDelegate handleCommandDelegate = (c) =>
            {
                return commandAsyncHandler.HandleAsync((TCommand)c);
            };

            ICollection<HandleCommandDelegate> handleCommandDelegates;
            if (_handleCommandDelegatesByCommandType.TryGetValue(commandType, out handleCommandDelegates))
            {
                handleCommandDelegates.Add(handleCommandDelegate);
            }
            else
            {
                _handleCommandDelegatesByCommandType.Add(commandType,
                    new List<HandleCommandDelegate> { handleCommandDelegate });
            }
        }

        internal IEnumerable<HandleCommandDelegate> GetRegisteredCommandHandlers(Type commandType)
        {
            ICollection<HandleCommandDelegate> handleCommandDelegates;

            if (!_handleCommandDelegatesByCommandType.TryGetValue(commandType, out handleCommandDelegates))
            {
                throw new NotSupportedException($"No command handler is registered to handle commands of type: { commandType.Name }");
            }

            return handleCommandDelegates.AsEnumerable();
        }
    }
}
