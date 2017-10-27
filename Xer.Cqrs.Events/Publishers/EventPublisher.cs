using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.Events.Publishers
{
    public class EventPublisher : IEventPublisher
    {
        #region Declarations

        private static readonly MethodInfo _resolveEventHandlersOpenGenericMethodInfo = Utilities.GetOpenGenericMethodInfo<IEventHandlerResolver>(c => c.ResolveEventHandlers<IEvent>());

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
        public Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<EventHandlerDelegate> eventHandlerDelegates = resolveEventHandlerDelegatesFor(@event);

            IEnumerable<Task> handleTasks = eventHandlerDelegates.Select(eventHandler =>
            {
                return Task.Run(async () =>
                {
                    try
                    {
                        await eventHandler.Invoke(@event, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(eventHandler, ex);
                    }
                });
            });

            return Task.WhenAll(handleTasks);
        }

        #endregion IEventPublisher Implementations

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

        #endregion Functions
    }
}
