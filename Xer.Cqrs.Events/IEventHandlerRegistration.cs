using System;

namespace Xer.Cqrs.Events
{
    /// <summary>
    /// Manages event handler registrations.
    /// </summary>
    public interface IEventHandlerRegistration
    {
        /// <summary>
        /// Register event handler as subscriber.
        /// </summary>
        /// <typeparam name="TEvent">Event to subscribe to.</typeparam>
        /// <param name="eventHandlerFactory">Event handler instance factory.</param>
        void Register<TEvent>(Func<IEventHandler<TEvent>> eventHandlerFactory) where TEvent : IEvent;

        /// <summary>
        /// Register event handler as subscriber.
        /// </summary>
        /// <typeparam name="TEvent">Event to subscribe to.</typeparam>
        /// <param name="eventHandlerFactory">Event async handler instance factory.</param>
        void Register<TEvent>(Func<IEventAsyncHandler<TEvent>> eventHandlerFactory) where TEvent : IEvent;
    }
}
