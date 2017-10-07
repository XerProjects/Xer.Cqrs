using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xer.Cqrs.CommandStack.Attributes;

namespace Xer.Cqrs.CommandStack.Registrations
{
    public class CommandHandlerAttributeRegistration : ICommandHandlerAttributeRegistration, ICommandHandlerProvider
    {
        #region Declarations
        
        private static readonly MethodInfo RegisterCommandHandlerOpenGenericMethodInfo = typeof(CommandHandlerAttributeRegistration).GetTypeInfo().DeclaredMethods.First(m => m.Name == nameof(registerCommandHandlerMethod));

        private readonly IDictionary<Type, CommandHandlerDelegate> _commandHandlerDelegatesByCommandType = new Dictionary<Type, CommandHandlerDelegate>();

        #endregion Declarations

        #region IQueryHandlerProvider Implementation

        /// <summary>
        /// Get a delegate to handle the command of the specified type.
        /// </summary>
        /// <param name="commandType">Type of command to be handled.</param>
        /// <returns>Instance of invokeable CommandAsyncHandlerDelegate.</returns>
        public CommandHandlerDelegate GetCommandHandler(Type commandType)
        {
            CommandHandlerDelegate handlerDelegate;

            if (!_commandHandlerDelegatesByCommandType.TryGetValue(commandType, out handlerDelegate))
            {
                throw new CommandNotHandledException($"No command handler is registered to handle command of type: { commandType.Name }.");
            }

            return handlerDelegate;
        }

        #endregion IQueryHandlerProvider Implementation

        #region ICommandHandlerAttributeRegistration Implementation

        /// <summary>
        /// Register all methods of the instance that are marked with [CommandHandler].
        /// In order to be registered successfully, methods should:
        /// - Request for the command to be handled as paramater.
        /// - Return a Task object.
        /// - (Optional) Request for a CancellationToken as parameter to listen for cancellation from Dispatcher.
        /// </summary>
        /// <param name="attributedHandlerFactory">Object which contains methods marked with [CommandHandler].</param>
        public void RegisterCommandHandlerAttributes<TAttributed>(Func<TAttributed> attributedHandlerFactory) where TAttributed : class
        {
            Type attributedObjectType = typeof(TAttributed);

            // Get all public methods marked with CommandHandler attribute.
            IEnumerable<CommandHandlerAttributeMethod> commandHandlerMethods = getCommandHandlerMethods(attributedObjectType);

            foreach (CommandHandlerAttributeMethod commandHandlerMethod in commandHandlerMethods)
            {                
                MethodInfo registerCommandHandlerGenericMethodInfo = RegisterCommandHandlerOpenGenericMethodInfo.MakeGenericMethod(
                    attributedObjectType, 
                    commandHandlerMethod.CommandType);

                registerCommandHandlerGenericMethodInfo.Invoke(this, new object[]
                {
                    attributedHandlerFactory, commandHandlerMethod
                });
            }
        }

        #endregion ICommandHandlerAttributeRegistration Implementation

        #region Functions

        private void registerCommandHandlerMethod<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory, CommandHandlerAttributeMethod commandHandlerMethod) where TCommand : ICommand
        {
            Type commandType = typeof(TCommand);

            CommandHandlerDelegate handleCommandDelegate;
            if (_commandHandlerDelegatesByCommandType.TryGetValue(commandType, out handleCommandDelegate))
            {
                throw new InvalidOperationException($"Duplicate command handler registered for {commandType.Name}.");
            }

            CommandHandlerDelegate newHandleCommandDelegate = commandHandlerMethod.CreateDelegate<TAttributed, TCommand>(attributedObjectFactory);

            _commandHandlerDelegatesByCommandType.Add(commandType, newHandleCommandDelegate);
        }

        private static IEnumerable<CommandHandlerAttributeMethod> getCommandHandlerMethods(Type commandHandlerType)
        {
            List<CommandHandlerAttributeMethod> commandHandlerMethods = new List<CommandHandlerAttributeMethod>();

            foreach (MethodInfo methodInfo in commandHandlerType.GetRuntimeMethods())
            {
                if (methodInfo.CustomAttributes.Any(a => a.AttributeType == typeof(CommandHandlerAttribute)))
                {
                    // Return methods marked with [CommandHandler].
                    yield return CommandHandlerAttributeMethod.Create(methodInfo);
                }
            }
        }

        #endregion Functions
    }
}
