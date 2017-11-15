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
        /// <param name="eventAsyncHandlerFactory">Factory which will provide an instance of an event handler that handles the specified <typeparamref name="TEvent"/> event.</param>
        void Register<TEvent>(Func<IEventHandler<TEvent>> eventHandlerFactory) where TEvent : class, IEvent;

        /// <summary>
        /// Register async event handler as subscriber.
        /// </summary>
        /// <typeparam name="TEvent">Event to subscribe to.</typeparam>
        /// <param name="eventAsyncHandlerFactory">Factory which will provide an instance of an event handler that handles the specified <typeparamref name="TEvent"/> event.</param>
        void Register<TEvent>(Func<IEventAsyncHandler<TEvent>> eventAsyncHandlerFactory) where TEvent : class, IEvent;
    }
}
