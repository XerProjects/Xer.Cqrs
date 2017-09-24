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

        private static readonly Type CommandHandlerAttributeType = typeof(CommandHandlerAttribute);
        private static readonly TypeInfo CommandTypeInfo = typeof(ICommand).GetTypeInfo();

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
            var methods = commandHandlerType
                .GetRuntimeMethods()
                .Where(m => m.CustomAttributes.Any(a => a.AttributeType == CommandHandlerAttributeType));

            List<CommandHandlerMethod> commandHandlerMethods = new List<CommandHandlerMethod>();

            foreach(MethodInfo methodInfo in methods)
            {
                ParameterInfo[] methodParameters = methodInfo.GetParameters();

                ParameterInfo commandParameter = methodParameters.FirstOrDefault(p => CommandTypeInfo.IsAssignableFrom(p.ParameterType.GetTypeInfo()));

                if (commandParameter == null)
                {
                    // Parameter is not a command. Skip.
                    throw new InvalidOperationException($"Methods marked with [CommandHandler] should accept a command parameter: {methodInfo.Name}");
                }

                Type commandType = commandParameter.ParameterType;

                bool isAsync;

                // Only valid return types are Task/void.
                if (methodInfo.ReturnType == typeof(Task))
                {
                    isAsync = true;
                }
                else if(methodInfo.ReturnType == typeof(void))
                {
                    isAsync = false;
                }
                else
                {
                    // Return type is not Task/void. Invalid.
                    throw new InvalidOperationException($"Method marked with [CommandHandler] can only have void or a Task as return value: {methodInfo.Name}");
                }

                bool supportsCancellation = methodParameters.Any(p => p.ParameterType == typeof(CancellationToken));

                commandHandlerMethods.Add(new CommandHandlerMethod(commandType, methodInfo, isAsync, supportsCancellation));
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

            CommandAsyncHandlerDelegate newHandleCommandDelegate;

            if (commandHandlerMethod.IsAsync)
            {
                newHandleCommandDelegate = createAsyncDelegate<TAttributed, TCommand>(attributedObjectFactory, commandHandlerMethod);
            }
            else
            {
                newHandleCommandDelegate = createWrappedSyncDelegate<TAttributed, TCommand>(attributedObjectFactory, commandHandlerMethod);
            }

            _commandHandlerDelegatesByCommandType.Add(commandType, newHandleCommandDelegate);
        }

        private static CommandAsyncHandlerDelegate createAsyncDelegate<TAttributed, TCommand>(Func<TAttributed> commandHandler, CommandHandlerMethod commandHandlerMethod)
        {
            if (commandHandlerMethod.SupportsCancellation)
            {
                return createCancellableAsyncDelegate<TAttributed, TCommand>(commandHandler, commandHandlerMethod);
            }
            else
            {
                return createNonCancellableAsyncDelegate<TAttributed, TCommand>(commandHandler, commandHandlerMethod);
            }
        }

        private static CommandAsyncHandlerDelegate createWrappedSyncDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory, CommandHandlerMethod commandHandlerMethod)
        {
            AttributedCommandHandlerDelegate<TAttributed, TCommand> action = (AttributedCommandHandlerDelegate<TAttributed, TCommand>)commandHandlerMethod.MethodInfo.CreateDelegate(typeof(AttributedCommandHandlerDelegate<TAttributed, TCommand>));

            CommandAsyncHandlerDelegate newHandleCommandDelegate = (c, ct) =>
            {
                TAttributed instance = attributedObjectFactory.Invoke();

                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create a command handler instance for {c.GetType().Name}");
                }

                action.Invoke(instance, (TCommand)c);

                return Task.FromResult(0);
            };

            return newHandleCommandDelegate;
        }

        private static CommandAsyncHandlerDelegate createCancellableAsyncDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory, CommandHandlerMethod commandHandlerMethod)
        {
            AttributedCommandAsyncHandlerCancellableDelegate<TAttributed, TCommand> action = (AttributedCommandAsyncHandlerCancellableDelegate<TAttributed, TCommand>)commandHandlerMethod.MethodInfo.CreateDelegate(typeof(AttributedCommandAsyncHandlerCancellableDelegate<TAttributed, TCommand>));

            CommandAsyncHandlerDelegate newHandleCommandDelegate = (c, ct) =>
            {
                TAttributed instance = attributedObjectFactory.Invoke();

                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create a command handler instance for {c.GetType().Name}");
                }

                return action.Invoke(instance, (TCommand)c, ct);
            };

            return newHandleCommandDelegate;
        }

        private static CommandAsyncHandlerDelegate createNonCancellableAsyncDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory, CommandHandlerMethod commandHandlerMethod)
        {
            AttributedCommandAsyncHandlerDelegate<TAttributed, TCommand> action = (AttributedCommandAsyncHandlerDelegate<TAttributed, TCommand>)commandHandlerMethod.MethodInfo.CreateDelegate(typeof(AttributedCommandAsyncHandlerDelegate<TAttributed, TCommand>));

            CommandAsyncHandlerDelegate newHandleCommandDelegate = (c, ct) =>
            {
                TAttributed instance = attributedObjectFactory.Invoke();

                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create a command handler instance for {c.GetType().Name}");
                }

                return action.Invoke(instance, (TCommand)c);
            };

            return newHandleCommandDelegate;
        }

        #endregion Functions

        #region Inner CommandHandlerMethod Class

        private class CommandHandlerMethod
        {
            public Type CommandType { get; }
            public MethodInfo MethodInfo { get; }
            public bool IsAsync { get; private set; }
            public bool SupportsCancellation { get; }

            public CommandHandlerMethod(Type commandType, MethodInfo methodInfo, bool isAsync, bool supportsCancellation)
            {
                CommandType = commandType ?? throw new ArgumentNullException(nameof(commandType));
                MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
                IsAsync = isAsync;
                SupportsCancellation = supportsCancellation;
            }
        }

        #endregion Inner CommandHandlerMethod Class
    }
}
