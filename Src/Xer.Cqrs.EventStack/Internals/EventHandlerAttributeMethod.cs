using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.EventStack.Attributes;

namespace Xer.Cqrs.EventStack
{
    internal class EventHandlerAttributeMethod
    {
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
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [EventHandler].</typeparam>
        /// <typeparam name="TEvent">Type of event that is handled by the EventHandlerDelegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory which returns an instance of the object with methods that are marked with EventHandlerAttribute.</param>
        /// <returns>Instance of MessageHandlerDelegate.</returns>
        public Func<TEvent, CancellationToken, Task> CreateMessageHandlerDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory) 
            where TAttributed : class
            where TEvent : class
        {
            if (attributedObjectFactory == null)
            {
                throw new ArgumentNullException(nameof(attributedObjectFactory));
            }

            if (IsAsync)
            {
                if (SupportsCancellation)
                {
                    return createCancellableAsyncDelegate<TAttributed, TEvent>(attributedObjectFactory);
                }
                else
                {
                    return createNonCancellableAsyncDelegate<TAttributed, TEvent>(attributedObjectFactory);
                }
            }
            else
            {
                return createWrappedSyncDelegate<TAttributed, TEvent>(attributedObjectFactory);
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
        /// Create a delegate from a synchronous action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [EventHandler].</typeparam>
        /// <typeparam name="TEvent">Type of event that is handled by the EventHandlerDelegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of <typeparamref name="TAttributed"/>.</param>
        /// <returns>Instance of MessageHandlerDelegate.</returns>
        private Func<TEvent, CancellationToken, Task> createWrappedSyncDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory) 
            where TAttributed : class
            where TEvent : class
        {
            Action<TAttributed, TEvent> action = (Action<TAttributed, TEvent>)MethodInfo.CreateDelegate(typeof(Action<TAttributed, TEvent>));
            
            return EventHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, action, yieldExecution: YieldSynchronousExecution);
        }

        /// <summary>
        /// Create a delegate from an asynchronous (cancellable) action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [EventHandler].</typeparam>
        /// <typeparam name="TEvent">Type of event that is handled by the EventHandlerDelegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of <typeparamref name="TAttributed"/>.</param>
        /// <returns>Instance of MessageHandlerDelegate.</returns>
        private Func<TEvent, CancellationToken, Task> createCancellableAsyncDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory) 
            where TAttributed : class
            where TEvent : class
        {
            Func<TAttributed, TEvent, CancellationToken, Task> asyncCancellableAction = (Func<TAttributed, TEvent, CancellationToken, Task>)MethodInfo.CreateDelegate(typeof(Func<TAttributed, TEvent, CancellationToken, Task>));

            return EventHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, asyncCancellableAction);
        }

        /// <summary>
        /// Create a delegate from an asynchronous (non-cancellable) action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [EventHandler].</typeparam>
        /// <typeparam name="TEvent">Type of event that is handled by the EventHandlerDelegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of <typeparamref name="TAttributed"/>.</param>
        /// <returns>Instance of MessageHandlerDelegate.</returns>
        private Func<TEvent, CancellationToken, Task> createNonCancellableAsyncDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory) where TAttributed : class
            where TEvent : class
        {
            Func<TAttributed, TEvent, Task> asyncAction = (Func<TAttributed, TEvent, Task>)MethodInfo.CreateDelegate(typeof(Func<TAttributed, TEvent, Task>));

            return EventHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, asyncAction);
        }

        #endregion Functions
    }
}
