using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Xer.Cqrs.EventStack.Attributes;

namespace Xer.Cqrs.EventStack.Registrations
{
    public class EventHandlerAttributeRegistration : IEventHandlerAttributeRegistration, IEventHandlerResolver
    {
        #region Declarations

        private static readonly MethodInfo RegisterEventHandlerOpenGenericMethodInfo = typeof(EventHandlerAttributeRegistration).GetTypeInfo().DeclaredMethods.First(m => m.Name == nameof(registerEventHandlerMethod));

        private readonly IDictionary<Type, IList<EventHandlerDelegate>> _eventHandlerDelegatesByEventType = new Dictionary<Type, IList<EventHandlerDelegate>>();

        #endregion Declarations

        #region IEventHandlerAttributeRegistration Implementation

        /// <summary>
        /// Register methods marked with the [EventHandler] attribute as event handlers.
        /// <para>Supported signatures for methods marked with [EventHandler] are: (Methods can be named differently)</para>
        /// <para>void HandleEvent(TEvent event);</para>
        /// <para>Task HandleEventAsync(TEvent event);</para>
        /// <para>Task HandleEventAsync(TEvent event, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <typeparam name="TAttributed">Type of the object which contains the methods marked with the [EventHandler] attribute.</typeparam>
        /// <param name="attributedHandlerFactory">Factory which will provide an instance of the specified <typeparamref name="TAttributed"/> type.</param>
        public void Register<TAttributed>(Func<TAttributed> attributedHandlerFactory) where TAttributed : class
        {
            if(attributedHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(attributedHandlerFactory));
            }

            Type attributedObjectType = typeof(TAttributed);

            // Get all public methods marked with EventHandler attribute.
            IEnumerable<EventHandlerAttributeMethod> eventHandlerMethods = getEventHandlerMethods(attributedObjectType);

            foreach (EventHandlerAttributeMethod eventHandlerMethod in eventHandlerMethods)
            {
                MethodInfo registerEventHandlerGenericMethodInfo = RegisterEventHandlerOpenGenericMethodInfo.MakeGenericMethod(
                    attributedObjectType,
                    eventHandlerMethod.EventType);

                registerEventHandlerGenericMethodInfo.Invoke(this, new object[]
                {
                    attributedHandlerFactory, eventHandlerMethod
                });
            }
        }

        #endregion IEventHandlerAttributeRegistration Implementation

        #region IEventHandlerResolver Implementation

        /// <summary>
        /// Get registered event handler delegates which handle the event of the specified type.
        /// </summary>
        /// <typeparam name="TEvent">Type of event to be handled.</typeparam>
        /// <returns>Collection of <see cref="EventHandlerDelegate"/> which executes event handler processing.</returns>
        public IEnumerable<EventHandlerDelegate> ResolveEventHandlers<TEvent>() where TEvent : class, IEvent
        {
            IList<EventHandlerDelegate> eventHandlerDelegates;

            if(!_eventHandlerDelegatesByEventType.TryGetValue(typeof(TEvent), out eventHandlerDelegates))
            {
                return Enumerable.Empty<EventHandlerDelegate>();
            }

            return new ReadOnlyCollection<EventHandlerDelegate>(eventHandlerDelegates);
        }

        #endregion IEventHandlerResolver Implementation

        #region Functions

        private void registerEventHandlerMethod<TAttributed, TEvent>(Func<TAttributed> attributedObjectFactory, EventHandlerAttributeMethod eventHandlerMethod) 
            where TAttributed : class 
            where TEvent : class, IEvent
        {
            Type eventType = typeof(TEvent);

            EventHandlerDelegate newHandleCommandDelegate = eventHandlerMethod.CreateDelegate<TAttributed, TEvent>(attributedObjectFactory);

            IList<EventHandlerDelegate> eventHandlerDelegates;
            if (!_eventHandlerDelegatesByEventType.TryGetValue(eventType, out eventHandlerDelegates))
            {
                eventHandlerDelegates = new List<EventHandlerDelegate>()
                {
                    newHandleCommandDelegate
                };

                _eventHandlerDelegatesByEventType.Add(eventType, eventHandlerDelegates);
            }
            else
            {
                eventHandlerDelegates.Add(newHandleCommandDelegate);
            }
        }

        private static IEnumerable<EventHandlerAttributeMethod> getEventHandlerMethods(Type eventHandlerType)
        {
            IEnumerable<MethodInfo> methods = eventHandlerType.GetRuntimeMethods().Where(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(EventHandlerAttribute)));

            List<EventHandlerAttributeMethod> eventHandlerMethods = new List<EventHandlerAttributeMethod>(methods.Count());
            
            foreach (MethodInfo methodInfo in methods)
            {
                // Return methods marked with [EventHandler].
                eventHandlerMethods.Add(EventHandlerAttributeMethod.Create(methodInfo));
            }

            return eventHandlerMethods;
        }

        #endregion Functions
    }
}
