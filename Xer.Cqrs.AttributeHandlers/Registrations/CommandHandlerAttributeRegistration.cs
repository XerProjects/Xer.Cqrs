using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Registrations;

namespace Xer.Cqrs.AttributeHandlers.Registrations
{
    public class CommandHandlerAttributeRegistration : IAttributedHandlerRegistration, ICommandHandlerProvider
    {
        #region Declarations
        
        private static readonly MethodInfo NonGenericRegisterCommandHandlerMethod = typeof(CommandHandlerAttributeRegistration).GetTypeInfo().DeclaredMethods.First(m => m.Name == nameof(registerCommandHandlerMethods));

        private readonly IDictionary<Type, CommandAsyncHandlerDelegate> _commandHandlerDelegatesByCommandType = new Dictionary<Type, CommandAsyncHandlerDelegate>();

        #endregion Declarations

        #region IQueryHandlerProvider Implementation

        /// <summary>
        /// Get a delegate to handle the command of the specified type.
        /// </summary>
        /// <param name="commandType">Type of command to be handled.</param>
        /// <returns>Instance of invokeable CommandAsyncHandlerDelegate.</returns>
        public CommandAsyncHandlerDelegate GetCommandHandler(Type commandType)
        {
            CommandAsyncHandlerDelegate handlerDelegate;

            if (!_commandHandlerDelegatesByCommandType.TryGetValue(commandType, out handlerDelegate))
            {
                throw new HandlerNotFoundException($"No command handler is registered to handle command of type: { commandType.Name }");
            }

            return handlerDelegate;
        }

        #endregion IQueryHandlerProvider Implementation

        #region IAttributedHandlerRegistration Implementation

        /// <summary>
        /// Register all methods of the instance that are marked with [CommandHandler].
        /// In order to be registered successfully, methods should:
        /// - Request for the command to be handled as paramater.
        /// - Return a Task object.
        /// - (Optional) Request for a CancellationToken as parameter to listen for cancellation from Dispatcher.
        /// </summary>
        /// <param name="attributedObjectFactory">Object which contains methods marked with [CommandHandler].</param>
        public void RegisterAttributedMethods<TAttributed>(Func<TAttributed> attributedObjectFactory)
        {
            Type attributedObjectType = typeof(TAttributed);

            // Get all public methods marked with CommandHandler attribute.
            IEnumerable<CommandHandlerMethod> commandHandlerMethods = GetCommandHandlerMethods(attributedObjectType);

            foreach (CommandHandlerMethod commandHandlerMethod in commandHandlerMethods)
            {                
                MethodInfo genericRegisterCommandHandlerMethod = NonGenericRegisterCommandHandlerMethod.MakeGenericMethod(attributedObjectType, commandHandlerMethod.CommandType);

                genericRegisterCommandHandlerMethod.Invoke(this, new object[]
                {
                    attributedObjectFactory, commandHandlerMethod
                });
            }
        }

        #endregion IAttributedHandlerRegistration Implementation

        #region Functions

        private static IEnumerable<CommandHandlerMethod> GetCommandHandlerMethods(Type commandHandlerType)
        {
            List<CommandHandlerMethod> commandHandlerMethods = new List<CommandHandlerMethod>();

            foreach(MethodInfo methodInfo in commandHandlerType.GetRuntimeMethods())
            {
                if (!methodInfo.CustomAttributes.Any(a => a.AttributeType == typeof(CommandHandlerAttribute)))
                {
                    // Method not marked with [CommandHandler]. Skip.
                    continue;
                }

                commandHandlerMethods.Add(CommandHandlerMethod.Create(methodInfo));
            }

            return commandHandlerMethods;
        }

        private void registerCommandHandlerMethods<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory, CommandHandlerMethod commandHandlerMethod) where TCommand : ICommand
        {
            Type commandType = typeof(TCommand);

            CommandAsyncHandlerDelegate handleCommandDelegate;
            if (_commandHandlerDelegatesByCommandType.TryGetValue(commandType, out handleCommandDelegate))
            {
                throw new InvalidOperationException($"Duplicate command handler registered for {commandType.Name}.");
            }

            CommandAsyncHandlerDelegate newHandleCommandDelegate = commandHandlerMethod.CreateDelegate<TAttributed, TCommand>(attributedObjectFactory);

            _commandHandlerDelegatesByCommandType.Add(commandType, newHandleCommandDelegate);
        }

        #endregion Functions
    }
}
