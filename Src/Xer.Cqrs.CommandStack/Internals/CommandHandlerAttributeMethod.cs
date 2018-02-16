using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.CommandStack.Attributes;

namespace Xer.Cqrs.CommandStack
{
    internal class CommandHandlerAttributeMethod
    {
        #region Static Declarations
        
        private static readonly ParameterExpression InstanceParameterExpression = Expression.Parameter(typeof(object), "instance");
        private static readonly ParameterExpression CancellationTokenParameterExpression = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        #endregion Static Declarations

        #region Properties

        /// <summary>
        /// Method's declaring type.
        /// </summary>
        /// <returns></returns>
        public Type DeclaringType { get; }

        /// <summary>
        /// Type of command handled by the method.
        /// </summary>
        /// <returns></returns>
        public Type CommandType { get; }
        
        /// <summary>
        /// Method info.
        /// </summary>
        public MethodInfo MethodInfo { get; }

        /// <summary>
        /// Indicates if method is an asynchronous method.
        /// </summary>
        public bool IsAsync { get; }

        /// <summary>
        /// Indicates if method supports cancellation.
        /// </summary>
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
        /// <remarks>This will try to retrieve an instance from <paramref name="attributedObjectFactory"/> to validate.</remarks>
        /// <typeparam name="TCommand">Type of command that is handled by the CommandHandlerDelegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of a class that contains [CommandHandler] methods.</param>
        /// <returns>Instance of CommandHandlerDelegate.</returns>
        public Func<TCommand, CancellationToken, Task> CreateCommandHandlerDelegate<TCommand>(Func<object> attributedObjectFactory) 
            where TCommand : class
        {
            validateInstanceFactory(attributedObjectFactory);

            try
            {
                if (IsAsync)
                {
                    if (SupportsCancellation)
                    {
                        return createCancellableAsyncDelegate<TCommand>(attributedObjectFactory);
                    }
                    else
                    {
                        return createNonCancellableAsyncDelegate<TCommand>(attributedObjectFactory);
                    }
                }
                else
                {
                    return createWrappedSyncDelegate<TCommand>(attributedObjectFactory);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create command handler delegate for {DeclaringType.Name}'s {MethodInfo.ToString()} method.", ex);
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
                throw new InvalidOperationException($"Method info is not marked with [CommandHandler] attribute: See {methodInfo.ToString()} method of {methodInfo.DeclaringType.Name}.");
            }

            // Get all method parameters.
            ParameterInfo[] methodParameters = methodInfo.GetParameters();

            // Get first method parameter that is a class (not struct). This assumes that the first parameter is the command.
            ParameterInfo commandParameter = methodParameters.FirstOrDefault(p => p.ParameterType.GetTypeInfo().IsClass);
            if (commandParameter == null)
            {
                // Method has no parameter.
                throw new InvalidOperationException($"Method info does not accept any parameters: See {methodInfo.ToString()} method of {methodInfo.DeclaringType.Name}.");
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
                throw new InvalidOperationException($"Method marked with [CommandHandler] can only have void or a Task as return value: See {methodInfo.ToString()} method of {methodInfo.DeclaringType.Name}.");
            }

            bool supportsCancellation = methodParameters.Any(p => p.ParameterType == typeof(CancellationToken));

            if (!isAsyncMethod && supportsCancellation)
            {
                throw new InvalidOperationException("Cancellation token support is only available for async methods (Methods returning a Task): See {methodInfo.ToString()} method of {methodInfo.DeclaringType.Name}.");
            }

            return new CommandHandlerAttributeMethod(methodInfo, commandType, isAsyncMethod, supportsCancellation);
        }

        /// <summary>
        /// Create CommandHandlerAttributeMethod from the method info.
        /// </summary>
        /// <param name="methodInfos">Method infos that have CommandHandlerAttribute custom attributes.</param>
        /// <returns>Instances of CommandHandlerAttributeMethod.</returns>
        public static IEnumerable<CommandHandlerAttributeMethod> FromMethodInfos(IEnumerable<MethodInfo> methodInfos)
        {
            if (methodInfos == null)
            {
                throw new ArgumentNullException(nameof(methodInfos));
            }

            return methodInfos.Select(m => FromMethodInfo(m));
        }

        /// <summary>
        /// Detect methods marked with [CommandHandler] attribute and translate to CommandHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="type">Type to scan for methods marked with the [CommandHandler] attribute.</param>
        /// <returns>List of all CommandHandlerAttributeMethod detected.</returns>
        public static IEnumerable<CommandHandlerAttributeMethod> FromType(Type type)
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
        public static IEnumerable<CommandHandlerAttributeMethod> FromTypes(IEnumerable<Type> types)
        {
            if (types == null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            return types.SelectMany(t => FromType(t));
        }

        /// <summary>
        /// Detect methods marked with [CommandHandler] attribute and translate to CommandHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="commandHandlerAssembly">Assembly to scan for methods marked with the [CommandHandler] attribute.</param>
        /// <returns>List of all CommandHandlerAttributeMethod detected.</returns>
        public static IEnumerable<CommandHandlerAttributeMethod> FromAssembly(Assembly commandHandlerAssembly)
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
        public static IEnumerable<CommandHandlerAttributeMethod> FromAssemblies(IEnumerable<Assembly> commandHandlerAssemblies)
        {
            if (commandHandlerAssemblies == null)
            {
                throw new ArgumentNullException(nameof(commandHandlerAssemblies));
            }

            return commandHandlerAssemblies.SelectMany(a => FromAssembly(a));
        }

        #endregion Methods

        #region Functions

        /// <summary>
        /// Create a delegate from an asynchronous (cancellable) action.
        /// </summary>
        /// <typeparam name="TCommand">Type of command that is handled by the CommandHandlerDelegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of a class that contains [CommandHandler] methods.</param>
        /// <returns>Instance of CommandHandlerDelegate.</returns>
        private Func<TCommand, CancellationToken, Task> createCancellableAsyncDelegate<TCommand>(Func<object> attributedObjectFactory) 
            where TCommand : class
        {
            // Create an expression that will invoke the command handler method of a given instance.
            var commandParameterExpression = Expression.Parameter(CommandType, "command");
            var convertToTypeExpression = Expression.Convert(InstanceParameterExpression, DeclaringType);
            var callExpression = Expression.Call(convertToTypeExpression, MethodInfo, commandParameterExpression, CancellationTokenParameterExpression);

            // Lambda signature:
            // (instance, command, cancallationToken) => ((ActualType)instance).HandleCommandAsync(command, cancellationToken);
            var cancellableAsyncDelegate = Expression.Lambda<Func<object, TCommand, CancellationToken, Task>>(callExpression, new[] 
            {  
                InstanceParameterExpression,
                commandParameterExpression,
                CancellationTokenParameterExpression
            }).Compile();

            return CommandHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, cancellableAsyncDelegate);
        }

        /// <summary>
        /// Create a delegate from an asynchronous (non-cancellable) action.
        /// </summary>
        /// <typeparam name="TCommand">Type of command that is handled by the CommandHandlerDelegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of a class that contains [CommandHandler] methods.</param>
        /// <returns>Instance of CommandHandlerDelegate.</returns>
        private Func<TCommand, CancellationToken, Task> createNonCancellableAsyncDelegate<TCommand>(Func<object> attributedObjectFactory) 
            where TCommand : class
        {
            // Create an expression that will invoke the command handler method of a given instance.
            var commandParameterExpression = Expression.Parameter(CommandType, "command");
            var convertToTypeExpression = Expression.Convert(InstanceParameterExpression, DeclaringType);
            var callMethodExpression = Expression.Call(convertToTypeExpression, MethodInfo, commandParameterExpression);
            
            // Lambda signature:
            // (instance, command) => ((ActualType)instance).HandleCommandAsync(command);
            var nonCancellableAsyncDelegate = Expression.Lambda<Func<object, TCommand, Task>>(callMethodExpression, new[] 
            {  
                InstanceParameterExpression,
                commandParameterExpression
            }).Compile();

            return CommandHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, nonCancellableAsyncDelegate);
        }

        /// <summary>
        /// Create a delegate from a synchronous action.
        /// </summary>
        /// <typeparam name="TCommand">Type of command that is handled by the CommandHandlerDelegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of a class that contains [CommandHandler] methods.</param>
        /// <returns>Instance of CommandHandlerDelegate.</returns>
        private Func<TCommand, CancellationToken, Task> createWrappedSyncDelegate<TCommand>(Func<object> attributedObjectFactory) 
            where TCommand : class
        {
            // Create an expression that will invoke the command handler method of a given instance.
            var commandParameterExpression = Expression.Parameter(CommandType, "command");
            var convertToTypeExpression = Expression.Convert(InstanceParameterExpression, DeclaringType);
            var callExpression = Expression.Call(convertToTypeExpression, MethodInfo, commandParameterExpression);

            // Lambda signature:
            // (instance, command) => ((ActualType)instance).HandleCommand(command);
            var action = Expression.Lambda<Action<object, TCommand>>(callExpression, new[] 
            {  
                InstanceParameterExpression,
                commandParameterExpression
            }).Compile();

            return CommandHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, action);
        }

        /// <summary>
        /// Validate that the instance factory returns a valid instance.
        /// </summary>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of a class that contains [CommandHandler] methods.</param>
        private void validateInstanceFactory(Func<object> attributedObjectFactory)
        {
            if (attributedObjectFactory == null)
            {
                throw new ArgumentNullException(nameof(attributedObjectFactory));
            }

            try
            {
                var instance = attributedObjectFactory.Invoke();
                if (instance == null)
                {
                    throw new ArgumentException($"Failed to retrieve an instance from the provided instance factory delegate. Please check registration configuration.");
                }
                
                Type instanceType = instance.GetType();
                if (instanceType != DeclaringType)
                {
                    // Instance does not match this method's declaring type.
                    throw new ArgumentException($"Expected an instance of {DeclaringType} but instance factory provided instance of type {instanceType}.");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error occurred while trying to retrieve an instance from the provided instance factory delegate. Please check registration configuration.", ex);
            }
        }

        #endregion Functions
    }
}
