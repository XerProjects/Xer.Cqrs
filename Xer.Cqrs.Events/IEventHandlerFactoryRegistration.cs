using System;

namespace Xer.Cqrs.Events
{
    /// <summary>
    /// Manages event subscribers.
    /// </summary>
    public interface IEventHandlerFactoryRegistration
    {
        /// <summary>
        /// Register event handler as subscriber.
        /// </summary>
        /// <typeparam name="TEvent">Event to subscribe to.</typeparam>
        /// <param name="eventSubscriberFactory">Event handler instance factory.</param>
        void Register<TEvent>(Func<IEventHandler<TEvent>> eventSubscriberFactory) where TEvent : IEvent;

        /// <summary>
        /// Register event handler as subscriber.
        /// </summary>
        /// <typeparam name="TEvent">Event to subscribe to.</typeparam>
        /// <param name="eventSubscriberFactory">Event async handler instance factory.</param>
        void Register<TEvent>(Func<IEventAsyncHandler<TEvent>> eventSubscriberFactory) where TEvent : IEvent;
    }
}
