using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Attributes;

namespace Xer.Delegator.Registrations
{
    /// <summary>
    /// Represents a single method that is marked with an [CommandHandler] attribute.
    /// <para>Supported signatures for methods marked with [CommandHandler] are: (Methods can be named differently)</para>
    /// <para>- void HandleCommand(TCommand command);</para>
    /// <para>- Task HandleCommandAsync(TCommand command);</para>
    /// <para>- Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);</para>
    /// </summary>
    public class CommandHandlerAttributeMethod
    {
        #region Static Declarations
        
        private static readonly ParameterExpression CancellationTokenParameterExpression = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
        private static readonly MethodInfo CreateWrappedSyncDelegateOpenGenericMethodInfo = typeof(CommandHandlerAttributeMethod).GetTypeInfo().GetDeclaredMethod(nameof(createWrappedSyncDelegate));
        private static readonly MethodInfo CreateCancellableAsyncDelegateOpenGenericMethodInfo = typeof(CommandHandlerAttributeMethod).GetTypeInfo().GetDeclaredMethod(nameof(createCancellableAsyncDelegate));
        private static readonly MethodInfo CreateNonCancellableAsyncDelegateOpenGenericMethodInfo = typeof(CommandHandlerAttributeMethod).GetTypeInfo().GetDeclaredMethod(nameof(createNonCancellableAsyncDelegate));

        #endregion Static Declarations

        #region Properties

        /// <summary>
        /// Method's declaring type.
        /// </summary>
        /// <returns></returns>
        public Type DeclaringType { get; }

        /// <summary>
        /// Factory delegate that provides an instance of this method's declaring type.
        /// </summary>
        public Func<object> InstanceFactory { get; }

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
        /// <param name="instanceFactory">Factory delegate that provides an instance of the method info's declaring type.</param>
        /// <param name="isAsync">Is method an async method?</param>
        /// <param name="supportsCancellation">Does method supports cancellation?</param>
        private CommandHandlerAttributeMethod(MethodInfo methodInfo, Type commandType, Func<object> instanceFactory, bool isAsync, bool supportsCancellation)
        {
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            DeclaringType = methodInfo.DeclaringType;
            CommandType = commandType ?? throw new ArgumentNullException(nameof(commandType));
            InstanceFactory = instanceFactory;
            IsAsync = isAsync;
            SupportsCancellation = supportsCancellation;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Create a delegate that handles a command that is specified in <see cref="Xer.Delegator.Registrations.CommandHandlerAttributeMethod.CommandType"/>.
        /// </summary>
        /// <returns>Delegate that handles a command that is specified in <see cref="Xer.Delegator.Registrations.CommandHandlerAttributeMethod.CommandType"/>.</returns>
        public Func<object, CancellationToken, Task> CreateCommandHandlerDelegate()
        {
            try
            {
                if (IsAsync)
                {
                    if (SupportsCancellation)
                    {
                        // Invoke createCancellableAsyncDelegate<TDeclaringType, TCommand>(attributedObjectFactory)
                        return (Func<object, CancellationToken, Task>)CreateCancellableAsyncDelegateOpenGenericMethodInfo
                            .MakeGenericMethod(DeclaringType, CommandType)
                            .Invoke(this, new object[] { InstanceFactory });
                    }
                    else
                    {
                        
                        // Invoke createNonCancellableAsyncDelegate<TDeclaringType, TCommand>(attributedObjectFactory)
                        return (Func<object, CancellationToken, Task>)CreateNonCancellableAsyncDelegateOpenGenericMethodInfo
                            .MakeGenericMethod(DeclaringType,CommandType)
                            .Invoke(this, new object[] { InstanceFactory });
                    }
                }
                else
                {
                    // Invoke createWrappedSyncDelegate<TDeclaringType, TCommand>(attributedObjectFactory)
                    return (Func<object, CancellationToken, Task>)CreateWrappedSyncDelegateOpenGenericMethodInfo
                        .MakeGenericMethod(DeclaringType, CommandType)
                        .Invoke(this, new object[] { InstanceFactory });
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
        /// <param name="instanceFactory">Factory delegate that provides an instance of the method info's declaring type.</param>
        /// <returns>Instance of CommandHandlerAttributeMethod.</returns>
        public static CommandHandlerAttributeMethod FromMethodInfo(MethodInfo methodInfo, Func<object> instanceFactory)
        {
            Type commandType;
            bool isAsyncMethod;

            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            if (!IsValid(methodInfo))
            {
                throw new InvalidOperationException($"Method is not marked with [CommandHandler] attribute. {createCheckMethodMessage(methodInfo)}.");
            }

            // Get all method parameters.
            ParameterInfo[] methodParameters = methodInfo.GetParameters();

            // Get first method parameter that is a class (not struct). This assumes that the first parameter is the command.
            ParameterInfo commandParameter = methodParameters.FirstOrDefault();
            if (commandParameter != null)
            {
                // Check if parameter is a class.
                if (!commandParameter.ParameterType.GetTypeInfo().IsClass)
                {
                    throw new InvalidOperationException($"Method's command parameter is not a reference type, only reference type commands are supported. {createCheckMethodMessage(methodInfo)}.");
                }
                           
                // Set command type.
                commandType = commandParameter.ParameterType;
            }
            else
            {                
                // Method has no parameter.
                throw new InvalidOperationException($"Method must accept a command object as a parameter. {createCheckMethodMessage(methodInfo)}.");
            }

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
                throw new InvalidOperationException($"Method marked with [CommandHandler] can only have void or a Task as return value. {createCheckMethodMessage(methodInfo)}.");
            }

            bool supportsCancellation = methodParameters.Any(p => p.ParameterType == typeof(CancellationToken));

            if (!isAsyncMethod && supportsCancellation)
            {
                throw new InvalidOperationException($"Cancellation token support is only available for async methods (methods returning a Task). {createCheckMethodMessage(methodInfo)}.");
            }

            return new CommandHandlerAttributeMethod(methodInfo, commandType, instanceFactory, isAsyncMethod, supportsCancellation);

            // Local function.
            string createCheckMethodMessage(MethodInfo method) => $"Check {methodInfo.DeclaringType.Name}'s {methodInfo.ToString()} method";
        }

        /// <summary>
        /// Create CommandHandlerAttributeMethod from the method info.
        /// </summary>
        /// <param name="methodInfos">Method infos that have CommandHandlerAttribute custom attributes.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of a method info's declaring type.</param>
        /// <returns>Instances of CommandHandlerAttributeMethod.</returns>
        public static IEnumerable<CommandHandlerAttributeMethod> FromMethodInfos(IEnumerable<MethodInfo> methodInfos, Func<Type, object> instanceFactory)
        {
            if (methodInfos == null)
            {
                throw new ArgumentNullException(nameof(methodInfos));
            }

            return methodInfos.Select(m => FromMethodInfo(m, () => instanceFactory.Invoke(m.DeclaringType)));
        }

        /// <summary>
        /// Detect methods marked with [CommandHandler] attribute and translate to CommandHandlerAttributeMethod instances.
        /// </summary>
        /// <typeparam name="T">Type to scan for methods marked with the [CommandHandler] attribute.</typeparam>
        /// <param name="instanceFactory">Factory delegate that provides an instance of the specified type.</param>
        /// <returns>List of all CommandHandlerAttributeMethod detected.</returns>
        public static IEnumerable<CommandHandlerAttributeMethod> FromType<T>(Func<T> instanceFactory) where T : class
        {
            return FromType(typeof(T), instanceFactory);
        }

        /// <summary>
        /// Detect methods marked with [CommandHandler] attribute and translate to CommandHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="type">Type to scan for methods marked with the [CommandHandler] attribute.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of the specified type.</param>
        /// <returns>List of all CommandHandlerAttributeMethod detected.</returns>
        public static IEnumerable<CommandHandlerAttributeMethod> FromType(Type type, Func<object> instanceFactory)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            IEnumerable<MethodInfo> methods = type.GetTypeInfo().DeclaredMethods.Where(m => IsValid(m));

            return FromMethodInfos(methods, _ => instanceFactory.Invoke());
        }

        /// <summary>
        /// Detect methods marked with [CommandHandler] attribute and translate to CommandHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="types">Types to scan for methods marked with the [CommandHandler] attribute.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of a given type.</param>
        /// <returns>List of all CommandHandlerAttributeMethod detected.</returns>
        public static IEnumerable<CommandHandlerAttributeMethod> FromTypes(IEnumerable<Type> types, Func<Type, object> instanceFactory)
        {
            if (types == null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            return types.SelectMany(type => FromType(type, () => instanceFactory.Invoke(type)));
        }

        /// <summary>
        /// Detect methods marked with [CommandHandler] attribute and translate to CommandHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="commandHandlerAssembly">Assembly to scan for methods marked with the [CommandHandler] attribute.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of a type that has methods marked with [CommandHandler] attribute.</param>
        /// <returns>List of all CommandHandlerAttributeMethod detected.</returns>
        public static IEnumerable<CommandHandlerAttributeMethod> FromAssembly(Assembly commandHandlerAssembly, Func<Type, object> instanceFactory)
        {
            if (commandHandlerAssembly == null)
            {
                throw new ArgumentNullException(nameof(commandHandlerAssembly));
            }

            IEnumerable<MethodInfo> commandHandlerMethods = commandHandlerAssembly.DefinedTypes
                                                                .Where(typeInfo => IsFoundInType(typeInfo))
                                                                .SelectMany(typeInfo => typeInfo.DeclaredMethods.Where(method => 
                                                                    IsValid(method)));
            
            return FromMethodInfos(commandHandlerMethods, instanceFactory);
        }

        /// <summary>
        /// Detect methods marked with [CommandHandler] attribute and translate to CommandHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="commandHandlerAssemblies">Assemblies to scan for methods marked with the [CommandHandler] attribute.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of a type that has methods marked with [CommandHandler] attribute.</param>
        /// <returns>List of all CommandHandlerAttributeMethod detected.</returns>
        public static IEnumerable<CommandHandlerAttributeMethod> FromAssemblies(IEnumerable<Assembly> commandHandlerAssemblies, Func<Type, object> instanceFactory)
        {
            if (commandHandlerAssemblies == null)
            {
                throw new ArgumentNullException(nameof(commandHandlerAssemblies));
            }

            return commandHandlerAssemblies.SelectMany(assembly => FromAssembly(assembly, instanceFactory));
        }

        /// <summary>
        /// Check if a method marked with [CommandHandler] attribute is found in the specified type.
        /// </summary>
        /// <param name="type">Type to search for methods marked with [CommandHandler] attribute.</param>
        /// <returns>True if atleast on method is found. Otherwise, false.</returns>
        public static bool IsFoundInType(Type type)
        {
            return IsFoundInType(type.GetTypeInfo());
        }

        /// <summary>
        /// Check if a method marked with [CommandHandler] attribute is found in the specified type.
        /// </summary>
        /// <param name="typeInfo">Type to search for methods marked with [CommandHandler] attribute.</param>
        /// <returns>True if atleast on method is found. Otherwise, false.</returns>
        public static bool IsFoundInType(TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            return typeInfo.DeclaredMethods.Any(method => IsValid(method));
        }

        /// <summary>
        /// Check if method is marked with [CommandHandler] attribute.
        /// </summary>
        /// <param name="methodInfo">Method to search for a [CommandHandler] attribute.</param>
        /// <returns>True if attribute is found. Otherwise, false.</returns>
        public static bool IsValid(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            return methodInfo.GetCustomAttributes(typeof(CommandHandlerAttribute), true).Any();
        }

        #endregion Methods

        #region Functions

        /// <summary>
        /// Create a delegate from an asynchronous (cancellable) action.
        /// </summary>
        /// <typeparam name="TAttributed">Type that contains [CommandHandler] methods. This should match DeclaringType property.</typeparam>
        /// <typeparam name="TCommand">Type of command that is handled by the CommandHandlerAttributeMethod. This should match CommandType property.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of a given type.</param>
        /// <returns>Delegate that handles a command.</returns>
        private Func<object, CancellationToken, Task> createCancellableAsyncDelegate<TAttributed, TCommand>(Func<object> attributedObjectFactory) 
            where TAttributed : class
            where TCommand : class
        {
            // Create an expression that will invoke the command handler method of a given instance.
            var instanceParameterExpression = Expression.Parameter(typeof(TAttributed), "instance");
            var commandParameterExpression = Expression.Parameter(typeof(TCommand), "command");
            var callExpression = Expression.Call(instanceParameterExpression, MethodInfo, commandParameterExpression, CancellationTokenParameterExpression);

            // Lambda signature:
            // (instance, command, cancallationToken) => instance.HandleCommandAsync(command, cancellationToken);
            var cancellableAsyncDelegate = Expression.Lambda<Func<TAttributed, TCommand, CancellationToken, Task>>(callExpression, new[] 
            {  
                instanceParameterExpression,
                commandParameterExpression,
                CancellationTokenParameterExpression
            }).Compile();

            Func<TCommand, CancellationToken, Task> genericDelegate = CommandHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, cancellableAsyncDelegate);
            
            return (obj, cancellationToken) => 
            {
                if (obj is TCommand command)
                {
                    return genericDelegate.Invoke(command, cancellationToken);
                }

                throw new ArgumentException($"Invalid command. Expected command of type {typeof(TCommand).Name} but was given {obj.GetType().Name}.", nameof(obj));
            };
        }

        /// <summary>
        /// Create a delegate from an asynchronous (non-cancellable) action.
        /// </summary>
        /// <typeparam name="TAttributed">Type that contains [CommandHandler] methods. This should match DeclaringType property.</typeparam>
        /// <typeparam name="TCommand">Type of command that is handled by the CommandHandlerAttributeMethod. This should match CommandType property.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of a given type.</param>
        /// <returns>Delegate that handles a command.</returns>
        private Func<object, CancellationToken, Task> createNonCancellableAsyncDelegate<TAttributed, TCommand>(Func<object> attributedObjectFactory) 
            where TAttributed : class
            where TCommand : class
        {
            // Create an expression that will invoke the command handler method of a given instance.
            var instanceParameterExpression = Expression.Parameter(typeof(TAttributed), "instance");
            var commandParameterExpression = Expression.Parameter(typeof(TCommand), "command");
            var callMethodExpression = Expression.Call(instanceParameterExpression, MethodInfo, commandParameterExpression);
            
            // Lambda signature:
            // (instance, command) => instance.HandleCommandAsync(command);
            var nonCancellableAsyncDelegate = Expression.Lambda<Func<TAttributed, TCommand, Task>>(callMethodExpression, new[] 
            {  
                instanceParameterExpression,
                commandParameterExpression
            }).Compile();

            Func<TCommand, CancellationToken, Task> genericDelegate = CommandHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, nonCancellableAsyncDelegate);
            
            return (obj, cancellationToken) => 
            {
                if (obj is TCommand command)
                {
                    return genericDelegate.Invoke(command, cancellationToken);
                }

                throw new ArgumentException($"Invalid command. Expected command of type {typeof(TCommand).Name} but was given {obj.GetType().Name}.", nameof(obj));
            };
        }

        /// <summary>
        /// Create a delegate from a synchronous action.
        /// </summary>
        /// <typeparam name="TAttributed">Type that contains [CommandHandler] methods. This should match DeclaringType property.</typeparam>
        /// <typeparam name="TCommand">Type of command that is handled by the CommandHandlerAttributeMethod. This should match CommandType property.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of a given type.</param>
        /// <returns>Delegate that handles a command.</returns>
        private Func<object, CancellationToken, Task> createWrappedSyncDelegate<TAttributed, TCommand>(Func<object> attributedObjectFactory) 
            where TAttributed : class
            where TCommand : class
        {
            // Create an expression that will invoke the command handler method of a given instance.
            var instanceParameterExpression = Expression.Parameter(typeof(TAttributed), "instance");
            var commandParameterExpression = Expression.Parameter(typeof(TCommand), "command");
            var callExpression = Expression.Call(instanceParameterExpression, MethodInfo, commandParameterExpression);

            // Lambda signature:
            // (instance, command) => instance.HandleCommand(command);
            var action = Expression.Lambda<Action<TAttributed, TCommand>>(callExpression, new[] 
            {  
                instanceParameterExpression,
                commandParameterExpression
            }).Compile();

            Func<TCommand, CancellationToken, Task> genericDelegate = CommandHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, action);
        
            return (obj, cancellationToken) => 
            {
                if (obj is TCommand command)
                {
                    return genericDelegate.Invoke(command, cancellationToken);
                }

                throw new ArgumentException($"Invalid command. Expected command of type {typeof(TCommand).Name} but was given {obj.GetType().Name}.", nameof(obj));
            };
        }

        #endregion Functions
    }
}
