using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Registrations;

namespace Xer.Cqrs.AttributeHandlers.Registrations
{
    internal class CommandHandlerMethod
    {
        private static readonly TypeInfo CommandTypeInfo = typeof(ICommand).GetTypeInfo();

        public Type CommandType { get; }
        public MethodInfo MethodInfo { get; }
        public bool IsAsync { get; }
        public bool SupportsCancellation { get; }

        private CommandHandlerMethod(Type commandType, MethodInfo methodInfo, bool isAsync, bool supportsCancellation)
        {
            CommandType = commandType ?? throw new ArgumentNullException(nameof(commandType));
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            IsAsync = isAsync;
            SupportsCancellation = supportsCancellation;
        }

        #region Methods

        public CommandAsyncHandlerDelegate CreateDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory) where TCommand : ICommand
        {
            CommandAsyncHandlerDelegate newCommandHandlerDelegate;

            if (IsAsync)
            {
                if (SupportsCancellation)
                {
                    newCommandHandlerDelegate = createCancellableAsyncDelegate<TAttributed, TCommand>(attributedObjectFactory);
                }
                else
                {
                    newCommandHandlerDelegate = createNonCancellableAsyncDelegate<TAttributed, TCommand>(attributedObjectFactory);
                }
            }
            else
            {
                newCommandHandlerDelegate = createWrappedSyncDelegate<TAttributed, TCommand>(attributedObjectFactory);
            }

            return newCommandHandlerDelegate;
        }

        public static CommandHandlerMethod Create(MethodInfo methodInfo)
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
            else if (methodInfo.ReturnType == typeof(void))
            {
                isAsync = false;
            }
            else
            {
                // Return type is not Task/void. Invalid.
                throw new InvalidOperationException($"Method marked with [CommandHandler] can only have void or a Task as return value: {methodInfo.Name}");
            }

            bool supportsCancellation = methodParameters.Any(p => p.ParameterType == typeof(CancellationToken));

            return new CommandHandlerMethod(commandType, methodInfo, isAsync, supportsCancellation);
        }

        #endregion Methods

        #region Functions

        private CommandAsyncHandlerDelegate createWrappedSyncDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory)
        {
            AttributedCommandHandlerDelegate<TAttributed, TCommand> action = (AttributedCommandHandlerDelegate<TAttributed, TCommand>)MethodInfo.CreateDelegate(typeof(AttributedCommandHandlerDelegate<TAttributed, TCommand>));

            CommandAsyncHandlerDelegate newHandleCommandDelegate = (c, ct) =>
            {
                TAttributed instance = attributedObjectFactory.Invoke();

                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create a command handler instance for {c.GetType().Name}");
                }

                action.Invoke(instance, (TCommand)c);

                return TaskUtility.CompletedTask;
            };

            return newHandleCommandDelegate;
        }

        private CommandAsyncHandlerDelegate createCancellableAsyncDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory)
        {
            AttributedCommandAsyncHandlerCancellableDelegate<TAttributed, TCommand> action = (AttributedCommandAsyncHandlerCancellableDelegate<TAttributed, TCommand>)MethodInfo.CreateDelegate(typeof(AttributedCommandAsyncHandlerCancellableDelegate<TAttributed, TCommand>));

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

        private CommandAsyncHandlerDelegate createNonCancellableAsyncDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory)
        {
            AttributedCommandAsyncHandlerDelegate<TAttributed, TCommand> action = (AttributedCommandAsyncHandlerDelegate<TAttributed, TCommand>)MethodInfo.CreateDelegate(typeof(AttributedCommandAsyncHandlerDelegate<TAttributed, TCommand>));

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
    }
}
