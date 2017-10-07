using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack.Registrations
{
    internal class CommandHandlerAttributeMethod
    {
        #region Declarations

        private static readonly TypeInfo CommandTypeInfo = typeof(ICommand).GetTypeInfo();

        #endregion Declarations

        #region Properties

        public Type CommandType { get; }
        public MethodInfo MethodInfo { get; }
        public bool IsAsync { get; }
        public bool SupportsCancellation { get; }

        #endregion Properties

        #region Constructors

        private CommandHandlerAttributeMethod(Type commandType, MethodInfo methodInfo, bool isAsync, bool supportsCancellation)
        {
            CommandType = commandType ?? throw new ArgumentNullException(nameof(commandType));
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            IsAsync = isAsync;
            SupportsCancellation = supportsCancellation;
        }

        #endregion Constructors

        #region Methods

        public CommandHandlerDelegate CreateDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory) where TCommand : ICommand
        {
            CommandHandlerDelegate newCommandHandlerDelegate;

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

        public static CommandHandlerAttributeMethod Create(MethodInfo methodInfo)
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

                if(methodInfo.CustomAttributes.Any(p => p.AttributeType == typeof(AsyncStateMachineAttribute)))
                {
                    throw new InvalidOperationException($"Methods marked with async void are not allowed. Exceptions from async void methods may crash the application: {methodInfo.Name}");
                }
            }
            else
            {
                // Return type is not Task/void. Invalid.
                throw new InvalidOperationException($"Method marked with [CommandHandler] can only have void or a Task as return value: {methodInfo.Name}");
            }

            bool supportsCancellation = methodParameters.Any(p => p.ParameterType == typeof(CancellationToken));

            return new CommandHandlerAttributeMethod(commandType, methodInfo, isAsync, supportsCancellation);
        }

        #endregion Methods

        #region Functions

        private CommandHandlerDelegate createWrappedSyncDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory)
        {
            Action<TAttributed, TCommand> action = (Action<TAttributed, TCommand>)MethodInfo.CreateDelegate(typeof(Action<TAttributed, TCommand>));

            CommandHandlerDelegate newHandleCommandDelegate = (c, ct) =>
            {
                TAttributed instance = attributedObjectFactory.Invoke();

                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create a command handler instance for {c.GetType().Name}.");
                }

                action.Invoke(instance, (TCommand)c);

                return TaskUtility.CompletedTask;
            };

            return newHandleCommandDelegate;
        }

        private CommandHandlerDelegate createCancellableAsyncDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory)
        {
            Func<TAttributed, TCommand, CancellationToken, Task> action = (Func<TAttributed, TCommand, CancellationToken, Task>)MethodInfo.CreateDelegate(typeof(Func<TAttributed, TCommand, CancellationToken, Task>));

            CommandHandlerDelegate newHandleCommandDelegate = (c, ct) =>
            {
                TAttributed instance = attributedObjectFactory.Invoke();

                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create a command handler instance for {c.GetType().Name}.");
                }

                return action.Invoke(instance, (TCommand)c, ct);
            };

            return newHandleCommandDelegate;
        }

        private CommandHandlerDelegate createNonCancellableAsyncDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory)
        {
            Func<TAttributed, TCommand, Task> action = (Func<TAttributed, TCommand, Task>)MethodInfo.CreateDelegate(typeof(Func<TAttributed, TCommand, Task>));

            CommandHandlerDelegate newHandleCommandDelegate = (c, ct) =>
            {
                TAttributed instance = attributedObjectFactory.Invoke();

                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create a command handler instance for {c.GetType().Name}.");
                }

                return action.Invoke(instance, (TCommand)c);
            };

            return newHandleCommandDelegate;
        }

        #endregion Functions
    }
}
