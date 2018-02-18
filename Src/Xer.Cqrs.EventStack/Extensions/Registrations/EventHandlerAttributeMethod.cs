using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Attributes;

namespace Xer.Delegator.Registrations
{
    /// <summary>
    /// Represents a single method that is marked with an [EventHandler] attribute.
    /// <para>Supported signatures for methods marked with [EventHandler] are: (Methods can be named differently)</para>
    /// <para>- void HandleEvent(TEvent event);</para>
    /// <para>- Task HandleEventAsync(TEvent event);</para>
    /// <para>- Task HandleEventAsync(TEvent event, CancellationToken cancellationToken);</para>
    /// </summary>
    public class EventHandlerAttributeMethod
    {
        #region Static Declarations
        
        private static readonly ParameterExpression CancellationTokenParameterExpression = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
        private static readonly MethodInfo CreateWrappedSyncDelegateOpenGenericMethodInfo = typeof(EventHandlerAttributeMethod).GetTypeInfo().GetDeclaredMethod(nameof(createWrappedSyncDelegate));
        private static readonly MethodInfo CreateCancellableAsyncDelegateOpenGenericMethodInfo = typeof(EventHandlerAttributeMethod).GetTypeInfo().GetDeclaredMethod(nameof(createCancellableAsyncDelegate));
        private static readonly MethodInfo CreateNonCancellableAsyncDelegateOpenGenericMethodInfo = typeof(EventHandlerAttributeMethod).GetTypeInfo().GetDeclaredMethod(nameof(createNonCancellableAsyncDelegate));

        #endregion Static Declarations
        
        #region Properties

        /// <summary>
        /// Method's declaring type.
        /// </summary>
        public Type DeclaringType { get; }
        
        /// <summary>
        /// Factory delegate that provides an instance of this method's declaring type.
        /// </summary>
        public Func<object> InstanceFactory { get; }
        
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
        /// <param name="instanceFactory">Factory delegate that provides an instance of the method info's declaring type.</param>
        /// <param name="isAsync">Is method an async method?</param>
        /// <param name="supportsCancellation">Does method supports cancellation?</param>
        /// <param name="yieldSynchronousExecution">Should yield synchronous method execution?</param>
        private EventHandlerAttributeMethod(MethodInfo methodInfo, Type eventType, Func<object> instanceFactory, bool isAsync, bool supportsCancellation, bool yieldSynchronousExecution = false)
        {
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            DeclaringType = methodInfo.DeclaringType;
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
            InstanceFactory = instanceFactory;
            IsAsync = isAsync;
            SupportsCancellation = supportsCancellation;
            YieldSynchronousExecution = !isAsync && yieldSynchronousExecution;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Create a delegate that handles an event that is specified in <see cref="Xer.Delegator.Registrations.EventHandlerAttributeMethod.EventType"/>.
        /// </summary>
        /// <returns>Delegate that handles an event that is specified in <see cref="Xer.Delegator.Registrations.EventHandlerAttributeMethod.EventType"/>.</returns>
        public Func<object, CancellationToken, Task> CreateEventHandlerDelegate()
        {
            try
            {
                if (IsAsync)
                {
                    if (SupportsCancellation)
                    {
                        // Invoke createCancellableAsyncDelegate<TDeclaringType, TEvent>(attributedObjectFactory)
                        return (Func<object, CancellationToken, Task>)CreateCancellableAsyncDelegateOpenGenericMethodInfo
                            .MakeGenericMethod(DeclaringType, EventType)
                            .Invoke(this, new[] {  InstanceFactory });
                    }
                    else
                    {
                        // Invoke createNonCancellableAsyncDelegate<TDeclaringType, TEvent>(attributedObjectFactory)
                        return (Func<object, CancellationToken, Task>)CreateNonCancellableAsyncDelegateOpenGenericMethodInfo
                            .MakeGenericMethod(DeclaringType, EventType)
                            .Invoke(this, new[] {  InstanceFactory });
                    }
                }
                else
                {
                    // Invoke createWrappedSyncDelegate<TDeclaringType, TEvent>(attributedObjectFactory)
                    return (Func<object, CancellationToken, Task>)CreateWrappedSyncDelegateOpenGenericMethodInfo
                        .MakeGenericMethod(DeclaringType, EventType)
                        .Invoke(this, new[] {  InstanceFactory });
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
        /// <param name="instanceFactory">Factory delegate that provides an instance of the method info's declaring type.</param>
        /// <returns>Instance of EventHandlerAttributeMethod.</returns>
        public static EventHandlerAttributeMethod FromMethodInfo(MethodInfo methodInfo, Func<object> instanceFactory)
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
                throw new InvalidOperationException($"Method is not marked with [EventHandler] attribute. {createCheckMethodMessage(methodInfo)}.");
            }

            // Get all method parameters.
            ParameterInfo[] methodParameters = methodInfo.GetParameters();

            // Get first method parameter that is a class (not struct). This assumes that the first parameter is the event.
            ParameterInfo eventParameter = methodParameters.FirstOrDefault();
            if (eventParameter != null)
            {
                // Check if parameter is a class.
                if (!eventParameter.ParameterType.GetTypeInfo().IsClass)
                {
                    throw new InvalidOperationException($"Method's event parameter is not a reference type, only reference type events are supported. {createCheckMethodMessage(methodInfo)}.");
                }
                           
                // Set event type.
                eventType = eventParameter.ParameterType;
            }
            else
            {                
                // Method has no parameter.
                throw new InvalidOperationException($"Method must accept an event object as a parameter. {createCheckMethodMessage(methodInfo)}.");
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
                throw new InvalidOperationException($"Method marked with [EventHandler] can only have void or a Task as return value. {createCheckMethodMessage(methodInfo)}.");
            }

            bool supportsCancellation = methodParameters.Any(p => p.ParameterType == typeof(CancellationToken));

            if (!isAsyncMethod && supportsCancellation)
            {
                throw new InvalidOperationException($"Cancellation token support is only available for async methods (methods returning a Task). {createCheckMethodMessage(methodInfo)}.");
            }

            // Instantiate.
            return new EventHandlerAttributeMethod(methodInfo, 
                                                   eventType,
                                                   instanceFactory,
                                                   isAsyncMethod, 
                                                   supportsCancellation, 
                                                   eventHandlerAttribute.YieldSynchronousExecution);
            
            // Local function.
            string createCheckMethodMessage(MethodInfo method) => $"Check {method.DeclaringType.Name}'s {method.ToString()} method";
        }

        /// <summary>
        /// Create EventHandlerAttributeMethod from the method info.
        /// </summary>
        /// <param name="methodInfos">Method infos that have EventHandlerAttribute custom attributes.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of a method info's declaring type.</param>
        /// <returns>Instances of EventHandlerAttributeMethod.</returns>
        public static IEnumerable<EventHandlerAttributeMethod> FromMethodInfos(IEnumerable<MethodInfo> methodInfos, Func<Type, object> instanceFactory)
        {
            if (methodInfos == null)
            {
                throw new ArgumentNullException(nameof(methodInfos));
            }

            return methodInfos.Select(method => FromMethodInfo(method, () => instanceFactory.Invoke(method.DeclaringType)));
        }

        /// <summary>
        /// Detect methods marked with [EventHandler] attribute and translate to EventHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="type">Type to scan for methods marked with the [EventHandler] attribute.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of the specified type.</param>
        /// <returns>List of all EventHandlerAttributeMethod detected.</returns>
        public static IEnumerable<EventHandlerAttributeMethod> FromType<T>(Func<T> instanceFactory) where T : class
        {
            return FromType(typeof(T), instanceFactory);
        }

        /// <summary>
        /// Detect methods marked with [EventHandler] attribute and translate to EventHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="type">Type to scan for methods marked with the [EventHandler] attribute.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of the specified type.</param>
        /// <returns>List of all EventHandlerAttributeMethod detected.</returns>
        public static IEnumerable<EventHandlerAttributeMethod> FromType(Type type, Func<object> instanceFactory)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            IEnumerable<MethodInfo> methods = type.GetTypeInfo().DeclaredMethods
                                                  .Where(m => m.GetCustomAttributes(typeof(EventHandlerAttribute), true).Any());

            return FromMethodInfos(methods, _ => instanceFactory.Invoke());
        }

        /// <summary>
        /// Detect methods marked with [EventHandler] attribute and translate to EventHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="types">Types to scan for methods marked with the [EventHandler] attribute.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of a given type.</param>
        /// <returns>List of all EventHandlerAttributeMethod detected.</returns>
        public static IEnumerable<EventHandlerAttributeMethod> FromTypes(IEnumerable<Type> types, Func<Type, object> instanceFactory)
        {
            if (types == null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            return types.SelectMany(type => FromType(type, () => instanceFactory.Invoke(type)));
        }

        /// <summary>
        /// Detect methods marked with [EventHandler] attribute and translate to EventHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="eventHandlerAssembly">Assembly to scan for methods marked with the [EventHandler] attribute.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of a type that has methods marked with [EventHandler] attribute.</param>
        /// <returns>List of all EventHandlerAttributeMethod detected.</returns>
        public static IEnumerable<EventHandlerAttributeMethod> FromAssembly(Assembly eventHandlerAssembly, Func<Type, object> instanceFactory)
        {
            if (eventHandlerAssembly == null)
            {
                throw new ArgumentNullException(nameof(eventHandlerAssembly));
            }

            IEnumerable<MethodInfo> eventHandlerMethods = eventHandlerAssembly.DefinedTypes.SelectMany(type => 
                                                                type.DeclaredMethods.Where(method => 
                                                                    method.GetCustomAttributes(typeof(EventHandlerAttribute), true).Any()));
            
            return FromMethodInfos(eventHandlerMethods, instanceFactory);
        }

        /// <summary>
        /// Detect methods marked with [EventHandler] attribute and translate to EventHandlerAttributeMethod instances.
        /// </summary>
        /// <param name="eventHandlerAssemblies">Assemblies to scan for methods marked with the [EventHandler] attribute.</param>
        /// <param name="instanceFactory">Factory delegate that provides an instance of a type that has methods marked with [EventHandler] attribute.</param>
        /// <returns>List of all EventHandlerAttributeMethod detected.</returns>
        public static IEnumerable<EventHandlerAttributeMethod> FromAssemblies(IEnumerable<Assembly> eventHandlerAssemblies, Func<Type, object> instanceFactory)
        {
            if (eventHandlerAssemblies == null)
            {
                throw new ArgumentNullException(nameof(eventHandlerAssemblies));
            }

            return eventHandlerAssemblies.SelectMany(assembly => FromAssembly(assembly, instanceFactory));
        }

        /// <summary>
        /// Check if a method marked with [EventHandler] attribute is found in the specified type.
        /// </summary>
        /// <param name="type">Type to search for methods marked with [EventHandler] attribute.</param>
        /// <returns>True if atleast on method is found. Otherwise, false.</returns>
        public static bool IsFoundInType(Type type)
        {
            return IsFoundInType(type.GetTypeInfo());
        }

        /// <summary>
        /// Check if a method marked with [EventHandler] attribute is found in the specified type.
        /// </summary>
        /// <param name="typeInfo">Type to search for methods marked with [EventHandler] attribute.</param>
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
        /// Check if method is marked with [EventHandler] attribute.
        /// </summary>
        /// <param name="methodInfo">Method to search for a [EventHandler] attribute.</param>
        /// <returns>True if attribute is found. Otherwise, false.</returns>
        public static bool IsValid(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            return methodInfo.GetCustomAttributes(typeof(EventHandlerAttribute), true).Any();
        }

        #endregion Methods

        #region Functions

        /// <summary>
        /// Create a delegate from an asynchronous (cancellable) action.
        /// </summary>
        /// <typeparam name="TAttributed">Type that contains [EventHandler] methods. This should match DeclaringType property.</typeparam>
        /// <typeparam name="TEvent">Type of event that is handled by the EventHandlerAttributeMethod. This should match EventType property.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which provides an instance of a class that contains methods marked with [EventHandler] attribute.</param>
        /// <returns>Delegate that handles an event.</returns>
        private Func<object, CancellationToken, Task> createCancellableAsyncDelegate<TAttributed, TEvent>(Func<object> attributedObjectFactory)
            where TAttributed : class
            where TEvent : class
        {
            // Create an expression that will invoke the event handler method of a given instance.
            var instanceParameterExpression = Expression.Parameter(typeof(TAttributed), "instance");
            var eventParameterExpression = Expression.Parameter(typeof(TEvent), "event");
            var callExpression = Expression.Call(instanceParameterExpression, MethodInfo, eventParameterExpression, CancellationTokenParameterExpression);

            // Lambda signature:
            // (instance, @event, cancallationToken) => instance.HandleEventAsync(@event, cancellationToken);
            var cancellableAsyncDelegate = Expression.Lambda<Func<TAttributed, TEvent, CancellationToken, Task>>(callExpression, new[] 
            {  
                instanceParameterExpression,
                eventParameterExpression,
                CancellationTokenParameterExpression
            }).Compile();

            Func<TEvent, CancellationToken, Task> genericDelegate = EventHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, cancellableAsyncDelegate);

            return (obj, cancellationToken) => 
            {
                if (obj is TEvent @event)
                {
                    return genericDelegate.Invoke(@event, cancellationToken);
                }

                throw new ArgumentException($"Invalid event. Expected event of type {typeof(TEvent).Name} but was given {obj.GetType().Name}.", nameof(obj));
            };
        }

        /// <summary>
        /// Create a delegate from an asynchronous (non-cancellable) action.
        /// </summary>
        /// <typeparam name="TAttributed">Type that contains [EventHandler] methods. This should match DeclaringType property.</typeparam>
        /// <typeparam name="TEvent">Type of event that is handled by the EventHandlerAttributeMethod. This should match EventType property.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which provides an instance of a class that contains methods marked with [EventHandler] attribute.</param>
        /// <returns>Delegate that handles an event.</returns>
        private Func<object, CancellationToken, Task> createNonCancellableAsyncDelegate<TAttributed, TEvent>(Func<object> attributedObjectFactory)
            where TAttributed : class
            where TEvent : class
        {
            // Create an expression that will invoke the event handler method of a given instance.
            var instanceParameterExpression = Expression.Parameter(typeof(TAttributed), "instance");
            var eventParameterExpression = Expression.Parameter(typeof(TEvent), "event");
            var callExpression = Expression.Call(instanceParameterExpression, MethodInfo, eventParameterExpression);

            // Lambda signature:
            // (instance, @event) => instance.HandleEventAsync(@event);
            var nonCancellableAsyncDelegate = Expression.Lambda<Func<TAttributed, TEvent, Task>>(callExpression, new[] 
            {  
                instanceParameterExpression,
                eventParameterExpression
            }).Compile();

            Func<TEvent, CancellationToken, Task> genericDelegate = EventHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, nonCancellableAsyncDelegate);

            return (obj, cancellationToken) => 
            {
                if (obj is TEvent @event)
                {
                    return genericDelegate.Invoke(@event, cancellationToken);
                }

                throw new ArgumentException($"Invalid event. Expected event of type {typeof(TEvent).Name} but was given {obj.GetType().Name}.", nameof(obj));
            };
        }

        /// <summary>
        /// Create a delegate from a synchronous action.
        /// </summary>
        /// <typeparam name="TAttributed">Type that contains [EventHandler] methods. This should match DeclaringType property.</typeparam>
        /// <typeparam name="TEvent">Type of event that is handled by the EventHandlerAttributeMethod. This should match EventType property.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which provides an instance of a class that contains methods marked with [EventHandler] attribute.</param>
        /// <returns>Delegate that handles an event.</returns>
        private Func<object, CancellationToken, Task> createWrappedSyncDelegate<TAttributed, TEvent>(Func<object> attributedObjectFactory)
            where TAttributed : class
            where TEvent : class
        {
            // Create an expression that will invoke the event handler method of a given instance.
            var instanceParameterExpression = Expression.Parameter(typeof(TAttributed), "instance");
            var eventParameterExpression = Expression.Parameter(typeof(TEvent), "event");
            var callExpression = Expression.Call(instanceParameterExpression, MethodInfo, eventParameterExpression);

            // Lambda signature:
            // (instance, @event) => instance.HandleEvent(@event);
            var action = Expression.Lambda<Action<TAttributed, TEvent>>(callExpression, new[] 
            {  
                instanceParameterExpression,
                eventParameterExpression
            }).Compile();
            
            Func<TEvent, CancellationToken, Task> genericDelegate = EventHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, action, yieldSynchronousExecution: YieldSynchronousExecution);

            return (obj, cancellationToken) => 
            {
                if (obj is TEvent @event)
                {
                    return genericDelegate.Invoke(@event, cancellationToken);
                }

                throw new ArgumentException($"Invalid event. Expected event of type {typeof(TEvent).Name} but was given {obj.GetType().Name}.", nameof(obj));
            };
        }

        #endregion Functions
    }
}
