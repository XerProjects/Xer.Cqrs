using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.EventStack.Attributes;

namespace Xer.Cqrs.EventStack
{
    internal class EventHandlerAttributeMethod
    {
        #region Static Declarations
        
        private static readonly ParameterExpression InstanceParameterExpression = Expression.Parameter(typeof(object), "instance");
        private static readonly ParameterExpression CancellationTokenParameterExpression = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        #endregion Static Declarations
        
        #region Properties

        /// <summary>
        /// Method's declaring type.
        /// </summary>
        public Type DeclaringType { get; }
        
        /// <summary>
        /// Type of event handled by the method.
        /// </summary>
        public Type EventType { get; }

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

        /// <summary>
        /// Indicates if execution should yield for the method.
        /// </summary>
        /// <remarks>
        /// This will never be true for async methods (Method whose IsAsync propoerty is true).
        /// </remarks>
        public bool YieldSynchronousExecution { get; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="methodInfo">Method info.</param>
        /// <param name="eventType">Type of event that is accepted by this method.</param>
        /// <param name="isAsync">Is method an async method?</param>
        /// <param name="supportsCancellation">Does method supports cancellation?</param>
        /// <param name="yieldSynchronousExecution">Should yield synchronous method execution?</param>
        private EventHandlerAttributeMethod(MethodInfo methodInfo, Type eventType, bool isAsync, bool supportsCancellation, bool yieldSynchronousExecution = false)
        {
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            DeclaringType = methodInfo.DeclaringType;
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
            IsAsync = isAsync;
            SupportsCancellation = supportsCancellation;
            YieldSynchronousExecution = !isAsync && yieldSynchronousExecution;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Create a EventHandlerDelegate based on the internal method info.
        /// </summary>
        /// <remarks>This will try to retrieve an instance from <paramref name="attributedObjectFactory"/> to validate.</remarks>
        /// <typeparam name="TEvent">Type of event that is handled by the EventHandlerDelegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which provides an instance of a class that contains methods marked with [EventHandler] attribute.</param>
        /// <returns>Instance of MessageHandlerDelegate.</returns>
        public Func<TEvent, CancellationToken, Task> CreateEventHandlerDelegate<TEvent>(Func<object> attributedObjectFactory) 
            where TEvent : class
        {
            validateInstanceFactory(attributedObjectFactory);

            try
            {
                if (IsAsync)
                {
                    if (SupportsCancellation)
                    {
                        return createCancellableAsyncDelegate<TEvent>(attributedObjectFactory);
                    }
                    else
                    {
                        return createNonCancellableAsyncDelegate<TEvent>(attributedObjectFactory);
                    }
                }
                else
                {
                    return createWrappedSyncDelegate<TEvent>(attributedObjectFactory);
                }
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException($"Failed to create event handler delegate for {DeclaringType.Name}'s {MethodInfo.ToString()} method.", ex);
            }
        }

        /// <summary>
        /// Create EventHandlerAttributeMethod from the method info.
        /// </summary>
        /// <param name="methodInfo">Method info that has EventHandlerAttribute custom attribute.</param>
        /// <returns>Instance of EventHandlerAttributeMethod.</returns>
        public static EventHandlerAttributeMethod FromMethodInfo(MethodInfo methodInfo)
        {
            Type eventType;
            bool isAsyncMethod;

            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            EventHandlerAttribute eventHandlerAttribute = methodInfo.GetCustomAttribute<EventHandlerAttribute>();
            if (eventHandlerAttribute == null)
            {
                throw new InvalidOperationException($"Method info is not marked with [EventHandler] attribute: See {methodInfo.ToString()} method of {methodInfo.DeclaringType.Name}.");
            }

            // Get all method parameters.
            ParameterInfo[] methodParameters = methodInfo.GetParameters();

            // Get first method parameter that is a class (not struct). This assumes that the first parameter is the event.
            ParameterInfo eventParameter = methodParameters.FirstOrDefault();
            if (eventParameter != null)
            {
                // Check if parameter is a class.
                if(!eventParameter.ParameterType.GetTypeInfo().IsClass)
                {
                    throw new InvalidOperationException($"First parameter in method info is not a reference type, only reference type events are supported: See {methodInfo.ToString()} method of {methodInfo.DeclaringType.Name}.");
                }
                           
                // Set command type.
                eventType = eventParameter.ParameterType;
            }
            else
            {                
                // Method has no parameter.
                throw new InvalidOperationException($"Method info does not accept any parameters: See {methodInfo.ToString()} method of {methodInfo.DeclaringType.Name}.");
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
                throw new InvalidOperationException($"Method marked with [CommandHandler] can only have void or a Task as return value: See {methodInfo.ToString()} method of {methodInfo.DeclaringType.Name}.");
            }

            bool supportsCancellation = methodParameters.Any(p => p.ParameterType == typeof(CancellationToken));

            if (!isAsyncMethod && supportsCancellation)
            {
                throw new InvalidOperationException($"Cancellation token support is only available for async methods (Methods returning a Task): See {methodInfo.ToString()} method of {methodInfo.DeclaringType.Name}.");
            }

            // Instantiate.
            return new EventHandlerAttributeMethod(methodInfo, 
                                                   eventType, 
                                                   isAsyncMethod, 
                                                   supportsCancellation, 
                                                   eventHandlerAttribute.YieldSynchronousExecution);
        }

        /// <summary>
        /// Create EventHandlerAttributeMethod from the method info.
        /// </summary>
        /// <param name="methodInfos">Method infos that have EventHandlerAttribute custom attributes.</param>
        /// <returns>Instances of EventHandlerAttributeMethod.</returns>
        public static IEnumerable<EventHandlerAttributeMethod> FromMethodInfos(IEnumerable<MethodInfo> methodInfos)
        {
            if (methodInfos == null)
            {
                throw new ArgumentNullException(nameof(methodInfos));
            }

            return methodInfos.Select(m => FromMethodInfo(m));
        }

        /// <summary>
        /// Detect methods marked with [EventHandler] attribute and translate to EventHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="type">Type to scan for methods marked with the [EventHandler] attribute.</param>
        /// <returns>List of all EventHandlerAttributeMethod detected.</returns>
        public static IEnumerable<EventHandlerAttributeMethod> FromType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            IEnumerable<MethodInfo> methods = type.GetRuntimeMethods()
                                                  .Where(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(EventHandlerAttribute)));

            return FromMethodInfos(methods);
        }

        /// <summary>
        /// Detect methods marked with [EventHandler] attribute and translate to EventHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="types">Types to scan for methods marked with the [EventHandler] attribute.</param>
        /// <returns>List of all EventHandlerAttributeMethod detected.</returns>
        public static IEnumerable<EventHandlerAttributeMethod> FromTypes(IEnumerable<Type> types)
        {
            if (types == null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            return types.SelectMany(t => FromType(t));
        }

        /// <summary>
        /// Detect methods marked with [EventHandler] attribute and translate to EventHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="eventHandlerAssembly">Assembly to scan for methods marked with the [EventHandler] attribute.</param>
        /// <returns>List of all EventHandlerAttributeMethod detected.</returns>
        public static IEnumerable<EventHandlerAttributeMethod> FromAssembly(Assembly eventHandlerAssembly)
        {
            if (eventHandlerAssembly == null)
            {
                throw new ArgumentNullException(nameof(eventHandlerAssembly));
            }

            IEnumerable<MethodInfo> eventHandlerMethods = eventHandlerAssembly.DefinedTypes.SelectMany(t => 
                                                                t.DeclaredMethods.Where(m => 
                                                                    m.CustomAttributes.Any(a => a.AttributeType == typeof(EventHandlerAttribute))));
            
            return FromMethodInfos(eventHandlerMethods);
        }

        /// <summary>
        /// Detect methods marked with [CommandHandler] attribute and translate to CommandHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="eventHandlerAssemblies">Assemblies to scan for methods marked with the [CommandHandler] attribute.</param>
        /// <returns>List of all CommandHandlerAttributeMethod detected.</returns>
        public static IEnumerable<EventHandlerAttributeMethod> FromAssemblies(IEnumerable<Assembly> eventHandlerAssemblies)
        {
            if (eventHandlerAssemblies == null)
            {
                throw new ArgumentNullException(nameof(eventHandlerAssemblies));
            }

            return eventHandlerAssemblies.SelectMany(a => FromAssembly(a));
        }

        #endregion Methods

        #region Functions

        /// <summary>
        /// Create a delegate from an asynchronous (cancellable) action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [EventHandler].</typeparam>
        /// <typeparam name="TEvent">Type of event that is handled by the EventHandlerDelegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which provides an instance of a class that contains methods marked with [EventHandler] attribute.</param>
        /// <returns>Instance of MessageHandlerDelegate.</returns>
        private Func<TEvent, CancellationToken, Task> createCancellableAsyncDelegate<TEvent>(Func<object> attributedObjectFactory)
            where TEvent : class
        {
            // Create an expression that will invoke the command handler method of a given instance.
            var commandParameterExpression = Expression.Parameter(EventType, "event");
            var convertToTypeExpression = Expression.Convert(InstanceParameterExpression, DeclaringType);
            var callExpression = Expression.Call(convertToTypeExpression, MethodInfo, commandParameterExpression, CancellationTokenParameterExpression);

            // Lambda signature:
            // (instance, command, cancallationToken) => ((ActualType)instance).HandleCommandAsync(command, cancellationToken);
            var cancellableAsyncDelegate = Expression.Lambda<Func<object, TEvent, CancellationToken, Task>>(callExpression, new[] 
            {  
                InstanceParameterExpression,
                commandParameterExpression,
                CancellationTokenParameterExpression
            }).Compile();

            return EventHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, cancellableAsyncDelegate);
        }

        /// <summary>
        /// Create a delegate from an asynchronous (non-cancellable) action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [EventHandler].</typeparam>
        /// <typeparam name="TEvent">Type of event that is handled by the EventHandlerDelegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which provides an instance of a class that contains methods marked with [EventHandler] attribute.</param>
        /// <returns>Instance of MessageHandlerDelegate.</returns>
        private Func<TEvent, CancellationToken, Task> createNonCancellableAsyncDelegate<TEvent>(Func<object> attributedObjectFactory)
            where TEvent : class
        {
            // Create an expression that will invoke the command handler method of a given instance.
            var commandParameterExpression = Expression.Parameter(EventType, "event");
            var convertToTypeExpression = Expression.Convert(InstanceParameterExpression, DeclaringType);
            var callExpression = Expression.Call(convertToTypeExpression, MethodInfo, commandParameterExpression);

            // Lambda signature:
            // (instance, command) => ((ActualType)instance).HandleCommandAsync(command);
            var nonCancellableAsyncDelegate = Expression.Lambda<Func<object, TEvent, Task>>(callExpression, new[] 
            {  
                InstanceParameterExpression,
                commandParameterExpression
            }).Compile();

            return EventHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, nonCancellableAsyncDelegate);
        }

        /// <summary>
        /// Create a delegate from a synchronous action.
        /// </summary>
        /// <typeparam name="TEvent">Type of event that is handled by the EventHandlerDelegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which provides an instance of a class that contains methods marked with [EventHandler] attribute.</param>
        /// <returns>Instance of MessageHandlerDelegate.</returns>
        private Func<TEvent, CancellationToken, Task> createWrappedSyncDelegate<TEvent>(Func<object> attributedObjectFactory)
            where TEvent : class
        {
            // Create an expression that will invoke the command handler method of a given instance.
            var commandParameterExpression = Expression.Parameter(EventType, "event");
            var convertToTypeExpression = Expression.Convert(InstanceParameterExpression, DeclaringType);
            var callExpression = Expression.Call(convertToTypeExpression, MethodInfo, commandParameterExpression);

            // Lambda signature:
            // (instance, command) => ((ActualType)instance).HandleCommand(command);
            var action = Expression.Lambda<Action<object, TEvent>>(callExpression, new[] 
            {  
                InstanceParameterExpression,
                commandParameterExpression
            }).Compile();

            return EventHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, action, yieldExecution: YieldSynchronousExecution);
        }

        /// <summary>
        /// Validate that the instance factory returns a valid instance.
        /// </summary>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of a class that contains [EventHandler] methods.</param>
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
