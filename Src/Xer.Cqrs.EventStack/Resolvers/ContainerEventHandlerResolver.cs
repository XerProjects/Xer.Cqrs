using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Xer.Delegator;
using Xer.Delegator.Exceptions;

namespace Xer.Cqrs.EventStack.Resolvers
{
    public class ContainerEventHandlerResolver : IMessageHandlerResolver
    {
        #region Static Declarations

        private static readonly MethodInfo ResolveMessageHandlerOpenGenericMethod = typeof(ContainerEventHandlerResolver).GetRuntimeMethod(nameof(ResolveMessageHandler), new Type[] { });
        private static readonly Dictionary<Type, Func<ContainerEventHandlerResolver, MessageHandlerDelegate>> _genericResolveDelegatesByMessageType = new Dictionary<Type, Func<ContainerEventHandlerResolver, MessageHandlerDelegate>>();
        private static readonly object _padlock = new object();

        #endregion Static Declarations
        
        #region Declarations
        
        private readonly IContainerAdapter _containerAdapter;
        private readonly bool _yieldExecutionOfSyncHandlers;
        private readonly Action<Exception> _exceptionHandler;

        #endregion Declarations

        #region Constructors
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="containerAdapter">Container adapter.</param>
        public ContainerEventHandlerResolver(IContainerAdapter containerAdapter)
        {
            _containerAdapter = containerAdapter;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="containerAdapter">Container adapter.</param>
        /// <param name="yieldExecutionOfSyncHandlers">
        /// True if the execution of all resolved synchronous <see cref="Xer.Cqrs.EventStack.IEventHandler{TEvent}"/> instances should be yielded. Otherwise, false. 
        /// </param>
        public ContainerEventHandlerResolver(IContainerAdapter containerAdapter, bool yieldExecutionOfSyncHandlers)
        {
            _containerAdapter = containerAdapter;
            _yieldExecutionOfSyncHandlers = yieldExecutionOfSyncHandlers;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="containerAdapter">Container adapter.</param>
        /// <param name="resolveExceptionHandler">Delegate that will execute when an exception occurs while resolving handlers from the container.</param>
        public ContainerEventHandlerResolver(IContainerAdapter containerAdapter, Action<Exception> resolveExceptionHandler)
        {
            _containerAdapter = containerAdapter;
            _exceptionHandler = resolveExceptionHandler;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="containerAdapter">Container adapter.</param>
        /// <param name="yieldExecutionOfSyncHandlers">
        /// True if the execution of all resolved synchronous <see cref="Xer.Cqrs.EventStack.IEventHandler{TEvent}"/> instances should be yielded. Otherwise, false. 
        /// </param>
        /// <param name="resolveExceptionHandler">Delegate that will execute when an exception occurs while resolving handlers from the container.</param>
        public ContainerEventHandlerResolver(IContainerAdapter containerAdapter, bool yieldExecutionOfSyncHandlers, Action<Exception> resolveExceptionHandler)
        {
            _containerAdapter = containerAdapter;
            _yieldExecutionOfSyncHandlers = yieldExecutionOfSyncHandlers;
            _exceptionHandler = resolveExceptionHandler;
        }

        #endregion Constructors

        #region IMessageHandlerResolver Implementation
        
        /// <summary>
        /// Resolve instances of <see cref="Xer.Cqrs.EventStack.IEventAsyncHandler{TEvent}"/> and 
        /// <see cref="Xer.Cqrs.EventStack.IEventHandler{TEvent}"/> which handles the given event type
        /// from the container and convert to a message handler delegate which invokes all event handlers.
        /// </summary>
        /// <param name="eventType">Type of event which is handled by the event handlers to resolve.</param>
        /// <returns>Instance of <see cref="MessageHandlerDelegate"/> which executes event handler processing.</returns>
        public MessageHandlerDelegate ResolveMessageHandler(Type eventType)
        {
            if(!_genericResolveDelegatesByMessageType.TryGetValue(eventType, out Func<ContainerEventHandlerResolver, MessageHandlerDelegate> genericResolveDelegate))
            {
                if(!eventType.GetTypeInfo().IsClass)
                {
                    throw new ArgumentException("Event is not a reference type.", nameof(eventType));
                }

                lock(_padlock)
                {
                    // Check after locking thread if another thread has already added a delegate.
                    if(!_genericResolveDelegatesByMessageType.TryGetValue(eventType, out genericResolveDelegate))
                    {
                        // Build and cache delegate.
                        genericResolveDelegate = buildGenericResolveMessageHandlerDelegate(eventType);

                        // Cache.
                        _genericResolveDelegatesByMessageType.Add(eventType, genericResolveDelegate);
                    }
                }
            }

            return genericResolveDelegate.Invoke(this);
        }

        #endregion IMessageHandlerResolver Implementation

        #region Methods

        // Note: When renaming or changing signature of this ResolveMessageHandler method, 
        // the cached ResolveMessageHandlerOpenGenericMethod should be updated. 
        
        /// <summary>
        /// <para>Resolve instances of IEventAsyncHandler<TEvent> and IEventHandler<TEvent> which handles the given event type</para>
        /// <para>from the container and convert to a message handler delegate which invokes all event handlers.</para>
        /// </summary>
        /// <typeparam name="TEvent">Type of event which is handled by the event handlers to resolve.</typeparam>
        /// <returns>Instance of <see cref="Xer.Delegator.MessageHandlerDelegate"/> which executes event handler processing.</returns>
        public MessageHandlerDelegate ResolveMessageHandler<TEvent>() where TEvent : class
        {
            return buildEventHandlerDelegate<TEvent>();
        }

        #endregion Methods

        #region Functions

        /// <summary>
        /// Build message handler delegate that will invoke all async + sync message handlers that were resolved from the container.
        /// </summary>
        /// <returns>Message handler delegate that invokes all async + sync message handlers that were resolved from the container.</returns>
        private MessageHandlerDelegate buildEventHandlerDelegate<TEvent>() where TEvent : class
        {
            var handlerDelegates = new List<MessageHandlerDelegate>();

            try
            {
                // Get all async handlers for the event.
                IEnumerable<IEventAsyncHandler<TEvent>> resolvedHandlers = _containerAdapter.ResolveMultiple<IEventAsyncHandler<TEvent>>();
                if (resolvedHandlers != null)
                {
                    handlerDelegates.Add(EventHandlerDelegateBuilder.FromEventHandlers(resolvedHandlers));
                }
            }
            catch (Exception ex)
            {
                // Some containers may throw exception when no instance is resolved.
                // This is to let clients have a way to be notified of the exception.
                _exceptionHandler?.Invoke(ex);
            }

            try
            {
                // Get all sync handlers for the event.
                IEnumerable<IEventHandler<TEvent>> syncEventHandlers = _containerAdapter.ResolveMultiple<IEventHandler<TEvent>>();
                if (syncEventHandlers != null)
                {
                    handlerDelegates.Add(EventHandlerDelegateBuilder.FromEventHandlers(syncEventHandlers, yieldExecution: _yieldExecutionOfSyncHandlers));
                }
            }
            catch (Exception ex)
            {
                // Some containers may throw exception when no instance is resolved.
                // This is to let clients have a way to be notified of the exception.
                _exceptionHandler?.Invoke(ex);
            }

            // Instantiate a MessageHandlerDelegate.
            return (message, cancellationToken) =>
            {
                // Task list.
                Task[] handleTasks = new Task[handlerDelegates.Count];

                // Invoke each message handler delegates to start the tasks and add to task list.
                for (int i = 0; i < handlerDelegates.Count; i++)
                    handleTasks[i] = handlerDelegates[i].Invoke(message, cancellationToken);

                // Wait for all tasks to complete.
                return Task.WhenAll(handleTasks);
            };
        }

        /// <summary>
        /// Create a delegate that calls the generic resolveMessageHandler function. 
        /// </summary>
        /// <param name="eventType">Type of event.</param>
        /// <returns>Delegate that calls the generic resolveMessageHandler function.</returns>
        private static Func<ContainerEventHandlerResolver, MessageHandlerDelegate> buildGenericResolveMessageHandlerDelegate(Type eventType)
        {
            // Create a delegate which calls the ResolveMessageHandler<TCommand> method where TCommand is set to the commandType.
            // Signature: (resolver) => resolver.ResolveMessageHandler<TCommand>();
            ParameterExpression resolverParameter = Expression.Parameter(typeof(ContainerEventHandlerResolver), "resolver");
            Expression callGenericResolveMessageHandler = Expression.Call(resolverParameter, ResolveMessageHandlerOpenGenericMethod.MakeGenericMethod(eventType));
            return Expression.Lambda<Func<ContainerEventHandlerResolver, MessageHandlerDelegate>>(callGenericResolveMessageHandler, resolverParameter).Compile();
        }

        #endregion Functions
    }
}