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

        public CommandHandlerDelegate CreateDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory) where TAttributed : class
                                   where TCommand : class, ICommand
        {
            if (IsAsync)
            {
                if (SupportsCancellation)
                {
                    return createCancellableAsyncDelegate<TAttributed, TCommand>(attributedObjectFactory);
                }
                else
                {
                    return createNonCancellableAsyncDelegate<TAttributed, TCommand>(attributedObjectFactory);
                }
            }
            else
            {
                return createWrappedSyncDelegate<TAttributed, TCommand>(attributedObjectFactory);
            }
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
                    throw new InvalidOperationException($"Methods with async void signatures are not allowed. A Task may be used as return type instead of void. Check method: {methodInfo.ToString()}.");
                }
            }
            else
            {
                // Return type is not Task/void. Invalid.
                throw new InvalidOperationException($"Method marked with [CommandHandler] can only have void or a Task as return value: {methodInfo.Name}");
            }

            bool supportsCancellation = methodParameters.Any(p => p.ParameterType == typeof(CancellationToken));

            if (!isAsync && supportsCancellation)
            {
                throw new InvalidOperationException("Cancellation token support is only available for async methods (Methods returning a Task).");
            }

            return new CommandHandlerAttributeMethod(commandType, methodInfo, isAsync, supportsCancellation);
        }

        #endregion Methods

        #region Functions

        /// <summary>
        /// Create a delegate from a synchronous action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [CommandHandler].</typeparam>
        /// <typeparam name="TCommand">Type of command that is handled by the command handler delegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of <typeparamref name="TAttributed"/>.</param>
        /// <returns>Instance of command handler delegate.</returns>
        private CommandHandlerDelegate createWrappedSyncDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory) where TAttributed : class
                                                                                                                                   where TCommand : class, ICommand
        {
            Action<TAttributed, TCommand> action = (Action<TAttributed, TCommand>)MethodInfo.CreateDelegate(typeof(Action<TAttributed, TCommand>));

            return CommandHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, action);
        }

        /// <summary>
        /// Create a delegate from an asynchronous (cancellable) action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [CommandHandler].</typeparam>
        /// <typeparam name="TCommand">Type of command that is handled by the command handler delegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of <typeparamref name="TAttributed"/>.</param>
        /// <returns>Instance of command handler delegate.</returns>
        private CommandHandlerDelegate createCancellableAsyncDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory) where TAttributed : class
                                                                                                                                        where TCommand : class, ICommand
        {
            Func<TAttributed, TCommand, CancellationToken, Task> cancellableAsyncAction = (Func<TAttributed, TCommand, CancellationToken, Task>)MethodInfo.CreateDelegate(typeof(Func<TAttributed, TCommand, CancellationToken, Task>));

            return CommandHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, cancellableAsyncAction);
        }

        /// <summary>
        /// Create a delegate from an asynchronous (non-cancellable) action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [CommandHandler].</typeparam>
        /// <typeparam name="TCommand">Type of command that is handled by the command handler delegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of <typeparamref name="TAttributed"/>.</param>
        /// <returns>Instance of command handler delegate.</returns>
        private CommandHandlerDelegate createNonCancellableAsyncDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory) where TAttributed : class
                                                                                                                                           where TCommand : class, ICommand
        {
            Func<TAttributed, TCommand, Task> asyncAction = (Func<TAttributed, TCommand, Task>)MethodInfo.CreateDelegate(typeof(Func<TAttributed, TCommand, Task>));

            return CommandHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, asyncAction);
        }

        #endregion Functions
    }
}
