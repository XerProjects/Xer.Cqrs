using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.EventStack.Registrations
{
    internal class EventHandlerAttributeMethod
    {
        #region Declarations

        private static readonly TypeInfo EventTypeInfo = typeof(IEvent).GetTypeInfo();

        #endregion Declarations

        #region Properties

        public Type EventType { get; }
        public MethodInfo MethodInfo { get; }
        public bool IsAsync { get; }
        public bool SupportsCancellation { get; }

        #endregion Properties

        #region Constructors

        private EventHandlerAttributeMethod(Type eventType, MethodInfo methodInfo, bool isAsync, bool supportsCancellation)
        {
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            IsAsync = isAsync;
            SupportsCancellation = supportsCancellation;
        }

        #endregion Constructors

        #region Methods

        public EventHandlerDelegate CreateDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory) where TAttributed : class
           where TEvent : class, IEvent
        {
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

        public static EventHandlerAttributeMethod Create(MethodInfo methodInfo)
        {
            ParameterInfo[] methodParameters = methodInfo.GetParameters();

            ParameterInfo eventParameter = methodParameters.FirstOrDefault(p => EventTypeInfo.IsAssignableFrom(p.ParameterType.GetTypeInfo()));

            if (eventParameter == null)
            {
                // Parameter is not a command. Skip.
                throw new InvalidOperationException($"Methods marked with [EventHandler] should accept an event as parameter: {methodInfo.Name}");
            }

            bool isAsync;

            // Only valid return types are Task/void.
            if (methodInfo.ReturnType == typeof(Task))
            {
                isAsync = true;
            }
            else if (methodInfo.ReturnType == typeof(void))
            {
                isAsync = false;

                if (methodInfo.CustomAttributes.Any(p => p.AttributeType == typeof(AsyncStateMachineAttribute)))
                {
                    throw new InvalidOperationException($"Methods marked with async void are not supported: {methodInfo.Name}.");
                }
            }
            else
            {
                // Return type is not Task/void. Invalid.
                throw new InvalidOperationException($"Method marked with [EventHandler] can only have void or a Task return values: {methodInfo.Name}");
            }

            bool supportsCancellation = methodParameters.Any(p => p.ParameterType == typeof(CancellationToken));

            if(!isAsync && supportsCancellation)
            {
                throw new InvalidOperationException("Cancellation token support is only available for async methods (Methods returning a Task).");
            }

            return new EventHandlerAttributeMethod(eventParameter.ParameterType, methodInfo, isAsync, supportsCancellation);
        }

        #endregion Methods

        #region Functions

        /// <summary>
        /// Create a delegate from a synchronous action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [EventHandler].</typeparam>
        /// <typeparam name="TEvent">Type of event that is handled by the event handler delegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of <typeparamref name="TAttributed"/>.</param>
        /// <returns>Instance of event handler delegate.</returns>
        private EventHandlerDelegate createWrappedSyncDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory) where TAttributed : class
                                   where TEvent : class, IEvent
        {
            Action<TAttributed, TEvent> action = (Action<TAttributed, TEvent>)MethodInfo.CreateDelegate(typeof(Action<TAttributed, TEvent>));
            
            return EventHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, action);
        }

        /// <summary>
        /// Create a delegate from an asynchronous (cancellable) action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [EventHandler].</typeparam>
        /// <typeparam name="TEvent">Type of event that is handled by the event handler delegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of <typeparamref name="TAttributed"/>.</param>
        /// <returns>Instance of event handler delegate.</returns>
        private EventHandlerDelegate createCancellableAsyncDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory) where TAttributed : class
                                   where TEvent : class, IEvent
        {
            Func<TAttributed, TEvent, CancellationToken, Task> asyncCancellableAction = (Func<TAttributed, TEvent, CancellationToken, Task>)MethodInfo.CreateDelegate(typeof(Func<TAttributed, TEvent, CancellationToken, Task>));

            return EventHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, asyncCancellableAction);
        }

        /// <summary>
        /// Create a delegate from an asynchronous (non-cancellable) action.
        /// </summary>
        /// <typeparam name="TAttributed">Type of object that contains methods marked with [EventHandler].</typeparam>
        /// <typeparam name="TEvent">Type of event that is handled by the event handler delegate.</typeparam>
        /// <param name="attributedObjectFactory">Factory delegate which produces an instance of <typeparamref name="TAttributed"/>.</param>
        /// <returns>Instance of event handler delegate.</returns>
        private EventHandlerDelegate createNonCancellableAsyncDelegate<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory) where TAttributed : class
                                   where TEvent : class, IEvent
        {
            Func<TAttributed, TEvent, Task> asyncAction = (Func<TAttributed, TEvent, Task>)MethodInfo.CreateDelegate(typeof(Func<TAttributed, TEvent, Task>));

            return EventHandlerDelegateBuilder.FromDelegate(attributedObjectFactory, asyncAction);
        }

        #endregion Functions
    }
}
