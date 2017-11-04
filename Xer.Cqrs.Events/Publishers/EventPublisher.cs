using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.Events.Publishers
{
    public class EventPublisher : IEventPublisher
    {
        #region Declarations

        private static readonly MethodInfo _resolveEventHandlersOpenGenericMethodInfo = getOpenGenericMethodInfo<IEventHandlerResolver>(c => c.ResolveEventHandlers<IEvent>());

        private readonly IDictionary<Type, Func<IEnumerable<EventHandlerDelegate>>> _cachedEventHandlerDelegatesResolver = new Dictionary<Type, Func<IEnumerable<EventHandlerDelegate>>>();

        private readonly IEventHandlerResolver _resolver;

        #endregion Declarations

        #region Constructors

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="resolver">Event handler resolver.</param>
        public EventPublisher(IEventHandlerResolver resolver)
        {
            _resolver = resolver;
        }

        #endregion Constructors

        #region IEventPublisher Implementations

        /// <summary>
        /// Triggers when an exception occurs while handling events.
        /// </summary>
        public event OnErrorHandler OnError;

        /// <summary>
        /// Publish event to subscribers.
        /// </summary>
        /// <param name="event">Event to publish.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        public async Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            IEnumerable<EventHandlerDelegate> eventHandlerDelegates = resolveEventHandlerDelegatesFor(@event);

            IEnumerable<Task> handleTasks = eventHandlerDelegates.Select(eventHandler =>
            {
                return ExecuteEventHandlerAsync(eventHandler, @event, cancellationToken);
            });

            // Execute all tasks and return an enumerable which contains tasks ordered by their completion.
            foreach(Task completedTask in TaskUtility.OrderByCompletion(handleTasks))
            {
                try
                {
                    // Await to handle any exceptions that might have occured.
                    await completedTask;
                }
                catch (OperationCanceledException)
                {
                    // Propagate cancellation.
                    throw;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(@event, ex);
                }
            }
        }

        /// <summary>
        /// Publish events to subscribers.
        /// </summary>
        /// <param name="events">Events to publish.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        public Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            IEnumerable<Task> publishTasks = events.Select(e => PublishAsync(e, cancellationToken));
            
            // Execute all tasks and return an enumerable which contains tasks ordered by their completion.
            IEnumerable<Task> interleavingTasks = TaskUtility.OrderByCompletion(publishTasks);

            return Task.WhenAll(interleavingTasks);
        }

        #endregion IEventPublisher Implementations

        #region Methods

        /// <summary>
        /// Execute event handler.
        /// </summary>
        /// <param name="eventHandler">Event handler delegate.</param>
        /// <param name="event">Event to publish.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        protected virtual Task ExecuteEventHandlerAsync(EventHandlerDelegate eventHandler, IEvent @event, CancellationToken cancellationToken)
        {
            return eventHandler.Invoke(@event, cancellationToken);
        }

        #endregion Methods

        #region Functions

        /// <summary>
        /// Resolve event handler delegates for the given event.
        /// </summary>
        /// <param name="event">Event.</param>
        /// <returns>Collection of event handler delegates which are registered for the event.</returns>
        private IEnumerable<EventHandlerDelegate> resolveEventHandlerDelegatesFor(IEvent @event)
        {
            Type eventType = @event.GetType();

            Func<IEnumerable<EventHandlerDelegate>> eventHandlerDelegatesResolver;
            if (!_cachedEventHandlerDelegatesResolver.TryGetValue(eventType, out eventHandlerDelegatesResolver))
            {
                // Make closed generic method info.
                // IEventHandlerResolver.ResolveEventHandlers<SpecificEventType>();
                MethodInfo resolveEventHandlersClosedGenericMethodInfo = _resolveEventHandlersOpenGenericMethodInfo.MakeGenericMethod(eventType);

                // Create delegate from closed generic method info.
                eventHandlerDelegatesResolver = (Func<IEnumerable<EventHandlerDelegate>>)resolveEventHandlersClosedGenericMethodInfo.CreateDelegate(typeof(Func<IEnumerable<EventHandlerDelegate>>), _resolver);

                // Cache delegate.
                _cachedEventHandlerDelegatesResolver.Add(eventType, eventHandlerDelegatesResolver);
            }

            return eventHandlerDelegatesResolver.Invoke();
        }
                
        private static MethodInfo getOpenGenericMethodInfo<T>(Expression<Action<T>> expression)
        {
            var methodCallExpression = expression.Body as MethodCallExpression;
            if (methodCallExpression != null)
            {
                string methodName = methodCallExpression.Method.Name;
                MethodInfo methodInfo = typeof(T).GetRuntimeMethods().FirstOrDefault(m => m.Name == methodName);
                return methodInfo;
            }

            return null;
        }

        #endregion Functions
    }
}
