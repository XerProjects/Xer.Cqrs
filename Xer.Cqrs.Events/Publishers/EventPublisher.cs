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
        private static readonly MethodInfo _resolveEventHandlerMethodInfo =
            typeof(IEventHandlerResolver).GetRuntimeMethods().First(m => m.Name == "ResolveEventHandler");

        private readonly IDictionary<Type, Func<EventHandlerDelegate>> _cachedEventHandlerDelegateResolver = new Dictionary<Type, Func<EventHandlerDelegate>>();

        private readonly IEventHandlerResolver _resolver;

        public EventPublisher(IEventHandlerResolver resolver)
        {
            _resolver = resolver;
        }

        public Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            EventHandlerDelegate eventHandlerDelegate = resolveHandlerDelegateFor(@event);

            return eventHandlerDelegate.Invoke(@event, cancellationToken);
        }

        private EventHandlerDelegate resolveHandlerDelegateFor(IEvent @event)
        {
            Type eventType = @event.GetType();

            Func<EventHandlerDelegate> eventHandlerDelegateResolver;
            if (!_cachedEventHandlerDelegateResolver.TryGetValue(eventType, out eventHandlerDelegateResolver))
            {
                // Make generic method declaration.
                // IEventHandlerResolver.ResolveEventHandler<EventType>();
                MethodInfo genericResolveEventHandlerMethodInfo = _resolveEventHandlerMethodInfo.MakeGenericMethod(eventType);

                // Create delegate from generic method declaration.
                eventHandlerDelegateResolver = (Func<EventHandlerDelegate>)genericResolveEventHandlerMethodInfo.CreateDelegate(typeof(Func<EventHandlerDelegate>), _resolver);

                // Cache delegate.
                _cachedEventHandlerDelegateResolver.Add(eventType, eventHandlerDelegateResolver);
            }

            return eventHandlerDelegateResolver.Invoke();
        }
    }
}
