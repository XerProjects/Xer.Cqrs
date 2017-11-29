using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Xer.Cqrs.Events.Registrations
{
    internal class EventHandlerDelegateCollectionStore
    {
        private static readonly IEnumerable<EventHandlerDelegate> NullEventHandlerDelegates = Enumerable.Empty<EventHandlerDelegate>();
        
        private readonly IDictionary<Type, IList<EventHandlerDelegate>> _eventHandlerDelegatesByEventType = new Dictionary<Type, IList<EventHandlerDelegate>>();

        public IEnumerable<EventHandlerDelegate> GetEventHandlerDelegates(Type eventType)
        {
            IList<EventHandlerDelegate> storedEventHandlerDelegates;

            if (!_eventHandlerDelegatesByEventType.TryGetValue(eventType, out storedEventHandlerDelegates))
            {
                return NullEventHandlerDelegates;
            }

            return new ReadOnlyCollection<EventHandlerDelegate>(storedEventHandlerDelegates);
        }

        public void Add(Type eventType, EventHandlerDelegate eventHandlerDelegate)
        {
            if (eventHandlerDelegate == null)
            {
                throw new ArgumentNullException(nameof(eventHandlerDelegate));
            }

            addEventHandlerDelegate(eventType, eventHandlerDelegate);
        }

        public void Add(Type eventType, IEnumerable<EventHandlerDelegate> eventHandlerDelegates)
        {
            if (eventHandlerDelegates == null)
            {
                throw new ArgumentNullException(nameof(eventHandlerDelegates));
            }

            foreach (EventHandlerDelegate eventHandlerDelegate in eventHandlerDelegates)
            {
                addEventHandlerDelegate(eventType, eventHandlerDelegate);
            }
        }

        private void addEventHandlerDelegate(Type eventType, EventHandlerDelegate newSubscribedEventHandlerDelegate)
        {
            IList<EventHandlerDelegate> eventHandlerDelegates;
            if (_eventHandlerDelegatesByEventType.TryGetValue(eventType, out eventHandlerDelegates))
            {
                eventHandlerDelegates.Add(newSubscribedEventHandlerDelegate);
            }
            else
            {
                _eventHandlerDelegatesByEventType.Add(eventType, new List<EventHandlerDelegate> { newSubscribedEventHandlerDelegate });
            }
        }
    }
}
