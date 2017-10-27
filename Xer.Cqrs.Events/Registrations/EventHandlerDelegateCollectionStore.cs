using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Xer.Cqrs.Events.Registrations
{
    internal class EventHandlerDelegateCollectionStore
    {
        private readonly IDictionary<Type, IList<EventHandlerDelegate>> _eventHandlerDelegatesByEventType = new Dictionary<Type, IList<EventHandlerDelegate>>();

        public bool TryGetEventHandlerDelegates(Type eventType, out IEnumerable<EventHandlerDelegate> eventHandlerDelegates)
        {
            IList<EventHandlerDelegate> localEventHandlerDelegates;
            if (_eventHandlerDelegatesByEventType.TryGetValue(eventType, out localEventHandlerDelegates))
            {
                eventHandlerDelegates = new ReadOnlyCollection<EventHandlerDelegate>(localEventHandlerDelegates);
                return true;
            }

            eventHandlerDelegates = Enumerable.Empty<EventHandlerDelegate>();
            return false;
        }

        public void Add(Type eventType, EventHandlerDelegate eventHandlerDelegate)
        {
            addEventHandlerDelegate(eventType, eventHandlerDelegate);
        }

        public void Add(Type eventType, IEnumerable<EventHandlerDelegate> eventHandlerDelegates)
        {
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
