using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.CommandStack.Attributes;
using Xer.Delegator;

namespace Xer.Cqrs.CommandStack
{
    internal class CommandHandlerAttributeMethod
    {
        #region Properties

        public Type DeclaringType { get; }
        public Type CommandType { get; }
        public MethodInfo MethodInfo { get; }
        public bool IsAsync { get; }
        public bool SupportsCancellation { get; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="methodInfo">Method info.</param>
        /// <param name="commandType">Type of command that is accepted by this method.</param>
        /// <param name="isAsync">Is method an async method?</param>
        /// <param name="supportsCancellation">Does method supports cancellation?</param>
        private CommandHandlerAttributeMethod(MethodInfo methodInfo, Type commandType, bool isAsync, bool supportsCancellation)
        {
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            DeclaringType = methodInfo.DeclaringType;
            CommandType = commandType ?? throw new ArgumentNullException(nameof(commandType));
            IsAsync = isAsync;
            SupportsCancellation = supportsCancellation;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Create a CommandHandlerDelegate based on the internal method info.
        /// </summary>
        /// <typeparam name="TAttributedObject">Type of object that contains methods marked with [CommandHandler].</typeparam>
        /// <typeparam name="TCommand">Type of command that is handled by the CommandHandlerDelegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory which returns an instance of the object with methods that are marked with CommandHandlerAttribute.</param>
        /// <returns>Instance of CommandHandlerDelegate.</returns>
        public MessageHandlerDelegate<TCommand> CreateMessageHandlerDelegate<TAttributedObject, TCommand>(Func<TAttributedObject> attributedObjectFactory) 
            where TAttributedObject : class
            where TCommand : class
        {
            if (attributedObjectFactory == null)
            {
                throw new ArgumentNullException(nameof(attributedObjectFactory));
            }

            if(typeof(TAttributedObject) != DeclaringType)
            {
                throw new ArgumentException($"{typeof(TAttributedObject)} generic parameter does not match command handler method's declaring type.");
            }

            try
            {
                if (IsAsync)
                {
                    if (SupportsCancellation)
                    {
                        return createCancellableAsyncDelegate<TAttributedObject, TCommand>(attributedObjectFactory);
                    }
                    else
                    {
                        return createNonCancellableAsyncDelegate<TAttributedObject, TCommand>(attributedObjectFactory);
                    }
                }
                else
                {
                    return createWrappedSyncDelegate<TAttributedObject, TCommand>(attributedObjectFactory);
                }
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException($"Failed to create message handler delegate for {DeclaringType.Name}'s {MethodInfo.ToString()} method. Check the methods parameters.", ex);
            }
        }

        /// <summary>
        /// Create CommandHandlerAttributeMethod from the method info.
        /// </summary>
        /// <param name="methodInfo">Method info that has CommandHandlerAttribute custom attribute.</param>
        /// <returns>Instance of CommandHandlerAttributeMethod.</returns>
        public static CommandHandlerAttributeMethod FromMethodInfo(MethodInfo methodInfo)
        {
            Type commandType;
            bool isAsyncMethod;

            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            CommandHandlerAttribute commandHandlerAttribute = methodInfo.GetCustomAttribute<CommandHandlerAttribute>();
            if (commandHandlerAttribute == null)
            {
                throw new InvalidOperationException("Method info is not marked with [CommandHandler] attribute.");
            }

            // Get all method parameters.
            ParameterInfo[] methodParameters = methodInfo.GetParameters();

            // Get first method parameter that is a class (not struct). This assumes that the first parameter is the command.
            ParameterInfo commandParameter = methodParameters.FirstOrDefault(p => p.ParameterType.GetTypeInfo().IsClass);
            if (commandParameter == null)
            {
                // Method has no parameter.
                throw new InvalidOperationException($"Method info does not accept any parameters.");
            }
            
            // Set command type.
            commandType = commandParameter.ParameterType;

            // Only valid return types are Task/void.
            if (methodInfo.ReturnType == typeof(Task))
            {
                isAsyncMethod = true;
            }
            else if (methodInfo.ReturnType == typeof(void))
            {
                isAsyncMethod = false;

                // if(methodInfo.CustomAttributes.Any(p => p.AttributeType == typeof(AsyncStateMachineAttribute)))
                // {
                //     throw new InvalidOperationException($"Methods with async void signatures are not allowed. A Task may be used as return type instead of void. Check method: {methodInfo.ToString()}.");
                // }
            }
            else
            {
                // Return type is not Task/void. Invalid.
                throw new InvalidOperationException($"Method marked with [CommandHandler] can only have void or a Task as return value: {methodInfo.Name}");
            }

            bool supportsCancellation = methodParameters.Any(p => p.ParameterType == typeof(CancellationToken));

            if (!isAsyncMethod && supportsCancellation)
            {
                throw new InvalidOperationException("Cancellation token support is only available for async methods (Methods returning a Task).");
            }

            return new CommandHandlerAttributeMethod(methodInfo, commandType, isAsyncMethod, supportsCancellation);
        }

        /// <summary>
        /// Create CommandHandlerAttributeMethod from the method info.
        /// </summary>
        /// <param name="methodInfos">Method infos that have CommandHandlerAttribute custom attributes.</param>
        /// <returns>Instances of CommandHandlerAttributeMethod.</returns>
        public static List<CommandHandlerAttributeMethod> FromMethodInfos(IEnumerable<MethodInfo> methodInfos)
        {
            if (methodInfos == null)
            {
                throw new ArgumentNullException(nameof(methodInfos));
            }

            return methodInfos.Select(m => FromMethodInfo(m)).ToList();
        }

        /// <summary>
        /// Detect methods marked with [CommandHandler] attribute and translate to CommandHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="type">Type to scan for methods marked with the [CommandHandler] attribute.</param>
        /// <returns>List of all CommandHandlerAttributeMethod detected.</returns>
        public static List<CommandHandlerAttributeMethod> FromType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            IEnumerable<MethodInfo> methods = type.GetRuntimeMethods()
                                                  .Where(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(CommandHandlerAttribute)));

            return FromMethodInfos(methods);
        }

        /// <summary>
        /// Detect methods marked with [CommandHandler] attribute and translate to CommandHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="types">Types to scan for methods marked with the [CommandHandler] attribute.</param>
        /// <returns>List of all CommandHandlerAttributeMethod detected.</returns>
        public static List<CommandHandlerAttributeMethod> FromTypes(IEnumerable<Type> types)
        {
            if (types == null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            return types.SelectMany(t => FromType(t)).ToList();
        }

        /// <summary>
        /// Detect methods marked with [CommandHandler] attribute and translate to CommandHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="commandHandlerAssembly">Assembly to scan for methods marked with the [CommandHandler] attribute.</param>
        /// <returns>List of all CommandHandlerAttributeMethod detected.</returns>
        public static List<CommandHandlerAttributeMethod> FromAssembly(Assembly commandHandlerAssembly)
        {
            if (commandHandlerAssembly == null)
            {
                throw new ArgumentNullException(nameof(commandHandlerAssembly));
            }

            IEnumerable<MethodInfo> commandHandlerMethods = commandHandlerAssembly.DefinedTypes.SelectMany(t => 
                                                                t.DeclaredMethods.Where(m => 
                                                                    m.CustomAttributes.Any(a => a.AttributeType == typeof(CommandHandlerAttribute))));
            
            return FromMethodInfos(commandHandlerMethods);
        }

        /// <summary>
        /// Detect methods marked with [CommandHandler] attribute and translate to CommandHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="commandHandlerAssemblies">Assemblies to scan for methods marked with the [CommandHandler] attribute.</param>
        /// <returns>List of all CommandHandlerAttributeMethod detected.</returns>
        public static List<CommandHandlerAttributeMethod> FromAssemblies(IEnumerable<Assembly> commandHandlerAssemblies)
        {
            if (commandHandlerAssemblies == null)
            {
                throw new ArgumentNullException(nameof(commandHandlerAssemblies));
            }

            return commandHandlerAssemblies.SelectMany(a => FromAssembly(a)).ToList();
        }

        #endregion Methods

        #region Functions

        /// <summary>
        /// Create a delegate from a synchronous action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [CommandHandler].</typeparam>
        /// <typeparam name="TCommand">Type of command that is handled by the CommandHandlerDelegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of <typeparamref name="TAttributed"/>.</param>
        /// <returns>Instance of CommandHandlerDelegate.</returns>
        private MessageHandlerDelegate<TCommand> createWrappedSyncDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory) 
            where TAttributed : class
            where TCommand : class
        {
            Action<TAttributed, TCommand> action = (Action<TAttributed, TCommand>)MethodInfo.CreateDelegate(typeof(Action<TAttributed, TCommand>));

            return CommandHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, action);
        }

        /// <summary>
        /// Create a delegate from an asynchronous (cancellable) action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [CommandHandler].</typeparam>
        /// <typeparam name="TCommand">Type of command that is handled by the CommandHandlerDelegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of <typeparamref name="TAttributed"/>.</param>
        /// <returns>Instance of CommandHandlerDelegate.</returns>
        private MessageHandlerDelegate<TCommand> createCancellableAsyncDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory) 
            where TAttributed : class
            where TCommand : class
        {
            Func<TAttributed, TCommand, CancellationToken, Task> cancellableAsyncAction = (Func<TAttributed, TCommand, CancellationToken, Task>)MethodInfo.CreateDelegate(typeof(Func<TAttributed, TCommand, CancellationToken, Task>));

            return CommandHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, cancellableAsyncAction);
        }

        /// <summary>
        /// Create a delegate from an asynchronous (non-cancellable) action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [CommandHandler].</typeparam>
        /// <typeparam name="TCommand">Type of command that is handled by the CommandHandlerDelegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of <typeparamref name="TAttributed"/>.</param>
        /// <returns>Instance of CommandHandlerDelegate.</returns>
        private MessageHandlerDelegate<TCommand> createNonCancellableAsyncDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory) 
            where TAttributed : class
            where TCommand : class
        {
            Func<TAttributed, TCommand, Task> asyncAction = (Func<TAttributed, TCommand, Task>)MethodInfo.CreateDelegate(typeof(Func<TAttributed, TCommand, Task>));

            return CommandHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, asyncAction);
        }

        #endregion Functions
    }
}
